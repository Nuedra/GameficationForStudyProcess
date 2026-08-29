using Microsoft.EntityFrameworkCore;

namespace Platform.DataAccess.Postgress;

/// <summary>
/// Контекст принадлежащих платформе данных о достижениях.
/// Идентификаторы людей и курсов являются внешними ссылками на LMS.
/// </summary>
public sealed class AchievementDbContext(DbContextOptions<AchievementDbContext> options)
    : DbContext(options)
{
    public DbSet<AchievementEntity> Achievements => Set<AchievementEntity>();
    public DbSet<StudentAchievementEntity> StudentAchievements => Set<StudentAchievementEntity>();
    public DbSet<AchievementCriteriaEntity> AchievementCriterias => Set<AchievementCriteriaEntity>();
    public DbSet<AchievementConnectionEntity> AchievementConnections => Set<AchievementConnectionEntity>();
    public DbSet<AchievementAwardAuditEventEntity> AchievementAwardAuditEvents =>
        Set<AchievementAwardAuditEventEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureAchievements(modelBuilder);
        ConfigureStudentAchievements(modelBuilder);
        ConfigureAchievementCriteria(modelBuilder);
        ConfigureAchievementConnections(modelBuilder);
        ConfigureAchievementAwardAuditEvents(modelBuilder);
    }

    private static void ConfigureAchievements(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AchievementEntity>(entity =>
        {
            entity.ToTable("achievements");
            entity.HasKey(achievement => achievement.Id);

            entity.Property(achievement => achievement.Title).IsRequired();
            entity.Property(achievement => achievement.Description).IsRequired(false);
            entity.Property(achievement => achievement.CourseID).IsRequired();
            entity.Property(achievement => achievement.Year).IsRequired();
            entity.Property(achievement => achievement.Rarity)
                .HasConversion(
                    rarity => rarity.ToString().ToLowerInvariant(),
                    value => Enum.Parse<AchievementRarity>(value, true))
                .IsRequired()
                .HasDefaultValue(AchievementRarity.Common);
            entity.Property(achievement => achievement.Track)
                .IsRequired()
                .HasDefaultValue("default");
            entity.Property(achievement => achievement.LabID).IsRequired(false);
            entity.HasIndex(achievement => achievement.LabID);
            entity.ToTable(table => table.HasCheckConstraint(
                "CK_achievements_Rarity",
                "\"Rarity\" IN ('common', 'rare', 'epic', 'legendary')"));

            entity.HasMany(achievement => achievement.StudentAchievements)
                .WithOne(studentAchievement => studentAchievement.Achievement)
                .HasForeignKey(studentAchievement => studentAchievement.AchievementID)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(achievement => achievement.Criteria)
                .WithOne(criteria => criteria.Achievement)
                .HasForeignKey<AchievementCriteriaEntity>(criteria => criteria.AchievementID)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureStudentAchievements(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StudentAchievementEntity>(entity =>
        {
            entity.ToTable("student_achievements");
            entity.HasKey(studentAchievement => studentAchievement.Id);

            entity.Property(studentAchievement => studentAchievement.StudentID).IsRequired();
            entity.Property(studentAchievement => studentAchievement.AchievementGotDate).IsRequired();
            entity.Property(studentAchievement => studentAchievement.AchievementFoundDate).IsRequired();
            entity.Property(studentAchievement => studentAchievement.IsNotificationSeen).IsRequired();
            entity.Property(studentAchievement => studentAchievement.IsFirstAnimationShown).IsRequired();
            entity.Property(studentAchievement => studentAchievement.LabID).IsRequired(false);
            entity.HasIndex(studentAchievement => studentAchievement.LabID);
            entity.HasIndex(studentAchievement => new
                {
                    studentAchievement.StudentID,
                    studentAchievement.AchievementID
                })
                .IsUnique();
        });
    }

    private static void ConfigureAchievementCriteria(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AchievementCriteriaEntity>(entity =>
        {
            entity.ToTable("achievement_criterias");
            entity.HasKey(criteria => criteria.Id);

            entity.Property(criteria => criteria.Expression).IsRequired();
            entity.Property(criteria => criteria.IsEnabled).IsRequired();
            entity.Property(criteria => criteria.Scope)
                .HasConversion<string>()
                .IsRequired()
                .HasDefaultValue(AchievementCriteriaScope.SameMark);
            entity.ToTable(table => table.HasCheckConstraint(
                "CK_achievement_criterias_Scope",
                "\"Scope\" IN ('SameMark', 'AcrossCourse', 'AllLabs')"));
        });
    }

    private static void ConfigureAchievementConnections(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AchievementConnectionEntity>(entity =>
        {
            entity.ToTable("achievement_connections");
            entity.HasKey(connection => connection.Id);

            entity.HasOne(connection => connection.Source)
                .WithMany()
                .HasForeignKey(connection => connection.SourceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(connection => connection.Target)
                .WithMany()
                .HasForeignKey(connection => connection.TargetId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(connection => new { connection.SourceId, connection.TargetId })
                .IsUnique();
        });
    }

    private static void ConfigureAchievementAwardAuditEvents(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AchievementAwardAuditEventEntity>(entity =>
        {
            entity.ToTable("achievement_award_audit_events");
            entity.HasKey(auditEvent => auditEvent.Id);

            entity.Property(auditEvent => auditEvent.AwardID).IsRequired(false);
            entity.Property(auditEvent => auditEvent.EventType)
                .HasConversion<string>()
                .IsRequired();
            entity.Property(auditEvent => auditEvent.OccurredAt).IsRequired();
            entity.Property(auditEvent => auditEvent.AwardedAt).IsRequired(false);
            entity.Property(auditEvent => auditEvent.StudentID).IsRequired();
            entity.Property(auditEvent => auditEvent.AchievementID).IsRequired();
            entity.Property(auditEvent => auditEvent.AchievementTitle).IsRequired();
            entity.Property(auditEvent => auditEvent.CourseID).IsRequired();
            entity.Property(auditEvent => auditEvent.Year).IsRequired();
            entity.Property(auditEvent => auditEvent.ActorID).IsRequired(false);
            entity.Property(auditEvent => auditEvent.ActorRole)
                .HasConversion<string>()
                .IsRequired();
            entity.Property(auditEvent => auditEvent.Reason)
                .HasConversion<string>()
                .IsRequired();
            entity.Property(auditEvent => auditEvent.CriterionExpression).IsRequired(false);
            entity.Property(auditEvent => auditEvent.CriterionScope)
                .HasConversion<string>()
                .IsRequired(false);

            entity.HasIndex(auditEvent => new
            {
                auditEvent.CourseID,
                auditEvent.Year,
                auditEvent.OccurredAt
            });
            entity.HasIndex(auditEvent => new
            {
                auditEvent.StudentID,
                auditEvent.OccurredAt
            });
            entity.HasIndex(auditEvent => new
            {
                auditEvent.AchievementID,
                auditEvent.OccurredAt
            });

            entity.ToTable(table =>
            {
                table.HasCheckConstraint(
                    "CK_achievement_award_audit_events_EventType",
                    "\"EventType\" IN ('Granted', 'Revoked', 'Rejected')");
                table.HasCheckConstraint(
                    "CK_achievement_award_audit_events_ActorRole",
                    "\"ActorRole\" IN ('System', 'Teacher', 'Administrator')");
                table.HasCheckConstraint(
                    "CK_achievement_award_audit_events_Reason",
                    "\"Reason\" IN ('CriteriaMatched', 'ManualGrant', 'ManualRevocation', 'AchievementDeletion', 'PrerequisiteRevocation', 'ManualGrantStudentNotFound', 'ManualGrantEnrollmentMissing', 'ManualGrantAlreadyExists', 'ManualGrantPrerequisiteMissing')");
                table.HasCheckConstraint(
                    "CK_achievement_award_audit_events_CriterionScope",
                    "\"CriterionScope\" IS NULL OR \"CriterionScope\" IN ('SameMark', 'AcrossCourse', 'AllLabs')");
                table.HasCheckConstraint(
                    "CK_achievement_award_audit_events_AwardSnapshot",
                    "\"EventType\" = 'Rejected' OR (\"AwardID\" IS NOT NULL AND \"AwardedAt\" IS NOT NULL)");
            });
        });
    }
}
