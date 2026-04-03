using System.Text;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using RatBot.Domain.Entities;

namespace RatBot.Interactions.Features.Emoji;

public sealed partial class EmojiModule
{
    /// <summary>
    /// Shows the top emoji usage counts.
    /// </summary>
    [SlashCommand("usage", "Show the top 25 emojis by usage.")]
    public Task UsageAsync()
    {
        return ReplyAsync(GetUsageResponseAsync, defer: true);
    }

    private async Task<string> GetUsageResponseAsync()
    {
        List<EmojiUsageCount> topUsage = await _emojiUsageService.GetTopUsageAsync(25);
        if (topUsage.Count == 0)
            return "No emoji usage has been recorded yet.";

        StringBuilder text = new StringBuilder("Top emoji usage:\n");

        foreach (EmojiUsageCount row in topUsage)
        {
            string emojiDisplay = FormatEmojiForDisplay(row.EmojiId);
            text.AppendLine($"{emojiDisplay}: {row.UsageCount}");
        }

        return text.ToString();
    }

    private string FormatEmojiForDisplay(string emojiId)
    {
        if (string.IsNullOrWhiteSpace(emojiId))
            return "[unknown]";

        if (!ulong.TryParse(emojiId, out ulong parsedEmojiId))
            return emojiId;

        foreach (SocketGuild guild in _discordClient.Guilds)
        {
            GuildEmote? guildEmote = guild.Emotes.FirstOrDefault(x => x.Id == parsedEmojiId);
            if (guildEmote is not null)
                return guildEmote.ToString();
        }

        return $"[custom:{parsedEmojiId}]";
    }
}
