using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Xunit;
using static Sf2.Remake.Engine.Tests.AttackSpellTests;

namespace Sf2.Remake.Engine.Tests;

public sealed class MasterMonkPhysicalTests
{
    [Theory]
    [InlineData(0x00641234u, true, false)]
    [InlineData(0x00011234u, true, true)]
    [InlineData(0x40001234u, false, false)]
    public void EveryMonkStrikeSelectsItsAnimationBeforeDodgeAndStillDrawsForForcedCritical(
        uint seed, bool special, bool dodge)
    {
        var original = Battle(d =>
        {
            d["encounters"]![0]!["placements"]![3]!["x"] = 3;
            d["actors"]![3]!["maxHp"] = 200;
            d["start"]!["actors"]![3]!["hp"] = 200;
        });
        var old = original.GetActor(Caster);
        var definition = new BattleActorDefinition(Caster, BattleClassRule.MasterMonk, 1, old.MaxHp, old.MaxMp,
            40, old.Defense, old.Agility, false, 5, [],
            old.Definition.Physical! with { Promoted = true, Critical = PhysicalCriticalRule.WolfBaron },
            BattleMover.Regular, new([127, 127, 127, 127], [63, 63, 63, 63]));
        var deployment = old.Deployment with { Definition = definition };
        var source = original.Definition;
        var encounter = source.WithDeployments(source.Deployments.Select(d => d.Actor == Caster ? deployment : d));
        var battle = new EngineBattleState(encounter, original.Actors.Select(a => a.Actor == Caster
            ? new BattleActorState(deployment, old.Hp, old.Mp, old.Exp, old.Position, old.Kills, old.Defeats) : a),
            seed, original.ThinkingSeed, original.Round, original.Queue, original.Cursor, original.Gold);
        var action = BattleActionRules.Prepare(new Sf2PhysicalAction(), battle, Caster, old.Position!,
            new(BattleActionKind.Physical), Center, new Sf2BattleProgressionRule());
        var first = action.Reactions[0];
        Assert.Equal(special, first.SpecialAnimation);
        Assert.Equal(dodge, first.Kind == BattleReactionKind.Dodge);
        if (special && !dodge) Assert.True(first.Critical);
        var draws = action.ConstructionEffects.Where(e => e.Kind.StartsWith("rng-")).ToArray();
        Assert.Equal("rng-monk-special", draws[0].Kind);
        Assert.Equal((ushort)16, draws[0].RandomRange);
        Assert.Equal("rng-dodge", draws[1].Kind);
        if (!dodge)
        {
            Assert.Equal("rng-critical", draws[2].Kind);
            Assert.Equal(special ? (ushort)1 : (ushort)16, draws[2].RandomRange);
        }
        Assert.Equal(battle.MainSeed, seed);
    }
}
