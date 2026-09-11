using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Sessions;

public abstract record PrivateOriginalBattle01DefeatRecoveryResult;
public sealed record PrivateOriginalBattle01DefeatRecovered(PrivateOriginalBattle01SessionSnapshot Snapshot)
    : PrivateOriginalBattle01DefeatRecoveryResult;
public sealed record PrivateOriginalBattle01DefeatRecoveryRejected(OriginalBattle01StartupDiagnostic Diagnostic)
    : PrivateOriginalBattle01DefeatRecoveryResult;

public sealed partial class GameSession
{
    public PrivateOriginalBattle01DefeatRecoveryResult RecoverPrivateOriginalBattle01Defeat(
        PrivateOriginalBattle01SessionSnapshot? expected)
    {
        var current = PrivateOriginalBattle01;
        if (current is null) return DefeatRecoveryRejected("battle");
        if (expected is null || !ReferenceEquals(expected, current)) return DefeatRecoveryRejected("snapshot");
        if (current.Battle.Phase != Battle01Phase.DefeatPending) return DefeatRecoveryRejected("recovery.phase");
        var party = current.Preparation.Party;
        if (party.GetAdmissionDiagnostic() is { } failure)
            return new PrivateOriginalBattle01DefeatRecoveryRejected(failure);
        if (party.Id != OriginalBattle01ControlledPartyPreset.LeaderDefeatComparisonId)
            return DefeatRecoveryRejected("party.id");
        Battle01InitializedState battle;
        try
        {
            // Authenticate preparation against the immutable HP0/gold120 terminal, not healed state.
            Battle01PlayerPhysicalAttack.RequireAccountingInputs(current.Battle, party.CurrentGold,
                party.Allies[0].CurrentKills, party.Allies[2].CurrentExp,
                party.Allies[2].CurrentDefeats, party.Allies[0].CurrentDefeats);
            battle = Battle01DefeatRecovery.Complete(current.Battle);
        }
        catch (ArgumentException error) { return DefeatRecoveryRejected(error.ParamName ?? "recovery"); }
        var next = new PrivateOriginalBattle01SessionSnapshot(current.Preparation, battle,
            current.SourceLocomotion, current.SourceBridge);
        var result = new PrivateOriginalBattle01DefeatRecovered(next);
        PrivateOriginalBattle01 = next;
        return result;
    }

    private static PrivateOriginalBattle01DefeatRecoveryRejected DefeatRecoveryRejected(string field) =>
        new(new(field, "Defeat recovery rejected (" + field + "); the current state is retained."));
}
