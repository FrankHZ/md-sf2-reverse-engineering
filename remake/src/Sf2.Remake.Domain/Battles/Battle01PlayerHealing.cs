using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

public sealed class Battle01PlayerHealingSelection
{
    internal Battle01PlayerHealingSelection(byte spellEntry, IEnumerable<int> targets, int selected = 0)
    { SpellEntry = spellEntry; Targets = Array.AsReadOnly(targets.ToArray()); Selected = selected; }
    public byte SpellEntry { get; }
    public IReadOnlyList<int> Targets { get; }
    public int Selected { get; }
    public int? TargetIndex => Selected >= 0 && Selected < Targets.Count ? Targets[Selected] : null;
}

public sealed record Battle01HealingReaction(int ActorIndex, int HpDelta, int MpDelta, int ExpDelta);
public sealed record Battle01HealingEffect(int Recovery, int AccumulatedExp, int AwardedExp,
    Battle01Stats ActorAfterStats, Battle01Stats TargetAfterStats,
    IReadOnlyList<Battle01MainRandomRoll> Rolls, IReadOnlyList<Battle01HealingReaction> Reactions)
{
    public uint MainSeedAfter => Rolls[^1].AfterImage;
}
public sealed record Battle01PlayerHealingDecision(Battle01Combatant Actor, Battle01Combatant Target,
    MapPosition MovementOrigin, IReadOnlyList<byte> MoveString, IReadOnlyList<int> LegalTargets,
    byte SpellEntry, uint MainSeedBefore, ushort SeedCopy, Battle01HealingEffect Effect)
{
    public int ActorIndex => Actor.Index;
    public int TargetIndex => Target.Index;
    public byte Action => 1;
    public ushort ItemOrSpellWord => SpellEntry;
    public string RandomPolicy => "controlled-presentation-omitted-semantics-v1";
}

// One accepted support action; the physical profiles and later spell/action families stay separate.
public static class Battle01PlayerHealing
{
    public const byte HealOne = 0;
    public const byte MpCost = 3;
    public const byte Power = 15;

    public static Battle01InitializedState Begin(Battle01InitializedState current, int actorIndex)
    {
        var control = RequireControl(current, actorIndex, Battle01Phase.PlayerActionChoice);
        return Select(current, control, Battle01PlayerMovementStage.HealingSpellSelection, new(HealOne, []));
    }

    public static Battle01InitializedState SelectSpell(Battle01InitializedState current, int actorIndex, byte spellEntry)
    {
        var control = RequireControl(current, actorIndex, Battle01Phase.PlayerHealingSpellSelection);
        RequireSpellSelection(control);
        if (spellEntry != HealOne) throw Rejected("spell", "Only the learned HEAL 1 entry is admitted.");
        return Select(current, control, Battle01PlayerMovementStage.HealingTargetSelection,
            new(spellEntry, Targets(current, actorIndex)));
    }

    public static Battle01InitializedState CycleTarget(Battle01InitializedState current, int actorIndex, int direction)
    {
        var control = RequireControl(current, actorIndex, Battle01Phase.PlayerHealingTargetSelection);
        var selected = RequireTargets(current, control);
        if (direction is not (-1 or 1)) throw Rejected("direction", "Choose the previous or next target.");
        return Select(current, control, Battle01PlayerMovementStage.HealingTargetSelection,
            new(selected.SpellEntry, selected.Targets, (selected.Selected + direction + selected.Targets.Count) % selected.Targets.Count));
    }

    public static Battle01InitializedState Cancel(Battle01InitializedState current, int actorIndex)
    {
        ArgumentNullException.ThrowIfNull(current);
        if (current.Phase == Battle01Phase.PlayerHealingTargetSelection)
        {
            var control = RequireControl(current, actorIndex, Battle01Phase.PlayerHealingTargetSelection);
            RequireTargets(current, control);
            return Select(current, control, Battle01PlayerMovementStage.HealingSpellSelection, new(HealOne, []));
        }
        var spellControl = RequireControl(current, actorIndex, Battle01Phase.PlayerHealingSpellSelection);
        RequireSpellSelection(spellControl);
        return Select(current, spellControl, Battle01PlayerMovementStage.ActionChoice, null);
    }

    public static Battle01InitializedState Confirm(Battle01InitializedState current, int actorIndex,
        Battle01HealingCompletionPolicy? policy)
    {
        if (!ReferenceEquals(policy, Battle01HealingCompletionPolicy.ControlledSarahHealOneBowie))
            throw Rejected("policy", "The bounded Sarah HEAL 1 policy is required.");
        var decision = Decide(current, actorIndex);
        var roster = current.Roster.ToArray();
        roster[Array.FindIndex(roster, unit => unit.Index == actorIndex)] = decision.Actor.WithStats(decision.Effect.ActorAfterStats);
        roster[Array.FindIndex(roster, unit => unit.Index == decision.TargetIndex)] = decision.Target.WithStats(decision.Effect.TargetAfterStats);
        var replayed = new Battle01InitializedState(current, roster, decision.Effect.MainSeedAfter);
        return Battle01TurnCompletion.CompletePlayerHealing(replayed, decision, policy);
    }

    internal static Battle01PlayerHealingDecision Decide(Battle01InitializedState current, int actorIndex)
    {
        var control = RequireControl(current, actorIndex, Battle01Phase.PlayerHealingTargetSelection);
        var selected = RequireTargets(current, control);
        // Self is range-legal; this slice does not consume its full-HP healing/EXP branch.
        if (selected.TargetIndex != 0) throw Rejected("targetUnsupported", "Only HEAL 1 on Bowie is implemented.");
        var actor = current.Roster.Single(unit => unit.Index == actorIndex);
        var target = current.Roster.Single(unit => unit.Index == selected.TargetIndex);
        Battle01EnemyPhysicalAttack.RequireTargetProfile(target);
        var effect = Resolve(actor.Stats, target.Stats, current.RandomSeedImage);
        return new(actor, target, control.Movement.Range.Origin, control.Movement.Preview.Directions,
            selected.Targets, selected.SpellEntry, current.RandomSeedImage, current.RandomSeedCopy!.Value, effect);
    }

    private static Battle01FirstControlState RequireControl(Battle01InitializedState current, int actorIndex, Battle01Phase phase)
    {
        ArgumentNullException.ThrowIfNull(current);
        if (current.Phase != phase || current.FirstControl is not { } control ||
            current.FirstRound is not { RoundNumber: 13, CurrentTurnOffset: 4 } order)
            throw Rejected("phase", "HEAL 1 belongs to the first R13 Sarah action.");
        if (actorIndex != 1 || control.ActorIndex != actorIndex || order.CurrentCandidate?.CombatantIndex != actorIndex)
            throw Rejected("actor", "Name the actual Sarah turn.");
        int count = 0;
        for (var r = current.TurnCompletion; r is not null; r = r.Previous) count++;
        if (count != 102) throw Rejected("history", "Retain the complete pre-heal history.");
        Battle01FirstRound.RequireCurrentPrefix(current);
        Battle01EnemyStandby.RequireThinkingHistory(current);
        Battle01TurnCompletion.RequireContinuingNoEffectState(current);
        var actor = current.Roster.Single(unit => unit.Index == actorIndex);
        RequireSupportOrigin(actor);
        if (!ReferenceEquals(actor.Stats, control.Movement.Range.EffectiveStatsAtEntry) ||
            actor.Position != control.Movement.Cursor || !control.Movement.Range.CanStopAt(actor.RequirePosition()) ||
            control.Movement.Attack is not null ||
            phase == Battle01Phase.PlayerActionChoice && control.Movement.Healing is not null)
            throw Rejected("control", "Retain Sarah's stats and confirmed provisional destination.");
        return control;
    }

    internal static void RequireSupportOrigin(Battle01Combatant actor)
    {
        var s = actor.Stats;
        if (actor.Index != 1 || actor.ClassId != 4 || actor.EnemySource is not null || actor.AiBitfield != 0 ||
            s.Level != 1 || s.HpMax != 11 || s.HpCurrent != 11 || s.MpMax != 10 || s.MpCurrent != 10 ||
            s.Attack != 9 || s.Defense != 5 || s.Agility != 5 || s.Move != 5 || s.Status != 0 ||
            s.CurrentExp != 0 || s.CurrentKills is not null || s.CurrentDefeats is not null ||
            !s.Items.SequenceEqual(new ushort[] { 213, 0, 0, 127 }) || !s.Spells.SequenceEqual(new byte[] { 0, 63, 63, 63 }))
            throw Rejected("supportInput", "Retain the explicit early Sarah EXP0 support input; physical combat is not admitted.");
    }

    internal static int[] Targets(Battle01InitializedState current, int actorIndex)
    {
        var origin = current.Roster.Single(unit => unit.Index == actorIndex).RequirePosition();
        return current.Roster.Where(unit => unit.Index < 128 && unit.Stats.HpCurrent > 0 &&
            unit.Position is { } p && Battle01Initialization.WithinArea(p) && current.OccupantAt(p) == unit.Index &&
            unit.AiBitfield is { } ai && (ai & 8) == 0 && Math.Abs(p.X - origin.X) + Math.Abs(p.Y - origin.Y) <= 1)
            .Select(unit => unit.Index).Order().ToArray();
    }

    private static void RequireSpellSelection(Battle01FirstControlState control)
    {
        if (control.Movement.Healing is not { SpellEntry: HealOne, Selected: 0, Targets.Count: 0 })
            throw Rejected("selection", "Retain the single HEAL 1 spell choice.");
    }

    private static Battle01PlayerHealingSelection RequireTargets(Battle01InitializedState current, Battle01FirstControlState control)
    {
        var selected = control.Movement.Healing;
        if (selected is null || selected.SpellEntry != HealOne || selected.TargetIndex is null ||
            !selected.Targets.SequenceEqual(Targets(current, control.ActorIndex)))
            throw Rejected("selection", "Retain every legal target and the selected index.");
        return selected;
    }

    private static Battle01InitializedState Select(Battle01InitializedState current, Battle01FirstControlState control,
        Battle01PlayerMovementStage stage, Battle01PlayerHealingSelection? selected) =>
        new(current, current.Roster.ToArray(), current.Occupancy,
            control.WithMovement(new(control.Movement.Range, control.Movement.Preview, stage, healing: selected)));

    // Accepted unpromoted PRST HEAL1 and same-side award; this scalar helper constructs no battle.
    internal static Battle01HealingEffect Resolve(Battle01Stats actor, Battle01Stats target, uint main)
    {
        if (actor.CurrentExp is not { } exp) throw Rejected("expInput", "Sarah EXP must be an explicit early input.");
        if (actor.MpCurrent < MpCost) throw Rejected("mp", "HEAL 1 requires three MP.");
        if (target.HpCurrent == 0 || target.HpCurrent >= target.HpMax) throw Rejected("hp", "This action heals a living injured Bowie.");
        int recovery = Math.Min(Power, target.HpMax - target.HpCurrent);
        int accumulated = Math.Min(25, Math.Max(10, 25 * recovery / target.HpMax));
        int award = accumulated; var rolls = new List<Battle01MainRandomRoll>();
        if (Battle01EnemyPhysicalAttack.MainRoll(ref main, rolls, "heal-exp-plus", 16) == 0) award++;
        if (Battle01EnemyPhysicalAttack.MainRoll(ref main, rolls, "heal-exp-minus", 16) == 0) award--;
        award = Math.Max(1, award);
        if (exp + award >= 100) throw Rejected("levelUp", "The healing slice does not implement level-up.");
        return new(recovery, accumulated, award, actor.WithCurrentMp((byte)(actor.MpCurrent - MpCost)).WithCurrentExp((byte)(exp + award)),
            target.WithCurrentHp((ushort)(target.HpCurrent + recovery)), rolls.AsReadOnly(),
            Array.AsReadOnly(new[] { new Battle01HealingReaction(1, 0, -MpCost, 0), new(0, recovery, 0, 0), new(1, 0, 0, award) }));
    }

    internal static void ValidateDecision(Battle01PlayerHealingDecision decision)
    {
        RequireSupportOrigin(decision.Actor); Battle01EnemyPhysicalAttack.RequireTargetProfile(decision.Target);
        if (decision.TargetIndex != 0 || decision.SpellEntry != HealOne ||
            !decision.LegalTargets.SequenceEqual(new[] { 0, 1 }) || decision.Actor.Position is not { } origin ||
            decision.Target.Position is not { } target || !Battle01Initialization.WithinArea(origin) ||
            !Battle01Initialization.WithinArea(target) || Math.Abs(origin.X - target.X) + Math.Abs(origin.Y - target.Y) != 1 ||
            decision.MoveString.Count == 0 || decision.MoveString[^1] != 255)
            throw Rejected("history", "Retain the distinct Sarah/Bowie HEAL 1 selection and placement.");
        var position = decision.MovementOrigin;
        foreach (byte direction in decision.MoveString.SkipLast(1))
        {
            (int x, int y) = direction switch { 0 => (1, 0), 1 => (0, -1), 2 => (-1, 0), 3 => (0, 1),
                _ => throw Rejected("history", "Retain the movement path.") };
            position = new(position.X + x, position.Y + y);
            if (!Battle01Initialization.WithinArea(position)) throw Rejected("history", "The path left the admitted area.");
        }
        if (position != origin) throw Rejected("history", "Cast range starts at the confirmed destination.");
        var expected = Resolve(decision.Actor.Stats, decision.Target.Stats, decision.MainSeedBefore);
        var actual = decision.Effect;
        if (actual.Recovery != expected.Recovery || actual.AccumulatedExp != expected.AccumulatedExp ||
            actual.AwardedExp != expected.AwardedExp || !actual.Rolls.SequenceEqual(expected.Rolls) ||
            !actual.Reactions.SequenceEqual(expected.Reactions) ||
            !Battle01EnemyPhysicalAttack.SameStats(actual.ActorAfterStats, expected.ActorAfterStats) ||
            !Battle01EnemyPhysicalAttack.SameStats(actual.TargetAfterStats, expected.TargetAfterStats))
            throw Rejected("history", "MP, HP, EXP and main RNG must replay the selected HEAL 1 input.");
    }

    private static ArgumentException Rejected(string field, string message) => new(message, "heal." + field);
}
