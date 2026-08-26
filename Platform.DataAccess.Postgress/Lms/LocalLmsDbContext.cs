using Microsoft.EntityFrameworkCore;

namespace Platform.DataAccess.Postgress.Lms;

/// <summary>
/// Временный контекст данных LMS. Его модель заменяет ещё не существующую LMS
/// и не входит в migrations подсистемы достижений.
/// </summary>
public sealed class LocalLmsDbContext(DbContextOptions<LocalLmsDbContext> options)
    : DbContext(options)
{
    public DbSet<StudentEntity> Students => Set<StudentEntity>();
    public DbSet<CourseEntity> Courses => Set<CourseEntity>();
    public DbSet<CourseInstanceEntity> CourseInstances => Set<CourseInstanceEntity>();
    public DbSet<CourseInstanceStudentEntity> CourseInstanceStudents => Set<CourseInstanceStudentEntity>();
    public DbSet<EducationalGroupEntity> EducationalGroups => Set<EducationalGroupEntity>();
    public DbSet<GroupStudentEntity> GroupStudents => Set<GroupStudentEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureStudents(modelBuilder);
        ConfigureCourses(modelBuilder);
        ConfigureCourseInstances(modelBuilder);
        ConfigureCourseInstanceStudents(modelBuilder);
        ConfigureEducationalGroups(modelBuilder);
        ConfigureGroupStudents(modelBuilder);
    }

    private static void ConfigureStudents(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StudentEntity>(entity =>
        {
            entity.ToTable("students");
            entity.HasKey(person => person.Id);

            entity.Property(person => person.Name).IsRequired();
            entity.Property(person => person.Surname).IsRequired();
            entity.Property(person => person.Group).IsRequired();
        });
    }

    private static void ConfigureCourses(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CourseEntity>(entity =>
        {
            entity.ToTable("courses");
            entity.HasKey(course => course.Id);

            entity.Property(course => course.Title).IsRequired();
            entity.Property(course => course.Description).IsRequired(false);
            entity.Property(course => course.AuthorEntity).IsRequired(false);
            entity.Property(course => course.ContentScopeID).IsRequired(false);
            entity.HasIndex(course => course.ContentScopeID).IsUnique();

            entity.HasOne(course => course.PreviousCourse)
                .WithMany()
                .HasForeignKey(course => course.PreviousID)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureCourseInstances(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CourseInstanceEntity>(entity =>
        {
            entity.ToTable("course_instances");
            entity.HasKey(course => new { course.CourseID, course.Year });

            entity.Property(course => course.ContentScopeID).IsRequired();
            entity.Property(course => course.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(course => course.ContentScopeID).IsUnique();

            entity.HasOne(course => course.Course)
                .WithMany(course => course.Instances)
                .HasForeignKey(course => course.CourseID)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureCourseInstanceStudents(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CourseInstanceStudentEntity>(entity =>
        {
            entity.ToTable("course_instance_students");
            entity.HasKey(enrollment => new
            {
                enrollment.CourseID,
                enrollment.Year,
                enrollment.PersonID
            });

            entity.Property(enrollment => enrollment.StartDate).IsRequired();
            entity.Property(enrollment => enrollment.EndDate).IsRequired(false);

            entity.HasOne(enrollment => enrollment.CourseInstance)
                .WithMany(course => course.Students)
                .HasForeignKey(enrollment => new { enrollment.CourseID, enrollment.Year })
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(enrollment => enrollment.Student)
                .WithMany(person => person.CourseEnrollments)
                .HasForeignKey(enrollment => enrollment.PersonID)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureEducationalGroups(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EducationalGroupEntity>(entity =>
        {
            entity.ToTable("educational_groups");
            entity.HasKey(group => group.GroupName);

            entity.Property(group => group.GroupName).IsRequired();
            entity.Property(group => group.GroupCaption).IsRequired();
            entity.Property(group => group.EdProgramID).IsRequired();
            entity.Property(group => group.AdmissionYear).IsRequired();
            entity.Property(group => group.StartDate).IsRequired();
            entity.Property(group => group.EndDate).IsRequired(false);
        });
    }

    private static void ConfigureGroupStudents(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GroupStudentEntity>(entity =>
        {
            entity.ToTable("group_students");
            entity.HasKey(membership => new
            {
                membership.PersonID,
                membership.EdGroupID,
                membership.StartDate
            });

            entity.Property(membership => membership.EdGroupID).IsRequired();
            entity.Property(membership => membership.StartDate).IsRequired();
            entity.Property(membership => membership.EndDate).IsRequired(false);

            entity.HasOne(membership => membership.Student)
                .WithMany(person => person.GroupMemberships)
                .HasForeignKey(membership => membership.PersonID)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(membership => membership.EducationalGroup)
                .WithMany(group => group.Students)
                .HasForeignKey(membership => membership.EdGroupID)
                .HasPrincipalKey(group => group.GroupName)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
