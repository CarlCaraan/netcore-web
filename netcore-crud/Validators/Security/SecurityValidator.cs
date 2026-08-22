using FluentValidation;
using Microsoft.Data.SqlClient;
using netcore_crud.App_Code;
using netcore_crud.Models.Security;

namespace netcore_crud.Validators.Security;

public class SecurityValidator : AbstractValidator<SecurityModel>
{
    public SecurityValidator()
    {
        RuleFor(x => x.GroupName)
            .NotEmpty()
            .WithMessage("Group Name is required.");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required.")
            .MaximumLength(200)
            .WithMessage("Description cannot exceed 200 characters.");
    }
}
