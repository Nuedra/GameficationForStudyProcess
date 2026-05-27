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
                    UpdatedAt = mark.UpdatedAt
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
}
