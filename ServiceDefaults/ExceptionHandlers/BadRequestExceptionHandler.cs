using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServiceDefaults.Exceptions;

namespace ServiceDefaults.ExceptionHandlers;
public sealed class BadRequestExceptionHandler(ILogger<BadRequestExceptionHandler> logger) : IExceptionHandler
{
    private readonly ILogger<BadRequestExceptionHandler> _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not BadRequestException badRequestException)
        {
            return false;
        }

        _logger.LogError(
            badRequestException,
            "Exception occurred: {Message}",
            badRequestException.Message);

        var problemDetails = new ProblemDetails
        {
            Type = "ValidationException",
            Status = StatusCodes.Status400BadRequest,
            Title = "Bad Request",
            Detail = badRequestException.Message
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response
            .WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
