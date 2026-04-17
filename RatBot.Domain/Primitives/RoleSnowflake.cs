namespace RatBot.Domain.Primitives;

public record RoleSnowflake(ulong Id) : SnowflakeBase(Id)
{
    public static implicit operator ulong(RoleSnowflake snowflake) => snowflake.Id;
    public static implicit operator RoleSnowflake(ulong id) => new RoleSnowflake(id);
    
    public override string ToMention() => $"<@&{Id}>";
}