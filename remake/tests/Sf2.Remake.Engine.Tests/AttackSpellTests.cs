using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class AttackSpellTests
{
    internal static readonly ActorRef Caster = new("mage"), Center = new("center"), Wing = new("wing");
    internal static EngineBattleState Battle(Action<JsonNode>? change = null)
    {
        var document = Document("attack-spells"); change?.Invoke(document);
        var accepted = Assert.IsType<ScenarioReadAccepted>(Reader(document).Read());
        return BattleTurnFlow.Start(accepted.Definition.Encounters.Values.Single(), accepted.Start);
    }
    internal static BattleActionResolution Prepare(EngineBattleState battle, string id = "blaze", byte rank = 2) =>
        BattleActionRules.Prepare(new Sf2AttackSpellAction(), battle, Caster, battle.GetActor(Caster).Position!,
            new(BattleActionKind.AttackSpell, new(id, rank)), Center, new Sf2BattleProgressionRule());

    [Theory]
    [InlineData(0, false, 0x12340000u, 10, false)]
    [InlineData(64, false, 0x12340000u, 8, false)]
    [InlineData(128, false, 0x12340000u, 5, false)]
    [InlineData(192, true, 0u, 18, true)]
    public void BlazeMatchesContractResistancePromotionCriticalAndDownwardVariance(int resistance, bool promoted,
        uint seed, int damage, bool critical)
    {
        var battle = Battle(d =>
        {
            d["start"]!["mainSeed"] = seed;
            d["start"]!["actors"]![3]!["hp"] = 30;
            d["actors"]![3]!["resistance"] = resistance;
            if (promoted) { d["actors"]![0]!["classRule"] = "ordinary"; d["actors"]![0]!["physical"]!["promoted"] = true; }
            d["spells"]![1]!["effect"]!["radius"] = 0;
        });
        var action = Prepare(battle); var hit = Assert.Single(action.Reactions);
        Assert.Equal(damage, hit.Amount); Assert.Equal(critical, hit.Critical);
        Assert.Equal(30 - damage, hit.HpAfter); Assert.Equal(30, action.Prepared.GetActor(Center).Hp);
        Assert.Equal(20, action.Prepared.GetActor(Caster).Mp); Assert.Null(action.Spell);
        Assert.Equal(5, action.ConstructionEffects.Count(e => e.RandomRange is not null));
        Assert.Equal(new ushort?[] { 32, (ushort)(damage / 8 + 1), (ushort)(damage / 8 + 1), 16, 16 },
            action.ConstructionEffects.Where(e => e.RandomRange is not null).Select(e => e.RandomRange));
    }

    [Theory]
    [InlineData(0, 6)] [InlineData(1, 5)] [InlineData(2, 3)] [InlineData(3, 7)]
    public void BlastUsesWindBitsIndependentlyOfFire(int wind, int damage)
    {
        var battle = Battle(d =>
        {
            d["start"]!["mainSeed"] = 0x12340000;
            d["start"]!["actors"]![0]!["positionOverride"] = new JsonObject { ["x"] = 3, ["y"] = 2 };
            d["start"]!["actors"]![3]!["hp"] = 30;
            d["actors"]![3]!["resistance"] = 192 | wind;
        });
        var action = Prepare(battle, "blast", 1);
        Assert.Equal(damage, Assert.Single(action.Reactions).Amount);
        Assert.Equal(2, action.AttackSpell!.MpCost);
    }

    [Fact]
    public void RadiusUsesLiveOpponentsManhattanBoundaryAndSourceOrder()
    {
        var battle = Battle(); var action = Prepare(battle);
        Assert.Equal(new[] { Wing, Center }, action.Reactions.Select(r => r.Target));
        Assert.DoesNotContain(action.Reactions, r => r.Target == Caster);
        Assert.Equal(7u, action.Prepared.Gold - battle.Gold);
        Assert.Equal(49, action.ConstructionEffects.Last(e => e.Kind == "spell-exp-contribution").After);
        Assert.Equal(8, action.ConstructionEffects.Count(e => e.RandomRange is not null));
        var diagonal = Battle(d => { d["encounters"]![0]!["placements"]![2]!["x"] = 5; });
        Assert.Equal(Center, Assert.Single(Prepare(diagonal).Reactions).Target);
        foreach (string excluded in new[] { "dead", "absent", "ally" })
        {
            var changed = Battle(d =>
            {
                if (excluded == "dead") d["start"]!["actors"]![2]!["hp"] = 0;
                if (excluded == "absent") d["start"]!["actors"]![2]!["absent"] = true;
                if (excluded == "ally")
                {
                    var p = d["encounters"]![0]!["placements"]![2]!;
                    p["faction"] = "ally"; p["control"] = "player"; p["aiStrategy"] = null;
                }
            });
            Assert.Equal(Center, Assert.Single(Prepare(changed).Reactions).Target);
        }
    }

    [Theory]
    [InlineData(1, true)] [InlineData(2, true)] [InlineData(3, false)]
    public void TargetCenterMustMeetRangeButRadiusMayExtendPastIt(int distance, bool legal)
    {
        var battle = Battle(d => d["encounters"]![0]!["placements"]![3]!["x"] = 2 + distance);
        var rule = new Sf2AttackSpellAction();
        Action check = () => rule.RequireTarget(battle, Caster, new(2, 2), new(BattleActionKind.AttackSpell, new("blaze", 2)), Center);
        if (legal) check(); else Assert.Equal("target-range", Assert.Throws<BattleRuleException>(check).Code);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void DamageAndKillExpShareOneCapThenBattleHalvingAndAwardPair(bool half)
    {
        var battle = Battle(d =>
        {
            d["encounters"]![0]!["rewards"]!["halvedExperience"] = half;
            d["start"]!["actors"]![2]!["hp"] = 1;
        });
        var action = Prepare(battle);
        Assert.All(action.Reactions, reaction => Assert.Equal(0, reaction.HpAfter));
        Assert.Equal(114u, action.Prepared.Gold);
        Assert.InRange(action.Reward!.Amount, half ? 23 : 48, half ? 25 : 50);
        Assert.Equal(2, action.ConstructionEffects.Count(e => e.Kind.StartsWith("rng-exp-")));
        Assert.All(action.ConstructionEffects.Where(e => e.Kind == "spell-exp-contribution"), e => Assert.Equal(49, e.After));
    }

    [Fact]
    public void ChangedSeedsAreDeterministicAndDamageNeverExceedsCriticalPower()
    {
        foreach (uint seed in new uint[] { 0, 1, 5, 0x1234, 0xabcdef, uint.MaxValue })
        {
            var battle = Battle(d => d["start"]!["mainSeed"] = seed);
            var first = Prepare(battle); var second = Prepare(battle);
            Assert.Equal(first.Reactions, second.Reactions); Assert.Equal(first.ConstructionEffects, second.ConstructionEffects);
            Assert.Equal(first.Prepared.MainSeed, second.Prepared.MainSeed);
            Assert.All(first.Reactions, r => Assert.InRange(r.Amount, r.Critical ? 10 : 8, r.Critical ? 12 : 10));
            Assert.Equal(battle.ThinkingSeed, first.Prepared.ThinkingSeed);
        }
    }
}
