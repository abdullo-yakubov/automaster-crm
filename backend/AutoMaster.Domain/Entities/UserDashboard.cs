namespace AutoMaster.Domain.Entities;

/// <summary>Персональная раскладка виджетов BI-конструктора.</summary>
public class UserDashboard
{
    public Guid UserId { get; set; }
    public List<string> Widgets { get; set; } = new();
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
