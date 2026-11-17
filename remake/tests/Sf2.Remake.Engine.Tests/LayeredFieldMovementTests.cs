using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class LayeredFieldMovementTests
{
    [Theory]
    [InlineData(0, 3, 30)]
    [InlineData(255, 0, 30)]
    public void FieldStepsUseSecondaryCollisionStairsAndExitMarkers(int mapLayer, int offsetX, int offsetY)
    {
        var words = new ushort[4096];
        words[4 * 64 + 5] = 0x2000; // Enter the second layer by an ordinary step.
        words[3 * 64 + 6] = 0xC000; // Primary collision must not block the secondary stairs.
        words[(4 + offsetY) * 64 + 5 + offsetX] = 0x8000;
        words[(3 + offsetY) * 64 + 6 + offsetX] = 0x8000;
        words[(3 + offsetY) * 64 + 7 + offsetX] = 0x2400;
        var world = World(words, mapLayer, offsetX, offsetY);
        world = Step(world, ExplorationDirection.East);
        Assert.Equal((byte)2, world.PlayerEntity.Motion.Layer);
        world = Step(world, ExplorationDirection.East);
        Assert.Equal(new MapPosition(6, 3), world.PlayerEntity.Position);
        Assert.Equal((byte)2, world.PlayerEntity.Motion.Layer);
        world = Step(world, ExplorationDirection.East);
        Assert.Equal(new MapPosition(7, 3), world.PlayerEntity.Position);
        Assert.Equal((byte)0, world.PlayerEntity.Motion.Layer);
        Assert.Same(world.Layout, world.Definition.Layout); // No copied or rewritten terrain.
    }

    [Fact]
    public void FollowersCheckTheirLeadersLayerAndBattleCoordinatesIgnoreFieldOffsets()
    {
        var words = new ushort[4096];
        words[4 * 64 + 7] = 0xC000;
        var world = World(words, 255, 0, 30);
        var leader = world.PlayerEntity with { Motion = world.PlayerEntity.Motion with
            { X = 8 * 384, XDestination = 8 * 384, Facing = 0, Layer = 2 } };
        var follower = FollowerMotion.Install(world.Entities[new("follower")], leader.Slot, -24, 0);
        var slots = world.WithEntity(leader).WithEntity(follower).AllEntities.ToDictionary(e => e.Slot);
        var next = FollowerMotion.Tick(follower, slots, world.WithEntity(leader));
        Assert.Equal(7 * 384, next.Motion.XDestination);
        var battle = FollowerMotion.Tick(follower, slots, world.WithEntity(leader), inBattle: true);
        Assert.Equal(8 * 384, battle.Motion.XDestination);
        Assert.Equal((ushort)0xC000, FieldLayout.Word(world, leader.Motion, 7, 4, inBattle: true));
        Assert.Equal((ushort)0, FieldLayout.Word(world, leader.Motion, 7, 4));
    }

    [Fact]
    public void FiniteSaveRefusesAnUnsupportedPlayerLayer()
    {
        var (definition, data) = OpeningRestoreTests.Fixture();
        var current = Assert.IsType<SessionStarted>(GameSession.Restore(definition, data, RuleCompositions.Sf2())).Session.Current;
        var world = current.Exploration!;
        world = world.WithEntity(world.PlayerEntity with { Motion = world.PlayerEntity.Motion with { Layer = 1 } });
        var changed = new SessionSnapshot(current.SessionId, current.Revision, current.ObservationSequence,
            new ActiveExploration(world), current.Story, current.StopReason);
        Assert.Equal("save-player-layer", OpeningSave.Query(definition, changed, RuleCompositions.Sf2()).Error);
        Assert.True(OpeningSave.Query(definition, current, RuleCompositions.Sf2()).Allowed);
    }

    private static ExplorationState Step(ExplorationState world, ExplorationDirection direction)
    {
        var moved = MapEventDispatcher.Move(world, direction, []);
        Assert.True(moved.Outcome.Moved);
        world = moved.World;
        for (int i = 0; i < 32 && world.PlayerEntity.Motion.IsMoving; i++) world = EntityActionRunner.Tick(world).World;
        Assert.False(world.PlayerEntity.Motion.IsMoving);
        return world;
    }

    private static ExplorationState World(ushort[] words, int layer, int x, int y)
    {
        var (definition, data) = OpeningRestoreTests.Fixture();
        var party = Assert.IsType<SessionStarted>(GameSession.Restore(definition, data, RuleCompositions.Sf2())).Session.Current.Exploration!.Party;
        var map = new ExplorationMapDefinition(new("layers"), new(words), new([new(0, 0, 20, 20)]),
            [new(new("follower"), new(1, 4), 0, 32)], [], viewAreas:
            [new(0, 0, 20, 20, layer == 0 ? x : 0, layer == 0 ? y : 0,
                layer == 0 ? 0 : x, layer == 0 ? 0 : y, 256, 256, 256, 256, 0, 0, 0, 0, layer)]);
        return MapTransfer.Build(map, new("entity-0"), new(4, 4), 0, 32, party, [], []);
    }
}
