using AutoMaster.Application.DTOs;
using FluentValidation;

namespace AutoMaster.Application.Validators;

public class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
{
    public CreateBookingRequestValidator()
    {
        RuleFor(x => x.CustomerName).NotEmpty();
        RuleFor(x => x.Phone).Matches(@"^\+992\d{9}$");
        RuleFor(x => x.Time).Matches(@"^\d{2}:\d{2}$");
        RuleFor(x => x.Service).NotEmpty();
    }
}
