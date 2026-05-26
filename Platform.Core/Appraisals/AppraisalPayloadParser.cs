using System.Text.Json;

namespace Platform.Core.Appraisals;

public sealed class AppraisalPayloadParser : IAppraisalPayloadParser
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AppraisalPayloadDto Parse(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new AppraisalPayloadException("Appraisal payload JSON is empty.");

        AppraisalPayloadDto? payload;

        try
        {
            payload = JsonSerializer.Deserialize<AppraisalPayloadDto>(json, SerializerOptions);
        }
        catch (JsonException ex)
        {
            throw new AppraisalPayloadException("Appraisal payload JSON is invalid.", ex);
        }

        if (payload is null)
            throw new AppraisalPayloadException("Appraisal payload JSON is empty.");

        Validate(payload);
        return payload;
    }

    private static void Validate(AppraisalPayloadDto payload)
    {
        if (payload.StudentId == Guid.Empty)
            throw new AppraisalPayloadException("studentId is required.");

        if (payload.CourseId == Guid.Empty)
            throw new AppraisalPayloadException("courseId is required.");

        if (payload.Year <= 0)
            throw new AppraisalPayloadException("year must be greater than zero.");

        if (payload.AppraisalLists is null)
            throw new AppraisalPayloadException("appraisalLists is required.");

        for (var listIndex = 0; listIndex < payload.AppraisalLists.Count; listIndex++)
        {
            var list = payload.AppraisalLists[listIndex];
            if (list is null)
                throw new AppraisalPayloadException($"appraisalLists[{listIndex}] cannot be null.");

            ValidateList(list, listIndex);
        }
    }

    private static void ValidateList(AppraisalListDto list, int listIndex)
    {
        var prefix = $"appraisalLists[{listIndex}]";

        if (list.ListId == Guid.Empty)
            throw new AppraisalPayloadException($"{prefix}.listId is required.");

        if (string.IsNullOrWhiteSpace(list.ListName))
            throw new AppraisalPayloadException($"{prefix}.listName is required.");

        if (list.Marks is null)
            throw new AppraisalPayloadException($"{prefix}.marks is required.");

        for (var markIndex = 0; markIndex < list.Marks.Count; markIndex++)
        {
            var mark = list.Marks[markIndex];
            if (mark is null)
                throw new AppraisalPayloadException($"{prefix}.marks[{markIndex}] cannot be null.");

            ValidateMark(mark, listIndex, markIndex);
        }
    }

    private static void ValidateMark(AppraisalMarkDto mark, int listIndex, int markIndex)
    {
        var prefix = $"appraisalLists[{listIndex}].marks[{markIndex}]";

        if (mark.ColumnId == Guid.Empty)
            throw new AppraisalPayloadException($"{prefix}.columnId is required.");

        if (string.IsNullOrWhiteSpace(mark.ColumnName))
            throw new AppraisalPayloadException($"{prefix}.columnName is required.");

        if (mark.MaxScore <= 0)
            throw new AppraisalPayloadException($"{prefix}.maxScore must be greater than zero.");

        if (mark.MinAcceptScore < 0)
            throw new AppraisalPayloadException($"{prefix}.minAcceptScore cannot be negative.");

        if (mark.Score.HasValue && mark.Score.Value < 0)
            throw new AppraisalPayloadException($"{prefix}.score cannot be negative.");

        if (mark.Score.HasValue && mark.Score.Value > mark.MaxScore)
            throw new AppraisalPayloadException($"{prefix}.score cannot be greater than maxScore.");
    }
}
