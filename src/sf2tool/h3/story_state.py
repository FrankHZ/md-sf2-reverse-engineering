"""Static bridge for the map-script story-state branch/prompt H3 question.

The source establishes instruction, cursor, and callback relations only. It
does not establish normal-story reachability, save/load lifecycle persistence,
or player-visible yes/no presentation and timing; those remain explicit,
separate H3 questions.
"""

from __future__ import annotations

import hashlib
import json
import re
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.h3.bizhawk import DERIVED_ROOT, run_observer, verify_runtime_contract
from sf2tool.h3.map_lifecycle import _with_instrumented_rom_database
from sf2tool.h3.observer_status import (
    assert_observer_status,
    callback_failure_status,
    observer_failure_contract,
)
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.rom import inspect_rom, mega_drive_checksum

H2_FIXTURE = repo_path("tests/fixtures/h2/map-script-engine-static-v1.json")
FIXTURE = repo_path("tests/fixtures/h3/story-state-v2.json")
FIXTURE_SCHEMA = repo_path("schemas/h3-story-state-fixture.schema.json")
OBSERVATION_SCHEMA = repo_path("schemas/h3-story-state-observation.schema.json")
FAILURE_SCHEMA = repo_path("schemas/h3/story-state-callback-failure.schema.json")
OBSERVER = repo_path("tools/bizhawk/story_state_observer.lua")
TOOLCHAIN_MANIFEST = repo_path("manifests/toolchain.json")
TECH_SERVICES_H2_FIXTURE = repo_path("tests/fixtures/h2/tech-services-static-v1.json")
H1_LISTING_PATH = Path("build/sf2build-h1.lst")
MAP_SCRIPT_SOURCE_PATH = Path("disasm/code/common/scripting/map/mapscriptengine_2.asm")
ENUM_SOURCE_PATH = Path("disasm/sf2enums.asm")
CONSTANT_SOURCE_PATH = Path("disasm/sf2const.asm")
GAME_FLAGS_SOURCE_PATH = Path("disasm/code/common/stats/gameflags.asm")
SRAM_SOURCE_PATH = Path("disasm/code/common/tech/sram/sramfunctions.asm")
MAP_SETUP_SOURCE_PATH = Path("disasm/code/common/scripting/map/mapsetupsfunctions_1.asm")
VINT_SOURCE_PATH = Path("disasm/code/common/tech/interrupts/vintengine_1.asm")
WEAPONSPRITE_SOURCE_PATH = Path(
    "disasm/code/gameflow/battle/battlescenes/battlesceneengine_1.asm"
)
ALIAS_SOURCE_PATHS = {
    "j_CheckFlag": Path("disasm/code/common/tech/jumpinterfaces/s02_jumpinterface.asm"),
    "j_ClearFlag": Path("disasm/code/common/tech/jumpinterfaces/s02_jumpinterface.asm"),
    "j_SetFlag": Path("disasm/code/common/tech/jumpinterfaces/s02_jumpinterface.asm"),
    "j_YesNoPrompt": Path("disasm/code/common/tech/jumpinterfaces/s03_jumpinterface_1.asm"),
}
HANDLER_NAMES = (
    "csc0C_jumpIfFlagSet",
    "csc0D_jumpIfFlagClear",
    "csc10_toggleFlag",
    "csc11_promptYesNoForStoryFlow",
    "csc13_setStoryFlag",
)
WIDTHS = {"b": 1, "w": 2, "l": 4}
OWNER = "story-state"
OBSERVER_FAILURE_CONTRACT = observer_failure_contract(OWNER)
V1_CASE_COUNT = 10
RETAINED_V1_PROJECTION_SHA256 = "8245C6580A2E7BB118EE713A723DE385D8AB96551D979795FE5131B5EFA8EC12"
PERSISTENCE_CASE_ORDER = (
    "csc10-set-slot1-save-load-branch",
    "csc10-clear-slot2-save-load-branch",
    "csc11-flag89-set-slot1-save-load-branch",
    "csc11-flag89-clear-slot2-save-load-branch",
    "csc13-flag400-slot1-save-load-branch",
    "csc13-word-wrap-flag0-slot2-save-load-branch",
)
SCRATCH_GAP_START = 0xFF4800
SCRATCH_GAP_END = 0xFF4A00
MAP_LAYOUT_HISTORY_START = 0xFF6000
MAP_LAYOUT_HISTORY_END = 0xFF6800
GENERATED_PROGRAM_BYTE_COUNT = 42
GENERATED_MUTATION_STREAM_BYTE_COUNT = 6
GENERATED_FINAL_STREAM_BYTE_COUNT = 6
TRAMPOLINE_POINTER_BYTE_COUNT = 4
RETAINED_V1_STREAM_ADDRESS = 0xFF4004
RETAINED_V1_STREAM_BYTE_COUNT = 6


def _literal(text: str) -> int:
    if re.fullmatch(r"\$[0-9A-Fa-f]+", text):
        return int(text[1:], 16)
    if re.fullmatch(r"-?\d+", text):
        return int(text)
    raise ValueError(f"story-state source literal is not numeric: {text}")


def _parse_equates(source: str, names: set[str]) -> dict[str, int]:
    values: dict[str, int] = {}
    for name in sorted(names):
        match = re.search(
            rf"^{re.escape(name)}:\s+equ\s+(?P<value>\$[0-9A-Fa-f]+|-?\d+)\b",
            source,
            re.MULTILINE,
        )
        if match is None:
            raise ValueError(f"story-state source equate is missing: {name}")
        values[name] = _literal(match["value"])
    return values


def _scratch_gap_facts(upstream: Path, constant_source: str) -> dict[str, Any]:
    """Prove the bounded generated-code gap without borrowing map-layout RAM."""
    constants = _parse_equates(constant_source, {"byte_FF4000", "byte_FF4A00"})
    if constants != {"byte_FF4000": 0xFF4000, "byte_FF4A00": SCRATCH_GAP_END}:
        raise ValueError("story-state scratch-gap neighbor constant drift")
    lower = re.search(
        r"^byte_FF4000:\s+equ\s+\$FF4000\s*;\s*cleared up to FF4800 after loading weaponsprite\s*$",
        constant_source,
        re.MULTILINE,
    )
    upper = re.search(r"^byte_FF4A00:\s+equ\s+\$FF4A00\s*$", constant_source, re.MULTILINE)
    if lower is None or upper is None:
        raise ValueError("story-state scratch-gap source-neighbor label drift")
    weapon_source = (upstream / WEAPONSPRITE_SOURCE_PATH).read_text(encoding="utf-8")
    clear_rows = _source_section(weapon_source, "LoadWeaponsprite")
    clear_required = (
        "lea (byte_FF4000).l,a0",
        "move.w #511,d0",
        "clr.l (a0)+",
        "dbf d0,@Loop",
    )
    clear_use_sites = _require_instruction_order(
        clear_rows, symbol="LoadWeaponsprite", required=clear_required
    )
    clear_match = re.fullmatch(r"move\.w #(?P<count>\d+),d0", clear_required[1])
    if clear_match is None:
        raise ValueError("story-state scratch-gap clear counter parse drift")
    clear_longword_count = _literal(clear_match["count"]) + 1
    clear_byte_count = clear_longword_count * 4
    if constants["byte_FF4000"] + clear_byte_count != SCRATCH_GAP_START:
        raise ValueError("story-state scratch-gap lower-bound derivation drift")

    reference_pattern = re.compile(rb"\$?FF4[89][0-9A-Fa-f]{2}")
    references: list[dict[str, Any]] = []
    for path in sorted((upstream / "disasm").rglob("*.asm")):
        for line_number, raw in enumerate(path.read_bytes().splitlines(), 1):
            code = raw.split(b";", 1)[0]
            if reference_pattern.search(code):
                references.append(
                    {
                        "sourcePath": path.relative_to(upstream / "disasm").as_posix(),
                        "sourceLine": line_number,
                    }
                )
    if references:
        raise ValueError(f"story-state scratch-gap source reference drift: {references}")
    return {
        "evidenceLabel": "Confirmed",
        "lowerNeighbor": {
            "symbol": "byte_FF4000",
            "address": constants["byte_FF4000"],
            "clearInstruction": clear_required[0],
            "clearCounterInstruction": clear_required[1],
            "clearLongwordCount": clear_longword_count,
            "clearByteCount": clear_byte_count,
            "clearEndExclusive": SCRATCH_GAP_START,
            "clearSourceLine": clear_use_sites[clear_required[0]]["sourceLine"],
        },
        "gap": {
            "startAddress": SCRATCH_GAP_START,
            "endExclusiveAddress": SCRATCH_GAP_END,
            "byteCount": SCRATCH_GAP_END - SCRATCH_GAP_START,
        },
        "upperNeighbor": {"symbol": "byte_FF4A00", "address": constants["byte_FF4A00"]},
        "sourceReferenceAudit": {
            "pattern": "FF48xx-or-FF49xx-code-reference",
            "referenceCount": len(references),
        },
        "rejectedOwnerRange": {
            "symbol": "MAP_LAYOUT_HISTORY_MAP_SIZES",
            "startAddress": MAP_LAYOUT_HISTORY_START,
            "endExclusiveAddress": MAP_LAYOUT_HISTORY_END,
        },
    }


def _generated_scratch_layout(
    instrumentation: dict[str, Any], gap: dict[str, Any]
) -> dict[str, Any]:
    """Validate isolated persistence scratch and the retained v1 input-stream seam."""
    probe = instrumentation.get("persistenceProbe")
    trampoline = instrumentation.get("trampoline")
    retained_v1_stream = instrumentation.get("retainedV1Stream")
    if (
        not isinstance(probe, dict)
        or not isinstance(trampoline, dict)
        or not isinstance(retained_v1_stream, dict)
    ):
        raise ValueError("story-state scratch instrumentation shape drift")
    ranges = [
        {
            "name": "generatedProgram",
            "address": probe.get("programAddress"),
            "byteCount": GENERATED_PROGRAM_BYTE_COUNT,
        },
        {
            "name": "mutationStream",
            "address": probe.get("mutationStreamAddress"),
            "byteCount": GENERATED_MUTATION_STREAM_BYTE_COUNT,
        },
        {
            "name": "finalStream",
            "address": probe.get("finalStreamAddress"),
            "byteCount": GENERATED_FINAL_STREAM_BYTE_COUNT,
        },
    ]
    start = gap["gap"]["startAddress"]
    end = gap["gap"]["endExclusiveAddress"]
    protected = gap["rejectedOwnerRange"]
    occupied: list[tuple[int, int, str]] = []
    for row in ranges:
        address = row["address"]
        if not isinstance(address, int):
            raise ValueError(f"story-state scratch address is not numeric: {row['name']}")
        range_end = address + row["byteCount"]
        if address < protected["endExclusiveAddress"] and range_end > protected["startAddress"]:
            raise ValueError(f"story-state scratch owner-range overlap: {row['name']}")
        if address < start or range_end > end:
            raise ValueError(f"story-state scratch gap escape: {row['name']}")
        for other_start, other_end, other_name in occupied:
            if address < other_end and range_end > other_start:
                raise ValueError(
                    f"story-state scratch range overlap: {row['name']}/{other_name}"
                )
        occupied.append((address, range_end, row["name"]))
    pointer = trampoline.get("ramInputAddress")
    if pointer != 0xFF4000:
        raise ValueError("story-state trampoline pointer scratch drift")
    v1_address = retained_v1_stream.get("address")
    v1_byte_count = retained_v1_stream.get("byteCount")
    if (
        v1_address != RETAINED_V1_STREAM_ADDRESS
        or v1_byte_count != RETAINED_V1_STREAM_BYTE_COUNT
        or pointer + TRAMPOLINE_POINTER_BYTE_COUNT != v1_address
    ):
        raise ValueError("story-state retained-v1 stream/pointer adjacency drift")
    v1_end = v1_address + v1_byte_count
    if v1_address < pointer + TRAMPOLINE_POINTER_BYTE_COUNT or v1_end <= v1_address:
        raise ValueError("story-state retained-v1 stream/pointer overlap drift")
    for other_start, other_end, other_name in occupied:
        if v1_address < other_end and v1_end > other_start:
            raise ValueError(f"story-state retained-v1 stream overlap: {other_name}")
    return {
        "ranges": ranges,
        "pointerScratch": {"address": pointer, "byteCount": TRAMPOLINE_POINTER_BYTE_COUNT},
        "retainedV1Stream": {"address": v1_address, "byteCount": v1_byte_count},
    }


def _source_section(source: str, symbol: str) -> list[dict[str, Any]]:
    """Parse one named function, removing comments before instruction parsing."""
    start = re.search(rf"^{re.escape(symbol)}:\s*$", source, re.MULTILINE)
    if start is None:
        raise ValueError(f"story-state source function is missing: {symbol}")
    end = source.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"story-state source function end is missing: {symbol}")
    first_line = source[: start.start()].count("\n")
    records: list[dict[str, Any]] = []
    for offset, raw in enumerate(source[start.start() : end].splitlines(), 1):
        instruction = re.sub(r"\s+", " ", raw.split(";", 1)[0].strip())
        if instruction and not instruction.endswith(":"):
            records.append({"instruction": instruction, "sourceLine": first_line + offset})
    return records


def _h1_function_lines(listing: str, symbol: str) -> list[tuple[int, str]]:
    """Parse real H1 instructions from one named function only."""
    start = re.search(rf"^[0-9A-F]{{8}}\s+{re.escape(symbol)}:\s*$", listing, re.MULTILINE)
    if start is None:
        raise ValueError(f"story-state H1 function is missing: {symbol}")
    end = listing.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"story-state H1 function end is missing: {symbol}")
    records: list[tuple[int, str]] = []
    for raw in listing[start.start() : end].splitlines():
        match = re.fullmatch(r"(?P<address>[0-9A-F]{8})\s+(?P<body>.*)", raw)
        if match is None:
            continue
        body = match["body"].split(";", 1)[0].strip()
        body = re.sub(r"^(?:[0-9A-F]{2,8}\s+)+", "", body).strip()
        if not body or body.endswith(":"):
            continue
        if re.fullmatch(r"[A-Za-z][A-Za-z0-9_]*(?:\.[bwls])?(?:\s+.+)?", body) is None:
            raise ValueError(f"story-state H1 instruction parse drift: {raw}")
        records.append((int(match["address"], 16), re.sub(r"\s+", " ", body)))
    return records


def _direct_calls(rows: list[dict[str, Any]]) -> list[dict[str, Any]]:
    """Retain only instruction-scoped JSR/BSR targets and their source identity."""
    calls: list[dict[str, Any]] = []
    pattern = re.compile(
        r"(?P<opcode>jsr|bsr)(?:\.(?P<suffix>[bwls]))?\s+"
        r"(?:(?P<bare>[A-Za-z_][A-Za-z0-9_]*)|"
        r"\((?P<wrapped>[A-Za-z_][A-Za-z0-9_]*)\)(?:\.[bwl])?)"
    )
    for row in rows:
        instruction = row["instruction"]
        match = pattern.fullmatch(instruction)
        if match is None:
            continue
        calls.append(
            {
                "opcode": match["opcode"],
                "instructionTarget": match["bare"] or match["wrapped"],
                "sourceLine": row["sourceLine"],
                "instruction": instruction,
            }
        )
    return calls


def _a6_cursor_use_sites(handler: str, rows: list[dict[str, Any]]) -> list[dict[str, Any]]:
    """Parse ordered reads, target loads, and explicit skips from the A6 cursor."""
    records: list[dict[str, Any]] = []
    for row in rows:
        instruction = row["instruction"]
        read = re.fullmatch(
            r"move(?P<address>a)?\.(?P<size>[bwl]) \(a6\)(?P<advance>\+)?"
            r",(?P<target>[ad][0-7])",
            instruction,
        )
        if read is not None:
            byte_count = WIDTHS[read["size"]]
            address_move = read["address"] is not None
            if address_move != read["target"].startswith("a") or (
                address_move and read["size"] != "l"
            ):
                raise ValueError(f"story-state A6 read form drift: {handler}/{instruction}")
            advance = byte_count if read["advance"] else 0
            records.append(
                {
                    "id": f"{handler}:a6:{len(records)}",
                    "kind": "targetRead" if not read["advance"] else "read",
                    "sourceRegister": "a6",
                    "destinationOperand": read["target"],
                    "transferredByteCount": byte_count,
                    "cursorAdvanceByteCount": advance,
                    "instruction": instruction,
                    "sourceLine": row["sourceLine"],
                }
            )
            continue
        skip = re.fullmatch(r"addq\.w #(?P<count>\d+),a6", instruction)
        if skip is not None:
            records.append(
                {
                    "id": f"{handler}:a6:{len(records)}",
                    "kind": "skip",
                    "sourceRegister": "a6",
                    "destinationOperand": "a6",
                    "transferredByteCount": 0,
                    "cursorAdvanceByteCount": _literal(skip["count"]),
                    "instruction": instruction,
                    "sourceLine": row["sourceLine"],
                }
            )
    return records


def _alias_target(source: str, instruction_target: str) -> str:
    """Resolve one jump-interface alias without merging the call identities."""
    rows = _source_section(source, instruction_target)
    instructions = [row["instruction"] for row in rows]
    if len(instructions) != 1:
        raise ValueError(f"story-state alias section drift: {instruction_target}")
    match = re.fullmatch(r"jmp (?P<target>[A-Za-z_][A-Za-z0-9_]*)\(pc\)", instructions[0])
    if match is None:
        raise ValueError(f"story-state alias target drift: {instruction_target}")
    return match["target"]


def _require_h2_story_facts(fixture: dict[str, Any]) -> dict[str, Any]:
    if fixture.get("id") != "sf2-map-script-engine-static-v1":
        raise ValueError("story-state H2 fixture identity drift")
    expected = fixture.get("expected")
    if not isinstance(expected, dict) or set(expected) == set():
        raise ValueError("story-state H2 fixture expected payload is missing")
    facts = expected.get("storyStateCommandFacts")
    if not isinstance(facts, dict):
        raise ValueError("story-state H2 facts are missing")
    required = {
        "macros",
        "handlers",
        "callerBreakdown",
        "runtimeQuestions",
    }
    if not required <= facts.keys():
        raise ValueError("story-state H2 facts shape drift")
    return facts


def _handler_branch_plan(
    handler: str,
    instructions: list[str],
    cursor_use_sites: list[dict[str, Any]],
) -> dict[str, Any]:
    """Bind every promoted branch/call result to the parsed local instruction order."""
    if handler in {"csc0C_jumpIfFlagSet", "csc0D_jumpIfFlagClear"}:
        if len(cursor_use_sites) != 3:
            raise ValueError(f"story-state jump cursor use-site count drift: {handler}")
        flag_read, target_read, skip = cursor_use_sites
        branch = instructions[2]
        match = re.fullmatch(r"(?P<opcode>beq|bne)\.w (?P<target>[A-Za-z_][A-Za-z0-9_]*)", branch)
        if match is None or flag_read["kind"] != "read" or target_read["kind"] != "targetRead":
            raise ValueError(f"story-state jump branch use-site drift: {handler}")
        if skip["kind"] != "skip" or instructions[3] != target_read["instruction"]:
            raise ValueError(f"story-state jump target/skip order drift: {handler}")
        if instructions[5] != skip["instruction"]:
            raise ValueError(f"story-state jump skip target drift: {handler}")
        return {
            "kind": "conditionalTarget",
            "checkFlagCallInstruction": instructions[1],
            "branchInstruction": branch,
            "branchOpcode": match["opcode"],
            "branchTargetLabel": match["target"],
            "flagReadUseSiteId": flag_read["id"],
            "targetReadUseSiteId": target_read["id"],
            "skipUseSiteId": skip["id"],
        }
    if handler == "csc10_toggleFlag":
        if len(cursor_use_sites) != 2:
            raise ValueError("story-state toggle cursor use-site count drift")
        flag_read, selector_read = cursor_use_sites
        if (
            instructions[2] != "bne.s loc_47488"
            or instructions[3] != "jsr j_ClearFlag"
            or instructions[5] != "jsr j_SetFlag"
        ):
            raise ValueError("story-state toggle branch/call order drift")
        return {
            "kind": "selectorMutation",
            "branchInstruction": instructions[2],
            "flagReadUseSiteId": flag_read["id"],
            "selectorReadUseSiteId": selector_read["id"],
            "zeroResultInstructionTarget": "j_ClearFlag",
            "nonzeroResultInstructionTarget": "j_SetFlag",
        }
    if handler == "csc11_promptYesNoForStoryFlow":
        if cursor_use_sites:
            raise ValueError("story-state yes/no unexpectedly reads A6")
        required = (
            "move.l a6,-(sp)",
            "jsr j_YesNoPrompt",
            "movea.l (sp)+,a6",
            "moveq #FLAG_INDEX_YES_NO_PROMPT,d1",
            "tst.w d0",
            "bne.s loc_474A8",
            "jsr j_SetFlag",
            "bra.s loc_474AE",
            "jsr j_ClearFlag",
            "moveq #10,d0",
            "jsr (Sleep).w",
            "rts",
        )
        if tuple(instructions) != required:
            raise ValueError("story-state yes/no branch or Sleep order drift")
        return {
            "kind": "promptResultMutation",
            "promptCallInstruction": instructions[1],
            "cursorSaveInstruction": instructions[0],
            "cursorRestoreInstruction": instructions[2],
            "branchInstruction": instructions[5],
            "zeroResultInstructionTarget": "j_SetFlag",
            "nonzeroResultInstructionTarget": "j_ClearFlag",
            "sleepValueInstruction": instructions[9],
            "sleepCallInstruction": instructions[10],
        }
    if handler == "csc13_setStoryFlag":
        if len(cursor_use_sites) != 1:
            raise ValueError("story-state battle-unlock cursor use-site count drift")
        if tuple(instructions[1:3]) != (
            "addi.w #BATTLE_UNLOCKED_FLAGS_START,d1",
            "jsr j_SetFlag",
        ):
            raise ValueError("story-state battle-unlock add/call order drift")
        return {
            "kind": "wordAddMutation",
            "battleReadUseSiteId": cursor_use_sites[0]["id"],
            "addInstruction": instructions[1],
            "setFlagInstruction": instructions[2],
            "wordWidthBytes": cursor_use_sites[0]["transferredByteCount"],
        }
    raise ValueError(f"story-state unexpected handler: {handler}")


def _handler_record(
    h2_row: dict[str, Any], source: str, listing: str, addresses: dict[str, int]
) -> dict[str, Any]:
    handler = h2_row.get("handler")
    if not isinstance(handler, str) or handler not in HANDLER_NAMES:
        raise ValueError("story-state H2 handler identity drift")
    rows = _source_section(source, handler)
    instructions = [row["instruction"] for row in rows]
    if instructions != h2_row.get("guardedStatements"):
        raise ValueError(f"story-state source section guard drift: {handler}")
    if len(instructions) != h2_row.get("statementCount"):
        raise ValueError(f"story-state source statement count drift: {handler}")
    h1_rows = _h1_function_lines(listing, handler)
    if [instruction for _, instruction in h1_rows] != instructions:
        raise ValueError(f"story-state H1/source instruction order drift: {handler}")
    address = addresses.get(handler)
    if address is None or address != h2_row.get("address"):
        raise ValueError(f"story-state H1 address drift: {handler}")
    cursor_use_sites = _a6_cursor_use_sites(handler, rows)
    read_widths = [
        row["transferredByteCount"]
        for row in cursor_use_sites
        if row["kind"] in {"read", "targetRead"}
    ]
    if read_widths != h2_row.get("cursorReadWidths"):
        raise ValueError(f"story-state H2 cursor read width drift: {handler}")
    calls = _direct_calls(rows)
    h1_calls = [
        (address, instruction)
        for address, instruction in h1_rows
        if re.fullmatch(r"(?:jsr|bsr)(?:\.[bwls])? .+", instruction)
    ]
    if [call["instruction"] for call in calls] != [instruction for _, instruction in h1_calls]:
        raise ValueError(f"story-state H1 direct-call order drift: {handler}")
    expected_direct = h2_row.get("directCalls")
    if [
        {"opcode": call["opcode"], "instructionTarget": call["instructionTarget"]} for call in calls
    ] != expected_direct:
        raise ValueError(f"story-state H2 direct-call identity drift: {handler}")
    direct_call_plan = [
        {**call, "h1Address": address} for call, (address, _) in zip(calls, h1_calls, strict=True)
    ]
    return {
        "handler": handler,
        "h1Address": address,
        "instructions": rows,
        "cursorUseSites": cursor_use_sites,
        "branchPlan": _handler_branch_plan(handler, instructions, cursor_use_sites),
        "directCallPlan": direct_call_plan,
    }


def _caller_plan(
    handler_rows: list[dict[str, Any]], facts: dict[str, Any], upstream: Path
) -> dict[str, Any]:
    """Count parsed call instructions under direct and alias-resolved identities."""
    h2_breakdown = facts["callerBreakdown"]
    target_resolutions = h2_breakdown.get("targetResolutions")
    if not isinstance(target_resolutions, list):
        raise ValueError("story-state H2 target resolution rows are missing")
    direct_targets = [row["instructionTarget"] for row in target_resolutions]
    if len(direct_targets) != len(set(direct_targets)):
        raise ValueError("story-state H2 direct target domain is not unique")
    resolutions: dict[str, dict[str, Any]] = {}
    for row in target_resolutions:
        target = row["instructionTarget"]
        if target in ALIAS_SOURCE_PATHS:
            alias_path = ALIAS_SOURCE_PATHS[target]
            effective = _alias_target((upstream / alias_path).read_text(encoding="utf-8"), target)
            resolution = {
                "instructionTarget": target,
                "effectiveTarget": effective,
                "aliasSourcePath": alias_path.relative_to("disasm").as_posix(),
                "effectiveTargetScope": "external",
            }
        else:
            resolution = {
                "instructionTarget": target,
                "effectiveTarget": target,
                "aliasSourcePath": None,
                "effectiveTargetScope": "external",
            }
        resolutions[target] = resolution
    parsed_resolutions = [resolutions[target] for target in direct_targets]
    if parsed_resolutions != target_resolutions:
        raise ValueError("story-state alias resolution drift")
    effective_targets = [row["effectiveTarget"] for row in parsed_resolutions]
    per_handler: list[dict[str, Any]] = []
    direct_totals: Counter[str] = Counter()
    effective_totals: Counter[str] = Counter()
    for handler in handler_rows:
        calls = handler["directCallPlan"]
        direct_counts = Counter(call["instructionTarget"] for call in calls)
        if not set(direct_counts) <= set(direct_targets):
            raise ValueError(f"story-state unplanned direct target: {handler['handler']}")
        effective_counts = Counter(
            resolutions[call["instructionTarget"]]["effectiveTarget"] for call in calls
        )
        direct_totals.update(direct_counts)
        effective_totals.update(effective_counts)
        per_handler.append(
            {
                "handler": handler["handler"],
                "instructionTargetSiteCounts": {
                    target: direct_counts[target] for target in direct_targets
                },
                "effectiveTargetSiteCounts": {
                    target: effective_counts[target] for target in effective_targets
                },
            }
        )
    direct_total_map = {target: direct_totals[target] for target in direct_targets}
    effective_total_map = {target: effective_totals[target] for target in effective_targets}
    internal_total_map = {target: 0 for target in effective_targets}
    caller_breakdown = {
        "callerHandlers": per_handler,
        "targetResolutions": parsed_resolutions,
        "instructionTargetTotals": direct_total_map,
        "effectiveTargetTotals": effective_total_map,
        "internalEffectiveTargetTotals": internal_total_map,
        "externalEffectiveTargetTotals": effective_total_map,
    }
    if caller_breakdown != h2_breakdown:
        raise ValueError("story-state parsed caller breakdown drift")
    return caller_breakdown


def _flag_storage_facts(
    enum_source: str, constant_source: str, game_flags_source: str, listing: str
) -> dict[str, Any]:
    constants = {
        **_parse_equates(enum_source, {"FLAG_MASK"}),
        **_parse_equates(constant_source, {"GAME_FLAGS"}),
    }
    rows = _source_section(game_flags_source, "GetFlag")
    required_instructions = (
        "andi.l #FLAG_MASK,d1",
        "divu.w #8,d1",
        "lea ((GAME_FLAGS-$1000000)).w,a0",
        "adda.w d1,a0",
        "swap d1",
        "moveq #$FFFFFF80,d0",
        "lsr.b d1,d0",
    )
    records = {row["instruction"]: row for row in rows}
    if tuple(row["instruction"] for row in rows) != (*required_instructions, "rts"):
        raise ValueError("story-state game-flag storage use-site order drift")
    (
        mask_instruction,
        divide_instruction,
        base_instruction,
        add_instruction,
        remainder_instruction,
        mask_seed_instruction,
        shift_instruction,
    ) = required_instructions
    divide_match = re.fullmatch(r"divu\.w #(?P<divisor>\d+),d1", divide_instruction)
    seed_match = re.fullmatch(r"moveq #\$(?P<seed>[0-9A-Fa-f]{8}),d0", mask_seed_instruction)
    if divide_match is None or seed_match is None or remainder_instruction != "swap d1":
        raise ValueError("story-state game-flag storage arithmetic use-site drift")
    divisor = _literal(divide_match["divisor"])
    mask_seed = _literal(f"${seed_match['seed']}") & 0xFF
    if divisor <= 0 or mask_seed != 0x80:
        raise ValueError("story-state game-flag storage arithmetic value drift")
    expected_h1_displacement = (constants["GAME_FLAGS"] - 0x1000000) & 0xFFFF
    h1_displacement = re.search(
        r"^[0-9A-F]{8}\s+41F8\s+(?P<value>[0-9A-F]{4})\s+lea\s+\(\(GAME_FLAGS-\$1000000\)\)\.w,a0",
        listing,
        re.MULTILINE,
    )
    if h1_displacement is None or int(h1_displacement["value"], 16) != expected_h1_displacement:
        raise ValueError("story-state GAME_FLAGS source/H1 displacement drift")
    addressable_byte_span, remainder = divmod(constants["FLAG_MASK"] + 1, divisor)
    if remainder or addressable_byte_span <= 0:
        raise ValueError("story-state game-flag storage span derivation drift")
    return {
        "evidenceLabel": "Confirmed",
        "GAME_FLAGS": {
            "value": constants["GAME_FLAGS"],
            "sourcePath": GAME_FLAGS_SOURCE_PATH.relative_to("disasm").as_posix(),
            "useSite": {
                "instruction": base_instruction,
                "sourceLine": records[base_instruction]["sourceLine"],
            },
        },
        "FLAG_MASK": {
            "value": constants["FLAG_MASK"],
            "sourcePath": GAME_FLAGS_SOURCE_PATH.relative_to("disasm").as_posix(),
            "useSite": {
                "instruction": mask_instruction,
                "sourceLine": records[mask_instruction]["sourceLine"],
            },
        },
        "addressing": {
            "inputWordMaskInstruction": mask_instruction,
            "inputWordMask": constants["FLAG_MASK"],
            "byteDivisorInstruction": divide_instruction,
            "byteDivisor": divisor,
            "addressableByteSpan": addressable_byte_span,
            "addressableByteSpanDerivedFrom": [mask_instruction, divide_instruction],
            "baseInstruction": base_instruction,
            "byteOffsetAddInstruction": add_instruction,
            "remainderInstruction": remainder_instruction,
            "msbMaskSeedInstruction": mask_seed_instruction,
            "msbMaskSeed": mask_seed,
            "bitShiftInstruction": shift_instruction,
        },
    }


def _require_instruction_order(
    rows: list[dict[str, Any]], *, symbol: str, required: tuple[str, ...]
) -> dict[str, dict[str, Any]]:
    """Bind a promoted relation to ordered, comment-free instructions in one function."""
    by_instruction: dict[str, dict[str, Any]] = {}
    cursor = 0
    for expected in required:
        while cursor < len(rows) and rows[cursor]["instruction"] != expected:
            cursor += 1
        if cursor == len(rows):
            raise ValueError(
                f"story-state persistence source use-site drift in {symbol}: {expected}"
            )
        by_instruction[expected] = rows[cursor]
        cursor += 1
    return by_instruction


def _persistence_static_facts(
    *,
    upstream: Path,
    listing: str,
    enum_source: str,
    constant_source: str,
    h1_addresses: dict[str, int],
) -> dict[str, Any]:
    """Derive the narrow story-state save/load bridge from independent owner inputs."""
    h2 = load_json(TECH_SERVICES_H2_FIXTURE)
    facts = h2.get("expected", {}).get("sramFacts")
    if not isinstance(facts, dict):
        raise ValueError("story-state persistence SRAM owner facts are missing")
    layout = facts.get("layout")
    constants = facts.get("constants")
    entries = facts.get("functionEntries")
    if not (
        isinstance(layout, dict) and isinstance(constants, dict) and isinstance(entries, dict)
    ):
        raise ValueError("story-state persistence SRAM owner shape drift")
    addresses = constants.get("addresses")
    sizes = constants.get("sizes")
    if not isinstance(addresses, dict) or not isinstance(sizes, dict):
        raise ValueError("story-state persistence SRAM owner constants drift")
    source_sizes = _parse_equates(enum_source, {"SAVE_SLOT_REAL_SIZE", "SAVE_SLOT_SIZE"})
    source_addresses = _parse_equates(
        constant_source,
        {
            "SAVE1_DATA",
            "SAVE1_CHECKSUM",
            "SAVE2_DATA",
            "SAVE2_CHECKSUM",
            "SAVE_FLAGS",
        },
    )
    if source_sizes != {
        "SAVE_SLOT_REAL_SIZE": sizes.get("SAVE_SLOT_REAL_SIZE"),
        "SAVE_SLOT_SIZE": sizes.get("SAVE_SLOT_SIZE"),
    }:
        raise ValueError("story-state persistence SRAM source-size/owner drift")
    if source_addresses != {
        name: addresses.get(name)
        for name in ("SAVE1_DATA", "SAVE1_CHECKSUM", "SAVE2_DATA", "SAVE2_CHECKSUM", "SAVE_FLAGS")
    }:
        raise ValueError("story-state persistence SRAM source-address/owner drift")
    required_owner_values = {
        "logicalSlotCount": 2,
        "logicalBytesPerSlot": sizes.get("SAVE_SLOT_REAL_SIZE"),
        "physicalAddressStepPerLogicalByte": 2,
        "physicalAddressIntervalPerSlot": sizes.get("SAVE_SLOT_SIZE"),
    }
    for name, expected in required_owner_values.items():
        if layout.get(name) != expected:
            raise ValueError(f"story-state persistence SRAM owner derivation drift: {name}")
    if layout.get("occupiedFlagBits") != {"slot1": 0, "slot2": 1}:
        raise ValueError("story-state persistence occupied-bit owner drift")
    if tuple(layout.get("slotSelector", {}).items()) != (("zero", "slot1"), ("nonZero", "slot2")):
        raise ValueError("story-state persistence slot-selector owner drift")
    if tuple(entries.get(name) for name in ("SaveGame", "LoadGame")) != tuple(
        h1_addresses.get(name) for name in ("SaveGame", "LoadGame")
    ):
        raise ValueError("story-state persistence SRAM H2/H1 entry drift")

    source = (upstream / SRAM_SOURCE_PATH).read_text(encoding="utf-8")
    save_rows = _source_section(source, "SaveGame")
    load_rows = _source_section(source, "LoadGame")
    save_use_sites = _require_instruction_order(
        save_rows,
        symbol="SaveGame",
        required=(
            "lea (COMBATANT_DATA).l,a0",
            "tst.b d0",
            "lea (SAVE1_DATA).l,a1",
            "lea (SAVE1_CHECKSUM).l,a2",
            "lea (SAVE2_DATA).l,a1",
            "lea (SAVE2_CHECKSUM).l,a2",
            "move.w #SAVE_SLOT_REAL_SIZE,d7",
            "bsr.w CopyBytesToSram",
            "move.b d0,(a2)",
            "bset d1,(SAVE_FLAGS).l",
        ),
    )
    load_use_sites = _require_instruction_order(
        load_rows,
        symbol="LoadGame",
        required=(
            "lea (COMBATANT_DATA).l,a1",
            "tst.b d0",
            "lea (SAVE1_DATA).l,a0",
            "lea (SAVE2_DATA).l,a0",
            "move.w #SAVE_SLOT_REAL_SIZE,d7",
            "bsr.w CopyBytesFromSram",
        ),
    )
    for symbol, rows in (("SaveGame", save_rows), ("LoadGame", load_rows)):
        h1_rows = _h1_function_lines(listing, symbol)
        if [instruction for _, instruction in h1_rows] != [row["instruction"] for row in rows]:
            raise ValueError(f"story-state persistence H1/source instruction order drift: {symbol}")

    ram_constants = _parse_equates(constant_source, {"COMBATANT_DATA", "GAME_FLAGS"})
    logical_byte_count = source_sizes["SAVE_SLOT_REAL_SIZE"]
    game_flags_offset = ram_constants["GAME_FLAGS"] - ram_constants["COMBATANT_DATA"]
    if not 0 <= game_flags_offset < logical_byte_count:
        raise ValueError("story-state persistence GAME_FLAGS offset escapes COMBATANT_DATA")
    slot_rows = []
    for selector, slot in ((0, "slot1"), (1, "slot2")):
        data_address = source_addresses[f"SAVE{selector + 1}_DATA"]
        checksum_address = source_addresses[f"SAVE{selector + 1}_CHECKSUM"]
        slot_rows.append(
            {
                "selector": selector,
                "slot": slot,
                "selectedDataAddress": data_address,
                "selectedChecksumAddress": checksum_address,
                "occupiedFlagBit": layout["occupiedFlagBits"][slot],
                "selectedPhysicalByteStride": source_sizes["SAVE_SLOT_SIZE"]
                // source_sizes["SAVE_SLOT_REAL_SIZE"],
                "selectedPhysicalAddressInterval": source_sizes["SAVE_SLOT_SIZE"],
                "selectedFlagByteAddress": data_address
                + game_flags_offset
                * (source_sizes["SAVE_SLOT_SIZE"] // source_sizes["SAVE_SLOT_REAL_SIZE"]),
            }
        )
    if slot_rows[1]["selectedDataAddress"] <= slot_rows[0]["selectedDataAddress"]:
        raise ValueError("story-state persistence slot address ordering drift")
    return {
        "evidenceLabel": "Confirmed",
        "h2Owner": {
            "fixturePath": display_path(TECH_SERVICES_H2_FIXTURE),
            "fixtureId": h2.get("id"),
            "field": "expected.sramFacts",
            "reproductionCommand": "uv run sf2 h2 tech-services",
        },
        "ramLogicalSpan": {
            "baseAddress": ram_constants["COMBATANT_DATA"],
            "logicalByteCount": logical_byte_count,
            "gameFlagsAddress": ram_constants["GAME_FLAGS"],
            "gameFlagsOffset": game_flags_offset,
        },
        "saveLoadFunctions": {
            "SaveGame": {
                "h1Address": entries["SaveGame"],
                "copyCallInstruction": "bsr.w CopyBytesToSram",
                "copyCallSourceLine": save_use_sites["bsr.w CopyBytesToSram"]["sourceLine"],
                "copyCounterInstruction": "move.w #SAVE_SLOT_REAL_SIZE,d7",
                "copyCounterSourceLine": save_use_sites[
                    "move.w #SAVE_SLOT_REAL_SIZE,d7"
                ]["sourceLine"],
                "checksumWriteInstruction": "move.b d0,(a2)",
                "checksumWriteSourceLine": save_use_sites["move.b d0,(a2)"]["sourceLine"],
                "occupiedFlagInstruction": "bset d1,(SAVE_FLAGS).l",
                "occupiedFlagSourceLine": save_use_sites["bset d1,(SAVE_FLAGS).l"]["sourceLine"],
            },
            "LoadGame": {
                "h1Address": entries["LoadGame"],
                "copyCallInstruction": "bsr.w CopyBytesFromSram",
                "copyCallSourceLine": load_use_sites["bsr.w CopyBytesFromSram"]["sourceLine"],
                "copyCounterInstruction": "move.w #SAVE_SLOT_REAL_SIZE,d7",
                "copyCounterSourceLine": load_use_sites[
                    "move.w #SAVE_SLOT_REAL_SIZE,d7"
                ]["sourceLine"],
            },
        },
        "slotSelections": slot_rows,
        "saveFlagsAddress": source_addresses["SAVE_FLAGS"],
        "physicalWindowBaseAddress": addresses["SRAM_START"] & ~1,
    }


def _source_form_summary(facts: dict[str, Any]) -> list[dict[str, Any]]:
    summary = []
    for macro in facts["macros"]:
        required = {
            "name",
            "opcode",
            "encodedBytes",
            "operandLayout",
            "aliasOf",
            "handler",
            "sourceCommandCount",
            "operandBytes",
            "parameterOrdinals",
        }
        if set(macro) != required:
            raise ValueError(f"story-state H2 macro shape drift: {macro.get('name')}")
        selector = None
        if macro["name"] in {"setF", "clearF"}:
            selector = _literal(macro["operandLayout"][1]["expression"])
        summary.append(
            {
                "name": macro["name"],
                "opcode": macro["opcode"],
                "encodedBytes": macro["encodedBytes"],
                "aliasOf": macro["aliasOf"],
                "handler": macro["handler"],
                "sourceCommandCount": macro["sourceCommandCount"],
                "selectorValue": selector,
            }
        )
    expected_names = (
        "jumpIfFlagSet",
        "jumpIfFlagClear",
        "csc10",
        "setF",
        "clearF",
        "yesNo",
        "setStoryFlag",
    )
    if tuple(row["name"] for row in summary) != expected_names:
        raise ValueError("story-state H2 macro order drift")
    return summary


def build_story_state_static_contract(upstream_path: Path) -> dict[str, Any]:
    """Build the five-handler static H3 bridge before any runtime fixture exists."""
    fixture = load_json(H2_FIXTURE)
    facts = _require_h2_story_facts(fixture)
    upstream = upstream_path.resolve(strict=True)
    source = (upstream / MAP_SCRIPT_SOURCE_PATH).read_text(encoding="utf-8")
    listing = (upstream / H1_LISTING_PATH).read_text(encoding="utf-8")
    addresses = listing_symbol_addresses(listing)
    h2_handlers = facts["handlers"]
    if [row.get("handler") for row in h2_handlers] != list(HANDLER_NAMES):
        raise ValueError("story-state H2 handler order drift")
    handlers = [_handler_record(row, source, listing, addresses) for row in h2_handlers]
    enum_source = (upstream / ENUM_SOURCE_PATH).read_text(encoding="utf-8")
    constant_source = (upstream / CONSTANT_SOURCE_PATH).read_text(encoding="utf-8")
    required_constants = _parse_equates(
        enum_source,
        {"FLAG_INDEX_YES_NO_PROMPT", "BATTLE_UNLOCKED_FLAGS_START"},
    )
    prompt = next(row for row in handlers if row["handler"] == "csc11_promptYesNoForStoryFlow")
    story_flag = next(row for row in handlers if row["handler"] == "csc13_setStoryFlag")
    if prompt["branchPlan"]["sleepValueInstruction"] != "moveq #10,d0":
        raise ValueError("story-state Sleep literal use-site drift")
    constants = {
        "evidenceLabel": "Confirmed",
        "FLAG_INDEX_YES_NO_PROMPT": {
            "value": required_constants["FLAG_INDEX_YES_NO_PROMPT"],
            "handler": prompt["handler"],
            "instruction": "moveq #FLAG_INDEX_YES_NO_PROMPT,d1",
        },
        "BATTLE_UNLOCKED_FLAGS_START": {
            "value": required_constants["BATTLE_UNLOCKED_FLAGS_START"],
            "handler": story_flag["handler"],
            "instruction": story_flag["branchPlan"]["addInstruction"],
        },
    }
    return {
        "evidenceLabel": "Confirmed",
        "provenance": {
            "sourceFixturePath": display_path(H2_FIXTURE),
            "sourceFixtureId": fixture["id"],
            "sourceFixtureField": "expected.storyStateCommandFacts",
            "reproductionCommand": "uv run sf2 h2 map-script-engine",
            "sourcePath": MAP_SCRIPT_SOURCE_PATH.relative_to("disasm").as_posix(),
            "h1ListingPath": H1_LISTING_PATH.as_posix(),
            "upstream": {
                field: load_json(TOOLCHAIN_MANIFEST)["sf2disasm"][field]
                for field in ("repository", "commit", "branch")
            },
        },
        "sourceForms": _source_form_summary(facts),
        "handlers": handlers,
        "constants": constants,
        "flagStorage": _flag_storage_facts(
            enum_source,
            constant_source,
            (upstream / GAME_FLAGS_SOURCE_PATH).read_text(encoding="utf-8"),
            listing,
        ),
        "saveLoadPersistence": _persistence_static_facts(
            upstream=upstream,
            listing=listing,
            enum_source=enum_source,
            constant_source=constant_source,
            h1_addresses=addresses,
        ),
        "scratchGap": _scratch_gap_facts(upstream, constant_source),
        "callerBreakdown": _caller_plan(handlers, facts, upstream),
        "runtimeQuestionQueue": [
            {"id": question, "evidenceLabel": "Unknown"} for question in facts["runtimeQuestions"]
        ],
    }


def _handler_by_name(static: dict[str, Any], name: str) -> dict[str, Any]:
    handlers = static.get("handlers")
    if not isinstance(handlers, list):
        raise ValueError("story-state static handler records are missing")
    matches = [row for row in handlers if row.get("handler") == name]
    if len(matches) != 1:
        raise ValueError(f"story-state static handler lookup drift: {name}")
    return matches[0]


def _cursor_offset_after(
    use_sites: list[dict[str, Any]], input_cursor_offset_bytes: int, *, through_id: str
) -> int:
    """Derive a relative cursor offset from the parsed operations, not command width math."""
    if not isinstance(input_cursor_offset_bytes, int) or input_cursor_offset_bytes < 0:
        raise ValueError("story-state input cursor offset must be a non-negative integer")
    offset = input_cursor_offset_bytes
    for use_site in use_sites:
        offset += use_site["cursorAdvanceByteCount"]
        if use_site["id"] == through_id:
            return offset
    raise ValueError(f"story-state cursor use site is missing: {through_id}")


def _flag_storage_record(
    static: dict[str, Any],
    *,
    flag_index_input_word: int,
    initial_flag_set: bool,
    final_flag_set: bool,
) -> dict[str, Any]:
    """Derive one real GAME_FLAGS bit location from the parsed GetFlag instruction plan."""
    storage = static["flagStorage"]["addressing"]
    if not isinstance(flag_index_input_word, int) or not 0 <= flag_index_input_word <= 0xFFFF:
        raise ValueError("story-state flag index must be an unsigned word")
    effective_flag_index = flag_index_input_word & storage["inputWordMask"]
    byte_offset = effective_flag_index // storage["byteDivisor"]
    bit_index = effective_flag_index % storage["byteDivisor"]
    bit_mask = storage["msbMaskSeed"] >> bit_index
    return {
        "flagIndexInputWord": flag_index_input_word,
        "effectiveFlagIndex": effective_flag_index,
        "flagByteOffset": byte_offset,
        "flagStorageAddress": static["flagStorage"]["GAME_FLAGS"]["value"] + byte_offset,
        "flagBitMask": bit_mask,
        "initialFlagSet": initial_flag_set,
        "finalFlagSet": final_flag_set,
        "derivedFromFlagStorageInstructions": [
            storage["inputWordMaskInstruction"],
            storage["byteDivisorInstruction"],
            storage["baseInstruction"],
            storage["byteOffsetAddInstruction"],
            storage["remainderInstruction"],
            storage["msbMaskSeedInstruction"],
            storage["bitShiftInstruction"],
        ],
    }


def _jump_case(
    static: dict[str, Any],
    handler: dict[str, Any],
    *,
    flag_index_input: int,
    initial_flag_set: bool,
    target_value: int,
    input_offset: int,
) -> dict[str, Any]:
    plan = handler["branchPlan"]
    use_sites = handler["cursorUseSites"]
    check_result_zero = not initial_flag_set
    branch_on_zero = plan["branchOpcode"] == "beq"
    use_target = check_result_zero != branch_on_zero
    if not isinstance(target_value, int) or not 0 <= target_value <= 0xFFFFFFFF:
        raise ValueError("story-state target value must be an unsigned longword")
    before_target = _cursor_offset_after(
        use_sites, input_offset, through_id=plan["flagReadUseSiteId"]
    )
    if use_target:
        cursor = {
            "kind": "targetValue",
            "value": target_value,
            "inputOffsetBeforeTargetRead": before_target,
            "targetReadUseSiteId": plan["targetReadUseSiteId"],
        }
    else:
        cursor = {
            "kind": "inputOffset",
            "value": _cursor_offset_after(
                use_sites, input_offset, through_id=plan["skipUseSiteId"]
            ),
            "skipUseSiteId": plan["skipUseSiteId"],
        }
    return {
        "handler": handler["handler"],
        "h1Address": handler["h1Address"],
        "flagStorage": _flag_storage_record(
            static,
            flag_index_input_word=flag_index_input,
            initial_flag_set=initial_flag_set,
            final_flag_set=initial_flag_set,
        ),
        "checkFlagResultZero": check_result_zero,
        "checkFlagInstructionTarget": "j_CheckFlag",
        "checkFlagEffectiveTarget": "CheckFlag",
        "targetInputValue": target_value,
        "cursor": cursor,
        "derivedFromUseSiteIds": [
            plan["flagReadUseSiteId"],
            plan["targetReadUseSiteId"],
            plan["skipUseSiteId"],
        ],
    }


def _toggle_case(
    static: dict[str, Any], macro: str, flag_index: int, input_offset: int
) -> dict[str, Any]:
    handler = _handler_by_name(static, "csc10_toggleFlag")
    form = next(row for row in static["sourceForms"] if row["name"] == macro)
    selector = form["selectorValue"]
    if not isinstance(selector, int):
        raise ValueError(f"story-state alias selector is missing: {macro}")
    plan = handler["branchPlan"]
    use_sites = handler["cursorUseSites"]
    target = (
        plan["nonzeroResultInstructionTarget"] if selector else plan["zeroResultInstructionTarget"]
    )
    effective = next(
        row["effectiveTarget"]
        for row in static["callerBreakdown"]["targetResolutions"]
        if row["instructionTarget"] == target
    )
    initial_flag_set = macro == "clearF"
    return {
        "handler": handler["handler"],
        "h1Address": handler["h1Address"],
        "sourceForm": macro,
        "flagIndexInput": flag_index,
        "selectorInput": selector,
        "expectedInstructionTarget": target,
        "expectedEffectiveTarget": effective,
        "flagStorage": _flag_storage_record(
            static,
            flag_index_input_word=flag_index,
            initial_flag_set=initial_flag_set,
            final_flag_set=not initial_flag_set,
        ),
        "cursorOutputOffsetBytes": _cursor_offset_after(
            use_sites, input_offset, through_id=plan["selectorReadUseSiteId"]
        ),
        "derivedFromUseSiteIds": [plan["flagReadUseSiteId"], plan["selectorReadUseSiteId"]],
    }


def _yes_no_case(static: dict[str, Any], result_zero: bool, input_offset: int) -> dict[str, Any]:
    handler = _handler_by_name(static, "csc11_promptYesNoForStoryFlow")
    plan = handler["branchPlan"]
    target = (
        plan["zeroResultInstructionTarget"]
        if result_zero
        else plan["nonzeroResultInstructionTarget"]
    )
    effective = next(
        row["effectiveTarget"]
        for row in static["callerBreakdown"]["targetResolutions"]
        if row["instructionTarget"] == target
    )
    return {
        "handler": handler["handler"],
        "h1Address": handler["h1Address"],
        "promptResultZero": result_zero,
        "flagStorage": _flag_storage_record(
            static,
            flag_index_input_word=static["constants"]["FLAG_INDEX_YES_NO_PROMPT"]["value"],
            initial_flag_set=not result_zero,
            final_flag_set=result_zero,
        ),
        "expectedInstructionTarget": target,
        "expectedEffectiveTarget": effective,
        "sleepInputValue": _literal(
            plan["sleepValueInstruction"].split("#", 1)[1].split(",", 1)[0]
        ),
        "cursorOutputOffsetBytes": input_offset,
        "derivedFromInstructions": [
            plan["cursorSaveInstruction"],
            plan["cursorRestoreInstruction"],
            plan["branchInstruction"],
            plan["sleepValueInstruction"],
            plan["sleepCallInstruction"],
        ],
    }


def _story_flag_case(
    static: dict[str, Any], battle_input: int, input_offset: int
) -> dict[str, Any]:
    handler = _handler_by_name(static, "csc13_setStoryFlag")
    plan = handler["branchPlan"]
    width = plan["wordWidthBytes"]
    if not isinstance(battle_input, int) or not 0 <= battle_input < 1 << (width * 8):
        raise ValueError("story-state battle input width drift")
    modulus = 1 << (width * 8)
    result_flag_index = (
        battle_input + static["constants"]["BATTLE_UNLOCKED_FLAGS_START"]["value"]
    ) % modulus
    return {
        "handler": handler["handler"],
        "h1Address": handler["h1Address"],
        "battleInputWord": battle_input,
        "resultFlagIndexWord": result_flag_index,
        "expectedInstructionTarget": "j_SetFlag",
        "expectedEffectiveTarget": "SetFlag",
        "flagStorage": _flag_storage_record(
            static,
            flag_index_input_word=result_flag_index,
            initial_flag_set=False,
            final_flag_set=True,
        ),
        "cursorOutputOffsetBytes": _cursor_offset_after(
            handler["cursorUseSites"], input_offset, through_id=plan["battleReadUseSiteId"]
        ),
        "derivedFromUseSiteIds": [plan["battleReadUseSiteId"]],
        "derivedFromInstruction": plan["addInstruction"],
    }


def derive_story_state_case_matrix(
    static: dict[str, Any], *, input_cursor_offset_bytes: int = 0
) -> list[dict[str, Any]]:
    """Derive the bounded ten-case H3 expectation matrix entirely in memory."""
    story_start = static["constants"]["BATTLE_UNLOCKED_FLAGS_START"]["value"]
    story_handler = _handler_by_name(static, "csc13_setStoryFlag")
    word_width = story_handler["branchPlan"]["wordWidthBytes"]
    word_modulus = 1 << (word_width * 8)
    cases = [
        (
            "jump-if-flag-set-zero-skip",
            _jump_case(
                static,
                _handler_by_name(static, "csc0C_jumpIfFlagSet"),
                flag_index_input=8,
                initial_flag_set=False,
                target_value=0x00123456,
                input_offset=input_cursor_offset_bytes,
            ),
        ),
        (
            "jump-if-flag-set-nonzero-target",
            _jump_case(
                static,
                _handler_by_name(static, "csc0C_jumpIfFlagSet"),
                flag_index_input=8,
                initial_flag_set=True,
                target_value=0x00123456,
                input_offset=input_cursor_offset_bytes,
            ),
        ),
        (
            "jump-if-flag-clear-zero-target",
            _jump_case(
                static,
                _handler_by_name(static, "csc0D_jumpIfFlagClear"),
                flag_index_input=71,
                initial_flag_set=False,
                target_value=0x00654321,
                input_offset=input_cursor_offset_bytes,
            ),
        ),
        (
            "jump-if-flag-clear-nonzero-skip",
            _jump_case(
                static,
                _handler_by_name(static, "csc0D_jumpIfFlagClear"),
                flag_index_input=71,
                initial_flag_set=True,
                target_value=0x00654321,
                input_offset=input_cursor_offset_bytes,
            ),
        ),
        ("set-f-nonzero-selector", _toggle_case(static, "setF", 0x001F, input_cursor_offset_bytes)),
        (
            "clear-f-zero-selector",
            _toggle_case(static, "clearF", 0x0020, input_cursor_offset_bytes),
        ),
        ("yes-no-zero-set", _yes_no_case(static, True, input_cursor_offset_bytes)),
        ("yes-no-nonzero-clear", _yes_no_case(static, False, input_cursor_offset_bytes)),
        ("set-story-flag-base", _story_flag_case(static, 0, input_cursor_offset_bytes)),
        (
            "set-story-flag-word-wrap-boundary",
            _story_flag_case(static, word_modulus - story_start, input_cursor_offset_bytes),
        ),
    ]
    return [{"id": case_id, "expected": expected} for case_id, expected in cases]


def _persistence_case(
    static: dict[str, Any],
    *,
    case_id: str,
    selector: int,
    mutation: dict[str, Any],
    pattern_seed: int,
) -> dict[str, Any]:
    """Connect a mutation to SaveGame, inverse poison, LoadGame, and a branch readback."""
    if selector not in (0, 1):
        raise ValueError("story-state persistence selector must be an original slot selector")
    storage = mutation["flagStorage"]
    final_flag_set = storage["finalFlagSet"]
    final_handler = (
        _handler_by_name(static, "csc0C_jumpIfFlagSet")
        if final_flag_set
        else _handler_by_name(static, "csc0D_jumpIfFlagClear")
    )
    final_plan = final_handler["branchPlan"]
    if final_plan["kind"] != "conditionalTarget":
        raise ValueError("story-state persistence final handler branch plan drift")
    slot = next(
        row
        for row in static["saveLoadPersistence"]["slotSelections"]
        if row["selector"] == selector
    )
    logical_offset = (
        static["saveLoadPersistence"]["ramLogicalSpan"]["gameFlagsOffset"]
        + storage["flagByteOffset"]
    )
    if logical_offset >= static["saveLoadPersistence"]["ramLogicalSpan"]["logicalByteCount"]:
        raise ValueError("story-state persistence selected flag byte exceeds copied logical span")
    selected_physical_address = (
        slot["selectedDataAddress"] + logical_offset * slot["selectedPhysicalByteStride"]
    )
    if selected_physical_address != (
        slot["selectedFlagByteAddress"]
        + storage["flagByteOffset"] * slot["selectedPhysicalByteStride"]
    ):
        raise ValueError("story-state persistence selected physical byte derivation drift")
    initial_byte = storage["flagBitMask"] if storage["initialFlagSet"] else 0
    mutated_byte = storage["flagBitMask"] if final_flag_set else 0
    poisoned_byte = storage["flagBitMask"] if not final_flag_set else 0
    if not 0 <= pattern_seed <= 0xFF:
        raise ValueError("story-state persistence pattern seed must be a byte")
    return {
        "id": case_id,
        "expected": {
            "selector": selector,
            "selectedSlot": {
                **slot,
                "gameFlagsLogicalOffset": logical_offset,
                "selectedFlagPhysicalAddress": selected_physical_address,
            },
            "mutation": mutation,
            "finalCheck": {
                "handler": final_handler["handler"],
                "h1Address": final_handler["h1Address"],
                "instructionTarget": "j_CheckFlag",
                "effectiveTarget": "CheckFlag",
                "branchOpcode": final_plan["branchOpcode"],
                "flagStorage": _flag_storage_record(
                    static,
                    flag_index_input_word=storage["flagIndexInputWord"],
                    initial_flag_set=final_flag_set,
                    final_flag_set=final_flag_set,
                ),
            },
            "stateBytes": {
                "before": initial_byte,
                "mutated": mutated_byte,
                "poisoned": poisoned_byte,
                "restored": mutated_byte,
            },
            "ramPatternSeed": pattern_seed,
            "derivedFrom": {
                "saveCopyInstruction": static["saveLoadPersistence"]["saveLoadFunctions"][
                    "SaveGame"
                ]["copyCallInstruction"],
                "loadCopyInstruction": static["saveLoadPersistence"]["saveLoadFunctions"][
                    "LoadGame"
                ]["copyCallInstruction"],
                "finalCheckBranchInstruction": final_plan["branchInstruction"],
            },
        },
    }


def derive_story_state_persistence_case_matrix(static: dict[str, Any]) -> list[dict[str, Any]]:
    """Derive the six fixed in-process story-state save/load probes without fixture truth."""
    cases = (
        _persistence_case(
            static,
            case_id=PERSISTENCE_CASE_ORDER[0],
            selector=0,
            mutation=_toggle_case(static, "setF", 31, 0),
            pattern_seed=0x21,
        ),
        _persistence_case(
            static,
            case_id=PERSISTENCE_CASE_ORDER[1],
            selector=1,
            mutation=_toggle_case(static, "clearF", 32, 0),
            pattern_seed=0x42,
        ),
        _persistence_case(
            static,
            case_id=PERSISTENCE_CASE_ORDER[2],
            selector=0,
            mutation=_yes_no_case(static, True, 0),
            pattern_seed=0x63,
        ),
        _persistence_case(
            static,
            case_id=PERSISTENCE_CASE_ORDER[3],
            selector=1,
            mutation=_yes_no_case(static, False, 0),
            pattern_seed=0x84,
        ),
        _persistence_case(
            static,
            case_id=PERSISTENCE_CASE_ORDER[4],
            selector=0,
            mutation=_story_flag_case(static, 0, 0),
            pattern_seed=0xA5,
        ),
        _persistence_case(
            static,
            case_id=PERSISTENCE_CASE_ORDER[5],
            selector=1,
            mutation=_story_flag_case(
                static,
                (
                    1
                    << (
                        8
                        * _handler_by_name(static, "csc13_setStoryFlag")["branchPlan"][
                            "wordWidthBytes"
                        ]
                    )
                )
                - static["constants"]["BATTLE_UNLOCKED_FLAGS_START"]["value"],
                0,
            ),
            pattern_seed=0xC6,
        ),
    )
    if tuple(case["id"] for case in cases) != PERSISTENCE_CASE_ORDER:
        raise ValueError("story-state persistence case order drift")
    return list(cases)


def persistence_fixture_projection(derived: list[dict[str, Any]]) -> list[dict[str, Any]]:
    """Keep the six fixture rows compact while construction stays source-derived."""
    projection = []
    for case in derived:
        expected = case["expected"]
        mutation = expected["mutation"]
        row = {
            "id": case["id"],
            "selector": expected["selector"],
            "mutationHandler": mutation["handler"],
            "mutationFlagIndexWord": mutation["flagStorage"]["flagIndexInputWord"],
            "mutationFinalFlagSet": mutation["flagStorage"]["finalFlagSet"],
            "finalCheckHandler": expected["finalCheck"]["handler"],
            "ramPatternSeed": expected["ramPatternSeed"],
        }
        if "sourceForm" in mutation:
            row["sourceForm"] = mutation["sourceForm"]
        if "promptResultZero" in mutation:
            row["promptResultZero"] = mutation["promptResultZero"]
        if "battleInputWord" in mutation:
            row["battleInputWord"] = mutation["battleInputWord"]
        projection.append(row)
    return projection


def _runtime_contract(static: dict[str, Any], upstream: Path) -> dict[str, Any]:
    """Bind the H3 observer only to the static handler, service, and cursor seams."""
    listing = (upstream / H1_LISTING_PATH).read_text(encoding="utf-8")
    addresses = listing_symbol_addresses(listing)
    required_symbols = {
        "RunMapSetupInitFunction",
        "CheckFlag",
        "SetFlag",
        "ClearFlag",
        "Sleep",
        "YesNoPrompt",
        "j_YesNoPrompt",
        "SaveGame",
        "LoadGame",
        "WaitForVInt",
    }
    if not required_symbols <= addresses.keys():
        raise ValueError("story-state runtime H1 seam symbols are missing")
    resolutions = {
        row["instructionTarget"]: row["effectiveTarget"]
        for row in static["callerBreakdown"]["targetResolutions"]
    }
    handler_records = []
    for handler in static["handlers"]:
        source_rows = _h1_function_lines(listing, handler["handler"])
        instructions_by_address = {instruction: address for address, instruction in source_rows}
        next_address_by_address = {
            address: source_rows[index + 1][0]
            for index, (address, _) in enumerate(source_rows[:-1])
        }
        cursor_sites = []
        for site in handler["cursorUseSites"]:
            address = instructions_by_address.get(site["instruction"])
            if address is None:
                raise ValueError(
                    f"story-state runtime cursor H1 use-site drift: {handler['handler']}"
                )
            cursor_sites.append({"id": site["id"], "h1Address": address})
        calls = []
        for call in handler["directCallPlan"]:
            effective = resolutions.get(call["instructionTarget"])
            if effective is None:
                raise ValueError("story-state runtime direct/effective target drift")
            calls.append(
                {
                    "h1Address": call["h1Address"],
                    "returnAddress": next_address_by_address.get(call["h1Address"]),
                    "instructionTarget": call["instructionTarget"],
                    "effectiveTarget": effective,
                }
            )
            if calls[-1]["returnAddress"] is None:
                raise ValueError("story-state runtime direct-call return derivation drift")
        handler_records.append(
            {
                "handler": handler["handler"],
                "h1Address": handler["h1Address"],
                "cursorUseSites": cursor_sites,
                "directCalls": calls,
            }
        )
    wrapper_rows = _h1_function_lines(listing, "RunMapSetupInitFunction")
    wrapper_instructions = [instruction for _, instruction in wrapper_rows]
    required_wrapper = (
        "movem.l d0-a1,-(sp)",
        "bsr.w GetCurrentMapSetup",
        "cmpi.w #-1,(a0)",
        "bne.s loc_4750E",
        "bra.w loc_47514",
        "movea.l MAPSETUP_OFFSET_INIT_FUNCTION(a0),a0",
        "jsr (a0)",
        "movem.l (sp)+,d0-a1",
        "rts",
    )
    if tuple(wrapper_instructions) != required_wrapper:
        raise ValueError("story-state map-setup wrapper H1 control-flow drift")
    call_site = next(address for address, instruction in wrapper_rows if instruction == "jsr (a0)")
    call_index = [address for address, _ in wrapper_rows].index(call_site)
    bypass_address = wrapper_rows[call_index + 1][0]
    return_instruction_address = wrapper_rows[call_index + 2][0]
    bypass_match = re.fullmatch(
        r"bra\.w loc_(?P<address>[0-9A-F]{5})", wrapper_instructions[4]
    )
    if (
        bypass_match is None
        or int(bypass_match["address"], 16) != bypass_address
        or wrapper_rows[call_index + 1][1] != "movem.l (sp)+,d0-a1"
        or wrapper_rows[call_index + 2][1] != "rts"
    ):
        raise ValueError("story-state map-setup wrapper bypass/return H1 drift")
    wait_source = (upstream / VINT_SOURCE_PATH).read_text(encoding="utf-8")
    wait_source_rows = _source_section(wait_source, "WaitForVInt")
    wait_rows = _h1_function_lines(listing, "WaitForVInt")
    wait_instructions = (
        "bset #ENABLE_VINT,(VINT_PARAMETERS).l",
        "move.b #1,((WAITING_NEXT_VINT-$1000000)).w",
        "tst.b ((WAITING_NEXT_VINT-$1000000)).w",
        "bne.s @Wait",
        "rts",
    )
    if (
        tuple(row["instruction"] for row in wait_source_rows) != wait_instructions
        or tuple(instruction for _, instruction in wait_rows) != wait_instructions
    ):
        raise ValueError("story-state WaitForVInt loop identity drift")
    wait_loop_address = next(
        address for address, instruction in wait_rows if instruction == "bne.s @Wait"
    )
    return {
        "entryAddress": addresses["RunMapSetupInitFunction"],
        "wrapper": {
            "outerCallSiteAddress": call_site,
            "bypassAddress": bypass_address,
            "returnInstructionAddress": return_instruction_address,
            "indirectCallInstruction": "jsr (a0)",
        },
        "waitForVInt": {
            "entryAddress": addresses["WaitForVInt"],
            "waitLoopBranchAddress": wait_loop_address,
            "waitLoopBranchInstruction": "bne.s @Wait",
        },
        "scratchGap": static["scratchGap"],
        "handlerRecords": handler_records,
        "effectiveServiceAddresses": {
            name: addresses[name]
            for name in ("CheckFlag", "SetFlag", "ClearFlag", "Sleep", "YesNoPrompt")
        },
        "yesNoJumpInterfaceAddress": addresses["j_YesNoPrompt"],
        "persistence": {
            "saveGameAddress": addresses["SaveGame"],
            "loadGameAddress": addresses["LoadGame"],
            "ramLogicalSpan": static["saveLoadPersistence"]["ramLogicalSpan"],
            "saveFlagsAddress": static["saveLoadPersistence"]["saveFlagsAddress"],
            "physicalWindowBaseAddress": static["saveLoadPersistence"]["physicalWindowBaseAddress"],
        },
    }


def _instrument_story_state_rom(rom_path: Path, fixture: dict[str, Any]) -> Path:
    """Create a session-only trampoline plus a RAM-controlled YesNoPrompt return seam."""
    original_hash = inspect_rom(rom_path.resolve(strict=True))["sha256"]
    data = bytearray(rom_path.read_bytes())
    patch = fixture["instrumentation"]
    trampoline = patch["trampoline"]
    prompt = patch["yesNoPromptStub"]
    call_site = trampoline["callSiteAddress"]
    original_call = bytes.fromhex(trampoline["callSiteOriginalHex"])
    patched_call = bytes.fromhex(trampoline["callSitePatchedHex"])
    stub_address = trampoline["stubAddress"]
    original_stub = bytes.fromhex(trampoline["stubOriginalHex"])
    stub = bytes.fromhex(trampoline["stubHex"])
    if data[call_site : call_site + len(original_call)] != original_call:
        raise ValueError("story-state trampoline call-site original bytes drift")
    if data[stub_address : stub_address + len(original_stub)] != original_stub:
        raise ValueError("story-state trampoline original padding bytes drift")
    if patched_call != b"\x4e\xb9" + stub_address.to_bytes(4, "big"):
        raise ValueError("story-state trampoline call shape drift")
    if len(patched_call) != len(original_call):
        raise ValueError("story-state trampoline patched outer-call width drift")
    if len(stub) > len(original_stub):
        raise ValueError("story-state trampoline exceeds verified padding")
    prompt_address = prompt["effectiveEntryAddress"]
    prompt_original = bytes.fromhex(prompt["originalHex"])
    prompt_stub = bytes.fromhex(prompt["stubHex"])
    if data[prompt_address : prompt_address + len(prompt_original)] != prompt_original:
        raise ValueError("story-state YesNoPrompt original bytes drift")
    if len(prompt_stub) != len(prompt_original):
        raise ValueError("story-state YesNoPrompt stub boundary drift")
    data[call_site : call_site + len(patched_call)] = patched_call
    data[stub_address : stub_address + len(stub)] = stub
    data[prompt_address : prompt_address + len(prompt_stub)] = prompt_stub
    data[0x18E:0x190] = int(mega_drive_checksum(bytes(data)), 16).to_bytes(2, "big")
    if inspect_rom(rom_path.resolve(strict=True))["sha256"] != original_hash:
        raise ValueError("story-state instrumentation altered the original ROM")
    DERIVED_ROOT.mkdir(parents=True, exist_ok=True)
    output = DERIVED_ROOT / "story-state.instrumented.bin"
    output.write_bytes(data)
    return output


def _trampoline_return_program_counter(instrumentation: dict[str, Any]) -> int:
    """Derive the RTS landing PC from the source-pinned trampoline body."""
    trampoline = instrumentation["trampoline"]
    stub_address = trampoline["stubAddress"]
    stub = bytes.fromhex(trampoline["stubHex"])
    if len(stub) < 2 or stub[-2:] != b"\x4e\x75":
        raise ValueError("story-state trampoline return instruction drift")
    return stub_address + len(stub) - 2


def _trampoline_route(
    instrumentation: dict[str, Any], runtime: dict[str, Any], scratch_layout: dict[str, Any]
) -> dict[str, int]:
    """Decode the session-only trampoline so callback chronology is not address folklore."""
    trampoline = instrumentation["trampoline"]
    stub_address = trampoline["stubAddress"]
    pointer = scratch_layout["pointerScratch"]["address"]
    stub = bytes.fromhex(trampoline["stubHex"])
    expected = (
        b"\x70\x00"
        + b"\x4d\xf9"
        + (pointer + TRAMPOLINE_POINTER_BYTE_COUNT).to_bytes(4, "big")
        + b"\x20\x79"
        + pointer.to_bytes(4, "big")
        + b"\x4e\x90\x4e\x75"
    )
    if stub != expected:
        raise ValueError("story-state trampoline instruction/operand drift")
    outer_call_site = runtime["wrapper"]["outerCallSiteAddress"]
    patched_outer_call = bytes.fromhex(trampoline["callSitePatchedHex"])
    original_outer_call = bytes.fromhex(trampoline["callSiteOriginalHex"])
    if trampoline["callSiteAddress"] != outer_call_site:
        raise ValueError("story-state trampoline wrapper call-site drift")
    if original_outer_call[:2] != b"\x4e\x90" or len(original_outer_call) != len(
        patched_outer_call
    ):
        raise ValueError("story-state trampoline original outer-call width drift")
    if patched_outer_call != b"\x4e\xb9" + stub_address.to_bytes(4, "big"):
        raise ValueError("story-state trampoline patched outer-call shape drift")
    outer_return = outer_call_site + len(patched_outer_call)
    if outer_return != runtime["wrapper"]["returnInstructionAddress"]:
        raise ValueError("story-state trampoline outer-return H1/instrumentation drift")
    inner_call = stub_address + len(stub) - 4
    inner_return = stub_address + len(stub) - 2
    return {
        "wrapperEntryAddress": runtime["entryAddress"],
        "bypassAddress": runtime["wrapper"]["bypassAddress"],
        "outerCallSiteAddress": outer_call_site,
        "outerTargetAddress": stub_address,
        "outerReturnAddress": outer_return,
        "trampolineEntryAddress": stub_address,
        "innerCallSiteAddress": inner_call,
        "innerTargetAddress": scratch_layout["ranges"][0]["address"],
        "innerReturnAddress": inner_return,
        "probeEntryAddress": scratch_layout["ranges"][0]["address"],
    }


def _case_stream(case: dict[str, Any]) -> list[int]:
    expected = case["expected"]
    handler = expected["handler"]
    if handler.startswith("csc0"):
        flag = expected["flagStorage"]["flagIndexInputWord"]
        target = expected["targetInputValue"]
        if not isinstance(target, int) or not 0 <= target <= 0xFFFFFFFF:
            raise ValueError("story-state jump target input is not an unsigned longword")
        return [flag >> 8, flag & 0xFF, *target.to_bytes(4, "big")]
    if handler == "csc10_toggleFlag":
        return [
            expected["flagIndexInput"] >> 8,
            expected["flagIndexInput"] & 0xFF,
            expected["selectorInput"] >> 8,
            expected["selectorInput"] & 0xFF,
        ]
    if handler == "csc13_setStoryFlag":
        return [expected["battleInputWord"] >> 8, expected["battleInputWord"] & 0xFF]
    if handler == "csc11_promptYesNoForStoryFlow":
        return []
    raise ValueError("story-state runtime case handler drift")


def _runtime_config(
    fixture: dict[str, Any],
    static: dict[str, Any],
    runtime: dict[str, Any],
    derived: list[dict[str, Any]],
) -> dict[str, Any]:
    """Build observer-only values from parsed storage and fixed instrumentation."""
    addressing = static["flagStorage"]["addressing"]
    base_address = static["flagStorage"]["GAME_FLAGS"]["value"]
    byte_span = addressing["addressableByteSpan"]
    if not isinstance(base_address, int) or not isinstance(byte_span, int) or byte_span <= 0:
        raise ValueError("story-state parsed GAME_FLAGS range drift")
    scratch_layout = _generated_scratch_layout(fixture["instrumentation"], runtime["scratchGap"])
    route = _trampoline_route(fixture["instrumentation"], runtime, scratch_layout)
    persistence_stream_address = next(
        row["address"] for row in scratch_layout["ranges"] if row["name"] == "mutationStream"
    )
    case_inputs = []
    for case in derived[:V1_CASE_COUNT]:
        expected = case["expected"]
        prompt_result_word = 0
        if expected["handler"] == "csc11_promptYesNoForStoryFlow":
            prompt_result_word = 0 if expected["promptResultZero"] else 1
        case_inputs.append(
            {
                "id": case["id"],
                "handlerAddress": expected["h1Address"],
                "streamBytes": _case_stream(case),
                "streamAddress": scratch_layout["retainedV1Stream"]["address"],
                "promptResultWord": prompt_result_word,
                "kind": "v1",
            }
        )
    for case in derived[V1_CASE_COUNT:]:
        expected = case["expected"]
        mutation = expected["mutation"]
        prompt_result_word = 0
        if mutation["handler"] == "csc11_promptYesNoForStoryFlow":
            prompt_result_word = 0 if mutation["promptResultZero"] else 1
        case_inputs.append(
            {
                "id": case["id"],
                "kind": "persistence",
                "handlerAddress": mutation["h1Address"],
                "streamBytes": _case_stream({"expected": mutation}),
                "streamAddress": persistence_stream_address,
                "promptResultWord": prompt_result_word,
                "finalHandlerAddress": expected["finalCheck"]["h1Address"],
                "finalFlagIndexWord": expected["finalCheck"]["flagStorage"]["flagIndexInputWord"],
            }
        )
    if any(len(row["streamBytes"]) > GENERATED_MUTATION_STREAM_BYTE_COUNT for row in case_inputs):
        raise ValueError("story-state generated mutation-stream width drift")
    for row in case_inputs:
        expected_address = (
            scratch_layout["retainedV1Stream"]["address"]
            if row["kind"] == "v1"
            else persistence_stream_address
        )
        if row["streamAddress"] != expected_address:
            raise ValueError("story-state case stream-address derivation drift")
    return {
        "fixtureId": fixture["id"],
        "jsonModulePath": OBSERVER.with_name("json.lua").as_posix(),
        "mapTest": fixture["mapTestIndex"],
        "runtimeContract": runtime,
        "wrapperRoute": route,
        "scratchLayout": scratch_layout,
        "gameFlags": {"baseAddress": base_address, "byteSpan": byte_span},
        "returnProgramCounter": _trampoline_return_program_counter(fixture["instrumentation"]),
        "instrumentation": fixture["instrumentation"],
        "cases": derived,
        "caseInputs": case_inputs,
        "maxFrames": fixture["maxFrames"],
        "harness": load_json(repo_path(fixture["sharedHarnessFixture"]))["harness"],
        "observerFailureContract": OBSERVER_FAILURE_CONTRACT,
    }


def _canonical_sha256(value: object) -> str:
    return hashlib.sha256(
        json.dumps(value, sort_keys=True, separators=(",", ":")).encode("utf-8")
    ).hexdigest().upper()


def _retained_v1_projection(fixture: dict[str, Any]) -> dict[str, Any]:
    observation = fixture.get("observation")
    if not isinstance(observation, dict):
        raise ValueError("story-state retained-v1 observation is missing")
    return {
        "cases": fixture.get("cases", [])[:V1_CASE_COUNT],
        "records": observation.get("records", [])[:V1_CASE_COUNT],
    }


def _assert_retained_v1_projection(fixture: dict[str, Any]) -> None:
    """Reject v1 drift before configuration and again at the final golden boundary."""
    retained = _retained_v1_projection(fixture)
    if (
        fixture.get("retainedV1ProjectionSha256") != RETAINED_V1_PROJECTION_SHA256
        or _canonical_sha256(retained) != RETAINED_V1_PROJECTION_SHA256
    ):
        raise ValueError("story-state retained-v1 projection drift")


def _h1_first_instruction_bytes(listing: str, symbol: str) -> bytes:
    start = re.search(rf"^[0-9A-F]{{8}}\s+{re.escape(symbol)}:\s*$", listing, re.MULTILINE)
    if start is None:
        raise ValueError(f"story-state H1 ROM guard missing symbol: {symbol}")
    match = re.search(
        r"^[0-9A-F]{8}\s+((?:[0-9A-F]{4}\s+)+)", listing[start.end() :], re.MULTILINE
    )
    if match is None:
        raise ValueError(f"story-state H1 ROM guard missing first instruction: {symbol}")
    return bytes.fromhex(re.sub(r"\s+", "", match.group(1)))


def _validate_story_state_h1_rom(static: dict[str, Any], rom_path: Path, upstream: Path) -> None:
    """Tie every observed handler and save/load target to the pinned H1/ROM seam."""
    listing = (upstream / H1_LISTING_PATH).read_text(encoding="utf-8")
    rom = rom_path.resolve(strict=True).read_bytes()
    symbols = [row["handler"] for row in static["handlers"]] + ["SaveGame", "LoadGame"]
    for symbol in symbols:
        address = next(
            (
                row["h1Address"]
                for row in static["handlers"]
                if row["handler"] == symbol
            ),
            static["saveLoadPersistence"]["saveLoadFunctions"].get(symbol, {}).get("h1Address"),
        )
        opcode = _h1_first_instruction_bytes(listing, symbol)
        if not isinstance(address, int) or rom[address : address + len(opcode)] != opcode:
            raise ValueError(f"story-state H1/ROM first-instruction guard drift: {symbol}")


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
        required_milestones=("milestone:story-state-probe",),
    )


def _runtime_call(
    runtime: dict[str, Any], *, handler: str, instruction_target: str
) -> dict[str, Any]:
    matches = [
        call
        for record in runtime["handlerRecords"]
        if record["handler"] == handler
        for call in record["directCalls"]
        if call["instructionTarget"] == instruction_target
    ]
    if len(matches) != 1:
        raise ValueError("story-state persistence runtime call derivation drift")
    return matches[0]


def _pattern_byte(seed: int, logical_offset: int) -> int:
    return (seed + 17 * logical_offset + 29 * (logical_offset // 8)) & 0xFF


def _persistence_observation_record(
    case: dict[str, Any], runtime: dict[str, Any], instrumentation: dict[str, Any]
) -> dict[str, Any]:
    """Predict one compact runtime record from the generated work-RAM probe and source seams."""
    expected = case["expected"]
    mutation = expected["mutation"]
    final = expected["finalCheck"]
    selected = expected["selectedSlot"]
    probe = instrumentation["persistenceProbe"]
    program = probe["programAddress"]
    mutation_call = _runtime_call(
        runtime,
        handler=mutation["handler"],
        instruction_target=mutation["expectedInstructionTarget"],
    )
    final_call = _runtime_call(
        runtime,
        handler=final["handler"],
        instruction_target=final["instructionTarget"],
    )
    state = expected["stateBytes"]
    original = _pattern_byte(expected["ramPatternSeed"], selected["gameFlagsLogicalOffset"])
    checksum = (
        sum(
            _pattern_byte(expected["ramPatternSeed"], offset)
            for offset in range(runtime["persistence"]["ramLogicalSpan"]["logicalByteCount"])
        )
        - original
        + state["mutated"]
    ) & 0xFF
    flag_bit = 1 << selected["occupiedFlagBit"]

    def row(
        role: str,
        pc: int,
        call_pc: int,
        target_pc: int,
        return_pc: int,
        ram_byte: int,
        selected_sram_byte: int,
        save_flags: int,
    ) -> dict[str, Any]:
        return {
            "role": role,
            "pc": pc,
            "callPc": call_pc,
            "targetPc": target_pc,
            "returnPc": return_pc,
            "ramByte": ram_byte,
            "selectedSramByte": selected_sram_byte,
            "saveFlags": save_flags,
        }

    chronology = [
        row(
            "mutation-handler-entry",
            mutation["h1Address"],
            program + 6,
            mutation["h1Address"],
            program + 12,
            state["before"],
            0,
            0,
        ),
        row(
            "mutation-call",
            mutation_call["h1Address"],
            mutation_call["h1Address"],
            runtime["effectiveServiceAddresses"][mutation["expectedEffectiveTarget"]],
            mutation_call["returnAddress"],
            state["before"],
            0,
            0,
        ),
        row(
            "mutation-return",
            program + 12,
            program + 6,
            mutation["h1Address"],
            program + 12,
            state["mutated"],
            0,
            0,
        ),
        row(
            "save-entry",
            runtime["persistence"]["saveGameAddress"],
            program + 14,
            runtime["persistence"]["saveGameAddress"],
            program + 20,
            state["mutated"],
            0,
            0,
        ),
        row(
            "save-return-poison",
            program + 20,
            program + 14,
            runtime["persistence"]["saveGameAddress"],
            program + 20,
            state["poisoned"],
            state["mutated"],
            flag_bit,
        ),
        row(
            "load-entry",
            runtime["persistence"]["loadGameAddress"],
            program + 22,
            runtime["persistence"]["loadGameAddress"],
            program + 28,
            state["poisoned"],
            state["mutated"],
            flag_bit,
        ),
        row(
            "load-return",
            program + 28,
            program + 22,
            runtime["persistence"]["loadGameAddress"],
            program + 28,
            state["restored"],
            state["mutated"],
            flag_bit,
        ),
        row(
            "final-handler-entry",
            final["h1Address"],
            program + 34,
            final["h1Address"],
            program + 40,
            state["restored"],
            state["mutated"],
            flag_bit,
        ),
        row(
            "final-check-call",
            final_call["h1Address"],
            final_call["h1Address"],
            runtime["effectiveServiceAddresses"][final["effectiveTarget"]],
            final_call["returnAddress"],
            state["restored"],
            state["mutated"],
            flag_bit,
        ),
        row(
            "final-branch-result",
            program + 40,
            program + 34,
            final["h1Address"],
            program + 40,
            state["restored"],
            state["mutated"],
            flag_bit,
        ),
    ]
    return {
        "id": case["id"],
        "mutationHandlerAddress": mutation["h1Address"],
        "finalHandlerAddress": final["h1Address"],
        "selector": expected["selector"],
        "ramLogicalSpan": runtime["persistence"]["ramLogicalSpan"],
        "trackedByte": {
            "ramAddress": mutation["flagStorage"]["flagStorageAddress"],
            "logicalOffset": selected["gameFlagsLogicalOffset"],
            "selectedPhysicalAddress": selected["selectedFlagPhysicalAddress"],
            "before": state["before"],
            "mutated": state["mutated"],
            "poisoned": state["poisoned"],
            "restored": state["restored"],
            "saved": state["mutated"],
        },
        "storedChecksumByte": checksum,
        "saveFlags": flag_bit,
        "finalA6Output": program,
        "chronology": chronology,
    }


def expected_story_state_observation(
    fixture: dict[str, Any], runtime: dict[str, Any], derived: list[dict[str, Any]]
) -> dict[str, Any]:
    """Build the full exact golden from retained v1 rows and six source-derived persistence rows."""
    retained = fixture["observation"]["records"][:V1_CASE_COUNT]
    persistence = [
        _persistence_observation_record(case, runtime, fixture["instrumentation"])
        for case in derived[V1_CASE_COUNT:]
    ]
    span = runtime["persistence"]["ramLogicalSpan"]
    scratch = _generated_scratch_layout(fixture["instrumentation"], runtime["scratchGap"])
    return {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["id"],
        "mapTest": fixture["mapTestIndex"],
        "recordOrder": [case["id"] for case in derived],
        "records": [*retained, *persistence],
        "callbacksCleared": 0,
        "scopedSramRestored": True,
        "restoration": {
            "logicalRam": {
                "baseAddress": span["baseAddress"],
                "logicalByteCount": span["logicalByteCount"],
                "restored": True,
            },
            "sram": {
                "logicalByteCountPerSlot": span["logicalByteCount"],
                "slotCount": 2,
                "checksumRestored": True,
                "saveFlagsRestored": True,
                "restored": True,
            },
            "generatedScratch": {
                "ranges": scratch["ranges"],
                "restored": True,
            },
            "pointerScratch": {**scratch["pointerScratch"], "restored": True},
            "retainedV1Stream": {**scratch["retainedV1Stream"], "restored": True},
            "promptResultWord": {
                "address": fixture["instrumentation"]["yesNoPromptStub"]["resultRamAddress"],
                "byteCount": 2,
                "restored": True,
            },
            "callStack": {"observedBalanced": True},
        },
    }


def validate_story_state_fixture_semantics(
    fixture: dict[str, Any], static: dict[str, Any], runtime: dict[str, Any]
) -> tuple[list[dict[str, Any]], dict[str, Any]]:
    """Keep the exact corpus, order, and golden in the fixture verifier, not its schema."""
    if (
        fixture["staticContractSha256"] != _canonical_sha256(static)
        or fixture["runtimeContract"] != runtime
        or fixture["function"]["runMapSetupInitFunctionAddress"] != runtime["entryAddress"]
    ):
        raise ValueError("story-state fixture/static runtime contract drift")
    scratch_layout = _generated_scratch_layout(
        fixture["instrumentation"], runtime["scratchGap"]
    )
    v1 = derive_story_state_case_matrix(
        static,
        input_cursor_offset_bytes=scratch_layout["retainedV1Stream"]["address"],
    )
    persistence = derive_story_state_persistence_case_matrix(static)
    if fixture["cases"][:V1_CASE_COUNT] != v1 or fixture["cases"][V1_CASE_COUNT:] != (
        persistence_fixture_projection(persistence)
    ):
        raise ValueError("story-state fixture semantic case matrix drift")
    derived = [*v1, *persistence]
    expected = expected_story_state_observation(fixture, runtime, derived)
    if fixture["observation"] != expected:
        raise ValueError("story-state fixture observation semantic drift")
    _assert_retained_v1_projection(fixture)
    return derived, expected


def verify_story_state(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 180
) -> dict[str, Any]:
    """Run the retained ten cases plus six original-function save/load probes in one launch."""
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner="story-state fixture")
    _assert_retained_v1_projection(fixture)
    verify_runtime_contract(fixture, rom_path)
    upstream = upstream_path.resolve(strict=True)
    static = build_story_state_static_contract(upstream)
    _validate_story_state_h1_rom(static, rom_path, upstream)
    runtime = _runtime_contract(static, upstream)
    derived, expected = validate_story_state_fixture_semantics(fixture, static, runtime)
    observer_config = _runtime_config(fixture, static, runtime, derived)
    instrumented_rom = _instrument_story_state_rom(rom_path, fixture)

    def observe() -> dict[str, Any]:
        return run_observer(
            rom_path=instrumented_rom,
            observer_path=OBSERVER,
            config=observer_config,
            output_name="story-state",
            timeout_seconds=timeout_seconds,
        )

    try:
        observed = _with_instrumented_rom_database(instrumented_rom, "SF2 H3 story state", observe)
    except RuntimeError as error:
        diagnostic = _status_diagnostic()
        if diagnostic is not None:
            raise RuntimeError(f"{OWNER} observer callback failure: {diagnostic}") from error
        raise
    finally:
        instrumented_rom.unlink(missing_ok=True)
    _assert_status()
    validate_json(observed, OBSERVATION_SCHEMA, owner="story-state observation")
    if observed != expected:
        raise ValueError(
            "story-state runtime observation mismatch:\n"
            f"expected={expected!r}\nobserved={observed!r}"
        )
    return {
        "Fixture": fixture["id"],
        "Cases": len(derived),
        "Handlers": len(runtime["handlerRecords"]),
        "BizHawkLaunches": 1,
        "Instrumentation": "session-only",
        "Status": "PASS",
    }
