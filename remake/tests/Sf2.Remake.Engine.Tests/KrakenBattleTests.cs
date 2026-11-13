using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class KrakenBattleTests
{
    [Theory]
    [InlineData(BattleMover.Aquatic, false, true)]
    [InlineData(BattleMover.Regular, true, false)]
    [InlineData(BattleMover.Flying, true, true)]
    public void ShipDeckAndSeaUseTheActorsOwnMovement(BattleMover mover, bool deck, bool sea)
    {
        var terrain = Enumerable.Repeat(new BattleTerrain(TerrainSurface.Barrier, TerrainProtection.None), 2304).ToArray();
        terrain[5 * 48 + 4] = new(TerrainSurface.Open, TerrainProtection.None);
        terrain[5 * 48 + 6] = new(TerrainSurface.Water, TerrainProtection.None);
        var stops = BattleTerrainRules.ReachableCells(terrain, new(5, 5), mover, 1, []);
        Assert.Equal(deck, stops.Contains(new MapPosition(4, 5)));
        Assert.Equal(sea, stops.Contains(new MapPosition(6, 5)));
        Assert.DoesNotContain(new MapPosition(5, 4), stops);
    }

    [Theory]
    [InlineData(16, 16, true)]
    [InlineData(15, 16, false)]
    [InlineData(16, 0, false)]
    public void SwarmChecksCurrentHealthAndDefeatedEnemiesBeforePursuit(int hp, int peerHp, bool waits)
    {
        var template = SourceEnemyAiTests.State(active: true, commandset: 15, primaryRegion: 15);
        var original = template.Actors[0];
        var deployment = original.Deployment with
        {
            Initialization = original.Deployment.Initialization! with { SwarmThreshold = 1 },
        };
        var actor = new BattleActorState(deployment, (ushort)hp, 0, null, original.Position, null, null,
            activationWord: original.ActivationWord);
        var peer = template.Actors[2].With(hp: (ushort)peerHp);
        var before = template.With(actors: [actor, template.Actors[1], peer]);
        var result = SourceEnemyAi.Resolve(before, actor.Actor);
        Assert.Equal(waits, result.Effects.Any(effect => effect.Kind == "swarm-wait"));
        if (waits)
        {
            Assert.Equal(actor.Position, result.Destination);
            Assert.Equal(before.MainSeed, result.Battle.MainSeed);
            Assert.Equal(before.ThinkingSeed, result.Battle.ThinkingSeed);
        }
        else Assert.Contains(result.Effects, effect => effect.Kind == "ai-command-move2");
    }

    [Fact]
    public void SilencedEnemyAtZeroMpChoosesAndResolvesAquaOverPhysical()
    {
        var template = EnemySpellDecisionTests.Battle(d =>
        {
            var spell = d["spells"]![0]!;
            spell["id"] = "aqua"; spell["mpCost"] = 0; spell["maximumRange"] = 3;
            spell["effect"]!["element"] = "ice"; spell["effect"]!["power"] = 13;
            d["actors"]![1]!["spells"]![0]!["id"] = "aqua";
            d["actors"]![0]!["resistance"] = 2 << 4;
        });
        var mage = EnemySpellDecisionTests.Mage;
        var before = template.With(actors: template.Actors.Select(a => a.Actor == mage
            ? a.With(position: new(3, 2), mp: 0, status: SilenceRules.Mask) : a));
        var decision = EnemySpellDecisionTests.Decide(before, mage);
        var action = new BattleActionRef(BattleActionKind.AttackSpell, new("aqua", 1));
        Assert.Equal(action, decision.ChosenAction);
        Assert.DoesNotContain(decision.Effects, e => e.Kind == "rng-ai-attack-category");
        var result = BattleActionRules.Prepare(RuleCompositions.Sf2().Action(action), decision.Battle, mage,
            decision.Destination, action, decision.Target, new Sf2BattleProgressionRule());
        Assert.Null(result.Silenced);
        Assert.Equal((ushort)8, result.ConstructionEffects.Single(e => e.Kind == "rng-spell-critical").RandomRange);
        Assert.InRange(Assert.Single(result.Reactions).Amount, 4, 9);
        Assert.Equal(0, result.Prepared.GetActor(mage).Mp);
    }
}
