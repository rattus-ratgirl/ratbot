using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RatBot.Application.Features.AdminSay;
using RatBot.Application.Features.Quorum;
using RatBot.Application.Features.Rps;
using RatBot.Infrastructure.Configuration.Quorum;
using RatBot.Infrastructure.Data;
using RatBot.Infrastructure.Persistence;

namespace RatBot.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = PostgresConnectionStringBuilder.Build(configuration);

        services.AddDbContext<BotDbContext>(options => options.UseNpgsql(connectionString));
        services.AddDbContextFactory<BotDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IBotDataContext>(sp => sp.GetRequiredService<BotDbContext>());

        services.AddSingleton<IAdminSaySessionRepository, InMemoryAdminSaySessionRepository>();
        services.AddSingleton<IRpsGameRepository, InMemoryRpsGameRepository>();
        services.AddScoped<IQuorumConfigurationRepository, EfQuorumConfigurationRepository>();

        return services;
    }
}