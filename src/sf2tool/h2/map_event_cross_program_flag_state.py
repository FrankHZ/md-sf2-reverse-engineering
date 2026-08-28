"""Public H2 writer-to-reader candidates for direct map-event flag accesses.

This rail starts with the complete direct-flag corpus and the later same-program
lifecycle owner.  It records a writer program and a reader program sharing one
numeric flag only as a static candidate: source order across programs, actual
values, persistence, and natural reachability remain outside this H2 result.
"""

from __future__ import annotations

import json
from collections import Counter, defaultdict
from copy import deepcopy
from pathlib import Path
from typing import Any

from sf2tool.h2.map_event_flag_lifecycle_state import (
    FIXTURE as LIFECYCLE_FIXTURE,
)
from sf2tool.h2.map_event_flag_lifecycle_state import (
    SCHEMA as LIFECYCLE_SCHEMA,
)
from sf2tool.h2.map_event_flag_lifecycle_state import (
    _h1_instruction_rows,
    _h1_span,
    _normalise_statement,
    _root,
    _sha,
    _source_line,
    canonical_json_bytes,
    normalize_map_event_flag_lifecycle_state_later_owner_index,
)
from sf2tool.h2.map_events_fixture import load_map_events_fixture
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path

ID = "sf2-map-event-cross-program-flag-state-static-v1"
FIXTURE = repo_path("tests/fixtures/h2/map-event-cross-program-flag-state-static-v1.json")
SCHEMA = repo_path("schemas/h2/map-event-cross-program-flag-state-static-fixture.schema.json")
MAP_EVENTS_FIXTURE = repo_path("tests/fixtures/h2/map-events-static-v1.json")
MAP_EVENTS_DIRECT_FLAGS_FIXTURE = repo_path("tests/fixtures/h2/map-events/direct-flags.json")
RESEARCH_INDEX = repo_path("manifests/research-index.json")

_UPSTREAM_COMMIT = "c834c652b6862bc5679fd7f69a38a7093206efc6"
_ROM_SHA256 = "9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9"
_PREDECESSOR_INDEX_SHA256 = "4D526EB33ED5A76D9D69D54E62FC6AB4B412603A16641928566A68200C7C656A"
_CATEGORIES = ("entityEvents", "zoneEvents", "itemEvents")
_PROGRAM_FIELDS = {
    "entityEvents": "entityTargetPrograms",
    "zoneEvents": "zoneTargetPrograms",
    "itemEvents": "itemTargetPrograms",
}
_ACCESS_KINDS = {"chkFlg": "read", "setFlg": "set", "clrFlg": "clear"}
_PAIR_ORDER = (
    ("entityEvents", "entityEvents"),
    ("entityEvents", "itemEvents"),
    ("entityEvents", "zoneEvents"),
    ("itemEvents", "entityEvents"),
    ("itemEvents", "zoneEvents"),
    ("zoneEvents", "entityEvents"),
    ("zoneEvents", "zoneEvents"),
)
_EXPECTED_PAIR_COUNTS = {
    "entityEvents->entityEvents": 374,
    "entityEvents->itemEvents": 2,
    "entityEvents->zoneEvents": 123,
    "itemEvents->entityEvents": 2,
    "itemEvents->zoneEvents": 1,
    "zoneEvents->entityEvents": 182,
    "zoneEvents->zoneEvents": 36,
}
_RETAINED_OWNER_EXPECTED = {
    "mapEventsDirectFlags": {
        "fixtureId": "sf2-map-events-static-v1:direct-flags",
        "fixtureSha256": "C05D38D53A3022C3EDEE6D1BFE16A44A25A99C3CF99F9EE63B08D8865F898366",
        "verifierPath": "src/sf2tool/h2/map_events.py",
        "verifierSha256": "5A5193DECF494292C679A17A51D10ADD481F62948803FC1CF6250FC05F32B5EE",
        "semanticSha256": "EF713A4AF6BE710780811E817EE27E5E5B0413CE8E0F84BA37731B57E3D6F389",
    },
    "flagLifecycleState": {
        "fixtureId": "sf2-map-event-flag-lifecycle-state-static-v1",
        "fixtureSha256": "6FFF8E2F4DF346F80C598A08FED71D93C930AC716B8AFA06B2C414668BBB5185",
        "verifierPath": "src/sf2tool/h2/map_event_flag_lifecycle_state.py",
        "verifierSha256": "0430A3735C6F3A4081B4921F0C8EE88D0B61BEBCACBBFBAB0570E6ADFE6DFADD",
        "semanticSha256": "764B26A40CEA1BCF9769384712716F52D74087D8D8A1CFA58348397CAED52BB6",
    },
}
_UNKNOWN_KEYS = (
    "naturalProgramReachability",
    "callerEntryFlagState",
    "actualFlagValueAtRead",
    "actualConditionalBranchSelection",
    "actualMutationReachability",
    "runtimeFlagValueAfterMutation",
    "producerConsumerTemporalOrder",
    "interveningFlagMutations",
    "mapSetupAndRecordSelection",
    "saveLoadAndCrossMapPersistence",
    "calleeScriptAndServiceEffects",
    "dialogueAudioPresentationAndStoryMeaning",
)


def _fixture_sha256(path: Path) -> str:
    return _sha(path.read_bytes())


def _retained_owner(path: Path, verifier_path: str, output: dict[str, Any]) -> dict[str, str]:
    return {
        "fixtureId": output["id"],
        "fixtureSha256": _fixture_sha256(path),
        "verifierPath": verifier_path,
        "verifierSha256": _fixture_sha256(repo_path(verifier_path)),
        "semanticSha256": _sha(canonical_json_bytes(output)),
    }


def _program_key(category: str, symbol: str, address: int) -> str:
    return f"{category}|{symbol}|{address}"


def _operation_statement(operation: dict[str, Any]) -> str:
    operands = operation["operandTexts"]
    return operation["sourceMnemonic"] + (" " + ",".join(operands) if operands else "")


def _guarded_retained_owners(
    rom_path: Path, upstream_path: Path
) -> tuple[dict[str, Any], dict[str, Any], dict[str, Any]]:
    """Validate retained fixture receipts before fresh assigned-surface guards.

    The retained map-events and lifecycle owners include wider private binary
    corpora outside this slice.  This rail consumes their public, hash-locked
    projections, then independently reads every assigned macro/map-event source
    identity plus its H1 and ROM anchors below.  It never reopens that wider
    input surface merely to recalculate another owner's complete corpus.
    """
    del rom_path, upstream_path
    lifecycle = load_json(LIFECYCLE_FIXTURE)
    validate_json(lifecycle, LIFECYCLE_SCHEMA, owner=str(LIFECYCLE_FIXTURE))
    from sf2tool.h2.map_event_flag_lifecycle_state import (
        _validate_order as _validate_lifecycle_order,
    )

    _validate_lifecycle_order(lifecycle)
    map_events_fixture = load_map_events_fixture()
    map_events = map_events_fixture["expected"]
    direct_flags = load_json(MAP_EVENTS_DIRECT_FLAGS_FIXTURE)["expected"]
    direct_flag_fields = (
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
    if direct_flags != {field: map_events[field] for field in direct_flag_fields}:
        raise ValueError("map-event cross-program flag state retained direct-flags semantic drift")
    owners = {
        "mapEventsDirectFlags": {
            "fixtureId": "sf2-map-events-static-v1:direct-flags",
            "fixtureSha256": _fixture_sha256(MAP_EVENTS_DIRECT_FLAGS_FIXTURE),
            "verifierPath": "src/sf2tool/h2/map_events.py",
            "verifierSha256": _fixture_sha256(repo_path("src/sf2tool/h2/map_events.py")),
            "semanticSha256": _sha(canonical_json_bytes(direct_flags)),
        },
        "flagLifecycleState": _retained_owner(
            LIFECYCLE_FIXTURE,
            "src/sf2tool/h2/map_event_flag_lifecycle_state.py",
            lifecycle,
        ),
    }
    if owners != _RETAINED_OWNER_EXPECTED:
        raise ValueError("map-event cross-program flag state retained owner identity/hash drift")
    return map_events, lifecycle, owners


def _guard_macro_definitions(map_events: dict[str, Any], *, disasm: Path) -> list[dict[str, Any]]:
    """Retain macro identities while source-validating their emitted trap forms."""
    macro_source = (disasm / "sf2macros.asm").read_text(encoding="utf-8").splitlines()
    rows: list[dict[str, Any]] = []
    for definition in map_events["directFlagServiceDefinitions"]:
        source_macro = definition["sourceMacro"]
        if _ACCESS_KINDS.get(source_macro) != definition["accessKind"]:
            raise ValueError("map-event cross-program flag state macro access-kind drift")
        line = definition["definitionSourceLine"]
        if macro_source[line - 1].split(":", maxsplit=1)[0].strip() != source_macro:
            raise ValueError("map-event cross-program flag state macro source identity drift")
        emissions: list[str] = []
        for raw_line in macro_source[line:]:
            statement = _normalise_statement(raw_line).lower()
            if statement == "endm":
                break
            if statement:
                emissions.append(statement)
        if emissions != definition["emissionStatementTemplates"]:
            raise ValueError("map-event cross-program flag state macro emission drift")
        rows.append(
            {
                "sourceMacro": source_macro,
                "accessKind": definition["accessKind"],
                "sourcePath": definition["sourcePath"],
                "definitionSourceLine": line,
                "trapOperand": definition["trapOperand"],
                "flagOperandOrdinal": definition["flagOperandOrdinal"],
                "emissionStatementCount": len(emissions),
            }
        )
    if [row["sourceMacro"] for row in rows] != ["chkFlg", "setFlg", "clrFlg"]:
        raise ValueError("map-event cross-program flag state macro definition order drift")
    return rows


def _programs_by_key(
    map_events: dict[str, Any],
) -> tuple[dict[tuple[str, str, int], dict[str, Any]], list[dict[str, Any]]]:
    """Build the complete mother program domain, preserving category source order."""
    expected = {
        "entityEvents": (684, 2624),
        "zoneEvents": (150, 809),
        "itemEvents": (80, 146),
    }
    programs: dict[tuple[str, str, int], dict[str, Any]] = {}
    domain: list[dict[str, Any]] = []
    context_order = 0
    for category in _CATEGORIES:
        rows = map_events[_PROGRAM_FIELDS[category]]
        if (len(rows), sum(len(row["operations"]) for row in rows)) != expected[category]:
            raise ValueError("map-event cross-program flag state mother corpus denominator drift")
        for program in rows:
            key = (category, program["canonicalSymbol"], program["entryAddress"])
            if key in programs:
                raise ValueError("map-event cross-program flag state duplicate program identity")
            programs[key] = program
            domain.append(
                {
                    "contextOrder": context_order,
                    "category": category,
                    "programSymbol": program["canonicalSymbol"],
                    "programEntryAddress": program["entryAddress"],
                    "programKey": _program_key(*key),
                    "sourcePath": program["sourcePath"],
                }
            )
            context_order += 1
    if (len(programs), sum(len(row["operations"]) for row in programs.values())) != (914, 3579):
        raise ValueError("map-event cross-program flag state mother corpus total drift")
    return programs, domain


def _site_public(site: dict[str, Any]) -> dict[str, Any]:
    """Expose structural source identities while omitting source or ROM payload."""
    row = {
        "siteOrder": site["siteOrder"],
        "category": site["category"],
        "programSymbol": site["programCanonicalSymbol"],
        "programEntryAddress": site["programEntryAddress"],
        "programKey": _program_key(
            site["category"], site["programCanonicalSymbol"], site["programEntryAddress"]
        ),
        "sourcePath": site["sourcePath"],
        "flagNumber": site["flagNumber"],
        "accessKind": site["accessKind"],
        "sourceMacro": site["sourceMacro"],
        "operationSourceOrder": site["operationSourceOrder"],
        "sourceLine": site["sourceLine"],
        "address": site["address"],
    }
    consumer = site["conditionConsumer"]
    if consumer is not None:
        target = consumer["target"]
        row["immediateConditionConsumer"] = {
            "operationSourceOrder": consumer["operationSourceOrder"],
            "sourceLine": consumer["sourceLine"],
            "address": consumer["address"],
            "sourceMnemonic": consumer["sourceMnemonic"],
            "mnemonic": consumer["mnemonic"],
            "sizeSuffix": consumer["sizeSuffix"],
            "branchPolarity": consumer["branchPolarity"],
            "targetSymbol": target["effectiveTargetSymbol"],
            "targetAddress": target["effectiveTargetAddress"],
        }
    return row


def _guard_sites_and_coverage(
    map_events: dict[str, Any],
    *,
    disasm: Path,
    listing: str,
    rom: bytes,
) -> tuple[list[dict[str, Any]], list[dict[str, Any]], dict[str, Any]]:
    """Guard every assigned direct access and its immediate reader consumer."""
    h1_rows = _h1_instruction_rows(listing)
    programs, _domain = _programs_by_key(map_events)
    source_lines: dict[str, list[str]] = {}
    anchors: dict[int, dict[str, Any]] = {}
    readers: list[dict[str, Any]] = []
    writers: list[dict[str, Any]] = []

    def add_anchor(address: int, length: int, role_key: str) -> None:
        row = anchors.setdefault(
            address,
            {"address": address, "h1ByteLength": length, "contextRoleKeys": []},
        )
        if row["h1ByteLength"] != length:
            raise ValueError("map-event cross-program flag state physical anchor byte drift")
        row["contextRoleKeys"].append(role_key)

    sites = map_events["directFlagAccessSites"]
    if [site["siteOrder"] for site in sites] != list(range(len(sites))):
        raise ValueError("map-event cross-program flag state direct access order drift")
    for site in sites:
        key = (
            site["category"],
            site["programCanonicalSymbol"],
            site["programEntryAddress"],
        )
        program = programs.get(key)
        if program is None:
            raise ValueError("map-event cross-program flag state direct access program drift")
        operation_order = site["operationSourceOrder"]
        operations = program["operations"]
        if not 0 <= operation_order < len(operations):
            raise ValueError(
                "map-event cross-program flag state direct access operation order drift"
            )
        operation = operations[operation_order]
        if (
            operation["sourceMnemonic"] != site["sourceMacro"]
            or _ACCESS_KINDS.get(operation["sourceMnemonic"]) != site["accessKind"]
            or operation["operandTexts"] != [site["flagOperandText"]]
            or operation["address"] != site["address"]
            or operation["sourceLine"] != site["sourceLine"]
        ):
            raise ValueError("map-event cross-program flag state direct access identity drift")
        if source_lines.get(program["sourcePath"]) is None:
            source_lines[program["sourcePath"]] = (
                (disasm / program["sourcePath"]).read_text(encoding="utf-8").splitlines()
            )
        _source_line(source_lines[program["sourcePath"]], operation)
        if operation_order + 1 >= len(operations):
            raise ValueError("map-event cross-program flag state direct access terminal drift")
        access_length = _h1_span(
            operation,
            end_address=operations[operation_order + 1]["address"],
            h1_rows=h1_rows,
            rom=rom,
        )
        public = _site_public(site)
        add_anchor(site["address"], access_length, f"{site['siteOrder']}|direct-flag")
        if site["accessKind"] == "read":
            consumer = site["conditionConsumer"]
            next_operation = operations[operation_order + 1]
            if consumer is None or (
                consumer["relation"] != "immediate-next-operation"
                or consumer["operationSourceOrder"] != operation_order + 1
                or consumer["address"] != next_operation["address"]
                or consumer["sourceMnemonic"] != next_operation["sourceMnemonic"]
                or consumer["mnemonic"] != next_operation["mnemonic"]
                or consumer["sizeSuffix"] != next_operation["sizeSuffix"]
            ):
                raise ValueError("map-event cross-program flag state read consumer drift")
            _source_line(source_lines[program["sourcePath"]], next_operation)
            target = next_operation.get("target")
            if not isinstance(target, dict) or (
                consumer["branchPolarity"]
                != ("not-equal" if next_operation["mnemonic"] == "bne" else "equal")
                or consumer["target"]["effectiveTargetSymbol"] != target["effectiveTargetSymbol"]
                or consumer["target"]["effectiveTargetAddress"] != target["effectiveTargetAddress"]
            ):
                raise ValueError(
                    "map-event cross-program flag state consumer polarity/target drift"
                )
            consumer_end = (
                operations[operation_order + 2]["address"]
                if operation_order + 2 < len(operations)
                else program["endAddressExclusive"]
            )
            consumer_length = _h1_span(
                next_operation,
                end_address=consumer_end,
                h1_rows=h1_rows,
                rom=rom,
            )
            add_anchor(
                next_operation["address"],
                consumer_length,
                f"{site['siteOrder']}|immediate-consumer",
            )
            readers.append(public)
        else:
            if site["conditionConsumer"] is not None:
                raise ValueError("map-event cross-program flag state writer consumer drift")
            writers.append(public)

    if (
        len(sites),
        len({site["address"] for site in sites}),
        len(readers),
        len({site["address"] for site in readers}),
        len(writers),
        len({site["address"] for site in writers}),
    ) != (493, 490, 316, 314, 177, 176):
        raise ValueError("map-event cross-program flag state access denominator drift")
    if Counter(site["accessKind"] for site in writers) != Counter({"set": 169, "clear": 8}):
        raise ValueError("map-event cross-program flag state writer access-kind denominator drift")
    if (
        len(anchors),
        sum(len(row["contextRoleKeys"]) for row in anchors.values()),
        sum(row["h1ByteLength"] for row in anchors.values()),
        sum(row["h1ByteLength"] * len(row["contextRoleKeys"]) for row in anchors.values()),
    ) != (804, 809, 2592, 2610):
        raise ValueError("map-event cross-program flag state anchor coverage drift")
    coverage = {
        "contextualAccessSiteCount": len(sites),
        "physicalAccessSiteCount": len({site["address"] for site in sites}),
        "contextualReadAccessSiteCount": len(readers),
        "physicalReadAccessSiteCount": len({site["address"] for site in readers}),
        "contextualSetAccessSiteCount": sum(site["accessKind"] == "set" for site in writers),
        "physicalSetAccessSiteCount": len(
            {site["address"] for site in writers if site["accessKind"] == "set"}
        ),
        "contextualClearAccessSiteCount": sum(site["accessKind"] == "clear" for site in writers),
        "physicalClearAccessSiteCount": len(
            {site["address"] for site in writers if site["accessKind"] == "clear"}
        ),
        "contextualImmediateReadConsumerCount": len(readers),
        "physicalImmediateReadConsumerCount": len(
            {site["immediateConditionConsumer"]["address"] for site in readers}
        ),
        "contextualAnchorPcCount": sum(len(row["contextRoleKeys"]) for row in anchors.values()),
        "physicalAnchorPcCount": len(anchors),
        "contextualAnchorEncodedByteCount": sum(
            row["h1ByteLength"] * len(row["contextRoleKeys"]) for row in anchors.values()
        ),
        "physicalAnchorByteCount": sum(row["h1ByteLength"] for row in anchors.values()),
        "overlapAnchorByteCount": sum(
            row["h1ByteLength"] * (len(row["contextRoleKeys"]) - 1) for row in anchors.values()
        ),
        "physicalAnchors": [anchors[address] for address in sorted(anchors)],
    }
    return readers, writers, coverage


def _cohorts(accesses: list[dict[str, Any]], *, role: str) -> list[dict[str, Any]]:
    grouped: dict[tuple[str, str, int, int], list[dict[str, Any]]] = defaultdict(list)
    for access in accesses:
        grouped[
            (
                access["category"],
                access["programSymbol"],
                access["programEntryAddress"],
                access["flagNumber"],
            )
        ].append(access)
    rows: list[dict[str, Any]] = []
    for cohort_order, key in enumerate(sorted(grouped)):
        values = grouped[key]
        if [value["siteOrder"] for value in values] != sorted(
            value["siteOrder"] for value in values
        ):
            raise ValueError("map-event cross-program flag state cohort site order drift")
        category, symbol, entry, flag = key
        rows.append(
            {
                "cohortOrder": cohort_order,
                "category": category,
                "programSymbol": symbol,
                "programEntryAddress": entry,
                "programKey": _program_key(category, symbol, entry),
                "sourcePath": values[0]["sourcePath"],
                "flagNumber": flag,
                "accessSiteOrders": [value["siteOrder"] for value in values],
                "accessKinds": [value["accessKind"] for value in values],
            }
        )
    if role == "reader" and (len(rows), sum(len(row["accessSiteOrders"]) for row in rows)) != (
        310,
        316,
    ):
        raise ValueError("map-event cross-program flag state reader cohort denominator drift")
    if role == "writer" and (len(rows), sum(len(row["accessSiteOrders"]) for row in rows)) != (
        171,
        177,
    ):
        raise ValueError("map-event cross-program flag state writer cohort denominator drift")
    return rows


def _cross_program_candidates(
    readers: list[dict[str, Any]], writers: list[dict[str, Any]]
) -> tuple[list[dict[str, Any]], list[dict[str, Any]], dict[str, Any]]:
    reader_cohorts = _cohorts(readers, role="reader")
    writer_cohorts = _cohorts(writers, role="writer")
    reader_by_key = {
        (row["category"], row["programSymbol"], row["programEntryAddress"], row["flagNumber"]): row
        for row in reader_cohorts
    }
    writer_by_key = {
        (row["category"], row["programSymbol"], row["programEntryAddress"], row["flagNumber"]): row
        for row in writer_cohorts
    }
    candidates: list[dict[str, Any]] = []
    for writer_key, writer in sorted(writer_by_key.items()):
        for reader_key, reader in sorted(reader_by_key.items()):
            if writer_key[3] != reader_key[3]:
                continue
            if writer_key[:3] == reader_key[:3]:
                continue
            candidates.append(
                {
                    "edgeOrder": len(candidates),
                    "flagNumber": writer_key[3],
                    "writerCohortOrder": writer["cohortOrder"],
                    "readerCohortOrder": reader["cohortOrder"],
                }
            )
    if len(candidates) != 720 or len(
        {
            (row["flagNumber"], row["writerCohortOrder"], row["readerCohortOrder"])
            for row in candidates
        }
    ) != len(candidates):
        raise ValueError("map-event cross-program flag state candidate uniqueness drift")
    pairs = Counter(
        f"{writer_cohorts[row['writerCohortOrder']]['category']}->"
        f"{reader_cohorts[row['readerCohortOrder']]['category']}"
        for row in candidates
    )
    if dict(pairs) != _EXPECTED_PAIR_COUNTS:
        raise ValueError("map-event cross-program flag state category-pair drift")
    totals = [
        {
            "writerCategory": writer_category,
            "readerCategory": reader_category,
            "candidateCount": pairs[f"{writer_category}->{reader_category}"],
        }
        for writer_category, reader_category in _PAIR_ORDER
    ]
    if any(
        writer_cohorts[row["writerCohortOrder"]]["category"]
        == reader_cohorts[row["readerCohortOrder"]]["category"]
        and writer_cohorts[row["writerCohortOrder"]]["programKey"]
        == reader_cohorts[row["readerCohortOrder"]]["programKey"]
        for row in candidates
    ):
        raise ValueError("map-event cross-program flag state self-edge leakage")
    return reader_cohorts, writer_cohorts, {"candidates": candidates, "categoryPairTotals": totals}


def _partitions(
    readers: list[dict[str, Any]],
    writers: list[dict[str, Any]],
    candidates: list[dict[str, Any]],
    reader_cohorts: list[dict[str, Any]],
    writer_cohorts: list[dict[str, Any]],
    lifecycle: dict[str, Any],
) -> dict[str, Any]:
    read_flags = {row["flagNumber"] for row in readers}
    written_flags = {row["flagNumber"] for row in writers}
    all_flags = read_flags | written_flags
    lifecycle_facts = lifecycle["flagLifecycleState"]
    lifecycle_relations = lifecycle_facts["lifecycleRelations"]
    direct_same_program = {
        (
            row["category"],
            row["programSymbol"],
            row["programEntryAddress"],
            row["flagNumber"],
        )
        for row in readers
    } & {
        (
            row["category"],
            row["programSymbol"],
            row["programEntryAddress"],
            row["flagNumber"],
        )
        for row in writers
    }
    retained_same_program = {
        (row["category"], row["programSymbol"], row["programEntryAddress"], row["flagNumber"])
        for row in lifecycle_relations
    }
    if direct_same_program != retained_same_program or len(retained_same_program) != 131:
        raise ValueError("map-event cross-program flag state lifecycle join drift")
    same_flags = {row[3] for row in retained_same_program}
    cross_flags = {row["flagNumber"] for row in candidates}
    reader_programs = {
        (row["category"], row["programSymbol"], row["programEntryAddress"])
        for row in reader_cohorts
    }
    writer_programs = {
        (row["category"], row["programSymbol"], row["programEntryAddress"])
        for row in writer_cohorts
    }
    if (
        len(all_flags),
        len(read_flags),
        len(written_flags),
        len(read_flags & written_flags),
        len(reader_programs),
        len(writer_programs),
        len(reader_programs | writer_programs),
        len(same_flags),
        len(cross_flags),
        len(same_flags - cross_flags),
        len(same_flags & cross_flags),
        len(cross_flags - same_flags),
    ) != (151, 128, 114, 91, 190, 135, 195, 82, 49, 42, 40, 9):
        raise ValueError("map-event cross-program flag state domain denominator drift")
    return {
        "programContextCount": 195,
        "readerProgramContextCount": 190,
        "writerProgramContextCount": 135,
        "flagDomains": {
            "all": sorted(all_flags),
            "read": sorted(read_flags),
            "written": sorted(written_flags),
            "readWriteOverlap": sorted(read_flags & written_flags),
            "readOnly": sorted(read_flags - written_flags),
            "writeOnly": sorted(written_flags - read_flags),
            "sameProgramLifecycle": sorted(same_flags),
            "crossProgram": sorted(cross_flags),
            "sameProgramOnly": sorted(same_flags - cross_flags),
            "bothSameAndCrossProgram": sorted(same_flags & cross_flags),
            "crossProgramOnly": sorted(cross_flags - same_flags),
        },
        "sameProgramLifecycleJoin": {
            "lifecycleRelationCount": len(retained_same_program),
            "lifecycleFlagCount": len(same_flags),
            "retainedRelationDigest": _sha(
                canonical_json_bytes({"lifecycleRelations": lifecycle_relations})
            ),
        },
    }


def _project(
    map_events: dict[str, Any],
    lifecycle: dict[str, Any],
    owners: dict[str, Any],
    rom_path: Path,
    upstream_path: Path,
) -> dict[str, Any]:
    disasm = _root(upstream_path)
    rom = rom_path.read_bytes()
    if _sha(rom) != _ROM_SHA256:
        raise ValueError("map-event cross-program flag state ROM identity drift")
    if (
        map_events["upstream"]["commit"] != _UPSTREAM_COMMIT
        or map_events["romSha256"] != _ROM_SHA256
        or lifecycle["upstreamCommit"] != _UPSTREAM_COMMIT
        or lifecycle["romSha256"] != _ROM_SHA256
    ):
        raise ValueError("map-event cross-program flag state retained provenance drift")

    macro_definitions = _guard_macro_definitions(map_events, disasm=disasm)
    programs, program_domain = _programs_by_key(map_events)
    readers, writers, coverage = _guard_sites_and_coverage(
        map_events,
        disasm=disasm,
        listing=(upstream_path / "build/sf2build-h1.lst").read_text(encoding="utf-8"),
        rom=rom,
    )
    reader_cohorts, writer_cohorts, candidate_facts = _cross_program_candidates(readers, writers)
    partitions = _partitions(
        readers,
        writers,
        candidate_facts["candidates"],
        reader_cohorts,
        writer_cohorts,
        lifecycle,
    )
    source_paths = sorted({row["sourcePath"] for row in readers + writers})
    if len(source_paths) != 90:
        raise ValueError("map-event cross-program flag state source surface drift")
    source_identity_paths = ["sf2macros.asm", *source_paths]
    source_context = {
        "sourceIdentities": [
            {"path": path, "sha256": _sha((disasm / path).read_bytes())}
            for path in source_identity_paths
        ],
        "h1Listing": {
            "path": "build/sf2build-h1.lst",
            "sha256": _sha((upstream_path / "build/sf2build-h1.lst").read_bytes()),
        },
        "motherDenominators": {
            "programContextCount": len(programs),
            "operationCount": sum(len(program["operations"]) for program in programs.values()),
        },
        "directFlagSourceSummary": {
            "mapEventSourceFileCount": len(source_paths),
            "sourceIdentityCount": len(source_identity_paths),
            "macroDefinitionCount": len(macro_definitions),
        },
    }
    facts = {
        "sourceMacroDefinitions": macro_definitions,
        "categoryRoles": {
            "entityEvents": lifecycle["flagLifecycleState"]["dispatchEntries"]["entityEvent"],
            "zoneEvents": lifecycle["flagLifecycleState"]["dispatchEntries"]["zoneEvent"],
            "itemEvents": lifecycle["flagLifecycleState"]["dispatchEntries"]["itemEvent"],
        },
        "serviceJoin": {
            "trapEntryAddress": lifecycle["flagLifecycleState"]["serviceDefinitions"]["chkFlg"][
                "trapEntryAddress"
            ]
        },
        "programDomain": program_domain,
        "readerAccessSites": readers,
        "writerAccessSites": writers,
        "readerCohorts": reader_cohorts,
        "writerCohorts": writer_cohorts,
        "partitions": partitions,
        "crossProgramCandidates": candidate_facts["candidates"],
        "categoryPairTotals": candidate_facts["categoryPairTotals"],
        "physicalContextCoverage": coverage,
        "digests": {
            "readerAccessSiteSha256": _sha(canonical_json_bytes({"readerAccessSites": readers})),
            "writerAccessSiteSha256": _sha(canonical_json_bytes({"writerAccessSites": writers})),
            "candidateSha256": _sha(
                canonical_json_bytes({"crossProgramCandidates": candidate_facts["candidates"]})
            ),
            "coverageSha256": _sha(canonical_json_bytes({"physicalContextCoverage": coverage})),
        },
    }
    summary = {
        "motherProgramContextCount": 914,
        "motherOperationCount": 3579,
        "sourceIdentityCount": 91,
        "mapEventSourceFileCount": 90,
        "contextualAccessSiteCount": coverage["contextualAccessSiteCount"],
        "physicalAccessSiteCount": coverage["physicalAccessSiteCount"],
        "contextualReadAccessSiteCount": coverage["contextualReadAccessSiteCount"],
        "physicalReadAccessSiteCount": coverage["physicalReadAccessSiteCount"],
        "contextualSetAccessSiteCount": coverage["contextualSetAccessSiteCount"],
        "physicalSetAccessSiteCount": coverage["physicalSetAccessSiteCount"],
        "contextualClearAccessSiteCount": coverage["contextualClearAccessSiteCount"],
        "physicalClearAccessSiteCount": coverage["physicalClearAccessSiteCount"],
        "contextualImmediateReadConsumerCount": coverage["contextualImmediateReadConsumerCount"],
        "physicalImmediateReadConsumerCount": coverage["physicalImmediateReadConsumerCount"],
        "contextualAnchorPcCount": coverage["contextualAnchorPcCount"],
        "physicalAnchorPcCount": coverage["physicalAnchorPcCount"],
        "contextualAnchorEncodedByteCount": coverage["contextualAnchorEncodedByteCount"],
        "physicalAnchorByteCount": coverage["physicalAnchorByteCount"],
        "overlapAnchorByteCount": coverage["overlapAnchorByteCount"],
        "programContextCount": partitions["programContextCount"],
        "readerProgramContextCount": partitions["readerProgramContextCount"],
        "writerProgramContextCount": partitions["writerProgramContextCount"],
        "numericFlagCount": len(partitions["flagDomains"]["all"]),
        "readFlagCount": len(partitions["flagDomains"]["read"]),
        "writtenFlagCount": len(partitions["flagDomains"]["written"]),
        "readWriteOverlapFlagCount": len(partitions["flagDomains"]["readWriteOverlap"]),
        "readOnlyFlagCount": len(partitions["flagDomains"]["readOnly"]),
        "writeOnlyFlagCount": len(partitions["flagDomains"]["writeOnly"]),
        "sameProgramLifecycleFlagCount": len(partitions["flagDomains"]["sameProgramLifecycle"]),
        "sameProgramLifecycleRelationCount": partitions["sameProgramLifecycleJoin"][
            "lifecycleRelationCount"
        ],
        "sameProgramOnlyFlagCount": len(partitions["flagDomains"]["sameProgramOnly"]),
        "bothSameAndCrossProgramFlagCount": len(
            partitions["flagDomains"]["bothSameAndCrossProgram"]
        ),
        "crossProgramOnlyFlagCount": len(partitions["flagDomains"]["crossProgramOnly"]),
        "crossProgramFlagCount": len(partitions["flagDomains"]["crossProgram"]),
        "crossProgramCandidateCount": len(candidate_facts["candidates"]),
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstreamCommit": _UPSTREAM_COMMIT,
        "romSha256": _ROM_SHA256,
        "sourceContext": source_context,
        "retainedOwners": owners,
        "crossProgramFlagState": facts,
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
        "crossProgramFlagState",
        "unknowns",
        "summary",
    ):
        raise ValueError("map-event cross-program flag state root order drift")
    facts = value["crossProgramFlagState"]
    if tuple(facts) != (
        "sourceMacroDefinitions",
        "categoryRoles",
        "serviceJoin",
        "programDomain",
        "readerAccessSites",
        "writerAccessSites",
        "readerCohorts",
        "writerCohorts",
        "partitions",
        "crossProgramCandidates",
        "categoryPairTotals",
        "physicalContextCoverage",
        "digests",
    ):
        raise ValueError("map-event cross-program flag state facts order drift")
    if tuple(value["unknowns"]) != _UNKNOWN_KEYS or set(value["unknowns"].values()) != {"Unknown"}:
        raise ValueError("map-event cross-program flag state Unknown register drift")
    if [row["contextOrder"] for row in facts["programDomain"]] != list(range(914)):
        raise ValueError("map-event cross-program flag state program domain order drift")
    for field, expected_count in (("readerAccessSites", 316), ("writerAccessSites", 177)):
        if [row["siteOrder"] for row in facts[field]] != sorted(
            row["siteOrder"] for row in facts[field]
        ) or len(facts[field]) != expected_count:
            raise ValueError("map-event cross-program flag state access cohort order drift")
    for field, expected_count in (("readerCohorts", 310), ("writerCohorts", 171)):
        if [row["cohortOrder"] for row in facts[field]] != list(range(expected_count)):
            raise ValueError("map-event cross-program flag state cohort order drift")
    candidates = facts["crossProgramCandidates"]
    if [row["edgeOrder"] for row in candidates] != list(range(720)):
        raise ValueError("map-event cross-program flag state candidate order drift")
    readers = facts["readerCohorts"]
    writers = facts["writerCohorts"]
    if any(
        row["flagNumber"] != writers[row["writerCohortOrder"]]["flagNumber"]
        or row["flagNumber"] != readers[row["readerCohortOrder"]]["flagNumber"]
        or writers[row["writerCohortOrder"]]["programKey"]
        == readers[row["readerCohortOrder"]]["programKey"]
        for row in candidates
    ):
        raise ValueError("map-event cross-program flag state candidate membership/self-edge drift")
    pair_counts = {
        f"{row['writerCategory']}->{row['readerCategory']}": row["candidateCount"]
        for row in facts["categoryPairTotals"]
    }
    if pair_counts != _EXPECTED_PAIR_COUNTS:
        raise ValueError("map-event cross-program flag state category-pair order drift")
    coverage = facts["physicalContextCoverage"]
    anchors = coverage["physicalAnchors"]
    if [row["address"] for row in anchors] != sorted(row["address"] for row in anchors):
        raise ValueError("map-event cross-program flag state physical anchor order drift")
    if coverage["physicalAnchorPcCount"] != len(anchors) or coverage[
        "contextualAnchorPcCount"
    ] != sum(len(row["contextRoleKeys"]) for row in anchors):
        raise ValueError("map-event cross-program flag state physical/context coverage drift")
    partitions = facts["partitions"]
    domains = partitions["flagDomains"]
    if any(
        values != sorted(values) or len(values) != len(set(values)) for values in domains.values()
    ):
        raise ValueError("map-event cross-program flag state flag domain order drift")
    if set(domains["sameProgramOnly"]) | set(domains["bothSameAndCrossProgram"]) != set(
        domains["sameProgramLifecycle"]
    ) or set(domains["crossProgramOnly"]) | set(domains["bothSameAndCrossProgram"]) != set(
        domains["crossProgram"]
    ):
        raise ValueError("map-event cross-program flag state partition drift")


def build_map_event_cross_program_flag_state_contract(
    rom_path: Path, upstream_path: Path
) -> dict[str, Any]:
    """Fresh-build retained owners and derive cross-program static candidates."""
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    map_events, lifecycle, owners = _guarded_retained_owners(rom_path, upstream_path)
    output = _project(map_events, lifecycle, owners, rom_path, upstream_path)
    _validate_order(output)
    return output


def verify_map_event_cross_program_flag_state_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner=str(FIXTURE))
    _validate_order(fixture)
    output = build_map_event_cross_program_flag_state_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="map-event cross-program flag state static contract")
    if fixture != output:
        raise ValueError("map-event cross-program flag state complete semantic fixture drift")
    destination = output_path or repo_path(
        "local/derived/map-event-cross-program-flag-state-static.json"
    )
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(
        json.dumps(output, ensure_ascii=False, indent=2).encode("utf-8") + b"\n"
    )
    return {
        "Contract": ID,
        "SHA256": _sha(canonical_json_bytes(output)),
        "Candidates": output["summary"]["crossProgramCandidateCount"],
        "Status": "PASS",
        "Output": display_path(destination),
    }


def _remove_map_event_cross_program_flag_state_later_owner_index_delta(
    index: dict[str, Any],
) -> dict[str, Any]:
    """Remove only this exact four-record delta before predecessor normalization."""
    normalized = deepcopy(index)
    records = normalized.get("records")
    if not isinstance(records, list) or len({row.get("id") for row in records}) != len(records):
        raise ValueError("map-event cross-program flag state index record shape drift")
    bindings = {
        "map.setup.entity-event": [("entry", "crossProgramFlagState.categoryRoles.entityEvents")],
        "map.setup.zone-event": [("entry", "crossProgramFlagState.categoryRoles.zoneEvents")],
        "map.setup.item-event": [("entry", "crossProgramFlagState.categoryRoles.itemEvents")],
        "tech.interrupts.trap-flags": [
            ("entry", "crossProgramFlagState.serviceJoin.trapEntryAddress")
        ],
    }
    document = "docs/research/map-event-cross-program-flag-state.md"
    seen: set[str] = set()
    for record in records:
        record_id = record.get("id")
        expected_bindings = bindings.get(record_id)
        if expected_bindings is None:
            continue
        expected_evidence = {
            "level": "H2",
            "fixture": "tests/fixtures/h2/map-event-cross-program-flag-state-static-v1.json",
            "fixtureId": ID,
            "verifier": "src/sf2tool/h2/map_event_cross_program_flag_state.py",
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
            raise ValueError("map-event cross-program flag state index delta drift")
        evidence.remove(expected_evidence)
        documents.remove(document)
        seen.add(record_id)
    if seen != set(bindings):
        raise ValueError("map-event cross-program flag state index coverage drift")
    if _sha(canonical_json_bytes(normalized)) != _PREDECESSOR_INDEX_SHA256:
        raise ValueError("map-event cross-program flag state predecessor index drift")
    return normalized


def normalize_map_event_cross_program_flag_state_later_owner_index(
    index: dict[str, Any],
) -> dict[str, Any]:
    """Strictly remove the newest delta, then delegate predecessor normalization."""
    return normalize_map_event_flag_lifecycle_state_later_owner_index(
        _remove_map_event_cross_program_flag_state_later_owner_index_delta(index)
    )
