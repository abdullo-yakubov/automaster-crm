using AutoMaster.Application.DTOs;

namespace AutoMaster.Application.Interfaces;

public interface IAnalyticsService
{
    Task<List<PopularServiceDto>> GetPopularServicesAsync(int top = 10);
    Task<List<MechanicPerformanceDto>> GetMechanicPerformanceAsync();
    Task<HeatmapDto> GetHeatmapAsync(int days = 90);
    Task<ForecastDto> GetForecastAsync();
}

public interface IAiService
{
    Task<DiagnosisResultDto> DiagnoseAsync(DiagnosisRequest request);
    Task<List<PredictiveItemDto>> GetPredictiveMaintenanceAsync();
    Task<List<MakeIssueStatDto>> GetIssuesByMakeAsync();
}

public interface IMarketingService
{
    Task<MarketingStatsDto> GetStatsAsync();
    Task<List<PromoCodeDto>> GetPromoCodesAsync();
    Task<PromoCodeDto> CreatePromoCodeAsync(CreatePromoCodeRequest request);
    Task<PromoCodeDto> SetPromoActiveAsync(Guid id, bool isActive);
    Task DeletePromoCodeAsync(Guid id);
    Task<PromoValidationDto> ValidatePromoAsync(string code);
    Task<List<CampaignDto>> GetCampaignsAsync();
    Task<CampaignDto> CreateCampaignAsync(CreateCampaignRequest request, Guid userId);
    CampaignTemplateDto GetTemplate(string segment);
}

public interface IDigitalTwinService
{
    Task<DigitalTwinDto> GetStateAsync();
    Task<ServiceBoxDto> CreateBoxAsync(CreateServiceBoxRequest request);
    Task AssignAsync(Guid boxId, Guid orderId);
    Task ReleaseAsync(Guid boxId);
}

public interface IVoiceService
{
    Task<VoiceCommandResultDto> ProcessAsync(string transcript, Guid userId);
    Task<List<VoiceHistoryItemDto>> GetHistoryAsync(Guid userId, int take = 20);
}

public interface IClientPortalService
{
    Task<ClientPortalDto> GetForCustomerAsync(Guid customerId);
    Task<ClientPortalDto> GetForUserAsync(Guid userId);
}

/// <summary>Подписанные (HMAC-SHA256) QR-коды: подделать гарантию или сумму оплаты нельзя.</summary>
public interface IQrService
{
    Task<SignedQrDto> CreateWarrantyAsync(Guid orderId);
    Task<SignedQrDto> CreatePaymentAsync(Guid orderId);
    Task<SignedQrDto> CreatePromoAsync(string code);
    QrVerificationDto Verify(string payload);
}

public interface IBiService
{
    static readonly string[] AvailableWidgets =
    {
        "revenue_card", "orders_chart", "mechanics_kpi_widget", "warehouse_alert",
        "top_services", "profit_margin", "customer_map", "sla_monitor"
    };

    Task<List<string>> GetWidgetsAsync(Guid userId);
    Task<List<string>> SaveWidgetsAsync(Guid userId, List<string> widgets);
    Task<BiSummaryDto> GetSummaryAsync();
}
