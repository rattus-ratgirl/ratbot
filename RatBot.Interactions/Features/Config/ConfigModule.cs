using JetBrains.Annotations;

namespace RatBot.Interactions.Features.Config;

/// <summary>
/// Defines shared configuration command groups.
/// </summary>
[Group("config", "Configuration commands.")]
[DefaultMemberPermissions(GuildPermission.Administrator)]
[UsedImplicitly]
public sealed partial class ConfigModule : SlashCommandBase;