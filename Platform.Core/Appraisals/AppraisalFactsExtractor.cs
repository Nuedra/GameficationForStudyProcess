namespace Platform.Core.Appraisals;

public sealed class AppraisalFactsExtractor : IAppraisalFactsExtractor
{
    public StudentCourseFacts Extract(AppraisalPayloadDto payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        var marks = new List<MarkFact>();

        foreach (var list in payload.AppraisalLists)
        {
            foreach (var mark in list.Marks)
            {
                marks.Add(new MarkFact
                {
                    ListId = list.ListId,
                    ListName = list.ListName,
                    DateCreated = list.DateCreated,
                    DateClosed = list.DateClosed,
                    ColumnId = mark.ColumnId,
                    ColumnName = mark.ColumnName,
                    IsComputed = mark.IsComputed,
                    MaxScore = mark.MaxScore,
                    MinAcceptScore = mark.MinAcceptScore,
                    Score = mark.Score,
                    ScoreSourceName = mark.ScoreSourceName,
                    UpdatedAt = mark.UpdatedAt,
                    Tags = BuildTags(mark),
                    Deadline = mark.Deadline,
                    UploadedAt = mark.UploadedAt
                });
            }
        }

        return new StudentCourseFacts
        {
            StudentId = payload.StudentId,
            CourseId = payload.CourseId,
            Year = payload.Year,
            Marks = marks
        };
    }

    private static IReadOnlyList<string> BuildTags(AppraisalMarkDto mark)
    {
        var tags = mark.Tags.ToHashSet(StringComparer.Ordinal);

        if (mark.UploadedAt.HasValue &&
            mark.Deadline.HasValue)
        {
            tags.Add(mark.UploadedAt.Value <= mark.Deadline.Value
                ? "intime"
                : "expired");
        }

        if (mark.Score.HasValue)
        {
            if (mark.Score.Value >= mark.MinAcceptScore)
                tags.Add("passed");

            var scorePercent = mark.Score.Value / mark.MaxScore * 100;
            switch (scorePercent)
            {
                case >= 100:
                    tags.Add("maxscore");
                    break;
                case >= 90:
                    tags.Add("highscore");
                    break;
                case >= 80:
                    tags.Add("goodscore");
                    break;
                case >= 70:
                    tags.Add("mediumscore");
                    break;
                case >= 60:
                    tags.Add("lowscore");
                    break;
            }
        }

        return tags.ToList();
    }
}
