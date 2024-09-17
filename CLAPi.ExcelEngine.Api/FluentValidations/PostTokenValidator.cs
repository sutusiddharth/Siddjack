using CLAPi.ExcelEngine.Api.Models;
using FluentValidation;

namespace CLAPi.ExcelEngine.Api.FluentValidations
{
    public class PostTokenValidator : AbstractValidator<PostToken>
    {
        public PostTokenValidator()
        {
            RuleFor(a => a.User_Type).NotEmpty()
                .WithMessage("User Type is required.");

            RuleFor(a => a.User_Nm).NotEmpty()
                .WithMessage("User Name is required.");

            RuleFor(a => a.Password).NotEmpty().When(a => !a.Is_External)
                .WithMessage("Password is required.");

            RuleFor(a => a.Application_Source).NotEmpty()
                .WithMessage("Application Source is required");

            RuleFor(a => a.Secret_Key).NotEmpty()
                .WithMessage("Secret Key is required");
        }
    }
}
