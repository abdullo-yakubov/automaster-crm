using AutoMaster.Domain.Enums;

namespace AutoMaster.Domain.Entities;

public class LoyaltyTransaction
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public LoyaltyTransactionType Type { get; set; }
    public int Points { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
