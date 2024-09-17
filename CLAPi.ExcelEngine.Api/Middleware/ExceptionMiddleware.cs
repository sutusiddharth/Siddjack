namespace CLAPi.ExcelEngine.Middleware;

// You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    private readonly RequestDelegate _next = next ?? throw new ArgumentNullException(nameof(next));
    private readonly ILogger<ExceptionMiddleware> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            LogException(ex);
            await HandleExceptionAsync(httpContext, ex);
        }
    }
    private void LogException(Exception ex)
    {
        _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
        if (ex.InnerException != null)
        {
            _logger.LogError(ex, "Inner Exception: {Message}", ex.InnerException.Message);
            if (ex.InnerException.StackTrace != null)
            {
                _logger.LogError(ex, "Inner Stack Trace: {StackTrace}", ex.InnerException.StackTrace);
            }
        }
        if (ex.StackTrace != null)
        {
            _logger.LogError(ex, "Stack Trace: {StackTrace}", ex.StackTrace);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var result = new
        {
            error = "An unexpected error occurred.",
            details = ex.Message
        }.ToString();

        return context.Response.WriteAsync(result ?? "");
    }
}

// Extension method used to add the middleware to the HTTP request pipeline.
public static class ExceptionsMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionsMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionMiddleware>();
    }
}
