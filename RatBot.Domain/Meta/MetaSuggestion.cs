namespace RatBot.Domain.Meta;

public sealed class MetaSuggestion
{
    // For EF
    private MetaSuggestion()
    {
    }

    public long Id { get; private set; }
    public ulong GuildId { get; private set; }
    public ulong AuthorUserId { get; private set; }
    public DateTimeOffset SubmittedAtUtc { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Summary { get; private set; } = string.Empty;
    public string Motivation { get; private set; } = string.Empty;
    public string Specification { get; private set; } = string.Empty;
    public bool IsAnonymous { get; private set; }
    public MetaSuggestionState State { get; private set; }
    public ulong? ForumChannelId { get; private set; }
    public ulong? ThreadChannelId { get; private set; }

    public static ErrorOr<MetaSuggestion> Create(
        ulong guildId,
        ulong authorUserId,
        string title,
        string summary,
        string motivation,
        string specification,
        bool isAnonymous,
        DateTimeOffset submittedAtUtc
    )
    {
        title = title.Trim();
        summary = summary.Trim();
        motivation = motivation.Trim();
        specification = specification.Trim();

        if (string.IsNullOrWhiteSpace(title))
            return RequiredFieldMissing(nameof(Title));

        if (string.IsNullOrWhiteSpace(summary))
            return RequiredFieldMissing(nameof(Summary));

        if (string.IsNullOrWhiteSpace(motivation))
            return RequiredFieldMissing(nameof(Motivation));

        if (string.IsNullOrWhiteSpace(specification))
            return RequiredFieldMissing(nameof(Specification));

        return new MetaSuggestion
        {
            Id = 0,
            GuildId = guildId,
            AuthorUserId = authorUserId,
            SubmittedAtUtc = submittedAtUtc,
            Title = title,
            Summary = summary,
            Motivation = motivation,
            Specification = specification,
            IsAnonymous = isAnonymous,
            State = MetaSuggestionState.New,
        };
    }

    private static Error RequiredFieldMissing(string fieldName) =>
        Error.Validation(
            $"MetaSuggestion.{fieldName}Required",
            $"Meta suggestion {fieldName.ToLowerInvariant()} is required.");

    public ErrorOr<Success> AttachThread(ulong threadChannelId)
    {
        if (ThreadChannelId is not null)
            return Error.Conflict(
                "MetaSuggestion.ThreadAlreadyAttached",
                "Meta suggestion already has a thread attached.");

        ThreadChannelId = threadChannelId;
        return Result.Success;
    }

    public ErrorOr<Success> AssignForum(ulong forumChannelId)
    {
        if (ForumChannelId is not null)
            return Error.Conflict(
                "MetaSuggestion.ForumAlreadyAssigned",
                "Meta suggestion already has an assigned forum.");

        if (forumChannelId == 0)
            return Error.Validation(
                "MetaSuggestion.ForumRequired",
                "A valid forum channel id is required.");

        ForumChannelId = forumChannelId;
        return Result.Success;
    }
}