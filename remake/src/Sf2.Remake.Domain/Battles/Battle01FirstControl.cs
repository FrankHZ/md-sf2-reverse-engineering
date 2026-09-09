namespace Sf2.Remake.Domain.Battles;

public sealed class Battle01FirstControlPreset
{
    private Battle01FirstControlPreset() { }
    public static Battle01FirstControlPreset ControlledPlayer { get; } = new();
    public string Id => "private-local-battle01-first-player-control-inputs-v1";
    public bool AllyAutoBattle => false;
    public bool OpponentControl => false;
    // Explicit caller policy, only for the current ally whose word was not supplied earlier.
    public ushort MissingCandidateAllyActivationWord => 0;
}

public enum Battle01FirstControlAvailability
{
    Player, Sentinel, MissingCombatant, Dead, Unplaced, MissingActivationWord,
    MuddledAi, AiControlled, AllyAutoBattle, OpponentAi, Sleeping, Stunned, UnsupportedMovementProfile,
}

public sealed record Battle01FirstControlDecision(int? ActorIndex, Battle01FirstControlAvailability Availability);
public sealed record Battle01FirstControlTransition(Battle01FirstControlDecision Decision, Battle01InitializedState? State);

public sealed class Battle01FirstControlState
{
    internal Battle01FirstControlState(Battle01FirstControlPreset preset, bool candidateWordSupplied,
        Battle01PlayerMovementSelection movement)
    {
        Preset = preset; CandidateWordSupplied = candidateWordSupplied; Movement = movement;
    }
    public Battle01FirstControlPreset Preset { get; }
    public bool CandidateWordSupplied { get; }
    public int ActorIndex => Movement.Range.ActorIndex;
    public Battle01PlayerMovementSelection Movement { get; }
    internal Battle01FirstControlState WithMovement(Battle01PlayerMovementSelection movement) =>
        new(Preset, CandidateWordSupplied, movement);
}

public static class Battle01FirstControl
{
    public static Battle01FirstControlTransition Enter(Battle01InitializedState current, int expectedActor)
    {
        ArgumentNullException.ThrowIfNull(current);
        if (current.Phase != Battle01Phase.FirstRoundGenerated || current.FirstRound is not { RoundNumber: 1, CurrentTurnOffset: 0 })
            throw new ArgumentException("First control requires the generated round before player entry.", "phase");
        return EnterCurrentCandidate(current, expectedActor);
    }

    internal static Battle01FirstControlTransition EnterCurrentCandidate(Battle01InitializedState current, int expectedActor)
    {
        var candidate = current.FirstRound!.CurrentCandidate;
        if (candidate is null) return Unavailable(null, Battle01FirstControlAvailability.Sentinel);
        int index = candidate.Value.CombatantIndex;
        if (expectedActor != index) throw new ArgumentException("The request must name the current candidate.", "actor");
        var actor = current.Roster.SingleOrDefault(unit => unit.Index == index);
        if (actor is null) return Unavailable(index, Battle01FirstControlAvailability.MissingCombatant);
        var preset = Battle01FirstControlPreset.ControlledPlayer;
        bool supplied = actor.Index < 128 && actor.AiBitfield is null;
        ushort? activationWord = supplied ? preset.MissingCandidateAllyActivationWord : actor.AiBitfield;
        var availability = Classify(actor.Index, actor.Stats.HpCurrent, actor.Position?.X ?? 255, actor.Position?.Y ?? 255,
            actor.Stats.Status, activationWord, preset.AllyAutoBattle, preset.OpponentControl);
        if (availability != Battle01FirstControlAvailability.Player) return Unavailable(index, availability);
        if (Battle01MovementProfile.ForClass(actor.ClassId) is null)
            return Unavailable(index, Battle01FirstControlAvailability.UnsupportedMovementProfile);
        var range = Battle01PlayerMovement.CreateRange(current, actor);
        var movement = new Battle01PlayerMovementSelection(range,
            Battle01PlayerMovement.CreatePreview(range, actor.RequirePosition()), Battle01PlayerMovementStage.Selection);
        var roster = current.Roster.ToArray();
        if (supplied) roster[Array.IndexOf(roster, actor)] = actor.WithAiBitfield(activationWord!.Value);
        var control = new Battle01FirstControlState(preset, supplied, movement);
        var next = new Battle01InitializedState(current, roster, current.Occupancy, control);
        return new(new(index, Battle01FirstControlAvailability.Player), next);
    }

    internal static Battle01FirstControlAvailability Classify(int index, ushort hp, int x, int y,
        ushort status, ushort? activationWord, bool allyAutoBattle, bool opponentControl)
    {
        if (hp == 0) return Battle01FirstControlAvailability.Dead;
        if (x is < 0 or >= 48 || y is < 0 or >= 48) return Battle01FirstControlAvailability.Unplaced;
        if ((status & 0x30) != 0) return Battle01FirstControlAvailability.MuddledAi;
        if (activationWord is null) return Battle01FirstControlAvailability.MissingActivationWord;
        if ((activationWord.Value & 4) != 0) return Battle01FirstControlAvailability.AiControlled;
        if (index < 128 && allyAutoBattle) return Battle01FirstControlAvailability.AllyAutoBattle;
        if (index >= 128 && !opponentControl) return Battle01FirstControlAvailability.OpponentAi;
        // The original checks these before player input. This slice does not consume a skipped turn.
        if ((status & 0xC0) != 0) return Battle01FirstControlAvailability.Sleeping;
        if ((status & 1) != 0) return Battle01FirstControlAvailability.Stunned;
        return Battle01FirstControlAvailability.Player;
    }

    private static Battle01FirstControlTransition Unavailable(int? actor, Battle01FirstControlAvailability reason) =>
        new(new(actor, reason), null);
}
