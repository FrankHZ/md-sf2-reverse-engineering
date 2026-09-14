using System.Text.Json;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Maps;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

internal static class ExplorationContentReader
{
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
        var maps = new List<ExplorationMapDefinition>();
        var mapIds = new HashSet<MapId>();
        foreach (var row in Array(world, "maps"))
        {
            ObjectOptional(row, "map", "input", ["id", "layout", "areas", "entities", "events", "onLoad", "battle", "setup",
                .. row.TryGetProperty("population", out _) ? new[] { "population" } : System.Array.Empty<string>(),
                .. row.TryGetProperty("entryFlags", out _) ? new[] { "entryFlags" } : System.Array.Empty<string>(),
                .. row.TryGetProperty("layoutEvents", out _) ? new[] { "layoutEvents" } : System.Array.Empty<string>()]);
            var id = new MapId(Id(row, "id"));
            Require(mapIds.Add(id), "duplicate-map", "world.maps");
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
                ObjectOptional(area, "area", "overlay", "minX", "minY", "maxX", "maxY");
                int minX = Number(area, "minX", 0, width - 1), minY = Number(area, "minY", 0, rows.Length - 1);
                return new OriginalMapTraversalArea(minX, minY, Number(area, "maxX", minX, width - 1), Number(area, "maxY", minY, rows.Length - 1));
            }).ToArray();
            Require(areas.Length > 0 && areas.Distinct().Count() == areas.Length, "map-areas", "map.areas");
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
            var events = Array(row, "events").Select(ReadEvent).ToArray();
            var route = row.GetProperty("battle");
            ExplorationBattleRoute? encounter = null;
            if (route.ValueKind != JsonValueKind.Null)
            {
                Object(route, "battle-route", "encounter", "unlockedFlag", "completedFlag", "introFlag", "before", "start");
                string encounterId = Id(route, "encounter");
                Require(battle.Definition.Encounters.ContainsKey(encounterId), "missing-encounter", "map.battle");
                encounter = new(encounterId, NullableNumber(route, "unlockedFlag", 65535), NullableNumber(route, "completedFlag", 65535),
                    NullableNumber(route, "introFlag", 65535), Location(route.GetProperty("before")), Location(route.GetProperty("start")));
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
                ObjectOptional(populationRow, "population", "allySprites", "allyCount", "nonAllyStart", "playerSprite", "followers");
                var followers = Array(populationRow, "followers").Select(follower =>
                {
                    Object(follower, "follower", "flag", "character", "sprite");
                    return new ExplorationFollowerDefinition(Number(follower, "flag", 0, 65535),
                        Number(follower, "character", 0, 255), Number(follower, "sprite", 0, 255));
                }).ToArray();
                population = new(Number(populationRow, "allyCount", 1, 128), Number(populationRow, "nonAllyStart", 128, 255),
                    Number(populationRow, "playerSprite", 0, 255), System.Array.AsReadOnly(followers),
                    populationRow.TryGetProperty("allySprites", out _) ? System.Array.AsReadOnly(Array(populationRow, "allySprites").Select(appearance =>
                    {
                        Object(appearance, "allySprite", "character", "sprite", "joinedFlag", "unjoinedSprite");
                        int? joined = NullableNumber(appearance, "joinedFlag", 65535), unjoined = NullableNumber(appearance, "unjoinedSprite", 255);
                        Require((joined is null) == (unjoined is null), "ally-sprite-override", "population.allySprites");
                        return new ExplorationAllySprite(Number(appearance, "character", 0, 255), Number(appearance, "sprite", 0, 255), joined, unjoined);
                    }).ToArray()) : null);
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
                }) : null));
        }
        Require(maps.Count > 0, "empty-world", "world.maps");
        var definition = new ExplorationDefinition(maps, programs, texts, provenance, partyFlags,
            world.TryGetProperty("presentation", out var presentation) ? ExplorationAssetReader.Read(presentation) : null,
            world.TryGetProperty("memberNames", out _) ? Array(world, "memberNames").Select(name =>
            {
                Require(name.ValueKind == JsonValueKind.String && name.GetString()!.Length is > 0 and <= 32, "member-name", "world.memberNames");
                return name.GetString()!;
            }) : null);
        Link(definition);
        ObjectOptional(start, "exploration-start", "program", "map", "player", "position", "facing", "speed", "flags");
        var selectedMap = new MapId(Id(start, "map"));
        Require(definition.Maps.ContainsKey(selectedMap), "missing-map", "start.map");
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
        return new(new(package, battle.Definition.Encounters.Values, battle.Definition.PrivateDefinitions, definition),
            new(selectedMap, new(Id(start, "player")), Position(start.GetProperty("position")),
                (byte)Number(start, "facing", 0, 3), (ushort)Number(start, "speed", 1, 384), flags, battle.Start, entryProgram));
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
        if (kind is "step" or "warp-frontier")
        {
            Object(row, "event", "kind", "x", "y", "program", "marker", "requiredFlag", "requiredValue");
            return new(kind == "step" ? ExplorationEventKind.Step : ExplorationEventKind.Warp,
                NullableNumber(row, "x", 63), NullableNumber(row, "y", 63), null,
                RequiredLocation(row.GetProperty("program")), RequiredMarker: (ushort?)NullableNumber(row, "marker", 0x3C00),
                RequiredFlag: NullableNumber(row, "requiredFlag", 65535), RequiredFlagValue: Boolean(row, "requiredValue"));
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
            case "return": Object(row, opcode, "op"); return new ReturnProgram();
            case "jump": Object(row, opcode, "op", "target"); return new JumpProgram(RequiredLocation(row.GetProperty("target")));
            case "call": Object(row, opcode, "op", "target"); return new CallProgram(RequiredLocation(row.GetProperty("target")));
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
                    .. row.TryGetProperty("useEventSpeaker", out _) ? new[] { "useEventSpeaker" } : System.Array.Empty<string>()]);
                string mode = Text(row, "mode"); Require(mode is "single" or "continued", "text-mode", "program.mode");
                return new ShowText(mode == "single" ? TextDisplayMode.Single : TextDisplayMode.Continued,
                    row.GetProperty("speaker").ValueKind == JsonValueKind.Null ? null : new EntityRef(Id(row, "speaker")),
                    row.TryGetProperty("speakerFlags", out _) ? (byte)Number(row, "speakerFlags", 0, 255) : (byte)0,
                    row.TryGetProperty("useEventSpeaker", out _) && Boolean(row, "useEventSpeaker"));
            case "close-text": Object(row, opcode, "op"); return new CloseText();
            case "yes-no": Object(row, opcode, "op", "flag"); return new ChooseYesNo(Number(row, "flag", 0, 65535));
            case "face": Object(row, opcode, "op", "entity", "facing"); return new SetEntityFacing(new(Id(row, "entity")), (byte)Number(row, "facing", 0, 7));
            case "priority": Object(row, opcode, "op", "entity", "value"); return new SetEntityPriority(new(Id(row, "entity")), Boolean(row, "value"));
            case "position":
                Object(row, opcode, "op", "entity", "position", "facing");
                return new SetEntityPosition(new(Id(row, "entity")), Position(row.GetProperty("position")), (byte)Number(row, "facing", 0, 7));
            case "hide": Object(row, opcode, "op", "entity", "removeAliases"); return new HideMapEntity(new(Id(row, "entity")), Boolean(row, "removeAliases"));
            case "speaker": Object(row, opcode, "op", "entity"); return new SetDialogueSpeaker(row.GetProperty("entity").ValueKind == JsonValueKind.Null ? null : new EntityRef(Id(row, "entity")));
            case "camera-target": Object(row, opcode, "op", "position"); return new SetCameraTarget(Position(row.GetProperty("position")));
            case "visibility": Object(row, opcode, "op", "entity", "visible"); return new SetEntityVisibility(new(Id(row, "entity")), Boolean(row, "visible"));
            case "motion":
                Object(row, opcode, "op", "entity", "actions", "wait");
                return new StartEntityMotion(new(Id(row, "entity")), Actions(row), Boolean(row, "wait"));
            case "join-party": Object(row, opcode, "op", "member"); return new JoinPartyMember(Number(row, "member", 0, 255));
            case "follow":
                Object(row, opcode, "op", "entity", "leader", "x", "y");
                return new FollowEntity(new(Id(row, "entity")), new(Id(row, "leader")), Number(row, "x", -127, 127), Number(row, "y", -127, 127));
            case "wait-entity": Object(row, opcode, "op", "entity"); return new WaitForEntity(new(Id(row, "entity")));
            case "wait-ticks": Object(row, opcode, "op", "ticks"); return new WaitProgramTicks(Number(row, "ticks", 0, 65535));
            case "present":
                Object(row, opcode, "op", "kind", "resource", "entity", "position");
                Require(Enum.GetNames<PresentationCueKind>().Contains(Text(row, "kind")), "presentation-cue", "program.kind", true);
                return new PresentCue(Enum.Parse<PresentationCueKind>(Text(row, "kind")), row.GetProperty("resource").ValueKind == JsonValueKind.Null ? null : Text(row, "resource"),
                    row.GetProperty("entity").ValueKind == JsonValueKind.Null ? null : new EntityRef(Id(row, "entity")),
                    row.GetProperty("position").ValueKind == JsonValueKind.Null ? null : Position(row.GetProperty("position")));
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
            Target(map.OnLoad); Target(map.InputProgram); Target(map.Battle?.BeforeProgram); Target(map.Battle?.StartProgram);
            foreach (var entry in map.Events)
            {
                Target(entry.Program);
                if (entry.DestinationMap is { } destination) Require(definition.Maps.ContainsKey(destination), "missing-map", "event.map");
                if (entry.Entity is { } entity) Require(map.Entities.Any(row => row.Entity == entity), "missing-entity", "event.entity");
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
