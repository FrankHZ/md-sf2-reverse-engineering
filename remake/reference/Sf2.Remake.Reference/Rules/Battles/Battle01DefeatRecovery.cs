namespace Sf2.Remake.Domain.Battles;

// Separate from the historical lethal action: recovery never completes another turn.
public sealed class Battle01DefeatRecoveryReceipt
{
    internal Battle01DefeatRecoveryReceipt(Battle01InitializedState before)
    {
        Before = before;
        LeaderHpAfter = before.Roster.Single(unit => unit.Index == 0).Stats.HpMax;
        GoldAfter = Battle01DefeatRecovery.HalveGold(before.CurrentGold!.Value);
    }
    public Battle01InitializedState Before { get; }
    public Battle01DefeatPendingReceipt Terminal => Before.DefeatPending!;
    public ushort LeaderHpBefore => 0;
    public ushort LeaderHpAfter { get; }
    public uint GoldBefore => Before.CurrentGold!.Value;
    public uint GoldAfter { get; }
    public bool EgressSelectionExecuted => false;
}

public static class Battle01DefeatRecovery
{
    public static Battle01InitializedState Complete(Battle01InitializedState current)
    {
        RequireBefore(current);
        var receipt = new Battle01DefeatRecoveryReceipt(current);
        var roster = current.Roster.Select(unit => unit.Index == 0
            ? unit.WithStats(unit.Stats.WithCurrentHp(receipt.LeaderHpAfter)) : unit).ToArray();
        var next = new Battle01InitializedState(current, roster, receipt);
        RequireRecovered(next);
        return next;
    }

    internal static uint HalveGold(uint gold)
    {
        if (gold > 9999999) throw new ArgumentOutOfRangeException(nameof(gold));
        return gold >> 1;
    }

    private static void RequireBefore(Battle01InitializedState before)
    {
        ArgumentNullException.ThrowIfNull(before);
        if (before.Phase != Battle01Phase.DefeatPending || before.DefeatRecovery is not null)
            throw new ArgumentException("Recovery requires an unrecovered leader-defeat terminal.", "recovery.phase");
        if (before.CurrentGold != 120)
            throw new ArgumentException("Retain the controlled terminal's current gold.", "recovery.gold");
        _ = Battle01TurnCompletion.RequireDefeatPending(before);
    }

    internal static void RequireRecovered(Battle01InitializedState current)
    {
        if (current.DefeatRecovery is not { } receipt || current.Phase != Battle01Phase.DefeatRecoveryPending)
            throw new ArgumentException("A linked recovery receipt is required.", "recovery.receipt");
        var before = receipt.Before;
        RequireBefore(before);
        if (!ReferenceEquals(current.DefeatPending, before.DefeatPending) ||
            !ReferenceEquals(current.TurnCompletion, before.TurnCompletion) ||
            !ReferenceEquals(current.FirstRound, before.FirstRound) || current.FirstControl is not null ||
            !ReferenceEquals(current.Regions, before.Regions) || !ReferenceEquals(current.Terrain, before.Terrain) ||
            !ReferenceEquals(current.Occupancy, before.Occupancy) ||
            !ReferenceEquals(current.AiMemory, before.AiMemory) || !ReferenceEquals(current.AiLastTargets, before.AiLastTargets) ||
            !ReferenceEquals(current.RegionFlags90Through105, before.RegionFlags90Through105) ||
            current.Map != before.Map || current.RandomSeedImage != before.RandomSeedImage ||
            current.RandomSeedCopy != before.RandomSeedCopy || current.NewlyTestedRegionMask != before.NewlyTestedRegionMask ||
            current.CurrentGold != HalveGold(before.CurrentGold!.Value) || current.CurrentGold != receipt.GoldAfter ||
            current.Roster.Count != before.Roster.Count)
            throw new ArgumentException("Recovery must preserve the complete terminal context.", "recovery.state");
        for (int index = 0; index < before.Roster.Count; index++)
        {
            var old = before.Roster[index]; var next = current.Roster[index];
            var stats = old.Index == 0 ? old.Stats.WithCurrentHp(old.Stats.HpMax) : old.Stats;
            if (next.Deployment != old.Deployment || next.ClassId != old.ClassId ||
                next.EnemySource != old.EnemySource || next.AiBitfield != old.AiBitfield || next.Position != old.Position ||
                !Battle01EnemyPhysicalAttack.SameStats(next.Stats, stats) ||
                (old.Index == 0 && (receipt.LeaderHpAfter != old.Stats.HpMax || old.Stats.HpMax != 12)))
                throw new ArgumentException("Only leader current HP may change in the roster.", "recovery.roster");
        }
    }
}
