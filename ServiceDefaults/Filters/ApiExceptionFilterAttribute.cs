using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ServiceDefaults.Exceptions;

namespace ServiceDefaults.Filters;

public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
{
    private static readonly Dictionary<Type, Action<ExceptionContext>> _exceptionHandlers = new()
    {
        { typeof(DataValidationException), HandleValidationException },
        { typeof(NotFoundException), HandleNotFoundException },
        { typeof(UnauthorizedAccessException), HandleUnauthorizedAccessException },
        { typeof(ForbiddenAccessException), HandleForbiddenAccessException },
    };

    public override void OnException(ExceptionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (_exceptionHandlers.TryGetValue(context.Exception.GetType(), out var handler))
        {
            handler(context);
        }
        else if (!context.ModelState.IsValid)
        {
            HandleInvalidModelStateException(context);
        }
        else
        {
            // Handle other exceptions or log them here if needed
        }

        base.OnException(context);
    }

    private static void HandleValidationException(ExceptionContext context)
    {
        var exception = (DataValidationException)context.Exception;
        var details = new ValidationProblemDetails(exception.Errors)
        {
            Type = "ValidationException",
            Title = "BadRequest"
        };

        context.Result = new BadRequestObjectResult(details);
        context.ExceptionHandled = true;
    }

    private static void HandleInvalidModelStateException(ExceptionContext context)
    {
        var details = new ValidationProblemDetails(context.ModelState)
        {
            Type = "ValidationException",
            Title = "BadRequest"
        };

        context.Result = new BadRequestObjectResult(details);
        context.ExceptionHandled = true;
    }

    private static void HandleNotFoundException(ExceptionContext context)
    {
        var exception = (NotFoundException)context.Exception;
        var details = new ProblemDetails
        {
            Type = "NotFound",
            Title = "The specified resource was not found.",
            Detail = exception.Message
        };

        context.Result = new NotFoundObjectResult(details);
        context.ExceptionHandled = true;
    }

    private static void HandleUnauthorizedAccessException(ExceptionContext context)
    {
        var details = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Type = "UnauthorizedAccess"
        };

        context.Result = new ObjectResult(details)
        {
            StatusCode = StatusCodes.Status401Unauthorized
        };

        context.ExceptionHandled = true;
    }

    private static void HandleForbiddenAccessException(ExceptionContext context)
    {
        var details = new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Forbidden",
            Type = "ForbiddenAccess"
        };

        context.Result = new ObjectResult(details)
        {
            StatusCode = StatusCodes.Status403Forbidden
        };

        context.ExceptionHandled = true;
    }
}
