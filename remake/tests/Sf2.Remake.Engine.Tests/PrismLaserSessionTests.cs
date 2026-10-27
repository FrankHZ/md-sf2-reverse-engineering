using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;
using static Sf2.Remake.Engine.Tests.PrismLaserTests;

namespace Sf2.Remake.Engine.Tests;

public sealed class PrismLaserSessionTests
{
    [Fact]
    public void OrdinaryMovementOffThenOnRayProducesOneShotOrderedDeathsAndUsableControl()
    {
        var s = Start("prism-laser");
        var events = Stay(s).Observations.ToList(); events.AddRange(Stay(s).Observations);
        Assert.Equal(2, s.Current.Battle.Round); Assert.Equal(Leader, s.Current.Selection!.Actor);
        Assert.DoesNotContain(events, e => e.Kind == "prism-laser" || e.Kind == "rng-prism-damage");
        MoveUp(s); Stay(s); MoveUp(s); var result = Stay(s);
        Assert.NotNull(s.Current.BattleScene?.Laser);
        Assert.Equal(new[] { Companion, Leader, Sentry }, s.Current.BattleScene!.Laser!.Targets);
        events.AddRange(result.Observations);
        int announcements = 0;
        while (s.Current.BattleScene is { } scene)
        {
            if (scene.Phase == BattleScenePhase.ActionMessage) announcements++;
            var before = s.Current;
            var command = Command(before);
            var accepted = Accept(s, command); events.AddRange(accepted.Observations);
            var after = s.Current;
            Assert.NotNull(Send(s, command).Failure); Assert.Same(after, s.Current);
        }
        Assert.Equal(1, announcements);
        Assert.Equal(new[] { Companion, Sentry }, events.Where(e => e.Kind == "death-cleanup").Select(e => e.Actor!.Value));
        Assert.Equal((ushort)1, s.Current.Battle.GetActor(Companion).Defeats);
        Assert.Equal((ushort)0, s.Current.Battle.GetActor(Leader).Kills);
        Assert.Null(s.Current.Battle.FirstAlly); Assert.NotNull(s.Current.Battle.DeathCredit);
        Assert.Equal(73u, s.Current.Battle.Gold);
        Assert.DoesNotContain(events, e => e.Kind is "exp" or "gold" or "kills" or "rng-item-drop" or "item-received");
        Assert.Equal(Leader, s.Current.Selection!.Actor); Assert.True(s.Current.HasBattleControl);
        var position = s.Current.Selection.Preview.Destination;
        Accept(s, new Move(ExplorationDirection.South));
        Assert.NotEqual(position, s.Current.Selection!.Preview.Destination);
    }
    [Fact]
    public void EmptyShotHasNoTargetButKeepsAnnouncementAnimationTokenAndKnownNoneCustody()
    {
        var b = Battle().With(firstAlly: Leader);
        b = b.With(actors: b.Actors.Select(a => a.Actor == Flower ? a : a.With(position: new(a.Position!.X, 6))),
            queue: [new(Flower, 20), new(Leader, 10)], cursor: 0);
        var resolution = Prepare(b);
        var snapshot = Snapshot(b); var result = BattleSceneContinuation.Begin(snapshot, resolution, []);
        List<BattleScenePhase> phases = [];
        while (result.Snapshot.BattleScene is { } scene)
        {
            Assert.Null(scene.Target); Assert.Null(scene.DisplayedAlly); Assert.Equal(Flower, scene.DisplayedEnemy);
            Assert.Equal("None", scene.ReactionKind); Assert.Equal(0, scene.Amount); Assert.Null(scene.ItemReceipt);
            Assert.NotNull(result.Snapshot.Battle.DeathCredit); Assert.Null(result.Snapshot.Battle.FirstAlly);
            phases.Add(scene.Phase);
            var old = result.Snapshot;
            result = BattleSceneContinuation.Submit(old, Command(old)); Assert.Null(result.Failure);
            if (result.Snapshot.BattleScene is not null)
            {
                var stale = BattleSceneContinuation.Submit(result.Snapshot, Command(old));
                Assert.NotNull(stale.Failure); Assert.Same(result.Snapshot, stale.Snapshot);
            }
        }
        Assert.Equal(new[] { BattleScenePhase.Initialize, BattleScenePhase.ActionMessage, BattleScenePhase.ActionAnimation, BattleScenePhase.End }, phases);
        Assert.Equal(b.MainSeed, result.Snapshot.Battle.MainSeed);
        Assert.True(result.Snapshot.HasBattleControl); Assert.Equal(Leader, result.Snapshot.Selection!.Actor);
    }
    [Fact]
    public void LaterAccountingFailureRetainsHpRngAndDeathTokenThenRetriesOnlyCleanup()
    {
        var b = Battle().With(firstAlly: Leader); var prepared = Prepare(b);
        var result = BattleSceneContinuation.Begin(Snapshot(b), prepared, []);
        var progression = new FailingDefeats();
        var rules = new SessionRules("prism-failure", new Sf2HealingRule(), new Sf2PhysicalAction(),
            RuleCompositions.SourceItem(), RuleCompositions.SourceStay(), progression: progression);
        while (result.Snapshot.BattleScene is not { Phase: BattleScenePhase.FieldExit, FieldStep: 2 })
        { result = BattleSceneContinuation.Submit(result.Snapshot, Command(result.Snapshot), rules); Assert.Null(result.Failure); }
        var before = result.Snapshot;
        var failed = BattleSceneContinuation.Submit(before, Command(before), rules);
        Assert.NotNull(failed.Failure); Assert.Same(before, failed.Snapshot);
        Assert.Equal(0, before.Battle.GetActor(Companion).Hp); Assert.Equal(0, before.Battle.GetActor(Sentry).Hp);
        Assert.NotNull(before.Battle.GetActor(Sentry).Position); Assert.Equal((ushort)0, before.Battle.GetActor(Companion).Defeats);
        progression.Fail = false;
        var fixedResult = BattleSceneContinuation.Submit(before, Command(before), rules); Assert.Null(fixedResult.Failure);
        Assert.Equal(before.Battle.MainSeed, fixedResult.Snapshot.Battle.MainSeed);
        Assert.Equal((ushort)1, fixedResult.Snapshot.Battle.GetActor(Companion).Defeats);
        Assert.Null(fixedResult.Snapshot.Battle.GetActor(Sentry).Position);
        Assert.DoesNotContain(fixedResult.Observations, e => e.Kind == "kills" || e.Kind.StartsWith("rng-"));
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LaserKnownNoneSurvivesLeaderSweepAndDefeatPrecedesVictory(bool leaderDies)
    {
        var b = Battle(); var d = b.Definition;
        var slots = new Dictionary<ActorRef, byte> { { Sentry, 128 }, { Flower, 130 } };
        var prism = new PrismLaserDefinition(d.PrismLaser!.Combatants.ToDictionary(p => p.Key, p => p.Key == Sentry ? p.Value with { Slot = 128 } : p.Value));
        var definition = new BattleDefinition(d.Encounter, d.Map, d.Width, d.Height, d.Terrain, d.Deployments, d.Spells.Values,
            outcome: new(Leader, Sentry), leaderCleanup: new(Leader, slots), prismLaser: prism);
        b = new(definition, b.Actors.Select(a => a.Actor == Leader && leaderDies ? a.With(hp: 1) : a), b.MainSeed, b.ThinkingSeed, 1, b.Queue, b.Cursor, b.Gold, firstAlly: Leader);
        var result = BattleSceneContinuation.Begin(Snapshot(b), Prepare(b), []); List<SessionObservation> facts = [];
        while (result.Snapshot.BattleScene is not null)
        { result = BattleSceneContinuation.Submit(result.Snapshot, Command(result.Snapshot)); Assert.Null(result.Failure); facts.AddRange(result.Observations); }
        Assert.Equal(leaderDies ? "Defeat" : "Victory", Assert.Single(facts, e => e.Kind == "battle-outcome").Detail);
        Assert.Equal(leaderDies ? new[] { Companion, Leader, Sentry } : new[] { Companion, Sentry, Flower }, facts.Where(e => e.Kind == "death-cleanup").Select(e => e.Actor!.Value));
        Assert.Equal((ushort)0, result.Snapshot.Battle.GetActor(Leader).Kills);
        Assert.Equal(73u, result.Snapshot.Battle.Gold);
        Assert.DoesNotContain(facts, e => e.Kind is "kills" or "exp" or "gold" or "item-received");
    }
    [Fact]
    public void PhysicalTargetAdmissionStillRejectsFriendlyFireAndUnknownCustodyStillRejectsDeaths()
    {
        var b = Battle(); var physical = new Sf2PhysicalAction();
        Assert.Throws<BattleRuleException>(() => BattleActionRules.Prepare(physical, b, Flower, b.GetActor(Flower).Position!, new(BattleActionKind.Physical), Sentry, new Sf2BattleProgressionRule()));
        var dead = b.GetActor(Sentry).With(hp: 0); var batch = BattleDeathBatch.Empty.Append(b.GetActor(Sentry), dead);
        var changed = b.With(actors: b.Actors.Select(a => a.Actor == Sentry ? dead : a));
        Assert.Throws<BattleRuleException>(() => BattleActionCommitter.Publish(Snapshot(b), changed, Flower, b.GetActor(Flower).Position, [], [], RuleCompositions.Sf2(), sceneEnded: true, deaths: batch));
    }

    [Fact]
    public void SleepingFlowerUsesExistingSkipRecoveryAndRetainsOrdinarySceneCredit()
    {
        var b = Battle().With(firstAlly: Leader, queue: [new(Flower, 20), new(Leader, 10)], cursor: 0);
        b = b.With(actors: b.Actors.Select(a => a.Actor == Flower ? a.With(status: 0xC0) : a));
        var result = BattleAdvancer.Advance(Snapshot(b), []);
        Assert.Null(result.Failure); Assert.True(result.Snapshot.BattleScene!.IsSleepTurn);
        Assert.Null(result.Snapshot.BattleScene.Laser); Assert.Equal(Leader, result.Snapshot.Battle.FirstAlly);
        Assert.DoesNotContain(result.Observations, e => e.Kind is "rng-prism-damage" or "prism-laser");
        Assert.Single(result.Observations, e => e.Kind == "rng-sleep-wake");
        result = BattleSceneContinuation.Submit(result.Snapshot, Command(result.Snapshot));
        Assert.Null(result.Failure); Assert.True(result.Snapshot.HasBattleControl);
        Assert.Equal(Leader, result.Snapshot.Battle.FirstAlly);
    }

    [Fact]
    public void LaserCannotConsumeConfiguredEnemyDropInventoryOrDeals()
    {
        var b = Battle(); var d = b.Definition;
        var drops = new EnemyItemDropDefinition(36, new Dictionary<ActorRef, byte> { [Sentry] = 140 },
            [new(36, 140, 110, 24)], [new(110, "Authored sword", true)]);
        var definition = new BattleDefinition(d.Encounter, d.Map, d.Width, d.Height, d.Terrain,
            d.Deployments, d.Spells.Values, itemDrops: drops, prismLaser: d.PrismLaser);
        var custody = new BattleItemCustody([], new Dictionary<byte, byte> { [110] = 3 });
        b = new(definition, b.Actors.Select(a => a.Actor == Sentry ? a.With(
            sourceLoadout: new([110, 127, 127, 127], [63, 63, 63, 63])) : a),
            b.MainSeed, b.ThinkingSeed, 1, b.Queue, b.Cursor, b.Gold, itemCustody: custody);
        var result = BattleActionRules.Prepare(new PrismLaser(), b, Flower, b.GetActor(Flower).Position!,
            new(BattleActionKind.PrismLaser), Leader, new FailingDefeats());
        Assert.Same(custody, result.Prepared.ItemCustody);
        Assert.Equal((ushort)110, result.Prepared.GetActor(Sentry).SourceLoadout!.Items[0]);
        Assert.All(result.Reactions, r => Assert.Null(r.Receipt)); Assert.Null(result.Reward);
        Assert.Equal(6, result.ConstructionEffects.Count);
    }

    private static SessionSnapshot Snapshot(EngineBattleState b) => new(Guid.NewGuid(), 0, 0, b, null, SessionStopReason.SimulationWait);
    private static SessionCommand Command(SessionSnapshot s) => s.BattleScene!.RequiresAcknowledgement ? new Acknowledge(s.BattleScene.Token) : new CompletePresentation(s.BattleScene.Token, s.BattleScene.CompletionKind);
    private static void MoveUp(GameSession s)
    { var result = Accept(s, new Move(ExplorationDirection.North)); FinishMovement(s, result); }
    private sealed class FailingDefeats : IBattleProgressionRule
    {
        public string Identity => "prism-accounting-fault";
        public bool Fail = true;
        public ushort Defeats(ushort current) => Fail ? throw new InvalidOperationException() : (ushort)(current + 1);
        public ushort Kills(ushort current) => throw new InvalidOperationException("laser cannot credit kills");
        public uint Gold(uint current, uint award) => throw new InvalidOperationException();
        public int Award(EngineBattleState b, BattleActorState a, BattleActionKind k, IReadOnlyList<BattleReaction> r, ref uint s, List<BattleEffect> e) => throw new InvalidOperationException();
        public BattleActorState Credit(BattleActorState a, int n, List<BattleEffect> e) => throw new InvalidOperationException();
        public BattleActorState Grow(BattleActorState a, ref uint s, List<BattleEffect> e) => throw new InvalidOperationException();
    }
}
