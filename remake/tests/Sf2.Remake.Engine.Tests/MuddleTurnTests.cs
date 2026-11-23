using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Content.Persistence;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Xunit;
using static Sf2.Remake.Engine.Tests.SilenceSpellTests;

namespace Sf2.Remake.Engine.Tests;

public sealed class MuddleTurnTests
{
    [Fact]
    public void SkippedSleepingEntryExpiresMuddleBeforeSilenceAndReturnsToTheSameActorsSecondEntry()
    {
        var battle = Status(Battle(), Mage, 0x158).With(mainSeed: 0,
            queue: [new(Mage, 30), new(Mage, 24), new(Priest, 20), new(null, 255)], cursor: 0);
        var skipped = Advance(battle);
        Assert.Contains(skipped.Observations, e => e.Kind == "sleeping-action-skipped" && e.Actor == Mage);
        Assert.Equal(BattleScenePhase.SleepTurnMessage, skipped.Snapshot.BattleScene!.Phase);
        var asleep = Assert.Single(skipped.Observations, e => e.Kind == "rng-sleep-wake");
        var clearedSleep = Complete(skipped);
        var muddle = clearedSleep.Snapshot.BattleScene!;
        Assert.Equal(BattleScenePhase.MuddleTurnMessage, muddle.Phase);
        Assert.Equal((ushort)0x118, clearedSleep.Snapshot.Battle.GetActor(Mage).Status);
        Assert.Equal("355", Assert.Single(clearedSleep.Observations, e => e.Kind == "muddle-turn-message").Detail);
        var recovery = Assert.Single(clearedSleep.Observations, e => e.Kind == "rng-muddle-expiry");
        Assert.Equal((ushort?)16, recovery.RandomRange);
        Assert.Equal(asleep.After, recovery.Before);

        var clearedMuddle = Complete(clearedSleep);
        Assert.Equal((ushort)0x100, clearedMuddle.Snapshot.Battle.GetActor(Mage).Status);
        Assert.Equal(BattleScenePhase.SilenceTurnMessage, clearedMuddle.Snapshot.BattleScene!.Phase);
        Assert.Equal(recovery.After, Assert.Single(clearedMuddle.Observations, e => e.Kind == "rng-silence-expiry").Before);
        var stale = BattleSceneContinuation.Submit(clearedMuddle.Snapshot,
            new CompletePresentation(muddle.Token, muddle.CompletionKind), RuleCompositions.Sf2());
        Assert.NotNull(stale.Failure);
        Assert.Same(clearedMuddle.Snapshot, stale.Snapshot);

        var next = Complete(clearedMuddle);
        Assert.Equal(SessionStopReason.PlayerInput, next.StopReason);
        Assert.Equal(Mage, next.Snapshot.Selection!.Actor);
        Assert.Equal(1, next.Snapshot.Battle.Cursor);
        Assert.Equal((ushort)0, next.Snapshot.Battle.GetActor(Mage).Status);
        Assert.Equal(battle.ThinkingSeed, next.Snapshot.Battle.ThinkingSeed);
    }

    [Fact]
    public void DecrementPreservesConfusionOnASkippedEntryWithoutAnExpiryMessage()
    {
        var battle = Status(Battle(), Mage, 0xF8).With(mainSeed: 1000u << 16,
            queue: [new(Mage, 30), new(Priest, 20), new(null, 255)], cursor: 0);
        var skipped = Advance(battle);
        Assert.Equal(BattleScenePhase.SleepTurnMessage, skipped.Snapshot.BattleScene!.Phase);
        var next = Complete(skipped);
        Assert.Equal(Priest, next.Snapshot.Selection!.Actor);
        Assert.Equal((ushort)0x28, next.Snapshot.Battle.GetActor(Mage).Status);
        var roll = Assert.Single(next.Observations, e => e.Kind == "rng-muddle-expiry");
        Assert.Equal((ushort?)48, roll.RandomRange);
        Assert.Equal((ushort?)27, roll.RandomValue);
        Assert.DoesNotContain(next.Observations, e => e.Kind == "muddle-turn-message");
    }

    [Fact]
    public void QueueOnlyEnemyConsumesItsOwnMuddleCountOnBothEntriesWithoutAScene()
    {
        var battle = Battle(d => d["encounters"]![0]!["placements"]![2]!["aiStrategy"] = "stay");
        battle = Status(battle, Smoke, 0x30).With(mainSeed: 4500u << 16,
            queue: [new(Smoke, 30), new(Smoke, 24), new(Priest, 20), new(null, 255)], cursor: 0);
        var result = Advance(battle);
        Assert.Equal(SessionStopReason.PlayerInput, result.StopReason);
        Assert.Equal(Priest, result.Snapshot.Selection!.Actor);
        Assert.Equal(2, result.Snapshot.Battle.Cursor);
        Assert.Equal((ushort)0x10, result.Snapshot.Battle.GetActor(Smoke).Status);
        Assert.Equal(battle.ThinkingSeed, result.Snapshot.Battle.ThinkingSeed);
        var draws = result.Observations.Where(e => e.Kind == "rng-muddle-expiry").ToArray();
        Assert.Equal(new ushort?[] { 48, 32 }, draws.Select(e => e.RandomRange));
        Assert.Equal(new ushort?[] { 42, 19 }, draws.Select(e => e.RandomValue));
        Assert.Equal(draws[0].After, draws[1].Before);
        Assert.DoesNotContain(result.Observations, e => e.Kind == "muddle-turn-message");
    }

    [Theory]
    [InlineData(true, 0x30, 0)]
    [InlineData(false, 0x38, 0)]
    [InlineData(true, 0x10, 0x40)]
    [InlineData(false, 0x28, 1)]
    public void PhysicalImpactUsesMuddledAttackerDodgeRangeUnlessTargetIsDisabled(bool allyAttacks, int status, int targetStatus)
    {
        var battle = Battle(d => d["terrains"]![0]!["rows"]![2] = "pppppppppp");
        var attacker = allyAttacks ? Mage : Smoke;
        var defender = allyAttacks ? Smoke : Mage;
        battle = Status(Status(battle, attacker, (ushort)status), defender, (ushort)targetStatus).With(mainSeed: 0);
        var cast = Sf2PhysicalAction.Prepare(battle, attacker, new(4, 2), defender, new Sf2BattleProgressionRule());
        var dodge = cast.ConstructionEffects.Where(e => e.Kind == "rng-dodge" && e.Actor == attacker);
        if (targetStatus == 0)
        {
            var rolls = dodge.ToArray();
            Assert.NotEmpty(rolls);
            Assert.All(rolls, roll => Assert.Equal((ushort?)2, roll.RandomRange));
            Assert.Equal((ushort?)0, rolls[0].RandomValue);
            Assert.Equal(BattleReactionKind.Dodge, cast.Reactions[0].Kind);
        }
        else
        {
            Assert.Empty(dodge);
            Assert.Equal(BattleReactionKind.Damage, cast.Reactions[0].Kind);
        }
        Assert.Equal((ushort)status, cast.Prepared.GetActor(attacker).Status);
    }

    [Theory]
    [InlineData(0x02, true)]
    [InlineData(0x08, false)]
    [InlineData(0x10, false)]
    [InlineData(0x38, false)]
    public void DirectAuthoredStartAdmitsOtherWordsAndRejectsMuddle(int status, bool admitted)
    {
        var input = EngineTestContent.Admitted("jaro-conversion");
        var enemy = new ActorRef("enemy-jaro");
        var actors = input.Start.Actors.Select(a => a.Actor == enemy ? a with { Status = (ushort)status } : a);
        var start = input.Start.WithResources(actors, input.Start.Gold);

        var result = GameSession.Start(input.Definition, start);
        if (admitted)
        {
            var started = Assert.IsType<SessionStarted>(result);
            Assert.Null(started.Result.Failure);
            Assert.Equal((ushort)status, started.Session.Current.Battle.GetActor(enemy).Status);
        }
        else
        {
            var failure = Assert.IsType<SessionStartFailed>(result).Failure;
            Assert.Equal(SessionFailureKind.UnsupportedCapability, failure.Kind);
            Assert.Equal("actor-status", failure.Code);
        }
    }

    [Theory]
    [InlineData(0x08)]
    [InlineData(0x10)]
    [InlineData(0x38)]
    public void PendingMuddleWordsRemainOutsidePublicSaveAndRestore(int status)
    {
        var (definition, current) = BattleSaveFixture.Create();
        var staged = BattleSaveFixture.At(Status(current.Battle, new("enemy"), (ushort)status), current.Story);
        var rules = RuleCompositions.Sf2();
        var capture = BattleSave.Capture(definition, staged, rules, staged.SessionId, staged.Revision);
        Assert.Equal("save-battle-actor-range", capture.Error);

        var legacy = BattleSaveFixture.Capture(definition, current);
        var edited = legacy with { Battle = legacy.Battle with { Actors = legacy.Battle.Actors.Select(a =>
            a.Actor == "enemy" ? a with { Status = (ushort)status } : a).ToArray() } };
        var wire = OpeningSaveJson.ReadRecord(OpeningSaveJson.WriteRecord(new BattleSaveRecord(1, 1, DateTimeOffset.UnixEpoch, edited)), 1);
        Assert.Equal("save-battle-actor-range", Assert.IsType<SessionStartFailed>(SaveRecordRestore.Restore(definition, wire, rules)).Failure.Code);
    }

    [Theory]
    [InlineData(true, 0x38)]
    [InlineData(false, 0x38)]
    public void UnimplementedMuddledAutomaticEntriesRefuseWithoutPublishingTurnWork(bool player, int status)
    {
        var actor = player ? Mage : Smoke;
        var battle = Status(Battle(), actor, (ushort)status).With(queue: [new(actor, 30), new(Priest, 20), new(null, 255)], cursor: 0);
        var result = Advance(battle);
        Assert.Equal("muddle-automatic-turn", result.Failure!.Code);
        Assert.Equal(SessionStopReason.Unsupported, result.StopReason);
        Assert.Same(battle, result.Snapshot.Battle);
        Assert.Empty(result.Observations);
    }

    private static SessionResult Advance(EngineBattleState battle) =>
        BattleAdvancer.Advance(new SessionSnapshot(Guid.NewGuid(), 0, 0, new ActiveBattle(battle, null),
            new StoryState(), SessionStopReason.SimulationWait), [], RuleCompositions.Sf2());

    private static SessionResult Complete(SessionResult previous)
    {
        var scene = previous.Snapshot.BattleScene!;
        var result = BattleSceneContinuation.Submit(previous.Snapshot,
            new CompletePresentation(scene.Token, scene.CompletionKind), RuleCompositions.Sf2());
        Assert.Null(result.Failure);
        return result;
    }
}
