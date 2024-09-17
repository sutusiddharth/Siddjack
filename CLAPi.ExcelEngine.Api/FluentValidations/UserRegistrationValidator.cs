using CLAPi.ExcelEngine.Api.DTOs;
using FluentValidation;

namespace CLAPi.ExcelEngine.Api.FluentValidations
{
    public class UserRegistrationValidator:AbstractValidator<UserRegistrationDto>
    {
        public UserRegistrationValidator()
        {
            RuleFor(x=>x.First_Nm).NotEmpty().WithMessage("First Name is Required.");
            RuleFor(x=>x.User_Nm).NotEmpty().WithMessage("User Name is Required.");
            RuleFor(x=>x.Password).NotEmpty().WithMessage("Password is Required.");
            RuleFor(x => x.Email).NotEmpty().WithMessage("Email is Required.");
        }
    }
}
