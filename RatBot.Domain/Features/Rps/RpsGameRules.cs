namespace RatBot.Domain.Features.Rps;

public static class RpsGameRules
{
    public static RpsGameOutcome DetermineOutcome(RpsPick challengerPick, RpsPick opponentPick)
    {
        if (challengerPick == opponentPick)
            return RpsGameOutcome.Tie;

        bool challengerWon = challengerPick == RpsPick.Rock && opponentPick == RpsPick.Scissors
                             || challengerPick == RpsPick.Paper && opponentPick == RpsPick.Rock
                             || challengerPick == RpsPick.Scissors && opponentPick == RpsPick.Paper;

        return challengerWon ? RpsGameOutcome.ChallengerWon : RpsGameOutcome.OpponentWon;
    }
}
