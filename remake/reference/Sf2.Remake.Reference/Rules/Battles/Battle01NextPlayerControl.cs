namespace Sf2.Remake.Domain.Battles;

public static class Battle01NextPlayerControl
{
    public static Battle01FirstControlTransition Enter(Battle01InitializedState current, int expectedActor)
    {
        ArgumentNullException.ThrowIfNull(current);
        if (current.FirstRound is { RoundNumber: > 1 })
        {
            if (current.Phase is not (Battle01Phase.RoundGenerated or Battle01Phase.PlayerTurnCompleted or Battle01Phase.EnemyTurnCompleted))
                throw new ArgumentException("Next control requires current generation or completed actor dispatch.", "phase");
            Battle01FirstRound.RequireCurrentPrefix(current);
            Battle01EnemyStandby.RequireThinkingHistory(current);
        }
        else if (current.Phase == Battle01Phase.EnemyTurnCompleted)
        {
            // Only the complete eight-actor prefix can hand control from enemies to Bowie.
            Battle01EnemyStandby.RequireCompletedPrefix(current, 8);
        }
        else if (current.Phase != Battle01Phase.PlayerTurnCompleted || current.TurnCompletion is null ||
            current.FirstRound is not { CurrentTurnOffset: > 0 })
            throw new ArgumentException("Next control requires a committed controlled turn at the current byte offset.", "phase");
        // Same classifier, candidate-only activation supplement and live occupancy; never offset0 re-entry.
        return Battle01FirstControl.EnterCurrentCandidate(current, expectedActor);
    }
}
