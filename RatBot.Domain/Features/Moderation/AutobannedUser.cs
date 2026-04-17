using RatBot.Domain.Primitives;

namespace RatBot.Domain.Features.Moderation;

public sealed record AutobannedUser
{
    private AutobannedUser()
    {
        BannedUser = null!;
        Moderator = null!;
        GuildId = null!;
    }

    public GuildSnowflake GuildId { get; private init; }
    public UserSnowflake BannedUser { get; private init; }
    public UserSnowflake Moderator { get; private init; }
    public DateTimeOffset RegisteredAtUtc { get; private init; }

    public static AutobannedUser Create(
        GuildSnowflake guildId,
        UserSnowflake bannedUser,
        UserSnowflake moderator,
        DateTimeOffset registeredAtUtc)
    {
        return new AutobannedUser
        {
            GuildId = guildId,
            BannedUser = bannedUser,
            Moderator = moderator,
            RegisteredAtUtc = registeredAtUtc
        };
    }
}
