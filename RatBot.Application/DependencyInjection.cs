using Microsoft.Extensions.DependencyInjection;
using RatBot.Application.Features.AdminSay;
using RatBot.Application.Features.Emoji;
using RatBot.Application.Features.Quorum;
using RatBot.Application.Features.Rps;

namespace RatBot.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<AdminSayWorkflowService>();
        services.AddScoped<EmojiAnalyticsService>();
        services.AddSingleton<EmojiAnalyticsBuffer>();
        services.AddScoped<QuorumConfigurationService>();
        services.AddScoped<RpsGameService>();

        return services;
    }
}
