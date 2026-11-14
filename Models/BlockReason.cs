namespace SpamBlocker.Models;

public enum BlockReason
{
    None,
    BlacklistedWord,
    IpPortNotWhitelisted,
    IpNotWhitelisted,
    ProtocolNotAllowed,
    DomainNotWhitelisted,
    DomainBlacklisted,
    NameTooShort,
    NameTooLong,
    BlacklistedName
}
