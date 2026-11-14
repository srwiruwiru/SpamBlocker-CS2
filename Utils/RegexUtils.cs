using System.Text.RegularExpressions;

namespace SpamBlocker.Utils;

public static class RegexUtils
{
    private const string OctetPattern = @"(?:25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])";
    private const string Ipv4Pattern = @"(?:" + OctetPattern + @"\.){3}" + OctetPattern;
    private const string PortPattern = @"(?:6553[0-5]|655[0-2][0-9]|65[0-4][0-9]{2}|6[0-4][0-9]{3}|[1-5][0-9]{4}|[1-9][0-9]{0,3})";

    // NOTE: Rejects addresses glued to letters or to more dotted numbers, such as "v1.1.8.1" or "1.1.8.1.2"
    private const string StandaloneStartPattern = @"(?<![\w.])";
    private const string StandaloneEndPattern = @"(?!\w|\.\d)";

    public static readonly Regex UrlRegex = new(
        @"(?:(?:https?|ftp|steam):\/\/)?(?:www\.)?(?:[a-zA-Z0-9](?:[a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}\b(?:[-a-zA-Z0-9()@:%_\+.~#?&=\/]*)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static readonly Regex IpRegex = new(
        StandaloneStartPattern + Ipv4Pattern + StandaloneEndPattern,
        RegexOptions.Compiled);

    public static readonly Regex IpPortRegex = new(
        StandaloneStartPattern + Ipv4Pattern + ":" + PortPattern + StandaloneEndPattern,
        RegexOptions.Compiled);

    public static readonly Regex TrailingDigitsRegex = new(@"\d+$", RegexOptions.Compiled);

    public static bool IsWholeWordMatch(string content, string word, RegexOptions options)
    {
        var pattern = $@"\b{Regex.Escape(word)}\b";
        return Regex.IsMatch(content, pattern, options);
    }
}
