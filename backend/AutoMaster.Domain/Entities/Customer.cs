using AutoMaster.Domain.Enums;

namespace AutoMaster.Domain.Entities;

public class Customer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateOnly? Birthday { get; set; }
    public CustomerStatus Status { get; set; } = CustomerStatus.Regular;
    public int LoyaltyPoints { get; set; }
    public decimal TotalSpent { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<Vehicle> Vehicles { get; set; } = new();
    public List<Order> Orders { get; set; } = new();
}
