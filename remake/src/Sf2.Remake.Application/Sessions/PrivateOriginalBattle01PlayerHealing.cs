using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Sessions;

public enum PrivateOriginalBattle01PlayerHealingOperation { Begin, SelectSpell, CycleTarget, Cancel, Confirm }
public abstract record PrivateOriginalBattle01PlayerHealingResult;
public sealed record PrivateOriginalBattle01PlayerHealingApplied(PrivateOriginalBattle01SessionSnapshot Snapshot,
    PrivateOriginalBattle01PlayerHealingOperation Operation) : PrivateOriginalBattle01PlayerHealingResult;
public sealed record PrivateOriginalBattle01PlayerHealingRejected(OriginalBattle01StartupDiagnostic Diagnostic)
    : PrivateOriginalBattle01PlayerHealingResult;

public sealed partial class GameSession
{
    public PrivateOriginalBattle01PlayerHealingResult BeginPrivateOriginalBattle01PlayerHealing(
        PrivateOriginalBattle01SessionSnapshot? expected, int actorIndex) =>
        ApplyPrivateOriginalBattle01PlayerHealing(expected, PrivateOriginalBattle01PlayerHealingOperation.Begin,
            battle => Battle01PlayerHealing.Begin(battle, actorIndex));

    public PrivateOriginalBattle01PlayerHealingResult SelectPrivateOriginalBattle01HealingSpell(
        PrivateOriginalBattle01SessionSnapshot? expected, int actorIndex, byte spellEntry) =>
        ApplyPrivateOriginalBattle01PlayerHealing(expected, PrivateOriginalBattle01PlayerHealingOperation.SelectSpell,
            battle => Battle01PlayerHealing.SelectSpell(battle, actorIndex, spellEntry));

    public PrivateOriginalBattle01PlayerHealingResult CyclePrivateOriginalBattle01HealingTarget(
        PrivateOriginalBattle01SessionSnapshot? expected, int actorIndex, int direction) =>
        ApplyPrivateOriginalBattle01PlayerHealing(expected, PrivateOriginalBattle01PlayerHealingOperation.CycleTarget,
            battle => Battle01PlayerHealing.CycleTarget(battle, actorIndex, direction));

    public PrivateOriginalBattle01PlayerHealingResult CancelPrivateOriginalBattle01PlayerHealing(
        PrivateOriginalBattle01SessionSnapshot? expected, int actorIndex) =>
        ApplyPrivateOriginalBattle01PlayerHealing(expected, PrivateOriginalBattle01PlayerHealingOperation.Cancel,
            battle => Battle01PlayerHealing.Cancel(battle, actorIndex));

    public PrivateOriginalBattle01PlayerHealingResult ConfirmPrivateOriginalBattle01PlayerHealing(
        PrivateOriginalBattle01SessionSnapshot? expected, int actorIndex) =>
        ApplyPrivateOriginalBattle01PlayerHealing(expected, PrivateOriginalBattle01PlayerHealingOperation.Confirm,
            battle => Battle01PlayerHealing.Confirm(battle, actorIndex, Battle01HealingCompletionPolicy.ControlledSarahHealOneBowie));

    private PrivateOriginalBattle01PlayerHealingResult ApplyPrivateOriginalBattle01PlayerHealing(
        PrivateOriginalBattle01SessionSnapshot? expected, PrivateOriginalBattle01PlayerHealingOperation operation,
        Func<Battle01InitializedState, Battle01InitializedState> transition)
    {
        var current = PrivateOriginalBattle01;
        if (current is null) return HealingRejected("battle");
        if (expected is null || !ReferenceEquals(expected, current)) return HealingRejected("snapshot");
        var party = current.Preparation.Party;
        if (party.GetAdmissionDiagnostic() is { } failure) return new PrivateOriginalBattle01PlayerHealingRejected(failure);
        if (party.Id != OriginalBattle01ControlledPartyPreset.SarahHealComparisonId) return HealingRejected("party.healInput");
        Battle01InitializedState battle;
        try
        {
            battle = transition(current.Battle);
            Battle01PlayerPhysicalAttack.RequireAccountingInputs(battle, party.CurrentGold, party.Allies[0].CurrentKills,
                party.Allies[2].CurrentExp, party.Allies[2].CurrentDefeats, party.Allies[0].CurrentDefeats,
                party.Allies[2].CurrentKills, party.Allies[1].CurrentExp);
        }
        catch (ArgumentException error) { return HealingRejected(error.ParamName ?? "heal"); }
        var next = new PrivateOriginalBattle01SessionSnapshot(current.Preparation, battle, current.SourceLocomotion, current.SourceBridge);
        var result = new PrivateOriginalBattle01PlayerHealingApplied(next, operation);
        PrivateOriginalBattle01 = next;
        return result;
    }

    private static PrivateOriginalBattle01PlayerHealingRejected HealingRejected(string field) =>
        new(new(field, "Healing rejected (" + field + "); current state retained."));
}
