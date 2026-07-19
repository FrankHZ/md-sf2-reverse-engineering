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

ID = "sf2-special-sprite-decode-v1"
POINTERS_PATH = Path("data/graphics/specialsprites/pointers.asm")
ENTRIES_PATH = Path("data/graphics/specialsprites/entries.asm")
CONSUMER_PATH = Path("code/common/tech/graphics/specialsprites.asm")
ENUMS_PATH = Path("sf2enums.asm")
MANIFEST = repo_path("manifests/extractions/special-sprite-decode.json")
SCHEMA = repo_path("schemas/special-sprite-decode.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/special-sprite-decode-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-special-sprite-decode-fixture.schema.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

SPECIAL_MAPSPRITES = [
    (240, "MAPSPRITE_SPECIAL15"),
    (241, "MAPSPRITE_SPECIAL14"),
    (242, "MAPSPRITE_SPECIAL13"),
    (243, "MAPSPRITE_SPECIAL12"),
    (244, "MAPSPRITE_SPECIAL11"),
    (245, "MAPSPRITE_SPECIAL10"),
    (246, "MAPSPRITE_SPECIAL9"),
    (247, "MAPSPRITE_SPECIAL8"),
    (248, "MAPSPRITE_SPECIAL7"),
    (249, "MAPSPRITE_SPECIAL6"),
    (250, "MAPSPRITE_SPECIAL5"),
    (251, "MAPSPRITE_ZEON"),
    (252, "MAPSPRITE_EVIL_SPIRIT"),
    (253, "MAPSPRITE_NAZCA_SHIP"),
    (254, "MAPSPRITE_KRAKEN_HEAD"),
    (255, "MAPSPRITE_TAROS"),
]
ANIMATION_ONLY_SYMBOL = "SpecialSprite_EvilSpiritAlt"


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _relative_table(source: str, label: str, next_marker: str) -> list[str]:
    match = re.search(
        rf"^{re.escape(label)}:\s*(?P<body>.*?)(?=^{re.escape(next_marker)})",
        source,
        re.MULTILINE | re.DOTALL,
    )
    if not match:
        raise ValueError(f"special-sprite dispatch table missing: {label}")
    return re.findall(r"^\s*dc\.w\s+([A-Za-z0-9_]+)-", match.group("body"), re.MULTILINE)


def _symbol_references(
    disasm: Path, symbols: list[str]
) -> dict[str, tuple[int, list[str]]]:
    pattern = re.compile(r"\b(?:" + "|".join(map(re.escape, symbols)) + r")\b")
    counts = {symbol: 0 for symbol in symbols}
    paths = {symbol: [] for symbol in symbols}
    for path in sorted(disasm.rglob("*.asm"), key=lambda item: item.as_posix()):
        relative = path.relative_to(disasm)
        if relative == ENUMS_PATH:
            continue
        matches = pattern.findall(read_upstream_text(path))
        for symbol in sorted(set(matches)):
            counts[symbol] += matches.count(symbol)
            paths[symbol].append(relative.as_posix())
    return {symbol: (counts[symbol], paths[symbol]) for symbol in symbols}


def build_special_sprite_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"special-sprite H1 listing is missing: {listing_path}")
    addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))
    rom = rom_path.read_bytes()
    rom_hash = hashlib.sha256(rom).hexdigest().upper()
    if rom_hash != load_json(ROM_MANIFEST)["hashes"]["sha256"]:
        raise ValueError("special-sprite input ROM identity drift")

    enums = read_upstream_text(disasm / ENUMS_PATH)
    for value, symbol in SPECIAL_MAPSPRITES:
        if not re.search(rf"^{re.escape(symbol)}:\s+equ\s+{value}\b", enums, re.MULTILINE):
            raise ValueError(f"special-sprite enum drift: {symbol}")

    pointer_source = read_upstream_text(disasm / POINTERS_PATH)
    pointer_symbols = re.findall(
        r"^\s*dc\.l\s+(SpecialSprite_[A-Za-z0-9]+)", pointer_source, re.MULTILINE
    )
    if len(pointer_symbols) != 10:
        raise ValueError(f"special-sprite pointer-count drift: {len(pointer_symbols)}")
    pointer_address = addresses["pt_SpecialSprites"]
    pointer_bytes = b"".join(addresses[symbol].to_bytes(4, "big") for symbol in pointer_symbols)
    if rom[pointer_address : pointer_address + len(pointer_bytes)] != pointer_bytes:
        raise ValueError("special-sprite pointer-table ROM parity drift")

    entry_source = read_upstream_text(disasm / ENTRIES_PATH)
    definitions = re.findall(
        r'^\s*(SpecialSprite_[A-Za-z0-9]+):\s*incbin\s+"([^"]+)"',
        entry_source,
        re.MULTILINE,
    )
    if len(definitions) != 6:
        raise ValueError(f"special-sprite payload-count drift: {len(definitions)}")

    resources = []
    for symbol, relative_path in definitions:
        data = (disasm / relative_path).read_bytes()
        source_address = addresses[symbol]
        if rom[source_address : source_address + len(data)] != data:
            raise ValueError(f"special-sprite payload ROM parity drift: {symbol}")
        animation_only = symbol == ANIMATION_ONLY_SYMBOL
        palette_bytes = 0 if animation_only else 32
        stream = data[palette_bytes:]
        decoded = decode_stack_compressed(stream)
        expected_bytes = 0x1440 if symbol == "SpecialSprite_NazcaShip" else 0x900
        if len(decoded.output) != expected_bytes:
            raise ValueError(
                f"special-sprite output-size drift for {symbol}: "
                f"expected {expected_bytes}, got {len(decoded.output)}"
            )
        row = {
            "symbol": symbol,
            "sourcePath": relative_path,
            "sourceAddress": source_address,
            "sourceBytes": len(data),
            "animationOnly": animation_only,
            "paletteBytes": palette_bytes,
            "compressedBytes": len(stream),
            "inputBitsConsumed": decoded.input_bits_consumed,
            "trailingBits": len(stream) * 8 - decoded.input_bits_consumed,
            "decodedBytes": len(decoded.output),
            "sourceSha256": hashlib.sha256(data).hexdigest().upper(),
            "decodedSha256": hashlib.sha256(decoded.output).hexdigest().upper(),
            "commandGroupCount": decoded.command_group_count,
            "literalWordCount": decoded.literal_word_count,
            "copyCommandCount": decoded.copy_command_count,
            "copiedWordCount": decoded.copied_word_count,
            "maximumCopyOffsetWords": decoded.maximum_copy_offset_words,
            "maximumCopyLengthWords": decoded.maximum_copy_length_words,
        }
        if palette_bytes:
            row["paletteSha256"] = hashlib.sha256(data[:palette_bytes]).hexdigest().upper()
        resources.append(row)

    corpus_start = addresses[definitions[0][0]]
    corpus_end = addresses["table_2784C"]
    if corpus_end - corpus_start != sum(row["sourceBytes"] for row in resources):
        raise ValueError("special-sprite contiguous source boundary drift")

    consumer = read_upstream_text(disasm / CONSUMER_PATH)
    load_dispatch = _relative_table(consumer, "rjt_SpecialSpriteFunctions", "specialSprite_Battle:")
    update_dispatch = _relative_table(
        consumer, "rjt_SpecialSpriteUpdates", "specialSpriteUpdate_Battle:"
    )
    if len(load_dispatch) != 9 or len(update_dispatch) != 9:
        raise ValueError(
            "special-sprite dispatch-count drift: "
            f"load {len(load_dispatch)}, update {len(update_dispatch)}"
        )
    for fragment in (
        "move.b  #MAPSPRITES_SPECIALS_END,d0",
        "sub.b   d1,d0",
        "movea.l pt_SpecialSprites(pc,d0.w),a0",
        "lea     (PALETTE_4_BASE).l,a1",
        "jsr     (LoadStackCompressedData).w",
        "lea     (SpecialSprite_EvilSpirit+$20)(pc), a0",
        "lea     SpecialSprite_EvilSpiritAlt(pc), a0",
        "lea     (SpecialSprite_Zeon+$20)(pc), a0",
    ):
        if fragment not in consumer:
            raise ValueError(f"special-sprite consumer source-shape drift: {fragment!r}")

    reference_symbols = [symbol for _, symbol in SPECIAL_MAPSPRITES] + [
        "MAPSPRITE_FREE_SPOT1",
        "MAPSPRITE_FREE_SPOT2",
        "MAPSPRITE_FREE_SPOT3",
    ]
    source_references = _symbol_references(disasm, reference_symbols)

    first_slot: dict[str, int] = {}
    aliases = []
    for slot, symbol in enumerate(pointer_symbols):
        if symbol in first_slot:
            aliases.append({"slot": slot, "payloadSymbol": symbol, "ownerSlot": first_slot[symbol]})
        else:
            first_slot[symbol] = slot

    routing = []
    for mapsprite, enum_symbol in SPECIAL_MAPSPRITES:
        special_index = 255 - mapsprite
        reference_count, reference_paths = source_references[enum_symbol]
        routing.append(
            {
                "mapsprite": mapsprite,
                "enumSymbol": enum_symbol,
                "specialIndex": special_index,
                "pointerSymbol": (
                    pointer_symbols[special_index] if special_index < len(pointer_symbols) else None
                ),
                "loadDispatchTarget": (
                    load_dispatch[special_index] if special_index < len(load_dispatch) else None
                ),
                "updateDispatchTarget": (
                    update_dispatch[special_index] if special_index < len(update_dispatch) else None
                ),
                "sourceReferenceCount": reference_count,
                "sourceReferencePaths": reference_paths,
            }
        )

    regular_sentinel_references = []
    for mapsprite, enum_symbol in (
        (237, "MAPSPRITE_FREE_SPOT1"),
        (238, "MAPSPRITE_FREE_SPOT2"),
        (239, "MAPSPRITE_FREE_SPOT3"),
    ):
        reference_count, reference_paths = source_references[enum_symbol]
        regular_sentinel_references.append(
            {
                "mapsprite": mapsprite,
                "enumSymbol": enum_symbol,
                "sourceReferenceCount": reference_count,
                "sourceReferencePaths": reference_paths,
            }
        )

    summary = {
        "pointerCount": len(pointer_symbols),
        "uniquePointerPayloadCount": len(set(pointer_symbols)),
        "aliasPointerCount": len(aliases),
        "resourceCount": len(resources),
        "paletteCount": sum(row["paletteBytes"] > 0 for row in resources),
        "animationOnlyStreamCount": sum(row["animationOnly"] for row in resources),
        "battleSizedStreamCount": sum(row["decodedBytes"] == 0x900 for row in resources),
        "explorationSizedStreamCount": sum(row["decodedBytes"] == 0x1440 for row in resources),
        "sourceByteCount": sum(row["sourceBytes"] for row in resources),
        "paletteByteCount": sum(row["paletteBytes"] for row in resources),
        "compressedByteCount": sum(row["compressedBytes"] for row in resources),
        "decodedByteCount": sum(row["decodedBytes"] for row in resources),
        "commandGroupCount": sum(row["commandGroupCount"] for row in resources),
        "literalWordCount": sum(row["literalWordCount"] for row in resources),
        "copyCommandCount": sum(row["copyCommandCount"] for row in resources),
        "copiedWordCount": sum(row["copiedWordCount"] for row in resources),
        "minimumTrailingBits": min(row["trailingBits"] for row in resources),
        "maximumTrailingBits": max(row["trailingBits"] for row in resources),
        "maximumCopyOffsetWords": max(row["maximumCopyOffsetWords"] for row in resources),
        "maximumCopyLengthWords": max(row["maximumCopyLengthWords"] for row in resources),
        "loadDispatchSlotCount": len(load_dispatch),
        "updateDispatchSlotCount": len(update_dispatch),
        "fullyRoutedMapSpriteCount": sum(
            row["pointerSymbol"] is not None
            and row["loadDispatchTarget"] is not None
            and row["updateDispatchTarget"] is not None
            for row in routing
        ),
        "pointerOnlyMapSpriteCount": sum(
            row["pointerSymbol"] is not None
            and (row["loadDispatchTarget"] is None or row["updateDispatchTarget"] is None)
            for row in routing
        ),
        "unbackedSpecialMapSpriteCount": sum(row["pointerSymbol"] is None for row in routing),
        "sourceReferencedSpecialMapSpriteCount": sum(
            row["sourceReferenceCount"] > 0 for row in routing
        ),
        "regularSentinelSymbolReferenceCount": sum(
            row["sourceReferenceCount"] for row in regular_sentinel_references
        ),
        "pointerTableRomParityCount": len(pointer_symbols),
        "payloadRomParityCount": len(resources),
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "romSha256": rom_hash,
        "function": {
            "loadSpecialSpriteAddress": addresses["LoadSpecialSprite"],
            "animateSpecialSpriteAddress": addresses["AnimateSpecialSprite"],
            "loadStackAddress": addresses["LoadStackCompressedData"],
        },
        "table": {
            "specialSpriteTableAddress": pointer_address,
            "firstPayloadAddress": corpus_start,
            "corpusEndAddress": corpus_end,
        },
        "summary": summary,
        "aliases": aliases,
        "routing": routing,
        "regularSentinelReferences": regular_sentinel_references,
        "resources": resources,
        "runtimeQuestions": [
            "Are map-sprite IDs 240-246 unreachable at runtime, as required by the missing pointer "
            "and/or dispatch slots even though the route threshold admits them?",
            "Can any runtime write or encoded script value select regular map-sprite IDs 237-239 "
            "despite the absence of symbolic source references?",
        ],
    }


def verify_special_sprite_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_special_sprite_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="special sprite decode contract")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != output["romSha256"]
    ):
        raise ValueError("special-sprite provenance drift")
    for field in (
        "function",
        "table",
        "summary",
        "routing",
        "regularSentinelReferences",
        "runtimeQuestions",
    ):
        if fixture[field] != output[field]:
            raise ValueError(f"special-sprite {field} drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"] or output["summary"] != manifest["summary"]:
        raise ValueError("special-sprite canonical output drift")
    destination = output_path or repo_path("local/derived/special-sprite-decode.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Pointers": output["summary"]["pointerCount"],
        "Streams": output["summary"]["resourceCount"],
        "DecodedBytes": output["summary"]["decodedByteCount"],
        "FullyRoutedIds": output["summary"]["fullyRoutedMapSpriteCount"],
        "Status": "PASS",
    }
