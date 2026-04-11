using RatBot.Domain.Common;

namespace RatBot.Domain.Tests;

public class MentionParserTests
{
    [Theory]
    [InlineData("<@123>", 123)]
    [InlineData("<@!123>", 123)]
    [InlineData("<@&456>", 456)]
    [InlineData("<#789>", 789)]
    public void Parse_ValidMention_ReturnsId(string mention, ulong expectedId)
    {
        Assert.True(MentionParser.TryParse(mention, out ulong actualId));
        Assert.Equal(expectedId, actualId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("123")]
    [InlineData("<@abc>")]
    [InlineData("<#def>")]
    [InlineData("<@&ghi>")]
    public void Parse_InvalidMention_ThrowsArgumentException(string mention)
    {
        Assert.Throws<ArgumentException>(() => MentionParser.TryParse(mention, out _));
    }
}