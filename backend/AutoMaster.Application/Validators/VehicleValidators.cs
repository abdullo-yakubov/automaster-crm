using AutoMaster.Application.DTOs;
using FluentValidation;

namespace AutoMaster.Application.Validators;

public class CreateVehicleRequestValidator : AbstractValidator<CreateVehicleRequest>
{
    public CreateVehicleRequestValidator()
    {
        RuleFor(x => x.Make).NotEmpty();
        RuleFor(x => x.Model).NotEmpty();
        RuleFor(x => x.Year).InclusiveBetween(1950, DateTime.UtcNow.Year + 1);
        RuleFor(x => x.LicensePlate).NotEmpty();
        RuleFor(x => x.Mileage).GreaterThanOrEqualTo(0);
        RuleFor(x => x).Must(x => x.CustomerId.HasValue || !string.IsNullOrWhiteSpace(x.OwnerName))
            .WithMessage("Укажите CustomerId или OwnerName");
    }
}

public class UpdateVehicleRequestValidator : AbstractValidator<UpdateVehicleRequest>
{
    public UpdateVehicleRequestValidator()
    {
        RuleFor(x => x.Make).NotEmpty();
        RuleFor(x => x.Model).NotEmpty();
        RuleFor(x => x.Year).InclusiveBetween(1950, DateTime.UtcNow.Year + 1);
        RuleFor(x => x.LicensePlate).NotEmpty();
        RuleFor(x => x.Mileage).GreaterThanOrEqualTo(0);
    }
}
