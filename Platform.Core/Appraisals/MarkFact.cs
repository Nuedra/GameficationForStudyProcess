namespace Platform.Core.Appraisals;

public sealed class MarkFact
{
    public required Guid ListId { get; init; }
    public required string ListName { get; init; }
    public required DateTimeOffset DateCreated { get; init; }
    public DateTimeOffset? DateClosed { get; init; }

    public required Guid ColumnId { get; init; }
    public required string ColumnName { get; init; }
    public required bool IsComputed { get; init; }

    public required decimal MaxScore { get; init; }
    public required decimal MinAcceptScore { get; init; }
    public decimal? Score { get; init; }
    public string? ScoreSourceName { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }

    public bool IsSet => Score.HasValue;

    public bool IsPassed => Score.HasValue && Score.Value >= MinAcceptScore;

    public decimal? ScorePercent => Score.HasValue && MaxScore > 0
        ? Score.Value / MaxScore * 100
        : null;
}
