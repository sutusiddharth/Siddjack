using ServiceDefaults.ExceptionHandlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ServiceDefaults.Extensions;
public static class ExceptionHandlerExtension
{
    public static void AddGlobalExceptionHandler(this WebApplicationBuilder builder)
    {
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        ////builder.Services.AddExceptionHandler<BadRequestExceptionHandler>();
        ////builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
        builder.Services.AddProblemDetails();
    }
    public static void UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler();
    }
}
