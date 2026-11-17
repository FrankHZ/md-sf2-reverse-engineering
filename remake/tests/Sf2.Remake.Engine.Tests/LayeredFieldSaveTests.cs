using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class LayeredFieldSaveTests
{
    [Theory]
    [InlineData(0, 3, 30, 5, 4)]
    [InlineData(255, 0, 30, 6, 3)]
    public void OrdinarySecondaryStepsSaveRestoreAndReturnThroughPrimaryMarker(int plane, int ox, int oy, int x, int y)
    {
        var (definition, data) = Fixture(plane, ox, oy);
        var session = Restore(definition, data);
        var world = Step(session.Current.Exploration!, ExplorationDirection.East);
        if (x == 6) world = Step(world, ExplorationDirection.East);
        Assert.Equal(new MapPosition(x, y), world.PlayerEntity.Position);
        Assert.Equal((byte)2, world.PlayerEntity.Motion.Layer);
        // Capture a settled actual movement result; the initial primary word above the second
        // stair is blocked, so admission must use the same layer lookup as movement.
        var current = new SessionSnapshot(session.Current.SessionId, 1, 0, new ActiveExploration(world),
            session.Current.Story, SessionStopReason.PlayerInput);
        var captured = OpeningSave.Capture(definition, current, session.Rules, current.SessionId, current.Revision);
        Assert.Null(captured.Error);
        Assert.Equal((byte)2, captured.Data!.Layer);
        var resumed = Restore(definition, captured.Data);
        world = resumed.Current.Exploration!;
        Assert.Equal(new MapPosition(x, y), world.PlayerEntity.Position);
        Assert.All(world.AllEntities.Where(e => e.Follower is not null), e =>
        {
            Assert.Equal((byte)2, e.Motion.Layer);
            Assert.Equal(world.PlayerEntity.Position, e.Position);
        });
        Assert.Contains(world.AllEntities, e => e.Follower is not null);
        Assert.Equal(captured.Data.MainSeed, world.Party.MainSeed);
        Assert.Equal(captured.Data.ThinkingSeed, world.Party.ThinkingSeed);
        var recaptured = resumed.CaptureSave(resumed.Current.SessionId, resumed.Current.Revision);
        Assert.Null(recaptured.Error);
        Assert.Equal(System.Text.Json.JsonSerializer.Serialize(captured.Data),
            System.Text.Json.JsonSerializer.Serialize(recaptured.Data));
        Assert.False(MapEventDispatcher.Move(world, ExplorationDirection.North, resumed.Current.Story.Flags).Outcome.Moved);
        if (x == 5) world = Step(world, ExplorationDirection.East);
        world = Step(world, ExplorationDirection.East);
        Assert.Equal(new MapPosition(7, 3), world.PlayerEntity.Position);
        Assert.Equal((byte)0, world.PlayerEntity.Motion.Layer);
    }

    [Theory]
    [InlineData(1)] [InlineData(3)] [InlineData(255)]
    public void UnsupportedLayerRejectsWithoutReplacingTheLiveSession(byte layer)
    {
        var (definition, data) = Fixture(255, 0, 30);
        var session = Restore(definition, data);
        var before = session.Current;
        var failed = Assert.IsType<SessionStartFailed>(GameSession.Restore(definition, data with { Layer = layer }, session.Rules));
        Assert.Equal("save-player-layer", failed.Failure.Code);
        Assert.Same(before, session.Current);
    }

    [Theory]
    [InlineData(6, 2)] // Secondary collision, primary passable.
    [InlineData(40, 40)] // Offset escapes the layout.
    public void SecondaryLandingMustBeLegalInItsRebuiltLayout(int x, int y)
    {
        var (definition, data) = Fixture(255, 0, 30);
        var failed = Assert.IsType<SessionStartFailed>(GameSession.Restore(definition,
            data with { Layer = 2, X = x, Y = y }, RuleCompositions.Sf2()));
        Assert.Equal("save-anchor-unusable", failed.Failure.Code);
    }

    [Fact]
    public void RelocatedPrimaryRecipeCannotSilentlyCarryASecondaryPosition()
    {
        var (definition, data) = OpeningRestoreTests.Fixture();
        var session = Restore(definition, data);
        var world = session.Current.Exploration!;
        var player = world.PlayerEntity;
        world = world.WithEntity(player with { Motion = player.Motion with
            { X = 55 * 384, XDestination = 55 * 384, Layer = 2 } });
        var current = new SessionSnapshot(session.Current.SessionId, 1, 0, new ActiveExploration(world),
            session.Current.Story, SessionStopReason.PlayerInput);
        Assert.Equal("save-player-layer", OpeningSave.Query(definition, current, session.Rules).Error);
    }

    private static GameSession Restore(ScenarioDefinition definition, OpeningSaveData data) =>
        Assert.IsType<SessionStarted>(GameSession.Restore(definition, data, RuleCompositions.Sf2())).Session;

    private static ExplorationState Step(ExplorationState world, ExplorationDirection direction)
    {
        var moved = MapEventDispatcher.Move(world, direction, []);
        Assert.True(moved.Outcome.Moved);
        world = moved.World;
        for (int i = 0; i < 64; i++) world = EntityActionRunner.Tick(world).World;
        Assert.False(world.PlayerEntity.Motion.IsMoving);
        return world;
    }

    private static (ScenarioDefinition, OpeningSaveData) Fixture(int plane, int ox, int oy)
    {
        var (original, data) = OpeningRestoreTests.Fixture(true);
        var words = new ushort[4096];
        words[4 * 64 + 5] = 0x2000;
        words[3 * 64 + 6] = 0xC000;
        words[(4 + oy) * 64 + 5 + ox] = 0x8000;
        words[(3 + oy) * 64 + 6 + ox] = 0x8000;
        words[(3 + oy) * 64 + 7 + ox] = 0x2400;
        words[(3 + oy) * 64 + 5 + ox] = words[(2 + oy) * 64 + 6 + ox] = 0xC000;
        var old = original.Exploration!;
        var map = old.Maps[new("map-22")];
        var layered = new ExplorationMapDefinition(map.Map, new(words), map.Traversal, map.Entities, map.Events,
            onLoad: map.OnLoad, population: map.Population, entryFlags: map.EntryFlags, basePalette: map.BasePalette,
            viewAreas: [new(0, 0, 63, 63, plane == 0 ? ox : 0, plane == 0 ? oy : 0,
                plane == 0 ? 0 : ox, plane == 0 ? 0 : oy, 256, 256, 256, 256, 0, 0, 0, 0, plane)]);
        var world = new ExplorationDefinition(old.Maps.Values.Select(m => m.Map == map.Map ? layered : m),
            old.Programs.Values, provenance: old.Provenance, partyFlags: old.PartyFlags, visuals: old.Visuals);
        var definition = new ScenarioDefinition(original.Package, original.Encounters.Values, exploration: world)
            { Persistence = original.Persistence };
        return (definition, data with { Recipe = "desktop-kingdom", Map = "map-22", X = 4, Y = 4, Facing = 0,
            Flags = data.Flags.Where(f => f != 256).Append(776).ToArray(), EgressMap = "map-22" });
    }
}
