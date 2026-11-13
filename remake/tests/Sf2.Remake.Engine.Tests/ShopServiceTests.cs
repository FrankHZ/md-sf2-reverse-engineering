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
        new(61, "Rare", 77, Rare: true), new(113, "Key", 100, Unsellable: true)]);

    internal static GameSession Session(uint? gold = 100, ushort[]? items = null, bool failedReturn = false)
    {
        var original = FieldInventoryTests.Session();
        var battle = original.Definition.Encounters.Values.Single();
        var map = new ExplorationMapDefinition(new("market"), new(new ushort[WorkingMapLayout.WordCount]),
            new([new(0, 0, 63, 63)]), [new(new("trader"), new(2, 1), 3, 32)],
            [new(ExplorationEventKind.Interact, null, null, new("trader"), new("caller", 0))]);
        var world = new ExplorationDefinition([map], [new("caller", [new CallProgram(new("trade", 0)),
            new WriteFlag(999, true), failedReturn ? new UnsupportedInstruction("stop", "caller") : new EndProgram()]),
            new("trade", [new OpenShop(Terms), new ReturnProgram()])], []);
        var definition = new ScenarioDefinition("shop-test", [battle], exploration: world) { FieldPreparation = original.Definition.FieldPreparation };
        var party = original.Current.Exploration!.Party;
        var start = new BattleStartInput(party.Encounter, party.Actors.Select(actor => actor.Actor == Buyer && items is not null
            ? actor with { SourceLoadout = new(items, []) } : actor), party.MainSeed, party.ThinkingSeed, gold,
            activeAllies: party.ActiveAllies, itemCustody: party.ItemCustody);
        var session = Assert.IsType<SessionStarted>(GameSession.Start(definition,
            new ExplorationStartInput(map.Map, new("player"), new(2, 2), 1, 32, [], start))).Session;
        Accept(session, new Interact(new("trader"))); return session;
    }
    internal static ShopWait Wait(GameSession session) => Assert.IsType<ShopWait>(session.Current.Story.Wait);
    internal static void Choose(GameSession session, ShopAction action) => Accept(session, new ChooseShop(Wait(session).Token, action));
    internal static void Buy(GameSession session, byte id, ActorRef member)
    {
        Choose(session, ShopAction.Buy); Accept(session, new SelectShopItem(Wait(session).Token, id));
        Accept(session, new SelectShopMember(Wait(session).Token, member));
    }
    internal static SessionResult Pay(GameSession session, bool yes = true) => Accept(session, new AnswerShop(Wait(session).Token, Wait(session).Offer!, yes));
    internal static void Continue(GameSession session) => Accept(session, new ContinueShop(Wait(session).Token));
    internal static BattleActorStartInput Owner(GameSession session) => session.Current.Exploration!.Party.Actors.Single(row => row.Actor == Buyer);

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
    [InlineData(113, ShopResult.Unsellable)] [InlineData(61, ShopResult.UnsupportedItem)] [InlineData(256, ShopResult.UnsupportedItem)]
    public void ProtectedRareAndBrokenItemsRetainAllCustody(ushort word, ShopResult expected)
    {
        var session = Session(items: [word,127,127,127]); var party = session.Current.Exploration!.Party;
        Choose(session, ShopAction.Sell); Accept(session, new SelectShopMember(Wait(session).Token, Buyer));
        Accept(session, new SelectShopSlot(Wait(session).Token, 0, word));
        Assert.Equal(expected, Wait(session).Result); Assert.Same(party, session.Current.Exploration.Party);
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
