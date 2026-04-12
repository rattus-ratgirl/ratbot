using ErrorOr;
using RatBot.Application.Common.Discord;
using Shouldly;

namespace RatBot.Domain.Tests;

public sealed class DiscordUtilsTests
{
    [Test]
    public void SplitMessageIntoChunks_WithExactLimit_ReturnsOneChunk()
    {
        // Arrange
        string message = new string('a', 2000);

        // Act
        string[] chunks = DiscordUtils.SplitMessageIntoChunks(message).Value;

        // Assert
        chunks.ShouldBe([message]);
    }

    [Test]
    public void SplitMessageIntoChunks_WithMessageOverLimit_ReturnsTwoChunks()
    {
        // Arrange
        string message = new string('a', 2001);

        // Act
        string[] chunks = DiscordUtils.SplitMessageIntoChunks(message).Value;

        // Assert
        chunks.Length.ShouldBe(2);
        chunks[0].Length.ShouldBe(2000);
        chunks[1].ShouldBe("a");
    }

    [Test]
    public void SplitMessageIntoChunks_WithNewlineBeforeLimit_SplitsAtNewline()
    {
        // Arrange
        string firstLine = new string('a', 10);
        string secondLine = new string('b', 10);
        string message = $"{firstLine}\n{secondLine}";

        // Act
        string[] chunks = DiscordUtils.SplitMessageIntoChunks(message, 12).Value;

        // Assert
        chunks.ShouldBe([$"{firstLine}\n", secondLine]);
    }

    [Test]
    public void SplitMessageIntoChunks_WithNoNewlineBeforeLimit_SplitsAtLimit()
    {
        // Arrange
        string message = "abcdef";

        // Act
        string[] chunks = DiscordUtils.SplitMessageIntoChunks(message, 3).Value;

        // Assert
        chunks.ShouldBe(["abc", "def"]);
    }

    [Test]
    public void SplitMessageIntoChunks_WithInvalidChunkSize_ReturnsValidationError()
    {
        // Arrange

        // Act
        ErrorOr<string[]> result = DiscordUtils.SplitMessageIntoChunks("message", 0);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.Validation);
    }
}