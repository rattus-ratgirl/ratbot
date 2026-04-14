using System.Collections.Concurrent;

namespace RatBot.Interactions.Modules.Meta.State;

public sealed class MetaSuggestionPendingStore
{
    private readonly ConcurrentDictionary<string, MetaSuggestionPending> _drafts =
        new ConcurrentDictionary<string, MetaSuggestionPending>();

    public string Save(MetaSuggestionPending pending)
    {
        string token = Guid.CreateVersion7().ToString("N");
        _drafts[token] = pending;
        return token;
    }

    public bool TryTake(string token, out MetaSuggestionPending? draft)
    {
        bool removed = _drafts.TryRemove(token, out MetaSuggestionPending? value);
        draft = value;
        return removed;
    }
}