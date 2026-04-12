namespace RatBot.Application.Features.Quorum;

public sealed record QuorumSettingsUpsertResult(bool Created, QuorumSettings Config);