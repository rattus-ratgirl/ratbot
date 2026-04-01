using RatBot.Domain.Entities;
using RatBot.Infrastructure.Services;

namespace RatBot.Discord;

public sealed class VirtueModule
{
    private readonly DiscordSocketClient _discordClient;
    private readonly IServiceProvider _services;
    private readonly ILogger _logger;
    private readonly IReadOnlyList<VirtueRoleTier> _fallbackRoleTiers;
    private readonly ulong _fallbackBaselineRoleId;

    private bool _isRegistered;

    public VirtueModule(
        DiscordSocketClient discordClient,
        IServiceProvider services,
        IConfiguration config,
        ILogger logger
    )
    {
        _discordClient = discordClient;
        _services = services;
        _logger = logger.ForContext<VirtueModule>();

        _fallbackRoleTiers = LoadRoleTiers(config).OrderBy(x => x.MinVirtue).Take(6).ToList();
        _fallbackBaselineRoleId = ParseUlong(config["Virtue:BaselineRoleId"]);

        if (_fallbackRoleTiers.Count != 6)
            _logger.Warning(
                "Expected 6 fallback configured virtue role tiers, but loaded {TierCount}.",
                _fallbackRoleTiers.Count
            );
    }

    public void RegisterHandlers()
    {
        if (_isRegistered)
            return;

        _discordClient.ReactionAdded += OnReactionAddedAsync;
        _discordClient.Ready += OnReadyAsync;
        _discordClient.UserJoined += OnUserJoinedAsync;

        _isRegistered = true;
    }

    private async Task OnReadyAsync()
    {
        foreach (SocketGuild guild in _discordClient.Guilds)
        {
            try
            {
                await guild.DownloadUsersAsync();
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Failed to download users for guild {GuildId}.", guild.Id);
            }

            List<SocketGuildUser> users = guild.Users.Where(x => !x.IsBot).ToList();
            await using AsyncServiceScope scope = _services.CreateAsyncScope();
            VirtueRoleTierConfigService configService = scope.ServiceProvider.GetRequiredService<VirtueRoleTierConfigService>();
            UserVirtueService userVirtueService = scope.ServiceProvider.GetRequiredService<UserVirtueService>();
            Dictionary<ulong, int> virtues = await userVirtueService.GetVirtuesAsync(users.Select(x => x.Id));
            GuildRoleAssignmentConfig assignmentConfig = await ResolveRoleAssignmentConfigAsync(guild.Id, configService);

            if (assignmentConfig.RoleTiers.Count == 0 && assignmentConfig.FallbackBaselineRoleId == 0)
            {
                _logger.Warning(
                    "Virtue role configuration is missing for guild {GuildId}. Configure 7 tiers via /virtue configure-role.",
                    guild.Id
                );

                continue;
            }

            foreach (SocketGuildUser user in users)
            {
                try
                {
                    int virtue = virtues.GetValueOrDefault(user.Id);
                    await ApplyRoleAssignmentAsync(user, virtue, assignmentConfig);
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "Failed to assign virtue role for user {UserId}.", user.Id);
                }
            }
        }
    }

    private async Task OnUserJoinedAsync(SocketGuildUser user)
    {
        if (user.IsBot)
            return;

        try
        {
            await using AsyncServiceScope scope = _services.CreateAsyncScope();
            VirtueRoleTierConfigService configService = scope.ServiceProvider.GetRequiredService<VirtueRoleTierConfigService>();
            UserVirtueService userVirtueService = scope.ServiceProvider.GetRequiredService<UserVirtueService>();
            int virtue = await userVirtueService.GetVirtueAsync(user.Id);
            GuildRoleAssignmentConfig assignmentConfig = await ResolveRoleAssignmentConfigAsync(user.Guild.Id, configService);
            await ApplyRoleAssignmentAsync(user, virtue, assignmentConfig);
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Failed to assign baseline/virtue role for new user {UserId}.", user.Id);
        }
    }

    private async Task OnReactionAddedAsync(
        Cacheable<IUserMessage, ulong> cachedMessage,
        Cacheable<IMessageChannel, ulong> cachedChannel,
        SocketReaction reaction
    )
    {
        try
        {
            IMessageChannel? channel = await cachedChannel.GetOrDownloadAsync();
            if (channel is not SocketGuildChannel guildChannel)
                return;

            IUserMessage? message = await cachedMessage.GetOrDownloadAsync();
            if (message is null)
                return;

            string emojiId = ResolveEmojiId(reaction.Emote);

            await using AsyncServiceScope scope = _services.CreateAsyncScope();
            EmojiUsageService emojiUsageService = scope.ServiceProvider.GetRequiredService<EmojiUsageService>();
            await emojiUsageService.IncrementUsageAsync(emojiId);

            if (message.Author.IsBot)
                return;

            EmojiVirtueService emojiVirtueService = scope.ServiceProvider.GetRequiredService<EmojiVirtueService>();
            int? virtueDelta = await emojiVirtueService.GetVirtueAsync(emojiId);
            if (virtueDelta is null)
                return;

            VirtueRoleTierConfigService configService = scope.ServiceProvider.GetRequiredService<VirtueRoleTierConfigService>();
            UserVirtueService userVirtueService = scope.ServiceProvider.GetRequiredService<UserVirtueService>();
            int updatedVirtue = await userVirtueService.AddVirtueDeltaAsync(message.Author.Id, virtueDelta.Value);
            int previousVirtue = updatedVirtue - virtueDelta.Value;

            _logger.Information(
                "Virtue changed for user {UserId} in guild {GuildId}: {PreviousVirtue} -> {UpdatedVirtue} (delta {VirtueDelta}) via emoji {EmojiId}, reactor {ReactorUserId}, message {MessageId}",
                message.Author.Id,
                guildChannel.Guild.Id,
                previousVirtue,
                updatedVirtue,
                virtueDelta.Value,
                emojiId,
                reaction.UserId,
                message.Id
            );

            SocketGuild guild = guildChannel.Guild;
            SocketGuildUser? author = message.Author as SocketGuildUser ?? guild.GetUser(message.Author.Id);

            if (author is null || author.IsBot)
                return;

            GuildRoleAssignmentConfig assignmentConfig = await ResolveRoleAssignmentConfigAsync(guild.Id, configService);
            await ApplyRoleAssignmentAsync(author, updatedVirtue, assignmentConfig);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed processing virtue reaction event.");
        }
    }

    private async Task ApplyRoleAssignmentAsync(SocketGuildUser user, int virtue, GuildRoleAssignmentConfig config)
    {
        VirtueRoleTier? matchedTier = config.RoleTiers.FirstOrDefault(x =>
            x.Contains(virtue) && user.Guild.GetRole(x.RoleId) is not null
        );
        ulong targetRoleId = matchedTier?.RoleId ?? config.FallbackBaselineRoleId;

        if (targetRoleId == 0)
            return;

        List<SocketRole> trackedRoles = config.RoleTiers
            .Select(x => user.Guild.GetRole(x.RoleId))
            .Where(x => x is not null)
            .Cast<SocketRole>()
            .ToList();

        SocketRole? baselineRole = user.Guild.GetRole(config.FallbackBaselineRoleId);
        if (baselineRole is not null && trackedRoles.All(x => x.Id != baselineRole.Id))
            trackedRoles.Add(baselineRole);

        List<IRole> toAdd = new List<IRole>();
        List<IRole> toRemove = new List<IRole>();

        foreach (SocketRole role in trackedRoles)
        {
            bool userHasRole = user.Roles.Any(r => r.Id == role.Id);
            bool shouldHaveRole = role.Id == targetRoleId;

            if (shouldHaveRole && !userHasRole)
                toAdd.Add(role);
            else if (!shouldHaveRole && userHasRole)
                toRemove.Add(role);
        }

        if (toRemove.Count > 0)
            await user.RemoveRolesAsync(toRemove);

        if (toAdd.Count > 0)
        {
            await user.AddRolesAsync(toAdd);

            foreach (IRole role in toAdd)
            {
                _logger.Information(
                    "Granted virtue role {RoleId} ({RoleName}) to user {UserId} in guild {GuildId} at virtue {Virtue}",
                    role.Id,
                    role.Name,
                    user.Id,
                    user.Guild.Id,
                    virtue
                );
            }
        }
    }

    private static List<VirtueRoleTier> LoadRoleTiers(IConfiguration config)
    {
        List<VirtueRoleTier> tiers = new List<VirtueRoleTier>();

        foreach (IConfigurationSection child in config.GetSection("Virtue:RoleTiers").GetChildren())
        {
            ulong roleId = ParseUlong(child["RoleId"]);
            if (roleId == 0)
                continue;

            if (
                !int.TryParse(child["MinVirtue"], out int minVirtue)
                || !int.TryParse(child["MaxVirtue"], out int maxVirtue)
            )
                continue;

            tiers.Add(new VirtueRoleTier(roleId, minVirtue, maxVirtue));
        }

        return tiers;
    }

    private async Task<GuildRoleAssignmentConfig> ResolveRoleAssignmentConfigAsync(
        ulong guildId,
        VirtueRoleTierConfigService configService
    )
    {
        List<VirtueRoleTierConfig> persisted = await configService.ListAsync(guildId);

        if (persisted.Count == 7)
        {
            IReadOnlyList<VirtueRoleTier> tiers = persisted
                .OrderBy(x => x.TierIndex)
                .Select(x => new VirtueRoleTier(x.RoleId, x.MinVirtue, x.MaxVirtue))
                .ToList();

            return new GuildRoleAssignmentConfig(tiers, 0);
        }

        return new GuildRoleAssignmentConfig(_fallbackRoleTiers, _fallbackBaselineRoleId);
    }

    private static ulong ParseUlong(string? value)
    {
        return ulong.TryParse(value, out ulong parsed) ? parsed : 0;
    }

    private static string ResolveEmojiId(IEmote emote)
    {
        return emote switch
        {
            Emote customEmoji => customEmoji.Id.ToString(),
            Emoji unicodeEmoji => unicodeEmoji.Name,
            _ => emote.Name,
        };
    }

    private sealed record VirtueRoleTier(ulong RoleId, int MinVirtue, int MaxVirtue)
    {
        public bool Contains(int value) => value >= MinVirtue && value <= MaxVirtue;
    }

    private sealed record GuildRoleAssignmentConfig(IReadOnlyList<VirtueRoleTier> RoleTiers, ulong FallbackBaselineRoleId);
}
