from __future__ import annotations

import hashlib
import json
from collections import Counter
from collections.abc import Callable
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.h2.map_content import (
    _encode_source,
    _parse_equates,
    build_map_content_contract,
)
from sf2tool.h2.map_layouts import decode_map_blocks, decode_map_layout
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path

ID = "sf2-canonical-map-import-v1"
MANIFEST = repo_path("manifests/extractions/canonical-map-import.json")
SCHEMA = repo_path("schemas/canonical-map-import.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/canonical-map-import-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-canonical-map-import-fixture.schema.json")


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _u16(data: bytes, offset: int) -> int:
    return int.from_bytes(data[offset : offset + 2], "big")


def _point(data: bytes, offset: int) -> dict[str, int]:
    return {"x": data[offset], "y": data[offset + 1]}


def _word_point(data: bytes, offset: int) -> dict[str, int]:
    return {"x": _u16(data, offset), "y": _u16(data, offset + 2)}


def _decode_areas(data: bytes, count: int) -> list[dict[str, Any]]:
    records = []
    for index in range(count):
        offset = index * 30
        records.append(
            {
                "mainLayerStart": _word_point(data, offset),
                "mainLayerEnd": _word_point(data, offset + 4),
                "secondLayerForegroundStart": _word_point(data, offset + 8),
                "secondLayerBackgroundStart": _word_point(data, offset + 12),
                "mainLayerParallax": _word_point(data, offset + 16),
                "secondLayerParallax": _word_point(data, offset + 20),
                "mainLayerAutoscroll": _point(data, offset + 24),
                "secondLayerAutoscroll": _point(data, offset + 26),
                "mainLayerType": data[offset + 28],
                "defaultMusic": data[offset + 29],
            }
        )
    return records


def _decode_flag_events(data: bytes, count: int) -> list[dict[str, Any]]:
    return [
        {
            "flag": _u16(data, offset),
            "source": _point(data, offset + 2),
            "size": {"width": data[offset + 4], "height": data[offset + 5]},
            "destination": _point(data, offset + 6),
        }
        for offset in range(0, count * 8, 8)
    ]


def _decode_copy_events(data: bytes, count: int) -> list[dict[str, Any]]:
    return [
        {
            "trigger": _point(data, offset),
            "source": _point(data, offset + 2),
            "size": {"width": data[offset + 4], "height": data[offset + 5]},
            "destination": _point(data, offset + 6),
        }
        for offset in range(0, count * 8, 8)
    ]


def _decode_warps(data: bytes, count: int) -> list[dict[str, Any]]:
    records = []
    for offset in range(0, count * 8, 8):
        mode = data[offset + 2]
        records.append(
            {
                "trigger": _point(data, offset),
                "scrollMode": mode,
                "retainsCoordinates": bool(mode & 0x10),
                "scrollDirection": mode & 0x03 if mode & 0x10 else None,
                "targetMap": data[offset + 3],
                "destination": _point(data, offset + 4),
                "facing": data[offset + 6],
                "reserved": data[offset + 7],
            }
        )
    return records


def _decode_items(data: bytes, count: int) -> list[dict[str, int]]:
    return [
        {
            "x": data[offset],
            "y": data[offset + 1],
            "flag": data[offset + 2],
            "item": data[offset + 3],
        }
        for offset in range(0, count * 4, 4)
    ]


def _decode_animations(data: bytes, count: int) -> dict[str, Any]:
    return {
        "tileset": _u16(data, 0),
        "speed": _u16(data, 2),
        "entries": [
            {
                "replacementStartTile": _u16(data, offset),
                "tileCount": _u16(data, offset + 2),
                "targetStartTile": _u16(data, offset + 4),
                "counter": _u16(data, offset + 6),
            }
            for offset in range(4, 4 + count * 8, 8)
        ],
    }


DECODERS: dict[str, tuple[int, Callable[[bytes, int], Any]]] = {
    "areas": (30, _decode_areas),
    "flagEvents": (8, _decode_flag_events),
    "stepEvents": (8, _decode_copy_events),
    "roofEvents": (8, _decode_copy_events),
    "warpEvents": (8, _decode_warps),
    "chestItems": (4, _decode_items),
    "otherItems": (4, _decode_items),
}


def _decode_source_table(kind: str, data: bytes, count: int, trailing_rts: bool) -> Any:
    if kind == "animations":
        payload_size = 4 + count * 8
        decoder: Callable[[bytes, int], Any] = _decode_animations
    else:
        record_size, decoder = DECODERS[kind]
        payload_size = record_size * count
    expected_tail = b"\xff\xff" + (b"\x4e\x75" if trailing_rts else b"")
    if data[payload_size:] != expected_tail:
        raise ValueError(f"{kind} logical terminator boundary drift")
    return decoder(data[:payload_size], count)


def _content_resources(
    content: dict[str, Any], disasm: Path, equates: dict[str, int]
) -> dict[str, list[dict[str, Any]]]:
    resources: dict[str, list[dict[str, Any]]] = {
        "areaTables": [],
        "flagEventTables": [],
        "stepEventTables": [],
        "roofEventTables": [],
        "warpEventTables": [],
        "itemTables": [],
        "animationTables": [],
    }
    resource_key = {
        "areas": "areaTables",
        "flagEvents": "flagEventTables",
        "stepEvents": "stepEventTables",
        "roofEvents": "roofEventTables",
        "warpEvents": "warpEventTables",
        "chestItems": "itemTables",
        "otherItems": "itemTables",
        "animations": "animationTables",
    }
    seen: set[str] = set()
    for section in content["sourceSections"]:
        kind = section["kind"]
        if kind == "tilesets":
            continue
        symbol = section["symbol"]
        if symbol in seen:
            raise ValueError(f"duplicate canonical map resource: {symbol}")
        seen.add(symbol)
        encoded, count, trailing_rts = _encode_source(disasm / section["path"], kind, equates)
        if count != section["recordCount"] or trailing_rts != section["trailingRts"]:
            raise ValueError(f"map source metadata drift while importing: {symbol}")
        resources[resource_key[kind]].append(
            {
                "id": symbol,
                "address": section["address"],
                "sourceKind": kind,
                "records": _decode_source_table(kind, encoded, count, trailing_rts),
            }
        )
    return resources


def _layout_resources(
    content: dict[str, Any], disasm: Path
) -> tuple[list[dict[str, Any]], list[dict[str, Any]]]:
    payloads = {(row["map"], row["kind"]): row for row in content["binaryPayloads"]}
    blocksets = []
    layouts = []
    for map_index in sorted({row["map"] for row in content["binaryPayloads"]}):
        block_meta = payloads[(map_index, "blocks")]
        layout_meta = payloads[(map_index, "layout")]
        block_data = (disasm / block_meta["path"]).read_bytes()
        layout_data = (disasm / layout_meta["path"]).read_bytes()
        block_words, _, _ = decode_map_blocks(block_data)
        block_count = len(block_words) // 9
        layout_words, _, _, _ = decode_map_layout(layout_data, block_count)
        blocksets.append(
            {
                "id": block_meta["symbol"],
                "address": block_meta["address"],
                "blocks": [
                    block_words[index : index + 9] for index in range(0, len(block_words), 9)
                ],
            }
        )
        layouts.append(
            {
                "id": layout_meta["symbol"],
                "address": layout_meta["address"],
                "width": 64,
                "height": 64,
                "words": layout_words,
            }
        )
    return blocksets, layouts


def build_canonical_map_import(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    content = build_map_content_contract(rom_path, upstream_path)
    disasm, commit, toolchain = _resolve_upstream(upstream_path.resolve(strict=True))
    if commit != content["upstream"]["commit"]:
        raise ValueError("canonical map import upstream provenance drift")
    equates = _parse_equates(disasm)
    resources = _content_resources(content, disasm, equates)
    blocksets, layouts = _layout_resources(content, disasm)
    resources = {"blocksets": blocksets, "layouts": layouts, **resources}

    reference_names = {
        "blocks": "blockset",
        "layout": "layout",
        "areas": "areaTable",
        "flagEvents": "flagEventTable",
        "stepEvents": "stepEventTable",
        "roofEvents": "roofEventTable",
        "warpEvents": "warpEventTable",
        "chestItems": "chestItemTable",
        "otherItems": "otherItemTable",
        "animations": "animationTable",
    }
    maps = []
    for entry in content["mapEntries"]:
        tilesets = entry["tilesets"]
        maps.append(
            {
                "id": entry["map"],
                "sourceSymbol": entry["symbol"],
                "palette": tilesets[0],
                "tilesets": tilesets[1:],
                "references": {
                    reference_names[name]: pointer["symbol"]
                    for name, pointer in entry["pointers"].items()
                },
            }
        )

    reference_values = [value for row in maps for value in row["references"].values()]
    reference_counts = Counter(value for value in reference_values if value is not None)
    resource_counts = {name: len(rows) for name, rows in resources.items()}
    record_counts = {
        "areas": sum(len(row["records"]) for row in resources["areaTables"]),
        "flagEvents": sum(len(row["records"]) for row in resources["flagEventTables"]),
        "stepEvents": sum(len(row["records"]) for row in resources["stepEventTables"]),
        "roofEvents": sum(len(row["records"]) for row in resources["roofEventTables"]),
        "warpEvents": sum(len(row["records"]) for row in resources["warpEventTables"]),
        "items": sum(len(row["records"]) for row in resources["itemTables"]),
        "animationEntries": sum(
            len(row["records"]["entries"]) for row in resources["animationTables"]
        ),
    }
    summary = {
        "mapCount": len(maps),
        "resourceCount": sum(resource_counts.values()),
        "nonNullReferenceCount": sum(value is not None for value in reference_values),
        "absentReferenceCount": sum(value is None for value in reference_values),
        "sharedReferenceCount": sum(count - 1 for count in reference_counts.values() if count > 1),
        "decodedBlockCount": sum(len(row["blocks"]) for row in blocksets),
        "decodedBlockWordCount": sum(len(block) for row in blocksets for block in row["blocks"]),
        "decodedLayoutWordCount": sum(len(row["words"]) for row in layouts),
        "logicalRecordCount": sum(record_counts.values()),
    }
    expected_resource_counts = {
        "blocksets": 77,
        "layouts": 77,
        "areaTables": 79,
        "flagEventTables": 79,
        "stepEventTables": 79,
        "roofEventTables": 79,
        "warpEventTables": 79,
        "itemTables": 156,
        "animationTables": 32,
    }
    if resource_counts != expected_resource_counts:
        raise ValueError(f"canonical map resource cardinality drift: {resource_counts}")
    all_ids = {row["id"] for rows in resources.values() for row in rows}
    missing = sorted({value for value in reference_values if value is not None} - all_ids)
    if missing:
        raise ValueError(f"canonical map references missing resources: {missing[:5]}")
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "romSha256": content["romSha256"],
        "geometry": {
            "layoutWidth": 64,
            "layoutHeight": 64,
            "blockWidthTiles": 3,
            "blockHeightTiles": 3,
            "rawWordBits": 16,
            "layoutBlockIndexMask": 0x03FF,
            "layoutFlagsMask": 0xFC00,
        },
        "table": content["table"],
        "summary": summary,
        "resourceCounts": resource_counts,
        "recordCounts": record_counts,
        "referenceFacts": {
            "blockLayoutAliases": [
                {"map": 24, "ownerMap": 23},
                {"map": 46, "ownerMap": 7},
            ],
            "animationReferenceCount": 41,
            "animationResourceCount": 32,
            "nullAnimationReferenceCount": 38,
            "otherItemTableAliases": [
                {"map": 47, "resource": "Map47s7_ChestItems"},
                {"map": 58, "resource": "Map58s7_ChestItems"},
            ],
            "rawFlagsPreserved": True,
        },
        "maps": maps,
        "resources": resources,
        "runtimeQuestions": [
            "map-transition-event-precedence-and-state-persistence",
            "map-animation-vdp-frame-timing",
            "canonical-map-rendered-vdp-parity",
        ],
    }


def verify_canonical_map_import(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_canonical_map_import(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="canonical map import")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != output["romSha256"]
    ):
        raise ValueError("canonical map import provenance drift")
    for field in (
        "geometry",
        "table",
        "summary",
        "resourceCounts",
        "recordCounts",
        "referenceFacts",
        "runtimeQuestions",
    ):
        if fixture[field] != output[field]:
            raise ValueError(f"canonical map import {field} drift")
    canonical = _canonical_bytes(output)
    if canonical != _canonical_bytes(build_canonical_map_import(rom_path, upstream_path)):
        raise ValueError("canonical map import is not deterministic across repeated builds")
    digest = hashlib.sha256(canonical).hexdigest().upper()
    if digest != manifest["outputSha256"] or output["summary"] != manifest["summary"]:
        raise ValueError("canonical map import output drift")
    destination = output_path or repo_path("local/derived/canonical-map-import.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(canonical)
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Maps": output["summary"]["mapCount"],
        "Resources": output["summary"]["resourceCount"],
        "LogicalRecords": output["summary"]["logicalRecordCount"],
        "Status": "PASS",
    }
