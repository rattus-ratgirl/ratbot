using Discord.Commands;
using Discord.Interactions;
using RatBot.Discord;
using RatBot.Infrastructure.Data;
using RatBot.Infrastructure.Services;

namespace RatBot;

public static class Program
{
    public static async Task Main(string[] args)
    {
        Env.TraversePath().Load();

        using IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((_, configurationBuilder) => { configurationBuilder.AddEnvironmentVariables(); }
            )
            .UseSerilog((_, _, loggerConfiguration) =>
                loggerConfiguration
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .WriteTo.File("logs/ratbot-.log", rollingInterval: RollingInterval.Day)
            )
            .ConfigureServices((ctx, services) =>
                {
                    IConfiguration config = ctx.Configuration;

                    #region Discord Core Services

                    services.AddSingleton(_ => new DiscordSocketClient(
                        new DiscordSocketConfig
                        {
                            GatewayIntents =
                                GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent,
                        }
                    ));

                    services.AddSingleton(sp => new InteractionService(sp.GetRequiredService<DiscordSocketClient>()));
                    services.AddSingleton<CommandService>();
                    services.AddSingleton<DiscordBotService>();
                    services.AddHostedService<DiscordBotHostedService>();

                    #endregion

                    #region EF Core Services

                    string connectionString = MySqlConnectionStringBuilder.Build(config);

                    services.AddDbContext<BotDbContext>(opt =>
                        opt.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
                    );

                    #endregion

                    #region Application Services

                    services.AddScoped<GuildConfigService>();
                    services.AddScoped<QuorumConfigService>();

                    #endregion
                }
            )
            .Build();

        await ApplyDatabaseMigrationsAsync(host);

        await host.RunAsync();
    }

    private static async Task ApplyDatabaseMigrationsAsync(IHost host)
    {
        try
        {
            await using AsyncServiceScope scope = host.Services.CreateAsyncScope();
            BotDbContext dbContext = scope.ServiceProvider.GetRequiredService<BotDbContext>();

            // If there are migrations, apply pending ones; otherwise ensure database is created (first run)
            List<string> pending = (await dbContext.Database.GetPendingMigrationsAsync()).ToList();
            if (pending.Count != 0)
            {
                Console.WriteLine($"[DB] Applying {pending.Count} pending migration(s)...");
                await dbContext.Database.MigrateAsync();
                Console.WriteLine("[DB] Migrations applied successfully.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DB] Migration/creation failed: {ex}");
            throw;
        }
    }
}
