"""Public-safe Map 3 entity-142 interactable two-half reference contract."""

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

ID = "sf2-map3-entity142-interactable-reference-static-v1"
FIXTURE = repo_path("tests/fixtures/h2/map3-entity142-interactable-reference-static-v1.json")
SCHEMA = repo_path("schemas/h2/map3-entity142-interactable-reference-static-output.schema.json")
FIXTURE_SCHEMA = repo_path(
    "schemas/h2/map3-entity142-interactable-reference-static-fixture.schema.json"
)
MANIFEST = repo_path("manifests/extractions/map3-entity142-interactable-reference-static.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

_LISTING = Path("build/sf2build-h1.lst")
_H1_BINARY = Path("build/sf2build-h1.bin")
_ENUMS = Path("sf2enums.asm")
_CONSTANTS = Path("sf2const.asm")
_MAP_FUNCTIONS = Path("code/common/scripting/map/mapfunctions.asm")
_FOLLOWER_FUNCTIONS = Path("code/common/scripting/map/followersfunctions_1.asm")
_FOLLOWER_TABLE = Path("data/scripting/entity/followers.asm")
_ENTITY_FUNCTIONS = Path("code/common/scripting/entity/entityfunctions_1.asm")
_MAPSCRIPT_FUNCTIONS = Path("code/common/scripting/map/mapscriptengine_1.asm")
_MAP_SETUP_FUNCTIONS = Path("code/common/scripting/map/mapsetupsfunctions_1.asm")
_MAP3_ENTITIES = Path("data/maps/entries/map03/mapsetups/s1_entities.asm")
_MAP3_EVENTS = Path("data/maps/entries/map03/mapsetups/s2_entityevents.asm")
_MAPSPRITE_ENTRIES = Path("data/graphics/mapsprites/entries.asm")
_DISPLAY_INIT = Path("code/common/tech/graphics/displayinit.asm")
_BASE_PALETTE = Path("data/graphics/tech/basepalette.bin")

_NATURAL_ROUTE_FIXTURE = repo_path("tests/fixtures/h3/map3-battle01-natural-route-v1.json")
_NATURAL_ROUTE_SCHEMA = repo_path("schemas/h3/map3-battle01-natural-route-fixture.schema.json")
_OPTIONAL_INTERACTIONS_FIXTURE = repo_path(
    "tests/fixtures/h2/map3-optional-interactions-static-v1.json"
)
_PLAYER_REFERENCE_FIXTURE = repo_path(
    "tests/fixtures/h2/map3-original-player-reference-frame-static-v1.json"
)
_PLAYER_REFERENCE_SCHEMA = repo_path(
    "schemas/h2/map3-original-player-reference-frame-static-fixture.schema.json"
)
_PLAYER_REFERENCE_OWNER = "sf2-map3-original-player-reference-frame-static-v1"

_TARGET_SOURCE_INDEX = 16
_TARGET_SOURCE_ORDINAL = 17
_TARGET_LOGICAL_ID = 142
_TARGET_PHYSICAL_SLOT = 17
_TARGET_MAPSPRITE = 209
_TARGET_SYMBOL = "Mapsprite209_0"
_ENTITY_RECORD_BYTES = 8
_EVENT_RECORD_INDEX = 15
_EVENT_RECORD_BYTES = 4
_DECODED_BYTES = 576
_HALF_BYTES = 288

_INDEX_FIXTURE = "tests/fixtures/h2/map3-entity142-interactable-reference-static-v1.json"
_INDEX_DOCUMENT = "docs/research/map3-entity142-interactable-reference.md"
_INDEX_VERIFIER = "src/sf2tool/h2/map3_entity142_interactable_reference.py"
_INDEX_BINDINGS = {
    "scripting.map.mapfunctions": (("entry", "static.indexBindings.initializeMapEntities"),),
    "scripting.map.followersfunctions-1": (
        ("entry", "static.indexBindings.initializeFollowerEntities"),
    ),
    "scripting.entity.declarenewentity": (("entry", "static.indexBindings.declareNewEntity"),),
    "map.entity-population.get-entity-address": (
        ("entry", "static.indexBindings.getEntityAddressFromCharacter"),
    ),
    "map.data.ms-map3-entities": (("entry", "static.indexBindings.map3Entities"),),
    "map.data.ms-map3-entityevents": (("entry", "static.indexBindings.map3EntityEvents"),),
    "auxiliary.data.pt-mapsprites": (("entry", "static.indexBindings.mapspritePointerTable"),),
    "tech.graphics.display-init": (("palette-base", "static.indexBindings.paletteBase"),),
}
_PREDECESSOR_INDEX_SHA256 = "7F5388A2D8F95C3046D9878E912E7E4508055CCBB6CE6E047091871A851F3B1D"


def canonical_json_bytes(value: dict[str, Any]) -> bytes:
    """Return the tracked and derived JSON representation."""
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode("utf-8")


def _canonical_digest(value: dict[str, Any]) -> str:
    return hashlib.sha256(canonical_json_bytes(value)).hexdigest().upper()


def _remove_map3_entity142_interactable_reference_later_owner_index_delta(
    index: dict[str, Any],
) -> dict[str, Any]:
    """Remove exactly this owner's evidence/document appends."""
    normalized = deepcopy(index)
    records = normalized.get("records")
    if not isinstance(records, list) or any(not isinstance(row, dict) for row in records):
        raise ValueError("Map 3 entity-142 reference index record shape drift")
    if len({row.get("id") for row in records}) != len(records):
        raise ValueError("Map 3 entity-142 reference index record identity drift")

    seen: set[str] = set()
    for record in records:
        record_id = record.get("id")
        evidence = record.get("evidence")
        documents = record.get("documents")
        if not isinstance(evidence, list) or not isinstance(documents, list):
            raise ValueError("Map 3 entity-142 reference index field drift")
        markers = [
            row
            for row in evidence
            if isinstance(row, dict)
            and (
                row.get("fixtureId") == ID
                or row.get("fixture") == _INDEX_FIXTURE
                or row.get("verifier") == _INDEX_VERIFIER
            )
        ]
        document_count = documents.count(_INDEX_DOCUMENT)
        expected_bindings = _INDEX_BINDINGS.get(record_id)
        if expected_bindings is None:
            if markers or document_count:
                raise ValueError("Map 3 entity-142 reference unknown-record drift")
            continue
        expected = {
            "level": "H2",
            "fixture": _INDEX_FIXTURE,
            "fixtureId": ID,
            "verifier": _INDEX_VERIFIER,
            "bindings": [
                {"addressId": address_id, "fixtureField": fixture_field}
                for address_id, fixture_field in expected_bindings
            ],
        }
        if markers != [expected] or evidence[-1] != expected:
            raise ValueError("Map 3 entity-142 reference index evidence drift")
        if document_count != 1 or documents[-1] != _INDEX_DOCUMENT:
            raise ValueError("Map 3 entity-142 reference index document drift")
        evidence.remove(expected)
        documents.remove(_INDEX_DOCUMENT)
        seen.add(str(record_id))

    if seen != set(_INDEX_BINDINGS):
        raise ValueError("Map 3 entity-142 reference index denominator drift")
    if _canonical_digest(normalized) != _PREDECESSOR_INDEX_SHA256:
        raise ValueError("Map 3 entity-142 reference predecessor index drift")
    return normalized


def _without_comments(source: str) -> str:
    return "\n".join(line.split(";", maxsplit=1)[0].rstrip() for line in source.splitlines())


def _source_text(disasm: Path, relative: Path) -> str:
    path = disasm / relative
    if not path.is_file():
        raise ValueError(f"Map 3 entity-142 reference source is missing: {relative.as_posix()}")
    return _without_comments(path.read_text(encoding="utf-8"))


def _require_order(source: str, fragments: tuple[str, ...], owner: str) -> None:
    cursor = 0
    for fragment in fragments:
        position = source.find(fragment, cursor)
        if position < 0:
            raise ValueError(f"Map 3 entity-142 reference source-use drift: {owner}")
        cursor = position + len(fragment)


def _function_section(source: str, symbol: str) -> str:
    start = source.find(f"{symbol}:")
    if start < 0:
        raise ValueError(f"Map 3 entity-142 reference function is missing: {symbol}")
    terminator = "\n                rts"
    end = source.find(terminator, start)
    if end < 0:
        raise ValueError(f"Map 3 entity-142 reference function boundary is missing: {symbol}")
    return source[start : end + len(terminator)]


def _parse_equate(source: str, name: str) -> int:
    match = re.search(
        rf"^\s*{re.escape(name)}:\s*equ\s+([^\s]+)",
        source,
        re.MULTILINE | re.IGNORECASE,
    )
    if match is None:
        raise ValueError(f"Map 3 entity-142 reference equate is missing: {name}")
    return int(match.group(1).replace("$", "0x").replace("%", "0b"), 0)


def _sha256(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest().upper()


def _source_and_mapping_contract(
    disasm: Path, addresses: dict[str, int], h1: bytes, rom: bytes
) -> tuple[dict[str, Any], dict[str, Any]]:
    enums = _source_text(disasm, _ENUMS)
    constants = _source_text(disasm, _CONSTANTS)
    expected_equates = {
        "ALLY_SARAH": 1,
        "ALLY_CHESTER": 2,
        "COMBATANT_ALLIES_NUMBER": 30,
        "ENTITY_ENEMY_INDEX_DIFFERENCE": 96,
        "NEXT_ENTITYDEF": 32,
        "ENTITYDEF_SIZE": 32,
        "UP": 1,
        "DOWN": 3,
        "MAPSPRITE_ASTRAL": 209,
    }
    actual_equates = {name: _parse_equate(enums, name) for name in expected_equates}
    if actual_equates != expected_equates:
        raise ValueError("Map 3 entity-142 reference enum denominator drift")
    entity_index_list = _parse_equate(constants, "ENTITY_INDEX_LIST")
    entity_data = _parse_equate(constants, "ENTITY_DATA")

    map_functions = _source_text(disasm, _MAP_FUNCTIONS)
    _require_order(
        map_functions,
        (
            "InitializeMapEntities:",
            "lea     ((ENTITY_INDEX_LIST-$1000000)).w,a1",
            "lea     NEXT_ENTITYDEF(a1),a2",
            "moveq   #1,d0",
            "bsr.w   InitializeFollowerEntities",
            "cmpi.b  #COMBATANT_ALLIES_NUMBER,d4",
            "tst.b   (a1,d4.w)",
            "move.l  (a0)+,d5",
            "bra.w   loc_4417E",
            "move.b  d0,(a2)+",
            "move.w  d0,d6",
            "bsr.w   DeclareNewEntity",
            "addq.w  #1,d0",
        ),
        "sequential allocation and follower-row reuse",
    )
    follower_functions = _source_text(disasm, _FOLLOWER_FUNCTIONS)
    _require_order(
        _function_section(follower_functions, "InitializeFollowerEntities"),
        (
            "InitializeFollowerEntities:",
            "move.b  1(a4),d0",
            "move.b  d0,(a1,d6.w)",
            "move.w  d0,d6",
            "bsr.w   DeclareNewEntity",
            "addq.w  #1,d0",
        ),
        "follower mapping",
    )
    followers = _source_text(disasm, _FOLLOWER_TABLE)
    if not re.search(
        r"^\s*table_Followers:\s*follower\s+66,\s*ALLY_SARAH,\s*\$FF,\s*1", followers, re.MULTILINE
    ):
        raise ValueError("Map 3 entity-142 reference Sarah follower row drift")
    if not re.search(r"^\s*follower\s+66,\s*ALLY_CHESTER,\s*\$FF,\s*1", followers, re.MULTILINE):
        raise ValueError("Map 3 entity-142 reference Chester follower row drift")
    entity_functions = _source_text(disasm, _ENTITY_FUNCTIONS)
    _require_order(
        _function_section(entity_functions, "DeclareNewEntity"),
        (
            "DeclareNewEntity:",
            "lea     ((ENTITY_DATA-$1000000)).w,a0",
            "lsl.w   #ENTITYDEF_SIZE_BITS,d0",
            "move.b  d6,ENTITYDEF_OFFSET_ENTNUM(a0)",
            "move.b  d4,ENTITYDEF_OFFSET_MAPSPRITE(a0)",
        ),
        "physical entity storage",
    )
    mapscript_functions = _source_text(disasm, _MAPSCRIPT_FUNCTIONS)
    _require_order(
        _function_section(mapscript_functions, "GetEntityAddressFromCharacter"),
        (
            "GetEntityAddressFromCharacter:",
            "lea     ((ENTITY_INDEX_LIST-$1000000)).w,a5",
            "andi.w  #COMBATANT_MASK_ALL,d0",
            "tst.b   d0",
            "subi.b  #ENTITY_ENEMY_INDEX_DIFFERENCE,d0",
            "move.b  (a5,d0.w),d0",
            "lsl.w   #ENTITYDEF_SIZE_BITS,d0",
            "lea     ((ENTITY_DATA-$1000000)).w,a5",
        ),
        "logical-to-physical lookup",
    )

    entity_source = _source_text(disasm, _MAP3_ENTITIES)
    rows = re.findall(
        r"^\s*(ms(?:Fixed|Walking)Entity)\s+([^\n]+?)\s*$", entity_source, re.MULTILINE
    )
    if len(rows) != 19:
        raise ValueError("Map 3 entity-142 reference source-record denominator drift")
    target_macro, target_args = rows[_TARGET_SOURCE_INDEX]
    fields = [field.strip() for field in target_args.split(",")]
    if target_macro != "msFixedEntity" or fields != [
        "54",
        "17",
        "UP",
        "MAPSPRITE_ASTRAL",
        "eas_Init",
    ]:
        raise ValueError("Map 3 entity-142 reference selected source record drift")

    record_address = addresses["ms_map3_Entities"] + _TARGET_SOURCE_INDEX * _ENTITY_RECORD_BYTES
    record = rom[record_address : record_address + _ENTITY_RECORD_BYTES]
    expected_record = bytes.fromhex("361101D1000460CE")
    if record != expected_record or h1[record_address : record_address + len(record)] != record:
        raise ValueError("Map 3 entity-142 reference source/H1/ROM record parity drift")

    first_nonally_source_ordinal = 3
    first_nonally_logical_id = 128
    first_nonally_physical_slot = 3
    nonally_declarations_through_target = _TARGET_SOURCE_ORDINAL - 2
    logical_list_offset = actual_equates["NEXT_ENTITYDEF"] + nonally_declarations_through_target - 1
    resolved_logical_id = logical_list_offset + actual_equates["ENTITY_ENEMY_INDEX_DIFFERENCE"]
    resolved_physical_slot = first_nonally_physical_slot + nonally_declarations_through_target - 1
    if (
        resolved_logical_id != _TARGET_LOGICAL_ID
        or resolved_physical_slot != _TARGET_PHYSICAL_SLOT
        or logical_list_offset != 46
    ):
        raise ValueError("Map 3 entity-142 reference logical/physical derivation drift")

    source_record = {
        "table": "ms_map3_Entities",
        "oneBasedOrdinal": _TARGET_SOURCE_ORDINAL,
        "zeroBasedIndex": _TARGET_SOURCE_INDEX,
        "address": record_address,
        "recordBytes": len(record),
        "recordHex": record.hex().upper(),
        "recordSha256": _sha256(record),
        "macro": target_macro,
        "raw": {"x": record[0], "y": record[1], "facing": record[2], "mapSprite": record[3]},
        "masked": {"x": record[0] & 0x3F, "y": record[1] & 0x3F},
        "facing": {"symbol": "UP", "value": actual_equates["UP"]},
        "mapSprite": {"symbol": "MAPSPRITE_ASTRAL", "value": record[3]},
        "fixedTail": {
            "sourceToken": fields[4],
            "hex": record[4:].hex().upper(),
            "semantics": "Unknown",
        },
        "sourceH1RomParity": True,
    }
    identity = {
        "logicalEntityId": resolved_logical_id,
        "logicalEncoding": {
            "entityIndexListOffset": logical_list_offset,
            "negativeByteTransform": "subtract-ENTITY_ENEMY_INDEX_DIFFERENCE",
            "entityEnemyIndexDifference": actual_equates["ENTITY_ENEMY_INDEX_DIFFERENCE"],
        },
        "followerReuse": {
            "requiredAcceptedRouteFlag": 66,
            "rows": [
                {"sourceOrdinal": 1, "entity": "ALLY_SARAH", "physicalSlot": 1},
                {"sourceOrdinal": 2, "entity": "ALLY_CHESTER", "physicalSlot": 2},
            ],
            "rowsConsumeNewPhysicalSlots": False,
        },
        "sequentialAllocation": {
            "firstNonAllySourceOrdinal": first_nonally_source_ordinal,
            "firstNonAllyLogicalEntityId": first_nonally_logical_id,
            "firstNonAllyPhysicalSlot": first_nonally_physical_slot,
            "nonAllyDeclarationsThroughTarget": nonally_declarations_through_target,
            "resolvedPhysicalSlot": resolved_physical_slot,
        },
        "entityIndexListRamAddress": entity_index_list,
        "entityDataRamAddress": entity_data,
        "entityDefinitionBytes": actual_equates["ENTITYDEF_SIZE"],
        "resolvedPhysicalAddress": entity_data
        + resolved_physical_slot * actual_equates["ENTITYDEF_SIZE"],
        "classification": "Confirmed-static-under-accepted-route-follower-state",
    }
    return source_record, identity


def _event_contract(
    disasm: Path, addresses: dict[str, int], h1: bytes, rom: bytes
) -> dict[str, Any]:
    event_address = addresses["ms_map3_EntityEvents"] + _EVENT_RECORD_INDEX * _EVENT_RECORD_BYTES
    event_record = rom[event_address : event_address + _EVENT_RECORD_BYTES]
    if (
        event_record != bytes.fromhex("8E030134")
        or h1[event_address : event_address + 4] != event_record
    ):
        raise ValueError("Map 3 entity-142 reference event-record parity drift")
    optional = load_json(_OPTIONAL_INTERACTIONS_FIXTURE)
    row = optional["entityEventRoutes"][_EVENT_RECORD_INDEX]
    if (
        row["recordIndex"] != _EVENT_RECORD_INDEX
        or row["entityId"] != "142"
        or row["facing"] != "DOWN"
        or row["program"]["target"] != "Map3_EntityEvent15"
        or row["routeRelevance"]
        != {"evidence": "Confirmed", "classification": "mandatory-observed-opening"}
    ):
        raise ValueError("Map 3 entity-142 reference retained event owner drift")
    if addresses["Map3_EntityEvent15"] != 331844:
        raise ValueError("Map 3 entity-142 reference event target address drift")
    setup_functions = _source_text(disasm, _MAP_SETUP_FUNCTIONS)
    _require_order(
        _function_section(setup_functions, "RunMapSetupEntityEvent"),
        (
            "RunMapSetupEntityEvent:",
            "move.b  1(a0,d7.w),d6",
            "btst    #0,d6",
            "jsr     (UpdateEntityProperties).w",
            "jsr     (a0)",
            "btst    #1,d6",
            "jsr     (UpdateEntityProperties).w",
        ),
        "entity-event facing-control operand use",
    )
    return {
        "table": "ms_map3_EntityEvents",
        "zeroBasedRecordIndex": _EVENT_RECORD_INDEX,
        "address": event_address,
        "recordBytes": len(event_record),
        "recordHex": event_record.hex().upper(),
        "recordSha256": _sha256(event_record),
        "logicalEntityId": event_record[0],
        "eventFacingControl": {
            "sourceSymbol": row["facing"],
            "value": event_record[1],
            "loadedRegister": "D6",
            "testedBits": [0, 1],
            "broaderSemantics": "Unknown",
        },
        "target": "Map3_EntityEvent15",
        "targetAddress": addresses["Map3_EntityEvent15"],
        "routeRelevance": "mandatory-observed-opening",
        "sourceH1RomParity": True,
    }


def _retained_player_reference_policy(source_record: dict[str, Any]) -> dict[str, Any]:
    fixture = load_json(_PLAYER_REFERENCE_FIXTURE)
    validate_json(fixture, _PLAYER_REFERENCE_SCHEMA, owner=str(_PLAYER_REFERENCE_FIXTURE))
    if fixture.get("id") != _PLAYER_REFERENCE_OWNER:
        raise ValueError("Map 3 entity-142 reference retained player owner drift")
    static = fixture["static"]
    direction = next(
        (
            rule
            for rule in static["directionSelection"]["rules"]
            if rule["direction"] == source_record["facing"]["symbol"]
            and rule["facing"] == source_record["facing"]["value"]
        ),
        None,
    )
    if direction is None:
        raise ValueError("Map 3 entity-142 reference retained direction policy drift")
    return {"direction": direction, "palette": static["palettePolicy"]}


def _drawable_contract(
    disasm: Path,
    addresses: dict[str, int],
    h1: bytes,
    rom: bytes,
    source_record: dict[str, Any],
) -> dict[str, Any]:
    retained_policy = _retained_player_reference_policy(source_record)
    direction = retained_policy["direction"]
    palette_policy = retained_policy["palette"]
    entries = (disasm / _MAPSPRITE_ENTRIES).read_text(encoding="utf-8")
    definitions = dict(
        re.findall(r'^\s*(Mapsprite\d{3}_[012]):\s*incbin\s+"([^"]+)"', entries, re.MULTILINE)
    )
    definition_start = entries.find(next(iter(definitions)) + ":")
    references = re.findall(r"\bdc\.l\s+(Mapsprite\d{3}_[012])\b", entries[:definition_start])
    pointer_slot = _TARGET_MAPSPRITE * 3 + direction["sourceSlot"]
    if len(references) != 720 or references[pointer_slot] != _TARGET_SYMBOL:
        raise ValueError("Map 3 entity-142 reference map-sprite pointer selection drift")
    source_path = definitions.get(_TARGET_SYMBOL)
    if source_path != "data/graphics/mapsprites/mapsprite209-0.bin":
        raise ValueError("Map 3 entity-142 reference payload source drift")
    compressed = (disasm / source_path).read_bytes()
    payload_address = addresses[_TARGET_SYMBOL]
    pointer_address = addresses["pt_Mapsprites"] + pointer_slot * 4
    expected_pointer = payload_address.to_bytes(4, "big")
    if rom[pointer_address : pointer_address + 4] != expected_pointer:
        raise ValueError("Map 3 entity-142 reference pointer ROM parity drift")
    if (
        rom[payload_address : payload_address + len(compressed)] != compressed
        or h1[payload_address : payload_address + len(compressed)] != compressed
    ):
        raise ValueError("Map 3 entity-142 reference payload source/H1/ROM parity drift")
    decoded = decode_basic_compressed(compressed, expected_output_bytes=_DECODED_BYTES)
    if decoded.input_bytes_consumed != len(compressed) or len(decoded.output) != _DECODED_BYTES:
        raise ValueError("Map 3 entity-142 reference Basic decode denominator drift")
    halves = [decoded.output[:_HALF_BYTES], decoded.output[_HALF_BYTES:]]
    if any(len(half) != _HALF_BYTES for half in halves):
        raise ValueError("Map 3 entity-142 reference decoded-half denominator drift")

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
    palette = (disasm / _BASE_PALETTE).read_bytes()
    palette_address = addresses["palette_Base"]
    if (
        len(palette) != palette_policy["encodedBytes"]
        or rom[palette_address : palette_address + len(palette)] != palette
        or h1[palette_address : palette_address + len(palette)] != palette
    ):
        raise ValueError("Map 3 entity-142 reference palette source/H1/ROM parity drift")
    words = [int.from_bytes(palette[i : i + 2], "big") for i in range(0, len(palette), 2)]
    palette_mask = int(palette_policy["wordMask"], 0)
    if len(words) != palette_policy["wordCount"] or any(word & ~palette_mask for word in words):
        raise ValueError("Map 3 entity-142 reference palette word/mask drift")

    return {
        "mapSprite": {"symbol": "MAPSPRITE_ASTRAL", "value": _TARGET_MAPSPRITE},
        "direction": {
            "policyOwner": _PLAYER_REFERENCE_OWNER,
            "symbol": direction["direction"],
            "value": direction["facing"],
            "sourceSlot": direction["sourceSlot"],
            "horizontalMirror": direction["horizontalMirror"],
        },
        "pointer": {
            "table": "pt_Mapsprites",
            "slot": pointer_slot,
            "entryAddress": pointer_address,
            "payloadSymbol": _TARGET_SYMBOL,
            "payloadAddress": payload_address,
        },
        "compressed": {
            "codec": "Basic",
            "sourcePath": source_path,
            "bytes": len(compressed),
            "sha256": _sha256(compressed),
            "sourceH1RomParity": True,
        },
        "decoded": {
            "bytes": len(decoded.output),
            "sha256": _sha256(decoded.output),
            "halfCount": 2,
            "halfBytes": _HALF_BYTES,
            "halves": [
                {"index": index, "sha256": _sha256(half)} for index, half in enumerate(halves)
            ],
        },
        "format": {
            "framePixels": [24, 24],
            "frameTiles": [3, 3],
            "tileBytes": 32,
            "bitsPerPixel": 4,
            "pixelNibbleOrder": palette_policy["pixelNibbleOrder"],
            "tileOrder": palette_policy["tileOrder"],
        },
        "palette": {
            "policyOwner": _PLAYER_REFERENCE_OWNER,
            "sourceSymbol": palette_policy["sourceSymbol"],
            "destination": palette_policy["destination"],
            "address": palette_address,
            "bytes": len(palette),
            "words": len(words),
            "wordEndian": palette_policy["wordEndian"],
            "wordMask": palette_policy["wordMask"],
            "transparentIndex": palette_policy["transparentIndex"],
            "sha256": _sha256(palette),
            "sourceH1RomParity": True,
        },
        "assetReadiness": {
            "classification": "two-half-reference",
            "selectedVisibleHalf": "Unknown",
            "interactionTimeAnimCounter": "Unknown",
        },
    }


def _retained_runtime_contract() -> dict[str, Any]:
    fixture = load_json(_NATURAL_ROUTE_FIXTURE)
    validate_json(fixture, _NATURAL_ROUTE_SCHEMA, owner=str(_NATURAL_ROUTE_FIXTURE))
    waypoint = next(
        row for row in fixture["static"]["route"]["waypoints"] if row["id"] == "map3-entity142"
    )
    expected_waypoint = {
        "id": "map3-entity142",
        "map": 3,
        "x": 55,
        "y": 17,
        "facing": "Left",
        "interaction": "entity",
        "entityTarget": {"id": 142, "map": 3, "x": 54, "y": 17, "facing": "Up"},
        "completionFlag": 602,
    }
    if waypoint != expected_waypoint:
        raise ValueError("Map 3 entity-142 reference retained waypoint drift")
    chronology = fixture["expectedObservation"]["records"][0]["chronology"]
    expected_sequence = [
        "action:ProcessPlayerAction:map3-entity142",
        "action:GetActivatedEntity:map3-entity142",
        "action:RunMapSetupEntityEvent:map3-entity142",
        "entity:Map3_EntityEvent15",
    ]
    positions = [chronology.index(item) for item in expected_sequence]
    if positions != list(range(positions[0], positions[0] + len(positions))):
        raise ValueError("Map 3 entity-142 reference retained runtime chronology drift")
    if fixture["expectedObservation"]["callbacksCleared"] is not True or not all(
        fixture["expectedObservation"]["restoration"].values()
    ):
        raise ValueError("Map 3 entity-142 reference retained runtime cleanup drift")
    return {
        "fixtureId": fixture["id"],
        "fixture": "tests/fixtures/h3/map3-battle01-natural-route-v1.json",
        "fixtureSha256": _sha256(_NATURAL_ROUTE_FIXTURE.read_bytes()),
        "classification": "retained-accepted-runtime",
        "player": {
            "map": waypoint["map"],
            "x": waypoint["x"],
            "y": waypoint["y"],
            "facing": waypoint["facing"],
        },
        "entityTarget": waypoint["entityTarget"],
        "dispatch": {
            "logicalEntityId": waypoint["entityTarget"]["id"],
            "register": "D0",
            "target": "Map3_EntityEvent15",
        },
        "chronology": expected_sequence,
        "newRuntimeObservation": False,
    }


def build_map3_entity142_interactable_reference(
    rom_path: Path, upstream_path: Path
) -> dict[str, Any]:
    """Reproduce the bounded static and retained-runtime cross-owner join."""
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / _LISTING
    h1_path = upstream_path / _H1_BINARY
    if not listing_path.is_file() or not h1_path.is_file():
        raise ValueError("Map 3 entity-142 reference H1 denominator is missing")
    addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))
    required_symbols = (
        "InitializeMapEntities",
        "InitializeFollowerEntities",
        "DeclareNewEntity",
        "GetEntityAddressFromCharacter",
        "ms_map3_Entities",
        "ms_map3_EntityEvents",
        "Map3_EntityEvent15",
        "pt_Mapsprites",
        _TARGET_SYMBOL,
        "palette_Base",
    )
    if any(symbol not in addresses for symbol in required_symbols):
        raise ValueError("Map 3 entity-142 reference H1 symbol denominator drift")
    rom = rom_path.read_bytes()
    h1 = h1_path.read_bytes()
    rom_sha256 = _sha256(rom)
    if rom_sha256 != load_json(ROM_MANIFEST)["hashes"]["sha256"]:
        raise ValueError("Map 3 entity-142 reference input ROM identity drift")
    if h1 != rom:
        raise ValueError("Map 3 entity-142 reference accepted H1/ROM identity drift")

    source_record, identity = _source_and_mapping_contract(disasm, addresses, h1, rom)
    event = _event_contract(disasm, addresses, h1, rom)
    drawable = _drawable_contract(disasm, addresses, h1, rom, source_record)
    runtime = _retained_runtime_contract()
    commitment = {
        "sourceRecordSha256": source_record["recordSha256"],
        "eventRecordSha256": event["recordSha256"],
        "payloadSymbol": drawable["pointer"]["payloadSymbol"],
        "decodedSha256": drawable["decoded"]["sha256"],
        "halfSha256": [half["sha256"] for half in drawable["decoded"]["halves"]],
        "paletteSha256": drawable["palette"]["sha256"],
    }
    transaction_digest = _canonical_digest(commitment)
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "romSha256": rom_sha256,
        "summary": {
            "existingIndexRecordObjects": len(_INDEX_BINDINGS),
            "newIndexRecordObjects": 0,
            "newIndexAddressObjects": 0,
            "newH2Bindings": sum(len(rows) for rows in _INDEX_BINDINGS.values()),
            "newDocumentAppends": len(_INDEX_BINDINGS),
            "decodedBytes": _DECODED_BYTES,
            "decodedHalves": 2,
            "paletteWords": 16,
            "unknowns": 3,
        },
        "static": {
            "indexBindings": {
                "initializeMapEntities": addresses["InitializeMapEntities"],
                "initializeFollowerEntities": addresses["InitializeFollowerEntities"],
                "declareNewEntity": addresses["DeclareNewEntity"],
                "getEntityAddressFromCharacter": addresses["GetEntityAddressFromCharacter"],
                "map3Entities": addresses["ms_map3_Entities"],
                "map3EntityEvents": addresses["ms_map3_EntityEvents"],
                "mapspritePointerTable": addresses["pt_Mapsprites"],
                "paletteBase": addresses["palette_Base"],
            },
            "sourceRecord": source_record,
            "identityMapping": identity,
            "interactionEvent": event,
            "retainedRuntime": runtime,
            "drawableReference": drawable,
            "transactionDigest": transaction_digest,
            "retainedOwners": [
                "sf2-map-entities-static-v1",
                "sf2-map3-optional-interactions-static-v1",
                "sf2-map-sprite-decode-v1",
                "sf2-map3-original-player-reference-frame-static-v1",
                "sf2-tech-graphics-static-v1",
                "sf2-map3-battle01-natural-route-runtime-v1",
            ],
            "consumerBoundary": {
                "applicationDtoOrApi": "out-of-scope",
                "futureConsumerRequirementsOnly": True,
            },
            "unknowns": {
                "interactionTimeAnimCounter": "Unknown",
                "selectedVisibleHalf": "Unknown",
                "exactObservedFrameOrAnimationContract": "Unknown",
            },
        },
    }


def verify_map3_entity142_interactable_reference(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    """Validate private inputs, public fixture/schema, index, and output digest."""
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    output = build_map3_entity142_interactable_reference(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="Map 3 entity-142 interactable reference output")
    if output != fixture:
        raise ValueError("Map 3 entity-142 interactable reference public fixture drift")
    digest = _sha256(canonical_json_bytes(output))
    manifest = load_json(MANIFEST)
    if (
        manifest.get("id") != ID
        or manifest.get("outputSha256") != digest
        or manifest.get("summary") != output["summary"]
    ):
        raise ValueError("Map 3 entity-142 interactable reference manifest drift")
    destination = output_path or repo_path(
        "local/derived/map3-entity142-interactable-reference-static-v1.json"
    )
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(canonical_json_bytes(output))
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "LogicalEntity": output["static"]["identityMapping"]["logicalEntityId"],
        "PhysicalSlot": output["static"]["identityMapping"]["sequentialAllocation"][
            "resolvedPhysicalSlot"
        ],
        "DecodedHalves": output["summary"]["decodedHalves"],
        "Status": "PASS",
    }
