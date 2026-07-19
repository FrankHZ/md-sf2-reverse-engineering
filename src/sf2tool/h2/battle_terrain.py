from __future__ import annotations

import hashlib
import json
import re
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.compression import decode_stack_compressed
from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.source_text import read_upstream_text

ID = "sf2-battle-terrain-decode-v1"
ENTRIES_PATH = Path("data/battles/terrainentries.asm")
LOADER_PATH = Path("code/gameflow/battle/battleloop/loadbattleterraindata.asm")
MANIFEST = repo_path("manifests/extractions/battle-terrain-decode.json")
SCHEMA = repo_path("schemas/battle-terrain-decode.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/battle-terrain-decode-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-battle-terrain-decode-fixture.schema.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

TERRAIN_BYTES = 48 * 48
VALID_TERRAIN_VALUES = frozenset((*range(9), 0xFF))


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _parse_entries(source: str) -> tuple[list[str], list[tuple[str, str]]]:
    definitions_start = source.find("BattleTerrain00:")
    if definitions_start < 0:
        raise ValueError("battle terrain source has no payload definitions")
    references = re.findall(r"\bdc\.l\s+(BattleTerrain\d{2})\b", source[:definitions_start])
    definitions = re.findall(
        r'^\s*(BattleTerrain\d{2}):\s*incbin\s+"([^"]+)"',
        source[definitions_start:],
        re.MULTILINE | re.IGNORECASE,
    )
    if len(references) != 45 or len(definitions) != 43:
        raise ValueError(
            f"battle terrain source-shape drift: {len(references)} pointers, "
            f"{len(definitions)} payloads"
        )
    if len({symbol for symbol, _ in definitions}) != len(definitions):
        raise ValueError("battle terrain payload symbol is duplicated")
    return references, definitions


def build_battle_terrain_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"battle terrain H1 listing is missing: {listing_path}")
    addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))
    rom = rom_path.read_bytes()
    rom_hash = hashlib.sha256(rom).hexdigest().upper()
    if rom_hash != load_json(ROM_MANIFEST)["hashes"]["sha256"]:
        raise ValueError("battle terrain input ROM identity drift")

    source = read_upstream_text(disasm / ENTRIES_PATH)
    references, definitions = _parse_entries(source)
    definition_paths = dict(definitions)
    missing = sorted(set(references) - set(definition_paths))
    if missing:
        raise ValueError(f"battle terrain pointer targets are undefined: {missing}")

    table_address = addresses["pt_BattleTerrainData"]
    encoded_table = b"".join(addresses[symbol].to_bytes(4, "big") for symbol in references)
    if rom[table_address : table_address + len(encoded_table)] != encoded_table:
        raise ValueError("battle terrain pointer-table ROM parity drift")

    value_counts: Counter[int] = Counter()
    history_index_counts = [0] * 16
    payloads: list[dict[str, Any]] = []
    for symbol, relative_path in definitions:
        path = disasm / Path(relative_path.replace("\\", "/"))
        compressed = path.read_bytes()
        source_address = addresses[symbol]
        if rom[source_address : source_address + len(compressed)] != compressed:
            raise ValueError(f"battle terrain compressed ROM parity drift: {symbol}")
        decoded = decode_stack_compressed(compressed, expected_output_bytes=TERRAIN_BYTES)
        invalid_values = sorted(set(decoded.output) - VALID_TERRAIN_VALUES)
        if invalid_values:
            raise ValueError(f"battle terrain {symbol} has invalid values: {invalid_values}")
        counts = Counter(decoded.output)
        value_counts.update(counts)
        for index, count in enumerate(decoded.history_index_counts):
            history_index_counts[index] += count
        payloads.append(
            {
                "battle": int(symbol[-2:]),
                "symbol": symbol,
                "sourcePath": relative_path.replace("\\", "/"),
                "sourceAddress": source_address,
                "compressedBytes": len(compressed),
                "inputBitsConsumed": decoded.input_bits_consumed,
                "trailingBits": len(compressed) * 8 - decoded.input_bits_consumed,
                "decodedBytes": len(decoded.output),
                "decodedSha256": hashlib.sha256(decoded.output).hexdigest().upper(),
                "commandGroupCount": decoded.command_group_count,
                "literalWordCount": decoded.literal_word_count,
                "copyCommandCount": decoded.copy_command_count,
                "copiedWordCount": decoded.copied_word_count,
                "maximumCopyOffsetWords": decoded.maximum_copy_offset_words,
                "maximumCopyLengthWords": decoded.maximum_copy_length_words,
                "valueCounts": {str(value): counts[value] for value in sorted(counts)},
            }
        )

    owner_by_symbol = {symbol: int(symbol[-2:]) for symbol, _ in definitions}
    pointer_owners = [owner_by_symbol[symbol] for symbol in references]
    aliases = [
        {"battle": battle, "terrainOwnerBattle": owner}
        for battle, owner in enumerate(pointer_owners)
        if battle != owner
    ]
    if aliases != [
        {"battle": 4, "terrainOwnerBattle": 3},
        {"battle": 32, "terrainOwnerBattle": 27},
    ]:
        raise ValueError(f"battle terrain alias drift: {aliases}")

    loader = read_upstream_text(disasm / LOADER_PATH)
    for fragment in (
        "LoadBattleTerrainData:",
        "lsl.l   #2,d1",
        "movea.l (a0,d1.w),a0",
        "lea     (BATTLE_TERRAIN_ARRAY).l,a1",
        "jsr     (LoadStackCompressedData).w",
    ):
        if fragment not in loader:
            raise ValueError(f"battle terrain loader source-shape drift: {fragment!r}")

    summary = {
        "battlePointerCount": len(references),
        "uniquePayloadCount": len(payloads),
        "aliasPointerCount": len(aliases),
        "compressedByteCount": sum(row["compressedBytes"] for row in payloads),
        "decodedByteCount": sum(row["decodedBytes"] for row in payloads),
        "decodedGridWidth": 48,
        "decodedGridHeight": 48,
        "commandGroupCount": sum(row["commandGroupCount"] for row in payloads),
        "literalWordCount": sum(row["literalWordCount"] for row in payloads),
        "copyCommandCount": sum(row["copyCommandCount"] for row in payloads),
        "copiedWordCount": sum(row["copiedWordCount"] for row in payloads),
        "maximumCopyOffsetWords": max(row["maximumCopyOffsetWords"] for row in payloads),
        "maximumCopyLengthWords": max(row["maximumCopyLengthWords"] for row in payloads),
        "minimumTrailingBits": min(row["trailingBits"] for row in payloads),
        "maximumTrailingBits": max(row["trailingBits"] for row in payloads),
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
            "loadTerrainAddress": addresses["LoadBattleTerrainData"],
        },
        "table": {"terrainTableAddress": table_address},
        "summary": summary,
        "aliases": aliases,
        "aggregateValueCounts": {
            str(value): value_counts[value] for value in sorted(value_counts)
        },
        "historyIndexCounts": history_index_counts,
        "payloads": payloads,
        "runtimeQuestions": [],
    }


def verify_battle_terrain_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_battle_terrain_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="battle terrain decode contract")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != output["romSha256"]
    ):
        raise ValueError("battle terrain provenance drift")
    for field in (
        "function",
        "table",
        "summary",
        "aliases",
        "aggregateValueCounts",
        "runtimeQuestions",
    ):
        if fixture[field] != output[field]:
            raise ValueError(f"battle terrain {field} drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"] or output["summary"] != manifest["summary"]:
        raise ValueError("battle terrain canonical output drift")
    destination = output_path or repo_path("local/derived/battle-terrain-decode.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "BattlePointers": output["summary"]["battlePointerCount"],
        "UniquePayloads": output["summary"]["uniquePayloadCount"],
        "DecodedBytes": output["summary"]["decodedByteCount"],
        "Status": "PASS",
    }
