using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Platform.DataAccess.Postgress;

namespace Platform.Application.Tests;

public sealed class StudentApiFactory : WebApplicationFactory<Program>
{
    public static readonly Guid StudentId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid OtherStudentId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid CourseId =
        Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid OtherCourseId =
        Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid EarnedAchievementId =
        Guid.Parse("55555555-5555-5555-5555-555555555555");
    public static readonly Guid LockedAchievementId =
        Guid.Parse("66666666-6666-6666-6666-666666666666");

    private readonly string _databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<PlatformDbContext>>();
            services.RemoveAll<PlatformDbContext>();
            services.AddDbContext<PlatformDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));

            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
            dbContext.Database.EnsureCreated();
            Seed(dbContext);
        });
    }

    private static void Seed(PlatformDbContext dbContext)
    {
        if (dbContext.Students.Any())
            return;

        var student = new StudentEntity
        {
            Id = StudentId,
            Name = "Иван",
            Surname = "Иванов",
            Group = "ИВТ-101"
        };
        var otherStudent = new StudentEntity
        {
            Id = OtherStudentId,
            Name = "Пётр",
            Surname = "Петров",
            Group = "ИВТ-102"
        };
        var course = new CourseEntity
        {
            Id = CourseId,
            Title = "Алгоритмы",
            Description = "Основной тестовый курс",
            ContentScopeID = Guid.NewGuid()
        };
        var otherCourse = new CourseEntity
        {
            Id = OtherCourseId,
            Title = "Базы данных",
            Description = "Курс другого студента",
            ContentScopeID = Guid.NewGuid()
        };
        var courseInstance = new CourseInstanceEntity
        {
            CourseID = CourseId,
            Course = course,
            Year = 2026,
            ContentScopeID = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };
        var otherCourseInstance = new CourseInstanceEntity
        {
            CourseID = OtherCourseId,
            Course = otherCourse,
            Year = 2026,
            ContentScopeID = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };
        var earnedAchievement = new AchievementEntity
        {
            Id = EarnedAchievementId,
            Title = "Первая ачивка",
            Description = "Полученная ачивка",
            CourseID = CourseId,
            Course = course,
            Year = 2026
        };
        var lockedAchievement = new AchievementEntity
        {
            Id = LockedAchievementId,
            Title = "Следующая ачивка",
            Description = "Ещё не полученная ачивка",
            CourseID = CourseId,
            Course = course,
            Year = 2026
        };

        dbContext.AddRange(
            student,
            otherStudent,
            course,
            otherCourse,
            courseInstance,
            otherCourseInstance,
            earnedAchievement,
            lockedAchievement,
            new CourseInstanceStudentEntity
            {
                CourseID = CourseId,
                Year = 2026,
                PersonID = StudentId,
                Student = student,
                CourseInstance = courseInstance,
                StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new CourseInstanceStudentEntity
            {
                CourseID = OtherCourseId,
                Year = 2026,
                PersonID = OtherStudentId,
                Student = otherStudent,
                CourseInstance = otherCourseInstance,
                StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new StudentAchievementEntity
            {
                Id = Guid.NewGuid(),
                StudentID = StudentId,
                Student = student,
                AchievementID = EarnedAchievementId,
                Achievement = earnedAchievement,
                AchievementGotDate =
                    new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc),
                AchievementFoundDate =
                    new DateTime(2026, 3, 1, 10, 5, 0, DateTimeKind.Utc)
            });

        dbContext.SaveChanges();
    }
}
