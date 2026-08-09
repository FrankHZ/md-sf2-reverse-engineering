from __future__ import annotations

import copy
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
    assert all(
        "expected" not in case and "result" not in case
        for case in [*fixture["cases"], *fixture["transactionCases"]]
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
        "tableSourcePath": "data/stats/items/mithrilweapons.asm",
        "h1ListingPath": "build/sf2build-h1.lst",
        "functionEntryAddress": 138966,
        "placeEntryAddress": 138690,
    }
    assert static["function"]["entryAddress"] == fixture["sourceContext"][
        "functionEntryAddress"
    ]
    assert static["transaction"]["placeEntryAddress"] == fixture["sourceContext"][
        "placeEntryAddress"
    ]

    for field, value in (
        ("pickSourcePath", "code/common/menus/blacksmith/wrong.asm"),
        ("placeSourcePath", "code/common/menus/blacksmith/wrong.asm"),
        ("tableSourcePath", "data/stats/items/wrong.asm"),
        ("h1ListingPath", "build/wrong.lst"),
        ("functionEntryAddress", 138968),
        ("placeEntryAddress", 138692),
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
    )
    assert all("const" not in shape and "enum" not in shape for shape in transaction_shapes)

    wrong_order = copy.deepcopy(fixture)
    wrong_order["transactionCaseOrder"] = list(
        reversed(wrong_order["transactionCaseOrder"])
    )
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
        "combatantEntrySizeBytes": 56,
        "combatantItemSlotCount": 4,
        "combatantItemsOffsetBytes": 32,
        "flag80Id": 80,
        "flag80ByteOffset": 10,
        "flag80BitMask": 128,
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
    assert transaction_bytes["addi.w #1,pendingOrdersNumber(a6)"] == bytes.fromhex(
        "066E0001FFF2"
    )
    assert transaction_bytes["move.w clientMember(a6),d0"] == bytes.fromhex("302EFFFA")
    assert transaction_bytes["move.w itemSlot(a6),d1"] == bytes.fromhex("322EFFF4")
    assert transaction_bytes["move.w #80,d1"] == bytes.fromhex("323C0050")
    assert [choice["denominator"] for choice in static["model"]["weaponRows"][0]] == [
        16,
        8,
        4,
        1,
    ]


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
        record["callbackChronology"][5]
        == {"role": "pending-orders-incremented", "pc": 138708}
        for record in transactions
    )
    assert observed["restoration"] == {
        "currentGoldLongRestored": True,
        "randomSeedWordRestored": True,
        "orderWordsRestored": True,
        "flag80OwningByteRestored": True,
        "clientCombatantRecordsRestored": True,
    }


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
def test_source_and_h1_mutations_fail_before_runtime(
    mutation: str, error: str
) -> None:
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
    source = (
        root / "disasm" / blacksmith_mithril.PICK_SOURCE_RELATIVE
    ).read_text(encoding="utf-8")
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
        blacksmith_mithril.build_static_contract(
            fixture, root, **{source_name: source}
        )


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
    source = (
        root / "disasm" / blacksmith_mithril.BLACKSMITH_ACTIONS_RELATIVE
    ).read_text(encoding="utf-8")
    start = source.rfind("; ===============", 0, source.index("BlacksmithAction_PlaceOrder:"))
    mutated = source[:start] + source[start:].replace(
        "clientMember = -6", "; clientMember = -6", 1
    )
    with pytest.raises(ValueError, match="frame declaration drift: clientMember"):
        blacksmith_mithril.build_static_contract(
            fixture, root, actions_source_text=mutated
        )


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
    table = (root / "disasm" / blacksmith_mithril.TABLE_SOURCE_RELATIVE).read_text(
        encoding="utf-8"
    )
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
    guarded = static["h1"]["instructionBytes"] + static["transaction"][
        "h1InstructionBytes"
    ]
    size = max(instruction["address"] + len(instruction["bytes"]) for instruction in guarded) + 1
    size = max(size, static["h1"]["weaponTableAddress"] + len(static["h1"]["weaponTableBytes"]))
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
    for address in (
        static["function"]["fallbackRngCallAddress"],
        static["function"]["clientClassReadAddress"],
        static["function"]["orderStrideAddress"],
        static["function"]["orderWriteAddress"],
        static["transaction"]["decreaseGoldCallAddress"],
        static["transaction"]["dropItemEffectiveReturnAddress"],
        static["transaction"]["clearFlagEffectiveReturnAddress"],
        transaction_instruction["addi.w #1,pendingOrdersNumber(a6)"],
        transaction_instruction["move.w clientMember(a6),d0"],
        transaction_instruction["move.w itemSlot(a6),d1"],
        transaction_instruction["move.w #80,d1"],
    ):
        corrupted = bytearray(clean)
        corrupted[address] ^= 1
        image.write_bytes(corrupted)
        with pytest.raises(ValueError, match="instruction guard drift"):
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
        },
        "error": "CheckSram return redirect write drift",
    }


def _observer_role_sets(source: str) -> tuple[set[str], set[str]]:
    registered_roles = set(re.findall(r'register_exec\([^,]+,"([^"]+)"', source))
    failure_roles = {"registration", "bootstrap-return-redirect"} | (
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
        shared["definitions"]["blacksmithMithrilPendingCallback"]["properties"][
            "rolesAtPc"
        ]["items"]["enum"]
    )
    assert failure_roles == failure_enum
    assert registered_roles == pending_enum
    assert "registration" in failure_roles and "registration" not in registered_roles

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
    assert 'mode,helper_index,transaction_index="none",0,0' in source
    assert 'current_role=="transaction-case-entry"' in source
    assert 'current_role=="registration" and nil or emu.getregister("M68K PC")' in source
    bootstrap = source.index("local function bootstrap_check_sram()")
    probe_write = source.index("write_probe();helper_index=1;bootstrapped=true", bootstrap)
    first_case_entry = source.index('register_exec(entry,"case-entry",index)', bootstrap)
    assert probe_write < first_case_entry


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
    assert "frame_base+24" not in source
    assert "order_write_seen==(#differences==1)" in source
    assert source.index("write_probe();helper_index=1;bootstrapped=true") < source.index(
        "status(\"milestone:direct-function-probe\")"
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
    assert "h1InstructionBytes" not in launches[0]["config"]["transaction"]
    assert result == {
        "Fixture": "sf2-blacksmith-mithril-runtime-v2",
        "Cases": 8,
        "HelperCases": 5,
        "TransactionCases": 3,
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
        "run_observer",
        lambda **_: (_ for _ in ()).throw(RuntimeError("BizHawk exited with code 1")),
    )
    with pytest.raises(RuntimeError, match="blacksmith-mithril observer callback failure") as error:
        blacksmith_mithril.verify_blacksmith_mithril(tmp_path / "input.bin", tmp_path)
    message = str(error.value)
    for expected in (
        '"caseId": "brn-fallback-zero-row2-slot2"',
        '"phase": "rng-entry"',
        '"role": "rng-entry"',
        '"actualPc": 5632',
        '"expectedCallPc": 139014',
        '"expectedTargetPc": 5632',
        '"expectedReturnPc": 139018',
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
