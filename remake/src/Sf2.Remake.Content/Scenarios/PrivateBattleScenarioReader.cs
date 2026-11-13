using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

public sealed class PrivateBattleScenarioReader(string placementPath, string scenePath, string terrainPath,
    string staticPath, string enemyPath, string goldPath, string controlledStartPath, string? battleSceneContentPath = null) : IScenarioSource
{
    public ScenarioReadResult Read() => Read([]);

    internal ScenarioReadResult Read(IReadOnlyList<byte> fieldItems)
    {
        try
        {
            foreach (string path in new[] { placementPath, scenePath, terrainPath, staticPath, enemyPath, goldPath, controlledStartPath })
                Require(!string.IsNullOrWhiteSpace(path) && Path.IsPathFullyQualified(path), "private-input-selection", "private-inputs");
            var result = new PrivateBattleEncounterReader(placementPath, scenePath, terrainPath).Read();
            if (result is BattleEncounterReadRejected failed)
                return new ScenarioReadRejected(new(SessionFailureKind.ContentError, "private-encounter", failed.Diagnostic.Field, failed.Diagnostic.Message));
            var encounter = ((BattleEncounterReadAccepted)result).Definition;
            var start = ControlledBattleStartReader.Read(controlledStartPath);
            var definitions = PrivateBattleDefinitionReader.Read(staticPath, enemyPath, goldPath, encounter, start, fieldItems: fieldItems);
            var accepted = Assemble(definitions, start);
            if (battleSceneContentPath is null) return accepted;
            var scenes = BattleSceneContentReader.Read(battleSceneContentPath, definitions);
            return new ScenarioReadAccepted(new(accepted.Definition.Package, accepted.Definition.Encounters.Values,
                definitions, battleScenes: scenes), accepted.Start);
        }
        catch (AdmissionIssue issue) { return new ScenarioReadRejected(issue.Failure); }
        catch (PrivateBattleEncounterReader.EncounterReadException issue)
        { return new ScenarioReadRejected(new(SessionFailureKind.ContentError, "private-definition-input", issue.Diagnostic.Field, issue.Diagnostic.Message)); }
        catch (BattleRuleException issue)
        { return new ScenarioReadRejected(new(issue.Unsupported ? SessionFailureKind.UnsupportedCapability : SessionFailureKind.ContentError,
            issue.Code, issue.Field, issue.Code.Replace('-', ' '))); }
        catch (JsonException) { return Rejected("invalid-json", "controlled-start"); }
        catch (InvalidOperationException) { return Rejected("invalid-definition", "definitions"); }
        catch (KeyNotFoundException) { return Rejected("missing-definition-field", "definitions"); }
        catch (FormatException) { return Rejected("invalid-definition-field", "definitions"); }
    }

    internal static ScenarioReadAccepted Assemble(PrivateBattleDefinitions definitions, PrivateBattleStart start, bool validateStart = true)
    {
        var encounter = definitions.Encounter;
        Require(start.Battle == encounter.Battle.Id, "controlled-encounter", "start.battle");
        // The pinned table_BattleRegionCutscenes has entries only for To Moun and Odd Eye.
        Require(encounter.Battle.Id is not (32 or 40), "region-program-table", "encounter.regionProgram", true);
        Require(encounter.AllyCount <= 30 && encounter.EnemyCount <= 32, "roster-capacity", "encounter.placements");
        var deployments = new List<BattleDeploymentDefinition>(); var actorInputs = new List<BattleActorStartInput>();
        var party = start.Allies.ToDictionary(row => row.Id); var usedAllies = new HashSet<byte>();
        int enemyOrder = 128;
        foreach (var row in encounter.Placements)
        {
            bool ally = row.Kind == EncounterEntityKind.Ally;
            byte spawnMode = row.Behavior.SpawnExpression switch
            {
                "STARTING" => 0,
                "HIDDEN" when !ally => 2,
                _ => throw Unsupported("spawn-expression", "placements.spawn"),
            };
            Require(row.ItemExpression is "NOTHING" or "MEDICAL_HERB|USABLE_BY_AI|UNUSED_ITEM_DROP" or "POWER_RING|EQUIPPED|UNUSED_ITEM_DROP" ||
                !ally && row.ItemExpression == "POWER_WATER",
                "placement-item-refresh", "placements.item", true);
            byte commandset = row.AiCommandsetExpression switch
            {
                "HEALER1" => 0, "HEALER3" => 2, "ATTACKER1" => 6, "ATTACKER2" => 7,
                "SENTRY" => 8, "ATTACKER3" => 9, "LEADER" => 14, "SWARM" => 15,
                _ => throw Unsupported("source-commandset", "placements.aiCommandset"),
            };
            ActorRef actor; BattleActorDefinition definition; BattleActorStartInput actorInput; int order;
            ushort? enemyAi = null;
            if (ally)
            {
                Require(byte.TryParse(row.IdentityExpression, NumberStyles.None, CultureInfo.InvariantCulture, out byte id) && id <= 29,
                    "ally-identity-expression", "placements.identity", true);
                // Source ally rows are formation capacity, not a roster/recruit command.
                if (!party.TryGetValue(id, out var input)) continue;
                Require(usedAllies.Add(id), "duplicate-party-actor", "placements.identity");
                order = id; actor = new("ally-" + id.ToString(CultureInfo.InvariantCulture));
                var classDefinition = definitions.Classes[input!.ClassId];
                var classRule = classDefinition.Id switch
                {
                    0 => BattleClassRule.UnpromotedSwordsman, 1 => BattleClassRule.UnpromotedKnight,
                    2 => BattleClassRule.UnpromotedWarrior, 4 => BattleClassRule.UnpromotedPriest,
                    3 => BattleClassRule.UnpromotedMage, 5 => BattleClassRule.UnpromotedArcher,
                    6 => BattleClassRule.UnpromotedBirdman, 7 => BattleClassRule.UnpromotedWolfman, 8 => BattleClassRule.UnpromotedRanger, 9 => BattleClassRule.UnpromotedPhoenix,
                    10 => BattleClassRule.UnpromotedThief, 11 => BattleClassRule.UnpromotedTortoise,
                    _ => throw Unsupported("source-class", "actors.class"),
                };
                Require(input.Hp <= input.MaxHp && input.Mp <= input.MaxMp, "numeric-range", "allies.resources");
                foreach (ushort word in input.Items)
                {
                    var item = definitions.Items[(byte)(word & 127)];
                    if ((word & 128) != 0) Require(item.EquipFlags.Contains(classDefinition.Code), "incompatible-equipment", "allies.items", true);
                }
                var spells = input.Spells.Where(value => (value & 63) != 63).SelectMany(packed =>
                    definitions.Spells.Values.Where(spell => spell.BaseId == (packed & 63) && spell.Level <= (packed >> 6) + 1)
                        .OrderBy(spell => spell.Level).Select(spell => spell.Spell)).ToArray();
                definition = new(actor, classRule, input.Level, input.MaxHp, input.MaxMp, input.Attack, input.Defense,
                    (byte)(input.Agility & 127), (input.Agility & 128) != 0, input.Move, spells,
                    physical: PrivateBattleActionBindings.Physical(definitions, classDefinition.ProwessExpression, input.Items,
                        leader: id == 0, gold: 0),
                    mover: Mover(classDefinition.MovementType), sourceLoadout: new(input.Items, input.Spells),
                    resistance: PrivateAttackSpellBindings.Resistance(classDefinition.ResistanceExpression));
                actorInput = new(actor, input.Hp, input.Mp, input.Exp, input.Kills, input.Defeats, input.Status, null, ActivationWord: input.ActivationWord);
            }
            else
            {
                order = enemyOrder++; actor = new("enemy-" + (order - 128).ToString(CultureInfo.InvariantCulture));
                var enemy = definitions.Enemies[row.IdentityExpression];
                Require(enemy.InitialStatusExpression == "NONE" && enemy.SpellPower == "REGULAR",
                    "enemy-derived-state", "enemy.initialStatus/spellPower", true);
                var match = Regex.Match(enemy.AiBitfieldExpression, "^PRIORITYMOD_([0-9]+)(\\|BIT4)?(\\|BIT5)?$", RegexOptions.CultureInvariant);
                Require(match.Success && byte.TryParse(match.Groups[1].Value, out byte _) && int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) <= 15,
                    "source-ai-word", "enemy.aiBitfield", true);
                enemyAi = (ushort)((int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) << 12) |
                    (match.Groups[2].Success ? 16 : 0) | (match.Groups[3].Success ? 32 : 0));
                var items = enemy.Items.Select(item => (ushort)(definitions.Items.Values.Single(value => value.Code == item.Item).Id | (item.Equipped ? 128 : 0))).ToArray();
                if (row.ItemExpression != "NOTHING")
                {
                    // InitializeEnemyItems -> AddItem masks with $807F. Source $6000 stores0.
                    int empty = System.Array.FindIndex(items, word => (word & 127) == 127);
                    if (empty >= 0)
                    {
                        string code = row.ItemExpression.Split('|')[0];
                        items[empty] = definitions.Items.Values.Single(value => value.Code == code).Id;
                    }
                }
                var spells = enemy.Spells.Select(spell => spell.Spell == "NOTHING" ? (byte)63 :
                    (byte)(definitions.Spells.Values.Single(value => value.Code == spell.Spell && value.Level == spell.Level).BaseId | ((spell.Level - 1) << 6))).ToArray();
                definition = new(actor, BattleClassRule.Ordinary, enemy.Level, enemy.MaxHp, enemy.MaxMp, enemy.BaseAttack,
                    enemy.BaseDefense, (byte)(enemy.BaseAgility & 127), (enemy.BaseAgility & 128) != 0, enemy.BaseMovement,
                    enemy.Spells.Where(spell => spell.Spell != "NOTHING").SelectMany(spell => Enumerable.Range(1, spell.Level)
                        .Select(level => new SpellRef(spell.Spell.ToLowerInvariant(), (byte)level))),
                    physical: PrivateBattleActionBindings.Physical(definitions, enemy.ProwessExpression, items,
                        leader: false, gold: definitions.EnemyGold[enemy.Id].Gold,
                        unarmedMaximum: enemy.Code switch { "KRAKEN_ARM" => 2, "KRAKEN_HEAD" => 3, _ => 1 },
                        cannotCounter: enemy.Code == "KRAKEN_HEAD"),
                    mover: Mover(enemy.MovementType), sourceLoadout: new(items, spells),
                    equipment: PrivateBattleActionBindings.EnemyEquipment(definitions, enemy.BaseAttack, items),
                    resistance: PrivateAttackSpellBindings.Resistance(enemy.ResistanceExpression));
                actorInput = new(actor, enemy.MaxHp, enemy.MaxMp, null, null, null, 0, null);
            }
            var initialization = new BattleDeploymentInitialization(enemyAi, spawnMode, row.Behavior.PrimaryRegion,
                row.Behavior.SecondaryRegion, row.Behavior.Filler, commandset,
                Order(row.Behavior.PrimaryOrderExpression), Order(row.Behavior.SecondaryOrderExpression),
                AllyPartyMember: ally ? order : null,
                SwarmThreshold: commandset == 15 && encounter.Battle.Id == 16 ? KrakenThresholds[order - 128] : null,
                KrakenPursuit: !ally && row.IdentityExpression is ("KRAKEN_HEAD" or "KRAKEN_ARM" or "KRAKEN_LEG"));
            deployments.Add(new(definition, ally ? BattleFaction.Ally : BattleFaction.Enemy, order,
                ally ? BattleControl.Player : BattleControl.Automatic, ally ? order == 7 ? new BattleStrategyRef("source-ally-physical") : null : new BattleStrategyRef("source-orders"),
                new(row.Position.X, row.Position.Y), initialization, SourceAllyControl: ally && order == 7));
            actorInputs.Add(actorInput);
        }
        Require(usedAllies.SetEquals(party.Keys), "unused-party-actor", "allies");
        var terrain = encounter.Terrain.Select(value => value switch
        {
            8 => new BattleTerrain(TerrainSurface.Water, TerrainProtection.None),
            0 => new BattleTerrain(TerrainSurface.Impassable, TerrainProtection.None),
            1 => new(TerrainSurface.Open, TerrainProtection.Light), 2 => new(TerrainSurface.Open, TerrainProtection.None),
            3 => new(TerrainSurface.Brush, TerrainProtection.Heavy), 4 => new(TerrainSurface.Deep, TerrainProtection.Heavy),
            5 => new(TerrainSurface.Rough, TerrainProtection.Heavy), 6 => new(TerrainSurface.Desert, TerrainProtection.Heavy),
            7 => new(TerrainSurface.HighSky, TerrainProtection.None),
            255 => new(TerrainSurface.Barrier, TerrainProtection.None),
            _ => throw Unsupported("source-terrain", "terrain"),
        }).ToArray();
        var regions = encounter.Regions.Select(region => new BattleActivationRegion(region.Id,
            region.Vertices.Select(point => new MapPosition(point.X, point.Y))));
        var healing = definitions.Spells.Values.Where(spell => spell.Code == "HEAL" && spell.Power != 255 && spell.Radius == 0 &&
            spell.PropertiesExpression == "TYPE_HEAL|TARGET_TEAMMATES|AFFECTEDBYSILENCE")
            .Select(spell => new HealingSpellDefinition(spell.Spell, spell.MpCost, spell.Power, spell.MinimumRange, spell.MaximumRange))
            .Concat(PrivateBattleActionBindings.PoisonCure(definitions));
        string encounterId = "battle-" + encounter.Battle.Id.ToString(CultureInfo.InvariantCulture);
        var battle = new BattleDefinition(encounterId, new("map" + encounter.Area.MapId.ToString(CultureInfo.InvariantCulture)),
            encounter.Area.Width, encounter.Area.Height, terrain, deployments, healing, new BattleRewardDefinition(encounter.Scene.HalfExperience),
            initialization: new(regions, BattleRegionProgram.None,
                encounter.Placements.Where(row => row.Kind == EncounterEntityKind.Ally).Select(row => new MapPosition(row.Position.X, row.Position.Y)),
                encounter.AiPoints.Select(point => new MapPosition(point.X, point.Y)),
                deployments.ToDictionary(row => row.ProcessingOrder, row => row.Actor), encounter.EnemyCount),
            healingItems: PrivateBattleActionBindings.HealingItems(definitions),
            sourceMapRegion: new(new("map-" + encounter.Area.MapId.ToString(CultureInfo.InvariantCulture)),
                new(encounter.Area.X, encounter.Area.Y)),
            retreatSpells: encounter.Battle.Id == 2 ? PrivateBattleActionBindings.RetreatSpells(definitions) : [],
            retreatItems: encounter.Battle.Id == 2 ? PrivateBattleActionBindings.RetreatItems(definitions) : [],
            leaderCleanup: encounter.Scene.EnemyLeaderPresent ? new(new("ally-0"),
                deployments.Where(d => d.Faction == BattleFaction.Enemy).ToDictionary(d => d.Actor, d => (byte)d.ProcessingOrder)) : null,
            itemDrops: encounter.Battle.Id == 10 ? new(10,
                deployments.Where(d => d.Faction == BattleFaction.Enemy).ToDictionary(d => d.Actor, d => (byte)d.ProcessingOrder),
                [new(10, 129, 19, 4)], [new(19, definitions.Items[19].DisplayName, true)]) : null,
            poison: encounter.Battle.Id >= 9, attackSpells: PrivateAttackSpellBindings.Spells(definitions), supportSpells: PrivateSupportSpellBindings.Spells(definitions));
        var inputStart = new BattleStartInput(encounterId, actorInputs, start.MainSeed, start.ThinkingSeed, start.Gold, start.Policy,
            deathCredit: battle.Poison ? new BattleDeathCredit(null) : null);
        // Product content is assembled before its ordinary before-battle program has run.
        // The actual battle entry still validates the routed policy through BattleTurnFlow.
        if (validateStart) BattleTurnFlow.ValidateStart(battle, inputStart);
        return new(new("private-encounter", [battle], definitions), inputStart);
    }

    // table_SwarmAiEnemyCounts0 is indexed by source enemy slot.
    private static ReadOnlySpan<byte> KrakenThresholds => [5, 4, 2, 2, 3, 0, 0, 4, 1, 3, 0];

    // Preserve unresolved source expressions in PrivateDefinitions; null never means NONE.
    private static byte? Order(string expression)
    {
        if (expression == "NONE") return 255;
        if (byte.TryParse(expression, NumberStyles.None, CultureInfo.InvariantCulture, out byte value)) return value;
        var parts = expression.Split('|');
        if (parts.Length != 2 || !byte.TryParse(parts[1], out byte index)) return null;
        return parts[0] switch
        {
            "MOVE_TO" when index < 16 => (byte)(64 | index),
            "FOLLOW_ENEMY" when index < 32 => (byte)(128 | index),
            "FOLLOW_TARGET" when index < 30 => index,
            _ => null,
        };
    }

    private static BattleMover Mover(string source) => source switch
    {
        "REGULAR" => BattleMover.Regular, "HEALER" => BattleMover.Healer,
        "CENTAUR" => BattleMover.Centaur, "HOVERING" => BattleMover.Hovering, "STEALTH" => BattleMover.Stealth,
        "CENTAUR_ARCHER" => BattleMover.CentaurArcher, "STEALTH_ARCHER" => BattleMover.StealthArcher,
        "FLYING" => BattleMover.Flying, "ARCHER" => BattleMover.Archer, "MAGE" => BattleMover.Mage, "AQUATIC" => BattleMover.Aquatic,
        "BRASS_GUNNER" => BattleMover.BrassGunner,
        _ => throw Unsupported("source-mover", "actor.movementType"),
    };
    private static AdmissionIssue Unsupported(string code, string field) =>
        new(new(SessionFailureKind.UnsupportedCapability, code, field, code.Replace('-', ' ')));
}
