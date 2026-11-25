using AutoMaster.Application.DTOs;

namespace AutoMaster.Application.Interfaces;

public interface IOperationsService
{
    Task<List<ChecklistItemDto>> GetChecklistAsync(string type);
    Task<ChecklistItemDto> ToggleChecklistItemAsync(string type, Guid itemId);
    Task<ChecklistItemDto> AddChecklistItemAsync(AddChecklistItemRequest request);
    Task<List<SensorReadingDto>> GetSensorsAsync();
    Task<List<SlaViolationDto>> GetSlaViolationsAsync();
}

public interface IAuditService
{
    Task LogAsync(Guid userId, string action, string objectType, string objectId, string? details, Guid? branchId = null);
    Task<List<AuditLogDto>> GetLogsAsync(Guid? userId, string? objectType, DateTime? from, DateTime? to);
    Task<MysteryAuditDto> SaveMysteryAuditAsync(Guid userId, SaveMysteryAuditRequest request);
    Task<List<MysteryAuditDto>> GetMysteryAuditsAsync();
}
