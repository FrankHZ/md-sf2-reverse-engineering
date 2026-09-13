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
            var policy = current.Preparation.Party.Id == OriginalBattle01ControlledPartyPreset.LeaderDefeatComparisonId
                ? Battle01PhysicalCompletionPolicy.ControlledLeaderDefeatPending
                : current.Preparation.Party.Id == OriginalBattle01ControlledPartyPreset.ChesterDefeatComparisonId
                ? Battle01PhysicalCompletionPolicy.ControlledFirstAllyDefeat
                : (current.Preparation.Party.Id is OriginalBattle01ControlledPartyPreset.ChesterFirstKillComparisonId or OriginalBattle01ControlledPartyPreset.SarahHealComparisonId)
                ? Battle01PhysicalCompletionPolicy.ControlledChesterDefeatAfterFirstKill
                : Battle01PhysicalCompletionPolicy.ControlledNonlethalStrike;
            battle = Battle01EnemyPhysicalAttack.CompleteNext(current.Battle, actorIndex, policy,
                allowChesterCounter: current.Preparation.Party.Allies[2].CurrentExp is not null);
            Battle01PlayerPhysicalAttack.RequireAccountingInputs(battle, current.Preparation.Party.CurrentGold,
                current.Preparation.Party.Allies[0].CurrentKills, current.Preparation.Party.Allies[2].CurrentExp,
                current.Preparation.Party.Allies[2].CurrentDefeats, current.Preparation.Party.Allies[0].CurrentDefeats,
                current.Preparation.Party.Allies[2].CurrentKills, current.Preparation.Party.Allies[1].CurrentExp);
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
