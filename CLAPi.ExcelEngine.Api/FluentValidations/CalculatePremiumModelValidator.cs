using CLAPi.ExcelEngine.Api.Models;
using FluentValidation;

namespace CLAPi.ExcelEngine.Api.FluentValidations;

public class CalculatePremiumModelValidator : AbstractValidator<CalculatePremiumModel>
{
    public CalculatePremiumModelValidator()
    {
        RuleFor(x => x.Folder_Nm)
            .NotEmpty().WithMessage("Folder Name is required.");

        RuleFor(x => x.SubFolder_Nm)
            .NotEmpty().WithMessage("Sub Folder Name is required.");

    }
}
