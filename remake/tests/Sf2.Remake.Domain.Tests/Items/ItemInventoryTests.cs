using Sf2.Remake.Domain.Items;
using Xunit;

namespace Sf2.Remake.Domain.Tests.Items;

public sealed class ItemInventoryTests
{
    [Fact]
    public void AcquisitionReturnsNewInventoryAndLeavesInputUnchanged()
    {
        ItemInventoryState input = new([]);
        ItemId item = new("synthetic-placeholder-item");

        ItemAcquisitionResult result = ItemInventoryReducer.TryAcquireUnique(input, item);

        Assert.Equal(ItemAcquisitionOutcome.Acquired, result.Outcome);
        Assert.Equal(item, result.Item);
        Assert.Empty(input.Items);
        Assert.Equal([item], result.State.Items);
        Assert.True(result.State.Contains(item));
    }

    [Fact]
    public void DuplicateAcquisitionIsAZeroMutationResult()
    {
        ItemId item = new("synthetic-placeholder-item");
        ItemInventoryState input = new([item]);

        ItemAcquisitionResult result = ItemInventoryReducer.TryAcquireUnique(input, item);

        Assert.Equal(ItemAcquisitionOutcome.AlreadyOwned, result.Outcome);
        Assert.Same(input, result.State);
        Assert.Equal([item], input.Items);
    }

    [Fact]
    public void InventoryDefensivelyCopiesAndPreservesAcquisitionOrder()
    {
        ItemId first = new("first-synthetic-item");
        ItemId second = new("second-synthetic-item");
        List<ItemId> callerItems = [first];
        ItemInventoryState input = new(callerItems);

        callerItems.Clear();
        ItemInventoryState result = ItemInventoryReducer.TryAcquireUnique(input, second).State;

        Assert.Equal([first], input.Items);
        Assert.Equal([first, second], result.Items);
    }

    [Fact]
    public void InventoryRejectsDuplicateAndNullEntries()
    {
        ItemId item = new("synthetic-placeholder-item");

        Assert.Throws<ArgumentException>(() => new ItemInventoryState([item, item]));
        Assert.Throws<ArgumentException>(() => new ItemInventoryState([item, null!]));
    }

    [Fact]
    public void ItemIdentityRejectsBlankValues()
    {
        Assert.Throws<ArgumentException>(() => new ItemId(" "));
    }
}
