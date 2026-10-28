using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class ExplorationInteractionTests
{
    [Fact]
    public void QueryIsReadOnlyAndInteractionUsesTheOfferedTarget()
    {
        var session = Start("harbor-arrival");
        var before = session.Current;
        Assert.Equal(new EntityRef("ferryman"), session.QueryExplorationInteraction());
        Assert.Same(before, session.Current);
        var result = Accept(session, new Interact(session.QueryExplorationInteraction()!.Value));
        Assert.IsType<DialogueWait>(result.Snapshot.Story.Wait);
        Assert.Null(session.QueryExplorationInteraction());
        Accept(session, new Acknowledge(session.Current.Story.Wait!.Token));
        Assert.Null(session.QueryExplorationInteraction());
        Accept(session, new ChooseDialogue(session.Current.Story.Wait!.Token, false));
        Assert.Equal(new EntityRef("ferryman"), session.QueryExplorationInteraction());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FlagEligibilityMatchesCommitAndIsRecheckedAfterInteraction(bool enabled)
    {
        var session = Start("harbor-arrival", document =>
        {
            document["world"]!["maps"]![0]!["events"]![0]!["requiredFlag"] = 77;
            if (enabled) document["start"]!["flags"]!.AsArray().Add(77);
            document["world"]!["programs"]![0]!["instructions"] = JsonNode.Parse("""
                [{"op":"set-flag","flag":77,"value":false},{"op":"end"}]
                """);
        });
        var target = new EntityRef("ferryman");
        Assert.Equal(enabled ? target : (EntityRef?)null, session.QueryExplorationInteraction());
        var result = Send(session, new Interact(target));
        Assert.Equal(enabled ? null : "no-interaction", result.Failure?.Code);
        Assert.Null(session.QueryExplorationInteraction());
        Assert.Equal("no-interaction", Send(session, new Interact(target)).Failure?.Code);
    }

    [Theory]
    [InlineData("hidden", "missing-interaction-entity")]
    [InlineData("far", "interaction-range-or-facing")]
    [InlineData("behind", "interaction-range-or-facing")]
    [InlineData("no-event", "no-interaction")]
    public void QueryDoesNotAdvertiseAnUnavailableInteraction(string change, string rejection)
    {
        var session = Start("harbor-arrival", document =>
        {
            var npc = document["world"]!["maps"]![0]!["entities"]![0]!;
            if (change == "hidden") npc["visible"] = false;
            if (change == "far") npc["position"]!["x"] = 4;
            if (change == "behind") document["start"]!["facing"] = 2;
            if (change == "no-event") document["world"]!["maps"]![0]!["events"] = new JsonArray();
        });
        Assert.Null(session.QueryExplorationInteraction());
        Assert.Equal(rejection, Send(session, new Interact(new("ferryman"))).Failure?.Code);
    }

    [Fact]
    public void FacingAndMotionUseLiveStateRatherThanTheLastHint()
    {
        var session = Start("harbor-arrival");
        var target = session.QueryExplorationInteraction()!.Value;
        Accept(session, new Move(ExplorationDirection.South));
        Assert.Null(session.QueryExplorationInteraction());
        for (int tick = 0; !session.Current.CanWaitAtInput && tick < 40; tick++)
            Accept(session, new AdvanceSimulation(session.Current.Story.Wait?.Token));
        Assert.True(session.Current.CanWaitAtInput);
        Assert.Null(session.QueryExplorationInteraction());
        Assert.Equal("interaction-range-or-facing", Send(session, new Interact(target)).Failure?.Code);
    }
}
