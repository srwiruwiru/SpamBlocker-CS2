using System.Text.Json.Serialization;

namespace SpamBlocker.Configs;

public sealed class NameBlacklistConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("blacklisted_names")]
    public List<string> BlacklistedNames { get; set; } = ["admin", "moderator", "owner"];

    [JsonPropertyName("case_sensitive")]
    public bool CaseSensitive { get; set; } = false;

    [JsonPropertyName("whole_name_only")]
    public bool WholeNameOnly { get; set; } = false;

    [JsonPropertyName("block_partial_matches")]
    public bool BlockPartialMatches { get; set; } = false;

    [JsonPropertyName("allow_numbers_suffix")]
    public bool AllowNumbersSuffix { get; set; } = false;

    [JsonPropertyName("min_name_length")]
    public int MinNameLength { get; set; } = 2;

    [JsonPropertyName("max_name_length")]
    public int MaxNameLength { get; set; } = 32;
}
