using System.Security.Claims;
using AutoMaster.Application.DTOs;
using AutoMaster.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AutoMaster.API.Controllers;

internal static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user) =>
        Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : throw new UnauthorizedAccessException("Нет идентификатора пользователя");
}

[ApiController]
[Route("api/analytics")]
[Authorize(Roles = "Admin,Manager,Accountant")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _service;
    public AnalyticsController(IAnalyticsService service) => _service = service;

    [HttpGet("popular-services")]
    public async Task<ActionResult<List<PopularServiceDto>>> PopularServices([FromQuery] int top = 10) => Ok(await _service.GetPopularServicesAsync(top));

    [HttpGet("mechanics")]
    public async Task<ActionResult<List<MechanicPerformanceDto>>> Mechanics() => Ok(await _service.GetMechanicPerformanceAsync());

    [HttpGet("heatmap")]
    public async Task<ActionResult<HeatmapDto>> Heatmap([FromQuery] int days = 90) => Ok(await _service.GetHeatmapAsync(days));

    [HttpGet("forecast")]
    public async Task<ActionResult<ForecastDto>> Forecast() => Ok(await _service.GetForecastAsync());
}

[ApiController]
[Route("api/ai")]
[Authorize(Roles = "Admin,Manager,Mechanic")]
public class AiController : ControllerBase
{
    private readonly IAiService _service;
    public AiController(IAiService service) => _service = service;

    [HttpPost("diagnose")]
    public async Task<ActionResult<DiagnosisResultDto>> Diagnose(DiagnosisRequest request) => Ok(await _service.DiagnoseAsync(request));

    [HttpGet("predictive")]
    public async Task<ActionResult<List<PredictiveItemDto>>> Predictive() => Ok(await _service.GetPredictiveMaintenanceAsync());

    [HttpGet("issues-by-make")]
    public async Task<ActionResult<List<MakeIssueStatDto>>> IssuesByMake() => Ok(await _service.GetIssuesByMakeAsync());
}

[ApiController]
[Route("api/marketing")]
[Authorize(Roles = "Admin,Manager")]
public class MarketingController : ControllerBase
{
    private readonly IMarketingService _service;
    public MarketingController(IMarketingService service) => _service = service;

    [HttpGet("stats")]
    public async Task<ActionResult<MarketingStatsDto>> Stats() => Ok(await _service.GetStatsAsync());

    [HttpGet("promos")]
    public async Task<ActionResult<List<PromoCodeDto>>> Promos() => Ok(await _service.GetPromoCodesAsync());

    [HttpPost("promos")]
    public async Task<ActionResult<PromoCodeDto>> CreatePromo(CreatePromoCodeRequest request) => Ok(await _service.CreatePromoCodeAsync(request));

    [HttpPatch("promos/{id:guid}")]
    public async Task<ActionResult<PromoCodeDto>> SetActive(Guid id, SetActiveRequest request) => Ok(await _service.SetPromoActiveAsync(id, request.IsActive));

    [HttpDelete("promos/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeletePromo(Guid id)
    {
        await _service.DeletePromoCodeAsync(id);
        return NoContent();
    }

    [HttpGet("promos/validate/{code}")]
    public async Task<ActionResult<PromoValidationDto>> Validate(string code) => Ok(await _service.ValidatePromoAsync(code));

    [HttpGet("campaigns")]
    public async Task<ActionResult<List<CampaignDto>>> Campaigns() => Ok(await _service.GetCampaignsAsync());

    [HttpPost("campaigns")]
    public async Task<ActionResult<CampaignDto>> CreateCampaign(CreateCampaignRequest request) =>
        Ok(await _service.CreateCampaignAsync(request, User.GetUserId()));

    [HttpGet("templates/{segment}")]
    public ActionResult<CampaignTemplateDto> Template(string segment) => Ok(_service.GetTemplate(segment));
}

[ApiController]
[Route("api/digital-twin")]
[Authorize(Roles = "Admin,Manager")]
public class DigitalTwinController : ControllerBase
{
    private readonly IDigitalTwinService _service;
    public DigitalTwinController(IDigitalTwinService service) => _service = service;

    [HttpGet]
    [Authorize(Roles = "Admin,Manager,Mechanic")]
    public async Task<ActionResult<DigitalTwinDto>> Get() => Ok(await _service.GetStateAsync());

    [HttpPost("boxes")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ServiceBoxDto>> CreateBox(CreateServiceBoxRequest request) => Ok(await _service.CreateBoxAsync(request));

    [HttpPost("boxes/{id:guid}/assign")]
    public async Task<IActionResult> Assign(Guid id, AssignBoxRequest request)
    {
        await _service.AssignAsync(id, request.OrderId);
        return NoContent();
    }

    [HttpPost("boxes/{id:guid}/release")]
    public async Task<IActionResult> Release(Guid id)
    {
        await _service.ReleaseAsync(id);
        return NoContent();
    }
}

[ApiController]
[Route("api/voice")]
[Authorize(Roles = "Admin,Manager,Mechanic,Accountant")]
public class VoiceController : ControllerBase
{
    private readonly IVoiceService _service;
    public VoiceController(IVoiceService service) => _service = service;

    [HttpPost("command")]
    public async Task<ActionResult<VoiceCommandResultDto>> Command(VoiceCommandRequest request) =>
        Ok(await _service.ProcessAsync(request.Transcript, User.GetUserId()));

    [HttpGet("history")]
    public async Task<ActionResult<List<VoiceHistoryItemDto>>> History([FromQuery] int take = 20) =>
        Ok(await _service.GetHistoryAsync(User.GetUserId(), take));
}

[ApiController]
[Route("api/portal")]
public class ClientPortalController : ControllerBase
{
    private readonly IClientPortalService _service;
    public ClientPortalController(IClientPortalService service) => _service = service;

    /// <summary>Личный кабинет текущего клиента — видит только свои данные.</summary>
    [HttpGet("me")]
    [Authorize(Roles = "Client")]
    public async Task<ActionResult<ClientPortalDto>> Me() => Ok(await _service.GetForUserAsync(User.GetUserId()));

    /// <summary>Предпросмотр кабинета сотрудником.</summary>
    [HttpGet("customers/{customerId:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ClientPortalDto>> Preview(Guid customerId) => Ok(await _service.GetForCustomerAsync(customerId));
}

[ApiController]
[Route("api/qr")]
[Authorize(Roles = "Admin,Manager")]
public class QrController : ControllerBase
{
    private readonly IQrService _service;
    public QrController(IQrService service) => _service = service;

    [HttpPost("warranty")]
    public async Task<ActionResult<SignedQrDto>> Warranty(OrderQrRequest request) => Ok(await _service.CreateWarrantyAsync(request.OrderId));

    [HttpPost("payment")]
    public async Task<ActionResult<SignedQrDto>> Payment(OrderQrRequest request) => Ok(await _service.CreatePaymentAsync(request.OrderId));

    [HttpPost("promo")]
    public async Task<ActionResult<SignedQrDto>> Promo(PromoQrRequest request) => Ok(await _service.CreatePromoAsync(request.Code));

    /// <summary>Публичная проверка подлинности QR (например, гарантийного талона).</summary>
    [HttpPost("verify")]
    [AllowAnonymous]
    [EnableRateLimiting("public")]
    public ActionResult<QrVerificationDto> Verify(QrVerifyRequest request) => Ok(_service.Verify(request.Payload));
}

[ApiController]
[Route("api/bi")]
[Authorize(Roles = "Admin,Manager,Accountant")]
public class BiController : ControllerBase
{
    private readonly IBiService _service;
    public BiController(IBiService service) => _service = service;

    [HttpGet("widgets")]
    public async Task<ActionResult<List<string>>> Widgets() => Ok(await _service.GetWidgetsAsync(User.GetUserId()));

    [HttpPut("widgets")]
    public async Task<ActionResult<List<string>>> SaveWidgets(SaveWidgetsRequest request) =>
        Ok(await _service.SaveWidgetsAsync(User.GetUserId(), request.Widgets));

    [HttpGet("summary")]
    public async Task<ActionResult<BiSummaryDto>> Summary() => Ok(await _service.GetSummaryAsync());
}
