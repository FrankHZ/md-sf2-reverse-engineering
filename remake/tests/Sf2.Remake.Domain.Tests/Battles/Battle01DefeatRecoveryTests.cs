using System.Text.Json;
using System.Text.Json.Nodes;
using Sf2.Remake.Domain.Battles;
using Xunit;

namespace Sf2.Remake.Domain.Tests.Battles;

public sealed class Battle01DefeatRecoveryTests
{
    private static readonly Lazy<Battle01InitializedState> Terminal = new(Battle01EnemyPhysicalAttackTests.LeaderDefeatCompleted);
    private static readonly JsonSerializerOptions Json = new() { MaxDepth = 256 };

    [Fact]
    public void RecoveryChangesOnlyLeaderCurrentHpAndGoldAndRetainsTheCompleteTerminal()
    {
        var before = Terminal.Value; string frozen = JsonSerializer.Serialize(before, Json);
        var after = Battle01DefeatRecovery.Complete(before);
        var receipt = Assert.IsType<Battle01DefeatRecoveryReceipt>(after.DefeatRecovery);
        Assert.Same(before, receipt.Before); Assert.Same(before.DefeatPending, receipt.Terminal);
        Assert.Same(before.TurnCompletion, after.TurnCompletion); Assert.Same(before.FirstRound, after.FirstRound);
        Assert.Equal(Battle01Phase.DefeatRecoveryPending, after.Phase);
        Assert.Equal(((ushort)0, (ushort)12, 120u, 60u),
            (receipt.LeaderHpBefore, receipt.LeaderHpAfter, receipt.GoldBefore, receipt.GoldAfter));
        Assert.False(receipt.EgressSelectionExecuted); Assert.Null(after.Roster[0].Position);
        Assert.Equal(5, after.Occupancy.Count(i => i >= 0)); Assert.True(after.UnlockFlag401); Assert.False(after.CompletedFlag501);
        Assert.Equal(new Battle01FactionCounts(0, 4), after.DefeatPending!.FirstOutcome);
        var expected = JsonSerializer.SerializeToNode(before, Json)!;
        expected["Roster"]![0]!["Stats"]!["HpCurrent"] = 12;
        expected["CurrentGold"] = 60; expected["Phase"] = (int)Battle01Phase.DefeatRecoveryPending;
        var actual = JsonSerializer.SerializeToNode(after, Json)!;
        actual["DefeatRecovery"] = null;
        Assert.True(JsonNode.DeepEquals(expected, actual));
        Assert.Equal(frozen, JsonSerializer.Serialize(before, Json));
        Assert.Throws<ArgumentException>(() => Battle01TurnCompletion.RequireDefeatPending(after));
        Assert.Throws<ArgumentException>(() => Battle01DefeatRecovery.Complete(after));
    }

    [Theory]
    [InlineData(0u, 0u)] [InlineData(1u, 0u)] [InlineData(3u, 1u)]
    [InlineData(120u, 60u)] [InlineData(121u, 60u)] [InlineData(9999999u, 4999999u)]
    public void GoldShiftUsesUnsignedFloorWithinExistingAdmission(uint before, uint after) =>
        Assert.Equal(after, Battle01DefeatRecovery.HalveGold(before));

    [Fact]
    public void GoldArithmeticDoesNotRelaxTheExistingStateLimit() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => Battle01DefeatRecovery.HalveGold(uint.MaxValue));

    [Theory]
    [InlineData("gold")] [InlineData("unknown-gold")] [InlineData("hp")] [InlineData("hp-max")]
    [InlineData("defeats")] [InlineData("placement")] [InlineData("main")]
    [InlineData("count")] [InlineData("history")] [InlineData("copy")]
    public void ForgedTerminalIsRejectedWithoutMutatingTheAuthenticBeforeImage(string change)
    {
        var before = Terminal.Value; string frozen = JsonSerializer.Serialize(before, Json);
        var roster = before.Roster.ToArray(); var leader = roster[0]; var s = leader.Stats;
        roster[0] = change switch
        {
            "hp" => leader.WithStats(s.WithCurrentHp(1)),
            "hp-max" => leader.WithStats(new(s.Level, 13, 0, s.MpMax, s.MpCurrent, s.Attack, s.Defense,
                s.Agility, s.Move, s.Status, s.Items, s.Spells, s.CurrentExp, s.CurrentKills, s.CurrentDefeats)),
            "defeats" => leader.WithStats(s.WithCurrentDefeats(2)),
            "placement" => leader.WithPosition(new(11, 15)),
            _ => leader,
        };
        var forged = new Battle01InitializedState(before, roster,
            change == "main" ? 0u : before.RandomSeedImage,
            change == "unknown-gold" ? null : change == "gold" ? 121u : before.CurrentGold);
        if (change is "count" or "history")
            forged = new(forged, before.DefeatPending! with {
                FirstOutcome = change == "count" ? new(1, 4) : before.DefeatPending!.FirstOutcome,
                Previous = change == "history" ? before.TurnCompletion!.Previous! : before.TurnCompletion! });
        if (change == "copy")
            forged = new(forged, roster, before.Occupancy.ToArray(), before.AiMemory.ToArray(), 0);
        Assert.Throws<ArgumentException>(() => Battle01DefeatRecovery.Complete(forged));
        Assert.Equal(frozen, JsonSerializer.Serialize(before, Json));
    }

    [Theory]
    [InlineData("gold")] [InlineData("hp")] [InlineData("occupancy")] [InlineData("history")]
    [InlineData("before")]
    public void RecoveredStateMustMatchItsAuthenticatedBeforeImage(string change)
    {
        var current = Battle01DefeatRecovery.Complete(Terminal.Value);
        var roster = current.Roster.ToArray();
        if (change == "hp") roster[0] = roster[0].WithStats(roster[0].Stats.WithCurrentHp(0));
        var forged = new Battle01InitializedState(current, roster, current.RandomSeedImage,
            change == "gold" ? 30u : current.CurrentGold);
        if (change == "occupancy")
            forged = new(forged, roster, Enumerable.Repeat(-1, 2304).ToArray(), null!);
        if (change == "history") forged = new(forged, current.FirstRound!, current.TurnCompletion!.Previous!);
        if (change == "before")
            forged = new(forged, roster, new Battle01DefeatRecoveryReceipt(current));
        Assert.Throws<ArgumentException>(() => Battle01DefeatRecovery.RequireRecovered(forged));
        Battle01DefeatRecovery.RequireRecovered(current);
    }
}
