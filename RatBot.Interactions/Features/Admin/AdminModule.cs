using Discord;
using Discord.Interactions;
using RatBot.Interactions.Common.Discord;

// ReSharper disable UnusedType.Global

namespace RatBot.Interactions.Features.Admin;

/// <summary>
/// Defines administrative slash commands.
/// </summary>
[Group("admin", "Administrative commands.")]
[DefaultMemberPermissions(GuildPermission.Administrator)]
public sealed partial class AdminModule : SlashCommandBase;
