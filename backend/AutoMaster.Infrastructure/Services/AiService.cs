using AutoMaster.Application.DTOs;
using AutoMaster.Application.Interfaces;
using AutoMaster.Domain.Enums;
using AutoMaster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMaster.Infrastructure.Services;

/// <summary>
/// Экспертная система диагностики на правилах (симптом → гипотезы).
/// Работает локально, данные клиентов не покидают сервер.
/// </summary>
public class AiService : IAiService
{
    private sealed record Rule(string[] Keywords, string Issue, string Recommendation, int BaseProbability,
        decimal CostFrom, decimal CostTo, string Urgency, int MileageBoostFrom = 0);

    private static readonly Rule[] Rules =
    {
        new(new[] { "тормоз", "скрип", "свист при торм", "педаль" }, "Износ тормозных колодок / дисков",
            "Замена передних колодок, проверка дисков", 80, 160, 450, "high", 40000),
        new(new[] { "стук", "подвеск", "кочк", "амортиз", "яма" }, "Неисправность амортизаторов / стоек",
            "Диагностика подвески, замена амортизаторов или втулок", 65, 320, 900, "medium", 60000),
        new(new[] { "вибрац", "руль", "бьёт", "бьет", "трясёт", "трясет" }, "Дисбаланс колёс / износ ШРУС",
            "Балансировка, проверка ШРУС и рулевых наконечников", 60, 60, 600, "medium"),
        new(new[] { "не завод", "аккумулятор", "стартер", "щелкает", "щёлкает" }, "Разряд АКБ / неисправность стартера",
            "Проверка АКБ и зарядки, диагностика стартера", 75, 50, 700, "high"),
        new(new[] { "перегрев", "температур", "антифриз", "тосол", "кипит" }, "Неисправность системы охлаждения",
            "Проверка термостата, помпы, радиатора; замена антифриза", 70, 150, 900, "high"),
        new(new[] { "масло", "дым", "сизый", "угар", "течь" }, "Утечка или угар масла",
            "Проверка сальников и прокладок, компрессии; замена масла", 60, 80, 1500, "medium", 100000),
        new(new[] { "чек", "check", "ошибк", "лампа", "троит", "провал" }, "Сбой системы зажигания / датчиков",
            "Компьютерная диагностика, проверка свечей и катушек", 70, 80, 500, "medium"),
        new(new[] { "короб", "передач", "пинок", "рывок", "сцеплен" }, "Неисправность КПП / сцепления",
            "Диагностика КПП, замена масла в коробке или комплекта сцепления", 55, 300, 3000, "medium", 120000),
        new(new[] { "кондиц", "печк", "не дует", "холод" }, "Неисправность климатической системы",
            "Заправка кондиционера, проверка вентилятора и салонного фильтра", 60, 100, 600, "low"),
        new(new[] { "фар", "свет", "электр", "предохран", "генератор" }, "Неисправность электрооборудования",
            "Проверка генератора, проводки и предохранителей", 55, 60, 800, "medium"),
        new(new[] { "ремень", "грм", "свистит под капот" }, "Износ ремня ГРМ / навесного оборудования",
            "Замена ремня и роликов", 65, 250, 900, "high", 90000),
        new(new[] { "расход", "жрёт", "жрет", "тяга", "мощност" }, "Загрязнение фильтров / форсунок",
            "Замена воздушного и топливного фильтров, чистка форсунок", 55, 40, 400, "low"),
    };

    private readonly AppDbContext _db;

    public AiService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<DiagnosisResultDto> DiagnoseAsync(DiagnosisRequest request)
    {
        var text = request.Problem.ToLowerInvariant();
        string? vehicleTitle = null;
        var mileage = request.Mileage;
        var recent = new List<string>();

        if (!string.IsNullOrWhiteSpace(request.PlateOrVin))
        {
            var key = request.PlateOrVin.Trim().ToUpperInvariant();
            var vehicle = await _db.Vehicles
                .Include(v => v.Orders)
                .FirstOrDefaultAsync(v => v.LicensePlate.ToUpper() == key || (v.VIN != null && v.VIN.ToUpper() == key));
            if (vehicle is not null)
            {
                vehicleTitle = $"{vehicle.Make} {vehicle.Model} {vehicle.Year} · {vehicle.LicensePlate}";
                mileage ??= vehicle.Mileage;
                recent = vehicle.Orders
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(5)
                    .Select(o => $"{TimeZoneHelper.ToLocal(o.CreatedAt):dd.MM.yyyy} — {o.ServiceDescription}")
                    .ToList();
            }
        }

        var recentText = string.Join(' ', recent).ToLowerInvariant();
        var hypotheses = Rules
            .Select(r => new { Rule = r, Hits = r.Keywords.Count(k => text.Contains(k)) })
            .Where(x => x.Hits > 0)
            .Select(x =>
            {
                var p = x.Rule.BaseProbability + (x.Hits - 1) * 8;
                if (x.Rule.MileageBoostFrom > 0 && mileage >= x.Rule.MileageBoostFrom) p += 10;
                // Если эта работа делалась недавно — вероятность ниже.
                if (x.Rule.Keywords.Any(k => recentText.Contains(k))) p -= 20;
                return new DiagnosisHypothesisDto(x.Rule.Issue, x.Rule.Recommendation, Math.Clamp(p, 10, 95),
                    x.Rule.CostFrom, x.Rule.CostTo, x.Rule.Urgency);
            })
            .OrderByDescending(h => h.Probability)
            .Take(3)
            .ToList();

        if (hypotheses.Count == 0)
        {
            hypotheses.Add(new DiagnosisHypothesisDto("Симптом не распознан",
                "Рекомендуется комплексная компьютерная диагностика", 50, 80, 150, "medium"));
        }

        return new DiagnosisResultDto(vehicleTitle, mileage, request.Problem, hypotheses, recent,
            "Предварительная оценка экспертной системы. Окончательный диагноз ставит мастер после осмотра.");
    }

    public async Task<List<PredictiveItemDto>> GetPredictiveMaintenanceAsync()
    {
        var vehicles = await _db.Vehicles
            .Include(v => v.Customer)
            .Select(v => new
            {
                v.Id, v.Make, v.Model, v.Year, v.Mileage, v.LicensePlate,
                Owner = v.Customer!.FullName, Phone = v.Customer.Phone,
                LastService = v.Orders.Where(o => o.Status == OrderStatus.Completed).Max(o => (DateTime?)o.CompletedAt),
                Services = v.Orders.Select(o => o.ServiceDescription).ToList()
            })
            .ToListAsync();

        var now = DateTime.UtcNow;
        var result = new List<PredictiveItemDto>();
        foreach (var v in vehicles)
        {
            var recs = new List<string>();
            var score = 0;
            var monthsSince = v.LastService is null ? (double?)null : (now - v.LastService.Value).TotalDays / 30.0;
            var history = string.Join(' ', v.Services).ToLowerInvariant();

            if (monthsSince is null) { recs.Add("Нет истории обслуживания — пригласить на диагностику"); score += 2; }
            else if (monthsSince > 6) { recs.Add($"Плановое ТО просрочено ({monthsSince:F0} мес. с последнего визита)"); score += 2; }

            if (v.Mileage > 50000 && !history.Contains("тормоз")) { recs.Add("Проверка тормозной системы"); score += 2; }
            if (v.Mileage > 90000 && !history.Contains("грм")) { recs.Add("Замена ремня ГРМ (пробег > 90 000 км)"); score += 3; }
            if (v.Mileage > 60000 && !history.Contains("подвеск")) { recs.Add("Диагностика подвески"); score += 1; }
            if (now.Year - v.Year >= 8) { recs.Add("Возраст авто 8+ лет — проверка АКБ и проводки"); score += 1; }

            if (recs.Count == 0) continue;

            var risk = score >= 5 ? "high" : score >= 3 ? "medium" : "low";
            result.Add(new PredictiveItemDto(v.Id, $"{v.Make} {v.Model}", v.LicensePlate, v.Owner, v.Phone, v.Year,
                v.Mileage, v.LastService, risk, recs));
        }

        return result
            .OrderBy(r => r.RiskLevel switch { "high" => 0, "medium" => 1, _ => 2 })
            .ThenByDescending(r => r.Mileage)
            .ToList();
    }

    public async Task<List<MakeIssueStatDto>> GetIssuesByMakeAsync()
    {
        var rows = await _db.Orders
            .Select(o => new { o.Vehicle!.Make, o.ServiceDescription })
            .ToListAsync();

        return rows
            .GroupBy(r => r.Make)
            .Select(g => new MakeIssueStatDto(
                g.Key,
                g.Count(),
                g.GroupBy(x => x.ServiceDescription).OrderByDescending(x => x.Count()).Take(3).Select(x => x.Key).ToList()))
            .OrderByDescending(x => x.Orders)
            .ToList();
    }
}
