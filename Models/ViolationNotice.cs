using SpamBlocker.Managers;

namespace SpamBlocker.Models;

public sealed record ViolationNotice(string ActionText, string PlayerName, FilterResult Result);
