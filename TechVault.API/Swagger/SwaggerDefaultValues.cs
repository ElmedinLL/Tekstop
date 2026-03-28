using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TechVault.API.Swagger;

/// <summary>Marks deprecated operations in OpenAPI when an API version is sunset.</summary>
public sealed class SwaggerDefaultValues : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context) =>
        operation.Deprecated |= context.ApiDescription.IsDeprecated();
}
