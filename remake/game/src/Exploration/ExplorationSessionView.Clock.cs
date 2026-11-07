using Godot;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.GodotAdapter.Input;

namespace Sf2.Remake.GodotAdapter.Exploration;

public sealed partial class ExplorationSessionView
{
    private long _rateGeneration;
    internal bool CanAccelerate => _session is { } session && IsVisibleInTree() && CanProcess() && GetWindow().HasFocus() &&
        !MenuEntryPending && !PartyMenuOpen && !_handedOff && !_battleHandoffPending && PresentationFailure is null &&
        session.Current.StopReason is not (SessionStopReason.Unsupported or SessionStopReason.Faulted) &&
        session.Current.Story.EntrySkip is null && !SessionPresentationOwner.IsBattle(session.Current) &&
        !session.Current.CanWaitForText && !session.Current.CanWaitForChoice &&
        session.Current.Story.Wait is not (DialogueWait or W1TextWait or FieldTextWait { LogicalDone: true } or
            FieldSearchWait or ChurchWait or ShopWait or ChoiceWait { Work: null } or ChoiceWait { Work.Automatic: false });

    internal void SuspendPacing()
    {
        _input?.AllowAcceleration(false);
        _tickTime = 0; _resumeAutomatic = true;
    }

    private double GameplayDelta(double delta)
    {
        _input.AllowAcceleration(CanAccelerate);
        if (_rateGeneration != _input.AccelerationGeneration)
        { _rateGeneration = _input.AccelerationGeneration; _tickTime = 0; }
        return delta * _input.AccelerationRate;
    }

    private Action<string?>? _menuEntry;
    private Guid _menuSession;
    private object? _menuMap;
    private int _menuSteps;
    internal bool MenuEntryPending => _menuEntry is not null;
    internal bool CanRequestFieldSave => _session?.Current.HasOrdinaryFieldTime == true && !PartyMenuOpen && !_battleMounted;

    internal bool BeginMenuEntry(Action<string?> completed)
    {
        if (PresentationFailure is { } failure) { completed(failure.Code); return true; }
        if (!CanRequestFieldSave || MenuEntryPending) return false;
        SuspendPacing();
        _movementHold.Clear(_input); CancelFieldWait();
        _menuSession = _session!.Current.SessionId;
        _menuMap = _session.Current.Exploration!.Definition;
        _menuSteps = 0; _menuEntry = completed;
        CheckMenuEntry();
        return true;
    }

    internal void CancelMenuEntry(string reason = "save-entry-cancelled") => FinishMenuEntry(reason);

    private void FinishMenuEntry(string? error)
    {
        var completed = _menuEntry;
        _menuEntry = null; _menuMap = null; _tickTime = 0;
        completed?.Invoke(error);
    }

    private void CheckMenuEntry()
    {
        if (!MenuEntryPending) return;
        var current = _session!.Current;
        if (PresentationFailure is { } failure) FinishMenuEntry(failure.Code);
        else if (current.SessionId != _menuSession || current.Exploration?.Definition != _menuMap || !current.HasOrdinaryFieldTime)
            FinishMenuEntry("save-entry-transition");
        else if (current.HasSettledFieldEntry) FinishMenuEntry(null);
        else if (_menuSteps >= 120) FinishMenuEntry("save-entry-budget");
    }

    private void AdvanceOrdinaryTime(double delta)
    {
        try { ServiceOrdinaryTime(delta); }
        finally
        {
            // A final service can open a pausing menu or enter a scene. Project its
            // actors and camera together even when service exits at that boundary.
            // A retired field must never move the newly mounted battle camera.
            if (!_handedOff && !_battleHandoffPending && PresentationFailure is null)
                _presentation?.UpdateCamera(delta, _session!.Current);
        }
    }

    private void ServiceOrdinaryTime(double delta)
    {
        CheckMenuEntry();
        if (!CanProcess()) return;
        const double step = 1.0 / 60;
        _tickTime += delta;
        int due = (int)Math.Min(4, Math.Floor(_tickTime / step));
        // Keep only fractional debt: never catch up missed whole steps after a slow callback.
        _tickTime %= step;
        for (int i = 0; i < due; i++)
        {
            var before = _session!.Current;
            if (!before.HasOrdinaryFieldTime || PresentationFailure is not null || !CanProcess() || _handedOff || _battleHandoffPending)
            { _tickTime = 0; return; }
            if (!MenuEntryPending && before.CanWaitAtInput &&
                _movementHold.Repeat(_input, before, Time.GetTicksUsec()) is { } direction)
                HandleAction(direction, repeating: true);
            // An action may publish a failure or hand off before its next service opportunity.
            // Never overwrite that result with a neutral step from the same callback.
            var afterAction = _session.Current;
            if (PresentationFailure is not null || _handedOff || _battleHandoffPending || !CanProcess() ||
                afterAction.SessionId != before.SessionId ||
                afterAction.Exploration?.Definition != before.Exploration?.Definition || !afterAction.HasOrdinaryFieldTime)
            { _tickTime = 0; return; }
            bool settling = MenuEntryPending;
            var result = Send(afterAction.CanWaitAtInput ? new WaitAtInput() :
                new AdvanceSimulation(afterAction.Story.Wait?.Token));
            if (settling) _menuSteps++;
            CheckMenuEntry();
            if (result.Failure is not null || _session.Current.SessionId != before.SessionId ||
                _session.Current.Exploration?.Definition != before.Exploration?.Definition ||
                !_session.Current.HasOrdinaryFieldTime || !CanProcess() || _handedOff || _battleHandoffPending)
            { _tickTime = 0; return; }
            // A completed step may hand straight to the next held direction. Read live input
            // only after Observe has rejected story/transfer ownership; retain no queued Move.
            if (!MenuEntryPending && !afterAction.CanWaitAtInput && _session.Current.CanWaitAtInput &&
                _movementHold.Repeat(_input, _session.Current, Time.GetTicksUsec()) is { } nextDirection)
                HandleAction(nextDirection, repeating: true);
        }
    }
}
