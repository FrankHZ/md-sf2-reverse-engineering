using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

public sealed class Battle01PlayerAttackSelection
{
    internal Battle01PlayerAttackSelection(int[] targets, int selected)
    {
        Targets = Array.AsReadOnly(targets); Selected = selected;
    }
    public IReadOnlyList<int> Targets { get; }
    public int Selected { get; }
    public int TargetIndex => Targets[Selected];
}

public sealed record Battle01PlayerPhysicalAttackDecision(Battle01Combatant Actor, Battle01Combatant Target,
    MapPosition MovementOrigin, IReadOnlyList<byte> MoveString, IReadOnlyList<int> LegalTargets,
    byte TargetTerrain, int LandMultiplier, uint MainSeedBefore, ushort SeedCopy,
    Battle01PhysicalEffect Effect, int AccumulatedExp, int HalvedExp, int AwardedExp, Battle01Stats ActorAfterStats,
    uint? GoldBefore = null, uint? GoldAfter = null)
{
    public int ActorIndex => Actor.Index;
    public int TargetIndex => Target.Index;
    public MapPosition RangeOrigin => Actor.Position ?? throw new ArgumentException("The attacker must be placed.", "attack.history");
    public bool DefeatedTarget => Effect.TemporaryHp == 0;
    public byte Action => 0;
    public ushort ItemOrSpellWord => (ushort)TargetIndex;
    public string CombatProfile => "battle01-class0-wooden-sword-effective-prowess3-v1";
    // Construction/reaction/award semantics only: original reaction animation and VInt RNG are omitted.
    public string RandomPolicy => "controlled-presentation-omitted-semantics-v1";
}

public static class Battle01PlayerPhysicalAttack
{
    public static void RequireAccountingInputs(Battle01InitializedState current, uint? gold, ushort? bowieKills)
    {
        var original = Battle01EnemyStandby.RequireThinkingHistory(current);
        if (original != (gold, bowieKills))
            throw new ArgumentException("Live accounting must retain its declared preparation inputs.", "accounting.input");
    }

    public static Battle01InitializedState Begin(Battle01InitializedState current, int actorIndex)
    {
        var control = RequireControl(current, actorIndex, Battle01Phase.PlayerActionChoice);
        RequireActor(current.Roster.Single(unit => unit.Index == actorIndex));
        var targets = Targets(current, actorIndex);
        if (targets.Length == 0) throw new Battle01PhysicalAttackUnsupportedException("emptyTargets");
        return Select(current, control, new(targets, 0));
    }

    public static Battle01InitializedState Cycle(Battle01InitializedState current, int actorIndex, int direction)
    {
        var control = RequireControl(current, actorIndex, Battle01Phase.PlayerAttackTargetSelection);
        var selection = RequireSelection(current, control);
        if (direction is not (-1 or 1)) throw new ArgumentException("Use the previous or next target.", nameof(direction));
        return Select(current, control, new(selection.Targets.ToArray(),
            (selection.Selected + direction + selection.Targets.Count) % selection.Targets.Count));
    }

    public static Battle01InitializedState Cancel(Battle01InitializedState current, int actorIndex)
    {
        var control = RequireControl(current, actorIndex, Battle01Phase.PlayerAttackTargetSelection);
        return new(current, current.Roster.ToArray(), current.Occupancy,
            control.WithMovement(new(control.Movement.Range, control.Movement.Preview, Battle01PlayerMovementStage.ActionChoice)));
    }

    public static Battle01InitializedState Confirm(Battle01InitializedState current, int actorIndex,
        Battle01PlayerPhysicalCompletionPolicy? policy)
    {
        if (!Battle01PlayerPhysicalCompletionPolicy.IsSupported(policy))
            throw new ArgumentException("The player physical/EXP completion policy must be explicit.", "policy");
        var decision = Decide(current, actorIndex, policy!.AllowsDefeat);
        var roster = current.Roster.ToArray();
        int actor = Array.FindIndex(roster, unit => unit.Index == actorIndex);
        int target = Array.FindIndex(roster, unit => unit.Index == decision.TargetIndex);
        roster[actor] = roster[actor].WithStats(decision.ActorAfterStats);
        roster[target] = roster[target].WithStats(decision.Effect.AfterStats);
        var replayed = new Battle01InitializedState(current, roster, decision.Effect.MainSeedAfter, decision.GoldAfter);
        return Battle01TurnCompletion.CompletePlayerPhysical(replayed, decision, policy);
    }

    internal static Battle01PlayerPhysicalAttackDecision Decide(Battle01InitializedState current, int actorIndex, bool allowDefeat = false)
    {
        var control = RequireControl(current, actorIndex, Battle01Phase.PlayerAttackTargetSelection);
        var selected = RequireSelection(current, control);
        var actor = current.Roster.Single(unit => unit.Index == actorIndex);
        var target = current.Roster.Single(unit => unit.Index == selected.TargetIndex);
        RequireActor(actor); RequireTarget(target, current.FirstRound!.RoundNumber);
        var targetPosition = target.Position ?? throw new Battle01PhysicalAttackUnsupportedException("targetPlacement");
        var actorPosition = actor.Position ?? throw new Battle01PhysicalAttackUnsupportedException("actorPlacement");
        byte terrain = current.TerrainAt(targetPosition);
        int multiplier = TargetLandMultiplier(terrain);
        var resolved = Resolve(actor.Stats, target.Stats, multiplier, current.RandomSeedImage,
            target.Index, actorPosition, targetPosition, allowDefeat);
        uint? goldAfter = current.CurrentGold;
        if (resolved.Effect.TemporaryHp == 0)
        {
            if (current.Roster.Any(unit => unit.Stats.HpCurrent == 0))
                throw new Battle01PhysicalAttackUnsupportedException("additionalDefeat");
            if (current.CurrentGold is not { } gold || actor.Stats.CurrentKills is null)
                throw new Battle01PhysicalAttackUnsupportedException("killAccountingInput");
            goldAfter = GoldAfterKill(gold);
        }
        return new(actor, target, control.Movement.Range.Origin, control.Movement.Preview.Directions,
            selected.Targets, terrain, multiplier, current.RandomSeedImage, current.RandomSeedCopy!.Value,
            resolved.Effect, resolved.Accumulated, resolved.Halved, resolved.Award, resolved.ActorAfter,
            current.CurrentGold, goldAfter);
    }

    private static Battle01FirstControlState RequireControl(Battle01InitializedState current, int actorIndex, Battle01Phase phase)
    {
        ArgumentNullException.ThrowIfNull(current);
        if (current.Phase != phase || current.FirstControl is not { } control || current.FirstRound is not { } order)
            throw new ArgumentException("Player attack requires its current manual action phase.", "phase");
        if (actorIndex >= 128 || control.ActorIndex != actorIndex || order.CurrentCandidate?.CombatantIndex != actorIndex)
            throw new ArgumentException("Player attack must name the actual controlled actor.", "actor");
        Battle01FirstRound.RequireCurrentPrefix(current);
        Battle01EnemyStandby.RequireThinkingHistory(current);
        Battle01TurnCompletion.RequireContinuingNoEffectState(current);
        var actor = current.Roster.Single(unit => unit.Index == actorIndex);
        if (!ReferenceEquals(actor.Stats, control.Movement.Range.EffectiveStatsAtEntry) ||
            actor.Position != control.Movement.Cursor || !control.Movement.Range.CanStopAt(actor.Position))
            throw new ArgumentException("Retain the admitted stats and confirmed provisional destination.", "attack.control");
        return control;
    }

    private static Battle01PlayerAttackSelection RequireSelection(Battle01InitializedState current, Battle01FirstControlState control)
    {
        var selected = control.Movement.Attack;
        if (selected is null || selected.Targets.Count == 0 || selected.Selected < 0 || selected.Selected >= selected.Targets.Count ||
            !selected.Targets.SequenceEqual(Targets(current, control.ActorIndex)))
            throw new ArgumentException("Retain the complete live legal target list and selection.", "attack.selection");
        return selected;
    }

    private static Battle01InitializedState Select(Battle01InitializedState current, Battle01FirstControlState control,
        Battle01PlayerAttackSelection selected) => new(current, current.Roster.ToArray(), current.Occupancy,
            control.WithMovement(new(control.Movement.Range, control.Movement.Preview, Battle01PlayerMovementStage.TargetSelection, selected)));

    internal static int[] Targets(Battle01InitializedState current, int actorIndex)
    {
        var origin = current.Roster.Single(unit => unit.Index == actorIndex).RequirePosition();
        var targets = new List<int>();
        // Range1's source ring: down, right, up, left, resolved through live occupancy.
        foreach (var (x, y) in new[] { (0, 1), (1, 0), (0, -1), (-1, 0) })
        {
            var position = new MapPosition(origin.X + x, origin.Y + y);
            if (!Battle01Initialization.WithinArea(position)) continue;
            int index = current.OccupantAt(position);
            var target = current.Roster.SingleOrDefault(unit => unit.Index == index);
            if (target is not null && (target.Index < 128) != (actorIndex < 128) && target.Position == position &&
                target.Stats.HpCurrent > 0 && target.AiBitfield is { } ai && (ai & 8) == 0) targets.Add(index);
        }
        return targets.ToArray();
    }

    internal static void RequireActor(Battle01Combatant actor)
    {
        Battle01EnemyPhysicalAttack.RequireTargetProfile(actor);
        if (actor.AiBitfield != 0 || actor.Stats.CurrentExp is null)
            throw new Battle01PhysicalAttackUnsupportedException("actorExpProfile");
    }

    internal static void RequireTarget(Battle01Combatant target, int roundNumber)
    {
        Battle01EnemyStandby.RequireRegularEnemy(target, roundNumber, active: (target.AiBitfield & 1) != 0);
    }

    internal static int TargetLandMultiplier(byte terrain) => terrain switch
    {
        1 => 230,
        _ => throw new Battle01PhysicalAttackUnsupportedException("targetTerrain")
    };

    internal static (Battle01PhysicalEffect Effect, int Accumulated, int Halved, int Award, Battle01Stats ActorAfter)
        Resolve(Battle01Stats actor, Battle01Stats target, int multiplier, uint main, int targetIndex,
            MapPosition actorPosition, MapPosition targetPosition, bool allowDefeat = false)
    {
        if (actor.CurrentExp is not { } exp) throw new Battle01PhysicalAttackUnsupportedException("actorExpProfile");
        var effect = Battle01EnemyPhysicalAttack.ResolveSingleStrike(actor.Attack, target, multiplier, main,
            targetIndex, actorPosition, targetPosition, 8, 16, 2, allowDefeat);
        // The admitted unpromoted level1 versus original level0 GIZMO gives kill EXP50.
        int accumulated = DamageExperience(effect.Damage, target.HpMax);
        if (effect.TemporaryHp == 0) accumulated = Math.Min(49, accumulated + 50);
        int halved = accumulated >> 1, award = halved;
        var rolls = effect.Rolls.ToList(); main = effect.MainSeedAfter;
        if (Battle01EnemyPhysicalAttack.MainRoll(ref main, rolls, "exp-plus", 16) == 0) award++;
        if (Battle01EnemyPhysicalAttack.MainRoll(ref main, rolls, "exp-minus", 16) == 0) award--;
        award = Math.Max(1, award);
        int after = Math.Min(200, exp + award);
        if (after >= 100) throw new Battle01PhysicalAttackUnsupportedException("levelUp");
        return (effect with { Rolls = rolls.AsReadOnly() }, accumulated, halved, award, actor.WithCurrentExp((byte)after));
    }

    internal static int DamageExperience(int damage, ushort targetMaxHp) => Math.Min(49, 50 * damage / targetMaxHp);
    internal static uint GoldAfterKill(uint gold) => (uint)Math.Min(9999999UL, (ulong)gold + 60);
    internal static ushort KillsAfterKill(ushort kills) => (ushort)Math.Min(9999, (int)kills + 1);

    internal static void ValidateDecision(Battle01PlayerPhysicalAttackDecision decision)
    {
        RequireActor(decision.Actor); RequireTarget(decision.Target, 2);
        if (decision.LandMultiplier != TargetLandMultiplier(decision.TargetTerrain) ||
            decision.LegalTargets.Count is < 1 or > 4 || !decision.LegalTargets.Contains(decision.TargetIndex) ||
            decision.LegalTargets.Any(index => index is < 128 or > 133) ||
            decision.LegalTargets.Distinct().Count() != decision.LegalTargets.Count ||
            decision.MoveString.Count == 0 || decision.MoveString[^1] != 255)
            throw new ArgumentException("Retain the player physical profile, target and path.", "attack.history");
        var position = decision.MovementOrigin;
        foreach (byte direction in decision.MoveString.SkipLast(1))
        {
            (int x, int y) = direction switch { 0 => (1, 0), 1 => (0, -1), 2 => (-1, 0), 3 => (0, 1),
                _ => throw new ArgumentException("Invalid player attack movement.", "attack.history") };
            position = new(position.X + x, position.Y + y);
            if (!Battle01Initialization.WithinArea(position)) throw new ArgumentException("Player path left the area.", "attack.history");
        }
        if (position != decision.RangeOrigin) throw new ArgumentException("Player range must start at the provisional tile.", "attack.history");
        var expected = Resolve(decision.Actor.Stats, decision.Target.Stats, decision.LandMultiplier,
            decision.MainSeedBefore, decision.TargetIndex, decision.RangeOrigin,
            decision.Target.Position ?? throw new ArgumentException("Retain pre-death placement.", "attack.history"), decision.DefeatedTarget);
        if (decision.DefeatedTarget
            ? decision.GoldBefore is not { } gold || decision.GoldAfter != GoldAfterKill(gold) || decision.Actor.Stats.CurrentKills is null
            : decision.GoldBefore != decision.GoldAfter)
            throw new ArgumentException("Player gold must reproduce its construction input.", "attack.history");
        var a = decision.Effect; var e = expected.Effect;
        if (!ReferenceEquals(a.BeforeStats, decision.Target.Stats) || a.Dodged != e.Dodged || a.Critical != e.Critical ||
            a.Damage != e.Damage || a.TemporaryHp != e.TemporaryHp || a.RestoredHp != e.RestoredHp || a.Reaction != e.Reaction ||
            !a.Rolls.SequenceEqual(e.Rolls) || !Battle01EnemyPhysicalAttack.SameStats(a.AfterStats, e.AfterStats) ||
            decision.AccumulatedExp != expected.Accumulated || decision.HalvedExp != expected.Halved ||
            decision.AwardedExp != expected.Award || !Battle01EnemyPhysicalAttack.SameStats(decision.ActorAfterStats, expected.ActorAfter))
            throw new ArgumentException("Player physical HP, EXP and main RNG must reproduce their inputs.", "attack.history");
    }
}
