using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class BirdmanTargetPriorityTests
{
    // Zero-based positions from the pinned US aipriority.asm, including all
    // previously admitted unpromoted classes in source class-ID order.
    [Theory]
    [InlineData(BattleMover.Regular, 8, new[] { 0, 22, 25, 2, 5, 15, 8, 20, 16, 9, 13, 30 })]
    [InlineData(BattleMover.Flying, 20, new[] { 0, 15, 8, 2, 5, 27, 20, 13, 28, 21, 25, 18 })]
    [InlineData(BattleMover.Archer, 0, new[] { 11, 22, 25, 5, 8, 15, 0, 20, 16, 1, 13, 30 })]
    [InlineData(BattleMover.Mage, 28, new[] { 0, 10, 5, 21, 24, 15, 28, 12, 14, 27, 19, 2 })]
    public void BirdmanCriticalTiesKeepEveryAdmittedClassRelationship(BattleMover mover, int rank, int[] ranks)
    {
        var table = PhysicalTargetRules.Table(mover);
        byte? birdman = Sf2ClassRules.SourceClass(BattleClassRule.UnpromotedBirdman);
        Assert.Equal((byte)6, birdman);
        Assert.Equal(rank, PhysicalTargetRules.ClassRank(birdman, table));
        for (byte peer = 0; peer < ranks.Length; peer++)
        {
            Assert.Equal(ranks[peer], PhysicalTargetRules.ClassRank(peer, table));
            if (peer == birdman) continue;
            foreach (byte priority in new byte[] { 15, 16 })
            {
                // Class wins even when movement would prefer the other candidate.
                PhysicalTargetPriority[] pair = [new(0, priority, birdman), new(10, priority, peer)];
                int expected = rank < ranks[peer] ? 0 : 1;
                Assert.Equal(expected, PhysicalTargetRules.Select(pair, table)!.Value.Index);
                Array.Reverse(pair);
                Assert.Equal(1 - expected, PhysicalTargetRules.Select(pair, table)!.Value.Index);
            }
        }
    }

    [Theory]
    [InlineData(BattleMover.Regular, true)]
    [InlineData(BattleMover.Flying, true)]
    [InlineData(BattleMover.Hovering, true)]
    [InlineData(BattleMover.Archer, true)]
    [InlineData(BattleMover.CentaurArcher, true)]
    [InlineData(BattleMover.Mage, false)]
    [InlineData(BattleMover.StealthArcher, false)]
    public void OrdinarySessionScoresBothCandidatesAndAttacksThePreferredClass(BattleMover mover, bool birdmanWins)
    {
        foreach (bool swap in new[] { false, true })
        {
            var document = Document("stone-court");
            document["start"]!["mainSeed"] = (55u << 16) | 0x1234u;
            document["start"]!["thinkingSeed"] = 0x00EF0042u;
            for (int i = 0; i < 3; i++)
            {
                document["start"]!["actors"]![i]!["hp"] = 500;
                document["actors"]![i]!["maxHp"] = 500;
                document["actors"]![i]!["defense"] = 4;
                document["actors"]![i]!["attack"] = i == 2 ? 30 : 18;
            }
            document["actors"]![2]!["move"] = 1;
            var placements = document["encounters"]![0]!["placements"]!;
            placements[2]!["aiStrategy"] = "attack-then-approach";
            placements[1]!["x"] = 1; placements[1]!["y"] = 3;
            var read = Reader(document).Read();
            Assert.True(read is ScenarioReadAccepted, (read as ScenarioReadRejected)?.Failure.ToString());
            var admitted = (ScenarioReadAccepted)read;
            var encounter = admitted.Definition.Encounters.Values.Single();
            // Bind through the typed content boundary used by private readers;
            // the public authored JSON vocabulary has a narrower class/mover set.
            var deployments = encounter.Deployments.Select(row =>
            {
                var actor = row.Definition;
                if (row.Faction != BattleFaction.Ally && row.Actor != new ActorRef("raider")) return row;
                var rule = row.Faction != BattleFaction.Ally ? actor.ClassRule :
                    (row.Actor == new ActorRef("swordsman")) != swap
                        ? BattleClassRule.UnpromotedBirdman : BattleClassRule.UnpromotedPhoenix;
                var movement = row.Faction == BattleFaction.Enemy ? mover : actor.Mover;
                return row with { Definition = new(actor.Actor, rule, actor.Level, actor.MaxHp, actor.MaxMp,
                    actor.Attack, actor.Defense, actor.Agility, actor.ExtraRoundAction, actor.Move,
                    actor.Spells, actor.Physical, movement, actor.SourceLoadout) };
            });
            var definition = new ScenarioDefinition("birdman-ties", [encounter.WithDeployments(deployments)]);
            var started = GameSession.Start(definition, admitted.Start);
            Assert.True(started is SessionStarted, (started as SessionStartFailed)?.Failure.ToString());
            var session = ((SessionStarted)started).Session;
            Assert.True(session.Current.HasBattleControl);
            var result = FinishBattleScenes(session, Stay(session));
            var expected = new ActorRef(birdmanWins != swap ? "swordsman" : "lookout");
            Assert.Equal(expected, Assert.Single(result.Observations, row => row.Kind == "ai-target").Target);
            Assert.Equal(new long?[] { 19, 19 }, result.Observations.Where(row => row.Kind == "ai-candidate").Select(row => row.After));
            Assert.True(result.Snapshot.Battle.GetActor(expected).Hp < 500);
            var untouched = new ActorRef(expected == new ActorRef("swordsman") ? "lookout" : "swordsman");
            Assert.Equal(500, result.Snapshot.Battle.GetActor(untouched).Hp);
            Assert.True(result.Snapshot.HasBattleControl);
        }
    }

    [Theory]
    [InlineData(BattleMover.Regular)]
    [InlineData(BattleMover.Flying)]
    [InlineData(BattleMover.Archer)]
    [InlineData(BattleMover.Mage)]
    public void SourcePursuitUsesBirdmanRankWithoutReplacingTheAdjacentPass(BattleMover mover)
    {
        foreach (var peer in new[] { BattleClassRule.UnpromotedPhoenix, BattleClassRule.UnpromotedSwordsman })
        foreach (int gap in new[] { 1, 4 })
        foreach (bool swap in new[] { false, true })
        {
            var battle = PursuitState(mover, peer, gap, swap);
            var result = SourceEnemyAi.Resolve(battle, new("pursuer"));
            bool exchange = peer == BattleClassRule.UnpromotedPhoenix && gap == 1;
            var target = Assert.Single(result.Effects, row => row.Kind == "ai-move-target");
            Assert.Equal(new ActorRef(exchange ? "east" : "west"), target.Target);
            Assert.Equal(255, target.Before); // Both targets are outside the order's scoring radius.
            Assert.Equal(new MapPosition(exchange ? 12 : 8, 10), result.Destination);
            Assert.Equal(new MapPosition(10, 10), result.Path![0]);
            Assert.Equal(result.Destination, result.Path[^1]);
            Assert.Equal(battle.MainSeed, result.Battle.MainSeed);
            Assert.Equal(battle.ThinkingSeed, result.Battle.ThinkingSeed);
            Assert.Equal(new MapPosition(10, 10), battle.GetActor(new("pursuer")).Position);
        }
    }

    private static EngineBattleState PursuitState(BattleMover mover, BattleClassRule peer, int gap, bool swap)
    {
        BattleActorDefinition Unit(string id, BattleClassRule rule, BattleMover movement = BattleMover.Regular) =>
            new(new(id), rule, 1, 20, 0, 10, 4, 5, false, 4, [],
                physical: new(PhysicalCriticalRule.OneIn32WithHalfBonus, false, false, 0), mover: movement,
                sourceLoadout: new([127, 127, 127, 127], [63, 63, 63, 63]));
        var origin = new MapPosition(10, 10);
        BattleDeploymentDefinition[] deployments = [
            new(Unit("pursuer", BattleClassRule.Ordinary, mover), BattleFaction.Enemy, 128,
                BattleControl.Automatic, new("source-orders"), origin, new(0x2020, 0, 15, 15, 0, 14, 64, 255)),
            new(Unit("west", swap ? peer : BattleClassRule.UnpromotedBirdman), BattleFaction.Ally, 0,
                BattleControl.Player, null, new(4, 10), new(null, 0, 0, 0, 0, 0, 255, 255)),
            new(Unit("east", swap ? BattleClassRule.UnpromotedBirdman : peer), BattleFaction.Ally, gap,
                BattleControl.Player, null, new(16, 10), new(null, 0, 0, 0, 0, 0, 255, 255))];
        var definition = new BattleDefinition("birdman-pursuit", new("field"), 20, 20,
            Enumerable.Range(0, 2304).Select(i => new BattleTerrain(
                i % 48 < 20 && i / 48 < 20 ? TerrainSurface.Open : TerrainSurface.Barrier, TerrainProtection.None)),
            deployments, [], rewards: new(false), initialization: new([], BattleRegionProgram.None, aiPoints: [origin],
                sourceCombatants: deployments.ToDictionary(row => row.ProcessingOrder, row => row.Actor)));
        return new(definition, deployments.Select(row => new BattleActorState(row, 20, 0, 0, row.Position, 0, 0,
            activationWord: row.Faction == BattleFaction.Ally ? (ushort)0 : (ushort)0x2020)),
            0x1234, 0x5678, 1, [], 0, 0, regions: new(new bool[16], 0));
    }
}
