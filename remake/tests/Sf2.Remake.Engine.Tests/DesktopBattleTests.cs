using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using System.Text.Json.Nodes;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class DesktopBattleTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void LearnedBlastFourUsesSingleTargetWindDamageAndItsOwnCost(int resistance)
    {
        var battle = AttackSpellTests.Battle(d =>
        {
            var spell = d["spells"]!.AsArray().Single(s => s!["id"]!.GetValue<string>() == "blast")!;
            spell["level"] = 4; spell["mpCost"] = 8; spell["maximumRange"] = 2;
            spell["effect"]!["power"] = 30; spell["effect"]!["radius"] = 0;
            var learned = d["actors"]![0]!["spells"]!.AsArray().Single(s => s!["id"]!.GetValue<string>() == "blast")!;
            learned["level"] = 4;
            d["start"]!["actors"]![0]!["positionOverride"] = new JsonObject { ["x"] = 3, ["y"] = 2 };
            d["actors"]![3]!["resistance"] = resistance;
        });
        var action = AttackSpellTests.Prepare(battle, "blast", 4);
        Assert.Equal(8, action.AttackSpell!.MpCost);
        var reaction = Assert.Single(action.Reactions);
        Assert.Equal(AttackSpellTests.Center, reaction.Target);
        Assert.True(reaction.Amount > 12);
        Assert.DoesNotContain(action.Reactions, r => r.Target == AttackSpellTests.Wing);
    }

    [Theory]
    [InlineData(0, 32, 2)]
    [InlineData(1, 32, 4)]
    [InlineData(2, 16, 2)]
    [InlineData(20, 4, 4)]
    public void EquippedCriticalIncrementPreservesDoubleCounterAndDoesNotAccumulate(byte increase, int chance, int bonus)
    {
        var basis = SourceEnemyAiTests.State(active: true).Actors[0];
        var equipment = new BattleEquipmentDefinition(11, [new(48, "Authored arrow", 0, 21, 2, 3, increase)]);
        var deployment = basis.Deployment with { Definition = basis.Definition.WithEquipment(equipment) };
        var equipped = new BattleActorState(deployment, basis.Hp, basis.Mp, null, basis.Position, null, null,
            sourceLoadout: new([176, 127, 127, 127], [63, 63, 63, 63]));
        for (int repeat = 0; repeat < 2; repeat++)
        {
            var actual = BattleEquipment.Physical(equipped)!;
            Assert.Equal(2, actual.MinimumRange); Assert.Equal(3, actual.MaximumRange);
            Assert.Equal(chance, actual.Critical.ChanceDenominator);
            Assert.Equal(bonus, actual.Critical.DamageBonusDenominator);
            Assert.Equal(basis.Definition.Physical!.Critical.DoubleChanceDenominator, actual.Critical.DoubleChanceDenominator);
            Assert.Equal(basis.Definition.Physical.Critical.CounterChanceDenominator, actual.Critical.CounterChanceDenominator);
        }
        var unarmed = equipped.With(sourceLoadout: new([48, 127, 127, 127], [63, 63, 63, 63]));
        Assert.Same(basis.Definition.Physical!.Critical, BattleEquipment.Physical(unarmed)!.Critical);
        Assert.Equal(1, BattleEquipment.Physical(unarmed)!.MaximumRange);
        Assert.Same(PhysicalCriticalRule.Poison, PhysicalCriticalRule.Poison.IncreaseCritical(increase));
    }

    [Fact]
    public void HealerFourPrefersHealingToAnAvailableAttack()
    {
        var before = EnemySpellDecisionTests.Battle(d =>
            d["encounters"]![0]!["placements"]![2]!["initialization"]!["commandset"] = 3);
        before = before.With(actors: before.Actors.Select(a => a.Actor == EnemySpellDecisionTests.Scout
            ? a.With(position: new(6, 4)) : a));
        var chosen = EnemySpellDecisionTests.Decide(before, EnemySpellDecisionTests.Cleric);
        Assert.Equal(EnemySpellDecisionTests.Wounded, chosen.Target);
        Assert.Equal(BattleActionKind.Healing, chosen.ChosenAction!.Value.Kind);
    }

    [Theory]
    [InlineData(2, "ai-command-move2")]
    [InlineData(3, "ai-command-move1")]
    public void HealerFallbackUsesItsOwnMovementCommand(byte commandset, string command)
    {
        var before = SourceEnemyAiTests.State(active: true, commandset: commandset,
            primaryRegion: 15, peer: false, blockSouth: false, allyPosition: new(5, 15));
        var actor = before.Actors[0].Actor;
        var chosen = SourceEnemyAi.Resolve(before, actor);
        Assert.Contains(chosen.Effects, e => e.Kind == "ai-command-heal2" && e.After == -1);
        Assert.Contains(chosen.Effects, e => e.Kind == "ai-command-move-order4" && e.After == -1);
        Assert.Contains(chosen.Effects, e => e.Kind == command && e.After == 0);
        Assert.NotEqual(before.GetActor(actor).Position, chosen.Destination);
        Assert.Null(chosen.ChosenAction);
    }
}
