namespace SpamBlocker.Models;

public enum ViolationType
{
    None,
    BlacklistedWord,
    BlacklistedName,
    BlockedUrl,
    BlockedIp
}
