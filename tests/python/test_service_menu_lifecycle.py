from __future__ import annotations

import copy
import json
from pathlib import Path

import pytest

from sf2tool.h3 import service_menu_lifecycle
from sf2tool.jsonio import load_json, validate_json


def _fixture() -> dict[str, object]:
    return load_json(service_menu_lifecycle.FIXTURE)


def _static() -> dict[str, object]:
    return service_menu_lifecycle.build_static_contract(
        service_menu_lifecycle.repo_path("local/roms/sf2-us.bin")
    )


def test_static_fixture_schemas_and_exact_golden_are_closed() -> None:
    fixture = _fixture()
    static = _static()
    validate_json(fixture, service_menu_lifecycle.FIXTURE_SCHEMA, owner="fixture")
    service_menu_lifecycle._assert_fixture(fixture, static)
    expected = service_menu_lifecycle.expected_observation(fixture, static)
    validate_json(expected, service_menu_lifecycle.OBSERVATION_SCHEMA, owner="observation")
    assert fixture["acceptedObservation"] == expected
    assert fixture["sourceContext"] == {
        "shopEntryAddress": 131172,
        "churchEntryAddress": 133634,
        "caravanEntryAddress": 139218,
        "blacksmithEntryAddress": 137786,
    }
    for record in expected["records"]:
        source_field = f"{record['service']}EntryAddress"
        assert record["entryAddress"] == fixture["sourceContext"][source_field]
        assert record["callbackChronology"][1] == {
            "role": "service-entry",
            "pc": fixture["sourceContext"][source_field],
        }
        assert record["serviceBodyBypassed"] is True
        assert record["callbackChronology"][2]["role"] == (
            "generated-blacksmith-return-stub"
            if record["service"] == "blacksmith"
            else "generated-service-cancel-stub"
        )
    assert expected["generatedStubReadback"] == [
        {
            "role": "generated-service-cancel-stub",
            "address": 0xFF6D00,
            "widthBytes": 4,
            "hex": "70FF4E75",
        },
        {
            "role": "generated-blacksmith-return-stub",
            "address": 0xFF6D10,
            "widthBytes": 2,
            "hex": "4E75",
        },
    ]
    assert len(expected["outerReturnTrampolineReadback"]) == 15
    assert expected["outerReturnTrampolineReadback"][0] == {
        "id": "context-menu-church",
        "role": "outer-caller-return",
        "address": 0xFF6D30,
        "targetAddress": 0x474C4,
        "widthBytes": 6,
        "hex": "4EF9000474C4",
        "serviceEntryStackAddress": 0xFFFEF8,
        "sourceReturnAddress": 0x474C4,
        "postServiceRtsStackAddress": 0xFFFEFC,
    }
    assert expected["outerReturnTrampolineReadback"][-1]["role"] == "outer-rts-harness-return"
    assert expected["outerReturnTrampolineReadback"][-1]["targetAddress"] == 0xFF6D20
    assert "sessionRomDeleted" not in expected["restoration"]

    malformed = copy.deepcopy(fixture)
    del malformed["static"]["callerFamilyServiceCounts"]
    with pytest.raises(ValueError, match="required property"):
        validate_json(malformed, service_menu_lifecycle.FIXTURE_SCHEMA, owner="fixture")
    malformed = copy.deepcopy(fixture)
    del malformed["static"]["harness"]["caseFrameBudget"]
    with pytest.raises(ValueError, match="required property"):
        validate_json(malformed, service_menu_lifecycle.FIXTURE_SCHEMA, owner="fixture")
    malformed = copy.deepcopy(fixture)
    malformed["static"]["harness"]["caseFrameBudget"] = 181
    with pytest.raises(ValueError, match="180 was expected"):
        validate_json(malformed, service_menu_lifecycle.FIXTURE_SCHEMA, owner="fixture")
    malformed = copy.deepcopy(fixture)
    malformed["sourceContext"]["shopEntryAddress"] += 2
    with pytest.raises(ValueError, match="source-context entry"):
        service_menu_lifecycle._assert_fixture(malformed, static)
    malformed = copy.deepcopy(fixture)
    malformed["acceptedObservation"]["records"][0]["registersAfter"]["a7"] += 2
    with pytest.raises(ValueError, match="accepted observation"):
        service_menu_lifecycle._assert_fixture(malformed, static)
    for field, value in (
        ("address", 0xFF6D02),
        ("instructionHex", "4E75"),
        ("widthBytes", 2),
        ("resultD0Word", None),
        ("role", "generated-blacksmith-return-stub"),
    ):
        malformed = copy.deepcopy(fixture)
        malformed["static"]["generatedStubs"][0][field] = value
        with pytest.raises(ValueError, match="static golden"):
            service_menu_lifecycle._assert_fixture(malformed, static)
    malformed = copy.deepcopy(fixture)
    del malformed["static"]["generatedStubs"]
    with pytest.raises(ValueError, match="required property"):
        validate_json(malformed, service_menu_lifecycle.FIXTURE_SCHEMA, owner="fixture")
    malformed = copy.deepcopy(fixture)
    malformed["cases"][0].update(
        {"transferKind": "tail-transfer", "returnKind": "outer-rts-harness"}
    )
    with pytest.raises(ValueError, match="null"):
        validate_json(malformed, service_menu_lifecycle.FIXTURE_SCHEMA, owner="fixture")
    malformed = copy.deepcopy(fixture)
    malformed["static"]["callerInventory"][0].update(
        {"transferKind": "tail-transfer", "returnKind": "outer-rts-harness"}
    )
    with pytest.raises(ValueError, match="null"):
        validate_json(malformed, service_menu_lifecycle.FIXTURE_SCHEMA, owner="fixture")
    malformed = copy.deepcopy(fixture)
    malformed["static"]["generatedStubs"][0]["instructionHex"] = "4E75"
    with pytest.raises(ValueError, match="70FF4E75"):
        validate_json(malformed, service_menu_lifecycle.FIXTURE_SCHEMA, owner="fixture")
    malformed = copy.deepcopy(fixture)
    malformed["acceptedObservation"]["generatedStubReadback"].reverse()
    with pytest.raises(ValueError, match="generated-service-cancel-stub"):
        validate_json(malformed, service_menu_lifecycle.FIXTURE_SCHEMA, owner="fixture")
    malformed = copy.deepcopy(expected)
    malformed["records"][0]["callbackChronology"].append({"role": "caller-result", "pc": 0})
    with pytest.raises(ValueError, match="is too long"):
        validate_json(malformed, service_menu_lifecycle.OBSERVATION_SCHEMA, owner="observation")
    malformed = copy.deepcopy(expected)
    tail_record = malformed["records"][-1]
    tail_record["returnAddress"] = 0
    with pytest.raises(ValueError, match="null"):
        validate_json(malformed, service_menu_lifecycle.OBSERVATION_SCHEMA, owner="observation")
    malformed = copy.deepcopy(expected)
    malformed["generatedStubReadback"][0]["hex"] = "4E75"
    with pytest.raises(ValueError, match="70FF4E75"):
        validate_json(malformed, service_menu_lifecycle.OBSERVATION_SCHEMA, owner="observation")
    malformed = copy.deepcopy(expected)
    malformed["outerReturnTrampolineReadback"][0]["targetAddress"] += 2
    with pytest.raises(ValueError, match="accepted observation"):
        service_menu_lifecycle._assert_fixture(
            {**fixture, "acceptedObservation": malformed}, static
        )


def test_complete_source_inventory_is_zero_inclusive_and_runtime_cases_are_derived() -> None:
    static = _static()
    assert static["callerDenominator"] == 69
    assert static["callerServiceCounts"] == {
        "shop": 33,
        "church": 29,
        "caravan": 5,
        "blacksmith": 2,
    }
    assert static["callerFamilyCounts"] == {
        "battle-test": 4,
        "context-menu": 3,
        "exploration-vint": 2,
        "map-entity": 60,
    }
    assert static["callerTransferCounts"] == {"returning-call": 62, "tail-transfer": 7}
    assert static["callerFamilyServiceCounts"] == {
        "context-menu": {"shop": 1, "church": 1, "caravan": 0, "blacksmith": 1},
        "exploration-vint": {"shop": 0, "church": 1, "caravan": 1, "blacksmith": 0},
        "battle-test": {"shop": 1, "church": 2, "caravan": 1, "blacksmith": 0},
        "map-entity": {"shop": 31, "church": 25, "caravan": 3, "blacksmith": 1},
    }
    assert static["callerFamilyServiceTransferCounts"]["map-entity"] == {
        "shop": {"returning-call": 28, "tail-transfer": 3},
        "church": {"returning-call": 22, "tail-transfer": 3},
        "caravan": {"returning-call": 3, "tail-transfer": 0},
        "blacksmith": {"returning-call": 0, "tail-transfer": 1},
    }
    assert static["representativeSelectionRule"] == {
        "positiveCell": "lowest-call-site-address",
        "additionalStackCase": "battle-test-church-movem-save-restore",
    }
    assert static["h2ServiceEntries"] == [
        "BlacksmithMenu",
        "CaravanMenu",
        "ChurchMenu",
        "FieldMenu",
        "ShopMenu",
    ]
    assert [case["caseId"] for case in static["cases"]] == list(service_menu_lifecycle.CASE_ORDER)
    special = static["cases"][8]
    assert special["callerEntryAddress"] == special["callSiteAddress"] - 4
    assert special["continuationRedirectAddress"] == special["returnAddress"] + 4
    rom = service_menu_lifecycle.repo_path("local/roms/sf2-us.bin").read_bytes()
    assert rom[special["callerEntryAddress"] : special["callSiteAddress"]] == bytes.fromhex(
        "48E7FFFE"
    )
    assert rom[special["returnAddress"] : special["continuationRedirectAddress"]] == bytes.fromhex(
        "4CDF7FFF"
    )
    assert special["serviceEntryStackAddress"] == 0xFFFEC0
    assert special["postServiceRtsStackAddress"] == 0xFFFEC4
    assert service_menu_lifecycle._movem_register_count("movem.l d0-a6,-(sp)", push=True) == 15
    assert service_menu_lifecycle._movem_register_count("movem.l (sp)+,d0-a6", push=False) == 15
    assert service_menu_lifecycle._movem_register_count("movem.l d0-a7,-(sp)", push=True) == 16
    with pytest.raises(ValueError, match="range identity"):
        service_menu_lifecycle._movem_register_count("movem.l d0-a6,(sp)+", push=True)
    bad_stack_mask = bytearray(rom)
    bad_stack_mask[special["callSiteAddress"] - 2] ^= 0x01
    listing = (service_menu_lifecycle.UPSTREAM / service_menu_lifecycle.LISTING_RELATIVE).read_text(
        encoding="utf-8"
    )
    with pytest.raises(ValueError, match="stack mask"):
        service_menu_lifecycle._outer_return_stack_contract(
            copy.deepcopy(static["cases"]),
            service_menu_lifecycle.DISASM,
            listing,
            service_menu_lifecycle.listing_symbol_addresses(listing),
            bytes(bad_stack_mask),
        )
    tails = [case for case in static["cases"] if case["transferKind"] == "tail-transfer"]
    assert [
        (case["caseId"], case["opcode"], case["callSiteAddress"], case["returnAddress"])
        for case in tails
    ] == [
        ("map-entity-tail-church", "jmp", 0x560B8, None),
        ("map-entity-tail-shop", "jmp", 0x560E8, None),
        ("map-entity-tail-blacksmith", "jmp", 0x5A110, None),
    ]
    bad_rom = bytearray(rom)
    bad_rom[tails[0]["callSiteAddress"] + 1] ^= 1
    with pytest.raises(ValueError, match="ROM opcode/target drift"):
        service_menu_lifecycle._bind_rom_instruction_bytes(
            copy.deepcopy(static["callerInventory"]), bytes(bad_rom), static["aliases"]
        )


def test_direct_caller_parser_excludes_comments_near_misses_and_rejects_h1_opcode_drift(
    tmp_path: Path,
) -> None:
    relative = "code/common/scripting/map/mapscriptengine_2.asm"
    source_path = tmp_path / relative
    source_path.parent.mkdir(parents=True)
    source_path.write_text(
        "; jsr j_ChurchMenu must not count\n"
        "j_ShopMenu:\n"
        "jsr.w j_ShopMenu ; one real direct alias call\n"
        "move.w #j_ChurchMenu,d0\n"
        "jsr j_ShopMenuExtra\n"
        "bsr.s j_CaravanMenu\n"
        "jmp.l j_BlacksmithMenu\n",
        encoding="utf-8",
    )
    listing = (
        f"; ASM FILE {relative} :\n"
        "00000010 jsr.w j_ShopMenu\n"
        "00000016 nop\n"
        "00000020 bsr.s j_CaravanMenu\n"
        "00000026 rts\n"
        "00000030 jmp.l j_BlacksmithMenu\n"
        "00000036 nop\n"
    )
    inventory = service_menu_lifecycle._caller_inventory(tmp_path, listing)
    assert [
        (
            site["opcode"],
            site["instructionTarget"],
            site["effectiveTarget"],
            site["transferKind"],
            site["callSiteAddress"],
        )
        for site in inventory
    ] == [
        ("jsr", "j_ShopMenu", "ShopMenu", "returning-call", 0x10),
        ("bsr", "j_CaravanMenu", "CaravanMenu", "returning-call", 0x20),
        ("jmp", "j_BlacksmithMenu", "BlacksmithMenu", "tail-transfer", 0x30),
    ]
    assert [site["returnAddress"] for site in inventory] == [0x16, 0x26, None]
    assert [site["instructionWidthBytes"] for site in inventory] == [6, 6, 6]

    source_path.write_text(
        source_path.read_text(encoding="utf-8").replace("bsr.s", "jsr.s"), encoding="utf-8"
    )
    with pytest.raises(ValueError, match="H1 identity"):
        service_menu_lifecycle._caller_inventory(tmp_path, listing)
    source_path.write_text(
        source_path.read_text(encoding="utf-8")
        .replace("jsr.s j_CaravanMenu", "bsr.s j_CaravanMenu")
        .replace("jmp.l j_BlacksmithMenu", "jsr.l j_BlacksmithMenu"),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="H1 identity"):
        service_menu_lifecycle._caller_inventory(tmp_path, listing)


def test_source_guard_rejects_control_branch_polarity_before_fixture_comparison() -> None:
    shop_source = (
        service_menu_lifecycle.DISASM / service_menu_lifecycle.SOURCE_PATHS["shop"]
    ).read_text(encoding="utf-8")
    service_menu_lifecycle._require_order(
        shop_source,
        "ShopMenu",
        ("cmpi.w #-1,d0", "beq.s @ExitShop", "unlk a6", "rts"),
    )
    bad_source = shop_source.replace("beq.s   @ExitShop", "bne.s   @ExitShop", 1)
    assert bad_source != shop_source
    with pytest.raises(ValueError, match="source guard drift"):
        service_menu_lifecycle._require_order(
            bad_source,
            "ShopMenu",
            ("cmpi.w #-1,d0", "beq.s @ExitShop", "unlk a6", "rts"),
        )


def test_prelaunch_guards_reject_schema_valid_matrix_and_patch_drift(tmp_path: Path) -> None:
    fixture = _fixture()
    static = _static()
    bad = copy.deepcopy(fixture)
    bad["cases"][0]["returnKind"] = "rts"
    with pytest.raises(ValueError, match="selected case matrix"):
        service_menu_lifecycle._assert_fixture(bad, static)
    bad = copy.deepcopy(fixture)
    bad["static"]["callerInventory"][0]["effectiveTarget"] = "ShopMenu"
    with pytest.raises(ValueError, match="static golden"):
        service_menu_lifecycle._assert_fixture(bad, static)

    bad_static = copy.deepcopy(static)
    bad_static["sessionPatches"][1]["address"] = bad_static["sessionPatches"][0]["address"]
    with pytest.raises(ValueError, match="overlap"):
        service_menu_lifecycle._instrument_session_rom(
            service_menu_lifecycle.repo_path("local/roms/sf2-us.bin"),
            bad_static,
            tmp_path / "overlap.bin",
        )
    bad_entry_seams = copy.deepcopy(static["entrySeams"])
    bad_entry_seams["shop"]["controlledResultD0Word"] = 0
    with pytest.raises(ValueError, match="cancel-result ABI"):
        service_menu_lifecycle._generated_stubs(bad_entry_seams)
    bad_stack = copy.deepcopy(static)
    bad_stack["cases"][0]["serviceEntryStackAddress"] += 4
    with pytest.raises(ValueError, match="static golden"):
        service_menu_lifecycle._assert_fixture(fixture, bad_stack)
    bad_stack = copy.deepcopy(static)
    bad_stack["cases"][-1]["postServiceRtsStackAddress"] -= 4
    with pytest.raises(ValueError, match="static golden"):
        service_menu_lifecycle._assert_fixture(fixture, bad_stack)
    assert service_menu_lifecycle.preflight_service_menu_lifecycle(
        service_menu_lifecycle.repo_path("local/roms/sf2-us.bin")
    ) == {
        "Fixture": "sf2-service-menu-entry-return-v1",
        "Cases": 15,
        "CallerDenominator": 69,
        "SessionPatches": 17,
        "Status": "PRELAUNCH-PASS",
    }


def test_preflight_removes_the_exact_session_rom_after_a_write_failure(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    original = service_menu_lifecycle._instrument_session_rom
    created: list[Path] = []

    def fail_after_write(rom_path: Path, static: dict[str, object], destination: Path) -> None:
        original(rom_path, static, destination)
        created.append(destination)
        raise RuntimeError("test after disposable session write")

    monkeypatch.setattr(service_menu_lifecycle, "_instrument_session_rom", fail_after_write)
    with pytest.raises(RuntimeError, match="test after disposable session write"):
        service_menu_lifecycle.preflight_service_menu_lifecycle(
            service_menu_lifecycle.repo_path("local/roms/sf2-us.bin")
        )
    assert created and all(not path.exists() for path in created)


def test_runtime_checks_the_exact_session_rom_after_success_and_observer_failure(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    fixture = _fixture()
    created: list[Path] = []

    def fake_database(session_rom: Path, _name: str, thunk: object) -> object:
        created.append(session_rom)
        return thunk()

    monkeypatch.setattr(service_menu_lifecycle, "_with_instrumented_rom_database", fake_database)
    monkeypatch.setattr(service_menu_lifecycle, "verify_runtime_contract", lambda *_args: None)
    monkeypatch.setattr(
        service_menu_lifecycle, "assert_observer_status", lambda *_args, **_kwargs: None
    )

    def passing_observer(*, config: dict[str, object], **_kwargs: object) -> dict[str, object]:
        return service_menu_lifecycle.expected_observation(fixture, config["static"])

    monkeypatch.setattr(service_menu_lifecycle, "run_observer", passing_observer)
    result = service_menu_lifecycle.verify_service_menu_lifecycle(
        service_menu_lifecycle.repo_path("local/roms/sf2-us.bin")
    )
    assert result["SessionRomDeleted"] is True
    assert created and all(not path.exists() for path in created)

    created.clear()
    monkeypatch.setattr(
        service_menu_lifecycle,
        "run_observer",
        lambda **_kwargs: (_ for _ in ()).throw(RuntimeError("synthetic observer failure")),
    )
    monkeypatch.setattr(
        service_menu_lifecycle, "_failure_diagnostic", lambda _path, _static=None: None
    )
    with pytest.raises(RuntimeError, match="synthetic observer failure"):
        service_menu_lifecycle.verify_service_menu_lifecycle(
            service_menu_lifecycle.repo_path("local/roms/sf2-us.bin")
        )
    assert created and all(not path.exists() for path in created)


def test_observer_config_omits_accepted_golden_and_lua_dispatch_is_closed() -> None:
    fixture = _fixture()
    static = _static()
    config = service_menu_lifecycle._observer_config(fixture, static)
    assert "acceptedObservation" not in config
    assert config["static"] == service_menu_lifecycle._canonical_static(static)
    assert config["sourceContext"] == fixture["sourceContext"]
    source = service_menu_lifecycle.OBSERVER.read_text(encoding="utf-8")
    assert source.count("event.on_bus_exec(function()") == 1
    assert "if not callbacks[address] then" in source
    assert "for _,entry in ipairs(callbacks[address]) do dispatch(address,entry) end" in source
    assert 'error("unknown deterministic dispatch role: "..entry.role)' in source
    assert "pcall(function() current_pc=address" in source
    assert "client.exitCode(config.observerFailureContract.exitCode)" in source
    assert "os.remove(config.outputPath);cleanup_session()" in source
    assert "milestone:callbacks-cleared:0" in source
    assert "milestone:observer-finished" in source
    assert 's.aliases[service].effectiveTargetAddress,"service-entry"' in source
    assert 'source[case.service.."EntryAddress"]==expected' in source
    assert "case._chronology={}" in source
    assert "table.remove(case._chronology,1)" in source
    assert 'case.transferKind=="tail-transfer"' in source
    assert '"tail-transfer-site"' in source
    assert '"outer-rts-harness-return"' in source
    assert "outer_return_trampoline_callback" in source
    assert "write_outer_return_trampoline" in source
    assert "case.serviceEntryStackAddress" in source
    assert "case.postServiceRtsStackAddress" in source
    assert '"generated-service-cancel-stub"' in source
    assert '"generated-blacksmith-return-stub"' in source
    assert "write_generated_stubs()" in source
    assert "stub.purpose==expected_purpose" in source
    assert '\\"serviceBodyBypassed\\":true' in source
    assert "sessionRomDeleted" not in source
    assert 'case_frames=0;mode="case"' in source
    assert "h.caseFrameBudget==180" in source
    assert "case_frames>h.caseFrameBudget" in source


def test_lua_role_audit_closes_registration_dispatch_schemas_and_chronologies(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    service_menu_lifecycle.assert_lua_role_contract()
    original_observer = service_menu_lifecycle.OBSERVER
    roles = service_menu_lifecycle.lua_role_contract()
    assert roles["registered"] == roles["dispatched"]
    assert "outer-rts-harness-return" in roles["registered"]
    assert {"tail-transfer-site", "outer-rts-harness-return"} <= roles["observation"]
    assert {"generated-service-cancel-stub", "generated-blacksmith-return-stub"} <= roles[
        "registered"
    ]
    assert roles["registered"] <= roles["failure"]
    assert roles["pending"] == roles["registered"]

    trampoline_dispatch = (
        'elseif entry.role=="outer-caller-return" or '
        'entry.role=="outer-rts-harness-return" then '
        "outer_return_trampoline_callback(address,entry.role)\n"
    )
    source = service_menu_lifecycle.OBSERVER.read_text(encoding="utf-8").replace(
        trampoline_dispatch, ""
    )
    observer = tmp_path / "observer.lua"
    observer.write_text(source, encoding="utf-8")
    monkeypatch.setattr(service_menu_lifecycle, "OBSERVER", observer)
    with pytest.raises(ValueError, match="deterministic-dispatch role audit"):
        service_menu_lifecycle.assert_lua_role_contract()

    source = original_observer.read_text(encoding="utf-8")
    observer.write_text(
        source.replace(
            'register_exec(generated_stub("generated-service-cancel-stub").address,'
            '"generated-service-cancel-stub",0,nil)\n',
            "",
        ),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="registered-role audit"):
        service_menu_lifecycle.assert_lua_role_contract()


def _failure_payload() -> dict[str, object]:
    return {
        "owner": "service-menu-entry-return",
        "caseId": "context-menu-church",
        "phase": "service-entry",
        "role": "service-entry",
        "actualPc": 133634,
        "expectedCallPc": 292030,
        "expectedTargetPc": 133634,
        "expectedReturnPc": 292036,
        "stackReadback": {
            "expectedA7": 0xFFFEF8,
            "actualA7": 0xFFFEF8,
            "expectedTopLongword": 0x474C4,
            "actualTopLongword": 0x474C4,
        },
        "pendingCallback": {
            "active": True,
            "caseIndex": 1,
            "expectedCaseId": "context-menu-church",
            "rolesAtPc": ["service-entry"],
            "observedChronology": [
                {"role": "case-entry", "pc": 0xFF6800},
                {"role": "caller-call-site", "pc": 0x474BE},
                {"role": "service-entry", "pc": 133634},
            ],
            "expectedChronology": [
                {"role": "case-entry", "pc": 0xFF6800},
                {"role": "caller-call-site", "pc": 0x474BE},
                {"role": "service-entry", "pc": 133634},
                {"role": "generated-service-cancel-stub", "pc": 0xFF6D00},
                {"role": "outer-caller-return", "pc": 0xFF6D30},
                {"role": "caller-result", "pc": 0xFF6818},
            ],
            "observedChronologyCount": 3,
            "expectedChronologyCount": 6,
        },
        "restoration": {
            "currentPortraitRestored": True,
            "callerFrameRestored": True,
            "callbacksCleared": True,
            "outputRemoved": True,
        },
        "error": "exact callback state drift",
    }


def test_callback_failure_contract_is_terminal_structured_and_promoted(tmp_path: Path) -> None:
    payload = _failure_payload()
    validate_json(payload, service_menu_lifecycle.FAILURE_SCHEMA, owner="failure")
    status = tmp_path / "service.status.txt"
    status.write_text(
        "milestone:observer-loaded\n"
        + service_menu_lifecycle.STATUS_PREFIX
        + json.dumps(payload)
        + "\n",
        encoding="utf-8",
    )
    assert service_menu_lifecycle._failure_diagnostic(status) == payload
    assert service_menu_lifecycle._failure_diagnostic(status, _static()) == payload
    malformed = copy.deepcopy(payload)
    (
        malformed["pendingCallback"]["observedChronology"][1],
        malformed["pendingCallback"]["observedChronology"][2],
    ) = (
        malformed["pendingCallback"]["observedChronology"][2],
        malformed["pendingCallback"]["observedChronology"][1],
    )
    status.write_text(
        "milestone:observer-loaded\n"
        + service_menu_lifecycle.STATUS_PREFIX
        + json.dumps(malformed)
        + "\n",
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="expected prefix"):
        service_menu_lifecycle._failure_diagnostic(status, _static())
    malformed = copy.deepcopy(payload)
    malformed["pendingCallback"]["observedChronologyCount"] = 2
    status.write_text(
        "milestone:observer-loaded\n"
        + service_menu_lifecycle.STATUS_PREFIX
        + json.dumps(malformed)
        + "\n",
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="observed chronology-count"):
        service_menu_lifecycle._failure_diagnostic(status, _static())
    malformed = copy.deepcopy(payload)
    malformed["pendingCallback"]["observedChronology"].insert(
        2, {"role": "caller-call-site", "pc": 0x474BE}
    )
    malformed["pendingCallback"]["observedChronologyCount"] = 4
    status.write_text(
        "milestone:observer-loaded\n"
        + service_menu_lifecycle.STATUS_PREFIX
        + json.dumps(malformed)
        + "\n",
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="expected prefix"):
        service_menu_lifecycle._failure_diagnostic(status, _static())
    malformed = copy.deepcopy(payload)
    malformed["stackReadback"]["expectedTopLongword"] = 0
    status.write_text(
        "milestone:observer-loaded\n"
        + service_menu_lifecycle.STATUS_PREFIX
        + json.dumps(malformed)
        + "\n",
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="service-entry stack diagnostic"):
        service_menu_lifecycle._failure_diagnostic(status, _static())
    status.write_text(
        "milestone:observer-loaded\n"
        + service_menu_lifecycle.STATUS_PREFIX
        + json.dumps(payload)
        + "\n",
        encoding="utf-8",
    )
    with pytest.raises(RuntimeError, match="observer callback failure"):
        service_menu_lifecycle.assert_observer_status(
            status,
            owner=service_menu_lifecycle.OWNER,
            schema_path=service_menu_lifecycle.FAILURE_SCHEMA,
        )
    malformed = copy.deepcopy(payload)
    malformed["restoration"]["callbacksCleared"] = False
    with pytest.raises(ValueError, match="True"):
        validate_json(malformed, service_menu_lifecycle.FAILURE_SCHEMA, owner="failure")
    malformed = copy.deepcopy(payload)
    malformed["caseId"] = "unbounded-case-id"
    with pytest.raises(ValueError, match="not valid under any"):
        validate_json(malformed, service_menu_lifecycle.FAILURE_SCHEMA, owner="failure")
    tail = copy.deepcopy(payload)
    tail.update(
        {
            "caseId": "map-entity-tail-church",
            "phase": "tail-transfer",
            "role": "tail-transfer-site",
            "actualPc": 0x560B8,
            "expectedCallPc": 0x560B8,
            "expectedTargetPc": 0x20A02,
            "expectedReturnPc": None,
        }
    )
    tail["pendingCallback"].update(
        {
            "caseIndex": 13,
            "expectedCaseId": "map-entity-tail-church",
            "rolesAtPc": ["tail-transfer-site"],
        }
    )
    validate_json(tail, service_menu_lifecycle.FAILURE_SCHEMA, owner="tail failure")
    generated = copy.deepcopy(payload)
    generated.update(
        {
            "phase": "generated-service-stub",
            "role": "generated-service-cancel-stub",
            "actualPc": 0xFF6D00,
            "expectedTargetPc": 0xFF6D00,
        }
    )
    generated["pendingCallback"]["rolesAtPc"] = ["generated-service-cancel-stub"]
    validate_json(generated, service_menu_lifecycle.FAILURE_SCHEMA, owner="generated stub failure")
    generated["phase"] = "service-entry"
    with pytest.raises(ValueError, match="generated-service-stub"):
        validate_json(
            generated, service_menu_lifecycle.FAILURE_SCHEMA, owner="generated stub failure"
        )
    status.write_text(
        "milestone:observer-loaded\n"
        + service_menu_lifecycle.STATUS_PREFIX
        + json.dumps(payload)
        + "\nlate-row\n",
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="terminal and unique"):
        service_menu_lifecycle._failure_diagnostic(status)


def test_cli_and_bootstrap_own_exactly_one_direct_function_launch() -> None:
    from sf2tool.h3.bootstrap import COMMAND_LAUNCHES, observer_profile

    launch = COMMAND_LAUNCHES["service-menu-lifecycle"]
    assert launch.expected_launches == 1
    assert launch.dispatch_module == "sf2tool.h3.service_menu_lifecycle"
    assert launch.dispatch_function == "verify_service_menu_lifecycle"
    assert observer_profile(service_menu_lifecycle.OBSERVER).name == "direct-function-seam"
