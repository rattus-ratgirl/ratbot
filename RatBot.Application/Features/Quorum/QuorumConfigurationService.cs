namespace RatBot.Application.Features.Quorum;

public sealed class QuorumConfigurationService(IQuorumConfigurationRepository repository, ILogger logger)
{
    private readonly ILogger _logger = logger.ForContext<QuorumConfigurationService>();

    public async Task<(bool Created, QuorumScopeConfig Config)> UpsertAsync(
        ulong guildId,
        QuorumScopeType scopeType,
        ulong scopeId,
        IReadOnlyCollection<ulong> roleIds,
        double quorumProportion,
        CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfZero(guildId);
        ArgumentOutOfRangeException.ThrowIfZero(scopeId);
        ArgumentNullException.ThrowIfNull(roleIds);

        QuorumScopeConfig config = new QuorumScopeConfig(
            guildId,
            scopeType,
            scopeId,
            roleIds.Distinct().ToArray(),
            quorumProportion);
        bool created = await repository.UpsertAsync(config, ct);

        _logger.Information(
            "Quorum scope config {Action} for guild {GuildId}, scope type {ScopeType}, scope {ScopeId}.",
            created ? "created" : "updated",
            guildId,
            scopeType,
            scopeId);

        return (created, config);
    }

    public async Task<bool> DeleteAsync(
        ulong guildId,
        QuorumScopeType scopeType,
        ulong scopeId,
        CancellationToken ct = default)
    {
        bool deleted = await repository.DeleteAsync(guildId, scopeType, scopeId, ct);

        _logger.Information(
            "Quorum scope config delete attempted for guild {GuildId}, scope type {ScopeType}, scope {ScopeId}. Deleted={Deleted}",
            guildId,
            scopeType,
            scopeId,
            deleted);

        return deleted;
    }

    public async Task<QuorumScopeConfig?> GetEffectiveAsync(
        ulong guildId,
        ulong channelId,
        ulong? categoryId,
        CancellationToken ct = default)
    {
        QuorumScopeConfig? channelConfig = await repository.GetAsync(guildId, QuorumScopeType.Channel, channelId, ct);
        if (channelConfig is not null)
            return channelConfig;

        return categoryId is null
            ? null
            : await repository.GetAsync(guildId, QuorumScopeType.Category, categoryId.Value, ct);
    }
}
