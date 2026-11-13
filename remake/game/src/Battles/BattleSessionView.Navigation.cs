using Godot;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.GodotAdapter.Input;

namespace Sf2.Remake.GodotAdapter.Battles;

public sealed partial class BattleSessionView
{
    // Menus and inspection use a repeat deadline. Player preview repeats on arrival.
    private const ulong NavigationRepeatMicros = 120000;
    private object? _navigationBoundary;
    private GameAction? _navigationDirection;
    private ulong _nextNavigation;
    private bool _navigationRejected;
    private bool NavigatingPreview => !_inspection.Active && _session?.Current.Selection?.Stage == BattleSelectionStage.Movement;

    private object? NavigationBoundary()
    {
        if (_input is null || _startupFailure is not null || _inputSuspended ||
            !IsVisibleInTree() || !CanProcess() || !GetWindow().HasFocus() ||
            _session?.Current is not { Selection: { } selection } current ||
            (!current.HasBattleControl && current.BattleMovement is not { Purpose: BattleMovementPurpose.Player }) ||
            current.StopReason is SessionStopReason.Unsupported or SessionStopReason.Faulted ||
            current.BattleScene is not null || current.BattleRound is not null || current.BattleRetreat is not null ||
            current.BattleGive is not null || current.BattleDiscard is not null || current.BattleSearch is not null ||
            current.BattleMovement is { Purpose: not BattleMovementPurpose.Player }) return null;
        if (_inspectionRelease) return null;
        if (_inspection.Active) return (current.SessionId, current.Revision, selection.Actor, "Inspection", _inspection.Page);
        var stage = selection.Stage;
        if (_menu.NavigationPage == "Commit" || stage is BattleSelectionStage.GiveCommitReady or BattleSelectionStage.DiscardCommitReady) return null;
        if (stage == BattleSelectionStage.CommitReady) stage = BattleSelectionStage.TargetChoice;
        return (current.SessionId, selection.Actor, stage, _menu.NavigationPage);
    }

    private void ClearNavigation()
    {
        _navigationBoundary = null; _navigationDirection = null; _nextNavigation = 0;
        _input?.DisarmMovement();
    }

    public override void _Notification(int what)
    {
        if (what == NotificationPaused || what == NotificationUnpaused ||
            what == NotificationVisibilityChanged && !IsVisibleInTree())
        { ClearNavigation(); SuspendPacing(); }
    }

    internal void HandleAction(GameAction action, bool repeating = false)
    {
        var boundary = NavigationBoundary();
        bool directional = action is GameAction.Up or GameAction.Right or GameAction.Down or GameAction.Left;
        if (!directional || boundary is null) ClearNavigation();
        _navigationRejected = false;
        HandleActionCore(action);
        if (!directional) return;
        if (_navigationRejected || boundary is null || !Equals(boundary, NavigationBoundary()))
        { ClearNavigation(); return; }
        if (!repeating)
        {
            _navigationBoundary = boundary; _navigationDirection = action;
            if (!NavigatingPreview) _nextNavigation = Time.GetTicksUsec() + NavigationRepeatMicros;
        }
    }

    private void ServiceNavigation(bool playerStepFinished = false)
    {
        if (_navigationDirection is not { } direction) return;
        if (!Equals(_navigationBoundary, NavigationBoundary()) || _input.MovementDirection != direction)
        { ClearNavigation(); return; }
        if (NavigatingPreview != playerStepFinished || _session!.Current.BattleMovement is not null) return;
        if (!playerStepFinished)
        {
            ulong now = Time.GetTicksUsec();
            if (now < _nextNavigation) return;
            _nextNavigation = now + NavigationRepeatMicros;
        }
        HandleAction(direction, repeating: true);
    }
}
