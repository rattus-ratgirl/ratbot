namespace RatBot.Infrastructure.Settings;

public record QuorumSettingsEntity
{
    public ulong GuildId { get; init; }

    public QuorumSettingsType TargetType { get; init; }

    public ulong TargetId { get; init; }

    public double QuorumProportion { get; init; }

    public required List<RoleEntity> Roles { get; init; }
}