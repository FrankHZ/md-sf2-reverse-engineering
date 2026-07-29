from __future__ import annotations

import hashlib
import re
from pathlib import Path
from typing import Any

from sf2tool.h2.map_script_engine import build_map_script_engine_contract
from sf2tool.h3.bizhawk import DERIVED_ROOT, bizhawk_contract, run_observer, verify_runtime_contract
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.rom import inspect_rom, mega_drive_checksum

FIXTURE = repo_path("tests/fixtures/h3/map-lifecycle-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h3-map-lifecycle-fixture.schema.json")
OBSERVATION_SCHEMA = repo_path("schemas/h3-map-lifecycle-observation.schema.json")
M68K_WORD_MASK = 0xFFFF
OBSERVER = repo_path("tools/bizhawk/map_lifecycle_observer.lua")


def _reset_current_map_load_followup(upstream_path: Path) -> dict[str, Any]:
    """Guard ResetCurrentMap's layout-clear loop and bounded LoadMap tail."""
    source_path = "code/gameflow/exploration/exploration.asm"
    source = (upstream_path / "disasm" / source_path).read_text(encoding="utf-8")
    section = re.search(
        r"^ResetCurrentMap:\s*\n(?P<body>.*?)^\s*; End of function ResetCurrentMap\s*$",
        source,
        re.MULTILINE | re.DOTALL,
    )
    if section is None:
        raise ValueError("map lifecycle ResetCurrentMap source section is missing")
    rows = [
        re.sub(r"\s+", " ", row.split(";", 1)[0].strip())
        for row in section.group("body").splitlines()
    ]
    rows = [row for row in rows if row and not row.endswith(":")]
    layout_clear_rows = [
        (index, re.fullmatch(r"clr\.(?P<size>[bwl]) \(a2\)\+", row))
        for index, row in enumerate(rows)
    ]
    layout_clear_rows = [
        (index, match) for index, match in layout_clear_rows if match is not None
    ]
    if len(layout_clear_rows) != 1:
        raise ValueError("map lifecycle ResetCurrentMap layout clear use-site drift")
    layout_clear_index, clear_match = layout_clear_rows[0]
    assert clear_match is not None
    try:
        layout_start_index = rows.index("lea (FF0000_RAM_START).l,a2")
        layout_counter_index = rows.index("move.w #MAP_LAYOUT_LONGS_COUNTER,d7")
        layout_loop_index = rows.index("dbf d7,@Clear_Loop")
        clear_index = rows.index("clr.w d0")
        selector_index = rows.index("moveq #-1,d1")
        transfer_index = rows.index("bra.w LoadMap")
    except ValueError as error:
        raise ValueError("map lifecycle ResetCurrentMap load use-site drift") from error
    if not (
        layout_start_index
        < layout_counter_index
        < layout_clear_index
        < layout_loop_index
        < clear_index
        < selector_index
        < transfer_index
    ):
        raise ValueError("map lifecycle ResetCurrentMap load order drift")
    if clear_match.group("size") != "l":
        raise ValueError("map lifecycle ResetCurrentMap layout clear width drift")
    clear_unit_bytes = {"b": 1, "w": 2, "l": 4}[clear_match.group("size")]
    return {
        "sourcePath": source_path,
        "layoutStartInstruction": rows[layout_start_index],
        "layoutCounterInstruction": rows[layout_counter_index],
        "layoutClearInstruction": rows[layout_clear_index],
        "layoutLoopInstruction": rows[layout_loop_index],
        "layoutClearUnitByteCount": clear_unit_bytes,
        "clearInstruction": rows[clear_index],
        "selectorInstruction": rows[selector_index],
        "transferInstruction": rows[transfer_index],
        "loadMapD0WordAtTransfer": 0,
        "loadMapD1WordAtTransfer": M68K_WORD_MASK,
    }


def _map_lifecycle_runtime_equates(upstream_path: Path) -> dict[str, int]:
    """Parse the authoritative RAM and loop constants used by this H3 observer."""
    required = (
        "FF0000_RAM_START",
        "VIEW_PLANE_A_PIXEL_X",
        "VIEW_PLANE_A_PIXEL_Y",
        "MAP_LAYOUT_LONGS_COUNTER",
        "CURRENT_MAP",
        "VIEW_TARGET_ENTITY",
        "FADING_SETTING",
    )
    values: dict[str, int] = {}
    for relative_path in ("disasm/sf2const.asm", "disasm/sf2enums.asm"):
        source = (upstream_path / relative_path).read_text(encoding="utf-8")
        for match in re.finditer(
            r"^(?P<name>[A-Za-z_][A-Za-z0-9_]*):\s+equ\s+"
            r"(?P<value>\$?[0-9A-Fa-f]+)\b",
            source,
            re.MULTILINE,
        ):
            name = match.group("name")
            if name not in required:
                continue
            encoded = match.group("value")
            value = int(encoded.removeprefix("$"), 16 if encoded.startswith("$") else 10)
            prior = values.setdefault(name, value)
            if prior != value:
                raise ValueError(f"map lifecycle source equate conflict: {name}")
    missing = [name for name in required if name not in values]
    if missing:
        raise ValueError(f"map lifecycle runtime source constants are missing: {missing}")
    return {name: values[name] for name in required}


def _handler_direct_target_order(macro: str, handler: dict[str, Any]) -> list[str]:
    if macro == "loadMapFadeIn":
        continuation = handler["continuation"]
        if continuation is None:
            raise ValueError("map lifecycle fade continuation is missing")
        calls = continuation["directCalls"]
    else:
        calls = handler["directCalls"]
    return [row["instructionTarget"] for row in calls]


def _pack_load_map_d0_word_at_call(
    *,
    macro: str,
    handler: dict[str, Any],
    operand_words: list[int],
) -> tuple[int, int | None, int | None]:
    """Derive the observed LoadMap word inputs from H2-guarded pack use sites."""
    if macro == "resetMap":
        raise ValueError("map lifecycle reset uses its bounded callee transfer")
    guard = handler["sectionGuard"]
    if macro == "loadMapFadeIn":
        continuation = handler["continuation"]
        if continuation is None:
            raise ValueError("map lifecycle fade continuation is missing")
        guard = continuation["sectionGuard"]
    pack = guard["operandPackUseSites"]
    if pack is None:
        raise ValueError(f"map lifecycle operand pack is missing: {macro}")
    ordinals = pack["parameterOrdinals"]
    if len(ordinals) != 2:
        raise ValueError(f"map lifecycle camera operand arity drift: {macro}")
    if not all(0 <= word <= M68K_WORD_MASK for word in operand_words):
        raise ValueError(f"map lifecycle operand is not a word: {macro}")
    try:
        x_word, y_word = (operand_words[index - 1] for index in ordinals)
    except IndexError as error:
        raise ValueError(f"map lifecycle operand ordinal drift: {macro}") from error
    shift = pack["shiftUseSite"]["value"]
    mask = pack["maskUseSite"]["value"]
    multiplier = pack["multiplierUseSite"]["value"]
    if not re.fullmatch(r"lsl\.w #[A-Za-z_][A-Za-z0-9_]*,d0", pack["shiftUseSite"]["instruction"]):
        raise ValueError(f"map lifecycle D0 shift width drift: {macro}")
    if not re.fullmatch(r"andi\.w #[A-Za-z_][A-Za-z0-9_]*,d2", pack["maskUseSite"]["instruction"]):
        raise ValueError(f"map lifecycle D2 mask width drift: {macro}")
    if pack["mergeInstruction"] != "or.w d2,d0":
        raise ValueError(f"map lifecycle D0 merge width drift: {macro}")
    if not re.fullmatch(r"mulu\.w #\d+,d0", pack["multiplierUseSite"]["instruction"]):
        raise ValueError(f"map lifecycle D0 multiply width drift: {macro}")
    shifted_x_word = (x_word << shift) & M68K_WORD_MASK
    masked_y_word = (y_word & mask) & M68K_WORD_MASK
    packed_word = (shifted_x_word | masked_y_word) & M68K_WORD_MASK
    load_map_d0_word_at_call = (packed_word * multiplier) & M68K_WORD_MASK
    if macro == "reloadMap":
        selector = guard["sourceD1SelectorUseSite"]
        if selector is None:
            raise ValueError("map lifecycle reload selector use-site is missing")
        load_map_d1 = selector["literalValue"] & M68K_WORD_MASK
        tileset_d1 = None
    else:
        load_map_d1 = operand_words[0] & M68K_WORD_MASK
        tileset_d1 = operand_words[0] & M68K_WORD_MASK
    return load_map_d0_word_at_call, load_map_d1, tileset_d1


def _derive_case_expectations(
    static: dict[str, Any], fixture: dict[str, Any], upstream_path: Path
) -> list[dict[str, Any]]:
    facts = static["mapLifecycleCommandFacts"]
    handlers = {row["macro"]: row for row in facts["handlers"]}
    macros = {row["name"]: row for row in facts["macros"]}
    reset_followup = _reset_current_map_load_followup(upstream_path)
    derived = []
    for case in fixture["cases"]:
        macro = case["macro"]
        handler = handlers.get(macro)
        abi = macros.get(macro)
        if handler is None or abi is None:
            raise ValueError(f"map lifecycle case uses unknown macro: {case['id']}")
        operand_words = case["operandWords"]
        if abi["operandBytes"] != 2 * len(operand_words):
            raise ValueError(f"map lifecycle operand width drift: {case['id']}")
        direct_target_order = _handler_direct_target_order(macro, handler)
        if macro == "resetMap":
            packed_d0 = None
            load_map_d1 = None
            tileset_d1 = None
            reset_tail_d0 = reset_followup["loadMapD0WordAtTransfer"]
            reset_tail_d1 = reset_followup["loadMapD1WordAtTransfer"]
        else:
            packed_d0, load_map_d1, tileset_d1 = _pack_load_map_d0_word_at_call(
                macro=macro, handler=handler, operand_words=operand_words
            )
            reset_tail_d0 = None
            reset_tail_d1 = None
        guard = handler["sectionGuard"]
        if macro == "loadMapFadeIn":
            continuation = handler["continuation"]
            if continuation is None:
                raise ValueError("map lifecycle fade continuation is missing")
            guard = continuation["sectionGuard"]
        expected = {
            "id": case["id"],
            "handlerAddress": handler["address"],
            "directCallSiteOrder": direct_target_order,
            "loadMapD0WordAtCall": packed_d0,
            "loadMapD1WordAtCall": load_map_d1,
            "tilesetD1WordAtCall": tileset_d1,
            "resetTailLoadMapD0WordAtTransfer": reset_tail_d0,
            "resetTailLoadMapD1WordAtTransfer": reset_tail_d1,
            "viewTargetEntityAfter": (
                case["viewTargetSeed"]
                if not guard["sourceStateMutationRecords"]
                else guard["sourceStateMutationRecords"][0]["literalValue"] & 0xFF
            ),
        }
        if case["expected"] != expected:
            raise ValueError(f"map lifecycle fixture/static disagreement: {case['id']}")
        derived.append(expected)
    return derived


def _listing_section_call_sites(
    listing_text: str, symbol: str, targets: list[str]
) -> list[dict[str, Any]]:
    """Resolve each H2 direct target to one exact H1 JSR instruction address."""
    section = re.search(
        rf"^[0-9A-F]{{8}}\s+{re.escape(symbol)}:\s*$"
        rf"(?P<body>.*?)^\s*[0-9A-F]{{8}}\s+; End of function {re.escape(symbol)}\s*$",
        listing_text,
        re.MULTILINE | re.DOTALL,
    )
    if section is None:
        raise ValueError(f"map lifecycle H1 call-site section is missing: {symbol}")
    matches = list(
        re.finditer(
            r"^(?P<address>[0-9A-F]{8})\s+.*?\bjsr\s+\(?(?P<target>[A-Za-z_][A-Za-z0-9_]*)",
            section.group("body"),
            re.MULTILINE,
        )
    )
    parsed_targets = [match.group("target") for match in matches]
    if parsed_targets != targets:
        raise ValueError(
            f"map lifecycle H1 call-site target/order drift: {symbol}: {parsed_targets!r}"
        )
    sites = [
        {"address": int(match.group("address"), 16), "target": match.group("target")}
        for match in matches
    ]
    if len({row["address"] for row in sites}) != len(sites):
        raise ValueError(f"map lifecycle H1 call-site uniqueness drift: {symbol}")
    return sites


def _listing_reset_tail_address(listing_text: str) -> int:
    section = re.search(
        r"^[0-9A-F]{8}\s+ResetCurrentMap:\s*$"
        r"(?P<body>.*?)^\s*[0-9A-F]{8}\s+; End of function ResetCurrentMap\s*$",
        listing_text,
        re.MULTILINE | re.DOTALL,
    )
    if section is None:
        raise ValueError("map lifecycle H1 ResetCurrentMap section is missing")
    match = re.search(
        r"^(?P<address>[0-9A-F]{8})\s+.*?\bbra\.w\s+LoadMap\s*$",
        section.group("body"),
        re.MULTILINE,
    )
    if match is None:
        raise ValueError("map lifecycle H1 ResetCurrentMap transfer use-site drift")
    return int(match.group("address"), 16)


def _runtime_navigation(
    static: dict[str, Any], fixture: dict[str, Any], upstream_path: Path
) -> dict[str, Any]:
    """Derive all execution addresses from the pinned H1 listing and H2 identities."""
    listing = upstream_path.resolve(strict=True) / "build/sf2build-h1.lst"
    listing_text = listing.read_text(encoding="utf-8")
    addresses = listing_symbol_addresses(listing_text)
    facts = static["mapLifecycleCommandFacts"]
    handlers = {row["macro"]: row for row in facts["handlers"]}
    handler_addresses = {
        row["macro"]: row["address"] for row in facts["handlers"]
    }
    for row in facts["handlers"]:
        if addresses.get(row["handler"]) != row["address"]:
            raise ValueError(f"map lifecycle H1 handler identity drift: {row['handler']}")
    function = {"entryAddress": addresses["RunMapSetupInitFunction"]}
    if fixture["function"] != function:
        raise ValueError("map lifecycle invocation seam address drift")
    reset_followup = _reset_current_map_load_followup(upstream_path)
    equates = _map_lifecycle_runtime_equates(upstream_path)
    layout_clear_span_byte_count = (
        (equates["MAP_LAYOUT_LONGS_COUNTER"] + 1)
        * reset_followup["layoutClearUnitByteCount"]
    )
    layout_start = equates["FF0000_RAM_START"]
    ram = {
        "currentMapAddress": equates["CURRENT_MAP"],
        "viewTargetEntityAddress": equates["VIEW_TARGET_ENTITY"],
        "fadingSettingAddress": equates["FADING_SETTING"],
        "viewPlaneAPixelXAddress": equates["VIEW_PLANE_A_PIXEL_X"],
        "viewPlaneAPixelYAddress": equates["VIEW_PLANE_A_PIXEL_Y"],
        "layoutClearStartMarkerAddress": layout_start,
        "layoutClearEndMarkerAddress": layout_start + layout_clear_span_byte_count - 2,
    }
    layout_markers = {
        "layoutClearStartMarkerSeed": 42330,
        "layoutClearEndMarkerSeed": 23130,
        "layoutClearSpanByteCount": layout_clear_span_byte_count,
    }
    if fixture["ram"] != ram:
        raise ValueError("map lifecycle RAM address drift")
    if fixture["layoutMarkers"] != layout_markers:
        raise ValueError("map lifecycle layout marker contract drift")
    call_sites_by_macro = {}
    for macro, handler in handlers.items():
        physical_handler = handler
        if macro == "loadMapFadeIn":
            continuation = handler["continuation"]
            if continuation is None:
                raise ValueError("map lifecycle fade continuation is missing")
            physical_handler = continuation
        targets = _handler_direct_target_order(macro, handler)
        call_sites_by_macro[macro] = _listing_section_call_sites(
            listing_text, physical_handler["handler"], targets
        )
    return {
        **function,
        "handlerAddresses": handler_addresses,
        "callSitesByMacro": call_sites_by_macro,
        "resetTailAddress": _listing_reset_tail_address(listing_text),
        "layoutMarkers": layout_markers,
    }


def _instrument_rom(rom_path: Path, fixture: dict[str, Any]) -> Path:
    """Build a deterministic session-only trampoline ROM without touching the input ROM."""
    original_hash = inspect_rom(rom_path.resolve(strict=True))["sha256"]
    data = bytearray(rom_path.read_bytes())
    patch = fixture["instrumentation"]
    call_site = patch["callSiteAddress"]
    stub_address = patch["stubAddress"]
    original_call = bytes.fromhex(patch["callSiteOriginalHex"])
    patched_call = bytes.fromhex(patch["callSitePatchedHex"])
    original_stub = bytes.fromhex(patch["stubOriginalHex"])
    stub = bytes.fromhex(patch["stubHex"])
    if data[call_site : call_site + len(original_call)] != original_call:
        raise ValueError("map lifecycle call-site bytes drifted")
    if data[stub_address : stub_address + len(original_stub)] != original_stub:
        raise ValueError("map lifecycle trampoline padding bytes drifted")
    expected_call = b"\x4E\xB9" + stub_address.to_bytes(4, "big")
    if patched_call != expected_call:
        raise ValueError("map lifecycle trampoline call shape drifted")
    if len(stub) > len(original_stub):
        raise ValueError("map lifecycle trampoline exceeds verified padding")
    if patch["postHandlerAddress"] != stub_address + len(stub) - 2:
        raise ValueError("map lifecycle trampoline return boundary drifted")
    data[call_site : call_site + len(patched_call)] = patched_call
    data[stub_address : stub_address + len(stub)] = stub
    data[0x18E:0x190] = int(mega_drive_checksum(bytes(data)), 16).to_bytes(2, "big")
    if inspect_rom(rom_path.resolve(strict=True))["sha256"] != original_hash:
        raise ValueError("map lifecycle instrumentation altered the original ROM")
    DERIVED_ROOT.mkdir(parents=True, exist_ok=True)
    output = DERIVED_ROOT / "map-lifecycle.instrumented.bin"
    output.write_bytes(data)
    return output


def _with_instrumented_rom_database(
    instrumented_rom: Path, name: str, action: Any
) -> dict[str, Any]:
    _, executable = bizhawk_contract()
    user_db = executable.parent / "gamedb" / "gamedb_user.txt"
    prior_user_db = user_db.read_bytes() if user_db.exists() else None
    md5 = hashlib.md5(instrumented_rom.read_bytes()).hexdigest().upper()
    prior_text = prior_user_db.decode("utf-8") if prior_user_db is not None else ""
    separator = "" if not prior_text or prior_text.endswith("\n") else "\n"
    user_db.write_text(
        f"{prior_text}{separator}{md5}\t\t{name}\tGEN\n", encoding="utf-8"
    )
    try:
        return action()
    finally:
        if prior_user_db is None:
            user_db.unlink(missing_ok=True)
        else:
            user_db.write_bytes(prior_user_db)


def verify_map_lifecycle(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 120
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner="map lifecycle runtime fixture")
    verify_runtime_contract(fixture, rom_path)
    static = build_map_script_engine_contract(rom_path, upstream_path)
    derived = _derive_case_expectations(static, fixture, upstream_path)
    navigation = _runtime_navigation(static, fixture, upstream_path)
    instrumented_rom = _instrument_rom(rom_path, fixture)

    def observe() -> dict[str, Any]:
        return run_observer(
            rom_path=instrumented_rom,
            observer_path=OBSERVER,
            config={
                "fixtureId": fixture["id"],
                "mapTestIndex": fixture["mapTestIndex"],
                "function": navigation,
                "ram": fixture["ram"],
                "layoutMarkers": fixture["layoutMarkers"],
                "instrumentation": fixture["instrumentation"],
                "maxFrames": fixture["maxFrames"],
                "harness": load_json(repo_path(fixture["sharedHarnessFixture"]))[
                    "harness"
                ],
                "cases": fixture["cases"],
            },
            output_name="map-lifecycle",
            timeout_seconds=timeout_seconds,
        )

    observed = _with_instrumented_rom_database(
        instrumented_rom, "SF2 H3 instrumented map lifecycle", observe
    )
    validate_json(observed, OBSERVATION_SCHEMA, owner="map lifecycle runtime observation")
    expected = {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["id"],
        "mapTest": fixture["mapTestIndex"],
        "recordOrder": [case["id"] for case in fixture["cases"]],
        "records": [
            {
                **case["expected"],
                "handlerReturned": True,
                "currentMapAfter": case["currentMapAfter"],
                **case["runtimeGolden"],
            }
            for case in fixture["cases"]
        ],
    }
    if observed != expected:
        raise ValueError(
            "map lifecycle runtime matrix mismatch\n"
            f"static={derived!r}\nexpected={expected!r}\nobserved={observed!r}"
        )
    return {
        "Fixture": fixture["id"],
        "Cases": len(derived),
        "Handlers": len({case["macro"] for case in fixture["cases"]}),
        "BizHawkLaunches": 1,
        "Instrumentation": "session-only",
        "Status": "PASS",
    }
