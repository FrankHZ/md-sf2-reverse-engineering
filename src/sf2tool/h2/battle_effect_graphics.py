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

ID = "sf2-battle-effect-graphics-decode-v1"
MANIFEST = repo_path("manifests/extractions/battle-effect-graphics-decode.json")
SCHEMA = repo_path("schemas/battle-effect-graphics-decode.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/battle-effect-graphics-decode-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-battle-effect-graphics-decode-fixture.schema.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

SPELL_SOURCE = "data/graphics/battles/spells/entries.asm"
INVOCATION_SOURCE = "data/graphics/battles/spells/invocations/entries.asm"
STATUS_SOURCE = "data/graphics/battles/tech/statusanimation/entries.asm"
TRANSITION_SOURCE = "data/graphics/battles/tech/battlescenetransition/entries.asm"
INVOCATION_NAMES = ("Dao", "Atlas", "Neptun", "Apollo")


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _stack_row(data: bytes) -> tuple[dict[str, Any], bytes]:
    decoded = decode_stack_compressed(data)
    return (
        {
            "compressedBytes": len(data),
            "decodedBytes": len(decoded.output),
            "sourceSha256": hashlib.sha256(data).hexdigest().upper(),
            "decodedSha256": hashlib.sha256(decoded.output).hexdigest().upper(),
            "inputBitsConsumed": decoded.input_bits_consumed,
            "trailingBits": len(data) * 8 - decoded.input_bits_consumed,
            "commandGroupCount": decoded.command_group_count,
            "literalWordCount": decoded.literal_word_count,
            "copyCommandCount": decoded.copy_command_count,
            "copiedWordCount": decoded.copied_word_count,
            "maximumCopyOffsetWords": decoded.maximum_copy_offset_words,
            "maximumCopyLengthWords": decoded.maximum_copy_length_words,
        },
        decoded.output,
    )


def _assert_consumer_shape(disasm: Path) -> None:
    engine = read_upstream_text(
        disasm / "code/gameflow/battle/battlescenes/battlesceneengine_1.asm"
    )
    for fragment in (
        "movea.l (p_pt_InvocationSprites).l,a0",
        "move.w  #$900,d0",
        "movea.l (p_pt_SpellGraphics).l,a0",
        "LoadSpellTilesetForInvocation:",
        "jsr     (LoadStackCompressedData).w",
    ):
        if fragment not in engine:
            raise ValueError(f"battle-effect graphics consumer drift: missing {fragment!r}")
    initialize = read_upstream_text(
        disasm / "code/gameflow/battle/battlescenes/initializebattlescene.asm"
    )
    for fragment in ("movea.l (p_tiles_StatusAnimation).l,a0", "move.w  #$270,d0"):
        if fragment not in initialize:
            raise ValueError(f"status-animation consumer drift: missing {fragment!r}")
    switch = read_upstream_text(
        disasm / "code/gameflow/battle/battlescenes/battlesceneengine_0.asm"
    )
    if "movea.l (p_pt_tiles_BattlesceneTransition).l,a2" not in switch:
        raise ValueError("battle-scene transition consumer drift")


def _check_pointer(rom: bytes, addresses: dict[str, int], pointer: str, target: str) -> None:
    pointer_address = addresses[pointer]
    if rom[pointer_address : pointer_address + 4] != addresses[target].to_bytes(4, "big"):
        raise ValueError(f"battle-effect pointer ROM parity drift: {pointer}")


def _check_payload(
    rom: bytes, addresses: dict[str, int], symbol: str, data: bytes
) -> int:
    address = addresses[symbol]
    if rom[address : address + len(data)] != data:
        raise ValueError(f"battle-effect payload ROM parity drift: {symbol}")
    return address


def build_battle_effect_graphics_contract(
    rom_path: Path, upstream_path: Path
) -> dict[str, Any]:
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"battle-effect graphics H1 listing is missing: {listing_path}")
    addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))
    rom = rom_path.read_bytes()
    rom_hash = hashlib.sha256(rom).hexdigest().upper()
    if rom_hash != load_json(ROM_MANIFEST)["hashes"]["sha256"]:
        raise ValueError("battle-effect graphics input ROM identity drift")
    _assert_consumer_shape(disasm)

    table = {
        "pt_SpellGraphics": addresses["pt_SpellGraphics"],
        "pt_InvocationSprites": addresses["pt_InvocationSprites"],
        "tiles_StatusAnimation": addresses["tiles_StatusAnimation"],
        "pt_BattlesceneTransitionTiles": addresses["pt_BattlesceneTransitionTiles"],
    }
    for pointer, target in (
        ("p_pt_SpellGraphics", "pt_SpellGraphics"),
        ("p_pt_InvocationSprites", "pt_InvocationSprites"),
        ("p_tiles_StatusAnimation", "tiles_StatusAnimation"),
        ("p_pt_tiles_BattlesceneTransition", "pt_BattlesceneTransitionTiles"),
    ):
        _check_pointer(rom, addresses, pointer, target)
        table[pointer] = addresses[pointer]

    spell_source = read_upstream_text(disasm / SPELL_SOURCE)
    spell_rows = []
    spell_symbols = [f"SpellGraphics{i:02}" for i in range(23)]
    for index, symbol in enumerate(spell_symbols):
        path = f"data/graphics/battles/spells/spellgraphics{index:02}.bin"
        if f'{symbol}:incbin "{path}"' not in spell_source:
            raise ValueError(f"spell graphics definition drift: {symbol}")
        data = (disasm / path).read_bytes()
        source_address = _check_payload(rom, addresses, symbol, data)
        decoded_size = int.from_bytes(data[:2], "big")
        stats, decoded = _stack_row(data[8:])
        if len(decoded) != decoded_size:
            raise ValueError(f"spell graphics size header drift: {symbol}")
        spell_rows.append(
            {
                "index": index,
                "symbol": symbol,
                "sourcePath": path,
                "sourceAddress": source_address,
                "containerBytes": len(data),
                "paletteSha256": hashlib.sha256(data[2:8]).hexdigest().upper(),
                **stats,
            }
        )
    spell_table = b"".join(addresses[symbol].to_bytes(4, "big") for symbol in spell_symbols)
    spell_table_address = addresses["pt_SpellGraphics"]
    if rom[spell_table_address : spell_table_address + len(spell_table)] != spell_table:
        raise ValueError("spell graphics table ROM parity drift")

    invocation_source = read_upstream_text(disasm / INVOCATION_SOURCE)
    invocation_rows = []
    invocation_streams = []
    invocation_symbols = [f"InvocationSprite{name}" for name in INVOCATION_NAMES]
    for index, (name, symbol) in enumerate(zip(INVOCATION_NAMES, invocation_symbols, strict=True)):
        path = f"data/graphics/battles/spells/invocations/{name.casefold()}.bin"
        pattern = rf'{re.escape(symbol)}:\s*incbin\s+"{re.escape(path)}"'
        if not re.search(pattern, invocation_source):
            raise ValueError(f"invocation graphics definition drift: {symbol}")
        data = (disasm / path).read_bytes()
        source_address = _check_payload(rom, addresses, symbol, data)
        palette_start = 6 + int.from_bytes(data[6:8], "big")
        if palette_start < 8 or (palette_start - 8) % 4:
            raise ValueError(f"invocation frame-table boundary drift: {symbol}")
        frame_count = (palette_start - 8) // 4
        stream_starts = []
        for stream_index in range(frame_count * 2):
            word_address = 8 + stream_index * 2
            stream_starts.append(
                word_address + int.from_bytes(data[word_address : word_address + 2], "big")
            )
        if stream_starts != sorted(stream_starts) or stream_starts[0] != palette_start + 32:
            raise ValueError(f"invocation stream ordering drift: {symbol}")
        for stream_index, start in enumerate(stream_starts):
            end = (
                stream_starts[stream_index + 1]
                if stream_index + 1 < len(stream_starts)
                else len(data)
            )
            stats, decoded = _stack_row(data[start:end])
            if len(decoded) != 4096:
                raise ValueError(f"invocation output-size drift: {symbol} stream {stream_index}")
            invocation_streams.append(
                {
                    "containerIndex": index,
                    "containerSymbol": symbol,
                    "frameIndex": stream_index // 2,
                    "layerIndex": stream_index % 2,
                    "streamOffset": start,
                    "transferBytes": 4608,
                    "transferTailBytes": 512,
                    **stats,
                }
            )
        invocation_rows.append(
            {
                "index": index,
                "symbol": symbol,
                "sourcePath": path,
                "sourceAddress": source_address,
                "containerBytes": len(data),
                "frameCount": frame_count,
                "streamCount": len(stream_starts),
                "paletteOffset": palette_start,
                "paletteSha256": hashlib.sha256(
                    data[palette_start : palette_start + 32]
                ).hexdigest().upper(),
            }
        )
    invocation_table = b"".join(
        addresses[symbol].to_bytes(4, "big") for symbol in invocation_symbols
    )
    invocation_table_address = addresses["pt_InvocationSprites"]
    if rom[
        invocation_table_address : invocation_table_address + len(invocation_table)
    ] != invocation_table:
        raise ValueError("invocation graphics table ROM parity drift")

    status_path = "data/graphics/battles/tech/statusanimation/statusanimationtiles.bin"
    status_data = (disasm / status_path).read_bytes()
    status_address = _check_payload(rom, addresses, "tiles_StatusAnimation", status_data)
    status_stats, status_decoded = _stack_row(status_data)
    if len(status_decoded) != 1248:
        raise ValueError("status-animation output-size drift")
    status_row = {
        "symbol": "tiles_StatusAnimation",
        "sourcePath": status_path,
        "sourceAddress": status_address,
        **status_stats,
    }

    transition_source = read_upstream_text(disasm / TRANSITION_SOURCE)
    transition_rows = []
    transition_symbols = [f"BattlesceneTransitionTiles{i}" for i in range(2)]
    for index, symbol in enumerate(transition_symbols):
        path = (
            "data/graphics/battles/tech/battlescenetransition/"
            f"battlescenetransitiontiles{index}.bin"
        )
        if not re.search(rf'{symbol}:\s*incbin\s+"{re.escape(path)}"', transition_source):
            raise ValueError(f"battle-scene transition definition drift: {symbol}")
        data = (disasm / path).read_bytes()
        source_address = _check_payload(rom, addresses, symbol, data)
        stats, decoded = _stack_row(data)
        if len(decoded) != 6144:
            raise ValueError(f"battle-scene transition output-size drift: {symbol}")
        transition_rows.append(
            {
                "index": index,
                "symbol": symbol,
                "sourcePath": path,
                "sourceAddress": source_address,
                **stats,
            }
        )
    transition_table = b"".join(
        addresses[symbol].to_bytes(4, "big") for symbol in transition_symbols
    )
    transition_table_address = addresses["pt_BattlesceneTransitionTiles"]
    if rom[
        transition_table_address : transition_table_address + len(transition_table)
    ] != transition_table:
        raise ValueError("battle-scene transition table ROM parity drift")

    stream_rows = spell_rows + invocation_streams + [status_row] + transition_rows
    summary = {
        "spellContainerCount": len(spell_rows),
        "invocationContainerCount": len(invocation_rows),
        "invocationFrameCount": sum(row["frameCount"] for row in invocation_rows),
        "invocationStreamCount": len(invocation_streams),
        "statusStreamCount": 1,
        "transitionStreamCount": len(transition_rows),
        "totalStreamCount": len(stream_rows),
        "compressedStreamByteCount": sum(row["compressedBytes"] for row in stream_rows),
        "decodedByteCount": sum(row["decodedBytes"] for row in stream_rows),
        "invocationTransferByteCount": sum(row["transferBytes"] for row in invocation_streams),
        "invocationTransferTailByteCount": sum(
            row["transferTailBytes"] for row in invocation_streams
        ),
        "resourceRomParityCount": len(spell_rows) + len(invocation_rows) + 3,
        "pointerRomParityCount": 4,
        "tableRomParityCount": 3,
        "commandGroupCount": sum(row["commandGroupCount"] for row in stream_rows),
        "literalWordCount": sum(row["literalWordCount"] for row in stream_rows),
        "copyCommandCount": sum(row["copyCommandCount"] for row in stream_rows),
        "copiedWordCount": sum(row["copiedWordCount"] for row in stream_rows),
        "minimumTrailingBits": min(row["trailingBits"] for row in stream_rows),
        "maximumTrailingBits": max(row["trailingBits"] for row in stream_rows),
        "maximumCopyOffsetWords": max(row["maximumCopyOffsetWords"] for row in stream_rows),
        "maximumCopyLengthWords": max(row["maximumCopyLengthWords"] for row in stream_rows),
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "romSha256": rom_hash,
        "function": {
            "loadStackAddress": addresses["LoadStackCompressedData"],
            "LoadInvocationSpriteFrameToVram": addresses["LoadInvocationSpriteFrameToVram"],
            "LoadSpellTileset": addresses["LoadSpellTileset"],
            "LoadSpellTilesetForInvocation": addresses["LoadSpellTilesetForInvocation"],
            "InitializeBattlescene": addresses["InitializeBattlescene"],
            "bsc06_switchEnemies": addresses["bsc06_switchEnemies"],
        },
        "table": table,
        "summary": summary,
        "spellGraphics": spell_rows,
        "invocationContainers": invocation_rows,
        "invocationStreams": invocation_streams,
        "statusAnimation": status_row,
        "transitionGraphics": transition_rows,
        "runtimeQuestions": [
            "What bytes occupy the 512-byte tail of each 4,608-byte invocation-sprite transfer "
            "after a 4,096-byte decode, and are those bytes stable or visible?",
            "Do spell, invocation, status, and transition tiles render with original palettes, "
            "layer ordering, frame timing, and transition composition?",
        ],
    }


def verify_battle_effect_graphics_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_battle_effect_graphics_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="battle-effect graphics decode contract")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != output["romSha256"]
    ):
        raise ValueError("battle-effect graphics provenance drift")
    for field in ("function", "table", "summary", "runtimeQuestions"):
        if fixture[field] != output[field]:
            raise ValueError(f"battle-effect graphics {field} drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"] or output["summary"] != manifest["summary"]:
        raise ValueError("battle-effect graphics canonical output drift")
    destination = output_path or repo_path("local/derived/battle-effect-graphics-decode.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Streams": output["summary"]["totalStreamCount"],
        "DecodedBytes": output["summary"]["decodedByteCount"],
        "InvocationTailBytes": output["summary"]["invocationTransferTailByteCount"],
        "Status": "PASS",
    }
