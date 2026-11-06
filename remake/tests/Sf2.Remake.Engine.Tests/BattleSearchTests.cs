using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Gameplay.Sf2;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Content.Persistence;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleSearchTests
{
    [Theory]
    [InlineData(0, 2, 2)] [InlineData(1, 1, 1)] [InlineData(2, 0, 2)] [InlineData(3, 1, 3)]
    public void FacingUsesPreviewDestinationAndPreservesOpenedOffer(byte facing, int x, int y)
    {
        var session = Start("battle-search"); var current = session.Current; var actor = current.Selection!.Actor;
        var chest = new BattleChest(new(x, y), 700, 0, "Herb", null);
        var definition = current.Battle.Definition.WithSearch(new([chest]));
        var old = current.Battle;
        var battle = new EngineBattleState(definition, old.Actors.Select(a => a.Actor == actor ? a.With(facing: facing) : a),
            old.MainSeed, old.ThinkingSeed, old.Round, old.Queue, old.Cursor, old.Gold);
        Assert.Equal(chest, BattleSearch.Faced(battle, actor, new(1, 2)));
        Assert.Null(BattleSearch.Faced(battle, actor, new(3, 1)));
        var snapshot = new SessionSnapshot(current.SessionId, 1, 1,
            new ActiveBattle(battle, new(actor, BattleMovement.Preview(battle, actor, new(1, 2)), BattleSelectionStage.ActionChoice)),
            current.Story.Copy(null, flags: [700]), SessionStopReason.PlayerInput);
        Assert.Equal(chest, BattleSearchContinuation.Query(snapshot));
        Assert.Equal(BattleSearchKind.Empty, BattleSearch.Prepare(battle, actor, chest, true).Kind);
    }

    [Fact]
    public void CancelAndWrongFacingDoNotGrantAndDeliveredMovementChoosesNewChest()
    {
        var session = Start("battle-search"); var initial = session.Current;
        Accept(session, new Confirm()); Accept(session, new BeginBattleSearch());
        Assert.Equal(BattleSelectionStage.SearchCommitReady, session.Current.Selection!.Stage);
        Accept(session, new Cancel());
        Assert.Equal(initial.Battle, session.Current.Battle); Assert.Empty(session.Current.Story.Flags);
        FinishMovement(session, Accept(session, new Cancel()));
        FinishMovement(session, Accept(session, new Move(ExplorationDirection.East)));
        Accept(session, new Confirm());
        Assert.Null(session.QueryBattleChoices().Search);
        Assert.NotNull(Send(session, new BeginBattleSearch()).Failure);
        FinishMovement(session, Accept(session, new Cancel()));
        FinishMovement(session, Accept(session, new Move(ExplorationDirection.East)));
        FinishMovement(session, Accept(session, new Move(ExplorationDirection.South)));
        Assert.Equal(new MapPosition(1, 2), session.Current.Battle.GetActor(initial.Selection!.Actor).Position);
        Accept(session, new Confirm());
        Assert.Equal(701, session.QueryBattleChoices().Search!.Flag);
        Accept(session, new BeginBattleSearch()); var result = Accept(session, new Confirm());
        Assert.Contains(result.Observations, o => o.Kind == "battle-search-receipt" && o.After == 701);
    }

    [Fact]
    public void GrantIsAtomicUniqueAndActualStaySuffixFollowsOnlyAcknowledgement()
    {
        var session = Start("battle-search"); var before = session.Current;
        Accept(session, new Confirm()); Accept(session, new BeginBattleSearch());
        var envelope = new CommandEnvelope(session.Current.SessionId, session.Current.Revision, session.Current.Selection!.Actor, new Confirm());
        var result = session.Submit(envelope); Assert.Null(result.Failure);
        var actor = before.Selection!.Actor; var committed = session.Current;
        Assert.Equal(new ushort[] { 0, 189, 113, 0 }, committed.Battle.GetActor(actor).SourceLoadout!.Items);
        Assert.Equal(new[] { 700 }, committed.Story.Flags);
        Assert.Equal(before.Battle.Cursor, committed.Battle.Cursor); Assert.Equal(before.Battle.MainSeed, committed.Battle.MainSeed);
        Assert.Single(result.Observations, o => o.Kind == "battle-search-receipt");
        Assert.DoesNotContain(result.Observations, o => o.Kind is "after-turn" or "action-committed");
        Assert.NotNull(session.Submit(envelope).Failure); Assert.Same(committed, session.Current);
        Assert.NotNull(Send(session, new Confirm()).Failure); Assert.NotNull(Send(session, new Cancel()).Failure);
        var wait = committed.BattleSearch!;
        Assert.NotNull(Send(session, new Acknowledge(wait.Token)).Failure);
        Accept(session, new CompleteTextReveal(wait.Token));
        Assert.NotNull(Send(session, new CompleteTextReveal(wait.Token)).Failure);
        result = Accept(session, new Acknowledge(wait.Token));
        Assert.Single(result.Observations, o => o.Kind == "action-committed" && o.Actor == actor);
        Assert.Single(result.Observations, o => o.Kind == "after-turn" && o.Actor == actor);
        Assert.NotNull(Send(session, new Acknowledge(wait.Token)).Failure);
        Assert.Equal(before.Battle.ItemCustody!.OnceFlags, session.Current.Battle.ItemCustody!.OnceFlags);
        Assert.Equal(before.Battle.ItemCustody.Deals, session.Current.Battle.ItemCustody.Deals);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void FullOrOccupiedReturnsFreeWithoutUsingOtherMemberCapacity(bool occupied)
    {
        var session = Start("battle-search", doc =>
        {
            if (occupied) { doc["encounters"]![0]!["placements"]![1]!["x"] = 1; doc["encounters"]![0]!["placements"]![1]!["y"] = 3; }
            else doc["actors"]![0]!["items"] = new JsonArray(0, 189, 113, 0);
        });
        Accept(session, new Confirm()); var before = session.Current;
        Accept(session, new BeginBattleSearch()); var result = Accept(session, new Confirm());
        Assert.Equal(occupied ? BattleSearchKind.Occupied : BattleSearchKind.Full, session.Current.BattleSearch!.Kind);
        Assert.Same(before.Battle, session.Current.Battle); Assert.Empty(session.Current.Story.Flags);
        Assert.DoesNotContain(result.Observations, o => o.Kind == "battle-search-receipt");
        result = Dismiss(session);
        Assert.Equal(BattleSelectionStage.ActionChoice, session.Current.Selection!.Stage);
        Assert.Equal(before.Selection!.Preview.Destination, session.Current.Selection.Preview.Destination);
        Assert.Same(before.Battle, session.Current.Battle);
        Assert.DoesNotContain(result.Observations, o => o.Kind is "action-committed" or "after-turn");
        Accept(session, new BeginBattleSearch()); Accept(session, new Confirm()); Dismiss(session);
        Assert.Empty(session.Current.Story.Flags);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void EmptyAndGoldConsumeTurnAndGoldSaturates(bool gold)
    {
        var session = Start("battle-search", doc =>
        {
            doc["start"]!["gold"] = 999990;
            doc["encounters"]![0]!["search"]!["chests"]![0]!["item"] = null;
            doc["encounters"]![0]!["search"]!["chests"]![0]!["gold"] = gold ? JsonValue.Create(30) : null;
        });
        var words = session.Current.Battle.Actors[0].SourceLoadout!.Items;
        Accept(session, new Confirm()); Accept(session, new BeginBattleSearch()); Accept(session, new Confirm());
        Assert.Equal(gold ? BattleSearchKind.Gold : BattleSearchKind.Empty, session.Current.BattleSearch!.Kind);
        Assert.Equal(gold ? 999999U : 999990U, session.Current.Battle.Gold);
        Assert.Equal(words, session.Current.Battle.Actors[0].SourceLoadout!.Items);
        Assert.Contains(Dismiss(session).Observations, o => o.Kind == "action-committed");
    }

    [Fact]
    public void StableV2RestoreKeepsReceiptAndDoesNotRegrant()
    {
        var (basis, initial) = BattleSaveFixture.Create(); var old = initial.Battle;
        var encounter = old.Definition.WithSearch(new([new(new(4, 6), 700, 0, "Herb", null)]));
        var definition = new ScenarioDefinition(basis.Package, basis.Encounters.Values.Select(e => e.Encounter == encounter.Encounter ? encounter : e),
            exploration: basis.Exploration, encounterStarts: basis.EncounterStarts.Values) { Persistence = new Sf2OpeningRestore() };
        var battle = new EngineBattleState(encounter, old.Actors.Select(a => a.Actor == initial.Selection!.Actor
            ? a.With(sourceLoadout: new([127, 127, 127, 127], a.SourceLoadout!.Spells)) : a), old.MainSeed, old.ThinkingSeed,
            old.Round, old.Queue, old.Cursor, old.Gold, old.StartPolicy, old.Regions, itemCustody: old.ItemCustody, deathCredit: old.DeathCredit);
        var data = BattleSaveFixture.Capture(definition, BattleSaveFixture.At(battle, initial.Story));
        var session = Assert.IsType<SessionStarted>(GameSession.RestoreBattle(definition, data, RuleCompositions.Sf2())).Session;
        Accept(session, new Confirm()); Accept(session, new BeginBattleSearch());
        Assert.NotNull(session.QueryBattleSave().Error);
        Accept(session, new Confirm()); Assert.NotNull(session.QueryBattleSave().Error);
        var result = FinishBattleScenes(session, Dismiss(session));
        for (int guard = 0; session.Current.StopReason != SessionStopReason.PlayerInput && guard < 100; guard++)
            result = FinishBattleScenes(session, Accept(session, new AdvanceSimulation()));
        Assert.Equal(BattleSelectionStage.Movement, session.Current.Selection!.Stage);
        var saved = BattleSaveFixture.Capture(definition, session.Current);
        var wire = OpeningSaveJson.WriteRecord(new BattleSaveRecord(1, 1, DateTimeOffset.UnixEpoch, saved));
        var record = Assert.IsType<BattleSaveRecord>(OpeningSaveJson.ReadRecord(wire, 1));
        var restored = Assert.IsType<SessionStarted>(SaveRecordRestore.Restore(definition, record, session.Rules)).Session;
        Assert.Contains(700, restored.Current.Story.Flags);
        Assert.Equal(session.Current.Battle.GetActor(initial.Selection!.Actor).SourceLoadout!.Items,
            restored.Current.Battle.GetActor(initial.Selection.Actor).SourceLoadout!.Items);
        Assert.Equal(initial.Selection.Actor, restored.Current.Selection!.Actor);
        Accept(restored, new Confirm()); Accept(restored, new BeginBattleSearch());
        var repeated = Accept(restored, new Confirm());
        Assert.Equal(BattleSearchKind.Empty, restored.Current.BattleSearch!.Kind);
        Assert.DoesNotContain(repeated.Observations, o => o.Kind == "battle-search-receipt");
        Assert.Equal(new ushort[] { 0, 127, 127, 127 }, restored.Current.Battle.GetActor(initial.Selection.Actor).SourceLoadout!.Items);
        Assert.Contains(Dismiss(restored).Observations, o => o.Kind == "action-committed");
    }

    [Fact]
    public void SuffixCallbackFailureRetainsGrantReceiptAndWaitWithoutRetry()
    {
        var rules = new SessionRules("search-failure", new Sf2HealingRule(), RuleCompositions.SourcePhysical(),
            RuleCompositions.SourceItem(), RuleCompositions.SourceStay(), outcome: new FailedSuffix());
        var session = Assert.IsType<SessionStarted>(GameSession.Start(Reader(Document("battle-search")), rules)).Session;
        Accept(session, new Confirm()); Accept(session, new BeginBattleSearch()); Accept(session, new Confirm());
        var committed = session.Current.Battle; var token = session.Current.BattleSearch!.Token;
        Accept(session, new CompleteTextReveal(token));
        var failed = Send(session, new Acknowledge(token));
        Assert.Equal(SessionStopReason.Faulted, failed.StopReason);
        Assert.Equal("battle-policy-failure", failed.Failure!.Code);
        Assert.Same(committed, session.Current.Battle); Assert.Contains(700, session.Current.Story.Flags);
        Assert.Equal(0, committed.GetActor(new("champion")).SourceLoadout!.Items[3]);
        Assert.Equal(token, session.Current.BattleSearch!.Token);
        Assert.Equal("session-stopped", Send(session, new Acknowledge(token)).Failure!.Code);
        Assert.Same(committed, session.Current.Battle);
    }

    private sealed class FailedSuffix : IBattleOutcomeRule
    {
        public string Identity => "fail-after-search";
        public BattleOutcomeKind? Check(EngineBattleState battle) => null;
        public bool DefeatedHook(EngineBattleState battle) =>
            battle.GetActor(new("champion")).SourceLoadout!.Items[3] == 0 ? throw new InvalidOperationException("suffix failed") : false;
    }

    internal static SessionResult Dismiss(GameSession session)
    {
        var token = session.Current.BattleSearch!.Token;
        Accept(session, new CompleteTextReveal(token)); return Accept(session, new Acknowledge(token));
    }
}
