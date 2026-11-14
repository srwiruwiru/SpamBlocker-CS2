namespace SpamBlocker.Models;

public sealed record FilterResult(bool IsBlocked, string Reason, string DetectedContent, ViolationType ViolationType)
{
    public static readonly FilterResult Allowed = new(false, "", "", ViolationType.None);

    public static FilterResult Blocked(ViolationType violationType, string reason, string detectedContent)
    {
        return new FilterResult(true, reason, detectedContent, violationType);
    }
}
