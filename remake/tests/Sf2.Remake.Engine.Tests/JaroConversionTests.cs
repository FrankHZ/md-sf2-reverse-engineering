using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class JaroConversionTests
{
    internal static readonly ActorRef Ally = new("ally-jaro"), Enemy = new("enemy-jaro"), Leader = new("leader"), Sentry = new("sentry");
    internal static SessionStarted AtRegion(Action<JsonNode>? change = null)
    {
        var doc = Document("jaro-conversion");
        doc["start"]!["actors"]![0]!["positionOverride"] = new JsonObject { ["x"] = 0, ["y"] = 5 };
        change?.Invoke(doc);
        return Assert.IsType<SessionStarted>(GameSession.Start(Reader(doc)));
    }
    internal static void FinishStory(GameSession session)
    {
        for (int i = 0; i < 20 && session.Current.BattleRound is { Program: true }; i++)
        {
            var wait = session.Current.Story.Wait;
            Accept(session, wait switch
            {
                DialogueWait dialogue => new Acknowledge(dialogue.Token),
                PresentationWait view => new CompletePresentation(view.Token, view.Cue.Kind),
                _ => new AdvanceSimulation(wait?.Token),
            });
        }
        Assert.Null(session.Current.Story.Cursor);
    }

    [Fact]
    public void OrdinaryMovementSceneAutomaticAttackAndPlayerMovementUseOneSession()
    {
        var session = Start("jaro-conversion"); var id = session.Current.SessionId;
        Assert.DoesNotContain(386, session.Current.Story.Flags);
        FinishMovement(session, Accept(session, new Move(ExplorationDirection.North)));
        FinishMovement(session, Accept(session, new Move(ExplorationDirection.North)));
        FinishBattleScenes(session, Stay(session));
        Assert.IsType<PresentationWait>(session.Current.Story.Wait); Assert.Contains(386, session.Current.Story.Flags);
        Assert.Null(session.Current.Selection); Assert.Equal(1, session.Current.Battle.Round);
        var waiting = session.Current; var wait = waiting.Story.Wait!;
        Assert.NotNull(Send(session, new CompletePresentation(wait.Token, PresentationCueKind.Sound)).Failure);
        Assert.Same(waiting, session.Current);
        FinishStory(session);
        Assert.Equal(2, session.Current.Battle.Round); Assert.Equal(Leader, session.Current.Selection!.Actor);
        Assert.Equal(BattleControl.Automatic, session.Current.Battle.GetActor(Ally).Control);
        var result = FinishBattleScenes(session, Stay(session));
        Assert.Contains(result.Observations, o => o.Kind == "ai-commandset6-ally" && o.Actor == Ally);
        Assert.Contains(result.Observations, o => o.Kind == "action-committed" && o.Actor == Ally);
        Assert.True(session.Current.Battle.GetActor(Sentry).Hp < 100);
        Assert.Null(session.Current.Battle.GetActor(Ally).LastTarget);
        var moved = session.Current.Battle.GetActor(Ally).Position!;
        Assert.NotEqual(new MapPosition(3, 2), moved);
        Assert.Equal(moved, session.Current.Battle.Visuals!.Resolve(Ally).Position);
        var departed = session.Current.Battle.With(actors: session.Current.Battle.Actors.Select(a =>
            a.Actor == Ally ? a.With(hp: 0, clearPosition: true) : a));
        var retained = new SessionSnapshot(id, 0, 0, new ActiveBattle(departed, null), session.Current.Story, SessionStopReason.SimulationWait);
        Assert.Equal(moved, retained.Battle.Visuals!.Resolve(Ally).Position);
        Assert.Equal(Leader, session.Current.Selection!.Actor);
        FinishMovement(session, Accept(session, new Move(ExplorationDirection.South)));
        Assert.Equal(6, session.Current.Selection!.Preview.Destination.Y);
        Assert.Equal(id, session.Current.SessionId); Assert.Contains(386, session.Current.Story.Flags);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void ConversionRetainsEachActorsResourcesAndDeadAllyAlias(bool dead)
    {
        var started = AtRegion(doc => { doc["start"]!["actors"]![1]!["hp"] = dead ? 0 : 13; doc["start"]!["actors"]![1]!["mp"] = 1; });
        Assert.Null(started.Result.Failure); var current = started.Session.Current;
        var enemy = current.Battle.GetActor(Enemy); var ally = current.Battle.GetActor(Ally);
        Assert.Equal(0, enemy.Hp); Assert.Null(enemy.Position); Assert.Equal(59, enemy.MaxHp);
        Assert.Equal(dead ? 0 : 13, ally.Hp); Assert.Equal(1, ally.Mp); Assert.Equal(24, ally.MaxHp);
        Assert.Equal(new MapPosition(3, 2), ally.Position); Assert.Equal((ushort)0, ally.ActivationWord);
        Assert.Contains(23, current.Story.Flags); Assert.Contains(55, current.Story.Flags);
        Assert.DoesNotContain(23, current.Story.PartyLists!.Active); Assert.Contains(23, current.Story.PartyLists.Reserve);
        Assert.Equal(current.Battle.Visuals!.Resolve(Enemy), current.Battle.Visuals.Resolve(Ally));
        FinishStory(started.Session);
        Assert.Equal(dead ? 0 : 13, started.Session.Current.Battle.GetActor(Ally).Hp);
        Assert.Contains(23, started.Session.Current.Story.PartyLists!.Active);
        Assert.Equal((ushort)4, started.Session.Current.Battle.GetActor(Ally).ActivationWord);
        Assert.Equal(dead ? (byte)3 : (byte)2, started.Session.Current.Battle.Visuals!.Resolve(Ally).Facing);
        Assert.Equal(!dead, started.Session.Current.Battle.TurnOrder.Any(t => t.Actor == Ally));
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void ChangedRetainedPositionOwnsConvertedPhysicalSlotEvenForDeadAlly(bool dead)
    {
        var started = AtRegion(doc => {
            doc["start"]!["actors"]![2]!["positionOverride"] = new JsonObject { ["x"] = 5, ["y"] = 1 };
            if (dead) doc["start"]!["actors"]![1]!["hp"] = 0;
        });
        Assert.Null(started.Result.Failure);
        Assert.Equal(new MapPosition(5, 1), started.Session.Current.Battle.Visuals!.Resolve(Ally).Position);
        Assert.Equal(new MapPosition(5, 1), started.Session.Current.Battle.GetActor(Ally).Position);
    }

    [Fact]
    public void InitialConflictingPlacedAliasesAreUnsupported()
    {
        var doc = Document("jaro-conversion");
        doc["start"]!["story"]!["aliases"]![2]!["slot"] = 1;
        Assert.Equal("ambiguous-visual-position", Assert.IsType<SessionStartFailed>(GameSession.Start(Reader(doc))).Failure.Code);
    }

    [Fact]
    public void DuplicatePhysicalAppearanceAndWorldCompositionAreExplicitlyUnsupported()
    {
        var doc = Document("jaro-conversion");
        doc["start"]!["story"]!["visuals"]![2]!["appearance"] = "enemy-jaro";
        Assert.Equal("duplicate-visual-appearance", Assert.IsType<SessionStartFailed>(GameSession.Start(Reader(doc))).Failure.Code);
        var world = new JsonObject { ["formatVersion"] = 8, ["package"] = "unsupported-mix",
            ["battle"] = Document("jaro-conversion"), ["world"] = new JsonObject(), ["start"] = new JsonObject() };
        Assert.Equal("world-battle-story", Assert.IsType<SessionStartFailed>(GameSession.Start(Reader(world))).Failure.Code);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void ConversionDoesNotMoveOldAllyPhysicalSlotBeforeAliasCopy(bool invalid)
    {
        var started = AtRegion(doc => {
            doc["start"]!["story"]!["aliases"]!.AsArray().Add(new JsonObject { ["actor"] = "ally-jaro", ["slot"] = 3 });
            doc["start"]!["story"]!["visuals"]!.AsArray().Add(new JsonObject {
                ["slot"] = 3, ["appearance"] = "ally-jaro", ["x"] = 7, ["y"] = 7, ["facing"] = 0 });
            if (invalid) doc["start"]!["story"]!["aliases"]![1]!["slot"] = 255;
        });
        Assert.Equal(new MapPosition(7, 7), started.Session.Current.Battle.Visuals!.Visuals[3].Position);
        Assert.Equal(new MapPosition(3, 2), started.Session.Current.Battle.GetActor(Ally).Position);
        Assert.Equal(invalid ? "battle-visual-alias" : null, started.Result.Failure?.Code);
        Assert.Equal(invalid ? 3 : 1, started.Session.Current.Battle.Visuals.Aliases[Ally]);
    }

    [Fact]
    public void FullTwelveKeepsPreJoinListThenNamedExplicitActivationAndAiRebuildCountThirteen()
    {
        var started = AtRegion(doc => doc["start"]!["story"]!["flags"] = new JsonArray(
            Enumerable.Range(0, 12).Concat(Enumerable.Range(32, 12)).Select(n => JsonValue.Create(n)).ToArray()));
        var before = started.Session.Current;
        Assert.Equal(12, before.Story.PartyLists!.Active.Count); Assert.Contains(55, before.Story.Flags);
        Assert.Contains(23, before.Story.PartyLists.Reserve);
        Assert.DoesNotContain(started.Result.Observations, o => o.Kind == "join-force-conditional-active");
        FinishStory(started.Session);
        Assert.Equal(13, started.Session.Current.Story.PartyLists!.Active.Count);
        Assert.Equal(Enumerable.Range(0, 12).Append(23), started.Session.Current.Story.PartyLists.Active);
    }

    [Fact]
    public void InvalidRetainedAliasFailsAtCopyAfterLogicalAndMembershipWrites()
    {
        var started = AtRegion(doc => doc["start"]!["story"]!["aliases"]![1]!["slot"] = 255);
        Assert.Equal("battle-visual-alias", started.Result.Failure!.Code);
        var current = started.Session.Current;
        Assert.Equal(new ProgramLocation("conversion", 0), current.Story.Cursor);
        Assert.Contains(386, current.Story.Flags); Assert.Contains(55, current.Story.Flags);
        Assert.Equal(0, current.Battle.GetActor(Enemy).Hp); Assert.Equal(new MapPosition(3, 2), current.Battle.GetActor(Ally).Position);
        Assert.Equal((ushort)0, current.Battle.GetActor(Ally).ActivationWord); Assert.Equal(0, current.Battle.Round);
        Assert.Equal("conversion-explicit-active", started.Result.Observations.Last().Kind);
        Assert.Equal("session-stopped", Send(started.Session, new AdvanceSimulation()).Failure!.Code);
        Assert.Same(current, started.Session.Current);
    }

    [Fact]
    public void AlreadyJoinedRequiresExplicitRetainedEnemyAndUsesItsCoordinates()
    {
        var doc = Document("jaro-conversion"); doc["start"]!["story"]!["flags"]!.AsArray().Add(23);
        var failure = Assert.IsType<SessionStartFailed>(GameSession.Start(Reader(doc)));
        Assert.Equal("retained-conversion-source-required", failure.Failure.Code);
        doc["start"]!["actors"]![0]!["positionOverride"] = new JsonObject { ["x"] = 0, ["y"] = 5 };
        doc["start"]!["story"]!["retainedSourceSlots"]!.AsArray().Add(143);
        doc["start"]!["actors"]![2]!["positionOverride"] = new JsonObject { ["x"] = 3, ["y"] = 1 };
        doc["start"]!["actors"]![2]!["hp"] = 7;
        var session = Assert.IsType<SessionStarted>(GameSession.Start(Reader(doc))).Session;
        Assert.Equal(new MapPosition(3, 1), session.Current.Battle.GetActor(Ally).Position);
        Assert.Equal(24, session.Current.Battle.GetActor(Ally).Hp);
    }

    [Fact]
    public void ZeroAiValueDoesNotJoinOrRebuildAndNonzeroPreservesEveryOtherBit()
    {
        var input = Admitted("jaro-conversion"); var battle = BattleTurnFlow.Start(input.Definition.Encounters[input.Start.Encounter], input.Start);
        battle = battle.With(actors: battle.Actors.Select(a => a.Actor == Ally ? a.With(activationWord: 0xA6F0) : a));
        var story = new StoryState([0, 32], new("conversion", 0), partyLists: new([], [], []));
        var current = new SessionSnapshot(Guid.NewGuid(), 0, 0, new ActiveBattle(battle, null), story, SessionStopReason.SimulationWait);
        List<SessionObservation> observations = [];
        var on = BattleStoryOperations.JoinAi(current, new(23, 7), observations);
        Assert.Equal((ushort)0xA6F4, on.Battle.GetActor(Ally).ActivationWord); Assert.Contains(23, on.Story.Flags);
        var lists = on.Story.PartyLists; observations.Clear();
        var off = BattleStoryOperations.JoinAi(on, new(23, 0), observations);
        Assert.Equal((ushort)0xA6F0, off.Battle.GetActor(Ally).ActivationWord); Assert.Same(lists, off.Story.PartyLists);
        Assert.Single(observations); Assert.Equal("join-force-ai-word", observations[0].Kind);
        Assert.Equal(BattleControl.Player, off.Battle.GetActor(Ally).Control);
    }

    [Fact]
    public void ChangedActorIdsSlotsVisualAndProgramDataStillConvertAndPlay()
    {
        var text = Document("jaro-conversion").ToJsonString().Replace("ally-jaro", "traveler").Replace("enemy-jaro", "old-guard");
        var doc = JsonNode.Parse(text)!;
        doc["start"]!["actors"]![0]!["positionOverride"] = new JsonObject { ["x"] = 1, ["y"] = 4 };
        var slots = doc["encounters"]![0]!["initialization"]!["sourceCombatants"]!;
        slots[1]!["slot"] = 19; slots[2]!["slot"] = 130;
        doc["start"]!["story"]!["aliases"]![1]!["slot"] = 7;
        doc["start"]!["story"]!["visuals"]![1]!["slot"] = 7;
        doc["battleStory"]!["programs"]![0]!["instructions"]![0]!["enemySlot"] = 130;
        doc["battleStory"]!["programs"]![0]!["instructions"]![0]!["allySlot"] = 19;
        doc["battleStory"]!["programs"]![0]!["instructions"]![4]!["allySlot"] = 19;
        doc["battleStory"]!["texts"]![0]!["text"] = "My choice is made.";
        var started = Assert.IsType<SessionStarted>(GameSession.Start(Reader(doc)));
        Assert.Null(started.Result.Failure); Assert.Contains(19, started.Session.Current.Story.Flags);
        Assert.Contains(51, started.Session.Current.Story.Flags); Assert.Equal(7, started.Session.Current.Story.CameraEntitySlot);
        FinishStory(started.Session);
        Assert.Equal(BattleControl.Automatic, started.Session.Current.Battle.GetActor(new("traveler")).Control);
        Assert.Equal(0, started.Session.Current.Battle.GetActor(new("old-guard")).Hp);
    }

    [Fact]
    public void RetainedDeadEnemyAndDistinctStatusItemsAndActivationAreNotNormalizedByConversion()
    {
        var input = Admitted("jaro-conversion"); var b = BattleTurnFlow.Start(input.Definition.Encounters[input.Start.Encounter], input.Start);
        var allyItems = new BattleSourceLoadout([0, 127, 127, 127], [2, 63, 63, 63]);
        var enemyItems = new BattleSourceLoadout([1, 2, 3, 4], [5, 6, 7, 8]);
        b = b.With(actors: b.Actors.Select(a => a.Actor == Ally ? a.With(hp: 11, mp: 1, status: 0xC0, activationWord: 0x80,
                sourceLoadout: allyItems) : a.Actor == Enemy ? a.With(hp: 0, mp: 0, position: new(3, 4), status: 7,
                activationWord: 0x2083, sourceLoadout: enemyItems) : a));
        var current = new SessionSnapshot(Guid.NewGuid(), 0, 0, new ActiveBattle(b, null), new StoryState([386]), SessionStopReason.SimulationWait);
        // This controlled retained state includes the actual last physical placement.
        current = new(current.SessionId, 0, 0, new ActiveBattle(b.With(visuals: b.Visuals!.Place(Enemy, new(3, 4))), null), current.Story, current.StopReason);
        var converted = BattleStoryOperations.Convert(current, new(143, 23), []);
        var ally = converted.Battle.GetActor(Ally); var enemy = converted.Battle.GetActor(Enemy);
        Assert.Equal(new MapPosition(3, 4), ally.Position); Assert.Equal(11, ally.Hp); Assert.Equal(1, ally.Mp);
        Assert.Equal(0xC0, ally.Status); Assert.Equal((ushort)0x80, ally.ActivationWord); Assert.Same(allyItems, ally.SourceLoadout);
        Assert.Equal(59, enemy.MaxHp); Assert.Equal(7, enemy.Status); Assert.Equal((ushort)0x2083, enemy.ActivationWord);
        Assert.Same(enemyItems, enemy.SourceLoadout); Assert.Null(enemy.Position);
        var queue = BattleTurnFlow.GenerateQueue(converted.Battle, out var generated);
        Assert.Contains(generated.Candidates, a => a.Actor == Ally && a.Hp == 11 && a.Placed && a.Agility == ally.Agility);
        Assert.DoesNotContain(queue.TurnOrder, a => a.Actor == Enemy);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void FirstOrSecondRegionUsesOnlyFirstEligibleRowAndNoHpGuard(bool secondOnly)
    {
        var input = Admitted("jaro-conversion"); var old = input.Definition.Encounters[input.Start.Encounter]; var init = old.Initialization!;
        var battle = new BattleDefinition(old.Encounter, old.Map, old.Width, old.Height, old.Terrain, old.Deployments,
            old.Spells.Values, old.Rewards, new(init.Regions, BattleRegionProgram.Authored, sourceCombatants: init.SourceCombatants,
                enemyCount: init.EnemyCount, regionPrograms: [new(0, 386, "first"), new(1, 386, "second")]));
        var b = BattleTurnFlow.Start(battle, input.Start); var flags = new bool[16]; flags[0] = !secondOnly; flags[1] = true;
        b = b.With(regions: new(flags, 3), actors: b.Actors.Select(a => a.Actor == Enemy ? a.With(hp: 0, clearPosition: true) : a));
        var current = new SessionSnapshot(Guid.NewGuid(), 0, 0, new ActiveBattle(b, null), new StoryState(), SessionStopReason.SimulationWait);
        var result = BattleRoundContinuation.Begin(current, []);
        Assert.Equal(new ProgramLocation(secondOnly ? "second" : "first", 0), result.Snapshot.Story.Cursor);
        Assert.Contains(386, result.Snapshot.Story.Flags); Assert.Equal(0, result.Snapshot.Battle.Round);
    }
}
