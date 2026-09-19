using System.Text.Json;
using Sf2.Remake.Domain.Battles;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class WeightedMovementReferenceTests
{
    [Theory]
    [InlineData("uniform-cost-two")]
    [InlineData("mixed-weight-two-routes")]
    [InlineData("budget-128-bucket-wrap")]
    [InlineData("array-end-precheck")]
    public void SupportedWeightedPropagationRetainsTheAcceptedRuntimeComparison(string id)
    {
        using var fixture = JsonDocument.Parse(File.ReadAllText(Path.Combine(
            AppContext.BaseDirectory, "fixtures", "battlefield-movement.json")));
        var row = fixture.RootElement.GetProperty("cases").EnumerateArray()
            .Single(value => value.GetProperty("id").GetString() == id);
        var terrain = Enumerable.Repeat(row.GetProperty("terrainDefault").GetByte(), 2304).ToArray();
        foreach (var range in row.GetProperty("terrainRanges").EnumerateArray())
            for (int index = range.GetProperty("start").GetInt32(); index <= range.GetProperty("end").GetInt32(); index++)
                terrain[index] = range.GetProperty("value").GetByte();
        foreach (var entry in row.GetProperty("terrainEntries").EnumerateArray())
            terrain[entry.GetProperty("offset").GetInt32()] = entry.GetProperty("value").GetByte();
        var moveCosts = row.GetProperty("moveCosts").EnumerateArray()
            .Select(value => checked((sbyte)value.GetInt32())).ToArray();
        // Decode source operands at the comparison boundary; the engine consumes semantic costs.
        var costs = terrain.Select(value => (value & 0x80) != 0 ? (sbyte)-1 : moveCosts[value & 0x1F]).ToArray();
        var before = costs.ToArray();
        var grid = WeightedMovement.Build(costs, row.GetProperty("startOffset").GetInt32(), row.GetProperty("budget").GetInt32());
        var expected = row.GetProperty("expected");
        var reachable = Enumerable.Range(0, costs.Length).Select(grid.CostAtOffset).Where(cost => cost is not null).ToArray();
        Assert.Equal(expected.GetProperty("reachableCount").GetInt32(), reachable.Length);
        Assert.Equal(expected.GetProperty("maximumCost").GetInt32(), reachable.Max());
        Assert.Equal(expected.GetProperty("expansionOrder").EnumerateArray().Select(value => value.GetInt32()), grid.ExpansionOrder);
        foreach (var probe in expected.GetProperty("probes").EnumerateArray())
            Assert.Equal(probe.GetProperty("cost").GetInt32(), grid.CostAtOffset(probe.GetProperty("offset").GetInt32()) ?? -1);
        Assert.Equal(before, costs);
        // The source flat-right-edge-wrap case is an intentional logical-map difference,
        // independently checked by BattleMovementTests.FullWidthAuthoredMapDoesNotWrapFromOneRowEdgeToTheNext.
    }
}
