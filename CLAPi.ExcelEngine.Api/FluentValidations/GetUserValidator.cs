using CLAPi.ExcelEngine.Api.Models;
using FluentValidation;

namespace CLAPi.ExcelEngine.Api.FluentValidations;

public class GetUserValidator : AbstractValidator<GetUser>
{
    public GetUserValidator()
    {
        RuleFor(a => a.User_Nm).NotEmpty().WithMessage("User Name is Required");
    }
}
