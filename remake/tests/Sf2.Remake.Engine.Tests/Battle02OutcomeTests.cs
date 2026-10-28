using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Gameplay.Sf2;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;
using System.Text.Json.Nodes;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class Battle02OutcomeTests
{
    [Fact]
    public void OverworldLayer254InitializesFollowsAndKeepsUnsupportedProfilesClosed()
    {
        var world = Start("harbor-arrival", document =>
        {
            document["world"]!["maps"]![0]!["layout"] = new JsonArray(Enumerable.Range(0, 64).Select(_ => (JsonNode)new JsonArray(Enumerable.Range(0, 64).Select(_ => (JsonNode)JsonValue.Create(0)!).ToArray())).ToArray());
            var area = document["world"]!["maps"]![0]!["areas"]![0]!;
            area["maxX"] = 63; area["maxY"] = 63;
            area["view"] = JsonNode.Parse("""{"foregroundX":0,"foregroundY":0,"backgroundX":0,"backgroundY":0,"parallaxAX":256,"parallaxAY":256,"parallaxBX":256,"parallaxBY":256,"autoscrollAX":0,"autoscrollAY":0,"autoscrollBX":0,"autoscrollBY":0,"layer":254}""");
        }).Current.Exploration!;
        world = world.WithEntity(world.PlayerEntity with { Motion = EntityMotionState.At(new(29, 32), 3, 32) });
        var view = ExplorationViewRunner.Initialize(world);
        Assert.Equal(9216, view.AX.Position); Assert.Equal(10368, view.AY.Position);
        Assert.Equal(view.AX, view.BX); Assert.Equal(view.AY, view.BY);
        world = world.WithEntity(world.PlayerEntity with { Motion = EntityMotionState.At(new(31, 32), 3, 32) });
        var followed = ExplorationViewRunner.Tick(world, view, new(2, 0, 0));
        Assert.Equal(9240, followed.AX.Position); Assert.Equal(followed.AX, followed.BX);
        Assert.Equal(10368, followed.AY.Position); Assert.True(followed.Scrolling);
        Assert.Throws<BattleRuleException>(() => ExplorationViewRunner.Validate(view.Area with { Layer = 253 }));
        Assert.Throws<BattleRuleException>(() => ExplorationViewRunner.Validate(view.Area with { AutoscrollAX = 1 }));
    }

    [Fact]
    public void GrownActorConsumesHerbWithoutChangingGrowthOrFacingAndRejectsUnownedMutations()
    {
        var session = Start(); var source = session.Current.Battle; var d = source.Definition;
        var definition = new BattleDefinition(d.Encounter, d.Map, d.Width, d.Height, d.Terrain, d.Deployments, d.Spells.Values, d.Rewards,
            d.Initialization, d.Outcome, [new(0, "Medical Herb", 10, 0, 1)]);
        var actor = source.GetActor(new("medic-a"));
        var loadout = new BattleSourceLoadout([199, 0, 0, 127], [0, 63, 63, 63]);
        var progress = new BattleActorProgress(2, actor.MaxHp, actor.MaxMp, actor.BaseAttack, actor.Defense, actor.Agility, actor.Spells, loadout);
        var grown = actor.With(progress: progress, sourceLoadout: loadout, facing: 2);
        var battle = new EngineBattleState(definition, source.Actors.Select(a => a.Actor == actor.Actor ? grown : a), source.MainSeed, source.ThinkingSeed, source.Round, source.Queue, source.Cursor, source.Gold);
        var action = new BattleActionRef(BattleActionKind.Item, ItemSlot: 1);
        var prepared = BattleChoices.Prepare(RuleCompositions.Sf2(), battle, actor.Actor, actor.Position!, action, actor.Actor);
        var consumed = prepared.Prepared.GetActor(actor.Actor);
        Assert.Equal(new ushort[] { 199, 0, 127, 127 }, consumed.SourceLoadout!.Items);
        Assert.Equal(progress, consumed.Progress! with { SourceLoadout = loadout });
        Assert.Equal((byte)2, consumed.Facing); Assert.Equal(actor.Hp, consumed.Hp);
        Assert.Equal(actor.Exp, consumed.Exp); Assert.Equal(actor.Kills, consumed.Kills); Assert.Equal(actor.Defeats, consumed.Defeats);
        var admission = new BattleActionAdmission(0, 1, Item: definition.HealingItems[0]);
        foreach (var changed in new[] { consumed.With(facing: 1), consumed.With(progress: consumed.Progress! with { Level = 3 }) })
        {
            var invalid = prepared with { Prepared = prepared.Prepared.With(actors: prepared.Prepared.Actors.Select(a => a.Actor == changed.Actor ? changed : a)) };
            Assert.Throws<InvalidOperationException>(() => invalid.Validate(battle, actor.Actor, actor.Position!, action, actor.Actor, admission));
        }
        var result = BattleSceneContinuation.Begin(session.Current, prepared, []);
        while (result.Snapshot.BattleScene is { } scene)
        {
            result = BattleSceneContinuation.Submit(result.Snapshot, scene.RequiresAcknowledgement ? new Acknowledge(scene.Token) : new CompletePresentation(scene.Token, scene.CompletionKind));
            Assert.Null(result.Failure);
        }
        var finished = result.Snapshot.Battle.GetActor(actor.Actor);
        Assert.Equal((ushort)100, finished.Hp); Assert.True(finished.Exp > actor.Exp);
        Assert.Equal(new ushort[] { 199, 0, 127, 127 }, finished.SourceLoadout!.Items);
        Assert.Equal(progress, finished.Progress! with { SourceLoadout = loadout });
        Assert.Equal(actor.Mp, finished.Mp); Assert.Equal(actor.Kills, finished.Kills); Assert.Equal(actor.Defeats, finished.Defeats);
    }

    [Theory]
    [InlineData(3, 2, 0, false)]
    [InlineData(5, 4, 1, true)]
    [InlineData(7, 6, 2, false)]
    [InlineData(9, 8, 3, true)]
    public void VictoryUsesFirstActiveMemberIdentityAreaAndLiveFacing(int x, int y, byte facing, bool deadAlly)
    {
        var (definition, battle, story, party) = Outcome(deadAlly);
        battle = battle.With(actors: battle.Actors.Select(a => a.Actor.Value == "medic-a" ? a.With(position: new(x, y), facing: facing) : a));
        var selected = new Sf2OutcomeReturnPolicy().Start(definition, battle, story, BattleOutcomeKind.Victory, party);
        Assert.Equal(new FieldReturnAnchor(new("map-66"), new(x + 16, y + 19), facing), selected.Return);
        Assert.Equal(selected.Return.Position, selected.ProjectionPosition);
        Assert.Equal(new ProgramLocation("empty", 0), selected.Program);
        Assert.Equal(party.MainSeed, selected.Party.MainSeed); Assert.Equal(party.ThinkingSeed, selected.Party.ThinkingSeed);
        Assert.Equal(party.Gold, selected.Party.Gold);
        foreach (var before in party.Actors)
        {
            var after = selected.Party.Actors.Single(a => a.Actor == before.Actor);
            Assert.Equal(before, after with { Hp = before.Hp, Mp = before.Mp });
        }
        Assert.Equal(deadAlly ? 0 : battle.GetActor(new("guard-a")).MaxHp,
            selected.Party.Actors.Single(a => a.Actor.Value == "guard-a").Hp);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void DefeatRecoversLeaderAndMoneyWithoutInventingCompletionThenRetryUsesFreshEnemies(bool completed, bool deadAlly)
    {
        var (definition, battle, story, party) = Outcome(deadAlly);
        party = new(party.Encounter, party.Actors.Select(a => a.Actor.Value == "medic-a" ? a with { Hp = 0, Defeats = 6 } : a),
            party.MainSeed, party.ThinkingSeed, 101, party.NewBattle, party.ActiveAllies);
        story = story.Copy(null, flags: completed ? [0, 1, 32, 33, 399, 402, 502] : [0, 1, 32, 33, 399, 402],
            continuation: ProgramContinuation.DefeatProgramFinished);
        var policy = new Sf2OutcomeReturnPolicy();
        var selected = policy.Start(definition, battle, story, BattleOutcomeKind.Defeat, party);
        Assert.Equal(new FieldReturnAnchor(new("map-3"), new(32, 13), 1), selected.Return);
        var finish = policy.Finish(definition, story, party);
        Assert.Equal(50u, finish.Returned.Gold); Assert.Equal(completed, finish.ClearUnlocked);
        Assert.False(finish.SetCompleted); Assert.Null(finish.JoinMember);
        Assert.Equal((ushort)100, finish.Returned.Actors.Single(a => a.Actor.Value == "medic-a").Hp);
        Assert.Equal(deadAlly ? 0 : 12, finish.Returned.Actors.Single(a => a.Actor.Value == "guard-a").Hp);
        foreach (var before in party.Actors)
            Assert.Equal(before, finish.Returned.Actors.Single(a => a.Actor == before.Actor) with { Hp = before.Hp, Mp = before.Mp });
        Assert.Equal(party.MainSeed, finish.Returned.MainSeed); Assert.Equal(party.ThinkingSeed, finish.Returned.ThinkingSeed);
        var field = Start("harbor-arrival").Current;
        var retry = new SessionSnapshot(field.SessionId, field.Revision, field.ObservationSequence,
            new ActiveExploration(field.Exploration!.WithParty(finish.Returned)),
            story.Copy(null, continuation: ProgramContinuation.BeforeBattleFinished), SessionStopReason.SimulationWait);
        var entered = BattleEntry.Continue(definition, retry, [], RuleCompositions.Sf2());
        Assert.Equal(retry.SessionId, entered.SessionId);
        Assert.Equal(50u, entered.Battle.Gold);
        Assert.Equal((ushort)6, entered.Battle.GetActor(new("medic-a")).Defeats);
        Assert.Equal((ushort)15, entered.Battle.GetActor(new("dummy-a")).Hp);
        Assert.Equal(new MapPosition(8, 5), entered.Battle.GetActor(new("dummy-a")).Position);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EmptyAfterProgramAndCompletedSkipBothKeepJoinZeroAndFlagTail(bool completed)
    {
        var (definition, battle, story, party) = Outcome(false);
        story = story.Copy(null, flags: completed ? [1, 33, 399, 402, 502] : [1, 33, 399, 402], continuation: ProgramContinuation.VictoryProgramFinished);
        // Test the wrapper tail independently of initial active-member selection.
        var field = Start("harbor-arrival").Current;
        var snapshot = new SessionSnapshot(field.SessionId, 1, 1, new ActiveExploration(field.Exploration!.WithParty(party)), story, SessionStopReason.SimulationWait);
        var events = new List<SessionObservation>();
        var result = BattleOutcome.Continue(definition, snapshot, events, RuleCompositions.Sf2());
        Assert.Contains(0, result.Story.Flags); Assert.Contains(32, result.Story.Flags);
        Assert.Contains(502, result.Story.Flags); Assert.DoesNotContain(402, result.Story.Flags); Assert.DoesNotContain(403, result.Story.Flags);
        Assert.Equal(new[] { "after-battle-join", "battle-unlock-cleared", "battle-completed-set", "exploration-return-started" }, events.Select(e => e.Kind));
        var skipStory = story.Copy(null, flags: completed ? [0, 1, 32, 33, 399, 402, 502] : [0, 1, 32, 33, 399, 402]);
        Assert.Equal(completed ? null : new ProgramLocation("empty", 0), new Sf2OutcomeReturnPolicy().Start(definition, battle, skipStory, BattleOutcomeKind.Victory, party).Program);
    }

    [Fact]
    public void DeliveredMovementAndCancelOwnFacingButStationarySelectionDoesNot()
    {
        var session = Start(); var before = session.Current.Battle; var actor = session.Current.Selection!.Actor;
        Assert.Equal((byte)3, before.GetActor(actor).Facing);
        FinishMovement(session, Accept(session, new Move(ExplorationDirection.East)));
        UnchangedExceptFacing(before, session.Current.Battle, actor, 0);
        Accept(session, new Confirm()); Assert.Equal((byte)0, session.Current.Battle.GetActor(actor).Facing);
        FinishMovement(session, Accept(session, new Cancel()));
        UnchangedExceptFacing(before, session.Current.Battle, actor, 2);
    }

    [Fact]
    public void NoActionResetsOnlyActorAndSceneCompletionResetsPlacedEntities()
    {
        var session = Start(); var before = session.Current; var actor = before.Selection!.Actor;
        var battle = before.Battle.With(actors: before.Battle.Actors.Select(a => a.With(facing: 1)));
        var events = new List<SessionObservation>();
        var stayed = BattleActionCommitter.Publish(before, battle, actor, battle.GetActor(actor).Position!, [], events, RuleCompositions.Sf2());
        Assert.Equal((byte)3, stayed.Battle.GetActor(actor).Facing);
        Assert.All(stayed.Battle.Actors.Where(a => a.Actor != actor), a => Assert.Equal((byte)1, a.Facing));
        var resolution = BattleChoices.Prepare(RuleCompositions.Sf2(), battle, actor, battle.GetActor(actor).Position!, new(BattleActionKind.Healing, battle.GetActor(actor).Spells[0]), actor);
        var result = BattleSceneContinuation.Begin(before, resolution, []);
        while (result.Snapshot.BattleScene is { } scene)
        {
            result = BattleSceneContinuation.Submit(result.Snapshot, scene.Healing is { LogicalComplete: false, AtTimedInput: false } ? new AdvanceSimulation(scene.Token) : scene.RequiresAcknowledgement ? new Acknowledge(scene.Token) : new CompletePresentation(scene.Token, scene.CompletionKind));
            Assert.Null(result.Failure);
        }
        Assert.All(result.Snapshot.Battle.Actors.Where(a => a.Position is not null), a => Assert.Equal((byte)3, a.Facing));
    }

    internal static void UnchangedExceptFacing(EngineBattleState before, EngineBattleState after, ActorRef actor, byte facing)
    {
        Assert.Same(before.Definition, after.Definition); Assert.Equal(before.MainSeed, after.MainSeed); Assert.Equal(before.ThinkingSeed, after.ThinkingSeed);
        Assert.Equal(before.Gold, after.Gold); Assert.Equal(before.Round, after.Round); Assert.Equal(before.Cursor, after.Cursor);
        Assert.Equal(before.Queue, after.Queue); Assert.Same(before.StartPolicy, after.StartPolicy); Assert.Same(before.Regions, after.Regions);
        Assert.Equal(before.Actors.Count, after.Actors.Count);
        foreach (var a in before.Actors)
        {
            var b = after.GetActor(a.Actor);
            Assert.Equal((a.Deployment, a.Hp, a.Mp, a.Exp, a.Position, a.Kills, a.Defeats, a.LastTarget, a.Attack, a.Status, a.ActivationWord, a.AiMemory, a.Progress, a.SourceLoadout, a.PrimaryOrder, a.SecondaryOrder),
                (b.Deployment, b.Hp, b.Mp, b.Exp, b.Position, b.Kills, b.Defeats, b.LastTarget, b.Attack, b.Status, b.ActivationWord, b.AiMemory, b.Progress, b.SourceLoadout, b.PrimaryOrder, b.SecondaryOrder));
            Assert.Equal(a.Actor == actor ? facing : a.Facing, b.Facing);
        }
    }

    private static (ScenarioDefinition, EngineBattleState, StoryState, BattleStartInput) Outcome(bool deadAlly)
    {
        var read = Admitted(); var initial = BattleTurnFlow.Start(read.Definition.Encounters[read.Start.Encounter], read.Start);
        var d = initial.Definition;
        var deployments = d.Deployments.Select((row, index) => row with { ProcessingOrder = 10 - index, Initialization = row.Faction == BattleFaction.Ally ? new(null, 0, 0, 0, 0, AllyPartyMember: index) : null }).ToArray();
        var encounter = new BattleDefinition(d.Encounter, d.Map, d.Width, d.Height, d.Terrain, deployments, d.Spells.Values, d.Rewards, d.Initialization, new(new("medic-a"), new("dummy-a")));
        var actors = initial.Actors.Select(a => new BattleActorState(deployments.Single(row => row.Actor == a.Actor), a.Actor.Value == "guard-a" && deadAlly ? (ushort)0 : a.Hp, a.Mp, 17, a.Position, 4, 5, facing: 2));
        var battle = new EngineBattleState(encounter, actors, 123, 456, 2, initial.Queue, initial.Cursor, 101);
        var field = Start("harbor-arrival").Definition.Exploration!;
        var world = new ExplorationDefinition(field.Maps.Values, [new("empty", [new EndProgram(true)]), new("return", [new ReturnBattleMap(), new EndProgram()])], field.Texts, partyFlags: new(2, 0, 32, 2));
        var definition = new ScenarioDefinition("outcome-tests", [encounter], exploration: world, encounterStarts: [read.Start]);
        var route = new ExplorationBattleRoute(d.Encounter, 402, 502, 452, null, null, Outcome: new(new("empty", 0), 0, new("empty", 0), new("empty", 0), new("return", 0), new("map-66"), 3, new("map-3"), new(32, 13), 1, new(16, 19)));
        var story = new StoryState([0, 1, 32, 33, 399, 402], enteringBattle: route);
        var party = new BattleStartInput(d.Encounter, battle.Actors.Where(a => a.IsAlly).Select(a => new BattleActorStartInput(a.Actor, a.Hp, a.Mp, a.Exp, a.Kills, a.Defeats, a.Status, null, a.Progress, a.SourceLoadout, a.ActivationWord)), 123, 456, 101);
        return (definition, battle, story, party);
    }
}
