using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Platform.Application.Contracts;
using Platform.Core.Models;
using Platform.DataAccess.Postgress;

namespace Platform.Application.Tests;

public sealed class StudentApiTests(StudentApiFactory factory)
    : IClassFixture<StudentApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    [Fact]
    public async Task HealthReady_AvailableDatabase_ReturnsReady()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("ready", document.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Login_ExistingStudent_ReturnsStudentRoleAndSessionCookie()
    {
        using var client = CreateClient();

        var response = await Login(client, StudentApiFactory.StudentId);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<AuthenticatedUserDto>(JsonOptions);
        Assert.Equal(StudentApiFactory.StudentId, user!.Id);
        Assert.Equal("Иванов Иван", user.DisplayName);
        Assert.Equal(UserRole.Student, user.Role);
        Assert.Equal("ИВТ-101", user.Group);
        Assert.Contains(
            response.Headers.GetValues("Set-Cookie"),
            value =>
                value.Contains("Platform.Auth=", StringComparison.Ordinal) &&
                !value.Contains("expires=", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("b1000000-0000-0000-0000-000000000001", UserRole.Teacher)]
    [InlineData("b2000000-0000-0000-0000-000000000001", UserRole.Administrator)]
    public async Task Login_ConfiguredPrivilegedUser_ReturnsServerAssignedRole(
        string userId,
        UserRole expectedRole)
    {
        using var client = CreateClient();

        var response = await Login(client, Guid.Parse(userId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<AuthenticatedUserDto>(JsonOptions);
        Assert.Equal(expectedRole, user!.Role);
        Assert.Null(user.Group);
    }

    [Fact]
    public async Task Login_UnknownStudent_ReturnsBasicJsonError()
    {
        using var client = CreateClient();

        var response = await Login(client, Guid.NewGuid());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorDto>(JsonOptions);
        Assert.Equal("invalid_credentials", error!.Code);
    }

    [Fact]
    public async Task Login_EmptyStudentId_ReturnsValidationError()
    {
        using var client = CreateClient();

        var response = await Login(client, Guid.Empty);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorDto>(JsonOptions);
        Assert.Equal("invalid_user_id", error!.Code);
        Assert.NotEmpty(error.Message);
    }

    [Fact]
    public async Task Login_WithoutAntiforgeryToken_ReturnsBadRequest()
    {
        using var client = CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new GuidLoginRequest(StudentApiFactory.StudentId),
            JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task StudentEndpoint_TeacherRole_ReturnsForbidden()
    {
        using var client = CreateClient();
        await Login(client, StudentApiFactory.TeacherId);

        var response = await client.GetAsync("/api/student/courses");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorDto>(JsonOptions);
        Assert.Equal("access_denied", error!.Code);
    }

    [Fact]
    public async Task StaffCourses_WithoutAuthentication_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/api/staff/courses");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorDto>(JsonOptions);
        Assert.Equal("authentication_required", error!.Code);
    }

    [Fact]
    public async Task StaffCourses_StudentRole_ReturnsForbidden()
    {
        using var client = CreateClient();
        await Login(client, StudentApiFactory.StudentId);

        var response = await client.GetAsync("/api/staff/courses");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorDto>(JsonOptions);
        Assert.Equal("access_denied", error!.Code);
    }

    [Fact]
    public async Task StaffCourses_Teacher_ReturnsOnlyActivelyAssignedCourse()
    {
        using var client = CreateClient();
        await Login(client, StudentApiFactory.TeacherId);

        var courses = await client.GetFromJsonAsync<List<CourseDto>>(
            "/api/staff/courses",
            JsonOptions);

        var course = Assert.Single(courses!);
        Assert.Equal(StudentApiFactory.CourseId, course.Id);
        Assert.Equal(2026, course.Year);
    }

    [Fact]
    public async Task StaffCourse_TeacherAssignedCourse_ReturnsCourse()
    {
        using var client = CreateClient();
        await Login(client, StudentApiFactory.TeacherId);

        var response = await client.GetAsync(
            $"/api/staff/courses/{StudentApiFactory.CourseId}/2026");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var course = await response.Content.ReadFromJsonAsync<CourseDto>(JsonOptions);
        Assert.Equal(StudentApiFactory.CourseId, course!.Id);
    }

    [Fact]
    public async Task StaffCourse_TeacherForeignCourse_ReturnsForbidden()
    {
        using var client = CreateClient();
        await Login(client, StudentApiFactory.TeacherId);

        var response = await client.GetAsync(
            $"/api/staff/courses/{StudentApiFactory.OtherCourseId}/2026");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorDto>(JsonOptions);
        Assert.Equal("course_access_denied", error!.Code);
    }

    [Fact]
    public async Task StaffCourses_Administrator_ReturnsEveryCourse()
    {
        using var client = CreateClient();
        await Login(client, StudentApiFactory.AdministratorId);

        var courses = await client.GetFromJsonAsync<List<CourseDto>>(
            "/api/staff/courses",
            JsonOptions);

        Assert.Equal(3, courses!.Count);
        Assert.Contains(courses, course => course.Id == StudentApiFactory.CourseId);
        Assert.Contains(courses, course => course.Id == StudentApiFactory.OtherCourseId);
        Assert.Contains(courses, course => course.Id == StudentApiFactory.AdditionalCourseId);
    }

    [Fact]
    public async Task StaffCourse_AdministratorCanOpenUnassignedCourse()
    {
        using var client = CreateClient();
        await Login(client, StudentApiFactory.AdministratorId);

        var response = await client.GetAsync(
            $"/api/staff/courses/{StudentApiFactory.OtherCourseId}/2026");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var course = await response.Content.ReadFromJsonAsync<CourseDto>(JsonOptions);
        Assert.Equal(StudentApiFactory.OtherCourseId, course!.Id);
    }

    [Fact]
    public async Task Session_AuthenticatedUser_ReturnsLifecycleMetadata()
    {
        using var client = CreateClient();
        await Login(client, StudentApiFactory.AdministratorId);

        var session = await client.GetFromJsonAsync<AuthenticatedSessionDto>(
            "/api/auth/session",
            JsonOptions);

        Assert.Equal(StudentApiFactory.AdministratorId, session!.User.Id);
        Assert.Equal(UserRole.Administrator, session.User.Role);
        Assert.NotEqual(Guid.Empty, session.SessionId);
        Assert.NotNull(session.IssuedUtc);
        Assert.NotNull(session.ExpiresUtc);
        Assert.True(session.ExpiresUtc > session.IssuedUtc);
    }

    [Fact]
    public async Task Logout_WithAntiforgeryToken_InvalidatesServerSession()
    {
        using var client = CreateClient();
        await Login(client, StudentApiFactory.StudentId);

        var logout = await client.PostAsync("/api/auth/logout", content: null);
        var me = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, me.StatusCode);
    }

    [Fact]
    public async Task Courses_WithoutAuthentication_ReturnsUnauthorizedJson()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/api/student/courses");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorDto>(JsonOptions);
        Assert.Equal("authentication_required", error!.Code);
    }

    [Fact]
    public async Task Courses_AuthenticatedStudent_ReturnsOnlyOwnActiveCourses()
    {
        using var client = CreateClient();
        await Login(client, StudentApiFactory.StudentId);

        var courses = await client.GetFromJsonAsync<List<CourseDto>>(
            "/api/student/courses",
            JsonOptions);

        Assert.Equal(2, courses!.Count);
        var course = Assert.Single(courses.Where(
            course => course.Id == StudentApiFactory.CourseId));
        Assert.Equal(StudentApiFactory.CourseId, course.Id);
        Assert.Equal("Алгоритмы", course.Title);
        Assert.Equal("Основной тестовый курс", course.Description);
        Assert.Equal(2026, course.Year);

        var additionalCourse = Assert.Single(courses.Where(
            course => course.Id == StudentApiFactory.AdditionalCourseId));
        Assert.Equal("Дискретная математика", additionalCourse.Title);
        Assert.Equal(2026, additionalCourse.Year);
    }

    [Fact]
    public async Task Statistics_AllCurrentCourses_AggregatesRaritiesAndExcludesForeignCourse()
    {
        using var client = CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AchievementDbContext>();
            var foreignAchievement = new AchievementEntity
            {
                Id = Guid.NewGuid(),
                Title = "Чужое достижение",
                Description = "Не должно попасть в статистику текущих курсов",
                Rarity = AchievementRarity.Epic,
                CourseID = StudentApiFactory.OtherCourseId,
                Year = 2026
            };
            dbContext.Achievements.Add(foreignAchievement);
            dbContext.StudentAchievements.Add(new StudentAchievementEntity
            {
                Id = Guid.NewGuid(),
                StudentID = StudentApiFactory.StudentId,
                AchievementID = foreignAchievement.Id,
                Achievement = foreignAchievement,
                AchievementGotDate = new DateTime(2026, 5, 1, 10, 0, 0, DateTimeKind.Utc),
                AchievementFoundDate = new DateTime(2026, 5, 1, 10, 5, 0, DateTimeKind.Utc)
            });
            await dbContext.SaveChangesAsync();
        }
        await Login(client, StudentApiFactory.StudentId);

        var statistics = await client.GetFromJsonAsync<StudentStatisticsDto>(
            "/api/student/statistics",
            JsonOptions);

        Assert.NotNull(statistics);
        Assert.Equal(2, statistics.TotalAchievements);
        Assert.Collection(
            statistics.ByRarity,
            item =>
            {
                Assert.Equal(AchievementRarity.Common, item.Rarity);
                Assert.Equal(1, item.Count);
            },
            item =>
            {
                Assert.Equal(AchievementRarity.Rare, item.Rarity);
                Assert.Equal(1, item.Count);
            },
            item =>
            {
                Assert.Equal(AchievementRarity.Epic, item.Rarity);
                Assert.Equal(0, item.Count);
            },
            item =>
            {
                Assert.Equal(AchievementRarity.Legendary, item.Rarity);
                Assert.Equal(0, item.Count);
            });
    }

    [Fact]
    public async Task Statistics_SelectedCourse_ReturnsOnlyThatCourseAchievements()
    {
        using var client = CreateClient();
        await Login(client, StudentApiFactory.StudentId);

        var mainCourse = await client.GetFromJsonAsync<StudentStatisticsDto>(
            $"/api/student/courses/{StudentApiFactory.CourseId}/2026/statistics",
            JsonOptions);
        var additionalCourse = await client.GetFromJsonAsync<StudentStatisticsDto>(
            $"/api/student/courses/{StudentApiFactory.AdditionalCourseId}/2026/statistics",
            JsonOptions);

        Assert.NotNull(mainCourse);
        Assert.Equal(1, mainCourse.TotalAchievements);
        Assert.Equal(
            1,
            Assert.Single(mainCourse.ByRarity, item => item.Rarity == AchievementRarity.Common).Count);
        Assert.Equal(
            0,
            Assert.Single(mainCourse.ByRarity, item => item.Rarity == AchievementRarity.Rare).Count);

        Assert.NotNull(additionalCourse);
        Assert.Equal(1, additionalCourse.TotalAchievements);
        Assert.Equal(
            0,
            Assert.Single(
                additionalCourse.ByRarity,
                item => item.Rarity == AchievementRarity.Common).Count);
        Assert.Equal(
            1,
            Assert.Single(
                additionalCourse.ByRarity,
                item => item.Rarity == AchievementRarity.Rare).Count);
    }

    [Fact]
    public async Task Statistics_ForeignAndMissingCourse_UseCourseAccessRules()
    {
        using var client = CreateClient();
        await Login(client, StudentApiFactory.StudentId);

        var foreignCourse = await client.GetAsync(
            $"/api/student/courses/{StudentApiFactory.OtherCourseId}/2026/statistics");
        var missingCourse = await client.GetAsync(
            $"/api/student/courses/{Guid.NewGuid()}/2026/statistics");

        Assert.Equal(HttpStatusCode.Forbidden, foreignCourse.StatusCode);
        var foreignError = await foreignCourse.Content.ReadFromJsonAsync<ApiErrorDto>(JsonOptions);
        Assert.Equal("course_access_denied", foreignError!.Code);

        Assert.Equal(HttpStatusCode.NotFound, missingCourse.StatusCode);
        var missingError = await missingCourse.Content.ReadFromJsonAsync<ApiErrorDto>(JsonOptions);
        Assert.Equal("course_not_found", missingError!.Code);
    }

    [Fact]
    public async Task Statistics_AnonymousAndStaffUser_CannotReadStudentStatistics()
    {
        using var anonymousClient = CreateClient();
        var anonymousResponse = await anonymousClient.GetAsync("/api/student/statistics");

        using var teacherClient = CreateClient();
        await Login(teacherClient, StudentApiFactory.TeacherId);
        var teacherResponse = await teacherClient.GetAsync("/api/student/statistics");

        Assert.Equal(HttpStatusCode.Unauthorized, anonymousResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, teacherResponse.StatusCode);
    }

    [Fact]
    public async Task AchievementGraph_OwnCourse_ReturnsXmlWithResolvedStatuses()
    {
        using var client = CreateClient();
        await Login(client, StudentApiFactory.StudentId);

        var response = await client.GetAsync(
            $"/api/student/courses/{StudentApiFactory.CourseId}/2026/achievements/graph");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/xml", response.Content.Headers.ContentType!.MediaType);

        var xml = await response.Content.ReadAsStringAsync();
        var document = XDocument.Parse(xml);

        Assert.Equal("earned", GetNodeStatus(document, "earned"));
        Assert.Equal("available", GetNodeStatus(document, "available"));
        Assert.Equal("locked", GetNodeStatus(document, "not-from-db"));
        Assert.Equal("available", GetEdgeStatus(document, "edge-earned-available"));
    }

    [Fact]
    public async Task AchievementGraph_AdditionalCourse_UsesTemplateCriteriaMapping()
    {
        using var client = CreateClient();
        await Login(client, StudentApiFactory.StudentId);

        var response = await client.GetAsync(
            $"/api/student/courses/{StudentApiFactory.AdditionalCourseId}/2026/achievements/graph");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/xml", response.Content.Headers.ContentType!.MediaType);

        var xml = await response.Content.ReadAsStringAsync();
        var document = XDocument.Parse(xml);

        Assert.Equal("earned", GetNodeStatus(document, "earned"));
        Assert.Equal("available", GetNodeStatus(document, "available"));
        Assert.Equal("locked", GetNodeStatus(document, "not-from-db"));
        Assert.Equal("available", GetEdgeStatus(document, "edge-earned-available"));
    }

    [Fact]
    public async Task AchievementGraph_ForeignCourse_ReturnsForbidden()
    {
        using var client = CreateClient();
        await Login(client, StudentApiFactory.StudentId);

        var response = await client.GetAsync(
            $"/api/student/courses/{StudentApiFactory.OtherCourseId}/2026/achievements/graph");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorDto>(JsonOptions);
        Assert.Equal("course_access_denied", error!.Code);
    }

    [Fact]
    public async Task Leaderboard_OwnCourse_ReturnsCourseStudentsOrderedByAchievementCount()
    {
        using var client = CreateClient();
        await Login(client, StudentApiFactory.StudentId);

        var entries = await client.GetFromJsonAsync<List<LeaderboardEntryDto>>(
            $"/api/student/courses/{StudentApiFactory.CourseId}/2026/leaderboard",
            JsonOptions);

        Assert.Collection(
            entries!,
            entry =>
            {
                Assert.Equal(StudentApiFactory.CoursePeerStudentId, entry.StudentId);
                Assert.Equal("Мария Сидорова", entry.StudentName);
                Assert.Equal("ИВТ-101", entry.Group);
                Assert.Equal(2, entry.AchievementCount);
            },
            entry =>
            {
                Assert.Equal(StudentApiFactory.StudentId, entry.StudentId);
                Assert.Equal("Иван Иванов", entry.StudentName);
                Assert.Equal("ИВТ-101", entry.Group);
                Assert.Equal(1, entry.AchievementCount);
            },
            entry =>
            {
                Assert.Equal(
                    StudentApiFactory.CourseZeroAchievementStudentId,
                    entry.StudentId);
                Assert.Equal("Анна Смирнова", entry.StudentName);
                Assert.Equal("ИВТ-101", entry.Group);
                Assert.Equal(0, entry.AchievementCount);
            });
        Assert.DoesNotContain(entries!, entry =>
            entry.StudentId == StudentApiFactory.OtherStudentId);
    }

    [Fact]
    public async Task Leaderboard_AdditionalCourse_ReturnsSelectedCourseStudentsWithIndependentCounts()
    {
        using var client = CreateClient();
        await Login(client, StudentApiFactory.StudentId);

        var entries = await client.GetFromJsonAsync<List<LeaderboardEntryDto>>(
            $"/api/student/courses/{StudentApiFactory.AdditionalCourseId}/2026/leaderboard",
            JsonOptions);

        Assert.Collection(
            entries!,
            entry =>
            {
                Assert.Equal(
                    StudentApiFactory.AdditionalCourseLeaderStudentId,
                    entry.StudentId);
                Assert.Equal("Сергей Васильев", entry.StudentName);
                Assert.Equal("ИВТ-101", entry.Group);
                Assert.Equal(3, entry.AchievementCount);
            },
            entry =>
            {
                Assert.Equal(StudentApiFactory.StudentId, entry.StudentId);
                Assert.Equal("Иван Иванов", entry.StudentName);
                Assert.Equal("ИВТ-101", entry.Group);
                Assert.Equal(1, entry.AchievementCount);
            },
            entry =>
            {
                Assert.Equal(
                    StudentApiFactory.AdditionalCourseTrailingStudentId,
                    entry.StudentId);
                Assert.Equal("Ольга Кузнецова", entry.StudentName);
                Assert.Equal("ИВТ-101", entry.Group);
                Assert.Equal(0, entry.AchievementCount);
            });
        Assert.DoesNotContain(entries!, entry =>
            entry.StudentId == StudentApiFactory.CoursePeerStudentId);
        Assert.DoesNotContain(entries!, entry =>
            entry.StudentId == StudentApiFactory.OtherStudentId);
    }

    [Fact]
    public async Task Leaderboard_ForeignCourse_ReturnsForbidden()
    {
        using var client = CreateClient();
        await Login(client, StudentApiFactory.StudentId);

        var response = await client.GetAsync(
            $"/api/student/courses/{StudentApiFactory.OtherCourseId}/2026/leaderboard");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorDto>(JsonOptions);
        Assert.Equal("course_access_denied", error!.Code);
    }

    [Fact]
    public async Task Leaderboard_MissingCourse_ReturnsNotFound()
    {
        using var client = CreateClient();
        await Login(client, StudentApiFactory.StudentId);

        var missingCourseId = Guid.Parse("a1000000-0000-0000-0000-000000000999");
        var response = await client.GetAsync(
            $"/api/student/courses/{missingCourseId}/2026/leaderboard");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorDto>(JsonOptions);
        Assert.Equal("course_not_found", error!.Code);
    }

    [Fact]
    public async Task Leaderboard_WithoutAuthentication_ReturnsUnauthorizedJson()
    {
        using var client = CreateClient();

        var response = await client.GetAsync(
            $"/api/student/courses/{StudentApiFactory.CourseId}/2026/leaderboard");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorDto>(JsonOptions);
        Assert.Equal("authentication_required", error!.Code);
    }

    [Fact]
    public async Task AchievementGraphRefresh_WithoutAuthentication_ReturnsUnauthorizedJson()
    {
        using var client = CreateClient();
        await SetCsrfToken(client);

        var response = await client.PostAsync(
            $"/api/student/courses/{StudentApiFactory.CourseId}/2026/achievements/graph/refresh",
            content: null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorDto>(JsonOptions);
        Assert.Equal("authentication_required", error!.Code);
        Assert.NotEmpty(error.Message);
    }

    [Fact]
    public async Task AchievementGraphRefresh_OwnCourse_RunsProcessingCycleAndReturnsUpdatedXml()
    {
        using var client = CreateClient();
        await Login(client, StudentApiFactory.StudentId);

        var beforeRefresh = await client.GetAsync(
            $"/api/student/courses/{StudentApiFactory.CourseId}/2026/achievements/graph");
        var beforeXml = XDocument.Parse(await beforeRefresh.Content.ReadAsStringAsync());
        Assert.Equal("available", GetNodeStatus(beforeXml, "available"));

        var response = await client.PostAsync(
            $"/api/student/courses/{StudentApiFactory.CourseId}/2026/achievements/graph/refresh",
            content: null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/xml", response.Content.Headers.ContentType!.MediaType);

        var xml = await response.Content.ReadAsStringAsync();
        var document = XDocument.Parse(xml);

        Assert.Equal("earned", GetNodeStatus(document, "available"));
        Assert.Equal("earned", GetEdgeStatus(document, "edge-earned-available"));

        var repeatedResponse = await client.PostAsync(
            $"/api/student/courses/{StudentApiFactory.CourseId}/2026/achievements/graph/refresh",
            content: null);
        Assert.Equal(HttpStatusCode.OK, repeatedResponse.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AchievementDbContext>();
        var assignedCount = await dbContext.StudentAchievements.CountAsync(item =>
            item.StudentID == StudentApiFactory.StudentId &&
            item.AchievementID == StudentApiFactory.LockedAchievementId);

        Assert.Equal(1, assignedCount);
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

    private static async Task<HttpResponseMessage> Login(HttpClient client, Guid userId)
    {
        await SetCsrfToken(client);
        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new GuidLoginRequest(userId),
            JsonOptions);
        if (response.IsSuccessStatusCode)
            await SetCsrfToken(client);
        return response;
    }

    private static async Task SetCsrfToken(HttpClient client)
    {
        var csrf = await client.GetFromJsonAsync<CsrfTokenDto>(
            "/api/auth/csrf",
            JsonOptions);
        client.DefaultRequestHeaders.Remove("X-CSRF-TOKEN");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", csrf!.Token);
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        return options;
    }

    private static string GetNodeStatus(XDocument document, string nodeId)
    {
        return document
            .Root!
            .Elements("node")
            .Single(node => node.Attribute("id")?.Value == nodeId)
            .Element("status")!
            .Attribute("state")!
            .Value;
    }

    private static string GetEdgeStatus(XDocument document, string edgeId)
    {
        return document
            .Root!
            .Elements("edge")
            .Single(edge => edge.Attribute("id")?.Value == edgeId)
            .Element("status")!
            .Attribute("state")!
            .Value;
    }
}
