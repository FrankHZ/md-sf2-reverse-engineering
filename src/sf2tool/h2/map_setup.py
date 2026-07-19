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

ID = "sf2-map-setup-static-v1"
MAP_SETUPS_PATH = Path("data/maps/mapsetups.asm")
MAP_SETUP_CODE_PATH = Path("code/common/scripting/map/mapsetupsfunctions_1.asm")
MACROS_PATH = Path("sf2mapsetupmacros.asm")
ENUMS_PATH = Path("sf2enums.asm")
POINTER_ROOT = Path("data/maps/entries")
MANIFEST = repo_path("manifests/extractions/map-setup-static.json")
SCHEMA = repo_path("schemas/map-setup-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/map-setup-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-map-setup-static-fixture.schema.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

FUNCTION_SYMBOLS = (
    "RunMapSetupInitFunction",
    "RunMapSetupZoneEvent",
    "RunMapSetupItemEvent",
    "RunMapSetupEntityEvent",
    "RunMapSetupAreaDescription",
    "DisplayAreaDescription",
    "GetMapSetupEntityList",
    "GetCurrentMapSetup",
)

POINTER_LAYOUT = (
    ("entities", 0),
    ("entityEvents", 4),
    ("zoneEvents", 8),
    ("areaDescriptions", 12),
    ("itemEvents", 16),
    ("initFunction", 20),
)

SELECTION_INPUTS = (
    ("missing-map", 32, ()),
    ("map3-default", 3, ()),
    ("map3-first-flag", 3, (609,)),
    ("map3-later-flag-wins", 3, (609, 506)),
    ("map3-last-flag-wins", 3, (609, 506, 543)),
    ("map7-alias-restores-default", 7, (701, 702)),
    ("map7-final-variant-wins", 7, (701, 702, 805)),
    ("map19-sixth-variant-wins", 19, (501, 609, 506, 507, 543, 982)),
    ("map33-late-alias-restores-default", 33, (523, 784, 786, 22)),
    ("map40-late-alias-restores-default", 40, (506, 507)),
)


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _require_ordered(source: str, fragments: tuple[str, ...], owner: str) -> None:
    position = -1
    for fragment in fragments:
        position = source.find(fragment, position + 1)
        if position < 0:
            raise ValueError(f"{owner} source-shape drift: missing or reordered {fragment!r}")


def _parse_routes(source: str) -> list[dict[str, Any]]:
    routes: list[dict[str, Any]] = []
    current: dict[str, Any] | None = None
    map_end_count = 0
    table_end_count = 0
    for raw_line in source.splitlines():
        line = raw_line.split(";", 1)[0]
        map_match = re.search(
            r"\bmsMap\s+(?P<map>\d+)\s*,\s*(?P<pointer>[A-Za-z_][A-Za-z0-9_]*)",
            line,
        )
        if map_match:
            if current is not None:
                raise ValueError("map setup route started before the previous row ended")
            current = {
                "map": int(map_match.group("map")),
                "defaultPointer": map_match.group("pointer"),
                "flagVariants": [],
            }
            routes.append(current)
            continue
        flag_match = re.search(
            r"\bmsFlag\s+(?P<flag>\d+)\s*,\s*(?P<pointer>[A-Za-z_][A-Za-z0-9_]*)",
            line,
        )
        if flag_match:
            if current is None:
                raise ValueError("map setup flag row has no owning map row")
            current["flagVariants"].append(
                {
                    "flag": int(flag_match.group("flag")),
                    "pointer": flag_match.group("pointer"),
                }
            )
            continue
        if re.search(r"\bmsMapEnd\b", line):
            if current is None:
                raise ValueError("map setup row terminator has no owning map row")
            current = None
            map_end_count += 1
            continue
        if re.search(r"\bmsEnd\b", line):
            if current is not None:
                raise ValueError("map setup table ended inside a map row")
            table_end_count += 1
    if current is not None or map_end_count != len(routes) or table_end_count != 1:
        raise ValueError("map setup route terminator boundary drift")
    maps = [route["map"] for route in routes]
    if len(maps) != len(set(maps)):
        raise ValueError("map setup table contains duplicate map rows")
    return routes


def _select_route(
    routes: list[dict[str, Any]], map_index: int, set_flags: set[int]
) -> str:
    route = next((route for route in routes if route["map"] == map_index), None)
    if route is None:
        return "ms_Void"
    selected = route["defaultPointer"]
    for variant in route["flagVariants"]:
        if variant["flag"] in set_flags:
            selected = variant["pointer"]
    return selected


def _encode_routes(routes: list[dict[str, Any]], addresses: dict[str, int]) -> bytes:
    encoded = bytearray()
    for route in routes:
        encoded.extend(route["map"].to_bytes(2, "big"))
        encoded.extend(addresses[route["defaultPointer"]].to_bytes(4, "big"))
        for variant in route["flagVariants"]:
            encoded.extend(variant["flag"].to_bytes(2, "big"))
            encoded.extend(addresses[variant["pointer"]].to_bytes(4, "big"))
        encoded.extend((0xFFFD).to_bytes(2, "big"))
    encoded.extend((0xFFFF).to_bytes(2, "big"))
    return bytes(encoded)


def _parse_pointer_tables(
    disasm: Path, addresses: dict[str, int], rom: bytes
) -> list[dict[str, Any]]:
    tables: list[dict[str, Any]] = []
    paths = sorted(
        (disasm / POINTER_ROOT).rglob("pointertable*.asm"),
        key=lambda path: path.as_posix(),
    )
    for path in paths:
        source = read_upstream_text(path)
        directives = re.findall(
            r"^(?:\s*(?P<label>[A-Za-z_][A-Za-z0-9_]*):)?\s*"
            r"dc\.l\s+(?P<target>[A-Za-z_][A-Za-z0-9_]*)",
            source,
            re.MULTILINE,
        )
        symbol = next((label for label, _ in directives if label), None)
        targets = [target for _, target in directives[: len(POINTER_LAYOUT)]]
        if symbol is None or len(targets) != len(POINTER_LAYOUT):
            raise ValueError(f"map setup pointer-table shape drift: {path}")
        missing = sorted({symbol, *targets} - set(addresses))
        if missing:
            raise ValueError(f"map setup pointer symbols absent from H1 listing: {missing}")
        expected = b"".join(addresses[target].to_bytes(4, "big") for target in targets)
        address = addresses[symbol]
        if rom[address : address + len(expected)] != expected:
            raise ValueError(f"map setup pointer-table ROM parity drift: {symbol}")
        tables.append(
            {
                "path": path.relative_to(disasm).as_posix(),
                "symbol": symbol,
                "address": address,
                "targets": {
                    name: {"symbol": target, "address": addresses[target]}
                    for (name, _), target in zip(POINTER_LAYOUT, targets, strict=True)
                },
            }
        )
    symbols = [table["symbol"] for table in tables]
    if len(symbols) != len(set(symbols)):
        raise ValueError("duplicate map setup pointer-table symbol")
    return tables


def _source_facts(disasm: Path) -> dict[str, Any]:
    code = read_upstream_text(disasm / MAP_SETUP_CODE_PATH)
    macros = read_upstream_text(disasm / MACROS_PATH)
    enums = read_upstream_text(disasm / ENUMS_PATH)
    _require_ordered(
        code,
        (
            "GetCurrentMapSetup:",
            "move.b  ((CURRENT_MAP-$1000000)).w,d0",
            "lea     MapSetups(pc), a1",
            "cmpi.w  #-1,(a1)",
            "lea     ms_Void(pc), a0",
            "cmp.w   (a1)+,d0",
            "movea.l (a1)+,a0",
            "move.w  (a1)+,d1",
            "cmpi.w  #$FFFD,d1",
            "jsr     j_CheckFlag",
            "movea.l (a1),a0",
            "adda.w  #4,a1",
        ),
        "map setup selector",
    )
    for fragments, owner in (
        (
            (
                "RunMapSetupZoneEvent:",
                "MAPSETUP_OFFSET_ZONE_EVENTS",
                "cmpi.b  #$FD,(a0,d7.w)",
                "cmpi.b  #-1,(a0,d7.w)",
                "cmpi.b  #-1,1(a0,d7.w)",
                "addq.w  #4,d7",
            ),
            "zone event dispatch",
        ),
        (
            (
                "RunMapSetupItemEvent:",
                "andi.w  #ITEMENTRY_MASK_INDEX,d4",
                "MAPSETUP_OFFSET_ITEM_EVENTS",
                "cmpi.b  #$FD,(a0,d7.w)",
                "cmpi.b  #-1,2(a0,d7.w)",
                "cmp.b   3(a0,d7.w),d4",
                "addq.w  #6,d7",
            ),
            "item event dispatch",
        ),
        (
            (
                "RunMapSetupEntityEvent:",
                "MAPSETUP_OFFSET_ENTITY_EVENTS",
                "cmpi.b  #$FD,(a0,d7.w)",
                "move.b  1(a0,d7.w),d6",
                "addq.w  #4,d7",
                "btst    #0,d6",
                "btst    #1,d6",
            ),
            "entity event dispatch",
        ),
        (
            (
                "DisplayAreaDescription:",
                "cmpi.b  #$FD,(a0,d7.w)",
                "cmp.w   (a0,d7.w),d0",
                "tst.b   2(a0,d7.w)",
                "tst.w   d6",
                "tst.b   3(a0,d7.w)",
                "move.b  4(a0,d7.w),d0",
                "move.b  5(a0,d7.w),d1",
                "adda.w  4(a0,d7.w),a0",
                "addq.w  #6,d7",
            ),
            "area-description dispatch",
        ),
    ):
        _require_ordered(code, fragments, owner)
    _require_ordered(
        macros,
        (
            "msMap: macro",
            "dc.w \\1",
            "dc.l \\2",
            "msFlag: macro",
            "dc.w \\1",
            "dc.l \\2",
            "msMapEnd: macro",
            "dc.w $FFFD",
            "msEnd: macro",
            "dc.w $FFFF",
        ),
        "map setup routing macros",
    )
    _require_ordered(
        enums,
        tuple(f"MAPSETUP_OFFSET_{name}: equ {offset}" for name, offset in (
            ("ENTITIES", 0),
            ("ENTITY_EVENTS", 4),
            ("ZONE_EVENTS", 8),
            ("AREA_DESCRIPTIONS", 12),
            ("ITEM_EVENTS", 16),
            ("INIT_FUNCTION", 20),
        )),
        "map setup pointer offsets",
    )
    return {
        "selector": {
            "mapTableEndWord": 0xFFFF,
            "mapRowEndWord": 0xFFFD,
            "defaultPointerLoadedBeforeFlags": True,
            "allFlagRowsAreScanned": True,
            "setFlagOverwritesCandidate": True,
            "winner": "last-set-flag-in-source-order",
            "missingMapResult": "ms_Void",
        },
        "pointerLayout": [
            {"name": name, "offset": offset} for name, offset in POINTER_LAYOUT
        ],
        "dispatch": {
            "initFunction": {"pointerOffset": 20, "voidSetupSkipsCall": True},
            "zoneEvent": {
                "pointerOffset": 8,
                "entryBytes": 4,
                "defaultMarker": 0xFD,
                "wildcardFields": ["x", "y"],
                "wildcardValue": 0xFF,
                "matchOrder": "first-matching-entry",
            },
            "itemEvent": {
                "pointerOffset": 16,
                "entryBytes": 6,
                "defaultMarker": 0xFD,
                "wildcardFields": ["x", "y", "facing"],
                "wildcardValue": 0xFF,
                "itemIndexMaskedBeforeMatch": True,
                "matchOrder": "first-matching-entry",
            },
            "entityEvent": {
                "pointerOffset": 4,
                "entryBytes": 4,
                "defaultMarker": 0xFD,
                "flagsByteOffset": 1,
                "turnTowardActorBit": 0,
                "restoreFacingAfterScriptBit": 1,
                "matchOrder": "first-matching-entry",
            },
            "areaDescription": {
                "pointerOffset": 12,
                "entryBytes": 6,
                "endMarker": 0xFD,
                "coordinateBytes": 2,
                "d6ConditionByteOffset": 2,
                "payloadKindByteOffset": 3,
                "zeroPayloadKindUsesTwoTextIndices": True,
                "nonzeroPayloadKindUsesRelativeFunction": True,
                "matchOrder": "first-matching-entry",
            },
        },
    }


def build_map_setup_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"map setup H1 listing is missing: {listing_path}")
    addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))
    rom = rom_path.read_bytes()
    expected_rom_hash = load_json(ROM_MANIFEST)["hashes"]["sha256"]
    rom_hash = hashlib.sha256(rom).hexdigest().upper()
    if rom_hash != expected_rom_hash:
        raise ValueError("map setup input ROM identity drift")

    routes = _parse_routes(read_upstream_text(disasm / MAP_SETUPS_PATH))
    if len(routes) != 64 or sum(len(route["flagVariants"]) for route in routes) != 66:
        raise ValueError("map setup routing cardinality drift")
    encoded_routes = _encode_routes(routes, addresses)
    map_setups_address = addresses["MapSetups"]
    if rom[map_setups_address : map_setups_address + len(encoded_routes)] != encoded_routes:
        raise ValueError("map setup routing source/ROM parity drift")

    pointer_tables = _parse_pointer_tables(disasm, addresses, rom)
    referenced_pointers = {
        route["defaultPointer"] for route in routes
    } | {
        variant["pointer"]
        for route in routes
        for variant in route["flagVariants"]
    }
    table_symbols = {table["symbol"] for table in pointer_tables}
    if table_symbols != referenced_pointers:
        raise ValueError("map setup routing/pointer-table ownership drift")

    alias_variants = [
        {"map": route["map"], **variant}
        for route in routes
        for variant in route["flagVariants"]
        if variant["pointer"] == route["defaultPointer"]
    ]
    selection_cases = [
        {
            "id": case_id,
            "map": map_index,
            "setFlags": list(set_flags),
            "selectedPointer": _select_route(routes, map_index, set(set_flags)),
        }
        for case_id, map_index, set_flags in SELECTION_INPUTS
    ]
    source_facts = _source_facts(disasm)
    summary = {
        "mapRowCount": len(routes),
        "flagRowCount": sum(len(route["flagVariants"]) for route in routes),
        "missingMapCount": len(set(range(79)) - {route["map"] for route in routes}),
        "routePointerReferenceCount": len(routes)
        + sum(len(route["flagVariants"]) for route in routes),
        "uniquePointerTableCount": len(pointer_tables),
        "aliasFlagRouteCount": len(alias_variants),
        "pointerSlotCount": len(pointer_tables) * len(POINTER_LAYOUT),
        "mapRoutingByteCount": len(encoded_routes),
        "pointerTableByteCount": len(pointer_tables) * len(POINTER_LAYOUT) * 4,
        "mapRoutingRomParityCount": 1,
        "pointerTableRomParityCount": len(pointer_tables),
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "romSha256": rom_hash,
        "sources": {
            "routing": MAP_SETUPS_PATH.as_posix(),
            "dispatch": MAP_SETUP_CODE_PATH.as_posix(),
            "macros": MACROS_PATH.as_posix(),
            "enums": ENUMS_PATH.as_posix(),
        },
        "function": {symbol: addresses[symbol] for symbol in FUNCTION_SYMBOLS},
        "table": {"MapSetups": map_setups_address},
        "summary": summary,
        "sourceFacts": source_facts,
        "mapOrder": [route["map"] for route in routes],
        "routes": routes,
        "pointerTables": pointer_tables,
        "aliasFlagRoutes": alias_variants,
        "selectionCases": selection_cases,
        "runtimeQuestions": [
            "area-description-byte2-d6-condition-meaning",
            "event-script-side-effects-and-transition-state-persistence",
            "portrait-text-and-entity-facing-presentation-timing",
        ],
    }


def verify_map_setup_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_map_setup_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="map setup static contract")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != output["romSha256"]
    ):
        raise ValueError("map setup fixture provenance drift")
    if output["function"] != fixture["function"] or output["table"] != fixture["table"]:
        raise ValueError("map setup address drift")
    for field in (
        "summary",
        "sourceFacts",
        "aliasFlagRoutes",
        "selectionCases",
        "runtimeQuestions",
    ):
        if output[field] != fixture["expected"][field]:
            raise ValueError(f"map setup {field} drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"] or output["summary"] != manifest["summary"]:
        raise ValueError("map setup canonical output drift")
    destination = output_path or repo_path("local/derived/map-setup-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "MapRows": output["summary"]["mapRowCount"],
        "FlagRows": output["summary"]["flagRowCount"],
        "PointerTables": output["summary"]["uniquePointerTableCount"],
        "PointerSlots": output["summary"]["pointerSlotCount"],
        "Status": "PASS",
    }
