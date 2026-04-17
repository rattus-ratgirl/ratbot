namespace RatBot.Domain.Primitives;

public sealed record GuildSnowflake(ulong Id) : SnowflakeBase(Id)
{
    public static implicit operator ulong(GuildSnowflake snowflake) => snowflake.Id;
    public static implicit operator GuildSnowflake(ulong id) => new GuildSnowflake(id);
    
    public override string ToMention() => throw new InvalidOperationException("Guilds cannot be mentioned.");
}