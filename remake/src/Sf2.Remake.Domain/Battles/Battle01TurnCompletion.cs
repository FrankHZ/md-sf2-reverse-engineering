namespace Sf2.Remake.Domain.Battles;

// Explicit remake policy: the admitted effective stats are already refreshed and unchanged.
// This is not an implementation of the original general UpdateCombatantStats routine.
public abstract class Battle01TurnCompletionPolicy
{
    private protected Battle01TurnCompletionPolicy() { }
    public abstract string Id { get; }
}

public sealed class Battle01StayCompletionPolicy : Battle01TurnCompletionPolicy
{
    private Battle01StayCompletionPolicy() { }
    public static Battle01StayCompletionPolicy ControlledUnchangedEffectiveStats { get; } = new();
    public override string Id => "battle01-controlled-stay-unchanged-effective-stats-v1";
}

public sealed class Battle01PhysicalCompletionPolicy : Battle01TurnCompletionPolicy
{
    private Battle01PhysicalCompletionPolicy() { }
    public static Battle01PhysicalCompletionPolicy ControlledNonlethalStrike { get; } = new();
    public override string Id => "battle01-controlled-nonlethal-physical-strike-v1";
}

public sealed record Battle01FactionCounts(int Allies, int Enemies);
public sealed record Battle01EnemyDefeatCleanup(IReadOnlyList<int> FirstWorklist,
    IReadOnlyList<int> AfterTurnWorklist, int CreditedAlly, ushort KillsBefore, ushort KillsAfter);
public sealed class Battle01PlayerPhysicalCompletionPolicy : Battle01TurnCompletionPolicy
{
    private Battle01PlayerPhysicalCompletionPolicy(bool allowDefeat) { AllowsDefeat = allowDefeat; }
    public static Battle01PlayerPhysicalCompletionPolicy ControlledNonlethalStrikeAndExp { get; } = new(false);
    public static Battle01PlayerPhysicalCompletionPolicy ControlledStrikeAndFirstDefeat { get; } = new(true);
    public bool AllowsDefeat { get; }
    public override string Id => AllowsDefeat ? "battle01-controlled-player-first-defeat-exp-gold-kills-v1" :
        "battle01-controlled-player-nonlethal-physical-exp-v1";
    internal static bool IsSupported(Battle01PlayerPhysicalCompletionPolicy? policy) =>
        ReferenceEquals(policy, ControlledNonlethalStrikeAndExp) || ReferenceEquals(policy, ControlledStrikeAndFirstDefeat);
}

public sealed record Battle01TurnCompletionReceipt(int CompletedActorIndex, Battle01TurnCompletionPolicy Policy,
    Battle01FactionCounts BeforeAfterTurn, Battle01FactionCounts AfterAfterTurn, Battle01TurnCompletionReceipt? Previous = null,
    Battle01EnemyStandbyDecision? EnemyStandby = null, int RoundNumber = 1,
    Battle01EnemyPursuitDecision? EnemyPursuit = null, Battle01EnemyPhysicalAttackDecision? EnemyPhysicalAttack = null,
    Battle01PlayerPhysicalAttackDecision? PlayerPhysicalAttack = null, Battle01EnemyDefeatCleanup? EnemyDefeat = null);

public static class Battle01TurnCompletion
{
    internal static bool HasValidPolicy(Battle01TurnCompletionReceipt receipt) =>
        receipt.PlayerPhysicalAttack is not null
            ? Battle01PlayerPhysicalCompletionPolicy.IsSupported(receipt.Policy as Battle01PlayerPhysicalCompletionPolicy) &&
                (!receipt.PlayerPhysicalAttack.DefeatedTarget || ((Battle01PlayerPhysicalCompletionPolicy)receipt.Policy).AllowsDefeat) &&
                receipt.PlayerPhysicalAttack.DefeatedTarget == (receipt.EnemyDefeat is not null) &&
                receipt.CompletedActorIndex < 128 && receipt.PlayerPhysicalAttack.ActorIndex == receipt.CompletedActorIndex &&
                receipt.EnemyStandby is null && receipt.EnemyPursuit is null && receipt.EnemyPhysicalAttack is null
            : receipt.EnemyDefeat is null && (receipt.EnemyPhysicalAttack is not null
            ? ReferenceEquals(receipt.Policy, Battle01PhysicalCompletionPolicy.ControlledNonlethalStrike) &&
                receipt.CompletedActorIndex >= 128 && receipt.EnemyStandby is null && receipt.EnemyPursuit is null
            : ReferenceEquals(receipt.Policy, Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats));

    internal static Battle01InitializedState CompletePlayerPhysical(Battle01InitializedState current,
        Battle01PlayerPhysicalAttackDecision decision, Battle01PlayerPhysicalCompletionPolicy? policy)
    {
        if (!Battle01PlayerPhysicalCompletionPolicy.IsSupported(policy) || (decision.DefeatedTarget && !policy!.AllowsDefeat))
            throw new ArgumentException("An explicit player physical/EXP completion policy is required.", "policy");
        if (current.Phase != Battle01Phase.PlayerAttackTargetSelection || current.FirstControl?.ActorIndex != decision.ActorIndex ||
            current.FirstRound?.CurrentCandidate?.CombatantIndex != decision.ActorIndex)
            throw new ArgumentException("Retain the actual selected player turn during local finalization.", "phase");
        Battle01PlayerPhysicalAttack.ValidateDecision(decision);
        if (current.CurrentGold != decision.GoldAfter)
            throw new ArgumentException("Retain the constructed gold value before replay and cleanup.", "attack.history");
        RequireDefeatedWrapperReturn(current);
        Battle01EnemyDefeatCleanup? cleanup = null;
        if (decision.DefeatedTarget)
        {
            (current, cleanup) = ApplyFirstDefeatCleanup(current, decision);
        }
        else RequireEmptyKilledCleanup(current, "cleanup.before");
        var before = RequireContinuingFactions(current, "outcome.before");
        var actorAfterCleanup = cleanup is null ? decision.ActorAfterStats :
            current.Roster.Single(unit => unit.Index == decision.ActorIndex).Stats;
        NormalizeControlledNoEffectTurn(current, decision.ActorIndex, actorAfterCleanup,
            Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
        // ProcessAfterTurnEffects clears the list length before this no-effect actor's refresh.
        RequireEmptyKilledCleanup(current, "cleanup.after", cleanup is null ? null : decision.TargetIndex);
        var after = RequireContinuingFactions(current, "outcome.after");
        var result = new Battle01InitializedState(current, current.FirstRound!.AdvanceCompletedPlayerTurn(),
            new(decision.ActorIndex, policy!, before, after, current.TurnCompletion,
                RoundNumber: current.FirstRound.RoundNumber, PlayerPhysicalAttack: decision, EnemyDefeat: cleanup));
        Battle01FirstRound.RequireCurrentPrefix(result);
        Battle01EnemyStandby.RequireThinkingHistory(result);
        return result;
    }

    private static (Battle01InitializedState State, Battle01EnemyDefeatCleanup Cleanup) ApplyFirstDefeatCleanup(
        Battle01InitializedState current, Battle01PlayerPhysicalAttackDecision decision)
    {
        var dead = current.Roster.Where(unit => unit.Stats.HpCurrent == 0).ToArray();
        if (dead.Length != 1 || dead[0].Index != decision.TargetIndex || decision.TargetIndex < 128 ||
            dead[0].Position is not { } position || position != decision.Target.Position ||
            !Battle01EnemyPhysicalAttack.SameStats(dead[0].Stats, decision.Effect.AfterStats) ||
            decision.Actor.Stats.CurrentKills is not { } kills)
            throw new ArgumentException("The first death worklist must contain exactly the reacted enemy.", "cleanup.before");
        var roster = current.Roster.ToArray(); var occupancy = current.Occupancy.ToArray();
        int cell = Battle01PlayerMovement.Offset(position);
        if (occupancy[cell] != decision.TargetIndex)
            throw new ArgumentException("Retain the defeated enemy's occupied cell before cleanup.", "occupancy");
        int actorSlot = Array.FindIndex(roster, unit => unit.Index == decision.ActorIndex);
        if (!Battle01EnemyPhysicalAttack.SameStats(roster[actorSlot].Stats, decision.ActorAfterStats))
            throw new ArgumentException("EXP replay must precede kill credit.", "attack.history");
        ushort afterKills = Battle01PlayerPhysicalAttack.KillsAfterKill(kills);
        roster[actorSlot] = roster[actorSlot].WithStats(roster[actorSlot].Stats.WithCurrentKills(afterKills));
        // Fixed regular GIZMO has no modifiers/status/equipment to alter on its source stat refresh.
        roster[Array.IndexOf(roster, dead[0])] = dead[0].WithPosition(null);
        occupancy[cell] = -1;
        var state = new Battle01InitializedState(current, roster, Array.AsReadOnly(occupancy), current.FirstControl!);
        return (state, new(Array.AsReadOnly(new[] { decision.TargetIndex }), Array.Empty<int>(),
            decision.ActorIndex, kills, afterKills));
    }

    internal static void ValidateDefeatReceipt(Battle01TurnCompletionReceipt receipt)
    {
        if (receipt.PlayerPhysicalAttack is not { DefeatedTarget: true } decision || receipt.EnemyDefeat is not { } cleanup ||
            !HasValidPolicy(receipt) || !cleanup.FirstWorklist.SequenceEqual(new[] { decision.TargetIndex }) ||
            cleanup.AfterTurnWorklist.Count != 0 || cleanup.CreditedAlly != decision.ActorIndex ||
            decision.Actor.Stats.CurrentKills != cleanup.KillsBefore ||
            cleanup.KillsAfter != Battle01PlayerPhysicalAttack.KillsAfterKill(cleanup.KillsBefore))
            throw new ArgumentException("Retain the first-ally kill credit and both ordered worklists.", "attack.history");
    }

    internal static Battle01InitializedState CompletePhysical(Battle01InitializedState current,
        Battle01EnemyPhysicalAttackDecision decision, Battle01PhysicalCompletionPolicy? policy)
    {
        if (!ReferenceEquals(policy, Battle01PhysicalCompletionPolicy.ControlledNonlethalStrike))
            throw new ArgumentException("An explicit controlled physical completion policy is required.", "policy");
        Battle01EnemyPhysicalAttack.ValidateDecision(decision);
        RequireDefeatedWrapperReturn(current);
        RequireEmptyKilledCleanup(current, "cleanup.before");
        var before = RequireContinuingFactions(current, "outcome.before");
        // The admitted actor's after-turn refresh changes no modifiers, status, MP or equipment.
        // Its target HP has already been authorized by the physical reaction, not by STAY.
        NormalizeControlledNoEffectTurn(current, decision.ActorIndex, decision.Actor.Stats,
            Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
        RequireEmptyKilledCleanup(current, "cleanup.after");
        var after = RequireContinuingFactions(current, "outcome.after");
        var result = new Battle01InitializedState(current, current.FirstRound!.AdvanceCompletedPlayerTurn(),
            new(decision.ActorIndex, policy!, before, after, current.TurnCompletion,
                RoundNumber: current.FirstRound.RoundNumber, EnemyPhysicalAttack: decision));
        Battle01FirstRound.RequireCurrentPrefix(result);
        Battle01EnemyStandby.RequireThinkingHistory(result);
        return result;
    }

    public static Battle01InitializedState CommitStay(Battle01InitializedState current, int actorIndex,
        Battle01StayCompletionPolicy? policy)
    {
        ArgumentNullException.ThrowIfNull(current);
        if (current.Phase != Battle01Phase.PlayerActionChoice || current.FirstControl is not { } control ||
            current.FirstRound is not { } order)
            throw new ArgumentException("STAY requires the current player's uncommitted action choice.", "phase");
        if (control.ActorIndex != actorIndex || order.CurrentCandidate?.CombatantIndex != actorIndex || actorIndex >= 128)
            throw new ArgumentException("STAY must name the current controlled player.", "actor");
        if (order.RoundNumber > 1)
        {
            Battle01FirstRound.RequireCurrentPrefix(current);
            Battle01EnemyStandby.RequireThinkingHistory(current);
        }
        return CompleteControlledStay(current, actorIndex, control.Movement.Range.EffectiveStatsAtEntry, policy);
    }

    internal static Battle01InitializedState CompleteControlledStay(Battle01InitializedState current, int actorIndex,
        Battle01Stats entryStats, Battle01StayCompletionPolicy? policy, Battle01EnemyStandbyDecision? enemyStandby = null,
        Battle01EnemyPursuitDecision? enemyPursuit = null)
    {
        // BattleLoop, pinned c834c652: defeated wrapper -> cleanup/count -> after-turn -> cleanup/count -> advance.
        // STAY never constructed a scene/worklist; unsupported deaths must not become empty cleanup.
        if (actorIndex < 128 ? enemyStandby is not null || enemyPursuit is not null :
            (enemyStandby is null) == (enemyPursuit is null))
            throw new ArgumentException("Only an enemy completion may carry exactly one enemy decision.", "completion");
        if ((enemyStandby is not null && enemyStandby.ActorIndex != actorIndex) ||
            (enemyPursuit is not null && enemyPursuit.ActorIndex != actorIndex))
            throw new ArgumentException("The completion decision must belong to its actor.", "completion");
        RequireDefeatedWrapperReturn(current);
        RequireEmptyKilledCleanup(current, "cleanup.before");
        var before = RequireContinuingFactions(current, "outcome.before");
        NormalizeControlledNoEffectTurn(current, actorIndex, entryStats, policy);
        RequireEmptyKilledCleanup(current, "cleanup.after");
        var after = RequireContinuingFactions(current, "outcome.after");
        return new(current, current.FirstRound!.AdvanceCompletedPlayerTurn(),
            new(actorIndex, policy!, before, after, current.TurnCompletion, enemyStandby, current.FirstRound.RoundNumber, enemyPursuit));
    }

    internal static void RequireContinuingNoEffectState(Battle01InitializedState current)
    {
        RequireDefeatedWrapperReturn(current);
        RequireEmptyKilledCleanup(current, "cleanup.before");
        RequireContinuingFactions(current, "outcome.before");
        NormalizeControlledNoEffectTurn(current, 0, current.Roster.Single(unit => unit.Index == 0).Stats,
            Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
    }

    private static void RequireDefeatedWrapperReturn(Battle01InitializedState current)
    {
        var bowie = current.Roster.SingleOrDefault(unit => unit.Index == 0);
        var firstEnemy = current.Roster.SingleOrDefault(unit => unit.Index == 128);
        if (bowie is null || firstEnemy is null || bowie.Stats.HpCurrent == 0 || firstEnemy.Stats.HpCurrent == 0)
            throw new ArgumentException("The controlled defeated wrapper must return before script and cleanup tail.", "defeated");
    }

    private static void RequireEmptyKilledCleanup(Battle01InitializedState current, string field, int? pendingDefeat = null)
    {
        foreach (var unit in current.Roster.Where(unit => unit.Stats.HpCurrent == 0))
        {
            if (unit.Position is not null || unit.Index < 128)
                throw new ArgumentException("Unprocessed deaths cannot become an empty worklist.", field);
            if (unit.Index == pendingDefeat) continue;
            Battle01TurnCompletionReceipt? death = null;
            for (var receipt = current.TurnCompletion; receipt is not null; receipt = receipt.Previous)
                if (receipt.PlayerPhysicalAttack is { DefeatedTarget: true } attack && attack.TargetIndex == unit.Index)
                {
                    if (death is not null) throw new ArgumentException("An enemy cannot be killed twice.", field);
                    death = receipt;
                }
            if (death is null) throw new ArgumentException("A cleaned enemy requires its death receipt.", field);
            ValidateDefeatReceipt(death);
            if (!Battle01EnemyPhysicalAttack.SameStats(unit.Stats, death.PlayerPhysicalAttack!.Effect.AfterStats))
                throw new ArgumentException("Retain the cleaned enemy's replayed stats.", field);
        }
    }

    private static Battle01FactionCounts RequireContinuingFactions(Battle01InitializedState current, string field)
    {
        var living = current.Roster.Where(unit => unit.Position is { X: >= 0 and < 128 } && unit.Stats.HpCurrent > 0);
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
            if (unit.Stats.HpCurrent == 0 && unit.Position is null && unit.Index >= 128) continue;
            if (unit.Stats.HpCurrent == 0 || unit.Position is not { } position || !Battle01Initialization.WithinArea(position))
                throw new ArgumentException("Every controlled combatant must remain placed in the fixed area.", "position");
            int offset = Battle01PlayerMovement.Offset(position);
            if (occupancy[offset] != -1)
                throw new ArgumentException("Controlled combatants cannot share a live cell.", "occupancy");
            occupancy[offset] = unit.Index;
        }
        if (!occupancy.SequenceEqual(current.Occupancy))
            throw new ArgumentException("The retained live occupancy must match the complete roster.", "occupancy");
        if (!ReferenceEquals(current.Roster.Single(unit => unit.Index == actorIndex).Stats, entryStats))
            throw new ArgumentException("The actor's admitted effective stats must remain unchanged since control entry.", "stats");
        // Retain this turn's immutable effective stats, including HP from a previous physical
        // reaction. Never reconstruct startup maxima, base stats or stacked equipment here.
    }
}
