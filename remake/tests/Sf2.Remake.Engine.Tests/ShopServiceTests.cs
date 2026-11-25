using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class ShopServiceTests
{
    internal static readonly ActorRef Buyer = FieldInventoryTests.Giver;
    internal static readonly ShopDefinition Terms = new([0, 50], [new(0, "Herb", 11), new(50, "Sword", 40),
        new(56, "Other", 100), new(61, "Rare", 77, Rare: true), new(113, "Key", 100, Unsellable: true)]);

    internal static GameSession Session(uint? gold = 100, ushort[]? items = null, bool failedReturn = false,
        BattleItemCustody? custody = null, ShopDefinition? terms = null, bool repairGear = false)
    {
        var original = FieldInventoryTests.Session();
        var battle = original.Definition.Encounters.Values.Single();
        if (repairGear) battle = battle.WithDeployments(battle.Deployments.Select(row => row.Actor == Buyer
            ? row with { Definition = row.Definition.WithEquipment(new(row.Definition.Equipment!.BaseAttack,
                row.Definition.Equipment.Weapons.Values.Concat([
                    new(92, "Indra Staff", uint.MaxValue, 25, 1, 1, DamageOnUse: true),
                    new(101, "Demon Rod", uint.MaxValue, 50, 1, 1, DamageOnUse: true, Cursed: true)]),
                rings: [new(19, "Ring", uint.MaxValue, RingStat.Attack, 5, DamageOnUse: true),
                    new(24, "Black Ring", uint.MaxValue, RingStat.Attack, 10, DamageOnUse: true, Cursed: true)])) }
            : row));
        var map = new ExplorationMapDefinition(new("market"), new(new ushort[WorkingMapLayout.WordCount]),
            new([new(0, 0, 63, 63)]), [new(new("trader"), new(2, 1), 3, 32)],
            [new(ExplorationEventKind.Interact, null, null, new("trader"), new("caller", 0))]);
        var world = new ExplorationDefinition([map], [new("caller", [new CallProgram(new("trade", 0)),
            new WriteFlag(999, true), failedReturn ? new UnsupportedInstruction("stop", "caller") : new EndProgram()]),
            new("trade", [new OpenShop(terms ?? Terms), new ReturnProgram()])], []);
        var definition = new ScenarioDefinition("shop-test", [battle], exploration: world) { FieldPreparation = original.Definition.FieldPreparation };
        var party = original.Current.Exploration!.Party;
        var start = new BattleStartInput(party.Encounter, party.Actors.Select(actor => actor.Actor == Buyer && items is not null
            ? actor with { SourceLoadout = new(items, []) } : actor), party.MainSeed, party.ThinkingSeed, gold,
            activeAllies: party.ActiveAllies, itemCustody: custody ?? party.ItemCustody);
        var session = Assert.IsType<SessionStarted>(GameSession.Start(definition,
            new ExplorationStartInput(map.Map, new("player"), new(2, 2), 1, 32, [], start))).Session;
        Accept(session, new Interact(new("trader"))); return session;
    }
    internal static ShopWait Wait(GameSession session) => Assert.IsType<ShopWait>(session.Current.Story.Wait);
    internal static void Choose(GameSession session, ShopAction action) => Accept(session, new ChooseShop(Wait(session).Token, action));
    internal static void Buy(GameSession session, byte id, ActorRef member, ShopAction action = ShopAction.Buy)
    {
        Choose(session, action); Accept(session, new SelectShopItem(Wait(session).Token, id));
        Accept(session, new SelectShopMember(Wait(session).Token, member));
    }
    internal static SessionResult Pay(GameSession session, bool yes = true) => Accept(session, new AnswerShop(Wait(session).Token, Wait(session).Offer!, yes));
    internal static void Continue(GameSession session) => Accept(session, new ContinueShop(Wait(session).Token));
    internal static BattleActorStartInput Owner(GameSession session) => session.Current.Exploration!.Party.Actors.Single(row => row.Actor == Buyer);
    private static BattleStartInput Party(GameSession session) => session.Current.Exploration!.Party;

    [Fact]
    public void DealsFiltersCurrentStockAndBuysLastCopyExactlyOnceAtListPrice()
    {
        var custody = new BattleItemCustody([4], new Dictionary<byte, byte> { [0] = 2, [56] = 0, [61] = 1, [99] = 2 }, [50]);
        var session = Session(77, [127, 127, 127, 127], custody: custody);
        Choose(session, ShopAction.Deals);
        Assert.Equal(new byte[] { 61 }, Wait(session).AvailableStock(Party(session).ItemCustody));
        foreach (byte item in new byte[] { 0, 56, 99 })
            Assert.NotNull(Send(session, new SelectShopItem(Wait(session).Token, item)).Failure);
        Accept(session, new SelectShopItem(Wait(session).Token, 61));
        Accept(session, new SelectShopMember(Wait(session).Token, Buyer));
        var wait = Wait(session);
        Assert.Equal(77u, wait.Offer!.Price);
        Pay(session);
        var bought = Party(session);
        Assert.Equal(0u, bought.Gold);
        Assert.Equal(new ushort[] { 61, 127, 127, 127 }, Owner(session).SourceLoadout!.Items);
        Assert.Equal(new byte[] { 4 }, bought.ItemCustody!.OnceFlags);
        Assert.Equal(new byte[] { 50 }, bought.ItemCustody.Depot);
        Assert.Equal(2, bought.ItemCustody.Deals[0]);
        Assert.Equal(2, bought.ItemCustody.Deals[99]);
        Assert.Equal(0, bought.ItemCustody.Deals.GetValueOrDefault((byte)61));
        Assert.Equal(1, custody.Deals[61]);
        Assert.NotNull(Send(session, new AnswerShop(wait.Token, wait.Offer, true)).Failure);
        Assert.Same(bought, Party(session));
        Continue(session);
        Choose(session, ShopAction.Deals);
        Assert.Empty(Wait(session).AvailableStock(Party(session).ItemCustody));
        Assert.NotNull(Send(session, new SelectShopItem(Wait(session).Token, 61)).Failure);
        Accept(session, new BackShop(Wait(session).Token));
        Accept(session, new BackShop(Wait(session).Token));
        Assert.True(session.Current.CanWaitAtInput);
        Assert.Contains(999, session.Current.Story.Flags);
    }

    [Theory]
    [InlineData(76u, false, true, ShopResult.InsufficientGold)]
    [InlineData(77u, true, true, ShopResult.Full)]
    [InlineData(77u, false, false, null)]
    public void RefusedOrCancelledDealsPreserveTheWholeParty(uint gold, bool full, bool confirm, ShopResult? result)
    {
        var custody = new BattleItemCustody([], new Dictionary<byte, byte> { [61] = 2 });
        var session = Session(gold, full ? [0, 0, 0, 0] : [127, 127, 127, 127], custody: custody);
        var party = Party(session);
        Buy(session, 61, Buyer, ShopAction.Deals);
        Pay(session, confirm);
        Assert.Same(party, Party(session));
        Assert.Equal(result, Wait(session).Result);
        if (confirm) return;
        Assert.Equal(ShopPhase.Member, Wait(session).Phase);
        Accept(session, new BackShop(Wait(session).Token));
        Assert.Equal(ShopAction.Deals, Wait(session).Action);
        Assert.Equal(new byte[] { 61 }, Wait(session).AvailableStock(Party(session).ItemCustody));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(14, 15)]
    [InlineData(15, 15)]
    public void RareSaleRoutesIntoTheSameSaturatingStoreAndCanBeBoughtBack(byte initial, byte afterSale)
    {
        var custody = new BattleItemCustody([], new Dictionary<byte, byte> { [61] = initial });
        var session = Session(100, [61, 0, 127, 127], custody: custody);
        Choose(session, ShopAction.Sell);
        Accept(session, new SelectShopMember(Wait(session).Token, Buyer));
        Accept(session, new SelectShopSlot(Wait(session).Token, 0, 61));
        Assert.Equal(57u, Wait(session).Offer!.Price);
        var beforeSale = Party(session);
        Pay(session, false);
        Assert.Same(beforeSale, Party(session));
        Accept(session, new SelectShopMember(Wait(session).Token, Buyer));
        Accept(session, new SelectShopSlot(Wait(session).Token, 0, 61));
        Pay(session);
        Assert.Equal(157u, Party(session).Gold);
        Assert.Equal(new ushort[] { 0, 127, 127, 127 }, Owner(session).SourceLoadout!.Items);
        Assert.Equal(afterSale, Party(session).ItemCustody!.Deals[61]);
        Continue(session);
        Buy(session, 61, Buyer, ShopAction.Deals);
        Pay(session);
        Assert.Equal(80u, Party(session).Gold);
        Assert.Equal(new ushort[] { 0, 61, 127, 127 }, Owner(session).SourceLoadout!.Items);
        Assert.Equal(afterSale - 1, Party(session).ItemCustody!.Deals.GetValueOrDefault((byte)61));
    }

    [Fact]
    public void EquippedCursedItemCannotBeSold()
    {
        var terms = new ShopDefinition([0], [Terms.Items[0], Terms.Items[50] with { Rare = true, Cursed = true }]);
        var session = Session(items: [178, 127, 127, 127],
            custody: new BattleItemCustody([], new Dictionary<byte, byte>()), terms: terms);
        var party = Party(session);
        Choose(session, ShopAction.Sell);
        Accept(session, new SelectShopMember(Wait(session).Token, Buyer));
        Accept(session, new SelectShopSlot(Wait(session).Token, 0, 178));
        Assert.Equal(ShopResult.EquippedCurse, Wait(session).Result);
        Assert.Same(party, Party(session));
    }

    [Theory]
    [InlineData(408, ShopResult.EquippedCurse)]
    [InlineData(280, ShopResult.Repaired)]
    public void BlackRingRepairChargesQuarterPriceOnlyForTheBareDamagedSelection(ushort word, ShopResult expected)
    {
        var terms = new ShopDefinition([0], [Terms.Items[0],
            new(24, "Black Ring", 5000, Rare: true, Cursed: true, Repairable: true)]);
        var session = Session(1250, [24, word, 0, 127], terms: terms, repairGear: true);
        var before = Party(session);
        Choose(session, ShopAction.Repair);
        Accept(session, new SelectShopMember(Wait(session).Token, Buyer));
        Accept(session, new SelectShopSlot(Wait(session).Token, 1, word));
        Assert.Equal(1250u, Wait(session).Offer!.Price);
        Pay(session);
        Assert.Equal(expected, Wait(session).Result);
        if (expected == ShopResult.Repaired)
        {
            Assert.Equal(0u, Party(session).Gold);
            Assert.Equal(new ushort[] { 24, 24, 0, 127 }, Owner(session).SourceLoadout!.Items);
        }
        else Assert.Same(before, Party(session));
    }

    [Theory]
    [InlineData(485, ShopResult.EquippedCurse)]
    [InlineData(357, ShopResult.Repaired)]
    public void WeaponRepairUsesTheSameCurseLockAndPreservesTheOtherSlot(ushort word, ShopResult expected)
    {
        var terms = new ShopDefinition([0], [Terms.Items[0],
            new(101, "Demon Rod", 12500, Rare: true, Cursed: true, Repairable: true)]);
        var session = Session(3125, [101, word, 0, 127], terms: terms, repairGear: true);
        var before = Party(session);
        Choose(session, ShopAction.Repair);
        Accept(session, new SelectShopMember(Wait(session).Token, Buyer));
        Accept(session, new SelectShopSlot(Wait(session).Token, 1, word));
        Assert.Equal(3125u, Wait(session).Offer!.Price);
        Pay(session);
        Assert.Equal(expected, Wait(session).Result);
        if (expected == ShopResult.Repaired)
        {
            Assert.Equal(0u, Party(session).Gold);
            Assert.Equal(new ushort[] { 101, 101, 0, 127 }, Owner(session).SourceLoadout!.Items);
        }
        else Assert.Same(before, Party(session));
    }

    [Theory]
    [InlineData(750u, 403, false, ShopResult.Repaired)]
    [InlineData(750u, 275, true, ShopResult.Repaired)]
    [InlineData(749u, 403, true, ShopResult.InsufficientGold)]
    [InlineData(750u, 403, true, ShopResult.EquippedCurse)]
    [InlineData(750u, 147, false, ShopResult.Undamaged)]
    public void RepairChecksTheLiveWordAndCommitsOnlyPriceAndBrokenBit(
        uint gold, ushort word, bool cursed, ShopResult expected)
    {
        var terms = new ShopDefinition([0], [Terms.Items[0], new(19, "Ring", 3000, Rare: true, Cursed: cursed, Repairable: true)]);
        var session = Session(gold, [0, word, 19, 0], terms: terms, repairGear: true);
        var before = Party(session);
        var owner = Owner(session);
        Choose(session, ShopAction.Repair);
        Accept(session, new SelectShopMember(Wait(session).Token, Buyer));
        Assert.NotNull(Send(session, new SelectShopSlot(Wait(session).Token, 1, (ushort)(word ^ 256))).Failure);
        Accept(session, new SelectShopSlot(Wait(session).Token, 1, word));
        if (expected != ShopResult.Undamaged)
        {
            Assert.Equal(750u, Wait(session).Offer!.Price);
            Pay(session, false);
            Assert.Same(before, Party(session));
            Accept(session, new SelectShopMember(Wait(session).Token, Buyer));
            Accept(session, new SelectShopSlot(Wait(session).Token, 1, word));
            var quoted = Wait(session);
            Pay(session);
            Assert.NotNull(Send(session, new AnswerShop(quoted.Token, quoted.Offer!, true)).Failure);
        }
        Assert.Equal(expected, Wait(session).Result);
        if (expected == ShopResult.Repaired)
        {
            Assert.Equal(0u, Party(session).Gold);
            Assert.Equal(new ushort[] { 0, (ushort)(word & ~256), 19, 0 }, Owner(session).SourceLoadout!.Items);
            Assert.Equal(owner.Status, Owner(session).Status);
            Assert.Equal(before.MainSeed, Party(session).MainSeed);
            Assert.Same(before.ItemCustody, Party(session).ItemCustody);
        }
        else Assert.Same(before, Party(session));
        Continue(session);
        Assert.Equal(ShopPhase.Member, Wait(session).Phase);
        Accept(session, new BackShop(Wait(session).Token));
        Accept(session, new BackShop(Wait(session).Token));
        Assert.True(session.Current.CanWaitAtInput);
        Assert.Contains(999, session.Current.Story.Flags);
    }

    [Fact]
    public void IndraRepairQuotes800AndClearsOnlyTheSelectedWeaponsDamage()
    {
        var terms = new ShopDefinition([92], [new(92, "Indra Staff", 3200, Repairable: true)]);
        var session = Session(800, [92, 476, 0, 0], terms: terms, repairGear: true);
        Choose(session, ShopAction.Repair);
        Accept(session, new SelectShopMember(Wait(session).Token, Buyer));
        Accept(session, new SelectShopSlot(Wait(session).Token, 1, 476));
        Assert.Equal(800u, Wait(session).Offer!.Price);
        Pay(session);
        Assert.Equal(ShopResult.Repaired, Wait(session).Result);
        Assert.Equal(0u, Party(session).Gold);
        Assert.Equal(new ushort[] { 92, 220, 0, 0 }, Owner(session).SourceLoadout!.Items);
    }

    [Fact]
    public void ExactGoldPurchasePublishesOneDetachedLoadoutAndReturnsToItsRealCaller()
    {
        var session = Session(11, [127,127,127,127]); var before = session.Current;
        Buy(session, 0, Buyer); Assert.Equal(11u, session.Current.Exploration!.Party.Gold);
        Assert.Equal(new ushort[] {127,127,127,127}, Owner(session).SourceLoadout!.Items);
        var paid = Pay(session); Assert.Equal(0u, session.Current.Exploration!.Party.Gold);
        Assert.Equal(new ushort[] {0,127,127,127}, Owner(session).SourceLoadout!.Items);
        Assert.Single(paid.Observations, row => row.Kind == "shop-bought");
        Assert.Equal(before.Exploration!.Party.MainSeed, session.Current.Exploration.Party.MainSeed);
        Assert.Equal(before.Exploration.Party.ThinkingSeed, session.Current.Exploration.Party.ThinkingSeed);
        Assert.Same(before.Exploration.Party.ItemCustody, session.Current.Exploration.Party.ItemCustody);
        Continue(session); Accept(session, new BackShop(Wait(session).Token));
        Assert.True(session.Current.CanWaitAtInput); Assert.Contains(999, session.Current.Story.Flags);
    }

    [Fact]
    public void BoughtWeaponOccupiesAFreeSlotUntilEquippedFromTheField()
    {
        var session = Session(100, [0, 127, 127, 127]);
        Buy(session, 50, Buyer);
        Pay(session);
        Continue(session);
        Accept(session, new BackShop(Wait(session).Token));
        Assert.Equal(new ushort[] { 0, 50, 127, 127 }, Owner(session).SourceLoadout!.Items);
        var choice = Assert.Single(session.QueryFieldActions(Buyer).Equipment, row => row.Slot == 1);
        Assert.False(choice.Equipped);
        Accept(session, new FieldEquip(Buyer, choice.Slot, 50));
        Assert.Equal(new ushort[] { 0, 178, 127, 127 }, Owner(session).SourceLoadout!.Items);
        Assert.Equal(60u, session.Current.Exploration!.Party.Gold);
    }

    [Theory]
    [InlineData(10, false, ShopResult.InsufficientGold)]
    [InlineData(100, true, ShopResult.Full)]
    public void FailedPurchaseHasNoResourceChange(uint gold, bool full, ShopResult expected)
    {
        var session = Session(gold, full ? [0,0,0,0] : [127,127,127,127]);
        Buy(session, 0, Buyer); var party = session.Current.Exploration!.Party; Pay(session);
        Assert.Equal(expected, Wait(session).Result); Assert.Same(party, session.Current.Exploration!.Party);
    }

    [Fact]
    public void CancelAtEachSelectionAndDuplicateConfirmationNeverSpendsAgain()
    {
        var session = Session(items: [127,127,127,127]); var party = session.Current.Exploration!.Party;
        Buy(session, 0, Buyer); Pay(session, false); Assert.Equal(ShopPhase.Member, Wait(session).Phase);
        Accept(session, new BackShop(Wait(session).Token)); Assert.Equal(ShopPhase.Stock, Wait(session).Phase);
        Accept(session, new BackShop(Wait(session).Token)); Assert.Equal(ShopPhase.Menu, Wait(session).Phase);
        Assert.Same(party, session.Current.Exploration.Party);
        Buy(session, 0, Buyer); var wait = Wait(session); var before = session.Current;
        var command = new AnswerShop(wait.Token, wait.Offer!, true);
        var envelope = new CommandEnvelope(before.SessionId, before.Revision, null, command);
        Assert.Null(session.Submit(envelope).Failure); var paid = session.Current;
        Assert.Equal("stale-input", session.Submit(envelope).Failure!.Code);
        Assert.NotNull(Send(session, command).Failure); Assert.Same(paid, session.Current);
        Assert.NotNull(Send(session, new AnswerShop(Wait(session).Token, wait.Offer!, true)).Failure);
        Assert.Same(paid, session.Current); Assert.Equal(89u, paid.Exploration!.Party.Gold);
    }

    [Theory]
    [InlineData(0, 178, 37)] [InlineData(1, 50, 37)]
    public void SellUsesSelectedDuplicateSlotAndRefreshesEquippedAttack(int slot, ushort word, uint gold)
    {
        var session = Session(7, [178,50,0,127]);
        Assert.Equal((byte)25, session.QueryPartyDetails()!.Members.Single(row => row.Carried.Actor == Buyer).Attack);
        Choose(session, ShopAction.Sell); Accept(session, new SelectShopMember(Wait(session).Token, Buyer));
        Accept(session, new SelectShopSlot(Wait(session).Token, slot, word));
        Assert.Equal(30u, Wait(session).Offer!.Price); Pay(session);
        Assert.Equal(gold, session.Current.Exploration!.Party.Gold);
        Assert.Equal(slot == 0 ? (byte)20 : (byte)25, session.QueryPartyDetails()!.Members.Single(row => row.Carried.Actor == Buyer).Attack);
        Assert.Equal(slot == 0 ? new ushort[]{50,0,127,127} : new ushort[]{178,0,127,127}, Owner(session).SourceLoadout!.Items);
    }

    [Theory]
    [InlineData(113, ShopResult.Unsellable)] [InlineData(256, ShopResult.UnsupportedItem)]
    public void ProtectedAndBrokenItemsRetainAllCustody(ushort word, ShopResult expected)
    {
        var session = Session(items: [word,127,127,127]); var party = session.Current.Exploration!.Party;
        Choose(session, ShopAction.Sell); Accept(session, new SelectShopMember(Wait(session).Token, Buyer));
        Accept(session, new SelectShopSlot(Wait(session).Token, 0, word));
        Assert.Equal(expected, Wait(session).Result); Assert.Same(party, session.Current.Exploration.Party);
    }

    [Fact]
    public void BrokenRareWeaponStillFailsShopLoadoutAdmission()
    {
        var session = Session(items: [317, 127, 127, 127],
            custody: new BattleItemCustody([], new Dictionary<byte, byte>()));
        Choose(session, ShopAction.Sell);
        var before = session.Current;
        Assert.Equal("equipment-broken", Send(session, new SelectShopMember(Wait(session).Token, Buyer)).Failure!.Code);
        Assert.Same(before, session.Current);
    }

    [Fact]
    public void WrongMemberSlotItemOfferOrOrdinaryCommandCannotMutateService()
    {
        var session = Session(); Choose(session, ShopAction.Sell);
        var before = session.Current;
        Assert.NotNull(Send(session, new SelectShopMember(Wait(session).Token, new("absent"))).Failure); Assert.Same(before, session.Current);
        Accept(session, new SelectShopMember(Wait(session).Token, Buyer)); before = session.Current;
        foreach (var command in new SessionCommand[] {new SelectShopSlot(Wait(session).Token, 0, 0),new Confirm(),
            new AdvanceSimulation(),new Move(ExplorationDirection.North), new FieldDrop(Buyer, 0, 178)})
        { Assert.NotNull(Send(session, command).Failure); Assert.Same(before, session.Current); }
        Accept(session, new SelectShopSlot(Wait(session).Token, 0, 178)); before = session.Current;
        Assert.NotNull(Send(session,new AnswerShop(Wait(session).Token, Wait(session).Offer! with {Price=1},true)).Failure);
        Assert.Same(before,session.Current);
    }

    [Fact]
    public void MissingGoldIsExplicitWhileDeclineAndExitRemainAvailable()
    {
        var session=Session(null, [127,127,127,127]); Buy(session,0,Buyer); var before=session.Current;
        Assert.Equal("service-gold-unavailable",Send(session,new AnswerShop(Wait(session).Token,Wait(session).Offer!,true)).Failure!.Code);
        Assert.Same(before,session.Current); Pay(session,false);
    }

    [Theory]
    [InlineData(0u,8u)] [InlineData(9999997u,9999999u)]
    public void SaleRoundsDownAndUsesExistingGoldCap(uint initial,uint expected)
    {
        var session=Session(initial,[0,127,127,127]);Choose(session,ShopAction.Sell);
        Accept(session,new SelectShopMember(Wait(session).Token,Buyer));
        Accept(session,new SelectShopSlot(Wait(session).Token,0,0));
        Assert.Equal(8u,Wait(session).Offer!.Price);Pay(session);
        Assert.Equal(expected,session.Current.Exploration!.Party.Gold);
        Assert.Equal(new ushort[]{127,127,127,127},Owner(session).SourceLoadout!.Items);
    }

    [Fact]
    public void LaterCallerFailureKeepsCompletedPurchaseAndAttributesFailure()
    {
        var session=Session(items:[127,127,127,127],failedReturn:true); Buy(session,0,Buyer);Pay(session);Continue(session);
        var result=Send(session,new BackShop(Wait(session).Token));
        Assert.Equal("program-opcode",result.Failure!.Code);Assert.Equal(89u,session.Current.Exploration!.Party.Gold);
        Assert.Equal((ushort)0,Owner(session).SourceLoadout!.Items[0]);Assert.Contains(999,session.Current.Story.Flags);
    }
}
