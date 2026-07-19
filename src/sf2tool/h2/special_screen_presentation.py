from __future__ import annotations

import hashlib
import json
import re
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.h2.ui_layouts import _parse_source_file, _vdp_equates
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.source_text import read_upstream_text

ID = "sf2-special-screen-presentation-static-v1"
MANIFEST = repo_path("manifests/extractions/special-screen-presentation-static.json")
SCHEMA = repo_path("schemas/special-screen-presentation-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/special-screen-presentation-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-special-screen-presentation-static-fixture.schema.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

TITLE_LAYOUT_SOURCE = "data/graphics/specialscreens/titlescreen/titlescreenlayouts.asm"
RESOURCE_SPECS = (
    (
        "palette_TitleScreen",
        "titlescreen/titlescreenpalettes.bin",
        "code/specialscreens/title/graphics.asm",
        "palette",
    ),
    (
        "palette_TitleScreenFont",
        "titlescreen/titlescreenfontpalette.bin",
        "code/common/tech/incbins/s06_incbins_titlescreen.asm",
        "palette",
    ),
    ("layout_TitleScreenA", "titlescreen/titlescreenlayoutA.bin", TITLE_LAYOUT_SOURCE, "layout"),
    ("layout_TitleScreenB", "titlescreen/titlescreenlayoutB.bin", TITLE_LAYOUT_SOURCE, "layout"),
    (
        "palette_Witch",
        "witchscreen/witchpalette.bin",
        "code/specialscreens/witch/graphics.asm",
        "palette",
    ),
    (
        "layout_Witch",
        "witchscreen/witchlayout.bin",
        "code/specialscreens/witch/graphics.asm",
        "layout",
    ),
    (
        "palette_EndingWitch",
        "witchscreen/endingwitchpalette.bin",
        "code/specialscreens/witchend/graphics.asm",
        "palette",
    ),
    (
        "layout_EndingWitch",
        "witchscreen/endingwitchlayout.bin",
        "code/specialscreens/witchend/graphics.asm",
        "layout",
    ),
    (
        "layout_EndingJewels",
        "endingjewels/endingjewelslayout.bin",
        "code/specialscreens/jewelend/graphics.asm",
        "layout",
    ),
    (
        "palette_EndingJewels",
        "endingjewels/endingjewelspalette.bin",
        "code/specialscreens/jewelend/graphics.asm",
        "palette",
    ),
    (
        "palette_EndingKissPicture",
        "endingkiss/endingkisspicturepalette.bin",
        "code/specialscreens/endkiss/graphics.asm",
        "palette",
    ),
    (
        "palette_SuspendString",
        "suspendscreen/suspendstringpalette.bin",
        "code/specialscreens/suspend/graphics.asm",
        "palette",
    ),
)
DATA_ROOT = "data/graphics/specialscreens"


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _direct_owner_has_resource(source: str, symbol: str, path: str) -> bool:
    pattern = rf"^{re.escape(symbol)}:\s*(?:\n\s*)?incbin\s+\"{re.escape(DATA_ROOT + '/' + path)}\""
    return re.search(pattern, source, re.MULTILINE) is not None


def build_special_screen_presentation_contract(
    rom_path: Path, upstream_path: Path
) -> dict[str, Any]:
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"special-screen presentation H1 listing is missing: {listing_path}")
    addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))
    table = {symbol: addresses[symbol] for symbol, *_ in RESOURCE_SPECS}
    rom = rom_path.resolve(strict=True).read_bytes()
    rom_hash = hashlib.sha256(rom).hexdigest().upper()
    if rom_hash != load_json(ROM_MANIFEST)["hashes"]["sha256"]:
        raise ValueError("special-screen presentation input ROM identity drift")

    title_layouts = _parse_source_file(disasm, TITLE_LAYOUT_SOURCE, addresses, _vdp_equates(disasm))
    resources = []
    palette_words: list[int] = []
    for symbol, relative_path, owner_path, kind in RESOURCE_SPECS:
        source_path = f"{DATA_ROOT}/{relative_path}"
        data = (disasm / source_path).read_bytes()
        if owner_path == TITLE_LAYOUT_SOURCE:
            offset = title_layouts["labels"][symbol]
            next_offsets = sorted(
                value for value in title_layouts["labels"].values() if value > offset
            )
            end = next_offsets[0] if next_offsets else len(title_layouts["data"])
            if title_layouts["data"][offset:end] != data:
                raise ValueError(f"title-screen ASM/binary mirror drift: {symbol}")
        else:
            owner = read_upstream_text(disasm / owner_path)
            if not _direct_owner_has_resource(owner, symbol, relative_path):
                raise ValueError(f"special-screen direct owner drift: {symbol}")
        address = table[symbol]
        if rom[address : address + len(data)] != data:
            raise ValueError(f"special-screen presentation ROM parity drift: {symbol}")
        words = [int.from_bytes(data[index : index + 2], "big") for index in range(0, len(data), 2)]
        if kind == "palette":
            if any(word > 0x0FFF for word in words):
                raise ValueError(f"special-screen palette word drift: {symbol}")
            palette_words.extend(words)
        resources.append(
            {
                "symbol": symbol,
                "kind": kind,
                "sourcePath": source_path,
                "ownerPath": owner_path,
                "address": address,
                "byteCount": len(data),
                "wordCount": len(words),
                "uniqueWordCount": len(set(words)),
                "sha256": hashlib.sha256(data).hexdigest().upper(),
            }
        )

    palette_rows = [row for row in resources if row["kind"] == "palette"]
    layout_rows = [row for row in resources if row["kind"] == "layout"]
    summary = {
        "resourceCount": len(resources),
        "resourceByteCount": sum(row["byteCount"] for row in resources),
        "h1AddressCount": len(table),
        "directIncbinResourceCount": sum(
            row["ownerPath"] != TITLE_LAYOUT_SOURCE for row in resources
        ),
        "asmExpandedResourceCount": sum(
            row["ownerPath"] == TITLE_LAYOUT_SOURCE for row in resources
        ),
        "asmBinaryMirrorByteCount": len(title_layouts["data"]),
        "paletteCount": len(palette_rows),
        "paletteByteCount": sum(row["byteCount"] for row in palette_rows),
        "paletteColorCount": len(palette_words),
        "paletteUniqueColorCount": len(set(palette_words)),
        "paletteZeroColorCount": palette_words.count(0),
        "layoutCount": len(layout_rows),
        "layoutByteCount": sum(row["byteCount"] for row in layout_rows),
        "layoutWordCount": sum(row["wordCount"] for row in layout_rows),
        "sourceRomParityByteCount": sum(row["byteCount"] for row in resources),
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
        "resources": resources,
        "titleLayoutFacts": {
            "sourcePath": TITLE_LAYOUT_SOURCE,
            "sourceByteCount": len(title_layouts["data"]),
            "layoutAByteCount": next(
                row["byteCount"] for row in resources if row["symbol"] == "layout_TitleScreenA"
            ),
            "layoutBByteCount": next(
                row["byteCount"] for row in resources if row["symbol"] == "layout_TitleScreenB"
            ),
            "binaryMirrorsMatchAsm": True,
        },
        "copyrightBoundary": {
            "tracksOnlyMetadataAndHashes": True,
            "rawPalettesAndLayoutsStayLocal": True,
            "compressedTileStreamsOwnedByExistingRails": True,
        },
        "runtimeQuestions": [
            "Do palette upload order, fades, and per-screen CRAM state reproduce all "
            "seven tracked palettes?",
            "Do layout transfer, mutation, scrolling, and pixel composition reproduce "
            "all five tracked layouts?",
        ],
    }


def verify_special_screen_presentation_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_special_screen_presentation_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="special-screen presentation static contract")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != output["romSha256"]
    ):
        raise ValueError("special-screen presentation provenance drift")
    resource_facts = [
        {key: row[key] for key in ("symbol", "kind", "byteCount", "wordCount", "sha256")}
        for row in output["resources"]
    ]
    for field, actual in (
        ("table", output["table"]),
        ("summary", output["summary"]),
        ("resourceFacts", resource_facts),
        ("titleLayoutFacts", output["titleLayoutFacts"]),
        ("runtimeQuestions", output["runtimeQuestions"]),
    ):
        if fixture[field] != actual:
            raise ValueError(f"special-screen presentation fixture drift: {field}")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if output["summary"] != manifest["summary"] or digest != manifest["outputSha256"]:
        raise ValueError("special-screen presentation canonical manifest drift")
    destination = output_path or repo_path("local/derived/special-screen-presentation-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Resources": output["summary"]["resourceCount"],
        "PaletteColors": output["summary"]["paletteColorCount"],
        "ParityBytes": output["summary"]["sourceRomParityByteCount"],
        "Status": "PASS",
    }
