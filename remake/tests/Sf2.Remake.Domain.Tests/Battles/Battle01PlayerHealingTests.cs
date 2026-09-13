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

    [Theory]
    [InlineData("unknownExp")][InlineData("wrongExp")][InlineData("class")][InlineData("level")]
    [InlineData("status")][InlineData("silence")][InlineData("curse")][InlineData("mp")]
    [InlineData("noSpell")][InlineData("spellLevel")][InlineData("slot")][InlineData("main")][InlineData("copy")]
    [InlineData("candidate")][InlineData("deadTarget")][InlineData("unplacedTarget")][InlineData("range")]
    [InlineData("reviveChester")][InlineData("place129")]
    public void PlayerHealingRejectsChangedInputsWithoutMutatingTheSelectedBattle(string mutation)
    {
        var source = Selected(); var roster = source.Roster.ToArray(); var actor = roster[1]; var s = actor.Stats;
        var spells = s.Spells.ToArray();
        if (mutation == "noSpell") spells[0] = 63;
        if (mutation == "spellLevel") spells[0] = 64;
        if (mutation == "slot") (spells[0], spells[1]) = (spells[1], spells[0]);
        if (mutation is "unknownExp" or "wrongExp" or "class" or "level" or "status" or "silence" or "curse" or "mp" or "noSpell" or "spellLevel" or "slot")
        {
            var stats = new Battle01Stats(mutation == "level" ? (byte)2 : s.Level, s.HpMax, s.HpCurrent, s.MpMax,
                mutation == "mp" ? (byte)2 : s.MpCurrent, s.Attack, s.Defense, s.Agility, s.Move,
                mutation is "status" or "silence" or "curse" ? (ushort)(mutation == "status" ? 1 : mutation == "silence" ? 0x400 : 0x8000) : s.Status,
                s.Items, spells, mutation == "unknownExp" ? null : mutation == "wrongExp" ? (byte)1 : s.CurrentExp, s.CurrentKills, s.CurrentDefeats);
            roster[1] = new(actor.Deployment, stats, mutation == "class" ? (byte)5 : actor.ClassId, actor.EnemySource, actor.AiBitfield, actor.Position);
        }
        if (mutation == "deadTarget") roster[0] = roster[0].WithStats(roster[0].Stats.WithCurrentHp(0));
        if (mutation == "unplacedTarget") roster[0] = roster[0].WithPosition(null);
        if (mutation == "range") roster[0] = roster[0].WithPosition(new(12, 13));
        if (mutation == "reviveChester") roster[2] = roster[2].WithStats(roster[2].Stats.WithCurrentHp(1));
        if (mutation == "place129") roster[4] = roster[4].WithPosition(new(9, 5));
        var order = mutation == "candidate" ? source.FirstRound!.AdvanceCompletedPlayerTurn() : source.FirstRound!;
        var changed = Battle01FirstRoundTests.CopyCurrent(source, roster: roster, order: order,
            mainImage: mutation == "main" ? 0x74A81234u : null, copy: mutation == "copy" ? (ushort)0x0235 : null);
        var input = new Battle01InitializedState(changed, roster, changed.Occupancy, source.FirstControl!);
        string frozen = Json(input);
        Assert.ThrowsAny<ArgumentException>(() => Battle01PlayerHealing.Confirm(input, 1, Battle01HealingCompletionPolicy.ControlledSarahHealOneBowie));
        Assert.Equal(frozen, Json(input));
    }

    [Theory]
    [InlineData("enemy")][InlineData("duplicate")][InlineData("missing")][InlineData("outOfBounds")][InlineData("spell")]
    public void PlayerHealingRejectsForgedTargetChoices(string mutation)
    {
        var source = Selected(); var c = source.FirstControl!; var m = c.Movement;
        int[] targets = mutation switch { "enemy" => [0, 1, 128], "duplicate" => [0, 0, 1], "missing" => [0], _ => [0, 1] };
        var selected = new Battle01PlayerHealingSelection(mutation == "spell" ? (byte)1 : (byte)0, targets, mutation == "outOfBounds" ? 2 : 0);
        var input = new Battle01InitializedState(source, source.Roster.ToArray(), source.Occupancy,
            c.WithMovement(new(m.Range, m.Preview, m.Stage, healing: selected)));
        string frozen = Json(input);
        Assert.Throws<ArgumentException>(() => Battle01PlayerHealing.Confirm(input, 1, Battle01HealingCompletionPolicy.ControlledSarahHealOneBowie));
        Assert.Equal(frozen, Json(input));
    }

    [Theory]
    [InlineData("noPolicy")][InlineData("wrongActor")][InlineData("wrongPhase")][InlineData("wrongSpell")][InlineData("direction")]
    public void PlayerHealingRequiresTheNamedOperationAtTheCurrentLayer(string mutation)
    {
        var source = Selected(); string frozen = Json(source);
        Assert.Throws<ArgumentException>(() => mutation switch
        {
            "noPolicy" => Battle01PlayerHealing.Confirm(source, 1, null),
            "wrongActor" => Battle01PlayerHealing.Confirm(source, 0, Battle01HealingCompletionPolicy.ControlledSarahHealOneBowie),
            "wrongPhase" => Battle01PlayerHealing.Begin(source, 1),
            "wrongSpell" => Battle01PlayerHealing.SelectSpell(Battle01PlayerHealing.Cancel(source, 1), 1, 64),
            _ => Battle01PlayerHealing.CycleTarget(source, 1, 0),
        });
        Assert.Equal(frozen, Json(source));
    }

    [Theory]
    [InlineData("unknownExp")][InlineData("levelUp")][InlineData("mpUnderflow")][InlineData("fullHp")][InlineData("dead")]
    public void PlayerHealingScalarRefusesUnconsumedAwardAndRecoveryBranches(string mutation)
    {
        var actor = new Battle01Stats(1, 11, 11, 10, mutation == "mpUnderflow" ? (byte)2 : (byte)10, 9, 5, 5, 5, 0,
            [213,0,0,127], [0,63,63,63], mutation == "unknownExp" ? null : mutation == "levelUp" ? (byte)83 : (byte)0);
        var target = new Battle01Stats(1, 12, mutation == "fullHp" ? (ushort)12 : mutation == "dead" ? (ushort)0 : (ushort)3,
            8, 8, 9, 4, 4, 6, 0, [199,0,127,127], [10,63,63,63], 63, 2, 0);
        Assert.Throws<ArgumentException>(() => Battle01PlayerHealing.Resolve(actor, target, 0x74A71234));
    }

    [Theory]
    [InlineData("missingHeal")][InlineData("duplicateHeal")][InlineData("stayLabel")][InlineData("deadLabel")]
    [InlineData("missing94")][InlineData("missing95")][InlineData("missing97")][InlineData("duplicate95")]
    [InlineData("beforeHp")][InlineData("beforeMp")][InlineData("hpOverCap")][InlineData("afterMp")][InlineData("exp")]
    [InlineData("rollOrder")][InlineData("main")][InlineData("copy")][InlineData("reactionOrder")]
    public void PlayerHealingHistoryRejectsCorruptedEffectsAndOldReceiptLinks(string mutation)
    {
        var source = Completed(); var r = source.TurnCompletion!; var d = r.PlayerHealing!; var effect = d.Effect;
        if (mutation == "missingHeal") r = r with { PlayerHealing = null, Policy = Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats };
        if (mutation == "duplicateHeal") r = r with { Previous = r };
        if (mutation == "stayLabel") r = r with { Policy = Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats };
        if (mutation == "deadLabel") r = r with { Policy = Battle01DefeatedTurnCompletionPolicy.ControlledEnemy129AfterChesterDefeat };
        if (mutation.StartsWith("missing", StringComparison.Ordinal) && int.TryParse(mutation[7..], out int missing))
            r = Rewrite(r, 103, missing, old => old.Previous!);
        if (mutation == "duplicate95") r = Rewrite(r, 103, 95, old => old with { Previous = old });
        if (mutation == "beforeHp") d = d with { Target = d.Target.WithStats(d.Target.Stats.WithCurrentHp(4)) };
        if (mutation == "beforeMp") d = d with { Actor = d.Actor.WithStats(d.Actor.Stats.WithCurrentMp(9)) };
        if (mutation == "hpOverCap") effect = effect with { Recovery = 15 };
        if (mutation == "afterMp") effect = effect with { ActorAfterStats = effect.ActorAfterStats.WithCurrentMp(6) };
        if (mutation == "exp") effect = effect with { AwardedExp = 18 };
        if (mutation == "rollOrder") effect = effect with { Rolls = effect.Rolls.Reverse().ToArray() };
        if (mutation == "main") d = d with { MainSeedBefore = 0x74A81234 };
        if (mutation == "copy") d = d with { SeedCopy = 0x0235 };
        if (mutation == "reactionOrder") effect = effect with { Reactions = effect.Reactions.Reverse().ToArray() };
        if (mutation is "beforeHp" or "beforeMp" or "hpOverCap" or "afterMp" or "exp" or "rollOrder" or "main" or "copy" or "reactionOrder")
            r = r with { PlayerHealing = d with { Effect = effect } };
        var input = Battle01FirstRoundTests.CopyCurrent(source, receipt: r); string frozen = Json(input);
        Assert.ThrowsAny<ArgumentException>(() => { Battle01FirstRound.RequireCurrentPrefix(input); Battle01EnemyStandby.RequireThinkingHistory(input); });
        Assert.Equal(frozen, Json(input));
        static Battle01TurnCompletionReceipt Rewrite(Battle01TurnCompletionReceipt receipt, int count, int target,
            Func<Battle01TurnCompletionReceipt, Battle01TurnCompletionReceipt> change) => count == target ? change(receipt) :
                receipt with { Previous = Rewrite(receipt.Previous!, count - 1, target, change) };
    }
}
