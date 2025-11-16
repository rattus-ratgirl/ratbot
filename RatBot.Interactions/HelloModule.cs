using Discord;
using Discord.Interactions;

namespace RatBot.Interactions;

public sealed class HelloModule : SlashCommandBase
{
    [SlashCommand("hello", "Says hello!")]
    [RequireUserPermission(GuildPermission.SendMessages)]
    public async Task HelloAsync()
    {
        await RespondAsync($"Hello, {Context.User.Mention}!", ephemeral: true);

        // Alternative way to defer and send a followup message in the channel
        // await DeferAsync(ephemeral: false);
        // await FollowupAsync($"Hello, {Context.User.Mention}!");
    }
}
