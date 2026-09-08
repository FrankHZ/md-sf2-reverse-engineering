using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Sessions;

public sealed class PrivateOriginalBattle01PendingAdmission
{
    internal PrivateOriginalBattle01PendingAdmission(OriginalBattle01AdmissionDefinition definition,
        PrivateOriginalMapSessionSnapshot sourceSnapshot, MapPosition trigger)
    {
        Definition = definition;
        SourceSnapshot = sourceSnapshot;
        Trigger = trigger;
    }

    public OriginalBattle01AdmissionDefinition Definition { get; }
    public PrivateOriginalMapSessionSnapshot SourceSnapshot { get; }
    public MapPosition Trigger { get; }
    public MapPosition Source => SourceSnapshot.PlayerPosition;
    public long SourceSimulationStep => SourceSnapshot.SimulationStep;
}

public sealed partial class GameSession
{
    public PrivateOriginalBattle01PendingAdmission? PrivateOriginalBattle01Admission { get; private set; }

    private bool TryBeginPrivateOriginalBattle01Admission(PrivateOriginalMapSessionSnapshot current,
        MoveExplorationCommand command, out PrivateOriginalMapMoveApplied? applied)
    {
        applied = null;
        if (current.Map.Value != OriginalMapRuntimeAdmission.Map40Id) return false;
        OriginalBattle01AdmissionDefinition definition = current.Definition.Battle01Admission!;
        MapPosition? target = current.CurrentRuntime.Traversal.ResolveCandidateTarget(
            current.WorkingLayout, current.PlayerPosition, command.Direction);
        if (target is null || !current.CurrentRuntime.Traversal.IsWithinActiveArea(target) ||
            target.Y != definition.TriggerY ||
            (definition.TriggerX != 255 && target.X != definition.TriggerX) ||
            (current.WorkingLayout.GetWord(target.Y * WorkingMapLayout.ColumnCount + target.X) & 0x3C00) != 0x1000)
            return false;
        if (PrivateOriginalMapPlayerLocomotion.IsMoving || IsPrivateOriginalMapBattleBridgeBusy ||
            current.CastleGate?.Opened != true || current.PalaceFirstVisit is null || current.AstralAcceptance is null ||
            current.MiddleTowerGuard?.ProgramStoryFlag1Set != true)
            throw new InvalidOperationException("Battle 01 admission requires the idle controlled route and guard unlock result.");
        // This is a request before relocation. World/locomotion/bridge/step are unchanged.
        var pending = new PrivateOriginalBattle01PendingAdmission(definition, current, target);
        PrivateOriginalBattle01Admission = pending;
        applied = new PrivateOriginalMapMoveApplied(current, pending);
        return true;
    }
}
