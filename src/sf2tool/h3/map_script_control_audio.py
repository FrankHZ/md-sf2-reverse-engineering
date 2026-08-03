"""One-launch H3 boundary for map-script control and sound-command dispatch.

The observer drives six synthetic, RAM-owned script streams through the original
``ExecuteMapScript`` loop.  It confirms only the bounded cursor, dispatch,
call/return, and skip-gate observations described by the H2 source contract;
it intentionally does not generalize the D0=1 wait result into timer semantics,
sound output, or callee service effects.
"""

from __future__ import annotations

import json
import re
from pathlib import Path
from typing import Any

from sf2tool.h2.map_script_engine import build_map_script_engine_contract
from sf2tool.h3.bizhawk import DERIVED_ROOT, run_observer, verify_runtime_contract
from sf2tool.h3.map_lifecycle import (
    _instrument_rom as _instrument_map_lifecycle_rom,
)
from sf2tool.h3.map_lifecycle import (
    _with_instrumented_rom_database,
)
from sf2tool.h3.observer_status import (
    assert_observer_status,
    callback_failure_status,
    observer_failure_contract,
)
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path
from sf2tool.research_index import listing_symbol_addresses

H2_FIXTURE = repo_path("tests/fixtures/h2/map-script-engine-static-v1.json")
FIXTURE = repo_path("tests/fixtures/h3/map-script-control-audio-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h3/h3-map-script-control-audio-fixture.schema.json")
OBSERVATION_SCHEMA = repo_path(
    "schemas/h3/h3-map-script-control-audio-observation.schema.json"
)
OBSERVER = repo_path("tools/bizhawk/map_script_control_audio_observer.lua")
FAILURE_SCHEMA = repo_path(
    "schemas/h3/map-script-control-audio-callback-failure.schema.json"
)
CALLBACK_AUDIT_SCHEMA = repo_path("schemas/h3/observer-callback-audit.schema.json")
H1_LISTING_PATH = Path("build/sf2build-h1.lst")

RUNTIME_QUESTIONS = (
    "map-script-control-audio/wait-normal-and-skip-gate",
    "map-script-control-audio/no-op-dispatch-boundary",
    "map-script-control-audio/sound-dispatch-boundary",
    "map-script-control-audio/subroutine-call-return-boundary",
    "map-script-control-audio/jump-cursor-redirect-and-end-boundary",
)
MACRO_SOURCE_ORDER = (
    "csWait",
    "playSound",
    "csc06",
    "executeSubroutine",
    "jump",
    "cscNop",
    "csc_end",
)
HANDLER_BY_MACRO = {
    "playSound": "csc05_playSound",
    "csc06": "csc06_doNothing",
    "executeSubroutine": "csc0A_executeSubroutine",
    "jump": "csc0B_jump",
}
OBSERVER_OUTPUT_NAME = "map-script-control-audio"
OBSERVER_FAILURE_CONTRACT = observer_failure_contract(OBSERVER_OUTPUT_NAME)
_CASE_ROLE_BY_PHASE_KIND = {
    "opcode-dispatch": {
        "no-op": "opcode-dispatch/no-op",
        "sound": "opcode-dispatch/sound",
        "subroutine": "opcode-dispatch/subroutine",
        "jump": "opcode-dispatch/jump",
    },
    "csc06-entry": {
        "no-op": "csc06-entry/direct-dispatch",
        "subroutine": "csc06-entry/subroutine-target",
    },
}


def _observer_status_path() -> Path:
    return DERIVED_ROOT / f"{OBSERVER_OUTPUT_NAME}.status.txt"


def _observer_output_path() -> Path:
    return DERIVED_ROOT / f"{OBSERVER_OUTPUT_NAME}.observed.json"


def _callback_failure_status(status_path: Path) -> dict[str, Any] | None:
    return callback_failure_status(
        status_path,
        owner=OBSERVER_OUTPUT_NAME,
        schema_path=FAILURE_SCHEMA,
    )


def _raise_for_callback_failure_status(status_path: Path, output_path: Path) -> None:
    payload = _callback_failure_status(status_path)
    if payload is not None:
        output_path.unlink(missing_ok=True)
        raise RuntimeError(
            "map-script control/audio observer callback failure status:\n"
            f"{json.dumps(payload, sort_keys=True)}"
        )


def _assert_success_status(status_path: Path) -> None:
    assert_observer_status(
        status_path,
        owner=OBSERVER_OUTPUT_NAME,
        schema_path=FAILURE_SCHEMA,
    )


def _parse_equates(upstream_path: Path) -> dict[str, int]:
    """Parse the source-owned input and skip-gate constants once."""
    required = (
        "PLAYER_2_INPUT",
        "DEBUG_MODE_TOGGLE",
        "SKIP_CUTSCENE_TEXT",
        "INPUT_BIT_START",
    )
    values: dict[str, int] = {}
    for relative in ("disasm/sf2const.asm", "disasm/sf2enums.asm"):
        text = (upstream_path / relative).read_text(encoding="utf-8")
        for match in re.finditer(
            r"^(?P<name>[A-Za-z_][A-Za-z0-9_]*):\s+equ\s+"
            r"(?P<value>\$?[0-9A-Fa-f]+)\b",
            text,
            re.MULTILINE,
        ):
            name = match.group("name")
            if name not in required:
                continue
            token = match.group("value")
            value = int(token.removeprefix("$"), 16 if token.startswith("$") else 10)
            previous = values.setdefault(name, value)
            if previous != value:
                raise ValueError(f"map-script control/audio equate conflict: {name}")
    missing = [name for name in required if name not in values]
    if missing:
        raise ValueError(f"map-script control/audio source constants missing: {missing}")
    return {name: values[name] for name in required}


def _h1_section(listing: str, symbol: str) -> str:
    match = re.search(
        rf"^[0-9A-F]{{8}}\s+{re.escape(symbol)}:\s*$"
        rf"(?P<body>.*?)^\s*[0-9A-F]{{8}}\s+; End of function {re.escape(symbol)}\s*$",
        listing,
        re.MULTILINE | re.DOTALL,
    )
    if match is None:
        raise ValueError(f"map-script control/audio H1 section missing: {symbol}")
    return match.group("body")


def _h1_instruction_address(section: str, instruction: str, *, owner: str) -> int:
    """Locate one exact H1 instruction, rejecting duplicate source-use sites."""
    matches: list[int] = []
    for match in re.finditer(
        r"^(?P<address>[0-9A-F]{8})[ \t]+(?P<tail>.*?)$", section, re.MULTILINE
    ):
        tail = match.group("tail").strip()
        tail = re.sub(r"^(?:[0-9A-F]{2,8}[ \t]+)+", "", tail)
        tail = re.sub(r"^M\s+", "", tail)
        normalized = re.sub(r"\s+", " ", tail).strip()
        if normalized == instruction:
            matches.append(int(match.group("address"), 16))
    if len(matches) != 1:
        raise ValueError(
            f"map-script control/audio H1 {owner} instruction identity drift: "
            f"{instruction!r} ({len(matches)} matches)"
        )
    return matches[0]


def _h1_instruction_bytes(section: str, instruction: str, *, owner: str) -> bytes:
    """Return one exact instruction's emitted bytes without accepting text near-misses."""
    matches: list[bytes] = []
    for match in re.finditer(
        r"^(?P<address>[0-9A-F]{8})[ \t]+(?P<tail>.*?)$", section, re.MULTILINE
    ):
        tail = match.group("tail").strip()
        encoded = re.match(r"(?P<bytes>(?:[0-9A-F]{2,8}[ \t]+)+)(?P<text>.*)$", tail)
        if encoded is None:
            continue
        normalized = re.sub(r"^M\s+", "", encoded.group("text").strip())
        normalized = re.sub(r"\s+", " ", normalized).strip()
        if normalized == instruction:
            matches.append(bytes.fromhex("".join(encoded.group("bytes").split())))
    if len(matches) != 1:
        raise ValueError(
            f"map-script control/audio H1 {owner} instruction byte identity drift: "
            f"{instruction!r} ({len(matches)} matches)"
        )
    return matches[0]


def _h1_followup_instruction_address(
    section: str, instruction: str, followup: str, *, owner: str
) -> int:
    """Resolve the next executable H1 instruction after one unique use site."""
    source_address = _h1_instruction_address(section, instruction, owner=owner)
    lines = section.splitlines()
    source_line: int | None = None
    for index, line in enumerate(lines):
        match = re.match(rf"^{source_address:08X}[ \t]+(?P<tail>.*?)$", line)
        if match is None:
            continue
        tail = re.sub(r"^(?:[0-9A-F]{2,8}[ \t]+)+", "", match.group("tail").strip())
        tail = re.sub(r"^M\s+", "", tail)
        if re.sub(r"\s+", " ", tail).strip() == instruction:
            source_line = index
            break
    if source_line is None:
        raise ValueError(f"map-script control/audio H1 {owner} source line is missing")
    for line in lines[source_line + 1 :]:
        match = re.match(r"^(?P<address>[0-9A-F]{8})[ \t]+(?P<tail>.*?)$", line)
        if match is None:
            continue
        tail = re.sub(r"^(?:[0-9A-F]{2,8}[ \t]+)+", "", match.group("tail").strip())
        tail = re.sub(r"^M\s+", "", tail)
        normalized = re.sub(r"\s+", " ", tail).strip()
        if not normalized or re.fullmatch(r"[A-Za-z_][A-Za-z0-9_]*:", normalized):
            continue
        if normalized != followup:
            raise ValueError(
                f"map-script control/audio H1 {owner} followup drift: "
                f"expected {followup!r}, got {normalized!r}"
            )
        return int(match.group("address"), 16)
    raise ValueError(f"map-script control/audio H1 {owner} followup is missing")


def _h1_label_address(section: str, label: str, *, owner: str) -> int:
    matches = re.findall(
        rf"^(?P<address>[0-9A-F]{{8}})[ \t]+{re.escape(label)}:\s*$",
        section,
        re.MULTILINE,
    )
    if len(matches) != 1:
        raise ValueError(
            f"map-script control/audio H1 {owner} label identity drift: {label!r}"
        )
    return int(matches[0], 16)


def _expected_trampoline_stub(
    fixture: dict[str, Any], *, epilogue: bytes = b"\x4c\xdf\x03\xff\x4e\x75"
) -> bytes:
    """Build the whole session-only trampoline from the source-owned ABI shape."""
    instrumentation = fixture["instrumentation"]
    return (
        b"\x20\x7c"
        + (instrumentation["ramInputAddress"] + 4).to_bytes(4, "big")
        + b"\x4e\xb9"
        + fixture["function"]["executeMapScriptAddress"].to_bytes(4, "big")
        + b"\x58\x8f"
        + epilogue
    )


def _preserved_wrapper_trampoline(section: str, fixture: dict[str, Any]) -> dict[str, int]:
    """Prove the source epilogue copied into the 20-byte session-only stub."""
    instrumentation = fixture["instrumentation"]
    call_address = _h1_instruction_address(
        section, "jsr (a0)", owner="RunMapSetupInitFunction interception"
    )
    call_bytes = _h1_instruction_bytes(
        section, "jsr (a0)", owner="RunMapSetupInitFunction interception"
    )
    restore_address = _h1_followup_instruction_address(
        section,
        "jsr (a0)",
        "movem.l (sp)+,d0-a1",
        owner="RunMapSetupInitFunction interception restore",
    )
    restore_bytes = _h1_instruction_bytes(
        section, "movem.l (sp)+,d0-a1", owner="RunMapSetupInitFunction interception restore"
    )
    retained_rts_address = _h1_followup_instruction_address(
        section,
        "movem.l (sp)+,d0-a1",
        "rts",
        owner="RunMapSetupInitFunction interception return",
    )
    rts_bytes = _h1_instruction_bytes(
        section, "rts", owner="RunMapSetupInitFunction interception return"
    )
    original = bytes.fromhex(instrumentation["callSiteOriginalHex"])
    patched = bytes.fromhex(instrumentation["callSitePatchedHex"])
    if (
        call_address != instrumentation["callSiteAddress"]
        or original != call_bytes + restore_bytes
    ):
        raise ValueError("map-script control/audio interception source bytes drifted")
    if restore_address != call_address + len(call_bytes):
        raise ValueError("map-script control/audio interception restore adjacency drifted")
    if retained_rts_address != restore_address + len(restore_bytes):
        raise ValueError("map-script control/audio interception return adjacency drifted")
    if patched != b"\x4E\xB9" + instrumentation["stubAddress"].to_bytes(4, "big"):
        raise ValueError("map-script control/audio interception patched JSR shape drifted")
    discarded_patched_return = call_address + len(patched)
    if discarded_patched_return != retained_rts_address:
        raise ValueError("map-script control/audio interception stack-discard target drifted")
    expected_stub = _expected_trampoline_stub(
        fixture, epilogue=restore_bytes + rts_bytes
    )
    stub = bytes.fromhex(instrumentation["stubHex"])
    original_stub = bytes.fromhex(instrumentation["stubOriginalHex"])
    if stub != expected_stub:
        raise ValueError("map-script control/audio preserved trampoline shape drifted")
    if original_stub != b"\xff" * len(expected_stub):
        raise ValueError("map-script control/audio trampoline padding corpus drifted")
    if instrumentation["trampolinePostHandlerAddress"] != (
        instrumentation["stubAddress"] + len(expected_stub) - len(rts_bytes)
    ):
        raise ValueError("map-script control/audio trampoline post-handler seam drifted")
    return {
        "callSiteAddress": call_address,
        "trampolinePostHandlerAddress": instrumentation["trampolinePostHandlerAddress"],
    }


def _handler_row(facts: dict[str, Any], macro: str) -> dict[str, Any]:
    rows = [row for row in facts["handlers"] if row["macro"] == macro]
    if len(rows) != 1:
        raise ValueError(f"map-script control/audio H2 handler inventory drift: {macro}")
    return rows[0]


def _validate_static_boundary(static: dict[str, Any], h2_fixture: dict[str, Any]) -> dict[str, Any]:
    """Reject any H2 boundary drift before deriving an executable H3 matrix."""
    facts = static.get("scriptControlCommandFacts")
    expected = h2_fixture.get("expected", {}).get("scriptControlCommandFacts")
    if (
        not isinstance(facts, dict)
        or not isinstance(expected, dict)
        or {field: facts.get(field) for field in expected} != expected
    ):
        raise ValueError("map-script control/audio H2 static authority drift")
    if tuple(facts["macroSourceOrder"]) != MACRO_SOURCE_ORDER:
        raise ValueError("map-script control/audio macro source order drift")
    macros = {row["name"]: row for row in facts["macros"]}
    if tuple(macros) != MACRO_SOURCE_ORDER:
        raise ValueError("map-script control/audio macro inventory order drift")
    if set(HANDLER_BY_MACRO) != {
        row["macro"] for row in facts["handlers"]
    }:
        raise ValueError("map-script control/audio handler macro inventory drift")
    for macro, handler_name in HANDLER_BY_MACRO.items():
        row = _handler_row(facts, macro)
        if row["handler"] != handler_name or row["opcode"] != macros[macro]["opcode"]:
            raise ValueError(f"map-script control/audio handler binding drift: {macro}")
    main = facts["mainLoopGuard"]
    if (
        main["scriptCursorReadUseSite"]["instruction"] != "move.w (a6)+,d0"
        or main["negativeWaitPath"]["sleepCallInstruction"] != "jsr (Sleep).w"
        or main["opcodeDispatchPath"]["dispatchCallInstruction"]
        != "jsr rjt_cutsceneScriptCommands(pc,d0.w)"
        or main["terminatorCompare"]["terminatorMacro"] != "csc_end"
    ):
        raise ValueError("map-script control/audio main-loop source guard drift")
    if main["negativeWaitPath"]["byteMaskUse"]["value"] != 0xFF:
        raise ValueError("map-script control/audio wait mask derivation drift")
    if (
        macros["csWait"]["negativeWaitLeadByte"] != 0x80
        or macros["csc_end"]["encodedWord"] != 0xFFFF
        or macros["csc06"]["sourceCommandCount"] != 0
    ):
        raise ValueError("map-script control/audio special form contract drift")
    return facts


def _runtime_navigation(
    static: dict[str, Any], fixture: dict[str, Any], upstream_path: Path
) -> dict[str, Any]:
    """Derive H1 event PCs and source-owned RAM state from their use sites."""
    listing = (upstream_path / H1_LISTING_PATH).read_text(encoding="utf-8")
    symbols = listing_symbol_addresses(listing)
    facts = static["scriptControlCommandFacts"]
    execute = static["function"]["ExecuteMapScript"]
    if symbols.get("ExecuteMapScript") != execute:
        raise ValueError("map-script control/audio ExecuteMapScript H1 identity drift")
    setup_section = _h1_section(listing, "RunMapSetupInitFunction")
    interception = _preserved_wrapper_trampoline(setup_section, fixture)
    main_section = _h1_section(listing, "ExecuteMapScript")
    handlers = {macro: _handler_row(facts, macro) for macro in HANDLER_BY_MACRO}
    for macro, handler in handlers.items():
        if symbols.get(handler["handler"]) != handler["address"]:
            raise ValueError(f"map-script control/audio H1 handler identity drift: {macro}")
    play_section = _h1_section(listing, HANDLER_BY_MACRO["playSound"])
    subroutine_section = _h1_section(listing, HANDLER_BY_MACRO["executeSubroutine"])
    jump_section = _h1_section(listing, HANDLER_BY_MACRO["jump"])
    navigation = {
        "entryAddress": symbols["RunMapSetupInitFunction"],
        "entryInjectionCallSiteAddress": interception["callSiteAddress"],
        "executeMapScriptAddress": execute,
        "scriptWordReadAfterAddress": _h1_instruction_address(
            main_section, "cmpi.w #-1,d0", owner="ExecuteMapScript script word read"
        ),
        "waitSkipGateSetAddress": _h1_instruction_address(
            main_section,
            "move.b #-1,((SKIP_CUTSCENE_TEXT-$1000000)).w",
            owner="ExecuteMapScript skip gate",
        ),
        "waitSleepCallAddress": _h1_instruction_address(
            main_section, "jsr (Sleep).w", owner="ExecuteMapScript wait"
        ),
        "waitSkipTargetAddress": _h1_label_address(
            main_section, "loc_47172", owner="ExecuteMapScript wait skip"
        ),
        "opcodeDispatchCallAddress": _h1_instruction_address(
            main_section,
            "jsr rjt_cutsceneScriptCommands(pc,d0.w)",
            owner="ExecuteMapScript opcode dispatch",
        ),
        "opcodeDispatchReturnAddress": _h1_followup_instruction_address(
            main_section,
            "jsr rjt_cutsceneScriptCommands(pc,d0.w)",
            "bra.s loc_47140",
            owner="ExecuteMapScript opcode dispatch return",
        ),
        "endAddress": _h1_label_address(main_section, "loc_47234", owner="ExecuteMapScript end"),
        "csc05PlaySoundAddress": handlers["playSound"]["address"],
        "playSoundTrapAddress": _h1_instruction_address(
            play_section, "trap #sound_command", owner="csc05 sound command"
        ),
        "playSoundReturnAddress": _h1_instruction_address(
            play_section, "rts", owner="csc05 return"
        ),
        "csc06DoNothingAddress": handlers["csc06"]["address"],
        "csc0AExecuteSubroutineAddress": handlers["executeSubroutine"]["address"],
        "subroutineCursorAfterReadAddress": _h1_instruction_address(
            subroutine_section, "move.l a0,-(sp)", owner="csc0A cursor read"
        ),
        "subroutineIndirectCallAddress": _h1_instruction_address(
            subroutine_section, "jsr (a1)", owner="csc0A indirect call"
        ),
        "subroutineResumeAddress": _h1_instruction_address(
            subroutine_section, "movea.l (sp)+,a0", owner="csc0A restore"
        ),
        "subroutineReturnAddress": _h1_instruction_address(
            subroutine_section, "rts", owner="csc0A return"
        ),
        "csc0BJumpAddress": handlers["jump"]["address"],
        "jumpCursorRedirectAfterAddress": _h1_instruction_address(
            jump_section, "rts", owner="csc0B cursor redirect"
        ),
    }
    equates = _parse_equates(upstream_path)
    ram = {
        "player2InputAddress": equates["PLAYER_2_INPUT"],
        "debugModeToggleAddress": equates["DEBUG_MODE_TOGGLE"],
        "skipCutsceneTextAddress": equates["SKIP_CUTSCENE_TEXT"],
    }
    constants = {
        "inputBitStart": equates["INPUT_BIT_START"],
        "inputStartMask": 1 << equates["INPUT_BIT_START"],
        "savedA0StackBytes": 4,
        "jsrReturnStackBytes": 4,
    }
    if (
        fixture["function"] != navigation
        or fixture["ram"] != ram
        or fixture["constants"] != constants
    ):
        raise ValueError("map-script control/audio fixture/H1 navigation drift")
    return {
        "function": navigation,
        "ram": ram,
        "constants": constants,
        "service": {
            "sleepAddress": symbols["Sleep"],
            "waitForVIntAddress": symbols["WaitForVInt"],
        },
    }


def _sound_value(facts: dict[str, Any], source_symbol: str) -> int:
    matches = [
        row
        for row in facts["soundOperandJoin"]["soundOperands"]
        if row["sourceSymbol"] == source_symbol
    ]
    if len(matches) != 1 or matches[0]["sourceCategory"] != "music":
        raise ValueError("map-script control/audio sound source-symbol derivation drift")
    return matches[0]["value"]


def _word_bytes(value: int) -> list[int]:
    return list(value.to_bytes(2, "big"))


def _long_bytes(value: int) -> list[int]:
    return list(value.to_bytes(4, "big"))


def _expected_case(
    facts: dict[str, Any], fixture: dict[str, Any], case: dict[str, Any]
) -> dict[str, Any]:
    """Derive one compact observation record from source-backed macro use sites."""
    macros = {row["name"]: row for row in facts["macros"]}
    input_data = case["input"]
    kind = case["kind"]
    expected: dict[str, Any] = {
        "id": case["id"],
        "questionId": case["questionId"],
        "handlerEntries": ["ExecuteMapScript"],
        "executeMapScriptReturned": True,
        "scriptWordReads": [],
        "skipGateSetObserved": False,
        "sleepCallObserved": False,
        "sleepD0Word": None,
        "dispatchCallObserved": False,
        "playSoundTrapD0Word": None,
        "csc06Returned": False,
        "subroutineCursorAfterReadOffset": None,
        "subroutineStackDeltaAtCall": None,
        "subroutineTargetStackDelta": None,
        "subroutineTargetReturned": False,
        "subroutineResumeStackDelta": None,
        "jumpCursorRedirectOffset": None,
        "endReached": True,
    }
    wait_for_vint_calls = 0
    bytes_out: list[int]
    end_word = macros["csc_end"]["encodedWord"]
    if kind in {"wait-normal", "wait-skip"}:
        duration = input_data["waitDuration"]
        if (
            not isinstance(duration, int)
            or not 0 <= duration <= 0xFF
            or input_data["soundSourceSymbol"] is not None
            or input_data["subroutineTarget"] is not None
        ):
            raise ValueError("map-script control/audio wait case input drift")
        wait_word = (macros["csWait"]["negativeWaitLeadByte"] << 8) | duration
        bytes_out = _word_bytes(wait_word) + _word_bytes(end_word)
        expected["scriptWordReads"] = [
            {"word": wait_word, "cursorAfterReadOffset": 2},
            {"word": end_word, "cursorAfterReadOffset": 4},
        ]
        expected["skipGateSetObserved"] = kind == "wait-skip"
        expected["sleepCallObserved"] = kind == "wait-normal"
        expected["sleepD0Word"] = duration if kind == "wait-normal" else None
        wait_for_vint_calls = duration if kind == "wait-normal" else 0
    elif kind == "no-op":
        if any(value is not None for value in input_data.values()):
            raise ValueError("map-script control/audio no-op case input drift")
        opcode = macros["csc06"]["opcode"]
        bytes_out = _word_bytes(opcode) + _word_bytes(end_word)
        expected["handlerEntries"].append(HANDLER_BY_MACRO["csc06"])
        expected["scriptWordReads"] = [
            {"word": opcode, "cursorAfterReadOffset": 2},
            {"word": end_word, "cursorAfterReadOffset": 4},
        ]
        expected["dispatchCallObserved"] = True
        expected["csc06Returned"] = True
    elif kind == "sound":
        if (
            input_data["waitDuration"] is not None
            or not isinstance(input_data["soundSourceSymbol"], str)
            or input_data["subroutineTarget"] is not None
        ):
            raise ValueError("map-script control/audio sound case input drift")
        opcode = macros["playSound"]["opcode"]
        sound = _sound_value(facts, input_data["soundSourceSymbol"])
        bytes_out = _word_bytes(opcode) + _word_bytes(sound) + _word_bytes(end_word)
        expected["handlerEntries"].append(HANDLER_BY_MACRO["playSound"])
        expected["scriptWordReads"] = [
            {"word": opcode, "cursorAfterReadOffset": 2},
            {"word": sound, "cursorAfterReadOffset": 4},
            {"word": end_word, "cursorAfterReadOffset": 6},
        ]
        expected["dispatchCallObserved"] = True
        expected["playSoundTrapD0Word"] = sound
    elif kind == "subroutine":
        if (
            input_data["waitDuration"] is not None
            or input_data["soundSourceSymbol"] is not None
            or input_data["subroutineTarget"] != HANDLER_BY_MACRO["csc06"]
        ):
            raise ValueError("map-script control/audio subroutine case input drift")
        opcode = macros["executeSubroutine"]["opcode"]
        target = _handler_row(facts, "csc06")["address"]
        bytes_out = _word_bytes(opcode) + _long_bytes(target) + _word_bytes(end_word)
        expected["handlerEntries"].extend(
            [HANDLER_BY_MACRO["executeSubroutine"], HANDLER_BY_MACRO["csc06"]]
        )
        expected["scriptWordReads"] = [
            {"word": opcode, "cursorAfterReadOffset": 2},
            {"word": end_word, "cursorAfterReadOffset": 8},
        ]
        expected["dispatchCallObserved"] = True
        expected["subroutineCursorAfterReadOffset"] = macros["executeSubroutine"]["encodedBytes"]
        expected["subroutineStackDeltaAtCall"] = -fixture["constants"]["savedA0StackBytes"]
        expected["subroutineTargetStackDelta"] = -(
            fixture["constants"]["savedA0StackBytes"]
            + fixture["constants"]["jsrReturnStackBytes"]
        )
        expected["subroutineTargetReturned"] = True
        expected["subroutineResumeStackDelta"] = -fixture["constants"]["savedA0StackBytes"]
    elif kind == "jump":
        if any(value is not None for value in input_data.values()):
            raise ValueError("map-script control/audio jump case input drift")
        opcode = macros["jump"]["opcode"]
        target_offset = macros["jump"]["encodedBytes"]
        target = fixture["instrumentation"]["ramInputAddress"] + 4 + target_offset
        bytes_out = _word_bytes(opcode) + _long_bytes(target) + _word_bytes(end_word)
        expected["handlerEntries"].append(HANDLER_BY_MACRO["jump"])
        expected["scriptWordReads"] = [
            {"word": opcode, "cursorAfterReadOffset": 2},
            {"word": end_word, "cursorAfterReadOffset": target_offset + 2},
        ]
        expected["dispatchCallObserved"] = True
        expected["jumpCursorRedirectOffset"] = target_offset
    else:
        raise ValueError(f"map-script control/audio unknown case kind: {kind}")
    return {
        "expected": expected,
        "scriptBytes": bytes_out,
        "waitForVIntCalls": wait_for_vint_calls,
    }


def derive_case_expectations(
    static: dict[str, Any], fixture: dict[str, Any]
) -> list[dict[str, Any]]:
    """Derive complete case objects before comparing fixture or emulator output."""
    facts = static["scriptControlCommandFacts"]
    derived = [_expected_case(facts, fixture, case) for case in fixture["cases"]]
    if [row["expected"] for row in derived] != [case["expected"] for case in fixture["cases"]]:
        raise ValueError("map-script control/audio fixture/static expectation drift")
    questions = {case["questionId"] for case in fixture["cases"]}
    if questions != set(RUNTIME_QUESTIONS):
        raise ValueError("map-script control/audio grouped question coverage drift")
    return derived


def _failure_roles(cases: list[dict[str, Any]]) -> dict[str, dict[str, str]]:
    """Route the two shared PCs by the fixture case that reaches each role."""
    roles = {phase: {} for phase in _CASE_ROLE_BY_PHASE_KIND}
    seen_case_ids: set[str] = set()
    for case in cases:
        case_id = case.get("id")
        kind = case.get("kind")
        if not isinstance(case_id, str) or not isinstance(kind, str) or case_id in seen_case_ids:
            raise ValueError("map-script control/audio failure-role case identity drift")
        seen_case_ids.add(case_id)
        for phase, roles_by_kind in _CASE_ROLE_BY_PHASE_KIND.items():
            role = roles_by_kind.get(kind)
            if role is not None:
                roles[phase][case_id] = role
    return roles


def _failure_expectation_contract(
    function: dict[str, int],
    instrumentation: dict[str, Any],
    service: dict[str, int],
    cases: list[dict[str, Any]],
) -> tuple[dict[str, dict[str, str]], dict[str, dict[str, dict[str, int | None]]]]:
    """Derive one role-aware expectation object for each physical callback PC."""
    failure_roles = _failure_roles(cases)
    target_for_phase = {
        "run-map-setup-entry": function["executeMapScriptAddress"],
        "trampoline-entry": function["executeMapScriptAddress"],
        "wait-for-vint": service["waitForVIntAddress"],
        "execute-map-script-entry": function["executeMapScriptAddress"],
        "script-word-read": function["executeMapScriptAddress"],
        "wait-skip-gate": function["executeMapScriptAddress"],
        "wait-sleep": service["sleepAddress"],
        "wait-skip-target": function["executeMapScriptAddress"],
        "opcode-dispatch": None,
        "opcode-dispatch-return": None,
        "end": function["executeMapScriptAddress"],
        "csc05-entry": function["csc05PlaySoundAddress"],
        "csc05-trap": function["csc05PlaySoundAddress"],
        "csc05-return": function["csc05PlaySoundAddress"],
        "csc06-entry": function["csc06DoNothingAddress"],
        "csc0a-entry": function["csc0AExecuteSubroutineAddress"],
        "csc0a-cursor": function["csc0AExecuteSubroutineAddress"],
        "csc0a-call": function["csc06DoNothingAddress"],
        "csc0a-resume": function["csc0AExecuteSubroutineAddress"],
        "csc0a-return": function["csc0AExecuteSubroutineAddress"],
        "csc0b-entry": function["csc0BJumpAddress"],
        "csc0b-redirect": function["csc0BJumpAddress"],
        "post-handler": instrumentation["trampolinePostHandlerAddress"],
    }
    address_for_phase = {
        "run-map-setup-entry": function["entryAddress"],
        "trampoline-entry": instrumentation["stubAddress"],
        "wait-for-vint": service["waitForVIntAddress"],
        "execute-map-script-entry": function["executeMapScriptAddress"],
        "script-word-read": function["scriptWordReadAfterAddress"],
        "wait-skip-gate": function["waitSkipGateSetAddress"],
        "wait-sleep": function["waitSleepCallAddress"],
        "wait-skip-target": function["waitSkipTargetAddress"],
        "opcode-dispatch": function["opcodeDispatchCallAddress"],
        "opcode-dispatch-return": function["opcodeDispatchReturnAddress"],
        "end": function["endAddress"],
        "csc05-entry": function["csc05PlaySoundAddress"],
        "csc05-trap": function["playSoundTrapAddress"],
        "csc05-return": function["playSoundReturnAddress"],
        "csc06-entry": function["csc06DoNothingAddress"],
        "csc0a-entry": function["csc0AExecuteSubroutineAddress"],
        "csc0a-cursor": function["subroutineCursorAfterReadAddress"],
        "csc0a-call": function["subroutineIndirectCallAddress"],
        "csc0a-resume": function["subroutineResumeAddress"],
        "csc0a-return": function["subroutineReturnAddress"],
        "csc0b-entry": function["csc0BJumpAddress"],
        "csc0b-redirect": function["jumpCursorRedirectAfterAddress"],
        "post-handler": instrumentation["trampolinePostHandlerAddress"],
    }
    caller_for_phase = {
        "run-map-setup-entry": None,
        "trampoline-entry": function["entryInjectionCallSiteAddress"],
        "wait-for-vint": None,
        "execute-map-script-entry": instrumentation["stubAddress"] + 6,
        "script-word-read": None,
        "wait-skip-gate": None,
        "wait-sleep": function["waitSleepCallAddress"],
        "wait-skip-target": None,
        "opcode-dispatch": function["opcodeDispatchCallAddress"],
        "opcode-dispatch-return": function["opcodeDispatchCallAddress"],
        "end": None,
        "csc05-entry": function["opcodeDispatchCallAddress"],
        "csc05-trap": function["playSoundTrapAddress"],
        "csc05-return": function["opcodeDispatchCallAddress"],
        "csc06-entry": None,
        "csc0a-entry": function["opcodeDispatchCallAddress"],
        "csc0a-cursor": None,
        "csc0a-call": function["subroutineIndirectCallAddress"],
        "csc0a-resume": function["subroutineIndirectCallAddress"],
        "csc0a-return": function["opcodeDispatchCallAddress"],
        "csc0b-entry": function["opcodeDispatchCallAddress"],
        "csc0b-redirect": None,
        "post-handler": None,
    }
    return_for_phase = {
        phase: (
            function["opcodeDispatchReturnAddress"]
            if phase in {"csc05-entry", "csc0a-entry", "csc0b-entry"}
            else function["subroutineResumeAddress"]
            if phase == "csc0a-call"
            else None
        )
        for phase in address_for_phase
    }
    roles_by_phase = {
        phase: {
            phase: {
                "callSiteAddress": caller_for_phase[phase],
                "targetAddress": target_for_phase[phase],
                "returnAddress": return_for_phase[phase],
            }
        }
        for phase in address_for_phase
    }
    dispatch_targets = {
        "opcode-dispatch/no-op": function["csc06DoNothingAddress"],
        "opcode-dispatch/sound": function["csc05PlaySoundAddress"],
        "opcode-dispatch/subroutine": function["csc0AExecuteSubroutineAddress"],
        "opcode-dispatch/jump": function["csc0BJumpAddress"],
    }
    roles_by_phase["opcode-dispatch"] = {
        role: {
            "callSiteAddress": address_for_phase["opcode-dispatch"],
            "targetAddress": target,
            "returnAddress": function["opcodeDispatchReturnAddress"],
        }
        for role, target in dispatch_targets.items()
    }
    roles_by_phase["csc06-entry"] = {
        "csc06-entry/direct-dispatch": {
            "callSiteAddress": function["opcodeDispatchCallAddress"],
            "targetAddress": function["csc06DoNothingAddress"],
            "returnAddress": function["opcodeDispatchReturnAddress"],
        },
        "csc06-entry/subroutine-target": {
            "callSiteAddress": function["subroutineIndirectCallAddress"],
            "targetAddress": function["csc06DoNothingAddress"],
            "returnAddress": function["subroutineResumeAddress"],
        },
    }
    expectations: dict[str, dict[str, dict[str, int | None]]] = {}
    for phase, address in address_for_phase.items():
        key = str(address)
        if key in expectations:
            raise ValueError("map-script control/audio duplicate physical callback PC")
        expectations[key] = {"roles": roles_by_phase[phase]}
    return failure_roles, expectations


def _validate_failure_expectations(
    failure_roles: dict[str, dict[str, str]],
    expectations: dict[str, dict[str, dict[str, int | None]]],
    function: dict[str, int],
    instrumentation: dict[str, Any],
    service: dict[str, int],
    cases: list[dict[str, Any]],
) -> None:
    """Reject missing, additional, or incorrect generated case-role diagnostics."""
    expected_roles, expected_expectations = _failure_expectation_contract(
        function, instrumentation, service, cases
    )
    if failure_roles != expected_roles:
        raise ValueError("map-script control/audio failure-role routing drift")
    if expectations != expected_expectations:
        raise ValueError("map-script control/audio failure expectation contract drift")


def _failure_expectations(
    function: dict[str, int],
    instrumentation: dict[str, Any],
    service: dict[str, int],
    cases: list[dict[str, Any]],
) -> dict[str, dict[str, dict[str, int | None]]]:
    """Give each physical callback PC source-derived, case-role diagnostics."""
    failure_roles, expectations = _failure_expectation_contract(
        function, instrumentation, service, cases
    )
    _validate_failure_expectations(
        failure_roles, expectations, function, instrumentation, service, cases
    )
    return expectations


def _instrument_rom(rom_path: Path, fixture: dict[str, Any]) -> Path:
    """Build the source-guarded trampoline without widening the shared adapter contract."""
    patch = fixture["instrumentation"]
    if bytes.fromhex(patch["stubHex"]) != _expected_trampoline_stub(fixture):
        raise ValueError("map-script control/audio preserved trampoline shape drifted")
    generic_fixture = {
        **fixture,
        "instrumentation": {
            **patch,
            "postHandlerAddress": patch["trampolinePostHandlerAddress"],
        },
    }
    return _instrument_map_lifecycle_rom(rom_path, generic_fixture)


def verify_map_script_control_audio(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 180
) -> dict[str, Any]:
    """Run six bounded cases through one original map-script interpreter launch."""
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner="map-script control/audio runtime fixture")
    verify_runtime_contract(fixture, rom_path)
    h2_fixture = load_json(H2_FIXTURE)
    static = build_map_script_engine_contract(rom_path, upstream_path)
    _validate_static_boundary(static, h2_fixture)
    if fixture["runtimeQuestions"] != list(RUNTIME_QUESTIONS):
        raise ValueError("map-script control/audio runtime-question fixture drift")
    if fixture["provenance"] != {
        "upstreamRepository": static["upstream"]["repository"],
        "upstreamBranch": "master",
        "upstreamCommit": static["upstream"]["commit"],
        "h2FixturePath": "tests/fixtures/h2/map-script-engine-static-v1.json",
        "h2FixtureId": "sf2-map-script-engine-static-v1",
        "h2FieldPath": "expected.scriptControlCommandFacts",
        "command": "uv run sf2 h2 map-script-engine",
    }:
        raise ValueError("map-script control/audio provenance fixture drift")
    navigation = _runtime_navigation(static, fixture, upstream_path)
    derived = derive_case_expectations(static, fixture)
    instrumented = _instrument_rom(rom_path, fixture)
    harness = load_json(repo_path(fixture["sharedHarnessFixture"]))["harness"]
    runtime_cases = [
        {
            "id": case["id"],
            "questionId": case["questionId"],
            "kind": case["kind"],
            **derived_case,
        }
        for case, derived_case in zip(fixture["cases"], derived, strict=True)
    ]
    failure_roles = _failure_roles(runtime_cases)
    failure_expectations = _failure_expectations(
        navigation["function"],
        fixture["instrumentation"],
        navigation["service"],
        runtime_cases,
    )
    validate_json(
        failure_expectations,
        CALLBACK_AUDIT_SCHEMA,
        owner="map-script control/audio callback audit",
    )

    def observe() -> dict[str, Any]:
        return run_observer(
            rom_path=instrumented,
            observer_path=OBSERVER,
            config={
                "fixtureId": fixture["id"],
                "mapTestIndex": fixture["mapTestIndex"],
                "function": navigation["function"],
                "ram": navigation["ram"],
                "constants": navigation["constants"],
                "service": navigation["service"],
                "instrumentation": fixture["instrumentation"],
                "maxFrames": fixture["maxFrames"],
                "harness": harness,
                "cases": runtime_cases,
                "failureRoles": failure_roles,
                "failureExpectations": failure_expectations,
                "observerFailureContract": OBSERVER_FAILURE_CONTRACT,
            },
            output_name=OBSERVER_OUTPUT_NAME,
            timeout_seconds=timeout_seconds,
        )

    try:
        observed = _with_instrumented_rom_database(
            instrumented, "SF2 H3 instrumented map-script control/audio", observe
        )
    except RuntimeError as error:
        failure = _callback_failure_status(_observer_status_path())
        if failure is None:
            raise
        _observer_output_path().unlink(missing_ok=True)
        raise RuntimeError(
            f"{error}\nMap-script control/audio callback failure status:\n"
            f"{json.dumps(failure, sort_keys=True)}"
        ) from error
    _raise_for_callback_failure_status(_observer_status_path(), _observer_output_path())
    _assert_success_status(_observer_status_path())
    validate_json(
        observed,
        OBSERVATION_SCHEMA,
        owner="map-script control/audio runtime observation",
    )
    expected = {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["id"],
        "mapTest": fixture["mapTestIndex"],
        "recordOrder": [case["id"] for case in fixture["cases"]],
        "records": [row["expected"] for row in derived],
    }
    if observed != expected:
        raise ValueError(
            "map-script control/audio runtime matrix mismatch\n"
            f"expected={expected!r}\nobserved={observed!r}"
        )
    return {
        "Fixture": fixture["id"],
        "Cases": len(derived),
        "RuntimeQuestions": len(RUNTIME_QUESTIONS),
        "BizHawkLaunches": 1,
        "Instrumentation": "session-only map-init trampoline and RAM-owned script streams",
        "Status": "PASS",
    }
