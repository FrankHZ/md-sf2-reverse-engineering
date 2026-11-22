using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Gameplay.Sf2;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Content.Persistence;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.TestSupport;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class Map3SaveSetupTests
{
    [PrivateInputFact("SF2_PRIVATE_EXPLORATION_CONTENT", "SF2_PRIVATE_STATIC_DATA", "SF2_PRIVATE_ENEMY_DATA", "SF2_PRIVATE_ENEMY_GOLD",
        "SF2_PRIVATE_BATTLE01_DATA", "SF2_PRIVATE_BATTLE01_SCENE", "SF2_PRIVATE_BATTLE01_TERRAIN")]
    public void AdmittedMap3SetupFamiliesKeepTheirExplicitRecoveryBoundary()
    {
        string Input(string key) => PrivateInputFactAttribute.RequireInput(key);
        var reader = new OpeningNewGameReader(Input("SF2_PRIVATE_EXPLORATION_CONTENT"), Input("SF2_PRIVATE_BATTLE01_DATA"),
            Input("SF2_PRIVATE_BATTLE01_SCENE"), Input("SF2_PRIVATE_BATTLE01_TERRAIN"), Input("SF2_PRIVATE_STATIC_DATA"),
            Input("SF2_PRIVATE_ENEMY_DATA"), Input("SF2_PRIVATE_ENEMY_GOLD"), 0, uint.MaxValue);
        var initial = Assert.IsType<SessionStarted>(GameSession.Start(reader)).Session;
        var saved = initial.CaptureSave(initial.Current.SessionId, initial.Current.Revision).Data!;
        var map = initial.Definition.Exploration!.Maps[new("map-3")];
        var families = new Dictionary<string, string?> { ["ms-map3"] = null, ["ms-map3-flag609"] = null,
            ["ms-map3-flag506"] = "save-recipe", ["ms-map3-flag543"] = "save-location-unavailable" };
        var cases = new[] { (Flag: (int?)null, Setup: map.Setup!.DefaultSetup) }.Concat(
            map.Setup.FlagAlternatives.Select(variant => (Flag: (int?)int.Parse(variant.Flag.Value), Setup: variant.Setup)));
        foreach (var phase in cases)
        {
            Assert.True(families.TryGetValue(phase.Setup.Value, out string? anchoredSaveFailure),
                "Classify new Map3 setup recovery before changing the save boundary: " + phase.Setup);
            foreach (bool joinedJaha in new[] { false, true })
            {
                int[] flags = saved.Flags.Concat([1, 2, 33, 34, 66, 501, 603, 610])
                    .Concat(phase.Flag is { } flag ? new[] { flag } : []).Concat(joinedJaha ? new[] { 3, 35 } : []).Order().ToArray();
                var data = saved with { Recipe = "map3-joined", X = 32, Y = 13, Facing = 1, Flags = flags,
                    ActiveAllies = joinedJaha ? ["ally-0", "ally-1", "ally-2", "ally-3"] : ["ally-0", "ally-1", "ally-2"] };
                var result = GameSession.Restore(initial.Definition, data, initial.Rules);
                if (anchoredSaveFailure is not null)
                {
                    Assert.Equal(anchoredSaveFailure, Assert.IsType<SessionStartFailed>(result).Failure.Code);
                    continue;
                }
                var session = Assert.IsType<SessionStarted>(result).Session;
                var world = session.Current.Exploration!;
                Assert.Equal(map.SelectSetup(flags).Entities, world.Definition.Entities);
                Assert.Equal(map.SelectSetup(flags).Events, world.Definition.Events);
                if (phase.Setup.Value == "ms-map3-flag609")
                {
                    Assert.Equal(!joinedJaha, world.TryResolveEntity(new("entity-3"), out _));
                    Assert.True(world.Entities[new("entity-142")].Visible);
                    Assert.Equal(new MapPosition(32, 27), world.Entities[new("entity-142")].Position);
                }
                Assert.Null(session.CaptureSave(session.Current.SessionId, session.Current.Revision).Error);
                Assert.DoesNotContain(999, session.Current.Story.Flags);
            }
        }
    }

    private static (ScenarioDefinition Definition, OpeningSaveData Data) Fixture(bool joinedJaha = false, bool includeChurch = false, bool includeF506 = true)
    {
        var (original, data) = OpeningRestoreTests.Fixture(true);
        var old = original.Exploration!;
        var basis = old.Maps[new("map-3")];
        ExplorationEntityDefinition Npc(string id, int x, int sprite) => new(new(id), new(x, 10), 1, 32, Sprite: sprite,
            Actions: new([new WaitEntityTicks(9), new JumpEntityAction(0)]));
        var population = basis.Population! with { NonAllyStart = 142 };
        var setups = new List<ExplorationSetupDefinition> {
            new(new("ms-map3-flag609"), [Npc("entity-3", 1, 3), Npc("entity-142", 2, 195)], [],
                new("dangerous-onload", 0), null, population)
        };
        if (includeF506)
            setups.Add(new(new("ms-map3-flag506"), [Npc("entity-6", 3, 6)], [], null, null, population));
        var map = new ExplorationMapDefinition(basis.Map, basis.Layout, basis.Traversal, basis.Entities, basis.Events,
            onLoad: basis.OnLoad, population: population, entryFlags: [new(256, false), new(260, false)],
            basePalette: basis.BasePalette, viewAreas: basis.ViewAreas,
            setup: new(new("ms-map3"), [new(new("609"), new("ms-map3-flag609")),
                new(new("506"), new("ms-map3-flag506")), new(new("543"), new("ms-map3-flag543")),
                new(new("700"), new("future-setup"))]), setups: setups)
            { ChurchSavepoint = includeChurch ? new(basis.Map, new(32, 13), 1) : null };
        var visuals = old.Visuals! with
        {
            Sprites = new[] { 0, 1, 6 }.ToDictionary(id => id, id => new ExplorationSpriteVisual(id, [], null, 0))
        };
        var world = new ExplorationDefinition([map], old.Programs.Values, provenance: old.Provenance,
            partyFlags: old.PartyFlags, visuals: visuals);
        var ally = original.Encounters.Values.First().Deployments.First().Definition;
        var jaha = new BattleDeploymentDefinition(new(new("ally-3"), ally.ClassRule, ally.Level, ally.MaxHp, ally.MaxMp,
            ally.Attack, ally.Defense, ally.Agility, false, ally.Move, [], sourceLoadout: ally.SourceLoadout, growth: ally.Growth),
            BattleFaction.Ally, 3, BattleControl.Player, null, new(1, 1), new(null, 0, 0, 0, 0, AllyPartyMember: 3));
        var battle = new BattleDefinition("opening", new("battle"), 1, 1, [],
            original.Encounters.Values.First().Deployments.Append(jaha), []);
        var definition = new ScenarioDefinition(original.Package, [battle], exploration: world)
            { Persistence = new Sf2OpeningRestore() };
        return (definition, data with { Flags = data.Flags.Concat(joinedJaha ? [3, 609, 610] : [609, 610]).ToArray(),
            Allies = data.Allies.Append(data.Allies[0] with { Actor = "ally-3" }).ToArray() });
    }

    [Theory]
    [InlineData(false, false)] [InlineData(false, true)] [InlineData(true, false)] [InlineData(true, true)]
    public void CommissionSaveRestoresItsPopulationAndProgressWithoutDefaultInitOrSceneReplay(bool joinedJaha, bool completedCommission)
    {
        var (definition, data) = Fixture(joinedJaha);
        if (!completedCommission) data = data with { Flags = data.Flags.Except([610]).ToArray() };
        var session = Assert.IsType<SessionStarted>(GameSession.Restore(definition, data, RuleCompositions.Sf2())).Session;
        var world = session.Current.Exploration!;
        Assert.Equal(new MapPosition(32, 13), world.PlayerEntity.Position);
        Assert.Equal((byte)1, world.PlayerEntity.Motion.Facing);
        Assert.Equal(new MapPosition(2, 10), world.Entities[new("entity-142")].Position);
        Assert.True(world.Entities[new("entity-142")].Visible); // Not the default messenger.
        Assert.Equal(!joinedJaha, world.TryResolveEntity(new("entity-3"), out _));
        Assert.Single(world.AllEntities, entity => entity.Follower is not null);
        Assert.DoesNotContain(999, session.Current.Story.Flags);
        Assert.DoesNotContain(256, session.Current.Story.Flags);
        var captured = session.CaptureSave(session.Current.SessionId, session.Current.Revision);
        Assert.Null(captured.Error);
        var expected = data with { Flags = data.Flags.Except([256, 260]).Order().ToArray() };
        byte[] Bytes(OpeningSaveData value) => OpeningSaveJson.Write(new(1, 1, 1, DateTimeOffset.UnixEpoch, value));
        Assert.Equal(Bytes(expected), Bytes(captured.Data!));
        var decoded = OpeningSaveJson.Read(Bytes(captured.Data!), 1).Data;
        var again = Assert.IsType<SessionStarted>(GameSession.Restore(definition, decoded, session.Rules)).Session;
        Assert.Null(EngineTestContent.Send(again, new WaitAtInput()).Failure);
        Assert.Contains(609, again.Current.Story.Flags);
        Assert.Equal(completedCommission, again.Current.Story.Flags.Contains(610));
        Assert.Equal(data.Gold, again.Current.Exploration!.Party.Gold);
        Assert.DoesNotContain(999, again.Current.Story.Flags);
    }

    [Theory]
    [InlineData(543)] [InlineData(700)]
    public void LaterAndUnknownSelectedSetupsRemainUnavailableEvenWithCommissionFlags(int flag)
    {
        var (definition, data) = Fixture();
        var failed = Assert.IsType<SessionStartFailed>(GameSession.Restore(definition,
            data with { Flags = data.Flags.Append(flag).ToArray() }, RuleCompositions.Sf2()));
        Assert.Equal("save-location-unavailable", failed.Failure.Code);
    }

    [Fact]
    public void CommissionSetupCannotUseThePreMessengerHomeRecipe()
    {
        var (definition, data) = Fixture();
        var failed = Assert.IsType<SessionStartFailed>(GameSession.Restore(definition,
            data with { Flags = data.Flags.Except([603]).ToArray(), Recipe = "map3-opening", X = 56, Y = 3, Facing = 3 },
            RuleCompositions.Sf2()));
        Assert.Equal("save-location-unavailable", failed.Failure.Code);
    }

    [Fact]
    public void SettledGransealKeepsTheSavedTileSeparateFromThePublishedChurchRefuge()
    {
        var session = StartSettledGranseal(includeChurch: true);
        var before = session.Current;
        var capture = session.CaptureSave(before.SessionId, before.Revision);
        Assert.Null(capture.Error);
        var ordinary = Assert.IsType<OpeningSaveData>(capture.Data);
        Assert.Equal("map-16", ordinary.EgressMap);
        Assert.DoesNotContain(399, ordinary.Flags);

        var offer = session.QueryChurchSave();
        Assert.True(offer.Allowed);
        Assert.Equal(before.Revision, offer.Revision);
        Assert.Equal(new MapPosition(31, 13), offer.Location!.Anchor.Position);
        Assert.Equal("save-stale-revision", session.RecordChurchSave(offer.SessionId, offer.Revision - 1,
            _ => throw new InvalidOperationException("Stale Record reached publication")).Error);
        Assert.Null(session.RecordChurchSave(offer.SessionId, offer.Revision, _ => false).Error);
        Assert.Same(before, session.Current);

        var recorded = Assert.IsType<OpeningSaveData>(
            session.RecordChurchSave(offer.SessionId, offer.Revision, _ => true).Data);
        byte[] Bytes(OpeningSaveData value) => OpeningSaveJson.Write(new(1, 1, 1, DateTimeOffset.UnixEpoch, value));
        Assert.Equal(Bytes(ordinary with { EgressMap = "map-3", Flags = recorded.Flags }), Bytes(recorded));
        Assert.Contains(399, session.Current.Story.Flags);
        Assert.Equal(new FieldReturnAnchor(new("map-3"), new(32, 13), 1),
            Sf2OutcomeReturnPolicy.FindEgress(session.Definition, session.Current.Story.EgressMap!));
        var resumed = Assert.IsType<SessionStarted>(GameSession.Restore(session.Definition, recorded, session.Rules)).Session;
        Assert.Equal(new MapPosition(31, 13), resumed.Current.Exploration!.PlayerEntity.Position);
        Assert.True(resumed.Current.CanWaitAtInput);
        Assert.Null(EngineTestContent.Send(resumed, new Move(ExplorationDirection.South)).Failure);
    }

    [Fact]
    public void SettledGransealOrdinarySaveNeedsItsSceneAndOnlyRecordNeedsTheChurchBinding()
    {
        var session = StartSettledGranseal(includeChurch: false);
        var before = session.Current;
        Assert.Equal("save-location-unavailable", session.QueryChurchSave().Error);
        Assert.Equal("save-location-unavailable", session.RecordChurchSave(before.SessionId, before.Revision,
            _ => throw new InvalidOperationException("Unbound Record reached publication")).Error);
        Assert.Same(before, session.Current);
        var capture = session.CaptureSave(before.SessionId, before.Revision);
        Assert.Null(capture.Error);
        var data = Assert.IsType<OpeningSaveData>(capture.Data);
        var cases = new[] {
            (session.Definition, data with { FieldScene = null }, "save-field-scene-required"),
            (session.Definition, data with { FieldScene = data.FieldScene! with { Setup = "ms-map3-flag609" } },
                "save-field-scene-context"),
            (Fixture(includeF506: false).Definition, data, "save-location-unavailable"),
            (session.Definition, data with { Flags = data.Flags.Append(640).ToArray() }, "save-location-unavailable"),
        };
        foreach (var (definition, record, code) in cases)
            Assert.Equal(code, Assert.IsType<SessionStartFailed>(GameSession.Restore(definition, record, session.Rules)).Failure.Code);
    }

    private static GameSession StartSettledGranseal(bool includeChurch)
    {
        var (definition, data) = Fixture(includeChurch: includeChurch);
        var rules = RuleCompositions.Sf2();
        var baseline = Assert.IsType<SessionStarted>(GameSession.Restore(definition, data, rules)).Session.Current;
        var start = new ExplorationStartInput(new("map-3"), new("entity-0"), new(31, 13), 2, 32,
            baseline.Story.Flags.Append(506), baseline.Exploration!.Party, display: baseline.Story.Display,
            textSettings: baseline.Story.TextSettings, stepCounter: data.StepCounter, egressMap: new("map-16"));
        return Assert.IsType<SessionStarted>(GameSession.Start(definition, start, rules)).Session;
    }

    [Fact]
    public void SelectedAmbientProgramCanResetButUnfinishedStoryWorkStillRefusesCapture()
    {
        var (definition, data) = Fixture();
        var session = Assert.IsType<SessionStarted>(GameSession.Restore(definition, data, RuleCompositions.Sf2())).Session;
        var current = session.Current;
        var world = current.Exploration!;
        var npc = world.Entities[new("entity-142")];
        SessionSnapshot At(EntityActionProgram actions) => new(current.SessionId, current.Revision, current.ObservationSequence,
            new ActiveExploration(world.WithEntity(npc with { Actions = actions })), current.Story, current.StopReason);
        Assert.True(OpeningSave.Query(definition, At(new(npc.Actions!.Actions.ToArray())), session.Rules).Allowed);
        Assert.Equal("save-story-motion", OpeningSave.Query(definition,
            At(new([new WaitEntityTicks(20), new IdleEntityAction()])), session.Rules).Error);
    }
}
