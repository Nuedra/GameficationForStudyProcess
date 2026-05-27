namespace Platform.Core.Appraisals;

public sealed class StudentCourseFacts
{
    public required Guid StudentId { get; init; }
    public required Guid CourseId { get; init; }
    public required int Year { get; init; }
    public required IReadOnlyList<MarkFact> Marks { get; init; }
}
