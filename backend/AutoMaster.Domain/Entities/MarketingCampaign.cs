using AutoMaster.Domain.Enums;

namespace AutoMaster.Domain.Entities;

public class MarketingCampaign
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public CampaignChannel Channel { get; set; }
    public CustomerSegment Segment { get; set; }
    public string Message { get; set; } = string.Empty;
    public int RecipientsCount { get; set; }
    public CampaignStatus Status { get; set; } = CampaignStatus.Queued;
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
