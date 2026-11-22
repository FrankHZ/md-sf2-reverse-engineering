using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Content.Persistence;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class TowerSaveTests
{
    [Fact]
    public void ContinuedStairVisitUsesTheOpenWorkingLayoutAndCanTakeItsWarp()
    {
        var session = Start();
        var before = session.Current;
        var saved = Assert.IsType<OpeningSaveData>(session.CaptureSave(before.SessionId, before.Revision).Data);
        Assert.Equal("tower-field", saved.Recipe);
        Assert.Equal((7, 5), (saved.X, saved.Y));
        var scene = Assert.IsType<SavedFieldScene>(saved.FieldScene);
        Assert.Contains(scene.Layout, cell => cell.Index == 4 * 64 + 7 && cell.Value == 0x1001);
        Assert.Equal("map-16", saved.EgressMap);
        Assert.Contains(399, saved.Flags);
        Assert.Contains(132, saved.Flags);
        Assert.Equal("save-location-unavailable", session.QueryChurchSave().Error);

        var bytes = OpeningSaveJson.Write(new(1, 1, 1, DateTimeOffset.UnixEpoch, saved));
        var resumed = Assert.IsType<SessionStarted>(GameSession.Restore(session.Definition,
            OpeningSaveJson.Read(bytes, 1).Data, session.Rules)).Session;
        Assert.Equal(before.Exploration!.Layout.Words, resumed.Current.Exploration!.Layout.Words);
        Assert.Contains(256, resumed.Current.Story.Flags);
        EngineTestContent.Accept(resumed, new Move(ExplorationDirection.North));
        Assert.False(resumed.QuerySave().Allowed);
        FieldEgressTests.Settle(resumed);
        Assert.Equal(new MapId("map-60"), resumed.Current.Exploration!.Map);
        Assert.Equal(new MapPosition(53, 1), resumed.Current.Exploration.PlayerEntity.Position);
        Assert.True(resumed.QuerySave().Allowed);
    }

    [Fact]
    public void SettledStairSaveRejectsUnboundLayoutAndOutOfScopeFlags()
    {
        var session = Start();
        var current = session.Current;
        var saved = Assert.IsType<OpeningSaveData>(session.CaptureSave(current.SessionId, current.Revision).Data);
        Assert.Equal("save-field-scene-layout", Reject(saved with { FieldScene = saved.FieldScene! with { Layout = [] } }));
        Assert.Equal("save-location-unavailable", Reject(saved with { Flags = [.. saved.Flags, 401] }));
        Assert.Equal("save-location-unavailable", Reject(saved with { Flags = [.. saved.Flags, 999] }));

        string Reject(OpeningSaveData data) => Assert.IsType<SessionStartFailed>(
            GameSession.Restore(session.Definition, data, session.Rules)).Failure.Code;
    }

    private static GameSession Start()
    {
        var (basis, legacy) = OpeningRestoreTests.Fixture(victory: true);
        var rules = RuleCompositions.Sf2();
        var prior = Assert.IsType<SessionStarted>(GameSession.Restore(basis, legacy, rules)).Session.Current;
        var world = basis.Exploration!;
        var stair = world.Maps[new("map-57")];
        var descent = new MapId("map-60");
        var words = stair.Layout.Words.ToArray();
        words[17] = 0x1001;
        words[4 * 64 + 7] = 0xC000;
        var pop = stair.Population! with { Followers = [] };
        var map57 = new ExplorationMapDefinition(stair.Map, new(words), stair.Traversal, [],
            [new(ExplorationEventKind.Warp, 7, 4, null, null, descent, new(53, 1), RequiredMarker: 0x1000)],
            onLoad: new("stair-entry", 0), population: pop,
            layoutEvents: new([], [new(506, new(17, 0, 7, 4, 2, 1))], new([])),
            entryFlags: stair.EntryFlags, basePalette: stair.BasePalette, viewAreas: stair.ViewAreas);
        var map60 = new ExplorationMapDefinition(descent, stair.Layout, stair.Traversal, [], [], population: pop,
            basePalette: stair.BasePalette, viewAreas: stair.ViewAreas);
        var visuals = world.Visuals! with
        {
            Maps = new Dictionary<MapId, ExplorationMapVisual>(world.Visuals!.Maps)
                { [descent] = world.Visuals.Maps[stair.Map] with { Map = descent } },
            Sprites = new Dictionary<int, ExplorationSpriteVisual> { [0] = new(0, [], null, 0) },
        };
        var maps = world.Maps.Values.Where(map => map.Map != stair.Map).Append(map57).Append(map60);
        var programs = world.Programs.Values.Append(new("stair-entry", [new WriteFlag(256, true), new EndProgram()]));
        var definition = new ScenarioDefinition(basis.Package, basis.Encounters.Values,
            exploration: new(maps, programs, provenance: world.Provenance, partyFlags: world.PartyFlags, visuals: visuals))
            { Persistence = basis.Persistence };
        var start = new ExplorationStartInput(stair.Map, new("entity-0"), new(7, 5), 1, 32,
            [0, 32, 1, 33, 80, 132, 399, 501, 506], prior.Exploration!.Party,
            display: prior.Story.Display, textSettings: prior.Story.TextSettings, stepCounter: 27, egressMap: new("map-16"));
        return Assert.IsType<SessionStarted>(GameSession.Start(definition, start, rules)).Session;
    }
}
