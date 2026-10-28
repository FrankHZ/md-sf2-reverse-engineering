using System.Text.Json.Nodes;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleDiscardSessionTests
{
    private static readonly ActorRef Champion = new("champion");
    private static GameSession Selection(int slot = 0)
    {
        var session = Start("battle-discard");
        Accept(session, new Confirm()); Accept(session, new BeginDiscard());
        Accept(session, new SelectDiscardItem(slot));
        return session;
    }

    [Fact]
    public void RefusalAndBothCancelBoundariesAreFreeAndPreservePreview()
    {
        var session = Start("battle-discard"); var battle = session.Current.Battle;
        FinishMovement(session, Accept(session, new Move(ExplorationDirection.South)));
        Accept(session, new Confirm()); var preview = session.Current.Selection!.Preview;
        Accept(session, new BeginDiscard()); Accept(session, new SelectDiscardItem(2));
        Assert.True(session.QueryBattleChoices().Discard.Items.Single(c => c.Slot == 2).Item.Unsellable);
        var selected = session.Current;
        Assert.Equal("discard-unsellable", Send(session, new Confirm()).Failure!.Code);
        Assert.Same(selected, session.Current); Assert.Same(battle, session.Current.Battle);
        Accept(session, new Cancel()); Assert.Equal(BattleSelectionStage.ActionChoice, session.Current.Selection!.Stage);
        Accept(session, new BeginDiscard()); Accept(session, new Confirm());
        Assert.Equal(BattleSelectionStage.DiscardCommitReady, session.Current.Selection!.Stage);
        Accept(session, new Cancel()); Assert.Equal(BattleSelectionStage.ActionChoice, session.Current.Selection!.Stage);
        Assert.Same(preview, session.Current.Selection.Preview); Assert.Same(battle, session.Current.Battle);
        FinishMovement(session, Accept(session, new Cancel()));
        Assert.Equal(BattleSelectionStage.Movement, session.Current.Selection!.Stage);
        Assert.Equal(new MapPosition(1, 2), session.Current.Selection.Preview.Destination);
    }

    [Theory]
    [InlineData(0)] [InlineData(1)]
    public void ActualRevealThenAcknowledgementReturnsSameActionAndLaterStayConsumesOneTurn(int slot)
    {
        var session = Start("battle-discard"); var original = session.Current.Battle;
        FinishMovement(session, Accept(session, new Move(ExplorationDirection.South)));
        Accept(session, new Confirm()); var preview = session.Current.Selection!.Preview;
        Accept(session, new BeginDiscard()); Accept(session, new SelectDiscardItem(slot)); Accept(session, new Confirm());
        var selected = session.Current;
        var envelope = new CommandEnvelope(selected.SessionId, selected.Revision, Champion, new Confirm());
        var committed = session.Submit(envelope); Assert.Null(committed.Failure);
        Assert.Single(committed.Observations); Assert.Equal("discard-committed", committed.Observations[0].Kind);
        var battle = session.Current.Battle; var wait = session.Current.BattleDiscard!;
        Assert.False(session.Current.HasBattleControl); Assert.False(wait.Revealed);
        Assert.Same(preview, session.Current.Selection!.Preview);
        Assert.Equal(original.Cursor, battle.Cursor); Assert.Equal(original.Queue, battle.Queue);
        Assert.Equal(original.MainSeed, battle.MainSeed); Assert.Equal(original.ThinkingSeed, battle.ThinkingSeed);
        Assert.Equal(new MapPosition(1, 2), battle.GetActor(Champion).Position);
        Assert.Contains(slot == 0 ? "Medical Herb" : "Achilles Sword", wait.Text);
        Assert.Equal("stale-input", session.Submit(envelope).Failure!.Code);
        Assert.Equal("stale-or-wrong-discard-wait", Send(session, new Confirm()).Failure!.Code);
        Assert.Equal("stale-or-wrong-discard-wait", Send(session, new Acknowledge(wait.Token)).Failure!.Code);
        Assert.Equal("stale-or-wrong-discard-wait", Send(session, new CompleteTextReveal(new(wait.Token.Value + 1))).Failure!.Code);
        Accept(session, new CompleteTextReveal(wait.Token));
        Assert.Equal("stale-or-wrong-discard-wait", Send(session, new CompleteTextReveal(wait.Token)).Failure!.Code);
        Accept(session, new Acknowledge(wait.Token));
        Assert.True(session.Current.HasBattleControl); Assert.Null(session.Current.BattleDiscard);
        Assert.Same(battle, session.Current.Battle); Assert.Same(preview, session.Current.Selection!.Preview);
        Assert.Equal(Champion, session.Current.Selection.Actor); Assert.Equal(BattleSelectionStage.ActionChoice, session.Current.Selection.Stage);
        Assert.NotNull(Send(session, new Acknowledge(wait.Token)).Failure); Assert.Same(battle, session.Current.Battle);
        FinishMovement(session, Accept(session, new Cancel()));
        Assert.Same(battle.GetActor(Champion).SourceLoadout, session.Current.Battle.GetActor(Champion).SourceLoadout);
        Assert.Same(battle.ItemCustody, session.Current.Battle.ItemCustody);
        Assert.Equal(new MapPosition(1, 2), session.Current.Selection!.Preview.Destination);
        Stay(session);
        Assert.Equal(original.Cursor + 1, session.Current.Battle.Cursor);
        Assert.Equal(new ActorRef("receiver"), session.Current.Selection!.Actor);
    }

    [Fact]
    public void DiscardNeverCallsDefeatedHookOrActionSuffix()
    {
        var rules = new SessionRules("discard-no-suffix", new Sf2HealingRule(), RuleCompositions.SourcePhysical(),
            RuleCompositions.SourceItem(), RuleCompositions.SourceStay(), outcome: new ForbiddenAfterRemoval());
        var session = Assert.IsType<SessionStarted>(GameSession.Start(Reader(Document("battle-discard")), rules)).Session;
        Accept(session, new Confirm()); Accept(session, new BeginDiscard()); Accept(session, new Confirm());
        Accept(session, new Confirm()); var token = session.Current.BattleDiscard!.Token;
        Accept(session, new CompleteTextReveal(token)); Accept(session, new Acknowledge(token));
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
        Assert.Equal(Champion, session.Current.Selection!.Actor);
    }

    private sealed class ForbiddenAfterRemoval : IBattleOutcomeRule
    {
        public string Identity => "forbidden-drop-hook";
        public BattleOutcomeKind? Check(EngineBattleState battle) =>
            battle.GetActor(Champion).SourceLoadout!.Items[0] != 0 ? throw new InvalidOperationException("Drop ran outcome") : null;
        public bool DefeatedHook(EngineBattleState battle) =>
            battle.GetActor(Champion).SourceLoadout!.Items[0] != 0 ? throw new InvalidOperationException("Drop ran hook") : false;
    }

    [Theory]
    [InlineData("word", "stale-discard-item")] [InlineData("empty", "discard-item-slot")]
    [InlineData("actor", "discard-actor")] [InlineData("status", "discard-status")]
    [InlineData("unsupported", "discard-item-unsupported")]
    public void FinalConfirmRevalidatesLiveSelectionBeforeAnyPublication(string variation, string code)
    {
        var session = Selection(); Accept(session, new Confirm());
        var current = session.Current; var actor = current.Battle.GetActor(Champion);
        actor = variation switch
        {
            "word" => actor.With(sourceLoadout: new([61, 189, 113, 0], [63, 63, 63, 63])),
            "empty" => actor.With(sourceLoadout: new([127, 189, 113, 0], [63, 63, 63, 63])),
            "actor" => actor.With(hp: 0), "status" => actor.With(status: 1),
            _ => actor.With(sourceLoadout: new([99, 189, 113, 0], [63, 63, 63, 63])),
        };
        var battle = current.Battle.With(actors: current.Battle.Actors.Select(old => old.Actor == Champion ? actor : old));
        var snapshot = new SessionSnapshot(current.SessionId, current.Revision, current.ObservationSequence,
            new ActiveBattle(battle, current.Selection), current.Story, current.StopReason);
        var result = BattleCommandDispatcher.Submit(snapshot, new Confirm());
        Assert.Equal(code, result.Failure!.Code); Assert.Same(snapshot, result.Snapshot);
        Assert.Empty(result.Observations); Assert.Same(current.Battle.ItemCustody, battle.ItemCustody);
    }

    [Fact]
    public void WrongStageActorStaleEnvelopeAndMissingRequiredCustodyAreTypedFailures()
    {
        var session = Start("battle-discard", d => d["start"]!.AsObject().Remove("itemCustody"));
        Assert.Equal("invalid-discard-selection", Send(session, new BeginDiscard()).Failure!.Code);
        Assert.Equal("invalid-discard-selection", Send(session, new SelectDiscardItem(0)).Failure!.Code);
        Accept(session, new Confirm()); var before = session.Current;
        Assert.Equal("wrong-actor", session.Submit(new(before.SessionId, before.Revision, new("receiver"), new BeginDiscard())).Failure!.Code);
        Assert.Equal("stale-input", session.Submit(new(before.SessionId, before.Revision - 1, Champion, new BeginDiscard())).Failure!.Code);
        Accept(session, new BeginDiscard());
        Assert.Equal("discard-item-slot", Send(session, new SelectDiscardItem(4)).Failure!.Code);
        Accept(session, new SelectDiscardItem(1)); Accept(session, new Confirm()); before = session.Current;
        Assert.Equal("discard-custody-unavailable", Send(session, new Confirm()).Failure!.Code);
        Assert.Same(before, session.Current); Assert.Null(session.Current.Battle.ItemCustody);
    }
}
