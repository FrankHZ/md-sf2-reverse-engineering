namespace Sf2.Remake.Domain.Battles;

public static class Battle01NextPlayerControl
{
    public static Battle01FirstControlTransition Enter(Battle01InitializedState current, int expectedActor)
    {
        ArgumentNullException.ThrowIfNull(current);
        if (current.Phase != Battle01Phase.PlayerTurnCompleted || current.TurnCompletion is null ||
            current.FirstRound is not { CurrentTurnOffset: > 0 })
            throw new ArgumentException("Next control requires a committed player STAY at the current byte offset.", "phase");
        // Same classifier, candidate-only activation supplement and live occupancy; never offset0 re-entry.
        return Battle01FirstControl.EnterCurrentCandidate(current, expectedActor);
    }
}
