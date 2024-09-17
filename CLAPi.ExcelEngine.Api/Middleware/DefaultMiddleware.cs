namespace CLAPi.ExcelEngine.Middleware;

// You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
public class DefaultMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public Task Invoke(HttpContext httpContext)
    {
        httpContext.Response.Headers.Append("Access-Control-Allow-Methods", "POST,GET,PUT,PATCH,DELETE,OPTIONS");
        return _next(httpContext);
    }
}

// Extension method used to add the middleware to the HTTP request pipeline.
public static class ConfigurationMiddlewareExtensions
{
    public static IApplicationBuilder UseConfigurationMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<DefaultMiddleware>();
    }
}