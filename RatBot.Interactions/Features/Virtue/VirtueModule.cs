using Discord.Interactions;
using RatBot.Infrastructure.Services;
using RatBot.Interactions.Common.Discord;

namespace RatBot.Interactions.Features.Virtue;

/// <summary>
/// Defines virtue interactions.
/// </summary>
[Group("virtue", "Virtue commands.")]
public sealed partial class VirtueModule : SlashCommandBase
{
    private readonly UserVirtueService _userVirtueService;

    public VirtueModule(UserVirtueService userVirtueService)
    {
        _userVirtueService = userVirtueService;
    }
}
