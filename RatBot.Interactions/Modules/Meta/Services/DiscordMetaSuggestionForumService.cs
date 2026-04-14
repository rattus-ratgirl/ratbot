using RatBot.Application.Common.Discord;
using RatBot.Application.Features.Meta.Errors;

namespace RatBot.Interactions.Modules.Meta.Services;

public sealed class DiscordMetaSuggestionForumService(IGuild guild) : IDiscordMetaSuggestionForumService
{
    public async Task<ErrorOr<CreatedMetaSuggestionThread>> CreateSuggestionThreadAsync(
        ulong forumChannelId,
        string title,
        string firstPost,
        string secondPost,
        string thirdPost)
    {
        IForumChannel? forumChannel = await guild.GetForumChannelAsync(forumChannelId);

        if (forumChannel is null)
            return MetaSuggestionErrors.ForumNotFound;

        IThreadChannel thread = await forumChannel.CreatePostAsync(title, text: firstPost);
        await thread.SendMessageAsync(secondPost);
        await thread.SendMessageAsync(thirdPost);

        return new CreatedMetaSuggestionThread(thread.Id);
    }
}
