"""Static and adversarial tests for the bounded Map 3 messenger H3 rail."""

from __future__ import annotations

import ctypes
import json
import re
import shutil
from copy import deepcopy
from functools import cache
from pathlib import Path
from typing import Any

import pytest
from jsonschema import Draft202012Validator

from sf2tool.h3 import map3_battle01_player_ready as player_ready
from sf2tool.h3 import map3_messenger_acceptance as rail
from sf2tool.h3.bizhawk import bizhawk_contract
from sf2tool.jsonio import load_json, validate_json


def _rom() -> Path:
    return rail.repo_path("local/roms/sf2-us.bin")


def _upstream() -> Path:
    return rail.repo_path("local/upstream/SF2DISASM")


@cache
def _contract() -> dict[str, Any]:
    return rail.build_map3_messenger_acceptance_source_contract(_rom(), _upstream())


def _unexpected_launch(**_: Any) -> dict[str, Any]:
    raise AssertionError("invalid prelaunch input must not launch BizHawk")


def _failure_payload() -> dict[str, Any]:
    return {
        "owner": rail.OWNER,
        "caseId": rail.CASE_IDS[0],
        "phase": "messenger",
        "role": "messenger-text-command",
        "actualPc": 291566,
        "expectedPc": 291566,
        "callbackCount": 0,
        "callbacksCleared": True,
        "outputRemoved": True,
        "restoration": {
            "scopeArmed": True,
            "gameFlags": True,
            "combatantAllyRecords": True,
            "mapAndBattleState": True,
            "playerEntity": True,
            "forceAndParty": True,
            "followerState": True,
            "touchedEntities": True,
            "dialogueAndInput": True,
            "cameraState": True,
            "bootstrapFrame": True,
            "gold": True,
            "generatedRam": True,
            "sessionCartPatches": True,
            "sessionStateRestored": True,
            "callbacksCleared": True,
            "outputRemoved": True,
        },
        "restorationMismatch": None,
        "error": "text command before Map 3 messenger body entry",
    }


def _run_lua_success_finalization_fault(
    tmp_path: Path,
    *,
    restoration_mismatch: bool,
    unregister_mode: str,
    output_remove_fault: bool,
) -> dict[str, Any]:
    """Run the observer's real deferred success/failure finalizers in Lua.

    The harness extracts the production cleanup, restoration, failure, and
    success-finalization functions. It supplies only their BizHawk-facing
    surface, then injects a first restoration mismatch or one unregister
    exception, or an output-removal failure. The fault must remain a single
    typed nonzero failure, with no success milestone left behind.
    """
    if unregister_mode not in {"none", "once", "persistent"}:
        raise ValueError(f"unknown unregister fault mode: {unregister_mode}")
    source = rail.OBSERVER.read_text(encoding="utf-8")
    helpers = source[
        source.index("local function cleanup_callbacks()") : source.index(
            "local function current_position()"
        )
    ]
    finalizer = source[
        source.index("local function finalize_success()") : source.index(
            'status("milestone:observer-started")'
        )
    ]
    result_path = (tmp_path / "lua-finalization-result.json").as_posix()
    status_path = (tmp_path / "lua-finalization-status.txt").as_posix()
    output_path = (tmp_path / "accepted-observation.json").as_posix()
    mismatch_literal = "true" if restoration_mismatch else "false"
    output_remove_literal = "true" if output_remove_fault else "false"
    harness = f'''\
local OWNER = "map3-messenger-acceptance"
local phase = "messenger"
local active = {{caseId = "natural-map3-messenger-accept-to-follower-ready-wait"}}
local saved_state = {{}}
local scope = {{
    gameFlags = {{1}}, combatantAllyRecords = {{0}}, mapAndBattleState = {{0}},
    playerEntity = {{0}}, forceAndParty = {{0}}, followerState = {{0}},
    touchedEntities = {{{{address = 120, values = {{0}}}}}}, dialogue = {{0}}, input = {{0}},
    cameraState = {{0}}, bootstrapFrame = {{a7 = 110, a6 = 111, stack = {{0}}}},
    gold = {{0}}, generatedRam = {{0}}
}}
local callbacks, callback_order = {{[11] = {{id = 11}}, [12] = {{id = 12}}}}, {{11, 12}}
local finish_pending, pending_failure = true, nil
local _status_lines, _exit_codes, _unregister_calls, _observation_writes = {{}}, {{}}, 0, 0
local _mismatch = {mismatch_literal}
local _unregister_mode = "{unregister_mode}"
local _output_remove_fault = {output_remove_literal}
local config = {{
    outputPath = "{output_path}",
    statusPath = "{status_path}",
    r1 = {{
        harness = {{checkpointAddress = 100, romPatchDomain = "M68K BUS"}},
        sessionPatches = {{}}
    }},
    ram = {{GAME_FLAGS = 1, COMBATANT_DATA = 10, CURRENT_MAP = 20, ENTITY_DATA = 30,
        TARGETS_LIST_LENGTH = 40, FOLLOWERS_LIST = 50, CUTSCENE_DIALOG_INDEX = 60,
        PLAYER_1_INPUT = 70, VIEW_TARGET_ENTITY = 80, CURRENT_GOLD = 90}},
    observerFailureContract = {{exitCode = 79}}
}}
local _values = {{}}
local function reset_values()
    for address = 1, 160 do _values[address] = 0 end
    _values[config.ram.GAME_FLAGS] = 1
end
reset_values()
local function reg(_) return 0x4712C end
local function restore_span(address, expected)
    for offset, value in ipairs(expected) do _values[address + offset - 1] = value end
end
local function first_mismatch(domain, address, expected)
    for offset, value in ipairs(expected) do
        local actual = _values[address + offset - 1] or 0
        if actual ~= value then
            return {{
                domain = domain, address = address + offset - 1, expected = value, actual = actual
            }}
        end
    end
    return nil
end
local function json_escape(value) return tostring(value) end
local function restore_cart(_) return true, nil end
local function status(value) _status_lines[#_status_lines + 1] = value end
memory = {{
    read_u8 = function(address, _) return _values[address] or 0 end,
    write_u8 = function(address, value, _) _values[address] = value end
}}
memorysavestate = {{
    loadcorestate = function(_)
        reset_values()
        if _mismatch then _values[config.ram.GAME_FLAGS] = 2 end
    end
}}
emu = {{setregister = function(_, _) end}}
event = {{
    unregisterbyid = function(_)
        _unregister_calls = _unregister_calls + 1
        if _unregister_mode == "persistent"
            or (_unregister_mode == "once" and _unregister_calls == 1) then
            error("injected unregister failure")
        end
    end
}}
client = {{exitCode = function(code) _exit_codes[#_exit_codes + 1] = code end}}
local function write_observation(_)
    _observation_writes = _observation_writes + 1
    error("faulted finalization wrote an accepted observation")
end
local prior = assert(io.open(config.outputPath, "w"))
prior:write("preexisting accepted output must be removed")
prior:close()
local _original_remove = os.remove
if _output_remove_fault then
    os.remove = function(_) return nil, "injected output removal failure" end
end
{helpers}
{finalizer}
finalize_success()
assert(pending_failure ~= nil, "faulted success finalization did not queue typed failure")
assert(#_exit_codes == 0 and _observation_writes == 0 and #_status_lines == 0,
    "faulted success finalization emitted PASS evidence")
finalize_failure()
assert(#_exit_codes == 1 and _exit_codes[1] == 79, "typed failure exit drift")
assert(_observation_writes == 0, "fault emitted accepted observation")
local output = io.open(config.outputPath, "r")
local output_exists = output ~= nil
if output then output:close() end
assert(
    output_exists == _output_remove_fault,
    "output-removal fact did not match injected removal fault"
)
local status_file = assert(io.open(config.statusPath, "r"))
local status_text = status_file:read("*a")
status_file:close()
local _, failure_count = string.gsub(status_text, "failure:observer%-callback:", "")
assert(failure_count == 1, "fault emitted duplicate terminal failure")
assert(not string.find(status_text, "observer%-finished"), "fault emitted observer-finished")
assert(not string.find(status_text, "callbacks%-cleared:0"), "fault emitted PASS cleanup milestone")
local result = assert(io.open("{result_path}", "w"))
result:write('{{"unregisterCalls":' .. _unregister_calls .. ',"statusCount":' .. #_status_lines
    .. ',"failureCount":' .. failure_count .. ',"callbacksRemaining":' .. #callback_order
    .. ',"observationWrites":' .. _observation_writes
    .. ',"outputExists":' .. tostring(output_exists)
    .. ',"exitCode":' .. _exit_codes[1] .. '}}')
result:close()
if _output_remove_fault then
    os.remove = _original_remove
    assert(os.remove(config.outputPath), "failed to clean injected output-removal residue")
end
'''

    _contract, executable = bizhawk_contract()
    library = ctypes.CDLL(str(executable.parent / "dll" / "lua54.dll"))
    library.luaL_newstate.argtypes = []
    library.luaL_newstate.restype = ctypes.c_void_p
    library.luaL_openlibs.argtypes = [ctypes.c_void_p]
    library.luaL_openlibs.restype = None
    library.luaL_loadbufferx.argtypes = [
        ctypes.c_void_p,
        ctypes.c_char_p,
        ctypes.c_size_t,
        ctypes.c_char_p,
        ctypes.c_char_p,
    ]
    library.luaL_loadbufferx.restype = ctypes.c_int
    library.lua_pcallk.argtypes = [
        ctypes.c_void_p,
        ctypes.c_int,
        ctypes.c_int,
        ctypes.c_int,
        ctypes.c_longlong,
        ctypes.c_void_p,
    ]
    library.lua_pcallk.restype = ctypes.c_int
    library.lua_tolstring.argtypes = [
        ctypes.c_void_p,
        ctypes.c_int,
        ctypes.POINTER(ctypes.c_size_t),
    ]
    library.lua_tolstring.restype = ctypes.c_void_p
    library.lua_close.argtypes = [ctypes.c_void_p]
    library.lua_close.restype = None

    state = library.luaL_newstate()
    assert state
    try:
        library.luaL_openlibs(state)
        encoded = harness.encode("utf-8")
        assert (
            library.luaL_loadbufferx(
                state,
                encoded,
                len(encoded),
                b"@map3-messenger-finalization-harness",
                b"t",
            )
            == 0
        )
        result = library.lua_pcallk(state, 0, 0, 0, 0, None)
        if result:
            length = ctypes.c_size_t()
            pointer = library.lua_tolstring(state, -1, ctypes.byref(length))
            message = (
                ctypes.string_at(pointer, length.value).decode("utf-8", errors="replace")
                if pointer
                else f"Lua status {result}"
            )
            raise AssertionError(f"Lua finalization harness failed: {message}")
    finally:
        library.lua_close(state)
    payload = json.loads((tmp_path / "lua-finalization-result.json").read_text(encoding="utf-8"))
    payload["status"] = (tmp_path / "lua-finalization-status.txt").read_text(encoding="utf-8")
    return payload


def test_static_contract_is_source_h1_rom_bound() -> None:
    fixture = load_json(rail.FIXTURE)
    validate_json(fixture, rail.FIXTURE_SCHEMA, owner=rail.OWNER)
    contract = _contract()

    rail._assert_fixture(fixture, contract)

    assert len(contract["stream"]) == 116
    assert contract["functions"]["ExecuteMapScript"] == 0x4712C
    assert contract["functions"]["cs_5149A"] == 0x5149A
    assert contract["functions"]["Map3_ZoneEvent8"] == 0x50ED2
    assert contract["ranges"]["messengerScript"] == {
        "address": 0x5149A,
        "length": 440,
        "sha256": "01C2ACC81830937BDD6510F88F9FA4E4BF67D6E8F1E49A6693BAEF19B88068AA",
    }
    assert contract["ranges"]["zoneEvent8"] == {
        "address": 0x50ED2,
        "length": 24,
        "sha256": "06B77DD8318014989C0E38C9E0922A9D8C5A1C7E344383091CAFEA1B693DA07F",
    }
    assert contract["text"]["ids"] == [*range(517, 532), 535, 536, 447]
    assert contract["text"]["speakers"] == [
        142,
        142,
        142,
        143,
        143,
        142,
        143,
        142,
        142,
        2,
        2,
        0xC001,
        2,
        0xC001,
        0xC001,
        1,
        2,
        None,
    ]


def test_text_operands_preserve_source_modifier_and_entity_word() -> None:
    expected = rail._expected_observation(_contract())
    observed = deepcopy(expected)
    observed["records"][0]["speakerOperands"][11] = 1

    assert "speakerOperands[11]" in rail._difference(expected, observed)
    assert expected["records"][0]["speakerOperands"][11] == 0xC001


def test_parser_rejects_prompt_polarity_and_speaker_near_misses() -> None:
    stream = deepcopy(_contract()["stream"])
    prompt = next(item for item in stream if item["opcode"] == "jumpifflagset")
    prompt["operand"] = "89,cs_51650"
    with pytest.raises(ValueError, match="prompt polarity"):
        rail._accepted_path(stream)

    stream = deepcopy(_contract()["stream"])
    speaker = next(
        item
        for item in stream
        if item["opcode"] in {"nexttext", "nextsingletext"} and item["operand"] == "$c0,ally_sarah"
    )
    speaker["operand"] = "$0,ally_sarah"
    with pytest.raises(ValueError, match="portrait-modifier"):
        rail._text_contract(stream)


def test_source_h1_and_rom_mutation_guards_are_independent(
    tmp_path: Path, monkeypatch: pytest.MonkeyPatch
) -> None:
    original_stream = rail._stream

    def wrong_source(*args: Any, **kwargs: Any) -> list[dict[str, Any]]:
        stream = original_stream(*args, **kwargs)
        next(item for item in stream if item["opcode"] == "setf" and item["operand"] == "600")[
            "operand"
        ] = "601"
        return stream

    monkeypatch.setattr(rail, "_stream", wrong_source)
    with pytest.raises(ValueError, match="accept stream"):
        rail.build_map3_messenger_acceptance_source_contract(_rom(), _upstream())
    monkeypatch.undo()

    monkeypatch.setattr(rail, "_h1_bytes", lambda *_: "0000")
    with pytest.raises(ValueError, match="H1/ROM entry"):
        rail.build_map3_messenger_acceptance_source_contract(_rom(), _upstream())
    monkeypatch.undo()

    mutated_rom = tmp_path / "mutated.bin"
    shutil.copy2(_rom(), mutated_rom)
    with mutated_rom.open("r+b") as handle:
        handle.seek(0)
        original = handle.read(1)
        handle.seek(0)
        handle.write(bytes([original[0] ^ 0xFF]))
    with pytest.raises(ValueError, match="canonical ROM"):
        rail.build_map3_messenger_acceptance_source_contract(mutated_rom, _upstream())


def test_fixture_and_retained_projection_drift_stop_before_launch(
    tmp_path: Path, monkeypatch: pytest.MonkeyPatch
) -> None:
    invalid = deepcopy(load_json(rail.FIXTURE))
    invalid["cases"][0]["frameBudget"] += 1
    fixture_path = tmp_path / "invalid-fixture.json"
    fixture_path.write_text(json.dumps(invalid), encoding="utf-8")
    monkeypatch.setattr(rail, "FIXTURE", fixture_path)
    monkeypatch.setattr(rail, "run_observer", _unexpected_launch)
    with pytest.raises(ValueError, match="fixture failed schema validation"):
        rail.verify_map3_messenger_acceptance(_rom(), _upstream())

    monkeypatch.undo()
    calls = 0
    retained = rail._retained_projection

    def drifting_projection() -> dict[str, Any]:
        nonlocal calls
        calls += 1
        result = deepcopy(retained())
        if calls > 1:
            result["projectionSha256"] = "0" * 64
        return result

    monkeypatch.setattr(rail, "_retained_projection", drifting_projection)
    monkeypatch.setattr(rail, "run_observer", _unexpected_launch)
    with pytest.raises(ValueError, match="golden boundary"):
        rail.verify_map3_messenger_acceptance(_rom(), _upstream())
    assert calls >= 2


def _assert_observer_callback_owners(source: str) -> None:
    assert "local extension_enabled = config.extension ~= nil" in source
    assert (
        'local OWNER = extension_enabled and config.extension.owner or "map3-messenger-acceptance"'
        in source
    )
    # These are the two top-level opt-in registration blocks in the shared observer.
    # Match their complete boundaries; a moved or unguarded callback must fail.
    guard = r"^if extension_enabled then\n.*?^end$"
    blocks = re.findall(guard, source, re.MULTILINE | re.DOTALL)
    assert len(blocks) == 2
    callback = r'add_callback\(([^,\n]+),\s*"([^"]+)"'
    registrations = re.findall(callback, source)
    assert len(re.findall(r"\badd_callback\(", source)) == len(registrations) + 1
    extension_callbacks = re.findall(callback, "\n".join(blocks))
    expected_extension = {
        "config.functions.ProcessMapEvent": "extension-map-event-dispatch",
        "config.extension.functions.CheckBattle": "extension-check-battle",
        "config.extension.functions.BattleLoop": "extension-battle-loop",
        "config.extension.functions.ExecuteBeforeBattleCutscene": "extension-before-battle",
        "config.extension.functions.csc15_setEntityActscript": "extension-before-set-actscript",
        "config.extension.functions.csc2A_entityShiver": "extension-before-entity-shiver",
        "config.extension.functions.csc2D_entityActionSequence": "extension-before-entity-actions",
        "config.extension.functions.LoadBattle": "extension-load-battle",
        "config.extension.functions.ExecuteBattleStartCutscene": "extension-battle-start",
        "config.extension.functions.ActivateEnemies": "extension-activate-enemies",
        "config.extension.functions.ExecuteBattleRegionCutscene": "extension-region-cutscene",
        "config.extension.functions.PopulateTargetsListWithSpawningEnemies": "extension-spawn-list",
        "config.extension.functions.GenerateBattleTurnOrder": "extension-turn-order",
        "config.extension.functions.ExecuteIndividualTurn": "extension-individual-turn",
        "config.extension.functions.ProcessBattleEntityControlPlayerInput": (
            "extension-player-control"
        ),
        "config.extension.functions.ControlBattleEntity": "extension-control-battle-entity",
        "config.extension.functions.playerReadyPc": "extension-player-ready",
    }
    assert len(extension_callbacks) == len(expected_extension) == 17
    assert dict(extension_callbacks) == expected_extension
    base_source = re.sub(guard, "", source, flags=re.MULTILINE | re.DOTALL)
    callback_roles = {role for _, role in re.findall(callback, base_source)}
    failure_schema = load_json(rail.FAILURE_SCHEMA)
    schema_roles = set(failure_schema["properties"]["role"]["enum"])
    assert callback_roles <= schema_roles
    assert callback_roles >= rail.REQUIRED_LUA_ROLES

    phase_lines = [line for line in source.splitlines() if "phase" in line]
    produced_phases = {
        phase
        for line in phase_lines
        for phase in re.findall(r'"([a-z][a-z0-9-]+)"', line)
        if phase.startswith("await-") or phase in {"route", "messenger", "follower-ready"}
    }
    schema_phases = set(failure_schema["properties"]["phase"]["enum"])
    assert produced_phases == schema_phases

    extension_phases = {
        "extension-bridge-requested",
        "extension-bridge-injected",
        "extension-bridge-loading",
        "extension-route",
        "extension-battle",
        "extension-before",
        "extension-load",
        "extension-start",
        "extension-turns",
        "extension-await-player-ready",
    }
    extension_roles = set(expected_extension.values()) | {"extension-phase-watchdog"}
    assert set(re.findall(r'"(extension-[a-z0-9-]+)"', source)) == (
        extension_roles | extension_phases
    )
    extension_schema = load_json(player_ready.FAILURE_SCHEMA)
    assert extension_schema["properties"]["owner"]["const"] == player_ready.OWNER
    # The accepted extension schema permits nonempty strings. Exact closure above
    # belongs to this test, not to a nonexistent enum in that schema.
    for role in extension_roles:
        Draft202012Validator(extension_schema["properties"]["role"]).validate(role)
    for phase in extension_phases:
        Draft202012Validator(extension_schema["properties"]["phase"]).validate(phase)


def test_observer_config_has_no_accepted_output_and_closed_roles_and_phases() -> None:
    fixture = load_json(rail.FIXTURE)
    config = rail._observer_config(fixture, _contract())
    rail._assert_clean_config(config)
    rail._assert_lua_roles()
    assert "extension" not in config
    extension_fixture = load_json(player_ready.FIXTURE)
    extension_config = player_ready._observer_config(extension_fixture, extension_fixture["static"])
    assert extension_config["extension"]["owner"] == player_ready.OWNER
    assert player_ready.OBSERVER == rail.OBSERVER
    assert "expectedObservation" not in json.dumps([config, extension_config], sort_keys=True)
    _assert_observer_callback_owners(rail.OBSERVER.read_text(encoding="utf-8"))


@pytest.mark.parametrize("mutation", ["outside-guard", "unknown-role"])
def test_shared_observer_callback_ownership_rejects_near_misses(mutation: str) -> None:
    source = rail.OBSERVER.read_text(encoding="utf-8")
    if mutation == "outside-guard":
        source = source.replace(
            "if extension_enabled then\n    add_callback", "do\n    add_callback", 1
        )
    else:
        source = source.replace('"extension-check-battle"', '"extension-unknown"', 1)
    with pytest.raises(AssertionError):
        _assert_observer_callback_owners(source)


def test_typed_callback_failure_reports_consistent_cleanup_facts(
    tmp_path: Path, monkeypatch: pytest.MonkeyPatch
) -> None:
    payload = _failure_payload()
    validate_json(payload, rail.FAILURE_SCHEMA, owner=rail.OWNER)
    status_path = tmp_path / "failure.status.txt"
    status_path.write_text(
        "failure:observer-callback:" + json.dumps(payload, sort_keys=True) + "\n",
        encoding="utf-8",
    )
    monkeypatch.setattr(rail, "STATUS_PATH", status_path)

    diagnostic = rail._failure_diagnostic()

    assert diagnostic == payload
    assert payload["callbackCount"] == 0
    assert payload["callbacksCleared"] and payload["outputRemoved"]

    persistent = deepcopy(payload)
    persistent["callbackCount"] = 2
    persistent["callbacksCleared"] = False
    persistent["restoration"]["callbacksCleared"] = False
    status_path.write_text(
        "failure:observer-callback:" + json.dumps(persistent, sort_keys=True) + "\n",
        encoding="utf-8",
    )
    assert rail._failure_diagnostic() == persistent


def test_typed_callback_failure_rejects_inconsistent_cleanup_or_restoration_facts(
    tmp_path: Path, monkeypatch: pytest.MonkeyPatch
) -> None:
    status_path = tmp_path / "failure.status.txt"
    monkeypatch.setattr(rail, "STATUS_PATH", status_path)

    def assert_rejected(payload: dict[str, Any], message: str) -> None:
        validate_json(payload, rail.FAILURE_SCHEMA, owner=rail.OWNER)
        status_path.write_text(
            "failure:observer-callback:" + json.dumps(payload, sort_keys=True) + "\n",
            encoding="utf-8",
        )
        with pytest.raises(ValueError, match=message):
            rail._failure_diagnostic()

    payload = _failure_payload()
    payload["restoration"]["callbacksCleared"] = False
    assert_rejected(payload, "restoration cleanup facts")

    payload = _failure_payload()
    payload["restoration"]["outputRemoved"] = False
    assert_rejected(payload, "restoration cleanup facts")

    payload = _failure_payload()
    payload["callbacksCleared"] = False
    payload["restoration"]["callbacksCleared"] = False
    assert_rejected(payload, "callback count consistency")

    payload = _failure_payload()
    payload["restoration"]["sessionStateRestored"] = False
    assert_rejected(payload, "restoration mismatch consistency")


@pytest.mark.parametrize(
    (
        "restoration_mismatch",
        "unregister_mode",
        "output_remove_fault",
        "expected_unregister_calls",
        "callback_count",
        "callbacks_cleared",
        "output_removed",
        "role",
    ),
    (
        (True, "none", False, 2, 0, True, True, "restoration"),
        (False, "once", False, 3, 0, True, True, "callback-cleanup"),
        (False, "persistent", False, 4, 2, False, True, "callback-cleanup"),
        (True, "none", True, 2, 0, True, False, "restoration"),
    ),
)
def test_lua_success_finalization_faults_report_one_typed_failure_without_pass(
    tmp_path: Path,
    monkeypatch: pytest.MonkeyPatch,
    restoration_mismatch: bool,
    unregister_mode: str,
    output_remove_fault: bool,
    expected_unregister_calls: int,
    callback_count: int,
    callbacks_cleared: bool,
    output_removed: bool,
    role: str,
) -> None:
    result = _run_lua_success_finalization_fault(
        tmp_path,
        restoration_mismatch=restoration_mismatch,
        unregister_mode=unregister_mode,
        output_remove_fault=output_remove_fault,
    )
    assert {key: value for key, value in result.items() if key != "status"} == {
        "unregisterCalls": expected_unregister_calls,
        "statusCount": 0,
        "failureCount": 1,
        "callbacksRemaining": callback_count,
        "observationWrites": 0,
        "outputExists": not output_removed,
        "exitCode": 79,
    }
    failure = json.loads(result["status"].removeprefix("failure:observer-callback:"))
    validate_json(failure, rail.FAILURE_SCHEMA, owner=rail.OWNER)
    assert failure["role"] == role
    assert failure["callbackCount"] == callback_count
    assert failure["callbacksCleared"] is callbacks_cleared
    assert failure["outputRemoved"] is output_removed
    assert failure["restoration"]["callbacksCleared"] is callbacks_cleared
    assert failure["restoration"]["outputRemoved"] is output_removed
    status_path = tmp_path / "typed-failure-status.txt"
    status_path.write_text(result["status"], encoding="utf-8")
    monkeypatch.setattr(rail, "STATUS_PATH", status_path)
    assert rail._failure_diagnostic() == failure
    if restoration_mismatch:
        assert failure["restoration"]["sessionStateRestored"] is False
        assert failure["restorationMismatch"] == {
            "domain": "gameFlags",
            "address": 1,
            "expected": 1,
            "actual": 2,
        }
        assert failure["error"] == "scoped restoration mismatch: gameFlags"
    else:
        assert failure["restoration"]["sessionStateRestored"] is True
        assert failure["restorationMismatch"] is None
        assert failure["error"].startswith("callback cleanup failed:")
        assert "injected unregister failure" in failure["error"]


def test_observation_and_failure_schemas_are_closed() -> None:
    fixture = load_json(rail.FIXTURE)
    expected = fixture["expectedObservation"]

    validate_json(expected, rail.OBSERVATION_SCHEMA, owner=rail.OWNER)
    invalid = deepcopy(expected)
    invalid["extra"] = True
    with pytest.raises(ValueError, match="Additional properties"):
        validate_json(invalid, rail.OBSERVATION_SCHEMA, owner=rail.OWNER)

    invalid_failure = _failure_payload()
    invalid_failure["callbackCount"] = -1
    with pytest.raises(ValueError, match="minimum"):
        validate_json(invalid_failure, rail.FAILURE_SCHEMA, owner=rail.OWNER)
