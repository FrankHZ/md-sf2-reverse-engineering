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
    assert all("expected" not in case and "result" not in case for case in fixture["cases"])
    static = _static(fixture)
    observed = blacksmith_mithril.expected_observation(fixture, static)
    validate_json(observed, blacksmith_mithril.OBSERVATION_SCHEMA, owner="blacksmith observation")
    blacksmith_mithril._assert_observation(fixture, static, observed)

    malformed = copy.deepcopy(fixture)
    malformed["cases"][0]["expected"] = {"itemIndex": 69}
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
    validate_json(
        malformed,
        blacksmith_mithril.OBSERVATION_SCHEMA,
        owner="blacksmith observation",
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
    assert static["ram"] == {"randomSeedAddress": 0xFFDEA4, "ordersAddress": 0xFFF7A8}
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
    }
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
    validate_json(
        fixture,
        blacksmith_mithril.FIXTURE_SCHEMA,
        owner="schema-valid fixture order-slot drift",
    )
    root = blacksmith_mithril.repo_path("local/upstream/SF2DISASM")
    with pytest.raises(ValueError, match="fixture order-slot domain drift"):
        blacksmith_mithril.build_static_contract(fixture, root)


def test_independent_model_covers_all_required_runtime_roles() -> None:
    fixture = _fixture()
    records = blacksmith_mithril.expected_observation(fixture, _static(fixture))["records"]
    assert [record["orderWriteIndex"] for record in records] == [0, 1, 2, 3, None]
    assert [record["choiceIndex"] for record in records] == [0, 3, 0, 0, 0]
    assert [record["classGroupIndex"] for record in records] == [0, 2, 8, 8, 0]
    assert [record["weaponRowIndex"] for record in records] == [0, 2, 2, 0, 0]
    assert [call["result"] for call in records[2]["rngCalls"]] == [0, 0]
    assert [call["result"] for call in records[3]["rngCalls"]] == [1, 0]
    assert [call["rangeWord"] for call in records[1]["rngCalls"]] == [16, 8, 4, 1]
    assert records[-1]["ordersAfter"] == fixture["cases"][-1]["ordersBefore"]


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
        ("client-class-offset", "client-class source/H1 offset drift"),
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
    size = max(
        instruction["address"] + len(instruction["bytes"])
        for instruction in static["h1"]["instructionBytes"]
    ) + 1
    size = max(size, static["h1"]["weaponTableAddress"] + len(static["h1"]["weaponTableBytes"]))
    rom = bytearray(size)
    for instruction in static["h1"]["instructionBytes"]:
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
    for address in (
        static["function"]["fallbackRngCallAddress"],
        static["function"]["clientClassReadAddress"],
        static["function"]["orderStrideAddress"],
        static["function"]["orderWriteAddress"],
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
        },
        "error": "CheckSram return redirect write drift",
    }


def _observer_role_sets(source: str) -> tuple[set[str], set[str]]:
    initial_roles = set(
        re.findall(
            r'current_phase,current_role(?:,current_pc,current_expectation)?="[^"]+","([^"]+)"',
            source,
        )
    )
    expectation_roles = set(
        re.findall(r'set_expectation\("[^"]+","([^"]+)"', source)
    )
    dynamic_rng_roles = set(
        re.findall(r'entry\.role=="([^"]+)" then rng_call\(entry\.role', source)
    )
    registered_roles = set(re.findall(r'register_exec\([^,]+,"([^"]+)"', source))
    return initial_roles | expectation_roles | dynamic_rng_roles, registered_roles


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

    renamed_failure, _ = _observer_role_sets(
        source.replace(
            'set_expectation("rng-entry","rng-entry"',
            'set_expectation("rng-entry","rng-entry-renamed"',
            1,
        )
    )
    assert renamed_failure != failure_enum
    _, renamed_pending = _observer_role_sets(
        source.replace(
            'register_exec(f.rngEntryAddress,"rng-entry",0)',
            'register_exec(f.rngEntryAddress,"rng-entry-renamed",0)',
            1,
        )
    )
    assert renamed_pending != pending_enum

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
        "local active,case_index,observer_failed,session_cleaned,bootstrapped="
        "false,0,false,false,false" in source
    )
    assert 'local case=(active or current_role=="case-entry") and current_case() or nil' in source
    assert 'current_role=="registration" and nil or emu.getregister("M68K PC")' in source
    bootstrap = source.index("local function bootstrap_check_sram()")
    probe_write = source.index("write_probe();case_index=1;bootstrapped=true", bootstrap)
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
    assert "milestone:seed-and-orders-restored" in source
    assert "milestone:callbacks-cleared:0" in source
    assert "milestone:observer-finished" in source
    assert "f.returnRtsAddress" in source
    assert "f.checkSramAddress" in source
    assert "frame_base-c.clientClassOffset" in source
    assert "frame_base+24" not in source
    assert "order_write_seen==(#differences==1)" in source
    assert source.index("write_probe();case_index=1;bootstrapped=true") < source.index(
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
    assert result == {
        "Fixture": "sf2-blacksmith-mithril-runtime-v1",
        "Cases": 5,
        "BizHawkLaunches": 1,
        "CallbacksCleared": 0,
        "SeedAndOrdersRestored": True,
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
