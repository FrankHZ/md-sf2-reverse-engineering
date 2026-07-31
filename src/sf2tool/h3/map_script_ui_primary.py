"""Handler-local runtime contract for primary map-script UI commands.

This rail deliberately intercepts the bounded service calls after their actual
68000 call sites execute.  It proves handler-local selection, cursor handling,
and return behavior without assigning UI/service side effects to those calls.
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
FIXTURE = repo_path("tests/fixtures/h3/map-script-ui-primary-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h3-map-script-ui-primary-fixture.schema.json")
OBSERVATION_SCHEMA = repo_path("schemas/h3-map-script-ui-primary-observation.schema.json")
OBSERVER = repo_path("tools/bizhawk/map_script_ui_primary_observer.lua")
H1_LISTING_PATH = Path("build/sf2build-h1.lst")
CONSTANTS_PATH = Path("disasm/sf2const.asm")

HANDLER_FORMS = (
    ("showPortrait", "csc1D_showPortrait", "code/common/scripting/map/mapscriptengine_1.asm"),
    ("hidePortrait", "csc1E_hidePortrait", "code/common/scripting/map/mapscriptengine_1.asm"),
    ("menu", "csc12_executeContextMenu", "code/common/scripting/map/mapscriptengine_2.asm"),
)

# These are intentionally the smallest named sections that support promoted
# handler-local claims.  Any operand, branch polarity/order, call, or return
# edit changes parser construction before a golden fixture is consulted.
CONTROL_SECTIONS = {
    "csc1D_showPortrait": (
        "move.w (a6)+,d0",
        "tst.w ((PORTRAIT_WINDOW_INDEX-$1000000)).w",
        "bne.w @Return",
        "moveq #0,d3",
        "btst #$F,d0",
        "beq.s @loc_1",
        "moveq #-1,d3",
        "moveq #0,d4",
        "btst #$E,d0",
        "beq.s @loc_2",
        "moveq #-1,d4",
        "jsr (WaitForViewScrollEnd).w",
        "bsr.w GetEntityPortaitAndSpeechSfx",
        "cmpi.w #-1,d1",
        "beq.s @Return",
        "move.w d1,d0",
        "move.w d3,d1",
        "move.w d4,d2",
        "jsr j_OpenPortraitWindow",
        "rts",
    ),
    "csc1E_hidePortrait": (
        "jsr (WaitForViewScrollEnd).w",
        "jsr j_ClosePortraitWindow",
        "rts",
    ),
    "csc12_executeContextMenu": (
        "move.w (a6)+,d0",
        "move.l a6,-(sp)",
        "tst.w d0",
        "bne.s loc_474C4",
        "jsr j_ChurchMenu",
        "cmpi.w #1,d0",
        "bne.s loc_474D0",
        "jsr j_ShopMenu",
        "cmpi.w #2,d0",
        "bne.s loc_474DC",
        "jsr j_BlacksmithMenu",
        "movea.l (sp)+,a6",
        "rts",
    ),
}

RUNTIME_QUESTIONS = [
    "map-script-ui-command/normal-story-reachability",
    "map-script-ui-command/full-window-animation-vdp-timing",
    "map-script-ui-command/real-user-choice-service-side-effects",
    "map-script-ui-command/save-persistence-map-entity-interactions",
]


def _literal(text: str) -> int:
    if re.fullmatch(r"\$[0-9A-Fa-f]+", text):
        return int(text[1:], 16)
    if re.fullmatch(r"%[01]+", text):
        return int(text[1:], 2)
    if re.fullmatch(r"-?\d+", text):
        return int(text)
    raise ValueError(f"map-script UI source literal is not numeric: {text}")


def _source_section(source: str, symbol: str) -> list[str]:
    start = re.search(rf"^{re.escape(symbol)}:\s*$", source, re.MULTILINE)
    if start is None:
        raise ValueError(f"map-script UI source function is missing: {symbol}")
    end = source.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"map-script UI source function end is missing: {symbol}")
    rows = []
    for raw in source[start.start() : end].splitlines():
        instruction = re.sub(r"\s+", " ", raw.split(";", 1)[0].strip())
        if instruction and not instruction.endswith(":"):
            rows.append(instruction)
    return rows


def _listing_section_lines(listing: str, symbol: str) -> list[dict[str, Any]]:
    start = re.search(rf"^[0-9A-F]{{8}}\s+{re.escape(symbol)}:\s*$", listing, re.MULTILINE)
    if start is None:
        raise ValueError(f"map-script UI H1 function is missing: {symbol}")
    end = listing.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"map-script UI H1 function end is missing: {symbol}")
    rows: list[dict[str, Any]] = []
    for raw in listing[start.start() : end].splitlines():
        match = re.fullmatch(r"(?P<address>[0-9A-F]{8})\s+(?P<body>.*)", raw)
        if match is None:
            continue
        body = match["body"].split(";", 1)[0].strip()
        body = re.sub(r"^(?:[0-9A-F]{2,8}\s+)+", "", body).strip()
        if not body or body.endswith(":"):
            continue
        if re.fullmatch(r"[A-Za-z][A-Za-z0-9_]*(?:\.[bwls])?(?:\s+.+)?", body) is None:
            raise ValueError(f"map-script UI H1 instruction parse drift: {raw}")
        rows.append(
            {"address": int(match["address"], 16), "instruction": re.sub(r"\s+", " ", body)}
        )
    return rows


def _instruction_width(instruction: str) -> int:
    match = re.fullmatch(r"[a-z]+\.(?P<size>[bwl])\s+.+", instruction)
    if match is None:
        raise ValueError(f"map-script UI transfer width is missing: {instruction}")
    return {"b": 1, "w": 2, "l": 4}[match["size"]]


def _unsigned_instruction_immediate(value: int, width_bytes: int) -> int:
    """Represent a parsed immediate in the exact width declared at its use site."""
    return value % (1 << (width_bytes * 8))


def _parse_equate(source: str, name: str) -> int:
    match = re.search(
        rf"^{re.escape(name)}:\s+equ\s+(?P<value>\$[0-9A-Fa-f]+|%[01]+|-?\d+)\b",
        source,
        re.MULTILINE,
    )
    if match is None:
        raise ValueError(f"map-script UI source equate is missing: {name}")
    return _literal(match["value"])


def _closed_rows(value: object, required: set[str], *, name: str) -> list[dict[str, Any]]:
    if not isinstance(value, list) or not all(isinstance(row, dict) for row in value):
        raise ValueError(f"map-script UI {name} container drift")
    if any(set(row) != required for row in value):
        raise ValueError(f"map-script UI {name} record shape drift")
    return list(value)


def _callback_plan(
    *,
    handler: str,
    direct_calls: list[dict[str, Any]],
    listing_rows: list[dict[str, Any]],
    target_resolutions: dict[str, dict[str, Any]],
    addresses: dict[str, int],
) -> list[dict[str, Any]]:
    plan = []
    for call in direct_calls:
        target = call["instructionTarget"]
        matches = [
            index
            for index, row in enumerate(listing_rows)
            if re.sub(r"\s+", "", row["instruction"])
            == re.sub(r"\s+", "", call["opcode"] + " " + target)
        ]
        # Parenthesized absolute-short calls have one source spelling and a
        # distinct H1 spelling; locate the source instruction identity instead.
        if len(matches) != 1:
            matches = [
                index
                for index, row in enumerate(listing_rows)
                if target in row["instruction"]
                and re.match(rf"^{re.escape(call['opcode'])}\b", row["instruction"])
            ]
        if len(matches) != 1 or matches[0] + 1 >= len(listing_rows):
            raise ValueError(f"map-script UI callback H1 identity drift: {handler}/{target}")
        resolution = target_resolutions.get(target)
        if resolution is None or resolution["effectiveTarget"] not in addresses:
            raise ValueError(f"map-script UI callback resolution drift: {handler}/{target}")
        row = listing_rows[matches[0]]
        plan.append(
            {
                "instructionTarget": target,
                "effectiveTarget": resolution["effectiveTarget"],
                "callSiteAddress": row["address"],
                "returnAddress": listing_rows[matches[0] + 1]["address"],
                "instructionTargetAddress": addresses[target],
                "effectiveTargetAddress": addresses[resolution["effectiveTarget"]],
            }
        )
    return plan


def _handler_record(
    row: dict[str, Any],
    source: str,
    listing: str,
    target_resolutions: dict[str, dict[str, Any]],
    addresses: dict[str, int],
) -> dict[str, Any]:
    handler = row["handler"]
    guard = row["sectionGuard"]
    required_guard = {
        "orderedInstructions",
        "scriptCursorReadUseSites",
        "sourceImmediateUseSites",
        "sourceOperandInstructions",
        "stackPointerTransferInstructions",
        "branchRecords",
        "loopRecords",
        "directCallOrder",
        "returnInstruction",
    }
    if set(guard) != required_guard:
        raise ValueError(f"map-script UI H2 guard shape drift: {handler}")
    source_rows = _source_section(source, handler)
    if (
        tuple(source_rows) != CONTROL_SECTIONS[handler]
        or source_rows != guard["orderedInstructions"]
    ):
        raise ValueError(f"map-script UI control-section source guard drift: {handler}")
    listing_rows = _listing_section_lines(listing, handler)
    if [re.sub(r"\s+", "", item["instruction"]) for item in listing_rows] != [
        re.sub(r"\s+", "", item) for item in source_rows
    ]:
        raise ValueError(f"map-script UI H1/source instruction order drift: {handler}")
    cursor_rows = _closed_rows(
        guard["scriptCursorReadUseSites"],
        {
            "sourceRegister",
            "destinationOperand",
            "transferredByteCount",
            "cursorAdvanceByteCount",
            "instruction",
        },
        name=f"{handler} cursor reads",
    )
    for cursor in cursor_rows:
        width = _instruction_width(cursor["instruction"])
        if (
            cursor["sourceRegister"] != "a6"
            or cursor["transferredByteCount"] != width
            or cursor["cursorAdvanceByteCount"] != width
        ):
            raise ValueError(f"map-script UI cursor source use-site drift: {handler}")
    direct_calls = _closed_rows(
        row["directCalls"],
        {"opcode", "instructionTarget", "addressingForm"},
        name=f"{handler} calls",
    )
    guarded_calls = []
    for instruction in guard["directCallOrder"]:
        match = re.fullmatch(
            r"(?P<opcode>jsr|bsr)(?:\.[bwls])?\s+(?:\()?(?P<target>[A-Za-z_][A-Za-z0-9_]*)(?:\)\.[bwls])?",
            instruction,
        )
        if match is None:
            raise ValueError(f"map-script UI direct-call syntax drift: {handler}/{instruction}")
        guarded_calls.append((match["opcode"], match["target"]))
    if [(call["opcode"], call["instructionTarget"]) for call in direct_calls] != guarded_calls:
        raise ValueError(f"map-script UI direct-call order drift: {handler}")
    callbacks = _callback_plan(
        handler=handler,
        direct_calls=direct_calls,
        listing_rows=listing_rows,
        target_resolutions=target_resolutions,
        addresses=addresses,
    )
    cursor_matches: list[int] = []
    if cursor_rows:
        cursor_matches = [
            index
            for index, item in enumerate(listing_rows)
            if item["instruction"] == cursor_rows[0]["instruction"]
        ]
        if len(cursor_matches) != 1 or cursor_matches[0] + 1 >= len(listing_rows):
            raise ValueError(f"map-script UI first cursor H1 use-site drift: {handler}")
    sentinel_matches = []
    for item in listing_rows:
        match = re.fullmatch(
            r"cmpi\.(?P<size>[bwl]) #(?P<literal>\$[0-9A-Fa-f]+|%[01]+|-?\d+),d1",
            item["instruction"],
        )
        if match is None:
            continue
        width_bytes = {"b": 1, "w": 2, "l": 4}[match["size"]]
        literal = _literal(match["literal"])
        sentinel_matches.append(
            {
                "address": item["address"],
                "instruction": item["instruction"],
                "widthBytes": width_bytes,
                "parsedImmediate": literal,
                "unsignedValue": _unsigned_instruction_immediate(literal, width_bytes),
            }
        )
    if len(sentinel_matches) > 1:
        raise ValueError(f"map-script UI sentinel compare H1 identity drift: {handler}")
    if listing_rows[-1]["instruction"] != guard["returnInstruction"]:
        raise ValueError(f"map-script UI handler return H1 identity drift: {handler}")
    return {
        "macro": row["macro"],
        "handler": handler,
        "handlerAddress": row["address"],
        "opcode": row["opcode"],
        "cursorUseSites": cursor_rows,
        "firstCursorReadAddress": listing_rows[cursor_matches[0]]["address"]
        if cursor_matches
        else None,
        "firstOperandFollowupAddress": (
            listing_rows[cursor_matches[0] + 1]["address"] if cursor_matches else None
        ),
        "sentinelCompareUseSite": sentinel_matches[0] if sentinel_matches else None,
        "branchRecords": guard["branchRecords"],
        "stackPointerTransferInstructions": guard["stackPointerTransferInstructions"],
        "callbacks": callbacks,
        "returnInstruction": guard["returnInstruction"],
        "returnInstructionAddress": listing_rows[-1]["address"],
    }


def _source_input_rows(facts: dict[str, Any], compact: dict[str, Any]) -> list[dict[str, Any]]:
    source_sites = _closed_rows(
        facts["sourceSites"], {"programId", "commands"}, name="source sites"
    )
    flattened = [command for site in source_sites for command in site["commands"]]
    if [command["sourceOrderKey"] for command in flattened] != compact["sourceSiteOrderKeys"]:
        raise ValueError("map-script UI compact/full source-order relation drift")
    full_source_sha256 = hashlib.sha256(
        _canonical_bytes({"sourceSites": facts["sourceSites"]})
    ).hexdigest().upper()
    if (
        full_source_sha256 != facts["sourceSitesSha256"]
        or full_source_sha256 != compact["sourceSitesSha256"]
    ):
        raise ValueError("map-script UI compact/full source hash relation drift")
    rows = []
    for site in source_sites:
        for command in site["commands"]:
            if command["macro"] != "showPortrait":
                continue
            if set(command) != {
                "commandIndex",
                "sourceLine",
                "macro",
                "arguments",
                "sourceOrderKey",
                "operandValues",
            }:
                raise ValueError("map-script UI show source command shape drift")
            operands = _closed_rows(
                command["operandValues"],
                {
                    "parameterOrdinal",
                    "sourceComment",
                    "streamOffset",
                    "widthBytes",
                    "encoding",
                    "rawValue",
                    "resolvedValue",
                    "resolution",
                },
                name="show source operands",
            )
            if [item["streamOffset"] for item in operands] != [2, 3] or [
                item["widthBytes"] for item in operands
            ] != [1, 1]:
                raise ValueError("map-script UI show source operand layout drift")
            if any(not 0 <= item["resolvedValue"] <= 0xFF for item in operands):
                raise ValueError("map-script UI show source operand byte boundary drift")
            rows.append(
                {
                    "programId": site["programId"],
                    **command,
                    "handlerInputWord": (operands[0]["resolvedValue"] << 8)
                    | operands[1]["resolvedValue"],
                }
            )
    if len(rows) != 4 or [row["sourceOrderKey"] for row in rows] != [
        key for key in compact["sourceSiteOrderKeys"] if key.endswith(":showPortrait")
    ]:
        raise ValueError("map-script UI complete show source-row inventory drift")
    return rows


def _menu_selector_dispatch(handler: dict[str, Any]) -> list[dict[str, Any]]:
    calls = handler["callbacks"]
    immediates = handler["branchRecords"]
    if [row["instructionTarget"] for row in calls] != [
        "j_ChurchMenu",
        "j_ShopMenu",
        "j_BlacksmithMenu",
    ] or [row["branchInstruction"] for row in immediates] != [
        "bne.s loc_474C4",
        "bne.s loc_474D0",
        "bne.s loc_474DC",
    ]:
        raise ValueError("map-script UI menu selector call/branch order drift")
    return [
        {"selectorWord": 0, "callback": calls[0]},
        {"selectorWord": 1, "callback": calls[1]},
        {"selectorWord": 2, "callback": calls[2]},
    ]


def build_map_script_ui_primary_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    """Parse the complete bounded handler surface before runtime comparison."""
    h2_fixture = load_json(H2_FIXTURE)
    h2_output = build_map_script_engine_contract(rom_path, upstream_path)
    compact = h2_fixture["expected"]["mapScriptUiPrimaryCommandFacts"]
    facts = h2_output["mapScriptUiPrimaryCommandFacts"]
    if {key: facts[key] for key in compact} != compact:
        raise ValueError("map-script UI H2 compact fixture/source drift")
    if facts["runtimeQuestions"] != RUNTIME_QUESTIONS:
        raise ValueError("map-script UI H2 runtime-question handoff drift")
    upstream = upstream_path.resolve(strict=True)
    listing = (upstream / H1_LISTING_PATH).read_text(encoding="utf-8")
    addresses = listing_symbol_addresses(listing)
    required_symbols = {
        "RunMapSetupInitFunction",
        "WaitForViewScrollEnd",
        "GetEntityPortaitAndSpeechSfx",
        "j_OpenPortraitWindow",
        "j_ClosePortraitWindow",
        "j_ChurchMenu",
        "j_ShopMenu",
        "j_BlacksmithMenu",
        "OpenPortraitWindow",
        "ClosePortraitWindow",
        "ChurchMenu",
        "ShopMenu",
        "BlacksmithMenu",
        *(handler for _, handler, _ in HANDLER_FORMS),
    }
    if not required_symbols <= addresses.keys():
        raise ValueError("map-script UI H1 symbol inventory drift")
    h2_handlers = _closed_rows(
        facts["handlers"],
        {
            "macro",
            "handler",
            "sourcePath",
            "address",
            "opcode",
            "sourceCommandCount",
            "operandAnnotations",
            "statementCount",
            "sectionGuard",
            "directCalls",
        },
        name="H2 handlers",
    )
    if [(row["macro"], row["handler"], row["sourcePath"]) for row in h2_handlers] != list(
        HANDLER_FORMS
    ):
        raise ValueError("map-script UI H2 handler identity/order drift")
    resolutions = _closed_rows(
        facts["callerBreakdown"]["targetResolutions"],
        {"instructionTarget", "effectiveTarget", "aliasSourcePath", "effectiveTargetScope"},
        name="target resolutions",
    )
    resolution_map = {row["instructionTarget"]: row for row in resolutions}
    if len(resolution_map) != len(resolutions) or any(
        row["effectiveTargetScope"] != "external" for row in resolutions
    ):
        raise ValueError("map-script UI effective target inventory drift")
    handlers = []
    for row in h2_handlers:
        source = (upstream / "disasm" / row["sourcePath"]).read_text(encoding="utf-8")
        record = _handler_record(row, source, listing, resolution_map, addresses)
        if record["handlerAddress"] != addresses[record["handler"]]:
            raise ValueError(f"map-script UI handler H1 address drift: {record['handler']}")
        handlers.append(record)
    show = handlers[0]
    menu = handlers[2]
    show_cursor = show["cursorUseSites"]
    menu_cursor = menu["cursorUseSites"]
    sentinel = show["sentinelCompareUseSite"]
    if len(show_cursor) != 1 or len(menu_cursor) != 1:
        raise ValueError("map-script UI command cursor inventory drift")
    if sentinel is None:
        raise ValueError("map-script UI sentinel compare source use-site drift")
    stack = menu["stackPointerTransferInstructions"]
    if stack != ["move.l a6,-(sp)", "movea.l (sp)+,a6"]:
        raise ValueError("map-script UI menu stack restore source guard drift")
    return {
        "provenance": {
            "upstreamRepository": "https://github.com/ShiningForceCentral/SF2DISASM",
            "upstreamBranch": "master",
            "upstreamCommit": h2_fixture["upstreamCommit"],
            "h2FixturePath": "tests/fixtures/h2/map-script-engine-static-v1.json",
            "h2FixtureId": h2_fixture["id"],
            "h2FieldPath": "expected.mapScriptUiPrimaryCommandFacts",
            "command": "uv run sf2 h2 map-script-engine",
        },
        "romSha256": h2_fixture["romSha256"],
        "function": {
            "entryAddress": addresses["RunMapSetupInitFunction"],
            **{f"{row['handler']}Address": row["handlerAddress"] for row in handlers},
            **{
                f"{row['handler']}ReturnInstructionAddress": row["returnInstructionAddress"]
                for row in handlers
            },
            "showPortraitFirstOperandFollowupAddress": show["firstOperandFollowupAddress"],
            "showPortraitSentinelCompareAddress": sentinel["address"],
            "menuFirstOperandFollowupAddress": menu["firstOperandFollowupAddress"],
        },
        "ram": {
            "portraitWindowIndexAddress": _parse_equate(
                (upstream / CONSTANTS_PATH).read_text(encoding="utf-8"), "PORTRAIT_WINDOW_INDEX"
            )
        },
        "constants": {
            "showPortraitCursorAdvanceByteCount": show_cursor[0]["cursorAdvanceByteCount"],
            "menuCursorAdvanceByteCount": menu_cursor[0]["cursorAdvanceByteCount"],
            "menuSavedA6StackByteCount": _instruction_width(stack[0]),
            "menuRestoredA6StackByteCount": _instruction_width(stack[1]),
            "signedWordSentinel": sentinel["unsignedValue"],
        },
        "sourceFacts": {
            "macroForms": facts["macros"],
            "compactSourceBoundary": {
                "sourceSiteOrderKeys": compact["sourceSiteOrderKeys"],
                "sourceSitesSha256": compact["sourceSitesSha256"],
                "showSourceOrderKeys": [
                    key for key in compact["sourceSiteOrderKeys"] if key.endswith(":showPortrait")
                ],
            },
            "sourceInputRows": _source_input_rows(facts, compact),
            "handlers": handlers,
            "callerBreakdown": facts["callerBreakdown"],
            "menuSelectorDispatch": _menu_selector_dispatch(menu),
            "evidenceLabels": {"staticFindings": "Confirmed", "runtimeObservations": "Confirmed"},
        },
        "runtimeQuestions": facts["runtimeQuestions"],
    }


def _handler(static: dict[str, Any], macro: str) -> dict[str, Any]:
    matches = [row for row in static["sourceFacts"]["handlers"] if row["macro"] == macro]
    if len(matches) != 1:
        raise ValueError(f"map-script UI handler seam drift: {macro}")
    return matches[0]


def _observed_callback_shape(plan: list[dict[str, Any]]) -> list[dict[str, Any]]:
    return [
        {
            "instructionTarget": row["instructionTarget"],
            "effectiveTarget": row["effectiveTarget"],
            "callSiteAddress": row["callSiteAddress"],
            "returnAddress": row["returnAddress"],
        }
        for row in plan
    ]


def _case_expected(
    case: dict[str, Any], static: dict[str, Any], *, script_input_ram_offset: int
) -> dict[str, Any]:
    kind = case["kind"]
    if kind.startswith("show"):
        handler = _handler(static, "showPortrait")
        word = case["handlerInputWord"]
        if not isinstance(word, int) or not 0 <= word <= 0xFFFF:
            raise ValueError(f"map-script UI show input-word boundary drift: {case['id']}")
        busy = case["portraitWindowIndexWordSeed"] != 0
        sentinel = case["helperD1Word"] == static["constants"]["signedWordSentinel"]
        if busy:
            plan = []
        elif sentinel:
            plan = handler["callbacks"][:2]
        else:
            plan = handler["callbacks"]
        result = {
            "id": case["id"],
            "kind": kind,
            "handlerAddress": handler["handlerAddress"],
            "handlerInputWord": word,
            "scriptCursorRamOffsetAfter": script_input_ram_offset
            + handler["cursorUseSites"][0]["cursorAdvanceByteCount"],
            "stackPointerDeltaBytesAfter": 0,
            "a6RestoredFromStack": False,
            "directCallbackPlan": _observed_callback_shape(plan),
            "sourceInput": case["sourceInput"],
        }
    elif kind == "hide":
        handler = _handler(static, "hidePortrait")
        result = {
            "id": case["id"],
            "kind": kind,
            "handlerAddress": handler["handlerAddress"],
            "handlerInputWord": None,
            "scriptCursorRamOffsetAfter": script_input_ram_offset,
            "stackPointerDeltaBytesAfter": 0,
            "a6RestoredFromStack": False,
            "directCallbackPlan": _observed_callback_shape(handler["callbacks"]),
            "sourceInput": None,
        }
    elif kind == "menu":
        handler = _handler(static, "menu")
        selector = case["handlerInputWord"]
        if not isinstance(selector, int) or not 0 <= selector <= 0xFFFF:
            raise ValueError(f"map-script UI menu selector boundary drift: {case['id']}")
        matches = [
            row["callback"]
            for row in static["sourceFacts"]["menuSelectorDispatch"]
            if row["selectorWord"] == selector
        ]
        plan = matches if matches else []
        result = {
            "id": case["id"],
            "kind": kind,
            "handlerAddress": handler["handlerAddress"],
            "handlerInputWord": selector,
            "scriptCursorRamOffsetAfter": script_input_ram_offset
            + handler["cursorUseSites"][0]["cursorAdvanceByteCount"],
            "stackPointerDeltaBytesAfter": 0,
            "a6RestoredFromStack": True,
            "directCallbackPlan": _observed_callback_shape(plan),
            "sourceInput": None,
        }
    else:
        raise ValueError(f"map-script UI case kind is unknown: {kind}")
    if case["expected"] is not None and case["expected"] != result:
        raise ValueError(f"map-script UI fixture/static disagreement: {case['id']}")
    return result


def derive_case_expectations(
    static: dict[str, Any], fixture: dict[str, Any]
) -> list[dict[str, Any]]:
    """Derive all static facts from parsed handlers before fixture comparison."""
    if fixture["sourceContract"] != static["sourceFacts"]["compactSourceBoundary"]:
        raise ValueError("map-script UI fixture/source compact-boundary drift")
    rows = static["sourceFacts"]["sourceInputRows"]
    source_cases = [case for case in fixture["cases"] if case["kind"] == "show-source"]
    if [case["sourceInput"] for case in source_cases] != rows:
        raise ValueError("map-script UI fixture/source input provenance drift")
    script_input_ram_offset = fixture["instrumentation"].get("scriptInputRamOffset")
    if not isinstance(script_input_ram_offset, int) or script_input_ram_offset < 0:
        raise ValueError("map-script UI script-input RAM offset drift")
    derived = [
        _case_expected(case, static, script_input_ram_offset=script_input_ram_offset)
        for case in fixture["cases"]
    ]
    if (
        all(case["expected"] is not None for case in fixture["cases"])
        and [case["expected"] for case in fixture["cases"]] != derived
    ):
        raise ValueError("map-script UI complete fixture/static disagreement")
    return derived


def _observer_cases(fixture: dict[str, Any]) -> list[dict[str, Any]]:
    """Pass only inputs to Lua; runtime goldens never become observer configuration."""
    fields = {
        "id",
        "kind",
        "portraitWindowIndexWordSeed",
        "handlerInputWord",
        "helperD1Word",
    }
    return [{key: case[key] for key in fields} for case in fixture["cases"]]


def _service_interception(static: dict[str, Any], fixture: dict[str, Any]) -> list[dict[str, Any]]:
    """Validate session-only shims against parsed callback identities and targets."""
    instrumentation = fixture["instrumentation"]
    interception = instrumentation["serviceInterception"]
    required_interception = {"helperD1SeedRamOffset", "patches"}
    if set(interception) != required_interception:
        raise ValueError("map-script UI service-interception shape drift")
    seed_offset = interception["helperD1SeedRamOffset"]
    if not isinstance(seed_offset, int) or seed_offset < 0:
        raise ValueError("map-script UI helper D1 seed offset drift")
    patch_rows = _closed_rows(
        interception["patches"],
        {"targetIdentity", "targetRole", "address", "originalHex", "patchedHex"},
        name="service interception patches",
    )
    callbacks = [
        callback
        for handler in static["sourceFacts"]["handlers"]
        for callback in handler["callbacks"]
    ]
    expected_targets: dict[tuple[str, str], int] = {}
    for callback in callbacks:
        expected_targets[(callback["instructionTarget"], "instruction")] = callback[
            "instructionTargetAddress"
        ]
        expected_targets[(callback["effectiveTarget"], "effective")] = callback[
            "effectiveTargetAddress"
        ]
    helper_seed_address = instrumentation["ramInputAddress"] + seed_offset
    if not 0xFF0000 <= helper_seed_address <= 0xFFFFFF:
        raise ValueError("map-script UI helper D1 seed RAM address drift")
    expected_patches = {
        ("WaitForViewScrollEnd", "effective"): bytes.fromhex("4E75"),
        ("GetEntityPortaitAndSpeechSfx", "effective"): (
            b"\x32\x39" + helper_seed_address.to_bytes(4, "big") + b"\x4e\x75"
        ),
        ("j_OpenPortraitWindow", "instruction"): bytes.fromhex("4E75"),
        ("j_ClosePortraitWindow", "instruction"): bytes.fromhex("4E75"),
        ("j_ChurchMenu", "instruction"): bytes.fromhex("4E75"),
        ("j_ShopMenu", "instruction"): bytes.fromhex("4E75"),
        ("j_BlacksmithMenu", "instruction"): bytes.fromhex("4E75"),
    }
    parsed_patches: dict[tuple[str, str], dict[str, Any]] = {}
    for patch in patch_rows:
        key = (patch["targetIdentity"], patch["targetRole"])
        if key in parsed_patches or key not in expected_patches:
            raise ValueError("map-script UI service-interception target inventory drift")
        if expected_targets.get(key) != patch["address"]:
            raise ValueError("map-script UI service-interception source target drift")
        original = bytes.fromhex(patch["originalHex"])
        patched = bytes.fromhex(patch["patchedHex"])
        if not original or len(original) != len(patched) or patched != expected_patches[key]:
            raise ValueError("map-script UI service-interception byte-shape drift")
        parsed_patches[key] = {**patch, "originalBytes": original, "patchedBytes": patched}
    if set(parsed_patches) != set(expected_patches):
        raise ValueError("map-script UI complete service-interception inventory drift")
    return [
        parsed_patches[key]
        for key in (
            ("WaitForViewScrollEnd", "effective"),
            ("GetEntityPortaitAndSpeechSfx", "effective"),
            ("j_OpenPortraitWindow", "instruction"),
            ("j_ClosePortraitWindow", "instruction"),
            ("j_ChurchMenu", "instruction"),
            ("j_ShopMenu", "instruction"),
            ("j_BlacksmithMenu", "instruction"),
        )
    ]


def _instrument_ui_rom(rom_path: Path, fixture: dict[str, Any], static: dict[str, Any]) -> Path:
    """Extend the generic trampoline with bounded service-return shims."""
    patches = _service_interception(static, fixture)
    base_rom = _instrument_rom(rom_path, fixture)
    source = rom_path.read_bytes()
    data = bytearray(base_rom.read_bytes())
    for patch in patches:
        address = patch["address"]
        original = patch["originalBytes"]
        if source[address : address + len(original)] != original:
            raise ValueError(
                "map-script UI service-interception original ROM bytes drift: "
                f"{patch['targetIdentity']}"
            )
        if data[address : address + len(original)] != original:
            raise ValueError(
                f"map-script UI service-interception generic-ROM overlap: {patch['targetIdentity']}"
            )
        data[address : address + len(original)] = patch["patchedBytes"]
    data[0x18E:0x190] = int(mega_drive_checksum(bytes(data)), 16).to_bytes(2, "big")
    DERIVED_ROOT.mkdir(parents=True, exist_ok=True)
    output = DERIVED_ROOT / "map-script-ui-primary.instrumented.bin"
    output.write_bytes(data)
    return output


def verify_map_script_ui_primary(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 180
) -> dict[str, Any]:
    """Run the eleven handler-local cases in one session-only Map Test launch."""
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner="map-script UI primary fixture")
    verify_runtime_contract(fixture, rom_path)
    static = build_map_script_ui_primary_contract(rom_path, upstream_path)
    for field in ("provenance", "romSha256", "function", "ram", "constants", "runtimeQuestions"):
        if fixture[field] != static[field]:
            raise ValueError(f"map-script UI fixture/source identity drift: {field}")
    derived = derive_case_expectations(static, fixture)
    instrumented_rom = _instrument_ui_rom(rom_path, fixture, static)

    def observe() -> dict[str, Any]:
        return run_observer(
            rom_path=instrumented_rom,
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
                "cases": _observer_cases(fixture),
                "derived": derived,
            },
            output_name="map-script-ui-primary",
            timeout_seconds=timeout_seconds,
        )

    observed = _with_instrumented_rom_database(
        instrumented_rom, "SF2 H3 instrumented map-script UI primary", observe
    )
    validate_json(observed, OBSERVATION_SCHEMA, owner="map-script UI primary observation")
    expected = {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["id"],
        "mapTest": fixture["mapTestIndex"],
        "recordOrder": [case["id"] for case in fixture["cases"]],
        "records": [{**case["expected"], **case["runtimeGolden"]} for case in fixture["cases"]],
    }
    if observed != expected:
        raise ValueError(
            "map-script UI primary runtime matrix mismatch\n"
            f"static={derived!r}\nexpected={expected!r}\nobserved={observed!r}"
        )
    return {
        "Fixture": fixture["id"],
        "Cases": len(derived),
        "Handlers": len({case["handlerAddress"] for case in derived}),
        "BizHawkLaunches": 1,
        "Instrumentation": "session-only handler-local service-call interception",
        "Status": "PASS",
    }
