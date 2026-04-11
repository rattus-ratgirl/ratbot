namespace RatBot.Infrastructure.Settings;

public record QuorumSettings
{
    public ulong GuildId { get; init; }

    public QuorumSettingsType TargetType { get; init; }

    public ulong TargetId { get; init; }

    public double QuorumProportion { get; init; }

    public required List<Role> Roles { get; init; }
}