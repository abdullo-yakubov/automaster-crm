using System.Security.Cryptography;
using AutoMaster.Application.DTOs;
using AutoMaster.Application.Interfaces;
using AutoMaster.Domain.Entities;
using AutoMaster.Domain.Enums;
using AutoMaster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMaster.Infrastructure.Services;

public class MarketingService : IMarketingService
{
    private const string CodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    private static readonly Dictionary<CustomerSegment, string> SegmentNames = new()
    {
        [CustomerSegment.All] = "Все клиенты",
        [CustomerSegment.Vip] = "VIP клиенты",
        [CustomerSegment.Dormant] = "Спящие (3+ мес)",
        [CustomerSegment.AfterService] = "После ТО (30 дней)",
    };

    private static readonly Dictionary<CustomerSegment, string> Templates = new()
    {
        [CustomerSegment.All] = "🔧 AutoMaster Душанбе — ваш надёжный автосервис! Специальные цены на ТО в этом месяце. Запишитесь сейчас!",
        [CustomerSegment.Vip] = "⭐ Дорогой VIP-клиент! Приглашаем вас на бесплатную диагностику в знак благодарности за верность. Ждём вас!",
        [CustomerSegment.Dormant] = "😊 Мы соскучились! Приходите к нам — скидка 15% на любой ремонт. Предложение действует неделю.",
        [CustomerSegment.AfterService] = "✅ Спасибо за визит! Через 5000 км — следующее ТО. Запишитесь заранее и получите скидку 10%.",
    };

    private readonly AppDbContext _db;

    public MarketingService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<MarketingStatsDto> GetStatsAsync()
    {
        var campaigns = await _db.MarketingCampaigns.CountAsync();
        var recipients = await _db.MarketingCampaigns.SumAsync(c => (int?)c.RecipientsCount) ?? 0;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var activePromos = await _db.PromoCodes.CountAsync(p => p.IsActive && p.ExpiresAt >= today);

        var sizes = new Dictionary<string, int>();
        foreach (var seg in Enum.GetValues<CustomerSegment>())
            sizes[seg.ToString()] = await SegmentQuery(seg).CountAsync();

        return new MarketingStatsDto(campaigns, recipients, activePromos, sizes);
    }

    public async Task<List<PromoCodeDto>> GetPromoCodesAsync()
    {
        var list = await _db.PromoCodes.OrderByDescending(p => p.CreatedAt).ToListAsync();
        return list.Select(ToDto).ToList();
    }

    public async Task<PromoCodeDto> CreatePromoCodeAsync(CreatePromoCodeRequest request)
    {
        var code = string.IsNullOrWhiteSpace(request.Code) ? await GenerateUniqueCodeAsync() : request.Code.Trim().ToUpperInvariant();
        if (await _db.PromoCodes.AnyAsync(p => p.Code == code))
            throw new InvalidOperationException($"Промокод {code} уже существует");

        var promo = new PromoCode
        {
            Code = code,
            DiscountPercent = request.DiscountPercent,
            ExpiresAt = request.ExpiresAt,
            MaxUses = request.MaxUses
        };
        _db.PromoCodes.Add(promo);
        await _db.SaveChangesAsync();
        return ToDto(promo);
    }

    public async Task<PromoCodeDto> SetPromoActiveAsync(Guid id, bool isActive)
    {
        var promo = await _db.PromoCodes.FindAsync(id) ?? throw new KeyNotFoundException("Промокод не найден");
        promo.IsActive = isActive;
        await _db.SaveChangesAsync();
        return ToDto(promo);
    }

    public async Task DeletePromoCodeAsync(Guid id)
    {
        var promo = await _db.PromoCodes.FindAsync(id) ?? throw new KeyNotFoundException("Промокод не найден");
        _db.PromoCodes.Remove(promo);
        await _db.SaveChangesAsync();
    }

    public async Task<PromoValidationDto> ValidatePromoAsync(string code)
    {
        var normalized = (code ?? string.Empty).Trim().ToUpperInvariant();
        var promo = await _db.PromoCodes.FirstOrDefaultAsync(p => p.Code == normalized);
        if (promo is null) return new PromoValidationDto(false, normalized, 0, "Промокод не найден");
        if (!promo.IsActive) return new PromoValidationDto(false, normalized, 0, "Промокод отключён");
        if (promo.ExpiresAt < DateOnly.FromDateTime(DateTime.UtcNow)) return new PromoValidationDto(false, normalized, 0, "Срок действия истёк");
        if (promo.MaxUses is not null && promo.UsedCount >= promo.MaxUses) return new PromoValidationDto(false, normalized, 0, "Лимит использований исчерпан");
        return new PromoValidationDto(true, normalized, promo.DiscountPercent, null);
    }

    public async Task<List<CampaignDto>> GetCampaignsAsync()
    {
        var list = await _db.MarketingCampaigns.OrderByDescending(c => c.CreatedAt).Take(100).ToListAsync();
        return list.Select(ToDto).ToList();
    }

    public async Task<CampaignDto> CreateCampaignAsync(CreateCampaignRequest request, Guid userId)
    {
        var channel = Enum.Parse<CampaignChannel>(request.Channel, ignoreCase: true);
        var segment = Enum.Parse<CustomerSegment>(request.Segment, ignoreCase: true);

        var query = SegmentQuery(segment);
        if (channel == CampaignChannel.Email) query = query.Where(c => c.Email != null && c.Email != "");
        var recipients = await query.CountAsync();
        if (recipients == 0)
            throw new InvalidOperationException("В выбранном сегменте нет получателей");

        var campaign = new MarketingCampaign
        {
            Name = $"{SegmentNames[segment]} · {channel}",
            Channel = channel,
            Segment = segment,
            Message = request.Message.Trim(),
            RecipientsCount = recipients,
            Status = CampaignStatus.Queued,
            CreatedByUserId = userId
        };
        _db.MarketingCampaigns.Add(campaign);
        await _db.SaveChangesAsync();
        return ToDto(campaign);
    }

    public CampaignTemplateDto GetTemplate(string segment)
    {
        var seg = Enum.TryParse<CustomerSegment>(segment, true, out var s) ? s : CustomerSegment.All;
        return new CampaignTemplateDto(seg.ToString(), Templates[seg]);
    }

    private IQueryable<Customer> SegmentQuery(CustomerSegment segment)
    {
        var now = DateTime.UtcNow;
        var dormantSince = now.AddDays(-90);
        var afterServiceSince = now.AddDays(-30);
        var q = _db.Customers.AsQueryable();
        return segment switch
        {
            CustomerSegment.Vip => q.Where(c => c.Status == CustomerStatus.Vip),
            CustomerSegment.Dormant => q.Where(c => c.Orders.Any() && !c.Orders.Any(o => o.CreatedAt >= dormantSince)),
            CustomerSegment.AfterService => q.Where(c => c.Orders.Any(o => o.Status == OrderStatus.Completed && o.CompletedAt >= afterServiceSince)),
            _ => q
        };
    }

    private async Task<string> GenerateUniqueCodeAsync()
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var code = "AUTO" + RandomNumberGenerator.GetString(CodeAlphabet, 5);
            if (!await _db.PromoCodes.AnyAsync(p => p.Code == code)) return code;
        }
        throw new InvalidOperationException("Не удалось сгенерировать уникальный код");
    }

    private static PromoCodeDto ToDto(PromoCode p) =>
        new(p.Id, p.Code, p.DiscountPercent, p.ExpiresAt, p.UsedCount, p.MaxUses, p.IsActive,
            p.ExpiresAt < DateOnly.FromDateTime(DateTime.UtcNow));

    private static CampaignDto ToDto(MarketingCampaign c) =>
        new(c.Id, c.Name, c.Channel.ToString(), c.Segment.ToString(), c.Message, c.RecipientsCount, c.Status.ToString(), c.CreatedAt);
}
