using Sf2.Remake.Domain.Battles;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class CaravanInventoryTests
{
    [Fact]
    public void DepositNormalizesAppendsCompactsAndRefusesSixtyFifthWithoutMutation()
    {
        var custody = new BattleItemCustody([3], new Dictionary<byte, byte> { [61] = 14 }, Enumerable.Repeat((byte)1, 63));
        var pack = new BattleSourceLoadout([384, 1, 1, 127], [7]);
        var changed = CaravanInventory.Deposit(pack, custody, 0, 384, false);
        Assert.Equal(64, changed.Custody.Depot.Count);
        Assert.Equal(0, changed.Custody.Depot[^1]);
        Assert.Equal(new ushort[] { 1, 1, 127, 127 }, changed.Pack.Items);
        Assert.Equal(pack.Spells, changed.Pack.Spells);
        Assert.Equal(custody.OnceFlags, changed.Custody.OnceFlags);
        Assert.Equal(custody.Deals, changed.Custody.Deals);
        Assert.Equal("depot-full", Assert.Throws<BattleRuleException>(() =>
            CaravanInventory.Deposit(changed.Pack, changed.Custody, 0, 1, false)).Code);
        Assert.Equal(63, custody.Depot.Count);
        Assert.Equal(new ushort[] { 384, 1, 1, 127 }, pack.Items);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public void DuplicateIndexDeterminesCompactionAndFullDepotCanExchange(int index)
    {
        var contents = new byte[] { 0, 1, 0 }.Concat(Enumerable.Repeat((byte)3, 61)).ToArray();
        var custody = new BattleItemCustody([], new Dictionary<byte, byte>(), contents);
        var pack = new BattleSourceLoadout([4, 5, 262, 7], []);
        var changed = CaravanInventory.Derive(pack, custody, index, 0, 2, 262);
        Assert.Equal(contents.Where((_, i) => i != index).Append((byte)6), changed.Custody.Depot);
        Assert.Equal(new ushort[] { 4, 5, 7, 0 }, changed.Pack.Items);
        Assert.Equal(64, changed.Custody.Depot.Count);
        Assert.Equal(contents, custody.Depot);
    }

    [Fact]
    public void CurseRefusalsStaleChoicesAndExchangeCancellationPreserveStores()
    {
        var custody = new BattleItemCustody([], new Dictionary<byte, byte>(), [1, 2]);
        var pack = new BattleSourceLoadout([128, 3, 4, 5], []);
        Assert.Equal("depot-equipped-curse", Assert.Throws<BattleRuleException>(() => CaravanInventory.Deposit(pack, custody, 0, 128, true)).Code);
        Assert.Equal("depot-equipped-curse", Assert.Throws<BattleRuleException>(() => CaravanInventory.Derive(pack, custody, 0, 1, 0, 128, true)).Code);
        Assert.Throws<BattleRuleException>(() => CaravanInventory.Derive(pack, custody, 0, 1));
        Assert.Throws<BattleRuleException>(() => CaravanInventory.Derive(pack, custody, 1, 1, 1, 3));
        Assert.Throws<BattleRuleException>(() => CaravanInventory.Deposit(pack, custody, 1, 4, false));
        var unequipped = CaravanInventory.Deposit(pack, custody, 1, 3, true);
        Assert.Equal(new byte[] { 1, 2, 3 }, unequipped.Custody.Depot);
        Assert.Equal(new byte[] { 1, 2 }, custody.Depot);
        Assert.Equal(new ushort[] { 128, 3, 4, 5 }, pack.Items);
    }

    [Theory]
    [InlineData(14)]
    [InlineData(15)]
    public void RareDropSaturatesDealsWhileMarkAndAddDealKeepOrderedDepot(byte count)
    {
        var custody = new BattleItemCustody([2], new Dictionary<byte, byte> { [61] = count }, [61, 1, 61]);
        var marked = custody.Mark(4).AddDeal(61);
        Assert.Equal(custody.Depot, marked.Depot);
        var dropped = CaravanInventory.Drop(marked, 2, 61, new(61, "rare", true, false));
        Assert.Equal(new byte[] { 61, 1 }, dropped.Depot);
        Assert.Equal(15, dropped.Deals[61]);
        Assert.Equal(new byte[] { 2, 4 }, dropped.OnceFlags);
        Assert.Throws<BattleRuleException>(() => CaravanInventory.Drop(custody, 0, 61, new(61, "key", true, true)));
    }

    [Fact]
    public void CustodyCopiesInputAndRejectsInvalidCapacityOrSentinel()
    {
        byte[] input = [1, 0, 1];
        var custody = new BattleItemCustody([], new Dictionary<byte, byte>(), input);
        input[0] = 2;
        Assert.Equal(new byte[] { 1, 0, 1 }, custody.Depot);
        Assert.Throws<ArgumentOutOfRangeException>(() => new BattleItemCustody([], new Dictionary<byte, byte>(), [127]));
        Assert.Throws<ArgumentOutOfRangeException>(() => new BattleItemCustody([], new Dictionary<byte, byte>(), Enumerable.Repeat((byte)0, 65)));
    }
}
