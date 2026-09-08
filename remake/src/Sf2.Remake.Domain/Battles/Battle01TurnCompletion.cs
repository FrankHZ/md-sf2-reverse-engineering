namespace Sf2.Remake.Domain.Battles;

// Explicit remake policy: the admitted effective stats are already refreshed and unchanged.
// This is not an implementation of the original general UpdateCombatantStats routine.
public sealed class Battle01StayCompletionPolicy
{
    private Battle01StayCompletionPolicy() { }
    public static Battle01StayCompletionPolicy ControlledUnchangedEffectiveStats { get; } = new();
    public string Id => "battle01-controlled-stay-unchanged-effective-stats-v1";
}

public sealed record Battle01FactionCounts(int Allies, int Enemies);
public sealed record Battle01TurnCompletionReceipt(int CompletedActorIndex, Battle01StayCompletionPolicy Policy,
    Battle01FactionCounts BeforeAfterTurn, Battle01FactionCounts AfterAfterTurn, Battle01TurnCompletionReceipt? Previous = null,
    Battle01EnemyStandbyDecision? EnemyStandby = null);

public static class Battle01TurnCompletion
{
    public static Battle01InitializedState CommitStay(Battle01InitializedState current, int actorIndex,
        Battle01StayCompletionPolicy? policy)
    {
        ArgumentNullException.ThrowIfNull(current);
        if (current.Phase != Battle01Phase.PlayerActionChoice || current.FirstControl is not { } control ||
            current.FirstRound is not { } order)
            throw new ArgumentException("STAY requires the current player's uncommitted action choice.", "phase");
        if (control.ActorIndex != actorIndex || order.CurrentCandidate?.CombatantIndex != actorIndex || actorIndex >= 128)
            throw new ArgumentException("STAY must name the current controlled player.", "actor");
        return CompleteControlledStay(current, actorIndex, control.Movement.Range.EffectiveStatsAtEntry, policy);
    }

    internal static Battle01InitializedState CompleteControlledStay(Battle01InitializedState current, int actorIndex,
        Battle01Stats entryStats, Battle01StayCompletionPolicy? policy, Battle01EnemyStandbyDecision? enemyStandby = null)
    {
        // BattleLoop, pinned c834c652: defeated wrapper -> cleanup/count -> after-turn -> cleanup/count -> advance.
        // STAY never constructed a scene/worklist; unsupported deaths must not become empty cleanup.
        RequireDefeatedWrapperReturn(current);
        RequireEmptyKilledCleanup(current, "cleanup.before");
        var before = RequireContinuingFactions(current, "outcome.before");
        NormalizeControlledNoEffectTurn(current, actorIndex, entryStats, policy);
        RequireEmptyKilledCleanup(current, "cleanup.after");
        var after = RequireContinuingFactions(current, "outcome.after");
        return new(current, current.FirstRound!.AdvanceCompletedPlayerTurn(),
            new(actorIndex, policy!, before, after, current.TurnCompletion, enemyStandby));
    }

    private static void RequireDefeatedWrapperReturn(Battle01InitializedState current)
    {
        var bowie = current.Roster.SingleOrDefault(unit => unit.Index == 0);
        var firstEnemy = current.Roster.SingleOrDefault(unit => unit.Index == 128);
        if (bowie is null || firstEnemy is null || bowie.Stats.HpCurrent == 0 || firstEnemy.Stats.HpCurrent == 0)
            throw new ArgumentException("The controlled defeated wrapper must return before script and cleanup tail.", "defeated");
    }

    private static void RequireEmptyKilledCleanup(Battle01InitializedState current, string field)
    {
        if (current.Roster.Any(unit => unit.Stats.HpCurrent == 0))
            throw new ArgumentException("Death worklists and their effects are unsupported by this STAY boundary.", field);
    }

    private static Battle01FactionCounts RequireContinuingFactions(Battle01InitializedState current, string field)
    {
        var living = current.Roster.Where(unit => unit.Position.X is >= 0 and < 128 && unit.Stats.HpCurrent > 0);
        int allies = living.Count(unit => unit.Index < 128), enemies = living.Count(unit => unit.Index >= 128);
        if (current.Roster.Single(unit => unit.Index == 0).Stats.HpCurrent == 0) allies = 0;
        if (allies == 0 || enemies == 0)
            throw new ArgumentException("Victory and defeat must not advance this controlled turn.", field);
        return new(allies, enemies);
    }

    private static void NormalizeControlledNoEffectTurn(Battle01InitializedState current, int actorIndex,
        Battle01Stats entryStats, Battle01StayCompletionPolicy? policy)
    {
        if (!ReferenceEquals(policy, Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats))
            throw new ArgumentException("The unchanged, already-refreshed effective-stat policy must be explicit.", "policy");
        if (!current.Roster.Select(unit => unit.Index).SequenceEqual(new[] { 0, 1, 2, 128, 129, 130, 131, 132, 133 }))
            throw new ArgumentException("The complete initialized nine-unit roster is required.", "roster");
        var occupancy = Enumerable.Repeat(-1, 48 * 48).ToArray();
        foreach (var unit in current.Roster)
        {
            if (unit.Stats.Status != 0)
                throw new ArgumentException("Nonzero status processing is outside controlled no-effect STAY.", "status");
            // sf2enums equipped bit7, IDs HOLY_STAFF61 / MYSTERY_STAFF64 / LIFE_RING7C.
            if (unit.Stats.Items.Any(item => (item & 0x80) != 0 && (item & 0x7F) is 0x61 or 0x64 or 0x7C))
                throw new ArgumentException("Equipped passive recovery is outside controlled no-effect STAY.", "equipment");
            if (!Battle01Initialization.WithinArea(unit.Position))
                throw new ArgumentException("Every controlled combatant must remain placed in the fixed area.", "position");
            int offset = Battle01PlayerMovement.Offset(unit.Position);
            if (occupancy[offset] != -1)
                throw new ArgumentException("Controlled combatants cannot share a live cell.", "occupancy");
            occupancy[offset] = unit.Index;
        }
        if (!occupancy.SequenceEqual(current.Occupancy))
            throw new ArgumentException("The retained live occupancy must match the complete roster.", "occupancy");
        if (!ReferenceEquals(current.Roster.Single(unit => unit.Index == actorIndex).Stats, entryStats))
            throw new ArgumentException("The actor's admitted effective stats must remain unchanged since control entry.", "stats");
        // All current production paths preserve the immutable initialized Stats objects. With no
        // status/item/action changes, retain them; never reconstruct base stats or stack equipment.
    }
}
