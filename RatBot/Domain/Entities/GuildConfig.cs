using System.ComponentModel.DataAnnotations.Schema;

namespace RatBot.Domain.Entities;

[Table("GuildConfigs", Schema = "config")]
public sealed class GuildConfig
{
    public required ulong GuildId { get; set; }
    public required string Prefix { get; set; }
}
