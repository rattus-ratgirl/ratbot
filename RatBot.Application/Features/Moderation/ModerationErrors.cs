using RatBot.Domain.Primitives;

namespace RatBot.Application.Features.Moderation;

public static class ModerationErrors
{
    public static Error UserAlreadyAutobanned(UserSnowflake userId) =>
        Error.Conflict(
            "Moderation.UserAlreadyAutobanned",
            $"User {userId} is already registered for autoban.");
}
