using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class WillardBurrowSaveTests
{
    [Theory]
    [InlineData(false, false, 2, 7, 1)]
    [InlineData(false, true, 30, 8, 0)]
    [InlineData(true, false, 42, 21, 3)]
    [InlineData(true, true, 20, 3, 2)]
    public void ColdRestorePreservesIndependentFlagsAndSelectsPopulation(bool complete, bool confronted,
        int x, int y, byte facing)
    {
        var (basis, data) = OpeningRestoreTests.Fixture();
        var world = basis.Exploration!;
        var source = world.Maps[new("map-57")];
        var cave = new ExplorationMapDefinition(new("map-27"), source.Layout, source.Traversal,
            [new(new("entity-128"), new(43, 20), 2, 32, Sprite: 79),
             new(new("entity-129"), new(43, 21), 2, 32, Sprite: 154),
             new(new("entity-130"), new(43, 22), 2, 32, Sprite: 79)], [],
            onLoad: source.OnLoad, population: source.Population,
            setup: new(new("ordinary"), [new(new("523"), new("empty"))]),
            setups: [new(new("empty"), [], [], source.OnLoad, null, source.Population!)],
            basePalette: source.BasePalette, viewAreas: source.ViewAreas);
        var definition = new ScenarioDefinition(basis.Package, basis.Encounters.Values,
            exploration: new(world.Maps.Values.Append(cave), world.Programs.Values,
                provenance: world.Provenance, partyFlags: world.PartyFlags, visuals: world.Visuals))
            { Persistence = basis.Persistence };
        var flags = data.Flags.Concat(complete ? new[] { 523 } : new[] { 423 })
            .Concat(confronted ? new[] { 781, 473, 519 } : []).ToArray();
        var slot = data with { Recipe = "willard-burrow", Map = "map-27", X = x, Y = y, Facing = facing,
            Flags = flags, EgressMap = "map-26" };
        var session = Assert.IsType<SessionStarted>(GameSession.Restore(definition, slot, RuleCompositions.Sf2())).Session;
        var field = session.Current.Exploration!;
        Assert.Equal(new MapPosition(x, y), field.PlayerEntity.Position);
        Assert.Equal(facing, field.PlayerEntity.Motion.Facing);
        Assert.Equal(!complete, field.AllEntities.Any(entity => entity.Entity.Value == "entity-129"));
        Assert.True(field.TryResolveEntity(new("entity-129"), out var lookup));
        Assert.Equal(complete ? field.PlayerEntity.Slot : 2, lookup.Slot);
        Assert.Equal(flags.Order(), session.Current.Story.Flags.Order());
        Assert.DoesNotContain(999, session.Current.Story.Flags);
        var capture = session.CaptureSave(session.Current.SessionId, session.Current.Revision);
        Assert.Null(capture.Error);
        Assert.Equal(slot, capture.Data! with { Allies = slot.Allies, ActiveAllies = slot.ActiveAllies,
            OnceFlags = slot.OnceFlags, Deals = slot.Deals, Flags = slot.Flags, Music = slot.Music, Names = slot.Names });
    }
}
