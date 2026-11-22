using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Gameplay.Sf2;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class GalamSaveTests
{
    private static readonly MapId Town = new("map-16");
    private static readonly FieldReturnAnchor Refuge = new(Town, new(27, 7), 1);

    [Fact]
    public void ContinueKeepsTheChangedDefaultVisitAndRecordPublishesItsSeparateRefuge()
    {
        var session = Start(includeChurch: true);
        Assert.Equal("ms-map16", Capture(session).FieldScene!.Setup);

        Accept(session, new Move(ExplorationDirection.East));
        Assert.Equal("save-busy", session.QuerySave().Error);
        FieldEgressTests.Settle(session);
        Assert.Contains(663, session.Current.Story.Flags);

        var saved = Capture(session);
        Assert.Equal("ms-map16", saved.FieldScene!.Setup);
        Assert.DoesNotContain("entity-129", saved.FieldScene.Aliases.Keys);
        Assert.Contains(saved.FieldScene.Actors, actor => actor.Entity == "entity-129" && !actor.Visible);
        Assert.Equal("map-43", saved.EgressMap);
        Assert.DoesNotContain(399, saved.Flags);

        var continued = Restore(session, saved);
        var before = continued.Current;
        Assert.Equal(new MapPosition(25, 7), before.Exploration!.PlayerEntity.Position);
        Assert.True(before.Exploration.TryResolveEntity(new("entity-128"), out var resident));
        Assert.Equal(new MapPosition(25, 6), resident.Position);
        Assert.False(before.Exploration.TryResolveEntity(new("entity-129"), out _));
        Assert.Equal("ms-map16", Capture(continued).FieldScene!.Setup);
        Assert.Equal("save-field-scene-context", Reject(session, saved with
        {
            FieldScene = saved.FieldScene with { Setup = "unknown" },
        }));
        Assert.Equal("save-field-scene-actor", Reject(session, saved with
        {
            FieldScene = saved.FieldScene with { Setup = "ms-map16-flag663" },
        }));

        var offer = continued.QueryChurchSave();
        Assert.True(offer.Allowed);
        Assert.Same(before, continued.Current);
        Assert.Equal("save-stale-revision", continued.RecordChurchSave(offer.SessionId, offer.Revision - 1,
            _ => throw new InvalidOperationException("Stale Record reached publication")).Error);
        var candidate = continued.RecordChurchSave(offer.SessionId, offer.Revision, _ => false);
        Assert.Equal("map-16", candidate.Data!.EgressMap);
        Assert.Contains(399, candidate.Data.Flags);
        Assert.Same(before, continued.Current);

        var committed = continued.RecordChurchSave(offer.SessionId, offer.Revision, _ => true);
        Assert.Equal(Refuge, Sf2OutcomeReturnPolicy.FindEgress(continued.Definition, continued.Current.Story.EgressMap!));
        Assert.Contains(399, continued.Current.Story.Flags);
        var recordReader = Restore(continued, committed.Data!);
        Assert.Equal(new MapPosition(25, 7), recordReader.Current.Exploration!.PlayerEntity.Position);
        Accept(recordReader, new Move(ExplorationDirection.South));
        FieldEgressTests.Settle(recordReader);
        Assert.Equal(new MapPosition(25, 8), recordReader.Current.Exploration!.PlayerEntity.Position);
    }

    [Fact]
    public void FreshAlternateVisitRequiresItsSceneAndOnlyRecordRequiresTheRefuge()
    {
        var session = Start(includeChurch: false, freshAlternate: true);
        var saved = Capture(session);
        Assert.Equal("ms-map16-flag663", saved.FieldScene!.Setup);
        Assert.Equal("save-location-unavailable", session.QueryChurchSave().Error);
        Assert.Equal("save-field-scene-required", Reject(session, saved with { FieldScene = null }));
        Assert.Equal("save-field-scene-actor", Reject(session, saved with
        {
            FieldScene = saved.FieldScene with { Setup = "ms-map16" },
        }));
        Assert.Equal("save-location-unavailable", Reject(session, saved with { Flags = [.. saved.Flags, 530] }));
        Assert.Equal("save-location-unavailable", Reject(session, saved with { Flags = [.. saved.Flags, 999] }));

        var continued = Restore(session, saved);
        Assert.Equal("ms-map16-flag663", Capture(continued).FieldScene!.Setup);
        Assert.True(continued.Current.Exploration!.TryResolveEntity(new("entity-128"), out var resident));
        Assert.Equal(new MapPosition(26, 4), resident.Position);
    }

    [Fact]
    public void IdenticalRallyFlagsKeepEachVisitsActualTalkBinding()
    {
        var changed = Start(includeChurch: true);
        var atRally = FieldVisitSnapshotTests.Perform(changed, changed.Current, new Move(ExplorationDirection.East));
        var defaultVisit = FieldVisitSnapshotTests.Perform(changed, atRally, new Move(ExplorationDirection.North));
        var fresh = Start(includeChurch: true, freshAlternate: true, position: new(26, 5));
        Assert.Equal(defaultVisit.Story.Flags, fresh.Current.Story.Flags);

        foreach (var (session, live, setup, talkFlag) in new[]
        {
            (changed, defaultVisit, "ms-map16", 900),
            (fresh, fresh.Current, "ms-map16-flag663", 901),
        })
        {
            var visit = FieldVisitSnapshot.Capture(live);
            var unknownSetup = visit with { Body = visit.Body with { Setup = new("unknown") } };
            Assert.Throws<Sf2.Remake.Domain.Battles.BattleRuleException>(() =>
                unknownSetup.Rebuild(session.Definition.Exploration!, live.Exploration!.Party));
            var detached = FieldVisitSnapshotTests.Rebuild(session, live);
            var world = detached.Exploration!;
            Assert.Equal(setup, world.Definition.Body.Setup!.Value);
            Assert.Equal(Refuge, world.Definition.ChurchSavepoint);
            var target = new EntityRef("entity-128");
            Assert.Equal(target, ExplorationInteractionQuery.Query(detached));
            foreach (var before in new[] { live, detached })
            {
                var after = FieldVisitSnapshotTests.Perform(session, before, new Interact(target));
                Assert.Contains(talkFlag, after.Story.Flags);
                Assert.DoesNotContain(talkFlag == 900 ? 901 : 900, after.Story.Flags);
            }
        }
    }

    private static OpeningSaveData Capture(GameSession session)
    {
        var snapshot = session.Current;
        var capture = session.CaptureSave(snapshot.SessionId, snapshot.Revision);
        Assert.Null(capture.Error);
        return Assert.IsType<OpeningSaveData>(capture.Data);
    }

    private static GameSession Restore(GameSession session, OpeningSaveData data) =>
        Assert.IsType<SessionStarted>(GameSession.Restore(session.Definition, data, session.Rules)).Session;

    private static string Reject(GameSession session, OpeningSaveData data) =>
        Assert.IsType<SessionStartFailed>(GameSession.Restore(session.Definition, data, session.Rules)).Failure.Code;

    private static GameSession Start(bool includeChurch, bool freshAlternate = false, MapPosition? position = null)
    {
        var (original, data) = OpeningRestoreTests.Fixture();
        var source = original.Exploration!;
        var sample = source.Maps[new("map-3")];
        var map = new ExplorationMapDefinition(Town, sample.Layout, sample.Traversal,
            [new(new("entity-128"), new(25, 6), 3, 32, Sprite: 192),
             new(new("entity-129"), new(28, 12), 3, 32, Sprite: 193)],
            [new(ExplorationEventKind.Step, 25, 7, null, new("close-gathering", 0)),
                new(ExplorationEventKind.Interact, null, null, new("entity-128"), new("usual-talk", 0))],
            population: sample.Population, basePalette: sample.BasePalette, viewAreas: sample.ViewAreas,
            setup: new(new("ms-map16"), [new(new("663"), new("ms-map16-flag663"))]),
            setups: [new(new("ms-map16-flag663"),
                [new(new("entity-128"), new(26, 4), 3, 32, Sprite: 193)],
                [new(ExplorationEventKind.Interact, null, null, new("entity-128"), new("alternate-talk", 0))],
                null, null, sample.Population!)])
            { ChurchSavepoint = includeChurch ? Refuge : null };
        var visuals = source.Visuals! with
        {
            Maps = new Dictionary<MapId, ExplorationMapVisual>(source.Visuals!.Maps)
                { [Town] = source.Visuals.Maps[sample.Map] with { Map = Town } },
            Sprites = new[] { 0, 192, 193 }.ToDictionary(id => id, id => new ExplorationSpriteVisual(id, [], null, 0)),
        };
        var programs = source.Programs.Values.Concat([
            new("close-gathering", [new HideMapEntity(new("entity-129"), true), new WriteFlag(663, true), new EndProgram()]),
            new("usual-talk", [new WriteFlag(900, true), new EndProgram()]),
            new("alternate-talk", [new WriteFlag(901, true), new EndProgram()]),
        ]);
        var world = new ExplorationDefinition(source.Maps.Values.Append(map), programs, provenance: source.Provenance,
            partyFlags: source.PartyFlags, visuals: visuals);
        var definition = new ScenarioDefinition(original.Package, original.Encounters.Values, exploration: world)
            { Persistence = new Sf2OpeningRestore() };
        var rules = RuleCompositions.Sf2();
        var opening = Assert.IsType<SessionStarted>(GameSession.Restore(definition, data, rules)).Session.Current;
        var start = new ExplorationStartInput(Town, new("entity-0"), position ?? new(24, 7), (byte)(position is null ? 2 : 1), 32,
            freshAlternate ? [.. opening.Story.Flags, 663] : opening.Story.Flags,
            opening.Exploration!.Party, display: opening.Story.Display, textSettings: opening.Story.TextSettings,
            stepCounter: data.StepCounter, egressMap: new("map-43"));
        return Assert.IsType<SessionStarted>(GameSession.Start(definition, start, rules)).Session;
    }
}
