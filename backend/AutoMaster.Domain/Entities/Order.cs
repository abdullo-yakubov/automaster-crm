using AutoMaster.Domain.Enums;

namespace AutoMaster.Domain.Entities;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OrderNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public Guid VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }
    public Guid? MechanicId { get; set; }
    public Mechanic? Mechanic { get; set; }
    public string ServiceDescription { get; set; } = string.Empty;
    public OrderStatus Status { get; set; } = OrderStatus.Intake;
    public OrderPriority Priority { get; set; } = OrderPriority.Normal;
    public decimal LaborCost { get; set; }
    public decimal PartsCost { get; set; }
    public decimal TotalCost { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public List<OrderTimelineEntry> Timeline { get; set; } = new();
}
