using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace ServiceDefaults.Extensions;

[AttributeUsage(
    AttributeTargets.Class |
    AttributeTargets.Struct |
    AttributeTargets.Parameter |
    AttributeTargets.Property |
    AttributeTargets.Enum,
    AllowMultiple = false)]
public class SwaggerSchemaAttribute(string example) : Attribute
{
    public string Example { get; set; } = example;
}

public class SwaggerSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.MemberInfo != null)
        {
            var schemaAttribute = context.MemberInfo.GetCustomAttributes<SwaggerSchemaAttribute>()
           .FirstOrDefault();
            if (schemaAttribute != null)
                ApplySchemaAttribute(schema, schemaAttribute);
        }
    }

    private static void ApplySchemaAttribute(OpenApiSchema schema, SwaggerSchemaAttribute schemaAttribute)
    {
        if (schemaAttribute.Example != null)
        {
            schema.Example = new Microsoft.OpenApi.Any.OpenApiString(schemaAttribute.Example);
        }
    }
}