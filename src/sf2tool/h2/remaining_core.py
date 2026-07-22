from __future__ import annotations

import hashlib
import json
import re
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_ai import _parse_source_file
from sf2tool.h2.battle_scene_animations import _listing_address
from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.source_text import read_upstream_text

ID = "sf2-remaining-core-static-v1"
SOURCE_PATHS = (
    Path("code/romheader.asm"),
    Path("code/common/windows/windowengine.asm"),
    Path("code/gameflow/special/battletest.asm"),
    Path("code/gameflow/special/configurationmode.asm"),
    Path("code/gameflow/special/debugmodebattleactions.asm"),
)
WINDOW_SOURCE_PATH = Path("code/common/windows/windowengine.asm")
WINDOW_CONST_PATH = Path("sf2const.asm")
WINDOW_ENUM_PATH = Path("sf2enums.asm")
WINDOW_BLACK_BAR_COMPARE = (
    "cmpi.w  #VDPTILE_SCREEN_BLACK_BAR|VDPTILE_PALETTE3|VDPTILE_PRIORITY,(SPRITE_00_VDPTILE).l"
)
WINDOW_TILE_ADDRESS_FORMULA = (
    "layoutAddress plus (packedCoordinateYTimesWidthPlusPackedCoordinateX)TimesLayoutTileWordBytes"
)
WINDOW_MAP_LAYOUT_OFFSET_FORMULA = (
    "(packedCoordinateYTimesMapTileColumnsPlusPackedCoordinateXModuloMapTileColumns)"
    "TimesLayoutTileWordBytes"
)
WINDOW_FUNCTIONS = (
    "InitializeWindowProperties",
    "CreateWindow",
    "SetWindowDestination",
    "FixWindowsPositions",
    "sub_48BE",
    "CopyPlaneALayoutForWindows",
    "MoveWindowWithSfx",
    "MoveWindow",
    "DeleteWindow",
    "WaitForWindowMovementEnd",
    "VInt_UpdateWindows",
    "sub_4AC8",
    "sub_4B5C",
    "sub_4BEA",
    "GetWindowEntryAddress",
    "GetWindowTileAddress",
)
MANIFEST = repo_path("manifests/extractions/remaining-core-static.json")
SCHEMA = repo_path("schemas/remaining-core-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/remaining-core-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-remaining-core-static-fixture.schema.json")
RESEARCH_INDEX = repo_path("manifests/research-index.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _require_fragments(source: str, fragments: tuple[str, ...], owner: str) -> None:
    missing = [fragment for fragment in fragments if fragment not in source]
    if missing:
        raise ValueError(f"{owner} source-shape drift: missing {missing}")


def _window_section(source: str, name: str) -> str:
    start = source.find(f"{name}:")
    end = source.find(f"; End of function {name}", start)
    if start < 0 or end < 0:
        raise ValueError(f"window section boundary drift: {name}")
    return source[start:end]


def _require_window_section(source: str, name: str, fragments: tuple[str, ...]) -> None:
    section = _window_section(source, name)
    missing = [fragment for fragment in fragments if fragment not in section]
    if missing:
        raise ValueError(f"window section semantic drift at {name}: {missing}")


def _require_window_ordered_section(source: str, name: str, fragments: tuple[str, ...]) -> None:
    section = _window_section(source, name)
    position = 0
    for fragment in fragments:
        position = section.find(fragment, position)
        if position < 0:
            raise ValueError(f"window ordered section semantic drift at {name}: {fragment}")
        position += len(fragment)


def _read_window_equ_values(path: Path, names: tuple[str, ...]) -> dict[str, int]:
    source = read_upstream_text(path)
    values: dict[str, int] = {}
    for name in names:
        match = re.search(
            rf"^{re.escape(name)}:\s+equ\s+(\$[0-9A-Fa-f]+|-?\d+)",
            source,
            re.MULTILINE,
        )
        if not match:
            raise ValueError(f"missing window constant: {name}")
        raw = match.group(1)
        values[name] = int(raw[1:], 16) if raw.startswith("$") else int(raw)
    return values


_WINDOW_CALL_PATTERN = re.compile(
    r"^\s*(?:[A-Za-z_][A-Za-z0-9_]*:\s*)?(?:bsr|jsr)(?:\.[bswl])?\s+([^\s,;]+)\s*$",
    re.IGNORECASE,
)
_WINDOW_LONGWORD_PATTERN = re.compile(
    r"^\s*(?:[A-Za-z_][A-Za-z0-9_]*:\s*)?dc\.l\s+([^\s,;]+)\s*$",
    re.IGNORECASE,
)
_WINDOW_DIRECT_TARGET_PATTERN = re.compile(r"^[A-Za-z_][A-Za-z0-9_]*$")
_WINDOW_REGISTER_TARGETS = {
    *(f"a{index}" for index in range(8)),
    *(f"d{index}" for index in range(8)),
    "sp",
    "pc",
}


def _window_direct_call_counts(path: Path, targets: set[str]) -> dict[str, int]:
    """Count only direct bsr/jsr instruction fields after stripping ASM comments."""
    counts: Counter[str] = Counter()
    for raw_line in read_upstream_text(path).splitlines():
        line = raw_line.split(";", 1)[0]
        match = _WINDOW_CALL_PATTERN.match(line)
        if not match:
            continue
        operand = re.sub(r"\.[bwl]$", "", match.group(1), flags=re.IGNORECASE)
        if operand.startswith("(") and operand.endswith(")"):
            operand = operand[1:-1]
        if (
            not _WINDOW_DIRECT_TARGET_PATTERN.fullmatch(operand)
            or operand.lower() in _WINDOW_REGISTER_TARGETS
            or operand not in targets
        ):
            continue
        counts[operand] += 1
    return dict(sorted(counts.items()))


def _window_longword_pointer_counts(path: Path, targets: set[str]) -> dict[str, int]:
    """Count only direct dc.l target operands; these are not call-site evidence."""
    counts: Counter[str] = Counter()
    for raw_line in read_upstream_text(path).splitlines():
        match = _WINDOW_LONGWORD_PATTERN.match(raw_line.split(";", 1)[0])
        if match and match.group(1) in targets:
            counts[match.group(1)] += 1
    return dict(sorted(counts.items()))


def _window_doubling_scale(section: str, owner: str) -> int:
    matches = re.findall(r"^\s*add\.w\s+(d[0-7]),(d[0-7])\s*$", section, re.MULTILINE)
    self_additions = [operands for operands in matches if operands[0] == operands[1]]
    if len(self_additions) != 1:
        raise ValueError(f"window {owner} doubling source drift")
    return 1 << len(self_additions)


def _header_facts(source: str) -> dict[str, Any]:
    vectors = source.split("aSegaGenesis:", 1)[0]
    vector_count = len(
        re.findall(r"^\s*(?:[A-Za-z_][A-Za-z0-9_]*:\s*)?dc\.l\s+", vectors, re.MULTILINE)
    )
    if vector_count != 64:
        raise ValueError("ROM vector table count drift")
    _require_fragments(
        source,
        (
            "dc.l HInt",
            "dc.l VInt",
            "dc.l Trap0_SoundCommand",
            "dc.l Trap9_ManageContextualFunctions",
            "dc.b 'GM MK-1315 -00'",
            "dc.w $8921",
            "dc.l $1FFFFF",
            "dc.l $200001",
            "dc.l $203FFF",
            "dc.b 'U               '",
        ),
        "ROM header",
    )
    return {
        "vectorEntryCount": vector_count,
        "horizontalInterruptLevel": 4,
        "verticalInterruptLevel": 6,
        "namedTrapRange": [0, 9],
        "productCode": "GM MK-1315 -00",
        "headerChecksum": 35105,
        "romEndAddress": 2097151,
        "sramStartAddress": 2097153,
        "sramEndAddress": 2113535,
        "regionCode": "U",
    }


def _window_facts(disasm: Path, listing: str) -> dict[str, Any]:
    source_path = disasm / WINDOW_SOURCE_PATH
    source = read_upstream_text(source_path)
    constants = _read_window_equ_values(
        disasm / WINDOW_CONST_PATH,
        (
            "WINDOW_LAYOUTS_END_POINTER",
            "WINDOW_ENTRIES",
            "FIX_WINDOWS_POSITIONS_TOGGLE",
            "UPDATE_WINDOWS_TOGGLE",
            "MOVING_WINDOWS_BITFIELD",
            "DIALOGUE_WINDOW_INDEX",
            "PORTRAIT_WINDOW_INDEX",
            "TIMER_WINDOW_INDEX",
            "SPECIAL_TURBO_TOGGLE",
            "WINDOW_IS_PRESENT",
            "HIDE_WINDOWS_TOGGLE",
            "WINDOW_TILE_LAYOUTS",
            "PLANE_A_MAP_LAYOUT",
            "PLANE_A_MAP_AND_WINDOWS_LAYOUT",
        ),
    )
    enum_values = _read_window_equ_values(
        disasm / WINDOW_ENUM_PATH,
        (
            "BYTE_SHIFT_COUNT",
            "NIBBLE_SHIFT_COUNT",
            "BYTE_MASK",
            "WINDOW_ENTRIES_COUNTER",
            "WINDOW_ENTRY_SIZE",
            "WINDOW_ENTRIES_LONGWORD_COUNTER",
            "WINDOWDEF_OFFSET_ACTIVE",
            "WINDOWDEF_OFFSET_LAYOUT_ADDRESS",
            "WINDOWDEF_OFFSET_WIDTH",
            "WINDOWDEF_OFFSET_HEIGHT",
            "WINDOWDEF_OFFSET_X",
            "WINDOWDEF_OFFSET_Y",
            "WINDOWDEF_OFFSET_ANIM_ORIG_X",
            "WINDOWDEF_OFFSET_ANIM_ORIG_Y",
            "WINDOWDEF_OFFSET_ANIM_DEST_X",
            "WINDOWDEF_OFFSET_ANIM_DEST_Y",
            "WINDOWDEF_OFFSET_ANIM_LENGTH",
            "WINDOWDEF_OFFSET_ANIM_COUNTER",
            "NEXT_WINDOWDEF",
            "MAP_CURRENT",
            "SFX_MENU_SWITCH",
        ),
    )
    slot_count = enum_values["WINDOW_ENTRIES_COUNTER"] + 1
    entry_size = enum_values["WINDOW_ENTRY_SIZE"]
    clear_longword_count = enum_values["WINDOW_ENTRIES_LONGWORD_COUNTER"] + 1
    clear_span = clear_longword_count * 4
    if clear_span != slot_count * entry_size:
        raise ValueError("window reset clear span disagrees with slot and entry constants")
    if enum_values["NEXT_WINDOWDEF"] != entry_size:
        raise ValueError("window next-entry stride disagrees with entry size")
    create_section = _window_section(source, "CreateWindow")
    tile_address_section = _window_section(source, "GetWindowTileAddress")
    create_layout_scale = _window_doubling_scale(create_section, "CreateWindow")
    tile_address_scale = _window_doubling_scale(tile_address_section, "GetWindowTileAddress")
    if create_layout_scale != tile_address_scale:
        raise ValueError("window layout scale disagrees between create and tile lookup")
    layout_word_bytes = create_layout_scale
    map_offset_section = _window_section(source, "sub_4BEA")
    composition_section = _window_section(source, "sub_4AC8")
    map_shift_match = re.search(r"asl\.w\s+#(\d+),d6", map_offset_section)
    map_mask_match = re.search(r"andi\.w\s+#\$([0-9A-Fa-f]+),d1", map_offset_section)
    vertical_limit_match = re.search(r"cmpi\.w\s+#\$([0-9A-Fa-f]+),d3", composition_section)
    horizontal_limit_match = re.search(r"cmpi\.w\s+#\$([0-9A-Fa-f]+),d2", composition_section)
    if (
        not map_shift_match
        or not map_mask_match
        or not vertical_limit_match
        or not horizontal_limit_match
    ):
        raise ValueError("window map-coordinate source constants drift")
    map_tile_columns = 1 << int(map_shift_match.group(1))
    map_x_mask = int(map_mask_match.group(1), 16)
    map_vertical_exclusive = int(vertical_limit_match.group(1), 16)
    map_horizontal_exclusive = int(horizontal_limit_match.group(1), 16)
    if map_horizontal_exclusive != map_tile_columns:
        raise ValueError("window map-coordinate shift and horizontal bound disagree")
    if map_x_mask + 1 != map_tile_columns:
        raise ValueError("window map-coordinate mask and column count disagree")
    if 1 << enum_values["NIBBLE_SHIFT_COUNT"] != entry_size:
        raise ValueError("window entry address shift and entry size disagree")
    map_row_stride_bytes = map_tile_columns * layout_word_bytes
    function_entries = {name: _listing_address(listing, name) for name in WINDOW_FUNCTIONS}

    _require_window_ordered_section(
        source,
        "InitializeWindowProperties",
        (
            "lea     (WINDOW_ENTRIES).l,a0",
            "moveq   #WINDOW_ENTRIES_LONGWORD_COUNTER,d7",
            "clr.l   (a0)+",
            "dbf     d7,@Clear_Loop",
            "move.l  #WINDOW_TILE_LAYOUTS,((WINDOW_LAYOUTS_END_POINTER-$1000000)).w",
            "clr.b   ((WINDOW_IS_PRESENT-$1000000)).w",
            "cmpi.b  #MAP_CURRENT,((CURRENT_MAP-$1000000)).w",
            "beq.s   @Continue",
            "addq.b  #1,((WINDOW_IS_PRESENT-$1000000)).w",
            "clr.w   ((PORTRAIT_WINDOW_INDEX-$1000000)).w",
            "clr.w   ((DIALOGUE_WINDOW_INDEX-$1000000)).w",
            "clr.w   ((TIMER_WINDOW_INDEX-$1000000)).w",
        ),
    )
    _require_window_ordered_section(
        source,
        "CreateWindow",
        (
            "moveq   #WINDOW_ENTRIES_COUNTER,d7",
            "tst.w   (a0)",
            "beq.s   @Found",
            "adda.w  #WINDOW_ENTRY_SIZE,a0",
            "dbf     d7,FindFreeWindowSlot_Loop",
            "moveq   #-1,d0",
            "bra.w   @Done",
            "movea.l ((WINDOW_LAYOUTS_END_POINTER-$1000000)).w,a1",
            "cmpa.l  #WINDOW_TILE_LAYOUTS,a1",
            "bne.s   @Continue",
            "bsr.w   CopyPlaneALayoutForWindows",
            "move.l  a1,(a0)+",
            "move.w  d0,(a0)+",
            "move.w  d1,(a0)+",
            "move.w  d1,(a0)+",
            "move.w  d1,(a0)+",
            "move.w  #$101,(a0)+",
            "clr.w   (a0)+",
            "lsr.w   #BYTE_SHIFT_COUNT,d7",
            "andi.w  #BYTE_MASK,d0",
            "mulu.w  d7,d0",
            "add.w   d0,d0",
            "adda.w  d0,a1",
            "move.l  a1,((WINDOW_LAYOUTS_END_POINTER-$1000000)).w",
        ),
    )
    _require_window_ordered_section(
        source,
        "SetWindowDestination",
        (
            "bsr.w   GetWindowEntryAddress",
            "tst.l   (a0)",
            "beq.w   loc_4898",
            "move.w  WINDOWDEF_OFFSET_X(a0),d0",
            "cmp.w   WINDOWDEF_OFFSET_ANIM_DEST_X(a0),d0",
            "bne.w   loc_4898",
            "cmpi.w  #$8080,d1",
            "bne.s   loc_488A",
            "move.w  d0,d1",
            "move.w  d1,WINDOWDEF_OFFSET_ANIM_ORIG_X(a0)",
            "move.w  d1,WINDOWDEF_OFFSET_ANIM_DEST_X(a0)",
            "move.w  #256,WINDOWDEF_OFFSET_ANIM_LENGTH(a0)",
        ),
    )
    _require_window_ordered_section(
        source,
        "FixWindowsPositions",
        (
            "bsr.w   CopyPlaneALayoutForWindows",
            "clr.w   d0",
            "move.w  #$8080,d1",
            "moveq   #7,d7",
            "bsr.s   SetWindowDestination",
            "addq.w  #1,d0",
            "dbf     d7,@Loop",
        ),
    )
    _require_window_section(source, "CopyPlaneALayoutForWindows", ("#$800,d7", "bsr.w   CopyBytes"))
    move_with_sfx = _window_section(source, "MoveWindowWithSfx")
    move_with_sfx_end = source.find("; End of function MoveWindowWithSfx")
    move_window_start = source.find("MoveWindow:", move_with_sfx_end)
    between_entries = source[move_with_sfx_end:move_window_start]
    control_transfer = re.search(
        r"^\s*(?:b[a-z]+(?:\.[bswl])?|j(?:mp|sr)|rts|rte|trap|dbf)\b",
        move_with_sfx,
        re.IGNORECASE | re.MULTILINE,
    )
    if (
        "sndCom  SFX_MENU_SWITCH" not in move_with_sfx
        or move_window_start < 0
        or control_transfer
        or re.sub(r"(?:\s|;[^\r\n]*)+", "", between_entries)
    ):
        raise ValueError("window MoveWindowWithSfx fallthrough shape drift")
    _require_window_ordered_section(
        source,
        "MoveWindow",
        (
            "tst.b   ((SPECIAL_TURBO_TOGGLE-$1000000)).w",
            "beq.s   loc_4900",
            "moveq   #1,d2",
            "bsr.w   GetWindowEntryAddress",
            "cmpi.w  #$8080,d1",
            "bne.s   loc_4914",
            "move.w  WINDOWDEF_OFFSET_X(a0),d1",
            "move.w  WINDOWDEF_OFFSET_X(a0),WINDOWDEF_OFFSET_ANIM_ORIG_X(a0)",
            "move.w  d1,WINDOWDEF_OFFSET_ANIM_DEST_X(a0)",
            "move.b  d2,WINDOWDEF_OFFSET_ANIM_LENGTH(a0)",
            "clr.b   WINDOWDEF_OFFSET_ANIM_COUNTER(a0)",
        ),
    )
    _require_window_ordered_section(
        source,
        "DeleteWindow",
        (
            "bsr.w   GetWindowEntryAddress",
            "clr.l   (a0)",
            "moveq   #WINDOW_ENTRIES_COUNTER,d0",
            "move.l  (a0),d2",
            "cmp.l   d1,d2",
            "bls.s   @Next",
            "move.l  d2,d1",
            "move.b  WINDOWDEF_OFFSET_WIDTH(a0),d3",
            "move.b  WINDOWDEF_OFFSET_HEIGHT(a0),d4",
            "lea     NEXT_WINDOWDEF(a0),a0",
            "dbf     d0,@ClearWindow_Loop",
            "tst.l   d1",
            "bne.s   @Continue",
            "move.l  #WINDOW_TILE_LAYOUTS,d1",
            "bra.s   @UpdateEndPointer",
            "mulu.w  d4,d3",
            "add.w   d3,d3",
            "ext.l   d3",
            "add.l   d3,d1",
            "move.l  d1,((WINDOW_LAYOUTS_END_POINTER-$1000000)).w",
        ),
    )
    _require_window_ordered_section(
        source,
        "WaitForWindowMovementEnd",
        (
            "bsr.w   WaitForVInt",
            "tst.b   ((MOVING_WINDOWS_BITFIELD-$1000000)).w",
            "bne.s   WaitForWindowMovementEnd",
        ),
    )
    _require_window_ordered_section(
        source,
        "VInt_UpdateWindows",
        (
            "cmpi.l  #WINDOW_TILE_LAYOUTS,((WINDOW_LAYOUTS_END_POINTER-$1000000)).w",
            "bne.s   loc_4994",
            "rts",
            "clr.b   ((MOVING_WINDOWS_BITFIELD-$1000000)).w",
            "tst.l   (a2)",
            "beq.w   @NextWindow",
            "move.b  WINDOWDEF_OFFSET_ANIM_LENGTH(a2),d0",
            "cmp.b   WINDOWDEF_OFFSET_ANIM_COUNTER(a2),d0",
            "beq.w   @NextWindow",
            "bset    d0,((MOVING_WINDOWS_BITFIELD-$1000000)).w",
            "bsr.w   sub_4B5C",
            "tst.l   (a2)",
            "beq.w   loc_4A72",
            "move.b  WINDOWDEF_OFFSET_ANIM_LENGTH(a2),d0",
            "cmp.b   WINDOWDEF_OFFSET_ANIM_COUNTER(a2),d0",
            "beq.w   loc_4A40",
            "addq.b  #1,WINDOWDEF_OFFSET_ANIM_COUNTER(a2)",
            "muls.w  d5,d1",
            "divs.w  d6,d1",
            "muls.w  d5,d2",
            "divs.w  d6,d2",
            "lsl.w   #BYTE_SHIFT_COUNT,d1",
            "andi.w  #BYTE_MASK,d2",
            "or.w    d2,d1",
            "tst.b   ((HIDE_WINDOWS_TOGGLE-$1000000)).w",
            "bne.s   loc_4A40",
            "bsr.w   sub_4AC8",
            "tst.b   $E(a2)",
            "beq.s   loc_4A5A",
            "clr.b   $E(a2)",
            "movea.l (a2),a0",
            "move.w  WINDOWDEF_OFFSET_WIDTH(a2),d0",
            "move.w  WINDOWDEF_OFFSET_X(a2),d1",
            "bsr.w   sub_4AC8",
            "bra.s   loc_4A72",
            "tst.b   $F(a2)",
            "beq.s   loc_4A72",
            "clr.b   $F(a2)",
            "movea.l (a2),a0",
            "move.w  WINDOWDEF_OFFSET_WIDTH(a2),d0",
            "move.w  WINDOWDEF_OFFSET_X(a2),d1",
            "bsr.w   sub_4B5C",
            "tst.b   ((HIDE_WINDOWS_TOGGLE-$1000000)).w",
            "beq.s   loc_4A92",
            "tst.b   ((FIX_WINDOWS_POSITIONS_TOGGLE-$1000000)).w",
            "bne.s   loc_4A90",
            "bsr.w   CopyPlaneALayoutForWindows",
            "move.w  #-1,((FIX_WINDOWS_POSITIONS_TOGGLE-$1000000)).w",
            "bra.s   loc_4AA2",
            "tst.b   ((FIX_WINDOWS_POSITIONS_TOGGLE-$1000000)).w",
            "beq.s   loc_4AA2",
            "bsr.w   FixWindowsPositions",
            "move.w  #$FF,((FIX_WINDOWS_POSITIONS_TOGGLE-$1000000)).w",
            "tst.b   ((UPDATE_WINDOWS_TOGGLE-$1000000)).w",
            "beq.s   @Return",
            "lea     (PLANE_A_MAP_AND_WINDOWS_LAYOUT).l,a0",
            "lea     ($C000).l,a1",
            "move.w  #$400,d0",
            "moveq   #2,d1",
            "bsr.w   ApplyVIntVramDma",
            "bsr.w   EnableDmaQueueProcessing",
            "clr.b   ((UPDATE_WINDOWS_TOGGLE-$1000000)).w",
        ),
    )
    for name, operation in (
        ("sub_4AC8", "move.w  d5,(a1,d6.w)"),
        ("sub_4B5C", "move.w  (a0,d6.w),(a1,d6.w)"),
    ):
        _require_window_ordered_section(
            source,
            name,
            (
                "tst.w   d3",
                "bpl.s   loc_4AFA" if name == "sub_4AC8" else "bmi.w   loc_4BC6",
                "cmpi.w  #$1C,d3",
                "bge.w   loc_4B46" if name == "sub_4AC8" else "bge.w   loc_4BD4",
                "tst.w   d2",
                "bmi.w   loc_4B1C" if name == "sub_4AC8" else "bmi.w   loc_4BAC",
                "cmpi.w  #$20,d2",
                "bge.w   loc_4B1C" if name == "sub_4AC8" else "bge.w   loc_4BAC",
                operation,
                "tst.b   ((HIDE_WINDOWS_TOGGLE-$1000000)).w",
                "bne.s   loc_4B52" if name == "sub_4AC8" else "bne.s   loc_4BE0",
                "move.b  #-1,((UPDATE_WINDOWS_TOGGLE-$1000000)).w",
            ),
        )
    _require_window_ordered_section(
        source,
        "sub_4BEA",
        (
            "asr.w   #BYTE_SHIFT_COUNT,d1",
            "andi.w  #$1F,d1",
            "asl.w   #5,d6",
            "add.w   d1,d6",
            "add.w   d6,d6",
            WINDOW_BLACK_BAR_COMPARE,
        ),
    )
    _require_window_ordered_section(
        source,
        "GetWindowEntryAddress",
        (
            "lsl.w   #NIBBLE_SHIFT_COUNT,d0",
            "lea     (WINDOW_ENTRIES).l,a0",
            "adda.w  d0,a0",
        ),
    )
    _require_window_ordered_section(
        source,
        "GetWindowTileAddress",
        (
            "bsr.s   GetWindowEntryAddress",
            "movea.l (a0)+,a1",
            "move.b  (a0),d0",
            "move.b  d1,d2",
            "lsr.w   #BYTE_SHIFT_COUNT,d1",
            "mulu.w  d2,d0",
            "add.w   d1,d0",
            "add.w   d0,d0",
            "adda.w  d0,a1",
        ),
    )

    callers: dict[str, dict[str, int]] = {}
    pointer_references: dict[str, dict[str, int]] = {}
    targets = set(WINDOW_FUNCTIONS)
    for path in sorted((disasm / "code").rglob("*.asm"), key=lambda item: item.as_posix()):
        if path.relative_to(disasm) == WINDOW_SOURCE_PATH:
            continue
        counts = _window_direct_call_counts(path, targets)
        if counts:
            callers[path.relative_to(disasm).as_posix()] = counts
        pointer_counts = _window_longword_pointer_counts(path, targets)
        if pointer_counts:
            pointer_references[path.relative_to(disasm).as_posix()] = pointer_counts
    external_counts = {
        target: sum(counts.get(target, 0) for counts in callers.values())
        for target in WINDOW_FUNCTIONS
    }
    internal_counts = _window_direct_call_counts(source_path, targets)

    return {
        "sourcePath": WINDOW_SOURCE_PATH.as_posix(),
        "sourceLineCount": len(source.splitlines()),
        "functionEntries": function_entries,
        "sourceLabels": {"addresses": constants, "enumValues": enum_values},
        "derived": {
            "windowSlotCount": slot_count,
            "entrySizeBytes": entry_size,
            "entryAddressShiftBits": enum_values["NIBBLE_SHIFT_COUNT"],
            "clearLongwordCount": clear_longword_count,
            "clearSpanBytes": clear_span,
            "layoutTileWordBytes": layout_word_bytes,
            "mapTileColumns": map_tile_columns,
            "mapCoordinateXMask": map_x_mask,
            "mapRowStrideBytes": map_row_stride_bytes,
            "coordinateXShiftBits": enum_values["BYTE_SHIFT_COUNT"],
            "coordinateYMask": enum_values["BYTE_MASK"],
        },
        "entryLayout": {
            "fields": [
                {
                    "offsetBytes": 0,
                    "widthBytes": 4,
                    "sourceAccess": "(a0)",
                    "role": "layoutAddressAndActiveTest",
                },
                {
                    "offsetBytes": 4,
                    "widthBytes": 1,
                    "sourceAccess": "WINDOWDEF_OFFSET_WIDTH",
                    "role": "width",
                },
                {
                    "offsetBytes": 5,
                    "widthBytes": 1,
                    "sourceAccess": "WINDOWDEF_OFFSET_HEIGHT",
                    "role": "height",
                },
                {
                    "offsetBytes": 6,
                    "widthBytes": 2,
                    "sourceAccess": "WINDOWDEF_OFFSET_X",
                    "role": "packedPosition",
                },
                {
                    "offsetBytes": 8,
                    "widthBytes": 2,
                    "sourceAccess": "WINDOWDEF_OFFSET_ANIM_ORIG_X",
                    "role": "packedAnimationOrigin",
                },
                {
                    "offsetBytes": 10,
                    "widthBytes": 2,
                    "sourceAccess": "WINDOWDEF_OFFSET_ANIM_DEST_X",
                    "role": "packedAnimationDestination",
                },
                {
                    "offsetBytes": 12,
                    "widthBytes": 1,
                    "sourceAccess": "WINDOWDEF_OFFSET_ANIM_LENGTH",
                    "role": "animationLength",
                },
                {
                    "offsetBytes": 13,
                    "widthBytes": 1,
                    "sourceAccess": "WINDOWDEF_OFFSET_ANIM_COUNTER",
                    "role": "animationCounter",
                },
                {
                    "offsetBytes": 14,
                    "widthBytes": 1,
                    "sourceAccess": "$E(a2)",
                    "role": "unlabeledByte14",
                },
                {
                    "offsetBytes": 15,
                    "widthBytes": 1,
                    "sourceAccess": "$F(a2)",
                    "role": "unlabeledByte15",
                },
            ],
            "packedSize": {"widthSourceByteOffset": 4, "heightSourceByteOffset": 5},
            "packedCoordinate": {
                "xShiftBits": enum_values["BYTE_SHIFT_COUNT"],
                "yMask": enum_values["BYTE_MASK"],
            },
        },
        "operations": {
            "initialize": {
                "clearsEntriesBeforeResettingLayoutEnd": True,
                "layoutEndAddress": constants["WINDOW_TILE_LAYOUTS"],
                "windowIsPresentWhenCurrentMapDiffersFromMapCurrent": True,
                "resetsWindowIndices": [
                    "PORTRAIT_WINDOW_INDEX",
                    "DIALOGUE_WINDOW_INDEX",
                    "TIMER_WINDOW_INDEX",
                ],
            },
            "create": {
                "freeSlotUsesZeroLowWordTest": True,
                "failureSentinel": -1,
                "copiesPlaneALayoutOnlyAtInitialLayoutEnd": True,
                "layoutBytesFormula": "widthTimesHeightTimesLayoutTileWordBytes",
                "initialPackedAnimationLengthAndCounter": [1, 1],
                "clearsUnlabeledBytes14And15": True,
            },
            "setDestination": {
                "requiresActiveEntryAndNoCurrentMovement": True,
                "sentinelPackedCoordinate": 32896,
                "sentinelUsesCurrentPackedPosition": True,
                "writesOriginThenDestinationThenAnimationLength": True,
                "animationLength": 256,
            },
            "fixPositions": {
                "copiesPlaneABeforeIteratingSlots": True,
                "iteratesAllSlotsInAscendingOrder": True,
                "usesSetDestinationSentinel": True,
            },
            "move": {
                "specialTurboForcesAnimationLength": 1,
                "sentinelPackedCoordinate": 32896,
                "writesOriginThenDestinationThenLengthThenClearsCounter": True,
            },
            "moveWithSfx": {
                "sourceMacro": "sndCom SFX_MENU_SWITCH",
                "fallsThroughToMoveWindow": True,
            },
            "delete": {
                "clearsSelectedEntryLayoutAddressBeforeScan": True,
                "recomputesLayoutEndFromHighestRemainingLayoutAddress": True,
                "noRemainingEntryResetsLayoutEndToBase": True,
                "layoutBytesFormula": "widthTimesHeightTimesLayoutTileWordBytes",
            },
            "wait": {
                "waitsForVIntBeforeTestingMovingBitfield": True,
                "repeatsWhileAnyMovingBitIsSet": True,
            },
            "vint": {
                "returnsWhenLayoutEndEqualsBase": True,
                "clearsMovingBitfieldBeforeFirstSlotPass": True,
                "firstPassRestoresMovingWindowsBeforeInterpolation": True,
                "secondPassUsesSignedIntegerLinearInterpolation": True,
                "hidingSkipsImmediateComposition": True,
                "byte14TriggersCompositionThenClears": True,
                "byte15TriggersRestoreThenClears": True,
                "hideFixTransitionRestoresPlaneABeforeMarkingFix": True,
                "showFixTransitionCallsFixWindowsPositions": True,
                "updateToggleQueuesPlaneAThenEnablesQueueProcessingThenClearsToggle": True,
                "queuedDmaDestinationArgument": 49152,
                "queuedDmaLengthArgument": 1024,
                "queuedDmaIncrementArgument": 2,
            },
            "composition": {
                "composeFunction": "sub_4AC8",
                "restoreFunction": "sub_4B5C",
                "mapCoordinateHorizontalExclusive": map_horizontal_exclusive,
                "mapCoordinateVerticalExclusive": map_vertical_exclusive,
                "setsUpdateToggleWhenNotHidden": True,
            },
        },
        "addressFormulas": {
            "windowEntryAddress": "WINDOW_ENTRIES plus slotIndexTimesEntrySizeBytes",
            "windowTileAddress": WINDOW_TILE_ADDRESS_FORMULA,
            "mapLayoutOffset": WINDOW_MAP_LAYOUT_OFFSET_FORMULA,
        },
        "internalDirectCallSiteCounts": internal_counts,
        "externalDirectCallerOccurrences": callers,
        "externalDirectCallSiteCounts": external_counts,
        "externalLongwordPointerOccurrences": pointer_references,
        "indirectBehavior": {
            "longwordPointerReferencesAreNotDirectCallSites": True,
            "zeroDirectCallerCountDoesNotEstablishUnreachability": True,
        },
        "runtimeQuestions": ["window-presentation-matrix-animation-hide-fix-scroll-clip-and-dma"],
    }


def _debug_facts(sources: dict[str, str]) -> dict[str, Any]:
    battle = sources["code/gameflow/special/battletest.asm"]
    config = sources["code/gameflow/special/configurationmode.asm"]
    actions = sources["code/gameflow/special/debugmodebattleactions.asm"]
    joined_allies = re.findall(r"moveq\s+#(ALLY_[A-Z0-9_]+),d0\s+bsr\.w\s+j_JoinForce", battle)
    if len(joined_allies) != 29:
        raise ValueError("debug battle-test force roster drift")
    action_table = re.search(
        r"rjt_DebugModeBattleactions:(?P<body>.*?)(?=^@Attack:)", actions, re.MULTILINE | re.DOTALL
    )
    if not action_table:
        raise ValueError("debug battle-action table is missing")
    action_targets = re.findall(r"dc\.w\s+@([A-Za-z0-9_]+)-", action_table.group("body"))
    _require_fragments(
        battle,
        (
            "move.w  #99,d1",
            "#BATTLES_DEBUG_MAX_INDEX",
            "#SHOPS_DEBUG_MAX_INDEX",
            "LevelUpWholeForce:",
        ),
        "battle test",
    )
    _require_fragments(
        config,
        (
            "SPECIAL_TURBO_TOGGLE",
            "CONTROL_OPPONENT_TOGGLE",
            "AUTO_BATTLE_TOGGLE",
            "bset    #7,(SAVE_FLAGS).l",
            "bclr    #7,(SAVE_FLAGS).l",
            "j_SoundTest",
        ),
        "configuration mode",
    )
    _require_fragments(
        actions,
        (
            "seq     debugDodge(a2)",
            "seq     debugCritical(a2)",
            "seq     debugDouble(a2)",
            "seq     debugCounter(a2)",
        ),
        "debug hit selection",
    )
    return {
        "battleTestJoinedAllyCount": len(joined_allies),
        "battleTestWholeForceCount": 30,
        "battleTestMaxBattleIndex": 49,
        "battleTestMaxShopIndex": 100,
        "battleTestBowieStatValue": 99,
        "configurationToggleCount": 4,
        "configurationToggles": [
            "special-turbo",
            "control-opponent",
            "auto-battle",
            "game-completed",
        ],
        "soundTestRequiresStartUpAndCompletedFlag": True,
        "debugBattleActionTargets": action_targets,
        "debugBattleActionCount": len(action_targets),
        "debugHitOverrideCount": 4,
        "debugHitOverrides": ["dodge", "critical", "double", "counter"],
    }


def build_remaining_core_inventory(upstream_path: Path) -> dict[str, Any]:
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"remaining core H1 listing is missing: {listing_path}")
    listing = listing_path.read_text(encoding="utf-8")
    paths = [disasm / path for path in SOURCE_PATHS]
    if not all(path.is_file() for path in paths):
        raise ValueError("remaining core source boundary is incomplete")
    files = [_parse_source_file(path, path.relative_to(disasm).as_posix()) for path in paths]
    layout = "\n".join(
        path.read_text(encoding="utf-8") for path in sorted((disasm / "layout").glob("*.asm"))
    )
    for row in files:
        if row["path"].replace("/", "\\") not in layout:
            raise ValueError(f"remaining core source is absent from layout: {row['path']}")
        if not row["globalLabels"]:
            raise ValueError(f"unexpected unlabeled remaining core file: {row['path']}")
    representative_symbols = {row["path"]: row["globalLabels"][0] for row in files}
    representative_addresses = {
        symbol: _listing_address(listing, symbol) for symbol in representative_symbols.values()
    }
    source_paths = {path.as_posix() for path in SOURCE_PATHS}
    records = [
        record
        for record in load_json(RESEARCH_INDEX)["records"]
        if record["sourcePath"] in source_paths
    ]
    labels = {label for row in files for label in row["globalLabels"]}
    calls: Counter[str] = Counter()
    for row in files:
        for call in row["directCalls"]:
            calls[call["target"]] += call["siteCount"]
    sources = {path.as_posix(): read_upstream_text(disasm / path) for path in SOURCE_PATHS}
    summary = {
        "fileCount": len(files),
        "sourceLineCount": sum(row["sourceLineCount"] for row in files),
        "statementCount": sum(row["statementCount"] for row in files),
        "globalLabelCount": sum(len(row["globalLabels"]) for row in files),
        "localLabelCount": sum(row["localLabelCount"] for row in files),
        "directCallSiteCount": sum(calls.values()),
        "uniqueDirectTargetCount": len(calls),
        "internalDirectTargetCount": sum(target in labels for target in calls),
        "externalDirectTargetCount": sum(target not in labels for target in calls),
        "layoutIncludedFileCount": len(files),
        "indexedRecordCount": len(records),
        "indexedFileCount": len({record["sourcePath"] for record in records}),
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "scopes": [path.as_posix() for path in SOURCE_PATHS],
        "summary": summary,
        "indexedRecordIds": sorted(record["id"] for record in records),
        "indexedSourcePaths": sorted({record["sourcePath"] for record in records}),
        "representativeSymbols": representative_symbols,
        "representativeAddresses": representative_addresses,
        "internalDirectCallTargets": sorted(target for target in calls if target in labels),
        "externalDirectCallTargets": sorted(target for target in calls if target not in labels),
        "headerFacts": _header_facts(sources["code/romheader.asm"]),
        "windowFacts": _window_facts(disasm, listing),
        "debugFacts": _debug_facts(sources),
        "runtimeQuestions": [
            "window-animation-hide-scroll-and-dma-frames",
            "debug-configuration-input-and-menu-presentation",
        ],
        "files": files,
    }


def verify_remaining_core_inventory(
    upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_remaining_core_inventory(upstream_path)
    validate_json(output, SCHEMA, owner="remaining core static inventory")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != load_json(ROM_MANIFEST)["hashes"]["sha256"]
    ):
        raise ValueError("remaining core provenance drift")
    if output["summary"] != manifest["summary"]:
        raise ValueError("remaining core summary drift")
    if output["representativeAddresses"] != fixture["function"]:
        raise ValueError("remaining core H1 address drift")
    for field in ("headerFacts", "windowFacts", "debugFacts", "runtimeQuestions"):
        if output[field] != fixture["expected"][field]:
            raise ValueError(f"remaining core {field} drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"]:
        raise ValueError("remaining core canonical hash drift")
    destination = output_path or repo_path("local/derived/remaining-core-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Inventory": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Files": output["summary"]["fileCount"],
        "WindowSlots": output["windowFacts"]["derived"]["windowSlotCount"],
        "DebugActions": output["debugFacts"]["debugBattleActionCount"],
        "RuntimeQuestions": len(output["runtimeQuestions"]),
        "Status": "PASS",
    }
