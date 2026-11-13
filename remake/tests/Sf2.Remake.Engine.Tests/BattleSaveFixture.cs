using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Gameplay.Sf2;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

internal static class BattleSaveFixture
{
    internal static (ScenarioDefinition Definition, SessionSnapshot Current) Create(uint main = 0x1234ABCD, uint thinking = 0x80ABCDEF, string encounterId = "battle01")
    {
        var (basis, fieldData) = OpeningRestoreTests.Fixture(true);
        var field = Assert.IsType<SessionStarted>(GameSession.Restore(basis, fieldData, RuleCompositions.Sf2())).Session.Current;
        var ally = basis.Encounters["opening"].Deployments[0];
        var physical = new PhysicalActorDefinition(PhysicalCriticalRule.OneIn32WithHalfBonus, false, true, 0);
        var heroDefinition = new BattleActorDefinition(ally.Actor, ally.Definition.ClassRule, 1, 20, 5, 8, 4, 6, false, 6, [],
            physical, sourceLoadout: ally.Definition.SourceLoadout, growth: ally.Definition.Growth);
        ally = ally with { Definition = heroDefinition };
        var enemyDefinition = new BattleActorDefinition(new("enemy"), BattleClassRule.Ordinary, 2, 25, 0, 6, 4, 5, false, 2, [],
            new(PhysicalCriticalRule.OneIn32WithHalfBonus, false, false, 12), sourceLoadout: new([127, 127, 127, 127], [63, 63, 63, 63]));
        var enemy = new BattleDeploymentDefinition(enemyDefinition, BattleFaction.Enemy, 128, BattleControl.Automatic,
            new("source-orders"), new(8, 8), new(0x2000, 0, 6, 15, 0x60, 6, 255, 255));
        var dormant = enemy with { Definition = new(new("hidden"), BattleClassRule.Ordinary, 1, 10, 0, 4, 2, 3, false, 2, [],
            new(PhysicalCriticalRule.OneIn32WithHalfBonus, false, false, 0), sourceLoadout: enemyDefinition.SourceLoadout),
            ProcessingOrder = 129, Position = new(9, 9), Initialization = new(0x2000, 2, 7, 15, 0x60, 6, 255, 255) };
        var encounter = new BattleDefinition(encounterId, new("battle-map"), 20, 20,
            Enumerable.Repeat(new BattleTerrain(TerrainSurface.Open, TerrainProtection.None), 2304), [ally, enemy, dormant], [],
            new(false), new([new(6, [new(17, 17), new(19, 17), new(19, 19)]), new(7, [new(17, 17), new(19, 17), new(19, 19)])], BattleRegionProgram.None, [ally.Position], sourceCombatants: new Dictionary<int, ActorRef> { [0] = ally.Actor, [128] = enemy.Actor, [129] = dormant.Actor }, enemyCount: 2), new(ally.Actor, enemy.Actor));
        var outcome = new ExplorationOutcomeRoute(new("after", 0), 1, new("empty", 0), new("empty", 0),
            new("return", 0), new("map-57"), 3, new("map-3"), new(32, 13), 1);
        var route = new ExplorationBattleRoute(encounterId, 401, 501, 451, null, null, Outcome: outcome);
        var maps = basis.Exploration!.Maps.Values.Select(m => new ExplorationMapDefinition(m.Map, m.Layout, m.Traversal,
            m.Entities, [], battles: m.Map.Value == "map-57" ? [route] : [], population: m.Population,
            basePalette: m.BasePalette, viewAreas: m.ViewAreas)).ToArray();
        var world = new ExplorationDefinition(maps, [new("empty", [new EndProgram()]),
            new("after", [new WriteFlag(777, true), new EndProgram()]), new("return", [new ReturnBattleMap(), new EndProgram()])],
            provenance: basis.Exploration.Provenance, partyFlags: basis.Exploration.PartyFlags, visuals: basis.Exploration.Visuals);
        var definition = new ScenarioDefinition("battle-save-test", [encounter, basis.Encounters["opening"]], exploration: world,
            encounterStarts: [new(encounterId, [], 0, 0, 0)]) { Persistence = new Sf2OpeningRestore() };
        var carried = field.Exploration!.Party.Actors[0];
        var actors = new BattleActorState[] {
            new(ally, 7, 2, 38, new(4, 5), 4, 2, attack: 14, progress: carried.Progress, sourceLoadout: carried.SourceLoadout, activationWord: 0),
            new(enemy, 19, 0, null, enemy.Position, null, null, lastTarget: ally.Actor, attack: 7, activationWord: 0x2060, aiMemory: 0x14),
            new(dormant, 0, 0, null, null, null, null, activationWord: 0x200, initialized: false) };
        var battle = new EngineBattleState(encounter, actors, main, thinking, 3,
            [new(enemy.Actor, 29), new(ally.Actor, 25), new(enemy.Actor, 20), new(ally.Actor, 18), new(null, 255), new(dormant.Actor, 250)], 1, 4321,
            new("opening", "", "", 0, false, true, true, false, false, null) { Origin = BattleStartOrigin.NewGame, BeforeBattleRouted = true },
            new(new bool[16], 0x8001), itemCustody: new([2], new Dictionary<byte, byte> { [1] = 3 }), deathCredit: new(ally.Actor));
        var flags = new[] { 0, 1, 32, 66, 80, 256, 399, 401, 451, 600, 602, 603 };
        var map57 = maps.Single(m => m.Map.Value == "map-57");
        var story = new StoryState(flags, enteringBattle: route, textCursor: 535,
            partyLists: MapPartyMembership.Rebuild(flags, world.PartyFlags!), portraitWindow: new ClosedPortraitWindow(),
            display: new(3, map57.BasePalette!, map57.BasePalette!, FullFadeVisibility.BaseRestored),
            logicalText: new(), logicalView: field.Story.LogicalView, textSettings: new(2, 0, 0),
            entityServices: false, windowFixPending: true, music: new(17, "field", ["previous"], map57.Map, 0, null, Battle: true),
            stepCounter: 27, egressMap: new("map-3"), fieldPlayer: new(new("entity-0"), 32,
                [field.Exploration.Party.Actors[1] with { Hp = 0, Absent = true }]), names: new Dictionary<string, string> { [ally.Actor.Value] = "Bowie" });
        return (definition, At(battle, story));
    }

    internal static SessionSnapshot At(EngineBattleState battle, StoryState story) => new(Guid.NewGuid(), 27, 60,
        new ActiveBattle(battle, new(BattleTurnFlow.QueuedActor(battle).Actor,
            BattleMovement.Preview(battle, BattleTurnFlow.QueuedActor(battle).Actor, BattleTurnFlow.QueuedActor(battle).Position!), BattleSelectionStage.Movement)),
        story, SessionStopReason.PlayerInput);

    internal static BattleSaveData Capture(ScenarioDefinition definition, SessionSnapshot current)
    {
        var capture = BattleSave.Capture(definition, current, RuleCompositions.Sf2(), current.SessionId, current.Revision);
        Assert.Null(capture.Error); return Assert.IsType<BattleSaveData>(capture.Data);
    }

    internal static SessionSnapshot Stay(SessionSnapshot current)
    {
        var rules = RuleCompositions.Sf2();
        foreach (SessionCommand command in new SessionCommand[] { new Confirm(), new ChooseAction(SessionAction.Stay), new Confirm() })
        {
            var result = BattleCommandDispatcher.Submit(current, command, rules);
            Assert.Null(result.Failure); current = result.Snapshot;
        }
        for (int guard = 0; current.StopReason != SessionStopReason.PlayerInput; guard++)
        {
            Assert.True(guard < 1000);
            SessionResult result = current.BattleMovement is { } move ? BattleMovementContinuation.Submit(current, new CompletePresentation(move.Token, move.CompletionKind), rules) :
                current.BattleScene is { } scene ? BattleSceneContinuation.Submit(current, scene.RequiresAcknowledgement ? new Acknowledge(scene.Token) : new CompletePresentation(scene.Token, scene.CompletionKind), rules) :
                BattleAdvancer.Advance(current, [], rules);
            Assert.Null(result.Failure); current = result.Snapshot;
        }
        return current;
    }

    internal static void EqualBattle(EngineBattleState expected, EngineBattleState actual)
    {
        Assert.Equal(expected.MainSeed, actual.MainSeed); Assert.Equal(expected.ThinkingSeed, actual.ThinkingSeed);
        Assert.Equal(expected.Queue, actual.Queue); Assert.Equal(expected.Cursor, actual.Cursor); Assert.Equal(expected.Round, actual.Round);
        Assert.Equal(expected.Gold, actual.Gold); Assert.Equal(expected.DeathCredit, actual.DeathCredit);
        Assert.Equal(expected.Regions!.Flags, actual.Regions!.Flags); Assert.Equal(expected.Regions.Tested, actual.Regions.Tested);
        Assert.Equal(expected.ItemCustody!.OnceFlags, actual.ItemCustody!.OnceFlags); Assert.Equal(expected.ItemCustody.Deals, actual.ItemCustody.Deals);
        foreach (var actor in expected.Actors)
        {
            var other = actual.GetActor(actor.Actor);
            Assert.Equal(actor.Position, other.Position); Assert.Equal(actor.Deployment.Position, other.Deployment.Position);
            Assert.Equal(actor.Hp, other.Hp); Assert.Equal(actor.Mp, other.Mp); Assert.Equal(actor.Exp, other.Exp);
            Assert.Equal(actor.Kills, other.Kills); Assert.Equal(actor.Defeats, other.Defeats); Assert.Equal(actor.Attack, other.Attack);
            Assert.Equal(actor.Status, other.Status); Assert.Equal(actor.Initialized, other.Initialized); Assert.Equal(actor.Facing, other.Facing);
            Assert.Equal(actor.ActivationWord, other.ActivationWord); Assert.Equal(actor.AiMemory, other.AiMemory);
            Assert.Equal(actor.PrimaryOrder, other.PrimaryOrder); Assert.Equal(actor.SecondaryOrder, other.SecondaryOrder); Assert.Equal(actor.LastTarget, other.LastTarget);
            Assert.Equal(actor.SourceLoadout!.Items, other.SourceLoadout!.Items); Assert.Equal(actor.SourceLoadout.Spells, other.SourceLoadout.Spells);
            Assert.Equal(actor.Progress, other.Progress is null ? null : other.Progress with { Spells = actor.Progress!.Spells, SourceLoadout = actor.Progress.SourceLoadout });
        }
    }
}
