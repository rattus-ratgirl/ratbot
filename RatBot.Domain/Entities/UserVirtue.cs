using System.ComponentModel.DataAnnotations.Schema;

namespace RatBot.Domain.Entities;

/// <summary>
/// Represents the tracked virtue score for a Discord user.
/// </summary>
[Table("UserScores")]
public sealed class UserVirtue
{
    /// <summary>
    /// Gets the Discord user identifier.
    /// </summary>
    public required ulong UserId { get; init; }

    /// <summary>
    /// Gets or sets the user's virtue score.
    /// </summary>
    public required int Virtue { get; set; }
}
