using System.Text.Json;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed partial class FieldServiceTests
{
    private static readonly ActorRef First = FieldInventoryTests.Giver, Second = FieldInventoryTests.Recipient;
    private static readonly ChurchDefinition Terms = new(10, 200, 200, 10, 20);
    private static GameSession Session(uint? gold = 100, ushort hp = 0, ushort status = 0,
        ushort secondHp = 100, ushort secondStatus = 0, byte level = 2, bool promoted = false, ushort maxHp = 250,
        ChurchDefinition? terms = null, byte? currentLevel = null, bool failReturn = false, bool promotion = false, bool armed = false, bool power = false,
        BattleClassRule sourceRule = BattleClassRule.UnpromotedSwordsman, byte warriorAxe = 33, ushort[]? curseItems = null,
        bool bindSpecial = false, bool selfHolder = false, bool missingScenes = false, int sorcererFirstLearn = 24)
    {
        var original = FieldInventoryTests.Session().Current.Exploration!.Party;
        var source = FieldInventoryTests.Session().Definition.Encounters.Values.Single();
        (byte baseClass, byte destination, string baseCode, string promotedCode, string title, byte move, string mover,
            byte weapon, ushort? promotionItem) profile = sourceRule switch
        {
            BattleClassRule.UnpromotedKnight => (1, 13, "KNTE", "PLDN", "Paladin", 7, "CENTAUR", 50, 118),
            BattleClassRule.UnpromotedWarrior => (2, 15, "WARR", "GLDT", "Gladiator", 5, "REGULAR", warriorAxe, 119),
            BattleClassRule.UnpromotedMage => (3, 17, "MAGE", "WIZ", "Wizard", 5, "MAGE", 85, 121),
            BattleClassRule.UnpromotedPriest => (4, 19, "PRST", "VICR", "Vicar", 5, "HEALER",
                (byte)(currentLevel >= 40 ? 86 : 89), bindSpecial ? (ushort)122 : null),
            BattleClassRule.UnpromotedArcher => (5, 21, "ACHR", "SNIP", "Sniper", 6, "ARCHER", 47, 120),
            BattleClassRule.UnpromotedBirdman => (6, 23, "BDMN", "BDBT", "BirdBattler", 6, "FLYING", 58, null),
            BattleClassRule.UnpromotedWolfman => (7, 24, "WFMN", "WFBR", "WolfBaron", 5, "STEALTH", 127, null),
            BattleClassRule.UnpromotedRanger => (8, 25, "RNGR", "BWNT", "BowKnight", 6, "CENTAUR_ARCHER", 47, null),
            BattleClassRule.UnpromotedPhoenix => (9, 26, "PHNK", "PHNX", "Phoenix", 7, "FLYING", 127, null),
            BattleClassRule.UnpromotedThief => (10, 27, "THIF", "NINJ", "Ninja", 7, "STEALTH", 103, null),
            BattleClassRule.UnpromotedTortoise => (11, 28, "TORT", "MNST", "Monster", 7, "HOVERING", 127, null),
            _ => (0, 12, "SDMN", "HERO", "Hero", 6, "REGULAR", 50, null),
        };
        var (baseClass, destination, baseCode, promotedCode, title, move, mover, weapon, promotionItem) = profile;
        bool sorcerer = sourceRule == BattleClassRule.UnpromotedMage;
        bool monk = sourceRule == BattleClassRule.UnpromotedPriest;
        bool baron = sourceRule == BattleClassRule.UnpromotedWarrior;
        bool gunner = sourceRule == BattleClassRule.UnpromotedArcher;
        byte specialClass = sorcerer ? (byte)18 : monk ? (byte)20 : gunner ? (byte)22 : baron ? (byte)16 : (byte)14;
        ushort material = promotionItem ?? 0;
        ushort duplicate = (ushort)(material | BattleInventory.Broken);
        byte[] book = sourceRule switch
        {
            BattleClassRule.UnpromotedMage => currentLevel >= 29 ? [139, 71, 6, 72] : [75, 76, 63, 63],
            BattleClassRule.UnpromotedPriest => currentLevel >= 40 ? [192, 130, 206, 68] : [128, 142, 3, 63],
            BattleClassRule.UnpromotedSwordsman => currentLevel >= 22 ? [10, 13, 63, 63] : [10, 63, 63, 63],
            _ => [63, 63, 63, 63],
        };
        string SpellName(int id) => id switch
        {
            0 => "heal", 1 => "aura", 2 => "detox", 3 => "boost", 4 => "slow", 6 => "dispel",
            7 => "muddle", 8 => "desoul", 10 => "egress", 11 => "blaze", 12 => "freeze", 14 => "blast", _ => "bolt",
        };
        var spells = book.Where(spell => spell != 63)
            .SelectMany(spell => Enumerable.Range(1, 1 + (spell >> 6))
                .Select(rank => new SpellRef(SpellName(spell & 63), (byte)rank))).ToArray();
        bool ranged = sourceRule is BattleClassRule.UnpromotedArcher or BattleClassRule.UnpromotedRanger;
        byte retainedRange = (byte)(ranged ? 2 : 1);
        bool innate = weapon == 127;
        bool phoenix = sourceRule == BattleClassRule.UnpromotedPhoenix;
        byte attackBonus = weapon switch { 36 or 47 => 17, 58 => 8, 127 => 0, _ => 5 };
        bool ninja = sourceRule == BattleClassRule.UnpromotedThief;
        bool monster = sourceRule == BattleClassRule.UnpromotedTortoise;
        var loadout = new BattleSourceLoadout([(ushort)(weapon | (armed ? 128 : 0)),
            power ? (ushort)147 : sourceRule == BattleClassRule.UnpromotedMage ? (ushort)24 : (ushort)127,
            ranged ? (ushort)50 : ninja ? (ushort)58 : (ushort)127, 127], book);
        if (selfHolder) loadout = new([material, power ? (ushort)147 : (ushort)0, duplicate, loadout.Items[0]], book);
        var initialLoadout = weapon == 50 ? null : new BattleSourceLoadout([weapon, 127, 127, 127], book);
        var deployments = source.Deployments.Select(row => row.Actor != First ? row : row with
        {
            Definition = new(row.Actor, promotion ? sourceRule : row.Definition.ClassRule, level, maxHp, row.Definition.MaxMp,
                row.Definition.Attack, row.Definition.Defense, row.Definition.Agility, false,
                innate ? (byte)(phoenix ? 6 : 5) : row.Definition.Move, row.Definition.Spells,
                innate ? new(PhysicalCriticalRule.OneIn16WithQuarterBonus, promoted, false, 0) :
                    row.Definition.Physical! with { Promoted = promoted },
                mover: innate ? (phoenix ? BattleMover.Hovering : BattleMover.Stealth) : row.Definition.Mover,
                sourceLoadout: initialLoadout ?? row.Definition.SourceLoadout),
        });
        if (promotion)
        {
            var growth = new BattleGrowthDefinition(baseClass, 0, Enumerable.Repeat(new StatGrowth(20, 20, []), 5).ToArray(),
                [new(1, 10), new(22, 13)], new Dictionary<byte, SpellRef> { [10] = new("egress", 1), [13] = new("bolt", 1) });
            List<BattleWeaponDefinition> weapons = innate ? [] :
                [new(weapon, "Weapon", (1u << baseClass) | (ninja ? 0 : 1u << destination) | (bindSpecial && !monk ? 1u << specialClass : 0), attackBonus, retainedRange, retainedRange)];
            if (bindSpecial && baron) weapons.Add(new(58, "Middle Sword", 1u << specialClass, 8, 1, 1));
            if (ninja) weapons.Add(new(58, "Middle Sword", 1u << destination, 8, 1, 1));
            if (ranged) weapons.Add(new(50, "Great Shot", (1u << 21) | (1u << 22) | (1u << 25), 29, 2, 3));
            var equipment = new BattleEquipmentDefinition(20, weapons,
                rings: [new(19, "Power Ring", uint.MaxValue, RingStat.Attack, 5),
                    new(24, "Black Ring", 1u << 17, RingStat.Attack, 0)]);
            if (sourceRule != BattleClassRule.UnpromotedSwordsman)
            {
                var fixture = source.WithDeployments(deployments);
                List<SourceClassDefinition> classRows = [new(destination, promotedCode, title, move, phoenix ? "FIRE_MAJOR|STATUS_MAJOR" :
                        monster ? "NEUTRAL_MAJOR|UNDEFINED1_MAJOR|UNDEFINED2_MAJOR|STATUS_MAJOR" : "NONE", mover,
                        ninja ? "CRITICAL150_1IN8|DOUBLE_1IN16|COUNTER_1IN8" :
                        sourceRule == BattleClassRule.UnpromotedWolfman ? "CRITICAL125_1IN16|DOUBLE_1IN32|COUNTER_1IN8" :
                            "CRITICAL125_1IN16|DOUBLE_1IN32|COUNTER_1IN16")];
                if (bindSpecial)
                    classRows.Add(sorcerer
                        ? new(18, "SORC", "Sorcerer", 5, "NONE", "MAGE", "CRITICAL125_1IN16|DOUBLE_1IN32|COUNTER_1IN16")
                        : monk
                        ? new(20, "MMNK", "Master Monk", 5, "NONE", "REGULAR", "CRITICAL125_1IN16|DOUBLE_1IN32|COUNTER_1IN8")
                        : gunner
                        ? new(22, "BRGN", "Brass Gunner", 5, "NONE", "BRASS_GUNNER", "CRITICAL125_1IN16|DOUBLE_1IN32|COUNTER_1IN16")
                        : baron
                        ? new(16, "BRN", "Baron", 6, "NONE", "REGULAR", "CRITICAL150_1IN8|DOUBLE_1IN32|COUNTER_1IN16")
                        : new(14, "PGNT", "Pegasus Knight", 7, "NONE", "FLYING", "CRITICAL125_1IN16|DOUBLE_1IN32|COUNTER_1IN16"));
                var classDefinitions = new PrivateBattleDefinitions(null!, [], classRows,
                    innate ? [] : [new(weapon, "WEAPON", "Weapon", "Weapon", baseCode + "|" + promotedCode,
                        [baseCode, promotedCode], retainedRange, retainedRange, 0, "WEAPON", "NOTHING", "NOTHING", 0,
                        [new("INCREASE_ATT", attackBonus)])], [], [], []);
                var stats = Enumerable.Repeat(new { start = 20, projected = 20, curve = System.Array.Empty<int>() }, 5);
                var schedule = book.Where(packed => packed != 63)
                    .Select(packed => new { packed, level = 22, spell = SpellName(packed & 63) });
                if (sourceRule == BattleClassRule.UnpromotedPriest && currentLevel < 40)
                    schedule = schedule.Append(new { packed = (byte)67, level = 30, spell = "boost" })
                        .Append(new { packed = (byte)1, level = 33, spell = "aura" });
                var ninjaSchedule = new (int Level, byte Packed, string Spell)[]
                {
                    (24, 27, "katon"), (29, 91, "katon"), (34, 28, "raijin"),
                    (39, 92, "raijin"), (45, 155, "katon"), (52, 156, "raijin"),
                }.Select(row => new { packed = row.Packed, level = row.Level, spell = row.Spell });
                var rows = JsonSerializer.SerializeToElement((bindSpecial ? new byte[] { baseClass, destination, specialClass } : [baseClass, destination]).Select(classId => new
                {
                    actor = First.Value, classId, stats,
                    spells = sorcerer && classId == 18
                        ? new[] { new { packed = (byte)29, level = 21, spell = "dao" }, new { packed = (byte)30, level = sorcererFirstLearn, spell = "apollo" } }
                        : ninja && classId == destination ? ninjaSchedule : schedule,
                }));
                deployments = BattleGrowthReader.Bind(rows, new ScenarioDefinition("promotion-test", [fixture], classDefinitions))
                    .Single().Deployments.Select(d => d.Actor != First ? d : d with
                    {
                        Definition = d.Definition.WithEquipment(equipment),
                        PromotionVariants = d.PromotionVariants.ToDictionary(p => p.Key, p => p.Value.WithEquipment(equipment)),
                    });
            }
            else
            {
                deployments = deployments.Select(d => d.Actor != First ? d : d with
                {
                    Definition = d.Definition.WithGrowth(growth).WithEquipment(equipment),
                    PromotionVariants = new Dictionary<byte, BattleActorDefinition> { [destination] = new(d.Actor, BattleClassRule.Hero, 1, 42, 14, 28, 25, 24, false, 6, [],
                        d.Definition.Physical! with { Promoted = true, Critical = PhysicalCriticalRule.Hero },
                        sourceLoadout: loadout, growth: growth with { ClassId = destination, RetainedSpells = growth.Spells }, equipment: equipment) },
                });
            }
        }
        PrivateBattleDefinitions? curseTerms = null;
        if (curseItems is not null)
        {
            var ring = new SourceItemDefinition(24, "BLACK_RING", "Black Ring", "Black Ring", "WIZ|SORC",
                ["WIZ", "SORC"], 0, 0, 5000, "RING|RARE|CURSED|BREAKABLE", "BLAZE|LV2", "BLAZE", 2,
                [new("INCREASE_ATT", 10), new("NONE", 0), new("NONE", 0)]);
            var rod = new SourceItemDefinition(101, "DEMON_ROD", "Demon Rod", "Demon Rod", "WIZ|SORC|VICR",
                ["WIZ", "SORC", "VICR"], 1, 1, 12500, "WEAPON|RARE|CURSED|BREAKABLE", "SPOIT", "SPOIT", 1,
                [new("INCREASE_ATT", 50), new("DECREASE_AGI", 10), new("NONE", 0)]);
            curseTerms = new(null!, [], [], [ring, rod], [], [], []);
            deployments = deployments.Select(row => row.Actor != First ? row : row with
            {
                Definition = new(row.Actor, BattleClassRule.Wizard, 2, maxHp, row.Definition.MaxMp, 20, 10, 20, false,
                    5, [], physical: row.Definition.Physical! with { Promoted = true },
                    equipment: new(20, [new(101, "Demon Rod", 1u << 17, 50, 1, 1, DamageOnUse: true,
                        Stats: new([new(RingStat.Attack, 50), new(RingStat.Agility, -10)]), Cursed: true)],
                        rings: [new(24, "Black Ring", 1u << 17, RingStat.Attack, 10, DamageOnUse: true, Cursed: true)])),
            });
        }
        var battle = new BattleDefinition(source.Encounter, source.Map, source.Width, source.Height,
            source.Terrain, deployments, source.Spells.Values);
        EntityRef priest = new("priest");
        var map = new ExplorationMapDefinition(new("village"), new(new ushort[WorkingMapLayout.WordCount]),
            new([new(0, 0, 63, 63)]), [new(priest, new(2, 1), 3, 32)],
            [new(ExplorationEventKind.Interact, null, null, priest, new("caller", 0))]);
        var world = new ExplorationDefinition([map],
            [new("caller", [new CallProgram(new("service", 0)), new WriteFlag(999, true),
                failReturn ? new UnsupportedInstruction("unadmitted", "authored-caller") : new EndProgram()]),
             new("service", [new OpenChurch(terms ?? Terms), new ReturnProgram()])], []);
        var definition = new ScenarioDefinition("service-behavior", [battle],
            missingScenes ? new PrivateBattleDefinitions(null!, [], [], [], [], [], []) : curseTerms, exploration: world);
        var party = new BattleStartInput(battle.Encounter, original.Actors.Select(actor => actor.Actor == First
            ? actor with { Hp = hp, Status = status,
                SourceLoadout = curseItems is null ? actor.SourceLoadout : new(curseItems, [63, 63, 63, 63]),
                Progress = currentLevel is { } attained
                ? new(attained, 76, ninja || monster ? (byte)0 : (byte)10, 20, 4, 60, [], actor.SourceLoadout) : null }
            : actor.Actor == Second ? actor with { Hp = secondHp, Status = secondStatus } : actor),
            original.MainSeed, original.ThinkingSeed, gold, original.NewBattle, original.ActiveAllies, original.ItemCustody);
        BattleActorStartInput PromotionItemHolder(BattleActorStartInput actor, ushort item)
        {
            if (!bindSpecial) return actor with { SourceLoadout = new([item, 127, 127, 127], [63, 63, 63, 63]) };
            var pack = new BattleSourceLoadout([item, 0, duplicate, 189], [63, 63, 63, 63]);
            return actor with { SourceLoadout = pack, Progress = new BattleActorProgress(1, 200, 0, 15, 4, 30, [], pack)
                { BaseMove = 6, CurrentStats = new(12, 42, 8) } };
        }
        if (promotion)
            party = new(party.Encounter, party.Actors.Select(a => a.Actor == First ? a with { SourceLoadout = loadout,
                Mp = ninja || monster ? (byte)0 : a.Mp,
                Progress = a.Progress! with { ClassId = baseClass, SourceLoadout = loadout, Spells = spells, BaseMove = 8 } }
                : a.Actor == Second && promotionItem is { } item ? PromotionItemHolder(a, item) : a),
                party.MainSeed, party.ThinkingSeed, party.Gold, party.NewBattle, [Second], party.ItemCustody);
        var session = Assert.IsType<SessionStarted>(GameSession.Start(definition,
            new ExplorationStartInput(map.Map, new("player"), new(2, 2), 1, 32, [], party))).Session;
        Accept(session, new Interact(priest));
        Assert.IsType<ChurchWait>(session.Current.Story.Wait);
        return session;
    }
}
