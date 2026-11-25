using AutoMaster.Application.DTOs;
using FluentValidation;

namespace AutoMaster.Application.Validators;

public class AwardPointsRequestValidator : AbstractValidator<AwardPointsRequest>
{
    public AwardPointsRequestValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Points).GreaterThan(0);
    }
}

public class RedeemPointsRequestValidator : AbstractValidator<RedeemPointsRequest>
{
    public RedeemPointsRequestValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Points).GreaterThan(0);
    }
}
