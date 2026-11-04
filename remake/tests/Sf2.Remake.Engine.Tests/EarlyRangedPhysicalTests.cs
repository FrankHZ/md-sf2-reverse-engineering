using System.Text.Json;
using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class EarlyRangedPhysicalTests
{
    private static readonly ActorRef Lancer = new("lancer"), Sentinel = new("sentinel");

    [Theory]
    [InlineData(1, 1, 1, true)] [InlineData(1, 1, 2, false)]
    [InlineData(1, 2, 1, true)] [InlineData(1, 2, 2, true)] [InlineData(1, 2, 3, false)]
    [InlineData(2, 2, 1, false)] [InlineData(2, 2, 2, true)] [InlineData(2, 2, 3, false)]
    public void PlayerQueryAndConfirmationUseLiveRangeAndRejectAtomically(int minimum, int maximum, int distance, bool legal)
    {
        var session = Start("early-ranged", d =>
        {
            d["weapons"]![0]!["minimumRange"] = minimum; d["weapons"]![0]!["maximumRange"] = maximum;
            d["encounters"]![0]!["placements"]![3]!["x"] = 2 + distance;
        });
        Accept(session, new Confirm());
        var offer = session.QueryBattleChoices().Actions.Single(a => a.Action.Kind == BattleActionKind.Physical);
        Assert.Equal(minimum, offer.MinimumRange!.Value); Assert.Equal(maximum, offer.MaximumRange!.Value);
        Assert.Equal(legal, offer.Targets.Single(t => t.Actor == Sentinel).Enabled);
        Accept(session, new ChooseAction(SessionAction.PhysicalAttack));
        var before = session.Current;
        var selected = Send(session, new SelectTarget(Sentinel));
        if (!legal)
        {
            Assert.Equal("target-range", selected.Failure!.Code); Assert.Same(before, session.Current);
            return;
        }
        Assert.Null(selected.Failure);
        var result = FinishBattleScenes(session, Accept(session, new Confirm()));
        Assert.Null(result.Failure); Assert.True(session.Current.Battle.GetActor(Sentinel).Hp < 200);
        Assert.True(session.Current.HasBattleControl); Assert.Equal(new ActorRef("archer"), session.Current.Selection!.Actor);
    }

    [Fact]
    public void MovementAndEquipmentRefreshRangeAndAttackOnceWithoutSpendingTheTurnOrRng()
    {
        var session = Start("early-ranged"); var initial = session.Current.Battle;
        FinishMovement(session, Accept(session, new Move(ExplorationDirection.West)));
        Accept(session, new Confirm());
        Assert.False(AttackOffer().Targets.Single(t => t.Actor == Sentinel).Enabled);
        FinishMovement(session, Accept(session, new Cancel()));
        FinishMovement(session, Accept(session, new Move(ExplorationDirection.East)));
        Accept(session, new Confirm());
        Assert.True(AttackOffer().Targets.Single(t => t.Actor == Sentinel).Enabled);
        foreach (var (slot, attack, maximum, word) in new[] { (1, 29, 1, 72), (4, 20, 1, 72), (0, 26, 2, 200), (0, 26, 2, 200) })
        {
            Accept(session, new BeginEquipment()); Accept(session, new SelectEquipment(slot));
            var selected = session.Current;
            var envelope = new CommandEnvelope(selected.SessionId, selected.Revision, Lancer, new Confirm());
            Assert.Null(session.Submit(envelope).Failure);
            var committed = session.Current;
            Assert.Equal("stale-input", session.Submit(envelope).Failure!.Code);
            Assert.Same(committed, session.Current);
            var actor = committed.Battle.GetActor(Lancer);
            Assert.Equal(attack, actor.Attack); Assert.Equal(20, actor.BaseAttack);
            Assert.Equal(word, actor.SourceLoadout!.Items[0]); Assert.Equal(maximum, AttackOffer().MaximumRange!.Value);
            Assert.Equal(initial.MainSeed, committed.Battle.MainSeed); Assert.Equal(initial.ThinkingSeed, committed.Battle.ThinkingSeed);
            Assert.Equal(initial.Gold, committed.Battle.Gold); Assert.Equal(initial.Cursor, committed.Battle.Cursor);
            Assert.Equal(initial.GetActor(Lancer).Exp, actor.Exp); Assert.Equal(initial.GetActor(Lancer).Level, actor.Level);
        }
        var before = session.Current;
        Accept(session, new BeginEquipment());
        var invalidBefore = session.Current;
        Assert.Equal("equipment-slot", Send(session, new SelectEquipment(3)).Failure!.Code);
        Assert.Same(invalidBefore, session.Current);

        Sf2.Remake.Application.Runtime.Battles.BattleActionChoice AttackOffer() =>
            session.QueryBattleChoices().Actions.Single(a => a.Action.Kind == BattleActionKind.Physical);
    }

    [Theory]
    [InlineData(1, 1, 3)] [InlineData(1, 2, 4)] [InlineData(2, 2, 4)]
    public void EnemyUsesTheSameActionRangeAndKeepsOriginWhenAlreadyInRange(int minimum, int maximum, int expectedX)
    {
        var before = AutomaticBattle(minimum, maximum);
        var decision = SourceEnemyAi.Resolve(before, Sentinel);
        Assert.Equal(Lancer, decision.Target); Assert.Equal(new MapPosition(expectedX, 2), decision.Destination);
        Assert.Equal(before.MainSeed, decision.Battle.MainSeed); Assert.Equal(before.Gold, decision.Battle.Gold);
        var action = PhysicalBattleAction.PrepareSourceDefault(decision.Battle, Sentinel, decision.Destination, Lancer);
        Assert.Equal(Lancer, action.Reactions[0].Target);
        Assert.True(BattleRange.Contains(decision.Destination, before.GetActor(Lancer).Position!, minimum, maximum));
    }

    [Theory]
    [InlineData(6)] [InlineData(7)] [InlineData(9)] [InlineData(14)]
    public void RangedSourceCommandsetsRetainTheirOrderedPhysicalDispatch(int commandset)
    {
        var decision = SourceEnemyAi.Resolve(AutomaticBattle(2, 2, (byte)commandset), Sentinel);
        Assert.Equal(Lancer, decision.Target); Assert.Equal(new MapPosition(4, 2), decision.Destination);
        string command = commandset == 9 ? "ai-command-attack3" : commandset == 14 ? "ai-command-attack2" : "ai-command-attack1";
        Assert.Contains(decision.Effects, e => e.Kind == command && e.After == 0);
        Assert.DoesNotContain(decision.Effects, e => e.Kind == "ai-command-heal1");
    }

    [Fact]
    public void BowMovesAwayFromAdjacentTargetAndCannotStopOnOccupiedOrBlockedTiles()
    {
        var battle = AutomaticBattle(2, 2);
        battle = battle.With(actors: battle.Actors.Select(a => a.Actor == Lancer ? a.With(position: new(3, 2)) : a));
        var decision = SourceEnemyAi.Resolve(battle, Sentinel);
        Assert.Equal(Lancer, decision.Target);
        Assert.NotEqual(new MapPosition(4, 2), decision.Destination);
        Assert.True(BattleRange.Contains(decision.Destination, new(3, 2), 2, 2));
        BattleMovement.RequireStop(battle, Sentinel, decision.Destination);
        // A source row-major diamond scan skips both the blocked terrain and an occupied cheapest candidate.
        var costs = Enumerable.Repeat((sbyte)-1, 2304).ToArray();
        costs[2 * 48 + 4] = 2; costs[1 * 48 + 4] = 2; costs[3 * 48 + 4] = 2;
        var grid = WeightedMovement.Build(costs, 2 * 48 + 4, 4);
        var station = AiMovementRules.AttackPosition(grid, new(3, 2), 2, 2, p => p == new MapPosition(4, 1));
        Assert.Equal(new MapPosition(4, 3), station);
    }

    [Fact]
    public void MissingReachableTargetFallsBackWithoutPhysicalOrMainRngAndNoOpponentStays()
    {
        var battle = AutomaticBattle(2, 2);
        battle = battle.With(actors: battle.Actors.Select(a => a.Actor == Lancer ? a.With(position: new(1, 5)) : a));
        var decision = SourceEnemyAi.Resolve(battle, Sentinel);
        Assert.Null(decision.Target); Assert.DoesNotContain(decision.Effects, e => e.Kind == "thinking-rng");
        Assert.Contains(decision.Effects, e => e.Kind == "ai-command-attack1" && e.After == -1);
        Assert.Equal(battle.MainSeed, decision.Battle.MainSeed); Assert.Equal(battle.ThinkingSeed, decision.Battle.ThinkingSeed);
        var empty = battle.With(actors: battle.Actors.Select(a => a.IsAlly ? a.With(hp: 0) : a));
        var idle = SourceEnemyAi.Resolve(empty, Sentinel);
        Assert.Null(idle.Target); Assert.Equal(empty.GetActor(Sentinel).Position, idle.Destination);
        Assert.Equal(empty.ThinkingSeed, idle.Battle.ThinkingSeed);
    }

    [Theory]
    [InlineData(256, false)] [InlineData(127, true)]
    public void UnknownAiItemOrSpellCannotSilentlyFallThroughAndLeavesStateUnchanged(int item, bool spell)
    {
        var battle = AutomaticBattle(2, 2);
        battle = battle.With(actors: battle.Actors.Select(a => a.Actor == Sentinel ? a.With(sourceLoadout:
            new([173, (ushort)item, 127, 127], spell ? [0, 63, 63, 63] : [63, 63, 63, 63])) : a));
        string before = JsonSerializer.Serialize(battle);
        Assert.Throws<BattleRuleException>(() => SourceEnemyAi.Resolve(battle, Sentinel));
        Assert.Equal(before, JsonSerializer.Serialize(battle));
    }

    [Theory]
    [InlineData(BattleMover.Archer, 11, 5, 8, 15, 22, 25)]
    [InlineData(BattleMover.Mage, 0, 21, 24, 15, 10, 5)]
    public void NewMoverPriorityUsesSourceTableRatherThanRegularTable(BattleMover mover, int swordsman, int mage, int priest, int archer, int knight, int warrior)
    {
        var table = PhysicalTargetRules.Table(mover);
        Assert.Equal(new[] { swordsman, mage, priest, archer, knight, warrior },
            new byte[] { 0, 3, 4, 5, 1, 2 }.Select(c => PhysicalTargetRules.ClassRank(c, table)));
        foreach (var surface in Enum.GetValues<TerrainSurface>())
            Assert.Equal(BattleTerrainRules.MovementCost(new(surface, TerrainProtection.None)),
                BattleTerrainRules.MovementCost(new(surface, TerrainProtection.None), mover));
    }

    [Fact]
    public void ThiefKnifeUsesItsSourceDoubleChanceInActualAttackWithoutChangingCounterChance()
    {
        var session = Start("early-ranged", d =>
        {
            d["actors"]![0]!["classRule"] = "unpromoted-thief";
            d["actors"]![0]!["physical"]!["critical"] = new JsonObject
                { ["chance"] = "one-in-16", ["damageBonus"] = "quarter", ["doubleChance"] = "one-in-16" };
            d["weapons"]![0]!["id"] = 103; d["weapons"]![0]!["classMask"] = 1024; d["weapons"]![0]!["maximumRange"] = 1;
            d["weapons"]![0]!["attackBonus"] = 5; d["actors"]![0]!["items"]![0] = 231; d["actors"]![0]!["attack"] = 25;
            d["encounters"]![0]!["placements"]![3]!["x"] = 3;
        });
        var before = session.Current.Battle;
        var action = PhysicalBattleAction.PrepareSourceDefault(before, Lancer, new(2, 2), Sentinel);
        Assert.Contains(action.ConstructionEffects, e => e.Kind == "rng-double" && e.Actor == Lancer && e.RandomRange == 16);
        Assert.All(action.ConstructionEffects.Where(e => e.Kind == "rng-counter"), e => Assert.Equal((ushort)32, e.RandomRange));
        Assert.Equal(10, Sf2ClassRules.SourceClass(before.GetActor(Lancer).Definition.ClassRule)!.Value);
        Assert.Equal(before.MainSeed, session.Current.Battle.MainSeed);
    }

    private static EngineBattleState AutomaticBattle(int minimum, int maximum, byte commandset = 6)
    {
        var session = Start("early-ranged", d =>
        { d["weapons"]![2]!["minimumRange"] = minimum; d["weapons"]![2]!["maximumRange"] = maximum; });
        var source = session.Current.Battle;
        var deployments = source.Definition.Deployments.Select(d => d.Actor == Sentinel ? d with
            { Initialization = new(0x2000, 0, 15, 15, 0, commandset, 255, 255), AiStrategy = new("source-orders") } : d).ToArray();
        var definition = new BattleDefinition("range-state", source.Definition.Map, 9, 7, source.Definition.Terrain, deployments, [],
            rewards: new(false), initialization: new([], BattleRegionProgram.None, sourceCombatants: deployments.ToDictionary(d => d.ProcessingOrder, d => d.Actor)));
        var actors = deployments.Select(d => new BattleActorState(d, (ushort)(d.Actor == Sentinel || d.Actor == Lancer ? 200 : 0), 0, 0,
            d.Position, 0, 0, activationWord: (ushort)(d.Actor == Sentinel ? 0x2001 : 0)));
        return new(definition, actors, 0x12341234, 0x12340042, 1, [], 0, 100, regions: new(new bool[16], 0));
    }
}
