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
        var auditEvent = Assert.Single(await verificationDb.AchievementAwardAuditEvents
            .Where(item => item.AchievementID == achievement.Id)
            .ToListAsync());
        Assert.Equal(AchievementAwardAuditEventType.Revoked, auditEvent.EventType);
        Assert.Equal(AchievementAwardAuditReason.AchievementDeletion, auditEvent.Reason);
        Assert.Equal(AchievementAwardAuditActorRole.Administrator, auditEvent.ActorRole);
        Assert.Equal(StudentApiFactory.AdministratorId, auditEvent.ActorID);
    }

    [Fact]
    public async Task Teacher_CanRevokeAwardAndReadPersistentAuditEvent()
    {
        using var client = CreateClient();
        await Login(client, StudentApiFactory.TeacherId);

        var revoke = await client.DeleteAsync(
            $"{AchievementsUrl(StudentApiFactory.CourseId)}/{StudentApiFactory.EarnedAchievementId}/awards/{StudentApiFactory.StudentId}");
        var audit = await client.GetAsync(
            $"{AchievementsUrl(StudentApiFactory.CourseId)}/audit?studentId={StudentApiFactory.StudentId}");

        Assert.Equal(HttpStatusCode.OK, revoke.StatusCode);
        Assert.Equal(HttpStatusCode.OK, audit.StatusCode);
        var events = await audit.Content.ReadFromJsonAsync<List<AchievementAwardAuditEventDto>>(
            JsonOptions);
        var auditEvent = Assert.Single(events!);
        Assert.Equal(AchievementAwardAuditEventType.Revoked, auditEvent.EventType);
        Assert.Equal(AchievementAwardAuditReason.ManualRevocation, auditEvent.Reason);
        Assert.Equal(AchievementAwardAuditActorRole.Teacher, auditEvent.ActorRole);
        Assert.Equal(StudentApiFactory.TeacherId, auditEvent.ActorId);
        Assert.Equal(StudentApiFactory.StudentId, auditEvent.StudentId);
        Assert.Equal(StudentApiFactory.EarnedAchievementId, auditEvent.AchievementId);

        using var verificationScope = factory.Services.CreateScope();
        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<AchievementDbContext>();
        Assert.False(await verificationDb.StudentAchievements.AnyAsync(item =>
            item.StudentID == StudentApiFactory.StudentId &&
            item.AchievementID == StudentApiFactory.EarnedAchievementId));
        Assert.True(await verificationDb.AchievementAwardAuditEvents.AnyAsync(item =>
            item.StudentID == StudentApiFactory.StudentId &&
            item.AchievementID == StudentApiFactory.EarnedAchievementId));
    }

    [Fact]
    public async Task Teacher_CanGrantAwardWhenGraphPrerequisiteIsUnlocked()
    {
        using var client = CreateClient();
        await Login(client, StudentApiFactory.TeacherId);

        var grant = await client.PostAsJsonAsync(
            $"{AchievementsUrl(StudentApiFactory.CourseId)}/{StudentApiFactory.LockedAchievementId}/awards",
            new ManualAchievementAwardRequest(StudentApiFactory.StudentId),
            JsonOptions);
        var audit = await client.GetAsync(
            $"{AchievementsUrl(StudentApiFactory.CourseId)}/audit?achievementId={StudentApiFactory.LockedAchievementId}&studentId={StudentApiFactory.StudentId}");

        Assert.Equal(HttpStatusCode.OK, grant.StatusCode);
        var achievement = await grant.Content.ReadFromJsonAsync<ManagedAchievementDto>(JsonOptions);
        Assert.True(achievement!.HasAwards);
        Assert.Equal(1, achievement.AwardCount);

        Assert.Equal(HttpStatusCode.OK, audit.StatusCode);
        var events = await audit.Content.ReadFromJsonAsync<List<AchievementAwardAuditEventDto>>(
            JsonOptions);
        var auditEvent = Assert.Single(events!);
        Assert.Equal(AchievementAwardAuditEventType.Granted, auditEvent.EventType);
        Assert.Equal(AchievementAwardAuditReason.ManualGrant, auditEvent.Reason);
        Assert.NotNull(auditEvent.AwardId);
        Assert.NotNull(auditEvent.AwardedAt);
        Assert.Equal(AchievementAwardAuditActorRole.Teacher, auditEvent.ActorRole);
        Assert.Equal(StudentApiFactory.TeacherId, auditEvent.ActorId);
        Assert.Equal(StudentApiFactory.StudentId, auditEvent.StudentId);
        Assert.Equal(StudentApiFactory.LockedAchievementId, auditEvent.AchievementId);

        using var verificationScope = factory.Services.CreateScope();
        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<AchievementDbContext>();
        Assert.True(await verificationDb.StudentAchievements.AnyAsync(item =>
            item.StudentID == StudentApiFactory.StudentId &&
            item.AchievementID == StudentApiFactory.LockedAchievementId));
    }

    [Fact]
    public async Task Administrator_CanGrantRootAward()
    {
        using var client = CreateClient();
        await Login(client, StudentApiFactory.AdministratorId);

        var grant = await client.PostAsJsonAsync(
            $"{AchievementsUrl(StudentApiFactory.CourseId)}/{StudentApiFactory.EarnedAchievementId}/awards",
            new ManualAchievementAwardRequest(StudentApiFactory.CourseZeroAchievementStudentId),
            JsonOptions);

        Assert.Equal(HttpStatusCode.OK, grant.StatusCode);
        var achievement = await grant.Content.ReadFromJsonAsync<ManagedAchievementDto>(JsonOptions);
        Assert.Equal(3, achievement!.AwardCount);

        using var verificationScope = factory.Services.CreateScope();
        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<AchievementDbContext>();
        var auditEvent = Assert.Single(await verificationDb.AchievementAwardAuditEvents
            .Where(item =>
                item.StudentID == StudentApiFactory.CourseZeroAchievementStudentId &&
                item.AchievementID == StudentApiFactory.EarnedAchievementId)
            .ToListAsync());
        Assert.Equal(AchievementAwardAuditEventType.Granted, auditEvent.EventType);
        Assert.Equal(AchievementAwardAuditReason.ManualGrant, auditEvent.Reason);
        Assert.Equal(AchievementAwardAuditActorRole.Administrator, auditEvent.ActorRole);
        Assert.Equal(StudentApiFactory.AdministratorId, auditEvent.ActorID);

        var cascadedAward = Assert.Single(await verificationDb.StudentAchievements
            .Where(item =>
                item.StudentID == StudentApiFactory.CourseZeroAchievementStudentId &&
                item.AchievementID == StudentApiFactory.LockedAchievementId)
            .ToListAsync());
        var cascadedAuditEvent = Assert.Single(await verificationDb.AchievementAwardAuditEvents
            .Where(item =>
                item.StudentID == StudentApiFactory.CourseZeroAchievementStudentId &&
                item.AchievementID == StudentApiFactory.LockedAchievementId)
            .ToListAsync());
        Assert.Equal(cascadedAward.Id, cascadedAuditEvent.AwardID!.Value);
        Assert.Equal(AchievementAwardAuditEventType.Granted, cascadedAuditEvent.EventType);
        Assert.Equal(AchievementAwardAuditReason.CriteriaMatched, cascadedAuditEvent.Reason);
        Assert.Equal(AchievementAwardAuditActorRole.System, cascadedAuditEvent.ActorRole);
        Assert.Null(cascadedAuditEvent.ActorID);
    }

    [Fact]
    public async Task ManualGrant_DependentAwardWithoutGraphPrerequisite_ReturnsConflict()
    {
        using var client = CreateClient();
        await Login(client, StudentApiFactory.TeacherId);

        var grant = await client.PostAsJsonAsync(
            $"{AchievementsUrl(StudentApiFactory.CourseId)}/{StudentApiFactory.LockedAchievementId}/awards",
            new ManualAchievementAwardRequest(StudentApiFactory.CourseZeroAchievementStudentId),
            JsonOptions);

        Assert.Equal(HttpStatusCode.Conflict, grant.StatusCode);
        var error = await grant.Content.ReadFromJsonAsync<ApiErrorDto>(JsonOptions);
        Assert.Equal("achievement_prerequisite_missing", error!.Code);

        using var verificationScope = factory.Services.CreateScope();
        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<AchievementDbContext>();
        Assert.False(await verificationDb.StudentAchievements.AnyAsync(item =>
            item.StudentID == StudentApiFactory.CourseZeroAchievementStudentId &&
            item.AchievementID == StudentApiFactory.LockedAchievementId));
        var auditEvent = Assert.Single(await verificationDb.AchievementAwardAuditEvents
            .Where(item =>
                item.StudentID == StudentApiFactory.CourseZeroAchievementStudentId &&
                item.AchievementID == StudentApiFactory.LockedAchievementId)
            .ToListAsync());
        AssertRejectedGrantAudit(
            auditEvent,
            AchievementAwardAuditReason.ManualGrantPrerequisiteMissing);
    }

    [Fact]
    public async Task ManualGrant_IgnoresCrossCourseGraphPrerequisite()
    {
        using var client = CreateClient();
        await Login(client, StudentApiFactory.TeacherId);

        var sourceId = Guid.Parse("00000000-0000-0000-0000-000000000201");
        var targetId = Guid.Parse("00000000-0000-0000-0000-000000000202");
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AchievementDbContext>();
            var foreignSource = new AchievementEntity
            {
                Id = sourceId,
                Title = "Чужой предшественник",
                Description = "Связь не принадлежит текущему графу",
                CourseID = StudentApiFactory.OtherCourseId,
                Year = 2026
            };
            var localTarget = new AchievementEntity
            {
                Id = targetId,
                Title = "Локальная ручная выдача",
                Description = "Достижение текущего курса",
                CourseID = StudentApiFactory.CourseId,
                Year = 2026
            };
            db.Achievements.AddRange(foreignSource, localTarget);
            db.AchievementConnections.Add(new AchievementConnectionEntity
            {
                Id = Guid.NewGuid(),
                SourceId = sourceId,
                Source = foreignSource,
                TargetId = targetId,
                Target = localTarget
            });
            await db.SaveChangesAsync();
        }

        var grant = await client.PostAsJsonAsync(
            $"{AchievementsUrl(StudentApiFactory.CourseId)}/{targetId}/awards",
            new ManualAchievementAwardRequest(StudentApiFactory.CourseZeroAchievementStudentId),
            JsonOptions);

        Assert.Equal(HttpStatusCode.OK, grant.StatusCode);
        using var verificationScope = factory.Services.CreateScope();
        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<AchievementDbContext>();
        Assert.True(await verificationDb.StudentAchievements.AnyAsync(item =>
            item.StudentID == StudentApiFactory.CourseZeroAchievementStudentId &&
            item.AchievementID == targetId));
    }

    [Fact]
    public async Task ManualGrant_AlreadyAwardedAchievement_ReturnsConflict()
    {
        using var client = CreateClient();
        await Login(client, StudentApiFactory.TeacherId);

        var grant = await client.PostAsJsonAsync(
            $"{AchievementsUrl(StudentApiFactory.CourseId)}/{StudentApiFactory.EarnedAchievementId}/awards",
            new ManualAchievementAwardRequest(StudentApiFactory.StudentId),
            JsonOptions);

        Assert.Equal(HttpStatusCode.Conflict, grant.StatusCode);
        var error = await grant.Content.ReadFromJsonAsync<ApiErrorDto>(JsonOptions);
        Assert.Equal("achievement_award_already_exists", error!.Code);

        using var verificationScope = factory.Services.CreateScope();
        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<AchievementDbContext>();
        var auditEvent = Assert.Single(await verificationDb.AchievementAwardAuditEvents
            .Where(item =>
                item.StudentID == StudentApiFactory.StudentId &&
                item.AchievementID == StudentApiFactory.EarnedAchievementId)
            .ToListAsync());
        AssertRejectedGrantAudit(
            auditEvent,
            AchievementAwardAuditReason.ManualGrantAlreadyExists);
    }

    [Fact]
    public async Task ManualGrant_StudentOutsideCourse_ReturnsConflict()
    {
        using var client = CreateClient();
        await Login(client, StudentApiFactory.TeacherId);

        var grant = await client.PostAsJsonAsync(
            $"{AchievementsUrl(StudentApiFactory.CourseId)}/{StudentApiFactory.EarnedAchievementId}/awards",
            new ManualAchievementAwardRequest(StudentApiFactory.OtherStudentId),
            JsonOptions);

        Assert.Equal(HttpStatusCode.Conflict, grant.StatusCode);
        var error = await grant.Content.ReadFromJsonAsync<ApiErrorDto>(JsonOptions);
        Assert.Equal("student_course_enrollment_required", error!.Code);

        using var verificationScope = factory.Services.CreateScope();
        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<AchievementDbContext>();
        var auditEvent = Assert.Single(await verificationDb.AchievementAwardAuditEvents
            .Where(item =>
                item.StudentID == StudentApiFactory.OtherStudentId &&
                item.AchievementID == StudentApiFactory.EarnedAchievementId)
            .ToListAsync());
        AssertRejectedGrantAudit(
            auditEvent,
            AchievementAwardAuditReason.ManualGrantEnrollmentMissing);
    }

    [Fact]
    public async Task ManualGrant_MissingStudent_ReturnsNotFoundAndAuditsRejection()
    {
        using var client = CreateClient();
        await Login(client, StudentApiFactory.TeacherId);

        var missingStudentId = Guid.Parse("b0000000-0000-0000-0000-000000000999");
        var grant = await client.PostAsJsonAsync(
            $"{AchievementsUrl(StudentApiFactory.CourseId)}/{StudentApiFactory.EarnedAchievementId}/awards",
            new ManualAchievementAwardRequest(missingStudentId),
            JsonOptions);

        Assert.Equal(HttpStatusCode.NotFound, grant.StatusCode);
        var error = await grant.Content.ReadFromJsonAsync<ApiErrorDto>(JsonOptions);
        Assert.Equal("student_not_found", error!.Code);

        using var verificationScope = factory.Services.CreateScope();
        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<AchievementDbContext>();
        var auditEvent = Assert.Single(await verificationDb.AchievementAwardAuditEvents
            .Where(item =>
                item.StudentID == missingStudentId &&
                item.AchievementID == StudentApiFactory.EarnedAchievementId)
            .ToListAsync());
        AssertRejectedGrantAudit(
            auditEvent,
            AchievementAwardAuditReason.ManualGrantStudentNotFound);
    }

    [Fact]
    public async Task ManualRevoke_PrerequisiteRevokesUnsupportedDependentAwards()
    {
        using var client = CreateClient();
        await Login(client, StudentApiFactory.TeacherId);

        var grantDependent = await client.PostAsJsonAsync(
            $"{AchievementsUrl(StudentApiFactory.CourseId)}/{StudentApiFactory.LockedAchievementId}/awards",
            new ManualAchievementAwardRequest(StudentApiFactory.StudentId),
            JsonOptions);
        var revokePrerequisite = await client.DeleteAsync(
            $"{AchievementsUrl(StudentApiFactory.CourseId)}/{StudentApiFactory.EarnedAchievementId}/awards/{StudentApiFactory.StudentId}");

        Assert.Equal(HttpStatusCode.OK, grantDependent.StatusCode);
        Assert.Equal(HttpStatusCode.OK, revokePrerequisite.StatusCode);

        using var verificationScope = factory.Services.CreateScope();
        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<AchievementDbContext>();
        Assert.False(await verificationDb.StudentAchievements.AnyAsync(item =>
            item.StudentID == StudentApiFactory.StudentId &&
            item.AchievementID == StudentApiFactory.EarnedAchievementId));
        Assert.False(await verificationDb.StudentAchievements.AnyAsync(item =>
            item.StudentID == StudentApiFactory.StudentId &&
            item.AchievementID == StudentApiFactory.LockedAchievementId));

        var prerequisiteAuditEvent = Assert.Single(await verificationDb.AchievementAwardAuditEvents
            .Where(item =>
                item.StudentID == StudentApiFactory.StudentId &&
                item.AchievementID == StudentApiFactory.EarnedAchievementId)
            .ToListAsync());
        Assert.Equal(AchievementAwardAuditEventType.Revoked, prerequisiteAuditEvent.EventType);
        Assert.Equal(AchievementAwardAuditReason.ManualRevocation, prerequisiteAuditEvent.Reason);

        var dependentAuditEvents = await verificationDb.AchievementAwardAuditEvents
            .Where(item =>
                item.StudentID == StudentApiFactory.StudentId &&
                item.AchievementID == StudentApiFactory.LockedAchievementId)
            .ToListAsync();
        Assert.Contains(dependentAuditEvents, item =>
            item.EventType == AchievementAwardAuditEventType.Granted &&
            item.Reason == AchievementAwardAuditReason.ManualGrant);
        Assert.Contains(dependentAuditEvents, item =>
            item.EventType == AchievementAwardAuditEventType.Revoked &&
            item.Reason == AchievementAwardAuditReason.PrerequisiteRevocation);
    }

    [Fact]
    public async Task ManualRevoke_KeepsDependentAwardWhenAnotherPrerequisiteIsEarned()
    {
        using var client = CreateClient();
        await Login(client, StudentApiFactory.TeacherId);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AchievementDbContext>();
            var bonus = await db.Achievements.SingleAsync(
                item => item.Id == StudentApiFactory.BonusAchievementId);
            var locked = await db.Achievements.SingleAsync(
                item => item.Id == StudentApiFactory.LockedAchievementId);
            db.StudentAchievements.Add(new StudentAchievementEntity
            {
                Id = Guid.NewGuid(),
                StudentID = StudentApiFactory.StudentId,
                AchievementID = StudentApiFactory.BonusAchievementId,
                Achievement = bonus,
                AchievementGotDate = DateTime.UtcNow,
                AchievementFoundDate = DateTime.UtcNow
            });
            db.AchievementConnections.Add(new AchievementConnectionEntity
            {
                Id = Guid.NewGuid(),
                SourceId = StudentApiFactory.BonusAchievementId,
                Source = bonus,
                TargetId = StudentApiFactory.LockedAchievementId,
                Target = locked
            });
            await db.SaveChangesAsync();
        }

        var grantDependent = await client.PostAsJsonAsync(
            $"{AchievementsUrl(StudentApiFactory.CourseId)}/{StudentApiFactory.LockedAchievementId}/awards",
            new ManualAchievementAwardRequest(StudentApiFactory.StudentId),
            JsonOptions);
        var revokeOnePrerequisite = await client.DeleteAsync(
            $"{AchievementsUrl(StudentApiFactory.CourseId)}/{StudentApiFactory.EarnedAchievementId}/awards/{StudentApiFactory.StudentId}");

        Assert.Equal(HttpStatusCode.OK, grantDependent.StatusCode);
        Assert.Equal(HttpStatusCode.OK, revokeOnePrerequisite.StatusCode);

        using var verificationScope = factory.Services.CreateScope();
        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<AchievementDbContext>();
        Assert.True(await verificationDb.StudentAchievements.AnyAsync(item =>
            item.StudentID == StudentApiFactory.StudentId &&
            item.AchievementID == StudentApiFactory.LockedAchievementId));
        Assert.True(await verificationDb.StudentAchievements.AnyAsync(item =>
            item.StudentID == StudentApiFactory.StudentId &&
            item.AchievementID == StudentApiFactory.BonusAchievementId));
        Assert.False(await verificationDb.StudentAchievements.AnyAsync(item =>
            item.StudentID == StudentApiFactory.StudentId &&
            item.AchievementID == StudentApiFactory.EarnedAchievementId));

        var dependentAuditEvents = await verificationDb.AchievementAwardAuditEvents
            .Where(item =>
                item.StudentID == StudentApiFactory.StudentId &&
                item.AchievementID == StudentApiFactory.LockedAchievementId)
            .ToListAsync();
        Assert.DoesNotContain(dependentAuditEvents, item =>
            item.EventType == AchievementAwardAuditEventType.Revoked &&
            item.Reason == AchievementAwardAuditReason.PrerequisiteRevocation);
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

    private static void AssertRejectedGrantAudit(
        AchievementAwardAuditEventEntity auditEvent,
        AchievementAwardAuditReason reason)
    {
        Assert.Equal(AchievementAwardAuditEventType.Rejected, auditEvent.EventType);
        Assert.Equal(reason, auditEvent.Reason);
        Assert.Null(auditEvent.AwardID);
        Assert.Null(auditEvent.AwardedAt);
        Assert.NotNull(auditEvent.ActorID);
        Assert.NotEqual(AchievementAwardAuditActorRole.System, auditEvent.ActorRole);
    }
}
