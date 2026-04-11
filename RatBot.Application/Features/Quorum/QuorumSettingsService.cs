namespace RatBot.Application.Features.Quorum;

public sealed class QuorumSettingsService(IQuorumSettingsRepository repository, ILogger logger)
{
    private readonly ILogger _logger = logger.ForContext<QuorumSettingsService>();

    public async Task<(bool Created, QuorumSettings Config)> UpsertAsync(
        ulong guildId,
        QuorumSettingsType targetType,
        ulong targetId,
        IReadOnlyCollection<ulong> roleIds,
        double quorumProportion,
        CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfZero(guildId);
        ArgumentOutOfRangeException.ThrowIfZero(targetId);
        ArgumentNullException.ThrowIfNull(roleIds);

        QuorumSettings config = new QuorumSettings(
            guildId,
            targetType,
            targetId,
            roleIds.Distinct().ToArray(),
            quorumProportion);

        bool created = await repository.UpsertAsync(config, ct);

        _logger.Information(
            "Quorum settings {Action} for guild {GuildId}, target type {TargetType}, target {TargetId}.",
            created
                ? "created"
                : "updated",
            guildId,
            targetType,
            targetId);

        return (created, config);
    }

    public async Task<bool> DeleteAsync(
        ulong guildId,
        QuorumSettingsType targetType,
        ulong targetId,
        CancellationToken ct = default)
    {
        bool deleted = await repository.DeleteAsync(guildId, targetType, targetId, ct);

        _logger.Information(
            "Quorum settings delete attempted for guild {GuildId}, target type {TargetType}, target {TargetId}. Deleted={Deleted}",
            guildId,
            targetType,
            targetId,
            deleted);

        return deleted;
    }

    public async Task<QuorumSettings?> GetEffectiveAsync(
        ulong guildId,
        ulong channelId,
        ulong? categoryId,
        CancellationToken ct = default)
    {
        QuorumSettings? channelConfig = await repository.GetAsync(guildId, QuorumSettingsType.Channel, channelId, ct);

        if (channelConfig is not null)
            return channelConfig;

        return categoryId is null
            ? null
            : await repository.GetAsync(guildId, QuorumSettingsType.Category, categoryId.Value, ct);
    }
}