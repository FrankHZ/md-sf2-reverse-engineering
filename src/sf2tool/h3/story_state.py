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
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.rom import inspect_rom, mega_drive_checksum

H2_FIXTURE = repo_path("tests/fixtures/h2/map-script-engine-static-v1.json")
FIXTURE = repo_path("tests/fixtures/h3/story-state-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h3-story-state-fixture.schema.json")
OBSERVATION_SCHEMA = repo_path("schemas/h3-story-state-observation.schema.json")
OBSERVER = repo_path("tools/bizhawk/story_state_observer.lua")
TOOLCHAIN_MANIFEST = repo_path("manifests/toolchain.json")
H1_LISTING_PATH = Path("build/sf2build-h1.lst")
MAP_SCRIPT_SOURCE_PATH = Path("disasm/code/common/scripting/map/mapscriptengine_2.asm")
ENUM_SOURCE_PATH = Path("disasm/sf2enums.asm")
CONSTANT_SOURCE_PATH = Path("disasm/sf2const.asm")
GAME_FLAGS_SOURCE_PATH = Path("disasm/code/common/stats/gameflags.asm")
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
    enum_source: str, constant_source: str, game_flags_source: str
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
        ),
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
                    "instructionTarget": call["instructionTarget"],
                    "effectiveTarget": effective,
                }
            )
        handler_records.append(
            {
                "handler": handler["handler"],
                "h1Address": handler["h1Address"],
                "cursorUseSites": cursor_sites,
                "directCalls": calls,
            }
        )
    return {
        "entryAddress": addresses["RunMapSetupInitFunction"],
        "handlerRecords": handler_records,
        "effectiveServiceAddresses": {
            name: addresses[name]
            for name in ("CheckFlag", "SetFlag", "ClearFlag", "Sleep", "YesNoPrompt")
        },
        "yesNoJumpInterfaceAddress": addresses["j_YesNoPrompt"],
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
    case_inputs = []
    for case in derived:
        expected = case["expected"]
        prompt_result_word = 0
        if expected["handler"] == "csc11_promptYesNoForStoryFlow":
            prompt_result_word = 0 if expected["promptResultZero"] else 1
        case_inputs.append(
            {
                "id": case["id"],
                "handlerAddress": expected["h1Address"],
                "streamBytes": _case_stream(case),
                "promptResultWord": prompt_result_word,
            }
        )
    return {
        "fixtureId": fixture["id"],
        "jsonModulePath": OBSERVER.with_name("json.lua").as_posix(),
        "mapTest": fixture["mapTestIndex"],
        "runtimeContract": runtime,
        "gameFlags": {"baseAddress": base_address, "byteSpan": byte_span},
        "returnProgramCounter": _trampoline_return_program_counter(fixture["instrumentation"]),
        "instrumentation": fixture["instrumentation"],
        "cases": derived,
        "caseInputs": case_inputs,
        "maxFrames": fixture["maxFrames"],
        "harness": load_json(repo_path(fixture["sharedHarnessFixture"]))["harness"],
    }


def verify_story_state(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 180
) -> dict[str, Any]:
    """Run the bounded ten-case story-state matrix in one session-only Map Test 0 launch."""
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner="story-state fixture")
    verify_runtime_contract(fixture, rom_path)
    upstream = upstream_path.resolve(strict=True)
    static = build_story_state_static_contract(upstream)
    runtime = _runtime_contract(static, upstream)
    static_digest = (
        hashlib.sha256(json.dumps(static, sort_keys=True, separators=(",", ":")).encode("utf-8"))
        .hexdigest()
        .upper()
    )
    if (
        fixture["staticContractSha256"] != static_digest
        or fixture["runtimeContract"] != runtime
        or fixture["function"]["runMapSetupInitFunctionAddress"] != runtime["entryAddress"]
    ):
        raise ValueError("story-state fixture/static runtime contract drift")
    derived = derive_story_state_case_matrix(
        static,
        input_cursor_offset_bytes=fixture["instrumentation"]["trampoline"]["ramInputAddress"] + 4,
    )
    if fixture["cases"] != derived:
        raise ValueError("story-state fixture semantic case matrix drift")
    instrumented_rom = _instrument_story_state_rom(rom_path, fixture)

    observer_config = _runtime_config(fixture, static, runtime, derived)

    def observe() -> dict[str, Any]:
        return run_observer(
            rom_path=instrumented_rom,
            observer_path=OBSERVER,
            config=observer_config,
            output_name="story-state",
            timeout_seconds=timeout_seconds,
        )

    observed = _with_instrumented_rom_database(instrumented_rom, "SF2 H3 story state", observe)
    validate_json(observed, OBSERVATION_SCHEMA, owner="story-state observation")
    expected = fixture["observation"]
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
