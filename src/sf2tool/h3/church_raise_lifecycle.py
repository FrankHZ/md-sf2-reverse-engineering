"""Static-first Church Raise admission, loop, cost, and commit contract.

The direct-function observer deliberately enters the original ``ChurchMenu``
entry.  Its session-only shims choose Raise and answer the bounded Yes/No seam;
all membership iteration, promotion lookup, affordability, gold, HP, and map
sprite work remains original code.
"""

from __future__ import annotations

import json
import re
import tempfile
from pathlib import Path
from typing import Any

from sf2tool.h3.bizhawk import run_observer
from sf2tool.h3.map_lifecycle import _with_instrumented_rom_database
from sf2tool.h3.observer_status import (
    CALLBACK_FAILURE_PREFIX,
    assert_observer_status,
    callback_failure_status,
    observer_failure_contract,
)
from sf2tool.h3.service_menu_lifecycle import CURRENT_PORTRAIT_ADDRESS
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

FIXTURE = repo_path("tests/fixtures/h3/church-raise-lifecycle-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h3-church-raise-lifecycle-fixture.schema.json")
OBSERVATION_SCHEMA = repo_path("schemas/h3-church-raise-lifecycle-observation.schema.json")
FAILURE_SCHEMA = repo_path("schemas/h3/church-raise-lifecycle-callback-failure.schema.json")
OBSERVER = repo_path("tools/bizhawk/church_raise_lifecycle_observer.lua")
UPSTREAM = repo_path("local/upstream/SF2DISASM")
DISASM = UPSTREAM / "disasm"
LISTING = UPSTREAM / "build/sf2build-h1.lst"
OWNER = "church-raise-lifecycle"
OBSERVED_OUTPUT = repo_path(f"local/derived/h3/{OWNER}.observed.json")
STATUS_PREFIX = CALLBACK_FAILURE_PREFIX
OBSERVER_FAILURE_CONTRACT = observer_failure_contract(OWNER)

CASE_IDS = (
    "all-alive-no-prompt",
    "unpromoted-decline",
    "unpromoted-insufficient-gold",
    "unpromoted-exact-cost-success",
    "promoted-exact-cost-success",
    "promoted-one-below-cost",
    "mixed-decline-then-success",
)
SUCCESS_CHRONOLOGY_ROLES = (
    "j-decrease-gold-entry",
    "decrease-gold-entry",
    "decrease-gold-return",
    "j-increase-current-hp-entry",
    "increase-current-hp-entry",
    "increase-current-hp-return",
    "mapsprite-entry",
    "mapsprite-return",
)

LUA_REQUIRED_CALLBACK_ROLES = frozenset(
    {
        "bootstrap-check-sram",
        "case-entry",
        "case-result",
        "terminal-finalize",
        "church-entry",
        "raise-route",
        "action-stub",
        "prompt-call",
        "prompt-stub",
        "promotion-check",
        "prompt-compare",
        "affordability-check",
        "do-raise",
        *SUCCESS_CHRONOLOGY_ROLES,
    }
)
LUA_FAILURE_ROLES = LUA_REQUIRED_CALLBACK_ROLES | {
    "registration",
    "bootstrap-watchdog",
    "case-watchdog",
}
RESTORATION_MISMATCH_STATE_KEYS = {
    "gold": "gold",
    "dialogueName": "dialogueScratch",
    "dialogueNumber": "dialogueScratch",
    "targetsListLength": "targetsListLength",
    "targetsListByte": "targetsListBytes",
    "currentPortrait": "currentPortrait",
    "combatantRecordByte": "combatantRecords",
    "mapspriteByte": "mapspriteBytes",
    "generatedRamByte": "generatedRam",
    "a6": "a6a7Balance",
    "a7": "a6a7Balance",
}
RESTORATION_CHECK_KEYS = frozenset(RESTORATION_MISMATCH_STATE_KEYS.values())

CHURCH_SOURCE = Path("code/common/menus/church/churchactions_1.asm")
CHURCH_HELPER_SOURCE = Path("code/common/menus/church/churchactions_2.asm")
ENUMS = Path("sf2enums.asm")
CONSTANTS = Path("sf2const.asm")
GOLD_SOURCE = Path("code/common/stats/gold.asm")
COMBATANT_SOURCE = Path("code/common/stats/combatantstats_2.asm")
PROMOTIONS_SOURCE = Path("data/stats/allies/classes/promotions.asm")

# Pinned source/H1 plan.  The actual candidate ROM must equal ``originalHex``;
# it must never be allowed to supply or redefine an original patch byte.
# ``h1Hex`` retains assembler relocation zeros where source/H1 owns shape and
# the plan owns the resolved original branch displacement.
SESSION_PATCH_SPECS = (
    (
        0x20A18,
        "4E45006E4E45FFFF",
        "4E45006E4E45FFFF",
        "4E714E714E714E71",
        "entry-presentation-bypass",
        "ChurchMenu",
        ("txt 110", "clsTxt"),
    ),
    (
        0x20A20,
        "4EB90001003C",
        "4EB90001003C",
        "4E714E714E71",
        "entry-portrait-close-bypass",
        "ChurchMenu",
        ("jsr j_ClosePortraitWindow",),
    ),
    (
        0x20A30,
        "4EB900010000",
        "4EB900010000",
        "4EB900FF6D00",
        "controlled-raise-selection",
        "ChurchMenu",
        ("jsr j_ExecuteDiamondMenu",),
    ),
    (0x20A6C, "4E450076", "4E450076", "4E714E71", "raise-text-bypass", "ChurchMenu", ("txt 118",)),
    (
        0x20A96,
        "4E450081",
        "4E450081",
        "4E714E71",
        "raise-member-text-bypass",
        "ChurchMenu",
        ("txt 129",),
    ),
    (
        0x20ACE,
        "4E450082",
        "4E450082",
        "4E714E71",
        "raise-cost-text-bypass",
        "ChurchMenu",
        ("txt 130",),
    ),
    (
        0x20AD2,
        "4EB900010050",
        "4EB900010050",
        "4E714E714E71",
        "gold-window-bypass",
        "ChurchMenu",
        ("jsr j_OpenGoldWindow",),
    ),
    (
        0x20AD8,
        "4EB900010074",
        "4EB900010074",
        "4EB900FF6D10",
        "controlled-yes-no",
        "ChurchMenu",
        ("jsr j_alt_YesNoPrompt",),
    ),
    (
        0x20ADE,
        "4EB900010058",
        "4EB900010058",
        "4E714E714E71",
        "gold-window-close-bypass",
        "ChurchMenu",
        ("jsr j_CloseGoldWindow",),
    ),
    (
        0x20AEC,
        "4E45007C",
        "4E45007C",
        "4E714E71",
        "decline-text-bypass",
        "ChurchMenu",
        ("txt 124",),
    ),
    (
        0x20B06,
        "4E45007D",
        "4E45007D",
        "4E714E71",
        "insufficient-text-bypass",
        "ChurchMenu",
        ("txt 125",),
    ),
    (
        0x20B26,
        "4E4000184EBA070C4E71",
        "4E4000184EBA00004E71",
        "4E714E714E714E714E71",
        "revival-presentation-bypass",
        "ChurchMenu",
        ("sndCom MUSIC_REVIVE", "jsr WaitForMusicResumeAndPlayerInput(pc)", "nop"),
    ),
    (
        0x20B3E,
        "4E450083",
        "4E450083",
        "4E714E71",
        "revival-text-bypass",
        "ChurchMenu",
        ("txt 131",),
    ),
    (
        0x20B46,
        "0C6E0000FFF2",
        "0C6E0000FFF2",
        "4EF900020A5C",
        "raise-terminal-return",
        "ChurchMenu",
        ("cmpi.w #0,deadMembersCount(a6)",),
    ),
    (
        0x21156,
        "4EB900008270",
        "4EB900008270",
        "4E714E714E71",
        "controlled-force-list",
        "Church_GetCurrentForceMemberInfo",
        ("jsr j_UpdateForce",),
    ),
)

EXACT_CASE_MATRIX = (
    ("all-alive-no-prompt", 999, (), ((0, 0, 1, False, 20, 20, 1),)),
    ("unpromoted-decline", 50, (-1,), ((1, 0, 5, False, 30, 0, 2),)),
    ("unpromoted-insufficient-gold", 69, (0,), ((2, 0, 7, False, 40, 0, 3),)),
    ("unpromoted-exact-cost-success", 70, (0,), ((3, 0, 7, False, 40, 0, 4),)),
    ("promoted-exact-cost-success", 280, (0,), ((4, 12, 8, True, 55, 0, 5),)),
    ("promoted-one-below-cost", 279, (0,), ((5, 12, 8, True, 55, 0, 6),)),
    (
        "mixed-decline-then-success",
        60,
        (-1, 0),
        ((6, 0, 1, False, 20, 20, 7), (7, 0, 5, False, 30, 0, 8), (8, 0, 6, False, 45, 0, 9)),
    ),
)


def _normal(line: str) -> str:
    return re.sub(r"\s+", " ", line.split(";", 1)[0].strip()).lower()


def _section(source: str, symbol: str) -> str:
    start = re.search(rf"^{re.escape(symbol)}:\s*$", source, re.MULTILINE)
    if start is None:
        raise ValueError(f"church raise source function missing: {symbol}")
    end = source.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"church raise source function end missing: {symbol}")
    return source[start.start() : end]


def _require_order(source: str, symbol: str, fragments: tuple[str, ...]) -> None:
    rows = [_normal(row) for row in _section(source, symbol).splitlines()]
    cursor = 0
    for fragment in fragments:
        expected = _normal(fragment)
        try:
            cursor = rows.index(expected, cursor) + 1
        except ValueError as error:
            raise ValueError(
                f"church raise source guard drift in {symbol}: {expected!r}"
            ) from error


def _listing_function(listing: str, symbol: str) -> tuple[dict[str, int], list[dict[str, Any]]]:
    start = re.search(rf"^[0-9A-F]{{8}}\s+{re.escape(symbol)}:\s*$", listing, re.MULTILINE)
    if start is None:
        raise ValueError(f"church raise H1 missing function: {symbol}")
    end = listing.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"church raise H1 missing function end: {symbol}")
    labels: dict[str, int] = {}
    rows: list[dict[str, Any]] = []
    for line in listing[start.start() : end].splitlines():
        label = re.fullmatch(r"([0-9A-F]{8})\s+([@A-Za-z_][@A-Za-z0-9_]*):\s*", line)
        if label:
            labels[label.group(2)] = int(label.group(1), 16)
            continue
        instruction = re.fullmatch(r"([0-9A-F]{8})\s+((?:[0-9A-F]{4}\s+)+)(.+?)\s*", line)
        if instruction:
            rows.append(
                {
                    "address": int(instruction.group(1), 16),
                    "hex": re.sub(r"\s+", "", instruction.group(2)),
                    "text": re.sub(r"\s+", " ", instruction.group(3).strip()),
                }
            )
    if symbol not in labels:
        raise ValueError(f"church raise H1 entry label omitted: {symbol}")
    return labels, rows


def _find(rows: list[dict[str, Any]], address: int, fragment: str) -> dict[str, Any]:
    matches = [
        row for row in rows if row["address"] == address and fragment.lower() in row["text"].lower()
    ]
    if len(matches) != 1:
        raise ValueError(f"church raise H1 instruction identity drift at 0x{address:X}: {fragment}")
    return matches[0]


def _single_rts(rows: list[dict[str, Any]], symbol: str) -> int:
    returns = [row["address"] for row in rows if row["text"].strip().lower() == "rts"]
    if len(returns) != 1:
        raise ValueError(f"church raise H1 return identity drift in {symbol}")
    return returns[0]


def _pc_relative_target(rom: bytes, address: int, *, opcode: str, symbol: str) -> int:
    actual = _rom_hex(rom, address, 4)
    if actual[:4] != opcode:
        raise ValueError(f"church raise ROM jump opcode drift for {symbol} at 0x{address:X}")
    return address + 2 + int.from_bytes(rom[address + 2 : address + 4], "big", signed=True)


def _absolute_call_target(rom: bytes, address: int, symbol: str) -> tuple[int, int]:
    if _rom_hex(rom, address, 2) != "4EB9":
        raise ValueError(f"church raise ROM call opcode drift for {symbol} at 0x{address:X}")
    return int.from_bytes(rom[address + 2 : address + 6], "big"), address + 6


def _word_branch_target(rom: bytes, address: int, *, opcode: str, symbol: str) -> tuple[int, int]:
    if _rom_hex(rom, address, 2) != opcode:
        raise ValueError(f"church raise ROM branch opcode drift for {symbol} at 0x{address:X}")
    target = address + 2 + int.from_bytes(rom[address + 2 : address + 4], "big", signed=True)
    return target, address + 4


def _equates(source: str) -> dict[str, int]:
    values: dict[str, int] = {}
    for name, literal in re.findall(
        r"^([A-Z][A-Z0-9_]*):\s+equ\s+(\$[0-9A-F]+|\d+)", source, re.MULTILINE
    ):
        values[name] = int(literal[1:], 16) if literal.startswith("$") else int(literal)
    return values


def _rom_hex(rom: bytes, address: int, width: int) -> str:
    value = rom[address : address + width].hex().upper()
    if len(value) != width * 2:
        raise ValueError(f"church raise ROM is short at 0x{address:X}")
    return value


def _listing_span_hex(listing: str, address: int, width: int) -> str:
    """Return one complete H1 byte span, including data macro output rows."""
    cells: dict[int, int] = {}
    for line in listing.splitlines():
        match = re.match(r"^([0-9A-F]{8})\s+((?:[0-9A-F]{4}\s+)+)", line)
        if match is None:
            continue
        row_address = int(match.group(1), 16)
        row = bytes.fromhex(re.sub(r"\s+", "", match.group(2)))
        for offset, value in enumerate(row):
            cell = row_address + offset
            if address <= cell < address + width:
                if cell in cells:
                    raise ValueError(f"church raise H1 patch cell overlap at 0x{cell:X}")
                cells[cell] = value
    if len(cells) != width:
        raise ValueError(f"church raise H1 patch span is incomplete at 0x{address:X}")
    return bytes(cells[cell] for cell in range(address, address + width)).hex().upper()


def _session_patch_plan(
    *, church_source: str, helper_source: str, listing: str, rom: bytes
) -> list[dict[str, Any]]:
    """Bind all session writes to source, H1 and the pinned original byte plan."""
    source_sections = {
        "ChurchMenu": _section(church_source, "ChurchMenu"),
        "Church_GetCurrentForceMemberInfo": _section(
            helper_source, "Church_GetCurrentForceMemberInfo"
        ),
    }
    patches: list[dict[str, Any]] = []
    for (
        address,
        original_hex,
        h1_hex,
        patched_hex,
        purpose,
        source_symbol,
        source_fragments,
    ) in SESSION_PATCH_SPECS:
        section_rows = {_normal(line) for line in source_sections[source_symbol].splitlines()}
        for fragment in source_fragments:
            if _normal(fragment) not in section_rows:
                raise ValueError(
                    f"church raise session patch source drift for {purpose}: {fragment}"
                )
        width = len(bytes.fromhex(original_hex))
        if len(bytes.fromhex(patched_hex)) != width:
            raise ValueError(f"church raise session patch width plan drift: {purpose}")
        if _listing_span_hex(listing, address, width) != h1_hex:
            raise ValueError(f"church raise session patch H1 drift at 0x{address:X}")
        if _rom_hex(rom, address, width) != original_hex:
            raise ValueError(
                f"church raise session patch source/H1/ROM original-byte drift at 0x{address:X}"
            )
        patches.append(
            {
                "address": address,
                "width": width,
                "originalHex": original_hex,
                "hex": patched_hex,
                "purpose": purpose,
            }
        )
    return patches


def _source_guards(disasm: Path) -> None:
    church = (disasm / CHURCH_SOURCE).read_text(encoding="utf-8")
    helper = (disasm / CHURCH_HELPER_SOURCE).read_text(encoding="utf-8")
    gold = (disasm / GOLD_SOURCE).read_text(encoding="utf-8")
    stats = (disasm / COMBATANT_SOURCE).read_text(encoding="utf-8")
    _require_order(
        church,
        "ChurchMenu",
        (
            "jsr j_ExecuteDiamondMenu",
            "cmpi.w #-1,d0",
            "bra.w @CheckRaiseAction",
            "@CheckRaiseAction:",
            "cmpi.w #0,d0",
            "bne.w @CheckCureAction",
            "bsr.w Church_GetCurrentForceMemberInfo",
            "clr.w deadMembersCount(a6)",
            "@CountDeadMembers_Loop:",
            "move.b (a0)+,d0",
            "jsr j_GetCurrentHp",
            "tst.w d1",
            "bhi.w @RaiseNextMember",
            "addi.w #1,deadMembersCount(a6)",
            "jsr j_GetLevel",
            "mulu.w #CHURCHMENU_PER_LEVEL_RAISE_COST,d1",
            "jsr j_GetClass",
            "bsr.w GetPromotionData",
            "cmpi.w #0,cannotPromoteFlag(a6)",
            "beq.w @ConfirmRaise",
            "addi.l #CHURCHMENU_RAISE_COST_EXTRA_WHEN_PROMOTED,actionCost(a6)",
            "@ConfirmRaise:",
            "jsr j_alt_YesNoPrompt",
            "cmpi.w #0,d0",
            "beq.w @CheckRaiseCost",
            "@CheckRaiseCost:",
            "jsr j_GetGold",
            "cmp.l d0,d1",
            "bcc.s @DoRaise",
            "@DoRaise:",
            "jsr j_DecreaseGold",
            "move.w #CHAR_STATCAP_HP,d1",
            "jsr j_IncreaseCurrentHp",
            "bsr.w UpdateAllyMapsprite",
            "@RaiseNextMember:",
            "dbf d7,@CountDeadMembers_Loop",
            "cmpi.w #0,deadMembersCount(a6)",
        ),
    )
    _require_order(
        helper,
        "Church_GetCurrentForceMemberInfo",
        (
            "jsr j_UpdateForce",
            "lea ((TARGETS_LIST-$1000000)).w,a0",
            "move.w ((TARGETS_LIST_LENGTH-$1000000)).w,membersListLength(a6)",
            "move.w ((TARGETS_LIST_LENGTH-$1000000)).w,d7",
            "subq.b #1,d7",
            "rts",
        ),
    )
    _require_order(
        helper,
        "GetPromotionData",
        (
            "clr.w cannotPromoteFlag(a6)",
            "dbf d7,@FindClass_Loop",
            "move.w #1,cannotPromoteFlag(a6)",
            "rts",
        ),
    )
    _require_order(
        helper,
        "UpdateAllyMapsprite",
        (
            "cmpi.b #COMBATANT_ALLIES_NUMBER,d0",
            "bhi.s @Return",
            "jsr j_GetAllyMapsprite",
            "rts",
        ),
    )
    _require_order(
        gold,
        "DecreaseGold",
        (
            "sub.l d1,d0",
            "bcc.s @Continue",
            "moveq #0,d0",
            "move.l d0,((CURRENT_GOLD-$1000000)).w",
            "rts",
        ),
    )
    _require_order(
        stats,
        "IncreaseCurrentHp",
        (
            "move.w COMBATANT_OFFSET_HP_MAX(a0),d6",
            "moveq #COMBATANT_OFFSET_HP_CURRENT,d7",
            "bsr.w IncreaseAndClampWord",
            "rts",
        ),
    )


def build_static_contract(rom_path: Path, upstream_path: Path = UPSTREAM) -> dict[str, Any]:
    """Derive every Raise branch operand from source, H1, and the supplied ROM."""
    disasm = upstream_path / "disasm"
    _source_guards(disasm)
    eq = _equates((disasm / ENUMS).read_text(encoding="utf-8"))
    eq.update(_equates((disasm / CONSTANTS).read_text(encoding="utf-8")))
    required = (
        "CHURCHMENU_PER_LEVEL_RAISE_COST",
        "CHURCHMENU_RAISE_COST_EXTRA_WHEN_PROMOTED",
        "CHAR_STATCAP_HP",
        "COMBATANT_DATA",
        "COMBATANT_DATA_ENTRY_REAL_SIZE",
        "COMBATANT_OFFSET_CLASS",
        "COMBATANT_OFFSET_LEVEL",
        "COMBATANT_OFFSET_HP_MAX",
        "COMBATANT_OFFSET_HP_CURRENT",
        "CURRENT_GOLD",
        "TARGETS_LIST",
        "TARGETS_LIST_LENGTH",
        "DIALOGUE_NAME_INDEX_1",
        "DIALOGUE_NUMBER",
        "ENTITY_DATA",
        "ENTITYDEF_OFFSET_MAPSPRITE",
    )
    missing = [name for name in required if name not in eq]
    if missing:
        raise ValueError(f"church raise required enum missing: {missing}")
    listing = (upstream_path / "build/sf2build-h1.lst").read_text(encoding="utf-8")
    church_labels, church_rows = _listing_function(listing, "ChurchMenu")
    helper_labels, helper_rows = _listing_function(listing, "Church_GetCurrentForceMemberInfo")
    promo_labels, promo_rows = _listing_function(listing, "GetPromotionData")
    mapsprite_labels, mapsprite_rows = _listing_function(listing, "UpdateAllyMapsprite")
    decrease_labels, decrease_rows = _listing_function(listing, "DecreaseGold")
    increase_labels, increase_rows = _listing_function(listing, "IncreaseCurrentHp")
    alias_rows = {
        symbol: _listing_function(listing, symbol)
        for symbol in (
            "j_DecreaseGold",
            "j_IncreaseCurrentHp",
            "j_GetGold",
            "j_GetCurrentHp",
            "j_GetLevel",
            "j_GetClass",
        )
    }
    entries = {
        "churchMenu": church_labels["ChurchMenu"],
        "raiseRoute": church_labels["@CheckRaiseAction"],
        "memberLoop": church_labels["@CountDeadMembers_Loop"],
        "confirmRaise": church_labels["@ConfirmRaise"],
        "checkRaiseCost": church_labels["@CheckRaiseCost"],
        "doRaise": church_labels["@DoRaise"],
        "raiseNextMember": church_labels["@RaiseNextMember"],
        "forceMemberInfo": helper_labels["Church_GetCurrentForceMemberInfo"],
        "promotionData": promo_labels["GetPromotionData"],
        "updateAllyMapsprite": mapsprite_labels["UpdateAllyMapsprite"],
        "decreaseGold": decrease_labels["DecreaseGold"],
        "increaseCurrentHp": increase_labels["IncreaseCurrentHp"],
    }
    instruction_rows = {
        str(address): _find(church_rows, address, fragment)
        for address, fragment in (
            (0x20A30, "j_ExecuteDiamondMenu"),
            (0x20A3C, "@CheckRaiseAction"),
            (0x20A80, "j_GetCurrentHp"),
            (0x20A9A, "j_GetLevel"),
            (0x20AA8, "j_GetClass"),
            (0x20AB2, "GetPromotionData"),
            (0x20AD8, "j_alt_YesNoPrompt"),
            (0x20AF4, "j_GetGold"),
            (0x20B02, "cmp.l d0,d1"),
            (0x20B12, "j_DecreaseGold"),
            (0x20B20, "j_IncreaseCurrentHp"),
            (0x20B34, "UpdateAllyMapsprite"),
            (0x20B42, "dbf d7"),
        )
    }
    for address, fragment in ((0x21156, "j_UpdateForce"),):
        instruction_rows[str(address)] = _find(helper_rows, address, fragment)
    for address, fragment in ((0x210F8, "dbf d7"),):
        instruction_rows[str(address)] = _find(promo_rows, address, fragment)
    for address, fragment in ((0x2124A, "cmpi.b"),):
        instruction_rows[str(address)] = _find(mapsprite_rows, address, fragment)
    for symbol, rows, entry in (
        ("DecreaseGold", decrease_rows, entries["decreaseGold"]),
        ("IncreaseCurrentHp", increase_rows, entries["increaseCurrentHp"]),
        ("UpdateAllyMapsprite", mapsprite_rows, entries["updateAllyMapsprite"]),
    ):
        instruction_rows[str(entry)] = _find(rows, entry, "")
        return_address = _single_rts(rows, symbol)
        instruction_rows[str(return_address)] = _find(rows, return_address, "rts")
    rom = rom_path.read_bytes()
    for row in instruction_rows.values():
        width = len(row["hex"]) // 2
        actual = _rom_hex(rom, row["address"], width)
        # H1 intentionally leaves relocatable word displacements as zero; its
        # fixed opcode/operand cells must still agree before the target audit.
        expected_bytes = bytes.fromhex(row["hex"])
        actual_bytes = bytes.fromhex(actual)
        if any(
            expected and expected != got
            for expected, got in zip(expected_bytes, actual_bytes, strict=True)
        ):
            raise ValueError(f"church raise H1/ROM parity drift at 0x{row['address']:X}")
        row["romHex"] = actual
    alias_targets = {
        "j_DecreaseGold": ("jDecreaseGold", "decreaseGold"),
        "j_IncreaseCurrentHp": ("jIncreaseCurrentHp", "increaseCurrentHp"),
        "j_GetGold": ("jGetGold", "getGold"),
        "j_GetCurrentHp": ("jGetCurrentHp", "getCurrentHp"),
        "j_GetLevel": ("jGetLevel", "getLevel"),
        "j_GetClass": ("jGetClass", "getClass"),
    }
    for source_symbol, (alias_name, entry_name) in alias_targets.items():
        labels, rows = alias_rows[source_symbol]
        entry = labels[source_symbol]
        row = _find(rows, entry, "jmp")
        width = len(row["hex"]) // 2
        actual = _rom_hex(rom, entry, width)
        if actual[:4] != "4EFA":
            raise ValueError(f"church raise alias opcode drift for {source_symbol}")
        entries[alias_name] = entry
        entries[entry_name] = _pc_relative_target(rom, entry, opcode="4EFA", symbol=source_symbol)
    for expected_name, expected_symbol in (
        ("decreaseGold", "DecreaseGold"),
        ("increaseCurrentHp", "IncreaseCurrentHp"),
    ):
        labels = {
            "DecreaseGold": decrease_labels,
            "IncreaseCurrentHp": increase_labels,
        }[expected_symbol]
        if entries[expected_name] != labels[expected_symbol]:
            raise ValueError(f"church raise alias target drift for {expected_symbol}")
    raise_target, raise_branch_fallthrough = _word_branch_target(
        rom, 0x20A3C, opcode="6000", symbol="@CheckRaiseAction"
    )
    promotion_target, _ = _word_branch_target(
        rom, 0x20AB2, opcode="6100", symbol="GetPromotionData"
    )
    mapsprite_branch_target, _ = _word_branch_target(
        rom, 0x20B34, opcode="6100", symbol="UpdateAllyMapsprite"
    )
    branch_targets = {
        "raiseRoute": raise_target,
        "promotionData": promotion_target,
        "updateAllyMapsprite": mapsprite_branch_target,
    }
    if branch_targets != {
        "raiseRoute": entries["raiseRoute"],
        "promotionData": entries["promotionData"],
        "updateAllyMapsprite": entries["updateAllyMapsprite"],
    }:
        raise ValueError("church raise source/H1/ROM branch target drift")
    helper_returns = {
        "decreaseGold": _single_rts(decrease_rows, "DecreaseGold"),
        "increaseCurrentHp": _single_rts(increase_rows, "IncreaseCurrentHp"),
        "updateAllyMapsprite": _single_rts(mapsprite_rows, "UpdateAllyMapsprite"),
    }
    decrease_target, decrease_call_return = _absolute_call_target(rom, 0x20B12, "j_DecreaseGold")
    increase_target, increase_call_return = _absolute_call_target(
        rom, 0x20B20, "j_IncreaseCurrentHp"
    )
    mapsprite_target, mapsprite_call_return = _word_branch_target(
        rom, 0x20B34, opcode="6100", symbol="UpdateAllyMapsprite"
    )
    if (decrease_target, increase_target, mapsprite_target) != (
        entries["jDecreaseGold"],
        entries["jIncreaseCurrentHp"],
        entries["updateAllyMapsprite"],
    ):
        raise ValueError("church raise original helper call target drift")
    promotions = (disasm / PROMOTIONS_SOURCE).read_text(encoding="utf-8")
    base_match = re.search(
        r"promotionSection\s*&\s*;\s*Regular base classes\s*\n\s*([^\n]+)",
        promotions,
    )
    if base_match is None:
        raise ValueError("church raise regular base promotion source drift")
    regular_base = [
        eq[f"CLASS_{token.strip().upper()}"] for token in base_match.group(1).split(",")
    ]
    promotion_address = 0x21046
    promotion_rom = list(rom[promotion_address : promotion_address + len(regular_base) + 1])
    if promotion_rom != [len(regular_base), *regular_base]:
        raise ValueError("church raise promotion table ROM drift")
    aliases = {
        key: {
            "address": entries[key],
            "effectiveTarget": entries[target],
            "return": helper_returns[target] if target in helper_returns else entries[key] + 4,
        }
        for key, target in (
            ("jDecreaseGold", "decreaseGold"),
            ("jIncreaseCurrentHp", "increaseCurrentHp"),
            ("jGetCurrentHp", "getCurrentHp"),
            ("jGetLevel", "getLevel"),
            ("jGetClass", "getClass"),
            ("jGetGold", "getGold"),
        )
    }
    callback_seams = {
        "raiseRoute": {
            "call": 0x20A3C,
            "target": raise_target,
            "return": None,
            "fallthrough": raise_branch_fallthrough,
        },
        "decreaseGold": {
            "call": 0x20B12,
            "target": entries["decreaseGold"],
            "return": helper_returns["decreaseGold"],
            "callReturn": decrease_call_return,
        },
        "increaseCurrentHp": {
            "call": 0x20B20,
            "target": entries["increaseCurrentHp"],
            "return": helper_returns["increaseCurrentHp"],
            "callReturn": increase_call_return,
        },
        "mapsprite": {
            "call": 0x20B34,
            "target": entries["updateAllyMapsprite"],
            "return": helper_returns["updateAllyMapsprite"],
            "callReturn": mapsprite_call_return,
        },
    }
    helper_chronology = [
        {"role": "j-decrease-gold-entry", "pc": aliases["jDecreaseGold"]["address"]},
        {"role": "decrease-gold-entry", "pc": aliases["jDecreaseGold"]["effectiveTarget"]},
        {"role": "decrease-gold-return", "pc": aliases["jDecreaseGold"]["return"]},
        {
            "role": "j-increase-current-hp-entry",
            "pc": aliases["jIncreaseCurrentHp"]["address"],
        },
        {
            "role": "increase-current-hp-entry",
            "pc": aliases["jIncreaseCurrentHp"]["effectiveTarget"],
        },
        {
            "role": "increase-current-hp-return",
            "pc": aliases["jIncreaseCurrentHp"]["return"],
        },
        {"role": "mapsprite-entry", "pc": entries["updateAllyMapsprite"]},
        {"role": "mapsprite-return", "pc": helper_returns["updateAllyMapsprite"]},
    ]
    session_patches = _session_patch_plan(
        church_source=(disasm / CHURCH_SOURCE).read_text(encoding="utf-8"),
        helper_source=(disasm / CHURCH_HELPER_SOURCE).read_text(encoding="utf-8"),
        listing=listing,
        rom=rom,
    )
    return {
        "entryAddresses": entries,
        "instructionRows": instruction_rows,
        "branchTargets": branch_targets,
        "cost": {
            "perLevel": eq["CHURCHMENU_PER_LEVEL_RAISE_COST"],
            "promotedExtra": eq["CHURCHMENU_RAISE_COST_EXTRA_WHEN_PROMOTED"],
            "affordability": "cmp.l-d0-d1-bcc-equal-admitted",
        },
        "loop": {
            "currentHpZeroPredicate": "tst.w-d1-bhi-alive-skip",
            "counter": "deadMembersCount",
            "iteration": "dbf-d7-member-list-order",
            "promptAccept": "d0-equals-zero",
        },
        "ram": {
            "combatantData": eq["COMBATANT_DATA"],
            "combatantRecordSize": eq["COMBATANT_DATA_ENTRY_REAL_SIZE"],
            "classOffset": eq["COMBATANT_OFFSET_CLASS"],
            "levelOffset": eq["COMBATANT_OFFSET_LEVEL"],
            "hpMaxOffset": eq["COMBATANT_OFFSET_HP_MAX"],
            "hpCurrentOffset": eq["COMBATANT_OFFSET_HP_CURRENT"],
            "currentGold": eq["CURRENT_GOLD"],
            "targetsList": eq["TARGETS_LIST"],
            "targetsListLength": eq["TARGETS_LIST_LENGTH"],
            "dialogueName": eq["DIALOGUE_NAME_INDEX_1"],
            "dialogueNumber": eq["DIALOGUE_NUMBER"],
            "entityData": eq["ENTITY_DATA"],
            "mapspriteOffset": eq["ENTITYDEF_OFFSET_MAPSPRITE"],
            "hpCap": eq["CHAR_STATCAP_HP"],
            "currentPortrait": CURRENT_PORTRAIT_ADDRESS,
        },
        "aliases": aliases,
        "callbackSeams": callback_seams,
        "helperChronology": helper_chronology,
        "regularBaseClasses": regular_base,
        "harness": {
            "caseFrameBudget": 180,
            "bootstrapFrameBudget": 720,
            "harnessBase": 0xFF6800,
            "harnessStride": 32,
            "resultOffset": 20,
            "stackTop": 0xFFFF00,
            "actionStub": 0xFF6D00,
            "promptStub": 0xFF6D10,
            "terminalStub": 0xFF6D20,
            "checkSram": 0x6EA6,
            "generatedHarnessBytes": 7 * 32,
            "generatedStubBytes": 4,
            "generatedTerminalBytes": 18,
            "targetsSnapshotBytes": max(len(case[3]) for case in EXACT_CASE_MATRIX),
        },
        "sessionPatches": session_patches,
    }


def _cost(member: dict[str, Any], static: dict[str, Any]) -> int:
    result = int(member["level"]) * int(static["cost"]["perLevel"])
    if member["promoted"]:
        result += int(static["cost"]["promotedExtra"])
    return result


def _assert_exact_case_matrix(fixture: dict[str, Any], static: dict[str, Any]) -> None:
    """Keep seven inputs independent from their derived accepted observation."""
    actual = tuple(
        (
            case["caseId"],
            case["gold"],
            tuple(case["promptResults"]),
            tuple(
                (
                    member["memberId"],
                    member["classId"],
                    member["level"],
                    member["promoted"],
                    member["hpMax"],
                    member["hpCurrent"],
                    member["mapsprite"],
                )
                for member in case["members"]
            ),
        )
        for case in fixture["cases"]
    )
    if actual != EXACT_CASE_MATRIX:
        raise ValueError("church raise exact seven-case input matrix drift")
    base_classes = set(static["regularBaseClasses"])
    for case in fixture["cases"]:
        for member in case["members"]:
            if bool(member["promoted"]) != (member["classId"] not in base_classes):
                raise ValueError("church raise promoted/class source relation drift")


def assert_lua_role_contract() -> None:
    """Keep the observer's shared-PC dispatcher and critical roles reviewable."""
    source = OBSERVER.read_text(encoding="utf-8")
    if source.count("event.on_bus_exec(function()") != 1:
        raise ValueError("church raise Lua must register one callback per physical PC")
    for fragment in (
        "for _,event in ipairs(callbacks[address])do dispatch(address,event)end",
        "local ok,msg=pcall(function()",
        "if not ok then failure(msg)end",
        "remove_callbacks()",
        "os.remove(config.outputPath)",
        "local function register_callbacks()",
        "register_callbacks();register(h.checkSram",
        "generated_snapshots={};for _,span in ipairs({{address=h.harnessBase",
        "{address=h.actionStub,width=h.generatedStubBytes}",
        "{address=h.promptStub,width=h.generatedStubBytes}",
        "{address=h.terminalStub,width=h.generatedTerminalBytes}",
        'elseif role=="terminal-finalize" then finalize_success()',
        "w32(a+24,i==#config.cases and h.terminalStub or epc(i+1))",
        "w16(h.terminalStub,0x2C7C)",
        'capture_restoration_mismatch("gold",s.ram.currentGold',
        'capture_restoration_mismatch("generatedRamByte"',
        'capture_restoration_mismatch("a6",nil',
        '"restorationMismatch":\'..restoration_mismatch_json()',
    ):
        if fragment not in source:
            raise ValueError(f"church raise Lua callback contract drift: {fragment}")
    registered = set(re.findall(r'(?<![A-Za-z_])register\([^,]+,"([^"]+)"', source))
    if registered != LUA_REQUIRED_CALLBACK_ROLES:
        raise ValueError(
            "church raise Lua callback role drift: "
            f"missing={sorted(LUA_REQUIRED_CALLBACK_ROLES - registered)} "
            f"extra={sorted(registered - LUA_REQUIRED_CALLBACK_ROLES)}"
        )
    bootstrap = source.index('if role=="bootstrap-check-sram"')
    registration = source.index("register_callbacks();register(h.checkSram")
    emission = source.index("function write_harness()")
    if source.find("write_harness()", registration, bootstrap) >= 0 or emission < bootstrap:
        raise ValueError("church raise Lua generated RAM write precedes bootstrap snapshot")


def expected_observation(fixture: dict[str, Any], static: dict[str, Any]) -> dict[str, Any]:
    """Produce the small golden corpus; static retains the full PC inventory."""
    records = []
    entries = static["entryAddresses"]
    cap = static["ram"]["hpCap"]
    for case in fixture["cases"]:
        gold = case["gold"]
        dead = 0
        prompts = iter(case["promptResults"])
        mutations: list[dict[str, Any]] = []
        chronology: list[dict[str, Any]] = []
        out_members = []
        for member in case["members"]:
            if member["hpCurrent"] != 0:
                out_members.append({**member, "raised": False})
                continue
            dead += 1
            cost = _cost(member, static)
            accepted = next(prompts)
            changed = accepted == 0 and gold >= cost
            if changed:
                chronology.extend(
                    [
                        {
                            "memberId": member["memberId"],
                            "roles": list(SUCCESS_CHRONOLOGY_ROLES),
                        },
                    ]
                )
                gold -= cost
                out_members.append(
                    {**member, "hpCurrent": min(member["hpMax"], cap), "raised": True}
                )
                mutations.append(
                    {
                        "memberId": member["memberId"],
                        "cost": cost,
                        "hpAfter": min(member["hpMax"], cap),
                    }
                )
            else:
                out_members.append({**member, "raised": False})
        records.append(
            {
                "caseId": case["caseId"],
                "churchEntryPc": entries["churchMenu"],
                "raiseRoutePc": entries["raiseRoute"],
                "deadMemberCount": dead,
                "goldBefore": case["gold"],
                "goldAfter": gold,
                "members": out_members,
                "successChronology": chronology,
                "mutations": mutations,
            }
        )
    return {
        "system": "sf2-church-raise-lifecycle-runtime-v1",
        "caseOrder": fixture["caseOrder"],
        "records": records,
        "callbacksCleared": True,
        "restoration": {
            "gold": True,
            "combatantRecords": True,
            "mapspriteBytes": True,
            "dialogueScratch": True,
            "targetsListLength": True,
            "targetsListBytes": True,
            "currentPortrait": True,
            "generatedRam": True,
            "a6a7Balance": True,
            "sessionCartPatches": True,
        },
    }


def _assert_fixture(fixture: dict[str, Any], static: dict[str, Any]) -> None:
    if fixture["sourceContext"] != {
        "churchMenuEntryAddress": static["entryAddresses"]["churchMenu"],
        "raiseRouteAddress": static["entryAddresses"]["raiseRoute"],
    }:
        raise ValueError("church raise source-context golden drift")
    if fixture["caseOrder"] != list(CASE_IDS) or [
        case["caseId"] for case in fixture["cases"]
    ] != list(CASE_IDS):
        raise ValueError("church raise case order drift")
    _assert_exact_case_matrix(fixture, static)
    if fixture["static"] != _canonical_static(static):
        raise ValueError("church raise static golden drift")
    if fixture["acceptedObservation"] != expected_observation(fixture, static):
        raise ValueError("church raise accepted observation drift")


def _canonical_static(static: dict[str, Any]) -> dict[str, Any]:
    """Fixture golden: exact derived facts, without duplicating each H1 row."""
    return {
        key: static[key]
        for key in (
            "entryAddresses",
            "branchTargets",
            "cost",
            "loop",
            "ram",
            "sessionPatches",
        )
    }


def _observer_config(fixture: dict[str, Any], static: dict[str, Any]) -> dict[str, Any]:
    return {key: fixture[key] for key in ("caseOrder", "cases", "sourceContext")} | {
        "static": static,
        "owner": OWNER,
        "observerFailureContract": OBSERVER_FAILURE_CONTRACT,
    }


def _instrument_session_rom(rom_path: Path, static: dict[str, Any], destination: Path) -> None:
    payload = bytearray(rom_path.read_bytes())
    canonical = rom_path.read_bytes()
    seen: list[range] = []
    for patch in static["sessionPatches"]:
        data = bytes.fromhex(patch["hex"])
        original = bytes.fromhex(patch["originalHex"])
        if patch["width"] != len(data) or len(original) != len(data):
            raise ValueError("church raise session patch width drift")
        span = range(patch["address"], patch["address"] + len(data))
        if any(set(span) & set(existing) for existing in seen):
            raise ValueError("church raise session patch overlap")
        if span.stop > len(payload):
            raise ValueError("church raise session patch outside ROM")
        if payload[span.start : span.stop] != original:
            raise ValueError(f"church raise session original-byte drift at 0x{span.start:X}")
        payload[span.start : span.stop] = data
        if payload[span.start : span.stop] != data:
            raise ValueError(f"church raise session patch write drift at 0x{span.start:X}")
        seen.append(span)
    destination.write_bytes(payload)
    if rom_path.read_bytes() != canonical:
        raise ValueError("church raise canonical ROM mutation detected")


def _assert_church_status(status_path: Path) -> None:
    required = (
        "milestone:observer-loaded",
        "milestone:direct-function-probe-armed",
        "milestone:direct-function-probe",
    )
    assert_observer_status(
        status_path,
        owner=OWNER,
        schema_path=FAILURE_SCHEMA,
        required_milestones=required,
    )
    lines = status_path.read_text(encoding="utf-8").splitlines()
    if (
        lines.count("milestone:observer-loaded") != 1
        or lines.count("milestone:direct-function-probe-armed") != 1
        or lines.count("milestone:direct-function-probe") != 1
    ):
        raise RuntimeError("church raise direct-probe milestone multiplicity drift")
    if lines.index("milestone:observer-loaded") > lines.index(
        "milestone:direct-function-probe-armed"
    ):
        raise RuntimeError("church raise observer-load milestone order drift")
    if lines[-3] != "milestone:do-raise:mixed-decline-then-success":
        raise RuntimeError("church raise final callback milestone drift")
    for case_id in CASE_IDS:
        if lines.count(f"milestone:case-entry:{case_id}") != 1:
            raise RuntimeError(f"church raise case milestone multiplicity drift: {case_id}")
    if lines.count("milestone:church-entry") != len(CASE_IDS) or lines.count(
        "milestone:raise-route"
    ) != len(CASE_IDS):
        raise RuntimeError("church raise Church entry/route milestone multiplicity drift")
    cursor = lines.index("milestone:direct-function-probe")
    for case_id in CASE_IDS:
        case_entry = f"milestone:case-entry:{case_id}"
        try:
            cursor = lines.index(case_entry, cursor + 1)
        except ValueError as error:
            raise RuntimeError(f"church raise ordered case milestone drift: {case_id}") from error
        for milestone in ("milestone:church-entry", "milestone:raise-route"):
            try:
                cursor = lines.index(milestone, cursor + 1)
            except ValueError as error:
                raise RuntimeError(
                    f"church raise ordered callback milestone drift: {case_id} {milestone}"
                ) from error


def _failure_diagnostic(
    status_path: Path, static: dict[str, Any] | None = None
) -> dict[str, Any] | None:
    value = callback_failure_status(status_path, owner=OWNER, schema_path=FAILURE_SCHEMA)
    if value is not None and static is not None:
        if value["caseId"] not in CASE_IDS:
            raise ValueError("church raise callback failure case identity drift")
        restoration = value["restoration"]
        if restoration["sessionCartPatches"] is not False:
            raise ValueError("church raise Lua cannot claim session-cart restoration")
        if not restoration["callbacksCleared"] or not restoration["outputRemoved"]:
            raise ValueError("church raise callback failure cleanup claim drift")
        mismatch = value["restorationMismatch"]
        restoration_ok = all(restoration[key] for key in RESTORATION_CHECK_KEYS)
        if not restoration["scopeArmed"]:
            if not restoration_ok or mismatch is not None:
                raise ValueError("church raise unarmed restoration state drift")
        elif restoration_ok:
            if mismatch is not None:
                raise ValueError("church raise successful restoration must have null mismatch")
        else:
            if mismatch is None:
                raise ValueError("church raise failed restoration requires first mismatch")
            domain = mismatch["domain"]
            if (
                domain not in RESTORATION_MISMATCH_STATE_KEYS
                or restoration[RESTORATION_MISMATCH_STATE_KEYS[domain]]
            ):
                raise ValueError("church raise restoration mismatch/state drift")
            if domain in {"a6", "a7"}:
                if mismatch["address"] is not None:
                    raise ValueError("church raise A6/A7 mismatch address drift")
            elif mismatch["address"] is None:
                raise ValueError("church raise RAM mismatch address drift")
        pending = value["pendingCallback"]
        if (
            pending["expectedCaseId"] != value["caseId"]
            or pending["caseIndex"] != CASE_IDS.index(value["caseId"]) + 1
        ):
            raise ValueError("church raise callback failure case state drift")
        if pending["kind"] == "route":
            seam = static["callbackSeams"]["raiseRoute"]
            if value["actualPc"] != seam["target"] or (
                value["expectedCallPc"],
                value["expectedTargetPc"],
                value["expectedReturnPc"],
            ) != (seam["call"], seam["target"], seam["return"]):
                raise ValueError("church raise callback failure route triple drift")
        if pending["kind"] == "helper":
            events = {event["role"]: event["pc"] for event in static["helperChronology"]}
            expected_actual = events.get(value["role"])
            if expected_actual is None or value["actualPc"] != expected_actual:
                raise ValueError("church raise callback failure stale actual PC")
            seam_name = (
                "decreaseGold"
                if "decrease-gold" in value["role"]
                else "increaseCurrentHp"
                if "increase-current-hp" in value["role"]
                else "mapsprite"
            )
            seam = static["callbackSeams"][seam_name]
            if (
                value["expectedCallPc"],
                value["expectedTargetPc"],
                value["expectedReturnPc"],
            ) != (seam["call"], seam["target"], seam["return"]):
                raise ValueError("church raise callback failure helper triple drift")
            if pending["expectedChronology"] != static["helperChronology"]:
                raise ValueError("church raise callback failure expected chronology drift")
    return value


def preflight_church_raise_lifecycle(rom_path: Path) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=OWNER)
    assert_lua_role_contract()
    static = build_static_contract(rom_path)
    _assert_fixture(fixture, static)
    with tempfile.TemporaryDirectory(prefix="sf2-church-raise-") as temporary:
        session = Path(temporary) / "church-raise-session.bin"
        _instrument_session_rom(rom_path, static, session)
        if not session.exists():
            raise ValueError("church raise session ROM creation drift")
    return {
        "Fixture": fixture["system"],
        "Cases": len(CASE_IDS),
        "SessionPatches": len(static["sessionPatches"]),
        "Status": "PRELAUNCH-PASS",
    }


def verify_church_raise_lifecycle(
    rom_path: Path, upstream_path: Path = UPSTREAM, *, timeout_seconds: int = 180
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=OWNER)
    assert_lua_role_contract()
    static = build_static_contract(rom_path, upstream_path)
    _assert_fixture(fixture, static)
    expected = expected_observation(fixture, static)
    validate_json(expected, OBSERVATION_SCHEMA, owner="church raise expected observation")
    observed: dict[str, Any]
    session_deleted = False
    try:
        with tempfile.TemporaryDirectory(prefix="sf2-church-raise-") as temporary:
            session = Path(temporary) / "church-raise-session.bin"
            _instrument_session_rom(rom_path, static, session)
            config = _observer_config(fixture, static)
            status_path = repo_path(f"local/derived/h3/{OWNER}.status.txt")
            try:
                observed = _with_instrumented_rom_database(
                    session,
                    "church-raise-lifecycle",
                    lambda: run_observer(
                        observer_path=OBSERVER,
                        rom_path=session,
                        config=config,
                        output_name=OWNER,
                        timeout_seconds=timeout_seconds,
                    ),
                )
            except Exception:
                _failure_diagnostic(status_path, static)
                raise
            _assert_church_status(status_path)
        session_deleted = not session.exists()
        observed["restoration"]["sessionCartPatches"] = session_deleted
        validate_json(observed, OBSERVATION_SCHEMA, owner="church raise observation")
        if observed != expected:
            differences = {
                record["caseId"]: {
                    key: {"expected": golden[key], "actual": record.get(key)}
                    for key in golden
                    if record.get(key) != golden[key]
                }
                for record, golden in zip(observed["records"], expected["records"], strict=True)
                if record != golden
            }
            milestones = status_path.read_text(encoding="utf-8").splitlines()
            raise ValueError(
                f"church raise runtime observation mismatch: {differences}; status={milestones}"
            )
        OBSERVED_OUTPUT.write_text(json.dumps(observed, indent=2) + "\n", encoding="utf-8")
    except Exception:
        OBSERVED_OUTPUT.unlink(missing_ok=True)
        raise
    return {
        "Fixture": fixture["system"],
        "Cases": len(CASE_IDS),
        "Status": "PASS",
        "SessionRomDeleted": session_deleted,
    }
