using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Gameplay.Sf2;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class CaravanWorldServiceTests
{
    [Fact]
    public void OverlapUsesTheActualCaravanAndReturnsWalkingWithoutChangingPartyOrTransport()
    {
        var s = Start(); var before = s.Current.Exploration!; var flags = s.Current.Story.Flags;
        Assert.Equal(new EntityRef("entity-30"), s.QueryExplorationInteraction());
        Accept(s, new Interact(new("entity-30")));
        for (int i = 0; s.Current.Story.Wait is not CaravanWait && i < 2000; i++) Deliver(s);
        var menu = Assert.IsType<CaravanWait>(s.Current.Story.Wait);
        Assert.Equal(FieldTravelMode.Caravan, s.Current.Exploration!.TravelMode);
        Assert.Equal(64 * 384, s.Current.Exploration.PlayerEntity.Motion.X);
        Assert.Equal(64 * 384, s.Current.Exploration.PlayerEntity.Motion.Y);
        Assert.Equal(before.AllEntities[1].Position, s.Current.Exploration.AllEntities[1].Position);
        Assert.Equal(before.AllEntities[1].Sprite, s.Current.Exploration.AllEntities[1].Sprite);
        Assert.NotNull(s.Submit(new(s.Current.SessionId, s.Current.Revision, null, new Move(ExplorationDirection.East))).Failure);
        Accept(s, new BackCaravan(menu.Token));
        for (int i = 0; !s.Current.CanWaitAtInput && i < 2000; i++) Deliver(s);
        Assert.True(s.Current.CanWaitAtInput);
        var after = s.Current.Exploration!;
        Assert.Equal(FieldTravelMode.Walking, after.TravelMode);
        Assert.Equal(before.PlayerEntity.Position, after.PlayerEntity.Position);
        Assert.Equal(before.Party.Actors, after.Party.Actors);
        Assert.Same(before.Party.ItemCustody, after.Party.ItemCustody);
        Assert.Equal(before.Party.MainSeed, after.Party.MainSeed);
        Assert.Equal(flags, s.Current.Story.Flags);
        Assert.Equal(0, s.Current.Story.LogicalView!.TargetSlot);
        Assert.Equal(before.CurrentEntities, after.CurrentEntities);
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    public void ServiceDoesNotReplaceNormalTalkWithoutThePhysicalSpriteAndOverlap(int variation)
    {
        var s = Start(); var current = s.Current; var world = current.Exploration!; var vehicle = world.AllEntities[1];
        vehicle = variation switch
        {
            0 => vehicle with { Sprite = 18 },
            1 => vehicle with { Visible = false },
            2 => vehicle with { Motion = vehicle.Motion with { XDestination = (short)(vehicle.Motion.XDestination + 384) } },
            _ => vehicle with { Motion = vehicle.Motion with { X = (short)(vehicle.Motion.X + 384), XDestination = (short)(vehicle.Motion.XDestination + 384) } },
        };
        var changed = new SessionSnapshot(current.SessionId, current.Revision, current.ObservationSequence,
            new ActiveExploration(world.WithEntity(vehicle)), current.Story, current.StopReason);
        Assert.Null(CaravanRuntime.ServiceTarget(changed));
        Assert.Null(ExplorationInteractionQuery.Query(changed, s.Definition));
    }

    private static void Deliver(GameSession s)
    {
        var pending = s.Current.Exploration!.AllEntities.FirstOrDefault(e => e.WaitingForSprite && e.SpriteReady != e.SpriteRequest);
        SessionCommand command = pending is not null ? new EntitySpriteReady(pending.Slot, pending.SpriteRequest) :
            s.Current.Story.Wait is PresentationWait p ? new CompletePresentation(p.Token, p.Cue.Kind) :
            new AdvanceSimulation(s.Current.Story.Wait?.Token);
        Accept(s, command);
    }
    private static void Accept(GameSession s, SessionCommand command) =>
        Assert.Null(s.Submit(new(s.Current.SessionId, s.Current.Revision, null, command)).Failure);

    private static GameSession Start()
    {
        var (basis, data) = OpeningRestoreTests.Fixture(); var old = basis.Exploration!;
        var original = old.Maps[new("map-67")];
        var map = new ExplorationMapDefinition(original.Map, new(new ushort[4096]), original.Traversal, [], [],
            population: new(30, 128, 0, [new(65, 30, 62), new(67, 7, 18), new(69, 158, 219)]),
            basePalette: original.BasePalette, viewAreas: original.ViewAreas);
        var visuals = old.Visuals! with { Audio = old.Visuals.Audio.Append(new("MUSIC_HEADQUARTERS",
            new(39, 195, 44100, 1, [0, 0, 0, 0], "", 0, 1))).ToDictionary() };
        var world = new ExplorationDefinition([map], CaravanPrograms.Create(new("entity-30"), new("entity-0"), service: true),
            provenance: old.Provenance, partyFlags: old.PartyFlags, visuals: visuals);
        var definition = new ScenarioDefinition(basis.Package, basis.Encounters.Values, exploration: world)
            { Persistence = new Sf2OpeningRestore() };
        var saved = data with { Recipe = "devils-tail", Map = "map-67", X = 26, Y = 23, EgressMap = "map-9",
            Flags = [0, 32, 65, 67, 69, 521] };
        return Assert.IsType<SessionStarted>(GameSession.Restore(definition, saved, RuleCompositions.Sf2())).Session;
    }
}
