using System.Text.Json.Nodes;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Xunit;
using static Sf2.Remake.Engine.Tests.AttackSpellTests;

namespace Sf2.Remake.Engine.Tests;

public sealed class SummonSpellTests
{
    [Theory]
    [InlineData("dao", "wind", 18, 8, 32, false)]
    [InlineData("dao", "wind", 18, 8, 32, true)]
    [InlineData("apollo", "fire", 23, 10, 32, true)]
    [InlineData("neptun", "ice", 30, 12, 16, true)]
    [InlineData("atlas", "neutral", 35, 14, 32, true)]
    public void ActualRecipientsDividePromotedPowerBeforeResistanceAndRandomDraws(
        string id, string element, int power, int cost, int criticalRange, bool multiple)
    {
        var battle = Battle(d =>
        {
            d["actors"]![0]!["classRule"] = "ordinary";
            d["actors"]![0]!["physical"]!["promoted"] = true;
            ((JsonArray)d["actors"]![0]!["spells"]!).Add(new JsonObject { ["id"] = id, ["level"] = 1 });
            ((JsonArray)d["spells"]!).Add(new JsonObject
            {
                ["id"] = id, ["level"] = 1, ["mpCost"] = cost, ["minimumRange"] = 1, ["maximumRange"] = 2,
                ["effect"] = new JsonObject { ["kind"] = "attack", ["power"] = power, ["radius"] = 1, ["element"] = element }
            });
            d["start"]!["actors"]![3]!["hp"] = 30;
            d["actors"]![3]!["resistance"] = 0;
            d["actors"]![2]!["resistance"] = 0;
            if (!multiple) d["start"]!["actors"]![2]!["hp"] = 0;
        });
        var rule = new Sf2AttackSpellAction();
        var command = new BattleActionRef(BattleActionKind.AttackSpell, new(id, 1));
        var seed = battle.MainSeed;
        var count = multiple ? 2 : 1;
        var recipients = Sf2AttackSpellAction.ResolveTargets(battle, Caster, battle.GetActor(Caster).Position!,
            rule.RequireAction(battle, Caster, command).AttackSpell!, Center);
        Assert.Equal(count, recipients.Count);
        Assert.Equal(seed, battle.MainSeed);
        var action = Prepare(battle, id, 1);
        Assert.Equal(recipients.Select(t => t.Actor), action.Reactions.Select(t => t.Target));
        int adjusted = power * 5 / 4 / count;
        foreach (var reaction in action.Reactions)
        {
            var draws = action.ConstructionEffects.Where(e => e.Target == reaction.Target && e.Kind.StartsWith("rng-spell-")).ToArray();
            Assert.Equal((ushort)criticalRange, draws[0].RandomRange);
            int damage = adjusted + (reaction.Critical ? adjusted / 4 : 0);
            Assert.Equal((ushort)(damage / 8 + 1), draws[1].RandomRange);
            Assert.Equal(Math.Max(1, damage - draws[1].RandomValue!.Value - draws[2].RandomValue!.Value), reaction.Amount);
        }
        Assert.Equal(20 - cost, action.ApplySpellCost(action.Prepared).GetActor(Caster).Mp);
        Assert.Equal(seed, battle.MainSeed);
    }
}
