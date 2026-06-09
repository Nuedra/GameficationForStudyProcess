namespace Platform.Core.Appraisals;

public interface IAppraisalPayloadProvider
{
    Task<IReadOnlyList<AppraisalPayloadDto>> GetPayloadsAsync(
        CancellationToken cancellationToken = default);
}
