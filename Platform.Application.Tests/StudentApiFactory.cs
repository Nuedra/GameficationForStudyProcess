using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Platform.Application.Services;
using Platform.Core.Appraisals;
using Platform.Core.Processing;
using Platform.DataAccess.Postgress;

namespace Platform.Application.Tests;

public sealed class StudentApiFactory : WebApplicationFactory<Program>
{
    public static readonly Guid StudentId =
        Guid.Parse("b0000000-0000-0000-0000-000000000001");
    public static readonly Guid OtherStudentId =
        Guid.Parse("b0000000-0000-0000-0000-000000000002");
    public static readonly Guid CourseId =
        Guid.Parse("a1000000-0000-0000-0000-000000000001");
    public static readonly Guid OtherCourseId =
        Guid.Parse("a1000000-0000-0000-0000-000000000002");
    public static readonly Guid EarnedAchievementId =
        Guid.Parse("00000000-0000-0000-0000-000000000002");
    public static readonly Guid LockedAchievementId =
        Guid.Parse("00000000-0000-0000-0000-000000000003");

    private readonly string _databaseName = Guid.NewGuid().ToString();
    private readonly object _databaseLock = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<PlatformDbContext>>();
            services.RemoveAll<PlatformDbContext>();
            services.RemoveAll<IAchievementGraphTemplateProvider>();
            services.RemoveAll<IAppraisalPayloadProvider>();
            services.RemoveAll<AchievementProcessingCycle>();
            services.AddDbContext<PlatformDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
            services.AddSingleton<IAchievementGraphTemplateProvider>(
                new TestAchievementGraphTemplateProvider());
            services.AddSingleton<IAppraisalPayloadProvider>(
                new TestAppraisalPayloadProvider());
            services.AddScoped(serviceProvider =>
            {
                var options = serviceProvider
                    .GetRequiredService<DbContextOptions<PlatformDbContext>>();

                return new AchievementProcessingCycle(
                    () => new PlatformDbContext(options),
                    serviceProvider.GetRequiredService<IAppraisalPayloadProvider>(),
                    serviceProvider.GetRequiredService<IAppraisalFactsExtractor>(),
                    TimeProvider.System);
            });

            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
            dbContext.Database.EnsureCreated();
            Seed(dbContext);
        });
    }

    public void ResetDatabase()
    {
        lock (_databaseLock)
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();

            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
            Seed(dbContext);
        }
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
            Title = "Первый коммит",
            Description = "Полученная ачивка",
            CourseID = CourseId,
            Course = course,
            Year = 2026
        };
        var lockedAchievement = new AchievementEntity
        {
            Id = LockedAchievementId,
            Title = "Полпути пройдено!",
            Description = "Ещё не полученная ачивка",
            CourseID = CourseId,
            Course = course,
            Year = 2026
        };
        var lockedCriteria = new AchievementCriteriaEntity
        {
            Id = Guid.NewGuid(),
            AchievementID = LockedAchievementId,
            Achievement = lockedAchievement,
            Expression = "template_achievement_3",
            IsEnabled = true,
            Scope = AchievementCriteriaScope.SameMark
        };
        lockedAchievement.Criteria = lockedCriteria;

        dbContext.AddRange(
            student,
            otherStudent,
            course,
            otherCourse,
            courseInstance,
            otherCourseInstance,
            earnedAchievement,
            lockedAchievement,
            lockedCriteria,
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
            },
            new AchievementConnectionEntity
            {
                Id = Guid.NewGuid(),
                SourceId = EarnedAchievementId,
                Source = earnedAchievement,
                TargetId = LockedAchievementId,
                Target = lockedAchievement
            });

        dbContext.SaveChanges();
    }

    private sealed class TestAchievementGraphTemplateProvider : IAchievementGraphTemplateProvider
    {
        public Task<string> GetTemplateAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                """
                <graph>
                  <node id="earned" AchivementId="00000000-0000-0000-0000-000000000002" label="Первый коммит">
                    <geometry x="0" y="0"/>
                    <status state="locked"/>
                  </node>
                  <node id="available" AchivementId="00000000-0000-0000-0000-000000000003" label="Полпути пройдено!">
                    <geometry x="1" y="0"/>
                    <status state="locked"/>
                  </node>
                  <node id="not-from-db" AchivementId="00000000-0000-0000-0000-000000000004" label="Нет в БД">
                    <geometry x="2" y="0"/>
                    <status state="earned"/>
                  </node>
                  <edge id="edge-earned-available" source="earned" target="available">
                    <status state="locked"/>
                  </edge>
                </graph>
                """);
        }
    }

    private sealed class TestAppraisalPayloadProvider : IAppraisalPayloadProvider
    {
        public Task<IReadOnlyList<AppraisalPayloadDto>> GetPayloadsAsync(
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<AppraisalPayloadDto> payloads =
            [
                new AppraisalPayloadDto
                {
                    StudentId = StudentId,
                    CourseId = CourseId,
                    Year = 2026,
                    AppraisalLists =
                    [
                        new AppraisalListDto
                        {
                            ListId = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                            ListName = "Тестовая ведомость",
                            DateCreated = new DateTimeOffset(
                                2026,
                                3,
                                1,
                                0,
                                0,
                                0,
                                TimeSpan.Zero),
                            DateClosed = null,
                            Marks =
                            [
                                new AppraisalMarkDto
                                {
                                    ColumnId = Guid.Parse(
                                        "99999999-9999-9999-9999-999999999999"),
                                    ColumnName = "Тестовая лабораторная",
                                    IsComputed = false,
                                    MaxScore = 10,
                                    MinAcceptScore = 6,
                                    Score = 10,
                                    ScoreSourceName = "Преподаватель",
                                    UpdatedAt = new DateTimeOffset(
                                        2026,
                                        3,
                                        2,
                                        0,
                                        0,
                                        0,
                                        TimeSpan.Zero),
                                    Tags = ["template_achievement_3"],
                                    Deadline = null,
                                    UploadedAt = null
                                }
                            ]
                        }
                    ]
                }
            ];

            return Task.FromResult(payloads);
        }
    }
}
