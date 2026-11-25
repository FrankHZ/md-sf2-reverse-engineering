using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class EquipmentProwessTests
{
    private static BattleWeaponDefinition Weapon(byte item) => new(item, "Equipment", uint.MaxValue, 0, 1, 1,
        DoubleIncrease: item == 108 ? (byte)1 : (byte)0,
        CounterIncrease: item == 68 ? (byte)1 : (byte)0,
        CriticalSetting: item == 109 ? (byte)6 : null);

    private static BattleActorState Equipped(BattleActorState original, byte item, ushort resistance = 0,
        PhysicalCriticalRule? prowess = null, byte level = 1)
    {
        var equipment = new BattleEquipmentDefinition(20, [Weapon(item)]);
        var definition = new BattleActorDefinition(original.Actor, BattleClassRule.Ninja, level, 500, 0,
            20, 4, 20, false, 6, [], original.Definition.Physical! with
            { Critical = prowess ?? PhysicalCriticalRule.Ninja }, equipment: equipment, resistance: resistance);
        return new(original.Deployment with { Definition = definition }, 500, 0, original.Exp,
            original.Position, original.Kills, original.Defeats, sourceLoadout: new([(ushort)(128 + item), 127, 127, 127], []));
    }

    private static EngineBattleState Battle(byte weapon, byte defenderWeapon = 57, ushort resistance = 0, uint seed = 0x01001234)
    {
        var battle = Start("stone-court").Current.Battle;
        var actor = Equipped(battle.Actors[0], weapon);
        var defender = Equipped(battle.Actors[2], defenderWeapon, resistance).With(position: new(actor.Position!.X - 1, actor.Position.Y));
        return battle.With(actors: battle.Actors.Select(a => a.Actor == actor.Actor ? actor : a.Actor == defender.Actor ? defender : a), mainSeed: seed);
    }

    private static BattleActionResolution Prepare(EngineBattleState battle, bool reverse = false)
    {
        var actor = battle.Actors[reverse ? 2 : 0]; var target = battle.Actors[reverse ? 0 : 2];
        return BattleActionRules.Prepare(new Sf2PhysicalAction(), battle, actor.Actor, actor.Position!,
            new(BattleActionKind.Physical), target.Actor, new Sf2BattleProgressionRule());
    }

    [Theory]
    [InlineData(68, 8, 16, 4)]
    [InlineData(108, 8, 8, 32)]
    [InlineData(109, 4, 16, 8)]
    public void LiveEquippedProwessRestoresAndUnequipsWithoutChangingBase(byte item, int critical, int twice, int counter)
    {
        var actor = Battle(item).Actors[0];
        var rule = BattleEquipment.Physical(actor)!.Critical;
        Assert.Equal((critical, twice, counter), ((int)rule.ChanceDenominator, rule.DoubleChanceDenominator, rule.CounterChanceDenominator));
        Assert.Equal(8, actor.Definition.Physical!.Critical.ChanceDenominator);
        var restored = new BattleActorState(actor.Deployment, actor.Hp, 0, actor.Exp, actor.Position, 0, 0, sourceLoadout: actor.SourceLoadout);
        Assert.Equal(counter, BattleEquipment.Physical(restored)!.Critical.CounterChanceDenominator);
        var bare = BattleEquipment.Equip(actor, BattleEquipment.UnarmedChoice);
        Assert.Equal((8, 16, 8), ((int)BattleEquipment.Physical(bare)!.Critical.ChanceDenominator,
            BattleEquipment.Physical(bare)!.Critical.DoubleChanceDenominator, BattleEquipment.Physical(bare)!.Critical.CounterChanceDenominator));
        // These three source weapons are not breakable: restore must reject a fabricated broken word.
        Assert.Equal("equipment-broken", Assert.Throws<BattleRuleException>(() => BattleEquipment.ValidateLoadout(actor.Definition,
            new([(ushort)(128 + 256 + item), 127, 127, 127], []))).Code);
        var forbidden = actor.Definition.WithEquipment(new(20, [Weapon(item) with { ClassMask = 1 }]));
        Assert.Equal("equipment-class", Assert.Throws<BattleRuleException>(() => BattleEquipment.ValidateLoadout(forbidden, actor.SourceLoadout)).Code);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void BothDirectionsUseLiveAttackerDoubleAndDefenderCounter(bool reverse)
    {
        var action = Prepare(Battle(reverse ? (byte)68 : (byte)108, reverse ? (byte)108 : (byte)68), reverse);
        Assert.Equal((ushort?)8, action.ConstructionEffects.First(e => e.Kind == "rng-double").RandomRange);
        Assert.Equal((ushort?)4, action.ConstructionEffects.First(e => e.Kind == "rng-counter").RandomRange);
        Assert.DoesNotContain(action.ConstructionEffects, e => e.Kind == "cutoff");
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void EquippedDoubleAndCounterChangeActualFollowupsAtTheSameSeed(bool reverse)
    {
        var ordinaryDouble = Prepare(Battle(57, 57, seed: 0x009F1234), reverse);
        var extraDouble = Prepare(Battle(reverse ? (byte)57 : (byte)108, reverse ? (byte)108 : (byte)57, seed: 0x009F1234), reverse);
        Assert.DoesNotContain(ordinaryDouble.Reactions, r => r.Action == "physical-second");
        Assert.Contains(extraDouble.Reactions, r => r.Action == "physical-second");
        var ordinaryCounter = Prepare(Battle(57, 57, seed: 0x009D1234), reverse);
        var extraCounter = Prepare(Battle(reverse ? (byte)68 : (byte)57, reverse ? (byte)57 : (byte)68, seed: 0x009D1234), reverse);
        Assert.DoesNotContain(ordinaryCounter.Reactions, r => r.Action == "physical-counter");
        Assert.Contains(extraCounter.Reactions, r => r.Action == "physical-counter");
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void CutoffKeepsVarianceAndKillPipelineInBothAttackDirections(bool reverse)
    {
        var battle = Battle(reverse ? (byte)57 : (byte)109, reverse ? (byte)109 : (byte)57, seed: 0x01841234);
        var action = Prepare(battle, reverse);
        var hit = Assert.Single(action.Reactions);
        Assert.NotNull(hit.CutoffDamage);
        Assert.False(hit.Critical);
        Assert.Equal(0, hit.HpAfter);
        Assert.Equal(0x8000, hit.Amount);
        Assert.Equal(new[] { "rng-dodge", "rng-critical", "rng-spread-1", "rng-spread-2" },
            action.ConstructionEffects.Where(e => e.Kind.StartsWith("rng-") && e.Target is not null).Select(e => e.Kind));
        var attacker = battle.Actors[reverse ? 2 : 0]; var target = battle.Actors[reverse ? 0 : 2];
        var result = Sf2PhysicalAction.Resolve(battle, attacker.Actor, attacker.Position!, target.Actor);
        Assert.Equal(0, result.Battle.GetActor(target.Actor).Hp);
        Assert.Null(result.Battle.GetActor(target.Actor).Position);
        Assert.Equal((ushort?)(reverse ? 0 : 1), result.Battle.GetActor(attacker.Actor).Kills);
        if (!reverse) { Assert.NotNull(action.Reward); Assert.True(result.Battle.Gold > battle.Gold); }
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void GisarmeCanCutOffDuringTheOppositeCounterDirection(bool allyCounters)
    {
        var battle = Battle(allyCounters ? (byte)109 : (byte)57, allyCounters ? (byte)57 : (byte)109, seed: 0x00A31234);
        var action = Prepare(battle, allyCounters);
        var hit = Assert.Single(action.Reactions, r => r.CutoffDamage is not null);
        Assert.Equal("physical-counter", hit.Action);
        Assert.Equal(battle.Actors[allyCounters ? 0 : 2].Actor, hit.Actor);
        Assert.Equal(0, hit.HpAfter);
        Assert.Equal(10, hit.CutoffDamage); // Half counter damage, then both variance draws.
        Assert.DoesNotContain(action.ConstructionEffects, e => e.Kind == "critical");
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void CutoffUsesTheExistingDropCustodyAndVictoryOrDefeatBoundary(bool enemyAttacks)
    {
        var battle = EnemyItemDropTests.Battle(86);
        battle = battle.With(actors: battle.Actors.Select(a => !a.IsAlly && a.Actor != new ActorRef("enemy") ? a.With(hp: 0) : a));
        var attacker = Equipped(battle.GetActor(new(enemyAttacks ? "enemy" : "leader")), 109);
        battle = battle.With(actors: battle.Actors.Select(a => a.Actor == attacker.Actor ? attacker : a), mainSeed: 0x01841234);
        var target = battle.GetActor(new(enemyAttacks ? "leader" : "enemy"));
        var action = PhysicalBattleAction.PrepareSourceDefault(battle, attacker.Actor, attacker.Position!, target.Actor);
        Assert.NotNull(action.Reactions[0].CutoffDamage);
        if (!enemyAttacks)
        {
            Assert.NotNull(action.Reactions[0].Receipt);
            Assert.Contains((ushort)86, action.Prepared.GetActor(attacker.Actor).SourceLoadout!.Items);
            Assert.Contains((byte)24, action.Prepared.ItemCustody!.OnceFlags);
        }
        var result = Sf2PhysicalAction.Resolve(battle, attacker.Actor, attacker.Position!, target.Actor);
        Assert.Equal(enemyAttacks ? BattleOutcomeKind.Defeat : BattleOutcomeKind.Victory, BattleOutcomeRules.Check(result.Battle));
        Assert.Equal(enemyAttacks ? battle.Gold : action.Prepared.Gold, result.Battle.Gold);
    }

    [Fact]
    public void CutoffExperienceUsesPreCutoffDamageInsteadOfTheLethalSentinel()
    {
        var battle = Battle(109, seed: 0x01841234);
        var actor = Equipped(battle.Actors[0], 109, level: 6);
        var action = Prepare(battle.With(actors: battle.Actors.Select(a => a.Actor == actor.Actor ? actor : a)));
        Assert.NotNull(action.Reactions[0].CutoffDamage);
        // Level difference5 gives20 kill EXP. The ordinary damage contribution rounds to0;
        // using32768 would incorrectly saturate at49 before the selected halved-EXP option.
        Assert.Equal(10, action.Reward!.Amount);
    }

    [Fact]
    public void SettingCriticalSixDoesNotByItselfGrantGisarmeCutoff()
    {
        var battle = Battle(57, seed: 0x01841234);
        var actor = battle.Actors[0];
        var definition = actor.Definition.WithEquipment(new(20, [Weapon(57) with { CriticalSetting = 6 }]));
        actor = new(actor.Deployment with { Definition = definition }, actor.Hp, actor.Mp, actor.Exp,
            actor.Position, actor.Kills, actor.Defeats, sourceLoadout: actor.SourceLoadout);
        var action = Prepare(battle.With(actors: battle.Actors.Select(a => a.Actor == actor.Actor ? actor : a)));
        Assert.True(action.Reactions[0].Critical);
        Assert.Null(action.Reactions[0].CutoffDamage);
        Assert.Equal(4, BattleEquipment.Physical(actor)!.Critical.ChanceDenominator);
    }

    [Fact]
    public void ImmunityRetainsOrdinaryCriticalWhileMissNeverRollsCritical()
    {
        var immune = Prepare(Battle(109, resistance: 0xC000, seed: 0x01841234));
        Assert.Null(immune.Reactions[0].CutoffDamage);
        Assert.True(immune.Reactions[0].Critical);
        Assert.True(immune.Reactions[0].HpAfter > 0);
        var miss = Prepare(Battle(109, seed: 0x00001234));
        Assert.Equal(BattleReactionKind.Dodge, miss.Reactions[0].Kind);
        Assert.Null(miss.Reactions[0].CutoffDamage);
        Assert.DoesNotContain(miss.ConstructionEffects, e => e.Kind == "cutoff");
    }
}
