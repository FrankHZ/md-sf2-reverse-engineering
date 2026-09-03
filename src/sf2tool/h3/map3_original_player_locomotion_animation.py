"""Controlled Map 3 player locomotion/animation runtime contract."""

from __future__ import annotations

import json
import shutil
import tempfile
from pathlib import Path
from typing import Any

from sf2tool.h3.bizhawk import (
    bizhawk_contract,
    run_observer,
    validate_lua_syntax,
    verify_runtime_contract,
)
from sf2tool.h3.map3_admitted_start import (
    CANONICAL_ROM_SHA256,
    UPSTREAM_COMMIT,
    _h1_bytes,
    build_map3_admitted_start_source_contract,
)
from sf2tool.h3.observer_status import (
    assert_observer_status,
    callback_failure_status,
    observer_failure_contract,
)
from sf2tool.h3.witch_save_actions import _equates
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.rom import inspect_rom

OWNER = "map3-original-player-locomotion-animation"
ID = "sf2-map3-original-player-locomotion-animation-runtime-v1"
FIXTURE = repo_path("tests/fixtures/h3/map3-original-player-locomotion-animation-runtime-v1.json")
FIXTURE_SCHEMA = repo_path(
    "schemas/h3/map3-original-player-locomotion-animation-fixture.schema.json"
)
OBSERVATION_SCHEMA = repo_path(
    "schemas/h3/map3-original-player-locomotion-animation-observation.schema.json"
)
FAILURE_SCHEMA = repo_path(
    "schemas/h3/map3-original-player-locomotion-animation-callback-failure.schema.json"
)
OBSERVER = repo_path("tools/bizhawk/map3_original_player_locomotion_animation_observer.lua")
INDEX = repo_path("manifests/research-index.json")
STATIC_REFERENCE = repo_path(
    "tests/fixtures/h2/map3-original-player-reference-frame-static-v1.json"
)
OBSERVED_OUTPUT = repo_path(f"local/derived/h3/{OWNER}.observed.json")
STATUS_PATH = repo_path(f"local/derived/h3/{OWNER}.status.txt")

CASE_IDS = (
    "attempt-up",
    "attempt-left",
    "attempt-right",
    "attempt-down",
)

SUCCESS_MILESTONES = (
    "milestone:observer-started",
    "milestone:scope-snapshotted-before-write",
    "milestone:core-state-saved-outside-callback",
    "milestone:controlled-new-admission-started",
    "milestone:first-wait-for-event-observed",
    "milestone:admission-state-saved-outside-callback",
    *(f"milestone:case-finished:{case_id}" for case_id in CASE_IDS),
    "milestone:callbacks-cleared:0",
    "milestone:observer-finished",
)

REQUIRED_LUA_ROLES = frozenset(
    {
        "bootstrap-check-sram",
        "checkpoint",
        "witch-new-action",
        "wait-for-event",
        "vint-update-entities",
        "update-entity-return",
        "control-character",
        "next-entity",
        "sprite-half-0",
        "sprite-half-1",
        "sprite-counter-after",
        "bootstrap-watchdog",
        "case-watchdog",
    }
)

INDEX_BINDINGS = {
    "gameflow.exploration.loop": {("wait-for-event", "sourceContext.functions.waitForEvent")},
    "scripting.entity.entityscriptengine-1": {
        ("entry", "sourceContext.functions.vintUpdateSprites"),
        ("half-0", "sourceContext.functions.spriteHalf0"),
        ("half-1", "sourceContext.functions.spriteHalf1"),
        ("counter-after", "sourceContext.functions.spriteCounterAfter"),
    },
    "entity.actions.update-core": {
        ("entry", "sourceContext.functions.updateEntityData"),
        ("return", "sourceContext.functions.updateEntityDataReturn"),
        ("entity-data", "sourceContext.ram.ENTITY_DATA"),
    },
}


def _unique_opcode(
    rom: bytes,
    listing: str,
    *,
    start: int,
    end: int,
    opcode: bytes,
    purpose: str,
) -> int:
    matches = [
        address
        for address in range(start, end - len(opcode) + 1, 2)
        if rom[address : address + len(opcode)] == opcode
    ]
    if len(matches) != 1:
        raise ValueError(f"Map 3 locomotion {purpose} opcode drift: {matches!r}")
    address = matches[0]
    if _h1_bytes(listing, address, len(opcode)) != opcode.hex().upper():
        raise ValueError(f"Map 3 locomotion {purpose} H1 listing drift")
    return address


def _require_order(text: str, fragments: tuple[str, ...], purpose: str) -> None:
    cursor = 0
    compact = "\n".join(line.strip() for line in text.splitlines())
    for fragment in fragments:
        index = compact.find(fragment, cursor)
        if index < 0:
            raise ValueError(f"Map 3 locomotion {purpose} source order drift: {fragment}")
        cursor = index + len(fragment)


def build_source_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    """Join admitted Map 3 setup to the exact movement and sprite branch seams."""
    admitted = build_map3_admitted_start_source_contract(rom_path, upstream_path)
    upstream = upstream_path.resolve(strict=True)
    disasm = upstream / "disasm"
    listing = (upstream / "build/sf2build-h1.lst").read_text(encoding="utf-8")
    addresses = listing_symbol_addresses(listing)
    rom = rom_path.resolve(strict=True).read_bytes()
    required = {
        "WaitForEvent",
        "VInt_UpdateEntities",
        "UpdateEntityData",
        "return_5F9A",
        "esc02_controlCharacter",
        "esc_goToNextEntity",
        "VInt_UpdateSprites",
        "table_4E16",
    }
    missing = required - addresses.keys()
    if missing:
        raise ValueError(f"Map 3 locomotion H1 symbol drift: {sorted(missing)!r}")

    engine1 = (disasm / "code/common/scripting/entity/entityscriptengine_1.asm").read_text(
        encoding="utf-8"
    )
    engine2 = (disasm / "code/common/scripting/entity/entityscriptengine_2.asm").read_text(
        encoding="utf-8"
    )
    _require_order(
        engine1,
        (
            "VInt_UpdateSprites:",
            "move.b  ENTITYDEF_OFFSET_ANIMCOUNTER(a0),d4",
            "cmpi.b  #15,d4",
            "move.w  #VDPTILE_ENTITIES_FRAME_1_START,d5",
            "move.w  #VDPTILE_ENTITIES_FRAME_2_START,d5",
            "cmpi.b  #-1,d4",
            "addq.b  #1,d4",
            "cmpi.b  #30,d4",
            "clr.w   d4",
            "move.b  d4,ENTITYDEF_OFFSET_ANIMCOUNTER(a0)",
        ),
        "sprite half/counter",
    )
    _require_order(
        engine2,
        (
            "VInt_UpdateEntities:",
            "bsr.w   UpdateEntityData",
            "esc02_controlCharacter:",
            "move.b  ((CURRENT_PLAYER_INPUT-$1000000)).w,currentPlayerInput(a6)",
            "cmpi.w  #$C000,(a4,d2.w)",
            "move.w  d2,ENTITYDEF_OFFSET_XVELOCITY(a0)",
            "move.w  d3,ENTITYDEF_OFFSET_YVELOCITY(a0)",
            "bsr.w   UpdateEntitySprite",
            "UpdateEntityData:",
            "add.w   d4,(a0)",
            "add.w   d5,ENTITYDEF_OFFSET_Y(a0)",
            "lsr.w   #5,d0",
            "add.b   d0,ENTITYDEF_OFFSET_ANIMCOUNTER(a0)",
        ),
        "movement/input/counter",
    )

    sprite_start = addresses["VInt_UpdateSprites"]
    sprite_end = addresses["table_4E16"]
    functions = {
        "waitForEvent": addresses["WaitForEvent"],
        "vintUpdateEntities": addresses["VInt_UpdateEntities"],
        "updateEntityData": addresses["UpdateEntityData"],
        "updateEntityDataReturn": addresses["return_5F9A"],
        "controlCharacter": addresses["esc02_controlCharacter"],
        "nextEntity": addresses["esc_goToNextEntity"],
        "vintUpdateSprites": sprite_start,
        "spriteHalf0": _unique_opcode(
            rom,
            listing,
            start=sprite_start,
            end=sprite_end,
            opcode=bytes.fromhex("3A3C0380"),
            purpose="half-0",
        ),
        "spriteHalf1": _unique_opcode(
            rom,
            listing,
            start=sprite_start,
            end=sprite_end,
            opcode=bytes.fromhex("3A3C0389"),
            purpose="half-1",
        ),
        "spriteCounterAfter": _unique_opcode(
            rom,
            listing,
            start=sprite_start,
            end=sprite_end,
            opcode=bytes.fromhex("3F06"),
            purpose="counter-after",
        ),
    }
    constants = _equates(
        (disasm / "sf2const.asm").read_text(encoding="utf-8"),
        ("CURRENT_PLAYER_INPUT",),
    )
    offsets = _equates(
        (disasm / "sf2enums.asm").read_text(encoding="utf-8"),
        (
            "ENTITYDEF_OFFSET_X",
            "ENTITYDEF_OFFSET_Y",
            "ENTITYDEF_OFFSET_XVELOCITY",
            "ENTITYDEF_OFFSET_YVELOCITY",
            "ENTITYDEF_OFFSET_XTRAVEL",
            "ENTITYDEF_OFFSET_YTRAVEL",
            "ENTITYDEF_OFFSET_XDEST",
            "ENTITYDEF_OFFSET_YDEST",
            "ENTITYDEF_OFFSET_FACING",
            "ENTITYDEF_OFFSET_ANIMCOUNTER",
            "ENTITYDEF_SIZE",
        ),
    )
    return {
        "admission": admitted,
        "functions": functions,
        "ram": {**admitted["ram"], **constants, **offsets},
        "facts": {
            "mapTileSize": 384,
            "halfThreshold": 15,
            "counterResetAbove": 30,
            "movementCounterVelocityShift": 5,
            "spriteCounterIncrement": 1,
            "updateOrder": [
                "movement-state-and-counter",
                "control-input-attempt",
                "sprite-half-selection",
                "sprite-counter-increment",
            ],
        },
    }


def _static_join() -> dict[str, Any]:
    static = load_json(STATIC_REFERENCE)
    if static["id"] != "sf2-map3-original-player-reference-frame-static-v1":
        raise ValueError("Map 3 locomotion static reference owner drift")
    return {
        "fixture": STATIC_REFERENCE.relative_to(repo_path(".")).as_posix(),
        "fixtureId": static["id"],
        "directionRules": static["static"]["directionSelection"]["rules"],
        "supersededPolicyOnly": static["static"]["framePolicy"],
    }


def _source_context(contract: dict[str, Any]) -> dict[str, Any]:
    return {
        "functions": contract["functions"],
        "ram": {"ENTITY_DATA": contract["ram"]["ENTITY_DATA"]},
    }


def _static_projection(contract: dict[str, Any]) -> dict[str, Any]:
    return {
        "facts": contract["facts"],
        "staticFacingJoin": _static_join(),
        "admittedStartFixtureId": "sf2-map3-admitted-start-runtime-v1",
        "entityMovementFixtureId": "sf2-entity-movement-runtime-v1",
    }


def _assert_fixture(fixture: dict[str, Any], contract: dict[str, Any]) -> None:
    if fixture["id"] != ID or fixture["caseOrder"] != list(CASE_IDS):
        raise ValueError("Map 3 locomotion fixture identity/case order drift")
    if [case["caseId"] for case in fixture["cases"]] != list(CASE_IDS):
        raise ValueError("Map 3 locomotion case matrix drift")
    expected_cases = [
        {"caseId": "attempt-up", "direction": "UP", "joypadName": "Up", "facing": 1},
        {"caseId": "attempt-left", "direction": "LEFT", "joypadName": "Left", "facing": 2},
        {"caseId": "attempt-right", "direction": "RIGHT", "joypadName": "Right", "facing": 0},
        {"caseId": "attempt-down", "direction": "DOWN", "joypadName": "Down", "facing": 3},
    ]
    if fixture["cases"] != expected_cases:
        raise ValueError("Map 3 locomotion controlled input matrix drift")
    if fixture["static"] != _static_projection(contract):
        raise ValueError("Map 3 locomotion static owner join drift")
    if fixture["sourceContext"] != _source_context(contract):
        raise ValueError("Map 3 locomotion source context drift")
    provenance = fixture["privateProvenance"]
    if provenance != {
        "romSha256": CANONICAL_ROM_SHA256,
        "upstreamCommit": UPSTREAM_COMMIT,
    }:
        raise ValueError("Map 3 locomotion private provenance drift")
    if fixture["expectedObservation"]["caseOrder"] != list(CASE_IDS):
        raise ValueError("Map 3 locomotion expected case order drift")


def _assert_expected_matrix(fixture: dict[str, Any]) -> None:
    observation = fixture["expectedObservation"]
    admission = observation["admission"]
    if admission["boundary"] != "first-original-WaitForEvent-entry":
        raise ValueError("Map 3 locomotion admission boundary drift")
    admission_sprite = admission["sprite"]
    if (
        admission_sprite["counterAtSelection"] != 25
        or admission_sprite["counterAfter"] != admission["entity"]["animCounter"]
        or admission_sprite["selectedHalf"] != 1
    ):
        raise ValueError("Map 3 locomotion admitted visible half/counter drift")

    records = observation["records"]
    if [record["caseId"] for record in records] != list(CASE_IDS):
        raise ValueError("Map 3 locomotion expected record order drift")
    directions = {row["direction"]: row for row in _static_join()["directionRules"]}
    expected_outcomes = {
        "attempt-up": ("blocked-no-movement", False, 1, None, 0),
        "attempt-left": ("moved-one-tile", True, 4, "x", -32),
        "attempt-right": ("blocked-no-movement", False, 8, None, 0),
        "attempt-down": ("moved-one-tile", True, 2, "y", 32),
    }
    outcomes = []
    for case, record in zip(fixture["cases"], records, strict=True):
        outcome, motion_installed, player_input, axis, velocity = expected_outcomes[case["caseId"]]
        if (
            record["direction"] != case["direction"]
            or record["settled"]["facing"] != case["facing"]
        ):
            raise ValueError("Map 3 locomotion facing outcome drift")
        if (
            record["outcome"] != outcome
            or record["motionInstalled"] is not motion_installed
            or not record["inputObserved"]
            or record["inputTick"] != 1
            or record["seed"] != admission["entity"]
        ):
            raise ValueError("Map 3 locomotion controlled outcome drift")
        if record["staticSelection"] != directions[case["direction"]]:
            raise ValueError("Map 3 locomotion static facing selection join drift")
        expected_tick_count = 13 if motion_installed else 1
        if len(record["ticks"]) != expected_tick_count:
            raise ValueError("Map 3 locomotion per-VInt sequence length drift")
        previous_final = None
        for expected_tick, tick in enumerate(record["ticks"], start=1):
            if tick["tick"] != expected_tick:
                raise ValueError("Map 3 locomotion tick chronology drift")
            if expected_tick == 1:
                if tick["beforeEntities"] != record["seed"]:
                    raise ValueError("Map 3 locomotion replay seed ordering drift")
            elif tick["beforeEntities"] != previous_final:
                raise ValueError("Map 3 locomotion cross-VInt state continuity drift")

            input_attempt = tick["inputAttempt"]
            if input_attempt is not None and input_attempt["before"] != tick["afterMovement"]:
                raise ValueError("Map 3 locomotion movement-before-input ordering drift")
            counter = tick["sprite"]["counterAtSelection"]
            selected = tick["sprite"]["selectedHalf"]
            counter_after = tick["sprite"]["counterAfter"]
            if counter != tick["afterMovement"]["animCounter"]:
                raise ValueError("Map 3 locomotion counter-before-sprite ordering drift")
            if selected != (0 if counter < 15 else 1):
                raise ValueError("Map 3 locomotion exact sprite-half branch drift")
            if counter_after != (0 if counter + 1 > 30 else counter + 1):
                raise ValueError("Map 3 locomotion sprite-counter increment/reset drift")

            final_before_sprite = (
                input_attempt["after"] if input_attempt is not None else tick["afterMovement"]
            )
            previous_final = {**final_before_sprite, "animCounter": counter_after}

        first_tick = record["ticks"][0]
        first_input = first_tick["inputAttempt"]
        if (
            first_tick["afterMovement"] != record["seed"]
            or first_input is None
            or first_input["playerInput"] != player_input
            or first_input["before"] != record["seed"]
            or first_input["after"]["facing"] != case["facing"]
        ):
            raise ValueError("Map 3 locomotion first-attempt ordering drift")

        if motion_installed:
            _assert_success_sequence(record, axis=axis, velocity=velocity)
        else:
            expected_after = {**record["seed"], "facing": case["facing"]}
            if first_input["after"] != expected_after:
                raise ValueError("Map 3 locomotion blocked attempt mutated motion state")
        if record["settled"] != previous_final:
            raise ValueError("Map 3 locomotion settled facing/half state drift")
        outcomes.append(record["outcome"])
    if "moved-one-tile" not in outcomes or "blocked-no-movement" not in outcomes:
        raise ValueError("Map 3 locomotion matrix must retain success and blocked outcomes")
    restoration = observation["restoration"]
    if (
        not observation["callbacksCleared"]
        or restoration["outputRemoved"]
        or not all(value for key, value in restoration.items() if key != "outputRemoved")
    ):
        raise ValueError("Map 3 locomotion cleanup/restoration drift")


def _assert_success_sequence(record: dict[str, Any], *, axis: str | None, velocity: int) -> None:
    if axis not in {"x", "y"}:
        raise ValueError("Map 3 locomotion success axis drift")
    other_axis = "y" if axis == "x" else "x"
    seed = record["seed"]
    first_after = record["ticks"][0]["inputAttempt"]["after"]
    destination = seed[axis] + 384 * (1 if velocity > 0 else -1)
    expected_install = {
        **seed,
        "facing": record["settled"]["facing"],
        f"{axis}Dest": destination,
        f"{axis}Travel": 384,
        f"{axis}Velocity": velocity,
    }
    if first_after != expected_install:
        raise ValueError("Map 3 locomotion success motion-install ordering drift")

    for tick_number, tick in enumerate(record["ticks"][1:], start=2):
        before = tick["beforeEntities"]
        after = tick["afterMovement"]
        expected_after = {
            **before,
            axis: seed[axis] + velocity * (tick_number - 1),
            "animCounter": 0 if before["animCounter"] + 1 > 30 else before["animCounter"] + 1,
        }
        if tick_number == 13:
            expected_after[f"{axis}Travel"] = 0
        if after != expected_after:
            raise ValueError("Map 3 locomotion successful per-VInt movement drift")
        if (
            after[f"{axis}Dest"] != destination
            or after[f"{axis}Velocity"] != velocity
            or after[other_axis] != seed[other_axis]
        ):
            raise ValueError("Map 3 locomotion successful axis state drift")
        expected_input = 0 if tick_number == 13 else None
        if expected_input is None:
            if tick["inputAttempt"] is not None:
                raise ValueError("Map 3 locomotion unexpected mid-traversal input seam")
        elif (
            tick["inputAttempt"] is None
            or tick["inputAttempt"]["playerInput"] != expected_input
            or tick["inputAttempt"]["before"] != after
            or tick["inputAttempt"]["after"] != after
        ):
            raise ValueError("Map 3 locomotion arrival input/settling drift")


def _assert_index_bindings() -> None:
    fixture_path = FIXTURE.relative_to(repo_path(".")).as_posix()
    actual: dict[str, set[tuple[str, str]]] = {}
    for record in load_json(INDEX)["records"]:
        bindings = {
            (binding["addressId"], binding["fixtureField"])
            for evidence in record["evidence"]
            if evidence["fixture"] == fixture_path and evidence["fixtureId"] == ID
            for binding in evidence["bindings"]
        }
        if bindings:
            actual[record["id"]] = bindings
    if actual != INDEX_BINDINGS:
        raise ValueError("Map 3 locomotion research-index binding drift")


def _assert_lua_roles() -> None:
    source = OBSERVER.read_text(encoding="utf-8")
    missing = sorted(role for role in REQUIRED_LUA_ROLES if f'"{role}"' not in source)
    if missing:
        raise ValueError(f"Map 3 locomotion Lua role drift: {missing!r}")


def _observer_config(fixture: dict[str, Any], contract: dict[str, Any]) -> dict[str, Any]:
    admitted = contract["admission"]
    return {
        "fixtureId": fixture["id"],
        "core": fixture["emulator"]["core"],
        "jsonModulePath": OBSERVER.with_name("json.lua").as_posix(),
        "caseOrder": fixture["caseOrder"],
        "cases": fixture["cases"],
        "functions": {**admitted["function"], **contract["functions"]},
        "ram": contract["ram"],
        "witchNewAction": admitted["witchNewAction"],
        "map3": admitted["map3"],
        "harness": admitted["harness"],
        "sessionPatches": admitted["sessionPatches"],
        "observerFailureContract": observer_failure_contract(OWNER),
    }


def _assert_clean_config(config: dict[str, Any]) -> None:
    forbidden = {"expectedObservation", "records", "ticks", "outcome", "staticFacingJoin"}

    def visit(value: Any) -> None:
        if isinstance(value, dict):
            overlap = forbidden & value.keys()
            if overlap:
                raise ValueError(
                    f"Map 3 locomotion observer config leaks golden data: {sorted(overlap)!r}"
                )
            for child in value.values():
                visit(child)
        elif isinstance(value, list):
            for child in value:
                visit(child)

    visit(config)


def _assert_status() -> None:
    assert_observer_status(
        STATUS_PATH,
        owner=OWNER,
        schema_path=FAILURE_SCHEMA,
        required_milestones=SUCCESS_MILESTONES,
    )
    lines = STATUS_PATH.read_text(encoding="utf-8").splitlines()
    if tuple(lines) != SUCCESS_MILESTONES:
        raise RuntimeError("Map 3 locomotion success status sequence drift")


def _failure_diagnostic() -> dict[str, Any] | None:
    payload = callback_failure_status(STATUS_PATH, owner=OWNER, schema_path=FAILURE_SCHEMA)
    if payload is None:
        return None
    if payload["caseId"] not in {"bootstrap", "admission", *CASE_IDS}:
        raise ValueError("Map 3 locomotion callback failure case drift")
    if payload["callbackCount"] != 0 or not payload["callbacksCleared"]:
        raise ValueError("Map 3 locomotion residual callback drift")
    if not payload["outputRemoved"] or not payload["restoration"]["outputRemoved"]:
        raise ValueError("Map 3 locomotion failure output cleanup drift")
    return payload


def preflight_map3_original_player_locomotion_animation(
    rom_path: Path, upstream_path: Path
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner="Map 3 locomotion fixture")
    _assert_lua_roles()
    _, executable = bizhawk_contract()
    validate_lua_syntax(OBSERVER, executable)
    contract = build_source_contract(rom_path, upstream_path)
    _assert_fixture(fixture, contract)
    _assert_expected_matrix(fixture)
    _assert_index_bindings()
    _assert_clean_config(_observer_config(fixture, contract))
    return {"Fixture": ID, "Cases": len(CASE_IDS), "Status": "PRELAUNCH-PASS"}


def verify_map3_original_player_locomotion_animation(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 240
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner="Map 3 locomotion fixture")
    verify_runtime_contract(fixture, rom_path)
    contract = build_source_contract(rom_path, upstream_path)
    _assert_fixture(fixture, contract)
    _assert_expected_matrix(fixture)
    _assert_index_bindings()
    _assert_lua_roles()
    config = _observer_config(fixture, contract)
    _assert_clean_config(config)
    canonical_before = inspect_rom(rom_path.resolve(strict=True))["sha256"]
    session_deleted = False
    try:
        with tempfile.TemporaryDirectory(prefix="sf2-map3-player-locomotion-") as temporary:
            session = Path(temporary) / "map3-player-locomotion-session.bin"
            shutil.copy2(rom_path, session)
            observed = run_observer(
                rom_path=session,
                observer_path=OBSERVER,
                config=config,
                output_name=OWNER,
                timeout_seconds=timeout_seconds,
            )
            _assert_status()
        session_deleted = not session.exists()
        observed["restoration"]["sessionRomDeleted"] = session_deleted
        directions = {row["direction"]: row for row in _static_join()["directionRules"]}
        for record in observed["records"]:
            record["staticSelection"] = directions[record["direction"]]
        validate_json(observed, OBSERVATION_SCHEMA, owner="Map 3 locomotion observation")
        if observed != fixture["expectedObservation"]:
            raise ValueError(
                "Map 3 locomotion runtime matrix mismatch\n"
                f"expected={fixture['expectedObservation']!r}\nobserved={observed!r}"
            )
        _assert_expected_matrix({**fixture, "expectedObservation": observed})
        OBSERVED_OUTPUT.write_text(json.dumps(observed, indent=2) + "\n", encoding="utf-8")
    except Exception:
        OBSERVED_OUTPUT.unlink(missing_ok=True)
        _failure_diagnostic()
        raise
    if inspect_rom(rom_path.resolve(strict=True))["sha256"] != canonical_before:
        raise ValueError("Map 3 locomotion canonical ROM changed during session run")
    return {
        "Fixture": ID,
        "Cases": len(CASE_IDS),
        "BizHawkLaunches": 1,
        "SessionRomDeleted": session_deleted,
        "Status": "PASS",
    }
