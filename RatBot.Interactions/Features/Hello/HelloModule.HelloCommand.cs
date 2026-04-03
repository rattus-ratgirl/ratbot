using Discord;
using Discord.Interactions;

namespace RatBot.Interactions.Features.Hello;

public sealed partial class HelloModule
{
    /// <summary>
    /// Sends a greeting response to the invoking user.
    /// </summary>
    [SlashCommand("hello", "Says hello!")]
    [RequireUserPermission(GuildPermission.SendMessages)]
    public Task HelloAsync()
    {
        _logger.Information("Received hello command from {User}", Context.User.Username);
        return ReplyAsync($"Hello, {Context.User.Mention}!");
    }
}
