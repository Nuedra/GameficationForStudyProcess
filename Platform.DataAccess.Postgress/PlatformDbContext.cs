using Microsoft.EntityFrameworkCore;

namespace Platform.DataAccess.Postgress
{
    public class PlatformDbContext : DbContext
    {
        public PlatformDbContext(DbContextOptions<PlatformDbContext> options)
            : base(options)
        {
        }

        public DbSet<StudentEntity> Students => Set<StudentEntity>();
        public DbSet<CourseEntity> Courses => Set<CourseEntity>();
        public DbSet<CourseInstanceEntity> CourseInstances => Set<CourseInstanceEntity>();
        public DbSet<CourseInstanceStudentEntity> CourseInstanceStudents => Set<CourseInstanceStudentEntity>();
        public DbSet<EducationalGroupEntity> EducationalGroups => Set<EducationalGroupEntity>();
        public DbSet<GroupStudentEntity> GroupStudents => Set<GroupStudentEntity>();
        public DbSet<AchievementEntity> Achievements => Set<AchievementEntity>();
        public DbSet<StudentAchievementEntity> StudentAchievements => Set<StudentAchievementEntity>();
        public DbSet<AchievementCriteriaEntity> AchievementCriterias => Set<AchievementCriteriaEntity>();
        public DbSet<AchievementConnectionEntity> AchievementConnections => Set<AchievementConnectionEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureStudents(modelBuilder);
            ConfigureCourses(modelBuilder);
            ConfigureCourseInstances(modelBuilder);
            ConfigureCourseInstanceStudents(modelBuilder);
            ConfigureEducationalGroups(modelBuilder);
            ConfigureGroupStudents(modelBuilder);
            ConfigureAchievements(modelBuilder);
            ConfigureStudentAchievements(modelBuilder);
            ConfigureAchievementCriteria(modelBuilder);
            ConfigureAchievementConnections(modelBuilder);
        }

        private static void ConfigureStudents(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StudentEntity>(e =>
            {
                e.ToTable("students");
                e.HasKey(x => x.Id);

                e.Property(x => x.Name).IsRequired();
                e.Property(x => x.Surname).IsRequired();
                e.Property(x => x.Group).IsRequired();

                e.HasMany(x => x.StudentAchievements)
                 .WithOne(x => x.Student)
                 .HasForeignKey(x => x.StudentID)
                 .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void ConfigureCourses(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CourseEntity>(e =>
            {
                e.ToTable("courses");
                e.HasKey(x => x.Id);

                e.Property(x => x.Title).IsRequired();
                e.Property(x => x.Description).IsRequired(false);
                e.Property(x => x.AuthorEntity).IsRequired(false);
                e.Property(x => x.ContentScopeID).IsRequired(false);
                e.HasIndex(x => x.ContentScopeID).IsUnique();

                // Course -> Achievements (1:N)
                e.HasMany(x => x.Achievements)
                 .WithOne(x => x.Course)
                 .HasForeignKey(x => x.CourseID)
                 .OnDelete(DeleteBehavior.Cascade);

                // self-reference (PreviousCourse)
                e.HasOne(x => x.PreviousCourse)
                 .WithMany()
                 .HasForeignKey(x => x.PreviousID)
                 .OnDelete(DeleteBehavior.SetNull);
            });
        }

        private static void ConfigureCourseInstances(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CourseInstanceEntity>(e =>
            {
                e.ToTable("course_instances");
                e.HasKey(x => new { x.CourseID, x.Year });

                e.Property(x => x.ContentScopeID).IsRequired();
                e.Property(x => x.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                e.HasIndex(x => x.ContentScopeID).IsUnique();

                e.HasOne(x => x.Course)
                    .WithMany(x => x.Instances)
                    .HasForeignKey(x => x.CourseID)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void ConfigureCourseInstanceStudents(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CourseInstanceStudentEntity>(e =>
            {
                e.ToTable("course_instance_students");
                e.HasKey(x => new { x.CourseID, x.Year, x.PersonID });

                e.Property(x => x.StartDate).IsRequired();
                e.Property(x => x.EndDate).IsRequired(false);

                e.HasOne(x => x.CourseInstance)
                    .WithMany(x => x.Students)
                    .HasForeignKey(x => new { x.CourseID, x.Year })
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Student)
                    .WithMany(x => x.CourseEnrollments)
                    .HasForeignKey(x => x.PersonID)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void ConfigureEducationalGroups(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EducationalGroupEntity>(e =>
            {
                e.ToTable("educational_groups");
                e.HasKey(x => x.GroupName);

                e.Property(x => x.GroupName).IsRequired();
                e.Property(x => x.GroupCaption).IsRequired();
                e.Property(x => x.EdProgramID).IsRequired();
                e.Property(x => x.AdmissionYear).IsRequired();
                e.Property(x => x.StartDate).IsRequired();
                e.Property(x => x.EndDate).IsRequired(false);
            });
        }

        private static void ConfigureGroupStudents(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GroupStudentEntity>(e =>
            {
                e.ToTable("group_students");
                e.HasKey(x => new { x.PersonID, x.EdGroupID, x.StartDate });

                e.Property(x => x.EdGroupID).IsRequired();
                e.Property(x => x.StartDate).IsRequired();
                e.Property(x => x.EndDate).IsRequired(false);

                e.HasOne(x => x.Student)
                    .WithMany(x => x.GroupMemberships)
                    .HasForeignKey(x => x.PersonID)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.EducationalGroup)
                    .WithMany(x => x.Students)
                    .HasForeignKey(x => x.EdGroupID)
                    .HasPrincipalKey(x => x.GroupName)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void ConfigureAchievements(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AchievementEntity>(e =>
            {
                e.ToTable("achievements");
                e.HasKey(x => x.Id);

                e.Property(x => x.Title).IsRequired();
                e.Property(x => x.Description).IsRequired(false);
                e.Property(x => x.Year).IsRequired();
                e.Property(x => x.Rarity)
                    .HasConversion(
                        rarity => rarity.ToString().ToLowerInvariant(),
                        value => Enum.Parse<AchievementRarity>(value, true))
                    .IsRequired()
                    .HasDefaultValue(AchievementRarity.Common);
                e.Property(x => x.Track).IsRequired().HasDefaultValue("default");
                e.Property(x => x.LabID).IsRequired(false);
                e.HasIndex(x => x.LabID);
                e.ToTable(table => table.HasCheckConstraint(
                    "CK_achievements_Rarity",
                    "\"Rarity\" IN ('common', 'rare', 'epic', 'legendary')"));

                // Achievement -> StudentAchievements (1:N)
                e.HasMany(x => x.StudentAchievements)
                 .WithOne(x => x.Achievement)
                 .HasForeignKey(x => x.AchievementID)
                 .OnDelete(DeleteBehavior.Cascade);

                // Achievement -> Criteria (1:1)
                e.HasOne(x => x.Criteria)
                 .WithOne(x => x.Achievement)
                 .HasForeignKey<AchievementCriteriaEntity>(x => x.AchievementID)
                 .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void ConfigureStudentAchievements(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StudentAchievementEntity>(e =>
            {
                e.ToTable("student_achievements");
                e.HasKey(x => x.Id);

                e.Property(x => x.AchievementGotDate).IsRequired();
                e.Property(x => x.AchievementFoundDate).IsRequired();
                e.Property(x => x.IsNotificationSeen).IsRequired();
                e.Property(x => x.IsFirstAnimationShown).IsRequired();
                e.Property(x => x.LabID).IsRequired(false);
                e.HasIndex(x => x.LabID);

                // можно запретить дубликаты: один студент — одно достижение
                e.HasIndex(x => new { x.StudentID, x.AchievementID }).IsUnique();
            });
        }

        private static void ConfigureAchievementCriteria(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AchievementCriteriaEntity>(e =>
            {
                e.ToTable("achievement_criterias");
                e.HasKey(x => x.Id);

                e.Property(x => x.Expression).IsRequired();
                e.Property(x => x.IsEnabled).IsRequired();
                e.Property(x => x.Scope)
                    .HasConversion<string>()
                    .IsRequired()
                    .HasDefaultValue(AchievementCriteriaScope.SameMark);
                e.ToTable(table => table.HasCheckConstraint(
                    "CK_achievement_criterias_Scope",
                    "\"Scope\" IN ('SameMark', 'AcrossCourse', 'AllLabs')"));
            });
        }

        private static void ConfigureAchievementConnections(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AchievementConnectionEntity>(e =>
            {
                e.ToTable("achievement_connections");
                e.HasKey(x => x.Id);

                // Source
                e.HasOne(x => x.Source)
                 .WithMany()
                 .HasForeignKey(x => x.SourceId)
                 .OnDelete(DeleteBehavior.Restrict);

                // Target
                e.HasOne(x => x.Target)
                 .WithMany()
                 .HasForeignKey(x => x.TargetId)
                 .OnDelete(DeleteBehavior.Restrict);

                // чтобы не было дублей связей
                e.HasIndex(x => new { x.SourceId, x.TargetId }).IsUnique();
            });
        }
    }
}
