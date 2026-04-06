// ReSharper disable UnusedType.Global
namespace RatBot.Interactions.Features.Hello;

/// <summary>
/// Defines greeting interactions.
/// </summary>
public sealed partial class HelloModule : SlashCommandBase
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="HelloModule"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public HelloModule(ILogger logger)
    {
        _logger = logger.ForContext<HelloModule>();
    }
}
