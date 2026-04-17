using RatBot.Application.Common.Discord;

namespace RatBot.Interactions.Modules.Meta.Services;

public delegate IMetaSuggestionForumService DiscordMetaSuggestionForumServiceFactory(IGuild guild);
