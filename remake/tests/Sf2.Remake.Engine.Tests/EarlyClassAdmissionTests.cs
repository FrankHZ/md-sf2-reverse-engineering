using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.TestSupport;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class EarlyClassAdmissionTests
{
    [Theory]
    [InlineData(1)] [InlineData(40)] [InlineData(99)]
    public void EarlyDefinitionsDoNotInventAPreVoyageLevelCap(int level)
    {
        var session = Start("early-ranged", d =>
        { foreach (var actor in d["actors"]!.AsArray()) actor!["level"] = level; });
        var battle = session.Current.Battle;
        Assert.Equal(new byte?[] { 1, 5, 3 }, battle.Actors.Where(a => a.IsAlly).Select(a => Sf2ClassRules.SourceClass(a.Definition.ClassRule)));
        Assert.Equal(new[] { BattleMover.Centaur, BattleMover.Archer, BattleMover.Mage }, battle.Actors.Where(a => a.IsAlly).Select(a => a.Definition.Mover));
        Assert.All(battle.Actors, a => Assert.Equal(level, a.Level));
        Assert.Equal(23, battle.GetActor(new("mage")).Attack);
        Assert.Empty(battle.GetActor(new("mage")).Spells); // Mage definition is not BLAZE implementation.
    }

    [Theory]
    [InlineData("class")] [InlineData("attack")] [InlineData("mover")] [InlineData("range")] [InlineData("broken")]
    public void InvalidEarlyConfigurationRejectsBeforeSessionPublication(string defect)
    {
        var document = Document("early-ranged");
        switch (defect)
        {
            case "class": document["actors"]![0]!["classRule"] = "unpromoted-mage"; break;
            case "attack": document["actors"]![0]!["attack"] = 32; break;
            case "mover": document["actors"]![0]!["physical"]!["movementType"] = "unsupported"; break;
            case "range": document["weapons"]![0]!["maximumRange"] = 0; break;
            case "broken": document["actors"]![0]!["items"]![0] = 456; break;
        }
        Assert.IsType<SessionStartFailed>(GameSession.Start(Reader(document)));
    }

    [PrivateInputFact("SF2_PRIVATE_BATTLE01_DATA", "SF2_PRIVATE_BATTLE01_SCENE", "SF2_PRIVATE_BATTLE01_TERRAIN",
        "SF2_PRIVATE_STATIC_DATA", "SF2_PRIVATE_ENEMY_DATA", "SF2_PRIVATE_ENEMY_GOLD", "SF2_PRIVATE_CONTROLLED_START")]
    public void PinnedEarlyClassesAndEnemyWeaponsAreAdmittedWithoutImportingTheirStoryOrSpells()
    {
        var accepted = Assert.IsType<ScenarioReadAccepted>(PrivateBattleScenarioTests.Selected().Read());
        var original = accepted.Definition.PrivateDefinitions!.Encounter;
        // Controlled composition of existing source rows; this is not original Battle01 placement evidence.
        var codes = new[] { "GALAM_KNIGHT", "GALAM_ARCHER", "GALAM_MAGE" }; int index = 0;
        var placements = original.Placements.Select(p => p.Kind == EncounterEntityKind.Enemy ? p with
            { IdentityExpression = codes[index++ % 3], AiCommandsetExpression = "ATTACKER1", ItemExpression = "NOTHING" } : p).ToArray();
        var encounter = new BattleEncounterDefinition(original.PlacementSource, original.TerrainSource, original.Battle,
            original.PlacementRange, original.Area, original.Scene, placements, original.Regions, original.AiPoints,
            original.CompressedRange, original.CompressedSha256, original.TerrainSha256, original.Terrain,
            original.TerrainValueCounts.ToDictionary(p => p.Key, p => p.Value));
        string Input(string name) => PrivateInputFactAttribute.RequireInput(name);
        var start = ControlledBattleStartReader.Read(Input("SF2_PRIVATE_CONTROLLED_START"));
        start = start with { Allies = start.Allies.Select((a, i) => i switch
        {
            1 => a with { ClassId = 3, Level = 4, Items = new ushort[] { 213, 127, 127, 127 }, Spells = new byte[] { 63, 63, 63, 63 } },
            2 => a with { ClassId = 5, Items = new ushort[] { 173, 127, 127, 127 }, Spells = new byte[] { 63, 63, 63, 63 } },
            0 => a with { ClassId = 10, Items = new ushort[] { 231, 127, 127, 127 }, Spells = new byte[] { 63, 63, 63, 63 } },
            _ => a,
        }).ToArray() };
        var definitions = PrivateBattleDefinitionReader.Read(Input("SF2_PRIVATE_STATIC_DATA"), Input("SF2_PRIVATE_ENEMY_DATA"),
            Input("SF2_PRIVATE_ENEMY_GOLD"), encounter, start);
        var admitted = PrivateBattleScenarioReader.Assemble(definitions, start);
        var battle = BattleTurnFlow.Start(admitted.Definition.Encounters.Values.Single(), admitted.Start);
        Assert.Equal(BattleClassRule.UnpromotedThief, battle.GetActor(new("ally-0")).Definition.ClassRule);
        Assert.Equal(16, battle.GetActor(new("ally-0")).Definition.Physical!.Critical.DoubleChanceDenominator);
        Assert.Equal(BattleClassRule.UnpromotedMage, battle.GetActor(new("ally-1")).Definition.ClassRule);
        Assert.Equal(BattleMover.Mage, battle.GetActor(new("ally-1")).Definition.Mover);
        Assert.Equal(BattleClassRule.UnpromotedArcher, battle.GetActor(new("ally-2")).Definition.ClassRule);
        Assert.Equal(BattleMover.Archer, battle.GetActor(new("ally-2")).Definition.Mover);
        foreach (var (id, code, minimum, maximum) in new[] { (0, "GALAM_KNIGHT", 1, 2), (1, "GALAM_ARCHER", 2, 2), (2, "GALAM_MAGE", 1, 1) })
        {
            var actor = battle.GetActor(new("enemy-" + id)); var source = definitions.Enemies[code];
            int bonus = actor.Definition.Equipment!.Weapons[(byte)(actor.SourceLoadout!.Items[0] & 127)].AttackBonus;
            Assert.Equal(source.BaseAttack + source.BaseAttack / 4 + bonus, actor.Attack);
            Assert.Equal(minimum, BattleEquipment.Physical(actor)!.MinimumRange);
            Assert.Equal(maximum, BattleEquipment.Physical(actor)!.MaximumRange);
            Assert.False(actor.Definition.Equipment.AllowChanges);
            Assert.Equal(code != "GALAM_MAGE", BattleEquipment.OrdinaryAiLoadout(actor.Definition, actor.SourceLoadout, true));
        }
        // Fresh initialization recomputes from the source base; it never compounds the weapon bonus.
        var again = BattleInitializationRules.Initialize(battle, admitted.Start);
        Assert.Equal(battle.Actors.Select(a => a.Attack), again.Actors.Select(a => a.Attack));
    }
}
