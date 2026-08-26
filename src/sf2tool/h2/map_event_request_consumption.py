"""Public H2 request-consumption access contract for map-event request state.

The retained request-state owner establishes the complete producer-side surface.
This owner begins at the first named fixed-RAM consumer access in eight bounded
source contexts.  It neither enters a consumer callee nor infers that an actual
producer reaches any consumer.
"""

from __future__ import annotations

import hashlib
import re
from pathlib import Path
from typing import Any

from sf2tool.h2.map_event_direct_handoff import _h1_instruction_rows, _parse_equates
from sf2tool.h2.map_event_request_state import (
    FIXTURE as REQUEST_STATE_FIXTURE,
)
from sf2tool.h2.map_event_request_state import (
    build_map_event_request_state_contract,
    canonical_json_bytes,
)
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path

ID = "sf2-map-event-request-consumption-static-v1"
FIXTURE = repo_path("tests/fixtures/h2/map-event-request-consumption-static-v1.json")
SCHEMA = repo_path("schemas/h2/map-event-request-consumption-static-fixture.schema.json")

_ROM_SHA256 = "9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9"
_UPSTREAM_COMMIT = "c834c652b6862bc5679fd7f69a38a7093206efc6"
_SYMBOLS = (
    "CURRENT_SHOP_INDEX",
    "MAP_EVENT_TYPE",
    "EGRESS_MAP",
    "RAFT_MAP",
    "RAFT_X",
    "RAFT_Y",
)
_SOURCE_PATHS = (
    "sf2const.asm",
    "code/common/menus/shop/shopactions.asm",
    "code/gameflow/exploration/explorationfunctions_2.asm",
    "code/common/menus/main/mainactions.asm",
    "code/gameflow/battle/battleloop/getegresspositionforbattle.asm",
    "code/common/scripting/map/mapfunctions.asm",
    "code/common/scripting/map/followersfunctions_2.asm",
)
_UNKNOWN_KEYS = (
    "normalStoryProducerConsumerReachability",
    "selectedProducerAndDefinition",
    "actualConsumerEntryState",
    "actualConsumerReadValue",
    "actualMapEventPollAndClearTiming",
    "actualMapEventDispatchPath",
    "actualShopSelectionAndOutcome",
    "actualFieldEgressDestination",
    "actualBattleEgressFallbackDestination",
    "actualRaftPresenceAndCoordinates",
    "crossMapSaveLoadPersistence",
    "inputUiMapTransitionAudioStoryMeaning",
)


def _access(
    identifier: str,
    symbol: str,
    source_line: int,
    statement: str,
    rom_pc: int,
    width: str,
    role: str,
) -> dict[str, Any]:
    return {
        "id": identifier,
        "symbol": symbol,
        "sourceLine": source_line,
        "statement": statement,
        "romPc": rom_pc,
        "width": width,
        "role": role,
    }


def _control(source_line: int, statement: str, kind: str, target: str | None) -> dict[str, Any]:
    return {
        "sourceLine": source_line,
        "statement": statement,
        "kind": kind,
        "target": target,
    }


# Source statements are intentionally private parser guards.  The generated
# fixture keeps only neutral mnemonic/width/target identities, never source
# prose, operands, or source bytes.
_CONTEXTS = (
    {
        "id": "get-shop-inventory-address",
        "fixtureKey": "getShopInventoryAddress",
        "sourcePath": "code/common/menus/shop/shopactions.asm",
        "entrySymbol": "GetShopInventoryAddress",
        "entrySourceLine": 706,
        "entryAddress": 0x20852,
        "accesses": (
            _access(
                "get-shop-inventory-address/current-shop-index/read",
                "CURRENT_SHOP_INDEX",
                711,
                "move.b (CURRENT_SHOP_INDEX).l,d7",
                0x2085C,
                "b",
                "inventory-index-read",
            ),
        ),
        "controls": (_control(713, "bcs.w @Done", "conditional-branch", "@Done"),),
    },
    {
        "id": "exploration-loop",
        "fixtureKey": "explorationLoop",
        "sourcePath": "code/gameflow/exploration/explorationfunctions_2.asm",
        "entrySymbol": "ExplorationLoop",
        "entrySourceLine": 8,
        "entryAddress": 0x257C0,
        "accesses": (
            _access(
                "exploration-loop/map-event-type/reset",
                "MAP_EVENT_TYPE",
                10,
                "clr.w ((MAP_EVENT_TYPE-$1000000)).w",
                0x257C0,
                "w",
                "entry-reset",
            ),
        ),
        "controls": (),
    },
    {
        "id": "wait-for-event",
        "fixtureKey": "waitForEvent",
        "sourcePath": "code/gameflow/exploration/explorationfunctions_2.asm",
        "entrySymbol": "WaitForEvent",
        "entrySourceLine": 174,
        "entryAddress": 0x2591C,
        "accesses": (
            _access(
                "wait-for-event/map-event-type/entry-poll-read",
                "MAP_EVENT_TYPE",
                176,
                "move.w ((MAP_EVENT_TYPE-$1000000)).w,d0",
                0x2591C,
                "w",
                "entry-poll-read",
            ),
            _access(
                "wait-for-event/map-event-type/loop-poll-read",
                "MAP_EVENT_TYPE",
                185,
                "move.w ((MAP_EVENT_TYPE-$1000000)).w,d0",
                0x25934,
                "w",
                "loop-poll-read",
            ),
        ),
        "controls": (
            _control(177, "bne.s loc_25930", "conditional-branch", "loc_25930"),
            _control(186, "beq.s loc_2593C", "conditional-branch", "loc_2593C"),
        ),
    },
    {
        "id": "process-map-event",
        "fixtureKey": "processMapEvent",
        "sourcePath": "code/gameflow/exploration/explorationfunctions_2.asm",
        "entrySymbol": "ProcessMapEvent",
        "entrySourceLine": 204,
        "entryAddress": 0x2594A,
        "accesses": (
            _access(
                "process-map-event/map-event-type/pre-dispatch-clear",
                "MAP_EVENT_TYPE",
                206,
                "clr.w ((MAP_EVENT_TYPE-$1000000)).w",
                0x2594A,
                "w",
                "pre-dispatch-consume-clear",
            ),
        ),
        "controls": (
            _control(
                208,
                "beq.w ProcessMapEventType1_Warp",
                "conditional-branch",
                "ProcessMapEventType1_Warp",
            ),
            _control(
                210,
                "beq.w ProcessMapEventType2_GetIntoCaravan",
                "conditional-branch",
                "ProcessMapEventType2_GetIntoCaravan",
            ),
            _control(
                212,
                "beq.w ProcessMapEventType3_GetIntoRaft",
                "conditional-branch",
                "ProcessMapEventType3_GetIntoRaft",
            ),
            _control(
                214,
                "beq.w ProcessMapEventType4_GetOutOfCaravan",
                "conditional-branch",
                "ProcessMapEventType4_GetOutOfCaravan",
            ),
            _control(
                216,
                "beq.w ProcessMapEventType5_GetOutOfRaft",
                "conditional-branch",
                "ProcessMapEventType5_GetOutOfRaft",
            ),
            _control(
                218,
                "beq.w ProcessMapEventType6_ZoneEvent",
                "conditional-branch",
                "ProcessMapEventType6_ZoneEvent",
            ),
        ),
    },
    {
        "id": "field-menu",
        "fixtureKey": "fieldMenu",
        "sourcePath": "code/common/menus/main/mainactions.asm",
        "entrySymbol": "FieldMenu",
        "entrySourceLine": 18,
        "entryAddress": 0x2127E,
        "accesses": (
            _access(
                "field-menu/egress-map/read",
                "EGRESS_MAP",
                116,
                "move.b ((EGRESS_MAP-$1000000)).w,d0",
                0x21384,
                "b",
                "field-egress-read",
            ),
        ),
        "controls": (
            _control(105, "blt.s byte_21348", "conditional-branch", "byte_21348"),
            _control(107, "bgt.s byte_21348", "conditional-branch", "byte_21348"),
            _control(117, "jsr (GetSavepointForMap).w", "direct-call", "GetSavepointForMap"),
        ),
    },
    {
        "id": "get-egress-position-for-battle",
        "fixtureKey": "getEgressPositionForBattle",
        "sourcePath": "code/gameflow/battle/battleloop/getegresspositionforbattle.asm",
        "entrySymbol": "GetEgressPositionForBattle",
        "entrySourceLine": 10,
        "entryAddress": 0x23E50,
        "accesses": (
            _access(
                "get-egress-position-for-battle/egress-map/read",
                "EGRESS_MAP",
                54,
                "move.b ((EGRESS_MAP-$1000000)).w,d0",
                0x23EA6,
                "b",
                "battle-egress-fallback-read",
            ),
        ),
        "controls": (
            _control(49, "bne.s loc_23EA6", "conditional-branch", "loc_23EA6"),
            _control(57, "jsr (GetSavepointForMap).w", "direct-call", "GetSavepointForMap"),
        ),
    },
    {
        "id": "declare-raft-entity",
        "fixtureKey": "declareRaftEntity",
        "sourcePath": "code/common/scripting/map/mapfunctions.asm",
        "entrySymbol": "DeclareRaftEntity",
        "entrySourceLine": 122,
        "entryAddress": 0x441AA,
        "accesses": (
            _access(
                "declare-raft-entity/raft-map/read",
                "RAFT_MAP",
                157,
                "cmp.b ((RAFT_MAP-$1000000)).w,d0",
                0x441FC,
                "b",
                "raft-presence-map-read",
            ),
            _access(
                "declare-raft-entity/raft-x/read",
                "RAFT_X",
                161,
                "move.b ((RAFT_X-$1000000)).w,d1",
                0x44202,
                "b",
                "raft-coordinate-x-read",
            ),
            _access(
                "declare-raft-entity/raft-y/read",
                "RAFT_Y",
                162,
                "move.b ((RAFT_Y-$1000000)).w,d2",
                0x44206,
                "b",
                "raft-coordinate-y-read",
            ),
        ),
        "controls": (
            _control(153, "beq.w @Done", "conditional-branch", "@Done"),
            _control(158, "bne.s @RaftNotOnMap", "conditional-branch", "@RaftNotOnMap"),
            _control(175, "bsr.w DeclareNewEntity", "direct-call", "DeclareNewEntity"),
        ),
    },
    {
        "id": "raft-refresh",
        "fixtureKey": "raftRefresh",
        "sourcePath": "code/common/scripting/map/followersfunctions_2.asm",
        "entrySymbol": "sub_44404",
        "entrySourceLine": 61,
        "entryAddress": 0x44404,
        "accesses": (
            _access(
                "raft-refresh/raft-map/read",
                "RAFT_MAP",
                78,
                "cmp.b ((RAFT_MAP-$1000000)).w,d0",
                0x4442C,
                "b",
                "raft-presence-map-read",
            ),
            _access(
                "raft-refresh/raft-x/read",
                "RAFT_X",
                80,
                "move.b ((RAFT_X-$1000000)).w,d1",
                0x44434,
                "b",
                "raft-coordinate-x-read",
            ),
            _access(
                "raft-refresh/raft-y/read",
                "RAFT_Y",
                81,
                "move.b ((RAFT_Y-$1000000)).w,d2",
                0x44438,
                "b",
                "raft-coordinate-y-read",
            ),
        ),
        "controls": (
            _control(76, "beq.w return_4446A", "conditional-branch", "return_4446A"),
            _control(79, "bne.w return_4446A", "conditional-branch", "return_4446A"),
            _control(96, "bsr.w DeclareNewEntity", "direct-call", "DeclareNewEntity"),
        ),
    },
)

_RETAINED_OWNER_FIXTURES = {
    "mapEventRequestState": REQUEST_STATE_FIXTURE,
    **{
        owner: repo_path("tests") / "fixtures" / "h2" / f"{fixture_name}.json"
        for owner, fixture_name in {
            "commonMenus": "common-menus-static-v1",
            "gameflowCore": "gameflow-core-static-v1",
            "fieldMenuControl": "field-menu-control-static-v1",
            "battleLoop": "battle-loop-static-v1",
            "commonScripting": "common-scripting-static-v1",
            "commonMaps": "common-maps-static-v1",
        }.items()
    },
}


def _normalise(statement: str) -> str:
    return re.sub(r"\s*,\s*", ",", re.sub(r"\s+", " ", statement.split(";", 1)[0].strip()))


def _disasm_root(upstream_path: Path) -> Path:
    root = upstream_path.resolve(strict=True)
    return root / "disasm" if (root / "disasm").is_dir() else root


def _assert_source_line(lines: list[str], line_number: int, expected: str, context: str) -> None:
    in_range = 1 <= line_number <= len(lines)
    matches = in_range and _normalise(lines[line_number - 1]) == _normalise(expected)
    if not matches:
        raise ValueError(
            f"map-event request-consumption source mnemonic/operand-order drift: {context}"
        )


def _assert_label(lines: list[str], line_number: int, label: str, context: str) -> None:
    in_range = 1 <= line_number <= len(lines)
    matches = in_range and lines[line_number - 1].split(";", 1)[0].strip() == f"{label}:"
    if not matches:
        raise ValueError(f"map-event request-consumption source label drift: {context}")


def _anchor(
    *,
    rom_pc: int,
    roles: list[str],
    h1_rows: dict[int, tuple[bytes, str]],
    rom: bytes,
) -> dict[str, Any]:
    row = h1_rows.get(rom_pc)
    if row is None:
        raise ValueError(f"map-event request-consumption H1 anchor missing: {rom_pc:#x}")
    h1_bytes = row[0]
    rom_bytes = rom[rom_pc : rom_pc + len(h1_bytes)]
    if len(rom_bytes) != len(h1_bytes) or rom_bytes != h1_bytes:
        raise ValueError(f"map-event request-consumption H1/ROM anchor drift: {rom_pc:#x}")
    return {
        "id": f"pc:{rom_pc:06X}",
        "romPc": rom_pc,
        "roles": roles,
        "instructionByteLength": len(h1_bytes),
        "h1InstructionSha256": hashlib.sha256(h1_bytes).hexdigest().upper(),
        "romInstructionSha256": hashlib.sha256(rom_bytes).hexdigest().upper(),
    }


def _fresh_retained_owners(rom_path: Path, upstream_path: Path) -> dict[str, dict[str, str]]:
    """Rebuild each retained owner before narrowing its consumer join."""
    request_state = build_map_event_request_state_contract(rom_path, upstream_path)
    if request_state != load_json(REQUEST_STATE_FIXTURE):
        raise ValueError("map-event request-consumption retained request-state projection drift")

    # These independent owners cover the selected consumer contexts.  Their
    # public verifiers rebuild source facts and compare their accepted fixture
    # without importing a consumer callee algorithm into this slice.
    from sf2tool.h2.battle_loop import verify_battle_loop_inventory
    from sf2tool.h2.field_menu_control import verify_field_menu_control_static
    from sf2tool.h2.gameflow import verify_gameflow_inventory
    from sf2tool.h2.maps import verify_map_inventory
    from sf2tool.h2.menus import verify_menu_inventory
    from sf2tool.h2.scripting import verify_scripting_inventory

    verify_menu_inventory(upstream_path)
    verify_gameflow_inventory(upstream_path)
    verify_field_menu_control_static(rom_path, upstream_path)
    verify_battle_loop_inventory(upstream_path)
    verify_scripting_inventory(upstream_path)
    verify_map_inventory(upstream_path)

    owners: dict[str, dict[str, str]] = {}
    for name, fixture_path in _RETAINED_OWNER_FIXTURES.items():
        fixture = load_json(fixture_path)
        fixture_id = fixture.get("id")
        if not isinstance(fixture_id, str):
            raise ValueError(f"map-event request-consumption retained owner identity drift: {name}")
        owners[name] = {
            "fixtureId": fixture_id,
            "fixtureSha256": hashlib.sha256(fixture_path.read_bytes()).hexdigest().upper(),
        }
    if set(owners) != set(_RETAINED_OWNER_FIXTURES):
        raise ValueError("map-event request-consumption retained-owner key drift")
    return owners


def _controls(spec: dict[str, Any], lines: list[str]) -> list[dict[str, Any]]:
    output: list[dict[str, Any]] = []
    for control in spec["controls"]:
        _assert_source_line(
            lines,
            control["sourceLine"],
            control["statement"],
            f"{spec['id']}:{control['sourceLine']}",
        )
        mnemonic = control["statement"].split(maxsplit=1)[0]
        output.append(
            {
                "sourceLine": control["sourceLine"],
                "kind": control["kind"],
                "mnemonic": mnemonic,
                "target": control["target"],
            }
        )
    return output


def _projection(rom_path: Path, upstream_path: Path) -> tuple[dict[str, Any], dict[str, Any]]:
    disasm = _disasm_root(upstream_path)
    listing_path = upstream_path.resolve(strict=True) / "build/sf2build-h1.lst"
    listing_text = listing_path.read_text(encoding="utf-8")
    h1_rows = _h1_instruction_rows(listing_text)
    rom = rom_path.resolve(strict=True).read_bytes()
    if hashlib.sha256(rom).hexdigest().upper() != _ROM_SHA256:
        raise ValueError("map-event request-consumption ROM identity drift")

    source_text: dict[str, list[str]] = {}
    source_identities: list[dict[str, str]] = []
    for source_path in _SOURCE_PATHS:
        path = disasm / source_path
        payload = path.read_bytes()
        source_identities.append(
            {"path": source_path, "sha256": hashlib.sha256(payload).hexdigest().upper()}
        )
        source_text[source_path] = payload.decode("utf-8").splitlines()
    if len(source_identities) != 7:
        raise ValueError("map-event request-consumption source denominator drift")
    constants = _parse_equates("\n".join(source_text["sf2const.asm"]))
    if set(_SYMBOLS) - set(constants):
        raise ValueError("map-event request-consumption symbol-definition drift")

    access_sites: list[dict[str, Any]] = []
    consumer_contexts: dict[str, dict[str, Any]] = {}
    relations: list[dict[str, Any]] = []
    anchor_roles: dict[int, list[str]] = {}
    for context_order, spec in enumerate(_CONTEXTS):
        lines = source_text[spec["sourcePath"]]
        _assert_label(lines, spec["entrySourceLine"], spec["entrySymbol"], spec["id"])
        anchor_roles.setdefault(spec["entryAddress"], []).append(f"context-entry:{spec['id']}")
        context_access_ids: list[str] = []
        by_symbol: dict[str, list[str]] = {}
        for access in spec["accesses"]:
            _assert_source_line(
                lines, access["sourceLine"], access["statement"], f"{spec['id']}:{access['id']}"
            )
            row = h1_rows.get(access["romPc"])
            source_join_matches = row is not None and _normalise(row[1]) == _normalise(
                access["statement"]
            )
            if not source_join_matches:
                raise ValueError(
                    f"map-event request-consumption H1 source join drift: {access['id']}"
                )
            access_id = access["id"]
            context_access_ids.append(access_id)
            by_symbol.setdefault(access["symbol"], []).append(access_id)
            access_sites.append(
                {
                    "siteOrder": len(access_sites),
                    "id": access_id,
                    "contextId": spec["id"],
                    "symbol": access["symbol"],
                    "address": constants[access["symbol"]]["value"],
                    "accessKind": (
                        "read" if access["statement"].startswith(("move", "cmp")) else "clear"
                    ),
                    "width": access["width"],
                    "sourceLine": access["sourceLine"],
                    "romPc": access["romPc"],
                    "role": access["role"],
                }
            )
            anchor_roles.setdefault(access["romPc"], []).append(f"access:{access_id}")
        controls = _controls(spec, lines)
        consumer_contexts[spec["fixtureKey"]] = {
            "contextOrder": context_order,
            "id": spec["id"],
            "sourcePath": spec["sourcePath"],
            "entrySymbol": spec["entrySymbol"],
            "entryAddress": spec["entryAddress"],
            "accessIds": context_access_ids,
            "roles": [
                {"kind": "context-entry", "id": f"context-entry:{spec['id']}"},
                *[
                    {"kind": "request-access", "id": f"access:{access_id}"}
                    for access_id in context_access_ids
                ],
            ],
            "controlShapes": controls,
        }
        for symbol, access_ids in by_symbol.items():
            relations.append(
                {
                    "contextId": spec["id"],
                    "symbol": symbol,
                    "accessIds": access_ids,
                }
            )

    anchors = [
        _anchor(rom_pc=rom_pc, roles=roles, h1_rows=h1_rows, rom=rom)
        for rom_pc, roles in sorted(anchor_roles.items())
    ]
    access_order = [row["id"] for row in access_sites]
    symbol_definitions = [
        {
            "symbol": symbol,
            "address": constants[symbol]["value"],
            "sourcePath": "sf2const.asm",
            "sourceLine": constants[symbol]["sourceLine"],
        }
        for symbol in _SYMBOLS
    ]
    role_count = sum(len(row["roles"]) for row in consumer_contexts.values())
    summary = {
        "retainedPositiveProgramContextCount": 39,
        "retainedZeroProgramContextCount": 875,
        "retainedContextOperationCount": 262,
        "retainedWriteDefinitionSiteCount": 45,
        "retainedHandoffStateSiteCount": 67,
        "retainedHandoffStateRelationCount": 69,
        "sourceFileCount": len(_SOURCE_PATHS),
        "consumerContextCount": len(consumer_contexts),
        "symbolDefinitionCount": len(symbol_definitions),
        "lifecycleAccessCount": len(access_sites),
        "symbolContextRelationCount": len(relations),
        "contextRoleCount": role_count,
        "physicalAnchorCount": len(anchors),
    }
    expected_summary = {
        "retainedPositiveProgramContextCount": 39,
        "retainedZeroProgramContextCount": 875,
        "retainedContextOperationCount": 262,
        "retainedWriteDefinitionSiteCount": 45,
        "retainedHandoffStateSiteCount": 67,
        "retainedHandoffStateRelationCount": 69,
        "sourceFileCount": 7,
        "consumerContextCount": 8,
        "symbolDefinitionCount": 6,
        "lifecycleAccessCount": 13,
        "symbolContextRelationCount": 12,
        "contextRoleCount": 21,
        "physicalAnchorCount": 18,
    }
    if summary != expected_summary:
        raise ValueError(f"map-event request-consumption denominator drift: {summary}")
    if (
        len({row["id"] for row in access_sites}) != 13
        or len({row["romPc"] for row in anchors}) != 18
    ):
        raise ValueError("map-event request-consumption access/anchor identity drift")

    consumption = {
        "symbolDefinitions": symbol_definitions,
        "accessSites": access_sites,
        "accessOrder": access_order,
        "consumerContexts": consumer_contexts,
        "symbolContextRelations": relations,
        "roleCounts": {"contextEntry": 8, "requestAccess": 13, "total": 21},
    }
    source_context = {
        "h1Listing": {
            "path": "build/sf2build-h1.lst",
            "sha256": hashlib.sha256(listing_text.encode("utf-8")).hexdigest().upper(),
        },
        "sourceIdentities": source_identities,
        "anchors": anchors,
    }
    return consumption, {"sourceContext": source_context, "summary": summary}


def build_map_event_request_consumption_contract(
    rom_path: Path, upstream_path: Path
) -> dict[str, Any]:
    """Build the public static request-consumption contract from pinned inputs."""
    retained_owners = _fresh_retained_owners(rom_path, upstream_path)
    consumption, shared = _projection(rom_path, upstream_path)
    return {
        "schemaVersion": 1,
        "id": ID,
        "system": "map-event-request-consumption",
        "romSha256": _ROM_SHA256,
        "upstream": {
            "repository": "ShiningForceCentral/SF2DISASM",
            "commit": _UPSTREAM_COMMIT,
        },
        "retainedOwners": retained_owners,
        "sourceContext": shared["sourceContext"],
        "eventRequestConsumption": consumption,
        "unknowns": {key: "Unknown" for key in _UNKNOWN_KEYS},
        "summary": shared["summary"],
    }


def verify_map_event_request_consumption_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    output = build_map_event_request_consumption_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="map-event request-consumption rebuilt contract")
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner="map-event request-consumption fixture")
    if output != fixture:
        raise ValueError("map-event request-consumption fixture drift")
    destination = output_path or FIXTURE
    if output_path is not None:
        destination.parent.mkdir(parents=True, exist_ok=True)
        destination.write_bytes(canonical_json_bytes(output))
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": hashlib.sha256(canonical_json_bytes(output)).hexdigest().upper(),
        "Accesses": output["summary"]["lifecycleAccessCount"],
        "Contexts": output["summary"]["consumerContextCount"],
        "Status": "PASS",
    }
