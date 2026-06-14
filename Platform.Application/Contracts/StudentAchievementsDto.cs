namespace Platform.Application.Contracts;

public sealed record StudentAchievementsDto(
    StudentDto Student,
    CourseDto Course,
    IReadOnlyList<AchievementDto> Achievements);
