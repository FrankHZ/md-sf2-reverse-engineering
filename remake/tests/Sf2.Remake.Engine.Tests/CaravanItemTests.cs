using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class CaravanItemTests
{
    private static CaravanWait Wait(GameSession s) => Assert.IsType<CaravanWait>(s.Current.Story.Wait);
    private static SessionResult Send(GameSession s, SessionCommand c) => s.Submit(new(s.Current.SessionId, s.Current.Revision, null, c));
    private static void Accept(GameSession s, SessionCommand c) => Assert.Null(Send(s, c).Failure);
    private static void Back(GameSession s) => Accept(s, new BackCaravan(Wait(s).Token));
    private static void Continue(GameSession s) => Accept(s, new ContinueCaravan(Wait(s).Token));
    private static void Select(GameSession s, int member) => Accept(s, new SelectCaravanMember(Wait(s).Token, member));
    private static void Slot(GameSession s, int slot, ushort? word) => Accept(s, new SelectCaravanItemSlot(Wait(s).Token, slot, word));
    private static void Action(GameSession s, CaravanItemAction action) => Accept(s, new ChooseCaravanItem(Wait(s).Token, action));
    private static ushort[] Pack(GameSession s, int member) => s.Current.Exploration!.Party.Actors[member].SourceLoadout!.Items.ToArray();

    private static GameSession Start(Func<int, BattleSourceLoadout>? pack = null, bool equipment = false,
        bool fail = false, Func<int, byte>? status = null, PrivateBattleDefinitions? source = null)
    {
        var preparation = new FieldPreparationDefinition(
            new byte[] { 0, 1, 2, 3, 5, 9, 50, 61, 125 }.Select(id => new BattleDiscardItem(id, "item-" + id, id == 61, id == 125)),
            [new(0, FieldRecoveryEffect.NoEffect), new(2, FieldRecoveryEffect.NoEffect),
             new(3, FieldRecoveryEffect.CurePoison), new(5, FieldRecoveryEffect.CurePoisonAndParalysis),
             new(125, FieldRecoveryEffect.MapEvent)]);
        var s = CaravanServiceTests.Session(pack: pack ?? (_ => new([50, 0, 61, 1], [])),
            custody: new([2], new Dictionary<byte, byte> { [61] = 15 }, [50, 50, 61]),
            preparation: preparation, equipment: equipment, failReturn: fail, status: status, privateDefinitions: source);
        Accept(s, new ChooseCaravan(Wait(s).Token, CaravanAction.Item));
        return s;
    }

    [Theory]
    [InlineData(0, 0, 61, 1, 50)]
    [InlineData(2, 50, 0, 1, 61)]
    public void FullSelfGiveRemovesSelectedSlotAndAppendsWithoutExchange(int slot, ushort a, ushort b, ushort c, ushort d)
    {
        var s = Start();
        var before = s.Current;
        Action(s, CaravanItemAction.Give);
        Select(s, 13);
        // Reserve packs use the same rule.
        ushort word = Pack(s, 13)[slot];
        Slot(s, slot, word);
        Select(s, 13);
        Assert.Equal(CaravanPhase.ItemResult, Wait(s).Phase);
        Assert.Equal(new[] { a, b, c, d }, Pack(s, 13));
        var p = s.Current.Exploration!.Party;
        Assert.Same(before.Exploration!.Party.ItemCustody, p.ItemCustody);
        Assert.Equal(before.Exploration.Party.Actors.Where((_, i) => i != 13), p.Actors.Where((_, i) => i != 13));
        Assert.Equal(before.Exploration.Party.MainSeed, p.MainSeed);
        Assert.Equal(before.Exploration.Party.ThinkingSeed, p.ThinkingSeed);
        Assert.Equal(before.Exploration.Party.Gold, p.Gold);
        Assert.Equal(before.Exploration.Party.ActiveAllies, p.ActiveAllies);
        Assert.Equal(before.Story.Flags, s.Current.Story.Flags);
        Assert.Equal(before.Story.Raft, s.Current.Story.Raft);
        Continue(s);
        Back(s);
        Back(s);
        Back(s);
        Assert.True(s.Current.CanWaitAtInput);
        Assert.Contains(999, s.Current.Story.Flags);
    }

    [Fact]
    public void CrossMemberExchangeChecksBothWordsAndCompactsBothPacks()
    {
        var s = Start(i => i == 13 ? new([50, 61, 127, 127], []) : new([1, 0, 61, 50], []));
        Action(s, CaravanItemAction.Give);
        Select(s, 13);
        Slot(s, 0, 50);
        Select(s, 2);
        Assert.Equal(CaravanPhase.ItemExchange, Wait(s).Phase);
        var before = s.Current;
        Assert.Equal("field-stale-item", Send(s, new SelectCaravanItemSlot(Wait(s).Token, 2, 0)).Failure!.Code);
        Assert.Same(before, s.Current);
        Back(s);
        Assert.Equal(CaravanPhase.ItemTarget, Wait(s).Phase);
        Assert.Same(before.Exploration!.Party, s.Current.Exploration!.Party);
        Select(s, 2);
        Slot(s, 2, 61);
        Assert.Equal(new ushort[] { 61, 61, 127, 127 }, Pack(s, 13));
        Assert.Equal(new ushort[] { 1, 0, 50, 50 }, Pack(s, 2));
    }

    [Fact]
    public void OrdinaryFieldGateAndSelfExchangeRemainDistinctFromCaravan()
    {
        var s = Start();
        var before = s.Current;
        var actor = before.Exploration!.Party.Actors[0].Actor;
        Assert.Equal("field-preparation-unavailable", s.QueryFieldActions(actor).Error);
        Assert.Equal("field-preparation-unavailable", Send(s, new FieldGive(actor, 0, 50, actor, 1, 0)).Failure!.Code);
        Assert.Same(before, s.Current);
        Back(s);
        Back(s);
        Accept(s, new FieldGive(actor, 0, 50, actor, 1, 0));
        Assert.Equal(new ushort[] { 61, 1, 0, 50 }, Pack(s, 0));
        Assert.Equal("field-preparation-unavailable", CaravanItemQuery.Query(s.Definition, s.Current, actor).Error);
    }

    [Theory]
    [InlineData(3, 0x82, "cured", 0x80)]
    [InlineData(3, 0x81, "no-effect", 0x81)]
    [InlineData(5, 0x81, "powder-cured", 0x80)]
    [InlineData(5, 0x82, "powder-cured", 0x80)]
    [InlineData(5, 0x83, "powder-cured", 0x80)]
    [InlineData(5, 0x80, "powder-no-effect", 0x80)]
    public void RecoveryUsesSelectedHolderSlotEvenWhenTargetHasNoMatchingStatus(
        ushort word, byte status, string result, ushort expectedStatus)
    {
        var s = Start(_ => new([0, word, 1, 127], []), status: _ => status);
        var p = s.Current.Exploration!.Party;
        Action(s, CaravanItemAction.Use);
        Select(s, 13);
        Slot(s, 1, word);
        var choosing = s.Current;
        Assert.NotNull(Send(s, new SelectCaravanMember(Wait(s).Token, 99)).Failure);
        Assert.Same(choosing, s.Current);
        Back(s);
        Assert.Equal(CaravanPhase.ItemMember, Wait(s).Phase);
        Assert.Same(p, s.Current.Exploration!.Party);
        Select(s, 13);
        Slot(s, 1, word);
        Select(s, 2);
        Assert.Equal(result, Wait(s).ItemResult);
        Assert.Equal(new ushort[] { 0, 1, 127, 127 }, Pack(s, 13));
        Assert.Equal(new ushort[] { 0, word, 1, 127 }, Pack(s, 2));
        Assert.Equal(expectedStatus, s.Current.Exploration!.Party.Actors[2].Status);
        Assert.Same(p.ItemCustody, s.Current.Exploration.Party.ItemCustody);
        Assert.Equal(p.Actors[2].Hp, s.Current.Exploration.Party.Actors[2].Hp);
        Assert.Equal(p.Actors[2].Mp, s.Current.Exploration.Party.Actors[2].Mp);
    }

    [Fact]
    public void SelfAntidotePublishesConsumptionAndStatusTogether()
    {
        var s = Start(_ => new([3, 127, 127, 127], []), status: _ => 3);
        Action(s, CaravanItemAction.Use);
        Select(s, 13);
        Slot(s, 0, 3);
        Select(s, 13);
        Assert.Equal(1, s.Current.Exploration!.Party.Actors[13].Status);
        Assert.All(Pack(s, 13), word => Assert.Equal(127, word));
    }

    [Theory]
    [InlineData(0, "caravan-item-not-usable")]
    [InlineData(2, "caravan-item-not-usable")]
    [InlineData(125, "caravan-item-not-usable")]
    [InlineData(9, "field-item-effect-unsupported")]
    public void NonusableMapEventAndUnsupportedEffectsNeverConsume(ushort word, string code)
    {
        var s = Start(_ => new([word, 127, 127, 127], []));
        Action(s, CaravanItemAction.Use);
        Select(s, 0);
        var before = s.Current;
        Assert.Equal(code, Send(s, new SelectCaravanItemSlot(Wait(s).Token, 0, word)).Failure!.Code);
        Assert.Same(before, s.Current);
        Back(s);
        Back(s);
        Back(s);
        Back(s);
        Assert.True(s.Current.CanWaitAtInput);
        Assert.Equal(word, Pack(s, 0)[0]);
    }

    [Fact]
    public void TargetsCancelToDonorAndDropRequiresConfirmationThenPreservesDepotAndSaturatedDeals()
    {
        var s = Start();
        Action(s, CaravanItemAction.Give);
        Select(s, 0);
        Slot(s, 0, 50);
        var before = s.Current.Exploration!.Party;
        Back(s);
        Assert.Equal(CaravanPhase.ItemMember, Wait(s).Phase);
        Assert.Null(Wait(s).Member);
        Assert.Same(before, s.Current.Exploration.Party);
        Back(s);
        Action(s, CaravanItemAction.Drop);
        Select(s, 0);
        Slot(s, 2, 61);
        Assert.Same(before, s.Current.Exploration.Party);
        Accept(s, new AnswerCaravan(Wait(s).Token, false));
        Assert.Equal(CaravanPhase.ItemMember, Wait(s).Phase);
        Select(s, 0);
        Slot(s, 2, 61);
        Accept(s, new AnswerCaravan(Wait(s).Token, true));
        Assert.Equal(new ushort[] { 50, 0, 1, 127 }, Pack(s, 0));
        Assert.Equal(before.ItemCustody!.Depot, s.Current.Exploration.Party.ItemCustody!.Depot);
        Assert.Equal(before.ItemCustody.OnceFlags, s.Current.Exploration.Party.ItemCustody.OnceFlags);
        Assert.Equal(15, s.Current.Exploration.Party.ItemCustody.Deals[61]);
    }

    [Fact]
    public void SupportedEquipAndSelfGiveRefreshLiveAttackAndClearEquipped()
    {
        var s = Start(_ => new([178, 0, 61, 1], []), equipment: true);
        Action(s, CaravanItemAction.Equip);
        Select(s, 0);
        Slot(s, 2, 61);
        Assert.Equal((ushort)189, Pack(s, 0)[2]);
        Continue(s);
        Back(s);
        Action(s, CaravanItemAction.Give);
        Select(s, 0);
        Slot(s, 2, 189);
        Select(s, 0);
        Assert.Equal(new ushort[] { 50, 0, 1, 61 }, Pack(s, 0));
        Assert.Equal((byte)20, s.QueryPartyDetails()!.Members[0].Attack!.Value);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void EquippedCursedOutgoingOnEitherSideRefusesAtomically(bool recipient)
    {
        var source = new PrivateBattleDefinitions(null!, [], [],
            [new(50, "CURSED_TEST", "Cursed", "", "NONE", [], 0, 0, 1,
                "WEAPON|CURSED", "NOTHING", "NOTHING", 0, [new("NONE", 0)])], [], [], []);
        var s = Start(i => (i == 1) == recipient ? new([178, 0, 61, 1], []) : new([0, 1, 61, 50], []), source: source);
        Action(s, CaravanItemAction.Give);
        Select(s, 0);
        if (recipient)
        {
            Slot(s, 0, 0);
            Select(s, 1);
        }
        var before = s.Current;
        Assert.Equal("caravan-equipped-curse", Send(s, new SelectCaravanItemSlot(Wait(s).Token, 0, 178)).Failure!.Code);
        Assert.Same(before, s.Current);
    }

    [Fact]
    public void StaleInputAndCallbackFailureRetainCommittedItems()
    {
        var s = Start(fail: true);
        Action(s, CaravanItemAction.Give);
        Select(s, 13);
        Slot(s, 0, 50);
        var stale = new SelectCaravanMember(Wait(s).Token, 13);
        Accept(s, stale);
        var accepted = s.Current;
        Assert.NotNull(Send(s, stale).Failure);
        Assert.Same(accepted, s.Current);
        Continue(s);
        Back(s);
        Back(s);
        Assert.NotNull(Send(s, new BackCaravan(Wait(s).Token)).Failure);
        Assert.Equal(new ushort[] { 0, 61, 1, 50 }, Pack(s, 13));
        Assert.Same(accepted.Exploration!.Party.ItemCustody, s.Current.Exploration!.Party.ItemCustody);
    }

    [Theory]
    [InlineData(CaravanItemAction.Drop)]
    [InlineData(CaravanItemAction.Equip)]
    public void EquippedCurseCannotBeDiscardedOrUnequipped(CaravanItemAction action)
    {
        var source = new PrivateBattleDefinitions(null!, [], [],
            [new(50, "CURSED_TEST", "Cursed", "", "NONE", [], 0, 0, 1,
                "WEAPON|CURSED", "NOTHING", "NOTHING", 0, [new("NONE", 0)])], [], [], []);
        var s = Start(_ => new([178, 127, 127, 127], []), source: source);
        Action(s, action);
        Select(s, 0);
        var before = s.Current;
        var command = action == CaravanItemAction.Drop
            ? new SelectCaravanItemSlot(Wait(s).Token, 0, 178)
            : new SelectCaravanItemSlot(Wait(s).Token, 4, null);
        Assert.Equal("caravan-equipped-curse", Send(s, command).Failure!.Code);
        Assert.Same(before, s.Current);
    }

    [Fact]
    public void BrokenWeaponAndEquippedNonweaponRemainUnsupportedWithoutLosingItems()
    {
        foreach (ushort word in new ushort[] { 306, 131 })
        {
            var s = Start(_ => new([word, 0, 127, 127], []), equipment: true);
            Action(s, CaravanItemAction.Give);
            Select(s, 0);
            var before = s.Current;
            var result = Send(s, new SelectCaravanItemSlot(Wait(s).Token, 0, word));
            Assert.Equal(SessionStopReason.Unsupported, result.StopReason);
            Assert.Same(before, s.Current);
        }
    }

    [Fact]
    public void MissingUseTargetAndUnsellableDropDoNotPublishOrConsume()
    {
        var s = Start(_ => new([3, 125, 127, 127], []));
        Action(s, CaravanItemAction.Use);
        Select(s, 0);
        Slot(s, 0, 3);
        var before = s.Current;
        Assert.NotNull(Send(s, new SelectCaravanMember(Wait(s).Token, 99)).Failure);
        Assert.Same(before, s.Current);
        Back(s);
        Back(s);
        Action(s, CaravanItemAction.Drop);
        Select(s, 0);
        before = s.Current;
        Assert.Equal("discard-unsellable", Send(s, new SelectCaravanItemSlot(Wait(s).Token, 1, 125)).Failure!.Code);
        Assert.Same(before, s.Current);
    }
}
