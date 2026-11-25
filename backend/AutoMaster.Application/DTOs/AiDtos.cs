namespace AutoMaster.Application.DTOs;

public record DiagnosisRequest(string? PlateOrVin, string Problem, int? Mileage);

public record DiagnosisHypothesisDto(
    string Issue,
    string Recommendation,
    int Probability,
    decimal CostFrom,
    decimal CostTo,
    string Urgency);

public record DiagnosisResultDto(
    string? VehicleTitle,
    int? Mileage,
    string Problem,
    List<DiagnosisHypothesisDto> Hypotheses,
    List<string> RecentServices,
    string Disclaimer);

public record PredictiveItemDto(
    Guid VehicleId,
    string Title,
    string LicensePlate,
    string OwnerName,
    string OwnerPhone,
    int Year,
    int Mileage,
    DateTime? LastServiceAt,
    string RiskLevel,
    List<string> Recommendations);

public record MakeIssueStatDto(string Make, int Orders, List<string> TopServices);
