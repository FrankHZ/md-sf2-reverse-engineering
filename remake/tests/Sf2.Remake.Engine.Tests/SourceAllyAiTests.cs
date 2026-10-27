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

    [Fact]
    public void AllyBypassesStandbyCheckedWordAndDeadFollowPromotionAndRetainsEnemyOnlyMemory()
    {
        var b = Initial(); var action = Decide(b);
        Assert.Equal(Sentry, action.Target);
        Assert.Equal(0x8173, action.Battle.Regions!.Tested);
        var ally = action.Battle.GetActor(Ally);
        Assert.Equal((ushort)4, ally.ActivationWord); Assert.Equal((byte)143, ally.PrimaryOrder);
        Assert.Equal((byte)128, ally.SecondaryOrder); Assert.Equal(17, ally.AiMemory); Assert.Equal(Leader, ally.LastTarget);
        Assert.DoesNotContain(action.Effects, e => e.Kind is "source-standby" or "regions-tested-cleared" or "ai-order-promoted");
        Assert.Equal(b.MainSeed, action.Battle.MainSeed);
        Assert.Single(action.Effects, e => e.Kind == "thinking-rng");
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
    [InlineData("item")] [InlineData("spell")] [InlineData("muddle")] [InlineData("neutral")]
    public void UnsupportedCategoriesRejectBeforePublishingDraws(string category)
    {
        var b = Initial(); b = b.With(actors: b.Actors.Select(a =>
            a.Actor == Ally ? a.With(status: category == "muddle" ? (ushort)0x20 : (ushort)0,
                sourceLoadout: new(category == "item" ? [0, 127, 127, 127] : [127, 127, 127, 127],
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
