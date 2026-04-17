namespace RatBot.Domain.Primitives;

public sealed record UserSnowflake(ulong Id) : SnowflakeBase(Id)
{
    public static implicit operator ulong(UserSnowflake snowflake) => snowflake.Id;
    public static implicit operator UserSnowflake(ulong id) => new UserSnowflake(id);
    
    public override string ToMention() => $"<@{Id}>";
}
