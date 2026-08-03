from __future__ import annotations

import hashlib
import json
import re
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.h2.entity_action_scripts import build_entity_action_script_contract
from sf2tool.h2.map_entities import build_map_entities_contract
from sf2tool.h2.map_events_fixture import (
    OUTPUT_SCHEMA,
    load_map_events_fixture,
)
from sf2tool.h2.map_script_engine import build_map_script_engine_contract
from sf2tool.h2.map_setup import build_map_setup_contract
from sf2tool.h2.sound_data import ID as SOUND_DATA_ID
from sf2tool.h2.sound_data import build_sound_data_inventory
from sf2tool.h2.text_banks import (
    GAMESCRIPT_PATH,
    build_text_line_domain_contract,
)
from sf2tool.h2.text_banks import (
    ID as TEXT_BANKS_ID,
)
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.source_text import read_upstream_text

ID = "sf2-map-events-static-v1"
SOURCE_ROOT = Path("data/maps/entries")
MAP_SETUP_MACROS_PATH = Path("sf2mapsetupmacros.asm")
SERVICE_MACROS_PATH = Path("sf2macros.asm")
CUTSCENE_MACROS_PATH = Path("sf2cutscenemacros.asm")
MANIFEST = repo_path("manifests/extractions/map-events-static.json")
SCHEMA = OUTPUT_SCHEMA
SOUND_ENUM_PATH = Path("sf2enums.asm")
SOUND_COMMAND_CATEGORIES = ("music", "sfx", "sound-command")

CATEGORY_CONFIG = {
    "entityEvents": {
        "glob": "s2_entityevents*.asm",
        "recordBytes": 4,
        "specificMacros": ("msEntityEvent",),
        "defaultMacros": ("msDefaultEntityEvent", "msDftEntityEvent"),
        "stubSymbols": ("ms_map52_EntityEvents", "ms_map55_EntityEvents"),
    },
    "zoneEvents": {
        "glob": "s3_zoneevents*.asm",
        "recordBytes": 4,
        "specificMacros": ("msZoneEvent",),
        "defaultMacros": ("msDefaultZoneEvent",),
        "stubSymbols": (),
    },
    "itemEvents": {
        "glob": "s5_itemevents*.asm",
        "recordBytes": 6,
        "specificMacros": ("msItemEvent",),
        "defaultMacros": ("msDefaultItemEvent",),
        "stubSymbols": (),
    },
}

RAW_ZONE_DEFAULT_SYMBOL = "ms_map44_ZoneEvents"
FUNCTION_SYMBOLS = (
    "RunMapSetupEntityEvent",
    "RunMapSetupZoneEvent",
    "RunMapSetupItemEvent",
)
REACHABILITY_FUNCTION_SYMBOLS = (
    "ProcessPlayerAction",
    "GetActivatedEntity",
    "GetEntityEventIndex",
)
SELECTION_INPUTS = (
    ("entity-specific-after-scan", "entityEvents", 3, (), {"entity": 128}),
    ("entity-default", "entityEvents", 3, (), {"entity": 135}),
    ("zone-exact", "zoneEvents", 3, (), {"x": 27, "y": 5}),
    ("zone-wildcard-y", "zoneEvents", 3, (), {"x": 2, "y": 42}),
    ("zone-first-overlapping-match", "zoneEvents", 3, (609,), {"x": 2, "y": 23}),
    ("zone-default", "zoneEvents", 3, (), {"x": 10, "y": 10}),
    (
        "item-index-mask",
        "itemEvents",
        8,
        (),
        {"x": 15, "y": 19, "facing": 1, "item": 240},
    ),
    (
        "item-facing-mismatch-default",
        "itemEvents",
        8,
        (),
        {"x": 15, "y": 19, "facing": 2, "item": 112},
    ),
    (
        "item-wildcard-facing",
        "itemEvents",
        22,
        (),
        {"x": 35, "y": 24, "facing": 3, "item": 125},
    ),
)

_PROGRAM_LABEL = re.compile(r"^\s*([A-Za-z_][A-Za-z0-9_]*):\s*(.*)$")
_PROGRAM_OPERATION = re.compile(
    r"^(?P<mnemonic>[A-Za-z][A-Za-z0-9_]*)(?P<suffix>\.[bBwWlLsS])?"
    r"(?:\s+(?P<operands>.+))?$"
)
_PROGRAM_END = re.compile(r"^\s*;\s*End of function ([A-Za-z_][A-Za-z0-9_]*)\s*$")
_LISTING_LINE = re.compile(r"^([0-9A-Fa-f]{8})(.*)$")
_PARENTHESIZED_TARGET = re.compile(r"^\(([A-Za-z_][A-Za-z0-9_]*)\)\.[bBwWlL]$")
_PLAIN_TARGET = re.compile(r"^[A-Za-z_][A-Za-z0-9_]*$")
_PC_RELATIVE_TARGET = re.compile(r"^([A-Za-z_][A-Za-z0-9_]*)\([pP][cC]\)$")

_CONTROL_FLOW_KINDS = {
    "beq": "conditional-branch",
    "bne": "conditional-branch",
    "bra": "unconditional-branch",
    "bsr": "direct-call",
    "jsr": "direct-call",
    "jmp": "direct-jump",
    "rts": "return",
}
_CONTROL_FLOW_COUNT_FIELDS = (
    "conditionalBranchSiteCount",
    "unconditionalBranchSiteCount",
    "directCallSiteCount",
    "directJumpSiteCount",
)
_DIRECT_FLAG_ACCESS_KINDS = ("read", "set", "clear")
_DIRECT_FLAG_CONSUMER_TARGET_FIELDS = (
    "instructionTargetSymbol",
    "instructionTargetAddress",
    "instructionTargetAddressLabels",
    "effectiveTargetSymbol",
    "effectiveTargetAddress",
    "effectiveTargetAddressLabels",
    "effectiveTargetScope",
)
_BASIC_BLOCK_TRANSFER_MNEMONICS = frozenset(
    {
        "bra",
        "bsr",
        "bhi",
        "bls",
        "bcc",
        "bhs",
        "bcs",
        "blo",
        "bne",
        "beq",
        "bvc",
        "bvs",
        "bpl",
        "bmi",
        "bge",
        "blt",
        "bgt",
        "ble",
        "dbf",
        "dbeq",
        "dbne",
        "dbcc",
        "dbcs",
        "dbhi",
        "dbls",
        "dbpl",
        "dbmi",
        "dbvc",
        "dbvs",
        "dbge",
        "dblt",
        "dbgt",
        "dble",
        "jmp",
        "jsr",
        "rte",
        "rtr",
        "rts",
        "trap",
    }
)
def _normalise_asm_statement(value: str) -> str:
    """Compare source and H1 statements without treating comments as code."""
    statement = re.sub(r"\s+", " ", value.split(";", 1)[0].strip())
    return re.sub(r"\s*,\s*", ",", statement)


def _basic_block_boundary(statement: str) -> bool:
    """Recognize a label or control transfer without treating operand text as code."""
    if _PROGRAM_LABEL.fullmatch(statement) is not None:
        return True
    token = statement.split(" ", 1)[0].lower().split(".", 1)[0]
    return token in _BASIC_BLOCK_TRANSFER_MNEMONICS


def _listing_statement(raw_line: str) -> tuple[int, str] | None:
    """Return one H1 address/source statement, excluding macro-expansion rows."""
    line_match = _LISTING_LINE.match(raw_line)
    if line_match is None:
        return None
    address = int(line_match.group(1), 16)
    remainder = line_match.group(2)
    byte_match = re.match(
        r"^\s*(?:[0-9A-Fa-f]{4}|[0-9A-Fa-f]{2})"
        r"(?:\s+(?:[0-9A-Fa-f]{4}|[0-9A-Fa-f]{2}))*\s{2,}(.*)$",
        remainder,
    )
    text = byte_match.group(1) if byte_match is not None else remainder
    statement = _normalise_asm_statement(text)
    if statement.startswith("M "):
        return None
    return address, statement


def _operation_target_symbol(operand_texts: list[str], source_line: int) -> str:
    if len(operand_texts) != 1:
        raise ValueError(
            f"map entity-event control-flow operand drift at source line {source_line}"
        )
    operand = operand_texts[0]
    plain_match = _PLAIN_TARGET.fullmatch(operand)
    if plain_match is not None:
        return operand
    parenthesized_match = _PARENTHESIZED_TARGET.fullmatch(operand)
    if parenthesized_match is not None:
        return parenthesized_match.group(1)
    pc_relative_match = _PC_RELATIVE_TARGET.fullmatch(operand)
    if pc_relative_match is not None:
        return pc_relative_match.group(1)
    raise ValueError(
        f"map entity-event control-flow target form drift at source line {source_line}"
    )


def _parse_program_operation(
    statement: str, *, source_line: int, source_order: int
) -> dict[str, Any]:
    """Parse one source operation while retaining its raw mnemonic and operands."""
    match = _PROGRAM_OPERATION.fullmatch(statement)
    if match is None:
        raise ValueError(f"map entity-event operation syntax drift at source line {source_line}")
    raw_mnemonic = match.group("mnemonic") + (match.group("suffix") or "")
    operand_text = match.group("operands")
    operand_texts = _split_macro_operands(operand_text) if operand_text else []
    mnemonic = match.group("mnemonic").lower()
    control_flow_kind = _CONTROL_FLOW_KINDS.get(mnemonic, "ordinary")
    target_symbol = (
        _operation_target_symbol(operand_texts, source_line)
        if control_flow_kind
        in {"conditional-branch", "unconditional-branch", "direct-call", "direct-jump"}
        else None
    )
    return {
        "sourceOrder": source_order,
        "sourceLine": source_line,
        "sourceMnemonic": raw_mnemonic,
        "mnemonic": mnemonic,
        "sizeSuffix": match.group("suffix").lower() if match.group("suffix") else None,
        "operandTexts": operand_texts,
        "controlFlowKind": control_flow_kind,
        "instructionTargetSymbol": target_symbol,
    }


def _parse_direct_flag_operand(operand_texts: list[str], *, source_line: int) -> int:
    """Parse the one numeric operand emitted by a direct flag-service macro."""
    if len(operand_texts) != 1:
        raise ValueError(f"map event direct flag operand count drift at source line {source_line}")
    operand = operand_texts[0]
    if re.fullmatch(r"(?:0|[1-9][0-9]*|\$[0-9A-Fa-f]+)", operand) is None:
        raise ValueError(f"map event direct flag operand syntax drift at source line {source_line}")
    return int(operand[1:], 16) if operand.startswith("$") else int(operand)


def _direct_flag_access_sites_for_program(
    category: str,
    program: dict[str, Any],
    service_accesses: dict[str, dict[str, Any]],
) -> list[dict[str, Any]]:
    """Derive direct flag sites and immediate ``chkFlg`` branch consumers for one program."""
    reference_counts = program["referenceCounts"]
    if set(reference_counts) != {
        "physicalRecordCount",
        "setupRecordReferenceCount",
        "routeRecordReferenceCount",
    } or any(not isinstance(value, int) or value < 0 for value in reference_counts.values()):
        raise ValueError("map event direct flag program reference-count shape drift")

    sites: list[dict[str, Any]] = []
    operations = program["operations"]
    for operation_index, operation in enumerate(operations):
        definition_id = operation["definitionId"]
        service = service_accesses.get(definition_id)
        if service is None:
            continue
        if operation["sourceOrder"] != operation_index:
            raise ValueError(
                "map event direct flag operation order drift: "
                f"{program['canonicalSymbol']}:{operation['sourceLine']}"
            )
        if (
            operation["sourceMnemonic"] != service["sourceMacro"]
            or operation["family"] != "event-service-macro"
            or service["accessKind"] not in _DIRECT_FLAG_ACCESS_KINDS
        ):
            raise ValueError(
                "map event direct flag service-definition join drift: "
                f"{program['canonicalSymbol']}:{operation['sourceLine']}"
            )
        flag_number = _parse_direct_flag_operand(
            operation["operandTexts"], source_line=operation["sourceLine"]
        )
        condition_consumer: dict[str, Any] | None = None
        if service["accessKind"] == "read":
            if operation_index + 1 >= len(operations):
                raise ValueError(
                    "map event direct flag read lacks an immediate condition consumer: "
                    f"{program['canonicalSymbol']}:{operation['sourceLine']}"
                )
            consumer = operations[operation_index + 1]
            if (
                consumer["sourceOrder"] != operation["sourceOrder"] + 1
                or consumer["controlFlowKind"] != "conditional-branch"
                or consumer["mnemonic"] not in {"beq", "bne"}
            ):
                raise ValueError(
                    "map event direct flag read consumer relationship drift: "
                    f"{program['canonicalSymbol']}:{operation['sourceLine']}"
                )
            target = consumer["target"]
            if not isinstance(target, dict) or tuple(target) != _DIRECT_FLAG_CONSUMER_TARGET_FIELDS:
                raise ValueError(
                    "map event direct flag read consumer target identity drift: "
                    f"{program['canonicalSymbol']}:{consumer['sourceLine']}"
                )
            condition_consumer = {
                "relation": "immediate-next-operation",
                "operationSourceOrder": consumer["sourceOrder"],
                "sourceLine": consumer["sourceLine"],
                "address": consumer["address"],
                "sourceMnemonic": consumer["sourceMnemonic"],
                "mnemonic": consumer["mnemonic"],
                "sizeSuffix": consumer["sizeSuffix"],
                "operandTexts": consumer["operandTexts"],
                "branchPolarity": "equal" if consumer["mnemonic"] == "beq" else "not-equal",
                "target": target,
            }
        sites.append(
            {
                "category": category,
                "accessKind": service["accessKind"],
                "sourceMacro": operation["sourceMnemonic"],
                "definitionId": definition_id,
                "flagNumber": flag_number,
                "flagOperandText": operation["operandTexts"][0],
                "programCanonicalSymbol": program["canonicalSymbol"],
                "programEntryAddress": program["entryAddress"],
                "programOrder": program["programOrder"],
                "sourcePath": program["sourcePath"],
                "operationSourceOrder": operation["sourceOrder"],
                "sourceLine": operation["sourceLine"],
                "address": operation["address"],
                "referenceWeights": {
                    "physicalRecordCount": reference_counts["physicalRecordCount"],
                    "setupRecordReferenceCount": reference_counts[
                        "setupRecordReferenceCount"
                    ],
                    "routeRecordReferenceCount": reference_counts[
                        "routeRecordReferenceCount"
                    ],
                },
                "conditionConsumer": condition_consumer,
            }
        )
    return sites


def _direct_flag_service_definitions(
    operation_definitions: list[dict[str, Any]],
) -> list[dict[str, Any]]:
    """Derive direct flag service identities from the parsed macro definitions once."""
    access_kinds = {"CHECK": "read", "SET": "set", "CLEAR": "clear"}
    definitions: list[dict[str, Any]] = []
    for definition in operation_definitions:
        if definition["family"] != "event-service-macro":
            continue
        service_target = definition["serviceTarget"]
        if not isinstance(service_target, str):
            raise ValueError(
                "map event direct flag service target type drift: "
                f"{definition['sourceMacro']}"
            )
        target_match = re.fullmatch(r"#(?P<action>[A-Z]+)_FLAG", service_target)
        if target_match is None:
            continue
        action = target_match.group("action")
        access_kind = access_kinds.get(action)
        if access_kind is None:
            raise ValueError(
                "map event direct flag service action identity drift: "
                f"{definition['sourceMacro']}"
            )
        templates = definition["emissionStatementTemplates"]
        if (
            definition["formalParameterOrdinals"] != [1]
            or templates != [f"trap {service_target.lower()}", "dc.w \\1"]
        ):
            raise ValueError(
                "map event direct flag service emission/order drift: "
                f"{definition['sourceMacro']}"
            )
        definitions.append(
            {
                "accessKind": access_kind,
                "sourceMacro": definition["sourceMacro"],
                "definitionId": definition["definitionId"],
                "sourcePath": definition["sourcePath"],
                "definitionSourceLine": definition["definitionSourceLine"],
                "trapOperand": service_target,
                "flagOperandOrdinal": definition["formalParameterOrdinals"][0],
                "emissionStatementTemplates": templates,
            }
        )
    if tuple(definition["accessKind"] for definition in definitions) != _DIRECT_FLAG_ACCESS_KINDS:
        raise ValueError("map event direct flag service coverage/order drift")
    return definitions


def _direct_flag_weight_counts(reference_weights: dict[str, int]) -> dict[str, int]:
    """Keep program occurrence and the three independently joined reference weights separate."""
    required = {
        "physicalRecordCount",
        "setupRecordReferenceCount",
        "routeRecordReferenceCount",
    }
    if set(reference_weights) != required or any(
        not isinstance(value, int) or value < 0 for value in reference_weights.values()
    ):
        raise ValueError("map event direct flag reference-weight shape drift")
    return {
        "physicalProgramOccurrenceCount": 1,
        "physicalRecordWeightedSiteCount": reference_weights["physicalRecordCount"],
        "setupRecordReferenceWeightedSiteCount": reference_weights[
            "setupRecordReferenceCount"
        ],
        "routeRecordReferenceWeightedSiteCount": reference_weights[
            "routeRecordReferenceCount"
        ],
    }


def _empty_direct_flag_weight_counts() -> dict[str, int]:
    return {
        "physicalProgramOccurrenceCount": 0,
        "physicalRecordWeightedSiteCount": 0,
        "setupRecordReferenceWeightedSiteCount": 0,
        "routeRecordReferenceWeightedSiteCount": 0,
    }


def _add_direct_flag_weight_counts(
    destination: dict[str, int], source: dict[str, int]
) -> None:
    if set(destination) != set(source) or any(
        not isinstance(value, int) or value < 0 for value in source.values()
    ):
        raise ValueError("map event direct flag weight aggregation drift")
    for field, value in source.items():
        destination[field] += value


def _empty_direct_flag_access_kind_counts(
    access_kinds: tuple[str, ...],
) -> dict[str, dict[str, int]]:
    return {access_kind: _empty_direct_flag_weight_counts() for access_kind in access_kinds}


def _empty_direct_flag_category_access_kind_counts(
    categories: tuple[str, ...], access_kinds: tuple[str, ...]
) -> dict[str, dict[str, dict[str, int]]]:
    return {
        category: _empty_direct_flag_access_kind_counts(access_kinds)
        for category in categories
    }


def _direct_flag_access_sites(
    programs_by_category: dict[str, list[dict[str, Any]]],
    service_definitions: list[dict[str, Any]],
) -> list[dict[str, Any]]:
    """Attach complete program identity and global order to the per-program flag sites."""
    service_accesses = {
        definition["definitionId"]: definition for definition in service_definitions
    }
    if len(service_accesses) != len(service_definitions):
        raise ValueError("map event direct flag service-definition identity ambiguity")
    sites: list[dict[str, Any]] = []
    for category, programs in programs_by_category.items():
        for program in programs:
            program_key = _program_key(program["canonicalSymbol"], program["entryAddress"])
            for site in _direct_flag_access_sites_for_program(
                category, program, service_accesses
            ):
                site["siteOrder"] = len(sites)
                site["programKey"] = program_key
                sites.append(site)
    return sites


def _direct_flag_state_aggregates(
    sites: list[dict[str, Any]],
    programs_by_category: dict[str, list[dict[str, Any]]],
    access_kinds: tuple[str, ...],
) -> dict[str, Any]:
    """Rebuild every direct-flag total from ordered source-bound sites."""
    categories = tuple(programs_by_category)
    if not access_kinds or len(set(access_kinds)) != len(access_kinds):
        raise ValueError("map event direct flag access-kind coverage drift")
    category_counts = _empty_direct_flag_category_access_kind_counts(
        categories, access_kinds
    )
    program_sites: dict[tuple[str, str], list[dict[str, Any]]] = {
        (
            category,
            _program_key(program["canonicalSymbol"], program["entryAddress"]),
        ): []
        for category, programs in programs_by_category.items()
        for program in programs
    }
    flag_sites: dict[int, list[dict[str, Any]]] = {}
    consumer_source_mnemonics: Counter[str] = Counter()
    for site_order, site in enumerate(sites):
        if site["siteOrder"] != site_order:
            raise ValueError("map event direct flag site-order drift")
        category = site["category"]
        access_kind = site["accessKind"]
        if category not in category_counts or access_kind not in access_kinds:
            raise ValueError("map event direct flag category/access-kind drift")
        program_key = site["programKey"]
        program_sites_key = (category, program_key)
        if program_sites_key not in program_sites:
            raise ValueError("map event direct flag site program identity drift")
        weights = _direct_flag_weight_counts(site["referenceWeights"])
        _add_direct_flag_weight_counts(category_counts[category][access_kind], weights)
        program_sites[program_sites_key].append(site)
        flag_sites.setdefault(site["flagNumber"], []).append(site)
        consumer = site["conditionConsumer"]
        if access_kind == "read":
            if consumer is None:
                raise ValueError("map event direct flag read consumer absence drift")
            consumer_source_mnemonics[consumer["sourceMnemonic"]] += 1
        elif consumer is not None:
            raise ValueError("map event direct flag write consumer presence drift")

    total_access_kind_counts = _empty_direct_flag_access_kind_counts(access_kinds)
    for category in categories:
        for access_kind in access_kinds:
            _add_direct_flag_weight_counts(
                total_access_kind_counts[access_kind],
                category_counts[category][access_kind],
            )

    program_totals: list[dict[str, Any]] = []
    for category, programs in programs_by_category.items():
        for program in programs:
            program_key = _program_key(program["canonicalSymbol"], program["entryAddress"])
            contained_sites = program_sites[(category, program_key)]
            access_counts = _empty_direct_flag_access_kind_counts(access_kinds)
            for site in contained_sites:
                _add_direct_flag_weight_counts(
                    access_counts[site["accessKind"]],
                    _direct_flag_weight_counts(site["referenceWeights"]),
                )
            program_totals.append(
                {
                    "category": category,
                    "programKey": program_key,
                    "programOrder": program["programOrder"],
                    "siteOrders": [site["siteOrder"] for site in contained_sites],
                    "accessKindCounts": access_counts,
                }
            )

    flag_totals: list[dict[str, Any]] = []
    for flag_number in sorted(flag_sites):
        contained_sites = flag_sites[flag_number]
        access_counts = _empty_direct_flag_access_kind_counts(access_kinds)
        category_access_counts = _empty_direct_flag_category_access_kind_counts(
            categories, access_kinds
        )
        for site in contained_sites:
            weights = _direct_flag_weight_counts(site["referenceWeights"])
            _add_direct_flag_weight_counts(access_counts[site["accessKind"]], weights)
            _add_direct_flag_weight_counts(
                category_access_counts[site["category"]][site["accessKind"]], weights
            )
        flag_totals.append(
            {
                "flagNumber": flag_number,
                "siteOrders": [site["siteOrder"] for site in contained_sites],
                "accessKindCounts": access_counts,
                "categoryAccessKindCounts": category_access_counts,
            }
        )

    read_flag_domain = sorted(
        {site["flagNumber"] for site in sites if site["accessKind"] == "read"}
    )
    write_flag_domain = sorted(
        {site["flagNumber"] for site in sites if site["accessKind"] != "read"}
    )
    return {
        "directFlagProgramTotals": program_totals,
        "directFlagTotals": flag_totals,
        "directFlagStateSummary": {
            "serviceDefinitionCount": len(access_kinds),
            "directFlagAccessSiteCount": len(sites),
            "observedFlagCount": len(flag_totals),
            "readFlagDomain": read_flag_domain,
            "writeFlagDomain": write_flag_domain,
            "readWriteOverlap": sorted(set(read_flag_domain) & set(write_flag_domain)),
            "accessKindCounts": total_access_kind_counts,
            "categoryAccessKindCounts": category_counts,
            "readConditionConsumerCounts": {
                "immediateConditionConsumerCount": total_access_kind_counts["read"][
                    "physicalProgramOccurrenceCount"
                ],
                "sourceMnemonicCounts": dict(sorted(consumer_source_mnemonics.items())),
                "missingImmediateOperationCount": 0,
                "nonConditionalImmediateOperationCount": 0,
                "nonAdjacentImmediateOperationCount": 0,
                "unrecognizedConditionalMnemonicCount": 0,
                "missingTargetIdentityCount": 0,
            },
        },
    }


def _reconcile_direct_flag_state_contract(
    direct_flag_contract: dict[str, Any],
    programs_by_category: dict[str, list[dict[str, Any]]],
) -> None:
    """Cross-check flag sites against their parsed program and reference-count use sites."""
    service_definitions = direct_flag_contract["directFlagServiceDefinitions"]
    access_kinds = tuple(definition["accessKind"] for definition in service_definitions)
    expected_sites = _direct_flag_access_sites(programs_by_category, service_definitions)
    if direct_flag_contract["directFlagAccessSites"] != expected_sites:
        raise ValueError("map event direct flag source/use-site reconciliation drift")
    expected_aggregates = _direct_flag_state_aggregates(
        expected_sites, programs_by_category, access_kinds
    )
    for field, expected in expected_aggregates.items():
        if direct_flag_contract[field] != expected:
            raise ValueError(f"map event direct flag {field} reconciliation drift")
    expected_orders = {
        "directFlagServiceDefinitionOrder": [
            "|".join(
                (
                    definition["accessKind"],
                    definition["sourceMacro"],
                    definition["definitionId"],
                    definition["trapOperand"],
                )
            )
            for definition in service_definitions
        ],
        "directFlagAccessSiteOrder": [
            str(site["siteOrder"]) for site in expected_sites
        ],
        "directFlagProgramTotalOrder": [
            f"{tuple(programs_by_category).index(row['category'])}:{row['programOrder']}"
            for row in expected_aggregates["directFlagProgramTotals"]
        ],
        "directFlagTotalOrder": [
            str(row["flagNumber"]) for row in expected_aggregates["directFlagTotals"]
        ],
    }
    for field, expected in expected_orders.items():
        if direct_flag_contract[field] != expected:
            raise ValueError(f"map event direct flag {field} reconciliation drift")


def _direct_flag_state_contract(
    operation_definitions: list[dict[str, Any]],
    programs_by_category: dict[str, list[dict[str, Any]]],
) -> dict[str, Any]:
    """Build the complete direct numeric flag access surface from parsed source use sites."""
    service_definitions = _direct_flag_service_definitions(operation_definitions)
    access_kinds = tuple(definition["accessKind"] for definition in service_definitions)
    sites = _direct_flag_access_sites(programs_by_category, service_definitions)
    aggregates = _direct_flag_state_aggregates(sites, programs_by_category, access_kinds)
    contract = {
        "directFlagServiceDefinitions": service_definitions,
        "directFlagServiceDefinitionOrder": [],
        "directFlagAccessSites": sites,
        "directFlagAccessSiteOrder": [],
        **aggregates,
        "directFlagProgramTotalOrder": [],
        "directFlagTotalOrder": [],
    }
    contract["directFlagServiceDefinitionOrder"] = [
        "|".join(
            (
                definition["accessKind"],
                definition["sourceMacro"],
                definition["definitionId"],
                definition["trapOperand"],
            )
        )
        for definition in service_definitions
    ]
    contract["directFlagAccessSiteOrder"] = [
        str(site["siteOrder"]) for site in sites
    ]
    contract["directFlagProgramTotalOrder"] = [
        f"{tuple(programs_by_category).index(row['category'])}:{row['programOrder']}"
        for row in contract["directFlagProgramTotals"]
    ]
    contract["directFlagTotalOrder"] = [
        str(row["flagNumber"]) for row in contract["directFlagTotals"]
    ]
    _reconcile_direct_flag_state_contract(contract, programs_by_category)
    return contract


def _script_invocation_service_definition(
    operation_definitions: list[dict[str, Any]],
) -> dict[str, Any]:
    """Derive the one direct map-script service definition from parsed macro evidence."""
    definitions = [
        definition
        for definition in operation_definitions
        if definition["family"] == "event-service-macro"
        and definition["serviceTarget"] == "#MAPSCRIPT"
    ]
    if len(definitions) != 1:
        raise ValueError("map event script-invocation service-definition coverage drift")
    definition = definitions[0]
    service_target = definition["serviceTarget"]
    if (
        definition["formalParameterOrdinals"] != [1]
        or definition["emissionStatementTemplates"]
        != ["lea \\1(pc),a0", f"trap {service_target.lower()}"]
    ):
        raise ValueError("map event script-invocation service emission/order drift")
    return {
        "definitionId": definition["definitionId"],
        "sourceMacro": definition["sourceMacro"],
        "sourcePath": definition["sourcePath"],
        "definitionSourceLine": definition["definitionSourceLine"],
        "serviceTarget": service_target,
        "targetOperandOrdinal": definition["formalParameterOrdinals"][0],
        "emissionStatementTemplates": definition["emissionStatementTemplates"],
    }


def _script_invocation_weight_counts(reference_counts: dict[str, int]) -> dict[str, int]:
    """Keep source-program and joined record/reference multiplicities distinct."""
    required = {
        "physicalRecordCount",
        "setupRecordReferenceCount",
        "routeRecordReferenceCount",
    }
    if set(reference_counts) != required or any(
        not isinstance(value, int) or value < 0 for value in reference_counts.values()
    ):
        raise ValueError("map event script-invocation caller reference-weight shape drift")
    return {
        "physicalProgramOccurrenceCount": 1,
        "physicalRecordWeightedSiteCount": reference_counts["physicalRecordCount"],
        "setupRecordReferenceWeightedSiteCount": reference_counts[
            "setupRecordReferenceCount"
        ],
        "routeRecordReferenceWeightedSiteCount": reference_counts[
            "routeRecordReferenceCount"
        ],
    }


def _empty_script_invocation_weight_counts() -> dict[str, int]:
    return {
        "physicalProgramOccurrenceCount": 0,
        "physicalRecordWeightedSiteCount": 0,
        "setupRecordReferenceWeightedSiteCount": 0,
        "routeRecordReferenceWeightedSiteCount": 0,
    }


def _add_script_invocation_weight_counts(
    destination: dict[str, int], source: dict[str, int]
) -> None:
    if set(destination) != set(source) or any(
        not isinstance(value, int) or value < 0 for value in source.values()
    ):
        raise ValueError("map event script-invocation weight aggregation drift")
    for field, value in source.items():
        destination[field] += value


def _script_invocation_target_rows(
    program_corpus: dict[str, Any], addresses: dict[str, int]
) -> tuple[dict[str, dict[str, Any]], list[dict[str, Any]]]:
    """Join every declared map-script label to its source-owned program identity."""
    programs = program_corpus["programs"]
    label_owners = program_corpus["labelOwners"]
    programs_by_id = {program["id"]: program for program in programs}
    if len(programs_by_id) != len(programs):
        raise ValueError("map event script-invocation effective-owner identity ambiguity")
    expected_label_owners = {
        label: program["id"] for program in programs for label in program["labels"]
    }
    if label_owners != expected_label_owners:
        raise ValueError("map event script-invocation label-owner mapping drift")
    for program in programs:
        entry_label = program["entryLabel"]
        if entry_label is not None and program["address"] != addresses.get(entry_label):
            raise ValueError("map event script-invocation effective-owner H1 address drift")

    effective_rows = []
    for program_order, program in enumerate(programs):
        effective_rows.append(
            {
                "effectiveOwnerProgramId": program["id"],
                "effectiveOwnerProgramOrder": program_order,
                "effectiveOwnerEntryLabel": program["entryLabel"],
                "effectiveOwnerEntryAddress": program["address"],
                "effectiveOwnerSourcePath": program["sourcePath"],
                "effectiveOwnerTermination": program["termination"],
            }
        )
    effective_by_id = {
        row["effectiveOwnerProgramId"]: row for row in effective_rows
    }
    instruction_by_label = {
        label: {
            "instructionTargetLabel": label,
            "instructionTargetAddress": addresses.get(label),
            **effective_by_id[owner_id],
        }
        for label, owner_id in sorted(label_owners.items())
    }
    return instruction_by_label, effective_rows


def _script_invocation_graph_contract(
    operation_definitions: list[dict[str, Any]],
    programs_by_category: dict[str, list[dict[str, Any]]],
    program_corpus: dict[str, Any],
    addresses: dict[str, int],
) -> dict[str, Any]:
    """Build source/H1 script invocation joins and zero-inclusive caller/target totals."""
    service_definition = _script_invocation_service_definition(operation_definitions)
    instruction_by_label, effective_rows = _script_invocation_target_rows(
        program_corpus, addresses
    )
    categories = tuple(programs_by_category)
    caller_sites: dict[tuple[str, str], list[dict[str, Any]]] = {
        (category, _program_key(program["canonicalSymbol"], program["entryAddress"])): []
        for category, programs in programs_by_category.items()
        for program in programs
    }
    instruction_sites = {label: [] for label in instruction_by_label}
    effective_sites = {
        row["effectiveOwnerProgramId"]: [] for row in effective_rows
    }
    sites: list[dict[str, Any]] = []
    for category, programs in programs_by_category.items():
        for program in programs:
            caller_key = _program_key(program["canonicalSymbol"], program["entryAddress"])
            site_weights = _script_invocation_weight_counts(program["referenceCounts"])
            for operation_index, operation in enumerate(program["operations"]):
                if operation["definitionId"] != service_definition["definitionId"]:
                    continue
                if (
                    operation["sourceMnemonic"] != service_definition["sourceMacro"]
                    or operation["family"] != "event-service-macro"
                    or operation["sourceOrder"] != operation_index
                    or len(operation["operandTexts"]) != 1
                ):
                    raise ValueError("map event script-invocation source/use-site drift")
                raw_operand = operation["operandTexts"][0]
                if re.fullmatch(r"[A-Za-z_][A-Za-z0-9_]*", raw_operand) is None:
                    raise ValueError("map event script-invocation operand syntax drift")
                target = instruction_by_label.get(raw_operand)
                if target is None:
                    raise ValueError("map event script-invocation label-owner coverage drift")
                if target["instructionTargetAddress"] is None:
                    raise ValueError("map event script-invocation target H1 address drift")
                site = {
                    "siteOrder": len(sites),
                    "category": category,
                    "sourceMacro": operation["sourceMnemonic"],
                    "definitionId": operation["definitionId"],
                    "callerProgramKey": caller_key,
                    "callerProgramCanonicalSymbol": program["canonicalSymbol"],
                    "callerProgramEntryAddress": program["entryAddress"],
                    "callerProgramOrder": program["programOrder"],
                    "callerSourcePath": program["sourcePath"],
                    "operationSourceOrder": operation["sourceOrder"],
                    "sourceLine": operation["sourceLine"],
                    "operationAddress": operation["address"],
                    "rawOperand": raw_operand,
                    **target,
                    "weightCounts": dict(site_weights),
                }
                sites.append(site)
                caller_sites[(category, caller_key)].append(site)
                instruction_sites[raw_operand].append(site)
                effective_sites[target["effectiveOwnerProgramId"]].append(site)

    def total_weights(contained_sites: list[dict[str, Any]]) -> dict[str, int]:
        weights = _empty_script_invocation_weight_counts()
        for site in contained_sites:
            _add_script_invocation_weight_counts(weights, site["weightCounts"])
        return weights

    def category_weights(contained_sites: list[dict[str, Any]]) -> dict[str, dict[str, int]]:
        totals = {
            category: _empty_script_invocation_weight_counts() for category in categories
        }
        for site in contained_sites:
            _add_script_invocation_weight_counts(totals[site["category"]], site["weightCounts"])
        return totals

    caller_totals = []
    for category, programs in programs_by_category.items():
        for program in programs:
            caller_key = _program_key(program["canonicalSymbol"], program["entryAddress"])
            contained_sites = caller_sites[(category, caller_key)]
            caller_totals.append(
                {
                    "category": category,
                    "callerProgramKey": caller_key,
                    "callerProgramOrder": program["programOrder"],
                    "siteOrders": [site["siteOrder"] for site in contained_sites],
                    "weightCounts": total_weights(contained_sites),
                }
            )
    instruction_totals = [
        {
            **target,
            "siteOrders": [site["siteOrder"] for site in instruction_sites[label]],
            "weightCounts": total_weights(instruction_sites[label]),
            "categoryWeightCounts": category_weights(instruction_sites[label]),
        }
        for label, target in instruction_by_label.items()
    ]
    effective_totals = [
        {
            **target,
            "siteOrders": [
                site["siteOrder"] for site in effective_sites[target["effectiveOwnerProgramId"]]
            ],
            "weightCounts": total_weights(effective_sites[target["effectiveOwnerProgramId"]]),
            "categoryWeightCounts": category_weights(
                effective_sites[target["effectiveOwnerProgramId"]]
            ),
        }
        for target in effective_rows
    ]
    all_weights = total_weights(sites)
    contract = {
        "scriptInvocationServiceDefinition": service_definition,
        "scriptInvocationSites": sites,
        "scriptInvocationCallerTotals": caller_totals,
        "scriptInvocationInstructionTargetTotals": instruction_totals,
        "scriptInvocationEffectiveTargetTotals": effective_totals,
        "scriptInvocationSummary": {
            "serviceDefinitionCount": 1,
            "siteCount": len(sites),
            "declaredInstructionTargetCount": len(instruction_totals),
            "observedInstructionTargetCount": sum(
                bool(row["siteOrders"]) for row in instruction_totals
            ),
            "declaredEffectiveTargetCount": len(effective_totals),
            "observedEffectiveTargetCount": sum(
                bool(row["siteOrders"]) for row in effective_totals
            ),
            "weightCounts": all_weights,
            "categoryWeightCounts": category_weights(sites),
        },
    }
    contract.update(
        {
            "scriptInvocationSiteOrder": [str(site["siteOrder"]) for site in sites],
            "scriptInvocationCallerTotalOrder": [
                f"{categories.index(row['category'])}:{row['callerProgramOrder']}"
                for row in caller_totals
            ],
            "scriptInvocationInstructionTargetTotalOrder": [
                row["instructionTargetLabel"] for row in instruction_totals
            ],
            "scriptInvocationEffectiveTargetTotalOrder": [
                f"{row['effectiveOwnerProgramOrder']}:{row['effectiveOwnerProgramId']}"
                for row in effective_totals
            ],
        }
    )
    return contract


def _reconcile_script_invocation_graph_contract(
    contract: dict[str, Any],
    operation_definitions: list[dict[str, Any]],
    programs_by_category: dict[str, list[dict[str, Any]]],
    program_corpus: dict[str, Any],
    addresses: dict[str, int],
) -> None:
    """Reject a source/H1 or owner-map graph replacement before fixture comparison."""
    expected = _script_invocation_graph_contract(
        operation_definitions, programs_by_category, program_corpus, addresses
    )
    for field, value in expected.items():
        if contract[field] != value:
            raise ValueError(f"map event script-invocation {field} reconciliation drift")


def _textbox_service_definitions(
    operation_definitions: list[dict[str, Any]],
) -> list[dict[str, Any]]:
    """Derive the two direct TEXTBOX source forms from parsed service definitions."""
    definitions = []
    for definition in operation_definitions:
        if (
            definition["family"] != "event-service-macro"
            or definition["serviceTarget"] != "#TEXTBOX"
        ):
            continue
        formal_ordinals = definition["formalParameterOrdinals"]
        templates = definition["emissionStatementTemplates"]
        if formal_ordinals == [1] and templates == ["trap #textbox", "dc.w \\1"]:
            source_kind = "line-reference"
            sentinel_encoding = None
        elif formal_ordinals == [] and templates == ["trap #textbox", "dc.w $ffff"]:
            source_kind = "close-sentinel"
            sentinel_encoding = "$FFFF"
        else:
            raise ValueError("map event textbox service emission/order drift")
        definitions.append(
            {
                "sourceKind": source_kind,
                "definitionId": definition["definitionId"],
                "sourceMacro": definition["sourceMacro"],
                "sourcePath": definition["sourcePath"],
                "definitionSourceLine": definition["definitionSourceLine"],
                "serviceTarget": definition["serviceTarget"],
                "formalParameterOrdinals": formal_ordinals,
                "emissionStatementTemplates": templates,
                "sentinelEncoding": sentinel_encoding,
            }
        )
    expected_kinds = ("line-reference", "close-sentinel")
    if tuple(definition["sourceKind"] for definition in definitions) != expected_kinds:
        raise ValueError("map event textbox service-definition coverage/order drift")
    return definitions


def _parse_textbox_line_operand(operand_texts: list[str], *, source_line: int) -> int:
    """Parse one direct TEXTBOX line-ID operand without assigning displayed meaning."""
    if len(operand_texts) != 1:
        raise ValueError(f"map event textbox line operand count drift at source line {source_line}")
    operand = operand_texts[0]
    if re.fullmatch(r"(?:0|[1-9][0-9]*|\$[0-9A-Fa-f]+)", operand) is None:
        raise ValueError(
            f"map event textbox line operand syntax drift at source line {source_line}"
        )
    return int(operand[1:], 16) if operand.startswith("$") else int(operand)


def _textbox_weight_counts(reference_counts: dict[str, int]) -> dict[str, int]:
    """Keep the event-record multiplicities owned by each direct TEXTBOX site."""
    required = {
        "physicalRecordCount",
        "setupRecordReferenceCount",
        "routeRecordReferenceCount",
    }
    if set(reference_counts) != required or any(
        not isinstance(value, int) or value < 0 for value in reference_counts.values()
    ):
        raise ValueError("map event textbox caller reference-weight shape drift")
    return {
        "physicalProgramOccurrenceCount": 1,
        "physicalRecordWeightedSiteCount": reference_counts["physicalRecordCount"],
        "setupRecordReferenceWeightedSiteCount": reference_counts[
            "setupRecordReferenceCount"
        ],
        "routeRecordReferenceWeightedSiteCount": reference_counts[
            "routeRecordReferenceCount"
        ],
    }


def _textbox_reference_sites(
    programs_by_category: dict[str, list[dict[str, Any]]],
    service_definitions: list[dict[str, Any]],
    declared_line_ids: set[int],
) -> list[dict[str, Any]]:
    """Build ordered source/H1 TEXTBOX sites against the declared text-line domain."""
    definitions_by_id = {
        definition["definitionId"]: definition for definition in service_definitions
    }
    if len(definitions_by_id) != len(service_definitions):
        raise ValueError("map event textbox service-definition identity ambiguity")
    sites: list[dict[str, Any]] = []
    for category, programs in programs_by_category.items():
        for program in programs:
            reference_counts = program["referenceCounts"]
            weights = _textbox_weight_counts(reference_counts)
            caller_key = _program_key(program["canonicalSymbol"], program["entryAddress"])
            for operation_index, operation in enumerate(program["operations"]):
                definition = definitions_by_id.get(operation["definitionId"])
                if definition is None:
                    continue
                if (
                    operation["sourceMnemonic"] != definition["sourceMacro"]
                    or operation["family"] != "event-service-macro"
                    or operation["sourceOrder"] != operation_index
                ):
                    raise ValueError("map event textbox source/use-site drift")
                line_id: int | None
                raw_operand: str | None
                if definition["sourceKind"] == "line-reference":
                    line_id = _parse_textbox_line_operand(
                        operation["operandTexts"], source_line=operation["sourceLine"]
                    )
                    if line_id not in declared_line_ids:
                        raise ValueError("map event textbox line-domain coverage drift")
                    raw_operand = operation["operandTexts"][0]
                else:
                    if operation["operandTexts"]:
                        raise ValueError("map event textbox close-sentinel operand drift")
                    line_id = None
                    raw_operand = None
                sites.append(
                    {
                        "siteOrder": len(sites),
                        "sourceKind": definition["sourceKind"],
                        "sourceMacro": operation["sourceMnemonic"],
                        "definitionId": operation["definitionId"],
                        "category": category,
                        "callerProgramKey": caller_key,
                        "callerProgramCanonicalSymbol": program["canonicalSymbol"],
                        "callerProgramEntryAddress": program["entryAddress"],
                        "callerProgramOrder": program["programOrder"],
                        "callerSourcePath": program["sourcePath"],
                        "operationSourceOrder": operation["sourceOrder"],
                        "sourceLine": operation["sourceLine"],
                        "operationAddress": operation["address"],
                        "rawOperand": raw_operand,
                        "lineId": line_id,
                        "sentinelEncoding": definition["sentinelEncoding"],
                        "weightCounts": dict(weights),
                    }
                )
    return sites


def _textbox_line_domain(
    text_banks: dict[str, Any], *, upstream_commit: str, rom_sha256: str
) -> tuple[dict[str, Any], set[int]]:
    """Join a source/ROM-derived text-bank parser result to the event-source domain."""
    upstream = text_banks.get("upstream")
    summary = text_banks.get("summary")
    gamescript = text_banks.get("gamescriptFacts")
    if (
        text_banks.get("id") != TEXT_BANKS_ID
        or not isinstance(upstream, dict)
        or upstream.get("commit") != upstream_commit
        or text_banks.get("romSha256") != rom_sha256
        or not isinstance(summary, dict)
        or not isinstance(gamescript, dict)
    ):
        raise ValueError("map event textbox text-bank provenance drift")
    line_id_count = gamescript.get("lineIdCount")
    first_line_id = gamescript.get("firstLineId")
    last_line_id = gamescript.get("lastLineId")
    if (
        not isinstance(line_id_count, int)
        or not isinstance(first_line_id, int)
        or not isinstance(last_line_id, int)
        or line_id_count <= 0
        or first_line_id < 0
        or gamescript.get("sourcePath") != GAMESCRIPT_PATH.as_posix()
        or summary.get("stringCount") != line_id_count
        or gamescript.get("idsAreContiguous") is not True
        or last_line_id - first_line_id + 1 != line_id_count
    ):
        raise ValueError("map event textbox text-line domain drift")
    return (
        {
            "contractId": text_banks["id"],
            "upstreamCommit": upstream_commit,
            "romSha256": rom_sha256,
            "sourcePath": gamescript.get("sourcePath"),
            "lineIdCount": line_id_count,
            "firstLineId": first_line_id,
            "lastLineId": last_line_id,
            "idsAreContiguous": True,
        },
        set(range(first_line_id, last_line_id + 1)),
    )


def _empty_textbox_weight_counts() -> dict[str, int]:
    return {
        "physicalProgramOccurrenceCount": 0,
        "physicalRecordWeightedSiteCount": 0,
        "setupRecordReferenceWeightedSiteCount": 0,
        "routeRecordReferenceWeightedSiteCount": 0,
    }


def _add_textbox_weight_counts(
    destination: dict[str, int], source: dict[str, int]
) -> None:
    if set(destination) != set(source) or any(
        not isinstance(value, int) or value < 0 for value in source.values()
    ):
        raise ValueError("map event textbox weight aggregation drift")
    for field, value in source.items():
        destination[field] += value


def _textbox_reference_contract(
    operation_definitions: list[dict[str, Any]],
    programs_by_category: dict[str, list[dict[str, Any]]],
    *,
    text_line_domain_contract: dict[str, Any],
    upstream_commit: str,
    rom_sha256: str,
) -> dict[str, Any]:
    """Aggregate direct TEXTBOX source sites with zero-inclusive caller and line totals."""
    line_domain, declared_line_ids = _textbox_line_domain(
        text_line_domain_contract,
        upstream_commit=upstream_commit,
        rom_sha256=rom_sha256,
    )
    service_definitions = _textbox_service_definitions(operation_definitions)
    sites = _textbox_reference_sites(
        programs_by_category, service_definitions, declared_line_ids
    )
    categories = tuple(programs_by_category)
    kinds = ("line-reference", "close-sentinel")
    caller_sites = {
        (category, _program_key(program["canonicalSymbol"], program["entryAddress"])): []
        for category, programs in programs_by_category.items()
        for program in programs
    }
    line_sites = {line_id: [] for line_id in sorted(declared_line_ids)}
    for site in sites:
        caller_sites[(site["category"], site["callerProgramKey"])].append(site)
        if site["lineId"] is not None:
            line_sites[site["lineId"]].append(site)

    def total_weights(contained_sites: list[dict[str, Any]]) -> dict[str, int]:
        result = _empty_textbox_weight_counts()
        for site in contained_sites:
            _add_textbox_weight_counts(result, site["weightCounts"])
        return result

    def kind_weights(contained_sites: list[dict[str, Any]]) -> dict[str, dict[str, int]]:
        result = {kind: _empty_textbox_weight_counts() for kind in kinds}
        for site in contained_sites:
            _add_textbox_weight_counts(result[site["sourceKind"]], site["weightCounts"])
        return result

    def category_weights(contained_sites: list[dict[str, Any]]) -> dict[str, dict[str, int]]:
        result = {category: _empty_textbox_weight_counts() for category in categories}
        for site in contained_sites:
            _add_textbox_weight_counts(result[site["category"]], site["weightCounts"])
        return result

    def category_kind_weights(
        contained_sites: list[dict[str, Any]],
    ) -> dict[str, dict[str, dict[str, int]]]:
        result = {
            category: {kind: _empty_textbox_weight_counts() for kind in kinds}
            for category in categories
        }
        for site in contained_sites:
            _add_textbox_weight_counts(
                result[site["category"]][site["sourceKind"]], site["weightCounts"]
            )
        return result

    caller_totals = []
    for category, programs in programs_by_category.items():
        for program in programs:
            caller_key = _program_key(program["canonicalSymbol"], program["entryAddress"])
            contained_sites = caller_sites[(category, caller_key)]
            caller_totals.append(
                {
                    "category": category,
                    "callerProgramKey": caller_key,
                    "callerProgramCanonicalSymbol": program["canonicalSymbol"],
                    "callerProgramEntryAddress": program["entryAddress"],
                    "callerProgramOrder": program["programOrder"],
                    "siteOrders": [site["siteOrder"] for site in contained_sites],
                    "kindWeightCounts": kind_weights(contained_sites),
                }
            )
    line_totals = [
        {
            "lineId": line_id,
            "siteOrders": [site["siteOrder"] for site in line_sites[line_id]],
            "weightCounts": total_weights(line_sites[line_id]),
            "categoryWeightCounts": category_weights(line_sites[line_id]),
        }
        for line_id in sorted(line_sites)
    ]
    line_reference_sites = [site for site in sites if site["sourceKind"] == "line-reference"]
    if not line_reference_sites:
        raise ValueError("map event textbox line-reference coverage drift")
    contract = {
        "textboxLineDomain": line_domain,
        "textboxServiceDefinitions": service_definitions,
        "textboxReferenceSites": sites,
        "textboxCallerTotals": caller_totals,
        "textboxLineTotals": line_totals,
        "textboxSummary": {
            "serviceDefinitionCount": len(service_definitions),
            "siteCount": len(sites),
            "lineReferenceSiteCount": len(line_reference_sites),
            "closeSentinelSiteCount": len(sites) - len(line_reference_sites),
            "declaredLineIdCount": len(line_totals),
            "observedLineIdCount": sum(bool(row["siteOrders"]) for row in line_totals),
            "minimumObservedLineId": min(site["lineId"] for site in line_reference_sites),
            "maximumObservedLineId": max(site["lineId"] for site in line_reference_sites),
            "weightCounts": total_weights(sites),
            "kindWeightCounts": kind_weights(sites),
            "categoryKindWeightCounts": category_kind_weights(sites),
        },
        "textboxServiceDefinitionOrder": [
            definition["sourceKind"] for definition in service_definitions
        ],
        "textboxReferenceSiteOrder": [str(site["siteOrder"]) for site in sites],
        "textboxCallerTotalOrder": [
            f"{categories.index(row['category'])}:{row['callerProgramOrder']}"
            for row in caller_totals
        ],
        "textboxLineTotalOrder": [str(row["lineId"]) for row in line_totals],
    }
    return contract


def _reconcile_textbox_reference_contract(
    contract: dict[str, Any],
    operation_definitions: list[dict[str, Any]],
    programs_by_category: dict[str, list[dict[str, Any]]],
    *,
    text_line_domain_contract: dict[str, Any],
    upstream_commit: str,
    rom_sha256: str,
) -> None:
    """Reject a changed TEXTBOX macro, source use, or declared line range before fixtures."""
    expected = _textbox_reference_contract(
        operation_definitions,
        programs_by_category,
        text_line_domain_contract=text_line_domain_contract,
        upstream_commit=upstream_commit,
        rom_sha256=rom_sha256,
    )
    for field, value in expected.items():
        if contract[field] != value:
            raise ValueError(f"map event textbox {field} reconciliation drift")


_SOUND_ENUM_EQUATE = re.compile(
    r"^\s*(?P<symbol>(?:MUSIC|SFX|SOUND_COMMAND)_[A-Z0-9_]+):\s+equ\s+"
    r"(?P<value>\$[0-9A-Fa-f]+|0|[1-9][0-9]*)\s*(?:;.*)?$",
    re.MULTILINE,
)


def _sound_command_category(symbol: str) -> str:
    """Classify only the source enum namespace, not an audible interpretation."""
    if symbol.startswith("MUSIC_"):
        return "music"
    if symbol.startswith("SFX_"):
        return "sfx"
    if symbol.startswith("SOUND_COMMAND_"):
        return "sound-command"
    raise ValueError(f"map event sound operand lacks a sound enum namespace: {symbol}")


def _sound_command_service_definition(
    operation_definitions: list[dict[str, Any]],
) -> dict[str, Any]:
    """Derive the one direct `sndCom` service form from its parsed macro definition."""
    definitions = [
        definition
        for definition in operation_definitions
        if definition["family"] == "event-service-macro"
        and definition["serviceTarget"] == "#SOUND_COMMAND"
    ]
    if len(definitions) != 1:
        raise ValueError("map event sound-command service-definition coverage drift")
    definition = definitions[0]
    if (
        definition["sourceMacro"] != "sndCom"
        or definition["formalParameterOrdinals"] != [1]
        or definition["emissionStatementTemplates"]
        != ["trap #sound_command", "dc.w \\1"]
    ):
        raise ValueError("map event sound-command service emission/order drift")
    return {
        "definitionId": definition["definitionId"],
        "sourceMacro": definition["sourceMacro"],
        "sourcePath": definition["sourcePath"],
        "definitionSourceLine": definition["definitionSourceLine"],
        "serviceTarget": definition["serviceTarget"],
        "operandOrdinal": definition["formalParameterOrdinals"][0],
        "emissionStatementTemplates": definition["emissionStatementTemplates"],
    }


def _sound_command_enum_values(source: str) -> dict[str, int]:
    """Parse the sound namespaces directly from `sf2enums.asm`."""
    values: dict[str, int] = {}
    for match in _SOUND_ENUM_EQUATE.finditer(source):
        symbol = match.group("symbol")
        if symbol in values:
            raise ValueError(f"map event sound enum duplicate definition: {symbol}")
        value_text = match.group("value")
        values[symbol] = int(value_text[1:], 16) if value_text.startswith("$") else int(value_text)
    if not values:
        raise ValueError("map event sound enum source inventory drift")
    return values


def _sound_command_domain(
    disasm: Path,
    sound_data_contract: dict[str, Any],
    *,
    upstream_commit: str,
    rom_sha256: str,
) -> dict[str, Any]:
    """Join direct enum parsing to the source-built music/SFX domain owner."""
    enum_path = disasm / SOUND_ENUM_PATH
    enum_source = read_upstream_text(enum_path)
    enum_sha256 = hashlib.sha256(enum_path.read_bytes()).hexdigest().upper()
    command_model = sound_data_contract.get("commandModel")
    bank_selection = command_model.get("bankSelection") if isinstance(command_model, dict) else None
    sfx_model = command_model.get("sfxModel") if isinstance(command_model, dict) else None
    if (
        sound_data_contract.get("id") != SOUND_DATA_ID
        or sound_data_contract.get("upstream", {}).get("commit") != upstream_commit
        or sound_data_contract.get("rom", {}).get("sha256") != rom_sha256
        or sound_data_contract.get("scope") != "data/sound"
        or not isinstance(bank_selection, dict)
        or not isinstance(sfx_model, dict)
        or bank_selection.get("enumSourcePath") != SOUND_ENUM_PATH.as_posix()
        or bank_selection.get("enumSourceSha256") != enum_sha256
    ):
        raise ValueError("map event sound-data contract provenance drift")
    music_slot_count = bank_selection.get("summary", {}).get("commandSlotCount")
    sfx_summary = sfx_model.get("summary")
    sfx_minimum = sfx_summary.get("minimumCommand") if isinstance(sfx_summary, dict) else None
    sfx_maximum = sfx_summary.get("maximumCommand") if isinstance(sfx_summary, dict) else None
    if (
        not isinstance(music_slot_count, int)
        or music_slot_count <= 0
        or not isinstance(sfx_minimum, int)
        or not isinstance(sfx_maximum, int)
        or sfx_minimum > sfx_maximum
    ):
        raise ValueError("map event sound-data command-domain drift")
    return {
        "soundDataContractId": SOUND_DATA_ID,
        "soundDataSourceScope": sound_data_contract["scope"],
        "sourceEnumPath": SOUND_ENUM_PATH.as_posix(),
        "sourceEnumSha256": enum_sha256,
        "musicCommandDomain": {
            "minimumValue": 1,
            "maximumValue": music_slot_count,
            "soundDataFactPath": "commandModel.bankSelection",
        },
        "sfxCommandDomain": {
            "minimumValue": sfx_minimum,
            "maximumValue": sfx_maximum,
            "soundDataFactPath": "commandModel.sfxModel",
        },
        "sourceNamespaceCategories": list(SOUND_COMMAND_CATEGORIES),
        "_enumValues": _sound_command_enum_values(enum_source),
    }


def _build_sound_command_domain(
    rom_path: Path,
    upstream_path: Path,
    *,
    disasm: Path,
    upstream_commit: str,
    rom_sha256: str,
) -> dict[str, Any]:
    """Build the source/ROM sound owner rather than treating its fixture as truth."""
    return _sound_command_domain(
        disasm,
        build_sound_data_inventory(rom_path, upstream_path),
        upstream_commit=upstream_commit,
        rom_sha256=rom_sha256,
    )


def _sound_command_weight_counts(reference_counts: dict[str, int]) -> dict[str, int]:
    """Keep target-program occurrence and joined record weights separate."""
    required = {
        "physicalRecordCount",
        "setupRecordReferenceCount",
        "routeRecordReferenceCount",
    }
    if set(reference_counts) != required or any(
        not isinstance(value, int) or value < 0 for value in reference_counts.values()
    ):
        raise ValueError("map event sound-command caller reference-weight shape drift")
    return {
        "physicalProgramOccurrenceCount": 1,
        "physicalRecordWeightedSiteCount": reference_counts["physicalRecordCount"],
        "setupRecordReferenceWeightedSiteCount": reference_counts[
            "setupRecordReferenceCount"
        ],
        "routeRecordReferenceWeightedSiteCount": reference_counts[
            "routeRecordReferenceCount"
        ],
    }


def _empty_sound_command_weight_counts() -> dict[str, int]:
    return {
        "physicalProgramOccurrenceCount": 0,
        "physicalRecordWeightedSiteCount": 0,
        "setupRecordReferenceWeightedSiteCount": 0,
        "routeRecordReferenceWeightedSiteCount": 0,
    }


def _add_sound_command_weight_counts(
    destination: dict[str, int], source: dict[str, int]
) -> None:
    if set(destination) != set(source) or any(
        not isinstance(value, int) or value < 0 for value in source.values()
    ):
        raise ValueError("map event sound-command weight aggregation drift")
    for field, value in source.items():
        destination[field] += value


def _sound_command_reference_contract(
    operation_definitions: list[dict[str, Any]],
    programs_by_category: dict[str, list[dict[str, Any]]],
    *,
    sound_domain: dict[str, Any],
) -> dict[str, Any]:
    """Build direct `sndCom` source/H1 sites and zero-inclusive caller totals."""
    service_definition = _sound_command_service_definition(operation_definitions)
    enum_values = sound_domain.get("_enumValues")
    music_domain = sound_domain.get("musicCommandDomain")
    sfx_domain = sound_domain.get("sfxCommandDomain")
    if (
        not isinstance(enum_values, dict)
        or not isinstance(music_domain, dict)
        or not isinstance(sfx_domain, dict)
        or sound_domain.get("sourceNamespaceCategories") != list(SOUND_COMMAND_CATEGORIES)
    ):
        raise ValueError("map event sound-command domain shape drift")
    caller_sites = {
        (category, _program_key(program["canonicalSymbol"], program["entryAddress"])): []
        for category, programs in programs_by_category.items()
        for program in programs
    }
    operand_sites: dict[str, list[dict[str, Any]]] = {}
    sites: list[dict[str, Any]] = []
    for category, programs in programs_by_category.items():
        for program in programs:
            caller_key = _program_key(program["canonicalSymbol"], program["entryAddress"])
            weights = _sound_command_weight_counts(program["referenceCounts"])
            for operation_index, operation in enumerate(program["operations"]):
                if operation["definitionId"] != service_definition["definitionId"]:
                    continue
                if (
                    operation["sourceMnemonic"] != service_definition["sourceMacro"]
                    or operation["family"] != "event-service-macro"
                    or operation["sourceOrder"] != operation_index
                    or len(operation["operandTexts"]) != 1
                ):
                    raise ValueError("map event sound-command source/use-site drift")
                source_operand = operation["operandTexts"][0]
                if re.fullmatch(r"[A-Za-z_][A-Za-z0-9_]*", source_operand) is None:
                    raise ValueError("map event sound-command operand syntax drift")
                try:
                    source_category = _sound_command_category(source_operand)
                except ValueError as error:
                    raise ValueError("map event sound-command operand namespace drift") from error
                resolved_value = enum_values.get(source_operand)
                if not isinstance(resolved_value, int) or not 0 <= resolved_value <= 0xFFFF:
                    raise ValueError("map event sound-command enum resolution drift")
                if source_category == "music" and not (
                    music_domain["minimumValue"]
                    <= resolved_value
                    <= music_domain["maximumValue"]
                ):
                    raise ValueError("map event sound-command music-domain drift")
                if source_category == "sfx" and not (
                    sfx_domain["minimumValue"]
                    <= resolved_value
                    <= sfx_domain["maximumValue"]
                ):
                    raise ValueError("map event sound-command SFX-domain drift")
                site = {
                    "category": category,
                    "callerProgramKey": caller_key,
                    "operationSourceOrder": operation["sourceOrder"],
                    "sourceOperand": source_operand,
                    "resolvedValue": resolved_value,
                    "sourceCategory": source_category,
                    "weightCounts": dict(weights),
                }
                sites.append(site)
                caller_sites[(category, caller_key)].append(site)
                operand_sites.setdefault(source_operand, []).append(site)
    if not sites:
        raise ValueError("map event sound-command source-use coverage drift")

    def total_weights(contained_sites: list[dict[str, Any]]) -> dict[str, int]:
        result = _empty_sound_command_weight_counts()
        for site in contained_sites:
            _add_sound_command_weight_counts(result, site["weightCounts"])
        return result

    def source_category_site_counts(contained_sites: list[dict[str, Any]]) -> dict[str, int]:
        result = {source_category: 0 for source_category in SOUND_COMMAND_CATEGORIES}
        for site in contained_sites:
            result[site["sourceCategory"]] += 1
        return result

    caller_totals = []
    complete_caller_program_count = 0
    positive_caller_program_count = 0
    for category, programs in programs_by_category.items():
        for program in programs:
            complete_caller_program_count += 1
            caller_key = _program_key(program["canonicalSymbol"], program["entryAddress"])
            contained_sites = caller_sites[(category, caller_key)]
            if not contained_sites:
                continue
            positive_caller_program_count += 1
            caller_totals.append(
                {
                    "callerProgramKey": caller_key,
                    "siteCount": len(contained_sites),
                    "weightCounts": total_weights(contained_sites),
                }
            )
    zero_caller_program_count = (
        complete_caller_program_count - positive_caller_program_count
    )
    if (
        complete_caller_program_count != len(caller_sites)
        or positive_caller_program_count != len(caller_totals)
        or zero_caller_program_count < 0
    ):
        raise ValueError("map event sound-command caller completeness drift")
    observed_source_symbol_count = 0
    for contained_sites in operand_sites.values():
        first_site = contained_sites[0]
        if any(
            site["sourceCategory"] != first_site["sourceCategory"]
            or site["resolvedValue"] != first_site["resolvedValue"]
            for site in contained_sites
        ):
            raise ValueError("map event sound-command operand identity drift")
        observed_source_symbol_count += 1
    contract = {
        "soundCommandSites": sites,
        "soundCommandCallerTotals": caller_totals,
        "soundCommandSummary": {
            "soundDataContractId": sound_domain["soundDataContractId"],
            "siteCount": len(sites),
            "observedSourceSymbolCount": observed_source_symbol_count,
            "observedResolvedValueCount": len(
                {site["resolvedValue"] for site in sites}
            ),
            "completeCallerProgramCount": complete_caller_program_count,
            "positiveCallerProgramCount": positive_caller_program_count,
            "zeroCallerProgramCount": zero_caller_program_count,
            "weightCounts": total_weights(sites),
            "sourceCategorySiteCounts": source_category_site_counts(sites),
        },
    }
    return contract


def _reconcile_sound_command_reference_contract(
    contract: dict[str, Any],
    operation_definitions: list[dict[str, Any]],
    programs_by_category: dict[str, list[dict[str, Any]]],
    *,
    sound_domain: dict[str, Any],
) -> None:
    """Reject macro, enum, source-use, order, and weight drift before fixtures."""
    expected = _sound_command_reference_contract(
        operation_definitions,
        programs_by_category,
        sound_domain=sound_domain,
    )
    for field, value in expected.items():
        if contract[field] != value:
            raise ValueError(f"map event sound-command {field} reconciliation drift")


def _source_program_block(
    disasm: Path,
    profile: dict[str, Any],
    addresses: dict[str, int],
    *,
    allow_source_stream_terminator: bool = False,
) -> dict[str, Any]:
    """Parse one target block through an H1-verifiable source boundary."""
    source_path = profile["ownerSourcePath"]
    lines = read_upstream_text(disasm / source_path).splitlines()
    entry_line = profile["ownerSourceLine"]
    if not 1 <= entry_line <= len(lines):
        raise ValueError(f"map entity-event entry line is out of range: {source_path}")
    entry_match = _PROGRAM_LABEL.fullmatch(lines[entry_line - 1])
    if entry_match is None or entry_match.group(1) != profile["canonicalSymbol"]:
        raise ValueError(f"map entity-event entry label drift: {profile['canonicalSymbol']}")
    if profile["targetH1Address"] != addresses[profile["canonicalSymbol"]]:
        raise ValueError(f"map entity-event entry H1 address drift: {profile['canonicalSymbol']}")

    end_index: int | None = None
    end_symbol: str | None = None
    source_stream_end_index: int | None = None
    for index in range(entry_line, len(lines)):
        statement = _normalise_asm_statement(lines[index])
        if allow_source_stream_terminator and statement == "csc_end":
            source_stream_end_index = index
            break
        end_match = _PROGRAM_END.fullmatch(lines[index])
        if end_match is not None:
            end_index = index
            end_symbol = end_match.group(1)
            break
    if source_stream_end_index is not None:
        end_index = source_stream_end_index + 1
    elif end_index is None or end_symbol is None:
        raise ValueError(f"map entity-event source function boundary is missing: {source_path}")

    labels: list[dict[str, Any]] = []
    operations: list[dict[str, Any]] = []
    for index in range(entry_line - 1, end_index):
        raw_line = lines[index]
        statement = _normalise_asm_statement(raw_line)
        if not statement:
            continue
        label_match = _PROGRAM_LABEL.fullmatch(statement)
        trailing_operation: str | None = None
        if label_match is None:
            inline_label = _PROGRAM_LABEL.match(statement)
            if inline_label is not None:
                label_match = inline_label
                trailing_operation = inline_label.group(2).strip()
        if label_match is not None:
            symbol = label_match.group(1)
            if symbol not in addresses:
                raise ValueError(f"map entity-event label lacks H1 address: {symbol}")
            labels.append(
                {
                    "sourceOrder": len(labels),
                    "sourceLine": index + 1,
                    "symbol": symbol,
                    "address": addresses[symbol],
                }
            )
            statement = (
                trailing_operation
                if trailing_operation is not None
                else label_match.group(2).strip()
            )
        if statement:
            operation = _parse_program_operation(
                statement, source_line=index + 1, source_order=len(operations)
            )
            operation["sourceStatement"] = statement
            operations.append(operation)
    if not labels or labels[0]["symbol"] != profile["canonicalSymbol"]:
        raise ValueError(
            f"map entity-event entry label coverage drift: {profile['canonicalSymbol']}"
        )
    if not operations:
        raise ValueError(f"map entity-event has no operations: {profile['canonicalSymbol']}")
    end_source_line = (
        source_stream_end_index + 1 if source_stream_end_index is not None else end_index + 1
    )
    block = {
        "labels": labels,
        "operations": operations,
        "endFunctionSymbol": end_symbol,
        "endSourceLine": end_source_line,
    }
    if source_stream_end_index is not None:
        block["sourceStreamTerminator"] = "csc_end"
    return block


def _listing_entry_index(
    listing_index: dict[str, dict[Any, Any]], *, symbol: str, address: int
) -> int:
    entry_index = listing_index["entries"].get((symbol, address))
    if entry_index is None:
        raise ValueError(f"map entity-event H1 entry listing drift: {symbol}")
    return entry_index


def _listing_program_end(
    listing_index: dict[str, dict[Any, Any]], *, entry_index: int, end_function_symbol: str
) -> tuple[int, int]:
    boundary = listing_index["ends"].get(end_function_symbol)
    if boundary is None or boundary[0] <= entry_index:
        raise ValueError(f"map entity-event H1 function end drift: {end_function_symbol}")
    return boundary


def _listing_source_stream_end(
    listing_lines: list[str], *, terminal_listing_index: int, terminal_address: int
) -> int:
    """Derive a source-stream terminal's exclusive address from its next H1 source row."""
    for raw_line in listing_lines[terminal_listing_index + 1 :]:
        row = _listing_statement(raw_line)
        if row is not None and row[0] > terminal_address:
            return row[0]
    raise ValueError("map entity-event H1 source stream boundary is missing")


def _h1_program_index(listing_lines: list[str]) -> dict[str, dict[Any, Any]]:
    """Index H1 labels and source function-end markers once for the full program corpus."""
    entries: dict[tuple[str, int], int] = {}
    ends: dict[str, tuple[int, int]] = {}
    for index, raw_line in enumerate(listing_lines):
        row = _listing_statement(raw_line)
        if row is not None and row[1].endswith(":"):
            symbol = row[1][:-1]
            if _PLAIN_TARGET.fullmatch(symbol) is not None:
                identity = (symbol, row[0])
                if identity in entries:
                    raise ValueError(f"map entity-event duplicate H1 label entry: {symbol}")
                entries[identity] = index
        line_match = _LISTING_LINE.match(raw_line)
        if line_match is None:
            continue
        end_match = _PROGRAM_END.fullmatch(line_match.group(2))
        if end_match is None:
            continue
        symbol = end_match.group(1)
        if symbol in ends:
            raise ValueError(f"map entity-event duplicate H1 function end: {symbol}")
        ends[symbol] = (index, int(line_match.group(1), 16))
    return {"entries": entries, "ends": ends}


def _bind_operations_to_h1(
    listing_lines: list[str],
    listing_index: dict[str, dict[Any, Any]],
    *,
    profile: dict[str, Any],
    block: dict[str, Any],
) -> int:
    """Guard source opcode/operand/order against the pinned H1 listing before fixtures."""
    entry_address = profile["targetH1Address"]
    entry_index = _listing_entry_index(
        listing_index, symbol=profile["canonicalSymbol"], address=entry_address
    )
    source_stream_terminator = block.get("sourceStreamTerminator")
    if source_stream_terminator is None:
        end_index, end_address = _listing_program_end(
            listing_index,
            entry_index=entry_index,
            end_function_symbol=block["endFunctionSymbol"],
        )
    else:
        if (
            source_stream_terminator != "csc_end"
            or block["operations"][-1]["sourceMnemonic"] != source_stream_terminator
            or block["endFunctionSymbol"] is not None
        ):
            raise ValueError(
                f"map entity-event source stream terminator drift: {profile['canonicalSymbol']}"
            )
        end_index, end_address = len(listing_lines), None
    if end_address is not None and end_address <= entry_address:
        raise ValueError(
            f"map entity-event H1 nonpositive program span: {profile['canonicalSymbol']}"
        )
    cursor = entry_index + 1
    operation_addresses: list[int] = []
    terminal_listing_index: int | None = None
    for operation in block["operations"]:
        expected_statement = operation["sourceStatement"]
        matched: tuple[int, int] | None = None
        for index in range(cursor, end_index):
            row = _listing_statement(listing_lines[index])
            if row is not None and row[1] == expected_statement:
                matched = (index, row[0])
                break
        if matched is None:
            raise ValueError(
                "map entity-event source/H1 operation relationship drift: "
                f"{profile['canonicalSymbol']}:{operation['sourceLine']}"
            )
        cursor, operation["address"] = matched[0] + 1, matched[1]
        # Retained only until the source macro expansion is checked below.  It is
        # deliberately removed before the canonical contract is returned.
        operation["_h1ListingSourceIndex"] = matched[0]
        operation_addresses.append(operation["address"])
        terminal_listing_index = matched[0]
        if end_address is not None and not entry_address <= operation["address"] < end_address:
            raise ValueError(
                f"map entity-event operation address falls outside program span: "
                f"{profile['canonicalSymbol']}:{operation['sourceLine']}"
            )
        del operation["sourceStatement"]
    if source_stream_terminator is not None:
        if terminal_listing_index is None:
            raise ValueError(
                "map entity-event source stream terminal lacks H1 use site: "
                f"{profile['canonicalSymbol']}"
            )
        end_address = _listing_source_stream_end(
            listing_lines,
            terminal_listing_index=terminal_listing_index,
            terminal_address=operation_addresses[-1],
        )
    if end_address <= entry_address:
        raise ValueError(
            f"map entity-event H1 nonpositive program span: {profile['canonicalSymbol']}"
        )
    if any(address < entry_address or address >= end_address for address in operation_addresses):
        raise ValueError(
            f"map entity-event operation address falls outside program span: "
            f"{profile['canonicalSymbol']}"
        )
    return end_address


def _alias_target_symbol(operand_texts: list[str], source_line: int) -> str:
    if len(operand_texts) != 1:
        raise ValueError(
            f"map entity-event jump-interface operand drift at source line {source_line}"
        )
    operand = operand_texts[0]
    target_match = _PC_RELATIVE_TARGET.fullmatch(operand)
    if target_match is not None:
        return target_match.group(1)
    return _operation_target_symbol(operand_texts, source_line)


def _parse_jump_interface_aliases(
    disasm: Path,
    addresses: dict[str, int],
    listing_lines: list[str],
    listing_index: dict[str, dict[Any, Any]],
    label_owners: dict[int, list[dict[str, Any]]],
    aliases: list[str],
) -> dict[str, dict[str, Any]]:
    """Resolve each called `j_` interface through its source/H1 jump definition."""
    definitions: dict[str, dict[str, Any]] = {}
    for alias in aliases:
        if alias not in addresses:
            raise ValueError(f"map entity-event jump-interface lacks H1 address: {alias}")
        owner_matches = [
            owner for owner in label_owners.get(addresses[alias], []) if owner["symbol"] == alias
        ]
        if len(owner_matches) != 1:
            raise ValueError(f"map entity-event jump-interface owner drift: {alias}")
        owner = owner_matches[0]
        source_lines = read_upstream_text(disasm / owner["sourcePath"]).splitlines()
        source_index = owner["sourceLine"] - 1
        label_match = _PROGRAM_LABEL.fullmatch(source_lines[source_index])
        if label_match is None or label_match.group(1) != alias:
            raise ValueError(f"map entity-event jump-interface label drift: {alias}")
        operation: dict[str, Any] | None = None
        for index in range(source_index + 1, len(source_lines)):
            statement = _normalise_asm_statement(source_lines[index])
            if not statement:
                continue
            if _PROGRAM_LABEL.fullmatch(statement) is not None:
                break
            operation = _parse_program_operation(statement, source_line=index + 1, source_order=0)
            break
        if operation is None or operation["controlFlowKind"] != "direct-jump":
            raise ValueError(f"map entity-event jump-interface definition drift: {alias}")
        target_symbol = _alias_target_symbol(operation["operandTexts"], operation["sourceLine"])
        if target_symbol not in addresses:
            raise ValueError(f"map entity-event jump-interface target lacks H1 address: {alias}")
        entry_index = _listing_entry_index(listing_index, symbol=alias, address=addresses[alias])
        expected_statement = _normalise_asm_statement(source_lines[operation["sourceLine"] - 1])
        h1_row: tuple[int, str] | None = None
        for raw_line in listing_lines[entry_index + 1 :]:
            row = _listing_statement(raw_line)
            if row is not None and row[1].endswith(":"):
                break
            if row is not None and row[1] == expected_statement:
                h1_row = row
                break
        if h1_row is None:
            raise ValueError(f"map entity-event jump-interface source/H1 drift: {alias}")
        definitions[alias] = {
            "aliasSymbol": alias,
            "aliasAddress": addresses[alias],
            "sourcePath": owner["sourcePath"],
            "sourceLine": owner["sourceLine"],
            "definitionSourceLine": operation["sourceLine"],
            "sourceMnemonic": operation["sourceMnemonic"],
            "mnemonic": operation["mnemonic"],
            "sizeSuffix": operation["sizeSuffix"],
            "operandTexts": operation["operandTexts"],
            "directTargetSymbol": target_symbol,
            "directTargetAddress": addresses[target_symbol],
            "listingAddress": h1_row[0],
        }
    return definitions


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _macro_block(source: str, macro: str) -> tuple[str, int]:
    match = re.search(rf"(?ms)^{re.escape(macro)}:\s+macro\s*$.*?^\s*endm\s*$", source)
    if match is None:
        raise ValueError(f"map event macro definition is missing: {macro}")
    return match.group(0), source[: match.start()].count("\n") + 1


def _directive_width(directive: str) -> int:
    return {"dc.b": 1, "dc.w": 2, "dc.l": 4}[directive]


def _macro_definition(source: str, macro: str, kind: str) -> dict[str, Any]:
    """Parse byte-emitting macro positions that bind a source use site to a record."""
    block, definition_line = _macro_block(source, macro)
    directives: list[dict[str, Any]] = []
    for raw_line in block.splitlines():
        line = raw_line.split(";", 1)[0].strip()
        if not line or line == f"{macro}: macro" or line == "endm":
            continue
        parts = line.split(None, 1)
        if len(parts) != 2 or parts[0] not in {"dc.b", "dc.w", "dc.l"}:
            raise ValueError(f"map event macro directive drift: {macro}: {line!r}")
        positions = [int(value) for value in re.findall(r"\\(\d+)", parts[1])]
        directives.append(
            {
                "sourceOrder": len(directives),
                "directive": parts[0],
                "operandText": parts[1],
                "widthBytes": _directive_width(parts[0]),
                "argumentPositions": positions,
            }
        )
    if not directives:
        raise ValueError(f"map event macro has no byte-emitting directives: {macro}")
    target_directives = [
        directive
        for directive in directives
        if directive["directive"] == "dc.w" and re.fullmatch(r"\\(\d+)", directive["operandText"])
    ]
    if len(target_directives) != 1:
        raise ValueError(f"map event macro target operand drift: {macro}")
    if target_directives[0]["sourceOrder"] != len(directives) - 1:
        raise ValueError(f"map event macro target directive order drift: {macro}")
    target_position = target_directives[0]["argumentPositions"][0]
    argument_positions = sorted(
        {position for directive in directives for position in directive["argumentPositions"]}
    )
    if argument_positions != list(range(1, max(argument_positions, default=0) + 1)):
        raise ValueError(f"map event macro argument positions drift: {macro}")
    marker: int | None = None
    if kind == "default":
        first_operand = directives[0]["operandText"]
        marker_match = re.fullmatch(r"\$([0-9A-Fa-f]+)", first_operand)
        if marker_match is None:
            raise ValueError(f"map event default macro marker drift: {macro}")
        literal = int(marker_match.group(1), 16)
        marker = literal >> ((directives[0]["widthBytes"] - 1) * 8)
    return {
        "macro": macro,
        "kind": kind,
        "definitionLine": definition_line,
        "argumentCount": max(argument_positions, default=0),
        "targetOperandPosition": target_position,
        "defaultMarker": marker,
        "encodedRecordBytes": sum(directive["widthBytes"] for directive in directives),
        "emittedDirectives": directives,
    }


def _event_macro_definitions(disasm: Path) -> dict[str, list[dict[str, Any]]]:
    source = read_upstream_text(disasm / MAP_SETUP_MACROS_PATH)
    definitions: dict[str, list[dict[str, Any]]] = {}
    for category, config in CATEGORY_CONFIG.items():
        category_definitions = [
            _macro_definition(source, macro, "specific") for macro in config["specificMacros"]
        ] + [_macro_definition(source, macro, "default") for macro in config["defaultMacros"]]
        if any(
            definition["encodedRecordBytes"] != config["recordBytes"]
            for definition in category_definitions
        ):
            raise ValueError(f"map event macro record width drift: {category}")
        definitions[category] = category_definitions
    return definitions


def _split_macro_operands(text: str) -> list[str]:
    operands: list[str] = []
    start = 0
    depth = 0
    for index, character in enumerate(text):
        if character == "(":
            depth += 1
        elif character == ")":
            depth -= 1
            if depth < 0:
                raise ValueError(f"map event macro operand has unmatched ')': {text!r}")
        elif character == "," and depth == 0:
            operands.append(text[start:index].strip())
            start = index + 1
    if depth != 0:
        raise ValueError(f"map event macro operand has unmatched '(': {text!r}")
    operands.append(text[start:].strip())
    if not all(operands):
        raise ValueError(f"map event macro has empty operand: {text!r}")
    return operands


def _source_macro_catalog(disasm: Path, paths: tuple[Path, ...]) -> dict[str, dict[str, Any]]:
    """Parse source macro bodies once, retaining their exact owner locations."""
    catalog: dict[str, dict[str, Any]] = {}
    header = re.compile(r"^\s*([A-Za-z_][A-Za-z0-9_]*):\s*macro\b.*$")
    for path in paths:
        source_path = path.as_posix()
        lines = read_upstream_text(disasm / path).splitlines()
        line_index = 0
        while line_index < len(lines):
            match = header.fullmatch(lines[line_index])
            if match is None:
                line_index += 1
                continue
            name = match.group(1)
            end_index = line_index + 1
            while end_index < len(lines) and lines[end_index].strip().lower() != "endm":
                end_index += 1
            if end_index == len(lines):
                raise ValueError(f"map event macro has no endm: {source_path}:{name}")
            if name in catalog:
                raise ValueError(f"map event duplicate macro definition: {name}")
            body = [
                (body_index + 1, _normalise_asm_statement(lines[body_index]))
                for body_index in range(line_index + 1, end_index)
                if _normalise_asm_statement(lines[body_index])
            ]
            catalog[name] = {
                "sourcePath": source_path,
                "definitionSourceLine": line_index + 1,
                "body": body,
            }
            line_index = end_index + 1
    return catalog


def _substitute_macro_arguments(statement: str, arguments: list[str], *, macro: str) -> str:
    """Bind source argument positions without assigning them a runtime meaning."""

    def substitute(match: re.Match[str]) -> str:
        ordinal = int(match.group(1))
        if not 1 <= ordinal <= len(arguments):
            raise ValueError(f"map event macro argument position drift: {macro}:{ordinal}")
        return arguments[ordinal - 1]

    return re.sub(r"\\(\d+)", substitute, statement)


def _macro_emission_statements(
    macro_catalog: dict[str, dict[str, Any]],
    *,
    macro: str,
    arguments: list[str],
    expansion_stack: tuple[str, ...] = (),
) -> list[str]:
    """Expand one source macro to its source-faithful emitted leaf statements."""
    if macro in expansion_stack:
        raise ValueError(f"map event recursive macro definition: {macro}")
    definition = macro_catalog.get(macro)
    if definition is None:
        raise ValueError(f"map event macro definition is missing: {macro}")
    emitted: list[str] = []
    for _, raw_statement in definition["body"]:
        statement = _substitute_macro_arguments(raw_statement, arguments, macro=macro)
        invocation = re.fullmatch(r"([A-Za-z_][A-Za-z0-9_]*(?:\.[A-Za-z])?)(?:\s+(.*))?", statement)
        if invocation is None:
            raise ValueError(f"map event macro statement syntax drift: {macro}:{statement!r}")
        token, argument_text = invocation.groups()
        if token in macro_catalog:
            child_arguments = _split_macro_operands(argument_text) if argument_text else []
            emitted.extend(
                _macro_emission_statements(
                    macro_catalog,
                    macro=token,
                    arguments=child_arguments,
                    expansion_stack=(*expansion_stack, macro),
                )
            )
            continue
        if token in {"dc.b", "dc.w", "dc.l", "trap", "lea"}:
            emitted.append(statement)
            continue
        raise ValueError(f"map event unsupported macro emission: {macro}:{statement!r}")
    if not emitted:
        raise ValueError(f"map event macro emits no source statements: {macro}")
    return emitted


def _canonical_macro_emission_statement(statement: str) -> str:
    """Compare source/H1 macro leaf statements while retaining operand order."""
    compact = _normalise_asm_statement(statement).lower()

    def canonical_hex(match: re.Match[str]) -> str:
        return f"${int(match.group(1), 16):x}"

    return re.sub(r"\$([0-9a-f]+)", canonical_hex, compact)


def _listing_macro_emission_rows(
    listing_lines: list[str], *, source_listing_index: int
) -> list[dict[str, Any]]:
    """Read only byte-emitting H1 macro-expansion rows after one source use site."""
    rows: list[dict[str, Any]] = []
    for raw_line in listing_lines[source_listing_index + 1 :]:
        line_match = _LISTING_LINE.match(raw_line)
        if line_match is None:
            continue
        remainder = line_match.group(2)
        marker = re.match(r"^(?P<before>.*?)\sM\s+(?P<statement>.*)$", remainder)
        if marker is None:
            break
        byte_tokens = re.findall(
            r"(?<![0-9A-Fa-f])[0-9A-Fa-f]{2,4}(?![0-9A-Fa-f])", marker.group("before")
        )
        if not byte_tokens:
            continue
        statement = _normalise_asm_statement(marker.group("statement"))
        if not statement:
            raise ValueError("map event H1 macro emission statement is empty")
        rows.append(
            {
                "address": int(line_match.group(1), 16),
                "byteCount": sum(len(token) // 2 for token in byte_tokens),
                "statement": statement,
            }
        )
    return rows


def _macro_parameter_ordinals(definition: dict[str, Any]) -> list[int]:
    """Derive declared source positions from the parsed macro body once."""
    ordinals = sorted(
        {
            int(match.group(1))
            for _, statement in definition["body"]
            for match in re.finditer(r"\\(\d+)", statement)
        }
    )
    if ordinals and ordinals != list(range(1, ordinals[-1] + 1)):
        raise ValueError("map event macro parameter ordinal gap")
    return ordinals


def _map_engine_handler_by_macro(contract: dict[str, Any]) -> dict[str, dict[str, Any]]:
    """Reuse the maintained map-script parser's handler-to-macro association."""
    by_macro: dict[str, dict[str, Any]] = {}
    for handler in contract["handlers"]:
        for macro in handler["macroNames"]:
            if macro in by_macro:
                raise ValueError(f"map event macro has multiple engine handlers: {macro}")
            by_macro[macro] = handler
    return by_macro


def _macro_emitted_word(macro_catalog: dict[str, dict[str, Any]], *, macro: str) -> int | None:
    """Read a no-argument macro's sole emitted word when it has one."""
    if not macro_catalog[macro]["body"]:
        return None
    emitted = _macro_emission_statements(macro_catalog, macro=macro, arguments=[])
    if len(emitted) != 1:
        return None
    match = re.fullmatch(r"dc\.w\s+\$([0-9A-Fa-f]{1,4})", emitted[0])
    if match is None:
        return None
    return int(match.group(1), 16)


def _has_csc2d_payload_terminator_use(disasm: Path, handler: dict[str, Any]) -> bool:
    """Guard csc2D's negative first-byte branch and second-byte skip order."""
    lines = read_upstream_text(disasm / handler["sourcePath"]).splitlines()
    start_line = handler["startLine"]
    end_line = handler["endLine"]
    if not 1 <= start_line <= end_line <= len(lines):
        raise ValueError(f"map event handler source range drift: {handler['name']}")
    statements = [
        _normalise_asm_statement(line) for line in lines[start_line - 1 : end_line]
    ]
    first_byte_index = next(
        (
            index
            for index, statement in enumerate(statements)
            if re.fullmatch(r"move\.b \(a6\)\+,d1", statement, re.IGNORECASE)
        ),
        None,
    )
    if first_byte_index is None:
        return False
    negative_branch_index = next(
        (
            index
            for index in range(first_byte_index + 1, len(statements))
            if re.fullmatch(r"bmi(?:\.[bwlst])?\s+.+", statements[index], re.IGNORECASE)
        ),
        None,
    )
    if negative_branch_index is None:
        return False
    second_byte_index = next(
        (
            index
            for index in range(negative_branch_index + 1, len(statements))
            if re.fullmatch(r"move\.b \(a6\)\+,d2", statements[index], re.IGNORECASE)
        ),
        None,
    )
    if second_byte_index is None:
        return False
    branch_target = re.fullmatch(
        r"bmi(?:\.[bwlst])?\s+([A-Za-z_][A-Za-z0-9_]*)",
        statements[negative_branch_index],
        re.IGNORECASE,
    )
    if branch_target is None:
        return False
    target_label = branch_target.group(1)
    target_index = next(
        (
            index
            for index, raw_line in enumerate(lines, start=1)
            if index > end_line
            if (statement := _normalise_asm_statement(raw_line))
            if re.fullmatch(rf"{re.escape(target_label)}:\s*", statement, re.IGNORECASE)
        ),
        None,
    )
    if target_index is None:
        return False
    chunk_start = next(
        (
            index
            for index, raw_line in enumerate(lines, start=1)
            if end_line < index < target_index
            if re.fullmatch(
                rf"\s*;\s*START OF FUNCTION CHUNK FOR {re.escape(handler['name'])}",
                raw_line,
                re.IGNORECASE,
            )
        ),
        None,
    )
    if chunk_start is None:
        return False
    chunk_end = next(
        (
            index
            for index in range(target_index, len(lines))
            if re.fullmatch(
                r"\s*;\s*END OF FUNCTION CHUNK FOR .+", lines[index], re.IGNORECASE
            )
        ),
        len(lines),
    )
    target_block: list[str] = []
    for raw_line in lines[target_index:chunk_end]:
        statement = _normalise_asm_statement(raw_line)
        if not statement:
            continue
        if _basic_block_boundary(statement):
            break
        target_block.append(statement)
    return any(
        re.fullmatch(r"addq\.l #1,a6", statement, re.IGNORECASE)
        for statement in target_block
    )


def _has_inline_action_terminator_use(
    disasm: Path, handler: dict[str, Any], *, terminator_word: int
) -> bool:
    """Guard the csc14 compare/not-equal/return ordering for the parsed terminator word."""
    lines = read_upstream_text(disasm / handler["sourcePath"]).splitlines()
    start_line = handler["startLine"]
    end_line = handler["endLine"]
    if not 1 <= start_line <= end_line <= len(lines):
        raise ValueError(f"map event handler source range drift: {handler['name']}")
    statements = [
        _normalise_asm_statement(line) for line in lines[start_line - 1 : end_line]
    ]
    compare_index = next(
        (
            index
            for index, statement in enumerate(statements)
            if (
                match := re.fullmatch(
                    r"cmpi\.w #\$([0-9A-Fa-f]{1,4}),\(a6\)\+", statement, re.IGNORECASE
                )
            )
            and int(match.group(1), 16) == terminator_word
        ),
        None,
    )
    if compare_index is None:
        return False
    non_equal_branch_index = next(
        (
            index
            for index in range(compare_index + 1, len(statements))
            if re.fullmatch(r"bne(?:\.[bwlst])?\s+.+", statements[index], re.IGNORECASE)
        ),
        None,
    )
    if non_equal_branch_index is None:
        return False
    target_match = re.fullmatch(
        r"bne(?:\.[bwlst])?\s+([A-Za-z_][A-Za-z0-9_]*)",
        statements[non_equal_branch_index],
        re.IGNORECASE,
    )
    if target_match is None:
        return False
    target_label = target_match.group(1)
    if not any(
        re.fullmatch(rf"{re.escape(target_label)}:\s*", statement, re.IGNORECASE)
        for statement in statements
    ):
        return False
    for statement in statements[non_equal_branch_index + 1 :]:
        if re.fullmatch(r"rts", statement, re.IGNORECASE):
            return True
        if _basic_block_boundary(statement):
            return False
    return False


def _derived_action_payload_context_specs(
    disasm: Path,
    macro_catalog: dict[str, dict[str, Any]],
    map_engine_contract: dict[str, Any],
    entity_action_contract: dict[str, Any],
) -> dict[str, dict[str, str]]:
    """Derive the four payload wrapper/terminator pairs from maintained contracts."""
    map_macros = map_engine_contract["macroContracts"]
    handlers_by_macro = _map_engine_handler_by_macro(map_engine_contract)
    inline_terminator = entity_action_contract["handlerFacts"]["inlineTerminatorMacro"]
    inline_word = entity_action_contract["handlerFacts"]["inlineTerminatorWord"]
    inline_binding = next(
        (
            binding
            for binding in entity_action_contract["handlerMacroBindings"]
            if binding["macro"] == inline_terminator
        ),
        None,
    )
    if (
        inline_binding is None
        or not inline_binding["isInlineTerminator"]
        or inline_binding["opcode"] != inline_word
        or inline_terminator not in macro_catalog
        or _macro_emitted_word(macro_catalog, macro=inline_terminator) != inline_word
    ):
        raise ValueError("map event inline payload terminator contract drift")

    stream_terminators = [
        macro
        for macro, contract in map_macros.items()
        if contract["kind"] == "terminator"
    ]
    if len(stream_terminators) != 1 or stream_terminators[0] not in macro_catalog:
        raise ValueError("map event stream terminator catalog drift")
    stream_terminator = stream_terminators[0]
    stream_terminator_definition = macro_catalog[stream_terminator]

    def aliases_for_handler(handler: dict[str, Any]) -> tuple[str, list[str]]:
        primary_macros = [
            macro
            for macro in handler["macroNames"]
            if map_macros[macro]["aliasOf"] is None
        ]
        if len(primary_macros) != 1:
            raise ValueError(f"map event action handler primary macro drift: {handler['name']}")
        primary = primary_macros[0]
        aliases = [
            macro
            for macro, contract in map_macros.items()
            if contract["aliasOf"] == primary
        ]
        if not aliases or any(
            macro not in handler["macroNames"] or handlers_by_macro.get(macro) is not handler
            for macro in aliases
        ):
            raise ValueError(f"map event action handler alias identity drift: {handler['name']}")
        return primary, sorted(aliases)

    inline_flow_groups = [
        handler
        for handler in map_engine_contract["handlers"]
        if handler["cursorFlow"] == "inline-action-program"
    ]
    if len(inline_flow_groups) != 1:
        raise ValueError("map event inline action handler cursor-flow drift")
    inline_groups = [
        handler
        for handler in inline_flow_groups
        if _has_inline_action_terminator_use(disasm, handler, terminator_word=inline_word)
    ]
    if len(inline_groups) != 1:
        raise ValueError("map event inline action handler terminator-use drift")
    _, inline_aliases = aliases_for_handler(inline_groups[0])

    sequence_groups = [
        handler
        for handler in map_engine_contract["handlers"]
        if handler["cursorFlow"] == "sequential"
        and _has_csc2d_payload_terminator_use(disasm, handler)
    ]
    if len(sequence_groups) != 1:
        raise ValueError("map event sequential action handler terminator-use drift")
    sequence_primary, sequence_aliases = aliases_for_handler(sequence_groups[0])
    primary_definition = macro_catalog.get(sequence_primary)
    if primary_definition is None:
        raise ValueError("map event sequential action primary macro source drift")
    sequence_terminators = [
        macro
        for macro, definition in macro_catalog.items()
        if definition["sourcePath"] == primary_definition["sourcePath"]
        and primary_definition["definitionSourceLine"] < definition["definitionSourceLine"]
        < stream_terminator_definition["definitionSourceLine"]
        and not _macro_parameter_ordinals(definition)
        and macro != stream_terminator
        and macro != inline_terminator
        and (word := _macro_emitted_word(macro_catalog, macro=macro)) is not None
        and word & 0x8000
    ]
    if len(sequence_terminators) != 1:
        raise ValueError("map event sequential action terminator definition drift")

    specs = {
        macro: {
            "contextFamily": "entity-action-command-payload",
            "terminatorMnemonic": inline_terminator,
        }
        for macro in inline_aliases
    }
    specs.update(
        {
            macro: {
                "contextFamily": "entity-action-payload",
                "terminatorMnemonic": sequence_terminators[0],
            }
            for macro in sequence_aliases
        }
    )
    if len(specs) != len(inline_aliases) + len(sequence_aliases):
        raise ValueError("map event action payload wrapper overlap")
    return specs


def _operation_definition_contract(
    macro_catalog: dict[str, dict[str, Any]],
    map_engine_contract: dict[str, Any],
    entity_action_contract: dict[str, Any],
    programs_by_category: dict[str, list[dict[str, Any]]],
    payload_context_specs: dict[str, dict[str, str]],
    payload_macro_families: dict[str, str],
) -> tuple[dict[str, dict[str, Any]], dict[str, dict[str, str]]]:
    """Classify the complete source vocabulary and join non-CPU uses once."""
    map_macros = map_engine_contract["macroContracts"]
    map_handlers = _map_engine_handler_by_macro(map_engine_contract)
    action_bindings = {
        binding["macro"]: binding for binding in entity_action_contract["handlerMacroBindings"]
    }
    definitions: dict[str, dict[str, Any]] = {}
    vocabulary: dict[str, dict[str, str]] = {}
    used_source_names = {
        operation["sourceMnemonic"]
        for programs in programs_by_category.values()
        for program in programs
        for operation in program["operations"]
        if operation["sourceMnemonic"] in macro_catalog
    }
    for source_macro in sorted(
        used_source_names,
        key=lambda value: (
            macro_catalog[value]["sourcePath"],
            macro_catalog[value]["definitionSourceLine"],
            value,
        ),
    ):
        source_definition = macro_catalog[source_macro]
        source_path = source_definition["sourcePath"]
        if source_path == SERVICE_MACROS_PATH.as_posix():
            family = "event-service-macro"
            engine_catalog: dict[str, Any] | None = None
            service_targets = [
                statement
                for statement in _macro_emission_statements(
                    macro_catalog,
                    macro=source_macro,
                    arguments=[
                        f"\\{ordinal}" for ordinal in _macro_parameter_ordinals(source_definition)
                    ],
                )
                if statement.startswith("trap ")
            ]
            if len(service_targets) != 1:
                raise ValueError(f"map event service macro emission target drift: {source_macro}")
            service_target = service_targets[0].split(" ", 1)[1]
        elif source_macro in payload_context_specs:
            family = "entity-action-wrapper"
            engine_catalog = {
                "catalog": "map-script-engine",
                "kind": map_macros[source_macro]["kind"],
                "opcode": map_macros[source_macro]["opcode"],
                "encodedBytes": map_macros[source_macro]["encodedBytes"],
                "aliasOf": map_macros[source_macro]["aliasOf"],
                "handler": (
                    map_handlers[source_macro]["name"] if source_macro in map_handlers else None
                ),
            }
            service_target = None
        elif map_macros.get(source_macro, {}).get("kind") == "terminator":
            family = "stream-terminator"
            engine_catalog = {
                "catalog": "map-script-engine",
                "kind": map_macros[source_macro]["kind"],
                "opcode": map_macros[source_macro]["opcode"],
                "encodedBytes": map_macros[source_macro]["encodedBytes"],
                "aliasOf": map_macros[source_macro]["aliasOf"],
                "handler": (
                    map_handlers[source_macro]["name"] if source_macro in map_handlers else None
                ),
            }
            service_target = None
        elif source_macro in action_bindings:
            binding = action_bindings[source_macro]
            family = "entity-action-command"
            engine_catalog = {
                "catalog": "entity-action-scripts",
                "handler": binding["handler"],
                "opcode": binding["opcode"],
                "encodedBytes": binding["encodedBytes"],
                "isInlineTerminator": binding["isInlineTerminator"],
            }
            service_target = None
        elif source_macro in map_macros:
            engine_macro = map_macros[source_macro]
            family = "map-script-macro"
            engine_catalog = {
                "catalog": "map-script-engine",
                "kind": engine_macro["kind"],
                "opcode": engine_macro["opcode"],
                "encodedBytes": engine_macro["encodedBytes"],
                "aliasOf": engine_macro["aliasOf"],
                "handler": (
                    map_handlers[source_macro]["name"] if source_macro in map_handlers else None
                ),
            }
            service_target = None
        elif source_macro in payload_macro_families:
            family = payload_macro_families[source_macro]
            engine_catalog = None
            service_target = None
        else:
            raise ValueError(f"map event macro vocabulary family is unclassified: {source_macro}")
        definition_id = f"{family}:{source_macro}"
        definitions[definition_id] = {
            "definitionId": definition_id,
            "family": family,
            "sourceMacro": source_macro,
            "sourcePath": source_path,
            "definitionSourceLine": source_definition["definitionSourceLine"],
            "formalParameterOrdinals": _macro_parameter_ordinals(source_definition),
            "emissionStatementTemplates": [
                _canonical_macro_emission_statement(statement)
                for statement in _macro_emission_statements(
                    macro_catalog,
                    macro=source_macro,
                    arguments=[
                        f"\\{ordinal}" for ordinal in _macro_parameter_ordinals(source_definition)
                    ],
                )
            ],
            "engineCatalog": engine_catalog,
            "serviceTarget": service_target,
        }
        vocabulary[source_macro.lower()] = {"family": family, "definitionId": definition_id}
    return definitions, vocabulary


def _raw_operation_family(operation: dict[str, Any]) -> tuple[str, str | None]:
    """Classify an assembly operation without extending macro vocabulary by guesswork."""
    if operation["mnemonic"] == "dc":
        return "data-directive", None
    if operation["controlFlowKind"] != "ordinary":
        return "raw-68000-control-flow", None
    return "raw-68000-instruction", None


def _source_line_operation_name(statement: str) -> str | None:
    """Parse a source operation line without mistaking a label or comment for one."""
    label_match = _PROGRAM_LABEL.fullmatch(statement)
    if label_match is not None:
        statement = label_match.group(2).strip()
    elif (inline_label := _PROGRAM_LABEL.match(statement)) is not None:
        statement = inline_label.group(2).strip()
    if not statement:
        return None
    match = _PROGRAM_OPERATION.fullmatch(statement)
    if match is None:
        return None
    return match.group("mnemonic") + (match.group("suffix") or "")


def _payload_context_contract(
    disasm: Path,
    programs_by_category: dict[str, list[dict[str, Any]]],
    *,
    payload_context_specs: dict[str, dict[str, str]],
    action_command_macros: set[str],
) -> tuple[list[dict[str, Any]], dict[str, str]]:
    """Retain nested entity-action payload boundaries from the source stream."""
    terminator_family_sets: dict[str, set[str]] = {}
    for spec in payload_context_specs.values():
        terminator_family_sets.setdefault(spec["terminatorMnemonic"], set()).add(
            spec["contextFamily"]
        )
    if any(len(families) != 1 for families in terminator_family_sets.values()):
        raise ValueError("map event payload terminator family ambiguity")
    terminator_families = {
        macro: next(iter(families)) for macro, families in terminator_family_sets.items()
    }
    operations_by_path_line: dict[tuple[str, int], list[tuple[str, dict[str, Any]]]] = {}
    for category, programs in programs_by_category.items():
        for program in programs:
            for operation in program["operations"]:
                key = (program["sourcePath"], operation["sourceLine"])
                operations_by_path_line.setdefault(key, []).append((category, operation))

    all_contexts: dict[str, dict[str, Any]] = {}
    payload_macro_families: dict[str, str] = {}

    def retain_payload_macro_family(macro: str, context_family: str, source_location: str) -> None:
        """Keep only non-dispatch payload macros in the source-shaped vocabulary family."""
        if macro in action_command_macros:
            return
        if context_family != "entity-action-payload":
            raise ValueError(
                f"map event inline action payload command identity drift: {source_location}"
            )
        operation_family = "entity-action-payload-command"
        previous_family = payload_macro_families.setdefault(macro, operation_family)
        if previous_family != operation_family:
            raise ValueError(f"map event action payload family drift: {source_location}")

    for source_path in sorted({path for path, _ in operations_by_path_line}):
        stack: list[str] = []
        lines = read_upstream_text(disasm / source_path).splitlines()
        for source_line, raw_line in enumerate(lines, start=1):
            macro = _source_line_operation_name(_normalise_asm_statement(raw_line))
            if macro is None:
                continue
            active_contexts = list(stack)
            key = (source_path, source_line)
            source_location = f"{source_path}:{source_line}"
            for _, operation in operations_by_path_line.get(key, []):
                operation["payloadContextIds"] = active_contexts
                if macro in action_command_macros and (
                    not stack
                    or all_contexts[stack[-1]]["contextFamily"]
                    != "entity-action-command-payload"
                ):
                    raise ValueError(
                        f"map event entity-action command context drift: {source_location}"
                    )
            if macro in payload_context_specs:
                spec = payload_context_specs[macro]
                context_family = spec["contextFamily"]
                terminator = spec["terminatorMnemonic"]
                context_id = f"{source_path}:{source_line}:{context_family}"
                if context_id in all_contexts:
                    raise ValueError(f"map event duplicate action payload context: {context_id}")
                all_contexts[context_id] = {
                    "contextId": context_id,
                    "sourcePath": source_path,
                    "openerSourceLine": source_line,
                    "openerSourceMnemonic": macro,
                    "contextFamily": context_family,
                    "parentContextId": stack[-1] if stack else None,
                    "terminatorMnemonic": terminator,
                    "terminatorSourceLine": None,
                }
                stack.append(context_id)
            elif macro in terminator_families:
                if not stack:
                    raise ValueError(
                        f"map event action payload terminator lacks opener: {source_location}"
                    )
                context = all_contexts[stack[-1]]
                if macro != context["terminatorMnemonic"]:
                    raise ValueError(
                        f"map event action payload terminator kind drift: {source_location}"
                    )
                retain_payload_macro_family(
                    macro, terminator_families[macro], source_location
                )
                context["terminatorSourceLine"] = source_line
                stack.pop()
            elif stack:
                context_family = all_contexts[stack[-1]]["contextFamily"]
                if macro in action_command_macros:
                    if context_family != "entity-action-command-payload":
                        raise ValueError(
                            f"map event entity-action command context drift: {source_location}"
                        )
                else:
                    retain_payload_macro_family(macro, context_family, source_location)
        if stack:
            raise ValueError(f"map event action payload context lacks terminator: {source_path}")

    referenced_ids = {
        context_id
        for operations in operations_by_path_line.values()
        for _, operation in operations
        for context_id in operation.get("payloadContextIds", [])
    }
    for category, programs in programs_by_category.items():
        del category
        for program in programs:
            context_ids = [
                context_id
                for operation in program["operations"]
                for context_id in operation.get("payloadContextIds", [])
            ]
            program["payloadContextIds"] = list(dict.fromkeys(context_ids))
            program["inheritedPayloadContextIds"] = [
                context_id
                for context_id in program["payloadContextIds"]
                if all_contexts[context_id]["openerSourceLine"] < program["entrySourceLine"]
            ]
    return (
        [context for context_id, context in all_contexts.items() if context_id in referenced_ids],
        payload_macro_families,
    )


def _guard_macro_emission(
    listing_lines: list[str],
    macro_catalog: dict[str, dict[str, Any]],
    *,
    operation: dict[str, Any],
    next_address: int,
) -> None:
    """Require each macro use's emitted leaf order to match H1 before fixtures."""
    source_macro = operation["sourceMnemonic"]
    expected = [
        _canonical_macro_emission_statement(statement)
        for statement in _macro_emission_statements(
            macro_catalog,
            macro=source_macro,
            arguments=operation["operandTexts"],
        )
    ]
    actual_rows = _listing_macro_emission_rows(
        listing_lines, source_listing_index=operation["_h1ListingSourceIndex"]
    )
    actual = [_canonical_macro_emission_statement(row["statement"]) for row in actual_rows]
    if actual != expected:
        raise ValueError(
            "map event source macro emission statement/order drift: "
            f"{operation['sourceLine']}:{source_macro}"
        )
    if not actual_rows or actual_rows[0]["address"] != operation["address"]:
        raise ValueError(f"map event source macro emission address drift: {source_macro}")
    emitted_end = actual_rows[-1]["address"] + actual_rows[-1]["byteCount"]
    if emitted_end != next_address:
        raise ValueError(f"map event source macro emission span drift: {source_macro}")


def _join_operation_vocabulary(
    listing_lines: list[str],
    macro_catalog: dict[str, dict[str, Any]],
    definitions: dict[str, dict[str, Any]],
    vocabulary: dict[str, dict[str, str]],
    programs_by_category: dict[str, list[dict[str, Any]]],
) -> dict[str, Any]:
    """Join every physical operation to a source-faithful family and definition."""
    categories = tuple(programs_by_category)
    mnemonic_rows: dict[str, dict[str, Any]] = {}
    family_counts: Counter[str] = Counter()
    weighted_family_counts: dict[str, Counter[str]] = {
        "physicalRecordCount": Counter(),
        "setupRecordReferenceCount": Counter(),
        "routeRecordReferenceCount": Counter(),
    }
    category_counts = {category: Counter() for category in categories}
    for category, programs in programs_by_category.items():
        for program in programs:
            reference_counts = program["referenceCounts"]
            per_program_weights = Counter()
            for index, operation in enumerate(program["operations"]):
                source_macro = operation["sourceMnemonic"]
                if source_macro in macro_catalog:
                    vocabulary_row = vocabulary.get(operation["mnemonic"])
                    if vocabulary_row is None:
                        raise ValueError(f"map event macro vocabulary join drift: {source_macro}")
                    family = vocabulary_row["family"]
                    definition_id = vocabulary_row["definitionId"]
                    definition = definitions[definition_id]
                    if operation["operandTexts"] and max(
                        definition["formalParameterOrdinals"], default=0
                    ) != len(operation["operandTexts"]):
                        raise ValueError(
                            "map event macro operand count/order drift: "
                            f"{source_macro}:{operation['sourceLine']}"
                        )
                    if not operation["operandTexts"] and definition["formalParameterOrdinals"]:
                        raise ValueError(
                            "map event macro operand omission drift: "
                            f"{source_macro}:{operation['sourceLine']}"
                        )
                    next_address = (
                        program["operations"][index + 1]["address"]
                        if index + 1 < len(program["operations"])
                        else program["endAddressExclusive"]
                    )
                    _guard_macro_emission(
                        listing_lines,
                        macro_catalog,
                        operation=operation,
                        next_address=next_address,
                    )
                else:
                    family, definition_id = _raw_operation_family(operation)
                operation["family"] = family
                operation["definitionId"] = definition_id
                if "payloadContextIds" not in operation:
                    operation["payloadContextIds"] = []
                del operation["_h1ListingSourceIndex"]
                key = operation["mnemonic"]
                row = mnemonic_rows.setdefault(
                    key,
                    {
                        "mnemonic": key,
                        "family": family,
                        "definitionId": definition_id,
                        "categoryOperationCounts": {name: 0 for name in categories},
                        "weightCounts": {
                            "uniquePhysicalOperationCount": 0,
                            "physicalRecordWeightedOperationCount": 0,
                            "setupRecordReferenceWeightedOperationCount": 0,
                            "routeRecordReferenceWeightedOperationCount": 0,
                        },
                    },
                )
                if row["family"] != family or row["definitionId"] != definition_id:
                    raise ValueError(f"map event mnemonic family/definition ambiguity: {key}")
                row["categoryOperationCounts"][category] += 1
                row["weightCounts"]["uniquePhysicalOperationCount"] += 1
                row["weightCounts"]["physicalRecordWeightedOperationCount"] += reference_counts[
                    "physicalRecordCount"
                ]
                row["weightCounts"]["setupRecordReferenceWeightedOperationCount"] += (
                    reference_counts["setupRecordReferenceCount"]
                )
                row["weightCounts"]["routeRecordReferenceWeightedOperationCount"] += (
                    reference_counts["routeRecordReferenceCount"]
                )
                family_counts[family] += 1
                category_counts[category][family] += 1
                per_program_weights["uniquePhysicalOperationCount"] += 1
                per_program_weights["physicalRecordWeightedOperationCount"] += reference_counts[
                    "physicalRecordCount"
                ]
                per_program_weights["setupRecordReferenceWeightedOperationCount"] += (
                    reference_counts["setupRecordReferenceCount"]
                )
                per_program_weights["routeRecordReferenceWeightedOperationCount"] += (
                    reference_counts["routeRecordReferenceCount"]
                )
                for weight_name, source_name in (
                    ("physicalRecordCount", "physicalRecordCount"),
                    ("setupRecordReferenceCount", "setupRecordReferenceCount"),
                    ("routeRecordReferenceCount", "routeRecordReferenceCount"),
                ):
                    weighted_family_counts[weight_name][family] += reference_counts[source_name]
            program["operationWeightCounts"] = dict(per_program_weights)
    family_order = sorted(family_counts)
    for row in mnemonic_rows.values():
        if set(row["categoryOperationCounts"]) != set(categories):
            raise ValueError("map event mnemonic category coverage drift")
    vocabulary_rows = sorted(mnemonic_rows.values(), key=lambda row: row["mnemonic"])
    family_rows = [
        {
            "family": family,
            "categoryOperationCounts": {
                category: category_counts[category][family] for category in categories
            },
            "weightCounts": {
                "uniquePhysicalOperationCount": family_counts[family],
                "physicalRecordWeightedOperationCount": weighted_family_counts[
                    "physicalRecordCount"
                ][family],
                "setupRecordReferenceWeightedOperationCount": weighted_family_counts[
                    "setupRecordReferenceCount"
                ][family],
                "routeRecordReferenceWeightedOperationCount": weighted_family_counts[
                    "routeRecordReferenceCount"
                ][family],
            },
        }
        for family in family_order
    ]
    category_operation_counts = {
        category: sum(row["categoryOperationCounts"][category] for row in vocabulary_rows)
        for category in categories
    }
    weight_totals = {
        name: sum(row["weightCounts"][name] for row in family_rows)
        for name in (
            "uniquePhysicalOperationCount",
            "physicalRecordWeightedOperationCount",
            "setupRecordReferenceWeightedOperationCount",
            "routeRecordReferenceWeightedOperationCount",
        )
    }
    if category_operation_counts != {
        category: sum(len(program["operations"]) for program in programs)
        for category, programs in programs_by_category.items()
    }:
        raise ValueError("map event operation category-total reconciliation drift")
    if weight_totals["uniquePhysicalOperationCount"] != sum(category_operation_counts.values()):
        raise ValueError("map event operation unique-weight reconciliation drift")
    for weight_name in weight_totals:
        if weight_totals[weight_name] != sum(
            row["weightCounts"][weight_name] for row in vocabulary_rows
        ):
            raise ValueError("map event operation vocabulary-weight reconciliation drift")
    definition_join_counts = {
        family: sum(definition["family"] == family for definition in definitions.values())
        for family in family_order
    }
    return {
        "operationVocabularySummary": {
            "uniqueMnemonicCount": len(vocabulary_rows),
            "definitionJoinCount": len(definitions),
            "unclassifiedOperationCount": 0,
            "ambiguousMnemonicFamilyDefinitionCount": 0,
            "categoryPhysicalOperationCounts": category_operation_counts,
            "definitionJoinCounts": definition_join_counts,
            "weightCounts": weight_totals,
        },
        "operationVocabulary": vocabulary_rows,
        "operationVocabularyOrder": [
            f"{row['mnemonic']}:{row['family']}:{row['definitionId'] or '-'}:"
            f"{row['categoryOperationCounts']['entityEvents']}:"
            f"{row['categoryOperationCounts']['zoneEvents']}:"
            f"{row['categoryOperationCounts']['itemEvents']}:"
            f"{row['weightCounts']['uniquePhysicalOperationCount']}:"
            f"{row['weightCounts']['physicalRecordWeightedOperationCount']}:"
            f"{row['weightCounts']['setupRecordReferenceWeightedOperationCount']}:"
            f"{row['weightCounts']['routeRecordReferenceWeightedOperationCount']}"
            for row in vocabulary_rows
        ],
        "operationFamilyOrder": family_order,
        "operationFamilyCounts": family_rows,
        "operationFamilyCountOrder": [
            json.dumps(row, ensure_ascii=False, separators=(",", ":")) for row in family_rows
        ],
    }


def _reconcile_operation_weight_contract(
    programs_by_category: dict[str, list[dict[str, Any]]],
    operation_contract: dict[str, Any],
) -> None:
    """Recompute all semantic operation weights from source-bound program references."""
    categories = tuple(programs_by_category)
    weight_sources = {
        "uniquePhysicalOperationCount": None,
        "physicalRecordWeightedOperationCount": "physicalRecordCount",
        "setupRecordReferenceWeightedOperationCount": "setupRecordReferenceCount",
        "routeRecordReferenceWeightedOperationCount": "routeRecordReferenceCount",
    }
    category_counts = {category: 0 for category in categories}
    total_weights = {name: 0 for name in weight_sources}
    family_counts: dict[str, dict[str, Any]] = {}
    vocabulary_counts: dict[str, dict[str, Any]] = {}
    for category, programs in programs_by_category.items():
        for program in programs:
            expected_weights = {name: 0 for name in weight_sources}
            for operation in program["operations"]:
                family = operation["family"]
                mnemonic = operation["mnemonic"]
                definition_id = operation["definitionId"]
                category_counts[category] += 1
                vocabulary = vocabulary_counts.setdefault(
                    mnemonic,
                    {
                        "family": family,
                        "definitionId": definition_id,
                        "categoryOperationCounts": {name: 0 for name in categories},
                        "weightCounts": {name: 0 for name in weight_sources},
                    },
                )
                if (
                    vocabulary["family"] != family
                    or vocabulary["definitionId"] != definition_id
                ):
                    raise ValueError("map event operation mnemonic join reconciliation drift")
                family_row = family_counts.setdefault(
                    family,
                    {
                        "categoryOperationCounts": {name: 0 for name in categories},
                        "weightCounts": {name: 0 for name in weight_sources},
                    },
                )
                for weight_name, reference_field in weight_sources.items():
                    amount = (
                        1
                        if reference_field is None
                        else program["referenceCounts"][reference_field]
                    )
                    expected_weights[weight_name] += amount
                    total_weights[weight_name] += amount
                    vocabulary["weightCounts"][weight_name] += amount
                    family_row["weightCounts"][weight_name] += amount
                vocabulary["categoryOperationCounts"][category] += 1
                family_row["categoryOperationCounts"][category] += 1
            if program["operationWeightCounts"] != expected_weights:
                raise ValueError("map event operation program weight reconciliation drift")
    expected_vocabulary = [
        {
            "mnemonic": mnemonic,
            **row,
        }
        for mnemonic, row in sorted(vocabulary_counts.items())
    ]
    if operation_contract["operationVocabulary"] != expected_vocabulary:
        raise ValueError("map event operation vocabulary reconciliation drift")
    expected_families = [
        {
            "family": family,
            **row,
        }
        for family, row in sorted(family_counts.items())
    ]
    if operation_contract["operationFamilyCounts"] != expected_families:
        raise ValueError("map event operation family reconciliation drift")
    summary = operation_contract["operationVocabularySummary"]
    if summary["categoryPhysicalOperationCounts"] != category_counts:
        raise ValueError("map event operation category summary reconciliation drift")
    if summary["weightCounts"] != total_weights:
        raise ValueError("map event operation weight summary reconciliation drift")


def _relative_target_expression(expression: str, table_symbol: str) -> dict[str, Any]:
    """Parse the source expression whose signed word is decoded from the table base."""
    compact = re.sub(r"\s+", "", expression)
    masked_to_16_bits = False
    mask_match = re.fullmatch(r"\((.+)\)&\$FFFF", compact, re.IGNORECASE)
    if mask_match is not None:
        compact = mask_match.group(1)
        masked_to_16_bits = True
    target_match = re.fullmatch(
        rf"(?P<symbol>[A-Za-z_][A-Za-z0-9_]*)(?P<adjustment>[+-](?:\$[0-9A-Fa-f]+|\d+))?"
        rf"-(?P<base>{re.escape(table_symbol)})",
        compact,
    )
    if target_match is None:
        raise ValueError(
            f"map event target expression does not resolve from its table base: {expression!r}"
        )
    adjustment_text = target_match.group("adjustment")
    adjustment = 0
    if adjustment_text:
        sign = -1 if adjustment_text.startswith("-") else 1
        token = adjustment_text[1:]
        adjustment = sign * (int(token[1:], 16) if token.startswith("$") else int(token))
    return {
        "targetExpression": expression,
        "targetBaseSymbol": target_match.group("symbol"),
        "targetBaseAdjustment": adjustment,
        "relativeBaseSymbol": target_match.group("base"),
        "maskedTo16Bits": masked_to_16_bits,
    }


def _event_macro_use_sites(
    source: str,
    *,
    category: str,
    path: str,
    table_symbol: str,
    definitions: list[dict[str, Any]],
) -> list[dict[str, Any]]:
    """Parse exact event-macro source rows without matching comments or near-miss names."""
    by_macro = {definition["macro"]: definition for definition in definitions}
    sites: list[dict[str, Any]] = []
    for line_number, raw_line in enumerate(source.splitlines(), start=1):
        line = raw_line.split(";", 1)[0].strip()
        line = re.sub(r"^[A-Za-z_][A-Za-z0-9_]*:\s*", "", line).strip()
        if not line:
            continue
        match = re.fullmatch(r"([A-Za-z_][A-Za-z0-9_]*)\s+(.+)", line)
        if match is None or match.group(1) not in by_macro:
            continue
        macro = match.group(1)
        definition = by_macro[macro]
        operands = _split_macro_operands(match.group(2))
        if len(operands) != definition["argumentCount"]:
            raise ValueError(f"map event macro operand count drift: {path}:{line_number}")
        expression = operands[definition["targetOperandPosition"] - 1]
        sites.append(
            {
                "sourceOrder": len(sites),
                "sourcePath": path,
                "sourceLine": line_number,
                "sourceTableSymbol": table_symbol,
                "macro": macro,
                "kind": definition["kind"],
                "operandTexts": operands,
                "sourceDefaultMarker": definition["defaultMarker"],
                "sourceMarkerWord": None,
                **_relative_target_expression(expression, table_symbol),
            }
        )
    if category == "zoneEvents" and table_symbol == RAW_ZONE_DEFAULT_SYMBOL:
        raw_rows = [
            (line_number, raw_line.split(";", 1)[0].strip())
            for line_number, raw_line in enumerate(source.splitlines(), start=1)
            if raw_line.split(";", 1)[0].strip()
        ]
        if len(raw_rows) < 3:
            raise ValueError("map 44 raw zone-default marker is missing")
        marker_match = re.fullmatch(r"dc\.w\s+\$([0-9A-Fa-f]{1,4})", raw_rows[1][1])
        if marker_match is None:
            raise ValueError("map 44 raw zone-default marker form drift")
        marker_word = int(marker_match.group(1), 16)
        marker_operand = f"${marker_match.group(1)}"
        target_line, target_statement = raw_rows[2]
        raw_match = re.fullmatch(r"dc\.w\s+(.+)", target_statement)
        if raw_match is None:
            raise ValueError("map 44 raw zone-default target expression drift")
        sites.append(
            {
                "sourceOrder": len(sites),
                "sourcePath": path,
                "sourceLine": target_line,
                "sourceTableSymbol": table_symbol,
                "macro": "raw-zone-default-expression",
                "kind": "default",
                "operandTexts": [marker_operand, raw_match.group(1)],
                "sourceDefaultMarker": marker_word >> 8,
                "sourceMarkerWord": marker_word,
                **_relative_target_expression(raw_match.group(1), table_symbol),
            }
        )
    return sites


def _decode_event_record(
    category: str, table_address: int, record_address: int, data: bytes
) -> dict[str, Any]:
    expected_size = CATEGORY_CONFIG[category]["recordBytes"]
    if len(data) != expected_size:
        raise ValueError(f"{category} record must contain {expected_size} bytes")
    relative_offset = int.from_bytes(data[-2:], "big", signed=True)
    record: dict[str, Any] = {
        "address": record_address,
        "kind": "default" if data[0] == 0xFD else "specific",
        "relativeOffset": relative_offset,
        "resolvedTargetAddress": table_address + relative_offset,
    }
    if category == "entityEvents":
        record.update({"entity": data[0], "flags": data[1]})
    elif category == "zoneEvents":
        record.update({"x": data[0], "y": data[1]})
    elif category == "itemEvents":
        record.update({"x": data[0], "y": data[1], "facing": data[2], "item": data[3]})
    else:
        raise ValueError(f"unknown map event category: {category}")
    return record


def _instruction_tokens(source: str) -> list[str]:
    tokens: list[str] = []
    for raw_line in source.splitlines():
        line = raw_line.split(";", 1)[0].strip()
        line = re.sub(r"^[A-Za-z_][A-Za-z0-9_]*:\s*", "", line).strip()
        if line:
            tokens.append(line)
    return tokens


def _event_matches(category: str, record: dict[str, Any], query: dict[str, int]) -> bool:
    if record["kind"] == "default":
        return True
    if category == "entityEvents":
        return record["entity"] == (query["entity"] & 0xFF)
    if category == "zoneEvents":
        return all(
            record[field] == 0xFF or record[field] == (query[field] & 0xFF) for field in ("x", "y")
        )
    if category == "itemEvents":
        coordinates_match = all(
            record[field] == 0xFF or record[field] == (query[field] & 0xFF)
            for field in ("x", "y", "facing")
        )
        return coordinates_match and record["item"] == (query["item"] & 0x7F)
    raise ValueError(f"unknown map event category: {category}")


def _selected_setup_symbol(
    setup: dict[str, Any], map_index: int, set_flags: set[int]
) -> str | None:
    route = next((row for row in setup["routes"] if row["map"] == map_index), None)
    if route is None:
        return None
    selected = route["defaultPointer"]
    for variant in route["flagVariants"]:
        if variant["flag"] in set_flags:
            selected = variant["pointer"]
    return selected


def _selection_cases(
    setup: dict[str, Any], categories: dict[str, dict[str, Any]]
) -> list[dict[str, Any]]:
    pointer_tables = {row["symbol"]: row for row in setup["pointerTables"]}
    event_tables = {
        category: {row["symbol"]: row for row in value["tables"]}
        for category, value in categories.items()
    }
    cases: list[dict[str, Any]] = []
    for case_id, category, map_index, flags, query in SELECTION_INPUTS:
        setup_symbol = _selected_setup_symbol(setup, map_index, set(flags))
        if setup_symbol is None:
            raise ValueError(f"selection case unexpectedly uses a missing map: {case_id}")
        table_symbol = pointer_tables[setup_symbol]["targets"][category]["symbol"]
        table = event_tables[category].get(table_symbol)
        if table is None:
            raise ValueError(f"selection case uses a direct-return event stub: {case_id}")
        selected = next(
            (row for row in table["records"] if _event_matches(category, row, query)),
            None,
        )
        if selected is None:
            raise ValueError(f"selection case has no default record: {case_id}")
        cases.append(
            {
                "id": case_id,
                "category": category,
                "map": map_index,
                "setFlags": list(flags),
                "query": query,
                "selectedSetup": setup_symbol,
                "selectedTable": table_symbol,
                "selectedRecordAddress": selected["address"],
                "selectedRecordKind": selected["kind"],
                "eventFlags": selected.get("flags"),
                "resolvedTargetAddress": selected["resolvedTargetAddress"],
            }
        )
    return cases


def _source_rows(
    disasm: Path,
    addresses: dict[str, int],
    category: str,
    definitions: list[dict[str, Any]],
) -> tuple[list[dict[str, Any]], dict[int, dict[str, Any]]]:
    config = CATEGORY_CONFIG[category]
    paths = sorted(
        (
            path
            for path in (disasm / SOURCE_ROOT).rglob(config["glob"])
            if "mapsetups" in path.parts
        ),
        key=lambda path: path.as_posix(),
    )
    files: list[dict[str, Any]] = []
    source_records: dict[int, dict[str, Any]] = {}
    for path in paths:
        source = read_upstream_text(path)
        relative_path = path.relative_to(disasm).as_posix()
        labels = re.findall(r"^([A-Za-z_][A-Za-z0-9_]*):", source, re.MULTILINE)
        if not labels or labels[0] not in addresses:
            raise ValueError(f"{category} source has no H1-bound entry label: {path}")
        symbol = labels[0]
        address = addresses[symbol]
        is_stub = symbol in config["stubSymbols"]
        if is_stub and _instruction_tokens(source) != ["rts"]:
            raise ValueError(f"{category} direct-return stub shape drift: {symbol}")

        use_sites = _event_macro_use_sites(
            source,
            category=category,
            path=relative_path,
            table_symbol=symbol,
            definitions=definitions,
        )
        kinds = [site["kind"] for site in use_sites]
        macro_counts: Counter[str] = Counter(site["macro"] for site in use_sites)
        is_raw_default = category == "zoneEvents" and symbol == RAW_ZONE_DEFAULT_SYMBOL
        if is_raw_default and [site["macro"] for site in use_sites] != [
            "raw-zone-default-expression"
        ]:
            raise ValueError("map 44 raw zone-default exception shape drift")
        if is_stub and use_sites:
            raise ValueError(f"direct-return stub unexpectedly owns table records: {symbol}")
        if not is_stub and (not kinds or kinds[-1] != "default"):
            raise ValueError(f"{category} table lacks a final default record: {symbol}")

        source_order_start = len(source_records)
        for index, site in enumerate(use_sites):
            record_address = address + index * config["recordBytes"]
            if record_address in source_records:
                raise ValueError(
                    f"overlapping source-owned map event record at 0x{record_address:X}"
                )
            source_records[record_address] = {
                **site,
                "recordSourceOrder": len(source_records),
                "tableRecordIndex": index,
                "recordAddress": record_address,
            }
        files.append(
            {
                "sourceOrder": len(files),
                "path": relative_path,
                "symbol": symbol,
                "address": address,
                "recordCount": len(kinds),
                "encodedRecordBytes": len(kinds) * config["recordBytes"],
                "recordSpanStartAddress": address if kinds else None,
                "recordSpanEndAddressExclusive": (
                    address + len(kinds) * config["recordBytes"] if kinds else None
                ),
                "recordSourceOrderStart": source_order_start if kinds else None,
                "recordSourceOrderEndInclusive": (len(source_records) - 1 if kinds else None),
                "specificRecordCount": kinds.count("specific"),
                "defaultRecordCount": kinds.count("default"),
                "macroCounts": dict(sorted(macro_counts.items())),
                "directReturnStub": is_stub,
                "rawDefaultException": is_raw_default,
            }
        )
    return files, source_records


def _join_source_rom_record(
    category: str,
    decoded: dict[str, Any],
    source_record: dict[str, Any],
    addresses: dict[str, int],
) -> dict[str, Any]:
    """Guard the source operand/ROM-relative-target relationship for one record."""
    if source_record["kind"] != decoded["kind"]:
        raise ValueError(f"{category} source/ROM record kind drift at 0x{decoded['address']:X}")
    if source_record["recordAddress"] != decoded["address"]:
        raise ValueError(f"{category} source/ROM record address drift at 0x{decoded['address']:X}")
    source_marker = source_record["sourceDefaultMarker"]
    if decoded["kind"] == "default":
        if source_marker is None:
            raise ValueError(f"{category} source default marker is missing")
        decoded_marker = decoded["entity"] if category == "entityEvents" else decoded["x"]
        if source_marker != decoded_marker:
            raise ValueError(
                f"{category} source/ROM default marker relationship drift at "
                f"0x{decoded['address']:X}"
            )
    elif source_marker is not None:
        raise ValueError(f"{category} specific source unexpectedly declares default marker")
    source_marker_word = source_record["sourceMarkerWord"]
    if source_marker_word is not None and (
        category != "zoneEvents" or source_marker_word != ((decoded["x"] << 8) | decoded["y"])
    ):
        raise ValueError(f"{category} raw source marker word/ROM relationship drift")
    target_base = source_record["targetBaseSymbol"]
    if target_base not in addresses:
        raise ValueError(f"{category} source target lacks H1 base label: {target_base}")
    source_target_address = addresses[target_base] + source_record["targetBaseAdjustment"]
    if source_target_address != decoded["resolvedTargetAddress"]:
        raise ValueError(
            f"{category} source/ROM target relationship drift at 0x{decoded['address']:X}"
        )
    return {**decoded, **source_record, "category": category}


def _category_contract(
    disasm: Path,
    addresses: dict[str, int],
    rom: bytes,
    setup: dict[str, Any],
    category: str,
    definitions: list[dict[str, Any]],
) -> dict[str, Any]:
    config = CATEGORY_CONFIG[category]
    files, source_records = _source_rows(disasm, addresses, category, definitions)
    targets = [table["targets"][category] for table in setup["pointerTables"]]
    target_counts = Counter(target["symbol"] for target in targets)
    unique_targets = {target["symbol"]: target["address"] for target in targets}
    if set(unique_targets) != {row["symbol"] for row in files}:
        raise ValueError(f"map setup pointers do not own the complete {category} source boundary")

    source_by_symbol = {row["symbol"]: row for row in files}
    tables: list[dict[str, Any]] = []
    physical_records: dict[int, dict[str, Any]] = {}
    for symbol, address in sorted(unique_targets.items()):
        source_row = source_by_symbol[symbol]
        if source_row["directReturnStub"]:
            if rom[address : address + 2] != b"\x4e\x75":
                raise ValueError(f"{category} direct-return stub ROM drift: {symbol}")
            continue
        records: list[dict[str, Any]] = []
        cursor = address
        while True:
            raw = rom[cursor : cursor + config["recordBytes"]]
            if len(raw) != config["recordBytes"] or len(records) >= 48:
                raise ValueError(f"{category} table has no bounded default record: {symbol}")
            decoded = _decode_event_record(category, address, cursor, raw)
            source_record = source_records.get(cursor)
            if source_record is None:
                raise ValueError(f"{category} source/ROM record drift at 0x{cursor:X}")
            if cursor in physical_records:
                raise ValueError(f"{category} physical records overlap at 0x{cursor:X}")
            joined = _join_source_rom_record(category, decoded, source_record, addresses)
            physical_records[cursor] = joined
            records.append(joined)
            cursor += config["recordBytes"]
            if decoded["kind"] == "default":
                break
        if len(records) != source_row["recordCount"]:
            raise ValueError(f"{category} source/ROM table length drift: {symbol}")
        tables.append(
            {
                "symbol": symbol,
                "address": address,
                "sourcePath": source_row["path"],
                "directReturnStub": False,
                "recordCount": len(records),
                "encodedRecordBytes": len(records) * config["recordBytes"],
                "recordSpanStartAddress": records[0]["address"],
                "recordSpanEndAddressExclusive": cursor,
                "recordSourceOrderStart": records[0]["recordSourceOrder"],
                "recordSourceOrderEndInclusive": records[-1]["recordSourceOrder"],
                "records": records,
            }
        )
    if set(physical_records) != set(source_records):
        raise ValueError(f"{category} source records are not exactly covered by setup tables")

    physical_kinds = Counter(record["kind"] for record in physical_records.values())
    setup_kinds: Counter[str] = Counter()
    table_by_symbol = {row["symbol"]: row for row in tables}
    for target in targets:
        table = table_by_symbol.get(target["symbol"])
        if table is not None:
            setup_kinds.update(record["kind"] for record in table["records"])
    source_macro_counts: Counter[str] = Counter()
    for row in files:
        source_macro_counts.update(row["macroCounts"])
    summary = {
        "sourceFileCount": len(files),
        "setupPointerReferenceCount": len(targets),
        "uniqueTargetCount": len(unique_targets),
        "decodedTableCount": len(tables),
        "aliasedTargetCount": sum(count > 1 for count in target_counts.values()),
        "physicalRecordCount": len(physical_records),
        "specificPhysicalRecordCount": physical_kinds["specific"],
        "defaultPhysicalRecordCount": physical_kinds["default"],
        "setupRecordReferenceCount": sum(setup_kinds.values()),
        "specificSetupRecordReferenceCount": setup_kinds["specific"],
        "defaultSetupRecordReferenceCount": setup_kinds["default"],
        "directReturnStubCount": sum(row["directReturnStub"] for row in files),
        "directReturnStubReferenceCount": sum(
            target_counts[row["symbol"]] for row in files if row["directReturnStub"]
        ),
        "rawDefaultExceptionCount": sum(row["rawDefaultException"] for row in files),
        "maximumTableRecordCount": max(row["recordCount"] for row in tables),
    }
    return {
        "summary": summary,
        "sourceMacroCounts": dict(sorted(source_macro_counts.items())),
        "duplicatePointerTargets": [
            {"symbol": symbol, "setupReferenceCount": count}
            for symbol, count in sorted(target_counts.items())
            if count > 1
        ],
        "sourceFiles": files,
        "tables": tables,
    }


def _source_label_owners(
    disasm: Path, addresses: dict[str, int]
) -> dict[int, list[dict[str, Any]]]:
    """Index every source/H1 label once, retaining same-address aliases."""
    owners: dict[int, list[dict[str, Any]]] = {}
    for path in sorted(disasm.rglob("*.asm")):
        relative_path = path.relative_to(disasm).as_posix()
        for line_number, line in enumerate(read_upstream_text(path).splitlines(), start=1):
            match = re.match(r"^\s*([A-Za-z_][A-Za-z0-9_]*):", line)
            if match is None:
                continue
            symbol = match.group(1)
            address = addresses.get(symbol)
            if address is None:
                continue
            owners.setdefault(address, []).append(
                {
                    "symbol": symbol,
                    "sourcePath": relative_path,
                    "sourceLine": line_number,
                }
            )
    for address in owners:
        owners[address].sort(
            key=lambda owner: (owner["sourcePath"], owner["sourceLine"], owner["symbol"])
        )
    return owners


def _label_owners(
    disasm: Path,
    addresses: dict[str, int],
    wanted_addresses: set[int],
    source_label_owners: dict[int, list[dict[str, Any]]] | None = None,
) -> dict[int, list[dict[str, Any]]]:
    """Select target labels from the reusable complete source/H1 label index."""
    all_owners = source_label_owners or _source_label_owners(disasm, addresses)
    return {address: list(all_owners.get(address, [])) for address in wanted_addresses}


def _program_key(symbol: str, address: int) -> str:
    return f"{symbol}:{address}"


def _target_program_boundary_order(program: dict[str, Any]) -> str:
    """Compactly pin source/H1 program-boundary facts without repeating program schemas."""
    return json.dumps(
        [
            program["canonicalSymbol"],
            program["entryAddress"],
            program["sourcePath"],
            program["entrySourceLine"],
            program["endFunctionSymbol"],
            program["endSourceLine"],
            program["endAddressExclusive"],
            program["encodedSpanBytes"],
            program["referenceCounts"],
        ],
        ensure_ascii=False,
        separators=(",", ":"),
    )


def _target_program_operation_order(operation: dict[str, Any], *, category: str) -> str:
    """Serialize one source/H1 operation's exact category-owned static facts compactly."""
    if category == "entityEvents":
        return f"{operation['sourceOrder']}:{operation['sourceLine']}:{operation['address']}"
    target = operation["target"]
    target_order = (
        None
        if target is None
        else [
            target["instructionTargetSymbol"],
            target["instructionTargetAddress"],
            target["effectiveTargetSymbol"],
            target["effectiveTargetAddress"],
            target["effectiveTargetScope"],
        ]
    )
    operand_texts = operation["operandTexts"]
    if any("|" in operand for operand in operand_texts):
        raise ValueError(
            f"map event target operation signature delimiter drift: {operation['sourceLine']}"
        )
    target_text = "-" if target_order is None else ":".join(str(value) for value in target_order)
    return "|".join(
        (
            str(operation["sourceOrder"]),
            str(operation["sourceLine"]),
            str(operation["address"]),
            operation["sourceMnemonic"],
            operation["mnemonic"],
            operation["sizeSuffix"] or "-",
            ",".join(operand_texts),
            operation["controlFlowKind"],
            target_text,
        )
    )


def _target_program_operation_join_order(
    operation: dict[str, Any],
    *,
    family_indices: dict[str, int],
    definition_indices: dict[str, int],
) -> str:
    """Pin the phase-2 source-family join beside the pre-existing operation signature."""
    payload_context_ids = operation["payloadContextIds"]
    if any("|" in context_id for context_id in payload_context_ids):
        raise ValueError(
            f"map event target operation join delimiter drift: {operation['sourceLine']}"
        )
    return "|".join(
        (
            str(family_indices[operation["family"]]),
            (
                str(definition_indices[operation["definitionId"]])
                if operation["definitionId"] is not None
                else "-"
            ),
            ",".join(payload_context_ids),
        )
    )


def _control_flow_count_field(kind: str) -> str:
    fields = {
        "conditional-branch": "conditionalBranchSiteCount",
        "unconditional-branch": "unconditionalBranchSiteCount",
        "direct-call": "directCallSiteCount",
        "direct-jump": "directJumpSiteCount",
    }
    if kind not in fields:
        raise ValueError(f"map entity-event non-target control-flow kind: {kind}")
    return fields[kind]


def _target_identity(
    symbol: str, addresses: dict[str, int], owners: dict[int, list[dict[str, Any]]]
) -> dict[str, Any]:
    if symbol not in addresses:
        raise ValueError(f"map entity-event control-flow target lacks H1 address: {symbol}")
    address = addresses[symbol]
    labels = owners.get(address, [])
    if not labels:
        raise ValueError(f"map entity-event control-flow target lacks source owner: {symbol}")
    return {"symbol": symbol, "address": address, "addressLabels": labels}


def _target_program_control_flow(
    programs: list[dict[str, Any]],
    alias_definitions: list[dict[str, Any]],
) -> tuple[dict[str, Any], dict[str, Any]]:
    """Build zero-inclusive instruction/effective target totals from parsed operations."""
    instruction_targets: dict[tuple[str, int], dict[str, Any]] = {}
    effective_targets: dict[tuple[str, int], dict[str, Any]] = {}
    counts: Counter[tuple[str, str, tuple[str, int], str]] = Counter()
    for program in programs:
        for operation in program["operations"]:
            target = operation["target"]
            if target is None:
                continue
            field = _control_flow_count_field(operation["controlFlowKind"])
            scope = target["effectiveTargetScope"]
            instruction_identity = (
                target["instructionTargetSymbol"],
                target["instructionTargetAddress"],
            )
            effective_identity = (
                target["effectiveTargetSymbol"],
                target["effectiveTargetAddress"],
            )
            instruction_targets.setdefault(
                instruction_identity,
                {
                    "symbol": instruction_identity[0],
                    "address": instruction_identity[1],
                    "addressLabels": target["instructionTargetAddressLabels"],
                },
            )
            effective_targets.setdefault(
                effective_identity,
                {
                    "symbol": effective_identity[0],
                    "address": effective_identity[1],
                    "addressLabels": target["effectiveTargetAddressLabels"],
                },
            )
            counts[("instruction", scope, instruction_identity, field)] += 1
            counts[("effective", scope, effective_identity, field)] += 1

    def target_rows(
        identity_kind: str,
        scope: str,
        declared_targets: dict[tuple[str, int], dict[str, Any]],
    ) -> list[dict[str, Any]]:
        rows: list[dict[str, Any]] = []
        for identity, target in declared_targets.items():
            row = dict(target)
            for field in _CONTROL_FLOW_COUNT_FIELDS:
                row[field] = counts[(identity_kind, scope, identity, field)]
            row["totalSiteCount"] = sum(row[field] for field in _CONTROL_FLOW_COUNT_FIELDS)
            rows.append(row)
        return rows

    totals = {
        "aliasDefinitions": alias_definitions,
        "targetTotals": {
            "instructionTargets": {
                "internal": target_rows("instruction", "internal", instruction_targets),
                "external": target_rows("instruction", "external", instruction_targets),
            },
            "effectiveTargets": {
                "internal": target_rows("effective", "internal", effective_targets),
                "external": target_rows("effective", "external", effective_targets),
            },
        },
    }

    def total_order(rows: list[dict[str, Any]]) -> list[str]:
        return [
            f"{row['symbol']}:{row['address']}:"
            f"{row['conditionalBranchSiteCount']}:"
            f"{row['unconditionalBranchSiteCount']}:"
            f"{row['directCallSiteCount']}:"
            f"{row['directJumpSiteCount']}"
            for row in rows
        ]

    orders = {
        "aliasOrder": [definition["aliasSymbol"] for definition in alias_definitions],
        "instructionTargetOrder": [
            _program_key(symbol, address) for symbol, address in instruction_targets
        ],
        "effectiveTargetOrder": [
            _program_key(symbol, address) for symbol, address in effective_targets
        ],
        "instructionInternalTargetTotalOrder": total_order(
            totals["targetTotals"]["instructionTargets"]["internal"]
        ),
        "instructionExternalTargetTotalOrder": total_order(
            totals["targetTotals"]["instructionTargets"]["external"]
        ),
        "effectiveInternalTargetTotalOrder": total_order(
            totals["targetTotals"]["effectiveTargets"]["internal"]
        ),
        "effectiveExternalTargetTotalOrder": total_order(
            totals["targetTotals"]["effectiveTargets"]["external"]
        ),
    }
    return totals, orders


def _reconcile_target_programs(
    profiles: list[dict[str, Any]],
    exclusions: list[dict[str, Any]],
    programs: list[dict[str, Any]],
    summary: dict[str, Any],
    control_flow: dict[str, Any],
    target_orders: dict[str, Any],
    *,
    category: str,
) -> None:
    """Reconcile profile weights, source operations, and zero-inclusive target totals."""
    profile_by_identity = {
        (profile["canonicalSymbol"], profile["targetAddress"]): profile for profile in profiles
    }
    program_by_identity = {
        (program["canonicalSymbol"], program["entryAddress"]): program for program in programs
    }
    if set(program_by_identity) != set(profile_by_identity):
        raise ValueError(f"map {category} target program/profile identity coverage drift")
    if len(program_by_identity) != len(programs):
        raise ValueError(f"map {category} duplicate target program identity")

    totals: Counter[str] = Counter()
    observed_control: Counter[tuple[str, str, tuple[str, int], str]] = Counter()
    kind_summary_fields = {
        "conditional-branch": "conditionalBranchCount",
        "unconditional-branch": "unconditionalBranchCount",
        "direct-call": "directCallCount",
        "direct-jump": "directJumpCount",
        "return": "returnCount",
        "ordinary": "ordinaryOperationCount",
    }
    for identity, program in program_by_identity.items():
        profile = profile_by_identity[identity]
        expected_weights = {
            "physicalRecordCount": profile["physicalRecordCount"],
            "setupRecordReferenceCount": profile["setupRecordReferenceCount"],
            "routeRecordReferenceCount": profile["routeRecordReferenceCount"],
        }
        if program["referenceCounts"] != expected_weights:
            raise ValueError(f"map {category} target program reference-count drift: {identity}")
        if program["encodedSpanBytes"] != program["endAddressExclusive"] - program["entryAddress"]:
            raise ValueError(f"map {category} target program span relationship drift: {identity}")
        if program["termination"]["sourceOrder"] != program["operations"][-1]["sourceOrder"]:
            raise ValueError(f"map {category} target termination order drift: {identity}")
        termination = program["termination"]
        if termination["controlFlowKind"] not in {"return", "direct-jump"} and not (
            program["endFunctionSymbol"] is None and termination["sourceMnemonic"] == "csc_end"
        ):
            raise ValueError(f"map {category} target termination kind drift: {identity}")
        totals["programCount"] += 1
        totals["labelCount"] += len(program["labels"])
        totals["operationCount"] += len(program["operations"])
        totals["encodedSpanBytes"] += program["encodedSpanBytes"]
        for field, value in expected_weights.items():
            totals[field] += value
        for operation in program["operations"]:
            kind = operation["controlFlowKind"]
            totals[kind_summary_fields[kind]] += 1
            target = operation["target"]
            if target is None:
                continue
            field = _control_flow_count_field(kind)
            scope = target["effectiveTargetScope"]
            instruction_identity = (
                target["instructionTargetSymbol"],
                target["instructionTargetAddress"],
            )
            effective_identity = (
                target["effectiveTargetSymbol"],
                target["effectiveTargetAddress"],
            )
            observed_control[("instruction", scope, instruction_identity, field)] += 1
            observed_control[("effective", scope, effective_identity, field)] += 1
            totals[f"{scope}ControlFlowSiteCount"] += 1
    totals["sourceFileCount"] = len({program["sourcePath"] for program in programs})
    totals["instructionTargetCount"] = len(target_orders["instructionTargetOrder"])
    totals["effectiveTargetCount"] = len(target_orders["effectiveTargetOrder"])
    totals["jumpInterfaceAliasCount"] = len(control_flow["aliasDefinitions"])
    exclusion_summary_fields = {
        "profileCount",
        "explicitNonProgramExclusionCount",
        "functionEndBoundaryCount",
        "sourceStreamTerminatorCount",
        "excludedPhysicalRecordCount",
        "excludedSetupRecordReferenceCount",
        "excludedRouteRecordReferenceCount",
    }
    base_summary = {
        field: value for field, value in summary.items() if field not in exclusion_summary_fields
    }
    if {field: totals[field] for field in base_summary} != base_summary:
        raise ValueError(f"map {category} target program summary reconciliation drift")

    if "profileCount" in summary:
        excluded_weights = {
            field: sum(exclusion["referenceCounts"][field] for exclusion in exclusions)
            for field in (
                "physicalRecordCount",
                "setupRecordReferenceCount",
                "routeRecordReferenceCount",
            )
        }
        extended_totals = {
            "profileCount": len(profiles) + len(exclusions),
            "explicitNonProgramExclusionCount": len(exclusions),
            "functionEndBoundaryCount": sum(
                program["endFunctionSymbol"] is not None for program in programs
            ),
            "sourceStreamTerminatorCount": sum(
                program["endFunctionSymbol"] is None for program in programs
            ),
            "excludedPhysicalRecordCount": excluded_weights["physicalRecordCount"],
            "excludedSetupRecordReferenceCount": excluded_weights["setupRecordReferenceCount"],
            "excludedRouteRecordReferenceCount": excluded_weights["routeRecordReferenceCount"],
        }
        if {field: summary[field] for field in extended_totals} != extended_totals:
            raise ValueError(f"map {category} target exclusion reconciliation drift")
        for field, excluded_value in excluded_weights.items():
            if summary[field] + excluded_value != sum(profile[field] for profile in profiles) + sum(
                exclusion["referenceCounts"][field] for exclusion in exclusions
            ):
                raise ValueError(f"map {category} target profile weight reconciliation drift")

    target_totals = control_flow["targetTotals"]
    expected_orders = {
        "instructionTargets": target_orders["instructionTargetOrder"],
        "effectiveTargets": target_orders["effectiveTargetOrder"],
    }
    for identity_kind, identity_key in (
        ("instruction", "instructionTargets"),
        ("effective", "effectiveTargets"),
    ):
        for scope in ("internal", "external"):
            rows = target_totals[identity_key][scope]
            observed_order = [_program_key(row["symbol"], row["address"]) for row in rows]
            if observed_order != expected_orders[identity_key]:
                raise ValueError(
                    f"map {category} {identity_kind} target zero-inclusive order drift: {scope}"
                )
            for row in rows:
                identity = (row["symbol"], row["address"])
                for field in _CONTROL_FLOW_COUNT_FIELDS:
                    if row[field] != observed_control[(identity_kind, scope, identity, field)]:
                        raise ValueError(
                            f"map {category} {identity_kind} target total drift: {scope}:{identity}"
                        )
                if row["totalSiteCount"] != sum(row[field] for field in _CONTROL_FLOW_COUNT_FIELDS):
                    raise ValueError(
                        f"map {category} {identity_kind} target aggregate drift: {scope}:{identity}"
                    )
            order_key = f"{identity_kind}{scope.title()}TargetTotalOrder"
            observed_total_order = [
                f"{row['symbol']}:{row['address']}:"
                f"{row['conditionalBranchSiteCount']}:"
                f"{row['unconditionalBranchSiteCount']}:"
                f"{row['directCallSiteCount']}:"
                f"{row['directJumpSiteCount']}"
                for row in rows
            ]
            if target_orders[order_key] != observed_total_order:
                raise ValueError(
                    f"map {category} {identity_kind} target count-order drift: {scope}"
                )


def _target_program_exclusions(
    profiles: list[dict[str, Any]], *, category: str
) -> list[dict[str, Any]]:
    """Retain non-program profiles separately instead of inventing an entry label."""
    exclusions: list[dict[str, Any]] = []
    for profile in profiles:
        if profile["targetH1Address"] is not None:
            continue
        if (
            category != "zoneEvents"
            or profile["canonicalSymbol"] != "raw-map44-zone-default-expression-boundary"
            or profile["ownershipClass"] != "raw-expression-boundary"
            or profile["targetAddressLabels"]
        ):
            raise ValueError(f"map {category} non-program target shape drift")
        exclusions.append(
            {
                "exclusionOrder": len(exclusions),
                "canonicalSymbol": profile["canonicalSymbol"],
                "targetAddress": profile["targetAddress"],
                "targetH1Address": profile["targetH1Address"],
                "targetBaseH1Address": profile["targetBaseH1Address"],
                "targetAddressLabels": profile["targetAddressLabels"],
                "sourcePath": profile["ownerSourcePath"],
                "sourceLine": profile["ownerSourceLine"],
                "ownershipClass": profile["ownershipClass"],
                "referenceCounts": {
                    "physicalRecordCount": profile["physicalRecordCount"],
                    "setupRecordReferenceCount": profile["setupRecordReferenceCount"],
                    "routeRecordReferenceCount": profile["routeRecordReferenceCount"],
                },
            }
        )
    return exclusions


def _target_program_contract(
    disasm: Path,
    addresses: dict[str, int],
    listing_lines: list[str],
    listing_index: dict[str, dict[Any, Any]],
    record_target_profiles: list[dict[str, Any]],
    source_label_owners: dict[int, list[dict[str, Any]]],
    *,
    category: str,
) -> tuple[
    list[dict[str, Any]],
    dict[str, Any],
    dict[str, Any],
    dict[str, Any],
    list[dict[str, Any]],
    list[dict[str, Any]],
    list[dict[str, Any]],
]:
    """Inventory one category's exact target bodies and source/H1 control-flow surface."""
    profiles = [
        profile
        for profile in record_target_profiles
        if profile["categories"] == [category] and profile["targetH1Address"] is not None
    ]
    category_profiles = [
        profile for profile in record_target_profiles if profile["categories"] == [category]
    ]
    exclusions = _target_program_exclusions(category_profiles, category=category)
    if len(profiles) + len(exclusions) != len(category_profiles):
        raise ValueError(f"map {category} target profile classification drift")
    raw_blocks: list[tuple[dict[str, Any], dict[str, Any], int]] = []
    instruction_symbols: list[str] = []
    for profile in profiles:
        block = _source_program_block(
            disasm,
            profile,
            addresses,
            allow_source_stream_terminator=category == "zoneEvents",
        )
        end_address = _bind_operations_to_h1(
            listing_lines,
            listing_index,
            profile=profile,
            block=block,
        )
        raw_blocks.append((profile, block, end_address))
        instruction_symbols.extend(
            operation["instructionTargetSymbol"]
            for operation in block["operations"]
            if operation["instructionTargetSymbol"] is not None
        )

    alias_symbols = list(
        dict.fromkeys(symbol for symbol in instruction_symbols if symbol.startswith("j_"))
    )
    initial_owner_addresses = {addresses[symbol] for symbol in instruction_symbols}
    initial_owners = _label_owners(disasm, addresses, initial_owner_addresses, source_label_owners)
    aliases_by_symbol = _parse_jump_interface_aliases(
        disasm,
        addresses,
        listing_lines,
        listing_index,
        initial_owners,
        alias_symbols,
    )
    if any(
        definition["directTargetSymbol"].startswith("j_")
        for definition in aliases_by_symbol.values()
    ):
        raise ValueError(f"map {category} jump-interface alias chain drift")
    target_owner_addresses = initial_owner_addresses | {
        definition["directTargetAddress"] for definition in aliases_by_symbol.values()
    }
    target_owners = _label_owners(disasm, addresses, target_owner_addresses, source_label_owners)
    alias_definitions = []
    for alias in alias_symbols:
        definition = aliases_by_symbol[alias]
        direct_target = _target_identity(definition["directTargetSymbol"], addresses, target_owners)
        alias_definitions.append(
            {**definition, "directTargetAddressLabels": direct_target["addressLabels"]}
        )

    programs: list[dict[str, Any]] = []
    label_orders: list[dict[str, Any]] = []
    operation_orders: list[dict[str, Any]] = []
    for profile, block, end_address in raw_blocks:
        entry_address = profile["targetH1Address"]
        operations: list[dict[str, Any]] = []
        for raw_operation in block["operations"]:
            operation = dict(raw_operation)
            instruction_symbol = operation.pop("instructionTargetSymbol")
            if instruction_symbol is None:
                operation["target"] = None
            else:
                instruction_target = _target_identity(instruction_symbol, addresses, target_owners)
                alias = aliases_by_symbol.get(instruction_symbol)
                effective_symbol = (
                    alias["directTargetSymbol"] if alias is not None else instruction_symbol
                )
                effective_target = _target_identity(effective_symbol, addresses, target_owners)
                operation["target"] = {
                    "instructionTargetSymbol": instruction_target["symbol"],
                    "instructionTargetAddress": instruction_target["address"],
                    "instructionTargetAddressLabels": instruction_target["addressLabels"],
                    "effectiveTargetSymbol": effective_target["symbol"],
                    "effectiveTargetAddress": effective_target["address"],
                    "effectiveTargetAddressLabels": effective_target["addressLabels"],
                    "effectiveTargetScope": (
                        "internal"
                        if entry_address <= effective_target["address"] < end_address
                        else "external"
                    ),
                }
            operations.append(operation)
        if operations[-1]["controlFlowKind"] not in {"return", "direct-jump"} and not (
            block.get("sourceStreamTerminator") == "csc_end"
            and operations[-1]["sourceMnemonic"] == "csc_end"
        ):
            raise ValueError(
                f"map {category} target program lacks stable termination: "
                f"{profile['canonicalSymbol']}"
            )
        termination_operation = operations[-1]
        termination = {
            field: termination_operation[field]
            for field in (
                "sourceOrder",
                "sourceLine",
                "address",
                "sourceMnemonic",
                "mnemonic",
                "sizeSuffix",
                "operandTexts",
                "controlFlowKind",
                "target",
            )
        }
        program = {
            "programOrder": len(programs),
            "canonicalSymbol": profile["canonicalSymbol"],
            "entryAddress": entry_address,
            "sourcePath": profile["ownerSourcePath"],
            "entrySourceLine": profile["ownerSourceLine"],
            "endFunctionSymbol": block["endFunctionSymbol"],
            "endSourceLine": block["endSourceLine"],
            "endAddressExclusive": end_address,
            "encodedSpanBytes": end_address - entry_address,
            "referenceCounts": {
                "physicalRecordCount": profile["physicalRecordCount"],
                "setupRecordReferenceCount": profile["setupRecordReferenceCount"],
                "routeRecordReferenceCount": profile["routeRecordReferenceCount"],
            },
            "labels": block["labels"],
            "operations": operations,
            "termination": termination,
        }
        programs.append(program)
        key = _program_key(program["canonicalSymbol"], program["entryAddress"])
        label_orders.append(
            {
                "programKey": key,
                "labelOrder": [
                    f"{label['sourceOrder']}:{label['sourceLine']}:{label['symbol']}:{label['address']}"
                    for label in program["labels"]
                ],
            }
        )
        operation_orders.append(
            {
                "programKey": key,
                "operationOrder": [
                    _target_program_operation_order(operation, category=category)
                    for operation in program["operations"]
                ],
            }
        )
    control_flow, target_orders = _target_program_control_flow(programs, alias_definitions)
    summary = {
        "programCount": len(programs),
        "sourceFileCount": len({program["sourcePath"] for program in programs}),
        "labelCount": sum(len(program["labels"]) for program in programs),
        "operationCount": sum(len(program["operations"]) for program in programs),
        "ordinaryOperationCount": sum(
            operation["controlFlowKind"] == "ordinary"
            for program in programs
            for operation in program["operations"]
        ),
        "conditionalBranchCount": sum(
            operation["controlFlowKind"] == "conditional-branch"
            for program in programs
            for operation in program["operations"]
        ),
        "unconditionalBranchCount": sum(
            operation["controlFlowKind"] == "unconditional-branch"
            for program in programs
            for operation in program["operations"]
        ),
        "directCallCount": sum(
            operation["controlFlowKind"] == "direct-call"
            for program in programs
            for operation in program["operations"]
        ),
        "directJumpCount": sum(
            operation["controlFlowKind"] == "direct-jump"
            for program in programs
            for operation in program["operations"]
        ),
        "returnCount": sum(
            operation["controlFlowKind"] == "return"
            for program in programs
            for operation in program["operations"]
        ),
        "encodedSpanBytes": sum(program["encodedSpanBytes"] for program in programs),
        "physicalRecordCount": sum(profile["physicalRecordCount"] for profile in profiles),
        "setupRecordReferenceCount": sum(
            profile["setupRecordReferenceCount"] for profile in profiles
        ),
        "routeRecordReferenceCount": sum(
            profile["routeRecordReferenceCount"] for profile in profiles
        ),
        "internalControlFlowSiteCount": sum(
            operation["target"] is not None
            and operation["target"]["effectiveTargetScope"] == "internal"
            for program in programs
            for operation in program["operations"]
        ),
        "externalControlFlowSiteCount": sum(
            operation["target"] is not None
            and operation["target"]["effectiveTargetScope"] == "external"
            for program in programs
            for operation in program["operations"]
        ),
        "instructionTargetCount": len(target_orders["instructionTargetOrder"]),
        "effectiveTargetCount": len(target_orders["effectiveTargetOrder"]),
        "jumpInterfaceAliasCount": len(alias_definitions),
    }
    if category != "entityEvents":
        summary.update(
            {
                "profileCount": len(category_profiles),
                "explicitNonProgramExclusionCount": len(exclusions),
                "functionEndBoundaryCount": sum(
                    program["endFunctionSymbol"] is not None for program in programs
                ),
                "sourceStreamTerminatorCount": sum(
                    program["endFunctionSymbol"] is None for program in programs
                ),
                "excludedPhysicalRecordCount": sum(
                    exclusion["referenceCounts"]["physicalRecordCount"] for exclusion in exclusions
                ),
                "excludedSetupRecordReferenceCount": sum(
                    exclusion["referenceCounts"]["setupRecordReferenceCount"]
                    for exclusion in exclusions
                ),
                "excludedRouteRecordReferenceCount": sum(
                    exclusion["referenceCounts"]["routeRecordReferenceCount"]
                    for exclusion in exclusions
                ),
            }
        )
    _reconcile_target_programs(
        profiles,
        exclusions,
        programs,
        summary,
        control_flow,
        target_orders,
        category=category,
    )
    return (
        programs,
        summary,
        control_flow,
        target_orders,
        label_orders,
        operation_orders,
        exclusions,
    )


def _ownership_class(record: dict[str, Any], owner_path: str) -> str:
    if record["macro"] == "raw-zone-default-expression":
        return "raw-expression-boundary"
    if owner_path == record["sourcePath"]:
        return "same-event-source"
    if owner_path.startswith("data/maps/entries/"):
        return "other-map-source"
    if owner_path.startswith("code/"):
        return "common-code"
    return "other-source"


def _record_target_ownership(
    record: dict[str, Any],
    addresses: dict[str, int],
    owners: dict[int, list[dict[str, Any]]],
) -> dict[str, Any]:
    """Resolve one decoded record to an exact owner, keeping raw map44 distinct."""
    target_address = record["resolvedTargetAddress"]
    labels = owners.get(target_address, [])
    base_address = addresses[record["targetBaseSymbol"]]
    if record["macro"] == "raw-zone-default-expression":
        base_labels = owners.get(base_address, [])
        if not base_labels:
            raise ValueError("map event target ownership unresolved raw expression base")
        owner = base_labels[0]
        canonical_symbol = "raw-map44-zone-default-expression-boundary"
        target_h1_address: int | None = None
    else:
        if not labels:
            raise ValueError("map event target ownership unresolved exact target")
        owner_paths = {label["sourcePath"] for label in labels}
        if len(owner_paths) != 1:
            raise ValueError("map event target ownership ambiguous exact target")
        owner = labels[0]
        canonical_symbol = owner["symbol"]
        target_h1_address = target_address
    return {
        "targetCanonicalSymbol": canonical_symbol,
        "targetAddressLabels": labels,
        "targetH1Address": target_h1_address,
        "targetBaseH1Address": base_address,
        "targetOwnerSourcePath": owner["sourcePath"],
        "targetOwnerSourceLine": owner["sourceLine"],
        "targetOwnershipClass": _ownership_class(record, owner["sourcePath"]),
    }


def _join_target_ownership(
    disasm: Path,
    addresses: dict[str, int],
    categories: dict[str, dict[str, Any]],
    source_label_owners: dict[int, list[dict[str, Any]]] | None = None,
) -> tuple[list[dict[str, Any]], list[dict[str, Any]], list[dict[str, Any]]]:
    """Join each source/ROM event record to an exact source/H1 target owner."""
    records = [
        record
        for category in categories.values()
        for table in category["tables"]
        for record in table["records"]
    ]
    target_addresses = {record["resolvedTargetAddress"] for record in records}
    target_addresses.update(addresses[record["targetBaseSymbol"]] for record in records)
    owners = _label_owners(disasm, addresses, target_addresses, source_label_owners)
    unresolved: list[dict[str, Any]] = []
    ambiguous: list[dict[str, Any]] = []
    for record in records:
        try:
            record.update(_record_target_ownership(record, addresses, owners))
        except ValueError as error:
            issue = {
                "recordAddress": record["address"],
                "targetExpression": record["targetExpression"],
            }
            if "ambiguous" in str(error):
                issue["resolvedTargetAddress"] = record["resolvedTargetAddress"]
                issue["targetAddressLabels"] = owners.get(record["resolvedTargetAddress"], [])
                ambiguous.append(issue)
            else:
                unresolved.append(issue)
    if unresolved or ambiguous:
        raise ValueError(
            "map event target ownership is incomplete: "
            f"unresolved={len(unresolved)}, ambiguous={len(ambiguous)}"
        )
    profiles_by_identity: dict[tuple[int, str], dict[str, Any]] = {}
    for record in records:
        identity = (record["resolvedTargetAddress"], record["targetCanonicalSymbol"])
        profile = profiles_by_identity.setdefault(
            identity,
            {
                "profileOrder": len(profiles_by_identity),
                "canonicalSymbol": record["targetCanonicalSymbol"],
                "targetAddress": record["resolvedTargetAddress"],
                "targetH1Address": record["targetH1Address"],
                "targetBaseH1Address": record["targetBaseH1Address"],
                "targetAddressLabels": record["targetAddressLabels"],
                "ownerSourcePath": record["targetOwnerSourcePath"],
                "ownerSourceLine": record["targetOwnerSourceLine"],
                "ownershipClass": record["targetOwnershipClass"],
                "physicalRecordCount": 0,
                "categories": [],
            },
        )
        profile["physicalRecordCount"] += 1
        if record["category"] not in profile["categories"]:
            profile["categories"].append(record["category"])
    profiles = list(profiles_by_identity.values())
    return profiles, unresolved, ambiguous


def _event_table_profiles(categories: dict[str, dict[str, Any]]) -> dict[str, dict[str, Any]]:
    """Index the complete declared category target surface, including RTS stubs."""
    profiles: dict[str, dict[str, Any]] = {}
    for category, value in categories.items():
        for source_file in value["sourceFiles"]:
            symbol = source_file["symbol"]
            if symbol in profiles:
                raise ValueError(f"map event table profile duplicates symbol: {symbol}")
            source_file["category"] = category
            profiles[symbol] = source_file
    return profiles


def _setup_category_joins(
    setup: dict[str, Any], categories: dict[str, dict[str, Any]]
) -> tuple[list[dict[str, Any]], list[dict[str, Any]]]:
    """Retain each setup-table category target without duplicating physical records."""
    profiles = _event_table_profiles(categories)
    pointer_tables = {table["symbol"]: table for table in setup["pointerTables"]}
    joins: list[dict[str, Any]] = []
    route_joins: list[dict[str, Any]] = []
    for pointer_order, pointer_table in enumerate(setup["pointerTables"]):
        for category in CATEGORY_CONFIG:
            target = pointer_table["targets"][category]
            profile = profiles.get(target["symbol"])
            if profile is None or profile["category"] != category:
                raise ValueError(
                    f"map event setup target lacks category profile: {target['symbol']}"
                )
            if target["address"] != profile["address"]:
                raise ValueError(f"map event setup target address drift: {target['symbol']}")
            joins.append(
                {
                    "sourceOrder": len(joins),
                    "pointerTableSourceOrder": pointer_order,
                    "pointerTableSymbol": pointer_table["symbol"],
                    "pointerTableAddress": pointer_table["address"],
                    "category": category,
                    "eventTableSymbol": target["symbol"],
                    "eventTableAddress": target["address"],
                    "directReturnStub": profile["directReturnStub"],
                    "physicalRecordCount": profile["recordCount"],
                }
            )
    route_selector_order = 0
    for route_order, route in enumerate(setup["routes"]):
        selectors = [("default", None, route["defaultPointer"])] + [
            ("flag", variant["flag"], variant["pointer"]) for variant in route["flagVariants"]
        ]
        for selector_order, (selector_kind, flag, pointer_symbol) in enumerate(selectors):
            pointer_table = pointer_tables.get(pointer_symbol)
            if pointer_table is None:
                raise ValueError(f"map event route lacks setup pointer table: {pointer_symbol}")
            for category in CATEGORY_CONFIG:
                target = pointer_table["targets"][category]
                profile = profiles.get(target["symbol"])
                if profile is None or profile["category"] != category:
                    raise ValueError(
                        f"map event route target lacks category profile: {target['symbol']}"
                    )
                if target["address"] != profile["address"]:
                    raise ValueError(f"map event route target address drift: {target['symbol']}")
                route_joins.append(
                    {
                        "sourceOrder": len(route_joins),
                        "routeSourceOrder": route_order,
                        "routeSelectorSourceOrder": route_selector_order,
                        "routeMap": route["map"],
                        "selectorSourceOrder": selector_order,
                        "selectorKind": selector_kind,
                        "flag": flag,
                        "pointerTableSymbol": pointer_symbol,
                        "pointerTableAddress": pointer_table["address"],
                        "category": category,
                        "eventTableSymbol": target["symbol"],
                        "eventTableAddress": target["address"],
                        "directReturnStub": profile["directReturnStub"],
                        "physicalRecordCount": profile["recordCount"],
                    }
                )
            route_selector_order += 1
    expected_pointer_category_joins = len(setup["pointerTables"]) * len(CATEGORY_CONFIG)
    expected_route_category_joins = setup["summary"]["routePointerReferenceCount"] * len(
        CATEGORY_CONFIG
    )
    if len(joins) != expected_pointer_category_joins:
        raise ValueError("map event setup category join cardinality drift")
    if len(route_joins) != expected_route_category_joins:
        raise ValueError("map event route category join cardinality drift")
    if route_selector_order != setup["summary"]["routePointerReferenceCount"]:
        raise ValueError("map event route selector source-order drift")
    return joins, route_joins


def _apply_reference_counts(
    setup: dict[str, Any],
    categories: dict[str, dict[str, Any]],
    setup_joins: list[dict[str, Any]],
    route_joins: list[dict[str, Any]],
    target_profiles: list[dict[str, Any]],
) -> None:
    """Derive table and target multiplicities from parsed joins, not record duplication."""
    table_profiles = _event_table_profiles(categories)
    tables_by_category = {
        category: {table["symbol"]: table for table in value["tables"]}
        for category, value in categories.items()
    }

    def join_records(join: dict[str, Any]) -> list[dict[str, Any]]:
        table = tables_by_category[join["category"]].get(join["eventTableSymbol"])
        if table is None:
            if join["directReturnStub"]:
                return []
            raise ValueError(
                "map event category join lacks a decoded non-stub table: "
                f"{join['eventTableSymbol']}"
            )
        if join["directReturnStub"] or table["address"] != join["eventTableAddress"]:
            raise ValueError(f"map event category join identity drift: {join['eventTableSymbol']}")
        return table["records"]

    profile_by_identity = {
        (profile["targetAddress"], profile["canonicalSymbol"]): profile
        for profile in target_profiles
    }
    setup_counts = Counter(join["eventTableSymbol"] for join in setup_joins)
    route_counts = Counter(join["eventTableSymbol"] for join in route_joins)
    route_category_counts = Counter(join["category"] for join in route_joins)
    for table in table_profiles.values():
        symbol = table["symbol"]
        table["setupReferenceCount"] = setup_counts[symbol]
        table["routeReferenceCount"] = route_counts[symbol]
    for profile in target_profiles:
        profile["setupRecordReferenceCount"] = 0
        profile["routeRecordReferenceCount"] = 0
    setup_record_counts: Counter[str] = Counter()
    for join in setup_joins:
        for event_record in join_records(join):
            identity = (
                event_record["resolvedTargetAddress"],
                event_record["targetCanonicalSymbol"],
            )
            if identity not in profile_by_identity:
                raise ValueError("map event setup record lacks a target profile")
            profile_by_identity[identity]["setupRecordReferenceCount"] += 1
            setup_record_counts[join["category"]] += 1
    route_record_counts: Counter[str] = Counter()
    for join in route_joins:
        for event_record in join_records(join):
            identity = (
                event_record["resolvedTargetAddress"],
                event_record["targetCanonicalSymbol"],
            )
            if identity not in profile_by_identity:
                raise ValueError("map event route record lacks a target profile")
            profile_by_identity[identity]["routeRecordReferenceCount"] += 1
            route_record_counts[join["category"]] += 1
    route_stub_counts = Counter(
        join["category"] for join in route_joins if join["directReturnStub"]
    )
    for category, value in categories.items():
        summary = value["summary"]
        summary["routeSelectorReferenceCount"] = setup["summary"]["routePointerReferenceCount"]
        summary["routeCategoryJoinCount"] = route_category_counts[category]
        summary["routeRecordReferenceCount"] = route_record_counts[category]
        summary["routeDirectReturnStubReferenceCount"] = route_stub_counts[category]


def _reconcile_event_reference_counts(
    categories: dict[str, dict[str, Any]],
    target_profiles: list[dict[str, Any]],
    setup_joins: list[dict[str, Any]],
    route_joins: list[dict[str, Any]],
    summary: dict[str, Any],
) -> None:
    """Cross-check every physical and weighted count from parsed records and joins."""
    tables_by_category = {
        category: {table["symbol"]: table for table in value["tables"]}
        for category, value in categories.items()
    }

    def join_records(join: dict[str, Any]) -> list[dict[str, Any]]:
        table = tables_by_category[join["category"]].get(join["eventTableSymbol"])
        if table is None:
            if join["directReturnStub"]:
                return []
            raise ValueError(
                "map event reconciliation lacks a decoded non-stub table: "
                f"{join['eventTableSymbol']}"
            )
        if join["directReturnStub"] or table["address"] != join["eventTableAddress"]:
            raise ValueError(
                f"map event reconciliation join identity drift: {join['eventTableSymbol']}"
            )
        return table["records"]

    physical_counts: Counter[str] = Counter()
    physical_kinds: dict[str, Counter[str]] = {category: Counter() for category in categories}
    profile_physical_counts: Counter[tuple[int, str]] = Counter()
    for category, value in categories.items():
        for table in value["tables"]:
            for record in table["records"]:
                identity = (record["resolvedTargetAddress"], record["targetCanonicalSymbol"])
                physical_counts[category] += 1
                physical_kinds[category][record["kind"]] += 1
                profile_physical_counts[identity] += 1

    setup_join_counts = Counter(join["category"] for join in setup_joins)
    route_join_counts = Counter(join["category"] for join in route_joins)
    setup_record_counts: Counter[str] = Counter()
    route_record_counts: Counter[str] = Counter()
    setup_kind_counts: dict[str, Counter[str]] = {category: Counter() for category in categories}
    route_kind_counts: dict[str, Counter[str]] = {category: Counter() for category in categories}
    profile_setup_counts: Counter[tuple[int, str]] = Counter()
    profile_route_counts: Counter[tuple[int, str]] = Counter()
    for join in setup_joins:
        for record in join_records(join):
            identity = (record["resolvedTargetAddress"], record["targetCanonicalSymbol"])
            setup_record_counts[join["category"]] += 1
            setup_kind_counts[join["category"]][record["kind"]] += 1
            profile_setup_counts[identity] += 1
    for join in route_joins:
        for record in join_records(join):
            identity = (record["resolvedTargetAddress"], record["targetCanonicalSymbol"])
            route_record_counts[join["category"]] += 1
            route_kind_counts[join["category"]][record["kind"]] += 1
            profile_route_counts[identity] += 1

    profiles_by_identity = {
        (profile["targetAddress"], profile["canonicalSymbol"]): profile
        for profile in target_profiles
    }
    if set(profiles_by_identity) != set(profile_physical_counts):
        raise ValueError("map event target profile physical identity coverage drift")
    for identity, profile in profiles_by_identity.items():
        expected = (
            profile_physical_counts[identity],
            profile_setup_counts[identity],
            profile_route_counts[identity],
        )
        observed = (
            profile["physicalRecordCount"],
            profile["setupRecordReferenceCount"],
            profile["routeRecordReferenceCount"],
        )
        if observed != expected:
            raise ValueError(f"map event target profile weighted-count drift: {identity}")

    profile_totals: Counter[str] = Counter()
    for profile in target_profiles:
        profile_totals["physical"] += profile["physicalRecordCount"]
        profile_totals["setup"] += profile["setupRecordReferenceCount"]
        profile_totals["route"] += profile["routeRecordReferenceCount"]
    parsed_totals = (
        sum(physical_counts.values()),
        sum(setup_record_counts.values()),
        sum(route_record_counts.values()),
    )
    if (
        profile_totals["physical"],
        profile_totals["setup"],
        profile_totals["route"],
    ) != parsed_totals:
        raise ValueError("map event target profile aggregate reconciliation drift")

    selector_orders = {join["routeSelectorSourceOrder"] for join in route_joins}
    route_stub_counts = Counter(
        join["category"] for join in route_joins if join["directReturnStub"]
    )
    for category, value in categories.items():
        category_summary = value["summary"]
        expected_summary = {
            "physicalRecordCount": physical_counts[category],
            "specificPhysicalRecordCount": physical_kinds[category]["specific"],
            "defaultPhysicalRecordCount": physical_kinds[category]["default"],
            "setupPointerReferenceCount": setup_join_counts[category],
            "setupRecordReferenceCount": setup_record_counts[category],
            "specificSetupRecordReferenceCount": setup_kind_counts[category]["specific"],
            "defaultSetupRecordReferenceCount": setup_kind_counts[category]["default"],
            "routeSelectorReferenceCount": len(selector_orders),
            "routeCategoryJoinCount": route_join_counts[category],
            "routeRecordReferenceCount": route_record_counts[category],
            "routeDirectReturnStubReferenceCount": route_stub_counts[category],
        }
        for field, expected in expected_summary.items():
            if category_summary[field] != expected:
                raise ValueError(f"map event category reconciliation drift: {category}.{field}")

    global_expected = {
        "physicalRecordCount": sum(physical_counts.values()),
        "setupPointerReferenceCount": len(setup_joins),
        "setupRecordReferenceCount": sum(setup_record_counts.values()),
        "routeSelectorReferenceCount": len(selector_orders),
        "routeCategoryJoinCount": len(route_joins),
        "routeRecordReferenceCount": sum(route_record_counts.values()),
        "recordTargetProfileCount": len(profiles_by_identity),
        "setupCategoryJoinCount": len(setup_joins),
    }
    for field, expected in global_expected.items():
        if summary[field] != expected:
            raise ValueError(f"map event global reconciliation drift: {field}")


def _consumer_facts(setup: dict[str, Any]) -> dict[str, Any]:
    dispatch = setup["sourceFacts"]["dispatch"]
    return {
        "defaultMarker": 0xFD,
        "relativeOffsetsResolveFromTableBase": True,
        "firstMatchingEntryWins": True,
        "entityEvents": dispatch["entityEvent"],
        "zoneEvents": dispatch["zoneEvent"],
        "itemEvents": {**dispatch["itemEvent"], "itemIndexMask": 0x7F},
    }


def _entity_event_reachability_facts(disasm: Path, addresses: dict[str, int]) -> dict[str, Any]:
    sources = {
        "ProcessPlayerAction": read_upstream_text(
            disasm / "code/gameflow/exploration/explorationvints.asm"
        ),
        "GetActivatedEntity": read_upstream_text(
            disasm / "code/gameflow/exploration/explorationfunctions_0.asm"
        ),
        "GetEntityEventIndex": read_upstream_text(
            disasm / "code/gameflow/battle/battlefunctions/battlefunctions_0.asm"
        ),
    }
    required = {
        "ProcessPlayerAction": (
            "bsr.w   GetActivatedEntity",
            "tst.w   d0",
            "bsr.w   GetEntityEventIndex",
            "jsr     j_RunMapSetupEntityEvent",
        ),
        "GetActivatedEntity": (
            "moveq   #$2F,d7",
            "bsr.w   IsFollowerEntity",
            "cmpi.w  #MAP_TILE_SIZE,d5",
            "moveq   #-1,d0",
        ),
        "GetEntityEventIndex": (
            "moveq   #BATTLE_ALL_ENTITIES_NUMBER,d7",
            "lea     ((ENTITY_INDEX_LIST-$1000000)).w,a0",
            "cmpi.w  #BATTLE_ALLY_ENTITIES_NUMBER,d0",
            "move.w  #$80,d0",
        ),
    }
    for symbol, fragments in required.items():
        if any(fragment not in sources[symbol] for fragment in fragments):
            raise ValueError(f"entity event reachability source-shape drift: {symbol}")
    return {
        "functionAddresses": {
            symbol: addresses[symbol] for symbol in REACHABILITY_FUNCTION_SYMBOLS
        },
        "activatedEntityScanSlots": 48,
        "followersAreSkipped": True,
        "adjacentDistanceIsStrictlyBelowMapTileSize": True,
        "entityIndexListSlotsScanned": 65,
        "enemyEventIndexBase": 128,
        "processActionCallsWrapperAfterNonnegativeActivation": True,
    }


def _clean_state_event_indices(records: list[dict[str, Any]]) -> list[int]:
    enemy_ordinal = 0
    event_indices: list[int] = []
    for record in records:
        if record["mapSprite"] >= 240:
            raise ValueError("direct-return reachability model does not cover special map sprites")
        if record["mapSprite"] < 30:
            event_indices.append(record["mapSprite"])
        else:
            event_indices.append(128 + enemy_ordinal)
            enemy_ordinal += 1
    return event_indices


def build_map_events_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"map events H1 listing is missing: {listing_path}")
    listing_text = listing_path.read_text(encoding="utf-8")
    listing_lines = listing_text.splitlines()
    listing_index = _h1_program_index(listing_lines)
    addresses = listing_symbol_addresses(listing_text)
    rom = rom_path.read_bytes()
    setup = build_map_setup_contract(rom_path, upstream_path)
    entities = build_map_entities_contract(rom_path, upstream_path)
    map_script_engine = build_map_script_engine_contract(rom_path, upstream_path)
    entity_action_scripts = build_entity_action_script_contract(rom_path, upstream_path)
    text_line_domain_contract = build_text_line_domain_contract(rom_path, upstream_path)
    sound_command_domain = _build_sound_command_domain(
        rom_path,
        upstream_path,
        disasm=disasm,
        upstream_commit=commit,
        rom_sha256=setup["romSha256"],
    )
    source_macro_catalog = _source_macro_catalog(
        disasm, (SERVICE_MACROS_PATH, CUTSCENE_MACROS_PATH)
    )
    if setup["upstream"]["commit"] != commit:
        raise ValueError("map events/setup provenance drift")

    macro_definitions = _event_macro_definitions(disasm)
    categories = {
        category: _category_contract(
            disasm, addresses, rom, setup, category, macro_definitions[category]
        )
        for category in CATEGORY_CONFIG
    }
    source_label_owners = _source_label_owners(disasm, addresses)
    record_target_profiles, unresolved_record_targets, ambiguous_record_targets = (
        _join_target_ownership(disasm, addresses, categories, source_label_owners)
    )
    setup_category_joins, route_category_joins = _setup_category_joins(setup, categories)
    _apply_reference_counts(
        setup,
        categories,
        setup_category_joins,
        route_category_joins,
        record_target_profiles,
    )
    (
        entity_target_programs,
        entity_target_program_summary,
        entity_target_program_control_flow,
        entity_target_program_control_flow_target_orders,
        entity_target_program_label_orders,
        entity_target_program_operation_orders,
        entity_target_program_exclusions,
    ) = _target_program_contract(
        disasm,
        addresses,
        listing_lines,
        listing_index,
        record_target_profiles,
        source_label_owners,
        category="entityEvents",
    )
    if entity_target_program_exclusions:
        raise ValueError("map entity-event target program exclusion drift")
    (
        zone_target_programs,
        zone_target_program_summary,
        zone_target_program_control_flow,
        zone_target_program_control_flow_target_orders,
        zone_target_program_label_orders,
        zone_target_program_operation_orders,
        zone_target_program_exclusions,
    ) = _target_program_contract(
        disasm,
        addresses,
        listing_lines,
        listing_index,
        record_target_profiles,
        source_label_owners,
        category="zoneEvents",
    )
    (
        item_target_programs,
        item_target_program_summary,
        item_target_program_control_flow,
        item_target_program_control_flow_target_orders,
        item_target_program_label_orders,
        item_target_program_operation_orders,
        item_target_program_exclusions,
    ) = _target_program_contract(
        disasm,
        addresses,
        listing_lines,
        listing_index,
        record_target_profiles,
        source_label_owners,
        category="itemEvents",
    )
    programs_by_category = {
        "entityEvents": entity_target_programs,
        "zoneEvents": zone_target_programs,
        "itemEvents": item_target_programs,
    }
    payload_context_specs = _derived_action_payload_context_specs(
        disasm,
        source_macro_catalog,
        map_script_engine,
        entity_action_scripts,
    )
    payload_contexts, payload_macro_families = _payload_context_contract(
        disasm,
        programs_by_category,
        payload_context_specs=payload_context_specs,
        action_command_macros={
            binding["macro"] for binding in entity_action_scripts["handlerMacroBindings"]
        },
    )
    operation_definitions, macro_vocabulary = _operation_definition_contract(
        source_macro_catalog,
        map_script_engine,
        entity_action_scripts,
        programs_by_category,
        payload_context_specs,
        payload_macro_families,
    )
    operation_vocabulary_contract = _join_operation_vocabulary(
        listing_lines,
        source_macro_catalog,
        operation_definitions,
        macro_vocabulary,
        programs_by_category,
    )
    _reconcile_operation_weight_contract(programs_by_category, operation_vocabulary_contract)
    direct_flag_state_contract = _direct_flag_state_contract(
        list(operation_definitions.values()), programs_by_category
    )
    script_invocation_contract = _script_invocation_graph_contract(
        list(operation_definitions.values()),
        programs_by_category,
        map_script_engine["programCorpus"],
        addresses,
    )
    _reconcile_script_invocation_graph_contract(
        script_invocation_contract,
        list(operation_definitions.values()),
        programs_by_category,
        map_script_engine["programCorpus"],
        addresses,
    )
    textbox_reference_contract = _textbox_reference_contract(
        list(operation_definitions.values()),
        programs_by_category,
        text_line_domain_contract=text_line_domain_contract,
        upstream_commit=commit,
        rom_sha256=setup["romSha256"],
    )
    _reconcile_textbox_reference_contract(
        textbox_reference_contract,
        list(operation_definitions.values()),
        programs_by_category,
        text_line_domain_contract=text_line_domain_contract,
        upstream_commit=commit,
        rom_sha256=setup["romSha256"],
    )
    sound_command_reference_contract = _sound_command_reference_contract(
        list(operation_definitions.values()),
        programs_by_category,
        sound_domain=sound_command_domain,
    )
    _reconcile_sound_command_reference_contract(
        sound_command_reference_contract,
        list(operation_definitions.values()),
        programs_by_category,
        sound_domain=sound_command_domain,
    )
    operation_orders_by_category = {
        "entityEvents": entity_target_program_operation_orders,
        "zoneEvents": zone_target_program_operation_orders,
        "itemEvents": item_target_program_operation_orders,
    }
    family_indices = {
        family: index
        for index, family in enumerate(operation_vocabulary_contract["operationFamilyOrder"])
    }
    definition_indices = {
        definition["definitionId"]: index
        for index, definition in enumerate(operation_definitions.values())
    }
    operation_weight_orders_by_category: dict[str, list[str]] = {}
    payload_context_orders_by_category: dict[str, list[str]] = {}
    for category, programs in programs_by_category.items():
        order_rows = operation_orders_by_category[category]
        if len(order_rows) != len(programs):
            raise ValueError(f"map {category} target operation-order coverage drift")
        for program, order_row in zip(programs, order_rows, strict=True):
            if order_row["programKey"] != _program_key(
                program["canonicalSymbol"], program["entryAddress"]
            ):
                raise ValueError(f"map {category} target operation-order identity drift")
            order_row["operationOrder"] = [
                _target_program_operation_order(operation, category=category)
                for operation in program["operations"]
            ]
            order_row["operationJoinOrder"] = [
                _target_program_operation_join_order(
                    operation,
                    family_indices=family_indices,
                    definition_indices=definition_indices,
                )
                for operation in program["operations"]
            ]
            program["termination"].update(
                {
                    "family": program["operations"][-1]["family"],
                    "definitionId": program["operations"][-1]["definitionId"],
                    "payloadContextIds": program["operations"][-1]["payloadContextIds"],
                }
            )
        operation_weight_orders_by_category[category] = [
            "|".join(
                (
                    _program_key(program["canonicalSymbol"], program["entryAddress"]),
                    str(program["operationWeightCounts"]["uniquePhysicalOperationCount"]),
                    str(program["operationWeightCounts"]["physicalRecordWeightedOperationCount"]),
                    str(
                        program["operationWeightCounts"][
                            "setupRecordReferenceWeightedOperationCount"
                        ]
                    ),
                    str(
                        program["operationWeightCounts"][
                            "routeRecordReferenceWeightedOperationCount"
                        ]
                    ),
                )
            )
            for program in programs
        ]
        payload_context_orders_by_category[category] = [
            "|".join(
                (
                    _program_key(program["canonicalSymbol"], program["entryAddress"]),
                    ",".join(program["payloadContextIds"]),
                    ",".join(program["inheritedPayloadContextIds"]),
                )
            )
            for program in programs
        ]
    entity_target_refs = [
        table["targets"]["entityEvents"]["symbol"] for table in setup["pointerTables"]
    ]
    entity_lists = {row["symbol"]: row for row in entities["lists"]}
    direct_return_stubs: list[dict[str, Any]] = []
    for symbol in CATEGORY_CONFIG["entityEvents"]["stubSymbols"]:
        owners = [
            table
            for table in setup["pointerTables"]
            if table["targets"]["entityEvents"]["symbol"] == symbol
        ]
        pairings: list[dict[str, Any]] = []
        for table in owners:
            entity_symbol = table["targets"]["entities"]["symbol"]
            entity_list = entity_lists[entity_symbol]
            event_indices = _clean_state_event_indices(entity_list["records"])
            pairings.append(
                {
                    "setupSymbol": table["symbol"],
                    "entityListSymbol": entity_symbol,
                    "entityRecordCount": entity_list["recordCount"],
                    "cleanStateEventIndices": event_indices,
                    "wrapperReachableWithAdjacentNonFollower": bool(event_indices),
                    "normalStoryRouteReachability": (
                        "unknown" if event_indices else "not-applicable-empty-list"
                    ),
                }
            )
        paired_record_counts = [row["entityRecordCount"] for row in pairings]
        direct_return_stubs.append(
            {
                "symbol": symbol,
                "address": addresses[symbol],
                "setupReferenceCount": entity_target_refs.count(symbol),
                "pairedEntityListRecordCounts": paired_record_counts,
                "nonEmptyPairedEntityListReferenceCount": sum(
                    record_count > 0 for record_count in paired_record_counts
                ),
                "setupPairings": pairings,
            }
        )
    raw_record = next(
        record
        for table in categories["zoneEvents"]["tables"]
        if table["symbol"] == RAW_ZONE_DEFAULT_SYMBOL
        for record in table["records"]
    )
    raw_zone_default = {
        "symbol": RAW_ZONE_DEFAULT_SYMBOL,
        "address": addresses[RAW_ZONE_DEFAULT_SYMBOL],
        "relativeOffset": raw_record["relativeOffset"],
        "resolvedTargetAddress": raw_record["resolvedTargetAddress"],
        "targetExpression": raw_record["targetExpression"],
        "targetBaseSymbol": raw_record["targetBaseSymbol"],
        "targetBaseH1Address": raw_record["targetBaseH1Address"],
        "targetBaseAdjustment": raw_record["targetBaseAdjustment"],
        "targetOwnerSourcePath": raw_record["targetOwnerSourcePath"],
        "targetOwnerSourceLine": raw_record["targetOwnerSourceLine"],
        "pointsInsideCutsceneEntityList": raw_record["resolvedTargetAddress"]
        == addresses["byte_54868"] + 4,
    }
    if not raw_zone_default["pointsInsideCutsceneEntityList"]:
        raise ValueError("map 44 raw zone-default target drift")

    category_summaries = {category: value["summary"] for category, value in categories.items()}
    summary = {
        "sourceFileCount": sum(row["sourceFileCount"] for row in category_summaries.values()),
        "setupPointerReferenceCount": sum(
            row["setupPointerReferenceCount"] for row in category_summaries.values()
        ),
        "uniqueTargetCount": sum(row["uniqueTargetCount"] for row in category_summaries.values()),
        "physicalRecordCount": sum(
            row["physicalRecordCount"] for row in category_summaries.values()
        ),
        "specificPhysicalRecordCount": sum(
            row["specificPhysicalRecordCount"] for row in category_summaries.values()
        ),
        "defaultPhysicalRecordCount": sum(
            row["defaultPhysicalRecordCount"] for row in category_summaries.values()
        ),
        "setupRecordReferenceCount": sum(
            row["setupRecordReferenceCount"] for row in category_summaries.values()
        ),
        "specificSetupRecordReferenceCount": sum(
            row["specificSetupRecordReferenceCount"] for row in category_summaries.values()
        ),
        "defaultSetupRecordReferenceCount": sum(
            row["defaultSetupRecordReferenceCount"] for row in category_summaries.values()
        ),
        "directReturnStubCount": sum(
            row["directReturnStubCount"] for row in category_summaries.values()
        ),
        "directReturnStubReferenceCount": sum(
            row["directReturnStubReferenceCount"] for row in category_summaries.values()
        ),
        "rawDefaultExceptionCount": sum(
            row["rawDefaultExceptionCount"] for row in category_summaries.values()
        ),
        "maximumTableRecordCount": max(
            row["maximumTableRecordCount"] for row in category_summaries.values()
        ),
        "selectionCaseCount": len(SELECTION_INPUTS),
        "recordTargetProfileCount": len(record_target_profiles),
        "setupCategoryJoinCount": len(setup_category_joins),
        "routeCategoryJoinCount": len(route_category_joins),
        "routeSelectorReferenceCount": setup["summary"]["routePointerReferenceCount"],
        "routeRecordReferenceCount": sum(
            row["routeRecordReferenceCount"] for row in category_summaries.values()
        ),
    }
    _reconcile_event_reference_counts(
        categories,
        record_target_profiles,
        setup_category_joins,
        route_category_joins,
        summary,
    )
    selection_cases = _selection_cases(setup, categories)
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "romSha256": setup["romSha256"],
        "scope": f"{SOURCE_ROOT.as_posix()}/*/mapsetups/s[235]_*.asm",
        "function": {symbol: addresses[symbol] for symbol in FUNCTION_SYMBOLS},
        "summary": summary,
        "categorySummaries": category_summaries,
        "sourceMacroCounts": {
            category: value["sourceMacroCounts"] for category, value in categories.items()
        },
        "eventMacroDefinitions": {
            "sourcePath": MAP_SETUP_MACROS_PATH.as_posix(),
            "categories": macro_definitions,
        },
        "operationDefinitions": list(operation_definitions.values()),
        "operationDefinitionOrder": [
            json.dumps(definition, ensure_ascii=False, separators=(",", ":"))
            for definition in operation_definitions.values()
        ],
        "operationPayloadContexts": payload_contexts,
        "operationPayloadContextOrder": [
            json.dumps(context, ensure_ascii=False, separators=(",", ":"))
            for context in payload_contexts
        ],
        **operation_vocabulary_contract,
        **direct_flag_state_contract,
        **script_invocation_contract,
        **textbox_reference_contract,
        **sound_command_reference_contract,
        "consumerFacts": _consumer_facts(setup),
        "entityEventReachabilityFacts": _entity_event_reachability_facts(disasm, addresses),
        "entityTargetProgramSummary": entity_target_program_summary,
        "entityTargetPrograms": entity_target_programs,
        "entityTargetProgramOrder": [
            _program_key(program["canonicalSymbol"], program["entryAddress"])
            for program in entity_target_programs
        ],
        "entityTargetProgramLabelOrders": entity_target_program_label_orders,
        "entityTargetProgramOperationOrders": entity_target_program_operation_orders,
        "entityTargetProgramOperationWeightOrders": operation_weight_orders_by_category[
            "entityEvents"
        ],
        "entityTargetProgramPayloadContextOrders": payload_context_orders_by_category[
            "entityEvents"
        ],
        "entityTargetProgramControlFlow": entity_target_program_control_flow,
        "entityTargetProgramControlFlowTargetOrders": (
            entity_target_program_control_flow_target_orders
        ),
        "zoneTargetProgramSummary": zone_target_program_summary,
        "zoneTargetPrograms": zone_target_programs,
        "zoneTargetProgramOrder": [
            _program_key(program["canonicalSymbol"], program["entryAddress"])
            for program in zone_target_programs
        ],
        "zoneTargetProgramBoundaryOrders": [
            _target_program_boundary_order(program) for program in zone_target_programs
        ],
        "zoneTargetProgramLabelOrders": zone_target_program_label_orders,
        "zoneTargetProgramOperationOrders": zone_target_program_operation_orders,
        "zoneTargetProgramOperationWeightOrders": operation_weight_orders_by_category["zoneEvents"],
        "zoneTargetProgramPayloadContextOrders": payload_context_orders_by_category["zoneEvents"],
        "zoneTargetProgramControlFlow": zone_target_program_control_flow,
        "zoneTargetProgramControlFlowTargetOrders": zone_target_program_control_flow_target_orders,
        "zoneTargetProgramExclusions": zone_target_program_exclusions,
        "zoneTargetProgramExclusionOrder": [
            _program_key(exclusion["canonicalSymbol"], exclusion["targetAddress"])
            for exclusion in zone_target_program_exclusions
        ],
        "itemTargetProgramSummary": item_target_program_summary,
        "itemTargetPrograms": item_target_programs,
        "itemTargetProgramOrder": [
            _program_key(program["canonicalSymbol"], program["entryAddress"])
            for program in item_target_programs
        ],
        "itemTargetProgramBoundaryOrders": [
            _target_program_boundary_order(program) for program in item_target_programs
        ],
        "itemTargetProgramLabelOrders": item_target_program_label_orders,
        "itemTargetProgramOperationOrders": item_target_program_operation_orders,
        "itemTargetProgramOperationWeightOrders": operation_weight_orders_by_category["itemEvents"],
        "itemTargetProgramPayloadContextOrders": payload_context_orders_by_category["itemEvents"],
        "itemTargetProgramControlFlow": item_target_program_control_flow,
        "itemTargetProgramControlFlowTargetOrders": item_target_program_control_flow_target_orders,
        "itemTargetProgramExclusions": item_target_program_exclusions,
        "itemTargetProgramExclusionOrder": [
            _program_key(exclusion["canonicalSymbol"], exclusion["targetAddress"])
            for exclusion in item_target_program_exclusions
        ],
        "directReturnStubs": direct_return_stubs,
        "rawZoneDefaultException": raw_zone_default,
        "unresolvedRecordTargets": unresolved_record_targets,
        "ambiguousRecordTargets": ambiguous_record_targets,
        "recordTargetProfiles": record_target_profiles,
        "setupCategoryJoins": setup_category_joins,
        "routeCategoryJoins": route_category_joins,
        "categorySourceFileOrders": {
            category: [
                f"{source_file['sourceOrder']}:{source_file['symbol']}:{source_file['address']}"
                for source_file in value["sourceFiles"]
            ]
            for category, value in categories.items()
        },
        "categoryDecodedTableOrders": {
            category: [f"{table['symbol']}:{table['address']}" for table in value["tables"]]
            for category, value in categories.items()
        },
        "physicalRecordOrder": [
            f"{category}:{record['recordSourceOrder']}:{record['address']}"
            for category, value in categories.items()
            for record in sorted(
                (record for table in value["tables"] for record in table["records"]),
                key=lambda record: record["recordSourceOrder"],
            )
        ],
        "recordTargetProfileOrder": [
            f"{profile['canonicalSymbol']}:{profile['targetAddress']}"
            for profile in record_target_profiles
        ],
        "setupCategoryJoinOrder": [
            f"{join['pointerTableSymbol']}:{join['category']}:{join['eventTableSymbol']}"
            for join in setup_category_joins
        ],
        "routeCategoryJoinOrder": [
            f"{join['routeSourceOrder']}:{join['selectorSourceOrder']}:"
            f"{join['category']}:{join['eventTableSymbol']}"
            for join in route_category_joins
        ],
        "selectionCases": selection_cases,
        "runtimeQuestions": [
            "entity-event-direct-return-stub-normal-story-route-reachability",
            "event-script-side-effects-and-transition-persistence",
            "event-portrait-facing-and-presentation-timing",
        ],
        "categories": categories,
    }


def _verify_complete_map_events_fixture(fixture: dict[str, Any], output: dict[str, Any]) -> None:
    """Reject a legal-shape fixture/output replacement that changes canonical evidence."""
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != output["romSha256"]
        or fixture["function"] != output["function"]
    ):
        raise ValueError("map events provenance/address drift")
    if fixture["expected"] != output:
        raise ValueError("map events complete semantic fixture drift")


def verify_map_events_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_map_events_fixture()
    manifest = load_json(MANIFEST)
    output = build_map_events_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="map events static contract")
    _verify_complete_map_events_fixture(fixture, output)
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"] or output["summary"] != manifest["summary"]:
        raise ValueError("map events canonical output drift")
    destination = output_path or repo_path("local/derived/map-events-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "SourceFiles": output["summary"]["sourceFileCount"],
        "UniqueTables": output["summary"]["uniqueTargetCount"],
        "PhysicalRecords": output["summary"]["physicalRecordCount"],
        "SetupReferences": output["summary"]["setupRecordReferenceCount"],
        "SelectionCases": output["summary"]["selectionCaseCount"],
        "Status": "PASS",
    }
