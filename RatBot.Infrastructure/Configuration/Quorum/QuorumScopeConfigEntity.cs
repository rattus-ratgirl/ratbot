namespace RatBot.Infrastructure.Configuration.Quorum;

public sealed class QuorumScopeConfigEntity
{
    public ulong GuildId { get; set; }

    public QuorumScopeType ScopeType { get; set; }

    public ulong ScopeId { get; set; }

    public double QuorumProportion { get; set; }

    public List<QuorumScopeConfigRoleEntity> Roles { get; set; } = [];
}
