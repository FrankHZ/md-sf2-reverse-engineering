using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Sessions;

public abstract record PrivateOriginalBattle01EnemyPursuitResult;
public sealed record PrivateOriginalBattle01EnemyPursuitCompleted(PrivateOriginalBattle01SessionSnapshot Snapshot)
    : PrivateOriginalBattle01EnemyPursuitResult;
public sealed record PrivateOriginalBattle01EnemyPursuitRejected(OriginalBattle01StartupDiagnostic Diagnostic)
    : PrivateOriginalBattle01EnemyPursuitResult;
public sealed record PrivateOriginalBattle01AttackSelectionRequired(int ActorIndex,
    IReadOnlyList<Battle01AttackCandidate> Targets, OriginalBattle01StartupDiagnostic Diagnostic)
    : PrivateOriginalBattle01EnemyPursuitResult;

public sealed partial class GameSession
{
    public PrivateOriginalBattle01EnemyPursuitResult CompletePrivateOriginalBattle01EnemyPursuit(
        PrivateOriginalBattle01SessionSnapshot? expected, int actorIndex)
    {
        var current = PrivateOriginalBattle01;
        if (current is null) return EnemyPursuitRejected("battle");
        if (expected is null || !ReferenceEquals(expected, current)) return EnemyPursuitRejected("snapshot");
        if (current.Preparation.Party.GetAdmissionDiagnostic() is { } failure)
            return new PrivateOriginalBattle01EnemyPursuitRejected(failure);
        Battle01InitializedState battle;
        try
        {
            battle = Battle01EnemyPursuit.CompleteNext(current.Battle, actorIndex,
                Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
        }
        catch (Battle01AttackSelectionRequiredException boundary)
        {
            return new PrivateOriginalBattle01AttackSelectionRequired(boundary.ActorIndex, boundary.Targets,
                new("attack.targets", $"Enemy {boundary.ActorIndex}: attack selection required; current turn retained."));
        }
        catch (ArgumentException error) { return EnemyPursuitRejected(error.ParamName ?? "pursuit"); }
        var next = new PrivateOriginalBattle01SessionSnapshot(current.Preparation, battle,
            current.SourceLocomotion, current.SourceBridge);
        var result = new PrivateOriginalBattle01EnemyPursuitCompleted(next);
        PrivateOriginalBattle01 = next;
        return result;
    }
    private static PrivateOriginalBattle01EnemyPursuitRejected EnemyPursuitRejected(string field) =>
        new(new(field, "Enemy pursuit rejected (" + field + "); the current state is retained."));
}
