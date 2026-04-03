using System.ComponentModel.DataAnnotations.Schema;

namespace RatBot.Domain.Entities;

/// <summary>
/// Represents guild-level configuration values.
/// </summary>
[Table("GuildConfigs")]
public sealed class GuildConfig
{
    /// <summary>
    /// Gets or sets the Discord guild identifier.
    /// </summary>
    public required ulong GuildId { get; set; }

    /// <summary>
    /// Gets or sets the text command prefix for the guild.
    /// </summary>
    public required string Prefix { get; set; }
}
