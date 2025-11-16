namespace RatBot.Infrastructure.Data;

public static class MySqlConnectionStringBuilder
{
    public static string Build(IConfiguration config)
    {
        string host = config["DB:Host"] ?? throw new InvalidOperationException("DB:Host not configured");
        string port = config["DB:Port"] ?? throw new InvalidOperationException("DB:Port not configured");
        string database = config["DB:Database"] ?? throw new InvalidOperationException("DB:Database not configured");
        string user = config["DB:User"] ?? throw new InvalidOperationException("DB:User not configured");
        string password = config["DB:Password"] ?? throw new InvalidOperationException("DB:Password not configured");

        return $"Server={host};Port={port};Database={database};User Id={user};Password={password};SslMode=Preferred";
    }
}
