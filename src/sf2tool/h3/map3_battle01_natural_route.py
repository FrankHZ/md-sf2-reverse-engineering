"""Natural Map 3 opening runtime contract with a static Battle 01 route seam.

This rail consumes the accepted controlled Map 3 admission seam, then drives
only original controller input through the reached opening.  It stops at the
original ``cs_5149A`` messenger-program entry before its body.  The later
Map 19/20/21/40/57 to Battle 01 route is source/H1/ROM reconstruction only.
"""

from __future__ import annotations

import json
import re
import shutil
import subprocess
import tempfile
from collections import deque
from hashlib import sha256
from pathlib import Path
from typing import Any

from sf2tool.h2.map_layouts import decode_map_blocks, decode_map_layout
from sf2tool.h3.bizhawk import (
    bizhawk_contract,
    run_observer,
    validate_lua_syntax,
    verify_runtime_contract,
)
from sf2tool.h3.map3_admitted_start import (
    CANONICAL_ROM_SHA256,
    UPSTREAM_COMMIT,
    _equates,
    _h1_bytes,
    _require_order,
    _section,
    build_map3_admitted_start_source_contract,
)
from sf2tool.h3.observer_status import (
    assert_observer_status,
    callback_failure_status,
    observer_failure_contract,
)
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.rom import inspect_rom

OWNER = "map3-battle01-natural-route"
FIXTURE = repo_path("tests/fixtures/h3/map3-battle01-natural-route-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h3/map3-battle01-natural-route-fixture.schema.json")
OBSERVATION_SCHEMA = repo_path("schemas/h3/map3-battle01-natural-route-observation.schema.json")
FAILURE_SCHEMA = repo_path("schemas/h3/map3-battle01-natural-route-callback-failure.schema.json")
OBSERVER = repo_path("tools/bizhawk/map3_battle01_natural_route_observer.lua")
INDEX = repo_path("manifests/research-index.json")
UPSTREAM = repo_path("local/upstream/SF2DISASM")
OBSERVED_OUTPUT = repo_path(f"local/derived/h3/{OWNER}.observed.json")
STATUS_PATH = repo_path(f"local/derived/h3/{OWNER}.status.txt")

FIXTURE_ID = "sf2-map3-battle01-natural-route-runtime-v1"
CASE_IDS = ("natural-map3-opening-to-messenger-entry",)
EXPECTED_CASES = (
    {
        "caseId": "natural-map3-opening-to-messenger-entry",
        "injectedInitialMenuReturn": 1,
        "injectedDifficultyMenuReturn": 0,
        "frameBudget": 36000,
    },
)
R1_FIXTURE_ID = "sf2-map3-admitted-start-runtime-v1"
R1_FIXTURE = repo_path("tests/fixtures/h3/map3-admitted-start-v1.json")

DISASM = Path("disasm")
CONST_SOURCE = Path("sf2const.asm")
ENUM_SOURCE = Path("sf2enums.asm")
LISTING = Path("build/sf2build-h1.lst")

ROUTE_SOURCE_PATHS = (
    Path("sf2const.asm"),
    Path("sf2enums.asm"),
    Path("code/gameflow/mainloop.asm"),
    Path("code/gameflow/exploration/exploration.asm"),
    Path("code/gameflow/exploration/explorationfunctions_0.asm"),
    Path("code/gameflow/exploration/explorationfunctions_2.asm"),
    Path("code/gameflow/exploration/explorationvints.asm"),
    Path("code/common/scripting/entity/entityscriptengine_2.asm"),
    Path("code/common/scripting/map/mapscriptengine_1.asm"),
    Path("code/common/scripting/map/mapsetupsfunctions_1.asm"),
    Path("code/common/stats/gameflags.asm"),
    Path("code/common/maps/mapload.asm"),
    Path("code/common/maps/getbattle.asm"),
    Path("code/gameflow/battle/battleloop_1.asm"),
    Path("code/gameflow/battle/battlefunctions/loadBattle.asm"),
    Path("code/gameflow/battle/battlefunctions/battlefunctions_0.asm"),
    Path("sf2mapsetupmacros.asm"),
    Path("code/gameflow/battle/cutscenes/beforebattlecutscenesstart.asm"),
    Path("code/gameflow/battle/cutscenes/battlestartcutscenesstart.asm"),
    Path("data/battles/global/battlemapcoords.asm"),
    Path("data/battles/entries/battle01/cs_beforebattle.asm"),
    Path("data/maps/entries/map03/mapsetups/s1_entities.asm"),
    Path("data/maps/entries/map03/mapsetups/s2_entityevents.asm"),
    Path("data/maps/entries/map03/mapsetups/s3_zoneevents.asm"),
    Path("data/maps/entries/map03/mapsetups/scripts_1.asm"),
    Path("data/maps/entries/map03/mapsetups/s6_initfunction.asm"),
    Path("data/maps/entries/map03/0-blocks.bin"),
    Path("data/maps/entries/map03/1-layout.bin"),
    Path("data/maps/entries/map03/2-areas.asm"),
    Path("data/maps/entries/map03/3-flag-events.asm"),
    Path("data/maps/entries/map03/4-step-events.asm"),
    Path("data/maps/entries/map03/6-warp-events.asm"),
    Path("data/maps/entries/map19/mapsetups/s6_initfunction.asm"),
    Path("data/maps/entries/map19/0-blocks.bin"),
    Path("data/maps/entries/map19/1-layout.bin"),
    Path("data/maps/entries/map19/2-areas.asm"),
    Path("data/maps/entries/map19/6-warp-events.asm"),
    Path("data/maps/entries/map20/mapsetups/s6_initfunction.asm"),
    Path("data/maps/entries/map20/0-blocks.bin"),
    Path("data/maps/entries/map20/1-layout.bin"),
    Path("data/maps/entries/map20/2-areas.asm"),
    Path("data/maps/entries/map20/6-warp-events.asm"),
    Path("data/maps/entries/map21/mapsetups/s1_entities.asm"),
    Path("data/maps/entries/map21/mapsetups/s2_entityevents_506.asm"),
    Path("data/maps/entries/map21/0-blocks.bin"),
    Path("data/maps/entries/map21/1-layout.bin"),
    Path("data/maps/entries/map21/2-areas.asm"),
    Path("data/maps/entries/map21/6-warp-events.asm"),
    Path("data/maps/entries/map40/0-blocks.bin"),
    Path("data/maps/entries/map40/1-layout.bin"),
    Path("data/maps/entries/map40/2-areas.asm"),
    Path("data/maps/entries/map40/6-warp-events.asm"),
    Path("data/maps/global/mapoffsethashtable.bin"),
)

# This source/H1 inventory combines opening callback targets with static
# source/H1/ROM guards for the unobserved continuation.  Later-route labels do
# not imply callback observation or an H3 absence assertion.
REQUIRED_SYMBOLS = (
    "MainLoop",
    "ExplorationLoop",
    "WaitForEvent",
    "ProcessPlayerAction",
    "GetActivatedEntity",
    "esc02_controlCharacter",
    "loc_52E8",
    "ProcessMapEvent",
    "ProcessMapEventType1_Warp",
    "ProcessMapEventType6_ZoneEvent",
    "RunMapSetupEntityEvent",
    "RunMapSetupZoneEvent",
    "ExecuteMapScript",
    "csc19_setEntityPosAndFacing",
    "GetEntityAddressFromCharacter",
    "CheckBattle",
    "BattleLoop",
    "ExecuteBeforeBattleCutscene",
    "LoadBattle",
    "ExecuteBattleStartCutscene",
    "ActivateEnemies",
    "GenerateBattleTurnOrder",
    "ExecuteIndividualTurn",
    "FieldMenu",
    "OpenDoor",
    "ms_map3_InitFunction",
    "Map3_ZoneEvent0",
    "Map3_EntityEvent0",
    "Map3_EntityEvent15",
    "Map3_ZoneEvent6",
    "Map3_ZoneEvent7",
    "Map3_ZoneEvent8",
    "Map3_ZoneEvent1",
    "cs_513A0",
    "cs_513D6",
    "cs_513E2",
    "cs_51444",
    "cs_51406",
    "cs_5145C",
    "cs_5148C",
    "cs_5149A",
    "cs_51652",
    "cs_53104",
    "ms_map20_InitFunction",
    "return_53994",
    "cs_53996",
    "cs_53B60",
    "Map21_EntityEvent0",
    "cs_53EF4",
    "bbcs_01",
)

REQUIRED_LUA_ROLES = frozenset(
    {
        "bootstrap-check-sram",
        "checkpoint",
        "r1-witch-new-action",
        "r1-new-game",
        "r1-save-game",
        "r1-main-loop",
        "r1-exploration-loop",
        "r1-map-setup-wrapper-entry",
        "r1-setup-resolution-return",
        "r1-init-call",
        "map3-init-dispatch",
        "r1-init-return",
        "r1-wait-for-event",
        "input-controller",
        "input-controller-move-commit",
        "player-action",
        "activated-entity",
        "entity-dispatch",
        "step-door",
        "zone-admission",
        "entity-event",
        "zone-event",
        "map-script",
        "warp",
        "bootstrap-watchdog",
        "case-watchdog",
        "route-phase-watchdog",
        "restoration",
        "callback-cleanup",
    }
)

SUCCESS_MILESTONES = (
    "milestone:observer-started",
    "milestone:r1-scope-snapshotted-before-write",
    "milestone:r1-core-state-saved-outside-callback",
    "milestone:r1-controlled-admission-started",
    "milestone:r1-first-wait-for-event-observed",
    "milestone:natural-route-input-started",
    "milestone:natural-map3-messenger-program-entry-observed",
    "milestone:callbacks-cleared:0",
    "milestone:observer-finished",
)

# This is the bounded, runtime-observed Map 3 opening only.  The labels are
# source symbols or harness seams, never dialogue prose.  The later Castle
# chain remains a static/Inferred route and is intentionally absent here.
EXPECTED_OPENING_CHRONOLOGY = (
    "r1:witch-new-action",
    "r1:new-game",
    "r1:save-game",
    "r1:main-loop",
    "r1:exploration-loop",
    "r1:wait-for-event",
    "map-event:warp:map3-bowie-house-exit",
    "exploration:3",
    "map-init:ms_map3_InitFunction",
    "route:post-warp-wait-for-event",
    "zone-admission:map3-house-exit-zone",
    "zone:Map3_ZoneEvent6",
    "script:cs_5145C",
    "step:map3-bowie-house-door",
    "step:map3-school-door",
    "action:ProcessPlayerAction:map3-sarah-classroom",
    "action:GetActivatedEntity:map3-sarah-classroom",
    "action:RunMapSetupEntityEvent:map3-sarah-classroom",
    "entity:Map3_EntityEvent0",
    "script:cs_513D6",
    "state:Sarah:post-cs_513D6",
    "map-event:warp:map3-school-stairs-down",
    "exploration:3",
    "map-init:ms_map3_InitFunction",
    "route:post-warp-wait-for-event",
    "zone-admission:map3-astral-zone-introduction",
    "zone:Map3_ZoneEvent7",
    "state:Map3:Zone7-pre-entity142",
    "action:ProcessPlayerAction:map3-entity142",
    "action:GetActivatedEntity:map3-entity142",
    "action:RunMapSetupEntityEvent:map3-entity142",
    "entity:Map3_EntityEvent15",
    "zone-admission:map3-astral-zone",
    "zone:Map3_ZoneEvent7",
    "script:cs_5148C",
    "state:Map3:post-cs_5148C",
    "map-event:warp:map3-school-stairs-up",
    "exploration:3",
    "map-init:ms_map3_InitFunction",
    "script:cs_513A0",
    "state:Map3:entity142-reinit-cs_513A0",
    "route:post-warp-wait-for-event",
    "zone-admission:map3-zone-messenger",
    "zone:Map3_ZoneEvent8",
    "script:cs_5149A",
    "endpoint:cs_5149A-entry-before-body",
)
EXPECTED_OPENING_SCRIPT_TRACE = (
    "cs_5145C",
    "cs_513D6",
    "cs_5148C",
    "cs_513A0",
    "cs_5149A",
)


def _assert_input_identity(rom_path: Path, upstream_path: Path) -> None:
    if inspect_rom(rom_path.resolve(strict=True))["sha256"] != CANONICAL_ROM_SHA256:
        raise ValueError("Map 3 natural-route canonical ROM SHA-256 drift")
    revision = subprocess.run(
        ["git", "-C", str(upstream_path.resolve(strict=True)), "rev-parse", "HEAD"],
        check=True,
        capture_output=True,
        text=True,
    ).stdout.strip()
    if revision != UPSTREAM_COMMIT:
        raise ValueError("Map 3 natural-route pinned SF2DISASM revision drift")


def _source_hashes(disasm: Path) -> dict[str, str]:
    return {
        path.as_posix(): sha256((disasm / path).read_bytes()).hexdigest().upper()
        for path in ROUTE_SOURCE_PATHS
    }


def _line_values(source: str, pattern: str, *, name: str) -> tuple[int, ...]:
    matches = re.findall(pattern, source, flags=re.MULTILINE)
    if len(matches) != 1:
        raise ValueError(f"Map 3 natural-route expected one {name}, got {len(matches)}")
    return tuple(int(value) for value in matches[0])


def _script_section(source: str, symbol: str) -> list[tuple[str, str, int]]:
    """Parse a source-form map-script block that has no function-end comment."""
    start = re.search(rf"^{re.escape(symbol)}:\s*", source, re.MULTILINE)
    if start is None:
        raise ValueError(f"Map 3 natural-route missing script label: {symbol}")
    tail = source[start.end() :]
    next_label = re.search(r"^cs_[A-Za-z0-9_]+:\s*", tail, re.MULTILINE)
    body = tail[: next_label.start()] if next_label is not None else tail
    result: list[tuple[str, str, int]] = []
    start_line = source[: start.start()].count("\n") + 1
    for offset, raw in enumerate(body.splitlines(), 1):
        line = raw.split(";", 1)[0].strip()
        if not line or line.endswith(":"):
            continue
        match = re.match(r"(?P<opcode>[A-Za-z][A-Za-z0-9_]*(?:\.[bwls])?)\s*(?P<operand>.*)$", line)
        if match is None:
            raise ValueError(f"Map 3 natural-route cannot parse {symbol} source line: {raw!r}")
        result.append(
            (
                match.group("opcode").lower(),
                re.sub(r"\s+", "", match.group("operand")).lower(),
                start_line + offset,
            )
        )
    return result


def _map3_navigation_plan(
    disasm: Path,
    admitted_start: dict[str, int],
    *,
    sarah_character: int,
    entity_enemy_index_difference: int,
) -> dict[str, Any]:
    """Derive the bounded original-input route from the movement seam.

    This deliberately models no navigation system beyond the exact waypoint
    transitions that R2 reaches.  Each candidate transition stays inside a
    parsed main-layer area; the observer fails closed on the first runtime
    coordinate that does not equal this source/layout-derived plan.
    """
    entity_engine = (disasm / "code/common/scripting/entity/entityscriptengine_2.asm").read_text(
        encoding="utf-8"
    )
    map3_entities = (
        disasm / "data/maps/entries/map03/mapsetups/s1_entities.asm"
    ).read_text(encoding="utf-8")
    if not re.search(
        r"^\s*msFixedEntity\s+42,\s*8,\s*DOWN,\s*ALLY_SARAH,\s*eas_Init\s*$",
        map3_entities,
        re.MULTILINE,
    ):
        raise ValueError("Map 3 natural-route school-path Sarah occupancy source drift")
    if not re.search(
        r"^\s*msFixedEntity\s+44,\s*10,\s*UP,\s*ALLY_CHESTER,\s*eas_Init\s*$",
        map3_entities,
        re.MULTILINE,
    ):
        raise ValueError("Map 3 natural-route school-path Chester occupancy source drift")
    _require_order(
        _section(entity_engine, "esc02_controlCharacter"),
        (
            ("move.b", "((player_1_input-$1000000)).w,currentplayerinput(a6)"),
            ("btst", "#input_bit_up,currentplayerinput(a6)"),
            ("btst", "#input_bit_down,currentplayerinput(a6)"),
            ("btst", "#input_bit_left,currentplayerinput(a6)"),
            ("btst", "#input_bit_right,currentplayerinput(a6)"),
            ("btst", "#0,d6"),
            ("bsr.w", "convertmappixelcoordinatestooffset"),
            ("andi.w", "#$c000,d1"),
            ("btst", "#$f,d1"),
            ("addi.w", "#-$7e,d0"),
            ("cmpi.w", "#2,d6"),
            ("addi.w", "#$7e,d0"),
            ("btst", "#$e,d1"),
            ("addi.w", "#$82,d0"),
            ("cmpi.w", "#2,d6"),
            ("addi.w", "#-$82,d0"),
            ("cmpi.w", "#$c000,(a4,d2.w)"),
            ("bcs.w", "loc_52e8"),
            ("btst", "#6,entitydef_offset_flags_a(a0)"),
        ),
        name="Map 3 natural-route directional movement seam",
    )
    _require_order(
        _section(entity_engine, "esc02_controlCharacter"),
        (
            ("andi.w", "#$3c00,d3"),
            ("cmpi.w", "#$1000,d3"),
            ("bsr.w", "warpifsetatpoint"),
            ("cmpi.w", "#$1400,d3"),
            ("move.w", "#map_event_zone_event,((map_event_type-$1000000)).w"),
            ("cmpi.w", "#$c000,(a4,d2.w)"),
            ("btst", "#6,entitydef_offset_flags_a(a0)"),
        ),
        name="Map 3 natural-route event-before-collision movement seam",
    )
    _require_order(
        _section(entity_engine, "ConvertMapPixelCoordinatesToOffset"),
        (
            ("lsr.w", "#7,d2"),
            ("lsr.w", "#7,d3"),
            ("add.w", "d2,d2"),
            ("move.b", "(a3,d2.w),d2"),
            ("andi.w", "#$3f,d2"),
            ("add.w", "d3,d3"),
            ("move.b", "(a3,d3.w),d3"),
            ("andi.w", "#$3f,d3"),
            ("lsl.w", "#6,d3"),
        ),
        name="Map 3 natural-route map-offset transform",
    )
    expected_areas = {
        3: ((0, 0, 50, 31), (51, 0, 61, 9), (51, 10, 61, 19)),
        19: ((0, 0, 40, 31),),
        20: ((0, 0, 40, 30), (0, 33, 41, 45), (14, 47, 41, 60)),
        21: ((0, 0, 11, 21),),
        40: ((0, 0, 31, 31),),
    }
    if admitted_start != {"map": 3, "x": 56, "y": 3, "facing": 3}:
        raise ValueError("Map 3 natural-route admitted R1 start drift")
    offset_hash = (disasm / "data/maps/global/mapoffsethashtable.bin").read_bytes()
    if len(offset_hash) != 1152:
        raise ValueError("Map 3 natural-route map-offset hash dimension drift")
    maps: dict[int, tuple[tuple[tuple[int, int, int, int], ...], tuple[int, ...]]] = {}
    for map_id, expected in expected_areas.items():
        entry = disasm / f"data/maps/entries/map{map_id:02d}"
        areas = (entry / "2-areas.asm").read_text(encoding="utf-8")
        starts = [
            tuple(map(int, row))
            for row in re.findall(r"mainLayerStart\s+(\d+),\s*(\d+)", areas)
        ]
        ends = [
            tuple(map(int, row))
            for row in re.findall(r"mainLayerEnd\s+(\d+),\s*(\d+)", areas)
        ]
        parsed = tuple(
            (start_x, start_y, end_x, end_y)
            for (start_x, start_y), (end_x, end_y) in zip(starts, ends, strict=True)
        )
        if parsed != expected:
            raise ValueError(f"Map {map_id} natural-route area bounds/order drift")
        blocks = decode_map_blocks((entry / "0-blocks.bin").read_bytes())[0]
        layout = decode_map_layout((entry / "1-layout.bin").read_bytes(), len(blocks) // 9)[0]
        if len(layout) != 64 * 64:
            raise ValueError(f"Map {map_id} natural-route layout corpus dimension drift")
        maps[map_id] = (parsed, layout)
    bounds = {
        "startX": expected_areas[3][0][0],
        "startY": expected_areas[3][0][1],
        "endX": expected_areas[3][0][2],
        "endY": expected_areas[3][0][3],
    }

    def area_for(map_id: int, x: int, y: int) -> tuple[int, int, int, int] | None:
        return next(
            (
                area
                for area in maps[map_id][0]
                if area[0] <= x <= area[2] and area[1] <= y <= area[3]
            ),
            None,
        )

    def layout_cell(map_id: int, x: int, y: int) -> tuple[int, int, int]:
        byte_x, byte_y = x * 6, y * 6
        if byte_x >= len(offset_hash) or byte_y >= len(offset_hash):
            raise ValueError("Map 3 natural-route coordinate exceeds map-offset table")
        hashed_x, hashed_y = offset_hash[byte_x] & 0x3F, offset_hash[byte_y] & 0x3F
        return hashed_x, hashed_y, maps[map_id][1][hashed_y * 64 + hashed_x]

    def layout_flags_at(map_id: int, hashed_x: int, hashed_y: int) -> int | None:
        if not 0 <= hashed_x < 64 or not 0 <= hashed_y < 64:
            return None
        return maps[map_id][1][hashed_y * 64 + hashed_x] & 0xC000

    def transition(
        map_id: int,
        position: tuple[int, int],
        input_name: str,
        *,
        terminal_event_target: tuple[int, int] | None = None,
        collision_guarded: bool,
        blocked_positions: frozenset[tuple[int, int]] = frozenset(),
    ) -> tuple[int, int]:
        x, y = position
        area = area_for(map_id, x, y)
        if area is None:
            raise ValueError(f"Map {map_id} natural-route position is outside all source areas")
        start_x, start_y, end_x, end_y = area
        input_deltas = {
            "Up": (0, -1),
            "Down": (0, 1),
            "Left": (-1, 0),
            "Right": (1, 0),
        }
        if input_name not in input_deltas:
            raise ValueError(f"Map 3 natural-route unsupported input: {input_name}")
        delta_x, delta_y = input_deltas[input_name]
        # esc02_controlCharacter processes all four button bits in source
        # order.  The last active direction is horizontal for a diagonal, then
        # its current-tile $8000/$4000 slope branch may replace Y movement.
        # This is why the first post-warp Right movement changes Y as well:
        # the landing word $5035 has $4000 and its +$82 neighbouring word
        # $5444 has the same collision class, yielding (3,3)->(4,4).
        direction = input_name
        collision_guarded_map3_step = collision_guarded and map_id == 3
        if collision_guarded_map3_step and direction in {"Left", "Right"}:
            hashed_x, hashed_y, layout_word = layout_cell(map_id, x, y)
            flags = layout_word & 0xC000
            slope = None
            if flags & 0x8000:
                slope = (1, -1, -1) if direction == "Right" else (-1, 1, 1)
            elif flags & 0x4000:
                slope = (1, 1, 1) if direction == "Right" else (-1, -1, -1)
            if slope is not None:
                neighbor_x, neighbor_y, slope_y = slope
                if layout_flags_at(map_id, hashed_x + neighbor_x, hashed_y + neighbor_y) == flags:
                    delta_y = slope_y
        output = (x + delta_x, y + delta_y)
        if not start_x <= output[0] <= end_x or not start_y <= output[1] <= end_y:
            return position
        if area_for(map_id, *output) is None:
            return position
        if output in blocked_positions:
            return position
        # The admitted Map 3 player starts with the source-initialized
        # FLAGS_A map-collidable bit.  R2 has runtime-confirmed that exact
        # predicate at its controlled-to-natural handoff.  The later castle
        # floors can change effective movement surfaces through their reached
        # event/cutscene flow, so their input chronology remains callback
        # verified rather than being falsely projected from a base layout.
        if (
            collision_guarded_map3_step
            and output != terminal_event_target
            and layout_cell(map_id, *output)[2] >= 0xC000
        ):
            return position
        return output

    def shortest_input_segment(
        map_id: int,
        start: tuple[int, int],
        target: tuple[int, int],
        waypoint: str,
        *,
        terminal_event_target: bool,
        collision_guarded: bool,
        blocked_positions: frozenset[tuple[int, int]] = frozenset(),
    ) -> list[dict[str, Any]]:
        queue: deque[tuple[int, int]] = deque([start])
        predecessors: dict[tuple[int, int], tuple[tuple[int, int], str] | None] = {start: None}
        while queue:
            current = queue.popleft()
            if current == target:
                break
            inputs = (
                "Up",
                "Down",
                "Left",
                "Right",
            )
            for input_name in inputs:
                destination = transition(
                    map_id,
                    current,
                    input_name,
                    terminal_event_target=target if terminal_event_target else None,
                    collision_guarded=collision_guarded,
                    blocked_positions=blocked_positions,
                )
                if destination != current and destination not in predecessors:
                    predecessors[destination] = (current, input_name)
                    queue.append(destination)
        if target not in predecessors:
            raise ValueError(f"Map 3 natural-route source layout cannot reach {waypoint}")
        reverse_steps: list[tuple[tuple[int, int], tuple[int, int], str]] = []
        current = target
        while predecessors[current] is not None:
            previous, input_name = predecessors[current]  # type: ignore[misc]
            reverse_steps.append((previous, current, input_name))
            current = previous
        return [
            {
                "waypoint": waypoint,
                "from": {"map": map_id, "x": previous[0], "y": previous[1]},
                "to": {"map": map_id, "x": destination[0], "y": destination[1]},
                "input": input_name,
            }
            for previous, destination, input_name in reversed(reverse_steps)
        ]

    no_entity_occupancy = frozenset()
    # Before the first classroom interaction, the source setup places Sarah
    # and Chester in the school corridor.  `Map3_EntityEvent0` executes
    # `cs_513D6`, moving Sarah left then up.  The post-script stair route must
    # retain Chester but must not pretend Sarah is still at her old position.
    school_initial_occupancy = frozenset({(42, 8), (44, 10)})
    school_post_sarah_occupancy = frozenset({(44, 10)})
    school_post_astral_occupancy = frozenset({(44, 10)})
    segments = (
        (
            3,
            (admitted_start["x"], admitted_start["y"]),
            (54, 3),
            "map3-bowie-house-exit",
            True,
            False,
            no_entity_occupancy,
        ),
        (3, (3, 3), (4, 4), "map3-house-exit-zone", True, True, no_entity_occupancy),
        (3, (4, 4), (4, 8), "map3-bowie-house-door", True, True, no_entity_occupancy),
        # The school door is a source-listed step event.  The original first
        # Sarah interaction at the reachable lower neighbour (42,9) runs the
        # program that moves her out of the stair corridor; only then is the
        # post-script cardinal path to 46,7 admissible.
        (3, (4, 8), (41, 13), "map3-school-door", True, True, no_entity_occupancy),
        (3, (41, 13), (42, 9), "map3-sarah-classroom", False, True, school_initial_occupancy),
        (3, (42, 9), (46, 7), "map3-school-stairs-down", True, True, school_post_sarah_occupancy),
        # The 46,7 MAP_CURRENT warp resolves to the lower school at 59,12.
        # Source record 16 is entity 142 (Astral) at 54,17.  Its north/south
        # neighbours are collidable; the decoded lower-school layout admits
        # only the reachable west neighbour (55,17), which faces Left into
        # that entity.  This is a C action, not an attempted movement onto it.
        # ZoneEvent7 is crossed first before Astral's interaction, when it
        # emits its F602-clear dialogue path without a state mutation.  The
        # same source event is crossed again after entity142 and then takes
        # its F602-set cs_5148C/F260 state-mutation path.
        (3, (59, 12), (58, 13), "map3-astral-zone-introduction", True, True, no_entity_occupancy),
        (3, (58, 13), (55, 17), "map3-entity142", True, True, no_entity_occupancy),
        # ZoneEvent7 is mandatory after Astral's interaction. It runs
        # cs_5148C and sets temporary F260 before the same-map stair return.
        (3, (55, 17), (58, 13), "map3-astral-zone", True, True, no_entity_occupancy),
        (3, (58, 13), (59, 12), "map3-school-stairs-up", True, True, no_entity_occupancy),
        # `cs_5148C` has exactly two setPos commands, neither of which moves
        # source record 1 (Chester) from (44,10).  Avoid that occupied tile
        # while still entering ZoneEvent8 at its raw target (43,10).
        (3, (46, 7), (43, 10), "map3-zone-messenger", True, True, school_post_astral_occupancy),
        (3, (43, 10), (42, 9), "map3-sarah", True, False, no_entity_occupancy),
        (3, (42, 9), (27, 5), "map3-castle-gate", True, False, no_entity_occupancy),
        (3, (27, 5), (27, 1), "map3-castle-warp", True, False, no_entity_occupancy),
        (19, (26, 30), (23, 3), "map19-kings-room-warp", True, False, no_entity_occupancy),
        # Map 20's initial program moves the party to (23,39) before its
        # returned WaitForEvent boundary.  The subsequent down-stair is an
        # original warp point, so it is deliberately admitted before the
        # collidable-layout rejection below.
        (20, (23, 39), (23, 37), "map20-kings-room-exit", True, False, no_entity_occupancy),
        (19, (23, 3), (16, 6), "map19-astral", False, False, no_entity_occupancy),
        (19, (16, 6), (6, 2), "map19-west-tower-warp", True, False, no_entity_occupancy),
        (20, (6, 37), (3, 36), "map20-west-tower-warp", True, False, no_entity_occupancy),
        (21, (3, 16), (4, 16), "map21-guard", False, False, no_entity_occupancy),
        (21, (4, 16), (9, 1), "map21-tower-exit", True, False, no_entity_occupancy),
        (40, (4, 30), (4, 12), "map40-entrance", True, False, no_entity_occupancy),
    )
    post_warp_x, post_warp_y, post_warp_word = layout_cell(3, 3, 3)
    slope_destination = transition(3, (3, 3), "Right", collision_guarded=True)
    slope_x, slope_y, slope_word = layout_cell(3, *slope_destination)
    if (
        post_warp_word != 0x5035
        or slope_destination != (4, 4)
        or slope_word != 0x5444
    ):
        raise ValueError("Map 3 natural-route post-warp $5035->$5444 slope guard drift")
    return {
        "area": bounds,
        "layoutWordCount": len(layout),
        "mapOffsetHashBytes": len(offset_hash),
        "mapCollidableFlagBit": 6,
        "blockedLayoutWordFloor": 0xC000,
        "collisionEnforcedMaps": [3],
        "eventBeforeCollision": {
            "mapWordEventMask": 0x3C00,
            "warpSelector": 0x1000,
            "zoneSelector": 0x1400,
            "collisionFloor": 0xC000,
            "order": ["warp", "zone", "collision"],
        },
        "postWarpLanding": {
            "map": 3,
            "x": 3,
            "y": 3,
            "layoutOffsetBytes": (post_warp_y * 64 + post_warp_x) * 2,
            "layoutWord": post_warp_word,
        },
        "postWarpSlope": {
            "input": "Right",
            "sourceOffsetBytes": 0x82,
            "fromLayoutWord": post_warp_word,
            "to": {"map": 3, "x": 4, "y": 4},
            "toLayoutOffsetBytes": (slope_y * 64 + slope_x) * 2,
            "toLayoutWord": slope_word,
        },
        "schoolPathEntityBlocks": [
            {
                "sourceRecord": 0,
                "entity": "ALLY_SARAH",
                "map": 3,
                "x": 42,
                "y": 8,
                "beforeWaypoint": "map3-school-stairs-down",
            },
            {
                "sourceRecord": 1,
                "entity": "ALLY_CHESTER",
                "map": 3,
                "x": 44,
                "y": 10,
                "beforeWaypoint": "map3-school-stairs-down",
            },
        ],
        "schoolSarahProgram": {
            "entityTarget": {"id": 1, "map": 3, "x": 42, "y": 8, "facing": "Down"},
            "event": "Map3_EntityEvent0",
            "program": "cs_513D6",
            "completionFlag": 256,
            "postProgramPosition": {"map": 3, "x": 41, "y": 7},
        },
        "postAstralRouteOccupancy": {
            "sourceRecord": 1,
            "entity": "ALLY_CHESTER",
            "map": 3,
            "x": 44,
            "y": 10,
            "unmovedByProgram": "cs_5148C",
            "beforeWaypoint": "map3-zone-messenger",
        },
        "astralZoneProgram": {
            "event": "Map3_ZoneEvent7",
            "program": "cs_5148C",
            "completionFlag": 260,
            "postProgramPositions": [
                {
                    "rawCharacter": sarah_character,
                    "entityIndexSelector": sarah_character,
                    "map": 3,
                    "x": 41,
                    "y": 10,
                    "facing": "Up",
                },
                {
                    "rawCharacter": 128,
                    "entityIndexSelector": 128 - entity_enemy_index_difference,
                    "map": 3,
                    "x": 6,
                    "y": 4,
                    "facing": "Up",
                },
            ],
        },
        "entity142ReinitProgram": {
            "triggerFlag": 602,
            "precedingFlagClear": 1,
            "program": "cs_513A0",
            "postProgramPosition": {"map": 3, "x": 41, "y": 10, "facing": "Up"},
        },
        "inputPlan": [
            step
            for (
                map_id,
                start,
                target,
                waypoint,
                terminal_event_target,
                collision_guarded,
                blocked_positions,
            ) in segments
            for step in shortest_input_segment(
                map_id,
                start,
                target,
                waypoint,
                terminal_event_target=terminal_event_target,
                collision_guarded=collision_guarded,
                blocked_positions=blocked_positions,
            )
        ],
    }


def _assert_source_route(
    disasm: Path, admitted_start: dict[str, int], ram: dict[str, int] | None = None
) -> dict[str, Any]:
    """Derive the minimum story route from executable source operands.

    This deliberately consumes no source comments or dialogue prose.  Each
    coordinate, flag, map target, and script identity comes from an instruction
    or table record that a smallest-scope source mutation can invalidate.
    """

    def read(relative: Path) -> str:
        return (disasm / relative).read_text(encoding="utf-8")

    ram = _ram_contract(disasm) if ram is None else ram
    navigation = _map3_navigation_plan(
        disasm,
        admitted_start,
        sarah_character=ram["ALLY_SARAH"],
        entity_enemy_index_difference=ram["ENTITY_ENEMY_INDEX_DIFFERENCE"],
    )
    map3_entities = read(Path("data/maps/entries/map03/mapsetups/s1_entities.asm"))
    map3_events = read(Path("data/maps/entries/map03/mapsetups/s2_entityevents.asm"))
    map3_zones = read(Path("data/maps/entries/map03/mapsetups/s3_zoneevents.asm"))
    map3_scripts = read(Path("data/maps/entries/map03/mapsetups/scripts_1.asm"))
    map3_init = read(Path("data/maps/entries/map03/mapsetups/s6_initfunction.asm"))
    map3_steps = read(Path("data/maps/entries/map03/4-step-events.asm"))
    map3_warps = read(Path("data/maps/entries/map03/6-warp-events.asm"))
    exploration = read(Path("code/gameflow/exploration/exploration.asm"))
    exploration_functions = read(Path("code/gameflow/exploration/explorationfunctions_0.asm"))
    exploration_functions_2 = read(Path("code/gameflow/exploration/explorationfunctions_2.asm"))
    exploration_vints = read(Path("code/gameflow/exploration/explorationvints.asm"))
    map_script_engine = read(Path("code/common/scripting/map/mapscriptengine_1.asm"))
    map_event_functions = read(Path("code/common/scripting/map/mapsetupsfunctions_1.asm"))
    game_flags = read(Path("code/common/stats/gameflags.asm"))
    map_setup_macros = read(Path("sf2mapsetupmacros.asm"))
    mapload = read(Path("code/common/maps/mapload.asm"))
    map19_init = read(Path("data/maps/entries/map19/mapsetups/s6_initfunction.asm"))
    map19_warps = read(Path("data/maps/entries/map19/6-warp-events.asm"))
    map20_init = read(Path("data/maps/entries/map20/mapsetups/s6_initfunction.asm"))
    map20_warps = read(Path("data/maps/entries/map20/6-warp-events.asm"))
    map21_entities = read(Path("data/maps/entries/map21/mapsetups/s1_entities.asm"))
    map21_events = read(Path("data/maps/entries/map21/mapsetups/s2_entityevents_506.asm"))
    map21_warps = read(Path("data/maps/entries/map21/6-warp-events.asm"))
    map40_warps = read(Path("data/maps/entries/map40/6-warp-events.asm"))
    battle_maps = read(Path("data/battles/global/battlemapcoords.asm"))
    before_battle = read(Path("data/battles/entries/battle01/cs_beforebattle.asm"))
    battle_functions = read(Path("code/gameflow/battle/battlefunctions/battlefunctions_0.asm"))

    _require_order(
        _section(mapload, "LoadMapBlocksAndLayout"),
        (
            ("lea", "(ff2000_loading_space).l,a1"),
            ("bsr.w", "loadmapblocks"),
            ("lea", "(ff0000_ram_start).l,a1"),
            ("bsr.w", "loadmaplayoutdata"),
        ),
        name="Map 3 natural-route runtime layout-load seam",
    )

    _require_order(
        _section(map3_events, "Map3_EntityEvent15"),
        (("chkflg", "261"), ("setflg", "602"), ("rts", "")),
        name="Map 3 natural-route first entity gate",
    )
    if not re.search(
        r"^\s*msEntityEvent\s+ALLY_SARAH,\s*DOWN,\s*Map3_EntityEvent0-ms_map3_EntityEvents\s*$",
        map3_events,
        re.MULTILINE,
    ):
        raise ValueError("Map 3 natural-route classroom Sarah interaction row drift")
    _require_order(
        _section(map3_events, "Map3_EntityEvent0"),
        (
            ("chkflg", "603"),
            ("chkflg", "602"),
            ("chkflg", "256"),
            ("script", "cs_513d6"),
            ("setflg", "256"),
        ),
        name="Map 3 natural-route classroom Sarah gate",
    )
    _require_order(
        _script_section(map3_scripts, "cs_513D6"),
        (
            ("entityactionswait", "ally_sarah"),
            ("moveleft", "1"),
            ("moveup", "1"),
            ("endactions", ""),
        ),
        name="Map 3 natural-route classroom Sarah movement program",
    )
    _require_order(
        _section(exploration_functions, "GetActivatedEntity"),
        (
            ("move.b", "entitydef_offset_facing(a0,d0.w),d3"),
            ("andi.w", "#direction_mask,d3"),
            ("add.w", "table_pixeloffsets_x(pc,d5.w),d1"),
            ("add.w", "table_pixeloffsets_y(pc,d5.w),d2"),
            ("divs.w", "#map_tile_size,d6"),
            ("divs.w", "#map_tile_size,d7"),
            ("cmpi.w", "#map_tile_size,d5"),
        ),
        name="Map 3 natural-route entity142 adjacent-facing activation seam",
    )
    _require_order(
        _section(exploration_vints, "ProcessPlayerAction"),
        (
            ("btst", "#input_bit_c,d7"),
            ("bsr.w", "getactivatedentity"),
            ("bsr.w", "getentityeventindex"),
            ("jsr", "j_runmapsetupentityevent"),
        ),
        name="Map 3 natural-route entity142 action admission seam",
    )
    _require_order(
        _section(map_event_functions, "RunMapSetupEntityEvent"),
        (
            ("move.b", "d2,((event_relative_position-$1000000)).w"),
            ("move.b", "1(a0,d7.w),d6"),
            ("btst", "#0,d6"),
            ("addi.w", "#2,d2"),
            ("btst", "#1,d6"),
        ),
        name="Map 3 natural-route entity event direction behavior seam",
    )
    if not re.search(
        r"(?ms)^msEntityEvent:\s+macro.*?^\s*dc\.b\s+\\1\b.*?^\s*dc\.b\s+\\2\b.*?^\s*dc\.w\s+\\3\b",
        map_setup_macros,
    ):
        raise ValueError("Map 3 natural-route entity-event table ABI drift")
    pixel_offsets = re.search(
        r"table_PixelOffsets_X:\s*\n\s*dc\.w\s+MAP_TILE_PLUS\s*\n"
        r"table_PixelOffsets_Y:\s*\n\s*dc\.w\s+0\s*\n\s*dc\.w\s+0\s*\n"
        r"\s*dc\.w\s+MAP_TILE_MINUS\s*\n\s*dc\.w\s+MAP_TILE_MINUS\s*\n"
        r"\s*dc\.w\s+0\s*\n\s*dc\.w\s+0\s*\n\s*dc\.w\s+MAP_TILE_PLUS",
        battle_functions,
    )
    if pixel_offsets is None:
        raise ValueError("Map 3 natural-route entity142 facing offset table drift")
    _require_order(
        _section(map_event_functions, "RunMapSetupZoneEvent"),
        (("cmp.b", "(a0,d7.w),d1"), ("cmp.b", "1(a0,d7.w),d2"), ("jsr", "(a0)")),
        name="Map 3 natural-route zone event dispatch seam",
    )
    _require_order(
        _section(exploration_functions_2, "ProcessMapEventType6_ZoneEvent"),
        (
            ("jsr", "j_applyinitactscript"),
            ("move.w", "((map_event_param_1-$1000000)).w,d1"),
            ("move.w", "((map_event_param_3-$1000000)).w,d2"),
            ("jsr", "j_runmapsetupzoneevent"),
        ),
        name="Map 3 natural-route zone raw-coordinate ABI",
    )
    _require_order(
        _section(map3_zones, "Map3_ZoneEvent6"),
        (("chkflg", "601"), ("script", "cs_5145c"), ("setflg", "601"), ("rts", "")),
        name="Map 3 natural-route house-exit interception gate",
    )
    _require_order(
        _section(exploration, "OpenDoor"),
        (
            ("cmpi.b", "#not_currently_in_battle,((current_battle-$1000000)).w"),
            ("lsr.w", "#7,d0"),
            ("lsr.w", "#7,d1"),
            ("movea.l", "mapdata_offset_event_step(a2),a2"),
            ("cmp.b", "(a2),d0"),
            ("cmp.b", "1(a2),d1"),
        ),
        name="Map 3 natural-route step-event dispatch",
    )
    step_rows = [
        (int(x), int(y))
        for x, y in re.findall(r"^\s*sbc\s+(\d+),\s*(\d+)\b", map3_steps, re.MULTILINE)
    ]
    if step_rows != [(4, 8), (12, 12), (19, 12), (32, 15), (38, 24), (41, 13)]:
        raise ValueError("Map 3 natural-route step-event corpus/order drift")
    _require_order(
        _section(map3_zones, "Map3_ZoneEvent8"),
        (("chkflg", "602"), ("chkflg", "603"), ("script", "cs_5149a"), ("setflg", "603")),
        name="Map 3 natural-route messenger zone gate",
    )
    if not re.search(
        r"^\s*msZoneEvent\s+58,\s*13,\s*Map3_ZoneEvent7-ms_map3_ZoneEvents\s*$",
        map3_zones,
        re.MULTILINE,
    ):
        raise ValueError("Map 3 natural-route Astral zone row drift")
    if not re.search(
        r"^\s*msZoneEvent\s+43,\s*10,\s*Map3_ZoneEvent8-ms_map3_ZoneEvents\s*$",
        map3_zones,
        re.MULTILINE,
    ):
        raise ValueError("Map 3 natural-route messenger zone row drift")
    _require_order(
        _section(map3_zones, "Map3_ZoneEvent7"),
        (
            ("chkflg", "603"),
            ("chkflg", "602"),
            ("chkflg", "260"),
            ("script", "cs_5148c"),
            ("setflg", "260"),
            ("rts", ""),
        ),
        name="Map 3 natural-route Astral zone program gate",
    )
    astral_program = _script_section(map3_scripts, "cs_5148C")
    astral_set_positions = [
        operand for opcode, operand, _ in astral_program if opcode == "setpos"
    ]
    if astral_set_positions != ["ally_sarah,41,10,up", "128,6,4,up"]:
        raise ValueError(
            "Map 3 natural-route Astral zone occupancy program set-position corpus drift"
        )
    _require_order(
        astral_program,
        (
            ("setpos", "ally_sarah,41,10,up"),
            ("setpos", "128,6,4,up"),
            ("csc_end", ""),
        ),
        name="Map 3 natural-route Astral zone occupancy program",
    )
    _require_order(
        _section(map3_init, "ms_map3_InitFunction"),
        (
            ("chkflg", "1"),
            ("beq.s", "byte_51390"),
            ("script", "cs_513ba"),
            ("chkflg", "602"),
            ("beq.s", "byte_513a8"),
            ("script", "cs_513a0"),
        ),
        name="Map 3 natural-route entity142 re-init program gate",
    )
    _require_order(
        _section(game_flags, "CheckFlag"),
        (("bsr.w", "getflag"), ("and.b", "(a0),d0")),
        name="Map 3 natural-route flag branch condition ABI",
    )
    _require_order(
        _section(game_flags, "GetFlag"),
        (
            ("andi.l", "#flag_mask,d1"),
            ("divu.w", "#8,d1"),
            ("lea", "((game_flags-$1000000)).w,a0"),
            ("adda.w", "d1,a0"),
            ("swap", "d1"),
            ("moveq", "#$ffffff80,d0"),
            ("lsr.b", "d1,d0"),
        ),
        name="Map 3 natural-route game-flag bit ABI",
    )
    _require_order(
        _script_section(map3_init, "cs_513A0"),
        (("setpos", "ally_sarah,41,10,up"), ("csc_end", "")),
        name="Map 3 natural-route entity142 re-init program effect",
    )
    _require_order(
        _section(map_script_engine, "csc19_setEntityPosAndFacing"),
        (
            ("move.b", "(a6),d0"),
            ("moveq", "#4,d7"),
            ("bsr.w", "adjustscriptpointerbycharacteralivestatus"),
            ("move.b", "(a6)+,d0"),
            ("bsr.w", "getentityaddressfromcharacter"),
            ("move.b", "(a6)+,d0"),
            ("mulu.w", "#map_tile_size,d0"),
            ("move.w", "d0,(a5)"),
            ("move.w", "d0,entitydef_offset_xdest(a5)"),
            ("move.b", "(a6)+,d0"),
            ("mulu.w", "#map_tile_size,d0"),
            ("move.w", "d0,entitydef_offset_y(a5)"),
            ("move.w", "d0,entitydef_offset_ydest(a5)"),
            ("move.b", "(a6)+,entitydef_offset_facing(a5)"),
        ),
        name="Map 3 natural-route set-position command ABI",
    )
    _require_order(
        _section(map_script_engine, "GetEntityAddressFromCharacter"),
        (
            ("lea", "((entity_index_list-$1000000)).w,a5"),
            ("andi.w", "#combatant_mask_all,d0"),
            ("tst.b", "d0"),
            ("bpl.s", "@ally"),
            ("subi.b", "#entity_enemy_index_difference,d0"),
            ("move.b", "(a5,d0.w),d0"),
            ("lsl.w", "#entitydef_size_bits,d0"),
            ("lea", "((entity_data-$1000000)).w,a5"),
            ("adda.w", "d0,a5"),
        ),
        name="Map 3 natural-route cutscene character/entity alias ABI",
    )
    _require_order(
        _script_section(map3_scripts, "cs_513E2"),
        (("yesno", ""), ("jumpifflagset", "89,cs_51406")),
        name="Map 3 natural-route follower prompt polarity",
    )
    _require_order(
        _script_section(map3_scripts, "cs_51406"),
        (("setf", "600"), ("setf", "66"), ("join", "128")),
        name="Map 3 natural-route follower confirmation effects",
    )
    _require_order(
        _section(map3_zones, "Map3_ZoneEvent1"),
        (("chkflg", "600"), ("chkflg", "604"), ("script", "cs_51652"), ("setflg", "604")),
        name="Map 3 natural-route castle gate",
    )
    _require_order(
        _section(map19_init, "ms_map19_InitFunction"),
        (("chkflg", "605"), ("script", "cs_53104"), ("chkflg", "608")),
        name="Castle 2F natural-route map-entry program",
    )
    _require_order(
        _script_section(map19_init, "cs_53104"),
        (("setpos", "140,63,63,left"), ("csc_end", "")),
        name="Castle 2F natural-route map-entry program effect",
    )
    _require_order(
        _section(map20_init, "ms_map20_InitFunction"),
        (
            ("cmpi.l", "#$22803780,((entity_data-$1000000)).w"),
            ("chkflg", "605"),
            ("script", "cs_53996"),
            ("setflg", "605"),
            ("script", "cs_53b60"),
            ("rts", ""),
        ),
        name="Castle 1F natural-route setup effect",
    )
    _require_order(
        _script_section(map20_init, "cs_53996"),
        (
            ("textcursor", "2176"),
            ("setpos", "ally_bowie,23,39,down"),
            ("setpos", "ally_sarah,23,38,down"),
            ("setpos", "ally_chester,23,37,down"),
            ("followentity", "ally_sarah,ally_bowie,2"),
            ("followentity", "ally_chester,ally_sarah,2"),
        ),
        name="Castle 1F natural-route palace scene setup and party result",
    )
    _require_order(
        _script_section(map20_init, "cs_53B60"),
        (("hide", "130"), ("csc_end", "")),
        name="Castle 1F natural-route palace scene terminal",
    )
    _require_order(
        _section(map21_events, "Map21_EntityEvent0"),
        (("chkflg", "608"), ("script", "cs_53ef4")),
        name="Castle 3F natural-route gate",
    )
    _require_order(
        _script_section(map21_events, "cs_53EF4"),
        (("setstoryflag", "1"), ("csc_end", "")),
        name="Battle 01 unlock effect",
    )
    battle_rows = [
        re.sub(r"\s+", "", line.split(";", 1)[0]).lower()
        for line in battle_maps.splitlines()
        if line.strip() and not line.lstrip().startswith(";")
    ]
    expected_battle_rows = [
        "battlemapcoordinates63,0,12,32,36,255,255",
        "battlemapcoordinates57,0,0,16,20,255,255",
    ]
    if [row for row in battle_rows if row.startswith("battlemapcoordinates")][
        :2
    ] != expected_battle_rows:
        raise ValueError("Battle 01 map trigger row order/value drift")
    before_rows = [
        re.sub(r"\s+", "", line.split(";", 1)[0]).lower()
        for line in before_battle.splitlines()
        if line.strip() and not line.lstrip().startswith(";")
    ]
    if before_rows[:3] != [
        "bbcs_01:textcursor2292",
        "loadmapfadeinmap_ancient_tower_entrance,2,10",
        "loadmapentitiesce_49694",
    ]:
        raise ValueError("Battle 01 before-cutscene first operations drift")

    # The source entity records retain two follower entries before the regular
    # map entities.  The rejected 140 dispatch proved that record 14 is the
    # guard at (1,23); record 16 is the actual source placement that the
    # `msEntityEvent 142,DOWN` row selects.  The H3 callback below observes
    # 142 at the original dispatch seam, so neither source ordering nor a
    # comment is being substituted for the runtime entity index.
    map3_entity_rows = re.findall(
        r"^\s*(ms(?:Fixed|Walking)Entity)\s+([^\n;]+)", map3_entities, re.MULTILINE
    )
    if len(map3_entity_rows) != 19:
        raise ValueError("Map 3 entity inventory count drift")
    row_140 = [part.strip() for part in map3_entity_rows[14][1].split(",")]
    if row_140[:4] != ["1", "23", "RIGHT", "MAPSPRITE_GUARD"]:
        raise ValueError("Map 3 source-order entity 140 guard row drift")
    row_142 = [part.strip() for part in map3_entity_rows[16][1].split(",")]
    if row_142[:4] != ["54", "17", "UP", "MAPSPRITE_ASTRAL"]:
        raise ValueError("Map 3 source-order entity 142 Astral row drift")
    if not re.search(
        r"^\s*msEntityEvent\s+142,\s*DOWN,\s*Map3_EntityEvent15-ms_map3_EntityEvents\s*$",
        map3_events,
        re.MULTILINE,
    ):
        raise ValueError("Map 3 source-order entity 142 interaction row drift")
    guard_rows = re.findall(r"^\s*msFixedEntity\s+([^\n;]+)", map21_entities, re.MULTILINE)
    if guard_rows != ["5, 16, DOWN, MAPSPRITE_GUARD, eas_Init"]:
        raise ValueError("Map 21 gate-guard entity row drift")

    def warp(
        source: str,
        source_map: int,
        destination_map: int,
        destination_symbol: str,
        x: int,
        y: int,
        destination_x: int,
        destination_y: int,
    ) -> dict[str, int]:
        pattern = (
            rf"mWarp\s+{x},\s*{y}\s*(?:;[^\n]*)?\n\s*warpNoScroll\s*\n"
            rf"\s*warpMap\s+{destination_symbol}\s*\n\s*warpDest\s+{destination_x},\s*{destination_y}"
        )
        if len(re.findall(pattern, source, re.MULTILINE)) != 1:
            raise ValueError(
                f"natural-route warp {source_map}->{destination_map} source/destination drift"
            )
        return {
            "fromMap": source_map,
            "toMap": destination_map,
            # `MAP_CURRENT` remains the raw event operand (0xFF) until
            # UpdatePlayerPosFromMapEvent resolves it at the original warp
            # handler.  The observer therefore validates both raw operand and
            # later resolved destination, rather than conflating the two.
            "eventDestinationMap": 255 if destination_symbol == "MAP_CURRENT" else destination_map,
            "x": x,
            "y": y,
            "destinationX": destination_x,
            "destinationY": destination_y,
        }

    return {
        "flags": {
            "afterHouseExit": 601,
            "afterEntity142": 602,
            "afterAstralZone": 260,
            "afterMessenger": 603,
            "followersAccepted": 600,
            "followers": 66,
            "castleAdmitted": 604,
            "castleScene": 605,
            "towerAccepted": 608,
            "battle01Unlocked": 401,
        },
        "maps": [3, 19, 20, 21, 40, 57],
        "battle01": {
            "id": 1,
            "map": 57,
            "areaX": 0,
            "areaY": 0,
            "areaWidth": 16,
            "areaHeight": 20,
            "triggerX": 255,
            "triggerY": 255,
            "beforeCutscene": "bbcs_01",
        },
        "navigation": navigation,
        "waypoints": [
            {
                "id": "map3-bowie-house-exit",
                "map": 3,
                "x": 54,
                "y": 3,
                "facing": "None",
                "interaction": "warp",
                "completionDestination": {"map": 3, "x": 3, "y": 3},
            },
            {
                "id": "map3-house-exit-zone",
                "map": 3,
                "x": 4,
                "y": 4,
                "facing": "None",
                "interaction": "zone",
                "completionFlag": 601,
            },
            {
                "id": "map3-bowie-house-door",
                "map": 3,
                "x": 4,
                "y": 8,
                "facing": "None",
                "interaction": "step",
            },
            {
                "id": "map3-school-door",
                "map": 3,
                "x": 41,
                "y": 13,
                "facing": "None",
                "interaction": "step",
            },
            {
                "id": "map3-sarah-classroom",
                "map": 3,
                "x": 42,
                "y": 9,
                "facing": "Up",
                "interaction": "entity",
                "entityTarget": {"id": 1, "map": 3, "x": 42, "y": 8, "facing": "Down"},
                "completionFlag": 256,
            },
            {
                "id": "map3-school-stairs-down",
                "map": 3,
                "x": 46,
                "y": 7,
                "facing": "None",
                "interaction": "warp",
                "completionDestination": {"map": 3, "x": 59, "y": 12},
            },
            {
                "id": "map3-astral-zone-introduction",
                "map": 3,
                "x": 58,
                "y": 13,
                "facing": "None",
                "interaction": "zone",
                "completionEvent": "Map3_ZoneEvent7",
            },
            {
                "id": "map3-entity142",
                "map": 3,
                "x": 55,
                "y": 17,
                "facing": "Left",
                "interaction": "entity",
                "entityTarget": {"id": 142, "map": 3, "x": 54, "y": 17, "facing": "Up"},
                "completionFlag": 602,
            },
            {
                "id": "map3-astral-zone",
                "map": 3,
                "x": 58,
                "y": 13,
                "facing": "None",
                "interaction": "zone",
                "completionFlag": 260,
            },
            {
                "id": "map3-school-stairs-up",
                "map": 3,
                "x": 59,
                "y": 12,
                "facing": "None",
                "interaction": "warp",
                "completionDestination": {"map": 3, "x": 46, "y": 7},
            },
            {
                "id": "map3-zone-messenger",
                "map": 3,
                "x": 43,
                "y": 10,
                "facing": "None",
                "interaction": "zone",
                "completionFlag": 603,
            },
            {
                "id": "map3-sarah",
                "map": 3,
                "x": 42,
                "y": 9,
                "facing": "Up",
                "interaction": "entity",
                "completionFlag": 600,
            },
            {
                "id": "map3-castle-gate",
                "map": 3,
                "x": 27,
                "y": 5,
                "facing": "None",
                "interaction": "zone",
                "completionFlag": 604,
            },
            {
                "id": "map3-castle-warp",
                "map": 3,
                "x": 27,
                "y": 1,
                "facing": "None",
                "interaction": "warp",
                "completionDestination": {"map": 19, "x": 26, "y": 30},
            },
            {
                "id": "map19-kings-room-warp",
                "map": 19,
                "x": 23,
                "y": 3,
                "facing": "None",
                "interaction": "warp",
                "completionDestination": {"map": 20, "x": 23, "y": 37},
            },
            {
                "id": "map20-palace-scene",
                "map": 20,
                "x": 23,
                "y": 39,
                "facing": "None",
                "interaction": "scene",
                "completionFlag": 605,
            },
            {
                "id": "map20-kings-room-exit",
                "map": 20,
                "x": 23,
                "y": 37,
                "facing": "None",
                "interaction": "warp",
                "completionDestination": {"map": 19, "x": 23, "y": 3},
            },
            {
                "id": "map19-astral",
                "map": 19,
                "x": 16,
                "y": 6,
                "facing": "Up",
                "interaction": "entity",
                "completionFlag": 608,
            },
            {
                "id": "map19-west-tower-warp",
                "map": 19,
                "x": 6,
                "y": 2,
                "facing": "None",
                "interaction": "warp",
                "completionDestination": {"map": 20, "x": 6, "y": 37},
            },
            {
                "id": "map20-west-tower-warp",
                "map": 20,
                "x": 3,
                "y": 36,
                "facing": "None",
                "interaction": "warp",
                "completionDestination": {"map": 21, "x": 3, "y": 16},
            },
            {
                "id": "map21-guard",
                "map": 21,
                "x": 4,
                "y": 16,
                "facing": "Right",
                "interaction": "entity",
                "completionFlag": 401,
            },
            {
                "id": "map21-tower-exit",
                "map": 21,
                "x": 9,
                "y": 1,
                "facing": "None",
                "interaction": "warp",
                "completionDestination": {"map": 40, "x": 4, "y": 30},
            },
            {
                "id": "map40-entrance",
                "map": 40,
                "x": 4,
                "y": 12,
                "facing": "None",
                "interaction": "warp",
                "completionDestination": {"map": 57, "x": 8, "y": 18},
            },
        ],
        "warps": [
            warp(map3_warps, 3, 3, "MAP_CURRENT", 54, 3, 3, 3),
            warp(map3_warps, 3, 3, "MAP_CURRENT", 46, 7, 59, 12),
            warp(map3_warps, 3, 3, "MAP_CURRENT", 59, 12, 46, 7),
            warp(map3_warps, 3, 19, "MAP_GRANSEAL_CASTLE_2F", 255, 1, 26, 30),
            warp(map19_warps, 19, 20, "MAP_GRANSEAL_CASTLE_1F", 23, 3, 23, 37),
            warp(map20_warps, 20, 19, "MAP_GRANSEAL_CASTLE_2F", 23, 37, 23, 3),
            warp(map19_warps, 19, 20, "MAP_GRANSEAL_CASTLE_1F", 6, 2, 6, 37),
            warp(map20_warps, 20, 21, "MAP_GRANSEAL_CASTLE_3F", 3, 36, 3, 16),
            warp(map21_warps, 21, 40, "MAP_ANCIENT_TOWER_EXTERIOR", 9, 1, 4, 30),
            warp(map40_warps, 40, 57, "MAP_ANCIENT_TOWER_ENTRANCE", 255, 12, 8, 18),
        ],
        "scriptSymbols": [
            "cs_513A0",
            "cs_513D6",
            "cs_5145C",
            "cs_5148C",
            "cs_5149A",
            "cs_513E2",
            "cs_51406",
            "cs_51652",
            "cs_53104",
            "cs_53996",
            "cs_53B60",
            "cs_53EF4",
            "bbcs_01",
        ],
    }


def _ram_contract(disasm: Path) -> dict[str, int]:
    constants = _equates(
        (disasm / CONST_SOURCE).read_text(encoding="utf-8"),
        (
            "CURRENT_MAP",
            "CURRENT_BATTLE",
            "GAME_FLAGS",
            "ENTITY_DATA",
            "ENTITY_INDEX_LIST",
            "CURRENT_GOLD",
            "COMBATANT_DATA",
            "MAP_EVENT_TYPE",
            "VIEW_TARGET_ENTITY",
            "MAP_EVENT_PARAM_1",
            "MAP_EVENT_PARAM_2",
            "MAP_EVENT_PARAM_3",
            "MAP_EVENT_PARAM_4",
            "BATTLE_AREA_X",
            "BATTLE_AREA_Y",
            "BATTLE_AREA_WIDTH",
            "BATTLE_AREA_HEIGHT",
            "CURRENT_BATTLE_TURN",
            "BATTLE_TURN_ORDER",
            "PLAYER_1_INPUT",
            "CURRENT_PLAYER_INPUT",
            "FF0000_RAM_START",
            "MAP_AREA_LAYER2_STARTX",
            "MAP_AREA_LAYER2_STARTY",
            "MAP_AREA_LAYER_TYPE",
            "MAP_AREA_LAYER1_STARTX",
            "MAP_AREA_LAYER1_STARTY",
            "MAP_AREA_LAYER1_ENDX",
            "MAP_AREA_LAYER1_ENDY",
        ),
    )
    enums = _equates(
        (disasm / ENUM_SOURCE).read_text(encoding="utf-8"),
        (
            "MAP_TILE_SIZE",
            "ENTITYDEF_OFFSET_X",
            "ENTITYDEF_OFFSET_Y",
            "ENTITYDEF_OFFSET_XDEST",
            "ENTITYDEF_OFFSET_YDEST",
            "ENTITYDEF_OFFSET_FACING",
            "ENTITYDEF_OFFSET_LAYER",
            "ENTITYDEF_OFFSET_FLAGS_A",
            "ENTITYDEF_SIZE",
            "COMBATANT_DATA_ENTRY_SIZE",
            "COMBATANT_ALLIES_COUNTER",
            "LONGWORD_GAMEFLAGS_COUNTER",
            "COMBATANT_MASK_ALL",
            "ENTITY_ENEMY_INDEX_DIFFERENCE",
            "ALLY_SARAH",
            "INPUT_BIT_UP",
            "INPUT_BIT_DOWN",
            "INPUT_BIT_LEFT",
            "INPUT_BIT_RIGHT",
            "INPUT_BIT_C",
            "DIRECTION_MASK",
            "RIGHT",
            "UP",
            "LEFT",
            "DOWN",
        ),
    )
    if (
        enums["MAP_TILE_SIZE"] != 384
        or enums["ENTITYDEF_OFFSET_XDEST"] != 12
        or enums["ENTITYDEF_OFFSET_YDEST"] != 14
        or enums["ENTITYDEF_OFFSET_LAYER"] != 17
        or enums["ENTITYDEF_OFFSET_FLAGS_A"] != 28
        or enums["ENTITYDEF_SIZE"] != 32
        or enums["COMBATANT_DATA_ENTRY_SIZE"] != 56
        or enums["COMBATANT_MASK_ALL"] != 0xFF
        or enums["ENTITY_ENEMY_INDEX_DIFFERENCE"] != 0x60
        or enums["ALLY_SARAH"] != 1
        or tuple(
            enums[key]
            for key in (
                "INPUT_BIT_UP",
                "INPUT_BIT_DOWN",
                "INPUT_BIT_LEFT",
                "INPUT_BIT_RIGHT",
                "INPUT_BIT_C",
            )
        )
        != (0, 1, 2, 3, 5)
        or enums["DIRECTION_MASK"] != 3
        or tuple(enums[key] for key in ("RIGHT", "UP", "LEFT", "DOWN")) != (0, 1, 2, 3)
    ):
        raise ValueError("Map 3 natural-route source RAM stride drift")
    return {**constants, **enums}


def build_map3_battle01_natural_route_source_contract(
    rom_path: Path, upstream_path: Path
) -> dict[str, Any]:
    """Derive the R2 route source/H1/ROM contract before fixture comparison."""
    _assert_input_identity(rom_path, upstream_path)
    upstream = upstream_path.resolve(strict=True)
    disasm = upstream / DISASM
    listing = (upstream / LISTING).read_text(encoding="utf-8")
    addresses = listing_symbol_addresses(listing)
    missing = sorted(symbol for symbol in REQUIRED_SYMBOLS if symbol not in addresses)
    if missing:
        raise ValueError(f"Map 3 natural-route H1 symbols missing: {missing}")
    r1 = build_map3_admitted_start_source_contract(rom_path, upstream)
    if r1["function"]["selectedInitAddress"] != addresses["ms_map3_InitFunction"]:
        raise ValueError("Map 3 natural-route selected-init/map3-init physical-PC alias drift")
    admitted_start = {
        "map": r1["witchNewAction"]["gameStartMap"],
        "x": r1["witchNewAction"]["gameStartSavepointX"],
        "y": r1["witchNewAction"]["gameStartSavepointY"],
        "facing": r1["witchNewAction"]["gameStartFacing"],
    }
    ram = _ram_contract(disasm)
    route = _assert_source_route(disasm, admitted_start, ram)
    opening_waypoint_ids = (
        "map3-bowie-house-exit",
        "map3-house-exit-zone",
        "map3-bowie-house-door",
        "map3-school-door",
        "map3-sarah-classroom",
        "map3-school-stairs-down",
        "map3-astral-zone-introduction",
        "map3-entity142",
        "map3-astral-zone",
        "map3-school-stairs-up",
        "map3-zone-messenger",
    )
    opening_waypoints = [
        waypoint for waypoint in route["waypoints"] if waypoint["id"] in opening_waypoint_ids
    ]
    if tuple(waypoint["id"] for waypoint in opening_waypoints) != opening_waypoint_ids:
        raise ValueError("Map 3 opening runtime waypoint order drift")
    opening_navigation = {
        **route["navigation"],
        "inputPlan": [
            step
            for step in route["navigation"]["inputPlan"]
            if step["waypoint"] in opening_waypoint_ids
        ],
    }
    messenger_steps = [
        step
        for step in opening_navigation["inputPlan"]
        if step["waypoint"] == "map3-zone-messenger"
    ]
    if not messenger_steps or messenger_steps[-1]["to"] != {"map": 3, "x": 43, "y": 10}:
        raise ValueError("Map 3 natural-route messenger input/zone target join drift")
    route["runtimeOpening"] = {
        "flags": {
            "afterHouseExit": route["flags"]["afterHouseExit"],
            "classroomSarah": 256,
            "afterEntity142": route["flags"]["afterEntity142"],
            "afterAstralZone": route["flags"]["afterAstralZone"],
            "afterMessenger": route["flags"]["afterMessenger"],
        },
        "navigation": opening_navigation,
        "waypoints": opening_waypoints,
        "warps": route["warps"][:3],
        "scriptSymbols": ["cs_513A0", "cs_513D6", "cs_5145C", "cs_5148C", "cs_5149A"],
        "endpoint": {
            "sourceTarget": messenger_steps[-1]["to"],
            "program": "cs_5149A",
            "notYetMutatedFlag": route["flags"]["afterMessenger"],
        },
    }
    mainloop = (disasm / "code/gameflow/mainloop.asm").read_text(encoding="utf-8")
    battleloop = (disasm / "code/gameflow/battle/battleloop_1.asm").read_text(encoding="utf-8")
    _require_order(
        _section(mainloop, "MainLoop"),
        (
            ("bsr.w", "checkbattle"),
            ("cmpi.w", "#-1,d7"),
            ("jsr", "j_battleloop"),
            ("jsr", "j_explorationloop"),
        ),
        name="natural battle admission main loop",
    )
    _require_order(
        _section(battleloop, "BattleLoop"),
        (
            ("jsr", "j_executebeforebattlecutscene"),
            ("bsr.w", "loadbattle"),
            ("jsr", "j_executebattlestartcutscene"),
            ("bsr.w", "activateenemies"),
            ("bsr.w", "generatebattleturnorder"),
            ("bsr.w", "executeindividualturn"),
        ),
        name="Battle 01 pre-turn lifecycle",
    )
    rom = rom_path.resolve(strict=True).read_bytes()
    for symbol in (
        "MainLoop",
        "CheckBattle",
        "BattleLoop",
        "ExecuteIndividualTurn",
        "ProcessPlayerAction",
        "GetActivatedEntity",
        "Map3_ZoneEvent0",
        "Map3_EntityEvent0",
        "Map3_EntityEvent15",
        "ms_map3_InitFunction",
        "cs_513A0",
        "cs_513D6",
        "csc19_setEntityPosAndFacing",
        "GetEntityAddressFromCharacter",
        "cs_51444",
        "ms_map20_InitFunction",
        "return_53994",
        "cs_53996",
        "cs_53B60",
    ):
        address = addresses[symbol]
        if _h1_bytes(listing, address, 2) != rom[address : address + 2].hex().upper():
            raise ValueError(f"Map 3 natural-route H1/ROM entry opcode drift: {symbol}")
    return {
        "r1": {
            "fixtureId": R1_FIXTURE_ID,
            "romSha256": CANONICAL_ROM_SHA256,
            "selectedMap": r1["map3"]["mapIndex"],
            "selectedSetup": r1["map3"]["defaultSetupSymbol"],
            "selectedInit": r1["map3"]["selectedInitSymbol"],
            "admittedStart": admitted_start,
            "functions": r1["function"],
            "harness": r1["harness"],
            "sessionPatches": r1["sessionPatches"],
        },
        "functions": {symbol: addresses[symbol] for symbol in REQUIRED_SYMBOLS},
        "ram": ram,
        "route": route,
        "sourceHashes": _source_hashes(disasm),
    }


def _fixture_static_projection(contract: dict[str, Any]) -> dict[str, Any]:
    source_hash_digest = (
        sha256(
            json.dumps(contract["sourceHashes"], sort_keys=True, separators=(",", ":")).encode(
                "utf-8"
            )
        )
        .hexdigest()
        .upper()
    )
    return {
        "r1": {
            "fixtureId": contract["r1"]["fixtureId"],
            "selectedMap": contract["r1"]["selectedMap"],
            "selectedSetup": contract["r1"]["selectedSetup"],
            "selectedInit": contract["r1"]["selectedInit"],
            "admittedStart": contract["r1"]["admittedStart"],
        },
        "functions": contract["functions"],
        "ram": contract["ram"],
        "route": contract["route"],
        "sourceHashDigest": source_hash_digest,
    }


def _fixture_source_context(contract: dict[str, Any]) -> dict[str, Any]:
    return {
        "r1FixtureId": R1_FIXTURE_ID,
        "functions": {
            key: contract["functions"][key]
            for key in (
                "MainLoop",
                "WaitForEvent",
                "CheckBattle",
                "BattleLoop",
                "ExecuteIndividualTurn",
            )
        },
        "battle01": {
            "map": contract["route"]["battle01"]["map"],
            "id": contract["route"]["battle01"]["id"],
        },
    }


def _assert_lua_role_contract() -> None:
    source = OBSERVER.read_text(encoding="utf-8")
    missing = sorted(role for role in REQUIRED_LUA_ROLES if f'"{role}"' not in source)
    if missing:
        raise ValueError(f"Map 3 natural-route Lua callback role contract drift: {missing}")
    if source.count("add_callback(config.r1.functions.selectedInitAddress") != 1:
        raise ValueError("Map 3 natural-route selected-init physical-PC dispatch count drift")
    if "add_callback(config.functions.ms_map3_InitFunction" in source:
        raise ValueError("Map 3 natural-route duplicate map3-init physical-PC callback")


def _expected_logical_input_trace(contract: dict[str, Any]) -> list[dict[str, Any]]:
    """Project original Map 3 controller edges without emulator frame timing.

    The source/layout model owns the coordinate and direction of every move.
    The two entity C edges are added only after their source-derived adjacent
    coordinate/facing transition.  Idle/release frames are harness scheduling
    detail, so they must not become a golden fact.
    """
    plan = contract["route"]["runtimeOpening"]["navigation"]["inputPlan"]
    result: list[dict[str, Any]] = []
    interaction_edges = {
        "map3-sarah-classroom": (),
        "map3-entity142": ("Left",),
    }
    for index, step in enumerate(plan):
        source = step["from"]
        result.append(
            {
                "map": source["map"],
                "x": source["x"],
                "y": source["y"],
                "input": step["input"],
                "waypoint": step["waypoint"],
            }
        )
        next_waypoint = plan[index + 1]["waypoint"] if index + 1 < len(plan) else None
        if step["waypoint"] not in interaction_edges or next_waypoint == step["waypoint"]:
            continue
        target = step["to"]
        for input_name in (*interaction_edges[step["waypoint"]], "C"):
            result.append(
                {
                    "map": target["map"],
                    "x": target["x"],
                    "y": target["y"],
                    "input": input_name,
                    "waypoint": step["waypoint"]
                    + ("-face" if input_name != "C" else ""),
                }
            )
    return result


def _expected_runtime_observation(contract: dict[str, Any]) -> dict[str, Any]:
    """Return the source/H1/ROM-validated opening Map 3 golden matrix."""
    endpoint = contract["route"]["runtimeOpening"]["endpoint"]
    admitted = contract["r1"]["admittedStart"]
    if tuple(contract["route"]["runtimeOpening"]["scriptSymbols"]) != (
        "cs_513A0",
        "cs_513D6",
        "cs_5145C",
        "cs_5148C",
        "cs_5149A",
    ):
        raise ValueError("Map 3 natural-route opening script inventory drift")
    if endpoint["sourceTarget"] != {"map": 3, "x": 43, "y": 10}:
        raise ValueError("Map 3 natural-route messenger source-target golden drift")
    if endpoint["program"] != "cs_5149A" or endpoint["notYetMutatedFlag"] != 603:
        raise ValueError("Map 3 natural-route endpoint golden drift")
    if admitted != {"map": 3, "x": 56, "y": 3, "facing": 3}:
        raise ValueError("Map 3 natural-route R1 opening golden drift")
    return {
        "system": FIXTURE_ID,
        "caseOrder": list(CASE_IDS),
        "records": [
            {
                "caseId": CASE_IDS[0],
                "r1Start": {"fixtureId": R1_FIXTURE_ID, "map": admitted["map"]},
                "chronology": list(EXPECTED_OPENING_CHRONOLOGY),
                "logicalInputTrace": _expected_logical_input_trace(contract),
                # These values are the original route's observed pre-warp
                # and post-Wait checkpoints.  They are intentionally a
                # bounded opening trace, not a claim about Castle/Battle 01.
                "mapTransitions": [
                    admitted,
                    {"map": 3, "x": 55, "y": 3, "facing": 2},
                    {"map": 3, "x": 45, "y": 7, "facing": 0},
                    {"map": 3, "x": 58, "y": 13, "facing": 0},
                ],
                "scriptTrace": list(EXPECTED_OPENING_SCRIPT_TRACE),
                "fieldMenu": "not-reached",
                "openingMap3": {
                    "sourceTarget": endpoint["sourceTarget"],
                    "program": endpoint["program"],
                    "afterHouseExit": True,
                    "classroomSarah": True,
                    "afterEntity142": True,
                    "afterAstralZone": True,
                    "afterMessenger": False,
                },
            }
        ],
        "callbacksCleared": True,
        "restoration": {
            "gameFlags": True,
            "combatantAllyRecords": True,
            "mapAndBattleState": True,
            "playerEntity": True,
            "gold": True,
            "generatedRam": True,
            "callbacksCleared": True,
            "sessionCartPatches": True,
            "sessionRomDeleted": True,
        },
    }


def _first_matrix_difference(expected: Any, actual: Any, path: str = "$") -> str | None:
    """Return one compact public metadata mismatch without retaining output."""
    if type(expected) is not type(actual):
        return f"{path}: expected type {type(expected).__name__}, actual {type(actual).__name__}"
    if isinstance(expected, dict):
        if expected.keys() != actual.keys():
            return f"{path}: expected keys {sorted(expected)!r}, actual keys {sorted(actual)!r}"
        for key in expected:
            difference = _first_matrix_difference(expected[key], actual[key], f"{path}.{key}")
            if difference is not None:
                return difference
        return None
    if isinstance(expected, list):
        if len(expected) != len(actual):
            return f"{path}: expected {len(expected)} items, actual {len(actual)}"
        for index, (expected_item, actual_item) in enumerate(zip(expected, actual, strict=True)):
            difference = _first_matrix_difference(expected_item, actual_item, f"{path}[{index}]")
            if difference is not None:
                return difference
        return None
    if expected != actual:
        return f"{path}: expected {expected!r}, actual {actual!r}"
    return None


def _assert_runtime_matrix(value: dict[str, Any], contract: dict[str, Any], *, owner: str) -> None:
    expected = _expected_runtime_observation(contract)
    difference = _first_matrix_difference(expected, value)
    if difference is not None:
        raise ValueError(f"Map 3 natural-route {owner} matrix/golden drift: {difference}")


def _assert_fixture(fixture: dict[str, Any], contract: dict[str, Any]) -> None:
    if fixture["id"] != FIXTURE_ID or fixture["system"] != FIXTURE_ID:
        raise ValueError("Map 3 natural-route fixture identity drift")
    if fixture["caseOrder"] != list(CASE_IDS) or [
        case["caseId"] for case in fixture["cases"]
    ] != list(CASE_IDS):
        raise ValueError("Map 3 natural-route exact case order/ID drift")
    if tuple(fixture["cases"]) != EXPECTED_CASES:
        raise ValueError("Map 3 natural-route exact input matrix drift")
    if fixture["static"] != _fixture_static_projection(contract):
        raise ValueError("Map 3 natural-route fixture static projection drift")
    if fixture["sourceContext"] != _fixture_source_context(contract):
        raise ValueError("Map 3 natural-route fixture source context drift")
    # The observer's warp callback compares the original event parameters with
    # the route waypoint completion destination.  Keep that destination tied to
    # the source-parsed warp rows instead of allowing the fixture/model to
    # agree on a separately hand-entered target.
    for waypoint in contract["route"]["waypoints"]:
        if waypoint["interaction"] != "warp":
            continue
        candidates = [
            warp
            for warp in contract["route"]["warps"]
            if warp["fromMap"] == waypoint["map"]
            and warp["y"] == waypoint["y"]
            and (warp["x"] == waypoint["x"] or warp["x"] == 255)
        ]
        if len(candidates) != 1:
            raise ValueError(
                "Map 3 natural-route source warp waypoint join drift: "
                f"{waypoint['id']} matched {len(candidates)} rows"
            )
        warp = candidates[0]
        expected_destination = {
            "map": warp["toMap"],
            "x": warp["destinationX"],
            "y": warp["destinationY"],
        }
        if waypoint.get("completionDestination") != expected_destination:
            raise ValueError(
                "Map 3 natural-route source warp completion destination drift: "
                f"{waypoint['id']}"
            )
    if (
        fixture["privateProvenance"]["romSha256"] != CANONICAL_ROM_SHA256
        or fixture["privateProvenance"]["upstreamCommit"] != UPSTREAM_COMMIT
    ):
        raise ValueError("Map 3 natural-route private input provenance drift")
    _assert_runtime_matrix(fixture["expectedObservation"], contract, owner="fixture expected")


def _observer_config(fixture: dict[str, Any], contract: dict[str, Any]) -> dict[str, Any]:
    harness = contract["r1"]["harness"]
    marker_address = harness["checkpointAddress"] + harness["generatedRamBytes"] - 1
    if marker_address <= harness["menuThunkAddress"] + 13:
        raise ValueError("Map 3 natural-route automation marker overlaps generated menu thunk")
    return {
        "fixtureId": fixture["id"],
        "core": fixture["emulator"]["core"],
        "caseOrder": fixture["caseOrder"],
        "cases": fixture["cases"],
        "functions": {
            key: contract["functions"][key]
            for key in (
                "WaitForEvent",
                "ProcessPlayerAction",
                "GetActivatedEntity",
                "esc02_controlCharacter",
                "loc_52E8",
                "ProcessMapEvent",
                "ProcessMapEventType1_Warp",
                "ProcessMapEventType6_ZoneEvent",
                "RunMapSetupEntityEvent",
                "RunMapSetupZoneEvent",
                "ExecuteMapScript",
                "OpenDoor",
                "Map3_ZoneEvent0",
                "Map3_EntityEvent0",
                "Map3_EntityEvent15",
                "Map3_ZoneEvent6",
                "Map3_ZoneEvent7",
                "Map3_ZoneEvent8",
                "cs_513A0",
                "cs_513D6",
                "cs_5145C",
                "cs_5148C",
                "cs_5149A",
            )
        },
        "ram": contract["ram"],
        "route": contract["route"]["runtimeOpening"],
        "r1": contract["r1"],
        "automation": {"markerAddress": marker_address},
        "observerFailureContract": observer_failure_contract(OWNER),
    }


def _assert_clean_observer_config(config: dict[str, Any]) -> None:
    forbidden = {
        "expectedObservation",
        "acceptedObservation",
        "records",
        "chronology",
        "restoration",
        "golden",
    }

    def visit(value: Any) -> None:
        if isinstance(value, dict):
            overlap = forbidden & set(value)
            if overlap:
                raise ValueError(
                    "Map 3 natural-route observer config contains accepted output: "
                    f"{sorted(overlap)!r}"
                )
            for child in value.values():
                visit(child)
        elif isinstance(value, list):
            for child in value:
                visit(child)

    visit(config)


def _assert_status() -> None:
    assert_observer_status(
        STATUS_PATH, owner=OWNER, schema_path=FAILURE_SCHEMA, required_milestones=SUCCESS_MILESTONES
    )
    if tuple(STATUS_PATH.read_text(encoding="utf-8").splitlines()) != SUCCESS_MILESTONES:
        raise RuntimeError("Map 3 natural-route success status sequence drift")


def _failure_diagnostic() -> dict[str, Any] | None:
    payload = callback_failure_status(STATUS_PATH, owner=OWNER, schema_path=FAILURE_SCHEMA)
    if payload is None:
        return None
    if payload["caseId"] not in {"bootstrap", *CASE_IDS}:
        raise ValueError("Map 3 natural-route failure case identity drift")
    if (
        payload["callbackCount"] != 0
        or not payload["callbacksCleared"]
        or not payload["outputRemoved"]
    ):
        raise ValueError("Map 3 natural-route failure cleanup contract drift")
    restoration = payload["restoration"]
    if (
        restoration["callbacksCleared"] != payload["callbacksCleared"]
        or restoration["outputRemoved"] != payload["outputRemoved"]
    ):
        raise ValueError("Map 3 natural-route failure restoration cleanup facts drift")
    if restoration["sessionStateRestored"] != (payload["restorationMismatch"] is None):
        raise ValueError("Map 3 natural-route restoration mismatch consistency drift")
    return payload


def preflight_map3_battle01_natural_route(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner="Map 3 Battle 01 natural-route fixture")
    _assert_lua_role_contract()
    _, executable = bizhawk_contract()
    validate_lua_syntax(OBSERVER, executable)
    contract = build_map3_battle01_natural_route_source_contract(rom_path, upstream_path)
    _assert_fixture(fixture, contract)
    _assert_clean_observer_config(_observer_config(fixture, contract))
    return {
        "Fixture": fixture["id"],
        "Cases": len(CASE_IDS),
        "Maps": len(contract["route"]["maps"]),
        "Status": "PRELAUNCH-PASS",
    }


def verify_map3_battle01_natural_route(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 300
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner="Map 3 Battle 01 natural-route fixture")
    verify_runtime_contract(fixture, rom_path)
    contract = build_map3_battle01_natural_route_source_contract(rom_path, upstream_path)
    _assert_fixture(fixture, contract)
    _assert_lua_role_contract()
    config = _observer_config(fixture, contract)
    _assert_clean_observer_config(config)
    canonical_before = inspect_rom(rom_path.resolve(strict=True))["sha256"]
    session_deleted = False
    try:
        with tempfile.TemporaryDirectory(prefix="sf2-map3-battle01-natural-route-") as temporary:
            session = Path(temporary) / "map3-battle01-natural-route-session.bin"
            shutil.copy2(rom_path, session)
            observed = run_observer(
                rom_path=session,
                observer_path=OBSERVER,
                config=config,
                output_name=OWNER,
                timeout_seconds=timeout_seconds,
            )
            _assert_status()
        session_deleted = not session.exists()
        observed["restoration"]["sessionRomDeleted"] = session_deleted
        validate_json(
            observed, OBSERVATION_SCHEMA, owner="Map 3 Battle 01 natural-route observation"
        )
        _assert_runtime_matrix(observed, contract, owner="runtime observed")
        if observed != fixture["expectedObservation"]:
            raise ValueError(
                "Map 3 Battle 01 natural-route runtime matrix mismatch\n"
                f"expected={fixture['expectedObservation']!r}\nobserved={observed!r}"
            )
        OBSERVED_OUTPUT.write_text(json.dumps(observed, indent=2) + "\n", encoding="utf-8")
    except Exception:
        OBSERVED_OUTPUT.unlink(missing_ok=True)
        _failure_diagnostic()
        raise
    if inspect_rom(rom_path.resolve(strict=True))["sha256"] != canonical_before:
        raise ValueError("Map 3 natural-route canonical ROM changed during session run")
    return {
        "Fixture": fixture["id"],
        "Cases": len(CASE_IDS),
        "BizHawkLaunches": 1,
        "Maps": len(contract["route"]["maps"]),
        "SessionRomDeleted": session_deleted,
        "Status": "PASS",
    }
