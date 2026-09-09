using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Sessions;

public abstract record PrivateOriginalBattle01EnemyPhysicalAttackResult;
public sealed record PrivateOriginalBattle01EnemyPhysicalAttackCompleted(PrivateOriginalBattle01SessionSnapshot Snapshot)
    : PrivateOriginalBattle01EnemyPhysicalAttackResult;
public sealed record PrivateOriginalBattle01EnemyPhysicalAttackRejected(OriginalBattle01StartupDiagnostic Diagnostic)
    : PrivateOriginalBattle01EnemyPhysicalAttackResult;

public sealed partial class GameSession
{
    public PrivateOriginalBattle01EnemyPhysicalAttackResult CompletePrivateOriginalBattle01EnemyPhysicalAttack(
        PrivateOriginalBattle01SessionSnapshot? expected, int actorIndex)
    {
        var current = PrivateOriginalBattle01;
        if (current is null) return EnemyPhysicalAttackRejected("battle");
        if (expected is null || !ReferenceEquals(expected, current)) return EnemyPhysicalAttackRejected("snapshot");
        if (current.Preparation.Party.GetAdmissionDiagnostic() is { } failure)
            return new PrivateOriginalBattle01EnemyPhysicalAttackRejected(failure);
        Battle01InitializedState battle;
        try
        {
            battle = Battle01EnemyPhysicalAttack.CompleteNext(current.Battle, actorIndex,
                Battle01PhysicalCompletionPolicy.ControlledNonlethalStrike);
        }
        catch (ArgumentException error) { return EnemyPhysicalAttackRejected(error.ParamName ?? "attack"); }
        var next = new PrivateOriginalBattle01SessionSnapshot(current.Preparation, battle,
            current.SourceLocomotion, current.SourceBridge);
        var result = new PrivateOriginalBattle01EnemyPhysicalAttackCompleted(next);
        PrivateOriginalBattle01 = next;
        return result;
    }

    private static PrivateOriginalBattle01EnemyPhysicalAttackRejected EnemyPhysicalAttackRejected(string field) =>
        new(new(field, "Physical attack rejected (" + field + "); the current state is retained."));
}
