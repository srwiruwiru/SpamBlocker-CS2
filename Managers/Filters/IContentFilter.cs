using SpamBlocker.Models;

namespace SpamBlocker.Managers.Filters;

public interface IContentFilter
{
    FilterResult Check(string content);
}
