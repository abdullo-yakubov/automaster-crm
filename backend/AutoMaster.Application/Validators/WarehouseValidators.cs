using AutoMaster.Application.DTOs;
using FluentValidation;

namespace AutoMaster.Application.Validators;

public class CreatePartRequestValidator : AbstractValidator<CreatePartRequest>
{
    private static readonly string[] Categories = { "filters", "oils", "brakes", "suspension", "engine", "electrical", "body", "consumables" };

    public CreatePartRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Category).Must(c => Categories.Contains(c.ToLower()));
        RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MinQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PurchasePrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SalePrice).GreaterThanOrEqualTo(0);
    }
}

public class UpdatePartRequestValidator : AbstractValidator<UpdatePartRequest>
{
    private static readonly string[] Categories = { "filters", "oils", "brakes", "suspension", "engine", "electrical", "body", "consumables" };

    public UpdatePartRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Category).Must(c => Categories.Contains(c.ToLower()));
        RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MinQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PurchasePrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SalePrice).GreaterThanOrEqualTo(0);
    }
}
