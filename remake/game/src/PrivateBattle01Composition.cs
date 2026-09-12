using Godot;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.GodotAdapter;

internal enum PrivateBattle01Input { None, Enter, North, East, South, West, Confirm, Cancel, Attack }

// Thin consumer: the session remains the sole owner of admission and battle snapshots.
internal static class PrivateBattle01Ui
{
    internal static bool OwnsInput(GameFlowStage stage, bool pending, bool inputsSelected) =>
        stage == GameFlowStage.Battle || (pending && inputsSelected);

    internal static string? Apply(GameSession session, IOriginalBattle01StartupSource? source,
        PrivateBattle01Input input, OriginalBattle01ControlledArrivalInputs? arrivalInputs = null)
    {
        if (session.PrivateOriginalBattle01 is not { } current)
        {
            if (input != PrivateBattle01Input.Enter) return "Pending: N starts the controlled battle.";
            var preparation = session.PreparePrivateOriginalBattle01Startup(
                session.PrivateOriginalBattle01Admission, source,
                OriginalBattle01ControlledPartyPreset.LeaderDefeatComparison,
                OriginalBattle01ControlledReturnInputs.GransealFirstAttemptComparison, arrivalInputs);
            if (preparation is not PrivateOriginalBattle01StartupPrepared prepared)
                return "Prepare rejected: " + ((PrivateOriginalBattle01StartupRejected)preparation).Diagnostic.Message;
            var initialization = session.InitializePrivateOriginalBattle01(prepared);
            if (initialization is not PrivateOriginalBattle01Initialized initialized)
                return "Initialize rejected: " + ((PrivateOriginalBattle01InitializationRejected)initialization).Diagnostic.Message;
            var round = session.EnterPrivateOriginalBattle01FirstRound(initialized.Snapshot);
            if (round is not PrivateOriginalBattle01FirstRoundEntered entered)
                return "First round rejected: " + ((PrivateOriginalBattle01FirstRoundRejected)round).Diagnostic.Message;
            var order = entered.Snapshot.Battle.FirstRound!;
            var candidate = order.CurrentCandidate;
            if (candidate is null) return "First control unavailable: current slot is sentinel.";
            return session.EnterPrivateOriginalBattle01FirstControl(entered.Snapshot, candidate.Value.CombatantIndex) switch
            {
                PrivateOriginalBattle01FirstControlEntered => arrivalInputs is null
                    ? "Granseal return comparison selected. First player ready."
                    : "Leader/Granseal return/arrival comparisons selected. First player ready.",
                PrivateOriginalBattle01FirstControlUnavailable unavailable =>
                    "First control unavailable: " + unavailable.Decision.Availability,
                PrivateOriginalBattle01FirstControlRejected rejected =>
                    "First control rejected: " + rejected.Diagnostic.Message,
                _ => "First control unavailable.",
            };
        }

        if (current.Arrival is not null) return input == PrivateBattle01Input.None ? null :
            "Battle controls unavailable. Use the church-pocket movement controls.";
        if (current.CanEnterExploration && input == PrivateBattle01Input.Confirm)
            return session.EnterPrivateOriginalBattle01Exploration(current) switch
            {
                PrivateOriginalBattle01ExplorationEntered => PrivateMap3PresentationPlan.ArrivalStatus,
                PrivateOriginalBattle01ExplorationEntryRejected rejected => rejected.Diagnostic.Message,
                _ => null,
            };
        if (current.CanRequestDefeatReturn && input == PrivateBattle01Input.Confirm)
            return session.RequestPrivateOriginalBattle01DefeatReturn(current) switch
            {
                PrivateOriginalBattle01DefeatReturnRequested => PrivateBattle01Presenter.ReturnRequestedStatus,
                PrivateOriginalBattle01DefeatReturnRejected rejected => rejected.Diagnostic.Message,
                _ => null,
            };

        if (current.Battle.Phase == Battle01Phase.DefeatPending && input == PrivateBattle01Input.Confirm)
            return session.RecoverPrivateOriginalBattle01Defeat(current) switch
            {
                PrivateOriginalBattle01DefeatRecovered recovered => recovered.Snapshot.CanRequestDefeatReturn
                    ? "Bowie HP restored. Gold halved. Space requests the Granseal return."
                    : "Bowie HP restored. Gold halved. Return unavailable.",
                PrivateOriginalBattle01DefeatRecoveryRejected rejected => rejected.Diagnostic.Message,
                _ => null,
            };

        // An unavailable next dispatch has already been reported. Unrelated keys cannot retry it
        // or overwrite its precise reason; the existing presenter retains that result.
        if (current.Battle.FirstControl is not { } control) return null;
        if (input == PrivateBattle01Input.Enter)
            return "Battle already initialized; current actor and round retained.";
        if (control.Movement.Stage == Battle01PlayerMovementStage.TargetSelection ||
            (control.Movement.Stage == Battle01PlayerMovementStage.ActionChoice && input == PrivateBattle01Input.Attack))
        {
            PrivateOriginalBattle01PlayerAttackResult? attack = input switch
            {
                PrivateBattle01Input.Attack when control.Movement.Stage == Battle01PlayerMovementStage.ActionChoice =>
                    session.BeginPrivateOriginalBattle01PlayerAttack(current, control.ActorIndex),
                PrivateBattle01Input.North or PrivateBattle01Input.West =>
                    session.CyclePrivateOriginalBattle01PlayerAttackTarget(current, control.ActorIndex, -1),
                PrivateBattle01Input.South or PrivateBattle01Input.East =>
                    session.CyclePrivateOriginalBattle01PlayerAttackTarget(current, control.ActorIndex, 1),
                PrivateBattle01Input.Cancel => session.CancelPrivateOriginalBattle01PlayerAttackTarget(current, control.ActorIndex),
                PrivateBattle01Input.Confirm => session.ConfirmPrivateOriginalBattle01PlayerAttack(current, control.ActorIndex),
                _ => null,
            };
            return attack switch
            {
                PrivateOriginalBattle01PlayerAttackApplied { Operation: PrivateOriginalBattle01PlayerAttackOperation.Confirm } applied =>
                    DispatchNext(session, applied.Snapshot),
                PrivateOriginalBattle01PlayerAttackApplied { Operation: PrivateOriginalBattle01PlayerAttackOperation.Cancel } =>
                    "Target cancelled. Provisional position retained; Backspace restores turn origin.",
                PrivateOriginalBattle01PlayerAttackApplied => "Select target: I/J previous, K/L next, Space attack, Backspace cancel.",
                PrivateOriginalBattle01PlayerAttackRejected rejected => rejected.Diagnostic.Message,
                _ => null,
            };
        }
        if (control.Movement.Stage == Battle01PlayerMovementStage.ActionChoice)
        {
            if (input == PrivateBattle01Input.Confirm)
                return session.CommitPrivateOriginalBattle01Stay(current, control.ActorIndex) switch
                {
                    PrivateOriginalBattle01StayCommitted committed => DispatchNext(session, committed.Snapshot),
                    PrivateOriginalBattle01TurnCompletionRejected rejected => "STAY rejected: " + rejected.Diagnostic.Message,
                    _ => "STAY unavailable.",
                };
            if (input != PrivateBattle01Input.Cancel)
                return "Action choice: A selects Attack; Space commits STAY; Backspace cancels.";
        }
        if (input is PrivateBattle01Input.Attack or PrivateBattle01Input.None) return null;

        PrivateOriginalBattle01PlayerMovementResult result;
        if (input == PrivateBattle01Input.Confirm)
            result = session.ConfirmPrivateOriginalBattle01PlayerMovement(current, control.ActorIndex);
        else if (input == PrivateBattle01Input.Cancel)
            result = session.CancelPrivateOriginalBattle01PlayerMovement(current, control.ActorIndex);
        else
        {
            var cursor = control.Movement.Cursor;
            (int dx, int dy) = input switch
            {
                PrivateBattle01Input.North => (0, -1), PrivateBattle01Input.East => (1, 0),
                PrivateBattle01Input.South => (0, 1), PrivateBattle01Input.West => (-1, 0),
                _ => (0, 0),
            };
            int x = cursor.X + dx, y = cursor.Y + dy;
            if (x < current.Battle.AreaX || x >= current.Battle.AreaX + current.Battle.AreaWidth ||
                y < current.Battle.AreaY || y >= current.Battle.AreaY + current.Battle.AreaHeight)
                return "Cursor rejected: outside the battle area; selection retained.";
            var destination = new MapPosition(x, y);
            if (control.Movement.Range.Grid.CostAt(destination) is null)
                return "Cursor rejected: tile is unreachable; selection retained.";
            result = session.SelectPrivateOriginalBattle01PlayerDestination(current, control.ActorIndex, destination);
        }
        return result switch
        {
            PrivateOriginalBattle01PlayerMovementApplied { Operation: PrivateOriginalBattle01PlayerMovementOperation.Confirm } =>
                "Provisional relocation. No action or turn has completed.",
            PrivateOriginalBattle01PlayerMovementApplied { Operation: PrivateOriginalBattle01PlayerMovementOperation.Cancel } =>
                "Cancelled. Actor and occupancy restored to turn origin.",
            PrivateOriginalBattle01PlayerMovementApplied => "Destination preview selected; live actor unchanged.",
            PrivateOriginalBattle01PlayerMovementRejected rejected => "Movement rejected: " + rejected.Diagnostic.Message,
            _ => "Movement unavailable.",
        };
    }

    internal static string DispatchNext(GameSession session, PrivateOriginalBattle01SessionSnapshot current)
    {
        bool generated = false;
        // At most the remainder of this buffer and one new buffer; actual player control yields.
        int limit = current.Battle.FirstRound!.Slots.Count * 2;
        for (int attempt = 0; attempt < limit; attempt++)
        {
            if (current.Battle.Phase == Battle01Phase.DefeatPending)
                return "Bowie defeated. Space applies HP/gold recovery.";
            if (current.Battle.Phase == Battle01Phase.DefeatRecoveryPending)
                return current.DefeatReturn is not null ? PrivateBattle01Presenter.ReturnRequestedStatus
                    : current.CanRequestDefeatReturn ? "Recovery applied. Space requests the Granseal return."
                    : "Recovery applied. Return unavailable.";
            var order = current.Battle.FirstRound!;
            if (order.CurrentCandidate is not { } candidate)
            {
                if (generated) return "Round dispatch stopped at its limit; current state retained.";
                var result = session.EnterPrivateOriginalBattle01NextRound(current);
                if (result is PrivateOriginalBattle01FirstRoundRejected rejected)
                    return $"Next round rejected: {rejected.Diagnostic.Field}; round {order.RoundNumber} retained.";
                current = ((PrivateOriginalBattle01FirstRoundEntered)result).Snapshot;
                generated = true;
                continue;
            }
            int actor = candidate.CombatantIndex;
            if (actor >= 128)
            {
                var word = current.Battle.Roster.SingleOrDefault(unit => unit.Index == actor)?.AiBitfield;
                if (word is { } bits && (bits & 1) != 0)
                {
                    switch (session.CompletePrivateOriginalBattle01EnemyPursuit(current, actor))
                    {
                        case PrivateOriginalBattle01AttackSelectionRequired boundary:
                            switch (session.CompletePrivateOriginalBattle01EnemyPhysicalAttack(current, boundary.ActorIndex))
                            {
                                case PrivateOriginalBattle01EnemyPhysicalAttackCompleted attack:
                                    current = attack.Snapshot;
                                    continue;
                                case PrivateOriginalBattle01EnemyPhysicalAttackRejected attackRejected:
                                    return $"Enemy {actor} physical attack rejected: {attackRejected.Diagnostic.Field}; current state retained.";
                                default:
                                    return $"Enemy {actor} physical attack unavailable; current state retained.";
                            }
                        case PrivateOriginalBattle01EnemyPursuitRejected failure:
                            return $"Enemy {actor} pursuit rejected: {failure.Diagnostic.Field}; current state retained.";
                        case PrivateOriginalBattle01EnemyPursuitCompleted completed:
                            current = completed.Snapshot;
                            continue;
                        default:
                            return $"Enemy {actor} pursuit unavailable; current state retained.";
                    }
                }
                var result = session.CompletePrivateOriginalBattle01EnemyStandby(current, actor);
                if (result is PrivateOriginalBattle01EnemyStandbyRejected rejected)
                    return $"Enemy {actor} standby rejected: {rejected.Diagnostic.Field}; last completed state retained.";
                current = ((PrivateOriginalBattle01EnemyStandbyCompleted)result).Snapshot;
                continue;
            }
            return session.EnterPrivateOriginalBattle01NextPlayerControl(current, actor) switch
            {
                PrivateOriginalBattle01NextPlayerControlEntered => $"Round {order.RoundNumber}. Player {actor} ready.",
                PrivateOriginalBattle01NextPlayerControlUnavailable unavailable =>
                    $"Control unavailable: actor {actor} / {unavailable.Decision.Availability}.",
                PrivateOriginalBattle01NextPlayerControlRejected rejected =>
                    $"Control rejected: {rejected.Diagnostic.Field}; actor {actor}; current state retained.",
                _ => "Control unavailable; current state retained.",
            };
        }
        return "Round dispatch stopped at its limit; current state retained.";
    }

}

public sealed partial class Map3Root
{
    private IOriginalBattle01StartupSource? _privateBattle01Source;
    private PrivateBattle01Presenter? _privateBattle01Presenter;
    private PrivateLocalPresentationRasterMount? _privateBattle01Atlas;

    private bool PollPrivateBattle01()
    {
        if (_session?.PrivateOriginalMapArrival is not null)
        {
            PollGransealMovement();
            return true;
        }
        if (_session is null || !PrivateBattle01Ui.OwnsInput(_session.PrivateOriginalFlowStage,
                _session.PrivateOriginalBattle01Admission is not null, _privateBattle01Source is not null))
            return false;
        if (_privateBattle01Presenter?.BaseArtUnavailable == true) return true;
        var input = _inputAdapter?.PollPrivateBattle01() ?? PrivateBattle01Input.None;
        if (input == PrivateBattle01Input.None) return true;
        string? outcome = PrivateBattle01Ui.Apply(_session, _privateBattle01Source, input,
            _privatePresenter?.CanDisplayArrival == true ? OriginalBattle01ControlledArrivalInputs.GransealFirstAttemptComparison : null);
        if (outcome is null) return true;
        if (_session.PrivateOriginalMapArrival is { } arrival)
        {
            ProjectGransealArrival(arrival);
            return true;
        }
        if (_session.PrivateOriginalBattle01 is { } battle)
        {
            if (_privateBattle01Presenter is null)
            {
                // All exploration, HUD and synthetic presenters attach beneath root canvas items.
                // Hide those complete subtrees before adding the diagnostic battlefield.
                foreach (var child in GetChildren().OfType<CanvasItem>()) child.Hide();
                _privateBattle01Presenter = new PrivateBattle01Presenter();
                AddChild(_privateBattle01Presenter);
                if (_privateBattle01Atlas is not null && !_privateBattle01Presenter.TryBindBaseAtlas(
                        _privateBattle01Atlas, battle.Preparation.Pending.Definition))
                {
                    _privateBattle01Presenter.ProjectBaseArtUnavailable();
                    return true;
                }
            }
            _privateBattle01Presenter.Project(battle, outcome);
        }
        else
            _privatePresenter?.ProjectBattle01Pending(outcome);
        return true;
    }
}
