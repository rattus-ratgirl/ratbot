using RatBot.Application.Common;
using RatBot.Application.Common.Forums;

namespace RatBot.Application.Meta;

public sealed class MetaSuggestionService(IUnitOfWork uow, IForumThreadClient forumThreadClient, ILogger logger)
{
    private readonly ILogger _logger = logger.ForContext<MetaSuggestionService>();

    private static string FormatThreadTitle(long suggestionId, string title) => $"#{suggestionId:D3} - {title}";

    private static string BuildFirstPost(MetaSuggestion suggestion) =>
        $"""
         ## Author
         <@{suggestion.AuthorUserId}>
         ## Date
         <t:{suggestion.SubmittedAtUtc.ToUnixTimeSeconds()}:F>
         ## Anonymity
         {(suggestion.IsAnonymous ? "anonymous" : "public")}
         ## Summary
         {suggestion.Summary}
         """;

    private static string BuildSecondPost(MetaSuggestion suggestion) =>
        $"""
         ## Motivation
         {suggestion.Motivation}
         """;

    private static string BuildThirdPost(MetaSuggestion suggestion) =>
        $"""
         ## Specification
         {suggestion.Specification}
         """;

    public async Task<ErrorOr<Success>> SubmitAsync(MetaSuggestion suggestion, CancellationToken ct = default)
    {
        _logger.Information(
            "Received meta suggestion submission for guild {GuildId} from author {AuthorUserId}.",
            suggestion.GuildId,
            suggestion.AuthorUserId);

        IRepository<MetaSuggestionSettings> settingsRepo = uow.GetRepository<MetaSuggestionSettings>();
        ErrorOr<MetaSuggestionSettings> settingsResult = await settingsRepo.TryFindAsync((long)suggestion.GuildId);

        if (settingsResult.IsError)
            return MetaSuggestionErrors.ForumNotConfigured;

        MetaSuggestionSettings settings = settingsResult.Value;

        ErrorOr<Success> assignForum = suggestion.AssignForum(settings.SuggestForumChannelId);

        if (assignForum.IsError)
            return assignForum.Errors;

        IRepository<MetaSuggestion> suggestionsRepo = uow.GetRepository<MetaSuggestion>();
        suggestionsRepo.Add(suggestion);
        await uow.SaveChangesAsync(ct);

        if (suggestion.Id <= 0)
            return Error.Failure(
                "MetaSuggestion.PersistenceFailed",
                "Suggestion row was saved without a valid database identifier.");

        string threadTitle = FormatThreadTitle(suggestion.Id, suggestion.Title);
        string firstPost = BuildFirstPost(suggestion);
        string secondPost = BuildSecondPost(suggestion);
        string thirdPost = BuildThirdPost(suggestion);

        if (suggestion.ForumChannelId is not { } forumChannelId)
            return Error.Validation(
                "MetaSuggestion.MissingChannelId",
                "Suggestion row was saved without a forum channel id.");

        ErrorOr<PublishedForumThread> threadResult = await forumThreadClient.CreateThreadWithMessagesAsync(
            forumChannelId,
            threadTitle,
            [firstPost, secondPost, thirdPost]
        );

        if (threadResult.IsError)
        {
            _logger.Error(
                "Forum thread creation failed for suggestion {SuggestionId} in forum {ForumChannelId}. Error={ErrorCode}",
                suggestion.Id,
                suggestion.ForumChannelId,
                threadResult.FirstError.Code
            );

            return MetaSuggestionErrors.ForumThreadCreationFailed(suggestion.Id);
        }

        PublishedForumThread createdThread = threadResult.Value;

        ErrorOr<Success> attachResult = suggestion.AttachThread(createdThread.ThreadChannelId);

        if (attachResult.IsError)
            return attachResult.Errors;

        await uow.SaveChangesAsync(ct);

        _logger.Information(
            "Created forum thread {ThreadChannelId} for suggestion {SuggestionId}.",
            createdThread.ThreadChannelId,
            suggestion.Id);

        return Result.Success;
    }
}