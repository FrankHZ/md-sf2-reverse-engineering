using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Content.Persistence;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class CastleSaveTests
{
    private static readonly MapId Castle = new("map-19");

    [Theory]
    [InlineData(false, "map-43")]
    [InlineData(true, "map-16")]
    public void ContinueRetainsTheCurrentRoyalTrainAndTheFreshVisitUsesDifferentPhysicalSlots(bool revisit, string egress)
    {
        var session = Start(revisit, egress);
        if (!revisit) Assert.Equal("save-busy", session.QuerySave().Error);
        FieldEgressTests.Settle(session);
        Assert.Equal("save-location-unavailable", session.QueryChurchSave().Error);

        var saved = Capture(session);
        var scene = Assert.IsType<SavedFieldScene>(saved.FieldScene);
        Assert.Equal("granseal-castle", saved.Recipe);
        Assert.Equal("ms-map19-flag506", scene.Setup);
        Assert.Equal(egress, saved.EgressMap);
        Assert.All(new[] { 83, 130, 256, 399, 620, 621 }, flag => Assert.Contains(flag, saved.Flags));
        Assert.Equal(["ally-0"], saved.ActiveAllies);
        Assert.Equal(4321u, saved.Gold);
        Assert.Equal("BOW", saved.Names["ally-0"]);
        Assert.Equal((4, 4, 3), (saved.X, saved.Y, (int)saved.Facing));
        Assert.Equal(revisit, scene.Aliases.ContainsKey("entity-128"));
        Assert.Equal(!revisit, scene.Layout.Count > 0);
        var king = Assert.Single(scene.Actors, row => row.Entity == (revisit ? "entity-30" : "entity-130"));
        var minister = Assert.Single(scene.Actors, row => row.Entity == (revisit ? "entity-159" : "entity-129"));
        Assert.Equal(0, king.LeaderSlot);
        Assert.Equal(king.Slot, minister.LeaderSlot);

        var restored = Assert.IsType<SessionStarted>(GameSession.Restore(session.Definition,
            OpeningSaveJson.Read(Encode(saved), 1).Data, session.Rules)).Session;
        Assert.True(restored.Current.CanWaitAtInput);
        Assert.Equal(Encode(saved), Encode(Capture(restored)));

        EngineTestContent.Accept(restored, new Move(ExplorationDirection.East));
        FieldEgressTests.Settle(restored);
        Assert.Equal(new MapPosition(5, 4), restored.Current.Exploration!.PlayerEntity.Position);
        Assert.Equal(king.Slot, restored.Current.Exploration.AllEntities.Single(actor => actor.Slot == minister.Slot).Follower!.LeaderSlot);
    }

    [Fact]
    public void ContinueRejectsMissingOrIncompatibleCastleScenes()
    {
        var session = Start(revisit: true, "map-3");
        FieldEgressTests.Settle(session);
        var saved = Capture(session);
        var scene = Assert.IsType<SavedFieldScene>(saved.FieldScene);
        Assert.Equal("save-field-scene-required", Reject(saved with { FieldScene = null }));
        Assert.Equal("save-field-scene-context", Reject(saved with { FieldScene = scene with { Setup = "ms-map19" } }));
        Assert.Equal("save-location-unavailable", Reject(saved with { Flags = [.. saved.Flags, 543] }));

        string Reject(OpeningSaveData data) => Assert.IsType<SessionStartFailed>(
            GameSession.Restore(session.Definition, data, session.Rules)).Failure.Code;
    }

    private static OpeningSaveData Capture(GameSession session)
    {
        var capture = session.CaptureSave(session.Current.SessionId, session.Current.Revision);
        Assert.Null(capture.Error);
        return Assert.IsType<OpeningSaveData>(capture.Data);
    }

    private static byte[] Encode(OpeningSaveData data) =>
        OpeningSaveJson.Write(new(1, 1, 1, DateTimeOffset.UnixEpoch, data));

    private static GameSession Start(bool revisit, string egress)
    {
        var (basis, data) = OpeningRestoreTests.Fixture();
        var rules = RuleCompositions.Sf2();
        var opening = Assert.IsType<SessionStarted>(GameSession.Restore(basis, data, rules)).Session.Current;
        var source = basis.Exploration!;
        var sample = source.Maps[new("map-3")];
        var population = sample.Population! with { Followers = [new(83, 30, 203), new(83, 159, 205)] };
        var words = sample.Layout.Words.ToArray();
        words[0] = 1;
        ExplorationEntityDefinition[] residents =
        [
            new(new("entity-128"), new(3, 2), 3, 32, Sprite: 202),
            new(new("entity-129"), new(2, 4), 3, 32, Sprite: 205),
            new(new("entity-130"), new(3, 4), 3, 32, Sprite: 203),
        ];
        var castle = new ExplorationMapDefinition(Castle, new(words), sample.Traversal, [], [],
            setup: new(new("ms-map19"), [new(new("506"), new("ms-map19-flag506")), new(new("543"), new("ms-map19-flag543"))]),
            population: population, entryFlags: [new(256, false)], basePalette: sample.BasePalette, viewAreas: sample.ViewAreas,
            setups: [new(new("ms-map19-flag506"), residents, [], new("occupied-castle", 0), null, population)]);
        StoryProgram[] scenes =
        [
            new("occupied-castle", [new BranchFlag(621, true, new("revisit", 0)), new WaitProgramTicks(2),
                new HideMapEntity(new("entity-128"), true), new FollowEntity(new("entity-130"), new("entity-0"), -24, 0),
                new FollowEntity(new("entity-129"), new("entity-130"), -24, 0),
                new CopyMapLayout(new(0, 0, 7, 7, 1, 1)), new WriteFlag(620, true), new WriteFlag(621, true),
                new WriteFlag(83, true), new WriteFlag(256, true), new EndProgram()]),
            new("revisit", [new HideMapEntity(new("entity-128"), false), new HideMapEntity(new("entity-129"), false),
                new HideMapEntity(new("entity-130"), false), new WriteFlag(256, true), new EndProgram()]),
        ];
        var visuals = source.Visuals! with
        {
            Maps = new Dictionary<MapId, ExplorationMapVisual>(source.Visuals!.Maps)
                { [Castle] = source.Visuals.Maps[sample.Map] with { Map = Castle } },
            Sprites = new[] { 0, 202, 203, 205 }.ToDictionary(id => id, id => new ExplorationSpriteVisual(id, [], null, 0)),
        };
        var world = new ExplorationDefinition(source.Maps.Values.Append(castle), scenes,
            provenance: source.Provenance, partyFlags: source.PartyFlags, visuals: visuals);
        var definition = new ScenarioDefinition(basis.Package, basis.Encounters.Values, exploration: world) { Persistence = basis.Persistence };
        int[] flags = revisit ? [0, 32, 80, 83, 130, 399, 506, 620, 621] : [0, 32, 80, 130, 399, 506];
        var start = new ExplorationStartInput(Castle, new("entity-0"), new(4, 4), 3, 32,
            flags, opening.Exploration!.Party, display: opening.Story.Display, textSettings: opening.Story.TextSettings,
            stepCounter: data.StepCounter, egressMap: new(egress)) { Names = new Dictionary<string, string> { ["ally-0"] = "BOW" } };
        return Assert.IsType<SessionStarted>(GameSession.Start(definition, start, rules)).Session;
    }
}
