using System.Text.Json;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;
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
    private IReadOnlyList<byte> _growthClasses = [];
    private IReadOnlyList<byte> _growthSpells = [];
    private IReadOnlyList<byte> _fieldItems = [];

    internal void SelectOpening(JsonElement growth, IReadOnlyList<byte> fieldItems)
    {
        _fieldItems = fieldItems;
        _allies = PrivateBattleDefinitionReader.OpeningAllies(staticPath, growth);
        _growthClasses = growth.EnumerateArray().Select(row => (byte)Number(row, "classId", 0, 31)).Distinct().ToArray();
        _growthSpells = growth.EnumerateArray().SelectMany(row => BattleGrowthReader.ReadDefinition(row, 0).Spells)
            .Select(spell => spell.PackedSpell).Distinct().ToArray();
        _baselines = _allies.Select(ally =>
        {
            var row = growth.EnumerateArray().Single(row => Text(row, "actor") == "ally-" + ally.Id && Number(row, "classId", 0, 31) == ally.ClassId);
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

    // Select source identities before opening/restore builds the immutable party and battle catalogs.
    internal static IEnumerable<byte> FieldItems(JsonElement world)
    {
        foreach (var program in Array(world, "programs"))
            foreach (var instruction in Array(program, "instructions"))
                if (Text(instruction, "op") is "try-add-actor-item" or "try-add-party-item")
                    yield return (byte)Number(instruction, "item", 0, 126);
                else if (Text(instruction, "op") == "shop")
                    foreach (var value in Array(instruction, "stock"))
                    {
                        Require(value.ValueKind == JsonValueKind.Number && value.TryGetByte(out byte id) && id < 127, "shop-stock", "shop.stock");
                        yield return value.GetByte();
                    }
        foreach (var map in Array(world, "maps"))
        {
            var configurations = map.TryGetProperty("setups", out _) ? Array(map, "setups").Prepend(map) : [map];
            foreach (var setup in configurations)
                if (setup.TryGetProperty("search", out var search) && search.ValueKind != JsonValueKind.Null)
                    foreach (var placement in Array(search, "chests").Concat(Array(search, "otherItems")))
                        if (placement.GetProperty("itemWord").ValueKind != JsonValueKind.Null)
                            yield return (byte)(Number(placement, "itemWord", 0, 511) & 127);
        }
    }

    internal ScenarioReadAccepted Read(string placement, string scene, string terrain, string core, string enemies, string gold, string? scenes, IReadOnlyList<byte>? repeatEnemies = null)
    {
        foreach (string path in new[] { placement, scene, terrain, core, enemies, gold })
            Require(!string.IsNullOrWhiteSpace(path) && Path.IsPathFullyQualified(path), "private-input-selection", "private-inputs");
        var result = new PrivateBattleEncounterReader(placement, scene, terrain).Read();
        if (result is BattleEncounterReadRejected failed)
            throw new AdmissionIssue(new(Application.Runtime.SessionFailureKind.ContentError, "private-encounter", failed.Diagnostic.Field, failed.Diagnostic.Message));
        var encounter = ((BattleEncounterReadAccepted)result).Definition;
        var policy = new NewBattleStartPolicy("opening-new-game", "source-initialization", "", 0, false, true, true, false, false, null)
            { Origin = BattleStartOrigin.NewGame };
        var start = new PrivateBattleStart(encounter.Battle.Id, mainSeed, thinkingSeed, 0, policy, _baselines);
        var definitions = PrivateBattleDefinitionReader.Read(core, enemies, gold, encounter, start, _allies, _growthSpells, _fieldItems, repeatEnemies, _growthClasses);
        // Movement is immutable class content; attack gets the admitted equipment contribution once.
        var baselines = _baselines.Select(ally => ally with
        {
            Move = definitions.Classes[ally.ClassId].Movement,
            Attack = (byte)Math.Min(200, ally.Attack + ally.Items.Where(word => (word & 128) != 0)
                .SelectMany(word => definitions.Items[(byte)(word & 127)].EquipEffects)
                .Where(effect => effect.Type == "INCREASE_ATT").Sum(effect => effect.Parameter)),
        }).ToArray();
        var accepted = PrivateBattleScenarioReader.Assemble(definitions, start with { Allies = baselines },
            campaign: true, bindRepeatEnemies: repeatEnemies is { Count: > 0 });
        if (scenes is null) return accepted;
        return new(new(accepted.Definition.Package, accepted.Definition.Encounters.Values, definitions,
            battleScenes: BattleSceneContentReader.Read(scenes, definitions)), accepted.Start);
    }

    internal ExplorationReadAccepted Initialize(ExplorationReadAccepted admitted, string? controlledBoundary = null)
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
        ActorRef[] active = [new("ally-0")];
        var policy = old.Party.NewBattle!;
        if (controlledBoundary is not null)
        {
            var layout = admitted.Definition.Exploration.PartyFlags;
            Require(layout is not null, "party-layout", "world.partyFlags");
            Require(old.EntryProgram is null && old.EntityPhases.Count == 0 && old.Display is null,
                "campaign-start-map-load", "start");
            var lists = MapPartyMembership.Rebuild(old.Flags, layout!);
            active = lists.Active.Select(member => new ActorRef($"ally-{member}")).ToArray();
            Require(lists.Active.Contains(0) && active.Length <= layout!.Capacity &&
                lists.Joined.All(member => actors.Any(actor => actor.Actor == new ActorRef($"ally-{member}"))) &&
                old.Flags.Count(flag => flag >= layout.ActiveStart && flag < layout.ActiveStart + layout.MemberCount) == active.Length,
                "campaign-start-membership", "start.flags");
            policy = policy with { Origin = BattleStartOrigin.Controlled, Declaration = "campaign-field",
                EvidenceOwner = "controlled campaign field start", BridgeBoundary = controlledBoundary };
        }
        var party = new BattleStartInput(old.Party.Encounter, actors, seed, thinkingSeed, 0, policy,
            active, new([], new Dictionary<byte, byte>()));
        var palette = admitted.Definition.Exploration.Maps[old.Map].BasePalette!;
        // Source systeminit/gameintro restore period3; current colors start at the admitted map base.
        var display = new ExplorationDisplay(3, palette, palette, FullFadeVisibility.BaseRestored);
        var product = Persistence.OpeningLoadoutBindings.Bind(admitted.Definition,
            controlledBoundary is null ? old.Map : new("map-3"));
        // witchMenuAction_New sets CURRENT_MAP and EGRESS_MAP to GAMESTART_MAP after
        // InitializeGameSettings and before its initial SaveGame/MainLoop entry.
        return new(product, new(old.Map, old.Player, old.Position, old.Facing, old.Speed, old.Flags,
            party, display: display, textSettings: old.TextSettings, stepCounter: 0, egressMap: old.Map));
    }

    // Restore admission uses only immutable source baselines; it never draws initial growth.
    internal ExplorationReadAccepted Definitions(ExplorationReadAccepted admitted)
    {
        Require(admitted.Definition.Exploration!.Maps.Values.All(map => map.BasePalette is { Valid: true }),
            "initial-map-palette-required", "world.maps.basePalette");
        return new(Persistence.OpeningLoadoutBindings.Bind(admitted.Definition, admitted.Start.Map), admitted.Start);
    }

    internal static JsonElement Start() => JsonSerializer.SerializeToElement(new
    {
        map = "map-3", player = "entity-0", position = new { x = 56, y = 3 }, facing = 3, speed = 32,
        flags = new[] { 0, 32, 80 },
        textSettings = new { messageSpeed = 2, mouthControl = 0, viewSpeed = 0 },
    });
}
