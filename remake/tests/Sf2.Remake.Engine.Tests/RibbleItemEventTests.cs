using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class RibbleItemEventTests
{
    private static readonly ActorRef Leader = FieldInventoryTests.Giver, Carrier = FieldInventoryTests.Recipient;
    private const byte Panel = 112, Sword = 61;

    private static GameSession Start(int x = 15, int y = 19, byte facing = 1, int[]? flags = null)
    {
        var baseSession = FieldInventoryTests.Session();
        var battle = baseSession.Definition.Encounters.Values.Single();
        var events = new[] { 15, 16 }.Select(tile => new ExplorationEvent(ExplorationEventKind.UseItem, tile, 19,
            null, new("open", 0), Item: Panel, ItemFacing: 1, RequiredFlag: 731, RequiredFlagValue: false));
        var map = new ExplorationMapDefinition(new("town"), new(new ushort[WorkingMapLayout.WordCount]),
            new([new(0, 0, 63, 63)]), [], events);
        var world = new ExplorationDefinition([map], [new("open", [new WriteFlag(731, true), new EndProgram()])]);
        var definition = new ScenarioDefinition("quest", [battle], exploration: world)
        {
            FieldPreparation = new([..baseSession.Definition.FieldPreparation!.Items.Values, new(Panel, "Panel", false, true)],
                new Dictionary<byte, FieldRecoveryEffect> { [Panel] = FieldRecoveryEffect.MapEvent, [Sword] = FieldRecoveryEffect.MapEvent }),
        };
        var before = baseSession.Current.Exploration!.Party;
        var actors = before.Actors.Select(row => row with {
            SourceLoadout = new(row.Actor == Leader ? [Panel, 0, Panel, 127] : [Panel, Sword, 127, 127], []),
        });
        return Assert.IsType<SessionStarted>(GameSession.Start(definition, new ExplorationStartInput(map.Map, new("hero"),
            new(x, y), facing, 32, flags ?? [], FieldInventory.Replace(before, actors)))).Session;
    }

    private static SessionResult Use(GameSession session, int slot, ushort word) =>
        session.Submit(new(session.Current.SessionId, session.Current.Revision, null, new FieldUseItem(Carrier, slot, word)));

    [Theory]
    [InlineData(15, 19, 1, true)]
    [InlineData(16, 19, 1, true)]
    [InlineData(15, 20, 1, false)]
    [InlineData(16, 19, 3, false)]
    public void PanelUsesCurrentPlayerTileAndFacing(int x, int y, byte facing, bool opens)
    {
        var session = Start(x, y, facing);
        var before = session.Current;
        Assert.Equal(FieldRecoveryEffect.MapEvent, session.QueryFieldActions(Carrier).Items[0].Use);
        Assert.Same(before, session.Current);
        Assert.Contains(Use(session, 1, Sword).Observations, row => row.Kind == "field-no-effect");
        Assert.Equal("field-stale-item", Use(session, 1, Panel).Failure!.Code);
        Assert.DoesNotContain(731, session.Current.Story.Flags);
        var result = Use(session, 0, Panel);
        Assert.Null(result.Failure);
        Assert.Equal(opens, session.Current.Story.Flags.Contains(731));
        Assert.Contains(result.Observations, row => row.Kind == (opens ? "field-item-event" : "field-no-effect"));
        Assert.Contains(Use(session, 0, Panel).Observations, row => row.Kind == "field-no-effect");
    }

    [Fact]
    public void RemovalUsesLastMatchingSlotOfFirstJoinedCarrierRegardlessOfSelection()
    {
        var session = Start();
        var after = Assert.IsType<ActiveExploration>(FieldItemEvents.Remove(session.Definition, session.Current, Panel)).World.Party;
        Assert.Equal(new ushort[] { Panel, 0, 127, 127 }, after.Actors.Single(row => row.Actor == Leader).SourceLoadout!.Items);
        Assert.Equal(new ushort[] { Panel, Sword, 127, 127 }, after.Actors.Single(row => row.Actor == Carrier).SourceLoadout!.Items);
        Assert.Equal(Carrier, FieldItemEvents.Find(session.Definition, session.Current, Sword)!.Value.Member.Carried.Actor);
        Assert.Same(session.Current.Active, FieldItemEvents.Remove(session.Definition, session.Current, 99));
    }
}
