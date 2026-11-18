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

public sealed class MineCannonTests
{
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void LiveAndFreshMineScenesKeepTheirOwnPopulationAndLayout(bool oddler)
    {
        var (definition, basis) = Fixture();
        int[] flags = oddler ? [67, 68, 69, 785] : [67, 69, 785];
        var source = definition.Exploration!.Maps[new("map-30")];
        var world = MapTransfer.Build(source, new("entity-0"), new(9, 11), 0, 32, basis.Exploration!.Party, flags);
        Assert.Equal(oddler ? 2 : 0, world.Aliases[new("entity-31")]);
        // Execute the action replacement at the recovery seam, including the absent-B alias.
        var program = new StoryProgram("mine-follow", [
            new FollowEntity(new("entity-31"), new("entity-7"), -24, 0),
            new StartEntityMotion(new("entity-0"), new([new FaceEntity(0), new StopEntityActions()]), false),
            new WaitProgramTicks(1), new EndProgram()]);
        var seam = new ScenarioDefinition(definition.Package, definition.Encounters.Values,
            exploration: new(definition.Exploration.Maps.Values, [program], visuals: definition.Exploration.Visuals));
        var state = new SessionSnapshot(basis.SessionId, basis.Revision, basis.ObservationSequence,
            new ActiveExploration(world), basis.Story.Copy(new("mine-follow", 0), flags: flags), basis.StopReason);
        var followed = ProgramRunner.Run(seam, state, []);
        Assert.Null(followed.Failure);
        world = followed.Snapshot.Exploration!;
        Assert.Null(world.PlayerEntity.Follower);
        if (oddler) Assert.Equal(world.Aliases[new("entity-7")], world.Entities[new("entity-31")].Follower!.LeaderSlot);
        world = world.Hide(world.Entities[new("entity-158")], true);
        world = world.WithLayout(world.Layout.ApplyBlockCopy(new(21, 6, 7, 5, 2, 2)));
        int[] complete = oddler ? [67, 68, 760, 761, 785] : [67, 760, 761, 785];
        var scene = Sf2MineRestore.Capture(definition, world);
        Assert.Equal("ms-map30-flag785", scene.Setup);
        var location = new SaveLocation("dwarven-mine", new(world.Map, new(9, 11), 0), "mine");
        var loaded = Sf2MineRestore.Rebuild(definition, location, world.Party, complete, scene, out _, 0);
        Assert.Equal(5, loaded.AllEntities.Count(row => row.Entity.Value is "entity-128" or "entity-129" or "entity-130" or "entity-131" or "entity-132"));
        Assert.Equal(world.Layout.Words, loaded.Layout.Words);
        Assert.Equal(world.Aliases.OrderBy(row => row.Key.Value), loaded.Aliases.OrderBy(row => row.Key.Value));
        Assert.Equal(world.CurrentEntities, loaded.CurrentEntities);
        Assert.Equal(scene.Actors, Sf2MineRestore.Capture(definition, loaded).Actors);
        Assert.Null(loaded.PlayerEntity.Follower);
        var fresh = MapTransfer.Build(source, new("entity-0"), new(9, 11), 0, 32, world.Party, complete);
        fresh = fresh.WithLayout(fresh.Layout.ApplyBlockCopy(new(21, 6, 7, 5, 2, 2)).ApplyBlockCopy(new(21, 0, 6, 0, 4, 4)));
        var freshScene = Sf2MineRestore.Capture(definition, fresh);
        Assert.Equal("ms-map30-flag761", freshScene.Setup);
        var freshLoaded = Sf2MineRestore.Rebuild(definition, location, fresh.Party, complete, freshScene, out _, 0);
        Assert.DoesNotContain(freshLoaded.AllEntities, row => row.Entity.Value.StartsWith("entity-12"));
        Assert.Equal(fresh.Layout.Words, freshLoaded.Layout.Words);
        Assert.NotEqual(loaded.Layout.Words[6], freshLoaded.Layout.Words[6]);
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(3)] [InlineData(4)]
    public void StoryGiftUsesOnlyTheNamedPackAndCanRepeatWithSpace(int occupied)
    {
        var (definition, basis) = Fixture();
        var field = basis.Exploration!;
        var actor = field.Party.Actors[0];
        var pack = new BattleSourceLoadout(Enumerable.Range(0, 4).Select(i => (ushort)(i < occupied ? 1 : 127)).ToArray(), []);
        actor = actor with { Hp = 0, SourceLoadout = pack, Progress = actor.Progress is { } p ? p with { SourceLoadout = pack } : null };
        field = field.WithParty(FieldInventory.Replace(field.Party, [actor]));
        // No joined, active, alive, recruitment or F761 restriction belongs to AddItem.
        var current = new SessionSnapshot(basis.SessionId, basis.Revision, basis.ObservationSequence,
            new ActiveExploration(field), basis.Story.Copy(null, flags: [761]), basis.StopReason);
        Assert.Equal(occupied < 4, FieldStoryInventory.HasSpace(definition, current, actor.Actor));
        var added = Assert.IsType<ActiveExploration>(FieldStoryInventory.Add(definition, current, actor.Actor, 114));
        var words = added.World.Party.Actors[0].SourceLoadout!.Items;
        Assert.Equal(occupied < 4 ? 1 : 0, words.Count(word => word == 114));
        Assert.Equal(field.Party.Actors.Skip(1), added.World.Party.Actors.Skip(1));
        var again = new SessionSnapshot(current.SessionId, current.Revision, current.ObservationSequence,
            added, current.Story, current.StopReason);
        var repeated = Assert.IsType<ActiveExploration>(FieldStoryInventory.Add(definition, again, actor.Actor, 114));
        Assert.Equal(Math.Min(2, 4 - occupied), repeated.World.Party.Actors[0].SourceLoadout!.Items.Count(word => word == 114));
        Assert.Same(field.Party.ItemCustody, repeated.World.Party.ItemCustody);
    }

    [Fact]
    public void MineRequiresSceneAndRejectsAnotherChaptersPopulation()
    {
        var (definition, basis) = Fixture();
        var (_, saved) = OpeningRestoreTests.Fixture();
        var policy = new Sf2OpeningRestore();
        Assert.Equal("save-field-scene-required", Assert.Throws<BattleRuleException>(() =>
            policy.ValidateRegisters(saved with { Map = "map-30", Recipe = "dwarven-mine" })).Code);
        int[] flags = [785];
        var world = MapTransfer.Build(definition.Exploration!.Maps[new("map-30")], new("entity-0"), new(9, 11), 0, 32, basis.Exploration!.Party, flags);
        var scene = Sf2MineRestore.Capture(definition, world) with { Setup = "ms-map33-flag784" };
        Assert.Equal("save-field-scene-population", Assert.Throws<BattleRuleException>(() =>
            Sf2MineRestore.Rebuild(definition, new("dwarven-mine", new(world.Map, new(9, 11), 0), "mine"), world.Party, flags, scene, out _, 0)).Code);
    }

    private static (ScenarioDefinition, SessionSnapshot) Fixture()
    {
        var (basis, data) = OpeningRestoreTests.Fixture();
        var session = Assert.IsType<SessionStarted>(GameSession.Restore(basis, data, RuleCompositions.Sf2())).Session;
        var population = new ExplorationPopulation(30, 128, 0, [new(67, 7, 7), new(68, 31, 213), new(69, 158, 219)]);
        var actors = Enumerable.Range(128, 5).Select(i => new ExplorationEntityDefinition(new("entity-" + i), new(15, i - 124), 3, 32, Sprite: 226)).ToArray();
        var words = new ushort[4096];
        for (int y = 0; y < 8; y++) for (int x = 21; x < 25; x++) words[y * 64 + x] = (ushort)(y + 1);
        var map = new ExplorationMapDefinition(new("map-30"), new(words), new([new(0, 0, 40, 40)]), actors.Take(4).ToArray(), [],
            setup: new(new("ms-map30"), [new(new("785"), new("ms-map30-flag785")), new(new("761"), new("ms-map30-flag761"))]),
            population: population, setups: [new(new("ms-map30-flag785"), actors, [], null, null, population),
                new(new("ms-map30-flag761"), [], [], null, null, population)]);
        var visuals = basis.Exploration!.Visuals! with { Sprites = Enumerable.Range(0, 256).ToDictionary(i => i, i => new ExplorationSpriteVisual(i, [], null, 0)) };
        return (new(basis.Package, basis.Encounters.Values, exploration: new([map], [], visuals: visuals)), session.Current);
    }
}
