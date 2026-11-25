namespace AutoMaster.Application.DTOs;

public record ChecklistItemDto(Guid Id, string Type, string Title, bool IsDone, int SortOrder);

public record AddChecklistItemRequest(string Type, string Title);

public record SensorReadingDto(string Name, string Type, double Value, string Unit, DateTime ReadAt);

public record SlaViolationDto(
    Guid OrderId,
    string OrderNumber,
    string CustomerName,
    string Status,
    int SlaHours,
    double ElapsedHours,
    double OverHours);

public record MysteryAuditScoreDto(
    int Greeting,
    int Speed,
    int Cleanliness,
    int Politeness,
    int RepairQuality,
    int BillAccuracy,
    int HandoverTime);

public record SaveMysteryAuditRequest(MysteryAuditScoreDto Scores);

public record MysteryAuditDto(Guid Id, MysteryAuditScoreDto Scores, decimal TotalScore, DateTime CreatedAt);

public record AuditLogDto(
    Guid Id,
    DateTime CreatedAt,
    string UserName,
    string Action,
    string ObjectType,
    string ObjectId,
    string? Details);
