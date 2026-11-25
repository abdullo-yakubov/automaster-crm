using AutoMaster.Application.DTOs;
using AutoMaster.Application.Interfaces;
using AutoMaster.Domain.Entities;
using AutoMaster.Domain.Enums;
using AutoMaster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMaster.Infrastructure.Services;

public class OperationsService : IOperationsService
{
    private readonly AppDbContext _db;
    private static readonly Random Rng = new();

    private static readonly Dictionary<OrderStatus, int> SlaHours = new()
    {
        [OrderStatus.Intake] = 2,
        [OrderStatus.Diagnosis] = 4,
        [OrderStatus.Estimate] = 8,
        [OrderStatus.Approved] = 6,
        [OrderStatus.InProgress] = 24,
        [OrderStatus.Waiting] = 48,
        [OrderStatus.Ready] = 12
    };

    public OperationsService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ChecklistItemDto>> GetChecklistAsync(string type)
    {
        var items = await _db.ChecklistItems.Where(x => x.Type == type).OrderBy(x => x.SortOrder).ToListAsync();
        if (items.Count == 0 && (type == "open" || type == "close"))
        {
            items = SeedDefaultChecklist(type);
            _db.ChecklistItems.AddRange(items);
            await _db.SaveChangesAsync();
        }
        return items.Select(ToDto).ToList();
    }

    public async Task<ChecklistItemDto> ToggleChecklistItemAsync(string type, Guid itemId)
    {
        var item = await _db.ChecklistItems.FirstOrDefaultAsync(x => x.Id == itemId && x.Type == type)
            ?? throw new KeyNotFoundException("Пункт чек-листа не найден");
        item.IsDone = !item.IsDone;
        await _db.SaveChangesAsync();
        return ToDto(item);
    }

    public async Task<ChecklistItemDto> AddChecklistItemAsync(AddChecklistItemRequest request)
    {
        var maxOrder = await _db.ChecklistItems.Where(x => x.Type == request.Type)
            .Select(x => x.SortOrder).DefaultIfEmpty(0).MaxAsync();
        var item = new ChecklistItem { Type = request.Type, Title = request.Title, SortOrder = maxOrder + 1 };
        _db.ChecklistItems.Add(item);
        await _db.SaveChangesAsync();
        return ToDto(item);
    }

    public Task<List<SensorReadingDto>> GetSensorsAsync()
    {
        var now = DateTime.UtcNow;
        var sensors = new List<SensorReadingDto>
        {
            new("Температура бокса", "temperature", Math.Round(18 + Rng.NextDouble() * 8, 1), "°C", now),
            new("Давление в магистрали", "pressure", Math.Round(4 + Rng.NextDouble() * 2, 2), "бар", now),
            new("Влажность", "humidity", Math.Round(35 + Rng.NextDouble() * 25, 1), "%", now),
            new("Уровень масла (бак)", "oil_level", Math.Round(40 + Rng.NextDouble() * 55, 1), "%", now)
        };
        return Task.FromResult(sensors);
    }

    public async Task<List<SlaViolationDto>> GetSlaViolationsAsync()
    {
        var orders = await _db.Orders.Include(o => o.Customer)
            .Where(o => o.Status != OrderStatus.Completed)
            .ToListAsync();

        var violations = new List<SlaViolationDto>();
        foreach (var o in orders)
        {
            if (!SlaHours.TryGetValue(o.Status, out var sla)) continue;
            var elapsed = (DateTime.UtcNow - o.UpdatedAt).TotalHours;
            if (elapsed > sla)
            {
                violations.Add(new SlaViolationDto(o.Id, o.OrderNumber, o.Customer?.FullName ?? "—",
                    o.Status.ToString(), sla, Math.Round(elapsed, 1), Math.Round(elapsed - sla, 1)));
            }
        }
        return violations.OrderByDescending(v => v.OverHours).ToList();
    }

    private static List<ChecklistItem> SeedDefaultChecklist(string type)
    {
        var titles = type == "open"
            ? new[] { "Проверить оборудование", "Включить освещение", "Подготовить рабочие места", "Проверить запасы", "Открыть кассу", "Провести планёрку" }
            : new[] { "Закрыть кассу", "Сдать смену", "Выключить оборудование", "Проверить боксы", "Закрыть ворота", "Сдать отчёт" };
        return titles.Select((t, i) => new ChecklistItem { Type = type, Title = t, SortOrder = i }).ToList();
    }

    private static ChecklistItemDto ToDto(ChecklistItem i) => new(i.Id, i.Type, i.Title, i.IsDone, i.SortOrder);
}

public class AuditService : IAuditService
{
    private readonly AppDbContext _db;

    public AuditService(AppDbContext db)
    {
        _db = db;
    }

    public async Task LogAsync(Guid userId, string action, string objectType, string objectId, string? details, Guid? branchId = null)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = action,
            ObjectType = objectType,
            ObjectId = objectId,
            Details = details,
            BranchId = branchId
        });
        await _db.SaveChangesAsync();
    }

    public async Task<List<AuditLogDto>> GetLogsAsync(Guid? userId, string? objectType, DateTime? from, DateTime? to)
    {
        var q = _db.AuditLogs.Include(a => a.User).AsQueryable();
        if (userId.HasValue) q = q.Where(a => a.UserId == userId);
        if (!string.IsNullOrWhiteSpace(objectType)) q = q.Where(a => a.ObjectType == objectType);
        if (from.HasValue) q = q.Where(a => a.CreatedAt >= from.Value);
        if (to.HasValue) q = q.Where(a => a.CreatedAt <= to.Value);

        var logs = await q.OrderByDescending(a => a.CreatedAt).Take(500).ToListAsync();
        return logs.Select(a => new AuditLogDto(a.Id, a.CreatedAt, a.User?.FullName ?? "—", a.Action, a.ObjectType, a.ObjectId, a.Details)).ToList();
    }

    public async Task<MysteryAuditDto> SaveMysteryAuditAsync(Guid userId, SaveMysteryAuditRequest request)
    {
        var s = request.Scores;
        var total = Math.Round((s.Greeting + s.Speed + s.Cleanliness + s.Politeness + s.RepairQuality + s.BillAccuracy + s.HandoverTime) / 7.0, 2);
        var audit = new MysteryAudit
        {
            ScoresJson = System.Text.Json.JsonSerializer.Serialize(s),
            TotalScore = (decimal)total,
            UserId = userId
        };
        _db.MysteryAudits.Add(audit);
        await _db.SaveChangesAsync();
        return new MysteryAuditDto(audit.Id, s, audit.TotalScore, audit.CreatedAt);
    }

    public async Task<List<MysteryAuditDto>> GetMysteryAuditsAsync()
    {
        var audits = await _db.MysteryAudits.OrderByDescending(a => a.CreatedAt).Take(100).ToListAsync();
        return audits.Select(a => new MysteryAuditDto(a.Id,
            System.Text.Json.JsonSerializer.Deserialize<MysteryAuditScoreDto>(a.ScoresJson)!,
            a.TotalScore, a.CreatedAt)).ToList();
    }
}
