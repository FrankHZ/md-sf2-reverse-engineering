from __future__ import annotations

import hashlib
import json
import re
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.source_text import read_upstream_text

ID = "sf2-map-content-static-v1"
ENTRIES_PATH = Path("data/maps/entries.asm")
ENTRY_ROOT = Path("data/maps/entries")
ENUMS_PATH = Path("sf2enums.asm")
MACROS_PATH = Path("sf2mapmacros.asm")
MAPLOAD_PATH = Path("code/common/maps/mapload.asm")
ANIMATIONS_PATH = Path("code/common/maps/animations.asm")
EXPLORATION_PATH = Path("code/gameflow/exploration/exploration.asm")
MANIFEST = repo_path("manifests/extractions/map-content-static.json")
SCHEMA = repo_path("schemas/map-content-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/map-content-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-map-content-static-fixture.schema.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

SECTION_LAYOUT = (
    ("tilesets", "00-tilesets.asm", "Map{map:02d}"),
    ("areas", "2-areas.asm", "Map{map:02d}s2_Areas"),
    ("flagEvents", "3-flag-events.asm", "Map{map:02d}s3_FlagEvents"),
    ("stepEvents", "4-step-events.asm", "Map{map:02d}s4_StepEvents"),
    ("roofEvents", "5-roof-events.asm", "Map{map:02d}s5_RoofEvents"),
    ("warpEvents", "6-warp-events.asm", "Map{map:02d}s6_WarpEvents"),
    ("chestItems", "7-chest-items.asm", "Map{map:02d}s7_ChestItems"),
    ("otherItems", "8-other-items.asm", "Map{map:02d}s8_OtherItems"),
    ("animations", "9-animations.asm", "Map{map:02d}s9_Animations"),
)

POINTER_LAYOUT = (
    ("blocks", 6),
    ("layout", 10),
    ("areas", 14),
    ("flagEvents", 18),
    ("stepEvents", 22),
    ("roofEvents", 26),
    ("warpEvents", 30),
    ("chestItems", 34),
    ("otherItems", 38),
    ("animations", 42),
)

BYTE_MACROS = {
    "mapPalette": 1,
    "mapTileset1": 1,
    "mapTileset2": 1,
    "mapTileset3": 1,
    "mapTileset4": 1,
    "mapTileset5": 1,
    "mainLayerAutoscroll": 2,
    "scndLayerAutoscroll": 2,
    "mainLayerType": 1,
    "areaDefaultMusic": 1,
    "fbcSource": 2,
    "fbcSize": 2,
    "fbcDest": 2,
    "sbc": 2,
    "sbcSource": 2,
    "sbcSize": 2,
    "sbcDest": 2,
    "slbc": 2,
    "slbcSource": 2,
    "slbcSize": 2,
    "slbcDest": 2,
    "mWarp": 2,
    "warpMap": 1,
    "warpDest": 2,
}

WORD_MACROS = {
    "mainLayerStart": 2,
    "mainLayerEnd": 2,
    "scndLayerFgndStart": 2,
    "scndLayerBgndStart": 2,
    "mainLayerParallax": 2,
    "scndLayerParallax": 2,
    "fbcFlag": 1,
    "mapAnimation": 2,
    "mapAnimEntry": 4,
}

RECORD_MACRO = {
    "areas": "mainLayerStart",
    "flagEvents": "fbcFlag",
    "stepEvents": "sbc",
    "roofEvents": "slbc",
    "warpEvents": "mWarp",
    "chestItems": "mapItem",
    "otherItems": "mapItem",
    "animations": "mapAnimEntry",
}


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _require_fragments(source: str, fragments: tuple[str, ...], owner: str) -> None:
    for fragment in fragments:
        if fragment not in source:
            raise ValueError(f"{owner} source-shape drift: missing {fragment!r}")


def _parse_equates(disasm: Path) -> dict[str, int]:
    source = read_upstream_text(disasm / ENUMS_PATH)
    definitions = re.findall(
        r"^([A-Z0-9_]+):\s+equ\s+(\$[0-9A-Fa-f]+|\d+)",
        source,
        re.MULTILINE,
    )
    return {
        name: int(value[1:], 16) if value.startswith("$") else int(value)
        for name, value in definitions
    }


def _value(expression: str, equates: dict[str, int]) -> int:
    expression = expression.strip()
    if expression in equates:
        return equates[expression]
    if expression.startswith("$"):
        return int(expression[1:], 16)
    return int(expression, 0)


def _arguments(text: str, count: int, equates: dict[str, int]) -> list[int]:
    parts = [part.strip() for part in text.split(",")]
    if len(parts) != count:
        raise ValueError(f"map content macro argument drift: expected {count}, got {text!r}")
    return [_value(part, equates) for part in parts]


def _encode_source(path: Path, kind: str, equates: dict[str, int]) -> tuple[bytes, int, bool]:
    encoded = bytearray()
    record_count = 0
    trailing_rts = False
    for raw_line in read_upstream_text(path).splitlines():
        line = raw_line.split(";", 1)[0].strip()
        if not line:
            continue
        if ":" in line:
            line = line.split(":", 1)[1].strip()
            if not line:
                continue
        macro, _, argument_text = line.partition(" ")
        argument_text = argument_text.strip()
        if macro in BYTE_MACROS:
            values = _arguments(argument_text, BYTE_MACROS[macro], equates)
            encoded.extend(value & 0xFF for value in values)
        elif macro in WORD_MACROS:
            values = _arguments(argument_text, WORD_MACROS[macro], equates)
            for value in values:
                encoded.extend((value & 0xFFFF).to_bytes(2, "big"))
        elif macro == "warpNoScroll":
            if argument_text:
                raise ValueError(f"warpNoScroll argument drift: {path}")
            encoded.append(0)
        elif macro == "warpScroll":
            encoded.append((0x10 + _value(argument_text, equates)) & 0xFF)
        elif macro == "warpFacing":
            encoded.extend((_value(argument_text, equates) & 0xFF, 0))
        elif macro == "mapItem":
            parts = [part.strip() for part in argument_text.split(",")]
            if len(parts) != 4:
                raise ValueError(f"mapItem argument drift: {path}")
            encoded.extend(_value(part, equates) & 0xFF for part in parts[:3])
            encoded.append(equates[f"ITEM_{parts[3]}"] & 0xFF)
        elif macro == "endWord":
            if argument_text:
                raise ValueError(f"endWord argument drift: {path}")
            encoded.extend(b"\xFF\xFF")
        elif macro == "rts":
            if argument_text:
                raise ValueError(f"rts argument drift: {path}")
            encoded.extend(b"\x4E\x75")
            trailing_rts = True
        else:
            raise ValueError(f"unsupported map content statement {line!r}: {path}")
        if RECORD_MACRO.get(kind) == macro:
            record_count += 1
    if kind == "tilesets":
        record_count = 6
    return bytes(encoded), record_count, trailing_rts


def _entry_pointer_expressions(source: str, map_index: int) -> list[str]:
    pattern = (
        rf"^Map{map_index:02d}:.*?\n"
        rf"(?P<body>(?:\s+dc\.l\s+[^\n]+\n){{10}})"
    )
    match = re.search(pattern, source, re.MULTILINE)
    if not match:
        raise ValueError(f"map {map_index:02d} entry pointer shape drift")
    return [
        line.split(";", 1)[0].split("dc.l", 1)[1].strip()
        for line in match.group("body").splitlines()
    ]


def _source_facts(disasm: Path) -> dict[str, Any]:
    enums = read_upstream_text(disasm / ENUMS_PATH)
    macros = read_upstream_text(disasm / MACROS_PATH)
    mapload = read_upstream_text(disasm / MAPLOAD_PATH)
    animations = read_upstream_text(disasm / ANIMATIONS_PATH)
    exploration = read_upstream_text(disasm / EXPLORATION_PATH)
    layout_offset_references = sum(
        read_upstream_text(path).count("MAPDATA_OFFSET_LAYOUT")
        for path in disasm.rglob("*.asm")
    )
    if layout_offset_references != 1:
        raise ValueError("MAPDATA_OFFSET_LAYOUT unexpectedly gained a code consumer")
    for name, offset in (
        ("TILESETS", 0),
        ("BLOCKS", 6),
        ("AREAS", 14),
        ("EVENT_FLAG", 18),
        ("EVENT_STEP", 22),
        ("EVENT_ROOF", 26),
        ("EVENT_WARP", 30),
        ("ITEM_CHEST", 34),
        ("ITEM_OTHER", 38),
        ("ANIMATIONS", 42),
    ):
        _require_fragments(enums, (f"MAPDATA_OFFSET_{name}: equ {offset}",), "map offsets")
    _require_fragments(enums, ("MAPDATA_OFFSET_LAYOUT: equ 8",), "map offsets")
    _require_fragments(
        macros,
        (
            "endWord: macro",
            "dc.w $FFFF",
            "mapItem: macro",
            "defineShorthand.b ITEM_,\\4",
            "mapAnimEntry: macro",
        ),
        "map content macros",
    )
    _require_fragments(
        mapload,
        (
            "LoadMapBlocksAndLayout:",
            "movea.l (a5)+,a0",
            "bsr.w   LoadMapBlocks",
            "bsr.w   LoadMapLayoutData",
            "movea.l 4(a5),a0",
            "bsr.w   CopyMapBlocks",
            "lea     MAPDATA_OFFSET_AREAS(a5),a5",
            "lea     $16(a4),a4",
            "move.l  $18(a5),((TILE_ANIMATION_DATA_ADDRESS-$1000000)).w",
        ),
        "map loader",
    )
    _require_fragments(
        exploration,
        tuple(
            f"MAPDATA_OFFSET_{name}"
            for name in (
                "EVENT_STEP",
                "EVENT_ROOF",
                "ITEM_CHEST",
                "ITEM_OTHER",
                "EVENT_WARP",
            )
        )
        + (
            "addq.l  #8,a2",
            "addq.l  #8,a0",
            "addq.l  #4,a2",
            "addq.l  #MAPDATA_EVENT_WARP_ENTRY_SIZE,a2",
        ),
        "exploration map consumers",
    )
    _require_fragments(
        animations,
        (
            "VInt_UpdateMapAnimations:",
            "MAPDATA_OFFSET_ANIMATIONS",
            "move.w  (a0)+,d0",
            "move.w  (a0)+,d2",
            "move.w  (a0)+,((TILE_ANIMATION_COUNTER-$1000000)).w",
        ),
        "map animations",
    )
    return {
        "entryBytes": 46,
        "tilesetBytes": 6,
        "pointerLayout": [{"name": name, "offset": offset} for name, offset in POINTER_LAYOUT],
        "recordBytes": {
            "area": 30,
            "flagEvent": 8,
            "stepEvent": 8,
            "roofEvent": 8,
            "warpEvent": 8,
            "item": 4,
            "animationHeader": 4,
            "animationEntry": 8,
            "terminator": 2,
        },
        "flagEventsAppliedDuringLayoutLoad": True,
        "areaSelectionSkipsRemaining22Bytes": True,
        "animationRunsFromVInt": True,
        "declaredLayoutOffset": 8,
        "actualLayoutPointerOffset": 10,
        "declaredLayoutOffsetReferenceCount": layout_offset_references - 1,
    }


def build_map_content_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"map content H1 listing is missing: {listing_path}")
    addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))
    rom = rom_path.read_bytes()
    rom_hash = hashlib.sha256(rom).hexdigest().upper()
    if rom_hash != load_json(ROM_MANIFEST)["hashes"]["sha256"]:
        raise ValueError("map content input ROM identity drift")
    equates = _parse_equates(disasm)
    entries_source = read_upstream_text(disasm / ENTRIES_PATH)

    pointer_table = b"".join(addresses[f"Map{index:02d}"].to_bytes(4, "big") for index in range(79))
    pointer_table_address = addresses["pt_MapData"]
    if rom[pointer_table_address : pointer_table_address + len(pointer_table)] != pointer_table:
        raise ValueError("pt_MapData source/ROM parity drift")

    map_entries = []
    for map_index in range(79):
        symbol = f"Map{map_index:02d}"
        tileset_path = disasm / ENTRY_ROOT / f"map{map_index:02d}" / "00-tilesets.asm"
        tileset_bytes, _, _ = _encode_source(tileset_path, "tilesets", equates)
        expressions = _entry_pointer_expressions(entries_source, map_index)
        pointer_values = [
            0xFFFFFFFF if expression == "$FFFFFFFF" else addresses[expression]
            for expression in expressions
        ]
        expected = tileset_bytes + b"".join(value.to_bytes(4, "big") for value in pointer_values)
        address = addresses[symbol]
        if len(expected) != 46 or rom[address : address + len(expected)] != expected:
            raise ValueError(f"map entry source/ROM parity drift: {symbol}")
        map_entries.append(
            {
                "map": map_index,
                "symbol": symbol,
                "address": address,
                "tilesets": list(tileset_bytes),
                "pointers": {
                    name: {
                        "symbol": None if expression == "$FFFFFFFF" else expression,
                        "address": value,
                    }
                    for (name, _), expression, value in zip(
                        POINTER_LAYOUT, expressions, pointer_values, strict=True
                    )
                },
            }
        )

    source_sections = []
    record_counts: dict[str, int] = {}
    for map_index in range(79):
        directory = disasm / ENTRY_ROOT / f"map{map_index:02d}"
        for kind, filename, symbol_template in SECTION_LAYOUT:
            path = directory / filename
            if not path.is_file():
                continue
            symbol = symbol_template.format(map=map_index)
            encoded, record_count, trailing_rts = _encode_source(path, kind, equates)
            address = addresses[symbol]
            if rom[address : address + len(encoded)] != encoded:
                raise ValueError(f"map content source/ROM parity drift: {symbol}")
            record_counts[kind] = record_counts.get(kind, 0) + record_count
            source_sections.append(
                {
                    "map": map_index,
                    "kind": kind,
                    "path": path.relative_to(disasm).as_posix(),
                    "symbol": symbol,
                    "address": address,
                    "byteCount": len(encoded),
                    "recordCount": record_count,
                    "trailingRts": trailing_rts,
                    "romParity": True,
                }
            )

    binary_payloads = []
    for map_index in range(79):
        directory = disasm / ENTRY_ROOT / f"map{map_index:02d}"
        for kind, filename, symbol in (
            ("blocks", "0-blocks.bin", f"Map{map_index:02d}s0_Blocks"),
            ("layout", "1-layout.bin", f"Map{map_index:02d}s1_Layout"),
        ):
            path = directory / filename
            if not path.is_file():
                continue
            payload = path.read_bytes()
            address = addresses[symbol]
            if rom[address : address + len(payload)] != payload:
                raise ValueError(f"map binary payload ROM parity drift: {symbol}")
            binary_payloads.append(
                {
                    "map": map_index,
                    "kind": kind,
                    "path": path.relative_to(disasm).as_posix(),
                    "symbol": symbol,
                    "address": address,
                    "byteCount": len(payload),
                    "sha256": hashlib.sha256(payload).hexdigest().upper(),
                    "romParity": True,
                }
            )

    source_facts = _source_facts(disasm)
    source_byte_count = sum(section["byteCount"] for section in source_sections)
    binary_byte_count = sum(payload["byteCount"] for payload in binary_payloads)
    section_counts = {
        kind: sum(section["kind"] == kind for section in source_sections)
        for kind, _, _ in SECTION_LAYOUT
    }
    record_counts["animationTables"] = section_counts["animations"]
    summary = {
        "mapCount": len(map_entries),
        "mapEntryRomParityCount": len(map_entries),
        "nullPointerCount": sum(
            pointer["address"] == 0xFFFFFFFF
            for entry in map_entries
            for pointer in entry["pointers"].values()
        ),
        "mapEntryByteCount": len(map_entries) * source_facts["entryBytes"],
        "pointerTableByteCount": len(pointer_table),
        "sourceSectionCount": len(source_sections),
        "sourceSectionByteCount": source_byte_count,
        "sourceSectionRomParityCount": sum(section["romParity"] for section in source_sections),
        "binaryPayloadCount": len(binary_payloads),
        "binaryPayloadByteCount": binary_byte_count,
        "binaryPayloadRomParityCount": sum(payload["romParity"] for payload in binary_payloads),
        "blockPayloadCount": sum(payload["kind"] == "blocks" for payload in binary_payloads),
        "blockPayloadByteCount": sum(
            payload["byteCount"] for payload in binary_payloads if payload["kind"] == "blocks"
        ),
        "blockPayloadMinBytes": min(
            payload["byteCount"] for payload in binary_payloads if payload["kind"] == "blocks"
        ),
        "blockPayloadMaxBytes": max(
            payload["byteCount"] for payload in binary_payloads if payload["kind"] == "blocks"
        ),
        "layoutPayloadCount": sum(payload["kind"] == "layout" for payload in binary_payloads),
        "layoutPayloadByteCount": sum(
            payload["byteCount"] for payload in binary_payloads if payload["kind"] == "layout"
        ),
        "layoutPayloadMinBytes": min(
            payload["byteCount"] for payload in binary_payloads if payload["kind"] == "layout"
        ),
        "layoutPayloadMaxBytes": max(
            payload["byteCount"] for payload in binary_payloads if payload["kind"] == "layout"
        ),
        "trailingRtsSectionCount": sum(section["trailingRts"] for section in source_sections),
    }
    if summary["sourceSectionCount"] != 662 or summary["binaryPayloadCount"] != 154:
        raise ValueError("map content section cardinality drift")
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "romSha256": rom_hash,
        "sources": {
            "entries": ENTRIES_PATH.as_posix(),
            "macros": MACROS_PATH.as_posix(),
            "enums": ENUMS_PATH.as_posix(),
            "mapLoader": MAPLOAD_PATH.as_posix(),
            "animations": ANIMATIONS_PATH.as_posix(),
            "exploration": EXPLORATION_PATH.as_posix(),
        },
        "table": {"pt_MapData": pointer_table_address},
        "summary": summary,
        "sectionCounts": section_counts,
        "recordCounts": record_counts,
        "sourceFacts": source_facts,
        "mapEntries": map_entries,
        "sourceSections": source_sections,
        "binaryPayloads": binary_payloads,
        "runtimeQuestions": [
            "map-transition-event-precedence-and-state-persistence",
            "map-animation-vdp-frame-timing",
            "map-block-layout-rendered-parity",
        ],
    }


def verify_map_content_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_map_content_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="map content static contract")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != output["romSha256"]
    ):
        raise ValueError("map content fixture provenance drift")
    for field in (
        "table",
        "summary",
        "sectionCounts",
        "recordCounts",
        "sourceFacts",
        "runtimeQuestions",
    ):
        if output[field] != fixture[field]:
            raise ValueError(f"map content {field} drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"] or output["summary"] != manifest["summary"]:
        raise ValueError("map content canonical output drift")
    destination = output_path or repo_path("local/derived/map-content-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Maps": output["summary"]["mapCount"],
        "SourceSections": output["summary"]["sourceSectionCount"],
        "BinaryPayloads": output["summary"]["binaryPayloadCount"],
        "Status": "PASS",
    }
