using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class SharedSceneGraphicsTests
{
    [Theory]
    [InlineData(4)] [InlineData(32)]
    public void SharedGraphicsChangeTheSlotResourceWithoutReplacingActorsOrWaiting(int slot)
    {
        var current = Start("harbor-arrival").Current;
        var world = current.Exploration!;
        world = world.WithEntities(world.AllEntities.Select(entity => entity with { SharedSpriteSlot = slot, Sprite = entity.Sprite ?? 200 }));
        var graphics = new Dictionary<string, IReadOnlyList<ExplorationRaster>> { ["shifted"] = [] };
        var sprites = world.AllEntities.Select(entity => entity.Sprite!.Value).Distinct().ToDictionary(id => id,
            id => new ExplorationSpriteVisual(id, [], null, 0, Graphics: graphics));
        var visuals = new ExplorationVisuals(new Dictionary<MapId, ExplorationMapVisual>(), sprites,
            new Dictionary<int, ExplorationPortraitVisual>(), new Dictionary<string, ExplorationAudio>());
        var programs = new[] { new StoryProgram("shared", [new SetSharedSpriteGraphics(slot, "shifted"), new WriteFlag(802, true), new EndProgram()]) };
        var definition = new ScenarioDefinition("shared-test", [], exploration: new([world.Definition], programs, visuals: visuals));
        var snapshot = new SessionSnapshot(Guid.NewGuid(), 0, 0, new ActiveExploration(world),
            new StoryState(cursor: new("shared", 0)), SessionStopReason.SimulationWait);
        var result = ProgramRunner.Run(definition, snapshot, []);
        Assert.Null(result.Failure);
        Assert.Null(result.Snapshot.Story.Wait);
        Assert.Contains(802, result.Snapshot.Story.Flags);
        Assert.Equal("shifted", result.Snapshot.Exploration!.SpriteGraphics[slot]);
        Assert.Equal(world.AllEntities, result.Snapshot.Exploration.AllEntities);
        Assert.Equal(world.Party, result.Snapshot.Exploration.Party);
        Assert.Equal(world.Aliases, result.Snapshot.Exploration.Aliases);
    }

    [Fact]
    public void HiddenCyclicActionsRemainDormantUntilThePhysicalRecordIsReactivated()
    {
        var world = Start("harbor-arrival").Current.Exploration!;
        var actor = world.AllEntities.Last() with { Actions = new([new WaitEntityTicks(3), new JumpEntityAction(0)]) };
        world = world.WithFollower(actor);
        var hidden = world.Hide(actor, true);
        Assert.DoesNotContain(actor.Slot, hidden.CurrentEntities);
        Assert.False(hidden.TryResolveEntity(actor.Entity, out _));
        var record = hidden.AllEntities.Single(entity => entity.Slot == actor.Slot);
        Assert.Same(actor.Actions, record.Actions);
        for (int i = 0; i < 20; i++) hidden = EntityActionRunner.Tick(hidden).World;
        Assert.Equal(record, hidden.AllEntities.Single(entity => entity.Slot == actor.Slot));
        Assert.Equal(world.Party.MainSeed, hidden.Party.MainSeed);
        var resumed = hidden.WithEntity(record with { Visible = true, Motion = actor.Motion with { WaitTimer = 0 } });
        Assert.NotEqual(record.Motion.WaitTimer, EntityActionRunner.Tick(resumed).World.AllEntities.Single(entity => entity.Slot == actor.Slot).Motion.WaitTimer);
    }

    [Fact]
    public void PaletteRequestContinuesToItsCallerWithoutWaitingForTheColorTransition()
    {
        var world = Start("harbor-arrival").Current.Exploration!;
        ushort[] lookup = Enumerable.Range(0, 512).Select(i => PaletteColorTransition.Word(511 - i)).ToArray();
        var program = new StoryProgram("colors", [new RemapPaletteColors(lookup), new WriteFlag(784, true), new EndProgram()]);
        var definition = new ScenarioDefinition("color-test", [], exploration: new([world.Definition], [program]));
        var palette = new PalettePair(0x222, 0x444);
        var snapshot = new SessionSnapshot(Guid.NewGuid(), 0, 0, new ActiveExploration(world),
            new StoryState(cursor: new("colors", 0), display: new(3, palette, palette, FullFadeVisibility.BaseRestored)),
            SessionStopReason.SimulationWait);
        var result = ProgramRunner.Run(definition, snapshot, []);
        Assert.Null(result.Failure);
        Assert.Null(result.Snapshot.Story.Wait);
        Assert.Null(result.Snapshot.Story.Cursor);
        Assert.Contains(784, result.Snapshot.Story.Flags);
        Assert.Equal(0, result.Snapshot.Story.Display!.Colors!.StartedAt);
        Assert.Equal(0xEEE, result.Snapshot.Story.Display.Colors.ColorsAt(32)[0]);
    }

    [Fact]
    public void PaletteCopyIsTheCurrentSnapshotAndRestoreBacksUpTheNewCurrent()
    {
        ushort[] lookup = Enumerable.Range(0, 512).Select(i => PaletteColorTransition.Word(511 - i)).ToArray();
        var first = PaletteColorTransition.Request(null, lookup, 10);
        Assert.Equal(0, first.ColorsAt(10)[0]);
        Assert.Equal(0xEEE, first.ColorsAt(42)[0]);
        var middle = first.ColorsAt(26);
        var restored = PaletteColorTransition.Request(first, null, 26);
        Assert.Equal(middle, restored.ColorsAt(26));
        Assert.Equal(0, restored.ColorsAt(58)[0]);
        var restoredAgain = PaletteColorTransition.Request(restored, null, 58);
        Assert.Equal(middle, restoredAgain.ColorsAt(90));
        Assert.Equal(first.ColorsAt(42), first.SnapToBase(20).ColorsAt(20));
    }

    [Theory]
    [InlineData(MapLoadMode.Rebuild)] [InlineData(MapLoadMode.Preserve)]
    public void LoadingANewMapPaletteClearsTheColorLookupWhileCurrentMapPassagesRetainIt(MapLoadMode mode)
    {
        var world = Start("harbor-arrival").Current.Exploration!;
        var palette = new PalettePair(0x222, 0x444);
        var map = new ExplorationMapDefinition(world.Map, world.Layout, world.Definition.Traversal,
            world.Definition.Entities, [], basePalette: palette);
        var definition = new ScenarioDefinition("reload-colors", [], exploration: new([map], []));
        ushort[] lookup = Enumerable.Range(0, 512).Select(i => PaletteColorTransition.Word(511 - i)).ToArray();
        var colors = PaletteColorTransition.Request(null, lookup, 0);
        var current = new SessionSnapshot(Guid.NewGuid(), 0, 0, new ActiveExploration(world),
            new StoryState(display: new(3, palette, palette, FullFadeVisibility.BaseRestored, colors)), SessionStopReason.PlayerInput);
        var next = MapTransfer.Apply(definition, current, map.Map, world.PlayerEntity.Position, 1, mode,
            current.Story, [], Sf2.Remake.Application.Gameplay.RuleCompositions.Sf2(), checkBattle: false);
        if (mode == MapLoadMode.Preserve) Assert.Same(colors, next.Story.Display!.Colors);
        else Assert.Null(next.Story.Display!.Colors);
        Assert.Equal(palette, next.Story.Display.Base);
    }

    [Theory]
    [InlineData("map-33", 785)] [InlineData("map-33", 787)]
    [InlineData("map-34", 785)] [InlineData("map-34", 787)]
    public void LaterCreedFlagsDoNotInventGlobalSaveOrRecruitmentGates(string mapId, int later)
    {
        var (basis, _) = OpeningRestoreTests.Fixture();
        var map = new ExplorationMapDefinition(new(mapId), new(new ushort[4096]), new([new(0, 0, 63, 63)]), [], []);
        var definition = new ScenarioDefinition(basis.Package, basis.Encounters.Values, exploration: new([map], []));
        var anchor = new FieldReturnAnchor(map.Map, new(7, 12), 3);
        Assert.Equal(mapId == "map-33" ? "creed-mansion" : "evil-spirit-shrine",
            basis.Persistence!.Select(definition, anchor, [784, 802, later], null).Recipe);
    }

    [Theory]
    [InlineData(false, false)] [InlineData(true, false)] [InlineData(true, true)]
    public void SettledShrineSaveUsesTheSameRecipeAcrossTheCallerFlags(bool completed, bool both)
    {
        var (basis, _) = OpeningRestoreTests.Fixture();
        var map = new ExplorationMapDefinition(new("map-34"), new(new ushort[4096]), new([new(0, 0, 63, 63)]), [], []);
        var definition = new ScenarioDefinition(basis.Package, basis.Encounters.Values, exploration: new([map], []));
        int[] flags = completed ? both ? [784, 802] : [784] : [802];
        Assert.Equal("evil-spirit-shrine", basis.Persistence!.Select(definition, new(map.Map, new(7, 12), 3), flags, null).Recipe);
    }
}
