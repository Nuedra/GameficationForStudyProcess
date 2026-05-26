namespace Platform.Core.Appraisals;

public sealed class AppraisalPayloadException : Exception
{
    public AppraisalPayloadException(string message)
        : base(message)
    {
    }

    public AppraisalPayloadException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
