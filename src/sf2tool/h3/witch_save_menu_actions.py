"""One-launch observation of bounded Witch Load/Copy/Delete action admission.

This rail deliberately owns only the action entries and their controlled menu/
prompt seams.  SRAM payload, checksums, and durable-media outcomes remain with
the pre-existing save-action and SRAM lifecycle owners.
"""

from __future__ import annotations

import re
from pathlib import Path
from typing import Any

from sf2tool.h3.bizhawk import DERIVED_ROOT, run_observer, verify_runtime_contract
from sf2tool.h3.observer_status import (
    CALLBACK_FAILURE_PREFIX,
    assert_observer_status,
    callback_failure_status,
    observer_failure_contract,
)
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path
from sf2tool.research_index import listing_symbol_addresses

FIXTURE = repo_path("tests/fixtures/h3/witch-save-menu-actions-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h3-witch-save-menu-actions-fixture.schema.json")
OBSERVATION_SCHEMA = repo_path("schemas/h3-witch-save-menu-actions-observation.schema.json")
FAILURE_SCHEMA = repo_path("schemas/h3/witch-save-menu-actions-callback-failure.schema.json")
OBSERVER = repo_path("tools/bizhawk/witch_save_menu_actions_observer.lua")

OWNER = "witch-save-menu-actions"
STATUS_PREFIX = CALLBACK_FAILURE_PREFIX
OBSERVER_FAILURE_CONTRACT = observer_failure_contract(OWNER)

WITCH_SOURCE = Path("code/specialscreens/witch/witchstart.asm")
MENU_SOURCE = Path("code/specialscreens/witch/witchmainmenu.asm")
SRAM_SOURCE = Path("code/common/tech/sram/sramfunctions.asm")
MENU_INTERFACE_SOURCE = Path("code/common/tech/jumpinterfaces/s03_jumpinterface_2.asm")
PROMPT_INTERFACE_SOURCE = Path("code/common/tech/jumpinterfaces/s03_jumpinterface_1.asm")
BATTLE_INTERFACE_SOURCE = Path("code/common/tech/jumpinterfaces/s05_jumpinterface.asm")
CONST_SOURCE = Path("sf2const.asm")
ENUM_SOURCE = Path("sf2enums.asm")
LISTING_SOURCE = Path("build/sf2build-h1.lst")

HARNESS_BASE = 0xFF6900
HARNESS_STRIDE = 0x20
HARNESS_RESULT_OFFSET = 0x0C
CONTROLLED_STUB = 0xFF6A80
TERMINAL_STUB = 0xFF6A90
SAVED_A7 = 0xFF6AB0
STACK_TOP = 0xFF6C00
GENERATED_BEGIN = HARNESS_BASE
GENERATED_END = 0xFF6AB8
BOOTSTRAP_TO_FIRST_CASE_FRAME_BUDGET = 600
CASE_FRAME_BUDGET = 600

CASE_MATRIX = (
    ("load-menu-cancel", "load", 1, -1, None, False),
    ("load-slot1-savepoint-route", "load", 1, 1, None, False),
    ("load-slot2-battle-route", "load", 2, 2, None, True),
    ("copy-prompt-cancel", "copy", 1, None, 1, False),
    ("copy-slot1-to-slot2", "copy", 1, None, 0, False),
    ("copy-slot2-to-slot1", "copy", 2, None, 0, False),
    ("delete-menu-cancel", "delete", 1, -1, None, False),
    ("delete-slot1-prompt-cancel", "delete", 1, 1, 1, False),
    ("delete-slot1-confirm", "delete", 1, 1, 0, False),
    ("delete-slot2-confirm", "delete", 2, 2, 0, False),
)


def _section(source: str, symbol: str) -> list[dict[str, Any]]:
    """Parse executable, comment-free instructions in one named function."""
    start = re.search(rf"^{re.escape(symbol)}:\s*$", source, re.MULTILINE)
    if start is None:
        raise ValueError(f"witch save-menu source guard missing section: {symbol}")
    end = source.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"witch save-menu source guard missing end marker: {symbol}")
    result: list[dict[str, Any]] = []
    start_line = source[: start.start()].count("\n")
    for offset, raw in enumerate(source[start.start() : end].splitlines(), start=1):
        instruction = raw.split(";", 1)[0].strip()
        if not instruction or instruction.endswith(":"):
            continue
        match = re.fullmatch(r"([A-Za-z][A-Za-z0-9]*(?:\.[bwls])?)\s*(.*)", instruction)
        if match is None:
            raise ValueError(f"witch save-menu source guard cannot parse instruction: {raw!r}")
        result.append(
            {
                "opcode": match.group(1).lower(),
                "operand": re.sub(r"\s+", "", match.group(2)).lower(),
                "sourceLine": start_line + offset,
            }
        )
    return result


def _require_order(
    section: list[dict[str, Any]],
    required: tuple[tuple[str, str], ...],
    *,
    owner: str,
) -> list[dict[str, Any]]:
    cursor = 0
    result: list[dict[str, Any]] = []
    for opcode, operand in required:
        while cursor < len(section) and (section[cursor]["opcode"], section[cursor]["operand"]) != (
            opcode,
            operand,
        ):
            cursor += 1
        if cursor == len(section):
            raise ValueError(
                f"witch save-menu source guard semantic drift in {owner}: "
                f"expected {opcode} {operand} in order"
            )
        result.append(section[cursor])
        cursor += 1
    return result


def _equate(source: str, name: str) -> int:
    match = re.search(rf"^{re.escape(name)}:\s+equ\s+(\$[0-9A-F]+|\d+)", source, re.MULTILINE)
    if match is None:
        raise ValueError(f"witch save-menu source guard missing constant: {name}")
    token = match.group(1)
    return int(token[1:], 16) if token.startswith("$") else int(token)


def _h1_instruction(listing: str, symbol: str, instruction: str) -> tuple[int, int, bytes]:
    start = re.search(rf"^[0-9A-F]{{8}}\s+{re.escape(symbol)}:\s*$", listing, re.MULTILINE)
    if start is None:
        raise ValueError(f"witch save-menu H1 guard missing function: {symbol}")
    end = listing.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"witch save-menu H1 guard missing end marker: {symbol}")
    matches: list[tuple[int, bytes]] = []
    for line in listing[start.end() : end].splitlines():
        match = re.fullmatch(r"([0-9A-F]{8})\s+((?:[0-9A-F]{4}\s+)+)\s+(.+?)\s*", line)
        if match is None or re.sub(r"\s+", " ", match.group(3).strip()) != instruction:
            continue
        encoded = bytes.fromhex(re.sub(r"\s+", "", match.group(2)))
        matches.append((int(match.group(1), 16), encoded))
    if len(matches) != 1:
        raise ValueError(
            f"witch save-menu H1 guard expected exactly one {symbol} instruction: {instruction}"
        )
    address, encoded = matches[0]
    return address, address + len(encoded), encoded


def _h1_macro_address(listing: str, symbol: str, macro: str, operand: int) -> int:
    start = re.search(rf"^[0-9A-F]{{8}}.*\b{re.escape(symbol)}:\s*$", listing, re.MULTILINE)
    if start is None:
        raise ValueError(f"witch save-menu H1 guard missing function: {symbol}")
    end = listing.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"witch save-menu H1 guard missing end marker: {symbol}")
    matches = re.findall(
        rf"^(?P<address>[0-9A-F]{{8}})\s+{re.escape(macro)}\s+{operand}\b.*$",
        listing[start.start() : end],
        re.MULTILINE | re.IGNORECASE,
    )
    if len(matches) != 1:
        raise ValueError(
            "witch save-menu H1 guard expected one "
            f"{macro} {operand} macro in {symbol}, found {len(matches)}"
        )
    return int(matches[0], 16)


def _alias_target(source: str, symbol: str, target: str) -> None:
    _require_order(_section(source, symbol), (("jmp", f"{target.lower()}(pc)"),), owner=symbol)


def _source_contract(
    witch: str,
    menu: str,
    sram: str,
    menu_interface: str,
    prompt_interface: str,
    battle_interface: str,
) -> dict[str, list[dict[str, Any]]]:
    load = _require_order(
        _section(witch, "witchMenuAction_Load"),
        (
            ("move.b", "(save_flags).l,d2"),
            ("andi.w", "#3,d2"),
            ("lsl.w", "#1,d2"),
            ("btst", "#1,d2"),
            ("moveq", "#2,d1"),
            ("jsr", "j_executewitchmainmenu"),
            ("tst.w", "d0"),
            ("bmi.w", "byte_73c2"),
            ("subq.w", "#1,d0"),
            ("move.w", "d0,((current_save_slot-$1000000)).w"),
            ("bsr.w", "loadgame"),
            ("chkflg", "88"),
            ("beq.s", "@loc_18"),
            ("jsr", "j_battleloop"),
            ("jsr", "getsavepointformap(pc)"),
        ),
        owner="witchMenuAction_Load",
    )
    copy = _require_order(
        _section(witch, "witchMenuAction_Copy"),
        (
            ("jsr", "j_alt_yesnoprompt"),
            ("tst.w", "d0"),
            ("bne.w", "byte_73c2"),
            ("move.b", "(save_flags).l,d0"),
            ("andi.w", "#3,d0"),
            ("subq.w", "#1,d0"),
            ("bsr.w", "copysave"),
        ),
        owner="witchMenuAction_Copy",
    )
    delete = _require_order(
        _section(witch, "witchMenuAction_Del"),
        (
            ("move.b", "(save_flags).l,d2"),
            ("andi.w", "#3,d2"),
            ("lsl.w", "#1,d2"),
            ("btst", "#1,d2"),
            ("moveq", "#2,d1"),
            ("jsr", "j_executewitchmainmenu"),
            ("tst.w", "d0"),
            ("bmi.w", "byte_73c2"),
            ("subq.w", "#1,d0"),
            ("move.w", "d0,((current_save_slot-$1000000)).w"),
            ("jsr", "j_alt_yesnoprompt"),
            ("tst.w", "d0"),
            ("bne.w", "byte_73c2"),
            ("move.w", "((current_save_slot-$1000000)).w,d0"),
            ("bsr.w", "clearsaveslotflag"),
        ),
        owner="witchMenuAction_Del",
    )
    _require_order(
        _section(menu, "ExecuteWitchMainMenu"),
        (("andi.w", "#byte_lower_nibble_mask,d0"), ("move.w", "#-1,d0"), ("rts", "")),
        owner="ExecuteWitchMainMenu",
    )
    _require_order(
        _section(sram, "LoadGame"),
        (("tst.b", "d0"), ("bsr.w", "copybytesfromsram")),
        owner="LoadGame",
    )
    _require_order(
        _section(sram, "CopySave"),
        (("bsr.s", "loadgame"), ("bsr.s", "savegame")),
        owner="CopySave",
    )
    _require_order(
        _section(sram, "ClearSaveSlotFlag"),
        (("tst.b", "d0"), ("bclr", "#0,(save_flags).l"), ("bclr", "#1,(save_flags).l")),
        owner="ClearSaveSlotFlag",
    )
    _alias_target(menu_interface, "j_ExecuteWitchMainMenu", "ExecuteWitchMainMenu")
    _alias_target(prompt_interface, "j_alt_YesNoPrompt", "alt_YesNoPrompt")
    _alias_target(battle_interface, "j_BattleLoop", "BattleLoop")
    return {"load": load, "copy": copy, "delete": delete}


def _patch(address: int, original: bytes, patched: bytes, role: str) -> dict[str, Any]:
    if len(original) != len(patched):
        raise ValueError(f"witch save-menu patch width drift: {role}")
    return {
        "role": role,
        "address": address,
        "widthBytes": len(original),
        "originalHex": original.hex().upper(),
        "patchedHex": patched.hex().upper(),
    }


def build_static_contract(
    rom_path: Path,
    upstream_path: Path,
    *,
    witch_source: str | None = None,
    menu_source: str | None = None,
    listing_text: str | None = None,
) -> dict[str, Any]:
    """Derive action control edges and every session patch from source/H1/ROM."""
    upstream = upstream_path.resolve(strict=True)
    disasm = upstream / "disasm"
    witch = witch_source or (disasm / WITCH_SOURCE).read_text(encoding="utf-8")
    menu = menu_source or (disasm / MENU_SOURCE).read_text(encoding="utf-8")
    sram = (disasm / SRAM_SOURCE).read_text(encoding="utf-8")
    menu_interface = (disasm / MENU_INTERFACE_SOURCE).read_text(encoding="utf-8")
    prompt_interface = (disasm / PROMPT_INTERFACE_SOURCE).read_text(encoding="utf-8")
    battle_interface = (disasm / BATTLE_INTERFACE_SOURCE).read_text(encoding="utf-8")
    use_sites = _source_contract(
        witch, menu, sram, menu_interface, prompt_interface, battle_interface
    )
    listing = listing_text or (upstream / LISTING_SOURCE).read_text(encoding="utf-8")
    addresses = listing_symbol_addresses(listing)
    constants = (disasm / CONST_SOURCE).read_text(encoding="utf-8")
    enums = (disasm / ENUM_SOURCE).read_text(encoding="utf-8")
    rom = rom_path.resolve(strict=True).read_bytes()

    action_symbols = {
        "load": "witchMenuAction_Load",
        "copy": "witchMenuAction_Copy",
        "delete": "witchMenuAction_Del",
    }
    action_addresses = {name: addresses[symbol] for name, symbol in action_symbols.items()}
    load_flag_trap = _h1_macro_address(listing, action_symbols["load"], "chkFlg", 88)
    menu_calls = {
        "load": _h1_instruction(listing, action_symbols["load"], "jsr j_ExecuteWitchMainMenu"),
        "delete": _h1_instruction(listing, action_symbols["delete"], "jsr j_ExecuteWitchMainMenu"),
    }
    prompt_calls = {
        "copy": _h1_instruction(listing, action_symbols["copy"], "jsr j_alt_YesNoPrompt"),
        "delete": _h1_instruction(listing, action_symbols["delete"], "jsr j_alt_YesNoPrompt"),
    }
    service_calls = {
        "load": _h1_instruction(listing, action_symbols["load"], "bsr.w LoadGame"),
        "copy": _h1_instruction(listing, action_symbols["copy"], "bsr.w CopySave"),
        "delete": _h1_instruction(listing, action_symbols["delete"], "bsr.w ClearSaveSlotFlag"),
    }
    copy_save_nested_load = _h1_instruction(listing, "CopySave", "bsr.s LoadGame")
    handoffs = {
        "savepoint": _h1_instruction(listing, action_symbols["load"], "jsr GetSavepointForMap(pc)"),
        "battle": _h1_instruction(listing, action_symbols["load"], "jsr j_BattleLoop"),
    }
    text_addresses = (0x74E2, 0x7516, 0x751E, 0x7522, 0x754C, 0x756C, 0x7574, 0x75A4, 0x75BC)
    patches: list[dict[str, Any]] = []
    for address in text_addresses:
        original = rom[address : address + 4]
        if len(original) != 4 or original[:2] != bytes.fromhex("4E45"):
            raise ValueError(f"witch save-menu text seam ROM drift at {address:#x}")
        patches.append(_patch(address, original, bytes.fromhex("4E714E71"), f"text-{address:04X}"))
    for name, (address, _, encoded) in menu_calls.items():
        original = rom[address : address + 6]
        expected = bytes.fromhex("4EB9") + addresses["j_ExecuteWitchMainMenu"].to_bytes(4, "big")
        if len(encoded) != 6 or len(original) != 6 or original != expected:
            raise ValueError(f"witch save-menu menu-call H1/ROM drift: {name}")
        patches.append(
            _patch(
                address,
                original,
                bytes.fromhex("4EB9") + CONTROLLED_STUB.to_bytes(4, "big"),
                f"menu-{name}",
            )
        )
    for name, (address, _, encoded) in prompt_calls.items():
        original = rom[address : address + 6]
        expected = bytes.fromhex("4EB9") + addresses["j_alt_YesNoPrompt"].to_bytes(4, "big")
        if len(encoded) != 6 or len(original) != 6 or original != expected:
            raise ValueError(f"witch save-menu prompt-call H1/ROM drift: {name}")
        patches.append(
            _patch(
                address,
                original,
                bytes.fromhex("4EB9") + CONTROLLED_STUB.to_bytes(4, "big"),
                f"prompt-{name}",
            )
        )
    for name, (address, _, encoded) in handoffs.items():
        original = rom[address : address + 6]
        expected = (
            bytes.fromhex("4EB9") + addresses["j_BattleLoop"].to_bytes(4, "big")
            if name == "battle"
            else bytes.fromhex("4EBA")
            + (addresses["GetSavepointForMap"] - (address + 2)).to_bytes(2, "big")
            + bytes.fromhex("4E71")
        )
        if len(encoded) not in (4, 6) or len(original) != 6 or original != expected:
            raise ValueError(f"witch save-menu handoff H1/ROM drift: {name}")
        patches.append(
            _patch(
                address,
                original,
                bytes.fromhex("4EF9") + TERMINAL_STUB.to_bytes(4, "big"),
                f"handoff-{name}",
            )
        )
    for name, (address, _, encoded) in service_calls.items():
        original = rom[address : address + len(encoded)]
        if original != encoded:
            raise ValueError(f"witch save-menu service-call H1/ROM drift: {name}")
    nested_address, _, nested_encoded = copy_save_nested_load
    if rom[nested_address : nested_address + len(nested_encoded)] != nested_encoded:
        raise ValueError("witch save-menu CopySave nested LoadGame H1/ROM drift")
    menu_loop = addresses["byte_73C2"]
    original_menu_loop = rom[menu_loop : menu_loop + 6]
    if original_menu_loop[:2] != bytes.fromhex("4E45"):
        raise ValueError("witch save-menu menu-loop ROM drift")
    patches.append(
        _patch(
            menu_loop,
            original_menu_loop,
            bytes.fromhex("4EF9") + TERMINAL_STUB.to_bytes(4, "big"),
            "menu-loop-terminal",
        )
    )
    patches.sort(key=lambda patch: patch["address"])
    for left, right in zip(patches, patches[1:], strict=False):
        if left["address"] + left["widthBytes"] > right["address"]:
            raise ValueError("witch save-menu session patch overlap")

    function = {
        "checkSramAddress": addresses["CheckSram"],
        "loadActionAddress": action_addresses["load"],
        "copyActionAddress": action_addresses["copy"],
        "deleteActionAddress": action_addresses["delete"],
        "menuInstructionTargetAddress": addresses["j_ExecuteWitchMainMenu"],
        "menuEffectiveTargetAddress": addresses["ExecuteWitchMainMenu"],
        "promptInstructionTargetAddress": addresses["j_alt_YesNoPrompt"],
        "promptEffectiveTargetAddress": addresses["alt_YesNoPrompt"],
        "loadGameAddress": addresses["LoadGame"],
        "copySaveAddress": addresses["CopySave"],
        "copySaveNestedLoadCallAddress": nested_address,
        "clearSaveSlotFlagAddress": addresses["ClearSaveSlotFlag"],
        "loadFlagTrapAddress": load_flag_trap,
        "savepointInstructionTargetAddress": addresses["GetSavepointForMap"],
        "savepointEffectiveTargetAddress": addresses["GetSavepointForMap"],
        "battleInstructionTargetAddress": addresses["j_BattleLoop"],
        "battleEffectiveTargetAddress": addresses["BattleLoop"],
        "menuLoopAddress": menu_loop,
    }
    calls = {
        "menu": {
            name: {"callSiteAddress": address, "returnAddress": returned}
            for name, (address, returned, _) in menu_calls.items()
        },
        "prompt": {
            name: {"callSiteAddress": address, "returnAddress": returned}
            for name, (address, returned, _) in prompt_calls.items()
        },
        "service": {
            name: {"callSiteAddress": address, "returnAddress": returned}
            for name, (address, returned, _) in service_calls.items()
        },
        "handoff": {
            name: {"callSiteAddress": address, "returnAddress": returned}
            for name, (address, returned, _) in handoffs.items()
        },
    }
    return {
        "function": function,
        "calls": calls,
        "ram": {
            "currentSaveSlotAddress": _equate(constants, "CURRENT_SAVE_SLOT"),
            "gameFlagsAddress": _equate(constants, "GAME_FLAGS"),
            "flag88ByteOffset": 88 // 8,
            "flag88Mask": 0x80 >> (88 % 8),
        },
        "storage": {
            "saveFlagsAddress": _equate(constants, "SAVE_FLAGS"),
            "physicalWindowBaseAddress": _equate(constants, "SRAM_START") & ~1,
            "slot1DataAddress": _equate(constants, "SAVE1_DATA"),
            "slot2DataAddress": _equate(constants, "SAVE2_DATA"),
            "slot1ChecksumAddress": _equate(constants, "SAVE1_CHECKSUM"),
            "slot2ChecksumAddress": _equate(constants, "SAVE2_CHECKSUM"),
            "logicalPayloadByteCount": _equate(enums, "SAVE_SLOT_REAL_SIZE"),
            "physicalAddressStep": 2,
        },
        "harness": {
            "baseAddress": HARNESS_BASE,
            "caseStride": HARNESS_STRIDE,
            "caseResultOffset": HARNESS_RESULT_OFFSET,
            "controlledStubAddress": CONTROLLED_STUB,
            "terminalStubAddress": TERMINAL_STUB,
            "savedA7Address": SAVED_A7,
            "stackTop": STACK_TOP,
            "generatedBegin": GENERATED_BEGIN,
            "generatedEnd": GENERATED_END,
            "bootstrapToFirstCaseFrameBudget": BOOTSTRAP_TO_FIRST_CASE_FRAME_BUDGET,
            "caseFrameBudget": CASE_FRAME_BUDGET,
        },
        "sourceUseSites": use_sites,
        "sessionPatches": patches,
    }


def _menu_initial(save_flags: int) -> tuple[int, int]:
    availability = (save_flags & 3) << 1
    return (1 if availability & 2 else 2, availability)


def _service_name(action: str) -> str:
    return {"load": "LoadGame", "copy": "CopySave", "delete": "ClearSaveSlotFlag"}[action]


def _service_entry_key(action: str) -> str:
    return {
        "load": "loadGameAddress",
        "copy": "copySaveAddress",
        "delete": "clearSaveSlotFlagAddress",
    }[action]


def _expected_chronology(
    case: dict[str, Any], static: dict[str, Any], case_index: int
) -> list[dict[str, Any]]:
    function, calls, harness = static["function"], static["calls"], static["harness"]
    action_address = function[f"{case['action']}ActionAddress"]
    result = [
        {"role": "case-entry", "pc": harness["baseAddress"] + case_index * harness["caseStride"]},
        {"role": "action-entry", "pc": action_address},
    ]
    if case["action"] in {"load", "delete"}:
        call = calls["menu"][case["action"]]
        result.extend(
            [
                {"role": "menu-call", "pc": call["callSiteAddress"]},
                {"role": "controlled-seam", "pc": harness["controlledStubAddress"]},
                {"role": "menu-return", "pc": call["returnAddress"]},
            ]
        )
    if case["action"] in {"copy", "delete"} and case["menuResult"] != -1:
        call = calls["prompt"][case["action"]]
        result.extend(
            [
                {"role": "prompt-call", "pc": call["callSiteAddress"]},
                {"role": "controlled-seam", "pc": harness["controlledStubAddress"]},
                {"role": "prompt-return", "pc": call["returnAddress"]},
            ]
        )
    admitted = (
        (case["action"] == "load" and case["menuResult"] != -1)
        or (case["action"] == "copy" and case["promptResult"] == 0)
        or (case["action"] == "delete" and case["menuResult"] != -1 and case["promptResult"] == 0)
    )
    if admitted:
        call = calls["service"][case["action"]]
        result.extend(
            [
                {"role": "service-call", "pc": call["callSiteAddress"]},
                {"role": "service-entry", "pc": function[_service_entry_key(case["action"])]},
                {"role": "service-return", "pc": call["returnAddress"]},
            ]
        )
    if case["action"] == "load" and case["menuResult"] != -1:
        handoff = calls["handoff"]["battle" if case["flag88Set"] else "savepoint"]
        result.append({"role": "load-flag-control", "pc": function["loadFlagTrapAddress"]})
        result.append({"role": "load-handoff", "pc": handoff["callSiteAddress"]})
    else:
        result.append({"role": "menu-loop-terminal", "pc": function["menuLoopAddress"]})
    result.append({"role": "terminal", "pc": harness["terminalStubAddress"]})
    return result


def expected_observation(fixture: dict[str, Any], static: dict[str, Any]) -> dict[str, Any]:
    """Model the bounded original action chronology without SRAM payload claims."""
    cases = fixture["cases"]
    matrix = tuple(
        (
            case["id"],
            case["action"],
            case["saveFlags"],
            case["menuResult"],
            case["promptResult"],
            case["flag88Set"],
        )
        for case in cases
    )
    if matrix != CASE_MATRIX or [case["id"] for case in cases] != fixture["caseOrder"]:
        raise ValueError("witch save-menu exact ten-case matrix/order drift")
    records: list[dict[str, Any]] = []
    for index, case in enumerate(cases):
        action = case["action"]
        menu = None
        if action in {"load", "delete"}:
            selector, availability = _menu_initial(case["saveFlags"])
            menu = {
                **static["calls"]["menu"][action],
                "page": 2,
                "initialSelector": selector,
                "availability": availability,
                "controlledReturn": case["menuResult"],
            }
        prompt = None
        if action in {"copy", "delete"} and case["menuResult"] != -1:
            prompt = {
                **static["calls"]["prompt"][action],
                "controlledReturn": case["promptResult"],
            }
        admitted = (
            (action == "load" and case["menuResult"] != -1)
            or (action == "copy" and case["promptResult"] == 0)
            or (action == "delete" and case["menuResult"] != -1 and case["promptResult"] == 0)
        )
        current_slot = None
        if action in {"load", "delete"} and case["menuResult"] != -1:
            current_slot = case["menuResult"] - 1
        service = None
        if admitted:
            selector = current_slot if action in {"load", "delete"} else (case["saveFlags"] & 3) - 1
            service = {
                "name": _service_name(action),
                **static["calls"]["service"][action],
                "entryAddress": static["function"][_service_entry_key(action)],
                "selector": selector,
            }
        handoff = None
        if action == "load" and admitted:
            kind = "battle" if case["flag88Set"] else "savepoint"
            handoff = {
                "kind": kind,
                **static["calls"]["handoff"][kind],
                "instructionTargetAddress": static["function"][f"{kind}InstructionTargetAddress"],
                "effectiveTargetAddress": static["function"][f"{kind}EffectiveTargetAddress"],
            }
        records.append(
            {
                "id": case["id"],
                "action": action,
                "actionEntryAddress": static["function"][f"{action}ActionAddress"],
                "menu": menu,
                "prompt": prompt,
                "currentSaveSlot": current_slot,
                "service": service,
                "handoff": handoff,
                "callbackChronology": _expected_chronology(case, static, index),
            }
        )
    return {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["id"],
        "caseOrder": fixture["caseOrder"],
        "observedActionEntries": {
            action: static["function"][f"{action}ActionAddress"]
            for action in ("load", "copy", "delete")
        },
        "records": records,
        "restoration": {
            "currentSaveSlotRestored": True,
            "gameFlag88Restored": True,
            "saveFlagsRestored": True,
            "slotDataRestored": True,
            "generatedBytesRestored": True,
            "stackRestored": True,
            "frameRestored": True,
            "cartPatchesRestored": True,
        },
        "callbacksCleared": 0,
    }


def _assert_source_fixture(fixture: dict[str, Any], static: dict[str, Any]) -> None:
    if (
        fixture["provenance"]["runtimeCommand"]
        != "uv run sf2 h3 witch-save-menu-actions --timeout-seconds 180"
    ):
        raise ValueError("witch save-menu fixture runtime command drift")
    if fixture["provenance"]["sourcePaths"] != [
        BATTLE_INTERFACE_SOURCE.as_posix(),
        MENU_INTERFACE_SOURCE.as_posix(),
        PROMPT_INTERFACE_SOURCE.as_posix(),
        SRAM_SOURCE.as_posix(),
        MENU_SOURCE.as_posix(),
        WITCH_SOURCE.as_posix(),
        CONST_SOURCE.as_posix(),
        ENUM_SOURCE.as_posix(),
    ]:
        raise ValueError("witch save-menu fixture source provenance drift")
    if (
        fixture["instrumentation"]["observedActionEntries"]
        != expected_observation(fixture, static)["observedActionEntries"]
    ):
        raise ValueError("witch save-menu fixture observed action callback drift")
    expected = expected_observation(fixture, static)
    if fixture["expectedObservation"] != expected:
        raise ValueError("witch save-menu fixture exact expected-observation drift")


def _failure_diagnostic(status_path: Path) -> dict[str, Any] | None:
    payload = callback_failure_status(status_path, owner=OWNER, schema_path=FAILURE_SCHEMA)
    if payload is None:
        return None
    lines = status_path.read_text(encoding="utf-8").splitlines()
    failures = [index for index, line in enumerate(lines) if line.startswith(STATUS_PREFIX)]
    if len(failures) != 1 or failures[0] != len(lines) - 1:
        raise ValueError("witch save-menu callback failure must be terminal and unique")
    if not any(line.startswith("milestone:") for line in lines[: failures[0]]):
        raise ValueError("witch save-menu callback failure lacks preceding milestone")
    return payload


def _assert_status(status_path: Path) -> None:
    diagnostic = _failure_diagnostic(status_path)
    if diagnostic is not None:
        raise RuntimeError(f"{OWNER} observer callback failure: {diagnostic}")
    assert_observer_status(
        status_path,
        owner=OWNER,
        schema_path=FAILURE_SCHEMA,
        required_milestones=(
            "milestone:action-probe-armed",
            "milestone:action-cases-entered",
            "milestone:callbacks-cleared:0",
            "milestone:observer-finished",
        ),
    )
    lines = status_path.read_text(encoding="utf-8").splitlines()
    required = (
        "milestone:action-probe-armed",
        "milestone:action-cases-entered",
        "milestone:callbacks-cleared:0",
        "milestone:observer-finished",
    )
    positions: list[int] = []
    for milestone in required:
        if lines.count(milestone) != 1:
            raise RuntimeError(
                f"{OWNER} observer required milestone multiplicity drift: {milestone}"
            )
        positions.append(lines.index(milestone))
    if positions != sorted(positions):
        raise RuntimeError(f"{OWNER} observer required milestone order drift")


def _assert_observation(
    fixture: dict[str, Any], static: dict[str, Any], observed: dict[str, Any]
) -> None:
    validate_json(observed, OBSERVATION_SCHEMA, owner="witch save-menu action observation")
    expected = expected_observation(fixture, static)
    if observed != expected:
        raise ValueError(
            "witch save-menu action runtime matrix mismatch\n"
            f"expected={expected!r}\nobserved={observed!r}"
        )


def verify_witch_save_menu_actions(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 180
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner="witch save-menu actions fixture")
    verify_runtime_contract(fixture, rom_path)
    static = build_static_contract(rom_path, upstream_path)
    _assert_source_fixture(fixture, static)
    status_path = DERIVED_ROOT / f"{OWNER}.status.txt"
    observed_path = DERIVED_ROOT / f"{OWNER}.observed.json"
    try:
        try:
            observed = run_observer(
                rom_path=rom_path,
                observer_path=OBSERVER,
                config={
                    "id": fixture["id"],
                    "core": fixture["emulator"]["core"],
                    "cases": fixture["cases"],
                    "caseOrder": fixture["caseOrder"],
                    "static": static,
                    "observerFailureContract": OBSERVER_FAILURE_CONTRACT,
                },
                output_name=OWNER,
                timeout_seconds=timeout_seconds,
            )
        except RuntimeError as error:
            diagnostic = _failure_diagnostic(status_path)
            if diagnostic is not None:
                raise RuntimeError(f"{OWNER} observer callback failure: {diagnostic}") from error
            raise
        _assert_status(status_path)
        _assert_observation(fixture, static, observed)
    except Exception:
        # A successful Lua exit is not accepted evidence until its terminal
        # status and schema/golden projection have both been accepted.
        observed_path.unlink(missing_ok=True)
        raise
    return {
        "Fixture": fixture["id"],
        "Cases": len(fixture["cases"]),
        "BizHawkLaunches": 1,
        "CallbacksCleared": observed["callbacksCleared"],
        "Status": "PASS",
    }
