using RatBot.Discord.Handlers;

namespace RatBot.Discord.Gateway;

public sealed class UserUpdatedGatewayHandler(
    DiscordSocketClient discordClient,
    IRoleColourReconciler reconciler,
    ILogger logger)
    : IDiscordGatewayHandler
{
    private readonly ILogger _logger = logger.ForContext<UserUpdatedGatewayHandler>();

    public Task InitializeAsync(CancellationToken ct)
    {
        Subscribe();
        return Task.CompletedTask;
    }

    public void Unsubscribe() => discordClient.GuildMemberUpdated -= HandleGuildMemberUpdatedAsync;

    private void Subscribe() => discordClient.GuildMemberUpdated += HandleGuildMemberUpdatedAsync;

    private async Task HandleGuildMemberUpdatedAsync(
        Cacheable<SocketGuildUser, ulong> before,
        SocketGuildUser after)
    {
        try
        {
            // We only care about role changes; if we can't get the before snapshot, conservatively reconcile.
            IReadOnlyCollection<ulong>? beforeRoles = null;

            if (before.HasValue)
                beforeRoles = ((IGuildUser)before.Value).RoleIds;

            IReadOnlyCollection<ulong> afterRoles = ((IGuildUser)after).RoleIds;

            bool rolesChanged = beforeRoles is null
                || beforeRoles.Count != afterRoles.Count
                || !beforeRoles.OrderBy(x => x).SequenceEqual(afterRoles.OrderBy(x => x));

            if (!rolesChanged)
                return;

            _logger
                .ForContext("GuildId", after.Guild.Id)
                .ForContext("UserId", after.Id)
                .ForContext("BeforeRoleCount", beforeRoles?.Count)
                .ForContext("AfterRoleCount", afterRoles.Count)
                .Debug("Guild member roles changed; triggering role-colour reconciliation.");

            await reconciler.ReconcileMemberAsync(after.Guild, after.Id, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger
                .ForContext("GuildId", after.Guild.Id)
                .ForContext("UserId", after.Id)
                .Error(ex, "Failed to reconcile after guild member update.");
        }
    }
}