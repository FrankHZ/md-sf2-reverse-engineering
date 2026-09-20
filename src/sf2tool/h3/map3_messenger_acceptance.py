"""Bounded H3 confirmation of the Map 3 messenger acceptance body."""

from __future__ import annotations

import json
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
from sf2tool.h3.bootstrap import BOOTSTRAP_LIBRARY
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


def _candidate_execution_sources() -> dict[str, str]:
    """Bind the executed launch/bootstrap helpers, beyond the observer/runner."""
    return {
        name: sha256(repo_path(name).read_bytes()).hexdigest().upper()
        for name in (
            "src/sf2tool/h3/bizhawk.py",
            "src/sf2tool/h3/bootstrap.py",
            "src/sf2tool/toolchain.py",
            BOOTSTRAP_LIBRARY.relative_to(repo_path(".")).as_posix(),
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


def prepare_map3_observation_candidate(
    rom_path: Path,
    upstream_path: Path,
    *,
    input_path: Path,
    output_directory: Path,
    proposed_timeout_seconds: int,
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
    if not input_path.is_file():
        raise FileNotFoundError(
            "candidate unavailable: explicit non-adaptive input table is missing"
        )
    input_bytes = input_path.read_bytes()
    trace = json.loads(input_bytes)
    if not isinstance(trace, dict) or set(trace) != {"clock", "provenance", "frames"}:
        raise ValueError("candidate input requires only clock, provenance and frames")
    if (
        trace["clock"] != "first-r1-wait-next-frame"
        or trace["provenance"] != "diagnostic-parameters"
    ):
        raise ValueError("candidate input clock/provenance is not the declared controlled start")
    frames = trace["frames"]
    if (
        not isinstance(frames, list)
        or not 1 <= len(frames) <= 36000
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
        "ExecutionSources": _candidate_execution_sources(),
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
    # All validation precedes materialization; no shared launch helper is invoked.
    output.mkdir()
    (output / "input.json").write_bytes(input_bytes)
    (output / "config.json").write_bytes(config_bytes)
    lua = output / "config.lua"
    lua.write_text("return " + _lua_literal(config) + "\n", encoding="utf-8")
    validate_lua_syntax(lua, executable)
    (output / "candidate.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    return report


def run_map3_observation_candidate(rom_path: Path, candidate_directory: Path) -> dict[str, Any]:
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
        if _candidate_execution_sources() != report["ExecutionSources"]:
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
        if config["candidate"]["frames"] != load_json(directory / "input.json")["frames"]:
            raise ValueError("candidate frame table differs from its frozen input")
        config["candidate"]["checkpointPath"] = (runtime / "checkpoints.jsonl").as_posix()
        diagnostic["stage"] = "session-copy"
        persist()
        shutil.copy2(rom_path, session)
        diagnostic["stage"] = "observer-preparation"
        persist()
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
        if (
            observed.get("kind") != "bounded-original-observation"
            or observed["terminal"]["map"] != 19
        ):
            raise ValueError("candidate terminal output is not the first Map19 observation")
        if (
            not observed["restoration"]["callbacksCleared"]
            or not observed["restoration"]["sessionStateRestored"]
        ):
            raise ValueError("candidate cleanup did not complete")
        diagnostic["status"] = "OBSERVATION-COMPLETE-UNREVIEWED"
    except Exception as error:
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
    return {
        "Status": "OBSERVATION-COMPLETE-UNREVIEWED",
        "EmulatorLaunches": int(diagnostic["process"]["started"]),
        "SessionRomDeleted": diagnostic["sessionRomDeleted"],
        "H4": "not established",
    }
