using Godot;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.GodotAdapter;

internal enum PrivateBattle01Input { None, Enter, North, East, South, West, Confirm, Cancel }

// Thin consumer: the session remains the sole owner of admission and battle snapshots.
internal static class PrivateBattle01Ui
{
    internal static bool OwnsInput(GameFlowStage stage, bool pending, bool inputsSelected) =>
        stage == GameFlowStage.Battle || (pending && inputsSelected);

    internal static string? Apply(GameSession session, IOriginalBattle01StartupSource? source,
        PrivateBattle01Input input)
    {
        if (session.PrivateOriginalBattle01 is not { } current)
        {
            if (input != PrivateBattle01Input.Enter) return "Pending: N starts the controlled battle.";
            var preparation = session.PreparePrivateOriginalBattle01Startup(
                session.PrivateOriginalBattle01Admission, source,
                OriginalBattle01ControlledPartyPreset.PlayerReadyComparison);
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
                PrivateOriginalBattle01FirstControlEntered => "First player ready. Select a reachable tile.",
                PrivateOriginalBattle01FirstControlUnavailable unavailable =>
                    "First control unavailable: " + unavailable.Decision.Availability,
                PrivateOriginalBattle01FirstControlRejected rejected =>
                    "First control rejected: " + rejected.Diagnostic.Message,
                _ => "First control unavailable.",
            };
        }

        // An unavailable next dispatch has already been reported. Unrelated keys cannot retry it
        // or overwrite its precise reason; the existing presenter retains that result.
        if (current.Battle.Phase is Battle01Phase.PlayerTurnCompleted or Battle01Phase.EnemyTurnCompleted) return null;
        if (current.Battle.FirstControl is not { } control)
            return "Current battle retained. First player control is unavailable; relaunch starts Map 3.";
        if (input == PrivateBattle01Input.Enter)
            return "Battle already initialized; current actor and round retained.";
        if (control.Movement.Stage == Battle01PlayerMovementStage.ActionChoice)
        {
            if (input == PrivateBattle01Input.Confirm)
                return session.CommitPrivateOriginalBattle01Stay(current, control.ActorIndex) switch
                {
                    PrivateOriginalBattle01StayCommitted committed => EnterNextPlayer(session, committed.Snapshot),
                    PrivateOriginalBattle01TurnCompletionRejected rejected => "STAY rejected: " + rejected.Diagnostic.Message,
                    _ => "STAY unavailable.",
                };
            if (input != PrivateBattle01Input.Cancel)
                return "Action choice: Space commits STAY; Backspace cancels.";
        }

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

    private static string EnterNextPlayer(GameSession session, PrivateOriginalBattle01SessionSnapshot completed)
    {
        int candidate = completed.Battle.FirstRound!.CurrentCandidate?.CombatantIndex ?? 255;
        // Exactly one dispatch after this successful STAY result; no frame or key-based retry loop.
        return session.EnterPrivateOriginalBattle01NextPlayerControl(completed, candidate) switch
        {
            PrivateOriginalBattle01NextPlayerControlEntered entered =>
                entered.Snapshot.Battle.FirstControl!.ActorIndex == 0
                    ? "First-round enemies complete. Bowie (A0) ready."
                    : $"STAY complete. Player {entered.Snapshot.Battle.FirstControl.ActorIndex} ready.",
            PrivateOriginalBattle01NextPlayerControlUnavailable { Decision.Availability: Battle01FirstControlAvailability.OpponentAi,
                Decision.ActorIndex: 128 } => CompleteEnemyRelay(session, completed),
            PrivateOriginalBattle01NextPlayerControlUnavailable { Decision.Availability: Battle01FirstControlAvailability.Sentinel }
                when completed.Battle.FirstRound!.CurrentTurnOffset == 18 && completed.Battle.TurnCompletion?.CompletedActorIndex == 0 =>
                "First round exhausted. Nine turns complete; next round not started.",
            PrivateOriginalBattle01NextPlayerControlUnavailable unavailable =>
                $"Next control unavailable: candidate {unavailable.Decision.ActorIndex?.ToString() ?? "sentinel"} / {unavailable.Decision.Availability}.",
            PrivateOriginalBattle01NextPlayerControlRejected rejected =>
                $"Next control rejected: {rejected.Diagnostic.Field}; candidate {candidate}; STAY retained.",
            _ => "Next control unavailable; committed STAY retained.",
        };
    }

    private static string CompleteEnemyRelay(GameSession session, PrivateOriginalBattle01SessionSnapshot current)
    {
        // Six finite attempts, each reading the actual current candidate and committing independently.
        // Never retry this relay from a frame or completed-phase input.
        for (int attempt = 0; attempt < 6; attempt++)
        {
            int actor = current.Battle.FirstRound!.CurrentCandidate?.CombatantIndex ?? 255;
            var result = session.CompletePrivateOriginalBattle01EnemyStandby(current, actor);
            if (result is PrivateOriginalBattle01EnemyStandbyRejected rejected)
                return $"Enemy {actor} standby rejected: {rejected.Diagnostic.Field}; completed actor {current.Battle.TurnCompletion!.CompletedActorIndex} retained.";
            current = ((PrivateOriginalBattle01EnemyStandbyCompleted)result).Snapshot;
            if (current.Battle.FirstRound!.CurrentCandidate is not { CombatantIndex: >= 128 })
                return EnterNextPlayer(session, current);
        }
        return "First-round enemy relay stopped at its limit; last completed turn retained.";
    }
}

public sealed partial class Map3Root
{
    private IOriginalBattle01StartupSource? _privateBattle01Source;
    private PrivateBattle01Presenter? _privateBattle01Presenter;
    private PrivateLocalPresentationRasterMount? _privateBattle01Atlas;

    private bool PollPrivateBattle01()
    {
        if (_session is null || !PrivateBattle01Ui.OwnsInput(_session.PrivateOriginalFlowStage,
                _session.PrivateOriginalBattle01Admission is not null, _privateBattle01Source is not null))
            return false;
        if (_privateBattle01Presenter?.BaseArtUnavailable == true) return true;
        var input = _inputAdapter?.PollPrivateBattle01() ?? PrivateBattle01Input.None;
        if (input == PrivateBattle01Input.None) return true;
        string? outcome = PrivateBattle01Ui.Apply(_session, _privateBattle01Source, input);
        if (outcome is null) return true;
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
            _privateBattle01Presenter.Project(battle.Battle, outcome);
        }
        else
            _privatePresenter?.ProjectBattle01Pending(outcome);
        return true;
    }
}
