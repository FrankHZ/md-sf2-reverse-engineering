using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class ConnectedBattleGiveTests
{
    internal static PrivateBattleDefinitions Source()
    {
        var basis = ConnectedBattleEquipmentTests.Source();
        return new(null!, [], basis.Classes.Values, basis.Items.Values.Select(item => item with
            { EquipFlagsExpression = "SDMN", EquipFlags = ["SDMN"] }).Concat(
            [new(0, "MEDICAL_HERB", "Healing Herb", "", "NONE", [], 0, 1, 0, "CONSUMABLE", "HEALIN", "HEALIN", 1, [new("NONE", 0)]),
             new(90, "RESTRICTED", "Unsupported equipment", "", "SDMN", ["SDMN"], 1, 1, 0, "WEAPON|CURSED", "NOTHING", "NOTHING", 0, [new("INCREASE_ATT", 5)])]), [], [], []);
    }

    internal static BattleDefinition Bind(BattleDefinition battle)
    {
        var source = Source();
        return PrivateFieldPreparation.Give(PrivateFieldPreparation.Equipment(battle, source, allowBattleChanges: true),
            PrivateFieldPreparation.Read(source));
    }

    private static GameSession Session(bool full = false, bool adjacent = true)
    {
        var doc = Document("battle-give"); doc.AsObject().Remove("battleGive");
        if (full) doc["actors"]![1]!["items"] = new JsonArray(189, 0, 0, 50);
        if (!adjacent) doc["encounters"]![0]!["placements"]![1]!["x"] = 4;
        var admitted = Assert.IsType<ScenarioReadAccepted>(Reader(doc).Read());
        var battle = admitted.Definition.Encounters.Values.Single(); Assert.Null(battle.Give);
        battle = battle.WithDeployments(battle.Deployments.Select(d => d.Faction == BattleFaction.Ally
            ? d with { Definition = d.Definition.WithGrowth(new(0,
                (byte)(d.Definition.Attack - d.Definition.Equipment!.BaseAttack), [], [], new Dictionary<byte, SpellRef>())) } : d));
        return Assert.IsType<SessionStarted>(GameSession.Start(new(admitted.Definition.Package, [Bind(battle)]), admitted.Start)).Session;
    }

    internal static void Choose(GameSession session, ActorRef target, int slot = 0, int? returned = null)
    {
        Accept(session, new Confirm()); Accept(session, new BeginGive()); Accept(session, new SelectGiveItem(slot));
        Accept(session, new Confirm()); Accept(session, new SelectTarget(target)); Accept(session, new Confirm());
        if (returned is { } selected) { Accept(session, new SelectGiveReturn(selected)); Accept(session, new Confirm()); }
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void ExplicitProductBindingTransfersEquippedWordsAndCompletesExactlyOneStay(bool full)
    {
        var session = Session(full); var before = session.Current.Battle;
        Choose(session, new("receiver"), returned: full ? 0 : null);
        var selected = session.Current;
        var envelope = new CommandEnvelope(selected.SessionId, selected.Revision, selected.Selection!.Actor, new Confirm());
        var commit = session.Submit(envelope); Assert.Null(commit.Failure);
        Assert.Single(commit.Observations, o => o.Kind == "give-committed");
        Assert.DoesNotContain(commit.Observations, o => o.Kind == "action-committed");
        var battle = session.Current.Battle;
        Assert.Equal(full ? new ushort[] { 0, 50, 61, 127 } : [0, 50, 127, 127], battle.GetActor(new("champion")).SourceLoadout!.Items);
        Assert.Equal(full ? new ushort[] { 0, 0, 50, 50 } : [189, 0, 0, 50], battle.GetActor(new("receiver")).SourceLoadout!.Items);
        Assert.Equal(20, battle.GetActor(new("champion")).Attack);
        Assert.Equal(full ? 15 : 25, battle.GetActor(new("receiver")).Attack);
        Assert.Equal(1, BattleEquipment.Physical(battle.GetActor(new("champion")))!.MaximumRange);
        Assert.Equal(before.Cursor, battle.Cursor); Assert.Equal(before.Queue, battle.Queue);
        Assert.Equal(before.MainSeed, battle.MainSeed); Assert.Equal(before.ThinkingSeed, battle.ThinkingSeed);
        Assert.Same(before.ItemCustody, battle.ItemCustody); Assert.Equal(before.Gold, battle.Gold);
        Assert.Equal("stale-input", session.Submit(envelope).Failure!.Code);
        var wait = session.Current.BattleGive!;
        Assert.Equal((byte)50, wait.GivenItemId); Assert.Equal(full ? (byte?)61 : null, wait.ReturnedItemId);
        Accept(session, new CompleteTextReveal(wait.Token));
        var completed = Accept(session, new Acknowledge(wait.Token));
        Assert.Single(completed.Observations, o => o.Kind == "action-committed" && o.Actor == new ActorRef("champion"));
        Assert.Single(completed.Observations, o => o.Kind == "after-turn" && o.Actor == new ActorRef("champion"));
        Assert.Equal("receiver", session.Current.Selection!.Actor.Value);
        var next = session.Current; Assert.NotNull(Send(session, new Acknowledge(wait.Token)).Failure); Assert.Same(next, session.Current);
        Accept(session, new Confirm()); Accept(session, new BeginEquipment()); Accept(session, new SelectEquipment(3)); Accept(session, new Confirm());
        Assert.Equal(20, session.Current.Battle.GetActor(new("receiver")).Attack);
        Assert.Equal(2, BattleEquipment.Physical(session.Current.Battle.GetActor(new("receiver")))!.MaximumRange);
        Assert.Equal("discard-unavailable", Send(session, new BeginDiscard()).Failure!.Code);
    }

    [Fact]
    public void CancelInvalidSlotsNoRecipientAndStaleOfferLeaveAuthoritativeStateUntouched()
    {
        var session = Session(true); Choose(session, new("receiver"), returned: 0);
        var before = session.Current.Battle;
        foreach (var stage in new[] { BattleSelectionStage.GiveExchangeChoice, BattleSelectionStage.GiveTargetChoice,
            BattleSelectionStage.GiveItemChoice, BattleSelectionStage.ActionChoice })
        { Accept(session, new Cancel()); Assert.Equal(stage, session.Current.Selection!.Stage); Assert.Same(before, session.Current.Battle); }
        Accept(session, new BeginGive()); var selected = session.Current;
        Assert.Equal("give-item-slot", Send(session, new SelectGiveItem(3)).Failure!.Code); Assert.Same(selected, session.Current);
        Accept(session, new Confirm()); Accept(session, new SelectTarget(new("receiver"))); Accept(session, new Confirm()); Accept(session, new Confirm());
        selected = session.Current;
        var receiver = before.GetActor(new("receiver"));
        // The offered equipped word moved/changed in-place without trusting an old selection.
        var changed = before.With(actors: before.Actors.Select(a => a.Actor == receiver.Actor
            ? a.With(sourceLoadout: new([0, 189, 50, 0], a.SourceLoadout!.Spells)) : a));
        var stale = new SessionSnapshot(selected.SessionId, selected.Revision, selected.ObservationSequence,
            new ActiveBattle(changed, selected.Selection), selected.Story, selected.StopReason);
        var rejected = BattleCommandDispatcher.Submit(stale, new Confirm());
        Assert.Equal("stale-give-return", rejected.Failure!.Code); Assert.Same(stale, rejected.Snapshot); Assert.Empty(rejected.Observations);
        session = Session(adjacent: false); Accept(session, new Confirm()); selected = session.Current;
        Assert.Equal("give-no-target", Send(session, new BeginGive()).Failure!.Code); Assert.Same(selected, session.Current);
    }

    [Theory]
    [InlineData(90)] [InlineData(218)] [InlineData(306)]
    public void RestrictedOrBrokenLiveItemsRejectBeforeTransfer(ushort word)
    {
        var session = Session(); var battle = session.Current.Battle; var donor = battle.GetActor(new("champion"));
        battle = battle.With(actors: battle.Actors.Select(a => a == donor ? a.With(sourceLoadout: new([178, word, 50, 127], a.SourceLoadout!.Spells)) : a));
        var error = Assert.Throws<BattleRuleException>(() => BattleGive.Prepare(battle, donor.Actor, donor.Position!, 0, 178, new("receiver"), null, null));
        Assert.True(error.Unsupported); Assert.Equal("give-item-unsupported", error.Code);
    }
}
