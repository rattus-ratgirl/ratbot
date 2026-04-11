using System.Text.Json;
using RatBot.Domain.Enums;
using RatBot.Domain.Features.Quorum;
using Shouldly;

namespace RatBot.Domain.Tests;

/// <summary>
///     Tests for the QuorumConfig domain model invariants.
/// </summary>
public sealed class QuorumSettingsInvariantTests
{
    private static ulong[] RoleIds(params ulong[] ids) => [..ids];

    [Fact]
    public void Create_WithValidParameters_ReturnsInstance()
    {
        QuorumSettings config = QuorumSettings.Create(
            123,
            QuorumSettingsType.Channel,
            456,
            RoleIds(10UL, 20UL, 10UL, 30UL),
            0.75);

        config.GuildId.ShouldBe(123UL);
        config.TargetType.ShouldBe(QuorumSettingsType.Channel);
        config.TargetId.ShouldBe(456UL);
        config.RoleIds.ShouldBe(RoleIds(10UL, 20UL, 30UL));
        config.QuorumProportion.ShouldBe(0.75);
    }

    [Fact]
    public void Create_SingleRoleOverload_SetsRoleAndProportion()
    {
        QuorumSettings config = QuorumSettings.Create(123, QuorumSettingsType.Category, 456, 99, 0.6);

        config.RoleIds.ShouldBe(RoleIds(99UL));
        config.QuorumProportion.ShouldBe(0.6);
    }

    [Fact]
    public void Create_WithZeroGuildId_ThrowsArgumentOutOfRange()
    {
        ArgumentOutOfRangeException ex = Should.Throw<ArgumentOutOfRangeException>(() =>
            QuorumSettings.Create(0, QuorumSettingsType.Channel, 1, RoleIds(1UL), 0.5));

        ex.ParamName.ShouldBe("guildId");
    }

    [Fact]
    public void Create_WithZeroTargetId_ThrowsArgumentOutOfRange()
    {
        ArgumentOutOfRangeException ex = Should.Throw<ArgumentOutOfRangeException>(() =>
            QuorumSettings.Create(1, QuorumSettingsType.Channel, 0, RoleIds(1UL), 0.5));

        ex.ParamName.ShouldBe("targetId");
    }

    [Fact]
    public void Create_WithInvalidTargetType_ThrowsArgumentOutOfRange()
    {
        ArgumentOutOfRangeException ex = Should.Throw<ArgumentOutOfRangeException>(() =>
            QuorumSettings.Create(1, (QuorumSettingsType)999, 2, RoleIds(1UL), 0.5));

        ex.ParamName.ShouldBe("targetType");
    }

    [Fact]
    public void Create_WithDefaultRoleIds_ThrowsArgumentOutOfRange()
    {
        ulong[] roleIds = [];

        ArgumentOutOfRangeException ex = Should.Throw<ArgumentOutOfRangeException>(() =>
            QuorumSettings.Create(1, QuorumSettingsType.Channel, 2, roleIds, 0.5));

        ex.ParamName.ShouldBe("roleIds");
    }

    [Fact]
    public void Create_WithEmptyRoleIds_ThrowsArgumentOutOfRange()
    {
        ArgumentOutOfRangeException ex = Should.Throw<ArgumentOutOfRangeException>(() =>
            QuorumSettings.Create(1, QuorumSettingsType.Channel, 2, RoleIds(), 0.5));

        ex.ParamName.ShouldBe("roleIds");
    }

    [Fact]
    public void Create_WithZeroRoleId_ThrowsArgumentOutOfRange()
    {
        ArgumentOutOfRangeException ex = Should.Throw<ArgumentOutOfRangeException>(() =>
            QuorumSettings.Create(1, QuorumSettingsType.Channel, 2, RoleIds(5UL, 0UL), 0.5));

        ex.ParamName.ShouldBe("roleIds");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.0001)]
    [InlineData(1.0001)]
    public void Create_WithOutOfRangeProportion_ThrowsArgumentOutOfRange(double quorumProportion)
    {
        ArgumentOutOfRangeException ex = Should.Throw<ArgumentOutOfRangeException>(() =>
            QuorumSettings.Create(1, QuorumSettingsType.Channel, 2, RoleIds(10UL), quorumProportion));

        ex.ParamName.ShouldBe("value");
    }

    [Fact]
    public void Create_WithNaNProportion_ThrowsArgumentOutOfRange()
    {
        ArgumentOutOfRangeException ex = Should.Throw<ArgumentOutOfRangeException>(() =>
            QuorumSettings.Create(1, QuorumSettingsType.Channel, 2, RoleIds(10UL), double.NaN));

        ex.ParamName.ShouldBe("value");
    }

    [Theory]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Create_WithInfiniteProportion_ThrowsArgumentOutOfRange(double quorumProportion)
    {
        ArgumentOutOfRangeException ex = Should.Throw<ArgumentOutOfRangeException>(() =>
            QuorumSettings.Create(1, QuorumSettingsType.Channel, 2, RoleIds(10UL), quorumProportion));

        ex.ParamName.ShouldBe("value");
    }

    [Fact]
    public void Reconfigure_WithValidInputs_UpdatesRolesAndProportion()
    {
        QuorumSettings config = QuorumSettings.Create(1, QuorumSettingsType.Channel, 2, RoleIds(11UL), 0.5);

        QuorumSettings reconfigured = config.Reconfigure(RoleIds(3UL, 2UL, 2UL, 1UL), 0.9);

        reconfigured.GuildId.ShouldBe(1UL);
        reconfigured.TargetType.ShouldBe(QuorumSettingsType.Channel);
        reconfigured.TargetId.ShouldBe(2UL);
        reconfigured.RoleIds.ShouldBe(RoleIds(3UL, 2UL, 1UL));
        reconfigured.QuorumProportion.ShouldBe(0.9);
    }

    [Fact]
    public void Reconfigure_WithSingleRoleOverload_UpdatesRolesAndProportion()
    {
        QuorumSettings config = QuorumSettings.Create(1, QuorumSettingsType.Channel, 2, RoleIds(11UL), 0.5);

        QuorumSettings reconfigured = config.Reconfigure(42UL, 1.0);

        reconfigured.GuildId.ShouldBe(1UL);
        reconfigured.TargetType.ShouldBe(QuorumSettingsType.Channel);
        reconfigured.TargetId.ShouldBe(2UL);
        reconfigured.RoleIds.ShouldBe(RoleIds(42UL));
        reconfigured.QuorumProportion.ShouldBe(1.0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1.01)]
    public void Reconfigure_WithOutOfRangeProportion_ThrowsArgumentOutOfRange(double quorumProportion)
    {
        QuorumSettings config = QuorumSettings.Create(1, QuorumSettingsType.Channel, 2, RoleIds(10UL), 0.5);

        ArgumentOutOfRangeException ex =
            Should.Throw<ArgumentOutOfRangeException>(() => _ = config.Reconfigure(RoleIds(10UL), quorumProportion));

        ex.ParamName.ShouldBe("value");
    }

    [Fact]
    public void Reconfigure_WithEmptyRoles_ThrowsArgumentOutOfRange()
    {
        QuorumSettings config = QuorumSettings.Create(1, QuorumSettingsType.Channel, 2, RoleIds(10UL), 0.5);

        ArgumentOutOfRangeException ex =
            Should.Throw<ArgumentOutOfRangeException>(() => _ = config.Reconfigure(RoleIds(), 0.5));

        ex.ParamName.ShouldBe("roleIds");
    }

    [Fact]
    public void JsonRoundTrip_WithPersistedShape_DeserializesSuccessfully()
    {
        QuorumSettings original = QuorumSettings.Create(
            123,
            QuorumSettingsType.Channel,
            456,
            RoleIds(10UL, 20UL, 30UL),
            0.75);

        string json = JsonSerializer.Serialize(original);

        QuorumSettings deserialized = JsonSerializer.Deserialize<QuorumSettings>(json) ??
                                    throw new InvalidOperationException(
                                        "Expected JSON payload to deserialize into QuorumSettings.");

        deserialized.GuildId.ShouldBe(original.GuildId);
        deserialized.TargetType.ShouldBe(original.TargetType);
        deserialized.TargetId.ShouldBe(original.TargetId);
        deserialized.RoleIds.ShouldBe(original.RoleIds);
        deserialized.QuorumProportion.ShouldBe(original.QuorumProportion);
    }
}