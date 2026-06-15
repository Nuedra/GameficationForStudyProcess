using Platform.Core.Appraisals;

namespace Platform.Core.Tests.Appraisals;

public sealed class AppraisalFactsExtractorTests
{
    [Fact]
    public void Extract_FlattensPayloadMarks()
    {
        var payload = CreatePayload();
        var facts = new AppraisalFactsExtractor().Extract(payload);

        Assert.Equal(payload.StudentId, facts.StudentId);
        Assert.Equal(payload.CourseId, facts.CourseId);
        Assert.Equal(payload.Year, facts.Year);
        Assert.Equal(2, facts.Marks.Count);
    }

    [Fact]
    public void Extract_CopiesListAndMarkFields()
    {
        var payload = CreatePayload();
        var facts = new AppraisalFactsExtractor().Extract(payload);

        var mark = facts.Marks[0];
        Assert.Equal(Guid.Parse("b112948c-9c12-4211-9fa6-324cba4388b3"), mark.ListId);
        Assert.Equal("Текущая аттестация: Модуль 1", mark.ListName);
        Assert.Equal(DateTimeOffset.Parse("2025-10-15T10:00:00Z"), mark.DateCreated);
        Assert.Equal(DateTimeOffset.Parse("2025-11-01T18:00:00Z"), mark.DateClosed);
        Assert.Equal(Guid.Parse("1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d"), mark.ColumnId);
        Assert.Equal("Лабораторная работа №1", mark.ColumnName);
        Assert.False(mark.IsComputed);
        Assert.Equal(10, mark.MaxScore);
        Assert.Equal(6, mark.MinAcceptScore);
        Assert.Equal(9, mark.Score);
        Assert.Equal("Иванов И.И.", mark.ScoreSourceName);
        Assert.Equal(DateTimeOffset.Parse("2025-10-20T14:35:00Z"), mark.UpdatedAt);
        Assert.Equal(5, mark.Tags.Count);
        Assert.Contains("lab1_completed", mark.Tags);
        Assert.Contains("lab1_success", mark.Tags);
        Assert.Contains("intime", mark.Tags);
        Assert.Contains("passed", mark.Tags);
        Assert.Contains("highscore", mark.Tags);
        Assert.Equal(DateTimeOffset.Parse("2025-10-19T20:59:59Z"), mark.Deadline);
        Assert.Equal(DateTimeOffset.Parse("2025-10-18T16:20:00Z"), mark.UploadedAt);
    }

    [Fact]
    public void MarkFact_ComputedProperties_UseScoreAndThresholds()
    {
        var payload = CreatePayload();
        var facts = new AppraisalFactsExtractor().Extract(payload);

        var setMark = facts.Marks[0];
        Assert.True(setMark.IsSet);
        Assert.True(setMark.IsPassed);
        Assert.Equal(90, setMark.ScorePercent);

        var unsetMark = facts.Marks[1];
        Assert.False(unsetMark.IsSet);
        Assert.False(unsetMark.IsPassed);
        Assert.Null(unsetMark.ScorePercent);
    }

    [Fact]
    public void Extract_AddsComputedTags()
    {
        var payload = CreatePayload();
        payload.AppraisalLists[0].Marks.Add(new AppraisalMarkDto
        {
            ColumnId = Guid.NewGuid(),
            ColumnName = "Лабораторная работа №2",
            IsComputed = false,
            MaxScore = 10,
            MinAcceptScore = 6,
            Score = 10,
            Tags = ["existing", "maxscore"],
            Deadline = DateTimeOffset.Parse("2025-10-19T20:59:59Z"),
            UploadedAt = DateTimeOffset.Parse("2025-10-20T16:20:00Z")
        });

        var facts = new AppraisalFactsExtractor().Extract(payload);
        var mark = facts.Marks[2];

        Assert.Contains("maxscore", mark.Tags);
        Assert.Contains("passed", mark.Tags);
        Assert.Contains("expired", mark.Tags);
        Assert.DoesNotContain("intime", mark.Tags);
        Assert.Equal(1, mark.Tags.Count(tag => tag == "maxscore"));
    }

    [Theory]
    [InlineData(100, "maxscore")]
    [InlineData(99, "highscore")]
    [InlineData(90, "highscore")]
    [InlineData(89, "goodscore")]
    [InlineData(80, "goodscore")]
    [InlineData(79, "mediumscore")]
    [InlineData(70, "mediumscore")]
    [InlineData(69, "lowscore")]
    [InlineData(60, "lowscore")]
    public void Extract_AddsScoreRangeTag(decimal score, string expectedTag)
    {
        var mark = ExtractSingleMark(CreateMark(score));

        Assert.Contains(expectedTag, mark.Tags);
        Assert.Equal(
            1,
            mark.Tags.Count(tag => tag is
                "maxscore" or "highscore" or "goodscore" or "mediumscore" or "lowscore"));
    }

    [Theory]
    [InlineData(60, true)]
    [InlineData(59, false)]
    public void Extract_AddsPassedOnlyAtOrAboveMinimum(decimal score, bool expectedPassed)
    {
        var mark = ExtractSingleMark(CreateMark(score));

        Assert.Equal(expectedPassed, mark.Tags.Contains("passed"));
    }

    [Fact]
    public void Extract_ScoreBelowSixtyPercent_HasNoScoreRangeTag()
    {
        var mark = ExtractSingleMark(CreateMark(59));

        Assert.DoesNotContain(
            mark.Tags,
            tag => tag is "maxscore" or "highscore" or "goodscore" or "mediumscore" or "lowscore");
    }

    [Fact]
    public void Extract_MissingScore_HasNoScoreTags()
    {
        var mark = ExtractSingleMark(CreateMark(null));

        Assert.DoesNotContain("passed", mark.Tags);
        Assert.DoesNotContain(
            mark.Tags,
            tag => tag is "maxscore" or "highscore" or "goodscore" or "mediumscore" or "lowscore");
    }

    [Fact]
    public void Extract_UploadedAfterDeadline_AddsExpired()
    {
        var dto = CreateMark(80);
        dto = new AppraisalMarkDto
        {
            ColumnId = dto.ColumnId,
            ColumnName = dto.ColumnName,
            IsComputed = dto.IsComputed,
            MaxScore = dto.MaxScore,
            MinAcceptScore = dto.MinAcceptScore,
            Score = dto.Score,
            Deadline = DateTimeOffset.Parse("2026-01-01T10:00:00Z"),
            UploadedAt = DateTimeOffset.Parse("2026-01-01T10:00:01Z")
        };

        var mark = ExtractSingleMark(dto);

        Assert.Contains("expired", mark.Tags);
        Assert.DoesNotContain("intime", mark.Tags);
    }

    [Fact]
    public void Extract_UploadedAtDeadline_AddsIntime()
    {
        var deadline = DateTimeOffset.Parse("2026-01-01T10:00:00Z");
        var dto = CreateMark(80);
        dto = new AppraisalMarkDto
        {
            ColumnId = dto.ColumnId,
            ColumnName = dto.ColumnName,
            IsComputed = dto.IsComputed,
            MaxScore = dto.MaxScore,
            MinAcceptScore = dto.MinAcceptScore,
            Score = dto.Score,
            Deadline = deadline,
            UploadedAt = deadline
        };

        var mark = ExtractSingleMark(dto);

        Assert.Contains("intime", mark.Tags);
        Assert.DoesNotContain("expired", mark.Tags);
    }

    private static MarkFact ExtractSingleMark(AppraisalMarkDto mark)
    {
        var payload = CreatePayload();
        payload.AppraisalLists[0].Marks.Clear();
        payload.AppraisalLists[0].Marks.Add(mark);

        return Assert.Single(new AppraisalFactsExtractor().Extract(payload).Marks);
    }

    private static AppraisalMarkDto CreateMark(decimal? score)
    {
        return new AppraisalMarkDto
        {
            ColumnId = Guid.NewGuid(),
            ColumnName = "Лабораторная работа",
            IsComputed = false,
            MaxScore = 100,
            MinAcceptScore = 60,
            Score = score
        };
    }

    private static AppraisalPayloadDto CreatePayload()
    {
        return new AppraisalPayloadDto
        {
            StudentId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
            CourseId = Guid.Parse("ca761232-ed42-4fb3-818b-0ed1de24f5a2"),
            Year = 2025,
            AppraisalLists =
            [
                new AppraisalListDto
                {
                    ListId = Guid.Parse("b112948c-9c12-4211-9fa6-324cba4388b3"),
                    ListName = "Текущая аттестация: Модуль 1",
                    DateCreated = DateTimeOffset.Parse("2025-10-15T10:00:00Z"),
                    DateClosed = DateTimeOffset.Parse("2025-11-01T18:00:00Z"),
                    Marks =
                    [
                        new AppraisalMarkDto
                        {
                            ColumnId = Guid.Parse("1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d"),
                            ColumnName = "Лабораторная работа №1",
                            IsComputed = false,
                            MaxScore = 10,
                            MinAcceptScore = 6,
                            Score = 9,
                            ScoreSourceName = "Иванов И.И.",
                            UpdatedAt = DateTimeOffset.Parse("2025-10-20T14:35:00Z"),
                            Tags = ["lab1_completed", "lab1_success"],
                            Deadline = DateTimeOffset.Parse("2025-10-19T20:59:59Z"),
                            UploadedAt = DateTimeOffset.Parse("2025-10-18T16:20:00Z")
                        },
                        new AppraisalMarkDto
                        {
                            ColumnId = Guid.Parse("7a8b9c0d-1e2f-3a4b-5c6d-1a2b3c4d5e6f"),
                            ColumnName = "Итог по Модулю 1",
                            IsComputed = true,
                            MaxScore = 10,
                            MinAcceptScore = 6,
                            Score = null,
                            ScoreSourceName = null,
                            UpdatedAt = null
                        }
                    ]
                }
            ]
        };
    }
}
