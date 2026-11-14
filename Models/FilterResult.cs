namespace SpamBlocker.Models;

public sealed record FilterResult(bool IsBlocked, BlockReason Reason, string DetectedContent, ViolationType ViolationType)
{
    public static readonly FilterResult Allowed = new(false, BlockReason.None, "", ViolationType.None);

    public static FilterResult Blocked(ViolationType violationType, BlockReason reason, string detectedContent)
    {
        return new FilterResult(true, reason, detectedContent, violationType);
    }
}
