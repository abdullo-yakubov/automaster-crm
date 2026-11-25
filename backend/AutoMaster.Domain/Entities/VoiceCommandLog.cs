namespace AutoMaster.Domain.Entities;

public class VoiceCommandLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Transcript { get; set; } = string.Empty;
    public string Intent { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
