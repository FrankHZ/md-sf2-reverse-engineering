from __future__ import annotations

import hashlib
import json
import re
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.compression import decode_basic_compressed
from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.source_text import read_upstream_text

ID = "sf2-map-sprite-decode-v1"
ENTRIES_PATH = Path("data/graphics/mapsprites/entries.asm")
CONSUMER_PATH = Path("code/common/scripting/entity/entityscriptengine_2.asm")
MANIFEST = repo_path("manifests/extractions/map-sprite-decode.json")
SCHEMA = repo_path("schemas/map-sprite-decode.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/map-sprite-decode-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-map-sprite-decode-fixture.schema.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

POINTER_COUNT = 720
DECODED_BYTES = 0x240
SENTINEL_SYMBOL = "Mapsprite237_0"
SENTINEL_BYTES = bytes.fromhex("FFFF")


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def build_map_sprite_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"map sprite H1 listing is missing: {listing_path}")
    addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))
    rom = rom_path.read_bytes()
    rom_hash = hashlib.sha256(rom).hexdigest().upper()
    if rom_hash != load_json(ROM_MANIFEST)["hashes"]["sha256"]:
        raise ValueError("map sprite input ROM identity drift")

    source = read_upstream_text(disasm / ENTRIES_PATH)
    definitions = re.findall(
        r'^\s*(Mapsprite\d{3}_[012]):\s*incbin\s+"([^"]+)"',
        source,
        re.MULTILINE | re.IGNORECASE,
    )
    if not definitions:
        raise ValueError("map sprite source has no payload definitions")
    definitions_start = source.find(f"{definitions[0][0]}:")
    references = re.findall(
        r"\bdc\.l\s+(Mapsprite\d{3}_[012])\b", source[:definitions_start]
    )
    definition_symbols = [symbol for symbol, _ in definitions]
    if (
        len(references) != POINTER_COUNT
        or len(definitions) != 670
        or set(references) != set(definition_symbols)
    ):
        raise ValueError(
            "map sprite source-shape drift: "
            f"{len(references)} pointers, {len(definitions)} payloads"
        )

    table_address = addresses["pt_Mapsprites"]
    pointer_bytes = b"".join(addresses[symbol].to_bytes(4, "big") for symbol in references)
    if rom[table_address : table_address + len(pointer_bytes)] != pointer_bytes:
        raise ValueError("map sprite pointer-table ROM parity drift")

    aliases = []
    for slot, symbol in enumerate(references):
        mapsprite = slot // 3
        direction = slot % 3
        expected_symbol = f"Mapsprite{mapsprite:03}_{direction}"
        if symbol != expected_symbol:
            aliases.append(
                {
                    "slot": slot,
                    "mapsprite": mapsprite,
                    "direction": direction,
                    "payloadSymbol": symbol,
                    "payloadOwnerMapsprite": int(symbol[9:12]),
                    "payloadOwnerDirection": int(symbol[-1]),
                }
            )
    if len(aliases) != 50:
        raise ValueError(f"map sprite alias-count drift: {len(aliases)}")

    payloads = []
    for symbol, relative_path in definitions:
        data = (disasm / relative_path).read_bytes()
        source_address = addresses[symbol]
        if rom[source_address : source_address + len(data)] != data:
            raise ValueError(f"map sprite payload ROM parity drift: {symbol}")
        mapsprite = int(symbol[9:12])
        direction = int(symbol[-1])
        if symbol == SENTINEL_SYMBOL:
            if data != SENTINEL_BYTES:
                raise ValueError("map sprite sentinel payload drift")
            payloads.append(
                {
                    "mapsprite": mapsprite,
                    "direction": direction,
                    "symbol": symbol,
                    "sourcePath": relative_path,
                    "sourceAddress": source_address,
                    "compressedBytes": len(data),
                    "sentinel": True,
                    "sourceSha256": hashlib.sha256(data).hexdigest().upper(),
                }
            )
            continue

        decoded = decode_basic_compressed(data, expected_output_bytes=DECODED_BYTES)
        if decoded.input_bytes_consumed != len(data):
            raise ValueError(f"map sprite trailing compressed bytes: {symbol}")
        payloads.append(
            {
                "mapsprite": mapsprite,
                "direction": direction,
                "symbol": symbol,
                "sourcePath": relative_path,
                "sourceAddress": source_address,
                "compressedBytes": len(data),
                "sentinel": False,
                "decodedBytes": len(decoded.output),
                "decodedSha256": hashlib.sha256(decoded.output).hexdigest().upper(),
                "commandWordCount": decoded.command_word_count,
                "literalWordCount": decoded.literal_word_count,
                "copyCommandCount": decoded.copy_command_count,
                "copiedWordCount": decoded.copied_word_count,
                "repeatLastWordCommandCount": decoded.repeat_last_word_command_count,
                "maximumCopyOffsetWords": decoded.maximum_copy_offset_words,
                "maximumCopyLengthWords": decoded.maximum_copy_length_words,
            }
        )

    consumer = read_upstream_text(disasm / CONSUMER_PATH)
    for fragment in (
        "ChangeEntityMapsprite:",
        "DmaEntityMapsprite:",
        "lea     (pt_Mapsprites).l,a0",
        "jsr     (LoadBasicCompressedData).w",
        "mulu.w  #$240,d0",
        "move.w  #$120,d0",
    ):
        if fragment not in consumer:
            raise ValueError(f"map sprite consumer source-shape drift: {fragment!r}")

    decoded_payloads = [row for row in payloads if not row["sentinel"]]
    reference_counts = Counter(references)
    sentinel_slots = [slot for slot, symbol in enumerate(references) if symbol == SENTINEL_SYMBOL]
    summary = {
        "pointerCount": len(references),
        "uniquePayloadCount": len(payloads),
        "decodedPayloadCount": len(decoded_payloads),
        "sentinelPayloadCount": 1,
        "aliasPointerCount": len(aliases),
        "sentinelPointerCount": reference_counts[SENTINEL_SYMBOL],
        "sourceByteCount": sum(row["compressedBytes"] for row in payloads),
        "compressedByteCount": sum(row["compressedBytes"] for row in decoded_payloads),
        "decodedByteCount": sum(row["decodedBytes"] for row in decoded_payloads),
        "decodedBytesPerPayload": DECODED_BYTES,
        "commandWordCount": sum(row["commandWordCount"] for row in decoded_payloads),
        "literalWordCount": sum(row["literalWordCount"] for row in decoded_payloads),
        "copyCommandCount": sum(row["copyCommandCount"] for row in decoded_payloads),
        "copiedWordCount": sum(row["copiedWordCount"] for row in decoded_payloads),
        "repeatLastWordCommandCount": sum(
            row["repeatLastWordCommandCount"] for row in decoded_payloads
        ),
        "maximumCopyOffsetWords": max(row["maximumCopyOffsetWords"] for row in decoded_payloads),
        "maximumCopyLengthWords": max(row["maximumCopyLengthWords"] for row in decoded_payloads),
        "pointerTableRomParityCount": len(references),
        "payloadRomParityCount": len(payloads),
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "romSha256": rom_hash,
        "function": {"loadBasicAddress": addresses["LoadBasicCompressedData"]},
        "table": {"mapspriteTableAddress": table_address},
        "summary": summary,
        "sentinel": {
            "symbol": SENTINEL_SYMBOL,
            "bytesHex": SENTINEL_BYTES.hex().upper(),
            "pointerSlots": sentinel_slots,
        },
        "aliases": aliases,
        "payloads": payloads,
        "runtimeQuestions": [
            "Are logical map-sprite IDs 237-239 statically unreachable from the regular Basic "
            "decoder, as required by their shared 0xFFFF sentinel payload?"
        ],
    }


def verify_map_sprite_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_map_sprite_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="map sprite decode contract")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != output["romSha256"]
    ):
        raise ValueError("map sprite provenance drift")
    for field in ("function", "table", "summary", "sentinel", "runtimeQuestions"):
        if fixture[field] != output[field]:
            raise ValueError(f"map sprite {field} drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"] or output["summary"] != manifest["summary"]:
        raise ValueError("map sprite canonical output drift")
    destination = output_path or repo_path("local/derived/map-sprite-decode.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Pointers": output["summary"]["pointerCount"],
        "DecodedPayloads": output["summary"]["decodedPayloadCount"],
        "DecodedBytes": output["summary"]["decodedByteCount"],
        "Status": "PASS",
    }
