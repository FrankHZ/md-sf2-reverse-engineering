from __future__ import annotations

from copy import deepcopy
from pathlib import Path

import pytest

import sf2tool.h3.map_entity_action_bridge as bridge
from sf2tool.h2.map_script_engine import build_map_script_engine_contract
from sf2tool.h3.bizhawk import bizhawk_contract, validate_lua_syntax
from sf2tool.h3.map_entity_action_bridge import (
    FIXTURE,
    FIXTURE_SCHEMA,
    OBSERVATION_SCHEMA,
    OBSERVER,
    _h1_ordered_call_addresses,
    _require_ordered_source_use_sites,
    build_map_entity_action_bridge_contract,
    build_map_entity_action_bridge_static_contract,
    derive_case_expectations,
)
from sf2tool.jsonio import load_json, validate_json
from sf2tool.research_index import listing_symbol_addresses

ROM = Path("local/roms/sf2-us.bin")
UPSTREAM = Path("local/upstream/SF2DISASM")


def _fixture() -> dict[str, object]:
    return load_json(FIXTURE)


def _observation(contract: dict[str, object]) -> dict[str, object]:
    fixture = _fixture()
    expected = derive_case_expectations(contract, fixture, UPSTREAM)
    return {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["id"],
        "mapTest": fixture["mapTestIndex"],
        "recordOrder": [case["id"] for case in fixture["cases"]],
        "records": [
            {**record, **case["runtimeGolden"]}
            for record, case in zip(expected, fixture["cases"], strict=True)
        ],
    }


def _source_inputs() -> tuple[str, dict[str, int], str, dict[str, int]]:
    source = (UPSTREAM / bridge.SOURCE_PATH).read_text(encoding="utf-8")
    constants_source = "\n".join(
        (UPSTREAM / "disasm" / path).read_text(encoding="utf-8") for path in bridge.CONSTANT_PATHS
    )
    equates = bridge._parse_equates(
        constants_source,
        {
            "ENTITY_DATA",
            "ENTITY_INDEX_LIST",
            "ENTITYDEF_SIZE",
            "ENTITYDEF_OFFSET_ACTSCRIPTADDR",
            "ENTITYDEF_OFFSET_ACTSCRIPTWAITTIMER",
            "ENTITYDEF_OFFSET_FLAGS_A",
            "BYTE_LOWER_NIBBLE_MASK",
            "dword_FFB1A4",
        },
    )
    listing = (UPSTREAM / bridge.H1_LISTING_PATH).read_text(encoding="utf-8")
    return source, equates, listing, listing_symbol_addresses(listing)


@pytest.fixture(scope="module")
def h2_contract() -> dict[str, object]:
    return build_map_script_engine_contract(ROM, UPSTREAM)


@pytest.fixture(scope="module")
def bridge_contract() -> dict[str, object]:
    return build_map_entity_action_bridge_contract(ROM, UPSTREAM)


def test_entity_action_bridge_contract_fixture_and_complete_case_matrix(
    bridge_contract: dict[str, object],
) -> None:
    fixture = _fixture()
    validate_json(fixture, FIXTURE_SCHEMA, owner="map entity action bridge fixture")
    assert {
        key: bridge_contract[key] for key in ("function", "ram", "constants", "sourceFacts")
    } == {key: fixture[key] for key in ("function", "ram", "constants", "sourceFacts")}
    derived = derive_case_expectations(bridge_contract, fixture, UPSTREAM)
    assert derived == [case["expected"] for case in fixture["cases"]]
    assert [case["id"] for case in fixture["cases"]] == [
        "set-actscript-wait-loop",
        "set-actscript-control-zero",
        "custom-actscript-wait-loop",
        "custom-actscript-control-zero",
        "entity-actions-wait-loop",
        "entity-actions-control-zero",
    ]
    assert bridge_contract["function"] == {
        "runMapSetupInitFunctionAddress": 292092,
        "csc15_setEntityActscriptAddress": 289144,
        "csc14_setEntityActscriptManualAddress": 289104,
        "csc2D_entityActionSequenceAddress": 288738,
        "easIdleAddress": 283132,
        "csc15GetEntityCallSiteAddress": 289146,
        "csc14GetEntityCallSiteAddress": 289106,
        "csc2DGetEntityCallSiteAddress": 288740,
        "csc2DIndexedCallSiteAddress": 288788,
        "csc15WaitCompareAddress": 289166,
        "csc15WaitBackEdgeAddress": 289174,
        "csc14WaitCompareAddress": 289126,
        "csc14WaitBackEdgeAddress": 289134,
        "csc14InlineTerminatorCompareAddress": 289136,
        "csc2DTerminalEntryAddress": 289064,
        "csc2DTerminalPayloadAfterWriteAddress": 289074,
        "csc2DTerminalWaitCompareAddress": 289092,
        "csc2DTerminalWaitBackEdgeAddress": 289100,
    }
    assert bridge_contract["constants"]["entityActionTerminalBranch"] == {
        "sourceUseSite": {"instruction": "bmi.w loc_46928", "sourceLine": 514},
        "branchPolarity": "negative",
        "targetLabel": "loc_46928",
        "targetAddress": 289064,
    }
    assert bridge_contract["sourceFacts"]["indexedDispatchTargets"][8] == {
        "index": 8,
        "target": "csc2D_8_faceRight",
        "targetAddress": 288938,
        "sourceUseSite": {
            "instruction": "dc.w csc2D_8_faceRight-rjt_EntityMoveCommands",
            "sourceLine": 536,
        },
    }
    assert bridge_contract["sourceFacts"]["callerBreakdown"]["instructionTargetTotals"] == {
        "GetEntityAddressFromCharacter": 3,
        "rjt_EntityMoveCommands": 1,
    }
    assert derived[0]["actscriptWaitTimerByteAfter"] == 0
    assert fixture["cases"][0]["selectorByte"] == 1
    assert derived[4]["actionBufferWords"] == [34, 0, 10, 0, 7, 52]
    assert derived[4]["indexedActionBufferWordTransferByteCount"] == 2
    assert derived[4]["actionBufferPointerLongAfter"] == 16728336
    assert fixture["runtimeQuestions"] == [
        "map-script-entity-action-bridge/normal-story-reachability",
        "map-script-entity-action-bridge/full-action-motion-collision-effects",
        "map-script-entity-action-bridge/presentation-timing-persistence",
    ]


@pytest.mark.parametrize(
    ("name", "mutation"),
    [
        (
            "missing-nested-field",
            lambda value: value["cases"][4]["expected"].pop("terminalActionBufferIdlePayloadLong"),
        ),
        (
            "renamed-nested-field",
            lambda value: value["cases"][0]["waitLoopExitInjection"].update(
                {
                    "backEdgeInstruction": value["cases"][0]["waitLoopExitInjection"].pop(
                        "backEdgeInstructionAddress"
                    )
                }
            ),
        ),
        (
            "extra-nested-field",
            lambda value: value["cases"][2]["entityStateSeed"].update({"unexpected": 1}),
        ),
        ("reordered-cases", lambda value: value["cases"].reverse()),
        (
            "byte-boundary",
            lambda value: value["cases"][4].__setitem__("actionCommandByte", 256),
        ),
    ],
)
def test_entity_action_bridge_fixture_schema_rejects_complete_mutations(
    name: str, mutation: object
) -> None:
    mutated = deepcopy(_fixture())
    assert callable(mutation)
    mutation(mutated)
    with pytest.raises(ValueError, match="fixture failed schema validation"):
        validate_json(mutated, FIXTURE_SCHEMA, owner="map entity action bridge fixture")


@pytest.mark.parametrize(
    ("name", "mutation"),
    [
        (
            "missing-nested-field",
            lambda value: value["records"][4].pop("terminalActionBufferIdlePayloadLong"),
        ),
        (
            "renamed-nested-field",
            lambda value: value["records"][0]["waitLoopExitInjection"].update(
                {
                    "backEdgeInstruction": value["records"][0]["waitLoopExitInjection"].pop(
                        "backEdgeInstructionAddress"
                    )
                }
            ),
        ),
        (
            "extra-nested-field",
            lambda value: value["records"][0]["waitLoopExitInjection"].update({"unexpected": 1}),
        ),
        ("reordered-records", lambda value: value["records"].reverse()),
        (
            "terminal-boundary",
            lambda value: value["records"][4].__setitem__("terminalCommandByte", 127),
        ),
    ],
)
def test_entity_action_bridge_observation_schema_rejects_complete_mutations(
    name: str, mutation: object, bridge_contract: dict[str, object]
) -> None:
    observed = _observation(bridge_contract)
    assert callable(mutation)
    mutation(observed)
    with pytest.raises(ValueError, match="observation failed schema validation"):
        validate_json(observed, OBSERVATION_SCHEMA, owner="map entity action bridge observation")


def test_entity_action_bridge_observation_schema_accepts_complete_semantic_object(
    bridge_contract: dict[str, object],
) -> None:
    validate_json(
        _observation(bridge_contract),
        OBSERVATION_SCHEMA,
        owner="map entity action bridge observation",
    )


@pytest.mark.parametrize(
    ("name", "mutate", "message"),
    [
        (
            "cursor-width",
            lambda facts: facts["handlers"][0]["sectionGuard"]["cursorUseSites"][2].update(
                {"transferredByteCount": 2}
            ),
            "cursor transfer width",
        ),
        (
            "wait-no-wait-polarity",
            lambda facts: facts["handlers"][0]["sectionGuard"]["branchRecords"][0].update(
                {"branchInstruction": "bne.w return_46998"}
            ),
            "H1 instruction identity drift",
        ),
        (
            "callback-order",
            lambda facts: facts["handlers"][2]["sectionGuard"].update(
                {
                    "directCallOrder": [
                        "jsr rjt_EntityMoveCommands(pc,d1.w)",
                        "bsr.w GetEntityAddressFromCharacter",
                    ]
                }
            ),
            "callback order",
        ),
        (
            "terminal-skip",
            lambda facts: facts["handlers"][2]["sectionGuard"]["cursorUseSites"][-1].update(
                {"cursorAdvanceByteCount": 2}
            ),
            "terminal cursor-skip relation",
        ),
        (
            "terminal-branch-polarity",
            lambda facts: facts["handlers"][2]["sectionGuard"]["branchRecords"][0].update(
                {"branchInstruction": "bpl.w loc_46928"}
            ),
            "H1 instruction identity drift",
        ),
        (
            "selector-mask-use-site",
            lambda facts: facts["handlers"][2]["sectionGuard"]["sourceConstantUseSites"][0].update(
                {"value": 14}
            ),
            "selector-mask H2/source use-site",
        ),
        (
            "terminal-order",
            lambda facts: facts["handlers"][2]["sectionGuard"]["terminalChunk"][
                "guardedStatements"
            ].__setitem__(0, "move.w #$35,(a0)+"),
            "terminal source/H2 write order",
        ),
    ],
)
def test_entity_action_bridge_h2_mutations_fail_before_fixture_comparison(
    monkeypatch: pytest.MonkeyPatch,
    h2_contract: dict[str, object],
    name: str,
    mutate: object,
    message: str,
) -> None:
    del name
    changed = deepcopy(h2_contract)
    assert callable(mutate)
    mutate(changed["entityActionBridgeCommandFacts"])
    monkeypatch.setattr(bridge, "build_map_script_engine_contract", lambda *_: changed)
    with pytest.raises(ValueError, match=message):
        build_map_entity_action_bridge_static_contract(ROM, UPSTREAM)


@pytest.mark.parametrize(
    ("name", "old", "new", "message", "requires_terminal_cross_check"),
    [
        (
            "selector-scale",
            "add.w   d1,d1",
            "add.w   d1,d2",
            "runtime source relation drift",
            False,
        ),
        (
            "dispatch-selected-target",
            "csc2D_8_faceRight-rjt_EntityMoveCommands",
            "missingTarget-rjt_EntityMoveCommands",
            "H1 label is missing",
            False,
        ),
        (
            "terminal-branch-polarity",
            "bmi.w   loc_46928",
            "bpl.w   loc_46928",
            "runtime source relation drift",
            False,
        ),
        (
            "cross-handler-pointer-width",
            "move.l  a6,ENTITYDEF_OFFSET_ACTSCRIPTADDR(a5)",
            "move.w  a6,ENTITYDEF_OFFSET_ACTSCRIPTADDR(a5)",
            "runtime source relation drift",
            False,
        ),
        (
            "terminal-record-word",
            "move.w  #$34,(a0)+",
            "move.w  #$35,(a0)+",
            "terminal source/H2 write order drift",
            True,
        ),
        (
            "terminal-idle-payload",
            "move.l  #eas_Idle,(a0)+",
            "move.l  #eas_Init,(a0)+",
            "terminal source/H2 write order drift",
            True,
        ),
        (
            "terminal-pointer-update",
            "move.l  a0,(dword_FFB1A4).l",
            "move.l  d0,(dword_FFB1A4).l",
            "terminal source relation drift",
            False,
        ),
    ],
)
def test_entity_action_bridge_source_use_site_mutations_fail_before_fixture_comparison(
    h2_contract: dict[str, object],
    name: str,
    old: str,
    new: str,
    message: str,
    requires_terminal_cross_check: bool,
) -> None:
    del name
    source, equates, listing, addresses = _source_inputs()
    changed = source.replace(old, new, 1)
    assert changed != source
    if requires_terminal_cross_check:
        fields = bridge._source_fields(changed, equates, listing, addresses)
        with pytest.raises(ValueError, match=message):
            bridge._validate_terminal_chunk_source_relation(
                h2_contract["entityActionBridgeCommandFacts"]["handlers"][2],
                fields["sourceUseSites"]["terminal"],
            )
    else:
        with pytest.raises(ValueError, match=message):
            bridge._source_fields(changed, equates, listing, addresses)


def test_entity_action_bridge_source_parser_scopes_sections_and_strips_comments() -> None:
    source = (
        "other:\n move.b (a6)+,d0\n; End of function other\n"
        "target:\n ; move.b (a6)+,d0\n move.b (a6)+,d0 ; real\n"
        "; End of function target\n"
    )
    assert _require_ordered_source_use_sites(source, "target", ("move.b (a6)+,d0",)) == [
        {"instruction": "move.b (a6)+,d0", "sourceLine": 6}
    ]


def test_entity_action_bridge_indexed_template_rejects_non_word_record_mutation() -> None:
    source = (UPSTREAM / bridge.SOURCE_PATH).read_text(encoding="utf-8")
    changed = source.replace("move.w  #$22,(a0)+", "move.b  #$22,(a0)+", 1)
    assert changed != source
    with pytest.raises(ValueError, match="indexed target layout drift"):
        bridge._indexed_action_template(changed, "csc2D_8_faceRight")


def test_entity_action_bridge_h1_parser_rejects_comment_and_accepts_suffixes() -> None:
    listing = (
        "00000000 test:\n"
        "00000000 6100 0000  bsr.w   First\n"
        "00000004             ; jsr (False).w\n"
        "00000004 4EB8 0000  jsr     (Second).w\n"
        "; End of function test\n"
    )
    assert _h1_ordered_call_addresses(listing, "test", ["bsr.w First", "jsr (Second).w"]) == [
        0,
        4,
    ]
    with pytest.raises(ValueError, match="direct-call order drift"):
        _h1_ordered_call_addresses(listing, "test", ["bsr.w First", "jsr (False).w"])


def test_entity_action_bridge_lua_syntax_and_reserved_key_preflight() -> None:
    source = OBSERVER.read_text(encoding="utf-8")
    assert "config.function" not in source
    assert "config.harness.function" not in source
    _, executable = bizhawk_contract()
    validate_lua_syntax(OBSERVER, executable)
