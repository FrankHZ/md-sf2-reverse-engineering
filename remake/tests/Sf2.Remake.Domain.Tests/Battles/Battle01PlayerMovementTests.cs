using System.Text.Json;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Domain.Tests.Battles;

public sealed class Battle01PlayerMovementTests
{
    [Theory]
    [InlineData("uniform-cost-two")]
    [InlineData("mixed-weight-two-routes")]
    [InlineData("budget-128-bucket-wrap")]
    [InlineData("flat-right-edge-wrap")]
    [InlineData("array-end-precheck")]
    public void WeightedPropagationMatchesTheAcceptedRuntimeMatrix(string id)
    {
        using var fixture = Fixture("h3", "battlefield-movement-matrix-v1");
        var row = fixture.RootElement.GetProperty("cases").EnumerateArray().Single(row => row.GetProperty("id").GetString() == id);
        var terrain = Enumerable.Repeat(row.GetProperty("terrainDefault").GetByte(), 2304).ToArray();
        foreach (var range in row.GetProperty("terrainRanges").EnumerateArray())
            for (int index = range.GetProperty("start").GetInt32(); index <= range.GetProperty("end").GetInt32(); index++)
                terrain[index] = range.GetProperty("value").GetByte();
        foreach (var entry in row.GetProperty("terrainEntries").EnumerateArray())
            terrain[entry.GetProperty("offset").GetInt32()] = entry.GetProperty("value").GetByte();
        var before = terrain.ToArray();
        var costs = row.GetProperty("moveCosts").EnumerateArray().Select(value => unchecked((sbyte)value.GetInt32())).ToArray();
        var grid = Battle01PlayerMovement.BuildWeightedGrid(terrain, costs,
            row.GetProperty("startOffset").GetInt32(), row.GetProperty("budget").GetInt32());
        var expected = row.GetProperty("expected");
        Assert.Equal(expected.GetProperty("reachableCount").GetInt32(), grid.ReachableCount);
        Assert.Equal(expected.GetProperty("expansionOrder").EnumerateArray().Select(value => value.GetInt32()), grid.ExpansionOrder);
        Assert.Equal(expected.GetProperty("maximumCost").GetInt32(),
            Enumerable.Range(0, 2304).Select(grid.CostAtOffset).Where(cost => cost is not null).Max());
        foreach (var probe in expected.GetProperty("probes").EnumerateArray())
            Assert.Equal(probe.GetProperty("cost").GetInt32(), grid.CostAtOffset(probe.GetProperty("offset").GetInt32()) ?? -1);
        Assert.Equal(before, terrain);
        Assert.Throws<NotSupportedException>(() => ((IList<byte>)grid.TotalCosts)[0] = 3);
        // The memory-safe API compares resulting state/order, not the original pre-check unsafe reads.
    }

    [Fact]
    public void HealerTerrainWeightsChangeBudgetAdmissionAndLandEffectRemainsSeparate()
    {
        var profile = Battle01MovementProfile.Priest;
        byte[] corridor = Enumerable.Repeat((byte)255, 2304).ToArray();
        for (int x = 4; x <= 9; x++) corridor[10 * 48 + x] = 1;
        var plains = Battle01PlayerMovement.BuildWeightedGrid(corridor, profile.Costs, 10 * 48 + 4, 10);
        Assert.Equal(10, plains.CostAt(new(9, 10)));
        corridor[10 * 48 + 6] = 4; // Forest costs four, versus plains two.
        var forest = Battle01PlayerMovement.BuildWeightedGrid(corridor, profile.Costs, 10 * 48 + 4, 10);
        Assert.Null(forest.CostAt(new(9, 10))); Assert.Equal(10, forest.CostAt(new(8, 10)));
        corridor[10 * 48 + 6] = 8; // Water is obstructed for this movement type.
        var water = Battle01PlayerMovement.BuildWeightedGrid(corridor, profile.Costs, 10 * 48 + 4, 10);
        Assert.Null(water.CostAt(new(6, 10)));
        Assert.Equal(1, profile.LandEffects[1]); Assert.Equal(2, profile.LandEffects[4]);
        Assert.Equal(4, profile.ClassId); Assert.Equal(12, profile.MovementType);
    }

    internal static JsonDocument Fixture(string tier, string name) => JsonDocument.Parse(File.ReadAllText(Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "../../../../../../tests/fixtures", tier, name + ".json"))));

    [Fact]
    public void AlliesAreTraversableButOnlyVacantDestinationsCanBeConfirmedAndEnemiesBlockPropagation()
    {
        var battle = CorridorBattle(); var actor = battle.Roster[1];
        var range = Battle01PlayerMovement.CreateRange(battle, actor);
        Assert.Equal(actor.Stats.Move * 2, range.Budget); Assert.Equal(actor.Position, range.Origin);
        Assert.Equal(2, range.Grid.CostAt(new(8, 18))); Assert.False(range.CanStopAt(new(8, 18)));
        Assert.Equal(4, range.Grid.CostAt(new(7, 18))); Assert.False(range.CanStopAt(new(7, 18)));
        Assert.Equal(6, range.Grid.CostAt(new(6, 18))); Assert.True(range.CanStopAt(new(6, 18)));
        Assert.True(range.CanStopAt(actor.Position)); Assert.Null(range.Grid.CostAt(new(10, 18)));
        Assert.Null(range.Grid.CostAt(new(11, 18)));
        Assert.Equal(1, battle.TerrainAt(new(10, 18))); Assert.Equal(0x81, range.ProjectedTerrain[18 * 48 + 10]);
        Assert.Equal(128, range.OriginOccupancy[18 * 48 + 10]); Assert.Same(battle.Occupancy, range.OriginOccupancy);
        Assert.DoesNotContain(new MapPosition(8, 18), range.LegalDestinations);
        Assert.Contains(new MapPosition(6, 18), range.LegalDestinations);
        Assert.Equal(new MapPosition(4, 5), battle.Roster[3].Deployment.Position);
        Assert.Equal(new MapPosition(10, 18), battle.Roster[3].Position);
    }

    internal static Battle01InitializedState CorridorBattle()
    {
        var initial = Battle01FirstRoundTests.Initial(); var roster = initial.Roster.ToArray();
        var actor = roster[1]; roster[1] = new(actor.Deployment, actor.Stats, 4, null);
        roster[3] = roster[3].WithPosition(new(10, 18));
        var terrain = Enumerable.Repeat((byte)255, 2304).ToArray();
        for (int x = 5; x <= 11; x++) terrain[18 * 48 + x] = 1;
        var occupancy = Enumerable.Repeat(-1, 2304).ToArray();
        foreach (var unit in roster) occupancy[unit.Position.Y * 48 + unit.Position.X] = unit.Index;
        return new(roster, initial.Regions.ToArray(), terrain, occupancy, initial.RandomSeedImage);
    }

    [Fact]
    public void ControlledPreviewCounterexampleChoosesCostSixAndFormsABudgetedTerminatedReturnablePath()
    {
        var round = CounterexampleRound(); var controlled = Battle01FirstControl.Enter(round, 1).State!;
        var range = controlled.FirstControl!.Movement.Range;
        Assert.Equal(10, range.Grid.CostAt(new(7, 6))); Assert.Equal(8, range.Grid.CostAt(new(8, 6)));
        Assert.Equal(6, range.Grid.CostAt(new(6, 6)));
        var selected = Battle01PlayerMovement.SelectDestination(controlled, 1, new(7, 6));
        var selection = selected.FirstControl!.Movement; var preview = selection.Preview;
        // Original mask accumulation retains right8, then left6 (mask5), and previousMask0 picks right.
        // The explicitly named remake policy chooses left6 and makes no original-cancel-path claim.
        Assert.Equal((byte)2, preview.ReturnDirections[0]);
        Assert.Equal(new byte[] { 2, 3, 3, 0, 255 }, preview.Directions);
        Assert.Equal(new byte[] { 2, 1, 1, 0, 255 }, preview.ReturnDirections);
        Assert.Equal(10, preview.Cost); Assert.Equal(10, selection.GridCost); Assert.Equal(10, range.Budget);
        Assert.Equal(preview.Cost, preview.Positions.Skip(1).Sum(point => (int)range.Profile.Costs[round.TerrainAt(point)]));
        Assert.All(preview.Positions, point => Assert.NotNull(range.Grid.CostAt(point)));
        Assert.Equal(new MapPosition(7, 6), Replay(range.Origin, preview.Directions));
        Assert.Equal(range.Origin, Replay(selection.Cursor, preview.ReturnDirections));
        Assert.Equal(range.Origin, Replay(selection.Cursor, preview.ReturnDirections.Append((byte)0)));
        Assert.Equal(1, preview.Directions.Count(code => code == 255)); Assert.Equal(255, preview.Directions[^1]);
        Assert.Equal(new MapPosition(7, 4), selected.Roster[1].Position); // Preview has not relocated the logical actor.
        Assert.Equal(new MapPosition(9, 18), selected.Roster[1].Deployment.Position);
        Assert.Same(controlled.Occupancy, selected.Occupancy);
        var confirmed = Battle01PlayerMovement.Confirm(selected, 1);
        Assert.Equal(Battle01Phase.PlayerActionChoice, confirmed.Phase); Assert.Equal(new MapPosition(7, 6), confirmed.Roster[1].Position);
        Assert.Equal(-1, confirmed.OccupantAt(range.Origin)); Assert.Equal(1, confirmed.OccupantAt(new(7, 6)));
        var cancelled = Battle01PlayerMovement.Cancel(confirmed, 1);
        Assert.Equal(Battle01Phase.PlayerMovementSelection, cancelled.Phase); Assert.Equal(range.Origin, cancelled.Roster[1].Position);
        Assert.Equal(controlled.Occupancy, cancelled.Occupancy); Assert.Equal(range.Origin, cancelled.FirstControl!.Movement.Cursor);
        Assert.Equal(0, cancelled.FirstControl.Movement.Preview.Cost);
        Assert.Equal(new MapPosition(9, 18), cancelled.Roster[1].Deployment.Position);
        Assert.Same(round.Roster[1].Stats, cancelled.Roster[1].Stats); Assert.Same(round.FirstRound, cancelled.FirstRound);
        Assert.Same(round.RegionFlags90Through105, cancelled.RegionFlags90Through105);
        Assert.Equal(round.RandomSeedImage, cancelled.RandomSeedImage); Assert.Equal(0, cancelled.FirstRound!.CurrentTurnOffset);
        Assert.Throws<ArgumentException>(() => Battle01PlayerMovement.Cancel(cancelled, 1));
        Assert.Equal(new MapPosition(7, 6), confirmed.Roster[1].Position); // Old snapshots retain their own phase/location.
    }

    [Fact]
    public void OccupiedReachablePreviewCannotConfirmAndSelectionCancelDoesNotMoveTheActor()
    {
        var before = Battle01FirstControl.Enter(Battle01FirstRound.Enter(CorridorBattle()), 1).State!;
        var selected = Battle01PlayerMovement.SelectDestination(before, 1, new(8, 18));
        Assert.False(selected.FirstControl!.Movement.CanConfirm); Assert.Equal(2, selected.FirstControl.Movement.Preview.Cost);
        Assert.Throws<ArgumentException>(() => Battle01PlayerMovement.Confirm(selected, 1));
        Assert.Equal(before.Occupancy, selected.Occupancy); Assert.Equal(before.Roster[1].Position, selected.Roster[1].Position);
        var cancelled = Battle01PlayerMovement.Cancel(selected, 1);
        Assert.Equal(before.Roster[1].Position, cancelled.FirstControl!.Movement.Cursor);
        Assert.Equal(before.Occupancy, cancelled.Occupancy);
        Assert.Throws<ArgumentException>(() => Battle01PlayerMovement.SelectDestination(before, 1, new(10, 18)));
        Assert.Throws<ArgumentException>(() => Battle01PlayerMovement.SelectDestination(before, 1, new(48, 18)));
        Assert.Throws<ArgumentException>(() => Battle01PlayerMovement.SelectDestination(before, 2, new(6, 18)));
    }

    internal static Battle01InitializedState CounterexampleRound()
    {
        var initial = Battle01FirstRoundTests.Initial(); var roster = initial.Roster.ToArray(); var actor = roster[1];
        roster[1] = new(actor.Deployment, Battle01FirstControlTests.Stats(actor.Stats, 0, 5), 4, null, position: new(7, 4));
        for (int index = 3; index < 9; index++) roster[index] = roster[index].WithPosition(new(index, 1));
        var terrain = Enumerable.Repeat((byte)255, 2304).ToArray();
        foreach (var point in new MapPosition[] { new(7, 4), new(6, 4), new(6, 5), new(6, 6), new(8, 5), new(8, 6) })
            terrain[point.Y * 48 + point.X] = 1;
        terrain[4 * 48 + 8] = 4; terrain[6 * 48 + 7] = 4;
        var occupancy = Enumerable.Repeat(-1, 2304).ToArray();
        foreach (var unit in roster) occupancy[unit.Position.Y * 48 + unit.Position.X] = unit.Index;
        return Battle01FirstRound.Enter(new(roster, initial.Regions.ToArray(), terrain, occupancy, initial.RandomSeedImage));
    }

    private static MapPosition Replay(MapPosition origin, IEnumerable<byte> directions)
    {
        var current = origin;
        foreach (byte direction in directions)
        {
            if (direction > 3) break;
            current = direction switch { 0 => new(current.X + 1, current.Y), 1 => new(current.X, current.Y - 1),
                2 => new(current.X - 1, current.Y), _ => new(current.X, current.Y + 1) };
        }
        return current;
    }
}
