"""Static caller-side preparation and lexical-return handoff for map events.

This H2 owner deliberately consumes the accepted map-event, direct-state, and
direct-control contracts as retained owners.  It identifies only the raw
instructions immediately before a direct transfer and the first lexical raw or
macro operation after a returning call.  It neither opens a callee body nor
attributes runtime values, effects, reachability, or presentation meaning.
"""

from __future__ import annotations

import hashlib
import json
import re
from collections import Counter, defaultdict
from pathlib import Path
from typing import Any

from sf2tool.h2.map_event_direct_control import (
    FIXTURE as DIRECT_CONTROL_FIXTURE,
)
from sf2tool.h2.map_event_direct_control import (
    build_map_event_direct_control_contract,
)
from sf2tool.h2.map_event_direct_control import (
    canonical_json_bytes as _direct_control_canonical_json_bytes,
)
from sf2tool.h2.map_event_direct_state import (
    FIXTURE as DIRECT_STATE_FIXTURE,
)
from sf2tool.h2.map_event_direct_state import (
    build_map_event_direct_state_contract,
)
from sf2tool.h2.map_event_direct_state import (
    canonical_json_bytes as _direct_state_canonical_json_bytes,
)
from sf2tool.h2.map_events import _canonical_bytes as _map_events_canonical_bytes
from sf2tool.h2.map_events import build_map_events_contract
from sf2tool.h2.map_events_fixture import load_map_events_fixture
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path

ID = "sf2-map-event-direct-handoff-static-v1"
FIXTURE = repo_path("tests/fixtures/h2/map-event-direct-handoff-static-v1.json")
SCHEMA = repo_path("schemas/h2/map-event-direct-handoff-static-fixture.schema.json")
MAP_EVENTS_MANIFEST = repo_path("manifests/extractions/map-events-static.json")

_CATEGORIES = ("entityEvents", "zoneEvents", "itemEvents")
_PROGRAM_FIELDS = {
    "entityEvents": "entityTargetPrograms",
    "zoneEvents": "zoneTargetPrograms",
    "itemEvents": "itemTargetPrograms",
}
_TRANSFER_MNEMONICS = {"jsr", "bsr", "jmp"}
_EQUATE = re.compile(r"^\s*([A-Za-z_][A-Za-z0-9_]*)\s*:\s+equ\s+([^\s;]+)")
_H1_INSTRUCTION = re.compile(
    r"^([0-9A-Fa-f]{8})\s+([0-9A-Fa-f]{2,4}(?: [0-9A-Fa-f]{2,4})*)\s{2,}(.+)$"
)
_INLINE_LABEL = re.compile(r"^[A-Za-z_][A-Za-z0-9_]*:\s*(.*)$")
_CS_PC_RELATIVE = re.compile(r"^cs_([0-9A-Fa-f]+)\(pc\)$")
_UNKNOWN_KEYS = (
    "naturalProgramReachability",
    "actualPreparationPath",
    "actualRegisterAndCcrValuesAtTransfer",
    "actualFixedStateValuesAtTransfer",
    "calleeEntryState",
    "calleeSideEffects",
    "calleeReturnRegistersAndCcr",
    "actualContinuationAndBranchSelection",
    "tailTransferReturnBehavior",
    "crossMapStateLifetime",
    "saveLoadPersistence",
    "inputUiDialogueAudioTimingAndStoryMeaning",
)


def canonical_json_bytes(value: dict[str, Any]) -> bytes:
    """Return the canonical public-fixture encoding."""
    return (
        json.dumps(value, ensure_ascii=False, sort_keys=True, separators=(",", ":")).encode("utf-8")
        + b"\n"
    )


def _validate_contract_order(value: Any, schema: dict[str, Any]) -> None:
    """Enforce the ordered, recursively closed public JSON representation.

    Draft-07 validates object membership but intentionally treats object-member
    order as insignificant. This owner publishes ordered map corpora, so its
    declared field sequence and compact record-map order arrays are checked
    before a fixture is accepted or compared.
    """

    def resolve(node: dict[str, Any]) -> dict[str, Any]:
        reference = node.get("$ref")
        if reference is None:
            return node
        if not reference.startswith("#/$defs/"):
            raise ValueError("map-event direct-handoff schema reference drift")
        return resolve(schema["$defs"][reference.rsplit("/", maxsplit=1)[-1]])

    def walk(instance: Any, node: dict[str, Any], location: str) -> None:
        node = resolve(node)
        if "anyOf" in node:
            for option in node["anyOf"]:
                resolved = resolve(option)
                expected_type = resolved.get("type")
                if expected_type == "null" and instance is None:
                    walk(instance, resolved, location)
                    return
                if expected_type == "object" and isinstance(instance, dict):
                    walk(instance, resolved, location)
                    return
            raise ValueError(f"map-event direct-handoff schema union drift: {location}")
        if node.get("type") == "object":
            if not isinstance(instance, dict):
                return
            properties = node.get("properties", {})
            expected = list(properties)
            if expected and list(instance) != expected:
                raise ValueError(f"map-event direct-handoff field order drift: {location}")
            property_names = node.get("propertyNames")
            if (
                isinstance(property_names, dict)
                and isinstance(property_names.get("enum"), list)
                and list(instance) != property_names["enum"]
            ):
                raise ValueError(f"map-event direct-handoff record order drift: {location}")
            for key, child in properties.items():
                if key in instance:
                    walk(instance[key], child, f"{location}.{key}")
            additional = node.get("additionalProperties")
            if isinstance(additional, dict):
                for key, child in instance.items():
                    if key not in properties:
                        walk(child, additional, f"{location}.{key}")
            return
        if node.get("type") == "array" and isinstance(instance, list):
            item = node.get("items")
            if isinstance(item, dict):
                for index, child in enumerate(instance):
                    walk(child, item, f"{location}.{index}")

    walk(value, schema, "<root>")
    handoff = value.get("eventDirectHandoff") if isinstance(value, dict) else None
    if not isinstance(handoff, dict):
        return
    for record_field, order_field in (
        ("sourceFiles", "sourceFileOrder"),
        ("programContexts", "programContextOrder"),
        ("transferHandoffs", "transferHandoffOrder"),
        ("setupOperations", "setupOperationOrder"),
        ("callContinuations", "callContinuationOrder"),
        ("physicalOperations", "physicalOperationOrder"),
        ("symbolicImmediates", "symbolicImmediateOrder"),
    ):
        records = handoff.get(record_field)
        order = handoff.get(order_field)
        if not isinstance(records, dict) or not isinstance(order, list):
            continue
        if set(records) != set(order):
            raise ValueError(f"map-event direct-handoff record name drift: {record_field}")


def _fixture_sha256(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest().upper()


def _normalise_statement(statement: str) -> str:
    statement = statement.split(";", maxsplit=1)[0].strip()
    inline_label = _INLINE_LABEL.match(statement)
    if inline_label is not None:
        statement = inline_label.group(1)
    return re.sub(r"\s*,\s*", ",", re.sub(r"\s+", " ", statement.strip()))


def _operation_statement(operation: dict[str, Any]) -> str:
    source = operation["sourceMnemonic"]
    operands = operation["operandTexts"]
    return _normalise_statement(source + (" " + ",".join(operands) if operands else ""))


def _parse_number(token: str) -> int | None:
    if token.startswith("$"):
        return int(token[1:], 16)
    if token.lstrip("-").isdigit():
        return int(token, 10)
    return None


def _parse_equates(source: str) -> dict[str, dict[str, int]]:
    definitions: dict[str, dict[str, int]] = {}
    for source_line, raw_line in enumerate(source.splitlines(), start=1):
        match = _EQUATE.match(raw_line)
        if match is None:
            continue
        value = _parse_number(match.group(2))
        if value is not None:
            definitions[match.group(1)] = {"sourceLine": source_line, "value": value}
    return definitions


def _h1_instruction_rows(listing_text: str) -> dict[int, tuple[bytes, str]]:
    rows: dict[int, tuple[bytes, str]] = {}
    for raw_line in listing_text.splitlines():
        match = _H1_INSTRUCTION.match(raw_line)
        if match is None:
            continue
        statement = _normalise_statement(match.group(3))
        if not statement:
            continue
        address = int(match.group(1), 16)
        row = (bytes.fromhex("".join(match.group(2).split())), statement)
        existing = rows.get(address)
        if existing is not None and existing != row:
            raise ValueError(f"map-event direct-handoff ambiguous H1 instruction: {address:#x}")
        rows[address] = row
    return rows


def _source_table_rows(map_events: dict[str, Any]) -> dict[tuple[str, str], dict[str, Any]]:
    rows: dict[tuple[str, str], dict[str, Any]] = {}
    for category in _CATEGORIES:
        for source_file in map_events["categories"][category]["sourceFiles"]:
            key = (category, source_file["path"])
            if key in rows:
                raise ValueError("map-event direct-handoff duplicate retained source table path")
            rows[key] = source_file
    return rows


def _fresh_retained_owners(
    rom_path: Path, upstream_path: Path
) -> tuple[dict[str, Any], dict[str, Any], dict[str, Any], dict[str, Any]]:
    """Fresh-build all three predecessor contracts before deriving handoff data."""
    map_events = build_map_events_contract(rom_path, upstream_path)
    map_events_fixture = load_map_events_fixture()
    if map_events_fixture["expected"] != map_events:
        raise ValueError("map-event direct-handoff retained map-events projection drift")
    map_events_digest = hashlib.sha256(_map_events_canonical_bytes(map_events)).hexdigest().upper()
    map_events_manifest = load_json(MAP_EVENTS_MANIFEST)
    if (
        map_events_digest != map_events_manifest["outputSha256"]
        or map_events["summary"] != map_events_manifest["summary"]
    ):
        raise ValueError("map-event direct-handoff retained map-events digest drift")

    direct_state = build_map_event_direct_state_contract(rom_path, upstream_path)
    direct_state_fixture = load_json(DIRECT_STATE_FIXTURE)
    if direct_state_fixture != direct_state:
        raise ValueError("map-event direct-handoff retained direct-state projection drift")
    direct_state_digest = (
        hashlib.sha256(_direct_state_canonical_json_bytes(direct_state)).hexdigest().upper()
    )

    direct_control = build_map_event_direct_control_contract(rom_path, upstream_path)
    direct_control_fixture = load_json(DIRECT_CONTROL_FIXTURE)
    if direct_control_fixture != direct_control:
        raise ValueError("map-event direct-handoff retained direct-control projection drift")
    direct_control_digest = (
        hashlib.sha256(_direct_control_canonical_json_bytes(direct_control)).hexdigest().upper()
    )

    return (
        map_events,
        direct_state,
        direct_control,
        {
            "mapEvents": {
                "fixtureId": map_events_fixture["id"],
                "fixtureSha256": _fixture_sha256(
                    repo_path("tests/fixtures/h2/map-events-static-v1.json")
                ),
                "outputSha256": map_events_digest,
                "summary": map_events["summary"],
            },
            "eventDirectState": {
                "fixtureId": direct_state_fixture["id"],
                "fixtureSha256": _fixture_sha256(DIRECT_STATE_FIXTURE),
                "outputSha256": direct_state_digest,
                "summary": direct_state["summary"],
            },
            "eventDirectControl": {
                "fixtureId": direct_control_fixture["id"],
                "fixtureSha256": _fixture_sha256(DIRECT_CONTROL_FIXTURE),
                "outputSha256": direct_control_digest,
                "summary": direct_control["summary"],
            },
        },
    )


def _mother_corpus_projection(map_events: dict[str, Any]) -> dict[str, Any]:
    expected = {
        "entityEvents": (684, 2624),
        "zoneEvents": (150, 809),
        "itemEvents": (80, 146),
    }
    categories: list[dict[str, Any]] = []
    for category in _CATEGORIES:
        programs = map_events[_PROGRAM_FIELDS[category]]
        row = {
            "category": category,
            "programContextCount": len(programs),
            "operationCount": sum(len(program["operations"]) for program in programs),
        }
        if (
            tuple(row[key] for key in ("programContextCount", "operationCount"))
            != expected[category]
        ):
            raise ValueError(f"map-event direct-handoff retained {category} denominator drift")
        categories.append(row)
    if sum(item["programContextCount"] for item in categories) != 914:
        raise ValueError("map-event direct-handoff retained program denominator drift")
    if sum(item["operationCount"] for item in categories) != 3579:
        raise ValueError("map-event direct-handoff retained operation denominator drift")
    return {"categories": categories}


def _continuation_kind(operation: dict[str, Any]) -> str:
    if operation["family"] != "raw-68000-control-flow":
        return "ordinary"
    mnemonic = operation["mnemonic"]
    if mnemonic == "rts":
        return "return"
    if mnemonic in {"jsr", "bsr"}:
        return "direct-call"
    if mnemonic == "jmp":
        return "direct-jump"
    if mnemonic == "bra":
        return "unconditional-branch"
    if mnemonic.startswith("b"):
        return "conditional-branch"
    raise ValueError("map-event direct-handoff continuation control-flow kind drift")


def _branch_shape(operation: dict[str, Any]) -> dict[str, Any] | None:
    """Keep source branch identity, polarity mnemonic, and target as static shape."""
    if operation["family"] != "raw-68000-control-flow":
        return None
    mnemonic = operation["mnemonic"]
    if mnemonic != "bra" and not mnemonic.startswith("b"):
        return None
    target = operation["target"]
    if target is None:
        raise ValueError("map-event direct-handoff branch target drift")
    return {
        "mnemonic": mnemonic,
        "polarity": "unconditional" if mnemonic == "bra" else mnemonic,
        "targetSymbol": target["instructionTargetSymbol"],
        "targetAddress": target["instructionTargetAddress"],
    }


def _source_target_address(operation: dict[str, Any]) -> int | None:
    target = operation["target"]
    return None if target is None else target["instructionTargetAddress"]


def _emitted_macro_statement(template: str, operands: list[str]) -> str:
    """Bind an already-retained macro emission template to its source operands."""

    def replace(match: re.Match[str]) -> str:
        ordinal = int(match.group(1))
        if not 1 <= ordinal <= len(operands):
            raise ValueError("map-event direct-handoff macro operand ordinal drift")
        return operands[ordinal - 1]

    return re.sub(r"\\(\d+)", replace, template)


def _emission_shape(statement: str) -> dict[str, Any]:
    match = re.fullmatch(r"([A-Za-z_][A-Za-z0-9_]*(?:\.[A-Za-z])?)(?:\s+(.*))?", statement)
    if match is None:
        raise ValueError("map-event direct-handoff macro emission syntax drift")
    source_mnemonic, operand_text = match.groups()
    if "." in source_mnemonic:
        mnemonic, suffix = source_mnemonic.split(".", maxsplit=1)
        size_suffix: str | None = f".{suffix.lower()}"
    else:
        mnemonic = source_mnemonic
        size_suffix = None
    return {
        "sourceMnemonic": source_mnemonic,
        "mnemonic": mnemonic.lower(),
        "sizeSuffix": size_suffix,
        "operandTexts": []
        if operand_text is None
        else [item.strip() for item in operand_text.split(",")],
    }


def _normalise_h1_macro_statement(statement: str) -> str:
    statement = _normalise_statement(statement)
    if statement.startswith("M "):
        statement = statement[2:]
    return statement.lower()


def _rom_control_target(address: int, encoded: bytes) -> int | None:
    """Resolve only the source-declared relocation forms admitted by this corpus."""
    if len(encoded) < 2:
        return None
    opcode = int.from_bytes(encoded[:2], byteorder="big")
    if opcode & 0xF000 == 0x6000:
        low_byte = encoded[1]
        if len(encoded) == 2 and low_byte != 0:
            return address + 2 + int.from_bytes(encoded[1:], byteorder="big", signed=True)
        if len(encoded) == 4 and low_byte == 0:
            return address + 2 + int.from_bytes(encoded[2:], byteorder="big", signed=True)
        return None
    if opcode in {0x6100, 0x4EBA, 0x4EFA} and len(encoded) == 4:
        return address + 2 + int.from_bytes(encoded[2:], byteorder="big", signed=True)
    if opcode in {0x4EB8, 0x4EF8} and len(encoded) == 4:
        return int.from_bytes(encoded[2:], byteorder="big")
    if opcode in {0x4EB9, 0x4EF9} and len(encoded) == 6:
        return int.from_bytes(encoded[2:], byteorder="big")
    return None


def _rom_pc_relative_target(address: int, encoded: bytes) -> int | None:
    if len(encoded) != 4 or encoded[1] != 0xFA:
        return None
    return address + 2 + int.from_bytes(encoded[2:], byteorder="big", signed=True)


def _source_pc_relative_target(operation: dict[str, Any]) -> int | None:
    """Resolve the sole retained caller-side ``cs_<hex>(pc)`` source form."""
    if operation["mnemonic"] != "lea" or not operation["operandTexts"]:
        return None
    match = _CS_PC_RELATIVE.match(operation["operandTexts"][0])
    return None if match is None else int(match.group(1), 16)


def _relocation_opcode_matches(h1_bytes: bytes, rom_bytes: bytes) -> bool:
    """Keep an 8-bit branch displacement separate from its one-byte opcode."""
    if len(h1_bytes) == 2 and h1_bytes and h1_bytes[0] & 0xF0 == 0x60:
        return h1_bytes[:1] == rom_bytes[:1]
    return h1_bytes[:2] == rom_bytes[:2]


def _physical_operation(
    *,
    operation: dict[str, Any],
    source_path: str,
    operation_definitions: dict[str, dict[str, Any]],
    macro_target_address: int | None,
    h1_rows: dict[int, tuple[bytes, str]],
    rom: bytes,
    context: str,
) -> dict[str, Any]:
    """Guard one raw operation against source, H1, and ROM before projection."""
    source_shape = _emission_shape(_operation_statement(operation))
    if (
        source_shape["mnemonic"] != operation["mnemonic"]
        or source_shape["sizeSuffix"] != operation["sizeSuffix"]
    ):
        raise ValueError("map-event direct-handoff source mnemonic/size drift")
    address = operation["address"]
    macro_emissions: list[dict[str, Any]] = []
    if operation["family"] == "event-service-macro":
        definition_id = operation["definitionId"]
        definition = operation_definitions.get(definition_id)
        if (
            definition is None
            or definition["family"] != "event-service-macro"
            or definition["sourceMacro"] != operation["sourceMnemonic"]
        ):
            raise ValueError("map-event direct-handoff retained macro definition drift")
        expected_emissions = [
            _emitted_macro_statement(template, operation["operandTexts"])
            for template in definition["emissionStatementTemplates"]
        ]
        h1_bytes = b""
        rom_bytes = b""
        cursor = address
        for expected in expected_emissions:
            h1_row = h1_rows.get(cursor)
            if h1_row is None or _normalise_h1_macro_statement(h1_row[1]) != expected.lower():
                raise ValueError(f"map-event direct-handoff H1 macro emission drift: {context}")
            emitted_h1_bytes = h1_row[0]
            emitted_rom_bytes = rom[cursor : cursor + len(emitted_h1_bytes)]
            emission_target_address: int | None = None
            if emitted_rom_bytes != emitted_h1_bytes:
                emission_target_address = _rom_pc_relative_target(cursor, emitted_rom_bytes)
                if (
                    macro_target_address is None
                    or emission_target_address != macro_target_address
                    or emitted_h1_bytes[:2] != emitted_rom_bytes[:2]
                ):
                    raise ValueError(f"map-event direct-handoff H1/ROM macro byte drift: {context}")
            macro_emissions.append(
                {
                    **_emission_shape(expected),
                    "targetAddress": emission_target_address,
                    "instructionByteLength": len(emitted_h1_bytes),
                    "h1InstructionSha256": hashlib.sha256(emitted_h1_bytes).hexdigest().upper(),
                    "romInstructionSha256": hashlib.sha256(emitted_rom_bytes).hexdigest().upper(),
                }
            )
            h1_bytes += emitted_h1_bytes
            rom_bytes += emitted_rom_bytes
            cursor += len(emitted_h1_bytes)
    else:
        expected_statement = _operation_statement(operation)
        h1_row = h1_rows.get(address)
        if h1_row is None or h1_row[1] != expected_statement:
            raise ValueError(f"map-event direct-handoff H1 opcode/operand/order drift: {context}")
        h1_bytes = h1_row[0]
        rom_bytes = rom[address : address + len(h1_bytes)]
        target_address = _source_target_address(operation)
        source_pc_relative_target = _source_pc_relative_target(operation)
        if rom_bytes != h1_bytes:
            resolved_branch_target = _rom_control_target(address, rom_bytes)
            resolved_pc_relative_target = _rom_pc_relative_target(address, rom_bytes)
            if not _relocation_opcode_matches(h1_bytes, rom_bytes) or (
                (
                    target_address is None
                    or resolved_branch_target is None
                    or resolved_branch_target != target_address
                )
                and (
                    source_pc_relative_target is None
                    or resolved_pc_relative_target != source_pc_relative_target
                )
            ):
                raise ValueError(
                    f"map-event direct-handoff H1/ROM instruction-byte drift: {context}"
                )
    return {
        "romPc": address,
        "family": operation["family"],
        "sourceMnemonic": operation["sourceMnemonic"],
        "mnemonic": operation["mnemonic"],
        "sizeSuffix": operation["sizeSuffix"],
        "operandTexts": operation["operandTexts"],
        "controlFlowKind": operation["controlFlowKind"],
        "branch": _branch_shape(operation),
        "macroEmissions": macro_emissions,
        "instructionByteLength": len(h1_bytes),
        "h1InstructionSha256": hashlib.sha256(h1_bytes).hexdigest().upper(),
        "romInstructionSha256": hashlib.sha256(rom_bytes).hexdigest().upper(),
        "contextReferences": [],
        "directStateAccessSiteIds": [],
        "symbolicImmediateIds": [],
    }


def _program_context_id(category: str, program: dict[str, Any]) -> str:
    return f"{category}|{program['canonicalSymbol']}|{program['entryAddress']}"


def _source_file_id(table: dict[str, Any]) -> str:
    return table["symbol"]


def _transfer_handoff_id(site_order: int) -> str:
    return f"transfer:{site_order}"


def _operation_id(address: int) -> str:
    return f"operation:{address:06X}"


def _setup_operation_id(transfer_id: str, setup_index: int) -> str:
    return f"setup:{transfer_id}:{setup_index}"


def _call_continuation_id(transfer_id: str) -> str:
    return f"continuation:{transfer_id}"


def _direct_state_access_index(
    direct_state: dict[str, Any],
) -> dict[tuple[str, str, int, int], list[str]]:
    result: dict[tuple[str, str, int, int], list[str]] = defaultdict(list)
    state = direct_state["eventDirectState"]
    if len(state["accessSiteOrder"]) != len(state["accessSites"]):
        raise ValueError("map-event direct-handoff retained direct-state access ID drift")
    for access_id, access in zip(state["accessSiteOrder"], state["accessSites"], strict=True):
        result[
            (
                access["category"],
                access["programSymbol"],
                access["programEntryAddress"],
                access["romPc"],
            )
        ].append(access_id)
    return dict(result)


def _append_physical_context(
    *,
    physical_operations: dict[str, dict[str, Any]],
    operation: dict[str, Any],
    source_path: str,
    operation_definitions: dict[str, dict[str, Any]],
    macro_target_address: int | None,
    source_context: str,
    role: str,
    role_id: str,
    role_index: int,
    direct_state_accesses: list[str],
    h1_rows: dict[int, tuple[bytes, str]],
    rom: bytes,
) -> str:
    physical_id = _operation_id(operation["address"])
    physical = physical_operations.get(physical_id)
    if physical is None:
        physical = _physical_operation(
            operation=operation,
            source_path=source_path,
            operation_definitions=operation_definitions,
            macro_target_address=macro_target_address,
            h1_rows=h1_rows,
            rom=rom,
            context=source_context,
        )
        physical_operations[physical_id] = physical
    else:
        candidate = _physical_operation(
            operation=operation,
            source_path=source_path,
            operation_definitions=operation_definitions,
            macro_target_address=macro_target_address,
            h1_rows=h1_rows,
            rom=rom,
            context=source_context,
        )
        comparable_fields = (
            "romPc",
            "family",
            "sourceMnemonic",
            "mnemonic",
            "sizeSuffix",
            "operandTexts",
            "controlFlowKind",
            "branch",
            "macroEmissions",
            "instructionByteLength",
            "h1InstructionSha256",
            "romInstructionSha256",
        )
        if any(physical[field] != candidate[field] for field in comparable_fields):
            raise ValueError("map-event direct-handoff shared physical operation drift")
    reference = {
        "role": role,
        "roleId": role_id,
        "roleIndex": role_index,
        "sourcePath": source_path,
        "sourceLine": operation["sourceLine"],
        "sourceOrder": operation["sourceOrder"],
    }
    if reference in physical["contextReferences"]:
        raise ValueError("map-event direct-handoff duplicate physical context reference")
    physical["contextReferences"].append(reference)
    for access_site_id in direct_state_accesses:
        if access_site_id not in physical["directStateAccessSiteIds"]:
            physical["directStateAccessSiteIds"].append(access_site_id)
    return physical_id


def _symbolic_immediate_rows(
    *,
    physical_operations: dict[str, dict[str, Any]],
    enum_definitions: dict[str, dict[str, int]],
    rom_bytes: bytes,
) -> dict[str, dict[str, Any]]:
    def encoded_value(operation: dict[str, Any]) -> int:
        instruction = rom_bytes[
            operation["romPc"] : operation["romPc"] + operation["instructionByteLength"]
        ]
        if len(instruction) != operation["instructionByteLength"]:
            raise ValueError("map-event direct-handoff immediate ROM span drift")
        if operation["mnemonic"] == "moveq":
            return instruction[1]
        if operation["mnemonic"] not in {"move", "andi"}:
            raise ValueError("map-event direct-handoff unsupported symbolic immediate encoding")
        suffix = operation["sizeSuffix"]
        if suffix == ".b":
            return instruction[3]
        if suffix == ".w":
            return int.from_bytes(instruction[2:4], byteorder="big")
        if suffix == ".l":
            return int.from_bytes(instruction[2:6], byteorder="big")
        raise ValueError("map-event direct-handoff symbolic immediate size drift")

    rows: dict[str, dict[str, Any]] = {}
    for physical_id, operation in physical_operations.items():
        for operand_index, operand in enumerate(operation["operandTexts"]):
            if not operand.startswith("#"):
                continue
            symbol = operand[1:]
            definition = enum_definitions.get(symbol)
            if definition is None:
                continue
            if operand_index != 0 or encoded_value(operation) != definition["value"]:
                raise ValueError("map-event direct-handoff authoritative enum value drift")
            immediate_id = f"{symbol}|{definition['value']}"
            row = rows.setdefault(
                immediate_id,
                {
                    "symbol": symbol,
                    "value": definition["value"],
                    "enumSourceLine": definition["sourceLine"],
                    "uses": [],
                },
            )
            for context_reference in operation["contextReferences"]:
                use = {
                    "physicalOperationId": physical_id,
                    "role": context_reference["role"],
                    "roleId": context_reference["roleId"],
                    "roleIndex": context_reference["roleIndex"],
                    "operandIndex": operand_index,
                }
                if use not in row["uses"]:
                    row["uses"].append(use)
            if immediate_id not in operation["symbolicImmediateIds"]:
                operation["symbolicImmediateIds"].append(immediate_id)
    return rows


def _handoff_projection(
    map_events: dict[str, Any],
    direct_state: dict[str, Any],
    direct_control: dict[str, Any],
    *,
    upstream_path: Path,
    rom_path: Path,
) -> tuple[dict[str, Any], dict[str, Any], dict[str, Any]]:
    """Derive handoff operations from retained transfer contexts with pre-golden guards."""
    root = upstream_path.resolve(strict=True)
    disasm = root / "disasm"
    if not disasm.is_dir():
        disasm = root
    listing_path = root / "build/sf2build-h1.lst"
    enums_path = disasm / "sf2enums.asm"
    listing_text = listing_path.read_text(encoding="utf-8")
    enum_source = enums_path.read_text(encoding="utf-8")
    rom = rom_path.resolve(strict=True).read_bytes()
    h1_rows = _h1_instruction_rows(listing_text)
    enum_definitions = _parse_equates(enum_source)
    retained_enum_identity = next(
        (
            row
            for row in direct_state["sourceContext"]["sourceIdentities"]
            if row["path"] == "sf2enums.asm"
        ),
        None,
    )
    if (
        retained_enum_identity is None
        or retained_enum_identity["sha256"]
        != hashlib.sha256(enums_path.read_bytes()).hexdigest().upper()
    ):
        raise ValueError("map-event direct-handoff authoritative sf2enums identity drift")
    operation_definitions = {row["definitionId"]: row for row in map_events["operationDefinitions"]}
    if len(operation_definitions) != len(map_events["operationDefinitions"]):
        raise ValueError("map-event direct-handoff retained operation definition identity drift")
    script_macro_targets = {
        (
            row["category"],
            row["callerProgramCanonicalSymbol"],
            row["callerProgramEntryAddress"],
            row["operationAddress"],
        ): row["instructionTargetAddress"]
        for row in map_events["scriptInvocationSites"]
    }
    if len(script_macro_targets) != len(map_events["scriptInvocationSites"]):
        raise ValueError("map-event direct-handoff retained script macro target identity drift")
    table_rows = _source_table_rows(map_events)
    state_accesses = _direct_state_access_index(direct_state)
    direct_control_rows = direct_control["eventDirectControl"]["transferSites"]
    if len(direct_control_rows) != 205:
        raise ValueError(
            "map-event direct-handoff retained direct-control transfer denominator drift"
        )
    alias_ids = {
        "|".join(
            (
                row["instructionTargetSymbol"],
                str(row["instructionTargetAddress"]),
                row["effectiveTargetSymbol"],
                str(row["effectiveTargetAddress"]),
            )
        ): f"alias:{index}"
        for index, row in enumerate(direct_control["eventDirectControl"]["aliasJoins"])
    }
    effective_target_ids = {
        f"{row['symbol']}|{row['entryAddress']}": f"effective-target:{index}"
        for index, row in enumerate(direct_control["eventDirectControl"]["effectiveTargets"])
    }
    if len(alias_ids) != len(direct_control["eventDirectControl"]["aliasJoins"]) or len(
        effective_target_ids
    ) != len(direct_control["eventDirectControl"]["effectiveTargets"]):
        raise ValueError("map-event direct-handoff retained control ID identity drift")

    source_files: dict[str, dict[str, Any]] = {}
    program_contexts: dict[str, dict[str, Any]] = {}
    transfer_handoffs: dict[str, dict[str, Any]] = {}
    setup_operations: dict[str, dict[str, Any]] = {}
    call_continuations: dict[str, dict[str, Any]] = {}
    physical_operations: dict[str, dict[str, Any]] = {}
    transfer_count_by_program: Counter[str] = Counter()
    control_by_site_order = {row["siteOrder"]: row for row in direct_control_rows}
    seen_transfer_orders: set[int] = set()

    transfer_site_order = 0
    for category in _CATEGORIES:
        for program in map_events[_PROGRAM_FIELDS[category]]:
            program_id = _program_context_id(category, program)
            program_contexts[program_id] = {
                "category": category,
                "programSymbol": program["canonicalSymbol"],
                "programEntryAddress": program["entryAddress"],
                "sourcePath": program["sourcePath"],
                "transferHandoffCount": 0,
            }
            operations = program["operations"]
            if [operation["sourceOrder"] for operation in operations] != list(
                range(len(operations))
            ):
                raise ValueError("map-event direct-handoff retained program source order drift")
            for operation_index, transfer in enumerate(operations):
                if (
                    transfer["family"] != "raw-68000-control-flow"
                    or transfer["mnemonic"] not in _TRANSFER_MNEMONICS
                ):
                    continue
                control = control_by_site_order.get(transfer_site_order)
                if control is None:
                    raise ValueError("map-event direct-handoff retained transfer site order drift")
                if (
                    control["category"] != category
                    or control["programSymbol"] != program["canonicalSymbol"]
                    or control["programEntryAddress"] != program["entryAddress"]
                    or control["romPc"] != transfer["address"]
                ):
                    raise ValueError(
                        "map-event direct-handoff retained transfer context join drift"
                    )
                table = table_rows.get((category, control["sourcePath"]))
                if table is None or table["symbol"] != control["tableSymbol"]:
                    raise ValueError("map-event direct-handoff retained caller-table join drift")
                source_file = source_files.setdefault(
                    _source_file_id(table),
                    {
                        "category": category,
                        "tableSymbol": table["symbol"],
                        "tableEntryAddress": table["address"],
                        "sourcePath": table["path"],
                        "transferHandoffCount": 0,
                    },
                )
                transfer_id = _transfer_handoff_id(transfer_site_order)
                if transfer_id in transfer_handoffs:
                    raise ValueError("map-event direct-handoff duplicate transfer context")
                seen_transfer_orders.add(transfer_site_order)

                setup_start = operation_index
                while (
                    setup_start > 0
                    and operations[setup_start - 1]["family"] == "raw-68000-instruction"
                ):
                    setup_start -= 1
                setup_ids: list[str] = []
                for setup_index, setup in enumerate(operations[setup_start:operation_index]):
                    setup_id = _setup_operation_id(transfer_id, setup_index)
                    setup_ids.append(setup_id)
                    physical_id = _append_physical_context(
                        physical_operations=physical_operations,
                        operation=setup,
                        source_path=program["sourcePath"],
                        operation_definitions=operation_definitions,
                        macro_target_address=script_macro_targets.get(
                            (
                                category,
                                program["canonicalSymbol"],
                                program["entryAddress"],
                                setup["address"],
                            )
                        ),
                        source_context=f"{program['canonicalSymbol']}:{setup['sourceLine']}",
                        role="setup",
                        role_id=transfer_id,
                        role_index=setup_index,
                        direct_state_accesses=state_accesses.get(
                            (
                                category,
                                program["canonicalSymbol"],
                                program["entryAddress"],
                                setup["address"],
                            ),
                            [],
                        ),
                        h1_rows=h1_rows,
                        rom=rom,
                    )
                    setup_operations[setup_id] = {
                        "transferHandoffId": transfer_id,
                        "setupIndex": setup_index,
                        "physicalOperationId": physical_id,
                    }

                continuation_id: str | None = None
                if transfer["mnemonic"] in {"jsr", "bsr"}:
                    if operation_index + 1 >= len(operations):
                        raise ValueError(
                            "map-event direct-handoff returning call lacks continuation"
                        )
                    continuation = operations[operation_index + 1]
                    if continuation["sourceOrder"] != transfer["sourceOrder"] + 1:
                        raise ValueError(
                            "map-event direct-handoff first lexical continuation order drift"
                        )
                    continuation_id = _call_continuation_id(transfer_id)
                    physical_id = _append_physical_context(
                        physical_operations=physical_operations,
                        operation=continuation,
                        source_path=program["sourcePath"],
                        operation_definitions=operation_definitions,
                        macro_target_address=script_macro_targets.get(
                            (
                                category,
                                program["canonicalSymbol"],
                                program["entryAddress"],
                                continuation["address"],
                            )
                        ),
                        source_context=f"{program['canonicalSymbol']}:{continuation['sourceLine']}",
                        role="call-continuation",
                        role_id=transfer_id,
                        role_index=0,
                        direct_state_accesses=state_accesses.get(
                            (
                                category,
                                program["canonicalSymbol"],
                                program["entryAddress"],
                                continuation["address"],
                            ),
                            [],
                        ),
                        h1_rows=h1_rows,
                        rom=rom,
                    )
                    call_continuations[continuation_id] = {
                        "transferHandoffId": transfer_id,
                        "directControlTransferSiteOrder": transfer_site_order,
                        "kind": _continuation_kind(continuation),
                        "physicalOperationId": physical_id,
                    }

                alias_reference = None
                if (
                    control["instructionTargetSymbol"] != control["effectiveTargetSymbol"]
                    or control["instructionTargetAddress"] != control["effectiveTargetAddress"]
                ):
                    alias_reference = alias_ids.get(
                        "|".join(
                            (
                                control["instructionTargetSymbol"],
                                str(control["instructionTargetAddress"]),
                                control["effectiveTargetSymbol"],
                                str(control["effectiveTargetAddress"]),
                            )
                        )
                    )
                    if alias_reference is None:
                        raise ValueError("map-event direct-handoff retained alias reference drift")
                effective_target_reference = effective_target_ids.get(
                    f"{control['effectiveTargetSymbol']}|{control['effectiveTargetAddress']}"
                )
                if effective_target_reference is None:
                    raise ValueError(
                        "map-event direct-handoff retained effective-target reference drift"
                    )
                transfer_handoffs[transfer_id] = {
                    "programContextId": program_id,
                    "directControlTransferSiteOrder": transfer_site_order,
                    "directControlAliasReference": alias_reference,
                    "directControlEffectiveTargetReference": effective_target_reference,
                    "setupOperationIds": setup_ids,
                    "callContinuationId": continuation_id,
                }
                transfer_count_by_program[program_id] += 1
                source_file["transferHandoffCount"] += 1
                transfer_site_order += 1

    if seen_transfer_orders != set(range(205)):
        raise ValueError("map-event direct-handoff retained transfer coverage drift")
    for program_id, count in transfer_count_by_program.items():
        program_contexts[program_id]["transferHandoffCount"] = count

    if len(source_files) != 53:
        raise ValueError("map-event direct-handoff caller-table source denominator drift")
    if set(source_files) != set(direct_control["eventDirectControl"]["sourceFiles"]):
        raise ValueError("map-event direct-handoff retained caller-table source projection drift")
    if len(program_contexts) != 914 or len(transfer_handoffs) != 205:
        raise ValueError("map-event direct-handoff program/transfer denominator drift")
    setup_distribution = Counter(
        len(row["setupOperationIds"]) for row in transfer_handoffs.values()
    )
    if setup_distribution != {0: 56, 1: 118, 2: 29, 4: 2}:
        raise ValueError("map-event direct-handoff setup distribution drift")
    if len(setup_operations) != 184:
        raise ValueError("map-event direct-handoff setup operation denominator drift")
    if len(call_continuations) != 143:
        raise ValueError("map-event direct-handoff continuation denominator drift")
    continuation_kinds = Counter(row["kind"] for row in call_continuations.values())
    if continuation_kinds != {
        "ordinary": 72,
        "return": 57,
        "direct-call": 6,
        "unconditional-branch": 6,
        "conditional-branch": 1,
        "direct-jump": 1,
    }:
        raise ValueError("map-event direct-handoff continuation kind distribution drift")
    if len(physical_operations) != 299:
        raise ValueError("map-event direct-handoff physical operation denominator drift")
    setup_physical_ids = {row["physicalOperationId"] for row in setup_operations.values()}
    continuation_physical_ids = {row["physicalOperationId"] for row in call_continuations.values()}
    if (
        len(setup_physical_ids) != 177
        or len(continuation_physical_ids) != 139
        or len(setup_physical_ids & continuation_physical_ids) != 17
    ):
        raise ValueError("map-event direct-handoff contextual/physical overlap drift")
    if sum(len(row["contextReferences"]) for row in physical_operations.values()) != 327:
        raise ValueError("map-event direct-handoff contextual operation denominator drift")

    symbolic_immediates = _symbolic_immediate_rows(
        physical_operations=physical_operations,
        enum_definitions=enum_definitions,
        rom_bytes=rom,
    )
    if (
        len(symbolic_immediates) != 78
        or sum(len(row["uses"]) for row in symbolic_immediates.values()) != 120
    ):
        raise ValueError("map-event direct-handoff symbolic immediate denominator drift")

    ordered_source_files = sorted(source_files)
    ordered_program_contexts = list(program_contexts)
    ordered_transfer_handoffs = list(transfer_handoffs)
    ordered_setup_operations = list(setup_operations)
    ordered_call_continuations = list(call_continuations)
    ordered_physical_operations = sorted(
        physical_operations, key=lambda item: physical_operations[item]["romPc"]
    )
    ordered_symbolic_immediates = sorted(symbolic_immediates)
    for physical_id in ordered_physical_operations:
        physical_operations[physical_id]["contextReferences"].sort(
            key=lambda row: (row["role"], row["roleId"], row["roleIndex"])
        )
        physical_operations[physical_id]["directStateAccessSiteIds"].sort()
        physical_operations[physical_id]["symbolicImmediateIds"].sort()

    source_identities = [
        {
            "path": source_files[key]["sourcePath"],
            "sha256": hashlib.sha256((disasm / source_files[key]["sourcePath"]).read_bytes())
            .hexdigest()
            .upper(),
        }
        for key in ordered_source_files
    ] + [
        {
            "path": "sf2enums.asm",
            "sha256": hashlib.sha256(enums_path.read_bytes()).hexdigest().upper(),
        }
    ]
    if len(source_identities) != 54 or len({row["path"] for row in source_identities}) != 54:
        raise ValueError("map-event direct-handoff source identity denominator drift")

    event_direct_handoff = {
        "sourceFileOrder": ordered_source_files,
        "sourceFiles": source_files,
        "programContextOrder": ordered_program_contexts,
        "programContexts": program_contexts,
        "transferHandoffOrder": ordered_transfer_handoffs,
        "transferHandoffs": transfer_handoffs,
        "setupOperationOrder": ordered_setup_operations,
        "setupOperations": setup_operations,
        "callContinuationOrder": ordered_call_continuations,
        "callContinuations": call_continuations,
        "physicalOperationOrder": ordered_physical_operations,
        "physicalOperations": physical_operations,
        "symbolicImmediateOrder": ordered_symbolic_immediates,
        "symbolicImmediates": symbolic_immediates,
        "digests": {
            "transferHandoffsSha256": hashlib.sha256(
                canonical_json_bytes({"transferHandoffs": transfer_handoffs})
            )
            .hexdigest()
            .upper(),
            "setupOperationsSha256": hashlib.sha256(
                canonical_json_bytes({"setupOperations": setup_operations})
            )
            .hexdigest()
            .upper(),
            "callContinuationsSha256": hashlib.sha256(
                canonical_json_bytes({"callContinuations": call_continuations})
            )
            .hexdigest()
            .upper(),
            "physicalOperationsSha256": hashlib.sha256(
                canonical_json_bytes({"physicalOperations": physical_operations})
            )
            .hexdigest()
            .upper(),
            "symbolicImmediatesSha256": hashlib.sha256(
                canonical_json_bytes({"symbolicImmediates": symbolic_immediates})
            )
            .hexdigest()
            .upper(),
        },
    }
    summary = {
        "sourceIdentityCount": 54,
        "programContextCount": 914,
        "contextTransferSiteCount": 205,
        "setupEmptyTransferCount": setup_distribution[0],
        "setupOneOperationTransferCount": setup_distribution[1],
        "setupTwoOperationTransferCount": setup_distribution[2],
        "setupFourOperationTransferCount": setup_distribution[4],
        "nonemptySetupTransferCount": 149,
        "contextSetupOperationCount": 184,
        "physicalSetupOperationCount": 177,
        "contextCallContinuationCount": 143,
        "physicalCallContinuationCount": 139,
        "contextOperationCount": 327,
        "physicalOperationCount": 299,
        "contextualPhysicalOverlapCount": 17,
        "symbolicImmediateIdentityCount": 78,
        "symbolicImmediateUseCount": 120,
    }
    return (
        event_direct_handoff,
        summary,
        {
            "h1Listing": {
                "path": "build/sf2build-h1.lst",
                "sha256": hashlib.sha256(listing_text.encode("utf-8")).hexdigest().upper(),
            },
            "sourceIdentities": source_identities,
        },
    )


def build_map_event_direct_handoff_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    """Build the public source/H1/ROM handoff contract."""
    map_events, direct_state, direct_control, retained_owners = _fresh_retained_owners(
        rom_path, upstream_path
    )
    event_direct_handoff, summary, source_context = _handoff_projection(
        map_events,
        direct_state,
        direct_control,
        upstream_path=upstream_path,
        rom_path=rom_path,
    )
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": map_events["upstream"],
        "romSha256": map_events["romSha256"],
        "scope": _mother_corpus_projection(map_events),
        "sourceContext": source_context,
        "retainedOwners": retained_owners,
        "eventDirectHandoff": event_direct_handoff,
        "unknowns": {key: "Unknown" for key in _UNKNOWN_KEYS},
        "summary": summary,
    }


def verify_map_event_direct_handoff_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    """Validate the closed fixture then atomically rebuild and compare it."""
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner=str(FIXTURE))
    _validate_contract_order(fixture, load_json(SCHEMA))
    output = build_map_event_direct_handoff_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="map-event direct-handoff static contract")
    _validate_contract_order(output, load_json(SCHEMA))
    if fixture != output:
        raise ValueError("map-event direct-handoff complete semantic fixture drift")
    digest = hashlib.sha256(canonical_json_bytes(output)).hexdigest().upper()
    destination = output_path or repo_path("local/derived/map-event-direct-handoff-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(
        json.dumps(output, ensure_ascii=False, indent=2).encode("utf-8") + b"\n"
    )
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Operations": output["summary"]["physicalOperationCount"],
        "Status": "PASS",
    }
