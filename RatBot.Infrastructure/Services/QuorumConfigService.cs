using RatBot.Domain.Entities;
using RatBot.Domain.Enums;
using RatBot.Infrastructure.Data;

namespace RatBot.Infrastructure.Services;

/// <summary>
/// Provides CRUD operations for quorum scope configurations.
/// </summary>
public sealed class QuorumConfigService
{
    private readonly BotDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="QuorumConfigService"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    public QuorumConfigService(BotDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Gets a specific quorum configuration for a guild scope.
    /// </summary>
    /// <param name="guildId">The guild identifier.</param>
    /// <param name="scopeType">The scope type.</param>
    /// <param name="scopeId">The scope identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The configuration when found; otherwise <see langword="null"/>.</returns>
    public Task<QuorumScopeConfig?> GetAsync(
        ulong guildId,
        QuorumScopeType scopeType,
        ulong scopeId,
        CancellationToken cancellationToken = default
    )
    {
        return _dbContext.QuorumScopeConfigs.FirstOrDefaultAsync(
            x => x.GuildId == guildId && x.ScopeType == scopeType && x.ScopeId == scopeId,
            cancellationToken
        );
    }

    /// <summary>
    /// Lists all quorum configurations for a guild.
    /// </summary>
    /// <param name="guildId">The guild identifier.</param>
    /// <returns>The ordered list of configurations.</returns>
    public Task<List<QuorumScopeConfig>> ListAsync(ulong guildId)
    {
        return _dbContext
            .QuorumScopeConfigs.Where(x => x.GuildId == guildId)
            .OrderBy(x => x.ScopeType)
            .ThenBy(x => x.ScopeId)
            .ToListAsync();
    }

    /// <summary>
    /// Creates a quorum configuration if it does not already exist.
    /// </summary>
    /// <param name="guildId">The guild identifier.</param>
    /// <param name="scopeType">The scope type.</param>
    /// <param name="scopeId">The scope identifier.</param>
    /// <param name="roleId">The role identifier.</param>
    /// <param name="proportion">The quorum proportion.</param>
    /// <returns>A task that completes when changes are persisted.</returns>
    public async Task CreateAsync(
        ulong guildId,
        QuorumScopeType scopeType,
        ulong scopeId,
        ulong roleId,
        double proportion
    )
    {
        QuorumScopeConfig? existing = await GetAsync(guildId, scopeType, scopeId);

        if (existing is not null)
            return;

        _dbContext.QuorumScopeConfigs.Add(
            QuorumScopeConfig.Create(guildId, scopeType, scopeId, roleId, proportion)
        );

        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Updates an existing quorum configuration.
    /// </summary>
    /// <param name="guildId">The guild identifier.</param>
    /// <param name="scopeType">The scope type.</param>
    /// <param name="scopeId">The scope identifier.</param>
    /// <param name="roleId">The role identifier.</param>
    /// <param name="proportion">The quorum proportion.</param>
    /// <returns><see langword="true"/> when an existing record was updated; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> UpdateAsync(
        ulong guildId,
        QuorumScopeType scopeType,
        ulong scopeId,
        ulong roleId,
        double proportion
    )
    {
        QuorumScopeConfig? existing = await GetAsync(guildId, scopeType, scopeId);
        if (existing is null)
            return false;

        existing.Reconfigure(roleId, proportion);

        await _dbContext.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Deletes a quorum configuration.
    /// </summary>
    /// <param name="guildId">The guild identifier.</param>
    /// <param name="scopeType">The scope type.</param>
    /// <param name="scopeId">The scope identifier.</param>
    /// <returns><see langword="true"/> when a record was deleted; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> DeleteAsync(ulong guildId, QuorumScopeType scopeType, ulong scopeId)
    {
        QuorumScopeConfig? existing = await GetAsync(guildId, scopeType, scopeId);
        if (existing is null)
            return false;

        _dbContext.QuorumScopeConfigs.Remove(existing);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}
