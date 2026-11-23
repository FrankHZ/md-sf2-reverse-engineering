using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;
using static Sf2.Remake.Engine.Tests.JaroConversionTests;

namespace Sf2.Remake.Engine.Tests;

public sealed class SourceAllyAiTests
{
    private static EngineBattleState Initial()
    {
        var input = Admitted("jaro-conversion");
        var b = BattleTurnFlow.Start(input.Definition.Encounters[input.Start.Encounter], input.Start);
        return b.With(regions: new(new bool[16], 0x8173), actors: b.Actors.Select(a => a.Actor == Ally
            ? a.With(position: new(4, 2), activationWord: 4, aiMemory: 17, lastTarget: Leader, primaryOrder: 143, secondaryOrder: 128)
            : a.Actor == Enemy ? a.With(hp: 0, clearPosition: true) : a));
    }
    private static BattleAutomaticAction Decide(EngineBattleState b) =>
        BattleDecisionRules.Decide(new SourceAllyAi(), b, Ally, new Sf2PhysicalAction(), new Sf2BattleProgressionRule());

    [Theory]
    [InlineData(0)]
    [InlineData(0x20)]
    public void AllyBypassesStandbyCheckedWordAndDeadFollowPromotionAndRetainsEnemyOnlyMemory(ushort status)
    {
        var b = Initial();
        b = b.With(actors: b.Actors.Select(a => a.Actor == Ally ? a.With(status: status) : a));
        var action = Decide(b);
        Assert.Equal(Sentry, action.Target);
        Assert.Equal(0x8173, action.Battle.Regions!.Tested);
        var ally = action.Battle.GetActor(Ally);
        Assert.Equal(BattleControl.Automatic, ally.Control); Assert.Equal(status, ally.Status);
        Assert.Equal((ushort)4, ally.ActivationWord); Assert.Equal((byte)143, ally.PrimaryOrder);
        Assert.Equal((byte)128, ally.SecondaryOrder); Assert.Equal(17, ally.AiMemory); Assert.Equal(Leader, ally.LastTarget);
        Assert.DoesNotContain(action.Effects, e => e.Kind is "source-standby" or "regions-tested-cleared" or "ai-order-promoted");
        Assert.Equal(b.MainSeed, action.Battle.MainSeed);
        Assert.Single(action.Effects, e => e.Kind == "thinking-rng");
    }

    [Fact]
    public void PermanentAutomaticAllyGrowsFromPhysicalExperienceWithAnActiveMuddleOneCounter()
    {
        var before = Initial();
        var ally = before.GetActor(Ally);
        var curve = Enumerable.Repeat(new StatGrowthFraction(128, 128), 29).ToArray();
        var growth = new BattleGrowthDefinition(13, 0, [
            new(24, 44, curve), new(3, 13, curve), new(12, 32, curve), new(8, 28, curve), new(80, 94, curve)
        ], [], new Dictionary<byte, SpellRef>());
        var deployment = ally.Deployment with { Definition = ally.Definition.WithGrowth(growth) };
        var attacker = new BattleActorState(deployment, ally.Hp, ally.Mp, 99, ally.Position, ally.Kills, ally.Defeats,
            status: 0x10, activationWord: 4, sourceLoadout: ally.SourceLoadout);
        var definition = before.Definition.WithDeployments(before.Definition.Deployments.Select(d => d.Actor == Ally ? deployment : d));
        var battle = new EngineBattleState(definition,
            before.Actors.Select(a => a.Actor == Ally ? attacker : a.Actor == Sentry ? a.With(status: 1) : a),
            before.MainSeed, before.ThinkingSeed, before.Round, [new(Ally, 80), new(Leader, 60), new(null, 255)],
            0, before.Gold, before.StartPolicy, before.Regions);
        var rules = RuleCompositions.Sf2().Bind([definition]);
        var start = new SessionSnapshot(Guid.NewGuid(), 0, 0, new ActiveBattle(battle, null), new StoryState(), SessionStopReason.SimulationWait);
        var result = BattleAdvancer.Advance(start, [], rules);
        var observations = new List<SessionObservation>();
        for (int steps = 0; ; steps++)
        {
            Assert.True(steps < 100);
            Assert.Null(result.Failure);
            observations.AddRange(result.Observations);
            var current = result.Snapshot;
            if (current.Selection is not null) break;
            result = current.BattleMovement is { } move
                ? BattleMovementContinuation.Submit(current, new CompletePresentation(move.Token, move.CompletionKind), rules)
                : current.BattleScene is { } scene
                    ? BattleSceneContinuation.Submit(current, scene.RequiresAcknowledgement ? new Acknowledge(scene.Token)
                        : new CompletePresentation(scene.Token, scene.CompletionKind), rules)
                    : BattleAdvancer.Advance(current, [], rules);
        }

        var after = result.Snapshot.Battle;
        var grown = after.GetActor(Ally);
        Assert.Equal(Leader, result.Snapshot.Selection!.Actor);
        Assert.True(after.GetActor(Sentry).Hp < before.GetActor(Sentry).Hp);
        var threshold = Assert.Single(observations, e => e.Kind == "exp-threshold" && e.Actor == Ally);
        var level = Assert.Single(observations, e => e.Kind == "level" && e.Actor == Ally);
        var expiry = Assert.Single(observations, e => e.Kind == "muddle-status" && e.Actor == Ally);
        Assert.True(threshold.Sequence < level.Sequence && level.Sequence < expiry.Sequence);
        Assert.Equal(0x10, expiry.Before);
        Assert.Equal(0, grown.Status);
        Assert.Equal(BattleControl.Automatic, grown.Control);
        Assert.Equal((ushort?)4, grown.ActivationWord);
        Assert.Equal(ally.Level + 1, grown.Level);
        Assert.Equal(threshold.After, grown.Exp);
        Assert.True(grown.MaxHp > ally.MaxHp);
        Assert.Equal(ally.Hp, grown.Hp);
        Assert.Equal(10, observations.Count(e => e.Kind is "rng-growth-plus" or "rng-growth-minus"));
    }

    [Fact]
    public void RawPriorityTiesUseSourceSlotsAndMovementWithoutEnemyClassPreferenceOrSelectionRng()
    {
        var b = Initial();
        uint seed = Enumerable.Range(0, 65536).Select(n => (ushort)n).First(s =>
        { var d = BattleRandom.NextThinkingWord(s, 3); return d.Value != 0 && BattleRandom.NextThinkingWord(d.After, 3).Value != 0; });
        b = b.With(thinkingSeed: seed << 16 | 0x1234, actors: b.Actors.Select(a => a.Actor == Sentry ? a.With(position: new(5, 2)) :
            a.Actor == Enemy ? a.With(hp: 59, position: new(4, 1)) : a).Reverse());
        var action = Decide(b);
        // Both untyped ordinary enemy classes would fail the enemy >=15 class table.
        Assert.Equal(Sentry, action.Target);
        Assert.Equal(new[] { Enemy, Sentry }, action.Effects.Where(e => e.Kind == "thinking-rng").Select(e => e.Target!.Value));
        Assert.All(action.Effects.Where(e => e.Kind == "ai-candidate"), e => Assert.Equal(19, e.After));
        Assert.Equal(2, action.Effects.Count(e => e.Kind == "thinking-rng"));
        Assert.Equal(0x1234u, action.Battle.ThinkingSeed & 65535); Assert.Equal(Leader, action.Battle.GetActor(Ally).LastTarget);
    }

    [Fact]
    public void AutomaticAllyCanFightWithAnEquippedPowerRing()
    {
        var before = Initial();
        var ally = before.GetActor(Ally);
        var loadout = new BattleSourceLoadout([147, 127, 127, 127], [63, 63, 63, 63]);
        var equipment = new BattleEquipmentDefinition(ally.Attack, [], allowChanges: false,
            rings: [new(19, "Power Ring", uint.MaxValue, RingStat.Attack, 5)]);
        var equipped = new BattleActorState(ally.Deployment with { Definition = ally.Definition.WithEquipment(equipment) },
            ally.Hp, ally.Mp, ally.Exp, ally.Position, ally.Kills, ally.Defeats, sourceLoadout: loadout, activationWord: 4);
        var result = Decide(before.With(actors: before.Actors.Select(a => a.Actor == Ally ? equipped : a)));
        Assert.Equal(Sentry, result.Target);
        Assert.Equal(ally.Attack + 5, result.Battle.GetActor(Ally).Attack);
        Assert.DoesNotContain(result.Effects, effect => effect.Kind == "item-used");
    }

    [Fact]
    public void NoAttackRunsHealSupportMoveInOrderWithoutRandomDrawsAndApproachesOpponent()
    {
        var b = Initial(); b = b.With(actors: b.Actors.Select(a => a.Actor == Ally ? a.With(position: new(0, 0)) : a));
        var action = Decide(b);
        Assert.Null(action.Target); Assert.NotEqual(b.GetActor(Ally).Position, action.Destination);
        Assert.Equal(new[] { "ai-command-attack1", "ai-command-heal1", "ai-command-support", "ai-command-move1" },
            action.Effects.Where(e => e.Kind.StartsWith("ai-command-")).Select(e => e.Kind));
        Assert.Equal(Sentry, action.Effects.Single(e => e.Kind == "ai-move-target").Target);
        Assert.Equal(b.ThinkingSeed, action.Battle.ThinkingSeed); Assert.Equal(b.MainSeed, action.Battle.MainSeed);
        Assert.Same(b.Regions, action.Battle.Regions);
    }

    [Theory]
    [InlineData("item")] [InlineData("spell")] [InlineData("neutral")]
    public void UnsupportedCategoriesRejectBeforePublishingDraws(string category)
    {
        var b = Initial(); b = b.With(actors: b.Actors.Select(a =>
            a.Actor == Ally ? a.With(sourceLoadout: new(category == "item" ? [4, 127, 127, 127] : [127, 127, 127, 127],
                    category == "spell" ? [1, 63, 63, 63] : [63, 63, 63, 63])) :
            a.Actor == Sentry && category == "neutral" ? a.With(activationWord: 8) : a));
        var before = b.ThinkingSeed;
        Assert.True(Assert.Throws<BattleRuleException>(() => Decide(b)).Unsupported);
        Assert.Equal(before, b.ThinkingSeed);
    }

    [Fact]
    public void ClearingLiveAiBitAtProgramSeamReturnsQueuedAllyToOrdinaryManualMovement()
    {
        var b = Initial().With(queue: [new(Ally, 80)], cursor: 0);
        var snapshot = new SessionSnapshot(Guid.NewGuid(), 0, 0, new ActiveBattle(b, null), new StoryState(), SessionStopReason.SimulationWait);
        var changed = BattleStoryOperations.JoinAi(snapshot, new(23, 0), []);
        var result = BattleAdvancer.Advance(changed, [], RuleCompositions.Sf2().Bind([b.Definition]));
        Assert.Null(result.Failure); Assert.Equal(Ally, result.Snapshot.Selection!.Actor);
        Assert.Equal(BattleSelectionStage.Movement, result.Snapshot.Selection.Stage);
        Assert.Equal(BattleControl.Player, result.Snapshot.Battle.GetActor(Ally).Control);
    }
}
