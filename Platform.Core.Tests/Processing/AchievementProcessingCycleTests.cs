using Microsoft.EntityFrameworkCore;
using Platform.Core.Appraisals;
using Platform.Core.Processing;
using Platform.DataAccess.Postgress;

namespace Platform.Core.Tests.Processing;

public sealed class AchievementProcessingCycleTests
{
    private static readonly Guid CourseId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OtherCourseId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid StudentId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid LabId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid OtherLabId = Guid.Parse("55555555-5555-5555-5555-555555555555");

    [Fact]
    public async Task RunAsync_MatchingTags_AssignsAchievementAndSetsFields()
    {
        var databaseName = Guid.NewGuid().ToString();
        var achievement = CreateAchievement("tag1, tag2", labId: LabId);
        await SeedDatabase(databaseName, achievement);
        var uploadedAt = DateTimeOffset.Parse("2026-03-01T10:00:00Z");
        var foundAt = DateTimeOffset.Parse("2026-06-10T12:00:00Z");
        var cycle = CreateCycle(
            databaseName,
            CreatePayload(["tag1", "tag2"], uploadedAt, LabId),
            foundAt);

        var result = await cycle.RunAsync(StudentId);

        Assert.Equal([achievement.Id], result.AssignedAchievements.Select(item => item.Id));
        await using var db = CreateDbContext(databaseName);
        var assigned = Assert.Single(await db.StudentAchievements.ToListAsync());
        Assert.Equal(StudentId, assigned.StudentID);
        Assert.Equal(achievement.Id, assigned.AchievementID);
        Assert.Equal(LabId, assigned.LabID);
        Assert.Equal(uploadedAt.UtcDateTime, assigned.AchievementGotDate);
        Assert.Equal(foundAt.UtcDateTime, assigned.AchievementFoundDate);
        Assert.False(assigned.IsNotificationSeen);
        Assert.False(assigned.IsFirstAnimationShown);
    }

    [Fact]
    public async Task RunAsync_AchievementAlreadyAssigned_DoesNotAssignDuplicate()
    {
        var databaseName = Guid.NewGuid().ToString();
        var achievement = CreateAchievement("tag1");
        await SeedDatabase(databaseName, achievement);
        var cycle = CreateCycle(
            databaseName,
            CreatePayload(["tag1"], DateTimeOffset.Parse("2026-03-01T10:00:00Z")),
            DateTimeOffset.Parse("2026-06-10T12:00:00Z"));

        var firstResult = await cycle.RunAsync(StudentId);
        var secondResult = await cycle.RunAsync(StudentId);

        Assert.Single(firstResult.AssignedAchievements);
        Assert.Empty(secondResult.AssignedAchievements);
        Assert.Single(secondResult.MatchedAchievements);
        await using var db = CreateDbContext(databaseName);
        Assert.Single(await db.StudentAchievements.ToListAsync());
    }

    [Fact]
    public async Task RunAsync_NonSameMarkAchievement_DoesNotSetLabId()
    {
        var databaseName = Guid.NewGuid().ToString();
        var achievement = CreateAchievement(
            "tag1",
            AchievementCriteriaScope.AcrossCourse,
            LabId);
        await SeedDatabase(databaseName, achievement);
        var cycle = CreateCycle(
            databaseName,
            CreatePayload(["tag1"], DateTimeOffset.Parse("2026-03-01T10:00:00Z"), OtherLabId),
            DateTimeOffset.Parse("2026-06-10T12:00:00Z"));

        await cycle.RunAsync(StudentId);

        await using var db = CreateDbContext(databaseName);
        var assigned = Assert.Single(await db.StudentAchievements.ToListAsync());
        Assert.Null(assigned.LabID);
    }

    [Fact]
    public void IsMatch_AllRequiredTagsExistInOneLab_ReturnsTrue()
    {
        var achievement = CreateAchievement("tag1, tag2");
        var facts = CreateFacts(
            CourseId,
            2026,
            CreateMark(["tag1", "tag2", "tag3"]));

        var result = AchievementProcessingCycle.IsMatch(achievement, facts);

        Assert.True(result);
    }

    [Fact]
    public void IsMatch_RequiredTagsAreSplitBetweenLabs_ReturnsFalse()
    {
        var achievement = CreateAchievement("tag1, tag2");
        var facts = CreateFacts(
            CourseId,
            2026,
            CreateMark(["tag1"]),
            CreateMark(["tag2"]));

        var result = AchievementProcessingCycle.IsMatch(achievement, facts);

        Assert.False(result);
    }

    [Fact]
    public void IsMatch_SameMarkWithLabId_OnlyChecksSpecifiedLab()
    {
        var achievement = CreateAchievement("tag1", labId: LabId);
        var facts = CreateFacts(
            CourseId,
            2026,
            CreateMark(["tag1"], columnId: OtherLabId),
            CreateMark(["other"], columnId: LabId));

        var result = AchievementProcessingCycle.IsMatch(achievement, facts);

        Assert.False(result);
    }

    [Fact]
    public void IsMatch_AcrossCourse_RequiredTagsCanBeSplitBetweenLabs()
    {
        var achievement = CreateAchievement(
            "tag1, tag2",
            AchievementCriteriaScope.AcrossCourse);
        var facts = CreateFacts(
            CourseId,
            2026,
            CreateMark(["tag1"]),
            CreateMark(["tag2"]));

        var result = AchievementProcessingCycle.IsMatch(achievement, facts);

        Assert.True(result);
    }

    [Fact]
    public void IsMatch_AcrossCourse_IgnoresAchievementLabId()
    {
        var achievement = CreateAchievement(
            "tag1, tag2",
            AchievementCriteriaScope.AcrossCourse,
            LabId);
        var facts = CreateFacts(
            CourseId,
            2026,
            CreateMark(["tag1"], columnId: OtherLabId),
            CreateMark(["tag2"], columnId: OtherLabId));

        var result = AchievementProcessingCycle.IsMatch(achievement, facts);

        Assert.True(result);
    }

    [Fact]
    public void IsMatch_AllLabs_AllMarksMustMatch()
    {
        var achievement = CreateAchievement("maxscore", AchievementCriteriaScope.AllLabs);
        var facts = CreateFacts(
            CourseId,
            2026,
            CreateMark(["maxscore"]),
            CreateMark(["maxscore"]));

        var result = AchievementProcessingCycle.IsMatch(achievement, facts);

        Assert.True(result);
    }

    [Fact]
    public void IsMatch_AllLabs_OneNonMatchingLabReturnsFalse()
    {
        var achievement = CreateAchievement("maxscore", AchievementCriteriaScope.AllLabs);
        var facts = CreateFacts(
            CourseId,
            2026,
            CreateMark(["maxscore"]),
            CreateMark(["other"]));

        var result = AchievementProcessingCycle.IsMatch(achievement, facts);

        Assert.False(result);
    }

    [Fact]
    public void IsMatch_AllLabs_NoLabsReturnsFalse()
    {
        var achievement = CreateAchievement("maxscore", AchievementCriteriaScope.AllLabs);
        var facts = CreateFacts(CourseId, 2026);

        var result = AchievementProcessingCycle.IsMatch(achievement, facts);

        Assert.False(result);
    }

    [Theory]
    [InlineData("tag1", "Tag1")]
    [InlineData("tag1", "TAG1")]
    public void IsMatch_TagCaseDiffers_ReturnsFalse(string expression, string completedTag)
    {
        var achievement = CreateAchievement(expression);
        var facts = CreateFacts(CourseId, 2026, CreateMark([completedTag]));

        var result = AchievementProcessingCycle.IsMatch(achievement, facts);

        Assert.False(result);
    }

    [Fact]
    public void IsMatch_CourseOrYearDiffers_ReturnsFalse()
    {
        var achievement = CreateAchievement("tag1");
        var otherCourseFacts = CreateFacts(OtherCourseId, 2026, CreateMark(["tag1"]));
        var otherYearFacts = CreateFacts(CourseId, 2025, CreateMark(["tag1"]));

        Assert.False(AchievementProcessingCycle.IsMatch(achievement, otherCourseFacts));
        Assert.False(AchievementProcessingCycle.IsMatch(achievement, otherYearFacts));
    }

    [Fact]
    public void GetAchievementGotDate_UsesEarliestUploadedAtOfMatchingLabs()
    {
        var achievement = CreateAchievement("tag1, tag2");
        var expected = DateTimeOffset.Parse("2026-03-01T10:00:00Z");
        var facts = CreateFacts(
            CourseId,
            2026,
            CreateMark(["tag1"], uploadedAt: DateTimeOffset.Parse("2026-02-01T10:00:00Z")),
            CreateMark(["tag1", "tag2"], uploadedAt: DateTimeOffset.Parse("2026-03-05T10:00:00Z")),
            CreateMark(["tag1", "tag2"], uploadedAt: expected));

        var result = AchievementProcessingCycle.GetAchievementGotDate(
            achievement,
            [facts],
            DateTime.Parse("2026-06-01T00:00:00Z").ToUniversalTime());

        Assert.Equal(expected.UtcDateTime, result);
    }

    [Fact]
    public void GetAchievementGotDate_AcrossCourse_UsesDateWhenLastRequiredTagAppeared()
    {
        var achievement = CreateAchievement(
            "tag1, tag2",
            AchievementCriteriaScope.AcrossCourse);
        var expected = DateTimeOffset.Parse("2026-03-05T10:00:00Z");
        var facts = CreateFacts(
            CourseId,
            2026,
            CreateMark(["tag1"], uploadedAt: DateTimeOffset.Parse("2026-02-01T10:00:00Z")),
            CreateMark(["tag2"], uploadedAt: expected));

        var result = AchievementProcessingCycle.GetAchievementGotDate(
            achievement,
            [facts],
            DateTime.Parse("2026-06-01T00:00:00Z").ToUniversalTime());

        Assert.Equal(expected.UtcDateTime, result);
    }

    [Fact]
    public void GetAchievementGotDate_AllLabs_UsesLatestLabDate()
    {
        var achievement = CreateAchievement("maxscore", AchievementCriteriaScope.AllLabs);
        var expected = DateTimeOffset.Parse("2026-03-05T10:00:00Z");
        var facts = CreateFacts(
            CourseId,
            2026,
            CreateMark(["maxscore"], uploadedAt: DateTimeOffset.Parse("2026-02-01T10:00:00Z")),
            CreateMark(["maxscore"], uploadedAt: expected));

        var result = AchievementProcessingCycle.GetAchievementGotDate(
            achievement,
            [facts],
            DateTime.Parse("2026-06-01T00:00:00Z").ToUniversalTime());

        Assert.Equal(expected.UtcDateTime, result);
    }

    [Fact]
    public void ResolveDependencies_AnyExistingSourceUnlocksTarget()
    {
        var sourceA = CreateAchievement("a");
        var sourceB = CreateAchievement("b");
        var target = CreateAchievement("target");
        var connections = new[]
        {
            new AchievementDependency(sourceA.Id, target.Id),
            new AchievementDependency(sourceB.Id, target.Id)
        };

        var result = AchievementProcessingCycle.ResolveDependencies(
            [target],
            new HashSet<Guid> { sourceB.Id },
            connections);

        Assert.Equal([target.Id], result.Select(achievement => achievement.Id));
    }

    [Fact]
    public void ResolveDependencies_SourceAssignedInCurrentRunUnlocksChain()
    {
        var source = CreateAchievement("source");
        var middle = CreateAchievement("middle");
        var target = CreateAchievement("target");
        var connections = new[]
        {
            new AchievementDependency(source.Id, middle.Id),
            new AchievementDependency(middle.Id, target.Id)
        };

        var result = AchievementProcessingCycle.ResolveDependencies(
            [target, middle, source],
            new HashSet<Guid>(),
            connections);

        Assert.True(new HashSet<Guid> { source.Id, middle.Id, target.Id }
            .SetEquals(result.Select(achievement => achievement.Id)));
    }

    [Fact]
    public void ResolveDependencies_NoAvailableSource_BlocksTarget()
    {
        var source = CreateAchievement("source");
        var target = CreateAchievement("target");

        var result = AchievementProcessingCycle.ResolveDependencies(
            [target],
            new HashSet<Guid>(),
            [new AchievementDependency(source.Id, target.Id)]);

        Assert.Empty(result);
    }

    [Fact]
    public void ResolveDependencies_CyclicDependenciesRemainBlocked()
    {
        var first = CreateAchievement("first");
        var second = CreateAchievement("second");

        var result = AchievementProcessingCycle.ResolveDependencies(
            [first, second],
            new HashSet<Guid>(),
            [
                new AchievementDependency(first.Id, second.Id),
                new AchievementDependency(second.Id, first.Id)
            ]);

        Assert.Empty(result);
    }

    private static AchievementEntity CreateAchievement(
        string expression,
        AchievementCriteriaScope scope = AchievementCriteriaScope.SameMark,
        Guid? labId = null)
    {
        var achievement = new AchievementEntity
        {
            Id = Guid.NewGuid(),
            Title = expression,
            CourseID = CourseId,
            Year = 2026,
            LabID = labId
        };

        achievement.Criteria = new AchievementCriteriaEntity
        {
            Id = Guid.NewGuid(),
            AchievementID = achievement.Id,
            Achievement = achievement,
            Expression = expression,
            Scope = scope,
            IsEnabled = true
        };

        return achievement;
    }

    private static StudentCourseFacts CreateFacts(Guid courseId, int year, params MarkFact[] marks)
    {
        return new StudentCourseFacts
        {
            StudentId = Guid.NewGuid(),
            CourseId = courseId,
            Year = year,
            Marks = marks
        };
    }

    private static MarkFact CreateMark(
        IReadOnlyList<string> tags,
        DateTimeOffset? uploadedAt = null,
        Guid? columnId = null)
    {
        return new MarkFact
        {
            ListId = Guid.NewGuid(),
            ListName = "List",
            DateCreated = DateTimeOffset.Parse("2026-01-01T00:00:00Z"),
            ColumnId = columnId ?? Guid.NewGuid(),
            ColumnName = "Lab",
            IsComputed = false,
            MaxScore = 10,
            MinAcceptScore = 6,
            Tags = tags,
            UploadedAt = uploadedAt
        };
    }

    private static AchievementProcessingCycle CreateCycle(
        string databaseName,
        AppraisalPayloadDto payload,
        DateTimeOffset currentTime)
    {
        return new AchievementProcessingCycle(
            () => CreateDbContext(databaseName),
            new FixedPayloadProvider(payload),
            new AppraisalFactsExtractor(),
            new FixedTimeProvider(currentTime));
    }

    private static PlatformDbContext CreateDbContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new PlatformDbContext(options);
    }

    private static async Task SeedDatabase(string databaseName, AchievementEntity achievement)
    {
        await using var db = CreateDbContext(databaseName);
        db.Students.Add(new StudentEntity
        {
            Id = StudentId,
            Name = "Ivan",
            Surname = "Petrov",
            Group = "IS-101"
        });
        db.Courses.Add(new CourseEntity
        {
            Id = CourseId,
            Title = "Course"
        });
        db.Achievements.Add(achievement);
        await db.SaveChangesAsync();
    }

    private static AppraisalPayloadDto CreatePayload(
        IReadOnlyList<string> tags,
        DateTimeOffset uploadedAt,
        Guid? columnId = null)
    {
        return new AppraisalPayloadDto
        {
            StudentId = StudentId,
            CourseId = CourseId,
            Year = 2026,
            AppraisalLists =
            [
                new AppraisalListDto
                {
                    ListId = Guid.NewGuid(),
                    ListName = "List",
                    DateCreated = DateTimeOffset.Parse("2026-01-01T00:00:00Z"),
                    Marks =
                    [
                        new AppraisalMarkDto
                        {
                            ColumnId = columnId ?? Guid.NewGuid(),
                            ColumnName = "Lab",
                            IsComputed = false,
                            MaxScore = 10,
                            MinAcceptScore = 6,
                            Score = 8,
                            Tags = tags.ToList(),
                            UploadedAt = uploadedAt
                        }
                    ]
                }
            ]
        };
    }

    private sealed class FixedPayloadProvider(AppraisalPayloadDto payload) : IAppraisalPayloadProvider
    {
        public Task<IReadOnlyList<AppraisalPayloadDto>> GetPayloadsAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<AppraisalPayloadDto>>([payload]);
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset currentTime) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => currentTime;
    }
}
