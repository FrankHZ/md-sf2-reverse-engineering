using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class MapTransferPolicyTests
{
    [Fact]
    public void PublicJsonControlledStartRunsDestinationOnLoadBeforeLaterPolicySelection()
    {
        var document = Document("harbor-arrival");
        document["start"]!["flags"] = new JsonArray(13, 21);
        document["start"]!["program"] = JsonNode.Parse("""{"program":"transfer-control","instruction":0}""");
        document["world"]!["programs"]!.AsArray().Add(JsonNode.Parse("""
            {"id":"transfer-control","instructions":[{"op":"wait-ticks","ticks":1},
             {"op":"transfer","map":"yard-map","position":{"x":1,"y":1},"facing":1,"loadMode":"rebuild"},{"op":"end"}]}
            """));
        var sf2 = RuleCompositions.Sf2();
        var policy = new ReplayPolicy(true);
        var rules = new SessionRules("selected", sf2.Healing, sf2.Physical,
            RuleCompositions.SourceItem(), RuleCompositions.SourceStay(), sourceStory: policy);
        var started = Assert.IsType<SessionStarted>(GameSession.Start(Reader(document), rules));
        Assert.Null(started.Result.Failure);
        var session = started.Session;
        Assert.Null(session.Current.Exploration!.Party.NewBattle);
        var result = Step(session);
        Assert.Equal(new MapId("yard-map"), session.Current.Exploration!.Map);
        Assert.Contains(14, session.Current.Story.Flags); // Destination on-load precedes later admission.
        Assert.Contains(15, session.Current.Story.Flags);
        Assert.Equal(new ProgramLocation("before-battle", 2), session.Current.Story.Cursor);
        Assert.IsType<DialogueWait>(session.Current.Story.Wait);
        Assert.Equal(1, policy.Calls);
        Assert.True(result.Observations.Single(row => row.Kind == "map-transferred").Sequence <
            result.Observations.Single(row => row.Kind == "battle-selected").Sequence);
    }

    [Fact]
    public void QueuedWarpAlsoUsesSelectedRepeatPolicyBeforeDestinationOnLoad()
    {
        var policy = new ReplayPolicy(true);
        var session = Start(false, policy, queued: true);
        var result = Step(session);
        AssertBefore(session);
        Assert.Contains(701, session.Current.Story.Flags); // Queue is consumed after its caller ends.
        Assert.DoesNotContain(result.Observations, row => row.Kind == "map-transferred");
        Assert.True(policy.Calls > 0);
        FinishEntry(session, callerContinued: true);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void CompletedTransferUsesSelectedPolicyBeforeDestinationOnLoad(bool sourceDefaultRepeats, bool repeat)
    {
        var policy = new ReplayPolicy(repeat);
        var session = Start(sourceDefaultRepeats, policy);
        Assert.NotNull(session.Current.Exploration!.Party.NewBattle);
        Assert.Equal(sourceDefaultRepeats, RuleCompositions.Sf2().SourceStory.EnterCompletedBattle(session.Definition));
        var result = Step(session);
        Assert.Contains(700, session.Current.Story.Flags);
        Assert.Contains(702, session.Current.Story.Flags);
        Assert.DoesNotContain(701, session.Current.Story.Flags);
        Assert.Contains(503, session.Current.Story.Flags);
        if (repeat)
        {
            AssertBefore(session);
            Assert.Equal(1, policy.Calls);
            Assert.DoesNotContain(result.Observations, row => row.Kind == "map-transferred");
            FinishEntry(session);
        }
        else
        {
            Assert.Equal(new MapId("destination"), session.Current.Exploration!.Map);
            Assert.Equal(1, policy.Calls);
            Assert.Equal(new ProgramLocation("on-load", 1), session.Current.Story.Cursor);
            Assert.Contains(800, session.Current.Story.Flags);
            Assert.Contains(403, session.Current.Story.Flags);
            Assert.DoesNotContain(399, session.Current.Story.Flags);
            Assert.DoesNotContain(710, session.Current.Story.Flags);
            Assert.Null(session.Current.Story.EnteringBattle);
            Assert.DoesNotContain(result.Observations, row => row.Kind == "battle-selected");
            Step(session);
            Assert.Equal(2, policy.Calls);
            Assert.True(session.Current.CanWaitAtInput);
            Assert.Null(session.Current.Story.Cursor);
            Assert.Null(session.Current.Story.EnteringBattle);
            Assert.Contains(701, session.Current.Story.Flags);
            Assert.DoesNotContain(403, session.Current.Story.Flags);
            Assert.DoesNotContain(453, session.Current.Story.Flags);
            Assert.DoesNotContain(720, session.Current.Story.Flags);
            Assert.DoesNotContain(730, session.Current.Story.Flags);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void UncompletedTransferDoesNotConsultReplayPolicy(bool sourceDefaultRepeats)
    {
        var policy = new ReplayPolicy(false) { Throw = true };
        var session = Start(sourceDefaultRepeats, policy, completed: false);
        Step(session);
        Assert.Equal(0, policy.Calls);
        AssertBefore(session, completed: false);
        FinishEntry(session);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ExplicitSourceDefaultCompositionKeepsItsReplayConvention(bool sourceDefaultRepeats)
    {
        var session = Start(sourceDefaultRepeats, null);
        Step(session);
        if (sourceDefaultRepeats) { AssertBefore(session); FinishEntry(session); }
        else
        {
            Assert.Contains(800, session.Current.Story.Flags);
            Step(session);
            Assert.True(session.Current.CanWaitAtInput);
            Assert.DoesNotContain(403, session.Current.Story.Flags);
            Assert.Null(session.Current.Story.EnteringBattle);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ThrowingPolicyRetainsCommittedPrefixAndRetriesOnlyReachedTransfer(bool sourceDefaultRepeats)
    {
        var policy = new ReplayPolicy(true) { Throw = true };
        var session = Start(sourceDefaultRepeats, policy);
        var initial = session.Current;
        var failure = Send(session, new AdvanceSimulation(initial.Story.Wait!.Token));
        Assert.NotNull(failure.Failure);
        Assert.Equal(SessionFailureKind.InvariantFailure, failure.Failure!.Kind);
        Assert.Equal("battle-policy-failure", failure.Failure.Code);
        Assert.Equal("rules", failure.Failure.Field);
        Assert.Equal("selected-replay: completed-battle-admission", failure.Failure.Message);
        Assert.Equal(SessionStopReason.Faulted, failure.StopReason);
        Assert.Equal(new ProgramLocation("transfer", 3), session.Current.Story.Cursor);
        Assert.Equal(new MapId("origin"), session.Current.Exploration!.Map);
        Assert.Contains(700, session.Current.Story.Flags);
        Assert.Contains(702, session.Current.Story.Flags);
        Assert.Contains(403, session.Current.Story.Flags);
        Assert.Contains(503, session.Current.Story.Flags);
        Assert.DoesNotContain(399, session.Current.Story.Flags);
        Assert.DoesNotContain(800, session.Current.Story.Flags);
        Assert.DoesNotContain(710, session.Current.Story.Flags);
        Assert.Null(session.Current.Story.EnteringBattle);
        var before = initial.Exploration!.Party;
        var retained = session.Current.Exploration.Party;
        Assert.Equal(before.Actors, retained.Actors);
        Assert.Equal(before.Gold, retained.Gold);
        Assert.Equal(before.MainSeed, retained.MainSeed);
        Assert.Equal(before.ThinkingSeed, retained.ThinkingSeed);
        Assert.Equal(before.NewBattle, retained.NewBattle);
        Assert.DoesNotContain(failure.Observations, row => row.Kind is "battle-selected" or "map-transferred" or "battle-initialized");
        var stopped = session.Current;
        var retry = Send(session, new AdvanceSimulation());
        Assert.Equal(failure.Failure, retry.Failure);
        Assert.Equal(stopped.Story.Cursor, session.Current.Story.Cursor);
        Assert.Equal(stopped.Story.Flags, session.Current.Story.Flags);
        // A typed retry owns its ordinary simulation service, but replays no committed instruction.
        Assert.Equal(stopped.Revision + 1, session.Current.Revision);
        Assert.Equal(stopped.Story.SimulationTick + 1, session.Current.Story.SimulationTick);
        Assert.Equal("simulation-tick", Assert.Single(retry.Observations).Kind);
        Assert.Equal(2, policy.Calls);
        policy.Throw = false;
        var resumed = Accept(session, new AdvanceSimulation());
        AssertBefore(session);
        Assert.Equal(3, policy.Calls);
        Assert.DoesNotContain(resumed.Observations, row => row.Kind == "program-instruction" && row.Detail == "WriteFlag" && row.Program == new ProgramLocation("transfer", 2));
        FinishEntry(session);
    }

    private static SessionResult Step(GameSession session) => Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));

    private static void AssertBefore(GameSession session, bool completed = true)
    {
        Assert.Equal(new MapId("origin"), session.Current.Exploration!.Map);
        Assert.Equal(new ProgramLocation("before", 1), session.Current.Story.Cursor);
        Assert.IsType<TickWait>(session.Current.Story.Wait);
        Assert.Equal("encounter", session.Current.Story.EnteringBattle!.Encounter);
        Assert.Equal(ProgramContinuation.BeforeBattleFinished, session.Current.Story.Continuation);
        Assert.Equal(!completed, session.Current.Story.Flags.Contains(403));
        Assert.Contains(399, session.Current.Story.Flags);
        Assert.Contains(710, session.Current.Story.Flags);
        Assert.DoesNotContain(800, session.Current.Story.Flags);
        Assert.DoesNotContain(720, session.Current.Story.Flags);
        Assert.DoesNotContain(730, session.Current.Story.Flags);
        Assert.DoesNotContain(453, session.Current.Story.Flags);
    }

    private static void FinishEntry(GameSession session, bool callerContinued = false)
    {
        var initialized = Step(session);
        Assert.Equal(SessionMode.Battle, session.Current.Mode);
        Assert.Equal(new ProgramLocation("load", 1), session.Current.Story.Cursor);
        Assert.Contains(711, session.Current.Story.Flags);
        Assert.Contains(720, session.Current.Story.Flags);
        Assert.DoesNotContain(730, session.Current.Story.Flags);
        Assert.DoesNotContain(453, session.Current.Story.Flags);
        Assert.Single(initialized.Observations, row => row.Kind == "battle-initialized");
        Step(session);
        Assert.Equal(new ProgramLocation("start", 1), session.Current.Story.Cursor);
        Assert.Contains(721, session.Current.Story.Flags);
        Assert.Contains(730, session.Current.Story.Flags);
        Assert.Contains(453, session.Current.Story.Flags);
        Step(session);
        Assert.Contains(731, session.Current.Story.Flags);
        Assert.Null(session.Current.Story.Cursor);
        for (int delivered = 0; session.Current.BattleRound is { Actor: not null } round && delivered < 4; delivered++)
            Accept(session, new CompletePresentation(round.Token, round.CompletionKind));
        Assert.True(session.Current.HasBattleControl);
        Assert.DoesNotContain(800, session.Current.Story.Flags);
        Assert.Equal(callerContinued, session.Current.Story.Flags.Contains(701));
    }

    // Project-authored initialized state, not private imported content or original-game evidence.
    private static GameSession Start(bool sourceDefaultRepeats, ReplayPolicy? policy, bool completed = true, bool queued = false)
    {
        BattleActorDefinition Unit(string id, bool ally) => new(new(id),
            ally ? BattleClassRule.UnpromotedSwordsman : BattleClassRule.Ordinary, 1, 20, 0, 10, 4, (byte)(ally ? 50 : 5), false, 4, [],
            physical: new(PhysicalCriticalRule.OneIn32WithHalfBonus, false, ally, 0),
            sourceLoadout: new([127, 127, 127, 127], [63, 63, 63, 63]));
        var hero = new BattleDeploymentDefinition(Unit("hero", true), BattleFaction.Ally, 0, BattleControl.Player, null,
            new(12, 12), new(null, 0, 15, 15, 0, 0, 255, 255, AllyPartyMember: 0));
        var enemy = new BattleDeploymentDefinition(Unit("enemy", false), BattleFaction.Enemy, 128, BattleControl.Automatic,
            new("source-orders"), new(3, 3), new(0x2000, 0, 15, 15, 0, 6, 255, 255));
        var battle = new BattleDefinition("encounter", new("destination"), 16, 16,
            Enumerable.Range(0, 2304).Select(_ => new BattleTerrain(TerrainSurface.Open, TerrainProtection.None)), [hero, enemy], [],
            initialization: new([], BattleRegionProgram.None, Enumerable.Range(0, 12).Select(i => new MapPosition(12 + i % 2, 4 + i / 2))));
        var input = new BattleStartInput("encounter", [new(hero.Actor, 20, 0, 17, 3, 2, 0, null, ActivationWord: 0),
            new(enemy.Actor, 20, 0, null, null, null, 0, null)], 0x12340000, 0x56780000, 987,
            new("controlled", "engine behavior", "No original runtime claim", 0, true, true, true, false, false, 0));
        var party = new BattleStartInput(input.Encounter, input.Actors.Where(row => row.Actor == hero.Actor),
            input.MainSeed, input.ThinkingSeed, input.Gold, input.NewBattle);
        var layout = new WorkingMapLayout(new ushort[4096]);
        var origin = new ExplorationMapDefinition(new("origin"), layout, new([new(0, 0, 63, 63)]), [], []);
        var destination = new ExplorationMapDefinition(new("destination"), layout, origin.Traversal, [], [], new("on-load", 0),
            new("encounter", 403, 503, 453, new("before", 0), new("start", 0), new("load", 0)));
        StoryProgram Hook(string id, int flag) => new(id, [new WriteFlag(flag, true), new WaitProgramTicks(1), new WriteFlag(flag + 1, true), new EndProgram()]);
        StoryInstruction transfer = queued ? new QueueMapWarp(destination.Map, new(4, 4), 2) :
            new TransferToMap(destination.Map, new(4, 4), 2, MapLoadMode.Rebuild);
        StoryProgram[] programs = [new("transfer", [new WriteFlag(700, true), new WaitProgramTicks(1), new WriteFlag(702, true),
            transfer, new WriteFlag(701, true), new EndProgram()]),
            new("on-load", [new WriteFlag(800, true), new WaitProgramTicks(1), new EndProgram()]),
            Hook("before", 710), Hook("load", 720), Hook("start", 730)];
        PrivateBattleDefinitions? sourceDefinitions = null;
        if (sourceDefaultRepeats)
        {
            var source = new EncounterSource("authored-test", "controlled", "synthetic", "not-source-evidence");
            var encounter = new BattleEncounterDefinition(source, source, new(2, "synthetic"), new(0, 0, 0),
                new(66, 0, 0, 16, 16, 255, 255), new("none", false, false), [], [], [], new(0, 0, 0), "", "", [], new Dictionary<byte, int>());
            sourceDefinitions = new(encounter, [], [], [], [], [], []);
        }
        var definition = new ScenarioDefinition("initialized-transfer", [battle], sourceDefinitions,
            new([origin, destination], programs), encounterStarts: [input]);
        var sf2 = RuleCompositions.Sf2();
        var rules = policy is null ? sf2 : new SessionRules("selected", sf2.Healing, sf2.Physical,
            RuleCompositions.SourceItem(), RuleCompositions.SourceStay(), sourceStory: policy);
        var started = Assert.IsType<SessionStarted>(GameSession.Start(definition,
            new ExplorationStartInput(origin.Map, new("traveler"), new(4, 4), 0, 32,
                completed ? [403, 503] : [403], party, new("transfer", 0)), rules));
        Assert.Null(started.Result.Failure);
        return started.Session;
    }

    private sealed class ReplayPolicy(bool repeat) : ISourceStoryPolicy
    {
        public string Identity => "selected-replay";
        internal int Calls { get; private set; }
        internal bool Throw { get; set; }
        public bool EnterCompletedBattle(ScenarioDefinition definition)
        {
            Calls++;
            if (Throw) throw new InvalidOperationException("private detail");
            return repeat;
        }
        public SourceStoryCandidate Apply(ScenarioDefinition definition, SessionSnapshot current,
            SourceStoryInstruction instruction, ProgramLocation location) => throw new InvalidOperationException();
    }
}
