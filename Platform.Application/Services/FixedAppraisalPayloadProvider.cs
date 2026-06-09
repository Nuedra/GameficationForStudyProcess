using Platform.Core.Appraisals;

namespace Platform.Application.Services;

public sealed class FixedAppraisalPayloadProvider : IAppraisalPayloadProvider
{
    public Task<IReadOnlyList<AppraisalPayloadDto>> GetPayloadsAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<AppraisalPayloadDto> payloads =
        [
            new AppraisalPayloadDto
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
                            }
                        ]
                    }
                ]
            }
        ];

        return Task.FromResult(payloads);
    }
}
