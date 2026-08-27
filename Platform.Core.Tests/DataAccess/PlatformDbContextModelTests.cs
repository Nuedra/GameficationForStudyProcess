using Microsoft.EntityFrameworkCore;
using Platform.DataAccess.Postgress;
using Platform.DataAccess.Postgress.Lms;

namespace Platform.Core.Tests.DataAccess;

public sealed class PlatformDbContextModelTests
{
    [Fact]
    public void AchievementContext_ContainsOnlyAchievementEntities()
    {
        using var dbContext = CreateAchievementDbContext();

        Assert.NotNull(dbContext.Model.FindEntityType(typeof(AchievementEntity)));
        Assert.Null(dbContext.Model.FindEntityType(typeof(StudentEntity)));
        Assert.Null(dbContext.Model.FindEntityType(typeof(CourseEntity)));
        Assert.Null(dbContext.Model.FindEntityType(typeof(CourseInstanceTeacherEntity)));
    }

    [Fact]
    public void LocalLmsContext_DoesNotContainAchievementEntities()
    {
        using var dbContext = CreateLocalLmsDbContext();

        Assert.NotNull(dbContext.Model.FindEntityType(typeof(StudentEntity)));
        Assert.NotNull(dbContext.Model.FindEntityType(typeof(CourseEntity)));
        Assert.NotNull(dbContext.Model.FindEntityType(typeof(CourseInstanceTeacherEntity)));
        Assert.Null(dbContext.Model.FindEntityType(typeof(AchievementEntity)));
    }

    [Fact]
    public void AchievementRarity_IsStoredAsLowercaseStringWithCommonDefault()
    {
        using var dbContext = CreateAchievementDbContext();
        var entity = dbContext.Model.FindEntityType(typeof(AchievementEntity));
        var rarity = entity!.FindProperty(nameof(AchievementEntity.Rarity))!;
        var converter = rarity.GetValueConverter()!;

        Assert.Equal(typeof(string), converter.ProviderClrType);
        Assert.Equal("common", converter.ConvertToProvider(AchievementRarity.Common));
        Assert.Equal(AchievementRarity.Legendary, converter.ConvertFromProvider("legendary"));
        Assert.Equal(AchievementRarity.Common, rarity.GetDefaultValue());
    }

    [Fact]
    public void CourseInstance_UsesCourseAndYearAsCompositeKey()
    {
        using var dbContext = CreateLocalLmsDbContext();
        var entity = dbContext.Model.FindEntityType(typeof(CourseInstanceEntity));

        var keyProperties = entity!.FindPrimaryKey()!
            .Properties
            .Select(property => property.Name);

        Assert.Equal(
            [nameof(CourseInstanceEntity.CourseID), nameof(CourseInstanceEntity.Year)],
            keyProperties);
    }

    [Fact]
    public void CourseInstanceStudent_ReferencesCourseInstanceAndStudent()
    {
        using var dbContext = CreateLocalLmsDbContext();
        var entity = dbContext.Model.FindEntityType(typeof(CourseInstanceStudentEntity));

        var keyProperties = entity!.FindPrimaryKey()!
            .Properties
            .Select(property => property.Name);
        var foreignKeys = entity.GetForeignKeys().ToList();

        Assert.Equal(
            [
                nameof(CourseInstanceStudentEntity.CourseID),
                nameof(CourseInstanceStudentEntity.Year),
                nameof(CourseInstanceStudentEntity.PersonID)
            ],
            keyProperties);
        Assert.Contains(
            foreignKeys,
            key => key.PrincipalEntityType.ClrType == typeof(CourseInstanceEntity));
        Assert.Contains(
            foreignKeys,
            key => key.PrincipalEntityType.ClrType == typeof(StudentEntity));
    }

    [Fact]
    public void GroupStudent_UsesMembershipPeriodInCompositeKey()
    {
        using var dbContext = CreateLocalLmsDbContext();
        var entity = dbContext.Model.FindEntityType(typeof(GroupStudentEntity));

        var keyProperties = entity!.FindPrimaryKey()!
            .Properties
            .Select(property => property.Name);

        Assert.Equal(
            [
                nameof(GroupStudentEntity.PersonID),
                nameof(GroupStudentEntity.EdGroupID),
                nameof(GroupStudentEntity.StartDate)
            ],
            keyProperties);
    }

    [Fact]
    public void CourseInstanceTeacher_UsesCourseYearAndPersonAsCompositeKey()
    {
        using var dbContext = CreateLocalLmsDbContext();
        var entity = dbContext.Model.FindEntityType(typeof(CourseInstanceTeacherEntity));

        var keyProperties = entity!.FindPrimaryKey()!
            .Properties
            .Select(property => property.Name);

        Assert.Equal(
            [
                nameof(CourseInstanceTeacherEntity.CourseID),
                nameof(CourseInstanceTeacherEntity.Year),
                nameof(CourseInstanceTeacherEntity.PersonID)
            ],
            keyProperties);
        Assert.Contains(
            entity.GetForeignKeys(),
            key => key.PrincipalEntityType.ClrType == typeof(CourseInstanceEntity));
        Assert.DoesNotContain(
            entity.GetForeignKeys(),
            key => key.PrincipalEntityType.ClrType == typeof(StudentEntity));
    }

    [Fact]
    public async Task CourseAndGroupMemberships_CanBeLoadedThroughNavigations()
    {
        await using var dbContext = CreateLocalLmsDbContext();
        var student = new StudentEntity
        {
            Id = Guid.NewGuid(),
            Name = "Иван",
            Surname = "Иванов",
            Group = "ИВТ-101"
        };
        var course = new CourseEntity
        {
            Id = Guid.NewGuid(),
            Title = "Информатика",
            ContentScopeID = Guid.NewGuid()
        };
        var courseInstance = new CourseInstanceEntity
        {
            CourseID = course.Id,
            Course = course,
            Year = 2026,
            ContentScopeID = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };
        var group = new EducationalGroupEntity
        {
            GroupName = "ИВТ-101",
            GroupCaption = "ИВТ-101",
            EdProgramID = Guid.NewGuid(),
            AdmissionYear = 2025,
            StartDate = DateTime.UtcNow
        };

        dbContext.AddRange(
            student,
            course,
            courseInstance,
            group,
            new CourseInstanceStudentEntity
            {
                CourseID = course.Id,
                Year = courseInstance.Year,
                PersonID = student.Id,
                StartDate = DateTime.UtcNow
            },
            new GroupStudentEntity
            {
                PersonID = student.Id,
                EdGroupID = group.GroupName,
                StartDate = DateTime.UtcNow
            });
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();

        var loadedStudent = await dbContext.Students
            .Include(item => item.CourseEnrollments)
            .Include(item => item.GroupMemberships)
            .SingleAsync(item => item.Id == student.Id);

        Assert.Single(loadedStudent.CourseEnrollments);
        Assert.Single(loadedStudent.GroupMemberships);
    }

    private static AchievementDbContext CreateAchievementDbContext()
    {
        var options = new DbContextOptionsBuilder<AchievementDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AchievementDbContext(options);
    }

    private static LocalLmsDbContext CreateLocalLmsDbContext()
    {
        var options = new DbContextOptionsBuilder<LocalLmsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new LocalLmsDbContext(options);
    }
}
