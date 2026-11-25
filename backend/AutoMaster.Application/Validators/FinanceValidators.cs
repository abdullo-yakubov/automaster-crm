using AutoMaster.Application.DTOs;
using FluentValidation;

namespace AutoMaster.Application.Validators;

public class CreateTransactionRequestValidator : AbstractValidator<CreateTransactionRequest>
{
    public CreateTransactionRequestValidator()
    {
        RuleFor(x => x.Type).Must(t => new[] { "income", "expense" }.Contains(t.ToLower()));
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.Category).NotEmpty();
    }
}
