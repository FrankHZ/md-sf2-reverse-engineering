"""Bounded H3 confirmation of the Map 3 messenger acceptance body."""

from __future__ import annotations

import json
import re
import shutil
import subprocess
import tempfile
from hashlib import sha256
from pathlib import Path
from typing import Any

from sf2tool.h3 import map3_admitted_start as r1
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
