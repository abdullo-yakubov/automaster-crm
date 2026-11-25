namespace AutoMaster.Application.DTOs;

public record MarketingStatsDto(int Campaigns, int TotalRecipients, int ActivePromos, Dictionary<string, int> SegmentSizes);

public record PromoCodeDto(Guid Id, string Code, int DiscountPercent, DateOnly ExpiresAt, int UsedCount, int? MaxUses, bool IsActive, bool IsExpired);

public record CreatePromoCodeRequest(string? Code, int DiscountPercent, DateOnly ExpiresAt, int? MaxUses);

public record SetActiveRequest(bool IsActive);

public record PromoValidationDto(bool IsValid, string Code, int DiscountPercent, string? Reason);

public record CampaignDto(Guid Id, string Name, string Channel, string Segment, string Message, int RecipientsCount, string Status, DateTime CreatedAt);

public record CreateCampaignRequest(string Channel, string Segment, string Message);

public record CampaignTemplateDto(string Segment, string Text);
