namespace RatBot.Application.RoleColours;

public sealed record RoleColourRegistrationContext(
    bool SourceRoleExists,
    bool DisplayRoleExists,
    bool SourceRoleHasColour
);
