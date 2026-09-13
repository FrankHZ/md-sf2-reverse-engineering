using System.Text.Json;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Battles;
using Xunit;

namespace Sf2.Remake.Application.Tests;

public sealed class PrivateOriginalBattle01PlayerHealingTests
{
    internal static GameSession Ready()
    {
        var session = PrivateOriginalBattle01FirstRoundTests.FiveSurvivorSession(OriginalBattle01ControlledPartyPreset.SarahHealComparison);
        Assert.IsType<PrivateOriginalBattle01FirstRoundEntered>(session.EnterPrivateOriginalBattle01NextRound(session.PrivateOriginalBattle01));
        for (int i = 0; i < 2; i++) Assert.IsType<PrivateOriginalBattle01EnemyPursuitCompleted>(session.CompletePrivateOriginalBattle01EnemyPursuit(
            session.PrivateOriginalBattle01, session.PrivateOriginalBattle01!.Battle.FirstRound!.CurrentCandidate!.Value.CombatantIndex));
        Assert.IsType<PrivateOriginalBattle01NextPlayerControlEntered>(session.EnterPrivateOriginalBattle01NextPlayerControl(session.PrivateOriginalBattle01, 1));
        return session;
    }

    internal static GameSession Selected()
    {
        var session = Ready();
        Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.ConfirmPrivateOriginalBattle01PlayerMovement(session.PrivateOriginalBattle01, 1));
        Assert.IsType<PrivateOriginalBattle01PlayerHealingApplied>(session.BeginPrivateOriginalBattle01PlayerHealing(session.PrivateOriginalBattle01, 1));
        Assert.IsType<PrivateOriginalBattle01PlayerHealingApplied>(session.SelectPrivateOriginalBattle01HealingSpell(session.PrivateOriginalBattle01, 1, 0));
        return session;
    }

    [Fact]
    public void PlayerHealingPublishesOneCompleteSnapshotAndRejectsStaleOrCopiedRequests()
    {
        var session = Selected(); var before = session.PrivateOriginalBattle01!; string frozen = Json(before);
        foreach (var invalid in new PrivateOriginalBattle01SessionSnapshot?[] { null,
            new(before.Preparation, before.Battle, before.SourceLocomotion, before.SourceBridge), Selected().PrivateOriginalBattle01 })
        {
            Assert.Equal("snapshot", Assert.IsType<PrivateOriginalBattle01PlayerHealingRejected>(session.ConfirmPrivateOriginalBattle01PlayerHealing(invalid, 1)).Diagnostic.Field);
            Assert.Same(before, session.PrivateOriginalBattle01); Assert.Equal(frozen, Json(before));
        }
        var local = Battle01PlayerHealing.Confirm(before.Battle, 1, Battle01HealingCompletionPolicy.ControlledSarahHealOneBowie);
        var after = Assert.IsType<PrivateOriginalBattle01PlayerHealingApplied>(session.ConfirmPrivateOriginalBattle01PlayerHealing(before, 1)).Snapshot;
        Assert.Equal(Json(local), Json(after.Battle)); Assert.NotSame(before, after);
        Assert.Same(before.Preparation, after.Preparation); Assert.Same(before.SourceLocomotion, after.SourceLocomotion); Assert.Same(before.SourceBridge, after.SourceBridge);
        Assert.Equal(frozen, Json(before));
        Assert.Equal("snapshot", Assert.IsType<PrivateOriginalBattle01PlayerHealingRejected>(session.ConfirmPrivateOriginalBattle01PlayerHealing(before, 1)).Diagnostic.Field);
        Assert.IsType<PrivateOriginalBattle01PlayerHealingRejected>(session.ConfirmPrivateOriginalBattle01PlayerHealing(after, 1));
        Assert.Same(after, session.PrivateOriginalBattle01);
    }

    internal static string Json(object value) => JsonSerializer.Serialize(value, new JsonSerializerOptions { MaxDepth = 256 });

    [Theory]
    [InlineData("gold")][InlineData("bowieKills")][InlineData("chesterExp")][InlineData("chesterDefeats")]
    [InlineData("bowieDefeats")][InlineData("chesterKills")][InlineData("sarahExp")][InlineData("unknownSarahExp")][InlineData("oldPreset")]
    public void PlayerHealingAuthenticatesAllSevenRetainedInputs(string mutation)
    {
        var session = Selected(); var source = session.PrivateOriginalBattle01!; var party = source.Preparation.Party;
        var allies = party.Allies.Select(a => new OriginalBattle01ControlledAlly(a.Id, a.ClassId, a.Level, a.HpMax, a.HpCurrent,
            a.MpMax, a.MpCurrent, a.EffectiveAttack, a.EffectiveDefense, a.EffectiveAgility, a.EffectiveMove, a.StatusEffects, a.Items, a.Spells,
            currentExp: mutation == "unknownSarahExp" && a.Id == 1 ? null :
                (mutation == "chesterExp" && a.Id == 2) || (mutation == "sarahExp" && a.Id == 1) ? (byte)1 : a.CurrentExp,
            currentKills: (mutation == "bowieKills" && a.Id == 0) || (mutation == "chesterKills" && a.Id == 2) ? (ushort)1 : a.CurrentKills,
            currentDefeats: (mutation == "bowieDefeats" && a.Id == 0) || (mutation == "chesterDefeats" && a.Id == 2) ? (ushort)1 : a.CurrentDefeats));
        var changed = mutation == "oldPreset" ? OriginalBattle01ControlledPartyPreset.ChesterFirstKillComparison :
            new OriginalBattle01ControlledPartyPreset(party.Id, party.RandomSeed, party.Difficulty, allies, party.RandomSeedCopy,
                mutation == "gold" ? 1u : party.CurrentGold);
        var input = new PrivateOriginalBattle01SessionSnapshot(new(source.Preparation.Pending, source.Preparation.Inputs, changed),
            source.Battle, source.SourceLocomotion, source.SourceBridge);
        PublishTestCopy(session, input); string frozen = Json(input);
        Assert.IsType<PrivateOriginalBattle01PlayerHealingRejected>(session.ConfirmPrivateOriginalBattle01PlayerHealing(input, 1));
        Assert.Same(input, session.PrivateOriginalBattle01); Assert.Equal(frozen, Json(input));
    }

    [Fact]
    public void PlayerHealingLateAccountingFailurePublishesNoneOfTheSuccessfulLocalCast()
    {
        var session = Selected(); var before = session.PrivateOriginalBattle01!;
        // Negative-only: erase one retained accounting origin consistently. Local Domain history
        // can replay unknown defeats, but the authentic early preparation still requires Bowie0.
        var stats = new HashSet<Battle01Stats> { before.Battle.Roster[0].Stats };
        for (var r = before.Battle.TurnCompletion; r is not null; r = r.Previous)
        {
            if (r.PlayerPhysicalAttack is { ActorIndex: 0 } p) { stats.Add(p.Actor.Stats); stats.Add(p.ActorAfterStats); }
            if (r.EnemyPhysicalAttack is { TargetIndex: 0 } e)
            { stats.Add(e.Priorities.Single(p => p.Target.Index == 0).Target.Stats); stats.Add(e.Effect.BeforeStats); stats.Add(e.Effect.AfterStats); }
        }
        var field = typeof(Battle01Stats).GetField("<CurrentDefeats>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        foreach (var value in stats) field.SetValue(value, null);
        string frozen = Json(before);
        var local = Battle01PlayerHealing.Confirm(before.Battle, 1, Battle01HealingCompletionPolicy.ControlledSarahHealOneBowie);
        Assert.Equal((byte)7, local.Roster[1].Stats.MpCurrent); Assert.Equal((ushort)12, local.Roster[0].Stats.HpCurrent);
        Assert.Equal("accounting.input", Assert.IsType<PrivateOriginalBattle01PlayerHealingRejected>(session.ConfirmPrivateOriginalBattle01PlayerHealing(before, 1)).Diagnostic.Field);
        Assert.Same(before, session.PrivateOriginalBattle01); Assert.Equal(frozen, Json(before));
    }

    [Fact]
    public void PlayerHealingBindsTheNewPreparationBeforeTheOldHistoryIsPlayed()
    {
        var session = PrivateOriginalBattle01FirstRoundTests.FiveSurvivorSession();
        Assert.IsType<PrivateOriginalBattle01FirstRoundEntered>(session.EnterPrivateOriginalBattle01NextRound(session.PrivateOriginalBattle01));
        foreach (int actor in new[] { 128, 130 }) Assert.IsType<PrivateOriginalBattle01EnemyPursuitCompleted>(session.CompletePrivateOriginalBattle01EnemyPursuit(session.PrivateOriginalBattle01, actor));
        Assert.IsType<PrivateOriginalBattle01NextPlayerControlEntered>(session.EnterPrivateOriginalBattle01NextPlayerControl(session.PrivateOriginalBattle01, 1));
        Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.ConfirmPrivateOriginalBattle01PlayerMovement(session.PrivateOriginalBattle01, 1));
        var source = session.PrivateOriginalBattle01!;
        var input = new PrivateOriginalBattle01SessionSnapshot(new(source.Preparation.Pending, source.Preparation.Inputs,
            OriginalBattle01ControlledPartyPreset.SarahHealComparison), source.Battle, source.SourceLocomotion, source.SourceBridge);
        PublishTestCopy(session, input); string frozen = Json(input);
        Assert.IsType<PrivateOriginalBattle01PlayerHealingRejected>(session.BeginPrivateOriginalBattle01PlayerHealing(input, 1));
        Assert.Same(input, session.PrivateOriginalBattle01); Assert.Equal(frozen, Json(input));
    }

    [Theory]
    [InlineData(PrivateOriginalBattle01PlayerHealingOperation.Begin)]
    [InlineData(PrivateOriginalBattle01PlayerHealingOperation.SelectSpell)]
    [InlineData(PrivateOriginalBattle01PlayerHealingOperation.CycleTarget)]
    [InlineData(PrivateOriginalBattle01PlayerHealingOperation.Cancel)]
    public void PlayerHealingEveryMenuOperationAuthenticatesTheWholeCurrentSnapshot(PrivateOriginalBattle01PlayerHealingOperation operation)
    {
        var session = Selected(); var before = session.PrivateOriginalBattle01!; string frozen = Json(before);
        var result = operation switch
        {
            PrivateOriginalBattle01PlayerHealingOperation.Begin => session.BeginPrivateOriginalBattle01PlayerHealing(null, 1),
            PrivateOriginalBattle01PlayerHealingOperation.SelectSpell => session.SelectPrivateOriginalBattle01HealingSpell(null, 1, 0),
            PrivateOriginalBattle01PlayerHealingOperation.CycleTarget => session.CyclePrivateOriginalBattle01HealingTarget(null, 1, 1),
            _ => session.CancelPrivateOriginalBattle01PlayerHealing(null, 1),
        };
        Assert.Equal("snapshot", Assert.IsType<PrivateOriginalBattle01PlayerHealingRejected>(result).Diagnostic.Field);
        Assert.Same(before, session.PrivateOriginalBattle01); Assert.Equal(frozen, Json(before));
    }

    private static void PublishTestCopy(GameSession session, PrivateOriginalBattle01SessionSnapshot input) =>
        typeof(GameSession).GetProperty(nameof(GameSession.PrivateOriginalBattle01))!.SetValue(session, input);
}
