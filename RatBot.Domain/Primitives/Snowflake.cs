namespace RatBot.Domain.Primitives;

public readonly record struct Snowflake
{
    private const long DiscordEpoch = 1420070400000;

    public long Id { get; init; }

    public DateTime Timestamp => DateTimeOffset.FromUnixTimeMilliseconds((Id >> 22) + DiscordEpoch).DateTime;
    public override string ToString() => Id.ToString();
}