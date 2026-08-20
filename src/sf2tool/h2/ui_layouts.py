from __future__ import annotations

import hashlib
import json
import re
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.source_text import read_upstream_text

ID = "sf2-ui-layout-static-v1"
MANIFEST = repo_path("manifests/extractions/ui-layout-static.json")
SCHEMA = repo_path("schemas/ui-layout-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/ui-layout-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-ui-layout-static-fixture.schema.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

DIAMOND_MENU_LAYOUT_WIDTH = 18
DIAMOND_MENU_LAYOUT_HEIGHT = 6
MENU_LAYOUT_CONSUMERS = (
    (
        "layout_DiamondMenu",
        "code/common/menus/diamondmenu.asm",
        "ExecuteDiamondMenu",
    ),
    (
        "layout_MagicMenu",
        "code/common/menus/magicmenu.asm",
        "ExecuteBattlefieldMagicMenu",
    ),
    (
        "layout_ItemMenu",
        "code/common/menus/itemmenu.asm",
        "ExecuteBattlefieldItemMenu",
    ),
)
WINDOW_ENGINE_SOURCE = "code/common/windows/windowengine.asm"

LAYOUT_SPECS = (
    (
        "layout_DiamondMenu",
        "data/graphics/tech/menus/diamondmenulayout.asm",
        DIAMOND_MENU_LAYOUT_WIDTH,
        DIAMOND_MENU_LAYOUT_HEIGHT,
    ),
    (
        "layout_MagicMenu",
        "data/graphics/tech/menus/magicmenulayout.asm",
        DIAMOND_MENU_LAYOUT_WIDTH,
        DIAMOND_MENU_LAYOUT_HEIGHT,
    ),
    (
        "layout_ItemMenu",
        "data/graphics/tech/menus/itemmenulayout.asm",
        DIAMOND_MENU_LAYOUT_WIDTH,
        DIAMOND_MENU_LAYOUT_HEIGHT,
    ),
    (
        "layout_SpellLevelIndicator",
        "data/graphics/tech/menus/spelllevelindiacatorlayouts.asm",
        3,
        2,
    ),
    (
        "layout_SpellLevelIndicator_2",
        "data/graphics/tech/menus/spelllevelindiacatorlayouts.asm",
        3,
        2,
    ),
    (
        "layout_SpellLevelIndicator_3",
        "data/graphics/tech/menus/spelllevelindiacatorlayouts.asm",
        3,
        2,
    ),
    (
        "layout_SpellLevelIndicator_4",
        "data/graphics/tech/menus/spelllevelindiacatorlayouts.asm",
        3,
        2,
    ),
    (
        "layout_SpellLevelIndicator_1in2",
        "data/graphics/tech/menus/spelllevelindiacatorlayouts.asm",
        3,
        2,
    ),
    (
        "layout_SpellLevelIndicator_2in3",
        "data/graphics/tech/menus/spelllevelindiacatorlayouts.asm",
        3,
        2,
    ),
    (
        "layout_SpellLevelIndicator_3in4",
        "data/graphics/tech/menus/spelllevelindiacatorlayouts.asm",
        3,
        2,
    ),
    (
        "layout_SpellLevelIndicator_1in3",
        "data/graphics/tech/menus/spelllevelindiacatorlayouts.asm",
        3,
        2,
    ),
    (
        "layout_SpellLevelIndicator_2in4",
        "data/graphics/tech/menus/spelllevelindiacatorlayouts.asm",
        3,
        2,
    ),
    (
        "layout_SpellLevelIndicator_1in4",
        "data/graphics/tech/menus/spelllevelindiacatorlayouts.asm",
        3,
        2,
    ),
    (
        "layout_BattleEquipWindow",
        "data/graphics/tech/windowlayouts/battleequipwindowlayout.asm",
        10,
        9,
    ),
    (
        "layout_MiniStatusWindow",
        "data/graphics/tech/windowlayouts/ministatuswindowlayout.asm",
        9,
        5,
    ),
    (
        "layout_PortraitWindow",
        "data/graphics/tech/windowlayouts/portraitwindowlayout.asm",
        8,
        10,
    ),
    (
        "layout_PortraitWindowMirrored",
        "data/graphics/tech/windowlayouts/portraitwindowlayout.asm",
        8,
        10,
    ),
    (
        "layout_AllyKillDefeatWindow",
        "data/graphics/tech/windowlayouts/allykilldefeatwindowlayout.asm",
        12,
        8,
    ),
    (
        "layout_GoldWindow",
        "data/graphics/tech/windowlayouts/goldwindowlayout.asm",
        8,
        4,
    ),
    (
        "layout_ShopInventoryWindow",
        "data/graphics/tech/windowlayouts/shopinventorylayout.asm",
        27,
        7,
    ),
    (
        "layout_YesNoPromptMenu",
        "data/graphics/tech/windowlayouts/yesnopromptlayout.asm",
        7,
        6,
    ),
    (
        "layout_BattlefieldSettingsWindow",
        "data/graphics/tech/windowlayouts/battleconfigwindowlayout.asm",
        19,
        9,
    ),
    (
        "layout_AlphabetWindow",
        "data/graphics/tech/windowlayouts/alphabetwindowlayout.asm",
        28,
        7,
    ),
    (
        "layout_NameEntryWindow",
        "data/graphics/tech/windowlayouts/namecharacterentrywindowlayout.asm",
        9,
        3,
    ),
    (
        "layout_TimerWindow",
        "data/graphics/tech/windowlayouts/timerwindowlayout.asm",
        8,
        4,
    ),
    (
        "layout_MemberStatusWindow",
        "data/graphics/tech/windowlayouts/memberstatswindowlayout.asm",
        26,
        21,
    ),
    (
        "layout_BattlesceneBackground",
        "data/graphics/tech/backgroundlayout.asm",
        32,
        12,
    ),
)

SPELL_POINTER_TARGETS = (
    "layout_SpellLevelIndicator",
    "layout_SpellLevelIndicator",
    "layout_SpellLevelIndicator",
    "layout_SpellLevelIndicator",
    "layout_SpellLevelIndicator_1in2",
    "layout_SpellLevelIndicator_2",
    "layout_SpellLevelIndicator_2",
    "layout_SpellLevelIndicator_2",
    "layout_SpellLevelIndicator_1in3",
    "layout_SpellLevelIndicator_2in3",
    "layout_SpellLevelIndicator_3",
    "layout_SpellLevelIndicator_3",
    "layout_SpellLevelIndicator_1in4",
    "layout_SpellLevelIndicator_2in4",
    "layout_SpellLevelIndicator_3in4",
    "layout_SpellLevelIndicator_4",
)

BORDER_SOURCE = "data/graphics/tech/menus/diamondmenubordertiles.asm"
BORDER_SYMBOLS = tuple(f"tiles_DiamondMenuBorder{index}" for index in range(1, 5))
ASSET_SPECS = (
    (
        "tiles_PriceTagBlank",
        "data/graphics/tech/pricetagblanktiles.bin",
        "layout/sf2-03-0x010000-0x018000.asm",
    ),
    (
        "tiles_PriceTagNumbers",
        "data/graphics/tech/pricetagnumberstiles.bin",
        "layout/sf2-03-0x010000-0x018000.asm",
    ),
    (
        "tiles_ShopInventoryItemHighlight",
        "data/graphics/tech/shopinventoryitemhighlighttiles.bin",
        "layout/sf2-03-0x010000-0x018000.asm",
    ),
    (
        "tiles_AlphabetHighlight",
        "data/graphics/tech/alphabethighlight/alphabethighlighttiles.bin",
        "data/graphics/tech/alphabethighlight/entries.asm",
    ),
)
EXCLUDED_SOURCES = (
    "data/graphics/tech/windowborder/entries.asm",
    "data/graphics/tech/windowlayouts/fighterministatuswindowlayout.asm",
)

LABEL_RE = re.compile(r"^([A-Za-z_][A-Za-z0-9_]*):")
EQUATE_RE = re.compile(r"^(VDPTILE_[A-Z0-9_]+):\s*equ\s+(\$[0-9A-F]+|\d+)", re.MULTILINE)
INCBIN_RE = re.compile(r'^incbin\s+"([^"]+)"$', re.IGNORECASE)


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _vdp_equates(disasm: Path) -> dict[str, int]:
    source = read_upstream_text(disasm / "enums/vdp.asm")
    values: dict[str, int] = {}
    for name, token in EQUATE_RE.findall(source):
        values[name] = int(token[1:], 16) if token.startswith("$") else int(token)
    return values


def _vdp_value(expression: str, equates: dict[str, int], *, base: bool) -> int:
    value = 0
    if expression:
        for raw_token in expression.split("|"):
            token = raw_token.strip()
            if token.startswith("$"):
                part = int(token[1:], 16)
            elif token.isdecimal():
                part = int(token)
            else:
                name = token if token.startswith("VDPTILE_") else f"VDPTILE_{token}"
                if name not in equates:
                    raise ValueError(f"UI layout VDP token is unknown: {token}")
                part = equates[name]
            value |= part
    if base:
        value |= equates["VDPTILE_PALETTE3"] | equates["VDPTILE_PRIORITY"]
    if not 0 <= value <= 0xFFFF:
        raise ValueError(f"UI layout VDP word is out of range: {expression}")
    return value


def _parse_source_file(
    disasm: Path, relative_path: str, addresses: dict[str, int], equates: dict[str, int]
) -> dict[str, Any]:
    source = read_upstream_text(disasm / relative_path)
    output = bytearray()
    labels: dict[str, int] = {}
    directives: Counter[str] = Counter()
    for line_number, raw_line in enumerate(source.splitlines(), 1):
        line = raw_line.split(";", 1)[0].strip()
        if not line:
            continue
        label_match = LABEL_RE.match(line)
        if label_match:
            label = label_match.group(1)
            if label in labels:
                raise ValueError(f"duplicate UI layout label in {relative_path}: {label}")
            labels[label] = len(output)
            line = line[label_match.end() :].strip()
            if not line:
                continue
        fields = line.split(None, 1)
        directive = fields[0]
        operand = fields[1].strip() if len(fields) == 2 else ""
        lower = directive.lower()
        if lower in ("vdptile", "vdpbasetile"):
            value = _vdp_value(operand, equates, base=lower == "vdpbasetile")
            output.extend(value.to_bytes(2, "big"))
        elif lower in ("dc.b", "dc.w", "dc.l"):
            size = {"dc.b": 1, "dc.w": 2, "dc.l": 4}[lower]
            for raw_value in operand.split(","):
                token = raw_value.strip()
                if token in addresses:
                    value = addresses[token]
                elif token.startswith("$"):
                    value = int(token[1:], 16)
                elif token.isdecimal():
                    value = int(token)
                else:
                    raise ValueError(
                        f"unsupported UI layout value in {relative_path}:{line_number}: {token}"
                    )
                output.extend(value.to_bytes(size, "big"))
        elif lower == "incbin":
            match = INCBIN_RE.fullmatch(line)
            if match is None:
                raise ValueError(f"unsupported incbin in {relative_path}:{line_number}")
            output.extend((disasm / match.group(1)).read_bytes())
        else:
            raise ValueError(
                f"unsupported UI layout directive in {relative_path}:{line_number}: {directive}"
            )
        directives[lower] += 1
    if not labels:
        raise ValueError(f"UI layout source has no labels: {relative_path}")
    first_label = next(iter(labels))
    base_address = addresses[first_label]
    for label, offset in labels.items():
        if addresses[label] != base_address + offset:
            raise ValueError(f"UI layout H1 label offset drift: {label}")
    return {
        "path": relative_path,
        "baseAddress": base_address,
        "labels": labels,
        "directives": dict(sorted(directives.items())),
        "data": bytes(output),
    }


def _layout_shapes() -> list[dict[str, int | str]]:
    return [
        {"symbol": symbol, "width": width, "height": height, "wordCount": width * height}
        for symbol, _, width, height in LAYOUT_SPECS
    ]


def _verify_menu_layout_consumer_shapes(disasm: Path) -> None:
    """Bind the three 108-word menu grids to their source window dimensions.

    ``CreateWindow`` consumes the high byte of ``d0.w`` as width and the low byte
    as height.  Each owning menu loads ``$1206`` immediately before its one
    ``CreateWindow`` request and later copies its corresponding layout into that
    window.  This consumer check prevents another factorization of 108 words from
    silently redefining the source grid.
    """
    expected_operand = (DIAMOND_MENU_LAYOUT_WIDTH << 8) | DIAMOND_MENU_LAYOUT_HEIGHT
    for layout_symbol, relative_path, entry_symbol in MENU_LAYOUT_CONSUMERS:
        source = read_upstream_text(disasm / relative_path)
        entry_match = re.search(
            rf"^{re.escape(entry_symbol)}:\s*$([\s\S]*?)"
            rf"^\s*; End of function {re.escape(entry_symbol)}\s*$",
            source,
            re.MULTILINE,
        )
        if entry_match is None:
            raise ValueError(f"UI menu consumer entry drift: {entry_symbol}")
        entry = entry_match.group(1)
        operands = re.findall(r"^\s*move\.w\s+#\$([0-9A-F]+),d0\s*$", entry, re.MULTILINE)
        create_calls = re.findall(r"^\s*jsr\s+\(CreateWindow\)\.w\s*$", entry, re.MULTILINE)
        if operands != [f"{expected_operand:X}"] or len(create_calls) != 1:
            raise ValueError(f"UI menu CreateWindow shape drift: {entry_symbol}")
        layout_uses = re.findall(
            rf"^\s*lea\s+{re.escape(layout_symbol)}\(pc\),\s*a0\s*$",
            source,
            re.MULTILINE,
        )
        if len(layout_uses) != 1:
            raise ValueError(f"UI menu layout consumer drift: {layout_symbol}")

    window_source = read_upstream_text(disasm / WINDOW_ENGINE_SOURCE)
    create_window = re.search(
        r"^CreateWindow:\s*$([\s\S]*?)^\s*; End of function CreateWindow\s*$",
        window_source,
        re.MULTILINE,
    )
    if create_window is None:
        raise ValueError("CreateWindow source boundary drift")
    shape_sequence = re.compile(
        r"move\.w\s+d0,d7\s*\n"
        r"\s*lsr\.w\s+#BYTE_SHIFT_COUNT,d7(?:\s*;[^\r\n]*)?\s*\n"
        r"\s*andi\.w\s+#BYTE_MASK,d0\s*\n"
        r"\s*mulu\.w\s+d7,d0",
    )
    if shape_sequence.search(create_window.group(1)) is None:
        raise ValueError("CreateWindow width/height byte-consumption drift")


def build_ui_layout_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"UI-layout H1 listing is missing: {listing_path}")
    addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))
    rom = rom_path.read_bytes()
    rom_hash = hashlib.sha256(rom).hexdigest().upper()
    if rom_hash != load_json(ROM_MANIFEST)["hashes"]["sha256"]:
        raise ValueError("UI-layout input ROM identity drift")

    _verify_menu_layout_consumer_shapes(disasm)

    source_paths = sorted({spec[1] for spec in LAYOUT_SPECS} | {BORDER_SOURCE})
    source_paths.append("data/graphics/tech/alphabethighlight/entries.asm")
    equates = _vdp_equates(disasm)
    parsed_files = {
        path: _parse_source_file(disasm, path, addresses, equates) for path in source_paths
    }
    for parsed in parsed_files.values():
        data = parsed["data"]
        address = parsed["baseAddress"]
        if rom[address : address + len(data)] != data:
            raise ValueError(f"UI layout source/H1/ROM parity drift: {parsed['path']}")

    layouts = []
    all_words: list[int] = []
    for symbol, path, width, height in LAYOUT_SPECS:
        parsed = parsed_files[path]
        offset = parsed["labels"][symbol]
        byte_count = width * height * 2
        data = parsed["data"][offset : offset + byte_count]
        if len(data) != byte_count:
            raise ValueError(f"UI layout source range is truncated: {symbol}")
        words = [int.from_bytes(data[index : index + 2], "big") for index in range(0, len(data), 2)]
        all_words.extend(words)
        layouts.append(
            {
                "symbol": symbol,
                "sourcePath": path,
                "sourceAddress": addresses[symbol],
                "width": width,
                "height": height,
                "wordCount": len(words),
                "byteCount": len(data),
                "sha256": hashlib.sha256(data).hexdigest().upper(),
            }
        )

    pointer_address = addresses["pt_layouts_SpellLevelIndicator"]
    pointer_bytes = b"".join(
        addresses[symbol].to_bytes(4, "big") for symbol in SPELL_POINTER_TARGETS
    )
    pointer_source = parsed_files[
        "data/graphics/tech/menus/spelllevelindiacatorlayouts.asm"
    ]["data"]
    if pointer_source[: len(pointer_bytes)] != pointer_bytes:
        raise ValueError("spell-level layout pointer source drift")
    if rom[pointer_address : pointer_address + len(pointer_bytes)] != pointer_bytes:
        raise ValueError("spell-level layout pointer ROM parity drift")
    pointer_entries = [
        {"index": index, "symbol": symbol, "address": addresses[symbol]}
        for index, symbol in enumerate(SPELL_POINTER_TARGETS)
    ]

    border_source = parsed_files[BORDER_SOURCE]
    borders = []
    for index, symbol in enumerate(BORDER_SYMBOLS):
        offset = border_source["labels"][symbol]
        data = border_source["data"][offset : offset + 48]
        if len(data) != 48:
            raise ValueError(f"diamond-menu border range drift: {symbol}")
        borders.append(
            {
                "index": index,
                "symbol": symbol,
                "sourceAddress": addresses[symbol],
                "byteCount": len(data),
                "sha256": hashlib.sha256(data).hexdigest().upper(),
            }
        )

    layout_owner = read_upstream_text(disasm / "layout/sf2-03-0x010000-0x018000.asm")
    layout_owner_normalized = layout_owner.replace("\\", "/")
    assets = []
    for symbol, path, owner in ASSET_SPECS:
        if owner.endswith("sf2-03-0x010000-0x018000.asm"):
            required = re.compile(
                rf'^{re.escape(symbol)}:\s*\n\s*incbin\s+"{re.escape(path)}"',
                re.MULTILINE,
            )
            if required.search(layout_owner) is None:
                raise ValueError(f"UI asset layout owner drift: {symbol}")
        data = (disasm / path).read_bytes()
        address = addresses[symbol]
        if rom[address : address + len(data)] != data:
            raise ValueError(f"UI asset ROM parity drift: {symbol}")
        assets.append(
            {
                "symbol": symbol,
                "sourcePath": path,
                "ownerPath": owner,
                "sourceAddress": address,
                "byteCount": len(data),
                "sha256": hashlib.sha256(data).hexdigest().upper(),
            }
        )

    for path in EXCLUDED_SOURCES:
        if path in layout_owner_normalized:
            raise ValueError(f"excluded UI source unexpectedly entered vanilla layout: {path}")

    palette_counts = Counter((word >> 13) & 3 for word in all_words)
    source_byte_count = sum(len(parsed["data"]) for parsed in parsed_files.values())
    alphabet_asset_bytes = next(
        row["byteCount"] for row in assets if row["symbol"] == "tiles_AlphabetHighlight"
    )
    expected_source_bytes = (
        len(all_words) * 2
        + len(pointer_bytes)
        + sum(row["byteCount"] for row in borders)
        + alphabet_asset_bytes
    )
    if source_byte_count != expected_source_bytes:
        raise ValueError("UI layout assembled source byte coverage drift")
    summary = {
        "assembledSourceFileCount": len(parsed_files),
        "assembledSourceByteCount": source_byte_count,
        "excludedSourceFileCount": len(EXCLUDED_SOURCES),
        "layoutCount": len(layouts),
        "layoutWordCount": len(all_words),
        "layoutByteCount": len(all_words) * 2,
        "uniqueLayoutWordCount": len(set(all_words)),
        "uniqueTileIndexCount": len({word & 0x7FF for word in all_words}),
        "priorityWordCount": sum(bool(word & 0x8000) for word in all_words),
        "mirrorWordCount": sum(bool(word & 0x0800) for word in all_words),
        "flipWordCount": sum(bool(word & 0x1000) for word in all_words),
        "palette1WordCount": palette_counts[0],
        "palette2WordCount": palette_counts[1],
        "palette3WordCount": palette_counts[2],
        "palette4WordCount": palette_counts[3],
        "spellPointerCount": len(pointer_entries),
        "spellPointerUniqueTargetCount": len(set(SPELL_POINTER_TARGETS)),
        "pointerTableByteCount": len(pointer_bytes),
        "borderVariantCount": len(borders),
        "borderByteCount": sum(row["byteCount"] for row in borders),
        "assetCount": len(assets),
        "assetByteCount": sum(row["byteCount"] for row in assets),
        "sourceFileRomParityCount": len(parsed_files),
        "assetRomParityCount": len(assets),
        "trackedUniqueByteCount": len(all_words) * 2
        + len(pointer_bytes)
        + sum(row["byteCount"] for row in borders)
        + sum(row["byteCount"] for row in assets),
    }
    table_symbols = ["pt_layouts_SpellLevelIndicator"]
    table_symbols.extend(spec[0] for spec in LAYOUT_SPECS)
    table_symbols.extend(BORDER_SYMBOLS)
    table_symbols.extend(spec[0] for spec in ASSET_SPECS)
    table = {symbol: addresses[symbol] for symbol in dict.fromkeys(table_symbols)}
    source_files = [
        {
            "sourcePath": parsed["path"],
            "baseAddress": parsed["baseAddress"],
            "byteCount": len(parsed["data"]),
            "labelCount": len(parsed["labels"]),
            "directives": parsed["directives"],
            "sha256": hashlib.sha256(parsed["data"]).hexdigest().upper(),
        }
        for parsed in parsed_files.values()
    ]
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "romSha256": rom_hash,
        "table": table,
        "summary": summary,
        "sourceFiles": source_files,
        "layouts": layouts,
        "spellLevelPointers": pointer_entries,
        "menuBorders": borders,
        "assets": assets,
        "excludedSources": list(EXCLUDED_SOURCES),
        "runtimeQuestions": [
            "Do window allocation, runtime tile writes, palettes, DMA ordering, and movement "
            "render "
            "all 27 static layouts with original presentation and timing?",
            "Are the unassembled window-border payload and fighter mini-status alternate reachable "
            "through any non-vanilla build path only, as the pinned original layout implies?",
        ],
    }


def verify_ui_layout_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_ui_layout_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="UI-layout contract")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != output["romSha256"]
    ):
        raise ValueError("UI-layout provenance drift")
    expected_shapes = _layout_shapes()
    if fixture["layoutShapes"] != expected_shapes:
        raise ValueError("UI-layout fixture shape declaration drift")
    asset_sizes = {row["symbol"]: row["byteCount"] for row in output["assets"]}
    for field, actual in (
        ("table", output["table"]),
        ("summary", output["summary"]),
        ("spellLevelPointerTargets", [row["symbol"] for row in output["spellLevelPointers"]]),
        ("assetSizes", asset_sizes),
        ("excludedSources", output["excludedSources"]),
        ("runtimeQuestions", output["runtimeQuestions"]),
    ):
        if fixture[field] != actual:
            raise ValueError(f"UI-layout {field} drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"] or output["summary"] != manifest["summary"]:
        raise ValueError("UI-layout canonical output drift")
    destination = output_path or repo_path("local/derived/ui-layout-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Layouts": output["summary"]["layoutCount"],
        "LayoutWords": output["summary"]["layoutWordCount"],
        "Assets": output["summary"]["assetCount"],
        "Status": "PASS",
    }
