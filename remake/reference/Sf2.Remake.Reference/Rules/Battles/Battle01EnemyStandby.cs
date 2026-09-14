using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

public sealed record Battle01ThinkingRoll(byte Range, ushort BeforeSeedCopy, ushort AfterSeedCopy,
    byte Result, IReadOnlyList<byte> GeneratedBytes)
{
    public int GeneratorSteps => GeneratedBytes.Count;
}
public sealed record Battle01StandbyCandidate(byte Index, MapPosition Position, int? GridCost,
    int? Occupant, bool Eligible);
public sealed record Battle01EnemyStandbyDecision(int ActorIndex, MapPosition Origin, MapPosition Destination,
    ushort SeedCopyBefore, ushort SeedCopyAfter, byte MemoryBefore, byte MemoryAfter,
    IReadOnlyList<Battle01ThinkingRoll> Rolls, IReadOnlyList<Battle01StandbyCandidate> Candidates,
    IReadOnlyList<byte> MoveString)
{
    public byte Action => 3;
    public byte MovementType => 6;
}

public static class Battle01EnemyStandby
{
    private static readonly int[] FirstRoundActors = [1, 2, 128, 131, 133, 129, 130, 132, 0];
    private static readonly MapPosition[] EnemyOrigins = [new(7, 3), new(9, 4), new(6, 4), new(8, 3), new(9, 5), new(6, 5)];

    public static Battle01InitializedState CompleteFirst(Battle01InitializedState current, int actorIndex,
        Battle01StayCompletionPolicy? policy)
    {
        ArgumentNullException.ThrowIfNull(current);
        if (current.Phase != Battle01Phase.PlayerTurnCompleted ||
            current.FirstRound is not { RoundNumber: 1, CurrentTurnOffset: 4 } order ||
            current.TurnCompletion is not { CompletedActorIndex: 2, Previous.CompletedActorIndex: 1 } ||
            current.TurnCompletion.Previous.Previous is not null)
            throw new ArgumentException("First enemy standby requires the two completed player turns.", "phase");
        if (actorIndex != 128 || order.CurrentCandidate?.CombatantIndex != actorIndex)
            throw new ArgumentException("Only the actual first enemy candidate may enter standby.", "actor");
        if (current.RandomSeedCopy != 0x1234)
            throw new ArgumentException("The independent retained comparison seed-copy must be supplied.", "randomSeedCopy");
        if (current.RandomSeedImage != 0xA4991234 || current.NewlyTestedRegionMask != 7 ||
            current.RegionFlags90Through105.Count != 16 || current.RegionFlags90Through105.Any(flag => flag))
            throw new ArgumentException("The accepted round's RNG and inactive regions must be retained.", "round");
        if (current.AiMemory.Count != 48 || current.AiMemory.Any(value => value != 0) ||
            current.AiLastTargets.Count != 48 || current.AiLastTargets.Any(value => value != 255))
            throw new ArgumentException("The first enemy requires initialized standby memory and last targets.", "memory");
        return CompleteAdmittedEnemy(current, actorIndex, policy);
    }

    public static Battle01InitializedState CompleteNext(Battle01InitializedState current, int actorIndex,
        Battle01StayCompletionPolicy? policy)
    {
        ArgumentNullException.ThrowIfNull(current);
        var order = current.FirstRound;
        if (order is null) throw new ArgumentException("A current round is required.", "phase");
        if (order.RoundNumber == 1)
        {
            if (current.Phase != Battle01Phase.EnemyTurnCompleted || order.CurrentTurnOffset is < 6 or > 14)
                throw new ArgumentException("Remaining first-round standby requires a completed enemy.", "phase");
            RequireCompletedPrefix(current, order.CurrentTurnOffset / 2);
        }
        else
        {
            RequireCurrentRound(current);
        }
        if (order.CurrentCandidate?.CombatantIndex != actorIndex)
            throw new ArgumentException("Standby must name the actual current enemy.", "actor");
        return CompleteAdmittedEnemy(current, actorIndex, policy);
    }

    internal static void RequireCurrentRound(Battle01InitializedState current)
    {
        if (current.FirstRound is not { RoundNumber: > 1 } order ||
            current.Phase is not (Battle01Phase.RoundGenerated or Battle01Phase.PlayerTurnCompleted or Battle01Phase.EnemyTurnCompleted))
            throw new ArgumentException("Enemy control requires current generation or completed actor dispatch.", "phase");
        Battle01FirstRound.RequireCurrentPrefix(current);
        RequireThinkingHistory(current);
        bool enemyCompleted = false;
        for (var receipt = current.TurnCompletion; receipt?.RoundNumber == order.RoundNumber; receipt = receipt.Previous)
            enemyCompleted |= receipt.EnemyStandby is not null || receipt.EnemyPursuit is not null || receipt.EnemyPhysicalAttack is not null;
        if (current.NewlyTestedRegionMask != (enemyCompleted ? 0 : 7))
            throw new ArgumentException("Retain the current round's tested mask.", "round");
        Battle01FirstRound.RequireActivationState(current.Roster, current.RegionFlags90Through105, order.RoundNumber);
    }

    internal static void RequireCompletedPrefix(Battle01InitializedState current, int completedCount)
    {
        var order = current.FirstRound;
        if (completedCount is < 3 or > 8 || order is null || order.CurrentTurnOffset != completedCount * 2 ||
            order.Slots.Count != 64 || !order.Slots.Take(9).Select(slot => (int)slot.CombatantIndex).SequenceEqual(FirstRoundActors) ||
            order.Slots.Skip(9).Any(slot => !slot.IsSentinel))
            throw new ArgumentException("Only the retained first-round candidate sequence is admitted.", "turnOrder");
        var receipts = new Battle01TurnCompletionReceipt[completedCount]; var receipt = current.TurnCompletion;
        for (int index = completedCount - 1; index >= 0; index--)
        {
            if (receipt is null || receipt.CompletedActorIndex != FirstRoundActors[index] ||
                !ReferenceEquals(receipt.Policy, Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats) ||
                receipt.BeforeAfterTurn != new Battle01FactionCounts(3, 6) || receipt.AfterAfterTurn != receipt.BeforeAfterTurn)
                throw new ArgumentException("The complete ordered no-effect receipt prefix must be retained.", "completion");
            receipts[index] = receipt; receipt = receipt.Previous;
        }
        if (receipt is not null || receipts.Any(item => item.EnemyPursuit is not null || item.EnemyPhysicalAttack is not null || item.PlayerPhysicalAttack is not null) ||
            receipts.Take(2).Any(player => player.EnemyStandby is not null))
            throw new ArgumentException("The receipt prefix must start with exactly the two player turns.", "completion");
        ushort seed = 0x1234; var memory = new byte[48];
        foreach (var completed in receipts.Skip(2))
        {
            if (completed.EnemyStandby is not { } decision || decision.ActorIndex != completed.CompletedActorIndex ||
                decision.SeedCopyBefore != seed || decision.MemoryBefore != 0)
                throw new ArgumentException("Each completed enemy must preserve its own decision and preceding seed.", "completion");
            seed = decision.SeedCopyAfter; memory[decision.ActorIndex - 128] = decision.MemoryAfter;
        }
        if (current.RandomSeedCopy != seed)
            throw new ArgumentException("Continue from the last committed thinking seed-copy.", "randomSeedCopy");
        if (!current.AiMemory.SequenceEqual(memory) || current.AiLastTargets.Count != 48 || current.AiLastTargets.Any(value => value != 255))
            throw new ArgumentException("Completed enemy memory and initialized remaining slots must be preserved.", "memory");
        if (current.RandomSeedImage != 0xA4991234 || current.NewlyTestedRegionMask != 0 ||
            current.RegionFlags90Through105.Count != 16 || current.RegionFlags90Through105.Any(flag => flag))
            throw new ArgumentException("Retain the first-round main RNG, cleared tested mask and inactive flags.", "round");
    }

    internal static (uint? Gold, ushort? BowieKills, byte? ChesterExp, ushort? ChesterDefeats, ushort? BowieDefeats, ushort? ChesterKills, byte? SarahExp) RequireThinkingHistory(Battle01InitializedState current)
    {
        if (current.DefeatPending is not null)
            return RequireThinkingHistory(Battle01TurnCompletion.RequireDefeatPending(current));
        if (current.RandomSeedCopy is not { } seed)
            throw new ArgumentException("The current thinking seed-copy must be retained.", "randomSeedCopy");
        if (current.AiMemory.Count != 48 || current.AiLastTargets.Count != 48)
            throw new ArgumentException("Retain all memory and last-target slots.", "memory");
        var memory = current.AiMemory.ToArray();
        var targets = current.AiLastTargets.ToArray();
        var stats = current.Roster.ToDictionary(unit => unit.Index, unit => unit.Stats);
        var positions = current.Roster.ToDictionary(unit => unit.Index, unit => unit.Position);
        uint? gold = current.CurrentGold;
        int livingEnemies = current.Roster.Count(unit => unit.Index >= 128 && unit.Stats.HpCurrent > 0);
        int livingAllies = current.Roster.Count(unit => unit.Index < 128 && unit.Stats.HpCurrent > 0);
        var damaged = new HashSet<int>();
        uint main = current.RandomSeedImage;
        int mainRound = current.FirstRound?.RoundNumber ?? 0;
        bool mainAnchored = true;
        uint? followingGenerationMain = null;
        bool followingIsCurrent = false;
        int followingRound = 0;
        void RewindMain(uint before, uint after)
        {
            if (mainAnchored && main != after)
                throw new ArgumentException("Physical main RNG history must remain linked.", "attack.history");
            if (!mainAnchored && followingGenerationMain is { } generated)
                Battle01FirstRound.RequireGenerationFromRecordedMain(current, after, generated, followingIsCurrent, followingRound);
            main = before; mainAnchored = true; followingGenerationMain = null;
        }
        for (var receipt = current.TurnCompletion; receipt is not null; receipt = receipt.Previous)
        {
            if (!Battle01TurnCompletion.HasValidPolicy(receipt))
                throw new ArgumentException("Retain the completion kind's distinct policy.", "completion");
            if (receipt.BeforeAfterTurn != new Battle01FactionCounts(livingAllies, livingEnemies) || receipt.AfterAfterTurn != receipt.BeforeAfterTurn)
                throw new ArgumentException("Retain both faction counts at their historical death boundary.", "completion");
            if (receipt.RoundNumber != mainRound)
            {
                followingGenerationMain = mainAnchored && receipt.RoundNumber == mainRound - 1 ? main : null;
                followingIsCurrent = mainRound == current.FirstRound?.RoundNumber;
                followingRound = mainRound;
                mainRound = receipt.RoundNumber; mainAnchored = false;
            }
            if (receipt.CompletedActorIndex < 128)
            {
                if (receipt.EnemyStandby is not null || receipt.EnemyPursuit is not null || receipt.EnemyPhysicalAttack is not null)
                    throw new ArgumentException("Player receipts cannot carry an enemy decision.", "completion");
                if (receipt.PlayerHealing is { } healing)
                {
                    Battle01TurnCompletion.ValidateHealingReceipt(receipt);
                    if (seed != healing.SeedCopy || !Battle01EnemyPhysicalAttack.SameStats(stats[1], healing.Effect.ActorAfterStats) ||
                        !Battle01EnemyPhysicalAttack.SameStats(stats[0], healing.Effect.TargetAfterStats))
                        throw new ArgumentException("Healing MP, HP, EXP and seed copy must remain linked.", "heal.history");
                    RewindMain(healing.MainSeedBefore, healing.Effect.MainSeedAfter);
                    stats[1] = healing.Actor.Stats; stats[0] = healing.Target.Stats;
                    damaged.Add(1); damaged.Add(0);
                }
                if (receipt.PlayerPhysicalAttack is { } player)
                {
                    Battle01PlayerPhysicalAttack.ValidateDecision(player);
                    var actorAfter = player.ActorAfterStats;
                    if (player.DefeatedTarget)
                    {
                        Battle01TurnCompletion.ValidateDefeatReceipt(receipt);
                        if (positions[player.TargetIndex] is not null)
                            throw new ArgumentException("A cleaned enemy must remain unplaced.", "attack.history");
                        actorAfter = actorAfter.WithCurrentKills(receipt.EnemyDefeat!.KillsAfter);
                        positions[player.TargetIndex] = player.Target.Position;
                        livingEnemies++;
                    }
                    if (receipt.RoundNumber <= 1 || seed != player.SeedCopy ||
                        (mainAnchored && main != player.Effect.MainSeedAfter) ||
                        gold != player.GoldAfter ||
                        !Battle01EnemyPhysicalAttack.SameStats(stats[player.ActorIndex], actorAfter) ||
                        !Battle01EnemyPhysicalAttack.SameStats(stats[player.TargetIndex], player.Effect.AfterStats))
                        throw new ArgumentException("Player HP, EXP and both RNG channels must remain linked.", "attack.history");
                    RewindMain(player.MainSeedBefore, player.Effect.MainSeedAfter);
                    stats[player.ActorIndex] = player.Actor.Stats; stats[player.TargetIndex] = player.Effect.BeforeStats;
                    gold = player.GoldBefore;
                    damaged.Add(player.ActorIndex); damaged.Add(player.TargetIndex);
                }
                continue;
            }
            if (receipt.DefeatedTurnCompleted)
            {
                Battle01TurnCompletion.ValidateDefeatedTurnReceipt(receipt);
                var death = receipt.Previous!.Previous!.EnemyPhysicalAttack!;
                var kill = receipt.Previous.Previous.Previous!.PlayerPhysicalAttack!;
                if (positions[129] is not null || stats[129].HpCurrent != 0 ||
                    !Battle01EnemyPhysicalAttack.SameStats(stats[129], kill.Effect.AfterStats) ||
                    seed != death.SeedCopyAfter || gold != kill.GoldAfter)
                    throw new ArgumentException("The dead129 slot cannot change its earlier cleanup or accounting.", "deadTurn.history");
                RewindMain(death.MainSeedAfter, death.MainSeedAfter);
                continue;
            }
            if ((receipt.EnemyStandby is null ? 0 : 1) + (receipt.EnemyPursuit is null ? 0 : 1) +
                (receipt.EnemyPhysicalAttack is null ? 0 : 1) != 1)
                throw new ArgumentException("Each enemy receipt requires exactly one decision kind.", "completion");
            int actor; ushort before, after; byte memoryBefore, memoryAfter;
            if (receipt.EnemyPhysicalAttack is { } attack)
            {
                if (receipt.RoundNumber <= 1)
                    throw new ArgumentException("Physical decisions cannot replace first-round standby.", "completion");
                Battle01EnemyPhysicalAttack.ValidateDecision(attack, receipt.AllyDefeat is not null,
                    allowChesterCounter: ReferenceEquals(receipt.Policy, Battle01PhysicalCompletionPolicy.ControlledNonlethalChesterCounterAndExp),
                    allowBowieCounter: ReferenceEquals(receipt.Policy, Battle01PhysicalCompletionPolicy.ControlledNonlethalBowieCounterAndExp));
                if (attack.Counterattack is { } counter && current.TerrainAt(attack.Destination) != counter.TargetTerrain)
                    throw new ArgumentException("Retain the counter target's actual terrain at the enemy attack stop.", "attack.history");
                actor = attack.ActorIndex; before = attack.SeedCopyBefore; after = attack.SeedCopyAfter;
                memoryBefore = attack.Memory; memoryAfter = attack.Memory;
                var targetAfter = attack.TargetAfterStats;
                if (receipt.AllyDefeat is { } allyCleanup)
                {
                    Battle01TurnCompletion.ValidateAllyDefeatReceipt(receipt);
                    if (positions[attack.TargetIndex] is not null ||
                        receipt.BeforeAfterTurn != new Battle01FactionCounts(livingAllies, livingEnemies))
                        throw new ArgumentException("Retain the unique unplaced ally and its validated policy's prior enemy defeats.", "attack.history");
                    targetAfter = targetAfter.WithCurrentDefeats(allyCleanup.DefeatsAfter);
                    positions[attack.TargetIndex] = attack.Target.Position;
                    livingAllies++;
                }
                if ((mainAnchored && main != attack.MainSeedAfter) ||
                    targets[actor - 128] != attack.TargetIndex ||
                    !Battle01EnemyPhysicalAttack.SameStats(stats[actor], attack.ActorAfterStats) ||
                    !Battle01EnemyPhysicalAttack.SameStats(stats[attack.TargetIndex], targetAfter))
                    throw new ArgumentException("Physical main RNG, last target and HP history must remain linked.", "attack.history");
                RewindMain(attack.MainSeedBefore, attack.MainSeedAfter);
                targets[actor - 128] = attack.LastTargetBefore;
                stats[actor] = attack.Actor.Stats;
                if (attack.Counterattack is not null) damaged.Add(actor);
                stats[attack.TargetIndex] = attack.Effect.BeforeStats; damaged.Add(attack.TargetIndex);
            }
            else if (receipt.EnemyPursuit is { } pursuit)
            {
                actor = pursuit.ActorIndex; before = pursuit.SeedCopyBefore; after = pursuit.SeedCopyAfter;
                memoryBefore = pursuit.MemoryBefore; memoryAfter = pursuit.MemoryAfter;
                if (receipt.RoundNumber <= 1 || before != after || memoryBefore != memoryAfter ||
                    (mainAnchored && pursuit.MainSeedImage != main))
                    throw new ArgumentException("Pursuit must preserve its round's main RNG and independent thinking state.", "completion");
                var livingTargets = stats.Where(pair => pair.Key < 128 && pair.Value.HpCurrent > 0 && positions[pair.Key] is not null)
                    .Select(pair => pair.Key).Order().ToArray();
                if (!pursuit.TargetCosts.Select(target => target.ActorIndex).SequenceEqual(livingTargets) ||
                    !livingTargets.Contains(pursuit.TargetIndex))
                    throw new ArgumentException("Pursuit history must retain all and only living ally targets.", "pursuit.targets");
                RewindMain(pursuit.MainSeedImage, pursuit.MainSeedImage);
            }
            else
            {
                var decision = receipt.EnemyStandby!;
                actor = decision.ActorIndex; before = decision.SeedCopyBefore; after = decision.SeedCopyAfter;
                memoryBefore = decision.MemoryBefore; memoryAfter = decision.MemoryAfter;
            }
            int slot = actor - 128;
            if (slot is < 0 or >= 6 || actor != receipt.CompletedActorIndex)
                throw new ArgumentException("Enemy decision must identify its completed actor.", "completion");
            if (seed != after) throw new ArgumentException("Thinking seed-copy history must remain linked.", "randomSeedCopy");
            if (memory[slot] != memoryAfter) throw new ArgumentException("Each actor's retained memory must match its last decision.", "memory");
            seed = before; memory[slot] = memoryBefore;
        }
        if (seed != 0x1234) throw new ArgumentException("Thinking history must retain its supplied comparison origin.", "randomSeedCopy");
        if (memory.Any(value => value != 0)) throw new ArgumentException("Thinking history must retain initialized memory provenance.", "memory");
        if (targets.Any(value => value != 255))
            throw new ArgumentException("Last-target history must rewind to initialized empty slots.", "memory");
        if (gold is not (null or 0) || livingEnemies != 6 || livingAllies != 3)
            throw new ArgumentException("Gold and enemy deaths must rewind to their explicit initialization inputs.", "attack.history");
        foreach (var unit in current.Roster)
        {
            var original = stats[unit.Index];
            if (unit.Index >= 128)
                RequireRegularEnemy(unit.WithStats(original).WithPosition(positions[unit.Index]),
                    Math.Max(2, current.FirstRound?.RoundNumber ?? 2), (unit.AiBitfield & 1) != 0);
            else if (damaged.Contains(unit.Index) || original.CurrentExp is not null)
            {
                if (unit.Index == 1) Battle01PlayerHealing.RequireSupportOrigin(unit.WithStats(original));
                else Battle01EnemyPhysicalAttack.RequireTargetProfile(unit.WithStats(original));
                if (original.CurrentExp is not (null or 0))
                    throw new ArgumentException("Known player EXP must rewind to the declared zero input.", "attack.history");
            }
            if (original.HpCurrent != original.HpMax)
                throw new ArgumentException("Physical HP history must retain the initialized full-HP origin.", "attack.history");
            if (original.CurrentKills is not null && (unit.Index is not (0 or 2) || original.CurrentKills != 0))
                throw new ArgumentException("Known kills must rewind to the explicit Bowie/Chester zero input.", "attack.history");
            if (original.CurrentDefeats is not null && (unit.Index is not (0 or 2) || original.CurrentDefeats != 0))
                throw new ArgumentException("Known defeats must rewind to the explicit leader/Chester zero input.", "attack.history");
        }
        if (current.Roster.Any(unit => unit.Stats.HpCurrent == 0))
            Battle01TurnCompletion.RequireContinuingNoEffectState(current);
        return (gold, stats[0].CurrentKills, stats[2].CurrentExp, stats[2].CurrentDefeats, stats[0].CurrentDefeats, stats[2].CurrentKills, stats[1].CurrentExp);
    }

    private static Battle01InitializedState CompleteAdmittedEnemy(Battle01InitializedState current, int actorIndex,
        Battle01StayCompletionPolicy? policy)
    {
        var actor = current.Roster.SingleOrDefault(unit => unit.Index == actorIndex)
            ?? throw new ArgumentException("The current enemy must exist.", "actor");
        RequireRegularEnemy(actor, current.FirstRound!.RoundNumber, active: false);
        var decision = Decide(current, actor, current.RandomSeedCopy!.Value, current.AiMemory[actorIndex - 128]);
        var roster = current.Roster.ToArray(); var occupancy = current.Occupancy.ToArray();
        if (occupancy.Length != 2304 || occupancy[Battle01PlayerMovement.Offset(actor.RequirePosition())] != actorIndex ||
            (decision.Destination != actor.RequirePosition() && occupancy[Battle01PlayerMovement.Offset(decision.Destination)] != -1))
            throw new ArgumentException("Standby relocation must preserve consistent live occupancy.", "occupancy");
        roster[Array.IndexOf(roster, actor)] = actor.WithPosition(decision.Destination);
        occupancy[Battle01PlayerMovement.Offset(actor.RequirePosition())] = -1;
        occupancy[Battle01PlayerMovement.Offset(decision.Destination)] = actorIndex;
        var memory = current.AiMemory.ToArray(); memory[actorIndex - 128] = decision.MemoryAfter;
        var moved = new Battle01InitializedState(current, roster, occupancy, memory, decision.SeedCopyAfter);
        // Local projection only: source clear-region/standby/move/STAY effects commit together.
        return Battle01TurnCompletion.CompleteControlledStay(moved, actorIndex, actor.Stats, policy, decision);
    }

    internal static void RequireRegularEnemy(Battle01Combatant actor, int roundNumber, bool active, bool allowDefeated = false)
    {
        int actorIndex = actor.Index;
        int memoryIndex = actorIndex - 128;
        if (memoryIndex is < 0 or >= 6) throw new ArgumentException("Only the six initialized GIZMOs are admitted.", "actor");
        var deployment = actor.Deployment;
        if (deployment.Spawn != 0)
            throw new ArgumentException("Only the existing STARTING roster is admitted.", "spawn");
        byte commandSet = memoryIndex >= 4 ? (byte)7 : (byte)6;
        byte primaryRegion = memoryIndex < 3 ? (byte)2 : memoryIndex < 5 ? (byte)1 : (byte)0;
        if (deployment.Identity != 39 || deployment.AiCommandSet != commandSet ||
            deployment.PrimaryOrder != 255 || deployment.SecondaryOrder != 255 ||
            deployment.PrimaryRegion != primaryRegion || deployment.SecondaryRegion != 15 ||
            actor.AiBitfield != (0x2000 | (commandSet << 4) | (active ? 1 : 0)))
            throw new ArgumentException("Only the fixed GIZMO regular branch with matching activation is admitted.", "activation");
        if (deployment.Position != EnemyOrigins[memoryIndex] || (actor.Stats.HpCurrent == 0 ? !allowDefeated || actor.Position is not null : actor.Position is not { } livePosition || !Battle01Initialization.WithinArea(livePosition)) ||
            (roundNumber == 1 && actor.Position != deployment.Position))
            throw new ArgumentException("Retain the original standby anchor and a valid live position.", "position");
        var stats = actor.Stats;
        var source = actor.EnemySource; var baseline = source?.SourceStats;
        if (actor.ClassId is not null || source is null || source.DefinitionId != 39 || source.SourceUnknownByte != 39 ||
            source.SpellPowerMode != 0 || source.BaseResistance != 0x40E3 || source.BaseProwess != 0 ||
            source.BaseAiBitfield != 0x2000 || source.MovementType != 6 || baseline is null ||
            baseline.Level != 0 || baseline.HpMax != 5 || baseline.HpCurrent != 5 || baseline.MpMax != 0 || baseline.MpCurrent != 0 ||
            baseline.Attack != 7 || baseline.Defense != 5 || baseline.Agility != 5 || baseline.Move != 5 || baseline.Status != 0 ||
            baseline.CurrentExp is not null || baseline.CurrentKills is not null || baseline.Items.Any(item => item != 127) || baseline.Spells.Any(spell => spell != 63) ||
            stats.Level != 0 || stats.CurrentExp is not null || stats.CurrentKills is not null || stats.HpMax != 5 || (stats.HpCurrent > 5 || (stats.HpCurrent == 0 && !allowDefeated)) ||
            (roundNumber == 1 && stats.HpCurrent != 5) ||
            stats.MpMax != 0 || stats.MpCurrent != 0 || stats.Attack != 8 || stats.Defense != 5 ||
            stats.Agility != 5 || stats.Move != 5 || stats.Status != 0 ||
            stats.Items.Any(item => item != 127) || stats.Spells.Any(spell => spell != 63))
            throw new ArgumentException("The already initialized effective enemy stats must be unchanged.", "stats");
    }

    internal static Battle01EnemyStandbyDecision Decide(Battle01InitializedState battle, Battle01Combatant actor,
        ushort seedCopy, byte memory)
    {
        var d = actor.Deployment;
        try
        {
            var decision = AiStandbyRules.Decide(actor.RequirePosition(), d.Position, d.PrimaryOrder, d.SecondaryOrder,
                d.PrimaryRegion, d.SecondaryRegion, seedCopy, memory, BuildGrid,
                battle.Roster.Where(unit => unit.Stats.HpCurrent > 0 && unit.Position is not null)
                    .Select(unit => new StandbyOccupant<int>(unit.Index, unit.RequirePosition(), unit.AiBitfield)).ToArray(), 16, 20);
            return new(actor.Index, actor.RequirePosition(), decision.Destination, seedCopy, decision.SeedAfter, memory, decision.MemoryAfter,
                Array.AsReadOnly(decision.Rolls.Select(roll => new Battle01ThinkingRoll(roll.Range, roll.Before, roll.After, roll.Value, roll.Bytes)).ToArray()),
                Array.AsReadOnly(decision.Candidates.Select(candidate => new Battle01StandbyCandidate(candidate.Index, candidate.Position,
                    candidate.Cost, candidate.Occupant, candidate.Eligible)).ToArray()), decision.MoveString);
        }
        catch (BattleRuleException error)
        {
            string field = error.Code switch { "standby-move-order" => "moveOrder", "source-occupancy-word" => "activation", "standby-memory" => "memory", _ => "path" };
            throw new ArgumentException(error.Message, field, error);
        }
        WeightedMovementGrid BuildGrid()
        {
            if (actor.EnemySource?.MovementType != 6)
                throw new ArgumentException("Standby requires the admitted Hovering6 profile.", "movementProfile");
            return Battle01PlayerMovement.BuildWeightedGrid(battle.Terrain, Battle01PlayerMovement.HoveringCosts,
                Battle01PlayerMovement.Offset(actor.RequirePosition()), actor.Stats.Move * 2).Core;
        }
    }

    internal static Battle01ThinkingRoll ThinkingRoll(ushort seedCopy, byte range)
    {
        var draw = Sf2.Remake.Domain.Battles.BattleRandom.NextThinkingWord(seedCopy, range);
        return new(range, draw.Before, draw.After, draw.Value, draw.Bytes);
    }

    internal static IReadOnlyList<byte> SourceMoveString(Battle01MovementGrid grid, MapPosition origin, MapPosition destination)
    {
        try { return AiMovementRules.MoveString(grid.Core, origin, destination, 16, 20); }
        catch (BattleRuleException error) { throw new ArgumentException(error.Message, "path", error); }
    }

    internal static (MapPosition Destination, IReadOnlyList<byte> MoveString) SourceWalk(
        Battle01MovementGrid grid, MapPosition origin, int targetCost)
    {
        try { return AiMovementRules.Walk(grid.Core, origin, targetCost, 16, 20); }
        catch (BattleRuleException error) { throw new ArgumentException(error.Message, "path", error); }
    }
}
