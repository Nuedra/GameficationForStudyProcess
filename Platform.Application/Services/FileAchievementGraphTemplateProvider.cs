namespace Platform.Application.Services;

public sealed class FileAchievementGraphTemplateProvider(
    IWebHostEnvironment environment,
    IConfiguration configuration) : IAchievementGraphTemplateProvider
{
    public async Task<string> GetTemplateAsync(CancellationToken cancellationToken = default)
    {
        var configuredPath = configuration["AchievementGraph:TemplatePath"];
        var path = string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine("Templates", "achievement-graph.xml")
            : configuredPath;
        var fullPath = Path.IsPathRooted(path)
            ? path
            : Path.Combine(environment.ContentRootPath, path);

        return await File.ReadAllTextAsync(fullPath, cancellationToken);
    }
}
