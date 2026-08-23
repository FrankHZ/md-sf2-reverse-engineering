"""Public-safe H2 reconstruction of the Map 3 castle continuation surface."""

from __future__ import annotations

import hashlib
import json
import re
import subprocess
from collections import deque
from heapq import heappop, heappush
from math import isqrt
from pathlib import Path
from typing import Any

from jsonschema import Draft7Validator

from sf2tool.h2.map_layouts import decode_map_blocks, decode_map_layout
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.rom import inspect_rom

ID = "sf2-map3-castle-battle-unlock-static-v1"
FIXTURE = repo_path("tests/fixtures/h2/map3-castle-battle-unlock-static-v1.json")
SCHEMA = repo_path("schemas/h2/map3-castle-battle-unlock-static-fixture.schema.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")
TOOLCHAIN = repo_path("manifests/toolchain.json")

_LISTING = Path("build/sf2build-h1.lst")
_H1_BINARY = Path("build/sf2build-h1.bin")
_SOURCE_SURFACE = (
    "sf2enums.asm",
    "sf2cutscenemacros.asm",
    "sf2const.asm",
    "sf2mapsetupmacros.asm",
    "code/common/tech/input.asm",
    "code/common/tech/interrupts/applyfadingeffectandz80busupdate.asm",
    "code/common/scripting/map/mapsetupsfunctions_1.asm",
    "code/common/scripting/map/mapscriptengine_2.asm",
    "code/common/scripting/entity/entityfunctions_1.asm",
    "code/common/scripting/entity/entityfunctions_2.asm",
    "code/common/scripting/entity/entityscriptengine_2.asm",
    "data/scripting/entity/eas_main.asm",
    "data/scripting/entity/eas_actions.asm",
    "data/maps/mapsetups.asm",
    "data/maps/entries/map03/mapsetups/pointertable.asm",
    "data/maps/entries/map19/mapsetups/pointertable.asm",
    "data/maps/entries/map20/mapsetups/pointertable.asm",
    "data/maps/entries/map21/mapsetups/pointertable.asm",
    "data/maps/entries/map03/mapsetups/s3_zoneevents.asm",
    "data/maps/entries/map19/mapsetups/s3_zoneevents.asm",
    "data/maps/entries/map19/mapsetups/s3_zoneevents_507.asm",
    "data/maps/entries/map20/mapsetups/s3_zoneevents.asm",
    "data/maps/entries/map20/mapsetups/s3_zoneevents_501.asm",
    "data/maps/entries/map21/mapsetups/s3_zoneevents.asm",
    "data/maps/entries/map03/mapsetups/scripts_1.asm",
    "data/maps/entries/map19/mapsetups/s6_initfunction.asm",
    "data/maps/entries/map19/mapsetups/s2_entityevents.asm",
    "data/maps/entries/map20/mapsetups/s6_initfunction.asm",
    "data/maps/entries/map21/mapsetups/s2_entityevents_506.asm",
    "data/maps/entries/map44/mapsetups/scripts.asm",
    "data/maps/entries/map19/6-warp-events.asm",
    "data/maps/entries/map20/6-warp-events.asm",
    "data/maps/entries/map21/6-warp-events.asm",
    "data/maps/entries/map03/4-step-events.asm",
    "data/maps/entries/map19/4-step-events.asm",
    "data/maps/entries/map20/4-step-events.asm",
    "data/maps/entries/map21/4-step-events.asm",
    "data/maps/entries/map03/2-areas.asm",
    "data/maps/entries/map19/2-areas.asm",
    "data/maps/entries/map20/2-areas.asm",
    "data/maps/entries/map21/2-areas.asm",
    "data/maps/entries/map03/mapsetups/s1_entities.asm",
    "data/maps/entries/map19/mapsetups/s1_entities.asm",
    "data/maps/entries/map20/mapsetups/s1_entities.asm",
    "data/maps/entries/map21/mapsetups/s1_entities.asm",
    "data/maps/entries/map03/0-blocks.bin",
    "data/maps/entries/map03/1-layout.bin",
    "data/maps/entries/map19/0-blocks.bin",
    "data/maps/entries/map19/1-layout.bin",
    "data/maps/entries/map20/0-blocks.bin",
    "data/maps/entries/map20/1-layout.bin",
    "data/maps/entries/map21/0-blocks.bin",
    "data/maps/entries/map21/1-layout.bin",
)

_FUNCTIONS = {
    "ApplyZ80BusUpdates": 0x008DE,
    "UpdatePlayerInputs": 0x0150E,
    "eas_ControlledCharacter": 0x44E3E,
    "esc01_waitUntilDestination": 0x04FD4,
    "esc02_controlCharacter": 0x04FF8,
    "RunMapSetupInitFunction": 0x474FC,
    "RunMapSetupZoneEvent": 0x4751A,
    "RunMapSetupEntityEvent": 0x4761A,
    "ExecuteMapScript": 0x4712C,
    "WaitForEvent": 0x2591C,
    "Map3_ZoneEvent1": 0x50DAC,
    "Map3_ZoneEvent4": 0x50DF8,
    "cs_51652": 0x51652,
    "ms_map19_InitFunction": 0x530EA,
    "cs_53104": 0x53104,
    "ms_map20_InitFunction": 0x53966,
    "cs_53996": 0x53996,
    "Map19_EntityEvent12": 0x52EF2,
    "cs_52F0C": 0x52F0C,
    "cs_52F40": 0x52F40,
    "Map21_EntityEvent0": 0x53EAE,
    "return_53EDC": 0x53EDC,
    "cs_53EF4": 0x53EF4,
}
_PRE_FLIGHT = {"loc_52E8": 0x52E8, "ms_map3_InitFunction": 0x51382}
_OWNER_ENTRY_SYMBOLS = {
    "map3Scripts": "cs_513D6",
    "map19Init": "ms_map19_InitFunction",
    "map20Init": "ms_map20_InitFunction",
    "map19EntityEvents": "ms_map19_EntityEvents",
    "map21Flag506EntityEvents": "ms_map21_flag506_EntityEvents",
}
_OWNER_ENTRY_ADDRESSES = {
    "map3Scripts": 0x513D6,
    "map19Init": 0x530EA,
    "map20Init": 0x53966,
    "map19EntityEvents": 0x52E02,
    "map21Flag506EntityEvents": 0x53EAA,
}
_INPUT_H1 = (
    ("ApplyZ80BusUpdates.call-UpdatePlayerInputs", 0x009F6, 4),
    ("eas_ControlledCharacter.ac_controlCharacter", 0x44E5C, 2),
    ("eas_ControlledCharacter.ac_waitDest", 0x44E5E, 2),
    ("esc02_controlCharacter.Bowie-current-input", 0x0500C, 2),
    ("esc02_controlCharacter.XDEST-commit", 0x0530C, 2),
    ("esc02_controlCharacter.YDEST-commit", 0x05310, 2),
    ("esc01_waitUntilDestination.equality", 0x04FF2, 2),
    ("esc01_waitUntilDestination.advance", 0x04FF4, 2),
)
_MAP19_TABLE = ("Map19EntityTable", 0x52BC2, 106)
_ROM_SHA256 = "9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9"
_UPSTREAM_COMMIT = "c834c652b6862bc5679fd7f69a38a7093206efc6"
_DIRECTIONS = {"Up": (0, -1), "Right": (1, 0), "Down": (0, 1), "Left": (-1, 0)}
_AREA = re.compile(
    r"mainLayerStart\s+(\d+),\s*(\d+)\s+mainLayerEnd\s+(\d+),\s*(\d+)",
    re.MULTILINE,
)
_ENTITY_ROW = re.compile(r"^\s*(msFixedEntity|msWalkingEntity)\s+(.+?)\s*$", re.MULTILINE)
_WARP = re.compile(
    r"mWarp\s+(\d+),\s*(\d+).*?warpMap\s+\S+.*?warpDest\s+(\d+),\s*(\d+).*?warpFacing\s+(\w+)",
    re.DOTALL,
)


def canonical_json_bytes(value: dict[str, Any]) -> bytes:
    """Emit the sole canonical UTF-8 representation for this public fixture."""
    return _canonical(value) + b"\n"


def _canonical(value: Any) -> bytes:
    return json.dumps(value, sort_keys=True, separators=(",", ":")).encode("utf-8")


def _disasm_root(upstream_path: Path) -> Path:
    root = upstream_path.resolve(strict=True)
    return root / "disasm" if (root / "disasm").is_dir() else root


def _without_comments(source: str) -> str:
    return "\n".join(line.split(";", maxsplit=1)[0].rstrip() for line in source.splitlines())


def _section(source: str, symbol: str) -> str:
    clean = _without_comments(source)
    match = re.search(rf"^{re.escape(symbol)}:\s*(?P<tail>.*)$", clean, re.MULTILINE)
    if match is None:
        raise ValueError(f"Map 3 castle static source label is missing: {symbol}")
    remaining = match.group("tail") + "\n" + clean[match.end() :]
    if symbol.startswith("cs_"):
        terminal = re.search(r"^\s*csc_end\s*$", remaining, re.MULTILINE)
        if terminal is None:
            raise ValueError(f"Map 3 castle static program terminator is missing: {symbol}")
        return remaining[: terminal.end()]
    # Assembly functions use local labels that share global-looking prefixes. The
    # explicit ordered guards below choose the bounded source use-site instead.
    return remaining


def _require_order(section: str, expected: tuple[str, ...], context: str) -> None:
    cursor = 0
    for value in expected:
        index = section.find(value, cursor)
        if index < 0:
            raise ValueError(f"Map 3 castle static {context} source-use drift: {value}")
        cursor = index + len(value)


def _listing_bytes(listing: str, address: int, width: int) -> bytes:
    cells: dict[int, int] = {}
    for line in listing.splitlines():
        match = re.match(r"^([0-9A-F]{8})\s+((?:[0-9A-F]{2,4}\s+)+)", line)
        if match is not None:
            start = int(match.group(1), 16)
            for offset, byte in enumerate(bytes.fromhex(re.sub(r"\s+", "", match.group(2)))):
                cells[start + offset] = byte
    if any(address + offset not in cells for offset in range(width)):
        raise ValueError(f"Map 3 castle static H1 span incomplete at {address:#x}")
    return bytes(cells[address + offset] for offset in range(width))


def _h1_projection(h1_binary: bytes, rom: bytes) -> list[dict[str, Any]]:
    spans = [
        *((name, address, 2) for name, address in _FUNCTIONS.items()),
        _MAP19_TABLE,
        *_INPUT_H1,
    ]
    projection: list[dict[str, Any]] = []
    for name, address, width in spans:
        h1 = h1_binary[address : address + width]
        if len(h1) != width:
            raise ValueError(f"Map 3 castle static H1 binary span is incomplete: {name}")
        if rom[address : address + width] != h1:
            raise ValueError(f"Map 3 castle static H1/ROM drift: {name}")
        projection.append(
            {
                "id": name,
                "address": address,
                "width": width,
                "sha256": hashlib.sha256(h1).hexdigest().upper(),
            }
        )
    if len(projection) != 32:
        raise ValueError("Map 3 castle static public H1 field denominator drift")
    return projection


def _read_source_surface(root: Path) -> tuple[dict[str, str], list[dict[str, str]]]:
    text: dict[str, str] = {}
    identities: list[dict[str, str]] = []
    for relative in _SOURCE_SURFACE:
        path = root / relative
        if not path.is_file():
            raise ValueError(f"Map 3 castle static source is missing: {relative}")
        data = path.read_bytes()
        identities.append({"path": relative, "sha256": hashlib.sha256(data).hexdigest().upper()})
        if path.suffix == ".asm":
            text[relative] = data.decode("utf-8")
    if len(identities) != 53:
        raise ValueError("Map 3 castle static source denominator drift")
    return text, identities


def _program_operations(text: dict[str, str]) -> dict[str, dict[str, Any]]:
    specs = (
        (
            "cs_51652",
            "data/maps/entries/map03/mapsetups/scripts_1.asm",
            (
                "textCursor 537",
                "entityActions 138",
                "moveRight 1",
                "endActions",
                "entityActionsWait 139",
                "moveLeft 1",
                "csc_end",
            ),
            {"callerSetFlags": [604], "role": "castle-gate"},
        ),
        (
            "cs_53104",
            "data/maps/entries/map19/mapsetups/s6_initfunction.asm",
            ("setPos 140,63,63,LEFT", "csc_end"),
            {"setFlags": [], "role": "map19-init"},
        ),
        (
            "cs_53996",
            "data/maps/entries/map20/mapsetups/s6_initfunction.asm",
            (
                "textCursor 2176",
                "setPos ALLY_BOWIE,23,39,DOWN",
                "entityActionsWait 131",
                "moveRight 1",
                "hide 130",
                "csc_end",
            ),
            {"callerSetFlags": [605], "role": "palace"},
        ),
        (
            "cs_52F0C",
            "data/maps/entries/map19/mapsetups/s2_entityevents.asm",
            (
                "textCursor 575",
                "yesNo",
                "jumpIfFlagSet 89,cs_52F40",
                "nextSingleText $0,140",
                "csc_end",
            ),
            {"setFlags": [], "role": "astral-prompt", "defaultBranch": "flag89-clear"},
        ),
        (
            "cs_52F40",
            "data/maps/entries/map19/mapsetups/s2_entityevents.asm",
            (
                "textCursor 578",
                "entityActionsWait 140",
                "moveUp 1",
                "moveLeft 8",
                "setPos 140,63,63,LEFT",
                "setF 608",
                "csc_end",
            ),
            {"setFlags": [608], "role": "astral-accept"},
        ),
        (
            "cs_53EF4",
            "data/maps/entries/map21/mapsetups/s2_entityevents_506.asm",
            (
                "entityActionsWait 128",
                "moveRight 1",
                "endActions",
                "setFacing 135,DOWN",
                "setStoryFlag 1",
                "csc_end",
            ),
            {
                "setStoryFlag": 1,
                "battleUnlockFlag": 401,
                "handlerSetFlag": 256,
                "role": "battle01-unlock",
            },
        ),
    )
    output: dict[str, dict[str, Any]] = {}
    for symbol, path, required, semantics in specs:
        section = _section(text[path], symbol)
        _require_order(section, required, symbol)
        output[symbol] = {
            "address": _FUNCTIONS[symbol],
            "controlEffectSha256": _program_control_effect_sha256(section),
            "operations": list(required),
            "semantics": semantics,
        }
    _map3_castle_gate_flag_owner(
        text["data/maps/entries/map03/mapsetups/s3_zoneevents.asm"],
        text["data/maps/entries/map03/mapsetups/scripts_1.asm"],
    )
    _require_order(
        _section(
            text["data/maps/entries/map19/mapsetups/s6_initfunction.asm"], "ms_map19_InitFunction"
        ),
        ("chkFlg  605", "script  cs_53104", "chkFlg  608", "script  cs_53104", "rts"),
        "Map19 init selector",
    )
    _require_order(
        _section(
            text["data/maps/entries/map20/mapsetups/s6_initfunction.asm"], "ms_map20_InitFunction"
        ),
        ("cmpi.l  #$22803780", "chkFlg  605", "script  cs_53996", "setFlg  605", "rts"),
        "Map20 init selector",
    )
    _require_order(
        _section(
            text["data/maps/entries/map19/mapsetups/s2_entityevents.asm"], "Map19_EntityEvent12"
        ),
        ("chkFlg  607", "script  cs_52F0C", "setFlg  607", "rts"),
        "Map19 entity selector",
    )
    _require_order(
        _section(
            text["data/maps/entries/map21/mapsetups/s2_entityevents_506.asm"], "Map21_EntityEvent0"
        ),
        ("chkFlg  608", "txt     579", "chkFlg  256", "script  cs_53EF4", "setFlg  256", "rts"),
        "Map21 entity selector",
    )
    return output


def _map3_castle_gate_flag_owner(zone_source: str, program_source: str) -> None:
    """Keep F604 with the Map 3 zone handler, not the called gate program."""
    _require_order(
        _section(zone_source, "Map3_ZoneEvent4"),
        ("chkFlg  604", "script  cs_51652", "setFlg  604", "rts"),
        "Map3 ZoneEvent4 F604 caller",
    )
    if re.search(r"\bsetF(?:lg)?\s+604\b", _without_comments(_section(program_source, "cs_51652"))):
        raise ValueError("Map 3 castle static cs_51652 must not own F604")


def _program_control_effect_sha256(section: str) -> str:
    """Hash ordered script/control tokens while excluding source comments and prose."""
    tokens: list[str] = []
    for raw_line in section.splitlines():
        line = raw_line.split(";", maxsplit=1)[0].strip()
        if not line:
            continue
        tokens.append(" ".join(line.split()))
    return hashlib.sha256(_canonical(tokens)).hexdigest().upper()


_ZONE_SPECS = (
    (
        3,
        "ms_map3",
        "ms_map3_ZoneEvents",
        0x50D4C,
        (609, 506, 543),
        "stateful",
        (
            ("specific", (2, 255), "Map3_ZoneEvent0", 0x50D74),
            ("specific", (27, 5), "Map3_ZoneEvent1", 0x50DAC),
            ("specific", (28, 5), "Map3_ZoneEvent1", 0x50DAC),
            ("specific", (29, 5), "Map3_ZoneEvent1", 0x50DAC),
            ("specific", (30, 5), "Map3_ZoneEvent4", 0x50DF8),
            ("specific", (31, 5), "Map3_ZoneEvent4", 0x50DF8),
            ("specific", (4, 4), "Map3_ZoneEvent6", 0x50E44),
            ("specific", (58, 13), "Map3_ZoneEvent7", 0x50E66),
            ("specific", (43, 10), "Map3_ZoneEvent8", 0x50ED2),
            ("default", (253, 0), "Map3_DefaultZoneEvent", 0x50EE8),
        ),
    ),
    (
        19,
        "ms_map19",
        "ms_map19_ZoneEvents",
        0x52D94,
        (501, 609, 506, 507, 543, 982),
        "direct-rts",
        (("default", (253, 0), "Map19_DefaultZoneEvent", 0x52DB4),),
    ),
    (
        20,
        "ms_map20",
        "ms_map20_ZoneEvents",
        0x53762,
        (501, 609, 506, 543),
        "direct-rts",
        (("default", (253, 0), "Map20_DefaultZoneEvent", 0x537AA),),
    ),
    (
        21,
        "ms_map21",
        "ms_map21_ZoneEvents",
        0x53E8C,
        (501, 609, 506, 543),
        "aliased-script",
        (("default", (253, 0), "Map21_DefaultZoneEvent", 0x545B6),),
    ),
)


def _zones(text: dict[str, str], h1_binary: bytes, rom: bytes) -> dict[str, Any]:
    rows: list[dict[str, Any]] = []
    for map_id, setup, symbol, address, flags, kind, expected in _ZONE_SPECS:
        source = text[f"data/maps/entries/map{map_id:02d}/mapsetups/s3_zoneevents.asm"]
        section = _section(source, symbol)
        parsed: list[tuple[str, str, str]] = []
        for line in _without_comments(section).splitlines():
            zone = re.search(
                r"msZoneEvent\s+([^,]+),\s*([^,]+),\s*([A-Za-z0-9_]+)-",
                line,
            )
            if zone is not None:
                parsed.append(zone.groups())
                continue
            default = re.search(r"msDefaultZoneEvent\s+([A-Za-z0-9_]+)-", line)
            if default is not None:
                parsed.append(("253", "0", default.group(1)))
        if len(parsed) != len(expected):
            raise ValueError(f"Map 3 castle static Map{map_id} zone row denominator drift")
        encoded = h1_binary[address : address + 4 * len(expected)]
        if len(encoded) != 4 * len(expected):
            raise ValueError(f"Map 3 castle static Map{map_id} zone H1 span drift")
        if rom[address : address + len(encoded)] != encoded:
            raise ValueError(f"Map 3 castle static Map{map_id} zone H1/ROM drift")
        actual = []
        for index, (expected_row, source_row) in enumerate(zip(expected, parsed, strict=True)):
            row_kind, point, target, target_address = expected_row
            x, y, source_target = source_row
            if (
                source_target != target
                or int(x.replace("$FD", "253"), 0) != point[0]
                or int(y, 0) != point[1]
            ):
                raise ValueError(f"Map 3 castle static Map{map_id} zone source row drift")
            actual.append(
                {
                    "kind": row_kind,
                    "point": list(point),
                    "recordAddress": address + 4 * index,
                    "target": target,
                    "targetAddress": target_address,
                }
            )
        rows.append(
            {
                "map": map_id,
                "setupPointer": setup,
                "zoneTable": symbol,
                "h1Address": address,
                "recordCount": len(actual),
                "encodedBytes": len(encoded),
                "encodedSha256": hashlib.sha256(encoded).hexdigest().upper(),
                "clearSelectorFlags": list(flags),
                "targetKind": kind,
                "records": actual,
            }
        )
    if (
        len(rows),
        sum(row["recordCount"] for row in rows),
        sum(row["encodedBytes"] for row in rows),
    ) != (4, 13, 52):
        raise ValueError("Map 3 castle static zone denominator drift")
    return {
        "denominator": {"tableCount": 4, "recordCount": 13, "encodedBytes": 52},
        "selectedTables": rows,
    }


def _areas(source: str, map_id: int) -> tuple[tuple[int, int, int, int], ...]:
    areas = tuple(
        tuple(int(value) for value in row) for row in _AREA.findall(_without_comments(source))
    )
    if not areas or any(x0 > x1 or y0 > y1 for x0, y0, x1, y1 in areas):
        raise ValueError(f"Map 3 castle static Map{map_id} area source drift")
    return areas


def _in_areas(point: tuple[int, int], areas: tuple[tuple[int, int, int, int], ...]) -> bool:
    x, y = point
    return any(x0 <= x <= x1 and y0 <= y <= y1 for x0, y0, x1, y1 in areas)


def _controller(text: dict[str, str]) -> dict[str, Any]:
    section = _section(
        text["code/common/scripting/entity/entityscriptengine_2.asm"], "esc02_controlCharacter"
    )
    _require_order(
        section,
        (
            "andi.w  #$C000,d1",
            "btst    #$F,d1",
            "addi.w  #-$7E,d0",
            "addi.w  #$7E,d0",
            "btst    #$E,d1",
            "addi.w  #$82,d0",
            "addi.w  #-$82,d0",
            "cmpi.w  #$C000,(a4,d2.w)",
            "bcs.w   loc_52E8",
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
    areas: tuple[tuple[int, int, int, int], ...],
    controller: dict[str, Any],
    addresses: dict[str, int],
    rom: bytes,
) -> dict[str, Any]:
    base = root / f"data/maps/entries/map{map_id:02d}"
    blocks, layout = (base / "0-blocks.bin").read_bytes(), (base / "1-layout.bin").read_bytes()
    for data, symbol in (
        (blocks, f"Map{map_id:02d}s0_Blocks"),
        (layout, f"Map{map_id:02d}s1_Layout"),
    ):
        address = addresses.get(symbol)
        if address is None or rom[address : address + len(data)] != data:
            raise ValueError(f"Map 3 castle static source/ROM private layout seam drift: {symbol}")
    block_words, _, _ = decode_map_blocks(blocks)
    layout_words, _, _, _ = decode_map_layout(layout, len(block_words) // 9)
    width = isqrt(len(layout_words))
    if width != 64 or width * width != len(layout_words):
        raise ValueError(f"Map 3 castle static Map{map_id} layout dimension drift")
    return {
        "map": map_id,
        "areas": areas,
        "layout": layout_words,
        "width": width,
        "blocksSha256": hashlib.sha256(blocks).hexdigest().upper(),
        "layoutSha256": hashlib.sha256(layout).hexdigest().upper(),
        "collisionSha256": hashlib.sha256(
            bytes(
                1 if word & controller["collisionMask"] == controller["collisionMask"] else 0
                for word in layout_words
            )
        )
        .hexdigest()
        .upper(),
    }


def _move(
    surface: dict[str, Any], point: tuple[int, int], input_name: str, controller: dict[str, Any]
) -> tuple[int, int] | None:
    dx, dy = _DIRECTIONS[input_name]
    x, y, width, layout = *point, surface["width"], surface["layout"]
    current = layout[y * width + x] & controller["collisionMask"]
    if input_name in {"Right", "Left"} and current in {
        controller["rightStairMask"],
        controller["leftStairMask"],
    }:
        offset_index = {
            (controller["rightStairMask"], "Right"): 0,
            (controller["rightStairMask"], "Left"): 1,
            (controller["leftStairMask"], "Right"): 2,
            (controller["leftStairMask"], "Left"): 3,
        }[(current, input_name)]
        offset = controller["stairWordDeltas"][offset_index]
        candidate = (
            x + dx,
            y + ((offset - 1) // width if input_name == "Right" else (offset + 1) // width),
        )
        if (
            0 <= candidate[0] < width
            and 0 <= candidate[1] < width
            and layout[candidate[1] * width + candidate[0]] & controller["collisionMask"] == current
        ):
            dy = candidate[1] - y
    candidate = (x + dx, y + dy)
    if not (0 <= candidate[0] < width and 0 <= candidate[1] < width) or not _in_areas(
        candidate, surface["areas"]
    ):
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
        for input_name in ("Up", "Right", "Down", "Left"):
            candidate = _move(surface, point, input_name, controller)
            if candidate is not None and candidate not in occupied and candidate not in previous:
                previous[candidate] = (point, input_name)
                queue.append(candidate)
    if end not in previous:
        raise ValueError(f"Map 3 castle static legal route is blocked: {start}->{end}")
    points, inputs = [list(end)], []
    cursor = end
    while previous[cursor] is not None:
        previous_point, input_name = previous[cursor]  # type: ignore[misc]
        points.append(list(previous_point))
        inputs.append(input_name)
        cursor = previous_point
    return list(reversed(points)), list(reversed(inputs))


def _navigation(
    identifier: str,
    surface: dict[str, Any],
    start: tuple[int, int],
    end: tuple[int, int],
    controller: dict[str, Any],
    occupied: frozenset[tuple[int, int]] = frozenset(),
    warp_predicate: dict[str, int] | None = None,
) -> dict[str, Any]:
    predicate_nodes: frozenset[tuple[int, int]] = frozenset()
    if warp_predicate is not None:
        if warp_predicate["fromMap"] != surface["map"] or warp_predicate["y"] != end[1]:
            raise ValueError(f"Map 3 castle static retained warp surface drift: {identifier}")

        def matches(point: tuple[int, int]) -> bool:
            return point[1] == warp_predicate["y"] and warp_predicate["x"] in {point[0], 255}

        if not matches(end):
            raise ValueError(f"Map 3 castle static retained warp terminal drift: {identifier}")
        predicate_nodes = frozenset(
            (x, warp_predicate["y"])
            for x in range(surface["width"])
            if (x, warp_predicate["y"]) != end
            and matches((x, warp_predicate["y"]))
            and _in_areas((x, warp_predicate["y"]), surface["areas"])
        )
    points, inputs = _shortest(surface, start, end, controller, occupied | predicate_nodes)
    if warp_predicate is not None:
        matched = [
            point
            for point in points[1:]
            if point[1] == warp_predicate["y"] and warp_predicate["x"] in {point[0], 255}
        ]
        if matched != [list(end)]:
            raise ValueError(
                "Map 3 castle static retained warp must occur only at the terminal: "
                f"{identifier}={matched}"
            )
    turns = [
        {"at": points[index], "input": input_name}
        for index, input_name in enumerate(inputs)
        if index == 0 or input_name != inputs[index - 1]
    ]
    return {
        "id": identifier,
        "kind": "navigation",
        "map": surface["map"],
        "from": list(start),
        "to": list(end),
        "points": points,
        "inputs": inputs,
        "turns": turns,
    }


def _entities(text: dict[str, str], map_id: int) -> list[dict[str, Any]]:
    source = _without_comments(text[f"data/maps/entries/map{map_id:02d}/mapsetups/s1_entities.asm"])
    symbol = f"ms_map{map_id:02d}_Entities"
    match = re.search(rf"^{symbol}:\s*$", source, re.MULTILINE)
    if match is None:
        raise ValueError(f"Map 3 castle static entity table missing: {symbol}")
    end = re.search(r"^\s*msEntitiesEnd\s*$", source[match.end() :], re.MULTILINE)
    if end is None:
        raise ValueError(f"Map 3 castle static entity terminator missing: {symbol}")
    rows = []
    for ordinal, (kind, operands) in enumerate(
        _ENTITY_ROW.findall(source[match.end() : match.end() + end.start()])
    ):
        values = [value.strip() for value in operands.split(",")]
        if len(values) != (7 if kind == "msWalkingEntity" else 5):
            raise ValueError(f"Map 3 castle static entity record width drift: Map{map_id}")
        row: dict[str, Any] = {
            "id": 128 + ordinal,
            "kind": "walking" if kind == "msWalkingEntity" else "fixed",
            "position": [int(values[0]), int(values[1])],
            "facing": values[2],
            "actscript": values[4],
        }
        if row["kind"] == "walking":
            if values[4] != values[0] or values[5] != values[1]:
                raise ValueError("Map 3 castle static walking origin drift")
            row.update(origin=[int(values[4]), int(values[5])], range=int(values[6]))
        rows.append(row)
    return rows


def _walk_cells(
    surface: dict[str, Any], record: dict[str, Any], controller: dict[str, Any]
) -> list[list[int]]:
    origin, radius = tuple(record["origin"]), record["range"]
    allowed = {
        (x, y)
        for x in range(origin[0] - radius, origin[0] + radius + 1)
        for y in range(origin[1] - radius, origin[1] + radius + 1)
        if _in_areas((x, y), surface["areas"])
    }
    reached, queue = {origin}, deque([origin])
    while queue:
        point = queue.popleft()
        for name in ("Up", "Right", "Down", "Left"):
            candidate = _move(surface, point, name, controller)
            if candidate is not None and candidate in allowed and candidate not in reached:
                reached.add(candidate)
                queue.append(candidate)
    return [list(point) for point in sorted(reached)]


def _occupancy(
    text: dict[str, str], surfaces: dict[int, dict[str, Any]], controller: dict[str, Any]
) -> dict[str, Any]:
    _require_order(
        _section(text["code/common/scripting/entity/entityfunctions_1.asm"], "DeclareNewEntity"),
        ("cmpi.b  #-1,d4", "bsr.w   SetWalkingActscript"),
        "walking sentinel",
    )
    _require_order(
        _section(text["code/common/scripting/entity/entityfunctions_2.asm"], "SetWalkingActscript"),
        (
            "lea     eas_Walking(pc), a0",
            "move.w  #50,d7",
            "move.w  d1,$22(a1)",
            "move.w  d2,$24(a1)",
            "move.w  d3,$26(a1)",
        ),
        "walking range",
    )
    _require_order(
        _section(text["data/scripting/entity/eas_main.asm"], "eas_Walking"),
        ("ac_randomWalk 16,16,12",),
        "walking action",
    )
    map19 = _entities(text, 19)
    if [row["id"] for row in map19] != list(range(128, 141)):
        raise ValueError("Map 3 castle static Map19 entity denominator drift")
    fixed = [
        {"id": row["id"], "point": row["position"]}
        for row in map19
        if row["kind"] == "fixed" and row["actscript"] == "eas_Init"
    ]
    expected_fixed = [
        {"id": 130, "point": [17, 25]},
        {"id": 131, "point": [14, 25]},
        {"id": 133, "point": [28, 17]},
        {"id": 134, "point": [30, 17]},
        {"id": 136, "point": [24, 16]},
        {"id": 138, "point": [19, 5]},
        {"id": 139, "point": [21, 4]},
        {"id": 140, "point": [16, 5]},
    ]
    if fixed != expected_fixed:
        raise ValueError("Map 3 castle static Map19 fixed occupancy drift")
    walking = [
        {
            "id": row["id"],
            "origin": row["origin"],
            "range": row["range"],
            "cells": _walk_cells(surfaces[19], row, controller),
        }
        for row in map19
        if row["kind"] == "walking"
    ]
    if [(row["id"], len(row["cells"])) for row in walking] != [
        (128, 24),
        (129, 24),
        (132, 8),
        (137, 7),
    ]:
        raise ValueError("Map 3 castle static Map19 walking domain drift")
    _require_order(
        _section(text["data/scripting/entity/eas_actions.asm"], "eas_LeftRightMoveLoop"),
        ("ac_branch", "byte_462AE", "ac_moveRel -1,0", "ac_moveRel 1,0"),
        "left-right loop",
    )
    if map19[7] != {
        "id": 135,
        "kind": "fixed",
        "position": [29, 8],
        "facing": "DOWN",
        "actscript": "eas_LeftRightMoveLoop",
    }:
        raise ValueError("Map 3 castle static left-right loop owner drift")
    _require_order(
        _section(text["data/maps/entries/map19/mapsetups/s6_initfunction.asm"], "cs_53104"),
        ("setPos 140,63,63,LEFT", "csc_end"),
        "Map19 visibility",
    )
    _require_order(
        _section(text["data/maps/entries/map19/mapsetups/s2_entityevents.asm"], "cs_52F40"),
        (
            "entityActionsWait 140",
            "moveUp 1",
            "moveLeft 8",
            "setPos 140,63,63,LEFT",
            "setF 608",
            "csc_end",
        ),
        "Astral visibility",
    )
    map20, map21 = _entities(text, 20), _entities(text, 21)
    if len(map20) != 8 or map21 != [
        {"id": 128, "kind": "fixed", "position": [5, 16], "facing": "DOWN", "actscript": "eas_Init"}
    ]:
        raise ValueError("Map 3 castle static Map20/21 entity denominator drift")
    program = _section(text["data/maps/entries/map20/mapsetups/s6_initfunction.asm"], "cs_53996")
    _require_order(
        program,
        ("entityActionsWait 131", "moveRight 1", "moveUp 1", "moveRight 1", "hide 130"),
        "Map20 post-program occupancy",
    )
    return {
        "map19": {
            "owner": {
                "source": "data/maps/entries/map19/mapsetups/s1_entities.asm",
                "h1Start": 0x52BC2,
                "recordCount": 13,
                "recordBytes": 8,
                "tableBytes": 106,
                "entity133Sha256": hashlib.sha256(bytes.fromhex("1C1103CF000460CE"))
                .hexdigest()
                .upper(),
            },
            "fixed": fixed,
            "walking": walking,
            "leftRightMoveLoop": [[28, 8], [29, 8], [30, 8]],
            "phaseVisibility": {
                "initialMap19Init": {"hidden": [140]},
                "palaceReturn": {"visible": [140]},
                "astralAcceptance": {"hidden": [140]},
            },
        },
        "map20": {
            "owner": {
                "source": "data/maps/entries/map20/mapsetups/s1_entities.asm",
                "recordCount": 8,
            },
            "postCs53996": {"hidden": [130], "entity131": [20, 39], "routeIntersection": "none"},
        },
        "map21": {
            "entity128": [5, 16],
            "interaction": {"before": [5, 16], "after": [6, 16], "player": [5, 15]},
        },
    }


def _warps(text: dict[str, str]) -> dict[str, list[dict[str, Any]]]:
    expected = {
        "map19Royal": (19, (23, 3, 23, 37, "DOWN")),
        "map19Tower": (19, (6, 2, 6, 37, "RIGHT")),
        "map20Royal": (20, (23, 37, 23, 3, "LEFT")),
        "map20Tower": (20, (3, 36, 3, 16, "RIGHT"), (6, 37, 6, 2, "LEFT")),
        "map21Tower": (21, (3, 16, 3, 36, "RIGHT")),
    }
    output: dict[str, list[dict[str, Any]]] = {}
    for name, value in expected.items():
        map_id, *rows = value
        found = {
            tuple([int(x), int(y), int(dx), int(dy), facing])
            for x, y, dx, dy, facing in _WARP.findall(
                _without_comments(text[f"data/maps/entries/map{map_id:02d}/6-warp-events.asm"])
            )
        }
        if not set(rows).issubset(found):
            raise ValueError(f"Map 3 castle static warp predicate drift: {name}")
        output[name] = [
            {"from": [x, y], "to": [dx, dy], "facing": facing} for x, y, dx, dy, facing in rows
        ]
    return output


def _map3_restored_guards(source: str) -> frozenset[tuple[int, int]]:
    """Derive the two Castle guard positions after the exact move-aside program."""
    clean = _without_comments(source)
    start = re.search(r"^cs_5149A:\s*", clean, re.MULTILINE)
    end = re.search(r"^cs_51652:\s*", clean, re.MULTILINE)
    if start is None or end is None or end.start() <= start.end():
        raise ValueError("Map 3 castle static guard setup/program boundary drift")
    starts = tuple(
        (int(entity), int(x), int(y))
        for entity, x, y in re.findall(
            r"^\s*setPos\s+(138|139),\s*(\d+),\s*(\d+),\s*(?:UP|DOWN|LEFT|RIGHT)\s*$",
            clean[start.end() : end.start()],
            re.MULTILINE,
        )
    )
    if starts != ((138, 27, 3), (139, 31, 3)):
        raise ValueError("Map 3 castle static guard source positions drift")
    program = _section(source, "cs_51652")
    actions = tuple(
        (entity, direction, int(count))
        for entity, direction, count in re.findall(
            r"^\s*entityActions(?:Wait)?\s+(138|139)\s*\n"
            r"\s*move(Up|Right|Down|Left)\s+(\d+)\s*\n\s*endActions\s*$",
            program,
            re.MULTILINE,
        )
    )
    if actions != (
        ("138", "Right", 1),
        ("139", "Left", 1),
        ("138", "Left", 1),
        ("139", "Right", 1),
    ):
        raise ValueError("Map 3 castle static guard move order drift")
    positions = {entity: (x, y) for entity, x, y in starts}
    for entity, direction, count in actions:
        dx, dy = _DIRECTIONS[direction]
        x, y = positions[int(entity)]
        positions[int(entity)] = (x + dx * count, y + dy * count)
    expected = {entity: (x, y) for entity, x, y in starts}
    if positions != expected:
        raise ValueError("Map 3 castle static guard restoration drift")
    return frozenset(positions.values())


def _retained_prefix() -> dict[str, Any]:
    paths = (
        (
            "r1",
            "tests/fixtures/h3/map3-admitted-start-v1.json",
            "sf2-map3-admitted-start-runtime-v1",
        ),
        (
            "r2",
            "tests/fixtures/h3/map3-battle01-natural-route-v1.json",
            "sf2-map3-battle01-natural-route-runtime-v1",
        ),
        (
            "r2a",
            "tests/fixtures/h3/map3-messenger-acceptance-v1.json",
            "sf2-map3-messenger-acceptance-runtime-v1",
        ),
    )
    records = []
    for key, path, fixture_id in paths:
        data = repo_path(path).read_bytes()
        if load_json(repo_path(path)).get("id") != fixture_id:
            raise ValueError(f"Map 3 castle static retained prefix identity drift: {key}")
        records.append(
            {
                "key": key,
                "fixtureId": fixture_id,
                "sha256": hashlib.sha256(data).hexdigest().upper(),
            }
        )
    return {"acceptedPrefixFixtures": records}


def _retained_r2_warps() -> list[dict[str, int]]:
    """Read the accepted R2 route predicates without importing a runtime rail."""
    fixture = load_json(repo_path("tests/fixtures/h3/map3-battle01-natural-route-v1.json"))
    if fixture.get("id") != "sf2-map3-battle01-natural-route-runtime-v1":
        raise ValueError("Map 3 castle static retained R2 fixture identity drift")
    warps = fixture.get("static", {}).get("route", {}).get("warps")
    if not isinstance(warps, list) or any(not isinstance(row, dict) for row in warps):
        raise ValueError("Map 3 castle static retained R2 warp table shape drift")
    return warps


def _retained_warp_predicate(
    warps: list[dict[str, int]],
    *,
    segment: str,
    source_map: int,
    source_point: tuple[int, int],
    destination_map: int,
    destination_point: tuple[int, int],
) -> dict[str, int]:
    matches = [
        row
        for row in warps
        if row["fromMap"] == source_map
        and row["y"] == source_point[1]
        and row["x"] in {source_point[0], 255}
    ]
    if len(matches) != 1:
        raise ValueError(
            f"Map 3 castle static retained warp predicate multiplicity drift: {segment}"
        )
    row = matches[0]
    if (
        row["toMap"] != destination_map
        or row["eventDestinationMap"] != destination_map
        or (row["destinationX"], row["destinationY"]) != destination_point
    ):
        raise ValueError(f"Map 3 castle static retained warp destination drift: {segment}")
    return row


def _retained_warp_joins(
    graph: dict[str, Any], warps: list[dict[str, int]]
) -> list[dict[str, Any]]:
    """Prove the five retained R2 predicates join only route-terminal edges."""
    joined: list[dict[str, Any]] = []
    for index, segment in enumerate(graph["segments"]):
        if segment["kind"] != "warp":
            continue
        if index == 0 or index + 1 >= len(graph["segments"]):
            raise ValueError("Map 3 castle static retained warp adjacency drift")
        navigation, following = graph["segments"][index - 1], graph["segments"][index + 1]
        source = segment["from"]
        destination = segment["to"]
        if (
            navigation["kind"] != "navigation"
            or navigation["map"] != source["map"]
            or navigation["to"] != source["point"]
            or (
                following["map"],
                following.get("from", following.get("entry")),
            )
            != (destination["map"], destination["point"])
        ):
            raise ValueError("Map 3 castle static retained warp route adjacency drift")
        joined.append(
            {
                "segment": segment["id"],
                "row": _retained_warp_predicate(
                    warps,
                    segment=segment["id"],
                    source_map=source["map"],
                    source_point=tuple(source["point"]),
                    destination_map=destination["map"],
                    destination_point=tuple(destination["point"]),
                ),
            }
        )
    if [row["segment"] for row in joined] != [
        "map3-to-map19-north-warp",
        "map19-to-map20-royal-warp",
        "map20-to-map19-royal-return",
        "map19-to-map20-west-tower-warp",
        "map20-to-map21-middle-tower-warp",
    ]:
        raise ValueError("Map 3 castle static retained warp join coverage drift")
    return joined


_ROUTE_SEGMENT_IDS = (
    "map3-school-door-to-castle-zone",
    "map3-castle-gate-zone",
    "map3-castle-zone-to-north-warp",
    "map3-to-map19-north-warp",
    "map19-entry-to-royal-warp",
    "map19-to-map20-royal-warp",
    "map20-palace-init-and-return",
    "map20-palace-return-to-royal-warp",
    "map20-to-map19-royal-return",
    "map19-royal-return-to-astral",
    "map19-astral-prompt-and-acceptance",
    "map19-astral-to-west-tower-warp",
    "map19-to-map20-west-tower-warp",
    "map20-west-tower-to-middle-warp",
    "map20-to-map21-middle-tower-warp",
    "map21-guard-unlock-return-and-terminal-wait",
)
_ROUTE_GRAPH_SHA256 = "D5AAD8F84D72F033012DD769C34FD6FCA8E8BC32EC81912712D9EFFDD5D3C5D4"


def _validate_route_graph(graph: dict[str, Any]) -> str:
    """Fail closed on the ordered public route corpus before fixture comparison."""
    segments = graph.get("segments")
    if not isinstance(segments, list) or (
        tuple(row.get("id") for row in segments) != _ROUTE_SEGMENT_IDS
        or sum(len(row.get("inputs", [])) for row in segments) != 110
    ):
        raise ValueError("Map 3 castle static legal route denominator/order drift")
    digest = hashlib.sha256(_canonical(graph)).hexdigest().upper()
    if digest != _ROUTE_GRAPH_SHA256:
        raise ValueError(f"Map 3 castle static legal route graph digest drift: {digest}")
    return digest


def _route_graph(
    text: dict[str, str],
    root: Path,
    addresses: dict[str, int],
    rom: bytes,
    warps: dict[str, list[dict[str, Any]]],
    occupancy: dict[str, Any],
    retained_r2_warps: list[dict[str, int]],
) -> dict[str, Any]:
    controller = _controller(text)
    surfaces = {
        map_id: _surface(
            root,
            map_id,
            _areas(text[f"data/maps/entries/map{map_id:02d}/2-areas.asm"], map_id),
            controller,
            addresses,
            rom,
        )
        for map_id in (3, 19, 20, 21)
    }
    step = _without_comments(text["data/maps/entries/map03/4-step-events.asm"])
    if not re.search(
        r"sbc\s+41,\s*13\s+sbcSource\s+62,\s*0\s+sbcSize\s+1,\s*1\s+sbcDest\s+41,\s*13", step
    ):
        raise ValueError("Map 3 castle static school-door mutation guard drift")
    destination, source = 13 * 64 + 41, 0 * 64 + 62
    if (
        surfaces[3]["layout"][destination] & 0xC000 != 0xC000
        or surfaces[3]["layout"][source] & 0xC000 == 0xC000
    ):
        raise ValueError("Map 3 castle static school-door collision polarity drift")
    surfaces[3]["layout"][destination] = surfaces[3]["layout"][source]
    fixed19 = {tuple(row["point"]) for row in occupancy["map19"]["fixed"]}
    walking19 = {tuple(point) for row in occupancy["map19"]["walking"] for point in row["cells"]}
    loop19 = {tuple(point) for point in occupancy["map19"]["leftRightMoveLoop"]}
    initial19 = (fixed19 | walking19 | loop19) - {(16, 5)}
    return19 = fixed19 | walking19 | loop19
    map20_entities = _entities(text, 20)
    map20_occupied = {
        tuple(row["position"]) for row in map20_entities if row["id"] not in {130, 131}
    } | {(20, 39)}
    s = []
    restored_guards = _map3_restored_guards(text["data/maps/entries/map03/mapsetups/scripts_1.asm"])

    def navigation_to_warp(
        identifier: str,
        surface: dict[str, Any],
        start: tuple[int, int],
        end: tuple[int, int],
        warp_id: str,
        destination_map: int,
        destination_point: tuple[int, int],
        *,
        occupied: frozenset[tuple[int, int]] = frozenset(),
    ) -> dict[str, Any]:
        return _navigation(
            identifier,
            surface,
            start,
            end,
            controller,
            occupied,
            _retained_warp_predicate(
                retained_r2_warps,
                segment=warp_id,
                source_map=surface["map"],
                source_point=end,
                destination_map=destination_map,
                destination_point=destination_point,
            ),
        )

    s.append(
        _navigation("map3-school-door-to-castle-zone", surfaces[3], (43, 10), (31, 5), controller)
    )
    s.append(
        {
            "id": "map3-castle-gate-zone",
            "kind": "zone-event",
            "map": 3,
            "point": [31, 5],
            "program": "cs_51652",
            "setFlag": 604,
        }
    )
    s.append(
        navigation_to_warp(
            "map3-castle-zone-to-north-warp",
            surfaces[3],
            (31, 5),
            (28, 1),
            "map3-to-map19-north-warp",
            19,
            (26, 30),
            occupied=restored_guards,
        )
    )
    s.append(
        {
            "id": "map3-to-map19-north-warp",
            "kind": "warp",
            "from": {"map": 3, "point": [28, 1], "facing": "UP"},
            "to": {"map": 19, "point": [26, 30], "facing": "UP"},
        }
    )
    s.append(
        navigation_to_warp(
            "map19-entry-to-royal-warp",
            surfaces[19],
            (26, 30),
            (23, 3),
            "map19-to-map20-royal-warp",
            20,
            (23, 37),
            occupied=frozenset(initial19),
        )
    )
    s.append(
        {
            "id": "map19-to-map20-royal-warp",
            "kind": "warp",
            "from": {"map": 19, "point": [23, 3], "facing": "DOWN"},
            "to": {"map": 20, "point": [23, 37], "facing": "DOWN"},
        }
    )
    s.append(
        {
            "id": "map20-palace-init-and-return",
            "kind": "init-program",
            "map": 20,
            "entry": [23, 37],
            "program": "cs_53996",
            "setFlag": 605,
            "sourcePostProgram": {"point": [23, 39], "facing": "DOWN"},
        }
    )
    s.append(
        navigation_to_warp(
            "map20-palace-return-to-royal-warp",
            surfaces[20],
            (23, 39),
            (23, 37),
            "map20-to-map19-royal-return",
            19,
            (23, 3),
            occupied=frozenset(map20_occupied),
        )
    )
    s.append(
        {
            "id": "map20-to-map19-royal-return",
            "kind": "warp",
            "from": {"map": 20, "point": [23, 37], "facing": "LEFT"},
            "to": {"map": 19, "point": [23, 3], "facing": "DOWN"},
        }
    )
    s.append(
        _navigation(
            "map19-royal-return-to-astral",
            surfaces[19],
            (23, 3),
            (16, 6),
            controller,
            frozenset(return19),
        )
    )
    s.append(
        {
            "id": "map19-astral-prompt-and-acceptance",
            "kind": "entity-interaction",
            "map": 19,
            "player": [16, 6],
            "facing": "UP",
            "entity": 140,
            "programs": ["cs_52F0C", "cs_52F40"],
            "setFlags": [608, 607],
        }
    )
    s.append(
        navigation_to_warp(
            "map19-astral-to-west-tower-warp",
            surfaces[19],
            (16, 6),
            (6, 2),
            "map19-to-map20-west-tower-warp",
            20,
            (6, 37),
            occupied=frozenset(initial19),
        )
    )
    s.append(
        {
            "id": "map19-to-map20-west-tower-warp",
            "kind": "warp",
            "from": {"map": 19, "point": [6, 2], "facing": "RIGHT"},
            "to": {"map": 20, "point": [6, 37], "facing": "RIGHT"},
        }
    )
    s.append(
        navigation_to_warp(
            "map20-west-tower-to-middle-warp",
            surfaces[20],
            (6, 37),
            (3, 36),
            "map20-to-map21-middle-tower-warp",
            21,
            (3, 16),
            occupied=frozenset(map20_occupied),
        )
    )
    s.append(
        {
            "id": "map20-to-map21-middle-tower-warp",
            "kind": "warp",
            "from": {"map": 20, "point": [3, 36], "facing": "RIGHT"},
            "to": {"map": 21, "point": [3, 16], "facing": "RIGHT"},
        }
    )
    terminal = _navigation(
        "map21-guard-unlock-return-and-terminal-wait", surfaces[21], (3, 16), (5, 15), controller
    )
    terminal.update(
        kind="entity-terminal",
        player=[5, 15],
        facing="DOWN",
        entity=128,
        program="cs_53EF4",
        setFlags=[401, 256],
        interactionPointIndex=2,
        interactionPoint=[4, 16],
        entityPointBefore=[5, 16],
        entityPointAfter=[6, 16],
        interactionFacing="RIGHT",
        terminal="unique-post-Map21_EntityEvent0-WaitForEvent",
    )
    s.append(terminal)
    graph = {
        "controller": {"layoutWidth": 64, **controller},
        "surfaces": [
            {
                key: surface[key]
                for key in ("map", "width", "blocksSha256", "layoutSha256", "collisionSha256")
            }
            for surface in surfaces.values()
        ],
        "schoolDoor": {
            "trigger": [41, 13],
            "source": [62, 0],
            "size": [1, 1],
            "destination": [41, 13],
        },
        "entityOccupancy": [
            {"map": 19, "id": 140, "position": [16, 5], "facing": "DOWN"},
            {"map": 20, "id": 130, "position": [19, 39], "facing": "LEFT"},
            {"map": 21, "id": 128, "position": [5, 16], "facing": "DOWN"},
        ],
        "segments": s,
    }
    _validate_route_graph(graph)
    return graph


def _zone_cells(surface: dict[str, Any]) -> frozenset[tuple[int, int]]:
    return frozenset(
        (x, y)
        for y in range(surface["width"])
        for x in range(surface["width"])
        if _in_areas((x, y), surface["areas"])
        and surface["layout"][y * surface["width"] + x] & 0x3C00 == 0x1400
    )


def _minimum_zone_metrics(
    surface: dict[str, Any],
    start: tuple[int, int],
    end: tuple[int, int],
    controller: dict[str, Any],
    zones: frozenset[tuple[int, int]],
    occupied: frozenset[tuple[int, int]],
    priority: tuple[str, str],
) -> tuple[int, int]:
    if priority not in {("edges", "zoneCount"), ("zoneCount", "edges")}:
        raise ValueError("Map 3 castle static zone topology priority drift")
    queue: list[tuple[tuple[int, int], tuple[int, int]]] = [((0, 0), start)]
    best = {start: (0, 0)}
    while queue:
        cost, point = heappop(queue)
        if cost != best[point]:
            continue
        if point == end:
            return cost if priority[0] == "edges" else (cost[1], cost[0])
        for input_name in ("Up", "Right", "Down", "Left"):
            candidate = _move(surface, point, input_name, controller)
            if candidate is None or candidate in occupied:
                continue
            next_cost = (
                (cost[0] + 1, cost[1] + int(candidate in zones))
                if priority[0] == "edges"
                else (cost[0] + int(candidate in zones), cost[1] + 1)
            )
            if candidate not in best or next_cost < best[candidate]:
                best[candidate] = next_cost
                heappush(queue, (next_cost, candidate))
    raise ValueError("Map 3 castle static zone topology path unexpectedly unreachable")


def _zone_topology(
    zones: dict[str, Any],
    graph: dict[str, Any],
    surfaces: dict[int, dict[str, Any]],
    controller: dict[str, Any],
    occupancy: dict[str, Any],
) -> dict[str, Any]:
    zone_sets = {map_id: _zone_cells(surface) for map_id, surface in surfaces.items()}
    if [len(zone_sets[map_id]) for map_id in (3, 19, 20, 21)] != [15, 7, 4, 0]:
        raise ValueError("Map 3 castle static layout zone-cell denominator drift")
    cells = [{"map": map_id, "cellCount": len(zone_sets[map_id])} for map_id in (3, 19, 20, 21)]
    scan_indexes = (0, 2, 4, 7, 9, 11, 13, 15)
    intersections = []
    for index in scan_indexes:
        segment = graph["segments"][index]
        hits = [
            {"pointIndex": point_index, "point": point}
            for point_index, point in enumerate(segment["points"][1:], start=1)
            if tuple(point) in zone_sets[segment["map"]]
        ]
        intersections.append(
            {
                "segment": segment["id"],
                "map": segment["map"],
                "edges": len(segment["inputs"]),
                "zoneCount": len(hits),
                "hits": hits,
            }
        )
    expected_intersections = (
        ("map3-school-door-to-castle-zone", 3, 31, [(31, [31, 5])]),
        ("map3-castle-zone-to-north-warp", 3, 7, []),
        ("map19-entry-to-royal-warp", 19, 38, [(18, [29, 15]), (24, [25, 13])]),
        ("map20-palace-return-to-royal-warp", 20, 2, []),
        ("map19-royal-return-to-astral", 19, 11, []),
        ("map19-astral-to-west-tower-warp", 19, 15, [(1, [16, 5])]),
        ("map20-west-tower-to-middle-warp", 20, 3, []),
        ("map21-guard-unlock-return-and-terminal-wait", 21, 3, []),
    )
    actual_intersections = tuple(
        (
            row["segment"],
            row["map"],
            row["edges"],
            [(hit["pointIndex"], hit["point"]) for hit in row["hits"]],
        )
        for row in intersections
    )
    if actual_intersections != expected_intersections:
        raise ValueError("Map 3 castle static exact zone route intersection drift")
    order = [
        {
            "target": "Map3_ZoneEvent4",
            "map": 3,
            "segment": "map3-school-door-to-castle-zone",
            "pointIndex": 31,
            "point": [31, 5],
            "lifecycle": "stateful",
        },
        {
            "target": "Map19_DefaultZoneEvent",
            "map": 19,
            "segment": "map19-entry-to-royal-warp",
            "pointIndex": 18,
            "point": [29, 15],
            "lifecycle": "direct-rts-pass-through",
        },
        {
            "target": "Map19_DefaultZoneEvent",
            "map": 19,
            "segment": "map19-entry-to-royal-warp",
            "pointIndex": 24,
            "point": [25, 13],
            "lifecycle": "direct-rts-pass-through",
        },
        {
            "target": "Map19_DefaultZoneEvent",
            "map": 19,
            "segment": "map19-astral-to-west-tower-warp",
            "pointIndex": 1,
            "point": [16, 5],
            "lifecycle": "direct-rts-pass-through",
        },
    ]
    table_by_map = {row["map"]: row for row in zones["selectedTables"]}
    for entry in order:
        records = table_by_map[entry["map"]]["records"]
        matching = [
            row
            for row in records
            if row["kind"] == "specific"
            and all(
                expected == actual or expected == 255
                for expected, actual in zip(row["point"], entry["point"], strict=True)
            )
        ]
        matching = matching[:1] or [row for row in records if row["kind"] == "default"]
        if len(matching) != 1 or matching[0]["target"] != entry["target"]:
            raise ValueError("Map 3 castle static zone admission target drift")
    fixed19 = frozenset(tuple(row["point"]) for row in occupancy["map19"]["fixed"])
    walking19 = frozenset(
        tuple(point) for row in occupancy["map19"]["walking"] for point in row["cells"]
    )
    loop19 = frozenset(tuple(point) for point in occupancy["map19"]["leftRightMoveLoop"])
    map19_initial = (fixed19 | walking19 | loop19) - {(16, 5)}
    topology = []
    for segment_index, expected_edges, expected_zones in ((4, 38, 2), (11, 15, 1)):
        segment = graph["segments"][segment_index]
        for priority in (("edges", "zoneCount"), ("zoneCount", "edges")):
            edges, zone_count = _minimum_zone_metrics(
                surfaces[19],
                tuple(segment["from"]),
                tuple(segment["to"]),
                controller,
                zone_sets[19],
                map19_initial,
                priority,
            )
            if (edges, zone_count) != (expected_edges, expected_zones):
                raise ValueError("Map 3 castle static zone topology minimum drift")
            topology.append(
                {
                    "segment": segment["id"],
                    "priority": list(priority),
                    "edges": edges,
                    "zoneCount": zone_count,
                }
            )
    try:
        _shortest(
            surfaces[19],
            tuple(graph["segments"][11]["from"]),
            tuple(graph["segments"][11]["to"]),
            controller,
            map19_initial | zone_sets[19],
        )
    except ValueError:
        pass
    else:
        raise ValueError("Map 3 castle static all-zone block unexpectedly reaches tower")
    return {
        "layoutZoneCellCounts": cells,
        "routeIntersections": intersections,
        "topology": topology,
        "zoneAdmissionOrder": order,
    }


def _structural_schema() -> dict[str, Any]:
    schema = load_json(SCHEMA)
    fixture = schema.get("$defs", {}).get("fixture")
    if not isinstance(fixture, dict):
        raise ValueError("Map 3 castle static fixture schema definition is missing")
    return {"$schema": schema["$schema"], "$ref": "#/$defs/fixture", "$defs": schema["$defs"]}


def _validate_structural_output(value: dict[str, Any]) -> None:
    errors = sorted(
        Draft7Validator(_structural_schema()).iter_errors(value),
        key=lambda error: list(error.absolute_path),
    )
    if errors:
        location = ".".join(str(part) for part in errors[0].absolute_path) or "<root>"
        raise ValueError(
            "Map 3 castle static structural schema validation failed at "
            f"{location}: {errors[0].message}"
        )


def build_map3_castle_battle_unlock_static(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    """Build the deterministic public H2 contract without a runtime observation."""
    if inspect_rom(rom_path.resolve(strict=True))["sha256"] != _ROM_SHA256:
        raise ValueError("Map 3 castle static canonical ROM SHA-256 drift")
    upstream = upstream_path.resolve(strict=True)
    revision = subprocess.run(
        ["git", "-C", str(upstream), "rev-parse", "HEAD"],
        check=True,
        capture_output=True,
        text=True,
    ).stdout.strip()
    if revision != _UPSTREAM_COMMIT:
        raise ValueError("Map 3 castle static upstream revision drift")
    root = _disasm_root(upstream)
    text, source_identities = _read_source_surface(root)
    listing = (upstream / _LISTING).read_text(encoding="utf-8")
    h1_binary = (upstream / _H1_BINARY).read_bytes()
    addresses = listing_symbol_addresses(listing)
    if {name: addresses.get(name) for name in _FUNCTIONS} != _FUNCTIONS or {
        name: addresses.get(name) for name in _PRE_FLIGHT
    } != _PRE_FLIGHT:
        raise ValueError("Map 3 castle static H1 named-function/preflight projection drift")
    owner_entry_addresses = {
        name: addresses.get(symbol) for name, symbol in _OWNER_ENTRY_SYMBOLS.items()
    }
    if owner_entry_addresses != _OWNER_ENTRY_ADDRESSES:
        raise ValueError("Map 3 castle static H1 owner-entry projection drift")
    rom = rom_path.resolve(strict=True).read_bytes()
    zones = _zones(text, h1_binary, rom)
    programs = _program_operations(text)
    controller = _controller(text)
    surfaces = {
        map_id: _surface(
            root,
            map_id,
            _areas(text[f"data/maps/entries/map{map_id:02d}/2-areas.asm"], map_id),
            controller,
            addresses,
            rom,
        )
        for map_id in (3, 19, 20, 21)
    }
    occupancy = _occupancy(text, surfaces, controller)
    warps = _warps(text)
    retained_r2_warps = _retained_r2_warps()
    graph = _route_graph(text, root, addresses, rom, warps, occupancy, retained_r2_warps)
    topology = _zone_topology(zones, graph, surfaces, controller, occupancy)
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
        "retainedPrefixGuards": _retained_prefix(),
        "sourceContext": {
            "sourceIdentities": source_identities,
            "functionAddresses": _FUNCTIONS,
            "preflightOnlyAddresses": _PRE_FLIGHT,
            "ownerEntryAddresses": owner_entry_addresses,
            "h1Projection": _h1_projection(h1_binary, rom),
        },
        "static": {
            "flags": {
                "f66": 66,
                "f600": 600,
                "f603": 603,
                "f604": 604,
                "f605": 605,
                "f607": 607,
                "f608": 608,
                "f401": 401,
                "f256": 256,
            },
            "programs": programs,
            "warps": warps,
            "retainedWarpJoins": _retained_warp_joins(graph, retained_r2_warps),
            "zones": {**zones, **topology},
            "occupancy": occupancy,
            "routeGraph": graph,
            "routeGraphSha256": hashlib.sha256(_canonical(graph)).hexdigest().upper(),
            "unknownBoundary": [
                "natural-execution-order",
                "caller-order",
                "runtime-endpoint",
                "final-WaitForEvent",
                "F401-F256-runtime-continuity",
                "RA-03-RA-04-runtime-continuity",
                "R2c-readiness",
            ],
        },
        "summary": {
            "sourceFiles": 53,
            "functions": 23,
            "h1Fields": 32,
            "programs": 6,
            "zoneTables": 4,
            "zoneRows": 13,
            "zoneEncodedBytes": 52,
            "routeSegments": 16,
            "logicalInputs": 110,
        },
    }
    _validate_structural_output(output)
    return output


def verify_map3_castle_battle_unlock_static(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    """Validate the checked-in public fixture against fresh H2 source/H1/ROM derivation."""
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner=str(FIXTURE))
    output = build_map3_castle_battle_unlock_static(rom_path, upstream_path)
    if output != fixture:
        raise ValueError("Map 3 castle static complete semantic fixture drift")
    return output
