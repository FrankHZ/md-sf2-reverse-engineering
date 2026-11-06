using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.TestSupport;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class TowerFieldSearchTests
{
    private const string World = "SF2_PRIVATE_TOWER_SEARCH_WORLD";

    [PrivateInputFact(World)]
    public void TowerExteriorSearchAtOrdinaryInputHandlesEmptyCellsAndTheSourcePickup()
    {
        string Input(string key) => PrivateInputFactAttribute.RequireInput("SF2_PRIVATE_" + key);
        var source = new OpeningNewGameReader(Input("TOWER_SEARCH_WORLD"), Input("BATTLE01_DATA"), Input("BATTLE01_SCENE"),
            Input("BATTLE01_TERRAIN"), Input("STATIC_DATA"), Input("ENEMY_DATA"), Input("ENEMY_GOLD"),
            305441741, 305397760, Environment.GetEnvironmentVariable("SF2_PRIVATE_BATTLE_SCENE_CONTENT"));
        var read = source.Read();
        Assert.True(read is ExplorationReadAccepted, read is ScenarioReadRejected rejected ? rejected.Failure.ToString() : "world admission");
        var admitted = (ExplorationReadAccepted)read;
        var map = admitted.Definition.Exploration!.Maps[new("map-40")];
        var search = Assert.IsType<FieldSearchDefinition>(map.Search);
        Assert.Empty(search.Descriptions); Assert.Empty(search.Chests);
        Assert.All(map.Setups.Values, setup => Assert.NotNull(setup.Search));
        var pickup = Assert.Single(search.OtherItems);
        Assert.Equal(new MapPosition(15, 6), pickup.Position); Assert.Equal(132, pickup.Flag);
        var mithril = admitted.Definition.PrivateDefinitions!.Items[(byte)(pickup.ItemWord!.Value & 127)];
        Assert.Equal("MITHRIL", mithril.Code); Assert.Equal(2000, mithril.Price);
        Assert.Equal("RARE", mithril.ItemTypeExpression); Assert.Equal("NOTHING", mithril.UseSpell);
        Assert.Equal("NONE", mithril.EquipFlagsExpression);
        Assert.All(mithril.EquipEffects, effect => { Assert.Equal("NONE", effect.Type); Assert.Equal(0, effect.Parameter); });

        GameSession Start(MapPosition position, byte facing)
        {
            var palette = map.BasePalette!;
            var start = new ExplorationStartInput(map.Map, new("entity-0"), position, facing, 32,
                [0, 32, 1, 33, 2, 34, 66, 605, 608, 401], admitted.Start.Party,
                display: new(3, palette, palette, FullFadeVisibility.BaseRestored), egressMap: admitted.Start.Map);
            var outcome = Assert.IsType<SessionStarted>(GameSession.Start(admitted.Definition, start, RuleCompositions.Sf2()));
            Assert.Null(outcome.Result.Failure);
            var session = outcome.Session;
            Assert.True(session.Current.CanWaitAtInput); Assert.True(session.CanSearch);
            Assert.Null(session.Current.Exploration!.Definition.InputProgram);
            Assert.Equal(0, ExplorationViewContext.Resolve(session.Current.Exploration, session.Current.Story.LogicalView).Area);
            return session;
        }
        var empty = Start(new(14, 14), 1);
        var before = empty.Current;
        Accept(empty, new Search(false)); Assert.Same(before, empty.Current);
        Accept(empty, new Search());
        var result = Assert.IsType<FieldSearchWait>(empty.Current.Story.Wait);
        Assert.Equal(FieldSearchKind.Empty, result.Result.Kind); Assert.Equal(new[] { 423, 412 }, result.Result.Texts);
        Assert.Same(before.Exploration, empty.Current.Exploration);
        Accept(empty, new Acknowledge(result.Token)); Assert.True(empty.CanSearch);

        var item = Start(new(15, 7), 1);
        Accept(item, new Search());
        result = Assert.IsType<FieldSearchWait>(item.Current.Story.Wait);
        Assert.Equal(FieldSearchKind.Item, result.Result.Kind); Assert.Equal(pickup.ItemWord, result.Result.ItemWord);
        Assert.Contains(132, item.Current.Story.Flags);
        Accept(item, new Acknowledge(result.Token));
        var party = item.Current.Exploration!.Party;
        Accept(item, new Search());
        Assert.Equal(FieldSearchKind.Empty, Assert.IsType<FieldSearchWait>(item.Current.Story.Wait).Result.Kind);
        Assert.Same(party, item.Current.Exploration!.Party);
        Accept(item, new Acknowledge(item.Current.Story.Wait!.Token));
        var recipient = result.Result.Recipient!.Value;
        var carried = Assert.Single(item.QueryFieldActions(recipient).Items, row => row.Word == pickup.ItemWord);
        Assert.Null(carried.Use); Assert.True(carried.CanDrop);
        var settled = item.Current;
        Assert.Equal("field-item-effect-unsupported", Send(item, new FieldUseItem(recipient, carried.Slot, carried.Word)).Failure!.Code);
        Assert.Same(settled, item.Current);
        Accept(item, new FieldGive(recipient, carried.Slot, carried.Word, new("ally-1")));
        carried = Assert.Single(item.QueryFieldActions(new("ally-1")).Items, row => row.Word == pickup.ItemWord);
        var custody = item.Current.Exploration!.Party.ItemCustody!;
        Accept(item, new FieldDrop(new("ally-1"), carried.Slot, carried.Word));
        Assert.Equal(custody.Deals.GetValueOrDefault((byte)123) + 1, item.Current.Exploration!.Party.ItemCustody!.Deals[123]);
        Assert.Equal(custody.OnceFlags, item.Current.Exploration.Party.ItemCustody.OnceFlags);
        Accept(item, new Search());
        Assert.Equal(FieldSearchKind.Empty, Assert.IsType<FieldSearchWait>(item.Current.Story.Wait).Result.Kind);
        Assert.Contains(132, item.Current.Story.Flags);
    }
}
