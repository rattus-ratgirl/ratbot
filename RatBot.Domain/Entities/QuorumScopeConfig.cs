using System.ComponentModel.DataAnnotations.Schema;
using RatBot.Domain.Enums;

namespace RatBot.Domain.Entities;

/// <summary>
/// Represents quorum configuration for a guild scope (channel or category).
/// </summary>
[Table("QuorumScopeConfigs")]
public sealed record QuorumScopeConfig
{
    private QuorumScopeConfig() { }

    public ulong GuildId { get; private set; }

    public QuorumScopeType ScopeType { get; private set; }

    public ulong ScopeId { get; private set; }

    public ulong RoleId { get; private set; }

    public double QuorumProportion { get; private set; }

    public static QuorumScopeConfig Create(ulong guildId, QuorumScopeType scopeType, ulong scopeId, ulong roleId, double quorumProportion)
    {
        QuorumScopeConfig config = new QuorumScopeConfig
        {
            GuildId = RequireDiscordId(guildId, nameof(guildId)),
            ScopeType = RequireScopeType(scopeType),
            ScopeId = RequireDiscordId(scopeId, nameof(scopeId)),
        };

        config.Reconfigure(roleId, quorumProportion);
        return config;
    }

    public void Reconfigure(ulong roleId, double quorumProportion)
    {
        RoleId = RequireDiscordId(roleId, nameof(roleId));
        QuorumProportion = RequireQuorumProportion(quorumProportion);
    }

    private static ulong RequireDiscordId(ulong value, string paramName) =>
        value == 0 ? throw new ArgumentOutOfRangeException(paramName, "Discord identifiers must be non-zero.") : value;

    private static QuorumScopeType RequireScopeType(QuorumScopeType scopeType) =>
        !Enum.IsDefined(scopeType) ? throw new ArgumentOutOfRangeException(nameof(scopeType), "Invalid quorum scope type.") : scopeType;

    private static double RequireQuorumProportion(double value) =>
        double.IsNaN(value) || double.IsInfinity(value)
            ? throw new ArgumentOutOfRangeException(nameof(value), "Quorum proportion must be a finite number.")
        : value is <= 0 or > 1 ? throw new ArgumentOutOfRangeException(nameof(value), "Quorum proportion must be greater than 0 and at most 1.")
        : value;
}
