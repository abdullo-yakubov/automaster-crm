using AutoMaster.Application.DTOs;
using AutoMaster.Application.Interfaces;
using AutoMaster.Domain.Entities;
using AutoMaster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMaster.Infrastructure.Services;

/// <summary>
/// Разбор голосовых команд. Распознавание речи делает браузер (Web Speech API),
/// сервер определяет намерение и пишет журнал команд.
/// </summary>
public class VoiceService : IVoiceService
{
    private sealed record Intent(string Name, string[] Keywords, string Message, string? Route, string? Prefill = null);

    // Порядок важен: более специфичные команды раньше общих.
    private static readonly Intent[] Intents =
    {
        new("add_oil_change", new[] { "замена масла", "заменить масло", "масло" }, "Открываю новый заказ с услугой «Замена масла»", "/orders?new=1", "Замена масла и фильтров"),
        new("add_diagnostics", new[] { "диагностик" }, "Открываю новый заказ с услугой «Диагностика»", "/orders?new=1", "Компьютерная диагностика"),
        new("new_order", new[] { "новый заказ", "создать заказ", "заказ" }, "Открываю форму нового заказа", "/orders?new=1"),
        new("dashboard", new[] { "статистик", "дашборд", "главная" }, "Перехожу на дашборд", "/dashboard"),
        new("customers", new[] { "клиент" }, "Открываю список клиентов", "/customers"),
        new("warehouse", new[] { "склад", "запчаст" }, "Открываю склад", "/warehouse"),
        new("finance", new[] { "финанс", "касс", "деньги" }, "Открываю финансы", "/finance"),
        new("kanban", new[] { "канбан", "доска" }, "Открываю Kanban-доску", "/kanban"),
        new("schedule", new[] { "расписан", "запис" }, "Открываю расписание", "/schedule"),
        new("analytics", new[] { "аналитик", "отчёт", "отчет" }, "Открываю аналитику", "/analytics"),
    };

    private readonly AppDbContext _db;

    public VoiceService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<VoiceCommandResultDto> ProcessAsync(string transcript, Guid userId)
    {
        var text = transcript.Trim();
        var lower = text.ToLowerInvariant();
        var intent = Intents.FirstOrDefault(i => i.Keywords.Any(lower.Contains));

        var result = intent is null
            ? new VoiceCommandResultDto("unknown", false, $"Команда не распознана: «{text}»", null, null)
            : new VoiceCommandResultDto(intent.Name, true, intent.Message, intent.Route, intent.Prefill);

        _db.VoiceCommandLogs.Add(new VoiceCommandLog
        {
            UserId = userId,
            Transcript = text.Length > 500 ? text[..500] : text,
            Intent = result.Intent,
            Result = result.Message.Length > 500 ? result.Message[..500] : result.Message
        });
        await _db.SaveChangesAsync();
        return result;
    }

    public async Task<List<VoiceHistoryItemDto>> GetHistoryAsync(Guid userId, int take = 20)
    {
        take = Math.Clamp(take, 1, 100);
        return await _db.VoiceCommandLogs
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.CreatedAt)
            .Take(take)
            .Select(v => new VoiceHistoryItemDto(v.Id, v.Transcript, v.Intent, v.Result, v.CreatedAt))
            .ToListAsync();
    }
}
