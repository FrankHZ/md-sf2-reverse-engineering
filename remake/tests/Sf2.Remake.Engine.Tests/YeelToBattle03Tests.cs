using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class YeelToBattle03Tests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void FirstRequestIgnoresVisitFollowerAndPrecedingCompletion(bool follower, bool precedingCompleted)
    {
        var (definition, current) = Field(flags: [.. follower ? new[] { 72 } : [], .. precedingCompleted ? new[] { 502 } : []]);
        var before = current.Exploration!.Party;
        var requested = SourceRandomBattleRunner.Check(current, new(3, new("request", 0)), new("caller", 0), []);
        Assert.Contains(403, requested.Story.Flags); Assert.DoesNotContain(4, requested.Story.Flags);
        Assert.True(requested.Story.BattleRequest); Assert.Equal((short)30000, requested.Story.StepCounter);
        Assert.Same(before, requested.Exploration!.Party);
        Assert.Equal(new ProgramLocation("request", 0), requested.Story.Cursor);
        Assert.Equal(new ProgramLocation("caller", 1), Assert.Single(requested.Story.Callers));
        var selected = SourceRandomBattleRunner.Consume(definition, requested.WithStory(requested.Story.Copy(null)), [], RuleCompositions.Sf2());
        Assert.Equal("battle-3", selected.Story.EnteringBattle!.Encounter);
        Assert.False(selected.Story.BattleRequest);
        Assert.Same(requested.Exploration, selected.Exploration);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1)]
    [InlineData(30000)]
    public void CompletedNonzeroCounterDefersWithoutRng(short steps)
    {
        var (_, current) = Field([503]);
        current = current.WithStory(current.Story.Copy(null, stepCounter: steps));
        var result = SourceRandomBattleRunner.Check(current, new(3, new("request", 0)), new("caller", 0), []);
        Assert.False(result.Story.BattleRequest); Assert.DoesNotContain(403, result.Story.Flags);
        Assert.Equal(steps, result.Story.StepCounter); Assert.Same(current.Exploration!.Party, result.Exploration!.Party);
    }

    [Theory]
    [InlineData(0u, true)]
    [InlineData(0x40000000u, false)]
    public void CompletedZeroCounterUsesOrderedMainDraws(uint seed, bool requests)
    {
        var (_, current) = Field([503], seed);
        current = current.WithStory(current.Story.Copy(null, stepCounter: 0));
        var first = BattleRandom.NextMain(seed, 8);
        var result = SourceRandomBattleRunner.Check(current, new(3, new("request", 0)), new("caller", 0), []);
        Assert.Equal(requests, result.Story.BattleRequest);
        var second = BattleRandom.NextMain(first.After, 4);
        Assert.Equal(requests ? first.After : second.After, result.Exploration!.Party.MainSeed);
        Assert.Equal(requests ? (short)30000 : (short)(second.Value + 2), result.Story.StepCounter);
        Assert.Equal(current.Exploration!.Party.ThinkingSeed, result.Exploration.Party.ThinkingSeed);
        Assert.Equal(current.Exploration.Party.Actors, result.Exploration.Party.Actors);
    }

    [Fact]
    public void UnknownCompletedCounterRejectsWithoutPublishingARequest()
    {
        var (_, current) = Field([503]);
        var failure = Assert.Throws<BattleRuleException>(() => SourceRandomBattleRunner.Check(current,
            new(3, new("request", 0)), new("caller", 0), []));
        Assert.Equal("random-battle-step-unbound", failure.Code);
        Assert.DoesNotContain(403, current.Story.Flags); Assert.False(current.Story.BattleRequest);
    }

    [Fact]
    public void FlashRetainsControlUntilActualCompletionAndRestoresUnchangedLogicalDisplay()
    {
        var (definition, current) = Field();
        var display = new ExplorationDisplay(3, new(0xE84, 0x24), new(0xE84, 0x24), FullFadeVisibility.BaseRestored);
        var flash = new StoryProgram("flash", [new PresentCue(PresentationCueKind.FlashWhite,
            "white-four-pulses", DurationTicks: 32), new EndProgram()]);
        definition = new(definition.Package, definition.Encounters.Values,
            exploration: new(definition.Exploration!.Maps.Values, [flash]));
        current = current.WithStory(current.Story.Copy(new("flash", 0), display: display, stepCounter: 7, egressMap: new("village")));
        var waiting = ProgramRunner.Run(definition, current, []);
        Assert.Null(waiting.Failure);
        var wait = Assert.IsType<PresentationWait>(waiting.Snapshot.Story.Wait);
        Assert.Equal(PresentationCueKind.FlashWhite, wait.Cue.Kind);
        Assert.Same(waiting.Snapshot, ProgramRunner.Run(definition, waiting.Snapshot, []).Snapshot);
        var done = ExplorationDispatcher.Submit(definition, waiting.Snapshot, new CompletePresentation(wait.Token, wait.Cue.Kind));
        Assert.Null(done.Failure); Assert.Null(done.Snapshot.Story.Wait);
        Assert.Same(display, done.Snapshot.Story.Display);
        Assert.Equal((short)7, done.Snapshot.Story.StepCounter); Assert.Equal("village", done.Snapshot.Story.EgressMap!.Value);
        Assert.Same(current.Exploration!.Party, done.Snapshot.Exploration!.Party);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EarlierEligibleEncounterWinsEvenWhenItIsCompleted(bool completed)
    {
        var (definition, current) = Field([402, 403, .. completed ? new[] { 502 } : []]);
        var sf2 = RuleCompositions.Sf2();
        var repeat = new SessionRules("repeat", sf2.Healing, sf2.Physical, RuleCompositions.SourceItem(),
            RuleCompositions.SourceStay(), sourceStory: new RepeatPolicy());
        var route = BattleEntry.First(definition, current.Story, current.Exploration!.Definition, repeat);
        Assert.Equal("battle-2", route!.Encounter);
        var result = BattleEntry.Select(current, current.Story, route, []);
        Assert.Equal(!completed, result.Story.Flags.Contains(402));
        Assert.Contains(403, result.Story.Flags);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReachedBeforeProgramOwnsEgressAndStartOwnsIntroFlag(bool seen)
    {
        var (definition, current) = Field([403, .. seen ? new[] { 453 } : []]);
        var selected = BattleEntry.Select(current, current.Story.Copy(null, egressMap: new("old")),
            current.Exploration!.Definition.Battles[1], []);
        var result = ProgramRunner.Run(definition, selected, []);
        Assert.Null(result.Failure);
        Assert.Equal(seen ? "old" : "village", result.Snapshot.Story.EgressMap!.Value);
        Assert.Contains(453, result.Snapshot.Story.Flags);
        Assert.Equal(SessionMode.Battle, result.Snapshot.Mode);
        Assert.Single(result.Snapshot.Battle.Actors, actor => actor.IsAlly);
        Assert.DoesNotContain(4, result.Snapshot.Story.Flags);
    }

    [Theory]
    [InlineData(9)]
    [InlineData(10)]
    [InlineData(28)]
    public void EarlierZoneRowsSuppressDefaultEvenWhenTheirProgramIsEmpty(int y)
    {
        var (_, current) = Field();
        var words = new ushort[4096]; words[y * 64 + 5] = 0x1400;
        var map = new ExplorationMapDefinition(new("zone"), new(words), new([new(0, 0, 63, 63)]), [],
            [new(ExplorationEventKind.SourceZone, null, y, null, new("empty", 0), RequiredMarker: 0x1400),
             new(ExplorationEventKind.SourceZone, null, null, null, new("default", 0), RequiredMarker: 0x1400)]);
        var world = new ExplorationState(map, map.Layout, new("entity-0"),
            [new(new("entity-0"), EntityMotionState.At(new(4, y), 0, 32), true)], current.Exploration!.Party);
        var reached = MapEventDispatcher.Move(world, ExplorationDirection.East, []).Outcome;
        Assert.Equal(new ProgramLocation("empty", 0), reached.Event!.Program);
        words[y * 64 + 5] = 0;
        var ordinary = world.WithLayout(new(words));
        Assert.Null(MapEventDispatcher.Move(ordinary, ExplorationDirection.East, []).Outcome.Event);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MovingCounterDecrementsOnceAtSuccessfulLaunchAndNeverForABlockedMove(bool blocked)
    {
        var (definition, current) = Field();
        if (blocked)
        {
            var words = new ushort[4096]; words[4 * 64 + 5] = 0xC000;
            current = new(current.SessionId, current.Revision, current.ObservationSequence,
                new ActiveExploration(current.Exploration!.WithLayout(new(words))), current.Story, current.StopReason);
        }
        current = current.WithStory(current.Story.Copy(null, stepCounter: 2, egressMap: new("village")));
        var requested = ExplorationDispatcher.Submit(definition, current, new Move(ExplorationDirection.East), RuleCompositions.Sf2());
        Assert.Null(requested.Failure); Assert.Equal((short)2, requested.Snapshot.Story.StepCounter);
        var result = requested;
        for (int tick = 0; tick < 20; tick++)
        {
            result = ExplorationDispatcher.Submit(definition, result.Snapshot,
                new AdvanceSimulation(result.Snapshot.Story.Wait?.Token), RuleCompositions.Sf2());
            Assert.Null(result.Failure);
        }
        Assert.Equal(blocked ? (short)2 : (short)1, result.Snapshot.Story.StepCounter);
        Assert.Equal("village", result.Snapshot.Story.EgressMap!.Value);
    }

    [Fact]
    public void FollowingReentryAllocatesOneKazinWithoutJoining()
    {
        var (_, current) = Field();
        var population = new ExplorationPopulation(30, 128, 0, [new(72, 4, 11)]);
        var map = new ExplorationMapDefinition(new("village"), current.Exploration!.Layout,
            new([new(0, 0, 63, 63)]), [new(new("entity-4"), new(7, 6), 3, 32, true, false, Sprite: 4)], [], population: population);
        foreach (var flags in new int[][] { [], [72], [72, 0, 32] })
        {
            var world = MapTransfer.Build(map, new("entity-0"), new(7, 7), 1, 32, current.Exploration.Party, flags);
            var kazin = Assert.Single(world.AllEntities, entity => entity.Entity == new EntityRef("entity-4"));
            Assert.Equal(flags.Contains(72), kazin.Follower is not null);
            Assert.Equal(flags.Contains(72) ? new MapPosition(7, 7) : new MapPosition(7, 6), kazin.Position);
            Assert.Same(current.Exploration.Party, world.Party);
            Assert.DoesNotContain(4, flags);
        }
    }

    [Fact]
    public void SynchronousNarrationKeepsItsCallerAndRealInputThenRestoresTheNextSpeaker()
    {
        var session = ExplorationTextWaitTests.StartFieldText("{CLEAR}Narrator{W1}{CLEAR}", second: "Speaker{W1}", interaction: true,
            configure: document =>
            {
                var text = document["world"]!["programs"]![0]!["instructions"]![1]!;
                text["speaker"] = null; text["narration"] = true;
            });
        var caller = session.Current.Story.EntityEvent;
        Assert.NotNull(caller);
        ReachInput(session);
        var first = Assert.IsType<OpenTextWindow>(session.Current.Story.TextWindow);
        Assert.Null(first.Speaker); Assert.IsType<ClosedPortraitWindow>(session.Current.Story.PortraitWindow);
        Assert.False(session.Current.CanWaitAtInput);
        EngineTestContent.Accept(session, new Acknowledge(session.Current.Story.Wait!.Token));
        Assert.Equal(FieldTextPhase.ClearToken, Assert.IsType<FieldTextWait>(session.Current.Story.Wait).Phase);
        Assert.Equal("", Assert.IsType<FieldTextWait>(session.Current.Story.Wait).Projection);
        ReachInput(session);
        Assert.Equal(new EntityRef("ferryman"), Assert.IsType<OpenTextWindow>(session.Current.Story.TextWindow).Speaker);
        Assert.Equal(caller!.Entity, session.Current.Story.EntityEvent!.Entity);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NarrationCannotHideAnExplicitOrEventSpeaker(bool eventSpeaker)
    {
        var session = ExplorationTextWaitTests.StartFieldText("A{W1}");
        var text = new ShowText(TextDisplayMode.Single, eventSpeaker ? null : new("missing"),
            UseEventSpeaker: eventSpeaker, ExplicitWindows: true, Narration: true);
        var failure = Assert.Throws<BattleRuleException>(() => ExplorationTextRunner.Begin(session.Definition.Exploration!, session.Current, text, new(999)));
        Assert.Equal("narration-speaker", failure.Code);
    }

    private static void ReachInput(GameSession session)
    {
        for (int step = 0; step < 250; step++)
        {
            if (session.Current.CanWaitForText) return;
            if (session.Current.Story.Wait is FieldTextWait { LogicalDone: true, Revealed: false } text)
                EngineTestContent.Accept(session, new CompleteTextReveal(text.Token));
            else EngineTestContent.Accept(session, new AdvanceSimulation(session.Current.Story.Wait?.Token));
        }
        Assert.Fail("No bounded synchronous text input.");
    }

    [Fact]
    public void OrdinaryHerbHasTheSameActualAiDecisionAndRngAsEmptyInventory()
    {
        var (empty, start) = Battle("battle-3");
        var ordinary = WithEnemyItem(empty, 0);
        var before = SourceEnemyAi.Resolve(BattleTurnFlow.Start(empty, start), new("enemy"));
        var after = SourceEnemyAi.Resolve(BattleTurnFlow.Start(ordinary, start), new("enemy"));
        Assert.Equal(before.Destination, after.Destination);
        Assert.Equal(before.Path, after.Path); Assert.Equal(before.Effects, after.Effects);
        Assert.Equal(before.Battle.MainSeed, after.Battle.MainSeed); Assert.Equal(before.Battle.ThinkingSeed, after.Battle.ThinkingSeed);
        Assert.Equal((ushort)0, after.Battle.GetActor(new("enemy")).SourceLoadout!.Items[0]);
    }

    [Theory]
    [InlineData(128)]
    [InlineData(0x2000)]
    [InlineData(1)]
    public void EnabledEquippedAndUnknownEnemyItemsRemainUnsupported(ushort item)
    {
        var (definition, start) = Battle("battle-3");
        var error = Assert.Throws<BattleRuleException>(() => BattleTurnFlow.Start(WithEnemyItem(definition, item), start));
        Assert.True(error.Unsupported || error.Code == "source-loadout");
    }

    private static BattleDefinition WithEnemyItem(BattleDefinition definition, ushort item)
    {
        var deployments = definition.Deployments.Select(row => row.Faction == BattleFaction.Ally ? row : row with
        {
            Definition = Unit("enemy", false, item),
        });
        return new(definition.Encounter, definition.Map, definition.Width, definition.Height, definition.Terrain,
            deployments, [], initialization: definition.Initialization);
    }

    private sealed class RepeatPolicy : ISourceStoryPolicy
    {
        public string Identity => "repeat";
        public bool EnterCompletedBattle(ScenarioDefinition definition) => true;
        public SourceStoryCandidate Apply(ScenarioDefinition definition, SessionSnapshot current,
            SourceStoryInstruction instruction, ProgramLocation location) => throw new InvalidOperationException();
    }

    private static BattleActorDefinition Unit(string id, bool ally, ushort item = 127) => new(new(id),
        ally ? BattleClassRule.UnpromotedSwordsman : BattleClassRule.Ordinary, 1, 20, 0, 10, 4, 5, false, 4, [],
        physical: new(PhysicalCriticalRule.OneIn32WithHalfBonus, false, ally, 0), sourceLoadout: new([item, 127, 127, 127], [63, 63, 63, 63]));

    private static (BattleDefinition, BattleStartInput) Battle(string id)
    {
        var hero = new BattleDeploymentDefinition(Unit("hero", true), BattleFaction.Ally, 0, BattleControl.Player, null,
            new(12, 12), new(null, 0, 15, 15, 0, 0, 255, 255, AllyPartyMember: 0));
        var enemy = new BattleDeploymentDefinition(Unit("enemy", false), BattleFaction.Enemy, 128, BattleControl.Automatic,
            new("source-orders"), new(3, 3), new(0x2000, 0, 15, 15, 0, 6, 255, 255));
        var terrain = Enumerable.Range(0, 2304).Select(_ => new BattleTerrain(TerrainSurface.Open, TerrainProtection.None));
        var definition = new BattleDefinition(id, new("field"), 16, 16, terrain, [hero, enemy], [],
            initialization: new([], BattleRegionProgram.None, Enumerable.Range(0, 12).Select(i => new MapPosition(12 + i % 2, 4 + i / 2))));
        return (definition, new(id, [new(hero.Actor, 20, 0, 17, 3, 2, 0, null, ActivationWord: 0),
            new(enemy.Actor, 20, 0, null, null, null, 0, null)], 0x12340000, 0x56780000, 987,
            new("controlled", "engine behavior", "No original runtime claim", 0, true, true, true, false, false, 0)));
    }

    private static (ScenarioDefinition, SessionSnapshot) Field(int[]? flags = null, uint seed = 0x12340000)
    {
        var (second, party) = Battle("battle-2"); var (third, thirdStart) = Battle("battle-3");
        party = new(party.Encounter, party.Actors.Where(actor => actor.Actor.Value == "hero"), seed, party.ThinkingSeed, party.Gold, party.NewBattle);
        var layout = new WorkingMapLayout(new ushort[4096]);
        var map = new ExplorationMapDefinition(new("field"), layout, new([new(0, 0, 63, 63)]), [], [],
            battles: [new("battle-2", 402, 502, 452, null, null), new("battle-3", 403, 503, 453, new("before", 0), null)]);
        StoryProgram[] programs = [new("before", [new SetEgressMap(new("village")), new EndProgram()]), new("request", [new EndProgram()])];
        var definition = new ScenarioDefinition("zone", [second, third], exploration: new([map], programs), encounterStarts: [party, thirdStart]);
        var world = new ExplorationState(map, layout, new("entity-0"), [new(new("entity-0"), EntityMotionState.At(new(4, 4), 0, 32), true)], party);
        return (definition, new(Guid.NewGuid(), 0, 0, new ActiveExploration(world), new StoryState(flags), SessionStopReason.PlayerInput));
    }
}
