using LanguageExt;
using RatBot.Domain.Enums;
using RatBot.Infrastructure.Services;

namespace RatBot.Interactions.Features.Quorum;

/// <summary>
/// Provides quorum-specific helpers over the shared config repository.
/// </summary>
public static class QuorumConfigs
{
    /// <summary>
    /// Gets the persisted family key for guild channel/category quorum configs.
    /// </summary>
    private const string GuildScopeFamily = "Quorum:GuildScopeConfig";

    /// <summary>
    /// Gets a specific quorum configuration for a guild scope.
    /// </summary>
    /// <param name="configRepository">The shared config repository.</param>
    /// <param name="guildId">The guild identifier.</param>
    /// <param name="scopeType">The scope type.</param>
    /// <param name="scopeId">The scope identifier.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The matching config when present; otherwise <c>None</c>.</returns>
    private async static Task<Option<QuorumScopeConfig>> GetAsync(
        IConfigRepository configRepository,
        ulong guildId,
        QuorumScopeType scopeType,
        ulong scopeId,
        CancellationToken ct = default)
        => (await configRepository.GetFamilyAsync<QuorumScopeConfig>(GuildScopeFamily, ct))
            .Find(config => Matches(config, guildId, scopeType, scopeId));

    /// <summary>
    /// Gets the effective quorum configuration for a text channel, falling back to its category.
    /// </summary>
    /// <param name="configRepository">The shared config repository.</param>
    /// <param name="guildId">The guild identifier.</param>
    /// <param name="channelId">The channel identifier.</param>
    /// <param name="categoryId">The category identifier when present.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The matching config when present; otherwise <c>None</c>.</returns>
    public async static Task<Option<QuorumScopeConfig>> GetForChannelAsync(
        IConfigRepository configRepository,
        ulong guildId,
        ulong channelId,
        ulong? categoryId,
        CancellationToken ct = default)
    {
        Option<QuorumScopeConfig> config = await GetAsync(
            configRepository,
            guildId,
            QuorumScopeType.Channel,
            channelId,
            ct);

        if (config.IsNone && categoryId is { } resolvedCategoryId)
            config = await GetAsync(
                configRepository,
                guildId,
                QuorumScopeType.Category,
                resolvedCategoryId,
                ct);

        return config;
    }

    /// <summary>
    /// Creates or updates a quorum configuration.
    /// </summary>
    /// <param name="configRepository">The shared config repository.</param>
    /// <param name="config">The config to persist.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns><see langword="true" /> when a new config was created; otherwise <see langword="false" />.</returns>
    public async static Task<bool> SetAsync(IConfigRepository configRepository, QuorumScopeConfig config, CancellationToken ct = default)
    {
        Arr<QuorumScopeConfig> existingConfigs = await configRepository.GetFamilyAsync<QuorumScopeConfig>(GuildScopeFamily, ct);

        bool created = existingConfigs
            .Find(existing => Matches(existing, config.GuildId, config.ScopeType, config.ScopeId))
            .IsNone;

        Arr<QuorumScopeConfig> updatedConfigs = new Arr<QuorumScopeConfig>(
            existingConfigs.Where(existing => !Matches(existing, config.GuildId, config.ScopeType, config.ScopeId))
                .Append(config));

        await configRepository.ReplaceFamilyAsync(GuildScopeFamily, updatedConfigs, ct);
        return created;
    }

    /// <summary>
    /// Deletes a quorum configuration.
    /// </summary>
    /// <param name="configRepository">The shared config repository.</param>
    /// <param name="guildId">The guild identifier.</param>
    /// <param name="scopeType">The scope type.</param>
    /// <param name="scopeId">The scope identifier.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns><see langword="true" /> when a config was removed; otherwise <see langword="false" />.</returns>
    public async static Task<bool> DeleteAsync(
        IConfigRepository configRepository,
        ulong guildId,
        QuorumScopeType scopeType,
        ulong scopeId,
        CancellationToken ct = default)
    {
        Arr<QuorumScopeConfig> existingConfigs = await configRepository.GetFamilyAsync<QuorumScopeConfig>(GuildScopeFamily, ct);
        Arr<QuorumScopeConfig> updatedConfigs = new Arr<QuorumScopeConfig>(
            existingConfigs.Where(config => !Matches(config, guildId, scopeType, scopeId)));

        bool deleted = updatedConfigs.Length != existingConfigs.Length;

        if (!deleted)
            return false;

        await configRepository.ReplaceFamilyAsync(GuildScopeFamily, updatedConfigs, ct);

        return true;
    }

    private static bool Matches(
        QuorumScopeConfig config,
        ulong guildId,
        QuorumScopeType scopeType,
        ulong scopeId)
        => config.GuildId == guildId && config.ScopeType == scopeType && config.ScopeId == scopeId;
}
