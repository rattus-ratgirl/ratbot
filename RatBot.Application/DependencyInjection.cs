using Microsoft.Extensions.DependencyInjection;
using RatBot.Application.Features.AdminSay;
using RatBot.Application.Features.Emoji;
using RatBot.Application.Features.Quorum;
using RatBot.Application.Features.Rps;

namespace RatBot.Application;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddApplication()
        {
            services.AddSingleton<EmojiAnalyticsBuffer>();

            services.AddScoped<AdminSayWorkflowService>();
            services.AddScoped<EmojiAnalyticsService>();
            services.AddScoped<QuorumSettingsService>();
            services.AddScoped<RpsGameService>();

            return services;
        }
    }
}