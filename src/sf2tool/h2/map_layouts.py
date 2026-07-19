from __future__ import annotations

import hashlib
import json
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.h2.map_content import _entry_pointer_expressions
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.source_text import read_upstream_text

ID = "sf2-map-layout-decode-v1"
ENTRY_ROOT = Path("data/maps/entries")
ENTRIES_PATH = Path("data/maps/entries.asm")
MAPLOAD_PATH = Path("code/common/maps/mapload.asm")
MANIFEST = repo_path("manifests/extractions/map-layout-decode.json")
SCHEMA = repo_path("schemas/map-layout-decode.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/map-layout-decode-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-map-layout-decode-fixture.schema.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

INITIAL_BLOCK_WORDS = (
    (0x100,) * 9
    + (0x32E, 0x32F, 0xB2E, 0x33E, 0x33F, 0xB3E, 0x34E, 0x34F, 0xB4E)
    + (0x32C, 0x32D, 0xB2C, 0x33C, 0x33D, 0xB3C, 0x34E, 0x34F, 0xB4E)
)


class _Bits:
    def __init__(self, data: bytes) -> None:
        self.data = data
        self.position = 0

    def bit(self) -> int:
        if self.position >= len(self.data) * 8:
            raise ValueError("compressed map bitstream ended early")
        value = (self.data[self.position // 8] >> (7 - self.position % 8)) & 1
        self.position += 1
        return value

    def bits(self, count: int) -> int:
        value = 0
        for _ in range(count):
            value = (value << 1) | self.bit()
        return value


def _count(counts: dict[str, int], command: str, amount: int = 1) -> None:
    counts[command] = counts.get(command, 0) + amount


def _words_bytes(words: list[int]) -> bytes:
    return b"".join((word & 0xFFFF).to_bytes(2, "big") for word in words)


def decode_map_blocks(data: bytes) -> tuple[list[int], int, dict[str, int]]:
    reader = _Bits(data)
    command_total = reader.bits(14)
    output = list(INITIAL_BLOCK_WORDS)
    history: dict[int, int] = {}
    commands: dict[str, int] = {}
    for _ in range(command_total):
        if reader.bit() == 0:
            if reader.bit() == 0:
                value = output[-1]
                command = "repeat"
            else:
                value = (output[-1] - 1 if output[-1] & 0x800 else output[-1] + 1) & 0xFFFF
                command = "adjacent"
        elif reader.bit() == 0:
            if reader.bit() == 0:
                previous = output[-1]
                key = ((previous & 0x3FF) << 1) | (previous & 0x800)
                command = "rightHistory"
            else:
                previous = output[-3]
                key = ((previous & 0x3FF) << 1) | (previous & 0x800) | 0x1000
                command = "bottomHistory"
            value = history.get(key, 0)
        else:
            if reader.bit() == 0:
                flags = output[-1] & 0x9800
                flag_mode = "sameFlags"
            else:
                flags = (reader.bit() << 15) | (reader.bit() << 12) | (reader.bit() << 11)
                flag_mode = "newFlags"
            if reader.bit() == 0:
                delta = reader.bits(5)
                if reader.bit():
                    delta = -delta
                value = ((output[-1] + delta) & 0x7FF) | flags
                command = f"relative{flag_mode[0].upper()}{flag_mode[1:]}"
            else:
                tile = reader.bits(9)
                if tile >= 384:
                    tile = tile * 2 + reader.bit() - 384
                value = (tile + 0x100) | flags
                command = f"absolute{flag_mode[0].upper()}{flag_mode[1:]}"
            previous = output[-1]
            history[((previous & 0x3FF) << 1) | (previous & 0x800)] = value
            previous = output[-3]
            history[((previous & 0x3FF) << 1) | (previous & 0x800) | 0x1000] = value
        output.append(value)
        _count(commands, command)
    if len(output) % 9:
        raise ValueError("decoded map block words do not form complete 3x3 blocks")
    return output, reader.position, commands


def _layout_flags(reader: _Bits) -> int:
    if reader.bit() == 0:
        return 0xC000 if reader.bit() else 0
    if reader.bit() == 0:
        return 0x8000 if reader.bit() else 0x4000
    return reader.bits(6) << 10


def _save_history(history: dict[int, list[int]], key: int, value: int) -> None:
    values = history.setdefault(key, [])
    if value in values:
        values.remove(value)
    values.insert(0, value)
    del values[4:]


def _choose_history(reader: _Bits, values: list[int]) -> int:
    for value in values[:-1]:
        if reader.bit():
            return value
    return values[-1]


def decode_map_layout(
    data: bytes, block_count: int
) -> tuple[list[int], int, dict[str, int], int]:
    reader = _Bits(data)
    output: list[int] = []
    history: dict[int, list[int]] = {}
    next_block = 2
    commands: dict[str, int] = {}
    while len(output) < 4096:
        save_history = True
        copy_count = 1
        if reader.bit() == 0:
            if reader.bit() == 0:
                next_block += 1
                value = _layout_flags(reader) | next_block
                command = "nextBlock"
            else:
                zero_count = 0
                while reader.bit() == 0:
                    zero_count += 1
                copy_count = (1 << zero_count) + (
                    reader.bits(zero_count) if zero_count else 0
                )
                command = "copyLeft" if reader.bit() else "copyUpper"
                save_history = False
        else:
            left = output[-1] if output else 0
            values = history.get(left & 0x3FF, [])
            if values and reader.bit() == 0:
                value = _choose_history(reader, values)
                command = "leftHistory"
            else:
                upper = output[-64] if len(output) >= 64 else 0
                values = history.get(0x400 | (upper & 0x3FF), [])
                if values and reader.bit() == 0:
                    value = _choose_history(reader, values)
                    command = "upperHistory"
                else:
                    index = reader.bits(next_block.bit_length())
                    value = _layout_flags(reader) | index
                    command = "literal"
        for _ in range(copy_count):
            if len(output) >= 4096:
                break
            if command == "copyLeft":
                value = output[-1]
            elif command == "copyUpper":
                value = output[-64]
            if save_history:
                left = output[-1] if output else 0
                upper = output[-64] if len(output) >= 64 else 0
                _save_history(history, left & 0x3FF, value)
                _save_history(history, 0x400 | (upper & 0x3FF), value)
            output.append(value)
            _count(commands, command)
    maximum_index = max(word & 0x3FF for word in output)
    if maximum_index >= block_count or next_block >= block_count:
        raise ValueError("decoded layout references outside its decoded blockset")
    return output, reader.position, commands, next_block


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def build_map_layout_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))
    rom = rom_path.read_bytes()
    rom_hash = hashlib.sha256(rom).hexdigest().upper()
    if rom_hash != load_json(ROM_MANIFEST)["hashes"]["sha256"]:
        raise ValueError("map layout input ROM identity drift")

    payloads = []
    symbol_owner: dict[str, int] = {}
    block_commands: dict[str, int] = {}
    layout_commands: dict[str, int] = {}
    for block_path in sorted((disasm / ENTRY_ROOT).rglob("0-blocks.bin")):
        map_index = int(block_path.parent.name[3:])
        layout_path = block_path.with_name("1-layout.bin")
        block_data = block_path.read_bytes()
        layout_data = layout_path.read_bytes()
        block_symbol = f"Map{map_index:02d}s0_Blocks"
        layout_symbol = f"Map{map_index:02d}s1_Layout"
        for symbol, data in ((block_symbol, block_data), (layout_symbol, layout_data)):
            address = addresses[symbol]
            if rom[address : address + len(data)] != data:
                raise ValueError(f"map layout payload ROM parity drift: {symbol}")
            symbol_owner[symbol] = map_index
        block_words, block_bits, block_counts = decode_map_blocks(block_data)
        block_count = len(block_words) // 9
        layout_words, layout_bits, layout_counts, sequential_max = decode_map_layout(
            layout_data, block_count
        )
        for name, value in block_counts.items():
            _count(block_commands, name, value)
        for name, value in layout_counts.items():
            _count(layout_commands, name, value)
        payloads.append(
            {
                "map": map_index,
                "blockAddress": addresses[block_symbol],
                "layoutAddress": addresses[layout_symbol],
                "blockInputBytes": len(block_data),
                "layoutInputBytes": len(layout_data),
                "blockCount": block_count,
                "blockBitsConsumed": block_bits,
                "blockPaddingBits": len(block_data) * 8 - block_bits,
                "layoutBitsConsumed": layout_bits,
                "layoutPaddingBits": len(layout_data) * 8 - layout_bits,
                "layoutWordCount": len(layout_words),
                "layoutMaxBlockIndex": max(word & 0x3FF for word in layout_words),
                "layoutUniqueBlockCount": len({word & 0x3FF for word in layout_words}),
                "sequentialBlockMax": sequential_max,
                "blockCommands": block_counts,
                "layoutCommands": layout_counts,
                "blockDecodedSha256": hashlib.sha256(_words_bytes(block_words)).hexdigest().upper(),
                "layoutDecodedSha256": hashlib.sha256(_words_bytes(layout_words))
                .hexdigest()
                .upper(),
            }
        )

    entries = read_upstream_text(disasm / ENTRIES_PATH)
    references = []
    for map_index in range(79):
        expressions = _entry_pointer_expressions(entries, map_index)
        references.append(
            {
                "map": map_index,
                "blockOwnerMap": symbol_owner[expressions[0]],
                "layoutOwnerMap": symbol_owner[expressions[1]],
            }
        )
    aliases = [row for row in references if row["map"] != row["blockOwnerMap"]]
    if aliases != [
        {"map": 24, "blockOwnerMap": 23, "layoutOwnerMap": 23},
        {"map": 46, "blockOwnerMap": 7, "layoutOwnerMap": 7},
    ]:
        raise ValueError("map block/layout alias boundary drift")

    mapload = read_upstream_text(disasm / MAPLOAD_PATH)
    for fragment in (
        "LoadMapLayoutData:",
        "lea     $2000(a1),a6",
        "LoadMapBlocks:",
        "first 14 bits = number of commands",
        "cmpi.w  #MAP_TILE_SIZE,d3",
    ):
        if fragment not in mapload:
            raise ValueError(f"map decoder source-shape drift: {fragment!r}")
    summary = {
        "payloadPairCount": len(payloads),
        "mapReferenceCount": len(references),
        "aliasReferenceCount": len(aliases),
        "decodedBlockCount": sum(row["blockCount"] for row in payloads),
        "decodedBlockWordCount": sum(row["blockCount"] * 9 for row in payloads),
        "decodedLayoutWordCount": sum(row["layoutWordCount"] for row in payloads),
        "decodedLayoutByteCount": sum(row["layoutWordCount"] * 2 for row in payloads),
        "minimumBlockCount": min(row["blockCount"] for row in payloads),
        "maximumBlockCount": max(row["blockCount"] for row in payloads),
        "maximumLayoutBlockIndex": max(row["layoutMaxBlockIndex"] for row in payloads),
        "blockPayloadRomParityCount": len(payloads),
        "layoutPayloadRomParityCount": len(payloads),
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "romSha256": rom_hash,
        "function": {
            "LoadMapLayoutData": addresses["LoadMapLayoutData"],
            "LoadMapBlocks": addresses["LoadMapBlocks"],
        },
        "summary": summary,
        "aliases": aliases,
        "blockCommands": block_commands,
        "layoutCommands": layout_commands,
        "payloads": payloads,
        "runtimeQuestions": [
            "decoded-map-rendered-vdp-parity",
            "map-transition-copy-event-persistence",
        ],
    }


def verify_map_layout_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_map_layout_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="map layout decode contract")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != output["romSha256"]
    ):
        raise ValueError("map layout decode provenance drift")
    for field in (
        "function",
        "summary",
        "aliases",
        "blockCommands",
        "layoutCommands",
        "runtimeQuestions",
    ):
        if fixture[field] != output[field]:
            raise ValueError(f"map layout decode {field} drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"] or output["summary"] != manifest["summary"]:
        raise ValueError("map layout decode canonical output drift")
    destination = output_path or repo_path("local/derived/map-layout-decode.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "PayloadPairs": output["summary"]["payloadPairCount"],
        "DecodedBlocks": output["summary"]["decodedBlockCount"],
        "DecodedLayoutWords": output["summary"]["decodedLayoutWordCount"],
        "Status": "PASS",
    }
