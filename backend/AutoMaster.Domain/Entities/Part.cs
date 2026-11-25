using AutoMaster.Domain.Enums;

namespace AutoMaster.Domain.Entities;

public class Part
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public PartCategory Category { get; set; }
    public int Quantity { get; set; }
    public int MinQuantity { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
