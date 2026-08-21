"""Static and adversarial tests for the controlled Map 3 admission H3 rail."""

from __future__ import annotations

import ctypes
import json
from copy import deepcopy
from pathlib import Path
from typing import Any

import pytest

from sf2tool.h3 import map3_admitted_start as rail
from sf2tool.h3.bizhawk import bizhawk_contract
from sf2tool.jsonio import load_json, validate_json


def _rom() -> Path:
    return rail.repo_path("local/roms/sf2-us.bin")


def _upstream() -> Path:
    return rail.repo_path("local/upstream/SF2DISASM")


def _contract() -> dict[str, Any]:
    return rail.build_map3_admitted_start_source_contract(_rom(), _upstream())


def _failure_payload() -> dict[str, Any]:
    return {
        "owner": rail.OWNER,
        "caseId": "controlled-new-map3-default",
        "phase": "await-main-loop",
        "role": "main-loop",
        "actualPc": 0x75C4,
        "expectedPc": 0x75C4,
        "callbackCount": 0,
        "callbacksCleared": True,
        "outputRemoved": True,
        "restoration": {
            "scopeArmed": True,
            "gameFlags": True,
            "combatantAllyRecords": True,
            "mapAndBattleState": True,
            "playerEntity": True,
            "gold": True,
            "timeState": True,
            "generatedRam": True,
            "sessionCartPatches": True,
            "sessionStateRestored": True,
            "callbacksCleared": True,
            "outputRemoved": True,
        },
        "restorationMismatch": None,
        "error": "forced callback failure",
    }


def _write_failure_status(path: Path, payload: dict[str, Any]) -> None:
    path.write_text(
        "failure:observer-callback:" + json.dumps(payload, sort_keys=True) + "\n",
        encoding="utf-8",
    )


def _run_lua_success_finalization_fault(
    tmp_path: Path, *, restoration_mismatch: bool, unregister_fault: bool
) -> dict[str, Any]:
    """Execute the observer's exact deferred-finalization helpers in Lua.

    The test harness deliberately extracts the production helper range from the
    tracked observer instead of duplicating it in Python.  It supplies only the
    BizHawk APIs touched by the deferred success/failure paths, then injects one
    first-mismatch or one-shot ``event.unregisterbyid`` fault.  Both faults must
    finish through the production typed failure finalizer without an observation
    or success milestones.
    """
    source = rail.OBSERVER.read_text(encoding="utf-8")
    start = source.index("local function cleanup_callbacks()")
    end = source.index("local function add_callback(")
    helpers = source[start:end]
    result_path = (tmp_path / "lua-finalization-result.json").as_posix()
    status_path = (tmp_path / "lua-finalization-status.txt").as_posix()
    output_path = (tmp_path / "accepted-observation.json").as_posix()
    mismatch_literal = "true" if restoration_mismatch else "false"
    unregister_literal = "true" if unregister_fault else "false"
    harness = f'''\
local phase = "await-wait-for-event"
local active = {{caseId = "controlled-new-map3-default", rawVintTime = {{}}, scenarioState = {{}}}}
local saved_state = {{}}
local scope = {{
    gameFlags = {{1}}, combatantAllyRecords = {{1}}, mapAndBattleState = {{1}},
    playerEntity = {{1}}, gold = {{1}},
    timeState = {{
        frameCounter = {{1}}, randomSeed = {{1}}, secondsCounter = {{1}},
        secondsCounterFrames = {{1}}
    }},
    generatedRam = {{1}}
}}
local callbacks, callback_order = {{[11] = {{id = 11}}, [12] = {{id = 12}}}}, {{11, 12}}
local finish_pending, failed = true, false
local pending_core_snapshot, pending_failure = false, nil
local write_observation
local _status_lines, _exit_codes, _unregister_calls, _loadcorestate_calls = {{}}, {{}}, 0, 0
local _mismatch, _unregister_fault = {mismatch_literal}, {unregister_literal}
local config = {{
    outputPath = "{output_path}",
    harness = {{checkpointAddress = 9}},
    sessionPatches = {{}},
    ram = {{GAME_FLAGS = 1, COMBATANT_DATA = 2, ENTITY_DATA = 3, CURRENT_GOLD = 4,
        CURRENT_MAP = 5, FRAME_COUNTER = 6, RANDOM_SEED = 7, SECONDS_COUNTER = 8,
        SECONDS_COUNTER_FRAMES = 10}},
    observerFailureContract = {{statusPrefix = "failure:observer-callback:", exitCode = 79}}
}}
local _values = {{}}
for address = 1, 10 do _values[address] = 1 end
local function json_escape(value) return tostring(value) end
local function status(value) _status_lines[#_status_lines + 1] = value end
local function register(_) return 0x20A02 end
local function restore_span(address, expected)
    for offset, value in ipairs(expected) do _values[address + offset - 1] = value end
end
memory = {{
    read_u8 = function(address, _) return _values[address] or 0 end,
    write_u8 = function(address, value, _) _values[address] = value end,
    read_u32_be = function(_, _) return 0 end,
    write_u32_be = function(address, value, _)
        _values[address] = value == 0 and 0 or value
    end
}}
memorysavestate = {{
    loadcorestate = function(_)
        _loadcorestate_calls = _loadcorestate_calls + 1
        for address = 1, 10 do _values[address] = 1 end
        if _mismatch and _loadcorestate_calls == 1 then
            _values[config.ram.GAME_FLAGS] = 2
        end
    end
}}
event = {{
    unregisterbyid = function(_)
        _unregister_calls = _unregister_calls + 1
        if _unregister_fault and _unregister_calls == 1 then
            error("injected unregister failure")
        end
    end
}}
client = {{exitCode = function(code) _exit_codes[#_exit_codes + 1] = code end}}
{helpers}
local completed = finalize_success()
assert(not completed, "faulted success finalization emitted PASS")
assert(
    pending_failure ~= nil and failed,
    "faulted success finalization did not queue typed failure"
)
finalize_failure()
assert(#_exit_codes == 1 and _exit_codes[1] == 79, "typed failure exit drift")
assert(
    #callback_order == 0 and next(callbacks) == nil,
    "residual callback after failure finalization"
)
local _failure_count = 0
for _, line in ipairs(_status_lines) do
    if string.find(line, "failure:observer-callback:", 1, true) then
        _failure_count = _failure_count + 1
    end
    assert(
        not string.find(line, "observer-finished", 1, true),
        "fault emitted success milestone"
    )
end
assert(_failure_count == 1, "fault emitted duplicate terminal failure")
assert(not io.open(config.outputPath, "r"), "fault emitted accepted observation")
local result = assert(io.open("{result_path}", "w"))
result:write('{{"unregisterCalls":' .. _unregister_calls .. ',"statusCount":' .. #_status_lines
    .. ',"failureCount":' .. _failure_count
    .. ',"loadCoreStateCalls":' .. _loadcorestate_calls
    .. ',"callbacksRemaining":' .. #callback_order .. '}}')
result:close()
local status_output = assert(io.open("{status_path}", "w"))
status_output:write(_status_lines[#_status_lines])
status_output:close()
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
                b"@map3-admitted-start-finalization-harness",
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


def test_fixture_static_projection_is_source_h1_rom_bound_and_map3_corpus_is_exact() -> None:
    fixture = load_json(rail.FIXTURE)
    validate_json(fixture, rail.FIXTURE_SCHEMA, owner=rail.OWNER)
    contract = _contract()
    rail._assert_fixture(fixture, contract)
    rail._validate_expected_matrix(fixture, contract)
    rail._assert_index_bindings()
    assert len(contract["map3SourceRecords"]) == 26
    assert [row["flag"] for row in contract["map3"]["flagVariants"]] == [609, 506, 543]
    assert contract["function"]["setupResolutionReturnAddress"] == 0x47504
    assert contract["function"]["initCallAddress"] == 0x47512
    assert contract["function"]["initReturnAddress"] == 0x47514


def test_structural_schemas_reject_nested_extra_properties() -> None:
    fixture = load_json(rail.FIXTURE)
    extra = deepcopy(fixture)
    extra["cases"][0]["extra"] = True
    with pytest.raises(ValueError, match="case"):
        validate_json(extra, rail.FIXTURE_SCHEMA, owner=rail.OWNER)

    extra = deepcopy(fixture)
    extra["static"]["function"]["extra"] = 1
    with pytest.raises(ValueError, match="function"):
        validate_json(extra, rail.FIXTURE_SCHEMA, owner=rail.OWNER)

    extra = deepcopy(fixture)
    extra["sourceContext"]["function"]["extra"] = 1
    with pytest.raises(ValueError, match="function"):
        validate_json(extra, rail.FIXTURE_SCHEMA, owner=rail.OWNER)

    observation = deepcopy(fixture["expectedObservation"])
    observation["records"][0]["selectedSetup"]["extra"] = True
    with pytest.raises(ValueError, match="selectedSetup"):
        validate_json(observation, rail.OBSERVATION_SCHEMA, owner=rail.OWNER)

    observation = deepcopy(fixture["expectedObservation"])
    observation["records"][0]["scenarioState"]["vintTime"]["extra"] = True
    with pytest.raises(ValueError, match="vintTime"):
        validate_json(observation, rail.OBSERVATION_SCHEMA, owner=rail.OWNER)


@pytest.mark.parametrize(
    ("before", "after"),
    (
        ("bsr.w   SwitchMap", "bsr.w   SwitchMapDrift"),
        ("jsr     j_RunMapSetupInitFunction", "jsr     j_RunMapSetupInitFunctionDrift"),
        ("bsr.w   WaitForEvent", "bsr.w   WaitForEventDrift"),
        ("moveq   #GAMESTART_GOLD,d1", "moveq   #GAMESTART_GOLD+1,d1"),
    ),
)
def test_source_use_site_guards_reject_operand_or_order_drift(before: str, after: str) -> None:
    disasm = _upstream() / "disasm"
    if "SwitchMap" in before:
        with pytest.raises(ValueError, match="MainLoop"):
            rail._main_loop_use_sites(
                (disasm / rail.MAIN_LOOP_SOURCE)
                .read_text(encoding="utf-8")
                .replace(before, after, 1)
            )
    elif "RunMapSetup" in before or "WaitForEvent" in before:
        with pytest.raises(ValueError, match="ExplorationLoop"):
            rail._exploration_use_sites(
                (disasm / rail.EXPLORATION_SOURCE)
                .read_text(encoding="utf-8")
                .replace(before, after, 1)
            )
    else:
        with pytest.raises(ValueError, match="NewGame"):
            rail._new_game_use_sites(
                (disasm / rail.NEW_GAME_SOURCE)
                .read_text(encoding="utf-8")
                .replace(before, after, 1)
            )

    map3_init = disasm / "data/maps/entries/map03/mapsetups/s6_initfunction.asm"
    with pytest.raises(ValueError, match="Map 3 selected init"):
        rail._map3_init_use_sites(
            map3_init.read_text(encoding="utf-8").replace(
                "jsr     MoveEntityOutOfMap", "jsr     MoveEntityOutOfMapDrift", 1
            )
        )


def test_rom_derived_seams_reject_target_and_indirect_call_drift() -> None:
    contract = _contract()
    rom = bytearray(_rom().read_bytes())
    entry = contract["function"]["runMapSetupInitFunctionAddress"]
    rom[contract["function"]["setupResolutionReturnAddress"] - 2] ^= 1
    with pytest.raises(ValueError, match="BSR target"):
        rail._relative_bsr_return(
            bytes(rom),
            entry=entry,
            target=0x4779E,
            scan_bytes=24,
        )
    rom = bytearray(_rom().read_bytes())
    rom[contract["function"]["initCallAddress"]] ^= 1
    with pytest.raises(ValueError, match="indirect init call"):
        rail._indirect_jsr_seams(bytes(rom), entry=entry)
    rom = bytearray(_rom().read_bytes())
    call = contract["function"]["unexpectedInitEffectCallAddress"]
    rom[call + 5] ^= 1
    with pytest.raises(ValueError, match="absolute JSR target"):
        rail._absolute_jsr_target(
            bytes(rom),
            entry=contract["function"]["selectedInitAddress"],
            target=contract["function"]["unexpectedInitEffectAddress"],
            scan_bytes=64,
        )


def test_schema_valid_case_matrix_drift_rejects_before_launch(
    tmp_path: Path, monkeypatch: pytest.MonkeyPatch
) -> None:
    fixture = load_json(rail.FIXTURE)
    fixture["cases"][0]["injectedDifficultyMenuReturn"] = 1
    path = tmp_path / "matrix.json"
    path.write_text(json.dumps(fixture), encoding="utf-8")
    launched = False

    def unexpected_launch(**_: Any) -> dict[str, Any]:
        nonlocal launched
        launched = True
        raise AssertionError("observer must not launch")

    monkeypatch.setattr(rail, "FIXTURE", path)
    monkeypatch.setattr(rail, "run_observer", unexpected_launch)
    with pytest.raises(ValueError, match="input matrix"):
        rail.verify_map3_admitted_start(_rom(), _upstream())
    assert not launched


def test_source_context_and_deferred_callback_cleanup_are_guarded() -> None:
    fixture = load_json(rail.FIXTURE)
    fixture["sourceContext"]["function"]["mainLoopAddress"] += 2
    with pytest.raises(ValueError, match="index source context"):
        rail._assert_fixture(fixture, _contract())

    observer = rail.OBSERVER.read_text(encoding="utf-8")
    for required in (
        "pending_core_snapshot",
        "core-state-saved-outside-callback",
        "pending_failure",
        "finalize_failure",
        "restore_span(config.harness.checkpointAddress, scope.generatedRam)",
    ):
        assert required in observer


@pytest.mark.parametrize(
    (
        "restoration_mismatch",
        "unregister_fault",
        "expected_unregister_calls",
        "expects_mismatch",
    ),
    (
        (True, False, 2, True),
        (False, True, 3, False),
    ),
)
def test_lua_success_finalization_faults_use_one_typed_failure_without_pass_observation(
    tmp_path: Path,
    monkeypatch: pytest.MonkeyPatch,
    restoration_mismatch: bool,
    unregister_fault: bool,
    expected_unregister_calls: int,
    expects_mismatch: bool,
) -> None:
    result = _run_lua_success_finalization_fault(
        tmp_path,
        restoration_mismatch=restoration_mismatch,
        unregister_fault=unregister_fault,
    )
    assert {key: value for key, value in result.items() if key != "status"} == {
        "unregisterCalls": expected_unregister_calls,
        "statusCount": 2,
        "failureCount": 1,
        "loadCoreStateCalls": 1,
        "callbacksRemaining": 0,
    }
    if expects_mismatch:
        assert '"sessionStateRestored":false' in result["status"]
        assert (
            '"restorationMismatch":{"domain":"gameFlags","address":1,"expected":1,"actual":2}'
            in result["status"]
        )
    else:
        assert '"restorationMismatch":null' in result["status"]
    failure_status = tmp_path / "typed-failure-status.txt"
    failure_status.write_text(result["status"] + "\n", encoding="utf-8")
    monkeypatch.setattr(rail, "STATUS_PATH", failure_status)
    diagnostic = rail._failure_diagnostic()
    assert diagnostic is not None
    if expects_mismatch:
        assert diagnostic["restoration"]["sessionStateRestored"] is False
        assert diagnostic["restorationMismatch"] == {
            "domain": "gameFlags",
            "address": 1,
            "expected": 1,
            "actual": 2,
        }
    else:
        assert diagnostic["restoration"]["sessionStateRestored"] is True
        assert diagnostic["restorationMismatch"] is None


def test_observer_config_omits_accepted_output_and_lua_is_preflight_valid() -> None:
    fixture = load_json(rail.FIXTURE)
    config = rail._observer_config(fixture, _contract())
    rail._assert_clean_observer_config(config)
    serialized = json.dumps(config, sort_keys=True)
    for forbidden in ("expectedObservation", "chronology", "restoration", "map3SourceRecords"):
        assert forbidden not in serialized
    rail._assert_lua_role_contract()


def test_expected_matrix_rejects_schema_valid_chronology_and_setup_drift() -> None:
    fixture = load_json(rail.FIXTURE)
    contract = _contract()
    for mutate, message in (
        (
            lambda value: value["expectedObservation"]["records"][0]["chronology"].reverse(),
            "chronology",
        ),
        (
            lambda value: value["expectedObservation"]["records"][0]["selectedSetup"].update(
                {"initReturnPc": 0x47516}
            ),
            "setup/init",
        ),
        (
            lambda value: value["expectedObservation"]["records"][0].update(
                {"programRequest": "guarded"}
            ),
            "handoff/program",
        ),
    ):
        drift = deepcopy(fixture)
        mutate(drift)
        validate_json(drift, rail.FIXTURE_SCHEMA, owner=rail.OWNER)
        with pytest.raises(ValueError, match=message):
            rail._validate_expected_matrix(drift, contract)


def test_status_requires_the_exact_unique_ordered_success_sequence(
    tmp_path: Path, monkeypatch: pytest.MonkeyPatch
) -> None:
    status_path = tmp_path / "status.txt"
    monkeypatch.setattr(rail, "STATUS_PATH", status_path)
    status_path.write_text("\n".join(rail.SUCCESS_MILESTONES) + "\n", encoding="utf-8")
    rail._assert_status()

    for lines in (
        list(rail.SUCCESS_MILESTONES[:-1]),
        [*rail.SUCCESS_MILESTONES, rail.SUCCESS_MILESTONES[4]],
        [
            *rail.SUCCESS_MILESTONES[:8],
            rail.SUCCESS_MILESTONES[9],
            rail.SUCCESS_MILESTONES[8],
            *rail.SUCCESS_MILESTONES[10:],
        ],
    ):
        status_path.write_text("\n".join(lines) + "\n", encoding="utf-8")
        with pytest.raises(RuntimeError, match="milestone|status"):
            rail._assert_status()


def test_failure_status_is_closed_and_verifier_promotes_cleanup_diagnostics(
    tmp_path: Path, monkeypatch: pytest.MonkeyPatch
) -> None:
    status_path = tmp_path / "status.txt"
    observed_path = tmp_path / "observed.json"
    payload = _failure_payload()
    _write_failure_status(status_path, payload)
    monkeypatch.setattr(rail, "STATUS_PATH", status_path)
    monkeypatch.setattr(rail, "OBSERVED_OUTPUT", observed_path)
    assert rail._failure_diagnostic() == payload

    for mutate in (
        lambda value: value.update({"role": "arbitrary-role"}),
        lambda value: value.update({"phase": "arbitrary-phase"}),
        lambda value: value.update({"callbackCount": 1}),
        lambda value: value.update({"outputRemoved": False}),
        lambda value: value["restoration"].update({"sessionStateRestored": False}),
    ):
        drift = deepcopy(payload)
        mutate(drift)
        _write_failure_status(status_path, drift)
        if (
            drift["role"] == "arbitrary-role"
            or drift["phase"] == "arbitrary-phase"
            or drift["restoration"]["sessionStateRestored"] is False
        ):
            with pytest.raises(ValueError, match="callback failure status"):
                rail._failure_diagnostic()
        else:
            with pytest.raises(ValueError, match="callback|cleanup"):
                rail._failure_diagnostic()

    _write_failure_status(status_path, payload)

    def forced_observer(**_: Any) -> dict[str, Any]:
        raise RuntimeError("forced emulator callback failure")

    monkeypatch.setattr(rail, "run_observer", forced_observer)
    with pytest.raises(RuntimeError, match="forced emulator callback failure"):
        rail.verify_map3_admitted_start(_rom(), _upstream())
    assert not observed_path.exists()


def test_index_bindings_close_primary_contract_and_keep_map3_aggregate_unassociated(
    tmp_path: Path, monkeypatch: pytest.MonkeyPatch
) -> None:
    rail._assert_index_bindings()
    index = load_json(rail.INDEX)
    path = tmp_path / "research-index.json"
    monkeypatch.setattr(rail, "INDEX", path)
    primary_id = "map.data.ms-map3-initfunction"

    for contracts in (
        None,
        ["docs/design/contracts/map-exploration.md"],
        [
            "docs/design/contracts/map3-controlled-admission.md",
            "docs/design/contracts/map-exploration.md",
        ],
    ):
        drift = deepcopy(index)
        primary = next(item for item in drift["records"] if item["id"] == primary_id)
        if contracts is None:
            primary.pop("designContracts")
        else:
            primary["designContracts"] = contracts
        path.write_text(json.dumps(drift), encoding="utf-8")
        with pytest.raises(ValueError, match="primary design contract singleton"):
            rail._assert_index_bindings()

    drift = deepcopy(index)
    aggregate = next(item for item in drift["records"] if item["id"] == "map.data.ms-map3")
    aggregate["evidence"].append(
        {
            "level": "H3",
            "fixture": "tests/fixtures/h3/map3-admitted-start-v1.json",
            "fixtureId": "sf2-map3-admitted-start-runtime-v1",
            "verifier": "src/sf2tool/h3/map3_admitted_start.py",
            "bindings": [
                {
                    "addressId": "entry",
                    "fixtureField": "sourceContext.map3.selectedInitAddress",
                }
            ],
        }
    )
    path.write_text(json.dumps(drift), encoding="utf-8")
    with pytest.raises(ValueError, match="binding set|bulk-associate"):
        rail._assert_index_bindings()


def test_lua_reads_current_battle_stats_as_bytes_and_guards_selected_init_only() -> None:
    observer = rail.OBSERVER.read_text(encoding="utf-8")
    assert "attack = byte(config.ram.COMBATANT_OFFSET_ATT_CURRENT)" in observer
    assert "defense = byte(config.ram.COMBATANT_OFFSET_DEF_CURRENT)" in observer
    assert "agility = byte(config.ram.COMBATANT_OFFSET_AGI_CURRENT)" in observer
    assert (
        'if phase == "await-init-return" then\n'
        '            error("default admitted Map 3 init requested a guarded script/program")'
    ) in observer
    assert 'error("default admitted Map 3 init requested MoveEntityOutOfMap")' in observer
    fixture = load_json(rail.FIXTURE)
    assert all(
        0 <= ally[field] <= 0xFF
        for ally in fixture["expectedObservation"]["records"][0]["scenarioState"]["allies"]
        for field in ("attack", "defense", "agility")
    )


def test_time_state_widths_are_source_h1_rom_bound_and_normalized_outside_callbacks() -> None:
    contract = _contract()
    time_state = contract["timeState"]
    assert time_state["frameCounter"]["observationWidthBytes"] == 1
    assert time_state["randomSeed"]["observationWidthBytes"] == 4
    assert time_state["secondsCounter"]["observationWidthBytes"] == 4
    assert time_state["secondsCounterFrames"]["observationWidthBytes"] == 1
    assert time_state["normalization"] == rail.VINT_TIME_NORMALIZATION

    disasm = _upstream() / "disasm"
    vint = (disasm / rail.VINT_SOURCE).read_text(encoding="utf-8")
    timer = (disasm / rail.TIMER_WINDOW_SOURCE).read_text(encoding="utf-8")
    rng = (disasm / rail.RNG_SOURCE).read_text(encoding="utf-8")
    with pytest.raises(ValueError, match="FRAME_COUNTER"):
        rail._time_state_source_use_sites(
            vint.replace(
                "addq.b  #1,((FRAME_COUNTER-$1000000)).w",
                "addq.w  #1,((FRAME_COUNTER-$1000000)).w",
                1,
            ),
            timer,
            rng,
        )
    with pytest.raises(ValueError, match="seconds-counter"):
        rail._time_state_source_use_sites(
            vint,
            timer.replace(
                "move.l  ((SECONDS_COUNTER-$1000000)).w,d1",
                "move.w  ((SECONDS_COUNTER-$1000000)).w,d1",
                1,
            ),
            rng,
        )

    listing = (_upstream() / "build/sf2build-h1.lst").read_text(encoding="utf-8")
    addresses = rail.listing_symbol_addresses(listing)
    rom = bytearray(_rom().read_bytes())
    rom[time_state["frameCounter"]["sourceInstructionAddress"]] ^= 1
    with pytest.raises(ValueError, match="H1/ROM instruction drift"):
        rail._time_state_contract(disasm, listing, addresses, bytes(rom), contract["ram"])

    observer = rail.OBSERVER.read_text(encoding="utf-8")
    for expected in (
        "read_span(config.ram.FRAME_COUNTER, 1)",
        "read_span(config.ram.RANDOM_SEED, 4)",
        "read_span(config.ram.SECONDS_COUNTER, 4)",
        "read_span(config.ram.SECONDS_COUNTER_FRAMES, 1)",
        'memory.read_u32_be(config.ram.RANDOM_SEED, "M68K BUS")',
        'memory.read_u32_be(config.ram.SECONDS_COUNTER, "M68K BUS")',
        "normalize_vint_time_outside_callback",
        "post-boundary-controlled-zeroed-vint-counters",
    ):
        assert expected in observer
    assert "read_span(config.ram.FRAME_COUNTER, 8)" not in observer
    assert 'memory.read_u32_be(config.ram.FRAME_COUNTER, "M68K BUS")' not in observer
