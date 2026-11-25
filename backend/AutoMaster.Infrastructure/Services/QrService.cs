using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AutoMaster.Application.DTOs;
using AutoMaster.Application.Interfaces;
using AutoMaster.Domain.Enums;
using AutoMaster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AutoMaster.Infrastructure.Services;

/// <summary>
/// Формат: AM1.&lt;base64url(json)&gt;.&lt;base64url(HMAC-SHA256)&gt;.
/// Подпись не даёт подделать гарантийный талон, сумму оплаты или скидку промокода.
/// </summary>
public class QrService : IQrService
{
    private const string Prefix = "AM1";
    private const int WarrantyDays = 180;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly AppDbContext _db;
    private readonly byte[] _key;

    private sealed record Envelope(string T, Dictionary<string, string> D, long Exp);

    public QrService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        var explicitKey = config["Qr:SigningKey"];
        _key = !string.IsNullOrWhiteSpace(explicitKey)
            ? Encoding.UTF8.GetBytes(explicitKey)
            // Отдельный ключ, производный от JWT-ключа: компрометация QR не раскрывает JWT-ключ.
            : HMACSHA256.HashData(Encoding.UTF8.GetBytes(config["Jwt:Key"]!), "automaster-qr-v1"u8.ToArray());
    }

    public async Task<SignedQrDto> CreateWarrantyAsync(Guid orderId)
    {
        var o = await _db.Orders.AsNoTracking().Include(x => x.Customer).Include(x => x.Vehicle)
            .FirstOrDefaultAsync(x => x.Id == orderId) ?? throw new KeyNotFoundException("Заказ не найден");
        if (o.Status != OrderStatus.Completed || o.CompletedAt is null)
            throw new InvalidOperationException("Гарантийный талон выдаётся только по завершённому заказу");

        var expires = o.CompletedAt.Value.AddDays(WarrantyDays);
        var data = new Dictionary<string, string>
        {
            ["order"] = o.OrderNumber,
            ["client"] = o.Customer?.FullName ?? "",
            ["vehicle"] = $"{o.Vehicle?.Make} {o.Vehicle?.Model} {o.Vehicle?.LicensePlate}".Trim(),
            ["service"] = o.ServiceDescription,
            ["completed"] = TimeZoneHelper.ToLocal(o.CompletedAt.Value).ToString("dd.MM.yyyy"),
            ["until"] = TimeZoneHelper.ToLocal(expires).ToString("dd.MM.yyyy"),
        };
        return Sign("warranty", data, expires, $"Гарантия {o.OrderNumber} до {data["until"]}");
    }

    public async Task<SignedQrDto> CreatePaymentAsync(Guid orderId)
    {
        var o = await _db.Orders.AsNoTracking().Include(x => x.Customer)
            .FirstOrDefaultAsync(x => x.Id == orderId) ?? throw new KeyNotFoundException("Заказ не найден");
        if (o.Status == OrderStatus.Completed) throw new InvalidOperationException("Заказ уже оплачен и закрыт");
        if (o.TotalCost <= 0) throw new InvalidOperationException("Сумма заказа не указана");

        var data = new Dictionary<string, string>
        {
            ["order"] = o.OrderNumber,
            ["amount"] = o.TotalCost.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
            ["currency"] = "TJS",
            ["client"] = o.Customer?.FullName ?? "",
        };
        return Sign("payment", data, DateTime.UtcNow.AddHours(24), $"Оплата {o.TotalCost:N2} TJS · {o.OrderNumber}");
    }

    public async Task<SignedQrDto> CreatePromoAsync(string code)
    {
        var normalized = code.Trim().ToUpperInvariant();
        var p = await _db.PromoCodes.AsNoTracking().FirstOrDefaultAsync(x => x.Code == normalized)
            ?? throw new KeyNotFoundException("Промокод не найден");
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (!p.IsActive || p.ExpiresAt < today) throw new InvalidOperationException("Промокод неактивен или истёк");

        var expires = TimeZoneHelper.ToUtc(p.ExpiresAt.ToDateTime(new TimeOnly(23, 59, 59)));
        var data = new Dictionary<string, string> { ["code"] = p.Code, ["discount"] = $"{p.DiscountPercent}%" };
        return Sign("promo", data, expires, $"Промо {p.Code} · скидка {p.DiscountPercent}%");
    }

    public QrVerificationDto Verify(string payload)
    {
        var parts = payload.Trim().Split('.');
        if (parts.Length != 3 || parts[0] != Prefix) return Invalid("Неизвестный формат QR");

        byte[] body, sig;
        try { body = Base64UrlDecode(parts[1]); sig = Base64UrlDecode(parts[2]); }
        catch (FormatException) { return Invalid("Повреждённые данные"); }

        var expected = HMACSHA256.HashData(_key, Encoding.ASCII.GetBytes($"{Prefix}.{parts[1]}"));
        if (!CryptographicOperations.FixedTimeEquals(expected, sig)) return Invalid("Подпись недействительна — QR подделан");

        Envelope? env;
        try { env = JsonSerializer.Deserialize<Envelope>(body, Json); }
        catch (JsonException) { return Invalid("Повреждённые данные"); }
        if (env is null) return Invalid("Повреждённые данные");

        if (DateTimeOffset.FromUnixTimeSeconds(env.Exp) < DateTimeOffset.UtcNow)
            return new QrVerificationDto(false, env.T, env.D, "Срок действия истёк");

        return new QrVerificationDto(true, env.T, env.D, null);
    }

    private SignedQrDto Sign(string type, Dictionary<string, string> data, DateTime expiresUtc, string label)
    {
        var env = new Envelope(type, data, new DateTimeOffset(DateTime.SpecifyKind(expiresUtc, DateTimeKind.Utc)).ToUnixTimeSeconds());
        var body = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(env, Json));
        var sig = HMACSHA256.HashData(_key, Encoding.ASCII.GetBytes($"{Prefix}.{body}"));
        return new SignedQrDto(type, $"{Prefix}.{body}.{Base64UrlEncode(sig)}", label, expiresUtc);
    }

    private static QrVerificationDto Invalid(string reason) => new(false, null, null, reason);

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static byte[] Base64UrlDecode(string s)
    {
        s = s.Replace('-', '+').Replace('_', '/');
        s = s.PadRight(s.Length + (4 - s.Length % 4) % 4, '=');
        return Convert.FromBase64String(s);
    }
}
