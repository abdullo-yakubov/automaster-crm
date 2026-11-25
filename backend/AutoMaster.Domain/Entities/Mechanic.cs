using AutoMaster.Domain.Enums;

namespace AutoMaster.Domain.Entities;

public class Mechanic
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? UserId { get; set; }
    public User? User { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public MechanicSpecialization Specialization { get; set; }
    public MechanicLevel Level { get; set; }
    public decimal Salary { get; set; }
    public bool IsActive { get; set; } = true;
    public decimal Rating { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Order> Orders { get; set; } = new();
}
