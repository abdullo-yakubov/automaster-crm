using AutoMaster.Application.DTOs;
using FluentValidation;

namespace AutoMaster.Application.Validators;

public class CreateMechanicRequestValidator : AbstractValidator<CreateMechanicRequest>
{
    public CreateMechanicRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty();
        RuleFor(x => x.Specialization).Must(s => new[] { "engine", "suspension", "electrical", "body", "general" }.Contains(s.ToLower()));
        RuleFor(x => x.Level).Must(l => new[] { "junior", "middle", "senior", "expert" }.Contains(l.ToLower()));
        RuleFor(x => x.Salary).GreaterThanOrEqualTo(0);
    }
}

public class UpdateMechanicRequestValidator : AbstractValidator<UpdateMechanicRequest>
{
    public UpdateMechanicRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty();
        RuleFor(x => x.Specialization).Must(s => new[] { "engine", "suspension", "electrical", "body", "general" }.Contains(s.ToLower()));
        RuleFor(x => x.Level).Must(l => new[] { "junior", "middle", "senior", "expert" }.Contains(l.ToLower()));
        RuleFor(x => x.Salary).GreaterThanOrEqualTo(0);
    }
}
