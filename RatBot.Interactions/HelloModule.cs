using Discord;
using Discord.Interactions;
using Serilog;

namespace RatBot.Interactions;

public sealed class HelloModule : SlashCommandBase
{
    private readonly ILogger _logger;
    
    public HelloModule(ILogger logger)
    {
        _logger = logger.ForContext<HelloModule>();
    }
    
    [SlashCommand("hello", "Says hello!")]
    [RequireUserPermission(GuildPermission.SendMessages)]
    public async Task HelloAsync()
    {
        _logger.Information("Received hello command from {User}", Context.User.Username);
        await RespondAsync($"Hello, {Context.User.Mention}!", ephemeral: true);

        // Alternative way to defer and send a followup message in the channel
        // await DeferAsync(ephemeral: false);
        // await FollowupAsync($"Hello, {Context.User.Mention}!");
    }
}
