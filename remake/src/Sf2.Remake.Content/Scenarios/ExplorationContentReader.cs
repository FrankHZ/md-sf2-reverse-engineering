using System.Text.Json;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

internal static class ExplorationContentReader
{
    private static IReadOnlyList<ExplorationTextToken> ReadTextTokens(string text)
    {
        var tokens = new List<ExplorationTextToken>();
        foreach (System.Text.RegularExpressions.Match match in System.Text.RegularExpressions.Regex.Matches(text, @"\{[^{}]*\}|[{}]|[^{}]+"))
        {
            string value = match.Value;
            if (value == "{N}") tokens.Add(new(ExplorationTextTokenKind.Newline, value));
            else if (value == "{W1}") tokens.Add(new(ExplorationTextTokenKind.Wait1, value));
            else if (value == "{W2}") tokens.Add(new(ExplorationTextTokenKind.Wait2, value));
            else if (value == "{LEADER}") tokens.Add(new(ExplorationTextTokenKind.MemberName, value, 0));
            else if (value.StartsWith("{NAME;", StringComparison.Ordinal) && value.EndsWith('}') &&
                int.TryParse(value.AsSpan(6, value.Length - 7), System.Globalization.NumberStyles.None,
                    System.Globalization.CultureInfo.InvariantCulture, out int member))
                tokens.Add(new(ExplorationTextTokenKind.MemberName, value, member));
            else tokens.Add(new(value.Contains('{') || value.Contains('}') ? ExplorationTextTokenKind.Unsupported :
                ExplorationTextTokenKind.Literal, value));
        }
        return tokens.AsReadOnly();
    }

    internal static ExplorationReadAccepted ReadAuthored(JsonElement root)
    {
        Object(root, "document", "formatVersion", "package", "battle", "world", "start");
        _ = Number(root, "formatVersion", 8, 8);
        var battle = AuthoredScenarioPackageReader.DecodeBattle(root.GetProperty("battle"));
        return Read(Id(root, "package"), root.GetProperty("world"), root.GetProperty("start"), battle);
    }

    internal static ExplorationReadAccepted Read(string package, JsonElement world, JsonElement start, ScenarioReadAccepted battle, ExplorationProvenance? provenance = null)
    {
        ObjectOptional(world, "world", "partyFlags", ["maps", "programs", "texts",
            .. world.TryGetProperty("textFont", out _) ? new[] { "textFont" } : System.Array.Empty<string>(),
            .. world.TryGetProperty("growth", out _) ? new[] { "growth" } : System.Array.Empty<string>(),
            .. world.TryGetProperty("memberNames", out _) ? new[] { "memberNames" } : System.Array.Empty<string>(),
            .. world.TryGetProperty("presentation", out _) ? new[] { "presentation" } : System.Array.Empty<string>()]);
        MapPartyFlagLayout? partyFlags = null;
        if (world.TryGetProperty("partyFlags", out var membership))
        {
            Object(membership, "partyFlags", "memberCount", "joinedStart", "activeStart", "capacity");
            partyFlags = new(Number(membership, "memberCount", 1, 256), Number(membership, "joinedStart", 0, 65280),
                Number(membership, "activeStart", 0, 65280), Number(membership, "capacity", 1, 256));
            Require(partyFlags.Capacity <= partyFlags.MemberCount, "party-capacity", "partyFlags");
        }
        var texts = new Dictionary<int, string>();
        foreach (var text in Array(world, "texts"))
        {
            Object(text, "text", "id", "text");
            string value = Text(text, "text");
            Require(value.Length is > 0 and <= 8192, "text-length", "world.texts");
            Require(texts.TryAdd(Number(text, "id", 0, 65535), value), "duplicate-text", "world.texts");
        }
        var programs = new List<StoryProgram>();
        var programIds = new HashSet<string>();
        foreach (var row in Array(world, "programs"))
        {
            ObjectOptional(row, "program", "source", ["id", "instructions",
                .. row.TryGetProperty("entitiesRunning", out _) ? new[] { "entitiesRunning" } : System.Array.Empty<string>()]);
            string id = Id(row, "id");
            Require(programIds.Add(id), "duplicate-program", "program.id");
            var instructions = Array(row, "instructions").Select(ReadInstruction).ToArray();
            Require(instructions.Length is >= 1 and <= 65536, "program-length", "program.instructions");
            Require(instructions[^1] is EndProgram or ReturnProgram or JumpProgram or UnsupportedInstruction,
                "unresolved-program-fallthrough", "program.instructions");
            programs.Add(new(id, instructions, row.TryGetProperty("source", out _) ? Text(row, "source") : null,
                !row.TryGetProperty("entitiesRunning", out _) || Boolean(row, "entitiesRunning")));
        }
        var paletteBindings = new Dictionary<MapId, PalettePair>();
        if (start.TryGetProperty("mapPalettes", out _))
            foreach (var binding in Array(start, "mapPalettes"))
            {
                Object(binding, "mapPalette", "map", "base");
                Require(paletteBindings.TryAdd(new(Id(binding, "map")), ReadPair(binding.GetProperty("base"))),
                    "duplicate-map-palette", "start.mapPalettes");
            }
        var maps = new List<ExplorationMapDefinition>();
        var mapIds = new HashSet<MapId>();
        foreach (var row in Array(world, "maps"))
        {
            ObjectOptional(row, "map", "input", ["id", "layout", "areas", "entities", "events", "onLoad", "battle", "setup",
                .. row.TryGetProperty("basePalette", out _) ? new[] { "basePalette" } : System.Array.Empty<string>(),
                .. row.TryGetProperty("population", out _) ? new[] { "population" } : System.Array.Empty<string>(),
                .. row.TryGetProperty("entryFlags", out _) ? new[] { "entryFlags" } : System.Array.Empty<string>(),
                .. row.TryGetProperty("layoutEvents", out _) ? new[] { "layoutEvents" } : System.Array.Empty<string>()]);
            var id = new MapId(Id(row, "id"));
            Require(mapIds.Add(id), "duplicate-map", "world.maps");
            PalettePair? palette = null;
            if (row.TryGetProperty("basePalette", out var embedded)) palette = ReadPair(embedded);
            if (paletteBindings.TryGetValue(id, out var bound))
            {
                Require(palette is null, "duplicate-map-palette", "map.basePalette");
                palette = bound;
            }
            var rows = Array(row, "layout").ToArray();
            Require(rows.Length is >= 1 and <= 64, "layout-height", "map.layout");
            Require(rows.All(line => line.ValueKind == JsonValueKind.Array), "layout-row", "map.layout");
            int width = rows[0].GetArrayLength();
            Require(width is >= 1 and <= 64 && rows.All(line => line.GetArrayLength() == width), "layout-width", "map.layout");
            var words = Enumerable.Repeat((ushort)0xC000, WorkingMapLayout.WordCount).ToArray();
            for (int y = 0; y < rows.Length; y++)
                for (int x = 0; x < width; x++)
                {
                    var value = rows[y][x];
                    Require(value.ValueKind == JsonValueKind.Number && value.TryGetUInt16(out _), "layout-word", "map.layout");
                    words[y * 64 + x] = value.GetUInt16();
                }
            var areas = Array(row, "areas").Select(area =>
            {
                ObjectOptional(area, "area", "overlay", ["minX", "minY", "maxX", "maxY",
                    .. area.TryGetProperty("view", out _) ? new[] { "view" } : System.Array.Empty<string>()]);
                int minX = Number(area, "minX", 0, width - 1), minY = Number(area, "minY", 0, rows.Length - 1);
                return new OriginalMapTraversalArea(minX, minY, Number(area, "maxX", minX, width - 1), Number(area, "maxY", minY, rows.Length - 1));
            }).ToArray();
            Require(areas.Length > 0 && areas.Distinct().Count() == areas.Length, "map-areas", "map.areas");
            var entities = ReadEntities(row);
            var events = Array(row, "events").Select(ReadEvent).ToArray();
            var route = row.GetProperty("battle");
            ExplorationBattleRoute? encounter = null;
            if (route.ValueKind != JsonValueKind.Null)
            {
                ObjectOptional(route, "battle-route", "load", ["encounter", "unlockedFlag", "completedFlag", "introFlag", "before", "start",
                    .. route.TryGetProperty("outcome", out _) ? new[] { "outcome" } : System.Array.Empty<string>()]);
                string encounterId = Id(route, "encounter");
                Require(battle.Definition.Encounters.ContainsKey(encounterId), "missing-encounter", "map.battle");
                encounter = new(encounterId, NullableNumber(route, "unlockedFlag", 65535), NullableNumber(route, "completedFlag", 65535),
                    NullableNumber(route, "introFlag", 65535), Location(route.GetProperty("before")), Location(route.GetProperty("start")),
                    route.TryGetProperty("load", out var load) ? Location(load) : null,
                    route.TryGetProperty("outcome", out var outcome) ? ReadOutcome(outcome, id) : null);
            }
            MapSetupRoute? setupRoute = null;
            var setup = row.GetProperty("setup");
            if (setup.ValueKind != JsonValueKind.Null)
            {
                Object(setup, "map.setup", "default", "variants");
                setupRoute = new(new MapSetupId(Id(setup, "default")), Array(setup, "variants").Select(variant =>
                {
                    Object(variant, "map.setup.variant", "flag", "setup");
                    return new MapSetupFlagVariant(new FlagId(Number(variant, "flag", 0, 65535).ToString(System.Globalization.CultureInfo.InvariantCulture)),
                        new MapSetupId(Id(variant, "setup")));
                }));
            }
            ExplorationPopulation? population = null;
            if (row.TryGetProperty("population", out var populationRow))
            {
                population = ReadPopulation(populationRow);
                Require(entities.All(entity => entity.Sprite is not null), "population-sprites", "map.entities");
            }
            maps.Add(new(id, new(words), new(areas), entities, events, Location(row.GetProperty("onLoad")), encounter, row.TryGetProperty("input", out var input) ? Location(input) : null, setupRoute, population, row.TryGetProperty("layoutEvents", out var layoutEvents) ? ReadLayoutEvents(layoutEvents) : null, Array(row, "areas").Select(area =>
                {
                    if (!area.TryGetProperty("overlay", out var offset)) return new MapOverlayOffset(0, 0);
                    Object(offset, "overlay", "x", "y");
                    return new MapOverlayOffset(Number(offset, "x", -63, 63), Number(offset, "y", -63, 63));
                }), row.TryGetProperty("entryFlags", out _) ? Array(row, "entryFlags").Select(flag =>
                {
                    Object(flag, "entryFlag", "flag", "value");
                    return new WriteFlag(Number(flag, "flag", 0, 65535), Boolean(flag, "value"));
                }) : null, palette, Array(row, "areas").Where(area => area.TryGetProperty("view", out _)).Select(ReadViewArea)));
        }
        Require(paletteBindings.Keys.All(mapIds.Contains), "unknown-map-palette", "start.mapPalettes");
        Require(maps.Count > 0, "empty-world", "world.maps");
        var definition = new ExplorationDefinition(maps, programs, texts, provenance, partyFlags,
            world.TryGetProperty("presentation", out var presentation) ? ExplorationAssetReader.Read(presentation) : null,
            world.TryGetProperty("memberNames", out _) ? Array(world, "memberNames").Select(name =>
            {
                Require(name.ValueKind == JsonValueKind.String && name.GetString()!.Length is > 0 and <= 32, "member-name", "world.memberNames");
                return name.GetString()!;
            }) : null, texts.Select(pair => new KeyValuePair<int, IReadOnlyList<ExplorationTextToken>>(pair.Key, ReadTextTokens(pair.Value))),
            world.TryGetProperty("textFont", out var font) ? ReadFont(font) : null);
        Link(definition);
        Object(start, "exploration-start", ["map", "player", "position", "facing", "speed", "flags",
            .. start.TryGetProperty("textSettings", out _) ? new[] { "textSettings" } : System.Array.Empty<string>(),
            .. start.TryGetProperty("display", out _) ? new[] { "display" } : System.Array.Empty<string>(),
            .. start.TryGetProperty("mapPalettes", out _) ? new[] { "mapPalettes" } : System.Array.Empty<string>(),
            .. start.TryGetProperty("program", out _) ? new[] { "program" } : System.Array.Empty<string>(),
            .. start.TryGetProperty("entityPhases", out _) ? new[] { "entityPhases" } : System.Array.Empty<string>()]);
        var selectedMap = new MapId(Id(start, "map"));
        Require(definition.Maps.ContainsKey(selectedMap), "missing-map", "start.map");
        var entityPhases = start.TryGetProperty("entityPhases", out _) ? Array(start, "entityPhases").Select(phase =>
        {
            Object(phase, "start.entityPhases", "entity", "slot", "actionCursor", "nextWaitTicks", "waitingForMotion", "motion");
            return new ExplorationEntityStartPhase(new(Id(phase, "entity")), Number(phase, "slot", 0, 48),
                Number(phase, "actionCursor", 0, 4095), (byte)Number(phase, "nextWaitTicks", 0, 255),
                Boolean(phase, "waitingForMotion"), ReadStartMotion(phase.GetProperty("motion")));
        }).ToArray() : [];
        Require(entityPhases.Select(phase => phase.Entity).Distinct().Count() == entityPhases.Length &&
            entityPhases.Select(phase => phase.Slot).Distinct().Count() == entityPhases.Length,
            "duplicate-entity-phase", "start.entityPhases");
        var flags = Array(start, "flags").Select(flag =>
        {
            Require(flag.ValueKind == JsonValueKind.Number && flag.TryGetInt32(out int _) &&
                flag.GetInt32() is >= 0 and <= 65535, "flag-range", "start.flags");
            return flag.GetInt32();
        }).ToArray();
        Require(flags.Distinct().Count() == flags.Length, "duplicate-flag", "start.flags");
        ProgramLocation? entryProgram = start.TryGetProperty("program", out var entry) ? Location(entry) : null;
        if (entryProgram is { } programEntry)
            Require(programEntry.Instruction == 0 && definition.Programs.ContainsKey(programEntry.Program),
                "program-entry", "start.program");
        var encounters = (world.TryGetProperty("growth", out var growth)
            ? BattleGrowthReader.Bind(growth, battle.Definition) : battle.Definition.Encounters.Values).ToArray();
        foreach (var map in maps.Where(map => map.Battle?.Outcome is not null))
        {
            Require(battle.Definition.PrivateDefinitions is { Encounter.Scene.EnemyLeaderPresent: false },
                "outcome-source", "map.battle.outcome", true);
            var route = map.Battle!;
            Require(definition.Programs[route.Outcome!.DefeatedProgram.Program].Instructions.All(instruction => instruction is EndProgram or ReturnProgram),
                "defeated-program", "map.battle.outcome.defeated", true);
            int index = System.Array.FindIndex(encounters, encounter => encounter.Encounter == route.Encounter);
            var encounter = encounters[index];
            var leader = encounter.Deployments.SingleOrDefault(row => row.Faction == BattleFaction.Ally && row.Definition.Physical?.Leader == true);
            var firstEnemy = encounter.Deployments.FirstOrDefault(row => row.Faction == BattleFaction.Enemy);
            Require(leader is not null && firstEnemy is not null, "outcome-roster", "map.battle.outcome");
            encounters[index] = new(encounter.Encounter, encounter.Map, encounter.Width, encounter.Height, encounter.Terrain,
                encounter.Deployments, encounter.Spells.Values, encounter.Rewards, encounter.Initialization, new(leader!.Actor, firstEnemy!.Actor), encounter.HealingItems.Values);
        }
        return new(new(package, encounters, battle.Definition.PrivateDefinitions, definition, battle.Definition.BattleScenes),
            new(selectedMap, new(Id(start, "player")), Position(start.GetProperty("position")),
                (byte)Number(start, "facing", 0, 3), (ushort)Number(start, "speed", 1, 384), flags, battle.Start, entryProgram, entityPhases,
                start.TryGetProperty("display", out var display) ? ReadDisplay(display) : null,
                start.TryGetProperty("textSettings", out var settings) ? ReadTextSettings(settings) : null));
    }

    private static ExplorationTextFont ReadFont(JsonElement row)
    {
        Object(row, "textFont", "asciiToSymbol", "advances");
        byte[] Bytes(string key, int length, int minimum, int maximum)
        {
            var values = Array(row, key).ToArray();
            Require(values.Length == length, "text-font-length", key);
            return values.Select(value =>
            {
                Require(value.TryGetInt32(out int n) && n >= minimum && n <= maximum, "text-font-value", key);
                return (byte)value.GetInt32();
            }).ToArray();
        }
        return new(System.Array.AsReadOnly(Bytes("asciiToSymbol", 256, 1, 80)), System.Array.AsReadOnly(Bytes("advances", 80, 0, 16)));
    }

    private static ExplorationTextSettings ReadTextSettings(JsonElement row)
    {
        Object(row, "textSettings", "messageSpeed", "mouthControl", "viewSpeed");
        return new((byte)Number(row, "messageSpeed", 0, 3), (byte)Number(row, "mouthControl", 0, 255),
            (ushort)Number(row, "viewSpeed", 0, 65535));
    }

    private static ExplorationViewArea ReadViewArea(JsonElement area)
    {
        var row = area.GetProperty("view");
        Object(row, "view", "foregroundX", "foregroundY", "backgroundX", "backgroundY", "parallaxAX", "parallaxAY",
            "parallaxBX", "parallaxBY", "autoscrollAX", "autoscrollAY", "autoscrollBX", "autoscrollBY", "layer");
        return new(Number(area, "minX", 0, 63), Number(area, "minY", 0, 63), Number(area, "maxX", 0, 63), Number(area, "maxY", 0, 63),
            Number(row, "foregroundX", 0, 63), Number(row, "foregroundY", 0, 63), Number(row, "backgroundX", 0, 63), Number(row, "backgroundY", 0, 63),
            Number(row, "parallaxAX", 0, 65535), Number(row, "parallaxAY", 0, 65535), Number(row, "parallaxBX", 0, 65535), Number(row, "parallaxBY", 0, 65535),
            Number(row, "autoscrollAX", -128, 255), Number(row, "autoscrollAY", -128, 255), Number(row, "autoscrollBX", -128, 255), Number(row, "autoscrollBY", -128, 255),
            Number(row, "layer", 0, 255));
    }

    private static PalettePair ReadPair(JsonElement row)
    {
        Object(row, "palettePair", "color2", "color3");
        var pair = new PalettePair((ushort)Number(row, "color2", 0, 0xEEE), (ushort)Number(row, "color3", 0, 0xEEE));
        Require(pair.Valid, "palette-word", "palettePair");
        return pair;
    }

    private static ExplorationDisplay ReadDisplay(JsonElement row)
    {
        Object(row, "display", "period", "base", "current", "visibility");
        string visibility = Text(row, "visibility");
        Require(visibility is "black" or "base-restored", "display-visibility", "start.display");
        var display = new ExplorationDisplay((byte)Number(row, "period", 1, 255), ReadPair(row.GetProperty("base")),
            ReadPair(row.GetProperty("current")), visibility == "black" ? FullFadeVisibility.Black : FullFadeVisibility.BaseRestored);
        Require(display.Visibility == FullFadeVisibility.Black ? display.Current.Black : display.Current == display.Base,
            "display-palette-state", "start.display");
        return display;
    }

    private static EntityMotionState ReadStartMotion(JsonElement row)
    {
        Object(row, "start.entityPhases.motion", "x", "y", "xDestination", "yDestination", "xVelocity", "yVelocity",
            "xTravel", "yTravel", "xSpeed", "ySpeed", "xAcceleration", "yAcceleration", "flagsA", "flagsB",
            "facing", "layer", "animationCounter", "waitTimer");
        return new((short)Number(row, "x", short.MinValue, short.MaxValue), (short)Number(row, "y", short.MinValue, short.MaxValue),
            (short)Number(row, "xDestination", short.MinValue, short.MaxValue),
            (short)Number(row, "yDestination", short.MinValue, short.MaxValue),
            (short)Number(row, "xVelocity", short.MinValue, short.MaxValue),
            (short)Number(row, "yVelocity", short.MinValue, short.MaxValue),
            (ushort)Number(row, "xTravel", 0, ushort.MaxValue), (ushort)Number(row, "yTravel", 0, ushort.MaxValue),
            (ushort)Number(row, "xSpeed", 0, 255), (ushort)Number(row, "ySpeed", 0, 255),
            (byte)Number(row, "xAcceleration", 0, 255), (byte)Number(row, "yAcceleration", 0, 255),
            (byte)Number(row, "flagsA", 0, 255), (byte)Number(row, "flagsB", 0, 255),
            (byte)Number(row, "facing", 0, 7), (byte)Number(row, "layer", 0, 255),
            (byte)Number(row, "animationCounter", 0, 255), (byte)Number(row, "waitTimer", 0, 255));
    }

    private static ExplorationOutcomeRoute ReadOutcome(JsonElement row, MapId battleMap)
    {
        Object(row, "outcome", "after", "joinMember", "defeated", "defeat", "return", "victoryFacing", "egress");
        // Non-empty defeated programs and enemy-leader cleanup need their own supported seam.
        var egress = row.GetProperty("egress");
        Object(egress, "egress", "map", "position", "facing");
        return new(Required("after"), Number(row, "joinMember", 0, 29),
            Required("defeated"), Required("defeat"), Required("return"), battleMap,
            (byte)Number(row, "victoryFacing", 0, 3), new(Id(egress, "map")), Position(egress.GetProperty("position")), (byte)Number(egress, "facing", 0, 3));
        ProgramLocation Required(string name)
        {
            var value = Location(row.GetProperty(name));
            Require(value is not null, "outcome-program", "map.battle.outcome." + name);
            return value!.Value;
        }
    }

    private static ExplorationEntityDefinition[] ReadEntities(JsonElement row)
    {
        var entities = Array(row, "entities").Select(entity =>
        {
            ObjectOptional(entity, "entity", "sprite", ["id", "position", "facing", "speed", "visible", "obstruction",
                .. entity.TryGetProperty("actions", out _) ? new[] { "actions" } : System.Array.Empty<string>()]);
            return new ExplorationEntityDefinition(new(Id(entity, "id")), Position(entity.GetProperty("position")),
                (byte)Number(entity, "facing", 0, 7), (ushort)Number(entity, "speed", 1, 384),
                Boolean(entity, "visible"), Boolean(entity, "obstruction"), entity.TryGetProperty("sprite", out _) ? Number(entity, "sprite", 0, 255) : null,
                entity.TryGetProperty("actions", out _) ? Actions(entity) : null);
        }).ToArray();
        Require(entities.Select(entity => entity.Entity).Distinct().Count() == entities.Length, "duplicate-entity", "map.entities");
        return entities;
    }

    private static ExplorationPopulation ReadPopulation(JsonElement row)
    {
        ObjectOptional(row, "population", "allySprites", "allyCount", "nonAllyStart", "playerSprite", "followers");
        var followers = Array(row, "followers").Select(follower =>
        {
            Object(follower, "follower", "flag", "character", "sprite");
            return new ExplorationFollowerDefinition(Number(follower, "flag", 0, 65535),
                Number(follower, "character", 0, 255), Number(follower, "sprite", 0, 255));
        }).ToArray();
        return new(Number(row, "allyCount", 1, 128), Number(row, "nonAllyStart", 128, 255),
            Number(row, "playerSprite", 0, 255), System.Array.AsReadOnly(followers),
            row.TryGetProperty("allySprites", out _) ? System.Array.AsReadOnly(Array(row, "allySprites").Select(appearance =>
            {
                Object(appearance, "allySprite", "character", "sprite", "joinedFlag", "unjoinedSprite");
                int? joined = NullableNumber(appearance, "joinedFlag", 65535), unjoined = NullableNumber(appearance, "unjoinedSprite", 255);
                Require((joined is null) == (unjoined is null), "ally-sprite-override", "population.allySprites");
                return new ExplorationAllySprite(Number(appearance, "character", 0, 255), Number(appearance, "sprite", 0, 255), joined, unjoined);
            }).ToArray()) : null);
    }

    private static ExplorationLayoutEvents ReadLayoutEvents(JsonElement row)
    {
        Object(row, "layoutEvents", "doors", "flags", "roofs");
        WorkingMapBlockCopy Copy(JsonElement copy)
        {
            Object(copy, "blockCopy", "source", "destination", "width", "height");
            var source = Position(copy.GetProperty("source"));
            var destination = Position(copy.GetProperty("destination"));
            int width = Number(copy, "width", 1, 64), height = Number(copy, "height", 1, 64);
            Require(source.X + width <= 64 && source.Y + height <= 64 && destination.X + width <= 64 && destination.Y + height <= 64,
                "block-copy-bounds", "layoutEvents");
            return new(source.X, source.Y, destination.X, destination.Y, width, height);
        }
        var doors = Array(row, "doors").Select(door =>
        {
            Object(door, "door", "trigger", "copy");
            return new ExplorationDoor(Position(door.GetProperty("trigger")), Copy(door.GetProperty("copy")));
        }).ToArray();
        var flags = Array(row, "flags").Select(flag =>
        {
            Object(flag, "flagCopy", "flag", "copy");
            return new ExplorationFlagCopy(Number(flag, "flag", 0, 65535), Copy(flag.GetProperty("copy")));
        }).ToArray();
        var roofs = Array(row, "roofs").Select(roof =>
        {
            Object(roof, "roof", "trigger", "copy", "clear");
            var trigger = Position(roof.GetProperty("trigger"));
            var copy = Copy(roof.GetProperty("copy"));
            return new MapBlockCopyActionRecord(new(trigger.X, trigger.Y), Boolean(roof, "clear")
                ? MapBlockRegionMutation.Clear(copy.DestinationX, copy.DestinationY, copy.Width, copy.Height)
                : MapBlockRegionMutation.CopyFrom(copy));
        }).ToArray();
        return new(System.Array.AsReadOnly(doors), System.Array.AsReadOnly(flags), new(roofs));
    }

    private static ExplorationEvent ReadEvent(JsonElement row)
    {
        string kind = Text(row, "kind");
        if (kind == "interact")
        {
            ObjectOptional(row, "event", "entityFlags", "kind", "entity", "program", "requiredFlag", "requiredValue");
            return new(ExplorationEventKind.Interact, null, null, row.GetProperty("entity").ValueKind == JsonValueKind.Null ? null : new EntityRef(Id(row, "entity")), RequiredLocation(row.GetProperty("program")),
                RequiredFlag: NullableNumber(row, "requiredFlag", 65535), RequiredFlagValue: Boolean(row, "requiredValue"),
                EntityFlags: row.TryGetProperty("entityFlags", out _) ? (byte)Number(row, "entityFlags", 0, 255) : null);
        }
        if (kind is "step" or "warp-frontier" or "source-zone")
        {
            Object(row, "event", ["kind", "x", "y", "program", "marker", "requiredFlag", "requiredValue",
                .. kind == "source-zone" ? new[] { "actions" } : System.Array.Empty<string>()]);
            return new(kind == "source-zone" ? ExplorationEventKind.SourceZone : kind == "step" ? ExplorationEventKind.Step : ExplorationEventKind.Warp,
                NullableNumber(row, "x", 63), NullableNumber(row, "y", 63), null,
                RequiredLocation(row.GetProperty("program")), RequiredMarker: (ushort?)NullableNumber(row, "marker", 0x3C00),
                RequiredFlag: NullableNumber(row, "requiredFlag", 65535), RequiredFlagValue: Boolean(row, "requiredValue"),
                SourceInit: kind == "source-zone" ? Actions(row) : null);
        }
        Require(kind == "warp", "event-kind", "event.kind", true);
        ObjectOptional(row, "event", "loadMode", "kind", "x", "y", "map", "position", "facing", "marker", "requiredFlag", "requiredValue");
        string mode = row.TryGetProperty("loadMode", out _) ? Text(row, "loadMode") : "rebuild";
        Require(mode is "rebuild" or "preserve", "map-load-mode", "event.loadMode");
        return new(ExplorationEventKind.Warp, NullableNumber(row, "x", 63), NullableNumber(row, "y", 63), null, null,
            new MapId(Id(row, "map")), Position(row.GetProperty("position")), (byte)Number(row, "facing", 0, 3),
            (ushort?)NullableNumber(row, "marker", 0x3C00), NullableNumber(row, "requiredFlag", 65535), Boolean(row, "requiredValue"),
            LoadMode: mode == "preserve" ? MapLoadMode.Preserve : MapLoadMode.Rebuild);
    }

    private static StoryInstruction ReadInstruction(JsonElement row)
    {
        string opcode = Text(row, "op");
        switch (opcode)
        {
            case "end": Object(row, opcode, "op"); return new EndProgram();
            case "end-map-script": Object(row, opcode, "op"); return new EndProgram(true);
            case "return": Object(row, opcode, "op"); return new ReturnProgram();
            case "reset-party-battle-stats": Object(row, opcode, "op"); return new ResetPartyBattleStats();
            case "battle-return-map": Object(row, opcode, "op"); return new ReturnBattleMap();
            case "retired-map3-entity-scratch": Object(row, opcode, "op"); return new RetiredMap3EntityScratch();
            case "jump": Object(row, opcode, "op", "target"); return new JumpProgram(RequiredLocation(row.GetProperty("target")));
            case "call":
                ObjectOptional(row, opcode, "activateEntities", ["op", "target"]);
                return new CallProgram(RequiredLocation(row.GetProperty("target")),
                    row.TryGetProperty("activateEntities", out _) && Boolean(row, "activateEntities"));
            case "branch-flag":
                Object(row, opcode, "op", "flag", "whenSet", "target");
                return new BranchFlag(Number(row, "flag", 0, 65535), Boolean(row, "whenSet"), RequiredLocation(row.GetProperty("target")));
            case "set-flag": Object(row, opcode, "op", "flag", "value"); return new WriteFlag(Number(row, "flag", 0, 65535), Boolean(row, "value"));
            case "branch-coordinates":
                Object(row, opcode, "op", "entity", "x", "y", "whenEqual", "target");
                return new BranchEntityCoordinates(new(Id(row, "entity")), (short)Number(row, "x", short.MinValue, short.MaxValue),
                    (short)Number(row, "y", short.MinValue, short.MaxValue), Boolean(row, "whenEqual"), RequiredLocation(row.GetProperty("target")));
            case "text-cursor": Object(row, opcode, "op", "text"); return new SetTextCursor(Number(row, "text", 0, 65534));
            case "show-text":
                ObjectOptional(row, opcode, "speakerFlags", ["op", "mode", "speaker",
                    .. row.TryGetProperty("explicitWindows", out _) ? new[] { "explicitWindows" } : System.Array.Empty<string>(),
                    .. row.TryGetProperty("waitForAcknowledgement", out _) ? new[] { "waitForAcknowledgement" } : System.Array.Empty<string>(),
                    .. row.TryGetProperty("useEventSpeaker", out _) ? new[] { "useEventSpeaker" } : System.Array.Empty<string>()]);
                string mode = Text(row, "mode"); Require(mode is "single" or "continued", "text-mode", "program.mode");
                return new ShowText(mode == "single" ? TextDisplayMode.Single : TextDisplayMode.Continued,
                    row.GetProperty("speaker").ValueKind == JsonValueKind.Null ? null : new EntityRef(Id(row, "speaker")),
                    row.TryGetProperty("speakerFlags", out _) ? (byte)Number(row, "speakerFlags", 0, 255) : (byte)0,
                    row.TryGetProperty("useEventSpeaker", out _) && Boolean(row, "useEventSpeaker"),
                    !row.TryGetProperty("waitForAcknowledgement", out _) || Boolean(row, "waitForAcknowledgement"),
                    row.TryGetProperty("explicitWindows", out _) && Boolean(row, "explicitWindows"));
            case "open-portrait":
                Object(row, opcode, "op", "entity", "flags");
                return new OpenPortrait(row.GetProperty("entity").ValueKind == JsonValueKind.Null ? null : new EntityRef(Id(row, "entity")),
                    (byte)Number(row, "flags", 0, 255));
            case "close-portrait": Object(row, opcode, "op"); return new ClosePortrait();
            case "wait-text-input": Object(row, opcode, "op"); return new WaitForTextInput();
            case "close-text": Object(row, opcode, "op"); return new CloseText();
            case "wait-view": Object(row, opcode, "op"); return new WaitForView();
            case "yes-no": Object(row, opcode, "op", "flag"); return new ChooseYesNo(Number(row, "flag", 0, 65535));
            case "sprite":
                Object(row, opcode, "op", "entity", "sprite");
                return new SetEntitySprite(new(Text(row, "entity")), Number(row, "sprite", 30, 255));
            case "face":
                ObjectOptional(row, opcode, "refreshSprite", "op", "entity", "facing");
                return new SetEntityFacing(new(Id(row, "entity")), (byte)Number(row, "facing", 0, 7),
                    row.TryGetProperty("refreshSprite", out _) && Boolean(row, "refreshSprite"));
            case "priority": Object(row, opcode, "op", "entity", "value"); return new SetEntityPriority(new(Id(row, "entity")), Boolean(row, "value"));
            case "position":
                Object(row, opcode, "op", "entity", "position", "facing");
                return new SetEntityPosition(new(Id(row, "entity")), Position(row.GetProperty("position")), (byte)Number(row, "facing", 0, 7));
            case "hide": Object(row, opcode, "op", "entity", "removeAliases"); return new HideMapEntity(new(Id(row, "entity")), Boolean(row, "removeAliases"));
            case "speaker": Object(row, opcode, "op", "entity"); return new SetDialogueSpeaker(row.GetProperty("entity").ValueKind == JsonValueKind.Null ? null : new EntityRef(Id(row, "entity")));
            case "camera-target": Object(row, opcode, "op", "position"); return new SetCameraTarget(Position(row.GetProperty("position")));
            case "camera-entity": Object(row, opcode, "op", "entity"); return new SetCameraEntity(row.GetProperty("entity").ValueKind == JsonValueKind.Null ? null : new EntityRef(Id(row, "entity")));
            case "scene-map": Object(row, opcode, "op", "map", "camera"); return new LoadSceneMap(new(Id(row, "map")), Position(row.GetProperty("camera")));
            case "scene-entities":
                Object(row, opcode, "op", "population", "position", "facing", "entities");
                var entities = ReadEntities(row);
                Require(entities.Length <= 48 && entities.All(entity => entity.Sprite is not null), "scene-entities", "program.entities");
                return new LoadSceneEntities(ReadPopulation(row.GetProperty("population")), Position(row.GetProperty("position")),
                    (byte)Number(row, "facing", 0, 3), System.Array.AsReadOnly(entities));
            case "visibility": Object(row, opcode, "op", "entity", "visible"); return new SetEntityVisibility(new(Id(row, "entity")), Boolean(row, "visible"));
            case "motion":
                ObjectOptional(row, opcode, "installation", "op", "entity", "actions", "wait");
                var installation = EntityScriptInstallation.Preserve;
                if (row.TryGetProperty("installation", out _))
                {
                    Require(Enum.GetNames<EntityScriptInstallation>().Contains(Text(row, "installation")),
                        "entity-script-installation", "program.installation");
                    installation = Enum.Parse<EntityScriptInstallation>(Text(row, "installation"));
                }
                return new StartEntityMotion(new(Id(row, "entity")), Actions(row), Boolean(row, "wait"), installation);
            case "join-party": Object(row, opcode, "op", "member"); return new JoinPartyMember(Number(row, "member", 0, 255));
            case "follow":
                Object(row, opcode, "op", "entity", "leader", "x", "y");
                return new FollowEntity(new(Id(row, "entity")), new(Id(row, "leader")), Number(row, "x", -127, 127), Number(row, "y", -127, 127));
            case "wait-entity": Object(row, opcode, "op", "entity"); return new WaitForEntity(new(Id(row, "entity")));
            case "wait-ticks": Object(row, opcode, "op", "ticks"); return new WaitProgramTicks(Number(row, "ticks", 0, 65535));
            case "present":
                ObjectOptional(row, opcode, "fullBlack", "op", "kind", "resource", "entity", "position");
                Require(Enum.GetNames<PresentationCueKind>().Contains(Text(row, "kind")), "presentation-cue", "program.kind", true);
                FullBlackFade? fullBlack = null;
                if (row.TryGetProperty("fullBlack", out var full))
                {
                    Object(full, "fullBlack", "period");
                    Require(Text(row, "kind") is "FadeIn" or "FadeOut" && Text(row, "resource") == "black",
                        "full-black-fade-binding", "program.fullBlack", true);
                    fullBlack = new(full.GetProperty("period").ValueKind == JsonValueKind.Null ? null : (byte)Number(full, "period", 1, 255));
                }
                return new PresentCue(Enum.Parse<PresentationCueKind>(Text(row, "kind")), row.GetProperty("resource").ValueKind == JsonValueKind.Null ? null : Text(row, "resource"),
                    row.GetProperty("entity").ValueKind == JsonValueKind.Null ? null : new EntityRef(Id(row, "entity")),
                    row.GetProperty("position").ValueKind == JsonValueKind.Null ? null : Position(row.GetProperty("position")), fullBlack);
            case "transfer":
                Object(row, opcode, "op", "map", "position", "facing", "loadMode");
                string loadMode = Text(row, "loadMode"); Require(loadMode is "rebuild" or "preserve", "map-load-mode", "program.loadMode", true);
                return new TransferToMap(new(Id(row, "map")), Position(row.GetProperty("position")), (byte)Number(row, "facing", 0, 3),
                    loadMode == "rebuild" ? MapLoadMode.Rebuild : MapLoadMode.Preserve);
            case "native-call":
                Object(row, opcode, "op", "symbol", "source"); return new UnsupportedInstruction(Text(row, "symbol"), Text(row, "source"));
            default: throw new AdmissionIssue(new(Application.Runtime.SessionFailureKind.ContentError, "unknown-instruction", "program.op", "unknown instruction"));
        }
    }

    private static EntityActionProgram Actions(JsonElement row)
    {
        var actions = new List<EntityAction>();
        foreach (var action in Array(row, "actions"))
        {
            string opcode = Text(action, "op");
            switch (opcode)
            {
                case "move":
                    ObjectOptional(action, opcode, "wait", "op", "x", "y");
                    actions.Add(new MoveEntityRelative(Number(action, "x", -63, 63), Number(action, "y", -63, 63),
                        !action.TryGetProperty("wait", out _) || Boolean(action, "wait"))); break;
                case "random-walk": Object(action, opcode, "op", "x", "y", "radius"); actions.Add(new RandomWalkEntity(new(Number(action, "x", 0, 63), Number(action, "y", 0, 63)), Number(action, "radius", 0, 63))); break;
                case "destination": Object(action, opcode, "op", "position"); actions.Add(new MoveEntityAbsolute(Position(action.GetProperty("position")))); break;
                case "face": Object(action, opcode, "op", "facing"); actions.Add(new FaceEntity((byte)Number(action, "facing", 0, 7))); break;
                case "wait": Object(action, opcode, "op", "ticks"); actions.Add(new WaitEntityTicks((byte)Number(action, "ticks", 0, 255))); break;
                case "speed": Object(action, opcode, "op", "x", "y"); actions.Add(new SetEntitySpeed((ushort)Number(action, "x", 0, 384), (ushort)Number(action, "y", 0, 384))); break;
                case "acceleration": Object(action, opcode, "op", "x", "y"); actions.Add(new SetEntityAcceleration((byte)Number(action, "x", 0, 255), (byte)Number(action, "y", 0, 255))); break;
                case "flags":
                    Object(action, opcode, "op", "field", "mask", "value");
                    string field = Text(action, "field"); Require(field is "a" or "b", "entity-flag-field", "program.actions");
                    actions.Add(new ChangeEntityFlags(field == "b", (byte)Number(action, "mask", 0, 255), (byte)Number(action, "value", 0, 255))); break;
                case "sprite-size": Object(action, opcode, "op", "size"); actions.Add(new SetGlobalSpriteSize((ushort)Number(action, "size", 0, 65535))); break;
                case "refresh-sprite": Object(action, opcode, "op", "source"); actions.Add(new RefreshEntitySprite(Text(action, "source"))); break;
                case "jump": Object(action, opcode, "op", "instruction"); actions.Add(new JumpEntityAction(Number(action, "instruction", 0, 4096))); break;
                case "idle": Object(action, opcode, "op"); actions.Add(new IdleEntityAction()); break;
                case "native-call": Object(action, opcode, "op", "symbol", "source"); actions.Add(new UnsupportedEntityAction(Text(action, "symbol"), Text(action, "source"))); break;
                default: Require(false, "entity-action", "program.actions.op", true); break;
            }
        }
        Require(actions.Count is >= 1 and <= 4096, "entity-action-length", "program.actions");
        actions.Add(new StopEntityActions());
        Require(actions.OfType<JumpEntityAction>().All(jump => jump.Instruction < actions.Count), "unresolved-entity-target", "program.actions");
        return new(actions);
    }

    private static void Link(ExplorationDefinition definition)
    {
        void Target(ProgramLocation? location)
        {
            if (location is not { } target) return;
            Require(definition.Programs.TryGetValue(target.Program, out var program) && target.Instruction >= 0 &&
                target.Instruction < program.Instructions.Count, "unresolved-program-target", "program.target");
        }
        foreach (var map in definition.Maps.Values)
        {
            if (definition.Visuals is { } visuals)
            {
                Require(visuals.Maps.TryGetValue(map.Map, out var visual), "missing-map-visual", "presentation.maps");
                Require(map.Layout.Words.All(word => (word & 0x3FF) < visual!.Blocks.Count), "map-block-binding", "presentation.blocks");
                var sprites = map.Population is { } population
                    ? map.Entities.Where(entity => entity.Sprite >= population.AllyCount).Select(entity => entity.Sprite)
                        .Concat(population.Followers.Where(follower => follower.Character >= population.AllyCount).Select(follower => (int?)follower.Sprite))
                        .Concat((population.AllySprites ?? []).SelectMany(sprite => new int?[] { sprite.Sprite, sprite.UnjoinedSprite })).Append(population.PlayerSprite)
                    : map.Entities.Select(entity => entity.Sprite);
                Require(sprites.All(sprite => sprite is null || visuals.Sprites.ContainsKey(sprite.Value)), "missing-sprite-visual", "presentation.sprites");
            }
            Target(map.OnLoad); Target(map.InputProgram); Target(map.Battle?.BeforeProgram); Target(map.Battle?.StartProgram); Target(map.Battle?.LoadProgram);
            if (map.Battle?.Outcome is { } outcome)
            {
                Target(outcome.AfterProgram); Target(outcome.DefeatedProgram); Target(outcome.DefeatProgram); Target(outcome.ReturnProgram);
                Require(definition.Maps.ContainsKey(outcome.EgressMap), "missing-map", "map.battle.outcome.egress");
            }
            foreach (var entry in map.Events)
            {
                Target(entry.Program);
                if (entry.DestinationMap is { } destination) Require(definition.Maps.ContainsKey(destination), "missing-map", "event.map");
                if (entry.Entity is { } entity)
                    Require(map.Entities.Any(row => row.Entity == entity) || (map.Population is not null &&
                        entity.Value.StartsWith("entity-", StringComparison.Ordinal) && int.TryParse(entity.Value.AsSpan(7), out int identity) &&
                        identity is >= 0 and < 32 or >= 128 and < 160), "missing-entity", "event.entity");
            }
        }
        foreach (var program in definition.Programs.Values)
            foreach (var instruction in program.Instructions)
                switch (instruction)
                {
                    case JumpProgram jump: Target(jump.Target); break;
                    case BranchFlag branch: Target(branch.Target); break;
                    case BranchEntityCoordinates branch: Target(branch.Target); break;
                    case CallProgram call: Target(call.Target); break;
                    case TransferToMap transfer: Require(definition.Maps.ContainsKey(transfer.Map), "missing-map", "program.map"); break;
                    case LoadSceneMap load: Require(definition.Maps.ContainsKey(load.Map), "missing-map", "program.map"); break;
                    case SetEntitySprite sprite when definition.Visuals is { } visuals:
                        Require(visuals.Sprites.ContainsKey(sprite.Sprite), "missing-sprite-visual", "program.sprite"); break;
                    case LoadSceneEntities load when definition.Visuals is { } visuals:
                        var sprites = load.Entities.Where(entity => entity.Sprite >= load.Population.AllyCount).Select(entity => entity.Sprite)
                            .Concat((load.Population.AllySprites ?? []).SelectMany(sprite => new int?[] { sprite.Sprite, sprite.UnjoinedSprite }))
                            .Concat(load.Population.Followers.Where(follower => follower.Character >= load.Population.AllyCount).Select(follower => (int?)follower.Sprite))
                            .Append(load.Population.PlayerSprite);
                        Require(sprites.All(sprite => sprite is null || visuals.Sprites.ContainsKey(sprite.Value)), "missing-sprite-visual", "presentation.sprites"); break;
                }
    }
    private static ProgramLocation RequiredLocation(JsonElement value) => Location(value) ?? throw new AdmissionIssue(
        new(Application.Runtime.SessionFailureKind.ContentError, "program-target-required", "program.target", "program target required"));
    private static ProgramLocation? Location(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Null) return null;
        Object(value, "program-location", "program", "instruction");
        return new(Id(value, "program"), Number(value, "instruction", 0, 65535));
    }
    private static MapPosition Position(JsonElement value)
    { Object(value, "position", "x", "y"); return new(Number(value, "x", 0, 63), Number(value, "y", 0, 63)); }
    private static int? NullableNumber(JsonElement row, string key, int maximum) =>
        row.GetProperty(key).ValueKind == JsonValueKind.Null ? null : Number(row, key, 0, maximum);
}
