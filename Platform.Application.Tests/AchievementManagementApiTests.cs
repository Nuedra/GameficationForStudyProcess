using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Platform.Application.Contracts;
using Platform.DataAccess.Postgress;

namespace Platform.Application.Tests;

public sealed class AchievementManagementApiTests(StudentApiFactory factory)
    : IClassFixture<StudentApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    [Fact]
    public async Task AssignedTeacher_CanReadOwnAchievementsButNotForeignCourse()
    {
        using var client = CreateClient();
        await Login(client, StudentApiFactory.TeacherId);

        var own = await client.GetAsync(AchievementsUrl(StudentApiFactory.CourseId));
        var foreign = await client.GetAsync(AchievementsUrl(StudentApiFactory.OtherCourseId));

        Assert.Equal(HttpStatusCode.OK, own.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, foreign.StatusCode);
    }

    [Fact]
    public async Task AchievementAndCriteria_CanBeCreatedUpdatedAndDeleted()
    {
        using var client = CreateClient();
        await Login(client, StudentApiFactory.TeacherId);

        var create = await client.PostAsJsonAsync(
            AchievementsUrl(StudentApiFactory.CourseId),
            AchievementRequest("Новое достижение"),
            JsonOptions);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var achievement = await create.Content.ReadFromJsonAsync<ManagedAchievementDto>(JsonOptions);

        var update = await client.PutAsJsonAsync(
            $"{AchievementsUrl(StudentApiFactory.CourseId)}/{achievement!.Id}",
            AchievementRequest("Обновлённое достижение", "rare"),
            JsonOptions);
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);

        var criteria = await client.PutAsJsonAsync(
            $"{AchievementsUrl(StudentApiFactory.CourseId)}/{achievement.Id}/criteria",
            new { expression = "tag_one, tag_two", scope = "acrossCourse", isEnabled = true },
            JsonOptions);
        Assert.Equal(HttpStatusCode.OK, criteria.StatusCode);
        var withCriteria = await criteria.Content.ReadFromJsonAsync<ManagedAchievementDto>(JsonOptions);
        Assert.Equal("tag_one, tag_two", withCriteria!.Criteria!.Expression);

        var deleteCriteria = await client.DeleteAsync(
            $"{AchievementsUrl(StudentApiFactory.CourseId)}/{achievement.Id}/criteria");
        Assert.Equal(HttpStatusCode.OK, deleteCriteria.StatusCode);

        var delete = await client.DeleteAsync(
            $"{AchievementsUrl(StudentApiFactory.CourseId)}/{achievement.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
    }

    [Fact]
    public async Task DuplicateTitle_ReturnsConflict()
    {
        using var client = CreateClient();
        await Login(client, StudentApiFactory.AdministratorId);

        var response = await client.PostAsJsonAsync(
            AchievementsUrl(StudentApiFactory.CourseId),
            AchievementRequest("Первый коммит"),
            JsonOptions);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorDto>(JsonOptions);
        Assert.Equal("duplicate_achievement_title", error!.Code);
    }

    [Fact]
    public async Task EmptyCriteria_ReturnsBadRequest()
    {
        using var client = CreateClient();
        await Login(client, StudentApiFactory.TeacherId);

        var response = await client.PutAsJsonAsync(
            $"{AchievementsUrl(StudentApiFactory.CourseId)}/{StudentApiFactory.LockedAchievementId}/criteria",
            new { expression = " , ", scope = "sameMark", isEnabled = true },
            JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorDto>(JsonOptions);
        Assert.Equal("invalid_achievement_criteria", error!.Code);
    }

    [Fact]
    public async Task AwardedAchievement_RequiresExplicitRevocationConfirmation()
    {
        using var client = CreateClient();
        await Login(client, StudentApiFactory.AdministratorId);
        var create = await client.PostAsJsonAsync(
            AchievementsUrl(StudentApiFactory.CourseId),
            AchievementRequest("Выданное тестовое достижение"),
            JsonOptions);
        var achievement = await create.Content.ReadFromJsonAsync<ManagedAchievementDto>(JsonOptions);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AchievementDbContext>();
            db.StudentAchievements.Add(new StudentAchievementEntity
            {
                Id = Guid.NewGuid(),
                StudentID = StudentApiFactory.StudentId,
                AchievementID = achievement!.Id,
                AchievementGotDate = DateTime.UtcNow,
                AchievementFoundDate = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var withoutConfirmation = await client.DeleteAsync(
            $"{AchievementsUrl(StudentApiFactory.CourseId)}/{achievement!.Id}");
        var withConfirmation = await client.DeleteAsync(
            $"{AchievementsUrl(StudentApiFactory.CourseId)}/{achievement.Id}?revokeAwards=true");

        Assert.Equal(HttpStatusCode.Conflict, withoutConfirmation.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, withConfirmation.StatusCode);
        using var verificationScope = factory.Services.CreateScope();
        var verificationDb = verificationScope.ServiceProvider.GetRequiredService<AchievementDbContext>();
        Assert.False(await verificationDb.StudentAchievements.AnyAsync(
            item => item.AchievementID == achievement.Id));
    }

    [Fact]
    public async Task AchievementWithDependency_CannotBeDeletedUntilGraphIsChanged()
    {
        using var client = CreateClient();
        await Login(client, StudentApiFactory.AdministratorId);

        var response = await client.DeleteAsync(
            $"{AchievementsUrl(StudentApiFactory.CourseId)}/{StudentApiFactory.LockedAchievementId}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorDto>(JsonOptions);
        Assert.Equal("achievement_has_dependencies", error!.Code);
    }

    private HttpClient CreateClient()
    {
        factory.ResetDatabase();
        return factory.CreateClient(new()
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
            HandleCookies = true
        });
    }

    private static async Task Login(HttpClient client, Guid userId)
    {
        await SetCsrfToken(client);
        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new GuidLoginRequest(userId),
            JsonOptions);
        response.EnsureSuccessStatusCode();
        await SetCsrfToken(client);
    }

    private static async Task SetCsrfToken(HttpClient client)
    {
        var csrf = await client.GetFromJsonAsync<CsrfTokenDto>("/api/auth/csrf", JsonOptions);
        client.DefaultRequestHeaders.Remove("X-CSRF-TOKEN");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", csrf!.Token);
    }

    private static string AchievementsUrl(Guid courseId) =>
        $"/api/staff/courses/{courseId}/2026/achievements";

    private static object AchievementRequest(string title, string rarity = "common") => new
    {
        title,
        description = "Описание",
        rarity,
        track = "default",
        labId = (Guid?)null
    };

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        return options;
    }
}
