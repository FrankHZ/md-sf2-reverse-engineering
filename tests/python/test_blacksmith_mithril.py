from __future__ import annotations

import copy
import hashlib
import json
import re
from pathlib import Path

import pytest

from sf2tool.h3 import blacksmith_mithril
from sf2tool.jsonio import load_json, schema_composition_audit, validate_json


def _fixture() -> dict[str, object]:
    return load_json(blacksmith_mithril.FIXTURE)


def _static(fixture: dict[str, object]) -> dict[str, object]:
    return blacksmith_mithril.build_static_contract(
        fixture, blacksmith_mithril.repo_path("local/upstream/SF2DISASM")
    )


def _write(tmp_path: Path, name: str, value: dict[str, object]) -> Path:
    path = tmp_path / name
    path.write_text(json.dumps(value), encoding="utf-8")
    return path


def test_fixture_is_input_only_and_observation_schema_is_recursively_closed() -> None:
    fixture = _fixture()
    validate_json(fixture, blacksmith_mithril.FIXTURE_SCHEMA, owner="blacksmith fixture")
    assert fixture["caseOrder"] == list(blacksmith_mithril.CASE_IDS)
    assert fixture["transactionCaseOrder"] == list(blacksmith_mithril.TRANSACTION_CASE_IDS)
    assert fixture["fulfillmentCaseOrder"] == list(blacksmith_mithril.FULFILLMENT_CASE_IDS)
    assert fixture["precommitCaseOrder"] == list(blacksmith_mithril.PRECOMMIT_CASE_IDS)
    assert all(
        "expected" not in case and "result" not in case
        for case in [
            *fixture["cases"],
            *fixture["transactionCases"],
            *fixture["fulfillmentCases"],
            *fixture["precommitCases"],
        ]
    )
    assert all(
        "expected" not in str(value) and "result" not in str(value)
        for value in fixture["sourceContext"].values()
    )
    static = _static(fixture)
    observed = blacksmith_mithril.expected_observation(fixture, static)
    validate_json(observed, blacksmith_mithril.OBSERVATION_SCHEMA, owner="blacksmith observation")
    blacksmith_mithril._assert_observation(fixture, static, observed)

    malformed = copy.deepcopy(fixture)
    malformed["cases"][0]["expected"] = {"itemIndex": 69}
    with pytest.raises(ValueError, match="Additional properties"):
        validate_json(malformed, blacksmith_mithril.FIXTURE_SCHEMA, owner="blacksmith fixture")
    malformed = copy.deepcopy(fixture)
    malformed["transactionCases"][0]["goldAfter"] = 500
    with pytest.raises(ValueError, match="Additional properties"):
        validate_json(malformed, blacksmith_mithril.FIXTURE_SCHEMA, owner="blacksmith fixture")
    malformed = copy.deepcopy(fixture)
    malformed["fulfillmentCases"][0]["fulfilledOrdersAfter"] = 1
    with pytest.raises(ValueError, match="Additional properties"):
        validate_json(malformed, blacksmith_mithril.FIXTURE_SCHEMA, owner="blacksmith fixture")
    malformed = copy.deepcopy(fixture)
    malformed["precommitCases"][0]["terminal"] = "done"
    with pytest.raises(ValueError, match="Additional properties"):
        validate_json(malformed, blacksmith_mithril.FIXTURE_SCHEMA, owner="blacksmith fixture")
    malformed = copy.deepcopy(observed)
    malformed["records"][0]["rngCalls"][0]["unexpected"] = True
    with pytest.raises(ValueError, match="Additional properties"):
        validate_json(
            malformed,
            blacksmith_mithril.OBSERVATION_SCHEMA,
            owner="blacksmith observation",
        )
    malformed = copy.deepcopy(observed)
    malformed["records"][1]["ordersAfter"] = [40]
    with pytest.raises(ValueError, match="is too short"):
        validate_json(
            malformed,
            blacksmith_mithril.OBSERVATION_SCHEMA,
            owner="blacksmith observation",
        )
    malformed = copy.deepcopy(observed)
    malformed["transactionRecords"][0]["callbackChronology"][5]["pc"] += 2
    validate_json(
        malformed,
        blacksmith_mithril.OBSERVATION_SCHEMA,
        owner="schema-valid transaction chronology drift",
    )
    with pytest.raises(ValueError, match="exact observed case matrix mismatch"):
        blacksmith_mithril._assert_observation(fixture, static, malformed)
    malformed = copy.deepcopy(observed)
    malformed["fulfillmentRecords"][0]["equippableCarrySet"] = False
    validate_json(
        malformed,
        blacksmith_mithril.OBSERVATION_SCHEMA,
        owner="schema-valid fulfillment carry drift",
    )
    with pytest.raises(ValueError, match="exact observed case matrix mismatch"):
        blacksmith_mithril._assert_observation(fixture, static, malformed)
    malformed = copy.deepcopy(observed)
    malformed["precommitRecords"][1]["callbackChronology"][7]["pc"] += 2
    validate_json(
        malformed,
        blacksmith_mithril.OBSERVATION_SCHEMA,
        owner="schema-valid precommit chronology drift",
    )
    with pytest.raises(ValueError, match="exact observed case matrix mismatch"):
        blacksmith_mithril._assert_observation(fixture, static, malformed)
    coordinated_drift = copy.deepcopy(fixture)
    coordinated_drift["acceptedObservation"]["records"][0]["itemIndex"] ^= 1
    validate_json(
        coordinated_drift,
        blacksmith_mithril.FIXTURE_SCHEMA,
        owner="schema-valid coordinated fixture drift",
    )
    with pytest.raises(ValueError, match="independent model"):
        blacksmith_mithril._assert_golden(coordinated_drift, static)


def test_transaction_schema_owns_shape_while_verifier_owns_case_identity() -> None:
    fixture = _fixture()
    static = _static(fixture)
    fixture_schema = load_json(blacksmith_mithril.FIXTURE_SCHEMA)
    observation_schema = load_json(blacksmith_mithril.OBSERVATION_SCHEMA)
    source_context = fixture_schema["definitions"]["sourceContext"]["properties"]
    assert {field: source_context[field]["const"] for field in source_context} == {
        "pickSourcePath": "code/common/menus/blacksmith/pickmithrilweapon.asm",
        "placeSourcePath": "code/common/menus/blacksmith/blacksmithactions.asm",
        "itemStatsSourcePath": "code/common/stats/itemstats.asm",
        "tableSourcePath": "data/stats/items/mithrilweapons.asm",
        "itemDefinitionsSourcePath": "data/stats/items/itemdefs.asm",
        "h1ListingPath": "build/sf2build-h1.lst",
        "functionEntryAddress": 138966,
        "placeEntryAddress": 138690,
        "fulfillEntryAddress": 138050,
        "fulfillSelectionLoopAddress": 138072,
        "fulfillAddItemEntryAddress": 138212,
        "fulfillDoneAddress": 138452,
    }
    assert static["function"]["entryAddress"] == fixture["sourceContext"]["functionEntryAddress"]
    assert (
        static["transaction"]["placeEntryAddress"] == fixture["sourceContext"]["placeEntryAddress"]
    )
    assert (
        static["fulfillment"]["addItemEntryAddress"]
        == fixture["sourceContext"]["fulfillAddItemEntryAddress"]
    )
    assert static["precommit"]["entryAddress"] == fixture["sourceContext"]["fulfillEntryAddress"]
    assert (
        static["precommit"]["runtimeStartAddress"]
        == fixture["sourceContext"]["fulfillSelectionLoopAddress"]
    )
    assert static["precommit"]["doneAddress"] == fixture["sourceContext"]["fulfillDoneAddress"]

    for field, value in (
        ("pickSourcePath", "code/common/menus/blacksmith/wrong.asm"),
        ("placeSourcePath", "code/common/menus/blacksmith/wrong.asm"),
        ("itemStatsSourcePath", "code/common/stats/wrong.asm"),
        ("tableSourcePath", "data/stats/items/wrong.asm"),
        ("itemDefinitionsSourcePath", "data/stats/items/wrong.asm"),
        ("h1ListingPath", "build/wrong.lst"),
        ("functionEntryAddress", 138968),
        ("placeEntryAddress", 138692),
        ("fulfillEntryAddress", 138052),
        ("fulfillSelectionLoopAddress", 138074),
        ("fulfillAddItemEntryAddress", 138214),
        ("fulfillDoneAddress", 138454),
    ):
        wrong_context = copy.deepcopy(fixture)
        wrong_context["sourceContext"][field] = value
        with pytest.raises(ValueError, match="failed schema validation"):
            validate_json(
                wrong_context,
                blacksmith_mithril.FIXTURE_SCHEMA,
                owner=f"source-context schema identity {field}",
            )

    transaction_shapes = (
        fixture_schema["properties"]["transactionCaseOrder"],
        fixture_schema["definitions"]["transactionCase"]["properties"]["id"],
        observation_schema["definitions"]["observationPayload"]["properties"][
            "transactionCaseOrder"
        ],
        observation_schema["definitions"]["transactionRecord"]["properties"]["id"],
        fixture_schema["properties"]["fulfillmentCaseOrder"],
        fixture_schema["definitions"]["fulfillmentCase"]["properties"]["id"],
        observation_schema["definitions"]["observationPayload"]["properties"][
            "fulfillmentCaseOrder"
        ],
        observation_schema["definitions"]["fulfillmentRecord"]["properties"]["id"],
        fixture_schema["properties"]["precommitCaseOrder"],
        fixture_schema["definitions"]["precommitCase"]["properties"]["id"],
        observation_schema["definitions"]["observationPayload"]["properties"]["precommitCaseOrder"],
        observation_schema["definitions"]["precommitRecord"]["properties"]["id"],
    )
    assert all("const" not in shape and "enum" not in shape for shape in transaction_shapes)

    wrong_order = copy.deepcopy(fixture)
    wrong_order["transactionCaseOrder"] = list(reversed(wrong_order["transactionCaseOrder"]))
    validate_json(
        wrong_order,
        blacksmith_mithril.FIXTURE_SCHEMA,
        owner="schema-valid transaction order drift",
    )
    with pytest.raises(ValueError, match="transaction case order drift"):
        blacksmith_mithril._assert_golden(wrong_order, static)

    renamed = copy.deepcopy(fixture)
    renamed_id = "renamed-transaction-case"
    renamed["transactionCases"][0]["id"] = renamed_id
    renamed["transactionCaseOrder"][0] = renamed_id
    renamed["acceptedObservation"]["transactionCaseOrder"][0] = renamed_id
    renamed["acceptedObservation"]["transactionRecords"][0]["id"] = renamed_id
    validate_json(
        renamed,
        blacksmith_mithril.FIXTURE_SCHEMA,
        owner="schema-valid coordinated transaction ID rename",
    )
    with pytest.raises(ValueError, match="transaction case order drift"):
        blacksmith_mithril._assert_golden(renamed, static)


def test_blacksmith_schema_registry_is_closed_and_golden_free() -> None:
    paths = [
        blacksmith_mithril.repo_path("schemas/h3/observer-callback-contract.schema.json"),
        blacksmith_mithril.FAILURE_SCHEMA,
        blacksmith_mithril.OBSERVATION_SCHEMA,
        blacksmith_mithril.FIXTURE_SCHEMA,
    ]
    audit = schema_composition_audit(paths)
    assert audit["schemaCount"] == 4
    assert audit["unresolvedReferences"] == []
    assert audit["duplicateBodyGroups"] == []
    assert audit["largeConstCount"] == 0


def test_static_contract_derives_source_h1_rng_and_table_boundaries() -> None:
    static = _static(_fixture())
    assert static["function"] == {
        "entryAddress": 138966,
        "returnRtsAddress": 139104,
        "classSearchLoopAddress": 138980,
        "rowResolvedAddress": 139030,
        "rowLoopAddress": 139042,
        "loadIndexAddress": 139068,
        "orderLoopAddress": 139076,
        "orderNextAddress": 139090,
        "orderWriteAddress": 139084,
        "orderStrideAddress": 139090,
        "clientClassReadAddress": 138986,
        "fallbackRngCallAddress": 139014,
        "fallbackRngReturnAddress": 139018,
        "weaponRngCallAddress": 139052,
        "weaponRngReturnAddress": 139056,
        "rngEntryAddress": 5632,
        "rngReturnRtsAddress": 5670,
        "checkSramAddress": 28326,
    }
    assert static["ram"] == {
        "randomSeedAddress": 0xFFDEA4,
        "ordersAddress": 0xFFF7A8,
        "currentGoldAddress": 0xFFF600,
        "gameFlagsAddress": 0xFFF686,
        "flag80OwningByteAddress": 0xFFF690,
        "combatantDataAddress": 0xFFE800,
        "dialogueNameIndex1Address": 0xFFB6E8,
        "selectedItemIndexAddress": 0xFFB13A,
        "currentItemSubmenuActionAddress": 0xFFB13C,
    }
    assert static["constants"] == {
        "classGroupsCounter": 7,
        "weaponRowsCounter": 3,
        "weaponRowCount": 8,
        "orderSlotsCounter": 3,
        "orderSlotCount": 4,
        "orderSlotSize": 2,
        "clientClassOffset": -24,
        "brnClass": 16,
        "rdbnClass": 31,
        "orderCost": 5000,
        "mithrilItemIndex": 123,
        "itemNothingIndex": 127,
        "itemIndexMask": 127,
        "itemIndexAndBrokenMask": 32895,
        "weaponTypeMask": 2,
        "ringTypeMask": 4,
        "combatantEntrySizeBytes": 56,
        "combatantItemSlotCount": 4,
        "combatantClassOffsetBytes": 10,
        "combatantItemsOffsetBytes": 32,
        "flag80Id": 80,
        "flag80ByteOffset": 10,
        "flag80BitMask": 128,
        "equipmentTypeTool": 0,
        "equipmentTypeWeapon": 1,
        "equipmentTypeRing": 65535,
    }
    assert static["transaction"] | {"h1InstructionBytes": []} == {
        "placeEntryAddress": 138690,
        "decreaseGoldCallAddress": 138696,
        "decreaseGoldInstructionTargetAddress": 33120,
        "decreaseGoldEffectiveTargetAddress": 35252,
        "decreaseGoldEffectiveReturnAddress": 35276,
        "pendingOrdersIncrementAddress": 138702,
        "pendingOrdersIncrementedObserveAddress": 138708,
        "dropItemCallAddress": 138716,
        "dropItemInstructionTargetAddress": 33184,
        "dropItemEffectiveTargetAddress": 36370,
        "dropItemTailUpdateTargetAddress": 35278,
        "dropItemEffectiveReturnAddress": 35364,
        "pickMithrilCallAddress": 138722,
        "pickMithrilReturnAddress": 138726,
        "clearFlagCallAddress": 138730,
        "clearFlagInstructionTargetAddress": 33388,
        "clearFlagEffectiveTargetAddress": 39124,
        "clearFlagEffectiveReturnAddress": 39142,
        "prePresentationReturnAddress": 138736,
        "frameOffsetsBytes": {
            "clientClass": -24,
            "clientMember": -6,
            "itemSlot": -12,
            "pendingOrdersNumber": -14,
        },
        "h1InstructionBytes": [],
    }
    transaction_bytes = {
        instruction["text"]: instruction["romBytes"]
        for instruction in static["transaction"]["h1InstructionBytes"]
    }
    assert transaction_bytes["addi.w #1,pendingOrdersNumber(a6)"] == bytes.fromhex("066E0001FFF2")
    assert transaction_bytes["move.w clientMember(a6),d0"] == bytes.fromhex("302EFFFA")
    assert transaction_bytes["move.w itemSlot(a6),d1"] == bytes.fromhex("322EFFF4")
    assert transaction_bytes["move.w #80,d1"] == bytes.fromhex("323C0050")
    assert static["fulfillment"] | {
        "h1InstructionBytes": [],
        "itemDefinitionFields": [],
    } == {
        "addItemEntryAddress": 138212,
        "addItemCallAddress": 138220,
        "addItemReturnAddress": 138226,
        "addItemInstructionTargetAddress": 33176,
        "addItemEffectiveTargetAddress": 36002,
        "addItemEffectiveReturnAddress": 36050,
        "orderReadInstructionAddress": 138242,
        "orderReadObserveAddress": 138244,
        "orderClearAddress": 138244,
        "orderClearedObserveAddress": 138248,
        "fulfilledOrdersIncrementAddress": 138248,
        "fulfilledOrdersIncrementedObserveAddress": 138254,
        "equippabilityCallAddress": 138262,
        "equippabilityInstructionTargetAddress": 33204,
        "equippabilityEffectiveTargetAddress": 36736,
        "equippabilityEffectiveReturnAddress": 36762,
        "postEquippabilityReturnAddress": 138268,
        "updateCombatantStatsAddress": 35278,
        "updateCombatantStatsReached": False,
        "frameOffsetsBytes": {
            "clientClass": -24,
            "clientMember": -6,
            "itemIndex": -10,
            "ordersCounter": -22,
            "fulfilledOrdersNumber": -16,
        },
        "ordersCounterMinimum": 1,
        "ordersCounterMaximum": 4,
        "h1InstructionBytes": [],
        "itemDefinitionFields": [],
    }
    assert static["fulfillment"]["itemDefinitionFields"] == [
        {
            "itemIndex": 69,
            "equipFlagsAddress": 94966,
            "equipFlagsBytes": bytes.fromhex("00001000"),
            "itemTypeAddress": 94974,
            "itemTypeBytes": bytes.fromhex("8A"),
        },
        {
            "itemIndex": 99,
            "equipFlagsAddress": 95446,
            "equipFlagsBytes": bytes.fromhex("00080000"),
            "itemTypeAddress": 95454,
            "itemTypeBytes": bytes.fromhex("8A"),
        },
        {
            "itemIndex": 100,
            "equipFlagsAddress": 95462,
            "equipFlagsBytes": bytes.fromhex("000E0000"),
            "itemTypeAddress": 95470,
            "itemTypeBytes": bytes.fromhex("0A"),
        },
    ]
    assert static["precommit"] | {"h1InstructionBytes": []} == {
        "entryAddress": 138050,
        "selectionLoopAddress": 138072,
        "runtimeStartAddress": 138072,
        "doneAddress": 138452,
        "addItemEntryAddress": 138212,
        "memberList": {
            "callAddress": 138088,
            "instructionTargetAddress": 65604,
            "effectiveTargetAddress": 77828,
            "returnAddress": 138094,
        },
        "heldItems": {
            "callAddress": 138114,
            "instructionTargetAddress": 33140,
            "effectiveTargetAddress": 35834,
            "returnAddress": 138120,
        },
        "equipmentType": {
            "callAddress": 138160,
            "instructionTargetAddress": 33144,
            "effectiveTargetAddress": 35880,
            "returnAddress": 138166,
        },
        "equippability": {
            "callAddress": 138180,
            "instructionTargetAddress": 33204,
            "effectiveTargetAddress": 36736,
            "returnAddress": 138186,
        },
        "fullInventoryYesNo": {
            "callAddress": 138136,
            "instructionTargetAddress": 65652,
            "effectiveTargetAddress": 86668,
            "returnAddress": 138142,
        },
        "nonEquippableYesNo": {
            "callAddress": 138198,
            "instructionTargetAddress": 65652,
            "effectiveTargetAddress": 86668,
            "returnAddress": 138204,
        },
        "memberCancelCompareAddress": 138094,
        "memberCancelBranchAddress": 138098,
        "capacityCompareAddress": 138120,
        "capacityBranchAddress": 138124,
        "fullInventoryPromptCompareAddress": 138142,
        "fullInventoryRetryBranchAddress": 138146,
        "equipmentTypeCompareAddress": 138166,
        "toolAdmissionBranchAddress": 138170,
        "equippabilityBranchAddress": 138186,
        "nonEquippablePromptCompareAddress": 138204,
        "nonEquippableRetryBranchAddress": 138208,
        "presentationTrapAddresses": [
            138060,
            138064,
            138068,
            138072,
            138100,
            138132,
            138148,
            138194,
        ],
        "presentationTrapReturnAddresses": [
            138062,
            138066,
            138070,
            138074,
            138102,
            138134,
            138150,
            138196,
        ],
        "frameOffsetsBytes": {"clientMember": -6, "itemIndex": -10, "fulfilledOrdersNumber": -16},
        "cleanupEquippability": {
            "callAddress": 138262,
            "instructionTargetAddress": 33204,
            "effectiveTargetAddress": 36736,
            "effectiveReturnAddress": 36762,
            "returnAddress": 138268,
        },
        "serviceShims": [
            {
                "role": "member-list",
                "callAddress": 138088,
                "instructionTargetAddress": 65604,
                "effectiveTargetAddress": 77828,
                "returnAddress": 138094,
                "originalHex": "4EB900010044",
                "patchedHex": "4EB900FF6D00",
                "generatedStubTarget": 16739584,
            },
            {
                "role": "held-items",
                "callAddress": 138114,
                "instructionTargetAddress": 33140,
                "effectiveTargetAddress": 35834,
                "returnAddress": 138120,
                "originalHex": "4EB900008174",
                "patchedHex": "4EB900FF6D00",
                "generatedStubTarget": 16739584,
            },
            {
                "role": "equipment-type",
                "callAddress": 138160,
                "instructionTargetAddress": 33144,
                "effectiveTargetAddress": 35880,
                "returnAddress": 138166,
                "originalHex": "4EB900008178",
                "patchedHex": "4EB900FF6D00",
                "generatedStubTarget": 16739584,
            },
            {
                "role": "equippability",
                "callAddress": 138180,
                "instructionTargetAddress": 33204,
                "effectiveTargetAddress": 36736,
                "returnAddress": 138186,
                "originalHex": "4EB9000081B4",
                "patchedHex": "4EB900FF6D00",
                "generatedStubTarget": 16739584,
            },
        ],
        "terminalShims": [
            {
                "role": "recipient-cancel-terminal-boundary-shim",
                "type": "terminal-jmp",
                "boundaryAddress": 138100,
                "originalHex": "4E4500C56000",
                "patchedHex": "4EF900FF6D20",
                "generatedStubTarget": 16739616,
            },
            {
                "role": "full-inventory-terminal-boundary-shim",
                "type": "terminal-jmp",
                "boundaryAddress": 138132,
                "originalHex": "4E4500D04EB9",
                "patchedHex": "4EF900FF6D20",
                "generatedStubTarget": 16739616,
            },
            {
                "role": "non-equippable-terminal-boundary-shim",
                "type": "terminal-jmp",
                "boundaryAddress": 138194,
                "originalHex": "4E4500A74EB9",
                "patchedHex": "4EF900FF6D20",
                "generatedStubTarget": 16739616,
            },
        ],
        "h1InstructionBytes": [],
    }
    assert [choice["denominator"] for choice in static["model"]["weaponRows"][0]] == [
        16,
        8,
        4,
        1,
    ]


@pytest.mark.parametrize(
    ("old", "new"),
    (
        ("00021B68 4EB9 0001 0044", "00021B68 4EF9 0001 0044"),
        ("00021B68 4EB9 0001 0044", "00021B68 4EB9 0001 0046"),
    ),
)
def test_precommit_service_h1_call_opcode_and_target_drift_fail_before_runtime(
    old: str, new: str
) -> None:
    fixture = _fixture()
    listing = (blacksmith_mithril.UPSTREAM / blacksmith_mithril.LISTING_RELATIVE).read_text(
        encoding="utf-8"
    )
    with pytest.raises(ValueError, match="precommit H1 call instruction-target drift"):
        blacksmith_mithril.build_static_contract(
            fixture, blacksmith_mithril.UPSTREAM, listing_text=listing.replace(old, new, 1)
        )


def test_precommit_service_shim_config_rejects_opcode_target_return_and_duplicate_drift() -> None:
    fixture = _fixture()
    static = _static(fixture)
    config = blacksmith_mithril._observer_config(fixture, static)
    shims = config["precommit"]["serviceShims"]
    assert [shim["originalHex"] for shim in shims] == [
        "4EB900010044",
        "4EB900008174",
        "4EB900008178",
        "4EB9000081B4",
    ]
    assert {shim["patchedHex"] for shim in shims} == {"4EB900FF6D00"}
    assert {shim["generatedStubTarget"] for shim in shims} == {
        blacksmith_mithril.PRECOMMIT_SERVICE_STUB_ADDRESS
    }
    terminal_shims = config["precommit"]["terminalShims"]
    assert [shim["type"] for shim in terminal_shims] == ["terminal-jmp"] * 3
    assert {shim["patchedHex"] for shim in terminal_shims} == {"4EF900FF6D20"}
    assert {shim["generatedStubTarget"] for shim in terminal_shims} == {
        blacksmith_mithril.PRECOMMIT_TERMINAL_STUB_ADDRESS
    }
    assert config["precommitCaseFrameBudget"] == blacksmith_mithril.PRECOMMIT_CASE_FRAME_BUDGET
    assert config["precommitTransitionFrameBudget"] == (
        blacksmith_mithril.PRECOMMIT_TRANSITION_FRAME_BUDGET
    )
    assert config["precommitCleanupStackDepthBytes"] == 8
    assert config["precommit"]["cleanupEquippability"] == {
        "callAddress": 138262,
        "instructionTargetAddress": 33204,
        "effectiveTargetAddress": 36736,
        "effectiveReturnAddress": 36762,
        "returnAddress": 138268,
    }
    for field, value, error in (
        ("originalHex", "4EB900010046", "ABI drift"),
        ("patchedHex", "4EF900FF6D00", "ABI drift"),
        ("generatedStubTarget", blacksmith_mithril.PRECOMMIT_SERVICE_STUB_ADDRESS + 2, "ABI drift"),
        ("returnAddress", shims[0]["returnAddress"] + 2, "ABI drift"),
    ):
        malformed = copy.deepcopy(static)
        malformed["precommit"]["serviceShims"][0][field] = value
        with pytest.raises(ValueError, match=error):
            blacksmith_mithril._observer_config(fixture, malformed)
    duplicate = copy.deepcopy(static)
    duplicate["precommit"]["serviceShims"][1]["callAddress"] = duplicate["precommit"][
        "serviceShims"
    ][0]["callAddress"]
    duplicate["precommit"]["heldItems"]["callAddress"] = duplicate["precommit"]["serviceShims"][
        0
    ]["callAddress"]
    with pytest.raises(ValueError, match="overlapping call-site"):
        blacksmith_mithril._observer_config(fixture, duplicate)
    overlap = copy.deepcopy(static)
    overlap["precommit"]["serviceShims"][1]["callAddress"] = overlap["precommit"][
        "serviceShims"
    ][0]["callAddress"] + 2
    overlap["precommit"]["heldItems"]["callAddress"] = overlap["precommit"]["serviceShims"][0][
        "callAddress"
    ] + 2
    with pytest.raises(ValueError, match="overlapping call-site"):
        blacksmith_mithril._observer_config(fixture, overlap)
    terminal_overlap = copy.deepcopy(static)
    terminal_overlap["precommit"]["terminalShims"][0]["boundaryAddress"] = terminal_overlap[
        "precommit"
    ]["serviceShims"][0]["callAddress"]
    with pytest.raises(ValueError, match="overlapping span"):
        blacksmith_mithril._observer_config(fixture, terminal_overlap)

    for field, value, error in (
        ("callAddress", static["precommit"]["equippability"]["callAddress"], "reuses admission"),
        ("callAddress", 138264, "source/H1/ROM relation"),
        ("returnAddress", 138270, "source/H1/ROM relation"),
    ):
        malformed = copy.deepcopy(static)
        malformed["precommit"]["cleanupEquippability"][field] = value
        with pytest.raises(ValueError, match=error):
            blacksmith_mithril._observer_config(fixture, malformed)


def test_precommit_session_rom_instrumentation_is_exact_seven_span_copy(
    tmp_path: Path,
) -> None:
    fixture = _fixture()
    static = _static(fixture)
    canonical = blacksmith_mithril.repo_path("local/roms/sf2-us.bin")
    before = canonical.read_bytes()
    first = blacksmith_mithril._instrument_precommit_rom(
        canonical, fixture, static, output_path=tmp_path / "first.instrumented.bin"
    )
    second = blacksmith_mithril._instrument_precommit_rom(
        canonical, fixture, static, output_path=tmp_path / "second.instrumented.bin"
    )
    first_bytes = first.read_bytes()
    spans = blacksmith_mithril._precommit_instrumentation_spans(static)
    assert len(spans) == 7
    assert [row["type"] for row in spans] == ["service-jsr"] * 4 + ["terminal-jmp"] * 3
    assert hashlib.sha256(first_bytes).hexdigest() == hashlib.sha256(
        second.read_bytes()
    ).hexdigest()
    assert canonical.read_bytes() == before
    changed = {
        index
        for index, pair in enumerate(zip(before, first_bytes, strict=True))
        if pair[0] != pair[1]
    }
    expected_changed = {
        row["address"] + offset
        for row in spans
        for offset, pair in enumerate(zip(row["originalBytes"], row["patchedBytes"], strict=True))
        if pair[0] != pair[1]
    }
    assert changed == expected_changed
    assert {
        row["address"]
        for row in spans
        if before[row["address"] : row["address"] + 6]
        != first_bytes[row["address"] : row["address"] + 6]
    } == {row["address"] for row in spans}
    for row in spans:
        assert first_bytes[row["address"] : row["address"] + 6] == row["patchedBytes"]


def test_precommit_session_rom_instrumentation_preserves_retained_v3_add_item_entry() -> None:
    fixture = _fixture()
    static = _static(fixture)
    spans = blacksmith_mithril._precommit_instrumentation_spans(static)
    retained = blacksmith_mithril._retained_blacksmith_observation_pcs(static)
    add_item_entry = static["precommit"]["addItemEntryAddress"]
    assert add_item_entry == 138212
    assert add_item_entry in retained
    assert all(
        not (span["address"] <= add_item_entry < span["address"] + 6) for span in spans
    )
    malformed = [*spans, {**spans[-1], "role": "retained-v3-overlap", "address": add_item_entry}]
    with pytest.raises(ValueError, match="overlaps retained v3 observation PCs"):
        blacksmith_mithril._validate_precommit_retained_compatibility(static, malformed)


@pytest.mark.parametrize(
    ("mutation", "error"),
    (
        ("missing-cleanup", "cleanup fixture identity drift"),
        ("wrong-item", "cleanup item identity drift"),
        ("wrong-order", "cleanup order-word identity drift"),
    ),
)
def test_precommit_add_item_cleanup_linkage_is_rejected_before_config_or_launch(
    mutation: str, error: str
) -> None:
    fixture = _fixture()
    case = fixture["precommitCases"][2]
    if mutation == "missing-cleanup":
        case["cleanupFulfillmentCaseId"] = "missing-accepted-fulfillment-case"
    elif mutation == "wrong-item":
        case["itemIndex"] = 99
    elif mutation == "wrong-order":
        case["ordersBefore"][3] = 99
    else:
        raise AssertionError(f"uncovered mutation: {mutation}")
    static = _static(fixture)
    with pytest.raises(ValueError, match=error):
        blacksmith_mithril._observer_config(fixture, static)


def test_precommit_session_rom_instrumentation_rejects_base_and_patch_drift(
    tmp_path: Path,
) -> None:
    fixture = _fixture()
    static = _static(fixture)
    canonical = blacksmith_mithril.repo_path("local/roms/sf2-us.bin")
    wrong_base = tmp_path / "wrong-base.bin"
    base_bytes = bytearray(canonical.read_bytes())
    base_bytes[static["precommit"]["serviceShims"][0]["callAddress"]] ^= 1
    wrong_base.write_bytes(base_bytes)
    with pytest.raises(ValueError, match="canonical ROM manifest identity"):
        blacksmith_mithril._instrument_precommit_rom(wrong_base, fixture, static)
    for field, value, error in (
        ("originalHex", "4EB900010046", "ABI drift"),
        ("patchedHex", "4EF900FF6D00", "ABI drift"),
        (
            "generatedStubTarget",
            blacksmith_mithril.PRECOMMIT_SERVICE_STUB_ADDRESS + 2,
            "ABI drift",
        ),
        ("returnAddress", 138096, "ABI drift"),
    ):
        malformed = copy.deepcopy(static)
        malformed["precommit"]["serviceShims"][0][field] = value
        with pytest.raises(ValueError, match=error):
            blacksmith_mithril._instrument_precommit_rom(canonical, fixture, malformed)
    for field, value, error in (
        ("patchedHex", "4EB900FF6D20", "terminal shim ABI drift"),
        (
            "generatedStubTarget",
            blacksmith_mithril.PRECOMMIT_TERMINAL_STUB_ADDRESS + 2,
            "terminal shim ABI drift",
        ),
        ("originalHex", "4E4500C56001", "source call-site bytes drift"),
    ):
        malformed = copy.deepcopy(static)
        malformed["precommit"]["terminalShims"][0][field] = value
        with pytest.raises(ValueError, match=error):
            blacksmith_mithril._instrument_precommit_rom(canonical, fixture, malformed)


def test_static_contract_joins_client_frame_and_order_slot_abi_to_h1() -> None:
    fixture = _fixture()
    static = _static(fixture)
    pick_path = blacksmith_mithril.UPSTREAM / "disasm" / blacksmith_mithril.PICK_SOURCE_RELATIVE
    source = pick_path.read_text(encoding="utf-8")
    instructions = {
        instruction["address"]: instruction for instruction in static["h1"]["instructionBytes"]
    }
    assert "clientClass = -24" in source
    assert "move.w  clientClass(a6),d2" in source
    assert "move.w  #2,d0\n                adda.w  d0,a0" in source
    assert "move.w  d1,(a0)" in source
    assert instructions[static["function"]["clientClassReadAddress"]]["romBytes"] == bytes.fromhex(
        "342EFFE8"
    )
    assert instructions[static["function"]["orderStrideAddress"]]["romBytes"] == bytes.fromhex(
        "303C0002"
    )
    assert instructions[static["function"]["orderWriteAddress"]]["romBytes"] == bytes.fromhex(
        "3081"
    )


@pytest.mark.parametrize(
    ("mutation", "error"),
    (
        ("slot-size", "count/stride/write-width relation"),
        ("orders-counter", "count/stride/write-width relation"),
        ("max-orders", "count/stride/write-width relation"),
    ),
)
def test_order_slot_equate_topology_mutations_fail_before_runtime(
    mutation: str, error: str
) -> None:
    fixture = _fixture()
    root = blacksmith_mithril.repo_path("local/upstream/SF2DISASM")
    enums = (root / "disasm" / blacksmith_mithril.ENUMS_RELATIVE).read_text(encoding="utf-8")
    if mutation == "slot-size":
        enums = enums.replace(
            "MITHRIL_WEAPON_ORDER_SLOT_SIZE: equ 2",
            "MITHRIL_WEAPON_ORDER_SLOT_SIZE: equ 3",
            1,
        )
    elif mutation == "orders-counter":
        enums = enums.replace(
            "BLACKSMITH_ORDERS_COUNTER: equ 3", "BLACKSMITH_ORDERS_COUNTER: equ 4", 1
        )
    elif mutation == "max-orders":
        enums = enums.replace(
            "BLACKSMITH_MAX_ORDERS_NUMBER: equ 4", "BLACKSMITH_MAX_ORDERS_NUMBER: equ 5", 1
        )
    else:
        raise AssertionError(f"uncovered mutation: {mutation}")
    with pytest.raises(ValueError, match=error):
        blacksmith_mithril.build_static_contract(fixture, root, enums_source_text=enums)


def test_source_derived_order_slot_domain_rejects_fixture_array_drift_before_runtime() -> None:
    fixture = _fixture()
    fixture["cases"][0]["ordersBefore"].append(0)
    with pytest.raises(ValueError, match="is too long"):
        validate_json(
            fixture,
            blacksmith_mithril.FIXTURE_SCHEMA,
            owner="fixture order-slot drift",
        )


def test_independent_model_covers_all_required_runtime_roles() -> None:
    fixture = _fixture()
    observed = blacksmith_mithril.expected_observation(fixture, _static(fixture))
    records = observed["records"]
    assert [record["orderWriteIndex"] for record in records] == [0, 1, 2, 3, None]
    assert [record["choiceIndex"] for record in records] == [0, 3, 0, 0, 0]
    assert [record["classGroupIndex"] for record in records] == [0, 2, 8, 8, 0]
    assert [record["weaponRowIndex"] for record in records] == [0, 2, 2, 0, 0]
    assert [call["result"] for call in records[2]["rngCalls"]] == [0, 0]
    assert [call["result"] for call in records[3]["rngCalls"]] == [1, 0]
    assert [call["rangeWord"] for call in records[1]["rngCalls"]] == [16, 8, 4, 1]
    assert records[-1]["ordersAfter"] == fixture["cases"][-1]["ordersBefore"]
    transactions = observed["transactionRecords"]
    assert [record["orderWriteIndex"] for record in transactions] == [0, 2, 1]
    assert [record["itemSlot"] for record in transactions] == [0, 1, 3]
    assert [record["weaponRowIndex"] for record in transactions] == [3, 1, 2]
    assert [len(record["rngCalls"]) for record in transactions] == [1, 4, 2]
    assert all(len(record["callbackChronology"]) == 18 for record in transactions)
    assert all(
        record["callbackChronology"][5] == {"role": "pending-orders-incremented", "pc": 138708}
        for record in transactions
    )
    fulfillments = observed["fulfillmentRecords"]
    assert [record["selectedOrderIndex"] for record in fulfillments] == [3, 2, 0]
    assert [record["itemWriteIndex"] for record in fulfillments] == [3, 2, 0]
    assert [record["equippableCarrySet"] for record in fulfillments] == [True, True, False]
    assert [record["fulfilledOrdersAfter"] for record in fulfillments] == [1, 2, 3]
    assert all(len(record["callbackChronology"]) == 11 for record in fulfillments)
    assert all(record["safeExitOriginalReturnPc"] == 138268 for record in fulfillments)
    precommits = observed["precommitRecords"]
    assert [record["terminal"] for record in precommits] == [
        "recipient-cancel-pre-presentation",
        "full-inventory-pre-presentation",
        "add-item",
        "add-item",
        "non-equippable-pre-presentation",
    ]
    assert [record["attemptCount"] for record in precommits] == [1, 1, 1, 1, 1]
    assert [record["selectedMember"] for record in precommits] == [None, 0, 3, 4, 5]
    assert [len(record["callbackChronology"]) for record in precommits] == [
        8,
        13,
        17,
        21,
        22,
    ]
    assert all(
        not record[mutation]
        for record in precommits
        for mutation in (
            "addItemMutationObserved",
            "orderMutationObserved",
            "fulfilledOrdersMutationObserved",
        )
    )
    assert observed["precommitRestoration"] == {
        "dialogueNameIndex1WordRestored": True,
        "selectedItemIndexWordRestored": True,
        "currentItemSubmenuActionByteRestored": True,
    }
    assert observed["precommitInstrumentation"] == {
        "serviceCallSitesReadback": [
            "member-list",
            "held-items",
            "equipment-type",
            "equippability",
        ],
        "terminalBoundarySitesReadback": [
            "recipient-cancel-terminal-boundary-shim",
            "full-inventory-terminal-boundary-shim",
            "non-equippable-terminal-boundary-shim",
        ],
        "generatedServiceStubWritesReadback": True,
        "generatedResultStubWritesReadback": True,
    }
    assert observed["restoration"] == {
        "currentGoldLongRestored": True,
        "randomSeedWordRestored": True,
        "orderWordsRestored": True,
        "flag80OwningByteRestored": True,
        "clientCombatantRecordsRestored": True,
    }


def test_v3_accepted_results_are_byte_for_byte_preserved_inside_v4() -> None:
    fixture = _fixture()
    observed = fixture["acceptedObservation"]
    v3_keys = (
        "caseOrder",
        "records",
        "transactionCaseOrder",
        "transactionRecords",
        "fulfillmentCaseOrder",
        "fulfillmentRecords",
        "callbacksCleared",
        "restoration",
    )
    preserved = {key: observed[key] for key in v3_keys}
    digest = hashlib.sha256(
        json.dumps(preserved, sort_keys=True, separators=(",", ":")).encode("utf-8")
    ).hexdigest()
    assert digest == "072e34831c982cbaa97ae9aff419ee6fd2e2b28380495d9d6d588bc579c981b4"


def test_research_index_binds_only_observed_fulfillment_pcs() -> None:
    fixture = _fixture()
    source_context = fixture["sourceContext"]
    accepted = fixture["acceptedObservation"]
    index = load_json(blacksmith_mithril.repo_path("manifests/research-index.json"))
    record = next(row for row in index["records"] if row["id"] == "menus.blacksmith-actions")
    addresses = {row["id"]: row["value"] for row in record["addresses"]}
    h3 = next(row for row in record["evidence"] if row["level"] == "H3")
    bindings = {row["addressId"]: row["fixtureField"] for row in h3["bindings"]}
    observed_pcs = {
        event["pc"]
        for records_key in (
            "records",
            "transactionRecords",
            "fulfillmentRecords",
            "precommitRecords",
        )
        for case in accepted[records_key]
        for event in case.get("callbackChronology", [])
    }

    assert bindings == {
        "place-order": "sourceContext.placeEntryAddress",
        "fulfill-selection-loop": "sourceContext.fulfillSelectionLoopAddress",
        "fulfill-add-item": "sourceContext.fulfillAddItemEntryAddress",
    }
    assert {
        addresses["fulfill-selection-loop"],
        addresses["fulfill-add-item"],
    } <= observed_pcs
    assert all(
        any(
            event["role"] == "precommit-selection-loop-entry"
            and event["pc"] == source_context["fulfillSelectionLoopAddress"]
            for event in case["callbackChronology"]
        )
        for case in accepted["precommitRecords"]
    )
    assert "fulfill-entry" not in addresses
    assert "fulfill-done" not in addresses
    assert "sourceContext.fulfillEntryAddress" not in bindings.values()
    assert "sourceContext.fulfillDoneAddress" not in bindings.values()
    assert source_context["fulfillEntryAddress"] not in observed_pcs
    assert source_context["fulfillDoneAddress"] not in observed_pcs


@pytest.mark.parametrize(
    ("mutation", "error"),
    (
        ("orders-counter-zero", "ordersCounter is outside source domain"),
        ("orders-counter-five", "ordersCounter is outside source domain"),
        ("order-item-mismatch", "target order/item mismatch"),
        ("order-already-empty", "target order is already empty"),
        ("full-inventory", "first ITEM_NOTHING inventory slot"),
        ("carry-drift", "item/class carry relation drift"),
    ),
)
def test_fulfillment_matrix_rejects_invalid_direct_entry_states(mutation: str, error: str) -> None:
    fixture = _fixture()
    case = fixture["fulfillmentCases"][0]
    if mutation == "orders-counter-zero":
        case["ordersCounter"] = 0
    elif mutation == "orders-counter-five":
        case["ordersCounter"] = 5
    elif mutation == "order-item-mismatch":
        case["ordersBefore"][3] = 99
    elif mutation == "order-already-empty":
        case["ordersBefore"][3] = 0
    elif mutation == "full-inventory":
        case["clientItemWordsBefore"] = [1, 2, 3, 4]
    elif mutation == "carry-drift":
        case["equippableCarrySet"] = False
    else:
        raise AssertionError(f"uncovered mutation: {mutation}")
    with pytest.raises(ValueError, match=error):
        blacksmith_mithril.model_fulfillment_case(case, _static(fixture))


def test_fulfillment_source_h1_and_item_definition_mutations_fail_before_runtime() -> None:
    fixture = _fixture()
    root = blacksmith_mithril.repo_path("local/upstream/SF2DISASM")
    disasm = root / "disasm"
    actions = (disasm / blacksmith_mithril.BLACKSMITH_ACTIONS_RELATIVE).read_text(encoding="utf-8")
    item_source = (disasm / blacksmith_mithril.ITEM_SOURCE_RELATIVE).read_text(encoding="utf-8")
    itemdefs = (disasm / blacksmith_mithril.ITEMDEFS_SOURCE_RELATIVE).read_text(encoding="utf-8")
    listing = (root / blacksmith_mithril.LISTING_RELATIVE).read_text(encoding="utf-8")
    mutations = (
        (
            {"actions_source_text": actions.replace("ordersCounter = -22", "ordersCounter = -20")},
            "fulfillment source/H1 ABI chronology drift",
        ),
        (
            {
                "actions_source_text": actions.replace(
                    "addi.w  #1,fulfilledOrdersNumber(a6)",
                    "addi.w  #2,fulfilledOrdersNumber(a6)",
                    1,
                )
            },
            "source guard drift",
        ),
        (
            {"item_source_text": item_source.replace("move.w  d1,-(a0)", "move.w  d0,-(a0)", 1)},
            "source guard drift",
        ),
        (
            {
                "item_source_text": item_source.replace(
                    "beq.s   @Break\n                dbf     d0,@Loop",
                    "bne.s   @Break\n                dbf     d0,@Loop",
                    1,
                )
            },
            "source guard drift",
        ),
        (
            {
                "item_source_text": item_source.replace(
                    "move.w  #1,d2           ; no empty slot available\n"
                    "                bra.s   @Done",
                    "move.w  #1,d2           ; no empty slot available\n"
                    "                bra.s   @Break",
                    1,
                )
            },
            "source guard drift",
        ),
        (
            {
                "itemdefs_source_text": itemdefs.replace(
                    "; 69: Levanter\n                equipFlags   HERO",
                    "; 69: Levanter\n                equipFlags   VICR",
                    1,
                )
            },
            "item/class carry relation drift",
        ),
        (
            {
                "listing_text": listing.replace(
                    "00021BEC 4EB9 0000 8198", "00021BEC 4EB9 0000 819A", 1
                )
            },
            "fulfillment source/H1 ABI chronology drift",
        ),
        (
            {
                "listing_text": listing.replace(
                    "00021C16 4EB9 0000 81B4", "00021C18 4EB9 0000 81B4", 1
                )
            },
            "fulfillment source/H1 ABI chronology drift",
        ),
        (
            {
                "listing_text": listing.replace(
                    "00021C16 4EB9 0000 81B4", "00021C16 4EB8 81B4", 1
                )
            },
            "fulfillment source/H1 ABI chronology drift",
        ),
        (
            {
                "listing_text": listing.replace(
                    "00021C1C 6400 0000", "00021C1E 6400 0000", 1
                )
            },
            "fulfillment source/H1 ABI chronology drift",
        ),
        (
            {
                "listing_text": listing.replace(
                    "00008CBA 6700                                       beq.s   @Break",
                    "00008CBA 6700                                       beq.s   @Done",
                    1,
                )
            },
            "H1 instruction missing: beq.s @Break",
        ),
        (
            {
                "listing_text": listing.replace(
                    "00008CC4 6000                                       bra.s   @Done",
                    "00008CC4 6100                                       bra.s   @Done",
                    1,
                )
            },
            "fulfillment source/H1 ABI chronology drift",
        ),
        (
            {"listing_text": listing.replace("00008F8C 1028 000A", "00008F8C 1028 FF0A", 1)},
            "fulfillment source/H1 ABI chronology drift",
        ),
        (
            {"listing_text": listing.replace("00008F8C 1028 000A", "00008F8C 1028 000B", 1)},
            "fulfillment source/H1 ABI chronology drift",
        ),
        (
            {"listing_text": listing.replace("00008F8C 1028 000A", "00008F8C 1029 000A", 1)},
            "class displacement opcode/width drift",
        ),
        (
            {"listing_text": listing.replace("00008F8C 1028 000A", "00008F8C 1028", 1)},
            "class displacement opcode/width drift",
        ),
    )
    for kwargs, error in mutations:
        with pytest.raises(ValueError, match=error):
            blacksmith_mithril.build_static_contract(fixture, root, **kwargs)


def test_fulfillment_schema_shape_is_structural_but_model_owns_ids_and_order() -> None:
    fixture = _fixture()
    static = _static(fixture)
    wrong_order = copy.deepcopy(fixture)
    wrong_order["fulfillmentCaseOrder"] = list(reversed(wrong_order["fulfillmentCaseOrder"]))
    validate_json(
        wrong_order,
        blacksmith_mithril.FIXTURE_SCHEMA,
        owner="schema-valid fulfillment order drift",
    )
    with pytest.raises(ValueError, match="fulfillment case order drift"):
        blacksmith_mithril._assert_golden(wrong_order, static)
    renamed = copy.deepcopy(fixture)
    renamed_id = "renamed-fulfillment-case"
    renamed["fulfillmentCases"][0]["id"] = renamed_id
    renamed["fulfillmentCaseOrder"][0] = renamed_id
    renamed["acceptedObservation"]["fulfillmentCaseOrder"][0] = renamed_id
    renamed["acceptedObservation"]["fulfillmentRecords"][0]["id"] = renamed_id
    validate_json(
        renamed,
        blacksmith_mithril.FIXTURE_SCHEMA,
        owner="schema-valid coordinated fulfillment ID rename",
    )
    with pytest.raises(ValueError, match="fulfillment case order drift"):
        blacksmith_mithril._assert_golden(renamed, static)


@pytest.mark.parametrize(
    ("mutation", "error"),
    (
        ("fallback-polarity", "source guard drift"),
        ("fallback-bound", "source guard drift"),
        ("order-write-operand", "source guard drift"),
        ("rng-range-register", "source guard drift"),
        ("rng-seed-register", "source guard drift"),
        ("weighted-denominator", "denominator source order drift"),
        ("h1-call-target", "H1 instruction missing"),
        ("client-class-offset", "client-class.*offset drift"),
        ("h1-client-class-offset", "client-class source/H1 offset drift"),
        ("h1-order-stride", "order-slot source/H1 stride drift"),
    ),
)
def test_source_and_h1_mutations_fail_before_runtime(mutation: str, error: str) -> None:
    fixture = _fixture()
    root = blacksmith_mithril.repo_path("local/upstream/SF2DISASM")
    disasm = root / "disasm"
    pick = (disasm / blacksmith_mithril.PICK_SOURCE_RELATIVE).read_text(encoding="utf-8")
    table = (disasm / blacksmith_mithril.TABLE_SOURCE_RELATIVE).read_text(encoding="utf-8")
    listing = (root / blacksmith_mithril.LISTING_RELATIVE).read_text(encoding="utf-8")
    rng_source = (root / "disasm" / blacksmith_mithril.RNG_SOURCE_RELATIVE).read_text(
        encoding="utf-8"
    )
    kwargs: dict[str, str] = {}
    if mutation == "fallback-polarity":
        kwargs["pick_source_text"] = pick.replace(
            "bne.w   @GetWeaponsEntryAddress", "beq.w   @GetWeaponsEntryAddress", 1
        )
    elif mutation == "fallback-bound":
        kwargs["pick_source_text"] = pick.replace("move.w  #2,d6", "move.w  #3,d6", 1)
    elif mutation == "order-write-operand":
        kwargs["pick_source_text"] = pick.replace("move.w  d1,(a0)", "move.w  d0,(a0)", 1)
    elif mutation == "rng-range-register":
        kwargs["pick_source_text"] = pick.replace("move.w  d0,d6", "move.w  d0,d5", 1)
    elif mutation == "rng-seed-register":
        kwargs["rng_source_text"] = rng_source.replace(
            "move.w  (RANDOM_SEED).l,d7", "move.w  (RANDOM_SEED).l,d6", 1
        )
    elif mutation == "weighted-denominator":
        kwargs["table_source_text"] = table.replace(
            "mithrilWeapons 16, LEVANTER", "mithrilWeapons 15, LEVANTER", 1
        )
    elif mutation == "h1-call-target":
        kwargs["listing_text"] = listing.replace(
            "00021F06 4EB8 1600                                  jsr     (GenerateRandomNumber).w",
            "00021F06 4EB8 1600                                  jsr     (GetRandomNumber).w",
            1,
        )
    elif mutation == "client-class-offset":
        kwargs["pick_source_text"] = pick.replace("clientClass = -24", "clientClass = -22", 1)
    elif mutation == "h1-client-class-offset":
        kwargs["listing_text"] = listing.replace("00021EEA 342E FFE8", "00021EEA 342E FFEA", 1)
    elif mutation == "h1-order-stride":
        kwargs["listing_text"] = listing.replace("00021F52 303C 0002", "00021F52 303C 0003", 1)
    else:
        raise AssertionError(f"uncovered mutation: {mutation}")
    with pytest.raises(ValueError, match=error):
        blacksmith_mithril.build_static_contract(fixture, root, **kwargs)


def test_source_comment_near_miss_does_not_satisfy_parser_guard() -> None:
    fixture = _fixture()
    root = blacksmith_mithril.repo_path("local/upstream/SF2DISASM")
    source = (root / "disasm" / blacksmith_mithril.PICK_SOURCE_RELATIVE).read_text(encoding="utf-8")
    source = source.replace("move.w  d1,(a0)", "move.w  d0,(a0)", 1)
    source += "\n; move.w d1,(a0)\n"
    with pytest.raises(ValueError, match="source guard drift"):
        blacksmith_mithril.build_static_contract(fixture, root, pick_source_text=source)


@pytest.mark.parametrize(
    ("source_name", "old", "new", "error"),
    (
        (
            "actions_source_text",
            "addi.w  #1,pendingOrdersNumber(a6)",
            "addi.w  #2,pendingOrdersNumber(a6)",
            "post-confirmation block",
        ),
        (
            "actions_source_text",
            "jsr     j_DropItemBySlot\n                bsr.w   PickMithrilWeapon",
            "bsr.w   PickMithrilWeapon\n                jsr     j_DropItemBySlot",
            "post-confirmation block",
        ),
        (
            "actions_source_text",
            "move.w  #80,d1",
            "move.w  #81,d1",
            "source/H2 readiness-flag relation",
        ),
        (
            "actions_source_text",
            "clientMember = -6",
            "clientMember = -8",
            "H1 frame/immediate chronology",
        ),
        (
            "actions_source_text",
            "itemSlot = -12",
            "itemSlot = -10",
            "H1 frame/immediate chronology",
        ),
        (
            "actions_source_text",
            "pendingOrdersNumber = -14",
            "pendingOrdersNumber = -16",
            "H1 frame/immediate chronology",
        ),
        (
            "actions_source_text",
            "clientClass = -24",
            "clientClass = -22",
            "action/picker client-class frame offset",
        ),
        (
            "gold_source_text",
            "move.l  d0,((CURRENT_GOLD-$1000000)).w",
            "move.l  d0,((CURRENT_GOLD-$1000000)).l",
            "DecreaseGold",
        ),
        ("item_source_text", "bra.w   UpdateCombatantStats", "rts", "DropItemBySlot"),
        ("flag_source_text", "and.b   d0,(a0)", "or.b    d0,(a0)", "ClearFlag"),
        ("flag_source_text", "andi.l  #FLAG_MASK,d1", "andi.l  #$03FF,d1", "GetFlag"),
        ("combatant_source_text", "sub.w   d1,d0", "add.w   d1,d0", "GetCombatantEntryAddress"),
    ),
)
def test_transaction_source_mutations_fail_before_runtime(
    source_name: str, old: str, new: str, error: str
) -> None:
    fixture = _fixture()
    root = blacksmith_mithril.repo_path("local/upstream/SF2DISASM")
    relative = {
        "actions_source_text": blacksmith_mithril.BLACKSMITH_ACTIONS_RELATIVE,
        "gold_source_text": blacksmith_mithril.GOLD_SOURCE_RELATIVE,
        "item_source_text": blacksmith_mithril.ITEM_SOURCE_RELATIVE,
        "flag_source_text": blacksmith_mithril.FLAG_SOURCE_RELATIVE,
        "combatant_source_text": blacksmith_mithril.COMBATANT_SOURCE_RELATIVE,
    }[source_name]
    source = (root / "disasm" / relative).read_text(encoding="utf-8")
    assert old in source
    if source_name == "item_source_text":
        start = source.index("DropItemBySlot:")
        source = source[:start] + source[start:].replace(old, new, 1)
    elif source_name == "actions_source_text" and " = -" in old:
        start = source.rfind("; ===============", 0, source.index("BlacksmithAction_PlaceOrder:"))
        source = source[:start] + source[start:].replace(old, new, 1)
    elif source_name == "actions_source_text" and old == "move.w  #80,d1":
        start = source.index("@PlaceOrder:")
        source = source[:start] + source[start:].replace(old, new, 1)
    else:
        source = source.replace(old, new, 1)
    with pytest.raises(ValueError, match=error):
        blacksmith_mithril.build_static_contract(fixture, root, **{source_name: source})


def test_transaction_h1_mutation_fails_before_runtime() -> None:
    fixture = _fixture()
    root = blacksmith_mithril.repo_path("local/upstream/SF2DISASM")
    listing = (root / blacksmith_mithril.LISTING_RELATIVE).read_text(encoding="utf-8")
    mutated = listing.replace(
        "00021DEA 4EB9 0000 826C                             jsr     j_ClearFlag",
        "00021DEA 4EB9 0000 826C                             jsr     j_SetFlag",
        1,
    )
    with pytest.raises(ValueError, match="transaction H1 instruction missing"):
        blacksmith_mithril.build_static_contract(fixture, root, listing_text=mutated)


@pytest.mark.parametrize(
    ("old", "new"),
    (
        ("00021DCE 066E 0001 FFF2", "00021DCE 066E 0001 FFF0"),
        ("00021DD4 302E FFFA", "00021DD4 302E FFF8"),
        ("00021DD8 322E FFF4", "00021DD8 322E FFF6"),
        ("00021DE6 323C 0050", "00021DE6 323C 0051"),
    ),
)
def test_transaction_h1_frame_displacement_and_flag_immediate_mutations_fail(
    old: str, new: str
) -> None:
    fixture = _fixture()
    root = blacksmith_mithril.repo_path("local/upstream/SF2DISASM")
    listing = (root / blacksmith_mithril.LISTING_RELATIVE).read_text(encoding="utf-8")
    assert old in listing
    with pytest.raises(ValueError, match="H1 frame/immediate chronology"):
        blacksmith_mithril.build_static_contract(
            fixture, root, listing_text=listing.replace(old, new, 1)
        )


def test_action_frame_near_miss_comment_does_not_satisfy_local_parser() -> None:
    fixture = _fixture()
    root = blacksmith_mithril.repo_path("local/upstream/SF2DISASM")
    source = (root / "disasm" / blacksmith_mithril.BLACKSMITH_ACTIONS_RELATIVE).read_text(
        encoding="utf-8"
    )
    start = source.rfind("; ===============", 0, source.index("BlacksmithAction_PlaceOrder:"))
    mutated = source[:start] + source[start:].replace("clientMember = -6", "; clientMember = -6", 1)
    with pytest.raises(ValueError, match="frame declaration drift: clientMember"):
        blacksmith_mithril.build_static_contract(fixture, root, actions_source_text=mutated)


@pytest.mark.parametrize(
    ("mutation", "error"),
    (
        ("few-groups", "searchable class-group"),
        ("many-groups", "searchable class-group"),
        ("few-rows", "weapon-row/class-group"),
        ("many-rows", "weapon-row/class-group"),
    ),
)
def test_source_table_topology_rejects_group_and_row_cardinality_drift(
    mutation: str, error: str
) -> None:
    fixture = _fixture()
    root = blacksmith_mithril.repo_path("local/upstream/SF2DISASM")
    table = (root / "disasm" / blacksmith_mithril.TABLE_SOURCE_RELATIVE).read_text(encoding="utf-8")
    if mutation == "few-groups":
        table = table.replace("                classes MMNK\n", "", 1)
    elif mutation == "many-groups":
        table = table.replace(
            "                classes BRN, RDBN\n",
            "                classes HERO\n                classes BRN, RDBN\n",
            1,
        )
    elif mutation == "few-rows":
        table = table.rsplit("                ; 7: MMNK", 1)[0]
    elif mutation == "many-rows":
        table += (
            "\n                mithrilWeapons 16, LEVANTER, &\n"
            "                               8, COUNTER_SWORD, &\n"
            "                               4, BATTLE_SWORD, &\n"
            "                               1, CRITICAL_SWORD\n"
        )
    else:
        raise AssertionError(f"uncovered mutation: {mutation}")
    with pytest.raises(ValueError, match=error):
        blacksmith_mithril.build_static_contract(fixture, root, table_source_text=table)


def test_cross_owner_and_source_context_drift_fail_before_runtime(tmp_path: Path) -> None:
    fixture = _fixture()
    root = blacksmith_mithril.repo_path("local/upstream/SF2DISASM")
    item_owner = load_json(blacksmith_mithril.ITEM_OWNER)
    item_owner["summary"]["mithrilWeaponRowCount"] -= 1
    with pytest.raises(ValueError, match="table-count drift"):
        blacksmith_mithril.build_static_contract(
            fixture,
            root,
            item_owner_path=_write(tmp_path, "item-owner.json", item_owner),
        )
    common_owner = load_json(blacksmith_mithril.COMMON_MENUS_OWNER)
    common_owner["romSha256"] = "0" * 64
    with pytest.raises(ValueError, match="provenance disagrees"):
        blacksmith_mithril.build_static_contract(
            fixture,
            root,
            common_menus_path=_write(tmp_path, "common-menus-owner.json", common_owner),
        )
    for owner_name, field, value in (
        ("commonStats", "fixture", "tests/fixtures/h2/not-common-stats.json"),
        ("coreStatsData", "fixtureId", "sf2-core-stats-static-v9"),
    ):
        wrong_owner = copy.deepcopy(fixture)
        wrong_owner["provenance"]["owners"][owner_name][field] = value
        validate_json(
            wrong_owner,
            blacksmith_mithril.FIXTURE_SCHEMA,
            owner=f"schema-valid {owner_name} {field} owner drift",
        )
        with pytest.raises(ValueError, match=f"{owner_name} owner identity drift"):
            blacksmith_mithril.build_static_contract(wrong_owner, root)
    common_stats = load_json(blacksmith_mithril.COMMON_STATS_OWNER)
    common_stats["romSha256"] = "0" * 64
    with pytest.raises(ValueError, match="provenance disagrees"):
        blacksmith_mithril.build_static_contract(
            fixture,
            root,
            common_stats_path=_write(tmp_path, "common-stats-rom-owner.json", common_stats),
        )
    core_stats_data = load_json(blacksmith_mithril.CORE_STATS_DATA_OWNER)
    core_stats_data["upstreamCommit"] = "0" * 40
    with pytest.raises(ValueError, match="provenance disagrees"):
        blacksmith_mithril.build_static_contract(
            fixture,
            root,
            core_stats_data_path=_write(
                tmp_path, "core-stats-data-commit-owner.json", core_stats_data
            ),
        )
    common_stats = load_json(blacksmith_mithril.COMMON_STATS_OWNER)
    common_stats["function"]["itemStatsAddress"] += 2
    with pytest.raises(ValueError, match="common-stats itemstats H1 owner drift"):
        blacksmith_mithril.build_static_contract(
            fixture,
            root,
            common_stats_path=_write(tmp_path, "common-stats-address-owner.json", common_stats),
        )
    common_stats = load_json(blacksmith_mithril.COMMON_STATS_OWNER)
    common_stats["expected"]["representativeSymbols"]["itemstats.asm"] = "GetItemType"
    with pytest.raises(ValueError, match="common-stats itemstats H1 owner drift"):
        blacksmith_mithril.build_static_contract(
            fixture,
            root,
            common_stats_path=_write(tmp_path, "common-stats-symbol-owner.json", common_stats),
        )
    core_stats_data = load_json(blacksmith_mithril.CORE_STATS_DATA_OWNER)
    core_stats_data["table"]["table_ItemDefinitions"] += 2
    with pytest.raises(ValueError, match="core-stats item-definition source/H1 domain drift"):
        blacksmith_mithril.build_static_contract(
            fixture,
            root,
            core_stats_data_path=_write(
                tmp_path, "core-stats-data-table-owner.json", core_stats_data
            ),
        )
    core_stats_data = load_json(blacksmith_mithril.CORE_STATS_DATA_OWNER)
    core_stats_data["expected"]["facts"]["items"]["definitionCount"] -= 1
    with pytest.raises(ValueError, match="core-stats item-definition source/H1 domain drift"):
        blacksmith_mithril.build_static_contract(
            fixture,
            root,
            core_stats_data_path=_write(
                tmp_path, "core-stats-data-count-owner.json", core_stats_data
            ),
        )
    for field, value in (
        ("entryAddress", 0x1602),
        ("observeAddress", 0x1624),
        ("seedAddress", 0xFFDEA6),
        ("rangeRegister", "M68K D5"),
        ("seedRegister", "M68K D6"),
    ):
        rng_owner = load_json(blacksmith_mithril.RNG_OWNER)
        rng_owner["function"][field] = value
        with pytest.raises(ValueError, match="RNG owner/source/H1 ABI join drift"):
            blacksmith_mithril.build_static_contract(
                fixture,
                root,
                rng_owner_path=_write(tmp_path, f"rng-owner-{field}.json", rng_owner),
            )
    wrong_context = copy.deepcopy(fixture)
    wrong_context["sourceContext"]["functionEntryAddress"] += 2
    with pytest.raises(ValueError, match="source-context identity drift"):
        blacksmith_mithril.build_static_contract(wrong_context, root)
    wrong_context = copy.deepcopy(fixture)
    wrong_context["sourceContext"]["placeEntryAddress"] += 2
    with pytest.raises(ValueError, match="place-order source-context drift"):
        blacksmith_mithril.build_static_contract(wrong_context, root)
    for field, value in (
        ("upstreamRepository", "https://example.invalid/SF2DISASM"),
        ("upstreamBranch", "alternate"),
        ("upstreamCommit", "0" * 40),
    ):
        coordinated = copy.deepcopy(fixture)
        coordinated["provenance"][field] = value
        validate_json(
            coordinated,
            blacksmith_mithril.FIXTURE_SCHEMA,
            owner=f"schema-valid coordinated {field} drift",
        )
        with pytest.raises(ValueError, match="provenance disagrees"):
            blacksmith_mithril.build_static_contract(coordinated, root)


def test_rom_guard_rejects_opcode_and_table_mutation_before_observer(tmp_path: Path) -> None:
    fixture = _fixture()
    static = _static(fixture)
    guarded = (
        static["h1"]["instructionBytes"]
        + static["transaction"]["h1InstructionBytes"]
        + static["fulfillment"]["h1InstructionBytes"]
        + static["precommit"]["h1InstructionBytes"]
    )
    size = max(instruction["address"] + len(instruction["bytes"]) for instruction in guarded) + 1
    size = max(size, static["h1"]["weaponTableAddress"] + len(static["h1"]["weaponTableBytes"]))
    size = max(
        size,
        max(
            field["itemTypeAddress"] + len(field["itemTypeBytes"])
            for field in static["fulfillment"]["itemDefinitionFields"]
        ),
    )
    rom = bytearray(size)
    for instruction in guarded:
        address = instruction["address"]
        rom[address : address + len(instruction["romBytes"])] = instruction["romBytes"]
    for address_key, bytes_key in (
        ("classTableAddress", "classTableBytes"),
        ("weaponTableAddress", "weaponTableBytes"),
    ):
        address = static["h1"][address_key]
        payload = static["h1"][bytes_key]
        rom[address : address + len(payload)] = payload
    for field in static["fulfillment"]["itemDefinitionFields"]:
        rom[
            field["equipFlagsAddress"] : field["equipFlagsAddress"] + len(field["equipFlagsBytes"])
        ] = field["equipFlagsBytes"]
        rom[field["itemTypeAddress"] : field["itemTypeAddress"] + len(field["itemTypeBytes"])] = (
            field["itemTypeBytes"]
        )
    image = tmp_path / "guard.bin"
    image.write_bytes(rom)
    blacksmith_mithril.validate_static_contract(
        fixture, image, blacksmith_mithril.repo_path("local/upstream/SF2DISASM")
    )
    clean = bytes(rom)
    transaction_instruction = {
        instruction["text"]: instruction["address"]
        for instruction in static["transaction"]["h1InstructionBytes"]
    }
    fulfillment_instruction = {
        instruction["text"]: instruction["address"]
        for instruction in static["fulfillment"]["h1InstructionBytes"]
    }
    precommit_instruction = {
        instruction["text"]: instruction["address"]
        for instruction in static["precommit"]["h1InstructionBytes"]
    }
    for address in (
        static["function"]["fallbackRngCallAddress"],
        static["function"]["clientClassReadAddress"],
        static["function"]["orderStrideAddress"],
        static["function"]["orderWriteAddress"],
        static["transaction"]["decreaseGoldCallAddress"],
        static["transaction"]["dropItemEffectiveReturnAddress"],
        static["transaction"]["clearFlagEffectiveReturnAddress"],
        static["fulfillment"]["addItemCallAddress"],
        static["fulfillment"]["addItemEffectiveReturnAddress"],
        static["fulfillment"]["equippabilityEffectiveReturnAddress"],
        static["precommit"]["memberList"]["callAddress"],
        static["precommit"]["heldItems"]["returnAddress"],
        static["precommit"]["equipmentType"]["returnAddress"],
        static["precommit"]["equippabilityBranchAddress"],
        precommit_instruction["bne.w byte_21B58"],
        fulfillment_instruction["beq.s @Break"],
        fulfillment_instruction["bra.s @Done"],
        fulfillment_instruction["move.b COMBATANT_OFFSET_CLASS(a0),d0"],
        transaction_instruction["addi.w #1,pendingOrdersNumber(a6)"],
        transaction_instruction["move.w clientMember(a6),d0"],
        transaction_instruction["move.w itemSlot(a6),d1"],
        transaction_instruction["move.w #80,d1"],
        static["fulfillment"]["itemDefinitionFields"][0]["equipFlagsAddress"],
        static["fulfillment"]["itemDefinitionFields"][2]["itemTypeAddress"],
    ):
        corrupted = bytearray(clean)
        corrupted[address] ^= 1
        image.write_bytes(corrupted)
        with pytest.raises(ValueError, match="guard drift"):
            blacksmith_mithril.validate_static_contract(
                fixture, image, blacksmith_mithril.repo_path("local/upstream/SF2DISASM")
            )


def _failure_payload() -> dict[str, object]:
    return {
        "owner": "blacksmith-mithril",
        "caseId": "brn-fallback-zero-row2-slot2",
        "phase": "rng-entry",
        "role": "rng-entry",
        "actualPc": 5632,
        "expectedEventPc": 5632,
        "expectedCallPc": 139014,
        "expectedTargetPc": 5632,
        "expectedReturnPc": 139018,
        "expectedStackTop": None,
        "actualStackTop": None,
        "expectedStackReturn": None,
        "actualStackReturn": None,
        "callbacksRemaining": 0,
        "sessionStateRestored": True,
        "outputRemoved": True,
        "pendingCallback": {
            "active": True,
            "caseIndex": 3,
            "functionReturnSeen": False,
            "orderWriteSeen": False,
            "pendingRngCall": {
                "role": "fallback-row-roll",
                "callPc": 139014,
                "targetPc": 5632,
                "returnPc": 139018,
                "rangeWord": 2,
            },
            "rolesAtPc": ["rng-entry"],
            "transaction": {
                "active": False,
                "mode": "helper",
                "decreaseGoldReturnSeen": False,
                "pendingOrdersIncrementSeen": False,
                "dropItemReturnSeen": False,
                "pickReturnSeen": False,
                "clearFlagReturnSeen": False,
                "prePresentationReturnAddress": None,
            },
            "fulfillment": {
                "active": False,
                "mode": "helper",
                "addItemReturnSeen": False,
                "orderReadSeen": False,
                "orderClearedSeen": False,
                "fulfilledOrdersIncrementSeen": False,
                "equippabilityCarrySet": None,
                "originalReturnAddress": None,
            },
            "precommit": {
                "active": False,
                "attemptIndex": 0,
                "equipmentTypeCallCount": 0,
                "equippabilityCallCount": 0,
                "expectedTerminal": "none",
                "frameBudget": 0,
                "frameCount": 0,
                "heldItemsCallCount": 0,
                "memberListCallCount": 0,
                "mode": "helper",
                "pendingService": None,
                "selectedMember": None,
                "terminal": "none",
            },
        },
        "error": "RNG entry PC drift",
    }


def _registration_failure_payload() -> dict[str, object]:
    return {
        "owner": "blacksmith-mithril",
        "caseId": None,
        "phase": "registration",
        "role": "registration",
        "actualPc": None,
        "expectedEventPc": None,
        "expectedCallPc": None,
        "expectedTargetPc": None,
        "expectedReturnPc": None,
        "expectedStackTop": None,
        "actualStackTop": None,
        "expectedStackReturn": None,
        "actualStackReturn": None,
        "callbacksRemaining": 0,
        "sessionStateRestored": False,
        "outputRemoved": True,
        "pendingCallback": {
            "active": False,
            "caseIndex": 0,
            "functionReturnSeen": False,
            "orderWriteSeen": False,
            "pendingRngCall": None,
            "rolesAtPc": [],
            "transaction": {
                "active": False,
                "mode": "none",
                "decreaseGoldReturnSeen": False,
                "pendingOrdersIncrementSeen": False,
                "dropItemReturnSeen": False,
                "pickReturnSeen": False,
                "clearFlagReturnSeen": False,
                "prePresentationReturnAddress": None,
            },
            "fulfillment": {
                "active": False,
                "mode": "none",
                "addItemReturnSeen": False,
                "orderReadSeen": False,
                "orderClearedSeen": False,
                "fulfilledOrdersIncrementSeen": False,
                "equippabilityCarrySet": None,
                "originalReturnAddress": None,
            },
            "precommit": {
                "active": False,
                "attemptIndex": 0,
                "equipmentTypeCallCount": 0,
                "equippabilityCallCount": 0,
                "expectedTerminal": "none",
                "frameBudget": 0,
                "frameCount": 0,
                "heldItemsCallCount": 0,
                "memberListCallCount": 0,
                "mode": "none",
                "pendingService": None,
                "selectedMember": None,
                "terminal": "none",
            },
        },
        "error": "probe registration write drift",
    }


def _bootstrap_failure_payload() -> dict[str, object]:
    return {
        "owner": "blacksmith-mithril",
        "caseId": None,
        "phase": "bootstrap-return-redirect",
        "role": "bootstrap-return-redirect",
        "actualPc": 28326,
        "expectedEventPc": 28326,
        "expectedCallPc": None,
        "expectedTargetPc": 28326,
        "expectedReturnPc": 0xFF6800,
        "expectedStackTop": None,
        "actualStackTop": None,
        "expectedStackReturn": None,
        "actualStackReturn": None,
        "callbacksRemaining": 0,
        "sessionStateRestored": False,
        "outputRemoved": True,
        "pendingCallback": {
            "active": False,
            "caseIndex": 0,
            "functionReturnSeen": False,
            "orderWriteSeen": False,
            "pendingRngCall": None,
            "rolesAtPc": ["bootstrap-check-sram"],
            "transaction": {
                "active": False,
                "mode": "none",
                "decreaseGoldReturnSeen": False,
                "pendingOrdersIncrementSeen": False,
                "dropItemReturnSeen": False,
                "pickReturnSeen": False,
                "clearFlagReturnSeen": False,
                "prePresentationReturnAddress": None,
            },
            "fulfillment": {
                "active": False,
                "mode": "none",
                "addItemReturnSeen": False,
                "orderReadSeen": False,
                "orderClearedSeen": False,
                "fulfilledOrdersIncrementSeen": False,
                "equippabilityCarrySet": None,
                "originalReturnAddress": None,
            },
            "precommit": {
                "active": False,
                "attemptIndex": 0,
                "equipmentTypeCallCount": 0,
                "equippabilityCallCount": 0,
                "expectedTerminal": "none",
                "frameBudget": 0,
                "frameCount": 0,
                "heldItemsCallCount": 0,
                "memberListCallCount": 0,
                "mode": "none",
                "pendingService": None,
                "selectedMember": None,
                "terminal": "none",
            },
        },
        "error": "CheckSram return redirect write drift",
    }


def _observer_role_sets(source: str) -> tuple[set[str], set[str]]:
    registered_roles = set(re.findall(r'register_exec\([^,]+,"([^"]+)"', source))
    watchdog_roles = set(
        re.findall(r'set_expectation\("precommit(?:-watchdog|-transition)","([^"]+)"', source)
    )
    failure_roles = {"registration", "bootstrap-return-redirect"} | watchdog_roles | (
        registered_roles - {"bootstrap-check-sram"}
    )
    return failure_roles, registered_roles


def test_observer_role_literals_exhaust_shared_failure_and_pending_enums() -> None:
    source = blacksmith_mithril.OBSERVER.read_text(encoding="utf-8")
    failure_roles, registered_roles = _observer_role_sets(source)
    shared = load_json(
        blacksmith_mithril.repo_path("schemas/h3/observer-callback-contract.schema.json")
    )
    failure_enum = set(
        shared["definitions"]["blacksmithMithrilFailure"]["properties"]["role"]["enum"]
    )
    pending_enum = set(
        shared["definitions"]["blacksmithMithrilPendingCallback"]["properties"]["rolesAtPc"][
            "items"
        ]["enum"]
    )
    assert failure_roles == failure_enum
    assert registered_roles == pending_enum
    assert "registration" in failure_roles and "registration" not in registered_roles
    assert "precommit-watchdog-timeout" in failure_roles
    assert "precommit-watchdog-timeout" not in registered_roles
    assert "precommit-transition-timeout" in failure_roles
    assert "precommit-transition-timeout" not in registered_roles

    _, renamed_pending = _observer_role_sets(
        source.replace(
            'register_exec(f.rngEntryAddress,"rng-entry",0)',
            'register_exec(f.rngEntryAddress,"rng-entry-renamed",0)',
            1,
        )
    )
    assert renamed_pending != pending_enum
    for role in failure_enum:
        assert f'"{role}"' in source

    missing_role = _failure_payload()
    del missing_role["role"]
    with pytest.raises(ValueError, match="required property"):
        validate_json(missing_role, blacksmith_mithril.FAILURE_SCHEMA, owner="missing role")
    renamed_role = _failure_payload()
    renamed_role["role"] = "rng-entry-renamed"
    with pytest.raises(ValueError, match="is not one of"):
        validate_json(renamed_role, blacksmith_mithril.FAILURE_SCHEMA, owner="renamed role")
    extra_pending_role = _failure_payload()
    extra_pending_role["pendingCallback"]["rolesAtPc"].append("rng-entry-renamed")
    with pytest.raises(ValueError, match="is not one of"):
        validate_json(
            extra_pending_role,
            blacksmith_mithril.FAILURE_SCHEMA,
            owner="extra pending role",
        )


def test_registration_and_bootstrap_failures_have_no_case_association(tmp_path: Path) -> None:
    for payload in (_registration_failure_payload(), _bootstrap_failure_payload()):
        validate_json(payload, blacksmith_mithril.FAILURE_SCHEMA, owner="inactive failure")
        status = tmp_path / f"{payload['role']}.status.txt"
        status.write_text(
            "milestone:observer-loaded\n"
            + blacksmith_mithril.STATUS_PREFIX
            + json.dumps(payload)
            + "\n",
            encoding="utf-8",
        )
        assert blacksmith_mithril._failure_diagnostic(status) is not None
    wrong_registration = _registration_failure_payload()
    wrong_registration["caseId"] = "ordinary-group0-early-slot0"
    with pytest.raises(ValueError):
        validate_json(
            wrong_registration,
            blacksmith_mithril.FAILURE_SCHEMA,
            owner="registration case leak",
        )
    wrong_bootstrap = _bootstrap_failure_payload()
    wrong_bootstrap["pendingCallback"]["caseIndex"] = 1
    with pytest.raises(ValueError):
        validate_json(
            wrong_bootstrap,
            blacksmith_mithril.FAILURE_SCHEMA,
            owner="bootstrap case leak",
        )
    source = blacksmith_mithril.OBSERVER.read_text(encoding="utf-8")
    assert (
        'mode,helper_index,transaction_index,fulfillment_index,precommit_index="none",0,0,0,0'
        in source
    )
    assert 'current_role=="transaction-case-entry"' in source
    assert 'current_role=="registration" and nil or emu.getregister("M68K PC")' in source
    bootstrap = source.index("local function bootstrap_check_sram()")
    probe_write = source.index("write_probe();helper_index=1;bootstrapped=true", bootstrap)
    first_case_entry = source.index('register_exec(entry,"case-entry",index)', bootstrap)
    assert probe_write < first_case_entry


def test_precommit_callback_failure_preserves_generated_return_abi_and_pending_state() -> None:
    payload = _failure_payload()
    payload.update(
        {
            "caseId": "tool-direct-add-item-admission",
            "phase": "precommit",
            "role": "precommit-member-list-original-return",
            "actualPc": 138094,
            "expectedEventPc": 138094,
            "expectedCallPc": 138088,
            "expectedTargetPc": blacksmith_mithril.PRECOMMIT_SERVICE_STUB_ADDRESS,
            "expectedReturnPc": 138094,
        }
    )
    pending = payload["pendingCallback"]
    pending["caseIndex"] = 4
    pending["rolesAtPc"] = [
        "precommit-member-list-original-return",
        "precommit-member-cancel-compare",
    ]
    pending["transaction"]["mode"] = "precommit"
    pending["fulfillment"]["mode"] = "precommit"
    pending["precommit"] = {
        "active": True,
        "attemptIndex": 1,
        "equipmentTypeCallCount": 0,
        "equippabilityCallCount": 0,
        "expectedTerminal": "add-item",
        "frameBudget": blacksmith_mithril.PRECOMMIT_CASE_FRAME_BUDGET,
        "frameCount": 1,
        "heldItemsCallCount": 0,
        "memberListCallCount": 1,
        "mode": "precommit",
        "pendingService": {
            "callPc": 138088,
            "returnPc": 138094,
            "role": "member-list",
                "targetPc": blacksmith_mithril.PRECOMMIT_SERVICE_STUB_ADDRESS,
        },
        "selectedMember": None,
        "terminal": "none",
    }
    validate_json(payload, blacksmith_mithril.FAILURE_SCHEMA, owner="precommit failure")
    payload["pendingCallback"]["precommit"]["pendingService"]["targetPc"] ^= 2
    validate_json(
        payload,
        blacksmith_mithril.FAILURE_SCHEMA,
        owner="schema-valid precommit target drift",
    )


def test_precommit_cleanup_failure_preserves_stack_and_terminal_cleanup_facts(
    tmp_path: Path,
) -> None:
    payload = _failure_payload()
    payload.update(
        {
            "caseId": "tool-direct-add-item-admission",
            "phase": "precommit-cleanup",
            "role": "precommit-cleanup-equippability-effective-return",
            "actualPc": 36762,
            "expectedEventPc": 36762,
            "expectedCallPc": 138262,
            "expectedTargetPc": 36736,
            "expectedReturnPc": 138268,
            "expectedStackTop": 0xFFFEF8,
            "actualStackTop": 0xFFFEF8,
            "expectedStackReturn": 138268,
            "actualStackReturn": 0xFF6B54,
            "callbacksRemaining": 0,
            "sessionStateRestored": True,
            "outputRemoved": True,
            "error": "precommit cleanup safe-return stack relation drift",
        }
    )
    pending = payload["pendingCallback"]
    pending["caseIndex"] = 3
    pending["rolesAtPc"] = [
        "fulfillment-equippability-effective-return",
        "precommit-cleanup-equippability-effective-return",
    ]
    pending["transaction"]["mode"] = "precommit-cleanup"
    pending["fulfillment"]["mode"] = "precommit-cleanup"
    pending["precommit"] = {
        "active": True,
        "attemptIndex": 1,
        "equipmentTypeCallCount": 1,
        "equippabilityCallCount": 0,
        "expectedTerminal": "add-item",
        "frameBudget": blacksmith_mithril.PRECOMMIT_CASE_FRAME_BUDGET,
        "frameCount": 0,
        "heldItemsCallCount": 1,
        "memberListCallCount": 1,
        "mode": "precommit-cleanup",
        "pendingService": None,
        "selectedMember": 3,
        "terminal": "add-item",
    }
    validate_json(payload, blacksmith_mithril.FAILURE_SCHEMA, owner="precommit cleanup failure")
    status = tmp_path / "blacksmith-mithril.status.txt"
    status.write_text(
        "milestone:precommit-cases-entered\n"
        + blacksmith_mithril.STATUS_PREFIX
        + json.dumps(payload)
        + "\n",
        encoding="utf-8",
    )
    diagnostic = blacksmith_mithril._failure_diagnostic(status)
    assert diagnostic is not None
    for expected in (
        '"phase": "precommit-cleanup"',
        '"role": "precommit-cleanup-equippability-effective-return"',
        '"expectedCallPc": 138262',
        '"expectedTargetPc": 36736',
        '"expectedReturnPc": 138268',
        '"actualStackReturn": 16739156',
        '"callbacksRemaining": 0',
        '"sessionStateRestored": true',
        '"outputRemoved": true',
    ):
        assert expected in diagnostic
    for field in ("actualStackReturn", "actualStackTop"):
        malformed = copy.deepcopy(payload)
        del malformed[field]
        with pytest.raises(ValueError, match="required property"):
            validate_json(
                malformed,
                blacksmith_mithril.FAILURE_SCHEMA,
                owner=f"missing cleanup {field}",
            )
    malformed_mode = copy.deepcopy(payload)
    malformed_mode["pendingCallback"]["precommit"]["mode"] = "precommit-cleanup-typo"
    with pytest.raises(ValueError, match="is not one of"):
        validate_json(
            malformed_mode,
            blacksmith_mithril.FAILURE_SCHEMA,
            owner="unknown cleanup mode",
        )
    malformed_role = copy.deepcopy(payload)
    malformed_role["role"] = "precommit-cleanup-equippability-return-typo"
    with pytest.raises(ValueError, match="is not one of"):
        validate_json(
            malformed_role,
            blacksmith_mithril.FAILURE_SCHEMA,
            owner="unknown cleanup role",
        )
    malformed_cleanup = copy.deepcopy(payload)
    malformed_cleanup["callbacksRemaining"] = 1
    with pytest.raises(ValueError, match="was expected"):
        validate_json(
            malformed_cleanup,
            blacksmith_mithril.FAILURE_SCHEMA,
            owner="residual cleanup callback",
        )
    malformed_pending = copy.deepcopy(payload)
    malformed_pending["pendingCallback"]["fulfillment"]["mode"] = "fulfillment"
    with pytest.raises(ValueError, match="precommit-cleanup"):
        validate_json(
            malformed_pending,
            blacksmith_mithril.FAILURE_SCHEMA,
            owner="malformed cleanup pending state",
        )
    malformed_terminal = copy.deepcopy(payload)
    malformed_terminal["pendingCallback"]["precommit"]["terminal"] = "none"
    with pytest.raises(ValueError, match="add-item"):
        validate_json(
            malformed_terminal,
            blacksmith_mithril.FAILURE_SCHEMA,
            owner="cleanup pending terminal drift",
        )
    malformed_output = copy.deepcopy(payload)
    malformed_output["outputRemoved"] = False
    with pytest.raises(ValueError, match="was expected"):
        validate_json(
            malformed_output,
            blacksmith_mithril.FAILURE_SCHEMA,
            owner="failure output residue",
        )
    malformed_restore = copy.deepcopy(payload)
    malformed_restore["sessionStateRestored"] = "true"
    with pytest.raises(ValueError, match="is not of type"):
        validate_json(
            malformed_restore,
            blacksmith_mithril.FAILURE_SCHEMA,
            owner="failure restoration type drift",
        )
    source = blacksmith_mithril.OBSERVER.read_text(encoding="utf-8")
    assert "local q=p.cleanupEquippability" in source
    assert "stack_top-config.precommitCleanupStackDepthBytes" in source
    assert "pcx.cleanupStackDiagnostic={expectedTop=expected_top" in source
    assert "actualReturn=actual_return" in source
    assert "memory.write_u32_be(stack,target,\"M68K BUS\")" in source


def test_precommit_watchdog_timeout_is_structured_and_cannot_pass(tmp_path: Path) -> None:
    payload = _failure_payload()
    payload.update(
        {
            "caseId": "recipient-cancel-pre-presentation",
            "phase": "precommit-watchdog",
            "role": "precommit-watchdog-timeout",
            "actualPc": 3836,
            "expectedEventPc": 3836,
            "expectedCallPc": None,
            "expectedTargetPc": None,
            "expectedReturnPc": None,
            "error": "precommit case frame budget exhausted before terminal",
        }
    )
    pending = payload["pendingCallback"]
    pending["caseIndex"] = 1
    pending["rolesAtPc"] = []
    pending["transaction"]["mode"] = "precommit"
    pending["fulfillment"]["mode"] = "precommit"
    pending["precommit"] = {
        "active": True,
        "attemptIndex": 1,
        "equipmentTypeCallCount": 0,
        "equippabilityCallCount": 0,
        "expectedTerminal": "recipient-cancel-pre-presentation",
        "frameBudget": blacksmith_mithril.PRECOMMIT_CASE_FRAME_BUDGET,
        "frameCount": blacksmith_mithril.PRECOMMIT_CASE_FRAME_BUDGET + 1,
        "heldItemsCallCount": 0,
        "memberListCallCount": 0,
        "mode": "precommit",
        "pendingService": None,
        "selectedMember": None,
        "terminal": "none",
    }
    validate_json(payload, blacksmith_mithril.FAILURE_SCHEMA, owner="watchdog failure")
    status = tmp_path / "blacksmith-mithril.status.txt"
    status.write_text(
        "milestone:precommit-cases-entered\n"
        + blacksmith_mithril.STATUS_PREFIX
        + json.dumps(payload)
        + "\n",
        encoding="utf-8",
    )
    assert blacksmith_mithril._failure_diagnostic(status) is not None
    source = blacksmith_mithril.OBSERVER.read_text(encoding="utf-8")
    assert "if pcx.frameCount>pcx.frameBudget then" in source
    assert 'set_expectation("precommit-watchdog","precommit-watchdog-timeout"' in source
    assert 'fail_callback("precommit case frame budget exhausted before terminal")' in source
    assert "os.remove(config.outputPath);cleanup_session()" in source
    assert source.index("os.remove(config.outputPath)") < source.index("status(diagnostic)")
    failure_start = source.index("local function fail_callback")
    failure_handler = source[failure_start : source.index("expect=function", failure_start)]
    assert "local restored,restore_message=pcall(restore_all)" in failure_handler
    assert failure_handler.index("pcall(restore_all)") < failure_handler.index(
        "os.remove(config.outputPath)"
    )
    assert "actualStackTop" in failure_handler
    assert "actualStackReturn" in failure_handler
    assert "callbacksRemaining" in failure_handler
    assert "sessionStateRestored" in failure_handler
    assert "outputRemoved" in failure_handler
    assert "restore_precommit" not in source
    assert "write_bytes(shim.callAddress" not in source
    assert "write_bytes(shim.boundaryAddress" not in source
    assert "memory.write_u16_be(shim.callAddress" not in source
    assert "memory.write_u16_be(shim.boundaryAddress" not in source
    assert "memory.write_u16_be(address,0x4EF9" not in source
    assert "precommitCallSitesAndStubsRestored" not in source
    assert "precommitExitBytesRestored" not in source
    assert "validate_precommit_service_call" in source
    assert "validate_precommit_terminal_boundary" in source


def test_precommit_transition_watchdog_is_structured_and_cannot_fall_to_external_timeout(
    tmp_path: Path,
) -> None:
    payload = _failure_payload()
    payload.update(
        {
            "caseId": "recipient-cancel-pre-presentation",
            "phase": "precommit-transition",
            "role": "precommit-transition-timeout",
            "actualPc": 0x548,
            "expectedEventPc": 0xFF6B00,
            "expectedCallPc": None,
            "expectedTargetPc": 0xFF6B00,
            "expectedReturnPc": None,
            "error": (
                "precommit transition frame budget exhausted before first generated case entry"
            ),
        }
    )
    pending = payload["pendingCallback"]
    pending["caseIndex"] = 1
    pending["rolesAtPc"] = []
    pending["transaction"]["mode"] = "none"
    pending["fulfillment"]["mode"] = "none"
    pending["precommit"] = {
        "active": False,
        "attemptIndex": 0,
        "equipmentTypeCallCount": 0,
        "equippabilityCallCount": 0,
        "expectedTerminal": "none",
        "frameBudget": 0,
        "frameCount": 0,
        "heldItemsCallCount": 0,
        "memberListCallCount": 0,
        "mode": "none",
        "pendingService": None,
        "selectedMember": None,
        "terminal": "none",
    }
    validate_json(payload, blacksmith_mithril.FAILURE_SCHEMA, owner="transition watchdog failure")
    status = tmp_path / "blacksmith-mithril.status.txt"
    status.write_text(
        "milestone:fulfillment-cases-entered\n"
        "milestone:precommit-cases-entered\n"
        + blacksmith_mithril.STATUS_PREFIX
        + json.dumps(payload)
        + "\n",
        encoding="utf-8",
    )
    assert blacksmith_mithril._failure_diagnostic(status) is not None
    source = blacksmith_mithril.OBSERVER.read_text(encoding="utf-8")
    assert "if pcx.transition.active then" in source
    assert "if pcx.transition.frameCount>pcx.transition.frameBudget then" in source
    assert 'set_expectation("precommit-transition","precommit-transition-timeout"' in source
    assert (
        'fail_callback("precommit transition frame budget exhausted before first '
        'generated case entry")'
        in source
    )
    assert "os.remove(config.outputPath);cleanup_session()" in source
    assert source.index("os.remove(config.outputPath)") < source.index("status(diagnostic)")


def test_callback_failure_schema_status_promotion_and_dispatcher_shape(tmp_path: Path) -> None:
    payload = _failure_payload()
    validate_json(payload, blacksmith_mithril.FAILURE_SCHEMA, owner="blacksmith failure")
    status = tmp_path / "blacksmith-mithril.status.txt"
    status.write_text(
        "milestone:direct-function-probe\n"
        + blacksmith_mithril.STATUS_PREFIX
        + json.dumps(payload)
        + "\n",
        encoding="utf-8",
    )
    assert blacksmith_mithril._failure_diagnostic(status) is not None
    with pytest.raises(RuntimeError, match="observer callback failure"):
        blacksmith_mithril._assert_status(status)
    status.write_text(
        "milestone:direct-function-probe\n"
        + blacksmith_mithril.STATUS_PREFIX
        + json.dumps(_failure_payload())
        + "\nlate-observer-row\n",
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="terminal exact failure line"):
        blacksmith_mithril._failure_diagnostic(status)
    status.write_text(
        "milestone:direct-function-probe\n"
        + blacksmith_mithril.STATUS_PREFIX
        + json.dumps(_failure_payload())
        + "\n"
        + blacksmith_mithril.STATUS_PREFIX
        + json.dumps(_failure_payload())
        + "\n",
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="multiplicity"):
        blacksmith_mithril._failure_diagnostic(status)
    status.write_text(
        "malformed " + blacksmith_mithril.STATUS_PREFIX + json.dumps(_failure_payload()) + "\n",
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="status line drift"):
        blacksmith_mithril._failure_diagnostic(status)
    payload["pendingCallback"]["rolesAtPc"] = ["wrong-role"]
    with pytest.raises(ValueError, match="failed schema validation"):
        validate_json(payload, blacksmith_mithril.FAILURE_SCHEMA, owner="blacksmith failure")

    source = blacksmith_mithril.OBSERVER.read_text(encoding="utf-8")
    assert source.count("event.on_bus_exec(function()") == 1
    assert "if not callbacks[address] then" in source
    assert "for _,entry in ipairs(callbacks[address]) do dispatch(address,entry) end" in source
    assert "local ok,message=pcall" in source
    assert "fail_callback(message)" in source
    assert "milestone:transaction-state-restored" in source
    assert "milestone:fulfillment-cases-entered" in source
    assert "milestone:precommit-cases-entered" in source
    assert "milestone:callbacks-cleared:0" in source
    assert "milestone:observer-finished" in source
    assert "f.returnRtsAddress" in source
    assert "f.checkSramAddress" in source
    assert "frame_base-c.clientClassOffset" in source
    assert "original_gold,original_seed,original_orders,original_flag,original_records" in source
    assert "c.combatantEntrySizeBytes" in source
    assert "original_return==t.prePresentationReturnAddress" in source
    assert "stack==stack_top-4" in source
    assert "memory.write_u32_be(stack,transaction_pc(transaction_index)+20" in source
    assert "memory.write_u32_be(stack,fulfillment_pc(fulfillment_index)+20" in source
    service_shim = source[
        source.index("local function validate_precommit_service_call") : source.index(
            "local function validate_precommit_terminal_boundary"
        )
    ]
    assert "0x4E and patched[2]==0xB9" in service_shim
    assert "0x4EF9" not in service_shim
    assert "precommit instrumented service call readback drift" in service_shim
    assert "local function write_precommit_service_stub" in source
    assert "local precommit_state={serviceStub=0xFF6D00,terminalStub=0xFF6D20" in source
    assert "precommitInstrumentation" in source
    assert "precommitCallSitesAndStubsRestored" not in source
    assert "precommitExitBytesRestored" not in source
    assert 'expect(terminal==pcx.expectedTerminal,"precommit terminal outcome drift")' in source
    precommit_block = source[
        source.index("local function pcx_selection_loop") : source.index(
            "local function bootstrap_check_sram"
        )
    ]
    assert "setregister" not in precommit_block
    assert "frame_base+24" not in source
    assert "order_write_seen==(#differences==1)" in source
    assert source.index("write_probe();helper_index=1;bootstrapped=true") < source.index(
        'status("milestone:direct-function-probe")'
    )


def test_verifier_uses_one_launch_and_omits_golden_output_from_lua_config(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    fixture = _fixture()
    static = _static(fixture)
    observed = blacksmith_mithril.expected_observation(fixture, static)
    launches: list[dict[str, object]] = []
    monkeypatch.setattr(blacksmith_mithril, "verify_runtime_contract", lambda *_: None)
    monkeypatch.setattr(blacksmith_mithril, "validate_static_contract", lambda *_: static)
    monkeypatch.setattr(blacksmith_mithril, "_assert_status", lambda *_: None)
    session_rom = tmp_path / "blacksmith-mithril.session.instrumented.bin"
    session_rom.write_bytes(b"disposable session ROM")
    monkeypatch.setattr(blacksmith_mithril, "_instrument_precommit_rom", lambda *_: session_rom)
    monkeypatch.setattr(
        blacksmith_mithril, "_with_instrumented_rom_database", lambda _, __, action: action()
    )
    monkeypatch.setattr(
        blacksmith_mithril, "run_observer", lambda **kwargs: launches.append(kwargs) or observed
    )
    result = blacksmith_mithril.verify_blacksmith_mithril(
        tmp_path / "input.bin", tmp_path, timeout_seconds=1
    )
    assert len(launches) == 1
    assert launches[0]["output_name"] == "blacksmith-mithril"
    assert "acceptedObservation" not in launches[0]["config"]
    assert "transactionCases" in launches[0]["config"]
    assert "transaction" in launches[0]["config"]
    assert "fulfillmentCases" in launches[0]["config"]
    assert "fulfillment" in launches[0]["config"]
    assert "precommitCases" in launches[0]["config"]
    assert "precommit" in launches[0]["config"]
    assert "h1InstructionBytes" not in launches[0]["config"]["transaction"]
    assert "h1InstructionBytes" not in launches[0]["config"]["fulfillment"]
    assert "itemDefinitionFields" not in launches[0]["config"]["fulfillment"]
    assert "h1InstructionBytes" not in launches[0]["config"]["precommit"]
    assert "fullInventoryYesNo" not in launches[0]["config"]["precommit"]
    assert "nonEquippableYesNo" not in launches[0]["config"]["precommit"]
    assert "fullInventoryRetryBranchAddress" not in launches[0]["config"]["precommit"]
    assert "nonEquippableRetryBranchAddress" not in launches[0]["config"]["precommit"]
    assert "precommitInstrumentation" not in launches[0]["config"]
    assert not session_rom.exists()
    assert result == {
        "Fixture": "sf2-blacksmith-mithril-runtime-v4",
        "Cases": 16,
        "HelperCases": 5,
        "TransactionCases": 3,
        "FulfillmentCases": 3,
        "PrecommitCases": 5,
        "BizHawkLaunches": 1,
        "CallbacksCleared": 0,
        "Restoration": {
            "currentGoldLongRestored": True,
            "randomSeedWordRestored": True,
            "orderWordsRestored": True,
            "flag80OwningByteRestored": True,
            "clientCombatantRecordsRestored": True,
        },
        "Status": "PASS",
    }


def test_verifier_promotes_terminal_structured_callback_failure(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    fixture = _fixture()
    static = _static(fixture)
    derived = tmp_path / "derived"
    derived.mkdir()
    payload = _failure_payload()
    payload.update(
        {
            "caseId": "tool-direct-add-item-admission",
            "phase": "precommit-cleanup",
            "role": "precommit-cleanup-equippability-effective-return",
            "actualPc": 36762,
            "expectedEventPc": 36762,
            "expectedCallPc": 138262,
            "expectedTargetPc": 36736,
            "expectedReturnPc": 138268,
            "expectedStackTop": 0xFFFEF8,
            "actualStackTop": 0xFFFEF8,
            "expectedStackReturn": 138268,
            "actualStackReturn": 0xFF6B54,
            "error": "precommit cleanup safe-return stack relation drift",
        }
    )
    pending = payload["pendingCallback"]
    pending["caseIndex"] = 3
    pending["rolesAtPc"] = ["precommit-cleanup-equippability-effective-return"]
    pending["transaction"]["mode"] = "precommit-cleanup"
    pending["fulfillment"]["mode"] = "precommit-cleanup"
    pending["precommit"] = {
        "active": True,
        "attemptIndex": 1,
        "equipmentTypeCallCount": 1,
        "equippabilityCallCount": 0,
        "expectedTerminal": "add-item",
        "frameBudget": blacksmith_mithril.PRECOMMIT_CASE_FRAME_BUDGET,
        "frameCount": 0,
        "heldItemsCallCount": 1,
        "memberListCallCount": 1,
        "mode": "precommit-cleanup",
        "pendingService": None,
        "selectedMember": 3,
        "terminal": "add-item",
    }
    (derived / "blacksmith-mithril.status.txt").write_text(
        "milestone:observer-loaded\n"
        + blacksmith_mithril.STATUS_PREFIX
        + json.dumps(payload)
        + "\n",
        encoding="utf-8",
    )
    monkeypatch.setattr(blacksmith_mithril, "DERIVED_ROOT", derived)
    monkeypatch.setattr(blacksmith_mithril, "verify_runtime_contract", lambda *_: None)
    monkeypatch.setattr(blacksmith_mithril, "validate_static_contract", lambda *_: static)
    monkeypatch.setattr(
        blacksmith_mithril,
        "_instrument_precommit_rom",
        lambda *_: tmp_path / "blacksmith-mithril.session.instrumented.bin",
    )
    monkeypatch.setattr(
        blacksmith_mithril, "_with_instrumented_rom_database", lambda _, __, action: action()
    )
    monkeypatch.setattr(
        blacksmith_mithril,
        "run_observer",
        lambda **_: (_ for _ in ()).throw(RuntimeError("BizHawk exited with code 1")),
    )
    with pytest.raises(RuntimeError, match="blacksmith-mithril observer callback failure") as error:
        blacksmith_mithril.verify_blacksmith_mithril(tmp_path / "input.bin", tmp_path)
    message = str(error.value)
    for expected in (
        '"caseId": "tool-direct-add-item-admission"',
        '"phase": "precommit-cleanup"',
        '"role": "precommit-cleanup-equippability-effective-return"',
        '"actualPc": 36762',
        '"expectedCallPc": 138262',
        '"expectedTargetPc": 36736',
        '"expectedReturnPc": 138268',
        '"actualStackReturn": 16739156',
        '"pendingCallback": {"active": true, "caseIndex": 3',
    ):
        assert expected in message


def test_verifier_stops_before_observer_when_static_preflight_fails(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    invoked = False

    def reject_static(*_: object) -> dict[str, object]:
        raise ValueError("blacksmith H1/ROM instruction guard drift")

    def never_observe(**_: object) -> dict[str, object]:
        nonlocal invoked
        invoked = True
        return {}

    monkeypatch.setattr(blacksmith_mithril, "verify_runtime_contract", lambda *_: None)
    monkeypatch.setattr(blacksmith_mithril, "validate_static_contract", reject_static)
    monkeypatch.setattr(blacksmith_mithril, "run_observer", never_observe)
    with pytest.raises(ValueError, match="instruction guard drift"):
        blacksmith_mithril.verify_blacksmith_mithril(tmp_path / "input.bin", tmp_path)
    assert not invoked
