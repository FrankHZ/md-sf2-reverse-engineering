using System.Text.Json;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Domain.Tests.Battles;

public sealed class Battle01PlayerHealingTests
{
    internal static Battle01InitializedState Ready()
    {
        var current = Battle01FirstRound.EnterNext(Battle01FirstRoundTests.FiveSurvivorBoundary(0));
        for (int i = 0; i < 2; i++) current = Battle01EnemyPursuit.CompleteNext(current,
            current.FirstRound!.CurrentCandidate!.Value.CombatantIndex, Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
        return Battle01NextPlayerControl.Enter(current, current.FirstRound!.CurrentCandidate!.Value.CombatantIndex).State!;
    }

    internal static Battle01InitializedState Selected() => Battle01PlayerHealing.SelectSpell(
        Battle01PlayerHealing.Begin(Battle01PlayerMovement.Confirm(Ready(), 1), 1), 1, 0);

    internal static Battle01InitializedState Completed() => Battle01PlayerHealing.Confirm(Selected(), 1,
        Battle01HealingCompletionPolicy.ControlledSarahHealOneBowie);

    [Fact]
    public void PlayerHealingReplaysTwoAwardRollsAndRetainsTheIndependentThinkingChannel()
    {
        var before = Selected(); string frozen = Json(before);
        var after = Battle01PlayerHealing.Confirm(before, 1, Battle01HealingCompletionPolicy.ControlledSarahHealOneBowie);
        var receipt = after.TurnCompletion!; var decision = receipt.PlayerHealing!;
        Assert.Equal(new[] { 0, 1 }, decision.LegalTargets); Assert.Equal(new byte[] { 255 }, decision.MoveString);
        Assert.Equal((9, 18, 17), (decision.Effect.Recovery, decision.Effect.AccumulatedExp, decision.Effect.AwardedExp));
        Assert.Equal(new[] {
            new Battle01MainRandomRoll("heal-exp-plus", 16, 0x74A71234, 0xEC821234, 14),
            new Battle01MainRandomRoll("heal-exp-minus", 16, 0xEC821234, 0x02A11234, 0) }, decision.Effect.Rolls);
        Assert.Equal(new[] { new Battle01HealingReaction(1, 0, -3, 0), new(0, 9, 0, 0), new(1, 0, 0, 17) }, decision.Effect.Reactions);
        Assert.Equal((1, 0, (byte)1, (ushort)0), (decision.ActorIndex, decision.TargetIndex, decision.Action, decision.ItemOrSpellWord));
        Assert.Equal(103, Battle01EnemyPursuitTests.Receipts(after).Count());
        Assert.Equal((13, 6), (after.FirstRound!.RoundNumber, after.FirstRound.CurrentTurnOffset));
        Assert.Null(after.FirstControl); Assert.Same(before.TurnCompletion, receipt.Previous);
        Assert.Equal((0x02A11234u, (ushort?)0x0234), (after.RandomSeedImage, after.RandomSeedCopy));
        Assert.Equal((ushort)12, after.Roster[0].Stats.HpCurrent);
        Assert.Equal(((byte)7, (byte?)17), (after.Roster[1].Stats.MpCurrent, after.Roster[1].Stats.CurrentExp));
        Assert.Equal(((uint?)0, (ushort?)0, (byte?)0, (ushort?)0, (ushort?)0, (ushort?)0, (byte?)0),
            Battle01EnemyStandby.RequireThinkingHistory(after));
        Assert.Equal(frozen, Json(before));
    }

    [Fact]
    public void PlayerHealingCancelsEveryLayerWithoutConsumingAnyChannel()
    {
        var ready = Ready(); var action = Battle01PlayerMovement.Confirm(ready, 1);
        var spell = Battle01PlayerHealing.Begin(action, 1); var target = Battle01PlayerHealing.SelectSpell(spell, 1, 0);
        Assert.Equal(Json(spell), Json(Battle01PlayerHealing.Cancel(target, 1)));
        Assert.Equal(Json(action), Json(Battle01PlayerHealing.Cancel(spell, 1)));
        Assert.Equal(Json(ready), Json(Battle01PlayerMovement.Cancel(action, 1)));
        var self = Battle01PlayerHealing.CycleTarget(target, 1, 1);
        Assert.Equal(1, self.FirstControl!.Movement.Healing!.TargetIndex);
        Assert.Equal("heal.targetUnsupported", Assert.Throws<ArgumentException>(() => Battle01PlayerHealing.Confirm(self, 1,
            Battle01HealingCompletionPolicy.ControlledSarahHealOneBowie)).ParamName);
        Assert.Equal(Json(target), Json(Battle01PlayerHealing.CycleTarget(self, 1, -1)));
    }

    [Fact]
    public void PlayerHealingRangeStartsAtTheConfirmedDestination()
    {
        var ready = Ready(); var moved = Battle01PlayerMovement.Confirm(Battle01PlayerMovement.SelectDestination(ready, 1, new(11, 15)), 1);
        var spell = Battle01PlayerHealing.Begin(moved, 1); var target = Battle01PlayerHealing.SelectSpell(spell, 1, 0);
        Assert.Equal(new[] { 1 }, target.FirstControl!.Movement.Healing!.Targets);
        Assert.Equal(new MapPosition(11, 15), target.Roster[1].Position);
        Assert.Throws<ArgumentException>(() => Battle01PlayerHealing.Confirm(target, 1, Battle01HealingCompletionPolicy.ControlledSarahHealOneBowie));
        Assert.Equal(Json(ready), Json(Battle01PlayerMovement.Cancel(Battle01PlayerHealing.Cancel(Battle01PlayerHealing.Cancel(target, 1), 1), 1)));
    }

    internal static string Json(object value) => JsonSerializer.Serialize(value, new JsonSerializerOptions { MaxDepth = 256 });
}
