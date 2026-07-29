from __future__ import annotations

import re
from pathlib import Path
from typing import Any

from sf2tool.h3.bizhawk import run_observer, verify_runtime_contract
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path
from sf2tool.research_index import listing_symbol_addresses

FIXTURE = repo_path("tests/fixtures/h3/witch-save-actions-v1.json")
SCHEMA = repo_path("schemas/h3-witch-save-actions-fixture.schema.json")
OBSERVER = repo_path("tools/bizhawk/witch_save_actions_observer.lua")

SRAM_SOURCE_PATH = Path("code/common/tech/sram/sramfunctions.asm")
WITCH_SOURCE_PATH = Path("code/specialscreens/witch/witchstart.asm")
JUMP_INTERFACE_PATH = Path("code/common/tech/jumpinterfaces/s05_jumpinterface.asm")
EGRESS_SOURCE_PATH = Path("code/common/maps/egressinit.asm")
CONST_PATH = Path("sf2const.asm")
ENUM_PATH = Path("sf2enums.asm")


def _equates(source: str, names: tuple[str, ...]) -> dict[str, int]:
    values: dict[str, int] = {}
    for name in names:
        match = re.search(
            rf"^{re.escape(name)}:\s+equ\s+(\$[0-9A-F]+|\d+)",
            source,
            re.MULTILINE,
        )
        if not match:
            raise ValueError(f"witch save source guard missing constant: {name}")
        raw = match.group(1)
        values[name] = int(raw[1:], 16) if raw.startswith("$") else int(raw)
    return values


def _section(source: str, symbol: str) -> list[tuple[str, str, int]]:
    start = re.search(rf"^{re.escape(symbol)}:\s*$", source, re.MULTILINE)
    if not start:
        raise ValueError(f"witch save source guard missing section: {symbol}")
    end = source.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"witch save source guard missing end marker: {symbol}")
    records: list[tuple[str, str, int]] = []
    for line_number, raw in enumerate(source[start.start() : end].splitlines(), 1):
        line = raw.split(";", 1)[0].strip()
        if not line or line.endswith(":"):
            continue
        match = re.match(r"(?P<opcode>[A-Za-z][A-Za-z0-9]*(?:\.[bwls])?)\s*(?P<operand>.*)$", line)
        if not match:
            raise ValueError(
                f"witch save source guard cannot parse {symbol} source line: {raw!r}"
            )
        records.append(
            (
                match.group("opcode").lower(),
                re.sub(r"\s+", "", match.group("operand")).lower(),
                source[: start.start()].count("\n") + line_number,
            )
        )
    return records


def _require_order(
    section: list[tuple[str, str, int]],
    required: tuple[tuple[str, str], ...],
    *,
    name: str,
) -> list[dict[str, Any]]:
    cursor = 0
    result: list[dict[str, Any]] = []
    for opcode, operand in required:
        while cursor < len(section) and section[cursor][:2] != (opcode, operand):
            cursor += 1
        if cursor == len(section):
            raise ValueError(
                f"witch save source guard semantic drift in {name}: "
                f"expected {opcode} {operand} in order"
            )
        observed_opcode, observed_operand, source_line = section[cursor]
        result.append(
            {
                "opcode": observed_opcode,
                "operand": observed_operand,
                "sourceLine": source_line,
            }
        )
        cursor += 1
    return result


def _source_use_sites(
    sram_source: str, witch_source: str, jump_source: str
) -> dict[str, list[dict[str, Any]]]:
    save = _section(sram_source, "SaveGame")
    load = _section(sram_source, "LoadGame")
    copy = _section(sram_source, "CopySave")
    clear = _section(sram_source, "ClearSaveSlotFlag")
    witch_load = _section(witch_source, "witchMenuAction_Load")
    alias = _section(jump_source, "j_BattleLoop")
    return {
        "save": _require_order(
            save,
            (
                ("tst.b", "d0"),
                ("bne.s", "@slot2"),
                ("lea", "(save1_data).l,a1"),
                ("lea", "(save1_checksum).l,a2"),
                ("clr.w", "d1"),
                ("lea", "(save2_data).l,a1"),
                ("lea", "(save2_checksum).l,a2"),
                ("moveq", "#1,d1"),
                ("move.w", "#save_slot_real_size,d7"),
                ("bsr.w", "copybytestosram"),
                ("move.b", "d0,(a2)"),
                ("bset", "d1,(save_flags).l"),
            ),
            name="SaveGame",
        ),
        "load": _require_order(
            load,
            (
                ("lea", "(combatant_data).l,a1"),
                ("tst.b", "d0"),
                ("bne.s", "@slot2"),
                ("lea", "(save1_data).l,a0"),
                ("clr.w", "d1"),
                ("lea", "(save2_data).l,a0"),
                ("moveq", "#1,d1"),
                ("move.w", "#save_slot_real_size,d7"),
                ("bsr.w", "copybytesfromsram"),
            ),
            name="LoadGame",
        ),
        "copy": _require_order(
            copy,
            (
                ("bsr.s", "loadgame"),
                ("eori.w", "#1,d0"),
                ("andi.w", "#1,d0"),
                ("bsr.s", "savegame"),
            ),
            name="CopySave",
        ),
        "delete": _require_order(
            clear,
            (
                ("tst.b", "d0"),
                ("bne.s", "@slot2"),
                ("bclr", "#0,(save_flags).l"),
                ("bclr", "#1,(save_flags).l"),
            ),
            name="ClearSaveSlotFlag",
        ),
        "loadControlFlow": _require_order(
            witch_load,
            (
                ("bsr.w", "loadgame"),
                ("chkflg", "88"),
                ("beq.s", "@loc_18"),
                ("jsr", "j_battleloop"),
                ("clr.w", "d0"),
                ("jsr", "getsavepointformap(pc)"),
            ),
            name="witchMenuAction_Load",
        ),
        "battleLoopAlias": _require_order(
            alias,
            (("jmp", "battleloop(pc)"),),
            name="j_BattleLoop",
        ),
    }


def _require_scratch_isolation(sram_source: str, egress_source: str) -> None:
    source_label = "FF6802_LOADING_SPACE"
    for name, source in (("SRAM services", sram_source), ("GetSavepointForMap", egress_source)):
        if source_label.lower() in source.lower():
            raise ValueError(
                f"witch save source guard scratch collision in {name}: {source_label}"
            )


def _listing_macro_address(
    listing: str, function: str, macro: str, operand: int
) -> int:
    start = re.search(rf"^[0-9A-F]{{8}}.*\b{re.escape(function)}:\s*$", listing, re.MULTILINE)
    if not start:
        raise ValueError(f"witch save source guard missing H1 listing function: {function}")
    end = listing.find(f"; End of function {function}", start.end())
    if end < 0:
        raise ValueError(f"witch save source guard missing H1 listing end marker: {function}")
    section = listing[start.start() : end]
    matches = re.findall(
        rf"^(?P<address>[0-9A-F]{{8}})\s+{re.escape(macro)}\s+{operand}\b.*$",
        section,
        re.MULTILINE | re.IGNORECASE,
    )
    if len(matches) != 1:
        raise ValueError(
            f"witch save source guard expected one H1 listing macro for {macro} {operand}, "
            f"found {len(matches)}"
        )
    return int(matches[0], 16)


def build_witch_save_actions_source_contract(upstream_path: Path) -> dict[str, Any]:
    disasm = upstream_path.resolve(strict=True) / "disasm"
    sram_source = (disasm / SRAM_SOURCE_PATH).read_text(encoding="utf-8")
    witch_source = (disasm / WITCH_SOURCE_PATH).read_text(encoding="utf-8")
    jump_source = (disasm / JUMP_INTERFACE_PATH).read_text(encoding="utf-8")
    egress_source = (disasm / EGRESS_SOURCE_PATH).read_text(encoding="utf-8")
    _require_scratch_isolation(sram_source, egress_source)
    constants = _equates(
        (disasm / CONST_PATH).read_text(encoding="utf-8"),
        (
            "SAVE1_DATA",
            "SAVE2_DATA",
            "SRAM_START",
            "SAVE_FLAGS",
            "SAVE1_CHECKSUM",
            "SAVE2_CHECKSUM",
            "COMBATANT_DATA",
            "GAME_FLAGS",
            "FF6802_LOADING_SPACE",
        ),
    )
    sizes = _equates(
        (disasm / ENUM_PATH).read_text(encoding="utf-8"),
        ("SAVE_SLOT_REAL_SIZE", "SAVE_SLOT_SIZE"),
    )
    if sizes["SAVE_SLOT_SIZE"] % sizes["SAVE_SLOT_REAL_SIZE"]:
        raise ValueError("witch save source guard physical slot interval is not integral")
    listing = (upstream_path.resolve(strict=True) / "build/sf2build-h1.lst").read_text(
        encoding="utf-8"
    )
    addresses = listing_symbol_addresses(listing)
    use_sites = _source_use_sites(sram_source, witch_source, jump_source)
    load_flag_trap_address = _listing_macro_address(
        listing, "witchMenuAction_Load", "chkFlg", 88
    )
    return {
        "function": {
            "checkSramAddress": addresses["CheckSram"],
            "saveGameAddress": addresses["SaveGame"],
            "loadGameAddress": addresses["LoadGame"],
            "copySaveAddress": addresses["CopySave"],
            "clearSaveSlotFlagAddress": addresses["ClearSaveSlotFlag"],
            "loadFlagTrapAddress": load_flag_trap_address,
            "normalInstructionTarget": "GetSavepointForMap",
            "normalInstructionTargetAddress": addresses["GetSavepointForMap"],
            "normalEffectiveTarget": "GetSavepointForMap",
            "normalEffectiveTargetAddress": addresses["GetSavepointForMap"],
            "suspendInstructionTarget": "j_BattleLoop",
            "suspendInstructionTargetAddress": addresses["j_BattleLoop"],
            "suspendEffectiveTarget": "BattleLoop",
            "suspendEffectiveTargetAddress": addresses["BattleLoop"],
        },
        "ram": {
            "combatantDataAddress": constants["COMBATANT_DATA"],
            "gameFlagsAddress": constants["GAME_FLAGS"],
            "workRamScratchAddress": constants["FF6802_LOADING_SPACE"],
        },
        "storage": {
            "logicalSlotCount": 2,
            "logicalPayloadByteCountPerSlot": sizes["SAVE_SLOT_REAL_SIZE"],
            "storedPhysicalByteCountPerSlot": sizes["SAVE_SLOT_REAL_SIZE"],
            "physicalAddressIntervalPerSlot": sizes["SAVE_SLOT_SIZE"],
            "physicalAddressStepPerLogicalByte": (
                sizes["SAVE_SLOT_SIZE"] // sizes["SAVE_SLOT_REAL_SIZE"]
            ),
            "physicalWindowBaseAddress": constants["SRAM_START"] & ~1,
            "firstStoredPhysicalByteAddress": constants["SRAM_START"],
            "saveFlagsAddress": constants["SAVE_FLAGS"],
            "slot1DataAddress": constants["SAVE1_DATA"],
            "slot2DataAddress": constants["SAVE2_DATA"],
            "slot1ChecksumAddress": constants["SAVE1_CHECKSUM"],
            "slot2ChecksumAddress": constants["SAVE2_CHECKSUM"],
        },
        "sourceUseSites": use_sites,
    }


def verify_witch_save_actions(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 120
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner="witch save actions runtime fixture")
    verify_runtime_contract(fixture, rom_path)
    source_contract = build_witch_save_actions_source_contract(upstream_path)
    for field in ("function", "ram", "storage"):
        if fixture[field] != source_contract[field]:
            raise ValueError(
                f"witch save runtime golden disagrees with parsed source {field} contract"
            )
    observed = run_observer(
        rom_path=rom_path,
        observer_path=OBSERVER,
        config={
            "fixtureId": fixture["id"],
            "core": fixture["emulator"]["core"],
            "function": source_contract["function"],
            "ram": source_contract["ram"],
            "storage": source_contract["storage"],
            "cases": fixture["cases"],
        },
        output_name="witch-save-actions",
        timeout_seconds=timeout_seconds,
    )
    if observed != fixture["expectedObservation"]:
        raise ValueError(
            "witch save actions runtime matrix mismatch\n"
            f"expected={fixture['expectedObservation']!r}\nobserved={observed!r}"
        )
    return {
        "Fixture": fixture["id"],
        "DirectServiceCases": len(fixture["cases"]["directService"]),
        "LoadControlFlowCases": len(fixture["cases"]["loadControlFlow"]),
        "BizHawkLaunches": 1,
        "Status": "PASS",
    }
