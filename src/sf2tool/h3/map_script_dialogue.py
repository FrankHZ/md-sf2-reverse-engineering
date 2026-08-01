"""One-launch handler-local runtime contract for map-script dialogue commands.

The original text, portrait, audio, and timing services remain outside this
bounded rail.  The session ROM only replaces their entry points after the
caller instruction and target-entry PCs are observed.  This preserves the
dialogue handlers' own branches, cursor movement, state writes, and call
chronology without treating a shim as evidence about presentation.
"""

from __future__ import annotations

import hashlib
import re
from pathlib import Path
from typing import Any

from sf2tool.h2.map_script_engine import _canonical_bytes, build_map_script_engine_contract
from sf2tool.h3.bizhawk import DERIVED_ROOT, run_observer, verify_runtime_contract
from sf2tool.h3.map_lifecycle import _instrument_rom, _with_instrumented_rom_database
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.rom import mega_drive_checksum

H2_FIXTURE = repo_path("tests/fixtures/h2/map-script-engine-static-v1.json")
FIXTURE = repo_path("tests/fixtures/h3/map-script-dialogue-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h3-map-script-dialogue-fixture.schema.json")
OBSERVATION_SCHEMA = repo_path("schemas/h3-map-script-dialogue-observation.schema.json")
OBSERVER = repo_path("tools/bizhawk/map_script_dialogue_observer.lua")
H1_LISTING_PATH = Path("build/sf2build-h1.lst")
CONSTANTS_PATH = Path("disasm/sf2const.asm")
ENUMS_PATH = Path("disasm/sf2enums.asm")

HANDLER_FORMS = (
    ("nextSingleText", "csc00_displaySingleTextbox"),
    ("nextSingleTextVar", "csc01_displaySingleTextboxWithVars"),
    ("nextText", "csc02_displayTextbox"),
    ("nextTextVar", "csc03_displayTextboxWithVars"),
    ("textCursor", "csc04_setTextIndex"),
    ("hideText", "csc09_hideDialogueAndPortraitWindows"),
)
DISPLAY_MACROS = tuple(name for name, _ in HANDLER_FORMS[:4])
SKIP_MACROS = ("nextSingleText", "nextText")
TARGET_RESOLUTIONS = {
    "csc1D_showPortrait": ("csc1D_showPortrait", "internal"),
    "GetEntityPortaitAndSpeechSfx": ("GetEntityPortaitAndSpeechSfx", "external"),
    "WaitForViewScrollEnd": ("WaitForViewScrollEnd", "external"),
    "DisplayText": ("DisplayText", "external"),
    "j_ClosePortraitWindow": ("ClosePortraitWindow", "external"),
    "Sleep": ("Sleep", "external"),
}
EFFECTIVE_TARGETS = tuple(dict.fromkeys(value[0] for value in TARGET_RESOLUTIONS.values()))
RUNTIME_QUESTIONS = [
    "map-script-dialogue/normal-story-reachability",
    "map-script-dialogue/rendered-portrait-speech-and-controller-timing",
    "map-script-dialogue/service-body-effects-and-persistence",
]

# Each sequence is a smallest named-handler guard.  Labels and comments are
# deliberately removed, but branch polarity, operand reads, mutation order,
# calls, and return remain source facts rather than file-wide fragments.
CONTROL_SECTIONS = {
    "csc00_displaySingleTextbox": (
        "tst.b ((SKIP_CUTSCENE_TEXT-$1000000)).w",
        "bne.s loc_47298",
        "cmpi.w #-1,(a6)",
        "beq.s loc_4726A",
        "move.l a6,-(sp)",
        "bsr.w csc1D_showPortrait",
        "movea.l (sp)+,a6",
        "move.w (a6),d0",
        "bsr.w GetEntityPortaitAndSpeechSfx",
        "move.w d2,((CURRENT_SPEECH_SFX-$1000000)).w",
        "bra.s loc_47270",
        "move.w #0,((CURRENT_SPEECH_SFX-$1000000)).w",
        "adda.w #2,a6",
        "move.w ((CUTSCENE_DIALOG_INDEX-$1000000)).w,d0",
        "jsr (WaitForViewScrollEnd).w",
        "jsr (DisplayText).l",
        "addq.w #1,((CUTSCENE_DIALOG_INDEX-$1000000)).w",
        "jsr j_ClosePortraitWindow",
        "clsTxt",
        "moveq #10,d0",
        "jsr (Sleep).w",
        "bra.s return_4729C",
        "adda.w #2,a6",
        "rts",
    ),
    "csc01_displaySingleTextboxWithVars": (
        "cmpi.w #-1,(a6)",
        "beq.s loc_472B8",
        "move.l a6,-(sp)",
        "bsr.w csc1D_showPortrait",
        "movea.l (sp)+,a6",
        "move.w (a6),d0",
        "bsr.w GetEntityPortaitAndSpeechSfx",
        "move.w d2,((CURRENT_SPEECH_SFX-$1000000)).w",
        "bra.s loc_472BE",
        "move.w #0,((CURRENT_SPEECH_SFX-$1000000)).w",
        "adda.w #2,a6",
        "move.w (a6)+,((DIALOGUE_NAME_INDEX_1-$1000000)).w",
        "move.w (a6)+,((DIALOGUE_NAME_INDEX_2-$1000000)).w",
        "move.w ((CUTSCENE_DIALOG_INDEX-$1000000)).w,d0",
        "jsr (WaitForViewScrollEnd).w",
        "jsr (DisplayText).l",
        "addq.w #1,((CUTSCENE_DIALOG_INDEX-$1000000)).w",
        "jsr j_ClosePortraitWindow",
        "clsTxt",
        "moveq #10,d0",
        "jsr (Sleep).w",
        "rts",
    ),
    "csc02_displayTextbox": (
        "tst.b ((SKIP_CUTSCENE_TEXT-$1000000)).w",
        "bne.s loc_4732C",
        "cmpi.w #-1,(a6)",
        "beq.s loc_4730E",
        "move.l a6,-(sp)",
        "bsr.w csc1D_showPortrait",
        "movea.l (sp)+,a6",
        "move.w (a6),d0",
        "bsr.w GetEntityPortaitAndSpeechSfx",
        "move.w d2,((CURRENT_SPEECH_SFX-$1000000)).w",
        "bra.s loc_47314",
        "move.w #0,((CURRENT_SPEECH_SFX-$1000000)).w",
        "adda.w #2,a6",
        "move.w ((CUTSCENE_DIALOG_INDEX-$1000000)).w,d0",
        "jsr (WaitForViewScrollEnd).w",
        "jsr (DisplayText).l",
        "addq.w #1,((CUTSCENE_DIALOG_INDEX-$1000000)).w",
        "bra.s return_47330",
        "adda.w #2,a6",
        "rts",
    ),
    "csc03_displayTextboxWithVars": (
        "cmpi.w #-1,(a6)",
        "beq.s loc_4734C",
        "move.l a6,-(sp)",
        "bsr.w csc1D_showPortrait",
        "movea.l (sp)+,a6",
        "move.w (a6),d0",
        "bsr.w GetEntityPortaitAndSpeechSfx",
        "move.w d2,((CURRENT_SPEECH_SFX-$1000000)).w",
        "bra.s loc_47352",
        "move.w #0,((CURRENT_SPEECH_SFX-$1000000)).w",
        "adda.w #2,a6",
        "move.w (a6)+,((DIALOGUE_NAME_INDEX_1-$1000000)).w",
        "move.w (a6)+,((DIALOGUE_NAME_INDEX_2-$1000000)).w",
        "move.w ((CUTSCENE_DIALOG_INDEX-$1000000)).w,d0",
        "jsr (WaitForViewScrollEnd).w",
        "jsr (DisplayText).l",
        "addq.w #1,((CUTSCENE_DIALOG_INDEX-$1000000)).w",
        "rts",
    ),
    "csc04_setTextIndex": ("move.w (a6)+,((CUTSCENE_DIALOG_INDEX-$1000000)).w", "rts"),
    "csc09_hideDialogueAndPortraitWindows": ("jsr j_ClosePortraitWindow", "clsTxt", "rts"),
}


def _literal(text: str) -> int:
    if re.fullmatch(r"\$[0-9A-Fa-f]+", text):
        return int(text[1:], 16)
    if re.fullmatch(r"-?\d+", text):
        return int(text)
    if re.fullmatch(r"%[01]+", text):
        return int(text[1:], 2)
    raise ValueError(f"dialogue literal is not numeric: {text}")


def _closed_rows(value: object, required: set[str], *, name: str) -> list[dict[str, Any]]:
    if not isinstance(value, list) or not all(isinstance(row, dict) for row in value):
        raise ValueError(f"dialogue {name} container drift")
    if any(set(row) != required for row in value):
        raise ValueError(f"dialogue {name} record shape drift")
    return list(value)


def _source_section(source: str, symbol: str) -> list[str]:
    start = re.search(rf"^{re.escape(symbol)}:\s*$", source, re.MULTILINE)
    if start is None:
        raise ValueError(f"dialogue source section is missing: {symbol}")
    end = source.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"dialogue source section end is missing: {symbol}")
    rows = []
    for raw in source[start.start() : end].splitlines():
        statement = re.sub(r"\s+", " ", raw.split(";", 1)[0].strip())
        if statement and not statement.endswith(":"):
            rows.append(statement)
    return rows


def _h1_rows(listing: str, symbol: str) -> list[dict[str, Any]]:
    start = re.search(rf"^[0-9A-F]{{8}}\s+{re.escape(symbol)}:\s*$", listing, re.MULTILINE)
    if start is None:
        raise ValueError(f"dialogue H1 section is missing: {symbol}")
    end = listing.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"dialogue H1 section end is missing: {symbol}")
    rows = []
    for raw in listing[start.start() : end].splitlines():
        match = re.fullmatch(r"(?P<address>[0-9A-F]{8})\s+(?P<body>.*)", raw)
        if match is None:
            continue
        body = re.sub(r"^(?:[0-9A-F]{2,8}\s+)+", "", match["body"].split(";", 1)[0]).strip()
        if not body or body.endswith(":") or body.startswith("M "):
            continue
        if re.fullmatch(r"[A-Za-z][A-Za-z0-9_]*(?:\.[bwls])?(?:\s+.+)?", body) is None:
            raise ValueError(f"dialogue H1 instruction parse drift: {raw}")
        rows.append(
            {"address": int(match["address"], 16), "instruction": re.sub(r"\s+", " ", body)}
        )
    return rows


def _parse_equates(upstream: Path, names: set[str]) -> dict[str, int]:
    source = (upstream / CONSTANTS_PATH).read_text(encoding="utf-8")
    values = {}
    for name in names:
        match = re.search(
            rf"^{re.escape(name)}:\s+equ\s+(\$[0-9A-Fa-f]+|-?\d+)\b", source, re.MULTILINE
        )
        if match is None:
            raise ValueError(f"dialogue RAM equate is missing: {name}")
        values[name] = _literal(match.group(1))
    return values


def _parse_source_equates(upstream: Path) -> dict[str, int]:
    """Use one parsed source-equate map for transient command operand resolution."""
    source = (upstream / ENUMS_PATH).read_text(encoding="utf-8")
    values = {}
    for match in re.finditer(
        r"^(?P<name>[A-Za-z_][A-Za-z0-9_]*):\s+equ\s+(?P<value>\$[0-9A-Fa-f]+|%[01]+|-?\d+)\b",
        source,
        re.MULTILINE,
    ):
        values[match["name"]] = _literal(match["value"])
    if not values:
        raise ValueError("dialogue source-equate inventory is empty")
    return values


def _direct_call_instruction(instruction: str) -> tuple[str, str] | None:
    """Parse one direct JSR/BSR instruction without accepting textual near-misses."""
    match = re.fullmatch(
        r"(?P<opcode>bsr|jsr)(?:\.[bwls])?\s+(?:\((?P<parenthesized>[A-Za-z_][A-Za-z0-9_]*)\)(?:\.[bwls])?|(?P<plain>[A-Za-z_][A-Za-z0-9_]*))",
        instruction,
    )
    if match is None:
        return None
    return match["opcode"], match["parenthesized"] or match["plain"]


def _source_call_targets(section: list[str]) -> list[tuple[str, str]]:
    calls = []
    for instruction in section:
        parsed = _direct_call_instruction(instruction)
        if parsed is not None:
            calls.append(parsed)
    if any(target not in TARGET_RESOLUTIONS for _, target in calls):
        raise ValueError(f"dialogue bounded direct-call target drift: {calls}")
    return calls


def _d0_write_source(instruction: str) -> str | None:
    """Return the exact source write retained by an entry shim, if any."""
    if re.search(r",\s*d0$", instruction) is None:
        return None
    if instruction not in {
        "move.w (a6),d0",
        "move.w ((CUTSCENE_DIALOG_INDEX-$1000000)).w,d0",
        "moveq #10,d0",
    }:
        raise ValueError(f"dialogue unmodeled d0 use-site drift: {instruction}")
    return instruction


def _callback_plan(
    section: list[str], h1: list[dict[str, Any]], addresses: dict[str, int]
) -> list[dict[str, Any]]:
    plan = []
    last_d0_source = None
    for instruction in section:
        d0_source = _d0_write_source(instruction)
        if d0_source is not None:
            last_d0_source = d0_source
        parsed = _direct_call_instruction(instruction)
        if parsed is None:
            continue
        opcode, target = parsed
        matches = [
            index
            for index, row in enumerate(h1)
            if _direct_call_instruction(row["instruction"]) == (opcode, target)
        ]
        if len(matches) != 1 or matches[0] + 1 >= len(h1):
            raise ValueError(f"dialogue H1 callback identity drift: {target}")
        effective, scope = TARGET_RESOLUTIONS[target]
        if target not in addresses or effective not in addresses:
            raise ValueError(f"dialogue callback symbol drift: {target}")
        row = h1[matches[0]]
        plan.append(
            {
                "instructionTarget": target,
                "effectiveTarget": effective,
                "effectiveTargetScope": scope,
                "callSiteAddress": row["address"],
                "instructionTargetAddress": addresses[target],
                "effectiveTargetAddress": addresses[effective],
                "returnAddress": h1[matches[0] + 1]["address"],
                "d0SourceInstruction": last_d0_source,
            }
        )
    return plan


def _state_write_sites(section: list[str], h1: list[dict[str, Any]]) -> list[dict[str, Any]]:
    site_kinds = {
        "move.w d2,((CURRENT_SPEECH_SFX-$1000000)).w": "speech-from-d2",
        "move.w #0,((CURRENT_SPEECH_SFX-$1000000)).w": "speech-zero",
        "move.w (a6)+,((DIALOGUE_NAME_INDEX_1-$1000000)).w": "name-index-1",
        "move.w (a6)+,((DIALOGUE_NAME_INDEX_2-$1000000)).w": "name-index-2",
        "addq.w #1,((CUTSCENE_DIALOG_INDEX-$1000000)).w": "dialogue-index-increment",
        "move.w (a6)+,((CUTSCENE_DIALOG_INDEX-$1000000)).w": "dialogue-index-direct",
    }
    result = []
    for instruction in section:
        kind = site_kinds.get(instruction)
        if kind is None:
            continue
        matches = [index for index, row in enumerate(h1) if row["instruction"] == instruction]
        if len(matches) != 1 or matches[0] + 1 >= len(h1):
            raise ValueError(f"dialogue H1 state-write use-site drift: {instruction}")
        result.append(
            {
                "kind": kind,
                "instruction": instruction,
                "instructionAddress": h1[matches[0]]["address"],
                "resumeAddress": h1[matches[0] + 1]["address"],
                "widthBytes": 2,
            }
        )
    return result


def _handler_record(
    row: dict[str, Any],
    source: str,
    listing: str,
    addresses: dict[str, int],
    *,
    operand_bytes: int,
) -> dict[str, Any]:
    macro, handler = row["macro"], row["handler"]
    section = _source_section(source, handler)
    if tuple(section) != CONTROL_SECTIONS[handler]:
        raise ValueError(f"dialogue guarded source section drift: {handler}")
    h1 = _h1_rows(listing, handler)
    if not h1 or h1[0]["address"] != row["address"] or h1[-1]["instruction"] != "rts":
        raise ValueError(f"dialogue H1 handler identity/return drift: {handler}")
    skip_target = None
    skip_guard = row.get("skipGuard")
    if macro in SKIP_MACROS:
        if not isinstance(skip_guard, dict) or set(skip_guard) != {"predicate", "branch"}:
            raise ValueError(f"dialogue skip guard shape drift: {handler}")
        if tuple(section[:2]) != (skip_guard["predicate"], skip_guard["branch"]):
            raise ValueError(f"dialogue skip guard source polarity drift: {handler}")
        branch_target = skip_guard["branch"].split()[-1]
        if branch_target not in addresses:
            raise ValueError(f"dialogue skip branch target drift: {handler}")
        skip_target = addresses[branch_target]
    elif skip_guard is not None:
        raise ValueError(f"dialogue unexpected skip guard: {handler}")
    if operand_bytes not in {0, 2, 4, 6}:
        raise ValueError(f"dialogue stored-operand byte boundary drift: {handler}")
    reads = [
        instruction
        for instruction in section
        if instruction == "adda.w #2,a6" or instruction.startswith("move.w (a6)+,")
    ]
    normal_reads = reads[:-1] if macro in SKIP_MACROS else reads
    source_cursor_bytes = sum(2 for _ in normal_reads)
    if source_cursor_bytes not in {0, 2, 6}:
        raise ValueError(f"dialogue cursor source/use-site boundary drift: {handler}")
    callbacks = _callback_plan(section, h1, addresses)
    writes = _state_write_sites(section, h1)
    sentinel = row.get("modifierEntityWordSentinel")
    if macro in DISPLAY_MACROS:
        if not isinstance(sentinel, dict) or set(sentinel) != {
            "unsignedValue",
            "signedValue",
            "branch",
        }:
            raise ValueError(f"dialogue sentinel source fact drift: {handler}")
        if sentinel["unsignedValue"] != 0xFFFF or sentinel["signedValue"] != -1:
            raise ValueError(f"dialogue sentinel value drift: {handler}")
    elif sentinel is not None:
        raise ValueError(f"dialogue non-display sentinel drift: {handler}")
    return {
        "macro": macro,
        "handler": handler,
        "handlerAddress": row["address"],
        "returnAddress": h1[-1]["address"],
        "storedOperandByteCount": operand_bytes,
        "cursorAdvanceByteCount": source_cursor_bytes,
        "skipBranchTargetAddress": skip_target,
        "sentinelWord": None if sentinel is None else sentinel["unsignedValue"],
        "directCallPlan": callbacks,
        "stateWriteUseSites": writes,
    }


def _h2_dialogue_handlers(facts: dict[str, Any]) -> list[dict[str, Any]]:
    """Accept the three exact H2 handler record shapes without broadening them."""
    if not isinstance(facts["handlers"], list):
        raise ValueError("dialogue H2 handler container drift")
    shapes = {
        "display": {
            "macro",
            "handler",
            "address",
            "opcode",
            "skipGuard",
            "modifierEntityWordSentinel",
            "nameWordDestinationCount",
            "displayThenIndexIncrement",
            "singleCloseSleepSequence",
        },
        "cursor": {"macro", "handler", "address", "opcode", "cursorWrite"},
        "hide": {"macro", "handler", "address", "opcode", "closeThenClear"},
    }
    result = []
    for row in facts["handlers"]:
        if not isinstance(row, dict):
            raise ValueError("dialogue H2 handler record is not an object")
        shape = (
            "display"
            if row.get("macro") in DISPLAY_MACROS
            else "cursor"
            if row.get("macro") == "textCursor"
            else "hide"
        )
        if set(row) != shapes[shape]:
            raise ValueError("dialogue H2 handler record shape drift")
        result.append(row)
    return result


def _full_source_rows(
    facts: dict[str, Any],
    compact: dict[str, Any],
    program_corpus: dict[str, Any],
    source_equates: dict[str, int],
) -> list[dict[str, Any]]:
    """Join H2's compact 205-program references to the one parsed source corpus.

    The joined 2,883 rows are intentionally transient.  They are neither a
    second H2 output field nor a fixture corpus; only their count/order digest
    is durable alongside the existing exact reference and total tables.
    """
    references = _closed_rows(
        facts["sourceSiteReferences"], {"programId", "commandIndexes"}, name="source references"
    )
    programs = {row["id"]: row for row in program_corpus["programs"]}
    if len(programs) != len(program_corpus["programs"]):
        raise ValueError("dialogue authoritative program identity drift")
    layouts = {row["name"]: row["operandLayout"] for row in facts["macros"]}
    if any(
        source_equates.get(name) != value
        for name, value in facts["operandFacts"]["constants"].items()
    ):
        raise ValueError("dialogue H2/source equate provenance drift")
    rows = []
    for reference in references:
        program = programs.get(reference["programId"])
        if program is None:
            raise ValueError("dialogue source-reference program drift")
        commands = {row["index"]: row for row in program["commands"]}
        for index in reference["commandIndexes"]:
            command = commands.get(index)
            if command is None or command["macro"] not in layouts:
                raise ValueError("dialogue source-reference command drift")
            arguments, layout = command["arguments"], layouts[command["macro"]]
            if len(arguments) != len(layout):
                raise ValueError("dialogue transient source operand-layout drift")
            operands = []
            for ordinal, (raw, field) in enumerate(zip(arguments, layout, strict=True), start=1):
                if raw in source_equates:
                    value, resolution = source_equates[raw], "source-constant"
                else:
                    value, resolution = _literal(raw), "literal"
                operands.append(
                    {
                        "parameterOrdinal": ordinal,
                        "rawValue": raw,
                        "resolvedValue": value,
                        "resolution": resolution,
                        "streamOffset": field["streamOffset"],
                        "widthBytes": field["widthBytes"],
                    }
                )
            rows.append(
                {
                    "programId": reference["programId"],
                    "commandIndex": index,
                    "sourceLine": command["sourceLine"],
                    "macro": command["macro"],
                    "arguments": arguments,
                    "sourceOrderKey": f"{reference['programId']}:{index}:{command['macro']}",
                    "operandValues": operands,
                }
            )
    source_summary = facts["sourceInputSummary"]
    keys = [row["sourceOrderKey"] for row in rows]
    digest = hashlib.sha256(_canonical_bytes({"sourceInputOrderKeys": keys})).hexdigest().upper()
    if source_summary != compact["sourceInputSummary"] or source_summary != {
        "count": len(keys),
        "sha256": digest,
    }:
        raise ValueError("dialogue compact/transient source summary drift")
    totals = []
    for program in program_corpus["programs"]:
        counts = {macro: 0 for macro, _ in HANDLER_FORMS}
        for command in program["commands"]:
            if command["macro"] in counts:
                counts[command["macro"]] += 1
        totals.append(
            {
                "programId": program["id"],
                "commandCount": sum(counts.values()),
                "macroCounts": counts,
            }
        )
    if totals != facts["programTotals"]:
        raise ValueError("dialogue zero-inclusive program-total join drift")
    counts = {macro: sum(row["macro"] == macro for row in rows) for macro, _ in HANDLER_FORMS}
    if counts != {row["name"]: row["sourceCommandCount"] for row in facts["macros"]}:
        raise ValueError("dialogue complete source macro inventory drift")
    return rows


def _entity_partition(value: int, sentinel: int) -> str:
    if value == sentinel:
        return "sentinel"
    return "high-bit-set" if value & 0x80 else "high-bit-clear"


def _source_partition_cases(
    rows: list[dict[str, Any]], facts: dict[str, Any], sentinel: int
) -> list[dict[str, Any]]:
    result = []
    for macro in ("nextSingleText", "nextText"):
        source_rows = [row for row in rows if row["macro"] == macro]
        partitions: dict[tuple[int, str], dict[str, Any]] = {}
        for row in source_rows:
            operands = row["operandValues"]
            if len(operands) != 2 or [item["widthBytes"] for item in operands] != [1, 1]:
                raise ValueError(f"dialogue packed source operand shape drift: {macro}")
            modifier, entity = (item["resolvedValue"] for item in operands)
            if not 0 <= modifier <= 0xFF or not 0 <= entity <= 0xFF:
                raise ValueError(f"dialogue packed source byte boundary drift: {macro}")
            partitions.setdefault((modifier, _entity_partition(entity, sentinel & 0xFF)), row)
        for (modifier, partition), row in partitions.items():
            result.append(
                {
                    "id": f"{macro}-source-modifier-{modifier}-{partition}",
                    "macro": macro,
                    "sourceStatus": "source",
                    "sourceOrderKey": row["sourceOrderKey"],
                    "skipFlagByteSeed": 0,
                    "inputWords": [],
                }
            )
    modifier_values = {row["value"] for row in facts["operandFacts"]["modifierByteCounts"]}
    case_values = {int(case["id"].split("-modifier-")[1].split("-", 1)[0]) for case in result}
    if case_values != modifier_values:
        raise ValueError("dialogue modifier partition coverage drift")
    return result


def build_map_script_dialogue_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    """Parse full H2 input rows plus the six H1 handler boundaries."""
    h2_fixture = load_json(H2_FIXTURE)
    h2 = build_map_script_engine_contract(rom_path, upstream_path)
    facts = h2["dialogueCommandFacts"]
    compact = h2_fixture["expected"]["dialogueCommandFacts"]
    if {key: facts[key] for key in compact} != compact:
        raise ValueError("dialogue H2 compact fixture/source drift")
    if facts["runtimeQuestions"] != RUNTIME_QUESTIONS:
        raise ValueError("dialogue H2 runtime-question handoff drift")
    upstream = upstream_path.resolve(strict=True)
    listing = (upstream / H1_LISTING_PATH).read_text(encoding="utf-8")
    addresses = listing_symbol_addresses(listing)
    required = {
        "RunMapSetupInitFunction",
        *TARGET_RESOLUTIONS,
        *EFFECTIVE_TARGETS,
        *(handler for _, handler in HANDLER_FORMS),
    }
    if not required <= addresses.keys():
        raise ValueError("dialogue H1 target symbol inventory drift")
    source_equates = _parse_source_equates(upstream)
    rows = _full_source_rows(facts, compact, h2["programCorpus"], source_equates)
    h2_handlers = _h2_dialogue_handlers(facts)
    if [(row["macro"], row["handler"]) for row in h2_handlers] != list(HANDLER_FORMS):
        raise ValueError("dialogue H2 handler identity/order drift")
    source = (upstream / "disasm/code/common/scripting/map/mapscriptengine_2.asm").read_text(
        encoding="utf-8"
    )
    macro_facts = {row["name"]: row for row in facts["macros"]}
    handlers = [
        _handler_record(
            row,
            source,
            listing,
            addresses,
            operand_bytes=macro_facts[row["macro"]]["operandBytes"],
        )
        for row in h2_handlers
    ]
    helper = facts["portraitHelper"]
    if (
        helper["handler"] != "csc1D_showPortrait"
        or helper["address"] != addresses[helper["handler"]]
    ):
        raise ValueError("dialogue portrait-helper provenance drift")
    return {
        "provenance": {
            "upstreamRepository": "https://github.com/ShiningForceCentral/SF2DISASM",
            "upstreamBranch": "master",
            "upstreamCommit": h2_fixture["upstreamCommit"],
            "h2FixturePath": "tests/fixtures/h2/map-script-engine-static-v1.json",
            "h2FixtureId": h2_fixture["id"],
            "h2FieldPath": "expected.dialogueCommandFacts",
            "uiPrimaryFixturePath": "tests/fixtures/h3/map-script-ui-primary-v1.json",
            "uiPrimaryFixtureId": "sf2-map-script-ui-primary-runtime-v1",
            "command": "uv run sf2 h2 map-script-engine",
        },
        "romSha256": h2_fixture["romSha256"],
        "function": {
            "entryAddress": addresses["RunMapSetupInitFunction"],
            **{f"{row['handler']}Address": row["handlerAddress"] for row in handlers},
            **{f"{row['handler']}ReturnAddress": row["returnAddress"] for row in handlers},
            **{
                f"{row['handler']}SkipBranchTargetAddress": row["skipBranchTargetAddress"]
                for row in handlers
                if row["skipBranchTargetAddress"] is not None
            },
        },
        "ram": _parse_equates(
            upstream,
            {
                "SKIP_CUTSCENE_TEXT",
                "CURRENT_SPEECH_SFX",
                "CUTSCENE_DIALOG_INDEX",
                "DIALOGUE_NAME_INDEX_1",
                "DIALOGUE_NAME_INDEX_2",
            },
        ),
        "constants": {
            "sentinelWord": handlers[0]["sentinelWord"],
            "modifierByteValues": [
                row["value"] for row in facts["operandFacts"]["modifierByteCounts"]
            ],
            "textCursorValueBounds": facts["operandFacts"]["textCursorValueBounds"],
        },
        "sourceFacts": {
            "sourceContract": {
                "sourceSiteReferenceCount": len(compact["sourceSiteReferences"]),
                "programTotalCount": len(compact["programTotals"]),
                "sourceInputSummary": compact["sourceInputSummary"],
            },
            "sourceInputRows": rows,
            "handlers": handlers,
            "callerBreakdown": facts["callerBreakdown"],
            "portraitHelperJoin": helper,
            "evidenceLabels": {"staticFindings": "Confirmed", "runtimeObservations": "Confirmed"},
        },
        "runtimeQuestions": facts["runtimeQuestions"],
    }


def _handler(static: dict[str, Any], macro: str) -> dict[str, Any]:
    matches = [row for row in static["sourceFacts"]["handlers"] if row["macro"] == macro]
    if len(matches) != 1:
        raise ValueError(f"dialogue handler lookup drift: {macro}")
    return matches[0]


def _row_by_key(static: dict[str, Any], key: str) -> dict[str, Any]:
    matches = [
        row for row in static["sourceFacts"]["sourceInputRows"] if row["sourceOrderKey"] == key
    ]
    if len(matches) != 1:
        raise ValueError(f"dialogue source case identity drift: {key}")
    return matches[0]


def _state_site(handler: dict[str, Any], kind: str) -> dict[str, Any]:
    matches = [row for row in handler["stateWriteUseSites"] if row["kind"] == kind]
    if len(matches) != 1:
        raise ValueError(f"dialogue state use-site drift: {handler['handler']}/{kind}")
    return matches[0]


def _callback_observation(call: dict[str, Any]) -> dict[str, Any]:
    return {
        "instructionTargetObserved": call["instructionTarget"],
        "effectiveTargetObserved": call["effectiveTarget"],
        "callSiteAddressObserved": call["callSiteAddress"],
        "targetEntryAddressObserved": call["instructionTargetAddress"],
        "returnAddressObserved": call["returnAddress"],
    }


def _session_register_seeds(fixture: dict[str, Any]) -> dict[str, int]:
    """Parse the session trampoline's explicit D0/D1/D2 inputs once.

    These words are intentionally harness inputs, not claims about the normal
    map-script caller.  The exact immediate encodings keep the recorded call
    registers falsifiable independently from the runtime golden fixture.
    """
    instrumentation = fixture["instrumentation"]
    seeds = instrumentation["registerSeeds"]
    if set(seeds) != {"d0", "d1", "d2"} or any(
        not isinstance(value, int) or not 0 <= value <= 0xFFFF for value in seeds.values()
    ):
        raise ValueError("dialogue session register-seed shape drift")
    ram = instrumentation["ramInputAddress"]
    expected = (
        b"\x4d\xf9"
        + (ram + instrumentation["scriptInputRamOffset"]).to_bytes(4, "big")
        + b"\x30\x3c"
        + seeds["d0"].to_bytes(2, "big")
        + b"\x32\x3c"
        + seeds["d1"].to_bytes(2, "big")
        + b"\x34\x3c"
        + seeds["d2"].to_bytes(2, "big")
        + b"\x20\x79"
        + ram.to_bytes(4, "big")
        + b"\x4e\x90\x4e\x75"
    )
    if bytes.fromhex(instrumentation["stubHex"]) != expected:
        raise ValueError("dialogue session register-seed trampoline drift")
    return seeds


def _register_words_for_calls(
    calls: list[dict[str, Any]], input_words: list[int], fixture: dict[str, Any]
) -> list[list[int]]:
    """Derive entry registers from parsed D0 use sites and verified RTS shims."""
    seeds = _session_register_seeds(fixture)
    d2 = seeds["d2"]
    derived = []
    for call in calls:
        source = call["d0SourceInstruction"]
        if source is None:
            d0 = seeds["d0"]
        elif source == "move.w (a6),d0":
            d0 = input_words[0]
        elif source == "move.w ((CUTSCENE_DIALOG_INDEX-$1000000)).w,d0":
            d0 = fixture["instrumentation"]["stateSeeds"]["dialogueIndexWord"]
        elif source == "moveq #10,d0":
            d0 = 10
        else:
            raise ValueError(f"dialogue d0 call-source drift: {source}")
        derived.append([d0, seeds["d1"], d2])
        if call["instructionTarget"] == "GetEntityPortaitAndSpeechSfx":
            d2 = fixture["instrumentation"]["speechSfxWordSeed"]
    return derived


def _derive_case(
    case: dict[str, Any], static: dict[str, Any], fixture: dict[str, Any]
) -> dict[str, Any]:
    macro = case["macro"]
    handler = _handler(static, macro)
    source = None
    if case["sourceStatus"] == "source":
        if not isinstance(case["sourceOrderKey"], str):
            raise ValueError(f"dialogue source case has no source key: {case['id']}")
        source = _row_by_key(static, case["sourceOrderKey"])
        if source["macro"] != macro:
            raise ValueError(f"dialogue source macro drift: {case['id']}")
        operands = source["operandValues"]
        if macro in DISPLAY_MACROS:
            input_words = [(operands[0]["resolvedValue"] << 8) | operands[1]["resolvedValue"]]
        elif macro == "textCursor":
            input_words = [operands[0]["resolvedValue"]]
        else:
            input_words = []
        if case["inputWords"]:
            raise ValueError(f"dialogue source case redundantly supplies inputs: {case['id']}")
    elif case["sourceStatus"] == "controlled-zero-source":
        if macro not in {"nextSingleTextVar", "nextTextVar"} or case["sourceOrderKey"] is not None:
            raise ValueError(f"dialogue controlled source boundary drift: {case['id']}")
        input_words = case["inputWords"]
    else:
        raise ValueError(f"dialogue source-status drift: {case['id']}")
    if any(not isinstance(value, int) or not 0 <= value <= 0xFFFF for value in input_words):
        raise ValueError(f"dialogue input word boundary drift: {case['id']}")
    if macro in DISPLAY_MACROS and len(input_words) != (3 if macro.endswith("Var") else 1):
        raise ValueError(f"dialogue display operand count drift: {case['id']}")
    if macro == "textCursor" and len(input_words) != 1:
        raise ValueError(f"dialogue cursor operand count drift: {case['id']}")
    if macro == "hideText" and input_words:
        raise ValueError(f"dialogue hide input drift: {case['id']}")
    skip = case["skipFlagByteSeed"] != 0
    if skip and macro not in SKIP_MACROS:
        raise ValueError(f"dialogue invalid controlled skip case: {case['id']}")
    sentinel = handler["sentinelWord"]
    calls = list(handler["directCallPlan"])
    if macro in DISPLAY_MACROS:
        if skip:
            calls = []
        elif input_words[0] == sentinel:
            calls = calls[2:]
    counts = {target: 0 for target in EFFECTIVE_TARGETS}
    for call in calls:
        counts[call["effectiveTarget"]] += 1
    seeds = fixture["instrumentation"]["stateSeeds"]
    writes = []
    if macro in DISPLAY_MACROS and not skip:
        speech_value = (
            0 if input_words[0] == sentinel else fixture["instrumentation"]["speechSfxWordSeed"]
        )
        kind = "speech-zero" if input_words[0] == sentinel else "speech-from-d2"
        site = _state_site(handler, kind)
        writes.append(
            {
                "kindObserved": kind,
                "resumeAddressObserved": site["resumeAddress"],
                "wordValueObserved": speech_value,
            }
        )
        if macro.endswith("Var"):
            for offset, kind in enumerate(("name-index-1", "name-index-2"), start=1):
                site = _state_site(handler, kind)
                writes.append(
                    {
                        "kindObserved": kind,
                        "resumeAddressObserved": site["resumeAddress"],
                        "wordValueObserved": input_words[offset],
                    }
                )
        site = _state_site(handler, "dialogue-index-increment")
        writes.append(
            {
                "kindObserved": "dialogue-index-increment",
                "resumeAddressObserved": site["resumeAddress"],
                "wordValueObserved": (seeds["dialogueIndexWord"] + 1) & 0xFFFF,
            }
        )
    elif macro == "textCursor":
        site = _state_site(handler, "dialogue-index-direct")
        writes.append(
            {
                "kindObserved": "dialogue-index-direct",
                "resumeAddressObserved": site["resumeAddress"],
                "wordValueObserved": input_words[0],
            }
        )
    return {
        "id": case["id"],
        "handlerEntryPcObserved": handler["handlerAddress"],
        "handlerReturnPcObserved": handler["returnAddress"],
        "handlerReturned": True,
        "scriptCursorRamOffsetAfterObserved": fixture["instrumentation"]["scriptInputRamOffset"]
        + handler["cursorAdvanceByteCount"],
        "stackPointerDeltaBytesObserved": _handler_call_frame_byte_count(fixture),
        "skipFlagBranchTakenObserved": skip if macro in SKIP_MACROS else None,
        "directCallsObserved": [_callback_observation(call) for call in calls],
        "effectiveTargetCountsObserved": counts,
        "stateWritesObserved": writes,
        "directCallRegisterWordsObserved": _register_words_for_calls(calls, input_words, fixture),
    }


def derive_case_expectations(
    static: dict[str, Any], fixture: dict[str, Any]
) -> list[dict[str, Any]]:
    if fixture["sourceContract"] != static["sourceFacts"]["sourceContract"]:
        raise ValueError("dialogue fixture/source compact contract drift")
    cases = _source_partition_cases(
        static["sourceFacts"]["sourceInputRows"],
        {
            "operandFacts": {
                "modifierByteCounts": [
                    {"value": value} for value in static["constants"]["modifierByteValues"]
                ]
            }
        },
        static["constants"]["sentinelWord"],
    )
    fixture_source = [
        {
            key: case[key]
            for key in (
                "id",
                "macro",
                "sourceStatus",
                "sourceOrderKey",
                "skipFlagByteSeed",
                "inputWords",
            )
        }
        for case in fixture["cases"]
        if case["sourceStatus"] == "source"
        and "-skip" not in case["id"]
        and case["macro"] in {"nextSingleText", "nextText"}
    ]
    if fixture_source != cases:
        raise ValueError("dialogue packed source partition fixture drift")
    source_rows = static["sourceFacts"]["sourceInputRows"]
    text_values = static["constants"]["textCursorValueBounds"]
    required_keys = []
    for value in (text_values["minimum"], text_values["maximum"]):
        matches = [
            row["sourceOrderKey"]
            for row in source_rows
            if row["macro"] == "textCursor" and row["operandValues"][0]["resolvedValue"] == value
        ]
        if not matches:
            raise ValueError("dialogue text-cursor boundary source identity drift")
        required_keys.append(matches[0])
    cursor_cases = [case for case in fixture["cases"] if case["macro"] == "textCursor"]
    if [case["sourceOrderKey"] for case in cursor_cases] != required_keys:
        raise ValueError("dialogue text-cursor boundary fixture drift")
    if {case["macro"] for case in fixture["cases"]} != {macro for macro, _ in HANDLER_FORMS}:
        raise ValueError("dialogue complete handler coverage drift")
    if sum(case["sourceStatus"] == "controlled-zero-source" for case in fixture["cases"]) != 2:
        raise ValueError("dialogue zero-source control count drift")
    if [
        case["skipFlagByteSeed"] for case in fixture["cases"] if "-skip-flag-set" in case["id"]
    ] != [1, 1]:
        raise ValueError("dialogue skip-polarity fixture drift")
    derived = [_derive_case(case, static, fixture) for case in fixture["cases"]]
    for row, case in zip(derived, fixture["cases"], strict=True):
        if (
            row["directCallRegisterWordsObserved"]
            != case["runtimeGolden"]["directCallRegisterWordsObserved"]
        ):
            raise ValueError(f"dialogue source-derived register golden drift: {case['id']}")
    digest = hashlib.sha256(_canonical_bytes({"cases": derived})).hexdigest().upper()
    if fixture["caseSemanticsSha256"] != digest:
        raise ValueError("dialogue derived case semantics drift")
    return derived


def _service_patches(
    static: dict[str, Any], rom_path: Path, fixture: dict[str, Any]
) -> list[dict[str, Any]]:
    rows = _closed_rows(
        fixture["instrumentation"]["serviceInterception"],
        {"instructionTarget", "address", "originalHex", "patchedHex"},
        name="service interception",
    )
    callbacks = [
        call for handler in static["sourceFacts"]["handlers"] for call in handler["directCallPlan"]
    ]
    addresses = {call["instructionTarget"]: call["instructionTargetAddress"] for call in callbacks}
    if set(addresses) != set(TARGET_RESOLUTIONS):
        raise ValueError("dialogue service target inventory drift")
    configured = {row["instructionTarget"]: row for row in rows}
    if len(configured) != len(rows) or set(configured) != set(TARGET_RESOLUTIONS):
        raise ValueError("dialogue service interception identity drift")
    seed = (
        fixture["instrumentation"]["ramInputAddress"]
        + fixture["instrumentation"]["speechSfxSeedRamOffset"]
    )
    expected_patch = {
        "csc1D_showPortrait": b"\x4e\x75",
        "GetEntityPortaitAndSpeechSfx": b"\x34\x39" + seed.to_bytes(4, "big") + b"\x4e\x75",
        "WaitForViewScrollEnd": b"\x4e\x75",
        "DisplayText": b"\x4e\x75",
        "j_ClosePortraitWindow": b"\x4e\x75",
        "Sleep": b"\x4e\x75",
    }
    rom = rom_path.read_bytes()
    result = []
    for target in TARGET_RESOLUTIONS:
        row = configured[target]
        original, patched = bytes.fromhex(row["originalHex"]), bytes.fromhex(row["patchedHex"])
        if (
            row["address"] != addresses[target]
            or patched != expected_patch[target]
            or len(original) != len(patched)
        ):
            raise ValueError(f"dialogue service shim shape drift: {target}")
        if rom[row["address"] : row["address"] + len(original)] != original:
            raise ValueError(f"dialogue service shim ROM preflight drift: {target}")
        result.append(
            {
                "instructionTarget": target,
                "address": row["address"],
                "originalBytes": original,
                "patchedBytes": patched,
            }
        )
    return result


def _instrument_dialogue_rom(
    rom_path: Path, fixture: dict[str, Any], static: dict[str, Any]
) -> Path:
    base = _instrument_rom(rom_path, fixture)
    data, original = bytearray(base.read_bytes()), rom_path.read_bytes()
    for patch in _service_patches(static, rom_path, fixture):
        address = patch["address"]
        if (
            original[address : address + len(patch["originalBytes"])] != patch["originalBytes"]
            or data[address : address + len(patch["originalBytes"])] != patch["originalBytes"]
        ):
            raise ValueError(f"dialogue instrumentation overlap: {patch['instructionTarget']}")
        data[address : address + len(patch["patchedBytes"])] = patch["patchedBytes"]
    data[0x18E:0x190] = int(mega_drive_checksum(bytes(data)), 16).to_bytes(2, "big")
    DERIVED_ROOT.mkdir(parents=True, exist_ok=True)
    output = DERIVED_ROOT / "map-script-dialogue.instrumented.bin"
    output.write_bytes(data)
    return output


def _observer_cases(fixture: dict[str, Any], static: dict[str, Any]) -> list[dict[str, Any]]:
    rows = []
    for case in fixture["cases"]:
        input_words = _derive_case_inputs(case, static)
        rows.append(
            {
                "id": case["id"],
                "handlerAddress": _handler(static, case["macro"])["handlerAddress"],
                "skipFlagByteSeed": case["skipFlagByteSeed"],
                "inputWords": input_words,
            }
        )
    return rows


def _derive_case_inputs(case: dict[str, Any], static: dict[str, Any]) -> list[int]:
    if case["sourceStatus"] == "controlled-zero-source":
        return case["inputWords"]
    row = _row_by_key(static, case["sourceOrderKey"])
    operands = row["operandValues"]
    if case["macro"] in DISPLAY_MACROS:
        return [(operands[0]["resolvedValue"] << 8) | operands[1]["resolvedValue"]]
    if case["macro"] == "textCursor":
        return [operands[0]["resolvedValue"]]
    return []


def _handler_call_frame_byte_count(fixture: dict[str, Any]) -> int:
    """Derive the observed pre-RTS stack delta from the instrumented JSR stub."""
    stub = bytes.fromhex(fixture["instrumentation"]["stubHex"])
    if stub.count(b"\x4e\x90") != 1 or not stub.endswith(b"\x4e\x90\x4e\x75"):
        raise ValueError("dialogue handler-call trampoline shape drift")
    return 4


def _state_write_addresses(static: dict[str, Any]) -> dict[str, dict[str, dict[str, Any]]]:
    """Map each guarded post-write PC to the RAM word read by the observer."""
    ram_name = {
        "speech-from-d2": "CURRENT_SPEECH_SFX",
        "speech-zero": "CURRENT_SPEECH_SFX",
        "name-index-1": "DIALOGUE_NAME_INDEX_1",
        "name-index-2": "DIALOGUE_NAME_INDEX_2",
        "dialogue-index-increment": "CUTSCENE_DIALOG_INDEX",
        "dialogue-index-direct": "CUTSCENE_DIALOG_INDEX",
    }
    by_resume: dict[str, dict[str, Any]] = {}
    by_instruction: dict[str, dict[str, Any]] = {}
    for handler in static["sourceFacts"]["handlers"]:
        for site in handler["stateWriteUseSites"]:
            value = {
                "kind": site["kind"],
                "address": static["ram"][ram_name[site["kind"]]],
                "resumeAddress": site["resumeAddress"],
            }
            resume_key, instruction_key = (
                str(site["resumeAddress"]),
                str(site["instructionAddress"]),
            )
            if resume_key in by_resume and by_resume[resume_key] != value:
                raise ValueError("dialogue state-write callback identity drift")
            if instruction_key in by_instruction and by_instruction[instruction_key] != value:
                raise ValueError("dialogue state-write instruction identity drift")
            by_resume[resume_key] = value
            by_instruction[instruction_key] = value
    return {"byResume": by_resume, "byInstruction": by_instruction}


def _observer_identity_maps(static: dict[str, Any]) -> dict[str, Any]:
    """Bind callback names only through unique source/H1 addresses for the observer."""
    handler_entries: dict[str, dict[str, str]] = {}
    call_sites: dict[str, dict[str, Any]] = {}
    target_entries: dict[str, dict[str, str]] = {}
    return_addresses: dict[str, bool] = {}
    for handler in static["sourceFacts"]["handlers"]:
        handler_key = str(handler["handlerAddress"])
        if handler_key in handler_entries:
            raise ValueError("dialogue observer handler-entry identity drift")
        handler_entries[handler_key] = {"handler": handler["handler"]}
        for call in handler["directCallPlan"]:
            call_key = str(call["callSiteAddress"])
            target_key = str(call["instructionTargetAddress"])
            return_key = str(call["returnAddress"])
            if call_key in call_sites:
                raise ValueError("dialogue observer call-site identity drift")
            call_sites[call_key] = {
                "handlerEntryAddress": handler["handlerAddress"],
                "instructionTarget": call["instructionTarget"],
                "effectiveTarget": call["effectiveTarget"],
                "targetEntryAddress": call["instructionTargetAddress"],
                "returnAddress": call["returnAddress"],
            }
            target_identity = {
                "instructionTarget": call["instructionTarget"],
                "effectiveTarget": call["effectiveTarget"],
            }
            existing_target = target_entries.setdefault(target_key, target_identity)
            if existing_target != target_identity:
                raise ValueError("dialogue observer target-entry identity drift")
            if return_key in return_addresses:
                raise ValueError("dialogue observer return-address identity drift")
            return_addresses[return_key] = True
    if {value["instructionTarget"] for value in target_entries.values()} != set(TARGET_RESOLUTIONS):
        raise ValueError("dialogue observer target coverage drift")
    return {
        "handlerEntryByAddress": handler_entries,
        "callSiteByAddress": call_sites,
        "targetEntryByAddress": target_entries,
        "returnAddressByAddress": return_addresses,
        "effectiveTargets": list(EFFECTIVE_TARGETS),
    }


def _validate_observed_identities(
    observed: dict[str, Any], derived: list[dict[str, Any]], static: dict[str, Any]
) -> None:
    """Reject a captured PC/address identity mismatch before golden comparison."""
    maps = _observer_identity_maps(static)
    records = observed.get("records")
    if not isinstance(records, list) or len(records) != len(derived):
        raise ValueError("dialogue observed record cardinality drift")
    declared_targets = maps["effectiveTargets"]
    for record, expected in zip(records, derived, strict=True):
        if record["id"] != expected["id"]:
            raise ValueError("dialogue observed record identity drift")
        entry_key = str(record["handlerEntryPcObserved"])
        if (
            entry_key not in maps["handlerEntryByAddress"]
            or record["handlerEntryPcObserved"] != expected["handlerEntryPcObserved"]
        ):
            raise ValueError("dialogue observed handler-entry PC drift")
        if record["handlerReturnPcObserved"] != expected["handlerReturnPcObserved"]:
            raise ValueError("dialogue observed handler-return PC drift")
        counts = {target: 0 for target in declared_targets}
        for call in record["directCallsObserved"]:
            call_key = str(call["callSiteAddressObserved"])
            target_key = str(call["targetEntryAddressObserved"])
            call_identity = maps["callSiteByAddress"].get(call_key)
            target_identity = maps["targetEntryByAddress"].get(target_key)
            if call_identity is None:
                raise ValueError("dialogue observed call-site PC drift")
            if target_identity is None:
                raise ValueError("dialogue observed target-entry PC drift")
            if (
                call_identity["handlerEntryAddress"] != record["handlerEntryPcObserved"]
                or call_identity["targetEntryAddress"] != call["targetEntryAddressObserved"]
                or call_identity["returnAddress"] != call["returnAddressObserved"]
                or target_identity["instructionTarget"] != call["instructionTargetObserved"]
                or target_identity["effectiveTarget"] != call["effectiveTargetObserved"]
                or call_identity["instructionTarget"] != call["instructionTargetObserved"]
                or call_identity["effectiveTarget"] != call["effectiveTargetObserved"]
            ):
                raise ValueError("dialogue observed call identity drift")
            counts[call["effectiveTargetObserved"]] += 1
        if record["effectiveTargetCountsObserved"] != counts:
            raise ValueError("dialogue observed zero-inclusive target count drift")


def verify_map_script_dialogue(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 180
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner="map-script dialogue fixture")
    verify_runtime_contract(fixture, rom_path)
    static = build_map_script_dialogue_contract(rom_path, upstream_path)
    for field in ("provenance", "romSha256", "function", "ram", "constants", "runtimeQuestions"):
        if fixture[field] != static[field]:
            raise ValueError(f"dialogue fixture/source identity drift: {field}")
    derived = derive_case_expectations(static, fixture)
    instrumented = _instrument_dialogue_rom(rom_path, fixture, static)

    def observe() -> dict[str, Any]:
        return run_observer(
            rom_path=instrumented,
            observer_path=OBSERVER,
            config={
                "fixtureId": fixture["id"],
                "mapTestIndex": fixture["mapTestIndex"],
                "function": static["function"],
                "ram": static["ram"],
                "constants": static["constants"],
                "instrumentation": fixture["instrumentation"],
                "maxFrames": fixture["maxFrames"],
                "harness": load_json(repo_path(fixture["sharedHarnessFixture"]))["harness"],
                "cases": _observer_cases(fixture, static),
                "observedIdentity": _observer_identity_maps(static),
                "stateWrites": _state_write_addresses(static),
            },
            output_name="map-script-dialogue",
            timeout_seconds=timeout_seconds,
        )

    observed = _with_instrumented_rom_database(
        instrumented, "SF2 H3 instrumented map-script dialogue", observe
    )
    validate_json(observed, OBSERVATION_SCHEMA, owner="map-script dialogue observation")
    _validate_observed_identities(observed, derived, static)
    expected = {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["id"],
        "mapTest": fixture["mapTestIndex"],
        "recordOrder": [case["id"] for case in fixture["cases"]],
        "records": derived,
    }
    if observed != expected:
        raise ValueError(
            f"dialogue runtime matrix mismatch\nexpected={expected!r}\nobserved={observed!r}"
        )
    return {
        "Fixture": fixture["id"],
        "Cases": len(derived),
        "Handlers": len(HANDLER_FORMS),
        "BizHawkLaunches": 1,
        "Instrumentation": "session-only dialogue service-entry shims",
        "Status": "PASS",
    }
