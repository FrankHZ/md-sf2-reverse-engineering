using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.GodotAdapter.Input;

namespace Sf2.Remake.GodotAdapter.Battles;

public sealed partial class BattleSessionView
{
    internal bool Inspecting => _inspection?.Active == true;
    internal Action? RequestSpeed { get; set; }
    internal Action? RequestQuit { get; set; }
    internal bool CanOpenBattleQuit => _inspection?.Active == true && _inspection.Page == "Options" &&
        _session?.QueryBattleInspection().Available == true;
    internal void ReturnFromBattleSave()
    {
        InspectionBarrier();
        if (!_inspection.Refresh(_session, afterSave: true)) Present();
    }
    internal bool CanOpenBattleSpeed => _inspection?.Active == true && _inspection.Page == "Options" &&
        _session?.QueryBattleInspection().Available == true;
    internal void ReturnFromBattleSpeed() { InspectionBarrier(); _inspection.Refresh(_session); }
    private BattleInspectionView _inspection = null!;
    private (Guid Session, ActorRef Actor)? _inspectionAfterReturn;
    private bool _inspectionRelease;

    private void InspectionBarrier()
    { SuspendPacing(); ClearNavigation(); _inspectionRelease = _input.GameplayHeld; }

    private void RetireInspection()
    { _inspectionAfterReturn = null; _inspection?.Close(); }

    private bool EnterInspection()
    {
        if (!_inspection.Open(_session!, _input, _map)) return false;
        InspectionBarrier(); _menu.Hide(); _hud.Hide(); _feedback.Hide(); return true;
    }

    private bool MovementInspectionEntry(GameAction action)
    {
        if (action != GameAction.Cancel || _session?.Current.Selection is not { Stage: BattleSelectionStage.Movement } selected)
            return false;
        // No-move entry is a read. A provisional move uses the existing real Cancel/delivery.
        if (EnterInspection()) return true;
        var current = _session.Current;
        _inspectionAfterReturn = (current.SessionId, selected.Actor);
        Send(new Cancel());
        return true;
    }

    private void CompleteInspectionEntry()
    {
        if (_inspectionAfterReturn is not { } pending) return;
        var current = _session!.Current;
        if (current.SessionId != pending.Session || current.Selection?.Actor != pending.Actor ||
            current.Selection.Stage != BattleSelectionStage.Movement || _result?.Failure is not null ||
            current.BattleMovement is { Purpose: not BattleMovementPurpose.Return })
        { _inspectionAfterReturn = null; return; }
        if (current.BattleMovement is not null) return;
        _inspectionAfterReturn = null; EnterInspection();
    }

    private bool InspectionInput(GameAction action)
    {
        if (_inspectionRelease) return true;
        if (!_inspection.Active) return false;
        string page = _inspection.Page;
        _inspection.Handle(action);
        if (!_inspection.Active) { InspectionBarrier(); Present(); }
        else if (page != _inspection.Page) InspectionBarrier();
        return true;
    }
}
