using AutoMaster.Application.DTOs;
using AutoMaster.Application.Interfaces;
using AutoMaster.Domain.Enums;
using FluentValidation;

namespace AutoMaster.Application.Validators;

public class DiagnosisRequestValidator : AbstractValidator<DiagnosisRequest>
{
    public DiagnosisRequestValidator()
    {
        RuleFor(x => x.Problem).NotEmpty().WithMessage("Опишите проблему").MaximumLength(1000);
        RuleFor(x => x.PlateOrVin).MaximumLength(32);
        RuleFor(x => x.Mileage).InclusiveBetween(0, 3_000_000).When(x => x.Mileage.HasValue);
    }
}

public class CreatePromoCodeRequestValidator : AbstractValidator<CreatePromoCodeRequest>
{
    public CreatePromoCodeRequestValidator()
    {
        RuleFor(x => x.Code)
            .Matches("^[A-Za-z0-9_-]{3,32}$").WithMessage("Код: 3–32 символа, латиница, цифры, - и _")
            .When(x => !string.IsNullOrWhiteSpace(x.Code));
        RuleFor(x => x.DiscountPercent).InclusiveBetween(1, 90);
        RuleFor(x => x.ExpiresAt).Must(d => d >= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Дата окончания не может быть в прошлом");
        RuleFor(x => x.MaxUses).GreaterThan(0).When(x => x.MaxUses.HasValue);
    }
}

public class CreateCampaignRequestValidator : AbstractValidator<CreateCampaignRequest>
{
    public CreateCampaignRequestValidator()
    {
        RuleFor(x => x.Channel).NotEmpty().IsEnumName(typeof(CampaignChannel), caseSensitive: false);
        RuleFor(x => x.Segment).NotEmpty().IsEnumName(typeof(CustomerSegment), caseSensitive: false);
        RuleFor(x => x.Message).NotEmpty().WithMessage("Введите текст сообщения").MaximumLength(1000);
    }
}

public class CreateServiceBoxRequestValidator : AbstractValidator<CreateServiceBoxRequest>
{
    public CreateServiceBoxRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Specialization).NotEmpty().MaximumLength(50);
    }
}

public class AssignBoxRequestValidator : AbstractValidator<AssignBoxRequest>
{
    public AssignBoxRequestValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }
}

public class VoiceCommandRequestValidator : AbstractValidator<VoiceCommandRequest>
{
    public VoiceCommandRequestValidator()
    {
        RuleFor(x => x.Transcript).NotEmpty().MaximumLength(500);
    }
}

public class PromoQrRequestValidator : AbstractValidator<PromoQrRequest>
{
    public PromoQrRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(32);
    }
}

public class QrVerifyRequestValidator : AbstractValidator<QrVerifyRequest>
{
    public QrVerifyRequestValidator()
    {
        RuleFor(x => x.Payload).NotEmpty().MaximumLength(4096);
    }
}

public class SaveWidgetsRequestValidator : AbstractValidator<SaveWidgetsRequest>
{
    public SaveWidgetsRequestValidator()
    {
        RuleFor(x => x.Widgets).NotNull().Must(w => w.Count <= 20).WithMessage("Слишком много виджетов");
        RuleForEach(x => x.Widgets).Must(w => IBiService.AvailableWidgets.Contains(w))
            .WithMessage("Неизвестный виджет: {PropertyValue}");
    }
}
