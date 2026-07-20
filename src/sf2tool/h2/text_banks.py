from __future__ import annotations

import hashlib
import json
import re
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.h2.text_huffman import _parse_tree
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.source_text import read_upstream_text

ID = "sf2-text-banks-static-v1"
MANIFEST = repo_path("manifests/extractions/text-banks-static.json")
SCHEMA = repo_path("schemas/text-banks-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/text-banks-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-text-banks-static-fixture.schema.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

TEXT_ROOT = Path("data/scripting/text")
ENTRIES_PATH = TEXT_ROOT / "entries.asm"
GAMESCRIPT_PATH = TEXT_ROOT / "gamescript.txt"
OFFSET_PATH = TEXT_ROOT / "huffmantreeoffsets.bin"
TREE_PATH = TEXT_ROOT / "huffmantrees.bin"
TOP_POINTER_PATH = Path("code/common/tech/pointers/s06_textbankspointer.asm")
DISPLAY_PATH = Path("code/common/scripting/text/textfunctions_1.asm")
BANK_SYMBOLS = tuple(f"TextBank{index:02d}" for index in range(17))
BANK_PATHS = tuple(TEXT_ROOT / f"textbank{index:02d}.bin" for index in range(17))
POINTER_TABLE_SYMBOL = "pt_TextBanks"
TOP_POINTER_SYMBOL = "p_pt_TextBanks"


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _code_maps(disasm: Path) -> dict[int, dict[str, int]]:
    offset_bytes = (disasm / OFFSET_PATH).read_bytes()
    tree_data = (disasm / TREE_PATH).read_bytes()
    offsets = [
        int.from_bytes(offset_bytes[index : index + 2], "big")
        for index in range(0, len(offset_bytes), 2)
    ]
    return {
        context: {
            entry["code"]: entry["symbol"]
            for entry in _parse_tree(tree_data, context, offset)["entries"]
        }
        for context, offset in enumerate(offsets)
        if offset != 0xFFFF
    }


def _decode_string(payload: bytes, code_maps: dict[int, dict[str, int]]) -> tuple[list[int], int]:
    position = 0
    previous = 254
    symbols: list[int] = []
    while True:
        codes = code_maps[previous]
        code = ""
        if code in codes:
            symbol = codes[code]
        else:
            while code not in codes:
                if position >= len(payload) * 8:
                    raise ValueError("compressed text ended before terminator symbol 254")
                bit = (payload[position // 8] >> (7 - position % 8)) & 1
                position += 1
                code += str(bit)
            symbol = codes[code]
        symbols.append(symbol)
        previous = symbol
        if symbol == 254:
            return symbols, position
        if len(symbols) > 4096:
            raise ValueError("compressed text exceeded static symbol safety bound")


def _assert_source_shape(disasm: Path) -> None:
    entries = read_upstream_text(disasm / ENTRIES_PATH)
    for symbol, path in zip(BANK_SYMBOLS, BANK_PATHS, strict=True):
        if re.search(
            rf'^{symbol}:\s+incbin\s+"{re.escape(path.as_posix())}"',
            entries,
            re.MULTILINE,
        ) is None:
            raise ValueError(f"text-bank incbin ownership drift: {symbol}")
    pointer_block = entries.split(f"{POINTER_TABLE_SYMBOL}:", 1)[1]
    targets = re.findall(r"dc\.l\s+(TextBank\d{2})", pointer_block)
    if targets != list(BANK_SYMBOLS):
        raise ValueError(f"text-bank pointer-table order drift: {targets}")
    top_pointer = read_upstream_text(disasm / TOP_POINTER_PATH)
    if re.search(
        rf"^{TOP_POINTER_SYMBOL}:\s*dc\.l\s+{POINTER_TABLE_SYMBOL}\b",
        top_pointer,
        re.MULTILINE,
    ) is None:
        raise ValueError("text-bank top pointer ownership drift")
    display = read_upstream_text(disasm / DISPLAY_PATH)
    for fragment in (
        "lsr.w   #6,d0",
        "andi.b  #$FC,d0",
        f"movea.l ({TOP_POINTER_SYMBOL}).l,a0",
        "andi.w  #BYTE_MASK,d0",
        "move.b  (a0),d7",
        "adda.l  d7,a0",
        "addq.l  #1,a0",
        "move.b  (a0)+,((COMPRESSED_STRING_LENGTH-$1000000)).w",
    ):
        if fragment not in display:
            raise ValueError(f"text-bank selection drift: missing {fragment!r}")


def build_text_banks_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"text-bank H1 listing is missing: {listing_path}")
    addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))
    table = {
        **{symbol: addresses[symbol] for symbol in BANK_SYMBOLS},
        POINTER_TABLE_SYMBOL: addresses[POINTER_TABLE_SYMBOL],
        TOP_POINTER_SYMBOL: addresses[TOP_POINTER_SYMBOL],
    }

    rom = rom_path.read_bytes()
    rom_hash = hashlib.sha256(rom).hexdigest().upper()
    if rom_hash != load_json(ROM_MANIFEST)["hashes"]["sha256"]:
        raise ValueError("text-bank input ROM identity drift")
    _assert_source_shape(disasm)
    code_maps = _code_maps(disasm)

    banks = []
    total_length_prefixes: Counter[int] = Counter()
    total_trailing_bits: Counter[int] = Counter()
    total_symbols: Counter[int] = Counter()
    global_index = 0
    for bank_index, (symbol, path) in enumerate(
        zip(BANK_SYMBOLS, BANK_PATHS, strict=True)
    ):
        data = (disasm / path).read_bytes()
        address = table[symbol]
        if rom[address : address + len(data)] != data:
            raise ValueError(f"text-bank source/ROM parity drift: {symbol}")
        cursor = 0
        strings = []
        bank_symbols = bytearray()
        bank_input_bits = 0
        while cursor < len(data):
            record_offset = cursor
            compressed_byte_count = data[cursor]
            cursor += 1
            end = cursor + compressed_byte_count
            if end > len(data):
                raise ValueError(f"text-bank record exceeds {symbol}")
            payload = data[cursor:end]
            decoded, input_bits = _decode_string(payload, code_maps)
            trailing_bits = len(payload) * 8 - input_bits
            if not 8 <= trailing_bits <= 15:
                raise ValueError(
                    f"text-bank trailing-bit boundary drift at string {global_index}: "
                    f"{trailing_bits}"
                )
            if decoded[-1] != 254 or decoded.count(254) != 1:
                raise ValueError(f"text-bank terminator drift at string {global_index}")
            strings.append(
                {
                    "globalIndex": global_index,
                    "bankStringIndex": len(strings),
                    "recordAddress": address + record_offset,
                    "recordOffset": record_offset,
                    "compressedByteCount": compressed_byte_count,
                    "inputBitsConsumed": input_bits,
                    "trailingBitCount": trailing_bits,
                    "decodedSymbolCount": len(decoded),
                    "compressedSha256": hashlib.sha256(payload).hexdigest().upper(),
                    "decodedSha256": hashlib.sha256(bytes(decoded)).hexdigest().upper(),
                    "symbols": decoded,
                }
            )
            total_length_prefixes[compressed_byte_count] += 1
            total_trailing_bits[trailing_bits] += 1
            total_symbols.update(decoded)
            bank_symbols.extend(decoded)
            bank_input_bits += input_bits
            cursor = end
            global_index += 1
        expected_string_count = 256 if bank_index < 16 else 171
        if len(strings) != expected_string_count:
            raise ValueError(
                f"text-bank string-count drift for {symbol}: {len(strings)}"
            )
        banks.append(
            {
                "bankIndex": bank_index,
                "symbol": symbol,
                "sourcePath": path.as_posix(),
                "address": address,
                "byteCount": len(data),
                "stringCount": len(strings),
                "payloadByteCount": sum(row["compressedByteCount"] for row in strings),
                "decodedSymbolCount": len(bank_symbols),
                "inputBitCount": bank_input_bits,
                "sourceSha256": hashlib.sha256(data).hexdigest().upper(),
                "decodedSymbolsSha256": hashlib.sha256(bank_symbols).hexdigest().upper(),
                "strings": strings,
            }
        )

    if global_index != 4267:
        raise ValueError(f"text-bank total string-count drift: {global_index}")
    if set(total_symbols) != set(code_maps):
        raise ValueError("decoded text does not exercise every defined Huffman context")
    if total_symbols[254] != global_index:
        raise ValueError("decoded text terminator count does not equal string count")

    pointer_address = table[POINTER_TABLE_SYMBOL]
    expected_pointer_bytes = b"".join(
        table[symbol].to_bytes(4, "big") for symbol in BANK_SYMBOLS
    )
    if (
        rom[pointer_address : pointer_address + len(expected_pointer_bytes)]
        != expected_pointer_bytes
    ):
        raise ValueError("text-bank pointer-table ROM parity drift")
    top_pointer_address = table[TOP_POINTER_SYMBOL]
    top_pointer_bytes = pointer_address.to_bytes(4, "big")
    if rom[top_pointer_address : top_pointer_address + 4] != top_pointer_bytes:
        raise ValueError("text-bank top pointer ROM parity drift")
    bank_end = banks[-1]["address"] + banks[-1]["byteCount"]
    alignment_byte_count = pointer_address - bank_end
    if alignment_byte_count != 1:
        raise ValueError(f"text-bank pointer alignment drift: {alignment_byte_count}")

    gamescript = (disasm / GAMESCRIPT_PATH).read_bytes()
    script_ids = [
        int(match, 16)
        for match in re.findall(rb"^([0-9A-F]{4})=", gamescript, re.MULTILINE)
    ]
    if script_ids != list(range(global_index)):
        raise ValueError("gamescript text IDs do not exactly cover decoded string indices")

    control_histogram = {
        str(symbol): total_symbols[symbol] for symbol in range(238, 255)
    }
    summary = {
        "bankCount": len(banks),
        "fullBankCount": sum(bank["stringCount"] == 256 for bank in banks),
        "partialBankCount": sum(bank["stringCount"] != 256 for bank in banks),
        "stringCount": global_index,
        "bankByteCount": sum(bank["byteCount"] for bank in banks),
        "lengthPrefixByteCount": global_index,
        "compressedPayloadByteCount": sum(bank["payloadByteCount"] for bank in banks),
        "decodedSymbolCount": sum(total_symbols.values()),
        "decodedNonTerminatorSymbolCount": sum(total_symbols.values()) - global_index,
        "distinctDecodedSymbolCount": len(total_symbols),
        "controlSymbolCountIncludingTerminators": sum(
            total_symbols[symbol] for symbol in range(238, 255)
        ),
        "controlSymbolCountExcludingTerminators": sum(
            total_symbols[symbol] for symbol in range(238, 254)
        ),
        "terminatorCount": total_symbols[254],
        "minimumCompressedByteCount": min(total_length_prefixes),
        "maximumCompressedByteCount": max(total_length_prefixes),
        "minimumDecodedSymbolCount": min(
            row["decodedSymbolCount"] for bank in banks for row in bank["strings"]
        ),
        "maximumDecodedSymbolCount": max(
            row["decodedSymbolCount"] for bank in banks for row in bank["strings"]
        ),
        "minimumTrailingBitCount": min(total_trailing_bits),
        "maximumTrailingBitCount": max(total_trailing_bits),
        "pointerTableByteCount": len(expected_pointer_bytes),
        "alignmentByteCount": alignment_byte_count,
        "topPointerByteCount": len(top_pointer_bytes),
        "sourceRomParityByteCount": sum(bank["byteCount"] for bank in banks)
        + alignment_byte_count
        + len(expected_pointer_bytes)
        + len(top_pointer_bytes),
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
        "pointerFacts": {
            "pointerTableSymbol": POINTER_TABLE_SYMBOL,
            "pointerTableAddress": pointer_address,
            "pointerCount": len(BANK_SYMBOLS),
            "pointerTableSha256": hashlib.sha256(expected_pointer_bytes).hexdigest().upper(),
            "topPointerSymbol": TOP_POINTER_SYMBOL,
            "topPointerAddress": top_pointer_address,
            "topPointerSha256": hashlib.sha256(top_pointer_bytes).hexdigest().upper(),
            "bankSelectionFormula": "bank=(stringIndex>>8); withinBank=stringIndex&255",
            "recordSelectionFormula": "advance 1+lengthByte for each preceding record",
        },
        "gamescriptFacts": {
            "sourcePath": GAMESCRIPT_PATH.as_posix(),
            "byteCount": len(gamescript),
            "lineIdCount": len(script_ids),
            "firstLineId": script_ids[0],
            "lastLineId": script_ids[-1],
            "idsAreContiguous": True,
            "sha256": hashlib.sha256(gamescript).hexdigest().upper(),
        },
        "lengthPrefixHistogram": {
            str(length): count for length, count in sorted(total_length_prefixes.items())
        },
        "trailingBitHistogram": {
            str(bits): count for bits, count in sorted(total_trailing_bits.items())
        },
        "controlSymbolHistogram": control_histogram,
        "decodedSymbolHistogram": {
            str(symbol): total_symbols[symbol] for symbol in sorted(total_symbols)
        },
        "banks": banks,
        "copyrightBoundary": {
            "trackedFixtureContainsPlaintext": False,
            "generatedDecodedSymbolsStayUnderIgnoredLocalRoot": True,
            "gamescriptPlaintextIsHashedButNotCopied": True,
        },
        "runtimeQuestions": [
            "Do all control-code side effects, parameters, waits, window state, and inserted "
            "names/items/spells reproduce the original presentation for the decoded corpus?",
            "Can the statically unreferenced color control symbol 253 or any nonstandard direct "
            "text input be reached outside the 4,267 original bank records?",
        ],
    }


def verify_text_banks_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_text_banks_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="text banks static contract")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != output["romSha256"]
    ):
        raise ValueError("text-bank provenance drift")
    for field in (
        "table",
        "summary",
        "pointerFacts",
        "gamescriptFacts",
        "histogramFacts",
        "bankFacts",
        "copyrightBoundary",
        "runtimeQuestions",
    ):
        actual: Any
        if field == "histogramFacts":
            actual = {
                "lengthPrefixHistogramSha256": hashlib.sha256(
                    _canonical_bytes(output["lengthPrefixHistogram"])
                ).hexdigest().upper(),
                "trailingBitHistogram": output["trailingBitHistogram"],
                "controlSymbolHistogram": output["controlSymbolHistogram"],
                "decodedSymbolHistogramSha256": hashlib.sha256(
                    _canonical_bytes(output["decodedSymbolHistogram"])
                ).hexdigest().upper(),
            }
        elif field == "bankFacts":
            actual = [
                {
                    key: bank[key]
                    for key in (
                        "bankIndex",
                        "symbol",
                        "address",
                        "byteCount",
                        "stringCount",
                        "payloadByteCount",
                        "decodedSymbolCount",
                        "inputBitCount",
                        "sourceSha256",
                        "decodedSymbolsSha256",
                    )
                }
                for bank in output["banks"]
            ]
        else:
            actual = output[field]
        if fixture[field] != actual:
            raise ValueError(f"text-bank fixture drift: {field}")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if output["summary"] != manifest["summary"] or digest != manifest["outputSha256"]:
        raise ValueError("text-bank canonical manifest drift")
    destination = output_path or repo_path("local/derived/text-banks-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Banks": output["summary"]["bankCount"],
        "Strings": output["summary"]["stringCount"],
        "Symbols": output["summary"]["decodedSymbolCount"],
        "ParityBytes": output["summary"]["sourceRomParityByteCount"],
        "Status": "PASS",
    }
