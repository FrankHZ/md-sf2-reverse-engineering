using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class Battle17RulesTests
{
    [Theory]
    [InlineData(TerrainSurface.Open, 2)]
    [InlineData(TerrainSurface.Brush, 2)]
    [InlineData(TerrainSurface.Rough, 3)]
    [InlineData(TerrainSurface.Deep, 3)]
    [InlineData(TerrainSurface.Desert, 3)]
    [InlineData(TerrainSurface.Water, -1)]
    public void BrassGunnerUsesGroundMovementCosts(TerrainSurface surface, int cost)
    {
        Assert.Equal(cost, BattleTerrainRules.MovementCost(new(surface, TerrainProtection.None), BattleMover.BrassGunner));
    }

    [Theory]
    [InlineData(BattleMover.BrassGunner, BattleMover.Flying, 25)]
    [InlineData(BattleMover.Archer, BattleMover.Hovering, 25)]
    [InlineData(BattleMover.CentaurArcher, BattleMover.Flying, 25)]
    [InlineData(BattleMover.StealthArcher, BattleMover.Hovering, 25)]
    [InlineData(BattleMover.BrassGunner, BattleMover.Regular, 20)]
    [InlineData(BattleMover.Mage, BattleMover.Flying, 20)]
    public void ArcherBonusUsesBothMovers(BattleMover attacker, BattleMover defender, int damage)
    {
        Assert.Equal(damage, PhysicalStrikeRules.LandDamage(30, 10, 256, Sf2PhysicalAction.ArcherBonus(attacker, defender)));
    }

    [Fact]
    public void StunCriticalUsesTheOrdinaryDamageDrawsAndRequiresASurvivor()
    {
        var stun = PhysicalStrikeRules.Resolve(12, 4, 20, 256, 0, 32, 1, 0,
            sleepingDefender: true, stunCritical: true);
        var ordinary = PhysicalStrikeRules.Resolve(12, 4, 20, 256, 0, 32, 1, 0, sleepingDefender: true);
        Assert.True(stun.StunInflicted);
        Assert.False(stun.Critical);
        Assert.Equal(ordinary.Damage, stun.Damage);
        Assert.Equal(ordinary.Seed, stun.Seed);
        Assert.False(PhysicalStrikeRules.Resolve(12, 4, 1, 256, 0, 32, 1, 0,
            sleepingDefender: true, stunCritical: true).StunInflicted);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void StunSkipsSelectionAndSettlesBeforePoison(int recoveryDraw)
    {
        var actor = new ActorRef("receiver");
        var original = PoisonTurnTests.Snapshot(actor, 10, new(new ActorRef("champion")));
        uint seed = 0;
        while (BattleRandom.NextMain(seed, 2).Value != recoveryDraw) seed++;
        var battle = original.Battle.With(mainSeed: seed, actors: original.Battle.Actors.Select(
            unit => unit.Actor == actor ? unit.With(status: 3, facing: 1) : unit));
        var ready = new SessionSnapshot(original.SessionId, 0, 0, battle, null, SessionStopReason.SimulationWait);
        var rules = new SessionRules("status-turn", new Sf2HealingRule());
        var hold = BattleAdvancer.Advance(ready, [], rules);
        Assert.Null(hold.Failure);
        Assert.Null(hold.Snapshot.Selection);
        Assert.Equal(BattleScenePhase.StunTurnMessage, hold.Snapshot.BattleScene!.Phase);
        Assert.Contains(hold.Observations, e => e.Kind == "stunned-action-skipped" && e.Actor == actor);
        Assert.Equal((ushort)3, hold.Snapshot.Battle.GetActor(actor).Status);

        var poison = BattleSceneContinuation.Submit(hold.Snapshot, PoisonTurnTests.Command(hold.Snapshot), rules);
        Assert.Null(poison.Failure);
        Assert.Equal(BattleScenePhase.PoisonDamageMessage, poison.Snapshot.BattleScene!.Phase);
        Assert.Equal((ushort)(recoveryDraw == 0 ? 2 : 3), poison.Snapshot.Battle.GetActor(actor).Status);
        Assert.Equal(3, poison.Snapshot.Battle.GetActor(actor).Facing);
        Assert.Equal((ushort)10, poison.Snapshot.Battle.GetActor(actor).Hp);
    }
}
