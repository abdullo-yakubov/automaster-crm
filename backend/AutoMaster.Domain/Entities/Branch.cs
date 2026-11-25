using AutoMaster.Domain.Enums;

namespace AutoMaster.Domain.Entities;

public class Branch
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public BranchType Type { get; set; } = BranchType.Own;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
