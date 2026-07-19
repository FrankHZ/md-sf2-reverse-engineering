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

ID = "sf2-battle-background-decode-v1"
ENTRIES_PATH = Path("data/graphics/battles/backgrounds/entries.asm")
LOADER_PATH = Path("code/gameflow/battle/battlescenes/battlesceneengine_1.asm")
MANIFEST = repo_path("manifests/extractions/battle-background-decode.json")
SCHEMA = repo_path("schemas/battle-background-decode.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/battle-background-decode-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-battle-background-decode-fixture.schema.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

PALETTE_BYTES = 32
TILESET_BYTES = 6144


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _parse_entries(source: str) -> tuple[list[str], list[tuple[str, str]]]:
    definitions_start = source.find("Background00:")
    if definitions_start < 0:
        raise ValueError("battle background source has no payload definitions")
    references = re.findall(r"\bdc\.l\s+(Background\d{2})\b", source[:definitions_start])
    definitions = re.findall(
        r'^\s*(Background\d{2}):\s*incbin\s+"([^"]+)"',
        source[definitions_start:],
        re.MULTILINE | re.IGNORECASE,
    )
    if len(references) != 30 or len(definitions) != 27:
        raise ValueError(
            f"battle background source-shape drift: {len(references)} pointers, "
            f"{len(definitions)} payloads"
        )
    if len({symbol for symbol, _ in definitions}) != len(definitions):
        raise ValueError("battle background payload symbol is duplicated")
    return references, definitions


def build_battle_background_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"battle background H1 listing is missing: {listing_path}")
    addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))
    rom = rom_path.read_bytes()
    rom_hash = hashlib.sha256(rom).hexdigest().upper()
    if rom_hash != load_json(ROM_MANIFEST)["hashes"]["sha256"]:
        raise ValueError("battle background input ROM identity drift")

    source = read_upstream_text(disasm / ENTRIES_PATH)
    references, definitions = _parse_entries(source)
    definition_paths = dict(definitions)
    missing = sorted(set(references) - set(definition_paths))
    if missing:
        raise ValueError(f"battle background pointer targets are undefined: {missing}")

    table_address = addresses["pt_Backgrounds"]
    encoded_table = b"".join(addresses[symbol].to_bytes(4, "big") for symbol in references)
    if rom[table_address : table_address + len(encoded_table)] != encoded_table:
        raise ValueError("battle background pointer-table ROM parity drift")

    payloads: list[dict[str, Any]] = []
    for symbol, relative_path in definitions:
        path = disasm / Path(relative_path.replace("\\", "/"))
        data = path.read_bytes()
        source_address = addresses[symbol]
        if rom[source_address : source_address + len(data)] != data:
            raise ValueError(f"battle background payload ROM parity drift: {symbol}")
        if len(data) < 38:
            raise ValueError(f"battle background header is truncated: {symbol}")

        tileset1_offset = int.from_bytes(data[0:2], "big")
        tileset2_offset = 2 + int.from_bytes(data[2:4], "big")
        palette_offset = 4 + int.from_bytes(data[4:6], "big")
        palette_end = palette_offset + PALETTE_BYTES
        if not (
            tileset1_offset == 38
            and palette_offset == 6
            and palette_end == tileset1_offset
            and tileset1_offset < tileset2_offset < len(data)
        ):
            raise ValueError(
                f"battle background relative-offset/header drift: {symbol} "
                f"({tileset1_offset}, {tileset2_offset}, {palette_offset})"
            )
        first = decode_stack_compressed(
            data[tileset1_offset:tileset2_offset], expected_output_bytes=TILESET_BYTES
        )
        second = decode_stack_compressed(
            data[tileset2_offset:], expected_output_bytes=TILESET_BYTES
        )
        payloads.append(
            {
                "background": int(symbol[-2:]),
                "symbol": symbol,
                "sourcePath": relative_path.replace("\\", "/"),
                "sourceAddress": source_address,
                "payloadBytes": len(data),
                "paletteOffset": palette_offset,
                "paletteSha256": hashlib.sha256(
                    data[palette_offset:palette_end]
                ).hexdigest().upper(),
                "tileset1Offset": tileset1_offset,
                "tileset2Offset": tileset2_offset,
                "tilesets": [
                    {
                        "compressedBytes": tileset2_offset - tileset1_offset,
                        "inputBitsConsumed": first.input_bits_consumed,
                        "trailingBits": (tileset2_offset - tileset1_offset) * 8
                        - first.input_bits_consumed,
                        "decodedBytes": len(first.output),
                        "decodedSha256": hashlib.sha256(first.output).hexdigest().upper(),
                        "commandGroupCount": first.command_group_count,
                        "literalWordCount": first.literal_word_count,
                        "copyCommandCount": first.copy_command_count,
                        "copiedWordCount": first.copied_word_count,
                        "maximumCopyOffsetWords": first.maximum_copy_offset_words,
                        "maximumCopyLengthWords": first.maximum_copy_length_words,
                    },
                    {
                        "compressedBytes": len(data) - tileset2_offset,
                        "inputBitsConsumed": second.input_bits_consumed,
                        "trailingBits": (len(data) - tileset2_offset) * 8
                        - second.input_bits_consumed,
                        "decodedBytes": len(second.output),
                        "decodedSha256": hashlib.sha256(second.output).hexdigest().upper(),
                        "commandGroupCount": second.command_group_count,
                        "literalWordCount": second.literal_word_count,
                        "copyCommandCount": second.copy_command_count,
                        "copiedWordCount": second.copied_word_count,
                        "maximumCopyOffsetWords": second.maximum_copy_offset_words,
                        "maximumCopyLengthWords": second.maximum_copy_length_words,
                    },
                ],
            }
        )

    owner_by_symbol = {symbol: int(symbol[-2:]) for symbol, _ in definitions}
    pointer_owners = [owner_by_symbol[symbol] for symbol in references]
    aliases = [
        {"background": background, "payloadOwnerBackground": owner}
        for background, owner in enumerate(pointer_owners)
        if background != owner
    ]
    if aliases != [
        {"background": 21, "payloadOwnerBackground": 12},
        {"background": 22, "payloadOwnerBackground": 12},
        {"background": 29, "payloadOwnerBackground": 13},
    ]:
        raise ValueError(f"battle background alias drift: {aliases}")

    loader = read_upstream_text(disasm / LOADER_PATH)
    for fragment in (
        "LoadBattlesceneBackground:",
        "move.w  (a2)+,d0        ; tileset 1 offset",
        "lea     -2(a2,d0.w),a0",
        "lea     $1800(a1),a1",
        "move.w  (a2),d0",
        "lea     (a2,d0.w),a0",
        "addq.w  #2,a0",
        "clr.w   (a1)+",
        "moveq   #14,d0",
    ):
        if fragment not in loader:
            raise ValueError(f"battle background loader source-shape drift: {fragment!r}")

    streams = [stream for payload in payloads for stream in payload["tilesets"]]
    summary = {
        "backgroundPointerCount": len(references),
        "uniquePayloadCount": len(payloads),
        "aliasPointerCount": len(aliases),
        "payloadByteCount": sum(row["payloadBytes"] for row in payloads),
        "headerByteCount": len(payloads) * 38,
        "paletteByteCount": len(payloads) * PALETTE_BYTES,
        "compressedStreamCount": len(streams),
        "compressedByteCount": sum(row["compressedBytes"] for row in streams),
        "decodedByteCount": sum(row["decodedBytes"] for row in streams),
        "decodedBytesPerTileset": TILESET_BYTES,
        "commandGroupCount": sum(row["commandGroupCount"] for row in streams),
        "literalWordCount": sum(row["literalWordCount"] for row in streams),
        "copyCommandCount": sum(row["copyCommandCount"] for row in streams),
        "copiedWordCount": sum(row["copiedWordCount"] for row in streams),
        "maximumCopyOffsetWords": max(row["maximumCopyOffsetWords"] for row in streams),
        "maximumCopyLengthWords": max(row["maximumCopyLengthWords"] for row in streams),
        "minimumTrailingBits": min(row["trailingBits"] for row in streams),
        "maximumTrailingBits": max(row["trailingBits"] for row in streams),
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
            "loadBackgroundAddress": addresses["LoadBattlesceneBackground"],
        },
        "table": {"backgroundTableAddress": table_address},
        "summary": summary,
        "aliases": aliases,
        "payloads": payloads,
        "runtimeQuestions": [],
    }


def verify_battle_background_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_battle_background_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="battle background decode contract")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != output["romSha256"]
    ):
        raise ValueError("battle background provenance drift")
    for field in ("function", "table", "summary", "aliases", "runtimeQuestions"):
        if fixture[field] != output[field]:
            raise ValueError(f"battle background {field} drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"] or output["summary"] != manifest["summary"]:
        raise ValueError("battle background canonical output drift")
    destination = output_path or repo_path("local/derived/battle-background-decode.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "BackgroundPointers": output["summary"]["backgroundPointerCount"],
        "UniquePayloads": output["summary"]["uniquePayloadCount"],
        "DecodedBytes": output["summary"]["decodedByteCount"],
        "Status": "PASS",
    }
