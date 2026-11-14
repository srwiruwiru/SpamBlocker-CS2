namespace SpamBlocker.Models;

public sealed record ViolationReport(
    string PlayerName,
    string SteamId,
    ViolationSource Source,
    FilterResult Result,
    DateTime Timestamp);
