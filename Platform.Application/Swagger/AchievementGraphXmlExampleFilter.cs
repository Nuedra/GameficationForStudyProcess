using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Platform.Application.Swagger;

public sealed class AchievementGraphXmlExampleFilter : IOperationFilter
{
    private const string AchievementGraphPathSuffix = "achievements/graph";
    private const string AchievementGraphRefreshPathSuffix = "achievements/graph/refresh";

    private const string ExampleXml =
        """
        <graph>
          <node id="node-1" AchivementId="55555555-5555-5555-5555-555555555555" label="Первая ачивка">
            <geometry x="0" y="0"/>
            <status state="earned"/>
          </node>
          <node id="node-2" AchivementId="66666666-6666-6666-6666-666666666666" label="Следующая ачивка">
            <geometry x="1" y="0"/>
            <status state="available"/>
          </node>
          <node id="node-3" AchivementId="77777777-7777-7777-7777-777777777777" label="Будущая ачивка">
            <geometry x="2" y="0"/>
            <status state="locked"/>
          </node>
          <edge id="edge-1" source="node-1" target="node-2">
            <status state="available"/>
          </edge>
        </graph>
        """;

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.ApiDescription.RelativePath is null ||
            !IsAchievementGraphPath(context.ApiDescription.RelativePath))
        {
            return;
        }

        if (!operation.Responses.TryGetValue(StatusCodes.Status200OK.ToString(), out var response) ||
            !response.Content.TryGetValue("application/xml", out var content))
        {
            return;
        }

        content.Schema = new OpenApiSchema
        {
            Type = "string",
            Description = "XML-граф достижений со статусами earned, available и locked."
        };
        content.Example = new OpenApiString(ExampleXml);
    }

    private static bool IsAchievementGraphPath(string relativePath)
    {
        return relativePath.EndsWith(
                   AchievementGraphPathSuffix,
                   StringComparison.OrdinalIgnoreCase) ||
               relativePath.EndsWith(
                   AchievementGraphRefreshPathSuffix,
                   StringComparison.OrdinalIgnoreCase);
    }
}
