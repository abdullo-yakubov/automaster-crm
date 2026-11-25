namespace AutoMaster.Domain.Entities;

public class OrderTimelineEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Note { get; set; }
    public Guid? UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
