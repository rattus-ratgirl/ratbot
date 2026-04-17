namespace RatBot.Domain.Primitives;

public abstract record SnowflakeBase(ulong Id)
{
    private const ulong DiscordEpoch = 1420070400000;

    public DateTime Timestamp => DateTimeOffset.FromUnixTimeMilliseconds((long)((Id >> 22) + DiscordEpoch)).DateTime;
    public override string ToString() => Id.ToString();

    public abstract string ToMention();
}