namespace RatBot.Discord;

public sealed class DiscordBotHostedService : BackgroundService
{
    private readonly DiscordBotService _botService;

    public DiscordBotHostedService(DiscordBotService botService)
    {
        _botService = botService;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await _botService.StartAsync(ct);
        await Task.Delay(Timeout.Infinite, ct);
    }

    public override Task StopAsync(CancellationToken cancellationToken) => _botService.StopAsync();
}
