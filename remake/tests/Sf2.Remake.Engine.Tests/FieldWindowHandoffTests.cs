using System.Text.Json.Nodes;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Xunit;
using static Sf2.Remake.Engine.Tests.ExplorationTextWaitTests;

namespace Sf2.Remake.Engine.Tests;

public sealed class FieldWindowHandoffTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void HiddenTextInputSettlesTheViewAndWaitsForItsOwnAcknowledgement(bool revealed)
    {
        var session = StartFieldText("A{W1}", second: "B{W1}", enabled: true, npcRandom: true,
            configure: document => document["world"]!["programs"]![0]!["instructions"]!.AsArray().Insert(0,
                JsonNode.Parse("""{"op":"camera-target","position":{"x":4,"y":0}}""")));
        DrainTextWork(session);
        var token = session.Current.Story.Wait!.Token;
        if (revealed) Submit(new CompleteTextReveal(token));

        var pending = session.Current;
        var wait = Assert.IsType<FieldTextWait>(pending.Story.Wait);
        Assert.Equal(FieldTextPhase.Input, wait.Phase);
        Assert.Equal(revealed, wait.Revealed);
        Assert.True(pending.NeedsFieldWindowService);
        Assert.True(pending.Story.WindowFixPending);

        var axis = pending.Story.LogicalView!.AX;
        int destination = Assert.IsType<int>(axis.Destination);
        int cameraSteps = (destination - axis.Position + axis.Speed - 1) / axis.Speed;
        Submit(new AdvanceSimulation(token, cameraSteps));
        Assert.Equal(destination, session.Current.Story.LogicalView!.AX.Position);
        Assert.Null(session.Current.Story.LogicalView.AX.Destination);
        Assert.True(session.Current.NeedsFieldWindowService);

        var serviced = Submit(new AdvanceSimulation(token, 4));
        var settled = serviced.Snapshot;
        Assert.Single(serviced.Observations);
        Assert.False(settled.Story.LogicalView!.HideWindows);
        Assert.False(settled.Story.WindowFixPending);
        Assert.Equal(0, settled.Story.LogicalText!.AnimationCounter);
        Assert.Equal(1, settled.Story.LogicalText.AnimationLength);
        Assert.Same(pending.Active, settled.Active);
        Assert.Same(wait, settled.Story.Wait);
        Assert.Equal(pending.Story.Cursor, settled.Story.Cursor);
        Assert.Equal(pending.Story.SimulationTick + cameraSteps + 1, settled.Story.SimulationTick);
        var rejected = session.Submit(new(settled.SessionId, settled.Revision, null, new AdvanceSimulation(token)));
        Assert.Equal("explicit-text-wait-required", rejected.Failure!.Code);
        Assert.Same(settled, session.Current);

        if (!revealed) Submit(new CompleteTextReveal(token));
        Assert.True(session.Current.CanWaitForText);
        var acknowledged = Submit(new Acknowledge(token));
        var next = Assert.IsType<FieldTextWait>(acknowledged.Snapshot.Story.Wait);
        Assert.NotEqual(token, next.Token);
        Assert.Equal(101, next.Text);
        Assert.Contains(acknowledged.Observations, row => row.Kind == "rng-text-w1");

        SessionResult Submit(SessionCommand command)
        {
            var current = session.Current;
            var result = session.Submit(new(current.SessionId, current.Revision, null, command));
            Assert.Null(result.Failure);
            return result;
        }
    }
}
