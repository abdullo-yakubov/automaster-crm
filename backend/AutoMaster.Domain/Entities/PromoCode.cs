namespace AutoMaster.Domain.Entities;

public class PromoCode
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public int DiscountPercent { get; set; }
    public DateOnly ExpiresAt { get; set; }
    public int UsedCount { get; set; }
    public int? MaxUses { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
