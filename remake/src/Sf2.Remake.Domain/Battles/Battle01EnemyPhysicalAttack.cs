using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

public sealed record Battle01MainRandomRoll(string Purpose, ushort Range, uint BeforeImage, uint AfterImage, ushort Result);
public sealed record Battle01PhysicalTargetPriority(Battle01Combatant Target, Battle01AttackCandidate Candidate,
    int LandMultiplier, int PotentialDamage, int RemainingHp, int Priority, Battle01ThinkingRoll Roll);
public sealed record Battle01PhysicalReaction(int TargetIndex, int HpDelta, int MpDelta, ushort Status, byte Flags);
public sealed record Battle01PhysicalEffect(bool Dodged, bool Critical, int Damage, ushort TemporaryHp,
    ushort RestoredHp, Battle01Stats BeforeStats, Battle01Stats AfterStats,
    IReadOnlyList<Battle01MainRandomRoll> Rolls, Battle01PhysicalReaction? Reaction)
{
    public uint MainSeedAfter => Rolls[^1].AfterImage;
}
public sealed record Battle01EnemyPhysicalAttackDecision(Battle01Combatant Actor, MapPosition Destination,
    ushort SeedCopyBefore, ushort SeedCopyAfter, byte Memory, byte LastTargetBefore, uint MainSeedBefore,
    IReadOnlyList<Battle01PhysicalTargetPriority> Priorities, int TargetIndex, IReadOnlyList<byte> MoveString,
    Battle01PhysicalEffect Effect)
{
    public int ActorIndex => Actor.Index;
    internal bool DefeatedTarget => Effect.TemporaryHp == 0;
    internal Battle01Combatant Target => Priorities.Single(priority => priority.Target.Index == TargetIndex).Target;
    public byte Action => 0;
    // The original physical action writes the target WORD at offset2; offset6 is not its target.
    public ushort ItemOrSpellWord => (ushort)TargetIndex;
    public string CombatProfile => Battle01EnemyPhysicalAttack.RequireTargetProfile(
        Priorities.Single(priority => priority.Target.Index == TargetIndex).Target);
    public MapPosition Origin => Actor.RequirePosition();
    public int GridCost => Priorities.Single(p => p.Target.Index == TargetIndex).Candidate.GridCost;
}

public sealed class Battle01PhysicalAttackUnsupportedException : ArgumentException
{
    internal Battle01PhysicalAttackUnsupportedException(string boundary)
        : base("Ordinary physical attack unsupported (" + boundary + "); current turn retained.", "attack." + boundary) { }
}

public static class Battle01EnemyPhysicalAttack
{
    public static Battle01InitializedState CompleteNext(Battle01InitializedState current, int actorIndex,
        Battle01PhysicalCompletionPolicy? policy)
    {
        ArgumentNullException.ThrowIfNull(current);
        Battle01EnemyStandby.RequireCurrentRound(current);
        if (actorIndex < 128 || current.FirstRound!.CurrentCandidate?.CombatantIndex != actorIndex)
            throw new ArgumentException("Physical attack must name the actual current enemy.", "actor");
        if (!Battle01PhysicalCompletionPolicy.IsSupported(policy))
            throw new ArgumentException("The controlled physical policy must be explicit.", "policy");
        var actor = current.Roster.Single(unit => unit.Index == actorIndex);
        Battle01EnemyStandby.RequireRegularEnemy(actor, current.FirstRound.RoundNumber, active: true);
        Battle01TurnCompletion.RequireContinuingNoEffectState(current);
        if (current.Roster.Take(3).Any(unit => unit.AiBitfield != 0))
            throw new ArgumentException("Retain nonneutral controlled ally activation words.", "activation");
        if (actor.Prowess != 0 || actor.Resistance != 0x40E3)
            throw new Battle01PhysicalAttackUnsupportedException("actorProfile");

        var decision = Decide(current, actor, policy!.AllowsAllyDefeat, policy.AllowsLeaderDefeat);
        var roster = current.Roster.ToArray(); var occupancy = current.Occupancy.ToArray();
        int from = Battle01PlayerMovement.Offset(actor.RequirePosition()), to = Battle01PlayerMovement.Offset(decision.Destination);
        if (occupancy[from] != actorIndex || (from != to && occupancy[to] != -1))
            throw new ArgumentException("The physical attack destination must preserve occupancy.", "occupancy");
        roster[Array.IndexOf(roster, actor)] = actor.WithPosition(decision.Destination);
        int targetSlot = Array.FindIndex(roster, unit => unit.Index == decision.TargetIndex);
        roster[targetSlot] = roster[targetSlot].WithStats(decision.Effect.AfterStats);
        occupancy[from] = -1; occupancy[to] = actorIndex;
        var lastTargets = current.AiLastTargets.ToArray(); lastTargets[actorIndex - 128] = (byte)decision.TargetIndex;
        var replayed = new Battle01InitializedState(current, roster, occupancy, current.AiMemory.ToArray(),
            decision.SeedCopyAfter, decision.Effect.MainSeedAfter, lastTargets);
        if (decision.DefeatedTarget && decision.TargetIndex == 0)
            return Battle01TurnCompletion.CompleteLeaderDefeat(replayed, decision, policy);
        // The new startup preset does not relabel any earlier continuing receipt.
        return Battle01TurnCompletion.CompletePhysical(replayed, decision,
            policy.AllowsLeaderDefeat ? Battle01PhysicalCompletionPolicy.ControlledFirstAllyDefeat : policy);
    }

    internal static Battle01EnemyPhysicalAttackDecision Decide(Battle01InitializedState battle, Battle01Combatant actor,
        bool allowAllyDefeat = false, bool allowLeaderDefeat = false)
    {
        var (grid, candidates) = Battle01EnemyPursuit.PhysicalCandidates(battle, actor);
        if (candidates.Length == 0) throw new Battle01PhysicalAttackUnsupportedException("emptyCohort");
        ushort copy = battle.RandomSeedCopy!.Value;
        var priorities = new List<Battle01PhysicalTargetPriority>();
        // Source fills priorities from the end of the reachable-target array.
        foreach (var candidate in candidates.Reverse())
        {
            var target = battle.Roster.Single(unit => unit.Index == candidate.ActorIndex);
            int multiplier = LandMultiplier(target.ClassId, battle.TerrainAt(target.RequirePosition()));
            var roll = Battle01EnemyStandby.ThinkingRoll(copy, 3); copy = roll.AfterSeedCopy;
            int potential = LandDamage(actor.Stats.Attack, target.Stats.Defense, multiplier);
            int remaining = Math.Max(0, target.Stats.HpCurrent - potential);
            priorities.Add(new(target, candidate, multiplier, potential, remaining,
                Priority(candidate.GridCost, remaining, roll.Result), roll));
        }
        var selected = SelectTarget(priorities);
        RequireTargetProfile(selected.Target);
        var moves = Battle01EnemyStandby.SourceMoveString(grid, actor.RequirePosition(), selected.Candidate.AttackPosition);
        var effect = ResolveSingleStrike(actor.Stats.Attack, selected.Target.Stats, selected.LandMultiplier,
            battle.RandomSeedImage, selected.Target.Index, selected.Candidate.AttackPosition, selected.Target.RequirePosition(),
            32, 32, 1, (allowAllyDefeat && selected.Target.Index == 2) || (allowLeaderDefeat && selected.Target.Index == 0));
        return new(actor, selected.Candidate.AttackPosition, battle.RandomSeedCopy.Value, copy,
            battle.AiMemory[actor.Index - 128], battle.AiLastTargets[actor.Index - 128], battle.RandomSeedImage,
            priorities.AsReadOnly(), selected.Target.Index, moves, effect);
    }

    // Pinned priority script3: difficulty0/type2 is fixed by the admitted regular GIZMO words.
    internal static int Priority(int movement, int remainingHp, byte thinkingResult) =>
        thinkingResult == 0 ? (remainingHp == 0 ? 16 : 1) : Math.Max(19 - 2 * movement, 1);

    internal static Battle01PhysicalTargetPriority SelectTarget(IReadOnlyList<Battle01PhysicalTargetPriority> priorities)
    {
        int maximum = priorities.Max(p => p.Priority);
        var cohort = priorities.Where(p => p.Priority == maximum);
        // c834c652 table_AttackPriority_Flying ranks the admitted SDMN/PRST/KNTE classes 0/5/15.
        if (maximum >= 15)
        {
            int rank = cohort.Min(p => PriorityClassRank(p.Target.ClassId));
            cohort = cohort.Where(p => PriorityClassRank(p.Target.ClassId) == rank);
        }
        // Collection is reverse input order; signed cost max, later-collected target wins a tie.
        return cohort.Aggregate((best, next) => next.Candidate.GridCost >= best.Candidate.GridCost ? next : best);
    }

    private static int PriorityClassRank(byte? classId) => classId switch
    {
        0 => 0, 4 => 5, 1 => 15, _ => throw new Battle01PhysicalAttackUnsupportedException("priorityProfile")
    };
    private static int LandMultiplier(byte? classId, byte terrain)
    {
        _ = PriorityClassRank(classId);
        // Regular1/Centaur2/Healer12 land nibbles, independent of their different movement costs.
        return terrain switch
        {
            1 => 230, 2 => 256, >= 3 and <= 6 => 205,
            _ => throw new Battle01PhysicalAttackUnsupportedException("targetTerrain")
        };
    }
    internal static int LandDamage(int attack, int defense, int multiplier) => (Math.Max(1, attack - defense) * multiplier) >> 8;

    internal static string RequireTargetProfile(Battle01Combatant target)
    {
        var s = target.Stats;
        if (target.Index >= 128 || target.EnemySource is not null || s.Level != 1 || s.Status != 0)
            throw new Battle01PhysicalAttackUnsupportedException("targetProfile");
        // Both class profiles have prowess3; their equipped weapons change ATT only.
        if (target.ClassId == 0 && s.HpMax == 12 && s.MpMax == 8 && s.MpCurrent == 8 &&
            s.Attack == 9 && s.Defense == 4 && s.Agility == 4 && s.Move == 6 &&
            s.Items.SequenceEqual(new ushort[] { 199, 0, 127, 127 }) &&
            s.Spells.SequenceEqual(new byte[] { 10, 63, 63, 63 }))
            return "battle01-class0-wooden-sword-effective-prowess3-v1";
        if (target.Index == 2 && target.ClassId == 1 && s.HpMax == 11 && s.MpMax == 0 && s.MpCurrent == 0 &&
            s.Attack == 8 && s.Defense == 5 && s.Agility == 7 && s.Move == 7 &&
            s.CurrentExp is null or < 100 && s.CurrentKills is null &&
            s.Items.SequenceEqual(new ushort[] { 184, 0, 127, 127 }) &&
            s.Spells.SequenceEqual(new byte[] { 63, 63, 63, 63 }))
            return "battle01-class1-wooden-stick-effective-prowess3-v1";
        throw new Battle01PhysicalAttackUnsupportedException("targetProfile");
    }

    internal static Battle01PhysicalEffect Resolve(int attack, Battle01Stats target, int multiplier, uint main,
        int targetIndex, MapPosition attackerPosition, MapPosition targetPosition)
        => ResolveSingleStrike(attack, target, multiplier, main, targetIndex, attackerPosition, targetPosition, 32, 32, 1);

    internal static ushort MainRoll(ref uint main, List<Battle01MainRandomRoll> rolls, string purpose, ushort range)
    {
        uint before = main; ushort word = (ushort)(main >> 16);
        ushort result = Battle01FirstRound.NextRandom(ref word, range);
        main = ((uint)word << 16) | (main & 0xFFFF);
        rolls.Add(new(purpose, range, before, main, result)); return result;
    }

    // Only the two separately admitted physical roles call this arithmetic seam.
    internal static Battle01PhysicalEffect ResolveSingleStrike(int attack, Battle01Stats target, int multiplier, uint main,
        int targetIndex, MapPosition attackerPosition, MapPosition targetPosition,
        ushort dodgeRange, ushort criticalRange, int criticalShift, bool allowDefeat = false)
    {
        var rolls = new List<Battle01MainRandomRoll>();
        ushort Roll(string purpose, ushort range) => MainRoll(ref main, rolls, purpose, range);
        bool dodge = Roll("dodge", dodgeRange) == 0, critical = false;
        int damage = 0;
        if (!dodge)
        {
            damage = LandDamage(attack, target.Defense, multiplier);
            critical = Roll("critical", criticalRange) == 0;
            if (critical) damage += damage >> criticalShift;
            ushort spreadRange = (ushort)((damage >> 3) + 1);
            damage -= Roll("spread-1", spreadRange);
            damage -= Roll("spread-2", spreadRange);
            damage = Math.Max(1, damage);
        }
        int temporary = Math.Max(0, target.HpCurrent - damage);
        // Source death exits before double/counter rolls. No partial hit is committed.
        if (temporary == 0 && !allowDefeat) throw new Battle01PhysicalAttackUnsupportedException("lethal");
        bool doubleRolled = temporary > 0 && Roll("double", 32) == 0;
        bool counterRolled = temporary > 0 && Roll("counter", 32) == 0; // Death returns before both calls.
        // Admitted enemy->ally, no muddle/death/status/debug, ordinary first physical action.
        bool validDouble = doubleRolled && temporary > 0;
        bool validCounter = counterRolled && temporary > 0 && target.Status == 0 &&
            Math.Abs(attackerPosition.X - targetPosition.X) + Math.Abs(attackerPosition.Y - targetPosition.Y) == 1;
        if (validDouble) throw new Battle01PhysicalAttackUnsupportedException("double");
        if (validCounter) throw new Battle01PhysicalAttackUnsupportedException("counter");
        if (Math.Abs(attackerPosition.X - targetPosition.X) + Math.Abs(attackerPosition.Y - targetPosition.Y) != 1)
            throw new ArgumentException("The admitted physical target must be adjacent after movement.", "attack.range");
        // Local construction lowers HP, source end restores its snapshot, then reaction replay applies once.
        ushort restored = target.HpCurrent;
        var reaction = dodge ? null : new Battle01PhysicalReaction(targetIndex, -damage, 0, target.Status, 1);
        var after = target.WithCurrentHp((ushort)Math.Max(0, restored + (reaction?.HpDelta ?? 0)));
        return new(dodge, critical, damage, (ushort)temporary, restored, target, after, rolls.AsReadOnly(), reaction);
    }

    internal static void ValidateDecision(Battle01EnemyPhysicalAttackDecision decision, bool allowAllyDefeat = false, bool allowLeaderDefeat = false)
    {
        if (decision.ActorIndex is < 128 or > 133 || decision.Priorities.Count is < 1 or > 3 ||
            decision.Actor.Prowess != 0 || decision.Actor.Stats.Attack != 8 ||
            decision.Priorities.Select(p => p.Target.Index).Distinct().Count() != decision.Priorities.Count)
            throw new ArgumentException("Retain the physical actor and complete distinct target-priority inputs.", "attack.history");
        ushort copy = decision.SeedCopyBefore;
        foreach (var p in decision.Priorities)
        {
            var roll = Battle01EnemyStandby.ThinkingRoll(copy, 3);
            int potential = LandDamage(decision.Actor.Stats.Attack, p.Target.Stats.Defense, p.LandMultiplier);
            int remaining = Math.Max(0, p.Target.Stats.HpCurrent - potential);
            if (p.Candidate.ActorIndex != p.Target.Index || p.Candidate.GridCost is < 0 or > 10 ||
                p.LandMultiplier is not (256 or 230 or 205) || p.PotentialDamage != potential || p.RemainingHp != remaining ||
                p.Priority != Priority(p.Candidate.GridCost, remaining, roll.Result) ||
                p.Roll.Range != 3 || p.Roll.BeforeSeedCopy != copy || p.Roll.AfterSeedCopy != roll.AfterSeedCopy ||
                p.Roll.Result != roll.Result || !p.Roll.GeneratedBytes.SequenceEqual(roll.GeneratedBytes))
                throw new ArgumentException("Physical priority/thinking history must reproduce its input.", "attack.history");
            copy = roll.AfterSeedCopy;
        }
        var selected = SelectTarget(decision.Priorities); RequireTargetProfile(selected.Target);
        if (copy != decision.SeedCopyAfter || selected.Target.Index != decision.TargetIndex ||
            selected.Candidate.AttackPosition != decision.Destination ||
            decision.MoveString.Count == 0 || decision.MoveString[^1] != 255 ||
            (decision.MoveString.Count - 1) * 2 != selected.Candidate.GridCost)
            throw new ArgumentException("Retain the selected physical target, movement and thinking endpoint.", "attack.history");
        var position = decision.Origin;
        foreach (byte d in decision.MoveString.SkipLast(1))
        {
            (int x, int y) = d switch { 0 => (1, 0), 1 => (0, -1), 2 => (-1, 0), 3 => (0, 1),
                _ => throw new ArgumentException("Invalid physical move string.", "attack.history") };
            position = new(position.X + x, position.Y + y);
            if (!Battle01Initialization.WithinArea(position)) throw new ArgumentException("Physical path left the area.", "attack.history");
        }
        if (position != decision.Destination) throw new ArgumentException("Physical path endpoint differs.", "attack.history");
        var expected = ResolveSingleStrike(decision.Actor.Stats.Attack, selected.Target.Stats, selected.LandMultiplier,
            decision.MainSeedBefore, selected.Target.Index, decision.Destination, selected.Target.RequirePosition(),
            32, 32, 1, (allowAllyDefeat && selected.Target.Index == 2) || (allowLeaderDefeat && selected.Target.Index == 0));
        var actual = decision.Effect;
        if (!ReferenceEquals(actual.BeforeStats, selected.Target.Stats) ||
            actual.Dodged != expected.Dodged || actual.Critical != expected.Critical || actual.Damage != expected.Damage ||
            actual.TemporaryHp != expected.TemporaryHp || actual.RestoredHp != expected.RestoredHp ||
            actual.Reaction != expected.Reaction || !actual.Rolls.SequenceEqual(expected.Rolls) ||
            !SameStats(actual.AfterStats, expected.AfterStats))
            throw new ArgumentException("Physical HP/reaction and main RNG history must reproduce its input.", "attack.history");
    }

    internal static bool SameStats(Battle01Stats a, Battle01Stats b) =>
        a.Level == b.Level && a.HpMax == b.HpMax && a.HpCurrent == b.HpCurrent && a.MpMax == b.MpMax && a.MpCurrent == b.MpCurrent &&
        a.Attack == b.Attack && a.Defense == b.Defense && a.Agility == b.Agility && a.Move == b.Move && a.Status == b.Status &&
        a.CurrentExp == b.CurrentExp && a.CurrentKills == b.CurrentKills && a.CurrentDefeats == b.CurrentDefeats &&
        a.Items.SequenceEqual(b.Items) && a.Spells.SequenceEqual(b.Spells);
}
