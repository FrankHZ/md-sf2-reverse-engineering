using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using System.Text.Json.Nodes;
using static Sf2.Remake.Engine.Tests.EngineTestContent;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class FieldSearchTests
{
    private static readonly MapPosition Faced = new(2, 1);
    private static readonly FieldAreaDescription Description = new(Faced, false, [425, 900]);
    private static GameSession Session(ushort marker = 0x1800, ushort? item = 0, uint? gold = null,
        ushort[]? leader = null, ushort[]? second = null, int[]? flags = null,
        FieldAreaDescription[]? descriptions = null, bool placement = true, ExplorationEvent[]? events = null,
        FieldPreparationDefinition? preparation = null)
    {
        var source = FieldInventoryTests.Session().Definition;
        var battle = source.Encounters.Values.Single();
        var actors = battle.Deployments.Where(row => row.Faction == BattleFaction.Ally).ToArray();
        var words = new ushort[WorkingMapLayout.WordCount]; words[1 * 64 + 2] = marker;
        var receipt = new FieldItemPlacement(Faced, 150, item, gold);
        var search = new FieldSearchDefinition(placement && marker == 0x1800 ? [receipt] : [],
            placement && marker != 0x1800 ? [receipt] : [], descriptions ?? [Description], 0xD801, 0xD802);
        var map = new ExplorationMapDefinition(new("sample-field"), new(words), new([new(0, 0, 63, 63)]), [], events ?? [], search: search);
        int[] ids = [403, 404, 405, 408, 409, 410, 412, 423, 425, 427, 900];
        var world = new ExplorationDefinition([map], [], ids.Select(id => new KeyValuePair<int, string>(id, "authored text " + id)));
        var definition = new ScenarioDefinition("search-test", [battle], exploration: world) { FieldPreparation = preparation ?? source.FieldPreparation };
        var party = actors.Select((actor, index) => new BattleActorStartInput(actor.Actor, index == 0 ? (ushort)10 : (ushort)0,
            0, null, null, null, 0, null, Initialized: true, SourceLoadout: new(index == 0 ? leader ?? [127, 127, 127, 127] : second ?? [127, 127, 127, 127], [63, 63, 63, 63])));
        return Assert.IsType<SessionStarted>(GameSession.Start(definition, new ExplorationStartInput(map.Map,
            new("player"), new(1, 1), 0, 32, flags ?? [], new(battle.Encounter, party, 1234, 5678, 999995)))).Session;
    }

    private static SessionResult Send(GameSession session, SessionCommand command) =>
        session.Submit(new(session.Current.SessionId, session.Current.Revision, null, command));
    private static FieldSearchResult Search(GameSession session, bool menu = true)
    {
        Assert.Null(Send(session, new Search(menu)).Failure);
        return Assert.IsType<FieldSearchWait>(session.Current.Story.Wait).Result;
    }
    private static void Close(GameSession session)
    {
        var wait = Assert.IsType<FieldSearchWait>(session.Current.Story.Wait);
        Assert.Null(Send(session, new Acknowledge(wait.Token)).Failure);
        Assert.True(session.Current.CanWaitAtInput);
    }

    [Fact]
    public void GrantAndReceiptAreAtomicPreserveBrokenWordsAndNeverUseBattleCustody()
    {
        var session = Session(item: 256); var before = session.Current;
        var result = Search(session);
        Assert.Equal(FieldSearchKind.Item, result.Kind); Assert.Equal(FieldInventoryTests.Giver, result.Recipient);
        Assert.Equal(150, result.Receipt!.Flag); Assert.True(result.Receipt.Chest);
        Assert.Equal(before.Revision + 1, session.Current.Revision);
        Assert.Contains(150, session.Current.Story.Flags);
        Assert.Equal((ushort)0xD802, session.Current.Exploration!.Layout[2, 1]);
        Assert.Equal((ushort)256, session.Current.Exploration.Party.Actors.First().SourceLoadout!.Items[0]);
        Assert.Equal("Healing Herb", result.ItemName); Assert.Null(result.Message);
        Assert.Same(before.Exploration!.Party.ItemCustody, session.Current.Exploration.Party.ItemCustody);
        Assert.Equal(before.Exploration.Party.MainSeed, session.Current.Exploration.Party.MainSeed);
        Assert.Equal(before.Exploration.Party.ThinkingSeed, session.Current.Exploration.Party.ThinkingSeed);
        Assert.Equal(before.Story.SimulationTick, session.Current.Story.SimulationTick);
        Close(session); var after = session.Current.Exploration.Party;
        Assert.Equal(FieldSearchKind.Empty, Search(session).Kind);
        Assert.Same(after, session.Current.Exploration.Party);
    }

    [Fact]
    public void FullLeaderHandsToFirstJoinedCarrierEvenWhenDefeatedAndKeepsEquipment()
    {
        var session = Session(leader: [178, 0, 0, 0], second: [127, 127, 127, 127]);
        var before = session.Current.Exploration!.Party.Actors.First().SourceLoadout;
        var result = Search(session);
        Assert.Equal(FieldInventoryTests.Recipient, result.Recipient);
        Assert.Same(before, session.Current.Exploration.Party.Actors.First().SourceLoadout);
        Assert.Equal((ushort)0, session.Current.Exploration.Party.Actors.Last().SourceLoadout!.Items[0]);
        Assert.Equal((ushort)0, session.Current.Exploration.Party.Actors.Last().Hp);
    }

    [Theory]
    [InlineData(0x1800)] [InlineData(0x2C00)] [InlineData(0x3000)] [InlineData(0x3400)] [InlineData(0x1C00)]
    public void FullPacksRefuseWithoutClaimingAndCanRetryAfterMakingRoom(int marker)
    {
        var session = Session((ushort)marker, leader: [0, 0, 0, 0], second: [0, 0, 0, 0]);
        var party = session.Current.Exploration!.Party; var layout = session.Current.Exploration.Layout;
        Assert.Equal(FieldSearchKind.Full, Search(session).Kind);
        Assert.Same(party, session.Current.Exploration.Party); Assert.Same(layout, session.Current.Exploration.Layout);
        Assert.DoesNotContain(150, session.Current.Story.Flags);
        Close(session);
        Assert.Null(Send(session, new FieldDrop(FieldInventoryTests.Giver, 0, 0)).Failure);
        Assert.Equal(FieldSearchKind.Item, Search(session).Kind);
        Assert.Contains(150, session.Current.Story.Flags);
        Close(session); Assert.Equal(FieldSearchKind.Empty, Search(session).Kind);
    }

    [Fact]
    public void MarkerDispatchPrecedesDescriptionAndEmptyChestStillOpens()
    {
        var session = Session(item: 127);
        var result = Search(session);
        Assert.Equal(FieldSearchKind.Empty, result.Kind); Assert.Equal(new[] { 403, 408 }, result.Texts);
        Assert.Contains(150, session.Current.Story.Flags);
        Assert.Equal((ushort)0xD802, session.Current.Exploration!.Layout[2, 1]);
    }

    [Theory]
    [InlineData(0x2C00, 404, 409)] [InlineData(0x3000, 405, 410)]
    [InlineData(0x3400, 427, 412)] [InlineData(0x1C00, 423, 412)]
    public void MissingPlacementUsesTheKindsEmptyText(int marker, int first, int last)
    {
        var session = Session((ushort)marker, placement: false);
        Assert.Equal(new[] { first, last }, Search(session).Texts);
        Assert.Empty(session.Current.Story.Flags);
    }

    [Fact]
    public void OrderedMenuOnlyDescriptionAndUnhandledActionRemainDistinct()
    {
        var session = Session(0, descriptions: [new(Faced, true, [425, 900])]);
        var before = session.Current;
        Assert.Null(Send(session, new Search(false)).Failure); Assert.Same(before, session.Current);
        Assert.Equal(FieldSearchKind.Description, Search(session).Kind); Close(session);
        session = Session(0, descriptions: []);
        Assert.Equal(new[] { 423, 412 }, Search(session).Texts);
    }

    [Fact]
    public void GoldUsesSharedAccountingCapsAndIsOnceOnly()
    {
        var session = Session(item: null, gold: 30);
        Assert.Equal(FieldSearchKind.Gold, Search(session).Kind);
        Assert.Equal(999999u, session.Current.Exploration!.Party.Gold);
        Close(session); Assert.Equal(FieldSearchKind.Empty, Search(session).Kind);
        Assert.Equal(999999u, session.Current.Exploration.Party.Gold);
    }

    [Fact]
    public void MapScopedNoEffectKeepsTheItemAndDoesNotEnableOtherMaps()
    {
        var admitted = new FieldPreparationDefinition([new(4, "Wing", false, false)],
            mapEffects: [new(4, FieldRecoveryEffect.NoEffect, [new("sample-field")])]);
        var session = Session(leader: [4, 127, 127, 127], preparation: admitted);
        var before = session.Current.Exploration!.Party;
        Assert.Equal(FieldRecoveryEffect.NoEffect, session.QueryFieldActions(FieldInventoryTests.Giver).Items.Single().Use);
        Assert.Null(Send(session, new FieldUseItem(FieldInventoryTests.Giver, 0, 4)).Failure);
        Assert.Same(before, session.Current.Exploration.Party);
        var excluded = new FieldPreparationDefinition([new(4, "Wing", false, false)],
            mapEffects: [new(4, FieldRecoveryEffect.NoEffect, [new("other-field")])]);
        session = Session(leader: [4, 127, 127, 127], preparation: excluded);
        var snapshot = session.Current;
        Assert.Null(session.QueryFieldActions(FieldInventoryTests.Giver).Items.Single().Use);
        Assert.Equal("field-item-effect-unsupported", Send(session, new FieldUseItem(FieldInventoryTests.Giver, 0, 4)).Failure!.Code);
        Assert.Same(snapshot, session.Current);
    }

    [Fact]
    public void FieldOnlyCatalogItemKeepsItsNameAndUnsellableRuleWithoutEnablingBattleDiscardOrUse()
    {
        var document = Document("localization-pilot");
        document["start"]!.AsObject().Remove("program");
        document["start"]!["position"]!["x"] = 2;
        document["world"]!["maps"]![0]!["entities"] = new JsonArray();
        document["world"]!["maps"]![0]!["events"] = new JsonArray();
        document["world"]!["maps"]![0]!["search"]!["chests"]![0]!["itemWord"] = 113;
        var session = Assert.IsType<SessionStarted>(GameSession.Start(Reader(document))).Session;
        Assert.All(session.Definition.Encounters.Values, encounter => Assert.Null(encounter.Discard));
        var before = session.Current;
        var found = Search(session);
        Assert.Equal(FieldSearchKind.Item, found.Kind); Assert.Equal((ushort)113, found.ItemWord);
        Assert.Equal("Practice Token", found.ItemName); Assert.Null(found.Message);
        Assert.Equal(before.Revision + 1, session.Current.Revision);
        Close(session);
        var actor = found.Recipient!.Value;
        var inventory = session.QueryFieldActions(actor);
        Assert.Null(inventory.Error);
        var item = Assert.Single(inventory.Items);
        Assert.Equal("Practice Token", item.Name); Assert.False(item.CanDrop); Assert.Null(item.Use);
        Assert.Empty(inventory.Spells);
        Assert.Equal("Practice Token", session.QueryPartyDetails()!.Members.Single(member => member.Carried.Actor == actor).Inventory[0].Name);
        var settled = session.Current;
        Assert.NotNull(Send(session, new FieldUseItem(actor, item.Slot, item.Word)).Failure);
        Assert.NotNull(Send(session, new FieldDrop(actor, item.Slot, item.Word)).Failure);
        Assert.Same(settled, session.Current);
        Assert.Equal(FieldSearchKind.Empty, Search(session).Kind);
        Assert.Same(settled.Exploration!.Party, session.Current.Exploration!.Party);
    }

    [Fact]
    public void RequiredSearchTextIsAdmittedBeforeAnySessionCanGrantAnItem()
    {
        var document = Document("harbor-arrival");
        document["world"]!["maps"]![0]!["search"] = JsonNode.Parse("""
            {"chests":[],"otherItems":[],"descriptions":[],"closedChest":55297,"openChest":55298}
            """);
        Assert.Equal("search-text-required", Assert.IsType<SessionStartFailed>(GameSession.Start(Reader(document))).Failure.Code);
        var texts = document["world"]!["texts"]!.AsArray();
        foreach (int id in new[] { 403, 404, 405, 408, 409, 410, 412, 423, 427 })
            texts.Add(new JsonObject { ["id"] = id, ["text"] = "authored search" });
        Assert.IsType<SessionStarted>(GameSession.Start(Reader(document)));
        texts.RemoveAt(texts.Count - 1);
        Assert.Equal("search-text-required", Assert.IsType<SessionStartFailed>(GameSession.Start(Reader(document))).Failure.Code);
    }

    [Fact]
    public void SameMapReentryCanRepeatAKnownRemovalWithoutResolvingAnAbsentAliasToPlayer()
    {
        var battle = FieldInventoryTests.Session().Definition.Encounters.Values.Single();
        var map = new ExplorationMapDefinition(new("repeat-field"), new(new ushort[4096]), new([new(0, 0, 63, 63)]),
            [new(new("entity-128"), new(5, 5), 0, 32, Sprite: 30)], [], onLoad: new("remove", 0),
            population: new(30, 128, 0, []));
        var definition = new ScenarioDefinition("repeat-field-test", [battle], exploration:
            new([map], [new("remove", [new HideMapEntity(new("entity-128"), true), new EndProgram()])]));
        var start = new ExplorationStartInput(map.Map, new("entity-0"), new(1, 1), 0, 32, [],
            new(battle.Encounter, [], 123, 456, 0));
        var session = Assert.IsType<SessionStarted>(GameSession.Start(definition, start)).Session;
        var before = session.Current;
        Assert.False(before.Exploration!.TryResolveEntity(new("entity-128"), out _));
        var transferred = MapTransfer.Apply(definition, before, map.Map, new(2, 2), 1,
            MapLoadMode.Preserve, before.Story, [], RuleCompositions.Sf2());
        var result = ProgramRunner.Run(definition, transferred, [], RuleCompositions.Sf2());
        Assert.Null(result.Failure); Assert.True(result.Snapshot.CanWaitAtInput);
        Assert.Equal(new MapPosition(2, 2), result.Snapshot.Exploration!.PlayerEntity.Position);
        Assert.False(result.Snapshot.Exploration.TryResolveEntity(new("entity-128"), out _));
        Assert.Equal(before.Exploration.AllEntities.Single(row => row.Entity == new EntityRef("entity-128")),
            result.Snapshot.Exploration.AllEntities.Single(row => row.Entity == new EntityRef("entity-128")));
        Assert.Throws<BattleRuleException>(() => result.Snapshot.Exploration.Hide(new EntityRef("entity-200"), true));
    }

    [Fact]
    public void StaleInputAndModalActionsLeaveAcquiredStateUntouched()
    {
        var session = Session(); var initial = session.Current;
        Search(session); var acquired = session.Current;
        Assert.Equal("stale-input", session.Submit(new(initial.SessionId, initial.Revision, null, new Search())).Failure!.Code);
        Assert.NotNull(Send(session, new FieldDrop(FieldInventoryTests.Giver, 0, 0)).Failure);
        Assert.NotNull(Send(session, new Search()).Failure);
        Assert.Same(acquired, session.Current);
        Close(session);
    }

    [Fact]
    public void UnavailableWarpDoesNotCommitFacingMotionFlagsOrRngAndCanBeDismissed()
    {
        var session = Session(0x1000, events: [new(ExplorationEventKind.Warp, 2, 1, null, new("later", 0),
            RequiredMarker: 0x1000, Unavailable: "Later destination unavailable")]);
        var before = session.Current;
        Assert.Null(Send(session, new Move(ExplorationDirection.East)).Failure);
        Assert.Equal(FieldSearchKind.Unavailable, Assert.IsType<FieldSearchWait>(session.Current.Story.Wait).Result.Kind);
        Assert.Same(before.Exploration, session.Current.Exploration);
        Assert.Equal(before.Story.Flags, session.Current.Story.Flags); Close(session);
        Assert.True(session.Current.CanWaitAtInput);
    }

    [Fact]
    public void UnavailableFeedbackKeepsThePrecedingFieldMusicBindingObservation()
    {
        var (source, data) = OpeningRestoreTests.Fixture();
        var maps = source.Exploration!.Maps.Values.Select(map => new ExplorationMapDefinition(map.Map,
            map.Layout, map.Traversal, map.Entities,
            [new(ExplorationEventKind.Warp, 56, 4, null, new("later", 0), Unavailable: "Unavailable")],
            population: map.Population, basePalette: map.BasePalette, viewAreas: map.ViewAreas)).ToArray();
        var definition = new ScenarioDefinition(source.Package, source.Encounters.Values, exploration:
            new(maps, [], provenance: source.Exploration.Provenance, partyFlags: source.Exploration.PartyFlags,
                visuals: source.Exploration.Visuals)) { Persistence = source.Persistence };
        var restored = Assert.IsType<SessionStarted>(GameSession.Restore(definition, data, RuleCompositions.Sf2())).Session.Current;
        var before = restored.WithStory(restored.Story.Copy(null,
            music: new(1, "previous", [], new("map-57"), 0, null, HistoryBound: false)));
        var result = ExplorationDispatcher.Submit(definition, before, new Move(ExplorationDirection.South));
        Assert.Null(result.Failure);
        Assert.Equal(new[] { "map-music-bound", "field-search" }, result.Observations.Select(row => row.Kind));
        Assert.Equal(before.ObservationSequence + 1, result.Observations[0].Sequence);
        Assert.Equal(before.ObservationSequence + 2, result.Observations[1].Sequence);
        Assert.Same(before.Exploration, result.Snapshot.Exploration);
        Assert.IsType<FieldSearchWait>(result.Snapshot.Story.Wait);
    }

    [Fact]
    public void SaveAndRestoreCarryFieldReceiptAndReconstructChestWithoutRegranting()
    {
        var (source, data) = OpeningRestoreTests.Fixture();
        var maps = source.Exploration!.Maps.Values.Select(map =>
        {
            var words = map.Layout.Words.ToArray(); words[4 * 64 + 56] = 0xD801;
            return new ExplorationMapDefinition(map.Map, new(words), map.Traversal, map.Entities, map.Events,
                population: map.Population, basePalette: map.BasePalette, viewAreas: map.ViewAreas,
                search: new([new(new(56, 4), 150, 0, null)], [], [], 0xD801, 0xD802));
        }).ToArray();
        var definition = new ScenarioDefinition(source.Package, source.Encounters.Values, exploration:
            new(maps, [], new[] { 403, 408 }.Select(id => new KeyValuePair<int, string>(id, "authored")), source.Exploration.Provenance,
                source.Exploration.PartyFlags, source.Exploration.Visuals))
        { Persistence = source.Persistence, FieldPreparation = new([new(0, "Herb", false, false)]) };
        var session = Assert.IsType<SessionStarted>(GameSession.Restore(definition, data, RuleCompositions.Sf2())).Session;
        Assert.Equal(FieldSearchKind.Item, Search(session).Kind); Close(session);
        var saved = session.CaptureSave(session.Current.SessionId, session.Current.Revision);
        Assert.Null(saved.Error); Assert.Contains(150, saved.Data!.Flags);
        var restored = Assert.IsType<SessionStarted>(GameSession.Restore(definition, saved.Data, RuleCompositions.Sf2())).Session;
        Assert.Equal((ushort)0xD802, restored.Current.Exploration!.Layout[56, 4]);
        Assert.Equal(FieldSearchKind.Empty, Search(restored).Kind);
        Assert.Equal((ushort)0, restored.Current.Exploration.Party.Actors.First().SourceLoadout!.Items[0]);
    }
}
