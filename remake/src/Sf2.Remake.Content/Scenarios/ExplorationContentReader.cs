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
        Object(world, "world", "maps", "programs", "texts");
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
            ObjectOptional(row, "program", "source", "id", "instructions");
            string id = Id(row, "id");
            Require(programIds.Add(id), "duplicate-program", "program.id");
            var instructions = Array(row, "instructions").Select(ReadInstruction).ToArray();
            Require(instructions.Length is >= 1 and <= 65536, "program-length", "program.instructions");
            Require(instructions[^1] is EndProgram or ReturnProgram or JumpProgram or UnsupportedInstruction,
                "unresolved-program-fallthrough", "program.instructions");
            programs.Add(new(id, instructions, row.TryGetProperty("source", out _) ? Text(row, "source") : null));
        }
        var maps = new List<ExplorationMapDefinition>();
        var mapIds = new HashSet<MapId>();
        foreach (var row in Array(world, "maps"))
        {
            ObjectOptional(row, "map", "input", "id", "layout", "areas", "entities", "events", "onLoad", "battle", "setup");
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
                Object(area, "area", "minX", "minY", "maxX", "maxY");
                int minX = Number(area, "minX", 0, width - 1), minY = Number(area, "minY", 0, rows.Length - 1);
                return new OriginalMapTraversalArea(minX, minY, Number(area, "maxX", minX, width - 1), Number(area, "maxY", minY, rows.Length - 1));
            }).ToArray();
            Require(areas.Length > 0 && areas.Distinct().Count() == areas.Length, "map-areas", "map.areas");
            var entities = Array(row, "entities").Select(entity =>
            {
                Object(entity, "entity", "id", "position", "facing", "speed", "visible", "obstruction");
                return new ExplorationEntityDefinition(new(Id(entity, "id")), Position(entity.GetProperty("position")),
                    (byte)Number(entity, "facing", 0, 7), (ushort)Number(entity, "speed", 1, 384),
                    Boolean(entity, "visible"), Boolean(entity, "obstruction"));
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
            maps.Add(new(id, new(words), new(areas), entities, events, Location(row.GetProperty("onLoad")), encounter, row.TryGetProperty("input", out var input) ? Location(input) : null, setupRoute));
        }
        Require(maps.Count > 0, "empty-world", "world.maps");
        var definition = new ExplorationDefinition(maps, programs, texts, provenance);
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

    private static ExplorationEvent ReadEvent(JsonElement row)
    {
        string kind = Text(row, "kind");
        if (kind == "interact")
        {
            Object(row, "event", "kind", "entity", "program", "requiredFlag", "requiredValue");
            return new(ExplorationEventKind.Interact, null, null, new(Id(row, "entity")), RequiredLocation(row.GetProperty("program")),
                RequiredFlag: NullableNumber(row, "requiredFlag", 65535), RequiredFlagValue: Boolean(row, "requiredValue"));
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
        Object(row, "event", "kind", "x", "y", "map", "position", "facing", "marker", "requiredFlag", "requiredValue");
        return new(ExplorationEventKind.Warp, NullableNumber(row, "x", 63), NullableNumber(row, "y", 63), null, null,
            new MapId(Id(row, "map")), Position(row.GetProperty("position")), (byte)Number(row, "facing", 0, 3),
            (ushort?)NullableNumber(row, "marker", 0x3C00), NullableNumber(row, "requiredFlag", 65535), Boolean(row, "requiredValue"));
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
            case "text-cursor": Object(row, opcode, "op", "text"); return new SetTextCursor(Number(row, "text", 0, 65534));
            case "show-text":
                ObjectOptional(row, opcode, "speakerFlags", "op", "mode", "speaker");
                string mode = Text(row, "mode"); Require(mode is "single" or "continued", "text-mode", "program.mode");
                return new ShowText(mode == "single" ? TextDisplayMode.Single : TextDisplayMode.Continued,
                    row.GetProperty("speaker").ValueKind == JsonValueKind.Null ? null : new EntityRef(Id(row, "speaker")),
                    row.TryGetProperty("speakerFlags", out _) ? (byte)Number(row, "speakerFlags", 0, 255) : (byte)0);
            case "close-text": Object(row, opcode, "op"); return new CloseText();
            case "yes-no": Object(row, opcode, "op", "flag"); return new ChooseYesNo(Number(row, "flag", 0, 65535));
            case "face": Object(row, opcode, "op", "entity", "facing"); return new SetEntityFacing(new(Id(row, "entity")), (byte)Number(row, "facing", 0, 7));
            case "position":
                Object(row, opcode, "op", "entity", "position", "facing");
                return new SetEntityPosition(new(Id(row, "entity")), Position(row.GetProperty("position")), (byte)Number(row, "facing", 0, 7));
            case "visibility": Object(row, opcode, "op", "entity", "visible"); return new SetEntityVisibility(new(Id(row, "entity")), Boolean(row, "visible"));
            case "motion":
                Object(row, opcode, "op", "entity", "actions", "wait");
                return new StartEntityMotion(new(Id(row, "entity")), Actions(row), Boolean(row, "wait"));
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
                case "move": Object(action, opcode, "op", "x", "y"); actions.Add(new MoveEntityRelative(Number(action, "x", -63, 63), Number(action, "y", -63, 63))); break;
                case "destination": Object(action, opcode, "op", "position"); actions.Add(new MoveEntityAbsolute(Position(action.GetProperty("position")))); break;
                case "face": Object(action, opcode, "op", "facing"); actions.Add(new FaceEntity((byte)Number(action, "facing", 0, 7))); break;
                case "wait": Object(action, opcode, "op", "ticks"); actions.Add(new WaitEntityTicks((byte)Number(action, "ticks", 0, 255))); break;
                case "speed": Object(action, opcode, "op", "x", "y"); actions.Add(new SetEntitySpeed((ushort)Number(action, "x", 1, 384), (ushort)Number(action, "y", 1, 384))); break;
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
