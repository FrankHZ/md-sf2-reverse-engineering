from __future__ import annotations

import json
from copy import deepcopy
from pathlib import Path

import pytest

from sf2tool.h3.bizhawk import bizhawk_contract, validate_lua_syntax
from sf2tool.h3.map_script_transition import (
    FIXTURE,
    FIXTURE_SCHEMA,
    OBSERVATION_SCHEMA,
    OBSERVER,
    _callback_failure_status,
    _failure_expectations,
    _h1_instruction_address,
    _listing_call_sites,
    _runtime_cases,
    _validate_failure_expectations,
    derive_case_expectations,
    runtime_navigation,
)
from sf2tool.jsonio import load_json, validate_json

UPSTREAM = Path("local/upstream/SF2DISASM")
H2_FIXTURE = Path("tests/fixtures/h2/map-script-engine-static-v1.json")


def _static() -> dict[str, object]:
    expected = load_json(H2_FIXTURE)["expected"]
    return {
        "transitionCommandFacts": deepcopy(expected["transitionCommandFacts"]),
        "mapLifecycleCommandFacts": deepcopy(expected["mapLifecycleCommandFacts"]),
    }


def _cases() -> list[dict[str, object]]:
    return [
        {"id": "warp-event", "macro": "warp", "operandWords": [4, 5, 6, 7]},
        {"id": "reset-current", "macro": "resetMap", "operandWords": []},
        {"id": "fade-load", "macro": "loadMapFadeIn", "operandWords": [4, 1, 2]},
        {"id": "reload-current", "macro": "reloadMap", "operandWords": [3, 5]},
        {"id": "map-load", "macro": "mapLoad", "operandWords": [4, 6, 7]},
    ]


def test_transition_navigation_is_h1_derived_for_all_five_handlers() -> None:
    navigation = runtime_navigation(_static(), UPSTREAM)
    assert navigation["function"] == {
        "entryAddress": 292092,
        "entryInjectionCallSiteAddress": 292114,
        "executeMapScriptAddress": 291116,
        "scriptWordReadAfterAddress": 291160,
        "opcodeDispatchCallAddress": 291194,
        "opcodeDispatchReturnAddress": 291198,
        "endAddress": 291380,
        "warpHandlerAddress": 291714,
        "resetHandlerAddress": 288142,
        "fadeHandlerAddress": 288154,
        "reloadHandlerAddress": 288520,
        "mapLoadHandlerAddress": 288182,
    }
    assert navigation["callSites"] == {
        "warp": [],
        "resetMap": [{"address": 288144, "target": "ResetCurrentMap"}],
        "loadMapFadeIn": [
            {"address": 288192, "target": "LoadMapTilesets"},
            {"address": 288196, "target": "WaitForVInt"},
            {"address": 288236, "target": "LoadMap"},
            {"address": 288242, "target": "EnableDisplayAndInterrupts"},
            {"address": 288254, "target": "WaitForVInt"},
        ],
        "reloadMap": [
            {"address": 288558, "target": "LoadMap"},
            {"address": 288564, "target": "EnableDisplayAndInterrupts"},
            {"address": 288576, "target": "WaitForVInt"},
        ],
        "mapLoad": [
            {"address": 288192, "target": "LoadMapTilesets"},
            {"address": 288196, "target": "WaitForVInt"},
            {"address": 288236, "target": "LoadMap"},
            {"address": 288242, "target": "EnableDisplayAndInterrupts"},
            {"address": 288254, "target": "WaitForVInt"},
        ],
    }
    assert navigation["service"] == {
        "ResetCurrentMap": 15878,
        "LoadMapTilesets": 10722,
        "LoadMap": 10892,
        "EnableDisplayAndInterrupts": 3142,
        "WaitForVInt": 3822,
    }
    assert navigation["resetTail"] == {
        "branchAddress": 15932,
        "nestedServiceSites": [
            {
                "address": 11574,
                "target": "EnableDisplayAndInterrupts",
                "returnAddress": 11578,
            },
            {
                "address": 11594,
                "target": "WaitForVInt",
                "returnAddress": 11598,
            },
        ],
    }
    assert navigation["fadeSourceWrite"] == {
        "address": 288154,
        "symbol": "OUT_TO_BLACK",
        "value": 2,
    }


def test_transition_static_cases_derive_exact_cursor_handler_and_service_shape() -> None:
    derived = derive_case_expectations(_static(), _cases())
    assert [row["scriptBytes"] for row in derived] == [
        [0, 7, 4, 5, 6, 7, 255, 255],
        [0, 54, 255, 255],
        [0, 55, 0, 4, 0, 1, 0, 2, 255, 255],
        [0, 70, 0, 3, 0, 5, 255, 255],
        [0, 72, 0, 4, 0, 6, 0, 7, 255, 255],
    ]
    assert [row["expected"] for row in derived] == [
        {
            "id": "warp-event",
            "macro": "warp",
            "handlerEntries": ["ExecuteMapScript", "csc07_warp"],
            "scriptWordReads": [
                {"word": 7, "cursorAfterReadOffset": 2},
                {"word": 65535, "cursorAfterReadOffset": 8},
            ],
            "cursorAfterHandlerOffset": 6,
            "handlerReturned": True,
            "fallthroughCsc48Observed": False,
            "serviceCallOrder": [],
        },
        {
            "id": "reset-current",
            "macro": "resetMap",
            "handlerEntries": ["ExecuteMapScript", "csc36_resetMap"],
            "scriptWordReads": [
                {"word": 54, "cursorAfterReadOffset": 2},
                {"word": 65535, "cursorAfterReadOffset": 4},
            ],
            "cursorAfterHandlerOffset": 2,
            "handlerReturned": True,
            "fallthroughCsc48Observed": False,
            "serviceCallOrder": ["ResetCurrentMap"],
        },
        {
            "id": "fade-load",
            "macro": "loadMapFadeIn",
            "handlerEntries": ["ExecuteMapScript", "csc37_loadMapAndFadeIn", "csc48_loadMap"],
            "scriptWordReads": [
                {"word": 55, "cursorAfterReadOffset": 2},
                {"word": 65535, "cursorAfterReadOffset": 10},
            ],
            "cursorAfterHandlerOffset": 8,
            "handlerReturned": True,
            "fallthroughCsc48Observed": True,
            "serviceCallOrder": [
                "LoadMapTilesets",
                "WaitForVInt",
                "LoadMap",
                "EnableDisplayAndInterrupts",
                "WaitForVInt",
            ],
        },
        {
            "id": "reload-current",
            "macro": "reloadMap",
            "handlerEntries": ["ExecuteMapScript", "csc46_reloadMap"],
            "scriptWordReads": [
                {"word": 70, "cursorAfterReadOffset": 2},
                {"word": 65535, "cursorAfterReadOffset": 8},
            ],
            "cursorAfterHandlerOffset": 6,
            "handlerReturned": True,
            "fallthroughCsc48Observed": False,
            "serviceCallOrder": ["LoadMap", "EnableDisplayAndInterrupts", "WaitForVInt"],
        },
        {
            "id": "map-load",
            "macro": "mapLoad",
            "handlerEntries": ["ExecuteMapScript", "csc48_loadMap"],
            "scriptWordReads": [
                {"word": 72, "cursorAfterReadOffset": 2},
                {"word": 65535, "cursorAfterReadOffset": 10},
            ],
            "cursorAfterHandlerOffset": 8,
            "handlerReturned": True,
            "fallthroughCsc48Observed": False,
            "serviceCallOrder": [
                "LoadMapTilesets",
                "WaitForVInt",
                "LoadMap",
                "EnableDisplayAndInterrupts",
                "WaitForVInt",
            ],
        },
    ]


def test_transition_parser_rejects_instruction_and_source_use_site_mutations() -> None:
    static = _static()
    for row in static["transitionCommandFacts"]["macros"]:
        if row["name"] == "reloadMap":
            row["encodedBytes"] = 8
    with pytest.raises(ValueError, match="ABI/handler binding drift"):
        derive_case_expectations(static, _cases())

    static = _static()
    static["transitionCommandFacts"]["callerBreakdown"]["targetResolutions"][2][
        "effectiveTarget"
    ] = "j_LoadMap"
    with pytest.raises(ValueError, match="caller target-resolution drift"):
        derive_case_expectations(static, _cases())

    section = "\n".join(
        [
            "00000010 4EBB 0000 jsr rjt_cutsceneScriptCommands(pc,d0.w)",
            "00000014 ; jsr rjt_cutsceneScriptCommands(pc,d0.w)",
            "00000016 map_jsr_rjt_cutsceneScriptCommands_pc_d0_w:",
        ]
    )
    assert (
        _h1_instruction_address(section, "jsr rjt_cutsceneScriptCommands(pc,d0.w)", owner="test")
        == 16
    )
    with pytest.raises(ValueError, match="0 matches"):
        _h1_instruction_address(section, "jsr (LoadMap).w", owner="test")

    call_listing = "\n".join(
        [
            "00000010                            test_calls:",
            "00000010 4EB8 2A8C                 jsr     (LoadMap).w",
            "00000014 4EB9 0000 0EEE            jsr WaitForVInt.l",
            "0000001A 4E71                       ; jsr (CommentOnly).w",
            "0000001C                            label_jsr_LoadMap:",
            "0000001C 4E71                       move.w (OperandOnly).w,d0",
            "0000001E 4E71                       jsrish (NearMiss).w",
            "00000020 4EB8 0000                 jsr (TrailingOperand).w,d0",
            "00000024 4EB8 0000                 jsr (TrailingComment).w ; no",
            "00000028                            ; End of function test_calls",
        ]
    )
    assert _listing_call_sites(call_listing, "test_calls", ["LoadMap", "WaitForVInt"]) == [
        {"address": 16, "target": "LoadMap"},
        {"address": 20, "target": "WaitForVInt"},
    ]
    with pytest.raises(ValueError, match="call-site target/order drift"):
        _listing_call_sites(
            call_listing.replace("jsr     (LoadMap).w", "jsr     (WrongTarget).w"),
            "test_calls",
            ["LoadMap", "WaitForVInt"],
        )


def test_transition_fixture_derivation_schemas_and_shared_pc_roles_are_complete() -> None:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner="transition fixture")
    changed_fixture = deepcopy(fixture)
    changed_fixture["fadeSourceWrite"]["value"] = 1
    with pytest.raises(ValueError, match="failed schema validation"):
        validate_json(changed_fixture, FIXTURE_SCHEMA, owner="transition fade source mutation")
    for mutation in (
        lambda value: value["cases"][0].update({"initialCurrentMap": 4}),
        lambda value: value["cases"].reverse(),
        lambda value: value["cases"].pop(),
        lambda value: value["cases"].append(deepcopy(value["cases"][0])),
    ):
        changed_fixture = deepcopy(fixture)
        mutation(changed_fixture)
        with pytest.raises(ValueError, match="failed schema validation"):
            validate_json(changed_fixture, FIXTURE_SCHEMA, owner="transition case corpus mutation")
    static = _static()
    navigation = runtime_navigation(static, UPSTREAM)
    listing = (UPSTREAM / "build/sf2build-h1.lst").read_text(encoding="utf-8")
    cases = _runtime_cases(static, fixture, navigation, listing)
    assert [case["expected"] for case in cases] == [case["expected"] for case in fixture["cases"]]
    expectations = _failure_expectations(navigation, fixture, cases, listing)
    _validate_failure_expectations(expectations, navigation, fixture, cases, listing)
    assert expectations[str(fixture["function"]["entryAddress"])]["roles"]["wrapper-entry"] == {
        "callSiteAddress": None,
        "targetAddress": None,
        "returnAddress": None,
    }
    assert expectations[str(fixture["instrumentation"]["stubAddress"])]["roles"][
        "trampoline-entry"
    ] == {
        "callSiteAddress": 292114,
        "targetAddress": 65416,
        "returnAddress": 292120,
    }
    assert expectations[str(fixture["function"]["executeMapScriptAddress"])]["roles"][
        "execute-entry"
    ] == {
        "callSiteAddress": 65422,
        "targetAddress": 291116,
        "returnAddress": 65428,
    }
    assert expectations[str(fixture["function"]["opcodeDispatchReturnAddress"])]["roles"] == {
        "dispatcher-return:warp-event": {
            "callSiteAddress": 291194,
            "targetAddress": 291714,
            "returnAddress": 291198,
        },
        "dispatcher-return:reset-current": {
            "callSiteAddress": 291194,
            "targetAddress": 288142,
            "returnAddress": 291198,
        },
        "dispatcher-return:fade-load": {
            "callSiteAddress": 291194,
            "targetAddress": 288154,
            "returnAddress": 291198,
        },
        "dispatcher-return:reload-current": {
            "callSiteAddress": 291194,
            "targetAddress": 288520,
            "returnAddress": 291198,
        },
        "dispatcher-return:map-load": {
            "callSiteAddress": 291194,
            "targetAddress": 288182,
            "returnAddress": 291198,
        },
    }
    for address, role in (
        (fixture["function"]["scriptWordReadAfterAddress"], "script-word-read"),
        (fixture["function"]["endAddress"], "script-end"),
        (fixture["instrumentation"]["trampolinePostHandlerAddress"], "trampoline-complete"),
    ):
        assert expectations[str(address)]["roles"][role] == {
            "callSiteAddress": None,
            "targetAddress": None,
            "returnAddress": None,
        }
    csc48 = str(fixture["function"]["mapLoadHandlerAddress"])
    wait_seam = str(288196)
    assert set(expectations[csc48]["roles"]) == {
        "fallthrough:fade-load",
        "handler:map-load",
    }
    assert set(expectations[wait_seam]["roles"]) == {
        "service:fade-load:1:LoadMapTilesets:return",
        "service:fade-load:2:WaitForVInt:call",
        "service:map-load:1:LoadMapTilesets:return",
        "service:map-load:2:WaitForVInt:call",
    }
    assert expectations[str(fixture["service"]["LoadMap"])]["roles"][
        "reset-tail:LoadMap:entry:reset-current"
    ] == {
        "callSiteAddress": 15932,
        "targetAddress": fixture["service"]["LoadMap"],
        "returnAddress": 288150,
    }
    assert set(expectations[str(fixture["service"]["LoadMap"])]["roles"]) == {
        "reset-tail:LoadMap:entry:reset-current",
        "service:fade-load:3:LoadMap:entry",
        "service:reload-current:1:LoadMap:entry",
        "service:map-load:3:LoadMap:entry",
    }
    assert expectations[str(fixture["service"]["WaitForVInt"])]["roles"][
        "service:fade-load:3:LoadMap:nested:WaitForVInt:entry"
    ] == {
        "callSiteAddress": 11594,
        "targetAddress": fixture["service"]["WaitForVInt"],
        "returnAddress": 11598,
    }
    for mutation in (
        lambda value: value[str(fixture["function"]["opcodeDispatchReturnAddress"])]["roles"].pop(
            "dispatcher-return:warp-event"
        ),
        lambda value: value[
            str(fixture["function"]["opcodeDispatchReturnAddress"])
        ]["roles"].update({"dispatcher-return:unexpected": {}}),
        lambda value: value[str(fixture["function"]["opcodeDispatchReturnAddress"])]["roles"][
            "dispatcher-return:map-load"
        ].update({"returnAddress": 0}),
    ):
        changed = deepcopy(expectations)
        mutation(changed)
        with pytest.raises(ValueError, match="expectation contract drift"):
            _validate_failure_expectations(changed, navigation, fixture, cases, listing)


def test_transition_callback_failure_pending_state_is_exact(tmp_path: Path) -> None:
    status = tmp_path / "transition.status.txt"
    payload = {
        "owner": "map-script-transition",
        "caseId": "fade-load",
        "phase": "service-seam",
        "actualPc": 288196,
        "expectedCallSiteAddress": 288196,
        "expectedTargetAddress": 3822,
        "expectedReturnAddress": 288200,
        "pendingCallback": {
            "active": True,
            "phase": "service-seam",
            "role": "service:fade-load:2:WaitForVInt:call",
            "handlerEntriesObserved": [
                "ExecuteMapScript",
                "csc37_loadMapAndFadeIn",
                "csc48_loadMap",
            ],
            "scriptWordReadCount": 1,
            "dispatchTargetAddress": 288154,
            "pendingService": {
                "callSiteAddress": 288196,
                "target": "WaitForVInt",
                "targetAddress": 3822,
                "returnAddress": 288200,
                "role": "service:fade-load:2:WaitForVInt",
            },
        },
        "error": "forced callback failure",
    }
    status.write_text(
        "failure:observer-callback:" + json.dumps(payload) + "\n",
        encoding="utf-8",
    )
    assert _callback_failure_status(status) == payload
    malformed = deepcopy(payload)
    del malformed["pendingCallback"]["pendingService"]["target"]
    status.write_text(
        "failure:observer-callback:" + json.dumps(malformed) + "\n",
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="pendingCallback.pendingService"):
        _callback_failure_status(status)
    malformed = deepcopy(payload)
    malformed["pendingCallback"]["scriptWordReadCount"] = -1
    status.write_text(
        "failure:observer-callback:" + json.dumps(malformed) + "\n",
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="pendingCallback.scriptWordReadCount"):
        _callback_failure_status(status)


def test_transition_observation_schema_and_lua_callback_contract_are_closed() -> None:
    fixture = load_json(FIXTURE)
    observation = {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["id"],
        "mapTest": fixture["mapTestIndex"],
        "recordOrder": [case["id"] for case in fixture["cases"]],
        "records": [case["expected"] for case in fixture["cases"]],
    }
    validate_json(observation, OBSERVATION_SCHEMA, owner="transition observation")
    changed = deepcopy(observation)
    changed["records"][2]["unexpected"] = True
    with pytest.raises(ValueError, match="failed schema validation"):
        validate_json(changed, OBSERVATION_SCHEMA, owner="nested observation mutation")
    for mutation in (
        lambda value: value["records"][0].update({"currentMapAfter": 4}),
        lambda value: value["records"].reverse(),
        lambda value: value["records"].pop(),
        lambda value: value["records"].append(deepcopy(value["records"][0])),
    ):
        changed = deepcopy(observation)
        mutation(changed)
        with pytest.raises(ValueError, match="failed schema validation"):
            validate_json(changed, OBSERVATION_SCHEMA, owner="transition record corpus mutation")
    source = OBSERVER.read_text(encoding="utf-8")
    assert "duplicate physical-PC callback" in source
    assert "failure:observer-callback:" not in source
    assert "observerFailureContract.statusPrefix" in source
    assert "callbacks-cleared:0" in source
    assert 'set_role("dispatcher-return:"..current_case().id)' in source
    _, executable = bizhawk_contract()
    validate_lua_syntax(OBSERVER, executable)
