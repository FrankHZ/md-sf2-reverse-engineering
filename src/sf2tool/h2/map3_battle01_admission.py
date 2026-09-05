"""Public-safe H2 reconstruction of the Map 21 to Battle 01 admission spine."""

from __future__ import annotations

import hashlib
import json
import re
import subprocess
from collections import deque
from pathlib import Path
from typing import Any

from jsonschema import Draft7Validator

from sf2tool.h2.battlefield import _evaluate_equate, _load_equates
from sf2tool.h2.map3_castle_battle_unlock import (
    FIXTURE as R2B_FIXTURE,
)
from sf2tool.h2.map3_castle_battle_unlock import (
    build_map3_castle_battle_unlock_static,
)
from sf2tool.h2.map_layouts import decode_map_blocks, decode_map_layout
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.rom import inspect_rom

ID = "sf2-map3-battle01-admission-static-v1"
FIXTURE = repo_path("tests/fixtures/h2/map3-battle01-admission-static-v1.json")
SCHEMA = repo_path("schemas/h2/map3-battle01-admission-static-fixture.schema.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")
TOOLCHAIN = repo_path("manifests/toolchain.json")

_LISTING = Path("build/sf2build-h1.lst")
_H1_BINARY = Path("build/sf2build-h1.bin")
_ROM_SHA256 = "9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9"
_UPSTREAM_COMMIT = "c834c652b6862bc5679fd7f69a38a7093206efc6"
_DIRECTIONS = {"Up": (0, -1), "Right": (1, 0), "Down": (0, 1), "Left": (-1, 0)}
_MAP_EVENT_TYPE_MASK = 0x3C00
_MAP_EVENT_WARP = 0x1000

_SOURCE_SURFACE = (
    "sf2const.asm",
    "sf2cutscenemacros.asm",
    "sf2enums.asm",
    "sf2mapmacros.asm",
    "code/common/maps/getbattle.asm",
    "code/common/maps/mapinit_0.asm",
    "code/common/scripting/entity/entityscriptengine_2.asm",
    "code/common/scripting/map/ms_empty.asm",
    "code/gameflow/battle/battlefunctions/loadBattle.asm",
    "code/gameflow/battle/battleloop/activateenemies.asm",
    "code/gameflow/battle/battleloop/loadbattleterraindata.asm",
    "code/gameflow/battle/battleloop/populateenemyspawns.asm",
    "code/gameflow/battle/battleloop/turnorderfunctions.asm",
    "code/gameflow/battle/battleloop_1.asm",
    "code/gameflow/battle/cutscenes/battlestartcutscenesstart.asm",
    "code/gameflow/battle/cutscenes/beforebattlecutscenesstart.asm",
    "code/gameflow/battle/cutscenes/regionactivatedcutscenes.asm",
    "code/gameflow/mainloop.asm",
    "data/battles/cutscenes/battlestartcutscenes.asm",
    "data/battles/cutscenes/beforebattlecutscenes.asm",
    "data/battles/cutscenes/regionactivatedcutscenes.asm",
    "data/battles/entries/battle01/cs_beforebattle.asm",
    "data/battles/entries/battle01/terrain.bin",
    "data/battles/global/battlemapcoords.asm",
    "data/battles/spritesets/spriteset01.asm",
    "data/battles/terrainentries.asm",
    "data/maps/entries/map21/0-blocks.bin",
    "data/maps/entries/map21/1-layout.bin",
    "data/maps/entries/map21/2-areas.asm",
    "data/maps/entries/map21/6-warp-events.asm",
    "data/maps/entries/map40/0-blocks.bin",
    "data/maps/entries/map40/1-layout.bin",
    "data/maps/entries/map40/2-areas.asm",
    "data/maps/entries/map40/6-warp-events.asm",
    "data/maps/entries/map40/mapsetups/s1_entities.asm",
)

_FUNCTIONS = {
    "MainLoop": 0x075C4,
    "SwitchMap": 0x07956,
    "CheckBattle": 0x0799C,
    "BattleLoop": 0x23A84,
    "LoadBattle": 0x25610,
    "ExecuteBeforeBattleCutscene": 0x47A50,
    "ExecuteBattleStartCutscene": 0x47AEE,
    "ActivateEnemies": 0x2550C,
    "ExecuteBattleRegionCutscene": 0x47E82,
    "PopulateTargetsListWithSpawningEnemies": 0x1ACF30,
    "GenerateBattleTurnOrder": 0x25544,
    "ExecuteIndividualTurn": 0x23EB0,
    "BattleSpriteset01": 0x1B32E2,
    "BattleTerrain01": 0x1AD344,
    "bbcs_01": 0x494BC,
    "ms_Empty": 0x47A4E,
    "table_BattleMapCoordinates": 0x07A36,
    "rpt_BeforeBattleCutscenes": 0x47A88,
    "rpt_BattleStartCutscenes": 0x47B2C,
    "table_BattleRegionCutscenes": 0x47EC8,
    "SetBaseVIntFunctions": 0x25A94,
    "HealLivingAndImmortalAllies": 0x23BFC,
    "InitializeAllAlliesBattlePositions": 0x1B1272,
    "InitializeAllEnemiesBattlePositions": 0x1B12F2,
    "ClearAiMemory": 0x0C070,
    "PositionBattleEntities": 0x446A2,
    "LoadMapTilesets": 0x029E2,
    "InitializeSprites": 0x01770,
    "LoadMap": 0x02A8C,
    "LoadEntityMapsprites": 0x06024,
    "PlayMapMusic": 0x04544,
    "FadeOutToBlackAll": 0x258EA,
    "FadeInFromBlack": 0x00CD6,
    "WaitForFadeToFinish": 0x2590E,
    "WaitForVInt": 0x00EEE,
    "GetCombatantX": 0x08436,
    "GetCurrentHp": 0x08336,
    "GetCurrentAgi": 0x083B6,
    "AddCombatantAndRandomizedAgiToTurnOrder": 0x255A4,
}
_TABLE_SPANS = (
    ("map21WarpRows", 0xA61F8, 18),
    ("map40WarpRows", 0xB0ABC, 18),
    ("battle01MapCoordinate", 0x07A3D, 7),
    ("battle01BeforeCutsceneTarget", 0x47A8A, 2),
    ("battle01StartCutsceneTarget", 0x47B2E, 2),
    ("regionCutsceneRows", 0x47EC8, 34),
    ("battle01Spriteset", 0x1B32E2, 148),
    ("battle01Terrain", 0x1AD344, 284),
)
_AREA = re.compile(
    r"mainLayerStart\s+(\d+),\s*(\d+)\s+mainLayerEnd\s+(\d+),\s*(\d+)",
    re.MULTILINE,
)
_WARP = re.compile(
    r"\bmWarp\s+(\d+),\s*(\d+).*?\bwarpMap\s+([A-Z0-9_]+).*?"
    r"\bwarpDest\s+(\d+),\s*(\d+).*?\bwarpFacing\s+(\w+)",
    re.DOTALL,
)
_BATTLE_MAP_ROW = re.compile(r"^\s*battleMapCoordinates\s+(.+?)\s*$", re.MULTILINE)
_SPRITESET_ENTRY = re.compile(
    r"^\s*(allyCombatant|enemyCombatant)\s+([^\n]+)\n"
    r"\s*combatantAiAndItem\s+([^\n]+)\n"
    r"\s*combatantBehavior\s+([^\n]+)$",
    re.MULTILINE,
)


def canonical_json_bytes(value: dict[str, Any]) -> bytes:
    """Emit the sole canonical UTF-8 representation for the public fixture."""
    return json.dumps(value, sort_keys=True, separators=(",", ":")).encode("utf-8") + b"\n"


def _canonical(value: Any) -> bytes:
    return json.dumps(value, sort_keys=True, separators=(",", ":")).encode("utf-8")


def _disasm_root(upstream_path: Path) -> Path:
    root = upstream_path.resolve(strict=True)
    return root / "disasm" if (root / "disasm").is_dir() else root


def _without_comments(source: str) -> str:
    return "\n".join(line.split(";", maxsplit=1)[0].rstrip() for line in source.splitlines())


def _normalized(source: str) -> str:
    return "\n".join(
        re.sub(r"\s*,\s*", ",", " ".join(line.split()))
        for line in _without_comments(source).splitlines()
    )


def _require_order(source: str, expected: tuple[str, ...], context: str) -> None:
    clean = _normalized(source)
    cursor = 0
    for fragment in expected:
        index = clean.find(fragment, cursor)
        if index < 0:
            raise ValueError(f"Map 3 Battle 01 admission {context} source-use drift: {fragment}")
        cursor = index + len(fragment)


def _section(source: str, symbol: str) -> str:
    clean = _without_comments(source)
    match = re.search(rf"^{re.escape(symbol)}:\s*(?P<tail>.*)$", clean, re.MULTILINE)
    if match is None:
        raise ValueError(f"Map 3 Battle 01 admission source label is missing: {symbol}")
    return match.group("tail") + "\n" + clean[match.end() :]


def _read_source_surface(root: Path) -> tuple[dict[str, str], list[dict[str, str]]]:
    text: dict[str, str] = {}
    identities: list[dict[str, str]] = []
    for relative in _SOURCE_SURFACE:
        path = root / relative
        if not path.is_file():
            raise ValueError(f"Map 3 Battle 01 admission source is missing: {relative}")
        data = path.read_bytes()
        identities.append({"path": relative, "sha256": hashlib.sha256(data).hexdigest().upper()})
        if path.suffix == ".asm":
            text[relative] = data.decode("utf-8").replace("\r\n", "\n")
    if len(identities) != len(_SOURCE_SURFACE):
        raise ValueError("Map 3 Battle 01 admission source denominator drift")
    return text, identities


def _h1_span(h1_binary: bytes, rom: bytes, name: str, address: int, width: int) -> dict[str, Any]:
    h1 = h1_binary[address : address + width]
    if len(h1) != width:
        raise ValueError(f"Map 3 Battle 01 admission H1 span is incomplete: {name}")
    if rom[address : address + width] != h1:
        raise ValueError(f"Map 3 Battle 01 admission H1/ROM drift: {name}")
    return {
        "id": name,
        "address": address,
        "width": width,
        "sha256": hashlib.sha256(h1).hexdigest().upper(),
    }


def _h1_projection(h1_binary: bytes, rom: bytes) -> list[dict[str, Any]]:
    spans = [*((name, address, 2) for name, address in _FUNCTIONS.items()), *_TABLE_SPANS]
    return [_h1_span(h1_binary, rom, *span) for span in spans]


def _constants(disasm: Path) -> dict[str, int]:
    definitions = _load_equates(disasm / "sf2const.asm", disasm / "sf2enums.asm")
    memo: dict[str, int] = {}

    def value(name: str) -> int:
        return _evaluate_equate(name, definitions, memo)

    names = (
        "BATTLE_UNLOCKED_FLAGS_START",
        "BATTLE_UNLOCKED_TO_COMPLETED_FLAGS_OFFSET",
        "BATTLE_REGION_FLAGS_START",
        "BATTLE_REGION_FLAGS_END",
        "BATTLE_INTRO_CUTSCENE_FLAGS_START",
        "COMBATANT_ALLIES_COUNTER",
        "BATTLE_ENEMY_ENTITIES_COUNTER",
        "COMBATANT_ENEMIES_START",
        "TURN_ORDER_ENTRIES_COUNTER",
        "TURN_ORDER_ENTRY_SIZE",
        "TURN_AGILITY_MASK",
        "TWO_TURN_THRESHOLD",
        "MAP_ANCIENT_TOWER_EXTERIOR",
        "MAP_ANCIENT_TOWER_ENTRANCE",
        "MAP_GRANSEAL_CASTLE_1F",
        "MAP_GRANSEAL_CASTLE_3F",
    )
    return {name: value(name) for name in names}


def _parse_warps(source: str, constants: dict[str, int], map_id: int) -> list[dict[str, Any]]:
    rows = []
    clean = _without_comments(source)
    for x, y, map_symbol, destination_x, destination_y, facing in _WARP.findall(clean):
        if map_symbol not in constants:
            raise ValueError(f"Map 3 Battle 01 admission unresolvable warp map: {map_symbol}")
        rows.append(
            {
                "from": [int(x), int(y)],
                "toMap": constants[map_symbol],
                "to": [int(destination_x), int(destination_y)],
                "facing": facing,
            }
        )
    expected_count = {21: 2, 40: 2}[map_id]
    if len(rows) != expected_count or clean.count("mWarp") != expected_count:
        raise ValueError(f"Map 3 Battle 01 admission Map{map_id} warp row denominator drift")
    return rows


def _parse_areas(source: str, map_id: int) -> list[int]:
    rows = [tuple(int(value) for value in row) for row in _AREA.findall(_without_comments(source))]
    if len(rows) != 1:
        raise ValueError(f"Map 3 Battle 01 admission Map{map_id} area denominator drift")
    x0, y0, x1, y1 = rows[0]
    if x0 > x1 or y0 > y1:
        raise ValueError(f"Map 3 Battle 01 admission Map{map_id} area bounds drift")
    return [x0, y0, x1, y1]


def _controller(source: str) -> dict[str, Any]:
    _require_order(
        source,
        (
            "andi.w #$C000,d1",
            "btst #$F,d1",
            "addi.w #-$7E,d0",
            "addi.w #$7E,d0",
            "btst #$E,d1",
            "addi.w #$82,d0",
            "addi.w #-$82,d0",
            "cmpi.w #$1000,d3",
            "bsr.w WarpIfSetAtPoint",
            "cmpi.w #$C000,(a4,d2.w)",
        ),
        "controller collision",
    )
    return {
        "collisionMask": 0xC000,
        "rightStairMask": 0x8000,
        "leftStairMask": 0x4000,
        "stairWordDeltas": [-63, 63, 65, -65],
    }


def _surface(
    root: Path,
    map_id: int,
    area: list[int],
    controller: dict[str, Any],
    addresses: dict[str, int],
    h1_binary: bytes,
    rom: bytes,
) -> dict[str, Any]:
    base = root / f"data/maps/entries/map{map_id:02d}"
    payloads = {
        f"Map{map_id:02d}s0_Blocks": (base / "0-blocks.bin").read_bytes(),
        f"Map{map_id:02d}s1_Layout": (base / "1-layout.bin").read_bytes(),
    }
    for symbol, data in payloads.items():
        address = addresses.get(symbol)
        if address is None:
            raise ValueError(f"Map 3 Battle 01 admission H1 layout symbol is missing: {symbol}")
        if (
            h1_binary[address : address + len(data)] != data
            or rom[address : address + len(data)] != data
        ):
            raise ValueError(f"Map 3 Battle 01 admission source/H1/ROM layout drift: {symbol}")
    block_words, _, _ = decode_map_blocks(payloads[f"Map{map_id:02d}s0_Blocks"])
    layout_words, _, _, _ = decode_map_layout(
        payloads[f"Map{map_id:02d}s1_Layout"], len(block_words) // 9
    )
    if len(layout_words) != 64 * 64:
        raise ValueError(f"Map 3 Battle 01 admission Map{map_id} layout dimension drift")
    return {
        "map": map_id,
        "area": area,
        "layout": layout_words,
        "width": 64,
        "blocksSha256": hashlib.sha256(payloads[f"Map{map_id:02d}s0_Blocks"]).hexdigest().upper(),
        "layoutSha256": hashlib.sha256(payloads[f"Map{map_id:02d}s1_Layout"]).hexdigest().upper(),
        "collisionSha256": hashlib.sha256(
            bytes(
                int(word & controller["collisionMask"] == controller["collisionMask"])
                for word in layout_words
            )
        )
        .hexdigest()
        .upper(),
    }


def _in_area(point: tuple[int, int], area: list[int]) -> bool:
    x, y = point
    x0, y0, x1, y1 = area
    return x0 <= x <= x1 and y0 <= y <= y1


def _move(
    surface: dict[str, Any], point: tuple[int, int], direction: str, controller: dict[str, Any]
) -> tuple[int, int] | None:
    dx, dy = _DIRECTIONS[direction]
    x, y = point
    width = surface["width"]
    layout = surface["layout"]
    current = layout[y * width + x] & controller["collisionMask"]
    if direction in {"Right", "Left"} and current in {
        controller["rightStairMask"],
        controller["leftStairMask"],
    }:
        offset_index = {
            (controller["rightStairMask"], "Right"): 0,
            (controller["rightStairMask"], "Left"): 1,
            (controller["leftStairMask"], "Right"): 2,
            (controller["leftStairMask"], "Left"): 3,
        }[(current, direction)]
        offset = controller["stairWordDeltas"][offset_index]
        candidate = (
            x + dx,
            y + ((offset - 1) // width if direction == "Right" else (offset + 1) // width),
        )
        if (
            0 <= candidate[0] < width
            and 0 <= candidate[1] < width
            and layout[candidate[1] * width + candidate[0]] & controller["collisionMask"] == current
        ):
            dy = candidate[1] - y
    candidate = (x + dx, y + dy)
    if not (0 <= candidate[0] < width and 0 <= candidate[1] < width):
        return None
    if not _in_area(candidate, surface["area"]):
        return None
    return (
        candidate
        if layout[candidate[1] * width + candidate[0]] & controller["collisionMask"]
        < controller["collisionMask"]
        else None
    )


def _shortest(
    surface: dict[str, Any],
    start: tuple[int, int],
    end: tuple[int, int],
    controller: dict[str, Any],
    occupied: frozenset[tuple[int, int]] = frozenset(),
) -> tuple[list[list[int]], list[str]]:
    queue: deque[tuple[int, int]] = deque([start])
    previous: dict[tuple[int, int], tuple[tuple[int, int], str] | None] = {start: None}
    while queue:
        point = queue.popleft()
        if point == end:
            break
        for direction in _DIRECTIONS:
            candidate = _move(surface, point, direction, controller)
            if candidate is not None and candidate not in occupied and candidate not in previous:
                previous[candidate] = (point, direction)
                queue.append(candidate)
    if end not in previous:
        raise ValueError(f"Map 3 Battle 01 admission static route is blocked: {start}->{end}")
    points, inputs = [list(end)], []
    cursor = end
    while previous[cursor] is not None:
        previous_point, direction = previous[cursor]  # type: ignore[misc]
        points.append(list(previous_point))
        inputs.append(direction)
        cursor = previous_point
    return list(reversed(points)), list(reversed(inputs))


def _navigation(
    identifier: str,
    surface: dict[str, Any],
    start: tuple[int, int],
    end: tuple[int, int],
    controller: dict[str, Any],
    occupied: frozenset[tuple[int, int]],
    blocked_predicate_nodes: frozenset[tuple[int, int]],
) -> dict[str, Any]:
    points, inputs = _shortest(surface, start, end, controller, occupied | blocked_predicate_nodes)
    return {
        "id": identifier,
        "kind": "navigation",
        "map": surface["map"],
        "from": list(start),
        "to": list(end),
        "points": points,
        "inputs": inputs,
    }


def _warp_trigger_points(
    surface: dict[str, Any], source: tuple[int, int]
) -> list[tuple[int, int]]:
    """Return source-coordinate matches that can actually dispatch a warp event."""
    source_x, source_y = source
    width = surface["width"]
    x_values = range(width) if source_x == 0xFF else (source_x,)
    y_values = range(width) if source_y == 0xFF else (source_y,)
    return [
        (x, y)
        for y in y_values
        for x in x_values
        if 0 <= x < width
        and 0 <= y < width
        and _in_area((x, y), surface["area"])
        and surface["layout"][y * width + x] & _MAP_EVENT_TYPE_MASK == _MAP_EVENT_WARP
    ]


def _retained_r2b(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    fixture_path = R2B_FIXTURE
    fixture = load_json(fixture_path)
    if fixture.get("id") != "sf2-map3-castle-battle-unlock-static-v1":
        raise ValueError("Map 3 Battle 01 admission retained R2b fixture identity drift")
    fresh = build_map3_castle_battle_unlock_static(rom_path, upstream_path)
    if fresh != fixture:
        raise ValueError("Map 3 Battle 01 admission retained R2b fixture projection drift")
    terminal = fresh["static"]["routeGraph"]["segments"][-1]
    projection = {
        "fixtureId": fixture["id"],
        "fixtureSha256": hashlib.sha256(fixture_path.read_bytes()).hexdigest().upper(),
        "routeGraphSha256": fresh["static"]["routeGraphSha256"],
        "terminal": {
            "map": terminal["map"],
            "player": terminal["player"],
            "facing": terminal["facing"],
            "program": terminal["program"],
            "setFlags": terminal["setFlags"],
            "entityPointAfter": terminal["entityPointAfter"],
        },
    }
    if projection["terminal"] != {
        "map": 21,
        "player": [5, 15],
        "facing": "DOWN",
        "program": "cs_53EF4",
        "setFlags": [401, 256],
        "entityPointAfter": [6, 16],
    }:
        raise ValueError("Map 3 Battle 01 admission retained R2b terminal model drift")
    projection["sha256"] = hashlib.sha256(_canonical(projection)).hexdigest().upper()
    return projection


def _extension_route(
    surfaces: dict[int, dict[str, Any]],
    warps: dict[str, list[dict[str, Any]]],
    controller: dict[str, Any],
    retained: dict[str, Any],
    map40_entities: str,
) -> dict[str, Any]:
    map21_rows = warps["map21"]
    map40_rows = warps["map40"]
    selected21 = [row for row in map21_rows if row["from"] == [9, 1]]
    selected40 = [row for row in map40_rows if row["from"] == [255, 12]]
    if selected21 != [{"from": [9, 1], "toMap": 40, "to": [4, 30], "facing": "UP"}]:
        raise ValueError("Map 3 Battle 01 admission Map21 selected warp drift")
    if selected40 != [{"from": [255, 12], "toMap": 57, "to": [8, 18], "facing": "UP"}]:
        raise ValueError("Map 3 Battle 01 admission Map40 selected wildcard warp drift")
    start21 = tuple(retained["terminal"]["player"])
    post_program_occupancy = frozenset({tuple(retained["terminal"]["entityPointAfter"])})
    route21 = _navigation(
        "map21-terminal-to-north-exit",
        surfaces[21],
        start21,
        (9, 1),
        controller,
        post_program_occupancy,
        frozenset(),
    )
    trigger_points = _warp_trigger_points(surfaces[40], tuple(selected40[0]["from"]))
    candidates: list[tuple[int, tuple[int, int], list[list[int]], list[str]]] = []
    for point in trigger_points:
        try:
            points, inputs = _shortest(surfaces[40], (4, 30), point, controller)
        except ValueError:
            continue
        candidates.append((len(inputs), point, points, inputs))
    if not candidates:
        raise ValueError(
            "Map 3 Battle 01 admission Map40 wildcard has no reachable warp-event terminal"
        )
    candidates.sort(key=lambda item: (item[0], item[1]))
    _, selected_terminal, _, _ = candidates[0]
    wildcard_nodes = frozenset(point for _, point, _, _ in candidates if point != selected_terminal)
    entity_lines = [
        line.strip()
        for line in _without_comments(map40_entities).splitlines()
        if line.strip() and not line.rstrip().endswith(":")
    ]
    if entity_lines != ["msEntitiesEnd"]:
        raise ValueError("Map 3 Battle 01 admission Map40 entity occupancy drift")
    route40 = _navigation(
        "map40-entry-to-wildcard-battle-warp",
        surfaces[40],
        (4, 30),
        selected_terminal,
        controller,
        frozenset(),
        wildcard_nodes,
    )
    segments = [
        route21,
        {
            "id": "map21-to-map40-north-warp",
            "kind": "warp",
            "from": {"map": 21, "point": [9, 1], "facing": "UP"},
            "to": {"map": 40, "point": [4, 30], "facing": "UP"},
        },
        route40,
        {
            "id": "map40-to-map57-wildcard-warp",
            "kind": "warp",
            "from": {"map": 40, "point": list(selected_terminal), "facing": "UP"},
            "to": {"map": 57, "point": [8, 18], "facing": "UP"},
        },
    ]
    route = {
        "surfaces": [
            {
                key: surfaces[map_id][key]
                for key in ("map", "width", "blocksSha256", "layoutSha256", "collisionSha256")
            }
            for map_id in (21, 40)
        ],
        "areas": [{"map": map_id, "bounds": surfaces[map_id]["area"]} for map_id in (21, 40)],
        "occupancy": [
            {"map": 21, "point": list(next(iter(post_program_occupancy)))},
            {"map": 40, "entityCount": 0},
        ],
        "map40WildcardCandidates": [
            {"point": list(point), "inputCount": count} for count, point, _, _ in candidates
        ],
        "segments": segments,
    }
    route["nodeCount"] = sum(len(segment.get("points", [])) for segment in segments)
    route["inputCount"] = sum(len(segment.get("inputs", [])) for segment in segments)
    route["sha256"] = hashlib.sha256(_canonical(route)).hexdigest().upper()
    if (route["nodeCount"], route["inputCount"]) != (48, 46):
        raise ValueError("Map 3 Battle 01 admission extension route denominator drift")
    return route


def _parse_battle_rows(source: str) -> list[list[int]]:
    rows = []
    for operands in _BATTLE_MAP_ROW.findall(_without_comments(source)):
        values = [part.strip() for part in operands.split(",")]
        if len(values) != 7:
            raise ValueError("Map 3 Battle 01 admission battle-map row width drift")
        rows.append([int(value, 0) for value in values])
    if len(rows) != 45:
        raise ValueError("Map 3 Battle 01 admission battle-map row denominator drift")
    return rows


def _main_and_battle_spine(constants: dict[str, int], text: dict[str, str]) -> dict[str, Any]:
    _require_order(
        text["code/common/maps/mapinit_0.asm"],
        (
            "SwitchMap:",
            "lea table_FlagSwitchedMaps(pc),a0",
            "jsr j_CheckFlag",
        ),
        "SwitchMap table dispatch",
    )
    _require_order(
        text["code/gameflow/mainloop.asm"],
        (
            "bsr.w SwitchMap",
            "bsr.w CheckBattle",
            "cmpi.w #-1,d7",
            "beq.w @Exploration",
            "move.w d7,d1",
            "jsr j_BattleLoop",
            "jsr j_ExplorationLoop",
        ),
        "MainLoop SwitchMap/CheckBattle/Exploration order",
    )
    check_battle = text["code/common/maps/getbattle.asm"]
    _require_order(
        check_battle,
        (
            "lea table_BattleMapCoordinates(pc),a0",
            "moveq #BATTLES_MAX_INDEX,d6",
            "clr.w d7",
            "cmp.b (a0),d0",
            "move.w #BATTLE_UNLOCKED_FLAGS_START,d1",
            "add.w d7,d1",
            "jsr j_CheckFlag",
            "cmpi.b #-1,BATTLEMAPCOORDINATES_OFFSET_TRIGGER_X(a0)",
            "cmpi.b #-1,BATTLEMAPCOORDINATES_OFFSET_TRIGGER_Y(a0)",
            "move.b BATTLEMAPCOORDINATES_OFFSET_X(a0),((BATTLE_AREA_X-$1000000)).w",
            "addi.w #BATTLE_UNLOCKED_TO_COMPLETED_FLAGS_OFFSET,d1",
            "jsr j_CheckFlag",
            "subi.w #BATTLE_UNLOCKED_TO_COMPLETED_FLAGS_OFFSET,d1",
            "jsr j_ClearFlag",
            "moveq #-1,d7",
        ),
        "CheckBattle selected-row control flow",
    )
    rows = _parse_battle_rows(text["data/battles/global/battlemapcoords.asm"])
    selected = rows[1]
    if selected != [57, 0, 0, 16, 20, 255, 255]:
        raise ValueError("Map 3 Battle 01 admission battle row 1 drift")
    battle_loop = text["code/gameflow/battle/battleloop_1.asm"]
    _require_order(
        battle_loop,
        (
            "chkFlg 88",
            "beq.s @Initialize",
            "clr.l ((SECONDS_COUNTER-$1000000)).w",
            "move.b d0,((CURRENT_MAP-$1000000)).w",
            "move.b d1,((CURRENT_BATTLE-$1000000)).w",
            "bsr.w SetBaseVIntFunctions",
            "jsr j_ExecuteBeforeBattleCutscene",
            "moveq #BATTLE_REGION_FLAGS_START,d1",
            "jsr j_ClearFlag",
            "cmpi.w #BATTLE_REGION_FLAGS_END,d1",
            "bsr.w HealLivingAndImmortalAllies",
            "jsr j_InitializeAllAlliesBattlePositions",
            "jsr j_InitializeAllEnemiesBattlePositions",
            "jsr j_ClearAiMemory",
            "clr.w d0",
            "bsr.w LoadBattle",
            "jsr j_ExecuteBattleStartCutscene",
            "bsr.w ActivateEnemies",
            "jsr j_ExecuteBattleRegionCutscene",
            "jsr j_PopulateTargetsListWithSpawningEnemies",
            "bsr.w GenerateBattleTurnOrder",
            "move.b (a0,d0.w),d0",
            "bsr.w ExecuteIndividualTurn",
        ),
        "BattleLoop new-battle and pre-first-turn order",
    )
    _require_order(
        text["code/gameflow/battle/battleloop/activateenemies.asm"],
        (
            "ActivateEnemies:",
            "move.w #COMBATANT_ENEMIES_START,d0",
            "bsr.w UpdateEnemyActivationBitfield",
        ),
        "first-round enemy activation",
    )
    _require_order(
        text["code/gameflow/battle/battleloop/populateenemyspawns.asm"],
        (
            "PopulateTargetsListWithSpawningEnemies:",
            "jsr j_GetActivationBitfield",
            "andi.w #AIBITFIELD_INITIALIZATION_MASK,d1",
            "lea ((TARGETS_LIST_LENGTH-$1000000)).w,a0",
        ),
        "first-round spawn eligibility",
    )
    return {
        "mainLoop": {
            "orderedCalls": ["SwitchMap", "CheckBattle", "BattleLoop", "ExplorationLoop"],
            "battleSentinel": -1,
            "staticOnly": True,
        },
        "checkBattle": {
            "tableRowIndex": 1,
            "map": selected[0],
            "area": selected[1:5],
            "trigger": selected[5:],
            "unlockedFlag": constants["BATTLE_UNLOCKED_FLAGS_START"] + 1,
            "completedFlag": constants["BATTLE_UNLOCKED_FLAGS_START"]
            + 1
            + constants["BATTLE_UNLOCKED_TO_COMPLETED_FLAGS_OFFSET"],
            "writes": ["BATTLE_AREA_X", "BATTLE_AREA_Y", "BATTLE_AREA_WIDTH", "BATTLE_AREA_HEIGHT"],
            "resultRegister": {"name": "d7", "value": 1},
        },
        "newBattle": {
            "suspendFlag": 88,
            "newBranchWhenClear": True,
            "currentMap": 57,
            "currentBattle": 1,
            "loadBattleD0": 0,
            "secondsCleared": True,
            "regionFlagRange": [
                constants["BATTLE_REGION_FLAGS_START"],
                constants["BATTLE_REGION_FLAGS_END"],
            ],
            "orderedSteps": [
                "SetBaseVIntFunctions",
                "ExecuteBeforeBattleCutscene",
                "ClearBattleRegionFlags",
                "HealLivingAndImmortalAllies",
                "InitializeAllAlliesBattlePositions",
                "InitializeAllEnemiesBattlePositions",
                "ClearAiMemory",
                "LoadBattle",
                "ExecuteBattleStartCutscene",
            ],
        },
        "firstRound": {
            "orderedSteps": [
                "ActivateEnemies",
                "ExecuteBattleRegionCutscene",
                "PopulateTargetsListWithSpawningEnemies",
                "GenerateBattleTurnOrder",
            ],
            "endpoint": {
                "after": "GenerateBattleTurnOrder",
                "before": ["BATTLE_TURN_ORDER read", "ExecuteIndividualTurn"],
                "notPlayerReady": True,
            },
        },
    }


def _parse_cutscene_table(source: str, table: str, expected_row: int) -> str:
    section = _section(source, table)
    entries = re.findall(
        r"dc\.w\s+(?:\((\w+)-\w+\)|([A-Za-z0-9_]+)-\w+)",
        _without_comments(section),
    )
    if len(entries) < 45:
        raise ValueError(f"Map 3 Battle 01 admission {table} row denominator drift")
    return next(value for value in entries[expected_row] if value)


def _command_catalog(source: str) -> dict[str, tuple[int, tuple[str, ...]]]:
    catalog: dict[str, tuple[int, tuple[str, ...]]] = {}
    for match in re.finditer(
        r"^(\w+):[ \t]*macro(?:[ \t]*;[ \t]*alias)?[ \t]*$", source, re.MULTILINE
    ):
        name = match.group(1)
        body = source[match.end() :]
        end = re.search(r"^\s*endm\s*$", body, re.MULTILINE)
        if end is None:
            raise ValueError(f"Map 3 Battle 01 admission macro terminator missing: {name}")
        lines = [
            line.strip()
            for line in _without_comments(body[: end.start()]).splitlines()
            if line.strip()
        ]
        direct = next((line for line in lines if line.startswith(("dc.w", "dc.b"))), None)
        if direct is not None:
            literal = re.search(r"\$(?:[0-9A-Fa-f]+)", direct)
            if literal is not None:
                catalog[name] = (int(literal.group(0)[1:], 16), ())
        elif lines and lines[0].split()[0] in catalog:
            target = lines[0].split()[0]
            catalog[name] = catalog[target]
    required = {
        "textCursor",
        "loadMapFadeIn",
        "loadMapEntities",
        "setActscriptWait",
        "setPos",
        "fadeInB",
        "setCamDest",
        "csWait",
        "setFacing",
        "nextText",
        "nextSingleText",
        "setCameraEntity",
        "setActscript",
        "mapFadeOutToWhite",
        "mapFadeInFromWhite",
        "animEntityFX",
        "entityActions",
        "entityActionsWait",
        "shiver",
        "csc_end",
    }
    if not required.issubset(catalog):
        raise ValueError(
            "Map 3 Battle 01 admission cutscene command catalog drift: "
            f"{sorted(required - set(catalog))}; parsed={sorted(catalog)}"
        )
    return catalog


def _before_battle_commands(
    source: str, macros: dict[str, tuple[int, tuple[str, ...]]]
) -> dict[str, Any]:
    clean = _without_comments(source)
    start = re.search(r"^bbcs_01:\s*", clean, re.MULTILINE)
    end = re.search(r"^\s*csc_end\s*$", clean, re.MULTILINE)
    if start is None or end is None or end.start() < start.end():
        raise ValueError("Map 3 Battle 01 admission bbcs_01 boundary drift")
    commands = []
    for raw in clean[start.end() : end.end()].splitlines():
        line = raw.strip()
        if not line:
            continue
        name, *remainder = line.split(maxsplit=1)
        if name not in macros:
            continue
        operands = [] if not remainder else [part.strip() for part in remainder[0].split(",")]
        if name == "setActscript":
            operands = [operands[0], "0", operands[1]]
        elif name == "setActscriptWait":
            operands = [operands[0], "$FF", operands[1]]
        elif name == "entityActions":
            operands = [operands[0], "0"]
        elif name == "entityActionsWait":
            operands = [operands[0], "$FF"]
        commands.append({"id": macros[name][0], "macro": name, "operands": operands})
    if not commands or commands[-1] != {"id": 65535, "macro": "csc_end", "operands": []}:
        raise ValueError("Map 3 Battle 01 admission bbcs_01 terminal command drift")
    return {
        "address": _FUNCTIONS["bbcs_01"],
        "commands": commands,
        "sha256": hashlib.sha256(_canonical(commands)).hexdigest().upper(),
    }


def _battle01_cutscene_targets(before_source: str, start_source: str) -> tuple[str, str]:
    before_target = _parse_cutscene_table(before_source, "rpt_BeforeBattleCutscenes", 1)
    start_target = _parse_cutscene_table(start_source, "rpt_BattleStartCutscenes", 1)
    if before_target != "bbcs_01" or start_target != "ms_Empty":
        raise ValueError("Map 3 Battle 01 admission Battle01 cutscene table target drift")
    return before_target, start_target


def _cutscene_and_definition_spine(
    constants: dict[str, int], text: dict[str, str]
) -> dict[str, Any]:
    before = text["code/gameflow/battle/cutscenes/beforebattlecutscenesstart.asm"]
    start = text["code/gameflow/battle/cutscenes/battlestartcutscenesstart.asm"]
    _require_order(
        before,
        (
            "move.b ((CURRENT_BATTLE-$1000000)).w,d1",
            "addi.w #BATTLE_INTRO_CUTSCENE_FLAGS_START,d1",
            "jsr j_CheckFlag",
            "move.w rpt_BeforeBattleCutscenes(pc,d0.w),d0",
            "bsr.w ExecuteMapScript",
        ),
        "before-battle cutscene route",
    )
    _require_order(
        start,
        (
            "move.b ((CURRENT_BATTLE-$1000000)).w,d1",
            "addi.w #BATTLE_INTRO_CUTSCENE_FLAGS_START,d1",
            "jsr j_CheckFlag",
            "jsr j_SetFlag",
            "move.w rpt_BattleStartCutscenes(pc,d0.w),d0",
            "bsr.w ExecuteMapScript",
        ),
        "battle-start cutscene route",
    )
    before_target, start_target = _battle01_cutscene_targets(
        text["data/battles/cutscenes/beforebattlecutscenes.asm"],
        text["data/battles/cutscenes/battlestartcutscenes.asm"],
    )
    if "ms_Empty: dc.w $FFFF" not in _normalized(text["code/common/scripting/map/ms_empty.asm"]):
        raise ValueError("Map 3 Battle 01 admission ms_Empty program drift")
    _require_order(
        text["code/gameflow/battle/cutscenes/regionactivatedcutscenes.asm"],
        (
            "ExecuteBattleRegionCutscene:",
            "lea table_BattleRegionCutscenes-8(pc),a0",
            "cmpi.w #TERMINATOR_WORD,(a0)",
            "trap #MAPSCRIPT",
        ),
        "region-cutscene route",
    )
    macros = _command_catalog(text["sf2cutscenemacros.asm"])
    commands = _before_battle_commands(
        text["data/battles/entries/battle01/cs_beforebattle.asm"], macros
    )
    spriteset = _spriteset(text["data/battles/spritesets/spriteset01.asm"])
    regions = _region_table(text["data/battles/cutscenes/regionactivatedcutscenes.asm"])
    return {
        "introFlag": constants["BATTLE_INTRO_CUTSCENE_FLAGS_START"] + 1,
        "beforeBattle": {"tableRow": 1, "target": before_target, "program": commands},
        "battleStart": {
            "tableRow": 1,
            "target": start_target,
            "introFlag": constants["BATTLE_INTRO_CUTSCENE_FLAGS_START"] + 1,
            "setsIntroFlagBeforeProgram": True,
        },
        "spriteset": spriteset,
        "regionCutscenes": regions,
    }


def _spriteset(source: str) -> dict[str, Any]:
    clean = _without_comments(source)
    header_match = re.search(
        r"BattleSpriteset01:\s*dc\.b\s+(\d+)\s*dc\.b\s+(\d+)\s*dc\.b\s+(\d+)\s*dc\.b\s+(\d+)",
        clean,
        re.DOTALL,
    )
    if header_match is None:
        raise ValueError("Map 3 Battle 01 admission spriteset header drift")
    header = [int(value) for value in header_match.groups()]
    entries = []
    for kind, primary, ai_item, behavior in _SPRITESET_ENTRY.findall(clean):
        item_values = [value.strip() for value in ai_item.split(",")]
        behavior_values = [value.strip() for value in behavior.split(",")]
        primary_values = [value.strip() for value in primary.split(",")]
        if len(item_values) != 2 or len(behavior_values) != 6 or len(primary_values) != 3:
            raise ValueError("Map 3 Battle 01 admission spriteset entry width drift")
        entries.append(
            {
                "kind": "ally" if kind == "allyCombatant" else "enemy",
                "startState": behavior_values[-1],
            }
        )
    allies = [entry for entry in entries if entry["kind"] == "ally"]
    enemies = [entry for entry in entries if entry["kind"] == "enemy"]
    if header != [3, 6, 3, 0] or len(allies) != 3 or len(enemies) != 6:
        raise ValueError("Map 3 Battle 01 admission spriteset count drift")
    if any(entry["startState"] != "STARTING" for entry in entries):
        raise ValueError("Map 3 Battle 01 admission spriteset starting-state drift")
    structural_rows = [
        {"kind": entry["kind"], "startState": entry["startState"]} for entry in entries
    ]
    return {
        "address": _FUNCTIONS["BattleSpriteset01"],
        "counts": header,
        "entryCount": len(entries),
        "allStarting": True,
        "sha256": hashlib.sha256(_canonical(structural_rows)).hexdigest().upper(),
    }


def _region_table(source: str) -> dict[str, Any]:
    clean = _without_comments(source)
    section = _section(clean, "table_BattleRegionCutscenes")
    rows = re.findall(
        r"dc\.b\s+([A-Z0-9_]+)\s*dc\.b\s+(\d+)\s*dc\.w\s+(\d+)\s*dc\.l\s+(\w+)", section, re.DOTALL
    )
    if len(rows) != 4:
        raise ValueError("Map 3 Battle 01 admission region-cutscene row denominator drift")
    if any(row[0] in {"BATTLE_INSIDE_ANCIENT_TOWER", "BATTLE01", "BATTLE_01"} for row in rows):
        raise ValueError("Map 3 Battle 01 admission unexpected Battle01 region-cutscene row")
    structural_rows = [
        {"battle": battle, "region": int(region), "flag": int(flag), "program": program}
        for battle, region, flag, program in rows
    ]
    return {
        "address": _FUNCTIONS["table_BattleRegionCutscenes"],
        "rowCount": len(rows),
        "battle01Rows": 0,
        "sha256": hashlib.sha256(_canonical(structural_rows)).hexdigest().upper(),
    }


def _load_battle_and_turn_order(constants: dict[str, int], text: dict[str, str]) -> dict[str, Any]:
    _require_order(
        text["code/gameflow/battle/battlefunctions/loadBattle.asm"],
        (
            "move.b ((CURRENT_MAP-$1000000)).w,d1",
            "bsr.w FadeOutToBlackAll",
            "jsr (LoadMapTilesets).w",
            "bsr.w WaitForFadeToFinish",
            "trap #VINT_FUNCTIONS",
            "dc.w VINTS_CLEAR",
            "jsr (WaitForVInt).w",
            "jsr j_PositionBattleEntities",
            "jsr (InitializeSprites).w",
            "jsr (LoadMap).w",
            "jsr (WaitForVInt).w",
            "jsr (LoadEntityMapsprites).w",
            "bsr.w SetBaseVIntFunctions",
            "jsr j_LoadBattleTerrainData",
            "jsr (PlayMapMusic).w",
            "jsr (FadeInFromBlack).w",
        ),
        "LoadBattle selected map order",
    )
    _require_order(
        text["code/gameflow/battle/battleloop/loadbattleterraindata.asm"],
        (
            "LoadBattleTerrainData:",
            "lea pt_BattleTerrainData(pc),a0",
            "move.b (a1),d1",
            "lsl.l #2,d1",
            "movea.l (a0,d1.w),a0",
            "jsr (LoadStackCompressedData).w",
        ),
        "BattleTerrain01 loader route",
    )
    terrain_entries = _without_comments(text["data/battles/terrainentries.asm"])
    if not re.search(
        r"pt_BattleTerrainData:.*?dc\.l\s+BattleTerrain01", terrain_entries, re.DOTALL
    ):
        raise ValueError("Map 3 Battle 01 admission BattleTerrain01 pointer drift")
    turn_order = text["code/gameflow/battle/battleloop/turnorderfunctions.asm"]
    _require_order(
        turn_order,
        (
            "lea ((BATTLE_TURN_ORDER-$1000000)).w,a0",
            "moveq #TURN_ORDER_ENTRIES_COUNTER,d7",
            "move.w #-1,(a0)+",
            "dbf d7,@ClearTurnOrder_Loop",
            "moveq #COMBATANT_ALLIES_COUNTER,d7",
            "bsr.w AddCombatantAndRandomizedAgiToTurnOrder",
            "dbf d7,@AddAllyTurns_Loop",
            "move.w #COMBATANT_ENEMIES_START,d0",
            "moveq #BATTLE_ENEMY_ENTITIES_COUNTER,d7",
            "bsr.w AddCombatantAndRandomizedAgiToTurnOrder",
            "dbf d7,@AddEnemyTurns_Loop",
            "moveq #COMBATANTS_ALL_COUNTER,d6",
            "moveq #TURN_ORDER_ENTRIES_MINUS_ONE_COUNTER,d7",
            "move.w (a0),d0",
            "move.w TURN_ORDER_ENTRY_SIZE(a0),d1",
            "cmp.b d0,d1",
            "ble.s @InOrder",
            "move.w d1,(a0)",
            "move.w d0,TURN_ORDER_ENTRY_SIZE(a0)",
            "addq.l #TURN_ORDER_ENTRY_SIZE,a0",
            "dbf d7,@SortCombatants_InnerLoop",
            "dbf d6,@SortCombatants_OuterLoop",
            "clr.b ((CURRENT_BATTLE_TURN-$1000000)).w",
        ),
        "turn-order generation",
    )
    _require_order(
        _section(turn_order, "AddCombatantAndRandomizedAgiToTurnOrder"),
        (
            "jsr j_GetCombatantX",
            "tst.b d1",
            "bmi.w @Return",
            "jsr j_GetCurrentHp",
            "tst.w d1",
            "beq.w @Return",
            "jsr j_GetCurrentAgi",
            "move.w d1,d3",
            "andi.w #TURN_AGILITY_MASK,d1",
            "move.w d1,d6",
            "lsr.w #3,d6",
            "jsr (GenerateRandomNumber).w",
            "add.w d7,d1",
            "jsr (GenerateRandomNumber).w",
            "sub.w d7,d1",
            "moveq #3,d6",
            "jsr (GenerateRandomNumber).w",
            "subq.w #1,d7",
            "add.w d7,d1",
            "move.b d0,(a0)+",
        ),
        "turn-order eligibility and randomized AGI",
    )
    return {
        "loadBattle": {
            "address": _FUNCTIONS["LoadBattle"],
            "currentMap": 57,
            "terrain": {"symbol": "BattleTerrain01", "address": _FUNCTIONS["BattleTerrain01"]},
            "orderedSteps": [
                "LoadCurrentMap",
                "FadeOutToBlackAll",
                "LoadMapTilesets",
                "WaitForFadeToFinish",
                "ClearVIntFunctions",
                "WaitForVInt",
                "PositionBattleEntities",
                "InitializeSprites",
                "LoadMap",
                "WaitForVInt",
                "LoadEntityMapsprites",
                "SetBaseVIntFunctions",
                "LoadBattleTerrainData",
                "PlayMapMusic",
                "FadeInFromBlack",
            ],
        },
        "turnOrder": {
            "entryBytes": constants["TURN_ORDER_ENTRY_SIZE"],
            "clearEntries": constants["TURN_ORDER_ENTRIES_COUNTER"] + 1,
            "allyCandidates": [0, constants["COMBATANT_ALLIES_COUNTER"]],
            "enemyCandidates": [
                constants["COMBATANT_ENEMIES_START"],
                constants["COMBATANT_ENEMIES_START"] + constants["BATTLE_ENEMY_ENTITIES_COUNTER"],
            ],
            "eligibility": ["placed", "living"],
            "randomizedAgi": True,
            "sortsDescending": True,
            "currentBattleTurn": 0,
        },
    }


def _structural_schema() -> dict[str, Any]:
    schema = load_json(SCHEMA)
    fixture = schema.get("$defs", {}).get("fixture")
    if not isinstance(fixture, dict):
        raise ValueError("Map 3 Battle 01 admission fixture schema definition is missing")
    return {"$schema": schema["$schema"], "$ref": "#/$defs/fixture", "$defs": schema["$defs"]}


def _validate_structural_output(value: dict[str, Any]) -> None:
    errors = sorted(
        Draft7Validator(_structural_schema()).iter_errors(value),
        key=lambda error: list(error.absolute_path),
    )
    if errors:
        location = ".".join(str(part) for part in errors[0].absolute_path) or "<root>"
        raise ValueError(
            "Map 3 Battle 01 admission structural schema validation failed at "
            f"{location}: {errors[0].message}"
        )


def build_map3_battle01_admission_static(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    """Build the deterministic public H2 admission contract without an H3 observation."""
    if inspect_rom(rom_path.resolve(strict=True))["sha256"] != _ROM_SHA256:
        raise ValueError("Map 3 Battle 01 admission canonical ROM SHA-256 drift")
    upstream = upstream_path.resolve(strict=True)
    revision = subprocess.run(
        ["git", "-C", str(upstream), "rev-parse", "HEAD"],
        check=True,
        capture_output=True,
        text=True,
        encoding="utf-8",
    ).stdout.strip()
    if revision != _UPSTREAM_COMMIT:
        raise ValueError("Map 3 Battle 01 admission upstream revision drift")
    root = _disasm_root(upstream)
    text, source_identities = _read_source_surface(root)
    h1_binary = (upstream / _H1_BINARY).read_bytes()
    rom = rom_path.resolve(strict=True).read_bytes()
    addresses = listing_symbol_addresses((upstream / _LISTING).read_text(encoding="utf-8"))
    if {name: addresses.get(name) for name in _FUNCTIONS} != _FUNCTIONS:
        raise ValueError("Map 3 Battle 01 admission H1 symbol projection drift")
    constants = _constants(root)
    _require_order(
        text["sf2mapmacros.asm"],
        (
            "mWarp: macro",
            "dc.b \\1",
            "dc.b \\2",
            "warpMap: macro",
            "warpDest: macro",
            "warpFacing: macro",
        ),
        "warp macro layout",
    )
    retained = _retained_r2b(rom_path, upstream_path)
    controller = _controller(text["code/common/scripting/entity/entityscriptengine_2.asm"])
    areas = {
        map_id: _parse_areas(text[f"data/maps/entries/map{map_id:02d}/2-areas.asm"], map_id)
        for map_id in (21, 40)
    }
    surfaces = {
        map_id: _surface(root, map_id, areas[map_id], controller, addresses, h1_binary, rom)
        for map_id in (21, 40)
    }
    warps = {
        "map21": _parse_warps(text["data/maps/entries/map21/6-warp-events.asm"], constants, 21),
        "map40": _parse_warps(text["data/maps/entries/map40/6-warp-events.asm"], constants, 40),
    }
    route = _extension_route(
        surfaces,
        warps,
        controller,
        retained,
        text["data/maps/entries/map40/mapsetups/s1_entities.asm"],
    )
    spine = _main_and_battle_spine(constants, text)
    cutscenes = _cutscene_and_definition_spine(constants, text)
    load_and_turn = _load_battle_and_turn_order(constants, text)
    toolchain = load_json(TOOLCHAIN)
    output = {
        "schemaVersion": 1,
        "id": ID,
        "system": ID,
        "romSha256": load_json(ROM_MANIFEST)["hashes"]["sha256"],
        "upstream": {
            "repository": toolchain["sf2disasm"]["repository"],
            "commit": toolchain["sf2disasm"]["commit"],
        },
        "retainedR2b": retained,
        "sourceContext": {
            "sourceIdentities": source_identities,
            "functionAddresses": _FUNCTIONS,
            "h1Projection": _h1_projection(h1_binary, rom),
        },
        "static": {
            "constants": constants,
            "controller": controller,
            "warps": warps,
            "extensionRoute": route,
            "admission": spine,
            "cutscenes": cutscenes,
            "loadAndTurnOrder": load_and_turn,
            "unknownBoundary": [
                "natural-R2a-R2b-R2c-continuity",
                "natural-admission-and-caller-order",
                "natural-initialized-snapshot",
                "natural-first-actor",
                "stable-idle-or-player-ready-state",
                "actual-dialogue-prose-and-chronology",
                "complete-8C-presentation",
            ],
        },
        "summary": {
            "sourceFiles": len(_SOURCE_SURFACE),
            "h1Fields": len(_FUNCTIONS) + len(_TABLE_SPANS),
            "map21WarpRows": len(warps["map21"]),
            "map40WarpRows": len(warps["map40"]),
            "extensionRouteNodes": route["nodeCount"],
            "extensionLogicalInputs": route["inputCount"],
            "battleMapRowIndex": 1,
            "beforeBattleCommands": len(cutscenes["beforeBattle"]["program"]["commands"]),
            "battleSpritesetEntries": cutscenes["spriteset"]["entryCount"],
        },
    }
    _validate_structural_output(output)
    return output


def verify_map3_battle01_admission_static(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    """Validate the checked-in public fixture against fresh H2 source/H1/ROM derivation."""
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner=str(FIXTURE))
    output = build_map3_battle01_admission_static(rom_path, upstream_path)
    if output != fixture:
        raise ValueError("Map 3 Battle 01 admission complete semantic fixture drift")
    return output
