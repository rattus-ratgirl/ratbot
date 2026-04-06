using System.Text.Json.Serialization;
using LanguageExt;

namespace RatBot.Domain.Features.Quorum;

/// <summary>
/// Represents quorum behaviour for a guild scope.
/// </summary>
public sealed class QuorumScopeConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QuorumScopeConfig"/> class.
    /// </summary>
    /// <param name="guildId">The guild identifier.</param>
    /// <param name="scopeType">The scope type.</param>
    /// <param name="scopeId">The scope identifier.</param>
    /// <param name="roleIds">The role identifiers used for quorum counting.</param>
    /// <param name="quorumProportion">The quorum proportion.</param>
    [JsonConstructor]
    public QuorumScopeConfig(ulong guildId, QuorumScopeType scopeType, ulong scopeId, IEnumerable<ulong> roleIds, double quorumProportion)
    {
        GuildId = RequireDiscordId(guildId, nameof(guildId));
        ScopeType = RequireScopeType(scopeType);
        ScopeId = RequireDiscordId(scopeId, nameof(scopeId));
        RoleIds = RequireRoleIds(roleIds);
        QuorumProportion = RequireQuorumProportion(quorumProportion);
    }

    /// <summary>
    /// Gets the guild identifier.
    /// </summary>
    public ulong GuildId { get; }

    /// <summary>
    /// Gets the scope type.
    /// </summary>
    public QuorumScopeType ScopeType { get; }

    /// <summary>
    /// Gets the scope identifier.
    /// </summary>
    public ulong ScopeId { get; }

    /// <summary>
    /// Gets the role identifiers used for quorum counting.
    /// </summary>
    public Arr<ulong> RoleIds { get; }

    /// <summary>
    /// Gets the quorum proportion.
    /// </summary>
    public double QuorumProportion { get; }

    /// <summary>
    /// Creates a new config using a single role identifier.
    /// </summary>
    /// <param name="guildId">The guild identifier.</param>
    /// <param name="scopeType">The scope type.</param>
    /// <param name="scopeId">The scope identifier.</param>
    /// <param name="roleId">The role identifier.</param>
    /// <param name="quorumProportion">The quorum proportion.</param>
    /// <returns>The created config.</returns>
    public static QuorumScopeConfig Create(
        ulong guildId,
        QuorumScopeType scopeType,
        ulong scopeId,
        ulong roleId,
        double quorumProportion
    ) => Create(guildId, scopeType, scopeId, new Arr<ulong>([roleId]), quorumProportion);

    /// <summary>
    /// Creates a new config.
    /// </summary>
    /// <param name="guildId">The guild identifier.</param>
    /// <param name="scopeType">The scope type.</param>
    /// <param name="scopeId">The scope identifier.</param>
    /// <param name="roleIds">The role identifiers.</param>
    /// <param name="quorumProportion">The quorum proportion.</param>
    /// <returns>The created config.</returns>
    public static QuorumScopeConfig Create(
        ulong guildId,
        QuorumScopeType scopeType,
        ulong scopeId,
        Arr<ulong> roleIds,
        double quorumProportion
    ) => new QuorumScopeConfig(guildId, scopeType, scopeId, roleIds, quorumProportion);

    private static ulong RequireDiscordId(ulong value, string paramName) =>
        value == 0
            ? throw new ArgumentOutOfRangeException(paramName, "Discord identifiers must be non-zero.")
            : value;

    private static QuorumScopeType RequireScopeType(QuorumScopeType scopeType) =>
        !Enum.IsDefined(scopeType)
            ? throw new ArgumentOutOfRangeException(nameof(scopeType), "Invalid quorum scope type.")
            : scopeType;

    private static Arr<ulong> RequireRoleIds(IEnumerable<ulong> roleIds)
    {
        ArgumentNullException.ThrowIfNull(roleIds);

        Arr<ulong> validatedRoleIds = new Arr<ulong>(roleIds
            .Select(roleId => RequireDiscordId(roleId, nameof(roleIds)))
            .Distinct());

        return validatedRoleIds.IsEmpty
            ? throw new ArgumentOutOfRangeException(nameof(roleIds), "At least one role identifier must be provided.")
            : validatedRoleIds;
    }

    private static double RequireQuorumProportion(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            throw new ArgumentOutOfRangeException(nameof(value), "Quorum proportion must be a finite number.");

        if (value is <= 0 or > 1)
            throw new ArgumentOutOfRangeException(
                nameof(value),
                "Quorum proportion must be greater than 0 and at most 1."
            );

        return value;
    }

    /// <summary>
    /// Creates a replacement config using a single role identifier.
    /// </summary>
    /// <param name="roleId">The role identifier.</param>
    /// <param name="quorumProportion">The quorum proportion.</param>
    /// <returns>The replacement config.</returns>
    public QuorumScopeConfig Reconfigure(ulong roleId, double quorumProportion) =>
        Reconfigure(new Arr<ulong>([roleId]), quorumProportion);

    /// <summary>
    /// Creates a replacement config.
    /// </summary>
    /// <param name="roleIds">The role identifiers.</param>
    /// <param name="quorumProportion">The quorum proportion.</param>
    /// <returns>The replacement config.</returns>
    public QuorumScopeConfig Reconfigure(Arr<ulong> roleIds, double quorumProportion) =>
        new QuorumScopeConfig(GuildId, ScopeType, ScopeId, roleIds, quorumProportion);
}
