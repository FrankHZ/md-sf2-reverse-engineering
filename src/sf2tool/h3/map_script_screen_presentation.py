"""One-launch, handler-local runtime contract for map-script presentation commands.

The observer invokes only the twelve H2-bounded handlers through the shared Map
Test trampoline.  Its five external services are session-only entry shims, so
the facts below concern cursor movement, handler-local branches, direct writes,
register payloads, and call/return chronology rather than presentation output.
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
FIXTURE = repo_path("tests/fixtures/h3/map-script-screen-presentation-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h3-map-script-screen-presentation-fixture.schema.json")
OBSERVATION_SCHEMA = repo_path("schemas/h3-map-script-screen-presentation-observation.schema.json")
OBSERVER = repo_path("tools/bizhawk/map_script_screen_presentation_observer.lua")
SOURCE_PATH = Path("disasm/code/common/scripting/map/mapscriptengine_1.asm")
H1_LISTING_PATH = Path("build/sf2build-h1.lst")

HANDLER_FORMS = (
    ("setQuake", "csc33_setQuakeAmount"),
    ("fadeInB", "csc39_fadeInFromBlack"),
    ("fadeOutB", "csc3A_fadeOutToBlack"),
    ("slowFadeInB", "csc3B_slowFadeInFromBlack"),
    ("slowFadeOutB", "csc3C_slowFadeOutToBlack"),
    ("tintMap", "csc3D_tintMap"),
    ("flickerOnce", "csc3E_FlickerOnce"),
    ("mapFadeOutToWhite", "csc3F_fadeMapOutToWhite"),
    ("mapFadeInFromWhite", "csc40_fadeMapInFromWhite"),
    ("flashScreenWhite", "csc41_flashScreenWhite"),
    ("fadeInFromBlackHalf", "csc4A_fadeInFromBlackHalf"),
    ("fadeOutToBlackHalf", "csc4B_fadeOutToBlackHalf"),
)
TARGET_IDENTITIES = (
    "Sleep",
    "FadeInFromBlack",
    "FadeOutToBlack",
    "LaunchFading",
    "DuplicatePalettes",
)
RUNTIME_QUESTIONS = [
    "map-script-screen-presentation/normal-story-reachability",
    "map-script-screen-presentation/visible-palette-vdp-and-frame-timing",
    "map-script-screen-presentation/service-body-completion-repeat-and-persistence",
    "map-script-screen-presentation/map-and-entity-state-interactions",
]
WORD_MASK = 0xFFFF


def _literal(text: str) -> int:
    if re.fullmatch(r"\$[0-9A-Fa-f]+", text):
        return int(text[1:], 16)
    if re.fullmatch(r"%[01]+", text):
        return int(text[1:], 2)
    if re.fullmatch(r"-?\d+", text):
        return int(text)
    raise ValueError(f"screen-presentation literal is not numeric: {text}")


def _closed_rows(value: object, required: set[str], *, name: str) -> list[dict[str, Any]]:
    if not isinstance(value, list) or not all(isinstance(row, dict) for row in value):
        raise ValueError(f"screen-presentation {name} container drift")
    if any(set(row) != required for row in value):
        raise ValueError(f"screen-presentation {name} shape drift")
    return list(value)


def _source_section(source: str, symbol: str) -> list[dict[str, Any]]:
    start = re.search(rf"^{re.escape(symbol)}:\s*$", source, re.MULTILINE)
    if start is None:
        raise ValueError(f"screen-presentation source section is missing: {symbol}")
    marker = f"; End of function {symbol}"
    end = source.find(marker, start.end())
    if end < 0:
        raise ValueError(f"screen-presentation source section end is missing: {symbol}")
    first_line = source[: start.start()].count("\n")
    rows: list[dict[str, Any]] = []
    for offset, raw in enumerate(source[start.start() : end].splitlines(), 1):
        instruction = re.sub(r"\s+", " ", raw.split(";", 1)[0].strip())
        if instruction and not instruction.endswith(":"):
            rows.append({"instruction": instruction, "sourceLine": first_line + offset})
    return rows


def _h1_section(listing: str, symbol: str) -> list[dict[str, Any]]:
    start = re.search(rf"^[0-9A-F]{{8}}\s+{re.escape(symbol)}:\s*$", listing, re.MULTILINE)
    if start is None:
        raise ValueError(f"screen-presentation H1 section is missing: {symbol}")
    end = listing.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"screen-presentation H1 section end is missing: {symbol}")
    rows: list[dict[str, Any]] = []
    for raw in listing[start.start() : end].splitlines():
        match = re.fullmatch(r"(?P<address>[0-9A-F]{8})\s+(?P<body>.*)", raw)
        if match is None:
            continue
        body = re.sub(r"^(?:[0-9A-F]{2,8}\s+)+", "", match["body"].split(";", 1)[0].strip())
        if body and not body.endswith(":"):
            rows.append(
                {"address": int(match["address"], 16), "instruction": re.sub(r"\s+", " ", body)}
            )
    return rows


def _assert_identity(
    source_rows: list[dict[str, Any]], h1_rows: list[dict[str, Any]], name: str
) -> None:
    source = [re.sub(r"\s+", "", row["instruction"]) for row in source_rows]
    h1 = [re.sub(r"\s+", "", row["instruction"]) for row in h1_rows]
    if source != h1:
        raise ValueError(f"screen-presentation H1/source instruction identity drift: {name}")


def _parse_equates(upstream: Path, names: set[str]) -> dict[str, int]:
    values: dict[str, int] = {}
    for relative in ("disasm/sf2const.asm", "disasm/sf2enums.asm"):
        source = (upstream / relative).read_text(encoding="utf-8")
        for match in re.finditer(
            r"^(?P<name>[A-Za-z_][A-Za-z0-9_]*):\s+equ\s+"
            r"(?P<value>\$[0-9A-Fa-f]+|%[01]+|-?\d+)\b",
            source,
            re.MULTILINE,
        ):
            if match["name"] in names:
                value = _literal(match["value"])
                old = values.setdefault(match["name"], value)
                if old != value:
                    raise ValueError(f"screen-presentation constant conflict: {match['name']}")
    if names - values.keys():
        raise ValueError(f"screen-presentation constants missing: {sorted(names - values.keys())}")
    return {name: values[name] for name in sorted(names)}


def _calls(
    rows: list[dict[str, Any]], h1_rows: list[dict[str, Any]], addresses: dict[str, int]
) -> list[dict[str, Any]]:
    parsed: list[dict[str, Any]] = []
    for index, row in enumerate(rows):
        match = re.fullmatch(
            r"jsr(?:\.[bwls])?\s+(?P<target>\([A-Za-z_][A-Za-z0-9_]*\)|[A-Za-z_][A-Za-z0-9_]*\(pc\))(?:\.[bwl])?",
            row["instruction"],
        )
        if match is None:
            continue
        raw_target = match["target"]
        target = raw_target[1:-1] if raw_target.startswith("(") else raw_target.removesuffix("(pc)")
        if target not in TARGET_IDENTITIES or target not in addresses:
            raise ValueError(f"screen-presentation direct call target drift: {target}")
        h1 = h1_rows[index]
        if index + 1 >= len(h1_rows):
            raise ValueError("screen-presentation direct-call return is missing")
        parsed.append(
            {
                "instructionTarget": target,
                "effectiveTarget": target,
                "targetRole": "effective",
                "callSiteAddress": h1["address"],
                "targetAddress": addresses[target],
                "returnAddress": h1_rows[index + 1]["address"],
                "addressingForm": "pc-relative" if raw_target.endswith("(pc)") else "direct",
            }
        )
    return parsed


def _source_inputs(facts: dict[str, Any]) -> list[dict[str, Any]]:
    rows = _closed_rows(facts["sourceSites"], {"programId", "commands"}, name="source sites")
    digest = (
        hashlib.sha256(_canonical_bytes({"sourceSites": facts["sourceSites"]})).hexdigest().upper()
    )
    if digest != facts["sourceSitesSha256"]:
        raise ValueError("screen-presentation complete H2 source-site hash drift")
    result = []
    for site in rows:
        for command in site["commands"]:
            expected = {
                "commandIndex",
                "sourceLine",
                "macro",
                "arguments",
                "sourceOrderKey",
                "operandValues",
            }
            if set(command) != expected or command["macro"] not in dict(HANDLER_FORMS):
                continue
            operands = _closed_rows(
                command["operandValues"],
                {
                    "parameterOrdinal",
                    "sourceComment",
                    "streamOffset",
                    "widthBytes",
                    "rawValue",
                    "resolvedValue",
                    "resolution",
                },
                name="source operands",
            )
            if command["macro"] in {"setQuake", "flashScreenWhite"}:
                if (
                    len(operands) != 1
                    or operands[0]["widthBytes"] != 2
                    or operands[0]["resolvedValue"] is None
                ):
                    raise ValueError("screen-presentation word operand source shape drift")
                word = operands[0]["resolvedValue"]
                if not isinstance(word, int) or not 0 <= word <= WORD_MASK:
                    raise ValueError("screen-presentation word operand boundary drift")
            elif operands:
                raise ValueError("screen-presentation zero-operand source shape drift")
            result.append({"programId": site["programId"], **command})
    handler_counts = {row["macro"]: row["sourceCommandCount"] for row in facts["handlers"]}
    if {
        macro: sum(row["macro"] == macro for row in result) for macro, _ in HANDLER_FORMS
    } != handler_counts:
        raise ValueError("screen-presentation complete H2 macro-source inventory drift")
    return result


def _ram_access_plan(macro: str, constants: dict[str, int]) -> list[dict[str, Any]]:
    if macro == "setQuake":
        return [{"address": constants["QUAKE_AMPLITUDE"], "widthBytes": 2, "access": "write"}]
    if macro in {"slowFadeInB", "slowFadeOutB"}:
        return [
            {"address": constants["FADING_COUNTER_MAX"], "widthBytes": 1, "access": "read"},
            {"address": constants["FADING_COUNTER_MAX"], "widthBytes": 1, "access": "write"},
            {"address": constants["FADING_COUNTER_MAX"], "widthBytes": 1, "access": "write"},
        ]
    return []


def _use_site_relations(h2: dict[str, Any]) -> dict[str, Any]:
    """Bind runtime arithmetic to the guarded source immediate use sites."""
    sites = h2["sectionGuard"]["sourceImmediateUseSites"]

    def one(instruction: str) -> int:
        rows = [row for row in sites if row["instruction"] == instruction]
        if len(rows) != 1 or not isinstance(rows[0]["resolvedValue"], int):
            raise ValueError(f"screen-presentation source use-site drift: {instruction}")
        return rows[0]["resolvedValue"]

    macro = h2["macro"]
    if macro == "setQuake":
        return {
            "kind": "quake",
            "mask": one("andi.w #$3FFF,d0"),
            "firstBit": one("btst #$F,d3"),
            "secondBit": one("btst #$E,d3"),
            "positiveStep": one("move.w #1,d2"),
            "negativeStep": one("move.w #-1,d2"),
            "sleepD0": one("move.w #$28,d0"),
        }
    if macro == "flashScreenWhite":
        loop_records = h2["sectionGuard"]["loopRecords"]
        if len(loop_records) != 1:
            raise ValueError("screen-presentation flash loop-record count drift")
        loop = loop_records[0]
        target = loop["loopTarget"]
        if (
            loop["loopInstruction"] != "dbf d7,loc_4667A"
            or target["counterRegister"] != "d7"
            or target["targetInstruction"] != "jsr LaunchFading(pc)"
        ):
            raise ValueError("screen-presentation flash DBF control-flow drift")
        return {
            "kind": "flash",
            "shift": one("lsr.w #3,d7"),
            "loopCounterRegister": target["counterRegister"],
            "loopInstruction": loop["loopInstruction"],
            "loopTargetInstruction": target["targetInstruction"],
            "d0": one("moveq #$F,d0"),
            "d1": one("moveq #1,d1"),
            "d2": one("moveq #FLASH_QUICKLY_2,d2"),
        }
    register_sites: dict[str, int] = {}
    for row in sites:
        match = re.fullmatch(r"moveq #.+,(d[012])", row["instruction"])
        if match is not None:
            register_sites[match[1]] = row["resolvedValue"]
    if macro in {
        "tintMap",
        "flickerOnce",
        "mapFadeOutToWhite",
        "mapFadeInFromWhite",
        "fadeInFromBlackHalf",
        "fadeOutToBlackHalf",
    }:
        if set(register_sites) != {"d0", "d1", "d2"}:
            raise ValueError(f"screen-presentation register source-use inventory drift: {macro}")
        return {"kind": "preset", **register_sites}
    return {"kind": "none"}


def build_map_script_screen_presentation_contract(
    rom_path: Path, upstream_path: Path
) -> dict[str, Any]:
    """Parse the entire H2 source domain and every bounded handler before H3."""
    h2_fixture = load_json(H2_FIXTURE)
    facts = build_map_script_engine_contract(rom_path, upstream_path)[
        "screenPresentationCommandFacts"
    ]
    compact = h2_fixture["expected"]["screenPresentationCommandFacts"]
    if {key: facts[key] for key in compact} != compact:
        raise ValueError("screen-presentation H2 compact fixture/source drift")
    if facts["runtimeQuestions"] != RUNTIME_QUESTIONS:
        raise ValueError("screen-presentation H2 runtime-question handoff drift")
    upstream = upstream_path.resolve(strict=True)
    source = (upstream / SOURCE_PATH).read_text(encoding="utf-8")
    listing = (upstream / H1_LISTING_PATH).read_text(encoding="utf-8")
    addresses = listing_symbol_addresses(listing)
    constants = _parse_equates(
        upstream,
        {
            "QUAKE_AMPLITUDE",
            "FADING_COUNTER_MAX",
            "HALF_OUT_TO_BLACK",
            "FLICKER_ONCE",
            "OUT_TO_WHITE",
            "IN_FROM_WHITE",
            "FLASH_QUICKLY_2",
            "HALF_IN_FROM_BLACK",
            "OUT_TO_BLACK_2",
        },
    )
    h2_handlers = _closed_rows(
        facts["handlers"],
        {
            "macro",
            "handler",
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
    if [(row["macro"], row["handler"]) for row in h2_handlers] != list(HANDLER_FORMS):
        raise ValueError("screen-presentation H2 handler identity/order drift")
    handlers = []
    h1_by_macro: dict[str, list[dict[str, Any]]] = {}
    for h2 in h2_handlers:
        source_rows = _source_section(source, h2["handler"])
        h1_rows = _h1_section(listing, h2["handler"])
        _assert_identity(source_rows, h1_rows, h2["handler"])
        if len(source_rows) != h2["statementCount"] or h1_rows[0]["address"] != h2["address"]:
            raise ValueError(f"screen-presentation handler guard/address drift: {h2['handler']}")
        call_plan = _calls(source_rows, h1_rows, addresses)
        if [row["instructionTarget"] for row in call_plan] != [
            row["instructionTarget"] for row in h2["directCalls"]
        ]:
            raise ValueError(f"screen-presentation call identity/order drift: {h2['handler']}")
        handlers.append(
            {
                "macro": h2["macro"],
                "handler": h2["handler"],
                "handlerAddress": h2["address"],
                "returnAddress": h1_rows[-1]["address"],
                "cursorAdvanceByteCount": sum(
                    row["cursorAdvanceByteCount"]
                    for row in h2["sectionGuard"]["scriptCursorReadUseSites"]
                ),
                "directCallPlan": call_plan,
                "directRamAccessPlan": _ram_access_plan(h2["macro"], constants),
                "useSiteRelations": _use_site_relations(h2),
                "orderedInstructions": h2["sectionGuard"]["orderedInstructions"],
            }
        )
        h1_by_macro[h2["macro"]] = h1_rows
    if not {"RunMapSetupInitFunction", *TARGET_IDENTITIES} <= addresses.keys():
        raise ValueError("screen-presentation H1 service symbol inventory drift")
    return {
        "provenance": {
            "upstreamRepository": "https://github.com/ShiningForceCentral/SF2DISASM",
            "upstreamBranch": "master",
            "upstreamCommit": h2_fixture["upstreamCommit"],
            "h2FixturePath": "tests/fixtures/h2/map-script-engine-static-v1.json",
            "h2FixtureId": h2_fixture["id"],
            "h2FieldPath": "expected.screenPresentationCommandFacts",
            "command": "uv run sf2 h2 map-script-engine",
        },
        "romSha256": h2_fixture["romSha256"],
        "function": {
            "entryAddress": addresses["RunMapSetupInitFunction"],
            **{f"{row['handler']}Address": row["handlerAddress"] for row in handlers},
            **{f"{row['handler']}ReturnAddress": row["returnAddress"] for row in handlers},
            "quakeDirectWriteResumeAddress": h1_by_macro["setQuake"][16]["address"],
            "quakeLoopWriteResumeAddress": h1_by_macro["setQuake"][20]["address"],
            "slowFadeInReadResumeAddress": h1_by_macro["slowFadeInB"][1]["address"],
            "slowFadeInSetResumeAddress": h1_by_macro["slowFadeInB"][2]["address"],
            "slowFadeInRestoreResumeAddress": h1_by_macro["slowFadeInB"][4]["address"],
            "slowFadeOutReadResumeAddress": h1_by_macro["slowFadeOutB"][1]["address"],
            "slowFadeOutSetResumeAddress": h1_by_macro["slowFadeOutB"][2]["address"],
            "slowFadeOutRestoreResumeAddress": h1_by_macro["slowFadeOutB"][4]["address"],
            "flashOperandReadResumeAddress": h1_by_macro["flashScreenWhite"][2]["address"],
        },
        "constants": constants,
        "sourceFacts": {
            "sourceSiteOrderKeys": compact["sourceSiteOrderKeys"],
            "sourceSitesSha256": compact["sourceSitesSha256"],
            "programTotalOrderKeys": compact["programTotalOrderKeys"],
            "programTotalsSha256": compact["programTotalsSha256"],
            "sourceInputRows": _source_inputs(facts),
            "handlers": handlers,
            "callerBreakdown": facts["callerBreakdown"],
            "evidenceLabels": {"staticFindings": "Confirmed", "runtimeObservations": "Confirmed"},
        },
        "runtimeQuestions": facts["runtimeQuestions"],
    }


def _handler(static: dict[str, Any], macro: str) -> dict[str, Any]:
    rows = [row for row in static["sourceFacts"]["handlers"] if row["macro"] == macro]
    if len(rows) != 1:
        raise ValueError(f"screen-presentation handler identity drift: {macro}")
    return rows[0]


def _source_row(static: dict[str, Any], key: str | None, macro: str) -> dict[str, Any] | None:
    if key is None:
        if macro != "slowFadeOutB":
            raise ValueError("only the zero-use slow fade-out row may be controlled")
        return None
    rows = [row for row in static["sourceFacts"]["sourceInputRows"] if row["sourceOrderKey"] == key]
    if len(rows) != 1 or rows[0]["macro"] != macro:
        raise ValueError(f"screen-presentation source case identity drift: {key}")
    return rows[0]


def _call_register_words(
    handler: dict[str, Any], input_word: int | None, index: int
) -> list[int] | None:
    relation = handler["useSiteRelations"]
    if relation["kind"] == "quake":
        value = (input_word or 0) & relation["mask"]
        positive = bool((input_word or 0) & (1 << relation["firstBit"]))
        step = relation["positiveStep"] if positive else relation["negativeStep"]
        return [
            relation["sleepD0"],
            (index + 1) * step if positive else value + (index + 1) * step,
            step & WORD_MASK,
        ]
    if relation["kind"] in {"flash", "preset"}:
        return [relation["d0"], relation["d1"], relation["d2"]]
    return None


def _dbf_dispatch_count(initial_counter: int) -> int:
    """Model the 68000 DBF decrement-and-branch execution count."""
    if not 0 <= initial_counter <= WORD_MASK:
        raise ValueError("screen-presentation DBF initial counter range drift")
    dispatch_count = 0
    counter = initial_counter
    while True:
        dispatch_count += 1
        counter = (counter - 1) & WORD_MASK
        if counter == WORD_MASK:
            return dispatch_count


def _derive_case(
    case: dict[str, Any], static: dict[str, Any], script_offset: int
) -> dict[str, Any]:
    macro = case["macro"]
    handler = _handler(static, macro)
    source = _source_row(static, case["sourceOrderKey"], macro)
    input_word = (
        None
        if source is None
        else (source["operandValues"][0]["resolvedValue"] if source["operandValues"] else None)
    )
    if source is None:
        input_word = None
    if macro == "setQuake" and input_word is None:
        raise ValueError("screen-presentation quake source operand is missing")
    if macro == "flashScreenWhite" and input_word is None:
        raise ValueError("screen-presentation flash source operand is missing")
    calls = handler["directCallPlan"]
    if macro == "setQuake":
        relation = handler["useSiteRelations"]
        magnitude = input_word & relation["mask"]
        if input_word & (1 << relation["firstBit"]):
            repeat = magnitude
            writes = [(index + 1) * relation["positiveStep"] for index in range(magnitude)]
        elif input_word & (1 << relation["secondBit"]):
            repeat = magnitude
            writes = [
                magnitude + (index + 1) * relation["negativeStep"] for index in range(magnitude)
            ]
        else:
            repeat = 0
            writes = [magnitude]
        direct_calls = calls * repeat
        flash_count = None
    elif macro == "flashScreenWhite":
        relation = handler["useSiteRelations"]
        if (
            relation["loopCounterRegister"] != "d7"
            or relation["loopInstruction"] != "dbf d7,loc_4667A"
            or relation["loopTargetInstruction"] != "jsr LaunchFading(pc)"
        ):
            raise ValueError("screen-presentation flash loop-use relation drift")
        flash_count = _dbf_dispatch_count(input_word >> relation["shift"])
        direct_calls = [calls[0]] * flash_count + [calls[1]]
        writes = None
        repeat = None
    else:
        direct_calls = calls
        writes = None
        flash_count = None
        repeat = None
    call_registers = (
        [_call_register_words(handler, input_word, index) for index, _ in enumerate(direct_calls)]
        if macro
        in {
            "setQuake",
            "tintMap",
            "flickerOnce",
            "mapFadeOutToWhite",
            "mapFadeInFromWhite",
            "flashScreenWhite",
            "fadeInFromBlackHalf",
            "fadeOutToBlackHalf",
        }
        else []
    )
    target_counts = {target: 0 for target in TARGET_IDENTITIES}
    for call in direct_calls:
        target_counts[call["effectiveTarget"]] += 1
    return {
        "id": case["id"],
        "macro": macro,
        "handlerAddress": handler["handlerAddress"],
        "sourceInputOrderKey": None if source is None else source["sourceOrderKey"],
        "handlerInputWord": input_word,
        "scriptCursorRamOffsetAfter": script_offset + handler["cursorAdvanceByteCount"],
        "directCallPlan": direct_calls,
        "directCallRegisterWords": call_registers,
        "effectiveTargetCounts": target_counts,
        "directRamAccessPlan": handler["directRamAccessPlan"],
        "quakeLoopIterationCount": repeat,
        "quakeAmplitudeWordWrites": writes,
        "flashLoopIterationCount": flash_count,
        "sourceInput": source,
    }


def derive_case_expectations(
    static: dict[str, Any], fixture: dict[str, Any]
) -> list[dict[str, Any]]:
    """Derive case values from H2 rows and named H1 use sites before runtime goldens."""
    if fixture["sourceContract"] != {
        "sourceSitesSha256": static["sourceFacts"]["sourceSitesSha256"],
        "programTotalsSha256": static["sourceFacts"]["programTotalsSha256"],
        "sourceSiteOrderKeyCount": len(static["sourceFacts"]["sourceSiteOrderKeys"]),
        "programTotalOrderKeyCount": len(static["sourceFacts"]["programTotalOrderKeys"]),
    }:
        raise ValueError("screen-presentation full H2 compact-contract drift")
    if fixture["runtimeQuestions"] != static["runtimeQuestions"]:
        raise ValueError("screen-presentation runtime-question fixture drift")
    offset = fixture["instrumentation"]["scriptInputRamOffset"]
    if not isinstance(offset, int) or offset < 0:
        raise ValueError("screen-presentation script input RAM offset drift")
    derived = [_derive_case(case, static, offset) for case in fixture["cases"]]
    if len({row["id"] for row in derived}) != len(derived):
        raise ValueError("screen-presentation case ID uniqueness drift")
    if {row["macro"] for row in derived} != {macro for macro, _ in HANDLER_FORMS}:
        raise ValueError("screen-presentation complete handler runtime coverage drift")
    source_rows = [row for row in derived if row["sourceInput"] is not None]
    if len(source_rows) != len(derived) - 1:
        raise ValueError("screen-presentation controlled-row separation drift")
    digest = hashlib.sha256(_canonical_bytes({"cases": derived})).hexdigest().upper()
    if fixture["caseSemanticsSha256"] != digest:
        raise ValueError("screen-presentation complete derived-case golden drift")
    return derived


def _service_patches(
    static: dict[str, Any], rom_path: Path, fixture: dict[str, Any]
) -> list[dict[str, Any]]:
    callbacks = [
        call for handler in static["sourceFacts"]["handlers"] for call in handler["directCallPlan"]
    ]
    addresses = {call["effectiveTarget"]: call["targetAddress"] for call in callbacks}
    if set(addresses) != set(TARGET_IDENTITIES):
        raise ValueError("screen-presentation service target identity inventory drift")
    configured = _closed_rows(
        fixture["instrumentation"]["serviceInterception"],
        {"targetIdentity", "address", "originalHex", "patchedHex"},
        name="service interception",
    )
    if [row["targetIdentity"] for row in configured] != list(TARGET_IDENTITIES):
        raise ValueError("screen-presentation service interception identity/order drift")
    rom = rom_path.read_bytes()
    rows = []
    for target, configured_patch in zip(TARGET_IDENTITIES, configured, strict=True):
        address = addresses[target]
        original = bytes.fromhex(configured_patch["originalHex"])
        patched = bytes.fromhex(configured_patch["patchedHex"])
        if configured_patch["address"] != address or len(original) != 2 or patched != b"\x4e\x75":
            raise ValueError(f"screen-presentation service interception contract drift: {target}")
        if rom[address : address + len(original)] != original:
            raise ValueError(f"screen-presentation service entry preflight drift: {target}")
        rows.append(
            {
                "targetIdentity": target,
                "address": address,
                "originalBytes": original,
                "patchedBytes": patched,
            }
        )
    return rows


def _instrument_screen_presentation_rom(
    rom_path: Path, fixture: dict[str, Any], static: dict[str, Any]
) -> Path:
    base = _instrument_rom(rom_path, fixture)
    data = bytearray(base.read_bytes())
    source = rom_path.read_bytes()
    for patch in _service_patches(static, rom_path, fixture):
        address = patch["address"]
        if source[address : address + 2] != patch["originalBytes"]:
            raise ValueError(f"screen-presentation ROM byte drift: {patch['targetIdentity']}")
        if data[address : address + 2] != patch["originalBytes"]:
            raise ValueError(
                f"screen-presentation instrumentation overlap: {patch['targetIdentity']}"
            )
        data[address : address + 2] = patch["patchedBytes"]
    data[0x18E:0x190] = int(mega_drive_checksum(bytes(data)), 16).to_bytes(2, "big")
    DERIVED_ROOT.mkdir(parents=True, exist_ok=True)
    output = DERIVED_ROOT / "map-script-screen-presentation.instrumented.bin"
    output.write_bytes(data)
    return output


def _observer_cases(fixture: dict[str, Any]) -> list[dict[str, Any]]:
    return [
        {key: case[key] for key in ("id", "macro", "sourceOrderKey")} for case in fixture["cases"]
    ]


def verify_map_script_screen_presentation(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 180
) -> dict[str, Any]:
    """Run all bounded handler-local partitions in one session-only launch."""
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner="screen-presentation fixture")
    verify_runtime_contract(fixture, rom_path)
    static = build_map_script_screen_presentation_contract(rom_path, upstream_path)
    for field in ("provenance", "romSha256", "function", "constants", "runtimeQuestions"):
        if fixture[field] != static[field]:
            raise ValueError(f"screen-presentation fixture/source identity drift: {field}")
    derived = derive_case_expectations(static, fixture)
    instrumented = _instrument_screen_presentation_rom(rom_path, fixture, static)

    def observe() -> dict[str, Any]:
        return run_observer(
            rom_path=instrumented,
            observer_path=OBSERVER,
            config={
                "fixtureId": fixture["id"],
                "mapTestIndex": fixture["mapTestIndex"],
                "function": static["function"],
                "constants": static["constants"],
                "instrumentation": fixture["instrumentation"],
                "maxFrames": fixture["maxFrames"],
                "harness": load_json(repo_path(fixture["sharedHarnessFixture"]))["harness"],
                "cases": _observer_cases(fixture),
                "derived": derived,
                "targetIdentityByAddress": {
                    str(call["targetAddress"]): call["effectiveTarget"]
                    for handler in static["sourceFacts"]["handlers"]
                    for call in handler["directCallPlan"]
                },
            },
            output_name="map-script-screen-presentation",
            timeout_seconds=timeout_seconds,
        )

    observed = _with_instrumented_rom_database(
        instrumented, "SF2 H3 instrumented map-script screen presentation", observe
    )
    validate_json(observed, OBSERVATION_SCHEMA, owner="screen-presentation observation")
    expected = {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["id"],
        "mapTest": fixture["mapTestIndex"],
        "recordOrder": [row["id"] for row in derived],
        "records": [
            {
                "id": row["id"],
                "handlerEntryPcObserved": row["handlerAddress"],
                "handlerReturnPcObserved": static["function"][
                    f"{_handler(static, row['macro'])['handler']}ReturnAddress"
                ],
                "handlerReturned": True,
                "scriptCursorRamOffsetAfterObserved": row["scriptCursorRamOffsetAfter"],
                "stackPointerDeltaBytesObserved": 4,
                "directCallsObserved": [
                    {
                        "effectiveTargetObserved": call["effectiveTarget"],
                        "callSiteAddressObserved": call["callSiteAddress"],
                        "targetAddressObserved": call["targetAddress"],
                        "returnAddressObserved": call["returnAddress"],
                    }
                    for call in row["directCallPlan"]
                ],
                "effectiveTargetCountsObserved": row["effectiveTargetCounts"],
                "quakeAmplitudeWordWritesObserved": row["quakeAmplitudeWordWrites"],
                "fadingCounterByteReadsObserved": [fixture["instrumentation"]["fadingCounterSeed"]]
                if row["macro"] in {"slowFadeInB", "slowFadeOutB"}
                else None,
                "fadingCounterByteWritesObserved": [
                    6,
                    fixture["instrumentation"]["fadingCounterSeed"],
                ]
                if row["macro"] in {"slowFadeInB", "slowFadeOutB"}
                else None,
                "flashDurationWordAfterShiftObserved": None
                if row["handlerInputWord"] is None or row["macro"] != "flashScreenWhite"
                else (
                    row["handlerInputWord"]
                    >> _handler(static, row["macro"])["useSiteRelations"]["shift"]
                ),
                "flashLoopIterationCountObserved": row["flashLoopIterationCount"],
                "directCallRegisterWordsObserved": row["directCallRegisterWords"],
            }
            for row in derived
        ],
    }
    if observed != expected:
        raise ValueError(
            "screen-presentation runtime matrix mismatch\n"
            f"expected={expected!r}\nobserved={observed!r}"
        )
    return {
        "Fixture": fixture["id"],
        "Cases": len(derived),
        "Handlers": len(HANDLER_FORMS),
        "BizHawkLaunches": 1,
        "Instrumentation": "session-only external-service entry shims",
        "Status": "PASS",
    }
