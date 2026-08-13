"""Church Cure's bounded poison, stun, and curse transaction lifecycle.

The H3 observer enters the original ``ChurchMenu`` at 0x20A02.  It controls
only menu selection, prompt return values, source-list population, and
presentation bypasses in a disposable ROM.  The loops, affordability tests,
and mutation helpers remain original code.
"""

from __future__ import annotations

import json
import re
import subprocess
import tempfile
from functools import cache
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
from sf2tool.rom import inspect_rom

OWNER = "church-cure-lifecycle"
FIXTURE = repo_path("tests/fixtures/h3/church-cure-lifecycle-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h3-church-cure-lifecycle-fixture.schema.json")
OBSERVATION_SCHEMA = repo_path("schemas/h3-church-cure-lifecycle-observation.schema.json")
FAILURE_SCHEMA = repo_path("schemas/h3/church-cure-lifecycle-callback-failure.schema.json")
OBSERVER = repo_path("tools/bizhawk/church_cure_lifecycle_observer.lua")
UPSTREAM = repo_path("local/upstream/SF2DISASM")
OBSERVED_OUTPUT = repo_path(f"local/derived/h3/{OWNER}.observed.json")
OBSERVER_FAILURE_CONTRACT = observer_failure_contract(OWNER)
STATUS_PREFIX = CALLBACK_FAILURE_PREFIX
CANONICAL_ROM_SHA256 = "9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9"
UPSTREAM_COMMIT = "c834c652b6862bc5679fd7f69a38a7093206efc6"

CASE_IDS = (
    "no-curable-status-no-prompt",
    "poison-decline",
    "poison-one-below-cost",
    "poison-exact-cost-success",
    "stun-decline",
    "stun-one-below-cost",
    "stun-exact-cost-success",
    "curse-dark-sword-decline",
    "curse-dark-sword-one-below-cost",
    "curse-dark-sword-exact-cost-success",
    "poison-stun-curse-ordered-success",
)
FAMILIES = ("poison", "stun", "curse")
FAMILY_MASKS = {"poison": 2, "stun": 1, "curse": 4}
FAMILY_ROLES = {
    "poison": (
        "j-decrease-gold-entry",
        "decrease-gold-entry",
        "decrease-gold-return",
        "j-set-status-effects-entry",
        "set-status-effects-entry",
        "set-status-effects-return",
    ),
    "stun": (
        "j-decrease-gold-entry",
        "decrease-gold-entry",
        "decrease-gold-return",
        "j-set-status-effects-entry",
        "set-status-effects-entry",
        "set-status-effects-return",
    ),
    "curse": (
        "j-decrease-gold-entry",
        "decrease-gold-entry",
        "decrease-gold-return",
        "j-unequip-all-items-if-not-cursed-entry",
        "unequip-all-items-if-not-cursed-entry",
        "update-combatant-stats-tail-entry",
        "update-combatant-stats-tail-return",
    ),
}
REQUIRED_LUA_ROLES = frozenset(
    {
        "registration",
        "bootstrap-check-sram",
        "case-entry",
        "case-result",
        "terminal-finalize",
        "church-entry",
        "cure-route",
        "action-stub",
        "prompt-poison",
        "prompt-stun",
        "prompt-curse",
        "do-poison",
        "do-stun",
        "do-curse",
        "j-decrease-gold-entry",
        "decrease-gold-entry",
        "decrease-gold-return",
        "j-set-status-effects-entry",
        "set-status-effects-entry",
        "set-status-effects-return",
        "j-unequip-all-items-if-not-cursed-entry",
        "unequip-all-items-if-not-cursed-entry",
        "update-combatant-stats-tail-entry",
        "update-combatant-stats-tail-return",
        "bootstrap-watchdog",
        "transition-watchdog",
        "case-watchdog",
    }
)

CHURCH = Path("code/common/menus/church/churchactions_1.asm")
CHURCH_HELPER = Path("code/common/menus/church/churchactions_2.asm")
STATS = Path("code/common/stats/combatantstats_2.asm")
ITEMS = Path("code/common/stats/itemstats.asm")
UPDATE = Path("code/common/stats/updatecombatantstats.asm")
ITEM_DEFINITIONS = Path("data/stats/items/itemdefs.asm")
ENUMS = Path("sf2enums.asm")
CONSTANTS = Path("sf2const.asm")


def _normal(text: str) -> str:
    return re.sub(r"\s+", " ", text.split(";", 1)[0].strip()).lower()


def _section(source: str, symbol: str) -> str:
    match = re.search(rf"^{re.escape(symbol)}:\s*$", source, re.MULTILINE)
    if match is None:
        raise ValueError(f"church cure source function missing: {symbol}")
    end = source.find(f"; End of function {symbol}", match.end())
    if end < 0:
        raise ValueError(f"church cure source function end missing: {symbol}")
    return source[match.start() : end]


def _require_order(source: str, symbol: str, fragments: tuple[str, ...]) -> None:
    rows = [_normal(row) for row in _section(source, symbol).splitlines()]
    cursor = 0
    for fragment in fragments:
        try:
            cursor = rows.index(_normal(fragment), cursor) + 1
        except ValueError as error:
            raise ValueError(f"church cure source guard drift: {symbol} {fragment}") from error


def _listing_function(listing: str, symbol: str) -> tuple[dict[str, int], list[dict[str, Any]]]:
    match = re.search(rf"^[0-9A-F]{{8}}\s+{re.escape(symbol)}:\s*$", listing, re.MULTILINE)
    if match is None:
        raise ValueError(f"church cure H1 function missing: {symbol}")
    end = listing.find(f"; End of function {symbol}", match.end())
    if end < 0:
        raise ValueError(f"church cure H1 function end missing: {symbol}")
    labels: dict[str, int] = {}
    rows: list[dict[str, Any]] = []
    for line in listing[match.start() : end].splitlines():
        label = re.fullmatch(r"([0-9A-F]{8})\s+([@A-Za-z_][@A-Za-z0-9_]*):\s*", line)
        if label:
            labels[label.group(2)] = int(label.group(1), 16)
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
        raise ValueError(f"church cure H1 entry missing: {symbol}")
    return labels, rows


def _listing_label(listing: str, symbol: str) -> int:
    match = re.search(rf"^([0-9A-F]{{8}})\s+{re.escape(symbol)}:\s*$", listing, re.MULTILINE)
    if match is None:
        raise ValueError(f"church cure H1 label missing: {symbol}")
    return int(match.group(1), 16)


def _find(rows: list[dict[str, Any]], address: int, fragment: str, width: int) -> dict[str, Any]:
    matches = [
        row for row in rows if row["address"] == address and fragment.lower() in row["text"].lower()
    ]
    if len(matches) != 1:
        raise ValueError(f"church cure H1 use-site drift at 0x{address:X}: {fragment}")
    row = matches[0]
    if len(row["hex"]) != width * 2:
        raise ValueError(f"church cure H1 instruction width drift at 0x{address:X}")
    return row


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
        raise ValueError(f"church cure ROM short at 0x{address:X}")
    return value


@cache
def _h1_cells(listing: str) -> dict[int, int]:
    cells: dict[int, int] = {}
    for line in listing.splitlines():
        match = re.match(r"^([0-9A-F]{8})\s+((?:[0-9A-F]{4}\s+)+)", line)
        if match is None:
            continue
        start = int(match.group(1), 16)
        for offset, value in enumerate(bytes.fromhex(re.sub(r"\s+", "", match.group(2)))):
            cells[start + offset] = value
    return cells


def _h1_span(listing: str, address: int, width: int) -> str:
    cells = _h1_cells(listing)
    if any(cell not in cells for cell in range(address, address + width)):
        raise ValueError(f"church cure H1 span incomplete: 0x{address:X}")
    return bytes(cells[cell] for cell in range(address, address + width)).hex().upper()


def _relative_target(rom: bytes, address: int, opcode: str) -> int:
    if _rom_hex(rom, address, 2) != opcode:
        raise ValueError(f"church cure ROM opcode drift at 0x{address:X}")
    return address + 2 + int.from_bytes(rom[address + 2 : address + 4], "big", signed=True)


def _absolute_target(rom: bytes, address: int) -> tuple[int, int]:
    if _rom_hex(rom, address, 2) != "4EB9":
        raise ValueError(f"church cure JSR drift at 0x{address:X}")
    return int.from_bytes(rom[address + 2 : address + 6], "big"), address + 6


def _source_guards(disasm: Path) -> None:
    church = (disasm / CHURCH).read_text(encoding="utf-8")
    helper = (disasm / CHURCH_HELPER).read_text(encoding="utf-8")
    stats = (disasm / STATS).read_text(encoding="utf-8")
    items = (disasm / ITEMS).read_text(encoding="utf-8")
    update = (disasm / UPDATE).read_text(encoding="utf-8")
    _require_order(
        church,
        "ChurchMenu",
        (
            "jsr j_ExecuteDiamondMenu",
            "bra.w @CheckRaiseAction",
            "@CheckCureAction:",
            "cmpi.w #1,d0",
            "bne.w @CheckPromoAction",
            "bsr.w Church_GetCurrentForceMemberInfo",
            "@CountPoisonedMembers_Loop:",
            "andi.w #STATUSEFFECT_POISON,d3",
            "move.l #CHURCHMENU_CURE_POISON_COST,actionCost(a6)",
            "jsr j_alt_YesNoPrompt",
            "cmpi.w #0,d0",
            "beq.w @CheckCurePoisonCost",
            "cmp.l d0,d1",
            "bcc.s @DoCurePoison",
            "@DoCurePoison:",
            "jsr j_DecreaseGold",
            "andi.w #(STATUSEFFECT_MASK-STATUSEFFECT_POISON),d1",
            "jsr j_SetStatusEffects",
            "dbf d7,@CountPoisonedMembers_Loop",
            "@CureStun:",
            "bsr.w Church_CureStun",
            "@CountCursedMembers_Loop:",
            "andi.w #STATUSEFFECT_CURSE,d2",
            "jsr j_GetItemBySlotAndHeldItemsNumber",
            "@CalculateCureCurseCost_Loop:",
            "jsr j_IsItemCursed",
            "jsr j_GetItemDefinitionAddress",
            "lsr.w #2,d4",
            "add.l d4,d3",
            "dbf d6,@CalculateCureCurseCost_Loop",
            "cmp.l d0,d1",
            "bcc.s @DoCureCurse",
            "@DoCureCurse:",
            "jsr j_DecreaseGold",
            "jsr j_UnequipAllItemsIfNotCursed",
            "dbf d7,@CountCursedMembers_Loop",
        ),
    )
    _require_order(
        helper,
        "Church_CureStun",
        (
            "bsr.s Church_GetCurrentForceMemberInfo",
            "@Loop:",
            "andi.w #STATUSEFFECT_STUN,d3",
            "move.l #CHURCHMENU_CURE_STUN_COST,actionCost(a6)",
            "jsr j_alt_YesNoPrompt",
            "cmpi.w #0,d0",
            "beq.w @CheckGold",
            "cmp.l d0,d1",
            "bcc.s @DoCureStun",
            "@DoCureStun:",
            "jsr j_DecreaseGold",
            "andi.w #(STATUSEFFECT_MASK-STATUSEFFECT_STUN),d1",
            "jsr j_SetStatusEffects",
            "dbf d7,@Loop",
        ),
    )
    _require_order(
        stats,
        "SetStatusEffects",
        ("moveq #COMBATANT_OFFSET_STATUSEFFECTS,d7", "bsr.w SetCombatantWord", "rts"),
    )
    _require_order(
        items,
        "UnequipAllItemsIfNotCursed",
        (
            "btst #ITEMTYPE_BIT_CURSED,ITEMDEF_OFFSET_TYPE(a0)",
            "bclr #ITEMENTRY_BIT_EQUIPPED,ITEMENTRY_OFFSET_INDEX_AND_EQUIPPED_BIT(a1)",
            "dbf d0,@Loop",
            "bra.w UpdateCombatantStats",
        ),
    )
    _require_order(
        update,
        "UpdateCombatantStats",
        (
            "@Loop:",
            "btst #ITEMENTRY_BIT_EQUIPPED,ITEMENTRY_OFFSET_INDEX_AND_EQUIPPED_BIT(a1)",
            "ori.w #STATUSEFFECT_CURSE,d3",
            "bsr.w SetStatusEffects",
            "rts",
        ),
    )


def _assert_input_identity(rom_path: Path, upstream_path: Path) -> None:
    if inspect_rom(rom_path)["sha256"] != CANONICAL_ROM_SHA256:
        raise ValueError("church cure canonical ROM SHA-256 drift")
    revision = subprocess.run(
        ["git", "-C", str(upstream_path), "rev-parse", "HEAD"],
        check=True,
        capture_output=True,
        text=True,
    ).stdout.strip()
    if revision != UPSTREAM_COMMIT:
        raise ValueError("church cure pinned SF2DISASM master revision drift")


def _session_patches(listing: str, rom: bytes) -> list[dict[str, Any]]:
    # Source/H1-owned known instruction spans.  Original bytes come from H1 and
    # are separately required to match the immutable canonical candidate.
    specs = (
        (0x20A18, 8, "entry-presentation-bypass", "4E714E714E714E71"),
        (0x20A20, 6, "entry-portrait-close-bypass", "4E714E714E71"),
        (0x20A30, 6, "controlled-cure-selection", "4EB900FF6D00"),
        (0x20B60, 4, "cure-investigation-text-bypass", "4E714E71"),
        (0x20BA2, 4, "poison-member-text-bypass", "4E714E71"),
        (0x20BB4, 4, "poison-cost-text-bypass", "4E714E71"),
        (0x20BB8, 6, "poison-gold-open-bypass", "4E714E714E71"),
        (0x20BBE, 6, "poison-controlled-prompt", "4EB900FF6D10"),
        (0x20BC4, 6, "poison-gold-close-bypass", "4E714E714E71"),
        (0x20BD2, 4, "poison-decline-text-bypass", "4E714E71"),
        (0x20BEC, 4, "poison-insufficient-text-bypass", "4E714E71"),
        (
            0x20C10,
            10,
            "poison-success-presentation-bypass",
            "4E714E714E714E714E71",
            "4E4000164EBA06224E71",
        ),
        (0x20C20, 4, "poison-success-text-bypass", "4E714E71"),
        (0x20C36, 4, "poison-none-text-bypass", "4E714E71"),
        (0x2119E, 4, "stun-member-text-bypass", "4E714E71"),
        (0x211B0, 4, "stun-cost-text-bypass", "4E714E71"),
        (0x211B4, 6, "stun-gold-open-bypass", "4E714E714E71"),
        (0x211BA, 6, "stun-controlled-prompt", "4EB900FF6D10"),
        (0x211C0, 6, "stun-gold-close-bypass", "4E714E714E71"),
        (0x211CE, 4, "stun-decline-text-bypass", "4E714E71"),
        (0x211E8, 4, "stun-insufficient-text-bypass", "4E714E71"),
        (
            0x2120C,
            10,
            "stun-success-presentation-bypass",
            "4E714E714E714E714E71",
            "4E4000164EBA00264E71",
        ),
        (0x2121C, 4, "stun-success-text-bypass", "4E714E71"),
        (0x21232, 4, "stun-none-text-bypass", "4E714E71"),
        (0x20C6E, 4, "curse-member-text-bypass", "4E714E71"),
        (0x20CB6, 4, "curse-cost-text-bypass", "4E714E71"),
        (0x20CBA, 6, "curse-gold-open-bypass", "4E714E714E71"),
        (0x20CC0, 6, "curse-controlled-prompt", "4EB900FF6D10"),
        (0x20CC6, 6, "curse-gold-close-bypass", "4E714E714E71"),
        (0x20CD4, 4, "curse-decline-text-bypass", "4E714E71"),
        (0x20CEE, 4, "curse-insufficient-text-bypass", "4E714E71"),
        (
            0x20D0C,
            10,
            "curse-success-presentation-bypass",
            "4E714E714E714E714E71",
            "4E4000164EBA05264E71",
        ),
        (0x20D1C, 4, "curse-success-text-bypass", "4E714E71"),
        (0x20D32, 4, "curse-none-text-bypass", "4E714E71"),
        (0x21028, 6, "cure-exit-cleanup", "4EF900020A5C"),
        (0x21156, 6, "controlled-force-list", "4E714E714E71"),
    )
    patches: list[dict[str, Any]] = []
    spans: list[range] = []
    for spec in specs:
        address, width, purpose, replacement, *pinned_original = spec
        h1_original = _h1_span(listing, address, width)
        original = pinned_original[0] if pinned_original else h1_original
        if len(replacement) != width * 2:
            raise ValueError(f"church cure session source/H1/ROM plan drift: {purpose}")
        if _rom_hex(rom, address, width) != original:
            raise ValueError(f"church cure canonical session byte drift: {purpose}")
        if pinned_original and h1_original[:12] != original[:12]:
            raise ValueError(f"church cure H1 relocation shape drift: {purpose}")
        span = range(address, address + width)
        if any(set(span) & set(existing) for existing in spans):
            raise ValueError(f"church cure session patch overlap: {purpose}")
        spans.append(span)
        patches.append(
            {
                "address": address,
                "width": width,
                "originalHex": original,
                "hex": replacement,
                "purpose": purpose,
            }
        )
    return patches


def build_static_contract(rom_path: Path, upstream_path: Path = UPSTREAM) -> dict[str, Any]:
    _assert_input_identity(rom_path, upstream_path)
    disasm = upstream_path / "disasm"
    _source_guards(disasm)
    equates = _equates((disasm / ENUMS).read_text(encoding="utf-8")) | _equates(
        (disasm / CONSTANTS).read_text(encoding="utf-8")
    )
    required = (
        "STATUSEFFECT_STUN",
        "STATUSEFFECT_POISON",
        "STATUSEFFECT_CURSE",
        "STATUSEFFECT_MASK",
        "CHURCHMENU_CURE_POISON_COST",
        "CHURCHMENU_CURE_STUN_COST",
        "ITEM_DARK_SWORD",
        "ITEMTYPE_CURSED",
        "CURRENT_GOLD",
        "TARGETS_LIST",
        "TARGETS_LIST_LENGTH",
        "COMBATANT_DATA",
        "COMBATANT_DATA_ENTRY_REAL_SIZE",
        "COMBATANT_OFFSET_STATUSEFFECTS",
        "COMBATANT_OFFSET_ITEMS",
        "ITEMENTRY_SIZE",
        "ITEM_NOTHING",
        "ITEMDEF_SIZE",
        "ITEMDEF_OFFSET_PRICE",
        "ENTITY_DATA",
        "ENTITYDEF_OFFSET_MAPSPRITE",
        "DIALOGUE_NAME_INDEX_1",
        "DIALOGUE_NUMBER",
        "ITEMENTRY_BIT_EQUIPPED",
    )
    missing = [name for name in required if name not in equates]
    if missing:
        raise ValueError(f"church cure authoritative constants missing: {missing}")
    listing = (upstream_path / "build/sf2build-h1.lst").read_text(encoding="utf-8")
    church_labels, church_rows = _listing_function(listing, "ChurchMenu")
    stun_labels, stun_rows = _listing_function(listing, "Church_CureStun")
    aliases = {
        name: _listing_function(listing, name)
        for name in ("j_DecreaseGold", "j_SetStatusEffects", "j_UnequipAllItemsIfNotCursed")
    }
    functions = {
        name: _listing_function(listing, name)
        for name in (
            "DecreaseGold",
            "SetStatusEffects",
            "UnequipAllItemsIfNotCursed",
            "UpdateCombatantStats",
        )
    }
    rom = rom_path.read_bytes()
    use_sites = (
        (0x20A3C, "bra.w", 4),
        (0x20BFA, "j_DecreaseGold", 6),
        (0x20C0A, "j_SetStatusEffects", 6),
        (0x211F6, "j_DecreaseGold", 6),
        (0x21206, "j_SetStatusEffects", 6),
        (0x20CFC, "j_DecreaseGold", 6),
        (0x20D06, "j_UnequipAllItemsIfNotCursed", 6),
    )
    instruction_rows = {
        str(address): _find(church_rows if address < 0x21100 else stun_rows, address, text, width)
        for address, text, width in use_sites
    }
    for row in instruction_rows.values():
        h1 = bytes.fromhex(row["hex"])
        actual = bytes.fromhex(_rom_hex(rom, row["address"], len(h1)))
        if any(expected and expected != got for expected, got in zip(h1, actual, strict=True)):
            raise ValueError(f"church cure H1/ROM use-site drift: 0x{row['address']:X}")
    entries = {
        "churchMenu": church_labels["ChurchMenu"],
        "cureRoute": church_labels["@CheckCureAction"],
        "poisonDo": church_labels["@DoCurePoison"],
        "stunDo": stun_labels["@DoCureStun"],
        "curseDo": church_labels["@DoCureCurse"],
    }
    alias_info: dict[str, dict[str, int]] = {}
    for alias, target in (
        ("j_DecreaseGold", "DecreaseGold"),
        ("j_SetStatusEffects", "SetStatusEffects"),
        ("j_UnequipAllItemsIfNotCursed", "UnequipAllItemsIfNotCursed"),
    ):
        labels, rows = aliases[alias]
        address = labels[alias]
        if _find(rows, address, "jmp", 4)["hex"][:4] != "4EFA":
            raise ValueError(f"church cure alias instruction drift: {alias}")
        effective = _relative_target(rom, address, "4EFA")
        target_labels, target_rows = functions[target]
        if effective != target_labels[target]:
            raise ValueError(f"church cure alias effective target drift: {alias}")
        alias_info[alias] = {"address": address, "effectiveTarget": effective}
    decrease_target, decrease_return = _absolute_target(rom, 0x20BFA)
    set_target, set_return = _absolute_target(rom, 0x20C0A)
    unequip_target, unequip_return = _absolute_target(rom, 0x20D06)
    for call, target in (
        (0x211F6, alias_info["j_DecreaseGold"]["address"]),
        (0x21206, alias_info["j_SetStatusEffects"]["address"]),
        (0x20CFC, alias_info["j_DecreaseGold"]["address"]),
    ):
        if _absolute_target(rom, call)[0] != target:
            raise ValueError(f"church cure mutation-call target drift: 0x{call:X}")

    def single_rts(rows: list[dict[str, Any]], symbol: str) -> int:
        returns = [row["address"] for row in rows if row["text"].strip().lower() == "rts"]
        if len(returns) != 1:
            raise ValueError(f"church cure H1 return identity drift: {symbol}")
        return returns[0]

    update_labels, update_rows = functions["UpdateCombatantStats"]
    update_return = single_rts(update_rows, "UpdateCombatantStats")
    decrease_rts = single_rts(functions["DecreaseGold"][1], "DecreaseGold")
    status_rts = single_rts(functions["SetStatusEffects"][1], "SetStatusEffects")
    unequip_rows = functions["UnequipAllItemsIfNotCursed"][1]
    tail = next(row for row in unequip_rows if "bra.w UpdateCombatantStats" in row["text"])
    if _relative_target(rom, tail["address"], "6000") != update_labels["UpdateCombatantStats"]:
        raise ValueError("church cure UnequipAllItemsIfNotCursed tail target drift")
    item_source = (disasm / ITEM_DEFINITIONS).read_text(encoding="utf-8")
    dark_sword_source = re.search(
        r";\s*70:\s*Dark Sword\s*\n(?P<body>.*?)(?=;\s*71:)",
        item_source,
        re.DOTALL,
    )
    if dark_sword_source is None:
        raise ValueError("church cure Dark Sword source definition drift")
    dark_sword_body = dark_sword_source.group("body")
    price_match = re.search(r"^\s*price\s+(\d+)\s*$", dark_sword_body, re.MULTILINE)
    item_type_match = re.search(r"^\s*itemType\s+([^\n]+)$", dark_sword_body, re.MULTILINE)
    if price_match is None or item_type_match is None or "CURSED" not in item_type_match.group(1):
        raise ValueError("church cure Dark Sword source fields drift")
    item_price = int(price_match.group(1))
    item_price_address = (
        _listing_label(listing, "table_ItemDefinitions")
        + equates["ITEM_DARK_SWORD"] * equates["ITEMDEF_SIZE"]
        + equates["ITEMDEF_OFFSET_PRICE"]
    )
    if int.from_bytes(rom[item_price_address : item_price_address + 2], "big") != item_price:
        raise ValueError("church cure Dark Sword source/H1/ROM price drift")
    return {
        "entryAddresses": entries,
        "constants": {
            "poisonMask": equates["STATUSEFFECT_POISON"],
            "stunMask": equates["STATUSEFFECT_STUN"],
            "curseMask": equates["STATUSEFFECT_CURSE"],
            "statusMask": equates["STATUSEFFECT_MASK"],
            "poisonCost": equates["CHURCHMENU_CURE_POISON_COST"],
            "stunCost": equates["CHURCHMENU_CURE_STUN_COST"],
            "darkSwordItem": equates["ITEM_DARK_SWORD"],
            "darkSwordPrice": item_price,
            "darkSwordCureCost": item_price >> 2,
            "cursedItemType": equates["ITEMTYPE_CURSED"],
            "equippedBit": equates["ITEMENTRY_BIT_EQUIPPED"],
        },
        "ram": {
            "currentGold": equates["CURRENT_GOLD"],
            "targetsList": equates["TARGETS_LIST"],
            "targetsListLength": equates["TARGETS_LIST_LENGTH"],
            "combatantData": equates["COMBATANT_DATA"],
            "combatantRecordSize": equates["COMBATANT_DATA_ENTRY_REAL_SIZE"],
            "statusOffset": equates["COMBATANT_OFFSET_STATUSEFFECTS"],
            "itemsOffset": equates["COMBATANT_OFFSET_ITEMS"],
            "itemSize": equates["ITEMENTRY_SIZE"],
            "itemNothing": equates["ITEM_NOTHING"],
            "dialogueName": equates["DIALOGUE_NAME_INDEX_1"],
            "dialogueNumber": equates["DIALOGUE_NUMBER"],
            "entityData": equates["ENTITY_DATA"],
            "mapspriteOffset": equates["ENTITYDEF_OFFSET_MAPSPRITE"],
            "currentPortrait": CURRENT_PORTRAIT_ADDRESS,
        },
        "aliases": alias_info,
        "callbackSeams": {
            "decreaseGold": {
                "call": 0x20BFA,
                "target": decrease_target,
                "return": decrease_rts,
            },
            "setStatusEffects": {
                "call": 0x20C0A,
                "target": set_target,
                "return": status_rts,
            },
            "unequip": {"call": 0x20D06, "target": unequip_target, "return": unequip_return},
            "updateStats": {
                "target": update_labels["UpdateCombatantStats"],
                "return": update_return,
            },
            "cureRoute": {"call": 0x20A3C, "target": _relative_target(rom, 0x20A3C, "6000")},
        },
        "instructionRows": instruction_rows,
        "harness": {
            "caseFrameBudget": 180,
            "transitionFrameBudget": 60,
            "bootstrapFrameBudget": 720,
            "harnessBase": 0xFF6800,
            "harnessStride": 32,
            "resultOffset": 20,
            "stackTop": 0xFFFF00,
            "actionStub": 0xFF6D00,
            "promptStub": 0xFF6D10,
            "terminalStub": 0xFF6D20,
            "checkSram": 0x6EA6,
            "generatedHarnessBytes": len(CASE_IDS) * 32,
            "generatedStubBytes": 4,
            "generatedTerminalBytes": 18,
            "targetsSnapshotBytes": 1,
        },
        "sessionPatches": _session_patches(listing, rom),
    }


def _family_cost(family: str, static: dict[str, Any]) -> int:
    constants = static["constants"]
    return int(constants[f"{family}Cost"] if family != "curse" else constants["darkSwordCureCost"])


def _assert_fixture(fixture: dict[str, Any], static: dict[str, Any]) -> None:
    if fixture["caseOrder"] != list(CASE_IDS) or [
        case["caseId"] for case in fixture["cases"]
    ] != list(CASE_IDS):
        raise ValueError("church cure exact ID/order drift")
    expected = (
        (0, 0, 0, (), (0x007F, 0x007F, 0x007F, 0x007F)),
        (1, 2, 10, (-1,), (0x007F, 0x007F, 0x007F, 0x007F)),
        (2, 2, 9, (0,), (0x007F, 0x007F, 0x007F, 0x007F)),
        (3, 2, 10, (0,), (0x007F, 0x007F, 0x007F, 0x007F)),
        (4, 1, 20, (-1,), (0x007F, 0x007F, 0x007F, 0x007F)),
        (5, 1, 19, (0,), (0x007F, 0x007F, 0x007F, 0x007F)),
        (6, 1, 20, (0,), (0x007F, 0x007F, 0x007F, 0x007F)),
        (7, 4, 4250, (-1,), (0x00C6, 0x007F, 0x007F, 0x007F)),
        (8, 4, 4249, (0,), (0x00C6, 0x007F, 0x007F, 0x007F)),
        (9, 4, 4250, (0,), (0x00C6, 0x007F, 0x007F, 0x007F)),
        (10, 7, 4280, (0, 0, 0), (0x00C6, 0x007F, 0x007F, 0x007F)),
    )
    actual = tuple(
        (
            case["member"]["memberId"],
            case["member"]["statusEffects"],
            case["gold"],
            tuple(case["promptResults"]),
            tuple(case["member"]["items"]),
        )
        for case in fixture["cases"]
    )
    if actual != expected:
        raise ValueError("church cure exact input matrix drift")
    if fixture["sourceContext"] != {
        "churchMenuEntryAddress": static["entryAddresses"]["churchMenu"],
        "cureRouteAddress": static["entryAddresses"]["cureRoute"],
    }:
        raise ValueError("church cure source context drift")
    constants = static["constants"]
    if (
        constants["poisonCost"],
        constants["stunCost"],
        constants["darkSwordItem"],
        constants["darkSwordPrice"],
        constants["darkSwordCureCost"],
    ) != (10, 20, 70, 17000, 4250):
        raise ValueError("church cure source-derived cost constants drift")


def expected_observation(fixture: dict[str, Any], static: dict[str, Any]) -> dict[str, Any]:
    records: list[dict[str, Any]] = []
    for case in fixture["cases"]:
        member = case["member"]
        status_before, status, gold = member["statusEffects"], member["statusEffects"], case["gold"]
        items = list(member["items"])
        answers = iter(case["promptResults"])
        chronology, mutations = [], []
        for family in FAMILIES:
            if status & FAMILY_MASKS[family] == 0:
                continue
            answer = next(answers)
            cost = _family_cost(family, static)
            if answer == 0 and gold >= cost:
                gold -= cost
                status &= ~FAMILY_MASKS[family]
                if family == "curse":
                    items = [
                        item & ~(1 << static["constants"]["equippedBit"])
                        if item & 0x7F == static["constants"]["darkSwordItem"]
                        else item
                        for item in items
                    ]
                chronology.append({"family": family, "roles": list(FAMILY_ROLES[family])})
                mutations.append(
                    {
                        "family": family,
                        "cost": cost,
                        "statusAfter": status,
                        "itemSlotsAfter": items.copy(),
                    }
                )
        records.append(
            {
                "caseId": case["caseId"],
                "churchEntryPc": static["entryAddresses"]["churchMenu"],
                "cureRoutePc": static["entryAddresses"]["cureRoute"],
                "goldBefore": case["gold"],
                "goldAfter": gold,
                "memberId": member["memberId"],
                "statusBefore": status_before,
                "statusAfter": status,
                "itemSlotsBefore": member["items"],
                "itemSlotsAfter": items,
                "successChronology": chronology,
                "mutations": mutations,
            }
        )
    return {
        "system": fixture["system"],
        "caseOrder": fixture["caseOrder"],
        "records": records,
        "callbacksCleared": True,
        "restoration": {
            "gold": True,
            "combatantRecords": True,
            "targetsListLength": True,
            "targetsListBytes": True,
            "dialogueScratch": True,
            "currentPortrait": True,
            "generatedRam": True,
            "a6a7Balance": True,
            "sessionCartPatches": True,
        },
    }


def _observer_config(fixture: dict[str, Any], static: dict[str, Any]) -> dict[str, Any]:
    return {
        "caseOrder": fixture["caseOrder"],
        "cases": fixture["cases"],
        "sourceContext": fixture["sourceContext"],
        "static": static,
        "owner": OWNER,
        "observerFailureContract": OBSERVER_FAILURE_CONTRACT,
    }


_FORBIDDEN_OBSERVER_CONFIG_KEYS = frozenset(
    {
        "acceptedobservation",
        "expectedobservation",
        "expectedoutput",
        "golden",
        "observation",
        "outputcorpus",
        "records",
        "mutations",
        "successchronology",
    }
)


def _assert_clean_observer_config(value: Any, location: str = "config") -> None:
    """Reject retained expected-output corpus before launching the observer."""
    if isinstance(value, dict):
        for key, nested in value.items():
            normalized = re.sub(r"[^a-z0-9]", "", key.casefold())
            is_equivalent_golden = ("accepted" in normalized or "expected" in normalized) and any(
                token in normalized
                for token in (
                    "observation",
                    "output",
                    "golden",
                    "record",
                    "mutation",
                    "chronology",
                )
            )
            if normalized in _FORBIDDEN_OBSERVER_CONFIG_KEYS or is_equivalent_golden:
                raise ValueError(
                    f"church cure observer config output corpus drift: {location}.{key}"
                )
            _assert_clean_observer_config(nested, f"{location}.{key}")
    elif isinstance(value, list):
        for index, nested in enumerate(value):
            _assert_clean_observer_config(nested, f"{location}[{index}]")


def _assert_session_readback(session: Path, patches: list[dict[str, Any]]) -> None:
    payload = session.read_bytes()
    for patch in patches:
        start = patch["address"]
        end = start + patch["width"]
        if payload[start:end].hex().upper() != patch["hex"]:
            raise ValueError(f"church cure session patch readback drift: 0x{start:X}")


def _instrument_session_rom(rom_path: Path, static: dict[str, Any], destination: Path) -> None:
    canonical = rom_path.read_bytes()
    payload = bytearray(canonical)
    spans: list[range] = []
    for patch in static["sessionPatches"]:
        data, original = bytes.fromhex(patch["hex"]), bytes.fromhex(patch["originalHex"])
        span = range(patch["address"], patch["address"] + patch["width"])
        if (
            len(data) != patch["width"]
            or len(original) != patch["width"]
            or any(set(span) & set(old) for old in spans)
        ):
            raise ValueError("church cure session patch width/overlap drift")
        if payload[span.start : span.stop] != original:
            raise ValueError(f"church cure canonical patch guard drift: 0x{span.start:X}")
        payload[span.start : span.stop] = data
        spans.append(span)
    destination.write_bytes(payload)
    _assert_session_readback(destination, static["sessionPatches"])
    if rom_path.read_bytes() != canonical:
        raise ValueError("church cure canonical ROM mutation detected")


def assert_lua_role_contract() -> None:
    source = OBSERVER.read_text(encoding="utf-8")
    if source.count("event.on_bus_exec(function()") != 1:
        raise ValueError("church cure requires one physical-PC callback dispatcher")
    for fragment in (
        "for _,event in ipairs(callbacks[address])do dispatch(address,event)end",
        "local ok,msg=pcall(function()",
        "if not ok then failure(msg)end",
        "remove_callbacks()",
        "os.remove(config.outputPath)",
        "callbacks-cleared:0",
        "observer-finished",
        "w16(h.terminalStub,0x2C7C)",
    ):
        if fragment not in source:
            raise ValueError(f"church cure Lua callback contract drift: {fragment}")
    roles = set(re.findall(r'(?<![A-Za-z_])register\([^,]+,"([^"]+)"', source))
    if roles != REQUIRED_LUA_ROLES - {
        "registration",
        "bootstrap-watchdog",
        "transition-watchdog",
        "case-watchdog",
    }:
        raise ValueError(f"church cure Lua role closure drift: {sorted(roles)}")


def _expected_milestones(fixture: dict[str, Any], static: dict[str, Any]) -> list[str]:
    milestones = [
        "milestone:observer-loaded",
        "milestone:direct-function-probe-armed",
        "milestone:direct-function-probe",
    ]
    for case in fixture["cases"]:
        case_id = case["caseId"]
        milestones.extend(
            (
                f"milestone:case-entry:{case_id}",
                "milestone:church-entry",
                "milestone:cure-route",
            )
        )
        gold = case["gold"]
        prompt_index = 0
        for family in FAMILIES:
            if case["member"]["statusEffects"] & FAMILY_MASKS[family] == 0:
                continue
            answer = case["promptResults"][prompt_index]
            prompt_index += 1
            milestones.append(f"milestone:prompt:{family}:{case_id}:{answer}")
            if answer == 0 and gold >= _family_cost(family, static):
                gold -= _family_cost(family, static)
                milestones.append(f"milestone:do-{family}:{case_id}")
    return milestones + ["milestone:callbacks-cleared:0", "milestone:observer-finished"]


def _assert_status(path: Path, fixture: dict[str, Any], static: dict[str, Any]) -> None:
    assert_observer_status(
        path,
        owner=OWNER,
        schema_path=FAILURE_SCHEMA,
        required_milestones=(
            "milestone:observer-loaded",
            "milestone:direct-function-probe-armed",
            "milestone:direct-function-probe",
        ),
    )
    lines = path.read_text(encoding="utf-8").splitlines()
    expected = _expected_milestones(fixture, static)
    if lines != expected:
        raise RuntimeError(
            f"church cure ordered milestone lifecycle drift: expected={expected}; actual={lines}"
        )


def _failure_diagnostic(
    path: Path, static: dict[str, Any], fixture: dict[str, Any]
) -> dict[str, Any] | None:
    value = callback_failure_status(path, owner=OWNER, schema_path=FAILURE_SCHEMA)
    if value is not None:
        pending = value["pendingCallback"]
        if value["caseId"] not in CASE_IDS or pending["expectedCaseId"] != value["caseId"]:
            raise ValueError("church cure callback failure case diagnostics drift")
        if any(
            value[f"expected{kind}Pc"] != pending[f"expected{kind}Pc"]
            for kind in ("Event", "Call", "Target", "Return")
        ):
            raise ValueError("church cure callback failure expectation mirror drift")
        case = next(case for case in fixture["cases"] if case["caseId"] == value["caseId"])
        if pending["memberId"] != case["member"]["memberId"]:
            raise ValueError("church cure callback failure member diagnostics drift")
        expected_event = _failure_event_expectation(value["role"], pending["caseIndex"], static)
        if value["expectedEventPc"] != expected_event:
            raise ValueError("church cure callback failure event-PC diagnostics drift")
        expected_seam = _failure_seam_expectation(value["role"], static)
        if tuple(value[f"expected{kind}Pc"] for kind in ("Call", "Target", "Return")) != (
            expected_seam["call"],
            expected_seam["target"],
            expected_seam["return"],
        ):
            raise ValueError("church cure callback failure seam diagnostics drift")
        if (
            not value["restoration"]["callbacksCleared"]
            or not value["restoration"]["outputRemoved"]
        ):
            raise ValueError("church cure callback failure cleanup drift")
        if value["restoration"]["sessionCartPatches"]:
            raise ValueError("church cure Lua claimed session-cart restoration")
        restoration = value["restoration"]
        scoped_fields = (
            "scopeArmed",
            "gold",
            "combatantRecords",
            "targetsListLength",
            "targetsListBytes",
            "dialogueScratch",
            "currentPortrait",
            "generatedRam",
            "a6a7Balance",
        )
        if not restoration["scopeArmed"] and any(restoration[field] for field in scoped_fields[1:]):
            raise ValueError("church cure unarmed scope claimed successful restoration")
        first_false = next((field for field in scoped_fields if not restoration[field]), None)
        mismatch = value["restorationMismatch"]
        if first_false is None:
            if mismatch is not None:
                raise ValueError(
                    "church cure restoration mismatch must be null after full restoration"
                )
            return value
        if mismatch is None:
            raise ValueError("church cure restoration mismatch missing for first failed scope")
        if mismatch["expected"] == mismatch["actual"]:
            raise ValueError("church cure restoration mismatch has equal expected/actual")
        member_base = (
            static["ram"]["combatantData"]
            + case["member"]["memberId"] * static["ram"]["combatantRecordSize"]
        )
        generated = static["harness"]
        generated_spans = (
            range(
                generated["harnessBase"],
                generated["harnessBase"] + generated["generatedHarnessBytes"],
            ),
            range(
                generated["actionStub"],
                generated["actionStub"] + generated["generatedStubBytes"],
            ),
            range(
                generated["promptStub"],
                generated["promptStub"] + generated["generatedStubBytes"],
            ),
            range(
                generated["terminalStub"],
                generated["terminalStub"] + generated["generatedTerminalBytes"],
            ),
        )
        expected_domain_address = {
            "scopeArmed": ("scope", None),
            "gold": ("gold", static["ram"]["currentGold"]),
            "combatantRecords": ("combatantRecordByte", member_base),
            "targetsListLength": ("targetsListLength", static["ram"]["targetsListLength"]),
            "targetsListBytes": ("targetsListByte", static["ram"]["targetsList"]),
            "currentPortrait": ("currentPortrait", static["ram"]["currentPortrait"]),
        }
        if first_false == "dialogueScratch":
            if mismatch["domain"] not in {"dialogueName", "dialogueNumber"} or mismatch[
                "address"
            ] not in {static["ram"]["dialogueName"], static["ram"]["dialogueNumber"]}:
                raise ValueError("church cure dialogue restoration mismatch drift")
        elif first_false == "generatedRam":
            if mismatch["domain"] != "generatedRamByte" or not any(
                mismatch["address"] in span for span in generated_spans
            ):
                raise ValueError("church cure generated restoration mismatch drift")
        elif first_false == "a6a7Balance":
            if (
                mismatch["domain"] not in {"a6", "a7", "terminalFinalize"}
                or mismatch["address"] is not None
            ):
                raise ValueError("church cure A6/A7 restoration mismatch drift")
        elif (mismatch["domain"], mismatch["address"]) != expected_domain_address[first_false]:
            raise ValueError("church cure restoration mismatch first-false drift")
    return value


def _failure_event_expectation(role: str, case_index: int, static: dict[str, Any]) -> int | None:
    """Return the exact source/harness event PC for an observer role."""
    harness = static["harness"]
    fixed = {
        "bootstrap-check-sram": harness["checkSram"],
        "terminal-finalize": harness["terminalStub"] + 12,
        "church-entry": static["entryAddresses"]["churchMenu"],
        "cure-route": static["entryAddresses"]["cureRoute"],
        "action-stub": harness["actionStub"],
        "prompt-poison": 0x20BBE,
        "prompt-stun": 0x211BA,
        "prompt-curse": 0x20CC0,
        "do-poison": static["entryAddresses"]["poisonDo"],
        "do-stun": static["entryAddresses"]["stunDo"],
        "do-curse": static["entryAddresses"]["curseDo"],
        "j-decrease-gold-entry": static["aliases"]["j_DecreaseGold"]["address"],
        "decrease-gold-entry": static["aliases"]["j_DecreaseGold"]["effectiveTarget"],
        "decrease-gold-return": static["callbackSeams"]["decreaseGold"]["return"],
        "j-set-status-effects-entry": static["aliases"]["j_SetStatusEffects"]["address"],
        "set-status-effects-entry": static["aliases"]["j_SetStatusEffects"]["effectiveTarget"],
        "set-status-effects-return": static["callbackSeams"]["setStatusEffects"]["return"],
        "j-unequip-all-items-if-not-cursed-entry": static["aliases"][
            "j_UnequipAllItemsIfNotCursed"
        ]["address"],
        "unequip-all-items-if-not-cursed-entry": static["aliases"]["j_UnequipAllItemsIfNotCursed"][
            "effectiveTarget"
        ],
        "update-combatant-stats-tail-entry": static["callbackSeams"]["updateStats"]["target"],
        "update-combatant-stats-tail-return": static["callbackSeams"]["updateStats"]["return"],
    }
    if role == "case-entry":
        return harness["harnessBase"] + (case_index - 1) * harness["harnessStride"]
    if role == "case-result":
        return (
            harness["harnessBase"]
            + (case_index - 1) * harness["harnessStride"]
            + harness["resultOffset"]
        )
    return fixed.get(role)


def _failure_seam_expectation(role: str, static: dict[str, Any]) -> dict[str, int | None]:
    empty: dict[str, int | None] = {"call": None, "target": None, "return": None}
    if "decrease" in role:
        return static["callbackSeams"]["decreaseGold"]
    if "set-status" in role:
        return static["callbackSeams"]["setStatusEffects"]
    if "unequip" in role:
        return static["callbackSeams"]["unequip"]
    if "update-combatant" in role:
        return {"call": None, **static["callbackSeams"]["updateStats"]}
    if role == "cure-route":
        return {
            "call": static["callbackSeams"]["cureRoute"]["call"],
            "target": static["callbackSeams"]["cureRoute"]["target"],
            "return": None,
        }
    return empty


def preflight_church_cure_lifecycle(rom_path: Path) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=OWNER)
    assert_lua_role_contract()
    static = build_static_contract(rom_path)
    _assert_fixture(fixture, static)
    with tempfile.TemporaryDirectory(prefix="sf2-church-cure-") as temporary:
        session = Path(temporary) / "church-cure-session.bin"
        _instrument_session_rom(rom_path, static, session)
        if not session.exists():
            raise ValueError("church cure session ROM not created")
    return {
        "Fixture": fixture["system"],
        "Cases": len(CASE_IDS),
        "SessionPatches": len(static["sessionPatches"]),
        "Status": "PRELAUNCH-PASS",
    }


def verify_church_cure_lifecycle(
    rom_path: Path, upstream_path: Path = UPSTREAM, *, timeout_seconds: int = 180
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=OWNER)
    assert_lua_role_contract()
    static = build_static_contract(rom_path, upstream_path)
    _assert_fixture(fixture, static)
    expected = expected_observation(fixture, static)
    validate_json(expected, OBSERVATION_SCHEMA, owner=f"{OWNER} expected observation")
    config = _observer_config(fixture, static)
    _assert_clean_observer_config(config)
    session_deleted = False
    status_path = repo_path(f"local/derived/h3/{OWNER}.status.txt")
    try:
        with tempfile.TemporaryDirectory(prefix="sf2-church-cure-") as temporary:
            session = Path(temporary) / "church-cure-session.bin"
            _instrument_session_rom(rom_path, static, session)
            try:
                observed = _with_instrumented_rom_database(
                    session,
                    "church-cure-lifecycle",
                    lambda: run_observer(
                        observer_path=OBSERVER,
                        rom_path=session,
                        config=config,
                        output_name=OWNER,
                        timeout_seconds=timeout_seconds,
                    ),
                )
            except Exception:
                _failure_diagnostic(status_path, static, fixture)
                raise
            _assert_status(status_path, fixture, static)
        session_deleted = not session.exists()
        observed["restoration"]["sessionCartPatches"] = session_deleted
        validate_json(observed, OBSERVATION_SCHEMA, owner=f"{OWNER} observation")
        if observed != expected:
            raise ValueError(
                f"church cure runtime observation mismatch: expected={expected}; actual={observed}"
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
