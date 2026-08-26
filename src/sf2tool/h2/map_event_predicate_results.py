"""Public H2 predicate-result control facts for retained map-event programs.

This owner narrows the already accepted map-event program corpus to the small
set of non-``chkFlg`` conditional branches.  It records only their immediate
condition producer, source-resolved branch shape, and the caller-side result
origin/entry seam.  The retained owners still own program parsing, direct RAM
accesses, direct transfers, handoff groups, aliases, and callee bodies.
"""

from __future__ import annotations

import hashlib
import json
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.h2.map_event_direct_control import (
    FIXTURE as DIRECT_CONTROL_FIXTURE,
)
from sf2tool.h2.map_event_direct_control import build_map_event_direct_control_contract
from sf2tool.h2.map_event_direct_handoff import (
    FIXTURE as DIRECT_HANDOFF_FIXTURE,
)
from sf2tool.h2.map_event_direct_handoff import build_map_event_direct_handoff_contract
from sf2tool.h2.map_event_direct_state import (
    FIXTURE as DIRECT_STATE_FIXTURE,
)
from sf2tool.h2.map_event_direct_state import (
    _direct_symbol,
    _h1_instruction_rows,
    _normalise_statement,
    build_map_event_direct_state_contract,
)
from sf2tool.h2.map_events import _canonical_bytes as _map_events_canonical_bytes
from sf2tool.h2.map_events import build_map_events_contract
from sf2tool.h2.map_events_fixture import load_map_events_fixture
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path

ID = "sf2-map-event-predicate-results-static-v1"
FIXTURE = repo_path("tests/fixtures/h2/map-event-predicate-results-static-v1.json")
SCHEMA = repo_path("schemas/h2/map-event-predicate-results-static-fixture.schema.json")
MAP_EVENTS_MANIFEST = repo_path("manifests/extractions/map-events-static.json")

_CATEGORIES = ("entityEvents", "zoneEvents", "itemEvents")
_PROGRAM_FIELDS = {
    "entityEvents": "entityTargetPrograms",
    "zoneEvents": "zoneTargetPrograms",
    "itemEvents": "itemTargetPrograms",
}
_SOURCE_PATHS = (
    "data/maps/entries/map03/mapsetups/s2_entityevents_506.asm",
    "data/maps/entries/map03/mapsetups/s2_entityevents_543.asm",
    "data/maps/entries/map06/mapsetups/s2_entityevents_701.asm",
    "data/maps/entries/map09/mapsetups/s2_entityevents.asm",
    "data/maps/entries/map10/mapsetups/s2_entityevents_722.asm",
    "data/maps/entries/map11/mapsetups/s2_entityevents.asm",
    "data/maps/entries/map20/mapsetups/s3_zoneevents_543.asm",
    "data/maps/entries/map22/mapsetups/s5_itemevents.asm",
    "data/maps/entries/map25/mapsetups/s2_entityevents.asm",
    "data/maps/entries/map28/mapsetups/s3_zoneevents.asm",
    "data/maps/entries/map31/mapsetups/s2_entityevents_830.asm",
    "data/maps/entries/map44/mapsetups/s2_entityevents_507.asm",
    "data/maps/entries/map63/mapsetups/s2_entityevents.asm",
    "data/maps/entries/map67/mapsetups/s3_zoneevents.asm",
    "data/maps/entries/map72/mapsetups/s3_zoneevents.asm",
)
_SOURCE_PATH_SET = frozenset(_SOURCE_PATHS)
_EXTRA_SOURCE_PATHS = (
    "sf2const.asm",
    "sf2enums.asm",
    "code/common/tech/jumpinterfaces/s02_jumpinterface.asm",
    "code/common/tech/jumpinterfaces/s03_jumpinterface_1.asm",
    "code/common/menus/yesnoprompt.asm",
    "code/common/stats/itemstats.asm",
    "code/common/stats/iteminventory.asm",
    "code/common/stats/combatantstats_1.asm",
    "code/common/scripting/entity/entityfunctions_2.asm",
)
_UNKNOWN_KEYS = (
    "naturalProgramReachability",
    "callerEntryRegisterAndState",
    "actualYesNoPromptResult",
    "actualInventoryLocationResult",
    "actualMandatoryItemResult",
    "actualCurrentHpResult",
    "actualEventRelativePosition",
    "actualEntityFacing",
    "actualCcrAndPredicateEvaluation",
    "actualBranchSelection",
    "successorExecutionAndSideEffects",
    "tailAndReturnState",
    "crossMapStateLifetime",
    "saveLoadPersistence",
    "inputUiDialogueAudioTimingAndStoryMeaning",
)
_EXPECTED_SOURCE_FILES = {
    "entityEvents": 10,
    "zoneEvents": 4,
    "itemEvents": 1,
}
_EXPECTED_CATEGORY_SUMMARIES = {
    "entityEvents": (684, 18, 14, 670, 16, 16, 10),
    "zoneEvents": (150, 5, 4, 146, 5, 5, 4),
    "itemEvents": (80, 1, 1, 79, 1, 1, 1),
}
_EXPECTED_ORIGIN_COUNTS = {
    "j_YesNoPrompt": (8, 8),
    "j_GetItemInventoryLocation": (7, 6),
    "EVENT_RELATIVE_POSITION": (5, 5),
    "ReceiveMandatoryItem": (2, 1),
    "j_GetCurrentHp": (1, 1),
    "ENTITY_FACING": (1, 1),
}
_EXPECTED_PRODUCER_COUNTS = {
    "cmpi": (12, 11),
    "tst": (8, 8),
    "btst": (3, 2),
    "direct-jsr-CCR": (1, 1),
}
_EXPECTED_EFFECTIVE_ORIGINS = {
    "j_YesNoPrompt": "YesNoPrompt",
    "j_GetItemInventoryLocation": "GetItemInventoryLocation",
    "ReceiveMandatoryItem": "ReceiveMandatoryItem",
    "j_GetCurrentHp": "GetCurrentHp",
}


def canonical_json_bytes(value: dict[str, Any]) -> bytes:
    """Emit the canonical public JSON representation."""
    return (
        json.dumps(value, ensure_ascii=False, sort_keys=True, separators=(",", ":")).encode("utf-8")
        + b"\n"
    )


def _validate_contract_order(value: dict[str, Any]) -> None:
    """Reject order drift in the compact corpus before whole-fixture comparison."""
    predicates = value["eventPredicateResults"]
    if predicates["sourceFileOrder"] != sorted(predicates["sourceFiles"]):
        raise ValueError("map-event predicate-results source-file order drift")
    expected_pair_order = [
        f"{row['category']}|{row['programSymbol']}|{row['producer']['romPc']}|{row['branch']['romPc']}"
        for row in predicates["pairs"]
    ]
    if predicates["pairOrder"] != expected_pair_order:
        raise ValueError("map-event predicate-results pair order drift")
    expected_physical_order = [
        f"{row['producerRomPc']}|{row['branchRomPc']}" for row in predicates["physicalPairs"]
    ]
    if predicates["physicalPairOrder"] != expected_physical_order:
        raise ValueError("map-event predicate-results physical pair order drift")
    if [row["symbol"] for row in predicates["resultOriginCohorts"]] != list(
        _EXPECTED_ORIGIN_COUNTS
    ):
        raise ValueError("map-event predicate-results result-origin cohort order drift")
    if [row["form"] for row in predicates["producerFormCohorts"]] != list(
        _EXPECTED_PRODUCER_COUNTS
    ):
        raise ValueError("map-event predicate-results producer-form cohort order drift")
    if [row["opcode"] for row in predicates["branchCohorts"]] != ["bne", "beq"]:
        raise ValueError("map-event predicate-results branch cohort order drift")


def _fixture_sha256(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest().upper()


def _fixture_projection(path: Path, output: dict[str, Any], *, name: str) -> dict[str, Any]:
    fixture = load_json(path)
    if fixture != output:
        raise ValueError(f"map-event predicate-results retained {name} fixture drift")
    return {
        "fixtureId": fixture["id"],
        "fixtureSha256": _fixture_sha256(path),
        "outputSha256": hashlib.sha256(canonical_json_bytes(output)).hexdigest().upper(),
    }


def _fresh_retained_owners(
    rom_path: Path, upstream_path: Path
) -> tuple[dict[str, Any], dict[str, Any]]:
    """Fresh-build all accepted owners before narrowing their read-only joins."""
    map_events = build_map_events_contract(rom_path, upstream_path)
    map_events_fixture = load_map_events_fixture()
    if map_events_fixture["expected"] != map_events:
        raise ValueError("map-event predicate-results retained map-events fixture drift")
    map_events_digest = hashlib.sha256(_map_events_canonical_bytes(map_events)).hexdigest().upper()
    map_events_manifest = load_json(MAP_EVENTS_MANIFEST)
    if (
        map_events_digest != map_events_manifest["outputSha256"]
        or map_events["summary"] != map_events_manifest["summary"]
    ):
        raise ValueError("map-event predicate-results retained map-events projection drift")
    direct_state = build_map_event_direct_state_contract(rom_path, upstream_path)
    direct_control = build_map_event_direct_control_contract(rom_path, upstream_path)
    direct_handoff = build_map_event_direct_handoff_contract(rom_path, upstream_path)
    return map_events, {
        "mapEvents": {
            "fixtureId": map_events_fixture["id"],
            "fixtureSha256": _fixture_sha256(
                repo_path("tests/fixtures/h2/map-events-static-v1.json")
            ),
            "outputSha256": map_events_digest,
        },
        "directState": _fixture_projection(DIRECT_STATE_FIXTURE, direct_state, name="direct-state"),
        "directControl": _fixture_projection(
            DIRECT_CONTROL_FIXTURE, direct_control, name="direct-control"
        ),
        "directHandoff": _fixture_projection(
            DIRECT_HANDOFF_FIXTURE, direct_handoff, name="direct-handoff"
        ),
        "directStateOutput": direct_state,
        "directControlOutput": direct_control,
    }


def _source_table_rows(map_events: dict[str, Any]) -> dict[tuple[str, str], dict[str, Any]]:
    rows: dict[tuple[str, str], dict[str, Any]] = {}
    for category in _CATEGORIES:
        for row in map_events["categories"][category]["sourceFiles"]:
            key = (category, row["path"])
            if key in rows:
                raise ValueError("map-event predicate-results duplicate table source path")
            rows[key] = row
    return rows


def _statement(operation: dict[str, Any]) -> str:
    operands = operation["operandTexts"]
    return _normalise_statement(
        operation["sourceMnemonic"] + (" " + ",".join(operands) if operands else "")
    )


def _assert_source_statement(lines: list[str], *, operation: dict[str, Any], context: str) -> None:
    source_line = operation["sourceLine"]
    if not 1 <= source_line <= len(lines):
        raise ValueError(f"map-event predicate-results source line range drift: {context}")
    if _normalise_statement(lines[source_line - 1]) != _statement(operation):
        raise ValueError(f"map-event predicate-results source opcode/operand drift: {context}")


def _branch_target(operation: dict[str, Any]) -> dict[str, Any]:
    target = operation["target"]
    if target is None or operation["mnemonic"] not in {"bne", "beq"}:
        raise ValueError("map-event predicate-results conditional branch target drift")
    if len(operation["operandTexts"]) != 1:
        raise ValueError("map-event predicate-results conditional branch operand count drift")
    return target


def _rom_branch_target(address: int, encoded: bytes) -> int:
    if len(encoded) == 2:
        displacement = int.from_bytes(encoded[1:], byteorder="big", signed=True)
        if displacement == 0:
            raise ValueError("map-event predicate-results short branch encoding drift")
        return address + 2 + displacement
    if len(encoded) == 4:
        return address + 2 + int.from_bytes(encoded[2:], byteorder="big", signed=True)
    raise ValueError("map-event predicate-results branch width drift")


def _h1_rom_anchor(
    operation: dict[str, Any], *, h1_rows: dict[int, tuple[bytes, str]], rom: bytes, context: str
) -> dict[str, Any]:
    row = h1_rows.get(operation["address"])
    if row is None or row[1] != _statement(operation):
        raise ValueError(f"map-event predicate-results H1 opcode/operand drift: {context}")
    encoded = row[0]
    rom_encoded = rom[operation["address"] : operation["address"] + len(encoded)]
    if operation["mnemonic"] in {"bne", "beq"}:
        # The H1 listing keeps an unresolved branch displacement as zero, so
        # the source statement establishes condition/width while the ROM
        # displacement establishes the exact resolved target.
        if len(rom_encoded) != len(encoded) or encoded[:1] != rom_encoded[:1]:
            raise ValueError(f"map-event predicate-results H1/ROM opcode/width drift: {context}")
        target = _branch_target(operation)["effectiveTargetAddress"]
        if _rom_branch_target(operation["address"], rom_encoded) != target:
            raise ValueError(f"map-event predicate-results ROM branch target drift: {context}")
    else:
        if encoded != rom_encoded:
            raise ValueError(
                f"map-event predicate-results H1/ROM instruction-byte drift: {context}"
            )
    return {
        "romPc": operation["address"],
        "instructionByteLength": len(encoded),
        "h1InstructionSha256": hashlib.sha256(encoded).hexdigest().upper(),
        "romInstructionSha256": hashlib.sha256(rom_encoded).hexdigest().upper(),
    }


def _producer(operation: dict[str, Any]) -> dict[str, Any]:
    """Keep the exact source-shaped CCR producer form, never a semantic result."""
    mnemonic = operation["mnemonic"]
    operands = operation["operandTexts"]
    width = operation["sizeSuffix"]
    common = {
        "sourceOrder": operation["sourceOrder"],
        "sourceLine": operation["sourceLine"],
        "romPc": operation["address"],
        "sourceMnemonic": operation["sourceMnemonic"],
        "width": width,
    }
    if mnemonic == "cmpi":
        if width not in {".b", ".w"} or len(operands) != 2 or not operands[0].startswith("#"):
            raise ValueError("map-event predicate-results cmpi predicate shape drift")
        return {
            **common,
            "form": "cmpi",
            "comparisonOperand": operands[0],
            "testedOperand": operands[1],
            "bitOperand": None,
        }
    if mnemonic == "tst":
        if width not in {".b", ".w"} or len(operands) != 1:
            raise ValueError("map-event predicate-results tst predicate shape drift")
        return {
            **common,
            "form": "tst",
            "comparisonOperand": None,
            "testedOperand": operands[0],
            "bitOperand": None,
        }
    if mnemonic == "btst":
        if width is not None or len(operands) != 2 or not operands[0].startswith("#"):
            raise ValueError("map-event predicate-results btst predicate shape drift")
        return {
            **common,
            "form": "btst",
            "comparisonOperand": None,
            "testedOperand": operands[1],
            "bitOperand": operands[0],
        }
    if mnemonic == "jsr":
        if width is not None or len(operands) != 1:
            raise ValueError("map-event predicate-results direct-jsr CCR predicate shape drift")
        return {
            **common,
            "form": "direct-jsr-CCR",
            "comparisonOperand": None,
            "testedOperand": None,
            "bitOperand": None,
        }
    raise ValueError("map-event predicate-results unclassified predicate producer")


def _context_key(category: str, program: dict[str, Any], rom_pc: int) -> tuple[str, str, int, int]:
    return (category, program["canonicalSymbol"], program["entryAddress"], rom_pc)


def _transfer_for(
    direct_control: dict[str, Any], *, category: str, program: dict[str, Any], rom_pc: int
) -> dict[str, Any]:
    matches = [
        row
        for row in direct_control["eventDirectControl"]["transferSites"]
        if _context_key(category, program, rom_pc)
        == (row["category"], row["programSymbol"], row["programEntryAddress"], row["romPc"])
    ]
    if len(matches) != 1 or matches[0]["transferKind"] != "direct-call":
        raise ValueError("map-event predicate-results retained direct-call result origin drift")
    return matches[0]


def _result_origin(
    operations: list[dict[str, Any]],
    producer_index: int,
    *,
    category: str,
    program: dict[str, Any],
    direct_state: dict[str, Any],
    direct_control: dict[str, Any],
) -> dict[str, Any]:
    """Join a test to its immediately retained caller-side source, not a callee body."""
    producer = operations[producer_index]
    shape = _producer(producer)
    direct_state_sites = {
        (row["category"], row["programSymbol"], row["programEntryAddress"], row["romPc"]): row
        for row in direct_state["eventDirectState"]["accessSites"]
    }
    direct_operand = (
        _direct_symbol(shape["testedOperand"]) if shape["testedOperand"] is not None else None
    )
    if direct_operand is not None:
        state_site = direct_state_sites.get(_context_key(category, program, producer["address"]))
        if state_site is None or state_site["symbol"] != direct_operand:
            raise ValueError("map-event predicate-results retained direct-state operand drift")
        if direct_operand == "EVENT_RELATIVE_POSITION":
            return {
                "symbol": direct_operand,
                "kind": "direct-fixed-ram",
                "transferRomPc": None,
                "instructionTargetSymbol": None,
                "effectiveTargetSymbol": None,
                "returnContinuationRomPc": None,
                "directStateAccessSiteOrder": state_site["siteOrder"],
            }
        if direct_operand == "ENTITY_FACING":
            transfer = _nearest_direct_call(
                operations,
                producer_index,
                category=category,
                program=program,
                direct_control=direct_control,
            )
            if transfer["effectiveTargetSymbol"] != "WaitForEntityToStopMoving":
                raise ValueError("map-event predicate-results entity-facing wait seam drift")
            return {
                "symbol": direct_operand,
                "kind": "direct-fixed-ram-after-wait",
                "transferRomPc": transfer["romPc"],
                "instructionTargetSymbol": transfer["instructionTargetSymbol"],
                "effectiveTargetSymbol": transfer["effectiveTargetSymbol"],
                "returnContinuationRomPc": _return_continuation_pc(transfer, direct_control),
                "directStateAccessSiteOrder": state_site["siteOrder"],
            }
        raise ValueError("map-event predicate-results unexpected direct-state predicate operand")
    transfer = (
        _transfer_for(
            direct_control,
            category=category,
            program=program,
            rom_pc=producer["address"],
        )
        if shape["form"] == "direct-jsr-CCR"
        else _nearest_direct_call(
            operations,
            producer_index,
            category=category,
            program=program,
            direct_control=direct_control,
        )
    )
    allowed = {
        "j_YesNoPrompt",
        "j_GetItemInventoryLocation",
        "ReceiveMandatoryItem",
        "j_GetCurrentHp",
    }
    if transfer["instructionTargetSymbol"] not in allowed:
        raise ValueError("map-event predicate-results unexpected result-origin target")
    if (
        transfer["effectiveTargetSymbol"]
        != _EXPECTED_EFFECTIVE_ORIGINS[transfer["instructionTargetSymbol"]]
    ):
        raise ValueError("map-event predicate-results result-origin effective target drift")
    return {
        "symbol": transfer["instructionTargetSymbol"],
        "kind": "direct-call-ccr" if shape["form"] == "direct-jsr-CCR" else "direct-call-return",
        "transferRomPc": transfer["romPc"],
        "instructionTargetSymbol": transfer["instructionTargetSymbol"],
        "effectiveTargetSymbol": transfer["effectiveTargetSymbol"],
        "returnContinuationRomPc": _return_continuation_pc(transfer, direct_control),
        "directStateAccessSiteOrder": None,
    }


def _nearest_direct_call(
    operations: list[dict[str, Any]],
    producer_index: int,
    *,
    category: str,
    program: dict[str, Any],
    direct_control: dict[str, Any],
) -> dict[str, Any]:
    for operation in reversed(operations[:producer_index]):
        if operation["family"] != "raw-68000-control-flow" or operation["mnemonic"] not in {
            "jsr",
            "bsr",
        }:
            continue
        return _transfer_for(
            direct_control,
            category=category,
            program=program,
            rom_pc=operation["address"],
        )
    raise ValueError("map-event predicate-results missing result-origin direct call")


def _return_continuation_pc(transfer: dict[str, Any], direct_control: dict[str, Any]) -> int:
    rows = [
        row
        for row in direct_control["eventDirectControl"]["callContinuations"]
        if row["siteOrder"] == transfer["siteOrder"] and row["romPc"] == transfer["romPc"]
    ]
    if len(rows) != 1:
        raise ValueError("map-event predicate-results retained return continuation drift")
    return rows[0]["operation"]["romPc"]


def _entry_seams(
    direct_control: dict[str, Any],
    *,
    source_lines: dict[str, list[str]],
    h1_rows: dict[int, tuple[bytes, str]],
    rom: bytes,
) -> list[dict[str, Any]]:
    """Keep only eight alias/effective entry identities; never parse their bodies."""
    aliases = direct_control["eventDirectControl"]["aliasJoins"]
    targets = direct_control["eventDirectControl"]["effectiveTargets"]
    wanted_aliases = {"j_YesNoPrompt", "j_GetItemInventoryLocation", "j_GetCurrentHp"}
    wanted_targets = {
        "YesNoPrompt",
        "GetItemInventoryLocation",
        "ReceiveMandatoryItem",
        "GetCurrentHp",
        "WaitForEntityToStopMoving",
    }
    seams: list[dict[str, Any]] = []
    for alias in aliases:
        if alias["instructionTargetSymbol"] not in wanted_aliases:
            continue
        if (
            alias["effectiveTargetSymbol"]
            != _EXPECTED_EFFECTIVE_ORIGINS[alias["instructionTargetSymbol"]]
        ):
            raise ValueError("map-event predicate-results alias effective target drift")
        _assert_entry_label(
            source_lines,
            path=alias["sourcePath"],
            source_line=alias["sourceLine"],
            symbol=alias["instructionTargetSymbol"],
        )
        _assert_entry_h1_rom(alias["instructionTargetAddress"], h1_rows=h1_rows, rom=rom)
        seams.append(
            {
                "role": "jump-interface-alias",
                "symbol": alias["instructionTargetSymbol"],
                "entryAddress": alias["instructionTargetAddress"],
                "sourcePath": alias["sourcePath"],
                "sourceLine": alias["sourceLine"],
            }
        )
    for target in targets:
        if target["symbol"] not in wanted_targets:
            continue
        _assert_entry_label(
            source_lines,
            path=target["sourcePath"],
            source_line=target["sourceLine"],
            symbol=target["symbol"],
        )
        _assert_entry_h1_rom(target["entryAddress"], h1_rows=h1_rows, rom=rom)
        seams.append(
            {
                "role": "effective-entry",
                "symbol": target["symbol"],
                "entryAddress": target["entryAddress"],
                "sourcePath": target["sourcePath"],
                "sourceLine": target["sourceLine"],
            }
        )
    seams.sort(key=lambda row: (row["role"], row["entryAddress"]))
    if len(seams) != 8:
        raise ValueError("map-event predicate-results retained entry seam denominator drift")
    return seams


def _assert_entry_label(
    source_lines: dict[str, list[str]], *, path: str, source_line: int, symbol: str
) -> None:
    line = source_lines[path][source_line - 1].split(";", maxsplit=1)[0].strip()
    if line != f"{symbol}:":
        raise ValueError("map-event predicate-results entry label drift")


def _assert_entry_h1_rom(
    address: int, *, h1_rows: dict[int, tuple[bytes, str]], rom: bytes
) -> None:
    row = h1_rows.get(address)
    if row is None:
        raise ValueError("map-event predicate-results entry H1 drift")
    encoded = row[0]
    rom_encoded = rom[address : address + len(encoded)]
    if len(rom_encoded) != len(encoded) or rom_encoded[:2] != encoded[:2]:
        raise ValueError("map-event predicate-results entry H1/ROM drift")


def _predicate_projection(
    map_events: dict[str, Any],
    *,
    direct_state: dict[str, Any],
    direct_control: dict[str, Any],
    upstream_path: Path,
    rom_path: Path,
) -> tuple[dict[str, Any], dict[str, Any], dict[str, Any]]:
    root = upstream_path.resolve(strict=True)
    disasm = root / "disasm"
    if not disasm.is_dir():
        disasm = root
    listing_text = (root / "build/sf2build-h1.lst").read_text(encoding="utf-8")
    h1_rows = _h1_instruction_rows(listing_text)
    rom = rom_path.resolve(strict=True).read_bytes()
    table_rows = _source_table_rows(map_events)
    source_lines = {
        path: (disasm / path).read_text(encoding="utf-8").splitlines()
        for path in _SOURCE_PATHS + _EXTRA_SOURCE_PATHS
    }
    pairs: list[dict[str, Any]] = []
    direct_flag_branches: list[tuple[str, str, int, int]] = []
    all_conditional_branches: list[tuple[str, str, int, int]] = []
    physical_pair_contexts: dict[tuple[int, int], list[int]] = {}
    physical_caller_anchors: dict[tuple[str, int], dict[str, Any]] = {}
    positive_programs: set[tuple[str, str, int]] = set()
    positive_sources: set[tuple[str, str]] = set()

    for category in _CATEGORIES:
        for program in map_events[_PROGRAM_FIELDS[category]]:
            operations = program["operations"]
            for index, branch in enumerate(operations):
                if branch["mnemonic"] not in {"bne", "beq"}:
                    continue
                if index == 0:
                    raise ValueError("map-event predicate-results branch without producer drift")
                all_conditional_branches.append(_context_key(category, program, branch["address"]))
                producer = operations[index - 1]
                if producer["sourceMnemonic"] == "chkFlg":
                    direct_flag_branches.append(_context_key(category, program, branch["address"]))
                    continue
                if program["sourcePath"] not in _SOURCE_PATH_SET:
                    raise ValueError("map-event predicate-results unexpected non-flag source path")
                table = table_rows[(category, program["sourcePath"])]
                context = f"{program['canonicalSymbol']}:{branch['sourceLine']}"
                _assert_source_statement(
                    source_lines[program["sourcePath"]], operation=producer, context=context
                )
                _assert_source_statement(
                    source_lines[program["sourcePath"]], operation=branch, context=context
                )
                producer_shape = _producer(producer)
                producer_anchor = _h1_rom_anchor(
                    producer, h1_rows=h1_rows, rom=rom, context=context
                )
                branch_anchor = _h1_rom_anchor(branch, h1_rows=h1_rows, rom=rom, context=context)
                target = _branch_target(branch)
                _assert_entry_label(
                    source_lines,
                    path=program["sourcePath"],
                    source_line=target["effectiveTargetAddressLabels"][0]["sourceLine"],
                    symbol=target["effectiveTargetSymbol"],
                )
                origin = _result_origin(
                    operations,
                    index - 1,
                    category=category,
                    program=program,
                    direct_state=direct_state,
                    direct_control=direct_control,
                )
                pair = {
                    "siteOrder": len(pairs),
                    "category": category,
                    "programSymbol": program["canonicalSymbol"],
                    "programEntryAddress": program["entryAddress"],
                    "tableSymbol": table["symbol"],
                    "tableEntryAddress": table["address"],
                    "sourcePath": program["sourcePath"],
                    "producer": producer_shape,
                    "branch": {
                        "sourceOrder": branch["sourceOrder"],
                        "sourceLine": branch["sourceLine"],
                        "romPc": branch["address"],
                        "sourceMnemonic": branch["sourceMnemonic"],
                        "targetSymbol": target["effectiveTargetSymbol"],
                        "targetAddress": target["effectiveTargetAddress"],
                        "takenTargetKind": "target",
                        "fallthroughKind": "next-lexical-operation",
                    },
                    "resultOrigin": origin,
                    "producerAnchor": producer_anchor,
                    "branchAnchor": branch_anchor,
                }
                pairs.append(pair)
                pair_key = (producer["address"], branch["address"])
                physical_pair_contexts.setdefault(pair_key, []).append(pair["siteOrder"])
                for role, anchor in (("producer", producer_anchor), ("branch", branch_anchor)):
                    physical_caller_anchors.setdefault(
                        (role, anchor["romPc"]), {"role": role, **anchor}
                    )
                if origin["kind"] != "direct-fixed-ram-after-wait":
                    transfer_pc = origin["transferRomPc"]
                    if transfer_pc is not None and transfer_pc != producer["address"]:
                        _transfer_for(
                            direct_control,
                            category=category,
                            program=program,
                            rom_pc=transfer_pc,
                        )
                        transfer_operation = next(
                            item for item in operations if item["address"] == transfer_pc
                        )
                        physical_caller_anchors.setdefault(
                            ("result-origin-call", transfer_pc),
                            {
                                "role": "result-origin-call",
                                **_h1_rom_anchor(
                                    transfer_operation,
                                    h1_rows=h1_rows,
                                    rom=rom,
                                    context=context,
                                ),
                            },
                        )
                positive_programs.add(
                    (category, program["canonicalSymbol"], program["entryAddress"])
                )
                positive_sources.add((category, table["symbol"]))

    all_physical = {item[3] for item in all_conditional_branches}
    direct_flag_physical = {item[3] for item in direct_flag_branches}
    if (len(all_conditional_branches), len(all_physical)) != (340, 336):
        raise ValueError("map-event predicate-results conditional corpus denominator drift")
    if (len(direct_flag_branches), len(direct_flag_physical)) != (316, 314):
        raise ValueError("map-event predicate-results direct-flag exclusion drift")
    if len(pairs) != 24 or len(physical_pair_contexts) != 22:
        raise ValueError("map-event predicate-results predicate pair denominator drift")
    if len(physical_caller_anchors) != 59:
        raise ValueError("map-event predicate-results caller anchor denominator drift")
    physical_pairs = [
        {
            "producerRomPc": producer_pc,
            "branchRomPc": branch_pc,
            "contextSiteOrders": orders,
        }
        for (producer_pc, branch_pc), orders in sorted(physical_pair_contexts.items())
    ]
    category_summaries: list[dict[str, Any]] = []
    for category in _CATEGORIES:
        program_count = len(map_events[_PROGRAM_FIELDS[category]])
        category_pairs = [row for row in pairs if row["category"] == category]
        physical_count = len(
            {(row["producer"]["romPc"], row["branch"]["romPc"]) for row in category_pairs}
        )
        expected = _EXPECTED_CATEGORY_SUMMARIES[category]
        actual = (
            program_count,
            len(category_pairs),
            sum(row[0] == category for row in positive_programs),
            program_count - sum(row[0] == category for row in positive_programs),
            len({row["producer"]["romPc"] for row in category_pairs}),
            physical_count,
            sum(row[0] == category for row in positive_sources),
        )
        if actual != expected:
            raise ValueError(f"map-event predicate-results {category} category denominator drift")
        category_summaries.append(
            {
                "category": category,
                "programContextCount": actual[0],
                "contextPairCount": actual[1],
                "positiveProgramContextCount": actual[2],
                "zeroProgramContextCount": actual[3],
                "physicalProducerCount": actual[4],
                "physicalPairCount": actual[5],
                "sourceFileCount": actual[6],
            }
        )
    origin_counts = Counter(row["resultOrigin"]["symbol"] for row in pairs)
    origin_physical_counts = Counter(
        pairs[row["contextSiteOrders"][0]]["resultOrigin"]["symbol"] for row in physical_pairs
    )
    if {
        key: (origin_counts[key], origin_physical_counts[key]) for key in _EXPECTED_ORIGIN_COUNTS
    } != _EXPECTED_ORIGIN_COUNTS:
        raise ValueError("map-event predicate-results result-origin cohort drift")
    form_counts = Counter(row["producer"]["form"] for row in pairs)
    form_physical_counts = Counter(
        pairs[row["contextSiteOrders"][0]]["producer"]["form"] for row in physical_pairs
    )
    if {
        key: (form_counts[key], form_physical_counts[key]) for key in _EXPECTED_PRODUCER_COUNTS
    } != _EXPECTED_PRODUCER_COUNTS:
        raise ValueError("map-event predicate-results producer-form cohort drift")
    branch_counts = Counter(
        row["branch"]["sourceMnemonic"].split(".", maxsplit=1)[0] for row in pairs
    )
    branch_physical_counts = Counter(
        pairs[row["contextSiteOrders"][0]]["branch"]["sourceMnemonic"].split(".", maxsplit=1)[0]
        for row in physical_pairs
    )
    if {key: (branch_counts[key], branch_physical_counts[key]) for key in ("bne", "beq")} != {
        "bne": (20, 18),
        "beq": (4, 4),
    }:
        raise ValueError("map-event predicate-results branch polarity cohort drift")
    source_files = {
        row["symbol"]: {
            "category": category,
            "tableEntryAddress": row["address"],
            "sourcePath": path,
        }
        for (category, path), row in table_rows.items()
        if (category, row["symbol"]) in positive_sources
    }
    if set(source_files) != {symbol for _category, symbol in positive_sources}:
        raise ValueError("map-event predicate-results source-file projection drift")
    source_file_category_counts = {
        category: sum(row["category"] == category for row in source_files.values())
        for category in _CATEGORIES
    }
    if len(source_files) != 15 or source_file_category_counts != _EXPECTED_SOURCE_FILES:
        raise ValueError("map-event predicate-results source-file denominator drift")
    entry_seams = _entry_seams(direct_control, source_lines=source_lines, h1_rows=h1_rows, rom=rom)
    source_identities = list(_SOURCE_PATHS + _EXTRA_SOURCE_PATHS)
    if len(source_identities) != 24 or len(set(source_identities)) != 24:
        raise ValueError("map-event predicate-results source identity denominator drift")
    source_context = {
        "h1Listing": {
            "path": "build/sf2build-h1.lst",
            "sha256": hashlib.sha256(listing_text.encode("utf-8")).hexdigest().upper(),
        },
        "sourceIdentities": [
            {
                "path": path,
                "sha256": hashlib.sha256((disasm / path).read_bytes()).hexdigest().upper(),
            }
            for path in source_identities
        ],
        "physicalCallerAnchors": list(physical_caller_anchors.values()),
        "entrySeams": entry_seams,
    }
    summary = {
        "programContextCount": 914,
        "operationCount": 3579,
        "conditionalContextCount": 340,
        "conditionalPhysicalCount": 336,
        "directFlagContextExclusionCount": 316,
        "directFlagPhysicalExclusionCount": 314,
        "contextPairCount": len(pairs),
        "physicalPairCount": len(physical_pairs),
        "positiveProgramContextCount": len(positive_programs),
        "zeroProgramContextCount": 914 - len(positive_programs),
        "sourceFileCount": len(source_files),
        "sourceIdentityCount": len(source_identities),
        "physicalCallerAnchorCount": len(physical_caller_anchors),
        "entrySeamCount": len(entry_seams),
    }
    return (
        {
            "categorySummaries": category_summaries,
            "sourceFiles": source_files,
            "sourceFileOrder": sorted(source_files),
            "pairs": pairs,
            "pairOrder": [
                f"{row['category']}|{row['programSymbol']}|{row['producer']['romPc']}|{row['branch']['romPc']}"
                for row in pairs
            ],
            "physicalPairs": physical_pairs,
            "physicalPairOrder": [
                f"{row['producerRomPc']}|{row['branchRomPc']}" for row in physical_pairs
            ],
            "resultOriginCohorts": [
                {
                    "symbol": symbol,
                    "contextPairCount": origin_counts[symbol],
                    "physicalPairCount": origin_physical_counts[symbol],
                }
                for symbol in _EXPECTED_ORIGIN_COUNTS
            ],
            "producerFormCohorts": [
                {
                    "form": form,
                    "contextPairCount": form_counts[form],
                    "physicalPairCount": form_physical_counts[form],
                }
                for form in _EXPECTED_PRODUCER_COUNTS
            ],
            "branchCohorts": [
                {
                    "opcode": opcode,
                    "contextPairCount": branch_counts[opcode],
                    "physicalPairCount": branch_physical_counts[opcode],
                }
                for opcode in ("bne", "beq")
            ],
        },
        summary,
        source_context,
    )


def build_map_event_predicate_results_contract(
    rom_path: Path, upstream_path: Path
) -> dict[str, Any]:
    """Build the static predicate-result contract from fresh retained owners."""
    map_events, retained = _fresh_retained_owners(rom_path, upstream_path)
    predicates, summary, source_context = _predicate_projection(
        map_events,
        direct_state=retained.pop("directStateOutput"),
        direct_control=retained.pop("directControlOutput"),
        upstream_path=upstream_path,
        rom_path=rom_path,
    )
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": map_events["upstream"],
        "romSha256": map_events["romSha256"],
        "retainedOwners": retained,
        "sourceContext": source_context,
        "eventPredicateResults": predicates,
        "unknowns": {key: "Unknown" for key in _UNKNOWN_KEYS},
        "summary": summary,
    }


def verify_map_event_predicate_results_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner=str(FIXTURE))
    _validate_contract_order(fixture)
    output = build_map_event_predicate_results_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="map-event predicate-results static contract")
    _validate_contract_order(output)
    if fixture != output:
        raise ValueError("map-event predicate-results complete semantic fixture drift")
    digest = hashlib.sha256(canonical_json_bytes(output)).hexdigest().upper()
    destination = output_path or repo_path("local/derived/map-event-predicate-results-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(
        json.dumps(output, ensure_ascii=False, indent=2).encode("utf-8") + b"\n"
    )
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Pairs": output["summary"]["contextPairCount"],
        "Status": "PASS",
    }
