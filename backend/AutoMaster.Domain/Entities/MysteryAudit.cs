namespace AutoMaster.Domain.Entities;

public class MysteryAudit
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ScoresJson { get; set; } = "{}";
    public decimal TotalScore { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public Guid? BranchId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
