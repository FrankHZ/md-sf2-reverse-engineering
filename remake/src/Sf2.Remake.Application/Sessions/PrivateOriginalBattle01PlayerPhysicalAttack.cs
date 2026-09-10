using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Sessions;

public enum PrivateOriginalBattle01PlayerAttackOperation { Begin, Cycle, Cancel, Confirm }
public abstract record PrivateOriginalBattle01PlayerAttackResult;
public sealed record PrivateOriginalBattle01PlayerAttackApplied(PrivateOriginalBattle01SessionSnapshot Snapshot,
    PrivateOriginalBattle01PlayerAttackOperation Operation) : PrivateOriginalBattle01PlayerAttackResult;
public sealed record PrivateOriginalBattle01PlayerAttackRejected(OriginalBattle01StartupDiagnostic Diagnostic)
    : PrivateOriginalBattle01PlayerAttackResult;

public sealed partial class GameSession
{
    public PrivateOriginalBattle01PlayerAttackResult BeginPrivateOriginalBattle01PlayerAttack(
        PrivateOriginalBattle01SessionSnapshot? expected, int actorIndex) =>
        ApplyPrivateOriginalBattle01PlayerAttack(expected, actorIndex, PrivateOriginalBattle01PlayerAttackOperation.Begin,
            (battle, _) => Battle01PlayerPhysicalAttack.Begin(battle, actorIndex));

    public PrivateOriginalBattle01PlayerAttackResult CyclePrivateOriginalBattle01PlayerAttackTarget(
        PrivateOriginalBattle01SessionSnapshot? expected, int actorIndex, int direction) =>
        ApplyPrivateOriginalBattle01PlayerAttack(expected, actorIndex, PrivateOriginalBattle01PlayerAttackOperation.Cycle,
            (battle, _) => Battle01PlayerPhysicalAttack.Cycle(battle, actorIndex, direction));

    public PrivateOriginalBattle01PlayerAttackResult CancelPrivateOriginalBattle01PlayerAttackTarget(
        PrivateOriginalBattle01SessionSnapshot? expected, int actorIndex) =>
        ApplyPrivateOriginalBattle01PlayerAttack(expected, actorIndex, PrivateOriginalBattle01PlayerAttackOperation.Cancel,
            (battle, _) => Battle01PlayerPhysicalAttack.Cancel(battle, actorIndex));

    public PrivateOriginalBattle01PlayerAttackResult ConfirmPrivateOriginalBattle01PlayerAttack(
        PrivateOriginalBattle01SessionSnapshot? expected, int actorIndex) =>
        ApplyPrivateOriginalBattle01PlayerAttack(expected, actorIndex, PrivateOriginalBattle01PlayerAttackOperation.Confirm,
            (battle, policy) => Battle01PlayerPhysicalAttack.Confirm(battle, actorIndex, policy));

    private PrivateOriginalBattle01PlayerAttackResult ApplyPrivateOriginalBattle01PlayerAttack(
        PrivateOriginalBattle01SessionSnapshot? expected, int actorIndex, PrivateOriginalBattle01PlayerAttackOperation operation,
        Func<Battle01InitializedState, Battle01PlayerPhysicalCompletionPolicy, Battle01InitializedState> transition)
    {
        var current = PrivateOriginalBattle01;
        if (current is null) return PlayerAttackRejected("battle");
        if (expected is null || !ReferenceEquals(expected, current)) return PlayerAttackRejected("snapshot");
        if (current.Preparation.Party.GetAdmissionDiagnostic() is { } failure)
            return new PrivateOriginalBattle01PlayerAttackRejected(failure);
        if (current.Preparation.Party.Id != OriginalBattle01ControlledPartyPreset.PlayerAttackComparisonId &&
            current.Preparation.Party.Id != OriginalBattle01ControlledPartyPreset.FirstDefeatComparisonId &&
            current.Preparation.Party.Id != OriginalBattle01ControlledPartyPreset.ChesterPlayerAttackComparisonId)
            return PlayerAttackRejected("party.expInput");
        var policy = current.Preparation.Party.Id == OriginalBattle01ControlledPartyPreset.FirstDefeatComparisonId ||
            current.Preparation.Party.Id == OriginalBattle01ControlledPartyPreset.ChesterPlayerAttackComparisonId && actorIndex == 0
            ? Battle01PlayerPhysicalCompletionPolicy.ControlledStrikeAndFirstDefeat
            : Battle01PlayerPhysicalCompletionPolicy.ControlledNonlethalStrikeAndExp;
        Battle01InitializedState battle;
        try
        {
            battle = transition(current.Battle, policy);
            Battle01PlayerPhysicalAttack.RequireAccountingInputs(battle, current.Preparation.Party.CurrentGold,
                current.Preparation.Party.Allies[0].CurrentKills, current.Preparation.Party.Allies[2].CurrentExp);
        }
        catch (ArgumentException error) { return PlayerAttackRejected(error.ParamName ?? "attack"); }
        var next = new PrivateOriginalBattle01SessionSnapshot(current.Preparation, battle, current.SourceLocomotion, current.SourceBridge);
        var result = new PrivateOriginalBattle01PlayerAttackApplied(next, operation);
        PrivateOriginalBattle01 = next;
        return result;
    }

    private static PrivateOriginalBattle01PlayerAttackRejected PlayerAttackRejected(string field) =>
        new(new(field, "Player attack rejected (" + field + "); current state retained."));
}
