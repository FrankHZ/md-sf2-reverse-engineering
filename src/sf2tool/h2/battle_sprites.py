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

ID = "sf2-battle-sprite-decode-v1"
LOADER_PATH = Path("code/gameflow/battle/battlescenes/battlesceneengine_1.asm")
MANIFEST = repo_path("manifests/extractions/battle-sprite-decode.json")
SCHEMA = repo_path("schemas/battle-sprite-decode.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/battle-sprite-decode-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-battle-sprite-decode-fixture.schema.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

GROUPS = (
    {
        "side": "ally",
        "folder": "allies",
        "prefix": "Ally",
        "pointerCount": 32,
        "decodedBytesPerFrame": 0x1200,
        "tableSymbol": "pt_AllyBattlesprites",
    },
    {
        "side": "enemy",
        "folder": "enemies",
        "prefix": "Enemy",
        "pointerCount": 54,
        "decodedBytesPerFrame": 0x1800,
        "tableSymbol": "pt_EnemyBattlesprites",
    },
)


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _parse_entries(
    source: str, *, prefix: str, pointer_count: int
) -> tuple[list[str], list[tuple[str, str]]]:
    pattern = rf"{prefix}Battlesprite\d{{2}}"
    definitions = re.findall(
        rf'^\s*({pattern}):\s*\n?\s*incbin\s+"([^"]+)"',
        source,
        re.MULTILINE | re.IGNORECASE,
    )
    if not definitions:
        raise ValueError(f"{prefix.lower()} battle sprite source has no payload definitions")
    definitions_start = source.find(f"{definitions[0][0]}:")
    references = re.findall(rf"\bdc\.l\s+({pattern})\b", source[:definitions_start])
    if len(references) != pointer_count or len(definitions) != pointer_count:
        raise ValueError(
            f"{prefix.lower()} battle sprite source-shape drift: "
            f"{len(references)} pointers, {len(definitions)} payloads"
        )
    definition_symbols = [symbol for symbol, _ in definitions]
    if references != definition_symbols:
        raise ValueError(f"{prefix.lower()} battle sprite pointer/definition order drift")
    return references, definitions


def _stream_record(data: bytes, *, expected_bytes: int) -> dict[str, Any]:
    decoded = decode_stack_compressed(data, expected_output_bytes=expected_bytes)
    return {
        "compressedBytes": len(data),
        "inputBitsConsumed": decoded.input_bits_consumed,
        "trailingBits": len(data) * 8 - decoded.input_bits_consumed,
        "decodedBytes": len(decoded.output),
        "decodedSha256": hashlib.sha256(decoded.output).hexdigest().upper(),
        "commandGroupCount": decoded.command_group_count,
        "literalWordCount": decoded.literal_word_count,
        "copyCommandCount": decoded.copy_command_count,
        "copiedWordCount": decoded.copied_word_count,
        "maximumCopyOffsetWords": decoded.maximum_copy_offset_words,
        "maximumCopyLengthWords": decoded.maximum_copy_length_words,
    }


def _summary(payloads: list[dict[str, Any]]) -> dict[str, Any]:
    frames = [frame for payload in payloads for frame in payload["frames"]]
    return {
        "pointerCount": len(payloads),
        "payloadCount": len(payloads),
        "payloadByteCount": sum(row["payloadBytes"] for row in payloads),
        "headerByteCount": sum(row["headerBytes"] for row in payloads),
        "paletteCount": sum(row["paletteCount"] for row in payloads),
        "paletteByteCount": sum(row["paletteBytes"] for row in payloads),
        "frameCount": len(frames),
        "compressedByteCount": sum(row["compressedBytes"] for row in frames),
        "decodedByteCount": sum(row["decodedBytes"] for row in frames),
        "minimumFramesPerPayload": min(row["frameCount"] for row in payloads),
        "maximumFramesPerPayload": max(row["frameCount"] for row in payloads),
        "minimumPalettesPerPayload": min(row["paletteCount"] for row in payloads),
        "maximumPalettesPerPayload": max(row["paletteCount"] for row in payloads),
        "minimumAnimationSpeed": min(row["animationSpeed"] for row in payloads),
        "maximumAnimationSpeed": max(row["animationSpeed"] for row in payloads),
        "minimumStatusOffsetX": min(row["statusOffsetX"] for row in payloads),
        "maximumStatusOffsetX": max(row["statusOffsetX"] for row in payloads),
        "minimumStatusOffsetY": min(row["statusOffsetY"] for row in payloads),
        "maximumStatusOffsetY": max(row["statusOffsetY"] for row in payloads),
        "commandGroupCount": sum(row["commandGroupCount"] for row in frames),
        "literalWordCount": sum(row["literalWordCount"] for row in frames),
        "copyCommandCount": sum(row["copyCommandCount"] for row in frames),
        "copiedWordCount": sum(row["copiedWordCount"] for row in frames),
        "maximumCopyOffsetWords": max(row["maximumCopyOffsetWords"] for row in frames),
        "maximumCopyLengthWords": max(row["maximumCopyLengthWords"] for row in frames),
        "minimumTrailingBits": min(row["trailingBits"] for row in frames),
        "maximumTrailingBits": max(row["trailingBits"] for row in frames),
        "pointerTableRomParityCount": len(payloads),
        "payloadRomParityCount": len(payloads),
    }


def build_battle_sprite_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"battle sprite H1 listing is missing: {listing_path}")
    addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))
    rom = rom_path.read_bytes()
    rom_hash = hashlib.sha256(rom).hexdigest().upper()
    if rom_hash != load_json(ROM_MANIFEST)["hashes"]["sha256"]:
        raise ValueError("battle sprite input ROM identity drift")

    payloads: list[dict[str, Any]] = []
    side_summaries: list[dict[str, Any]] = []
    table_addresses: dict[str, int] = {}
    for group in GROUPS:
        side = group["side"]
        entries_path = Path(
            f"data/graphics/battles/battlesprites/{group['folder']}/entries.asm"
        )
        references, definitions = _parse_entries(
            read_upstream_text(disasm / entries_path),
            prefix=str(group["prefix"]),
            pointer_count=int(group["pointerCount"]),
        )
        table_address = addresses[str(group["tableSymbol"])]
        table_addresses[f"{side}BattlespriteTableAddress"] = table_address
        encoded_table = b"".join(addresses[symbol].to_bytes(4, "big") for symbol in references)
        if rom[table_address : table_address + len(encoded_table)] != encoded_table:
            raise ValueError(f"{side} battle sprite pointer-table ROM parity drift")

        side_payloads: list[dict[str, Any]] = []
        for index, (symbol, relative_path) in enumerate(definitions):
            path = disasm / Path(relative_path.replace("\\", "/"))
            data = path.read_bytes()
            source_address = addresses[symbol]
            if rom[source_address : source_address + len(data)] != data:
                raise ValueError(f"battle sprite payload ROM parity drift: {symbol}")
            if len(data) < 12:
                raise ValueError(f"battle sprite payload is truncated: {symbol}")

            palette_offset = 4 + int.from_bytes(data[4:6], "big")
            if palette_offset < 10 or (palette_offset - 6) % 2:
                raise ValueError(f"battle sprite palette/header offset drift: {symbol}")
            frame_count = (palette_offset - 6) // 2
            frame_offsets: list[int] = []
            for frame_index in range(frame_count):
                offset_position = 6 + frame_index * 2
                frame_offsets.append(
                    offset_position
                    + int.from_bytes(data[offset_position : offset_position + 2], "big")
                )
            if frame_offsets != sorted(frame_offsets) or frame_offsets[-1] >= len(data):
                raise ValueError(f"battle sprite frame-offset order drift: {symbol}")
            palette_bytes = frame_offsets[0] - palette_offset
            if palette_bytes <= 0 or palette_bytes % 32:
                raise ValueError(f"battle sprite palette boundary drift: {symbol}")
            palette_count = palette_bytes // 32
            if not 1 <= palette_count <= 4:
                raise ValueError(f"battle sprite palette-count drift: {symbol}")

            frames = []
            for frame_index, start in enumerate(frame_offsets):
                end = frame_offsets[frame_index + 1] if frame_index + 1 < frame_count else len(data)
                frames.append(
                    {
                        "frame": frame_index,
                        "sourceOffset": start,
                        **_stream_record(
                            data[start:end],
                            expected_bytes=int(group["decodedBytesPerFrame"]),
                        ),
                    }
                )
            row = {
                "side": side,
                "index": index,
                "symbol": symbol,
                "sourcePath": relative_path.replace("\\", "/"),
                "sourceAddress": source_address,
                "payloadBytes": len(data),
                "animationSpeed": int.from_bytes(data[0:2], "big"),
                "statusOffsetX": data[2],
                "statusOffsetY": data[3],
                "headerBytes": palette_offset,
                "frameCount": frame_count,
                "paletteCount": palette_count,
                "paletteBytes": palette_bytes,
                "paletteSha256": hashlib.sha256(
                    data[palette_offset : frame_offsets[0]]
                ).hexdigest().upper(),
                "decodedBytesPerFrame": int(group["decodedBytesPerFrame"]),
                "frames": frames,
            }
            side_payloads.append(row)
            payloads.append(row)
        side_summaries.append({"side": side, **_summary(side_payloads)})

    loader = read_upstream_text(disasm / LOADER_PATH)
    for fragment in (
        "LoadEnemyBattlespritePropertiesAndPalette:",
        "LoadEnemyBattlespriteFrameToVram:",
        "move.w  #$C00,d0",
        "LoadAllyBattlespritePropertiesAndPalette:",
        "LoadAllyBattlespriteFrameToVram:",
        "move.w  #$900,d0",
        "jmp     (ApplyImmediateVramDmaOnCompressedTiles).w",
    ):
        if fragment not in loader:
            raise ValueError(f"battle sprite loader source-shape drift: {fragment!r}")

    frames = [frame for payload in payloads for frame in payload["frames"]]
    summary = {
        "pointerCount": len(payloads),
        "payloadCount": len(payloads),
        "payloadByteCount": sum(row["payloadBytes"] for row in payloads),
        "headerByteCount": sum(row["headerBytes"] for row in payloads),
        "paletteCount": sum(row["paletteCount"] for row in payloads),
        "paletteByteCount": sum(row["paletteBytes"] for row in payloads),
        "frameCount": len(frames),
        "compressedByteCount": sum(row["compressedBytes"] for row in frames),
        "decodedByteCount": sum(row["decodedBytes"] for row in frames),
        "commandGroupCount": sum(row["commandGroupCount"] for row in frames),
        "literalWordCount": sum(row["literalWordCount"] for row in frames),
        "copyCommandCount": sum(row["copyCommandCount"] for row in frames),
        "copiedWordCount": sum(row["copiedWordCount"] for row in frames),
        "maximumCopyOffsetWords": max(row["maximumCopyOffsetWords"] for row in frames),
        "maximumCopyLengthWords": max(row["maximumCopyLengthWords"] for row in frames),
        "minimumTrailingBits": min(row["trailingBits"] for row in frames),
        "maximumTrailingBits": max(row["trailingBits"] for row in frames),
        "pointerTableRomParityCount": len(payloads),
        "payloadRomParityCount": len(payloads),
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "romSha256": rom_hash,
        "function": {
            "loadStackAddress": addresses["LoadStackCompressedData"],
            "loadEnemyPropertiesAddress": addresses[
                "LoadEnemyBattlespritePropertiesAndPalette"
            ],
            "loadEnemyFrameAddress": addresses["LoadEnemyBattlespriteFrameToVram"],
            "loadAllyPropertiesAddress": addresses[
                "LoadAllyBattlespritePropertiesAndPalette"
            ],
            "loadAllyFrameAddress": addresses["LoadAllyBattlespriteFrameToVram"],
        },
        "table": table_addresses,
        "summary": summary,
        "sideSummaries": side_summaries,
        "payloads": payloads,
        "runtimeQuestions": [],
    }


def verify_battle_sprite_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_battle_sprite_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="battle sprite decode contract")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != output["romSha256"]
    ):
        raise ValueError("battle sprite provenance drift")
    for field in ("function", "table", "summary", "sideSummaries", "runtimeQuestions"):
        if fixture[field] != output[field]:
            raise ValueError(f"battle sprite {field} drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"] or output["summary"] != manifest["summary"]:
        raise ValueError("battle sprite canonical output drift")
    destination = output_path or repo_path("local/derived/battle-sprite-decode.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Payloads": output["summary"]["payloadCount"],
        "Frames": output["summary"]["frameCount"],
        "DecodedBytes": output["summary"]["decodedByteCount"],
        "Status": "PASS",
    }
