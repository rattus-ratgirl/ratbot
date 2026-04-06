using RatBot.Infrastructure.Services;

namespace RatBot.Interactions.Features.Emoji;

/// <summary>
/// Defines emoji analytics interactions.
/// </summary>
[Group("emoji", "Emoji analytics commands.")]
[DefaultMemberPermissions(GuildPermission.MuteMembers)]
public sealed partial class EmojiModule : SlashCommandBase
{
    private readonly EmojiUsageService _emojiUsageService;
    private readonly DiscordSocketClient _discordClient;

    public EmojiModule(EmojiUsageService emojiUsageService, DiscordSocketClient discordClient)
    {
        _emojiUsageService = emojiUsageService;
        _discordClient = discordClient;
    }
}
