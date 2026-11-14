using SpamBlocker.Models;

namespace SpamBlocker.Utils;

public static class BlockReasonUtils
{
    public static string GetLangKey(BlockReason reason)
    {
        return reason switch
        {
            BlockReason.BlacklistedWord => "reason_blacklisted_word",
            BlockReason.IpPortNotWhitelisted => "reason_ip_port_not_whitelisted",
            BlockReason.IpNotWhitelisted => "reason_ip_not_whitelisted",
            BlockReason.ProtocolNotAllowed => "reason_protocol_not_allowed",
            BlockReason.DomainNotWhitelisted => "reason_domain_not_whitelisted",
            BlockReason.DomainBlacklisted => "reason_domain_blacklisted",
            BlockReason.NameTooShort => "reason_name_too_short",
            BlockReason.NameTooLong => "reason_name_too_long",
            BlockReason.BlacklistedName => "reason_blacklisted_name",
            _ => reason.ToString()
        };
    }
}
