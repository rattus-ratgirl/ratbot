using LanguageExt;
using RatBot.Domain.Enums;
using RatBot.Domain.Features.Quorum;
using Shouldly;

namespace RatBot.Domain.Tests;

/// <summary>
/// Tests for the QuorumScopeConfig domain model invariants.
/// </summary>
public sealed class QuorumScopeConfigInvariantTests
{
    private static Arr<ulong> RoleIds(params ulong[] roleIds) => new Arr<ulong>(roleIds);

    [Fact]
    public void Create_WithValidInputs_SetsExpectedProperties()
    {
        QuorumScopeConfig config = QuorumScopeConfig.Create(
            guildId: 123,
            scopeType: QuorumScopeType.Channel,
            scopeId: 456,
            roleIds: RoleIds(10UL, 20UL, 10UL, 30UL),
            quorumProportion: 0.75
        );

        config.GuildId.ShouldBe(123UL);
        config.ScopeType.ShouldBe(QuorumScopeType.Channel);
        config.ScopeId.ShouldBe(456UL);
        config.RoleIds.ShouldBe(RoleIds(10UL, 20UL, 30UL));
        config.QuorumProportion.ShouldBe(0.75);
    }

    [Fact]
    public void Create_SingleRoleOverload_SetsRoleAndProportion()
    {
        QuorumScopeConfig config = QuorumScopeConfig.Create(
            guildId: 123,
            scopeType: QuorumScopeType.Category,
            scopeId: 456,
            roleId: 99,
            quorumProportion: 0.6
        );

        config.RoleIds.ShouldBe(RoleIds(99UL));
        config.QuorumProportion.ShouldBe(0.6);
    }

    [Fact]
    public void Create_WithZeroGuildId_ThrowsArgumentOutOfRange()
    {
        ArgumentOutOfRangeException ex = Should.Throw<ArgumentOutOfRangeException>(() =>
            QuorumScopeConfig.Create(0, QuorumScopeType.Channel, 1, RoleIds(1UL), 0.5)
        );

        ex.ParamName.ShouldBe("guildId");
    }

    [Fact]
    public void Create_WithZeroScopeId_ThrowsArgumentOutOfRange()
    {
        ArgumentOutOfRangeException ex = Should.Throw<ArgumentOutOfRangeException>(() =>
            QuorumScopeConfig.Create(1, QuorumScopeType.Channel, 0, RoleIds(1UL), 0.5)
        );

        ex.ParamName.ShouldBe("scopeId");
    }

    [Fact]
    public void Create_WithInvalidScopeType_ThrowsArgumentOutOfRange()
    {
        ArgumentOutOfRangeException ex = Should.Throw<ArgumentOutOfRangeException>(() =>
            QuorumScopeConfig.Create(1, (QuorumScopeType)999, 2, RoleIds(1UL), 0.5)
        );

        ex.ParamName.ShouldBe("scopeType");
    }

    [Fact]
    public void Create_WithDefaultRoleIds_ThrowsArgumentOutOfRange()
    {
        Arr<ulong> roleIds = default;

        ArgumentOutOfRangeException ex = Should.Throw<ArgumentOutOfRangeException>(() => QuorumScopeConfig.Create(1, QuorumScopeType.Channel, 2, roleIds, 0.5));

        ex.ParamName.ShouldBe("roleIds");
    }

    [Fact]
    public void Create_WithEmptyRoleIds_ThrowsArgumentOutOfRange()
    {
        ArgumentOutOfRangeException ex = Should.Throw<ArgumentOutOfRangeException>(() =>
            QuorumScopeConfig.Create(1, QuorumScopeType.Channel, 2, RoleIds(), 0.5)
        );

        ex.ParamName.ShouldBe("roleIds");
    }

    [Fact]
    public void Create_WithZeroRoleId_ThrowsArgumentOutOfRange()
    {
        ArgumentOutOfRangeException ex = Should.Throw<ArgumentOutOfRangeException>(() =>
            QuorumScopeConfig.Create(1, QuorumScopeType.Channel, 2, RoleIds(5UL, 0UL), 0.5)
        );

        ex.ParamName.ShouldBe("roleIds");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.0001)]
    [InlineData(1.0001)]
    public void Create_WithOutOfRangeProportion_ThrowsArgumentOutOfRange(double quorumProportion)
    {
        ArgumentOutOfRangeException ex = Should.Throw<ArgumentOutOfRangeException>(() =>
            QuorumScopeConfig.Create(1, QuorumScopeType.Channel, 2, RoleIds(10UL), quorumProportion)
        );

        ex.ParamName.ShouldBe("value");
    }

    [Fact]
    public void Create_WithNaNProportion_ThrowsArgumentOutOfRange()
    {
        ArgumentOutOfRangeException ex = Should.Throw<ArgumentOutOfRangeException>(() =>
            QuorumScopeConfig.Create(1, QuorumScopeType.Channel, 2, RoleIds(10UL), double.NaN)
        );

        ex.ParamName.ShouldBe("value");
    }

    [Theory]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Create_WithInfiniteProportion_ThrowsArgumentOutOfRange(double quorumProportion)
    {
        ArgumentOutOfRangeException ex = Should.Throw<ArgumentOutOfRangeException>(() =>
            QuorumScopeConfig.Create(1, QuorumScopeType.Channel, 2, RoleIds(10UL), quorumProportion)
        );

        ex.ParamName.ShouldBe("value");
    }

    [Fact]
    public void Reconfigure_WithValidInputs_UpdatesRolesAndProportion()
    {
        QuorumScopeConfig config = QuorumScopeConfig.Create(1, QuorumScopeType.Channel, 2, RoleIds(11UL), 0.5);

        QuorumScopeConfig reconfigured = config.Reconfigure(RoleIds(3UL, 2UL, 2UL, 1UL), 0.9);

        reconfigured.GuildId.ShouldBe(1UL);
        reconfigured.ScopeType.ShouldBe(QuorumScopeType.Channel);
        reconfigured.ScopeId.ShouldBe(2UL);
        reconfigured.RoleIds.ShouldBe(RoleIds(3UL, 2UL, 1UL));
        reconfigured.QuorumProportion.ShouldBe(0.9);
    }

    [Fact]
    public void Reconfigure_WithSingleRoleOverload_UpdatesRolesAndProportion()
    {
        QuorumScopeConfig config = QuorumScopeConfig.Create(1, QuorumScopeType.Channel, 2, RoleIds(11UL), 0.5);

        QuorumScopeConfig reconfigured = config.Reconfigure(42UL, 1.0);

        reconfigured.GuildId.ShouldBe(1UL);
        reconfigured.ScopeType.ShouldBe(QuorumScopeType.Channel);
        reconfigured.ScopeId.ShouldBe(2UL);
        reconfigured.RoleIds.ShouldBe(RoleIds(42UL));
        reconfigured.QuorumProportion.ShouldBe(1.0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1.01)]
    public void Reconfigure_WithOutOfRangeProportion_ThrowsArgumentOutOfRange(double quorumProportion)
    {
        QuorumScopeConfig config = QuorumScopeConfig.Create(1, QuorumScopeType.Channel, 2, RoleIds(10UL), 0.5);

        ArgumentOutOfRangeException ex = Should.Throw<ArgumentOutOfRangeException>(() => _ = config.Reconfigure(RoleIds(10UL), quorumProportion));

        ex.ParamName.ShouldBe("value");
    }

    [Fact]
    public void Reconfigure_WithEmptyRoles_ThrowsArgumentOutOfRange()
    {
        QuorumScopeConfig config = QuorumScopeConfig.Create(1, QuorumScopeType.Channel, 2, RoleIds(10UL), 0.5);

        ArgumentOutOfRangeException ex = Should.Throw<ArgumentOutOfRangeException>(() => _ = config.Reconfigure(RoleIds(), 0.5));

        ex.ParamName.ShouldBe("roleIds");
    }
}
