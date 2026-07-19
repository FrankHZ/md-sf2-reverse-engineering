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

ID = "sf2-special-screen-graphics-decode-v1"
MANIFEST = repo_path("manifests/extractions/special-screen-graphics-decode.json")
SCHEMA = repo_path("schemas/special-screen-graphics-decode.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/special-screen-graphics-decode-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-special-screen-graphics-decode-fixture.schema.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

RESOURCES = [
    {
        "symbol": "tiles_TitleScreen",
        "sourcePath": "data/graphics/specialscreens/titlescreen/titlescreentiles.bin",
        "definitionPath": "code/specialscreens/title/graphics.asm",
        "consumerPath": "code/specialscreens/title/title.asm",
        "consumerSymbol": "StartTitleScreen",
        "pointerSymbol": None,
        "expectedDecodedBytes": 8192,
        "transferBytes": 8192,
        "transferMethod": "direct-stack-immediate-dma",
        "consumerFragments": ["lea     tiles_TitleScreen(pc), a0", "move.w  #4096,d0"],
    },
    {
        "symbol": "font_TitleScreen",
        "sourcePath": "data/graphics/tech/fonts/titlescreenfont.bin",
        "definitionPath": "code/common/tech/incbins/s06_incbins_titlescreen.asm",
        "consumerPath": "code/specialscreens/title/loadfont.asm",
        "consumerSymbol": "LoadTitleScreenFont",
        "pointerSymbol": None,
        "expectedDecodedBytes": 4096,
        "transferBytes": 4096,
        "transferMethod": "direct-stack-immediate-dma",
        "consumerFragments": ["lea     font_TitleScreen(pc), a0", "move.w  #2048,d0"],
    },
    {
        "symbol": "tiles_SuspendString",
        "sourcePath": "data/graphics/specialscreens/suspendscreen/suspendstringtiles.bin",
        "definitionPath": "code/specialscreens/suspend/graphics.asm",
        "consumerPath": "code/specialscreens/suspend/suspend.asm",
        "consumerSymbol": "SuspendGame",
        "pointerSymbol": "p_tiles_SuspendString",
        "expectedDecodedBytes": 448,
        "transferBytes": 2048,
        "transferMethod": "direct-stack-queued-dma",
        "consumerFragments": ["movea.l (p_tiles_SuspendString).l,a0", "move.w  #$400,d0"],
    },
    {
        "symbol": "tiles_EndingKissPicture",
        "sourcePath": "data/graphics/specialscreens/endingkiss/endingkisspicturetiles.bin",
        "definitionPath": "code/specialscreens/endkiss/graphics.asm",
        "consumerPath": "code/specialscreens/endkiss/endkissfunctions_0.asm",
        "consumerSymbol": "DisplayEndingKissPicture",
        "pointerSymbol": "p_tiles_EndingKiss",
        "expectedDecodedBytes": 6144,
        "transferBytes": None,
        "transferMethod": "direct-stack-pixel-fill-consumer",
        "consumerFragments": [
            "movea.l (p_tiles_EndingKiss).l,a0",
            "bsr.w   DrawEndingKissPictureWithPixelFilling",
        ],
    },
    {
        "symbol": "tiles_EndingWitch",
        "sourcePath": "data/graphics/specialscreens/witchscreen/endingwitchtiles.bin",
        "definitionPath": "code/specialscreens/witchend/graphics.asm",
        "consumerPath": "code/specialscreens/witchend/witchend.asm",
        "consumerSymbol": "EndGame",
        "pointerSymbol": "p_tiles_WitchEnd",
        "expectedDecodedBytes": 7808,
        "transferBytes": 16384,
        "transferMethod": "direct-stack-immediate-dma",
        "consumerFragments": ["movea.l (p_tiles_WitchEnd).l,a0", "move.w  #$2000,d0"],
    },
    {
        "symbol": "tiles_EndingJewels",
        "sourcePath": "data/graphics/specialscreens/endingjewels/endingjewelstiles.bin",
        "definitionPath": "code/specialscreens/jewelend/graphics.asm",
        "consumerPath": "code/specialscreens/witchend/witchend.asm",
        "consumerSymbol": "EndGame",
        "pointerSymbol": "p_tiles_JewelEndScreen",
        "expectedDecodedBytes": 1856,
        "transferBytes": 16384,
        "transferMethod": "direct-stack-immediate-dma",
        "consumerFragments": ["movea.l (p_tiles_JewelEndScreen).l,a0", "move.w  #$2000,d0"],
    },
    {
        "symbol": "tiles_Witch",
        "sourcePath": "data/graphics/specialscreens/witchscreen/witchtiles.bin",
        "definitionPath": "code/specialscreens/witch/graphics.asm",
        "consumerPath": "code/specialscreens/witch/witchfunctions.asm",
        "consumerSymbol": "BuildWitchScreen",
        "pointerSymbol": "p_tiles_Witch",
        "expectedDecodedBytes": 13568,
        "transferBytes": 16384,
        "transferMethod": "direct-stack-immediate-dma",
        "consumerFragments": ["movea.l (p_tiles_Witch).l,a0", "move.w  #8192,d0"],
    },
    {
        "symbol": "tiles_SpeechBalloon",
        "sourcePath": "data/graphics/specialscreens/witchscreen/speechballoontiles.bin",
        "definitionPath": "code/common/tech/incbins/s06_incbins_graphics.asm",
        "consumerPath": "code/specialscreens/witch/witchstart.asm",
        "consumerSymbol": "StartWitchScreen",
        "pointerSymbol": "p_tiles_SpeechBalloon",
        "expectedDecodedBytes": 1920,
        "transferBytes": 2048,
        "transferMethod": "compressed-immediate-dma-wrapper",
        "consumerFragments": ["movea.l (p_tiles_SpeechBalloon).l,a0", "move.w  #1024,d0"],
    },
    {
        "symbol": "tiles_SegaLogo",
        "sourcePath": "data/graphics/tech/segalogotiles.bin",
        "definitionPath": "code/specialscreens/segalogo/segalogo_0.asm",
        "consumerPath": "code/specialscreens/segalogo/segalogo_0.asm",
        "consumerSymbol": "DisplaySegaLogo",
        "pointerSymbol": None,
        "expectedDecodedBytes": 6144,
        "transferBytes": 6144,
        "transferMethod": "compressed-immediate-dma-wrapper",
        "consumerFragments": ["lea     tiles_SegaLogo(pc), a0", "move.w  #$C00,d0"],
    },
]


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def build_special_screen_graphics_contract(
    rom_path: Path, upstream_path: Path
) -> dict[str, Any]:
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"special-screen graphics H1 listing is missing: {listing_path}")
    addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))
    rom = rom_path.read_bytes()
    rom_hash = hashlib.sha256(rom).hexdigest().upper()
    if rom_hash != load_json(ROM_MANIFEST)["hashes"]["sha256"]:
        raise ValueError("special-screen graphics input ROM identity drift")

    rows = []
    consumer_addresses: dict[str, int] = {}
    resource_addresses: dict[str, int] = {}
    for spec in RESOURCES:
        symbol = spec["symbol"]
        source_path = spec["sourcePath"]
        definition = read_upstream_text(disasm / spec["definitionPath"])
        if not re.search(
            rf'^\s*{re.escape(symbol)}:\s*incbin\s+"{re.escape(source_path)}"',
            definition,
            re.MULTILINE,
        ):
            raise ValueError(f"special-screen resource definition drift: {symbol}")
        data = (disasm / source_path).read_bytes()
        source_address = addresses[symbol]
        if rom[source_address : source_address + len(data)] != data:
            raise ValueError(f"special-screen resource ROM parity drift: {symbol}")

        pointer_symbol = spec["pointerSymbol"]
        pointer_address = None
        if pointer_symbol is not None:
            pointer_address = addresses[pointer_symbol]
            if rom[pointer_address : pointer_address + 4] != source_address.to_bytes(4, "big"):
                raise ValueError(f"special-screen pointer ROM parity drift: {pointer_symbol}")

        consumer = read_upstream_text(disasm / spec["consumerPath"])
        for fragment in spec["consumerFragments"]:
            if fragment not in consumer:
                raise ValueError(
                    f"special-screen consumer drift for {symbol}: missing {fragment!r}"
                )
        if spec["transferMethod"].startswith("compressed-"):
            if "ApplyImmediateVramDmaOnCompressedTiles" not in consumer:
                raise ValueError(f"special-screen compressed DMA wrapper drift: {symbol}")
        elif "LoadStackCompressedData" not in consumer:
            raise ValueError(f"special-screen direct Stack call drift: {symbol}")

        decoded = decode_stack_compressed(data)
        if len(decoded.output) != spec["expectedDecodedBytes"]:
            raise ValueError(
                f"special-screen output-size drift for {symbol}: "
                f"expected {spec['expectedDecodedBytes']}, got {len(decoded.output)}"
            )
        transfer_bytes = spec["transferBytes"]
        padding_bytes = None if transfer_bytes is None else transfer_bytes - len(decoded.output)
        if padding_bytes is not None and padding_bytes < 0:
            raise ValueError(f"special-screen DMA shorter than decoded output: {symbol}")

        consumer_symbol = spec["consumerSymbol"]
        consumer_addresses[consumer_symbol] = addresses[consumer_symbol]
        resource_addresses[symbol] = source_address
        rows.append(
            {
                "symbol": symbol,
                "sourcePath": source_path,
                "definitionPath": spec["definitionPath"],
                "sourceAddress": source_address,
                "compressedBytes": len(data),
                "inputBitsConsumed": decoded.input_bits_consumed,
                "trailingBits": len(data) * 8 - decoded.input_bits_consumed,
                "decodedBytes": len(decoded.output),
                "sourceSha256": hashlib.sha256(data).hexdigest().upper(),
                "decodedSha256": hashlib.sha256(decoded.output).hexdigest().upper(),
                "commandGroupCount": decoded.command_group_count,
                "literalWordCount": decoded.literal_word_count,
                "copyCommandCount": decoded.copy_command_count,
                "copiedWordCount": decoded.copied_word_count,
                "maximumCopyOffsetWords": decoded.maximum_copy_offset_words,
                "maximumCopyLengthWords": decoded.maximum_copy_length_words,
                "consumerPath": spec["consumerPath"],
                "consumerSymbol": consumer_symbol,
                "pointerSymbol": pointer_symbol,
                "pointerAddress": pointer_address,
                "transferMethod": spec["transferMethod"],
                "transferBytes": transfer_bytes,
                "transferPaddingBytes": padding_bytes,
            }
        )

    transfer_boundaries = [
        {
            "symbol": row["symbol"],
            "decodedBytes": row["decodedBytes"],
            "transferMethod": row["transferMethod"],
            "transferBytes": row["transferBytes"],
            "transferPaddingBytes": row["transferPaddingBytes"],
        }
        for row in rows
    ]
    fixed_transfer_rows = [row for row in rows if row["transferBytes"] is not None]
    summary = {
        "resourceCount": len(rows),
        "consumerFunctionCount": len(consumer_addresses),
        "directStackCallResourceCount": sum(
            row["transferMethod"].startswith("direct-stack") for row in rows
        ),
        "compressedDmaWrapperResourceCount": sum(
            row["transferMethod"].startswith("compressed-") for row in rows
        ),
        "pixelFillResourceCount": sum(row["transferBytes"] is None for row in rows),
        "fixedTransferResourceCount": len(fixed_transfer_rows),
        "exactTransferResourceCount": sum(
            row["transferPaddingBytes"] == 0 for row in fixed_transfer_rows
        ),
        "oversizedTransferResourceCount": sum(
            row["transferPaddingBytes"] > 0 for row in fixed_transfer_rows
        ),
        "compressedByteCount": sum(row["compressedBytes"] for row in rows),
        "decodedByteCount": sum(row["decodedBytes"] for row in rows),
        "fixedTransferByteCount": sum(row["transferBytes"] for row in fixed_transfer_rows),
        "transferPaddingByteCount": sum(
            row["transferPaddingBytes"] for row in fixed_transfer_rows
        ),
        "commandGroupCount": sum(row["commandGroupCount"] for row in rows),
        "literalWordCount": sum(row["literalWordCount"] for row in rows),
        "copyCommandCount": sum(row["copyCommandCount"] for row in rows),
        "copiedWordCount": sum(row["copiedWordCount"] for row in rows),
        "minimumTrailingBits": min(row["trailingBits"] for row in rows),
        "maximumTrailingBits": max(row["trailingBits"] for row in rows),
        "maximumCopyOffsetWords": max(row["maximumCopyOffsetWords"] for row in rows),
        "maximumCopyLengthWords": max(row["maximumCopyLengthWords"] for row in rows),
        "resourceRomParityCount": len(rows),
        "pointerRomParityCount": sum(row["pointerSymbol"] is not None for row in rows),
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "romSha256": rom_hash,
        "function": {
            "loadStackAddress": addresses["LoadStackCompressedData"],
            **consumer_addresses,
        },
        "table": resource_addresses,
        "summary": summary,
        "transferBoundaries": transfer_boundaries,
        "resources": rows,
        "runtimeQuestions": [
            "What bytes occupy the fixed DMA tails for the five special-screen resources whose "
            "transfer length exceeds decoder output, and are those bytes cleared or stable?",
            "Do decoded special-screen tiles, layouts, palettes, transitions, and pixel-fill order "
            "match rendered original frames?",
        ],
    }


def verify_special_screen_graphics_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_special_screen_graphics_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="special-screen graphics decode contract")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != output["romSha256"]
    ):
        raise ValueError("special-screen graphics provenance drift")
    for field in (
        "function",
        "table",
        "summary",
        "transferBoundaries",
        "runtimeQuestions",
    ):
        if fixture[field] != output[field]:
            raise ValueError(f"special-screen graphics {field} drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"] or output["summary"] != manifest["summary"]:
        raise ValueError("special-screen graphics canonical output drift")
    destination = output_path or repo_path("local/derived/special-screen-graphics-decode.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Resources": output["summary"]["resourceCount"],
        "DecodedBytes": output["summary"]["decodedByteCount"],
        "OversizedTransfers": output["summary"]["oversizedTransferResourceCount"],
        "Status": "PASS",
    }
