using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Sessions;

public abstract record PrivateOriginalBattle01DefeatReturnResult;
public sealed record PrivateOriginalBattle01DefeatReturnRequested(PrivateOriginalBattle01SessionSnapshot Snapshot)
    : PrivateOriginalBattle01DefeatReturnResult;
public sealed record PrivateOriginalBattle01DefeatReturnRejected(OriginalBattle01StartupDiagnostic Diagnostic)
    : PrivateOriginalBattle01DefeatReturnResult;

public sealed partial class GameSession
{
    public PrivateOriginalBattle01DefeatReturnResult RequestPrivateOriginalBattle01DefeatReturn(
        PrivateOriginalBattle01SessionSnapshot? expected)
    {
        var current = PrivateOriginalBattle01;
        if (current is null) return DefeatReturnRejected("battle");
        if (expected is null || !ReferenceEquals(expected, current)) return DefeatReturnRejected("snapshot");
        if (current.Battle.Phase != Battle01Phase.DefeatRecoveryPending || current.DefeatReturn is not null)
            return DefeatReturnRejected("return.phase");
        var prepared = current.Preparation;
        if (prepared.ReturnInputs is null) return DefeatReturnRejected("return.inputs");
        if (prepared.ReturnInputs.GetAdmissionDiagnostic() is { } inputFailure)
            return new PrivateOriginalBattle01DefeatReturnRejected(inputFailure);
        if (prepared.Party.GetAdmissionDiagnostic() is { } partyFailure)
            return new PrivateOriginalBattle01DefeatReturnRejected(partyFailure);
        if (prepared.Party.Id != OriginalBattle01ControlledPartyPreset.LeaderDefeatComparisonId)
            return DefeatReturnRejected("party.id");
        Battle01DefeatReturnRequest request;
        try
        {
            var party = prepared.Party;
            Battle01PlayerPhysicalAttack.RequireAccountingInputs(current.Battle.DefeatRecovery!.Before,
                party.CurrentGold, party.Allies[0].CurrentKills, party.Allies[2].CurrentExp,
                party.Allies[2].CurrentDefeats, party.Allies[0].CurrentDefeats);
            request = Battle01DefeatReturn.Select(current.Battle, prepared.ReturnAdmission);
        }
        catch (ArgumentException error) { return DefeatReturnRejected(error.ParamName ?? "return"); }
        var next = new PrivateOriginalBattle01SessionSnapshot(prepared, current.Battle,
            current.SourceLocomotion, current.SourceBridge, request);
        var result = new PrivateOriginalBattle01DefeatReturnRequested(next);
        PrivateOriginalBattle01 = next;
        return result;
    }

    private static PrivateOriginalBattle01DefeatReturnRejected DefeatReturnRejected(string field) =>
        new(new(field, "Defeat return rejected (" + field + "); the current state is retained."));
}
