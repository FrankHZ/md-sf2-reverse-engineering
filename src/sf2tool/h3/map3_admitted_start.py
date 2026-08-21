"""Map 3 controlled-admission runtime contract.

This rail starts at the original Witch/New handoff, then observes the original
MainLoop and Map 3 setup path until the first ``WaitForEvent`` boundary.  It
does not describe a natural route through Map 3.
"""

from __future__ import annotations

import json
import re
import shutil
import subprocess
import tempfile
from hashlib import sha256
from pathlib import Path
from typing import Any

from sf2tool.h2.map_init import build_map_init_contract
from sf2tool.h2.map_setup import build_map_setup_contract
from sf2tool.h3.bizhawk import (
    bizhawk_contract,
    run_observer,
    validate_lua_syntax,
    verify_runtime_contract,
)
from sf2tool.h3.observer_status import (
    assert_observer_status,
    callback_failure_status,
    observer_failure_contract,
)
from sf2tool.h3.witch_new_game_lifecycle import build_witch_new_game_lifecycle_source_contract
from sf2tool.h3.witch_save_actions import _equates, _require_order, _section
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.rom import inspect_rom

OWNER = "map3-admitted-start"
FIXTURE = repo_path("tests/fixtures/h3/map3-admitted-start-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h3/map3-admitted-start-fixture.schema.json")
OBSERVATION_SCHEMA = repo_path("schemas/h3/map3-admitted-start-observation.schema.json")
FAILURE_SCHEMA = repo_path("schemas/h3/map3-admitted-start-callback-failure.schema.json")
OBSERVER = repo_path("tools/bizhawk/map3_admitted_start_observer.lua")
INDEX = repo_path("manifests/research-index.json")
UPSTREAM = repo_path("local/upstream/SF2DISASM")
OBSERVED_OUTPUT = repo_path(f"local/derived/h3/{OWNER}.observed.json")
STATUS_PATH = repo_path(f"local/derived/h3/{OWNER}.status.txt")
CANONICAL_ROM_SHA256 = "9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9"
UPSTREAM_COMMIT = "c834c652b6862bc5679fd7f69a38a7093206efc6"
CASE_IDS = ("controlled-new-map3-default",)
REQUIRED_LUA_ROLES = frozenset(
    {
        "bootstrap-check-sram",
        "checkpoint",
        "witch-new-action",
        "new-game",
        "save-game",
        "main-loop",
        "exploration-loop",
        "map-setup-wrapper-entry",
        "setup-resolution-return",
        "init-call",
        "selected-init",
        "unexpected-script",
        "unexpected-init-effect",
        "init-return",
        "wait-for-event",
        "bootstrap-watchdog",
        "case-watchdog",
    }
)

SUCCESS_MILESTONES = (
    "milestone:observer-started",
    "milestone:bootstrap-new-game-before-admission",
    "milestone:scope-snapshotted-before-write",
    "milestone:core-state-saved-outside-callback",
    "milestone:controlled-new-admission-started",
    "milestone:original-witch-new-entered",
    "milestone:original-new-game-entered",
    "milestone:original-save-game-entered",
    "milestone:original-main-loop-entered",
    "milestone:original-exploration-loop-entered",
    "milestone:original-map-setup-wrapper-entered",
    "milestone:setup-resolution-return-observed",
    "milestone:original-init-call-observed",
    "milestone:original-selected-init-entered",
    "milestone:original-selected-init-return-observed",
    "milestone:raw-vint-time-read-at-wait",
    "milestone:first-wait-for-event-observed",
    "milestone:vint-time-normalized-outside-callback",
    "milestone:callbacks-cleared:0",
    "milestone:observer-finished",
)

EXPECTED_CHRONOLOGY = (
    "check-sram",
    "witch-new-action",
    "new-game",
    "save-game",
    "main-loop",
    "exploration-loop",
    "map-setup-wrapper-entry",
    "setup-resolution-return",
    "init-call",
    "selected-init",
    "init-return",
    "wait-for-event",
)

INDEX_BINDINGS = {
    "screens.witch.new-game-lifecycle": {
        ("entry", "sourceContext.function.newActionAddress"),
        ("check-sram", "sourceContext.function.checkSramAddress"),
        ("new-game-effective-target", "sourceContext.function.newGameAddress"),
        ("save-game", "sourceContext.function.saveGameAddress"),
        ("main-loop", "sourceContext.function.mainLoopAddress"),
    },
    "stats.new-game": {("entry", "sourceContext.function.newGameAddress")},
    "tech.services.sram-actions": {
        ("check-sram", "sourceContext.function.checkSramAddress"),
        ("save-game", "sourceContext.function.saveGameAddress"),
    },
    "gameflow.main-loop": {("entry", "sourceContext.function.mainLoopAddress")},
    "gameflow.exploration.loop": {
        ("entry", "sourceContext.function.explorationLoopAddress"),
        ("wait-for-event", "sourceContext.function.waitForEventAddress"),
    },
    "scripting.map.mapsetupsfunctions-1": {
        ("entry", "sourceContext.function.runMapSetupInitFunctionAddress"),
        ("call", "sourceContext.function.initCallAddress"),
        ("return", "sourceContext.function.initReturnAddress"),
        ("current-map", "sourceContext.ram.CURRENT_MAP"),
        ("game-flags", "sourceContext.ram.GAME_FLAGS"),
    },
    "map.data.ms-map3-initfunction": {
        ("entry", "sourceContext.map3.selectedInitAddress")
    },
}

MAIN_LOOP_SOURCE = Path("code/gameflow/mainloop.asm")
EXPLORATION_SOURCE = Path("code/gameflow/exploration/explorationfunctions_2.asm")
MAP_SETUP_FUNCTIONS_SOURCE = Path("code/common/scripting/map/mapsetupsfunctions_1.asm")
NEW_GAME_SOURCE = Path("code/common/stats/newgame.asm")
VINT_SOURCE = Path("code/common/tech/interrupts/vint.asm")
TIMER_WINDOW_SOURCE = Path("code/common/menus/timerwindow.asm")
RNG_SOURCE = Path("code/common/tech/randomnumbergenerator.asm")
CONST_SOURCE = Path("sf2const.asm")
ENUM_SOURCE = Path("sf2enums.asm")

MAP3_SOURCE_PREFIX = "data/maps/entries/map03/"
MAP3_SELECTED_POINTER = "ms_map3"
MAP3_SELECTED_INIT = "ms_map3_InitFunction"
MAP3_VARIANTS = (
    (609, "ms_map3_flag609"),
    (506, "ms_map3_flag506"),
    (543, "ms_map3_flag543"),
)
MAP3_DEFAULT_GUARD_FLAGS = (1, 602, 603, 506, 543, 609)
VINT_TIME_NORMALIZATION = "post-boundary-controlled-zeroed-vint-counters"


def _relative_bsr_return(
    rom: bytes, *, entry: int, target: int, scan_bytes: int
) -> int:
    """Derive one BSR.W return seam from its encoded target, not a fixed offset."""
    matches: list[int] = []
    for address in range(entry, entry + scan_bytes, 2):
        opcode = int.from_bytes(rom[address : address + 2], "big")
        if opcode != 0x6100:
            continue
        displacement = int.from_bytes(rom[address + 2 : address + 4], "big", signed=True)
        if address + 2 + displacement == target:
            matches.append(address + 4)
    if len(matches) != 1:
        raise ValueError(
            "Map 3 admitted-start source/H1/ROM BSR target must resolve once: "
            f"entry={entry:#x}, target={target:#x}, matches={matches!r}"
        )
    return matches[0]


def _indirect_jsr_seams(rom: bytes, *, entry: int) -> tuple[int, int]:
    """Derive the one ``jsr (a0)`` call/return pair from the accepted ROM."""
    matches = [
        address
        for address in range(entry, entry + 48, 2)
        if rom[address : address + 2] == b"\x4e\x90"
    ]
    if len(matches) != 1:
        raise ValueError(
            "Map 3 admitted-start source/H1/ROM indirect init call must resolve once: "
            f"entry={entry:#x}, matches={matches!r}"
        )
    return matches[0], matches[0] + 2


def _absolute_jsr_target(rom: bytes, *, entry: int, target: int, scan_bytes: int) -> int:
    """Derive one original absolute JSR use, preserving its encoded target."""
    matches = [
        address
        for address in range(entry, entry + scan_bytes, 2)
        if rom[address : address + 2] == b"\x4e\xb9"
        and int.from_bytes(rom[address + 2 : address + 6], "big") == target
    ]
    if len(matches) != 1:
        raise ValueError(
            "Map 3 admitted-start source/H1/ROM absolute JSR target must resolve once: "
            f"entry={entry:#x}, target={target:#x}, matches={matches!r}"
        )
    return matches[0]


def _h1_bytes(listing: str, address: int, width: int) -> str:
    """Read a byte-exact span from the pinned H1 listing."""
    cells: dict[int, int] = {}
    for line in listing.splitlines():
        match = re.match(r"^([0-9A-F]{8})\s+((?:[0-9A-F]{4}\s+)+)", line)
        if match is None:
            continue
        start = int(match.group(1), 16)
        for offset, value in enumerate(bytes.fromhex(re.sub(r"\s+", "", match.group(2)))):
            cells[start + offset] = value
    if any(cell not in cells for cell in range(address, address + width)):
        raise ValueError(f"Map 3 admitted-start H1 span is incomplete at {address:#x}")
    return bytes(cells[cell] for cell in range(address, address + width)).hex().upper()


def _assert_input_identity(rom_path: Path, upstream_path: Path) -> None:
    if inspect_rom(rom_path.resolve(strict=True))["sha256"] != CANONICAL_ROM_SHA256:
        raise ValueError("Map 3 admitted-start canonical ROM SHA-256 drift")
    revision = subprocess.run(
        ["git", "-C", str(upstream_path.resolve(strict=True)), "rev-parse", "HEAD"],
        check=True,
        capture_output=True,
        text=True,
    ).stdout.strip()
    if revision != UPSTREAM_COMMIT:
        raise ValueError("Map 3 admitted-start pinned SF2DISASM revision drift")


def _session_patches(listing: str, rom: bytes, witch: dict[str, Any]) -> list[dict[str, Any]]:
    """Pin the three small, session-only Witch presentation seams to H1 and ROM."""
    scratch = witch["ram"]["workRamScratchAddress"]
    menu_thunk = scratch + 0x20
    specs = (
        (
            witch["function"]["menuInstructionTargetAddress"],
            6,
            "controlled-witch-menu-alias",
            "4EFA65AE4EFA",
            (b"\x4e\xf9" + menu_thunk.to_bytes(4, "big")).hex().upper(),
            witch["function"]["menuEffectiveTargetAddress"],
        ),
        (
            witch["function"]["nameAllyInstructionTargetAddress"],
            2,
            "name-ally-rts",
            "4EFA",
            "4E75",
            witch["function"]["nameAllyEffectiveTargetAddress"],
        ),
        (witch["function"]["displayTextAddress"], 2, "display-text-rts", "48E7", "4E75", None),
    )
    patches: list[dict[str, Any]] = []
    spans: list[range] = []
    for address, width, purpose, original, replacement, target in specs:
        h1 = _h1_bytes(listing, address, min(width, 4))
        if h1[:4] != original[:4]:
            raise ValueError(f"Map 3 admitted-start H1 session opcode drift: {purpose}")
        actual = rom[address : address + width].hex().upper()
        if actual != original:
            raise ValueError(f"Map 3 admitted-start H1/ROM session span drift: {purpose}")
        if target is not None:
            effective_target = address + 2 + int.from_bytes(
                rom[address + 2 : address + 4], "big", signed=True
            )
            if effective_target != target:
                raise ValueError(f"Map 3 admitted-start H1/ROM alias target drift: {purpose}")
        if len(replacement) != width * 2:
            raise ValueError(f"Map 3 admitted-start session patch width drift: {purpose}")
        span = range(address, address + width)
        if any(set(span) & set(previous) for previous in spans):
            raise ValueError(f"Map 3 admitted-start session patch overlap: {purpose}")
        spans.append(span)
        patches.append(
            {
                "address": address,
                "width": width,
                "originalHex": original,
                "hex": replacement,
                "purpose": purpose,
            }
        )
    return patches


def _main_loop_use_sites(source: str) -> dict[str, list[dict[str, Any]]]:
    return {
        "mainLoop": _require_order(
            _section(source, "MainLoop"),
            (
                ("clr.b", "((deactivate_window_hiding-$1000000)).w"),
                ("bsr.w", "switchmap"),
                ("bsr.w", "checkbattle"),
                ("cmpi.w", "#-1,d7"),
                ("beq.w", "@exploration"),
                ("move.w", "d7,d1"),
                ("jsr", "j_battleloop"),
                ("bsr.w", "switchmap"),
                ("jsr", "j_explorationloop"),
                ("bra.s", "@start"),
            ),
            name="MainLoop admitted exploration route",
        )
    }


def _exploration_use_sites(source: str) -> dict[str, list[dict[str, Any]]]:
    return {
        "admittedEntry": _require_order(
            _section(source, "ExplorationLoop"),
            (
                ("clr.w", "((map_event_type-$1000000)).w"),
                ("jsr", "heallivingandimmortalallies"),
                ("jsr", "fadeouttoblackall(pc)"),
                ("move.b", "#-1,((view_target_entity-$1000000)).w"),
                ("jsr", "j_getmapsetupentities"),
                ("jsr", "j_initializemapentities"),
                ("bsr.w", "clearmapsetuptempflags"),
                ("setflg", "80"),
                ("jsr", "(loadmap).w"),
                ("bsr.w", "setbasevintfunctions"),
                ("jsr", "j_runmapsetupinitfunction"),
                ("jsr", "(playmapmusic).w"),
                ("jsr", "(fadeinfromblack).w"),
                ("clr.w", "d0"),
                ("bsr.w", "setmovesfx"),
                ("bsr.w", "waitforevent"),
            ),
            name="ExplorationLoop admitted Map 3 prefix",
        )
    }


def _new_game_use_sites(source: str) -> dict[str, list[dict[str, Any]]]:
    return {
        "newGame": _require_order(
            _section(source, "NewGame"),
            (
                ("bsr.w", "initializegamesettings"),
                ("bsr.w", "initializeallycombatantentry"),
                ("dbf", "d7,@loop"),
                ("moveq", "#gamestart_gold,d1"),
                ("bsr.w", "setgold"),
                ("moveq", "#ally_bowie,d0"),
                ("bsr.w", "joinforce"),
                ("rts", ""),
            ),
            name="NewGame admitted-state initialization",
        ),
        "settings": _require_order(
            _section(source, "InitializeGameSettings"),
            (
                ("moveq", "#longword_gameflags_initvalue,d0"),
                ("lea", "((game_flags-$1000000)).w,a0"),
                ("move.l", "d0,(a0)+"),
                ("dbf", "d7,@cleargameflags_loop"),
                ("move.w", "d0,((current_gold-$1000000)).w"),
                ("move.b", "d0,((current_map-$1000000)).w"),
                ("move.b", "d0,((egress_map-$1000000)).w"),
                ("move.b", "#2,((message_speed-$1000000)).w"),
                ("rts", ""),
            ),
            name="InitializeGameSettings admission reset",
        ),
    }


def _map3_init_use_sites(source: str) -> dict[str, list[dict[str, Any]]]:
    return {
        "map3Init": _require_order(
            _section(source, MAP3_SELECTED_INIT),
            (
                ("chkflg", "1"),
                ("script", "cs_513ba"),
                ("chkflg", "602"),
                ("script", "cs_513a0"),
                ("chkflg", "603"),
                ("move.w", "#142,d0"),
                ("jsr", "moveentityoutofmap"),
                ("rts", ""),
            ),
            name="Map 3 selected init guarded scripts and local effect",
        )
    }


def _map3_index_records() -> list[dict[str, str]]:
    index = load_json(INDEX)
    records = [
        {"id": row["id"], "sourcePath": row["sourcePath"]}
        for row in index["records"]
        if row["sourcePath"].startswith(MAP3_SOURCE_PREFIX)
    ]
    if len(records) != 26:
        raise ValueError(f"Map 3 admitted-start requires 26 indexed records, got {len(records)}")
    if len({row["id"] for row in records}) != len(records):
        raise ValueError("Map 3 admitted-start index corpus has duplicate record IDs")
    if any(not row["sourcePath"].endswith(".asm") for row in records):
        raise ValueError("Map 3 admitted-start index corpus includes a non-ASM record")
    return records


def _map3_route_contract(
    setup: dict[str, Any], init: dict[str, Any], addresses: dict[str, int], rom: bytes
) -> dict[str, Any]:
    route = next((row for row in setup["routes"] if row["map"] == 3), None)
    if route is None:
        raise ValueError("Map 3 is absent from the H2 map-setup route contract")
    variants = tuple((row["flag"], row["pointer"]) for row in route["flagVariants"])
    if route["defaultPointer"] != MAP3_SELECTED_POINTER or variants != MAP3_VARIANTS:
        raise ValueError(
            "Map 3 setup default/flag route order drift: "
            f"default={route['defaultPointer']!r}, variants={variants!r}"
        )
    tables = {row["symbol"]: row for row in setup["pointerTables"]}
    selected = tables.get(MAP3_SELECTED_POINTER)
    if selected is None:
        raise ValueError("Map 3 default setup table is absent from H2 pointer tables")
    selected_init = selected["targets"].get("initFunction")
    if selected_init is None or selected_init["symbol"] != MAP3_SELECTED_INIT:
        raise ValueError("Map 3 default setup table no longer targets its selected init function")
    if selected_init["address"] != addresses[MAP3_SELECTED_INIT]:
        raise ValueError("Map 3 selected init H1 address disagrees with map-setup contract")
    source_files = {row["symbol"]: row for row in init["sourceFiles"]}
    selected_profile = source_files.get(MAP3_SELECTED_INIT)
    if selected_profile is None:
        raise ValueError("Map 3 selected init function is absent from H2 map-init source profiles")
    if selected_profile["directReturnStub"]:
        raise ValueError("Map 3 default init function unexpectedly became a direct-return stub")
    dispatcher = init["dispatcher"]
    entry = addresses["RunMapSetupInitFunction"]
    get_current = addresses["GetCurrentMapSetup"]
    setup_resolution_return = _relative_bsr_return(
        rom, entry=entry, target=get_current, scan_bytes=24
    )
    return {
        "mapIndex": 3,
        "defaultSetupSymbol": MAP3_SELECTED_POINTER,
        "defaultSetupAddress": selected["address"],
        "flagVariants": [
            {"flag": flag, "setupSymbol": symbol, "setupAddress": tables[symbol]["address"]}
            for flag, symbol in variants
        ],
        "selectedInitSymbol": MAP3_SELECTED_INIT,
        "selectedInitAddress": selected_init["address"],
        "selectedInitDirectReturnStub": selected_profile["directReturnStub"],
        "setupResolutionReturnAddress": setup_resolution_return,
        "dispatcher": dispatcher,
        "selectedInitSourcePath": selected_profile["path"],
        "selectedInitStatementCount": selected_profile["statementCount"],
        "selectedInitScriptTargets": selected_profile["scriptTargets"],
    }


def _ram_contract(disasm: Path) -> dict[str, int]:
    constants = _equates(
        (disasm / CONST_SOURCE).read_text(encoding="utf-8"),
        (
            "COMBATANT_DATA",
            "CURRENT_GOLD",
            "CURRENT_MAP",
            "CURRENT_BATTLE",
            "CURRENT_SAVE_SLOT",
            "EGRESS_MAP",
            "ENTITY_DATA",
            "ENTITY_INDEX_LIST",
            "FRAME_COUNTER",
            "GAME_FLAGS",
            "MAP_EVENT_TYPE",
            "RANDOM_SEED",
            "SECONDS_COUNTER",
            "SECONDS_COUNTER_FRAMES",
            "PLAYER_1_INPUT",
        ),
    )
    enums = _equates(
        (disasm / ENUM_SOURCE).read_text(encoding="utf-8"),
        (
            "COMBATANT_DATA_ENTRY_SIZE",
            "COMBATANT_ALLIES_COUNTER",
            "COMBATANT_OFFSET_AGI_CURRENT",
            "COMBATANT_OFFSET_ATT_CURRENT",
            "COMBATANT_OFFSET_CLASS",
            "COMBATANT_OFFSET_DEF_CURRENT",
            "COMBATANT_OFFSET_HP_CURRENT",
            "COMBATANT_OFFSET_HP_MAX",
            "COMBATANT_OFFSET_ITEM_0",
            "COMBATANT_OFFSET_LEVEL",
            "COMBATANT_OFFSET_MOV_CURRENT",
            "COMBATANT_OFFSET_MP_CURRENT",
            "COMBATANT_OFFSET_MP_MAX",
            "COMBATANT_OFFSET_SPELLS",
            "ENTITYDEF_OFFSET_FACING",
            "ENTITYDEF_OFFSET_X",
            "ENTITYDEF_OFFSET_Y",
            "ENTITYDEF_SIZE",
            "FLAG_INDEX_DIFFICULTY1",
            "FLAG_INDEX_DIFFICULTY2",
            "FORCEMEMBER_ACTIVE_FLAGS_START",
            "FORCEMEMBER_JOINED_FLAGS_START",
            "LONGWORD_GAMEFLAGS_COUNTER",
        ),
    )
    if (
        enums["COMBATANT_DATA_ENTRY_SIZE"] != 56
        or enums["ENTITYDEF_SIZE"] != 32
        or enums["COMBATANT_ALLIES_COUNTER"] != 29
        or enums["LONGWORD_GAMEFLAGS_COUNTER"] != 31
    ):
        raise ValueError("Map 3 admitted-start state-entry stride drift")
    return {**constants, **enums}


def _unique_h1_rom_instruction(
    listing: str,
    rom: bytes,
    *,
    entry: int,
    scan_bytes: int,
    instruction: bytes,
    purpose: str,
) -> int:
    """Find one source-backed instruction and require H1/ROM byte agreement."""
    matches = [
        address
        for address in range(entry, entry + scan_bytes, 2)
        if rom[address : address + len(instruction)] == instruction
    ]
    if len(matches) != 1:
        raise ValueError(
            "Map 3 admitted-start time-state H1/ROM instruction drift: "
            f"{purpose} matches={matches!r}"
        )
    address = matches[0]
    h1 = _h1_bytes(listing, address, len(instruction))
    if h1 != instruction.hex().upper():
        raise ValueError(f"Map 3 admitted-start time-state H1 bytes drift: {purpose}")
    return address


def _time_state_source_use_sites(vint: str, timer_window: str, rng: str) -> dict[str, Any]:
    """Guard source operand widths before recording the narrow state spans."""
    return {
        "frameCounter": _require_order(
            _section(vint, "VInt"),
            (
                ("addq.b", "#1,((frame_counter-$1000000)).w"),
                ("clr.b", "(byte_ffdea1).l"),
            ),
            name="VInt FRAME_COUNTER byte update",
        ),
        "secondsCounter": _require_order(
            _section(vint, "CallContextualFunctions"),
            (
                ("move.b", "((seconds_counter_frames-$1000000)).w,d0"),
                ("addq.b", "#1,d0"),
                ("cmpi.b", "#60,d0"),
                ("clr.b", "d0"),
                ("addq.l", "#1,((seconds_counter-$1000000)).w"),
                ("move.b", "d0,((seconds_counter_frames-$1000000)).w"),
            ),
            name="VInt seconds counter width and cadence",
        ),
        "timerWindow": _require_order(
            _section(timer_window, "VInt_UpdateTimerWindow"),
            (("move.l", "((seconds_counter-$1000000)).w,d1"),),
            name="Timer window seconds-counter long read",
        ),
        "randomSeed": _require_order(
            _section(rng, "GenerateRandomNumber"),
            (
                ("move.w", "(random_seed).l,d7"),
                ("move.w", "d7,(random_seed).l"),
            ),
            name="RANDOM_SEED generator word update",
        ),
    }


def _time_state_contract(
    disasm: Path, listing: str, addresses: dict[str, int], rom: bytes, ram: dict[str, int]
) -> dict[str, Any]:
    """Derive the four exact observed time/RNG spans without adjacent RAM."""
    _time_state_source_use_sites(
        (disasm / VINT_SOURCE).read_text(encoding="utf-8"),
        (disasm / TIMER_WINDOW_SOURCE).read_text(encoding="utf-8"),
        (disasm / RNG_SOURCE).read_text(encoding="utf-8"),
    )
    frame_instruction = _unique_h1_rom_instruction(
        listing,
        rom,
        entry=addresses["VInt"],
        scan_bytes=0x100,
        instruction=b"\x52\x38" + (ram["FRAME_COUNTER"] & 0xFFFF).to_bytes(2, "big"),
        purpose="FRAME_COUNTER addq.b",
    )
    seconds_increment = _unique_h1_rom_instruction(
        listing,
        rom,
        entry=addresses["CallContextualFunctions"],
        scan_bytes=0x80,
        instruction=b"\x52\xb8" + (ram["SECONDS_COUNTER"] & 0xFFFF).to_bytes(2, "big"),
        purpose="SECONDS_COUNTER addq.l",
    )
    seconds_frames_write = _unique_h1_rom_instruction(
        listing,
        rom,
        entry=addresses["CallContextualFunctions"],
        scan_bytes=0x80,
        instruction=b"\x11\xc0" + (ram["SECONDS_COUNTER_FRAMES"] & 0xFFFF).to_bytes(2, "big"),
        purpose="SECONDS_COUNTER_FRAMES move.b",
    )
    seconds_timer_read = _unique_h1_rom_instruction(
        listing,
        rom,
        entry=addresses["VInt_UpdateTimerWindow"],
        scan_bytes=0x50,
        instruction=b"\x22\x38" + (ram["SECONDS_COUNTER"] & 0xFFFF).to_bytes(2, "big"),
        purpose="SECONDS_COUNTER timer move.l",
    )
    random_read = _unique_h1_rom_instruction(
        listing,
        rom,
        entry=addresses["GenerateRandomNumber"],
        scan_bytes=0x40,
        instruction=b"\x3e\x39" + ram["RANDOM_SEED"].to_bytes(4, "big"),
        purpose="RANDOM_SEED generator move.w read",
    )
    random_write = _unique_h1_rom_instruction(
        listing,
        rom,
        entry=addresses["GenerateRandomNumber"],
        scan_bytes=0x40,
        instruction=b"\x33\xc7" + ram["RANDOM_SEED"].to_bytes(4, "big"),
        purpose="RANDOM_SEED generator move.w write",
    )
    return {
        "frameCounter": {
            "address": ram["FRAME_COUNTER"],
            "observationWidthBytes": 1,
            "sourceUpdateWidthBytes": 1,
            "sourceInstructionAddress": frame_instruction,
        },
        "randomSeed": {
            "address": ram["RANDOM_SEED"],
            "observationWidthBytes": 4,
            "sourceUpdateWidthBytes": 2,
            "sourceReadAddress": random_read,
            "sourceWriteAddress": random_write,
        },
        "secondsCounter": {
            "address": ram["SECONDS_COUNTER"],
            "observationWidthBytes": 4,
            "sourceUpdateWidthBytes": 4,
            "sourceIncrementAddress": seconds_increment,
            "timerReadAddress": seconds_timer_read,
        },
        "secondsCounterFrames": {
            "address": ram["SECONDS_COUNTER_FRAMES"],
            "observationWidthBytes": 1,
            "sourceUpdateWidthBytes": 1,
            "sourceWriteAddress": seconds_frames_write,
        },
        "normalization": VINT_TIME_NORMALIZATION,
    }


def build_map3_admitted_start_source_contract(
    rom_path: Path, upstream_path: Path
) -> dict[str, Any]:
    """Build the source/H1/ROM admission contract before fixture comparison."""
    upstream = upstream_path.resolve(strict=True)
    _assert_input_identity(rom_path, upstream)
    disasm = upstream / "disasm"
    rom = rom_path.resolve(strict=True).read_bytes()
    listing = (upstream / "build/sf2build-h1.lst").read_text(encoding="utf-8")
    addresses = listing_symbol_addresses(listing)
    witch = build_witch_new_game_lifecycle_source_contract(upstream)
    main_loop_source = (disasm / MAIN_LOOP_SOURCE).read_text(encoding="utf-8")
    exploration_source = (disasm / EXPLORATION_SOURCE).read_text(encoding="utf-8")
    new_game_source = (disasm / NEW_GAME_SOURCE).read_text(encoding="utf-8")
    map_setup_source = (disasm / MAP_SETUP_FUNCTIONS_SOURCE).read_text(encoding="utf-8")
    ram = _ram_contract(disasm)
    use_sites = {
        **_main_loop_use_sites(main_loop_source),
        **_exploration_use_sites(exploration_source),
        **_new_game_use_sites(new_game_source),
        "mapSetup": _require_order(
            _section(map_setup_source, "RunMapSetupInitFunction"),
            (
                ("movem.l", "d0-a1,-(sp)"),
                ("bsr.w", "getcurrentmapsetup"),
                ("cmpi.w", "#-1,(a0)"),
                ("bne.s", "loc_4750e"),
                ("bra.w", "loc_47514"),
                ("movea.l", "mapsetup_offset_init_function(a0),a0"),
                ("jsr", "(a0)"),
                ("movem.l", "(sp)+,d0-a1"),
                ("rts", ""),
            ),
            name="RunMapSetupInitFunction admitted chain",
        ),
        **_map3_init_use_sites(
            (disasm / "data/maps/entries/map03/mapsetups/s6_initfunction.asm").read_text(
                encoding="utf-8"
            )
        ),
        "timeState": _time_state_source_use_sites(
            (disasm / VINT_SOURCE).read_text(encoding="utf-8"),
            (disasm / TIMER_WINDOW_SOURCE).read_text(encoding="utf-8"),
            (disasm / RNG_SOURCE).read_text(encoding="utf-8"),
        ),
    }
    setup = build_map_setup_contract(rom_path, upstream)
    init = build_map_init_contract(rom_path, upstream)
    map3 = _map3_route_contract(setup, init, addresses, rom)
    init_call, init_return = _indirect_jsr_seams(
        rom, entry=addresses["RunMapSetupInitFunction"]
    )
    for required in (
        "CheckSram",
        "MainLoop",
        "ExplorationLoop",
        "RunMapSetupInitFunction",
        "WaitForEvent",
        "ms_map3_InitFunction",
        "cs_513A0",
        "cs_513BA",
        "MoveEntityOutOfMap",
        "VInt",
        "CallContextualFunctions",
        "VInt_UpdateTimerWindow",
        "GenerateRandomNumber",
    ):
        if required not in addresses:
            raise ValueError(f"Map 3 admitted-start H1 symbol is missing: {required}")
    source_hashes = {
        path.as_posix(): sha256((disasm / path).read_bytes()).hexdigest().upper()
        for path in (
            MAIN_LOOP_SOURCE,
            EXPLORATION_SOURCE,
            MAP_SETUP_FUNCTIONS_SOURCE,
            NEW_GAME_SOURCE,
            VINT_SOURCE,
            TIMER_WINDOW_SOURCE,
            RNG_SOURCE,
            Path("data/maps/entries/map03/mapsetups/s6_initfunction.asm"),
        )
    }
    unexpected_init_effect_call = _absolute_jsr_target(
        rom,
        entry=addresses[MAP3_SELECTED_INIT],
        target=addresses["MoveEntityOutOfMap"],
        scan_bytes=64,
    )
    session_patches = _session_patches(listing, rom, witch)
    scratch = witch["ram"]["workRamScratchAddress"]
    harness = {
        "checkpointAddress": scratch,
        "menuThunkAddress": scratch + 0x20,
        "generatedRamBytes": 64,
        "bootstrapFrameBudget": 1800,
        "caseFrameBudget": 3600,
        "romPatchDomain": "MD CART",
    }
    return {
        "function": {
            "checkSramAddress": witch["function"]["checkSramAddress"],
            "newActionAddress": witch["function"]["newActionAddress"],
            "newGameAddress": witch["function"]["newGameEffectiveTargetAddress"],
            "saveGameAddress": witch["function"]["saveGameAddress"],
            "mainLoopAddress": addresses["MainLoop"],
            "explorationLoopAddress": addresses["ExplorationLoop"],
            "runMapSetupInitFunctionAddress": addresses["RunMapSetupInitFunction"],
            "setupResolutionReturnAddress": map3["setupResolutionReturnAddress"],
            "initCallAddress": init_call,
            "initReturnAddress": init_return,
            "waitForEventAddress": addresses["WaitForEvent"],
            "selectedInitAddress": addresses[MAP3_SELECTED_INIT],
            "unexpectedScriptAddresses": [addresses["cs_513A0"], addresses["cs_513BA"]],
            "unexpectedInitEffectAddress": addresses["MoveEntityOutOfMap"],
            "unexpectedInitEffectCallAddress": unexpected_init_effect_call,
        },
        "ram": ram,
        "timeState": _time_state_contract(disasm, listing, addresses, rom, ram),
        "witchNewAction": {
            "initialMenuInstructionTargetAddress": witch["function"][
                "menuInstructionTargetAddress"
            ],
            "nameAllyInstructionTargetAddress": witch["function"][
                "nameAllyInstructionTargetAddress"
            ],
            "displayTextAddress": witch["function"]["displayTextAddress"],
            "workRamScratchAddress": witch["ram"]["workRamScratchAddress"],
            "initialMenuPage": witch["newAction"]["initialMenuPage"],
            "difficultyMenuPage": witch["newAction"]["difficultyMenuPage"],
            "gameStartMap": witch["newAction"]["gameStartMap"],
            "gameStartSavepointX": witch["newAction"]["gameStartSavepointX"],
            "gameStartSavepointY": witch["newAction"]["gameStartSavepointY"],
            "gameStartFacing": witch["newAction"]["gameStartFacing"],
            "mainLoopD4": witch["newAction"]["mainLoopD4"],
        },
        "map3": map3,
        "map3SourceRecords": _map3_index_records(),
        "defaultGuardFlags": list(MAP3_DEFAULT_GUARD_FLAGS),
        "sessionPatches": session_patches,
        "harness": harness,
        "sourceHashes": source_hashes,
        "sourceUseSites": use_sites,
    }


def _fixture_static_projection(contract: dict[str, Any]) -> dict[str, Any]:
    projection = {
        key: contract[key]
        for key in (
            "function",
            "ram",
            "timeState",
            "witchNewAction",
            "map3",
            "map3SourceRecords",
            "defaultGuardFlags",
            "sessionPatches",
            "harness",
            "sourceHashes",
        )
    }
    projection["map3"] = {
        key: contract["map3"][key]
        for key in (
            "mapIndex",
            "defaultSetupSymbol",
            "defaultSetupAddress",
            "flagVariants",
            "selectedInitSymbol",
            "selectedInitAddress",
            "selectedInitDirectReturnStub",
            "setupResolutionReturnAddress",
            "selectedInitSourcePath",
            "selectedInitStatementCount",
            "selectedInitScriptTargets",
        )
    }
    return projection


def _fixture_source_context(contract: dict[str, Any]) -> dict[str, Any]:
    """Return only source-derived fields that are address-bound in the index."""
    return {
        "function": {
            key: contract["function"][key]
            for key in (
                "checkSramAddress",
                "newActionAddress",
                "newGameAddress",
                "saveGameAddress",
                "mainLoopAddress",
                "explorationLoopAddress",
                "runMapSetupInitFunctionAddress",
                "initCallAddress",
                "initReturnAddress",
                "waitForEventAddress",
                "selectedInitAddress",
            )
        },
        "ram": {
            key: contract["ram"][key]
            for key in ("CURRENT_MAP", "GAME_FLAGS")
        },
        "map3": {"selectedInitAddress": contract["map3"]["selectedInitAddress"]},
    }


def _assert_fixture(fixture: dict[str, Any], contract: dict[str, Any]) -> None:
    if fixture["caseOrder"] != list(CASE_IDS):
        raise ValueError("Map 3 admitted-start case order drift")
    if [case["caseId"] for case in fixture["cases"]] != list(CASE_IDS):
        raise ValueError("Map 3 admitted-start case ID matrix drift")
    if fixture["cases"] != [
        {
            "caseId": "controlled-new-map3-default",
            "injectedInitialMenuReturn": 1,
            "injectedDifficultyMenuReturn": 0,
            "preconditionSaveFlags": 0,
        }
    ]:
        raise ValueError("Map 3 admitted-start controlled admission input matrix drift")
    if fixture["static"] != _fixture_static_projection(contract):
        raise ValueError(
            "Map 3 admitted-start fixture static projection disagrees with source/H1/ROM"
        )
    if fixture["sourceContext"] != _fixture_source_context(contract):
        raise ValueError(
            "Map 3 admitted-start fixture index source context disagrees with source/H1/ROM"
        )
    provenance = fixture["privateProvenance"]
    if provenance["romSha256"] != CANONICAL_ROM_SHA256:
        raise ValueError("Map 3 admitted-start private input provenance ROM drift")
    if provenance["upstreamCommit"] != UPSTREAM_COMMIT:
        raise ValueError("Map 3 admitted-start private input provenance upstream drift")
    if fixture["expectedObservation"]["caseOrder"] != list(CASE_IDS):
        raise ValueError("Map 3 admitted-start expected observation case order drift")


def _validate_expected_matrix(fixture: dict[str, Any], contract: dict[str, Any]) -> None:
    """Lock the compact observed chronology and source-derived setup relations."""
    observation = fixture["expectedObservation"]
    if observation["caseOrder"] != list(CASE_IDS) or len(observation["records"]) != 1:
        raise ValueError("Map 3 admitted-start expected observation matrix drift")
    record = observation["records"][0]
    if record["caseId"] != CASE_IDS[0] or tuple(record["chronology"]) != EXPECTED_CHRONOLOGY:
        raise ValueError("Map 3 admitted-start expected chronology drift")
    setup = record["selectedSetup"]
    expected_setup = {
        "setupAddress": contract["map3"]["defaultSetupAddress"],
        "initAddress": contract["map3"]["selectedInitAddress"],
        "setupResolutionReturnPc": contract["function"]["setupResolutionReturnAddress"],
        "initCallPc": contract["function"]["initCallAddress"],
        "initReturnPc": contract["function"]["initReturnAddress"],
    }
    if setup != expected_setup:
        raise ValueError("Map 3 admitted-start selected setup/init seam drift")
    handoff = record["handoff"]
    if (
        handoff["currentMap"] != contract["map3"]["mapIndex"]
        or handoff["egressMap"] != contract["map3"]["mapIndex"]
        or handoff["d0"] != contract["witchNewAction"]["gameStartMap"]
        or handoff["d1"] != contract["witchNewAction"]["gameStartSavepointX"]
        or handoff["d2"] != contract["witchNewAction"]["gameStartSavepointY"]
        or handoff["d3"] != contract["witchNewAction"]["gameStartFacing"]
        or handoff["d4"] != contract["witchNewAction"]["mainLoopD4"]
        or record["programRequest"] != "none"
    ):
        raise ValueError("Map 3 admitted-start handoff/program matrix drift")
    vint_time = record["scenarioState"]["vintTime"]
    if vint_time["normalization"] != contract["timeState"]["normalization"]:
        raise ValueError("Map 3 admitted-start VInt time normalization drift")
    if (
        vint_time["frameCounter"] != 0
        or vint_time["secondsCounter"] != 0
        or vint_time["secondsCounterFrames"] != 0
    ):
        raise ValueError("Map 3 admitted-start VInt controlled normalization drift")
    if not observation["callbacksCleared"] or not all(
        value is True for value in observation["restoration"].values()
    ):
        raise ValueError("Map 3 admitted-start expected cleanup/restoration matrix drift")


def _assert_index_bindings() -> None:
    """Only actual callbacks/state reads may justify this rail's index bindings."""
    fixture_relative = FIXTURE.relative_to(repo_path(".")).as_posix()
    index = load_json(INDEX)
    records = {record["id"]: record for record in index["records"]}
    actual: dict[str, set[tuple[str, str]]] = {}
    for record in index["records"]:
        bindings = {
            (binding["addressId"], binding["fixtureField"])
            for evidence in record["evidence"]
            if evidence["fixture"] == fixture_relative
            and evidence["fixtureId"] == "sf2-map3-admitted-start-runtime-v1"
            for binding in evidence["bindings"]
        }
        if bindings:
            actual[record["id"]] = bindings
    if actual != INDEX_BINDINGS:
        raise ValueError(
            "Map 3 admitted-start index binding set must match actual callback/state roles"
        )
    map3_records = [
        record for record in index["records"] if record["sourcePath"].startswith(MAP3_SOURCE_PREFIX)
    ]
    if len(map3_records) != 26:
        raise ValueError("Map 3 admitted-start aggregate index corpus drift")
    if any(
        record["id"] != "map.data.ms-map3-initfunction"
        and record["id"] in actual
        for record in map3_records
    ):
        raise ValueError("Map 3 admitted-start must not bulk-associate aggregate Map 3 records")
    primary = records["map.data.ms-map3-initfunction"]
    expected_primary_contracts = ["docs/design/contracts/map3-controlled-admission.md"]
    if primary.get("designContracts") != expected_primary_contracts:
        raise ValueError("Map 3 admitted-start primary design contract singleton drift")


def _assert_lua_role_contract() -> None:
    source = OBSERVER.read_text(encoding="utf-8")
    missing = sorted(role for role in REQUIRED_LUA_ROLES if f'"{role}"' not in source)
    if missing:
        raise ValueError(f"Map 3 admitted-start Lua callback role contract drift: {missing}")


def _observer_config(fixture: dict[str, Any], contract: dict[str, Any]) -> dict[str, Any]:
    return {
        "fixtureId": fixture["id"],
        "core": fixture["emulator"]["core"],
        "caseOrder": fixture["caseOrder"],
        "cases": fixture["cases"],
        "functions": contract["function"],
        "ram": contract["ram"],
        "witchNewAction": contract["witchNewAction"],
        "map3": contract["map3"],
        "harness": contract["harness"],
        "sessionPatches": contract["sessionPatches"],
        "observerFailureContract": observer_failure_contract(OWNER),
    }


def _assert_clean_observer_config(config: dict[str, Any]) -> None:
    """Keep fixture goldens and accepted outcomes out of the Lua configuration."""
    forbidden = {
        "expectedObservation",
        "acceptedObservation",
        "records",
        "chronology",
        "restoration",
        "map3SourceRecords",
    }

    def visit(value: Any) -> None:
        if isinstance(value, dict):
            overlap = forbidden & set(value)
            if overlap:
                raise ValueError(
                    "Map 3 admitted-start observer config contains accepted output corpus: "
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
        STATUS_PATH,
        owner=OWNER,
        schema_path=FAILURE_SCHEMA,
        required_milestones=SUCCESS_MILESTONES,
    )
    lines = STATUS_PATH.read_text(encoding="utf-8").splitlines()
    if tuple(lines) != SUCCESS_MILESTONES:
        raise RuntimeError("Map 3 admitted-start success status sequence drift")


def _failure_diagnostic() -> dict[str, Any] | None:
    """Validate failure cleanup facts before propagating the emulator error."""
    payload = callback_failure_status(STATUS_PATH, owner=OWNER, schema_path=FAILURE_SCHEMA)
    if payload is None:
        return None
    if payload["caseId"] not in {"bootstrap", *CASE_IDS}:
        raise ValueError("Map 3 admitted-start callback failure case identity drift")
    restoration = payload["restoration"]
    if payload["callbackCount"] != 0 or not payload["callbacksCleared"]:
        raise ValueError("Map 3 admitted-start callback failure residual callback drift")
    if not payload["outputRemoved"] or not restoration["outputRemoved"]:
        raise ValueError("Map 3 admitted-start callback failure output cleanup drift")
    if restoration["callbacksCleared"] != payload["callbacksCleared"]:
        raise ValueError("Map 3 admitted-start callback cleanup fact mismatch")
    if restoration["sessionStateRestored"]:
        if payload["restorationMismatch"] is not None:
            raise ValueError("Map 3 admitted-start restored failure must not report a mismatch")
    elif payload["restorationMismatch"] is None:
        raise ValueError("Map 3 admitted-start failed restoration needs its first mismatch")
    return payload


def preflight_map3_admitted_start(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner="Map 3 admitted-start fixture")
    _assert_lua_role_contract()
    _, executable = bizhawk_contract()
    validate_lua_syntax(OBSERVER, executable)
    contract = build_map3_admitted_start_source_contract(rom_path, upstream_path)
    _assert_fixture(fixture, contract)
    _validate_expected_matrix(fixture, contract)
    _assert_index_bindings()
    _assert_clean_observer_config(_observer_config(fixture, contract))
    return {
        "Fixture": fixture["id"],
        "Cases": len(CASE_IDS),
        "Map3IndexedRecords": len(contract["map3SourceRecords"]),
        "Status": "PRELAUNCH-PASS",
    }


def verify_map3_admitted_start(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 180
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner="Map 3 admitted-start fixture")
    verify_runtime_contract(fixture, rom_path)
    contract = build_map3_admitted_start_source_contract(rom_path, upstream_path)
    _assert_fixture(fixture, contract)
    _validate_expected_matrix(fixture, contract)
    _assert_index_bindings()
    _assert_lua_role_contract()
    config = _observer_config(fixture, contract)
    _assert_clean_observer_config(config)
    canonical_before = inspect_rom(rom_path.resolve(strict=True))["sha256"]
    session_deleted = False
    try:
        with tempfile.TemporaryDirectory(prefix="sf2-map3-admitted-start-") as temporary:
            session = Path(temporary) / "map3-admitted-start-session.bin"
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
        validate_json(observed, OBSERVATION_SCHEMA, owner="Map 3 admitted-start observation")
        if observed != fixture["expectedObservation"]:
            raise ValueError(
                "Map 3 admitted-start runtime matrix mismatch\n"
                f"expected={fixture['expectedObservation']!r}\nobserved={observed!r}"
            )
        _validate_expected_matrix(fixture, contract)
        OBSERVED_OUTPUT.write_text(json.dumps(observed, indent=2) + "\n", encoding="utf-8")
    except Exception:
        OBSERVED_OUTPUT.unlink(missing_ok=True)
        _failure_diagnostic()
        raise
    if inspect_rom(rom_path.resolve(strict=True))["sha256"] != canonical_before:
        raise ValueError("Map 3 admitted-start canonical ROM changed during session run")
    return {
        "Fixture": fixture["id"],
        "Cases": len(fixture["cases"]),
        "BizHawkLaunches": 1,
        "Map3IndexedRecords": len(contract["map3SourceRecords"]),
        "SessionRomDeleted": session_deleted,
        "Status": "PASS",
    }
