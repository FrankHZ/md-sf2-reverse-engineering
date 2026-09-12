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
    private Battle01PhysicalCompletionPolicy(bool allowsAllyDefeat, bool allowsLeaderDefeat = false, bool allowsChesterCounter = false)
    { AllowsAllyDefeat = allowsAllyDefeat; AllowsLeaderDefeat = allowsLeaderDefeat; AllowsChesterCounter = allowsChesterCounter; }
    public static Battle01PhysicalCompletionPolicy ControlledNonlethalStrike { get; } = new(false);
    public static Battle01PhysicalCompletionPolicy ControlledFirstAllyDefeat { get; } = new(true);
    public static Battle01PhysicalCompletionPolicy ControlledLeaderDefeatPending { get; } = new(true, true);
    public static Battle01PhysicalCompletionPolicy ControlledNonlethalChesterCounterAndExp { get; } = new(false, allowsChesterCounter: true);
    internal bool AllowsAllyDefeat { get; }
    internal bool AllowsLeaderDefeat { get; }
    internal bool AllowsChesterCounter { get; }
    public override string Id => AllowsChesterCounter ? "battle01-controlled-nonlethal-chester-counter-exp-v1" :
        AllowsLeaderDefeat ? "battle01-controlled-leader-defeat-pending-v1" : AllowsAllyDefeat
        ? "battle01-controlled-first-chester-defeat-v1" : "battle01-controlled-nonlethal-physical-strike-v1";
    internal static bool IsSupported(Battle01PhysicalCompletionPolicy? policy) =>
        ReferenceEquals(policy, ControlledNonlethalStrike) || ReferenceEquals(policy, ControlledFirstAllyDefeat) ||
        ReferenceEquals(policy, ControlledLeaderDefeatPending) || ReferenceEquals(policy, ControlledNonlethalChesterCounterAndExp);
}

public sealed record Battle01FactionCounts(int Allies, int Enemies);
public sealed record Battle01EnemyDefeatCleanup(IReadOnlyList<int> FirstWorklist,
    IReadOnlyList<int> AfterTurnWorklist, int CreditedAlly, ushort KillsBefore, ushort KillsAfter);
public sealed record Battle01AllyDefeatCleanup(IReadOnlyList<int> FirstWorklist,
    IReadOnlyList<int> AfterTurnWorklist, int DefeatedAlly, ushort DefeatsBefore, ushort DefeatsAfter);
public sealed class Battle01PlayerPhysicalCompletionPolicy : Battle01TurnCompletionPolicy
{
    private Battle01PlayerPhysicalCompletionPolicy(int maximumDefeats) { MaximumDefeats = maximumDefeats; }
    public static Battle01PlayerPhysicalCompletionPolicy ControlledNonlethalStrikeAndExp { get; } = new(0);
    public static Battle01PlayerPhysicalCompletionPolicy ControlledStrikeAndFirstDefeat { get; } = new(1);
    public static Battle01PlayerPhysicalCompletionPolicy ControlledStrikeAndSecondDefeat { get; } = new(2);
    internal int MaximumDefeats { get; }
    public bool AllowsDefeat => MaximumDefeats > 0;
    public override string Id => MaximumDefeats switch
    {
        0 => "battle01-controlled-player-nonlethal-physical-exp-v1",
        1 => "battle01-controlled-player-first-defeat-exp-gold-kills-v1",
        _ => "battle01-controlled-player-second-defeat-exp-gold-kills-v1"
    };
    internal static bool IsSupported(Battle01PlayerPhysicalCompletionPolicy? policy) =>
        ReferenceEquals(policy, ControlledNonlethalStrikeAndExp) || ReferenceEquals(policy, ControlledStrikeAndFirstDefeat) ||
        ReferenceEquals(policy, ControlledStrikeAndSecondDefeat);
}

public sealed record Battle01TurnCompletionReceipt(int CompletedActorIndex, Battle01TurnCompletionPolicy Policy,
    Battle01FactionCounts BeforeAfterTurn, Battle01FactionCounts AfterAfterTurn, Battle01TurnCompletionReceipt? Previous = null,
    Battle01EnemyStandbyDecision? EnemyStandby = null, int RoundNumber = 1,
    Battle01EnemyPursuitDecision? EnemyPursuit = null, Battle01EnemyPhysicalAttackDecision? EnemyPhysicalAttack = null,
    Battle01PlayerPhysicalAttackDecision? PlayerPhysicalAttack = null, Battle01EnemyDefeatCleanup? EnemyDefeat = null,
    Battle01AllyDefeatCleanup? AllyDefeat = null);

// Separate from an advanced continuing turn: the first outcome exits before after-turn work.
public sealed record Battle01DefeatPendingReceipt(Battle01PhysicalCompletionPolicy Policy,
    Battle01EnemyPhysicalAttackDecision Attack, Battle01AllyDefeatCleanup Cleanup,
    Battle01FactionCounts FirstOutcome, Battle01TurnCompletionReceipt Previous, int RoundNumber)
{
    public bool AfterTurnExecuted => false;
    public bool TurnAdvanced => false;
}

public static class Battle01TurnCompletion
{
    internal static bool HasValidPolicy(Battle01TurnCompletionReceipt receipt) =>
        receipt.PlayerPhysicalAttack is not null
            ? receipt.AllyDefeat is null && Battle01PlayerPhysicalCompletionPolicy.IsSupported(receipt.Policy as Battle01PlayerPhysicalCompletionPolicy) &&
                (!receipt.PlayerPhysicalAttack.DefeatedTarget ||
                    ((Battle01PlayerPhysicalCompletionPolicy)receipt.Policy).AllowsDefeat &&
                    6 - receipt.BeforeAfterTurn.Enemies <= ((Battle01PlayerPhysicalCompletionPolicy)receipt.Policy).MaximumDefeats) &&
                (!ReferenceEquals(receipt.Policy, Battle01PlayerPhysicalCompletionPolicy.ControlledStrikeAndSecondDefeat) ||
                    receipt.CompletedActorIndex == 0 && receipt.BeforeAfterTurn.Enemies >= 4 &&
                    receipt.BeforeAfterTurn.Enemies <= (receipt.PlayerPhysicalAttack.DefeatedTarget ? 4 : 5)) &&
                receipt.PlayerPhysicalAttack.DefeatedTarget == (receipt.EnemyDefeat is not null) &&
                receipt.CompletedActorIndex < 128 && receipt.PlayerPhysicalAttack.ActorIndex == receipt.CompletedActorIndex &&
                receipt.EnemyStandby is null && receipt.EnemyPursuit is null && receipt.EnemyPhysicalAttack is null
            : receipt.EnemyDefeat is null && (receipt.EnemyPhysicalAttack is not null
            ? ((ReferenceEquals(receipt.Policy, Battle01PhysicalCompletionPolicy.ControlledNonlethalStrike) &&
                    receipt.EnemyPhysicalAttack.Counterattack is null && !receipt.EnemyPhysicalAttack.DefeatedTarget && receipt.AllyDefeat is null) ||
                (ReferenceEquals(receipt.Policy, Battle01PhysicalCompletionPolicy.ControlledNonlethalChesterCounterAndExp) &&
                    receipt.EnemyPhysicalAttack.Counterattack is not null && !receipt.EnemyPhysicalAttack.DefeatedTarget && receipt.AllyDefeat is null) ||
                (ReferenceEquals(receipt.Policy, Battle01PhysicalCompletionPolicy.ControlledFirstAllyDefeat) &&
                    receipt.EnemyPhysicalAttack.Counterattack is null &&
                    receipt.EnemyPhysicalAttack.DefeatedTarget && receipt.AllyDefeat is not null &&
                    receipt.BeforeAfterTurn == new Battle01FactionCounts(2, 4))) &&
                receipt.CompletedActorIndex >= 128 && receipt.EnemyStandby is null && receipt.EnemyPursuit is null
            : receipt.AllyDefeat is null && ReferenceEquals(receipt.Policy, Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats));

    internal static Battle01InitializedState CompletePlayerPhysical(Battle01InitializedState current,
        Battle01PlayerPhysicalAttackDecision decision, Battle01PlayerPhysicalCompletionPolicy? policy)
    {
        if (!Battle01PlayerPhysicalCompletionPolicy.IsSupported(policy) || (decision.DefeatedTarget && !policy!.AllowsDefeat))
            throw new ArgumentException("An explicit player physical/EXP completion policy is required.", "policy");
        if (ReferenceEquals(policy, Battle01PlayerPhysicalCompletionPolicy.ControlledStrikeAndSecondDefeat) && decision.ActorIndex != 0)
            throw new ArgumentException("The second-defeat policy belongs to Bowie only.", "policy");
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
            (current, cleanup) = ApplyDefeatCleanup(current, decision, policy!);
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

    private static (Battle01InitializedState State, Battle01EnemyDefeatCleanup Cleanup) ApplyDefeatCleanup(
        Battle01InitializedState current, Battle01PlayerPhysicalAttackDecision decision, Battle01PlayerPhysicalCompletionPolicy policy)
    {
        var dead = current.Roster.Where(unit => unit.Stats.HpCurrent == 0).ToArray();
        var target = dead.FirstOrDefault(unit => unit.Index == decision.TargetIndex);
        if (dead.Length > policy.MaximumDefeats || dead.Count(unit => unit.Index == decision.TargetIndex) != 1 ||
            target is null || decision.TargetIndex < 128 ||
            target.Position is not { } position || position != decision.Target.Position ||
            !Battle01EnemyPhysicalAttack.SameStats(target.Stats, decision.Effect.AfterStats) ||
            decision.Actor.Stats.CurrentKills is not { } kills)
            throw new ArgumentException("Only this reacted enemy may enter the bounded new death worklist.", "cleanup.before");
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
        // Previously cleaned enemies are independently checked against their own receipts below.
        roster[Array.IndexOf(roster, target)] = target.WithPosition(null);
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

    internal static Battle01InitializedState CompleteLeaderDefeat(Battle01InitializedState current,
        Battle01EnemyPhysicalAttackDecision decision, Battle01PhysicalCompletionPolicy? policy)
    {
        if (!ReferenceEquals(policy, Battle01PhysicalCompletionPolicy.ControlledLeaderDefeatPending))
            throw new ArgumentException("The leader-defeat policy must be explicit.", "policy");
        Battle01EnemyPhysicalAttack.ValidateDecision(decision, allowAllyDefeat: true, allowLeaderDefeat: true);
        var target = current.Roster.Single(unit => unit.Index == decision.TargetIndex);
        if (decision.ActorIndex != 129 || decision.TargetIndex != 0 || !decision.DefeatedTarget ||
            decision.Target.Stats.CurrentDefeats != 0 || target.Position != decision.Target.Position ||
            !Battle01EnemyPhysicalAttack.SameStats(target.Stats, decision.Effect.AfterStats) ||
            current.TurnCompletion is null)
            throw new ArgumentException("Only the supplied first leader death enters the terminal cleanup.", "cleanup.before");
        // Bowie's zero HP returns directly from ExecuteBattleCutscene_Defeated; no boss cutscene.
        var roster = current.Roster.ToArray(); var occupancy = current.Occupancy.ToArray();
        int cell = Battle01PlayerMovement.Offset(target.RequirePosition());
        if (occupancy[cell] != 0) throw new ArgumentException("Retain Bowie's cell before cleanup.", "occupancy");
        ushort defeats = DefeatsAfterDeath(decision.Target.Stats.CurrentDefeats.Value);
        roster[Array.IndexOf(roster, target)] = target.WithStats(target.Stats.WithCurrentDefeats(defeats)).WithPosition(null);
        occupancy[cell] = -1;
        var cleaned = new Battle01InitializedState(current, roster, Array.AsReadOnly(occupancy), null!);
        // First CountRemainingCombatants overrides the living ally count to zero when Bowie is dead.
        // Stop at BattleLoop_Defeat entry: no after-turn effects, second cleanup/count or pointer advance.
        var result = new Battle01InitializedState(cleaned, new Battle01DefeatPendingReceipt(policy!, decision,
            new(Array.AsReadOnly(new[] { 0 }), Array.Empty<int>(), 0, 0, defeats),
            new(0, cleaned.Roster.Count(unit => unit.Index >= 128 && unit.Stats.HpCurrent > 0)),
            current.TurnCompletion, current.FirstRound!.RoundNumber));
        _ = RequireDefeatPending(result);
        return result;
    }

    // Validate the terminal effect, then reuse the complete continuing-history validator on its before-image.
    internal static Battle01InitializedState RequireDefeatPending(Battle01InitializedState current)
    {
        if (current.DefeatPending is not { } terminal || current.FirstControl is not null ||
            current.FirstRound is not { RoundNumber: 16, CurrentTurnOffset: 0, CurrentCandidate.CombatantIndex: 129 } ||
            terminal.RoundNumber != 16 || !ReferenceEquals(terminal.Previous, current.TurnCompletion) ||
            !ReferenceEquals(terminal.Policy, Battle01PhysicalCompletionPolicy.ControlledLeaderDefeatPending) ||
            terminal.FirstOutcome != new Battle01FactionCounts(0, 4))
            throw new ArgumentException("Retain the first leader-defeat boundary and unadvanced round.", "defeat.history");
        int count = 0;
        for (var receipt = current.TurnCompletion; receipt is not null; receipt = receipt.Previous) count++;
        var d = terminal.Attack; var cleanup = terminal.Cleanup;
        if (count != 119 || d.ActorIndex != 129 || d.TargetIndex != 0 || !d.DefeatedTarget ||
            d.Target.Stats.CurrentDefeats != 0 || cleanup.DefeatedAlly != 0 || cleanup.DefeatsBefore != 0 ||
            cleanup.DefeatsAfter != DefeatsAfterDeath(cleanup.DefeatsBefore) ||
            !cleanup.FirstWorklist.SequenceEqual(new[] { 0 }) || cleanup.AfterTurnWorklist.Count != 0 ||
            current.NewlyTestedRegionMask != 0 || current.RandomSeedImage != d.Effect.MainSeedAfter ||
            current.RandomSeedCopy != d.SeedCopyAfter || current.AiLastTargets.Count != 48 || current.AiLastTargets[1] != 0 ||
            current.AiMemory.Count != 48 || current.AiMemory[1] != d.Memory)
            throw new ArgumentException("Retain the terminal strike, first cleanup and RNG endpoints.", "defeat.history");
        Battle01EnemyPhysicalAttack.ValidateDecision(d, allowAllyDefeat: true, allowLeaderDefeat: true);
        var roster = current.Roster.ToArray();
        var actor = roster.Single(unit => unit.Index == 129); var target = roster.Single(unit => unit.Index == 0);
        if (actor.Position != d.Destination || target.Position is not null ||
            !SameCombatant(actor, d.Actor.WithPosition(d.Destination)) ||
            !SameCombatant(target, d.Target.WithStats(d.Effect.AfterStats.WithCurrentDefeats(cleanup.DefeatsAfter)).WithPosition(null)) ||
            !roster.Where(unit => unit.Stats.HpCurrent == 0).Select(unit => unit.Index).SequenceEqual(new[] { 0, 2, 131, 132 }) ||
            roster.Count(unit => unit.Index < 128 && unit.Stats.HpCurrent > 0) != 1)
            throw new ArgumentException("Retain the replayed actor, cleaned leader and earlier deaths.", "defeat.history");
        RequireOccupancy(roster, current.Occupancy);
        roster[Array.IndexOf(roster, actor)] = d.Actor;
        roster[Array.IndexOf(roster, target)] = d.Target;
        var occupancy = Enumerable.Repeat(-1, 48 * 48).ToArray();
        foreach (var unit in roster.Where(unit => unit.Position is not null))
        {
            int cell = Battle01PlayerMovement.Offset(unit.RequirePosition());
            if (occupancy[cell] != -1) throw new ArgumentException("Before-image cells must be distinct.", "occupancy");
            occupancy[cell] = unit.Index;
        }
        var lastTargets = current.AiLastTargets.ToArray(); lastTargets[1] = d.LastTargetBefore;
        var prior = new Battle01InitializedState(current, roster, occupancy, d.SeedCopyBefore, d.MainSeedBefore, lastTargets, 7);
        Battle01EnemyStandby.RequireCurrentRound(prior);
        var expected = Battle01EnemyPhysicalAttack.Decide(prior, d.Actor, allowAllyDefeat: true, allowLeaderDefeat: true);
        if (expected.TargetIndex != d.TargetIndex || expected.Destination != d.Destination ||
            !expected.MoveString.SequenceEqual(d.MoveString) || expected.Priorities.Count != d.Priorities.Count ||
            expected.Priorities.Where((p, i) => p.Candidate != d.Priorities[i].Candidate ||
                p.LandMultiplier != d.Priorities[i].LandMultiplier || !SameCombatant(p.Target, d.Priorities[i].Target)).Any())
            throw new ArgumentException("The terminal target cohort and path must reproduce the retained terrain.", "defeat.history");
        return prior;
    }

    internal static bool SameCombatant(Battle01Combatant a, Battle01Combatant b) =>
        a.Deployment == b.Deployment && a.ClassId == b.ClassId && a.EnemySource == b.EnemySource &&
        a.AiBitfield == b.AiBitfield && a.Position == b.Position && Battle01EnemyPhysicalAttack.SameStats(a.Stats, b.Stats);

    private static void RequireOccupancy(IReadOnlyList<Battle01Combatant> roster, IReadOnlyList<int> actual)
    {
        var expected = Enumerable.Repeat(-1, 48 * 48).ToArray();
        foreach (var unit in roster)
        {
            if (unit.Stats.HpCurrent == 0 && unit.Position is null) continue;
            if (unit.Stats.HpCurrent == 0 || unit.Position is not { } position || !Battle01Initialization.WithinArea(position))
                throw new ArgumentException("Only living units retain terminal placement.", "occupancy");
            int cell = Battle01PlayerMovement.Offset(position);
            if (expected[cell] != -1) throw new ArgumentException("Live cells must be distinct.", "occupancy");
            expected[cell] = unit.Index;
        }
        if (!expected.SequenceEqual(actual)) throw new ArgumentException("Retain the complete terminal occupancy.", "occupancy");
    }

    internal static Battle01InitializedState CompletePhysical(Battle01InitializedState current,
        Battle01EnemyPhysicalAttackDecision decision, Battle01PhysicalCompletionPolicy? policy)
    {
        if (!Battle01PhysicalCompletionPolicy.IsSupported(policy))
            throw new ArgumentException("An explicit controlled physical completion policy is required.", "policy");
        Battle01EnemyPhysicalAttack.ValidateDecision(decision, policy!.AllowsAllyDefeat, allowChesterCounter: policy.AllowsChesterCounter);
        RequireDefeatedWrapperReturn(current);
        Battle01AllyDefeatCleanup? cleanup = null;
        if (decision.DefeatedTarget) (current, cleanup) = ApplyAllyDefeatCleanup(current, decision);
        else RequireEmptyKilledCleanup(current, "cleanup.before");
        var before = RequireContinuingFactions(current, "outcome.before");
        // The admitted actor's after-turn refresh changes no modifiers, status, MP or equipment.
        // Its target HP has already been authorized by the physical reaction, not by STAY.
        NormalizeControlledNoEffectTurn(current, decision.ActorIndex, decision.ActorAfterStats,
            Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
        RequireEmptyKilledCleanup(current, "cleanup.after", cleanup?.DefeatedAlly);
        var after = RequireContinuingFactions(current, "outcome.after");
        var result = new Battle01InitializedState(current, current.FirstRound!.AdvanceCompletedPlayerTurn(),
            new(decision.ActorIndex, decision.Counterattack is not null ? Battle01PhysicalCompletionPolicy.ControlledNonlethalChesterCounterAndExp :
                cleanup is null ? Battle01PhysicalCompletionPolicy.ControlledNonlethalStrike : policy!,
                before, after, current.TurnCompletion, RoundNumber: current.FirstRound.RoundNumber,
                EnemyPhysicalAttack: decision, AllyDefeat: cleanup));
        Battle01FirstRound.RequireCurrentPrefix(result);
        Battle01EnemyStandby.RequireThinkingHistory(result);
        return result;
    }

    internal static ushort DefeatsAfterDeath(ushort before) => (ushort)Math.Min(9999, before + 1);

    private static (Battle01InitializedState State, Battle01AllyDefeatCleanup Cleanup) ApplyAllyDefeatCleanup(
        Battle01InitializedState current, Battle01EnemyPhysicalAttackDecision decision)
    {
        var target = current.Roster.Single(unit => unit.Index == decision.TargetIndex);
        if (decision.ActorIndex != 133 || decision.TargetIndex != 2 || decision.Target.Stats.HpCurrent == 0 ||
            decision.Target.Stats.CurrentDefeats != 0 || decision.Target.Stats.CurrentKills is not null ||
            target.Position is not { } position || position != decision.Target.Position ||
            !Battle01EnemyPhysicalAttack.SameStats(target.Stats, decision.Effect.AfterStats) ||
            !current.Roster.Where(unit => unit.Stats.HpCurrent == 0).Select(unit => unit.Index).SequenceEqual(new[] { 2, 131, 132 }))
            throw new ArgumentException("Only the first supplied Chester defeat after the two enemy cleanups is admitted.", "cleanup.before");
        var roster = current.Roster.ToArray(); var occupancy = current.Occupancy.ToArray();
        int cell = Battle01PlayerMovement.Offset(position);
        if (occupancy[cell] != 2) throw new ArgumentException("Retain Chester's cell before cleanup.", "occupancy");
        ushort defeats = DefeatsAfterDeath(decision.Target.Stats.CurrentDefeats.Value);
        // Status0 and already-refreshed controlled effective stats remain unchanged; HP stays0.
        roster[Array.IndexOf(roster, target)] = target.WithStats(target.Stats.WithCurrentDefeats(defeats)).WithPosition(null);
        occupancy[cell] = -1;
        return (new Battle01InitializedState(current, roster, Array.AsReadOnly(occupancy), current.FirstControl!),
            new(Array.AsReadOnly(new[] { 2 }), Array.Empty<int>(), 2, 0, defeats));
    }

    internal static void ValidateAllyDefeatReceipt(Battle01TurnCompletionReceipt receipt)
    {
        if (receipt.EnemyPhysicalAttack is not { DefeatedTarget: true } decision || receipt.AllyDefeat is not { } cleanup ||
            !HasValidPolicy(receipt) || decision.ActorIndex != 133 || receipt.CompletedActorIndex != 133 ||
            decision.TargetIndex != 2 || decision.Target.Stats.HpCurrent == 0 || decision.Target.Stats.CurrentKills is not null ||
            decision.Target.Stats.CurrentDefeats != 0 || cleanup.DefeatedAlly != 2 || cleanup.DefeatsBefore != 0 ||
            cleanup.DefeatsAfter != DefeatsAfterDeath(cleanup.DefeatsBefore) ||
            !cleanup.FirstWorklist.SequenceEqual(new[] { 2 }) || cleanup.AfterTurnWorklist.Count != 0 ||
            receipt.AfterAfterTurn != receipt.BeforeAfterTurn)
            throw new ArgumentException("Retain Chester's distinct defeat, counter and both worklists.", "attack.history");
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
            if (unit.Position is not null || (unit.Index < 128 && unit.Index != 2))
                throw new ArgumentException("Unprocessed deaths cannot become an empty worklist.", field);
            if (unit.Index == pendingDefeat) continue;
            if (unit.Index == 2)
            {
                Battle01TurnCompletionReceipt? allyDeath = null;
                for (var prior = current.TurnCompletion; prior is not null; prior = prior.Previous)
                    if (prior.AllyDefeat is not null)
                    {
                        if (allyDeath is not null) throw new ArgumentException("Chester cannot be defeated twice.", field);
                        allyDeath = prior;
                    }
                if (allyDeath is null) throw new ArgumentException("Cleaned Chester requires his defeat receipt.", field);
                ValidateAllyDefeatReceipt(allyDeath);
                if (!Battle01EnemyPhysicalAttack.SameStats(unit.Stats,
                    allyDeath.EnemyPhysicalAttack!.Effect.AfterStats.WithCurrentDefeats(allyDeath.AllyDefeat!.DefeatsAfter)))
                    throw new ArgumentException("Retain Chester's replayed HP and defeat counter.", field);
                continue;
            }
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
            if (unit.Stats.HpCurrent == 0 && unit.Position is null && (unit.Index >= 128 || unit.Index == 2)) continue;
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
