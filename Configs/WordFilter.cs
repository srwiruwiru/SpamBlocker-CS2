using System.Text.Json.Serialization;

namespace SpamBlocker.Configs;

public sealed class WordFilterConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("blacklisted_words")]
    public List<string> BlacklistedWords { get; set; } = ["badword1", "badword2", "spam", "cheat"];

    [JsonPropertyName("case_sensitive")]
    public bool CaseSensitive { get; set; } = false;

    [JsonPropertyName("whole_word_only")]
    public bool WholeWordOnly { get; set; } = true;
}
