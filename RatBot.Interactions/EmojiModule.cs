using System.Text;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using RatBot.Domain.Entities;
using RatBot.Infrastructure.Services;

namespace RatBot.Interactions;

[Group("emoji", "Emoji analytics commands.")]
[DefaultMemberPermissions(GuildPermission.MuteMembers)]
public sealed class EmojiModule(EmojiUsageService emojiUsageService, DiscordSocketClient discordClient) : SlashCommandBase
{
    [SlashCommand("usage", "Show the top 25 emojis by usage.")]
    public async Task UsageAsync()
    {
        if (!await TryDeferEphemeralAsync())
            return;

        List<EmojiUsageCount> topUsage = await emojiUsageService.GetTopUsageAsync(25);
        if (topUsage.Count == 0)
        {
            await SendEphemeralAsync("No emoji usage has been recorded yet.");
            return;
        }

        StringBuilder text = new StringBuilder("Top emoji usage:\n");

        foreach (EmojiUsageCount row in topUsage)
        {
            string emojiDisplay = FormatEmojiForDisplay(row.EmojiId);
            text.AppendLine($"{emojiDisplay}: {row.UsageCount}");
        }

        await SendEphemeralAsync(text.ToString());
    }

    private string FormatEmojiForDisplay(string emojiId)
    {
        if (string.IsNullOrWhiteSpace(emojiId))
            return "[unknown]";

        if (!ulong.TryParse(emojiId, out ulong parsedEmojiId))
            return emojiId;

        foreach (SocketGuild guild in discordClient.Guilds)
        {
            GuildEmote? guildEmote = guild.Emotes.FirstOrDefault(x => x.Id == parsedEmojiId);
            if (guildEmote is not null)
                return guildEmote.ToString();
        }

        return $"[custom:{parsedEmojiId}]";
    }
}
