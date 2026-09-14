using Sf2.Remake.Domain.Maps;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using System.Text.Json.Nodes;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class MapEntityLifecycleTests
{
    [Fact]
    public void PlayerInputAvoidsMovingObstructableActorsButCanCrossAnUnobstructableFollower()
    {
        var player = EntityMotionState.At(new(2, 2), 0, 32) with { FlagsA = 0xE0 };
        var other = EntityMotionState.At(new(3, 2), 0, 32) with { FlagsA = 0x80, XDestination = 4 * 384 };
        Assert.Null(EntityMotion.Start(player, 3 * 384, 2 * 384, [other], fieldInput: true));
        Assert.NotNull(EntityMotion.Start(player, 3 * 384, 2 * 384, [other with { FlagsA = 0x60 }], fieldInput: true));
        // Scripted moveTo uses its own destination test, not the current-position player test.
        Assert.NotNull(EntityMotion.Start(player, 3 * 384, 2 * 384, [other]));
    }

    [Theory]
    [InlineData(false, 3)]
    [InlineData(true, 4)]
    public void CommonContentAllocatesSlotsFromLiveFlagsAndJoinsThroughProgramExecution(bool extra, int npcSlot)
    {
        var session = StartPopulation(extra);
        var world = session.Current.Exploration!;
        Assert.Equal(npcSlot, world.Entities[new("entity-128")].Slot);
        Assert.Equal(1, world.Entities[new("entity-1")].Slot);
        Assert.Equal(world.PlayerEntity.Position, world.Entities[new("entity-1")].Position);
        Assert.Equal(new[] { 0, 1, 2 }, session.Current.Story.PartyLists!.Joined);
        Assert.Equal(new[] { 0, 1 }, session.Current.Story.PartyLists.Active);
        Assert.Contains(34, session.Current.Story.Flags);
        Assert.Equal(new EntityFollower(0, -24, 0), world.Entities[new("entity-1")].Follower);
        Assert.Equal(new EntityFollower(1, -24, 0), world.Entities[new("entity-2")].Follower);
        var before = world.Entities[new("entity-1")].Motion;
        for (int step = 0; step < 3; step++)
        {
            Accept(session, new Move(ExplorationDirection.East));
            while (session.Current.Story.Wait is EntityWait wait) Accept(session, new AdvanceSimulation(wait.Token, 60));
        }
        Accept(session, new AdvanceSimulation(null, 60));
        world = session.Current.Exploration!;
        Assert.True(world.Entities[new("entity-1")].Motion.X > before.X);
        Assert.True(world.Entities[new("entity-2")].Motion.X > before.X);
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
        Assert.Equal(npcSlot, world.Aliases[new("entity-128")]);
        // Turn back into the follower's occupied tile. esc02_input ignores its cleared bit7.
        var destination = new MapPosition(world.PlayerEntity.Position.X - 1, world.PlayerEntity.Position.Y);
        Accept(session, new Move(ExplorationDirection.West));
        for (int tick = 0; tick < 30 && session.Current.Story.Wait is EntityWait wait; tick++)
            Accept(session, new AdvanceSimulation(wait.Token));
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
        Assert.Equal(destination, session.Current.Exploration!.PlayerEntity.Position);
    }

    [Theory]
    [InlineData(135, 0)]
    [InlineData(39, 0)]
    [InlineData(391, 0)]
    [InlineData(128, 3)]
    [InlineData(32, 3)]
    [InlineData(64, -1)]
    [InlineData(160, -1)]
    [InlineData(255, -1)]
    public void SourceSelectorsResolveTheClearedAndPopulatedIdentityTable(int selector, int expectedSlot)
    {
        var world = StartPopulation(false).Current.Exploration!;
        bool found = world.TryResolveEntity(new("entity-" + selector), out var entity);
        Assert.Equal(expectedSlot >= 0, found);
        if (found) Assert.Same(world.AllEntities.Single(row => row.Slot == expectedSlot), entity);
        Assert.Equal(4, world.AllEntities.Count);
        Assert.DoesNotContain(world.AllEntities, row => row.Entity.Value == "entity-135");
    }

    [Theory]
    [InlineData("preserve", false)]
    [InlineData("rebuild", true)]
    public void RemovedSourceReferencesStayTombstonesThroughCopiesUntilPopulationRebuild(string mode, bool resolves)
    {
        var document = Document("harbor-arrival");
        string map = document["world"]!["maps"]![0]!["id"]!.GetValue<string>();
        var session = StartPopulation(false, $$$"""
            [{"op":"hide","entity":"entity-128","removeAliases":true},
             {"op":"face","entity":"entity-0","facing":2},
             {"op":"transfer","map":"{{{map}}}","position":{"x":2,"y":2},"facing":1,"loadMode":"{{{mode}}}"},
             {"op":"wait-ticks","ticks":1},{"op":"face","entity":"entity-32","facing":3},{"op":"end"}]
            """);
        var world = session.Current.Exploration!;
        Assert.Equal(resolves, world.TryResolveEntity(new("entity-128"), out _));
        Assert.Equal(resolves, world.TryResolveEntity(new("entity-32"), out _));
        Assert.True(world.TryResolveEntity(new("entity-135"), out var zero));
        Assert.Equal(0, zero.Slot);
        var result = Send(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        if (resolves) Assert.Null(result.Failure);
        else Assert.Equal("program-entity", result.Failure!.Code);
    }

    [Fact]
    public void HidingSlotZeroRemovesEveryUnassignedSourceReferenceInsteadOfRecreatingThePlayer()
    {
        var session = StartPopulation(false, """
            [{"op":"hide","entity":"entity-135","removeAliases":true},
             {"op":"wait-ticks","ticks":1},{"op":"face","entity":"entity-39","facing":3},{"op":"end"}]
            """);
        var world = session.Current.Exploration!;
        foreach (int id in new[] { 0, 3, 135, 39, 159 }) Assert.False(world.TryResolveEntity(new("entity-" + id), out _));
        Assert.True(world.TryResolveEntity(new("entity-128"), out _));
        Assert.False(world.AllEntities[0].Visible);
        Assert.Equal("program-entity", Send(session, new AdvanceSimulation(session.Current.Story.Wait!.Token)).Failure!.Code);
    }

    [Fact]
    public void SourceFacingWaitsForTheResolvedPhysicalSpriteAndRejectsStaleCompletion()
    {
        var session = StartPopulation(false, """
            [{"op":"face","entity":"entity-135","facing":3,"refreshSprite":true},
             {"op":"set-flag","flag":401,"value":true},{"op":"end"}]
            """);
        var wait = Assert.IsType<EntitySpriteWait>(session.Current.Story.Wait);
        Assert.Equal(0, wait.Slot);
        Assert.Equal(3, session.Current.Exploration!.PlayerEntity.Motion.Facing);
        Assert.DoesNotContain(401, session.Current.Story.Flags);
        Assert.NotNull(Send(session, new Acknowledge(wait.Token)).Failure);
        Assert.NotNull(Send(session, new EntitySpriteReady(wait.Slot, wait.Request - 1)).Failure);
        Accept(session, new AdvanceSimulation(wait.Token, 2));
        Assert.DoesNotContain(401, session.Current.Story.Flags);
        Accept(session, new EntitySpriteReady(wait.Slot, wait.Request));
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
        Assert.Contains(401, session.Current.Story.Flags);
        Assert.NotNull(Send(session, new EntitySpriteReady(wait.Slot, wait.Request)).Failure);
    }

    [Fact]
    public void AuthoredMissingIdentityStillStopsInsteadOfUsingTheSourceZeroDefault()
    {
        var session = Start("harbor-arrival", document =>
        {
            document["world"]!["programs"]![0]!["instructions"] = JsonNode.Parse("""
                [{"op":"wait-ticks","ticks":1},{"op":"face","entity":"entity-135","facing":3},{"op":"end"}]
                """);
            document["start"]!["program"] = JsonNode.Parse("""{"program":"invitation","instruction":0}""");
        });
        Assert.False(session.Current.Exploration!.TryResolveEntity(new("entity-135"), out _));
        Assert.Equal("program-entity", Send(session, new AdvanceSimulation(session.Current.Story.Wait!.Token)).Failure!.Code);
    }

    private static GameSession StartPopulation(bool extra, string? instructions = null) => Start("harbor-arrival", document =>
    {
        var world = document["world"]!;
        world["partyFlags"] = JsonNode.Parse("""{"memberCount":30,"joinedStart":0,"activeStart":32,"capacity":12}""");
        var map = world["maps"]![0]!;
        map["events"] = new JsonArray();
        map["onLoad"] = null;
        map["layout"] = new JsonArray(Enumerable.Range(0, 8).Select(_ => (JsonNode)new JsonArray(Enumerable.Range(0, 8).Select(_ => (JsonNode)JsonValue.Create(0)!).ToArray())).ToArray());
        map["areas"] = JsonNode.Parse("""[{"minX":0,"minY":0,"maxX":7,"maxY":7}]""");
        map["entities"] = JsonNode.Parse("""
            [{"id":"entity-1","position":{"x":2,"y":2},"facing":1,"speed":32,"visible":true,"obstruction":false,"sprite":1},
             {"id":"entity-2","position":{"x":3,"y":2},"facing":1,"speed":32,"visible":true,"obstruction":false,"sprite":2},
             {"id":"entity-128","position":{"x":4,"y":2},"facing":1,"speed":32,"visible":true,"obstruction":false,"sprite":200}]
            """);
        map["population"] = JsonNode.Parse("""
            {"allyCount":30,"nonAllyStart":128,"playerSprite":0,"followers":[
             {"flag":66,"character":1,"sprite":1},{"flag":66,"character":2,"sprite":2},{"flag":67,"character":9,"sprite":9}]}
            """);
        world["programs"]![0]!["instructions"] = JsonNode.Parse("""
            [{"op":"join-party","member":1},{"op":"join-party","member":2},
             {"op":"follow","entity":"entity-1","leader":"entity-0","x":-24,"y":0},
             {"op":"follow","entity":"entity-2","leader":"entity-1","x":-24,"y":0},{"op":"end"}]
            """);
        document["start"]!["player"] = "entity-0";
        document["start"]!["flags"] = JsonNode.Parse(extra ? "[0,32,66,67]" : "[0,32,66]");
        document["start"]!["program"] = JsonNode.Parse("""{"program":"invitation","instruction":0}""");
        if (instructions is not null) world["programs"]![0]!["instructions"] = JsonNode.Parse(instructions);
    });

    [Theory]
    [InlineData(false, false, 17)]
    [InlineData(true, false, 17)]
    [InlineData(true, true, 18)]
    public void LiveFollowersDeterminePhysicalSlotsWithoutChangingTheLogicalInteractionIdentity(bool pair, bool extra, int expectedSlot)
    {
        int[] sourceSprites = [1, 2, .. Enumerable.Repeat(100, 14), 209];
        List<MapFollowerSpawn> followers = [];
        if (pair) followers.AddRange([new(1, 1), new(2, 2)]);
        if (extra) followers.Add(new(9, 9));
        var allocation = MapEntityAllocator.Allocate(sourceSprites, followers, 30, 128, 0);
        Assert.Equal(expectedSlot, allocation.Aliases[142]);
        Assert.Equal(209, allocation.Slots.Single(slot => slot.Slot == expectedSlot).Sprite);
        Assert.Equal(pair, allocation.Sources[0].ReusedFollower);
        Assert.Equal(pair, allocation.Sources[1].ReusedFollower);
        Assert.Equal(1, allocation.Aliases[1]);
        Assert.Equal(2, allocation.Aliases[2]);
        Assert.Equal(0, allocation.Aliases[0]);
        Assert.Equal(allocation.Slots.Count, allocation.Slots.Select(slot => slot.Slot).Distinct().Count());
    }

    [Fact]
    public void JoiningPublishesMembershipBeforeTheNextCountedListRefresh()
    {
        var layout = new MapPartyFlagLayout(4, 1000, 2000, 3);
        var first = MapPartyMembership.Join([1000, 2000], layout, 1);
        Assert.Equal(new[] { 0, 1 }, first.Lists.Joined);
        Assert.Equal(new[] { 0 }, first.Lists.Active);
        Assert.Contains(2001, first.Flags);
        var second = MapPartyMembership.Join(first.Flags, layout, 2);
        Assert.Equal(new[] { 0, 1 }, second.Lists.Active);
        Assert.Contains(2002, second.Flags);
        Assert.Equal(new[] { 0, 1, 2 }, MapPartyMembership.Rebuild(second.Flags, layout).Active);
        var full = MapPartyMembership.Join(second.Flags, layout, 3);
        Assert.Contains(1003, full.Flags);
        Assert.DoesNotContain(2003, full.Flags);
        Assert.Equal(new[] { 3 }, full.Lists.Reserve);
        var repeated = MapPartyMembership.Join(full.Flags, layout, 1);
        Assert.Equal(full.Flags, repeated.Flags);
    }
}
