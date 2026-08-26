"""Public H2 request-state flow for selected map-event programs.

This owner starts with the six direct fixed-RAM request-state write classes in
the accepted map-event corpus.  It derives caller-local CFGs and source-defined
may/must reaching definitions through only ``ShopMenu`` transfers and program
returns.  Callees are deliberately not entered: this is not evidence that a
request is consumed, that a menu is shown, or that a map transition occurs.
"""

from __future__ import annotations

import hashlib
from collections import Counter, deque
from pathlib import Path
from typing import Any

from sf2tool.h2.map_event_dialogue_state import (
    FIXTURE as DIALOGUE_FIXTURE,
)
from sf2tool.h2.map_event_dialogue_state import (
    ID as DIALOGUE_ID,
)
from sf2tool.h2.map_event_dialogue_state import (
    _assert_source_label,
    _disasm_root,
    _fixture_sha256,
    _fresh_retained_owners,
    _h1_symbol_addresses,
    _operation_shape,
    _operation_statement,
    _physical_anchor,
    _program_edges,
)
from sf2tool.h2.map_event_direct_control import _assert_source_statement
from sf2tool.h2.map_event_direct_handoff import _h1_instruction_rows
from sf2tool.h2.map_event_direct_state import _parse_equates, canonical_json_bytes
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path

ID = "sf2-map-event-request-state-static-v1"
FIXTURE = repo_path("tests/fixtures/h2/map-event-request-state-static-v1.json")
SCHEMA = repo_path("schemas/h2/map-event-request-state-static-fixture.schema.json")
MAP_EVENTS_MANIFEST = repo_path("manifests/extractions/map-events-static.json")

_CATEGORIES = ("entityEvents", "zoneEvents", "itemEvents")
_PROGRAM_FIELDS = {
    "entityEvents": "entityTargetPrograms",
    "zoneEvents": "zoneTargetPrograms",
    "itemEvents": "itemTargetPrograms",
}
_SYMBOLS = (
    "CURRENT_SHOP_INDEX",
    "MAP_EVENT_TYPE",
    "EGRESS_MAP",
    "RAFT_MAP",
    "RAFT_X",
    "RAFT_Y",
)
_SUPPORT_SOURCE_PATHS = ("sf2const.asm", "sf2enums.asm", "sf2macros.asm")
_UNKNOWN_KEYS = (
    "normalStoryProgramReachability",
    "selectedControlFlowPath",
    "callerEntryState",
    "actualRequestWriteOrder",
    "actualDefinitionAtHandoff",
    "actualShopSelection",
    "actualShopMenuEntryAndOutcome",
    "actualEgressDestination",
    "actualRaftDestinationAndCoordinates",
    "actualMapEventReloadRequestConsumption",
    "actualProgramReturnState",
    "crossMapStateLifetime",
    "saveLoadPersistence",
    "inputUiMapTransitionAudioTimingAndStoryMeaning",
)


def _source_table_rows(map_events: dict[str, Any]) -> dict[tuple[str, str], dict[str, Any]]:
    rows: dict[tuple[str, str], dict[str, Any]] = {}
    for category in _CATEGORIES:
        for row in map_events["categories"][category]["sourceFiles"]:
            key = category, row["path"]
            if key in rows:
                raise ValueError("map-event request-state duplicate source table path")
            rows[key] = row
    return rows


def _assert_source_table_symbol(lines: list[str], *, table_symbol: str, context: str) -> None:
    """Guard the parsed table/program owner against its lexical source label."""
    matches = [
        line_number
        for line_number, line in enumerate(lines, start=1)
        if line.split(";", maxsplit=1)[0].strip() == f"{table_symbol}:"
    ]
    if len(matches) != 1:
        raise ValueError(
            "map-event request-state table source owner drift: "
            f"{context} expected={table_symbol!r} matches={matches}"
        )


def _selected_write_rows(direct_state: dict[str, Any]) -> list[dict[str, Any]]:
    rows = [
        row
        for row in direct_state["accessSites"]
        if row["accessKind"] == "write" and row["symbol"] in _SYMBOLS
    ]
    expected_counts = {
        "CURRENT_SHOP_INDEX": 32,
        "MAP_EVENT_TYPE": 8,
        "EGRESS_MAP": 2,
        "RAFT_MAP": 1,
        "RAFT_X": 1,
        "RAFT_Y": 1,
    }
    if len(rows) != 45 or Counter(row["symbol"] for row in rows) != expected_counts:
        raise ValueError("map-event request-state write-source denominator drift")
    return rows


def _selected_programs(
    map_events: dict[str, Any], writes: list[dict[str, Any]]
) -> list[tuple[str, dict[str, Any]]]:
    selected_keys = {
        (row["category"], row["programSymbol"], row["programEntryAddress"]) for row in writes
    }
    expected_categories = {"entityEvents": 30, "zoneEvents": 9}
    actual_categories = Counter(category for category, _symbol, _address in selected_keys)
    if len(selected_keys) != 39 or actual_categories != expected_categories:
        raise ValueError("map-event request-state selected-program denominator drift")
    selected: list[tuple[str, dict[str, Any]]] = []
    for category in _CATEGORIES:
        for program in map_events[_PROGRAM_FIELDS[category]]:
            key = category, program["canonicalSymbol"], program["entryAddress"]
            if key in selected_keys:
                selected.append((category, program))
    if len(selected) != len(selected_keys):
        raise ValueError("map-event request-state selected-program owner join drift")
    return selected


def _canonical_source_file_rows(rows: list[dict[str, Any]]) -> list[dict[str, Any]]:
    canonical = sorted(
        rows,
        key=lambda row: (
            row["sourcePath"],
            row["category"],
            row["tableSymbol"],
            row["tableEntryAddress"],
        ),
    )
    if len(canonical) != 24 or len({row["tableSymbol"] for row in canonical}) != 24:
        raise ValueError("map-event request-state table-source denominator drift")
    return canonical


def _definition_id(row: dict[str, Any]) -> str:
    return f"write:{row['romPc']:06X}:{row['symbol']}"


def _state_rows(may: dict[str, set[str]], must: dict[str, set[str]]) -> list[dict[str, Any]]:
    return [
        {
            "symbol": symbol,
            "mayDefinitionIds": sorted(may[symbol]),
            "mustDefinitionIds": sorted(must[symbol]),
        }
        for symbol in _SYMBOLS
    ]


def _reaching_definitions(
    program: dict[str, Any],
    definitions_by_order: dict[int, dict[str, str]],
    successors: dict[int, list[int]],
) -> tuple[dict[int, dict[str, set[str]]], dict[int, dict[str, set[str]]]]:
    """Compute source-defined may/must writes before each selected operation.

    There is intentionally no synthetic incoming definition.  The static result
    distinguishes a source-defined write on a path from an unobserved caller
    entry value, without assigning either a runtime value or lifetime.
    """
    count = len(program["operations"])
    predecessors: dict[int, list[int]] = {index: [] for index in range(count)}
    for source, targets in successors.items():
        for target in targets:
            predecessors[target].append(source)
    empty = {symbol: set() for symbol in _SYMBOLS}
    in_may: dict[int, dict[str, set[str]]] = {}
    in_must: dict[int, dict[str, set[str]]] = {}
    out_may: dict[int, dict[str, set[str]]] = {}
    out_must: dict[int, dict[str, set[str]]] = {}
    pending: deque[int] = deque([0])
    queued = {0}
    while pending:
        order = pending.popleft()
        queued.remove(order)
        if order == 0:
            may_inputs = [empty]
            must_inputs = [empty]
        else:
            may_inputs = [out_may[item] for item in predecessors[order] if item in out_may]
            must_inputs = [out_must[item] for item in predecessors[order] if item in out_must]
        if not may_inputs or not must_inputs:
            continue
        next_may = {
            symbol: set().union(*(state[symbol] for state in may_inputs)) for symbol in _SYMBOLS
        }
        next_must = {
            symbol: set.intersection(*(state[symbol] for state in must_inputs))
            for symbol in _SYMBOLS
        }
        next_out_may = {symbol: set(values) for symbol, values in next_may.items()}
        next_out_must = {symbol: set(values) for symbol, values in next_must.items()}
        for symbol, definition_id in definitions_by_order[order].items():
            next_out_may[symbol] = {definition_id}
            next_out_must[symbol] = {definition_id}
        changed = (
            in_may.get(order) != next_may
            or in_must.get(order) != next_must
            or out_may.get(order) != next_out_may
            or out_must.get(order) != next_out_must
        )
        in_may[order] = next_may
        in_must[order] = next_must
        out_may[order] = next_out_may
        out_must[order] = next_out_must
        if changed:
            for target in successors[order]:
                if target not in queued:
                    pending.append(target)
                    queued.add(target)
    if len(in_may) != count:
        missing = sorted(set(range(count)) - set(in_may))
        raise ValueError(
            "map-event request-state selected CFG has an unreachable operation: "
            f"{program['canonicalSymbol']} missing={missing}"
        )
    return in_may, in_must


def _definition_rows_for_program(
    category: str,
    program: dict[str, Any],
    writes_by_operation: dict[tuple[str, str, int, int], list[dict[str, Any]]],
) -> dict[int, dict[str, str]]:
    definitions: dict[int, dict[str, str]] = {}
    for operation in program["operations"]:
        key = category, program["canonicalSymbol"], program["entryAddress"], operation["address"]
        rows = writes_by_operation.get(key, [])
        values = {row["symbol"]: _definition_id(row) for row in rows}
        if len(values) != len(rows):
            raise ValueError("map-event request-state same-operation write identity drift")
        definitions[operation["sourceOrder"]] = values
    return definitions


def _write_site_row(
    *,
    site_order: int,
    row: dict[str, Any],
    program_operations: dict[tuple[str, str, int], dict[int, dict[str, Any]]],
) -> dict[str, Any]:
    program_key = row["category"], row["programSymbol"], row["programEntryAddress"]
    operation = program_operations.get(program_key, {}).get(row["romPc"])
    if operation is None or operation["sourceLine"] != row["sourceLine"]:
        raise ValueError("map-event request-state write operation owner drift")
    if operation["mnemonic"] != row["mnemonic"] or operation["sizeSuffix"] != f".{row['width']}":
        raise ValueError("map-event request-state write opcode/width drift")
    if list(operation["operandTexts"]) != row["operandTexts"]:
        raise ValueError("map-event request-state write operand-order drift")
    return {
        "siteOrder": site_order,
        "id": _definition_id(row),
        "category": row["category"],
        "programSymbol": row["programSymbol"],
        "programEntryAddress": row["programEntryAddress"],
        "tableSymbol": row["tableSymbol"],
        "sourcePath": row["sourcePath"],
        "sourceOrder": operation["sourceOrder"],
        "sourceLine": row["sourceLine"],
        "romPc": row["romPc"],
        "mnemonic": row["mnemonic"],
        "width": row["width"],
        "operandTexts": row["operandTexts"],
        "accessOperandIndex": row["accessOperandIndex"],
        "symbol": row["symbol"],
        "address": row["address"],
        "valueKind": row["valueKind"],
        "valueToken": row["valueToken"],
        "resolvedValue": row["resolvedValue"],
    }


def _retained_owner_projection(
    retained: dict[str, Any], dialogue: dict[str, Any]
) -> dict[str, dict[str, str]]:
    projections = dict(retained["projections"])
    projections["dialogueState"] = {
        "fixtureId": DIALOGUE_ID,
        "fixtureSha256": _fixture_sha256(DIALOGUE_FIXTURE),
        "outputSha256": hashlib.sha256(canonical_json_bytes(dialogue)).hexdigest().upper(),
    }
    if set(projections) != {
        "mapEvents",
        "directState",
        "directControl",
        "directHandoff",
        "predicateResults",
        "dialogueState",
    }:
        raise ValueError("map-event request-state retained-owner identity drift")
    return projections


def _fresh_retained_request_state_owners(
    rom_path: Path, upstream_path: Path
) -> tuple[dict[str, Any], dict[str, Any], dict[str, Any]]:
    """Fresh-guard every accepted map-event owner before narrowing it.

    The dialogue owner's retained build performs source/H1/ROM reconstruction of
    the map-events, direct-state, direct-control, direct-handoff, and predicate
    owners.  Reconstructing its own projection before use makes the final sixth
    retained owner equally source-first rather than fixture-derived.
    """
    retained = _fresh_retained_owners(rom_path, upstream_path)
    map_events_manifest = load_json(MAP_EVENTS_MANIFEST)
    if (
        retained["projections"]["mapEvents"]["outputSha256"] != map_events_manifest["outputSha256"]
        or retained["mapEvents"]["summary"] != map_events_manifest["summary"]
    ):
        raise ValueError("map-event request-state retained map-events manifest drift")
    from sf2tool.h2.map_event_dialogue_state import _projection as dialogue_projection

    dialogue_state, dialogue_summary, dialogue_context = dialogue_projection(
        retained, upstream_path=upstream_path, rom_path=rom_path
    )
    dialogue = {
        "schemaVersion": 1,
        "id": DIALOGUE_ID,
        "upstream": {
            "repository": "ShiningForceCentral/SF2DISASM",
            "commit": "c834c652b6862bc5679fd7f69a38a7093206efc6",
        },
        "romSha256": "9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9",
        "sourceContext": dialogue_context,
        "retainedOwners": retained["projections"],
        "eventDialogueState": dialogue_state,
        "unknowns": {key: "Unknown" for key in load_json(DIALOGUE_FIXTURE)["unknowns"]},
        "summary": dialogue_summary,
    }
    if dialogue != load_json(DIALOGUE_FIXTURE):
        raise ValueError("map-event request-state retained dialogue-state projection drift")
    return retained, dialogue, _retained_owner_projection(retained, dialogue)


def _projection(
    retained: dict[str, Any], *, upstream_path: Path, rom_path: Path
) -> tuple[dict[str, Any], dict[str, Any], dict[str, Any]]:
    map_events = retained["mapEvents"]
    writes = _selected_write_rows(retained["eventDirectState"])
    selected = _selected_programs(map_events, writes)
    disasm = _disasm_root(upstream_path)
    listing_text = (upstream_path.resolve(strict=True) / "build/sf2build-h1.lst").read_text(
        encoding="utf-8"
    )
    h1_rows = _h1_instruction_rows(listing_text)
    h1_symbols = _h1_symbol_addresses(listing_text)
    rom = rom_path.resolve(strict=True).read_bytes()
    constants = _parse_equates(
        (disasm / "sf2const.asm").read_text(encoding="utf-8"), source_path="sf2const.asm"
    )
    enums = _parse_equates(
        (disasm / "sf2enums.asm").read_text(encoding="utf-8"), source_path="sf2enums.asm"
    )
    if set(_SYMBOLS) - set(constants):
        raise ValueError("map-event request-state fixed-RAM symbol definition drift")
    operation_definitions = {row["definitionId"]: row for row in map_events["operationDefinitions"]}
    if len(operation_definitions) != len(map_events["operationDefinitions"]):
        raise ValueError("map-event request-state operation-definition identity drift")

    writes_by_operation: dict[tuple[str, str, int, int], list[dict[str, Any]]] = {}
    for row in writes:
        key = row["category"], row["programSymbol"], row["programEntryAddress"], row["romPc"]
        writes_by_operation.setdefault(key, []).append(row)

    table_rows = _source_table_rows(map_events)
    source_text: dict[str, list[str]] = {}
    physical_anchors: dict[int, dict[str, Any]] = {}
    program_operations: dict[tuple[str, str, int], dict[int, dict[str, Any]]] = {}
    flows: list[dict[str, Any]] = []
    handoffs: list[dict[str, Any]] = []
    source_files: list[dict[str, Any]] = []
    seen_tables: set[tuple[str, str, int]] = set()
    for category, program in selected:
        table = table_rows.get((category, program["sourcePath"]))
        if table is None:
            raise ValueError("map-event request-state table owner missing")
        table_key = category, table["symbol"], table["address"]
        lines = source_text.setdefault(
            program["sourcePath"],
            (disasm / program["sourcePath"]).read_text(encoding="utf-8").splitlines(),
        )
        if table_key not in seen_tables:
            _assert_source_table_symbol(
                lines,
                table_symbol=table["symbol"],
                context=f"{category}:{program['sourcePath']}",
            )
            seen_tables.add(table_key)
            source_files.append(
                {
                    "category": category,
                    "tableSymbol": table["symbol"],
                    "tableEntryAddress": table["address"],
                    "sourcePath": table["path"],
                }
            )
        key = category, program["canonicalSymbol"], program["entryAddress"]
        program_operations[key] = {row["address"]: row for row in program["operations"]}
        definition_rows = _definition_rows_for_program(category, program, writes_by_operation)
        operation_shapes: list[dict[str, Any]] = []
        for index, operation in enumerate(program["operations"]):
            context = f"{program['canonicalSymbol']}:{operation['sourceLine']}"
            _assert_source_statement(
                lines,
                source_line=operation["sourceLine"],
                expected=_operation_statement(operation),
                context=context,
            )
            next_address = (
                program["operations"][index + 1]["address"]
                if index + 1 < len(program["operations"])
                else None
            )
            physical = _physical_anchor(
                operation=operation,
                next_address=next_address,
                operation_definitions=operation_definitions,
                h1_rows=h1_rows,
                h1_symbols=h1_symbols,
                rom=rom,
                context=context,
            )
            anchor = {
                "id": f"operation:{operation['address']:06X}",
                "kind": "target-program-operation",
                "romPc": operation["address"],
                "instructionByteLength": physical["instructionByteLength"],
                "h1InstructionSha256": physical["h1InstructionSha256"],
                "romInstructionSha256": physical["romInstructionSha256"],
            }
            previous = physical_anchors.setdefault(operation["address"], anchor)
            if previous != anchor:
                raise ValueError("map-event request-state physical operation anchor drift")
            operation_shapes.append(
                {
                    **_operation_shape(operation),
                    "operandTexts": operation["operandTexts"],
                    "definitionIds": [
                        definition_rows[operation["sourceOrder"]][symbol]
                        for symbol in _SYMBOLS
                        if symbol in definition_rows[operation["sourceOrder"]]
                    ],
                }
            )
        for label in program["labels"]:
            _assert_source_label(
                lines, label, context=f"{program['canonicalSymbol']}:{label['sourceLine']}"
            )
        edges, successors = _program_edges(program)
        in_may, in_must = _reaching_definitions(program, definition_rows, successors)
        flows.append(
            {
                "category": category,
                "programSymbol": program["canonicalSymbol"],
                "programEntryAddress": program["entryAddress"],
                "tableSymbol": table["symbol"],
                "sourcePath": program["sourcePath"],
                "labels": [
                    {
                        "sourceOrder": label["sourceOrder"],
                        "symbol": label["symbol"],
                        "romPc": label["address"],
                    }
                    for label in program["labels"]
                ],
                "operations": operation_shapes,
                "edges": edges,
            }
        )
        for operation in program["operations"]:
            order = operation["sourceOrder"]
            target = operation["target"]
            is_shop_transfer = target is not None and target["effectiveTargetSymbol"] == "ShopMenu"
            is_return = operation["controlFlowKind"] == "return"
            if not (is_shop_transfer or is_return):
                continue
            if is_shop_transfer:
                if operation["controlFlowKind"] not in {"direct-call", "direct-jump"}:
                    raise ValueError("map-event request-state ShopMenu transfer kind drift")
                if target["instructionTargetSymbol"] != "j_ShopMenu":
                    raise ValueError("map-event request-state ShopMenu alias identity drift")
                kind = "shop-menu-transfer"
                transfer_kind = operation["controlFlowKind"]
                instruction_target_symbol = target["instructionTargetSymbol"]
                instruction_target_address = target["instructionTargetAddress"]
                effective_target_symbol = target["effectiveTargetSymbol"]
                effective_target_address = target["effectiveTargetAddress"]
            else:
                kind = "program-return"
                transfer_kind = "return"
                instruction_target_symbol = None
                instruction_target_address = None
                effective_target_symbol = None
                effective_target_address = None
            handoffs.append(
                {
                    "siteOrder": len(handoffs),
                    "kind": kind,
                    "transferKind": transfer_kind,
                    "category": category,
                    "programSymbol": program["canonicalSymbol"],
                    "programEntryAddress": program["entryAddress"],
                    "tableSymbol": table["symbol"],
                    "sourcePath": program["sourcePath"],
                    "sourceOrder": order,
                    "sourceLine": operation["sourceLine"],
                    "romPc": operation["address"],
                    "instructionTargetSymbol": instruction_target_symbol,
                    "instructionTargetAddress": instruction_target_address,
                    "effectiveTargetSymbol": effective_target_symbol,
                    "effectiveTargetAddress": effective_target_address,
                    "state": _state_rows(in_may[order], in_must[order]),
                }
            )

    source_files = _canonical_source_file_rows(source_files)
    write_sites = [
        _write_site_row(site_order=index, row=row, program_operations=program_operations)
        for index, row in enumerate(writes)
    ]
    if len(physical_anchors) != 262:
        raise ValueError("map-event request-state physical-operation denominator drift")
    if sum(len(row["operations"]) for row in flows) != 262:
        raise ValueError("map-event request-state contextual-operation denominator drift")
    if (
        sum(len(row["labels"]) for row in flows) != 82
        or len({label["romPc"] for row in flows for label in row["labels"]}) != 82
    ):
        raise ValueError("map-event request-state label denominator drift")

    symbol_definitions = [
        {
            "symbol": symbol,
            "address": constants[symbol]["value"],
            "sourcePath": "sf2const.asm",
            "sourceLine": constants[symbol]["sourceLine"],
        }
        for symbol in _SYMBOLS
    ]
    source_operands = {
        (row["valueKind"], row["valueToken"], row["resolvedValue"]) for row in write_sites
    }
    enum_operands = {
        row["valueToken"] for row in write_sites if row["valueKind"] == "immediate-enum"
    }
    numeric_values = {
        row["resolvedValue"] for row in write_sites if row["valueKind"] == "immediate-number"
    }
    if len(source_operands) != 37 or len(enum_operands) != 35 or numeric_values != {43, 48}:
        raise ValueError("map-event request-state source-operand denominator drift")
    for row in write_sites:
        if row["valueKind"] == "immediate-enum":
            enum = enums.get((row["valueToken"] or "")[1:])
            if enum is None or enum["value"] != row["resolvedValue"]:
                raise ValueError("map-event request-state enum operand resolution drift")

    control_counts = {
        kind: sum(
            operation["controlFlowKind"] == kind
            for flow in flows
            for operation in flow["operations"]
        )
        for kind in (
            "ordinary",
            "conditional-branch",
            "unconditional-branch",
            "direct-call",
            "direct-jump",
            "return",
        )
    }
    shop_handoffs = [row for row in handoffs if row["kind"] == "shop-menu-transfer"]
    return_handoffs = [row for row in handoffs if row["kind"] == "program-return"]
    # A handoff relation is one request-state symbol with at least one
    # source-defined reaching write.  May/must definition-cardinality stays in
    # the per-state record; a branch merge therefore remains one symbol/handoff
    # relation rather than inflating this compact denominator.
    handoff_relations = sum(
        bool(state["mayDefinitionIds"]) for row in handoffs for state in row["state"]
    )
    summary = {
        "motherProgramContextCount": 914,
        "motherOperationCount": 3579,
        "positiveProgramContextCount": len(flows),
        "zeroProgramContextCount": 914 - len(flows),
        "entityProgramContextCount": sum(row["category"] == "entityEvents" for row in flows),
        "zoneProgramContextCount": sum(row["category"] == "zoneEvents" for row in flows),
        "sourceFileCount": len(source_files),
        "sourceIdentityCount": len(source_files) + len(_SUPPORT_SOURCE_PATHS),
        "symbolDefinitionCount": len(symbol_definitions),
        "contextOperationCount": sum(len(row["operations"]) for row in flows),
        "physicalOperationCount": len(physical_anchors),
        "contextLabelCount": sum(len(row["labels"]) for row in flows),
        "physicalLabelCount": len({label["romPc"] for row in flows for label in row["labels"]}),
        "ordinaryOperationCount": control_counts["ordinary"],
        "conditionalBranchCount": control_counts["conditional-branch"],
        "unconditionalBranchCount": control_counts["unconditional-branch"],
        "directCallCount": control_counts["direct-call"],
        "directJumpCount": control_counts["direct-jump"],
        "returnCount": control_counts["return"],
        "writeDefinitionSiteCount": len(write_sites),
        "currentShopIndexWriteCount": sum(
            row["symbol"] == "CURRENT_SHOP_INDEX" for row in write_sites
        ),
        "mapEventTypeWriteCount": sum(row["symbol"] == "MAP_EVENT_TYPE" for row in write_sites),
        "egressMapWriteCount": sum(row["symbol"] == "EGRESS_MAP" for row in write_sites),
        "raftMapWriteCount": sum(row["symbol"] == "RAFT_MAP" for row in write_sites),
        "raftXWriteCount": sum(row["symbol"] == "RAFT_X" for row in write_sites),
        "raftYWriteCount": sum(row["symbol"] == "RAFT_Y" for row in write_sites),
        "uniqueSourceOperandCount": len(source_operands),
        "enumSourceOperandCount": len(enum_operands),
        "numericSourceOperandCount": len(numeric_values),
        "shopMenuTransferSiteCount": len(shop_handoffs),
        "shopMenuReturningCallCount": sum(
            row["transferKind"] == "direct-call" for row in shop_handoffs
        ),
        "shopMenuTailJumpCount": sum(row["transferKind"] == "direct-jump" for row in shop_handoffs),
        "returnStateSiteCount": len(return_handoffs),
        "handoffStateSiteCount": len(handoffs),
        "handoffStateRelationCount": handoff_relations,
        "h1RomAnchorCount": len(physical_anchors),
    }
    expected_summary = {
        "motherProgramContextCount": 914,
        "motherOperationCount": 3579,
        "positiveProgramContextCount": 39,
        "zeroProgramContextCount": 875,
        "entityProgramContextCount": 30,
        "zoneProgramContextCount": 9,
        "sourceFileCount": 24,
        "sourceIdentityCount": 27,
        "symbolDefinitionCount": 6,
        "contextOperationCount": 262,
        "physicalOperationCount": 262,
        "contextLabelCount": 82,
        "physicalLabelCount": 82,
        "ordinaryOperationCount": 139,
        "conditionalBranchCount": 34,
        "unconditionalBranchCount": 21,
        "directCallCount": 29,
        "directJumpCount": 3,
        "returnCount": 36,
        "writeDefinitionSiteCount": 45,
        "currentShopIndexWriteCount": 32,
        "mapEventTypeWriteCount": 8,
        "egressMapWriteCount": 2,
        "raftMapWriteCount": 1,
        "raftXWriteCount": 1,
        "raftYWriteCount": 1,
        "uniqueSourceOperandCount": 37,
        "enumSourceOperandCount": 35,
        "numericSourceOperandCount": 2,
        "shopMenuTransferSiteCount": 31,
        "shopMenuReturningCallCount": 28,
        "shopMenuTailJumpCount": 3,
        "returnStateSiteCount": 36,
        "handoffStateSiteCount": 67,
        "handoffStateRelationCount": 69,
        "h1RomAnchorCount": 262,
    }
    if summary != expected_summary:
        mismatch = {
            key: {"expected": expected_summary[key], "actual": summary.get(key)}
            for key in expected_summary
            if summary.get(key) != expected_summary[key]
        }
        raise ValueError(f"map-event request-state denominator drift: {mismatch}")

    event_request_state = {
        "symbolDefinitions": symbol_definitions,
        "symbolDefinitionOrder": [
            f"{row['symbol']}:{row['address']}" for row in symbol_definitions
        ],
        "programFlows": flows,
        "programFlowOrder": [
            f"{row['category']}|{row['programSymbol']}|{row['programEntryAddress']}"
            for row in flows
        ],
        "writeDefinitionSites": write_sites,
        "writeDefinitionSiteOrder": [row["id"] for row in write_sites],
        "handoffStateSites": handoffs,
        "handoffStateSiteOrder": [
            f"{row['kind']}|{row['category']}|{row['programSymbol']}|{row['programEntryAddress']}|{row['romPc']}"
            for row in handoffs
        ],
        "sourceFiles": {row["tableSymbol"]: row for row in source_files},
        "sourceFileOrder": [
            f"{row['category']}|{row['tableSymbol']}|{row['tableEntryAddress']}"
            for row in source_files
        ],
        "digests": {
            "programFlowsSha256": hashlib.sha256(canonical_json_bytes({"programFlows": flows}))
            .hexdigest()
            .upper(),
            "writeDefinitionSitesSha256": hashlib.sha256(
                canonical_json_bytes({"writeDefinitionSites": write_sites})
            )
            .hexdigest()
            .upper(),
            "handoffStateSitesSha256": hashlib.sha256(
                canonical_json_bytes({"handoffStateSites": handoffs})
            )
            .hexdigest()
            .upper(),
        },
    }
    source_identities = [
        {
            "path": row["sourcePath"],
            "sha256": hashlib.sha256((disasm / row["sourcePath"]).read_bytes()).hexdigest().upper(),
        }
        for row in source_files
    ] + [
        {
            "path": path,
            "sha256": hashlib.sha256((disasm / path).read_bytes()).hexdigest().upper(),
        }
        for path in _SUPPORT_SOURCE_PATHS
    ]
    source_context = {
        "h1Listing": {
            "path": "build/sf2build-h1.lst",
            "sha256": hashlib.sha256(listing_text.encode("utf-8")).hexdigest().upper(),
        },
        "sourceIdentities": source_identities,
        "anchors": list(physical_anchors.values()),
    }
    return event_request_state, summary, source_context


def build_map_event_request_state_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    """Build the exact public request-state contract from fresh source/H1/ROM inputs."""
    retained, dialogue, retained_owners = _fresh_retained_request_state_owners(
        rom_path, upstream_path
    )
    state, summary, source_context = _projection(
        retained, upstream_path=upstream_path, rom_path=rom_path
    )
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {
            "repository": "ShiningForceCentral/SF2DISASM",
            "commit": "c834c652b6862bc5679fd7f69a38a7093206efc6",
        },
        "romSha256": "9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9",
        "sourceContext": source_context,
        "retainedOwners": retained_owners,
        "eventRequestState": state,
        "unknowns": {key: "Unknown" for key in _UNKNOWN_KEYS},
        "summary": summary,
    }


def verify_map_event_request_state_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    output = build_map_event_request_state_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="map-event request-state rebuilt contract")
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner="map-event request-state fixture")
    if output != fixture:
        raise ValueError("map-event request-state fixture drift")
    destination = output_path or FIXTURE
    if output_path is not None:
        destination.parent.mkdir(parents=True, exist_ok=True)
        destination.write_bytes(canonical_json_bytes(output))
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": hashlib.sha256(canonical_json_bytes(output)).hexdigest().upper(),
        "Programs": output["summary"]["positiveProgramContextCount"],
        "Handoffs": output["summary"]["handoffStateSiteCount"],
        "Status": "PASS",
    }
