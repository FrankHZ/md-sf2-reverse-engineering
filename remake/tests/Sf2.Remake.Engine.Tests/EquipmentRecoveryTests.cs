using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class EquipmentRecoveryTests
{
    private static SessionSnapshot Setup(byte weapon = 97, ushort hp = 10, byte mp = 5, bool ringFirst = false, bool poison = false)
    {
        var initial = Start("poison-turn", d =>
        {
            d["start"]!["actors"]![0]!["status"] = "none";
            d["start"]!["actors"]![1]!["status"] = "none";
        }).Current;
        var old = initial.Battle.GetActor(initial.Selection!.Actor);
        var equipment = new BattleEquipmentDefinition(10,
            [new(97, "Holy Staff", uint.MaxValue, 29, 1, 1, HpRecovery: 3),
             new(100, "Mystery Staff", uint.MaxValue, 39, 1, 1, MpRecovery: 2)],
            rings: [new(124, "Life Ring", uint.MaxValue, null, 0, HpRecovery: 5)]);
        var definition = new BattleActorDefinition(old.Actor, BattleClassRule.Vicar, 1, 30, 10, 10, 5, 20,
            false, 6, [], old.Definition.Physical, equipment: equipment);
        ushort[] words = ringFirst ? [252, (ushort)(weapon + 128), 127, 127] : [(ushort)(weapon + 128), 252, 127, 127];
        var actor = new BattleActorState(old.Deployment with { Definition = definition }, hp, mp, 0, old.Position,
            0, 0, sourceLoadout: new(words, []), status: poison ? (ushort)2 : (ushort)0);
        return new(initial.SessionId, initial.Revision, initial.ObservationSequence,
            initial.Battle.With(actors: initial.Battle.Actors.Select(a => a.Actor == old.Actor ? actor : a)),
            initial.Selection, initial.StopReason);
    }

    private static SessionResult Send(SessionSnapshot state, SessionCommand command)
    {
        var result = state.BattleScene is not null ? BattleSceneContinuation.Submit(state, command) : BattleCommandDispatcher.Submit(state, command);
        Assert.Null(result.Failure);
        return result;
    }

    private static SessionResult Stay(SessionSnapshot state)
    {
        state = Send(state, new Confirm()).Snapshot;
        state = Send(state, new ChooseAction(SessionAction.Stay)).Snapshot;
        return Send(state, new Confirm());
    }

    private static SessionResult Complete(SessionSnapshot state) =>
        Send(state, new CompletePresentation(state.BattleScene!.Token, state.BattleScene.CompletionKind));

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void StatusFreeStayRecoversTwoSlotsInSourceOrderAndCompletesOnlyOnce(bool ringFirst)
    {
        var initial = Setup(ringFirst: ringFirst); var actor = initial.Selection!.Actor;
        var result = Stay(initial); var holy = result.Snapshot;
        Assert.Equal(13, holy.Battle.GetActor(actor).Hp);
        Assert.Equal(356, holy.BattleScene!.EquipmentRecoveryText); Assert.Equal(3, holy.BattleScene.Amount);
        Assert.Equal(initial.Battle.Cursor, holy.Battle.Cursor);
        var life = Complete(holy).Snapshot;
        Assert.Equal(18, life.Battle.GetActor(actor).Hp); Assert.Equal(5, life.BattleScene!.Amount);
        Assert.NotEqual(holy.BattleScene.Token, life.BattleScene.Token);
        Assert.NotNull(BattleSceneContinuation.Submit(life, new CompletePresentation(holy.BattleScene.Token, holy.BattleScene.CompletionKind)).Failure);
        var done = Complete(life).Snapshot;
        Assert.Null(done.BattleScene); Assert.Equal(initial.Battle.Cursor + 1, done.Battle.Cursor);
        Assert.Equal(initial.Battle.MainSeed, done.Battle.MainSeed);
        Assert.Equal(initial.Battle.ThinkingSeed, done.Battle.ThinkingSeed);
        Assert.Equal(initial.Battle.GetActor(actor).Exp, done.Battle.GetActor(actor).Exp);
        Assert.Equal(initial.Battle.Gold, done.Battle.Gold);
    }

    [Fact]
    public void MysteryRecoversMpBeforeRingAndPoisonAndCapsEachResource()
    {
        var initial = Setup(100, hp: 28, mp: 9, poison: true); var actor = initial.Selection!.Actor;
        var mystery = Stay(initial).Snapshot;
        Assert.Equal(357, mystery.BattleScene!.EquipmentRecoveryText); Assert.Equal(1, mystery.BattleScene.Amount);
        Assert.Equal(10, mystery.Battle.GetActor(actor).Mp); Assert.Equal(28, mystery.Battle.GetActor(actor).Hp);
        var life = Complete(mystery).Snapshot;
        Assert.Equal(2, life.BattleScene!.Amount); Assert.Equal(30, life.Battle.GetActor(actor).Hp);
        var poison = Complete(life).Snapshot;
        Assert.True(poison.BattleScene!.IsPoisonTurn); Assert.Equal(30, poison.Battle.GetActor(actor).Hp);
        var done = Complete(poison).Snapshot;
        while (done.BattleScene is not null) done = Complete(done).Snapshot;
        Assert.Equal(28, done.Battle.GetActor(actor).Hp);
        Assert.Equal(initial.Battle.MainSeed, done.Battle.MainSeed);
    }

    [Theory]
    [InlineData(27, 3, false)] [InlineData(28, 2, false)] [InlineData(26, 3, true)]
    public void HolyCapsBeforeLifeAndOnlyPositiveDeltasShow(ushort hp, int delta, bool lifeExpected)
    {
        var first = Stay(Setup(hp: hp)).Snapshot;
        Assert.Equal(delta, first.BattleScene!.Amount);
        var second = Complete(first).Snapshot;
        Assert.Equal(lifeExpected, second.BattleScene is not null);
        if (lifeExpected) { Assert.Equal(1, second.BattleScene!.Amount); second = Complete(second).Snapshot; }
        Assert.Equal(30, second.Battle.Actors[0].Hp);
    }

    [Fact]
    public void FullResourcesHaveNoNoticeAndNoFreeReadOrRefreshRecovers()
    {
        var full = Setup(100, 30, 10);
        Assert.Null(Stay(full).Snapshot.BattleScene);
        var state = Setup(); var actor = state.Battle.Actors[0];
        _ = BattleEquipment.Choices(actor);
        var refreshed = actor.With(refreshStats: true);
        Assert.Equal(actor.Hp, refreshed.Hp); Assert.Equal(actor.Mp, refreshed.Mp);
        var bare = BattleEquipment.Equip(BattleEquipment.Equip(actor, 4), 5);
        Assert.Null(EquipmentRecoveryRules.Next(bare));
        Assert.Equal(actor.Hp, bare.Hp);
    }

    [Fact]
    public void DeadAbsentAndReserveActorsDoNotRecoverAndBrokenWordsReject()
    {
        var initial = Setup(); var actor = initial.Battle.Actors[0];
        Assert.Null(EquipmentRecoveryRules.Next(actor.With(hp: 0)));
        var reserve = new BattleActorState(actor.Deployment, 10, 5, 0, null, 0, 0,
            sourceLoadout: actor.SourceLoadout, initialized: false);
        Assert.Null(EquipmentRecoveryRules.Next(reserve));
        var absent = new BattleActorState(actor.Deployment, 10, 5, 0, null, 0, 0, sourceLoadout: actor.SourceLoadout);
        Assert.Null(EquipmentRecoveryRules.Next(absent));
        Assert.False(BattleActionCommitter.NeedsAfterTurn(initial.Battle, absent));
        foreach (int slot in new[] { 0, 1 })
        {
            var words = actor.SourceLoadout!.Items.ToArray(); words[slot] |= 256;
            Assert.Equal("equipment-broken", Assert.Throws<BattleRuleException>(() => BattleEquipment.ValidateLoadout(actor.Definition, new(words, []))).Code);
        }
    }

    [Fact]
    public void EachQueueEntryRunsRecoveryOnceRatherThanOncePerRound()
    {
        var state = Setup(); var actor = state.Selection!.Actor;
        var queue = state.Battle.Queue.ToList(); queue.Insert(state.Battle.Cursor + 1, queue[state.Battle.Cursor]);
        state = new(state.SessionId, state.Revision, state.ObservationSequence, state.Battle.With(queue: queue), state.Selection, state.StopReason);
        var first = Stay(state).Snapshot;
        while (first.BattleScene is not null) first = Complete(first).Snapshot;
        Assert.Equal(actor, first.Selection!.Actor); Assert.Equal(18, first.Battle.GetActor(actor).Hp);
        var second = Stay(first).Snapshot;
        while (second.BattleScene is not null) second = Complete(second).Snapshot;
        Assert.Equal(26, second.Battle.GetActor(actor).Hp); Assert.Equal(state.Battle.Round, second.Battle.Round);
    }
}
