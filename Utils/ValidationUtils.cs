using System.Text.RegularExpressions;

namespace SpamBlocker.Utils;

public static class ValidationUtils
{
    private const int IpOctetCount = 4;
    private const int MaxOctetValue = 255;
    private const int MinTopLevelDomainLength = 2;
    private const int MinUrlLength = 4;
    private const char DomainSeparator = '.';
    private const string LeadingZero = "0";
    private const string ProtocolSeparator = "://";
    private const string WwwPrefix = "www.";
    private const string DefaultScheme = "http://";

    private static readonly string[] KnownSchemes = ["http://", "https://", "steam://"];
    private static readonly Regex LetterRegex = new("[a-zA-Z]", RegexOptions.Compiled);
    private static readonly Regex OnlyLettersRegex = new("^[a-zA-Z]+$", RegexOptions.Compiled);
    private static readonly Regex OnlyDigitsAndDotsRegex = new(@"^[\d\.]+$", RegexOptions.Compiled);
    private static readonly Regex VersionNumberRegex = new(@"^\d+\.\d+(\.\d+)?$", RegexOptions.Compiled);

    public static bool IsValidIpAddress(string ip)
    {
        if (string.IsNullOrWhiteSpace(ip))
            return false;

        var octets = ip.Split(DomainSeparator);
        if (octets.Length != IpOctetCount)
            return false;

        foreach (var octet in octets)
        {
            if (!IsValidOctet(octet))
                return false;
        }

        return true;
    }

    public static bool IsAllowedProtocol(string url, List<string> allowedProtocols)
    {
        foreach (var protocol in allowedProtocols)
        {
            if (url.StartsWith(protocol + ProtocolSeparator, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return !url.Contains(ProtocolSeparator);
    }

    public static bool IsInDomainList(string domain, List<string> domainList)
    {
        foreach (var listDomain in domainList)
        {
            if (string.IsNullOrWhiteSpace(listDomain))
                continue;

            var cleanListDomain = listDomain.ToLowerInvariant().Replace(WwwPrefix, "");
            if (domain == cleanListDomain || domain.EndsWith(DomainSeparator + cleanListDomain))
                return true;
        }

        return false;
    }

    public static string? FindDomain(string url)
    {
        if (!IsLikelyUrl(url))
            return null;

        var absoluteUrl = HasKnownScheme(url) ? url : DefaultScheme + url;
        if (!Uri.TryCreate(absoluteUrl, UriKind.Absolute, out var uri))
            return null;

        var domain = uri.Host.ToLowerInvariant();
        if (IsValidIpAddress(domain) || !IsValidDomainFormat(domain))
            return null;

        return domain;
    }

    private static bool IsValidOctet(string octet)
    {
        if (!int.TryParse(octet, out var value) || value < 0 || value > MaxOctetValue)
            return false;

        return octet.Length == 1 || !octet.StartsWith(LeadingZero);
    }

    private static bool IsValidDomainFormat(string domain)
    {
        if (!domain.Contains(DomainSeparator) || !LetterRegex.IsMatch(domain) || OnlyDigitsAndDotsRegex.IsMatch(domain))
            return false;

        var topLevelDomain = domain[(domain.LastIndexOf(DomainSeparator) + 1)..];
        return topLevelDomain.Length >= MinTopLevelDomainLength && OnlyLettersRegex.IsMatch(topLevelDomain);
    }

    private static bool IsLikelyUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url) || url.Length < MinUrlLength)
            return false;

        return !VersionNumberRegex.IsMatch(url) && LetterRegex.IsMatch(url);
    }

    private static bool HasKnownScheme(string url)
    {
        foreach (var scheme in KnownSchemes)
        {
            if (url.StartsWith(scheme, StringComparison.Ordinal))
                return true;
        }

        return false;
    }
}
