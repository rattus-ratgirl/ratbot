namespace RatBot.Application.Features.Quorum;

public interface IQuorumSettingsRepository
{
    Task<QuorumSettings?> GetAsync(
        ulong guildId,
        QuorumSettingsType targetType,
        ulong targetId,
        CancellationToken ct = default);

    Task<bool> UpsertAsync(QuorumSettings config, CancellationToken ct = default);

    Task<bool> DeleteAsync(ulong guildId, QuorumSettingsType targetType, ulong targetId, CancellationToken ct = default);
}