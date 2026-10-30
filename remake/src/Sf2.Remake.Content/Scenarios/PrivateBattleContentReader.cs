using System.Text.Json;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

internal sealed record PrivateAllyStart(byte Id, byte ClassId, byte Level, ushort MaxHp, ushort Hp,
    byte MaxMp, byte Mp, byte Attack, byte Defense, byte Agility, byte Move, ushort Status,
    IReadOnlyList<ushort> Items, IReadOnlyList<byte> Spells, byte? Exp, ushort? Kills, ushort? Defeats, ushort? ActivationWord = null);
internal sealed record PrivateBattleStart(byte Battle, uint MainSeed, uint ThinkingSeed, uint? Gold,
    NewBattleStartPolicy Policy, IReadOnlyList<PrivateAllyStart> Allies);

// Admits source capabilities and encounter baselines, independently of controlled progression.
internal sealed class PrivateBattleContentReader(string placementPath, string scenePath, string terrainPath,
    string staticPath, string enemyPath, string goldPath, uint mainSeed, uint thinkingSeed, string? scenesPath)
{
    private IReadOnlyList<SourceAllyDefinition> _allies = [];
    private IReadOnlyList<PrivateAllyStart> _baselines = [];
    private IReadOnlyList<byte> _growthSpells = [];

    internal void SelectOpening(JsonElement growth)
    {
        _allies = PrivateBattleDefinitionReader.OpeningAllies(staticPath, growth);
        _growthSpells = growth.EnumerateArray().SelectMany(row => BattleGrowthReader.ReadDefinition(row, 0).Spells)
            .Select(spell => spell.PackedSpell).Distinct().ToArray();
        _baselines = _allies.Select(ally =>
        {
            var row = growth.EnumerateArray().Single(row => Text(row, "actor") == "ally-" + ally.Id);
            var definition = BattleGrowthReader.ReadDefinition(row, 0);
            var stats = definition.Stats.Select(stat => stat.Start).ToArray();
            Require(stats[0] > 0, "initial-hp-required", "world.growth.stats");
            // The immutable baseline contains source starts, never seed-dependent attained stats.
            // Spell identities required at initialization are selected from the admitted growth list.
            byte[] spells = definition.Spells.Where(spell => spell.Level <= 1).Select(spell => spell.PackedSpell).ToArray();
            Require(spells.Length <= 4, "initial-spell-slots", "world.growth.spells");
            return new PrivateAllyStart(ally.Id, ally.ClassId, 1, stats[0], stats[0], stats[1], stats[1], stats[2],
                stats[3], stats[4], 1, 0, ally.Items, System.Array.AsReadOnly(spells.Concat(Enumerable.Repeat((byte)63, 4 - spells.Length)).ToArray()),
                0, 0, 0, 0);
        }).ToArray();
    }

    internal ScenarioReadAccepted Read() => Read(placementPath, scenePath, terrainPath, staticPath, enemyPath, goldPath, scenesPath);

    internal ScenarioReadAccepted Read(string placement, string scene, string terrain, string core, string enemies, string gold, string? scenes)
    {
        foreach (string path in new[] { placement, scene, terrain, core, enemies, gold })
            Require(!string.IsNullOrWhiteSpace(path) && Path.IsPathFullyQualified(path), "private-input-selection", "private-inputs");
        var result = new PrivateBattleEncounterReader(placement, scene, terrain).Read();
        if (result is BattleEncounterReadRejected failed)
            throw new AdmissionIssue(new(Application.Runtime.SessionFailureKind.ContentError, "private-encounter", failed.Diagnostic.Field, failed.Diagnostic.Message));
        var encounter = ((BattleEncounterReadAccepted)result).Definition;
        var members = encounter.Placements.Where(row => row.Kind == EncounterEntityKind.Ally)
            .Select(row => byte.Parse(row.IdentityExpression, System.Globalization.CultureInfo.InvariantCulture)).ToHashSet();
        var selected = _baselines.Where(ally => members.Contains(ally.Id)).ToArray();
        var policy = new NewBattleStartPolicy("opening-new-game", "source-initialization", "", 0, false, true, true, false, false, null)
            { Origin = BattleStartOrigin.NewGame };
        var start = new PrivateBattleStart(encounter.Battle.Id, mainSeed, thinkingSeed, 0, policy, selected);
        var definitions = PrivateBattleDefinitionReader.Read(core, enemies, gold, encounter, start, _allies, _growthSpells);
        // Movement is immutable class content; attack gets the admitted equipment contribution once.
        var baselines = selected.Select(ally => ally with
        {
            Move = definitions.Classes[ally.ClassId].Movement,
            Attack = (byte)Math.Min(200, ally.Attack + ally.Items.Where(word => (word & 128) != 0)
                .SelectMany(word => definitions.Items[(byte)(word & 127)].EquipEffects)
                .Where(effect => effect.Type == "INCREASE_ATT").Sum(effect => effect.Parameter)),
        }).ToArray();
        var accepted = PrivateBattleScenarioReader.Assemble(definitions, start with { Allies = baselines }, validateStart: false);
        if (scenes is null) return accepted;
        return new(new(accepted.Definition.Package, accepted.Definition.Encounters.Values, definitions,
            battleScenes: BattleSceneContentReader.Read(scenes, definitions)), accepted.Start);
    }

    internal ExplorationReadAccepted Initialize(ExplorationReadAccepted admitted)
    {
        Require(admitted.Definition.Exploration!.Maps.Values.All(map => map.BasePalette is { Valid: true }),
            "initial-map-palette-required", "world.maps.basePalette");
        Require(admitted.Definition.Exploration.Maps.Values.SelectMany(map => map.Battles)
            .Where(route => route.Outcome is not null).All(route => _allies.Any(ally => ally.Id == route.Outcome!.JoinMember)),
            "initial-join-member-required", "world.growth");
        uint seed = mainSeed;
        var actors = _allies.OrderBy(ally => ally.Id).Select(ally =>
        {
            var deployment = admitted.Definition.Encounters.Values.SelectMany(encounter => encounter.Deployments)
                .First(row => row.Actor.Value == "ally-" + ally.Id);
            return OpeningPartyInitialization.Initialize(deployment, ally.StartingLevel, ref seed);
        }).ToArray();
        var old = admitted.Start;
        var party = new BattleStartInput(old.Party.Encounter, actors, seed, thinkingSeed, 0, old.Party.NewBattle,
            [new("ally-0")], new([], new Dictionary<byte, byte>()));
        var palette = admitted.Definition.Exploration.Maps[old.Map].BasePalette!;
        // Source systeminit/gameintro restore period3; current colors start at the admitted map base.
        var display = new ExplorationDisplay(3, palette, palette, FullFadeVisibility.BaseRestored);
        var definition = admitted.Definition;
        var product = new ScenarioDefinition(definition.Package, definition.Encounters.Values, definition.PrivateDefinitions,
            definition.Exploration, definition.BattleScenes, definition.EncounterStarts.Values, definition.EncounterScenes.Values,
            definition.BattleStory, party.NewBattle!.Origin);
        return new(product, new(old.Map, old.Player, old.Position, old.Facing, old.Speed, old.Flags,
            party, display: display, textSettings: old.TextSettings, stepCounter: 0, egressMap: new("map-0")));
    }

    internal static JsonElement Start() => JsonSerializer.SerializeToElement(new
    {
        map = "map-3", player = "entity-0", position = new { x = 56, y = 3 }, facing = 3, speed = 32,
        flags = new[] { 0, 32, 80 },
        textSettings = new { messageSpeed = 2, mouthControl = 0, viewSpeed = 0 },
    });
}
