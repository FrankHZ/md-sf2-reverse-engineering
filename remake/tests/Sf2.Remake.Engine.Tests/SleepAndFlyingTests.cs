using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class SleepAndFlyingTests
{
    [Theory]
    [InlineData(TerrainSurface.Open)]
    [InlineData(TerrainSurface.Brush)]
    [InlineData(TerrainSurface.Rough)]
    [InlineData(TerrainSurface.Deep)]
    [InlineData(TerrainSurface.Desert)]
    [InlineData(TerrainSurface.Impassable)]
    [InlineData(TerrainSurface.HighSky)]
    public void FlyingTraversesAdmittedSkyAndGroundWithoutLandReduction(TerrainSurface surface)
    {
        var tile = new BattleTerrain(surface, TerrainProtection.Heavy);
        Assert.Equal((sbyte)2, BattleTerrainRules.MovementCost(tile, BattleMover.Flying));
        Assert.Equal(256, Sf2PhysicalAction.LandMultiplier(tile, BattleMover.Flying));
        Assert.Equal(205, Sf2PhysicalAction.LandMultiplier(tile, BattleMover.Hovering));
        if (surface == TerrainSurface.HighSky)
            Assert.Equal((sbyte)-1, BattleTerrainRules.MovementCost(tile, BattleMover.Hovering));
        Assert.Equal((sbyte)-1, BattleTerrainRules.MovementCost(new(TerrainSurface.Barrier, TerrainProtection.None), BattleMover.Flying));
    }

    [Fact]
    public void FlyingUsesOneKernelForPreviewAiMovementAndOccupiedStops()
    {
        var battle = Battle(flying: true);
        var actor = battle.GetActor(new("bat"));
        var preview = BattleMovement.Preview(battle, actor.Actor, new(4, 4));
        Assert.Equal(2, preview.Cost);
        var decision = SourceEnemyAi.Resolve(battle, actor.Actor);
        _ = BattleMovement.Preview(battle, actor.Actor, decision.Destination);
        Assert.DoesNotContain(decision.Path, point => !battle.Definition.Contains(point));
        Assert.Throws<BattleRuleException>(() => BattleMovement.Preview(battle, actor.Actor, new(16, 5)));
        Assert.Throws<BattleRuleException>(() => BattleMovement.RequireStop(battle, actor.Actor, battle.GetActor(new("hero")).Position!));
        var adjacent = battle.With(actors: battle.Actors.Select(unit => unit.Actor == new ActorRef("hero") ? unit.With(position: new(5, 5)) : unit));
        var action = Sf2PhysicalAction.Prepare(adjacent, new("hero"), new(5, 5), actor.Actor, new Sf2BattleProgressionRule());
        Assert.Equal((ushort)8, action.ConstructionEffects.First(effect => effect.Kind == "rng-dodge").RandomRange);
    }

    [Fact]
    public void SleepCriticalIsOneInFourWithoutNumericalBonusAndLethalHitsDoNotInflictIt()
    {
        var (seed, sleeping) = Strike(result => result.SleepInflicted);
        Assert.False(sleeping.Critical);
        Assert.Equal((ushort)4, sleeping.Rolls.Single(roll => roll.Purpose == "critical").Range);
        Assert.InRange(sleeping.Damage, 6, 8); // ATT12-DEF4, source spread only; no critical bonus.
        var lethal = PhysicalStrikeRules.Resolve(12, 4, 1, 256, seed, 32, 4, 0, sleepCritical: true);
        Assert.Equal((ushort)0, lethal.Hp); Assert.False(lethal.SleepInflicted);
        Assert.DoesNotContain(lethal.Rolls, roll => roll.Purpose is "double" or "counter");
    }

    [Fact]
    public void AlreadySleepingTargetSkipsDodgeDrawAndOrdinaryDamageDoesNotWakeIt()
    {
        var battle = Battle();
        battle = battle.With(actors: battle.Actors.Select(actor => actor.Actor == new ActorRef("hero") ? actor.With(status: 0x80) : actor));
        var action = Sf2PhysicalAction.Prepare(battle, new("bat"), new(4, 5), new("hero"), new Sf2BattleProgressionRule());
        Assert.DoesNotContain(action.ConstructionEffects, effect => effect.Kind == "rng-dodge");
        Assert.DoesNotContain(action.Reactions, reaction => reaction.Action == "physical-counter");
        var damaged = action.ApplyReaction(action.Prepared, action.Reactions[0]);
        Assert.Equal((ushort)0x80, damaged.GetActor(new("hero")).Status);
    }

    [Fact]
    public void ConstructionRetainsOldStatusAndEachPlaybackWriteKeepsSourceDoubleOrder()
    {
        var battle = Battle();
        BattleActionResolution? chosen = null;
        for (uint word = 0; word <= ushort.MaxValue; word++)
        {
            var candidate = Sf2PhysicalAction.Prepare(battle.With(mainSeed: word << 16), new("bat"), new(4, 5), new("hero"), new Sf2BattleProgressionRule());
            if (candidate.Reactions.Count >= 2 && candidate.Reactions[0].InflictedSleep == 0xC0 &&
                candidate.Reactions[1].Action == "physical-second" && candidate.Reactions[1].InflictedSleep is null)
            { chosen = candidate; break; }
        }
        Assert.NotNull(chosen);
        Assert.Equal((ushort)0, chosen.Prepared.GetActor(new("hero")).Status);
        var firstDamage = chosen.ApplyReaction(chosen.Prepared, chosen.Reactions[0]);
        Assert.Equal((ushort)0, firstDamage.GetActor(new("hero")).Status);
        var firstSleep = chosen.ApplySleep(firstDamage, chosen.Reactions[0]);
        Assert.Equal((ushort)0xC0, firstSleep.GetActor(new("hero")).Status);
        var secondDamage = chosen.ApplyReaction(firstSleep, chosen.Reactions[1]);
        Assert.Equal((ushort)0, secondDamage.GetActor(new("hero")).Status); // Original constructed damage operand.
        Assert.Equal((ushort)0, chosen.ApplySleep(secondDamage, chosen.Reactions[1]).GetActor(new("hero")).Status);
    }

    [Fact]
    public void NewlyConstructedSleepDoesNotSuppressAnOtherwiseLegalConstructedCounter()
    {
        var battle = Battle();
        BattleActionResolution? selected = null;
        for (uint word = 0; word <= ushort.MaxValue; word++)
        {
            var candidate = Sf2PhysicalAction.Prepare(battle.With(mainSeed: word << 16), new("bat"), new(4, 5), new("hero"), new Sf2BattleProgressionRule());
            if (candidate.Reactions[0].InflictedSleep is not null && candidate.Reactions.Any(reaction => reaction.Action == "physical-counter"))
            { selected = candidate; break; }
        }
        Assert.NotNull(selected);
        Assert.Equal((ushort)0, selected.Prepared.GetActor(new("hero")).Status);
        Assert.Equal(new ActorRef("hero"), selected.Reactions.Last().Actor);
    }

    [Theory]
    [InlineData(0x40)]
    [InlineData(0x80)]
    [InlineData(0xC0)]
    public void AfterTurnUsesTheActualCounterAsRangeAndItsMaskRatherThanCommentProbabilities(ushort counter)
    {
        var battle = Battle();
        var actor = battle.GetActor(new("hero")).With(status: counter);
        var result = SleepTurnRules.Prepare(actor, 0x60001234)!;
        var draw = BattleRandom.NextMain(0x60001234, counter);
        Assert.Equal(draw, result.Draw);
        Assert.Equal((ushort)((draw.Value & 0xC0) == 0 ? 0 : counter - 0x40), result.After);
        Assert.Equal(result.After == 0 ? 354 : 306, result.Text);
        Assert.Equal(counter, actor.Status);
        Assert.Null(SleepTurnRules.Prepare(actor.With(hp: 0), 0x60001234));
    }

    [Fact]
    public void SleepingEntryHoldsOneQueueSlotAndPublishesStatusOnlyAfterActualMessageCompletion()
    {
        var battle = Battle();
        battle = BattleTurnFlow.GenerateRound(battle, out _);
        var queued = BattleTurnFlow.QueuedActor(battle);
        battle = battle.With(mainSeed: 0, actors: battle.Actors.Select(actor => actor.Actor == queued.Actor ? actor.With(status: 0x40) : actor));
        var story = new StoryState(flags: [72]);
        var current = new SessionSnapshot(Guid.NewGuid(), 0, 0, new ActiveBattle(battle, null), story, SessionStopReason.SimulationWait);
        var waiting = BattleAdvancer.Advance(current, []);
        Assert.Null(waiting.Failure);
        Assert.Equal(BattleScenePhase.SleepTurnMessage, waiting.Snapshot.BattleScene!.Phase);
        Assert.Equal(battle.Cursor, waiting.Snapshot.Battle.Cursor);
        Assert.Equal((ushort)0x40, waiting.Snapshot.Battle.GetActor(queued.Actor).Status);
        Assert.Equal(BattleRandom.NextMain(0, 0x40).After, waiting.Snapshot.Battle.MainSeed);
        Assert.Contains(waiting.Observations, observation => observation.Kind == "sleeping-action-skipped");
        Assert.Same(story, waiting.Snapshot.Story);
        var repeated = BattleAdvancer.Advance(waiting.Snapshot, []);
        Assert.Same(waiting.Snapshot, repeated.Snapshot);
        var token = waiting.Snapshot.BattleScene.Token;
        foreach (var command in new SessionCommand[] { new Acknowledge(token), new CompletePresentation(new(token.Value + 1), PresentationCueKind.Gesture),
            new CompletePresentation(token, PresentationCueKind.FadeIn), new Confirm() })
        {
            var rejected = BattleSceneContinuation.Submit(waiting.Snapshot, command);
            Assert.NotNull(rejected.Failure); Assert.Same(waiting.Snapshot, rejected.Snapshot);
        }
        var finished = BattleSceneContinuation.Submit(waiting.Snapshot, new CompletePresentation(token, PresentationCueKind.Gesture));
        Assert.Null(finished.Failure);
        Assert.Equal((ushort)0, finished.Snapshot.Battle.GetActor(queued.Actor).Status);
        Assert.Equal(1, finished.Observations.Count(observation => observation.Kind == "after-turn" && observation.Actor == queued.Actor));
        Assert.Same(story, finished.Snapshot.Story);
        var stale = BattleCommandDispatcher.Submit(finished.Snapshot, new CompletePresentation(token, PresentationCueKind.Gesture));
        Assert.NotNull(stale.Failure); Assert.Same(finished.Snapshot, stale.Snapshot);
        Assert.Empty(stale.Observations);
    }

    [Fact]
    public void SleepInflictionHasItsOwnSceneCompletionAndDoesNotReplayDamageOrReward()
    {
        var battle = Battle();
        BattleActionResolution? action = null;
        for (uint word = 0; word <= ushort.MaxValue; word++)
        {
            var candidate = Sf2PhysicalAction.Prepare(battle.With(mainSeed: word << 16), new("bat"), new(4, 5), new("hero"), new Sf2BattleProgressionRule());
            if (candidate.Reactions.Count == 1 && candidate.Reactions[0].InflictedSleep is not null) { action = candidate; break; }
        }
        Assert.NotNull(action);
        var initial = new SessionSnapshot(Guid.NewGuid(), 0, 0, battle, null, SessionStopReason.SimulationWait);
        var result = BattleSceneContinuation.Begin(initial, action, []);
        while (result.Snapshot.BattleScene!.Phase != BattleScenePhase.SleepMessage)
        {
            var scene = result.Snapshot.BattleScene;
            result = BattleSceneContinuation.Submit(result.Snapshot, scene.RequiresAcknowledgement ? new Acknowledge(scene.Token) : new CompletePresentation(scene.Token, scene.CompletionKind));
            Assert.Null(result.Failure);
        }
        ushort hp = result.Snapshot.Battle.GetActor(new("hero")).Hp;
        Assert.Equal((ushort)0, result.Snapshot.Battle.GetActor(new("hero")).Status);
        var sleeping = BattleSceneContinuation.Submit(result.Snapshot, new CompletePresentation(result.Snapshot.BattleScene.Token, PresentationCueKind.Gesture));
        Assert.Null(sleeping.Failure);
        Assert.Equal((ushort)0xC0, sleeping.Snapshot.Battle.GetActor(new("hero")).Status);
        Assert.Equal(hp, sleeping.Snapshot.Battle.GetActor(new("hero")).Hp);
        Assert.Single(sleeping.Observations, observation => observation.Kind == "sleep-inflicted");
    }

    private static (uint, PhysicalStrike) Strike(Func<PhysicalStrike, bool> predicate)
    {
        for (uint word = 0; word <= ushort.MaxValue; word++)
        {
            var result = PhysicalStrikeRules.Resolve(12, 4, 200, 256, word << 16, 32, 4, 0, sleepCritical: true);
            if (predicate(result)) return (word << 16, result);
        }
        throw new InvalidOperationException("No bounded variant found.");
    }

    private static EngineBattleState Battle(bool flying = false)
    {
        BattleActorDefinition Unit(string id, bool ally) => new(new(id), ally ? BattleClassRule.UnpromotedSwordsman : BattleClassRule.Ordinary,
            1, 200, 0, 10, 4, ally ? (byte)80 : (byte)5, false, 4, [],
            physical: new(ally ? PhysicalCriticalRule.OneIn32WithHalfBonus : PhysicalCriticalRule.Sleep, false, ally, 0),
            mover: flying && !ally ? BattleMover.Flying : BattleMover.Regular,
            sourceLoadout: new([127, 127, 127, 127], [63, 63, 63, 63]));
        var hero = new BattleDeploymentDefinition(Unit("hero", true), BattleFaction.Ally, 0, BattleControl.Player, null,
            new(5, 5), new(null, 0, 15, 15, 0, 0, 255, 255, AllyPartyMember: 0));
        var bat = new BattleDeploymentDefinition(Unit("bat", false), BattleFaction.Enemy, 128, BattleControl.Automatic, new("source-orders"),
            new(4, 5), new(0x2000, 0, 15, 15, 0, 6, 255, 255));
        var terrain = Enumerable.Range(0, 2304).Select(_ => new BattleTerrain(TerrainSurface.Open, TerrainProtection.Heavy)).ToArray();
        if (flying) terrain[4 * 48 + 4] = new(TerrainSurface.HighSky, TerrainProtection.Heavy);
        var definition = new BattleDefinition("sleep", new("field"), 16, 16, terrain, [hero, bat], [], new(false),
            initialization: new([], BattleRegionProgram.None, [hero.Position]), outcome: new(hero.Actor, bat.Actor));
        var start = new BattleStartInput("sleep", [new(hero.Actor, 200, 0, 0, 0, 0, 0, null, ActivationWord: 0),
            new(bat.Actor, 200, 0, null, null, null, 0, null)], 0x12340000, 0x56780000, 500,
            new("controlled", "engine behavior", "Sleep/Flying variants", 0, true, true, true, false, false, 0));
        return BattleTurnFlow.Start(definition, start);
    }
}
