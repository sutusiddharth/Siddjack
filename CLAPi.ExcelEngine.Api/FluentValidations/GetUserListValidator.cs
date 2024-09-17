using CLAPi.ExcelEngine.Api.Models;
using FluentValidation;

namespace CLAPi.ExcelEngine.Api.FluentValidations
{
    public class GetUserListValidator : AbstractValidator<GetUserList>
    {
        public GetUserListValidator()
        {
            ////RuleFor(x => x)
            ////    .Must(model => !string.IsNullOrEmpty(model.Email)
            ////    || !string.IsNullOrEmpty(model.Mobile)
            ////    || !string.IsNullOrEmpty(model.UserName)
            ////    ).WithMessage("Atleast one field is required");
        }
    }
}
