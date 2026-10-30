using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Gameplay.Sf2;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class OpeningRestoreTests
{
    internal static (ScenarioDefinition Definition, OpeningSaveData Data) Fixture(bool joined = false, bool victory = false)
    {
        var growth = new BattleGrowthDefinition(0, 0, [], [], new Dictionary<byte, SpellRef>());
        var deployments = Enumerable.Range(0, 2).Select(id => new BattleDeploymentDefinition(
            new(new("ally-" + id), BattleClassRule.UnpromotedSwordsman, 1, 20, 5, 8, 4, 6, false, 6, [],
                sourceLoadout: new([127, 127, 127, 127], [63, 63, 63, 63]), growth: growth), BattleFaction.Ally,
            id, BattleControl.Player, null, new(1 + id, 1), new(null, 0, 0, 0, 0, AllyPartyMember: id))).ToArray();
        var battle = new BattleDefinition("opening", new("battle"), 1, 1, [], deployments, []);
        var palette = new PalettePair(0x222, 0x444);
        var maps = new[] { "map-3", "map-57" }.Select(id => new ExplorationMapDefinition(new(id),
            new WorkingMapLayout(new ushort[WorkingMapLayout.WordCount]), new([new(0, 0, 63, 63)]),
            id == "map-3" ? [new(new("entity-1"), new(42, 8), 3, 32, Sprite: 1,
                Actions: new([new WaitEntityTicks(5), new JumpEntityAction(0)])), new(new("entity-142"), new(54, 17), 1, 32, Sprite: 2)] : [], [],
            // This must never execute during restore.
            onLoad: new("dangerous-onload", 0), population: new(30, 128, 0, [new(66, 1, 1)]),
            entryFlags: [new(256, false)], basePalette: palette,
            viewAreas: [new(0, 0, 63, 63, 0, 0, 0, 0, 256, 256, 256, 256, 0, 0, 0, 0, 0)])).ToArray();
        var audio = new ExplorationAudio(1, 0, 44100, 1, [], "", 0, 1);
        var visuals = new ExplorationVisuals(maps.ToDictionary(map => map.Map, map => new ExplorationMapVisual(map.Map,
            new(1, 1, "rgba8", []), 1, [], [new(1, 1)])), new Dictionary<int, ExplorationSpriteVisual>(),
            new Dictionary<int, ExplorationPortraitVisual>(), new Dictionary<string, ExplorationAudio> { ["field"] = audio,
                ["previous"] = new(2, 0, 44100, 1, [], "", 0, 1) });
        var world = new ExplorationDefinition(maps, [new("dangerous-onload", [new WriteFlag(999, true), new EndProgram()])],
            provenance: new("project-test", "accepted", "test-rom", [], "engine behavior"), partyFlags: new(30, 0, 32, 12), visuals: visuals);
        var definition = new ScenarioDefinition("opening-test", [battle], exploration: world) { Persistence = new Sf2OpeningRestore() };
        var flags = new List<int> { 0, 32, 80, 256 };
        if (joined || victory) flags.AddRange([1, 33, 66, 600, 602, 603]);
        if (victory) flags.AddRange([401, 501]);
        var allies = deployments.Select(row => new SavedAlly(row.Actor.Value, 0, 7, 2, 38, 4, 2, 2, 0, true, false,
            3, 25, 8, 14, 9, 10, [127, 127, 127, 127], [63, 63, 63, 63])).ToArray();
        var data = new OpeningSaveData(OpeningSave.Compatibility(definition, RuleCompositions.Sf2()),
            victory ? "battle01-return" : joined ? "map3-joined" : "map3-opening", victory ? "map-57" : "map-3",
            victory ? 4 : joined ? 32 : 56, victory ? 5 : joined ? 13 : 3, joined && !victory ? (byte)1 : (byte)3,
            "opening", allies, joined || victory ? ["ally-0", "ally-1"] : ["ally-0"], 0x1234abcdu, uint.MaxValue,
            4321, [2], [new(1, 3)], flags, 27, "map-3", 535, new(2, 0, 0), 3, new("field", []), 0, false, false);
        return (definition, data);
    }

    [Theory]
    [InlineData(false, false)] [InlineData(true, false)] [InlineData(true, true)]
    public void RestoreRebuildsEachRecipeWithoutOnloadGrowthHealingOrReward(bool joined, bool victory)
    {
        var (definition, data) = Fixture(joined, victory);
        data = data with { Allies = data.Allies.Select(a => a.Actor == "ally-1" ? a with { Hp = 0 } : a).ToArray() };
        var first = Assert.IsType<SessionStarted>(GameSession.Restore(definition, data, RuleCompositions.Sf2())).Session;
        var second = Assert.IsType<SessionStarted>(GameSession.Restore(definition, data, RuleCompositions.Sf2())).Session;
        Assert.NotEqual(first.Current.SessionId, second.Current.SessionId);
        Assert.True(first.Current.CanWaitAtInput);
        Assert.Equal(new MapPosition(data.X, data.Y), first.Current.Exploration!.PlayerEntity.Position);
        Assert.DoesNotContain(999, first.Current.Story.Flags); Assert.DoesNotContain(256, first.Current.Story.Flags);
        Assert.Equal(data.MainSeed, first.Current.Exploration.Party.MainSeed);
        Assert.Equal(data.ThinkingSeed, first.Current.Exploration.Party.ThinkingSeed);
        Assert.Equal(data.Gold, first.Current.Exploration.Party.Gold);
        Assert.Equal(7, first.Current.Exploration.Party.Actors[0].Hp);
        Assert.Equal(0, first.Current.Exploration.Party.Actors[1].Hp);
        Assert.Equal((byte)38, first.Current.Exploration.Party.Actors[0].Exp);
        Assert.Equal(3, first.Current.Exploration.Party.Actors[0].Progress!.Level);
        Assert.Equal(535, first.Current.Story.TextCursor);
        Assert.Equal(0, first.Current.Story.Music!.Step);
        var capture = first.CaptureSave(first.Current.SessionId, first.Current.Revision);
        Assert.Null(capture.Error); Assert.Equal(data.Allies[0], capture.Data!.Allies[0] with { Items = data.Allies[0].Items, Spells = data.Allies[0].Spells });
        Assert.NotSame(first.Current.Exploration.Party, second.Current.Exploration!.Party);
        foreach (var session in new[] { first, second })
            Assert.Null(EngineTestContent.Send(session, new WaitAtInput()).Failure);
        Assert.Equal(first.Current.Exploration.Party.MainSeed, second.Current.Exploration.Party.MainSeed);
        Assert.Equal(first.Current.Exploration.AllEntities.Select(e => e.Motion), second.Current.Exploration.AllEntities.Select(e => e.Motion));
    }

    [Theory]
    [InlineData(4, 5)] [InlineData(12, 8)]
    public void VictoryRestoreUsesTheCompletedOutcomeLanding(int x, int y)
    {
        var (definition, data) = Fixture(true, true);
        data = data with { X = x, Y = y, Gold = 942, MainSeed = 0, ThinkingSeed = uint.MaxValue };
        var session = Assert.IsType<SessionStarted>(GameSession.Restore(definition, data, RuleCompositions.Sf2())).Session;
        Assert.Equal(new MapPosition(x, y), session.Current.Exploration!.PlayerEntity.Position);
        Assert.Equal(new FieldReturnAnchor(new("map-57"), new(x, y), 3), session.Current.Story.SaveAnchor);
        Assert.Equal(data, session.CaptureSave(session.Current.SessionId, session.Current.Revision).Data! with
        {
            Allies = data.Allies, ActiveAllies = data.ActiveAllies, OnceFlags = data.OnceFlags, Deals = data.Deals,
            Flags = data.Flags, Music = data.Music,
        });
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)]
    public void InvalidProgressCannotPublishAReplacement(int variation)
    {
        var (definition, data) = Fixture();
        var original = Assert.IsType<SessionStarted>(GameSession.Restore(definition, data, RuleCompositions.Sf2())).Session;
        var before = original.Current;
        var invalid = variation switch
        {
            0 => data with { Compatibility = data.Compatibility with { GameplayRevision = 2 } },
            1 => data with { ActiveAllies = ["ally-0", "ally-1"] },
            2 => data with { Allies = [data.Allies[0], data.Allies[0]] },
            3 => data with { Allies = data.Allies.Select(a => a with { Hp = 201 }).ToArray() },
            4 => data with { Recipe = "battle01-return" },
            _ => data with { Music = new("missing", []) },
        };
        Assert.IsType<SessionStartFailed>(GameSession.Restore(definition, invalid, RuleCompositions.Sf2()));
        Assert.Same(before, original.Current);
    }

    [Theory]
    [InlineData(601)] [InlineData(602)] [InlineData(604)]
    public void IntermediateConversationFlagsSurviveAndF602RepositionsSarah(int flag)
    {
        var (definition, data) = Fixture();
        data = data with { Flags = data.Flags.Append(flag).ToArray() };
        var session = Assert.IsType<SessionStarted>(GameSession.Restore(definition, data, RuleCompositions.Sf2())).Session;
        Assert.Contains(flag, session.Current.Story.Flags);
        Assert.Equal(flag == 602 ? new MapPosition(41, 10) : new(42, 8), session.Current.Exploration!.Entities[new("entity-1")].Position);
        Assert.Equal((byte)0xE0, session.Current.Exploration.Entities[new("entity-1")].Motion.FlagsA);
    }
}
