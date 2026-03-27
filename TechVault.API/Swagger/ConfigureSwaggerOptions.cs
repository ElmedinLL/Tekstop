using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TechVault.API.Swagger;

public sealed class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(
                description.GroupName,
                new OpenApiInfo
                {
                    Title = "TechVault API",
                    Version = description.GroupName,
                    Description =
                        "REST API for TechVault. All endpoints are under `/api/v{version}/...`. "
                        + (description.IsDeprecated ? "This API version is deprecated." : string.Empty)
                });
        }
    }
}
