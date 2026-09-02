"""Static same-program direct-flag lifecycle relations for map-event programs.

This module deliberately narrows the accepted map-events direct-flag corpus.
It proves only source/H1/ROM-local ordering: a direct ``chkFlg`` and a direct
``setFlg`` or ``clrFlg`` use the same numeric operand in one parsed program.
It does not claim that a conditional path or its later mutation executes.
"""

from __future__ import annotations

import hashlib
import json
from collections import Counter, defaultdict
from copy import deepcopy
from pathlib import Path
from typing import Any

from sf2tool.h2.interrupts import FIXTURE as INTERRUPTS_FIXTURE
from sf2tool.h2.map_event_direct_control import (
    FIXTURE as DIRECT_CONTROL_FIXTURE,
)
from sf2tool.h2.map_event_direct_control import (
    build_map_event_direct_control_contract,
)
from sf2tool.h2.map_event_direct_handoff import (
    FIXTURE as DIRECT_HANDOFF_FIXTURE,
)
from sf2tool.h2.map_event_direct_handoff import (
    _h1_instruction_rows,
    _normalise_statement,
    build_map_event_direct_handoff_contract,
)
from sf2tool.h2.map_event_direct_state import (
    FIXTURE as DIRECT_STATE_FIXTURE,
)
from sf2tool.h2.map_event_direct_state import (
    build_map_event_direct_state_contract,
)
from sf2tool.h2.map_event_predicate_results import (
    FIXTURE as PREDICATE_RESULTS_FIXTURE,
)
from sf2tool.h2.map_event_predicate_results import (
    build_map_event_predicate_results_contract,
)
from sf2tool.h2.map_events import _canonical_bytes as _map_events_canonical_bytes
from sf2tool.h2.map_events import build_map_events_contract
from sf2tool.h2.map_events_fixture import load_map_events_fixture
from sf2tool.h2.stats import FIXTURE as STATS_FIXTURE
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.research_index import listing_symbol_addresses

ID = "sf2-map-event-flag-lifecycle-state-static-v1"
FIXTURE = repo_path("tests/fixtures/h2/map-event-flag-lifecycle-state-static-v1.json")
SCHEMA = repo_path("schemas/h2/map-event-flag-lifecycle-state-static-fixture.schema.json")
MAP_EVENTS_FIXTURE = repo_path("tests/fixtures/h2/map-events-static-v1.json")
MAP_EVENTS_DIRECT_FLAGS_FIXTURE = repo_path("tests/fixtures/h2/map-events/direct-flags.json")
MAP_EVENTS_MANIFEST = repo_path("manifests/extractions/map-events-static.json")
RESEARCH_INDEX = repo_path("manifests/research-index.json")

_ROM_SHA256 = "9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9"
_UPSTREAM_COMMIT = "c834c652b6862bc5679fd7f69a38a7093206efc6"
_PREDECESSOR_INDEX_SHA256 = "4241E190B1C52409862AD53412DCCC1F1E8BA3A9868725EC77631851854C6CB1"
_CATEGORIES = ("entityEvents", "zoneEvents", "itemEvents")
_PROGRAM_FIELDS = {
    "entityEvents": "entityTargetPrograms",
    "zoneEvents": "zoneTargetPrograms",
    "itemEvents": "itemTargetPrograms",
}
_RETAINED_OWNER_EXPECTED = {
    "mapEventsDirectFlags": {
        "fixtureId": "sf2-map-events-static-v1:direct-flags",
        "fixtureSha256": "C05D38D53A3022C3EDEE6D1BFE16A44A25A99C3CF99F9EE63B08D8865F898366",
        "verifierPath": "src/sf2tool/h2/map_events.py",
        "verifierSha256": "5A5193DECF494292C679A17A51D10ADD481F62948803FC1CF6250FC05F32B5EE",
        "semanticSha256": "EF713A4AF6BE710780811E817EE27E5E5B0413CE8E0F84BA37731B57E3D6F389",
    },
    "directState": {
        "fixtureId": "sf2-map-event-direct-state-static-v1",
        "fixtureSha256": "2756ED5556939AD0DE9A98101A38C12F985D6BA2E892611B7610EDCB366F74D7",
        "verifierPath": "src/sf2tool/h2/map_event_direct_state.py",
        "verifierSha256": "AA724E98E1AF3C434B5846E63A97D60F09061D344B3C0E76FB753AF644614E19",
        "semanticSha256": "CBB64D022F49F811E903A9458B944FFBC1978EF5D2038EFFBF114A7AA425627A",
    },
    "directControl": {
        "fixtureId": "sf2-map-event-direct-control-static-v1",
        "fixtureSha256": "6645CA05C16A13FDAA6FCC43F155E70EFE7B76F43ACA12FFFBF8EBA6E5AA5CBD",
        "verifierPath": "src/sf2tool/h2/map_event_direct_control.py",
        "verifierSha256": "1A3962071BD8365642E82685E4C9C2B7555EB47F45975A0D48A7A3AAF0FE219A",
        "semanticSha256": "CE5178147C24BEDF3CB146C0AB55B80DB7270DB3A3904EB867E20ED0E41779D4",
    },
    "directHandoff": {
        "fixtureId": "sf2-map-event-direct-handoff-static-v1",
        "fixtureSha256": "66535A7F951A96BDB44745CFF300B790BFC67B844C9E9FAE8764F1E069122208",
        "verifierPath": "src/sf2tool/h2/map_event_direct_handoff.py",
        "verifierSha256": "7F0D606DC8C2A09D5B32FD600E22FF25F7D119E5934D2C1149CE7BA01CCCDB34",
        "semanticSha256": "F0A5CEE2D98900CFC96BD4930557614823DA0F9E3FB9BB646B2EE83D64B2CC52",
    },
    "predicateResults": {
        "fixtureId": "sf2-map-event-predicate-results-static-v1",
        "fixtureSha256": "B2AF49C8A20B96C4A92A26383154942DBDC4ABFBCE304183F2597F64AD220745",
        "verifierPath": "src/sf2tool/h2/map_event_predicate_results.py",
        "verifierSha256": "FD8D2519BEE4B12556B585F986BBA1E3D1FFEC2742C06387C027680624B9D4B1",
        "semanticSha256": "7CB470BF94A243257F5432E1A8D129F82A527D1EEF882A99307306449844E299",
    },
    "commonStatsFlags": {
        "fixtureId": "sf2-common-stats-static-v1",
        "fixtureSha256": "828368BBD364873EB9D58DB473DBE2B26D3F4524F7F74593902C91B2D4989865",
        "verifierPath": "src/sf2tool/h2/stats.py",
        "verifierSha256": "E63F744ACECF59D1F25AA5B424368A432297CF3F73864494AA06D7298DB045B8",
        "semanticSha256": "FD8DAF2D40DD6F864FB1217F0DE281CD75847EE1376792E985D3A39490DA3C3B",
    },
    "technicalInterruptTrapFlags": {
        "fixtureId": "sf2-tech-interrupts-static-v1",
        "fixtureSha256": "B3B18201CDAC43F0735A301BFFF01A4937100466DCA672EE598133F0C5A14965",
        "verifierPath": "src/sf2tool/h2/interrupts.py",
        "verifierSha256": "98B78DE87735DC910540A6D8A0439C5398E4EBADF92A3FEF64BE2E2E6E05EFC8",
        "semanticSha256": "78A146F1115EB3F34AB92ABEF36CB95429174B1EB0E759D8F1BDC253BEA499D7",
    },
}
_ACCESS_KINDS = {"chkFlg": "read", "setFlg": "set", "clrFlg": "clear"}
_LIFECYCLE_ACCESS_SEQUENCES = {
    ("read", "set"): 121,
    ("read", "set", "read", "set"): 3,
    ("read", "clear"): 2,
    ("read", "read", "set"): 1,
    ("set", "read"): 1,
    ("read", "clear", "clear"): 1,
    ("read", "set", "clear"): 1,
    ("read", "clear", "set"): 1,
}
_UNKNOWN_KEYS = (
    "naturalProgramReachability",
    "callerEntryFlagState",
    "actualFlagValueAtRead",
    "actualConditionalBranchSelection",
    "actualMutationReachability",
    "runtimeFlagValueAfterMutation",
    "crossProgramLifecycleOrdering",
    "mapSetupAndRecordSelection",
    "calleeAndScriptEffects",
    "saveLoadAndCrossMapPersistence",
    "dialogueAudioPresentationAndTiming",
    "storyMeaning",
)


def canonical_json_bytes(value: dict[str, Any]) -> bytes:
    """Return the canonical public semantic form used for retained hashes."""
    return (
        json.dumps(value, ensure_ascii=False, sort_keys=True, separators=(",", ":")).encode("utf-8")
        + b"\n"
    )


def _sha(value: bytes) -> str:
    return hashlib.sha256(value).hexdigest().upper()


def _fixture_sha256(path: Path) -> str:
    return _sha(path.read_bytes())


def _root(upstream_path: Path) -> Path:
    root = upstream_path.resolve(strict=True)
    return root / "disasm" if (root / "disasm").is_dir() else root


def _number(token: str) -> int:
    if token.startswith("$"):
        return int(token[1:], 16)
    if token.isdecimal():
        return int(token, 10)
    raise ValueError(f"map-event flag lifecycle numeric flag operand drift: {token}")


def _operation_statement(operation: dict[str, Any]) -> str:
    operands = operation["operandTexts"]
    return operation["sourceMnemonic"] + (" " + ",".join(operands) if operands else "")


def _source_line(lines: list[str], operation: dict[str, Any]) -> None:
    line = operation["sourceLine"]
    if not 1 <= line <= len(lines):
        raise ValueError("map-event flag lifecycle source line range drift")
    if _normalise_statement(lines[line - 1]) != _normalise_statement(
        _operation_statement(operation)
    ):
        raise ValueError("map-event flag lifecycle source opcode/operand drift")


def _h1_span(
    operation: dict[str, Any],
    *,
    end_address: int,
    h1_rows: dict[int, tuple[bytes, str]],
    rom: bytes,
) -> int:
    """Require one source operation's complete emitted H1/ROM byte range."""
    start_address = operation["address"]
    if end_address <= start_address:
        raise ValueError("map-event flag lifecycle operation range drift")
    cursor = start_address
    while cursor < end_address:
        row = h1_rows.get(cursor)
        if row is None:
            raise ValueError("map-event flag lifecycle H1 operation range drift")
        encoded, _statement = row
        if not encoded or cursor + len(encoded) > end_address:
            raise ValueError("map-event flag lifecycle H1 operation span drift")
        rom_encoded = rom[cursor : cursor + len(encoded)]
        # The accepted H1 listing uses zero displacement placeholders for
        # source-resolved PC-relative control forms. Preserve their
        # opcode/width guard here; branch target identity is checked
        # separately against the canonical ROM below.
        if rom_encoded != encoded and not (
            (
                operation["controlFlowKind"] in {"conditional-branch", "unconditional-branch"}
                and len(encoded) == 2
                and encoded[:1] == rom_encoded[:1]
                and encoded[1] == 0
            )
            or (len(encoded) >= 4 and encoded[:2] == rom_encoded[:2] and not any(encoded[2:]))
        ):
            raise ValueError(f"map-event flag lifecycle H1/ROM byte drift: {cursor:#x}")
        cursor += len(encoded)
    if cursor != end_address:
        raise ValueError("map-event flag lifecycle H1 range closure drift")
    return end_address - start_address


def _branch_target(address: int, encoded: bytes) -> int:
    if len(encoded) == 2:
        displacement = int.from_bytes(encoded[1:], byteorder="big", signed=True)
        if displacement == 0:
            raise ValueError("map-event flag lifecycle short branch encoding drift")
        return address + 2 + displacement
    if len(encoded) == 4:
        return address + 2 + int.from_bytes(encoded[2:], byteorder="big", signed=True)
    raise ValueError("map-event flag lifecycle branch width drift")


def _guard_branch(
    operation: dict[str, Any], *, h1_rows: dict[int, tuple[bytes, str]], rom: bytes
) -> None:
    target = operation["target"]
    if operation["mnemonic"] not in {"bne", "beq"} or not isinstance(target, dict):
        raise ValueError("map-event flag lifecycle immediate branch shape drift")
    if len(operation["operandTexts"]) != 1:
        raise ValueError("map-event flag lifecycle immediate branch operand drift")
    encoded = h1_rows[operation["address"]][0]
    rom_encoded = rom[operation["address"] : operation["address"] + len(encoded)]
    if not rom_encoded or encoded[:1] != rom_encoded[:1]:
        raise ValueError("map-event flag lifecycle branch opcode drift")
    if _branch_target(operation["address"], rom_encoded) != target["effectiveTargetAddress"]:
        raise ValueError("map-event flag lifecycle branch target drift")
    expected_polarity = "not-equal" if operation["mnemonic"] == "bne" else "equal"
    if (
        operation["sourceMnemonic"] not in {"bne.s", "bne.w", "beq.s"}
        or operation["sourceMnemonic"].split(".", maxsplit=1)[0] != operation["mnemonic"]
        or expected_polarity not in {"not-equal", "equal"}
    ):
        raise ValueError("map-event flag lifecycle branch polarity drift")


def _guard_program(
    program: dict[str, Any],
    *,
    disasm: Path,
    h1_rows: dict[int, tuple[bytes, str]],
    symbol_addresses: dict[str, int],
    rom: bytes,
) -> list[tuple[int, int]]:
    lines = (disasm / program["sourcePath"]).read_text(encoding="utf-8").splitlines()
    for label in program["labels"]:
        source_line = label["sourceLine"]
        if not 1 <= source_line <= len(lines):
            raise ValueError("map-event flag lifecycle label source range drift")
        if lines[source_line - 1].split(";", maxsplit=1)[0].strip() != f"{label['symbol']}:":
            raise ValueError("map-event flag lifecycle label source identity drift")
        if symbol_addresses.get(label["symbol"]) != label["address"]:
            raise ValueError("map-event flag lifecycle label H1 identity drift")
    spans: list[tuple[int, int]] = []
    operations = program["operations"]
    for index, operation in enumerate(operations):
        _source_line(lines, operation)
        end_address = (
            operations[index + 1]["address"]
            if index + 1 < len(operations)
            else program["endAddressExclusive"]
        )
        spans.append(
            (
                operation["address"],
                _h1_span(operation, end_address=end_address, h1_rows=h1_rows, rom=rom),
            )
        )
        if operation["controlFlowKind"] == "conditional-branch":
            _guard_branch(operation, h1_rows=h1_rows, rom=rom)
    if sum(length for _address, length in spans) != program["encodedSpanBytes"]:
        raise ValueError("map-event flag lifecycle program encoded-span drift")
    return spans


def _accesses_for_program(
    category: str, program: dict[str, Any], *, h1_rows: dict[int, tuple[bytes, str]], rom: bytes
) -> list[dict[str, Any]]:
    accesses: list[dict[str, Any]] = []
    operations = program["operations"]
    for index, operation in enumerate(operations):
        source_macro = operation["sourceMnemonic"]
        access_kind = _ACCESS_KINDS.get(source_macro)
        if access_kind is None:
            continue
        if len(operation["operandTexts"]) != 1:
            raise ValueError("map-event flag lifecycle access operand count drift")
        item = {
            "accessOrder": len(accesses),
            "accessKind": access_kind,
            "sourceMacro": source_macro,
            "flagNumber": _number(operation["operandTexts"][0]),
            "sourceOrder": operation["sourceOrder"],
            "sourceLine": operation["sourceLine"],
            "address": operation["address"],
        }
        if access_kind == "read":
            if index + 1 == len(operations):
                raise ValueError("map-event flag lifecycle read missing immediate branch")
            branch = operations[index + 1]
            _guard_branch(branch, h1_rows=h1_rows, rom=rom)
            target = branch["target"]
            assert isinstance(target, dict)
            item["immediateBranch"] = {
                "sourceOrder": branch["sourceOrder"],
                "sourceLine": branch["sourceLine"],
                "address": branch["address"],
                "mnemonic": branch["mnemonic"],
                "sizeSuffix": branch["sizeSuffix"],
                "branchPolarity": "not-equal" if branch["mnemonic"] == "bne" else "equal",
                "targetSymbol": target["effectiveTargetSymbol"],
                "targetAddress": target["effectiveTargetAddress"],
                "fallthroughAddress": (
                    operations[index + 2]["address"]
                    if index + 2 < len(operations)
                    else program["endAddressExclusive"]
                ),
            }
        accesses.append(item)
    if accesses != sorted(accesses, key=lambda item: item["sourceOrder"]):
        raise ValueError("map-event flag lifecycle access source-order drift")
    return accesses


def _program_flow(category: str, program: dict[str, Any], context_order: int) -> dict[str, Any]:
    operations: list[dict[str, Any]] = []
    for operation in program["operations"]:
        row = {
            "sourceOrder": operation["sourceOrder"],
            "sourceLine": operation["sourceLine"],
            "address": operation["address"],
            "mnemonic": operation["mnemonic"],
            "sizeSuffix": operation["sizeSuffix"],
            "controlFlowKind": operation["controlFlowKind"],
        }
        if operation["controlFlowKind"] == "conditional-branch":
            target = operation["target"]
            assert isinstance(target, dict)
            row["targetSymbol"] = target["effectiveTargetSymbol"]
            row["targetAddress"] = target["effectiveTargetAddress"]
        operations.append(row)
    return {
        "contextOrder": context_order,
        "category": category,
        "programSymbol": program["canonicalSymbol"],
        "entryAddress": program["entryAddress"],
        "sourcePath": program["sourcePath"],
        "entrySourceLine": program["entrySourceLine"],
        "endAddressExclusive": program["endAddressExclusive"],
        "encodedSpanBytes": program["encodedSpanBytes"],
        "labels": [
            {
                "sourceOrder": label["sourceOrder"],
                "sourceLine": label["sourceLine"],
                "symbol": label["symbol"],
                "address": label["address"],
            }
            for label in program["labels"]
        ],
        "operations": operations,
    }


def _intervals(spans: dict[int, dict[str, Any]]) -> list[dict[str, int]]:
    intervals: list[dict[str, int]] = []
    for address, row in sorted(spans.items()):
        end = address + row["h1ByteLength"]
        if intervals and address <= intervals[-1]["endAddressExclusive"]:
            intervals[-1]["endAddressExclusive"] = max(intervals[-1]["endAddressExclusive"], end)
        else:
            intervals.append({"startAddress": address, "endAddressExclusive": end})
    return intervals


def _retained_owner(path: Path, verifier_path: str, output: dict[str, Any]) -> dict[str, str]:
    return {
        "fixtureId": output["id"],
        "fixtureSha256": _fixture_sha256(path),
        "verifierPath": verifier_path,
        "verifierSha256": _fixture_sha256(repo_path(verifier_path)),
        "semanticSha256": _sha(canonical_json_bytes(output)),
    }


def _fresh_retained_owners(
    rom_path: Path, upstream_path: Path
) -> tuple[dict[str, Any], dict[str, Any]]:
    """Fresh-build all retained static owners before this narrower projection."""
    map_events = build_map_events_contract(rom_path, upstream_path)
    map_events_fixture = load_map_events_fixture()
    if map_events_fixture["expected"] != map_events:
        raise ValueError("map-event flag lifecycle retained map-events fixture drift")
    if (
        _sha(_map_events_canonical_bytes(map_events))
        != load_json(MAP_EVENTS_MANIFEST)["outputSha256"]
    ):
        raise ValueError("map-event flag lifecycle retained map-events semantic drift")
    # These are retained owner joins, rather than copied caller corpora.  Each
    # maintained builder independently guards its source/H1/ROM seam before
    # this narrower same-program projection consumes only its identity/hash.
    direct_state = build_map_event_direct_state_contract(rom_path, upstream_path)
    direct_control = build_map_event_direct_control_contract(rom_path, upstream_path)
    direct_handoff = build_map_event_direct_handoff_contract(rom_path, upstream_path)
    predicates = build_map_event_predicate_results_contract(rom_path, upstream_path)
    for path, name, output in (
        (DIRECT_STATE_FIXTURE, "direct-state", direct_state),
        (DIRECT_CONTROL_FIXTURE, "direct-control", direct_control),
        (DIRECT_HANDOFF_FIXTURE, "direct-handoff", direct_handoff),
        (PREDICATE_RESULTS_FIXTURE, "predicate-results", predicates),
    ):
        if load_json(path) != output:
            raise ValueError(f"map-event flag lifecycle retained {name} fixture drift")
    stats = load_json(STATS_FIXTURE)["expected"]
    interrupts = load_json(INTERRUPTS_FIXTURE)["expected"]
    direct_flags = load_json(MAP_EVENTS_DIRECT_FLAGS_FIXTURE)["expected"]
    if direct_flags != {
        key: map_events[key]
        for key in (
            "directFlagServiceDefinitions",
            "directFlagServiceDefinitionOrder",
            "directFlagAccessSites",
            "directFlagAccessSiteOrder",
            "directFlagProgramTotals",
            "directFlagProgramTotalOrder",
            "directFlagTotals",
            "directFlagTotalOrder",
            "directFlagStateSummary",
        )
    }:
        raise ValueError("map-event flag lifecycle retained direct-flags semantic drift")
    owners = {
        "mapEventsDirectFlags": {
            "fixtureId": "sf2-map-events-static-v1:direct-flags",
            "fixtureSha256": _fixture_sha256(MAP_EVENTS_DIRECT_FLAGS_FIXTURE),
            "verifierPath": "src/sf2tool/h2/map_events.py",
            "verifierSha256": _fixture_sha256(repo_path("src/sf2tool/h2/map_events.py")),
            "semanticSha256": _sha(canonical_json_bytes(direct_flags)),
        },
        "directState": _retained_owner(
            DIRECT_STATE_FIXTURE, "src/sf2tool/h2/map_event_direct_state.py", direct_state
        ),
        "directControl": _retained_owner(
            DIRECT_CONTROL_FIXTURE, "src/sf2tool/h2/map_event_direct_control.py", direct_control
        ),
        "directHandoff": _retained_owner(
            DIRECT_HANDOFF_FIXTURE, "src/sf2tool/h2/map_event_direct_handoff.py", direct_handoff
        ),
        "predicateResults": _retained_owner(
            PREDICATE_RESULTS_FIXTURE,
            "src/sf2tool/h2/map_event_predicate_results.py",
            predicates,
        ),
        "commonStatsFlags": {
            "fixtureId": load_json(STATS_FIXTURE)["id"],
            "fixtureSha256": _fixture_sha256(STATS_FIXTURE),
            "verifierPath": "src/sf2tool/h2/stats.py",
            "verifierSha256": _fixture_sha256(repo_path("src/sf2tool/h2/stats.py")),
            "semanticSha256": _sha(canonical_json_bytes(stats)),
        },
        "technicalInterruptTrapFlags": {
            "fixtureId": load_json(INTERRUPTS_FIXTURE)["id"],
            "fixtureSha256": _fixture_sha256(INTERRUPTS_FIXTURE),
            "verifierPath": "src/sf2tool/h2/interrupts.py",
            "verifierSha256": _fixture_sha256(repo_path("src/sf2tool/h2/interrupts.py")),
            "semanticSha256": _sha(canonical_json_bytes(interrupts)),
        },
    }
    if owners != _RETAINED_OWNER_EXPECTED:
        raise ValueError("map-event flag lifecycle retained owner identity/hash drift")
    return map_events, owners


def _project(
    map_events: dict[str, Any], owners: dict[str, Any], rom_path: Path, upstream_path: Path
) -> dict[str, Any]:
    disasm = _root(upstream_path)
    listing = (upstream_path / "build/sf2build-h1.lst").read_text(encoding="utf-8")
    h1_rows = _h1_instruction_rows(listing)
    symbol_addresses = listing_symbol_addresses(listing)
    rom = rom_path.read_bytes()
    if _sha(rom) != _ROM_SHA256:
        raise ValueError("map-event flag lifecycle ROM identity drift")
    if (
        map_events["upstream"]["commit"] != _UPSTREAM_COMMIT
        or map_events["romSha256"] != _ROM_SHA256
    ):
        raise ValueError("map-event flag lifecycle retained provenance drift")

    programs = [
        (category, program)
        for category in _CATEGORIES
        for program in map_events[_PROGRAM_FIELDS[category]]
    ]
    if (len(programs), sum(len(program["operations"]) for _category, program in programs)) != (
        914,
        3579,
    ):
        raise ValueError("map-event flag lifecycle mother corpus denominator drift")
    grouped: dict[tuple[str, str, int, int], list[dict[str, Any]]] = {}
    access_by_program: dict[tuple[str, str, int], list[dict[str, Any]]] = {}
    program_by_key = {
        (category, program["canonicalSymbol"], program["entryAddress"]): program
        for category, program in programs
    }
    for category, program in programs:
        key = (category, program["canonicalSymbol"], program["entryAddress"])
        accesses = _accesses_for_program(category, program, h1_rows=h1_rows, rom=rom)
        access_by_program[key] = accesses
        for access in accesses:
            grouped.setdefault((*key, access["flagNumber"]), []).append(access)
    relations_raw = [
        (key, accesses)
        for key, accesses in grouped.items()
        if any(row["accessKind"] == "read" for row in accesses)
        and any(row["accessKind"] in {"set", "clear"} for row in accesses)
    ]
    selected_program_keys = {key[:3] for key, _accesses in relations_raw}
    selected = [
        (category, program)
        for category, program in programs
        if (category, program["canonicalSymbol"], program["entryAddress"]) in selected_program_keys
    ]
    if len(relations_raw) != 131 or len(selected) != 117:
        raise ValueError("map-event flag lifecycle selection denominator drift")

    source_spans: dict[int, dict[str, Any]] = {}
    flows: list[dict[str, Any]] = []
    source_paths: dict[tuple[str, str], dict[str, Any]] = {}
    source_table_rows = {
        (category, row["path"]): row
        for category in _CATEGORIES
        for row in map_events["categories"][category]["sourceFiles"]
    }
    for context_order, (category, program) in enumerate(selected):
        spans = _guard_program(
            program,
            disasm=disasm,
            h1_rows=h1_rows,
            symbol_addresses=symbol_addresses,
            rom=rom,
        )
        flows.append(_program_flow(category, program, context_order))
        for address, length in spans:
            existing = source_spans.setdefault(
                address,
                {"address": address, "h1ByteLength": length, "contextOrders": []},
            )
            if existing["h1ByteLength"] != length:
                raise ValueError("map-event flag lifecycle physical alias byte-length drift")
            existing["contextOrders"].append(context_order)
        source_table = source_table_rows.get((category, program["sourcePath"]))
        if source_table is None:
            raise ValueError("map-event flag lifecycle selected source-table identity drift")
        source_paths[(category, program["sourcePath"])] = {
            "category": category,
            "sourcePath": program["sourcePath"],
            "tableSymbol": source_table["symbol"],
            "tableEntryAddress": source_table["address"],
        }

    relations: list[dict[str, Any]] = []
    for relation_order, ((category, symbol, entry, flag), accesses) in enumerate(relations_raw):
        program = program_by_key[(category, symbol, entry)]
        relations.append(
            {
                "relationOrder": relation_order,
                "category": category,
                "programSymbol": symbol,
                "programEntryAddress": entry,
                "sourcePath": program["sourcePath"],
                "flagNumber": flag,
                "accesses": accesses,
            }
        )
    relation_access_count = sum(len(row["accesses"]) for row in relations)
    sequence_counts = Counter(
        tuple(access["accessKind"] for access in relation["accesses"]) for relation in relations
    )
    if relation_access_count != 272 or dict(sequence_counts) != _LIFECYCLE_ACCESS_SEQUENCES:
        raise ValueError("map-event flag lifecycle access sequence denominator drift")

    category_relations = Counter(row["category"] for row in relations)
    category_programs = Counter(row["category"] for row in flows)
    operation_count = sum(len(row["operations"]) for row in flows)
    label_count = sum(len(row["labels"]) for row in flows)
    encoded_bytes = sum(row["encodedSpanBytes"] for row in flows)
    control_counts = Counter(
        operation["controlFlowKind"] for flow in flows for operation in flow["operations"]
    )
    selected_all_accesses = sum(
        len(access_by_program[(flow["category"], flow["programSymbol"], flow["entryAddress"])])
        for flow in flows
    )
    if (
        dict(category_relations) != {"entityEvents": 65, "zoneEvents": 62, "itemEvents": 4}
        or dict(category_programs) != {"entityEvents": 60, "zoneEvents": 53, "itemEvents": 4}
        or (operation_count, label_count, encoded_bytes, len(source_paths), selected_all_accesses)
        != (1177, 339, 4216, 67, 348)
        or dict(control_counts)
        != {
            "ordinary": 740,
            "conditional-branch": 207,
            "return": 119,
            "unconditional-branch": 69,
            "direct-call": 41,
            "direct-jump": 1,
        }
    ):
        raise ValueError("map-event flag lifecycle selected corpus denominator drift")

    intervals = _intervals(source_spans)
    physical_bytes = sum(row["h1ByteLength"] for row in source_spans.values())
    union_bytes = sum(row["endAddressExclusive"] - row["startAddress"] for row in intervals)
    if (
        len(source_spans),
        physical_bytes,
        len(intervals),
        union_bytes,
        encoded_bytes - union_bytes,
    ) != (
        1137,
        4066,
        79,
        4066,
        150,
    ):
        raise ValueError("map-event flag lifecycle physical interval denominator drift")

    flag_totals = []
    for flag in sorted({row["flagNumber"] for row in relations}):
        rows = [row for row in relations if row["flagNumber"] == flag]
        accesses = [access for row in rows for access in row["accesses"]]
        flag_totals.append(
            {
                "flagNumber": flag,
                "relationCount": len(rows),
                "accessCount": len(accesses),
                "readCount": sum(row["accessKind"] == "read" for row in accesses),
                "setCount": sum(row["accessKind"] == "set" for row in accesses),
                "clearCount": sum(row["accessKind"] == "clear" for row in accesses),
            }
        )
    if len(flag_totals) != 82:
        raise ValueError("map-event flag lifecycle numeric flag denominator drift")

    service_definitions: dict[str, dict[str, Any]] = {}
    macro_source = (disasm / "sf2macros.asm").read_text(encoding="utf-8").splitlines()
    for definition in map_events["directFlagServiceDefinitions"]:
        line = definition["definitionSourceLine"]
        if macro_source[line - 1].split(":", maxsplit=1)[0].strip() != definition["sourceMacro"]:
            raise ValueError("map-event flag lifecycle service definition source drift")
        emissions: list[str] = []
        for raw_line in macro_source[line:]:
            statement = _normalise_statement(raw_line).lower()
            if statement == "endm":
                break
            if statement:
                emissions.append(statement)
        if emissions != definition["emissionStatementTemplates"]:
            raise ValueError("map-event flag lifecycle service definition emission drift")
        service_definitions[definition["sourceMacro"]] = {
            "accessKind": definition["accessKind"],
            "sourceMacro": definition["sourceMacro"],
            "sourcePath": definition["sourcePath"],
            "definitionSourceLine": line,
            "trapOperand": definition["trapOperand"],
            "flagOperandOrdinal": definition["flagOperandOrdinal"],
            "emissionStatementTemplates": definition["emissionStatementTemplates"],
            "emissionStatementCount": len(definition["emissionStatementTemplates"]),
            "trapEntryAddress": symbol_addresses["Trap4_CheckFlag"]
            if definition["sourceMacro"] == "chkFlg"
            else None,
        }
    if tuple(service_definitions) != ("chkFlg", "setFlg", "clrFlg"):
        raise ValueError("map-event flag lifecycle service definition order drift")

    source_identity_paths = [
        "sf2macros.asm",
        "code/common/tech/interrupts/trap1-4_flags.asm",
    ] + sorted(row["sourcePath"] for row in source_paths.values())
    source_context = {
        "sourceIdentities": [
            {"path": path, "sha256": _sha((disasm / path).read_bytes())}
            for path in source_identity_paths
        ],
        "h1Listing": {
            "path": "build/sf2build-h1.lst",
            "sha256": _sha((upstream_path / "build/sf2build-h1.lst").read_bytes()),
        },
        "motherDenominators": {"programContextCount": 914, "operationCount": 3579},
        "selectedDenominators": {
            "programContextCount": 117,
            "lifecycleRelationCount": 131,
            "sourceFileCount": 67,
        },
    }
    source_file_rows = [source_paths[key] for key in sorted(source_paths)]
    facts = {
        "serviceDefinitions": service_definitions,
        "dispatchEntries": {
            "entityEvent": map_events["function"]["RunMapSetupEntityEvent"],
            "zoneEvent": map_events["function"]["RunMapSetupZoneEvent"],
            "itemEvent": map_events["function"]["RunMapSetupItemEvent"],
        },
        "selectionSummary": {
            "motherProgramContextCount": 914,
            "motherOperationCount": 3579,
            "positiveProgramContextCount": 117,
            "zeroProgramContextCount": 797,
            "lifecycleRelationCount": 131,
            "numericFlagCount": 82,
            "relationLocalAccessCount": 272,
            "selectedProgramAccessCount": 348,
            "categoryRelationCounts": {
                "entityEvents": 65,
                "zoneEvents": 62,
                "itemEvents": 4,
            },
            "accessSequenceCounts": [
                {"accessKinds": list(sequence), "relationCount": count}
                for sequence, count in _LIFECYCLE_ACCESS_SEQUENCES.items()
            ],
            "sourceFileOrder": [
                f"{row['category']}|{row['sourcePath']}" for row in source_file_rows
            ],
            "programFlowOrder": [
                f"{row['category']}|{row['programSymbol']}|{row['entryAddress']}" for row in flows
            ],
            "lifecycleRelationOrder": [
                f"{row['category']}|{row['programSymbol']}|{row['programEntryAddress']}|{row['flagNumber']}"
                for row in relations
            ],
        },
        "sourceFiles": source_file_rows,
        "programFlows": flows,
        "lifecycleRelations": relations,
        "flagTotals": flag_totals,
        "intervalCoverage": {
            "contextualOperationCount": operation_count,
            "physicalOperationCount": len(source_spans),
            "contextualLabelCount": label_count,
            "contextualEncodedByteCount": encoded_bytes,
            "physicalUniqueByteCount": union_bytes,
            "overlapByteCount": encoded_bytes - union_bytes,
            "physicalOperations": [source_spans[address] for address in sorted(source_spans)],
            "intervals": intervals,
        },
        "digests": {
            "programFlowSha256": _sha(canonical_json_bytes({"programFlows": flows})),
            "lifecycleRelationSha256": _sha(
                canonical_json_bytes({"lifecycleRelations": relations})
            ),
            "intervalCoverageSha256": _sha(canonical_json_bytes({"intervalCoverage": intervals})),
        },
    }
    summary = {
        "motherProgramContextCount": 914,
        "motherOperationCount": 3579,
        "positiveProgramContextCount": 117,
        "zeroProgramContextCount": 797,
        "lifecycleRelationCount": 131,
        "numericFlagCount": 82,
        "relationLocalAccessCount": 272,
        "readAccessCount": 135,
        "setAccessCount": 131,
        "clearAccessCount": 6,
        "selectedProgramAccessCount": 348,
        "sourceFileCount": 67,
        "contextualOperationCount": 1177,
        "physicalOperationCount": 1137,
        "contextualLabelCount": 339,
        "contextualEncodedByteCount": 4216,
        "physicalUniqueByteCount": 4066,
        "intervalCount": 79,
        "overlapByteCount": 150,
    }
    if Counter(
        access["accessKind"] for relation in relations for access in relation["accesses"]
    ) != Counter({"read": 135, "set": 131, "clear": 6}):
        raise ValueError("map-event flag lifecycle access-kind denominator drift")
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstreamCommit": _UPSTREAM_COMMIT,
        "romSha256": _ROM_SHA256,
        "sourceContext": source_context,
        "retainedOwners": owners,
        "flagLifecycleState": facts,
        "unknowns": {key: "Unknown" for key in _UNKNOWN_KEYS},
        "summary": summary,
    }


def _validate_order(value: dict[str, Any]) -> None:
    if tuple(value) != (
        "schemaVersion",
        "id",
        "upstreamCommit",
        "romSha256",
        "sourceContext",
        "retainedOwners",
        "flagLifecycleState",
        "unknowns",
        "summary",
    ):
        raise ValueError("map-event flag lifecycle root order drift")
    facts = value["flagLifecycleState"]
    if tuple(facts) != (
        "serviceDefinitions",
        "dispatchEntries",
        "selectionSummary",
        "sourceFiles",
        "programFlows",
        "lifecycleRelations",
        "flagTotals",
        "intervalCoverage",
        "digests",
    ):
        raise ValueError("map-event flag lifecycle facts order drift")
    if tuple(value["unknowns"]) != _UNKNOWN_KEYS or set(value["unknowns"].values()) != {"Unknown"}:
        raise ValueError("map-event flag lifecycle Unknown register drift")
    if [row["contextOrder"] for row in facts["programFlows"]] != list(range(117)):
        raise ValueError("map-event flag lifecycle program flow order drift")
    if [row["relationOrder"] for row in facts["lifecycleRelations"]] != list(range(131)):
        raise ValueError("map-event flag lifecycle relation order drift")
    access_sequences = Counter(
        tuple(access["accessKind"] for access in relation["accesses"])
        for relation in facts["lifecycleRelations"]
    )
    if dict(access_sequences) != _LIFECYCLE_ACCESS_SEQUENCES:
        raise ValueError("map-event flag lifecycle access sequence denominator drift")
    for relation in facts["lifecycleRelations"]:
        accesses = relation["accesses"]
        if [access["accessOrder"] for access in accesses] != sorted(
            access["accessOrder"] for access in accesses
        ) or [access["sourceOrder"] for access in accesses] != sorted(
            access["sourceOrder"] for access in accesses
        ):
            raise ValueError("map-event flag lifecycle relation access order drift")
    if [row["flagNumber"] for row in facts["flagTotals"]] != sorted(
        row["flagNumber"] for row in facts["flagTotals"]
    ):
        raise ValueError("map-event flag lifecycle flag total order drift")
    physical_rows = facts["intervalCoverage"]["physicalOperations"]
    expected_context_orders: dict[int, list[int]] = defaultdict(list)
    for flow in facts["programFlows"]:
        for operation in flow["operations"]:
            expected_context_orders[operation["address"]].append(flow["contextOrder"])
    if [row["address"] for row in physical_rows] != sorted(expected_context_orders) or any(
        row["contextOrders"] != expected_context_orders[row["address"]] for row in physical_rows
    ):
        raise ValueError("map-event flag lifecycle physical/context alias accounting drift")
    intervals = facts["intervalCoverage"]["intervals"]
    if intervals != _intervals({row["address"]: row for row in physical_rows}):
        raise ValueError("map-event flag lifecycle interval-union order drift")
    summary = facts["selectionSummary"]
    if summary["sourceFileOrder"] != [
        f"{row['category']}|{row['sourcePath']}" for row in facts["sourceFiles"]
    ]:
        raise ValueError("map-event flag lifecycle source-file order drift")
    if summary["programFlowOrder"] != [
        f"{row['category']}|{row['programSymbol']}|{row['entryAddress']}"
        for row in facts["programFlows"]
    ]:
        raise ValueError("map-event flag lifecycle program flow key order drift")
    if summary["lifecycleRelationOrder"] != [
        f"{row['category']}|{row['programSymbol']}|{row['programEntryAddress']}|{row['flagNumber']}"
        for row in facts["lifecycleRelations"]
    ]:
        raise ValueError("map-event flag lifecycle relation key order drift")


def build_map_event_flag_lifecycle_state_contract(
    rom_path: Path, upstream_path: Path
) -> dict[str, Any]:
    """Fresh-build the retained owners, then emit the exact static projection."""
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    map_events, owners = _fresh_retained_owners(rom_path, upstream_path)
    output = _project(map_events, owners, rom_path, upstream_path)
    _validate_order(output)
    return output


def verify_map_event_flag_lifecycle_state_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner=str(FIXTURE))
    _validate_order(fixture)
    output = build_map_event_flag_lifecycle_state_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="map-event flag lifecycle static contract")
    if fixture != output:
        raise ValueError("map-event flag lifecycle complete semantic fixture drift")
    destination = output_path or repo_path(
        "local/derived/map-event-flag-lifecycle-state-static.json"
    )
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(
        json.dumps(output, ensure_ascii=False, indent=2).encode("utf-8") + b"\n"
    )
    return {
        "Contract": ID,
        "SHA256": _sha(canonical_json_bytes(output)),
        "Relations": output["summary"]["lifecycleRelationCount"],
        "Status": "PASS",
        "Output": display_path(destination),
    }


def _remove_map_event_flag_lifecycle_state_later_owner_index_delta(
    index: dict[str, Any],
) -> dict[str, Any]:
    """Remove only this four-record index delta before predecessor normalization."""
    normalized = deepcopy(index)
    records = normalized.get("records")
    if not isinstance(records, list) or len({row.get("id") for row in records}) != len(records):
        raise ValueError("map-event flag lifecycle index record shape drift")
    bindings = {
        "map.setup.entity-event": [("entry", "flagLifecycleState.dispatchEntries.entityEvent")],
        "map.setup.zone-event": [("entry", "flagLifecycleState.dispatchEntries.zoneEvent")],
        "map.setup.item-event": [("entry", "flagLifecycleState.dispatchEntries.itemEvent")],
        "tech.interrupts.trap-flags": [
            ("entry", "flagLifecycleState.serviceDefinitions.chkFlg.trapEntryAddress")
        ],
    }
    document = "docs/research/map-event-flag-lifecycle-state.md"
    seen: set[str] = set()
    for record in records:
        record_id = record.get("id")
        expected_bindings = bindings.get(record_id)
        if expected_bindings is None:
            continue
        expected_evidence = {
            "level": "H2",
            "fixture": "tests/fixtures/h2/map-event-flag-lifecycle-state-static-v1.json",
            "fixtureId": ID,
            "verifier": "src/sf2tool/h2/map_event_flag_lifecycle_state.py",
            "bindings": [
                {"addressId": address_id, "fixtureField": field}
                for address_id, field in expected_bindings
            ],
        }
        evidence = record.get("evidence")
        documents = record.get("documents")
        matches = (
            [row for row in evidence if row.get("fixtureId") == ID]
            if isinstance(evidence, list)
            else []
        )
        if (
            matches != [expected_evidence]
            or not isinstance(documents, list)
            or documents.count(document) != 1
            or documents[-1] != document
        ):
            raise ValueError("map-event flag lifecycle index delta drift")
        evidence.remove(expected_evidence)
        documents.remove(document)
        seen.add(record_id)
    if seen != set(bindings):
        raise ValueError("map-event flag lifecycle index coverage drift")
    if _sha(canonical_json_bytes(normalized)) != _PREDECESSOR_INDEX_SHA256:
        raise ValueError("map-event flag lifecycle predecessor index drift")
    return normalized


def normalize_map_event_flag_lifecycle_state_later_owner_index(
    index: dict[str, Any],
) -> dict[str, Any]:
    """Strictly normalize the current index through this owner's predecessor."""
    from sf2tool.research_index import normalize_current_index_to_owner_predecessor

    return normalize_current_index_to_owner_predecessor(
        index, owner_id=ID
    )
