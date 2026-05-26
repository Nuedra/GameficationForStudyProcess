namespace Platform.Core.Appraisals;

public interface IAppraisalPayloadParser
{
    AppraisalPayloadDto Parse(string json);
}
