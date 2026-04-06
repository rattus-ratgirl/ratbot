using RatBot.Infrastructure.Services;

namespace RatBot.Interactions.Features.Quorum;

/// <summary>
/// Defines quorum-related interactions.
/// </summary>
[Group("quorum", "Quorum commands.")]
public sealed partial class QuorumModule : SlashCommandBase
{
    private readonly ILogger _logger;
    private readonly IConfigRepository _configRepository;

    public QuorumModule(ILogger logger, IConfigRepository configRepository)
    {
        _logger = logger;
        _configRepository = configRepository;
    }
}
