"""Bounded H3 contract for the map-script active-party/AI/follower forms.

The H2 contract owns the complete source corpus.  This module deliberately
re-parses the four local sections and the service owners before accepting the
small runtime matrix; a fixture is never a substitute for the source use site.
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
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.rom import inspect_rom, mega_drive_checksum

FIXTURE = repo_path("tests/fixtures/h3/force-state-active-party-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h3-force-state-active-party-fixture.schema.json")
OBSERVATION_SCHEMA = repo_path("schemas/h3-force-state-active-party-observation.schema.json")
OBSERVER = repo_path("tools/bizhawk/force_state_active_party_observer.lua")
H1_LISTING = Path("build/sf2build-h1.lst")
MAP_SOURCE = Path("disasm/code/common/scripting/map/mapscriptengine_1.asm")
OWNER_SOURCES = {
    "battleParty": Path("disasm/code/common/stats/battleparty.asm"),
    "activationGet": Path("disasm/code/common/stats/combatantstats_1.asm"),
    "activationSet": Path("disasm/code/common/stats/combatantstats_2.asm"),
    "reset": Path("disasm/code/common/scripting/map/resetalliesstats.asm"),
    "follower": Path("disasm/code/common/scripting/entity/entityfunctions_2.asm"),
}
CONSTANT_SOURCES = (Path("disasm/sf2const.asm"), Path("disasm/sf2enums.asm"))
HANDLERS = (
    "csc51_joinBattleParty",
    "csc54_joinForceAi",
    "csc55_resetCharacterBattleStats",
    "csc56_addFollower",
)


def _section(source: str, symbol: str) -> list[str]:
    start = re.search(rf"^{re.escape(symbol)}:\s*$", source, re.MULTILINE)
    if start is None:
        raise ValueError(f"active-party source section is missing: {symbol}")
    end = source.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"active-party source section end is missing: {symbol}")
    rows = []
    for raw in source[start.start() : end].splitlines():
        instruction = re.sub(r"\s+", " ", raw.split(";", 1)[0].strip())
        if instruction and not instruction.endswith(":") and instruction != "nop":
            rows.append(instruction)
    return rows


def _function_section(source: str, symbol: str) -> list[str]:
    start = re.search(rf"^{re.escape(symbol)}:\s*$", source, re.MULTILINE)
    if start is None:
        raise ValueError(f"active-party owner section is missing: {symbol}")
    end = source.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"active-party owner section end is missing: {symbol}")
    return [
        re.sub(r"\s+", " ", raw.split(";", 1)[0].strip())
        for raw in source[start.start() : end].splitlines()
        if raw.split(";", 1)[0].strip() and not raw.split(";", 1)[0].strip().endswith(":")
    ]


def _h1_rows(listing: str, symbol: str) -> list[tuple[int, str]]:
    start = re.search(rf"^[0-9A-F]{{8}}\s+{re.escape(symbol)}:\s*$", listing, re.MULTILINE)
    if start is None:
        raise ValueError(f"active-party H1 section is missing: {symbol}")
    end = listing.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"active-party H1 section end is missing: {symbol}")
    result = []
    for raw in listing[start.start() : end].splitlines():
        match = re.fullmatch(r"(?P<address>[0-9A-F]{8})\s+(?P<body>.*)", raw)
        if match is None:
            continue
        body = re.sub(r"^(?:[0-9A-F]{2,8}\s+)+", "", match["body"].split(";", 1)[0].strip())
        if body and not body.endswith(":") and body != "nop":
            result.append((int(match["address"], 16), re.sub(r"\s+", " ", body)))
    return result


def _calls(instructions: list[str]) -> list[str]:
    return [
        row
        for row in instructions
        if re.fullmatch(r"(?:jsr|bsr)(?:\.[bwls])? (?!d[0-7]$)[A-Za-z_][A-Za-z0-9_]*", row)
    ]


def _equates(upstream: Path) -> dict[str, int]:
    """Read the authoritative constants once; consumers do not carry RAM literals."""
    result: dict[str, int] = {}
    for relative in CONSTANT_SOURCES:
        for raw in (upstream / relative).read_text(encoding="utf-8").splitlines():
            match = re.match(r"^\s*([A-Z][A-Z0-9_]*):\s+equ\s+(-?\$[0-9A-F]+|-?\d+)", raw)
            if match is None:
                continue
            value = match[2]
            negative = value.startswith("-")
            number = value[1:] if negative else value
            parsed = int(number[1:], 16) if number.startswith("$") else int(number)
            result[match[1]] = -parsed if negative else parsed
    return result


def _require_equates(constants: dict[str, int], names: tuple[str, ...]) -> dict[str, int]:
    missing = [name for name in names if name not in constants]
    if missing:
        raise ValueError(f"active-party constants missing: {', '.join(missing)}")
    return {name: constants[name] for name in names}


def _require_h2(facts: object) -> dict[str, Any]:
    if not isinstance(facts, dict) or set(facts) != {
        "macros",
        "sourceSites",
        "programTotals",
        "handlers",
        "callerBreakdown",
        "sourceIdentityJoins",
        "runtimeQuestions",
    }:
        raise ValueError("active-party H2 fact shape drift")
    if facts["runtimeQuestions"] != [
        "force-state/active-party-ai-follower/normal-story-reachability",
        "force-state/active-party-ai-follower/save-load-capacity-lifecycle",
        "force-state/active-party-ai-follower/player-visible-presentation",
    ]:
        raise ValueError("active-party H2 runtime queue identity drift")
    handlers = facts["handlers"]
    if not isinstance(handlers, list) or [row.get("handler") for row in handlers] != list(HANDLERS):
        raise ValueError("active-party H2 handler identity/order drift")
    return facts


def build_force_state_active_party_static_contract(
    upstream_path: Path, rom_path: Path | None = None
) -> dict[str, Any]:
    """Parse source/H1 identity and guard the four local mutation paths."""
    upstream = upstream_path.resolve(strict=True)
    h2 = build_map_script_engine_contract(rom_path or repo_path("local/roms/sf2-us.bin"), upstream)[
        "forceStateCommandFacts"
    ]["activePartyCommandFacts"]
    facts = _require_h2(h2)
    source = (upstream / MAP_SOURCE).read_text(encoding="utf-8")
    listing = (upstream / H1_LISTING).read_text(encoding="utf-8")
    addresses = listing_symbol_addresses(listing)
    records = []
    for row in facts["handlers"]:
        symbol = row["handler"]
        instructions = _section(source, symbol)
        guarded = row["sectionGuard"]["orderedInstructions"]
        if instructions != guarded:
            raise ValueError(f"active-party source guard drift: {symbol}")
        h1 = _h1_rows(listing, symbol)
        if [instruction for _, instruction in h1] != instructions:
            raise ValueError(f"active-party H1/source order drift: {symbol}")
        if addresses.get(symbol) != row["address"]:
            raise ValueError(f"active-party H1 address drift: {symbol}")
        direct = _calls(instructions)
        direct_identities = [re.sub(r"^(bsr)\.[bwls] ", r"\1 ", item) for item in direct]
        if direct_identities != [
            f"{item['opcode']} {item['instructionTarget']}" for item in row["directCalls"]
        ]:
            raise ValueError(f"active-party direct call order drift: {symbol}")
        records.append(
            {
                "handler": symbol,
                "address": row["address"],
                "instructions": instructions,
                "h1InstructionAddresses": [address for address, _ in h1],
                "branchRecords": row["sectionGuard"]["branchRecords"],
                "mutationCallOrder": row["sectionGuard"]["mutationCallOrder"],
                "directCalls": row["directCalls"],
            }
        )
    expected = {
        "csc51_joinBattleParty": ("bne.w @Return", "beq.w @ReplaceLastActiveMember"),
        "csc54_joinForceAi": ("bne.s @SetAiControl",),
        "csc55_resetCharacterBattleStats": (),
        "csc56_addFollower": ("beq.w @Break",),
    }
    for record in records:
        if (
            tuple(item["branchInstruction"] for item in record["branchRecords"])
            != expected[record["handler"]]
        ):
            raise ValueError(f"active-party branch polarity drift: {record['handler']}")
    join_battle_rows = next(
        row["instructions"] for row in records if row["handler"] == "csc51_joinBattleParty"
    )
    join_battle_order = ["subq.w #2,d7", "jsr j_LeaveBattleParty", "jsr j_JoinBattleParty"]
    if [row for row in join_battle_rows if row in join_battle_order] != join_battle_order:
        raise ValueError("active-party csc51 replacement mutation order drift")
    follower_handler_rows = next(
        row["instructions"] for row in records if row["handler"] == "csc56_addFollower"
    )
    follower_handler_order = [
        "bsr.w GetEntityAddressFromCharacter",
        "cmpi.b #-1,(a0)",
        "jsr AddFollower",
    ]
    if [
        row for row in follower_handler_rows if row in follower_handler_order
    ] != follower_handler_order:
        raise ValueError("active-party csc56 scan/call order drift")
    owners = {}
    for name, path in OWNER_SOURCES.items():
        text = (upstream / path).read_text(encoding="utf-8")
        owners[name] = {
            "path": path.relative_to("disasm").as_posix(),
            "sha256": hashlib.sha256(text.encode()).hexdigest().upper(),
        }
    return {
        "evidenceLabel": "Confirmed",
        "provenance": {
            "sourceFixturePath": "tests/fixtures/h2/map-script-engine-static-v1.json",
            "sourceFixtureField": "expected.forceStateCommandFacts.activePartyCommandFacts",
            "reproductionCommand": "uv run sf2 h2 map-script-engine",
            "sourcePath": MAP_SOURCE.relative_to("disasm").as_posix(),
            "h1ListingPath": H1_LISTING.as_posix(),
        },
        "handlers": records,
        "callerBreakdown": facts["callerBreakdown"],
        "ownerSources": owners,
    }


def derive_force_state_active_party_cases(static: dict[str, Any]) -> list[dict[str, Any]]:
    """Keep case identities/axes explicit; values are measured by the Lua observer."""
    addresses = {row["handler"]: row["address"] for row in static["handlers"]}
    forms = (
        ("join-already-active", "csc51_joinBattleParty", "already-active"),
        ("join-replace-dead", "csc51_joinBattleParty", "dead-active-replacement"),
        ("join-replace-living-tail", "csc51_joinBattleParty", "living-tail-replacement"),
        ("ai-clear-no-join", "csc54_joinForceAi", "selector-zero"),
        ("ai-set-join", "csc54_joinForceAi", "selector-nonzero"),
        ("reset-mixed-allies", "csc55_resetCharacterBattleStats", "mixed-reset"),
        ("follower-empty", "csc56_addFollower", "empty-list"),
        ("follower-existing", "csc56_addFollower", "existing-chain"),
        ("follower-duplicate", "csc56_addFollower", "duplicate-entity"),
    )
    return [
        {"id": case_id, "handler": handler, "handlerAddress": addresses[handler], "axis": axis}
        for case_id, handler, axis in forms
    ]


def _runtime_contract(static: dict[str, Any], upstream: Path) -> dict[str, Any]:
    """Map each observed call to one H1 call instruction without alias collapse."""
    listing = (upstream / H1_LISTING).read_text(encoding="utf-8")
    addresses = listing_symbol_addresses(listing)
    resolutions = {
        row["instructionTarget"]: row["effectiveTarget"]
        for row in static["callerBreakdown"]["targetResolutions"]
    }
    handlers = []
    for record in static["handlers"]:
        calls = []
        for address, instruction in _h1_rows(listing, record["handler"]):
            match = re.fullmatch(
                r"(?P<opcode>jsr|bsr)(?:\.[bwls])? (?P<target>[A-Za-z_][A-Za-z0-9_]*)", instruction
            )
            if match is not None and match["target"] in resolutions:
                calls.append(
                    {
                        "h1Address": address,
                        "instructionTarget": match["target"],
                        "effectiveTarget": resolutions[match["target"]],
                    }
                )
        if [row["instructionTarget"] for row in calls] != [
            row["instructionTarget"] for row in record["directCalls"]
        ]:
            raise ValueError(f"active-party runtime H1 call identity drift: {record['handler']}")
        h1 = _h1_rows(listing, record["handler"])
        if not h1 or h1[-1][1] != "rts":
            raise ValueError(f"active-party runtime return seam drift: {record['handler']}")
        handlers.append(
            {
                "handler": record["handler"],
                "handlerAddress": record["address"],
                "returnAddress": h1[-1][0],
                "calls": calls,
            }
        )
    required = {
        "RunMapSetupInitFunction",
        "GetCurrentHp",
        "GetMaxHp",
        "SetCurrentHp",
        "GetMaxMp",
        "SetCurrentMp",
        "GetStatusEffects",
        "SetStatusEffects",
        "UpdateCombatantStats",
        *(row["handler"] for row in static["handlers"]),
    }
    if not required <= addresses.keys():
        raise ValueError("active-party runtime H1 seam inventory drift")
    reset_rows = _h1_rows(listing, "ResetAlliesBattleStats")
    reset_targets = (
        ("GHP", "j_GetCurrentHp", "GetCurrentHp"),
        ("GMAX", "j_GetMaxHp", "GetMaxHp"),
        ("SHP", "j_SetCurrentHp", "SetCurrentHp"),
        ("GMP", "j_GetMaxMp", "GetMaxMp"),
        ("SMP", "j_SetCurrentMp", "SetCurrentMp"),
        ("GSTATUS", "j_GetStatusEffects", "GetStatusEffects"),
        ("SSTATUS", "j_SetStatusEffects", "SetStatusEffects"),
        ("UPDATE", "j_UpdateCombatantStats", "UpdateCombatantStats"),
    )
    reset_calls = []
    for code, instruction_target, effective_target in reset_targets:
        matches = [
            (address, instruction)
            for address, instruction in reset_rows
            if instruction == f"jsr {instruction_target}"
        ]
        if len(matches) != 1:
            raise ValueError(f"active-party reset source use-site drift: {instruction_target}")
        reset_calls.append(
            {
                "code": code,
                "h1Address": matches[0][0],
                "instructionTarget": instruction_target,
                "effectiveTarget": effective_target,
            }
        )
    reset_instructions = [instruction for _, instruction in reset_rows]
    required_reset_rows = [f"jsr {target}" for _, target, _ in reset_targets]
    if [item for item in reset_instructions if item in required_reset_rows] != required_reset_rows:
        raise ValueError("active-party reset call order drift")
    if (
        "andi.w #STATUSEFFECT_STUN|STATUSEFFECT_POISON|STATUSEFFECT_CURSE,d1"
        not in reset_instructions
    ):
        raise ValueError("active-party reset status-mask use-site drift")
    follower_rows = _function_section(
        (upstream / OWNER_SOURCES["follower"]).read_text(encoding="utf-8"), "AddFollower"
    )
    parameter_rows = [
        row for row in follower_rows if re.fullmatch(r"move\.w d[123],\$[0-9A-F]+\(a1\)", row)
    ]
    parameter_offsets = [
        int(re.search(r"\$([0-9A-F]+)", row).group(1), 16) for row in parameter_rows
    ]
    if (
        parameter_offsets != [30, 32, 34]
        or "addi.l #42,(ENTITY_WALKING_PARAMETERS).l" not in follower_rows
    ):
        raise ValueError("active-party follower parameter/block use-site drift")
    join_force_rows = _function_section(
        (upstream / OWNER_SOURCES["battleParty"]).read_text(encoding="utf-8"), "JoinForce"
    )
    join_force_guard = [
        "bsr.w SetFlag",
        "bsr.s UpdateForce",
        "cmpi.w #FORCE_MAX_SIZE,((BATTLE_PARTY_MEMBERS_NUMBER-$1000000)).w",
        "bcc.s @SkipActiveForce",
        "bsr.w JoinBattleParty",
    ]
    if [row for row in join_force_rows if row in join_force_guard] != join_force_guard:
        raise ValueError("active-party JoinForce owner chain drift")
    ai_rows = next(
        row["instructions"] for row in static["handlers"] if row["handler"] == "csc54_joinForceAi"
    )
    if (
        "ori.w #AIBITFIELD_AI_CONTROLLED,d1" not in ai_rows
        or "andi.w #($FFFF-AIBITFIELD_AI_CONTROLLED),d1" not in ai_rows
    ):
        raise ValueError("active-party AI-control mask use-site drift")
    if (
        "moveq #COMBATANT_ALLIES_COUNTER,d7" not in reset_instructions
        or "dbf d7,@Loop" not in reset_instructions
    ):
        raise ValueError("active-party reset counter use-site drift")
    follower_order = [
        "movea.l (ENTITY_WALKING_PARAMETERS).l,a1",
        "move.l a1,ENTITYDEF_OFFSET_ACTSCRIPTADDR(a0)",
        "addi.l #42,(ENTITY_WALKING_PARAMETERS).l",
        "move.w d1,$1E(a1)",
        "move.w d2,$20(a1)",
        "move.w d3,$22(a1)",
    ]
    if [row for row in follower_rows if row in follower_order] != follower_order:
        raise ValueError("active-party follower mutation order drift")
    constants = _require_equates(
        _equates(upstream),
        (
            "GAME_FLAGS",
            "FORCEMEMBER_JOINED_FLAGS_START",
            "FORCEMEMBER_ACTIVE_FLAGS_START",
            "COMBATANT_DATA",
            "COMBATANT_OFFSET_HP_MAX",
            "COMBATANT_OFFSET_HP_CURRENT",
            "COMBATANT_OFFSET_MP_MAX",
            "COMBATANT_OFFSET_MP_CURRENT",
            "COMBATANT_OFFSET_STATUSEFFECTS",
            "COMBATANT_OFFSET_ACTIVATION_BITFIELD",
            "COMBATANT_ALLIES_COUNTER",
            "COMBATANT_DATA_ENTRY_SIZE",
            "STATUSEFFECT_STUN",
            "STATUSEFFECT_POISON",
            "STATUSEFFECT_CURSE",
            "AIBITFIELD_AI_CONTROLLED",
            "EXPLORATION_ENTITIES",
            "ENTITY_INDEX_LIST",
            "ENTITY_DATA",
            "ENTITY_WALKING_PARAMETERS",
            "BATTLE_PARTY_MEMBERS",
            "BATTLE_PARTY_MEMBERS_NUMBER",
            "DIALOGUE_NAME_INDEX_1",
            "ENTITYDEF_SIZE",
            "ENTITYDEF_OFFSET_ACTSCRIPTADDR",
            "BYTE_SHIFT_COUNT",
        ),
    )
    force_flag_clear_byte_span = (
        max(
            constants["FORCEMEMBER_JOINED_FLAGS_START"],
            constants["FORCEMEMBER_ACTIVE_FLAGS_START"],
        )
        + constants["COMBATANT_ALLIES_COUNTER"]
        + 1
        + constants["BYTE_SHIFT_COUNT"]
        - 1
    ) // constants["BYTE_SHIFT_COUNT"]
    return {
        "entryAddress": addresses["RunMapSetupInitFunction"],
        "handlers": handlers,
        "resetServices": {
            "calls": reset_calls,
            "allyCounter": constants["COMBATANT_ALLIES_COUNTER"],
            "preUpdateStatusRetainMask": (
                constants["STATUSEFFECT_STUN"]
                | constants["STATUSEFFECT_POISON"]
                | constants["STATUSEFFECT_CURSE"]
            ),
        },
        "follower": {
            "parameterOffsets": parameter_offsets,
            "blockBytes": int(
                re.search(
                    r"#(\d+)", next(row for row in follower_rows if row.startswith("addi.l #"))
                ).group(1)
            ),
        },
        "joinForceOwner": {"orderedInstructions": join_force_guard},
        "aiControl": {
            "mask": constants["AIBITFIELD_AI_CONTROLLED"],
            "setUseSite": "ori.w #AIBITFIELD_AI_CONTROLLED,d1",
            "clearUseSite": "andi.w #($FFFF-AIBITFIELD_AI_CONTROLLED),d1",
        },
        "ram": {
            "gameFlags": constants["GAME_FLAGS"],
            "forceFlagJoinedStart": constants["FORCEMEMBER_JOINED_FLAGS_START"],
            "forceFlagActiveStart": constants["FORCEMEMBER_ACTIVE_FLAGS_START"],
            "forceFlagClearByteSpan": force_flag_clear_byte_span,
            "bitsPerByte": constants["BYTE_SHIFT_COUNT"],
            "firstFlagMask": 1 << (constants["BYTE_SHIFT_COUNT"] - 1),
            "combatantData": constants["COMBATANT_DATA"],
            "combatantEntryBytes": constants["COMBATANT_DATA_ENTRY_SIZE"],
            "maxHpOffset": constants["COMBATANT_OFFSET_HP_MAX"],
            "currentHpOffset": constants["COMBATANT_OFFSET_HP_CURRENT"],
            "maxMpOffset": constants["COMBATANT_OFFSET_MP_MAX"],
            "currentMpOffset": constants["COMBATANT_OFFSET_MP_CURRENT"],
            "statusOffset": constants["COMBATANT_OFFSET_STATUSEFFECTS"],
            "activationOffset": constants["COMBATANT_OFFSET_ACTIVATION_BITFIELD"],
            "explorationEntities": constants["EXPLORATION_ENTITIES"],
            "entityIndexList": constants["ENTITY_INDEX_LIST"],
            "entityData": constants["ENTITY_DATA"],
            "entityWalkingParameters": constants["ENTITY_WALKING_PARAMETERS"],
            "entityEntryBytes": constants["ENTITYDEF_SIZE"],
            "entityActscriptOffset": constants["ENTITYDEF_OFFSET_ACTSCRIPTADDR"],
            "battlePartyMembers": constants["BATTLE_PARTY_MEMBERS"],
            "battlePartyMembersNumber": constants["BATTLE_PARTY_MEMBERS_NUMBER"],
            "dialogueNameIndex": constants["DIALOGUE_NAME_INDEX_1"],
            "followerParameterOffsets": parameter_offsets,
        },
    }


def _instrument_rom(rom_path: Path, fixture: dict[str, Any]) -> Path:
    """Apply the verified, session-only Map Test trampoline; handler bytes remain original."""
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
        raise ValueError("active-party trampoline original call-site bytes drift")
    if data[stub_address : stub_address + len(stub_original)] != stub_original:
        raise ValueError("active-party trampoline original padding bytes drift")
    if patched != b"\x4e\xb9" + stub_address.to_bytes(4, "big") or len(stub) > len(stub_original):
        raise ValueError("active-party trampoline shape/span drift")
    data[call_site : call_site + len(patched)] = patched
    data[stub_address : stub_address + len(stub)] = stub
    data[0x18E:0x190] = int(mega_drive_checksum(bytes(data)), 16).to_bytes(2, "big")
    if inspect_rom(rom_path.resolve(strict=True))["sha256"] != original_hash:
        raise ValueError("active-party instrumentation altered original input")
    DERIVED_ROOT.mkdir(parents=True, exist_ok=True)
    output = DERIVED_ROOT / "force-state-active-party.instrumented.bin"
    output.write_bytes(data)
    return output


def _case_inputs(cases: list[dict[str, Any]], ai_control_mask: int) -> list[dict[str, Any]]:
    streams = {
        "join-already-active": [0, 0],
        "join-replace-dead": [0, 2],
        "join-replace-living-tail": [0, 2],
        "ai-clear-no-join": [0, 3, 0, 0],
        "ai-set-join": [0, 4, 0, 1],
        "reset-mixed-allies": [],
        "follower-empty": [0, 0],
        "follower-existing": [0, 0],
        "follower-duplicate": [0, 0],
    }
    states = {
        "join-already-active": {
            "joined": [0, 1],
            "active": [0, 1],
            "hp": [10, 10],
            "probeMembers": [0, 1, 2],
            "probeCharacter": 0,
        },
        "join-replace-dead": {
            "joined": [0, 1, 2],
            "active": [0, 1],
            "hp": [0, 10],
            "probeMembers": [0, 1, 2],
            "probeCharacter": 0,
        },
        "join-replace-living-tail": {
            "joined": [0, 1, 2],
            "active": [0, 1],
            "hp": [10, 10],
            "probeMembers": [0, 1, 2],
            "probeCharacter": 0,
        },
        "ai-clear-no-join": {
            "joined": [3],
            "active": [],
            "hp": [],
            "activation": ai_control_mask,
            "probeMembers": [3],
            "probeCharacter": 3,
        },
        "ai-set-join": {
            "joined": [],
            "active": [],
            "hp": [],
            "activation": 0,
            "probeMembers": [4],
            "probeCharacter": 4,
        },
        "reset-mixed-allies": {
            "joined": [],
            "active": [],
            "hp": [],
            "reset": True,
            "probeCharacter": 0,
        },
        "follower-empty": {"followers": [-1], "entityIndexAssignments": [0], "probeEntities": [0]},
        "follower-existing": {
            "followers": [1, -1],
            "entityIndexAssignments": [0],
            "probeEntities": [0],
        },
        "follower-duplicate": {
            "followers": [0, -1],
            "entityIndexAssignments": [0],
            "probeEntities": [0],
        },
    }
    return [
        {
            "id": row["id"],
            "handlerAddress": row["handlerAddress"],
            "streamBytes": streams[row["id"]],
            "state": states[row["id"]],
        }
        for row in cases
    ]


def verify_force_state_active_party(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 180
) -> dict[str, Any]:
    """Validate the checked-in bounded H3 matrix and its independently emitted observation."""
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner="force-state active-party fixture")
    validate_json(
        fixture["observation"], OBSERVATION_SCHEMA, owner="force-state active-party observation"
    )
    static = build_force_state_active_party_static_contract(upstream_path, rom_path)
    digest = (
        hashlib.sha256(json.dumps(static, sort_keys=True, separators=(",", ":")).encode())
        .hexdigest()
        .upper()
    )
    if fixture["staticContractSha256"] != digest:
        raise ValueError("force-state active-party static contract digest drift")
    cases = derive_force_state_active_party_cases(static)
    if fixture["cases"] != cases or fixture["observation"]["recordOrder"] != [
        row["id"] for row in cases
    ]:
        raise ValueError("force-state active-party case/observation order drift")
    runtime = _runtime_contract(static, upstream_path.resolve(strict=True))
    if fixture["runtimeContract"] != runtime:
        raise ValueError("force-state active-party runtime contract drift")
    if fixture["caseInputs"] != _case_inputs(cases, runtime["aiControl"]["mask"]):
        raise ValueError("force-state active-party semantic input matrix drift")
    verify_runtime_contract(fixture, rom_path)
    instrumented_rom = _instrument_rom(rom_path, fixture)
    config = {
        "fixtureId": fixture["id"],
        "jsonModulePath": OBSERVER.with_name("json.lua").as_posix(),
        "mapTest": fixture["mapTestIndex"],
        "runtimeContract": runtime,
        "instrumentation": fixture["instrumentation"],
        "cases": cases,
        "caseInputs": fixture["caseInputs"],
        "maxFrames": fixture["maxFrames"],
        "ram": runtime["ram"],
        "harness": load_json(repo_path(fixture["sharedHarnessFixture"]))["harness"],
    }

    def observe() -> dict[str, Any]:
        return run_observer(
            rom_path=instrumented_rom,
            observer_path=OBSERVER,
            config=config,
            output_name="force-state-active-party",
            timeout_seconds=timeout_seconds,
        )

    observed = _with_instrumented_rom_database(instrumented_rom, "SF2 H3 active party", observe)
    validate_json(observed, OBSERVATION_SCHEMA, owner="force-state active-party observed")
    if observed != fixture["observation"]:
        raise ValueError("force-state active-party runtime observation mismatch")
    return {
        "Fixture": fixture["id"],
        "Cases": len(cases),
        "Handlers": len(static["handlers"]),
        "BizHawkLaunches": 1,
        "Instrumentation": "session-only",
        "Status": "PASS",
    }
