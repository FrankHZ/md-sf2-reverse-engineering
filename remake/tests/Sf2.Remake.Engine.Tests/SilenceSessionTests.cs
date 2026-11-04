using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;
using static Sf2.Remake.Engine.Tests.SilenceSpellTests;

namespace Sf2.Remake.Engine.Tests;

public sealed class SilenceSessionTests
{
    [Fact]
    public void SilencedSelectionCancelAndBadTargetAreFreeButConfirmedSpellPaysExactlyOnce()
    {
        var session = Start("silence-watch", d => { d["start"]!["actors"]![0]!["status"] = "silence3"; d["start"]!["actors"]![2]!["mp"] = 0; });
        var initial = session.Current.Battle;
        Accept(session, new Confirm());
        var offered = session.QueryBattleChoices().Actions.Single(a => a.Action.Spell == new SpellRef("blaze", 1));
        Assert.True(offered.Enabled); // Silence is not an unavailable menu item.
        Assert.Equal(initial.MainSeed, session.Current.Battle.MainSeed);
        Accept(session, new SelectSpell(new("blaze", 1))); Accept(session, new Cancel());
        Assert.Same(initial, session.Current.Battle);
        Accept(session, new Confirm());
        Accept(session, new SelectSpell(new("blaze", 1)));
        var bad = session.Current; var reject = Send(session, new SelectTarget(Mage));
        Assert.NotNull(reject.Failure); Assert.Same(bad, session.Current);
        Accept(session, new SelectTarget(Smoke)); var committed = Accept(session, new Confirm());
        Assert.Equal(20, session.Current.Battle.GetActor(Mage).Mp);
        var prepared = session.Current.Battle;
        while (session.Current.BattleScene!.Phase != BattleScenePhase.SpellCost)
        {
            var scene = session.Current.BattleScene;
            committed = Accept(session, scene.RequiresAcknowledgement ? new Acknowledge(scene.Token) : new CompletePresentation(scene.Token, scene.CompletionKind));
        }
        Assert.Equal(18, session.Current.Battle.GetActor(Mage).Mp); Assert.Equal(80, session.Current.Battle.GetActor(Smoke).Hp);
        var paid = session.Current;
        reject = Send(session, new Acknowledge(session.Current.BattleScene!.Token));
        Assert.NotNull(reject.Failure); Assert.Same(paid, session.Current);
        var done = FinishBattleScenes(session, committed);
        Assert.Equal(18, session.Current.Battle.GetActor(Mage).Mp); Assert.Equal(80, session.Current.Battle.GetActor(Smoke).Hp);
        Assert.Equal((byte?)0, session.Current.Battle.GetActor(Mage).Exp);
        Assert.DoesNotContain(done.Observations, e => e.Kind.StartsWith("rng-spell") || e.Kind == "exp");
        Assert.True(session.Current.HasBattleControl);
    }

    [Theory]
    [InlineData(0x100, 0u, 0)] [InlineData(0x200, 0u, 0)] [InlineData(0x300, 0u, 0)]
    [InlineData(0x200, 0x12340000u, 0x100)] [InlineData(0x300, 0x12340000u, 0x200)]
    public void SilenceLifecycleFollowsSourceEarlyExpiryOrOneUnitContinuation(int before, uint seed, int after)
    {
        var battle = Status(Battle().With(mainSeed: seed), Mage, (ushort)before);
        var result = SilenceRules.AfterTurn(battle.GetActor(Mage), seed)!;
        Assert.Equal(after, result.After); Assert.Equal(before, result.Draw.Range);
    }

    [Theory]
    [InlineData(30)] [InlineData(1)]
    public void SleepSilencePoisonComposeInOrderAndConsumeOneEntry(int hp)
    {
        var session = Start("silence-watch", d =>
        {
            d["battlePoison"] = "fixed-two-after-turn"; d["start"]!["firstAlly"] = "mage";
            d["encounters"]![0]!["outcome"] = new JsonObject { ["leader"] = "priest", ["defeatedGateEnemy"] = "smoke" };
            d["start"]!["actors"]![0]!["status"] = "sleep1-silence1-poison";
            d["start"]!["actors"]![0]!["hp"] = hp; d["start"]!["actors"]![2]!["mp"] = 0;
        });
        Assert.True(session.Current.BattleScene!.IsSleepTurn);
        int cursor = session.Current.Battle.Cursor;
        var sleep = session.Current.BattleScene; Accept(session, new CompletePresentation(sleep.Token, sleep.CompletionKind));
        Assert.True(session.Current.BattleScene!.IsSilenceTurn); Assert.Equal(0x102, session.Current.Battle.GetActor(Mage).Status);
        var silence = session.Current.BattleScene; Accept(session, new CompletePresentation(silence.Token, silence.CompletionKind));
        Assert.True(session.Current.BattleScene!.IsPoisonTurn); Assert.Equal(2, session.Current.Battle.GetActor(Mage).Status);
        Assert.Equal(cursor, session.Current.Battle.Cursor);
        var done = FinishBattleScenes(session, new(session.Current, [], session.Current.StopReason));
        Assert.Equal(Math.Max(0, hp - 2), session.Current.Battle.GetActor(Mage).Hp);
        Assert.Equal(cursor + 1, session.Current.Battle.Cursor);
        Assert.Single(done.Observations, e => e.Kind == "after-turn" && e.Actor == Mage);
        Assert.True(session.Current.HasBattleControl);
    }

    [Fact]
    public void AutomaticSupportPublishesAllRecipientsAndExtraActionsRemainSeparateQueueEntries()
    {
        var session = Start("silence-watch"); Stay(session);
        var done = FinishBattleScenes(session, Stay(session));
        Assert.Contains(done.Observations, e => e.Kind == "ai-command-support" && e.After == 0);
        Assert.Contains(done.Observations, e => e.Kind == "support-spell");
        Assert.Equal(0, session.Current.Battle.GetActor(Smoke).Mp);
        Assert.Equal(2, session.Current.Battle.Queue.Count(e => e.Actor == Smoke));
        Assert.True(session.Current.HasBattleControl);
    }

    [Theory]
    [InlineData("silence4")] [InlineData("muddle")] [InlineData("silence1-silence2")]
    public void UnsupportedOrAmbiguousStartStatusRejectsBeforeSession(string status)
    {
        var document = Document("silence-watch"); document["start"]!["actors"]![0]!["status"] = status;
        Assert.IsType<ScenarioReadRejected>(Reader(document).Read());
    }

    [Fact]
    public void PhysicalSilenceMessageCompletesItsStatusWriteAtTheOrdinaryDeliveryBoundary()
    {
        var battle = Battle();
        battle = battle.With(mainSeed: 388u << 16, actors: battle.Actors.Select(a => a.Actor == Smoke ? a.With(position: new(4, 2)) : a));
        var action = PhysicalBattleAction.PrepareSourceDefault(battle, Smoke, new(4, 2), Mage);
        Assert.NotNull(action.Reactions[0].InflictedSilence);
        var snapshot = new SessionSnapshot(Guid.NewGuid(), 0, 0, new ActiveBattle(battle, null),
            new Sf2.Remake.Application.Runtime.Exploration.StoryState([]), SessionStopReason.SimulationWait);
        var result = BattleSceneContinuation.Begin(snapshot, action, []);
        for (int i = 0; i < 15 && result.Snapshot.BattleScene!.Phase != BattleScenePhase.SilenceMessage; i++)
        {
            var scene = result.Snapshot.BattleScene!;
            result = BattleSceneContinuation.Submit(result.Snapshot, scene.RequiresAcknowledgement ? new Acknowledge(scene.Token) :
                new CompletePresentation(scene.Token, scene.CompletionKind));
            Assert.Null(result.Failure);
        }
        var notice = result.Snapshot.BattleScene!; Assert.Equal(BattleScenePhase.SilenceMessage, notice.Phase);
        Assert.Equal(0, result.Snapshot.Battle.GetActor(Mage).Status); Assert.False(notice.RequiresAcknowledgement);
        result = BattleSceneContinuation.Submit(result.Snapshot, new CompletePresentation(notice.Token, notice.CompletionKind));
        Assert.Null(result.Failure); Assert.Equal(0x300, result.Snapshot.Battle.GetActor(Mage).Status);
    }

    [Fact]
    public void AnExtraActionProcessesItsOwnSilencePassInsteadOfWaitingForRoundEnd()
    {
        var session = Start("silence-watch", d =>
        { d["start"]!["actors"]![2]!["status"] = "silence3"; d["start"]!["actors"]![2]!["mp"] = 0; });
        Stay(session); var done = FinishBattleScenes(session, Stay(session));
        Assert.Equal(2, done.Observations.Count(e => e.Kind == "after-turn" && e.Actor == Smoke));
        var draws = done.Observations.Where(e => e.Kind == "rng-silence-expiry" && e.Actor == Smoke).ToArray();
        Assert.InRange(draws.Length, 1, 2);
        Assert.Equal((ushort?)0x300, draws[0].RandomRange);
        Assert.True((session.Current.Battle.GetActor(Smoke).Status & 0x300) <= 0x100);
    }
}
