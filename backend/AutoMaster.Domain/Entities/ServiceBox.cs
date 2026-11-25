namespace AutoMaster.Domain.Entities;

/// <summary>Рабочий бокс (подъёмник) сервиса — основа Digital Twin.</summary>
public class ServiceBox
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? CurrentOrderId { get; set; }
    public Order? CurrentOrder { get; set; }
    public DateTime? OccupiedSince { get; set; }
}
