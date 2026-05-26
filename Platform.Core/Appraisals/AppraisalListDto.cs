namespace Platform.Core.Appraisals;

public sealed class AppraisalListDto
{
    public required Guid ListId { get; init; }
    public required string ListName { get; init; }
    public required DateTimeOffset DateCreated { get; init; }
    public DateTimeOffset? DateClosed { get; init; }
    public required List<AppraisalMarkDto> Marks { get; init; }
}
