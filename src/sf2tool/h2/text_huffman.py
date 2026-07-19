from __future__ import annotations

import hashlib
import json
import re
from collections import Counter, deque
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.h2.variable_width_font import ASCII_TABLE_PATH, _parse_ascii_map
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.source_text import read_upstream_text

ID = "sf2-text-huffman-static-v1"
MANIFEST = repo_path("manifests/extractions/text-huffman-static.json")
SCHEMA = repo_path("schemas/text-huffman-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/text-huffman-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-text-huffman-static-fixture.schema.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

OFFSET_SYMBOL = "TextBankTreeOffsets"
TREE_SYMBOL = "TextBankTreeData"
INITIALIZER_SYMBOL = "InitializeHuffmanDecoder"
DECODER_SYMBOL = "HuffmanDecode"
OFFSET_PATH = "data/scripting/text/huffmantreeoffsets.bin"
TREE_PATH = "data/scripting/text/huffmantrees.bin"
OFFSET_NOTE_PATH = "data/scripting/text/huffmantreeoffsets.txt"
TREE_NOTE_PATH = "data/scripting/text/huffmantrees.txt"
OWNER_PATH = "code/common/tech/incbins/s06_incbins_textbanktrees.asm"
DECODER_PATH = "code/common/scripting/text/decoding.asm"
ADDRESS_SYMBOLS = (
    OFFSET_SYMBOL,
    TREE_SYMBOL,
    INITIALIZER_SYMBOL,
    DECODER_SYMBOL,
)


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _missing_ranges(values: list[int]) -> list[dict[str, int]]:
    ranges: list[dict[str, int]] = []
    for value in values:
        if not ranges or value != ranges[-1]["end"] + 1:
            ranges.append({"start": value, "end": value})
        else:
            ranges[-1]["end"] = value
    return ranges


def _parse_tree(data: bytes, context_symbol: int, offset: int) -> dict[str, Any]:
    bits: list[int] = []
    open_slots = 1
    while open_slots:
        byte_index = offset + len(bits) // 8
        if byte_index >= len(data):
            raise ValueError(f"Huffman tree {context_symbol} exceeds tree data")
        bit = (data[byte_index] >> (7 - len(bits) % 8)) & 1
        bits.append(bit)
        open_slots += 1 if bit == 0 else -1

    leaf_count = (len(bits) + 1) // 2
    symbol_start = offset - leaf_count
    if symbol_start < 0:
        raise ValueError(f"Huffman tree {context_symbol} has negative symbol start")
    leaf_symbols = list(reversed(data[symbol_start:offset]))
    position = 0
    leaf_index = 0
    entries: list[dict[str, Any]] = []

    def visit(code: str) -> None:
        nonlocal position, leaf_index
        if position >= len(bits):
            raise ValueError(f"Huffman tree {context_symbol} ended inside a branch")
        bit = bits[position]
        position += 1
        if bit:
            entries.append(
                {
                    "symbol": leaf_symbols[leaf_index],
                    "code": code,
                    "codeBitLength": len(code),
                }
            )
            leaf_index += 1
            return
        visit(code + "0")
        visit(code + "1")

    visit("")
    if position != len(bits) or leaf_index != leaf_count:
        raise ValueError(f"Huffman tree {context_symbol} traversal did not close")
    if len({entry["symbol"] for entry in entries}) != leaf_count:
        raise ValueError(f"Huffman tree {context_symbol} repeats a leaf symbol")

    node_byte_count = (len(bits) + 7) // 8
    padding_bit_count = node_byte_count * 8 - len(bits)
    padding_value = (
        data[offset + node_byte_count - 1] & ((1 << padding_bit_count) - 1)
        if padding_bit_count
        else 0
    )
    if padding_value:
        raise ValueError(f"Huffman tree {context_symbol} has nonzero padding bits")
    code_lengths = [entry["codeBitLength"] for entry in entries]
    return {
        "contextSymbol": context_symbol,
        "offset": offset,
        "symbolStartOffset": symbol_start,
        "endOffsetExclusive": offset + node_byte_count,
        "leafCount": leaf_count,
        "nonLeafCount": leaf_count - 1,
        "nodeBitCount": len(bits),
        "nodeByteCount": node_byte_count,
        "paddingBitCount": padding_bit_count,
        "minimumCodeBitLength": min(code_lengths),
        "maximumCodeBitLength": max(code_lengths),
        "entries": entries,
    }


def _assert_source_shape(disasm: Path) -> None:
    owner = read_upstream_text(disasm / OWNER_PATH)
    expected = (
        (OFFSET_SYMBOL, OFFSET_PATH),
        (TREE_SYMBOL, TREE_PATH),
    )
    for symbol, source_path in expected:
        if re.search(
            rf'^{symbol}:\s*\n\s*incbin\s+"{re.escape(source_path)}"',
            owner,
            re.MULTILINE,
        ) is None:
            raise ValueError(f"Huffman incbin ownership drift: {symbol}")

    decoder = read_upstream_text(disasm / DECODER_PATH)
    for fragment in (
        "move.b  #$FE,(DECODED_TEXT_SYMBOL).l",
        "move.b  2(a3),d1",
        "add.w   d1,d1",
        f"lea     {OFFSET_SYMBOL}(pc), a1",
        "move.w  (a1,d1.w),d1",
        f"lea     {TREE_SYMBOL}(pc), a1",
        "adda.w  d1,a1",
        "add.b   d2,d2",
        "bcs.s   loc_2E182",
        "add.b   d7,d7",
        "bcc.s   loc_2E150",
        "move.b  -1(a2,d5.w),d0",
        "move.b  d0,2(a3)",
    ):
        if fragment not in decoder:
            raise ValueError(f"Huffman decoder drift: missing {fragment!r}")


def build_text_huffman_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"text Huffman H1 listing is missing: {listing_path}")
    addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))
    table = {symbol: addresses[symbol] for symbol in ADDRESS_SYMBOLS}

    rom = rom_path.read_bytes()
    rom_hash = hashlib.sha256(rom).hexdigest().upper()
    if rom_hash != load_json(ROM_MANIFEST)["hashes"]["sha256"]:
        raise ValueError("text Huffman input ROM identity drift")

    _assert_source_shape(disasm)
    offset_bytes = (disasm / OFFSET_PATH).read_bytes()
    tree_data = (disasm / TREE_PATH).read_bytes()
    if len(offset_bytes) % 2:
        raise ValueError("Huffman offset table has an odd byte count")
    offsets = [
        int.from_bytes(offset_bytes[index : index + 2], "big")
        for index in range(0, len(offset_bytes), 2)
    ]
    if len(offsets) != 255:
        raise ValueError(f"Huffman offset count drift: {len(offsets)}")

    missing_contexts = [index for index, offset in enumerate(offsets) if offset == 0xFFFF]
    trees = [
        _parse_tree(tree_data, context, offset)
        for context, offset in enumerate(offsets)
        if offset != 0xFFFF
    ]
    if [tree["offset"] for tree in trees] != sorted(tree["offset"] for tree in trees):
        raise ValueError("defined Huffman offsets are not strictly ordered")
    if len({tree["offset"] for tree in trees}) != len(trees):
        raise ValueError("defined Huffman offsets are not unique")
    expected_start = 0
    for tree in trees:
        if tree["symbolStartOffset"] != expected_start:
            raise ValueError(
                f"Huffman tree {tree['contextSymbol']} does not pack after its predecessor"
            )
        expected_start = tree["endOffsetExclusive"]
    if expected_start != len(tree_data):
        raise ValueError("Huffman tree records do not cover the complete data payload")

    defined_contexts = {tree["contextSymbol"] for tree in trees}
    emitted_symbols = {
        entry["symbol"] for tree in trees for entry in tree["entries"]
    }
    if emitted_symbols != defined_contexts:
        raise ValueError("Huffman context graph is not closed over its defined trees")
    reachable = {254}
    queue = deque([254])
    tree_by_context = {tree["contextSymbol"]: tree for tree in trees}
    while queue:
        context = queue.popleft()
        for entry in tree_by_context[context]["entries"]:
            symbol = entry["symbol"]
            if symbol not in reachable:
                reachable.add(symbol)
                queue.append(symbol)
    if reachable != defined_contexts:
        raise ValueError("not every defined Huffman tree is reachable from initial context 254")

    offset_address = table[OFFSET_SYMBOL]
    tree_address = table[TREE_SYMBOL]
    if offset_address + len(offset_bytes) != tree_address:
        raise ValueError("Huffman offset and tree payloads are not adjacent")
    if rom[offset_address:tree_address] != offset_bytes:
        raise ValueError("Huffman offset source/ROM parity drift")
    if rom[tree_address : tree_address + len(tree_data)] != tree_data:
        raise ValueError("Huffman tree source/ROM parity drift")

    ascii_values = _parse_ascii_map(read_upstream_text(disasm / ASCII_TABLE_PATH))
    huffman_glyph_ids = sorted(symbol for symbol in emitted_symbols if 1 <= symbol <= 80)
    ascii_glyph_ids = sorted(set(ascii_values))
    combined_glyph_ids = sorted(set(huffman_glyph_ids) | set(ascii_glyph_ids))
    missing_glyph_ids = sorted(set(range(1, 81)) - set(combined_glyph_ids))
    code_lengths = Counter(
        entry["codeBitLength"] for tree in trees for entry in tree["entries"]
    )
    summary = {
        "offsetByteCount": len(offset_bytes),
        "offsetEntryCount": len(offsets),
        "definedTreeCount": len(trees),
        "missingTreeCount": len(missing_contexts),
        "treeDataByteCount": len(tree_data),
        "leafEntryCount": sum(tree["leafCount"] for tree in trees),
        "nonLeafNodeCount": sum(tree["nonLeafCount"] for tree in trees),
        "nodeBitCount": sum(tree["nodeBitCount"] for tree in trees),
        "nodeStorageByteCount": sum(tree["nodeByteCount"] for tree in trees),
        "symbolStorageByteCount": sum(tree["leafCount"] for tree in trees),
        "zeroPaddingBitCount": sum(tree["paddingBitCount"] for tree in trees),
        "minimumLeafCount": min(tree["leafCount"] for tree in trees),
        "maximumLeafCount": max(tree["leafCount"] for tree in trees),
        "minimumCodeBitLength": min(code_lengths),
        "maximumCodeBitLength": max(code_lengths),
        "emittedSymbolCount": len(emitted_symbols),
        "reachableContextCount": len(reachable),
        "sourceRomParityByteCount": len(offset_bytes) + len(tree_data),
    }
    for tree in trees:
        tree["symbolStartAddress"] = tree_address + tree["symbolStartOffset"]
        tree["nodeStartAddress"] = tree_address + tree["offset"]
        tree["endAddressExclusive"] = tree_address + tree["endOffsetExclusive"]

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
        "resources": {
            "offsets": {
                "sourcePath": OFFSET_PATH,
                "address": offset_address,
                "byteCount": len(offset_bytes),
                "sha256": hashlib.sha256(offset_bytes).hexdigest().upper(),
            },
            "trees": {
                "sourcePath": TREE_PATH,
                "address": tree_address,
                "byteCount": len(tree_data),
                "sha256": hashlib.sha256(tree_data).hexdigest().upper(),
            },
            "ownerPath": OWNER_PATH,
            "offsetFormatNoteSha256": hashlib.sha256(
                (disasm / OFFSET_NOTE_PATH).read_bytes()
            ).hexdigest().upper(),
            "treeFormatNoteSha256": hashlib.sha256(
                (disasm / TREE_NOTE_PATH).read_bytes()
            ).hexdigest().upper(),
        },
        "offsetTableFacts": {
            "sentinel": 65535,
            "actualEntryCount": len(offsets),
            "upstreamNoteClaimedEntryCount": 256,
            "upstreamNoteCountMatchesPayload": False,
            "definedOffsetsStrictlyIncreasing": True,
            "definedOffsetsUnique": True,
            "missingContextRanges": _missing_ranges(missing_contexts),
        },
        "packingFacts": {
            "firstRecordStartsAtOffset": trees[0]["symbolStartOffset"],
            "lastRecordEndsAtOffset": trees[-1]["endOffsetExclusive"],
            "gapByteCount": 0,
            "overlapByteCount": 0,
            "nonzeroPaddingTreeCount": 0,
            "recordsCoverEntireTreePayload": True,
        },
        "decoderFacts": {
            "initialPreviousSymbol": 254,
            "treeSelectedByPreviousSymbol": True,
            "offsetEntryByteCount": 2,
            "nodeOrder": "preorder",
            "nonLeafBit": 0,
            "leafBit": 1,
            "leftBranchInputBit": 0,
            "rightBranchInputBit": 1,
            "leafSymbolsStoredBeforeNodeOffsetInReverseOrder": True,
            "singleLeafTreeConsumesZeroInputBits": True,
        },
        "graphFacts": {
            "definedContextSymbols": sorted(defined_contexts),
            "emittedSymbols": sorted(emitted_symbols),
            "emittedSetEqualsDefinedContextSet": True,
            "allDefinedContextsReachableFrom254": True,
            "emittedMissingContextCount": 0,
        },
        "textGlyphReachability": {
            "glyphIdRangeStart": 1,
            "glyphIdRangeEnd": 80,
            "huffmanGlyphIds": huffman_glyph_ids,
            "asciiGlyphIds": ascii_glyph_ids,
            "combinedGlyphIds": combined_glyph_ids,
            "combinedMissingGlyphIds": missing_glyph_ids,
            "glyphs70And71ReachableThroughNormalTextInputs": False,
        },
        "codeLengthHistogram": {
            str(length): count for length, count in sorted(code_lengths.items())
        },
        "trees": trees,
        "runtimeQuestions": [
            "Do compressed text-bank bit lengths and control-code side effects reproduce all "
            "4,267 original strings when decoded through this static tree contract?",
            "Can any nonstandard direct symbol injection outside the ASCII and Huffman input "
            "paths intentionally render otherwise unreachable glyph IDs 70 and 71?",
        ],
    }


def verify_text_huffman_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_text_huffman_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="text Huffman static contract")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != output["romSha256"]
    ):
        raise ValueError("text Huffman provenance drift")
    for field in (
        "table",
        "summary",
        "resourceHashes",
        "offsetTableFacts",
        "packingFacts",
        "decoderFacts",
        "graphSummary",
        "textGlyphReachabilitySummary",
        "codeLengthHistogram",
        "selectedTreeFacts",
        "runtimeQuestions",
    ):
        actual: Any
        if field == "resourceHashes":
            actual = {
                "offsetSha256": output["resources"]["offsets"]["sha256"],
                "treeSha256": output["resources"]["trees"]["sha256"],
                "offsetFormatNoteSha256": output["resources"]["offsetFormatNoteSha256"],
                "treeFormatNoteSha256": output["resources"]["treeFormatNoteSha256"],
            }
        elif field == "selectedTreeFacts":
            selected = {0, 1, 54, 238, 254}
            actual = [
                {
                    key: tree[key]
                    for key in (
                        "contextSymbol",
                        "offset",
                        "symbolStartOffset",
                        "endOffsetExclusive",
                        "leafCount",
                        "nodeBitCount",
                        "minimumCodeBitLength",
                        "maximumCodeBitLength",
                    )
                }
                for tree in output["trees"]
                if tree["contextSymbol"] in selected
            ]
        elif field == "graphSummary":
            graph = output["graphFacts"]
            actual = {
                "definedContextCount": len(graph["definedContextSymbols"]),
                "emittedSymbolCount": len(graph["emittedSymbols"]),
                "definedContextSha256": hashlib.sha256(
                    bytes(graph["definedContextSymbols"])
                ).hexdigest().upper(),
                "emittedSymbolSha256": hashlib.sha256(
                    bytes(graph["emittedSymbols"])
                ).hexdigest().upper(),
                "emittedSetEqualsDefinedContextSet": graph[
                    "emittedSetEqualsDefinedContextSet"
                ],
                "allDefinedContextsReachableFrom254": graph[
                    "allDefinedContextsReachableFrom254"
                ],
                "emittedMissingContextCount": graph["emittedMissingContextCount"],
            }
        elif field == "textGlyphReachabilitySummary":
            reach = output["textGlyphReachability"]
            actual = {
                "huffmanGlyphCount": len(reach["huffmanGlyphIds"]),
                "asciiGlyphCount": len(reach["asciiGlyphIds"]),
                "combinedGlyphCount": len(reach["combinedGlyphIds"]),
                "huffmanGlyphSha256": hashlib.sha256(
                    bytes(reach["huffmanGlyphIds"])
                ).hexdigest().upper(),
                "asciiGlyphSha256": hashlib.sha256(
                    bytes(reach["asciiGlyphIds"])
                ).hexdigest().upper(),
                "combinedGlyphSha256": hashlib.sha256(
                    bytes(reach["combinedGlyphIds"])
                ).hexdigest().upper(),
                "combinedMissingGlyphIds": reach["combinedMissingGlyphIds"],
                "glyphs70And71ReachableThroughNormalTextInputs": reach[
                    "glyphs70And71ReachableThroughNormalTextInputs"
                ],
            }
        else:
            actual = output[field]
        if fixture[field] != actual:
            raise ValueError(f"text Huffman fixture drift: {field}")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if output["summary"] != manifest["summary"] or digest != manifest["outputSha256"]:
        raise ValueError("text Huffman canonical manifest drift")
    destination = output_path or repo_path("local/derived/text-huffman-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Trees": output["summary"]["definedTreeCount"],
        "Leaves": output["summary"]["leafEntryCount"],
        "ParityBytes": output["summary"]["sourceRomParityByteCount"],
        "Status": "PASS",
    }
