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

ID = "sf2-ui-graphics-decode-v1"
MANIFEST = repo_path("manifests/extractions/ui-graphics-decode.json")
SCHEMA = repo_path("schemas/ui-graphics-decode.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/ui-graphics-decode-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-ui-graphics-decode-fixture.schema.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

MENU_TABLE_SYMBOL = "pt_tiles_Menu"
MENU_TABLE_PATH = "code/common/menus/menutilespointertable.asm"
PACKED_MAIN_MENU_ENTRIES = [0x85010204, 0x80010203, 0x80010204]

RESOURCES = [
    {
        "symbol": "tiles_Base",
        "pointerSymbol": "p_tiles_Base",
        "sourcePath": "data/graphics/tech/basetiles.bin",
        "definitionPath": "code/common/tech/incbins/s17_incbins_basetiles.asm",
        "expectedDecodedBytes": 8192,
        "family": "base",
    },
    {
        "symbol": "tiles_ItemMenu",
        "pointerSymbol": "p_tiles_ItemMenu",
        "sourcePath": "data/graphics/tech/menus/itemmenutiles.bin",
        "definitionPath": "code/common/tech/incbins/s06_incbins_graphics.asm",
        "expectedDecodedBytes": 2304,
        "family": "diamond-menu",
    },
    {
        "symbol": "tiles_BattleFieldMenu",
        "pointerSymbol": "p_tiles_BattlefieldMenu",
        "sourcePath": "data/graphics/tech/menus/battlefieldmenutiles.bin",
        "definitionPath": "code/common/tech/incbins/s06_incbins_graphics.asm",
        "expectedDecodedBytes": 2304,
        "family": "diamond-menu",
    },
    {
        "symbol": "tiles_ChurchMenu",
        "pointerSymbol": "p_tiles_ChurchMenu",
        "sourcePath": "data/graphics/tech/menus/churchmenutiles.bin",
        "definitionPath": "code/common/tech/incbins/s06_incbins_graphics.asm",
        "expectedDecodedBytes": 2304,
        "family": "diamond-menu",
    },
    {
        "symbol": "tiles_ShopMenu",
        "pointerSymbol": "p_tiles_ShopMenu",
        "sourcePath": "data/graphics/tech/menus/shopmenutiles.bin",
        "definitionPath": "code/common/tech/incbins/s06_incbins_graphics.asm",
        "expectedDecodedBytes": 2304,
        "family": "diamond-menu",
    },
    {
        "symbol": "tiles_CaravanMenu",
        "pointerSymbol": "p_tiles_CaravanMenu",
        "sourcePath": "data/graphics/tech/menus/caravanmenutiles.bin",
        "definitionPath": "code/common/tech/incbins/s06_incbins_graphics.asm",
        "expectedDecodedBytes": 2304,
        "family": "diamond-menu",
    },
    {
        "symbol": "tiles_DepotMenu",
        "pointerSymbol": "p_tiles_DepotMenu",
        "sourcePath": "data/graphics/tech/menus/depotmenutiles.bin",
        "definitionPath": "code/common/tech/incbins/s06_incbins_graphics.asm",
        "expectedDecodedBytes": 2304,
        "family": "diamond-menu",
    },
    {
        "symbol": "tiles_YesNoPrompt",
        "pointerSymbol": "p_tiles_YesNoPrompt",
        "sourcePath": "data/graphics/tech/menus/yesnoprompttiles.bin",
        "definitionPath": "code/common/tech/incbins/s06_incbins_graphics.asm",
        "expectedDecodedBytes": 1152,
        "family": "yes-no",
    },
]


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _assert_consumer_shape(disasm: Path) -> None:
    base = read_upstream_text(disasm / "code/gameflow/start/basetiles.asm")
    for fragment in (
        "movea.l (p_tiles_Base).l,a0",
        "move.w  #4096,d0",
        "ApplyImmediateVramDmaOnCompressedTiles",
    ):
        if fragment not in base:
            raise ValueError(f"base-tile consumer drift: missing {fragment!r}")

    credits = read_upstream_text(disasm / "code/common/scripting/endcredits.asm")
    for fragment in (
        "GenerateEndingCreditsFont:",
        "movea.l (p_tiles_Base).l,a0",
        "LoadStackCompressedData",
    ):
        if fragment not in credits:
            raise ValueError(f"ending-credits base-tile consumer drift: missing {fragment!r}")

    diamond = read_upstream_text(disasm / "code/common/menus/diamondmenu.asm")
    for fragment in (
        "lea     pt_tiles_Menu(pc), a0",
        "bclr    #31,d0",
        "jsr     (LoadStackCompressedData).w",
        "bsr.w   LoadMainMenuIcon",
    ):
        if fragment not in diamond:
            raise ValueError(f"diamond-menu consumer drift: missing {fragment!r}")

    yes_no = read_upstream_text(disasm / "code/common/menus/yesnoprompt.asm")
    for fragment in (
        "movea.l (p_tiles_YesNoPrompt).l,a0",
        "jsr     (LoadStackCompressedData).w",
        "move.w  #$90,d0",
    ):
        if fragment not in yes_no:
            raise ValueError(f"yes/no consumer drift: missing {fragment!r}")


def build_ui_graphics_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"UI graphics H1 listing is missing: {listing_path}")
    addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))
    rom = rom_path.read_bytes()
    rom_hash = hashlib.sha256(rom).hexdigest().upper()
    if rom_hash != load_json(ROM_MANIFEST)["hashes"]["sha256"]:
        raise ValueError("UI graphics input ROM identity drift")

    _assert_consumer_shape(disasm)
    table_source = read_upstream_text(disasm / MENU_TABLE_PATH)
    for value in PACKED_MAIN_MENU_ENTRIES:
        if f"${value:X}" not in table_source:
            raise ValueError(f"packed main-menu entry drift: {value:08X}")

    rows = []
    table_addresses = {MENU_TABLE_SYMBOL: addresses[MENU_TABLE_SYMBOL]}
    for spec in RESOURCES:
        definition = read_upstream_text(disasm / spec["definitionPath"])
        if not re.search(
            rf'^\s*{re.escape(spec["symbol"])}:\s*incbin\s+"{re.escape(spec["sourcePath"])}"',
            definition,
            re.MULTILINE,
        ):
            raise ValueError(f"UI graphics resource definition drift: {spec['symbol']}")
        data = (disasm / spec["sourcePath"]).read_bytes()
        source_address = addresses[spec["symbol"]]
        pointer_address = addresses[spec["pointerSymbol"]]
        if rom[source_address : source_address + len(data)] != data:
            raise ValueError(f"UI graphics resource ROM parity drift: {spec['symbol']}")
        if rom[pointer_address : pointer_address + 4] != source_address.to_bytes(4, "big"):
            raise ValueError(f"UI graphics pointer ROM parity drift: {spec['pointerSymbol']}")
        decoded = decode_stack_compressed(data)
        if len(decoded.output) != spec["expectedDecodedBytes"]:
            raise ValueError(
                f"UI graphics output-size drift for {spec['symbol']}: "
                f"expected {spec['expectedDecodedBytes']}, got {len(decoded.output)}"
            )
        table_addresses[spec["symbol"]] = source_address
        table_addresses[spec["pointerSymbol"]] = pointer_address
        rows.append(
            {
                **spec,
                "sourceAddress": source_address,
                "pointerAddress": pointer_address,
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
            }
        )

    menu_pointer_symbols = [
        row["pointerSymbol"] for row in rows if row["family"] == "diamond-menu"
    ]
    menu_table_values = PACKED_MAIN_MENU_ENTRIES + [
        addresses[symbol] for symbol in menu_pointer_symbols
    ]
    table_address = addresses[MENU_TABLE_SYMBOL]
    table_bytes = b"".join(value.to_bytes(4, "big") for value in menu_table_values)
    if rom[table_address : table_address + len(table_bytes)] != table_bytes:
        raise ValueError("diamond-menu table ROM parity drift")

    summary = {
        "resourceCount": len(rows),
        "baseResourceCount": sum(row["family"] == "base" for row in rows),
        "diamondMenuResourceCount": sum(row["family"] == "diamond-menu" for row in rows),
        "yesNoResourceCount": sum(row["family"] == "yes-no" for row in rows),
        "menuTableEntryCount": len(menu_table_values),
        "packedMainMenuEntryCount": len(PACKED_MAIN_MENU_ENTRIES),
        "compressedMenuPointerCount": len(menu_pointer_symbols),
        "compressedByteCount": sum(row["compressedBytes"] for row in rows),
        "decodedByteCount": sum(row["decodedBytes"] for row in rows),
        "resourceRomParityCount": len(rows),
        "pointerRomParityCount": len(rows),
        "menuTableRomParityCount": len(menu_table_values),
        "commandGroupCount": sum(row["commandGroupCount"] for row in rows),
        "literalWordCount": sum(row["literalWordCount"] for row in rows),
        "copyCommandCount": sum(row["copyCommandCount"] for row in rows),
        "copiedWordCount": sum(row["copiedWordCount"] for row in rows),
        "minimumTrailingBits": min(row["trailingBits"] for row in rows),
        "maximumTrailingBits": max(row["trailingBits"] for row in rows),
        "maximumCopyOffsetWords": max(row["maximumCopyOffsetWords"] for row in rows),
        "maximumCopyLengthWords": max(row["maximumCopyLengthWords"] for row in rows),
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "romSha256": rom_hash,
        "function": {
            "loadStackAddress": addresses["LoadStackCompressedData"],
            "LoadBaseTiles": addresses["LoadBaseTiles"],
            "GenerateEndingCreditsFont": addresses["GenerateEndingCreditsFont"],
            "ExecuteDiamondMenu": addresses["ExecuteDiamondMenu"],
            "alt_YesNoPrompt": addresses["alt_YesNoPrompt"],
        },
        "table": table_addresses,
        "menuTable": {
            "values": menu_table_values,
            "packedMainMenuEntries": PACKED_MAIN_MENU_ENTRIES,
            "compressedPointerSymbols": menu_pointer_symbols,
        },
        "summary": summary,
        "resources": rows,
        "runtimeQuestions": [
            "Do the decoded base, diamond-menu, and yes/no tiles render with original palette, "
            "frame timing, and icon-selection behavior?"
        ],
    }


def verify_ui_graphics_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_ui_graphics_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="UI graphics decode contract")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != output["romSha256"]
    ):
        raise ValueError("UI graphics provenance drift")
    for field in ("function", "table", "menuTable", "summary", "runtimeQuestions"):
        if fixture[field] != output[field]:
            raise ValueError(f"UI graphics {field} drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"] or output["summary"] != manifest["summary"]:
        raise ValueError("UI graphics canonical output drift")
    destination = output_path or repo_path("local/derived/ui-graphics-decode.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Resources": output["summary"]["resourceCount"],
        "DecodedBytes": output["summary"]["decodedByteCount"],
        "MenuEntries": output["summary"]["menuTableEntryCount"],
        "Status": "PASS",
    }
