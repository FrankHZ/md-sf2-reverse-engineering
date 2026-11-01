using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class FieldInventoryTests
{
    internal static readonly ActorRef Giver = new("champion"), Recipient = new("receiver");

    internal static GameSession Session(Action<JsonNode>? change = null,
        Func<BattleActorStartInput, BattleActorStartInput>? carried = null, FieldPreparationDefinition? preparation = null,
        BattleItemCustody? custody = null)
    {
        var document = Document("battle-give"); change?.Invoke(document);
        var read = Assert.IsType<ScenarioReadAccepted>(Reader(document).Read());
        var field = Assert.IsType<ExplorationReadAccepted>(new AuthoredScenarioPackageReader(PathFor("harbor-arrival")).Read());
        var battle = read.Definition.Encounters[read.Start.Encounter];
        var definition = new ScenarioDefinition("field-preparation-test", [battle], exploration: field.Definition.Exploration)
        {
            FieldPreparation = preparation ?? new(battle.Give!.Items.Select(item => new BattleDiscardItem(item.Key, item.Value, item.Key == 61, item.Key == 50))),
        };
        var start = field.Start;
        return Assert.IsType<SessionStarted>(GameSession.Start(definition, new ExplorationStartInput(start.Map, start.Player,
            start.Position, start.Facing, start.Speed, [], new BattleStartInput(read.Start.Encounter,
                read.Start.Actors.Select(actor => carried?.Invoke(actor) ?? actor), read.Start.MainSeed, read.Start.ThinkingSeed,
                read.Start.Gold, read.Start.NewBattle, read.Start.ActiveAllies, custody ?? read.Start.ItemCustody)))).Session;
    }

    [Fact]
    public void QueryAndCancelledPreviewPreserveTheWholeSessionThenEquipPublishesOnce()
    {
        var session = Session(); var before = session.Current;
        var query = session.QueryFieldActions(Giver);
        Assert.Null(query.Error); Assert.NotEmpty(query.Equipment);
        Assert.Same(before, session.Current);
        var selected = query.Equipment.Single(row => row.Slot == 4);
        Assert.Equal(20, selected.Attack);
        // Preview and cancel are disposable view state; no command is submitted.
        Assert.Same(before, session.Current);
        Accept(session, new FieldEquip(Giver, 4, null));
        Assert.Equal(before.Revision + 1, session.Current.Revision);
        Assert.Equal((byte)20, session.QueryPartyDetails()!.Members.Single(row => row.Carried.Actor == Giver).Attack!.Value);
        Assert.Equal(before.Exploration!.Party.MainSeed, session.Current.Exploration!.Party.MainSeed);
        Assert.Equal(before.Exploration.Party.ThinkingSeed, session.Current.Exploration.Party.ThinkingSeed);
        Assert.Same(before.Story, session.Current.Story);
        Assert.Equal(before.Exploration.Party.Gold, session.Current.Exploration.Party.Gold);
        var stale = session.Submit(new(before.SessionId, before.Revision, null, new FieldEquip(Giver, 4, null)));
        Assert.Equal("stale-input", stale.Failure!.Code);
        Assert.Equal(before.Revision + 1, session.Current.Revision);
    }

    [Fact]
    public void DuplicateSlotsAndUnrelatedFullWordsSurviveEquipmentAndDerivedAttack()
    {
        var session = Session(carried: actor => actor.Actor == Giver
            ? actor with { SourceLoadout = new([178, 50, 256, 127], []) } : actor);
        var query = session.QueryFieldActions(Giver); Assert.Null(query.Error);
        Assert.Equal(new[] { 0, 1, 4 }, query.Equipment.Select(row => row.Slot));
        Accept(session, new FieldEquip(Giver, 1, 50));
        var member = session.QueryPartyDetails()!.Members.Single(row => row.Carried.Actor == Giver);
        Assert.Equal(new ushort[] { 50, 178, 256, 127 }, member.Carried.SourceLoadout!.Items);
        Assert.Equal((byte)25, member.Attack!.Value);
        Assert.Same(member.Carried.SourceLoadout, member.Carried.Progress?.SourceLoadout ?? member.Carried.SourceLoadout);
        var before = session.Current;
        Assert.Equal("field-stale-item", Send(session, new FieldEquip(Giver, 0, 178)).Failure!.Code);
        Assert.Same(before, session.Current);
    }

    [Fact]
    public void GiveEquippedLastWeaponLeavesDonorUnarmedAndRecipientUnequipped()
    {
        var session = Session(d => d["actors"]![0]!["items"] = new JsonArray(178, 127, 127, 127));
        var before = session.Current;
        Accept(session, new FieldGive(Giver, 0, 178, Recipient));
        var party = session.QueryPartyDetails()!;
        var giver = party.Members.Single(row => row.Carried.Actor == Giver);
        var recipient = party.Members.Single(row => row.Carried.Actor == Recipient);
        Assert.All(giver.Carried.SourceLoadout!.Items, word => Assert.Equal(127, word));
        Assert.Equal((byte)20, giver.Attack!.Value);
        Assert.Contains((ushort)50, recipient.Carried.SourceLoadout!.Items);
        Assert.Same(before.Exploration!.Party.ItemCustody, session.Current.Exploration!.Party.ItemCustody);
        Assert.Equal(before.Exploration.Party.ActiveAllies, session.Current.Exploration.Party.ActiveAllies);
    }

    [Fact]
    public void FullPackRequiresExplicitExchangeAndRecalculatesBothWeapons()
    {
        var session = Session(d => d["actors"]![1]!["items"] = new JsonArray(189, 0, 0, 50));
        var before = session.Current;
        Assert.Equal("field-give-exchange-required", Send(session, new FieldGive(Giver, 0, 178, Recipient)).Failure!.Code);
        Assert.Same(before, session.Current);
        Accept(session, new FieldGive(Giver, 0, 178, Recipient, 0, 189));
        var giver = session.QueryPartyDetails()!.Members.Single(row => row.Carried.Actor == Giver);
        var recipient = session.QueryPartyDetails()!.Members.Single(row => row.Carried.Actor == Recipient);
        Assert.Contains((ushort)61, giver.Carried.SourceLoadout!.Items);
        Assert.Equal(new ushort[] { 0, 0, 50, 50 }, recipient.Carried.SourceLoadout!.Items);
        Assert.DoesNotContain(giver.Carried.SourceLoadout.Items, word => (word & 128) != 0);
        Assert.Equal(before.Revision + 1, session.Current.Revision);
    }

    [Theory]
    [InlineData(-1, 178)] [InlineData(3, 127)] [InlineData(0, 50)]
    public void BadSlotsAndStaleWordsRejectWithoutPartialPublication(int slot, ushort word)
    {
        var session = Session(); var before = session.Current;
        Assert.NotNull(Send(session, new FieldGive(Giver, slot, word, Recipient)).Failure);
        Assert.Same(before, session.Current);
        Assert.NotNull(Send(session, new FieldGive(Giver, 0, 178, new("missing"))).Failure);
        Assert.Same(before, session.Current);
    }

    [Fact]
    public void DeadMembersKeepInventoryAccessButBusyFieldRejectsBeforeMutation()
    {
        var session = Session(d => d["start"]!["actors"]![1]!["hp"] = 0); var before = session.Current;
        Accept(session, new FieldGive(Giver, 0, 178, Recipient));
        Assert.Equal(0, session.Current.Exploration!.Party.Actors.Single(actor => actor.Actor == Recipient).Hp);
        var dead = session.QueryFieldActions(Recipient); Assert.Null(dead.Error);
        Accept(session, new FieldEquip(Recipient, 4, null));
        Accept(session, new Move(Sf2.Remake.Domain.Maps.ExplorationDirection.South));
        var busy = session.Current;
        Assert.Equal("field-preparation-unavailable", Send(session, new FieldEquip(Giver, 4, null)).Failure!.Code);
        Assert.Same(busy, session.Current);
    }

    [Fact]
    public void UnsellableDropAndUnknownRareCustodyRejectButOrdinaryDropCompacts()
    {
        var session = Session(); var before = session.Current;
        Assert.Equal("discard-unsellable", Send(session, new FieldDrop(Giver, 0, 178)).Failure!.Code);
        Assert.Same(before, session.Current);
        var ordinary = session.QueryFieldActions(Giver).Items.First(row => row.Word == 0);
        Accept(session, new FieldDrop(Giver, ordinary.Slot, ordinary.Word));
        Assert.Equal(before.Revision + 1, session.Current.Revision);
        Assert.Equal(127, session.Current.Exploration!.Party.Actors.First(row => row.Actor == Giver).SourceLoadout!.Items[^1]);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void FieldGiveToSelfChangesHandsWithoutLosingOrDuplicatingAnItem(bool full)
    {
        var session = Session(d => d["actors"]![0]!["items"] = full ? new JsonArray(178, 0, 0, 50) : new JsonArray(178, 0, 127, 127));
        var before = FieldInventory.Loadout(session.QueryPartyDetails()!.Members.Single(row => row.Carried.Actor == Giver)).Items;
        Accept(session, new FieldGive(Giver, 0, 178, Giver, full ? 1 : null, full ? (ushort)0 : null));
        var after = session.Current.Exploration!.Party.Actors.Single(actor => actor.Actor == Giver).SourceLoadout!.Items;
        Assert.Equal(before.Select(word => word & 127).Order(), after.Select(word => word & 127).Order());
        Assert.All(after, word => Assert.Equal(0, word & 128));
    }

    [Theory]
    [InlineData(0, 50, 0, 61, 50)]
    [InlineData(1, 61, 50, 0, 50)]
    [InlineData(2, 0, 50, 61, 50)]
    [InlineData(3, 0, 61, 50, 50)]
    public void FullSelfExchangeUsesTheSourceAdjustedSlotAndEqualSlotUnequip(int returned, ushort a, ushort b, ushort c, ushort d)
    {
        var session = Session(document => document["actors"]![0]!["items"] = new JsonArray(178, 0, 61, 50));
        ushort word = new ushort[] { 178, 0, 61, 50 }[returned];
        Accept(session, new FieldGive(Giver, 0, 178, Giver, returned, word));
        Assert.Equal(new[] { a, b, c, d }, session.Current.Exploration!.Party.Actors.Single(actor => actor.Actor == Giver).SourceLoadout!.Items);
    }

    [Fact]
    public void AStaleReturnWordRejectsTheWholeExchange()
    {
        var session = Session(document => document["actors"]![1]!["items"] = new JsonArray(189, 0, 0, 50));
        var before = session.Current;
        Assert.Equal("field-stale-item", Send(session, new FieldGive(Giver, 0, 178, Recipient, 0, 61)).Failure!.Code);
        Assert.Same(before, session.Current);
    }

    [Fact]
    public void RareDropRequiresKnownCustodyAddsOneSaturatingDealAndPreservesOnceFlags()
    {
        GameSession Rare(BattleItemCustody? custody = null) => Session(carried: actor => actor.Actor == Giver
            ? actor with { SourceLoadout = new([189, 127, 127, 127], []) } : actor, custody: custody);
        var unknown = Rare(); var before = unknown.Current;
        Assert.Equal("discard-custody-unavailable", Send(unknown, new FieldDrop(Giver, 0, 189)).Failure!.Code);
        Assert.Same(before, unknown.Current);
        var session = Rare(new([2], new Dictionary<byte, byte> { [61] = 15 }));
        Accept(session, new FieldDrop(Giver, 0, 189));
        Assert.Equal((byte)15, session.Current.Exploration!.Party.ItemCustody!.Deals[61]);
        Assert.Equal(new byte[] { 2 }, session.Current.Exploration.Party.ItemCustody.OnceFlags);
        Assert.Equal((byte)20, session.QueryPartyDetails()!.Members.Single(row => row.Carried.Actor == Giver).Attack!.Value);
    }
}
