"""Static seam builder for the entity-population/reload H3 matrix.

The H3 observer consumes this small, source-derived contract rather than
duplicating handler or callback addresses in Lua.  The one-launch matrix is
assembled from H2 source-site identities plus explicit, bounded RAM seeds.
"""

from __future__ import annotations

import hashlib
import re
from pathlib import Path
from typing import Any

from sf2tool.h2.map_init import build_map_init_contract
from sf2tool.h2.map_script_engine import build_map_script_engine_contract
from sf2tool.h2.map_setup import build_map_setup_contract
from sf2tool.h3.bizhawk import (
    DERIVED_ROOT,
    bizhawk_contract,
    run_observer,
    verify_runtime_contract,
)
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.rom import inspect_rom, mega_drive_checksum

H1_LISTING_PATH = Path("build/sf2build-h1.lst")
H2_FACT_PATH = "entityPopulationCommandFacts"
HANDLER_ORDER = (
    "newEntity",
    "loadMapEntities",
    "reloadEntities",
    "loadEntitiesFromMapSetup",
)
FIXTURE = repo_path("tests/fixtures/h3/entity-population-reload-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h3-entity-population-reload-fixture.schema.json")
OBSERVATION_SCHEMA = repo_path("schemas/h3-entity-population-reload-observation.schema.json")
OBSERVER = repo_path("tools/bizhawk/entity_population_reload_observer.lua")


def _parse_equates(source: str, names: set[str]) -> dict[str, int]:
    """Parse each source authority exactly once for an H3 derived field."""
    values: dict[str, int] = {}
    for name in sorted(names):
        match = re.search(
            rf"^{re.escape(name)}:\s+equ\s+(?P<value>\$[0-9A-Fa-f]+|-?\d+)\b",
            source,
            re.MULTILINE,
        )
        if match is None:
            raise ValueError(f"entity population source equate is missing: {name}")
        token = match.group("value")
        values[name] = int(token[1:], 16) if token.startswith("$") else int(token)
    return values


def _h1_function_lines(listing: str, symbol: str) -> list[tuple[int, str]]:
    """Return comment-free instruction records from one stable named H1 section."""
    start = re.search(rf"^[0-9A-F]{{8}}\s+{re.escape(symbol)}:\s*$", listing, re.MULTILINE)
    if start is None:
        raise ValueError(f"entity population H1 function is missing: {symbol}")
    end = listing.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"entity population H1 function end is missing: {symbol}")
    records: list[tuple[int, str]] = []
    for raw in listing[start.start() : end].splitlines():
        match = re.fullmatch(r"(?P<address>[0-9A-F]{8})\s+(?P<body>.*)", raw)
        if match is None:
            continue
        body = match.group("body").split(";", 1)[0].strip()
        body = re.sub(r"^(?:[0-9A-F]{2,8}\s+)+", "", body).strip()
        if not body or body.endswith(":"):
            continue
        if re.fullmatch(r"[A-Za-z][A-Za-z0-9_]*(?:\.[bwls])?(?:\s+.+)?", body) is None:
            raise ValueError(f"entity population H1 instruction parse drift: {raw}")
        records.append((int(match.group("address"), 16), re.sub(r"\s+", "", body)))
    return records


def _h1_instruction_address(listing: str, symbol: str, instruction: str) -> int:
    """Resolve exactly one H1 use site after stripping comments, never by text search."""
    expected = re.sub(r"\s+", "", instruction)
    matches = [
        address
        for address, actual in _h1_function_lines(listing, symbol)
        if actual == expected
    ]
    if len(matches) != 1:
        raise ValueError(
            "entity population H1 instruction identity drift for "
            f"{symbol}/{instruction}: {len(matches)}"
        )
    return matches[0]


def _closed(value: object, required: set[str], *, owner: str) -> list[dict[str, Any]]:
    if not isinstance(value, list) or not all(isinstance(row, dict) for row in value):
        raise ValueError(f"entity population H2 {owner} is not a record list")
    rows = list(value)
    if any(set(row) != required for row in rows):
        raise ValueError(f"entity population H2 {owner} record shape drift")
    return rows


def _source_statements(source: str, symbol: str) -> list[str]:
    """Read one ordinary named source function while rejecting comment look-alikes."""
    start = re.search(rf"^{re.escape(symbol)}:\s*$", source, re.MULTILINE)
    if start is None:
        raise ValueError(f"entity population source section is missing: {symbol}")
    end = source.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"entity population source section end is missing: {symbol}")
    statements = []
    for raw in source[start.end() : end].splitlines():
        line = raw.split(";", 1)[0].strip()
        if not line or line.endswith(":"):
            continue
        if re.fullmatch(r"[A-Za-z][A-Za-z0-9_]*(?:\.[bwls])?(?:\s+.+)?", line) is None:
            raise ValueError(f"entity population source instruction parse drift: {raw}")
        statements.append(re.sub(r"\s+", " ", line))
    return statements


def _ordered_source_use(
    source: str, symbol: str, expected: list[str]
) -> list[str]:
    """Guard a compact ordered use-site relationship before a fixture can compare."""
    statements = _source_statements(source, symbol)
    indexes: list[int] = []
    for instruction in expected:
        matches = [index for index, actual in enumerate(statements) if actual == instruction]
        if len(matches) != 1:
            raise ValueError(f"entity population source use-site drift: {symbol}/{instruction}")
        indexes.append(matches[0])
    if indexes != sorted(indexes):
        raise ValueError(f"entity population source use-site order drift: {symbol}")
    return [statements[index] for index in indexes]


def _literal(text: str) -> int:
    if re.fullmatch(r"\$[0-9A-Fa-f]+", text):
        return int(text[1:], 16)
    if re.fullmatch(r"-?\d+", text):
        return int(text)
    raise ValueError(f"entity population source literal is not numeric: {text}")


def _allocation_scan_facts(
    entity_source: str, constants: dict[str, int]
) -> dict[str, int | str]:
    """Bind the scan high-water edge to its counter/add use sites, not a magic limit."""
    required = [
        "moveq #62,d7",
        "dbf d7,loc_44586",
        "addq.w #1,d0",
        "mulu.w #MAP_TILE_SIZE,d1",
        "mulu.w #MAP_TILE_SIZE,d2",
        "bsr.w DeclareNewEntity",
    ]
    _ordered_source_use(entity_source, "InitializeNewEntity", required)
    counter = _literal(re.fullmatch(r"moveq #(?P<value>-?\d+),d7", required[0]).group("value"))
    if counter < 0:
        raise ValueError("entity population allocation scan counter cannot be negative")
    if constants["MAP_TILE_SIZE"] <= 0:
        raise ValueError("entity population coordinate scale cannot be nonpositive")
    return {
        "allocationScanCounter": counter,
        "allocationScanItemCount": counter + 1,
        "allocationIncrementInstruction": required[2],
        "coordinateScaleSymbol": "MAP_TILE_SIZE",
        "coordinateScaleValue": constants["MAP_TILE_SIZE"],
    }


def _clear_record_facts(entity_source: str) -> dict[str, int | str]:
    """Keep ClearEntities' record-loop counter separate from allocation scan width."""
    required = [
        "lea ((ENTITY_DATA-$1000000)).w,a0",
        "move.w #48,d7",
        "dbf d7,loc_44666",
    ]
    _ordered_source_use(entity_source, "ClearEntities", required)
    statements = _source_statements(entity_source, "ClearEntities")
    sentinel_literals = [
        match.group("value")
        for statement in statements
        if (
            match := re.fullmatch(
                r"move\.l #(?P<value>\$[0-9A-Fa-f]+),\(a0\)\+", statement
            )
        )
    ]
    if sentinel_literals != ["$70007000", "$70007000"]:
        raise ValueError("entity population clear-record sentinel use-site drift")
    counter = _literal(re.fullmatch(r"move.w #(?P<value>-?\d+),d7", required[1]).group("value"))
    if counter < 0:
        raise ValueError("entity population clear-record counter cannot be negative")
    return {
        "clearRecordCounter": counter,
        "clearRecordCount": counter + 1,
        "emptyCoordinateWord": _literal(sentinel_literals[0]) >> 16,
    }


def _handler_cursor_use_sites(row: dict[str, Any]) -> list[dict[str, Any]]:
    guard = row["sectionGuard"]
    records = _closed(
        guard["scriptCursorReadUseSites"],
        {"sourceRegister", "destinationRegister", "transferredByteCount", "instruction"},
        owner=f"{row['macro']} script cursor",
    )
    offset = 0
    result = []
    for record in records:
        if record["sourceRegister"] != "a6" or record["transferredByteCount"] not in (1, 2, 4):
            raise ValueError(f"entity population H2 cursor use-site drift: {row['macro']}")
        result.append(
            {
                "destinationRegister": record["destinationRegister"],
                "scriptInputByteOffset": offset,
                "transferredByteCount": record["transferredByteCount"],
                "instruction": record["instruction"],
            }
        )
        offset += record["transferredByteCount"]
    if offset != sum(record["widthBytes"] for record in row["operandAnnotations"]):
        raise ValueError(f"entity population H2 operand/cursor width drift: {row['macro']}")
    return result


def _source_sites(facts: dict[str, Any], macro: str) -> list[dict[str, Any]]:
    rows = []
    for site in facts["sourceSites"]:
        for command in site["commands"]:
            if command["macro"] == macro:
                expected_order_key = (
                    f"{site['programId']}:{command['commandIndex']}:{command['macro']}"
                )
                if command["sourceOrderKey"] != expected_order_key:
                    raise ValueError(
                        "entity population H2 source-site order identity drift: "
                        f"{command['sourceOrderKey']}"
                    )
                rows.append({"programId": site["programId"], **command})
    if not rows:
        raise ValueError(f"entity population H2 source sites are missing: {macro}")
    return rows


def _find_source_site(
    facts: dict[str, Any], macro: str, source_order_key: str
) -> dict[str, Any]:
    matches = [
        row for row in _source_sites(facts, macro) if row["sourceOrderKey"] == source_order_key
    ]
    if len(matches) != 1:
        raise ValueError(f"entity population H2 source-site identity drift: {source_order_key}")
    return matches[0]


def _map_setup_default_entity_table(
    setup: dict[str, Any], map_index: int
) -> dict[str, Any]:
    """Resolve an exact default map-setup entity pointer through the H2 selection table."""
    route = next((row for row in setup["routes"] if row["map"] == map_index), None)
    if route is None or route["flagVariants"] is None:
        raise ValueError(f"entity population H2 map setup route is missing: {map_index}")
    pointer = route["defaultPointer"]
    table = next((row for row in setup["pointerTables"] if row["symbol"] == pointer), None)
    if table is None:
        raise ValueError(f"entity population H2 default pointer is missing: {pointer}")
    entities = table["targets"]["entities"]
    if set(entities) != {"symbol", "address"}:
        raise ValueError("entity population H2 map setup entity target shape drift")
    return {
        "currentMap": map_index,
        "selectedPointer": pointer,
        "selectedPointerAddress": table["address"],
        "entityTableSymbol": entities["symbol"],
        "entityTableAddress": entities["address"],
    }


def _map_init_script_joins(
    init: dict[str, Any], source_programs: set[str]
) -> list[dict[str, Any]]:
    """Retain the exact H2 map-init callers that name a bounded source program."""
    rows = []
    for source in init["primarySourceBodies"]:
        for operation in source["operations"]:
            target = operation["scriptTargetSymbol"]
            if target in source_programs:
                rows.append(
                    {
                        "sourceOwnerSymbol": source["sourceOwnerSymbol"],
                        "sourcePath": source["path"],
                        "operationIndex": operation["index"],
                        "scriptTargetSymbol": target,
                        "scriptTargetAddress": operation["scriptTargetAddress"],
                        "scriptTargetResolution": operation["scriptTargetResolution"],
                    }
                )
    if [row["scriptTargetSymbol"] for row in rows] != ["cs_5249E", "cs_5EF60"]:
        raise ValueError("entity population H2 map-init script join drift")
    return rows


def _resolved_source_input_sites(facts: dict[str, Any], macro: str) -> list[dict[str, Any]]:
    """Expose exact, resolved source operands only when every operand is a numeric authority."""
    rows = []
    for site in _source_sites(facts, macro):
        operands = site["operandValues"]
        if any(not isinstance(row["resolvedValue"], int) for row in operands):
            continue
        rows.append(
            {
                "sourceOrderKey": site["sourceOrderKey"],
                "programId": site["programId"],
                "inputValues": [row["resolvedValue"] for row in operands],
            }
        )
    if not rows:
        raise ValueError(f"entity population resolved source input sites are missing: {macro}")
    return rows


def _entity_index_list_offset(entity_number: int, constants: dict[str, int]) -> int:
    """Resolve InitializeNewEntity's signed-byte identity lookup from parsed source constants."""
    if not 0 <= entity_number <= constants["combatantMaskAll"]:
        raise ValueError("entity population entity-number byte boundary drift")
    return (
        entity_number - constants["entityEnemyIndexDifference"]
        if entity_number & 0x80
        else entity_number
    )


def _entity_identity_index_facts(
    entity_source: str, constants: dict[str, int]
) -> dict[str, int | str]:
    """Guard the signed identity branch and its parsed subtraction use site."""
    required = [
        "tst.b d7",
        "bpl.s @Ally",
        "subi.w #ENTITY_ENEMY_INDEX_DIFFERENCE,d7",
        "adda.w d7,a0",
    ]
    _ordered_source_use(entity_source, "InitializeNewEntity", required)
    return {
        "identitySignedByteBranchInstruction": required[1],
        "entityEnemyIndexDifference": constants["ENTITY_ENEMY_INDEX_DIFFERENCE"],
        "combatantMaskAll": constants["COMBATANT_MASK_ALL"],
    }


def _reload_input_use_sites(
    handler: dict[str, Any], constants: dict[str, int]
) -> list[dict[str, Any]]:
    """Bind reload's record offsets and tile divisions to guarded H2 use sites."""
    uses = _closed(
        handler["sectionGuard"]["sourceConstantUses"],
        {"symbol", "value", "instruction"},
        owner="reload source constants",
    )
    expected = [
        ("MAP_TILE_SIZE", "divu.w #MAP_TILE_SIZE,d1"),
        ("ENTITYDEF_OFFSET_Y", "move.w ENTITYDEF_OFFSET_Y(a5),d2"),
        ("MAP_TILE_SIZE", "divu.w #MAP_TILE_SIZE,d2"),
        ("ENTITYDEF_OFFSET_FACING", "move.b ENTITYDEF_OFFSET_FACING(a5),d3"),
    ]
    if [(row["symbol"], row["instruction"]) for row in uses] != expected:
        raise ValueError("entity population reload source-use order drift")
    if any(row["value"] != constants[row["symbol"]] for row in uses):
        raise ValueError("entity population reload source-use value drift")
    return uses


def _callbacks(
    handler: dict[str, Any], listing: str, addresses: dict[str, int]
) -> list[dict[str, Any]]:
    """Bind the H2 direct-call order to the one corresponding H1 call use site."""
    calls = _closed(
        handler["directCalls"], {"opcode", "instructionTarget"}, owner="direct calls"
    )
    order = handler["sectionGuard"]["directCallOrder"]
    if not isinstance(order, list) or len(order) != len(calls):
        raise ValueError(f"entity population H2 direct-call order drift: {handler['handler']}")
    result = []
    for call, instruction in zip(calls, order, strict=True):
        target = call["instructionTarget"]
        if (
            not isinstance(target, str)
            or target not in addresses
            or not isinstance(instruction, str)
            or re.fullmatch(
                rf"{call['opcode']}(?:\.[bwls])?\s+\(?{re.escape(target)}\)?(?:\.w)?",
                instruction,
            )
            is None
        ):
            raise ValueError(f"entity population H2 call identity drift: {handler['handler']}")
        result.append(
            {
                "opcode": call["opcode"],
                "instructionTarget": target,
                "instruction": instruction,
                "callSiteAddress": _h1_instruction_address(
                    listing, handler["handler"], instruction
                ),
                "targetAddress": addresses[target],
            }
        )
    return result


def build_entity_population_reload_static_contract(
    rom_path: Path, upstream_path: Path
) -> dict[str, Any]:
    """Derive H3 callback identities and entity storage facts from guarded H2 uses."""
    static = build_map_script_engine_contract(rom_path, upstream_path)
    facts = static[H2_FACT_PATH]
    setup = build_map_setup_contract(rom_path, upstream_path)
    init = build_map_init_contract(rom_path, upstream_path)
    entity_source = (
        upstream_path
        / "disasm"
        / "code/common/scripting/entity/entityfunctions_1.asm"
    ).read_text(encoding="utf-8")
    handlers = facts["handlers"]
    if [row.get("macro") for row in handlers] != list(HANDLER_ORDER):
        raise ValueError("entity population H2 handler source order drift")
    listing = (upstream_path / H1_LISTING_PATH).read_text(encoding="utf-8")
    addresses = listing_symbol_addresses(listing)
    constants = _parse_equates(
        "\n".join(
            (upstream_path / "disasm" / path).read_text(encoding="utf-8")
            for path in ("sf2const.asm", "sf2enums.asm")
        ),
        {
            "ENTITY_DATA",
            "ENTITY_INDEX_LIST",
            "ENTITYDEF_OFFSET_X",
            "ENTITYDEF_OFFSET_FACING",
            "ENTITYDEF_OFFSET_MAPSPRITE",
            "ENTITYDEF_OFFSET_ENTNUM",
            "ENTITYDEF_OFFSET_XDEST",
            "ENTITYDEF_OFFSET_Y",
            "ENTITYDEF_SIZE",
            "MAP_TILE_SIZE",
            "CURRENT_MAP",
            "GAME_FLAGS",
            "MAP_TACTICAL_BASE",
            "ENTITY_ENEMY_INDEX_DIFFERENCE",
            "COMBATANT_MASK_ALL",
        },
    )
    handler_rows = []
    for row in handlers:
        guard = row.get("sectionGuard")
        required_guard = {
            "orderedInstructions",
            "scriptCursorReadUseSites",
            "pointerReadUseSites",
            "vintControlRecords",
            "sourceConstantUses",
            "directCallOrder",
            "returnInstruction",
        }
        if not isinstance(guard, dict) or set(guard) != required_guard:
            raise ValueError(f"entity population H2 section guard drift: {row['handler']}")
        if guard["orderedInstructions"] != row.get("guardedStatements"):
            raise ValueError(f"entity population H2 guarded statement drift: {row['handler']}")
        if addresses.get(row["handler"]) != row["address"]:
            raise ValueError(f"entity population H1 handler address drift: {row['handler']}")
        handler_rows.append(
            {
                "macro": row["macro"],
                "handler": row["handler"],
                "handlerAddress": row["address"],
                "opcode": row["opcode"],
                "operandByteCount": sum(
                    record["transferredByteCount"]
                    for record in guard["scriptCursorReadUseSites"]
                ),
                "scriptCursorUseSites": _handler_cursor_use_sites(row),
                "callbacks": _callbacks(row, listing, addresses),
                "vintControlRecords": guard["vintControlRecords"],
            }
        )
    direct_table_site = _find_source_site(
        facts, "loadMapEntities", "cs_55832:2:loadMapEntities"
    )
    direct_table_symbol = direct_table_site["arguments"][0]
    if (
        direct_table_site["operandValues"][0]["resolution"] != "symbol"
        or direct_table_symbol not in addresses
    ):
        raise ValueError("entity population direct-table source identity drift")
    reload_source_site = _find_source_site(
        facts, "reloadEntities", "cs_55832:76:reloadEntities"
    )
    reload_table_symbol = reload_source_site["arguments"][0]
    if (
        reload_source_site["operandValues"][0]["resolution"] != "symbol"
        or reload_table_symbol not in addresses
    ):
        raise ValueError("entity population reload source identity drift")
    map_setup_sites = _source_sites(facts, "loadEntitiesFromMapSetup")
    if len(map_setup_sites) != 7:
        raise ValueError("entity population map-setup source-site count drift")
    map_setup_site_rows = []
    for site in map_setup_sites:
        operands = site["operandValues"]
        if (
            len(operands) != 3
            or [row["parameterOrdinal"] for row in operands] != [1, 2, 3]
            or any(row["widthBytes"] != 2 for row in operands)
            or any(not isinstance(row["resolvedValue"], int) for row in operands)
        ):
            raise ValueError(
                f"entity population map-setup operand use-site drift: {site['sourceOrderKey']}"
            )
        map_setup_site_rows.append(
            {
                "sourceOrderKey": site["sourceOrderKey"],
                "programId": site["programId"],
                "inputWords": [row["resolvedValue"] for row in operands],
            }
        )
    allocation = _allocation_scan_facts(entity_source, constants)
    cleared = _clear_record_facts(entity_source)
    identity = _entity_identity_index_facts(entity_source, constants)
    if allocation["coordinateScaleSymbol"] != "MAP_TILE_SIZE":
        raise ValueError("entity population coordinate scale source identity drift")
    if allocation["allocationScanItemCount"] <= cleared["clearRecordCount"]:
        raise ValueError("entity population scan/clear range relationship drift")
    map17_setup = _map_setup_default_entity_table(setup, 17)
    if map17_setup["entityTableSymbol"] != "ms_map17_Entities":
        raise ValueError("entity population selected map setup entity table drift")
    map_init_joins = _map_init_script_joins(
        init, {row["programId"] for row in map_setup_site_rows}
    )
    new_entity_sites = _resolved_source_input_sites(facts, "newEntity")
    if not any(row["sourceOrderKey"] == "cs_55242:1:newEntity" for row in new_entity_sites):
        raise ValueError("entity population new-entity source input identity drift")
    reload_handler = next(row for row in handlers if row["macro"] == "reloadEntities")
    reload_input_uses = _reload_input_use_sites(reload_handler, constants)
    return {
        "function": {
            "runMapSetupInitFunctionAddress": addresses["RunMapSetupInitFunction"],
            "newEntityHandlerAddress": addresses["csc2B_initializeNewEntity"],
            "loadMapEntitiesHandlerAddress": addresses["csc42_loadMapEntities"],
            "reloadEntitiesHandlerAddress": addresses["csc44_reloadEntities"],
            "loadEntitiesFromMapSetupHandlerAddress": addresses[
                "csc49_loadEntitiesFromMapSetup"
            ],
            "initializeNewEntityAddress": addresses["InitializeNewEntity"],
            "initializeMapEntitiesAddress": addresses["InitializeMapEntities"],
            "getEntityAddressFromCharacterAddress": addresses[
                "GetEntityAddressFromCharacter"
            ],
            "getMapSetupEntityListAddress": addresses["GetMapSetupEntityList"],
            "loadEntityMapspritesAddress": addresses["LoadEntityMapsprites"],
        },
        "ram": {
            "entityDataAddress": constants["ENTITY_DATA"],
            "entityIndexListAddress": constants["ENTITY_INDEX_LIST"],
            "currentMapAddress": constants["CURRENT_MAP"],
            "gameFlagsAddress": constants["GAME_FLAGS"],
        },
        "constants": {
            "entityRecordByteCount": constants["ENTITYDEF_SIZE"],
            "mapTileSize": constants["MAP_TILE_SIZE"],
            "mapTacticalBase": constants["MAP_TACTICAL_BASE"],
            **identity,
            **allocation,
            **cleared,
            "entityFieldOffsets": {
                "xWord": constants["ENTITYDEF_OFFSET_X"],
                "yWord": constants["ENTITYDEF_OFFSET_Y"],
                "xDestWord": constants["ENTITYDEF_OFFSET_XDEST"],
                "facingByte": constants["ENTITYDEF_OFFSET_FACING"],
                "entityNumberByte": constants["ENTITYDEF_OFFSET_ENTNUM"],
                "mapspriteByte": constants["ENTITYDEF_OFFSET_MAPSPRITE"],
            },
        },
        "sourceFacts": {
            "provenance": {
                "h2FixturePath": "tests/fixtures/h2/map-script-engine-static-v1.json",
                "h2FixtureId": "sf2-map-script-engine-static-v1",
                "h2FieldPath": f"expected.{H2_FACT_PATH}",
            },
            "handlers": handler_rows,
            "directTableSourceSite": {
                "sourceOrderKey": direct_table_site["sourceOrderKey"],
                "programId": direct_table_site["programId"],
                "tableSymbol": direct_table_symbol,
                "tableAddress": addresses[direct_table_symbol],
            },
            "reloadSourceSite": {
                "sourceOrderKey": reload_source_site["sourceOrderKey"],
                "programId": reload_source_site["programId"],
                "tableSymbol": reload_table_symbol,
                "tableAddress": addresses[reload_table_symbol],
            },
            "mapSetupInputSourceSites": map_setup_site_rows,
            "newEntityInputSourceSites": new_entity_sites,
            "mapSetupDefaultEntityTable": map17_setup,
            "mapInitScriptJoins": map_init_joins,
            "reloadInputUseSites": reload_input_uses,
            "callerBreakdown": facts["callerBreakdown"],
            "runtimeQuestions": facts["runtimeQuestions"],
        },
    }


def _instrument_rom(rom_path: Path, instrumentation: dict[str, Any]) -> Path:
    """Create the fixture-checked, session-only handler trampoline ROM."""
    original = rom_path.resolve(strict=True)
    original_hash = inspect_rom(original)["sha256"]
    data = bytearray(original.read_bytes())
    call_site = instrumentation["callSiteAddress"]
    stub_address = instrumentation["stubAddress"]
    original_call = bytes.fromhex(instrumentation["callSiteOriginalHex"])
    patched_call = bytes.fromhex(instrumentation["callSitePatchedHex"])
    original_stub = bytes.fromhex(instrumentation["stubOriginalHex"])
    stub = bytes.fromhex(instrumentation["stubHex"])
    if data[call_site : call_site + len(original_call)] != original_call:
        raise ValueError("entity population trampoline call-site bytes drifted")
    if data[stub_address : stub_address + len(original_stub)] != original_stub:
        raise ValueError("entity population trampoline padding bytes drifted")
    if patched_call != b"\x4E\xB9" + stub_address.to_bytes(4, "big"):
        raise ValueError("entity population trampoline call shape drifted")
    if len(stub) > len(original_stub):
        raise ValueError("entity population trampoline exceeds verified padding")
    if instrumentation["postHandlerAddress"] != stub_address + len(stub) - 2:
        raise ValueError("entity population trampoline return boundary drifted")
    data[call_site : call_site + len(patched_call)] = patched_call
    data[stub_address : stub_address + len(stub)] = stub
    data[0x18E:0x190] = int(mega_drive_checksum(bytes(data)), 16).to_bytes(2, "big")
    if inspect_rom(original)["sha256"] != original_hash:
        raise ValueError("entity population instrumentation altered the original ROM")
    DERIVED_ROOT.mkdir(parents=True, exist_ok=True)
    output = DERIVED_ROOT / "entity-population-reload.instrumented.bin"
    output.write_bytes(data)
    return output


def _with_instrumented_rom_database(
    instrumented_rom: Path, name: str, action: Any
) -> dict[str, Any]:
    """Register the session checksum with BizHawk and restore its user database exactly."""
    _, executable = bizhawk_contract()
    user_db = executable.parent / "gamedb" / "gamedb_user.txt"
    prior = user_db.read_bytes() if user_db.exists() else None
    prior_text = prior.decode("utf-8") if prior is not None else ""
    separator = "" if not prior_text or prior_text.endswith("\n") else "\n"
    md5 = hashlib.md5(instrumented_rom.read_bytes()).hexdigest().upper()
    user_db.write_text(f"{prior_text}{separator}{md5}\t\t{name}\tGEN\n", encoding="utf-8")
    try:
        return action()
    finally:
        if prior is None:
            user_db.unlink(missing_ok=True)
        else:
            user_db.write_bytes(prior)


def _entity_field_layouts(static: dict[str, Any]) -> dict[str, dict[str, int]]:
    fields = static["constants"]["entityFieldOffsets"]
    return {
        "xWord": {"byteOffset": fields["xWord"], "transferByteCount": 2},
        "yWord": {"byteOffset": fields["yWord"], "transferByteCount": 2},
        "xDestWord": {"byteOffset": fields["xDestWord"], "transferByteCount": 2},
        "facingByte": {"byteOffset": fields["facingByte"], "transferByteCount": 1},
        "entityNumberByte": {
            "byteOffset": fields["entityNumberByte"],
            "transferByteCount": 1,
        },
        "mapspriteByte": {
            "byteOffset": fields["mapspriteByte"],
            "transferByteCount": 1,
        },
    }


def _source_input_values(static: dict[str, Any], case: dict[str, Any]) -> list[int]:
    """Resolve fixture source-site identity to one H2-recorded operand sequence."""
    macro = case["macro"]
    key = case["sourceOrderKey"]
    facts = static["sourceFacts"]
    if macro == "newEntity":
        rows = facts["newEntityInputSourceSites"]
        match = next((row for row in rows if row["sourceOrderKey"] == key), None)
        if match is None:
            raise ValueError(f"entity population new-entity source key drift: {key}")
        return match["inputValues"]
    if macro == "loadMapEntities":
        row = facts["directTableSourceSite"]
        if row["sourceOrderKey"] != key:
            raise ValueError(f"entity population direct-table source key drift: {key}")
        return [row["tableAddress"]]
    if macro == "reloadEntities":
        row = facts["reloadSourceSite"]
        if row["sourceOrderKey"] != key:
            raise ValueError(f"entity population reload source key drift: {key}")
        return [row["tableAddress"]]
    if macro == "loadEntitiesFromMapSetup":
        rows = facts["mapSetupInputSourceSites"]
        match = next((row for row in rows if row["sourceOrderKey"] == key), None)
        if match is None:
            raise ValueError(f"entity population map-setup source key drift: {key}")
        return match["inputWords"]
    raise ValueError(f"entity population unsupported runtime macro: {macro}")


def _expand_index_list_seeds(
    case: dict[str, Any], constants: dict[str, Any]
) -> list[dict[str, int]]:
    """Expand compact fixture ranges while rejecting duplicate identity-list writes."""
    records = list(case["indexListSeedRecords"])
    for item in case["indexListSeedRanges"]:
        start = item["startOffset"]
        count = item["count"]
        value = item["value"]
        if count < 1 or start < 0 or start + count > constants["allocationScanItemCount"]:
            raise ValueError(f"entity population index-list range boundary drift: {case['id']}")
        records.extend({"offset": start + index, "value": value} for index in range(count))
    if any(
        not isinstance(item["offset"], int)
        or not isinstance(item["value"], int)
        or not 0 <= item["offset"] < constants["allocationScanItemCount"]
        or not 0 <= item["value"] <= 0xFF
        for item in records
    ):
        raise ValueError(f"entity population index-list seed boundary drift: {case['id']}")
    if len({item["offset"] for item in records}) != len(records):
        raise ValueError(f"entity population duplicate index-list seed: {case['id']}")
    return sorted(records, key=lambda item: item["offset"])


def _current_map_value(static: dict[str, Any], case: dict[str, Any]) -> int:
    mode = case["currentMapMode"]
    if mode == "map-tactical-base":
        return static["constants"]["mapTacticalBase"]
    if mode == "map17-default":
        return static["sourceFacts"]["mapSetupDefaultEntityTable"]["currentMap"]
    raise ValueError(f"entity population current-map mode drift: {mode}")


def _case_inputs(static: dict[str, Any], fixture: dict[str, Any]) -> list[dict[str, Any]]:
    """Build observer-only setup from source identities and mutable case seeds."""
    handlers = {row["macro"]: row for row in static["sourceFacts"]["handlers"]}
    constants = static["constants"]
    field_layouts = _entity_field_layouts(static)
    result = []
    for case in fixture["cases"]:
        handler = handlers.get(case["macro"])
        if handler is None:
            raise ValueError(f"entity population unknown case macro: {case['id']}")
        values = _source_input_values(static, case)
        cursor = handler["scriptCursorUseSites"]
        if len(values) != len(cursor):
            raise ValueError(f"entity population source input arity drift: {case['id']}")
        script_writes = [
            {
                "byteOffset": record["scriptInputByteOffset"],
                "transferredByteCount": record["transferredByteCount"],
                "value": value,
            }
            for record, value in zip(cursor, values, strict=True)
        ]
        seed_records = _expand_index_list_seeds(case, constants)
        entity_slot_seeds = []
        for seed in case["entitySlotSeedRecords"]:
            slot_index = seed["slotIndex"]
            if (
                not isinstance(slot_index, int)
                or not 0 <= slot_index < constants["clearRecordCount"]
            ):
                raise ValueError(f"entity population entity seed slot boundary drift: {case['id']}")
            names = [field["field"] for field in seed["fields"]]
            if len(set(names)) != len(names) or any(name not in field_layouts for name in names):
                raise ValueError(
                    f"entity population entity seed field identity drift: {case['id']}"
                )
            field_writes = []
            for field in seed["fields"]:
                layout = field_layouts[field["field"]]
                width = layout["transferByteCount"]
                value = field["value"]
                if not isinstance(value, int) or not 0 <= value < 1 << (width * 8):
                    raise ValueError(
                        f"entity population entity seed value boundary drift: {case['id']}"
                    )
                field_writes.append({**layout, "value": value})
            entity_slot_seeds.append({"slotIndex": slot_index, "fields": field_writes})
        if case["macro"] == "newEntity":
            identity_offset = _entity_index_list_offset(values[0], constants)
            high_water = max((item["value"] for item in seed_records), default=0)
            allocation = high_water + 1
            if case["indexReadOffsets"] != [identity_offset] or case["entitySlotReadIndices"] != [
                allocation
            ]:
                raise ValueError(
                    f"entity population new-entity observation boundary drift: {case['id']}"
                )
        result.append(
            {
                "id": case["id"],
                "macro": case["macro"],
                "handlerAddress": handler["handlerAddress"],
                "currentMap": _current_map_value(static, case),
                "clearGameFlags": case["clearGameFlags"],
                "indexListSeeds": seed_records,
                "entitySlotSeeds": entity_slot_seeds,
                "scriptInputWrites": script_writes,
                "indexReadOffsets": case["indexReadOffsets"],
                "entitySlotReadIndices": case["entitySlotReadIndices"],
            }
        )
    return result


def _callback_orders(static: dict[str, Any]) -> dict[str, list[str]]:
    return {
        handler["macro"]: [callback["instructionTarget"] for callback in handler["callbacks"]]
        for handler in static["sourceFacts"]["handlers"]
    }


def _expected_observation(fixture: dict[str, Any]) -> dict[str, Any]:
    """Project the fixture-owned runtime result without giving it to Lua."""
    golden = fixture["runtimeGolden"]
    return {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["id"],
        "mapTest": fixture["mapTestIndex"],
        "recordOrder": golden["recordOrder"],
        "records": golden["records"],
    }


def build_entity_population_reload_contract(
    rom_path: Path, upstream_path: Path
) -> dict[str, Any]:
    """Public H3 static contract entry point for the entity-population matrix."""
    return build_entity_population_reload_static_contract(rom_path, upstream_path)


def verify_entity_population_reload(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 180
) -> dict[str, Any]:
    """Run all four population/reload handler families in one BizHawk launch."""
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner="entity population reload runtime fixture")
    verify_runtime_contract(fixture, rom_path)
    static = build_entity_population_reload_contract(rom_path, upstream_path)
    for key in ("function", "ram", "constants", "sourceFacts"):
        if fixture[key] != static[key]:
            raise ValueError(f"entity population static contract drift: {key}")
    case_inputs = _case_inputs(static, fixture)
    instrumented_rom = _instrument_rom(rom_path, fixture["instrumentation"])

    def observe() -> dict[str, Any]:
        return run_observer(
            rom_path=instrumented_rom,
            observer_path=OBSERVER,
            config={
                "fixtureId": fixture["id"],
                "mapTestIndex": fixture["mapTestIndex"],
                "function": static["function"],
                "ram": static["ram"],
                "constants": {
                    **static["constants"],
                    "entityFieldLayouts": _entity_field_layouts(static),
                },
                "instrumentation": fixture["instrumentation"],
                "handlers": static["sourceFacts"]["handlers"],
                "callbackOrdersByMacro": _callback_orders(static),
                "cases": case_inputs,
                "maxFrames": fixture["maxFrames"],
                "jsonModulePath": OBSERVER.with_name("json.lua").as_posix(),
                "harness": load_json(repo_path(fixture["sharedHarnessFixture"]))["harness"],
            },
            output_name="entity-population-reload",
            timeout_seconds=timeout_seconds,
        )

    observed = _with_instrumented_rom_database(
        instrumented_rom, "SF2 H3 instrumented entity population reload", observe
    )
    validate_json(
        observed, OBSERVATION_SCHEMA, owner="entity population reload runtime observation"
    )
    expected = _expected_observation(fixture)
    if observed != expected:
        raise ValueError(
            "entity population reload runtime matrix mismatch\n"
            f"expected={expected!r}\nobserved={observed!r}"
        )
    return {
        "Fixture": fixture["id"],
        "Cases": len(case_inputs),
        "Handlers": len({row["macro"] for row in case_inputs}),
        "MapSetupInputPartitions": len(
            static["sourceFacts"]["mapSetupInputSourceSites"]
        ),
        "BizHawkLaunches": 1,
        "Instrumentation": "session-only",
        "Status": "PASS",
    }
