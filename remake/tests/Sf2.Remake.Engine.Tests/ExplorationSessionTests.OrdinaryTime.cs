using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed partial class ExplorationSessionTests
{
    [Fact]
    public void ControlledSetupOnNeutralServiceIsIdempotentAndKeepsOneOrderedPass()
    {
        var walker = new ExplorationEntity(new("entity-128"), EntityMotionState.At(new(8, 8), 0, 32), true,
            new([new RandomWalkEntity(new(8, 8), 1), new StopEntityActions()]), Slot: 0);
        var timer = new ExplorationEntity(new("entity-129"), EntityMotionState.At(new(9, 9), 0, 32), true,
            new([new WaitEntityTicks(10)]), Slot: 4);
        var (definition, before) = ControlWorld(npcs: [walker, timer]);
        var first = Step(definition, before, new WaitAtInput()).Snapshot;
        Assert.Equal(1, first.Story.SimulationTick);
        Assert.Equal(0xECAB1234u, first.Exploration!.Party.MainSeed);
        Assert.Equal(9 * 384, first.Exploration.Entities[walker.Entity].Motion.YDestination);
        Assert.Equal(1, first.Exploration.Entities[timer.Entity].Motion.WaitTimer);
        var motion = first.Exploration.PlayerEntity.Motion;
        Assert.Equal((32, 32, 0, 0, 0xFF, 0),
            ((int)motion.XSpeed, (int)motion.YSpeed, (int)motion.XAcceleration, (int)motion.YAcceleration, (int)motion.FlagsA, (int)motion.WaitTimer));
        Assert.Equal(before.Exploration!.PlayerEntity.Motion.AnimationCounter + 1, motion.AnimationCounter);
        Assert.Equal(before.Exploration.PlayerEntity.Motion.FlagsB, motion.FlagsB);
        var second = Step(definition, first, new WaitAtInput()).Snapshot;
        Assert.Equal(motion with { AnimationCounter = (byte)(motion.AnimationCounter + 1) }, second.Exploration!.PlayerEntity.Motion);
        Assert.Equal(first.Exploration.Party.MainSeed, second.Exploration.Party.MainSeed);
        Assert.Equal(2, second.Exploration.Entities[timer.Entity].Motion.WaitTimer);
    }

    [Theory]
    [InlineData(true, 0, 0x12341234u, 0xECAB1234u, 5, 6)]
    [InlineData(true, 2, 0x12341234u, 0x12341234u, 5, 5)]
    [InlineData(true, 0, 0xC632A55Au, 0x1091A55Au, 6, 5)]
    [InlineData(false, 0, 0x12341234u, 0xECAB1234u, 5, 6)]
    public void WarpProducingPassRetainsOldPopulationBeforeFade(bool preserve, byte delay, uint seed,
        uint expectedSeed, int npcX, int npcY)
    {
        var npc = new ExplorationEntity(new("walker"), EntityMotionState.At(new(5, 5), 0, 32), true,
            new([new WaitEntityTicks(delay), new RandomWalkEntity(new(5, 5), 1), new StopEntityActions()]), Slot: 1);
        var idle = new ExplorationEntity(new("idle"), EntityMotionState.At(new(8, 8), 0, 32), true, Slot: 2);
        var (definition, before) = WarpWorld([npc, idle], preserve, seed);
        var pending = ExplorationDispatcher.Submit(definition, before, new Move(ExplorationDirection.East));
        Assert.Null(pending.Failure);
        Assert.Equal(seed, pending.Snapshot.Exploration!.Party.MainSeed);
        Assert.Equal(before.Exploration!.PlayerEntity, pending.Snapshot.Exploration.PlayerEntity);
        var token = Assert.IsType<EntityWait>(pending.Snapshot.Story.Wait).Token;
        var result = ExplorationDispatcher.Submit(definition, pending.Snapshot, new AdvanceSimulation(token, 600));
        Assert.Null(result.Failure);
        var world = result.Snapshot.Exploration!;
        Assert.Equal(expectedSeed, world.Party.MainSeed);
        Assert.Equal(1, result.Snapshot.Story.SimulationTick);
        Assert.IsType<FullFadeWait>(result.Snapshot.Story.Wait);
        Assert.Equal(new MapPosition(1, 1), world.PlayerEntity.Position);
        Assert.Equal((npcX * 384, npcY * 384), ((int)world.Entities[npc.Entity].Motion.XDestination, (int)world.Entities[npc.Entity].Motion.YDestination));
        Assert.Equal(delay == 0 ? 0 : 1, world.Entities[npc.Entity].Motion.WaitTimer);
        Assert.Equal(idle with { Motion = idle.Motion with { AnimationCounter = 1 } }, world.Entities[idle.Entity]);
        var stale = ExplorationDispatcher.Submit(definition, result.Snapshot, new AdvanceSimulation(token));
        Assert.Equal("stale-or-wrong-wait", stale.Failure!.Code);
        Assert.Same(result.Snapshot, stale.Snapshot);
    }

    [Theory]
    [InlineData(0x12341234u, 0, 0xECAB1234u, 2, 2)]
    [InlineData(0xC632A55Au, 2, 0x1091A55Au, 3, 1)]
    public void PlayerWaitAdvancesOneEligibleOpportunityWithSourceWaitAndRandomWalkRules(
        uint seed, int waitTicks, uint afterSeed, int targetX, int targetY)
    {
        var session = Start("harbor-arrival", document =>
        {
            document["battle"]!["start"]!["mainSeed"] = seed;
            document["world"]!["maps"]![0]!["entities"]![0]!["actions"] = System.Text.Json.Nodes.JsonNode.Parse($$"""
                [{"op":"wait","ticks":{{waitTicks}}},{"op":"random-walk","x":2,"y":1,"radius":1}]
                """);
        });
        var initial = session.Current;
        for (int tick = 1; tick <= waitTicks; tick++)
        {
            Accept(session, new WaitAtInput());
            Assert.Equal(tick, session.Current.Exploration!.Entities[new("ferryman")].Motion.WaitTimer);
            Assert.Equal(seed, session.Current.Exploration.Party.MainSeed);
        }
        var before = session.Current;
        var result = Accept(session, new WaitAtInput());
        var npc = session.Current.Exploration!.Entities[new("ferryman")];
        // Source LCG advances the high word; scaled range 4 selects South or East above.
        Assert.Equal(afterSeed, session.Current.Exploration.Party.MainSeed);
        Assert.Equal((targetX * 384, targetY * 384), ((int)npc.Motion.XDestination, (int)npc.Motion.YDestination));
        Assert.Equal((768, 384), ((int)npc.Motion.X, (int)npc.Motion.Y));
        Assert.Equal(0, npc.Motion.WaitTimer);
        Assert.True(npc.Busy);
        Assert.Equal(SessionStopReason.PlayerInput, result.StopReason);
        Assert.Equal(before.Revision + 1, session.Current.Revision);
        Assert.Equal(before.Story.SimulationTick + 1, session.Current.Story.SimulationTick);
        Assert.Equal("gameplay-wait", Assert.Single(result.Observations).Kind);
        Assert.Equal(initial.SessionId, session.Current.SessionId);
        Assert.Equal(initial.Exploration!.PlayerEntity with { Motion = initial.Exploration.PlayerEntity.Motion with
            { AnimationCounter = (byte)(waitTicks + 1) } }, session.Current.Exploration.PlayerEntity);
        Assert.Equal(initial.Exploration.Party.ThinkingSeed, session.Current.Exploration.Party.ThinkingSeed);
        Assert.Equal(initial.CurrentRandomSeedCopy, session.Current.CurrentRandomSeedCopy);
        Accept(session, new WaitAtInput());
        npc = session.Current.Exploration.Entities[new("ferryman")];
        Assert.Equal((768 + (targetX - 2) * 96, 384 + (targetY - 1) * 96), ((int)npc.Motion.X, (int)npc.Motion.Y));
        Assert.Equal(afterSeed, session.Current.Exploration.Party.MainSeed);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PlayerWaitResolvesCompetingDestinationsInPhysicalSlotOrder(bool reverse)
    {
        var session = Start("harbor-arrival", document =>
        {
            var entities = document["world"]!["maps"]![0]!["entities"]!.AsArray();
            entities[0]!["actions"] = System.Text.Json.Nodes.JsonNode.Parse("""[{"op":"move","x":1,"y":0}]""");
            entities.Add(System.Text.Json.Nodes.JsonNode.Parse("""
                {"id":"other","position":{"x":4,"y":1},"facing":2,"speed":96,"visible":true,
                 "obstruction":true,"actions":[{"op":"move","x":-1,"y":0}]}
                """));
            if (reverse)
            {
                var first = entities[0]; entities.RemoveAt(0); entities.Add(first);
            }
        });
        var seed = session.Current.Exploration!.Party.MainSeed;
        Accept(session, new WaitAtInput());
        var firstNpc = session.Current.Exploration!.AllEntities[1];
        var secondNpc = session.Current.Exploration.AllEntities[2];
        Assert.Equal(reverse ? "other" : "ferryman", firstNpc.Entity.Value);
        Assert.Equal((1, 2), (firstNpc.Slot, secondNpc.Slot));
        Assert.Equal(1152, firstNpc.Motion.XDestination);
        Assert.Equal(1, firstNpc.ActionCursor);
        Assert.Equal(secondNpc.Motion.X, secondNpc.Motion.XDestination);
        Assert.Equal(0, secondNpc.ActionCursor);
        Accept(session, new WaitAtInput());
        var moved = session.Current.Exploration.AllEntities[1];
        Assert.Equal(firstNpc.Motion.X + (reverse ? -96 : 96), moved.Motion.X);
        Assert.Equal(0, session.Current.Exploration.AllEntities[2].ActionCursor);
        Assert.Equal(seed, session.Current.Exploration.Party.MainSeed);
    }

    [Fact]
    public void PlayerWaitThenMoveUsesChangedOccupancyAndRejectsAnotherWaitDuringPlayerMotion()
    {
        var session = StartProgram("""
            [{"op":"motion","entity":"ferryman","wait":false,"actions":[{"op":"move","x":0,"y":-1}]},{"op":"end"}]
            """);
        var blocked = Accept(session, new Move(ExplorationDirection.East));
        Assert.Contains(blocked.Observations, row => row.Kind == "movement-blocked");
        Assert.Equal(new MapPosition(1, 1), session.Current.Exploration!.PlayerEntity.Position);
        var before = session.Current;
        foreach (SessionCommand wrong in new SessionCommand[]
        {
            new Acknowledge(new(1)), new CompletePresentation(new(1), PresentationCueKind.SoundWait),
        })
        {
            Assert.NotNull(Send(session, wrong).Failure);
            Assert.Same(before, session.Current);
        }
        Accept(session, new WaitAtInput());
        var moving = Accept(session, new Move(ExplorationDirection.East));
        Assert.Contains(moving.Observations, row => row.Kind == "movement-requested");
        var pending = session.Current;
        Assert.Equal("field-input-unavailable", Send(session, new WaitAtInput()).Failure!.Code);
        Assert.Same(pending, session.Current);
        // Mandatory motion still requires the existing token-bound simulation command.
        Accept(session, new AdvanceSimulation(pending.Story.Wait!.Token, 600));
        Assert.Equal(new MapPosition(2, 1), session.Current.Exploration!.PlayerEntity.Position);
        Accept(session, new WaitAtInput());
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
    }

    [Theory]
    [InlineData("dialogue")]
    [InlineData("choice")]
    [InlineData("ticks")]
    [InlineData("audio")]
    [InlineData("open-text")]
    [InlineData("player-actions")]
    public void PlayerWaitRejectsOtherExplorationConsumersWithoutAdvancing(string consumer)
    {
        string instructions = consumer switch
        {
            "dialogue" => """{"op":"text-cursor","text":100},{"op":"show-text","mode":"single","speaker":null},""",
            "choice" => """{"op":"yes-no","flag":10},""",
            "ticks" => """{"op":"wait-ticks","ticks":2},""",
            "audio" => """{"op":"present","kind":"SoundWait","resource":null,"entity":null,"position":null},""",
            "open-text" => """{"op":"text-cursor","text":100},{"op":"show-text","mode":"continued","speaker":null,"waitForAcknowledgement":false},""",
            _ => """{"op":"motion","entity":"traveler","wait":false,"actions":[{"op":"move","x":0,"y":1}]},""",
        };
        var session = StartProgram("[" + instructions + """{"op":"end"}]""");
        var before = session.Current;
        var result = Send(session, new WaitAtInput());
        Assert.Equal("field-input-unavailable", result.Failure!.Code);
        Assert.Equal("text-input-unavailable", Send(session, new WaitForText(before.Story.Wait?.Token ?? new(1))).Failure!.Code);
        Assert.Empty(result.Observations);
        Assert.Same(before, session.Current);
    }

    [Fact]
    public void PlayerWaitRejectsBattleControlAndSceneWithoutGrantingBattleTicks()
    {
        var session = Start("stone-court");
        var battle = session.Current;
        Assert.Equal("field-input-unavailable", Send(session, new WaitAtInput()).Failure!.Code);
        Assert.Same(battle, session.Current);
        Assert.Equal("text-input-unavailable", Send(session, new WaitForText(new(1))).Failure!.Code);
        Accept(session, new Confirm());
        Accept(session, new ChooseAction(SessionAction.PhysicalAttack));
        Accept(session, new SelectTarget(new("raider")));
        Accept(session, new Confirm());
        var scene = session.Current;
        Assert.NotNull(scene.BattleScene);
        Assert.Equal("text-input-unavailable", Send(session, new WaitForText(scene.BattleScene!.Token)).Failure!.Code);
        Assert.Equal("field-input-unavailable", Send(session, new WaitAtInput()).Failure!.Code);
        Assert.Equal("invalid-battle-tick", Send(session, new AdvanceSimulation(scene.BattleScene!.Token)).Failure!.Code);
        Assert.Same(scene, session.Current);
    }

    [Fact]
    public void PlayerWaitUsesSessionRevisionAndDoesNotAdvanceForStaleOrWrongActorInput()
    {
        var session = Start("harbor-arrival");
        var before = session.Current;
        var envelope = new CommandEnvelope(before.SessionId, before.Revision, null, new WaitAtInput());
        Assert.Equal("stale-input", session.Submit(envelope with { SessionId = Guid.NewGuid() }).Failure!.Code);
        Assert.Equal("wrong-actor", session.Submit(envelope with { Actor = new("outsider") }).Failure!.Code);
        Assert.Same(before, session.Current);
        Assert.Null(session.Submit(envelope).Failure);
        var after = session.Current;
        Assert.Equal(before.Story.SimulationTick + 1, after.Story.SimulationTick);
        Assert.Equal(before.Exploration!.Party.MainSeed, after.Exploration!.Party.MainSeed);
        Assert.Equal("stale-input", session.Submit(envelope).Failure!.Code);
        Assert.Same(after, session.Current);
    }

    [Fact]
    public void PlayerWaitPreservesEntityFailureAndCannotContinueTheStoppedConsumer()
    {
        var session = StartProgram("""
            [{"op":"motion","entity":"ferryman","wait":false,"actions":[
              {"op":"native-call","symbol":"unimplemented-service","source":"authored"}]},{"op":"end"}]
            """);
        var before = session.Current;
        var result = Send(session, new WaitAtInput());
        Assert.Equal(SessionFailureKind.UnsupportedCapability, result.Failure!.Kind);
        Assert.Equal(SessionStopReason.Unsupported, result.StopReason);
        Assert.Equal(before.Story.SimulationTick + 1, session.Current.Story.SimulationTick);
        Assert.Equal(before.Exploration!.Party.MainSeed, session.Current.Exploration!.Party.MainSeed);
        Assert.Equal("entity-action-stopped", Assert.Single(result.Observations).Kind);
        var stopped = session.Current;
        Assert.Equal("session-stopped", Send(session, new WaitAtInput()).Failure!.Code);
        Assert.Same(stopped, session.Current);
    }

}
