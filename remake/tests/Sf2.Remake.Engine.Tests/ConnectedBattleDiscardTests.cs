using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class ConnectedBattleDiscardTests
{
    internal static BattleDefinition Bind(BattleDefinition battle)
    {
        var basis = ConnectedBattleGiveTests.Source();
        var source = new PrivateBattleDefinitions(null!, [], basis.Classes.Values, basis.Items.Values.Concat(
            [new(123, "MITHRIL", "Mithril", "", "NONE", [], 0, 0, 2000, "RARE", "NOTHING", "NOTHING", 0, [new("NONE", 0)]),
             new(113, "SKY_ORB", "Sky Orb", "", "NONE", [], 0, 0, 0, "RARE|UNSELLABLE", "NOTHING", "NOTHING", 0, [new("NONE", 0)])]), [], [], []);
        return PrivateFieldPreparation.Discard(PrivateFieldPreparation.Give(
            PrivateFieldPreparation.Equipment(battle, source, allowBattleChanges: true), PrivateFieldPreparation.Read(source)),
            PrivateFieldPreparation.Read(source));
    }

    private static GameSession Session()
    {
        var doc = Document("battle-give"); doc.AsObject().Remove("battleGive");
        var admitted = Assert.IsType<ScenarioReadAccepted>(Reader(doc).Read());
        var battle = admitted.Definition.Encounters.Values.Single(); Assert.Null(battle.Discard);
        battle = battle.WithDeployments(battle.Deployments.Select(d => d.Faction == BattleFaction.Ally
            ? d with { Definition = d.Definition.WithGrowth(new(0,
                (byte)(d.Definition.Attack - d.Definition.Equipment!.BaseAttack), [], [], new Dictionary<byte, SpellRef>())) } : d));
        var start = admitted.Start;
        var known = new BattleStartInput(start.Encounter, start.Actors.Select(a => a.Actor.Value == "champion"
            ? a with { SourceLoadout = new([178, 0, 123, 0], [63, 63, 63, 63]) } : a), start.MainSeed, start.ThinkingSeed, start.Gold,
            itemCustody: new([], new Dictionary<byte, byte>()));
        return Assert.IsType<SessionStarted>(GameSession.Start(new(admitted.Definition.Package, [Bind(battle)]), known)).Session;
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)]
    public void ProductDropCommitsOnceReturnsSamePreviewAndOuterCancelCannotRestoreIt(int slot)
    {
        var session = Session(); var before = session.Current.Battle;
        FinishMovement(session, Accept(session, new Move(ExplorationDirection.South)));
        Accept(session, new Confirm()); var preview = session.Current.Selection!.Preview;
        Accept(session, new BeginDiscard()); Accept(session, new SelectDiscardItem(slot));
        Accept(session, new Cancel()); Assert.Same(before, session.Current.Battle);
        Accept(session, new BeginDiscard()); Accept(session, new SelectDiscardItem(slot)); Accept(session, new Confirm());
        Accept(session, new Cancel()); Assert.Same(before, session.Current.Battle);
        Accept(session, new BeginDiscard()); Accept(session, new SelectDiscardItem(slot)); Accept(session, new Confirm());
        var selected = session.Current;
        var envelope = new CommandEnvelope(selected.SessionId, selected.Revision, selected.Selection!.Actor, new Confirm());
        var result = session.Submit(envelope); Assert.Null(result.Failure);
        Assert.Single(result.Observations, e => e.Kind == "discard-committed");
        Assert.Equal("stale-input", session.Submit(envelope).Failure!.Code);
        var committed = session.Current.Battle; var wait = session.Current.BattleDiscard!;
        Assert.Equal(before.Cursor, committed.Cursor); Assert.Equal(before.Queue, committed.Queue);
        Assert.Equal(before.MainSeed, committed.MainSeed); Assert.Equal(before.ThinkingSeed, committed.ThinkingSeed);
        Assert.Equal(slot == 0 ? 20 : 25, committed.GetActor(new("champion")).Attack);
        Assert.Equal(slot == 2 ? 1 : 0, committed.ItemCustody!.Deals.GetValueOrDefault((byte)123));
        Assert.Equal(before.ItemCustody!.OnceFlags, committed.ItemCustody.OnceFlags);
        var expected = new ushort[] { 178, 0, 123, 0 }.Where((_, i) => i != slot).Append((ushort)127);
        Assert.Equal(expected, committed.GetActor(new("champion")).SourceLoadout!.Items);
        Accept(session, new CompleteTextReveal(wait.Token)); Accept(session, new Acknowledge(wait.Token));
        Assert.Equal(BattleSelectionStage.ActionChoice, session.Current.Selection!.Stage);
        Assert.Same(preview, session.Current.Selection.Preview); Assert.Same(committed, session.Current.Battle);
        var settled = session.Current; Assert.NotNull(Send(session, new Acknowledge(wait.Token)).Failure); Assert.Same(settled, session.Current);
        FinishMovement(session, Accept(session, new Cancel()));
        Assert.Equal(BattleSelectionStage.Movement, session.Current.Selection!.Stage);
        Assert.Equal(before.GetActor(new("champion")).Position, session.Current.Selection.Preview.Destination);
        Assert.Same(committed.GetActor(new("champion")).SourceLoadout, session.Current.Battle.GetActor(new("champion")).SourceLoadout);
        Assert.Same(committed.ItemCustody, session.Current.Battle.ItemCustody);
    }

    [Theory]
    [InlineData(90)] [InlineData(218)] [InlineData(113)] [InlineData(306)]
    public void UnsupportedCursedProtectedAndBrokenWordsRefuseWithoutMutation(ushort word)
    {
        var session = Session(); var original = session.Current.Battle; var actor = original.GetActor(new("champion"));
        var battle = original.With(actors: original.Actors.Select(a => a == actor
            ? a.With(sourceLoadout: new([178, word, 123, 0], a.SourceLoadout!.Spells)) : a));
        var error = Assert.Throws<BattleRuleException>(() => BattleDiscard.Prepare(battle, actor.Actor, 1, word));
        Assert.True(error.Unsupported); Assert.Equal("discard-item-unsupported", error.Code);
        Assert.Same(original.ItemCustody, battle.ItemCustody);
    }

    [Fact]
    public void ConfirmationRevalidatesChangedSlotAndRareRequiresKnownCustody()
    {
        var session = Session(); Accept(session, new Confirm()); Accept(session, new BeginDiscard()); Accept(session, new Confirm());
        var current = session.Current; var actor = current.Battle.GetActor(new("champion"));
        var changed = current.Battle.With(actors: current.Battle.Actors.Select(a => a == actor
            ? a.With(sourceLoadout: new([50, 0, 123, 0], a.SourceLoadout!.Spells), attack: a.BaseAttack) : a));
        var stale = new SessionSnapshot(current.SessionId, current.Revision, current.ObservationSequence,
            new ActiveBattle(changed, current.Selection), current.Story, current.StopReason);
        var rejected = BattleCommandDispatcher.Submit(stale, new Confirm());
        Assert.Equal("stale-discard-item", rejected.Failure!.Code); Assert.Same(stale, rejected.Snapshot); Assert.Empty(rejected.Observations);
        var b = current.Battle;
        var unknown = new EngineBattleState(b.Definition, b.Actors, b.MainSeed, b.ThinkingSeed, b.Round, b.Queue, b.Cursor, b.Gold);
        Assert.Equal("discard-custody-unavailable", Assert.Throws<BattleRuleException>(() => BattleDiscard.Prepare(unknown, actor.Actor, 2, 123)).Code);
        Assert.Null(unknown.ItemCustody);
    }
}
