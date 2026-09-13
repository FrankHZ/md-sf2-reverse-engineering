using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Sessions;

public abstract record PrivateOriginalBattle01TurnCompletionResult;
public sealed record PrivateOriginalBattle01StayCommitted(PrivateOriginalBattle01SessionSnapshot Snapshot)
    : PrivateOriginalBattle01TurnCompletionResult;
public sealed record PrivateOriginalBattle01DefeatedTurnCompleted(PrivateOriginalBattle01SessionSnapshot Snapshot)
    : PrivateOriginalBattle01TurnCompletionResult;
public sealed record PrivateOriginalBattle01TurnCompletionRejected(OriginalBattle01StartupDiagnostic Diagnostic)
    : PrivateOriginalBattle01TurnCompletionResult;

public sealed partial class GameSession
{
    public PrivateOriginalBattle01TurnCompletionResult CompletePrivateOriginalBattle01DefeatedTurn(
        PrivateOriginalBattle01SessionSnapshot? expected, int actorIndex)
    {
        var current = PrivateOriginalBattle01;
        if (current is null) return DefeatedTurnRejected("battle");
        if (expected is null || !ReferenceEquals(expected, current)) return DefeatedTurnRejected("snapshot");
        if (current.Preparation.Party.GetAdmissionDiagnostic() is { } failure)
            return new PrivateOriginalBattle01TurnCompletionRejected(failure);
        if (current.Preparation.Party.Id is not (OriginalBattle01ControlledPartyPreset.ChesterFirstKillComparisonId or OriginalBattle01ControlledPartyPreset.SarahHealComparisonId))
            return DefeatedTurnRejected("party.deadTurnInput");
        Battle01InitializedState battle;
        try
        {
            battle = Battle01TurnCompletion.CompleteDefeatedTurn(current.Battle, actorIndex,
                Battle01DefeatedTurnCompletionPolicy.ControlledEnemy129AfterChesterDefeat);
            Battle01PlayerPhysicalAttack.RequireAccountingInputs(battle, current.Preparation.Party.CurrentGold,
                current.Preparation.Party.Allies[0].CurrentKills, current.Preparation.Party.Allies[2].CurrentExp,
                current.Preparation.Party.Allies[2].CurrentDefeats, current.Preparation.Party.Allies[0].CurrentDefeats,
                current.Preparation.Party.Allies[2].CurrentKills, current.Preparation.Party.Allies[1].CurrentExp);
        }
        catch (ArgumentException error) { return DefeatedTurnRejected(error.ParamName ?? "deadTurn"); }
        var next = new PrivateOriginalBattle01SessionSnapshot(current.Preparation, battle,
            current.SourceLocomotion, current.SourceBridge);
        var result = new PrivateOriginalBattle01DefeatedTurnCompleted(next);
        PrivateOriginalBattle01 = next;
        return result;
    }

    private static PrivateOriginalBattle01TurnCompletionRejected DefeatedTurnRejected(string field) =>
        new(new(field, "Dead turn rejected (" + field + "); the current state is retained."));

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
