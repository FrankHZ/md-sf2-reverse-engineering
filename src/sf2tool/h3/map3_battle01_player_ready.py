"""Bounded H3 observation of Battle 01's first stable player-input seam.

The runtime prefix is the accepted R1/R2/R2a observation.  A declared harness
bridge then installs the accepted static R2b terminal before original code
performs the retained R2c route, battle admission, initialization, turn-order
consumption, and player-control dispatch.
"""

from __future__ import annotations

import json
import shutil
import subprocess
import tempfile
from copy import deepcopy
from hashlib import sha256
from pathlib import Path
from typing import Any

from sf2tool.h3.bizhawk import (
    bizhawk_contract,
    run_observer,
    validate_lua_syntax,
    verify_runtime_contract,
)
from sf2tool.h3.observer_status import (
    assert_observer_status,
    callback_failure_status,
    observer_failure_contract,
)
from sf2tool.h3.witch_save_actions import _equates
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path
from sf2tool.rom import inspect_rom

OWNER = "map3-battle01-player-ready"
FIXTURE_ID = "sf2-map3-battle01-player-ready-runtime-v1"
ID = FIXTURE_ID
FIXTURE = repo_path("tests/fixtures/h3/map3-battle01-player-ready-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h3/map3-battle01-player-ready-fixture.schema.json")
OBSERVATION_SCHEMA = repo_path("schemas/h3/map3-battle01-player-ready-observation.schema.json")
FAILURE_SCHEMA = repo_path("schemas/h3/map3-battle01-player-ready-callback-failure.schema.json")
OBSERVER = repo_path("tools/bizhawk/map3_messenger_acceptance_observer.lua")
OBSERVED_OUTPUT = repo_path(f"local/derived/h3/{OWNER}.observed.json")
STATUS_PATH = repo_path(f"local/derived/h3/{OWNER}.status.txt")
UPSTREAM = repo_path("local/upstream/SF2DISASM")
CANONICAL_ROM_SHA256 = "9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9"
UPSTREAM_COMMIT = "c834c652b6862bc5679fd7f69a38a7093206efc6"
CASE_IDS = ("bridged-r2b-to-first-battle01-player-ready",)
_INDEX_RECORD_ID = "battle.functions.player-input"
_INDEX_DOCUMENT = "docs/research/map3-battle01-admission.md"
_INDEX_ADDRESS = {
    "id": "stable-input-seam",
    "space": "rom",
    "kind": "observation",
    "value": 142960,
    "description": (
        "ControlBattleEntity after WaitForVInt and before the first controller-state read"
    ),
}
_INDEX_EVIDENCE = {
    "level": "H3",
    "fixture": "tests/fixtures/h3/map3-battle01-player-ready-v1.json",
    "fixtureId": FIXTURE_ID,
    "verifier": "src/sf2tool/h3/map3_battle01_player_ready.py",
    "bindings": [
        {
            "addressId": "entry",
            "fixtureField": "static.functions.ProcessBattleEntityControlPlayerInput",
        },
        {"addressId": "stable-input-seam", "fixtureField": "static.functions.playerReadyPc"},
    ],
}
_PREDECESSOR_INDEX_SHA256 = "49B80B7692154290D234A270DFB289AD158AFC96F1EDDE57270C981B438E176A"
EXPECTED_CASES = (
    {
        "caseId": CASE_IDS[0],
        "injectedInitialMenuReturn": 1,
        "injectedDifficultyMenuReturn": 0,
        "promptDefaultReturn": 0,
        "frameBudget": 90000,
    },
)

R1 = (
    repo_path("tests/fixtures/h3/map3-admitted-start-v1.json"),
    repo_path("schemas/h3/map3-admitted-start-fixture.schema.json"),
    "sf2-map3-admitted-start-runtime-v1",
)
R2 = (
    repo_path("tests/fixtures/h3/map3-battle01-natural-route-v1.json"),
    repo_path("schemas/h3/map3-battle01-natural-route-fixture.schema.json"),
    "sf2-map3-battle01-natural-route-runtime-v1",
)
R2A = (
    repo_path("tests/fixtures/h3/map3-messenger-acceptance-v1.json"),
    repo_path("schemas/h3/map3-messenger-acceptance-fixture.schema.json"),
    "sf2-map3-messenger-acceptance-runtime-v1",
)
R2B = (
    repo_path("tests/fixtures/h2/map3-castle-battle-unlock-static-v1.json"),
    repo_path("schemas/h2/map3-castle-battle-unlock-static-fixture.schema.json"),
    "sf2-map3-castle-battle-unlock-static-v1",
)
R2C = (
    repo_path("tests/fixtures/h2/map3-battle01-admission-static-v1.json"),
    repo_path("schemas/h2/map3-battle01-admission-static-fixture.schema.json"),
    "sf2-map3-battle01-admission-static-v1",
)
R3A = (
    repo_path("tests/fixtures/h2/map3-battle01-turn-control-static-v1.json"),
    repo_path("schemas/h2/map3-battle01-turn-control-static-fixture.schema.json"),
    "sf2-map3-battle01-turn-control-static-v1",
)
RETAINED = (R1, R2, R2A, R2B, R2C, R3A)

SOURCE_PATHS = (
    Path("sf2const.asm"),
    Path("sf2enums.asm"),
    Path("code/gameflow/mainloop.asm"),
    Path("code/gameflow/exploration/explorationfunctions_2.asm"),
    Path("code/gameflow/battle/battleloop_1.asm"),
    Path("code/gameflow/battle/battleloop/turnorderfunctions.asm"),
    Path("code/gameflow/battle/battlefunctions/battlefunctions_0.asm"),
    Path("code/gameflow/battle/battlefunctions/battlefunctions_2.asm"),
    Path("code/gameflow/battle/battlefunctions/executeindividualturn.asm"),
    Path("code/common/scripting/map/mapscriptengine_1.asm"),
    Path("code/common/scripting/map/ms_empty.asm"),
    Path("data/battles/entries/battle01/cs_beforebattle.asm"),
)

SUCCESS_MILESTONES = (
    "milestone:observer-started",
    "milestone:r1-scope-snapshotted-before-write",
    "milestone:r1-core-state-saved-outside-callback",
    "milestone:r1-controlled-admission-started",
    "milestone:r1-first-wait-for-event-observed",
    "milestone:natural-route-input-started",
    "milestone:messenger-body-started",
    "milestone:messenger-prompt-accepted",
    "milestone:messenger-followers-ready",
    "milestone:r2b-terminal-bridge-requested",
    "milestone:r2b-terminal-bridge-event-injected",
    "milestone:r2b-terminal-bridge-event-dispatched",
    "milestone:r2b-terminal-bridge-warp-handler-entered",
    "milestone:r2b-terminal-bridge-seeded",
    "milestone:r2c-natural-extension-complete",
    "milestone:battle01-loop-entered",
    "milestone:battle01-turn-order-entered",
    "milestone:battle01-player-control-entered",
    "milestone:battle01-player-ready",
    "milestone:callbacks-cleared:0",
    "milestone:observer-finished",
)


def _canonical(value: Any) -> bytes:
    return json.dumps(value, sort_keys=True, separators=(",", ":")).encode("utf-8")


def _index_digest(value: dict[str, Any]) -> str:
    payload = json.dumps(
        value, ensure_ascii=False, sort_keys=True, separators=(",", ":")
    ).encode("utf-8") + b"\n"
    return sha256(payload).hexdigest().upper()


def _remove_map3_battle01_player_ready_later_owner_index_delta(
    index: dict[str, Any],
) -> dict[str, Any]:
    """Remove exactly this H3 owner's address, evidence, and document appends."""
    normalized = deepcopy(index)
    records = normalized.get("records")
    if not isinstance(records, list) or any(not isinstance(row, dict) for row in records):
        raise ValueError("player-ready research-index record shape drift")
    matches = [row for row in records if row.get("id") == _INDEX_RECORD_ID]
    if len(matches) != 1:
        raise ValueError("player-ready research-index record identity drift")
    marker_records = [
        row
        for row in records
        if any(
            isinstance(item, dict)
            and (
                item.get("fixtureId") == FIXTURE_ID
                or item.get("fixture") == _INDEX_EVIDENCE["fixture"]
                or item.get("verifier") == _INDEX_EVIDENCE["verifier"]
            )
            for item in row.get("evidence", [])
        )
    ]
    if marker_records != matches:
        raise ValueError("player-ready research-index evidence owner drift")
    record = matches[0]
    addresses = record.get("addresses")
    evidence = record.get("evidence")
    documents = record.get("documents")
    if not all(isinstance(value, list) for value in (addresses, evidence, documents)):
        raise ValueError("player-ready research-index field shape drift")
    if addresses.count(_INDEX_ADDRESS) != 1 or addresses[-1] != _INDEX_ADDRESS:
        raise ValueError("player-ready research-index address drift")
    if evidence.count(_INDEX_EVIDENCE) != 1 or evidence[-1] != _INDEX_EVIDENCE:
        raise ValueError("player-ready research-index evidence drift")
    if documents.count(_INDEX_DOCUMENT) != 1 or documents[-1] != _INDEX_DOCUMENT:
        raise ValueError("player-ready research-index document drift")
    addresses.pop()
    evidence.pop()
    documents.pop()
    if _index_digest(normalized) != _PREDECESSOR_INDEX_SHA256:
        raise ValueError("player-ready predecessor research-index drift")
    return normalized


def _difference(expected: Any, actual: Any, path: str = "$") -> str | None:
    if type(expected) is not type(actual):
        return f"{path}: type drift"
    if isinstance(expected, dict):
        if expected.keys() != actual.keys():
            return f"{path}: key drift"
        for key in expected:
            if result := _difference(expected[key], actual[key], f"{path}.{key}"):
                return result
    elif isinstance(expected, list):
        if len(expected) != len(actual):
            return f"{path}: length drift"
        for index, value in enumerate(expected):
            if result := _difference(value, actual[index], f"{path}[{index}]"):
                return result
    elif expected != actual:
        return f"{path}: expected {expected!r}, actual {actual!r}"
    return None


def _retained() -> tuple[dict[str, Any], dict[str, dict[str, Any]]]:
    records: dict[str, Any] = {}
    fixtures: dict[str, dict[str, Any]] = {}
    for path, schema, expected_id in RETAINED:
        fixture = load_json(path)
        validate_json(fixture, schema, owner=f"player-ready retained {expected_id}")
        if fixture["id"] != expected_id:
            raise ValueError(f"player-ready retained fixture identity drift: {expected_id}")
        records[expected_id] = {
            "fixtureId": expected_id,
            "fixtureSha256": sha256(path.read_bytes()).hexdigest().upper(),
        }
        fixtures[expected_id] = fixture
    return records, fixtures


def _assert_input_identity(rom_path: Path, upstream_path: Path) -> bytes:
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    if inspect_rom(rom_path)["sha256"] != CANONICAL_ROM_SHA256:
        raise ValueError("player-ready canonical ROM SHA-256 drift")
    revision = subprocess.run(
        ["git", "-C", str(upstream_path), "rev-parse", "HEAD"],
        check=True,
        capture_output=True,
        text=True,
    ).stdout.strip()
    if revision != UPSTREAM_COMMIT:
        raise ValueError("player-ready pinned SF2DISASM revision drift")
    return rom_path.read_bytes()


def _input_plan(extension: dict[str, Any]) -> list[dict[str, Any]]:
    plan: list[dict[str, Any]] = []
    for segment in extension["segments"]:
        if segment["kind"] != "navigation":
            continue
        points = segment["points"]
        inputs = segment["inputs"]
        if len(points) != len(inputs) + 1:
            raise ValueError(f"player-ready retained route cardinality drift: {segment['id']}")
        for index, input_name in enumerate(inputs):
            plan.append(
                {
                    "waypoint": segment["id"],
                    "input": input_name,
                    "from": {
                        "map": segment["map"],
                        "x": points[index][0],
                        "y": points[index][1],
                    },
                    "to": {
                        "map": segment["map"],
                        "x": points[index + 1][0],
                        "y": points[index + 1][1],
                    },
                }
            )
    if len(plan) != extension["inputCount"] or len(plan) != 46:
        raise ValueError("player-ready retained R2c input count drift")
    return plan


def _static_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    rom = _assert_input_identity(rom_path, upstream_path)
    retained, fixtures = _retained()
    r1 = fixtures[R1[2]]
    r2 = fixtures[R2[2]]
    r2a = fixtures[R2A[2]]
    r2c = fixtures[R2C[2]]
    r3a = fixtures[R3A[2]]
    disasm = upstream_path.resolve(strict=True) / "disasm"
    constants = _equates(
        (disasm / "sf2const.asm").read_text(encoding="utf-8"),
        (
            "MAP_EVENT_PARAM_5",
            "RANDOM_SEED_COPY",
            "MOVING_BATTLE_ENTITY_INDEX",
            "CURRENT_BATTLEACTION",
            "IS_TARGETING",
        ),
    )
    enums = _equates(
        (disasm / "sf2enums.asm").read_text(encoding="utf-8"),
        (
            "COMBATANT_OFFSET_STATUSEFFECTS",
            "COMBATANT_OFFSET_X",
            "COMBATANT_OFFSET_Y",
            "COMBATANT_OFFSET_ACTIVATION_BITFIELD",
            "COMBATANT_ENEMIES_START",
            "ENTITYDEF_OFFSET_ACTSCRIPTADDR",
            "ENTITYDEF_OFFSET_ACTSCRIPTWAITTIMER",
        ),
    )
    functions = {
        **r2["static"]["functions"],
        **r2a["static"]["functions"],
        **r2c["sourceContext"]["functionAddresses"],
        **r3a["controlDispatch"]["functionAddresses"],
        "ControlBattleEntity": 0x22E1A,
        "playerReadyPc": 0x22E70,
        "csc15_setEntityActscript": 0x46978,
        "csc2A_entityShiver": 0x46DEE,
        "csc2D_entityActionSequence": 0x467E2,
    }
    ram = {
        **r1["static"]["ram"],
        **r2["static"]["ram"],
        **r2a["static"]["ram"],
        **constants,
        **enums,
        "MAP_EVENT_PARAM_5": constants["MAP_EVENT_PARAM_5"],
        "RANDOM_SEED_COPY": constants["RANDOM_SEED_COPY"],
        "MOVING_BATTLE_ENTITY_INDEX": constants["MOVING_BATTLE_ENTITY_INDEX"],
        "CURRENT_BATTLEACTION": constants["CURRENT_BATTLEACTION"],
        "IS_TARGETING": constants["IS_TARGETING"],
        "BATTLE_PARTY_MEMBERS_NUMBER": r2a["static"]["ram"]["BATTLE_PARTY_MEMBERS_NUMBER"],
        "BATTLE_PARTY_MEMBERS": r2a["static"]["ram"]["BATTLE_PARTY_MEMBERS"],
        "COMBATANT_OFFSET_STATUSEFFECTS": enums["COMBATANT_OFFSET_STATUSEFFECTS"],
        "COMBATANT_OFFSET_X": enums["COMBATANT_OFFSET_X"],
        "COMBATANT_OFFSET_Y": enums["COMBATANT_OFFSET_Y"],
        "COMBATANT_OFFSET_ACTIVATION_BITFIELD": enums["COMBATANT_OFFSET_ACTIVATION_BITFIELD"],
        "COMBATANT_ENEMIES_START": enums["COMBATANT_ENEMIES_START"],
        "ENTITYDEF_OFFSET_ACTSCRIPTADDR": enums["ENTITYDEF_OFFSET_ACTSCRIPTADDR"],
        "ENTITYDEF_OFFSET_ACTSCRIPTWAITTIMER": enums["ENTITYDEF_OFFSET_ACTSCRIPTWAITTIMER"],
    }
    extension = r2c["static"]["extensionRoute"]
    ready = functions["playerReadyPc"]
    if rom[ready : ready + 4] != bytes.fromhex("1838DE9B"):
        raise ValueError("player-ready ControlBattleEntity input-read opcode drift")
    anchors = [
        {
            "id": name,
            "address": functions[name],
            "width": width,
            "sha256": sha256(rom[functions[name] : functions[name] + width]).hexdigest().upper(),
        }
        for name, width in (
            ("BattleLoop", 2),
            ("GenerateBattleTurnOrder", 2),
            ("ExecuteIndividualTurn", 2),
            ("ProcessBattleEntityControlPlayerInput", 2),
            ("ControlBattleEntity", 2),
            ("playerReadyPc", 4),
            ("csc15_setEntityActscript", 2),
            ("csc2A_entityShiver", 2),
            ("csc2D_entityActionSequence", 2),
        )
    ]
    return {
        "retained": retained,
        "bridge": {
            "kind": "explicit-controlled-harness-bridge",
            "naturalR2bContinuity": False,
            "eventType": 1,
            "eventParam1": 0,
            "map": 21,
            "player": {"x": 5, "y": 15, "facing": ram["DOWN"]},
            "guard": {
                "character": 128,
                "entityIndexSelector": 32,
                "x": 6,
                "y": 16,
                "facing": ram["DOWN"],
            },
            "setFlags": [600, 66, 603, 604, 605, 607, 608, 401, 256],
            "clearFlags": [501, 609, 506, 543],
            "randomSeed": 0x00001234,
            "randomSeedCopy": 0x00001234,
            "frameCounter": 0,
            "secondsCounter": 0,
            "secondsCounterFrames": 0,
        },
        "inputPlan": _input_plan(extension),
        "warps": [
            {
                "id": "map21-to-map40-north-warp",
                "from": {"map": 21, "x": 9, "y": 1},
                "to": {"map": 40, "x": 4, "y": 30, "facing": ram["UP"]},
            },
            {
                "id": "map40-to-map57-wildcard-warp",
                "from": {"map": 40, "x": 14, "y": 12},
                "to": {"map": 57, "x": 8, "y": 18, "facing": ram["UP"]},
            },
        ],
        "admission": {
            "map": 57,
            "battle": 1,
            "area": [0, 0, 16, 20],
            "unlockedFlag": 401,
            "completedFlag": 501,
            "introFlag": 451,
            "regionFlagStart": 90,
            "regionFlagEnd": 105,
        },
        "participatingCombatants": [0, 1, 2, 128, 129, 130, 131, 132, 133],
        "turnOrderEntries": 64,
        "functions": functions,
        "ram": ram,
        "romAnchors": anchors,
        "sourceHashes": {
            path.as_posix(): sha256((disasm / path).read_bytes()).hexdigest().upper()
            for path in SOURCE_PATHS
        },
        "sourceProjectionSha256": sha256(
            _canonical(
                {
                    "route": extension,
                    "turnControl": {
                        "retainedR2c": r3a["retainedR2c"],
                        "turnOrderConsumer": r3a["turnOrderConsumer"],
                        "controlDispatch": r3a["controlDispatch"],
                    },
                }
            )
        ).hexdigest().upper(),
    }


def _r1_config(fixtures: dict[str, dict[str, Any]]) -> dict[str, Any]:
    r1 = fixtures[R1[2]]
    return {
        "functions": r1["static"]["function"],
        "harness": r1["static"]["harness"],
        "sessionPatches": r1["static"]["sessionPatches"],
        "selectedMap": 3,
    }


def _observer_config(fixture: dict[str, Any], static: dict[str, Any]) -> dict[str, Any]:
    _, fixtures = _retained()
    r2 = fixtures[R2[2]]
    return {
        "fixtureId": fixture["id"],
        "core": fixture["emulator"]["core"],
        "caseOrder": fixture["caseOrder"],
        "cases": fixture["cases"],
        "functions": static["functions"],
        "ram": static["ram"],
        "route": r2["static"]["route"]["runtimeOpening"],
        "r1": _r1_config(fixtures),
        "automation": {
            "markerAddress": (
                fixtures[R1[2]]["static"]["harness"]["checkpointAddress"]
                + fixtures[R1[2]]["static"]["harness"]["generatedRamBytes"]
                - 1
            )
        },
        "extension": {
            "owner": OWNER,
            **{
                key: static[key]
                for key in (
                    "retained",
                    "bridge",
                    "inputPlan",
                    "warps",
                    "admission",
                    "participatingCombatants",
                    "turnOrderEntries",
                    "functions",
                )
            },
        },
        "observerFailureContract": observer_failure_contract(OWNER),
    }


def _assert_fixture(fixture: dict[str, Any], static: dict[str, Any]) -> None:
    if (
        fixture["id"] != FIXTURE_ID
        or fixture["system"] != FIXTURE_ID
        or fixture["caseOrder"] != list(CASE_IDS)
        or tuple(fixture["cases"]) != EXPECTED_CASES
    ):
        raise ValueError("player-ready fixture identity/case order drift")
    if fixture["static"] != static:
        raise ValueError("player-ready fixture static/source/retained drift")


def _assert_status() -> None:
    assert_observer_status(
        STATUS_PATH,
        owner=OWNER,
        schema_path=FAILURE_SCHEMA,
        required_milestones=SUCCESS_MILESTONES,
    )
    if tuple(STATUS_PATH.read_text(encoding="utf-8").splitlines()) != SUCCESS_MILESTONES:
        raise RuntimeError("player-ready success status sequence drift")


def _failure_diagnostic() -> dict[str, Any] | None:
    payload = callback_failure_status(STATUS_PATH, owner=OWNER, schema_path=FAILURE_SCHEMA)
    if payload is None:
        return None
    restoration = payload["restoration"]
    if (
        restoration["callbacksCleared"] != payload["callbacksCleared"]
        or restoration["outputRemoved"] != payload["outputRemoved"]
    ):
        raise ValueError("player-ready failure restoration cleanup facts drift")
    if payload["callbacksCleared"] != (payload["callbackCount"] == 0):
        raise ValueError("player-ready failure callback count consistency drift")
    if restoration["sessionStateRestored"] != (payload["restorationMismatch"] is None):
        raise ValueError("player-ready failure restoration mismatch consistency drift")
    return payload


def preflight_map3_battle01_player_ready(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner="Map 3 Battle01 player-ready fixture")
    verify_runtime_contract(fixture, rom_path)
    static = _static_contract(rom_path, upstream_path)
    _assert_fixture(fixture, static)
    _, executable = bizhawk_contract()
    validate_lua_syntax(OBSERVER, executable)
    _observer_config(fixture, static)
    return {"Fixture": FIXTURE_ID, "Cases": 1, "LogicalInputs": 46, "Status": "PRELAUNCH-PASS"}


def verify_map3_battle01_player_ready(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 600
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner="Map 3 Battle01 player-ready fixture")
    verify_runtime_contract(fixture, rom_path)
    static = _static_contract(rom_path, upstream_path)
    _assert_fixture(fixture, static)
    config = _observer_config(fixture, static)
    canonical_before = inspect_rom(rom_path.resolve(strict=True))["sha256"]
    session_deleted = False
    try:
        with tempfile.TemporaryDirectory(prefix="sf2-map3-battle01-player-ready-") as temporary:
            session = Path(temporary) / "map3-battle01-player-ready-session.bin"
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
        validate_json(observed, OBSERVATION_SCHEMA, owner="Map 3 Battle01 player-ready observation")
        if difference := _difference(fixture["expectedObservation"], observed):
            raise ValueError(f"player-ready runtime golden drift: {difference}")
        OBSERVED_OUTPUT.write_text(json.dumps(observed, indent=2) + "\n", encoding="utf-8")
    except Exception:
        OBSERVED_OUTPUT.unlink(missing_ok=True)
        _failure_diagnostic()
        raise
    if inspect_rom(rom_path.resolve(strict=True))["sha256"] != canonical_before:
        raise ValueError("player-ready canonical ROM changed during session run")
    return {
        "Fixture": FIXTURE_ID,
        "Cases": 1,
        "LogicalInputs": 46,
        "BizHawkLaunches": 1,
        "SessionRomDeleted": session_deleted,
        "Status": "PASS",
    }
