"""Static and adversarial tests for the Map 3 natural Battle 01 route rail."""

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

from sf2tool.h3 import map3_battle01_natural_route as rail
from sf2tool.h3.bizhawk import bizhawk_contract
from sf2tool.jsonio import load_json, validate_json


def _rom() -> Path:
    return rail.repo_path("local/roms/sf2-us.bin")


def _upstream() -> Path:
    return rail.repo_path("local/upstream/SF2DISASM")


@cache
def _contract() -> dict[str, Any]:
    return rail.build_map3_battle01_natural_route_source_contract(_rom(), _upstream())


def _admitted_start() -> dict[str, int]:
    return _contract()["r1"]["admittedStart"]


def _failure_payload() -> dict[str, Any]:
    return {
        "owner": rail.OWNER,
        "caseId": rail.CASE_IDS[0],
        "phase": "route",
        "role": "map-script",
        "actualPc": 0x4710C,
        "expectedPc": 0x4710C,
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


def _copy_route_sources(tmp_path: Path) -> Path:
    source = _upstream() / "disasm"
    target = tmp_path / "disasm"
    for relative in rail.ROUTE_SOURCE_PATHS:
        destination = target / relative
        destination.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(source / relative, destination)
    return target


def _execute_lua(harness: str) -> None:
    """Execute a narrow production-Lua extraction through the pinned runtime."""
    _, executable = bizhawk_contract()
    library = ctypes.CDLL(str(executable.parent / "dll" / "lua54.dll"))
    library.luaL_newstate.argtypes = []
    library.luaL_newstate.restype = ctypes.c_void_p
    library.luaL_openlibs.argtypes = [ctypes.c_void_p]
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
    state = library.luaL_newstate()
    assert state
    try:
        library.luaL_openlibs(state)
        encoded = harness.encode("utf-8")
        assert (
            library.luaL_loadbufferx(state, encoded, len(encoded), b"@r2-production-extract", b"t")
            == 0
        )
        result = library.lua_pcallk(state, 0, 0, 0, 0, None)
        if result:
            length = ctypes.c_size_t()
            pointer = library.lua_tolstring(state, -1, ctypes.byref(length))
            message = (
                ctypes.string_at(pointer, length.value).decode("utf-8") if pointer else str(result)
            )
            raise AssertionError(f"Lua production extraction failed: {message}")
    finally:
        library.lua_close(state)


def _run_lua_safe_core_snapshot_checkpoint(tmp_path: Path) -> dict[str, Any]:
    """Exercise production checkpoint callbacks around the deferred core snapshot."""
    source = rail.OBSERVER.read_text(encoding="utf-8")
    callback_registration = source[
        source.index("local function add_callback(address, role, handler)") : source.index(
            "local function write_menu_thunk(case)"
        )
    ]
    bootstrap_callbacks = source[
        source.index('-- "bootstrap-check-sram"') : source.index('-- "r1-witch-new-action"')
    ]
    main_loop_start = source.index('status("milestone:observer-started")')
    snapshot_transition = source[
        source.index("if pending_core_snapshot then", main_loop_start) : source.index(
            "    if finish_pending then", main_loop_start
        )
    ]
    result_path = (tmp_path / "lua-safe-core-snapshot-result.json").as_posix()
    harness = f'''\
local phase = "await-check-sram"
local active, scope, saved_state = nil, nil, nil
local callbacks, callback_order, registered = {{}}, {{}}, {{}}
local pending_core_snapshot, pending_failure = false, nil
local route_started, last_callback_role, last_callback_pc = false, nil, nil
local status_lines, save_calls, jumps = {{}}, 0, {{}}
local config = {{
  caseOrder={{"natural-map3-opening-to-messenger-entry"}},
  cases={{{{injectedInitialMenuReturn=1,injectedDifficultyMenuReturn=0}}}},
  r1={{
    functions={{checkSramAddress=1,newActionAddress=2}},
    harness={{checkpointAddress=3}},
    sessionPatches={{}}
  }},
  ram={{
    GAME_FLAGS=10,LONGWORD_GAMEFLAGS_COUNTER=0,COMBATANT_DATA=20,
    COMBATANT_ALLIES_COUNTER=0,COMBATANT_DATA_ENTRY_SIZE=1,CURRENT_MAP=30,
    ENTITY_DATA=40,ENTITYDEF_SIZE=1,CURRENT_GOLD=50
  }}
}}
local function read_span(_, _) return {{0}} end
local function patch_cart(_) end
local function write_jump(address, target)
  jumps[#jumps+1]={{address=address,target=target}}
end
local function write_menu_thunk(_) end
local function reg(_) return 0x100 end
local function status(value) status_lines[#status_lines+1]=value end
local function fail(role, expected_pc, message)
  pending_failure={{role=role,expectedPc=expected_pc,message=message}}
end
memory = {{write_u32_be=function(_, _, _) end}}
memorysavestate = {{savecorestate=function()
  save_calls=save_calls+1
  return {{saved=save_calls}}
end}}
event = {{on_bus_exec=function(handler, address, _, _)
  registered[address]=handler
  return address
end}}
{callback_registration}
{bootstrap_callbacks}
registered[config.r1.functions.checkSramAddress]()
assert(phase == "await-safe-core-snapshot" and pending_core_snapshot,
  "check-sram did not enter safe deferred snapshot phase")
registered[config.r1.harness.checkpointAddress]()
assert(saved_state == nil and phase == "await-safe-core-snapshot",
  "first checkpoint hit was not inert before core snapshot")
assert(#status_lines == 1 and #jumps == 1,
  "first checkpoint hit started controlled admission before snapshot")
{snapshot_transition}
assert(saved_state ~= nil and save_calls == 1 and phase == "await-checkpoint",
  "outer-loop core snapshot did not explicitly arm checkpoint admission")
registered[config.r1.harness.checkpointAddress]()
assert(phase == "await-r1-new-action" and #jumps == 2,
  "checkpoint did not begin admission after explicit phase advance")
local saved_index, admission_index = nil, nil
for index, value in ipairs(status_lines) do
  if value == "milestone:r1-core-state-saved-outside-callback" then saved_index=index end
  if value == "milestone:r1-controlled-admission-started" then admission_index=index end
end
assert(saved_index and admission_index and saved_index < admission_index,
  "success milestones admitted control before core state save")
local out=assert(io.open("{result_path}", "w"))
out:write('{{"saveCalls":'..save_calls..',"savedBeforeAdmission":'
  .. tostring(saved_index < admission_index) .. ',"phase":"'..phase..'"}}')
out:close()
'''
    _execute_lua(harness)
    return json.loads((tmp_path / "lua-safe-core-snapshot-result.json").read_text(encoding="utf-8"))


def _run_lua_success_finalization_fault(
    tmp_path: Path, *, restoration_mismatch: bool, unregister_fault: bool
) -> dict[str, Any]:
    """Exercise production deferred finalization without starting an emulator."""
    source = rail.OBSERVER.read_text(encoding="utf-8")
    helpers = source[
        source.index("local function first_mismatch(") : source.index(
            "local function current_position()"
        )
    ]
    finalizers = source[
        source.index("local function write_observation(restoration)") : source.index(
            'status("milestone:observer-started")'
        )
    ]
    result_path = (tmp_path / "lua-finalization-result.json").as_posix()
    status_path = (tmp_path / "lua-finalization-status.txt").as_posix()
    output_path = (tmp_path / "accepted-observation.json").as_posix()
    mismatch_literal = "true" if restoration_mismatch else "false"
    unregister_literal = "true" if unregister_fault else "false"
    harness = f'''\
local OWNER = "map3-battle01-natural-route"
local phase = "route"
local active = {{caseId = "natural-map3-opening-to-messenger-entry"}}
local saved_state = {{}}
local scope = {{
  gameFlags={{1}}, combatantAllyRecords={{1}}, mapAndBattleState={{1}},
  playerEntity={{1}}, gold={{1}}, generatedRam={{1}}
}}
local callbacks, callback_order = {{[11]={{id=11}}, [12]={{id=12}}}}, {{11, 12}}
local pending_failure, finish_pending = nil, true
local _status_lines, _exit_codes, _unregister_calls, _load_calls = {{}}, {{}}, 0, 0
local _mismatch, _unregister_fault = {mismatch_literal}, {unregister_literal}
local config = {{
  outputPath="{output_path}", statusPath="{status_path}",
  r1={{harness={{checkpointAddress=9}}, sessionPatches={{}}}},
  ram={{GAME_FLAGS=1, COMBATANT_DATA=2, CURRENT_MAP=3, ENTITY_DATA=4, CURRENT_GOLD=5}},
  observerFailureContract={{exitCode=79}}
}}
local _values = {{}}
for address=1,20 do _values[address]=1 end
local function reg(_) return 0x20A02 end
local function status(value) _status_lines[#_status_lines+1]=value end
local function json_escape(value) return tostring(value) end
local function restore_span(address, expected)
  for offset,value in ipairs(expected) do _values[address+offset-1]=value end
end
memory = {{
  read_u8=function(address,_) return _values[address] or 0 end,
  write_u8=function(address,value,_) _values[address]=value end
}}
memorysavestate = {{
  loadcorestate=function(_)
    _load_calls=_load_calls+1
    for address=1,20 do _values[address]=1 end
    if _mismatch and _load_calls==1 then _values[1]=2 end
  end
}}
event = {{
  unregisterbyid=function(_)
    _unregister_calls=_unregister_calls+1
    if _unregister_fault and _unregister_calls==1 then error("injected unregister failure") end
  end
}}
client = {{exitCode=function(code) _exit_codes[#_exit_codes+1]=code end}}
{helpers}
{finalizers}
finalize_success()
assert(pending_failure ~= nil, "faulted finalization did not queue typed failure")
finalize_failure()
assert(#_exit_codes == 1 and _exit_codes[1] == 79, "typed failure exit drift")
assert(
  #callback_order == 0 and next(callbacks) == nil,
  "residual callback after failure finalization"
)
    local status_file=assert(io.open(config.statusPath,"r"))
    local terminal_status=status_file:read("*a")
    status_file:close()
    local first_failure=string.find(terminal_status,"failure:observer-callback:",1,true)
    assert(first_failure ~= nil, terminal_status)
local second_failure=string.find(
  terminal_status,"failure:observer-callback:",first_failure+1,true
)
assert(not string.find(terminal_status,"observer-finished",1,true), "no success milestone")
assert(second_failure==nil, terminal_status)
assert(not io.open(config.outputPath,"r"), "fault emitted accepted observation")
local out=assert(io.open("{result_path}","w"))
out:write('{{"unregisterCalls":'.._unregister_calls..',"loadCalls":'.._load_calls..',"failureCount":1}}')
out:close()
'''
    _, executable = bizhawk_contract()
    library = ctypes.CDLL(str(executable.parent / "dll" / "lua54.dll"))
    library.luaL_newstate.argtypes = []
    library.luaL_newstate.restype = ctypes.c_void_p
    library.luaL_openlibs.argtypes = [ctypes.c_void_p]
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
    state = library.luaL_newstate()
    assert state
    try:
        library.luaL_openlibs(state)
        encoded = harness.encode("utf-8")
        assert (
            library.luaL_loadbufferx(state, encoded, len(encoded), b"@r2-finalization", b"t") == 0
        )
        result = library.lua_pcallk(state, 0, 0, 0, 0, None)
        if result:
            length = ctypes.c_size_t()
            pointer = library.lua_tolstring(state, -1, ctypes.byref(length))
            message = (
                ctypes.string_at(pointer, length.value).decode("utf-8") if pointer else str(result)
            )
            raise AssertionError(f"Lua finalization harness failed: {message}")
    finally:
        library.lua_close(state)
    result = json.loads((tmp_path / "lua-finalization-result.json").read_text(encoding="utf-8"))
    result["status"] = (tmp_path / "lua-finalization-status.txt").read_text(encoding="utf-8")
    return result


def _run_lua_route_phase_watchdog(tmp_path: Path) -> dict[str, str]:
    """Execute the production route watchdog path without an emulator launch."""
    source = rail.OBSERVER.read_text(encoding="utf-8")
    watchdog = source[
        source.index("local function note_route_progress()") : source.index(
            "-- The movement controller calls WarpIfSetAtPoint"
        )
    ]
    result_path = (tmp_path / "lua-route-watchdog-result.json").as_posix()
    harness = f'''\
local route_started, finish_pending, pending_failure = true, false, nil
local route_progress_frame, frame_count = 1, 1202
local phase, route_index, planned_input_index = "route", 1, 7
local last_input, observed_warp_id = "", "map3-bowie-house-exit"
local wait_after_warp, route_control_ready = true, false
local automation_epoch, automation_expected_input, automation_expected_mask = 9, "", 0
local last_callback_role, last_callback_pc = "warp", 153976
local ROUTE_PHASE_WATCHDOG_FRAME_LIMIT = 1200
local config = {{
  route={{waypoints={{{{id="map3-bowie-house-exit"}}}}}},
  automation={{markerAddress=99}}
}}
memory = {{read_u8=function(address, _) assert(address == 99); return 9 end}}
local function current_position() return 3, 55, 3 end
local function fail(role, expected_pc, message)
  pending_failure={{role=role, expectedPc=expected_pc, message=message}}
end
{watchdog}
enforce_route_phase_watchdog()
assert(
  pending_failure and pending_failure.role == "route-phase-watchdog",
  "route watchdog did not queue typed failure"
)
assert(
  string.find(pending_failure.message, "pendingWarp=map3-bowie-house-exit", 1, true),
  pending_failure.message
)
assert(
  string.find(pending_failure.message, "lastCallback=warp/153976", 1, true),
  pending_failure.message
)
local out=assert(io.open("{result_path}","w"))
out:write('{{"role":"'..pending_failure.role..'","error":"'..pending_failure.message..'"}}')
out:close()
'''
    _execute_lua(harness)
    return json.loads((tmp_path / "lua-route-watchdog-result.json").read_text(encoding="utf-8"))


def _run_lua_entity142_masked_facing_branch(tmp_path: Path) -> dict[str, Any]:
    """Exercise the production adjacent-entity facing branch without BizHawk."""
    source = rail.OBSERVER.read_text(encoding="utf-8")
    route_input = source[
        source.index("local function route_input()") : source.index(
            "append_trace = function(kind, value)"
        )
    ]
    result_path = (tmp_path / "lua-entity142-facing-result.json").as_posix()
    harness = f'''\
local route_started, finish_pending, route_control_ready = true, false, true
local planned_input_committed, route_index, frame_count = false, 1, 1
local entity142_face_left_seen = false
local config = {{
  ram={{UP=1,LEFT=2,DIRECTION_MASK=3}},
  route={{
    waypoints={{{{id="map3-entity142",map=3,x=55,y=17,facing="Left",interaction="entity",entityTarget={{id=142}}}}}},
    navigation={{inputPlan={{}}}}
  }}
}}
local issued_input = nil
local function current_position() return 3, 55, 17, 0x12 end
local function advance_planned_inputs(_, _, _) end
local function advance_completed_waypoints() end
local function bounded_route_wait(_, _, _, _, _) end
local function set_input(input, _) issued_input = input end
{route_input}
route_input()
assert(entity142_face_left_seen, "raw facing mask did not admit the Left entity branch")
assert(issued_input == "C", "masked-facing entity branch did not issue original interaction input")
local out=assert(io.open("{result_path}","w"))
out:write(
  '{{"facingMasked":' .. (0x12 & config.ram.DIRECTION_MASK)
    .. ',"input":"' .. issued_input .. '"}}'
)
out:close()
'''
    _execute_lua(harness)
    return json.loads((tmp_path / "lua-entity142-facing-result.json").read_text(encoding="utf-8"))


def _run_lua_zone_input_neutral_then_house_door(tmp_path: Path) -> dict[str, Any]:
    """Exercise the production zone-controller branch without BizHawk.

    The synthetic route may wait for the original zone callback, but it must
    not press C.  The first source-plan edge after the house-zone callback is
    the door's Down edge.
    """
    source = rail.OBSERVER.read_text(encoding="utf-8")
    route_input = source[
        source.index("local function route_input()") : source.index(
            "append_trace = function(kind, value)"
        )
    ]
    zone_start = route_input.index('elseif waypoint.interaction == "zone" then')
    zone_end = route_input.index("        else", zone_start)
    zone_branch = route_input[zone_start:zone_end]
    assert 'set_input("", waypoint.id)' in zone_branch
    assert '"C"' not in zone_branch
    result_path = (tmp_path / "lua-zone-input-result.json").as_posix()
    harness = f'''\
local route_started, finish_pending, route_control_ready = true, false, true
local planned_input_committed, planned_input_index, route_index, frame_count = false, 1, 1, 1
local config = {{
  route={{
    waypoints={{
      {{id="map3-house-exit-zone",map=3,x=4,y=4,interaction="zone"}},
      {{id="map3-bowie-house-door",map=3,x=4,y=8,interaction="step"}}
    }},
    navigation={{inputPlan={{}}}}
  }}
}}
local issued_input, issued_waypoint = nil, nil
local function current_position() return 3, 4, 4, 0 end
local function advance_planned_inputs(_, _, _) end
local function advance_completed_waypoints() end
local function bounded_route_wait(_, _, _, _, _) end
local function set_input(input, waypoint) issued_input, issued_waypoint = input, waypoint end
{route_input}
route_input()
assert(issued_input == "", "zone waypoint emitted controller input " .. tostring(issued_input))
assert(issued_waypoint == "map3-house-exit-zone", "zone waypoint identity drift")
local zone_input = issued_input
route_index = 2
config.route.navigation.inputPlan = {{
  {{waypoint="map3-bowie-house-door",from={{map=3,x=4,y=4}},to={{map=3,x=4,y=5}},input="Down"}}
}}
route_input()
assert(issued_input == "Down" and issued_waypoint == "map3-bowie-house-door",
  "house-door first source edge did not follow zone callback")
local out = assert(io.open("{result_path}", "w"))
out:write('{{"zoneInput":"' .. zone_input .. '","nextInput":"' .. issued_input
  .. '","nextWaypoint":"' .. issued_waypoint .. '"}}')
out:close()
'''
    _execute_lua(harness)
    return json.loads((tmp_path / "lua-zone-input-result.json").read_text(encoding="utf-8"))


def _run_lua_logical_input_edge_contract(tmp_path: Path) -> dict[str, Any]:
    """Exercise production logical-edge accounting without an emulator launch."""
    source = rail.OBSERVER.read_text(encoding="utf-8")
    start = source.index("local function logical_input_edge(map, x, y, input, waypoint)")
    end = source.index("local function input_mask(input)")
    record_input = source[start:end]
    result_path = (tmp_path / "lua-logical-input-result.json").as_posix()
    harness = f'''\
local route_started, last_input = true, nil
local input_trace = {{}}
local recorded_input_plan_index = 1
local recorded_sarah_action, recorded_entity142_face = false, false
local recorded_entity142_action = false
local zone_admissions_seen = {{}}
local first_logical_duplicate, first_logical_unmodeled = nil, nil
local config = {{route={{
  navigation={{inputPlan={{
    {{waypoint="map3-astral-zone-introduction",from={{map=3,x=59,y=12}},to={{map=3,x=58,y=13}},input="Left"}},
    {{waypoint="map3-entity142",from={{map=3,x=58,y=13}},to={{map=3,x=58,y=14}},input="Down"}}
  }}}},
}}}}
local position_x, position_y = 59, 12
local function current_position() return 3, position_x, position_y end
{record_input}
local missing = logical_input_closure_diagnostic()
local missing_expected = "firstMissing=map=3 x=59 y=12 input=Left "
  .. "waypoint=map3-astral-zone-introduction"
assert(
  string.find(missing, missing_expected, 1, true),
  "missing logical edge did not report its exact source-plan edge: " .. missing
)
record_input("Left", "map3-astral-zone-introduction")
record_input("", "movement-commit")
local duplicate_ok = pcall(function() record_input("Left", "map3-astral-zone-introduction") end)
assert(not duplicate_ok, "duplicate source-plan input did not fail closed")
position_x, position_y = 58, 13
record_input("Down", "map3-entity142")
assert(
  #input_trace == 2 and input_trace[2].input == "Down"
    and input_trace[2].waypoint == "map3-entity142",
  "Zone7 introduction did not advance directly to the next source-plan Down edge"
)
local down_after_zone7_accepted = true
last_input, input_trace, recorded_input_plan_index = nil, {{}}, 1
recorded_sarah_action, recorded_entity142_face, recorded_entity142_action = false, false, false
zone_admissions_seen, first_logical_duplicate, first_logical_unmodeled = {{}}, nil, nil
config.route.navigation.inputPlan = {{
  {{
    waypoint="map3-sarah-classroom",
    from={{map=3,x=42,y=10}}, to={{map=3,x=42,y=9}}, input="Up"
  }}
}}
position_x, position_y = 42, 10
record_input("Up", "map3-sarah-classroom")
record_input("", "movement-commit")
position_x, position_y = 42, 9
record_input("C", "map3-sarah-classroom")
record_input("", "await-action")
record_input("C", "map3-sarah-classroom")
assert(
  #input_trace == 2 and recorded_sarah_action,
  "C polling was promoted to multiple logical edges"
)
local trace_count = #input_trace
last_input, input_trace, recorded_input_plan_index = nil, {{}}, 1
recorded_sarah_action, recorded_entity142_face, recorded_entity142_action = false, false, false
zone_admissions_seen, first_logical_duplicate, first_logical_unmodeled = {{}}, nil, nil
config.route.navigation.inputPlan = {{
  {{
    waypoint="map3-astral-zone-introduction",
    from={{map=3,x=59,y=12}}, to={{map=3,x=58,y=13}}, input="Left"
  }}
}}
position_x, position_y = 58, 13
local unmodeled_ok, unmodeled_error = pcall(function()
  record_input("C", "map3-astral-zone-introduction")
end)
assert(not unmodeled_ok and first_logical_unmodeled ~= nil,
  "unmodeled logical edge did not fail closed")
assert(string.find(unmodeled_error,"unmodeled source-derived logical input edge",1,true),
  "unmodeled logical edge diagnostic drift: " .. tostring(unmodeled_error))
local out=assert(io.open("{result_path}","w"))
out:write(
  '{{"traceCount":' .. trace_count
    .. ',"downAfterZone7Accepted":' .. tostring(down_after_zone7_accepted)
    .. ',"duplicateRejected":' .. tostring(not duplicate_ok)
    .. ',"missingReported":' .. tostring(string.find(missing,"firstMissing=",1,true) ~= nil)
    .. ',"unmodeledRejected":' .. tostring(not unmodeled_ok) .. '}}'
)
out:close()
'''
    _execute_lua(harness)
    return json.loads((tmp_path / "lua-logical-input-result.json").read_text(encoding="utf-8"))


def _run_lua_zone_admission_accounting(tmp_path: Path) -> dict[str, Any]:
    """Exercise the production callback-only zone-admission accounting."""
    source = rail.OBSERVER.read_text(encoding="utf-8")
    zone_callbacks = source[
        source.index('for _, symbol in ipairs({"Map3_ZoneEvent0"') : source.index('-- "step-door"')
    ]
    result_path = (tmp_path / "lua-zone-admission-result.json").as_posix()
    harness = f'''\
local route_started, route_index = true, 1
local pending_zone_admission = {{id="map3-house-exit-zone",x=4,y=4}}
local zone_admissions_seen, messenger_zone_admission = {{}}, nil
local callback_trace, callbacks = {{}}, {{}}
local config = {{
  functions={{Map3_ZoneEvent0=0,Map3_ZoneEvent6=6,Map3_ZoneEvent7=7,Map3_ZoneEvent8=8}},
  route={{waypoints={{{{id="map3-house-exit-zone",map=3,x=4,y=4}}}}}}
}}
local function append_trace(kind, value)
  callback_trace[#callback_trace + 1] = kind .. ":" .. value
end
local function flag_is_set(_) return false end
local function add_callback(address, _, handler) callbacks[address] = handler end
{zone_callbacks}
callbacks[6]()
assert(
  zone_admissions_seen["map3-house-exit-zone"],
  "zone callback did not record its source admission"
)
assert(pending_zone_admission == nil, "zone callback did not close its pending raw admission")
local out = assert(io.open("{result_path}", "w"))
out:write('{{"zoneAdmission":' .. tostring(zone_admissions_seen["map3-house-exit-zone"])
  .. ',"trace":"' .. callback_trace[1] .. '"}}')
out:close()
'''
    _execute_lua(harness)
    return json.loads((tmp_path / "lua-zone-admission-result.json").read_text(encoding="utf-8"))


def _run_lua_map3_init_physical_pc_dispatch(tmp_path: Path) -> dict[str, Any]:
    """Exercise the production aliased-PC dispatch and typed callback error path."""
    source = rail.OBSERVER.read_text(encoding="utf-8")
    callback_registration = source[
        source.index("local function add_callback(address, role, handler)") : source.index(
            "local function write_menu_thunk(case)"
        )
    ]
    map3_init_dispatch = source[
        source.index("-- `selectedInitAddress` aliases") : source.index('-- "r1-init-return"')
    ]
    result_path = (tmp_path / "lua-map3-init-dispatch-result.json").as_posix()
    harness = f'''\
local callbacks, callback_order, registered = {{}}, {{}}, {{}}
local pending_failure = nil
local route_started, phase, current_map = false, "await-r1-init", 3
local last_callback_role, last_callback_pc = nil, nil
local map3_init_seen, chronology = false, {{}}
local config = {{r1={{functions={{selectedInitAddress=0x51382}}}}, ram={{CURRENT_MAP=0xFFB000}}}}
local function append_trace(kind, value) chronology[#chronology+1]=kind..":"..value end
local function fail(role, pc, message) pending_failure={{role=role, pc=pc, message=message}} end
memory = {{read_u8=function(address, _)
  assert(address == config.ram.CURRENT_MAP)
  return current_map
end}}
event = {{on_bus_exec=function(handler, address, _, _)
  registered[address]=handler
  return address
end}}
{callback_registration}
{map3_init_dispatch}
assert(
  #callback_order == 1 and callbacks[0x51382].role == "map3-init-dispatch",
  "aliased PC was not registered once"
)
local duplicate_ok = pcall(function()
  add_callback(config.r1.functions.selectedInitAddress, "duplicate-map3-init", function() end)
end)
assert(not duplicate_ok, "duplicate physical-PC role was admitted")
registered[0x51382]()
assert(phase == "await-r1-init-return" and not map3_init_seen, "R1 phase dispatch drift")
phase, route_started = "route", true
registered[0x51382]()
assert(
  map3_init_seen and chronology[1] == "map-init:ms_map3_InitFunction",
  "route map-init dispatch drift"
)
current_map = 2
registered[0x51382]()
assert(
  pending_failure and pending_failure.role == "map3-init-dispatch",
  "callback exception did not enter typed failure"
)
local out=assert(io.open("{result_path}","w"))
out:write('{{"callbackCount":'..#callback_order..',"role":"'..pending_failure.role..'"}}')
out:close()
'''
    _execute_lua(harness)
    return json.loads((tmp_path / "lua-map3-init-dispatch-result.json").read_text(encoding="utf-8"))


def test_static_contract_is_source_h1_rom_bound() -> None:
    fixture = load_json(rail.FIXTURE)
    validate_json(fixture, rail.FIXTURE_SCHEMA, owner=rail.OWNER)
    contract = _contract()
    rail._assert_fixture(fixture, contract)
    assert contract["r1"]["admittedStart"] == {"map": 3, "x": 56, "y": 3, "facing": 3}
    assert contract["route"]["maps"] == [3, 19, 20, 21, 40, 57]
    assert contract["route"]["battle01"]["beforeCutscene"] == "bbcs_01"
    assert contract["functions"]["ExecuteIndividualTurn"] == 0x23EB0
    assert contract["functions"]["ProcessPlayerAction"] == 0x25AD6
    assert contract["functions"]["GetActivatedEntity"] == 0x2379A
    assert contract["functions"]["esc02_controlCharacter"] == 0x4FF8
    assert contract["functions"]["loc_52E8"] == 0x52E8
    assert contract["functions"]["ms_map20_InitFunction"] == 0x53966
    assert contract["functions"]["return_53994"] == 0x53994
    assert contract["functions"]["cs_53996"] == 0x53996
    assert contract["functions"]["cs_53B60"] == 0x53B60
    assert contract["ram"]["ENTITYDEF_OFFSET_LAYER"] == 17
    assert contract["ram"]["MAP_AREA_LAYER2_STARTY"] == 0xFFA838
    assert contract["ram"]["ENTITYDEF_OFFSET_FLAGS_A"] == 28
    assert contract["ram"]["MAP_EVENT_PARAM_1"] == 0xFFA84C
    assert contract["ram"]["VIEW_TARGET_ENTITY"] == 0xFFA82C
    assert contract["ram"]["ENTITY_INDEX_LIST"] == 0xFFB140
    assert contract["ram"]["COMBATANT_MASK_ALL"] == 0xFF
    assert contract["ram"]["ENTITY_ENEMY_INDEX_DIFFERENCE"] == 0x60
    assert contract["ram"]["ALLY_SARAH"] == 1
    assert tuple(
        contract["ram"][key]
        for key in (
            "INPUT_BIT_UP",
            "INPUT_BIT_DOWN",
            "INPUT_BIT_LEFT",
            "INPUT_BIT_RIGHT",
            "INPUT_BIT_C",
        )
    ) == (0, 1, 2, 3, 5)
    assert contract["ram"]["DIRECTION_MASK"] == 3
    assert tuple(contract["ram"][key] for key in ("RIGHT", "UP", "LEFT", "DOWN")) == (0, 1, 2, 3)
    assert contract["functions"]["Map3_ZoneEvent0"] == 0x50D74
    assert contract["functions"]["Map3_EntityEvent0"] == 0x50F54
    assert contract["functions"]["Map3_EntityEvent15"] == 0x51044
    assert contract["functions"]["Map3_ZoneEvent7"] == 0x50E66
    assert contract["functions"]["ms_map3_InitFunction"] == 0x51382
    assert contract["functions"]["cs_513A0"] == 0x513A0
    assert contract["functions"]["cs_513D6"] == 0x513D6
    assert contract["functions"]["cs_51444"] == 0x51444
    assert contract["functions"]["cs_5148C"] == 0x5148C
    assert contract["functions"]["csc19_setEntityPosAndFacing"] == 0x46A12
    assert contract["functions"]["GetEntityAddressFromCharacter"] == 0x4704A
    assert "cs_53104" in contract["route"]["scriptSymbols"]
    assert "cs_51444" not in contract["route"]["scriptSymbols"]
    navigation = contract["route"]["navigation"]
    assert navigation["area"] == {"startX": 0, "startY": 0, "endX": 50, "endY": 31}
    assert navigation["layoutWordCount"] == 4096
    assert navigation["mapOffsetHashBytes"] == 1152
    assert navigation["mapCollidableFlagBit"] == 6
    assert navigation["blockedLayoutWordFloor"] == 0xC000
    assert navigation["collisionEnforcedMaps"] == [3]
    assert navigation["eventBeforeCollision"] == {
        "mapWordEventMask": 0x3C00,
        "warpSelector": 0x1000,
        "zoneSelector": 0x1400,
        "collisionFloor": 0xC000,
        "order": ["warp", "zone", "collision"],
    }
    assert navigation["postWarpLanding"] == {
        "map": 3,
        "x": 3,
        "y": 3,
        "layoutOffsetBytes": 390,
        "layoutWord": 0x5035,
    }
    assert navigation["postWarpSlope"] == {
        "input": "Right",
        "sourceOffsetBytes": 0x82,
        "fromLayoutWord": 0x5035,
        "to": {"map": 3, "x": 4, "y": 4},
        "toLayoutOffsetBytes": 520,
        "toLayoutWord": 0x5444,
    }
    assert navigation["schoolPathEntityBlocks"] == [
        {
            "sourceRecord": 0,
            "entity": "ALLY_SARAH",
            "map": 3,
            "x": 42,
            "y": 8,
            "beforeWaypoint": "map3-school-stairs-down",
        },
        {
            "sourceRecord": 1,
            "entity": "ALLY_CHESTER",
            "map": 3,
            "x": 44,
            "y": 10,
            "beforeWaypoint": "map3-school-stairs-down",
        },
    ]
    assert navigation["schoolSarahProgram"] == {
        "entityTarget": {"id": 1, "map": 3, "x": 42, "y": 8, "facing": "Down"},
        "event": "Map3_EntityEvent0",
        "program": "cs_513D6",
        "completionFlag": 256,
        "postProgramPosition": {"map": 3, "x": 41, "y": 7},
    }
    assert navigation["postAstralRouteOccupancy"] == {
        "sourceRecord": 1,
        "entity": "ALLY_CHESTER",
        "map": 3,
        "x": 44,
        "y": 10,
        "unmovedByProgram": "cs_5148C",
        "beforeWaypoint": "map3-zone-messenger",
    }
    assert contract["route"]["runtimeOpening"]["endpoint"] == {
        "sourceTarget": {"map": 3, "x": 43, "y": 10},
        "program": "cs_5149A",
        "notYetMutatedFlag": 603,
    }
    assert navigation["astralZoneProgram"] == {
        "event": "Map3_ZoneEvent7",
        "program": "cs_5148C",
        "completionFlag": 260,
        "postProgramPositions": [
            {
                "rawCharacter": 1,
                "entityIndexSelector": 1,
                "map": 3,
                "x": 41,
                "y": 10,
                "facing": "Up",
            },
            {
                "rawCharacter": 128,
                "entityIndexSelector": 32,
                "map": 3,
                "x": 6,
                "y": 4,
                "facing": "Up",
            },
        ],
    }
    assert navigation["entity142ReinitProgram"] == {
        "triggerFlag": 602,
        "precedingFlagClear": 1,
        "program": "cs_513A0",
        "postProgramPosition": {"map": 3, "x": 41, "y": 10, "facing": "Up"},
    }
    assert len(navigation["inputPlan"]) == 216
    assert navigation["inputPlan"][0] == {
        "waypoint": "map3-bowie-house-exit",
        "from": {"map": 3, "x": 56, "y": 3},
        "to": {"map": 3, "x": 55, "y": 3},
        "input": "Left",
    }
    assert navigation["inputPlan"][2] == {
        "waypoint": "map3-house-exit-zone",
        "from": {"map": 3, "x": 3, "y": 3},
        "to": {"map": 3, "x": 4, "y": 4},
        "input": "Right",
    }
    assert navigation["inputPlan"][61] == {
        "waypoint": "map3-sarah-classroom",
        "from": {"map": 3, "x": 42, "y": 10},
        "to": {"map": 3, "x": 42, "y": 9},
        "input": "Up",
    }
    assert navigation["inputPlan"][62] == {
        "waypoint": "map3-school-stairs-down",
        "from": {"map": 3, "x": 42, "y": 9},
        "to": {"map": 3, "x": 42, "y": 8},
        "input": "Up",
    }
    assert navigation["inputPlan"][67] == {
        "waypoint": "map3-school-stairs-down",
        "from": {"map": 3, "x": 45, "y": 7},
        "to": {"map": 3, "x": 46, "y": 7},
        "input": "Right",
    }
    assert navigation["inputPlan"][75] == {
        "waypoint": "map3-entity142",
        "from": {"map": 3, "x": 55, "y": 16},
        "to": {"map": 3, "x": 55, "y": 17},
        "input": "Down",
    }
    messenger_plan = [
        step for step in navigation["inputPlan"] if step["waypoint"] == "map3-zone-messenger"
    ]
    assert [step["input"] for step in messenger_plan] == [
        "Down",
        "Left",
        "Left",
        "Left",
        "Left",
        "Down",
        "Down",
        "Right",
    ]
    assert [(step["from"]["x"], step["from"]["y"]) for step in messenger_plan] == [
        (46, 7),
        (46, 8),
        (45, 8),
        (44, 8),
        (43, 8),
        (42, 8),
        (42, 9),
        (42, 10),
    ]
    assert messenger_plan[-1]["to"] == {"map": 3, "x": 43, "y": 10}
    assert navigation["inputPlan"][68] == {
        "waypoint": "map3-astral-zone-introduction",
        "from": {"map": 3, "x": 59, "y": 12},
        "to": {"map": 3, "x": 58, "y": 13},
        "input": "Left",
    }
    assert navigation["inputPlan"][-1] == {
        "waypoint": "map40-entrance",
        "from": {"map": 40, "x": 4, "y": 13},
        "to": {"map": 40, "x": 4, "y": 12},
        "input": "Up",
    }
    route = contract["route"]
    assert route["waypoints"][4] == {
        "id": "map3-sarah-classroom",
        "map": 3,
        "x": 42,
        "y": 9,
        "facing": "Up",
        "interaction": "entity",
        "entityTarget": {"id": 1, "map": 3, "x": 42, "y": 8, "facing": "Down"},
        "completionFlag": 256,
    }
    assert route["waypoints"][6] == {
        "id": "map3-astral-zone-introduction",
        "map": 3,
        "x": 58,
        "y": 13,
        "facing": "None",
        "interaction": "zone",
        "completionEvent": "Map3_ZoneEvent7",
    }
    assert route["waypoints"][7] == {
        "id": "map3-entity142",
        "map": 3,
        "x": 55,
        "y": 17,
        "facing": "Left",
        "interaction": "entity",
        "entityTarget": {"id": 142, "map": 3, "x": 54, "y": 17, "facing": "Up"},
        "completionFlag": 602,
    }
    assert route["waypoints"][8] == {
        "id": "map3-astral-zone",
        "map": 3,
        "x": 58,
        "y": 13,
        "facing": "None",
        "interaction": "zone",
        "completionFlag": 260,
    }
    assert [waypoint["id"] for waypoint in route["waypoints"]][14:20] == [
        "map19-kings-room-warp",
        "map20-palace-scene",
        "map20-kings-room-exit",
        "map19-astral",
        "map19-west-tower-warp",
        "map20-west-tower-warp",
    ]
    assert route["waypoints"][15] == {
        "id": "map20-palace-scene",
        "map": 20,
        "x": 23,
        "y": 39,
        "facing": "None",
        "interaction": "scene",
        "completionFlag": 605,
    }
    assert route["warps"][5:7] == [
        {
            "fromMap": 20,
            "toMap": 19,
            "eventDestinationMap": 19,
            "x": 23,
            "y": 37,
            "destinationX": 23,
            "destinationY": 3,
        },
        {
            "fromMap": 19,
            "toMap": 20,
            "eventDestinationMap": 20,
            "x": 6,
            "y": 2,
            "destinationX": 6,
            "destinationY": 37,
        },
    ]
    assert [warp["eventDestinationMap"] for warp in route["warps"]] == [
        255,
        255,
        255,
        19,
        20,
        19,
        20,
        21,
        40,
        57,
    ]
    assert [
        waypoint["completionDestination"]
        for waypoint in route["waypoints"]
        if waypoint["interaction"] == "warp"
    ] == [
        {"map": 3, "x": 3, "y": 3},
        {"map": 3, "x": 59, "y": 12},
        {"map": 3, "x": 46, "y": 7},
        {"map": 19, "x": 26, "y": 30},
        {"map": 20, "x": 23, "y": 37},
        {"map": 19, "x": 23, "y": 3},
        {"map": 20, "x": 6, "y": 37},
        {"map": 21, "x": 3, "y": 16},
        {"map": 40, "x": 4, "y": 30},
        {"map": 57, "x": 8, "y": 18},
    ]


def test_source_route_guards_reject_route_gate_and_warp_drift(tmp_path: Path) -> None:
    disasm = _copy_route_sources(tmp_path)
    events = disasm / "data/maps/entries/map03/mapsetups/s2_entityevents.asm"
    events.write_text(
        events.read_text(encoding="utf-8").replace("setFlg  602", "setFlg  601", 1),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="first entity gate"):
        rail._assert_source_route(disasm, _admitted_start())

    disasm = _copy_route_sources(tmp_path / "entity142-interaction")
    events = disasm / "data/maps/entries/map03/mapsetups/s2_entityevents.asm"
    events.write_text(
        events.read_text(encoding="utf-8").replace(
            "msEntityEvent 142, DOWN, Map3_EntityEvent15",
            "msEntityEvent 142, UP, Map3_EntityEvent15",
            1,
        ),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="entity 142 interaction row"):
        rail._assert_source_route(disasm, _admitted_start())

    disasm = _copy_route_sources(tmp_path / "entity142-placement")
    entities = disasm / "data/maps/entries/map03/mapsetups/s1_entities.asm"
    entities.write_text(
        entities.read_text(encoding="utf-8").replace(
            "msFixedEntity 54, 17, UP, MAPSPRITE_ASTRAL",
            "msFixedEntity 53, 17, UP, MAPSPRITE_ASTRAL",
            1,
        ),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="entity 142 Astral row"):
        rail._assert_source_route(disasm, _admitted_start())

    disasm = _copy_route_sources(tmp_path / "school-sarah-occupancy")
    entities = disasm / "data/maps/entries/map03/mapsetups/s1_entities.asm"
    entities.write_text(
        entities.read_text(encoding="utf-8").replace(
            "msFixedEntity 42, 8, DOWN, ALLY_SARAH, eas_Init",
            "msFixedEntity 42, 9, DOWN, ALLY_SARAH, eas_Init",
            1,
        ),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="school-path Sarah occupancy"):
        rail._assert_source_route(disasm, _admitted_start())

    disasm = _copy_route_sources(tmp_path / "school-chester-occupancy")
    entities = disasm / "data/maps/entries/map03/mapsetups/s1_entities.asm"
    entities.write_text(
        entities.read_text(encoding="utf-8").replace(
            "msFixedEntity 44, 10, UP, ALLY_CHESTER, eas_Init",
            "msFixedEntity 44, 11, UP, ALLY_CHESTER, eas_Init",
            1,
        ),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="school-path Chester occupancy"):
        rail._assert_source_route(disasm, _admitted_start())

    disasm = _copy_route_sources(tmp_path / "messenger-zone-row")
    zones = disasm / "data/maps/entries/map03/mapsetups/s3_zoneevents.asm"
    zones.write_text(
        zones.read_text(encoding="utf-8").replace(
            "msZoneEvent 43, 10, Map3_ZoneEvent8-ms_map3_ZoneEvents",
            "msZoneEvent 44, 10, Map3_ZoneEvent8-ms_map3_ZoneEvents",
            1,
        ),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="messenger zone row"):
        rail._assert_source_route(disasm, _admitted_start())

    disasm = _copy_route_sources(tmp_path / "astral-set-position-corpus")
    scripts = disasm / "data/maps/entries/map03/mapsetups/scripts_1.asm"
    scripts.write_text(
        scripts.read_text(encoding="utf-8").replace(
            "setPos 128,6,4,UP",
            "setPos ALLY_CHESTER,6,4,UP",
            1,
        ),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="Astral zone occupancy program set-position corpus"):
        rail._assert_source_route(disasm, _admitted_start())

    disasm = _copy_route_sources(tmp_path / "classroom-sarah-event")
    events = disasm / "data/maps/entries/map03/mapsetups/s2_entityevents.asm"
    events.write_text(
        events.read_text(encoding="utf-8").replace(
            "msEntityEvent ALLY_SARAH, DOWN, Map3_EntityEvent0",
            "msEntityEvent ALLY_SARAH, UP, Map3_EntityEvent0",
            1,
        ),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="classroom Sarah interaction row"):
        rail._assert_source_route(disasm, _admitted_start())

    disasm = _copy_route_sources(tmp_path / "classroom-sarah-program")
    scripts = disasm / "data/maps/entries/map03/mapsetups/scripts_1.asm"
    scripts.write_text(
        scripts.read_text(encoding="utf-8").replace("moveUp 1", "moveUp 2", 1),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="classroom Sarah movement program"):
        rail._assert_source_route(disasm, _admitted_start())

    disasm = _copy_route_sources(tmp_path / "entity142-action")
    actions = disasm / "code/gameflow/exploration/explorationvints.asm"
    actions.write_text(
        actions.read_text(encoding="utf-8").replace(
            "bsr.w   GetActivatedEntity", "bsr.w   GetEntityEventIndex", 1
        ),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="entity142 action admission seam"):
        rail._assert_source_route(disasm, _admitted_start())

    disasm = _copy_route_sources(tmp_path / "entity142-offsets")
    offsets = disasm / "code/gameflow/battle/battlefunctions/battlefunctions_0.asm"
    before_table, table_and_after = offsets.read_text(encoding="utf-8").split(
        "table_PixelOffsets_X:", 1
    )
    offsets.write_text(
        before_table
        + "table_PixelOffsets_X:"
        + table_and_after.replace("dc.w MAP_TILE_PLUS", "dc.w MAP_TILE_MINUS", 1),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="entity142 facing offset table"):
        rail._assert_source_route(disasm, _admitted_start())


def test_ram_contract_rejects_entity_facing_mask_drift(tmp_path: Path) -> None:
    disasm = tmp_path / "disasm"
    disasm.mkdir()
    for relative in (rail.CONST_SOURCE, rail.ENUM_SOURCE):
        source = _upstream() / "disasm" / relative
        destination = disasm / relative
        shutil.copy2(source, destination)
    enums = disasm / rail.ENUM_SOURCE
    enums.write_text(
        enums.read_text(encoding="utf-8").replace(
            "DIRECTION_MASK: equ 3", "DIRECTION_MASK: equ 7", 1
        ),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="source RAM stride drift"):
        rail._ram_contract(disasm)

    disasm = _copy_route_sources(tmp_path / "warp")
    warps = disasm / "data/maps/entries/map21/6-warp-events.asm"
    warps.write_text(
        warps.read_text(encoding="utf-8").replace("warpDest   4, 30", "warpDest   5, 30", 1),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="21->40"):
        rail._assert_source_route(disasm, _admitted_start())


def test_source_route_guard_rejects_post_warp_slope_layout_drift(tmp_path: Path) -> None:
    disasm = _copy_route_sources(tmp_path)
    offset_hash = disasm / "data/maps/global/mapoffsethashtable.bin"
    contents = bytearray(offset_hash.read_bytes())
    contents[3 * 6] = (contents[3 * 6] + 1) & 0x3F
    offset_hash.write_bytes(contents)
    with pytest.raises(ValueError, match="post-warp \\$5035->\\$5444 slope"):
        rail._assert_source_route(disasm, _admitted_start())

    disasm = _copy_route_sources(tmp_path / "house-exit")
    zones = disasm / "data/maps/entries/map03/mapsetups/s3_zoneevents.asm"
    zones.write_text(
        zones.read_text(encoding="utf-8").replace("setFlg  601", "setFlg  600", 1),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="house-exit interception"):
        rail._assert_source_route(disasm, _admitted_start())

    disasm = _copy_route_sources(tmp_path / "astral-zone")
    zones = disasm / "data/maps/entries/map03/mapsetups/s3_zoneevents.asm"
    zones.write_text(
        zones.read_text(encoding="utf-8").replace("setFlg  260", "setFlg  261", 1),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="Astral zone program gate"):
        rail._assert_source_route(disasm, _admitted_start())

    disasm = _copy_route_sources(tmp_path / "astral-zone-row")
    zones = disasm / "data/maps/entries/map03/mapsetups/s3_zoneevents.asm"
    zones.write_text(
        zones.read_text(encoding="utf-8").replace(
            "msZoneEvent 58, 13, Map3_ZoneEvent7", "msZoneEvent 57, 13, Map3_ZoneEvent7", 1
        ),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="Astral zone row"):
        rail._assert_source_route(disasm, _admitted_start())

    disasm = _copy_route_sources(tmp_path / "astral-zone-program")
    scripts = disasm / "data/maps/entries/map03/mapsetups/scripts_1.asm"
    scripts.write_text(
        scripts.read_text(encoding="utf-8").replace(
            "setPos ALLY_SARAH,41,10,UP", "setPos ALLY_SARAH,41,11,UP", 1
        ),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="Astral zone occupancy program"):
        rail._assert_source_route(disasm, _admitted_start())

    disasm = _copy_route_sources(tmp_path / "step-door")
    steps = disasm / "data/maps/entries/map03/4-step-events.asm"
    steps.write_text(
        steps.read_text(encoding="utf-8").replace("sbc 41, 13", "sbc 42, 13", 1),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="step-event corpus/order"):
        rail._assert_source_route(disasm, _admitted_start())

    disasm = _copy_route_sources(tmp_path / "map3-area")
    areas = disasm / "data/maps/entries/map03/2-areas.asm"
    areas.write_text(
        areas.read_text(encoding="utf-8").replace(
            "mainLayerEnd        50, 31", "mainLayerEnd        49, 31", 1
        ),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="area bounds/order"):
        rail._assert_source_route(disasm, _admitted_start())

    disasm = _copy_route_sources(tmp_path / "map-offset-double")
    movement = disasm / "code/common/scripting/entity/entityscriptengine_2.asm"
    movement.write_text(
        movement.read_text(encoding="utf-8").replace(
            "add.w   d2,d2\n                move.b  (a3,d2.w),d2",
            "add.w   d2,d1\n                move.b  (a3,d2.w),d2",
            1,
        ),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="map-offset transform"):
        rail._assert_source_route(disasm, _admitted_start())

    disasm = _copy_route_sources(tmp_path / "event-before-collision")
    movement = disasm / "code/common/scripting/entity/entityscriptengine_2.asm"
    movement.write_text(
        movement.read_text(encoding="utf-8").replace(
            "cmpi.w  #$1400,d3", "cmpi.w  #$1500,d3", 1
        ),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="event-before-collision"):
        rail._assert_source_route(disasm, _admitted_start())

    disasm = _copy_route_sources(tmp_path / "zone-raw-coordinate-abi")
    zone_dispatch = disasm / "code/gameflow/exploration/explorationfunctions_2.asm"
    zone_dispatch.write_text(
        zone_dispatch.read_text(encoding="utf-8").replace(
            "move.w  ((MAP_EVENT_PARAM_3-$1000000)).w,d2",
            "move.w  ((MAP_EVENT_PARAM_4-$1000000)).w,d2",
            1,
        ),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="zone raw-coordinate ABI"):
        rail._assert_source_route(disasm, _admitted_start())

    disasm = _copy_route_sources(tmp_path / "cutscene-character-alias")
    map_script_engine = disasm / "code/common/scripting/map/mapscriptengine_1.asm"
    map_script_engine.write_text(
        map_script_engine.read_text(encoding="utf-8").replace(
            "andi.w  #COMBATANT_MASK_ALL,d0",
            "andi.w  #$7F,d0",
            1,
        ),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="cutscene character/entity alias ABI"):
        rail._assert_source_route(disasm, _admitted_start())

    disasm = _copy_route_sources(tmp_path / "entity142-reinit")
    map3_init = disasm / "data/maps/entries/map03/mapsetups/s6_initfunction.asm"
    map3_init.write_text(
        map3_init.read_text(encoding="utf-8").replace("chkFlg  602", "chkFlg  601", 1),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="entity142 re-init program gate"):
        rail._assert_source_route(disasm, _admitted_start())

    disasm = _copy_route_sources(tmp_path / "game-flag-bit-abi")
    game_flags = disasm / "code/common/stats/gameflags.asm"
    game_flags.write_text(
        game_flags.read_text(encoding="utf-8").replace("lsr.b   d1,d0", "lsr.b   d0,d0", 1),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="game-flag bit ABI"):
        rail._assert_source_route(disasm, _admitted_start())

    disasm = _copy_route_sources(tmp_path / "zone-raw-coordinate-width")
    zone_dispatch = disasm / "code/gameflow/exploration/explorationfunctions_2.asm"
    zone_dispatch.write_text(
        zone_dispatch.read_text(encoding="utf-8").replace(
            "move.w  ((MAP_EVENT_PARAM_1-$1000000)).w,d1",
            "move.b  ((MAP_EVENT_PARAM_1-$1000000)).w,d1",
            1,
        ),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="zone raw-coordinate ABI"):
        rail._assert_source_route(disasm, _admitted_start())


def test_schema_valid_case_and_static_drift_reject_before_launch(
    tmp_path: Path, monkeypatch: pytest.MonkeyPatch
) -> None:
    fixture = load_json(rail.FIXTURE)
    canonical_fixture = deepcopy(fixture)
    fixture["cases"][0]["frameBudget"] += 1
    path = tmp_path / "fixture.json"
    path.write_text(json.dumps(fixture), encoding="utf-8")
    launched = False

    def unexpected_launch(**_: Any) -> dict[str, Any]:
        nonlocal launched
        launched = True
        raise AssertionError("observer must not launch")

    monkeypatch.setattr(rail, "FIXTURE", path)
    monkeypatch.setattr(rail, "run_observer", unexpected_launch)
    with pytest.raises(ValueError, match="input matrix|static projection|case"):
        rail.verify_map3_battle01_natural_route(_rom(), _upstream())
    assert not launched

    fixture = deepcopy(canonical_fixture)
    fixture["expectedObservation"]["records"][0]["chronology"][-1] = "endpoint:wrong"
    path.write_text(json.dumps(fixture), encoding="utf-8")
    with pytest.raises(ValueError, match="fixture expected matrix/golden"):
        rail.verify_map3_battle01_natural_route(_rom(), _upstream())
    assert not launched

    fixture = deepcopy(canonical_fixture)
    logical_trace = fixture["expectedObservation"]["records"][0]["logicalInputTrace"]
    zone7_left_index = logical_trace.index(
        {
            "map": 3,
            "x": 59,
            "y": 12,
            "input": "Left",
            "waypoint": "map3-astral-zone-introduction",
        }
    )
    logical_trace.insert(
        zone7_left_index + 1,
        {
            "map": 3,
            "x": 58,
            "y": 13,
            "input": "C",
            "waypoint": "map3-astral-zone-introduction",
        },
    )
    path.write_text(json.dumps(fixture), encoding="utf-8")
    with pytest.raises(ValueError, match="fixture expected matrix/golden"):
        rail.verify_map3_battle01_natural_route(_rom(), _upstream())
    assert not launched

    fixture = load_json(rail.FIXTURE)
    fixture["static"]["route"]["maps"] = [3, 19, 20, 21, 40, 58]
    with pytest.raises(ValueError, match="maps"):
        validate_json(fixture, rail.FIXTURE_SCHEMA, owner=rail.OWNER)


def test_expected_opening_matrix_is_source_projected_and_omits_scheduler_frames() -> None:
    contract = _contract()
    fixture = load_json(rail.FIXTURE)
    expected = rail._expected_runtime_observation(contract)
    assert fixture["expectedObservation"] == expected
    record = expected["records"][0]
    assert record["chronology"] == list(rail.EXPECTED_OPENING_CHRONOLOGY)
    assert record["scriptTrace"] == list(rail.EXPECTED_OPENING_SCRIPT_TRACE)
    assert record["fieldMenu"] == "not-reached"
    assert record["openingMap3"] == {
        "sourceTarget": {"map": 3, "x": 43, "y": 10},
        "program": "cs_5149A",
        "afterHouseExit": True,
        "classroomSarah": True,
        "afterEntity142": True,
        "afterAstralZone": True,
        "afterMessenger": False,
    }
    assert len(record["logicalInputTrace"]) == 95
    assert all("frame" not in row for row in record["logicalInputTrace"])
    assert record["logicalInputTrace"][0] == {
        "map": 3,
        "x": 56,
        "y": 3,
        "input": "Left",
        "waypoint": "map3-bowie-house-exit",
    }
    entity142_edges = [
        row
        for row in record["logicalInputTrace"]
        if row["waypoint"].startswith("map3-entity142") and row["input"] in {"Left", "C"}
    ]
    assert entity142_edges[-2:] == [
        {"map": 3, "x": 55, "y": 17, "input": "Left", "waypoint": "map3-entity142-face"},
        {"map": 3, "x": 55, "y": 17, "input": "C", "waypoint": "map3-entity142"},
    ]
    assert not any(
        row["input"] == "C" and "zone" in row["waypoint"]
        for row in record["logicalInputTrace"]
    )
    zone7_intro_left = {
        "map": 3,
        "x": 59,
        "y": 12,
        "input": "Left",
        "waypoint": "map3-astral-zone-introduction",
    }
    zone7_intro_index = record["logicalInputTrace"].index(zone7_intro_left)
    assert record["logicalInputTrace"][zone7_intro_index + 1] == {
        "map": 3,
        "x": 58,
        "y": 13,
        "input": "Down",
        "waypoint": "map3-entity142",
    }
    observed = deepcopy(expected)
    observed["records"][0]["chronology"][-1] = "endpoint:wrong"
    assert rail._first_matrix_difference(expected, observed) == (
        "$.records[0].chronology[45]: expected 'endpoint:cs_5149A-entry-before-body', "
        "actual 'endpoint:wrong'"
    )


def test_closed_case_identity_rejects_the_old_battle_ready_overclaim() -> None:
    old_case_id = "natural-map3-admission-to-battle01-ready"
    assert rail.CASE_IDS == ("natural-map3-opening-to-messenger-entry",)
    fixture = deepcopy(load_json(rail.FIXTURE))
    fixture["caseOrder"] = [old_case_id]
    with pytest.raises(ValueError, match="caseOrder"):
        validate_json(fixture, rail.FIXTURE_SCHEMA, owner=rail.OWNER)


def test_callback_failure_schema_closes_reachable_roles_and_phases() -> None:
    callback_roles = set(
        re.findall(
            r'add_callback\([^,\n]+,\s*"([^"]+)"',
            rail.OBSERVER.read_text(encoding="utf-8"),
        )
    )
    assert callback_roles == {
        "bootstrap-check-sram",
        "checkpoint",
        "r1-witch-new-action",
        "r1-new-game",
        "r1-save-game",
        "r1-main-loop",
        "r1-exploration-loop",
        "r1-map-setup-wrapper-entry",
        "r1-setup-resolution-return",
        "r1-init-call",
        "map3-init-dispatch",
        "r1-init-return",
        "r1-wait-for-event",
        "input-controller",
        "input-controller-move-commit",
        "player-action",
        "activated-entity",
        "entity-dispatch",
        "entity-event",
        "zone-admission",
        "zone-event",
        "step-door",
        "map-script",
        "warp",
    }
    terminal_roles = {
        "bootstrap-watchdog",
        "case-watchdog",
        "route-phase-watchdog",
        "restoration",
        "callback-cleanup",
    }
    reachable_phases = [
        "await-check-sram",
        "await-safe-core-snapshot",
        "await-checkpoint",
        "await-r1-new-action",
        "await-r1-new-game",
        "await-r1-save-game",
        "await-r1-main-loop",
        "await-r1-exploration",
        "await-r1-setup",
        "await-r1-init-call",
        "await-r1-init",
        "await-r1-init-return",
        "await-r1-wait",
        "route",
    ]
    schema = load_json(rail.FAILURE_SCHEMA)
    assert schema["properties"]["phase"]["enum"] == reachable_phases
    assert set(schema["properties"]["role"]["enum"]) == callback_roles | terminal_roles
    assert "battleReady" not in load_json(rail.FIXTURE_SCHEMA)["definitions"]

    for role in callback_roles | terminal_roles:
        payload = _failure_payload()
        payload["role"] = role
        validate_json(payload, rail.FAILURE_SCHEMA, owner=rail.OWNER)
    for role in ("arbitrary-role", "check-battle", "battle-ready", "field-menu"):
        payload = _failure_payload()
        payload["role"] = role
        with pytest.raises(ValueError, match="role"):
            validate_json(payload, rail.FAILURE_SCHEMA, owner=rail.OWNER)
    for phase in reachable_phases:
        payload = _failure_payload()
        payload["phase"] = phase
        validate_json(payload, rail.FAILURE_SCHEMA, owner=rail.OWNER)
    for phase in ("natural-route", "await-battle", "battle-ready"):
        payload = _failure_payload()
        payload["phase"] = phase
        with pytest.raises(ValueError, match="phase"):
            validate_json(payload, rail.FAILURE_SCHEMA, owner=rail.OWNER)


def test_config_omits_accepted_output_and_roles_are_closed() -> None:
    fixture = load_json(rail.FIXTURE)
    config = rail._observer_config(fixture, _contract())
    rail._assert_clean_observer_config(config)
    serialized = json.dumps(config, sort_keys=True)
    for forbidden in ("expectedObservation", "chronology", "restoration", "logicalInputTrace"):
        assert forbidden not in serialized
    rail._assert_lua_role_contract()
    observer = rail.OBSERVER.read_text(encoding="utf-8")
    assert "advance_planned_inputs(map, x, y)" in observer
    assert "source-derived Map 3 input plan diverged" in observer
    assert 'if waypoint.interaction == "entity" then' in observer
    assert "entity142_face_left_seen" in observer
    assert "source-derived adjacent Left-facing lower-school interaction" in observer
    assert "facing & config.ram.DIRECTION_MASK" in observer
    assert "rawFacing=" in observer
    assert "unexpected Map3 Zone0/guard route event before Astral interaction" in observer
    assert '"player-action"' in observer
    assert '"activated-entity"' in observer
    assert '"entity-dispatch"' in observer
    assert "VIEW_TARGET_ENTITY" in observer
    assert 'route %s stalled before %s' in observer
    assert 'bounded_route_wait(waypoint, map, x, y, "zone-admission")' in observer
    assert 'bounded_route_wait(waypoint, map, x, y, "input-transition")' in observer
    assert "no source-derived input transition before %s" in observer
    assert "ROUTE_MOVE_STALL_FRAME_LIMIT = 120" in observer
    assert "ROUTE_PHASE_WATCHDOG_FRAME_LIMIT = 1200" in observer
    assert 'fail("route-phase-watchdog", nil' in observer
    assert "enforce_route_phase_watchdog()" in observer
    assert 'set_input("", "await-map-settle")' in observer
    assert "route_control_ready, wait_after_warp = false, true" in observer
    assert "local planned_input_committed = false" in observer
    assert "planned_input_committed = true" in observer
    assert 'set_input("", "movement-commit")' in observer
    assert 'set_input("", "await-move-settle")' in observer
    assert '"Map3_ZoneEvent7"' in observer
    assert '"cs_513A0"' in observer
    assert "entity142_reinit_seen" in observer
    assert "map3_init_seen" in observer
    assert "messenger_zone_admission" in observer
    assert "ZoneEvent8 raw-target admission" in observer
    assert "ZoneEvent8 admission" in observer
    assert "original Map3-init caller chronology" in observer
    assert observer.count("add_callback(config.r1.functions.selectedInitAddress") == 1
    assert "add_callback(config.functions.ms_map3_InitFunction" not in observer
    assert '"map3-init-dispatch"' in observer
    assert "elseif route_started then" in observer
    assert "Zone7=%s F602=%s F260=%s F603=%s" in observer
    assert '"cs_5148C"' in observer
    assert '"zone-admission"' in observer
    assert "natural zone admission raw-coordinate drift" in observer
    assert "MAP_EVENT_PARAM_1" in observer
    assert "MAP_EVENT_PARAM_3" in observer
    assert "memory.read_u16_be(config.ram.MAP_EVENT_PARAM_1" in observer
    assert "memory.read_u16_be(config.ram.MAP_EVENT_PARAM_3" in observer
    assert "ENTITY_INDEX_LIST" in observer
    assert "entityIndexSelector" in observer
    assert "resolvedEntity" in observer
    assert (
        "Astral zone completion flag set without complete original zone/program chronology"
        in observer
    )
    assert '"map20-init"' not in observer
    assert "automation source marker" in observer
    assert "external or stale controller input" in observer
    assert "MAP_EVENT_PARAM_2" in observer
    assert "eventDestinationMap" in observer
    assert (
        "Map 20 palace scene did not close at the source-derived WaitForEvent boundary"
        not in observer
    )
    assert 'append_trace("route", "post-warp-wait-for-event")' in observer
    assert "completed warp without matching original warp callback" in observer
    assert "completed warp has no exact final source-derived transition" in observer
    assert "unexpected original warp callback: current map=" in observer
    assert (
        "planned_input_committed = false\n        controller_move_commit_seen = false" in observer
    )
    assert "ENTITYDEF_OFFSET_XDEST" in observer
    assert "ENTITYDEF_OFFSET_YDEST" in observer
    assert "ENTITYDEF_OFFSET_LAYER" in observer
    assert '"input-controller"' in observer
    assert "playerControllerSeen" in observer
    assert '"input-controller-move-commit"' in observer
    assert "layer1=(%d,%d)-(%d,%d)" in observer
    assert "post-warp Map 3 layout drift" in observer
    assert "local destination_x, destination_y = current_destination()" in observer
    assert "currentPlayerInput" in observer
    assert "layer=%d" in observer
    assert 'fail("case-watchdog", nil, message)' in observer


def test_route_phase_watchdog_executes_production_typed_failure_path(tmp_path: Path) -> None:
    result = _run_lua_route_phase_watchdog(tmp_path)
    assert result["role"] == "route-phase-watchdog"
    assert "waypoint=map3-bowie-house-exit" in result["error"]
    assert "automationEpoch=9 marker=9" in result["error"]


def test_entity142_facing_branch_masks_raw_direction_bits(tmp_path: Path) -> None:
    result = _run_lua_entity142_masked_facing_branch(tmp_path)
    assert result == {"facingMasked": 2, "input": "C"}


def test_zone_controller_stays_neutral_until_callback_then_house_door_moves_down(
    tmp_path: Path,
) -> None:
    assert _run_lua_zone_input_neutral_then_house_door(tmp_path) == {
        "zoneInput": "",
        "nextInput": "Down",
        "nextWaypoint": "map3-bowie-house-door",
    }


def test_logical_input_trace_reports_missing_duplicate_unmodeled_edges_and_collapses_c_polling(
    tmp_path: Path,
) -> None:
    assert _run_lua_logical_input_edge_contract(tmp_path) == {
        "traceCount": 2,
        "downAfterZone7Accepted": True,
        "duplicateRejected": True,
        "missingReported": True,
        "unmodeledRejected": True,
    }


def test_zone_admissions_are_callback_accounted_without_controller_c(tmp_path: Path) -> None:
    assert _run_lua_zone_admission_accounting(tmp_path) == {
        "zoneAdmission": True,
        "trace": "zone:Map3_ZoneEvent6",
    }


def test_map3_init_alias_uses_one_physical_callback_and_typed_dispatch(tmp_path: Path) -> None:
    assert _run_lua_map3_init_physical_pc_dispatch(tmp_path) == {
        "callbackCount": 1,
        "role": "map3-init-dispatch",
    }


def test_checkpoint_waits_for_outer_core_snapshot_before_controlled_admission(
    tmp_path: Path,
) -> None:
    assert _run_lua_safe_core_snapshot_checkpoint(tmp_path) == {
        "saveCalls": 1,
        "savedBeforeAdmission": True,
        "phase": "await-r1-new-action",
    }
    assert rail.SUCCESS_MILESTONES.index(
        "milestone:r1-core-state-saved-outside-callback"
    ) < rail.SUCCESS_MILESTONES.index("milestone:r1-controlled-admission-started")


@pytest.mark.parametrize(
    ("restoration_mismatch", "unregister_fault", "expected_unregister_calls"),
    ((True, False, 2), (False, True, 3)),
)
def test_success_finalization_faults_are_typed_and_never_emit_pass_observation(
    tmp_path: Path,
    monkeypatch: pytest.MonkeyPatch,
    restoration_mismatch: bool,
    unregister_fault: bool,
    expected_unregister_calls: int,
) -> None:
    result = _run_lua_success_finalization_fault(
        tmp_path, restoration_mismatch=restoration_mismatch, unregister_fault=unregister_fault
    )
    assert {key: value for key, value in result.items() if key != "status"} == {
        "unregisterCalls": expected_unregister_calls,
        "loadCalls": 1,
        "failureCount": 1,
    }
    status_path = tmp_path / "typed-failure-status.txt"
    status_path.write_text(result["status"] + "\n", encoding="utf-8")
    monkeypatch.setattr(rail, "STATUS_PATH", status_path)
    diagnostic = rail._failure_diagnostic()
    assert diagnostic is not None
    if restoration_mismatch:
        assert diagnostic["restoration"]["sessionStateRestored"] is False
        assert diagnostic["restorationMismatch"]["domain"] == "gameFlags"
    else:
        assert diagnostic["restoration"]["sessionStateRestored"] is True
        assert diagnostic["restorationMismatch"] is None


def test_failure_status_and_exact_success_status_are_closed(
    tmp_path: Path, monkeypatch: pytest.MonkeyPatch
) -> None:
    status = tmp_path / "status.txt"
    payload = _failure_payload()
    _write_failure_status(status, payload)
    monkeypatch.setattr(rail, "STATUS_PATH", status)
    assert rail._failure_diagnostic() == payload

    action_dispatch = deepcopy(payload)
    action_dispatch["role"] = "entity-dispatch"
    _write_failure_status(status, action_dispatch)
    assert rail._failure_diagnostic() == action_dispatch

    for mutate in (
        lambda value: value.update({"role": "arbitrary-role"}),
        lambda value: value.update({"callbackCount": 1}),
        lambda value: value["restoration"].update({"sessionStateRestored": False}),
    ):
        drift = deepcopy(payload)
        mutate(drift)
        _write_failure_status(status, drift)
        with pytest.raises(ValueError):
            rail._failure_diagnostic()

    status.write_text("\n".join(rail.SUCCESS_MILESTONES) + "\n", encoding="utf-8")
    rail._assert_status()
    status.write_text("\n".join(reversed(rail.SUCCESS_MILESTONES)) + "\n", encoding="utf-8")
    with pytest.raises(RuntimeError, match="status|milestone"):
        rail._assert_status()
