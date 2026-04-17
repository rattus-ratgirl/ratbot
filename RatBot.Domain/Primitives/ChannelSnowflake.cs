namespace RatBot.Domain.Primitives;

public sealed record ChannelSnowflake(ulong Id) : SnowflakeBase(Id)
{
    public static implicit operator ulong(ChannelSnowflake snowflake) => snowflake.Id;
    public static implicit operator ChannelSnowflake(ulong id) => new ChannelSnowflake(id);
    
    public override string ToMention() => $"<#{Id}>";
}