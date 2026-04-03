using Discord;
using Discord.Interactions;
using Serilog;

namespace RatBot.Interactions;

/// <summary>
/// Defines greeting interactions.
/// </summary>
public sealed class HelloModule : SlashCommandBase
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="HelloModule"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public HelloModule(ILogger logger)
    {
        _logger = logger.ForContext<HelloModule>();
    }

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
