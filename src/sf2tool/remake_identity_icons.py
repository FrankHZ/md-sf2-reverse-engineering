"""Bounded private spell/item menu art, using the existing verified icon renderer."""

import base64
import hashlib
import json
import re
from pathlib import Path

from sf2tool.h2.icon_graphics import _equ, _expected_entries
from sf2tool.paths import repo_path
from sf2tool.source_text import read_upstream_text
from sf2tool.texture_extract import (
    BASE_PALETTE_PATH,
    ICON_BYTES,
    _render_tile_pool_sheet,
    _verified_source,
    md_palette_color,
)

# Current admitted party equipment, carry/Use and spell capabilities. These are source
# IDs, not row positions or translated labels. Additional gameplay admission owns expansion.
ITEMS = (
    "MEDICAL_HERB",
    "HEALING_SEED",
    "ANTIDOTE",
    "ANGEL_WING",
    "SHORT_AXE",
    "WOODEN_STICK",
    "SHORT_SWORD",
    "WOODEN_SWORD",
    "SHORT_SPEAR",
    "BRONZE_LANCE",
    "WOODEN_ROD",
    "SHORT_ROD",
    "SHORT_KNIFE",
    "MITHRIL",
    "NOTHING",
)
SPELLS = ("HEAL", "DETOX", "DISPEL", "EGRESS", "BLAZE", "BLAST")


def identity_assets(rom: Path, upstream: Path, listing: Path | None, rom_assets: bool):
    source, commit, identity = _verified_source(
        rom, upstream, owner="identity-icons", listing_path=listing
    )
    fixture = json.loads(
        repo_path("tests/fixtures/h2/icon-graphics-static-v1.json").read_text(encoding="utf-8")
    )
    for symbol, address in fixture["table"].items():
        if source.addresses[symbol] != address:
            raise ValueError("identity icon accepted address drift")
    base = source.addresses["ItemIcon000"]
    pointer = source.addresses["p_Icons"]
    if int.from_bytes(source.rom[pointer : pointer + 4], "big") != base:
        raise ValueError("identity icon pointer drift")

    def payload(path, symbol, size):
        address = source.addresses[symbol]
        if not rom_assets or (source.disasm / path).exists():
            data = source.read(path, symbol=symbol)
        else:
            data = source.rom[address : address + size]
        if len(data) != size:
            raise ValueError("identity icon source size drift")
        return data

    entries = re.findall(
        r'^\s*((?:Item|Other|Spell)Icon\d{3}):\s*incbin\s+"([^"]+)"',
        read_upstream_text(source.disasm / "data/graphics/icons/entries.asm"),
        re.MULTILINE,
    )
    if entries != _expected_entries():
        raise ValueError("identity icon storage drift")
    palette_address = source.addresses["palette_Base"]
    palette_fixture = json.loads(
        repo_path(
            "tests/fixtures/h2/map3-original-player-reference-frame-static-v1.json"
        ).read_text(encoding="utf-8")
    )
    if palette_address != palette_fixture["static"]["indexBindings"]["paletteBase"]:
        raise ValueError("identity icon palette drift")
    palette_bytes = payload(BASE_PALETTE_PATH, "palette_Base", 32)
    palette = [
        md_palette_color(int.from_bytes(palette_bytes[i : i + 2], "big")) for i in range(0, 32, 2)
    ]
    mask = payload("data/graphics/tech/iconhighlighttiles.bin", "tiles_IconHighlight", 192)
    resources, receipts = {}, []
    enums = read_upstream_text(source.disasm / "sf2enums.asm")
    item_ids = [_equ(enums, "ITEM_" + code) for code in ITEMS]
    spell_ids = [_equ(enums, "SPELL_" + code) for code in SPELLS]
    selections = [("item", i, i) for i in item_ids]
    selections += [("spell", i, i + 130) for i in spell_ids] + [("other", 128, 128)]
    for family, semantic_id, storage in selections:
        symbol, path = entries[storage]
        if source.addresses[symbol] != base + storage * ICON_BYTES:
            raise ValueError("identity icon index drift")
        data = payload(path, symbol, ICON_BYTES)
        for state, raw in (
            ("normal", data),
            ("selected", bytes(a & b for a, b in zip(data, mask, strict=True))),
        ):
            pixels, width, height = _render_tile_pool_sheet(raw, palette, sheet_columns=2)
            if (width, height) != (16, 24):
                raise ValueError("identity icon shape drift")
            key = f"identity-icon.{family}.{semantic_id}.{state}"
            digest = hashlib.sha256(bytes(pixels)).hexdigest()
            resources[key] = dict(
                width=width,
                height=height,
                format="rgba8",
                data=base64.b64encode(bytes(pixels)).decode("ascii"),
                sha256=digest,
            )
            receipts.append(
                dict(
                    resource=key,
                    symbol=symbol,
                    storageIndex=storage,
                    source=path,
                    address=source.addresses[symbol],
                    sha256=digest,
                )
            )
    return resources, dict(
        upstreamCommit=commit,
        romSha256=identity["hashes"]["sha256"],
        paletteAddress=palette_address,
        highlightAddress=source.addresses["tiles_IconHighlight"],
        layout="2x3 row-major tiles per magicmenulayout.asm MENUTILE1..6",
        highlight="source bitwise AND tiles_IconHighlight",
        resources=receipts,
    )
