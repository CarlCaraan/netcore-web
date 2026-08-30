using FluentValidation;
using Microsoft.Data.SqlClient;
using netcore_crud.App_Code;
using netcore_crud.Models.CardFile;

namespace netcore_crud.Validators.CardFile;

public class CardFileValidator : AbstractValidator<CardFileModel>
{
    public CardFileValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Code is required.");

        RuleFor(x => x.Type)
            .NotEmpty()
            .WithMessage("Type is required.");

        RuleFor(x => x.Address)
            .NotEmpty()
            .WithMessage("Address is required.");

        RuleFor(x => x.TINNumber)
            .NotEmpty()
            .WithMessage("TIN Number is required.");
    }
}
