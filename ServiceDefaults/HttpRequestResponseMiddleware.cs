using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
namespace ServiceDefaults.Middleware
{
    public class HttpRequestResponseMiddleware(RequestDelegate next, ILogger<HttpRequestResponseMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<HttpRequestResponseMiddleware> _logger = logger;

        public async Task InvokeAsync(HttpContext context)
        {

            _logger.LogInformation("HTTP request: {Request}", context.Request.Path.Value);

            // Skip logging for requests to static files
            if (IsRequestForStaticFile(context.Request.Path))
            {
                await _next(context);
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            await _next(context);

            stopwatch.Stop();
            var responseTime = stopwatch.ElapsedMilliseconds;
            _logger.LogInformation("HTTP response time: {Time}", responseTime);

        }

        private static bool IsRequestForStaticFile(PathString path)
        {
            // Define patterns or conditions to identify static file requests
            var extensions = new List<string> { ".js", ".css", ".png", ".jpg", ".jpeg", ".gif", ".ico" };
            var pathString = path.ToString();
            return extensions.Exists(ext => pathString.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
        }
    }

    public static class HttpRequestResponseMiddlewareExtensions
    {
        public static IApplicationBuilder UseHttpRequestResponseMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<HttpRequestResponseMiddleware>();
        }
    }
}
