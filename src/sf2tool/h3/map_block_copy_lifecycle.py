"""Static provenance and bounded H3 replay for the map-block copy show/hide lifecycle."""

from __future__ import annotations

import re
from pathlib import Path
from typing import Any

from sf2tool.h3.bizhawk import DERIVED_ROOT, run_observer, verify_runtime_contract
from sf2tool.h3.map_lifecycle import _with_instrumented_rom_database
from sf2tool.h3.observer_status import assert_observer_status, observer_failure_contract
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.rom import inspect_rom, mega_drive_checksum

H1_LISTING_PATH = Path("build/sf2build-h1.lst")
DISPATCH_SOURCE = Path("code/common/scripting/entity/entityscriptengine_2.asm")
EXPLORATION_SOURCE = Path("code/gameflow/exploration/exploration.asm")
MACRO_SOURCE = Path("sf2cutscenemacros.asm")
MAP_MACRO_SOURCE = Path("sf2mapmacros.asm")
EQUATE_PATHS = (Path("sf2const.asm"), Path("sf2enums.asm"))
USE_PATHS = (
    Path("data/scripting/entity/eas_main.asm"),
    Path("data/scripting/map/debugscripts.asm"),
)
ROOF_PATH = Path("data/maps/entries/map03/5-roof-events.asm")
FIXTURE = repo_path("tests/fixtures/h3/map-block-copy-lifecycle-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h3-map-block-copy-lifecycle-fixture.schema.json")
OBSERVATION_SCHEMA = repo_path("schemas/h3-map-block-copy-lifecycle-observation.schema.json")
FAILURE_SCHEMA = repo_path("schemas/h3/map-block-copy-lifecycle-callback-failure.schema.json")
OBSERVER = repo_path("tools/bizhawk/map_block_copy_lifecycle_observer.lua")
OUTPUT_NAME = "map-block-copy-lifecycle"
OBSERVER_FAILURE_CONTRACT = observer_failure_contract(OUTPUT_NAME)

CASE_MATRIX: tuple[tuple[str, str, str | None], ...] = (
    ("dispatcher-fading-skip", "dispatcher-fading-skip", None),
    ("dispatcher-neutral-flag", "dispatcher-neutral-flag", None),
    ("dispatcher-show", "dispatcher-show", "positive"),
    ("dispatcher-hide", "dispatcher-hide", "positive"),
    ("perform-busy-skip", "perform-busy-skip", None),
    ("perform-terminator-miss", "perform-terminator-miss", None),
    ("perform-matched-positive", "perform-matched-positive", "positive"),
    ("perform-matched-negative", "perform-matched-negative", "negative"),
    ("csub-inactive-skip", "csub-inactive-skip", None),
    ("csub-active-restore", "csub-active", "positive"),
)
CASE_IDS = tuple(case_id for case_id, _, _ in CASE_MATRIX)


def _validate_case_matrix(fixture: dict[str, Any]) -> None:
    """Fixture schemas are structural; this owns the accepted ten-row corpus."""
    actual = [(row.get("id"), row.get("kind"), row.get("roofKind")) for row in fixture["cases"]]
    if actual != list(CASE_MATRIX):
        raise ValueError("map block copy lifecycle exact case ID/order/kind matrix drift")
    for row, (case_id, kind, roof_kind) in zip(fixture["cases"], CASE_MATRIX, strict=True):
        common = {
            "id",
            "kind",
            "mapIndex",
            "fadingSeed",
            "busySeed",
            "sentinelSeed",
            "layoutSeeds",
            "expected",
        }
        if kind.startswith("dispatcher-"):
            allowed = common | {"entityCoordinate", "blockWord"}
        else:
            allowed = common | {"generatedCallSiteAddress", "generatedReturnAddress"}
        if roof_kind is not None:
            allowed.add("roofKind")
        if set(row) != allowed:
            raise ValueError(f"map block copy lifecycle case scope drift: {case_id}")
        if row["id"] != case_id or row["kind"] != kind or row.get("roofKind") != roof_kind:
            raise ValueError(f"map block copy lifecycle case classification drift: {case_id}")


def _statement(line: str) -> str:
    return re.sub(r"\s+", " ", line.split(";", 1)[0].strip())


def _section(source: str, symbol: str) -> list[str]:
    start = re.search(
        rf"^{re.escape(symbol)}:\s*(?P<macro>macro)?(?:\s*;.*)?$", source, re.MULTILINE
    )
    if start is None:
        raise ValueError(f"map block copy lifecycle missing source section: {symbol}")
    end_marker = "endm" if start.group("macro") else f"; End of function {symbol}"
    end = source.find(end_marker, start.end())
    if end < 0:
        raise ValueError(f"map block copy lifecycle missing source end: {symbol}")
    return [line for raw in source[start.end() : end].splitlines() if (line := _statement(raw))]


def _parse_equates(upstream: Path, names: set[str]) -> dict[str, int]:
    pattern = re.compile(
        r"^(?P<name>[A-Za-z_][A-Za-z0-9_]*):\s+equ\s+(?P<value>\$[0-9A-Fa-f]+|-?\d+|[A-Za-z_][A-Za-z0-9_]*)\b",
        re.MULTILINE,
    )
    sources = [(upstream / "disasm" / path).read_text(encoding="utf-8") for path in EQUATE_PATHS]
    raw = {
        match.group("name"): match.group("value")
        for source in sources
        for match in pattern.finditer(source)
    }
    resolved: dict[str, int] = {}

    def resolve(name: str, stack: tuple[str, ...] = ()) -> int:
        if name in resolved:
            return resolved[name]
        if name in stack or name not in raw:
            raise ValueError(f"map block copy lifecycle equate drift: {name}")
        value = raw[name]
        answer = (
            int(value[1:], 16)
            if value.startswith("$")
            else int(value)
            if re.fullmatch(r"-?\d+", value)
            else resolve(value, (*stack, name))
        )
        resolved[name] = answer
        return answer

    return {name: resolve(name) for name in sorted(names)}


def _h1_instruction_rows(listing: str, symbol: str) -> list[dict[str, Any]]:
    """Return instruction-scoped H1 rows, retaining the assembled width and bytes.

    H1 deliberately leaves relocatable branch operands as zero.  The caller joins
    those opcode/width facts to the canonical ROM before treating a seam as
    usable; a listing address alone is never an instrumentation authority.
    """
    start = re.search(rf"^[0-9A-F]{{8}}\s+{re.escape(symbol)}:\s*$", listing, re.MULTILINE)
    if start is None:
        raise ValueError(f"map block copy lifecycle missing H1: {symbol}")
    end = listing.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"map block copy lifecycle missing H1 end: {symbol}")
    rows: list[dict[str, Any]] = []
    for raw in listing[start.start() : end].splitlines():
        if not re.match(r"^[0-9A-F]{8}\s", raw):
            continue
        payload = "".join(raw[9:52].split())
        instruction = _statement(raw[52:])
        if not payload or not re.fullmatch(r"(?:[0-9A-F]{2})+", payload) or not instruction:
            continue
        rows.append(
            {"pc": int(raw[:8], 16), "bytes": bytes.fromhex(payload), "instruction": instruction}
        )
    if not rows:
        raise ValueError(f"map block copy lifecycle empty H1 instruction scope: {symbol}")
    return rows


def _h1_instruction(listing: str, symbol: str, instruction: str) -> dict[str, Any]:
    found = [
        row for row in _h1_instruction_rows(listing, symbol) if row["instruction"] == instruction
    ]
    if len(found) != 1:
        raise ValueError(f"map block copy lifecycle H1 use-site drift: {symbol}/{instruction}")
    return found[0]


def _h1_next(rows: list[dict[str, Any]], row: dict[str, Any], label: str) -> int:
    try:
        next_row = rows[rows.index(row) + 1]
    except IndexError as error:
        raise ValueError(
            f"map block copy lifecycle H1 missing next instruction: {label}"
        ) from error
    expected = row["pc"] + len(row["bytes"])
    if next_row["pc"] != expected:
        raise ValueError(f"map block copy lifecycle H1 return seam drift: {label}")
    return expected


def _rom_row(rom: bytes, row: dict[str, Any], label: str, *, relocatable: bool = False) -> None:
    """Guard source/H1 instruction identity against the canonical ROM bytes."""
    actual = rom[row["pc"] : row["pc"] + len(row["bytes"])]
    expected = row["bytes"]
    if len(actual) != len(expected) or (
        actual != expected if not relocatable else actual[:2] != expected[:2]
    ):
        raise ValueError(f"map block copy lifecycle ROM/H1 instruction drift: {label}")


def _bsr_target(rom: bytes, row: dict[str, Any], label: str) -> int:
    if len(row["bytes"]) != 4 or row["bytes"][:2] != b"\x61\x00":
        raise ValueError(f"map block copy lifecycle H1 BSR opcode/width drift: {label}")
    _rom_row(rom, row, label, relocatable=True)
    displacement = int.from_bytes(rom[row["pc"] + 2 : row["pc"] + 4], "big", signed=True)
    return row["pc"] + 4 + displacement


def _effective_bsr_target(
    rom: bytes, rows: list[dict[str, Any]], symbol_address: int, row: dict[str, Any], label: str
) -> int:
    """Validate the ROM's alias-aware BSR entry against the source/H1 prologue."""
    first = rows[0]
    if first["pc"] != symbol_address:
        raise ValueError(f"map block copy lifecycle H1 symbol/entry drift: {label}")
    _rom_row(rom, first, f"{label}/source-entry")
    target = _bsr_target(rom, row, label)
    next_pc = _h1_next(rows, first, f"{label}/prologue")
    # The pinned ROM enters three source-labelled helpers through an alias inside
    # their first H1 instruction (the source label is still the observer entry).
    # Constrain the effective target to that exact H1 width; do not silently turn
    # a raw ROM displacement into a new helper identity.
    if target < symbol_address or target > next_pc:
        raise ValueError(f"map block copy lifecycle {label} effective target drift")
    return target


def _immediate(instruction: str, label: str) -> int:
    match = re.search(r"#(?P<value>\$[0-9A-Fa-f]+|-?\d+)", instruction)
    if match is None:
        raise ValueError(f"map block copy lifecycle source immediate drift: {label}")
    value = match.group("value")
    return int(value[1:], 16) if value.startswith("$") else int(value)


def _required_order(source: str, symbol: str, expected: list[str]) -> list[str]:
    actual = _section(source, symbol)
    if actual != expected:
        raise ValueError(f"map block copy lifecycle source use-site/order drift: {symbol}")
    return actual


def _require_subsequence(actual: list[str], label: str, expected: tuple[str, ...]) -> None:
    """Reject an instruction-order mutation without making labels into semantics."""
    cursor = 0
    for instruction in actual:
        if cursor < len(expected) and instruction == expected[cursor]:
            cursor += 1
    if cursor != len(expected):
        raise ValueError(f"map block copy lifecycle source sequence drift: {label}")


def _source_instruction(section: list[str], instruction: str, label: str) -> str:
    found = [actual for actual in section if actual == instruction]
    if len(found) != 1:
        raise ValueError(f"map block copy lifecycle source instruction drift: {label}")
    return found[0]


def _short_branch_target(rom: bytes, row: dict[str, Any], label: str) -> int:
    if len(row["bytes"]) != 2 or row["bytes"][0] not in {0x60, 0x66}:
        raise ValueError(f"map block copy lifecycle H1 branch opcode/width drift: {label}")
    actual = rom[row["pc"] : row["pc"] + 2]
    if len(actual) != 2 or actual[0] != row["bytes"][0] or actual[1] == 0:
        raise ValueError(f"map block copy lifecycle ROM/H1 branch drift: {label}")
    return row["pc"] + 2 + int.from_bytes(actual[1:], "big", signed=True)


def _roof_records(text: str) -> list[dict[str, Any]]:
    pattern = re.compile(
        r"^\s*slbc\s+(\d+),\s*(\d+).*?^\s*slbcSource\s+(\d+),\s*(\d+).*?^\s*slbcSize\s+(\d+),\s*(\d+).*?^\s*slbcDest\s+(\d+),\s*(\d+)",
        re.MULTILINE | re.DOTALL,
    )
    rows = [
        {
            "trigger": {"x": int(a), "y": int(b)},
            "source": {"x": int(c), "y": int(d)},
            "dimensions": {"width": int(e), "height": int(f)},
            "destination": {"x": int(g), "y": int(h)},
        }
        for a, b, c, d, e, f, g, h in pattern.findall(text)
    ]
    if len(rows) != 10:
        raise ValueError("map block copy lifecycle map03 roof record inventory drift")
    return rows


def build_map_block_copy_lifecycle_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    """Parse the complete assigned source surface and its source/H1 seams."""
    rom = rom_path.resolve(strict=True).read_bytes()
    upstream = upstream_path / "disasm"
    dispatcher = (upstream / DISPATCH_SOURCE).read_text(encoding="utf-8")
    exploration = (upstream / EXPLORATION_SOURCE).read_text(encoding="utf-8")
    macro = (upstream / MACRO_SOURCE).read_text(encoding="utf-8")
    map_macro = (upstream / MAP_MACRO_SOURCE).read_text(encoding="utf-8")
    equates = _parse_equates(
        upstream_path,
        {
            "ENTITY_DATA",
            "ENTITYDEF_OFFSET_ACTSCRIPTADDR",
            "ENTITYDEF_OFFSET_X",
            "ENTITYDEF_OFFSET_Y",
            "ENTITYDEF_OFFSET_XDEST",
            "ENTITYDEF_OFFSET_YDEST",
            "ENTITYDEF_SIZE",
            "FADING_SETTING",
            "FF0000_RAM_START",
            "INDEX_SHIFT_COUNT",
            "MAPDATA_OFFSET_EVENT_ROOF",
            "MAP_TILE_SIZE",
            "VIEW_PLANE_UPDATE_TOGGLE_BITFIELD",
            "word_FF5C00",
            "word_FF5C02",
            "word_FF5C04",
            "word_FF5C06",
            "word_FFAF42",
            "byte_FF5C08",
            "CURRENT_MAP",
            "CURRENT_BATTLE",
            "NOT_CURRENTLY_IN_BATTLE",
        },
    )
    if _section(macro, "ac_checkMapBlockCopy") != ["dc.w $40"]:
        raise ValueError("map block copy lifecycle action macro drift")
    for symbol, expected in (
        ("slbc", ["dc.b \\1", "dc.b \\2"]),
        ("slbcSource", ["dc.b \\1", "dc.b \\2"]),
        ("slbcSize", ["dc.b \\1", "dc.b \\2"]),
        ("slbcDest", ["dc.b \\1", "dc.b \\2"]),
    ):
        if _section(map_macro, symbol) != expected:
            raise ValueError(f"map block copy lifecycle roof macro drift: {symbol}")
    dispatch_expected = [
        "tst.b ((FADING_SETTING-$1000000)).w",
        "bne.s loc_5D42",
        "move.w (a0),d0",
        "move.w ENTITYDEF_OFFSET_Y(a0),d1",
        "bsr.w ConvertMapPixelCoordinatesToOffset",
        "move.w (a4,d2.w),d3",
        "move.w d3,d2",
        "andi.w #$3C00,d2",
        "cmpi.w #$800,d2",
        "bne.s loc_5D38",
        "bsr.w PerformMapBlockCopyScript",
        "bra.s loc_5D42",
        "loc_5D38:",
        "cmpi.w #$C00,d2",
        "bne.s loc_5D42",
        "loc_5D3E:",
        "bsr.w csub_40F2",
        "loc_5D42:",
        "addq.l #2,a1",
        "bra.w esc_clearTimerGoToNextCommand",
    ]
    dispatch_actual = _required_order(dispatcher, "esc40_checkMapBlockCopy", dispatch_expected)
    perform = _section(exploration, "PerformMapBlockCopyScript")
    hide = _section(exploration, "csub_40F2")
    convert = _section(dispatcher, "ConvertMapPixelCoordinatesToOffset")
    required_perform = [
        "tst.w ((word_FFAF42-$1000000)).w",
        "bne.w loc_40E6",
        "lsr.w #7,d0",
        "lsr.w #7,d1",
        "moveq #1,d2",
        "movea.l MAPDATA_OFFSET_EVENT_ROOF(a2),a2",
        "tst.b (a2)",
        "bmi.w loc_40E6",
        "cmp.b (a2),d0",
        "cmp.b 1(a2),d1",
        "move.w d2,((word_FFAF42-$1000000)).w",
        "move.b (a2)+,d0",
        "ext.w d0",
        "move.b (a2)+,d1",
        "ext.w d1",
        "move.b (a2)+,d6",
        "subq.b #1,d6",
        "move.b (a2)+,d7",
        "subq.b #1,d7",
        "move.b (a2)+,d2",
        "move.b (a2)+,d3",
        "move.w d2,(word_FF5C00).l",
        "move.w d3,(word_FF5C02).l",
        "move.w d6,(word_FF5C04).l",
        "move.w d7,(word_FF5C06).l",
        "tst.w d1",
        "blt.s loc_40BA",
        "move.w (a2,d2.w),(a3)+",
        "move.w (a2,d0.w),(a2,d2.w)",
        "clr.w (a2,d2.w)",
        "move.w #-1,(a3)",
        "bset #0,((VIEW_PLANE_UPDATE_TOGGLE_BITFIELD-$1000000)).w",
    ]
    if any(instruction not in perform for instruction in required_perform):
        raise ValueError("map block copy lifecycle perform use-site drift")
    _require_subsequence(
        perform,
        "perform roof scan",
        (
            "moveq #1,d2",
            "lea table_MapOffsetHash(pc), a3",
            "move.b (a3,d0.w),d0",
            "move.b (a3,d1.w),d1",
            "movea.l MAPDATA_OFFSET_EVENT_ROOF(a2),a2",
            "tst.b (a2)",
            "cmp.b (a2),d0",
            "cmp.b 1(a2),d1",
            "move.w d2,((word_FFAF42-$1000000)).w",
            "tst.w (a2)+",
            "addq.l #8,a2",
            "addq.w #1,d2",
            "bra.w loc_4028",
        ),
    )
    _require_subsequence(
        perform,
        "perform copy rectangle",
        (
            "lsl.w #6,d3",
            "add.w d3,d2",
            "add.w d2,d2",
            "lsl.w #6,d1",
            "add.w d1,d0",
            "add.w d0,d0",
            "move.w (a2,d2.w),(a3)+",
            "move.w (a2,d0.w),(a2,d2.w)",
            "dbf d6,loc_4096",
            "addi.w #$80,d0",
            "addi.w #$80,d2",
            "dbf d7,loc_4092",
            "move.w #-1,(a3)",
        ),
    )
    _require_subsequence(
        perform,
        "perform clear rectangle",
        (
            "move.w (a2,d2.w),(a3)+",
            "clr.w (a2,d2.w)",
            "dbf d6,loc_40BE",
            "addi.w #$80,d2",
            "dbf d7,loc_40BA",
        ),
    )
    required_hide = [
        "tst.w ((word_FFAF42-$1000000)).w",
        "beq.w loc_4150",
        "clr.w ((word_FFAF42-$1000000)).w",
        "move.w (word_FF5C00).l,d2",
        "move.w (word_FF5C02).l,d3",
        "move.w (word_FF5C04).l,d6",
        "move.w (word_FF5C06).l,d7",
        "move.w (a3)+,(a2,d2.w)",
        "bset #0,((VIEW_PLANE_UPDATE_TOGGLE_BITFIELD-$1000000)).w",
    ]
    if any(instruction not in hide for instruction in required_hide):
        raise ValueError("map block copy lifecycle hide use-site drift")
    _require_subsequence(
        hide,
        "hide restore rectangle",
        (
            "clr.w ((word_FFAF42-$1000000)).w",
            "lsl.w #6,d3",
            "add.w d3,d2",
            "add.w d2,d2",
            "move.w (a3)+,(a2,d2.w)",
            "dbf d6,loc_4130",
            "addi.w #128,d2",
            "dbf d7,loc_412C",
            "bset #0,((VIEW_PLANE_UPDATE_TOGGLE_BITFIELD-$1000000)).w",
        ),
    )
    required_convert = [
        "lsr.w #7,d2",
        "lsr.w #7,d3",
        "andi.w #$3F,d2",
        "andi.w #$3F,d3",
        "lsl.w #6,d3",
        "add.w d3,d2",
        "add.w d2,d2",
    ]
    if any(instruction not in convert for instruction in required_convert):
        raise ValueError("map block copy lifecycle coordinate conversion drift")
    uses = []
    for path in USE_PATHS:
        matches = [
            index + 1
            for index, line in enumerate((upstream / path).read_text(encoding="utf-8").splitlines())
            if re.search(r"(?:^|\s)ac_checkMapBlockCopy$", _statement(line))
        ]
        uses.append(
            {
                "sourcePath": path.as_posix(),
                "instructionSiteCount": len(matches),
                "lineNumbers": matches,
            }
        )
    if uses != [
        {
            "sourcePath": "data/scripting/entity/eas_main.asm",
            "instructionSiteCount": 3,
            "lineNumbers": [42, 54, 66],
        },
        {
            "sourcePath": "data/scripting/map/debugscripts.asm",
            "instructionSiteCount": 2,
            "lineNumbers": [41, 44],
        },
    ]:
        raise ValueError("map block copy lifecycle five-use inventory drift")
    listing = (upstream_path / H1_LISTING_PATH).read_text(encoding="utf-8")
    symbols = listing_symbol_addresses(listing)
    vint_rows = _h1_instruction_rows(listing, "VInt_UpdateEntities")
    dispatch_rows = _h1_instruction_rows(listing, "esc40_checkMapBlockCopy")
    perform_rows = _h1_instruction_rows(listing, "PerformMapBlockCopyScript")
    hide_rows = _h1_instruction_rows(listing, "csub_40F2")
    convert_rows = _h1_instruction_rows(listing, "ConvertMapPixelCoordinatesToOffset")
    _rom_row(rom, vint_rows[0], "VInt_UpdateEntities/entry")
    _rom_row(rom, dispatch_rows[0], "esc40_checkMapBlockCopy/entry")

    update_call = _h1_instruction(listing, "VInt_UpdateEntities", "bsr.w UpdateEntityData")
    update_next = _h1_instruction(
        listing, "VInt_UpdateEntities", "move.l ENTITYDEF_OFFSET_ACTSCRIPTADDR(a0),d0"
    )
    if (
        _h1_next(vint_rows, update_call, "VInt_UpdateEntities/UpdateEntityData")
        != update_next["pc"]
    ):
        raise ValueError("map block copy lifecycle instrumentation instruction order drift")
    update_effective_target = _effective_bsr_target(
        rom,
        _h1_instruction_rows(listing, "UpdateEntityData"),
        symbols["UpdateEntityData"],
        update_call,
        "VInt_UpdateEntities/UpdateEntityData",
    )
    _rom_row(rom, update_next, "VInt_UpdateEntities/post-UpdateEntityData")
    action_dispatch = _h1_instruction(
        listing, "VInt_UpdateEntities", "jmp rjt_EntityScriptCommands(pc,d2.w)"
    )
    _rom_row(rom, action_dispatch, "VInt_UpdateEntities/action-dispatch", relocatable=True)

    convert_call = _h1_instruction(
        listing, "esc40_checkMapBlockCopy", "bsr.w ConvertMapPixelCoordinatesToOffset"
    )
    perform_call = _h1_instruction(
        listing, "esc40_checkMapBlockCopy", "bsr.w PerformMapBlockCopyScript"
    )
    hide_call = _h1_instruction(listing, "esc40_checkMapBlockCopy", "bsr.w csub_40F2")
    for row, target_rows, target, label in (
        (
            convert_call,
            convert_rows,
            symbols["ConvertMapPixelCoordinatesToOffset"],
            "dispatch/convert",
        ),
        (perform_call, perform_rows, symbols["PerformMapBlockCopyScript"], "dispatch/perform"),
        (hide_call, hide_rows, symbols["csub_40F2"], "dispatch/hide"),
    ):
        _effective_bsr_target(rom, target_rows, target, row, label)
    convert_return = _h1_next(dispatch_rows, convert_call, "dispatch/convert")
    perform_return = _h1_next(dispatch_rows, perform_call, "dispatch/perform")
    hide_return = _h1_next(dispatch_rows, hide_call, "dispatch/hide")
    tail = _h1_instruction(listing, "esc40_checkMapBlockCopy", "addq.l #2,a1")
    _rom_row(rom, tail, "dispatch/tail")
    if hide_return != tail["pc"]:
        raise ValueError("map block copy lifecycle dispatcher hide/tail order drift")
    branch_targets = {
        _short_branch_target(
            rom,
            _h1_instruction(listing, "esc40_checkMapBlockCopy", "bra.s loc_5D42"),
            "dispatch/show-tail",
        ),
    }
    # Both source BNEs that reach the dispatcher tail share one H1 physical-PC
    # spelling; select them by their actual ROM PCs to retain both branches.
    tail_bnes = [row for row in dispatch_rows if row["instruction"] == "bne.s loc_5D42"]
    if len(tail_bnes) != 2:
        raise ValueError("map block copy lifecycle dispatcher tail branch inventory drift")
    branch_targets.update(
        _short_branch_target(rom, row, f"dispatch/tail-bne-{index}")
        for index, row in enumerate(tail_bnes, start=1)
    )
    if branch_targets != {tail["pc"]}:
        raise ValueError("map block copy lifecycle dispatcher branch/tail identity drift")
    if (
        _short_branch_target(
            rom,
            _h1_instruction(listing, "esc40_checkMapBlockCopy", "bne.s loc_5D38"),
            "dispatch/show-to-hide",
        )
        != _h1_instruction(listing, "esc40_checkMapBlockCopy", "cmpi.w #$C00,d2")["pc"]
    ):
        raise ValueError("map block copy lifecycle dispatcher show/hide branch identity drift")
    for row, label in (
        (_h1_instruction(listing, "esc40_checkMapBlockCopy", "andi.w #$3C00,d2"), "dispatch/mask"),
        (_h1_instruction(listing, "esc40_checkMapBlockCopy", "cmpi.w #$800,d2"), "dispatch/show"),
        (
            _h1_instruction(listing, "esc40_checkMapBlockCopy", "cmpi.w #$C00,d2"),
            "dispatch/hide-flag",
        ),
        (_h1_instruction(listing, "PerformMapBlockCopyScript", "lsl.w #6,d3"), "perform/row-shift"),
        (
            _h1_instruction(listing, "PerformMapBlockCopyScript", "addi.w #$80,d0"),
            "perform/row-stride",
        ),
        (
            _h1_instruction(listing, "PerformMapBlockCopyScript", "move.w #-1,(a3)"),
            "perform/sentinel",
        ),
        (
            _h1_instruction(listing, "PerformMapBlockCopyScript", "addq.l #8,a2"),
            "perform/roof-record-stride",
        ),
        (_h1_instruction(listing, "csub_40F2", "addi.w #128,d2"), "hide/row-stride"),
        (
            _h1_instruction(listing, "csub_40F2", "clr.w ((word_FFAF42-$1000000)).w"),
            "hide/busy-clear",
        ),
        (
            _h1_instruction(listing, "ConvertMapPixelCoordinatesToOffset", "lsl.w #6,d3"),
            "convert/row-shift",
        ),
    ):
        _rom_row(rom, row, label)
    # Source operands are deliberately parsed at their use sites instead of being a
    # parallel hand-maintained numeric contract.
    block_mask = _immediate(
        _source_instruction(dispatch_actual, "andi.w #$3C00,d2", "dispatch/mask"), "dispatch/mask"
    )
    show_flag = _immediate(
        _source_instruction(dispatch_actual, "cmpi.w #$800,d2", "dispatch/show"), "dispatch/show"
    )
    hide_flag = _immediate(
        _source_instruction(dispatch_actual, "cmpi.w #$C00,d2", "dispatch/hide"), "dispatch/hide"
    )
    row_shift = _immediate(
        _source_instruction(perform, "lsl.w #6,d3", "perform/row-shift"), "perform/row-shift"
    )
    row_bytes = _immediate(
        _source_instruction(perform, "addi.w #$80,d0", "perform/row-stride"),
        "perform/row-stride",
    )
    sentinel = (
        _immediate(
            _source_instruction(perform, "move.w #-1,(a3)", "perform/sentinel"), "perform/sentinel"
        )
        & 0xFFFF
    )
    roof_record_bytes = _immediate(
        _source_instruction(perform, "addq.l #8,a2", "perform/roof-record-stride"),
        "perform/roof-record-stride",
    )
    if (
        _immediate(
            _source_instruction(hide, "addi.w #128,d2", "hide/row-stride"), "hide/row-stride"
        )
        != row_bytes
    ):
        raise ValueError("map block copy lifecycle source row-stride polarity drift")
    if row_bytes != (1 << row_shift) * 2:
        raise ValueError("map block copy lifecycle row geometry derivation drift")
    update_original = rom[update_call["pc"] : update_next["pc"] + len(update_next["bytes"])]
    stub_address = 0xFFA0
    generated_probe = 0xFF4050
    action_script = 0xFF3FF0
    stub_before = rom[stub_address : stub_address + 32]
    if len(stub_before) != 32 or stub_before != b"\xff" * 32:
        raise ValueError("map block copy lifecycle instrumentation stub ROM guard drift")
    stub = (
        b"\x4e\xb9"
        + symbols["UpdateEntityData"].to_bytes(4, "big")
        + b"\x4e\xb9"
        + generated_probe.to_bytes(4, "big")
        + update_next["bytes"]
        + b"\x4e\x75"
    )
    instrumentation = {
        "updateCallSiteAddress": update_call["pc"],
        "originalEffectiveTargetAddress": update_effective_target,
        "updateCallOriginalHex": update_original.hex().upper(),
        "updateCallPatchedHex": (b"\x4e\xb9" + stub_address.to_bytes(4, "big") + b"\x4e\x71")
        .hex()
        .upper(),
        "stubAddress": stub_address,
        "stubOriginalHex": stub_before.hex().upper(),
        "stubHex": stub.hex().upper(),
        "returnAddress": update_next["pc"] + len(update_next["bytes"]),
        "probeCallAddress": stub_address + 6,
        "probeReturnAddress": stub_address + 12,
        "generatedProbeAddress": generated_probe,
        "actionScriptAddress": action_script,
    }
    roof = _roof_records((upstream / ROOF_PATH).read_text(encoding="utf-8"))
    if roof_record_bytes != sum(
        len(roof[0][field]) for field in ("trigger", "source", "dimensions", "destination")
    ):
        raise ValueError("map block copy lifecycle roof scan record-width derivation drift")
    selected = {
        "negative": {**roof[0], "ordinal": roof.index(roof[0]) + 1},
        "positive": {**roof[5], "ordinal": roof.index(roof[5]) + 1},
    }
    if selected["negative"]["source"] != {"x": 255, "y": 255} or selected["positive"]["source"] != {
        "x": 51,
        "y": 20,
    }:
        raise ValueError("map block copy lifecycle selected roof source polarity drift")
    return {
        "function": {
            "vintUpdateEntitiesPc": symbols["VInt_UpdateEntities"],
            "updateEntityDataAddress": symbols["UpdateEntityData"],
            "updateEntityDataCallPc": update_call["pc"],
            "actionDispatchPc": action_dispatch["pc"],
            "dispatcherPc": symbols["esc40_checkMapBlockCopy"],
            "convertPc": symbols["ConvertMapPixelCoordinatesToOffset"],
            "convertCallPc": convert_call["pc"],
            "convertReturnPc": convert_return,
            "performPc": symbols["PerformMapBlockCopyScript"],
            "performCallPc": perform_call["pc"],
            "performReturnPc": perform_return,
            "hidePc": symbols["csub_40F2"],
            "hideCallPc": hide_call["pc"],
            "hideReturnPc": hide_return,
            "dispatcherTailPc": tail["pc"],
        },
        "ram": {
            "entityData": equates["ENTITY_DATA"],
            "layoutBase": equates["FF0000_RAM_START"],
            "busyWord": equates["word_FFAF42"],
            "savedRectangleMetadata": equates["word_FF5C00"],
            "savedRectangleBuffer": equates["byte_FF5C08"],
            "fadingSetting": equates["FADING_SETTING"],
            "currentMap": equates["CURRENT_MAP"],
            "currentBattle": equates["CURRENT_BATTLE"],
            "updateToggle": equates["VIEW_PLANE_UPDATE_TOGGLE_BITFIELD"],
        },
        "constants": {
            "mapTileSize": equates["MAP_TILE_SIZE"],
            "rowWords": 1 << row_shift,
            "wordBytes": row_bytes // (1 << row_shift),
            "rowBytes": row_bytes,
            "entityBytes": equates["ENTITYDEF_SIZE"],
            "entityActscriptOffset": equates["ENTITYDEF_OFFSET_ACTSCRIPTADDR"],
            "entityXOffset": equates["ENTITYDEF_OFFSET_X"],
            "entityYOffset": equates["ENTITYDEF_OFFSET_Y"],
            "entityXDestinationOffset": equates["ENTITYDEF_OFFSET_XDEST"],
            "entityYDestinationOffset": equates["ENTITYDEF_OFFSET_YDEST"],
            "blockFlagMask": block_mask,
            "showFlag": show_flag,
            "hideFlag": hide_flag,
            "savedBufferSentinel": sentinel,
            "mapDataRoofOffset": equates["MAPDATA_OFFSET_EVENT_ROOF"],
            "mapIndexShiftBits": equates["INDEX_SHIFT_COUNT"],
            "notCurrentlyInBattle": equates["NOT_CURRENTLY_IN_BATTLE"],
        },
        "sourceFacts": {
            "macroOpcode": 64,
            "dispatcherInstructions": dispatch_expected,
            "performRequiredInstructions": required_perform,
            "hideRequiredInstructions": required_hide,
            "coordinateRequiredInstructions": required_convert,
            "fiveUseInventory": uses,
            "map03RoofRecordCount": len(roof),
            "selectedRoofRecords": selected,
            "runtimeQuestions": [
                "map-block-copy-lifecycle/collision-pathfinding-navigation-effects",
                "map-block-copy-lifecycle/rendered-vdp-fade-audio-timing",
                "map-block-copy-lifecycle/persistence-reload-normal-story-reachability",
            ],
        },
        "instrumentation": instrumentation,
    }


def _address(static: dict[str, Any], coordinate: dict[str, int]) -> int:
    return (
        static["ram"]["layoutBase"]
        + (coordinate["y"] * static["constants"]["rowWords"] + coordinate["x"])
        * static["constants"]["wordBytes"]
    )


def _rectangle_words(static: dict[str, Any], row: dict[str, Any]) -> list[dict[str, int]]:
    destination = row["destination"]
    dims = row["dimensions"]
    return [
        {
            "address": _address(
                static, {"x": destination["x"] + column, "y": destination["y"] + line}
            ),
            "value": (0x5100 + line * 16 + column),
        }
        for line in range(dims["height"])
        for column in range(dims["width"])
    ]


def _derive_cases(static: dict[str, Any], fixture: dict[str, Any]) -> list[dict[str, Any]]:
    rows = []
    selected = static["sourceFacts"]["selectedRoofRecords"]
    for case in fixture["cases"]:
        kind = case["kind"]
        roof = selected.get(case.get("roofKind"))
        roof_ordinal = roof["ordinal"] if roof is not None else None
        if (
            kind in {"perform-matched-positive", "perform-matched-negative", "csub-active"}
            and roof is None
        ):
            raise ValueError(f"map block copy lifecycle roof binding drift: {case['id']}")
        if kind in {"perform-matched-positive", "dispatcher-show"}:
            expected = {
                "busyWordAfter": roof_ordinal,
                "layoutReadbacks": [
                    {"address": _address(static, roof["destination"]), "value": 0x3101}
                ],
                "savedBufferSentinelAfter": {
                    "address": static["ram"]["savedRectangleBuffer"]
                    + roof["dimensions"]["width"] * roof["dimensions"]["height"] * 2,
                    "value": 65535,
                },
            }
        elif kind == "perform-matched-negative":
            expected = {
                "busyWordAfter": roof_ordinal,
                "layoutReadbacks": [{"address": _address(static, roof["destination"]), "value": 0}],
                "savedBufferSentinelAfter": {
                    "address": static["ram"]["savedRectangleBuffer"]
                    + roof["dimensions"]["width"] * roof["dimensions"]["height"] * 2,
                    "value": 65535,
                },
            }
        elif kind in {"csub-active", "dispatcher-hide"}:
            words = _rectangle_words(static, roof)
            expected = {
                "busyWordAfter": 0,
                "layoutReadbacks": words,
                "savedBufferSentinelAfter": {
                    "address": static["ram"]["savedRectangleBuffer"]
                    + roof["dimensions"]["width"] * roof["dimensions"]["height"] * 2,
                    "value": case["sentinelSeed"],
                },
            }
        else:
            expected = {
                "busyWordAfter": case["busySeed"],
                "layoutReadbacks": case["layoutSeeds"],
                "savedBufferSentinelAfter": {
                    "address": static["ram"]["savedRectangleBuffer"],
                    "value": case["sentinelSeed"],
                },
            }
        dispatcher = kind.startswith("dispatcher-")
        helper = None
        if kind == "dispatcher-show":
            helper = {
                "callSiteAddress": static["function"]["performCallPc"],
                "targetAddress": static["function"]["performPc"],
                "returnAddress": static["function"]["performReturnPc"],
            }
        if kind == "dispatcher-hide":
            helper = {
                "callSiteAddress": static["function"]["hideCallPc"],
                "targetAddress": static["function"]["hidePc"],
                "returnAddress": static["function"]["hideReturnPc"],
            }
        if kind.startswith("perform-"):
            helper = {
                "callSiteAddress": case["generatedCallSiteAddress"],
                "targetAddress": static["function"]["performPc"],
                "returnAddress": case["generatedReturnAddress"],
            }
        if kind.startswith("csub-"):
            helper = {
                "callSiteAddress": case["generatedCallSiteAddress"],
                "targetAddress": static["function"]["hidePc"],
                "returnAddress": case["generatedReturnAddress"],
            }
        update_toggle_after = (
            1
            if kind
            in {
                "dispatcher-show",
                "dispatcher-hide",
                "perform-matched-positive",
                "perform-matched-negative",
                "csub-active",
            }
            else 0
        )
        if fixture["toggleGolden"].get(case["id"]) != update_toggle_after:
            raise ValueError(f"map block copy lifecycle toggle golden disagreement: {case['id']}")
        row = {
            "id": case["id"],
            "kind": kind,
            "updateEntityDataEntryObserved": dispatcher,
            "helperEvent": helper,
            "dispatcherTailAddressObserved": static["function"]["dispatcherTailPc"]
            if dispatcher
            else None,
            "updateToggleByteAfter": update_toggle_after,
            **expected,
        }
        fixture_row = {key: value for key, value in row.items() if key != "updateToggleByteAfter"}
        if case["expected"] != fixture_row:
            raise ValueError(f"map block copy lifecycle fixture/model disagreement: {case['id']}")
        rows.append(row)
    return rows


def _case_inputs(static: dict[str, Any], fixture: dict[str, Any]) -> list[dict[str, Any]]:
    """Pass only setup and read addresses to Lua; Python retains every accepted golden."""
    _validate_case_matrix(fixture)
    selected = static["sourceFacts"]["selectedRoofRecords"]
    inputs = []
    for case in fixture["cases"]:
        row = {key: value for key, value in case.items() if key != "expected"}
        roof = selected.get(case.get("roofKind"))
        kind = case["kind"]
        if kind in {"dispatcher-hide", "csub-active"}:
            if roof is None:
                raise ValueError(f"map block copy lifecycle missing restore roof: {case['id']}")
            addresses = [word["address"] for word in _rectangle_words(static, roof)]
        elif kind in {
            "dispatcher-show",
            "perform-matched-positive",
            "perform-matched-negative",
        }:
            if roof is None:
                raise ValueError(f"map block copy lifecycle missing copy roof: {case['id']}")
            addresses = [_address(static, roof["destination"])]
        else:
            addresses = [seed["address"] for seed in case["layoutSeeds"]]
        sentinel_address = static["ram"]["savedRectangleBuffer"]
        if roof is not None and kind in {
            "dispatcher-show",
            "dispatcher-hide",
            "perform-matched-positive",
            "perform-matched-negative",
            "csub-active",
        }:
            sentinel_address += roof["dimensions"]["width"] * roof["dimensions"]["height"] * 2
        layout_restore = {seed["address"] for seed in case["layoutSeeds"]}
        if case.get("entityCoordinate") is not None:
            position = case["entityCoordinate"]
            layout_restore.add(_address(static, position))
        if roof is not None and kind in {
            "dispatcher-show",
            "dispatcher-hide",
            "perform-matched-positive",
            "perform-matched-negative",
            "csub-active",
        }:
            layout_restore.update(word["address"] for word in _rectangle_words(static, roof))
        state_restore = [
            {"address": static["ram"][name], "width": 1}
            for name in ("fadingSetting", "currentMap", "currentBattle", "updateToggle")
        ] + [{"address": static["ram"]["busyWord"], "width": 2}]
        entity = static["ram"]["entityData"]
        state_restore.extend(
            {"address": entity + static["constants"][name], "width": width}
            for name, width in (
                ("entityXOffset", 2),
                ("entityYOffset", 2),
                ("entityXDestinationOffset", 2),
                ("entityYDestinationOffset", 2),
                ("entityActscriptOffset", 4),
            )
        )
        state_restore.extend(
            [
                {"address": static["ram"]["savedRectangleBuffer"], "width": 2},
                {"address": fixture["instrumentation"]["actionScriptAddress"], "width": 2},
            ]
        )
        probe_words = 7 if kind.startswith("perform-") else 3 if kind.startswith("csub-") else 1
        state_restore.extend(
            {
                "address": fixture["instrumentation"]["generatedProbeAddress"] + index * 2,
                "width": 2,
            }
            for index in range(probe_words)
        )
        if roof is not None and kind in {
            "dispatcher-show",
            "dispatcher-hide",
            "perform-matched-positive",
            "perform-matched-negative",
            "csub-active",
        }:
            metadata = static["ram"]["savedRectangleMetadata"]
            state_restore.extend(
                {"address": metadata + offset, "width": 2} for offset in range(0, 8, 2)
            )
            state_restore.extend(
                {
                    "address": static["ram"]["savedRectangleBuffer"] + index * 2,
                    "width": 2,
                }
                for index in range(roof["dimensions"]["width"] * roof["dimensions"]["height"] + 1)
            )
        restoration = [
            *({"address": address, "width": 2} for address in sorted(layout_restore)),
            *state_restore,
        ]
        by_address: dict[int, int] = {}
        for cell in restoration:
            address, width = cell["address"], cell["width"]
            if address in by_address and by_address[address] != width:
                raise ValueError(
                    f"map block copy lifecycle restoration width overlap: {case['id']}"
                )
            by_address[address] = width
        if len(by_address) != len(restoration):
            restoration = [
                {"address": address, "width": width} for address, width in by_address.items()
            ]
        for left, left_width in by_address.items():
            for right, right_width in by_address.items():
                if left < right < left + left_width or right < left < right + right_width:
                    raise ValueError(
                        f"map block copy lifecycle restoration range overlap: {case['id']}"
                    )
        inputs.append(
            {
                **row,
                "layoutReadbackAddresses": addresses,
                "sentinelAddress": sentinel_address,
                "restorationPlan": restoration,
            }
        )
    return inputs


def _validate_instrumentation_plan(patch: dict[str, Any], rom_size: int) -> None:
    call = patch["updateCallSiteAddress"]
    original = bytes.fromhex(patch["updateCallOriginalHex"])
    patched = bytes.fromhex(patch["updateCallPatchedHex"])
    stub = patch["stubAddress"]
    stub_before = bytes.fromhex(patch["stubOriginalHex"])
    stub_bytes = bytes.fromhex(patch["stubHex"])
    if len(original) != len(patched) or len(stub_bytes) > len(stub_before):
        raise ValueError("map block copy lifecycle instrumentation span drift")
    spans = ((call, call + len(patched)), (stub, stub + len(stub_before)))
    if any(start < 0 or end > rom_size for start, end in spans) or (
        spans[0][0] < spans[1][1] and spans[1][0] < spans[0][1]
    ):
        raise ValueError("map block copy lifecycle instrumentation ROM overlap drift")
    probe_start = patch["generatedProbeAddress"]
    probe_end = probe_start + 14
    action_start = patch["actionScriptAddress"]
    action_end = action_start + 2
    if probe_start < action_end and action_start < probe_end:
        raise ValueError("map block copy lifecycle instrumentation generated-input overlap drift")


def _instrument_rom(rom_path: Path, static: dict[str, Any], fixture: dict[str, Any]) -> Path:
    original = rom_path.resolve(strict=True)
    original_hash = inspect_rom(original)["sha256"]
    data = bytearray(original.read_bytes())
    patch = static["instrumentation"]
    if fixture["instrumentation"] != patch:
        raise ValueError("map block copy lifecycle fixture instrumentation contract drift")
    _validate_instrumentation_plan(patch, len(data))
    call = patch["updateCallSiteAddress"]
    before = bytes.fromhex(patch["updateCallOriginalHex"])
    after = bytes.fromhex(patch["updateCallPatchedHex"])
    stub_address = patch["stubAddress"]
    stub_before = bytes.fromhex(patch["stubOriginalHex"])
    stub = bytes.fromhex(patch["stubHex"])
    if (
        data[call : call + len(before)] != before
        or data[stub_address : stub_address + len(stub_before)] != stub_before
    ):
        raise ValueError("map block copy lifecycle instrumentation source bytes drift")
    if (
        after[:6] != b"\x4e\xb9" + stub_address.to_bytes(4, "big")
        or after[6:] != b"\x4e\x71"
        or len(stub) > len(stub_before)
    ):
        raise ValueError("map block copy lifecycle instrumentation shape drift")
    if (
        patch["returnAddress"] != call + len(after)
        or patch["probeCallAddress"] != stub_address + 6
        or patch["probeReturnAddress"] != stub_address + 12
    ):
        raise ValueError("map block copy lifecycle instrumentation return seam drift")
    data[call : call + len(after)] = after
    data[stub_address : stub_address + len(stub)] = stub
    data[0x18E:0x190] = int(mega_drive_checksum(bytes(data)), 16).to_bytes(2, "big")
    if inspect_rom(original)["sha256"] != original_hash:
        raise ValueError("map block copy lifecycle altered canonical ROM")
    DERIVED_ROOT.mkdir(parents=True, exist_ok=True)
    output = DERIVED_ROOT / "map-block-copy-lifecycle.instrumented.bin"
    output.write_bytes(data)
    return output


def verify_map_block_copy_lifecycle(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 180
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner="map block copy lifecycle fixture")
    _validate_case_matrix(fixture)
    verify_runtime_contract(fixture, rom_path)
    static = build_map_block_copy_lifecycle_contract(rom_path, upstream_path)
    for key in ("function", "ram", "constants", "sourceFacts"):
        if fixture[key] != static[key]:
            raise ValueError(f"map block copy lifecycle static contract drift: {key}")
    if fixture["instrumentation"] != static["instrumentation"]:
        raise ValueError("map block copy lifecycle static contract drift: instrumentation")
    expected_records = _derive_cases(static, fixture)
    _validate_case_matrix(fixture)
    instrumented = _instrument_rom(rom_path, static, fixture)

    def observe() -> dict[str, Any]:
        return run_observer(
            rom_path=instrumented,
            observer_path=OBSERVER,
            config={
                "fixtureId": fixture["id"],
                "mapTestIndex": fixture["mapTestIndex"],
                "function": static["function"],
                "ram": static["ram"],
                "constants": static["constants"],
                "sourceFacts": static["sourceFacts"],
                "instrumentation": fixture["instrumentation"],
                "cases": _case_inputs(static, fixture),
                "maxFrames": fixture["maxFrames"],
                "harness": load_json(repo_path(fixture["sharedHarnessFixture"]))["harness"],
                "jsonModulePath": OBSERVER.with_name("json.lua").as_posix(),
                "observerFailureContract": OBSERVER_FAILURE_CONTRACT,
            },
            output_name=OUTPUT_NAME,
            timeout_seconds=timeout_seconds,
        )

    observed_path = DERIVED_ROOT / f"{OUTPUT_NAME}.observed.json"
    try:
        observed = _with_instrumented_rom_database(
            instrumented, "SF2 H3 instrumented map-block copy lifecycle", observe
        )
        assert_observer_status(
            DERIVED_ROOT / f"{OUTPUT_NAME}.status.txt",
            owner=OUTPUT_NAME,
            schema_path=FAILURE_SCHEMA,
            required_milestones=(
                "milestone:observer-ready",
                *(f"milestone:restored:{case_id}" for case_id in CASE_IDS),
            ),
        )
        validate_json(observed, OBSERVATION_SCHEMA, owner="map block copy lifecycle observation")
        expected = {
            "system": "GEN",
            "core": fixture["emulator"]["core"],
            "id": fixture["id"],
            "mapTest": fixture["mapTestIndex"],
            "recordOrder": [row["id"] for row in expected_records],
            "records": expected_records,
        }
        if observed != expected:
            raise ValueError(
                "map block copy lifecycle runtime matrix mismatch\n"
                f"expected={expected!r}\nobserved={observed!r}"
            )
    except Exception:
        observed_path.unlink(missing_ok=True)
        raise
    finally:
        instrumented.unlink(missing_ok=True)
    return {
        "Fixture": fixture["id"],
        "Cases": len(expected_records),
        "DispatcherCases": 4,
        "BizHawkLaunches": 1,
        "Instrumentation": "session-only",
        "Status": "PASS",
    }
