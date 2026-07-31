from __future__ import annotations

from copy import deepcopy
from pathlib import Path

import pytest

import sf2tool.h3.map_script_ui_primary as ui_primary
from sf2tool.h2.map_script_engine import build_map_script_engine_contract
from sf2tool.h3.map_script_ui_primary import (
    _instruction_width,
    _observer_cases,
    _service_interception,
    _source_section,
    build_map_script_ui_primary_contract,
    derive_case_expectations,
)
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

FIXTURE = repo_path("tests/fixtures/h3/map-script-ui-primary-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h3-map-script-ui-primary-fixture.schema.json")
OBSERVATION_SCHEMA = repo_path("schemas/h3-map-script-ui-primary-observation.schema.json")
OBSERVER = repo_path("tools/bizhawk/map_script_ui_primary_observer.lua")


def _static() -> dict:
    return build_map_script_ui_primary_contract(
        repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
    )


def _observation(fixture: dict) -> dict:
    return {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["id"],
        "mapTest": fixture["mapTestIndex"],
        "recordOrder": [case["id"] for case in fixture["cases"]],
        "records": [{**case["expected"], **case["runtimeGolden"]} for case in fixture["cases"]],
    }


def _assert_all_object_schemas_closed(schema: dict) -> None:
    failures: list[str] = []

    def visit(value: object, path: str) -> None:
        if isinstance(value, dict):
            if (
                value.get("type") == "object"
                and "properties" in value
                and (
                    value.get("additionalProperties") is not False
                    or set(value.get("required", [])) != set(value["properties"])
                )
            ):
                failures.append(path)
            for key, child in value.items():
                visit(child, f"{path}/{key}")
        elif isinstance(value, list):
            for index, child in enumerate(value):
                visit(child, f"{path}/{index}")

    visit(schema, "#")
    assert not failures, "open or incomplete object schemas: " + ", ".join(failures)


def test_static_contract_keeps_full_source_rows_distinct_from_compact_h2_boundary() -> None:
    static = _static()
    fixture = load_json(FIXTURE)
    facts = static["sourceFacts"]

    assert static["provenance"] == fixture["provenance"]
    assert static["function"] == fixture["function"] == {
        "entryAddress": 292092,
        "csc1D_showPortraitAddress": 289432,
        "csc1E_hidePortraitAddress": 289490,
        "csc12_executeContextMenuAddress": 292022,
        "csc1D_showPortraitReturnInstructionAddress": 289488,
        "csc1E_hidePortraitReturnInstructionAddress": 289500,
        "csc12_executeContextMenuReturnInstructionAddress": 292062,
        "showPortraitFirstOperandFollowupAddress": 289434,
        "showPortraitSentinelCompareAddress": 289470,
        "menuFirstOperandFollowupAddress": 292024,
    }
    assert static["ram"] == fixture["ram"] == {"portraitWindowIndexAddress": 16756864}
    assert static["constants"] == fixture["constants"] == {
        "showPortraitCursorAdvanceByteCount": 2,
        "menuCursorAdvanceByteCount": 2,
        "menuSavedA6StackByteCount": 4,
        "menuRestoredA6StackByteCount": 4,
        "signedWordSentinel": 65535,
    }
    assert fixture["sourceContract"] == facts["compactSourceBoundary"]
    assert [row["sourceOrderKey"] for row in facts["sourceInputRows"]] == [
        "cs_615E6:558:showPortrait",
        "data/scripting/map/debugscripts.asm#L16:2:showPortrait",
        "data/scripting/map/debugscripts.asm#L26:3:showPortrait",
        "data/scripting/map/debugscripts.asm#L76:8:showPortrait",
    ]
    assert [row["handlerInputWord"] for row in facts["sourceInputRows"]] == [32900, 0, 0, 0]
    assert facts["compactSourceBoundary"]["sourceSiteOrderKeys"][-2:] == [
        "data/scripting/map/debugscripts.asm#L26:6:hidePortrait",
        "data/scripting/map/debugscripts.asm#L76:8:showPortrait",
    ]
    assert facts["callerBreakdown"]["instructionTargetTotals"] == {
        "WaitForViewScrollEnd": 2,
        "GetEntityPortaitAndSpeechSfx": 1,
        "j_OpenPortraitWindow": 1,
        "j_ClosePortraitWindow": 1,
        "j_ChurchMenu": 1,
        "j_ShopMenu": 1,
        "j_BlacksmithMenu": 1,
    }
    assert facts["callerBreakdown"]["effectiveTargetTotals"] == {
        "WaitForViewScrollEnd": 2,
        "GetEntityPortaitAndSpeechSfx": 1,
        "OpenPortraitWindow": 1,
        "ClosePortraitWindow": 1,
        "ChurchMenu": 1,
        "ShopMenu": 1,
        "BlacksmithMenu": 1,
    }
    assert facts["handlers"][0]["sentinelCompareUseSite"] == {
        "address": 289470,
        "instruction": "cmpi.w #-1,d1",
        "widthBytes": 2,
        "parsedImmediate": -1,
        "unsignedValue": 65535,
    }
    assert static["runtimeQuestions"] == [
        "map-script-ui-command/normal-story-reachability",
        "map-script-ui-command/full-window-animation-vdp-timing",
        "map-script-ui-command/real-user-choice-service-side-effects",
        "map-script-ui-command/save-persistence-map-entity-interactions",
    ]


def test_complete_eleven_case_derivation_matches_fixture_and_has_no_lua_runtime_golden() -> None:
    fixture = load_json(FIXTURE)
    static = _static()

    assert derive_case_expectations(static, fixture) == [
        case["expected"] for case in fixture["cases"]
    ]
    assert [(case["id"], case["kind"]) for case in fixture["cases"]] == [
        ("show-source-0", "show-source"),
        ("show-source-1", "show-source"),
        ("show-source-2", "show-source"),
        ("show-source-3", "show-source"),
        ("show-busy-early-return", "show-busy"),
        ("show-sentinel-d1-branch", "show-sentinel"),
        ("hide-close-chronology", "hide"),
        ("menu-selector-0", "menu"),
        ("menu-selector-1", "menu"),
        ("menu-selector-2", "menu"),
        ("menu-selector-other", "menu"),
    ]
    assert [case["expected"]["directCallbackPlan"] for case in fixture["cases"]] == [
        [
            {
                "instructionTarget": "WaitForViewScrollEnd",
                "effectiveTarget": "WaitForViewScrollEnd",
                "callSiteAddress": 289462,
                "returnAddress": 289466,
            },
            {
                "instructionTarget": "GetEntityPortaitAndSpeechSfx",
                "effectiveTarget": "GetEntityPortaitAndSpeechSfx",
                "callSiteAddress": 289466,
                "returnAddress": 289470,
            },
            {
                "instructionTarget": "j_OpenPortraitWindow",
                "effectiveTarget": "OpenPortraitWindow",
                "callSiteAddress": 289482,
                "returnAddress": 289488,
            },
        ],
        *[
            [
                {
                    "instructionTarget": "WaitForViewScrollEnd",
                    "effectiveTarget": "WaitForViewScrollEnd",
                    "callSiteAddress": 289462,
                    "returnAddress": 289466,
                },
                {
                    "instructionTarget": "GetEntityPortaitAndSpeechSfx",
                    "effectiveTarget": "GetEntityPortaitAndSpeechSfx",
                    "callSiteAddress": 289466,
                    "returnAddress": 289470,
                },
                {
                    "instructionTarget": "j_OpenPortraitWindow",
                    "effectiveTarget": "OpenPortraitWindow",
                    "callSiteAddress": 289482,
                    "returnAddress": 289488,
                },
            ]
        ]
        * 3,
        [],
        [
            {
                "instructionTarget": "WaitForViewScrollEnd",
                "effectiveTarget": "WaitForViewScrollEnd",
                "callSiteAddress": 289462,
                "returnAddress": 289466,
            },
            {
                "instructionTarget": "GetEntityPortaitAndSpeechSfx",
                "effectiveTarget": "GetEntityPortaitAndSpeechSfx",
                "callSiteAddress": 289466,
                "returnAddress": 289470,
            },
        ],
        [
            {
                "instructionTarget": "WaitForViewScrollEnd",
                "effectiveTarget": "WaitForViewScrollEnd",
                "callSiteAddress": 289490,
                "returnAddress": 289494,
            },
            {
                "instructionTarget": "j_ClosePortraitWindow",
                "effectiveTarget": "ClosePortraitWindow",
                "callSiteAddress": 289494,
                "returnAddress": 289500,
            },
        ],
        [
            {
                "instructionTarget": "j_ChurchMenu",
                "effectiveTarget": "ChurchMenu",
                "callSiteAddress": 292030,
                "returnAddress": 292036,
            }
        ],
        [
            {
                "instructionTarget": "j_ShopMenu",
                "effectiveTarget": "ShopMenu",
                "callSiteAddress": 292042,
                "returnAddress": 292048,
            }
        ],
        [
            {
                "instructionTarget": "j_BlacksmithMenu",
                "effectiveTarget": "BlacksmithMenu",
                "callSiteAddress": 292054,
                "returnAddress": 292060,
            }
        ],
        [],
    ]
    assert [case["expected"]["scriptCursorRamOffsetAfter"] for case in fixture["cases"]] == [
        6,
        6,
        6,
        6,
        6,
        6,
        4,
        6,
        6,
        6,
        6,
    ]
    assert [case["expected"]["a6RestoredFromStack"] for case in fixture["cases"][-4:]] == [
        True,
        True,
        True,
        True,
    ]
    def observed_callback(
        instruction_target: str,
        effective_target: str,
        call_site_address: int,
        target_role: str,
        target_address: int,
        return_address: int,
    ) -> dict:
        return {
            "instructionTarget": instruction_target,
            "effectiveTarget": effective_target,
            "callSiteAddressObserved": call_site_address,
            "targetRole": target_role,
            "targetAddressObserved": target_address,
            "returnAddressObserved": return_address,
        }

    wait = observed_callback(
        "WaitForViewScrollEnd", "WaitForViewScrollEnd", 289462, "effective", 18184, 289466
    )
    helper = observed_callback(
        "GetEntityPortaitAndSpeechSfx",
        "GetEntityPortaitAndSpeechSfx",
        289466,
        "effective",
        284216,
        289470,
    )
    open_window = observed_callback(
        "j_OpenPortraitWindow", "OpenPortraitWindow", 289482, "instruction", 65592, 289488
    )
    assert [case["runtimeGolden"]["callbackDispatchesObserved"] for case in fixture["cases"]] == [
        [wait, helper, open_window],
        *[[wait, helper, open_window]] * 3,
        [],
        [wait, helper],
        [
            observed_callback(
                "WaitForViewScrollEnd", "WaitForViewScrollEnd", 289490, "effective", 18184, 289494
            ),
            observed_callback(
                "j_ClosePortraitWindow",
                "ClosePortraitWindow",
                289494,
                "instruction",
                65596,
                289500,
            ),
        ],
        [
            observed_callback(
                "j_ChurchMenu", "ChurchMenu", 292030, "instruction", 131076, 292036
            )
        ],
        [observed_callback("j_ShopMenu", "ShopMenu", 292042, "instruction", 131072, 292048)],
        [
            observed_callback(
                "j_BlacksmithMenu", "BlacksmithMenu", 292054, "instruction", 131084, 292060
            )
        ],
        [],
    ]
    runtime_fields = (
        "handlerReturned",
        "handlerInputWordAtFirstOperandUse",
        "portraitWindowBusyEarlyReturnObserved",
        "sentinelD1BranchObserved",
        "helperD1WordAtComparison",
        "stackPointerDeltaBytesObserved",
        "a6AtMenuSaveBoundaryObserved",
        "a6RestoredFromStackObserved",
        "scriptCursorRamOffsetAfterObserved",
    )
    assert [
        tuple(case["runtimeGolden"][field] for field in runtime_fields) for case in fixture["cases"]
    ] == [
        (True, 32900, False, False, 1, 0, None, None, 6),
        (True, 0, False, False, 1, 0, None, None, 6),
        (True, 0, False, False, 1, 0, None, None, 6),
        (True, 0, False, False, 1, 0, None, None, 6),
        (True, 32900, True, False, None, 0, None, None, 6),
        (True, 32900, False, True, 65535, 0, None, None, 6),
        (True, None, False, False, None, 0, None, None, 4),
        (True, 0, False, False, None, 0, 16728070, True, 6),
        (True, 1, False, False, None, 0, 16728070, True, 6),
        (True, 2, False, False, None, 0, 16728070, True, 6),
        (True, 3, False, False, None, 0, 16728070, True, 6),
    ]
    observer_cases = _observer_cases(fixture)
    assert all("runtimeGolden" not in row and "expected" not in row for row in observer_cases)
    assert {key for row in observer_cases for key in row} == {
        "id",
        "kind",
        "portraitWindowIndexWordSeed",
        "handlerInputWord",
        "helperD1Word",
    }


def test_source_guards_reject_use_site_opcode_and_order_mutations_before_fixture_compare(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    h2_output = build_map_script_engine_contract(
        repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
    )
    monkeypatch.setattr(ui_primary, "build_map_script_engine_contract", lambda *_: h2_output)
    original_read_text = Path.read_text

    def altered_read_text(path: Path, *args: object, **kwargs: object) -> str:
        source = original_read_text(path, *args, **kwargs)
        if path.name == "mapscriptengine_2.asm":
            return source.replace("cmpi.w  #2,d0", "cmpi.w  #3,d0", 1)
        return source

    monkeypatch.setattr(Path, "read_text", altered_read_text)
    with pytest.raises(ValueError, match="control-section source guard"):
        _static()


def test_full_h2_show_row_mutation_fails_before_h3_fixture_comparison(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    h2_output = build_map_script_engine_contract(
        repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
    )
    altered = deepcopy(h2_output)
    for site in altered["mapScriptUiPrimaryCommandFacts"]["sourceSites"]:
        for command in site["commands"]:
            if command["macro"] == "showPortrait":
                command["operandValues"][0]["resolvedValue"] ^= 1
                monkeypatch.setattr(
                    ui_primary, "build_map_script_engine_contract", lambda *_: altered
                )
                with pytest.raises(ValueError, match="compact/full source hash relation"):
                    _static()
                return
    raise AssertionError("expected a full-H2 showPortrait source row")


def test_sentinel_use_site_operand_mutation_fails_before_fixture_comparison(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    h2_output = build_map_script_engine_contract(
        repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
    )
    monkeypatch.setattr(ui_primary, "build_map_script_engine_contract", lambda *_: h2_output)
    original_read_text = Path.read_text

    def altered_read_text(path: Path, *args: object, **kwargs: object) -> str:
        source = original_read_text(path, *args, **kwargs)
        if path.name == "mapscriptengine_1.asm":
            return source.replace("cmpi.w  #-1,d1", "cmpi.w  #-2,d1", 1)
        return source

    monkeypatch.setattr(Path, "read_text", altered_read_text)
    with pytest.raises(ValueError, match="control-section source guard"):
        _static()


def test_service_shims_validate_targets_bytes_and_helper_return() -> None:
    static = _static()
    fixture = load_json(FIXTURE)
    assert [
        (row["targetIdentity"], row["targetRole"], row["address"], row["patchedHex"])
        for row in _service_interception(static, fixture)
    ] == [
        ("WaitForViewScrollEnd", "effective", 18184, "4E75"),
        ("GetEntityPortaitAndSpeechSfx", "effective", 284216, "323900FF40084E75"),
        ("j_OpenPortraitWindow", "instruction", 65592, "4E75"),
        ("j_ClosePortraitWindow", "instruction", 65596, "4E75"),
        ("j_ChurchMenu", "instruction", 131076, "4E75"),
        ("j_ShopMenu", "instruction", 131072, "4E75"),
        ("j_BlacksmithMenu", "instruction", 131084, "4E75"),
    ]
    target_mutation = deepcopy(fixture)
    target_mutation["instrumentation"]["serviceInterception"]["patches"][0]["address"] = 18186
    with pytest.raises(ValueError, match="source target"):
        _service_interception(static, target_mutation)
    helper_mutation = deepcopy(fixture)
    helper_mutation["instrumentation"]["serviceInterception"]["patches"][1]["patchedHex"] = (
        "323900FF40084E74"
    )
    with pytest.raises(ValueError, match="byte-shape"):
        _service_interception(static, helper_mutation)
    original_bytes_mutation = deepcopy(fixture)
    original_bytes_mutation["instrumentation"]["serviceInterception"]["patches"][1][
        "originalHex"
    ] = "FFFFFFFFFFFFFFFF"
    with pytest.raises(ValueError, match="original ROM bytes"):
        ui_primary._instrument_ui_rom(
            repo_path("local/roms/sf2-us.bin"), original_bytes_mutation, static
        )


def test_small_parsers_handle_comments_boundaries_suffixes_and_near_misses() -> None:
    assert [
        _instruction_width(item)
        for item in ("move.b (a6)+,d0", "move.w (a6)+,d0", "move.l a6,-(sp)")
    ] == [
        1,
        2,
        4,
    ]
    for near_miss in ("move.s (a6)+,d0", "move (a6)+,d0", "label: move.w (a6)+,d0"):
        with pytest.raises(ValueError):
            _instruction_width(near_miss)
    source = "\n".join(
        (
            "csc1E_hidePortrait:",
            "  jsr (WaitForViewScrollEnd).w ; tracked instruction",
            "near_WaitForViewScrollEnd:",
            "  jsr j_ClosePortraitWindow",
            "  rts ; j_ClosePortraitWindow in a comment must not add a row",
            "; End of function csc1E_hidePortrait",
        )
    )
    assert _source_section(source, "csc1E_hidePortrait") == [
        "jsr (WaitForViewScrollEnd).w",
        "jsr j_ClosePortraitWindow",
        "rts",
    ]


def test_schemas_reject_nested_shape_order_and_boundary_mutations() -> None:
    fixture = load_json(FIXTURE)
    observation = _observation(fixture)
    validate_json(fixture, FIXTURE_SCHEMA, owner="map-script UI fixture")
    validate_json(observation, OBSERVATION_SCHEMA, owner="map-script UI observation")
    for schema_path in (FIXTURE_SCHEMA, OBSERVATION_SCHEMA):
        _assert_all_object_schemas_closed(load_json(schema_path))

    missing = deepcopy(fixture)
    del missing["cases"][0]["sourceInput"]["operandValues"][0]["rawValue"]
    with pytest.raises(ValueError):
        validate_json(missing, FIXTURE_SCHEMA, owner="map-script UI missing nested")
    renamed = deepcopy(fixture)
    operand = renamed["cases"][0]["sourceInput"]["operandValues"][0]
    operand["value"] = operand.pop("rawValue")
    with pytest.raises(ValueError):
        validate_json(renamed, FIXTURE_SCHEMA, owner="map-script UI renamed nested")
    extra = deepcopy(fixture)
    extra["cases"][0]["expected"]["directCallbackPlan"][0]["unexpected"] = True
    with pytest.raises(ValueError):
        validate_json(extra, FIXTURE_SCHEMA, owner="map-script UI extra nested")
    reordered = deepcopy(fixture)
    (
        reordered["sourceContract"]["showSourceOrderKeys"][0],
        reordered["sourceContract"]["showSourceOrderKeys"][1],
    ) = (
        reordered["sourceContract"]["showSourceOrderKeys"][1],
        reordered["sourceContract"]["showSourceOrderKeys"][0],
    )
    with pytest.raises(ValueError):
        validate_json(reordered, FIXTURE_SCHEMA, owner="map-script UI reordered source keys")
    out_of_bounds = deepcopy(fixture)
    out_of_bounds["cases"][0]["handlerInputWord"] = 65536
    with pytest.raises(ValueError):
        validate_json(out_of_bounds, FIXTURE_SCHEMA, owner="map-script UI input boundary")

    observation_missing = deepcopy(observation)
    del observation_missing["records"][0]["callbackDispatchesObserved"][0]["returnAddressObserved"]
    with pytest.raises(ValueError):
        validate_json(
            observation_missing, OBSERVATION_SCHEMA, owner="map-script UI observed missing"
        )
    observation_extra = deepcopy(observation)
    observation_extra["records"][0]["callbackDispatchesObserved"][0]["unexpected"] = True
    with pytest.raises(ValueError):
        validate_json(observation_extra, OBSERVATION_SCHEMA, owner="map-script UI observed extra")
    observation_reordered = deepcopy(observation)
    observation_reordered["recordOrder"][0], observation_reordered["recordOrder"][1] = (
        observation_reordered["recordOrder"][1],
        observation_reordered["recordOrder"][0],
    )
    with pytest.raises(ValueError):
        validate_json(
            observation_reordered, OBSERVATION_SCHEMA, owner="map-script UI observed order"
        )
    observation_boundary = deepcopy(observation)
    observation_boundary["records"][5]["helperD1WordAtComparison"] = 65534
    with pytest.raises(ValueError):
        validate_json(
            observation_boundary, OBSERVATION_SCHEMA, owner="map-script UI observed boundary"
        )
    observation_target_role = deepcopy(observation)
    observation_target_role["records"][0]["callbackDispatchesObserved"][0]["targetRole"] = "other"
    with pytest.raises(ValueError):
        validate_json(
            observation_target_role, OBSERVATION_SCHEMA, owner="map-script UI observed target role"
        )
    observation_a6_boundary = deepcopy(observation)
    observation_a6_boundary["records"][7]["a6AtMenuSaveBoundaryObserved"] = 16728071
    with pytest.raises(ValueError):
        validate_json(
            observation_a6_boundary, OBSERVATION_SCHEMA, owner="map-script UI observed menu A6"
        )


def test_observer_is_syntax_valid_and_documents_handler_local_interception() -> None:
    source = OBSERVER.read_text(encoding="utf-8")
    assert "helperD1SeedRamOffset" in source
    assert 'emu.setregister("M68K PC"' not in source
    assert "runtimeGolden" not in source
    assert "callback identity/order drift" in source
    assert "callSiteAddress=callback.callSiteAddress" not in source
    assert "returnAddress=callback.returnAddress" not in source
    assert "record.a6RestoredFromStackObserved=derived.a6RestoredFromStack" not in source
    assert "record.portraitWindowBusyEarlyReturnObserved=(case.kind" not in source
    assert "record.sentinelD1BranchObserved=(case.kind" not in source
    assert "observe_callback_return" in source
    assert "observe_callback_target" in source
    assert 'local pc=emu.getregister("M68K PC")' in source
    assert "callSiteAddressObserved=pc" in source
    assert "returnAddressObserved=pc" in source
    assert (
        'a6_restored_from_stack_observed=emu.getregister("M68K A6")==menu_a6_at_save_boundary'
        in source
    )
