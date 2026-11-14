using System.Net;

namespace SpamBlocker.Utils;

public static class IpAddressUtils
{
    private const char OctetSeparator = '.';
    private const int SingleDigitLength = 1;

    private static readonly IPNetwork[] NonPublicNetworks =
    [
        IPNetwork.Parse("0.0.0.0/8"),
        IPNetwork.Parse("10.0.0.0/8"),
        IPNetwork.Parse("100.64.0.0/10"),
        IPNetwork.Parse("127.0.0.0/8"),
        IPNetwork.Parse("169.254.0.0/16"),
        IPNetwork.Parse("172.16.0.0/12"),
        IPNetwork.Parse("192.168.0.0/16"),
        IPNetwork.Parse("224.0.0.0/4"),
        IPNetwork.Parse("240.0.0.0/4")
    ];

    public static bool IsPublicAddress(string ip)
    {
        if (!IPAddress.TryParse(ip, out var address))
            return false;

        foreach (var network in NonPublicNetworks)
        {
            if (network.Contains(address))
                return false;
        }

        return true;
    }

    // NOTE: Versions like "1.1.8.1" are valid addresses; real server addresses almost never have only single-digit octets
    public static bool IsVersionNumberLike(string ip)
    {
        foreach (var octet in ip.Split(OctetSeparator))
        {
            if (octet.Length > SingleDigitLength)
                return false;
        }

        return true;
    }
}
