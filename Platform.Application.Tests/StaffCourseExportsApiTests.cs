using System.IO.Compression;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Platform.Application.Contracts;
using Platform.DataAccess.Postgress;

namespace Platform.Application.Tests;

public sealed class StaffCourseExportsApiTests(StudentApiFactory factory)
    : IClassFixture<StudentApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task AssignedTeacher_CanDownloadUtf8CsvReportForActiveCourseStudents()
    {
        using var client = CreateClient();
        await Login(client, StudentApiFactory.TeacherId);

        var response = await client.GetAsync(TeacherReportUrl(StudentApiFactory.CourseId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("utf-8", response.Content.Headers.ContentType?.CharSet);
        Assert.Equal(
            $"course-achievements-{StudentApiFactory.CourseId:D}-2026.csv",
            response.Content.Headers.ContentDisposition?.FileNameStar);

        var bytes = await response.Content.ReadAsByteArrayAsync();
        Assert.True(bytes.AsSpan().StartsWith(Encoding.UTF8.GetPreamble()));

        var csv = Encoding.UTF8.GetString(bytes);
        Assert.Contains("\"StudentId\";\"ФИО\";\"Группа\"", csv);
        Assert.Contains("Иван Иванов", csv);
        Assert.Contains("Мария Сидорова", csv);
        Assert.Contains("Анна Смирнова", csv);
        Assert.Contains("Первый коммит", csv);
        Assert.DoesNotContain("Пётр Петров", csv);
        Assert.DoesNotContain("Базы данных", csv);
    }

    [Fact]
    public async Task TeacherCsvReport_EscapesSpreadsheetFormulaValues()
    {
        using var client = CreateClient();
        var achievementId = Guid.NewGuid();
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AchievementDbContext>();
            dbContext.Achievements.Add(new AchievementEntity
            {
                Id = achievementId,
                Title = "=2+2",
                Description = "Проверка безопасного CSV",
                CourseID = StudentApiFactory.CourseId,
                Year = 2026
            });
            dbContext.StudentAchievements.Add(new StudentAchievementEntity
            {
                Id = Guid.NewGuid(),
                StudentID = StudentApiFactory.StudentId,
                AchievementID = achievementId,
                AchievementGotDate = DateTime.UtcNow,
                AchievementFoundDate = DateTime.UtcNow
            });
            await dbContext.SaveChangesAsync();
        }
        await Login(client, StudentApiFactory.TeacherId);

        var response = await client.GetAsync(TeacherReportUrl(StudentApiFactory.CourseId));

        var csv = Encoding.UTF8.GetString(await response.Content.ReadAsByteArrayAsync());
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("\"'=2+2\"", csv);
        Assert.DoesNotContain(";\"=2+2\";", csv);
    }

    [Fact]
    public async Task TeacherCsvReport_IsUnavailableToAdministratorAndForForeignCourse()
    {
        using var administratorClient = CreateClient();
        await Login(administratorClient, StudentApiFactory.AdministratorId);

        var administratorResponse = await administratorClient.GetAsync(
            TeacherReportUrl(StudentApiFactory.CourseId));

        using var teacherClient = CreateClient();
        await Login(teacherClient, StudentApiFactory.TeacherId);
        var foreignCourseResponse = await teacherClient.GetAsync(
            TeacherReportUrl(StudentApiFactory.OtherCourseId));

        Assert.Equal(HttpStatusCode.Forbidden, administratorResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, foreignCourseResponse.StatusCode);
    }

    [Fact]
    public async Task CourseArchive_ContainsDocumentedFilesAndValidChecksums()
    {
        using var client = CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AchievementDbContext>();
            dbContext.AchievementAwardAuditEvents.Add(new AchievementAwardAuditEventEntity
            {
                Id = Guid.NewGuid(),
                AwardID = dbContext.StudentAchievements
                    .Single(award =>
                        award.StudentID == StudentApiFactory.StudentId &&
                        award.AchievementID == StudentApiFactory.EarnedAchievementId)
                    .Id,
                EventType = AchievementAwardAuditEventType.Granted,
                OccurredAt = new DateTime(2026, 3, 1, 10, 5, 0, DateTimeKind.Utc),
                AwardedAt = new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc),
                StudentID = StudentApiFactory.StudentId,
                AchievementID = StudentApiFactory.EarnedAchievementId,
                AchievementTitle = "Первый коммит",
                CourseID = StudentApiFactory.CourseId,
                Year = 2026,
                ActorRole = AchievementAwardAuditActorRole.System,
                Reason = AchievementAwardAuditReason.CriteriaMatched
            });
            await dbContext.SaveChangesAsync();
        }
        await Login(client, StudentApiFactory.TeacherId);

        var response = await client.GetAsync(ArchiveUrl(StudentApiFactory.CourseId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/zip", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(
            $"course-export-{StudentApiFactory.CourseId:D}-2026.zip",
            response.Content.Headers.ContentDisposition?.FileNameStar);

        await using var archiveStream = new MemoryStream(
            await response.Content.ReadAsByteArrayAsync());
        using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Read);

        var expectedEntries = new[]
        {
            "README.txt",
            "achievements.json",
            "audit-events.jsonl",
            "awards.json",
            "connections.json",
            "graph.xml",
            "leaderboard.csv",
            "manifest.json",
            "students.json"
        };
        Assert.Equal(
            expectedEntries.OrderBy(name => name),
            archive.Entries.Select(entry => entry.FullName).OrderBy(name => name));

        using var manifest = JsonDocument.Parse(await ReadEntryAsync(archive, "manifest.json"));
        var root = manifest.RootElement;
        Assert.Equal("achievement-platform-course-export", root.GetProperty("format").GetString());
        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal(
            StudentApiFactory.CourseId,
            root.GetProperty("scope").GetProperty("courseId").GetGuid());
        Assert.Equal(2026, root.GetProperty("scope").GetProperty("year").GetInt32());
        Assert.Equal(
            "teacher",
            root.GetProperty("exportedBy").GetProperty("role").GetString());
        Assert.True(root.GetProperty("containsPersonalData").GetBoolean());

        foreach (var file in root.GetProperty("files").EnumerateArray())
        {
            var path = file.GetProperty("path").GetString()!;
            var content = await ReadEntryAsync(archive, path);
            var actualHash = Convert.ToHexString(SHA256.HashData(content)).ToLowerInvariant();
            Assert.Equal(file.GetProperty("sha256").GetString(), actualHash);
        }

        using var achievements = JsonDocument.Parse(
            await ReadEntryAsync(archive, "achievements.json"));
        Assert.Equal(3, achievements.RootElement.GetArrayLength());
        Assert.All(
            achievements.RootElement.EnumerateArray(),
            achievement => Assert.DoesNotContain(
                "дополнительного курса",
                achievement.GetProperty("description").GetString() ?? string.Empty,
                StringComparison.OrdinalIgnoreCase));

        using var students = JsonDocument.Parse(await ReadEntryAsync(archive, "students.json"));
        Assert.Equal(3, students.RootElement.GetArrayLength());

        var auditLines = Encoding.UTF8.GetString(
                await ReadEntryAsync(archive, "audit-events.jsonl"))
            .Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Single(auditLines);
        using var auditEvent = JsonDocument.Parse(auditLines[0]);
        Assert.Equal("granted", auditEvent.RootElement.GetProperty("eventType").GetString());

        var leaderboard = Encoding.UTF8.GetString(
            await ReadEntryAsync(archive, "leaderboard.csv"));
        Assert.True(
            leaderboard.IndexOf("Мария Сидорова", StringComparison.Ordinal) <
            leaderboard.IndexOf("Иван Иванов", StringComparison.Ordinal));

        var graph = Encoding.UTF8.GetString(await ReadEntryAsync(archive, "graph.xml"));
        var graphDocument = XDocument.Parse(graph);
        var graphRoot = Assert.IsType<XElement>(graphDocument.Root);
        var graphNodes = graphRoot.Elements("node").ToList();
        var graphAchievementIds = graphNodes
            .Select(node => Guid.Parse(node.Attribute("AchivementId")!.Value))
            .OrderBy(id => id)
            .ToList();
        Assert.Equal(
            new[]
            {
                StudentApiFactory.EarnedAchievementId,
                StudentApiFactory.LockedAchievementId
            }.OrderBy(id => id),
            graphAchievementIds);
        Assert.DoesNotContain(graphNodes, node => node.Attribute("id")?.Value == "not-from-db");

        var graphNodeIds = graphNodes
            .Select(node => node.Attribute("id")!.Value)
            .ToHashSet(StringComparer.Ordinal);
        Assert.All(
            graphRoot.Elements("edge"),
            edge =>
            {
                Assert.Contains(edge.Attribute("source")!.Value, graphNodeIds);
                Assert.Contains(edge.Attribute("target")!.Value, graphNodeIds);
            });
    }

    [Fact]
    public async Task CourseArchive_ResolvesEveryStudentReferencedByAwardsAndAudit()
    {
        using var client = CreateClient();
        var missingLmsStudentId = Guid.NewGuid();
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AchievementDbContext>();
            var achievement = dbContext.Achievements.Single(item =>
                item.Id == StudentApiFactory.EarnedAchievementId);
            dbContext.StudentAchievements.Add(new StudentAchievementEntity
            {
                Id = Guid.NewGuid(),
                StudentID = StudentApiFactory.OtherStudentId,
                AchievementID = achievement.Id,
                Achievement = achievement,
                AchievementGotDate = new DateTime(2025, 12, 1, 10, 0, 0, DateTimeKind.Utc),
                AchievementFoundDate = new DateTime(2025, 12, 1, 10, 5, 0, DateTimeKind.Utc)
            });
            dbContext.AchievementAwardAuditEvents.Add(new AchievementAwardAuditEventEntity
            {
                Id = Guid.NewGuid(),
                EventType = AchievementAwardAuditEventType.Rejected,
                OccurredAt = new DateTime(2026, 3, 2, 12, 0, 0, DateTimeKind.Utc),
                StudentID = missingLmsStudentId,
                AchievementID = achievement.Id,
                AchievementTitle = achievement.Title,
                CourseID = StudentApiFactory.CourseId,
                Year = 2026,
                ActorID = StudentApiFactory.TeacherId,
                ActorRole = AchievementAwardAuditActorRole.Teacher,
                Reason = AchievementAwardAuditReason.ManualGrantStudentNotFound
            });
            await dbContext.SaveChangesAsync();
        }
        await Login(client, StudentApiFactory.TeacherId);

        var response = await client.GetAsync(ArchiveUrl(StudentApiFactory.CourseId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await using var archiveStream = new MemoryStream(
            await response.Content.ReadAsByteArrayAsync());
        using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Read);
        using var students = JsonDocument.Parse(await ReadEntryAsync(archive, "students.json"));
        using var awards = JsonDocument.Parse(await ReadEntryAsync(archive, "awards.json"));

        var studentIds = students.RootElement
            .EnumerateArray()
            .Select(student => student.GetProperty("id").GetGuid())
            .ToHashSet();
        Assert.All(
            awards.RootElement.EnumerateArray(),
            award => Assert.Contains(award.GetProperty("studentId").GetGuid(), studentIds));

        var auditLines = Encoding.UTF8.GetString(
                await ReadEntryAsync(archive, "audit-events.jsonl"))
            .Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var auditLine in auditLines)
        {
            using var auditEvent = JsonDocument.Parse(auditLine);
            Assert.Contains(
                auditEvent.RootElement.GetProperty("studentId").GetGuid(),
                studentIds);
        }

        var formerStudent = students.RootElement
            .EnumerateArray()
            .Single(student => student.GetProperty("id").GetGuid() == StudentApiFactory.OtherStudentId);
        Assert.False(formerStudent.GetProperty("isActiveEnrollment").GetBoolean());
        Assert.True(formerStudent.GetProperty("isPresentInLms").GetBoolean());
        Assert.Equal("Петров Пётр", formerStudent.GetProperty("fullName").GetString());

        var missingStudent = students.RootElement
            .EnumerateArray()
            .Single(student => student.GetProperty("id").GetGuid() == missingLmsStudentId);
        Assert.False(missingStudent.GetProperty("isActiveEnrollment").GetBoolean());
        Assert.False(missingStudent.GetProperty("isPresentInLms").GetBoolean());
        Assert.Equal(JsonValueKind.Null, missingStudent.GetProperty("fullName").ValueKind);

        var leaderboard = Encoding.UTF8.GetString(
            await ReadEntryAsync(archive, "leaderboard.csv"));
        Assert.DoesNotContain("Петров Пётр", leaderboard);
    }

    [Fact]
    public async Task CourseArchive_UsesCourseAccessRulesForBothStaffRoles()
    {
        using var teacherClient = CreateClient();
        await Login(teacherClient, StudentApiFactory.TeacherId);
        var assignedCourse = await teacherClient.GetAsync(
            ArchiveUrl(StudentApiFactory.CourseId));
        var foreignCourse = await teacherClient.GetAsync(
            ArchiveUrl(StudentApiFactory.OtherCourseId));

        using var administratorClient = CreateClient();
        await Login(administratorClient, StudentApiFactory.AdministratorId);
        var administratorCourse = await administratorClient.GetAsync(
            ArchiveUrl(StudentApiFactory.OtherCourseId));
        var missingCourse = await administratorClient.GetAsync(
            ArchiveUrl(Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.OK, assignedCourse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, foreignCourse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, administratorCourse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missingCourse.StatusCode);
    }

    [Fact]
    public async Task StudentAndAnonymousUser_CannotDownloadStaffArchive()
    {
        using var anonymousClient = CreateClient();
        var anonymousResponse = await anonymousClient.GetAsync(
            ArchiveUrl(StudentApiFactory.CourseId));

        using var studentClient = CreateClient();
        await Login(studentClient, StudentApiFactory.StudentId);
        var studentResponse = await studentClient.GetAsync(
            ArchiveUrl(StudentApiFactory.CourseId));

        Assert.Equal(HttpStatusCode.Unauthorized, anonymousResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, studentResponse.StatusCode);
    }

    private HttpClient CreateClient()
    {
        factory.ResetDatabase();
        return factory.CreateClient(new WebApplicationFactoryClientOptions
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

    private static async Task<byte[]> ReadEntryAsync(ZipArchive archive, string path)
    {
        var entry = archive.GetEntry(path);
        Assert.NotNull(entry);
        await using var stream = entry.Open();
        await using var output = new MemoryStream();
        await stream.CopyToAsync(output);
        return output.ToArray();
    }

    private static string TeacherReportUrl(Guid courseId) =>
        $"/api/staff/courses/{courseId:D}/2026/exports/teacher-report.csv";

    private static string ArchiveUrl(Guid courseId) =>
        $"/api/staff/courses/{courseId:D}/2026/exports/archive.zip";
}
