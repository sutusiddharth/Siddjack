using CLAPi.ExcelEngine.Api.Models;
using FluentValidation;

namespace CLAPi.ExcelEngine.Api.FluentValidations;

public class GenerateApiModelValidator : AbstractValidator<GenerateApiModel>
{
    public GenerateApiModelValidator()
    {
        RuleFor(x => x.Folder_Nm)
            .NotEmpty()
            .WithMessage("Folder Name is required.");

        RuleFor(x => x.SubFolder_Nm)
            .NotEmpty()
            .WithMessage("Sub Folder Name is required.");

        RuleFor(x => x.Upload_Type)
            .NotEmpty()
            .WithMessage("Upload Type is required.");

        RuleFor(x => x.Effective_From)
            .NotNull()
            .WithMessage("Effective from is required");

        RuleFor(x => x.Effective_From)
            .NotNull()
            .WithMessage("Effective from is required")
            .LessThanOrEqualTo(a => a.Effective_Upto)
            .When(a => a.Effective_Upto != null)
            .WithMessage("Effective from should not be greater than Effective upto");

        RuleFor(x => x.Effective_Upto)
            .NotNull()
            .WithMessage("Effective upto is required");
        
        RuleFor(x => x.Release_Note)
            .NotEmpty()
            .WithMessage("Release Note is required");
    }
}