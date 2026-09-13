using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

// A distinct instance belongs to one preparation and enters the battle only at initialization.
public sealed class Battle01DefeatReturnAdmission
{
    public Battle01DefeatReturnAdmission(byte egressMap, bool flag64, bool flag640)
    {
        if (egressMap != 3 || flag64 || flag640)
            throw new ArgumentException("Only explicit Granseal egress3/F64false/F640false is admitted.", "return.inputs");
    }
    public byte EgressMap => 3;
    public bool Flag64 => false;
    public bool Flag640 => false;
}

public sealed class Battle01DefeatReturnRequest
{
    internal Battle01DefeatReturnRequest(Battle01DefeatRecoveryReceipt recovery, Battle01DefeatReturnAdmission admission)
    { Recovery = recovery; Admission = admission; }
    public Battle01DefeatRecoveryReceipt Recovery { get; }
    public Battle01DefeatReturnAdmission Admission { get; }
    public MapId SavepointMap { get; } = new("map3");
    public MapPosition SavepointPosition { get; } = new(32, 13);
    public byte SavepointOpaqueFacing => 1;
    public bool RaftWriteExecuted => false;
    public short HandlerResultD4 => -1;
    public MapId DestinationMap => SavepointMap;
    public MapPosition DestinationPosition => SavepointPosition;
    public byte DestinationOpaqueFacing => SavepointOpaqueFacing;
    public bool ExplorationEntered => false;
}

public static class Battle01DefeatReturn
{
    public static Battle01DefeatReturnRequest Select(Battle01InitializedState current,
        Battle01DefeatReturnAdmission? admission)
    {
        ArgumentNullException.ThrowIfNull(current);
        Battle01DefeatRecovery.RequireRecovered(current);
        if (admission is null || !current.BattleEntryFlag399 ||
            !ReferenceEquals(current.ReturnAdmission, admission) ||
            !ReferenceEquals(current.DefeatRecovery!.Before.ReturnAdmission, admission))
            throw new ArgumentException("Return inputs must retain their original initialization binding.", "return.binding");
        // The admitted Battle01 tuple selects savepoint3, skips raft writes, returns D4=-1,
        // then SwitchMap preserves map3 and its coordinates/facing. No ExplorationLoop work runs.
        return new(current.DefeatRecovery, admission);
    }
}
