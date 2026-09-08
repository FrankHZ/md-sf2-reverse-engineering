using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Sessions;

public abstract record PrivateOriginalBattle01TurnCompletionResult;
public sealed record PrivateOriginalBattle01StayCommitted(PrivateOriginalBattle01SessionSnapshot Snapshot)
    : PrivateOriginalBattle01TurnCompletionResult;
public sealed record PrivateOriginalBattle01TurnCompletionRejected(OriginalBattle01StartupDiagnostic Diagnostic)
    : PrivateOriginalBattle01TurnCompletionResult;

public sealed partial class GameSession
{
    public PrivateOriginalBattle01TurnCompletionResult CommitPrivateOriginalBattle01Stay(
        PrivateOriginalBattle01SessionSnapshot? expected, int actorIndex)
    {
        var current = PrivateOriginalBattle01;
        if (current is null) return StayRejected("battle");
        if (expected is null || !ReferenceEquals(expected, current)) return StayRejected("snapshot");
        // Only the accepted already-refreshed comparison inputs authorize the controlled policy.
        if (current.Preparation.Party.GetAdmissionDiagnostic() is { } failure)
            return new PrivateOriginalBattle01TurnCompletionRejected(failure);
        Battle01InitializedState battle;
        try { battle = Battle01TurnCompletion.CommitStay(current.Battle, actorIndex,
            Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats); }
        catch (ArgumentException error) { return StayRejected(error.ParamName ?? "stay"); }
        var next = new PrivateOriginalBattle01SessionSnapshot(current.Preparation, battle,
            current.SourceLocomotion, current.SourceBridge);
        var result = new PrivateOriginalBattle01StayCommitted(next);
        PrivateOriginalBattle01 = next;
        return result;
    }

    private static PrivateOriginalBattle01TurnCompletionRejected StayRejected(string field) =>
        new(new(field, "STAY cannot complete this snapshot under the controlled no-effect policy (" + field + ")."));
}
