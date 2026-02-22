using System.ComponentModel.DataAnnotations.Schema;
using RatBot.Domain.Enums;

namespace RatBot.Domain.Entities;

[Table("QuorumScopeConfigs")]
public sealed class QuorumScopeConfig
{
    public required ulong GuildId { get; set; }
    public required QuorumScopeType ScopeType { get; set; }
    public required ulong ScopeId { get; set; }
    public required ulong RoleId { get; set; }
    public required double QuorumProportion { get; set; }
}
