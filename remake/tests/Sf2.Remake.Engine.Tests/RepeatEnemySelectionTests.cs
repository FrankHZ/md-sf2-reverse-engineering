using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class RepeatEnemySelectionTests
{
    [Theory]
    [InlineData(28, 11, 0)]
    [InlineData(9, 12, 1)]
    [InlineData(38, 0, 1)]
    [InlineData(39, 0, 2)]
    [InlineData(9, 255, 0)]
    [InlineData(255, 12, 25)]
    public void CompletedBattleUsesEffectiveLevelAndSignedClass(int level, int classId, int quotient) =>
        Assert.Equal(quotient, RepeatEnemySelection.Quotient(level, classId, 29));

    // USA table bytes at 1B6DBC..1B6DD9, including the airborne overread.
    [Theory]
    [InlineData(0, 0x1357, 12, 79, 70, 11, 0x1357)]
    [InlineData(1, 0x1357, 17, 82, 70, 12, 0x531A)]
    [InlineData(1, 0x4AB0, 16, 82, 70, 11, 0x0A3A)]
    [InlineData(2, 0x4AB0, 21, 83, 72, 18, 0x6A18)]
    [InlineData(6, 0xF111, 12, 79, 70, 11, 0x9ABD)]
    [InlineData(25, 0xF111, 12, 79, 70, 11, 0x9ABD)]
    public void EachSlotConsumesItsOwnRollsBeforeTheNext(int quotient, uint word, byte orc, byte priest,
        byte madam, byte soulsower, uint after)
    {
        uint main = (word << 16) | 0xBEEF;
        byte Draw(byte id, BattleMover mover) => RepeatEnemySelection.Select(id, mover, quotient, ref main);
        Assert.Equal(orc, Draw(12, BattleMover.Regular));
        Assert.Equal(43, Draw(43, BattleMover.Flying));
        Assert.Equal(priest, Draw(79, BattleMover.Healer));
        Assert.Equal(madam, Draw(70, BattleMover.Mage));
        Assert.Equal(60, Draw(60, BattleMover.BrassGunner));
        Assert.Equal(soulsower, Draw(11, BattleMover.Regular));
        Assert.Equal((after << 16) | 0xBEEF, main);
    }

    [Theory]
    [InlineData(false, 29, 14)]
    [InlineData(true, 34, 27)]
    public void AConfiguredEncounterUsesItsCandidateOnlyOnCompletedReentry(bool completed, byte enemyId, ushort hp)
    {
        const string prowess = "CRITICAL150_1IN32|DOUBLE_1IN32|COUNTER_1IN32";
        var source = new EncounterSource("test", "test", "test", "test");
        var range = new EncounterRange(0, 1, 1);
        var behavior = new EncounterBehavior("NONE", 15, "NONE", 15, 0, "STARTING");
        var encounter = new BattleEncounterDefinition(source, source, new(6, "TRAINING"),
            range, new(1, 0, 0, 12, 12, 255, 255), new("NONE", false, false),
            [
                new(0, EncounterEntityKind.Ally, "HERO", new(2, 2), "ATTACKER1", "NOTHING", behavior),
                new(128, EncounterEntityKind.Enemy, "SCOUT", new(5, 5), "ATTACKER1", "NOTHING", behavior),
            ],
            [], [], range, "", "", Enumerable.Repeat((byte)2, 48 * 48), new Dictionary<byte, int> { [2] = 48 * 48 });
        SourceEnemyDefinition Enemy(byte id, string code, ushort maxHp) => new(id, code, code, code, 0, [], 0, "REGULAR",
            2, maxHp, 0, 5, 4, 6, 3, "NONE", prowess, Enumerable.Repeat(new SourceEnemyItem("NOTHING", "NOTHING", false), 4).ToArray(),
            Enumerable.Repeat(new SourceEnemySpell("NOTHING", "NOTHING", 1), 4).ToArray(), "NONE", "REGULAR", "PRIORITYMOD_2");
        var definitions = new PrivateBattleDefinitions(encounter, [source], [new(0, "HERO", "Hero", 4, "NONE", "REGULAR", prowess)],
            [new(127, "NOTHING", "", "", "NONE", [], 0, 0, 0, "NONE", "NOTHING", "NOTHING", 1, [])],
            [], [Enemy(29, "SCOUT", 14), Enemy(34, "VETERAN", 27)], [new(29, 3, "", "", "", 0), new(34, 8, "", "", "", 0)]);
        var policy = new NewBattleStartPolicy("controlled", "engine behavior", "synthetic encounter", 0, true, true, true, false, false, 0);
        var assembled = PrivateBattleScenarioReader.Assemble(definitions, new(6, 0x1357BEEF, 0, 0, policy,
            [new(0, 0, 18, 20, 20, 0, 0, 10, 4, 15, 4, 0, [127, 127, 127, 127], [63, 63, 63, 63], 0, 0, 0, 0)]),
            campaign: true, bindRepeatEnemies: true);
        var route = new ExplorationBattleRoute(assembled.Start.Encounter, 210, 211, null, null, null, RepeatUpgradeLevel: 18);
        var layout = new WorkingMapLayout(new ushort[4096]);
        var map = new ExplorationMapDefinition(new("training-field"), layout, new([new(0, 0, 63, 63)]), [], [], battles: [route]);
        var definition = new ScenarioDefinition("training", assembled.Definition.Encounters.Values, definitions,
            new([map], []), encounterStarts: [assembled.Start]);
        var world = MapTransfer.Build(map, new("player"), new(3, 3), 0, 32, assembled.Start, []);
        var current = new SessionSnapshot(Guid.NewGuid(), 0, 0, new ActiveExploration(world),
            new StoryState(completed ? [210, 211] : [210]), SessionStopReason.PlayerInput);

        var selected = BattleEntry.Select(current, current.Story, route, []);
        var entered = BattleEntry.Continue(definition, selected, [], RuleCompositions.Sf2());

        var enemy = entered.Battle.GetActor(new("enemy-0"));
        Assert.Equal(enemyId, enemy.Deployment.SourceEnemy);
        Assert.Equal(hp, enemy.Hp);
        Assert.Equal(new MapPosition(5, 5), enemy.Position);
    }
}
