using System.Globalization;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Platform.Application.Contracts;
using Platform.Core.Models;
using Platform.DataAccess.Postgress;
using Platform.Lms;

namespace Platform.Application.Services;

public sealed class StaffCourseExportService(
    IStaffCourseService staffCourseService,
    ILmsDataSource lmsDataSource,
    AchievementDbContext dbContext,
    IAchievementGraphTemplateProvider graphTemplateProvider,
    TimeProvider timeProvider,
    ILogger<StaffCourseExportService> logger) : IStaffCourseExportService
{
    private const string CsvContentType = "text/csv; charset=utf-8";
    private const string ZipContentType = "application/zip";
    private const string ExportFormat = "achievement-platform-course-export";
    private const string SchemaVersion = "1.0";

    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions(writeIndented: true);
    private static readonly JsonSerializerOptions JsonLinesOptions =
        CreateJsonOptions(writeIndented: false);

    public async Task<StaffCourseExportResult> CreateTeacherReportAsync(
        Guid userId,
        UserRole role,
        Guid courseId,
        int year,
        CancellationToken cancellationToken = default)
    {
        if (role != UserRole.Teacher)
            return new StaffCourseExportResult(StaffCourseExportStatus.AccessDenied);

        var snapshotResult = await LoadSnapshotAsync(
            userId,
            role,
            courseId,
            year,
            includeAudit: false,
            cancellationToken);
        if (snapshotResult.Status != StaffCourseExportStatus.Success)
            return new StaffCourseExportResult(snapshotResult.Status);

        var snapshot = snapshotResult.Snapshot!;
        var content = BuildTeacherReportCsv(snapshot);
        var activeStudentIds = snapshot.Students
            .Where(student => student.IsActiveEnrollment)
            .Select(student => student.Id)
            .ToHashSet();
        logger.LogInformation(
            "Teacher course achievement report exported. CourseId={CourseId}, Year={Year}, ExportedBy={ExportedBy}, ExportedByRole={ExportedByRole}, ActiveStudentCount={ActiveStudentCount}, AwardCount={AwardCount}, ContentLength={ContentLength}",
            courseId,
            year,
            userId,
            role,
            activeStudentIds.Count,
            snapshot.Awards.Count(award => activeStudentIds.Contains(award.StudentId)),
            content.Length);

        return new StaffCourseExportResult(
            StaffCourseExportStatus.Success,
            content,
            CsvContentType,
            $"course-achievements-{courseId:D}-{year}.csv");
    }

    public async Task<StaffCourseExportResult> CreateCourseArchiveAsync(
        Guid userId,
        UserRole role,
        Guid courseId,
        int year,
        CancellationToken cancellationToken = default)
    {
        var snapshotResult = await LoadSnapshotAsync(
            userId,
            role,
            courseId,
            year,
            includeAudit: true,
            cancellationToken);
        if (snapshotResult.Status != StaffCourseExportStatus.Success)
            return new StaffCourseExportResult(snapshotResult.Status);

        var snapshot = snapshotResult.Snapshot!;
        var graphTemplate = await graphTemplateProvider.GetTemplateAsync(cancellationToken);
        var graphXml = BuildCourseGraphXml(
            graphTemplate,
            snapshot.Achievements.Select(achievement => achievement.Id).ToHashSet());
        var exportId = Guid.NewGuid();
        var archive = await BuildArchiveAsync(
            snapshot,
            graphXml,
            exportId,
            userId,
            role,
            cancellationToken);
        logger.LogInformation(
            "Course archive exported. ExportId={ExportId}, CourseId={CourseId}, Year={Year}, ExportedBy={ExportedBy}, ExportedByRole={ExportedByRole}, StudentCount={StudentCount}, ActiveStudentCount={ActiveStudentCount}, AchievementCount={AchievementCount}, AwardCount={AwardCount}, AuditEventCount={AuditEventCount}, ContentLength={ContentLength}",
            exportId,
            courseId,
            year,
            userId,
            role,
            snapshot.Students.Count,
            snapshot.Students.Count(student => student.IsActiveEnrollment),
            snapshot.Achievements.Count,
            snapshot.Awards.Count,
            snapshot.AuditEvents.Count,
            archive.Length);

        return new StaffCourseExportResult(
            StaffCourseExportStatus.Success,
            archive,
            ZipContentType,
            $"course-export-{courseId:D}-{year}.zip");
    }

    private async Task<SnapshotResult> LoadSnapshotAsync(
        Guid userId,
        UserRole role,
        Guid courseId,
        int year,
        bool includeAudit,
        CancellationToken cancellationToken)
    {
        var courseResult = await staffCourseService.GetCourseAsync(
            userId,
            role,
            courseId,
            year,
            cancellationToken);
        if (courseResult.Status != StaffCourseQueryStatus.Success)
        {
            return new SnapshotResult(
                courseResult.Status == StaffCourseQueryStatus.CourseNotFound
                    ? StaffCourseExportStatus.CourseNotFound
                    : StaffCourseExportStatus.AccessDenied);
        }

        var generatedAt = timeProvider.GetUtcNow();
        var students = await lmsDataSource.GetActiveCourseInstanceStudentsAsync(
            courseId,
            year,
            generatedAt,
            cancellationToken);

        var achievements = await dbContext.Achievements
            .AsNoTracking()
            .Where(achievement =>
                achievement.CourseID == courseId &&
                achievement.Year == year)
            .OrderBy(achievement => achievement.Title)
            .ThenBy(achievement => achievement.Id)
            .Select(achievement => new ExportAchievement(
                achievement.Id,
                achievement.Title,
                achievement.Description,
                achievement.Rarity,
                achievement.Track,
                achievement.LabID,
                achievement.Criteria == null
                    ? null
                    : new ExportAchievementCriteria(
                        achievement.Criteria.Id,
                        achievement.Criteria.Expression,
                        achievement.Criteria.Scope,
                        achievement.Criteria.IsEnabled)))
            .ToListAsync(cancellationToken);

        var achievementIds = achievements
            .Select(achievement => achievement.Id)
            .ToHashSet();

        var connections = achievementIds.Count == 0
            ? []
            : await dbContext.AchievementConnections
                .AsNoTracking()
                .Where(connection =>
                    achievementIds.Contains(connection.SourceId) &&
                    achievementIds.Contains(connection.TargetId))
                .OrderBy(connection => connection.SourceId)
                .ThenBy(connection => connection.TargetId)
                .Select(connection => new ExportConnection(
                    connection.Id,
                    connection.SourceId,
                    connection.TargetId))
                .ToListAsync(cancellationToken);

        var awards = achievementIds.Count == 0
            ? []
            : await dbContext.StudentAchievements
                .AsNoTracking()
                .Where(award => achievementIds.Contains(award.AchievementID))
                .OrderBy(award => award.StudentID)
                .ThenBy(award => award.AchievementGotDate)
                .Select(award => new ExportAward(
                    award.Id,
                    award.StudentID,
                    award.AchievementID,
                    award.AchievementGotDate,
                    award.AchievementFoundDate,
                    award.LabID,
                    award.IsNotificationSeen,
                    award.IsFirstAnimationShown))
                .ToListAsync(cancellationToken);

        var auditEvents = includeAudit
            ? await dbContext.AchievementAwardAuditEvents
                .AsNoTracking()
                .Where(auditEvent =>
                    auditEvent.CourseID == courseId &&
                    auditEvent.Year == year)
                .OrderBy(auditEvent => auditEvent.OccurredAt)
                .ThenBy(auditEvent => auditEvent.Id)
                .Select(auditEvent => new ExportAuditEvent(
                    auditEvent.Id,
                    auditEvent.AwardID,
                    auditEvent.EventType,
                    auditEvent.OccurredAt,
                    auditEvent.AwardedAt,
                    auditEvent.StudentID,
                    auditEvent.AchievementID,
                    auditEvent.AchievementTitle,
                    auditEvent.ActorID,
                    auditEvent.ActorRole,
                    auditEvent.Reason,
                    auditEvent.CriterionExpression,
                    auditEvent.CriterionScope))
                .ToListAsync(cancellationToken)
            : [];

        var exportStudentsById = students
            .Select(student => new ExportStudent(
                student.Id,
                student.DisplayName,
                student.CurrentEducationalGroupName,
                IsActiveEnrollment: true,
                IsPresentInLms: true))
            .ToDictionary(student => student.Id);

        if (includeAudit)
        {
            var referencedStudentIds = awards
                .Select(award => award.StudentId)
                .Concat(auditEvents.Select(auditEvent => auditEvent.StudentId))
                .Distinct()
                .Where(studentId => !exportStudentsById.ContainsKey(studentId))
                .OrderBy(studentId => studentId)
                .ToList();

            foreach (var studentId in referencedStudentIds)
            {
                var person = await lmsDataSource.GetPersonAsync(studentId, cancellationToken);
                exportStudentsById[studentId] = person is null
                    ? new ExportStudent(
                        studentId,
                        FullName: null,
                        Group: null,
                        IsActiveEnrollment: false,
                        IsPresentInLms: false)
                    : new ExportStudent(
                        person.Id,
                        person.DisplayName,
                        person.CurrentEducationalGroupName,
                        IsActiveEnrollment: false,
                        IsPresentInLms: true);
            }
        }

        var exportStudents = exportStudentsById.Values
            .OrderByDescending(student => student.IsActiveEnrollment)
            .ThenBy(student => student.FullName ?? string.Empty, StringComparer.Ordinal)
            .ThenBy(student => student.Id)
            .ToList();

        return new SnapshotResult(
            StaffCourseExportStatus.Success,
            new CourseExportSnapshot(
                generatedAt,
                courseResult.Course!,
                exportStudents,
                achievements,
                connections,
                awards,
                auditEvents));
    }

    private static byte[] BuildTeacherReportCsv(CourseExportSnapshot snapshot)
    {
        var builder = new StringBuilder();
        AppendCsvRow(
            builder,
            "StudentId",
            "ФИО",
            "Группа",
            "AchievementId",
            "Достижение",
            "Редкость",
            "Дата получения (UTC)");

        var activeStudents = snapshot.Students
            .Where(student => student.IsActiveEnrollment)
            .ToList();
        var activeStudentIds = activeStudents.Select(student => student.Id).ToHashSet();
        var achievementsById = snapshot.Achievements.ToDictionary(item => item.Id);
        var awardsByStudent = snapshot.Awards
            .Where(award => activeStudentIds.Contains(award.StudentId))
            .GroupBy(award => award.StudentId)
            .ToDictionary(group => group.Key, group => group.ToList());

        foreach (var student in activeStudents)
        {
            if (!awardsByStudent.TryGetValue(student.Id, out var studentAwards) ||
                studentAwards.Count == 0)
            {
                AppendCsvRow(
                    builder,
                    student.Id.ToString("D"),
                    student.FullName ?? string.Empty,
                    student.Group ?? string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty);
                continue;
            }

            foreach (var award in studentAwards
                         .OrderBy(award => award.AwardedAt)
                         .ThenBy(award => award.AchievementId))
            {
                if (!achievementsById.TryGetValue(award.AchievementId, out var achievement))
                    continue;

                AppendCsvRow(
                    builder,
                    student.Id.ToString("D"),
                    student.FullName ?? string.Empty,
                    student.Group ?? string.Empty,
                    achievement.Id.ToString("D"),
                    achievement.Title,
                    RarityLabel(achievement.Rarity),
                    AsUtc(award.AwardedAt).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
            }
        }

        return EncodeCsv(builder.ToString());
    }

    private static byte[] BuildLeaderboardCsv(CourseExportSnapshot snapshot)
    {
        var builder = new StringBuilder();
        AppendCsvRow(builder, "Место", "StudentId", "ФИО", "Группа", "Количество достижений");

        var activeStudents = snapshot.Students
            .Where(student => student.IsActiveEnrollment)
            .ToList();
        var activeStudentIds = activeStudents.Select(student => student.Id).ToHashSet();
        var countsByStudent = snapshot.Awards
            .Where(award => activeStudentIds.Contains(award.StudentId))
            .GroupBy(award => award.StudentId)
            .ToDictionary(group => group.Key, group => group.Count());

        var leaderboard = activeStudents
            .Select(student => new
            {
                Student = student,
                AchievementCount = countsByStudent.GetValueOrDefault(student.Id)
            })
            .OrderByDescending(item => item.AchievementCount)
            .ThenBy(item => item.Student.FullName ?? string.Empty, StringComparer.Ordinal)
            .ThenBy(item => item.Student.Id)
            .ToList();

        for (var index = 0; index < leaderboard.Count; index++)
        {
            var entry = leaderboard[index];
            AppendCsvRow(
                builder,
                (index + 1).ToString(CultureInfo.InvariantCulture),
                entry.Student.Id.ToString("D"),
                entry.Student.FullName ?? string.Empty,
                entry.Student.Group ?? string.Empty,
                entry.AchievementCount.ToString(CultureInfo.InvariantCulture));
        }

        return EncodeCsv(builder.ToString());
    }

    private static async Task<byte[]> BuildArchiveAsync(
        CourseExportSnapshot snapshot,
        string graphXml,
        Guid exportId,
        Guid exportedBy,
        UserRole exportedByRole,
        CancellationToken cancellationToken)
    {
        var files = new List<ArchiveFile>
        {
            new(
                "README.txt",
                "text/plain; charset=utf-8",
                EncodeUtf8(BuildReadme()),
                1),
            new(
                "achievements.json",
                "application/json",
                SerializeJson(snapshot.Achievements),
                snapshot.Achievements.Count),
            new(
                "connections.json",
                "application/json",
                SerializeJson(snapshot.Connections),
                snapshot.Connections.Count),
            new(
                "students.json",
                "application/json",
                SerializeJson(snapshot.Students),
                snapshot.Students.Count),
            new(
                "awards.json",
                "application/json",
                SerializeJson(snapshot.Awards),
                snapshot.Awards.Count),
            new(
                "audit-events.jsonl",
                "application/x-ndjson",
                SerializeJsonLines(snapshot.AuditEvents),
                snapshot.AuditEvents.Count),
            new(
                "leaderboard.csv",
                CsvContentType,
                BuildLeaderboardCsv(snapshot),
                snapshot.Students.Count(student => student.IsActiveEnrollment)),
            new(
                "graph.xml",
                "application/xml",
                EncodeUtf8(graphXml),
                1)
        };

        var manifest = new ExportManifest(
            ExportFormat,
            SchemaVersion,
            exportId,
            snapshot.GeneratedAt,
            new ExportScope(snapshot.Course.Id, snapshot.Course.Year),
            new ExportCourse(snapshot.Course.Title, snapshot.Course.Description),
            new ExportActor(exportedBy, exportedByRole),
            ContainsPersonalData: true,
            files.Select(file => new ExportManifestFile(
                    file.Path,
                    file.MediaType,
                    file.RecordCount,
                    Convert.ToHexString(SHA256.HashData(file.Content)).ToLowerInvariant()))
                .ToList());
        files.Insert(
            0,
            new ArchiveFile(
                "manifest.json",
                "application/json",
                SerializeJson(manifest),
                1));

        await using var output = new MemoryStream();
        using (var archive = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var file in files)
            {
                var entry = archive.CreateEntry(file.Path, CompressionLevel.Optimal);
                await using var entryStream = entry.Open();
                await entryStream.WriteAsync(file.Content, cancellationToken);
            }
        }

        return output.ToArray();
    }

    private static string BuildCourseGraphXml(
        string template,
        IReadOnlySet<Guid> courseAchievementIds)
    {
        if (string.IsNullOrWhiteSpace(template))
            throw new InvalidDataException("Achievement graph XML template is empty.");

        var graphStart = template.IndexOf("<graph", StringComparison.Ordinal);
        var graphEnd = template.LastIndexOf("</graph>", StringComparison.Ordinal);
        if (graphStart < 0 || graphEnd < graphStart)
            throw new InvalidDataException("Achievement graph template has no graph XML block.");

        var graphXml = template[graphStart..(graphEnd + "</graph>".Length)];
        XDocument document;
        try
        {
            document = XDocument.Parse(graphXml, LoadOptions.PreserveWhitespace);
        }
        catch (Exception exception)
        {
            throw new InvalidDataException("Achievement graph XML template is invalid.", exception);
        }

        var root = document.Root;
        if (root is null || root.Name.LocalName != "graph")
            throw new InvalidDataException("Achievement graph XML must have a graph root element.");

        var retainedNodeIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var node in root.Elements().Where(element => element.Name.LocalName == "node").ToList())
        {
            var achievementId = GetGraphAchievementId(node);
            if (!achievementId.HasValue || !courseAchievementIds.Contains(achievementId.Value))
            {
                node.Remove();
                continue;
            }

            var nodeId = node.Attribute("id")?.Value;
            if (!string.IsNullOrWhiteSpace(nodeId))
                retainedNodeIds.Add(nodeId);
        }

        foreach (var edge in root.Elements().Where(element => element.Name.LocalName == "edge").ToList())
        {
            var source = edge.Attribute("source")?.Value;
            var target = edge.Attribute("target")?.Value;
            if (string.IsNullOrWhiteSpace(source) ||
                string.IsNullOrWhiteSpace(target) ||
                !retainedNodeIds.Contains(source) ||
                !retainedNodeIds.Contains(target))
            {
                edge.Remove();
            }
        }

        return document.ToString(SaveOptions.DisableFormatting);
    }

    private static Guid? GetGraphAchievementId(XElement node)
    {
        foreach (var attributeName in new[]
                 {
                     "achievementId",
                     "AchievementId",
                     "achievement-id",
                     "data-achievement-id",
                     "achivementId",
                     "AchivementId"
                 })
        {
            if (Guid.TryParse(node.Attribute(attributeName)?.Value, out var achievementId))
                return achievementId;
        }

        return null;
    }

    private static byte[] SerializeJson<T>(T value) =>
        JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);

    private static byte[] SerializeJsonLines(IReadOnlyList<ExportAuditEvent> auditEvents)
    {
        if (auditEvents.Count == 0)
            return [];

        var builder = new StringBuilder();
        foreach (var auditEvent in auditEvents)
            builder.AppendLine(JsonSerializer.Serialize(auditEvent, JsonLinesOptions));

        return EncodeUtf8(builder.ToString());
    }

    private static void AppendCsvRow(StringBuilder builder, params string[] values)
    {
        builder.AppendJoin(';', values.Select(EscapeCsv));
        builder.Append("\r\n");
    }

    private static string EscapeCsv(string value)
    {
        var safeValue = value.Length > 0 && value[0] is '=' or '+' or '-' or '@' or '\t' or '\r'
            ? $"'{value}"
            : value;
        return $"\"{safeValue.Replace("\"", "\"\"")}\"";
    }

    private static byte[] EncodeCsv(string value)
    {
        var preamble = Encoding.UTF8.GetPreamble();
        var content = Encoding.UTF8.GetBytes(value);
        var result = new byte[preamble.Length + content.Length];
        preamble.CopyTo(result, 0);
        content.CopyTo(result, preamble.Length);
        return result;
    }

    private static byte[] EncodeUtf8(string value) => Encoding.UTF8.GetBytes(value);

    private static DateTime AsUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };

    private static string RarityLabel(AchievementRarity rarity) => rarity switch
    {
        AchievementRarity.Common => "Обычная",
        AchievementRarity.Rare => "Редкая",
        AchievementRarity.Epic => "Эпическая",
        AchievementRarity.Legendary => "Легендарная",
        _ => rarity.ToString()
    };

    private static string BuildReadme() =>
        """
        Полная выгрузка экземпляра курса платформы достижений.

        Формат: achievement-platform-course-export
        Версия схемы: 1.0
        Кодировка текстовых файлов: UTF-8
        Даты JSON: ISO 8601, UTC
        Идентификаторы: UUID

        manifest.json       - описание выгрузки, состав и SHA-256 файлов
        achievements.json   - достижения и критерии
        connections.json    - направленные связи графа
        students.json       - активные студенты и исторически связанные пользователи; признаки isActiveEnrollment и isPresentInLms описывают их состояние
        awards.json         - текущие выдачи достижений курса
        audit-events.jsonl  - история выдач, отзывов и отказов, одно событие на строку
        leaderboard.csv     - таблица лидеров активных студентов
        graph.xml           - визуальный XML-граф, отфильтрованный по достижениям курса

        Файл содержит персональные данные и должен храниться с ограничением доступа.
        """;

    private static JsonSerializerOptions CreateJsonOptions(bool writeIndented)
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = writeIndented
        };
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        return options;
    }

    private sealed record SnapshotResult(
        StaffCourseExportStatus Status,
        CourseExportSnapshot? Snapshot = null);

    private sealed record CourseExportSnapshot(
        DateTimeOffset GeneratedAt,
        CourseDto Course,
        IReadOnlyList<ExportStudent> Students,
        IReadOnlyList<ExportAchievement> Achievements,
        IReadOnlyList<ExportConnection> Connections,
        IReadOnlyList<ExportAward> Awards,
        IReadOnlyList<ExportAuditEvent> AuditEvents);

    private sealed record ExportAchievement(
        Guid Id,
        string Title,
        string Description,
        AchievementRarity Rarity,
        string Track,
        Guid? LabId,
        ExportAchievementCriteria? Criteria);

    private sealed record ExportAchievementCriteria(
        Guid Id,
        string Expression,
        AchievementCriteriaScope Scope,
        bool IsEnabled);

    private sealed record ExportConnection(
        Guid Id,
        Guid SourceAchievementId,
        Guid TargetAchievementId);

    private sealed record ExportStudent(
        Guid Id,
        string? FullName,
        string? Group,
        bool IsActiveEnrollment,
        bool IsPresentInLms);

    private sealed record ExportAward(
        Guid Id,
        Guid StudentId,
        Guid AchievementId,
        DateTime AwardedAt,
        DateTime FoundAt,
        Guid? LabId,
        bool IsNotificationSeen,
        bool IsFirstAnimationShown);

    private sealed record ExportAuditEvent(
        Guid Id,
        Guid? AwardId,
        AchievementAwardAuditEventType EventType,
        DateTime OccurredAt,
        DateTime? AwardedAt,
        Guid StudentId,
        Guid AchievementId,
        string AchievementTitle,
        Guid? ActorId,
        AchievementAwardAuditActorRole ActorRole,
        AchievementAwardAuditReason Reason,
        string? CriterionExpression,
        AchievementCriteriaScope? CriterionScope);

    private sealed record ExportManifest(
        string Format,
        string SchemaVersion,
        Guid ExportId,
        DateTimeOffset GeneratedAt,
        ExportScope Scope,
        ExportCourse Course,
        ExportActor ExportedBy,
        bool ContainsPersonalData,
        IReadOnlyList<ExportManifestFile> Files);

    private sealed record ExportScope(Guid CourseId, int Year);

    private sealed record ExportCourse(string Title, string Description);

    private sealed record ExportActor(Guid Id, UserRole Role);

    private sealed record ExportManifestFile(
        string Path,
        string MediaType,
        int RecordCount,
        string Sha256);

    private sealed record ArchiveFile(
        string Path,
        string MediaType,
        byte[] Content,
        int RecordCount);
}
