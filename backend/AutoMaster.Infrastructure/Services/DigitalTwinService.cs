using AutoMaster.Application.DTOs;
using AutoMaster.Application.Interfaces;
using AutoMaster.Domain.Entities;
using AutoMaster.Domain.Enums;
using AutoMaster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMaster.Infrastructure.Services;

public class DigitalTwinService : IDigitalTwinService
{
    private const int WorkdayHours = 10;
    private static readonly List<OrderStatus> FinishedStatuses = new() { OrderStatus.Ready, OrderStatus.Completed };
    private static readonly List<OrderStatus> QueueStatuses = new() { OrderStatus.Approved, OrderStatus.InProgress, OrderStatus.Waiting };

    private readonly AppDbContext _db;

    public DigitalTwinService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<DigitalTwinDto> GetStateAsync()
    {
        await ReleaseFinishedAsync();

        var boxes = await _db.ServiceBoxes
            .Include(b => b.CurrentOrder).ThenInclude(o => o!.Customer)
            .Include(b => b.CurrentOrder).ThenInclude(o => o!.Vehicle)
            .Include(b => b.CurrentOrder).ThenInclude(o => o!.Mechanic)
            .Where(b => b.IsActive)
            .OrderBy(b => b.SortOrder).ThenBy(b => b.Name)
            .ToListAsync();

        var avgHours = await AverageRepairHoursAsync();
        var now = DateTime.UtcNow;
        var boxDtos = boxes.Select(b => ToDto(b, avgHours, now)).ToList();

        var inBoxes = boxes.Where(b => b.CurrentOrderId != null).Select(b => b.CurrentOrderId!.Value).ToHashSet();
        var queue = await _db.Orders
            .Include(o => o.Customer).Include(o => o.Vehicle)
            .Where(o => QueueStatuses.Contains(o.Status) && !inBoxes.Contains(o.Id))
            .OrderByDescending(o => o.Priority).ThenBy(o => o.CreatedAt)
            .Take(50)
            .ToListAsync();
        var queueDtos = queue.Select(o => new QueuedOrderDto(o.Id, o.OrderNumber, o.Customer?.FullName ?? "—",
            $"{o.Vehicle?.Make} {o.Vehicle?.Model}".Trim(), o.ServiceDescription, o.Status.ToString(), o.Priority.ToString())).ToList();

        var total = boxes.Count;
        var busy = boxDtos.Count(b => b.OrderId != null);
        var load = total == 0 ? 0 : (int)Math.Round(busy * 100.0 / total);
        var throughput = avgHours > 0 ? (int)Math.Floor(total * WorkdayHours / avgHours) : total * 3;
        var hourly = await HourlyLoadAsync();

        var insights = new List<string>();
        if (total == 0) insights.Add("Боксы не настроены — добавьте их, чтобы видеть загрузку.");
        else if (load >= 80) insights.Add($"Загрузка {load}% — близко к пределу, новые заказы попадут в очередь.");
        else insights.Add($"Загрузка {load}% — есть свободные мощности ({total - busy} бокс(а)).");
        if (queueDtos.Count > 0 && busy < total)
            insights.Add($"В очереди {queueDtos.Count} заказ(ов) при свободных боксах — назначьте их на боксы.");
        var peak = hourly.OrderByDescending(h => h.Forecast).FirstOrDefault();
        if (peak is not null && peak.Forecast > 0)
            insights.Add($"Ожидаемый пик приёма сегодня — около {peak.Hour}:00 (по статистике за 4 недели).");
        if (avgHours > 0) insights.Add($"Среднее время ремонта — {avgHours:F1} ч, пропускная способность ≈ {throughput} авто/день.");

        return new DigitalTwinDto(boxDtos, queueDtos, total, busy, load, Math.Round(avgHours, 1), throughput, hourly, insights);
    }

    public async Task<ServiceBoxDto> CreateBoxAsync(CreateServiceBoxRequest request)
    {
        var maxOrder = await _db.ServiceBoxes.MaxAsync(b => (int?)b.SortOrder) ?? 0;
        var box = new ServiceBox { Name = request.Name.Trim(), Specialization = request.Specialization.Trim(), SortOrder = maxOrder + 1 };
        _db.ServiceBoxes.Add(box);
        await _db.SaveChangesAsync();
        return ToDto(box, 0, DateTime.UtcNow);
    }

    public async Task AssignAsync(Guid boxId, Guid orderId)
    {
        var box = await _db.ServiceBoxes.FindAsync(boxId) ?? throw new KeyNotFoundException("Бокс не найден");
        if (!box.IsActive) throw new InvalidOperationException("Бокс отключён");
        if (box.CurrentOrderId is not null) throw new InvalidOperationException("Бокс уже занят");

        var order = await _db.Orders.FindAsync(orderId) ?? throw new KeyNotFoundException("Заказ не найден");
        if (FinishedStatuses.Contains(order.Status)) throw new InvalidOperationException("Заказ уже завершён");
        if (await _db.ServiceBoxes.AnyAsync(b => b.CurrentOrderId == orderId))
            throw new InvalidOperationException("Заказ уже находится в другом боксе");

        box.CurrentOrderId = orderId;
        box.OccupiedSince = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task ReleaseAsync(Guid boxId)
    {
        var box = await _db.ServiceBoxes.FindAsync(boxId) ?? throw new KeyNotFoundException("Бокс не найден");
        box.CurrentOrderId = null;
        box.OccupiedSince = null;
        await _db.SaveChangesAsync();
    }

    private async Task ReleaseFinishedAsync()
    {
        var stale = await _db.ServiceBoxes
            .Where(b => b.CurrentOrder != null && FinishedStatuses.Contains(b.CurrentOrder.Status))
            .ToListAsync();
        if (stale.Count == 0) return;
        foreach (var b in stale) { b.CurrentOrderId = null; b.OccupiedSince = null; }
        await _db.SaveChangesAsync();
    }

    private async Task<double> AverageRepairHoursAsync()
    {
        var since = DateTime.UtcNow.AddDays(-90);
        var durations = await _db.Orders
            .Where(o => o.Status == OrderStatus.Completed && o.CompletedAt != null && o.CompletedAt >= since && o.CompletedAt > o.CreatedAt)
            .Select(o => new { o.CreatedAt, o.CompletedAt })
            .ToListAsync();
        // Отбрасываем выбросы (заказы, висевшие больше 3 суток, искажают среднее)
        var hours = durations.Select(d => (d.CompletedAt!.Value - d.CreatedAt).TotalHours).Where(h => h is > 0 and <= 72).ToList();
        return hours.Count == 0 ? 0 : hours.Average();
    }

    private async Task<List<HourlyLoadDto>> HourlyLoadAsync()
    {
        var nowLocal = TimeZoneHelper.ToLocal(DateTime.UtcNow);
        var todayLocal = nowLocal.Date;
        var sinceUtc = TimeZoneHelper.ToUtc(todayLocal.AddDays(-28));
        var created = await _db.Orders.Where(o => o.CreatedAt >= sinceUtc).Select(o => o.CreatedAt).ToListAsync();
        var locals = created.Select(TimeZoneHelper.ToLocal).ToList();

        var result = new List<HourlyLoadDto>();
        for (var h = 8; h <= 18; h++)
        {
            var actual = locals.Count(l => l.Date == todayLocal && l.Hour == h);
            var history = locals.Count(l => l.Date < todayLocal && l.DayOfWeek == todayLocal.DayOfWeek && l.Hour == h);
            result.Add(new HourlyLoadDto(h, actual, (int)Math.Round(history / 4.0)));
        }
        return result;
    }

    private static ServiceBoxDto ToDto(ServiceBox b, double avgHours, DateTime now)
    {
        var o = b.CurrentOrder;
        var progress = 0;
        if (o is not null && b.OccupiedSince is not null)
        {
            var expected = avgHours > 0 ? avgHours : 2.5;
            progress = (int)Math.Clamp((now - b.OccupiedSince.Value).TotalHours / expected * 100, 5, 95);
        }
        return new ServiceBoxDto(b.Id, b.Name, b.Specialization, b.IsActive, o?.Id, o?.OrderNumber, o?.Customer?.FullName,
            o is null ? null : $"{o.Vehicle?.Make} {o.Vehicle?.Model} · {o.Vehicle?.LicensePlate}",
            o?.ServiceDescription, o?.Mechanic?.FullName, b.OccupiedSince, progress);
    }
}
