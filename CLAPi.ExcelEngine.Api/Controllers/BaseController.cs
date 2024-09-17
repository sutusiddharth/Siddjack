using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ServiceDefaults.Exceptions;
using ServiceDefaults.Filters;
using System.Net.Mime;

namespace CLAPi.ExcelEngine.Api.Controllers;

[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
[ApiExceptionFilter]
[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status403Forbidden)]
[ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
public abstract class BaseController : Controller
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            // Model state is invalid, construct validation errors

            var failures = new List<ValidationFailure>();
            foreach (var key in context.ModelState.Keys)
            {
                if (context.ModelState[key] != null)
                {
                    var errorMessages = context.ModelState[key]!.Errors //ModelErrorCollection
                    .Select(error => error.ErrorMessage)
                    .ToArray();
                    failures.AddRange(errorMessages.Select(errorMessage => new ValidationFailure(key, errorMessage)));
                }
            }
            if (failures.Count > 0)
            {
                throw new DataValidationException(failures);
            }
        }

        base.OnActionExecuting(context);
    }
}
