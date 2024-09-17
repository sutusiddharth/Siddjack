using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ServiceDefaults.Extensions;
public static class ConfigureSwagger
{
    public static void AddCustomSwagger(this WebApplicationBuilder builder)
    {
        builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(options =>
        {
            options.EnableAnnotations();
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey
            });
            options.OperationFilter<SecurityRequirementsOperationFilter>();
        });
    }

    public static void UseCustomSwagger(this IApplicationBuilder app, IApiVersionDescriptionProvider apiVersionDescriptionProvider)
    {
        app.UseStaticFiles();
        app.UseSwagger();


        app.UseSwaggerUI(options =>
        {
            foreach (var GroupName in apiVersionDescriptionProvider.ApiVersionDescriptions.Select(a => a.GroupName))
            {
                options.SwaggerEndpoint($"/swagger/{GroupName}/swagger.json",

                GroupName.ToUpperInvariant());
            }
            options.InjectStylesheet("/swagger-ui/custom.css");
            options.InjectJavascript("/swagger-ui/custom.js", "text/javascript");
        });
    }
}
public class SecurityRequirementsOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (!context.MethodInfo.GetCustomAttributes(true).Any(x => x is AllowAnonymousAttribute))
        {
            operation.Security =
            [
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                }
            ];
        }
        else
        {
            operation.Responses.Remove("401");
        }
    }
}