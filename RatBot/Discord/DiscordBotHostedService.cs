namespace RatBot.Discord;

/// <summary>
/// Hosted-service wrapper for <see cref="DiscordBotService"/>.
/// </summary>
public sealed class DiscordBotHostedService : BackgroundService
{
    private readonly DiscordBotService _botService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiscordBotHostedService"/> class.
    /// </summary>
    /// <param name="botService">The Discord bot service.</param>
    public DiscordBotHostedService(DiscordBotService botService)
    {
        _botService = botService;
    }

    /// <summary>
    /// Starts the bot service and waits until cancellation.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task that completes when execution stops.</returns>
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await _botService.StartAsync(ct);
        await Task.Delay(Timeout.Infinite, ct);
    }

    /// <summary>
    /// Stops the underlying bot service.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when shutdown finishes.</returns>
    public override Task StopAsync(CancellationToken cancellationToken) => _botService.StopAsync();
}
