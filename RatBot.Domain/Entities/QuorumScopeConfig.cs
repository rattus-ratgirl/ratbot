using System.ComponentModel.DataAnnotations.Schema;
using RatBot.Domain.Enums;

namespace RatBot.Domain.Entities;

/// <summary>
/// Represents quorum configuration for a guild scope (channel or category).
/// </summary>
[Table("QuorumScopeConfigs")]
public sealed record QuorumScopeConfig
{
    private static readonly IReadOnlyList<ulong> EmptyRoleIds = Array.Empty<ulong>();

    private QuorumScopeConfig() { }

    public ulong GuildId { get; private set; }

    public QuorumScopeType ScopeType { get; private set; }

    public ulong ScopeId { get; private set; }

    public IReadOnlyList<ulong> RoleIds { get; private set; } = EmptyRoleIds;

    public double QuorumProportion { get; private set; }

    public static QuorumScopeConfig Create(ulong guildId, QuorumScopeType scopeType, ulong scopeId, ulong roleId, double quorumProportion)
    {
        return Create(guildId, scopeType, scopeId, new[] { roleId }, quorumProportion);
    }

    public static QuorumScopeConfig Create(
        ulong guildId,
        QuorumScopeType scopeType,
        ulong scopeId,
        IEnumerable<ulong> roleIds,
        double quorumProportion
    )
    {
        QuorumScopeConfig config = new QuorumScopeConfig
        {
            GuildId = RequireDiscordId(guildId, nameof(guildId)),
            ScopeType = RequireScopeType(scopeType),
            ScopeId = RequireDiscordId(scopeId, nameof(scopeId)),
        };

        config.Reconfigure(roleIds, quorumProportion);
        return config;
    }

    private static ulong RequireDiscordId(ulong value, string paramName) =>
        value == 0 ? throw new ArgumentOutOfRangeException(paramName, "Discord identifiers must be non-zero.") : value;

    private static QuorumScopeType RequireScopeType(QuorumScopeType scopeType) =>
        !Enum.IsDefined(scopeType) ? throw new ArgumentOutOfRangeException(nameof(scopeType), "Invalid quorum scope type.") : scopeType;

    private static IReadOnlyList<ulong> RequireRoleIds(IEnumerable<ulong> roleIds)
    {
        ArgumentNullException.ThrowIfNull(roleIds);

        List<ulong> validatedRoleIds = roleIds.Select(roleId => RequireDiscordId(roleId, nameof(roleIds))).Distinct().ToList();

        if (validatedRoleIds.Count == 0)
            throw new ArgumentOutOfRangeException(nameof(roleIds), "At least one role identifier must be provided.");

        return validatedRoleIds;
    }

    private static double RequireQuorumProportion(double value) =>
        double.IsNaN(value) || double.IsInfinity(value)
            ? throw new ArgumentOutOfRangeException(nameof(value), "Quorum proportion must be a finite number.")
        : value is <= 0 or > 1 ? throw new ArgumentOutOfRangeException(nameof(value), "Quorum proportion must be greater than 0 and at most 1.")
        : value;

    public void Reconfigure(ulong roleId, double quorumProportion)
    {
        Reconfigure(new[] { roleId }, quorumProportion);
    }

    public void Reconfigure(IEnumerable<ulong> roleIds, double quorumProportion)
    {
        RoleIds = RequireRoleIds(roleIds);
        QuorumProportion = RequireQuorumProportion(quorumProportion);
    }
}
