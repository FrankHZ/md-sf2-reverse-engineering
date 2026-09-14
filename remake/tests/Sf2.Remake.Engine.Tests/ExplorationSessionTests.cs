using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class ExplorationSessionTests
{
    [Theory]
    [InlineData("harbor-arrival", "ferryman", 2, 1, 3, 1)]
    [InlineData("hill-passage", "watcher", 3, 2, 3, 1)]
    public void DialogueMotionCallTransferAndBothIntroHooksReachExistingBattle(string package, string npc,
        int fromX, int fromY, int toX, int toY)
    {
        var session = Start(package);
        var identity = session.Current.SessionId;
        Assert.Equal(SessionMode.Exploration, session.Current.Mode);
        var initialSeed = session.Current.Exploration!.Party.MainSeed;
        var dialogue = Accept(session, new Interact(new(npc)));
        var wait = Assert.IsType<DialogueWait>(dialogue.Snapshot.Story.Wait);
        Assert.Equal(1, session.Current.Story.Cursor!.Value.Instruction);
        Assert.DoesNotContain(10, session.Current.Story.Flags);
        Assert.Same(session.Current, Send(session, new Acknowledge(new(wait.Token.Value + 1))).Snapshot);
        Accept(session, new Acknowledge(wait.Token));
        var choice = Assert.IsType<ChoiceWait>(session.Current.Story.Wait);
        Accept(session, new ChooseDialogue(choice.Token, true));
        var motionWait = Assert.IsType<EntityWait>(session.Current.Story.Wait);
        Assert.Equal(new MapPosition(fromX, fromY), session.Current.Exploration!.Entities[new(npc)].Position);
        Accept(session, new AdvanceSimulation(motionWait.Token));
        var first = session.Current.Exploration.Entities[new(npc)].Motion;
        Assert.Equal((fromX * 384, fromY * 384), ((int)first.X, (int)first.Y));
        Assert.True(first.IsMoving);
        Assert.Equal(initialSeed, session.Current.Exploration.Party.MainSeed);
        for (int i = 0; session.Current.Story.Wait is EntityWait && i < 10; i++)
            Accept(session, new AdvanceSimulation(session.Current.Story.Wait.Token));
        Assert.Equal(new MapPosition(toX, toY), session.Current.Exploration.Entities[new(npc)].Position);
        Assert.Contains(12, session.Current.Story.Flags);
        var timer = Assert.IsType<TickWait>(session.Current.Story.Wait);
        Assert.Equal(2, timer.Remaining);
        Accept(session, new AdvanceSimulation(timer.Token));
        Assert.Equal(1, Assert.IsType<TickWait>(session.Current.Story.Wait).Remaining);
        var transferred = Accept(session, new AdvanceSimulation(timer.Token));
        Assert.Contains(transferred.Observations, observation => observation.Kind == "map-transferred");
        Assert.Contains(14, session.Current.Story.Flags);
        Assert.Contains(15, session.Current.Story.Flags);
        Assert.DoesNotContain(20, session.Current.Story.Flags);
        Assert.Equal(SessionMode.Exploration, session.Current.Mode);
        Assert.IsType<DialogueWait>(session.Current.Story.Wait);
        var entered = Accept(session, new Acknowledge(session.Current.Story.Wait.Token));
        Assert.Equal(identity, session.Current.SessionId);
        Assert.Equal(SessionMode.Battle, session.Current.Mode);
        Assert.Null(session.Current.Exploration);
        Assert.NotNull(session.Current.Selection);
        Assert.Equal(1, session.Current.Battle.Round);
        Assert.Equal(new[] { 10, 12, 13, 14, 15, 16, 20 }, session.Current.Story.Flags);
        string[] kinds = entered.Observations.Select(observation => observation.Kind).ToArray();
        Assert.True(Array.IndexOf(kinds, "battle-initialized") < Array.IndexOf(kinds, "battle-loaded"));
        Assert.True(Array.IndexOf(kinds, "battle-loaded") < Array.IndexOf(kinds, "round-started"));
        var flags = session.Current.Story.Flags;
        Stay(session);
        Assert.Equal(flags, session.Current.Story.Flags);
    }

    [Theory]
    [InlineData("harbor-arrival", "ferryman")]
    [InlineData("hill-passage", "watcher")]
    public void DecliningReturnsControlAndAReplayedChoiceCannotMutateFlags(string package, string npc)
    {
        var session = Start(package);
        var position = session.Current.Exploration!.PlayerEntity.Position;
        Accept(session, new Interact(new(npc)));
        Accept(session, new Acknowledge(session.Current.Story.Wait!.Token));
        var token = session.Current.Story.Wait!.Token;
        Accept(session, new ChooseDialogue(token, false));
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
        Assert.Equal(position, session.Current.Exploration!.PlayerEntity.Position);
        Assert.Empty(session.Current.Story.Flags);
        var before = session.Current;
        Assert.NotNull(Send(session, new ChooseDialogue(token, true)).Failure);
        Assert.Same(before, session.Current);
    }

    [Fact]
    public void UnsupportedInstructionKeepsItsCursorAndEarlierCommittedFlag()
    {
        var session = Start("harbor-arrival", document =>
        {
            document["world"]!["programs"]![0]!["instructions"] = System.Text.Json.Nodes.JsonNode.Parse("""
                [{"op":"set-flag","flag":7,"value":true},
                 {"op":"native-call","symbol":"unimplemented-service","source":"authored"},
                 {"op":"set-flag","flag":8,"value":true},{"op":"end"}]
                """);
        });
        var result = Send(session, new Interact(new("ferryman")));
        Assert.Equal(SessionFailureKind.UnsupportedCapability, result.Failure!.Kind);
        Assert.Equal(new ProgramLocation("invitation", 1), session.Current.Story.Cursor);
        Assert.Equal(new[] { 7 }, session.Current.Story.Flags);
        Assert.Equal(SessionStopReason.Unsupported, session.Current.StopReason);
    }

    [Fact]
    public void BlockedMovementStillFacesTheObstacleWithoutRelocatingOrAdvancingRng()
    {
        var session = Start("harbor-arrival");
        var before = session.Current;
        var result = Accept(session, new Move(ExplorationDirection.East));
        Assert.Equal(before.Exploration!.PlayerEntity.Position, session.Current.Exploration!.PlayerEntity.Position);
        Assert.Equal(before.Exploration.Party.MainSeed, session.Current.Exploration.Party.MainSeed);
        Assert.Contains(result.Observations, observation => observation.Kind == "movement-blocked");
        Accept(session, new Interact(new("ferryman")));
    }

    [Fact]
    public void ContinuedTextSurvivesItsAcknowledgementAndTickWaitUntilExplicitClose()
    {
        var session = StartProgram("""
            [{"op":"text-cursor","text":100},
             {"op":"show-text","mode":"continued","speaker":"ferryman","speakerFlags":192},
             {"op":"wait-ticks","ticks":2},{"op":"close-text"},
             {"op":"wait-ticks","ticks":1},{"op":"end"}]
            """);
        Assert.Equal(0, session.Current.Story.SimulationTick);
        var text = Assert.IsType<OpenTextWindow>(session.Current.Story.TextWindow);
        Assert.Equal(100, text.Text);
        Assert.Equal(192, text.SpeakerFlags);
        Assert.Equal(192, Assert.IsType<DialogueWait>(session.Current.Story.Wait).SpeakerFlags);
        Accept(session, new Acknowledge(session.Current.Story.Wait!.Token));
        Assert.Equal(text, session.Current.Story.TextWindow);
        Assert.Equal(0, session.Current.Story.SimulationTick);
        var wait = session.Current.Story.Wait!.Token;
        Accept(session, new AdvanceSimulation(wait, 600));
        Assert.Equal(2, session.Current.Story.SimulationTick);
        Assert.IsType<ClosedTextWindow>(session.Current.Story.TextWindow);
        Assert.Equal(1, Assert.IsType<TickWait>(session.Current.Story.Wait).Remaining);
        var stopped = session.Current;
        Assert.NotNull(Send(session, new AdvanceSimulation(wait)).Failure);
        Assert.Same(stopped, session.Current);
        Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        Assert.Equal(3, session.Current.Story.SimulationTick);
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
    }

    [Fact]
    public void SameTickActionConfigurationSurvivesReachedNativeFailureAtItsOwnCursor()
    {
        var session = StartProgram("""
            [{"op":"set-flag","flag":7,"value":true},
             {"op":"motion","entity":"ferryman","wait":true,"actions":[
               {"op":"speed","x":32,"y":48},
               {"op":"acceleration","x":2,"y":3},
               {"op":"flags","field":"a","mask":128,"value":128},
               {"op":"sprite-size","size":24},
               {"op":"refresh-sprite","source":"authored/native-refresh"},
               {"op":"face","facing":2}]},
             {"op":"set-flag","flag":8,"value":true},{"op":"end"}]
            """);
        var result = Send(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        Assert.Equal(SessionFailureKind.UnsupportedCapability, result.Failure!.Kind);
        Assert.Equal("entity-sprite-refresh", result.Failure.Code);
        Assert.Equal(new ProgramLocation("invitation", 1), session.Current.Story.Cursor);
        Assert.Equal(new[] { 7 }, session.Current.Story.Flags);
        Assert.Equal(1, session.Current.Story.SimulationTick);
        Assert.Equal(24, session.Current.Exploration!.SpriteSize);
        var entity = session.Current.Exploration.Entities[new("ferryman")];
        Assert.Equal(4, entity.ActionCursor);
        Assert.Equal((32, 48, 2, 3, 160), ((int)entity.Motion.XSpeed, (int)entity.Motion.YSpeed,
            (int)entity.Motion.XAcceleration, (int)entity.Motion.YAcceleration, (int)entity.Motion.FlagsA));
        var stopped = session.Current;
        Assert.NotNull(Send(session, new AdvanceSimulation(stopped.Story.Wait!.Token)).Failure);
        Assert.Same(stopped, session.Current);
    }

    [Fact]
    public void NonblockingActionOperationsRunInOrderBeforeTheFirstMovementTick()
    {
        var session = StartProgram("""
            [{"op":"motion","entity":"ferryman","wait":true,"actions":[
               {"op":"speed","x":192,"y":192},
               {"op":"face","facing":3},{"op":"move","x":1,"y":0}]},
             {"op":"set-flag","flag":11,"value":true},{"op":"end"}]
            """);
        var initial = session.Current.Exploration!.Entities[new("ferryman")].Motion;
        Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        var scheduled = session.Current.Exploration.Entities[new("ferryman")];
        Assert.Equal(initial.X, scheduled.Motion.X);
        Assert.Equal(initial.X + 384, scheduled.Motion.XDestination);
        Assert.Equal(3, scheduled.Motion.Facing);
        Assert.Equal(3, scheduled.ActionCursor);
        Assert.DoesNotContain(11, session.Current.Story.Flags);
        Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        Assert.Equal(initial.X + 192, session.Current.Exploration.Entities[new("ferryman")].Motion.X);
        Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        Assert.Equal(initial.X + 384, session.Current.Exploration.Entities[new("ferryman")].Motion.X);
        Assert.Contains(11, session.Current.Story.Flags);
        Assert.Equal(3, session.Current.Story.SimulationTick);
    }

    [Fact]
    public void ReplacingAnActionStreamChangesTheRemainingPathWithoutSnappingCurrentMotion()
    {
        var session = StartProgram("""
            [{"op":"motion","entity":"ferryman","wait":false,"actions":[
               {"op":"move","x":1,"y":0},{"op":"move","x":1,"y":0}]},
             {"op":"wait-ticks","ticks":1},
             {"op":"motion","entity":"ferryman","wait":true,"actions":[
               {"op":"face","facing":3}]},{"op":"end"}]
            """);
        var initial = session.Current.Exploration!.Entities[new("ferryman")].Motion;
        Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        Assert.Equal(initial.X, session.Current.Exploration.Entities[new("ferryman")].Motion.X);
        Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token, 600));
        Assert.Equal(initial.X + 384, session.Current.Exploration.Entities[new("ferryman")].Motion.X);
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
    }

    [Fact]
    public void SeenIntroFlagSkipsBothHooksWhileStillInitializingAndLoadingTheEncounter()
    {
        var session = Start("harbor-arrival", document =>
        {
            document["start"]!["flags"] = System.Text.Json.Nodes.JsonNode.Parse("[20]");
        });
        Accept(session, new Interact(new("ferryman")));
        Accept(session, new Acknowledge(session.Current.Story.Wait!.Token));
        Accept(session, new ChooseDialogue(session.Current.Story.Wait!.Token, true));
        for (int ticks = 0; session.Current.Mode == SessionMode.Exploration && ticks < 30; ticks++)
            Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        Assert.Equal(SessionMode.Battle, session.Current.Mode);
        Assert.Equal(1, session.Current.Battle.Round);
        Assert.DoesNotContain(15, session.Current.Story.Flags);
        Assert.DoesNotContain(16, session.Current.Story.Flags);
        Assert.Contains(20, session.Current.Story.Flags);
    }

    [Theory]
    [InlineData("dialogue")]
    [InlineData("choice")]
    [InlineData("ticks")]
    public void FreshBattleStartRetainsProgramControlUntilItsWaitCompletes(string waiting)
    {
        var session = Start("harbor-arrival", document =>
        {
            document["start"]!["map"] = "yard-map";
            document["start"]!["flags"] = System.Text.Json.Nodes.JsonNode.Parse("[13]");
            document["world"]!["maps"]![1]!["battle"]!["before"] = null;
            string instructions = waiting switch
            {
                "dialogue" => """{"op":"text-cursor","text":100},{"op":"show-text","mode":"single","speaker":null}""",
                "choice" => """{"op":"yes-no","flag":10}""",
                _ => """{"op":"wait-ticks","ticks":2}""",
            };
            document["world"]!["programs"]!.AsArray().Single(program => program!["id"]!.GetValue<string>() == "battle-start")!["instructions"] =
                System.Text.Json.Nodes.JsonNode.Parse("[" + instructions + """,{"op":"set-flag","flag":16,"value":true},{"op":"end"}]""");
        });
        var pending = session.Current;
        Assert.Equal(SessionMode.Battle, pending.Mode);
        Assert.False(pending.HasBattleControl);
        Assert.Equal(0, pending.Battle.Round);
        Assert.Null(pending.Selection);
        Assert.DoesNotContain(16, pending.Story.Flags);
        Assert.Equal("program-owns-control", Send(session, new Confirm()).Failure!.Code);
        Assert.Same(pending, session.Current);
        var token = pending.Story.Wait!.Token;
        SessionCommand command = waiting switch
        {
            "dialogue" => new Acknowledge(token),
            "choice" => new ChooseDialogue(token, true),
            _ => new AdvanceSimulation(token, 2),
        };
        Accept(session, command);
        Assert.Equal(pending.SessionId, session.Current.SessionId);
        Assert.True(session.Current.HasBattleControl);
        Assert.Equal(1, session.Current.Battle.Round);
        Assert.NotNull(session.Current.Selection);
        Assert.Contains(16, session.Current.Story.Flags);
        if (waiting == "choice") Assert.Contains(10, session.Current.Story.Flags);
        Accept(session, new Confirm());
        Assert.Equal(BattleSelectionStage.ActionChoice, session.Current.Selection!.Stage);
    }

    [Fact]
    public void SimulationBatchesConsumeElapsedTicksAndStopBeforeAcknowledgingTheNextDialogue()
    {
        var session = StartProgram("""
            [{"op":"wait-ticks","ticks":120},{"op":"text-cursor","text":100},
             {"op":"show-text","mode":"single","speaker":null},
             {"op":"set-flag","flag":7,"value":true},{"op":"end"}]
            """);
        var timer = session.Current.Story.Wait!.Token;
        Accept(session, new AdvanceSimulation(timer, 30));
        Assert.Equal(30, session.Current.Story.SimulationTick);
        Assert.Equal(90, Assert.IsType<TickWait>(session.Current.Story.Wait).Remaining);
        Accept(session, new AdvanceSimulation(timer, 60));
        Assert.Equal(90, session.Current.Story.SimulationTick);
        Assert.Equal(30, Assert.IsType<TickWait>(session.Current.Story.Wait).Remaining);
        Accept(session, new AdvanceSimulation(timer, 60));
        Assert.Equal(120, session.Current.Story.SimulationTick);
        var dialogue = Assert.IsType<DialogueWait>(session.Current.Story.Wait);
        Assert.DoesNotContain(7, session.Current.Story.Flags);
        var pending = session.Current;
        Assert.Equal("stale-or-wrong-wait", Send(session, new AdvanceSimulation(timer, 30)).Failure!.Code);
        Assert.Same(pending, session.Current);
        Accept(session, new Acknowledge(dialogue.Token));
        Assert.Contains(7, session.Current.Story.Flags);
        Assert.Equal(120, session.Current.Story.SimulationTick);
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BackgroundMotionConsumesTheBatchWithoutReleasingExistingInputOrDialogue(bool dialogue)
    {
        var session = StartProgram("""
            [{"op":"set-flag","flag":7,"value":true},
             {"op":"motion","entity":"ferryman","wait":false,"actions":[
               {"op":"speed","x":1,"y":1},{"op":"move","x":1,"y":0}]},
            """ + (dialogue ? """
             {"op":"text-cursor","text":100},{"op":"show-text","mode":"single","speaker":null},
            """ : "") + """{"op":"end"}]""");
        var wait = session.Current.Story.Wait;
        var expectedStop = dialogue ? SessionStopReason.PresentationWait : SessionStopReason.PlayerInput;
        Assert.Equal(expectedStop, session.Current.StopReason);
        Accept(session, new AdvanceSimulation(wait?.Token));
        var before = session.Current;
        var npc = before.Exploration!.Entities[new("ferryman")];
        var result = Accept(session, new AdvanceSimulation(wait?.Token, 60));
        Assert.Equal(before.Story.SimulationTick + 60, session.Current.Story.SimulationTick);
        Assert.Equal(npc.Motion.X + 60, session.Current.Exploration!.Entities[new("ferryman")].Motion.X);
        Assert.Equal(expectedStop, result.StopReason);
        Assert.Same(wait, session.Current.Story.Wait);
        Assert.Equal(before.SessionId, session.Current.SessionId);
        Assert.Equal(before.Story.Flags, session.Current.Story.Flags);
        Assert.Equal(before.Exploration.Party.MainSeed, session.Current.Exploration.Party.MainSeed);
        Assert.Equal(60, result.Observations.Count(observation => observation.Kind == "simulation-tick"));
        Accept(session, new AdvanceSimulation(wait?.Token, 30));
        Assert.Equal(before.Story.SimulationTick + 90, session.Current.Story.SimulationTick);
        Assert.Equal(npc.Motion.X + 90, session.Current.Exploration.Entities[new("ferryman")].Motion.X);
        Assert.Equal(expectedStop, session.Current.StopReason);
    }

    [Fact]
    public void CompletingForegroundMotionYieldsAtNewFieldInputBeforeContinuingBackgroundTicks()
    {
        var session = StartProgram("""
            [{"op":"motion","entity":"ferryman","wait":false,"actions":[
               {"op":"speed","x":1,"y":1},{"op":"move","x":1,"y":0}]},
             {"op":"motion","entity":"traveler","wait":true,"actions":[
               {"op":"move","x":1,"y":0}]},{"op":"end"}]
            """);
        var startX = session.Current.Exploration!.Entities[new("ferryman")].Motion.X;
        var wait = Assert.IsType<EntityWait>(session.Current.Story.Wait).Token;
        Accept(session, new AdvanceSimulation(wait, 60));
        Assert.Equal(5, session.Current.Story.SimulationTick);
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
        Assert.Null(session.Current.Story.Wait);
        Assert.Equal(startX + 4, session.Current.Exploration!.Entities[new("ferryman")].Motion.X);
        var boundary = session.Current;
        Assert.Equal("stale-or-wrong-wait", Send(session, new AdvanceSimulation(wait, 55)).Failure!.Code);
        Assert.Same(boundary, session.Current);
        Accept(session, new AdvanceSimulation(null, 2));
        Assert.Equal(7, session.Current.Story.SimulationTick);
        Assert.Equal(startX + 6, session.Current.Exploration.Entities[new("ferryman")].Motion.X);
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
    }

    private static GameSession StartProgram(string instructions) => Start("harbor-arrival", document =>
    {
        document["world"]!["programs"]![0]!["instructions"] = System.Text.Json.Nodes.JsonNode.Parse(instructions);
        document["start"]!["program"] = System.Text.Json.Nodes.JsonNode.Parse("""
            {"program":"invitation","instruction":0}
            """);
    });

    [Fact]
    public void UnsupportedTargetSetupKeepsTheSourceMapAndTheCompletedProgramPrefix()
    {
        var session = Start("harbor-arrival", document =>
        {
            document["world"]!["maps"]![1]!["setup"] = System.Text.Json.Nodes.JsonNode.Parse("""
                {"default":"base","variants":[{"flag":10,"setup":"closed"}]}
                """);
        });
        var originalMap = session.Current.Exploration!.Map;
        Accept(session, new Interact(new("ferryman")));
        Accept(session, new Acknowledge(session.Current.Story.Wait!.Token));
        Accept(session, new ChooseDialogue(session.Current.Story.Wait!.Token, true));
        SessionResult? result = null;
        for (int tick = 0; session.Current.StopReason == SessionStopReason.SimulationWait && tick < 100; tick++)
            result = Send(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        Assert.Equal("map-setup", result!.Failure!.Code);
        Assert.Equal(originalMap, session.Current.Exploration!.Map);
        Assert.Contains(13, session.Current.Story.Flags);
        Assert.DoesNotContain(14, session.Current.Story.Flags);
    }
}
