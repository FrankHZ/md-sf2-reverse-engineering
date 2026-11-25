using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class MixedEquipmentTests
{
    private static BattleActorState Actor(BattleEquipmentStat[] terms, RingStat ringStat,
        byte ringBonus, byte basis, bool ringFirst = false, bool cursed = false, bool extra = false)
    {
        var original = Start("battle-equipment").Current.Battle.GetActor(new("champion"));
        var weapon = new BattleWeaponDefinition(50, "Mixed", uint.MaxValue, 0, 1, 1,
            DamageOnUse: true, Stats: new(terms), Cursed: cursed);
        var equipment = new BattleEquipmentDefinition(basis, [weapon], rings:
            [new(19, "Ring", uint.MaxValue, ringStat, ringBonus, DamageOnUse: true, Cursed: cursed)]);
        var definition = new BattleActorDefinition(original.Actor, original.Definition.ClassRule, 1,
            20, 0, basis, basis, (byte)(basis & 127), extra, basis, [], original.Definition.Physical,
            equipment: equipment);
        return new(original.Deployment with { Definition = definition }, 20, 0, 0, original.Position, 0, 0,
            sourceLoadout: new(ringFirst ? [147, 178, 127, 127] : [178, 147, 127, 127], [63, 63, 63, 63]));
    }

    [Theory]
    [InlineData(RingStat.Attack, 0, false, 10)]
    [InlineData(RingStat.Attack, 0, true, 5)]
    [InlineData(RingStat.Attack, 200, false, 200)]
    [InlineData(RingStat.Attack, 200, true, 195)]
    [InlineData(RingStat.Defense, 0, false, 10)]
    [InlineData(RingStat.Defense, 0, true, 5)]
    [InlineData(RingStat.Defense, 200, false, 200)]
    [InlineData(RingStat.Defense, 200, true, 195)]
    [InlineData(RingStat.Move, 0, false, 10)]
    [InlineData(RingStat.Move, 0, true, 5)]
    [InlineData(RingStat.Move, 200, false, 200)]
    [InlineData(RingStat.Move, 200, true, 195)]
    public void EveryOperationClampsInActualPackOrder(RingStat stat, byte basis, bool ringFirst, byte expected)
    {
        var actor = Actor([new(stat, -5)], stat, 10, basis, ringFirst);
        Assert.Equal(expected, stat switch { RingStat.Attack => actor.Attack, RingStat.Defense => actor.Defense, _ => actor.Move });
        Assert.Equal(basis, actor.BaseDefense);
        var choice = BattleEquipment.Choices(actor).Single(choice => choice.Slot == (ringFirst ? 1 : 0));
        Assert.Equal(expected, stat switch { RingStat.Attack => choice.Attack, RingStat.Defense => choice.Defense, _ => choice.Move });
    }

    [Fact]
    public void RepeatedMixedTermsClampSeparatelyAfterStatusAndSurviveReconstruction()
    {
        var actor = Actor([new(RingStat.Attack, 50), new(RingStat.Attack, -5),
            new(RingStat.Defense, -5), new(RingStat.Move, -2)], RingStat.Defense, 10, 190);
        actor = actor.With(status: 0xFC00, refreshStats: true);
        Assert.Equal(195, actor.Attack); // ATTACK -> 200 -> 200 -> 195
        Assert.Equal(134, actor.Defense); // BOOST 200, SLOW -71, weapon -5, ring +10
        Assert.Equal(188, actor.Move);
        var restored = new BattleActorState(actor.Deployment, 20, 0, 0, actor.Position, 0, 0,
            status: actor.Status, sourceLoadout: actor.SourceLoadout);
        Assert.Equal((actor.Attack, actor.Defense, actor.Move), (restored.Attack, restored.Defense, restored.Move));
        var bare = BattleEquipment.Equip(actor, BattleEquipment.UnarmedChoice);
        Assert.Equal(200, bare.Attack);
        Assert.Equal(139, bare.Defense);
        Assert.Equal(190, bare.Move);
    }

    [Theory]
    [InlineData(false, 123, false)] [InlineData(true, 123, false)]
    public void NegativeAgilityUsesPackedOperandWhilePositiveRestoresCurrentHighBit(bool ringFirst, byte agility, bool extra)
    {
        var actor = Actor([new(RingStat.Agility, -10)], RingStat.Agility, 5, 0, ringFirst, extra: true);
        Assert.Equal(agility, actor.Agility);
        Assert.Equal(extra, actor.ExtraRoundAction);
    }

    [Fact]
    public void BothCursedSlotsBlockRemovalAndTransferUntilCureClearsBothEquippedWords()
    {
        var actor = Actor([new(RingStat.Attack, 50), new(RingStat.Agility, -10)], RingStat.Attack, 15, 20, cursed: true);
        actor = actor.With(refreshStats: true);
        Assert.Equal(CurseRules.Mask, actor.Status & CurseRules.Mask);
        foreach (int choice in new[] { BattleEquipment.UnarmedChoice, BattleEquipment.RemoveRingChoice })
            Assert.Equal("equipped-curse", Assert.Throws<BattleRuleException>(() => BattleEquipment.Equip(actor, choice)).Code);
        foreach (ushort word in actor.SourceLoadout!.Items.Take(2))
            Assert.Equal("equipped-curse", Assert.Throws<BattleRuleException>(() => BattleEquipment.RequireTransferable(actor.Definition, word)).Code);
        var cured = BattleEquipment.RemoveCurse(actor.Definition, actor.SourceLoadout);
        Assert.Equal(new ushort[] { 50, 19, 127, 127 }, cured.Items);
        Assert.Equal(0, BattleEquipment.RefreshCurse(actor.Definition, cured, actor.Status));
        Assert.Equal(20, BattleEquipment.Attack(actor.Definition, null, cured));
        var onlyWeapon = new BattleSourceLoadout([434, 19, 127, 127], cured.Spells);
        Assert.Equal(CurseRules.Mask, BattleEquipment.RefreshCurse(actor.Definition, onlyWeapon, 0));
        Assert.Equal(70, BattleEquipment.Attack(actor.Definition, null, onlyWeapon));
    }
}
