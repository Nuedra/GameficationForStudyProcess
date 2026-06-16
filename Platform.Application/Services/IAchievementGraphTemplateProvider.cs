namespace Platform.Application.Services;

public interface IAchievementGraphTemplateProvider
{
    Task<string> GetTemplateAsync(CancellationToken cancellationToken = default);
}
