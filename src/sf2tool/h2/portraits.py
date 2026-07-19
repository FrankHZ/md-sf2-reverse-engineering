from __future__ import annotations

import hashlib
import json
import re
from pathlib import Path
from typing import Any

from sf2tool.compression import decode_stack_compressed
from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.source_text import read_upstream_text

ID = "sf2-portrait-graphics-decode-v1"
ENTRIES_PATH = Path("data/graphics/portraits/entries.asm")
LOADER_PATH = Path("code/common/menus/portraitfunctions.asm")
MANIFEST = repo_path("manifests/extractions/portrait-graphics-decode.json")
SCHEMA = repo_path("schemas/portrait-graphics-decode.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/portrait-graphics-decode-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-portrait-graphics-decode-fixture.schema.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

PALETTE_BYTES = 32
DECODED_BYTES = 2048


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _parse_entries(source: str) -> tuple[list[str], list[tuple[str, str]]]:
    definitions_start = source.find("Portrait00:")
    if definitions_start < 0:
        raise ValueError("portrait source has no payload definitions")
    references = re.findall(r"\bdc\.l\s+(Portrait\d{2})\b", source[:definitions_start])
    definitions = re.findall(
        r'^\s*(Portrait\d{2}):\s*incbin\s+"([^"]+)"',
        source[definitions_start:],
        re.MULTILINE | re.IGNORECASE,
    )
    if len(references) != 56 or len(definitions) != 52:
        raise ValueError(
            f"portrait source-shape drift: {len(references)} pointers, "
            f"{len(definitions)} payloads"
        )
    if len({symbol for symbol, _ in definitions}) != len(definitions):
        raise ValueError("portrait payload symbol is duplicated")
    return references, definitions


def _read_animation_entries(data: bytes, offset: int, kind: str) -> tuple[list[list[int]], int]:
    if offset + 2 > len(data):
        raise ValueError(f"portrait {kind} count is truncated")
    count = int.from_bytes(data[offset : offset + 2], "big")
    offset += 2
    end = offset + count * 4
    if end > len(data):
        raise ValueError(f"portrait {kind} entries are truncated")
    entries = [list(data[index : index + 4]) for index in range(offset, end, 4)]
    if any(coordinate > 7 for entry in entries for coordinate in entry):
        raise ValueError(f"portrait {kind} coordinate exceeds the 8x8 tile grid")
    return entries, end


def build_portrait_graphics_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"portrait H1 listing is missing: {listing_path}")
    addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))
    rom = rom_path.read_bytes()
    rom_hash = hashlib.sha256(rom).hexdigest().upper()
    if rom_hash != load_json(ROM_MANIFEST)["hashes"]["sha256"]:
        raise ValueError("portrait input ROM identity drift")

    source = read_upstream_text(disasm / ENTRIES_PATH)
    references, definitions = _parse_entries(source)
    definition_paths = dict(definitions)
    missing = sorted(set(references) - set(definition_paths))
    if missing:
        raise ValueError(f"portrait pointer targets are undefined: {missing}")

    table_address = addresses["pt_Portraits"]
    encoded_table = b"".join(addresses[symbol].to_bytes(4, "big") for symbol in references)
    if rom[table_address : table_address + len(encoded_table)] != encoded_table:
        raise ValueError("portrait pointer-table ROM parity drift")

    payloads: list[dict[str, Any]] = []
    for symbol, relative_path in definitions:
        path = disasm / Path(relative_path.replace("\\", "/"))
        data = path.read_bytes()
        source_address = addresses[symbol]
        if rom[source_address : source_address + len(data)] != data:
            raise ValueError(f"portrait payload ROM parity drift: {symbol}")

        eye_entries, offset = _read_animation_entries(data, 0, "eye")
        mouth_entries, offset = _read_animation_entries(data, offset, "mouth")
        header_end = offset + PALETTE_BYTES
        if header_end >= len(data):
            raise ValueError(f"portrait palette or compressed payload is truncated: {symbol}")
        decoded = decode_stack_compressed(
            data[header_end:], expected_output_bytes=DECODED_BYTES
        )
        payloads.append(
            {
                "portrait": int(symbol[-2:]),
                "symbol": symbol,
                "sourcePath": relative_path.replace("\\", "/"),
                "sourceAddress": source_address,
                "payloadBytes": len(data),
                "headerBytes": header_end,
                "eyeEntryCount": len(eye_entries),
                "mouthEntryCount": len(mouth_entries),
                "metadataSha256": hashlib.sha256(data[:offset]).hexdigest().upper(),
                "paletteSha256": hashlib.sha256(data[offset:header_end]).hexdigest().upper(),
                "compressedBytes": len(data) - header_end,
                "inputBitsConsumed": decoded.input_bits_consumed,
                "trailingBits": (len(data) - header_end) * 8 - decoded.input_bits_consumed,
                "decodedBytes": len(decoded.output),
                "decodedSha256": hashlib.sha256(decoded.output).hexdigest().upper(),
                "commandGroupCount": decoded.command_group_count,
                "literalWordCount": decoded.literal_word_count,
                "copyCommandCount": decoded.copy_command_count,
                "copiedWordCount": decoded.copied_word_count,
                "maximumCopyOffsetWords": decoded.maximum_copy_offset_words,
                "maximumCopyLengthWords": decoded.maximum_copy_length_words,
            }
        )

    owner_by_symbol = {symbol: int(symbol[-2:]) for symbol, _ in definitions}
    pointer_owners = [owner_by_symbol[symbol] for symbol in references]
    aliases = [
        {"portrait": portrait, "payloadOwnerPortrait": owner}
        for portrait, owner in enumerate(pointer_owners)
        if portrait != owner
    ]
    if aliases != [
        {"portrait": 35, "payloadOwnerPortrait": 33},
        {"portrait": 53, "payloadOwnerPortrait": 52},
        {"portrait": 54, "payloadOwnerPortrait": 52},
        {"portrait": 55, "payloadOwnerPortrait": 52},
    ]:
        raise ValueError(f"portrait alias drift: {aliases}")

    loader = read_upstream_text(disasm / LOADER_PATH)
    for fragment in (
        "LoadPortrait:",
        "move.w  (a0)+,d0",
        "move.l  (a0)+,(a1)+",
        "lea     (PALETTE_2_CURRENT).l,a1",
        "moveq   #7,d7",
        "jsr     (LoadStackCompressedData).w",
        "move.w  #$400,d0",
        "jsr     (ApplyVIntVramDma).w",
    ):
        if fragment not in loader:
            raise ValueError(f"portrait loader source-shape drift: {fragment!r}")

    summary = {
        "portraitPointerCount": len(references),
        "uniquePayloadCount": len(payloads),
        "aliasPointerCount": len(aliases),
        "payloadByteCount": sum(row["payloadBytes"] for row in payloads),
        "headerByteCount": sum(row["headerBytes"] for row in payloads),
        "paletteByteCount": len(payloads) * PALETTE_BYTES,
        "compressedByteCount": sum(row["compressedBytes"] for row in payloads),
        "decodedByteCount": sum(row["decodedBytes"] for row in payloads),
        "decodedBytesPerPortrait": DECODED_BYTES,
        "eyeEntryCount": sum(row["eyeEntryCount"] for row in payloads),
        "mouthEntryCount": sum(row["mouthEntryCount"] for row in payloads),
        "minimumHeaderBytes": min(row["headerBytes"] for row in payloads),
        "maximumHeaderBytes": max(row["headerBytes"] for row in payloads),
        "commandGroupCount": sum(row["commandGroupCount"] for row in payloads),
        "literalWordCount": sum(row["literalWordCount"] for row in payloads),
        "copyCommandCount": sum(row["copyCommandCount"] for row in payloads),
        "copiedWordCount": sum(row["copiedWordCount"] for row in payloads),
        "maximumCopyOffsetWords": max(row["maximumCopyOffsetWords"] for row in payloads),
        "maximumCopyLengthWords": max(row["maximumCopyLengthWords"] for row in payloads),
        "minimumTrailingBits": min(row["trailingBits"] for row in payloads),
        "maximumTrailingBits": max(row["trailingBits"] for row in payloads),
        "pointerTableRomParityCount": len(references),
        "payloadRomParityCount": len(payloads),
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "romSha256": rom_hash,
        "function": {
            "loadStackAddress": addresses["LoadStackCompressedData"],
            "loadPortraitAddress": addresses["LoadPortrait"],
        },
        "table": {"portraitTableAddress": table_address},
        "summary": summary,
        "aliases": aliases,
        "payloads": payloads,
        "runtimeQuestions": [],
    }


def verify_portrait_graphics_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_portrait_graphics_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="portrait graphics decode contract")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != output["romSha256"]
    ):
        raise ValueError("portrait graphics provenance drift")
    for field in ("function", "table", "summary", "aliases", "runtimeQuestions"):
        if fixture[field] != output[field]:
            raise ValueError(f"portrait graphics {field} drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"] or output["summary"] != manifest["summary"]:
        raise ValueError("portrait graphics canonical output drift")
    destination = output_path or repo_path("local/derived/portrait-graphics-decode.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "PortraitPointers": output["summary"]["portraitPointerCount"],
        "UniquePayloads": output["summary"]["uniquePayloadCount"],
        "DecodedBytes": output["summary"]["decodedByteCount"],
        "Status": "PASS",
    }
