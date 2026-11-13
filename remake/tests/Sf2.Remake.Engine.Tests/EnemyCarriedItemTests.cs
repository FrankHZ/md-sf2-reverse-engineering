using System.Text.Json;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.TestSupport;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class EnemyCarriedItemTests
{
    private static readonly ActorRef Ally = new("recipient"), Enemy = new("holder");

    [PrivateInputFact("SF2_PRIVATE_BATTLE01_DATA", "SF2_PRIVATE_BATTLE01_SCENE", "SF2_PRIVATE_BATTLE01_TERRAIN",
        "SF2_PRIVATE_STATIC_DATA", "SF2_PRIVATE_ENEMY_DATA", "SF2_PRIVATE_ENEMY_GOLD", "SF2_PRIVATE_CONTROLLED_START")]
    public void PinnedPlacementBindingCarriesPowerRingWithAndWithoutAWeapon()
    {
        var original = Assert.IsType<ScenarioReadAccepted>(PrivateBattleScenarioTests.Selected().Read()).Definition.PrivateDefinitions!.Encounter;
        string Input(string name) => PrivateInputFactAttribute.RequireInput(name);
        var start = ControlledBattleStartReader.Read(Input("SF2_PRIVATE_CONTROLLED_START"));
        // Controlled composition of authenticated rows, not natural Battle01/10 reach evidence.
        foreach (string code in new[] { "GIZMO", "GALAM_ARCHER" })
        {
            var placements = original.Placements.Select(p => p.Kind == EncounterEntityKind.Enemy ? p with
                { IdentityExpression = code, ItemExpression = "POWER_RING|EQUIPPED|UNUSED_ITEM_DROP" } : p);
            var encounter = new BattleEncounterDefinition(original.PlacementSource, original.TerrainSource, original.Battle,
                original.PlacementRange, original.Area, original.Scene, placements, original.Regions, original.AiPoints,
                original.CompressedRange, original.CompressedSha256, original.TerrainSha256, original.Terrain,
                original.TerrainValueCounts.ToDictionary(p => p.Key, p => p.Value));
            var definitions = PrivateBattleDefinitionReader.Read(Input("SF2_PRIVATE_STATIC_DATA"), Input("SF2_PRIVATE_ENEMY_DATA"),
                Input("SF2_PRIVATE_ENEMY_GOLD"), encounter, start);
            var admitted = PrivateBattleScenarioReader.Assemble(definitions, start);
            var battle = BattleTurnFlow.Start(admitted.Definition.Encounters.Values.Single(), admitted.Start);
            foreach (var enemy in battle.Actors.Where(a => !a.IsAlly))
            {
                Assert.Contains((ushort)19, enemy.SourceLoadout!.Items);
                Assert.DoesNotContain((ushort)147, enemy.SourceLoadout.Items);
                Assert.Contains((byte)19, enemy.Definition.Equipment!.InertItems);
                Assert.False(enemy.Definition.Equipment.AllowChanges);
                Assert.True(BattleEquipment.OrdinaryAiLoadout(enemy.Definition, enemy.SourceLoadout, true));
                Assert.Equal(code == "GALAM_ARCHER" ? 2 : 1, BattleEquipment.Physical(enemy)!.MaximumRange);
                Assert.Equal(definitions.Enemies[code].BaseAttack * 5 / 4 +
                    (code == "GALAM_ARCHER" ? BattleEquipment.Contribution(enemy.Definition, enemy.SourceLoadout) : 0), enemy.Attack);
            }
        }
    }

    [Theory]
    [InlineData("unarmed-encounter", 19, false)]
    [InlineData("armed-encounter", 19, true)]
    [InlineData("alternate-carry", 61, true)]
    public void BoundBareItemSurvivesEntryAndDropsWithoutGrantingEquipmentEffects(string encounter, byte item, bool armed)
    {
        var (definition, start) = Setup(encounter, item, armed);
        var session = Assert.IsType<SessionStarted>(GameSession.Start(new ScenarioDefinition("carry", [definition]), start)).Session;
        var battle = session.Current.Battle; var holder = battle.GetActor(Enemy);
        Assert.True(session.Current.HasBattleControl);
        Assert.Equal(item, holder.SourceLoadout!.Items[1]);
        Assert.True(BattleEquipment.OrdinaryAiLoadout(holder.Definition, holder.SourceLoadout, false));
        Assert.Equal(armed ? 18 : 13, holder.Attack); // floor(11 * 1.25), plus only the bound weapon's 5.
        Assert.Equal(armed ? 2 : 1, BattleEquipment.Physical(holder)!.MaximumRange);
        Assert.Equal("equipment-unavailable", Assert.Throws<BattleRuleException>(() => BattleEquipment.Choices(holder)).Code);
        Assert.Empty(definition.HealingItems); Assert.Empty(definition.RetreatItems); Assert.Empty(holder.Spells);

        var result = EnemyItemDropRules.Prepare(battle, Ally, Enemy, targetDies: true);
        Assert.Equal(item, result.Receipt!.Item.ItemId);
        Assert.Equal(new ushort[] { item, 127, 127, 127 }, result.Battle.GetActor(Ally).SourceLoadout!.Items);
        Assert.Equal(new ushort[] { armed ? (ushort)161 : (ushort)127, 127, 127, 127 }, result.Battle.GetActor(Enemy).SourceLoadout!.Items);
        Assert.Equal(new byte[] { 4 }, result.Battle.ItemCustody!.OnceFlags);
        Assert.Equal(battle.MainSeed, result.Battle.MainSeed); Assert.Equal(battle.ThinkingSeed, result.Battle.ThinkingSeed);
        Assert.Equal(battle.Gold, result.Battle.Gold);
        Assert.Equal(battle.GetActor(Ally).Attack, result.Battle.GetActor(Ally).Attack);
        Assert.Equal(item, holder.SourceLoadout.Items[1]); Assert.Empty(battle.ItemCustody!.OnceFlags);
        var repeated = EnemyItemDropRules.Prepare(result.Battle, Ally, Enemy, true);
        Assert.Null(repeated.Receipt); Assert.Same(result.Battle, repeated.Battle);
    }

    [Theory]
    [InlineData(19, false)] // The former global whitelist must no longer admit an unbound ring.
    [InlineData(20, false)]
    [InlineData(147, true)] // Equipped Power Ring.
    [InlineData(275, true)] // Broken Power Ring.
    public void UnboundOrFlaggedInertItemsRejectWithoutPublishingAStart(ushort word, bool bound)
    {
        var (definition, start) = Setup("rejected", 19, false, word, bound);
        var before = JsonSerializer.Serialize(start);
        Assert.IsType<SessionStartFailed>(GameSession.Start(new ScenarioDefinition("carry", [definition]), start));
        Assert.False(BattleEquipment.OrdinaryItem(definition.Deployments.Single(d => d.Actor == Enemy).Definition, word, false));
        Assert.Equal(before, JsonSerializer.Serialize(start));
    }

    private static (BattleDefinition Definition, BattleStartInput Start) Setup(string encounter, byte item, bool armed,
        ushort? held = null, bool bound = true)
    {
        var ally = new BattleActorDefinition(Ally, BattleClassRule.Ordinary, 1, 30, 0, 10, 5, 100, false, 5, [],
            physical: new(PhysicalCriticalRule.OneIn32WithHalfBonus, false, true, 0),
            sourceLoadout: new([127, 127, 127, 127], [63, 63, 63, 63]));
        var enemy = new BattleActorDefinition(Enemy, BattleClassRule.Ordinary, 1, 20, 0, 11, 5, 1, false, 5, [],
            physical: new(PhysicalCriticalRule.OneIn32WithHalfBonus, false, false, 0),
            sourceLoadout: new([armed ? (ushort)161 : (ushort)127, held ?? item, 127, 127], [63, 63, 63, 63]),
            equipment: new(11, armed ? [new(33, "Authored spear", 0, 5, 1, 2)] : [], false, bound ? [item] : []));
        byte dropBattle = armed ? (byte)18 : (byte)10;
        var definition = new BattleDefinition(encounter, new("test-map"), 12, 12,
            Enumerable.Repeat(new BattleTerrain(TerrainSurface.Open, TerrainProtection.None), 2304),
            [new(ally, BattleFaction.Ally, 0, BattleControl.Player, null, new(2, 2), new(null, 0, 15, 15, 0)),
             new(enemy, BattleFaction.Enemy, 128, BattleControl.Automatic, new BattleStrategyRef("source-orders"),
                 new(8, 8), new(0x3000, 0, 15, 15, 0, 6, 255, 255))], [],
            initialization: new([], BattleRegionProgram.None),
            itemDrops: new(dropBattle, new Dictionary<ActorRef, byte> { [Enemy] = 128 }, [new(dropBattle, 128, item, 4)], [new(item, "Carried item", true)]));
        var start = new BattleStartInput(encounter, [new(Ally, 30, 0, 0, 0, 0, 0, null), new(Enemy, 20, 0, null, null, null, 0, null)],
            1234, 5678, 91, new("authored-entry", "engine-test", "controlled", 0, true, true, true, false, false, 0),
            itemCustody: new([], new Dictionary<byte, byte>()));
        return (definition, start);
    }
}
