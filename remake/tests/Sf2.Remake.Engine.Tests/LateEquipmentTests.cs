using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class LateEquipmentTests
{
    [Theory]
    [InlineData(BattleClassRule.Robot, false)]
    [InlineData(BattleClassRule.Golem, false)]
    [InlineData(BattleClassRule.RedBaron, true)]
    public void LateClassesUseTheirOwnMaskAndAllClassRings(BattleClassRule kind, bool weaponAllowed)
    {
        var gear = new BattleEquipmentDefinition(20, [new(39, "axe", 1u << 31, 15, 1, 1),
            new(102, "ineligible", 0, 20, 1, 1)], rings: [new(19, "ring", uint.MaxValue, RingStat.Attack, 5)]);
        var definition = new BattleActorDefinition(new("holder"), kind, 1, 30, 0, 20, 10, 10, false, 5, [],
            physical: new(PhysicalCriticalRule.BrassGunner, true, false, 0, 1, 1),
            sourceLoadout: new([39, 19, 102, 127], []), equipment: gear);
        var deployment = new BattleDeploymentDefinition(definition, BattleFaction.Ally, 0, BattleControl.Player, null, new(1, 1));
        var actor = new BattleActorState(deployment, 30, 0, 0, new(1, 1), 0, 0, sourceLoadout: definition.SourceLoadout);
        var choices = BattleEquipment.Choices(actor);
        Assert.Equal(weaponAllowed, choices.Any(choice => choice.Slot == 0));
        Assert.DoesNotContain(choices, choice => choice.Slot == 2);
        var ring = BattleEquipment.Equip(actor, 1);
        Assert.Equal(25, ring.Attack);
        Assert.Null(BattleEquipment.EquippedWeaponItem(ring));
        if (weaponAllowed)
        {
            var armed = BattleEquipment.Equip(ring, 0);
            Assert.Equal(40, armed.Attack);
            Assert.Equal((byte?)39, BattleEquipment.EquippedWeaponItem(armed));
            var bare = BattleEquipment.Equip(armed, BattleEquipment.UnarmedChoice);
            Assert.Equal(25, bare.Attack);
            Assert.Null(BattleEquipment.EquippedWeaponItem(bare));
        }
        else Assert.Throws<BattleRuleException>(() => BattleEquipment.Equip(ring, 0));
    }

    [Theory]
    [InlineData((byte)29, 24, 17, 9)]
    [InlineData((byte)30, 29, 12, 4)]
    [InlineData((byte)31, 26, 9, 8)]
    public void EnemyTiesRankLateHoldersUsingSourceClass(byte role, int regular, int flying, int mage)
    {
        foreach (var (table, rank) in new[] { (PhysicalPriorityTable.Regular, regular),
            (PhysicalPriorityTable.Archer, regular), (PhysicalPriorityTable.Flying, flying), (PhysicalPriorityTable.Mage, mage) })
        {
            Assert.Equal(rank, PhysicalTargetRules.ClassRank(role, table));
            var selection = PhysicalTargetRules.Select([new(1, 16, 28), new(1, 16, role)], table)!.Value;
            Assert.Equal(rank < PhysicalTargetRules.ClassRank(28, table) ? 1 : 0, selection.Index);
        }
    }

    [Fact]
    public void PromotedStartingHolderRetainsSourceEffectiveSpellThreshold()
    {
        var growth = new BattleGrowthDefinition(19, 0, [], [new(30, 128)], new Dictionary<byte, SpellRef>());
        Assert.False(growth.CanKnow(128, 9));
        Assert.True(growth.CanKnow(128, 10));
        var original = PhysicalCriticalRule.Cleric;
        var changed = OpeningPartyInitialization.InitialCritical(growth, 13, original);
        Assert.Equal(original.ChanceDenominator, changed.ChanceDenominator);
        Assert.Equal(16, changed.DoubleChanceDenominator);
        Assert.Equal(16, changed.CounterChanceDenominator);
    }
}
