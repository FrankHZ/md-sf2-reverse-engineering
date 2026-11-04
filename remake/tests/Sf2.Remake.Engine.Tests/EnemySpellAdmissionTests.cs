using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.TestSupport;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class EnemySpellAdmissionTests
{
    [PrivateInputFact("SF2_PRIVATE_BATTLE01_DATA", "SF2_PRIVATE_BATTLE01_SCENE", "SF2_PRIVATE_BATTLE01_TERRAIN",
        "SF2_PRIVATE_STATIC_DATA", "SF2_PRIVATE_ENEMY_DATA", "SF2_PRIVATE_ENEMY_GOLD", "SF2_PRIVATE_CONTROLLED_START")]
    public void PinnedGalamMageSentryAndDarkClericHealerBindingsReachSharedRulesWithoutChapterImport()
    {
        var original = Assert.IsType<ScenarioReadAccepted>(PrivateBattleScenarioTests.Selected().Read()).Definition.PrivateDefinitions!.Encounter;
        int index = 0;
        // Controlled source-row composition for admission, not original Battle01 evidence.
        var placements = original.Placements.Select(p => p.Kind == EncounterEntityKind.Enemy ? p with
        {
            IdentityExpression = index++ % 2 == 0 ? "GALAM_MAGE" : "DARK_CLERIC",
            AiCommandsetExpression = index % 2 == 1 ? "SENTRY" : "HEALER3", ItemExpression = "NOTHING",
        } : p).ToArray();
        var encounter = new BattleEncounterDefinition(original.PlacementSource, original.TerrainSource, original.Battle,
            original.PlacementRange, original.Area, original.Scene, placements, original.Regions, original.AiPoints,
            original.CompressedRange, original.CompressedSha256, original.TerrainSha256, original.Terrain,
            original.TerrainValueCounts.ToDictionary(p => p.Key, p => p.Value));
        string Input(string name) => PrivateInputFactAttribute.RequireInput(name);
        var start = ControlledBattleStartReader.Read(Input("SF2_PRIVATE_CONTROLLED_START"));
        var definitions = PrivateBattleDefinitionReader.Read(Input("SF2_PRIVATE_STATIC_DATA"), Input("SF2_PRIVATE_ENEMY_DATA"),
            Input("SF2_PRIVATE_ENEMY_GOLD"), encounter, start);
        var admitted = PrivateBattleScenarioReader.Assemble(definitions, start);
        var battle = BattleTurnFlow.Start(admitted.Definition.Encounters.Values.Single(), admitted.Start);
        var mage = battle.GetActor(new("enemy-0")); var cleric = battle.GetActor(new("enemy-1"));
        Assert.Equal((byte)8, mage.Deployment.Initialization!.AiCommandset);
        Assert.Equal((byte)2, cleric.Deployment.Initialization!.AiCommandset);
        Assert.Equal(BattleMover.Mage, mage.Definition.Mover); Assert.Equal(BattleMover.Healer, cleric.Definition.Mover);
        Assert.Equal(BattleClassRule.Ordinary, cleric.Definition.ClassRule);
        Assert.Equal((ushort)16, cleric.Definition.Physical!.Critical.CounterChanceDenominator);
        Assert.Equal((ushort)32, cleric.Definition.Physical.Critical.ChanceDenominator);
        Assert.Equal(2, cleric.Definition.Physical.Critical.DamageBonusDenominator);
        Assert.True(BattleEquipment.OrdinaryAiLoadout(mage.Definition, mage.SourceLoadout, true, true));
        Assert.True(BattleEquipment.OrdinaryAiLoadout(cleric.Definition, cleric.SourceLoadout, true, true));
        var damage = new Sf2AttackSpellAction().RequireAction(battle, mage.Actor, new(BattleActionKind.AttackSpell, new("blaze", 1))).AttackSpell!;
        var recovery = new Sf2HealingRule().RequireSpell(battle, cleric.Actor, new("heal", 1));
        Assert.Equal((byte)2, damage.MpCost); Assert.Equal((ushort)6, damage.Power);
        Assert.Equal((byte)3, recovery.MpCost); Assert.Equal(15, recovery.Power);
        Assert.Equal((byte)0, recovery.MinimumRange); Assert.Equal((byte)1, recovery.MaximumRange);
        Assert.All(new Sf2HealingRule().QueryTargets(battle, cleric.Actor), id => Assert.Equal(cleric.Faction, battle.GetActor(id).Faction));
    }
}
