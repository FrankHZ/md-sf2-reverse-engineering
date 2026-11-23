using Godot;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.GodotAdapter.Input;
using Page = Sf2.Remake.GodotAdapter.Battles.BattleDecisionPage;

namespace Sf2.Remake.GodotAdapter.Battles;

public sealed partial class BattleSessionView
{
    private const int MainRowCount = 4;
    private BattleDecision? _decision;
    private BattleCommandRow _command;
    private bool _listOpen;
    private int _row;
    private ActorRef? _attemptedTarget;

    private bool CanRouteBattleInput => _startupFailure is null && !_inputSuspended && !_hostCovered &&
        IsVisibleInTree() && CanProcess() && GetWindow().HasFocus() &&
        _result is { StopReason: not (SessionStopReason.Unsupported or SessionStopReason.Faulted) };

    internal bool NeedsFreshDirection => CanRouteBattleInput && !_inspection.Active && !_inspectionRelease &&
        _decision?.Targeting == true;

    private void RetireDecision()
    {
        _decision = null;
        _command = BattleCommandRow.Attack;
        _row = 0;
        _listOpen = false;
        _attemptedTarget = null;
    }

    private BattleDecision PresentDecision(SessionSnapshot current, BattleChoicesSnapshot choices, BattleActingStatus? acting)
    {
        var previous = _decision;
        var selection = current.Selection;
        bool sameTurn = previous?.Snapshot.SessionId == current.SessionId &&
            previous.Snapshot.Selection?.Actor == selection?.Actor;
        if (!sameTurn || selection is null || selection.Stage == BattleSelectionStage.Movement) RetireDecision();
        if (selection?.Stage == BattleSelectionStage.ActionChoice &&
            (!sameTurn || previous?.Snapshot.Selection?.Stage == BattleSelectionStage.Movement))
            _command = choices.Failure is null && choices.Actions.Any(offer =>
                offer.Action.Kind == BattleActionKind.Physical && offer.Enabled) ? BattleCommandRow.Attack : BattleCommandRow.Stay;
        if (selection?.Stage is not (BattleSelectionStage.TargetChoice or BattleSelectionStage.CommitReady))
            _attemptedTarget = null;

        bool show = acting is not null && _result!.StopReason is not (SessionStopReason.Unsupported or SessionStopReason.Faulted);
        var decision = new BattleDecision(current, choices, show, _command, _listOpen, _row, _attemptedTarget);
        _decision = decision;
        _row = decision.Row;
        if (previous?.Targeting != decision.Targeting)
        {
            ClearNavigation();
            _boardPointer.Suspend();
            _menu.SuspendPointer(keyboard: true);
        }
        _menu.Present(decision, _result!.Failure, _input, acting);
        return decision;
    }

    private bool HandleDecision(GameAction action)
    {
        if (_decision is not { Visible: true, Moving: false } decision) return false;
        if (action == GameAction.Cancel)
        {
            if (decision.Page is Page.Target or Page.Commit) Send(new ReturnToBattleActions());
            else if (decision.IsList) { _listOpen = false; _row = 0; Present(); }
            else Send(new Cancel());
            return true;
        }
        if ((decision.Page == Page.Main || decision.IsList) && action is GameAction.Left or GameAction.Right)
        {
            _menu.ReadDescription(action);
            return true;
        }
        if (decision.Page == Page.Main)
        {
            if (action is GameAction.Equipment or GameAction.Give or GameAction.Discard) return false;
            var command = action switch
            {
                GameAction.Attack => BattleCommandRow.Attack,
                GameAction.Spell => BattleCommandRow.Magic,
                GameAction.Item => BattleCommandRow.Item,
                GameAction.Stay => BattleCommandRow.Stay,
                GameAction.Up => (BattleCommandRow)(((int)decision.Command + MainRowCount - 1) % MainRowCount),
                GameAction.Down => (BattleCommandRow)(((int)decision.Command + 1) % MainRowCount),
                _ => decision.Command,
            };
            FocusCommand(decision, command,
                action is GameAction.Confirm or GameAction.Attack or GameAction.Spell or GameAction.Item or GameAction.Stay);
        }
        else if (decision.IsList)
        {
            int count = decision.Offers(decision.Command).Length;
            if (count > 0)
            {
                int step = action == GameAction.Up ? -1 : action == GameAction.Down ? 1 : 0;
                FocusRow(decision, (decision.Row + step + count) % count, action == GameAction.Confirm);
            }
        }
        else if (decision.Targeting &&
            action is GameAction.Target or GameAction.Up or GameAction.Right or GameAction.Down or GameAction.Left)
        {
            if (decision.StepTarget(action is GameAction.Up or GameAction.Left ? -1 : 1) is { } target)
            {
                SelectTarget(target);
                FinishSubmission(decision.Snapshot);
            }
        }
        else if (action == GameAction.Confirm && decision.Confirmation is { } confirm) Send(confirm);
        return true;
    }

    private void FocusCommand(BattleDecision decision, BattleCommandRow command, bool activate)
    {
        bool changed = _command != command;
        _command = command;
        if (activate)
        {
            _row = 0;
            if (BattleDecision.OpensList(command)) { _listOpen = true; Present(); }
            else ActivateDecision(decision.MainActivation(command));
        }
        else if (changed) Present();
    }

    private void FocusRow(BattleDecision decision, int row, bool activate)
    {
        bool changed = _row != row;
        _row = row;
        if (activate) ActivateDecision(decision.RowActivation(row));
        else if (changed) Present();
    }

    private void ActivateDecision(SessionCommand? activation)
    {
        if (activation is not SelectBattleAction choose)
        {
            if (activation is not null) Send(activation);
            else Present(); // Keep the refused row and its reason visible.
            return;
        }

        var before = _session!.Current;
        var result = Submit(choose);
        _attemptedTarget = null;
        if (CanRouteBattleInput && result is { Failure: null, StopReason: SessionStopReason.PlayerInput,
                Snapshot.Selection: { Stage: BattleSelectionStage.TargetChoice } selection } &&
            BattleDecision.ActionOffer(_session.QueryBattleChoices(), selection.ActionReference) is { } offer)
        {
            // The action's live offers determine its default, with self preferred.
            var target = offer.Targets.FirstOrDefault(item => item.Enabled && item.Actor == selection.Actor)
                ?? offer.Targets.FirstOrDefault(item => item.Enabled);
            if (target is not null) SelectTarget(new SelectTarget(target.Actor));
        }
        FinishSubmission(before);
    }

    private void SelectTarget(SelectTarget target)
    {
        _attemptedTarget = target.Target;
        if (Submit(target).Failure is not null) _attemptedTarget = null;
    }

    internal void HandlePointer(InputEvent input)
    {
        ClearNavigation();
        if (input is not InputEventMouse mouse || !CanRouteBattleInput || _inspectionRelease) return;
        if (_inspection.Active)
        {
            string page = _inspection.Page;
            _inspection.HandlePointer(input);
            if (!_inspection.Active) { InspectionBarrier(); Present(); }
            else if (page != _inspection.Page) InspectionBarrier();
            return;
        }
        if (_decision is not { Visible: true, Moving: false } decision) return;
        bool boardGesture = mouse is InputEventMouseMotion or InputEventMouseButton { ButtonIndex: MouseButton.Left };
        if (decision.Targeting && boardGesture && !_menu.Covers(mouse.Position) && _map.TryCell(mouse.Position, out var cell))
        {
            _menu.SuspendPointer(keyboard: true);
            var actor = decision.Snapshot.Battle.Actors.FirstOrDefault(unit => unit.Hp > 0 && unit.Initialized &&
                (unit.Actor == decision.Snapshot.Selection?.Actor ? decision.Snapshot.Selection.Preview.Destination : unit.Position) == cell);
            if (mouse is InputEventMouseMotion motion)
            {
                if (_boardPointer.Moved(motion)) PointAtTarget(decision, actor?.Actor);
                return;
            }
            var button = (InputEventMouseButton)mouse;
            // Select on press (or hover), and commit through the same live confirmation as a key.
            if (button.Pressed) PointAtTarget(decision, actor?.Actor);
            if (_decision is { Targeting: true } live &&
                _boardPointer.Read(button, live.Candidate == actor?.Actor ? live.Candidate?.Value : null,
                    _map.CellRect(cell), live.PointerBoundary) is not null &&
                live.Confirmation is { } confirmation) Send(confirmation);
            return;
        }
        _boardPointer.Cancel();
        if (_menu.ReadPointer(mouse, decision) is not { } hit) return;
        if (hit.Id.StartsWith("command:", StringComparison.Ordinal))
            FocusCommand(decision, (BattleCommandRow)int.Parse(hit.Id.AsSpan(8)), hit.Activate);
        else if (hit.Id.StartsWith("row:", StringComparison.Ordinal))
            FocusRow(decision, int.Parse(hit.Id.AsSpan(4)), hit.Activate);
        else if (hit.Activate)
            HandleDecision(hit.Id switch
            {
                "PreviousTarget" => GameAction.Left,
                "NextTarget" => GameAction.Right,
                "Confirm" => GameAction.Confirm,
                "Back" => GameAction.Cancel,
                _ => throw new InvalidOperationException("Unknown battle decision control " + hit.Id),
            });
    }

    private void PointAtTarget(BattleDecision decision, ActorRef? actor)
    {
        if (decision.TargetAt(actor) is { } target)
        {
            if (target.Target == decision.Candidate) return;
            SelectTarget(target);
            FinishSubmission(decision.Snapshot);
        }
        else if (_attemptedTarget is not null)
        {
            _attemptedTarget = null;
            Present();
        }
    }
}
