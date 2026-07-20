from __future__ import annotations

import hashlib
import json
import re
from pathlib import Path
from typing import Any

from sf2tool.compression import StackDecodeResult, decode_stack_compressed
from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.source_text import read_upstream_text

ID = "sf2-unused-technical-assets-static-v1"
MANIFEST = repo_path("manifests/extractions/unused-technical-assets-static.json")
SCHEMA = repo_path("schemas/unused-technical-assets-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/unused-technical-assets-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-unused-technical-assets-static-fixture.schema.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

CLOUD_SYMBOL = "tiles_UnusedCloud"
CLOUD_PATH = "data/graphics/tech/unusedcloudtiles.bin"
CLOUD_OWNER_PATH = "code/common/tech/incbins/s06_incbins_titlescreen.asm"
PALETTE_SYMBOL = "palette_UnusedBase"
PALETTE_POINTER_SYMBOL = "p_palette_UnusedBase"
PALETTE_PATH = "data/graphics/tech/unusedbasepalettes.bin"
PALETTE_OWNER_PATH = "code/common/tech/incbins/s17_incbins_basetiles.asm"
PALETTE_POINTER_PATH = "code/common/tech/pointers/s17_pointers.asm"


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _asm_symbol_counts(disasm: Path, symbols: tuple[str, ...]) -> dict[str, int]:
    counts = dict.fromkeys(symbols, 0)
    for path in (disasm / "code").rglob("*.asm"):
        source = read_upstream_text(path)
        code = "\n".join(line.split(";", 1)[0] for line in source.splitlines())
        for symbol in symbols:
            counts[symbol] += len(re.findall(rf"\b{re.escape(symbol)}\b", code))
    return counts


def _find_cloud_streams(data: bytes) -> list[tuple[int, StackDecodeResult]]:
    candidates: list[tuple[int, StackDecodeResult]] = []
    for start in range(0, len(data), 2):
        try:
            decoded = decode_stack_compressed(
                data[start:], expected_output_bytes=8192
            )
        except ValueError:
            continue
        candidates.append((start, decoded))
    if [start for start, _ in candidates] != [0, 1328, 2696, 4460]:
        raise ValueError(
            "unused cloud Stack-stream boundary drift: "
            f"{[start for start, _ in candidates]}"
        )
    return candidates


def _assert_source_shape(disasm: Path) -> None:
    cloud_owner = read_upstream_text(disasm / CLOUD_OWNER_PATH)
    if re.search(
        rf'^{CLOUD_SYMBOL}:\s*\n\s*incbin\s+"{re.escape(CLOUD_PATH)}"',
        cloud_owner,
        re.MULTILINE,
    ) is None:
        raise ValueError("unused cloud incbin ownership drift")
    palette_owner = read_upstream_text(disasm / PALETTE_OWNER_PATH)
    if re.search(
        rf'^{PALETTE_SYMBOL}:\s*\n\s*incbin\s+"{re.escape(PALETTE_PATH)}"',
        palette_owner,
        re.MULTILINE,
    ) is None:
        raise ValueError("unused base palette incbin ownership drift")
    pointer_owner = read_upstream_text(disasm / PALETTE_POINTER_PATH)
    if re.search(
        rf"^{PALETTE_POINTER_SYMBOL}:\s*\n\s*dc\.l\s+{PALETTE_SYMBOL}\b",
        pointer_owner,
        re.MULTILINE,
    ) is None:
        raise ValueError("unused base palette pointer ownership drift")


def build_unused_technical_assets_contract(
    rom_path: Path, upstream_path: Path
) -> dict[str, Any]:
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"unused technical assets H1 listing is missing: {listing_path}")
    addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))

    rom = rom_path.read_bytes()
    rom_hash = hashlib.sha256(rom).hexdigest().upper()
    if rom_hash != load_json(ROM_MANIFEST)["hashes"]["sha256"]:
        raise ValueError("unused technical assets input ROM identity drift")
    _assert_source_shape(disasm)

    cloud = (disasm / CLOUD_PATH).read_bytes()
    cloud_address = addresses[CLOUD_SYMBOL]
    if rom[cloud_address : cloud_address + len(cloud)] != cloud:
        raise ValueError("unused cloud source/ROM parity drift")
    candidates = _find_cloud_streams(cloud)
    stream_starts = [start for start, _ in candidates]
    stream_ends = stream_starts[1:] + [len(cloud)]
    streams = []
    for index, ((start, decoded), end) in enumerate(zip(candidates, stream_ends, strict=True)):
        consumed_byte_count = (decoded.input_bits_consumed + 7) // 8
        stored = cloud[start:end]
        streams.append(
            {
                "index": index,
                "address": cloud_address + start,
                "startOffset": start,
                "endOffsetExclusive": end,
                "storedByteCount": len(stored),
                "inputBitsConsumed": decoded.input_bits_consumed,
                "trailingStorageBitCount": len(stored) * 8
                - decoded.input_bits_consumed,
                "trailingWholeByteCount": len(stored) - consumed_byte_count,
                "decodedByteCount": len(decoded.output),
                "tileCount": len(decoded.output) // 32,
                "sourceSha256": hashlib.sha256(stored).hexdigest().upper(),
                "decodedSha256": hashlib.sha256(decoded.output).hexdigest().upper(),
                "commandGroupCount": decoded.command_group_count,
                "literalWordCount": decoded.literal_word_count,
                "copyCommandCount": decoded.copy_command_count,
                "copiedWordCount": decoded.copied_word_count,
                "maximumCopyOffsetWords": decoded.maximum_copy_offset_words,
                "maximumCopyLengthWords": decoded.maximum_copy_length_words,
            }
        )
    if len({stream["decodedSha256"] for stream in streams}) != len(streams):
        raise ValueError("unused cloud decoded streams are not unique")

    palette = (disasm / PALETTE_PATH).read_bytes()
    if len(palette) != 64:
        raise ValueError(f"unused base palette size drift: {len(palette)}")
    palette_address = addresses[PALETTE_SYMBOL]
    pointer_address = addresses[PALETTE_POINTER_SYMBOL]
    if rom[palette_address : palette_address + len(palette)] != palette:
        raise ValueError("unused base palette source/ROM parity drift")
    pointer_bytes = palette_address.to_bytes(4, "big")
    if rom[pointer_address : pointer_address + 4] != pointer_bytes:
        raise ValueError("unused base palette pointer ROM parity drift")
    colors = [
        int.from_bytes(palette[index : index + 2], "big")
        for index in range(0, len(palette), 2)
    ]
    if any(color & 0xF111 for color in colors):
        raise ValueError("unused base palette contains invalid Mega Drive color bits")
    first, second = colors[:16], colors[16:]
    differences = [
        {"colorIndex": index, "first": left, "second": right}
        for index, (left, right) in enumerate(zip(first, second, strict=True))
        if left != right
    ]

    reference_counts = _asm_symbol_counts(
        disasm, (CLOUD_SYMBOL, PALETTE_SYMBOL, PALETTE_POINTER_SYMBOL)
    )
    expected_counts = {
        CLOUD_SYMBOL: 1,
        PALETTE_SYMBOL: 2,
        PALETTE_POINTER_SYMBOL: 1,
    }
    if reference_counts != expected_counts:
        raise ValueError(f"unused technical asset reference drift: {reference_counts}")
    table = {
        CLOUD_SYMBOL: cloud_address,
        "tiles_UnusedCloudStream1": streams[1]["address"],
        "tiles_UnusedCloudStream2": streams[2]["address"],
        "tiles_UnusedCloudStream3": streams[3]["address"],
        PALETTE_SYMBOL: palette_address,
        PALETTE_POINTER_SYMBOL: pointer_address,
    }
    summary = {
        "resourceCount": 2,
        "addressCount": len(table),
        "cloudCompressedByteCount": len(cloud),
        "cloudStreamCount": len(streams),
        "cloudDecodedByteCount": sum(stream["decodedByteCount"] for stream in streams),
        "cloudTileCount": sum(stream["tileCount"] for stream in streams),
        "cloudUniqueDecodedStreamCount": len(
            {stream["decodedSha256"] for stream in streams}
        ),
        "cloudTrailingStorageBitCount": sum(
            stream["trailingStorageBitCount"] for stream in streams
        ),
        "paletteByteCount": len(palette),
        "paletteCount": 2,
        "paletteColorCount": len(colors),
        "paletteUniqueColorCount": len(set(colors)),
        "paletteZeroColorCount": colors.count(0),
        "paletteDifferenceCount": len(differences),
        "pointerByteCount": len(pointer_bytes),
        "symbolicConsumerCount": 0,
        "sourceRomParityByteCount": len(cloud) + len(palette) + len(pointer_bytes),
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
        "cloud": {
            "symbol": CLOUD_SYMBOL,
            "sourcePath": CLOUD_PATH,
            "ownerPath": CLOUD_OWNER_PATH,
            "address": cloud_address,
            "byteCount": len(cloud),
            "sha256": hashlib.sha256(cloud).hexdigest().upper(),
            "streamBoundaryDiscovery": "all even offsets yielding exactly 8192 Stack-decoded bytes",
            "streams": streams,
        },
        "basePalettes": {
            "symbol": PALETTE_SYMBOL,
            "sourcePath": PALETTE_PATH,
            "ownerPath": PALETTE_OWNER_PATH,
            "address": palette_address,
            "byteCount": len(palette),
            "sha256": hashlib.sha256(palette).hexdigest().upper(),
            "paletteCount": 2,
            "colorsPerPalette": 16,
            "differences": differences,
            "pointerSymbol": PALETTE_POINTER_SYMBOL,
            "pointerPath": PALETTE_POINTER_PATH,
            "pointerAddress": pointer_address,
            "pointerSha256": hashlib.sha256(pointer_bytes).hexdigest().upper(),
        },
        "reachabilityFacts": {
            "asmTokenCounts": reference_counts,
            "cloudHasNoSymbolicConsumer": True,
            "basePaletteHasOnlyItsPointerReference": True,
            "basePalettePointerHasNoSymbolicConsumer": True,
            "rawAddressOrComputedAccessExcludedFromStaticClaim": True,
            "resourceNamesAndCommentsDoNotProveRenderedMeaning": True,
        },
        "runtimeQuestions": [
            "Can an original-game path reach either payload through a raw address, computed "
            "pointer, or debug-only state despite the absence of symbolic consumers?",
            "If reached, what frame order, palette assignment, VDP destination, and rendered "
            "meaning apply to the four decoded tile streams?",
        ],
    }


def verify_unused_technical_assets_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_unused_technical_assets_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="unused technical assets static contract")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != output["romSha256"]
    ):
        raise ValueError("unused technical assets provenance drift")
    for field in (
        "table",
        "summary",
        "cloudFacts",
        "streamFacts",
        "paletteFacts",
        "reachabilityFacts",
        "runtimeQuestions",
    ):
        actual: Any
        if field == "cloudFacts":
            actual = {
                "byteCount": output["cloud"]["byteCount"],
                "sha256": output["cloud"]["sha256"],
                "streamBoundaryDiscovery": output["cloud"]["streamBoundaryDiscovery"],
            }
        elif field == "streamFacts":
            actual = [
                {
                    key: stream[key]
                    for key in (
                        "index",
                        "address",
                        "startOffset",
                        "endOffsetExclusive",
                        "storedByteCount",
                        "inputBitsConsumed",
                        "trailingStorageBitCount",
                        "decodedByteCount",
                        "tileCount",
                        "sourceSha256",
                        "decodedSha256",
                    )
                }
                for stream in output["cloud"]["streams"]
            ]
        elif field == "paletteFacts":
            actual = {
                key: output["basePalettes"][key]
                for key in (
                    "byteCount",
                    "sha256",
                    "paletteCount",
                    "colorsPerPalette",
                    "differences",
                    "pointerSha256",
                )
            }
        else:
            actual = output[field]
        if fixture[field] != actual:
            raise ValueError(f"unused technical assets fixture drift: {field}")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if output["summary"] != manifest["summary"] or digest != manifest["outputSha256"]:
        raise ValueError("unused technical assets canonical manifest drift")
    destination = output_path or repo_path("local/derived/unused-technical-assets-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "CloudStreams": output["summary"]["cloudStreamCount"],
        "DecodedBytes": output["summary"]["cloudDecodedByteCount"],
        "ParityBytes": output["summary"]["sourceRomParityByteCount"],
        "Status": "PASS",
    }
