using Microsoft.EntityFrameworkCore.Design;

namespace RatBot.Infrastructure.Data;

public sealed class DesignTimeBotDbContextFactory : IDesignTimeDbContextFactory<BotDbContext>
{
    public BotDbContext CreateDbContext(string[] args)
    {
        Env.TraversePath().Load();

        IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddEnvironmentVariables().Build();

        string? connectionString =
            configurationRoot.GetConnectionString("BotDb")
            ?? configurationRoot["DB:ConnectionString"]
            ?? Environment.GetEnvironmentVariable("DB__CONNECTION_STRING")
            ?? BuildFromDiscrete(configurationRoot);

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "Design-time connection string missing. Set DB__CONNECTION_STRING or discrete DB__* vars."
            );

        DbContextOptions<BotDbContext> options = new DbContextOptionsBuilder<BotDbContext>()
            .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
            .Options;

        return new BotDbContext(options);
    }

    private static string? BuildFromDiscrete(IConfiguration c)
    {
        string? host = c["DB:Host"] ?? c["Database:Host"] ?? Environment.GetEnvironmentVariable("DB__HOST");
        string? port = c["DB:Port"] ?? c["Database:Port"] ?? Environment.GetEnvironmentVariable("DB__PORT");
        string? db = c["DB:Database"] ?? c["Database:Name"] ?? Environment.GetEnvironmentVariable("DB__DATABASE");
        string? user = c["DB:User"] ?? c["Database:User"] ?? Environment.GetEnvironmentVariable("DB__USER");
        string? pwd = c["DB:Password"] ?? c["Database:Password"] ?? Environment.GetEnvironmentVariable("DB__PASSWORD");

        if (
            string.IsNullOrWhiteSpace(host)
            || string.IsNullOrWhiteSpace(db)
            || string.IsNullOrWhiteSpace(user)
            || string.IsNullOrWhiteSpace(pwd)
        )
            return null;

        string portPart = string.IsNullOrWhiteSpace(port) ? string.Empty : $";Port={port}";
        return $"Server={host}{portPart};Database={db};User Id={user};Password={pwd};SslMode=Preferred";
    }
}
