using FluentValidation;
using netcore_crud.Models.Security;

namespace netcore_crud.Validators.Security;

public class ManageUsersValidator : AbstractValidator<LoginNamesModel>
{
    public ManageUsersValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required.")
            .MaximumLength(50)
            .WithMessage("Username cannot exceed 50 characters.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.")
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email Address is required.")
            .EmailAddress()
            .WithMessage("Please enter a valid email address.")
            .MaximumLength(200)
            .WithMessage("Email Address cannot exceed 200 characters.");
    }
}