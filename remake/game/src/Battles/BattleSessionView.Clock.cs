using Godot;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.GodotAdapter.Battles;

public sealed partial class BattleSessionView
{
    private const double PacingStep = 1.0 / 60;
    private double _pacingDebt;
    private long _pacingGeneration, _pacingSteps;
    private readonly record struct PacingOwner(Guid Session, WaitToken? Movement, WaitToken? Scene,
        WaitToken? Retreat, WaitToken? Round, WaitToken? Story, ActorRef? Actor, BattleSelectionStage? Stage, bool Reading);
    private PacingOwner? _pacingBoundary;
    private bool _resumePacing = true;
    private bool _servicingPacing;
    private int _maxPacingSteps;
    private bool OwnsPacing => _session is { } session && SessionPresentationOwner.IsBattle(session.Current) && IsVisibleInTree();
    private bool PacingReady => OwnsPacing && _startupFailure is null && !_inputSuspended && CanProcess() && GetWindow().HasFocus() &&
        _session!.Current.StopReason is not (SessionStopReason.Unsupported or SessionStopReason.Faulted);
    private bool ReadingBoundary => _session!.Current is { } current &&
        (current.BattleSearch is not null || current.BattleGive is not null || current.BattleDiscard is not null ||
         current.BattleRetreat is { Phase: BattleRetreatPhase.Text } ||
         current.BattleRound is { Program: true } && current.Story.Wait is DialogueWait ||
         current.BattleScene is not null && _scene.WaitingForInput);
    internal bool CanAccelerate => PacingReady && !_inspection.Active && !_inspectionRelease &&
        _inspectionAfterReturn is null && !ReadingBoundary;

    internal void SuspendPacing()
    {
        _input?.AllowAcceleration(false);
        _pacingDebt = 0; _pacingBoundary = null; _resumePacing = true;
    }

    private PacingOwner? PacingBoundary()
    {
        var current = _session?.Current;
        return current is null ? null : new PacingOwner(current.SessionId, current.BattleMovement?.Token,
            current.BattleScene?.Token, current.BattleRetreat?.Token, current.BattleRound?.Token,
            current.Story.Wait?.Token, current.Selection?.Actor, current.Selection?.Stage, ReadingBoundary);
    }

    private void SyncPacing()
    {
        var boundary = PacingBoundary();
        if (_pacingBoundary != boundary)
        {
            _pacingDebt = 0; _pacingBoundary = boundary;
            if (!_servicingPacing) _resumePacing = true;
        }
        if (OwnsPacing) _input.AllowAcceleration(CanAccelerate);
        if (_pacingGeneration != _input.AccelerationGeneration)
        { _pacingGeneration = _input.AccelerationGeneration; _pacingDebt = 0; }
    }

    private object ObservePacing() => new
    { input = _input.ObserveAccelerationState(), debt = _pacingDebt, steps = _pacingSteps, maxSteps = _maxPacingSteps, reading = _session is not null && ReadingBoundary };

    private void ServicePacing(double delta)
    {
        // Prepared/retired battle views coexist with the field owner. They must
        // neither advance its shared camera nor disarm its current input hold.
        if (!OwnsPacing) { _pacingDebt = 0; return; }
        if (!PacingReady) { SuspendPacing(); return; }
        _servicingPacing = true;
        try { AdvancePacing(delta); }
        finally { _servicingPacing = false; }
    }

    private void AdvancePacing(double delta)
    {
        SyncPacing();
        if (_resumePacing) { delta = 0; _resumePacing = false; }
        if (_inspectionRelease && !_input.GameplayHeld) _inspectionRelease = false;
        if (_inspection.Active)
        {
            if (!_inspection.Refresh(_session)) { InspectionBarrier(); Present(); }
            else { ServiceNavigation(); _inspection.AdvancePointer(delta); }
            _map.AdvanceCamera(delta);
            return;
        }
        if (_input.Settings.DebugAutoVictory && _session!.Current.CanDebugWinBattle)
        {
            Send(new DebugWinBattle());
            return;
        }
        var navigationOwner = PacingBoundary();
        ServiceNavigation();
        if (navigationOwner != PacingBoundary()) delta = 0;
        if (!PacingReady) return;
        var current = _session!.Current;
        SessionCommand? reading = current.BattleSearch is not null ? _search.Consume(delta) :
            current.BattleDiscard is not null ? _discard.Consume(delta) :
            current.BattleGive is not null ? _give.Consume(delta) :
            current.BattleRetreat is { Phase: BattleRetreatPhase.Text } ? _retreat.Consume(delta) :
            current.BattleScene is not null ? _scene.Read(delta) : null;
        if (reading is not null) { Send(reading); return; }
        if (ReadingBoundary) { _pacingDebt = 0; _map.AdvanceCamera(delta); return; }
        SyncPacing();
        _pacingDebt += delta * _input.AccelerationRate;
        int due = (int)Math.Min(4, Math.Floor(_pacingDebt / PacingStep));
        _pacingDebt %= PacingStep;
        int serviced = 0;
        for (int i = 0; i < due; i++)
        {
            var boundary = PacingBoundary();
            if (!PacingReady || ReadingBoundary) { _pacingDebt = 0; break; }
            _pacingSteps++; serviced++;
            if (ConsumePacing() is { } command)
            {
                bool playerStep = _session.Current.BattleMovement?.Purpose == BattleMovementPurpose.Player;
                Send(command);
                if (playerStep) ServiceNavigation(playerStepFinished: true);
            }
            if (!PacingReady) { _pacingDebt = 0; break; }
            _map.AdvanceCamera(PacingStep);
            if (boundary != PacingBoundary()) { _pacingDebt = 0; break; }
        }
        _maxPacingSteps = Math.Max(_maxPacingSteps, serviced);
        if (due == 0) _map.AdvanceCamera(0);
    }

    private SessionCommand? ConsumePacing()
    {
        var current = _session!.Current;
        if (current.BattleRound is { Program: true }) return _story.Consume();
        if (current.BattleRound is not null) return _appearance.Consume(PacingStep);
        if (current.BattleRetreat is not null) return _retreat.Consume(PacingStep);
        if (current.BattleMovement is not null) return _map.ConsumeMovement(PacingStep, _input.Settings.ReducedFlash);
        if (current.BattleScene is not null)
        {
            try { return _scene.Consume(PacingStep); }
            catch (InvalidOperationException error)
            { FailStartup(new(SessionFailureKind.AdapterError, error.Message, "battleScene", "Battle scene playback is unavailable.")); return null; }
        }
        return current is { HasBattleControl: true, StopReason: SessionStopReason.SimulationWait } ? new AdvanceSimulation() : null;
    }
}
