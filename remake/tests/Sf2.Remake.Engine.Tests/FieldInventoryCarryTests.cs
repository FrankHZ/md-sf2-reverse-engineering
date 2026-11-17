using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class FieldInventoryCarryTests
{
    private static GameSession Start(bool caravan, Func<int, BattleSourceLoadout> pack)
    {
        var source = new PrivateBattleDefinitions(null!, [], [],
            [new(19, "POWER_RING", "Power Ring", "", "NONE", [], 0, 0, 100,
                "RING|RARE|BREAKABLE", "NOTHING", "NOTHING", 0, [new("INCREASE_ATT", 5)])], [], [], []);
        var preparation = new FieldPreparationDefinition(
            new byte[] { 0, 3, 50, 61 }.Select(id => new BattleDiscardItem(id, "item-" + id, false, false)),
            [new(0, FieldRecoveryEffect.NoEffect), new(3, FieldRecoveryEffect.CurePoison)]);
        var s = CaravanServiceTests.Session(pack: pack, equipment: true, preparation: preparation,
            privateDefinitions: source, status: _ => 2,
            custody: new([4], new Dictionary<byte, byte> { [61] = 2 }, [50, 61]));
        if (caravan)
            Accept(s, new ChooseCaravan(Wait(s).Token, CaravanAction.Item));
        else
            Accept(s, new BackCaravan(Wait(s).Token));
        return s;
    }

    private static CaravanWait Wait(GameSession s) => Assert.IsType<CaravanWait>(s.Current.Story.Wait);
    private static SessionResult Send(GameSession s, SessionCommand command) =>
        s.Submit(new(s.Current.SessionId, s.Current.Revision, null, command));
    private static void Accept(GameSession s, SessionCommand command) => Assert.Null(Send(s, command).Failure);
    private static ActorRef Actor(int index) => new("member-" + index);
    private static ushort[] Pack(GameSession s, int member) =>
        s.Current.Exploration!.Party.Actors[member].SourceLoadout!.Items.ToArray();
    private static void Select(GameSession s, CaravanItemAction action, int member, int slot, ushort word)
    {
        Accept(s, new ChooseCaravanItem(Wait(s).Token, action));
        Accept(s, new SelectCaravanMember(Wait(s).Token, member));
        Accept(s, new SelectCaravanItemSlot(Wait(s).Token, slot, word));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void KnownUnwornRingDoesNotBlockDonorOrRecipientAndStaysUnchanged(bool caravan)
    {
        var s = Start(caravan, i => i == 0 ? new([178, 0, 19, 127], []) : new([189, 19, 127, 127], []));
        var before = s.Current.Exploration!.Party;
        var query = caravan ? CaravanItemQuery.Query(s.Definition, s.Current, Actor(0)) : s.QueryFieldActions(Actor(0));
        Assert.Null(query.Error);
        Assert.Equal("Power Ring", query.Items.Single(item => item.Slot == 2).Name);
        Assert.Null(query.Items.Single(item => item.Slot == 2).Use);
        Assert.DoesNotContain(query.Equipment, item => item.Slot == 2);
        if (caravan)
        {
            Select(s, CaravanItemAction.Give, 0, 1, 0);
            Accept(s, new SelectCaravanMember(Wait(s).Token, 1));
        }
        else
            Accept(s, new FieldGive(Actor(0), 1, 0, Actor(1)));
        Assert.Equal(new ushort[] { 178, 19, 127, 127 }, Pack(s, 0));
        Assert.Equal(new ushort[] { 189, 19, 0, 127 }, Pack(s, 1));
        Assert.Same(before.ItemCustody, s.Current.Exploration!.Party.ItemCustody);
        Assert.Equal(before.MainSeed, s.Current.Exploration.Party.MainSeed);
        Assert.Equal(before.ThinkingSeed, s.Current.Exploration.Party.ThinkingSeed);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FullExchangeValidatesSelectedWordsWithoutRequiringUnselectedRingEffects(bool caravan)
    {
        var s = Start(caravan, i => i == 0 ? new([178, 19, 0, 3], []) : new([189, 19, 0, 3], []));
        if (caravan)
        {
            Select(s, CaravanItemAction.Give, 0, 2, 0);
            Accept(s, new SelectCaravanMember(Wait(s).Token, 1));
            var before = s.Current;
            Assert.Equal("field-stale-item", Send(s, new SelectCaravanItemSlot(Wait(s).Token, 0, 61)).Failure!.Code);
            Assert.Same(before, s.Current);
            Accept(s, new SelectCaravanItemSlot(Wait(s).Token, 0, 189));
        }
        else
        {
            var before = s.Current;
            Assert.Equal("field-stale-item", Send(s, new FieldGive(Actor(0), 2, 0, Actor(1), 0, 61)).Failure!.Code);
            Assert.Same(before, s.Current);
            Accept(s, new FieldGive(Actor(0), 2, 0, Actor(1), 0, 189));
        }
        Assert.Equal(new ushort[] { 178, 19, 3, 61 }, Pack(s, 0));
        Assert.Equal(new ushort[] { 19, 0, 3, 0 }, Pack(s, 1));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void DropAndAdmittedUsePreserveUnselectedRingAndOtherResources(bool caravan, bool use)
    {
        var s = Start(caravan, _ => new([178, 19, 3, 0], []));
        var before = s.Current.Exploration!.Party;
        if (caravan)
        {
            Select(s, use ? CaravanItemAction.Use : CaravanItemAction.Drop, 0, use ? 2 : 3, use ? (ushort)3 : (ushort)0);
            if (use)
                Accept(s, new SelectCaravanMember(Wait(s).Token, 1));
            else
                Accept(s, new AnswerCaravan(Wait(s).Token, true));
        }
        else
            Accept(s, use ? new FieldUseItem(Actor(0), 2, 3, Actor(1)) : new FieldDrop(Actor(0), 3, 0));
        Assert.Equal(new ushort[] { 178, 19, use ? (ushort)0 : (ushort)3, 127 }, Pack(s, 0));
        Assert.Equal(new ushort[] { 178, 19, 3, 0 }, Pack(s, 1));
        Assert.Equal(use ? 0 : 2, s.Current.Exploration!.Party.Actors[1].Status);
        Assert.Same(before.ItemCustody, s.Current.Exploration.Party.ItemCustody);
        Assert.Equal(before.Gold, s.Current.Exploration.Party.Gold);
        Assert.Equal(before.Actors[0].Hp, s.Current.Exploration.Party.Actors[0].Hp);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void KnowingRingIdentityDoesNotAdmitItsUseOrEquip(bool caravan)
    {
        foreach (bool equip in new[] { false, true })
        {
            var s = Start(caravan, _ => new([178, 19, 3, 127], []));
            if (caravan)
            {
                Accept(s, new ChooseCaravanItem(Wait(s).Token, equip ? CaravanItemAction.Equip : CaravanItemAction.Use));
                Accept(s, new SelectCaravanMember(Wait(s).Token, 0));
            }
            var before = s.Current;
            var result = Send(s, caravan ? new SelectCaravanItemSlot(Wait(s).Token, 1, 19) :
                equip ? new FieldEquip(Actor(0), 1, 19) : new FieldUseItem(Actor(0), 1, 19, Actor(0)));
            Assert.NotNull(result.Failure);
            Assert.Same(before, s.Current);
        }
    }

    [Fact]
    public void UnknownIdentityStillRejectsAndOrdinaryFieldSelfExchangeKeepsItsSlotPolicy()
    {
        var unknown = Start(false, _ => new([178, 20, 0, 127], []));
        var before = unknown.Current;
        Assert.Equal("field-item-unsupported", unknown.QueryFieldActions(Actor(0)).Error);
        Assert.NotNull(Send(unknown, new FieldDrop(Actor(0), 2, 0)).Failure);
        Assert.Same(before, unknown.Current);
        var field = Start(false, _ => new([178, 19, 0, 3], []));
        Accept(field, new FieldGive(Actor(0), 0, 178, Actor(0), 2, 0));
        Assert.Equal(new ushort[] { 19, 3, 0, 50 }, Pack(field, 0));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void UnsupportedWornRingStillBlocksMutationThatWouldRequireItsActiveEffect(bool caravan)
    {
        var s = Start(caravan, _ => new([147, 0, 127, 127], []));
        var query = caravan ? CaravanItemQuery.Query(s.Definition, s.Current, Actor(0)) : s.QueryFieldActions(Actor(0));
        Assert.Equal("equipment-nonweapon", query.Error);
        if (caravan)
        {
            Accept(s, new ChooseCaravanItem(Wait(s).Token, CaravanItemAction.Drop));
            Accept(s, new SelectCaravanMember(Wait(s).Token, 0));
        }
        var before = s.Current;
        var result = Send(s, caravan ? new SelectCaravanItemSlot(Wait(s).Token, 1, 0) : new FieldDrop(Actor(0), 1, 0));
        Assert.Equal("equipment-nonweapon", result.Failure!.Code);
        Assert.Same(before, s.Current);
    }
}
