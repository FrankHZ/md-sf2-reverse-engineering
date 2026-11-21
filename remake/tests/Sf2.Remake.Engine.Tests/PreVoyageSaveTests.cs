using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Gameplay.Sf2;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Content.Persistence;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class PreVoyageSaveTests
{
    private static (ScenarioDefinition Definition, OpeningSaveData Data) Fixture()
    {
        var (original, data) = OpeningRestoreTests.Fixture(true, true);
        var old = original.Exploration!;
        var basis = old.Maps[new("map-57")];
        var map = new ExplorationMapDefinition(new("map-58"), basis.Layout, basis.Traversal, [], [],
            onLoad: new("dangerous-onload", 0),
            population: new(30, 128, 0, [new(74, 31, 31), new(74, 30, 30), new(74, 159, 59)]),
            basePalette: basis.BasePalette, viewAreas: basis.ViewAreas);
        var world = new ExplorationDefinition(old.Maps.Values.Append(map), old.Programs.Values,
            provenance: old.Provenance, partyFlags: old.PartyFlags, visuals: old.Visuals);
        var definition = new ScenarioDefinition(original.Package, original.Encounters.Values, exploration: world)
            { Persistence = new Sf2OpeningRestore() };
        return (definition, data with { Recipe = "battle07-return", Map = "map-58", X = 13, Y = 35, Facing = 3,
            Flags = data.Flags.Except([256, 401, 66]).Concat([74, 385, 506, 507, 640]).Order().ToArray(),
            EgressMap = "map-16", Names = new Dictionary<string, string> { ["ally-1"] = "Shelly" },
            Allies = data.Allies.Select(ally => ally.Actor == "ally-1" ? ally with { Hp = 0, Status = 3 } : ally).ToArray() });
    }

    [Fact]
    public void TowerAftermathCapturesAndRestoresProgressWithoutHealingOrReplayingTheScene()
    {
        var (definition, data) = Fixture();
        var session = Assert.IsType<SessionStarted>(GameSession.Restore(definition, data, RuleCompositions.Sf2())).Session;
        Assert.True(session.Current.CanWaitAtInput);
        Assert.DoesNotContain(999, session.Current.Story.Flags);
        Assert.Equal(4, session.Current.Exploration!.CurrentEntities.Count);
        Assert.Equal(2, session.Current.Exploration.Party.ActiveAllies!.Count);
        Assert.Equal(0, session.Current.Exploration.Party.Actors.Single(a => a.Actor.Value == "ally-1").Hp);
        var captured = session.CaptureSave(session.Current.SessionId, session.Current.Revision);
        Assert.Null(captured.Error);
        var bytes = OpeningSaveJson.Write(new(1, 1, 1, DateTimeOffset.UnixEpoch, captured.Data!));
        var decoded = OpeningSaveJson.Read(bytes, 1).Data;
        var again = Assert.IsType<SessionStarted>(GameSession.Restore(definition, decoded, session.Rules)).Session;
        var recaptured = again.CaptureSave(again.Current.SessionId, again.Current.Revision);
        Assert.Null(recaptured.Error);
        Assert.Equal(bytes, OpeningSaveJson.Write(new(1, 1, 1, DateTimeOffset.UnixEpoch, recaptured.Data!)));
        Assert.Equal(data.MainSeed, again.Current.Exploration!.Party.MainSeed);
        Assert.Equal(data.ThinkingSeed, again.Current.Exploration.Party.ThinkingSeed);
        Assert.Equal(data.Gold, again.Current.Exploration.Party.Gold);
        Assert.Equal("Shelly", again.Current.Story.Names["ally-1"]);
    }

    [Theory]
    [InlineData(74)] [InlineData(507)] [InlineData(640)]
    public void MissingAftermathPredicatesCannotSelectTheNewRecipe(int flag)
    {
        var (definition, data) = Fixture();
        var failed = Assert.IsType<SessionStartFailed>(GameSession.Restore(definition,
            data with { Flags = data.Flags.Where(value => value != flag).ToArray() }, RuleCompositions.Sf2()));
        Assert.Equal("save-location-unavailable", failed.Failure.Code);
    }

    [Theory]
    [InlineData(407)] [InlineData(999)]
    public void ArmedBattleOrSubstitutedMapCannotRestoreTheTowerReturn(int flag)
    {
        var (definition, data) = Fixture();
        var failed = Assert.IsType<SessionStartFailed>(GameSession.Restore(definition,
            data with { Flags = data.Flags.Append(flag).ToArray() }, RuleCompositions.Sf2()));
        Assert.Equal("save-location-unavailable", failed.Failure.Code);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void VillageSaveRestoresItsSettledVisitAndChurchRecord(bool revisit, bool following)
    {
        var session = StartVillage(revisit, following);
        var before = session.Current;
        var ordinary = session.CaptureSave(before.SessionId, before.Revision);
        Assert.Null(ordinary.Error);
        Assert.Equal("map-3", ordinary.Data!.EgressMap);
        Assert.DoesNotContain(399, ordinary.Data.Flags);

        // A failed publication leaves the ordinary session and its egress register in place.
        var rejected = session.RecordChurchSave(before.SessionId, before.Revision, _ => false);
        Assert.Null(rejected.Error);
        Assert.Same(before, session.Current);
        var recorded = session.RecordChurchSave(before.SessionId, before.Revision, _ => true);
        Assert.Null(recorded.Error);
        Assert.Equal("map-5", recorded.Data!.EgressMap);
        Assert.Contains(399, recorded.Data.Flags);

        var bytes = OpeningSaveJson.Write(new(1, 1, 1, DateTimeOffset.UnixEpoch, recorded.Data));
        var resumed = Assert.IsType<SessionStarted>(GameSession.Restore(session.Definition,
            OpeningSaveJson.Read(bytes, 1).Data, session.Rules)).Session;
        Assert.NotEqual(before.SessionId, resumed.Current.SessionId);
        Assert.NotNull(resumed.Current.Exploration);
        Assert.True(resumed.Current.CanWaitAtInput);
        Assert.Equal(new MapPosition(6, 6), resumed.Current.Exploration!.PlayerEntity.Position);
        var resident = Assert.Single(resumed.Current.Exploration.AllEntities, entity => entity.Slot != 0);
        Assert.Equal(revisit ? "entity-129" : "entity-128", resident.Entity.Value);
        Assert.Equal(following, resumed.Current.Exploration.AllEntities.Any(entity => entity.Entity.Value == "entity-128" &&
            entity.Follower is { LeaderSlot: 0, OffsetX: -24 }));
        Assert.Equal(recorded.Data.Flags, resumed.Current.Story.Flags);
        var roundtrip = resumed.CaptureSave(resumed.Current.SessionId, resumed.Current.Revision);
        Assert.Null(roundtrip.Error);
        Assert.Equal(bytes, OpeningSaveJson.Write(new(1, 1, 1, DateTimeOffset.UnixEpoch, roundtrip.Data!)));
    }

    [Fact]
    public void VillageRestoreRejectsAnUnavailableSetupAndSaveRejectsActiveMovement()
    {
        var session = StartVillage(false, includeRevisitSetup: false);
        var before = session.Current;
        var slot = Assert.IsType<OpeningSaveData>(session.CaptureSave(before.SessionId, before.Revision).Data);
        var wrongPhase = slot with { Flags = slot.Flags.Append(650).ToArray() };
        var failed = Assert.IsType<SessionStartFailed>(GameSession.Restore(session.Definition, wrongPhase, session.Rules));
        Assert.Equal("save-location-unavailable", failed.Failure.Code);

        var result = session.Submit(new(before.SessionId, before.Revision, null, new Move(ExplorationDirection.North)));
        Assert.Null(result.Failure);
        Assert.False(session.QuerySave().Allowed);
    }

    private static GameSession StartVillage(bool revisit, bool following = false, bool includeRevisitSetup = true)
    {
        var (original, data) = OpeningRestoreTests.Fixture(true);
        var source = Assert.IsType<SessionStarted>(GameSession.Restore(original, data, RuleCompositions.Sf2())).Session.Current;
        var basis = original.Exploration!.Maps[new("map-3")];
        var mapId = new MapId("map-5");
        var replacement = new MapSetupId("ms-map5-flag650");
        var guide = new ExplorationEntityDefinition(new("entity-128"), new(5, 6), 0, 32, Sprite: 32);
        var resident = new ExplorationEntityDefinition(new("entity-129"), new(7, 8), 1, 32, Sprite: 33);
        var population = new ExplorationPopulation(30, 128, 0, []);
        var map = new ExplorationMapDefinition(mapId, basis.Layout, basis.Traversal, [guide], [],
            onLoad: following ? new("guide-arrival", 0) : null, population: population, basePalette: basis.BasePalette, viewAreas: basis.ViewAreas,
            setup: new(new("ms-map5"), [new(new("650"), replacement)]),
            setups: includeRevisitSetup ? [new(replacement, [resident], [], null, null, population with { NonAllyStart = 129 })] : [])
            { ChurchSavepoint = new(mapId, new(3, 3), 1) };
        var originalVisuals = original.Exploration.Visuals!;
        var visuals = originalVisuals with
        {
            Maps = new Dictionary<MapId, ExplorationMapVisual>(originalVisuals.Maps)
                { [mapId] = originalVisuals.Maps[basis.Map] with { Map = mapId } },
            Sprites = new[] { 0, 32, 33 }.ToDictionary(id => id, id => new ExplorationSpriteVisual(id, [], null, 0))
        };
        var visited = new StoryProgram("guide-arrival",
            [new FollowEntity(guide.Entity, new("entity-0"), -24, 0), new EndProgram()]);
        var world = new ExplorationDefinition(original.Exploration.Maps.Values.Append(map), [visited],
            provenance: original.Exploration.Provenance, partyFlags: original.Exploration.PartyFlags, visuals: visuals);
        var definition = new ScenarioDefinition(original.Package, original.Encounters.Values, exploration: world)
            { Persistence = original.Persistence };
        var flags = source.Story.Flags.Concat(revisit ? [650] : following ? [72] : Array.Empty<int>());
        var start = new ExplorationStartInput(mapId, new("entity-0"), new(6, 6), 0, 32, flags,
            source.Exploration!.Party, display: source.Story.Display, textSettings: source.Story.TextSettings,
            stepCounter: data.StepCounter, egressMap: new(data.EgressMap));
        return Assert.IsType<SessionStarted>(GameSession.Start(definition, start, RuleCompositions.Sf2())).Session;
    }
}
