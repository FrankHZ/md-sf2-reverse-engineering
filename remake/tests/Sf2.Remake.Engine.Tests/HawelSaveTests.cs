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

public sealed class HawelSaveTests
{
    private static readonly MapId House = new("map-43");

    [Theory]
    [InlineData(false, "ms-map43", "map-43")]
    [InlineData(true, "ms-map43-flag612", "map-3")]
    public void ContinueRebuildsTheLoadedVisitWithoutReplayingTheHouseProgram(bool revisit, string setup, string egress)
    {
        var session = Start(revisit);
        if (!revisit) Assert.Equal("save-busy", session.QuerySave().Error);
        FieldEgressTests.Settle(session);

        var before = session.Current;
        Assert.Equal("save-location-unavailable", session.QueryChurchSave().Error);
        var capture = session.CaptureSave(before.SessionId, before.Revision);
        Assert.Null(capture.Error);
        var saved = Assert.IsType<OpeningSaveData>(capture.Data);
        var scene = Assert.IsType<SavedFieldScene>(saved.FieldScene);
        Assert.Equal("hawel-field", saved.Recipe);
        Assert.Equal(setup, scene.Setup);
        Assert.Equal(egress, saved.EgressMap);
        Assert.Equal(revisit, saved.Flags.Contains(399));
        Assert.Equal(!revisit, saved.Flags.Contains(404));
        Assert.Contains(4, saved.Flags);
        Assert.Contains(36, saved.Flags);
        Assert.DoesNotContain(72, saved.Flags);
        Assert.Single(saved.ActiveAllies, id => id == "ally-4");
        Assert.Single(saved.Allies, ally => ally.Actor == "ally-4");
        Assert.Equal(!revisit, scene.Actors.Any(actor => actor.Entity == "entity-4" && actor.LeaderSlot == 0));
        Assert.Equal(!revisit, scene.Actors.Any(actor => actor.Entity == "entity-128" && !actor.Visible));
        Assert.Equal(!revisit, scene.Layout.Count > 0);

        var bytes = Encode(saved);
        var restored = Assert.IsType<SessionStarted>(GameSession.Restore(session.Definition,
            OpeningSaveJson.Read(bytes, 1).Data, session.Rules)).Session;
        Assert.True(restored.Current.CanWaitAtInput);
        Assert.Equal(before.Exploration!.PlayerEntity.Position, restored.Current.Exploration!.PlayerEntity.Position);
        Assert.Equal(bytes, Encode(restored.CaptureSave(restored.Current.SessionId, restored.Current.Revision).Data!));

        Assert.Equal("save-field-scene-required", Reject(saved with { FieldScene = null }));
        Assert.Equal("save-field-scene-actor", Reject(saved with
        {
            FieldScene = scene with { Setup = revisit ? "ms-map43" : "ms-map43-flag612" },
        }));
        Assert.Equal("save-location-unavailable", Reject(saved with { Flags = [.. saved.Flags, 72] }));

        string Reject(OpeningSaveData data) => Assert.IsType<SessionStartFailed>(
            GameSession.Restore(session.Definition, data, session.Rules)).Failure.Code;

        static byte[] Encode(OpeningSaveData data) =>
            OpeningSaveJson.Write(new(1, 1, 1, DateTimeOffset.UnixEpoch, data));
    }

    private static GameSession Start(bool revisit)
    {
        var (basis, data) = OpeningRestoreTests.Fixture();
        var rules = RuleCompositions.Sf2();
        var opening = Assert.IsType<SessionStarted>(GameSession.Restore(basis, data, rules)).Session.Current;
        var source = basis.Exploration!;
        var sample = source.Maps[new("map-3")];
        var battle = basis.Encounters.Values.Single();
        var guide = new BattleActorDefinition(new("ally-4"), BattleClassRule.UnpromotedSwordsman, 1,
            20, 5, 8, 4, 6, false, 6, [], sourceLoadout: new([127, 127, 127, 127], [63, 63, 63, 63]),
            growth: battle.Deployments[0].Definition.Growth);
        battle = battle.WithDeployments([.. battle.Deployments, new(guide, BattleFaction.Ally, 4, BattleControl.Player,
            null, new(3, 1), new(null, 0, 0, 0, 0, AllyPartyMember: 4))]);

        var population = sample.Population! with { Followers = [new(72, 4, 4)] };
        var words = sample.Layout.Words.ToArray();
        words[0] = 1;
        var house = new ExplorationMapDefinition(House, new(words), sample.Traversal,
            [new(new("entity-128"), new(7, 4), 3, 32, Sprite: 192)], [], onLoad: new("house-intro", 0),
            setup: new(new("ms-map43"), [new(new("612"), new("ms-map43-flag612"))]), population: population,
            basePalette: sample.BasePalette, viewAreas: sample.ViewAreas,
            setups: [new(new("ms-map43-flag612"), [], [], null, null, population)]);
        var scene = new StoryProgram("house-intro",
            [new WaitProgramTicks(2), new HideMapEntity(new("entity-128"), true), new JoinPartyMember(4),
             new FollowEntity(new("entity-4"), new("entity-0"), -24, 0),
             new CopyMapLayout(new(0, 0, 7, 7, 1, 1)), new WriteFlag(72, false), new WriteFlag(404, true),
             new WriteFlag(612, true), new WriteFlag(650, true), new SetEgressMap(House), new EndProgram()]);
        var visuals = source.Visuals! with
        {
            Maps = new Dictionary<MapId, ExplorationMapVisual>(source.Visuals!.Maps)
                { [House] = source.Visuals.Maps[sample.Map] with { Map = House } },
            Sprites = new[] { 0, 4, 192 }.ToDictionary(id => id, id => new ExplorationSpriteVisual(id, [], null, 0)),
        };
        var world = new ExplorationDefinition(source.Maps.Values.Append(house), [scene],
            provenance: source.Provenance, partyFlags: source.PartyFlags, visuals: visuals);
        var definition = new ScenarioDefinition(basis.Package, [battle], exploration: world) { Persistence = basis.Persistence };
        var initial = opening.Exploration!.Party;
        var party = new BattleStartInput(initial.Encounter,
            [.. initial.Actors, new(guide.Actor, 7, 2, 0, 0, 0, 0, null,
                new(1, 20, 5, 8, 4, 6, [], guide.SourceLoadout), guide.SourceLoadout, ActivationWord: 0, Initialized: true)],
            initial.MainSeed, initial.ThinkingSeed, initial.Gold, initial.NewBattle,
            activeAllies: revisit ? [new("ally-0"), guide.Actor] : [new("ally-0")], itemCustody: initial.ItemCustody);
        int[] flags = revisit ? [0, 4, 32, 36, 80, 399, 612, 650] : [0, 32, 72, 80];
        var start = new ExplorationStartInput(House, new("entity-0"), new(6, 6), 2, 32,
            flags, party, display: opening.Story.Display, textSettings: opening.Story.TextSettings,
            stepCounter: data.StepCounter, egressMap: new("map-3"));
        return Assert.IsType<SessionStarted>(GameSession.Start(definition, start, rules)).Session;
    }
}
