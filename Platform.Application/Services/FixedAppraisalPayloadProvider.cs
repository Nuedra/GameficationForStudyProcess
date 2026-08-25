using Platform.Core.Appraisals;

namespace Platform.Application.Services;

public sealed class FixedAppraisalPayloadProvider : IAppraisalPayloadProvider
{
    private static readonly Guid DemoStudentId =
        Guid.Parse("b0000000-0000-0000-0000-000000000001");
    private static readonly Guid DemoCourseId =
        Guid.Parse("a1000000-0000-0000-0000-000000000001");

    public Task<IReadOnlyList<AppraisalPayloadDto>> GetPayloadsAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<AppraisalPayloadDto> payloads =
        [
            new AppraisalPayloadDto
            {
                StudentId = DemoStudentId,
                CourseId = DemoCourseId,
                Year = 2026,
                AppraisalLists =
                [
                    new AppraisalListDto
                    {
                        ListId = Guid.Parse("d3000000-0000-0000-0000-000000000001"),
                        ListName = "Демонстрационная ведомость: обновление графа",
                        DateCreated = DateTimeOffset.Parse("2026-05-15T10:00:00Z"),
                        DateClosed = null,
                        Marks =
                        [
                            new AppraisalMarkDto
                            {
                                ColumnId = Guid.Parse("d4000000-0000-0000-0000-000000000003"),
                                ColumnName = "Демонстрационное задание №3",
                                IsComputed = false,
                                MaxScore = 10,
                                MinAcceptScore = 6,
                                Score = 10,
                                ScoreSourceName = "Демонстрационный преподаватель",
                                UpdatedAt = DateTimeOffset.Parse("2026-05-15T12:00:00Z"),
                                Tags = ["template_achievement_3"],
                                Deadline = null,
                                UploadedAt = DateTimeOffset.Parse("2026-05-15T11:30:00Z")
                            }
                        ]
                    }
                ]
            }
        ];

        return Task.FromResult(payloads);
    }
}
