using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using Platform.Application.Contracts;

namespace Platform.Application.Tests;

public sealed class StudentApiTests(StudentApiFactory factory)
    : IClassFixture<StudentApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    [Fact]
    public async Task Login_ExistingStudent_ReturnsStudentAndPersistentCookie()
    {
        using var client = CreateClient();

        var response = await Login(client, StudentApiFactory.StudentId);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var student = await response.Content.ReadFromJsonAsync<StudentDto>(JsonOptions);
        Assert.Equal(StudentApiFactory.StudentId, student!.Id);
        Assert.Contains(
            response.Headers.GetValues("Set-Cookie"),
            value =>
                value.Contains("Platform.Student=", StringComparison.Ordinal) &&
                value.Contains("expires=", StringComparison.OrdinalIgnoreCase));
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

        var course = Assert.Single(courses!);
        Assert.Equal(StudentApiFactory.CourseId, course.Id);
        Assert.Equal(2026, course.Year);
    }

    [Fact]
    public async Task Achievements_OwnCourse_ReturnsOnlyEarnedAchievements()
    {
        using var client = CreateClient();
        await Login(client, StudentApiFactory.StudentId);

        var response = await client.GetAsync(
            $"/api/student/courses/{StudentApiFactory.CourseId}/2026/achievements");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<StudentAchievementsDto>(
            JsonOptions);
        var achievement = Assert.Single(result!.Achievements);
        Assert.Equal(StudentApiFactory.EarnedAchievementId, achievement.Id);
        Assert.Equal(AchievementStatus.Earned, achievement.Status);
        Assert.DoesNotContain(
            result.Achievements,
            item => item.Id == StudentApiFactory.LockedAchievementId);

        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"status\":\"earned\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Achievements_ForeignCourse_ReturnsForbidden()
    {
        using var client = CreateClient();
        await Login(client, StudentApiFactory.StudentId);

        var response = await client.GetAsync(
            $"/api/student/courses/{StudentApiFactory.OtherCourseId}/2026/achievements");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorDto>(JsonOptions);
        Assert.Equal("course_access_denied", error!.Code);
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

    private HttpClient CreateClient()
    {
        return factory.CreateClient(new()
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
            HandleCookies = true
        });
    }

    private static Task<HttpResponseMessage> Login(HttpClient client, Guid studentId)
    {
        return client.PostAsJsonAsync(
            "/api/auth/student/login",
            new StudentLoginRequest(studentId),
            JsonOptions);
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
