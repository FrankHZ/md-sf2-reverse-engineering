"""Public-safe H2 contract for the original Map 3 player reference frame."""

from __future__ import annotations

import hashlib
import json
import re
from copy import deepcopy
from pathlib import Path
from typing import Any

from sf2tool.compression import decode_basic_compressed
from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.research_index import listing_symbol_addresses

ID = "sf2-map3-original-player-reference-frame-static-v1"
FIXTURE = repo_path("tests/fixtures/h2/map3-original-player-reference-frame-static-v1.json")
SCHEMA = repo_path("schemas/map3-original-player-reference-frame-static.schema.json")
FIXTURE_SCHEMA = repo_path(
    "schemas/h2/map3-original-player-reference-frame-static-fixture.schema.json"
)
MANIFEST = repo_path("manifests/extractions/map3-original-player-reference-frame-static.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

_LISTING = Path("build/sf2build-h1.lst")
_H1_BINARY = Path("build/sf2build-h1.bin")
_ENUMS = Path("sf2enums.asm")
_ALLY_TABLE = Path("data/stats/allies/allymapsprites.asm")
_MAP_FUNCTIONS = Path("code/common/scripting/map/mapfunctions.asm")
_ENTITY_FUNCTIONS = Path("code/common/scripting/entity/entityfunctions_1.asm")
_ALLY_SELECTOR = Path("code/common/scripting/entity/getallymapsprite.asm")
_ENTITY_ENGINE_1 = Path("code/common/scripting/entity/entityscriptengine_1.asm")
_ENTITY_ENGINE_2 = Path("code/common/scripting/entity/entityscriptengine_2.asm")
_EXPLORATION = Path("code/gameflow/exploration/explorationfunctions_2.asm")
_DISPLAY_INIT = Path("code/common/tech/graphics/displayinit.asm")
_MAPSPRITE_ENTRIES = Path("data/graphics/mapsprites/entries.asm")
_BASE_PALETTE = Path("data/graphics/tech/basepalette.bin")

_SELECTED_SYMBOL = "Mapsprite000_2"
_SELECTED_SLOT = 2
_DECODED_BYTES = 576
_HALF_BYTES = 288
_PALETTE_BYTES = 32
_PALETTE_WORDS = 16
_PALETTE_MASK = 0x0EEE
_DIRECTIONS = ("RIGHT", "UP", "LEFT", "DOWN")
_FACING_TABLE = (0, 1, 2, 3, 0, 2, 2, 0)
_UNKNOWN_KEYS = (
    "admissionAnimCounter",
    "admissionVisibleFrame",
    "livePalette3AtAdmission",
    "originalRenderedColorParity",
    "movementFacingTiming",
    "dmaCacheCompletion",
)
_INDEX_FIXTURE = "tests/fixtures/h2/map3-original-player-reference-frame-static-v1.json"
_INDEX_DOCUMENT = "docs/research/map3-original-player-reference-frame.md"
_INDEX_BINDINGS = {
    "scripting.map.mapfunctions": (("entry", "static.indexBindings.initializeMapEntities"),),
    "scripting.entity.declarenewentity": (("entry", "static.indexBindings.declareNewEntity"),),
    "scripting.entity.getallymapsprite": (("entry", "static.indexBindings.getAllyMapsprite"),),
    "ally.data.map-sprites": (("entry", "static.indexBindings.allyMapspriteTable"),),
    "auxiliary.data.pt-mapsprites": (
        ("entry", "static.indexBindings.mapspritePointerTable"),
        ("selected-payload", "static.indexBindings.selectedMapsprite"),
    ),
    "tech.graphics.decompression": (("entry", "static.indexBindings.loadBasicCompressedData"),),
    "scripting.entity.entityscriptengine-1": (("entry", "static.indexBindings.vintUpdateSprites"),),
    "tech.graphics.display-init": (
        ("entry", "static.indexBindings.initializeDisplay"),
        ("palette-base", "static.indexBindings.paletteBase"),
    ),
    "scripting.entity.entityscriptengine-2": (
        ("dma-entity-mapsprite", "static.indexBindings.dmaEntityMapsprite"),
    ),
    "map.entity-population.load-entity-mapsprites": (
        ("entry", "static.indexBindings.loadEntityMapsprites"),
    ),
}
_INDEX_ADDRESSES = {
    "auxiliary.data.pt-mapsprites": {
        "id": "selected-payload",
        "space": "rom",
        "kind": "observation",
        "value": 822782,
        "symbol": "Mapsprite000_2",
    },
    "tech.graphics.display-init": {
        "id": "palette-base",
        "space": "rom",
        "kind": "observation",
        "value": 12446,
        "symbol": "palette_Base",
    },
    "scripting.entity.entityscriptengine-2": {
        "id": "dma-entity-mapsprite",
        "space": "rom",
        "kind": "observation",
        "value": 24970,
        "symbol": "DmaEntityMapsprite",
    },
}
_PREDECESSOR_RECORD_SHA256 = {
    "scripting.map.mapfunctions": (
        "BC1055985063087D53AE71C65820A3865D7BE93CE4D3C1B3ADC04DE5A94ECF48"
    ),
    "scripting.entity.declarenewentity": (
        "97199A2B071B8D16C5BBE463BD5BF206BA5080091D81B79F232B4713021B8415"
    ),
    "scripting.entity.getallymapsprite": (
        "7B8E1F2E8F1894B7725A234EA4B66234B03DB0AAB2308C8BFBE1F83A63A203EC"
    ),
    "ally.data.map-sprites": ("9DA4D89FE8C014D062D920F12C7A8337C36E8A37CD0F5AFC38476E8157308BE6"),
    "auxiliary.data.pt-mapsprites": (
        "D95EC4C33D215CE49DEBAAD3B31E0F685601398A762D33878CA202A7BE921377"
    ),
    "tech.graphics.decompression": (
        "AA0910FD12359AFA4CDD3E4B1CAE45557507BCF636438B072F7F498DF5BF40AE"
    ),
    "scripting.entity.entityscriptengine-1": (
        "9D645F9CCCD01A7F31326D8ED9DFDA7C09CAEA66BCA2D82C1DC12BB231411A15"
    ),
    "tech.graphics.display-init": (
        "99A966B9E6244A9B1BBA8BD1C4400FEE44D202D09859859217F6C5C231717B31"
    ),
    "scripting.entity.entityscriptengine-2": (
        "BB8C5E2D75CCE099C8281ACEA331931FC6049B2EAF9A7EFC8740BABD1B5E7A47"
    ),
    "map.entity-population.load-entity-mapsprites": (
        "25256BBB5F0FE49D6805DD498C5B747FDFB5C0A974717B21E7EA2CE340F1FE0E"
    ),
}
_PREDECESSOR_INDEX_SHA256 = "BBE0B64A2B6FD8ED0C3C1170524767DD1F523D7C6B7137FF16734248395F6472"


def canonical_json_bytes(value: dict[str, Any]) -> bytes:
    """Return the sole canonical tracked/derived JSON representation."""
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode("utf-8")


def _canonical_digest(value: dict[str, Any]) -> str:
    return hashlib.sha256(canonical_json_bytes(value)).hexdigest().upper()


def _remove_map3_original_player_reference_frame_later_owner_index_delta(
    index: dict[str, Any],
) -> dict[str, Any]:
    """Remove only this exact ten-record delta before predecessor normalization."""
    normalized = deepcopy(index)
    records = normalized.get("records")
    if not isinstance(records, list) or any(not isinstance(row, dict) for row in records):
        raise ValueError("Map 3 player reference-frame index record shape drift")
    record_ids = [row.get("id") for row in records]
    if len(record_ids) != len(set(record_ids)):
        raise ValueError("Map 3 player reference-frame index record identity drift")

    marker_address_ids = {address["id"] for address in _INDEX_ADDRESSES.values()}
    marker_symbols = {address["symbol"] for address in _INDEX_ADDRESSES.values()}
    seen: set[str] = set()
    binding_count = 0
    address_count = 0
    for record in records:
        record_id = record.get("id")
        expected_bindings = _INDEX_BINDINGS.get(record_id)
        evidence = record.get("evidence")
        documents = record.get("documents")
        addresses = record.get("addresses")
        if (
            not isinstance(evidence, list)
            or not isinstance(documents, list)
            or not isinstance(addresses, list)
        ):
            raise ValueError("Map 3 player reference-frame index record field drift")
        marker_evidence = [
            row
            for row in evidence
            if isinstance(row, dict)
            and (
                row.get("fixtureId") == ID
                or row.get("fixture") == _INDEX_FIXTURE
                or row.get("verifier") == "src/sf2tool/h2/map3_original_player_reference_frame.py"
            )
        ]
        marker_documents = documents.count(_INDEX_DOCUMENT)
        marker_addresses = [
            row
            for row in addresses
            if isinstance(row, dict)
            and (row.get("id") in marker_address_ids or row.get("symbol") in marker_symbols)
        ]
        if expected_bindings is None:
            if marker_evidence or marker_documents or marker_addresses:
                raise ValueError("Map 3 player reference-frame index unknown-record drift")
            continue

        expected_evidence = {
            "level": "H2",
            "fixture": _INDEX_FIXTURE,
            "fixtureId": ID,
            "verifier": "src/sf2tool/h2/map3_original_player_reference_frame.py",
            "bindings": [
                {"addressId": address_id, "fixtureField": fixture_field}
                for address_id, fixture_field in expected_bindings
            ],
        }
        expected_address = _INDEX_ADDRESSES.get(record_id)
        if (
            marker_evidence != [expected_evidence]
            or evidence[-1] != expected_evidence
            or marker_documents != 1
            or documents[-1] != _INDEX_DOCUMENT
        ):
            raise ValueError("Map 3 player reference-frame index evidence/document drift")
        if expected_address is None:
            if marker_addresses:
                raise ValueError("Map 3 player reference-frame index unexpected address drift")
        elif marker_addresses != [expected_address] or addresses[-1] != expected_address:
            raise ValueError("Map 3 player reference-frame index address drift")

        evidence.remove(expected_evidence)
        documents.remove(_INDEX_DOCUMENT)
        if expected_address is not None:
            addresses.remove(expected_address)
            address_count += 1
        expected_digest = _PREDECESSOR_RECORD_SHA256[record_id]
        if _canonical_digest(record) != expected_digest:
            raise ValueError("Map 3 player reference-frame predecessor record drift")
        binding_count += len(expected_bindings)
        seen.add(record_id)

    if seen != set(_INDEX_BINDINGS) or binding_count != 12 or address_count != 3:
        raise ValueError("Map 3 player reference-frame index delta denominator drift")
    if _canonical_digest(normalized) != _PREDECESSOR_INDEX_SHA256:
        raise ValueError("Map 3 player reference-frame predecessor index drift")
    return normalized


def _without_comments(source: str) -> str:
    return "\n".join(line.split(";", maxsplit=1)[0].rstrip() for line in source.splitlines())


def _require_order(source: str, fragments: tuple[str, ...], owner: str) -> None:
    cursor = 0
    for fragment in fragments:
        index = source.find(fragment, cursor)
        if index < 0:
            raise ValueError(f"Map 3 player reference-frame source-use drift: {owner}")
        cursor = index + len(fragment)


def _parse_enum(source: str, name: str) -> int:
    match = re.search(
        rf"^\s*{re.escape(name)}:\s*equ\s+([^\s]+)",
        source,
        re.MULTILINE | re.IGNORECASE,
    )
    if match is None:
        raise ValueError(f"Map 3 player reference-frame enum is missing: {name}")
    value = match.group(1).replace("$", "0x").replace("%", "0b")
    return int(value, 0)


def _parse_facing_table(source: str, symbol: str, enums: str) -> tuple[int, ...]:
    match = re.search(
        rf"{re.escape(symbol)}:\s*((?:\s*dc\.b\s+\w+\s*){{8}})",
        source,
        re.DOTALL,
    )
    if match is None:
        raise ValueError(f"Map 3 player reference-frame facing table is missing: {symbol}")
    names = re.findall(r"dc\.b\s+(\w+)", match.group(1))
    return tuple(_parse_enum(enums, name) for name in names)


def _parse_numeric_table(source: str, symbol: str) -> tuple[int, ...]:
    match = re.search(
        rf"{re.escape(symbol)}:\s*((?:\s*dc\.b\s+\w+\s*){{8}})",
        source,
        re.DOTALL,
    )
    if match is None:
        raise ValueError(f"Map 3 player reference-frame render table is missing: {symbol}")
    return tuple(int(value, 0) for value in re.findall(r"dc\.b\s+(\w+)", match.group(1)))


def _source_text(disasm: Path, relative: Path) -> str:
    path = disasm / relative
    if not path.is_file():
        raise ValueError(f"Map 3 player reference-frame source is missing: {relative.as_posix()}")
    return _without_comments(path.read_text(encoding="utf-8"))


def _selection_contract(disasm: Path) -> tuple[dict[str, Any], tuple[dict[str, Any], ...]]:
    enums = _source_text(disasm, _ENUMS)
    values = {name: _parse_enum(enums, name) for name in _DIRECTIONS}
    if values != {"RIGHT": 0, "UP": 1, "LEFT": 2, "DOWN": 3}:
        raise ValueError("Map 3 player reference-frame direction enum drift")
    if (
        _parse_enum(enums, "ALLY_BOWIE") != 0
        or _parse_enum(enums, "CLASS_SDMN") != 0
        or _parse_enum(enums, "MAPSPRITE_BOWIE_PROMO") != 1
    ):
        raise ValueError("Map 3 player reference-frame ally/class/mapsprite enum drift")

    ally_table = _source_text(disasm, _ALLY_TABLE)
    ally_rows = re.findall(r"^\s*mapsprite\s+([A-Za-z0-9_]+)", ally_table, re.MULTILINE)
    if not ally_rows or ally_rows[0] != "BOWIE_PROMO":
        raise ValueError("Map 3 player reference-frame ally row-zero drift")
    regular_map_sprite = _parse_enum(enums, f"MAPSPRITE_{ally_rows[0]}") - 1
    if regular_map_sprite != 0:
        raise ValueError("Map 3 player reference-frame regular map-sprite ID drift")

    map_functions = _source_text(disasm, _MAP_FUNCTIONS)
    _require_order(
        map_functions,
        (
            "InitializeMapEntities:",
            "movem.w d1-d3,-(sp)",
            "moveq   #1,d0",
            "bsr.w   InitializeFollowerEntities",
            "movem.w (sp)+,d1-d3",
            "clr.w   d0",
            "clr.l   d6",
            "bsr.w   GetAllyMapsprite",
            "move.l  #eas_Idle,d5",
            "bsr.w   DeclareNewEntity",
        ),
        "controlled-player declaration",
    )
    ally_selector = _source_text(disasm, _ALLY_SELECTOR)
    _require_order(
        ally_selector,
        (
            "GetAllyMapsprite:",
            "move.w  d0,d4",
            "andi.w  #ALLY_MASK_INDEX,d4",
            "move.b  table_AllyMapsprites(pc,d4.w),d4",
            "jsr     j_GetClass",
            "cmpi.b  #CLASS_HERO,d1",
            "cmpi.b  #CLASS_SDMN,d1",
            "subq.w  #1,d4",
        ),
        "ally map-sprite derivation",
    )
    entity_functions = _source_text(disasm, _ENTITY_FUNCTIONS)
    _require_order(
        entity_functions,
        (
            "DeclareNewEntity:",
            "move.b  d3,ENTITYDEF_OFFSET_FACING(a0)",
            "move.b  d4,ENTITYDEF_OFFSET_MAPSPRITE(a0)",
            "move.b  d0,ENTITYDEF_OFFSET_ANIMCOUNTER(a0)",
            "addq.b  #1,ENTITYDEF_OFFSET_ANIMCOUNTER(a0)",
        ),
        "entity storage and animation counter",
    )

    engine_2 = _source_text(disasm, _ENTITY_ENGINE_2)
    if _parse_facing_table(engine_2, "table_FacingValues_1", enums) != _FACING_TABLE:
        raise ValueError("Map 3 player reference-frame change-facing table drift")
    if _parse_facing_table(engine_2, "table_FacingValues_2", enums) != _FACING_TABLE:
        raise ValueError("Map 3 player reference-frame load-facing table drift")
    _require_order(
        engine_2,
        (
            "DmaEntityMapsprite:",
            "move.b  ENTITYDEF_OFFSET_FACING(a0),d6",
            "move.b  table_FacingValues_2(pc,d6.w),d6",
            "bne.s   @Continue",
            "addq.w  #2,d6",
            "move.b  ENTITYDEF_OFFSET_MAPSPRITE(a0),d1",
            "move.w  d1,d0",
            "add.w   d1,d1",
            "add.w   d0,d1",
            "add.w   d6,d1",
            "subq.w  #1,d1",
            "lsl.w   #INDEX_SHIFT_COUNT,d1",
            "lea     (pt_Mapsprites).l,a0",
            "movea.l (a0,d1.w),a0",
            "jsr     (LoadBasicCompressedData).w",
        ),
        "facing selector and pointer arithmetic",
    )

    engine_1 = _source_text(disasm, _ENTITY_ENGINE_1)
    render_table = _parse_numeric_table(engine_1, "table_4E16")
    if render_table != _FACING_TABLE:
        raise ValueError("Map 3 player reference-frame mirror table drift")
    _require_order(
        engine_1,
        (
            "VInt_UpdateSprites:",
            "move.b  ENTITYDEF_OFFSET_ANIMCOUNTER(a0),d4",
            "cmpi.b  #15,d4",
            "bge.s   @WalkingFrame2",
            "move.w  #VDPTILE_ENTITIES_FRAME_1_START,d5",
            "move.w  #VDPTILE_ENTITIES_FRAME_2_START,d5",
            "move.b  ENTITYDEF_OFFSET_FACING(a0),d0",
            "move.b  table_4E16(pc,d0.w),d0",
            "bne.s   loc_4DB0",
            "ori.w   #VDPTILE_MIRROR,d5",
        ),
        "VInt first-frame and mirror branch",
    )
    exploration = _source_text(disasm, _EXPLORATION)
    _require_order(
        exploration,
        (
            "ExplorationLoop:",
            "jsr     j_InitializeMapEntities",
            "jsr     (LoadEntityMapsprites).w",
            "jsr     (InitializeExplorationSpritesFrameCounter).w",
            "jsr     (LoadMap).w",
            "bsr.w   SetBaseVIntFunctions",
            "bsr.w   WaitForEvent",
        ),
        "initialization-to-WaitForEvent order",
    )

    rules: list[dict[str, Any]] = []
    for name in ("UP", "LEFT", "RIGHT", "DOWN"):
        facing = values[name]
        transformed = _FACING_TABLE[facing]
        if transformed == 0:
            transformed += 2
        slot = regular_map_sprite * 3 + transformed - 1
        rules.append(
            {
                "direction": name,
                "facing": facing,
                "sourceSlot": slot,
                "horizontalMirror": render_table[facing] == 0,
            }
        )
    expected_rules = (
        {"direction": "UP", "facing": 1, "sourceSlot": 0, "horizontalMirror": False},
        {"direction": "LEFT", "facing": 2, "sourceSlot": 1, "horizontalMirror": False},
        {"direction": "RIGHT", "facing": 0, "sourceSlot": 1, "horizontalMirror": True},
        {"direction": "DOWN", "facing": 3, "sourceSlot": 2, "horizontalMirror": False},
    )
    if tuple(rules) != expected_rules:
        raise ValueError("Map 3 player reference-frame facing/slot/mirror derivation drift")

    player = {
        "sourceFunction": "InitializeMapEntities",
        "controlledEntityIndex": 0,
        "allyIndex": 0,
        "selectionBasis": "explicit-ally-zero",
        "selectionNotEntityRowOrder": True,
        "allyTable": "table_AllyMapsprites",
        "allyRowValue": 1,
        "classSymbol": "CLASS_SDMN",
        "classValue": 0,
        "classTransform": "subtract-one",
        "regularMapSpriteId": regular_map_sprite,
        "storageFunction": "DeclareNewEntity",
        "storedField": "ENTITYDEF_OFFSET_MAPSPRITE",
        "controlledFacing": values["DOWN"],
        "controlledDirection": "DOWN",
    }
    return player, tuple(rules)


def _payload_contract(
    disasm: Path,
    addresses: dict[str, int],
    h1: bytes,
    rom: bytes,
) -> dict[str, Any]:
    entries = (disasm / _MAPSPRITE_ENTRIES).read_text(encoding="utf-8")
    definitions = re.findall(
        r'^\s*(Mapsprite\d{3}_[012]):\s*incbin\s+"([^"]+)"',
        entries,
        re.MULTILINE,
    )
    if not definitions:
        raise ValueError("Map 3 player reference-frame map-sprite definitions are missing")
    definition_start = entries.find(f"{definitions[0][0]}:")
    references = re.findall(r"\bdc\.l\s+(Mapsprite\d{3}_[012])\b", entries[:definition_start])
    if len(references) != 720 or references[_SELECTED_SLOT] != _SELECTED_SYMBOL:
        raise ValueError("Map 3 player reference-frame source slot/symbol drift")
    paths = dict(definitions)
    if _SELECTED_SYMBOL not in paths:
        raise ValueError("Map 3 player reference-frame selected payload definition is missing")
    data = (disasm / paths[_SELECTED_SYMBOL]).read_bytes()
    source_address = addresses[_SELECTED_SYMBOL]
    if (
        rom[source_address : source_address + len(data)] != data
        or h1[source_address : source_address + len(data)] != data
    ):
        raise ValueError("Map 3 player reference-frame selected source/H1/ROM parity drift")
    table_address = addresses["pt_Mapsprites"] + _SELECTED_SLOT * 4
    pointer = source_address.to_bytes(4, "big")
    if (
        rom[table_address : table_address + 4] != pointer
        or h1[table_address : table_address + 4] != pointer
    ):
        raise ValueError("Map 3 player reference-frame selected pointer parity drift")
    decoded = decode_basic_compressed(data, expected_output_bytes=_DECODED_BYTES)
    if decoded.input_bytes_consumed != len(data):
        raise ValueError("Map 3 player reference-frame selected stream has trailing bytes")
    halves = (decoded.output[:_HALF_BYTES], decoded.output[_HALF_BYTES:])
    if len(decoded.output) != _DECODED_BYTES or tuple(map(len, halves)) != (
        _HALF_BYTES,
        _HALF_BYTES,
    ):
        raise ValueError("Map 3 player reference-frame decoded-half denominator drift")
    return {
        "pointerTable": "pt_Mapsprites",
        "sourceSlot": _SELECTED_SLOT,
        "symbol": _SELECTED_SYMBOL,
        "codec": "Basic",
        "sourceH1RomParity": True,
        "decodedBytes": _DECODED_BYTES,
        "halfCount": 2,
        "halfBytes": _HALF_BYTES,
        "framePixels": [24, 24],
        "frameTiles": [3, 3],
        "tileBytes": 32,
        "bitsPerPixel": 4,
        "tileOrder": "column-major",
    }


def _palette_contract(
    disasm: Path, addresses: dict[str, int], h1: bytes, rom: bytes
) -> dict[str, Any]:
    display = _source_text(disasm, _DISPLAY_INIT)
    _require_order(
        display,
        (
            "InitializeDisplay:",
            "lea     palette_Base(pc), a0",
            "lea     (PALETTE_3_BASE).l,a1",
            "move.w  #CRAM_PALETTE_SIZE,d7",
            "bsr.w   CopyBytes",
            "palette_Base:",
        ),
        "palette_Base to palette3 copy",
    )
    data = (disasm / _BASE_PALETTE).read_bytes()
    address = addresses["palette_Base"]
    if (
        len(data) != _PALETTE_BYTES
        or rom[address : address + len(data)] != data
        or h1[address : address + len(data)] != data
    ):
        raise ValueError("Map 3 player reference-frame palette source/H1/ROM parity drift")
    words = [int.from_bytes(data[offset : offset + 2], "big") for offset in range(0, len(data), 2)]
    if len(words) != _PALETTE_WORDS or any(word & ~_PALETTE_MASK for word in words):
        raise ValueError("Map 3 player reference-frame palette word/mask drift")
    return {
        "sourceSymbol": "palette_Base",
        "destination": "palette3",
        "sourceH1RomParity": True,
        "encodedBytes": _PALETTE_BYTES,
        "wordCount": _PALETTE_WORDS,
        "wordEndian": "big",
        "wordMask": "0x0EEE",
        "transparentIndex": 0,
        "pixelNibbleOrder": "high-nibble-left",
        "tileOrder": "column-major",
        "rightFacingTransform": "horizontal-mirror",
        "rgbExpansionPolicy": "project-inferred-rendering-policy",
    }


def build_map3_original_player_reference_frame(
    rom_path: Path, upstream_path: Path
) -> dict[str, Any]:
    """Reproduce the bounded public-safe static contract from accepted private inputs."""
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / _LISTING
    h1_path = upstream_path / _H1_BINARY
    if not listing_path.is_file() or not h1_path.is_file():
        raise ValueError("Map 3 player reference-frame H1 denominator is missing")
    listing = listing_path.read_text(encoding="utf-8")
    addresses = listing_symbol_addresses(listing)
    required_symbols = (
        "InitializeMapEntities",
        "DeclareNewEntity",
        "GetAllyMapsprite",
        "table_AllyMapsprites",
        "pt_Mapsprites",
        "Mapsprite000_2",
        "LoadBasicCompressedData",
        "VInt_UpdateSprites",
        "InitializeExplorationSpritesFrameCounter",
        "DmaEntityMapsprite",
        "LoadEntityMapsprites",
        "InitializeDisplay",
        "palette_Base",
    )
    if any(symbol not in addresses for symbol in required_symbols):
        raise ValueError("Map 3 player reference-frame H1 symbol denominator drift")
    rom = rom_path.read_bytes()
    h1 = h1_path.read_bytes()
    expected_rom = load_json(ROM_MANIFEST)["hashes"]["sha256"]
    rom_sha256 = hashlib.sha256(rom).hexdigest().upper()
    if rom_sha256 != expected_rom:
        raise ValueError("Map 3 player reference-frame input ROM identity drift")
    if h1 != rom:
        raise ValueError("Map 3 player reference-frame accepted H1/ROM identity drift")

    player, direction_rules = _selection_contract(disasm)
    payload = _payload_contract(disasm, addresses, h1, rom)
    palette = _palette_contract(disasm, addresses, h1, rom)
    if player["controlledFacing"] != 3 or payload["sourceSlot"] != 2:
        raise ValueError("Map 3 player reference-frame DOWN/slot-two selection drift")

    static = {
        "indexBindings": {
            "initializeMapEntities": addresses["InitializeMapEntities"],
            "declareNewEntity": addresses["DeclareNewEntity"],
            "getAllyMapsprite": addresses["GetAllyMapsprite"],
            "allyMapspriteTable": addresses["table_AllyMapsprites"],
            "mapspritePointerTable": addresses["pt_Mapsprites"],
            "selectedMapsprite": addresses[_SELECTED_SYMBOL],
            "loadBasicCompressedData": addresses["LoadBasicCompressedData"],
            "vintUpdateSprites": addresses["VInt_UpdateSprites"],
            "initializeDisplay": addresses["InitializeDisplay"],
            "paletteBase": addresses["palette_Base"],
            "dmaEntityMapsprite": addresses["DmaEntityMapsprite"],
            "loadEntityMapsprites": addresses["LoadEntityMapsprites"],
        },
        "controlledPlayer": player,
        "directionSelection": {
            "enumOrder": ["RIGHT", "UP", "LEFT", "DOWN"],
            "rules": list(direction_rules),
            "selectedFacing": 3,
            "selectedDirection": "DOWN",
            "selectedSlot": 2,
            "selectedSymbol": _SELECTED_SYMBOL,
        },
        "selectedPayload": payload,
        "framePolicy": {
            "label": "initial-reference-frame",
            "selectedHalf": 0,
            "classification": "project-import-policy",
            "sourceRoots": [
                "DeclareNewEntity-animation-counter-initialization",
                "VInt_UpdateSprites-first-frame-branch",
            ],
            "observedStandingOrIdleFrame": False,
            "observedVisibleAtFirstWaitForEvent": False,
        },
        "palettePolicy": palette,
        "retainedOwners": [
            "sf2-map-entities-static-v1",
            "sf2-map-sprite-assignments-static-v1",
            "sf2-map-sprite-decode-v1",
            "sf2-tech-graphics-static-v1",
        ],
        "consumerBoundary": {
            "applicationDtoOrApi": "out-of-scope",
            "futureConsumerRequirementsOnly": True,
        },
        "unknowns": {key: "Unknown" for key in _UNKNOWN_KEYS},
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {
            "repository": toolchain["sf2disasm"]["repository"],
            "commit": commit,
        },
        "romSha256": rom_sha256,
        "summary": {
            "existingIndexRecordObjects": 10,
            "newIndexRecordObjects": 0,
            "newIndexAddressObjects": 3,
            "newH2Bindings": 12,
            "newDocumentAppends": 10,
            "decodedBytes": _DECODED_BYTES,
            "decodedHalves": 2,
            "paletteWords": _PALETTE_WORDS,
            "unknowns": len(_UNKNOWN_KEYS),
        },
        "static": static,
    }


def verify_map3_original_player_reference_frame(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    """Validate the private inputs, public fixture/schema, and canonical output digest."""
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    output = build_map3_original_player_reference_frame(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="Map 3 original player reference-frame output")
    if output != fixture:
        raise ValueError("Map 3 original player reference-frame public fixture drift")
    digest = hashlib.sha256(canonical_json_bytes(output)).hexdigest().upper()
    manifest = load_json(MANIFEST)
    if manifest["id"] != ID or manifest["outputSha256"] != digest:
        raise ValueError("Map 3 original player reference-frame manifest digest drift")
    if manifest["summary"] != output["summary"]:
        raise ValueError("Map 3 original player reference-frame manifest summary drift")
    destination = output_path or repo_path(
        "local/derived/map3-original-player-reference-frame-static-v1.json"
    )
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(canonical_json_bytes(output))
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "DecodedBytes": output["summary"]["decodedBytes"],
        "PaletteWords": output["summary"]["paletteWords"],
        "Status": "PASS",
    }
