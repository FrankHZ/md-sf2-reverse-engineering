using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleGrowthTests
{
    private static BattleActorState Actor(byte exp = 99, byte level = 1, bool learn = false)
    {
        var curve = Array.AsReadOnly(Enumerable.Repeat(new StatGrowthFraction(128, 128), 29).ToArray());
        var growth = new BattleGrowthDefinition(4, 3,
            new StatGrowth[] { new(10, 30, curve), new(4, 24, curve), new(5, 25, curve), new(3, 23, curve), new(6, 26, curve) },
            learn ? [new(2, 64)] : [], new Dictionary<byte, SpellRef> { [0] = new("heal", 1), [64] = new("heal", 2) });
        var definition = new BattleActorDefinition(new("priest"), BattleClassRule.UnpromotedPriest,
            level, 10, 4, 8, 3, 6, false, 5, [new("heal", 1)],
            sourceLoadout: new([128, 127, 127, 127], [0, 63, 63, 63]), growth: growth);
        var deployment = new BattleDeploymentDefinition(definition, BattleFaction.Ally, 1,
            BattleControl.Player, null, new MapPosition(1, 1));
        return new(deployment, 7, 2, exp, deployment.Position, 0, 0);
    }

    [Fact]
    public void AwardSeparatesCurrentResourcesAndDefinitionFromGrowthAndEquipmentRefresh()
    {
        var before = Actor();
        uint seed = 0x1234;
        List<BattleEffect> effects = [];
        var after = BattleGrowthRules.Award(before, 24, ref seed, effects);
        Assert.Equal((byte)23, after.Exp);
        Assert.Equal(2, after.Level);
        Assert.Equal(7, after.Hp); Assert.Equal(2, after.Mp);
        Assert.Equal(20, after.MaxHp); Assert.Equal(14, after.MaxMp);
        Assert.Equal(15, after.BaseAttack); Assert.Equal(18, after.Attack);
        Assert.Equal(13, after.Defense); Assert.Equal(16, after.Agility);
        Assert.Equal(1, before.Definition.Level); Assert.Equal(10, before.Definition.MaxHp);
        Assert.Same(before.Definition, after.Definition);
        Assert.Equal(10, effects.Count(effect => effect.Kind.StartsWith("rng-growth-", StringComparison.Ordinal)));
        Assert.NotEqual(0x1234u, seed);
    }

    [Fact]
    public void SaturatedExperienceProcessesOnlyOneThresholdAtTheClassCapWithoutDrawing()
    {
        var before = Actor(199, 40);
        uint seed = 0x1234;
        List<BattleEffect> effects = [];
        var after = BattleGrowthRules.Award(before, 24, ref seed, effects);
        Assert.Equal((byte)100, after.Exp); Assert.Equal(40, after.Level);
        Assert.Equal(before.Hp, after.Hp); Assert.Equal(before.Attack, after.Attack);
        Assert.Equal(0x1234u, seed);
        Assert.Single(effects, effect => effect.Kind == "exp-threshold");
        Assert.Contains(effects, effect => effect.Kind == "level-cap" && effect.After == 255);
    }

    [Fact]
    public void LearnedUpgradeChangesLiveSpellChoicesAndPackedSpellbookTogether()
    {
        var before = Actor(learn: true);
        uint seed = 0x1234;
        List<BattleEffect> effects = [];
        var after = BattleGrowthRules.Award(before, 24, ref seed, effects);
        Assert.Contains(new SpellRef("heal", 1), after.Spells);
        Assert.Contains(new SpellRef("heal", 2), after.Spells);
        Assert.Equal(64, after.SourceLoadout!.Spells[0]);
        Assert.Equal(0, before.SourceLoadout!.Spells[0]);
        Assert.Equal(before.SourceLoadout.Items, after.SourceLoadout.Items);
    }
}
