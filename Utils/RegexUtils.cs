using System.Text.RegularExpressions;

namespace SpamBlocker.Utils;

public static class RegexUtils
{
    public static readonly Regex UrlRegex = new(
        @"(?:(?:https?|ftp|steam):\/\/)?(?:www\.)?(?:[a-zA-Z0-9](?:[a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}\b(?:[-a-zA-Z0-9()@:%_\+.~#?&=\/]*)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static readonly Regex IpRegex = new(
        @"\b(?:(?:25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\b",
        RegexOptions.Compiled);

    public static readonly Regex IpPortRegex = new(
        @"\b(?:(?:25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9]):(?:[1-9][0-9]{0,4}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])\b",
        RegexOptions.Compiled);

    public static readonly Regex TrailingDigitsRegex = new(@"\d+$", RegexOptions.Compiled);

    public static bool IsWholeWordMatch(string content, string word, RegexOptions options)
    {
        var pattern = $@"\b{Regex.Escape(word)}\b";
        return Regex.IsMatch(content, pattern, options);
    }
}
