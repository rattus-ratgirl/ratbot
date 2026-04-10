namespace RatBot.Infrastructure.Data;

/// <summary>
/// Legacy migration-only config entry entity retained so historical EF migrations continue to compile.
/// </summary>
public sealed class ConfigEntry
{
    public required string Key { get; set; }

    public required string Value { get; set; }
}
