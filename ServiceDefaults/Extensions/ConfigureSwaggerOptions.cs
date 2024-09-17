using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ServiceDefaults.Extensions;
/// <summary>
/// Configures the Swagger generation options.
/// </summary>
/// <remarks>This allows API versioning to define a Swagger document per API version after the
/// <see cref="IApiVersionDescriptionProvider"/> service has been resolved from the service container.</remarks>
public class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider, IConfiguration configuration) : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider = provider;
    private readonly IConfiguration _configuration = configuration;

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _provider.ApiVersionDescriptions.Select(a => new { a.GroupName, a.ApiVersion, a.IsDeprecated }))
        {
            options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description.ApiVersion.ToString(), description.IsDeprecated));
        }
    }

    private OpenApiInfo CreateInfoForApiVersion(string Version, bool IsDeprecated)
    {
        var swaggerConfig = _configuration.GetSection("OpenApi");

        var info = new OpenApiInfo()
        {
            Title = swaggerConfig["Title"] ?? "Default API Title",
            Version = Version,
            Description = swaggerConfig["Description"] ?? "Default API Description",
            Contact = new OpenApiContact()
            {
                Name = swaggerConfig["ContactName"] ?? "Default Contact Name",
                Email = swaggerConfig["ContactEmail"] ?? "Default Contact Email"
            }
        };

        if (IsDeprecated)
        {
            info.Description += " [This API version has been deprecated]";
        }

        return info;
    }
}
