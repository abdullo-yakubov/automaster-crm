namespace AutoMaster.Domain.Entities;

public class AppSettings
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? BranchId { get; set; }
    public string ServiceName { get; set; } = "AutoMaster";
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Currency { get; set; } = "TJS";
    public string Theme { get; set; } = "dark";
    public string Language { get; set; } = "ru";
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
