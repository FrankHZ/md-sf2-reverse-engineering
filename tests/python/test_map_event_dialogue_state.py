from __future__ import annotations

import hashlib
import json
import subprocess
from copy import deepcopy
from pathlib import Path

import pytest

from sf2tool.cli import build_parser
from sf2tool.h2.map_event_combatant_state import (
    normalize_map_event_combatant_state_later_owner_index as normalize_later_owner_index,
)
from sf2tool.h2.map_event_dialogue_state import (
    _EXPECTED_TABLE_OWNERS,
    _canonical_source_file_rows,
    _contextual_continuation_bindings,
    _dispatcher_prefill_writes,
    _lookup_call_anchor,
    _macro_source_target,
    _physical_anchor,
    _physical_state_access_pcs,
)
from sf2tool.jsonio import load_json as _load_json
from sf2tool.jsonio import validate_json
from sf2tool.research_index import verify_index

ROOT = Path(__file__).resolve().parents[2]
FIXTURE = ROOT / "tests/fixtures/h2/map-event-dialogue-state-static-v1.json"
SCHEMA = ROOT / "schemas/h2/map-event-dialogue-state-static-fixture.schema.json"
INDEX = ROOT / "manifests/research-index.json"
INDEX_SCHEMA = ROOT / "schemas/research-index.schema.json"
UPSTREAM = ROOT / "local/upstream/SF2DISASM"
BASE = "ae71e74055b73f7fd680f4d4a98f0af421a2cc51"
ID = "sf2-map-event-dialogue-state-static-v1"
DOCUMENT = "docs/research/map-event-dialogue-state.md"
VERIFIER = "src/sf2tool/h2/map_event_dialogue_state.py"
_REQUEST_STATE_FIXTURE_ID = "sf2-map-event-request-state-static-v1"
_REQUEST_STATE_DOCUMENT = "docs/research/map-event-request-state.md"


def load_json(path):
    value = _load_json(path)
    return normalize_later_owner_index(value) if path == INDEX else value
EXPECTED_INDEX_BINDINGS = {
    "map.data.ms-map3-flag506-entityevents": "ms_map3_flag506_EntityEvents",
    "map.data.ms-map3-zoneevents": "ms_map3_ZoneEvents",
    "map.data.ms-map5-flag530-entityevents": "ms_map5_flag530_EntityEvents",
    "map.data.ms-map5-flag650-entityevents": "ms_map5_flag650_EntityEvents",
    "map.data.ms-map6-flag701-entityevents": "ms_map6_flag701_EntityEvents",
    "map.data.ms-map16-flag530-entityevents": "ms_map16_flag530_EntityEvents",
    "map.data.ms-map18-entityevents": "ms_map18_EntityEvents",
    "map.data.ms-map19-flag506-entityevents": "ms_map19_flag506_EntityEvents",
    "map.data.ms-map20-flag543-zoneevents": "ms_map20_flag543_ZoneEvents",
    "map.data.ms-map21-flag506-entityevents": "ms_map21_flag506_EntityEvents",
    "map.data.ms-map25-entityevents": "ms_map25_EntityEvents",
    "map.data.ms-map37-section5": "ms_map37_Section5",
    "map.data.ms-map40-entityevents": "ms_map40_EntityEvents",
    "map.data.ms-map44-flag507-entityevents": "ms_map44_flag507_EntityEvents",
    "map.data.ms-map63-entityevents": "ms_map63_EntityEvents",
    "map.data.ms-map72-zoneevents": "ms_map72_ZoneEvents",
    "map.data.ms-map77-section5": "ms_map77_Section5",
}


def _short_branch_operation(target_address: int) -> dict[str, object]:
    return {
        "family": "raw-68000-control-flow",
        "address": 0,
        "sourceMnemonic": "bne.s",
        "mnemonic": "bne",
        "sizeSuffix": ".s",
        "operandTexts": ["loc_000046"],
        "controlFlowKind": "conditional-branch",
        "target": {"instructionTargetAddress": target_address},
        "definitionId": None,
    }


def test_physical_anchor_accepts_only_proven_short_branch_placeholder() -> None:
    output = _physical_anchor(
        operation=_short_branch_operation(0x46),
        next_address=2,
        operation_definitions={},
        h1_rows={0: (bytes.fromhex("6600"), "bne.s loc_000046")},
        h1_symbols={},
        rom=bytes.fromhex("6644"),
        context="short-branch-placeholder",
    )

    assert output == {
        "instructionByteLength": 2,
        "h1InstructionSha256": hashlib.sha256(bytes.fromhex("6600")).hexdigest().upper(),
        "romInstructionSha256": hashlib.sha256(bytes.fromhex("6644")).hexdigest().upper(),
        "relocationKinds": ["short-branch-zero-placeholder"],
    }

    with pytest.raises(ValueError, match="H1/ROM relocation drift"):
        _physical_anchor(
            operation=_short_branch_operation(0x46),
            next_address=2,
            operation_definitions={},
            h1_rows={0: (bytes.fromhex("6600"), "bne.s loc_000046")},
            h1_symbols={},
            rom=bytes.fromhex("6744"),
            context="short-branch-wrong-opcode",
        )


def _script_operation(operand: str = "cs_0000FC") -> dict[str, object]:
    return {
        "family": "event-service-macro",
        "address": 0,
        "sourceMnemonic": "script",
        "mnemonic": "script",
        "sizeSuffix": None,
        "operandTexts": [operand],
        "controlFlowKind": "ordinary",
        "target": None,
        "definitionId": "event-service-macro:script",
    }


def test_physical_anchor_accepts_only_proven_pc_relative_placeholder() -> None:
    definitions = {
        "event-service-macro:script": {
            "family": "event-service-macro",
            "sourceMacro": "script",
            "emissionStatementTemplates": ["lea \\1(pc),a0", "trap #mapscript"],
        }
    }
    rows = {
        0: (bytes.fromhex("41FA0000"), "M lea cs_0000fc(pc),a0"),
        4: (bytes.fromhex("4E46"), "M trap #mapscript"),
    }

    output = _physical_anchor(
        operation=_script_operation(),
        next_address=6,
        operation_definitions=definitions,
        h1_rows=rows,
        h1_symbols={},
        rom=bytes.fromhex("41FA00FA4E46"),
        context="pc-relative-placeholder",
    )
    assert output["instructionByteLength"] == 6

    with pytest.raises(ValueError, match="H1/ROM relocation drift"):
        _physical_anchor(
            operation=_script_operation(),
            next_address=6,
            operation_definitions=definitions,
            h1_rows=rows,
            h1_symbols={},
            rom=bytes.fromhex("41FA00F84E46"),
            context="pc-relative-wrong-target",
        )
    with pytest.raises(ValueError, match="H1/ROM relocation drift"):
        _physical_anchor(
            operation=_short_branch_operation(0x44),
            next_address=2,
            operation_definitions={},
            h1_rows={0: (bytes.fromhex("6600"), "bne.s loc_000046")},
            h1_symbols={},
            rom=bytes.fromhex("6644"),
            context="short-branch-wrong-target",
        )


def test_physical_anchor_resolves_named_h1_script_symbol() -> None:
    definitions = {
        "event-service-macro:script": {
            "family": "event-service-macro",
            "sourceMacro": "script",
            "emissionStatementTemplates": ["lea \\1(pc),a0", "trap #mapscript"],
        }
    }
    rows = {
        0: (bytes.fromhex("41FA0000"), "M lea cs_endingkiss(pc),a0"),
        4: (bytes.fromhex("4E46"), "M trap #mapscript"),
    }
    output = _physical_anchor(
        operation=_script_operation("cs_EndingKiss"),
        next_address=6,
        operation_definitions=definitions,
        h1_rows=rows,
        h1_symbols={"cs_endingkiss": 0xFC},
        rom=bytes.fromhex("41FA00FA4E46"),
        context="named-pc-relative-placeholder",
    )
    assert output["relocationKinds"] == ["pc-relative-zero-placeholder", "byte-identical"]

    with pytest.raises(ValueError, match="unresolved H1 script symbol"):
        _physical_anchor(
            operation=_script_operation("cs_EndingKiss"),
            next_address=6,
            operation_definitions=definitions,
            h1_rows=rows,
            h1_symbols={},
            rom=bytes.fromhex("41FA00FA4E46"),
            context="named-pc-relative-missing-symbol",
        )


def test_macro_code_target_uses_only_the_declared_pc_relative_parameter() -> None:
    assert (
        _macro_source_target(
            {"emissionStatementTemplates": ["lea \\2(pc),a0"]},
            ["cs_000010", "cs_0000FC"],
            {},
        )
        == 0xFC
    )

    with pytest.raises(ValueError, match="macro code operand position drift"):
        _macro_source_target(
            {"emissionStatementTemplates": ["lea \\2(pc),a0"]},
            ["cs_000010"],
            {},
        )
    with pytest.raises(ValueError, match="ambiguous macro code operand position"):
        _macro_source_target(
            {
                "emissionStatementTemplates": [
                    "lea \\1(pc),a0",
                    "lea \\2(pc),a1",
                ]
            },
            ["cs_000010", "cs_0000FC"],
            {},
        )


@pytest.mark.parametrize(
    ("operation", "h1", "rom", "expected_kind"),
    [
        (
            {
                **_short_branch_operation(0x1E),
                "sourceMnemonic": "bne.w",
                "sizeSuffix": ".w",
                "operandTexts": ["loc_00001E"],
            },
            bytes.fromhex("66000000"),
            bytes.fromhex("6600001C"),
            "word-branch-zero-placeholder",
        ),
        (
            {
                "family": "raw-68000-control-flow",
                "address": 0,
                "sourceMnemonic": "jsr",
                "mnemonic": "jsr",
                "sizeSuffix": None,
                "operandTexts": ["j_Target"],
                "controlFlowKind": "direct-call",
                "target": {"instructionTargetAddress": 0x1AC068},
                "definitionId": None,
            },
            bytes.fromhex("4EB900000000"),
            bytes.fromhex("4EB9001AC068"),
            "absolute-control-zero-placeholder",
        ),
    ],
)
def test_physical_anchor_accepts_only_closed_zero_placeholder_kinds(
    operation: dict[str, object], h1: bytes, rom: bytes, expected_kind: str
) -> None:
    output = _physical_anchor(
        operation=operation,
        next_address=len(h1),
        operation_definitions={},
        h1_rows={0: (h1, f"{operation['sourceMnemonic']} {operation['operandTexts'][0]}")},
        h1_symbols={},
        rom=rom,
        context=expected_kind,
    )
    assert output["relocationKinds"] == [expected_kind]

    with pytest.raises(ValueError, match="H1/ROM relocation drift"):
        _physical_anchor(
            operation=operation,
            next_address=len(h1),
            operation_definitions={},
            h1_rows={0: (h1, f"{operation['sourceMnemonic']} {operation['operandTexts'][0]}")},
            h1_symbols={},
            rom=rom[:-1] + bytes([rom[-1] - 2]),
            context=f"{expected_kind}-wrong-target",
        )


def _continuation_operation(address: int, mnemonic: str) -> dict[str, object]:
    return {
        "address": address,
        "family": "raw-68000-control-flow" if mnemonic == "rts" else "event-service-macro",
        "sourceMnemonic": mnemonic,
        "mnemonic": mnemonic,
        "operandTexts": [],
        "controlFlowKind": "return" if mnemonic == "rts" else "ordinary",
        "target": None,
    }


def _continuation_program(
    symbol: str, entry: int, labels: list[dict[str, object]], operations: list[dict[str, object]]
) -> dict[str, object]:
    return {
        "canonicalSymbol": symbol,
        "entryAddress": entry,
        "sourcePath": "event.asm",
        "labels": labels,
        "operations": operations,
    }


def test_contextual_continuation_rejects_suffix_and_entry_label_mutations(
    tmp_path: Path,
) -> None:
    (tmp_path / "event.asm").write_text("Owner:\nDefault:\n\nTail:\n", encoding="utf-8")
    owner = _continuation_program(
        "Owner",
        0,
        [
            {"symbol": "Owner", "address": 0, "sourceLine": 1},
            {"symbol": "Tail", "address": 4, "sourceLine": 4},
        ],
        [_continuation_operation(0, "txt"), _continuation_operation(4, "txt")],
    )
    continuation = _continuation_program(
        "Default",
        2,
        [
            {"symbol": "Default", "address": 2, "sourceLine": 2},
            {"symbol": "Tail", "address": 4, "sourceLine": 4},
        ],
        [_continuation_operation(2, "rts"), _continuation_operation(4, "txt")],
    )
    selected = [("entityEvents", owner), ("entityEvents", continuation)]
    assert _contextual_continuation_bindings(selected, disasm=tmp_path) == {
        ("entityEvents", "Default", 2): {
            "ownerKey": ("entityEvents", "Owner", 0),
            "ownerSourceOrder": 1,
            "continuationSourceOrder": 1,
            "romPc": 4,
        }
    }

    suffix_drift = deepcopy(selected)
    suffix_drift[1][1]["operations"][1]["sourceMnemonic"] = "clsTxt"
    with pytest.raises(ValueError, match="contextual continuation owner drift"):
        _contextual_continuation_bindings(suffix_drift, disasm=tmp_path)

    address_drift = deepcopy(selected)
    address_drift[1][1]["labels"][1]["address"] = 6
    with pytest.raises(ValueError, match="contextual continuation label drift"):
        _contextual_continuation_bindings(address_drift, disasm=tmp_path)

    (tmp_path / "event.asm").write_text("Owner:\nDefault drift\n\nTail:\n", encoding="utf-8")
    with pytest.raises(ValueError, match="label source drift"):
        _contextual_continuation_bindings(selected, disasm=tmp_path)


def _table_owner_rows() -> list[dict[str, object]]:
    return [
        {
            "category": category,
            "tableSymbol": symbol,
            "tableEntryAddress": address,
            "sourcePath": f"data/maps/entries/{index:02d}.asm",
        }
        for index, (category, symbol, address) in enumerate(_EXPECTED_TABLE_OWNERS)
    ]


def test_source_file_owner_order_is_source_path_canonical_and_fail_closed() -> None:
    expected = _table_owner_rows()
    assert _canonical_source_file_rows(list(reversed(expected))) == expected

    with pytest.raises(ValueError, match="expected=.*actual="):
        _canonical_source_file_rows(expected[:-1])
    with pytest.raises(ValueError, match="expected=.*actual="):
        _canonical_source_file_rows(
            expected
            + [
                {
                    "category": "entityEvents",
                    "tableSymbol": "ms_map99_EntityEvents",
                    "tableEntryAddress": 999999,
                    "sourcePath": "data/maps/entries/99-extra.asm",
                }
            ]
        )


def test_lookup_call_anchor_rejects_opcode_operand_and_target_drift() -> None:
    helper_address = 0x100
    call_address = 0x110
    statement = "bsr.w GetEntityAddressFromCharacter"
    h1_rows = {call_address: (bytes.fromhex("61000000"), statement)}
    symbols = {"getentityaddressfromcharacter": 0x132}
    rom = bytearray(call_address + 4)
    rom[call_address : call_address + 4] = bytes.fromhex("61000020")

    anchor = _lookup_call_anchor(
        helper_address=helper_address,
        call_address=call_address,
        call_statement=statement,
        h1_rows=h1_rows,
        h1_symbols=symbols,
        rom=bytes(rom),
    )
    assert anchor["romPc"] == call_address

    with pytest.raises(ValueError, match="ROM call drift"):
        _lookup_call_anchor(
            helper_address=helper_address,
            call_address=call_address,
            call_statement=statement,
            h1_rows={call_address: (bytes.fromhex("60000000"), statement)},
            h1_symbols=symbols,
            rom=bytes(rom),
        )
    with pytest.raises(ValueError, match="target symbol drift"):
        _lookup_call_anchor(
            helper_address=helper_address,
            call_address=call_address,
            call_statement="bsr.w WrongLookup",
            h1_rows={call_address: (bytes.fromhex("61000000"), "bsr.w WrongLookup")},
            h1_symbols=symbols,
            rom=bytes(rom),
        )
    wrong_target = bytearray(rom)
    wrong_target[call_address + 3] = 0x1E
    with pytest.raises(ValueError, match="expectedTarget=000132; decodedTarget=000130"):
        _lookup_call_anchor(
            helper_address=helper_address,
            call_address=call_address,
            call_statement=statement,
            h1_rows=h1_rows,
            h1_symbols=symbols,
            rom=bytes(wrong_target),
        )


def _dispatcher_lines(
    writes: list[str], *, lookup: str = "bsr.w GetEntityPortaitAndSpeechSfx"
) -> list[str]:
    return ["RunMapSetupEntityEvent:", f"    {lookup}", *[f"    {row}" for row in writes], "next:"]


def test_dispatcher_prefills_require_both_source_order_symbol_and_value() -> None:
    expected = [
        "move.w d2,((CURRENT_SPEECH_SFX-$1000000)).w",
        "move.w d1,((CURRENT_PORTRAIT-$1000000)).w",
    ]
    actual = _dispatcher_prefill_writes(_dispatcher_lines(expected), dispatcher_line=1)
    assert [row[:2] for row in actual] == [
        ("CURRENT_SPEECH_SFX", "d2"),
        ("CURRENT_PORTRAIT", "d1"),
    ]

    for rows in (
        expected[:1],
        list(reversed(expected)),
        [
            "move.w d2,((CURRENT_PORTRAIT-$1000000)).w",
            "move.w d1,((CURRENT_SPEECH_SFX-$1000000)).w",
        ],
        [
            "move.w d3,((CURRENT_SPEECH_SFX-$1000000)).w",
            "move.w d1,((CURRENT_PORTRAIT-$1000000)).w",
        ],
    ):
        with pytest.raises(ValueError, match="prefill source drift: expected=.*actual="):
            _dispatcher_prefill_writes(_dispatcher_lines(rows), dispatcher_line=1)


def test_physical_state_pc_denominator_keeps_edges_and_rejects_conflicts() -> None:
    row = {
        "romPc": 0x100,
        "accessOperandIndex": 0,
        "accessKind": "read",
        "symbol": "CURRENT_SPEECH_SFX",
        "mnemonic": "move",
        "width": "w",
        "operandTexts": ["((CURRENT_SPEECH_SFX-$1000000)).w", "d0"],
        "address": 0x1000000,
        "instructionByteLength": 4,
        "instructionSha256": "A" * 64,
        "valueKind": "ram-read",
        "valueToken": "d0",
        "resolvedValue": None,
    }
    duplicate_context = {**row, "category": "zoneEvents", "programSymbol": "Other"}
    assert _physical_state_access_pcs([row, duplicate_context]) == {0x100}

    conflict = {**row, "symbol": "CURRENT_PORTRAIT"}
    with pytest.raises(ValueError, match="conflicting same-PC state edge drift"):
        _physical_state_access_pcs([row, conflict])


def test_state_set_schema_allows_empty_must_only() -> None:
    fixture = load_json(FIXTURE)
    positive = deepcopy(fixture)
    state = positive["eventDialogueState"]["returnStateSites"][4]["state"][0]
    state["mustDefinitionIds"] = []
    validate_json(positive, SCHEMA, owner="dialogue-state empty-must positive")

    empty_may = deepcopy(positive)
    empty_may["eventDialogueState"]["returnStateSites"][4]["state"][0]["mayDefinitionIds"] = []
    with pytest.raises(ValueError, match="schema validation"):
        validate_json(empty_may, SCHEMA, owner="dialogue-state empty-may negative")

    duplicate_must = deepcopy(positive)
    duplicate_must["eventDialogueState"]["returnStateSites"][4]["state"][0]["mustDefinitionIds"] = [
        "entry:entityEvents-inherited:MESSAGE_SPEED"
    ] * 2
    with pytest.raises(ValueError, match="schema validation"):
        validate_json(duplicate_must, SCHEMA, owner="dialogue-state duplicate-must negative")

    wrong_type = deepcopy(positive)
    wrong_type["eventDialogueState"]["returnStateSites"][4]["state"][0]["mustDefinitionIds"] = [1]
    with pytest.raises(ValueError, match="schema validation"):
        validate_json(wrong_type, SCHEMA, owner="dialogue-state wrong-must-type negative")


def test_fixture_is_closed_and_preserves_the_accepted_static_denominators() -> None:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner="map-event dialogue-state fixture")
    assert list(fixture) == [
        "schemaVersion",
        "id",
        "upstream",
        "romSha256",
        "sourceContext",
        "retainedOwners",
        "eventDialogueState",
        "unknowns",
        "summary",
    ]
    assert fixture["id"] == ID
    assert fixture["summary"] == {
        "motherProgramContextCount": 914,
        "motherOperationCount": 3579,
        "positiveProgramContextCount": 24,
        "zeroProgramContextCount": 890,
        "sourceFileCount": 17,
        "sourceIdentityCount": 22,
        "symbolDefinitionCount": 5,
        "contextStateAccessCount": 100,
        "physicalStateAccessCount": 72,
        "textBearingProgramContextCount": 23,
        "stateOnlyProgramContextCount": 1,
        "contextTextSiteCount": 89,
        "physicalTextSiteCount": 76,
        "contextLineReferenceCount": 69,
        "contextCloseSentinelCount": 20,
        "contextStateOrTextSiteCount": 189,
        "physicalStateOrTextPcCount": 148,
        "contextOperationCount": 414,
        "physicalOperationCount": 374,
        "contextLabelCount": 72,
        "physicalLabelCount": 64,
        "ordinaryOperationCount": 278,
        "conditionalBranchCount": 40,
        "unconditionalBranchCount": 20,
        "directCallCount": 49,
        "directJumpCount": 1,
        "returnCount": 26,
        "returnStateSiteCount": 26,
        "h1RomAnchorCount": 379,
    }
    assert list(fixture["unknowns"]) == [
        "normalStoryProgramReachability",
        "selectedControlFlowPath",
        "entityLookupInputIdentity",
        "entityPortraitLookupResult",
        "entitySpeechSfxLookupResult",
        "zoneItemInheritedEntryState",
        "actualTextServiceExecution",
        "actualDisplayedLineOrder",
        "dialogueNameSubstitutionValue",
        "portraitWindowVisibilityAndPlacement",
        "speechSfxPlaybackAndTiming",
        "messageSpeedCadence",
        "controllerAdvanceTiming",
        "postReturnStateLifetimeAndPersistence",
        "storyMeaning",
    ]
    assert set(fixture["unknowns"].values()) == {"Unknown"}

    for mutator in (
        lambda value: value.__setitem__("privateRomBytes", "00"),
        lambda value: value["eventDialogueState"]["textStateSites"][0].__setitem__(
            "decodedProse", "text"
        ),
        lambda value: value["sourceContext"].__setitem__("runtimeCapturePath", "local/capture"),
    ):
        broken = deepcopy(fixture)
        mutator(broken)
        with pytest.raises(ValueError, match="schema validation"):
            validate_json(broken, SCHEMA, owner="map-event dialogue-state fixture")


def _without_request_state(index):
    normalized = deepcopy(index)
    removed: set[str] = set()
    for record in normalized["records"]:
        evidence = [
            item for item in record["evidence"] if item["fixtureId"] == _REQUEST_STATE_FIXTURE_ID
        ]
        if not evidence:
            continue
        assert evidence == [
            {
                "level": "H2",
                "fixture": "tests/fixtures/h2/map-event-request-state-static-v1.json",
                "fixtureId": _REQUEST_STATE_FIXTURE_ID,
                "verifier": "src/sf2tool/h2/map_event_request_state.py",
                "bindings": [
                    {
                        "addressId": "entry",
                        "fixtureField": (
                            f"eventRequestState.sourceFiles.{record['symbol']}.tableEntryAddress"
                        ),
                    }
                ],
            }
        ]
        assert record["documents"][-1] == _REQUEST_STATE_DOCUMENT
        record["evidence"].remove(evidence[0])
        record["documents"].pop()
        removed.add(record["id"])
    assert len(removed) == 24
    return normalized


def _without_request_consumption(index: dict[str, object]) -> dict[str, object]:
    for record in index["records"]:
        evidence = [
            item
            for item in record["evidence"]
            if item["fixtureId"] == "sf2-map-event-request-consumption-static-v1"
        ]
        if not evidence:
            continue
        assert len(evidence) == 1
        assert record["documents"].count("docs/research/map-event-request-consumption.md") == 1
        record["evidence"] = [item for item in record["evidence"] if item not in evidence]
        record["documents"].remove("docs/research/map-event-request-consumption.md")
        record["addresses"] = [
            address
            for address in record["addresses"]
            if address["id"]
            not in {
                "get-shop-inventory-address",
                "process-map-event",
                "declare-raft-entity",
                "raft-refresh",
            }
        ]
    return index

def test_research_index_delta_is_exact_17_binding_append_without_object_or_design_drift() -> None:
    index = _without_request_consumption(_without_request_state(load_json(INDEX)))
    base = json.loads(
        subprocess.run(
            ["git", "show", f"{BASE}:manifests/research-index.json"],
            cwd=ROOT,
            check=True,
            capture_output=True,
            text=True,
            encoding="utf-8",
        ).stdout
    )
    records = {record["id"]: record for record in index["records"]}
    base_records = {record["id"]: record for record in base["records"]}
    assert set(records) == set(base_records)
    changed = {record_id for record_id in records if records[record_id] != base_records[record_id]}
    assert changed == set(EXPECTED_INDEX_BINDINGS)
    assert len(records) == len(base_records) == 1625
    assert sum(len(record["addresses"]) for record in records.values()) == sum(
        len(record["addresses"]) for record in base_records.values()
    )
    assert sum(len(record.get("designContracts", [])) for record in records.values()) == sum(
        len(record.get("designContracts", [])) for record in base_records.values()
    )
    for record_id, symbol in EXPECTED_INDEX_BINDINGS.items():
        record, previous = records[record_id], base_records[record_id]
        assert record["symbol"] == symbol
        assert record["addresses"] == previous["addresses"]
        assert record["documents"] == previous["documents"] + [DOCUMENT]
        assert record["evidence"] == previous["evidence"] + [
            {
                "level": "H2",
                "fixture": "tests/fixtures/h2/map-event-dialogue-state-static-v1.json",
                "fixtureId": ID,
                "verifier": VERIFIER,
                "bindings": [
                    {
                        "addressId": "entry",
                        "fixtureField": (
                            f"eventDialogueState.sourceFiles.{symbol}.tableEntryAddress"
                        ),
                    }
                ],
            }
        ]
    for record_id in set(records) - set(EXPECTED_INDEX_BINDINGS):
        assert records[record_id] == base_records[record_id]
    assert verify_index(UPSTREAM) == {
        "Index": "manifests/research-index.json",
        "Records": 1626,
        "Confirmed": 1626,
        "H2Fixtures": 95,
        "H3Fixtures": 94,
        "H3FixtureFiles": 94,
        "AddressBindings": 3019,
        "IndexedCodeFiles": 381,
        "IndexedDataFiles": 1017,
        "H1ListingRecords": 1589,
        "AlternateListingRecords": 37,
        "Z80MusicBankRecords": 37,
        "ResearchDocuments": 57,
        "DesignContracts": 68,
        "UpstreamSourcesChecked": True,
        "H1ListingChecked": True,
        "Status": "PASS",
    }


def test_research_index_schema_allows_only_exact_dialogue_state_bindings() -> None:
    index = load_json(INDEX)
    validate_json(index, INDEX_SCHEMA, owner="map-event dialogue-state index")
    bindings = [
        binding
        for record in index["records"]
        for evidence in record["evidence"]
        if evidence["fixtureId"] == ID
        for binding in evidence["bindings"]
    ]
    assert len(bindings) == 17
    assert {binding["fixtureField"] for binding in bindings} == {
        f"eventDialogueState.sourceFiles.{symbol}.tableEntryAddress"
        for symbol in EXPECTED_INDEX_BINDINGS.values()
    }
    for fixture_field in (
        "unknownRoot.eventDialogueState",
        "eventDialogueState.sourceFiles.ms_map3_Foo.tableEntryAddress",
        "eventDialogueState.sourceFiles.ms_map3_ZoneEvents.unknown",
        "sourceContext.eventDialogueState.sourceFiles.ms_map3_ZoneEvents.tableEntryAddress",
    ):
        broken = deepcopy(index)
        next(
            binding
            for record in broken["records"]
            for evidence in record["evidence"]
            if evidence["fixtureId"] == ID
            for binding in evidence["bindings"]
        )["fixtureField"] = fixture_field
        with pytest.raises(ValueError, match="schema validation"):
            validate_json(broken, INDEX_SCHEMA, owner="map-event dialogue-state index")


def test_dialogue_state_has_a_static_rom_parity_command() -> None:
    args = build_parser().parse_args(["h2", "map-event-dialogue-state"])
    assert args.h2_command == "map-event-dialogue-state"
    assert args.rom_path.name == "sf2-us.bin"
    assert args.output_path is None
