using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Sessions;

public sealed record PrivateOriginalMapCrossMapTransitionReceipt
{
    internal PrivateOriginalMapCrossMapTransitionReceipt(
        OriginalMapCrossMapTransitionDefinition definition,
        long simulationStep)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentOutOfRangeException.ThrowIfLessThan(simulationStep, 1);
        RecordIdentity = definition.Identity;
        Source = definition.AdmittedApproach;
        Trigger = definition.AdmittedTrigger;
        DestinationMap = definition.DestinationMap;
        Destination = definition.Destination;
        DestinationOpaqueFacing = definition.DestinationOpaqueFacing;
        SimulationStep = simulationStep;
    }

    public string Capability =>
        (RecordIdentity.Profile, RecordIdentity.SourceMap.Value,
            RecordIdentity.SourceResourceId, RecordIdentity.OneBasedRecordOrdinal) switch
        {
            (ContentProfile.PrivateLocal, OriginalMapRuntimeAdmission.MapId,
                OriginalMapRuntimeAdmission.SameMapWarpResourceId, OriginalMapRuntimeAdmission.NorthMap19WarpRecordOrdinal) =>
                OriginalMapRuntimeAdmission.NorthMap19TransitionCapability,
            (ContentProfile.PrivateLocal, OriginalMapRuntimeAdmission.Map19Id,
                OriginalMapRuntimeAdmission.RoyalMap20WarpResourceId, OriginalMapRuntimeAdmission.RoyalMap20WarpRecordOrdinal) =>
                OriginalMapRuntimeAdmission.RoyalMap20TransitionCapability,
            (ContentProfile.PrivateLocal, OriginalMapRuntimeAdmission.Map20Id,
                OriginalMapRuntimeAdmission.RoyalReturnWarpResourceId, OriginalMapRuntimeAdmission.RoyalReturnWarpRecordOrdinal) =>
                OriginalMapRuntimeAdmission.RoyalReturnMap19TransitionCapability,
            (ContentProfile.PrivateLocal, OriginalMapRuntimeAdmission.Map19Id,
                OriginalMapRuntimeAdmission.RoyalMap20WarpResourceId, OriginalMapRuntimeAdmission.WestTowerWarpRecordOrdinal) =>
                OriginalMapRuntimeAdmission.WestTowerMap20TransitionCapability,
            (ContentProfile.PrivateLocal, OriginalMapRuntimeAdmission.Map20Id,
                OriginalMapRuntimeAdmission.RoyalReturnWarpResourceId, OriginalMapRuntimeAdmission.MiddleTowerWarpRecordOrdinal) =>
                OriginalMapRuntimeAdmission.MiddleTowerMap21TransitionCapability,
            (ContentProfile.PrivateLocal, OriginalMapRuntimeAdmission.Map21Id,
                OriginalMapRuntimeAdmission.NorthMap40WarpResourceId, OriginalMapRuntimeAdmission.NorthMap40WarpRecordOrdinal) =>
                OriginalMapRuntimeAdmission.NorthMap40TransitionCapability,
            _ => throw new InvalidOperationException("The cross-map receipt has no admitted source identity."),
        };

    public OriginalMapCrossMapTransitionIdentity RecordIdentity { get; }

    public MapPosition Source { get; }

    public MapPosition Trigger { get; }

    public MapId DestinationMap { get; }

    public MapPosition Destination { get; }

    public byte DestinationOpaqueFacing { get; }

    public long SimulationStep { get; }
}
