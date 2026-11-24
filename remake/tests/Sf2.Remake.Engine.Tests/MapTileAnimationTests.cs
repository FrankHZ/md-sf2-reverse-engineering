using Sf2.Remake.Application.Content.Scenarios;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class MapTileAnimationTests
{
    [Fact]
    public void TransferAppearsAfterSubmissionAndHoldsForEachEntryCounter()
    {
        var blocks = new Dictionary<int, MapTileReplacement>();
        var contexts = new Dictionary<BlockNeighborhood, ExplorationRaster>();
        var table = new MapTileAnimation(512, 2, [new(4, 1, blocks, contexts), new(2, 3, blocks, contexts)]);
        Assert.Equal(new[] { -1, -1, 0, 1, 1, 1, 0, 1 }, Enumerable.Range(0, 8).Select(i => table.PhaseAt(i)));
        Assert.Equal(1, table.PhaseAt(long.MaxValue));
    }
}
