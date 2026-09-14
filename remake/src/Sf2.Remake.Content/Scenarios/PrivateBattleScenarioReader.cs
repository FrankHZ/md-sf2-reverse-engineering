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
    string staticPath, string enemyPath, string goldPath, string controlledStartPath) : IScenarioSource
{
    public ScenarioReadResult Read()
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
            var definitions = PrivateBattleDefinitionReader.Read(staticPath, enemyPath, goldPath, encounter, start);
            return Assemble(definitions, start);
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

    internal static ScenarioReadAccepted Assemble(PrivateBattleDefinitions definitions, ControlledBattleStart start)
    {
        var encounter = definitions.Encounter;
        Require(start.Battle == encounter.Battle.Id, "controlled-encounter", "start.battle");
        // This selected source table has no Battle01 region program. Other programs need an owner.
        Require(encounter.Battle.Id == 1, "region-program-table", "encounter.regionProgram", true);
        Require(encounter.Area.X == 0 && encounter.Area.Y == 0, "battle-area-origin", "encounter.area", true);
        Require(encounter.AllyCount <= 30 && encounter.EnemyCount <= 32, "roster-capacity", "encounter.placements");
        var deployments = new List<BattleDeploymentDefinition>(); var actorInputs = new List<BattleActorStartInput>();
        var party = start.Allies.ToDictionary(row => row.Id); var usedAllies = new HashSet<byte>();
        int enemyOrder = 128;
        foreach (var row in encounter.Placements)
        {
            bool ally = row.Kind == EncounterEntityKind.Ally;
            Require(row.Behavior.SpawnExpression == "STARTING", "spawn-expression", "placements.spawn", true);
            Require(row.ItemExpression == "NOTHING", "placement-item-refresh", "placements.item", true);
            byte commandset = row.AiCommandsetExpression switch
            {
                "HEALER1" => 0, "ATTACKER1" => 6, "ATTACKER2" => 7,
                _ => throw Unsupported("source-commandset", "placements.aiCommandset"),
            };
            ActorRef actor; BattleActorDefinition definition; BattleActorStartInput actorInput; int order;
            ushort? enemyAi = null;
            if (ally)
            {
                Require(byte.TryParse(row.IdentityExpression, NumberStyles.None, CultureInfo.InvariantCulture, out byte id) && id <= 29,
                    "ally-identity-expression", "placements.identity", true);
                Require(party.TryGetValue(id, out var input) && usedAllies.Add(id), "missing-or-duplicate-party-actor", "placements.identity");
                order = id; actor = new("ally-" + id.ToString(CultureInfo.InvariantCulture));
                var classDefinition = definitions.Classes[input!.ClassId];
                var classRule = classDefinition.Id switch
                {
                    0 => BattleClassRule.UnpromotedSwordsman, 1 => BattleClassRule.UnpromotedKnight,
                    4 => BattleClassRule.UnpromotedPriest, _ => throw Unsupported("source-class", "actors.class"),
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
                    mover: Mover(classDefinition.MovementType), sourceLoadout: new(input.Items, input.Spells));
                actorInput = new(actor, input.Hp, input.Mp, input.Exp, input.Kills, input.Defeats, input.Status, null);
            }
            else
            {
                order = enemyOrder++; actor = new("enemy-" + (order - 128).ToString(CultureInfo.InvariantCulture));
                var enemy = definitions.Enemies[row.IdentityExpression];
                Require(enemy.InitialStatusExpression == "NONE" && enemy.SpellPower == "REGULAR",
                    "enemy-derived-state", "enemy.initialStatus/spellPower", true);
                var match = Regex.Match(enemy.AiBitfieldExpression, "^PRIORITYMOD_([0-9]+)$", RegexOptions.CultureInvariant);
                Require(match.Success && byte.TryParse(match.Groups[1].Value, out byte _) && int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) <= 15,
                    "source-ai-word", "enemy.aiBitfield", true);
                enemyAi = (ushort)(int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) << 12);
                var items = enemy.Items.Select(item => (ushort)(definitions.Items.Values.Single(value => value.Code == item.Item).Id | (item.Equipped ? 128 : 0))).ToArray();
                var spells = enemy.Spells.Select(spell => spell.Spell == "NOTHING" ? (byte)63 :
                    (byte)(definitions.Spells.Values.Single(value => value.Code == spell.Spell && value.Level == spell.Level).BaseId | ((spell.Level - 1) << 6))).ToArray();
                definition = new(actor, BattleClassRule.Ordinary, enemy.Level, enemy.MaxHp, enemy.MaxMp, enemy.BaseAttack,
                    enemy.BaseDefense, (byte)(enemy.BaseAgility & 127), (enemy.BaseAgility & 128) != 0, enemy.BaseMovement,
                    enemy.Spells.Where(spell => spell.Spell != "NOTHING").Select(spell => new SpellRef(spell.Spell.ToLowerInvariant(), spell.Level)),
                    physical: PrivateBattleActionBindings.Physical(definitions, enemy.ProwessExpression, items,
                        leader: false, gold: definitions.EnemyGold[enemy.Id].Gold),
                    mover: Mover(enemy.MovementType), sourceLoadout: new(items, spells));
                actorInput = new(actor, enemy.MaxHp, enemy.MaxMp, null, null, null, 0, null);
            }
            var initialization = new BattleDeploymentInitialization(enemyAi, 0, row.Behavior.PrimaryRegion,
                row.Behavior.SecondaryRegion, row.Behavior.Filler, commandset,
                Order(row.Behavior.PrimaryOrderExpression), Order(row.Behavior.SecondaryOrderExpression));
            deployments.Add(new(definition, ally ? BattleFaction.Ally : BattleFaction.Enemy, order,
                ally ? BattleControl.Player : BattleControl.Automatic, ally ? null : BattleAiStrategy.SourceOrders,
                new(row.Position.X, row.Position.Y), initialization));
            actorInputs.Add(actorInput);
        }
        Require(usedAllies.SetEquals(party.Keys), "unused-party-actor", "allies");
        var terrain = encounter.Terrain.Select(value => value switch
        {
            0 or 8 => new BattleTerrain(TerrainSurface.Impassable, TerrainProtection.None),
            1 => new(TerrainSurface.Open, TerrainProtection.Light), 2 => new(TerrainSurface.Open, TerrainProtection.None),
            3 => new(TerrainSurface.Brush, TerrainProtection.Heavy), 4 => new(TerrainSurface.Deep, TerrainProtection.Heavy),
            5 or 6 => new(TerrainSurface.Rough, TerrainProtection.Heavy), 7 or 255 => new(TerrainSurface.Barrier, TerrainProtection.None),
            _ => throw Unsupported("source-terrain", "terrain"),
        }).ToArray();
        var regions = encounter.Regions.Select(region => new BattleActivationRegion(region.Id,
            region.Vertices.Select(point => new MapPosition(point.X, point.Y))));
        var healing = definitions.Spells.Values.Where(spell => spell.Code == "HEAL" && spell.Power != 255 && spell.Radius == 0 &&
            spell.PropertiesExpression == "TYPE_HEAL|TARGET_TEAMMATES|AFFECTEDBYSILENCE")
            .Select(spell => new HealingSpellDefinition(spell.Spell, spell.MpCost, spell.Power, spell.MinimumRange, spell.MaximumRange));
        string encounterId = "battle-" + encounter.Battle.Id.ToString(CultureInfo.InvariantCulture);
        var battle = new BattleDefinition(encounterId, new("map" + encounter.Area.MapId.ToString(CultureInfo.InvariantCulture)),
            encounter.Area.Width, encounter.Area.Height, terrain, deployments, healing, new BattleRewardDefinition(HalvedExperience: true),
            initialization: new(regions, BattleRegionProgram.None));
        var inputStart = new BattleStartInput(encounterId, actorInputs, start.MainSeed, start.ThinkingSeed, start.Gold, start.Policy);
        BattleTurnFlow.ValidateStart(battle, inputStart);
        return new(new("private-encounter", [battle], definitions), inputStart);
    }

    // Preserve unresolved source expressions in PrivateDefinitions; null never means NONE.
    private static byte? Order(string expression) => expression == "NONE" ? (byte)255 :
        byte.TryParse(expression, NumberStyles.None, CultureInfo.InvariantCulture, out byte value) ? value : null;

    private static BattleMover Mover(string source) => source switch
    {
        "REGULAR" => BattleMover.Regular, "HEALER" => BattleMover.Healer,
        "CENTAUR" => BattleMover.Centaur, "HOVERING" => BattleMover.Hovering,
        _ => throw Unsupported("source-mover", "actor.movementType"),
    };
    private static AdmissionIssue Unsupported(string code, string field) =>
        new(new(SessionFailureKind.UnsupportedCapability, code, field, code.Replace('-', ' ')));
}
