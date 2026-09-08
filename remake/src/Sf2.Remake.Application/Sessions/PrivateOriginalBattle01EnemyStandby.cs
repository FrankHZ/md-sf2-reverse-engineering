using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Sessions;

public abstract record PrivateOriginalBattle01EnemyStandbyResult;
public sealed record PrivateOriginalBattle01EnemyStandbyCompleted(PrivateOriginalBattle01SessionSnapshot Snapshot)
    : PrivateOriginalBattle01EnemyStandbyResult;
public sealed record PrivateOriginalBattle01EnemyStandbyRejected(OriginalBattle01StartupDiagnostic Diagnostic)
    : PrivateOriginalBattle01EnemyStandbyResult;

public sealed partial class GameSession
{
    public PrivateOriginalBattle01EnemyStandbyResult CompletePrivateOriginalBattle01EnemyStandby(
        PrivateOriginalBattle01SessionSnapshot? expected, int actorIndex)
    {
        var current = PrivateOriginalBattle01;
        if (current is null) return EnemyStandbyRejected("battle");
        if (expected is null || !ReferenceEquals(expected, current)) return EnemyStandbyRejected("snapshot");
        if (current.Preparation.Party.GetAdmissionDiagnostic() is { } failure)
            return new PrivateOriginalBattle01EnemyStandbyRejected(failure);
        Battle01InitializedState battle;
        try { battle = Battle01EnemyStandby.CompleteFirst(current.Battle, actorIndex,
            Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats); }
        catch (ArgumentException error) { return EnemyStandbyRejected(error.ParamName ?? "standby"); }
        var next = new PrivateOriginalBattle01SessionSnapshot(current.Preparation, battle,
            current.SourceLocomotion, current.SourceBridge);
        var result = new PrivateOriginalBattle01EnemyStandbyCompleted(next);
        PrivateOriginalBattle01 = next;
        return result;
    }
    private static PrivateOriginalBattle01EnemyStandbyRejected EnemyStandbyRejected(string field) =>
        new(new(field, "First enemy standby rejected (" + field + "); completed player STAY is retained."));
}
