from __future__ import annotations

from copy import deepcopy
from pathlib import Path
from typing import Any

from sf2tool.h3.bizhawk import run_observer, verify_runtime_contract
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path
from sf2tool.research_index import listing_symbol_addresses

FIXTURE = repo_path("tests/fixtures/h3/entity-movement-matrix-v1.json")
SCHEMA = repo_path("schemas/h3-entity-movement-matrix-fixture.schema.json")
OBSERVER = repo_path("tools/bizhawk/entity_movement_matrix_observer.lua")

RAM = {
    "entityDataAddress": 0xFFA902,
    "entityCount": 49,
    "entitySize": 32,
    "scriptAddress": 0xFF4000,
    "mapBlockDataAddress": 0xFF0000,
    "spritesToLoadAddress": 0xFFAF67,
}

FACING_TABLE = (5, 2, 6, -1, 1, -1, 3, -1, 4, 0, 7, -1, -1, -1, -1, -1)
STATE_FIELDS = (
    "tick",
    "x",
    "y",
    "xVelocity",
    "yVelocity",
    "xTravel",
    "yTravel",
    "xDest",
    "yDest",
    "facing",
    "layer",
    "flagsB",
    "animCounter",
    "waitTimer",
    "scriptOffset",
)


def _s16(value: int) -> int:
    value &= 0xFFFF
    return value - 0x10000 if value & 0x8000 else value


def _u8(value: int) -> int:
    return value & 0xFF


def _adjust_velocity(velocity: int, acceleration: int, *, increasing: bool) -> int:
    candidate = _s16(velocity + acceleration if increasing else velocity - acceleration)
    return velocity if candidate == 0 else candidate


def _snap_axis(position: int, destination: int, travel: int, old_delta: int) -> tuple[int, int]:
    new_delta = _s16(destination - position)
    crossed = ((_s16(old_delta) & 0xFFFF) ^ (new_delta & 0xFFFF)) & 0x8000
    if new_delta == 0 or crossed:
        return destination, 0
    return position, travel


def _core_tick(entity: dict[str, int], tile_word: int | None) -> None:
    x = _s16(entity["x"])
    y = _s16(entity["y"])
    x_dest = _s16(entity["xDest"])
    y_dest = _s16(entity["yDest"])
    old_dx = _s16(x_dest - x)
    old_dy = _s16(y_dest - y)
    if old_dx != 0 or old_dy != 0:
        x_acceleration = 0
        if x != x_dest:
            remaining = abs(_s16(x - x_dest))
            quarter = (entity["xTravel"] & 0xFFFF) >> 2
            three_quarters = (((entity["xTravel"] * 4) & 0xFFFF) - entity["xTravel"]) & 0xFFFF
            three_quarters >>= 2
            if entity["flagsA"] & 0x01 and remaining >= three_quarters:
                x_acceleration = entity["xAccel"] & 0xFF
            if entity["flagsA"] & 0x04 and remaining < quarter:
                x_acceleration = -(entity["xAccel"] & 0xFF)
        y_acceleration = 0
        if y != y_dest:
            remaining = abs(_s16(y - y_dest))
            quarter = (entity["yTravel"] & 0xFFFF) >> 2
            three_quarters = (((entity["yTravel"] * 4) & 0xFFFF) - entity["yTravel"]) & 0xFFFF
            three_quarters >>= 2
            if entity["flagsA"] & 0x02 and remaining >= three_quarters:
                y_acceleration = entity["yAccel"] & 0xFF
            if entity["flagsA"] & 0x08 and remaining < quarter:
                y_acceleration = -(entity["yAccel"] & 0xFF)

        if x != x_dest:
            entity["xVelocity"] = _adjust_velocity(
                entity["xVelocity"], x_acceleration, increasing=x < x_dest
            )
        if y != y_dest:
            entity["yVelocity"] = _adjust_velocity(
                entity["yVelocity"], y_acceleration, increasing=y < y_dest
            )
        if entity["xTravel"]:
            entity["x"] = _s16(x + entity["xVelocity"])
        if entity["yTravel"]:
            entity["y"] = _s16(y + entity["yVelocity"])

        x_direction = 0
        x_magnitude = 0
        if entity["xTravel"]:
            x_direction = -1 if entity["xVelocity"] < 0 else 1
            x_magnitude = abs(entity["xVelocity"])
        y_direction = 0
        y_magnitude = 0
        if entity["yTravel"]:
            y_direction = -1 if entity["yVelocity"] < 0 else 1
            y_magnitude = abs(entity["yVelocity"])
        dominance = y_magnitude - x_magnitude
        if dominance < -8:
            y_direction = 0
        if dominance > 8:
            x_direction = 0
        facing = FACING_TABLE[((x_direction + 1) << 2) + y_direction + 1]
        if facing < 0:
            facing = entity["facing"]

        if entity["animCounter"] != 0xFF:
            entity["animCounter"] = _u8(
                entity["animCounter"] + ((x_magnitude + y_magnitude) >> 5)
            )
        if entity["flagsB"] & 0x40 and facing != entity["facing"]:
            entity["facing"] = facing

        entity["x"], entity["xTravel"] = _snap_axis(
            entity["x"], x_dest, entity["xTravel"], old_dx
        )
        entity["y"], entity["yTravel"] = _snap_axis(
            entity["y"], y_dest, entity["yTravel"], old_dy
        )
        if entity["xTravel"] == 0 and entity["yTravel"] == 0 and tile_word is not None:
            masked = tile_word & 0x3C00
            if masked == 0x2000:
                entity["layer"] = 2
            if masked == 0x2400:
                entity["layer"] = 0
            if masked == 0x3400:
                entity["flagsB"] |= 0x20
            else:
                entity["flagsB"] &= ~0x20
    if 30 < entity["animCounter"] < 128:
        entity["animCounter"] = 0


def _destination_is_blocked(case: dict[str, Any], x_dest: int, y_dest: int) -> bool:
    blocker = case["blocker"]
    if blocker is None or not (case["entity"]["flagsA"] & 0x20):
        return False
    return abs(blocker["xDest"] - x_dest) + abs(blocker["yDest"] - y_dest) < 384


def _script_tick(
    case: dict[str, Any], entity: dict[str, int], script_offset: int | None
) -> int | None:
    if script_offset is None or script_offset in (4, 6):
        return script_offset
    script = case["script"]
    kind = script["kind"]
    if kind == "wait":
        if script["timer"] <= entity["waitTimer"]:
            entity["waitTimer"] = 0
            return 4
        entity["waitTimer"] = _u8(entity["waitTimer"] + 1)
        return 0
    if kind in ("relative", "absolute"):
        if kind == "relative":
            x_dest = _s16(entity["x"] + script["x"] * 384)
            y_dest = _s16(entity["y"] + script["y"] * 384)
        else:
            x_dest = _s16(script["x"] * 384)
            y_dest = _s16(script["y"] * 384)
        if _destination_is_blocked(case, x_dest, y_dest):
            return 0
        entity["xDest"] = x_dest
        entity["yDest"] = y_dest
        entity["xTravel"] = abs(_s16(x_dest - entity["x"]))
        entity["yTravel"] = abs(_s16(y_dest - entity["y"]))
        x_speed = entity["xSpeed"] if entity["xTravel"] else 0
        y_speed = entity["ySpeed"] if entity["yTravel"] else 0
        entity["xVelocity"] = x_speed if x_dest >= entity["x"] else -x_speed
        entity["yVelocity"] = y_speed if y_dest >= entity["y"] else -y_speed
        entity["waitTimer"] = 0
        return 6
    if kind != "none":
        raise ValueError(f"unsupported entity movement script kind: {kind}")
    return None


def _snapshot(tick: int, entity: dict[str, int], script_offset: int | None) -> list[int | None]:
    return [
        tick,
        entity["x"],
        entity["y"],
        entity["xVelocity"],
        entity["yVelocity"],
        entity["xTravel"],
        entity["yTravel"],
        entity["xDest"],
        entity["yDest"],
        entity["facing"],
        entity["layer"],
        entity["flagsB"],
        entity["animCounter"],
        entity["waitTimer"],
        script_offset,
    ]


def model_entity_movement_case(case: dict[str, Any]) -> dict[str, Any]:
    entity = deepcopy(case["entity"])
    script_offset = None if case["script"]["kind"] == "none" else 0
    states = []
    for tick in range(1, case["ticks"] + 1):
        _core_tick(entity, case["arrivalTileWord"])
        script_offset = _script_tick(case, entity, script_offset)
        states.append(_snapshot(tick, entity, script_offset))
    return {"id": case["id"], "states": states}


def verify_entity_movement_matrix(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 120
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner="entity movement matrix fixture")
    verify_runtime_contract(fixture, rom_path)
    listing = upstream_path.resolve(strict=True) / "build/sf2build-h1.lst"
    addresses = listing_symbol_addresses(listing.read_text(encoding="utf-8"))
    source = (
        upstream_path.resolve(strict=True)
        / "disasm/code/common/scripting/entity/entityscriptengine_2.asm"
    ).read_text(encoding="utf-8")
    for fragment in (
        "UpdateEntityData:",
        "bsr.w   HasSameDestinationAsOtherEntity",
        "cmpi.w  #$FFF8,d3",
        "bsr.w   ConvertMapPixelCoordinatesToOffset",
        "cmpi.b  #$1E,ENTITYDEF_OFFSET_ANIMCOUNTER(a0)",
    ):
        if fragment not in source:
            raise ValueError(f"entity movement runtime source contract drift: {fragment}")
    function = {
        "updateAddress": addresses["UpdateEntityData"],
        "nextEntityAddress": addresses["UpdateNextEntity"],
        "convertReturnAddress": addresses["UpdateEntityData"] + 0x1D4,
        "waitForEventAddress": addresses["WaitForEvent"],
    }
    if (
        function["convertReturnAddress"] != 0x5F40
        or addresses["ConvertMapPixelCoordinatesToOffset"] != 0x61FC
    ):
        raise ValueError("entity movement arrival call-site contract drift")
    if fixture["function"] != function or fixture["ram"] != RAM:
        raise ValueError("entity movement function/RAM contract drift")
    if tuple(fixture["stateFields"]) != STATE_FIELDS:
        raise ValueError("entity movement state-vector contract drift")
    modeled = [model_entity_movement_case(case) for case in fixture["cases"]]
    for case, expected in zip(fixture["cases"], modeled, strict=True):
        if case["expected"] != expected:
            raise ValueError(f"entity movement golden disagrees with model: {case['id']}")

    observed = run_observer(
        rom_path=rom_path,
        observer_path=OBSERVER,
        config={
            "fixtureId": fixture["id"],
            "mapTestIndex": fixture["mapTestIndex"],
            "function": fixture["function"],
            "ram": fixture["ram"],
            "stateFields": fixture["stateFields"],
            "harness": load_json(repo_path(fixture["sharedHarnessFixture"]))["harness"],
            "cases": fixture["cases"],
        },
        output_name="entity-movement-matrix",
        timeout_seconds=timeout_seconds,
    )
    expected = {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["id"],
        "mapTest": fixture["mapTestIndex"],
        "stateFields": fixture["stateFields"],
        "records": modeled,
    }
    if observed != expected:
        raise ValueError(
            "entity movement runtime matrix mismatch\n"
            f"expected={expected!r}\nobserved={observed!r}"
        )
    return {
        "Fixture": fixture["id"],
        "Cases": len(modeled),
        "Ticks": sum(len(row["states"]) for row in modeled),
        "BizHawkLaunches": 1,
        "Status": "PASS",
    }
