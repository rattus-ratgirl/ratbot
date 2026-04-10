namespace RatBot.Infrastructure.Configuration.Quorum;

public sealed class QuorumScopeConfigRoleEntity
{
    public ulong GuildId { get; set; }

    public QuorumScopeType ScopeType { get; set; }

    public ulong ScopeId { get; set; }

    public ulong RoleId { get; set; }

    public QuorumScopeConfigEntity Config { get; set; } = null!;
}
