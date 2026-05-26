using Platform.Core.Appraisals;

namespace Platform.Core.Tests.Appraisals;

public sealed class AppraisalPayloadParserTests
{
    private readonly AppraisalPayloadParser _parser = new();

    [Fact]
    public void Parse_ValidPayload_ReturnsDto()
    {
        var payload = _parser.Parse(ValidPayloadJson);

        Assert.Equal(Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"), payload.StudentId);
        Assert.Equal(Guid.Parse("ca761232-ed42-4fb3-818b-0ed1de24f5a2"), payload.CourseId);
        Assert.Equal(2025, payload.Year);
        Assert.Single(payload.AppraisalLists);

        var list = payload.AppraisalLists[0];
        Assert.Equal(Guid.Parse("b112948c-9c12-4211-9fa6-324cba4388b3"), list.ListId);
        Assert.Equal("Текущая аттестация: Модуль 1", list.ListName);
        Assert.Equal(DateTimeOffset.Parse("2025-10-15T10:00:00Z"), list.DateCreated);
        Assert.Equal(DateTimeOffset.Parse("2025-11-01T18:00:00Z"), list.DateClosed);
        Assert.Equal(2, list.Marks.Count);

        var firstMark = list.Marks[0];
        Assert.Equal(Guid.Parse("1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d"), firstMark.ColumnId);
        Assert.Equal("Лабораторная работа №1", firstMark.ColumnName);
        Assert.False(firstMark.IsComputed);
        Assert.Equal(10, firstMark.MaxScore);
        Assert.Equal(6, firstMark.MinAcceptScore);
        Assert.Equal(9, firstMark.Score);
        Assert.Equal("Иванов И.И.", firstMark.ScoreSourceName);
        Assert.Equal(DateTimeOffset.Parse("2025-10-20T14:35:00Z"), firstMark.UpdatedAt);
    }

    [Fact]
    public void Parse_NullableFields_AllowsNullValues()
    {
        var payload = _parser.Parse(NullableFieldsJson);

        var list = Assert.Single(payload.AppraisalLists);
        Assert.Null(list.DateClosed);

        var mark = Assert.Single(list.Marks);
        Assert.Null(mark.Score);
        Assert.Null(mark.ScoreSourceName);
        Assert.Null(mark.UpdatedAt);
    }

    [Fact]
    public void Parse_EmptyAppraisalLists_AllowsEmptyCollection()
    {
        var payload = _parser.Parse("""
        {
          "studentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
          "courseId": "ca761232-ed42-4fb3-818b-0ed1de24f5a2",
          "year": 2025,
          "appraisalLists": []
        }
        """);

        Assert.Empty(payload.AppraisalLists);
    }

    [Fact]
    public void Parse_EmptyMarks_AllowsEmptyCollection()
    {
        var payload = _parser.Parse("""
        {
          "studentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
          "courseId": "ca761232-ed42-4fb3-818b-0ed1de24f5a2",
          "year": 2025,
          "appraisalLists": [
            {
              "listId": "b112948c-9c12-4211-9fa6-324cba4388b3",
              "listName": "Текущая аттестация: Модуль 1",
              "dateCreated": "2025-10-15T10:00:00Z",
              "dateClosed": null,
              "marks": []
            }
          ]
        }
        """);

        var list = Assert.Single(payload.AppraisalLists);
        Assert.Empty(list.Marks);
    }

    [Theory]
    [InlineData("""
    {
      "courseId": "ca761232-ed42-4fb3-818b-0ed1de24f5a2",
      "year": 2025,
      "appraisalLists": []
    }
    """)]
    [InlineData("""
    {
      "studentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "year": 2025,
      "appraisalLists": []
    }
    """)]
    public void Parse_MissingRequiredIds_Throws(string json)
    {
        Assert.Throws<AppraisalPayloadException>(() => _parser.Parse(json));
    }

    [Fact]
    public void Parse_ScoreGreaterThanMaxScore_Throws()
    {
        var ex = Assert.Throws<AppraisalPayloadException>(() => _parser.Parse("""
        {
          "studentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
          "courseId": "ca761232-ed42-4fb3-818b-0ed1de24f5a2",
          "year": 2025,
          "appraisalLists": [
            {
              "listId": "b112948c-9c12-4211-9fa6-324cba4388b3",
              "listName": "Текущая аттестация: Модуль 1",
              "dateCreated": "2025-10-15T10:00:00Z",
              "dateClosed": null,
              "marks": [
                {
                  "columnId": "1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d",
                  "columnName": "Лабораторная работа №1",
                  "isComputed": false,
                  "maxScore": 10,
                  "minAcceptScore": 6,
                  "score": 11,
                  "scoreSourceName": "Иванов И.И.",
                  "updatedAt": "2025-10-20T14:35:00Z"
                }
              ]
            }
          ]
        }
        """));

        Assert.Contains("score cannot be greater than maxScore", ex.Message);
    }

    private const string ValidPayloadJson = """
    {
      "studentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "courseId": "ca761232-ed42-4fb3-818b-0ed1de24f5a2",
      "year": 2025,
      "appraisalLists": [
        {
          "listId": "b112948c-9c12-4211-9fa6-324cba4388b3",
          "listName": "Текущая аттестация: Модуль 1",
          "dateCreated": "2025-10-15T10:00:00Z",
          "dateClosed": "2025-11-01T18:00:00Z",
          "marks": [
            {
              "columnId": "1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d",
              "columnName": "Лабораторная работа №1",
              "isComputed": false,
              "maxScore": 10,
              "minAcceptScore": 6,
              "score": 9,
              "scoreSourceName": "Иванов И.И.",
              "updatedAt": "2025-10-20T14:35:00Z"
            },
            {
              "columnId": "7a8b9c0d-1e2f-3a4b-5c6d-1a2b3c4d5e6f",
              "columnName": "Итог по Модулю 1",
              "isComputed": true,
              "maxScore": 10,
              "minAcceptScore": 6,
              "score": 9,
              "scoreSourceName": "Calculated",
              "updatedAt": "2025-10-21T09:00:00Z"
            }
          ]
        }
      ]
    }
    """;

    private const string NullableFieldsJson = """
    {
      "studentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "courseId": "ca761232-ed42-4fb3-818b-0ed1de24f5a2",
      "year": 2025,
      "appraisalLists": [
        {
          "listId": "b112948c-9c12-4211-9fa6-324cba4388b3",
          "listName": "Текущая аттестация: Модуль 1",
          "dateCreated": "2025-10-15T10:00:00Z",
          "dateClosed": null,
          "marks": [
            {
              "columnId": "1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d",
              "columnName": "Лабораторная работа №1",
              "isComputed": false,
              "maxScore": 10,
              "minAcceptScore": 6,
              "score": null,
              "scoreSourceName": null,
              "updatedAt": null
            }
          ]
        }
      ]
    }
    """;
}
