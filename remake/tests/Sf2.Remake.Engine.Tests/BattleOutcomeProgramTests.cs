using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleOutcomeProgramTests
{
    [Theory]
    [InlineData("normal", true)]
    [InlineData("live-alias", true)]
    [InlineData("wrong-map", false)]
    [InlineData("wrong-cursor", false)]
    [InlineData("active-window", false)]
    [InlineData("field-input", false)]
    [InlineData("missing-flag", false)]
    [InlineData("missing-record", false)]
    [InlineData("visible-record", false)]
    public void PostMessengerScratchRequiresTheExactNormalReloadAndRealEntityRetirement(string shape, bool accepted)
    {
        var source = Start("harbor-arrival");
        var original = source.Current.Exploration!;
        var map = new ExplorationMapDefinition(new(shape == "wrong-map" ? "elsewhere" : "map-3"),
            original.Layout, original.Definition.Traversal, [], []);
        var actor = new ExplorationEntity(new("entity-142"), EntityMotionState.At(new(2, 2), 3, 32), true);
        var world = new ExplorationState(map, original.Layout, original.Player, [original.PlayerEntity, actor], original.Party);
        if (shape is not ("live-alias" or "visible-record")) world = world.Hide(world.Entities[actor.Entity], removeAliases: true);
        if (shape == "visible-record")
            world = new(map, original.Layout, original.Player, world.AllEntities, original.Party,
                aliases: new Dictionary<EntityRef, int> { [original.Player] = original.PlayerEntity.Slot });
        if (shape == "missing-record") world = new(map, original.Layout, original.Player, [original.PlayerEntity], original.Party);
        string id = shape == "wrong-cursor" ? "another-call" : "byte-513a8";
        var program = new StoryProgram(id, [new EndProgram(), new RetiredMap3EntityScratch(), new EndProgram()]);
        var definition = new ScenarioDefinition("scratch-context", source.Definition.Encounters.Values,
            exploration: new([map], [program]));
        var flags = shape == "missing-flag" ? new[] { 1 } : shape == "live-alias" ? new[] { 603 } : new[] { 1, 603 };
        var story = new StoryState(flags, new(id, 1), continuation: shape == "field-input" ? ProgramContinuation.FieldInput : ProgramContinuation.MapLoaded,
            textWindow: shape == "active-window" ? new OpenTextWindow(1, TextDisplayMode.Single, null) : new ClosedTextWindow());
        var before = new SessionSnapshot(Guid.NewGuid(), 1, 1, new ActiveExploration(world), story, SessionStopReason.SimulationWait);
        var result = ProgramRunner.Run(definition, before, []);
        if (!accepted)
        {
            Assert.Equal("inactive-window-scratch-context", result.Failure!.Code);
            Assert.Same(world, result.Snapshot.Exploration);
            Assert.Equal(story.Flags, result.Snapshot.Story.Flags);
            return;
        }
        Assert.Null(result.Failure);
        var after = result.Snapshot.Exploration!;
        Assert.Same(world.Layout, after.Layout);
        Assert.Equal(original.PlayerEntity.Position, after.PlayerEntity.Position);
        Assert.Equal(flags, result.Snapshot.Story.Flags);
        if (shape == "normal")
        {
            Assert.Same(world, after);
            Assert.False(after.TryResolveEntity(new("entity-142"), out _));
        }
        else Assert.True(after.TryResolveEntity(new("entity-142"), out _));
        var hidden = after.AllEntities.Single(row => row.Entity.Value == "entity-142");
        Assert.False(hidden.Visible); Assert.Equal(0x7000, hidden.Motion.X); Assert.Equal(0x7000, hidden.Motion.Y);
    }

    [Fact]
    public void SpriteChangeRetainsTheEntityIdentityAndWaitsForItsExactRendererRequest()
    {
        var session = Start("harbor-arrival", document =>
        {
            document["world"]!["programs"]![0]!["instructions"] = JsonNode.Parse("""
                [{"op":"sprite","entity":"ferryman","sprite":60},{"op":"end"}]
                """);
            document["start"]!["program"] = JsonNode.Parse("""{"program":"invitation","instruction":0}""");
        });
        var before = session.Current;
        var wait = Assert.IsType<EntitySpriteWait>(before.Story.Wait);
        var entity = before.Exploration!.Entities[new("ferryman")];
        Assert.Equal(60, entity.Sprite);
        Assert.True(entity.WaitingForSprite);
        Assert.NotNull(Send(session, new Acknowledge(wait.Token)).Failure);
        Assert.NotNull(Send(session, new EntitySpriteReady(wait.Slot, wait.Request + 1)).Failure);
        Accept(session, new EntitySpriteReady(wait.Slot, wait.Request));
        var after = session.Current.Exploration!.Entities[new("ferryman")];
        Assert.Equal(entity.Entity, after.Entity); Assert.Equal(entity.Position, after.Position);
        Assert.False(after.WaitingForSprite); Assert.Null(session.Current.Story.Wait);
    }
}
