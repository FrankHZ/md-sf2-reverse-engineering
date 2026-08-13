"""Static-first bridge for the bounded roster/death Map Script H3 rail.

This owner deliberately keeps the ``DEAD_COMBATANTS_LIST`` handler-local.  It
is not in the logical ``COMBATANT_DATA`` span copied by the original SRAM
services, so list mutation cases do not imply save/load persistence.
"""

from __future__ import annotations

import hashlib
import json
import re
from pathlib import Path
from typing import Any

from sf2tool.h2.map_script_engine import build_map_script_engine_contract
from sf2tool.h3.bizhawk import DERIVED_ROOT, run_observer, verify_runtime_contract
from sf2tool.h3.map_lifecycle import _with_instrumented_rom_database
from sf2tool.h3.observer_status import (
    assert_observer_status,
    callback_failure_status,
    observer_failure_contract,
)
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.rom import inspect_rom, mega_drive_checksum

H1_LISTING = Path("build/sf2build-h1.lst")
MAP_SOURCE_1 = Path("disasm/code/common/scripting/map/mapscriptengine_1.asm")
MAP_SOURCE_2 = Path("disasm/code/common/scripting/map/mapscriptengine_2.asm")
PARTY_SOURCE = Path("disasm/code/common/stats/battleparty.asm")
SRAM_SOURCE = Path("disasm/code/common/tech/sram/sramfunctions.asm")
CONSTANT_SOURCES = (Path("disasm/sf2const.asm"), Path("disasm/sf2enums.asm"))
HANDLERS = (
    "csc08_joinForce",
    "csc0E_jumpIfForceMemberInList",
    "csc0F_jumpIfCharacterDead",
    "csc1F_addDefeatedAlly",
    "csc20_updateDefeatedAllies",
    "csc21_reviveAlly",
)
CASE_ORDER = (
    "csc08-join-absent",
    "csc08-join-already-present",
    "csc0E-defeated-list-empty",
    "csc0E-defeated-list-hit",
    "csc0E-defeated-list-miss",
    "csc0F-hp-dead",
    "csc0F-hp-live",
    "csc1F-ally-defeated-append",
    "csc20-update-defeated-offscreen-skip",
    "csc20-update-defeated-onscreen-append",
    "csc21-revive-empty",
    "csc21-revive-hit-first",
    "csc21-revive-hit-middle",
    "csc21-revive-miss",
)
MAP_HOST_REENTRY_FRAME_FLOOR = 360
OWNER = "force-state-roster-death"
FIXTURE = repo_path("tests/fixtures/h3/force-state-roster-death-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h3-force-state-roster-death-fixture.schema.json")
OBSERVATION_SCHEMA = repo_path("schemas/h3-force-state-roster-death-observation.schema.json")
FAILURE_SCHEMA = repo_path("schemas/h3/force-state-roster-death-callback-failure.schema.json")
OBSERVER = repo_path("tools/bizhawk/force_state_roster_death_observer.lua")
OBSERVER_FAILURE_CONTRACT = observer_failure_contract(OWNER)


def _section(source: str, symbol: str) -> list[str]:
    start = re.search(rf"^{re.escape(symbol)}:\s*$", source, re.MULTILINE)
    if start is None:
        raise ValueError(f"roster/death source section is missing: {symbol}")
    end = source.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"roster/death source section end is missing: {symbol}")
    return [
        re.sub(r"\s+", " ", line.split(";", 1)[0].strip())
        for line in source[start.start() : end].splitlines()
        if line.split(";", 1)[0].strip()
        and not line.split(";", 1)[0].strip().endswith(":")
        and line.split(";", 1)[0].strip() != "nop"
    ]


def _h1_rows(listing: str, symbol: str) -> list[tuple[int, str]]:
    start = re.search(rf"^[0-9A-F]{{8}}\s+{re.escape(symbol)}:\s*$", listing, re.MULTILINE)
    if start is None:
        raise ValueError(f"roster/death H1 section is missing: {symbol}")
    end = listing.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"roster/death H1 section end is missing: {symbol}")
    rows: list[tuple[int, str]] = []
    for raw in listing[start.start() : end].splitlines():
        match = re.fullmatch(r"(?P<address>[0-9A-F]{8})\s+(?P<body>.*)", raw)
        if match is None:
            continue
        body = re.sub(r"^(?:[0-9A-F]{2,8}\s+)+", "", match["body"].split(";", 1)[0].strip())
        # H1 expands ``sndCom``/``txt`` into follow-on ``M`` listing records;
        # they are assembler expansion bookkeeping, not source instructions.
        if body and not body.endswith(":") and body != "nop" and not body.startswith("M "):
            rows.append((int(match["address"], 16), re.sub(r"\s+", " ", body)))
    return rows


def _equates(upstream: Path) -> dict[str, int]:
    """Read all authoritative numerical names once from their source owners."""
    values: dict[str, int] = {}
    for relative in CONSTANT_SOURCES:
        for raw in (upstream / relative).read_text(encoding="utf-8").splitlines():
            match = re.match(r"^\s*([A-Z][A-Z0-9_]*):\s+equ\s+(-?\$[0-9A-F]+|-?\d+)", raw)
            if match is None:
                continue
            text = match[2]
            negative = text.startswith("-")
            body = text[1:] if negative else text
            value = int(body[1:], 16) if body.startswith("$") else int(body)
            values[match[1]] = -value if negative else value
    return values


def _require_constants(constants: dict[str, int]) -> dict[str, int]:
    names = (
        "COMBATANT_DATA",
        "COMBATANT_DATA_ENTRY_SIZE",
        "COMBATANT_OFFSET_HP_CURRENT",
        "COMBATANT_OFFSET_X",
        "COMBATANT_ALLIES_SPACE_END",
        "COMBATANT_ENEMIES_COUNTER",
        "COMBATANT_ENEMIES_START",
        "COMBATANT_ENEMIES_START_MINUS_ALLIES_SPACE_END",
        "COMBATANT_ENEMIES_END",
        "COMBATANT_ALLIES_COUNTER",
        "DEAD_COMBATANTS_LIST",
        "DEAD_COMBATANTS_LIST_LENGTH",
        "FORCEMEMBER_JOINED_FLAGS_START",
        "GAME_FLAGS",
        "SAVE1_DATA",
        "SAVE2_DATA",
        "SAVE1_CHECKSUM",
        "SAVE2_CHECKSUM",
        "SAVE_FLAGS",
        "SAVE_SLOT_REAL_SIZE",
    )
    missing = [name for name in names if name not in constants]
    if missing:
        raise ValueError(f"roster/death constants missing: {', '.join(missing)}")
    return {name: constants[name] for name in names}


def _calls(instructions: list[str]) -> list[str]:
    target = (
        r"(?:(?!d[0-7]$)[A-Za-z_][A-Za-z0-9_]*|"
        r"\([A-Za-z_][A-Za-z0-9_]*\)(?:\.[bwls])?)"
    )
    return [
        row
        for row in instructions
        if re.fullmatch(
            rf"(?:jsr|bsr)(?:\.[bwls])? {target}",
            row,
        )
    ]


def _call_identity(row: str) -> str:
    """Normalize legal direct syntax but leave source target identity intact."""
    target = (
        r"(?:(?!d[0-7]$)[A-Za-z_][A-Za-z0-9_]*|"
        r"\([A-Za-z_][A-Za-z0-9_]*\)(?:\.[bwls])?)"
    )
    match = re.fullmatch(rf"(?P<opcode>jsr|bsr)(?:\.[bwls])? (?P<target>{target})", row)
    if match is None:
        raise ValueError(f"roster/death direct call syntax drift: {row}")
    return f"{match['opcode']} {match['target'].split('(')[-1].split(')')[0]}"


def _require_h2(facts: object) -> dict[str, Any]:
    required = {
        "macros",
        "sourceSites",
        "programTotals",
        "handlers",
        "callerBreakdown",
        "commonStatsIdentity",
        "runtimeQuestions",
        "activePartyCommandFacts",
    }
    if not isinstance(facts, dict) or set(facts) != required:
        raise ValueError("roster/death H2 fact shape drift")
    if facts["runtimeQuestions"] != ["force-state/roster-death-persistence-visible-outcomes"]:
        raise ValueError("roster/death H2 runtime question drift")
    if [row.get("handler") for row in facts["handlers"]] != list(HANDLERS):
        raise ValueError("roster/death H2 handler order drift")
    return facts


def _handler_records(
    facts: dict[str, Any], source_1: str, source_2: str, listing: str, addresses: dict[str, int]
) -> list[dict[str, Any]]:
    records: list[dict[str, Any]] = []
    for row in facts["handlers"]:
        symbol = row["handler"]
        source = source_1 if symbol.startswith(("csc1F", "csc20", "csc21")) else source_2
        instructions = _section(source, symbol)
        guard = row["sectionGuard"]["orderedInstructions"]
        if instructions != guard:
            raise ValueError(f"roster/death source guard drift: {symbol}")
        h1 = _h1_rows(listing, symbol)
        if [instruction for _, instruction in h1] != instructions:
            raise ValueError(f"roster/death H1/source order drift: {symbol}")
        if addresses.get(symbol) != row["address"]:
            raise ValueError(f"roster/death H1 address drift: {symbol}")
        calls = _calls(instructions)
        expected_calls = [
            f"{item['opcode']} {item['instructionTarget']}" for item in row["directCalls"]
        ]
        actual_calls = [_call_identity(item) for item in calls]
        if actual_calls != expected_calls:
            raise ValueError(f"roster/death direct caller order drift: {symbol}")
        records.append(
            {
                "macro": row["macro"],
                "handler": symbol,
                "address": row["address"],
                "instructions": instructions,
                "h1InstructionAddresses": [address for address, _ in h1],
                "branchRecords": row["sectionGuard"]["branchRecords"],
                "directCalls": row["directCalls"],
            }
        )
    return records


def _derive_csc20_storage_shape(
    instructions: list[str], constants: dict[str, int]
) -> tuple[int, int, int]:
    """Derive csc20's enemy start, DBF count, and byte-write width from use sites."""
    start_rows = [row for row in instructions if re.fullmatch(r"moveq #\$[0-9A-F]{1,8},d0", row)]
    if len(start_rows) != 1:
        raise ValueError("roster/death csc20 enemy start/use-site drift")
    start_literal = int(start_rows[0].split("#$", 1)[1].split(",", 1)[0], 16)
    enemy_start = start_literal & 0xFF
    if enemy_start != constants["COMBATANT_ENEMIES_START"]:
        raise ValueError("roster/death csc20 enemy start does not match COMBATANT_ENEMIES_START")

    counter_rows = [row for row in instructions if re.fullmatch(r"moveq #\$[0-9A-F]{1,8},d7", row)]
    if len(counter_rows) != 1 or "dbf d7,loc_46AFE" not in instructions:
        raise ValueError("roster/death csc20 loop counter/use-site drift")
    loop_counter = int(counter_rows[0].split("#$", 1)[1].split(",", 1)[0], 16)
    if loop_counter > 0x7F:
        raise ValueError("roster/death csc20 loop counter must be nonnegative")
    if loop_counter != constants["COMBATANT_ENEMIES_COUNTER"]:
        raise ValueError("roster/death csc20 loop counter does not match COMBATANT_ENEMIES_COUNTER")
    if enemy_start + loop_counter != constants["COMBATANT_ENEMIES_END"]:
        raise ValueError("roster/death csc20 loop end does not match COMBATANT_ENEMIES_END")
    append_iterations = loop_counter + 1

    write_rows = [
        row for row in instructions if re.fullmatch(r"move\.(?P<width>[bwl]) d0,\(a1\)\+", row)
    ]
    if len(write_rows) != 1:
        raise ValueError("roster/death csc20 list write/use-site drift")
    width = re.fullmatch(r"move\.(?P<width>[bwl]) d0,\(a1\)\+", write_rows[0])
    assert width is not None
    entry_bytes = {"b": 1, "w": 2, "l": 4}[width["width"]]
    return enemy_start, append_iterations, entry_bytes


def build_force_state_roster_death_static_contract(
    upstream_path: Path, rom_path: Path | None = None
) -> dict[str, Any]:
    """Parse the six source forms before any H3 observer can launch."""
    upstream = upstream_path.resolve(strict=True)
    h2 = build_map_script_engine_contract(rom_path or repo_path("local/roms/sf2-us.bin"), upstream)[
        "forceStateCommandFacts"
    ]
    facts = _require_h2(h2)
    source_1 = (upstream / MAP_SOURCE_1).read_text(encoding="utf-8")
    source_2 = (upstream / MAP_SOURCE_2).read_text(encoding="utf-8")
    listing = (upstream / H1_LISTING).read_text(encoding="utf-8")
    addresses = listing_symbol_addresses(listing)
    records = _handler_records(facts, source_1, source_2, listing, addresses)
    constants = _require_constants(_equates(upstream))
    by_handler = {record["handler"]: record for record in records}
    # These guards make the reported RAM domains and branch polarities falsifiable
    # before a fixture comparison.  They are source use-sites, not duplicated truth.
    if by_handler["csc1F_addDefeatedAlly"]["instructions"][-3:] != [
        "move.b d0,(a1)",
        "addq.w #1,((DEAD_COMBATANTS_LIST_LENGTH-$1000000)).w",
        "rts",
    ]:
        raise ValueError("roster/death append write/length order drift")
    update = by_handler["csc20_updateDefeatedAllies"]["instructions"]
    update_guard = ("cmpi.w #-1,d1", "beq.s loc_46B0E", "move.b d0,(a1)+", "addq.w #1,d2")
    if [row for row in update if row in update_guard] != list(update_guard):
        raise ValueError("roster/death update offscreen branch/write order drift")
    enemy_start, append_iterations, defeated_list_entry_bytes = _derive_csc20_storage_shape(
        update, constants
    )
    revive = by_handler["csc21_reviveAlly"]["instructions"]
    revive_guard = (
        "subq.w #1,d7",
        "bcs.w return_46B40",
        "cmp.b (a1),d0",
        "bne.s loc_46B3A",
        "subq.w #1,((DEAD_COMBATANTS_LIST_LENGTH-$1000000)).w",
        "move.b (a1)+,(a2)+",
    )
    if [row for row in revive if row in revive_guard] != list(revive_guard):
        raise ValueError("roster/death revive cursor/branch/write order drift")
    join_owner = _section((upstream / PARTY_SOURCE).read_text(encoding="utf-8"), "JoinForce")
    join_guard = ("bsr.w SetFlag", "bsr.s UpdateForce", "bsr.w JoinBattleParty")
    if [row for row in join_owner if row in join_guard] != list(join_guard):
        raise ValueError("roster/death JoinForce membership/mutation order drift")
    sram_source = (upstream / SRAM_SOURCE).read_text(encoding="utf-8")
    save = _section(sram_source, "SaveGame")
    load = _section(sram_source, "LoadGame")
    save_guard = (
        "lea (COMBATANT_DATA).l,a0",
        "move.w #SAVE_SLOT_REAL_SIZE,d7",
        "bsr.w CopyBytesToSram",
    )
    if [row for row in save if row in save_guard] != list(save_guard):
        raise ValueError("roster/death SaveGame logical-span use-site drift")
    load_guard = (
        "lea (COMBATANT_DATA).l,a1",
        "move.w #SAVE_SLOT_REAL_SIZE,d7",
        "bsr.w CopyBytesFromSram",
    )
    if [row for row in load if row in load_guard] != list(load_guard):
        raise ValueError("roster/death LoadGame logical-span use-site drift")
    copy_to = _section(sram_source, "CopyBytesToSram")
    copy_from = _section(sram_source, "CopyBytesFromSram")
    if "addq.l #2,a1" not in copy_to:
        raise ValueError("roster/death SaveGame physical SRAM stride drift")
    if "addq.l #2,a0" not in copy_from:
        raise ValueError("roster/death LoadGame physical SRAM stride drift")
    logical_span_end = constants["COMBATANT_DATA"] + constants["SAVE_SLOT_REAL_SIZE"]
    if (
        constants["DEAD_COMBATANTS_LIST"] >= constants["COMBATANT_DATA"]
        and constants["DEAD_COMBATANTS_LIST"] < logical_span_end
    ):
        raise ValueError("roster/death list unexpectedly overlaps saved logical span")
    if not constants["COMBATANT_DATA"] <= constants["GAME_FLAGS"] < logical_span_end:
        raise ValueError("roster/death membership flags must reside in SaveGame logical span")
    if (
        constants["COMBATANT_ENEMIES_START"]
        - constants["COMBATANT_ENEMIES_START_MINUS_ALLIES_SPACE_END"]
        != constants["COMBATANT_ALLIES_SPACE_END"]
    ):
        raise ValueError("roster/death enemy combatant-index mapping drift")
    return {
        "evidenceLabel": "Confirmed",
        "provenance": {
            "sourceFixturePath": "tests/fixtures/h2/map-script-engine-static-v1.json",
            "sourceFixtureField": "expected.forceStateCommandFacts",
            "reproductionCommand": "uv run sf2 h2 map-script-engine",
            "sourcePaths": [
                MAP_SOURCE_1.relative_to("disasm").as_posix(),
                MAP_SOURCE_2.relative_to("disasm").as_posix(),
                PARTY_SOURCE.relative_to("disasm").as_posix(),
                SRAM_SOURCE.relative_to("disasm").as_posix(),
            ],
            "h1ListingPath": H1_LISTING.as_posix(),
        },
        "handlers": records,
        "callerBreakdown": facts["callerBreakdown"],
        "storage": {
            "rosterMembership": {
                "gameFlagsAddress": constants["GAME_FLAGS"],
                "joinedFlagStart": constants["FORCEMEMBER_JOINED_FLAGS_START"],
                "logicalSavedDomain": True,
            },
            "currentHp": {
                "combatantDataAddress": constants["COMBATANT_DATA"],
                "entryBytes": constants["COMBATANT_DATA_ENTRY_SIZE"],
                "offsetBytes": constants["COMBATANT_OFFSET_HP_CURRENT"],
                "logicalSavedDomain": True,
            },
            "currentX": {
                "offsetBytes": constants["COMBATANT_OFFSET_X"],
                "entryBytes": constants["COMBATANT_DATA_ENTRY_SIZE"],
                "enemyFirstIndex": enemy_start,
                "enemyIndexAdjustment": constants["COMBATANT_ENEMIES_START_MINUS_ALLIES_SPACE_END"],
                "firstEnemyDataEntry": constants["COMBATANT_ALLIES_SPACE_END"],
                "logicalSavedDomain": True,
            },
            "defeatedList": {
                "baseAddress": constants["DEAD_COMBATANTS_LIST"],
                "lengthAddress": constants["DEAD_COMBATANTS_LIST_LENGTH"],
                "entryBytes": defeated_list_entry_bytes,
                "logicalSavedDomain": False,
                "updateDefeatedAlliesAppendIterations": append_iterations,
            },
            "saveLoad": {
                "logicalRam": {
                    "baseAddress": constants["COMBATANT_DATA"],
                    "logicalByteCount": constants["SAVE_SLOT_REAL_SIZE"],
                    "endExclusiveAddress": logical_span_end,
                },
                "slots": [
                    {
                        "selector": 0,
                        "dataAddress": constants["SAVE1_DATA"],
                        "checksumAddress": constants["SAVE1_CHECKSUM"],
                        "occupiedFlagBit": 0,
                    },
                    {
                        "selector": 1,
                        "dataAddress": constants["SAVE2_DATA"],
                        "checksumAddress": constants["SAVE2_CHECKSUM"],
                        "occupiedFlagBit": 1,
                    },
                ],
                "saveFlagsAddress": constants["SAVE_FLAGS"],
                "physicalByteStride": 2,
                "saveGameAddress": addresses["SaveGame"],
                "loadGameAddress": addresses["LoadGame"],
            },
        },
        "sourceSha256": {
            path.relative_to("disasm").as_posix(): hashlib.sha256((upstream / path).read_bytes())
            .hexdigest()
            .upper()
            for path in (MAP_SOURCE_1, MAP_SOURCE_2, PARTY_SOURCE, SRAM_SOURCE)
        },
    }


def derive_force_state_roster_death_cases(static: dict[str, Any]) -> list[dict[str, Any]]:
    """Return the exact 14-case matrix and explicitly classify persistence scope."""
    addresses = {row["handler"]: row["address"] for row in static["handlers"]}
    forms = (
        ("csc08-join-absent", "csc08_joinForce", "absent", "persistence"),
        ("csc08-join-already-present", "csc08_joinForce", "already-present", "persistence"),
        ("csc0E-defeated-list-empty", "csc0E_jumpIfForceMemberInList", "empty", "local-only"),
        ("csc0E-defeated-list-hit", "csc0E_jumpIfForceMemberInList", "hit", "local-only"),
        ("csc0E-defeated-list-miss", "csc0E_jumpIfForceMemberInList", "miss", "local-only"),
        ("csc0F-hp-dead", "csc0F_jumpIfCharacterDead", "dead", "persistence"),
        ("csc0F-hp-live", "csc0F_jumpIfCharacterDead", "live", "persistence"),
        ("csc1F-ally-defeated-append", "csc1F_addDefeatedAlly", "append", "local-only"),
        (
            "csc20-update-defeated-offscreen-skip",
            "csc20_updateDefeatedAllies",
            "offscreen-skip",
            "local-only",
        ),
        (
            "csc20-update-defeated-onscreen-append",
            "csc20_updateDefeatedAllies",
            "onscreen-append",
            "local-only",
        ),
        ("csc21-revive-empty", "csc21_reviveAlly", "empty", "local-only"),
        ("csc21-revive-hit-first", "csc21_reviveAlly", "hit-first", "local-only"),
        ("csc21-revive-hit-middle", "csc21_reviveAlly", "hit-middle", "local-only"),
        ("csc21-revive-miss", "csc21_reviveAlly", "miss", "local-only"),
    )
    cases = [
        {
            "id": case_id,
            "handler": handler,
            "handlerAddress": addresses[handler],
            "axis": axis,
            "scope": scope,
        }
        for case_id, handler, axis, scope in forms
    ]
    if tuple(case["id"] for case in cases) != CASE_ORDER:
        raise ValueError("roster/death case ordering drift")
    return cases


def _canonical_sha256(value: object) -> str:
    return (
        hashlib.sha256(json.dumps(value, sort_keys=True, separators=(",", ":")).encode("utf-8"))
        .hexdigest()
        .upper()
    )


def _h1_call_records(
    listing: str, record: dict[str, Any], resolutions: dict[str, str]
) -> list[dict[str, Any]]:
    rows = _h1_rows(listing, record["handler"])
    calls: list[dict[str, Any]] = []
    for index, (address, instruction) in enumerate(rows):
        normalized = _call_identity(instruction) if instruction in _calls([instruction]) else None
        if normalized is None:
            continue
        _, target = normalized.split(" ", 1)
        if target not in resolutions:
            continue
        if index + 1 >= len(rows):
            raise ValueError(f"roster/death call lacks H1 return seam: {record['handler']}")
        calls.append(
            {
                "h1Address": address,
                "returnAddress": rows[index + 1][0],
                "instructionTarget": target,
                "effectiveTarget": resolutions[target],
            }
        )
    expected = [entry["instructionTarget"] for entry in record["directCalls"]]
    if [entry["instructionTarget"] for entry in calls] != expected:
        raise ValueError(f"roster/death H1 caller inventory drift: {record['handler']}")
    return calls


def _runtime_contract(static: dict[str, Any], upstream: Path) -> dict[str, Any]:
    """Bind callback seams to H1 PCs and preserve alias/effective identities."""
    listing = (upstream / H1_LISTING).read_text(encoding="utf-8")
    addresses = listing_symbol_addresses(listing)
    resolutions = {
        row["instructionTarget"]: row["effectiveTarget"]
        for row in static["callerBreakdown"]["targetResolutions"]
    }
    handler_records = []
    for record in static["handlers"]:
        h1 = _h1_rows(listing, record["handler"])
        if not h1 or h1[-1][1] != "rts":
            raise ValueError(f"roster/death H1 return seam drift: {record['handler']}")
        handler_records.append(
            {
                "handler": record["handler"],
                "handlerAddress": record["address"],
                "returnAddress": h1[-1][0],
                "calls": _h1_call_records(listing, record, resolutions),
            }
        )
    required = {
        "RunMapSetupInitFunction",
        "SaveGame",
        "LoadGame",
        "CheckFlag",
        "GetCurrentHp",
        "GetCombatantX",
        "JoinForce",
    }
    if not required <= addresses.keys():
        raise ValueError("roster/death runtime H1 symbol inventory drift")
    storage = static["storage"]
    if (
        storage["saveLoad"]["saveGameAddress"] != addresses["SaveGame"]
        or storage["saveLoad"]["loadGameAddress"] != addresses["LoadGame"]
    ):
        raise ValueError("roster/death SaveGame/LoadGame H1 address drift")
    return {
        "entryAddress": addresses["RunMapSetupInitFunction"],
        "handlers": handler_records,
        "services": {
            name: addresses[name]
            for name in (
                "SaveGame",
                "LoadGame",
                "CheckFlag",
                "GetCurrentHp",
                "GetCombatantX",
                "JoinForce",
            )
        },
        "storage": storage,
    }


def _flag_storage(storage: dict[str, Any], member: int) -> dict[str, int]:
    """Derive the joined-flag byte/mask from the source-owned MSB-first bitmap."""
    bit = storage["rosterMembership"]["joinedFlagStart"] + member
    return {
        "member": member,
        "bitIndex": bit,
        "address": storage["rosterMembership"]["gameFlagsAddress"] + bit // 8,
        "mask": 0x80 >> (bit % 8),
    }


def _case_inputs(cases: list[dict[str, Any]], runtime: dict[str, Any]) -> list[dict[str, Any]]:
    """Derive all launch input without exposing fixture observations to Lua."""
    storage = runtime["storage"]
    hp = storage["currentHp"]
    current_x = storage["currentX"]
    save_load = storage["saveLoad"]
    stream_target = 0xFF48A0
    result: list[dict[str, Any]] = []
    for row in cases:
        case_id = row["id"]
        state: dict[str, Any]
        stream: list[int]
        if case_id == "csc08-join-absent":
            joined_flag = _flag_storage(storage, 5)
            slot = save_load["slots"][0]
            logical_offset = joined_flag["address"] - save_load["logicalRam"]["baseAddress"]
            state = {
                "joinedFlag": {**joined_flag, "initialSet": False},
                "persistence": {
                    "selector": 0,
                    "logicalOffset": logical_offset,
                    "selectedPhysicalAddress": slot["dataAddress"]
                    + logical_offset * save_load["physicalByteStride"],
                    "checksumAddress": slot["checksumAddress"],
                    "saveFlagsAddress": save_load["saveFlagsAddress"],
                    "occupiedFlagMask": 1 << slot["occupiedFlagBit"],
                },
            }
            stream = [0, 5]
        elif case_id == "csc08-join-already-present":
            state = {"joinedFlag": {**_flag_storage(storage, 5), "initialSet": True}}
            stream = [0, 5]
        elif case_id.startswith("csc0E-"):
            lists = {"empty": [], "hit": [7, 9], "miss": [5, 9]}
            axis = row["axis"]
            state = {
                "defeatedList": lists[axis],
                "listTouchedByteCount": len(lists[axis]),
                "probeMember": 7,
                "targetAddress": stream_target,
            }
            stream = [0, 7, 0, 255, 72, 160]
        elif case_id.startswith("csc0F-"):
            current_hp = 0 if row["axis"] == "dead" else 10
            state = {
                "hp": {
                    "member": 0,
                    "address": hp["combatantDataAddress"] + hp["offsetBytes"],
                    "value": current_hp,
                },
                "targetAddress": stream_target,
            }
            stream = [0, 0, 0, 255, 72, 160]
        elif case_id == "csc1F-ally-defeated-append":
            state = {"defeatedList": [1, 2], "listTouchedByteCount": 3, "probeMember": 7}
            stream = [0, 7]
        elif case_id.startswith("csc20-"):
            x_values = [-1] * 32
            if row["axis"] == "onscreen-append":
                x_values[0] = 0
            state = {
                "defeatedList": [3],
                "listTouchedByteCount": 33,
                "combatantX": x_values,
                "combatantXAddresses": [
                    hp["combatantDataAddress"]
                    + (current_x["enemyFirstIndex"] - current_x["enemyIndexAdjustment"] + member)
                    * current_x["entryBytes"]
                    + current_x["offsetBytes"]
                    for member in range(32)
                ],
            }
            stream = []
        else:
            lists = {"empty": [], "hit-first": [7, 9, 11], "hit-middle": [5, 7, 9], "miss": [5, 9]}
            state = {
                "defeatedList": lists[row["axis"]],
                "listTouchedByteCount": len(lists[row["axis"]]),
                "probeMember": 7,
            }
            stream = [0, 7]
        result.append(
            {
                "id": case_id,
                "handlerAddress": row["handlerAddress"],
                "streamBytes": stream,
                "state": state,
            }
        )
    if [row["id"] for row in result] != list(CASE_ORDER):
        raise ValueError("roster/death input case ordering drift")
    return result


def _validate_fixture_observation(
    observation: dict[str, Any],
    cases: list[dict[str, Any]],
    inputs: list[dict[str, Any]],
    runtime: dict[str, Any],
) -> None:
    """Reject a corpus that loses the bounded local/persistence distinction."""
    if observation["recordOrder"] != list(CASE_ORDER) or len(observation["records"]) != len(cases):
        raise ValueError("roster/death fixture observation matrix drift")
    for record, case, case_input in zip(observation["records"], cases, inputs, strict=True):
        if {key: record[key] for key in ("id", "handlerAddress", "scope")} != {
            key: case[key] for key in ("id", "handlerAddress", "scope")
        }:
            raise ValueError(f"roster/death fixture record identity drift: {case['id']}")
        if case["scope"] == "local-only" and "persistence" in record:
            raise ValueError(f"roster/death local-only record invented persistence: {case['id']}")
        if case["id"] != "csc08-join-absent" and "persistence" in record:
            raise ValueError(f"roster/death nonmutating record invented Save/Load: {case['id']}")
        if case["id"].startswith("csc0F-") and any(
            milestone.startswith(("save", "load")) for milestone in record["milestones"]
        ):
            raise ValueError(f"roster/death HP branch invented persistence operation: {case['id']}")
        list_fields = {"listBefore", "listAfter"}
        input_state = case_input["state"]
        if "defeatedList" not in input_state:
            invented_list_state = (
                list_fields & record.keys()
                or "length" in record["before"]
                or "length" in record["after"]
            )
            if invented_list_state:
                raise ValueError(
                    f"roster/death non-list record invented list observation: {case['id']}"
                )
            continue
        if not list_fields <= record.keys():
            raise ValueError(
                f"roster/death list record lacks scoped list observation: {case['id']}"
            )
        seed = input_state["defeatedList"]
        if record["listBefore"] != seed or record["before"].get("length") != len(seed):
            raise ValueError(f"roster/death list seed observation drift: {case['id']}")
        result = record["listAfter"]
        if record["after"].get("length") != len(result):
            raise ValueError(f"roster/death list length/result mismatch: {case['id']}")
        extra_touched_bytes = 0
        if case["handler"] == "csc1F_addDefeatedAlly":
            expected = seed + [input_state["probeMember"]]
            extra_touched_bytes = 1
        elif case["handler"] == "csc20_updateDefeatedAllies":
            expected = (
                seed
                if case["axis"] == "offscreen-skip"
                else seed + [runtime["storage"]["currentX"]["enemyFirstIndex"]]
            )
            extra_touched_bytes = runtime["storage"]["defeatedList"][
                "updateDefeatedAlliesAppendIterations"
            ]
        elif case["handler"] == "csc21_reviveAlly":
            probe = input_state["probeMember"]
            expected = seed.copy()
            if probe in expected:
                expected.remove(probe)
        else:
            expected = seed
        if result != expected:
            if case["handler"] == "csc20_updateDefeatedAllies":
                raise ValueError(f"roster/death csc20 branch/list result drift: {case['id']}")
            raise ValueError(f"roster/death list handler result drift: {case['id']}")
        if input_state["listTouchedByteCount"] != len(seed) + extra_touched_bytes:
            raise ValueError(f"roster/death list scoped range drift: {case['id']}")
        if case["id"].startswith("csc20-") and len(result) > input_state["listTouchedByteCount"]:
            raise ValueError(f"roster/death csc20 result exceeds scoped list range: {case['id']}")


def _instrument_rom(rom_path: Path, fixture: dict[str, Any]) -> Path:
    """Create and hash-check a session-only trampoline copy; never patch input."""
    original_hash = inspect_rom(rom_path.resolve(strict=True))["sha256"]
    data = bytearray(rom_path.read_bytes())
    trampoline = fixture["instrumentation"]["trampoline"]
    call_site = trampoline["callSiteAddress"]
    original = bytes.fromhex(trampoline["callSiteOriginalHex"])
    patched = bytes.fromhex(trampoline["callSitePatchedHex"])
    stub_address = trampoline["stubAddress"]
    stub_original = bytes.fromhex(trampoline["stubOriginalHex"])
    stub = bytes.fromhex(trampoline["stubHex"])
    if data[call_site : call_site + len(original)] != original:
        raise ValueError("roster/death trampoline original call-site bytes drift")
    if data[stub_address : stub_address + len(stub_original)] != stub_original:
        raise ValueError("roster/death trampoline original padding bytes drift")
    if patched != b"\x4e\xb9" + stub_address.to_bytes(4, "big") or len(stub) > len(stub_original):
        raise ValueError("roster/death trampoline shape/span drift")
    data[call_site : call_site + len(patched)] = patched
    data[stub_address : stub_address + len(stub)] = stub
    data[0x18E:0x190] = int(mega_drive_checksum(bytes(data)), 16).to_bytes(2, "big")
    if inspect_rom(rom_path.resolve(strict=True))["sha256"] != original_hash:
        raise ValueError("roster/death instrumentation altered original input")
    DERIVED_ROOT.mkdir(parents=True, exist_ok=True)
    output = DERIVED_ROOT / "force-state-roster-death.instrumented.bin"
    output.write_bytes(data)
    return output


def _status_diagnostic() -> str | None:
    payload = callback_failure_status(
        DERIVED_ROOT / f"{OWNER}.status.txt", owner=OWNER, schema_path=FAILURE_SCHEMA
    )
    return json.dumps(payload, sort_keys=True) if payload is not None else None


def _assert_status() -> None:
    assert_observer_status(
        DERIVED_ROOT / f"{OWNER}.status.txt",
        owner=OWNER,
        schema_path=FAILURE_SCHEMA,
        required_milestones=("milestone:force-state-roster-death-probe",),
    )


def validate_force_state_roster_death_fixture_semantics(
    fixture: dict[str, Any], static: dict[str, Any], runtime: dict[str, Any]
) -> tuple[list[dict[str, Any]], list[dict[str, Any]]]:
    cases = derive_force_state_roster_death_cases(static)
    inputs = _case_inputs(cases, runtime)
    if fixture["staticContractSha256"] != _canonical_sha256(static):
        raise ValueError("roster/death static-contract digest drift")
    if fixture["runtimeContract"] != runtime:
        raise ValueError("roster/death runtime-contract drift")
    if fixture["function"]["runMapSetupInitFunctionAddress"] != runtime["entryAddress"]:
        raise ValueError("roster/death entry-address drift")
    if fixture["cases"] != cases or fixture["caseInputs"] != inputs:
        raise ValueError("roster/death case/input matrix drift")
    required_frame_budget = len(cases) * MAP_HOST_REENTRY_FRAME_FLOOR
    if fixture["maxFrames"] < required_frame_budget:
        raise ValueError(
            "roster/death frame budget cannot accommodate all map-host reentries: "
            f"{fixture['maxFrames']} < {required_frame_budget}"
        )
    _validate_fixture_observation(fixture["observation"], cases, inputs, runtime)
    local_only = [row["id"] for row in cases if row["scope"] == "local-only"]
    persistence = [row["id"] for row in cases if row["scope"] == "persistence"]
    if fixture["scopeClassification"] != {"localOnly": local_only, "persistence": persistence}:
        raise ValueError("roster/death local/persistence scope classification drift")
    return cases, inputs


def build_force_state_roster_death_observer_config(
    fixture: dict[str, Any], static: dict[str, Any], runtime: dict[str, Any]
) -> dict[str, Any]:
    """Validate the fixture, then construct observer-only configuration without its golden."""
    cases, inputs = validate_force_state_roster_death_fixture_semantics(fixture, static, runtime)
    config = {
        "fixtureId": fixture["id"],
        "jsonModulePath": OBSERVER.with_name("json.lua").as_posix(),
        "mapTest": fixture["mapTestIndex"],
        "maxFrames": fixture["maxFrames"],
        "instrumentation": fixture["instrumentation"],
        "runtimeContract": runtime,
        "cases": cases,
        "caseInputs": inputs,
        "scopeClassification": fixture["scopeClassification"],
        "observerFailureContract": OBSERVER_FAILURE_CONTRACT,
        "harness": load_json(repo_path(fixture["sharedHarnessFixture"]))["harness"],
    }
    golden_fields = {"observation", "recordOrder", "records"}
    if golden_fields & config.keys():
        raise ValueError("roster/death observer configuration leaked fixture golden fields")
    return config


def verify_force_state_roster_death(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 180
) -> dict[str, Any]:
    """Run the one-launch 14-case original-function roster/death matrix."""
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner="roster/death fixture")
    validate_json(fixture["observation"], OBSERVATION_SCHEMA, owner="roster/death observation")
    verify_runtime_contract(fixture, rom_path)
    upstream = upstream_path.resolve(strict=True)
    static = build_force_state_roster_death_static_contract(upstream, rom_path)
    runtime = _runtime_contract(static, upstream)
    config = build_force_state_roster_death_observer_config(fixture, static, runtime)
    instrumented_rom = _instrument_rom(rom_path, fixture)

    def observe() -> dict[str, Any]:
        return run_observer(
            rom_path=instrumented_rom,
            observer_path=OBSERVER,
            config=config,
            output_name=OWNER,
            timeout_seconds=timeout_seconds,
        )

    try:
        observed = _with_instrumented_rom_database(instrumented_rom, "SF2 H3 roster death", observe)
    except RuntimeError as error:
        diagnostic = _status_diagnostic()
        if diagnostic is not None:
            raise RuntimeError(f"{OWNER} observer callback failure: {diagnostic}") from error
        raise
    finally:
        instrumented_rom.unlink(missing_ok=True)
    _assert_status()
    validate_json(observed, OBSERVATION_SCHEMA, owner="roster/death observed")
    if observed != fixture["observation"]:
        raise ValueError(
            "roster/death runtime observation mismatch:\n"
            f"expected={fixture['observation']!r}\nobserved={observed!r}"
        )
    return {
        "Fixture": fixture["id"],
        "Cases": len(config["cases"]),
        "Handlers": len(runtime["handlers"]),
        "BizHawkLaunches": 1,
        "Instrumentation": "session-only",
        "Status": "PASS",
    }
