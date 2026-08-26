using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Platform.Application.Swagger;

public sealed class AntiforgeryHeaderOperationFilter : IOperationFilter
{
    private static readonly HashSet<string> ProtectedMethods =
        new(StringComparer.OrdinalIgnoreCase)
        {
            HttpMethods.Post,
            HttpMethods.Put,
            HttpMethods.Patch,
            HttpMethods.Delete
        };

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var httpMethod = context.ApiDescription.HttpMethod;
        if (httpMethod is null || !ProtectedMethods.Contains(httpMethod))
            return;

        operation.Parameters ??= [];
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-CSRF-TOKEN",
            In = ParameterLocation.Header,
            Required = true,
            Description = "Request token из GET /api/auth/csrf.",
            Schema = new OpenApiSchema { Type = "string" }
        });
    }
}
