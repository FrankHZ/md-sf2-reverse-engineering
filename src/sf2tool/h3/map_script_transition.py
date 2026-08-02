"""Static-first H3 preparation for the map-script transition command family.

This module deliberately separates the existing H2 command contract from the
runtime-only presentation question.  It parses the exact H1 instruction sites
used to enter the five handlers and their bounded external-service calls before
the BizHawk observer is allowed to run.
"""

from __future__ import annotations

import json
import re
from pathlib import Path
from typing import Any

from sf2tool.h2.map_script_engine import build_map_script_engine_contract
from sf2tool.h3.bizhawk import DERIVED_ROOT, run_observer, verify_runtime_contract
from sf2tool.h3.map_lifecycle import _instrument_rom as _instrument_map_lifecycle_rom
from sf2tool.h3.map_lifecycle import _with_instrumented_rom_database
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path
from sf2tool.research_index import listing_symbol_addresses

H1_LISTING_PATH = Path("build/sf2build-h1.lst")
H2_FIXTURE = repo_path("tests/fixtures/h2/map-script-engine-static-v1.json")
FIXTURE = repo_path("tests/fixtures/h3/map-script-transition-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h3/h3-map-script-transition-fixture.schema.json")
OBSERVATION_SCHEMA = repo_path("schemas/h3/h3-map-script-transition-observation.schema.json")
OBSERVER = repo_path("tools/bizhawk/map_script_transition_observer.lua")
OBSERVER_OUTPUT_NAME = "map-script-transition"
OBSERVER_FAILURE_CONTRACT = {
    "exitCode": 1,
    "removeOutputBeforeExit": True,
    "statusPrefix": "failure:observer-callback:",
}
_FAILURE_FIELDS = {
    "actualPc",
    "caseId",
    "error",
    "expectedCallSiteAddress",
    "expectedReturnAddress",
    "expectedTargetAddress",
    "pendingCallback",
    "phase",
}
_PENDING_FIELDS = {
    "active",
    "dispatchTargetAddress",
    "handlerEntriesObserved",
    "pendingService",
    "phase",
    "role",
    "scriptWordReadCount",
}
_PENDING_SERVICE_FIELDS = {
    "callSiteAddress",
    "returnAddress",
    "role",
    "target",
    "targetAddress",
}
TRANSITION_MACROS = (
    "warp",
    "resetMap",
    "loadMapFadeIn",
    "reloadMap",
    "mapLoad",
)
HANDLER_BY_MACRO = {
    "warp": "csc07_warp",
    "resetMap": "csc36_resetMap",
    "loadMapFadeIn": "csc37_loadMapAndFadeIn",
    "reloadMap": "csc46_reloadMap",
    "mapLoad": "csc48_loadMap",
}
SERVICE_TARGETS = (
    "ResetCurrentMap",
    "LoadMapTilesets",
    "LoadMap",
    "EnableDisplayAndInterrupts",
    "WaitForVInt",
)
M68K_WORD_MASK = 0xFFFF


def _h1_section(listing: str, symbol: str) -> str:
    match = re.search(
        rf"^[0-9A-F]{{8}}\s+{re.escape(symbol)}:\s*$"
        rf"(?P<body>.*?)^\s*[0-9A-F]{{8}}\s+; End of function {re.escape(symbol)}\s*$",
        listing,
        re.MULTILINE | re.DOTALL,
    )
    if match is None:
        raise ValueError(f"map-script transition H1 section missing: {symbol}")
    return match.group("body")


def _h1_instruction_address(section: str, instruction: str, *, owner: str) -> int:
    """Find exactly one emitted instruction, excluding labels and comments."""
    addresses: list[int] = []
    for row in section.splitlines():
        match = re.match(r"^(?P<address>[0-9A-F]{8})\s+(?P<tail>.*)$", row)
        if match is None:
            continue
        tail = match.group("tail").strip()
        encoded = re.match(r"(?P<bytes>(?:[0-9A-F]{2,8}\s+)+)(?P<text>.*)$", tail)
        if encoded is None:
            continue
        normalized = re.sub(r"^M\s+", "", encoded.group("text").strip())
        normalized = re.sub(r"\s+", " ", normalized)
        if normalized == instruction:
            addresses.append(int(match.group("address"), 16))
    if len(addresses) != 1:
        raise ValueError(
            "map-script transition H1 instruction identity drift: "
            f"{owner}: {instruction!r} ({len(addresses)} matches)"
        )
    return addresses[0]


def _h1_followup_address(section: str, instruction: str, followup: str, *, owner: str) -> int:
    source = _h1_instruction_address(section, instruction, owner=owner)
    rows = section.splitlines()
    source_index: int | None = None
    for index, row in enumerate(rows):
        match = re.match(rf"^{source:08X}\s+(?P<tail>.*)$", row)
        if match is None:
            continue
        encoded = re.match(
            r"(?P<bytes>(?:[0-9A-F]{2,8}\s+)+)(?P<text>.*)$",
            match.group("tail").strip(),
        )
        if encoded is None:
            continue
        normalized = re.sub(r"^M\s+", "", encoded.group("text").strip())
        if re.sub(r"\s+", " ", normalized) == instruction:
            source_index = index
            break
    if source_index is None:
        raise ValueError(f"map-script transition H1 source row missing: {owner}")
    for row in rows[source_index + 1 :]:
        match = re.match(r"^(?P<address>[0-9A-F]{8})\s+(?P<tail>.*)$", row)
        if match is None:
            continue
        tail = match.group("tail").strip()
        encoded = re.match(r"(?P<bytes>(?:[0-9A-F]{2,8}\s+)+)(?P<text>.*)$", tail)
        if encoded is None:
            continue
        normalized = re.sub(r"^M\s+", "", encoded.group("text").strip())
        normalized = re.sub(r"\s+", " ", normalized)
        if normalized != followup:
            raise ValueError(
                "map-script transition H1 followup drift: "
                f"{owner}: expected {followup!r}, got {normalized!r}"
            )
        return int(match.group("address"), 16)
    raise ValueError(f"map-script transition H1 followup missing: {owner}")


def _h1_label_address(section: str, label: str, *, owner: str) -> int:
    matches = re.findall(
        rf"^(?P<address>[0-9A-F]{{8}})\s+{re.escape(label)}:\s*$",
        section,
        re.MULTILINE,
    )
    if len(matches) != 1:
        raise ValueError(f"map-script transition H1 label identity drift: {owner}")
    return int(matches[0], 16)


def _parse_equates(upstream_path: Path, names: tuple[str, ...]) -> dict[str, int]:
    values: dict[str, int] = {}
    for relative in ("disasm/sf2const.asm", "disasm/sf2enums.asm"):
        source = (upstream_path / relative).read_text(encoding="utf-8")
        for match in re.finditer(
            r"^(?P<name>[A-Za-z_][A-Za-z0-9_]*):\s+equ\s+"
            r"(?P<value>\$?[0-9A-Fa-f]+)\b",
            source,
            re.MULTILINE,
        ):
            name = match.group("name")
            if name not in names:
                continue
            token = match.group("value")
            value = int(token.removeprefix("$"), 16 if token.startswith("$") else 10)
            previous = values.setdefault(name, value)
            if previous != value:
                raise ValueError(f"map-script transition equate conflict: {name}")
    missing = [name for name in names if name not in values]
    if missing:
        raise ValueError(f"map-script transition equates missing: {missing}")
    return {name: values[name] for name in names}


def _handler_row(facts: dict[str, Any], macro: str) -> dict[str, Any]:
    rows = [row for row in facts["handlers"] if row["macro"] == macro]
    if len(rows) != 1:
        raise ValueError(f"map-script transition handler inventory drift: {macro}")
    return rows[0]


def _validate_static_boundary(static: dict[str, Any]) -> tuple[dict[str, Any], dict[str, Any]]:
    """Tie all five macro ABIs to their H2 handler-control-flow records."""
    transition = static.get("transitionCommandFacts")
    lifecycle = static.get("mapLifecycleCommandFacts")
    if not isinstance(transition, dict) or not isinstance(lifecycle, dict):
        raise ValueError("map-script transition H2 facts are missing")
    macros = transition.get("macros")
    handlers = transition.get("handlers")
    if not isinstance(macros, list) or not isinstance(handlers, list):
        raise ValueError("map-script transition H2 macro/handler facts are missing")
    macro_by_name = {row.get("name"): row for row in macros}
    handler_by_macro = {row.get("macro"): row for row in handlers}
    if tuple(row.get("name") for row in macros) != TRANSITION_MACROS:
        raise ValueError("map-script transition macro order drift")
    if set(macro_by_name) != set(TRANSITION_MACROS) or set(handler_by_macro) != set(
        TRANSITION_MACROS
    ):
        raise ValueError("map-script transition macro inventory drift")
    lifecycle_by_macro = {row["macro"]: row for row in lifecycle.get("handlers", [])}
    for macro in TRANSITION_MACROS:
        abi = macro_by_name[macro]
        handler = handler_by_macro[macro]
        if (
            abi.get("handler") != HANDLER_BY_MACRO[macro]
            or handler.get("handler") != HANDLER_BY_MACRO[macro]
            or abi.get("opcode") != handler.get("opcode")
            or abi.get("encodedBytes") != abi.get("operandBytes", -1) + 2
        ):
            raise ValueError(f"map-script transition ABI/handler binding drift: {macro}")
        if macro != "warp":
            lifecycle_row = lifecycle_by_macro.get(macro)
            if lifecycle_row is None or lifecycle_row.get("address") != handler.get("address"):
                raise ValueError(f"map-script transition lifecycle join drift: {macro}")
    warp = handler_by_macro["warp"]
    if warp.get("cursorReadWidths") != [1, 1, 1, 1] or warp.get("mapEventTypeValue") != 1:
        raise ValueError("map-script transition warp use-site drift")
    fade = handler_by_macro["loadMapFadeIn"]
    if fade.get("fallsThroughTo") != HANDLER_BY_MACRO["mapLoad"]:
        raise ValueError("map-script transition fade fall-through drift")
    caller_breakdown = transition.get("callerBreakdown")
    targets = (
        "ResetCurrentMap",
        "LoadMapTilesets",
        "LoadMap",
        "EnableDisplayAndInterrupts",
    )
    expected_totals = {
        "ResetCurrentMap": 1,
        "LoadMapTilesets": 1,
        "LoadMap": 2,
        "EnableDisplayAndInterrupts": 2,
    }
    expected_resolutions = [
        {
            "instructionTarget": target,
            "effectiveTarget": target,
            "effectiveTargetScope": "external",
        }
        for target in targets
    ]
    if (
        not isinstance(caller_breakdown, dict)
        or caller_breakdown.get("instructionTargetTotals") != expected_totals
        or caller_breakdown.get("effectiveTargetTotals") != expected_totals
        or caller_breakdown.get("internalEffectiveTargetTotals")
        != {target: 0 for target in targets}
        or caller_breakdown.get("externalEffectiveTargetTotals") != expected_totals
        or caller_breakdown.get("targetResolutions") != expected_resolutions
    ):
        raise ValueError("map-script transition caller target-resolution drift")
    return transition, lifecycle


def _listing_call_sites(
    listing: str, symbol: str, targets: list[str]
) -> list[dict[str, int | str]]:
    """Inventory only emitted, direct-symbol JSR instructions in one H1 section."""
    section = _h1_section(listing, symbol)
    rows: list[dict[str, int | str]] = []
    for row in section.splitlines():
        address_match = re.match(r"^(?P<address>[0-9A-F]{8})\s+(?P<tail>.*)$", row)
        if address_match is None:
            continue
        emitted = re.match(
            r"(?P<bytes>(?:[0-9A-F]{2,8}\s+)+)(?P<instruction>.*)$",
            address_match.group("tail").strip(),
        )
        if emitted is None:
            continue
        instruction = re.sub(r"^M\s+", "", emitted.group("instruction").strip())
        instruction = re.sub(r"\s+", " ", instruction)
        call = re.fullmatch(
            r"jsr (?:\((?P<parenthesized>[A-Za-z_][A-Za-z0-9_]*)\)|"
            r"(?P<bare>[A-Za-z_][A-Za-z0-9_]*))(?:\.(?:w|l))?",
            instruction,
        )
        if call is None:
            continue
        rows.append(
            {
                "address": int(address_match.group("address"), 16),
                "target": call.group("parenthesized") or call.group("bare"),
            }
        )
    if [row["target"] for row in rows] != targets:
        raise ValueError(f"map-script transition call-site target/order drift: {symbol}")
    if len({row["address"] for row in rows}) != len(rows):
        raise ValueError(f"map-script transition duplicate H1 call site: {symbol}")
    return rows


def _reset_tail_navigation(listing: str) -> dict[str, Any]:
    """Resolve ResetCurrentMap's bounded LoadMap tail and its shared entry seams."""
    reset = _h1_section(listing, "ResetCurrentMap")
    load_map = _h1_section(listing, "LoadMap")
    nested = []
    for target in ("EnableDisplayAndInterrupts", "WaitForVInt"):
        instruction = f"bsr.w {target}"
        address = _h1_instruction_address(
            load_map, instruction, owner=f"LoadMap {target} tail"
        )
        nested.append(
            {
                "address": address,
                "target": target,
                "returnAddress": _next_instruction_address(listing, address),
            }
        )
    return {
        "branchAddress": _h1_instruction_address(
            reset, "bra.w LoadMap", owner="ResetCurrentMap LoadMap tail"
        ),
        "nestedServiceSites": nested,
    }


def runtime_navigation(static: dict[str, Any], upstream_path: Path) -> dict[str, Any]:
    """Derive source/H1 navigation for the exact five transition forms."""
    transition, lifecycle = _validate_static_boundary(static)
    listing = (upstream_path / H1_LISTING_PATH).read_text(encoding="utf-8")
    symbols = listing_symbol_addresses(listing)
    main = _h1_section(listing, "ExecuteMapScript")
    wrapper = _h1_section(listing, "RunMapSetupInitFunction")
    handler_by_macro = {row["macro"]: row for row in transition["handlers"]}
    for macro, symbol in HANDLER_BY_MACRO.items():
        if symbols.get(symbol) != handler_by_macro[macro]["address"]:
            raise ValueError(f"map-script transition H1 handler address drift: {macro}")
    function = {
        "entryAddress": symbols["RunMapSetupInitFunction"],
        "entryInjectionCallSiteAddress": _h1_instruction_address(
            wrapper, "jsr (a0)", owner="wrapper"
        ),
        "executeMapScriptAddress": symbols["ExecuteMapScript"],
        "scriptWordReadAfterAddress": _h1_followup_address(
            main, "move.w (a6)+,d0", "cmpi.w #-1,d0", owner="script word read"
        ),
        "opcodeDispatchCallAddress": _h1_instruction_address(
            main, "jsr rjt_cutsceneScriptCommands(pc,d0.w)", owner="opcode dispatch"
        ),
        "opcodeDispatchReturnAddress": _h1_followup_address(
            main,
            "jsr rjt_cutsceneScriptCommands(pc,d0.w)",
            "bra.s loc_47140",
            owner="opcode dispatch",
        ),
        "endAddress": _h1_label_address(main, "loc_47234", owner="script end"),
        "warpHandlerAddress": handler_by_macro["warp"]["address"],
        "resetHandlerAddress": handler_by_macro["resetMap"]["address"],
        "fadeHandlerAddress": handler_by_macro["loadMapFadeIn"]["address"],
        "reloadHandlerAddress": handler_by_macro["reloadMap"]["address"],
        "mapLoadHandlerAddress": handler_by_macro["mapLoad"]["address"],
    }
    lifecycle_by_macro = {row["macro"]: row for row in lifecycle["handlers"]}
    call_sites: dict[str, list[dict[str, int | str]]] = {"warp": []}
    for macro in ("resetMap", "loadMapFadeIn", "reloadMap", "mapLoad"):
        row = lifecycle_by_macro[macro]
        physical = row["continuation"] if macro == "loadMapFadeIn" else row
        if physical is None:
            raise ValueError("map-script transition fade continuation missing")
        targets = [call["instructionTarget"] for call in physical["directCalls"]]
        call_sites[macro] = _listing_call_sites(listing, physical["handler"], targets)
    service = {name: symbols[name] for name in SERVICE_TARGETS}
    equates = _parse_equates(
        upstream_path,
        (
            "CURRENT_MAP",
            "VIEW_TARGET_ENTITY",
            "FADING_SETTING",
            "OUT_TO_BLACK",
            "MAP_EVENT_TYPE",
            "MAP_EVENT_PARAM_1",
            "VIEW_PLANE_A_PIXEL_X",
            "VIEW_PLANE_A_PIXEL_Y",
        ),
    )
    ram = {
        "currentMapAddress": equates["CURRENT_MAP"],
        "viewTargetEntityAddress": equates["VIEW_TARGET_ENTITY"],
        "fadingSettingAddress": equates["FADING_SETTING"],
        "mapEventTypeAddress": equates["MAP_EVENT_TYPE"],
        "mapEventParam1Address": equates["MAP_EVENT_PARAM_1"],
        "viewPlaneAPixelXAddress": equates["VIEW_PLANE_A_PIXEL_X"],
        "viewPlaneAPixelYAddress": equates["VIEW_PLANE_A_PIXEL_Y"],
    }
    fade = _h1_section(listing, "csc37_loadMapAndFadeIn")
    fade_source_write = {
        "address": _h1_instruction_address(
            fade,
            "move.b #OUT_TO_BLACK,((FADING_SETTING-$1000000)).w",
            owner="csc37 fade setting write",
        ),
        "symbol": "OUT_TO_BLACK",
        "value": equates["OUT_TO_BLACK"],
    }
    return {
        "function": function,
        "service": service,
        "ram": ram,
        "callSites": call_sites,
        "fadeSourceWrite": fade_source_write,
        "resetTail": _reset_tail_navigation(listing),
    }


def _word_bytes(value: int) -> list[int]:
    if not isinstance(value, int) or isinstance(value, bool) or not 0 <= value <= M68K_WORD_MASK:
        raise ValueError("map-script transition script word boundary drift")
    return list(value.to_bytes(2, "big"))


def derive_case_expectations(
    static: dict[str, Any], cases: list[dict[str, Any]]
) -> list[dict[str, Any]]:
    """Build complete source-derived script bytes and cursor/service expectations."""
    transition, lifecycle = _validate_static_boundary(static)
    abi_by_macro = {row["name"]: row for row in transition["macros"]}
    handler_by_macro = {row["macro"]: row for row in transition["handlers"]}
    lifecycle_by_macro = {row["macro"]: row for row in lifecycle["handlers"]}
    output: list[dict[str, Any]] = []
    seen: set[str] = set()
    for case in cases:
        case_id = case.get("id")
        macro = case.get("macro")
        operands = case.get("operandWords")
        if not isinstance(case_id, str) or case_id in seen or macro not in TRANSITION_MACROS:
            raise ValueError("map-script transition case identity/macro drift")
        if not isinstance(operands, list) or not all(
            isinstance(value, int) and not isinstance(value, bool) for value in operands
        ):
            raise ValueError(f"map-script transition operand shape drift: {case_id}")
        seen.add(case_id)
        abi = abi_by_macro[macro]
        if abi["operandBytes"] != 2 * len(operands) and macro != "warp":
            raise ValueError(f"map-script transition operand-width drift: {case_id}")
        if macro == "warp" and (
            len(operands) != 4 or not all(0 <= value <= 0xFF for value in operands)
        ):
            raise ValueError(f"map-script transition warp-byte ABI drift: {case_id}")
        if macro != "warp" and not all(0 <= value <= M68K_WORD_MASK for value in operands):
            raise ValueError(f"map-script transition operand-word boundary drift: {case_id}")
        bytes_out = _word_bytes(abi["opcode"])
        if macro == "warp":
            bytes_out.extend(operands)
        else:
            for operand in operands:
                bytes_out.extend(_word_bytes(operand))
        bytes_out.extend(_word_bytes(0xFFFF))
        lifecycle_row = lifecycle_by_macro.get(macro)
        direct_calls = [] if lifecycle_row is None else lifecycle_row["directCalls"]
        handler_entries = ["ExecuteMapScript", handler_by_macro[macro]["handler"]]
        if macro == "loadMapFadeIn":
            handler_entries.append(HANDLER_BY_MACRO["mapLoad"])
            direct_calls = lifecycle_row["continuation"]["directCalls"]
        output.append(
            {
                "id": case_id,
                "macro": macro,
                "scriptBytes": bytes_out,
                "expected": {
                    "id": case_id,
                    "macro": macro,
                    "handlerEntries": handler_entries,
                    "scriptWordReads": [
                        {"word": abi["opcode"], "cursorAfterReadOffset": 2},
                        {"word": 0xFFFF, "cursorAfterReadOffset": abi["encodedBytes"] + 2},
                    ],
                    "cursorAfterHandlerOffset": abi["encodedBytes"],
                    "handlerReturned": True,
                    "fallthroughCsc48Observed": macro == "loadMapFadeIn",
                    "serviceCallOrder": [row["instructionTarget"] for row in direct_calls],
                },
            }
        )
    if tuple(row["macro"] for row in output) != TRANSITION_MACROS:
        raise ValueError("map-script transition representative case order drift")
    return output


def _observer_status_path() -> Path:
    return DERIVED_ROOT / f"{OBSERVER_OUTPUT_NAME}.status.txt"


def _observer_output_path() -> Path:
    return DERIVED_ROOT / f"{OBSERVER_OUTPUT_NAME}.observed.json"


def _callback_failure_status(status_path: Path) -> dict[str, Any] | None:
    """Parse the observer failure sentinel with no lossy pending-state fields."""
    if not status_path.is_file():
        return None
    rows = [
        line.removeprefix(OBSERVER_FAILURE_CONTRACT["statusPrefix"])
        for line in status_path.read_text(encoding="utf-8").splitlines()
        if line.startswith(OBSERVER_FAILURE_CONTRACT["statusPrefix"])
    ]
    if not rows:
        return None
    if len(rows) != 1:
        raise ValueError("map-script transition callback failure multiplicity drift")
    try:
        payload = json.loads(rows[0])
    except json.JSONDecodeError as error:
        raise ValueError("map-script transition callback failure JSON drift") from error
    if not isinstance(payload, dict) or set(payload) != _FAILURE_FIELDS:
        raise ValueError("map-script transition callback failure field-set drift")
    if payload["caseId"] is not None and (
        not isinstance(payload["caseId"], str) or not payload["caseId"]
    ):
        raise ValueError("map-script transition callback failure case identity drift")
    if not isinstance(payload["phase"], str) or not isinstance(payload["error"], str):
        raise ValueError("map-script transition callback failure text drift")
    for name in (
        "actualPc",
        "expectedCallSiteAddress",
        "expectedTargetAddress",
        "expectedReturnAddress",
    ):
        value = payload[name]
        if value is not None and (not isinstance(value, int) or isinstance(value, bool)):
            raise ValueError(f"map-script transition callback failure {name} drift")
    pending = payload["pendingCallback"]
    if not isinstance(pending, dict) or set(pending) != _PENDING_FIELDS:
        raise ValueError("map-script transition callback pending field-set drift")
    if not isinstance(pending["active"], bool):
        raise ValueError("map-script transition callback pending active drift")
    for name in ("phase", "role"):
        if not isinstance(pending[name], str) or not pending[name]:
            raise ValueError(f"map-script transition callback pending {name} drift")
    dispatch_target = pending["dispatchTargetAddress"]
    if dispatch_target is not None and (
        not isinstance(dispatch_target, int) or isinstance(dispatch_target, bool)
    ):
        raise ValueError("map-script transition callback pending dispatchTargetAddress drift")
    script_word_count = pending["scriptWordReadCount"]
    if (
        not isinstance(script_word_count, int)
        or isinstance(script_word_count, bool)
        or script_word_count < 0
    ):
        raise ValueError("map-script transition callback pending script-word count drift")
    entries = pending["handlerEntriesObserved"]
    if not isinstance(entries, list) or not all(isinstance(value, str) for value in entries):
        raise ValueError("map-script transition callback pending handler entries drift")
    service = pending["pendingService"]
    if service is not None:
        if not isinstance(service, dict) or set(service) != _PENDING_SERVICE_FIELDS:
            raise ValueError("map-script transition callback pending service field-set drift")
        if not isinstance(service["target"], str) or not service["target"]:
            raise ValueError("map-script transition callback pending service target drift")
        if not isinstance(service["role"], str) or not service["role"]:
            raise ValueError("map-script transition callback pending service role drift")
        for name in ("callSiteAddress", "returnAddress", "targetAddress"):
            if not isinstance(service[name], int) or isinstance(service[name], bool):
                raise ValueError(f"map-script transition callback pending service {name} drift")
    return payload


def _assert_success_status(status_path: Path) -> None:
    if not status_path.is_file():
        raise RuntimeError("map-script transition observer wrote no status record")
    if _callback_failure_status(status_path) is not None:
        raise RuntimeError("map-script transition observer reported callback failure")
    lines = status_path.read_text(encoding="utf-8").splitlines()
    if lines[-2:] != ["milestone:callbacks-cleared:0", "milestone:observer-finished"]:
        raise RuntimeError("map-script transition callback cleanup milestone drift")


def _h1_instruction_bytes(section: str, instruction: str, *, owner: str) -> bytes:
    rows: list[bytes] = []
    for row in section.splitlines():
        match = re.match(r"^[0-9A-F]{8}\s+(?P<tail>.*)$", row)
        if match is None:
            continue
        encoded = re.match(
            r"(?P<bytes>(?:[0-9A-F]{2,8}\s+)+)(?P<text>.*)$",
            match.group("tail").strip(),
        )
        if encoded is None:
            continue
        normalized = re.sub(r"^M\s+", "", encoded.group("text").strip())
        if re.sub(r"\s+", " ", normalized) == instruction:
            rows.append(bytes.fromhex("".join(encoded.group("bytes").split())))
    if len(rows) != 1:
        raise ValueError(f"map-script transition H1 byte identity drift: {owner}")
    return rows[0]


def _expected_trampoline_stub(fixture: dict[str, Any]) -> bytes:
    patch = fixture["instrumentation"]
    return (
        b"\x20\x7c"
        + (patch["ramInputAddress"] + 4).to_bytes(4, "big")
        + b"\x4e\xb9"
        + fixture["function"]["executeMapScriptAddress"].to_bytes(4, "big")
        + b"\x58\x8f\x4c\xdf\x03\xff\x4e\x75"
    )


def _validate_trampoline(listing: str, fixture: dict[str, Any]) -> None:
    """Guard the 20-byte wrapper-preserving trampoline against source ABI drift."""
    patch = fixture["instrumentation"]
    wrapper = _h1_section(listing, "RunMapSetupInitFunction")
    call = _h1_instruction_address(wrapper, "jsr (a0)", owner="wrapper injection")
    restore = _h1_followup_address(
        wrapper, "jsr (a0)", "movem.l (sp)+,d0-a1", owner="wrapper restore"
    )
    retained_return = _h1_followup_address(
        wrapper, "movem.l (sp)+,d0-a1", "rts", owner="wrapper return"
    )
    call_bytes = _h1_instruction_bytes(wrapper, "jsr (a0)", owner="wrapper injection")
    restore_bytes = _h1_instruction_bytes(wrapper, "movem.l (sp)+,d0-a1", owner="wrapper restore")
    rts_bytes = _h1_instruction_bytes(wrapper, "rts", owner="wrapper return")
    if (
        call != patch["callSiteAddress"]
        or bytes.fromhex(patch["callSiteOriginalHex"]) != call_bytes + restore_bytes
        or retained_return != restore + len(restore_bytes)
        or bytes.fromhex(patch["callSitePatchedHex"])
        != b"\x4e\xb9" + patch["stubAddress"].to_bytes(4, "big")
        or bytes.fromhex(patch["stubHex"]) != _expected_trampoline_stub(fixture)
        or bytes.fromhex(patch["stubOriginalHex"])
        != b"\xff" * len(_expected_trampoline_stub(fixture))
        or patch["trampolinePostHandlerAddress"]
        != patch["stubAddress"] + len(_expected_trampoline_stub(fixture)) - len(rts_bytes)
    ):
        raise ValueError("map-script transition wrapper-preserving trampoline drift")


def _next_instruction_address(listing: str, address: int) -> int:
    """Get one H1 JSR's actual return PC from the next emitted instruction."""
    rows: list[int] = []
    for row in listing.splitlines():
        match = re.match(r"^(?P<address>[0-9A-F]{8})\s+(?P<tail>[0-9A-F]{2,8}\s+)", row)
        if match is not None:
            value = int(match.group("address"), 16)
            if value > address:
                rows.append(value)
    if not rows:
        raise ValueError(f"map-script transition H1 return PC missing: {address}")
    return min(rows)


def _runtime_cases(
    static: dict[str, Any],
    fixture: dict[str, Any],
    navigation: dict[str, Any],
    listing: str,
) -> list[dict[str, Any]]:
    derived = derive_case_expectations(static, fixture["cases"])
    runtime_cases: list[dict[str, Any]] = []
    for source, derived_case in zip(fixture["cases"], derived, strict=True):
        expected = {**derived_case["expected"], **source["runtimeGolden"]}
        if source["expected"] != expected:
            raise ValueError(
                f"map-script transition fixture/static expectation drift: {source['id']}"
            )
        sites = [
            {**site, "returnAddress": _next_instruction_address(listing, site["address"])}
            for site in navigation["callSites"][source["macro"]]
        ]
        runtime_cases.append(
            {**source, **derived_case, "expected": expected, "serviceSites": sites}
        )
    return runtime_cases


def _failure_expectations(
    navigation: dict[str, Any], fixture: dict[str, Any], cases: list[dict[str, Any]], listing: str
) -> dict[str, dict[str, dict[str, int | None]]]:
    """Generate one exact role map per physical PC, including shared service seams."""
    function, service = navigation["function"], navigation["service"]
    instrumentation = fixture["instrumentation"]
    patched_jsr_length = len(bytes.fromhex(instrumentation["callSitePatchedHex"]))
    stub_jsr_offset = bytes.fromhex(instrumentation["stubHex"]).index(b"\x4e\xb9")
    stub_jsr_address = instrumentation["stubAddress"] + stub_jsr_offset
    result: dict[str, dict[str, dict[str, int | None]]] = {}

    def add(
        address: int, role: str, call: int | None, target: int | None, returned: int | None
    ) -> None:
        roles = result.setdefault(str(address), {"roles": {}})["roles"]
        if role in roles:
            raise ValueError(f"map-script transition duplicate callback role: {address}:{role}")
        roles[role] = {"callSiteAddress": call, "targetAddress": target, "returnAddress": returned}

    add(function["entryAddress"], "wrapper-entry", None, None, None)
    add(
        instrumentation["stubAddress"],
        "trampoline-entry",
        function["entryInjectionCallSiteAddress"],
        instrumentation["stubAddress"],
        function["entryInjectionCallSiteAddress"] + patched_jsr_length,
    )
    add(
        function["executeMapScriptAddress"],
        "execute-entry",
        stub_jsr_address,
        function["executeMapScriptAddress"],
        stub_jsr_address + patched_jsr_length,
    )
    add(
        function["scriptWordReadAfterAddress"],
        "script-word-read",
        None,
        None,
        None,
    )
    add(function["endAddress"], "script-end", None, None, None)
    add(
        instrumentation["trampolinePostHandlerAddress"],
        "trampoline-complete",
        None,
        None,
        None,
    )
    for case in cases:
        macro, case_id = case["macro"], case["id"]
        handler_key = {
            "warp": "warpHandlerAddress",
            "resetMap": "resetHandlerAddress",
            "loadMapFadeIn": "fadeHandlerAddress",
            "reloadMap": "reloadHandlerAddress",
            "mapLoad": "mapLoadHandlerAddress",
        }[macro]
        handler = function[handler_key]
        add(
            function["opcodeDispatchCallAddress"],
            f"dispatch:{case_id}",
            function["opcodeDispatchCallAddress"],
            handler,
            function["opcodeDispatchReturnAddress"],
        )
        add(
            function["opcodeDispatchReturnAddress"],
            f"dispatcher-return:{case_id}",
            function["opcodeDispatchCallAddress"],
            handler,
            function["opcodeDispatchReturnAddress"],
        )
        add(
            handler,
            f"handler:{case_id}",
            function["opcodeDispatchCallAddress"],
            handler,
            function["opcodeDispatchReturnAddress"],
        )
        if macro == "loadMapFadeIn":
            add(
                function["mapLoadHandlerAddress"],
                f"fallthrough:{case_id}",
                None,
                function["mapLoadHandlerAddress"],
                function["opcodeDispatchReturnAddress"],
            )
        for index, site in enumerate(case["serviceSites"], start=1):
            target_name = site["target"]
            target = service[target_name]
            returned = site["returnAddress"]
            base = f"service:{case_id}:{index}:{target_name}"
            add(site["address"], f"{base}:call", site["address"], target, returned)
            add(target, f"{base}:entry", site["address"], target, returned)
            add(returned, f"{base}:return", site["address"], target, returned)
            if target_name == "LoadMap":
                for nested in navigation["resetTail"]["nestedServiceSites"]:
                    nested_name = nested["target"]
                    add(
                        service[nested_name],
                        f"{base}:nested:{nested_name}:entry",
                        nested["address"],
                        service[nested_name],
                        nested["returnAddress"],
                    )
        if macro == "resetMap":
            reset_site = case["serviceSites"][0]
            add(
                service["LoadMap"],
                f"reset-tail:LoadMap:entry:{case_id}",
                navigation["resetTail"]["branchAddress"],
                service["LoadMap"],
                reset_site["returnAddress"],
            )
            for nested in navigation["resetTail"]["nestedServiceSites"]:
                target_name = nested["target"]
                add(
                    service[target_name],
                    f"reset-tail:{target_name}:entry:{case_id}",
                    nested["address"],
                    service[target_name],
                    nested["returnAddress"],
                )
    return result


def _validate_failure_expectations(
    actual: dict[str, dict[str, dict[str, int | None]]],
    navigation: dict[str, Any],
    fixture: dict[str, Any],
    cases: list[dict[str, Any]],
    listing: str,
) -> None:
    """Reject missing, additional, or stale case/role diagnostic routes."""
    expected = _failure_expectations(navigation, fixture, cases, listing)
    if actual != expected:
        raise ValueError("map-script transition callback expectation contract drift")


def _instrument_rom(rom_path: Path, fixture: dict[str, Any]) -> Path:
    generic_fixture = {
        **fixture,
        "instrumentation": {
            **fixture["instrumentation"],
            "postHandlerAddress": fixture["instrumentation"]["trampolinePostHandlerAddress"],
        },
    }
    return _instrument_map_lifecycle_rom(rom_path, generic_fixture)


def verify_map_script_transition(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 180
) -> dict[str, Any]:
    """Run the one-launch, five-case transition command/service boundary matrix."""
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner="map-script transition runtime fixture")
    verify_runtime_contract(fixture, rom_path)
    static = build_map_script_engine_contract(rom_path, upstream_path)
    _validate_static_boundary(static)
    if fixture["runtimeQuestions"] != ["map-script-transition-presentation-matrix"]:
        raise ValueError("map-script transition runtime question queue drift")
    navigation = runtime_navigation(static, upstream_path)
    if (
        fixture["function"] != navigation["function"]
        or fixture["service"] != navigation["service"]
        or fixture["ram"] != navigation["ram"]
        or fixture["fadeSourceWrite"] != navigation["fadeSourceWrite"]
    ):
        raise ValueError("map-script transition fixture/H1 navigation drift")
    listing = (upstream_path / H1_LISTING_PATH).read_text(encoding="utf-8")
    _validate_trampoline(listing, fixture)
    cases = _runtime_cases(static, fixture, navigation, listing)
    expectations = _failure_expectations(navigation, fixture, cases, listing)
    instrumented = _instrument_rom(rom_path, fixture)
    harness = load_json(repo_path(fixture["sharedHarnessFixture"]))["harness"]

    def observe() -> dict[str, Any]:
        return run_observer(
            rom_path=instrumented,
            observer_path=OBSERVER,
            config={
                "fixtureId": fixture["id"],
                "mapTestIndex": fixture["mapTestIndex"],
                "function": navigation["function"],
                "service": navigation["service"],
                "resetTail": navigation["resetTail"],
                "ram": navigation["ram"],
                "instrumentation": fixture["instrumentation"],
                "maxFrames": fixture["maxFrames"],
                "harness": harness,
                "cases": cases,
                "failureExpectations": expectations,
                "observerFailureContract": OBSERVER_FAILURE_CONTRACT,
            },
            output_name=OBSERVER_OUTPUT_NAME,
            timeout_seconds=timeout_seconds,
        )

    try:
        observed = _with_instrumented_rom_database(
            instrumented, "SF2 H3 instrumented map-script transition", observe
        )
    except RuntimeError as error:
        payload = _callback_failure_status(_observer_status_path())
        if payload is None:
            raise
        _observer_output_path().unlink(missing_ok=True)
        raise RuntimeError(
            f"{error}\nMap-script transition callback failure: "
            f"{json.dumps(payload, sort_keys=True)}"
        ) from error
    payload = _callback_failure_status(_observer_status_path())
    if payload is not None:
        _observer_output_path().unlink(missing_ok=True)
        raise RuntimeError(
            f"map-script transition callback failure: {json.dumps(payload, sort_keys=True)}"
        )
    _assert_success_status(_observer_status_path())
    validate_json(observed, OBSERVATION_SCHEMA, owner="map-script transition runtime observation")
    expected = {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["id"],
        "mapTest": fixture["mapTestIndex"],
        "recordOrder": [case["id"] for case in cases],
        "records": [case["expected"] for case in cases],
    }
    if observed != expected:
        raise ValueError(
            "map-script transition runtime matrix mismatch\n"
            f"expected={expected!r}\nobserved={observed!r}"
        )
    return {
        "Fixture": fixture["id"],
        "Cases": len(cases),
        "RuntimeQuestions": 1,
        "BizHawkLaunches": 1,
        "Instrumentation": (
            "session-only wrapper-preserving trampoline and RAM-owned map-script streams"
        ),
        "Status": "PASS",
    }
