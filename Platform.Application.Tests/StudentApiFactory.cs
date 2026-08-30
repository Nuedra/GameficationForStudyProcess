using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Platform.Application.Services;
using Platform.Core.Appraisals;
using Platform.Core.Processing;
using Platform.DataAccess.Postgress;
using Platform.DataAccess.Postgress.Lms;
using Platform.Lms;

namespace Platform.Application.Tests;

public sealed class StudentApiFactory : WebApplicationFactory<Program>
{
    public static readonly Guid StudentId =
        Guid.Parse("b0000000-0000-0000-0000-000000000001");
    public static readonly Guid OtherStudentId =
        Guid.Parse("b0000000-0000-0000-0000-000000000002");
    public static readonly Guid CoursePeerStudentId =
        Guid.Parse("b0000000-0000-0000-0000-000000000003");
    public static readonly Guid CourseZeroAchievementStudentId =
        Guid.Parse("b0000000-0000-0000-0000-000000000004");
    public static readonly Guid AdditionalCourseLeaderStudentId =
        Guid.Parse("b0000000-0000-0000-0000-000000000005");
    public static readonly Guid AdditionalCourseTrailingStudentId =
        Guid.Parse("b0000000-0000-0000-0000-000000000006");
    public static readonly Guid TeacherId =
        Guid.Parse("b1000000-0000-0000-0000-000000000001");
    public static readonly Guid AdministratorId =
        Guid.Parse("b2000000-0000-0000-0000-000000000001");
    public static readonly Guid CourseId =
        Guid.Parse("a1000000-0000-0000-0000-000000000001");
    public static readonly Guid OtherCourseId =
        Guid.Parse("a1000000-0000-0000-0000-000000000002");
    public static readonly Guid AdditionalCourseId =
        Guid.Parse("a1000000-0000-0000-0000-000000000003");
    public static readonly Guid EarnedAchievementId =
        Guid.Parse("00000000-0000-0000-0000-000000000002");
    public static readonly Guid LockedAchievementId =
        Guid.Parse("00000000-0000-0000-0000-000000000003");
    public static readonly Guid BonusAchievementId =
        Guid.Parse("00000000-0000-0000-0000-000000000005");
    public static readonly Guid AdditionalCourseEarnedAchievementId =
        Guid.Parse("00000000-0000-0000-0000-000000000101");
    public static readonly Guid AdditionalCourseAvailableAchievementId =
        Guid.Parse("00000000-0000-0000-0000-000000000102");
    public static readonly Guid AdditionalCourseBonusAchievementId =
        Guid.Parse("00000000-0000-0000-0000-000000000103");

    private readonly string _databaseName = Guid.NewGuid().ToString();
    private readonly object _databaseLock = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GuidAuthentication:PrivilegedUsers:0:Id"] = TeacherId.ToString(),
                ["GuidAuthentication:PrivilegedUsers:0:DisplayName"] = "Преподаватель Тестовый",
                ["GuidAuthentication:PrivilegedUsers:0:Role"] = "Teacher",
                ["GuidAuthentication:PrivilegedUsers:0:IsActive"] = "true",
                ["GuidAuthentication:PrivilegedUsers:1:Id"] = AdministratorId.ToString(),
                ["GuidAuthentication:PrivilegedUsers:1:DisplayName"] = "Администратор Тестовый",
                ["GuidAuthentication:PrivilegedUsers:1:Role"] = "Administrator",
                ["GuidAuthentication:PrivilegedUsers:1:IsActive"] = "true"
            }));
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AchievementDbContext>>();
            services.RemoveAll<AchievementDbContext>();
            services.RemoveAll<DbContextOptions<LocalLmsDbContext>>();
            services.RemoveAll<LocalLmsDbContext>();
            services.RemoveAll<IAchievementGraphTemplateProvider>();
            services.RemoveAll<IAppraisalPayloadProvider>();
            services.RemoveAll<AchievementProcessingCycle>();
            services.AddDbContext<AchievementDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
            services.AddDbContext<LocalLmsDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
            services.AddSingleton<IAchievementGraphTemplateProvider>(
                new TestAchievementGraphTemplateProvider());
            services.AddSingleton<IAppraisalPayloadProvider>(
                new TestAppraisalPayloadProvider());
            services.AddScoped(serviceProvider =>
            {
                var options = serviceProvider
                    .GetRequiredService<DbContextOptions<AchievementDbContext>>();

                return new AchievementProcessingCycle(
                    () => new AchievementDbContext(options),
                    serviceProvider.GetRequiredService<ILmsDataSource>(),
                    serviceProvider.GetRequiredService<IAppraisalPayloadProvider>(),
                    serviceProvider.GetRequiredService<IAppraisalFactsExtractor>(),
                    TimeProvider.System);
            });

            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var achievementDbContext = scope.ServiceProvider
                .GetRequiredService<AchievementDbContext>();
            var lmsDbContext = scope.ServiceProvider
                .GetRequiredService<LocalLmsDbContext>();
            achievementDbContext.Database.EnsureCreated();
            lmsDbContext.Database.EnsureCreated();
            Seed(achievementDbContext, lmsDbContext);
        });
    }

    public void ResetDatabase()
    {
        lock (_databaseLock)
        {
            using var scope = Services.CreateScope();
            var achievementDbContext = scope.ServiceProvider
                .GetRequiredService<AchievementDbContext>();
            var lmsDbContext = scope.ServiceProvider
                .GetRequiredService<LocalLmsDbContext>();

            achievementDbContext.Database.EnsureDeleted();
            achievementDbContext.Database.EnsureCreated();
            lmsDbContext.Database.EnsureCreated();
            Seed(achievementDbContext, lmsDbContext);
        }
    }

    private static void Seed(
        AchievementDbContext achievementDbContext,
        LocalLmsDbContext lmsDbContext)
    {
        if (lmsDbContext.Students.Any())
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
        var coursePeerStudent = new StudentEntity
        {
            Id = CoursePeerStudentId,
            Name = "Мария",
            Surname = "Сидорова",
            Group = "ИВТ-101"
        };
        var courseZeroAchievementStudent = new StudentEntity
        {
            Id = CourseZeroAchievementStudentId,
            Name = "Анна",
            Surname = "Смирнова",
            Group = "ИВТ-101"
        };
        var additionalCourseLeaderStudent = new StudentEntity
        {
            Id = AdditionalCourseLeaderStudentId,
            Name = "Сергей",
            Surname = "Васильев",
            Group = "ИВТ-101"
        };
        var additionalCourseTrailingStudent = new StudentEntity
        {
            Id = AdditionalCourseTrailingStudentId,
            Name = "Ольга",
            Surname = "Кузнецова",
            Group = "ИВТ-101"
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
        var additionalCourse = new CourseEntity
        {
            Id = AdditionalCourseId,
            Title = "Дискретная математика",
            Description = "Дополнительный курс текущего студента",
            ContentScopeID = Guid.NewGuid()
        };
        var courseInstance = new CourseInstanceEntity
        {
            CourseID = CourseId,
            Year = 2026,
            ContentScopeID = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };
        var otherCourseInstance = new CourseInstanceEntity
        {
            CourseID = OtherCourseId,
            Year = 2026,
            ContentScopeID = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };
        var additionalCourseInstance = new CourseInstanceEntity
        {
            CourseID = AdditionalCourseId,
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
            Year = 2026
        };
        var lockedAchievement = new AchievementEntity
        {
            Id = LockedAchievementId,
            Title = "Полпути пройдено!",
            Description = "Ещё не полученная ачивка",
            CourseID = CourseId,
            Year = 2026
        };
        var bonusAchievement = new AchievementEntity
        {
            Id = BonusAchievementId,
            Title = "Командная работа",
            Description = "Дополнительная тестовая ачивка",
            CourseID = CourseId,
            Year = 2026
        };
        var additionalCourseEarnedAchievement = new AchievementEntity
        {
            Id = AdditionalCourseEarnedAchievementId,
            Title = "Первый коммит",
            Description = "Полученная ачивка дополнительного курса",
            Rarity = AchievementRarity.Rare,
            CourseID = AdditionalCourseId,
            Year = 2026
        };
        var additionalCourseAvailableAchievement = new AchievementEntity
        {
            Id = AdditionalCourseAvailableAchievementId,
            Title = "Полпути пройдено!",
            Description = "Доступная ачивка дополнительного курса",
            CourseID = AdditionalCourseId,
            Year = 2026
        };
        var additionalCourseBonusAchievement = new AchievementEntity
        {
            Id = AdditionalCourseBonusAchievementId,
            Title = "Образцовый студент",
            Description = "Дополнительная ачивка для таблицы лидеров",
            CourseID = AdditionalCourseId,
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
        var additionalCourseEarnedCriteria = new AchievementCriteriaEntity
        {
            Id = Guid.NewGuid(),
            AchievementID = AdditionalCourseEarnedAchievementId,
            Achievement = additionalCourseEarnedAchievement,
            Expression = "template_achievement_2",
            IsEnabled = true,
            Scope = AchievementCriteriaScope.SameMark
        };
        var additionalCourseAvailableCriteria = new AchievementCriteriaEntity
        {
            Id = Guid.NewGuid(),
            AchievementID = AdditionalCourseAvailableAchievementId,
            Achievement = additionalCourseAvailableAchievement,
            Expression = "template_achievement_3",
            IsEnabled = true,
            Scope = AchievementCriteriaScope.SameMark
        };
        additionalCourseEarnedAchievement.Criteria = additionalCourseEarnedCriteria;
        additionalCourseAvailableAchievement.Criteria = additionalCourseAvailableCriteria;

        lmsDbContext.AddRange(
            student,
            otherStudent,
            coursePeerStudent,
            courseZeroAchievementStudent,
            additionalCourseLeaderStudent,
            additionalCourseTrailingStudent,
            course,
            otherCourse,
            additionalCourse,
            courseInstance,
            otherCourseInstance,
            additionalCourseInstance,
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
                CourseID = AdditionalCourseId,
                Year = 2026,
                PersonID = StudentId,
                Student = student,
                CourseInstance = additionalCourseInstance,
                StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new CourseInstanceStudentEntity
            {
                CourseID = AdditionalCourseId,
                Year = 2026,
                PersonID = AdditionalCourseLeaderStudentId,
                Student = additionalCourseLeaderStudent,
                CourseInstance = additionalCourseInstance,
                StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new CourseInstanceStudentEntity
            {
                CourseID = AdditionalCourseId,
                Year = 2026,
                PersonID = AdditionalCourseTrailingStudentId,
                Student = additionalCourseTrailingStudent,
                CourseInstance = additionalCourseInstance,
                StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new CourseInstanceStudentEntity
            {
                CourseID = CourseId,
                Year = 2026,
                PersonID = CoursePeerStudentId,
                Student = coursePeerStudent,
                CourseInstance = courseInstance,
                StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new CourseInstanceStudentEntity
            {
                CourseID = CourseId,
                Year = 2026,
                PersonID = CourseZeroAchievementStudentId,
                Student = courseZeroAchievementStudent,
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
            new CourseInstanceTeacherEntity
            {
                CourseID = CourseId,
                Year = 2026,
                PersonID = TeacherId,
                CourseInstance = courseInstance,
                StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsLead = true
            });

        lmsDbContext.SaveChanges();

        achievementDbContext.AddRange(
            earnedAchievement,
            lockedAchievement,
            bonusAchievement,
            additionalCourseEarnedAchievement,
            additionalCourseAvailableAchievement,
            additionalCourseBonusAchievement,
            lockedCriteria,
            additionalCourseEarnedCriteria,
            additionalCourseAvailableCriteria,
            new StudentAchievementEntity
            {
                Id = Guid.NewGuid(),
                StudentID = StudentId,
                AchievementID = EarnedAchievementId,
                Achievement = earnedAchievement,
                AchievementGotDate =
                    new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc),
                AchievementFoundDate =
                    new DateTime(2026, 3, 1, 10, 5, 0, DateTimeKind.Utc)
            },
            new StudentAchievementEntity
            {
                Id = Guid.NewGuid(),
                StudentID = StudentId,
                AchievementID = AdditionalCourseEarnedAchievementId,
                Achievement = additionalCourseEarnedAchievement,
                AchievementGotDate =
                    new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc),
                AchievementFoundDate =
                    new DateTime(2026, 4, 1, 10, 5, 0, DateTimeKind.Utc)
            },
            new StudentAchievementEntity
            {
                Id = Guid.NewGuid(),
                StudentID = CoursePeerStudentId,
                AchievementID = EarnedAchievementId,
                Achievement = earnedAchievement,
                AchievementGotDate =
                    new DateTime(2026, 3, 2, 10, 0, 0, DateTimeKind.Utc),
                AchievementFoundDate =
                    new DateTime(2026, 3, 2, 10, 5, 0, DateTimeKind.Utc)
            },
            new StudentAchievementEntity
            {
                Id = Guid.NewGuid(),
                StudentID = CoursePeerStudentId,
                AchievementID = BonusAchievementId,
                Achievement = bonusAchievement,
                AchievementGotDate =
                    new DateTime(2026, 3, 3, 10, 0, 0, DateTimeKind.Utc),
                AchievementFoundDate =
                    new DateTime(2026, 3, 3, 10, 5, 0, DateTimeKind.Utc)
            },
            new StudentAchievementEntity
            {
                Id = Guid.NewGuid(),
                StudentID = AdditionalCourseLeaderStudentId,
                AchievementID = AdditionalCourseEarnedAchievementId,
                Achievement = additionalCourseEarnedAchievement,
                AchievementGotDate =
                    new DateTime(2026, 4, 2, 10, 0, 0, DateTimeKind.Utc),
                AchievementFoundDate =
                    new DateTime(2026, 4, 2, 10, 5, 0, DateTimeKind.Utc)
            },
            new StudentAchievementEntity
            {
                Id = Guid.NewGuid(),
                StudentID = AdditionalCourseLeaderStudentId,
                AchievementID = AdditionalCourseAvailableAchievementId,
                Achievement = additionalCourseAvailableAchievement,
                AchievementGotDate =
                    new DateTime(2026, 4, 3, 10, 0, 0, DateTimeKind.Utc),
                AchievementFoundDate =
                    new DateTime(2026, 4, 3, 10, 5, 0, DateTimeKind.Utc)
            },
            new StudentAchievementEntity
            {
                Id = Guid.NewGuid(),
                StudentID = AdditionalCourseLeaderStudentId,
                AchievementID = AdditionalCourseBonusAchievementId,
                Achievement = additionalCourseBonusAchievement,
                AchievementGotDate =
                    new DateTime(2026, 4, 4, 10, 0, 0, DateTimeKind.Utc),
                AchievementFoundDate =
                    new DateTime(2026, 4, 4, 10, 5, 0, DateTimeKind.Utc)
            },
            new AchievementConnectionEntity
            {
                Id = Guid.NewGuid(),
                SourceId = EarnedAchievementId,
                Source = earnedAchievement,
                TargetId = LockedAchievementId,
                Target = lockedAchievement
            },
            new AchievementConnectionEntity
            {
                Id = Guid.NewGuid(),
                SourceId = AdditionalCourseEarnedAchievementId,
                Source = additionalCourseEarnedAchievement,
                TargetId = AdditionalCourseAvailableAchievementId,
                Target = additionalCourseAvailableAchievement
            });

        achievementDbContext.SaveChanges();
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
                },
                new AppraisalPayloadDto
                {
                    StudentId = CourseZeroAchievementStudentId,
                    CourseId = CourseId,
                    Year = 2026,
                    AppraisalLists =
                    [
                        new AppraisalListDto
                        {
                            ListId = Guid.Parse("88888888-8888-8888-8888-888888888889"),
                            ListName = "Тестовая ведомость для каскада",
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
                                        "99999999-9999-9999-9999-999999999998"),
                                    ColumnName = "Тестовая лабораторная для каскада",
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
