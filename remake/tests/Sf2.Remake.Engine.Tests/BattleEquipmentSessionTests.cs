using System.Text.Json.Nodes;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleEquipmentSessionTests
{
    private static readonly ActorRef Champion = new("champion"), Guardian = new("guardian");

    [Fact]
    public void PreviewCancelConfirmAndLaterMovementCancelHaveSeparateCommitBoundaries()
    {
        var session = Start("battle-equipment");
        var before = session.Current.Battle;
        Assert.Equal(Champion, session.Current.Selection!.Actor);
        Accept(session, new Confirm());
        Accept(session, new BeginEquipment());
        Assert.Equal(0, session.Current.Selection!.EquipmentSlot);
        Accept(session, new SelectEquipment(1));
        Assert.Same(before, session.Current.Battle);
        var query = session.QueryBattleChoices();
        Assert.Equal(30, query.Equipment.Single(o => o.Slot == 1).Attack);
        Assert.Equal(25, session.Current.Battle.GetActor(Champion).Attack);
        Accept(session, new Cancel());
        Assert.Equal(BattleSelectionStage.ActionChoice, session.Current.Selection!.Stage);
        Assert.Same(before, session.Current.Battle);
        Accept(session, new BeginEquipment()); Accept(session, new SelectEquipment(1));
        var selected = session.Current;
        var envelope = new CommandEnvelope(selected.SessionId, selected.Revision, Champion, new Confirm());
        var confirmed = session.Submit(envelope);
        Assert.Null(confirmed.Failure);
        Assert.Single(confirmed.Observations);
        Assert.Equal(selected.Revision + 1, confirmed.Snapshot.Revision);
        Assert.Equal("equipment-confirmed", confirmed.Observations[0].Kind);
        Assert.Equal(BattleSelectionStage.ActionChoice, session.Current.Selection!.Stage);
        Assert.Equal(30, session.Current.Battle.GetActor(Champion).Attack);
        Assert.Equal(before.MainSeed, session.Current.Battle.MainSeed);
        Assert.Equal(before.ThinkingSeed, session.Current.Battle.ThinkingSeed);
        Assert.Equal(before.Cursor, session.Current.Battle.Cursor); Assert.Equal(before.Round, session.Current.Battle.Round);
        Assert.Equal(before.TurnOrder, session.Current.Battle.TurnOrder);
        Assert.Same(before.GetActor(Guardian), session.Current.Battle.GetActor(Guardian));
        Assert.Equal("stale-input", session.Submit(envelope).Failure!.Code);
        Assert.Same(confirmed.Snapshot, session.Current);
        Assert.Equal("wrong-selection-stage", Send(session, new Confirm()).Failure!.Code);
        Assert.Equal("wrong-actor", session.Submit(new(session.Current.SessionId, session.Current.Revision, Guardian, new BeginEquipment())).Failure!.Code);
        FinishMovement(session, Accept(session, new Cancel()));
        FinishMovement(session, Accept(session, new Move(ExplorationDirection.South)));
        FinishMovement(session, Accept(session, new Cancel()));
        Assert.Equal(BattleSelectionStage.Movement, session.Current.Selection!.Stage);
        Assert.Equal(new MapPosition(1, 2), session.Current.Selection.Preview.Destination);
        Assert.Equal(new ushort[] { 50, 189, 50, 0 }, session.Current.Battle.GetActor(Champion).SourceLoadout!.Items);
        Assert.Equal(30, session.Current.Battle.GetActor(Champion).Attack);
        Assert.Equal(before.MainSeed, session.Current.Battle.MainSeed);
        Assert.Equal(before.Gold, session.Current.Battle.Gold);
    }

    [Fact]
    public void LiveRangeIsQueriedAndConfirmedFromTheSameSelectionAuthority()
    {
        var session = Start("battle-equipment");
        FinishMovement(session, Accept(session, new Move(ExplorationDirection.South)));
        Accept(session, new Confirm());
        var attack = session.QueryBattleChoices().Actions.Single(o => o.Action.Kind == BattleActionKind.Physical);
        Assert.Equal(2, attack.MaximumRange!.Value); Assert.True(attack.Targets.Single(t => t.Actor == Guardian).Enabled);
        Accept(session, new BeginEquipment()); Accept(session, new SelectEquipment(1)); Accept(session, new Confirm());
        attack = session.QueryBattleChoices().Actions.Single(o => o.Action.Kind == BattleActionKind.Physical);
        Assert.Equal(1, attack.MaximumRange!.Value); Assert.False(attack.Targets.Single(t => t.Actor == Guardian).Enabled);
        Accept(session, new ChooseAction(SessionAction.PhysicalAttack));
        Assert.Equal("target-range", Send(session, new SelectTarget(Guardian)).Failure!.Code);
        FinishMovement(session, Accept(session, new Cancel()));
        Accept(session, new Confirm()); Accept(session, new ChooseAction(SessionAction.PhysicalAttack));
        Accept(session, new SelectTarget(Guardian));
        var result = FinishBattleScenes(session, Accept(session, new Confirm()));
        Assert.Null(result.Failure); Assert.True(session.Current.Battle.GetActor(Guardian).Hp < 500);
        Assert.Equal(30, session.Current.Battle.GetActor(Champion).Attack);
    }

    [Fact]
    public void NoEligibleWeaponAndInvalidSlotDoNotCreateOrChangeSelection()
    {
        var session = Start("battle-equipment", d =>
        { d["actors"]![0]!["items"] = new JsonArray(0); d["actors"]![0]!["attack"] = 20; });
        Accept(session, new Confirm()); var before = session.Current;
        Assert.Empty(session.QueryBattleChoices().Equipment);
        Assert.Equal("no-equippable-weapons", Send(session, new BeginEquipment()).Failure!.Code);
        Assert.Same(before, session.Current);
        session = Start("battle-equipment"); Accept(session, new Confirm()); Accept(session, new BeginEquipment());
        before = session.Current;
        Assert.Equal("equipment-slot", Send(session, new SelectEquipment(3)).Failure!.Code);
        Assert.Same(before, session.Current);
        Assert.Equal("wrong-selection-stage", Send(session, new ChooseAction(SessionAction.Stay)).Failure!.Code);
        Assert.Same(before, session.Current);
    }

    [Fact]
    public void FourWeaponUnarmedStartPreviewsFirstSlotWithoutSilentlyEquippingIt()
    {
        var session = Start("battle-equipment", d =>
        { d["actors"]![0]!["items"] = new JsonArray(50, 61, 50, 61); d["actors"]![0]!["attack"] = 20; });
        Accept(session, new Confirm()); var battle = session.Current.Battle;
        Accept(session, new BeginEquipment());
        Assert.Equal(0, session.Current.Selection!.EquipmentSlot); Assert.Same(battle, session.Current.Battle);
        Assert.Equal("equipment-slot", Send(session, new SelectEquipment(4)).Failure!.Code);
        Accept(session, new Cancel()); Assert.Same(battle, session.Current.Battle);
        Accept(session, new BeginEquipment()); Accept(session, new SelectEquipment(3)); Accept(session, new Confirm());
        Assert.Equal(new ushort[] { 50, 61, 50, 189 }, session.Current.Battle.GetActor(Champion).SourceLoadout!.Items);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void ConfirmationRevalidatesLiveStatusAndSelectedPackedSlot(bool changedWord)
    {
        var session = Start("battle-equipment");
        Accept(session, new Confirm()); Accept(session, new BeginEquipment()); Accept(session, new SelectEquipment(1));
        var selected = session.Current;
        var actor = selected.Battle.GetActor(Champion);
        var changed = changedWord ? actor.With(sourceLoadout: new([178, 50, 61, 0], [63, 63, 63, 63])) : actor.With(status: 1);
        var snapshot = new SessionSnapshot(selected.SessionId, selected.Revision, selected.ObservationSequence,
            selected.Battle.With(actors: selected.Battle.Actors.Select(a => a.Actor == Champion ? changed : a)),
            selected.Selection, selected.StopReason);
        var result = BattleCommandDispatcher.Submit(snapshot, new Confirm());
        Assert.Equal(changedWord ? "stale-equipment-selection" : "equipment-actor", result.Failure!.Code);
        Assert.Same(snapshot, result.Snapshot); Assert.Empty(result.Observations);
        Assert.Equal("stale-input", session.Submit(new(Guid.NewGuid(), selected.Revision, Champion, new Confirm())).Failure!.Code);
    }
}
