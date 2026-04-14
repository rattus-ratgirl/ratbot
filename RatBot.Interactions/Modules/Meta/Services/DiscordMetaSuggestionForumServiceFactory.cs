using RatBot.Application.Common.Discord;

namespace RatBot.Interactions.Modules.Meta.Services;

public delegate IDiscordMetaSuggestionForumService DiscordMetaSuggestionForumServiceFactory(IGuild guild);
