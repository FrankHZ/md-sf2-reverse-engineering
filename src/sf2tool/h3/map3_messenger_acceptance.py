"""Bounded H3 confirmation of the Map 3 messenger acceptance body."""

from __future__ import annotations

import json
import math
import re
import shutil
import subprocess
import tempfile
from dataclasses import asdict
from hashlib import sha256
from pathlib import Path
from typing import Any

from sf2tool.h2.map_content import _encode_source, _parse_equates
from sf2tool.h2.map_import import _decode_warps
from sf2tool.h3 import map3_admitted_start as r1
from sf2tool.h3.bizhawk import (
    NativeProcessResult,
    _lua_literal,
    bizhawk_contract,
    run_observer,
    validate_lua_syntax,
    verify_runtime_contract,
)
from sf2tool.h3.bootstrap import BOOTSTRAP_LIBRARY, runtime_bootstrap
from sf2tool.h3.observer_status import (
    SUCCESS_STATUS_TAIL,
    assert_observer_status,
    callback_failure_status,
    observer_failure_contract,
)
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.rom import inspect_rom

OWNER = "map3-messenger-acceptance"
FIXTURE_ID = "sf2-map3-messenger-acceptance-runtime-v1"
FIXTURE = repo_path("tests/fixtures/h3/map3-messenger-acceptance-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h3/map3-messenger-acceptance-fixture.schema.json")
OBSERVATION_SCHEMA = repo_path("schemas/h3/map3-messenger-acceptance-observation.schema.json")
FAILURE_SCHEMA = repo_path("schemas/h3/map3-messenger-acceptance-callback-failure.schema.json")
OBSERVER = repo_path("tools/bizhawk/map3_messenger_acceptance_observer.lua")
OBSERVED_OUTPUT = repo_path(f"local/derived/h3/{OWNER}.observed.json")
STATUS_PATH = repo_path(f"local/derived/h3/{OWNER}.status.txt")
UPSTREAM = repo_path("local/upstream/SF2DISASM")
R1_FIXTURE = repo_path("tests/fixtures/h3/map3-admitted-start-v1.json")
R1_SCHEMA = repo_path("schemas/h3/map3-admitted-start-fixture.schema.json")
R2_FIXTURE = repo_path("tests/fixtures/h3/map3-battle01-natural-route-v1.json")
R2_SCHEMA = repo_path("schemas/h3/map3-battle01-natural-route-fixture.schema.json")
R1_FIXTURE_ID = "sf2-map3-admitted-start-runtime-v1"
R2_FIXTURE_ID = "sf2-map3-battle01-natural-route-runtime-v1"
CASE_IDS = ("natural-map3-messenger-accept-to-follower-ready-wait",)
EXPECTED_CASES = (
    {
        "caseId": CASE_IDS[0],
        "injectedInitialMenuReturn": 1,
        "injectedDifficultyMenuReturn": 0,
        "promptDefaultReturn": 0,
        "frameBudget": 36000,
    },
)
DISASM = Path("disasm")
LISTING = Path("build/sf2build-h1.lst")
SOURCES = (
    Path("sf2const.asm"),
    Path("sf2enums.asm"),
    Path("code/common/scripting/map/mapscriptengine_1.asm"),
    Path("code/common/scripting/map/mapscriptengine_2.asm"),
    Path("code/common/scripting/map/mapsetupsfunctions_1.asm"),
    Path("code/common/scripting/entity/entityfunctions_2.asm"),
    Path("code/common/menus/yesnoprompt.asm"),
    Path("code/common/stats/battleparty.asm"),
    Path("data/maps/entries/map03/mapsetups/scripts_1.asm"),
    Path("data/maps/entries/map03/mapsetups/s3_zoneevents.asm"),
)
SYMBOLS = (
    "ExecuteMapScript",
    "WaitForEvent",
    "RunMapSetupZoneEvent",
    "Map3_ZoneEvent8",
    "cs_5149A",
    "cs_51614",
    "csc00_displaySingleTextbox",
    "csc02_displayTextbox",
    "csc04_setTextIndex",
    "csc08_joinForce",
    "csc0C_jumpIfFlagSet",
    "csc11_promptYesNoForStoryFlow",
    "csc2C_followEntity",
    "AddFollower",
    "YesNoPrompt",
    "SetFlag",
    "JoinForce",
    "UpdateForce",
    "JoinBattleParty",
)
REQUIRED_LUA_ROLES = frozenset(
    {
        "messenger-script-entry",
        "prompt-story-flow",
        "prompt-yes-no",
        "prompt-set-flag",
        "prompt-return",
        "prompt-branch",
        "join-force-command",
        "join-force-service",
        "update-force-service",
        "update-force-return",
        "join-battle-party-service",
        "join-battle-party-return",
        "messenger-text-command",
        "follower-command",
        "follower-service",
        "zone-event8-return",
        "follower-ready-wait",
    }
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
    "milestone:callbacks-cleared:0",
    "milestone:observer-finished",
)


def _canonical(value: Any) -> bytes:
    return json.dumps(value, sort_keys=True, separators=(",", ":")).encode("utf-8")


def _retained_projection() -> dict[str, Any]:
    """Validate the accepted R1/R2 bytes and semantics before this rail runs."""
    one, two = load_json(R1_FIXTURE), load_json(R2_FIXTURE)
    validate_json(one, R1_SCHEMA, owner="Map 3 messenger retained R1 fixture")
    validate_json(two, R2_SCHEMA, owner="Map 3 messenger retained R2 fixture")
    if one["id"] != R1_FIXTURE_ID or two["id"] != R2_FIXTURE_ID:
        raise ValueError("Map 3 messenger retained fixture identity drift")
    projection = {
        "r1": {key: one[key] for key in ("id", "caseOrder", "expectedObservation")},
        "r2": {
            key: two[key] for key in ("id", "caseOrder", "cases", "static", "expectedObservation")
        },
    }
    return {
        "projection": projection,
        "r1FixtureSha256": sha256(R1_FIXTURE.read_bytes()).hexdigest().upper(),
        "r2FixtureSha256": sha256(R2_FIXTURE.read_bytes()).hexdigest().upper(),
        "projectionSha256": sha256(_canonical(projection)).hexdigest().upper(),
    }


def _stream(source: str, symbol: str) -> list[dict[str, Any]]:
    """Parse one exact stream through ``csc_end``, retaining local branch labels."""
    found = re.search(rf"^{re.escape(symbol)}:\s*", source, re.MULTILINE)
    if found is None:
        raise ValueError(f"Map 3 messenger missing script label {symbol}")
    start_line, result = source[: found.start()].count("\n") + 1, []
    for offset, raw in enumerate(source[found.end() :].splitlines(), 1):
        line = raw.split(";", 1)[0].strip()
        if not line or line.endswith(":"):
            continue
        if ":" in line:
            line = line.split(":", 1)[1].strip()
        if not line:
            continue
        item = re.match(r"(?P<opcode>[A-Za-z][A-Za-z0-9_]*(?:\.[bwls])?)\s*(?P<operand>.*)$", line)
        if item is None:
            raise ValueError(f"Map 3 messenger unparsable source line {raw!r}")
        operation = {
            "opcode": item.group("opcode").lower(),
            "operand": re.sub(r"\s+", "", item.group("operand")).lower(),
            "line": start_line + offset,
        }
        result.append(operation)
        if operation["opcode"] == "csc_end":
            return result
    raise ValueError("Map 3 messenger stream omitted csc_end")


def _section(source: str, symbol: str) -> list[tuple[str, str]]:
    start = re.search(rf"^{re.escape(symbol)}:\s*", source, re.MULTILINE)
    if start is None:
        raise ValueError(f"Map 3 messenger missing source section {symbol}")
    end = re.search(r"^\s*; End of function ", source[start.end() :], re.MULTILINE)
    body = source[start.end() : start.end() + end.start()] if end else source[start.end() :]
    records = []
    for raw in body.splitlines():
        line = raw.split(";", 1)[0].strip()
        match = re.match(r"(?P<opcode>[A-Za-z][A-Za-z0-9_]*(?:\.[bwls])?)\s*(?P<operand>.*)$", line)
        if match and not line.endswith(":"):
            records.append(
                (match.group("opcode").lower(), re.sub(r"\s+", "", match.group("operand")).lower())
            )
    return records


def _require_order(
    records: list[tuple[str, str]], expected: tuple[tuple[str, str], ...], name: str
) -> None:
    index = 0
    for item in expected:
        while index < len(records) and records[index] != item:
            index += 1
        if index == len(records):
            raise ValueError(f"Map 3 messenger {name} source use-site/order drift at {item!r}")
        index += 1


def _text_contract(stream: list[dict[str, Any]]) -> dict[str, Any]:
    display = [item for item in stream if item["opcode"] in {"nexttext", "nextsingletext"}]
    source_operands = tuple(item["operand"].split(",") for item in display)
    speakers = tuple(parts[-1] for parts in source_operands)
    modifiers = tuple(parts[0] for parts in source_operands)
    expected = (
        "142",
        "142",
        "142",
        "143",
        "143",
        "142",
        "143",
        "142",
        "142",
        "ally_chester",
        "ally_chester",
        "ally_sarah",
        "ally_chester",
        "ally_sarah",
        "ally_sarah",
        "ally_sarah",
        "ally_chester",
    )
    if speakers != expected:
        raise ValueError("Map 3 messenger text speaker use-site order drift")
    if modifiers != (
        "$0",
        "$0",
        "$0",
        "$0",
        "$0",
        "$0",
        "$0",
        "$0",
        "$0",
        "$0",
        "$0",
        "$c0",
        "$0",
        "$c0",
        "$c0",
        "$0",
        "$0",
    ):
        raise ValueError("Map 3 messenger text portrait-modifier order drift")
    if len(display) != 17:
        raise ValueError("Map 3 messenger text command count drift")
    character_values = {"ally_bowie": 0, "ally_sarah": 1, "ally_chester": 2}
    raw_operands = []
    for modifier, speaker in source_operands:
        character = character_values[speaker] if speaker in character_values else int(speaker, 0)
        raw_operands.append((int(modifier.replace("$", "0x"), 0) << 8) | character)
    return {
        "ids": [*range(517, 532), 535, 536, 447],
        "speakers": [*raw_operands, None],
        "controlShapeSha256": sha256(_canonical(display)).hexdigest().upper(),
    }


def _h1_bytes(listing: str, address: int, width: int) -> str:
    cells: dict[int, int] = {}
    for line in listing.splitlines():
        match = re.match(r"^([0-9A-F]{8})\s+((?:[0-9A-F]{4}\s+)+)", line)
        if match is None:
            continue
        start = int(match.group(1), 16)
        for offset, value in enumerate(bytes.fromhex(re.sub(r"\s+", "", match.group(2)))):
            cells[start + offset] = value
    if any(cell not in cells for cell in range(address, address + width)):
        raise ValueError(f"Map 3 messenger H1 span is incomplete at {address:#x}")
    return bytes(cells[cell] for cell in range(address, address + width)).hex().upper()


def _accepted_path(stream: list[dict[str, Any]]) -> list[dict[str, Any]]:
    """Select only the source branch reached by the original zero prompt return."""
    branch = next(
        (index for index, item in enumerate(stream) if item["opcode"] == "jumpifflagset"),
        None,
    )
    target = next(
        (
            index
            for index, item in enumerate(stream)
            if index > (branch if branch is not None else len(stream))
            and item["opcode"] == "textcursor"
            and item["operand"] == "535"
        ),
        None,
    )
    if branch is None or target is None or stream[branch]["operand"] != "89,cs_51614":
        raise ValueError("Map 3 messenger prompt polarity/accept target drift")
    return [*stream[: branch + 1], *stream[target:]]


def build_map3_messenger_acceptance_source_contract(
    rom_path: Path, upstream_path: Path
) -> dict[str, Any]:
    """Derive the R2a source/H1/ROM contract before fixture/golden comparison."""
    retained = _retained_projection()
    if inspect_rom(rom_path.resolve(strict=True))["sha256"] != r1.CANONICAL_ROM_SHA256:
        raise ValueError("Map 3 messenger canonical ROM SHA-256 drift")
    revision = subprocess.run(
        ["git", "-C", str(upstream_path.resolve(strict=True)), "rev-parse", "HEAD"],
        check=True,
        capture_output=True,
        text=True,
    ).stdout.strip()
    if revision != r1.UPSTREAM_COMMIT:
        raise ValueError("Map 3 messenger upstream revision drift")
    disasm = upstream_path.resolve(strict=True) / DISASM
    listing = (upstream_path.resolve(strict=True) / LISTING).read_text(encoding="utf-8")
    addresses = listing_symbol_addresses(listing)
    missing = sorted(set(SYMBOLS) - set(addresses))
    if missing:
        raise ValueError(f"Map 3 messenger H1 symbols missing: {missing}")
    script = (disasm / "data/maps/entries/map03/mapsetups/scripts_1.asm").read_text(
        encoding="utf-8"
    )
    zones = (disasm / "data/maps/entries/map03/mapsetups/s3_zoneevents.asm").read_text(
        encoding="utf-8"
    )
    engine_one = (disasm / "code/common/scripting/map/mapscriptengine_1.asm").read_text(
        encoding="utf-8"
    )
    engine = (disasm / "code/common/scripting/map/mapscriptengine_2.asm").read_text(
        encoding="utf-8"
    )
    follower = (disasm / "code/common/scripting/entity/entityfunctions_2.asm").read_text(
        encoding="utf-8"
    )
    prompt = (disasm / "code/common/menus/yesnoprompt.asm").read_text(encoding="utf-8")
    party = (disasm / "code/common/stats/battleparty.asm").read_text(encoding="utf-8")
    stream = _accepted_path(_stream(script, "cs_5149A"))
    if len(stream) != 116:
        raise ValueError(f"Map 3 messenger operation count drift: {len(stream)}")
    _require_order(
        [(item["opcode"], item["operand"]) for item in stream],
        (
            ("yesno", ""),
            ("jumpifflagset", "89,cs_51614"),
            ("setf", "600"),
            ("setf", "66"),
            ("join", "128"),
            ("followentity", "ally_sarah,ally_bowie,2"),
            ("followentity", "ally_chester,ally_sarah,2"),
            ("setpos", "138,27,3,down"),
            ("setpos", "139,31,3,down"),
            ("csc_end", ""),
        ),
        "accept stream",
    )
    if not re.search(
        r"Map3_ZoneEvent8:.*?script\s+cs_5149A.*?setFlg\s+603.*?\brts", zones, re.DOTALL
    ):
        raise ValueError("Map 3 messenger ZoneEvent8 setFlg/RTS seam drift")
    _require_order(
        _section(engine, "csc11_promptYesNoForStoryFlow"),
        (("jsr", "j_yesnoprompt"), ("tst.w", "d0"), ("jsr", "j_setflag"), ("rts", "")),
        "prompt",
    )
    _require_order(
        _section(engine, "csc0C_jumpIfFlagSet"),
        (
            ("move.w", "(a6)+,d1"),
            ("jsr", "j_checkflag"),
            ("beq.w", "loc_47428"),
            ("movea.l", "(a6),a6"),
            ("rts", ""),
        ),
        "jump",
    )
    _require_order(
        _section(engine, "csc08_joinForce"),
        (
            ("move.w", "(a6)+,d0"),
            ("cmpi.w", "#128,d0"),
            ("move.w", "#ally_sarah,d0"),
            ("jsr", "j_joinforce"),
            ("move.w", "#ally_chester,d0"),
            ("jsr", "j_joinforce"),
            ("txt", "447"),
            ("rts", ""),
        ),
        "selector-128 join",
    )
    _require_order(
        _section(prompt, "YesNoPrompt"),
        (("clr.w", "d0"), ("move.b", "((player_1_input-$1000000)).w,d0"), ("bra.s", "loc_1528e")),
        "default-zero prompt",
    )
    _require_order(
        _section(party, "JoinForce"),
        (
            ("bsr.w", "setflag"),
            ("bsr.s", "updateforce"),
            ("cmpi.w", "#force_max_size,((battle_party_members_number-$1000000)).w"),
            ("bsr.w", "joinbattleparty"),
            ("rts", ""),
        ),
        "join service",
    )
    _require_order(
        _section(engine_one, "csc2C_followEntity"),
        (
            ("move.w", "(a6)+,d0"),
            ("bsr.w", "getentityaddressfromcharacter"),
            ("move.w", "(a6)+,d0"),
            ("bsr.w", "getentityaddressfromcharacter"),
            ("jsr", "addfollower"),
            ("rts", ""),
        ),
        "follower command",
    )
    _require_order(
        _section(engine_one, "GetEntityAddressFromCharacter"),
        (
            ("lea", "((entity_index_list-$1000000)).w,a5"),
            ("andi.w", "#combatant_mask_all,d0"),
            ("subi.b", "#entity_enemy_index_difference,d0"),
            ("move.b", "(a5,d0.w),d0"),
            ("lsl.w", "#entitydef_size_bits,d0"),
            ("lea", "((entity_data-$1000000)).w,a5"),
            ("rts", ""),
        ),
        "character entity alias",
    )
    _require_order(
        _section(follower, "AddFollower"),
        (
            ("bsr.w", "getentityentryaddress"),
            ("move.l", "a1,entitydef_offset_actscriptaddr(a0)"),
            ("move.b", "d0,-1(a0)"),
            ("move.b", "#-1,(a0)"),
            ("rts", ""),
        ),
        "follower service",
    )
    rom = rom_path.resolve(strict=True).read_bytes()
    ranges = {
        "messengerScript": {
            "address": addresses["cs_5149A"],
            "length": 440,
            "sha256": sha256(rom[addresses["cs_5149A"] : addresses["cs_5149A"] + 440])
            .hexdigest()
            .upper(),
        },
        "zoneEvent8": {
            "address": addresses["Map3_ZoneEvent8"],
            "length": 24,
            "sha256": sha256(rom[addresses["Map3_ZoneEvent8"] : addresses["Map3_ZoneEvent8"] + 24])
            .hexdigest()
            .upper(),
        },
    }
    if (
        ranges["messengerScript"]["sha256"]
        != "01C2ACC81830937BDD6510F88F9FA4E4BF67D6E8F1E49A6693BAEF19B88068AA"
        or ranges["zoneEvent8"]["sha256"]
        != "06B77DD8318014989C0E38C9E0922A9D8C5A1C7E344383091CAFEA1B693DA07F"
    ):
        raise ValueError("Map 3 messenger source/ROM range hash drift")
    h1 = {symbol: _h1_bytes(listing, addresses[symbol], 2) for symbol in SYMBOLS}
    if any(
        h1[symbol] != rom[addresses[symbol] : addresses[symbol] + 2].hex().upper()
        for symbol in SYMBOLS
    ):
        raise ValueError("Map 3 messenger H1/ROM entry drift")
    r1_contract = r1.build_map3_admitted_start_source_contract(rom_path, upstream_path)
    ram = r1._equates(
        (disasm / "sf2const.asm").read_text(encoding="utf-8"),
        (
            "BATTLE_PARTY_MEMBERS",
            "BATTLE_PARTY_MEMBERS_NUMBER",
            "CUTSCENE_DIALOG_INDEX",
            "ENTITY_WALKING_PARAMETERS",
            "FOLLOWERS_LIST",
            "RESERVE_MEMBERS",
            "TARGETS_LIST",
            "TARGETS_LIST_LENGTH",
        ),
    )
    return {
        "retained": retained,
        "functions": {symbol: addresses[symbol] for symbol in SYMBOLS},
        "ram": ram,
        "stream": stream,
        "text": _text_contract(stream),
        "ranges": ranges,
        "h1": h1,
        "sourceHashes": {
            path.as_posix(): sha256((disasm / path).read_bytes()).hexdigest().upper()
            for path in SOURCES
        },
        "r1": {
            "functions": r1_contract["function"],
            "harness": r1_contract["harness"],
            "sessionPatches": r1_contract["sessionPatches"],
            "selectedMap": 3,
        },
    }


def _static_projection(contract: dict[str, Any]) -> dict[str, Any]:
    retained = contract["retained"]
    return {
        "retained": {
            "r1FixtureSha256": retained["r1FixtureSha256"],
            "r2FixtureSha256": retained["r2FixtureSha256"],
            "projectionSha256": retained["projectionSha256"],
        },
        **{
            key: contract[key]
            for key in ("functions", "ram", "stream", "text", "ranges", "h1", "sourceHashes")
        },
    }


def _source_context(contract: dict[str, Any]) -> dict[str, Any]:
    """Expose only the four indexed source seams at the fixture's conventional root."""
    return {
        "function": {
            name: contract["functions"][name]
            for name in (
                "ExecuteMapScript",
                "RunMapSetupZoneEvent",
                "WaitForEvent",
                "cs_5149A",
            )
        }
    }


def _expected_observation(contract: dict[str, Any]) -> dict[str, Any]:
    return {
        "system": FIXTURE_ID,
        "caseOrder": list(CASE_IDS),
        "records": [
            {
                "caseId": CASE_IDS[0],
                "r1FixtureId": R1_FIXTURE_ID,
                "r2FixtureId": R2_FIXTURE_ID,
                "textIds": contract["text"]["ids"],
                "speakerOperands": contract["text"]["speakers"],
                "promptReturn": 0,
                "promptFlag89": True,
                "joinSelector": 128,
                "joined": [1, 2],
                "followers": [
                    {"follower": 1, "leader": 0, "distance": 2},
                    {"follower": 2, "leader": 1, "distance": 2},
                ],
                "guards": [
                    {"id": 138, "x": 27, "y": 3, "facing": 3},
                    {"id": 139, "x": 31, "y": 3, "facing": 3},
                ],
                "flags": {"f600": True, "f66": True, "f603": True},
                "endpoint": {"map": 3, "x": 43, "y": 10, "facing": 3},
                "terminal": "WaitForEvent",
            }
        ],
        "callbacksCleared": True,
        "restoration": {
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
            "callbacksCleared": True,
            "sessionCartPatches": True,
            "sessionRomDeleted": True,
        },
    }


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


def _assert_fixture(fixture: dict[str, Any], contract: dict[str, Any]) -> None:
    if (
        fixture["id"] != FIXTURE_ID
        or fixture["system"] != FIXTURE_ID
        or fixture["caseOrder"] != list(CASE_IDS)
        or tuple(fixture["cases"]) != EXPECTED_CASES
    ):
        raise ValueError("Map 3 messenger fixture identity/case order drift")
    if fixture["static"] != _static_projection(contract):
        raise ValueError("Map 3 messenger fixture static drift")
    if fixture["sourceContext"] != _source_context(contract):
        raise ValueError("Map 3 messenger source context drift")
    if difference := _difference(_expected_observation(contract), fixture["expectedObservation"]):
        raise ValueError(f"Map 3 messenger fixture golden drift: {difference}")


def _assert_lua_roles() -> None:
    source = OBSERVER.read_text(encoding="utf-8")
    missing = sorted(role for role in REQUIRED_LUA_ROLES if f'"{role}"' not in source)
    if missing:
        raise ValueError(f"Map 3 messenger Lua role closure drift: {missing}")
    if source.count("add_callback(config.r1.functions.selectedInitAddress") != 1:
        raise ValueError("Map 3 messenger shared-PC dispatch drift")


def _observer_config(fixture: dict[str, Any], contract: dict[str, Any]) -> dict[str, Any]:
    r2 = load_json(R2_FIXTURE)
    extra = (
        "ProcessPlayerAction",
        "GetActivatedEntity",
        "esc02_controlCharacter",
        "loc_52E8",
        "ProcessMapEvent",
        "ProcessMapEventType1_Warp",
        "ProcessMapEventType6_ZoneEvent",
        "RunMapSetupEntityEvent",
        "RunMapSetupZoneEvent",
        "OpenDoor",
        "Map3_ZoneEvent0",
        "Map3_EntityEvent0",
        "Map3_EntityEvent15",
        "Map3_ZoneEvent6",
        "Map3_ZoneEvent7",
        "cs_513A0",
        "cs_513D6",
        "cs_5145C",
        "cs_5148C",
    )
    harness = contract["r1"]["harness"]
    return {
        "fixtureId": fixture["id"],
        "core": fixture["emulator"]["core"],
        "caseOrder": fixture["caseOrder"],
        "cases": fixture["cases"],
        "functions": {
            **{key: contract["functions"][key] for key in SYMBOLS},
            **{key: r2["static"]["functions"][key] for key in extra},
        },
        "ram": {**r2["static"]["ram"], **contract["ram"]},
        "route": r2["static"]["route"]["runtimeOpening"],
        "r1": contract["r1"],
        "automation": {
            "markerAddress": harness["checkpointAddress"] + harness["generatedRamBytes"] - 1
        },
        "observerFailureContract": observer_failure_contract(OWNER),
    }


def _assert_clean_config(config: dict[str, Any]) -> None:
    forbidden = {
        "expectedObservation",
        "acceptedObservation",
        "records",
        "chronology",
        "restoration",
        "golden",
        "result",
    }
    if isinstance(config, dict):
        if forbidden & set(config):
            raise ValueError("Map 3 messenger observer config leaks accepted output")
        for child in config.values():
            _assert_clean_config(child)
    elif isinstance(config, list):
        for child in config:
            _assert_clean_config(child)


def preflight_map3_messenger_acceptance(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner="Map 3 messenger fixture")
    verify_runtime_contract(fixture, rom_path)
    contract = build_map3_messenger_acceptance_source_contract(rom_path, upstream_path)
    _assert_fixture(fixture, contract)
    _assert_lua_roles()
    _, executable = bizhawk_contract()
    validate_lua_syntax(OBSERVER, executable)
    config = _observer_config(fixture, contract)
    _assert_clean_config(config)
    if contract["retained"] != _retained_projection():
        raise ValueError("Map 3 messenger retained projection drift at golden boundary")
    return {
        "Fixture": fixture["id"],
        "Cases": len(CASE_IDS),
        "Operations": len(contract["stream"]),
        "Status": "PRELAUNCH-PASS",
    }


def _assert_status() -> None:
    assert_observer_status(
        STATUS_PATH,
        owner=OWNER,
        schema_path=FAILURE_SCHEMA,
        required_milestones=SUCCESS_MILESTONES,
    )
    if tuple(STATUS_PATH.read_text(encoding="utf-8").splitlines()) != SUCCESS_MILESTONES:
        raise RuntimeError("Map 3 messenger success status sequence drift")


def _failure_diagnostic() -> dict[str, Any] | None:
    payload = callback_failure_status(STATUS_PATH, owner=OWNER, schema_path=FAILURE_SCHEMA)
    if payload is None:
        return None
    if payload["caseId"] not in {"bootstrap", *CASE_IDS}:
        raise ValueError("Map 3 messenger failure case identity drift")
    restoration = payload["restoration"]
    if (
        restoration["callbacksCleared"] != payload["callbacksCleared"]
        or restoration["outputRemoved"] != payload["outputRemoved"]
    ):
        raise ValueError("Map 3 messenger failure restoration cleanup facts drift")
    if payload["callbacksCleared"] != (payload["callbackCount"] == 0):
        raise ValueError("Map 3 messenger failure callback count consistency drift")
    if restoration["sessionStateRestored"] != (payload["restorationMismatch"] is None):
        raise ValueError("Map 3 messenger restoration mismatch consistency drift")
    return payload


def verify_map3_messenger_acceptance(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 300
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner="Map 3 messenger fixture")
    verify_runtime_contract(fixture, rom_path)
    contract = build_map3_messenger_acceptance_source_contract(rom_path, upstream_path)
    _assert_fixture(fixture, contract)
    _assert_lua_roles()
    _, executable = bizhawk_contract()
    validate_lua_syntax(OBSERVER, executable)
    config = _observer_config(fixture, contract)
    _assert_clean_config(config)
    if contract["retained"] != _retained_projection():
        raise ValueError("Map 3 messenger retained projection drift at golden boundary")
    canonical_before = inspect_rom(rom_path.resolve(strict=True))["sha256"]
    session_deleted = False
    try:
        with tempfile.TemporaryDirectory(prefix="sf2-map3-messenger-acceptance-") as temporary:
            session = Path(temporary) / "map3-messenger-acceptance-session.bin"
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
        validate_json(observed, OBSERVATION_SCHEMA, owner="Map 3 messenger observation")
        if difference := _difference(_expected_observation(contract), observed):
            raise ValueError(f"Map 3 messenger runtime golden drift: {difference}")
        OBSERVED_OUTPUT.write_text(json.dumps(observed, indent=2) + "\n", encoding="utf-8")
    except Exception:
        OBSERVED_OUTPUT.unlink(missing_ok=True)
        _failure_diagnostic()
        raise
    if inspect_rom(rom_path.resolve(strict=True))["sha256"] != canonical_before:
        raise ValueError("Map 3 messenger canonical ROM changed during session run")
    return {
        "Fixture": fixture["id"],
        "Cases": len(CASE_IDS),
        "Operations": len(contract["stream"]),
        "BizHawkLaunches": 1,
        "SessionRomDeleted": session_deleted,
        "Status": "PASS",
    }


def _candidate_execution_sources(interactive: bool = False) -> dict[str, str]:
    """Bind the executed launch/bootstrap helpers, beyond the observer/runner."""
    return {
        name: sha256(repo_path(name).read_bytes()).hexdigest().upper()
        for name in (
            "src/sf2tool/h3/bizhawk.py",
            "src/sf2tool/h3/bootstrap.py",
            "src/sf2tool/toolchain.py",
            BOOTSTRAP_LIBRARY.relative_to(repo_path(".")).as_posix(),
        )
        + (
            (
                "src/sf2tool/bizhawk_debug_bridge.py",
                "tools/debug_bridge.lua",
                "tools/bizhawk/json.lua",
            )
            if interactive
            else ()
        )
    }


def _candidate_warps(
    config: dict[str, Any],
    disasm: Path,
    sources: dict[str, str],
    addresses: dict[str, int],
    listing: str,
    rom: bytes,
    castle: dict[str, Any],
) -> dict[str, Any]:
    """Bind read-only admission to original no-scroll operands and R2 navigation."""
    constants = _parse_equates(disasm)
    current = constants["MAP_CURRENT"]
    exploration = sources["code/gameflow/exploration/explorationfunctions_2.asm"]
    _require_order(
        _section(exploration, "ProcessMapEventType1_Warp"),
        (
            ("tst.b", "((map_event_param_1-$1000000)).w"),
            ("bne.w", "loc_259cc"),
            ("movem.l", "(sp)+,d0"),
            ("move.b", "((map_event_param_2-$1000000)).w,d0"),
            ("bsr.w", "updateplayerposfrommapevent"),
            ("rts", ""),
            ("move.b", "d0,((current_map-$1000000)).w"),
        ),
        "candidate no-scroll warp",
    )
    _require_order(
        _section(exploration, "ExplorationLoop"),
        (
            ("cmpi.b", "#-1,d0"),
            ("beq.s", "@mapindexnotprovided"),
            ("move.b", "d0,((current_map-$1000000)).w"),
            ("bra.s", "loc_25836"),
            ("bsr.w", "waitforfadetofinish"),
            ("bsr.w", "updatemainentityproperties"),
            ("jsr", "j_declareraftentity"),
        ),
        "candidate current-map branch",
    )
    # Named pinned instruction seams; final ROM resolves H1's branch placeholders.
    compare = bytes.fromhex("0C00") + current.to_bytes(2, "big")
    if (
        _h1_bytes(listing, 0x257E4, 4) != compare.hex().upper()
        or rom[0x257E4:0x257E8] != compare
        or rom[0x257E8] != 0x67
        or 0x257EA + int.from_bytes(rom[0x257E9:0x257EA], "big", signed=True) != 0x25828
        or rom[0x2597C:0x2597E] != bytes.fromhex("6600")
        or 0x2597E + int.from_bytes(rom[0x2597E:0x25980], "big", signed=True)
        != addresses["loc_259CC"]
    ):
        raise ValueError("candidate current-map equate/ROM branch drift")
    encoded, count, trailing = _encode_source(
        disasm / "data/maps/entries/map03/6-warp-events.asm", "warpEvents", constants
    )
    start = addresses["Map03s6_WarpEvents"]
    if trailing or rom[start : start + len(encoded)] != encoded:
        raise ValueError("candidate Map3 warp source/ROM table drift")
    rows = _decode_warps(encoded, count)

    def bind(warp: dict[str, int]) -> dict[str, Any]:
        matches = [row for row in rows if row["trigger"] == {"x": warp["x"], "y": warp["y"]}]
        if len(matches) != 1:
            raise ValueError("candidate warp source row missing/ambiguous")
        row = matches[0]
        effective = warp["fromMap"] if row["targetMap"] == current else row["targetMap"]
        if (
            row["scrollMode"] != 0
            or row["targetMap"] != warp["eventDestinationMap"]
            or effective != warp["toMap"]
            or row["destination"] != {"x": warp["destinationX"], "y": warp["destinationY"]}
        ):
            raise ValueError("candidate retained warp/source operand drift")
        return {**warp, "scrollMode": row["scrollMode"], "facing": row["facing"]}

    prefix = []
    for warp in config["route"]["warps"]:
        steps = [
            step
            for step in config["route"]["navigation"]["inputPlan"]
            if step["to"] == {"map": warp["fromMap"], "x": warp["x"], "y": warp["y"]}
        ]
        if len(steps) != 1:
            raise ValueError("candidate warp navigation join missing/ambiguous")
        prefix.append({**bind(warp), "source": steps[0]["from"], "target": steps[0]["to"]})
    north = castle["routeGraph"]["segments"][3]
    retained = next(
        join["row"] for join in castle["retainedWarpJoins"] if join["segment"] == north["id"]
    )
    north_row = bind(retained)
    if (
        north_row["toMap"] != north["to"]["map"]
        or [north_row["destinationX"], north_row["destinationY"]] != north["to"]["point"]
        or north_row["facing"] != constants[north["to"]["facing"]]
    ):
        raise ValueError("candidate north warp graph/source drift")
    points = castle["routeGraph"]["segments"][2]["points"]
    if (
        north["from"]["map"] != north_row["fromMap"]
        or points[-1] != north["from"]["point"]
        or north_row["x"] != 255
        or north_row["y"] != points[-1][1]
    ):
        raise ValueError("candidate north warp navigation/source trigger drift")
    return {
        "currentMapOperand": current,
        "prefixWarps": prefix,
        "northWarpOperands": north_row,
        "northWarpSource": points[-2],
    }


NATURAL_CONTINUATION = "natural-battle01-player-ready"
VICTORY_CONTINUATION = "natural-battle01-victory-5b"
SEGMENT_FILES = (
    "segment.State",
    "continuation.json",
    "checkpoints.jsonl",
    "actual-inputs.jsonl",
    "observer.observed.json",
    "observer.status.txt",
    "host-status.json",
    "bridge/receipt.json",
)
SEGMENT_IDENTITIES = (
    "Continuation",
    "RomSha256",
    "SourceCommit",
    "H1ListingSha256",
    "CastleFixtureSha256",
    "RetainedFixtures",
    "ObserverSha256",
    "RunnerSha256",
    "ExecutionSources",
    "ExecutableSha256",
    "LuaLibrarySha256",
    "SourceHashes",
)


def _read_segment(
    directory: Path, *, require_resumable: bool = True
) -> tuple[dict[str, Any], dict[str, Any]]:
    """Read one complete private pair; a save API success alone never admits resume."""
    directory = directory.resolve(strict=True)
    if not directory.is_relative_to(repo_path("local").resolve()):
        raise ValueError("segment must belong to this worktree's ignored local directory")
    runtime = directory / "runtime"
    pair = load_json(runtime / "segment-pair.json")
    if (
        pair.get("kind") != "savestate-linked-original-acquisition"
        or type(pair.get("resumable")) is not bool
        or (require_resumable and not pair["resumable"])
    ):
        raise ValueError("segment is incomplete or not resumable")
    if set(pair["files"]) != set(SEGMENT_FILES):
        raise ValueError("segment evidence set mismatch")
    for name in SEGMENT_FILES:
        if sha256((runtime / name).read_bytes()).hexdigest().upper() != pair["files"][name]:
            raise ValueError(f"segment state/evidence identity mismatch: {name}")
    if sha256((directory / "candidate.json").read_bytes()).hexdigest().upper() != pair["material"]:
        raise ValueError("segment prepared material identity mismatch")
    report = load_json(directory / "candidate.json")
    metadata = load_json(runtime / "continuation.json")
    ordinal = pair["ordinal"]
    if (
        type(ordinal) is not int
        or ordinal < 1
        or (
            ordinal > (3 if pair["resumable"] else 4)
            and report.get("Continuation") != VICTORY_CONTINUATION
        )
        or metadata["ordinal"] != ordinal
        or metadata["resumable"] != pair["resumable"]
        or report["Segment"]["ordinal"] != ordinal
        or type(report["HistoricalControlledStarts"]) is not int
        or report["HistoricalControlledStarts"] < 0
        or pair["historicalStarts"] != report["HistoricalControlledStarts"] + 1
        or pair["historicalStarts"] != report["FutureControlledOrdinal"]
        or type(pair["activeSeconds"]) not in (int, float)
        or not math.isfinite(pair["activeSeconds"])
        or pair["activeSeconds"] <= 0
        or type(pair["deliveredFrames"]) is not int
        or pair["deliveredFrames"] <= 0
        or type(metadata["deliveredFrames"]) is not int
        or pair["deliveredFrames"] != metadata["deliveredFrames"]
        or metadata["deliveredFrames"] < metadata["observer"]["frame"]
        or metadata["observer"]["frame"] <= 0
        or type(metadata["batches"]) is not int
        or metadata["batches"] <= 0
        or metadata["observer"]["phase"]
        not in ("candidate-route", "candidate-gate-route", "candidate-natural-route")
        or type(metadata.get("checkpoint", {}).get("rank")) is not int
        or metadata["stateBytes"] != (runtime / "segment.State").stat().st_size
    ):
        raise ValueError("segment continuation/accounting mismatch")
    host = load_json(runtime / "host-status.json")
    bridge = load_json(runtime / "bridge/receipt.json")
    if (
        host["status"]
        != ("SEGMENT-SAVED-UNREVIEWED" if pair["resumable"] else "OBSERVATION-COMPLETE-UNREVIEWED")
        or host.get("error")
        or not host["canonicalRomUnchanged"]
        or not host["sessionRomDeleted"]
        or bridge["outcome"] != "completed"
        or bridge["started"] is not True
        or bridge["returncode"] != 0
        or bridge["forcedTermination"]
        or bridge["timedOut"]
        or bridge["historicalStarts"] != pair["historicalStarts"]
        or bridge["runtimeSettingsSha256"] != pair["runtimeSettingsSha256"]
        or ("continuationSettingsIdentity" in bridge) != ("continuationSettingsIdentity" in pair)
        or bridge.get("continuationSettingsIdentity") != pair.get("continuationSettingsIdentity")
    ):
        raise ValueError("segment process/cleanup did not complete")
    identity = pair.get("continuationSettingsIdentity")
    if "continuationSettingsIdentity" in pair and (
        not isinstance(identity, dict)
        or set(identity) != {"contract", "sha256"}
        or identity["contract"] != "bizhawk-local-path-roles-v1"
        or not isinstance(identity["sha256"], str)
        or re.fullmatch(r"[0-9A-F]{64}", identity["sha256"]) is None
    ):
        raise ValueError("segment continuation settings contract mismatch")
    return pair, metadata


def _resume_accounting(directory: Path) -> tuple[dict[str, Any], int]:
    """Reconcile the completed lineage before choosing any compatible saved game state."""
    directory = directory.resolve(strict=True)
    _read_segment(directory)  # The selected parent must itself remain resumable.
    local = repo_path("local").resolve()

    def claims_for(parent: Path) -> list[Path]:
        runtime = parent / "runtime"
        claims = sorted(runtime.glob("resumed-by-*.json"))
        if (runtime / "resumed-by.json").exists():
            claims.insert(0, runtime / "resumed-by.json")
        return claims

    root = directory
    ancestors: set[Path] = set()
    while True:
        if root in ancestors or not root.is_relative_to(local):
            raise ValueError("segment ancestry is cyclic or outside this worktree")
        ancestors.add(root)
        report = load_json(root / "candidate.json")
        parent = report["Segment"]["parentDirectory"]
        if parent is None:
            break
        root = Path(parent).resolve(strict=True)
    pair, metadata = _read_segment(root)
    totals = dict(
        starts=pair["historicalStarts"],
        seconds=pair["activeSeconds"],
        frames=pair["deliveredFrames"],
        batches=metadata["batches"],
    )
    seen = {root}
    pending = [(root, pair, metadata)]
    attempts = []
    while pending:
        parent, parent_pair, previous = pending.pop()
        parent_report = load_json(parent / "candidate.json")
        if not parent_pair["resumable"] and claims_for(parent):
            raise ValueError("terminal segment cannot have descendants")
        for claim in claims_for(parent):
            claimed = load_json(claim)
            child = Path(claimed["candidate"]).resolve(strict=True)
            if child in seen or not child.is_relative_to(local):
                raise ValueError(
                    "segment claims repeat a child, form a cycle or leave this worktree"
                )
            seen.add(child)
            report = load_json(child / "candidate.json")
            selection = report["Segment"]
            ordinal = selection["ordinal"]
            if (
                selection["parentDirectory"] != parent.as_posix()
                or selection["parentPairSha256"]
                != sha256((parent / "runtime/segment-pair.json").read_bytes()).hexdigest().upper()
                or claimed["ordinal"] != ordinal
                or report["HistoricalControlledStarts"] < parent_pair["historicalStarts"]
                or selection["priorActiveSeconds"] < parent_pair["activeSeconds"]
                or selection["priorFrames"] < parent_pair["deliveredFrames"]
                or selection["priorBatches"] < previous["batches"]
                or not (
                    ordinal == parent_pair["ordinal"] + 1
                    or ordinal == parent_pair["ordinal"] == 1
                    and previous["checkpoint"]["rank"] < 5
                )
                or any(report[key] != parent_report[key] for key in SEGMENT_IDENTITIES)
            ):
                raise ValueError("segment claim parent/source/ordinal identity mismatch")
            sealed = None
            if (child / "runtime/segment-pair.json").exists():
                sealed = _read_segment(child, require_resumable=False)
                if sealed[1]["checkpoint"]["rank"] <= previous["checkpoint"]["rank"]:
                    raise ValueError("child checkpoint does not advance beyond its parent")
                pending.append((child, *sealed))
            elif claims_for(child):
                raise ValueError("unsealed attempt cannot have descendants")
            attempts.append((child, report, sealed))
    if not ancestors.issubset(seen):
        raise ValueError("selected segment ancestry is missing its parent claim")

    def prior_cost(attempt: tuple) -> tuple:
        report = attempt[1]
        selection = report["Segment"]
        values = (
            report["HistoricalControlledStarts"],
            selection["priorActiveSeconds"],
            selection["priorFrames"],
            selection["priorBatches"],
        )
        if (
            any(type(values[i]) is not int or values[i] < 0 for i in (0, 2, 3))
            or type(values[1]) not in (int, float)
            or not math.isfinite(values[1])
            or values[1] < 0
        ):
            raise ValueError("segment prior accounting is invalid")
        # A zero-duration pre-process failure can have exactly the same prior
        # totals as the next actual launch; charge the no-start attempt first.
        receipt = load_json(attempt[0] / "runtime/bridge/receipt.json")
        return (*values, receipt.get("started") is not False)

    # Claims express parentage, not launch order: a later attempt can return to
    # an ancestor or cousin. Its frozen cumulative prior cost supplies the order.
    for child, report, sealed in sorted(attempts, key=prior_cost):
        host = load_json(child / "runtime/host-status.json")
        receipt = load_json(child / "runtime/bridge/receipt.json")
        if (
            not host.get("canonicalRomUnchanged")
            or not host.get("sessionRomDeleted")
            or not (sealed or host.get("error") or host.get("status") == "INCOMPLETE-OBSERVATION")
            or report["HistoricalControlledStarts"] != totals["starts"]
            or report["Segment"]["priorActiveSeconds"] != totals["seconds"]
            or report["Segment"]["priorFrames"] != totals["frames"]
            or report["Segment"]["priorBatches"] != totals["batches"]
        ):
            raise ValueError(
                "resume needs completed lineage attempts with reconciled cleanup/accounting"
            )
        if receipt.get("started") is False:
            # Absence of an input log is not evidence of zero delivery. Both owners
            # must positively record a completed failure before process creation.
            if (
                host.get("failureKind") != "pre-process-failure"
                or host.get("process", {}).get("started") is not False
                or host["process"].get("pid") is not None
                or receipt.get("outcome") != "failed"
                or not receipt.get("error")
                or receipt.get("commands") != []
                or receipt.get("pid") is not None
                or receipt.get("returncode") is not None
                or "historicalStarts" in receipt
                or receipt.get("luaStatus") is not None
                or (child / "runtime/actual-inputs.jsonl").exists()
            ):
                raise ValueError(
                    "pre-process retry lacks consistent no-process/no-delivery evidence"
                )
            # A failed Popen can have a measured launch-attempt duration. Retain it
            # conservatively in cumulative charged seconds, even without a native start.
            if "elapsedSeconds" in receipt:
                seconds = receipt["elapsedSeconds"]
                if type(seconds) not in (int, float) or not math.isfinite(seconds) or seconds < 0:
                    raise ValueError("pre-process attempt duration is invalid")
                totals["seconds"] += seconds
            continue
        if receipt.get("started") is not True or receipt.get("returncode") is None:
            raise ValueError("started retry needs a completed process receipt")
        if (
            receipt.get("historicalStarts") != totals["starts"] + 1
            or report["FutureControlledOrdinal"] != totals["starts"] + 1
        ):
            raise ValueError("segment actual native-start accounting mismatch")
        rows = [
            json.loads(line)
            for line in (child / "runtime/actual-inputs.jsonl")
            .read_text(encoding="utf-8")
            .splitlines()
        ]
        frames = [row for row in rows if row["kind"] == "frame"]
        seconds = receipt["elapsedSeconds"]
        if type(seconds) not in (int, float) or not math.isfinite(seconds) or seconds < 0:
            raise ValueError("failed attempt elapsed/start accounting is unavailable")
        totals["starts"] += 1
        totals["seconds"] += seconds
        totals["frames"] += len(frames)
        totals["batches"] += len({row["id"] for row in frames if row["id"] != 0})
        if sealed and (
            sealed[0]["historicalStarts"] != totals["starts"]
            or sealed[0]["activeSeconds"] != totals["seconds"]
            or sealed[0]["deliveredFrames"] != totals["frames"]
            or sealed[1]["batches"] != totals["batches"]
        ):
            raise ValueError("sealed child cumulative accounting mismatch")
    return totals, len(claims_for(directory))


def _seal_segment(directory: Path, report: dict[str, Any], diagnostic: dict[str, Any]) -> None:
    """Publish the pair last, after native exit, original identity and cleanup checks."""
    runtime = directory / "runtime"
    if (
        diagnostic["bridge"]["started"] is not True
        or diagnostic["bridge"]["historicalStarts"] != report["HistoricalControlledStarts"] + 1
        or diagnostic["bridge"]["historicalStarts"] != report["FutureControlledOrdinal"]
    ):
        raise ValueError("segment actual native-start accounting mismatch")
    metadata = load_json(runtime / "continuation.json")
    prior = report["Segment"]["priorActiveSeconds"]
    active = prior + diagnostic["bridge"]["elapsedSeconds"]
    if not math.isfinite(active) or not metadata["activeSecondsAtSave"] <= active:
        raise ValueError("segment cumulative active-time budget mismatch/exhaustion")
    inputs = [
        json.loads(line)
        for line in (runtime / "actual-inputs.jsonl").read_text(encoding="utf-8").splitlines()
    ]
    frames = [row for row in inputs if row["kind"] == "frame"]
    steps = {row["id"] for row in frames if row["id"] != 0}
    prior_frame, prior_order = 0, 0
    if report["Segment"]["parentDirectory"]:
        parent, previous = _read_segment(Path(report["Segment"]["parentDirectory"]))
        prior_frame, prior_order = previous["observer"]["frame"], parent["lastOrder"]
        if metadata["checkpoint"]["rank"] <= previous["checkpoint"]["rank"]:
            raise ValueError("segment must advance beyond its parent's checkpoint")
    if (
        not frames
        or len(frames) + prior_frame != metadata["observer"]["frame"]
        or len(frames) + report["Segment"]["priorFrames"] != metadata["deliveredFrames"]
        or metadata["deliveredFrames"] <= 0
        or len(steps) + report["Segment"]["priorBatches"] != metadata["batches"]
        or frames[-1]["afterFrame"] != metadata["original"]["emulatorFrame"]
        or any(
            row["frame"] != prior_frame + i or row["afterFrame"] != row["beforeFrame"] + 1
            for i, row in enumerate(frames, 1)
        )
        or any(
            a["afterFrame"] != b["beforeFrame"] for a, b in zip(frames, frames[1:], strict=False)
        )
    ):
        raise ValueError("segment delivered-frame/batch reconciliation failed")
    orders = [
        json.loads(line)["order"]
        for name in ("checkpoints.jsonl", "actual-inputs.jsonl")
        for line in (runtime / name).read_text(encoding="utf-8").splitlines()
    ]
    if len(orders) != len(set(orders)) or not orders or min(orders) <= prior_order:
        raise ValueError("segment evidence order is not unique")
    pair = {
        "kind": "savestate-linked-original-acquisition",
        "ordinal": metadata["ordinal"],
        "resumable": metadata["resumable"],
        "historicalStarts": diagnostic["bridge"]["historicalStarts"],
        "activeSeconds": active,
        "deliveredFrames": metadata["deliveredFrames"],
        "endedAtUnix": diagnostic["bridge"]["endedAtUnix"],
        "lastOrder": max(orders),
        "runtimeSettingsSha256": diagnostic["bridge"]["runtimeSettingsSha256"],
        "material": sha256((directory / "candidate.json").read_bytes()).hexdigest().upper(),
        "files": {
            name: sha256((runtime / name).read_bytes()).hexdigest().upper()
            for name in SEGMENT_FILES
        },
    }
    # Exclusive publication: partial JSON also fails closed, never repaired in place.
    if "continuationSettingsIdentity" in diagnostic["bridge"]:
        pair["continuationSettingsIdentity"] = diagnostic["bridge"]["continuationSettingsIdentity"]
    with (runtime / "segment-pair.json").open("x", encoding="utf-8") as output:
        output.write(json.dumps(pair, indent=2) + "\n")


def _interactive_limits(continuation: str | None) -> dict[str, int]:
    if continuation not in (None, NATURAL_CONTINUATION, VICTORY_CONTINUATION):
        raise ValueError("unsupported candidate continuation")
    if continuation:
        return {
            "stabilization": True,
            "wallSeconds": 7200,
            "totalFrames": 36000,
            "maxBatches": 600,
            "maxBatchFrames": 120,
            "map19Seconds": 3600,
            "guardSeconds": 5700,
            "idleSeconds": 120,
            "progressFrames": 3600,
        }
    return {"wallSeconds": 1800, "totalFrames": 28634, "maxBatches": 2048, "maxBatchFrames": 120}


def _natural_configuration(
    rom_path: Path,
    upstream: Path,
    config: dict[str, Any],
    sources: dict[str, str],
    addresses: dict[str, int],
    listing: str,
    rom: bytes,
    castle: dict[str, Any],
) -> dict[str, Any]:
    """Read-only R2d shapes plus pinned natural callers; never install its bridge."""
    from sf2tool.h3 import map3_battle01_player_ready as ready

    static = ready._static_contract(rom_path, upstream)
    config["ram"].update(static["ram"])
    disasm = upstream / DISASM
    paths = {p.as_posix() for p in ready.SOURCE_PATHS} | {
        "code/common/scripting/map/mapscriptengine_2.asm",
        "code/common/scripting/map/mapsetupsfunctions_1.asm",
        "code/common/tech/interrupts/trap0_soundcommand.asm",
        "code/common/tech/interrupts/applyfadingeffectandz80busupdate.asm",
        "code/gameflow/battle/ai/startaicontrol.asm",
        "code/gameflow/battle/battleactions/battleactionsengine_2.asm",
        "code/gameflow/battle/battleloop_2.asm",
        "code/common/maps/getbattle.asm",
        "code/common/stats/gold.asm",
        "code/common/maps/camerafunctions.asm",
        "code/common/maps/animations.asm",
        "code/common/maps/mapload.asm",
        "code/common/tech/graphics/display.asm",
        "code/gameflow/exploration/exploration.asm",
        "code/gameflow/exploration/explorationvints.asm",
        "code/gameflow/exploration/explorationfunctions_0.asm",
        "code/common/menus/main/mainactions.asm",
        "code/common/menus/diamondmenu.asm",
        "code/common/tech/input.asm",
        "data/maps/entries/map19/2-areas.asm",
    }
    for number in (19, 20, 21, 40, 57):
        root = disasm / f"data/maps/entries/map{number:02}"
        paths.update(p.relative_to(disasm).as_posix() for p in (root / "mapsetups").glob("*.asm"))
        paths.add(f"data/maps/entries/map{number:02}/6-warp-events.asm")
    for name in sorted(paths):
        source = (disasm / name).read_text(encoding="utf-8")
        pinned = subprocess.run(
            ["git", "-C", str(upstream), "show", f"{r1.UPSTREAM_COMMIT}:disasm/{name}"],
            check=True,
            capture_output=True,
            text=True,
            encoding="utf-8",
        ).stdout
        if source != pinned:
            raise ValueError(f"natural candidate pinned source differs: {name}")
        sources[name] = source
    for path, symbol, instructions in (
        (
            "code/common/menus/main/mainactions.asm",
            "FieldMenu",
            (
                ("jsr", "j_executediamondmenu"),
                ("cmpi.w", "#-1,d0"),
                ("beq.s", "@exitmain"),
                ("unlk", "a6"),
                ("movem.l", "(sp)+,d0-a5"),
                ("rts", ""),
            ),
        ),
        (
            "code/common/menus/diamondmenu.asm",
            "ExecuteDiamondMenu",
            (
                ("btst", "#input_bit_b,((current_player_input-$1000000)).w"),
                ("beq.s", "@loc_11"),
                ("moveq", "#-1,d1"),
                ("moveq", "#-1,d0"),
                ("move.b", "d0,((current_diamond_menu_choice-$1000000)).w"),
            ),
        ),
        (
            "code/common/maps/mapload.asm",
            "LoadMap",
            (
                (
                    "move.l",
                    "((view_plane_a_pixel_x_dest-$1000000)).w,((view_plane_a_pixel_x-$1000000)).w",
                ),
                ("lsl.w", "#7,d0"),
                ("mulu.w", "((map_area_layer1_parallax_x-$1000000)).w,d0"),
                ("lsr.l", "#byte_shift_count,d0"),
                ("add.w", "d4,d0"),
                ("move.w", "d0,((view_plane_a_pixel_x-$1000000)).w"),
            ),
        ),
    ):
        _require_order(_section(sources[path], symbol), instructions, symbol)
    # These non-relocated original instructions bind the reviewed cancellation and
    # loaded-view coordinate writes to the canonical ROM, beyond callback entries.
    for address, size in ((0x2CB4, 22), (0x102EC, 6), (0x102F4, 8)):
        if _h1_bytes(listing, address, size) != rom[address : address + size].hex().upper():
            raise ValueError("loaded camera source/H1/ROM mismatch")
    constants = _parse_equates(disasm)
    extra_ram = (
        "VIEW_PLANE_A_PIXEL_X",
        "VIEW_PLANE_A_PIXEL_Y",
        "VIEW_PLANE_A_PIXEL_X_DEST",
        "VIEW_PLANE_A_PIXEL_Y_DEST",
        "VIEW_PLANE_B_PIXEL_X",
        "VIEW_PLANE_B_PIXEL_Y",
        "VIEW_PLANE_B_PIXEL_X_DEST",
        "VIEW_PLANE_B_PIXEL_Y_DEST",
        "VIEW_SCROLLING_PLANES_BITFIELD",
        "MAP_AREA_LAYER1_AUTOSCROLL_X",
        "MAP_AREA_LAYER2_AUTOSCROLL_X",
        "MAP_AREA_LAYER_TYPE",
        "FADING_SETTING",
        "FADING_POINTER",
        "FADING_COUNTER",
        "SOUND_COMMAND_QUEUE",
        "SOUND_COMMANDS_DEACTIVATED",
        "AUTO_BATTLE_TOGGLE",
        "STATUSEFFECT_MUDDLE",
        "AIBITFIELD_AI_CONTROLLED",
        "COMBATANT_ALLIES_NUMBER",
        "COMBATANT_ENEMIES_NUMBER",
        "MUSIC_STACK",
        "DIALOGUE_WINDOW_INDEX",
        "PORTRAIT_WINDOW_INDEX",
        "CURRENT_DIAMOND_MENU_CHOICE",
    )
    constants.update(
        r1._equates(sources["sf2const.asm"] + "\n" + sources["sf2enums.asm"], extra_ram)
    )
    config["ram"].update({name: constants[name] for name in extra_ram})
    # H1 leaves forward branches unresolved. Resolve only these source-named
    # operands; every other byte of both scrolling functions must match the ROM.
    for start_name, end_name, relocation_count in (
        ("WaitForViewScrollEnd", "IsMapScrollingToViewTarget", 3),
        ("IsMapScrollingToViewTarget", "VInt_UpdateMapPlanes", 2),
    ):
        start, end = addresses[start_name], addresses[end_name]
        encoded = bytearray.fromhex(_h1_bytes(listing, start, end - start))
        rows = [
            line
            for line in listing.splitlines()
            if re.match(r"^[0-9A-F]{8} ", line) and start <= int(line[:8], 16) < end
        ]
        local_labels = {
            match[2]: int(match[1], 16)
            for line in rows
            if (match := re.match(r"^([0-9A-F]{8}) +(@\w+):", line))
        }
        resolved = 0
        for line in rows:
            match = re.match(r"^([0-9A-F]{8}) +(6100 0000|6700) +(bsr\.w|beq\.s) +([@\w]+)", line)
            if not match:
                continue
            address, form, mnemonic, target_name = match.groups()
            pc = int(address, 16)
            target = local_labels.get(target_name, addresses.get(target_name))
            if target is None or (form, mnemonic) not in {
                ("6100 0000", "bsr.w"),
                ("6700", "beq.s"),
            }:
                raise ValueError(f"camera predicate H1 relocation drift: {start_name}")
            width = 2 if mnemonic == "bsr.w" else 1
            offset = pc - start + (2 if width == 2 else 1)
            encoded[offset : offset + width] = (target - pc - 2).to_bytes(width, "big", signed=True)
            resolved += 1
        if resolved != relocation_count or encoded != rom[start:end]:
            raise ValueError(f"camera predicate H1/ROM mismatch: {start_name}")
    area, _, _ = _encode_source(disasm / "data/maps/entries/map19/2-areas.asm", "areas", constants)
    area_start = addresses["Map19s2_Areas"]
    if rom[area_start : area_start + len(area)] != area:
        raise ValueError("Map19 camera area source/ROM mismatch")
    names = (
        "ExecuteDiamondMenu",
        "WaitForPlayerInput",
        "ms_map19_InitFunction",
        "ms_map20_InitFunction",
        "ms_map21_InitFunction",
        "Map19_EntityEvent12",
        "Map21_EntityEvent0",
        "Map19_DefaultZoneEvent",
        "GetEntityAddressFromCharacter",
        "cs_53996",
        "cs_52F0C",
        "cs_52F24",
        "cs_52F40",
        "cs_53EF4",
        "cs_53B60",
        "bbcs_01",
        "ms_Empty",
        "loc_47140",
        "loc_47156",
        "loc_4756A",
        "Trap0_SoundCommand",
        "ApplyZ80BusUpdates",
        "StartAiControl",
        "ExecuteAiControl",
        "battlesceneScript_ApplyActionEffect",
        "BattleLoop_Victory",
        "BattleLoop_Defeat",
        "ms_map40_InitFunction",
    )
    functions = {
        name: static["functions"][name]
        for name in (
            "CheckBattle",
            "BattleLoop",
            "ExecuteBeforeBattleCutscene",
            "LoadBattle",
            "ExecuteBattleStartCutscene",
            "ActivateEnemies",
            "ExecuteBattleRegionCutscene",
            "PopulateTargetsListWithSpawningEnemies",
            "GenerateBattleTurnOrder",
            "ExecuteIndividualTurn",
            "ProcessBattleEntityControlPlayerInput",
            "playerReadyPc",
        )
    }
    for name in names:
        functions[name] = addresses[name]
    # The stable seam is a named original input-read instruction, not a synthetic PC.
    for name, address in functions.items():
        if _h1_bytes(listing, address, 2) != rom[address : address + 2].hex().upper():
            raise ValueError(f"natural callback H1/ROM mismatch: {name}")
    programs = ["cs_53996", "cs_52F0C", "cs_53EF4", "bbcs_01", "ms_Empty"]
    segments = (
        castle["routeGraph"]["segments"][4:]
        + load_json(ready.R2C[0])["static"]["extensionRoute"]["segments"]
    )
    warps = []
    for i, segment in enumerate(segments):
        if segment["kind"] != "warp":
            continue
        origin, destination = segment["from"], segment["to"]
        number = origin["map"]
        path = disasm / f"data/maps/entries/map{number:02}/6-warp-events.asm"
        encoded, count, trailing = _encode_source(path, "warpEvents", constants)
        start = addresses[f"Map{number:02}s6_WarpEvents"]
        if trailing or rom[start : start + len(encoded)] != encoded:
            raise ValueError("natural warp source/ROM mismatch")
        rows = [
            row
            for row in _decode_warps(encoded, count)
            if row["trigger"]["x"] in (255, origin["point"][0])
            and row["trigger"]["y"] in (255, origin["point"][1])
            and row["targetMap"] == destination["map"]
            and row["destination"] == dict(zip(("x", "y"), destination["point"], strict=True))
        ]
        if len(rows) != 1 or rows[0]["scrollMode"] != 0:
            raise ValueError("natural selected warp missing/ambiguous")
        previous = segments[i - 1]
        # Royal return is a one-tile Up movement from the script-owned (23,39).
        source = previous["points"][-2]
        warps.append(
            {
                "id": segment["id"],
                "fromMap": number,
                "target": origin["point"],
                "source": source,
                **rows[0],
            }
        )
    entity_calls = []
    for line in listing.splitlines():
        match = re.match(r"^([0-9A-F]{8})\s+4E90\s+jsr\s+\(a0\)(?:\s|$)", line)
        if (
            match
            and addresses["RunMapSetupEntityEvent"] <= int(match[1], 16) < addresses["sub_476DC"]
        ):
            entity_calls.append(int(match[1], 16))
    if len(entity_calls) != 1 or rom[entity_calls[0] : entity_calls[0] + 2] != bytes.fromhex(
        "4E90"
    ):
        raise ValueError("entity dispatch source/H1/ROM seam drift")
    functions["entityCallPc"] = entity_calls[0]
    sound_dispatch = []
    for line in listing.splitlines():
        match = re.match(r"^([0-9A-F]{8})\s+((?:[0-9A-F]{4}\s+)+)\s*(move\.b.*)", line)
        if (
            match
            and "(Z80_SoundDriverCommand).l" in match[3]
            and 0x8DE <= int(match[1], 16) < 0xB1E
        ):
            address = int(match[1], 16)
            emitted = bytes.fromhex(match[2])
            if rom[address : address + len(emitted)] != emitted:
                raise ValueError("sound dispatch H1/ROM operand drift")
            sound_dispatch.append(
                {"pc": address, "width": len(emitted), "previousMusic": "MUSIC_STACK" in match[3]}
            )
    if len(sound_dispatch) != 4:
        raise ValueError("sound dispatch use-site inventory drift")
    config["ram"]["MUSIC_STACK"] = constants["MUSIC_STACK"]
    # Consume the existing route owners; do not add a second hand-entered route.
    interactions = [
        point for point in config["route"]["waypoints"] if point["interaction"] == "entity"
    ]
    for point in castle["routeGraph"]["segments"]:
        if point["kind"] not in ("entity-interaction", "entity-terminal"):
            continue
        x, y = point.get("interactionPoint", point["player"])
        facing = point.get("interactionFacing", point["facing"])
        dx, dy = {"UP": (0, -1), "DOWN": (0, 1), "LEFT": (-1, 0), "RIGHT": (1, 0)}[facing]
        tx, ty = point.get("entityPointBefore", [x + dx, y + dy])
        interactions.append(
            dict(
                id=point["id"],
                map=point["map"],
                x=x,
                y=y,
                facing=facing,
                entityTarget=dict(id=point["entity"], map=point["map"], x=tx, y=ty),
            )
        )
    checkpoints = []
    for rank, name in enumerate(
        ("map3-house-exit-zone", "map3-sarah-classroom", "map3-entity142", "map3-zone-messenger"), 1
    ):
        point = next(point for point in config["route"]["waypoints"] if point["id"] == name)
        checkpoints.append(
            dict(
                id=name,
                rank=rank,
                map=point["map"],
                x=point["x"],
                y=point["y"],
                flag=256 if rank == 3 else point["completionFlag"],
                beforeFlag=602 if rank == 3 else None,
                phase="candidate-gate-route" if rank == 4 else "candidate-route",
                r2a=rank == 4,
            )
        )
    checkpoints.append(
        dict(
            id="map19-first-displacement",
            rank=5,
            map=19,
            x=26,
            y=29,
            flag=604,
            phase="candidate-natural-route",
        )
    )
    return {
        "selection": NATURAL_CONTINUATION,
        "soundDispatch": sound_dispatch,
        "setups": {str(n): addresses[f"ms_map{n}"] for n in (3, 19, 20, 21, 40)},
        "functions": functions,
        "programs": [functions[name] for name in programs],
        "warps": warps,
        "interactions": interactions,
        "checkpoints": checkpoints,
        "admission": static["admission"],
        "turnOrderEntries": static["turnOrderEntries"],
    }


def _victory_configuration(
    upstream: Path,
    config: dict[str, Any],
    sources: dict[str, str],
    addresses: dict[str, int],
    listing: str,
    rom: bytes,
) -> None:
    """Bind the continuation to the accepted R3a-d/R4a evidence, without a new fixture."""
    from sf2tool.h2.map_setup import _encode_routes, _parse_routes, _select_route

    natural = config["candidate"]["natural"]
    natural["selection"] = VICTORY_CONTINUATION
    disasm = upstream / DISASM
    owners = {}
    for suffix in (
        "turn-control",
        "action-effect",
        "action-completion",
        "turn-finalization",
        "victory-return",
    ):
        fixture = load_json(repo_path(f"tests/fixtures/h2/map3-battle01-{suffix}-static-v1.json"))
        if (
            fixture["upstream"]["commit"] != r1.UPSTREAM_COMMIT
            or fixture["romSha256"] != r1.CANONICAL_ROM_SHA256
        ):
            raise ValueError("victory retained provenance drift")
        owners[suffix] = fixture["id"]
        for row in fixture["sourceContext"]["sourceIdentities"]:
            data = (disasm / row["path"]).read_bytes()
            if sha256(data).hexdigest().upper() != row["sha256"]:
                raise ValueError(f"victory retained source drift: {row['path']}")
            sources[row["path"]] = data.decode("utf-8")
        for row in fixture["sourceContext"]["h1RomAnchors"]:
            start, width = row["address"], row["width"]
            if sha256(rom[start : start + width]).hexdigest().upper() != row["sha256"]:
                raise ValueError(f"victory retained H1/ROM drift: {row['id']}")
    for path in (
        "code/common/tech/randomnumbergenerator.asm",
        "code/common/menus/diamondmenu.asm",
        "code/common/menus/magicmenu.asm",
        "code/common/menus/itemmenu.asm",
        "code/common/stats/itemstats.asm",
        "code/gameflow/battle/battleactions/breakuseditem.asm",
        "data/stats/items/itemdefs.asm",
        "data/stats/spells/spelldefs.asm",
        "data/maps/mapsetups.asm",
        "code/common/scripting/map/mapsetupsfunctions_1.asm",
    ):
        source = (disasm / path).read_text(encoding="utf-8")
        pinned = subprocess.run(
            ["git", "-C", str(upstream), "show", f"{r1.UPSTREAM_COMMIT}:disasm/{path}"],
            check=True,
            capture_output=True,
            text=True,
            encoding="utf-8",
        ).stdout
        if source != pinned:
            raise ValueError(f"victory pinned source drift: {path}")
        sources[path] = source
    # Map 57 has no setup row. The original resolver returns ms_Void after
    # victory, so bind that exact fallback instead of accepting arbitrary setups.
    routes = _parse_routes(sources["data/maps/mapsetups.asm"])
    encoded_routes = _encode_routes(routes, addresses)
    setup_pc, void_pc = addresses["MapSetups"], addresses["ms_Void"]
    if (
        _select_route(routes, 57, set()) != "ms_Void"
        or rom[setup_pc : setup_pc + len(encoded_routes)] != encoded_routes
        or rom[void_pc : void_pc + 2] != bytes.fromhex("FFFF")
    ):
        raise ValueError("post-victory Map57 void setup source/H1/ROM drift")
    _require_order(
        _section(
            sources["code/common/scripting/map/mapsetupsfunctions_1.asm"], "GetCurrentMapSetup"
        ),
        (("cmpi.w", "#-1,(a1)"), ("lea", "ms_void(pc),a0"), ("bra.w", "@return")),
        "post-victory void setup resolver",
    )
    # H1 leaves PC-relative operands unresolved; bind those from named symbols
    # and check their canonical ROM encoding instead of comparing placeholders.
    void_loads = [
        int(match[1], 16)
        for line in listing.splitlines()
        if (
            match := re.match(
                r"^([0-9A-F]{8})\s+41FA\s+[0-9A-F]{4}\s+lea\s+ms_Void\(pc\),\s*a0", line
            )
        )
    ]
    if len(void_loads) != 1:
        raise ValueError("void setup resolver load missing/ambiguous")
    void_load = void_loads[0]
    tail_branch = addresses["rpt_AfterBattleCutscenes"] - 4
    bindings = (
        (
            void_load,
            bytes.fromhex("41FA") + (void_pc - void_load - 2).to_bytes(2, "big", signed=True),
            2,
        ),
        (void_pc, bytes.fromhex("FFFF"), 2),
        (addresses["ExecuteAfterBattleCutscene"], bytes.fromhex("48E7C000"), 4),
        (
            tail_branch,
            bytes.fromhex("6000")
            + (addresses["EndAfterBattleCutscene"] - tail_branch - 2).to_bytes(
                2, "big", signed=True
            ),
            2,
        ),
        (addresses["table_AfterBattleJoins"] - 6, bytes.fromhex("4CDF00034E75"), 6),
    )
    for pc, encoded, listing_width in bindings:
        if (
            rom[pc : pc + len(encoded)] != encoded
            or _h1_bytes(listing, pc, listing_width) != encoded[:listing_width].hex().upper()
        ):
            raise ValueError("post-victory setup/tail source/H1/ROM operand drift")
    _require_order(
        _section(
            sources["code/gameflow/battle/cutscenes/afterbattlecutscenesstart.asm"],
            "ExecuteAfterBattleCutscene",
        ),
        (
            ("movem.l", "d0-d1,-(sp)"),
            ("bsr.w", "executemapscript"),
            ("movem.l", "(sp)+,d0/a0"),
            ("bra.w", "endafterbattlecutscene"),
        ),
        "after-cutscene shared tail entry",
    )
    _require_order(
        _section(
            sources["code/gameflow/battle/cutscenes/afterbattlecutscenesend.asm"],
            "EndAfterBattleCutscene",
        ),
        (("jsr", "j_joinforce"), ("movem.l", "(sp)+,d0-d1"), ("rts", "")),
        "after-cutscene shared tail return",
    )
    natural["postVictorySetup"] = {"map": 57, "pointer": void_pc}
    names = (
        "ExecuteAfterBattleCutscene",
        "EndAfterBattleCutscene",
        "abcs_battle01",
        "SwitchMap",
        "ExplorationLoop",
        "ClearFlag",
        "SetFlag",
        "GenerateRandomNumber",
        "GenerateRandomOrDebugNumber",
        "WriteBattlesceneScript",
        "ExecuteAiCommand",
        "ControlCursorEntity_ChooseTarget",
        "InitializeBattlescene",
        "ExecuteBattlesceneScript",
        "EndBattlescene",
        "ProcessAfterTurnEffects",
        "ProcessKilledCombatants",
        "CountRemainingCombatants",
        "battlesceneScript_DropEnemyItem",
        "battlesceneScript_End",
        "loc_23186",
        "BattlefieldMenu",
        "ExecuteBattlefieldMagicMenu",
        "SelectSpellLevel",
        "ExecuteBattlefieldItemMenu",
        "CreatePulsatingItemRangeGrid",
        "battlesceneScript_UseItem",
        "battlesceneScript_BreakUsedItem",
        "RemoveItemBySlot",
        "ExecuteBattleaction_Egress",
        "ExecuteBattleaction_AngelWing",
    )
    for name in names:
        address = addresses[name]
        if _h1_bytes(listing, address, 2) != rom[address : address + 2].hex().upper():
            raise ValueError(f"victory callback H1/ROM drift: {name}")
        natural["functions"][name] = address
    # Select the actual menu input read, not the entry before its window animation.
    for name, start, end in (
        ("diamondInputPc", "ExecuteDiamondMenu", "LoadDiamondMenuWindowLayout"),
        ("magicInputPc", "ExecuteBattlefieldMagicMenu", "BuildMagicMenu"),
        ("spellLevelInputPc", "SelectSpellLevel", "sub_10D56"),
        ("itemInputPc", "ExecuteBattlefieldItemMenu", "BuildItemMenu"),
    ):
        reads = []
        for line in listing.splitlines():
            match = re.match(r"^([0-9A-F]{8})\s+((?:[0-9A-F]{4}\s+)+)\s*(btst.*)", line)
            if (
                match
                and addresses[start] <= int(match[1], 16) < addresses[end]
                and "#INPUT_BIT_LEFT,((CURRENT_PLAYER_INPUT" in match[3]
            ):
                pc, data = int(match[1], 16), bytes.fromhex(match[2])
                if rom[pc : pc + len(data)] != data:
                    raise ValueError(f"{name} input H1/ROM drift")
                reads.append(pc)
        if len(reads) != 1:
            raise ValueError(f"{name} input source seam missing/ambiguous")
        natural["functions"][name] = reads[0]
    # loc_23186 waits one VInt, then examines the live target count before input.
    pc = addresses["loc_23186"]
    if (
        rom[pc : pc + 4] != bytes.fromhex("4EB8") + addresses["WaitForVInt"].to_bytes(2, "big")
        or _h1_bytes(listing, pc + 4, 4) != rom[pc + 4 : pc + 8].hex().upper()
    ):
        raise ValueError("target input WaitForVInt seam drift")
    natural["functions"]["targetInputPc"] = pc + 4
    extra = (
        "COMBATANT_OFFSET_EXP",
        "BATTLEACTION_ITEM_OR_SPELL",
        "BATTLEACTION_ITEM_SLOT",
        "BATTLESCENE_ATTACK_TYPE",
        "BATTLESCENE_ACTOR",
        "BATTLESCENE_EXP",
        "BATTLESCENE_GOLD",
        "TARGETS_LIST_LENGTH",
        "TARGETS_LIST",
        "BATTLE_ACTOR_X",
        "BATTLE_ACTOR_Y",
        "BATTLE_TARGET_X",
        "BATTLE_TARGET_Y",
        "BATTLE_ENTITY_CHOSEN_X",
        "BATTLE_ENTITY_CHOSEN_Y",
        "FF4D00_LOADING_SPACE",
        "FF5600_LOADING_SPACE",
        "GAME_FLAGS",
        "MAP_SIZE_MAX_TILEWIDTH",
        "MAP_SIZE_MAX_TILEHEIGHT",
        "MAP_ARRAY_BYTESIZE",
        "INPUT_A",
        "INPUT_B",
        "INPUT_C",
        "INPUT_UP",
        "INPUT_DOWN",
        "INPUT_LEFT",
        "INPUT_RIGHT",
        "NOT_CURRENTLY_IN_BATTLE",
        "DISPLAYED_ICON_1",
        "SPELL_HEAL",
        "MENU_BATTLE_WITH_STAY",
        "MENU_BATTLE_WITH_SEARCH",
        "MENU_ITEM",
        "ITEM_MEDICAL_HERB",
        "ITEMENTRY_MASK_INDEX",
        "ITEMDEF_SIZE",
        "ITEMTYPE_CONSUMABLE",
        "SPELL_HEALIN",
        "SPELLDEF_ENTRY_SIZE",
        "SPELLANIMATION_HEALING_FAIRY",
        "SPELLPROPS_TYPE_HEAL",
        "SPELLPROPS_TARGET_TEAMMATES",
        "BATTLESCENE_ITEM",
    )
    constants = r1._equates(
        sources["sf2const.asm"] + "\n" + sources["sf2enums.asm"],
        tuple(name for name in extra if name != "MAP_ARRAY_BYTESIZE"),
    )
    if not re.search(
        r"^MAP_ARRAY_BYTESIZE:\s+equ\s+MAP_SIZE_MAX_TILEWIDTH\*MAP_SIZE_MAX_TILEHEIGHT\s*(?:;[^\n]*)?$",
        sources["sf2enums.asm"],
        re.MULTILINE,
    ):
        raise ValueError("movement grid size expression drift")
    constants["MAP_ARRAY_BYTESIZE"] = (
        constants["MAP_SIZE_MAX_TILEWIDTH"] * constants["MAP_SIZE_MAX_TILEHEIGHT"]
    )
    config["ram"].update({name: constants[name] for name in extra})
    if (
        config["ram"]["MAP_ARRAY_BYTESIZE"]
        != config["ram"]["FF5600_LOADING_SPACE"] - config["ram"]["FF4D00_LOADING_SPACE"]
    ):
        raise ValueError("original movement grid storage mismatch")
    # ExplorationLoop entry precedes the conditional original no-battle write.
    # Bind its actual byte operand; entry/return bookkeeping alone cannot prove it ran.
    no_battle_writes = [
        int(match[1], 16)
        for line in listing.splitlines()
        if (
            match := re.match(
                r"^([0-9A-F]{8})\s+[0-9A-F ]+\s+move\.b\s+#NOT_CURRENTLY_IN_BATTLE,"
                r"\(\(CURRENT_BATTLE-\$1000000\)\)\.w\s*$",
                line,
            )
        )
        and addresses["ExplorationLoop"] <= int(match[1], 16) < addresses["WaitForEvent"]
    ]
    expected_write = (
        bytes.fromhex("11FC")
        + constants["NOT_CURRENTLY_IN_BATTLE"].to_bytes(2, "big")
        + (config["ram"]["CURRENT_BATTLE"] & 0xFFFF).to_bytes(2, "big")
    )
    if len(no_battle_writes) != 1:
        raise ValueError("ExplorationLoop no-battle write missing/ambiguous")
    no_battle_pc = no_battle_writes[0]
    if (
        _h1_bytes(listing, no_battle_pc, 6) != expected_write.hex().upper()
        or rom[no_battle_pc : no_battle_pc + 6] != expected_write
    ):
        raise ValueError("ExplorationLoop no-battle source/H1/ROM operand drift")
    heal = re.search(
        r"^\s*entry\s+HEAL\s*;[^\n]*\n\s*mpCost\s+(\d+)\s*$",
        sources["data/stats/spells/spelldefs.asm"],
        re.MULTILINE,
    )
    if heal is None:
        raise ValueError("Heal 1 source definition missing/ambiguous")
    heal_bytes = bytes((constants["SPELL_HEAL"], int(heal[1])))
    heal_pc = addresses["table_SpellDefinitions"]
    # These byte data macros have no machine-word row for _h1_bytes; the pinned
    # source fields bind directly to ROM at the H1 table symbol.
    if rom[heal_pc : heal_pc + 2] != heal_bytes:
        raise ValueError("Heal 1 spell/MP cost source/H1/ROM drift")
    # The first item is Medical Herb. Bind its complete definition, then its
    # packed HEALIN-1 spell entry; never infer item use from the display name.
    herb = re.search(
        r"; 0: Medical Herb\s+equipFlags\s+NONE\s+range\s+(\d+),\s*(\d+)\s+"
        r"price\s+(\d+)\s+itemType\s+CONSUMABLE\s+useSpell\s+HEALIN\s+"
        r"equipEffects\s+NONE, 0, &\s+NONE, 0, &\s+NONE, 0",
        sources["data/stats/items/itemdefs.asm"],
    )
    if herb is None or constants["ITEM_MEDICAL_HERB"] != 0:
        raise ValueError("Medical Herb source definition missing/ambiguous")
    herb_bytes = (
        bytes(4)
        + bytes((int(herb[2]), int(herb[1])))
        + int(herb[3]).to_bytes(2, "big")
        + bytes((constants["ITEMTYPE_CONSUMABLE"], constants["SPELL_HEALIN"]))
        + bytes(6)
    )
    herb_pc = addresses["table_ItemDefinitions"]
    if (
        len(herb_bytes) != constants["ITEMDEF_SIZE"]
        or rom[herb_pc : herb_pc + len(herb_bytes)] != herb_bytes
    ):
        raise ValueError("Medical Herb item source/H1/ROM drift")
    spell_source = sources["data/stats/spells/spelldefs.asm"]
    herb_spell = re.search(
        r"^\s*entry\s+HEALIN\s*;[^\n]*\n\s*mpCost\s+(\d+)\s+"
        r"animation\s+HEALING_FAIRY\s+properties\s+TYPE_HEAL\|TARGET_TEAMMATES\s+"
        r"range\s+(\d+),\s*(\d+)\s+radius\s+(\d+)\s+power\s+(\d+)",
        spell_source,
        re.MULTILINE,
    )
    if herb_spell is None:
        raise ValueError("Medical Herb HEALIN-1 source definition missing/ambiguous")
    spell_ordinal = len(
        re.findall(r"^\s*entry\s+", spell_source[: herb_spell.start()], re.MULTILINE)
    )
    herb_spell_pc = heal_pc + spell_ordinal * constants["SPELLDEF_ENTRY_SIZE"]
    herb_spell_bytes = bytes(
        (
            constants["SPELL_HEALIN"],
            int(herb_spell[1]),
            constants["SPELLANIMATION_HEALING_FAIRY"],
            constants["SPELLPROPS_TYPE_HEAL"] | constants["SPELLPROPS_TARGET_TEAMMATES"],
            int(herb_spell[3]),
            int(herb_spell[2]),
            int(herb_spell[4]),
            int(herb_spell[5]),
        )
    )
    if (
        len(herb_spell_bytes) != constants["SPELLDEF_ENTRY_SIZE"]
        or rom[herb_spell_pc : herb_spell_pc + len(herb_spell_bytes)] != herb_spell_bytes
    ):
        raise ValueError("Medical Herb HEALIN-1 source/H1/ROM drift")
    natural["programs"].append(addresses["abcs_battle01"])
    natural["victory"] = {
        "owners": owners,
        "noBattleWritePc": no_battle_pc,
        "heal": {"spell": constants["SPELL_HEAL"], "mpCost": int(heal[1])},
        "herb": {
            "item": constants["ITEM_MEDICAL_HERB"],
            "indexMask": constants["ITEMENTRY_MASK_INDEX"],
            "spell": constants["SPELL_HEALIN"],
            "minRange": int(herb_spell[2]),
            "maxRange": int(herb_spell[3]),
            "power": int(herb_spell[5]),
        },
        "checkpoint": "neutral completed player-movement frame with reconstructible battle returns",
        "terminal": "stable exploration, ordinary direction read/acceptance, settled displacement",
    }


def prepare_map3_observation_candidate(
    rom_path: Path,
    upstream_path: Path,
    *,
    input_path: Path | None = None,
    output_directory: Path,
    proposed_timeout_seconds: int,
    interactive: bool = False,
    continuation: str | None = None,
    segment: int | None = None,
    resume_directory: Path | None = None,
    reviewed_prior_starts: int | None = None,
    reviewed_prior_active_seconds: float | None = None,
    reviewed_prior_delivered_frames: int | None = None,
    reviewed_prior_advancing_batches: int | None = None,
) -> dict[str, Any]:
    """Materialize a private review candidate without starting an emulator.

    The input is an explicit frame table relative to the first admitted wait,
    not a route planner, a replay receipt, or a replacement public golden.
    """
    output = output_directory.resolve()
    if type(proposed_timeout_seconds) is not int or proposed_timeout_seconds <= 0:
        raise ValueError("candidate needs an explicit positive proposed wall-time limit")
    local = repo_path("local").resolve()
    if not output.is_relative_to(local) or output == local or output.exists():
        raise ValueError("candidate output must be a fresh directory beneath this worktree's local")
    limits = _interactive_limits(continuation)
    if continuation == VICTORY_CONTINUATION and segment is None:
        raise ValueError("victory continuation requires explicit savestate-linked accounting")
    parent_pair, parent_metadata = None, None
    if segment is not None:
        if (
            type(segment) is not int
            or segment < 1
            or (segment > 4 and continuation != VICTORY_CONTINUATION)
            or continuation not in (NATURAL_CONTINUATION, VICTORY_CONTINUATION)
            or not interactive
        ):
            raise ValueError(
                "segment requires a valid ordinal and natural interactive continuation"
            )
        if segment != 1 and resume_directory is None:
            raise ValueError("only the first segment may start without a complete parent pair")
        if resume_directory is None:
            if type(reviewed_prior_starts) is not int or reviewed_prior_starts < 0:
                raise ValueError("initial segment requires an explicit reviewed prior-start count")
            if (
                type(reviewed_prior_active_seconds) not in (int, float)
                or not math.isfinite(reviewed_prior_active_seconds)
                or reviewed_prior_active_seconds < 0
            ):
                raise ValueError(
                    "initial segment requires reviewed finite nonnegative prior active seconds"
                )
            for value, label in (
                (reviewed_prior_delivered_frames, "delivered frames"),
                (reviewed_prior_advancing_batches, "advancing batches"),
            ):
                if type(value) is not int or value < 0:
                    raise ValueError(f"initial segment requires reviewed nonnegative prior {label}")
        elif reviewed_prior_starts is not None:
            raise ValueError("resumed historical starts come only from the parent receipt")
        elif reviewed_prior_active_seconds is not None:
            raise ValueError("resumed active seconds come only from the parent pair")
        elif (
            reviewed_prior_delivered_frames is not None
            or reviewed_prior_advancing_batches is not None
        ):
            raise ValueError("resumed frame/batch consumption comes only from the parent pair")
        if resume_directory is not None:
            resume_directory = resume_directory.resolve(strict=True)
            parent_pair, parent_metadata = _read_segment(resume_directory)
            if parent_pair["ordinal"] != segment - 1 and not (
                segment == parent_pair["ordinal"] == 1 and parent_metadata["checkpoint"]["rank"] < 5
            ):
                raise ValueError("segment skips or repeats its parent")
            totals, _ = _resume_accounting(resume_directory)
            reviewed_prior_starts = totals["starts"]
            reviewed_prior_active_seconds = totals["seconds"]
            reviewed_prior_delivered_frames = totals["frames"]
            reviewed_prior_advancing_batches = totals["batches"]
    elif resume_directory is not None:
        raise ValueError("resume requires an explicit segment")
    elif reviewed_prior_starts is not None:
        raise ValueError("reviewed prior-start count requires explicit segmented acquisition")
    elif reviewed_prior_active_seconds is not None:
        raise ValueError("reviewed prior active seconds require explicit segmented acquisition")
    elif (
        reviewed_prior_delivered_frames is not None or reviewed_prior_advancing_batches is not None
    ):
        raise ValueError("reviewed frame/batch consumption requires explicit segmented acquisition")
    if continuation and not interactive:
        raise ValueError("natural continuation requires explicit interactive acquisition")
    if interactive and (
        input_path is not None or proposed_timeout_seconds != limits["wallSeconds"]
    ):
        raise ValueError(
            "interactive preparation requires no frozen input and the selected wall limit"
        )
    if not interactive and (input_path is None or not input_path.is_file()):
        raise FileNotFoundError(
            "candidate unavailable: explicit non-adaptive input table is missing"
        )
    input_bytes = (
        (
            json.dumps(
                {
                    "clock": "first-r1-wait-next-frame",
                    "provenance": "interactive-acquisition",
                    "frames": [],
                },
                indent=2,
            )
            + "\n"
        ).encode()
        if interactive
        else input_path.read_bytes()
    )
    trace = json.loads(input_bytes)
    if not isinstance(trace, dict) or set(trace) != {"clock", "provenance", "frames"}:
        raise ValueError("candidate input requires only clock, provenance and frames")
    if trace["clock"] != "first-r1-wait-next-frame" or trace["provenance"] != (
        "interactive-acquisition" if interactive else "diagnostic-parameters"
    ):
        raise ValueError("candidate input clock/provenance is not the declared controlled start")
    frames = trace["frames"]
    if (
        not isinstance(frames, list)
        or not (len(frames) == 0 if interactive else 1 <= len(frames) <= 36000)
        or any(
            type(button) is not str
            or button not in {"", "Up", "Down", "Left", "Right", "A", "B", "C"}
            for button in frames
        )
    ):
        raise ValueError(
            "candidate input must contain 1..36000 explicit single-button/neutral frames"
        )
    fixture = load_json(FIXTURE)
    contract = build_map3_messenger_acceptance_source_contract(rom_path, upstream_path)
    _assert_fixture(fixture, contract)
    _assert_lua_roles()
    toolchain, executable = bizhawk_contract()
    executable_hash = sha256(executable.read_bytes()).hexdigest().upper()
    if (
        executable.stat().st_size != toolchain["executableSizeBytes"]
        or executable_hash != toolchain["executableSha256"]
    ):
        raise ValueError("candidate BizHawk executable identity drift")
    validate_lua_syntax(OBSERVER, executable)
    disasm = upstream_path / DISASM
    listing = (upstream_path / LISTING).read_text(encoding="utf-8")
    addresses = listing_symbol_addresses(listing)
    rom = rom_path.read_bytes()
    sources = {
        name: (disasm / name).read_text(encoding="utf-8")
        for name in (
            "sf2enums.asm",
            "sf2mapmacros.asm",
            "data/maps/entries/map03/6-warp-events.asm",
            "code/gameflow/exploration/explorationfunctions_2.asm",
            "data/maps/entries/map03/mapsetups/s3_zoneevents.asm",
            "data/maps/entries/map03/mapsetups/scripts_1.asm",
            "data/maps/entries/map19/mapsetups/s6_initfunction.asm",
            "code/common/scripting/map/mapscriptengine_1.asm",
            "code/common/scripting/text/textfunctions_1.asm",
        )
    }
    if interactive:
        name = "code/common/menus/yesnoprompt.asm"
        sources[name] = (disasm / name).read_text(encoding="utf-8")
    for name, source in sources.items():
        pinned = subprocess.run(
            ["git", "-C", str(upstream_path), "show", f"{r1.UPSTREAM_COMMIT}:disasm/{name}"],
            check=True,
            capture_output=True,
            text=True,
            encoding="utf-8",
        ).stdout
        if source != pinned:
            raise ValueError(f"candidate pinned source differs: {name}")
    _require_order(
        _section(sources["data/maps/entries/map03/mapsetups/s3_zoneevents.asm"], "Map3_ZoneEvent4"),
        (
            ("chkflg", "600"),
            ("bne.s", "byte_50e32"),
            ("chkflg", "604"),
            ("bne.s", "return_50e42"),
            ("script", "cs_51652"),
            ("setflg", "604"),
            ("rts", ""),
        ),
        "candidate gate branch",
    )
    gate = _stream(sources["data/maps/entries/map03/mapsetups/scripts_1.asm"], "cs_51652")
    _require_order(
        [(op["opcode"], op["operand"]) for op in gate],
        (
            ("textcursor", "537"),
            ("entityactions", "138"),
            ("moveright", "1"),
            ("entityactionswait", "139"),
            ("moveleft", "1"),
            ("entityactions", "138"),
            ("moveleft", "1"),
            ("entityactionswait", "139"),
            ("moveright", "1"),
            ("csc_end", ""),
        ),
        "candidate non-awaited/awaited guards",
    )
    _require_order(
        _section(
            sources["code/common/scripting/map/mapscriptengine_1.asm"],
            "csc14_setEntityActscriptManual",
        ),
        (
            ("move.b", "(a6)+,d0"),
            ("bsr.w", "getentityaddressfromcharacter"),
            ("move.b", "(a6)+,d0"),
            ("move.l", "a6,entitydef_offset_actscriptaddr(a5)"),
            ("tst.b", "d0"),
            ("beq.w", "loc_46970"),
            ("cmpi.l", "#eas_idle,entitydef_offset_actscriptaddr(a5)"),
            ("bne.s", "loc_46966"),
            ("cmpi.w", "#$8080,(a6)+"),
            ("rts", ""),
        ),
        "candidate original entity wait",
    )
    symbols = (
        "Map3_ZoneEvent4",
        "byte_50E32",
        "return_50E42",
        "cs_51652",
        "cs_516A8",
        "ms_map19_InitFunction",
        "ms_map19_flag_501_InitFunction",
        "cs_53104",
        "csc14_setEntityActscriptManual",
        "loc_46966",
        "loc_46970",
        "eas_Idle",
        "DisplayText",
        "loc_62FE",
        "symbol_wait1",
        "loc_65B4",
        "loc_62CA",
        "CloseDialogueWindow",
        "FieldMenu",
        "loc_2593C",
    )
    if interactive:
        symbols += ("loc_6472", "loc_1530C", "loc_15314")
    missing = sorted(set(symbols) - set(addresses))
    if missing:
        raise ValueError(f"candidate unavailable: H1 symbols missing: {missing}")
    # H1 emitted bytes, not fixture guesses, bind every added entry callback.
    functions = {name: addresses[name] for name in symbols}
    for name, address in functions.items():
        if _h1_bytes(listing, address, 2) != rom[address : address + 2].hex().upper():
            raise ValueError(f"candidate H1/ROM callback drift: {name}")
    branch = addresses["byte_50E32"]
    end = addresses["return_50E42"]
    # ASM68K's listing leaves PC-relative relocations unresolved. Bind its
    # instruction locations/opcodes and independently decode final ROM operands.
    span = rom[branch : end + 2]
    if (
        len(span) != 18
        or span[:5].hex().upper() != "4E41025C66"
        or span[6:8] != b"\x41\xfa"
        or span[10:12] != b"\x4e\x46"
    ):
        raise ValueError("candidate original chkFlg/branch/LEA/script macro drift")
    if (
        branch + 6 + int.from_bytes(span[5:6], "big", signed=True) != end
        or branch + 8 + int.from_bytes(span[8:10], "big", signed=True) != addresses["cs_51652"]
    ):
        raise ValueError("candidate gate branch/script ROM target drift")
    commit = branch + 12
    if (
        _h1_bytes(listing, commit, 6) != "4E42025C4E75"
        or rom[commit : end + 2].hex().upper() != "4E42025C4E75"
    ):
        raise ValueError("candidate original F604 setFlg/RTS seam drift")
    functions["gateCommit"] = commit
    config = _observer_config(fixture, contract)
    ram_names = (
        "COMBATANT_OFFSET_STATUSEFFECTS",
        "ENTITIES_COUNTER",
        "ENTITYDEF_OFFSET_ACTSCRIPTADDR",
        "ENTITYDEF_OFFSET_ACTSCRIPTWAITTIMER",
        "CURRENTLY_TYPEWRITING",
        "WINDOW_IS_PRESENT",
        "RANDOM_SEED",
        "RANDOM_SEED_COPY",
        "FRAME_COUNTER",
        "SECONDS_COUNTER",
        "SECONDS_COUNTER_FRAMES",
        "COMBATANT_OFFSET_CLASS",
        "COMBATANT_OFFSET_LEVEL",
        "COMBATANT_OFFSET_HP_MAX",
        "COMBATANT_OFFSET_HP_CURRENT",
        "COMBATANT_OFFSET_MP_MAX",
        "COMBATANT_OFFSET_MP_CURRENT",
        "COMBATANT_OFFSET_ATT_CURRENT",
        "COMBATANT_OFFSET_DEF_CURRENT",
        "COMBATANT_OFFSET_AGI_CURRENT",
        "COMBATANT_OFFSET_MOV_CURRENT",
        "COMBATANT_OFFSET_ITEM_0",
        "COMBATANT_OFFSET_SPELLS",
        "STATUSEFFECT_POISON",
        "FORCEMEMBER_JOINED_FLAGS_START",
        "FORCEMEMBER_ACTIVE_FLAGS_START",
        "FLAG_INDEX_DIFFICULTY1",
        "FLAG_INDEX_DIFFICULTY2",
        "CURRENT_PORTRAIT",
        "CURRENT_SPEECH_SFX",
    )
    equates = (
        (disasm / "sf2const.asm").read_text(encoding="utf-8")
        + "\n"
        + (disasm / "sf2enums.asm").read_text(encoding="utf-8")
    )
    config["ram"].update(r1._equates(equates, ram_names))
    config["cases"] = [{**EXPECTED_CASES[0], "frameBudget": len(frames) + 3600}]
    castle_fixture = repo_path("tests/fixtures/h2/map3-castle-battle-unlock-static-v1.json")
    castle = load_json(castle_fixture)["static"]
    castle_segments = castle["routeGraph"]["segments"]
    config["candidate"] = {
        **_candidate_warps(config, disasm, sources, addresses, listing, rom, castle),
        "functions": functions,
        "frames": frames,
        "inputIdentity": sha256(input_bytes).hexdigest().upper(),
        "clock": trace["clock"],
        "provenance": trace["provenance"],
        "checkpointPath": (output / "checkpoints.jsonl").as_posix(),
        "admission": load_json(R1_FIXTURE)["expectedObservation"]["records"][0]["scenarioState"],
        "northWarp": castle_segments[3],
        "gatePoint": castle_segments[1]["point"],
    }
    if continuation:
        config["candidate"]["natural"] = _natural_configuration(
            rom_path, upstream_path, config, sources, addresses, listing, rom, castle
        )
        if continuation == VICTORY_CONTINUATION:
            _victory_configuration(upstream_path, config, sources, addresses, listing, rom)
    if interactive:
        from sf2tool.bizhawk_debug_bridge import SCRIPT

        validate_lua_syntax(SCRIPT, executable)
        config["candidate"]["interactive"] = {
            **limits,
            "bridgePath": SCRIPT.as_posix(),
            "inputLogPath": (output / "actual-inputs.jsonl").as_posix(),
        }
        config["cases"][0]["frameBudget"] = (
            limits["totalFrames"] - config["r1"]["harness"]["bootstrapFrameBudget"]
        )
    if segment is not None:
        config["candidate"]["segment"] = {
            "ordinal": segment,
            "priorActiveSeconds": reviewed_prior_active_seconds,
            "priorFrames": reviewed_prior_delivered_frames,
            "priorBatches": reviewed_prior_advancing_batches,
        }
        # Historical consumption is observational; never shorten a new bootstrap
        # watchdog or forge acquired frame/R1 clocks to carry that consumption.
    config["outputPath"] = (output / "observed.json").as_posix()
    config["statusPath"] = (output / "status.txt").as_posix()
    config_bytes = (json.dumps(config, indent=2) + "\n").encode("utf-8")
    report = {
        "Status": "CANDIDATE-PREPARED-NOT-ADMITTED",
        "EmulatorLaunches": 0,
        "RuntimeGates": "NOT RUN / pending independent method and lineage-budget admission",
        "RomSha256": r1.CANONICAL_ROM_SHA256,
        "SourceCommit": r1.UPSTREAM_COMMIT,
        "CastleFixtureSha256": sha256(castle_fixture.read_bytes()).hexdigest().upper(),
        "H1ListingSha256": sha256((upstream_path / LISTING).read_bytes()).hexdigest().upper(),
        "RetainedFixtures": {
            key: contract["retained"][key]
            for key in ("r1FixtureSha256", "r2FixtureSha256", "projectionSha256")
        },
        "ObserverSha256": sha256(OBSERVER.read_bytes()).hexdigest().upper(),
        "RunnerSha256": sha256(Path(__file__).read_bytes()).hexdigest().upper(),
        "ExecutionSources": _candidate_execution_sources(interactive),
        "ExecutableSha256": executable_hash,
        "LuaLibrarySha256": sha256((executable.parent / "dll/lua54.dll").read_bytes())
        .hexdigest()
        .upper(),
        "ConfigurationSha256": sha256(config_bytes).hexdigest().upper(),
        "InputSha256": config["candidate"]["inputIdentity"],
        "InputFrames": len(frames),
        "ProposedWallTimeoutSeconds": proposed_timeout_seconds,
        "TotalFrameBudget": config["r1"]["harness"]["bootstrapFrameBudget"]
        + config["cases"][0]["frameBudget"],
        "InputClock": trace["clock"],
        "InputProvenance": trace["provenance"],
        "Start": "controlled R1 bootstrap; original services restored at first WaitForEvent",
        "Terminal": "first Map19 player controller after original gate/F604/warp/init returns",
        "Functions": functions,
        "SourceHashes": {
            name: sha256(value.encode()).hexdigest().upper() for name, value in sources.items()
        },
        "RemainingUnknowns": [
            "unshimmed input timing and natural reach",
            "inherited POISON and live NPC state",
            "runtime callback/cleanup compatibility",
        ],
    }
    if interactive:
        report.update(
            Mode="interactive-acquisition",
            HistoricalControlledStarts=2,
            FutureControlledOrdinal=3,
            MaximumAdditionalStarts=1,
            InputIdentityMeaning="mode declaration; actual input not yet acquired",
            InputBatchesLimit=2048,
            InputBatchFramesLimit=120,
        )
    if continuation:
        report.update(
            Continuation=continuation,
            HistoricalControlledStarts=3,
            FutureControlledOrdinal=4,
            MaximumAdditionalStarts=0,
            InputBatchesLimit=limits["maxBatches"],
            ProposedLimits=limits,
            Terminal=(
                "ordinary input and settled field displacement after natural Battle01 victory"
                if continuation == VICTORY_CONTINUATION
                else "first natural Battle01 player-ready at 0x22E70"
            ),
            RuntimeAuthorization="NONE; independent acceptance and fresh approval required",
        )
    if segment is not None:
        if parent_pair:
            parent_report = load_json(resume_directory / "candidate.json")
            if any(report[key] != parent_report[key] for key in SEGMENT_IDENTITIES):
                raise ValueError("segment ROM/source/observer/tool lineage identity drift")
        report.update(
            Segment={
                **config["candidate"]["segment"],
                "parentDirectory": resume_directory.as_posix() if resume_directory else None,
                "parentPairSha256": sha256(
                    (resume_directory / "runtime/segment-pair.json").read_bytes()
                )
                .hexdigest()
                .upper()
                if resume_directory
                else None,
            },
            HistoricalControlledStarts=reviewed_prior_starts,
            FutureControlledOrdinal=reviewed_prior_starts + 1,
            MaximumAdditionalStarts=None,
            RuntimeAuthorization=(
                "Issue515; accepted source and concrete preparation required before native"
                if continuation == VICTORY_CONTINUATION
                else "Issue485 user-directed stabilization; independent integration required"
            ),
            Start="native parent savestate; no R1 bootstrap" if parent_pair else report["Start"],
            Terminal="forward battle checkpoint or post-victory ordinary input effect"
            if continuation == VICTORY_CONTINUATION and segment >= 4
            else (
                "settled Map19 next-tile field control",
                "returned royal field control",
                "neutral Map21(5,15)/Down guard wait",
                "first natural player-ready",
            )[segment - 1],
        )
    # All validation precedes materialization; no shared launch helper is invoked.
    output.mkdir()
    (output / "input.json").write_bytes(input_bytes)
    (output / "config.json").write_bytes(config_bytes)
    lua = output / "config.lua"
    lua.write_text("return " + _lua_literal(config) + "\n", encoding="utf-8")
    validate_lua_syntax(lua, executable)
    (output / "candidate.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    return report


def run_map3_observation_candidate(
    rom_path: Path,
    candidate_directory: Path,
    *,
    interactive: bool = False,
    continuation: str | None = None,
    segment: int | None = None,
) -> dict[str, Any]:
    """Future admitted execution composition; NOT authorized by preparation.

    There is deliberately no CLI wiring or call from prepare/preflight. Independent
    method and old-lineage/budget admission must precede invoking this function.
    """
    directory = candidate_directory.resolve(strict=True)
    if not directory.is_relative_to(repo_path("local").resolve()):
        raise ValueError("candidate must remain in the owning worktree's ignored local directory")
    # Reserve the attempt before validation, retaining pre-process failures too.
    # A collision never overwrites or cleans up an earlier attempt.
    runtime = directory / "runtime"
    runtime.mkdir()
    session = runtime / "session.bin"
    canonical: str | None = None
    diagnostic: dict[str, Any] = {
        "kind": "candidate-host-status",
        "status": "FAIL",
        "stage": "identity",
        "process": {
            "started": False,
            "pid": None,
            "returncode": None,
            "stdout": None,
            "stderr": None,
            "timed_out": None,
            "process_terminated": None,
            "timeout_tree_killed": None,
            "error": None,
        },
        "launch": None,
        "sessionRomDeleted": None,
        "canonicalRomUnchanged": None,
        "artifacts": {
            "luaStatus": "observer.status.txt",
            "checkpoints": "checkpoints.jsonl",
            "observation": "observer.observed.json",
        },
    }

    def persist() -> None:
        (runtime / "host-status.json").write_text(
            json.dumps(diagnostic, indent=2) + "\n", encoding="utf-8"
        )

    def prepared(launch: dict[str, Any]) -> None:
        diagnostic["launch"] = launch
        identities = {
            "ExecutableSha256": Path(launch["executable"]),
            "LuaLibrarySha256": Path(launch["executable"]).parent / "dll/lua54.dll",
            "RuntimeSettingsSha256": Path(launch["config"]),
            "RuntimeObserverConfigSha256": Path(launch["observerConfig"]),
        }
        diagnostic["runtimeIdentities"] = {
            key: sha256(path.read_bytes()).hexdigest().upper() for key, path in identities.items()
        }
        persist()
        for key in ("ExecutableSha256", "LuaLibrarySha256"):
            if diagnostic["runtimeIdentities"][key] != report[key]:
                raise ValueError(f"candidate runtime-copy identity drift: {key}")

    def started(pid: int) -> None:
        diagnostic["stage"] = "process-started"
        diagnostic["process"].update(started=True, pid=pid)
        persist()

    def timed_out() -> None:
        diagnostic["stage"] = "process-timeout"
        diagnostic["process"]["timed_out"] = True
        persist()

    def completed(result: NativeProcessResult) -> None:
        diagnostic["process"] = asdict(result)
        diagnostic["stage"] = "process-ended"
        persist()

    persist()
    try:
        report = load_json(directory / "candidate.json")
        diagnostic["reviewedMaterial"] = report
        selection = report.get("Segment")
        if continuation == VICTORY_CONTINUATION and selection is None:
            raise ValueError("victory execution requires reviewed segmented accounting")
        if (selection is None) != (segment is None) or (
            selection and (type(segment) is not int or selection["ordinal"] != segment)
        ):
            raise ValueError("execution segment must explicitly match prepared material")
        if selection and (
            type(report["HistoricalControlledStarts"]) is not int
            or report["HistoricalControlledStarts"] < 0
            or report["FutureControlledOrdinal"] != report["HistoricalControlledStarts"] + 1
        ):
            raise ValueError("segment reviewed prior-start accounting mismatch")
        expected_limits = _interactive_limits(continuation)
        if selection and (
            type(selection["priorActiveSeconds"]) not in (int, float)
            or not math.isfinite(selection["priorActiveSeconds"])
            or selection["priorActiveSeconds"] < 0
        ):
            raise ValueError("segment requires reviewed finite nonnegative prior active seconds")
        if selection:
            for key in ("priorFrames", "priorBatches"):
                if type(selection[key]) is not int or selection[key] < 0:
                    raise ValueError(f"invalid reviewed segment {key}")
        if report.get("Continuation") != continuation or (continuation and not interactive):
            raise ValueError("execution continuation must explicitly match preparation")
        if (report.get("Mode") == "interactive-acquisition") != interactive:
            raise ValueError("execution mode must explicitly match reviewed preparation")
        if _candidate_execution_sources(interactive) != report["ExecutionSources"]:
            raise ValueError("frozen candidate execution helper/bootstrap identity drift")
        _, executable = bizhawk_contract()
        for path, field in (
            (OBSERVER, "ObserverSha256"),
            (Path(__file__), "RunnerSha256"),
            (executable, "ExecutableSha256"),
            (executable.parent / "dll/lua54.dll", "LuaLibrarySha256"),
            (directory / "config.json", "ConfigurationSha256"),
            (directory / "input.json", "InputSha256"),
        ):
            if sha256(path.read_bytes()).hexdigest().upper() != report[field]:
                raise ValueError(f"frozen candidate identity drift: {field}")
        canonical = inspect_rom(rom_path)["sha256"]
        if canonical != report["RomSha256"] or canonical != r1.CANONICAL_ROM_SHA256:
            raise ValueError("candidate canonical ROM identity drift")
        timeout_seconds = report["ProposedWallTimeoutSeconds"]
        if type(timeout_seconds) is not int or timeout_seconds <= 0:
            raise ValueError("candidate is missing its reviewed positive wall-time limit")
        config = load_json(directory / "config.json")
        parent_pair = None
        if selection:
            if (
                continuation not in (NATURAL_CONTINUATION, VICTORY_CONTINUATION)
                or not interactive
                or segment < 1
                or (segment > 4 and continuation != VICTORY_CONTINUATION)
            ):
                raise ValueError("invalid segmented composition")
            settings = config["candidate"]["segment"]
            if (
                type(settings["priorActiveSeconds"]) not in (int, float)
                or type(settings["priorFrames"]) is not int
                or type(settings["priorBatches"]) is not int
                or settings
                != {
                    key: selection[key]
                    for key in ("ordinal", "priorActiveSeconds", "priorFrames", "priorBatches")
                }
            ):
                raise ValueError("segment configuration/accounting drift")
            settings.update(
                statePath=(runtime / "segment.State").as_posix(),
                metadataPath=(runtime / "continuation.json").as_posix(),
            )
            if selection["parentDirectory"] is not None:
                parent = Path(selection["parentDirectory"])
                parent_pair, metadata = _read_segment(parent)
                totals, attempts = _resume_accounting(parent)
                if (
                    sha256((parent / "runtime/segment-pair.json").read_bytes()).hexdigest().upper()
                    != selection["parentPairSha256"]
                    or not (
                        parent_pair["ordinal"] == segment - 1
                        or (
                            segment == parent_pair["ordinal"] == 1
                            and metadata["checkpoint"]["rank"] < 5
                        )
                    )
                    or report["HistoricalControlledStarts"] != totals["starts"]
                    or totals["seconds"] != settings["priorActiveSeconds"]
                    or totals["frames"] != settings["priorFrames"]
                    or totals["batches"] != settings["priorBatches"]
                    or any(
                        report[key] != load_json(parent / "candidate.json")[key]
                        for key in SEGMENT_IDENTITIES
                    )
                ):
                    raise ValueError("parent segment identity/accounting drift")
                metadata["observer"]["order"] = parent_pair["lastOrder"]
                settings.update(
                    resume=metadata, loadPath=(parent / "runtime/segment.State").as_posix()
                )
                # Exclusive attempts preserve failed branches and their real costs.
                name = "resumed-by.json" if attempts == 0 else f"resumed-by-{attempts + 1:04}.json"
                with (parent / "runtime" / name).open("x", encoding="utf-8") as claim:
                    claim.write(
                        json.dumps({"candidate": directory.as_posix(), "ordinal": segment}) + "\n"
                    )
            elif selection["parentDirectory"] is not None:
                raise ValueError("initial segment has unexpected parent accounting")
        if config["candidate"]["frames"] != load_json(directory / "input.json")["frames"]:
            raise ValueError("candidate frame table differs from its frozen input")
        config["candidate"]["checkpointPath"] = (runtime / "checkpoints.jsonl").as_posix()
        if interactive:
            limits = config["candidate"]["interactive"]
            if (
                timeout_seconds != expected_limits["wallSeconds"]
                or any(limits.get(key) != value for key, value in expected_limits.items())
                or config["candidate"]["frames"]
            ):
                raise ValueError("interactive bounds or empty input declaration drift")
            limits["inputLogPath"] = (runtime / "actual-inputs.jsonl").as_posix()
            diagnostic["artifacts"]["actualInputs"] = "actual-inputs.jsonl"
        diagnostic["stage"] = "session-copy"
        persist()
        shutil.copy2(rom_path, session)
        diagnostic["stage"] = "observer-preparation"
        persist()
        if interactive:
            from sf2tool.bizhawk_debug_bridge import DebugBridge

            config.update(
                bootstrap=runtime_bootstrap(OBSERVER),
                bootstrapLibraryPath=BOOTSTRAP_LIBRARY.as_posix(),
                outputPath=(runtime / "observer.observed.json").as_posix(),
                statusPath=(runtime / "observer.status.txt").as_posix(),
            )
            config_path = runtime / "observer.config.lua"
            config_path.write_text("return " + _lua_literal(config) + "\n", encoding="utf-8")
            bridge = DebugBridge(runtime / "bridge", timeout=60)
            try:
                with bridge:
                    hello = bridge.start(
                        observer=OBSERVER,
                        observer_config=config_path,
                        rom_path=session,
                        wall_seconds=expected_limits["wallSeconds"],
                        acquisition_limits=expected_limits if continuation else None,
                        prior_active_seconds=selection["priorActiveSeconds"] if selection else 0,
                        historical_starts=report["HistoricalControlledStarts"]
                        if selection
                        else None,
                        expected_settings=parent_pair["runtimeSettingsSha256"]
                        if parent_pair and "continuationSettingsIdentity" not in parent_pair
                        else None,
                        expected_continuation_settings=parent_pair.get("continuationSettingsIdentity")
                        if parent_pair
                        else None,
                    )
                    if hello["system"] != "GEN" or hello["version"] != "2.11.1":
                        raise ValueError("interactive runtime identity drift")
                    started(bridge.process.pid)
                    print(json.dumps(hello["state"]), flush=True)
                    bridge.interact()
            finally:
                diagnostic["bridge"] = bridge.receipt
                if parent_pair and bridge.receipt.get("started"):
                    diagnostic["offlineGapSeconds"] = (
                        bridge.receipt["startedAtUnix"] - parent_pair["endedAtUnix"]
                    )
                diagnostic["launch"] = bridge.receipt.get("launch")
                diagnostic["process"].update(
                    started=bridge.receipt["started"],
                    pid=bridge.receipt.get("pid"),
                    returncode=bridge.receipt.get("returncode"),
                    timed_out=bridge.receipt.get("timedOut"),
                    process_terminated=bridge.receipt.get("processTerminated"),
                    forcedTermination=bridge.receipt.get("forcedTermination"),
                )
                persist()
            if (
                bridge.receipt["outcome"] != "completed"
                or bridge.receipt.get("forcedTermination")
                or (bridge.receipt.get("luaStatus") or {}).get("state") != "closed"
            ):
                raise RuntimeError("interactive process/transport cleanup failed")
            observed = load_json(runtime / "observer.observed.json")
        else:
            observed = run_observer(
                rom_path=session,
                observer_path=OBSERVER,
                config=config,
                output_name=(runtime / "observer").as_posix(),
                timeout_seconds=timeout_seconds,
                on_launch=prepared,
                on_started=started,
                on_timeout=timed_out,
                on_result=completed,
            )
        diagnostic["stage"] = "status-and-terminal"
        # The candidate has private phase/role diagnostics, not the legacy
        # fixture's closed failure enum. Reuse the shared terminal protocol.
        lines = (runtime / "observer.status.txt").read_text(encoding="utf-8").splitlines()
        if (
            "milestone:observer-started" not in lines
            or any(line.startswith("failure:") for line in lines)
            or tuple(lines[-len(SUCCESS_STATUS_TAIL) :]) != SUCCESS_STATUS_TAIL
        ):
            raise RuntimeError("candidate callback/terminal status did not complete cleanly")
        if observed.get("kind") != "bounded-original-observation" or (
            not continuation and observed["terminal"]["map"] != 19
        ):
            raise ValueError("candidate terminal output differs from its selected boundary")
        if (
            not observed["restoration"]["callbacksCleared"]
            or not observed["restoration"]["sessionStateRestored"]
        ):
            raise ValueError("candidate cleanup did not complete")
        diagnostic["status"] = "OBSERVATION-COMPLETE-UNREVIEWED"
        if continuation:
            reason = observed.get("stopReason")
            diagnostic["stopReason"] = reason
            if reason == "out-of-scope-before-player-ready":
                diagnostic["status"] = "OUT-OF-SCOPE-BEFORE-PLAYER-READY"
            elif (
                selection
                and (segment < 4 or continuation == VICTORY_CONTINUATION)
                and reason == "segment-saved"
            ):
                diagnostic["status"] = "SEGMENT-SAVED-UNREVIEWED"
            elif continuation == VICTORY_CONTINUATION:
                if reason != "controllable-5b":
                    diagnostic["status"] = "INCOMPLETE-OBSERVATION"
                elif (
                    not observed["terminal"]["flags"]["501"]
                    or observed["terminal"]["flags"]["401"]
                    or observed["terminal"].get("battle")
                    != config["ram"]["NOT_CURRENTLY_IN_BATTLE"]
                ):
                    diagnostic["status"] = "FAIL"
                    raise ValueError("natural victory terminal flag/no-battle mismatch")
                else:
                    facts = observed["terminal"]["stop"]["facts"]
                    before, applied = facts["beforeInput"], facts["input"]
                    poll, accepted = applied["poll"], applied["acceptance"]
                    direction = applied["button"]
                    if (
                        direction not in ("Up", "Down", "Left", "Right")
                        or poll["value"] != config["ram"][f"INPUT_{direction.upper()}"]
                        or not before["completedFrame"] < applied["frame"] < facts["firstFrame"]
                        or poll["frame"] != applied["frame"]
                        or accepted["frame"] != applied["frame"]
                        or poll["pc"] != config["functions"]["esc02_controlCharacter"]
                        or accepted["pc"] != config["functions"]["loc_52E8"]
                        or any(
                            facts["state"][key] != observed["terminal"][key]
                            for key in ("map", "rawX", "rawY", "facing", "battle", "flags")
                        )
                        or before["state"]["map"] != facts["state"]["map"]
                        or (before["state"]["rawX"], before["state"]["rawY"])
                        == (facts["state"]["rawX"], facts["state"]["rawY"])
                        or (before["state"]["rawX"] + accepted["d4"]) & 0xFFFF
                        != facts["state"]["rawX"]
                        or (before["state"]["rawY"] + accepted["d5"]) & 0xFFFF
                        != facts["state"]["rawY"]
                        or any(
                            point["pending"] != 0
                            or any(point["consumers"].values())
                            or point["completedFrame"] != point["firstFrame"] + 1
                            for point in (before, facts)
                        )
                    ):
                        diagnostic["status"] = "FAIL"
                        raise ValueError("post-victory ordinary input/effect mismatch")
            elif reason != "player-ready":
                diagnostic["status"] = "INCOMPLETE-OBSERVATION"
            elif observed["terminal"]["map"] != 57:
                raise ValueError("natural player-ready map mismatch")
    except BaseException as error:
        process = diagnostic["process"]
        diagnostic["failureKind"] = (
            "timeout"
            if process["timed_out"] is True
            else "started-failure"
            if process["started"]
            else "pre-process-failure"
        )
        diagnostic["errorType"], diagnostic["error"] = type(error).__name__, str(error)
        raise
    finally:
        # Attempt both cleanup checks independently, preserving the primary failure
        # and available Lua status/checkpoints even when either cleanup step fails.
        cleanup_errors = []
        try:
            session.unlink(missing_ok=True)
            diagnostic["sessionRomDeleted"] = not session.exists()
        except Exception as error:
            diagnostic["sessionRomDeleted"] = not session.exists()
            cleanup_errors.append(f"session removal: {type(error).__name__}: {error}")
        try:
            if canonical is not None:
                diagnostic["canonicalRomUnchanged"] = inspect_rom(rom_path)["sha256"] == canonical
                if not diagnostic["canonicalRomUnchanged"]:
                    raise ValueError("canonical ROM changed during candidate execution")
        except Exception as error:
            cleanup_errors.append(f"canonical check: {type(error).__name__}: {error}")
        diagnostic["artifactsPresent"] = {
            key: (runtime / name).is_file() for key, name in diagnostic["artifacts"].items()
        }
        if cleanup_errors:
            diagnostic["status"], diagnostic["cleanupErrors"] = "FAIL", cleanup_errors
        persist()
        if cleanup_errors and "error" not in diagnostic:
            raise RuntimeError("; ".join(cleanup_errors))
    if segment is not None and diagnostic["status"] in (
        "SEGMENT-SAVED-UNREVIEWED",
        "OBSERVATION-COMPLETE-UNREVIEWED",
    ):
        try:
            _seal_segment(directory, report, diagnostic)
        except BaseException as error:
            diagnostic.update(
                status="FAIL",
                stage="segment-pair-publication",
                errorType=type(error).__name__,
                error=str(error),
            )
            persist()
            raise
    return {
        "Status": diagnostic["status"],
        "StopReason": diagnostic.get("stopReason"),
        "EmulatorLaunches": int(diagnostic["process"]["started"]),
        "SessionRomDeleted": diagnostic["sessionRomDeleted"],
        "H4": "not established",
    }
