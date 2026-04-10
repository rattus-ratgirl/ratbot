namespace RatBot.Application.Features.Quorum;

public interface IQuorumConfigurationRepository
{
    Task<QuorumScopeConfig?> GetAsync(
        ulong guildId,
        QuorumScopeType scopeType,
        ulong scopeId,
        CancellationToken ct = default);

    Task<bool> UpsertAsync(QuorumScopeConfig config, CancellationToken ct = default);

    Task<bool> DeleteAsync(
        ulong guildId,
        QuorumScopeType scopeType,
        ulong scopeId,
        CancellationToken ct = default);
}
