namespace Platform.Core.Appraisals;

public sealed class AppraisalPayloadDto
{
    public required Guid StudentId { get; init; }
    public required Guid CourseId { get; init; }
    public required int Year { get; init; }
    public required List<AppraisalListDto> AppraisalLists { get; init; }
}
