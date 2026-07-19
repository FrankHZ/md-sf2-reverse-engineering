from __future__ import annotations

import hashlib
import json
import re
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.source_text import read_upstream_text

ID = "sf2-witch-menu-graphics-static-v1"
MANIFEST = repo_path("manifests/extractions/witch-menu-graphics-static.json")
SCHEMA = repo_path("schemas/witch-menu-graphics-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/witch-menu-graphics-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-witch-menu-graphics-static-fixture.schema.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

OWNER_PATH = "code/common/tech/incbins/s06_incbins_graphics.asm"
POINTER_PATH = "code/common/tech/pointers/s06_pointers.asm"
CONSUMER_PATH = "code/specialscreens/witch/witchmainmenu.asm"
PALETTE_SYMBOL = "palette_WitchChoice"
PALETTE_PATH = "data/graphics/specialscreens/witchscreen/witchchoicepalette.bin"
PALETTE_POINTER = "p_palette_WitchChoice"
ANIMATION_SYMBOL = "table_WitchBubbleAnimation"
ANIMATION_PATH = "data/graphics/specialscreens/witchscreen/witchbubbleanimation.bin"
ANIMATION_POINTER = "p_table_WitchBubbleAnimation"

ADDRESS_SYMBOLS = (
    "ExecuteWitchMainMenu",
    "DrawWitchMenuBubble",
    PALETTE_POINTER,
    ANIMATION_POINTER,
    PALETTE_SYMBOL,
    ANIMATION_SYMBOL,
)

OPTION_SOURCE_OFFSETS = (0x000, 0x0F0, 0x1E0, 0x2D0)
OPTION_DESTINATION_OFFSETS = (0x188, 0x004, 0x024, 0x1B0)
SELECTED_TIMER_PHASES = (
    {"timerMinimum": 1, "timerMaximum": 4, "frameIndex": 0},
    {"timerMinimum": 5, "timerMaximum": 9, "frameIndex": 1},
    {"timerMinimum": 10, "timerMaximum": 14, "frameIndex": 2},
    {"timerMinimum": 15, "timerMaximum": 20, "frameIndex": 1},
)


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _assert_source_shape(disasm: Path) -> None:
    owner = read_upstream_text(disasm / OWNER_PATH)
    for symbol, path in (
        (PALETTE_SYMBOL, PALETTE_PATH),
        (ANIMATION_SYMBOL, ANIMATION_PATH),
    ):
        if re.search(
            rf'^{symbol}:\s*\n\s*incbin\s+"{re.escape(path)}"',
            owner,
            re.MULTILINE,
        ) is None:
            raise ValueError(f"witch menu resource ownership drift: {symbol}")

    pointer = read_upstream_text(disasm / POINTER_PATH)
    for symbol, target in (
        (PALETTE_POINTER, PALETTE_SYMBOL),
        (ANIMATION_POINTER, ANIMATION_SYMBOL),
    ):
        if re.search(
            rf"^{symbol}:\s*\n\s*dc\.l\s+{target}\b", pointer, re.MULTILINE
        ) is None:
            raise ValueError(f"witch menu pointer ownership drift: {symbol}")

    consumer = read_upstream_text(disasm / CONSUMER_PATH)
    for fragment in (
        "ExecuteWitchMainMenu:",
        f"movea.l ({PALETTE_POINTER}).l,a0",
        "lea     (PALETTE_2_CURRENT).l,a1",
        "move.w  #CRAM_PALETTE_SIZE,d7",
        "jsr     (CopyBytes).w",
        "jsr     (ApplyVIntCramDma).w",
        "move.w  #$14,var_8(a6)",
        "DrawWitchMenuBubble:",
        f"movea.l ({ANIMATION_POINTER}).l,a0",
        "addi.w  #$50,d2",
        "addi.w  #-$5D00,(a1)+",
        "moveq   #4,d7",
        "moveq   #7,d5",
    ):
        if fragment not in consumer:
            raise ValueError(f"witch menu consumer drift: missing {fragment!r}")
    for offset in OPTION_SOURCE_OFFSETS[1:]:
        if f"move.w  #${offset:X},d2" not in consumer:
            raise ValueError(f"witch bubble source offset drift: {offset:X}")
    for offset, token in zip(
        OPTION_DESTINATION_OFFSETS,
        ("#$188,d1", "#4,d1", "#$24,d1", "#$1B0,d1"),
        strict=True,
    ):
        if token not in consumer:
            raise ValueError(f"witch bubble destination offset drift: {offset:X}")


def build_witch_menu_graphics_contract(
    rom_path: Path, upstream_path: Path
) -> dict[str, Any]:
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"witch menu H1 listing is missing: {listing_path}")
    addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))
    table = {symbol: addresses[symbol] for symbol in ADDRESS_SYMBOLS}

    rom = rom_path.read_bytes()
    rom_hash = hashlib.sha256(rom).hexdigest().upper()
    if rom_hash != load_json(ROM_MANIFEST)["hashes"]["sha256"]:
        raise ValueError("witch menu input ROM identity drift")
    _assert_source_shape(disasm)

    palette = (disasm / PALETTE_PATH).read_bytes()
    animation = (disasm / ANIMATION_PATH).read_bytes()
    if len(palette) != 32 or len(animation) != 960:
        raise ValueError("witch menu resource size drift")
    for symbol, data in ((PALETTE_SYMBOL, palette), (ANIMATION_SYMBOL, animation)):
        address = table[symbol]
        if rom[address : address + len(data)] != data:
            raise ValueError(f"witch menu resource ROM parity drift: {symbol}")
    if table[ANIMATION_SYMBOL] != table[PALETTE_SYMBOL] + len(palette):
        raise ValueError("witch menu resource adjacency drift")
    if addresses["tiles_SpeechBalloon"] != table[ANIMATION_SYMBOL] + len(animation):
        raise ValueError("witch bubble table no longer ends at speech-balloon tiles")

    pointers = []
    for symbol, target in (
        (PALETTE_POINTER, PALETTE_SYMBOL),
        (ANIMATION_POINTER, ANIMATION_SYMBOL),
    ):
        data = table[target].to_bytes(4, "big")
        address = table[symbol]
        if rom[address : address + 4] != data:
            raise ValueError(f"witch menu pointer ROM parity drift: {symbol}")
        pointers.append(
            {
                "symbol": symbol,
                "address": address,
                "targetSymbol": target,
                "targetAddress": table[target],
                "sha256": hashlib.sha256(data).hexdigest().upper(),
            }
        )

    palette_words = [
        int.from_bytes(palette[index : index + 2], "big")
        for index in range(0, len(palette), 2)
    ]
    source_words = [
        int.from_bytes(animation[index : index + 2], "big")
        for index in range(0, len(animation), 2)
    ]
    adjusted_words = [(word - 0x5D00) & 0xFFFF for word in source_words]
    frames = []
    for option_index, option_offset in enumerate(OPTION_SOURCE_OFFSETS):
        for frame_index in range(3):
            offset = option_offset + frame_index * 80
            frame = animation[offset : offset + 80]
            frames.append(
                {
                    "optionIndex": option_index,
                    "frameIndex": frame_index,
                    "sourceOffset": offset,
                    "byteCount": len(frame),
                    "wordCount": len(frame) // 2,
                    "sha256": hashlib.sha256(frame).hexdigest().upper(),
                }
            )
    if len({row["sha256"] for row in frames}) != 12:
        raise ValueError("witch bubble animation frame alias drift")

    palette_indices = Counter((word >> 13) & 3 for word in adjusted_words)
    summary = {
        "resourceCount": 2,
        "resourceByteCount": len(palette) + len(animation),
        "pointerCount": len(pointers),
        "pointerByteCount": len(pointers) * 4,
        "sourceRomParityByteCount": len(palette) + len(animation) + len(pointers) * 4,
        "paletteByteCount": len(palette),
        "paletteColorCount": len(palette_words),
        "paletteUniqueColorCount": len(set(palette_words)),
        "paletteZeroColorCount": palette_words.count(0),
        "animationByteCount": len(animation),
        "optionCount": 4,
        "framesPerOption": 3,
        "frameCount": len(frames),
        "frameWidthTiles": 8,
        "frameHeightTiles": 5,
        "frameWordCount": 40,
        "animationWordCount": len(source_words),
        "uniqueSourceWordCount": len(set(source_words)),
        "uniqueAdjustedWordCount": len(set(adjusted_words)),
        "uniqueTileIndexCount": len({word & 0x7FF for word in adjusted_words}),
        "priorityWordCount": sum(bool(word & 0x8000) for word in adjusted_words),
        "mirrorWordCount": sum(bool(word & 0x0800) for word in adjusted_words),
        "flipWordCount": sum(bool(word & 0x1000) for word in adjusted_words),
        "palette2WordCount": palette_indices[1],
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {
            "repository": toolchain["sf2disasm"]["repository"],
            "commit": commit,
        },
        "romSha256": rom_hash,
        "table": table,
        "summary": summary,
        "resources": [
            {
                "symbol": PALETTE_SYMBOL,
                "sourcePath": PALETTE_PATH,
                "ownerPath": OWNER_PATH,
                "address": table[PALETTE_SYMBOL],
                "byteCount": len(palette),
                "sha256": hashlib.sha256(palette).hexdigest().upper(),
            },
            {
                "symbol": ANIMATION_SYMBOL,
                "sourcePath": ANIMATION_PATH,
                "ownerPath": OWNER_PATH,
                "address": table[ANIMATION_SYMBOL],
                "byteCount": len(animation),
                "sha256": hashlib.sha256(animation).hexdigest().upper(),
            },
        ],
        "pointers": pointers,
        "paletteFacts": {
            "destination": "PALETTE_2_CURRENT",
            "copyByteCount": 32,
            "queuesCramDma": True,
            "colorCount": len(palette_words),
            "uniqueColorCount": len(set(palette_words)),
            "zeroColorCount": palette_words.count(0),
            "maximumChannelNibble": max(
                (word >> shift) & 0xF for word in palette_words for shift in (0, 4, 8)
            ),
        },
        "animationFacts": {
            "optionSourceOffsets": list(OPTION_SOURCE_OFFSETS),
            "optionDestinationOffsets": list(OPTION_DESTINATION_OFFSETS),
            "selectedTimerReset": 20,
            "selectedTimerPhases": list(SELECTED_TIMER_PHASES),
            "unselectedFrameIndex": 0,
            "writeWordAdjustment": -0x5D00,
            "adjustedPaletteIndex": 2,
            "adjustedTileIndexMinimum": min(word & 0x7FF for word in adjusted_words),
            "adjustedTileIndexMaximum": max(word & 0x7FF for word in adjusted_words),
            "allAdjustedWordsHavePriority": all(word & 0x8000 for word in adjusted_words),
        },
        "frames": frames,
        "runtimeQuestions": [
            "Do the 20 timer states, menu redraws, and window motion present the four "
            "bubbles with original frame timing?",
            "Does palette-2 CRAM upload plus the adjusted priority/mirror/flip tilemap "
            "reproduce the original witch menu pixels?",
        ],
    }


def verify_witch_menu_graphics_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_witch_menu_graphics_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="witch menu graphics static contract")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != output["romSha256"]
    ):
        raise ValueError("witch menu graphics provenance drift")
    for field in (
        "table",
        "summary",
        "resourceHashes",
        "paletteFacts",
        "animationFacts",
        "frameHashes",
        "runtimeQuestions",
    ):
        if field == "resourceHashes":
            actual: Any = {row["symbol"]: row["sha256"] for row in output["resources"]}
        elif field == "frameHashes":
            actual = [row["sha256"] for row in output["frames"]]
        else:
            actual = output[field]
        if fixture[field] != actual:
            raise ValueError(f"witch menu graphics fixture drift: {field}")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if output["summary"] != manifest["summary"] or digest != manifest["outputSha256"]:
        raise ValueError("witch menu graphics canonical manifest drift")
    destination = output_path or repo_path("local/derived/witch-menu-graphics-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "PaletteColors": output["summary"]["paletteColorCount"],
        "BubbleFrames": output["summary"]["frameCount"],
        "ParityBytes": output["summary"]["sourceRomParityByteCount"],
        "Status": "PASS",
    }
