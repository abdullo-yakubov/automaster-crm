using AutoMaster.Application.DTOs;
using FluentValidation;

namespace AutoMaster.Application.Validators;

public class CreateAdminUserRequestValidator : AbstractValidator<CreateAdminUserRequest>
{
    public CreateAdminUserRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).MinimumLength(8);
        RuleFor(x => x.FullName).NotEmpty();
        RuleFor(x => x.Role).Must(r => new[] { "admin", "manager", "mechanic", "accountant", "client" }.Contains(r.ToLower()));
    }
}

public class UpdateAdminUserRequestValidator : AbstractValidator<UpdateAdminUserRequest>
{
    public UpdateAdminUserRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty();
        RuleFor(x => x.Role).Must(r => new[] { "admin", "manager", "mechanic", "accountant", "client" }.Contains(r.ToLower()));
    }
}
