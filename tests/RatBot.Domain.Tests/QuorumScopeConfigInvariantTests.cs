using RatBot.Domain.Entities;
using RatBot.Domain.Enums;
using Shouldly;

namespace RatBot.Domain.Tests;

/// <summary>
/// Tests for the QuorumScopeConfig domain model invariants.
/// </summary>
public sealed class QuorumScopeConfigInvariantTests
{
    [Fact]
    public void Create_WithValidInputs_SetsExpectedProperties()
    {
        QuorumScopeConfig config = QuorumScopeConfig.Create(
            guildId: 123,
            scopeType: QuorumScopeType.Channel,
            scopeId: 456,
            roleIds: [10UL, 20UL, 10UL, 30UL],
            quorumProportion: 0.75
        );

        config.GuildId.ShouldBe(123UL);
        config.ScopeType.ShouldBe(QuorumScopeType.Channel);
        config.ScopeId.ShouldBe(456UL);
        config.RoleIds.ShouldBe([10UL, 20UL, 30UL]);
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

        config.RoleIds.ShouldBe([99UL]);
        config.QuorumProportion.ShouldBe(0.6);
    }

    [Fact]
    public void Create_WithZeroGuildId_ThrowsArgumentOutOfRange()
    {
        ArgumentOutOfRangeException ex = Should.Throw<ArgumentOutOfRangeException>(() =>
            QuorumScopeConfig.Create(0, QuorumScopeType.Channel, 1, [1UL], 0.5)
        );

        ex.ParamName.ShouldBe("guildId");
    }

    [Fact]
    public void Create_WithZeroScopeId_ThrowsArgumentOutOfRange()
    {
        ArgumentOutOfRangeException ex = Should.Throw<ArgumentOutOfRangeException>(() =>
            QuorumScopeConfig.Create(1, QuorumScopeType.Channel, 0, [1UL], 0.5)
        );

        ex.ParamName.ShouldBe("scopeId");
    }

    [Fact]
    public void Create_WithInvalidScopeType_ThrowsArgumentOutOfRange()
    {
        ArgumentOutOfRangeException ex = Should.Throw<ArgumentOutOfRangeException>(() =>
            QuorumScopeConfig.Create(1, (QuorumScopeType)999, 2, [1UL], 0.5)
        );

        ex.ParamName.ShouldBe("scopeType");
    }

    [Fact]
    public void Create_WithNullRoleIds_ThrowsArgumentNullException()
    {
        IEnumerable<ulong>? roleIds = null;

        ArgumentNullException ex = Should.Throw<ArgumentNullException>(() => QuorumScopeConfig.Create(1, QuorumScopeType.Channel, 2, roleIds!, 0.5));

        ex.ParamName.ShouldBe("roleIds");
    }

    [Fact]
    public void Create_WithEmptyRoleIds_ThrowsArgumentOutOfRange()
    {
        ArgumentOutOfRangeException ex = Should.Throw<ArgumentOutOfRangeException>(() =>
            QuorumScopeConfig.Create(1, QuorumScopeType.Channel, 2, [], 0.5)
        );

        ex.ParamName.ShouldBe("roleIds");
    }

    [Fact]
    public void Create_WithZeroRoleId_ThrowsArgumentOutOfRange()
    {
        ArgumentOutOfRangeException ex = Should.Throw<ArgumentOutOfRangeException>(() =>
            QuorumScopeConfig.Create(1, QuorumScopeType.Channel, 2, [5UL, 0UL], 0.5)
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
            QuorumScopeConfig.Create(1, QuorumScopeType.Channel, 2, [10UL], quorumProportion)
        );

        ex.ParamName.ShouldBe("value");
    }

    [Fact]
    public void Create_WithNaNProportion_ThrowsArgumentOutOfRange()
    {
        ArgumentOutOfRangeException ex = Should.Throw<ArgumentOutOfRangeException>(() =>
            QuorumScopeConfig.Create(1, QuorumScopeType.Channel, 2, [10UL], double.NaN)
        );

        ex.ParamName.ShouldBe("value");
    }

    [Theory]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Create_WithInfiniteProportion_ThrowsArgumentOutOfRange(double quorumProportion)
    {
        ArgumentOutOfRangeException ex = Should.Throw<ArgumentOutOfRangeException>(() =>
            QuorumScopeConfig.Create(1, QuorumScopeType.Channel, 2, [10UL], quorumProportion)
        );

        ex.ParamName.ShouldBe("value");
    }

    [Fact]
    public void Reconfigure_WithValidInputs_UpdatesRolesAndProportion()
    {
        QuorumScopeConfig config = QuorumScopeConfig.Create(1, QuorumScopeType.Channel, 2, [11UL], 0.5);

        config.Reconfigure([3UL, 2UL, 2UL, 1UL], 0.9);

        config.RoleIds.ShouldBe([3UL, 2UL, 1UL]);
        config.QuorumProportion.ShouldBe(0.9);
    }

    [Fact]
    public void Reconfigure_WithSingleRoleOverload_UpdatesRolesAndProportion()
    {
        QuorumScopeConfig config = QuorumScopeConfig.Create(1, QuorumScopeType.Channel, 2, [11UL], 0.5);

        config.Reconfigure(42UL, 1.0);

        config.RoleIds.ShouldBe([42UL]);
        config.QuorumProportion.ShouldBe(1.0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1.01)]
    public void Reconfigure_WithOutOfRangeProportion_ThrowsArgumentOutOfRange(double quorumProportion)
    {
        QuorumScopeConfig config = QuorumScopeConfig.Create(1, QuorumScopeType.Channel, 2, [10UL], 0.5);

        ArgumentOutOfRangeException ex = Should.Throw<ArgumentOutOfRangeException>(() => config.Reconfigure([10UL], quorumProportion));

        ex.ParamName.ShouldBe("value");
    }

    [Fact]
    public void Reconfigure_WithEmptyRoles_ThrowsArgumentOutOfRange()
    {
        QuorumScopeConfig config = QuorumScopeConfig.Create(1, QuorumScopeType.Channel, 2, [10UL], 0.5);

        ArgumentOutOfRangeException ex = Should.Throw<ArgumentOutOfRangeException>(() => config.Reconfigure([], 0.5));

        ex.ParamName.ShouldBe("roleIds");
    }
}
