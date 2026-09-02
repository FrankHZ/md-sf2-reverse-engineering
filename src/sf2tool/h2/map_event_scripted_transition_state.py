"""Static content-semantic projection for Map 21's scripted transition stream.

This owner deliberately projects one selected source/H1/ROM program.  It does
not infer that the program runs in a natural playthrough, nor any handler or
payload effect; those caller-dependent questions remain a closed Unknown
register in the public fixture.
"""

from __future__ import annotations

import hashlib
import json
import re
from collections import Counter
from copy import deepcopy
from pathlib import Path
from typing import Any

from sf2tool.h2.map_event_direct_handoff import _h1_instruction_rows
from sf2tool.h2.map_events import build_map_events_contract
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.source_text import read_upstream_text

ID = "sf2-map-event-scripted-transition-state-static-v1"
FIXTURE = repo_path("tests/fixtures/h2/map-event-scripted-transition-state-static-v1.json")
SCHEMA = repo_path("schemas/h2/map-event-scripted-transition-state-static-fixture.schema.json")
_ROM_SHA256 = "9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9"
_UPSTREAM_COMMIT = "c834c652b6862bc5679fd7f69a38a7093206efc6"
_PREDECESSOR_INDEX_SHA256 = "9A08422491985FF3277A11A1F2BFE2277D3D379FF12681EF835F44AF70CB671D"
_SOURCE_PATHS = (
    "data/maps/entries/map44/mapsetups/scripts.asm",
    "sf2cutscenemacros.asm",
    "sf2enums.asm",
    "data/scripting/entity/eas_actions.asm",
    "data/scripting/entity/eas_main.asm",
    "code/common/scripting/map/mapscriptengine_1.asm",
    "code/common/scripting/map/mapscriptengine_2.asm",
    "code/common/scripting/entity/entityscriptengine_2.asm",
)
_PROGRAM_SYMBOL = "Map21_DefaultZoneEvent"
_PROGRAM_ENTRY = 0x545B6
_PROGRAM_END_EXCLUSIVE = 0x54714
_TERMINAL_ADDRESS = 0x54712
_PROGRAM_SPAN_BYTES = 350
_SELECTED_FAMILIES = frozenset(
    {
        "map-script-macro",
        "entity-action-wrapper",
        "entity-action-command",
        "entity-action-payload-command",
        "stream-terminator",
    }
)
_EXPECTED_SOURCE_MACRO_COUNTS = {
    "csWait": 17,
    "setFacing": 9,
    "setCamDest": 4,
    "setBlocks": 9,
    "nextSingleText": 6,
    "setActscriptWait": 3,
    "entityActions": 2,
    "customActscriptWait": 1,
    "moveRight": 7,
    "moveUp": 4,
    "moveDown": 1,
    "moveLeft": 1,
    "endActions": 3,
    "ac_setSpeed": 1,
    "ac_jump": 1,
    "ac_end": 1,
    "clearF": 2,
    "executeSubroutine": 1,
    "loadMapFadeIn": 1,
    "loadMapEntities": 1,
    "setSprite": 1,
    "fadeInB": 1,
    "setQuake": 5,
    "playSound": 1,
    "flashScreenWhite": 2,
    "warp": 1,
    "csc_end": 1,
}
_POINTER_TARGETS = ("csub_54714", "ce_54736", "eas_Jump", "eas_Init", "eas_Idle")
_UNKNOWN_KEYS = (
    "naturalProgramReachability",
    "callerEntryState",
    "actualScriptExecution",
    "runtimeEntityIdentityAndState",
    "runtimeEntityMovementAndFacing",
    "runtimeCameraProgression",
    "runtimeMapBlockEffects",
    "runtimeSubroutineEffects",
    "runtimeMapAndEntityLoadEffects",
    "runtimeWarpCompletionAndDestination",
    "runtimeFlagValuesLifetimeAndPersistence",
    "runtimeDialogueAudioAndPresentation",
    "runtimeTimingAndCadence",
    "postScriptControlAndEndpoint",
)
_EQUATE = re.compile(r"^\s*([A-Za-z_][A-Za-z0-9_]*)\s*:\s+equ\s+([^\s;]+)")
_LABEL = re.compile(r"^\s*([A-Za-z_@][A-Za-z0-9_@]*)\s*:")
_OPERATION = re.compile(r"^\s*([A-Za-z][A-Za-z0-9_]*)(?:\s+(.*?))?\s*$")


def canonical_json_bytes(value: dict[str, Any]) -> bytes:
    """Return the canonical public-fixture encoding."""
    return (
        json.dumps(value, ensure_ascii=False, sort_keys=True, separators=(",", ":")).encode("utf-8")
        + b"\n"
    )


def _root(upstream_path: Path) -> Path:
    root = upstream_path.resolve(strict=True)
    return root / "disasm" if (root / "disasm").is_dir() else root


def _sha(value: bytes) -> str:
    return hashlib.sha256(value).hexdigest().upper()


def _validate_order(value: dict[str, Any]) -> None:
    """Close order-sensitive public arrays beyond JSON Schema's object rules."""
    if list(value) != [
        "schemaVersion",
        "id",
        "upstreamCommit",
        "romSha256",
        "sourceIdentity",
        "scriptedTransitionState",
        "unknowns",
        "unknownOrder",
    ]:
        raise ValueError("scripted-transition root field order drift")
    if [row["sourcePath"] for row in value["sourceIdentity"]] != list(_SOURCE_PATHS):
        raise ValueError("scripted-transition source identity order drift")
    if list(value["unknowns"]) != list(_UNKNOWN_KEYS) or value["unknownOrder"] != list(
        _UNKNOWN_KEYS
    ):
        raise ValueError("scripted-transition Unknown queue order drift")
    state = value["scriptedTransitionState"]
    if list(state) != [
        "sourceProgram",
        "selectionSummary",
        "sourceMacroCounts",
        "operationRows",
        "commandDefinitions",
        "payloadContexts",
        "pointerTargets",
        "retainedHandlers",
    ]:
        raise ValueError("scripted-transition state field order drift")
    if [row["sourceMacro"] for row in state["sourceMacroCounts"]] != list(
        _EXPECTED_SOURCE_MACRO_COUNTS
    ):
        raise ValueError("scripted-transition source macro order drift")
    if [row["sourceOrder"] for row in state["operationRows"]] != list(range(87)):
        raise ValueError("scripted-transition operation order drift")
    if len({row["definitionId"] for row in state["commandDefinitions"]}) != 27:
        raise ValueError("scripted-transition definition identity drift")
    if [row["symbol"] for row in state["pointerTargets"]] != list(_POINTER_TARGETS):
        raise ValueError("scripted-transition pointer order drift")


def _parse_number(token: str) -> int | None:
    if re.fullmatch(r"0|[1-9][0-9]*", token):
        return int(token)
    if re.fullmatch(r"\$[0-9A-Fa-f]+", token):
        return int(token[1:], 16)
    return None


def _equates(disasm: Path) -> dict[str, int]:
    values: dict[str, int] = {}
    for line in read_upstream_text(disasm / "sf2enums.asm").splitlines():
        match = _EQUATE.match(line)
        if match is None:
            continue
        value = _parse_number(match.group(2))
        if value is not None:
            values[match.group(1)] = value
    if not values:
        raise ValueError("scripted-transition enum source drift")
    return values


def _source_identity(disasm: Path) -> list[dict[str, str]]:
    identities: list[dict[str, str]] = []
    for source_path in _SOURCE_PATHS:
        path = disasm / source_path
        if not path.is_file():
            raise ValueError(f"scripted-transition source path missing: {source_path}")
        identities.append({"sourcePath": source_path, "sha256": _sha(path.read_bytes())})
    return identities


def _all_programs(map_events: dict[str, Any]) -> list[dict[str, Any]]:
    fields = ("entityTargetPrograms", "zoneTargetPrograms", "itemTargetPrograms")
    programs = [program for field in fields for program in map_events[field]]
    identities = {(row["canonicalSymbol"], row["entryAddress"]) for row in programs}
    if len(programs) != 914 or len(identities) != 914:
        raise ValueError("scripted-transition mother program denominator drift")
    return programs


def _selected_programs(map_events: dict[str, Any]) -> tuple[list[dict[str, Any]], int]:
    programs = _all_programs(map_events)
    selected = [
        program
        for program in programs
        if any(operation["family"] in _SELECTED_FAMILIES for operation in program["operations"])
    ]
    if len(selected) != 1:
        raise ValueError("scripted-transition selected program denominator drift")
    program = selected[0]
    if (
        program["canonicalSymbol"] != _PROGRAM_SYMBOL
        or program["entryAddress"] != _PROGRAM_ENTRY
        or program["sourcePath"] != _SOURCE_PATHS[0]
    ):
        raise ValueError("scripted-transition selected program identity drift")
    return selected, len(programs) - len(selected)


def _find_label(disasm: Path, symbol: str) -> tuple[str, int]:
    matches: list[tuple[str, int]] = []
    for source_path in _SOURCE_PATHS:
        for line_number, line in enumerate(
            read_upstream_text(disasm / source_path).splitlines(), start=1
        ):
            match = _LABEL.match(line)
            if match is not None and match.group(1) == symbol:
                matches.append((source_path, line_number))
    if len(matches) != 1:
        raise ValueError(f"scripted-transition source label drift: {symbol}")
    return matches[0]


def _source_operation(source: str, *, source_line: int) -> tuple[str, list[str]]:
    """Parse one source operation, excluding a comment or label from the command stream."""
    statement = source.split(";", 1)[0].strip()
    label = _LABEL.match(statement)
    if label is not None:
        statement = statement[label.end() :].strip()
    match = _OPERATION.fullmatch(statement)
    if match is None:
        raise ValueError(f"scripted-transition source operation syntax drift: {source_line}")
    operands = (
        [] if match.group(2) is None else [value.strip() for value in match.group(2).split(",")]
    )
    return match.group(1), operands


def _guard_program_source(disasm: Path, program: dict[str, Any]) -> None:
    lines = read_upstream_text(disasm / program["sourcePath"]).splitlines()
    for operation in program["operations"]:
        source_line = operation["sourceLine"]
        macro, operands = _source_operation(lines[source_line - 1], source_line=source_line)
        if macro != operation["sourceMnemonic"] or operands != operation["operandTexts"]:
            raise ValueError(f"scripted-transition source operation drift: {source_line}")


def _guard_definition_sources(disasm: Path, definitions: list[dict[str, Any]]) -> None:
    """Guard each selected macro's source header and emitted encoding before golden comparison."""
    lines = read_upstream_text(disasm / "sf2cutscenemacros.asm").splitlines()

    def canonical(statement: str) -> str:
        normalized = re.sub(r"\s+", " ", statement)
        normalized = re.sub(r"\$[0-9A-Fa-f]+", lambda match: match.group(0).lower(), normalized)
        return re.sub(r"\$0+([0-9A-Fa-f]+)", lambda match: "$" + match.group(1), normalized)

    for definition in definitions:
        start = definition["definitionSourceLine"] - 1
        if lines[start].split(";", 1)[0].strip() != f"{definition['sourceMacro']}: macro":
            raise ValueError(
                f"scripted-transition macro definition header drift: {definition['sourceMacro']}"
            )
        emitted: list[str] = []
        for line in lines[start + 1 :]:
            statement = line.split(";", 1)[0].strip()
            if statement == "endm":
                break
            if statement:
                emitted.append(canonical(statement))
        else:
            raise ValueError(
                "scripted-transition macro definition terminator drift: "
                f"{definition['sourceMacro']}"
            )
        expected = [canonical(value) for value in definition["emissionStatementTemplates"]]
        alias = definition["engineCatalog"] and definition["engineCatalog"].get("aliasOf")
        alias_body = (
            len(emitted) == 1
            and alias is not None
            and emitted[0].split(" ", 1)[0].lower() == alias.lower()
        )
        if not alias_body and emitted != expected:
            raise ValueError(
                "scripted-transition macro definition emission/order drift: "
                f"{definition['sourceMacro']}"
            )


def _operand(
    token: str, *, enum_values: dict[str, int], symbol_addresses: dict[str, int]
) -> dict[str, Any]:
    value = _parse_number(token)
    if value is not None:
        return {"kind": "numeric", "value": value}
    if token in enum_values:
        return {"kind": "enum", "symbol": token, "value": enum_values[token]}
    if token in _POINTER_TARGETS:
        address = symbol_addresses.get(token)
        if address is None:
            raise ValueError(f"scripted-transition pointer address missing: {token}")
        return {"kind": "pointer", "symbol": token, "address": address}
    raise ValueError(f"scripted-transition operand is neither numeric nor resolved enum: {token}")


def _definition_projection(definition: dict[str, Any]) -> dict[str, Any]:
    catalog = definition["engineCatalog"]
    if catalog is None:
        encoding: dict[str, Any] = {
            "catalog": None,
            "opcode": None,
            "encodedBytes": sum(
                1 if template.startswith("dc.b") else 2 if template.startswith("dc.w") else 4
                for template in definition["emissionStatementTemplates"]
            ),
            "aliasOf": None,
            "handler": None,
            "isInlineTerminator": False,
        }
    else:
        encoding = {
            "catalog": catalog["catalog"],
            "opcode": catalog["opcode"],
            "encodedBytes": catalog["encodedBytes"],
            "aliasOf": catalog.get("aliasOf"),
            "handler": catalog.get("handler"),
            "isInlineTerminator": catalog.get("isInlineTerminator", False),
        }
    if not isinstance(encoding["encodedBytes"], int) or encoding["encodedBytes"] <= 0:
        raise ValueError(f"scripted-transition definition width drift: {definition['sourceMacro']}")
    return {
        "definitionId": definition["definitionId"],
        "family": definition["family"],
        "sourceMacro": definition["sourceMacro"],
        "sourcePath": definition["sourcePath"],
        "definitionSourceLine": definition["definitionSourceLine"],
        "formalParameterOrdinals": definition["formalParameterOrdinals"],
        "encoding": encoding,
    }


def _operation_rows(
    program: dict[str, Any],
    *,
    enum_values: dict[str, int],
    symbol_addresses: dict[str, int],
    h1_rows: dict[int, tuple[bytes, str]],
    rom: bytes,
) -> list[dict[str, Any]]:
    rows: list[dict[str, Any]] = []
    operations = program["operations"]
    previous_end = program["entryAddress"]
    for operation_index, operation in enumerate(operations):
        address = operation["address"]
        operation_end = (
            operations[operation_index + 1]["address"]
            if operation_index + 1 < len(operations)
            else program["endAddressExclusive"]
        )
        if address != previous_end or operation_end <= address:
            raise ValueError(f"scripted-transition H1 operation boundary drift: {address:#x}")
        h1_parts: list[bytes] = []
        cursor = address
        while cursor < operation_end:
            encoded, _ = h1_rows.get(cursor, (b"", ""))
            if not encoded or cursor + len(encoded) > operation_end:
                raise ValueError(
                    f"scripted-transition H1 operation byte coverage drift: {cursor:#x}"
                )
            h1_parts.append(encoded)
            cursor += len(encoded)
        encoded = b"".join(h1_parts)
        rom_encoded = rom[address : address + len(encoded)]
        if len(rom_encoded) != len(encoded):
            raise ValueError(f"scripted-transition ROM operation boundary drift: {address:#x}")
        rows.append(
            {
                "sourceOrder": operation["sourceOrder"],
                "sourceLine": operation["sourceLine"],
                "address": address,
                "sourceMacro": operation["sourceMnemonic"],
                "definitionId": operation["definitionId"],
                "family": operation["family"],
                "operands": [
                    _operand(token, enum_values=enum_values, symbol_addresses=symbol_addresses)
                    for token in operation["operandTexts"]
                ],
                "payloadContextIds": operation["payloadContextIds"],
                "h1Rom": {
                    "encodedByteLength": len(encoded),
                    "h1Sha256": _sha(encoded),
                    "romSha256": _sha(rom_encoded),
                },
            }
        )
        previous_end = address + len(encoded)
    if (
        len(rows) != 87
        or previous_end != _PROGRAM_END_EXCLUSIVE
        or rows[-1]["address"] != _TERMINAL_ADDRESS
    ):
        raise ValueError("scripted-transition operation row/span drift")
    return rows


def _project(
    map_events: dict[str, Any],
    *,
    disasm: Path,
    listing_text: str,
    rom: bytes,
) -> dict[str, Any]:
    selected, zero_count = _selected_programs(map_events)
    program = selected[0]
    if (
        program["endAddressExclusive"] != _PROGRAM_END_EXCLUSIVE
        or program["encodedSpanBytes"] != _PROGRAM_SPAN_BYTES
        or program["termination"]["address"] != _TERMINAL_ADDRESS
        or program["termination"]["sourceMnemonic"] != "csc_end"
    ):
        raise ValueError("scripted-transition program termination/span drift")
    if program["referenceCounts"] != {
        "physicalRecordCount": 1,
        "setupRecordReferenceCount": 4,
        "routeRecordReferenceCount": 4,
    } or program["operationWeightCounts"] != {
        "uniquePhysicalOperationCount": 87,
        "physicalRecordWeightedOperationCount": 87,
        "setupRecordReferenceWeightedOperationCount": 348,
        "routeRecordReferenceWeightedOperationCount": 348,
    }:
        raise ValueError("scripted-transition reference/operation weight drift")

    definitions_by_id = {row["definitionId"]: row for row in map_events["operationDefinitions"]}
    definition_ids = list(
        dict.fromkeys(operation["definitionId"] for operation in program["operations"])
    )
    if len(definition_ids) != 27 or any(
        identifier not in definitions_by_id for identifier in definition_ids
    ):
        raise ValueError("scripted-transition command definition denominator drift")
    definitions = [
        _definition_projection(definitions_by_id[identifier]) for identifier in definition_ids
    ]
    _guard_program_source(disasm, program)
    _guard_definition_sources(
        disasm, [definitions_by_id[identifier] for identifier in definition_ids]
    )
    if any(row["family"] not in _SELECTED_FAMILIES for row in definitions):
        raise ValueError("scripted-transition definition family drift")
    handler_names = [row["encoding"]["handler"] for row in definitions]
    handler_names = [name for name in handler_names if name is not None]
    if len(handler_names) != 19 or len(set(handler_names)) != 19:
        raise ValueError("scripted-transition retained handler denominator drift")

    symbol_addresses = listing_symbol_addresses(listing_text)
    enum_values = _equates(disasm)
    h1_rows = _h1_instruction_rows(listing_text)
    operation_rows = _operation_rows(
        program,
        enum_values=enum_values,
        symbol_addresses=symbol_addresses,
        h1_rows=h1_rows,
        rom=rom,
    )
    source_counts = Counter(row["sourceMacro"] for row in operation_rows)
    if dict(source_counts) != _EXPECTED_SOURCE_MACRO_COUNTS:
        raise ValueError("scripted-transition source macro count drift")
    if sum(row["family"] == "entity-action-payload-command" for row in operation_rows) != 16:
        raise ValueError("scripted-transition payload command count drift")
    if sum(row["family"] == "entity-action-command" for row in operation_rows) != 3:
        raise ValueError("scripted-transition entity-action command count drift")

    context_by_id = {row["contextId"]: row for row in map_events["operationPayloadContexts"]}
    context_ids = program["payloadContextIds"]
    if len(context_ids) != 4 or any(identifier not in context_by_id for identifier in context_ids):
        raise ValueError("scripted-transition payload context denominator drift")
    contexts = []
    for context_id in context_ids:
        context = context_by_id[context_id]
        inherited = context_id in program["inheritedPayloadContextIds"]
        contexts.append(
            {
                "contextId": context_id,
                "sourcePath": context["sourcePath"],
                "openerSourceLine": context["openerSourceLine"],
                "openerSourceMacro": context["openerSourceMnemonic"],
                "contextFamily": context["contextFamily"],
                "parentContextId": context["parentContextId"],
                "terminatorSourceMacro": context["terminatorMnemonic"],
                "terminatorSourceLine": context["terminatorSourceLine"],
                "inheritedAtProgramEntry": inherited,
            }
        )
    if sum(context["inheritedAtProgramEntry"] for context in contexts) != 1:
        raise ValueError("scripted-transition inherited payload context drift")
    if any(
        context_id not in context_by_id
        for row in operation_rows
        for context_id in row["payloadContextIds"]
    ):
        raise ValueError("scripted-transition operation payload linkage drift")

    pointer_targets = []
    for symbol in _POINTER_TARGETS:
        source_path, source_line = _find_label(disasm, symbol)
        address = symbol_addresses.get(symbol)
        if address is None:
            raise ValueError(f"scripted-transition pointer target address drift: {symbol}")
        pointer_targets.append(
            {
                "symbol": symbol,
                "address": address,
                "sourcePath": source_path,
                "sourceLine": source_line,
            }
        )
    observed_pointer_symbols = {
        operand["symbol"]
        for row in operation_rows
        for operand in row["operands"]
        if operand["kind"] == "pointer"
    }
    if observed_pointer_symbols != set(_POINTER_TARGETS):
        raise ValueError("scripted-transition pointer target use drift")

    handler_entries = []
    for name in handler_names:
        source_path, source_line = _find_label(disasm, name)
        address = symbol_addresses.get(name)
        if address is None:
            raise ValueError(f"scripted-transition handler address drift: {name}")
        handler_entries.append(
            {
                "handler": name,
                "entryAddress": address,
                "sourcePath": source_path,
                "sourceLine": source_line,
            }
        )
    if len(operation_rows) + len(handler_entries) + len(pointer_targets) != 111:
        raise ValueError("scripted-transition address anchor denominator drift")
    handler_by_name = {row["handler"]: row for row in handler_entries}
    retained_handlers = {
        "setCameraDestination": handler_by_name["csc32_setCameraDestInTiles"],
        "dispatch": {
            "handler": "rjt_cutsceneScriptCommands",
            "entryAddress": symbol_addresses["rjt_cutsceneScriptCommands"],
            "sourcePath": _SOURCE_PATHS[6],
            "sourceLine": _find_label(disasm, "rjt_cutsceneScriptCommands")[1],
        },
        "setFacing": handler_by_name["csc23_setEntityFacing"],
        "setBlocks": handler_by_name["csc34_setBlocks"],
        "nextSingleText": handler_by_name["csc00_displaySingleTextbox"],
        "setActscript": handler_by_name["csc15_setEntityActscript"],
        "entityActions": handler_by_name["csc2D_entityActionSequence"],
        "playSound": handler_by_name["csc05_playSound"],
        "executeSubroutine": handler_by_name["csc0A_executeSubroutine"],
        "warp": handler_by_name["csc07_warp"],
        "loadMapFadeIn": handler_by_name["csc37_loadMapAndFadeIn"],
        "loadMapEntities": handler_by_name["csc42_loadMapEntities"],
        "setSprite": handler_by_name["csc1A_setEntitySprite"],
        "fadeIn": handler_by_name["csc39_fadeInFromBlack"],
        "customActscript": handler_by_name["csc14_setEntityActscriptManual"],
        "entityActionDispatcher": {
            "handler": "rjt_EntityScriptCommands",
            "entryAddress": symbol_addresses["rjt_EntityScriptCommands"],
            "sourcePath": _SOURCE_PATHS[7],
            "sourceLine": _find_label(disasm, "rjt_EntityScriptCommands")[1],
        },
        "setQuake": handler_by_name["csc33_setQuakeAmount"],
        "flashWhite": handler_by_name["csc41_flashScreenWhite"],
        "clearFlag": handler_by_name["csc10_toggleFlag"],
    }
    if len(retained_handlers) != 19:
        raise ValueError("scripted-transition retained handler projection drift")

    return {
        "schemaVersion": 1,
        "id": ID,
        "upstreamCommit": _UPSTREAM_COMMIT,
        "romSha256": _ROM_SHA256,
        "sourceIdentity": _source_identity(disasm),
        "scriptedTransitionState": {
            "sourceProgram": {
                "canonicalSymbol": program["canonicalSymbol"],
                "sourcePath": program["sourcePath"],
                "entrySourceLine": program["entrySourceLine"],
                "entryAddress": program["entryAddress"],
                "enclosingScriptEntryAddress": 345464,
                "endAddressExclusive": program["endAddressExclusive"],
                "terminalAddress": program["termination"]["address"],
                "encodedSpanBytes": program["encodedSpanBytes"],
                "referenceCounts": program["referenceCounts"],
                "operationWeightCounts": program["operationWeightCounts"],
            },
            "selectionSummary": {
                "motherProgramCount": 914,
                "positiveProgramCount": 1,
                "zeroProgramCount": zero_count,
                "sourceOperationCount": len(operation_rows),
                "commandDefinitionCount": len(definitions),
                "payloadContextCount": len(contexts),
                "inheritedPayloadContextCount": 1,
                "retainedHandlerCount": len(retained_handlers),
                "pointerTargetCount": len(pointer_targets),
                "addressAnchorCount": 111,
            },
            "sourceMacroCounts": [
                {"sourceMacro": name, "count": count}
                for name, count in _EXPECTED_SOURCE_MACRO_COUNTS.items()
            ],
            "operationRows": operation_rows,
            "commandDefinitions": definitions,
            "payloadContexts": contexts,
            "pointerTargets": pointer_targets,
            "retainedHandlers": retained_handlers,
        },
        "unknowns": {key: "Unknown" for key in _UNKNOWN_KEYS},
        "unknownOrder": list(_UNKNOWN_KEYS),
    }


def build_map_event_scripted_transition_state_contract(
    rom_path: Path, upstream_path: Path
) -> dict[str, Any]:
    """Build the closed source/H1/ROM content-semantic projection."""
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    rom = rom_path.read_bytes()
    if _sha(rom) != _ROM_SHA256:
        raise ValueError("scripted-transition ROM identity drift")
    disasm = _root(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"scripted-transition H1 listing is missing: {listing_path}")
    map_events = build_map_events_contract(rom_path, upstream_path)
    if (
        map_events["upstream"]["commit"] != _UPSTREAM_COMMIT
        or map_events["romSha256"] != _ROM_SHA256
    ):
        raise ValueError("scripted-transition retained map-events provenance drift")
    return _project(
        map_events,
        disasm=disasm,
        listing_text=listing_path.read_text(encoding="utf-8"),
        rom=rom,
    )


def verify_map_event_scripted_transition_state_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, str]:
    """Verify this exact static projection against its public golden fixture."""
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner=str(FIXTURE))
    _validate_order(fixture)
    output = build_map_event_scripted_transition_state_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="map-event scripted-transition static contract")
    _validate_order(output)
    if output != fixture:
        raise ValueError("map-event scripted-transition complete semantic fixture drift")
    if output_path is None:
        return {"Contract": ID, "SHA256": _sha(canonical_json_bytes(output))}
    destination = output_path.resolve()
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(json.dumps(output, indent=2).encode("utf-8") + b"\n")
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": _sha(canonical_json_bytes(output)),
    }


def _remove_map_event_scripted_transition_state_later_owner_index_delta(
    index: dict[str, Any],
) -> dict[str, Any]:
    """Remove only this exact index delta before earlier-owner projections run."""
    normalized = deepcopy(index)
    records = normalized.get("records")
    if not isinstance(records, list) or len({row.get("id") for row in records}) != len(records):
        raise ValueError("scripted-transition later-owner record shape drift")
    bindings = {
        "map.data.cs-54578": [
            ("entry", "scriptedTransitionState.sourceProgram.enclosingScriptEntryAddress"),
            ("map21-default-zone-event", "scriptedTransitionState.sourceProgram.entryAddress"),
        ],
        "scripting.map.mapscriptengine-1": [
            ("entry", "scriptedTransitionState.retainedHandlers.setCameraDestination.entryAddress")
        ],
        "scripting.map.mapscriptengine-2": [
            ("jump-table", "scriptedTransitionState.retainedHandlers.dispatch.entryAddress")
        ],
        "map.entity-placement.set-facing": [
            ("entry", "scriptedTransitionState.retainedHandlers.setFacing.entryAddress")
        ],
        "map.block-mutation.set-blocks-handler": [
            ("entry", "scriptedTransitionState.retainedHandlers.setBlocks.entryAddress")
        ],
        "map.script-dialogue.next-single-text": [
            ("entry", "scriptedTransitionState.retainedHandlers.nextSingleText.entryAddress")
        ],
        "map.entity-action-bridge.set-actscript": [
            ("entry", "scriptedTransitionState.retainedHandlers.setActscript.entryAddress")
        ],
        "map.entity-action-bridge.entity-actions": [
            ("entry", "scriptedTransitionState.retainedHandlers.entityActions.entryAddress")
        ],
        "map.script-control-audio.runtime": [
            ("csc05-entry", "scriptedTransitionState.retainedHandlers.playSound.entryAddress"),
            (
                "csc0a-entry",
                "scriptedTransitionState.retainedHandlers.executeSubroutine.entryAddress",
            ),
        ],
        "scripting.map.transition-runtime-boundary": [
            ("fade-handler", "scriptedTransitionState.retainedHandlers.loadMapFadeIn.entryAddress"),
            ("warp-handler", "scriptedTransitionState.retainedHandlers.warp.entryAddress"),
        ],
        "map.entity-population.load-map-entities": [
            ("entry", "scriptedTransitionState.retainedHandlers.loadMapEntities.entryAddress")
        ],
        "map.entity-lifecycle-presentation.set-sprite": [
            ("entry", "scriptedTransitionState.retainedHandlers.setSprite.entryAddress")
        ],
        "map.script-screen-presentation.fade-in": [
            ("entry", "scriptedTransitionState.retainedHandlers.fadeIn.entryAddress")
        ],
        "map.entity-action-bridge.custom-actscript": [
            ("entry", "scriptedTransitionState.retainedHandlers.customActscript.entryAddress")
        ],
        "scripting.entity.dispatch-table": [
            (
                "entry",
                "scriptedTransitionState.retainedHandlers.entityActionDispatcher.entryAddress",
            )
        ],
        "map.script-screen-presentation.set-quake": [
            ("entry", "scriptedTransitionState.retainedHandlers.setQuake.entryAddress")
        ],
        "map.script-screen-presentation.flash-white": [
            ("entry", "scriptedTransitionState.retainedHandlers.flashWhite.entryAddress")
        ],
    }
    if len(bindings) != 17 or sum(len(rows) for rows in bindings.values()) != 20:
        raise ValueError("scripted-transition index delta denominator drift")
    document = "docs/research/map-event-scripted-transition-state.md"
    expected_address = {
        "id": "map21-default-zone-event",
        "space": "rom",
        "kind": "observation",
        "value": _PROGRAM_ENTRY,
    }
    seen: set[str] = set()
    for record in records:
        record_id = record.get("id")
        expected_bindings = bindings.get(record_id)
        if expected_bindings is None:
            continue
        expected_evidence = {
            "level": "H2",
            "fixture": "tests/fixtures/h2/map-event-scripted-transition-state-static-v1.json",
            "fixtureId": ID,
            "verifier": "src/sf2tool/h2/map_event_scripted_transition_state.py",
            "bindings": [
                {"addressId": address_id, "fixtureField": fixture_field}
                for address_id, fixture_field in expected_bindings
            ],
        }
        evidence = record.get("evidence")
        documents = record.get("documents")
        addresses = record.get("addresses")
        matches = (
            [item for item in evidence if item.get("fixtureId") == ID]
            if isinstance(evidence, list)
            else []
        )
        if (
            matches != [expected_evidence]
            or not isinstance(documents, list)
            or documents.count(document) != 1
            or documents[-1] != document
            or not isinstance(addresses, list)
        ):
            raise ValueError("scripted-transition later-owner record fields drift")
        if record_id == "map.data.cs-54578":
            if addresses.count(expected_address) != 1 or addresses[-1] != expected_address:
                raise ValueError("scripted-transition index address delta drift")
            addresses.remove(expected_address)
        evidence.remove(expected_evidence)
        documents.remove(document)
        seen.add(record_id)
    if seen != set(bindings):
        raise ValueError("scripted-transition later-owner coverage drift")
    if _sha(canonical_json_bytes(normalized)) != _PREDECESSOR_INDEX_SHA256:
        raise ValueError("scripted-transition predecessor index drift")
    return normalized


def normalize_map_event_scripted_transition_state_later_owner_index(
    index: dict[str, Any],
) -> dict[str, Any]:
    """Strictly normalize the current index through this owner's predecessor."""
    from sf2tool.research_index import normalize_current_index_to_owner_predecessor

    return normalize_current_index_to_owner_predecessor(
        index, owner_id=ID
    )
