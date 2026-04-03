namespace RatBot.Interactions.Common.Responses;

/// <summary>
/// Represents a Discord interaction response whose visibility is chosen at runtime.
/// </summary>
public sealed record InteractionResponse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InteractionResponse"/> class.
    /// </summary>
    /// <param name="content">The response text content.</param>
    /// <param name="isEphemeral">
    /// <see langword="true"/> to send an ephemeral response; otherwise, <see langword="false"/>.
    /// </param>
    public InteractionResponse(string content, bool isEphemeral = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        Content = content;
        IsEphemeral = isEphemeral;
    }

    /// <summary>
    /// Gets the text content to send to Discord.
    /// </summary>
    public string Content { get; }

    /// <summary>
    /// Gets a value indicating whether the response should be sent ephemerally.
    /// </summary>
    public bool IsEphemeral { get; }

    /// <summary>
    /// Creates an ephemeral response.
    /// </summary>
    public static InteractionResponse Ephemeral(string content) => new InteractionResponse(content);

    /// <summary>
    /// Creates a non-ephemeral response.
    /// </summary>
    public static InteractionResponse Public(string content) => new InteractionResponse(content, false);
}
