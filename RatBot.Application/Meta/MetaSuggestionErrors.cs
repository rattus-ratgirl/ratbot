namespace RatBot.Application.Meta;

public static class MetaSuggestionErrors
{
    public static readonly Error ForumNotConfigured = Error.NotFound(
        "MetaSuggestion.ForumNotConfigured",
        "Meta suggestions forum is not configured. Ask an admin to run `/config meta suggest <channel>`."
    );

    public static readonly Error ForumNotFound = Error.NotFound(
        "MetaSuggestion.ForumNotFound",
        "I couldn't find the configured meta suggestions forum channel. Ask an admin to run `/config meta suggest <channel>`."
    );

    public static Error ForumThreadCreationFailed(long suggestionId) =>
        Error.Failure(
            "MetaSuggestion.ForumThreadCreationFailed",
            $"Saved suggestion #{suggestionId:D3}, but thread creation failed. The suggestion is recoverable."
        );
}