using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ServiceDefaults.Extensions;

/// <summary>
/// Configure the API versioning properties of the project.
/// </summary>
public static class ConfigureApiVersioning
{
    /// <summary>
    /// Configure the API versioning properties of the project, such as return headers, version format, etc.
    /// </summary>
    /// <param name="services"></param>
    public static void AddApiVersioningConfigured(this WebApplicationBuilder builder)
    {
        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(5, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = ApiVersionReader.Combine(new UrlSegmentApiVersionReader(),
                                                            new HeaderApiVersionReader("x-api-version"),
                                                            new MediaTypeApiVersionReader("x-api-version"));

            ////options.Policies.Sunset(0.9)
            ////                            .Effective(DateTimeOffset.Now.AddDays(60))
            ////                            .Link("policy.html")
            ////                                .Title("Versioning Policy")
            ////                                .Type("text/html");

        })
            .AddMvc()
            .AddApiExplorer(
            options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });
    }
}
