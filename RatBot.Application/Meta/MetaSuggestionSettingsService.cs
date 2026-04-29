using RatBot.Application.Common;

namespace RatBot.Application.Meta;

public sealed class MetaSuggestionSettingsService(IUnitOfWork uow, ILogger logger)
{
    private readonly ILogger _logger = logger.ForContext<MetaSuggestionSettingsService>();

    public async Task<ErrorOr<Success>> UpsertSuggestForumChannelAsync(
        ulong guildId,
        ulong forumChannelId,
        CancellationToken ct = default)
    {
        if (forumChannelId == 0)
            return MetaSuggestionErrors.ForumNotFound;

        IRepository<MetaSuggestionSettings> settings = uow.GetRepository<MetaSuggestionSettings>();
        ErrorOr<MetaSuggestionSettings> setting = await settings.TryFindAsync((long)guildId);

        if (!setting.IsError)
            settings.Delete(setting.Value);

        settings.Add(new MetaSuggestionSettings(guildId, forumChannelId));

        await uow.SaveChangesAsync(ct);

        _logger.Information(
            "Meta suggestion forum settings updated for guild {GuildId}. ForumChannelId={ForumChannelId}",
            guildId,
            forumChannelId);

        return Result.Success;
    }
}