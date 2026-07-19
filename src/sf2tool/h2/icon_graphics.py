from __future__ import annotations

import hashlib
import json
import re
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.source_text import read_upstream_text

ID = "sf2-icon-graphics-static-v1"
MANIFEST = repo_path("manifests/extractions/icon-graphics-static.json")
SCHEMA = repo_path("schemas/icon-graphics-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/icon-graphics-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-icon-graphics-static-fixture.schema.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

ICON_SOURCE = Path("data/graphics/icons/entries.asm")
ICON_ROOT = Path("data/graphics/icons")
HIGHLIGHT_SOURCE = Path("code/common/tech/incbins/s03_incbins_iconhighlight.asm")
HIGHLIGHT_PATH = Path("data/graphics/tech/iconhighlighttiles.bin")
HIGHLIGHT_CONSUMER = Path("code/common/menus/loadhighlightableicon.asm")
MEMBER_CONSUMER = Path("code/common/menus/memberslistscreen.asm")
SHOP_CONSUMER = Path("code/common/menus/shopscreen.asm")
ICON_BYTES = 192

ENTRY_RE = re.compile(
    r'^\s*((?:Item|Other|Spell)Icon\d{3}):\s*incbin\s+"([^"]+)"', re.MULTILINE
)


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _equ(source: str, name: str) -> int:
    match = re.search(rf"^{re.escape(name)}:\s*equ\s+(\$?[0-9A-F]+)", source, re.MULTILINE)
    if match is None:
        raise ValueError(f"icon enum missing: {name}")
    token = match.group(1)
    return int(token[1:], 16) if token.startswith("$") else int(token)


def _expected_entries() -> list[tuple[str, str]]:
    entries = [
        (f"ItemIcon{index:03}", f"data/graphics/icons/item/icon{index:03}.bin")
        for index in range(127)
    ]
    entries.extend(
        (f"OtherIcon{index:03}", f"data/graphics/icons/other/icon{index:03}.bin")
        for index in range(3)
    )
    entries.extend(
        (f"SpellIcon{index:03}", f"data/graphics/icons/spell/icon{index:03}.bin")
        for index in range(16)
    )
    entries.extend(
        (f"OtherIcon{index:03}", f"data/graphics/icons/other/icon{index:03}.bin")
        for index in range(3, 6)
    )
    entries.extend(
        (f"SpellIcon{index:03}", f"data/graphics/icons/spell/icon{index:03}.bin")
        for index in range(19, 33)
    )
    return entries


def _require_fragments(source: str, owner: str, fragments: tuple[str, ...]) -> None:
    for fragment in fragments:
        if fragment not in source:
            raise ValueError(f"{owner} consumer drift: missing {fragment!r}")


def build_icon_graphics_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"icon-graphics H1 listing is missing: {listing_path}")
    addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))
    rom = rom_path.read_bytes()
    rom_hash = hashlib.sha256(rom).hexdigest().upper()
    if rom_hash != load_json(ROM_MANIFEST)["hashes"]["sha256"]:
        raise ValueError("icon-graphics input ROM identity drift")

    enums = read_upstream_text(disasm / "sf2enums.asm")
    constants = {
        name: _equ(enums, name)
        for name in (
            "ICON_NOTHING",
            "ICON_UNARMED",
            "ICON_SPELLS_START",
            "ICON_JEWEL_OF_LIGHT",
            "ICON_JEWEL_OF_EVIL",
            "ICON_CRACKS_OVERLAY",
            "ICON_PIXELS_LONGWORD_COUNTER",
            "ICON_TILE_BYTESIZE",
            "ICONS_OFFSET_CRACKS",
            "ITEM_NOTHING",
            "SPELL_HEALIN",
            "SPELL_FLAME",
            "SPELL_SNOW",
        )
    }
    expected_constants = {
        "ICON_NOTHING": 127,
        "ICON_UNARMED": 128,
        "ICON_SPELLS_START": 130,
        "ICON_JEWEL_OF_LIGHT": 146,
        "ICON_JEWEL_OF_EVIL": 147,
        "ICON_CRACKS_OVERLAY": 148,
        "ICON_PIXELS_LONGWORD_COUNTER": 47,
        "ICON_TILE_BYTESIZE": ICON_BYTES,
        "ICONS_OFFSET_CRACKS": 28416,
        "ITEM_NOTHING": 127,
        "SPELL_HEALIN": 16,
        "SPELL_FLAME": 17,
        "SPELL_SNOW": 18,
    }
    if constants != expected_constants:
        raise ValueError("icon enum value drift")

    entry_source = read_upstream_text(disasm / ICON_SOURCE)
    entries = ENTRY_RE.findall(entry_source)
    expected_entries = _expected_entries()
    if entries != expected_entries:
        raise ValueError("assembled icon order/path drift")

    available_paths = sorted(
        path.relative_to(disasm).as_posix() for path in (disasm / ICON_ROOT).rglob("*.bin")
    )
    if any((disasm / path).stat().st_size != ICON_BYTES for path in available_paths):
        raise ValueError("available icon payload size drift")
    assembled_paths = {path for _, path in entries}
    unassembled_paths = sorted(set(available_paths) - assembled_paths)
    expected_unassembled = [
        "data/graphics/icons/item/icon127.bin",
        "data/graphics/icons/spell/icon016.bin",
        "data/graphics/icons/spell/icon017.bin",
        "data/graphics/icons/spell/icon018.bin",
    ]
    if unassembled_paths != expected_unassembled:
        raise ValueError("unassembled icon payload boundary drift")

    rows = []
    cursor = addresses["ItemIcon000"]
    for storage_index, (symbol, path) in enumerate(entries):
        data = (disasm / path).read_bytes()
        address = addresses[symbol]
        if address != cursor:
            raise ValueError(f"icon storage is not contiguous at {symbol}")
        if rom[address : address + ICON_BYTES] != data:
            raise ValueError(f"icon payload ROM parity drift: {symbol}")
        rows.append(
            {
                "storageIndex": storage_index,
                "symbol": symbol,
                "sourcePath": path,
                "sourceAddress": address,
                "byteCount": len(data),
                "sha256": hashlib.sha256(data).hexdigest().upper(),
            }
        )
        cursor += ICON_BYTES

    pointer_address = addresses["p_Icons"]
    base_address = addresses["ItemIcon000"]
    if rom[pointer_address : pointer_address + 4] != base_address.to_bytes(4, "big"):
        raise ValueError("icon base pointer ROM parity drift")

    highlight_source = read_upstream_text(disasm / HIGHLIGHT_SOURCE)
    expected_highlight = f'incbin "{HIGHLIGHT_PATH.as_posix()}"'
    if expected_highlight not in highlight_source:
        raise ValueError("icon highlight resource path drift")
    highlight = (disasm / HIGHLIGHT_PATH).read_bytes()
    highlight_address = addresses["tiles_IconHighlight"]
    if len(highlight) != ICON_BYTES:
        raise ValueError("icon highlight mask size drift")
    if rom[highlight_address : highlight_address + ICON_BYTES] != highlight:
        raise ValueError("icon highlight mask ROM parity drift")

    highlight_consumer = read_upstream_text(disasm / HIGHLIGHT_CONSUMER)
    _require_fragments(
        highlight_consumer,
        "highlight icon",
        (
            "mulu.w  #ICON_TILE_BYTESIZE,d0",
            "movea.l (p_Icons).l,a0",
            "move.w  #ICON_PIXELS_LONGWORD_COUNTER,d1",
            "lea     tiles_IconHighlight(pc), a2",
            "move.l  d0,-ICON_TILE_BYTESIZE(a1)",
            "and.l   (a2)+,d0",
        ),
    )
    member_consumer = read_upstream_text(disasm / MEMBER_CONSUMER)
    shop_consumer = read_upstream_text(disasm / SHOP_CONSUMER)
    for owner, source in (("member icon", member_consumer), ("shop icon", shop_consumer)):
        _require_fragments(
            source,
            owner,
            (
                "movea.l (p_Icons).l,a0" if owner == "member icon" else "movea.l (p_Icons).l,a1",
                "moveq   #ICON_PIXELS_LONGWORD_COUNTER,d7",
                "ori.w   #$F000,-192",
                "ori.w   #$F,-158",
                "ori.w   #$F000,-36",
                "ori.w   #$F,-2",
            ),
        )

    storage_roles = [
        {
            "storageIndex": 127,
            "symbol": "OtherIcon000",
            "enumName": "ICON_NOTHING",
            "spellIndexCollision": None,
        },
        {
            "storageIndex": 128,
            "symbol": "OtherIcon001",
            "enumName": "ICON_UNARMED",
            "spellIndexCollision": None,
        },
        {
            "storageIndex": 129,
            "symbol": "OtherIcon002",
            "enumName": None,
            "spellIndexCollision": None,
        },
        {
            "storageIndex": 146,
            "symbol": "OtherIcon003",
            "enumName": "ICON_JEWEL_OF_LIGHT",
            "spellIndexCollision": 16,
        },
        {
            "storageIndex": 147,
            "symbol": "OtherIcon004",
            "enumName": "ICON_JEWEL_OF_EVIL",
            "spellIndexCollision": 17,
        },
        {
            "storageIndex": 148,
            "symbol": "OtherIcon005",
            "enumName": "ICON_CRACKS_OVERLAY",
            "spellIndexCollision": 18,
        },
    ]
    consumer_rules = {
        "directCopyBytes": ICON_BYTES,
        "tilesPerIcon": ICON_BYTES // 32,
        "cornerClean": [
            {"byteOffset": 0, "orMask": 0xF000},
            {"byteOffset": 34, "orMask": 0x000F},
            {"byteOffset": 156, "orMask": 0xF000},
            {"byteOffset": 190, "orMask": 0x000F},
        ],
        "highlightFrameCount": 2,
        "highlightOutputBytes": ICON_BYTES * 2,
        "highlightOperation": "source-bitwise-and-mask",
    }
    summary = {
        "availablePayloadCount": len(available_paths),
        "availablePayloadByteCount": len(available_paths) * ICON_BYTES,
        "assembledIconCount": len(rows),
        "assembledIconByteCount": len(rows) * ICON_BYTES,
        "unassembledPayloadCount": len(unassembled_paths),
        "iconByteCount": ICON_BYTES,
        "tilesPerIcon": ICON_BYTES // 32,
        "itemStorageCount": sum(row["symbol"].startswith("ItemIcon") for row in rows),
        "spellStorageCount": sum(row["symbol"].startswith("SpellIcon") for row in rows),
        "otherStorageCount": sum(row["symbol"].startswith("OtherIcon") for row in rows),
        "storageCollisionCount": sum(
            role["spellIndexCollision"] is not None for role in storage_roles
        ),
        "unnamedStorageCount": sum(role["enumName"] is None for role in storage_roles),
        "iconPayloadRomParityCount": len(rows),
        "basePointerRomParityCount": 1,
        "highlightMaskByteCount": len(highlight),
        "highlightMaskRomParityCount": 1,
        "directCopyOutputBytes": ICON_BYTES,
        "highlightOutputBytes": ICON_BYTES * 2,
        "cornerCleanWordCount": len(consumer_rules["cornerClean"]),
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "romSha256": rom_hash,
        "function": {
            "LoadHighlightableIcon": addresses["LoadHighlightableIcon"],
            "LoadIcon": addresses["LoadIcon"],
            "LoadIconPixelsInShopScreen": addresses["LoadIconPixelsInShopScreen"],
        },
        "table": {
            "p_Icons": pointer_address,
            "ItemIcon000": base_address,
            "tiles_IconHighlight": highlight_address,
        },
        "constants": constants,
        "summary": summary,
        "storageRoles": storage_roles,
        "consumerRules": consumer_rules,
        "unassembledPayloads": unassembled_paths,
        "highlightMaskSha256": hashlib.sha256(highlight).hexdigest().upper(),
        "icons": rows,
        "runtimeQuestions": [
            "Can menu state select unnamed storage index 129 or pass spell indices 16-18 "
            "to the generic icon loaders, exposing the physical slot collisions?",
            "Do base icons, forced corner pixels, highlight masks, palette selection, and DMA "
            "ordering reproduce the original rendered menu presentation?",
        ],
    }


def verify_icon_graphics_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_icon_graphics_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="icon-graphics contract")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != output["romSha256"]
    ):
        raise ValueError("icon-graphics provenance drift")
    for field in (
        "function",
        "table",
        "constants",
        "summary",
        "storageRoles",
        "consumerRules",
        "unassembledPayloads",
        "runtimeQuestions",
    ):
        if fixture[field] != output[field]:
            raise ValueError(f"icon-graphics {field} drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"] or output["summary"] != manifest["summary"]:
        raise ValueError("icon-graphics canonical output drift")
    destination = output_path or repo_path("local/derived/icon-graphics-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "AvailablePayloads": output["summary"]["availablePayloadCount"],
        "AssembledIcons": output["summary"]["assembledIconCount"],
        "UnassembledPayloads": output["summary"]["unassembledPayloadCount"],
        "Status": "PASS",
    }
