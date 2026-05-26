namespace Platform.Core.Appraisals;

public interface IAppraisalFactsExtractor
{
    StudentCourseFacts Extract(AppraisalPayloadDto payload);
}
