using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Gameplay.Sf2;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class StoryPolicyReplacementTests
{
    [Theory]
    [InlineData("a", 41, 4, 2, 3, 2)]
    [InlineData("b", 51, 3, 4, 4, 4)]
    public void ExternalProgramsExecuteBothFlagBranchesWithNestedReturnsAndRealWaits(
        string variant, int flag, int firstX, int firstY, int againX, int againY)
    {
        var session = StartStory(variant);
        var identity = session.Current.SessionId;
        var party = session.Current.Exploration!.Party;
        var rules = session.Rules;
        var initial = new MapPosition(4, 3);
        for (int branch = 0; branch < 2; branch++)
        {
            int pre = flag + branch * 2;
            string called = (branch == 0 ? "first-" : "again-") + variant;
            string caller = variant == "a" ? (branch == 0 ? "invitation" : "repeat") : (branch == 0 ? "first-visit-b" : "invitation");
            int returnPc = variant == "a" ? (branch == 0 ? 2 : 1) : (branch == 0 ? 1 : 2);
            var input = Accept(session, new Interact(new("ferryman")));
            var current = session.Current;
            var dialogue = Assert.IsType<DialogueWait>(current.Story.Wait);
            Assert.Contains(pre, current.Story.Flags);
            Assert.DoesNotContain(pre + 1, current.Story.Flags);
            Assert.Equal(branch == 1, current.Story.Flags.Contains(40));
            Assert.Equal(new ProgramLocation("pause-" + variant, 1), current.Story.Cursor);
            Assert.Equal(new[] { new ProgramLocation(caller, returnPc),
                new ProgramLocation(called, 3) }, current.Story.Callers);
            var marker = current.Exploration!.Entities[new("marker")];
            Assert.Equal(branch == 1, marker.Visible);
            Assert.Equal(branch == 0 ? initial : new(firstX, firstY), marker.Position);
            Assert.Equal(new[] { "CallProgram", "CallProgram" }, input.ProgramControlReads!.Select(read => read.Operation));
            RejectUnchanged(session, new CompletePresentation(dialogue.Token, PresentationCueKind.Gesture));
            RejectUnchanged(session, new Acknowledge(new(dialogue.Token.Value + 1)));
            Accept(session, new Acknowledge(dialogue.Token));
            var ticks = Assert.IsType<TickWait>(session.Current.Story.Wait);
            Assert.Equal(30, ticks.Remaining);
            long startTick = session.Current.Story.SimulationTick;
            RejectUnchanged(session, new Acknowledge(dialogue.Token));
            RejectUnchanged(session, new AdvanceSimulation(dialogue.Token));
            Accept(session, new AdvanceSimulation(ticks.Token, 29));
            Assert.Equal(1, Assert.IsType<TickWait>(session.Current.Story.Wait).Remaining);
            Assert.DoesNotContain(pre + 1, session.Current.Story.Flags);
            Assert.Equal(marker.Position, session.Current.Exploration!.Entities[new("marker")].Position);
            var returned = Accept(session, new AdvanceSimulation(ticks.Token));
            Assert.Equal(startTick + 30, session.Current.Story.SimulationTick);
            Assert.Equal(new[] { "ReturnProgram", "ReturnProgram", "EndProgram" },
                returned.ProgramControlReads!.Select(read => read.Operation));
            Assert.Equal(new ProgramLocation(called, 3), returned.ProgramControlReads![0].Cursor);
            Assert.Equal(new ProgramLocation(caller, returnPc),
                returned.ProgramControlReads[1].Cursor);
            Assert.Contains(pre + 1, session.Current.Story.Flags);
            Assert.Equal(branch == 0, session.Current.Story.Flags.Contains(40));
            Assert.Null(session.Current.Story.Cursor); Assert.Empty(session.Current.Story.Callers);
            Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
            marker = session.Current.Exploration!.Entities[new("marker")];
            Assert.Equal(branch == 0 ? new(firstX, firstY) : new(againX, againY), marker.Position);
            Assert.Equal(branch == 0, marker.Visible);
            RejectUnchanged(session, new AdvanceSimulation(ticks.Token));
            RejectUnchanged(session, new Acknowledge(dialogue.Token));
        }
        Accept(session, new Move(ExplorationDirection.West));
        var moving = Assert.IsType<EntityWait>(session.Current.Story.Wait);
        for (int tick = 0; tick < 8 && session.Current.Story.Wait is not null; tick++)
            Accept(session, new AdvanceSimulation(moving.Token));
        Assert.Equal(new MapPosition(0, 1), session.Current.Exploration!.PlayerEntity.Position);
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
        Assert.Same(rules, session.Rules);
        Assert.Equal(party.Actors, session.Current.Exploration.Party.Actors);
        Assert.Equal(party.Gold, session.Current.Exploration.Party.Gold);
        Assert.Equal(party.MainSeed, session.Current.Exploration.Party.MainSeed);
        Assert.Equal(identity, session.Current.SessionId);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("b")]
    public void BadUntakenBranchAndNestedCallReferencesRejectBeforeSessionAdmission(string variant)
    {
        foreach (var target in new[] { "branch", "call" })
        {
            var document = Document("story-rule-demo-" + variant);
            var program = target == "branch" ? 0 : 2;
            var instruction = target == "branch" ? 0 : 2;
            document["world"]!["programs"]![program]!["instructions"]![instruction]!["target"]!["program"] = "absent";
            var failure = Assert.IsType<SessionStartFailed>(GameSession.Start(Reader(document), RuleCompositions.AuthoredStory())).Failure;
            Assert.Equal(SessionFailureKind.ContentError, failure.Kind);
            Assert.Equal("unresolved-program-target", failure.Code);
        }
    }

    [Theory]
    [InlineData("throw")]
    [InlineData("null")]
    [InlineData("visible")]
    [InlineData("foreign")]
    [InlineData("sprite")]
    [InlineData("position")]
    [InlineData("reject")]
    [InlineData("source-unknown")]
    [InlineData("authored")]
    public void SourcePolicyFailureRetainsEarlierEffectsAndExactNestedCursor(string shape)
    {
        var session = StartStory("a");
        var basis = session.Current;
        StoryInstruction operation = shape == "source-unknown" ? new AnotherSourceInstruction() : new RetiredMap3EntityScratch();
        var world = session.Definition.Exploration!;
        StoryProgram[] programs = [
            new("outer", [new WriteFlag(90, true), new CallProgram(new("inner", 0)), new WriteFlag(93, true), new EndProgram()]),
            new("inner", [new WriteFlag(91, true), operation, new WriteFlag(92, true), new ReturnProgram()])];
        var definition = new ScenarioDefinition("policy-failure", session.Definition.Encounters.Values,
            exploration: new(world.Maps.Values, programs));
        var rules = shape == "authored" ? RuleCompositions.AuthoredStory() : shape == "source-unknown" ? RuleCompositions.Sf2() :
            new SessionRules("test-story", RuleCompositions.Sf2().Healing, RuleCompositions.SourcePhysical(),
                RuleCompositions.SourceItem(), RuleCompositions.SourceStay(), sourceStory: new BrokenPolicy(shape));
        var input = basis.WithStory(basis.Story.Copy(new("outer", 0)));
        var result = ProgramRunner.Run(definition, input, [], rules);
        Assert.NotNull(result.Failure);
        Assert.Equal(shape is "reject" or "source-unknown" or "authored" ? SessionFailureKind.UnsupportedCapability : SessionFailureKind.InvariantFailure,
            result.Failure.Kind);
        Assert.Contains(shape == "authored" ? "authored-story" : shape == "source-unknown" ? "sf2-story" : "broken-story", result.Failure.Message);
        Assert.Contains(operation.GetType().Name, result.Failure.Message);
        Assert.Contains("inner:1", result.Failure.Message);
        Assert.DoesNotContain("private detail", result.Failure.Message);
        Assert.Equal(new ProgramLocation("inner", 1), result.Snapshot.Story.Cursor);
        Assert.Equal(new[] { new ProgramLocation("outer", 2) }, result.Snapshot.Story.Callers);
        Assert.Equal(new[] { 90, 91 }, result.Snapshot.Story.Flags);
        Assert.Same(basis.Active, result.Snapshot.Active);
        Assert.Null(result.Snapshot.Story.Wait);
        Assert.Equal(3, result.Snapshot.Revision - basis.Revision);
        Assert.Equal(3, result.Observations.Count);
        Assert.Single(result.ProgramControlReads!);
    }

    [Fact]
    public void UnsupportedOrdinaryOpcodeStopsAtItsOwnPcAfterTheWaitWithoutLosingPriorEffects()
    {
        var session = StartStory("a", document =>
            document["world"]!["programs"]![2]!["instructions"]!.AsArray().Insert(3,
                JsonNode.Parse("""{"op":"native-call","symbol":"later-source-service","source":"authored-boundary"}""")));
        Accept(session, new Interact(new("ferryman")));
        var dialogue = session.Current.Story.Wait!;
        Accept(session, new Acknowledge(dialogue.Token));
        var before = session.Current;
        var failure = Send(session, new AdvanceSimulation(before.Story.Wait!.Token, 30));
        Assert.Equal("program-opcode", failure.Failure!.Code);
        Assert.Equal(new ProgramLocation("first-a", 3), session.Current.Story.Cursor);
        Assert.Equal(new[] { new ProgramLocation("invitation", 2) }, session.Current.Story.Callers);
        Assert.Contains(41, session.Current.Story.Flags); Assert.DoesNotContain(42, session.Current.Story.Flags);
        Assert.False(session.Current.Exploration!.Entities[new("marker")].Visible);
        Assert.Equal(new MapPosition(4, 3), session.Current.Exploration.Entities[new("marker")].Position);
        Assert.Equal(before.Exploration!.Party.Actors, session.Current.Exploration.Party.Actors);
        Assert.Equal(before.Exploration.Party.MainSeed, session.Current.Exploration.Party.MainSeed);
        Assert.Equal(before.Exploration.Party.Gold, session.Current.Exploration.Party.Gold);
    }

    private static GameSession StartStory(string variant, Action<JsonNode>? configure = null)
    {
        var document = Document("story-rule-demo-" + variant); configure?.Invoke(document);
        return Assert.IsType<SessionStarted>(GameSession.Start(Reader(document), RuleCompositions.AuthoredStory())).Session;
    }

    private static void RejectUnchanged(GameSession session, SessionCommand command)
    {
        var before = session.Current;
        var rejected = Send(session, command);
        Assert.NotNull(rejected.Failure); Assert.Same(before, rejected.Snapshot);
        Assert.Empty(rejected.Observations);
    }

    private sealed record AnotherSourceInstruction : SourceStoryInstruction;
    private sealed class BrokenPolicy(string shape) : ISourceStoryPolicy
    {
        public string Identity => "broken-story";
        public SourceStoryCandidate Apply(ScenarioDefinition definition, SessionSnapshot current,
            SourceStoryInstruction instruction, ProgramLocation location)
        {
            if (shape == "throw") throw new Exception("private detail");
            if (shape == "reject") throw new BattleRuleException("not-supported", "program.sourceStory", true);
            if (shape == "null") return null!;
            var world = current.Exploration!;
            var entity = world.Entities[new("marker")];
            var retired = world.Hide(entity, false).AllEntities.Single(row => row.Slot == entity.Slot);
            return new(shape switch
            {
                "visible" => retired with { Visible = true },
                "foreign" => retired with { Slot = 999 },
                "sprite" => retired with { Sprite = 99 },
                "position" => retired with { Motion = retired.Motion with { X = 0 } },
                _ => retired,
            });
        }
    }
}
