using RatBot.Infrastructure.Services;

namespace RatBot.Interactions.Features.Quorum;

/// <summary>
/// Defines quorum-related interactions.
/// </summary>
[Group("quorum", "Quorum commands.")]
public sealed partial class QuorumModule : SlashCommandBase
{
    private readonly ILogger _logger;
    private readonly QuorumConfigService _quorumConfigService;

    public QuorumModule(ILogger logger, QuorumConfigService quorumConfigService)
    {
        _logger = logger;
        _quorumConfigService = quorumConfigService;
    }
}
