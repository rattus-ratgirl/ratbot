using System.ComponentModel.DataAnnotations.Schema;

namespace RatBot.Domain.Entities;

[Table("VirtueRoleTierConfigs")]
public sealed class VirtueRoleTierConfig
{
    public required ulong GuildId { get; set; }
    public required int TierIndex { get; set; }
    public required ulong RoleId { get; set; }
    public required int MinVirtue { get; set; }
    public required int MaxVirtue { get; set; }
}
