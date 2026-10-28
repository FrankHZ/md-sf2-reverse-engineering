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

public sealed class BattleGiveSessionTests
{
    private static readonly ActorRef Giver = new("champion"), Recipient = new("receiver");
    private static GameSession Selection(bool full = false)
    {
        var session = Start("battle-give", d => { if (full) d["actors"]![1]!["items"] = new JsonArray(189, 0, 0, 50); });
        Accept(session, new Confirm()); Accept(session, new BeginGive());
        return session;
    }
    private static void Target(GameSession session)
    { Accept(session, new Confirm()); Accept(session, new SelectTarget(Recipient)); Accept(session, new Confirm()); }

    [Fact]
    public void UnknownRecipientIsATypedRejectionWithoutChangingSessionOrCustody()
    {
        var session = Selection(); Accept(session, new Confirm());
        var before = session.Current;
        var result = Send(session, new SelectTarget(new("not-in-battle")));
        Assert.Equal("give-target-missing", result.Failure!.Code);
        Assert.Equal(SessionFailureKind.IllegalCommand, result.Failure.Kind);
        Assert.Same(before, result.Snapshot); Assert.Same(before, session.Current);
        Assert.Empty(result.Observations);
        Assert.Equal(before.Revision, session.Current.Revision);
        Assert.Equal(before.Battle.Cursor, session.Current.Battle.Cursor);
        Assert.Equal(before.Battle.MainSeed, session.Current.Battle.MainSeed);
        Assert.Equal(before.Battle.ThinkingSeed, session.Current.Battle.ThinkingSeed);
        Assert.Same(before.Battle.GetActor(Giver).SourceLoadout, session.Current.Battle.GetActor(Giver).SourceLoadout);
        Assert.Same(before.Battle.GetActor(Recipient).SourceLoadout, session.Current.Battle.GetActor(Recipient).SourceLoadout);
    }

    [Fact]
    public void LaterSuffixFailureCannotRewindOrRepeatAcceptedCustody()
    {
        var rules = new SessionRules("give-failure", new Sf2HealingRule(), RuleCompositions.SourcePhysical(),
            RuleCompositions.SourceItem(), RuleCompositions.SourceStay(), outcome: new FailedSuffix());
        var session = Assert.IsType<SessionStarted>(GameSession.Start(Reader(Document("battle-give")), rules)).Session;
        Accept(session, new Confirm()); Accept(session, new BeginGive()); Target(session); Accept(session, new Confirm());
        var committed = session.Current.Battle; var token = session.Current.BattleGive!.Token;
        Accept(session, new CompleteTextReveal(token));
        var failed = Send(session, new Acknowledge(token));
        Assert.Equal(SessionStopReason.Faulted, failed.StopReason);
        Assert.Equal("battle-policy-failure", failed.Failure!.Code);
        Assert.Same(committed, session.Current.Battle);
        Assert.Equal(new ushort[] { 0, 50, 127, 127 }, committed.GetActor(Giver).SourceLoadout!.Items);
        Assert.Equal(new ushort[] { 189, 0, 0, 50 }, committed.GetActor(Recipient).SourceLoadout!.Items);
        Assert.Equal("session-stopped", Send(session, new Acknowledge(token)).Failure!.Code);
        Assert.Same(committed, session.Current.Battle);
    }

    private sealed class FailedSuffix : IBattleOutcomeRule
    {
        public string Identity => "fail-after-give";
        public BattleOutcomeKind? Check(EngineBattleState battle) => null;
        public bool DefeatedHook(EngineBattleState battle) =>
            battle.GetActor(Giver).SourceLoadout!.Items[0] == 0 ? throw new InvalidOperationException("suffix failed") : false;
    }

    [Fact]
    public void EverySelectionCancelIsFreeAndPreservesProvisionalMovement()
    {
        var session = Start("battle-give", d => d["actors"]![1]!["items"] = new JsonArray(189, 0, 0, 50));
        var original = session.Current.Battle;
        FinishMovement(session, Accept(session, new Move(ExplorationDirection.South)));
        FinishMovement(session, Accept(session, new Move(ExplorationDirection.East)));
        var battle = session.Current.Battle;
        Accept(session, new Confirm()); Accept(session, new BeginGive());
        Accept(session, new SelectGiveItem(2));
        Assert.Equal((ushort)50, session.Current.Selection!.ItemWord); Assert.Same(battle, session.Current.Battle);
        Accept(session, new Cancel()); Assert.Equal(BattleSelectionStage.ActionChoice, session.Current.Selection!.Stage);
        Accept(session, new BeginGive()); Accept(session, new Confirm()); Accept(session, new Cancel());
        Assert.Equal(BattleSelectionStage.GiveItemChoice, session.Current.Selection!.Stage);
        Target(session); Assert.Equal(BattleSelectionStage.GiveExchangeChoice, session.Current.Selection!.Stage);
        Accept(session, new SelectGiveReturn(2)); Accept(session, new Confirm()); Accept(session, new Cancel());
        Assert.Equal(BattleSelectionStage.GiveExchangeChoice, session.Current.Selection!.Stage);
        Accept(session, new Cancel()); Assert.Equal(BattleSelectionStage.GiveTargetChoice, session.Current.Selection!.Stage);
        Assert.Same(battle, session.Current.Battle); Assert.Equal(new MapPosition(2, 3), session.Current.Selection.Preview.Destination);
        Accept(session, new Cancel()); Accept(session, new Cancel());
        FinishMovement(session, Accept(session, new Cancel()));
        Assert.Equal(new MapPosition(1, 2), session.Current.Selection!.Preview.Destination);
        Assert.Equal(original.MainSeed, session.Current.Battle.MainSeed);
        Assert.Equal(original.ThinkingSeed, session.Current.Battle.ThinkingSeed);
        Assert.Equal(original.Cursor, session.Current.Battle.Cursor);
        foreach (var actor in original.Actors)
        {
            Assert.Equal(actor.Position, session.Current.Battle.GetActor(actor.Actor).Position);
            Assert.Same(actor.SourceLoadout, session.Current.Battle.GetActor(actor.Actor).SourceLoadout);
        }
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void ConfirmationPublishesOnceThenTypedDeliveryCompletesOneActionAndAllowsEquip(bool full)
    {
        var session = Selection(full); var before = session.Current.Battle;
        Target(session);
        if (full) { Accept(session, new SelectGiveReturn(0)); Accept(session, new Confirm()); }
        var selected = session.Current;
        var envelope = new CommandEnvelope(selected.SessionId, selected.Revision, Giver, new Confirm());
        var committed = session.Submit(envelope);
        Assert.Null(committed.Failure); Assert.Single(committed.Observations, o => o.Kind == "give-committed");
        Assert.DoesNotContain(committed.Observations, o => o.Kind == "action-committed");
        Assert.Equal(before.Cursor, session.Current.Battle.Cursor); Assert.False(session.Current.HasBattleControl);
        Assert.Equal(before.MainSeed, session.Current.Battle.MainSeed); Assert.Equal(before.ThinkingSeed, session.Current.Battle.ThinkingSeed);
        Assert.Equal(full ? new ushort[] { 0, 50, 61, 127 } : new ushort[] { 0, 50, 127, 127 }, session.Current.Battle.GetActor(Giver).SourceLoadout!.Items);
        Assert.Equal(20, session.Current.Battle.GetActor(Giver).Attack);
        Assert.Equal("stale-input", session.Submit(envelope).Failure!.Code);
        var wait = session.Current.BattleGive!;
        var pending = session.Current;
        Assert.Equal("stale-or-wrong-give-wait", Send(session, new Acknowledge(wait.Token)).Failure!.Code);
        Assert.Equal("stale-or-wrong-give-wait", Send(session, new CompleteTextReveal(new(wait.Token.Value + 1))).Failure!.Code);
        Assert.Equal("stale-or-wrong-give-wait", Send(session, new Cancel()).Failure!.Code);
        Assert.Same(pending, session.Current);
        Accept(session, new CompleteTextReveal(wait.Token));
        Assert.Equal("stale-or-wrong-give-wait", Send(session, new CompleteTextReveal(wait.Token)).Failure!.Code);
        var completed = Accept(session, new Acknowledge(wait.Token));
        Assert.Single(completed.Observations, o => o.Kind == "action-committed" && o.Actor == Giver);
        Assert.Single(completed.Observations, o => o.Kind == "after-turn" && o.Actor == Giver);
        Assert.Equal(Recipient, session.Current.Selection!.Actor);
        var received = session.Current;
        Assert.NotNull(Send(session, new Acknowledge(wait.Token)).Failure); Assert.Same(received, session.Current);
        Accept(session, new Confirm()); Accept(session, new BeginEquipment()); Accept(session, new SelectEquipment(3)); Accept(session, new Confirm());
        Assert.Equal(20, session.Current.Battle.GetActor(Recipient).Attack);
        Assert.Equal(2, BattleEquipment.Physical(session.Current.Battle.GetActor(Recipient))!.MaximumRange);
        Assert.Equal(20, session.Current.Battle.GetActor(Giver).Attack);
    }

    [Theory]
    [InlineData("word")] [InlineData("return-word")] [InlineData("capacity")]
    [InlineData("range")] [InlineData("status")] [InlineData("movement")]
    public void ConfirmationRevalidatesLiveTwoActorStateAndPublishesNoPartialMutation(string change)
    {
        var session = Selection(true); Target(session); Accept(session, new Confirm());
        var selected = session.Current; var battle = selected.Battle;
        var giver = battle.GetActor(Giver); var target = battle.GetActor(Recipient);
        if (change == "word") giver = giver.With(sourceLoadout: new([50, 0, 178, 127], [63, 63, 63, 63]));
        if (change == "return-word") target = target.With(sourceLoadout: new([0, 189, 0, 50], [63, 63, 63, 63]));
        if (change == "capacity") target = target.With(sourceLoadout: new([189, 0, 0, 127], [63, 63, 63, 63]));
        if (change == "range") target = target.With(position: new(3, 3));
        if (change == "status") target = target.With(status: 1);
        if (change == "movement") target = target.With(position: new(1, 2));
        battle = battle.With(actors: battle.Actors.Select(a => a.Actor == Giver ? giver : a.Actor == Recipient ? target : a));
        var current = new SessionSnapshot(selected.SessionId, selected.Revision, selected.ObservationSequence,
            battle, selected.Selection, selected.StopReason);
        var rejected = BattleCommandDispatcher.Submit(current, new Confirm());
        Assert.NotNull(rejected.Failure); Assert.Same(current, rejected.Snapshot); Assert.Empty(rejected.Observations);
    }

    [Fact]
    public void InvalidCommandsNoTargetNoItemsAndFrozenProfileLeaveStateAlone()
    {
        var session = Selection(); var before = session.Current;
        Assert.Equal("give-item-slot", Send(session, new SelectGiveItem(3)).Failure!.Code);
        Assert.Equal("wrong-selection-stage", Send(session, new SelectGiveReturn(0)).Failure!.Code);
        Assert.Equal("wrong-actor", session.Submit(new(before.SessionId, before.Revision, Recipient, new Confirm())).Failure!.Code);
        Assert.Equal("stale-input", session.Submit(new(before.SessionId, before.Revision - 1, Giver, new Confirm())).Failure!.Code);
        Assert.Same(before, session.Current);
        Accept(session, new Confirm()); before = session.Current;
        Assert.NotNull(Send(session, new Confirm()).Failure);
        Assert.NotNull(Send(session, new SelectTarget(Giver)).Failure);
        Assert.NotNull(Send(session, new SelectTarget(new("reserve"))).Failure);
        Assert.Equal("give-target-missing", Send(session, new SelectTarget(new("unknown"))).Failure!.Code);
        Assert.Same(before, session.Current);
        session = Start("battle-give", d => d["encounters"]![0]!["placements"]![1]!["x"] = 4);
        Accept(session, new Confirm()); before = session.Current;
        Assert.Equal("give-no-target", Send(session, new BeginGive()).Failure!.Code); Assert.Same(before, session.Current);
        session = Start("battle-give", d => { d["actors"]![0]!["items"] = new JsonArray(); d["actors"]![0]!["attack"] = 20; });
        Accept(session, new Confirm()); Assert.Equal("give-no-items", Send(session, new BeginGive()).Failure!.Code);
        session = Start("battle-equipment"); Accept(session, new Confirm());
        Assert.Equal("give-unavailable", Send(session, new BeginGive()).Failure!.Code);
    }

    [Fact]
    public void TransferFromMovedPreviewCommitsPositionBeforeFeedbackAndReceivedHerbIsUsable()
    {
        var session = Start("battle-give", d => d["actors"]![1]!["items"] = new JsonArray(189, 0));
        FinishMovement(session, Accept(session, new Move(ExplorationDirection.South)));
        FinishMovement(session, Accept(session, new Move(ExplorationDirection.East)));
        Accept(session, new Confirm()); Accept(session, new BeginGive()); Accept(session, new SelectGiveItem(1)); Target(session);
        var result = Accept(session, new Confirm());
        Assert.Equal(new MapPosition(2, 3), session.Current.Battle.GetActor(Giver).Position);
        Assert.Single(result.Observations, o => o.Kind == "movement");
        var wait = session.Current.BattleGive!;
        Accept(session, new CompleteTextReveal(wait.Token)); Accept(session, new Acknowledge(wait.Token));
        Accept(session, new Confirm()); Accept(session, new SelectItem(2)); Accept(session, new SelectTarget(Recipient));
        FinishBattleScenes(session, Accept(session, new Confirm()));
        Assert.Equal(110, session.Current.Battle.GetActor(Recipient).Hp);
        Assert.Equal(new ushort[] { 189, 0, 127, 127 }, session.Current.Battle.GetActor(Recipient).SourceLoadout!.Items);
    }
}
