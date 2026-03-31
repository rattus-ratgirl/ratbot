using System.Text;
using Discord;
using Discord.Interactions;
using RatBot.Domain.Entities;
using RatBot.Infrastructure.Services;

namespace RatBot.Interactions;

[Group("emoji", "Emoji analytics commands.")]
public sealed class EmojiModule(EmojiUsageService emojiUsageService) : SlashCommandBase
{
    [SlashCommand("usage", "Show top emoji usage counts.")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task UsageAsync()
    {
        List<EmojiUsageCount> topUsage = await emojiUsageService.GetTopUsageAsync(25);
        if (topUsage.Count == 0)
        {
            await RespondAsync("No emoji usage has been recorded yet.", ephemeral: true);
            return;
        }

        StringBuilder text = new StringBuilder("Top emoji usage:\n");

        foreach (EmojiUsageCount row in topUsage)
        {
            string emojiDisplay = FormatEmojiForDisplay(row.EmojiId);
            text.AppendLine($"{emojiDisplay}: {row.UsageCount}");
        }

        await RespondAsync(text.ToString(), ephemeral: true);
    }

    private string FormatEmojiForDisplay(string emojiId)
    {
        if (!ulong.TryParse(emojiId, out ulong parsedEmojiId))
            return emojiId;

        GuildEmote? guildEmote = Context.Guild.Emotes.FirstOrDefault(x => x.Id == parsedEmojiId);
        if (guildEmote is not null)
            return guildEmote.ToString();

        return $"<:emoji:{parsedEmojiId}>";
    }
}
