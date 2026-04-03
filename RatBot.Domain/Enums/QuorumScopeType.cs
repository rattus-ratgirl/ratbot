namespace RatBot.Domain.Enums;

/// <summary>
/// Identifies the scope level used for quorum configuration.
/// </summary>
public enum QuorumScopeType
{
    /// <summary>
    /// Scope corresponds to an individual text channel.
    /// </summary>
    Channel = 1,

    /// <summary>
    /// Scope corresponds to a channel category.
    /// </summary>
    Category = 2,
}
