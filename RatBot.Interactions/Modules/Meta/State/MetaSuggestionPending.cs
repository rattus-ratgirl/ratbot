namespace RatBot.Interactions.Modules.Meta.State;

public sealed record MetaSuggestionPending(
    ulong GuildId,
    ulong AuthorUserId,
    string Title,
    string Summary,
    string Motivation,
    string Specification);
