using AutoMaster.Application.DTOs;
using FluentValidation;

namespace AutoMaster.Application.Validators;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CustomerPhone).NotEmpty().Matches(@"^\+992\d{9}$").WithMessage("Телефон должен быть в формате +992XXXXXXXXX");
        RuleFor(x => x.VehicleMake).NotEmpty();
        RuleFor(x => x.VehicleModel).NotEmpty();
        RuleFor(x => x.LicensePlate).NotEmpty();
        RuleFor(x => x.Mileage).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ServiceDescription).NotEmpty();
        RuleFor(x => x.Priority).Must(p => new[] { "normal", "high", "urgent" }.Contains(p.ToLower()));
        RuleFor(x => x.LaborCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PartsCost).GreaterThanOrEqualTo(0);
    }
}

public class UpdateOrderStatusRequestValidator : AbstractValidator<UpdateOrderStatusRequest>
{
    public UpdateOrderStatusRequestValidator()
    {
        RuleFor(x => x.Status).NotEmpty().Must(s => new[]
        {
            "intake", "diagnosis", "estimate", "approved", "inprogress", "waiting", "ready", "completed"
        }.Contains(s.ToLower()));
    }
}
