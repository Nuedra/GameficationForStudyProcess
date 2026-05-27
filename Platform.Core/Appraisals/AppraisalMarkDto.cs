namespace Platform.Core.Appraisals;

public sealed class AppraisalMarkDto
{
    public required Guid ColumnId { get; init; }
    public required string ColumnName { get; init; }
    public required bool IsComputed { get; init; }
    public required decimal MaxScore { get; init; }
    public required decimal MinAcceptScore { get; init; }
    public decimal? Score { get; init; }
    public string? ScoreSourceName { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}
