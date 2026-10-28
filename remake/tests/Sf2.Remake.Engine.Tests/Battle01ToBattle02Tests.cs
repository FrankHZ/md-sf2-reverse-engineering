using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Xunit;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Security.Cryptography;

namespace Sf2.Remake.Engine.Tests;

public sealed class Battle01ToBattle02Tests
{
    [Theory]
    [InlineData(7)]
    [InlineData(13)]
    public void NextEncounterCarriesAllyAccountingAndSeedsThroughSourceResourceRefresh(ushort hp)
    {
        var (battle, defaults) = Setup();
        var (fieldDefinition, field) = Field([new EndProgram()]);
        var carried = defaults.Actors.Select(actor => actor.Actor.Value == "hero"
            ? actor with { Hp = hp, Exp = 17, Kills = 9, Defeats = 4, PositionOverride = new(1, 1) }
            : actor with { Hp = 0 }).ToArray();
        var party = new BattleStartInput("previous", carried, 0x1234abcd, 0x5678fedc, 987, defaults.NewBattle);
        var definition = new ScenarioDefinition("carried", [battle], exploration: fieldDefinition.Exploration,
            encounterStarts: [defaults]);
        var route = new ExplorationBattleRoute(battle.Encounter, 402, 502, 452, null, null);
        var current = new SessionSnapshot(field.SessionId, field.Revision, field.ObservationSequence,
            new ActiveExploration(field.Exploration!.WithParty(party)),
            field.Story.Copy(null, enteringBattle: route, continuation: ProgramContinuation.BeforeBattleFinished),
            SessionStopReason.SimulationWait);
        var entered = BattleEntry.Continue(definition, current, [], Sf2.Remake.Application.Gameplay.RuleCompositions.Sf2());
        var hero = entered.Battle.GetActor(new("hero"));
        Assert.Equal((ushort)20, hero.Hp); // Source new-battle setup heals living members.
        Assert.Equal(hp, carried.Single(actor => actor.Actor.Value == "hero").Hp);
        Assert.Equal((byte)17, hero.Exp);
        Assert.Equal((ushort)9, hero.Kills); Assert.Equal((ushort)4, hero.Defeats);
        Assert.Equal(new MapPosition(12, 12), hero.Position);
        Assert.All(entered.Battle.Actors.Where(actor => !actor.IsAlly), actor => Assert.Equal((ushort)20, actor.Hp));
        Assert.Equal(party.MainSeed, entered.Battle.MainSeed); Assert.Equal(party.ThinkingSeed, entered.Battle.ThinkingSeed);
        Assert.Equal(party.Gold, entered.Battle.Gold);
        Assert.DoesNotContain(452, entered.Story.Flags);
    }

    [Theory]
    [InlineData(false, false, "default")]
    [InlineData(true, false, "first")]
    [InlineData(false, true, "second")]
    [InlineData(true, true, "second")]
    public void SelectedSetupUsesLastMatchingSourceVariantWithoutChangingMapLayout(bool first, bool second, string expected)
    {
        var (_, initial) = Field([new EndProgram()]);
        var original = initial.Exploration!.Definition;
        var population = new ExplorationPopulation(0, 128, 0, []);
        var map = new ExplorationMapDefinition(original.Map, original.Layout, original.Traversal, [], [],
            onLoad: new("default", 0), setup: new(new("default"), [new(new("501"), new("first")), new(new("609"), new("second"))]),
            setups: [new(new("first"), [], [], new("first", 0), null, population),
                new(new("second"), [], [], new("second", 0), null, population)]);
        var selected = map.SelectSetup([.. first ? new[] { 501 } : [], .. second ? new[] { 609 } : []]);
        Assert.Equal(expected, selected.OnLoad!.Value.Program);
        Assert.Same(original.Layout, selected.Layout);
        Assert.Equal(original.Map, selected.Map);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void ExternalFieldInputBindsOnlyAChangedPublishedMapAndPreservesExplicitMusic(bool roundTrip, bool explicitAfter)
    {
        var session = ExplorationTextWaitTests.StartFieldText("A", configure: document =>
        {
            var world = document["world"]!;
            byte[] pcm = [0, 0, 0, 0];
            JsonNode Audio(string cue, int command, int? end) => JsonSerializer.SerializeToNode(new
            {
                cue, command, timerB = 192, sampleRate = 8000, channels = 1, sampleFrames = 2,
                pcm16 = Convert.ToBase64String(pcm), sha256 = Convert.ToHexString(SHA256.HashData(pcm)),
                loopBegin = (int?)null, loopEnd = (int?)null, modernEndStep = end,
            })!;
            var audio = new JsonArray(Audio("town", 8, null), Audio("explicit", 9, null), Audio("join", 19, 505));
            audio[0]!.AsObject().Remove("modernEndStep"); audio[1]!.AsObject().Remove("modernEndStep");
            world["presentation"]!["audio"] = audio;
            foreach (var visual in world["presentation"]!["maps"]!.AsArray())
                visual!["music"] = JsonNode.Parse("""[{"field":8,"battle":8}]""");
            foreach (var map in world["maps"]!.AsArray())
            { map!["onLoad"] = null; map["battle"] = null; map["events"] = new JsonArray(); }
            string origin = world["maps"]![0]!["id"]!.GetValue<string>();
            string target = world["maps"]![1]!["id"]!.GetValue<string>();
            JsonNode Transfer(string map) => JsonSerializer.SerializeToNode(new
                { op = "transfer", map, position = new { x = 1, y = 1 }, facing = 1, loadMode = "rebuild" })!;
            JsonNode Sound(string cue) => JsonSerializer.SerializeToNode(new
                { op = "present", kind = "Sound", resource = cue, entity = (string?)null, position = (object?)null })!;
            var instructions = new JsonArray(Sound("town"), Transfer(target));
            if (roundTrip) instructions.Add(Transfer(origin));
            if (explicitAfter) instructions.Add(Sound("explicit"));
            instructions.Add(JsonNode.Parse("""{"op":"end"}"""));
            world["programs"]![0]!["instructions"] = instructions;
        });
        var sound = Assert.IsType<PresentationWait>(session.Current.Story.Wait);
        EngineTestContent.Accept(session, new CompletePresentation(sound.Token, sound.Cue.Kind));
        if (explicitAfter)
        {
            sound = Assert.IsType<PresentationWait>(session.Current.Story.Wait);
            EngineTestContent.Accept(session, new CompletePresentation(sound.Token, sound.Cue.Kind));
        }
        Assert.True(session.Current.CanWaitAtInput);
        var before = session.Current.Story.Music!;
        Assert.Equal(explicitAfter, before.HistoryBound);
        var result = EngineTestContent.Accept(session, new WaitAtInput());
        var current = session.Current;
        Assert.Equal(!explicitAfter && !roundTrip, result.Observations.Any(row => row.Kind == "map-music-bound"));
        if (explicitAfter || roundTrip) Assert.Same(before, current.Story.Music);
        var joined = ExplorationMusicRunner.Request(session.Definition.Exploration!, current, "join", new(9000))!;
        if (roundTrip && !explicitAfter)
        { Assert.Null(joined.EndStep); Assert.Empty(joined.Previous); }
        else
        { Assert.Equal(505, joined.EndStep); Assert.Equal(new[] { explicitAfter ? "explicit" : "town" }, joined.Previous); }
    }

    [Theory]
    [InlineData(0, 0, "SDMN", "Leader")]
    [InlineData(1, 2, "WARR", "Mara")]
    public void JoinMessageCapturesMemberClassAndPreservesTheScriptCursorUntilCallerReturn(int member, byte classId, string role, string name)
    {
        var (definition, initial) = JoinMessage(member, classId, role);
        var shown = ProgramRunner.Run(definition, initial, []);
        Assert.Null(shown.Failure);
        var wait = Assert.IsType<FieldTextWait>(shown.Snapshot.Story.Wait);
        Assert.Equal(name + " the " + role, string.Concat(wait.Units.Select(unit => unit.Text)));
        Assert.Equal(100, wait.Text);
        Assert.Equal(664, shown.Snapshot.Story.TextCursor);
        Assert.Contains(member, shown.Snapshot.Story.Flags);
        Assert.Contains(32 + member, shown.Snapshot.Story.Flags);
        Assert.DoesNotContain(402, shown.Snapshot.Story.Flags);
        Assert.Contains(66, shown.Snapshot.Story.Flags);
        var party = shown.Snapshot.Exploration!.Party;
        Assert.Equal(initial.Exploration!.Party.MainSeed, party.MainSeed);
        Assert.Equal(initial.Exploration.Party.Gold, party.Gold);
        Assert.Single(party.Actors, actor => actor.Actor == new ActorRef("hero"));
        var current = shown.Snapshot;
        for (int step = 0; step < 2000 && !current.Story.Flags.Contains(402); step++)
        {
            SessionCommand command = current.Story.Wait switch
            {
                FieldTextWait { LogicalDone: true, Revealed: false } text => new CompleteTextReveal(text.Token),
                DialogueWait dialogue => new Acknowledge(dialogue.Token),
                { } pending => new AdvanceSimulation(pending.Token),
                _ => throw new InvalidOperationException("Join caller returned without its flag."),
            };
            var next = ExplorationDispatcher.Submit(definition, current, command);
            Assert.Null(next.Failure);
            current = next.Snapshot;
        }
        Assert.Contains(402, current.Story.Flags);
        Assert.DoesNotContain(66, current.Story.Flags);
        Assert.Equal(664, current.Story.TextCursor);
    }

    [Fact]
    public void MissingJoinClassRetainsCommittedMembershipAndDoesNotExecuteTheCaller()
    {
        var (definition, initial) = JoinMessage(1, 2, null);
        var failed = ProgramRunner.Run(definition, initial, []);
        Assert.Equal("join-text-class", failed.Failure!.Code);
        Assert.Contains(1, failed.Snapshot.Story.Flags);
        Assert.Contains(33, failed.Snapshot.Story.Flags);
        Assert.DoesNotContain(402, failed.Snapshot.Story.Flags);
        Assert.Contains(66, failed.Snapshot.Story.Flags);
        Assert.Equal(1, failed.Snapshot.Story.Cursor!.Value.Instruction);
        var before = failed.Snapshot.Exploration!.Party;
        var repeated = ProgramRunner.Run(definition, failed.Snapshot.WithStory(
            failed.Snapshot.Story.Copy(new("join-message", 0))), []);
        Assert.Equal("join-text-class", repeated.Failure!.Code);
        Assert.Same(before, repeated.Snapshot.Exploration!.Party);
        Assert.Single(before.Actors, actor => actor.Actor == new ActorRef("hero"));
    }

    private static (ScenarioDefinition, SessionSnapshot) JoinMessage(int member, byte classId, string? className)
    {
        var field = ExplorationTextWaitTests.StartFieldText("{NAME} the {CLASS}", name: "Mara", configure: document =>
            document["world"]!["programs"]![0]!["instructions"] = System.Text.Json.Nodes.JsonNode.Parse(
                """[{"op":"wait-ticks","ticks":1},{"op":"end"}]"""));
        var (original, start) = Setup();
        var deployments = original.Deployments.Select(actor => actor.Faction != BattleFaction.Ally ? actor :
            actor with { Definition = actor.Definition.WithGrowth(new(classId, 0, [], [], new Dictionary<byte, SpellRef>())),
                Initialization = actor.Initialization! with { AllyPartyMember = member } }).ToArray();
        var battle = new BattleDefinition(original.Encounter, original.Map, original.Width, original.Height,
            original.Terrain, deployments, [], initialization: original.Initialization);
        var worldDefinition = field.Definition.Exploration!;
        var programs = worldDefinition.Programs.Values.Append(new("join-message",
            [new JoinPartyMember(member), new ShowText(TextDisplayMode.Single, null, WaitForAcknowledgement: false,
                ExplicitWindows: true, NameMember: member, TextId: 100), new WaitForTextInput(), new CloseText(),
                new WriteFlag(402, true), new WriteFlag(66, false), new EndProgram()]));
        var exploration = new ExplorationDefinition(worldDefinition.Maps.Values, programs, worldDefinition.Texts,
            partyFlags: new(30, 0, 32, 12), visuals: worldDefinition.Visuals, memberNames: worldDefinition.MemberNames,
            textTokens: worldDefinition.TextTokens, textFont: worldDefinition.TextFont);
        var source = new EncounterSource("authored-test", "controlled", "synthetic", "not-source-evidence");
        var encounter = new BattleEncounterDefinition(source, source, new(2, "synthetic"), new(0, 0, 0),
            new(66, 0, 0, 16, 16, 255, 255), new("none", false, false), [], [], [], new(0, 0, 0), "", "", [], new Dictionary<byte, int>());
        var definitions = new PrivateBattleDefinitions(encounter, [],
            className is null ? [] : [new(classId, className, "\"" + className + "\"", 5, "", "REGULAR", "")], [], [], [], []);
        var definition = new ScenarioDefinition("join-text", [battle], definitions, exploration, encounterStarts: [start]);
        var party = new BattleStartInput(start.Encounter, [], 0x12340000, 0x56780000, 75, start.NewBattle);
        var active = new ActiveExploration(field.Current.Exploration!.WithParty(party));
        return (definition, new(field.Current.SessionId, field.Current.Revision, field.Current.ObservationSequence, active,
            field.Current.Story.Copy(new("join-message", 0), flags: [66], textCursor: 664), SessionStopReason.SimulationWait));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void CompletedWarpUsesTheSelectedStoryPolicyAndConsumesTheUnlockOnlyOnce(bool authored, bool completed)
    {
        var (initialDefinition, initial) = Field([new EndProgram()]);
        var old = initial.Exploration!;
        var route = new ExplorationBattleRoute("varied", 402, 502, 452, new("before", 0), null);
        var target = new ExplorationMapDefinition(new("next-field"), old.Layout, old.Definition.Traversal,
            [], [], battle: route);
        var source = new EncounterSource("authored-test", "controlled", "synthetic", "not-source-evidence");
        var encounter = new BattleEncounterDefinition(source, source, new(2, "synthetic"), new(0, 0, 0),
            new(66, 0, 0, 16, 16, 255, 255), new("none", false, false), [], [], [], new(0, 0, 0), "", "", [], new Dictionary<byte, int>());
        var definition = new ScenarioDefinition("completed-entry", initialDefinition.Encounters.Values,
            new(encounter, [], [], [], [], [], []),
            new([old.Definition, target], [new("before", [new WaitProgramTicks(1), new EndProgram()])]));
        var rules = authored ? Sf2.Remake.Application.Gameplay.RuleCompositions.AuthoredStory() :
            Sf2.Remake.Application.Gameplay.RuleCompositions.Sf2();
        initial = initial.WithStory(initial.Story.Copy(null, flags: completed ? [402, 502] : [402]));
        var transferred = MapTransfer.Apply(definition, initial, target.Map, new(4, 4), 3,
            MapLoadMode.Rebuild, initial.Story, [], rules);
        bool selected = !authored || !completed;
        if (selected)
        {
            Assert.Same(old, transferred.Exploration);
            Assert.Equal(route, transferred.Story.EnteringBattle);
            Assert.Equal(ProgramContinuation.BeforeBattleFinished, transferred.Story.Continuation);
            Assert.Equal(!completed, transferred.Story.Flags.Contains(402));
            Assert.Equal(new ProgramLocation("before", 0), transferred.Story.Cursor);
        }
        else
        {
            Assert.Equal(target.Map, transferred.Exploration!.Map);
            var returned = BattleEntry.Continue(definition, transferred, [], rules);
            Assert.Equal(ProgramContinuation.FieldInput, returned.Story.Continuation);
            Assert.DoesNotContain(402, returned.Story.Flags);
            Assert.Null(returned.Story.EnteringBattle);
        }
        Assert.Equal(completed, transferred.Story.Flags.Contains(502));
        // A subsequent selection with the source-cleared unlock cannot re-enter.
        var locked = initial.WithStory(initial.Story.Copy(null, flags: [502]));
        var next = MapTransfer.Apply(definition, locked, target.Map, new(4, 4), 3, MapLoadMode.Rebuild, locked.Story, [], rules);
        var field = BattleEntry.Continue(definition, next, [], rules);
        Assert.Null(field.Story.EnteringBattle);
        Assert.Equal(ProgramContinuation.FieldInput, field.Story.Continuation);
    }

    [Fact]
    public void MotionWaitReturnsWithTheInstalledFollowerScriptIntact()
    {
        var (definition, initial) = Field([new WaitForEntityMotion(new("entity-128")), new WriteFlag(901, true), new EndProgram()]);
        var script = initial.Exploration!.Entities[new("entity-128")].Actions;
        var returned = ProgramRunner.Run(definition, initial, []);
        Assert.Null(returned.Failure);
        Assert.Null(returned.Snapshot.Story.Wait);
        Assert.Contains(901, returned.Snapshot.Story.Flags);
        var entity = returned.Snapshot.Exploration!.Entities[new("entity-128")];
        Assert.Same(script, entity.Actions);
        Assert.NotNull(entity.Follower);
        Assert.True(entity.Busy);
    }

    [Fact]
    public void MotionWaitKeepsMovingActorAndReleasesOnlyOnceAtItsDestination()
    {
        var (definition, initial) = Field([new WaitForEntityMotion(new("entity-128")), new WriteFlag(901, true), new EndProgram()]);
        var world = initial.Exploration!;
        var entity = world.Entities[new("entity-128")];
        world = world.WithEntity(entity with { Follower = null, Motion = entity.Motion with
            { XDestination = (short)(entity.Motion.X + 64), XTravel = 64, XVelocity = 32 } });
        initial = new(initial.SessionId, initial.Revision, initial.ObservationSequence, new ActiveExploration(world), initial.Story, initial.StopReason);
        var waiting = ProgramRunner.Run(definition, initial, []).Snapshot;
        var token = Assert.IsType<EntityWait>(waiting.Story.Wait).Token;
        var first = ExplorationDispatcher.Submit(definition, waiting, new AdvanceSimulation(token));
        Assert.Null(first.Failure);
        Assert.Equal(token, first.Snapshot.Story.Wait!.Token);
        Assert.DoesNotContain(901, first.Snapshot.Story.Flags);
        var second = ExplorationDispatcher.Submit(definition, first.Snapshot, new AdvanceSimulation(token));
        Assert.Null(second.Failure);
        Assert.Null(second.Snapshot.Story.Wait);
        Assert.Single(second.Observations, row => row.EntityWaitRelease is not null);
        Assert.Contains(901, second.Snapshot.Story.Flags);
        Assert.NotNull(ExplorationDispatcher.Submit(definition, second.Snapshot, new AdvanceSimulation(token)).Failure);
    }

    [Theory]
    [InlineData(EntityWaitCompletion.NotBusy)]
    [InlineData(EntityWaitCompletion.ScriptIdle)]
    public void ExistingWaitModesStillWaitForTheInstalledScript(EntityWaitCompletion completion)
    {
        var (definition, initial) = Field([new WaitForEntityMotion(new("entity-128")), new EndProgram()]);
        initial = initial.WithStory(initial.Story.Copy(initial.Story.Cursor,
            new EntityWait(new(77), new("entity-128"), Completion: completion)));
        var next = ExplorationDispatcher.Submit(definition, initial, new AdvanceSimulation(new(77)));
        Assert.Null(next.Failure);
        Assert.Equal(completion, Assert.IsType<EntityWait>(next.Snapshot.Story.Wait).Completion);
    }

    [Fact]
    public void OrderedAiRejectsUnknownOpposingActivationBeforeChoosingAMove()
    {
        var (definition, start) = Setup();
        var error = Assert.Throws<BattleRuleException>(() => SourceEnemyAi.Resolve(BattleTurnFlow.Start(definition, start), new("follower")));
        Assert.Equal("source-occupancy-word", error.Code);
    }

    private static (ScenarioDefinition, SessionSnapshot) Field(StoryInstruction[] instructions)
    {
        var (battle, party) = Setup();
        var layout = new WorkingMapLayout(new ushort[WorkingMapLayout.WordCount]);
        var map = new ExplorationMapDefinition(new("field"), layout, new OriginalMapTraversal([new(0, 0, 12, 12)]), [], []);
        var player = new ExplorationEntity(new("entity-0"), EntityMotionState.At(new(4, 4), 1, 32), true, Slot: 0);
        var follower = new ExplorationEntity(new("entity-128"), EntityMotionState.At(new(4, 4), 1, 32), true,
            new([new WaitEntityTicks(100), new JumpEntityAction(0)]), Slot: 1, Follower: new(0, 0, 0));
        var world = new ExplorationState(map, layout, player.Entity, [player, follower], party);
        var definition = new ScenarioDefinition("motion", [battle], exploration: new([map], [new StoryProgram("motion", instructions)]));
        return (definition, new(Guid.NewGuid(), 1, 1, new ActiveExploration(world),
            new StoryState(cursor: new("motion", 0)), SessionStopReason.SimulationWait));
    }

    [Theory]
    [InlineData(BattleMover.Stealth, 2, 4)]
    [InlineData(BattleMover.Regular, 3, 3)]
    [InlineData(BattleMover.Healer, 3, 3)]
    [InlineData(BattleMover.Centaur, 5, 5)]
    [InlineData(BattleMover.Hovering, 2, 2)]
    public void HillsAndDesertKeepTheirDistinctMoverCosts(BattleMover mover, int hills, int desert)
    {
        Assert.Equal(hills, BattleTerrainRules.MovementCost(new(TerrainSurface.Rough, TerrainProtection.Heavy), mover));
        Assert.Equal(desert, BattleTerrainRules.MovementCost(new(TerrainSurface.Desert, TerrainProtection.Heavy), mover));
        Assert.Equal(-1, BattleTerrainRules.MovementCost(new(TerrainSurface.Barrier, TerrainProtection.None), mover));
        if (mover == BattleMover.Stealth)
            Assert.Equal(-1, BattleTerrainRules.MovementCost(new(TerrainSurface.Impassable, TerrainProtection.None), mover));
    }

    [Fact]
    public void NoRegionEntryRetainsUnusedAllyOperandsAndDoesNotFabricateActivation()
    {
        var (definition, start) = Setup();
        var initial = BattleTurnFlow.Start(definition, start);
        Assert.Equal((byte)0, initial.GetActor(new("hero")).Deployment.Initialization!.PrimaryRegion);
        Assert.Same(initial, BattleActivationRules.BeforeRound(initial));
        Assert.Equal((ushort)0x2000, initial.GetActor(new("leader")).ActivationWord);
        Assert.All(initial.Regions!.Flags, flag => Assert.False(flag));
    }

    [Theory]
    [InlineData(255, 6)]
    [InlineData(64, 6)]
    [InlineData(255, 7)]
    [InlineData(64, 7)]
    [InlineData(255, 9)]
    [InlineData(64, 9)]
    [InlineData(255, 14)]
    [InlineData(64, 14)]
    public void DeadFollowTargetPromotesSecondaryAndPreservesItOnLaterDecisions(byte secondary, byte commandset)
    {
        var (definition, start) = Setup(secondary, commandset);
        var battle = BattleTurnFlow.Start(definition, start);
        battle = battle.With(actors: battle.Actors.Select(actor => actor.Actor.Value == "leader" ? actor.With(hp: 0) :
            actor.IsAlly ? actor.With(activationWord: 0) : actor));
        var first = SourceEnemyAi.Resolve(battle, new("follower"));
        Assert.Equal(secondary, first.Battle.GetActor(new("follower")).PrimaryOrder);
        Assert.Equal((byte)255, first.Battle.GetActor(new("follower")).SecondaryOrder);
        Assert.Equal((byte)128, battle.GetActor(new("follower")).PrimaryOrder);
        var second = SourceEnemyAi.Resolve(first.Battle, new("follower"));
        Assert.Equal(secondary, second.Battle.GetActor(new("follower")).PrimaryOrder);
        Assert.DoesNotContain(second.Effects, effect => effect.Kind == "ai-order-promoted");
    }

    [Theory]
    [InlineData(0, -1)]
    [InlineData(1, 0)]
    public void PointOrderRetainsTheSourceReturnedXZeroBranch(int x, int expected)
    {
        var (definition, start) = Setup();
        var battle = BattleTurnFlow.Start(definition, start);
        battle = battle.With(actors: battle.Actors.Select(actor => actor.Actor.Value == "leader"
            ? actor.With(position: new(x, 3)) : actor.IsAlly ? actor.With(activationWord: 0) : actor));
        var action = SourceEnemyAi.Resolve(battle, new("leader"));
        Assert.Equal(expected, Assert.Single(action.Effects, effect => effect.Kind == "ai-command-move-order5").After);
    }

    [Theory]
    [InlineData(10, 10, 5, 10, 8, 10)]
    [InlineData(6, 6, 12, 6, 8, 6)]
    [InlineData(8, 8, 8, 14, 8, 10)]
    public void OrderedEnemyApproachesAnOutsideTargetButStopsInsideItsOrderPocket(
        int x, int y, int targetX, int targetY, int stopX, int stopY)
    {
        var battle = PursuitState(new(x, y), [(0, BattleClassRule.UnpromotedSwordsman, new(targetX, targetY))]);
        string frozen = JsonSerializer.Serialize(battle);
        var result = SourceEnemyAi.Resolve(battle, new("pursuer"));
        Assert.Equal(new MapPosition(stopX, stopY), result.Destination);
        Assert.Equal(255, Assert.Single(result.Effects, effect => effect.Kind == "ai-move-target").Before);
        Assert.Contains(result.Effects, effect => effect.Kind == "ai-command-move3" && effect.After == 0);
        Assert.All(result.Path, point => Assert.InRange(Math.Abs(point.X - x) + Math.Abs(point.Y - y), 0, 2));
        Assert.Equal(battle.MainSeed, result.Battle.MainSeed);
        Assert.Equal(battle.ThinkingSeed, result.Battle.ThinkingSeed);
        Assert.Equal(frozen, JsonSerializer.Serialize(battle));
    }

    [Theory]
    [InlineData(1, BattleClassRule.UnpromotedSwordsman, 12)]
    [InlineData(4, BattleClassRule.UnpromotedSwordsman, 8)]
    [InlineData(1, BattleClassRule.UnpromotedPriest, 8)]
    public void OutsideTargetClassPassUsesBothAdjacentPredicates(int secondId, BattleClassRule secondClass, int stopX)
    {
        var battle = PursuitState(new(10, 10), [(0, BattleClassRule.UnpromotedSwordsman, new(4, 10)),
            (secondId, secondClass, new(16, 10))]);
        var result = SourceEnemyAi.Resolve(battle, new("pursuer"));
        Assert.Equal(new MapPosition(stopX, 10), result.Destination);
        Assert.Equal(new ActorRef(stopX == 12 ? "target-" + secondId : "target-0"),
            Assert.Single(result.Effects, effect => effect.Kind == "ai-move-target").Target);
    }

    [Theory]
    [InlineData(false, 4, 0, 4)]
    [InlineData(false, 1, 1, 255)]
    [InlineData(true, 1, 1, 4)]
    public void OrderedPursuitRanksRestrictedScoreBytesBeforeUnrestrictedApproach(
        bool outsideFirst, int secondId, int selected, int score)
    {
        var battle = PursuitState(new(10, 10), [(0, BattleClassRule.UnpromotedSwordsman, outsideFirst ? new(5, 10) : new(10, 12)),
            (secondId, BattleClassRule.UnpromotedSwordsman, outsideFirst ? new(10, 12) : new(5, 10))]);
        var costs = battle.Definition.Terrain.Select(tile => BattleTerrainRules.MovementCost(tile, BattleMover.Regular)).ToArray();
        var restricted = costs.Select((cost, index) => Math.Abs(index % 48 - 10) + Math.Abs(index / 48 - 10) <= 2 ? cost : (sbyte)-1).ToArray();
        var targets = battle.Actors.Where(actor => actor.IsAlly).OrderBy(actor => actor.ProcessingOrder).ToArray();
        var scores = WeightedMovement.Build(restricted, 10 * 48 + 10, 128);
        foreach (var target in targets) restricted[target.Position!.Y * 48 + target.Position.X] = -1;
        var result = AiMovementRules.PursueOrdered(costs, scores, WeightedMovement.Build(restricted, 10 * 48 + 10, 8),
            new(10, 10), targets, PhysicalPriorityTable.Regular, point => battle.Actors.Any(actor => actor.Position == point), 20, 20);
        Assert.Equal(selected, result.TargetIndex);
        Assert.Equal(score, result.TargetCosts[selected]);
        Assert.Equal(new[] { outsideFirst ? 255 : 4, outsideFirst ? 4 : 255 }, result.TargetCosts);
        Assert.Equal(score == 255 ? new MapPosition(8, 10) : new MapPosition(10, 11), result.Destination);
    }

    [Fact]
    public void OrderedPursuitRetainsThe128BoundaryAndUnreachable255Score()
    {
        var battle = PursuitState(new(0, 0), [(0, BattleClassRule.UnpromotedSwordsman, new(32, 32)),
            (4, BattleClassRule.UnpromotedSwordsman, new(33, 32))], width: 48);
        var costs = Enumerable.Repeat((sbyte)2, 2304).ToArray();
        var targets = battle.Actors.Where(actor => actor.IsAlly).ToArray();
        var result = AiMovementRules.PursueOrdered(costs, WeightedMovement.Build(costs, 0, 128),
            WeightedMovement.Build(costs, 0, 8), new(0, 0), targets, PhysicalPriorityTable.Regular,
            point => battle.Actors.Any(actor => actor.Position == point), 48, 48);
        Assert.Equal(new[] { 128, 255 }, result.TargetCosts);
        Assert.Equal(0, result.TargetIndex); // Class pass cannot swap source slots four apart.
        Assert.Equal(2, result.Destination.X + result.Destination.Y);
        Assert.Equal(4, result.Cost);
    }

    [Theory]
    [InlineData(0, false, 0)]
    [InlineData(0, true, 20)]
    [InlineData(7, false, 20)]
    [InlineData(7, true, 20)]
    public void RecoveryRetainsLivePartyFactsForAnAllyFromAnotherEncounter(ushort hp, bool all, ushort expected)
    {
        var definition = RecoveryContent();
        var party = new BattleStartInput("first", [new(new("target-0"), 20, 0, 2, 3, 4, 0, null),
            new(new("target-1"), hp, 0, 17, 8, 9, 0, null, ActivationWord: 32)], 0x1234, 0x5678, 987);
        var recovered = BattleOutcome.Heal(definition, party, all);
        Assert.Equal(party.Actors[0], recovered.Actors[0]);
        Assert.Equal(party.Actors[1] with { Hp = expected }, recovered.Actors[1]);
        Assert.Equal(party.Encounter, recovered.Encounter);
        Assert.Equal(party.MainSeed, recovered.MainSeed); Assert.Equal(party.ThinkingSeed, recovered.ThinkingSeed);
        Assert.Equal(party.Gold, recovered.Gold); Assert.Equal(hp, party.Actors[1].Hp);
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("pursuer")]
    public void RecoveryRejectsUnknownActorsAndOtherEncounterEnemies(string actor)
    {
        var definition = RecoveryContent();
        var party = new BattleStartInput("first", [new(new(actor), 1, 0, 2, 3, 4, 0, null)], 0x1234, 0x5678, 987);
        string frozen = JsonSerializer.Serialize(party);
        var failure = Assert.Throws<BattlePolicyFault>(() => BattleOutcome.Heal(definition, party, all: true));
        Assert.Equal("party-recovery", failure.Operation);
        Assert.Equal(frozen, JsonSerializer.Serialize(party));
    }

    private static ScenarioDefinition RecoveryContent()
    {
        var first = PursuitState(new(10, 10), [(0, BattleClassRule.UnpromotedSwordsman, new(5, 10))]).Definition;
        var second = PursuitState(new(10, 10), [(1, BattleClassRule.UnpromotedPriest, new(5, 10))]).Definition;
        return new("recovery", [new("first", first.Map, first.Width, first.Height, first.Terrain,
            first.Deployments.Where(row => row.Faction == BattleFaction.Ally), []),
            new("second", second.Map, second.Width, second.Height, second.Terrain, second.Deployments, [])]);
    }

    private static EngineBattleState PursuitState(MapPosition origin, (int Id, BattleClassRule Class, MapPosition Position)[] targets, int width = 20)
    {
        BattleActorDefinition Unit(string id, BattleClassRule rule) => new(new(id), rule, 1, 20, 0, 10, 4, 5, false, 4, [],
            physical: new(PhysicalCriticalRule.OneIn32WithHalfBonus, false, false, 0),
            sourceLoadout: new([127, 127, 127, 127], [63, 63, 63, 63]));
        var enemy = new BattleDeploymentDefinition(Unit("pursuer", BattleClassRule.Ordinary), BattleFaction.Enemy, 128,
            BattleControl.Automatic, new("source-orders"), origin, new(0x2020, 0, 15, 15, 0, 14, 64, 255));
        var allies = targets.Select(row => new BattleDeploymentDefinition(Unit("target-" + row.Id, row.Class), BattleFaction.Ally,
            row.Id, BattleControl.Player, null, row.Position, new(null, 0, 0, 0, 0, 0, 255, 255))).ToArray();
        BattleDeploymentDefinition[] deployments = [enemy, .. allies];
        var definition = new BattleDefinition("ordered-pursuit", new("field"), width, width,
            Enumerable.Range(0, 2304).Select(i => new BattleTerrain(i % 48 < width && i / 48 < width ? TerrainSurface.Open : TerrainSurface.Barrier, TerrainProtection.None)),
            deployments, [], rewards: new(false), initialization: new([], BattleRegionProgram.None, aiPoints: [origin],
                sourceCombatants: deployments.ToDictionary(row => row.ProcessingOrder, row => row.Actor)));
        return new(definition, deployments.Select(row => new BattleActorState(row, 20, 0, 0, row.Position, 0, 0,
            activationWord: row.Faction == BattleFaction.Ally ? (ushort)0 : (ushort)0x2020)), 0x1234, 0x5678, 1, [], 0, 0, regions: new(new bool[16], 0));
    }

    private static (BattleDefinition, BattleStartInput) Setup(byte secondary = 255, byte commandset = 9)
    {
        BattleActorDefinition Unit(string id, bool ally) => new(new(id), ally ? BattleClassRule.UnpromotedSwordsman : BattleClassRule.Ordinary,
            1, 20, 0, 10, 4, 5, false, 4, [], physical: new(PhysicalCriticalRule.OneIn32WithHalfBonus, false, ally, 0),
            sourceLoadout: new([127,127,127,127], [63,63,63,63]));
        var hero = new BattleDeploymentDefinition(Unit("hero", true), BattleFaction.Ally, 0, BattleControl.Player, null, new(12,12), new(null,0,0,0,0,0,255,255));
        var leader = new BattleDeploymentDefinition(Unit("leader", false), BattleFaction.Enemy,128,BattleControl.Automatic,new("source-orders"),new(3,3),new(0x2020,0,15,15,0,14,64,255));
        var follower = new BattleDeploymentDefinition(Unit("follower", false),BattleFaction.Enemy,129,BattleControl.Automatic,new("source-orders"),new(6,5),new(0x2020,0,15,15,0,commandset,128,secondary));
        var terrain = Enumerable.Range(0,2304).Select(i => new BattleTerrain(i%48<16 && i/48<16 ? TerrainSurface.Open : TerrainSurface.Barrier,TerrainProtection.None));
        var definition = new BattleDefinition("varied",new("field"),16,16,terrain,[follower,hero,leader],[],
            initialization:new([],BattleRegionProgram.None,aiPoints:[new(2,2)],sourceCombatants:new Dictionary<int,ActorRef>{{0,hero.Actor},{128,leader.Actor},{129,follower.Actor}}));
        var start = new BattleStartInput("varied",definition.Deployments.Select(row => new BattleActorStartInput(row.Actor,20,0,0,0,0,0,null)),0,0,0,
            new("controlled","authored engine behavior","No original runtime claim",0,true,true,true,false,false,0));
        return (definition,start);
    }
}
