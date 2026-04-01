using Discord.Interactions;
using RatBot.Infrastructure.Services;

namespace RatBot.Interactions;

[Group("virtue", "Virtue commands.")]
public sealed class VirtueModule(UserVirtueService userVirtueService) : SlashCommandBase
{
    [SlashCommand("show", "Show your current virtue.")]
    public async Task ShowAsync()
    {
        if (!await TryDeferEphemeralAsync())
            return;

        int? virtue = await userVirtueService.TryGetVirtueAsync(Context.User.Id);
        if (virtue is null)
        {
            await SendEphemeralAsync("You do not have a recorded virtue yet.");
            return;
        }

        await SendEphemeralAsync($"Your virtue is `{virtue.Value}`.");
    }
}
