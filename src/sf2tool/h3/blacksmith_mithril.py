"""One-launch direct observation of blacksmith picker, commitment, and fulfillment seams.

The work-RAM probe enters only original ROM routines after the ordinary startup
``CheckSram`` return.  It retains the accepted direct ``PickMithrilWeapon``
matrix and adds separate, post-confirmation ``@PlaceOrder`` and fulfillment
matrices.  The commitment cohort redirects only the original ``ClearFlag``
return away from the first text trap.  The fulfillment cohort enters the
original ``@AddItem`` block and redirects only the original
``IsWeaponOrRingEquippable`` return away from its following branch.
"""

from __future__ import annotations

import json
import re
from pathlib import Path
from typing import Any

from sf2tool.h3 import rng
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

FIXTURE = repo_path("tests/fixtures/h3/blacksmith-mithril-v3.json")
FIXTURE_SCHEMA = repo_path("schemas/h3/blacksmith-mithril-fixture.schema.json")
OBSERVATION_SCHEMA = repo_path("schemas/h3/blacksmith-mithril-observation.schema.json")
FAILURE_SCHEMA = repo_path("schemas/h3/blacksmith-mithril-callback-failure.schema.json")
OBSERVER = repo_path("tools/bizhawk/blacksmith_mithril_observer.lua")
TOOLCHAIN_MANIFEST = repo_path("manifests/toolchain.json")
COMMON_MENUS_OWNER = repo_path("tests/fixtures/h2/common-menus-static-v1.json")
COMMON_STATS_OWNER = repo_path("tests/fixtures/h2/common-stats-static-v1.json")
CORE_STATS_DATA_OWNER = repo_path("tests/fixtures/h2/core-stats-data-static-v1.json")
ITEM_OWNER = repo_path("tests/fixtures/h2/item-auxiliary-static-v1.json")
RNG_OWNER = repo_path("tests/fixtures/h3/rng-v1.json")

UPSTREAM = repo_path("local/upstream/SF2DISASM")
DISASM = UPSTREAM / "disasm"
PICK_SOURCE_RELATIVE = Path("code/common/menus/blacksmith/pickmithrilweapon.asm")
BLACKSMITH_ACTIONS_RELATIVE = Path("code/common/menus/blacksmith/blacksmithactions.asm")
TABLE_SOURCE_RELATIVE = Path("data/stats/items/mithrilweapons.asm")
ITEMDEFS_SOURCE_RELATIVE = Path("data/stats/items/itemdefs.asm")
ENUMS_RELATIVE = Path("sf2enums.asm")
CONST_RELATIVE = Path("sf2const.asm")
LISTING_RELATIVE = Path("build/sf2build-h1.lst")
RNG_SOURCE_RELATIVE = Path("code/common/tech/randomnumbergenerator.asm")
GOLD_SOURCE_RELATIVE = Path("code/common/stats/gold.asm")
ITEM_SOURCE_RELATIVE = Path("code/common/stats/itemstats.asm")
FLAG_SOURCE_RELATIVE = Path("code/common/stats/gameflags.asm")
COMBATANT_SOURCE_RELATIVE = Path("code/common/stats/combatantstats_3.asm")

OWNER = "blacksmith-mithril"
STATUS_PREFIX = CALLBACK_FAILURE_PREFIX
OBSERVER_FAILURE_CONTRACT = observer_failure_contract(OWNER)
CASE_IDS = (
    "ordinary-group0-early-slot0",
    "ordinary-group2-final-slot1",
    "brn-fallback-zero-row2-slot2",
    "rdbn-fallback-nonzero-row0-slot3",
    "all-occupied-no-order-write",
)
TRANSACTION_CASE_IDS = (
    "wizard-row3-first-order-slot0",
    "paladin-row1-final-roll-order-slot2",
    "brn-fallback-row2-order-slot1",
)
FULFILLMENT_CASE_IDS = (
    "hero-levanter-slot3-order3-equippable",
    "vicr-goddess-staff-slot2-order2-equippable",
    "snip-mystery-staff-slot0-order0-not-equippable",
)


def _source_section(source: str, symbol: str) -> str:
    start = re.search(rf"^{re.escape(symbol)}:\s*$", source, re.MULTILINE)
    if not start:
        raise ValueError(f"blacksmith source missing section: {symbol}")
    end = source.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"blacksmith source missing section end: {symbol}")
    return source[start.start() : end]


def _source_local_offset(source: str, symbol: str, name: str) -> int:
    """Read the local frame declaration immediately associated with ``symbol``."""
    symbol_match = re.search(rf"^{re.escape(symbol)}:\s*$", source, re.MULTILINE)
    if not symbol_match:
        raise ValueError(f"blacksmith source missing local symbol: {symbol}")
    declarations = source[: symbol_match.start()]
    matches = re.findall(
        rf"^\s*{re.escape(name)}\s*=\s*(-\d+)\s*$", declarations, re.MULTILINE
    )
    if len(matches) != 1:
        raise ValueError(f"blacksmith source local declaration drift: {name}")
    return int(matches[0])


def _source_frame_offsets(
    source: str, symbol: str, names: tuple[str, ...]
) -> dict[str, int]:
    """Read one exact local-frame declaration block immediately before ``symbol``."""
    symbol_match = re.search(rf"^{re.escape(symbol)}:\s*$", source, re.MULTILINE)
    if not symbol_match:
        raise ValueError(f"blacksmith source missing frame symbol: {symbol}")
    block_start = source.rfind("; ===============", 0, symbol_match.start())
    if block_start < 0:
        raise ValueError(f"blacksmith source missing frame declaration block: {symbol}")
    declarations = source[block_start : symbol_match.start()]
    offsets: dict[str, int] = {}
    for name in names:
        matches = re.findall(
            rf"^\s*{re.escape(name)}\s*=\s*(-\d+)\s*$",
            declarations,
            re.MULTILINE,
        )
        if len(matches) != 1:
            raise ValueError(f"blacksmith source frame declaration drift: {name}")
        offsets[name] = int(matches[0])
    return offsets


def _source_tokens(source: str) -> list[str]:
    """Return labels/instructions only; comments and look-alikes cannot count."""
    tokens: list[str] = []
    for raw in source.splitlines():
        line = raw.split(";", 1)[0].strip()
        if not line:
            continue
        if re.fullmatch(r"[@A-Za-z_][@A-Za-z0-9_]*:", line):
            tokens.append(re.sub(r"\s+", " ", line).lower())
            continue
        match = re.fullmatch(r"([A-Za-z][A-Za-z0-9]*(?:\.[A-Za-z]+)?)\s*(.*)", line)
        if not match:
            raise ValueError(f"blacksmith source instruction parse drift: {raw!r}")
        operand = re.sub(r"\s+", " ", match.group(2)).strip()
        tokens.append(f"{match.group(1)} {operand}".strip().lower())
    return tokens


def _require_source_sequence(source: str, sequence: tuple[str, ...], *, name: str) -> None:
    tokens = _source_tokens(source)
    cursor = 0
    for expected in sequence:
        expected = expected.lower()
        try:
            cursor = tokens.index(expected, cursor) + 1
        except ValueError as error:
            raise ValueError(f"blacksmith source guard drift in {name}: {expected!r}") from error


def _parse_equates(source: str) -> dict[str, int]:
    values: dict[str, int] = {}
    for match in re.finditer(
        r"^([A-Z][A-Z0-9_]*):\s+equ\s+(\$[0-9A-F]+|\d+)(?:\s*;.*)?$",
        source,
        re.MULTILINE,
    ):
        raw = match.group(2)
        values[match.group(1)] = int(raw[1:], 16) if raw.startswith("$") else int(raw)
    return values


def _required_equates(values: dict[str, int]) -> dict[str, int]:
    names = (
        "BLACKSMITH_ORDER_COST",
        "MITHRIL_WEAPON_CLASSES_COUNTER",
        "MITHRIL_WEAPONS_PER_CLASS_COUNTER",
        "MITHRIL_WEAPON_ORDER_SLOT_SIZE",
        "BLACKSMITH_ORDERS_COUNTER",
        "BLACKSMITH_MAX_ORDERS_NUMBER",
        "COMBATANT_DATA_ENTRY_REAL_SIZE",
        "COMBATANT_ITEMSLOTS_COUNTER",
        "COMBATANT_ITEMSLOTS",
        "COMBATANT_OFFSET_CLASS",
        "COMBATANT_OFFSET_ITEMS",
        "GAME_FLAGS",
        "FLAG_MASK",
        "CURRENT_GOLD",
        "COMBATANT_DATA",
        "ITEM_NOTHING",
        "ITEMENTRY_MASK_INDEX",
        "ITEMENTRY_MASK_INDEX_AND_BROKEN_BIT",
        "ITEMDEF_SIZE",
        "ITEMDEF_OFFSET_EQUIPFLAGS",
        "ITEMDEF_OFFSET_TYPE",
        "ITEMTYPE_WEAPON",
        "ITEMTYPE_RING",
        "CLASS_HERO",
        "CLASS_VICR",
        "CLASS_SNIP",
        "ITEM_LEVANTER",
        "ITEM_GODDESS_STAFF",
        "ITEM_MYSTERY_STAFF",
        "CLASS_BRN",
        "CLASS_RDBN",
        "ITEM_MITHRIL",
        "MITHRIL_WEAPONS_ON_ORDER",
        "RANDOM_SEED",
    )
    missing = [name for name in names if name not in values]
    if missing:
        raise ValueError(f"blacksmith required equate missing: {missing}")
    return {name: values[name] for name in names}


def _parse_mithril_tables(
    source: str, equates: dict[str, int]
) -> tuple[list[list[int]], list[list[dict[str, int]]], bytes, bytes]:
    """Parse source macros directly instead of importing a golden H2 table payload."""
    groups: list[list[int]] = []
    rows: list[list[dict[str, int]]] = []
    class_bytes = bytearray()
    weapon_bytes = bytearray()
    pending: list[str] = []
    saw_table = False
    for raw in source.splitlines():
        line = raw.split(";", 1)[0].strip()
        if not line:
            continue
        classes = re.fullmatch(r"classes\s+([A-Z0-9_,\s]+)", line)
        if classes:
            names = [token.strip() for token in classes.group(1).split(",")]
            values = []
            for name in names:
                key = f"CLASS_{name}"
                if key not in equates:
                    raise ValueError(f"blacksmith class enum missing: {key}")
                values.append(equates[key])
            if not values:
                raise ValueError("blacksmith class group is empty")
            groups.append(values)
            class_bytes.extend(len(values).to_bytes(2, "big"))
            for value in values:
                class_bytes.extend(value.to_bytes(2, "big"))
            continue
        if line == "table_MithrilWeapons:":
            saw_table = True
            continue
        if not saw_table:
            continue
        if line.startswith("mithrilWeapons "):
            if pending:
                raise ValueError("blacksmith mithril row continuation drift")
            line = line.removeprefix("mithrilWeapons ").strip()
        elif not pending:
            continue
        continuation = line.endswith("&")
        pending.append(line.removesuffix("&").strip().removesuffix(",").strip())
        if continuation:
            continue
        tokens = [token.strip() for token in ",".join(pending).split(",")]
        pending = []
        if len(tokens) != 8:
            raise ValueError(f"blacksmith mithril row width drift: {len(tokens)}")
        choices: list[dict[str, int]] = []
        for offset in range(0, len(tokens), 2):
            denominator = int(tokens[offset], 10)
            item_key = f"ITEM_{tokens[offset + 1]}"
            if item_key not in equates:
                raise ValueError(f"blacksmith item enum missing: {item_key}")
            item_index = equates[item_key]
            if not 1 <= denominator <= 0xFF or not 0 <= item_index <= 0xFF:
                raise ValueError("blacksmith table byte range drift")
            choices.append({"denominator": denominator, "itemIndex": item_index})
            weapon_bytes.extend((denominator, item_index))
        rows.append(choices)
    if pending:
        raise ValueError("blacksmith unterminated mithril row")
    if not groups or not rows:
        raise ValueError("blacksmith mithril table parse produced no rows")
    return groups, rows, bytes(class_bytes), bytes(weapon_bytes)


def _listing_section(listing: str, symbol: str) -> tuple[dict[str, int], list[dict[str, Any]]]:
    start = re.search(rf"^[0-9A-F]{{8}}\s+{re.escape(symbol)}:\s*$", listing, re.MULTILINE)
    if not start:
        raise ValueError(f"blacksmith H1 listing missing symbol: {symbol}")
    end = listing.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"blacksmith H1 listing missing end marker: {symbol}")
    labels: dict[str, int] = {}
    instructions: list[dict[str, Any]] = []
    for raw in listing[start.start() : end].splitlines():
        label = re.fullmatch(r"([0-9A-F]{8})\s+([@A-Za-z_][@A-Za-z0-9_]*):\s*", raw)
        if label:
            labels[label.group(2)] = int(label.group(1), 16)
            continue
        instruction = re.fullmatch(
            r"([0-9A-F]{8})\s+((?:[0-9A-F]{4}\s+)+)(.+?)\s*", raw
        )
        if instruction:
            encoded = re.sub(r"\s+", "", instruction.group(2))
            instructions.append(
                {
                    "address": int(instruction.group(1), 16),
                    "bytes": bytes.fromhex(encoded),
                    "text": re.sub(r"\s+", " ", instruction.group(3).strip()),
                }
            )
    if symbol not in labels:
        raise ValueError(f"blacksmith H1 listing omitted entry label: {symbol}")
    return labels, instructions


def _listing_symbol_section(
    listing: str, symbol: str
) -> tuple[dict[str, int], list[dict[str, Any]]]:
    """Read a listing function whose source does not carry an end comment."""
    start = re.search(rf"^[0-9A-F]{{8}}\s+{re.escape(symbol)}:\s*$", listing, re.MULTILINE)
    if not start:
        raise ValueError(f"blacksmith H1 listing missing symbol: {symbol}")
    tail = listing[start.start() :]
    next_symbol = re.search(r"^[0-9A-F]{8}\s+[A-Za-z_][A-Za-z0-9_]*:\s*$", tail[1:], re.MULTILINE)
    section = tail if not next_symbol else tail[: next_symbol.start() + 1]
    labels: dict[str, int] = {}
    instructions: list[dict[str, Any]] = []
    for raw in section.splitlines():
        label = re.fullmatch(r"([0-9A-F]{8})\s+([@A-Za-z_][@A-Za-z0-9_]*):\s*", raw)
        if label:
            labels[label.group(2)] = int(label.group(1), 16)
            continue
        instruction = re.fullmatch(r"([0-9A-F]{8})\s+((?:[0-9A-F]{4}\s+)+)(.+?)\s*", raw)
        if instruction:
            encoded = re.sub(r"\s+", "", instruction.group(2))
            instructions.append(
                {
                    "address": int(instruction.group(1), 16),
                    "bytes": bytes.fromhex(encoded),
                    "text": re.sub(r"\s+", " ", instruction.group(3).strip()),
                }
            )
    if symbol not in labels:
        raise ValueError(f"blacksmith H1 listing omitted entry label: {symbol}")
    return labels, instructions


def _h1_instruction(
    instructions: list[dict[str, Any]], text: str, *, occurrence: int = 1
) -> tuple[int, int]:
    text = re.sub(r"\s+", " ", text.strip())
    matches = [item for item in instructions if item["text"] == text]
    if len(matches) < occurrence:
        raise ValueError(f"blacksmith H1 instruction missing: {text}")
    item = matches[occurrence - 1]
    return item["address"], item["address"] + len(item["bytes"])


def _h1_text(instruction: dict[str, Any]) -> str:
    return instruction["text"].split(" ;", 1)[0]


def _order_slot_contract_from_source_h1(
    pick_source: str,
    instructions: list[dict[str, Any]],
    labels: dict[str, int],
) -> dict[str, int]:
    """Join the order-slot equate's byte stride to this helper's exact instructions."""
    tokens = _source_tokens(_source_section(pick_source, "PickMithrilWeapon"))
    try:
        next_index = tokens.index("@next:")
    except ValueError as error:
        raise ValueError("blacksmith order-slot source label drift") from error
    stride = re.fullmatch(r"move\.w #(\d+),d0", tokens[next_index + 1])
    source_write = next(
        (
            re.fullmatch(r"move\.([bwl]) d1,\(a0\)", token)
            for token in tokens
            if re.fullmatch(r"move\.[bwl] d1,\(a0\)", token)
        ),
        None,
    )
    if (
        stride is None
        or tokens[next_index + 2] != "adda.w d0,a0"
        or tokens[next_index + 3] != "dbf d7,@loadindex_loop"
        or source_write is None
    ):
        raise ValueError("blacksmith order-slot source stride/write drift")
    source_stride = int(stride.group(1))
    if source_stride < 1:
        raise ValueError("blacksmith order-slot source stride is not positive")
    stride_instruction = next(
        (item for item in instructions if item["address"] == labels["@Next"]), None
    )
    write_instruction = next(
        (item for item in instructions if re.fullmatch(r"move\.[bwl] d1,\(a0\)", _h1_text(item))),
        None,
    )
    write_widths = {"b": 1, "w": 2, "l": 4}
    if (
        stride_instruction is None
        or _h1_text(stride_instruction) != "move.w #2,d0"
        or len(stride_instruction["bytes"]) != 4
        or write_instruction is None
        or re.fullmatch(r"move\.([bwl]) d1,\(a0\)", _h1_text(write_instruction))
        is None
    ):
        raise ValueError("blacksmith order-slot H1 stride/write drift")
    h1_stride = int.from_bytes(stride_instruction["bytes"][-2:], "big")
    h1_write = re.fullmatch(r"move\.([bwl]) d1,\(a0\)", _h1_text(write_instruction))
    if h1_stride != source_stride or h1_write is None or h1_write.group(1) != source_write.group(1):
        raise ValueError("blacksmith order-slot source/H1 stride drift")
    return {
        "strideBytes": source_stride,
        "strideAddress": stride_instruction["address"],
        "writeAddress": write_instruction["address"],
        "writeWidthBytes": write_widths[source_write.group(1)],
    }


def _rom_guard_instruction_bytes(
    instruction: dict[str, Any], labels: dict[str, int], table_addresses: dict[str, int]
) -> bytes:
    """Resolve H1's zero-filled local branch/PC-relative placeholders for the rebuilt ROM."""
    encoded = instruction["bytes"]
    text = instruction["text"].split(" ;", 1)[0]
    branch = re.fullmatch(
        r"(?:b[a-z]+)\.(s|w)\s+([@A-Za-z_][@A-Za-z0-9_]*)", text
    )
    targets = labels | table_addresses
    if branch and branch.group(2) in targets:
        # The 68000 branches from the address immediately after the opcode word,
        # before consuming the optional word displacement.
        displacement = targets[branch.group(2)] - (instruction["address"] + 2)
        if branch.group(1) == "s":
            return encoded[:1] + displacement.to_bytes(1, "big", signed=True)
        return encoded[:2] + displacement.to_bytes(2, "big", signed=True)
    lea = re.fullmatch(r"lea\s+([A-Za-z0-9_]+)\(pc\), a0", text)
    if lea and lea.group(1) in table_addresses:
        target = table_addresses[lea.group(1)]
        displacement = target - (instruction["address"] + 2)
        return encoded[:2] + displacement.to_bytes(2, "big", signed=True)
    return encoded


def _source_function_tokens(source: str, symbol: str) -> list[str]:
    return _source_tokens(_source_section(source, symbol))


def _require_place_order_source_shape(source: str) -> int:
    """Guard just the original post-confirmation mutation block and exit seam."""
    _require_source_sequence(
        _source_section(source, "BlacksmithAction_PlaceOrder"),
        (
            "@placeorder:",
            "move.l #blacksmith_order_cost,d1",
            "jsr j_decreasegold",
            "addi.w #1,pendingordersnumber(a6)",
            "move.w clientmember(a6),d0",
            "move.w itemslot(a6),d1",
            "jsr j_dropitembyslot",
            "bsr.w pickmithrilweapon",
            "jsr j_clearflag",
            "txt 204",
        ),
        name="BlacksmithAction_PlaceOrder post-confirmation block",
    )
    tokens = _source_tokens(_source_section(source, "BlacksmithAction_PlaceOrder"))
    clear_index = tokens.index("jsr j_clearflag")
    immediate = re.fullmatch(r"move\.w #(\d+),d1", tokens[clear_index - 1])
    if immediate is None:
        raise ValueError("blacksmith source guard drift in PlaceOrder readiness-flag immediate")
    return int(immediate.group(1))


def _require_supporting_mutation_source_shape(
    gold_source: str,
    item_source: str,
    flag_source: str,
    combatant_source: str,
) -> None:
    """Bind only helper behavior that the post-confirmation transaction executes."""
    _require_source_sequence(
        _source_section(gold_source, "DecreaseGold"),
        (
            "decreasegold:",
            "move.l ((current_gold-$1000000)).w,d0",
            "sub.l d1,d0",
            "bcc.s @continue",
            "moveq #0,d0",
            "move.l d0,((current_gold-$1000000)).w",
            "move.l d0,d1",
            "rts",
        ),
        name="DecreaseGold",
    )
    _require_source_sequence(
        _source_section(item_source, "DropItemBySlot"),
        (
            "dropitembyslot:",
            "bsr.w getcombatantentryaddress",
            "add.w d1,d1",
            "lea combatant_offset_items(a0,d1.w),a0",
            "andi.w #itementry_mask_index,d1",
            "cmpi.w #item_nothing,d1",
            "bsr.s removeandarrangeitems",
            "bra.w updatecombatantstats",
        ),
        name="DropItemBySlot",
    )
    _require_source_sequence(
        _source_section(flag_source, "ClearFlag"),
        (
            "clearflag:",
            "bsr.w getflag",
            "eori.b #$ff,d0",
            "and.b d0,(a0)",
            "rts",
        ),
        name="ClearFlag",
    )
    _require_source_sequence(
        _source_section(flag_source, "GetFlag"),
        (
            "getflag:",
            "andi.l #flag_mask,d1",
            "divu.w #8,d1",
            "lea ((game_flags-$1000000)).w,a0",
            "adda.w d1,a0",
            "moveq #$ffffff80,d0",
            "lsr.b d1,d0",
            "rts",
        ),
        name="GetFlag",
    )
    _require_source_sequence(
        _source_section(combatant_source, "GetCombatantEntryAddress"),
        (
            "getcombatantentryaddress:",
            "andi.w #byte_mask,d0",
            "lsl.w #3,d0",
            "move.w d0,d1",
            "lsl.w #3,d0",
            "sub.w d1,d0",
            "lea ((combatant_data-$1000000)).w,a0",
            "adda.w d0,a0",
            "rts",
        ),
        name="GetCombatantEntryAddress",
    )


def _require_fulfillment_source_shape(actions_source: str, item_source: str) -> None:
    """Bind only the direct fulfillment block and the helpers it actually reaches."""
    _require_source_sequence(
        _source_section(actions_source, "BlacksmithAction_FulfillOrder"),
        (
            "@additem:",
            "move.w clientmember(a6),d0",
            "move.w itemindex(a6),d1",
            "jsr j_additem",
            "move.w #blacksmith_max_orders_number,d6",
            "sub.w orderscounter(a6),d6",
            "lea ((mithril_weapons_on_order-$1000000)).w,a1",
            "lsl.w #1,d6",
            "adda.w d6,a1",
            "move.w (a1),d2",
            "move.w #0,(a1)",
            "addi.w #1,fulfilledordersnumber(a6)",
            "move.w itemindex(a6),d1",
            "move.w clientmember(a6),d0",
            "jsr j_isweaponorringequippable",
            "bcc.w byte_21cd0",
        ),
        name="BlacksmithAction_FulfillOrder @AddItem block",
    )
    _require_source_sequence(
        _source_section(item_source, "AddItem"),
        (
            "additem:",
            "bsr.w getcombatantentryaddress",
            "lea combatant_offset_items(a0),a0",
            "moveq #combatant_itemslots_counter,d0",
            "@loop:",
            "move.w (a0)+,d2",
            "andi.w #itementry_mask_index,d2",
            "cmpi.w #item_nothing,d2",
            "beq.s @break",
            "dbf d0,@loop",
            "move.w #1,d2",
            "bra.s @done",
            "@break:",
            "andi.w #itementry_mask_index_and_broken_bit,d1",
            "move.w d1,-(a0)",
            "clr.w d2",
            "@done:",
            "movem.l (sp)+,d0/a0",
            "rts",
        ),
        name="AddItem first-ITEM_NOTHING write",
    )
    _require_source_sequence(
        _source_section(item_source, "IsWeaponOrRingEquippable"),
        (
            "isweaponorringequippable:",
            "movem.l d0/d2-d6/a0,-(sp)",
            "move.w #itemtype_weapon|itemtype_ring,d2",
            "bsr.w getcombatantentryaddress",
            "move.b combatant_offset_class(a0),d0",
            "moveq #1,d3",
            "lsl.l d0,d3",
            "bsr.s isitemequippable",
            "movem.l (sp)+,d0/d2-d6/a0",
            "rts",
        ),
        name="IsWeaponOrRingEquippable carry ABI",
    )


def _item_definition_rows(
    source: str, equates: dict[str, int], item_indexes: set[int]
) -> dict[int, dict[str, int]]:
    """Parse the three source rows needed for this cohort, including class masks."""
    rows: dict[int, dict[str, int]] = {}
    pattern = re.compile(
        r"^\s*;\s*(\d+):[^\n]*\n"
        r"\s*equipFlags\s+([A-Z0-9_|]+)\s*\n"
        r"(?:.*\n){2}\s*itemType\s+([A-Z0-9_|]+)\s*$",
        re.MULTILINE,
    )
    for match in pattern.finditer(source):
        item_index = int(match.group(1))
        if item_index not in item_indexes:
            continue
        class_mask = 0
        for name in match.group(2).split("|"):
            if name == "NONE":
                continue
            key = f"CLASS_{name}"
            if key not in equates:
                raise ValueError(f"blacksmith item equip flag enum missing: {key}")
            class_mask |= 1 << equates[key]
        item_type = 0
        for name in match.group(3).split("|"):
            key = f"ITEMTYPE_{name}"
            if key not in equates:
                raise ValueError(f"blacksmith item type enum missing: {key}")
            item_type |= equates[key]
        rows[item_index] = {"equipFlags": class_mask, "itemType": item_type}
    if set(rows) != item_indexes:
        raise ValueError("blacksmith fulfillment item-definition source row drift")
    return rows


def _item_definition_source_domain(source: str) -> tuple[int, int, int]:
    """Derive the complete source cardinality and annotated table span before selecting rows."""
    range_match = re.search(
        r"^;\s*0x([0-9A-F]+)\.\.0x([0-9A-F]+)\s*:\s*Item definitions\s*$",
        source,
        re.MULTILINE,
    )
    if range_match is None:
        raise ValueError("blacksmith item-definition source range drift")
    count = len(re.findall(r"^\s*equipFlags\s+", source, re.MULTILINE))
    if count == 0:
        raise ValueError("blacksmith item-definition source cardinality drift")
    return int(range_match.group(1), 16), int(range_match.group(2), 16), count


def _validate_owners(
    fixture: dict[str, Any],
    *,
    common_menus_path: Path = COMMON_MENUS_OWNER,
    common_stats_path: Path = COMMON_STATS_OWNER,
    core_stats_data_path: Path = CORE_STATS_DATA_OWNER,
    item_owner_path: Path = ITEM_OWNER,
    rng_owner_path: Path = RNG_OWNER,
) -> tuple[dict[str, Any], dict[str, Any], dict[str, Any], dict[str, Any], dict[str, Any]]:
    toolchain = load_json(TOOLCHAIN_MANIFEST)["sf2disasm"]
    common_menus = load_json(common_menus_path)
    common_stats = load_json(common_stats_path)
    core_stats_data = load_json(core_stats_data_path)
    item_owner = load_json(item_owner_path)
    rng_owner = load_json(rng_owner_path)
    owners = fixture["provenance"]["owners"]
    expected_repository = toolchain["repository"].removesuffix(".git")
    for name, owner, expected_path in (
        ("commonMenus", common_menus, COMMON_MENUS_OWNER),
        ("commonStats", common_stats, COMMON_STATS_OWNER),
        ("coreStatsData", core_stats_data, CORE_STATS_DATA_OWNER),
        ("itemAuxiliary", item_owner, ITEM_OWNER),
        ("rng", rng_owner, RNG_OWNER),
    ):
        declared = owners[name]
        if declared["fixture"] != expected_path.relative_to(repo_path(".")).as_posix() or declared[
            "fixtureId"
        ] != owner["id"]:
            raise ValueError(f"blacksmith {name} owner identity drift")
    if (
        fixture["romSha256"] != common_menus["romSha256"]
        or fixture["romSha256"] != common_stats["romSha256"]
        or fixture["romSha256"] != core_stats_data["romSha256"]
        or fixture["romSha256"] != item_owner["romSha256"]
        or fixture["romSha256"] != rng_owner["romSha256"]
        or fixture["provenance"]["upstreamRepository"] != expected_repository
        or fixture["provenance"]["upstreamBranch"] != toolchain["branch"]
        or fixture["provenance"]["upstreamCommit"] != toolchain["commit"]
        or fixture["provenance"]["upstreamCommit"] != common_menus["upstreamCommit"]
        or fixture["provenance"]["upstreamCommit"] != common_stats["upstreamCommit"]
        or fixture["provenance"]["upstreamCommit"] != core_stats_data["upstreamCommit"]
        or fixture["provenance"]["upstreamCommit"] != item_owner["upstreamCommit"]
    ):
        raise ValueError("blacksmith provenance disagrees with pinned/H2/H3 owners")
    return common_menus, common_stats, core_stats_data, item_owner, rng_owner


def _require_pick_source_shape(source: str) -> None:
    section = _source_section(source, "PickMithrilWeapon")
    _require_source_sequence(
        section,
        (
            "pickmithrilweapon:",
            "movem.l d0-a0,-(sp)",
            "clr.w d0",
            "lea list_mithrilweaponclasses(pc), a0",
            "move.w #mithril_weapon_classes_counter,d7",
            "@findweaponclass_loop:",
            "move.w (a0)+,d6",
            "subq.w #1,d6",
            "@findcharacterclass_loop:",
            "move.w (a0)+,d1",
            "move.w clientclass(a6),d2",
            "cmp.w d1,d2",
            "beq.w @getweaponsentryaddress",
            "dbf d6,@findcharacterclass_loop",
            "addi.w #1,d0",
            "dbf d7,@findweaponclass_loop",
            "clr.w d0",
            "move.w #2,d6",
            "jsr (generaterandomnumber).w",
            "cmpi.w #0,d7",
            "bne.w @getweaponsentryaddress",
            "move.w #2,d0",
            "@getweaponsentryaddress:",
            "lsl.w #3,d0",
            "lea table_mithrilweapons(pc), a0",
            "adda.w d0,a0",
            "move.w #mithril_weapons_per_class_counter,d5",
            "@pickweapon_loop:",
            "clr.w d0",
            "clr.w d1",
            "move.b (a0)+,d0",
            "move.b (a0)+,d1",
            "move.w d0,d6",
            "jsr (generaterandomnumber).w",
            "cmpi.w #0,d7",
            "beq.w @loadindex",
            "dbf d5,@pickweapon_loop",
            "@loadindex:",
            "lea ((mithril_weapons_on_order-$1000000)).w,a0",
            "move.w #blacksmith_orders_counter,d7",
            "@loadindex_loop:",
            "cmpi.w #0,(a0)",
            "bne.w @next",
            "move.w d1,(a0)",
            "bra.w @done",
            "@next:",
            "move.w #2,d0",
            "adda.w d0,a0",
            "dbf d7,@loadindex_loop",
            "@done:",
            "movem.l (sp)+,d0-a0",
            "rts",
        ),
        name="PickMithrilWeapon",
    )


def _require_rng_source_shape(source: str) -> None:
    _require_source_sequence(
        _source_section(source, "GenerateRandomNumber"),
        (
            "generaterandomnumber:",
            "move.w (random_seed).l,d7",
            "mulu.w #13,d7",
            "addi.w #7,d7",
            "move.w d7,(random_seed).l",
            "swap d7",
            "lsr.w #1,d7",
            "rts",
        ),
        name="GenerateRandomNumber dependency",
    )


def _rng_abi_from_source(pick_source: str, rng_source: str) -> dict[str, str]:
    """Derive the accepted generator ABI registers from the two source use sites."""
    picker_tokens = _source_tokens(_source_section(pick_source, "PickMithrilWeapon"))
    generator_tokens = _source_tokens(_source_section(rng_source, "GenerateRandomNumber"))
    generator_call = "jsr (generaterandomnumber).w"
    call_indices = [
        index for index, token in enumerate(picker_tokens) if token == generator_call
    ]
    if len(call_indices) != 2:
        raise ValueError("blacksmith RNG caller inventory drift")
    fallback_range = re.fullmatch(r"move\.w #2,(d[0-7])", picker_tokens[call_indices[0] - 1])
    weapon_range = re.fullmatch(r"move\.w d0,(d[0-7])", picker_tokens[call_indices[1] - 1])
    seed_read = next(
        (
            re.fullmatch(r"move\.w \(random_seed\)\.l,(d[0-7])", token)
            for token in generator_tokens
            if re.fullmatch(r"move\.w \(random_seed\)\.l,d[0-7]", token)
        ),
        None,
    )
    seed_write = next(
        (
            re.fullmatch(r"move\.w (d[0-7]),\(random_seed\)\.l", token)
            for token in generator_tokens
            if re.fullmatch(r"move\.w d[0-7],\(random_seed\)\.l", token)
        ),
        None,
    )
    if (
        fallback_range is None
        or weapon_range is None
        or seed_read is None
        or seed_write is None
        or fallback_range.group(1) != weapon_range.group(1)
        or seed_read.group(1) != seed_write.group(1)
    ):
        raise ValueError("blacksmith RNG source ABI drift")
    return {
        "rangeRegister": f"M68K {fallback_range.group(1).upper()}",
        "seedRegister": f"M68K {seed_read.group(1).upper()}",
    }


def build_static_contract(
    fixture: dict[str, Any],
    upstream_path: Path = UPSTREAM,
    *,
    common_menus_path: Path = COMMON_MENUS_OWNER,
    common_stats_path: Path = COMMON_STATS_OWNER,
    core_stats_data_path: Path = CORE_STATS_DATA_OWNER,
    item_owner_path: Path = ITEM_OWNER,
    rng_owner_path: Path = RNG_OWNER,
    pick_source_text: str | None = None,
    table_source_text: str | None = None,
    enums_source_text: str | None = None,
    const_source_text: str | None = None,
    listing_text: str | None = None,
    rng_source_text: str | None = None,
    actions_source_text: str | None = None,
    gold_source_text: str | None = None,
    item_source_text: str | None = None,
    flag_source_text: str | None = None,
    combatant_source_text: str | None = None,
    itemdefs_source_text: str | None = None,
) -> dict[str, Any]:
    """Derive H3 configuration from source, H1, ROM-owner facts, and accepted RNG semantics."""
    common_menus, common_stats, core_stats_data, item_owner, rng_owner = _validate_owners(
        fixture,
        common_menus_path=common_menus_path,
        common_stats_path=common_stats_path,
        core_stats_data_path=core_stats_data_path,
        item_owner_path=item_owner_path,
        rng_owner_path=rng_owner_path,
    )
    upstream_path = upstream_path.resolve(strict=True)
    disasm = upstream_path / "disasm"
    pick_source = pick_source_text or (disasm / PICK_SOURCE_RELATIVE).read_text(encoding="utf-8")
    table_source = table_source_text or (disasm / TABLE_SOURCE_RELATIVE).read_text(encoding="utf-8")
    enums_source = enums_source_text or (disasm / ENUMS_RELATIVE).read_text(encoding="utf-8")
    const_source = const_source_text or (disasm / CONST_RELATIVE).read_text(encoding="utf-8")
    listing = listing_text or (upstream_path / LISTING_RELATIVE).read_text(encoding="utf-8")
    rng_source = rng_source_text or (disasm / RNG_SOURCE_RELATIVE).read_text(encoding="utf-8")
    actions_source = actions_source_text or (
        disasm / BLACKSMITH_ACTIONS_RELATIVE
    ).read_text(encoding="utf-8")
    gold_source = gold_source_text or (disasm / GOLD_SOURCE_RELATIVE).read_text(
        encoding="utf-8"
    )
    item_source = item_source_text or (disasm / ITEM_SOURCE_RELATIVE).read_text(
        encoding="utf-8"
    )
    flag_source = flag_source_text or (disasm / FLAG_SOURCE_RELATIVE).read_text(
        encoding="utf-8"
    )
    combatant_source = combatant_source_text or (
        disasm / COMBATANT_SOURCE_RELATIVE
    ).read_text(encoding="utf-8")
    itemdefs_source = itemdefs_source_text or (
        disasm / ITEMDEFS_SOURCE_RELATIVE
    ).read_text(encoding="utf-8")
    _require_pick_source_shape(pick_source)
    _require_rng_source_shape(rng_source)
    readiness_flag_id = _require_place_order_source_shape(actions_source)
    _require_supporting_mutation_source_shape(
        gold_source, item_source, flag_source, combatant_source
    )
    _require_fulfillment_source_shape(actions_source, item_source)
    client_class_offset = _source_local_offset(pick_source, "PickMithrilWeapon", "clientClass")
    action_frame_offsets = _source_frame_offsets(
        actions_source,
        "BlacksmithAction_PlaceOrder",
        ("clientClass", "clientMember", "itemSlot", "pendingOrdersNumber"),
    )
    fulfillment_frame_offsets = _source_frame_offsets(
        actions_source,
        "BlacksmithAction_FulfillOrder",
        ("clientClass", "clientMember", "itemIndex", "ordersCounter", "fulfilledOrdersNumber"),
    )
    if action_frame_offsets["clientClass"] != client_class_offset:
        raise ValueError("blacksmith action/picker client-class frame offset drift")
    try:
        h2_readiness_flag_id = common_menus["expected"]["menuFacts"][
            "serviceStateMachines"
        ]["blacksmith"]["derived"]["process"]["readiness"]["flagId"]
    except KeyError as error:
        raise ValueError("blacksmith common-menus readiness-flag owner drift") from error
    if readiness_flag_id != h2_readiness_flag_id:
        raise ValueError("blacksmith source/H2 readiness-flag relation drift")
    rng_abi = _rng_abi_from_source(pick_source, rng_source)
    equates = _parse_equates(enums_source) | _parse_equates(const_source)
    required = _required_equates(equates)
    groups, rows, class_bytes, weapon_bytes = _parse_mithril_tables(table_source, equates)
    ordinary_groups = groups[:-1]
    fallback_groups = [
        group
        for group in groups
        if group == [required["CLASS_BRN"], required["CLASS_RDBN"]]
    ]
    if len(ordinary_groups) != required["MITHRIL_WEAPON_CLASSES_COUNTER"] + 1:
        raise ValueError("blacksmith searchable class-group counter relationship drift")
    if len(fallback_groups) != 1 or groups[-1] != fallback_groups[0]:
        raise ValueError("blacksmith exactly-one BRN/RDBN fallback group drift")
    if len(rows) != len(ordinary_groups):
        raise ValueError("blacksmith weapon-row/class-group relationship drift")
    if any(len(row) != required["MITHRIL_WEAPONS_PER_CLASS_COUNTER"] + 1 for row in rows):
        raise ValueError("blacksmith weapon-choice counter relationship drift")
    if [choice["denominator"] for choice in rows[0]] != [16, 8, 4, 1]:
        raise ValueError("blacksmith row-0 denominator source order drift")
    labels, instructions = _listing_section(listing, "PickMithrilWeapon")
    rng_labels, rng_instructions = _listing_symbol_section(listing, "GenerateRandomNumber")
    order_slot = _order_slot_contract_from_source_h1(pick_source, instructions, labels)
    h1_entries = listing_symbol_addresses(listing)
    entry = labels["PickMithrilWeapon"]
    if common_menus["function"]["PickMithrilWeapon"] != entry:
        raise ValueError("blacksmith common-menus H2/H1 entry derivation drift")
    if (
        item_owner["table"]["list_MithrilWeaponClasses"]
        != h1_entries["list_MithrilWeaponClasses"]
        or item_owner["table"]["table_MithrilWeapons"] != h1_entries["table_MithrilWeapons"]
    ):
        raise ValueError("blacksmith item-owner H1 table address drift")
    try:
        common_stats_itemstats_symbol = common_stats["expected"][
            "representativeSymbols"
        ]["itemstats.asm"]
        common_stats_itemstats_address = common_stats["function"]["itemStatsAddress"]
        core_item_definition_address = core_stats_data["table"]["table_ItemDefinitions"]
        core_item_definition_count = core_stats_data["expected"]["facts"]["items"][
            "definitionCount"
        ]
    except KeyError as error:
        raise ValueError("blacksmith common/core-stats owner shape drift") from error
    item_definition_start, item_definition_end, item_definition_count = (
        _item_definition_source_domain(itemdefs_source)
    )
    if (
        common_stats_itemstats_symbol != "GetItemName"
        or common_stats_itemstats_address != h1_entries["GetItemName"]
    ):
        raise ValueError("blacksmith common-stats itemstats H1 owner drift")
    if (
        item_definition_start != h1_entries["table_ItemDefinitions"]
        or core_item_definition_address != h1_entries["table_ItemDefinitions"]
        or item_definition_end - item_definition_start
        != item_definition_count * required["ITEMDEF_SIZE"]
        or core_item_definition_count != item_definition_count
        or required["ITEM_NOTHING"] != item_definition_count - 1
    ):
        raise ValueError("blacksmith core-stats item-definition source/H1 domain drift")
    fulfillment_item_indexes = {
        int(case["itemIndex"]) for case in fixture["fulfillmentCases"]
    }
    if any(
        item_index < 0 or item_index >= item_definition_count
        for item_index in fulfillment_item_indexes
    ):
        raise ValueError("blacksmith fulfillment item-definition source domain drift")
    item_definitions = _item_definition_rows(
        itemdefs_source,
        equates,
        fulfillment_item_indexes,
    )
    summary = item_owner["summary"]
    if (
        summary["mithrilClassGroupCount"] != len(groups)
        or summary["mithrilWeaponRowCount"] != len(rows)
        or summary["mithrilChoiceCount"] != sum(len(row) for row in rows)
    ):
        raise ValueError("blacksmith item-owner table-count drift")
    fallback_call, fallback_return = _h1_instruction(
        instructions, "jsr (GenerateRandomNumber).w", occurrence=1
    )
    weapon_call, weapon_return = _h1_instruction(
        instructions, "jsr (GenerateRandomNumber).w", occurrence=2
    )
    function_return, _ = _h1_instruction(instructions, "rts")
    rng_return, _ = _h1_instruction(rng_instructions, "rts")
    client_class_reads = [
        instruction
        for instruction in instructions
        if _h1_text(instruction) == "move.w clientClass(a6),d2"
    ]
    if len(client_class_reads) != 1 or len(client_class_reads[0]["bytes"]) != 4:
        raise ValueError("blacksmith client-class H1 read drift")
    client_class_read = client_class_reads[0]
    h1_client_class_offset = int.from_bytes(client_class_read["bytes"][-2:], "big", signed=True)
    if h1_client_class_offset != client_class_offset:
        raise ValueError("blacksmith client-class source/H1 offset drift")
    if (
        required["BLACKSMITH_MAX_ORDERS_NUMBER"]
        != required["BLACKSMITH_ORDERS_COUNTER"] + 1
        or required["MITHRIL_WEAPON_ORDER_SLOT_SIZE"] != order_slot["strideBytes"]
        or order_slot["strideBytes"] != order_slot["writeWidthBytes"]
    ):
        raise ValueError("blacksmith order-slot count/stride/write-width relation drift")
    if any(
        len(case["ordersBefore"]) != required["BLACKSMITH_MAX_ORDERS_NUMBER"]
        for case in fixture["cases"]
    ):
        raise ValueError("blacksmith fixture order-slot domain drift")
    if rng_labels["GenerateRandomNumber"] != h1_entries["GenerateRandomNumber"]:
        raise ValueError("blacksmith RNG H1 entry derivation drift")
    rng_function = rng_owner["function"]
    if (
        rng_function["entryAddress"] != h1_entries["GenerateRandomNumber"]
        or rng_function["observeAddress"] != rng_return
        or rng_function["seedAddress"] != required["RANDOM_SEED"]
        or rng_function["rangeRegister"] != rng_abi["rangeRegister"]
        or rng_function["seedRegister"] != rng_abi["seedRegister"]
    ):
        raise ValueError("blacksmith RNG owner/source/H1 ABI join drift")
    source_context = fixture["sourceContext"]
    if (
        source_context["pickSourcePath"] != PICK_SOURCE_RELATIVE.as_posix()
        or source_context["placeSourcePath"] != BLACKSMITH_ACTIONS_RELATIVE.as_posix()
        or source_context["itemStatsSourcePath"] != ITEM_SOURCE_RELATIVE.as_posix()
        or source_context["tableSourcePath"] != TABLE_SOURCE_RELATIVE.as_posix()
        or source_context["itemDefinitionsSourcePath"]
        != ITEMDEFS_SOURCE_RELATIVE.as_posix()
        or source_context["h1ListingPath"] != LISTING_RELATIVE.as_posix()
        or source_context["functionEntryAddress"] != entry
    ):
        raise ValueError("blacksmith fixture source-context identity drift")

    place_labels, place_instructions = _listing_section(
        listing, "BlacksmithAction_PlaceOrder"
    )
    if source_context["placeEntryAddress"] != place_labels["@PlaceOrder"]:
        raise ValueError("blacksmith fixture place-order source-context drift")
    fulfill_labels, fulfill_instructions = _listing_section(
        listing, "BlacksmithAction_FulfillOrder"
    )
    if source_context["fulfillAddItemEntryAddress"] != fulfill_labels["@AddItem"]:
        raise ValueError("blacksmith fixture fulfill-order source-context drift")
    add_labels, add_instructions = _listing_section(listing, "AddItem")
    equippable_labels, equippable_instructions = _listing_section(
        listing, "IsWeaponOrRingEquippable"
    )
    decrease_labels, decrease_instructions = _listing_section(listing, "DecreaseGold")
    drop_labels, drop_instructions = _listing_section(listing, "DropItemBySlot")
    clear_labels, clear_instructions = _listing_section(listing, "ClearFlag")
    get_flag_labels, get_flag_instructions = _listing_section(listing, "GetFlag")
    combatant_labels, combatant_instructions = _listing_section(
        listing, "GetCombatantEntryAddress"
    )
    update_labels, update_instructions = _listing_section(
        listing, "UpdateCombatantStats"
    )

    def instruction_record(
        instructions: list[dict[str, Any]], text: str, *, occurrence: int = 1
    ) -> dict[str, Any]:
        normalized = re.sub(r"\s+", " ", text.strip())
        matches = [item for item in instructions if _h1_text(item) == normalized]
        if len(matches) < occurrence:
            raise ValueError(f"blacksmith transaction H1 instruction missing: {text}")
        return matches[occurrence - 1]

    def successor_record(
        instructions: list[dict[str, Any]], record: dict[str, Any]
    ) -> dict[str, Any]:
        index = next(index for index, item in enumerate(instructions) if item is record)
        try:
            return instructions[index + 1]
        except IndexError as error:
            raise ValueError("blacksmith transaction H1 successor drift") from error

    decrease_call_record = instruction_record(place_instructions, "jsr j_DecreaseGold")
    pending_increment_record = successor_record(place_instructions, decrease_call_record)
    pending_increment_after_record = successor_record(
        place_instructions, pending_increment_record
    )
    client_member_record = instruction_record(
        place_instructions, "move.w clientMember(a6),d0"
    )
    item_slot_record = instruction_record(place_instructions, "move.w itemSlot(a6),d1")
    drop_call_record = instruction_record(place_instructions, "jsr j_DropItemBySlot")
    pick_call_record = instruction_record(place_instructions, "bsr.w PickMithrilWeapon")
    clear_call_record = instruction_record(place_instructions, "jsr j_ClearFlag")
    clear_flag_load_record = place_instructions[place_instructions.index(clear_call_record) - 1]
    pre_presentation_record = successor_record(place_instructions, clear_call_record)
    def h1_frame_displacement(record: dict[str, Any], name: str) -> int:
        encoded = record["bytes"]
        if len(encoded) < 4:
            raise ValueError(f"blacksmith transaction H1 frame displacement width drift: {name}")
        return int.from_bytes(encoded[-2:], "big", signed=True)

    h1_frame_offsets = {
        "clientMember": h1_frame_displacement(client_member_record, "clientMember"),
        "itemSlot": h1_frame_displacement(item_slot_record, "itemSlot"),
        "pendingOrdersNumber": h1_frame_displacement(
            pending_increment_record, "pendingOrdersNumber"
        ),
    }
    if (
        pending_increment_record["text"] != "addi.w #1,pendingOrdersNumber(a6)"
        or pending_increment_after_record["address"]
        != pending_increment_record["address"] + len(pending_increment_record["bytes"])
        or clear_flag_load_record["text"] != f"move.w #{readiness_flag_id},d1"
        or len(clear_flag_load_record["bytes"]) != 4
        or int.from_bytes(clear_flag_load_record["bytes"][-2:], "big")
        != readiness_flag_id
        or h1_frame_offsets
        != {
            name: action_frame_offsets[name]
            for name in ("clientMember", "itemSlot", "pendingOrdersNumber")
        }
        or pre_presentation_record["text"] not in {"trap #textbox", "M trap #textbox"}
        or place_labels.get("@PlaceOrder") != decrease_call_record["address"] - 6
    ):
        raise ValueError("blacksmith transaction H1 frame/immediate chronology drift")

    decrease_return_rts, _ = _h1_instruction(decrease_instructions, "rts")
    instruction_record(drop_instructions, "bra.w UpdateCombatantStats")
    update_return_rts, _ = _h1_instruction(update_instructions, "rts")
    clear_return_rts, _ = _h1_instruction(clear_instructions, "rts")
    flag_mask_record = instruction_record(get_flag_instructions, "andi.l #FLAG_MASK,d1")
    flag_base_record = instruction_record(
        get_flag_instructions, "lea ((GAME_FLAGS-$1000000)).w,a0"
    )
    combatant_stride_first = instruction_record(combatant_instructions, "lsl.w #3,d0")
    combatant_stride_second = instruction_record(
        combatant_instructions, "lsl.w #3,d0", occurrence=2
    )
    combatant_stride_subtract = instruction_record(combatant_instructions, "sub.w d1,d0")
    combatant_base_record = instruction_record(
        combatant_instructions, "lea ((COMBATANT_DATA-$1000000)).w,a0"
    )
    if (
        decrease_labels["DecreaseGold"] != h1_entries["DecreaseGold"]
        or drop_labels["DropItemBySlot"] != h1_entries["DropItemBySlot"]
        or clear_labels["ClearFlag"] != h1_entries["ClearFlag"]
        or get_flag_labels["GetFlag"] != h1_entries["GetFlag"]
        or combatant_labels["GetCombatantEntryAddress"]
        != h1_entries["GetCombatantEntryAddress"]
        or update_labels["UpdateCombatantStats"] != h1_entries["UpdateCombatantStats"]
        or flag_mask_record["address"] >= flag_base_record["address"]
        or combatant_stride_first["address"] >= combatant_stride_second["address"]
        or combatant_stride_second["address"] >= combatant_stride_subtract["address"]
        or combatant_stride_subtract["address"] >= combatant_base_record["address"]
    ):
        raise ValueError("blacksmith transaction H1 supporting-helper relation drift")
    flag_id = readiness_flag_id
    flag_byte_offset = flag_id // 8
    flag_bit_mask = 0x80 >> (flag_id % 8)
    if (
        required["FLAG_MASK"] < flag_id
        or required["COMBATANT_DATA_ENTRY_REAL_SIZE"] != 56
        or required["COMBATANT_ITEMSLOTS"] != 4
        or required["COMBATANT_OFFSET_ITEMS"] != 32
        or required["ITEMENTRY_MASK_INDEX"] != required["ITEM_NOTHING"]
        or required["BLACKSMITH_ORDER_COST"] <= 0
        or required["ITEM_MITHRIL"] == required["ITEM_NOTHING"]
    ):
        raise ValueError("blacksmith transaction work-RAM constant relation drift")

    fulfillment_start = next(
        index
        for index, instruction in enumerate(fulfill_instructions)
        if instruction["address"] == fulfill_labels["@AddItem"]
    )
    fulfillment_instructions = fulfill_instructions[fulfillment_start:]

    def fulfillment_instruction(text: str, *, occurrence: int = 1) -> dict[str, Any]:
        matches = [
            item for item in fulfillment_instructions if _h1_text(item) == text
        ]
        if len(matches) < occurrence:
            raise ValueError(f"blacksmith fulfillment H1 instruction missing: {text}")
        return matches[occurrence - 1]

    fulfillment_client_member = fulfillment_instruction("move.w clientMember(a6),d0")
    fulfillment_item_index = fulfillment_instruction("move.w itemIndex(a6),d1")
    add_call_record = fulfillment_instruction("jsr j_AddItem")
    orders_counter_record = fulfillment_instruction("sub.w ordersCounter(a6),d6")
    order_base_record = fulfillment_instruction(
        "lea ((MITHRIL_WEAPONS_ON_ORDER-$1000000)).w,a1"
    )
    order_read_record = fulfillment_instruction("move.w (a1),d2")
    order_clear_record = fulfillment_instruction("move.w #0,(a1)")
    fulfilled_increment_record = fulfillment_instruction(
        "addi.w #1,fulfilledOrdersNumber(a6)"
    )
    fulfilled_increment_after = successor_record(
        fulfill_instructions, fulfilled_increment_record
    )
    equippable_call_record = fulfillment_instruction(
        "jsr j_IsWeaponOrRingEquippable", occurrence=1
    )
    post_equippable_record = successor_record(
        fulfill_instructions, equippable_call_record
    )
    fulfillment_block_instructions = [
        instruction
        for instruction in fulfill_instructions
        if fulfill_labels["@AddItem"]
        <= instruction["address"]
        <= post_equippable_record["address"]
    ]
    add_return_rts, _ = _h1_instruction(add_instructions, "rts")
    equippable_return_rts, _ = _h1_instruction(equippable_instructions, "rts")
    equippable_class_read = instruction_record(
        equippable_instructions, "move.b COMBATANT_OFFSET_CLASS(a0),d0"
    )
    add_empty_compare = instruction_record(add_instructions, "cmpi.w #ITEM_NOTHING,d2")
    add_empty_break = instruction_record(add_instructions, "beq.s @Break")
    add_full = instruction_record(add_instructions, "move.w #1,d2")
    add_full_done = instruction_record(add_instructions, "bra.s @Done")
    add_mask_write = instruction_record(
        add_instructions, "andi.w #ITEMENTRY_MASK_INDEX_AND_BROKEN_BIT,d1"
    )
    add_write = instruction_record(add_instructions, "move.w d1,-(a0)")
    add_success = instruction_record(add_instructions, "clr.w d2")
    add_done_restore = instruction_record(add_instructions, "movem.l (sp)+,d0/a0")
    fulfillment_h1_offsets = {
        "clientMember": h1_frame_displacement(
            fulfillment_client_member, "fulfillment clientMember"
        ),
        "itemIndex": h1_frame_displacement(
            fulfillment_item_index, "fulfillment itemIndex"
        ),
        "ordersCounter": h1_frame_displacement(
            orders_counter_record, "fulfillment ordersCounter"
        ),
        "fulfilledOrdersNumber": h1_frame_displacement(
            fulfilled_increment_record, "fulfillment fulfilledOrdersNumber"
        ),
    }
    equippable_class_bytes = equippable_class_read["bytes"]
    if len(equippable_class_bytes) != 4 or equippable_class_bytes[:2] != b"\x10\x28":
        raise ValueError("blacksmith fulfillment class displacement opcode/width drift")
    h1_combatant_class_offset_signed = int.from_bytes(
        equippable_class_bytes[2:], "big", signed=True
    )
    h1_combatant_class_offset_unsigned = int.from_bytes(
        equippable_class_bytes[2:], "big", signed=False
    )
    if (
        fulfill_labels["@AddItem"] != add_call_record["address"] - 8
        or fulfillment_h1_offsets
        != {
            name: fulfillment_frame_offsets[name]
            for name in fulfillment_h1_offsets
        }
        or order_base_record["address"]
        != orders_counter_record["address"] + len(orders_counter_record["bytes"])
        or order_read_record["address"]
        != order_clear_record["address"] - len(order_read_record["bytes"])
        or fulfilled_increment_after["address"]
        != fulfilled_increment_record["address"] + len(fulfilled_increment_record["bytes"])
        or post_equippable_record["text"] not in {
            "bcc.w byte_21CD0 ; @DoNotEquipNewItem",
            "bcc.w byte_21CD0",
        }
        or int.from_bytes(add_call_record["bytes"][-4:], "big")
        != h1_entries["j_AddItem"]
        or int.from_bytes(equippable_call_record["bytes"][-4:], "big")
        != h1_entries["j_IsWeaponOrRingEquippable"]
        or h1_combatant_class_offset_signed != h1_combatant_class_offset_unsigned
        or h1_combatant_class_offset_signed != required["COMBATANT_OFFSET_CLASS"]
        or add_empty_compare["address"] >= add_empty_break["address"]
        or add_empty_break["address"] >= add_full["address"]
        or add_full["address"] >= add_full_done["address"]
        or add_full_done["address"] >= add_mask_write["address"]
        or add_labels["@Break"] != add_mask_write["address"]
        or add_mask_write["address"] >= add_write["address"]
        or add_write["address"] >= add_success["address"]
        or add_success["address"] >= add_done_restore["address"]
        or add_labels["@Done"] != add_done_restore["address"]
        or add_done_restore["address"] >= add_return_rts
        or len(add_empty_break["bytes"]) != 2
        or add_empty_break["bytes"][0] != 0x67
        or len(add_full_done["bytes"]) != 2
        or add_full_done["bytes"][0] != 0x60
        or required["COMBATANT_ITEMSLOTS_COUNTER"] + 1
        != required["COMBATANT_ITEMSLOTS"]
        or (required["ITEMTYPE_WEAPON"] | required["ITEMTYPE_RING"]) == 0
    ):
        raise ValueError("blacksmith fulfillment source/H1 ABI chronology drift")
    item_definition_address = h1_entries["table_ItemDefinitions"]
    fulfillment_item_definitions: dict[int, dict[str, int]] = {}
    for item_index, definition in item_definitions.items():
        address = item_definition_address + item_index * required["ITEMDEF_SIZE"]
        fulfillment_item_definitions[item_index] = {
            **definition,
            "address": address,
        }
    for case in fixture["fulfillmentCases"]:
        item_index = int(case["itemIndex"])
        definition = fulfillment_item_definitions[item_index]
        class_mask = 1 << int(case["recipientClass"])
        expected_carry = bool(
            definition["itemType"]
            & (required["ITEMTYPE_WEAPON"] | required["ITEMTYPE_RING"])
            and definition["equipFlags"] & class_mask
        )
        if expected_carry != bool(case["equippableCarrySet"]):
            raise ValueError("blacksmith fulfillment item/class carry relation drift")

    def guarded_instructions(
        instructions: list[dict[str, Any]], labels: dict[str, int]
    ) -> list[dict[str, Any]]:
        return [
            {
                **instruction,
                "romBytes": _rom_guard_instruction_bytes(
                    instruction,
                    labels,
                    h1_entries,
                ),
            }
            for instruction in instructions
        ]

    return {
        "function": {
            "entryAddress": entry,
            "returnRtsAddress": function_return,
            "classSearchLoopAddress": labels["@FindWeaponClass_Loop"],
            "rowResolvedAddress": labels["@GetWeaponsEntryAddress"],
            "rowLoopAddress": labels["@PickWeapon_Loop"],
            "loadIndexAddress": labels["@LoadIndex"],
            "orderLoopAddress": labels["@LoadIndex_Loop"],
            "orderNextAddress": labels["@Next"],
            "orderWriteAddress": order_slot["writeAddress"],
            "orderStrideAddress": order_slot["strideAddress"],
            "clientClassReadAddress": client_class_read["address"],
            "fallbackRngCallAddress": fallback_call,
            "fallbackRngReturnAddress": fallback_return,
            "weaponRngCallAddress": weapon_call,
            "weaponRngReturnAddress": weapon_return,
            "rngEntryAddress": h1_entries["GenerateRandomNumber"],
            "rngReturnRtsAddress": rng_return,
            "checkSramAddress": h1_entries["CheckSram"],
        },
        "ram": {
            "randomSeedAddress": required["RANDOM_SEED"],
            "ordersAddress": required["MITHRIL_WEAPONS_ON_ORDER"],
            "currentGoldAddress": required["CURRENT_GOLD"],
            "gameFlagsAddress": required["GAME_FLAGS"],
            "flag80OwningByteAddress": required["GAME_FLAGS"] + flag_byte_offset,
            "combatantDataAddress": required["COMBATANT_DATA"],
        },
        "constants": {
            "classGroupsCounter": required["MITHRIL_WEAPON_CLASSES_COUNTER"],
            "weaponRowsCounter": required["MITHRIL_WEAPONS_PER_CLASS_COUNTER"],
            "weaponRowCount": len(rows),
            "orderSlotsCounter": required["BLACKSMITH_ORDERS_COUNTER"],
            "orderSlotCount": required["BLACKSMITH_MAX_ORDERS_NUMBER"],
            "orderSlotSize": required["MITHRIL_WEAPON_ORDER_SLOT_SIZE"],
            "clientClassOffset": client_class_offset,
            "brnClass": required["CLASS_BRN"],
            "rdbnClass": required["CLASS_RDBN"],
            "orderCost": required["BLACKSMITH_ORDER_COST"],
            "mithrilItemIndex": required["ITEM_MITHRIL"],
            "itemNothingIndex": required["ITEM_NOTHING"],
            "itemIndexMask": required["ITEMENTRY_MASK_INDEX"],
            "itemIndexAndBrokenMask": required["ITEMENTRY_MASK_INDEX_AND_BROKEN_BIT"],
            "weaponTypeMask": required["ITEMTYPE_WEAPON"],
            "ringTypeMask": required["ITEMTYPE_RING"],
            "combatantEntrySizeBytes": required["COMBATANT_DATA_ENTRY_REAL_SIZE"],
            "combatantItemSlotCount": required["COMBATANT_ITEMSLOTS"],
            "combatantClassOffsetBytes": required["COMBATANT_OFFSET_CLASS"],
            "combatantItemsOffsetBytes": required["COMBATANT_OFFSET_ITEMS"],
            "flag80Id": flag_id,
            "flag80ByteOffset": flag_byte_offset,
            "flag80BitMask": flag_bit_mask,
        },
        "transaction": {
            "placeEntryAddress": place_labels["@PlaceOrder"],
            "decreaseGoldCallAddress": decrease_call_record["address"],
            "decreaseGoldInstructionTargetAddress": h1_entries["j_DecreaseGold"],
            "decreaseGoldEffectiveTargetAddress": h1_entries["DecreaseGold"],
            "decreaseGoldEffectiveReturnAddress": decrease_return_rts,
            "pendingOrdersIncrementAddress": pending_increment_record["address"],
            "pendingOrdersIncrementedObserveAddress": pending_increment_after_record[
                "address"
            ],
            "dropItemCallAddress": drop_call_record["address"],
            "dropItemInstructionTargetAddress": h1_entries["j_DropItemBySlot"],
            "dropItemEffectiveTargetAddress": h1_entries["DropItemBySlot"],
            "dropItemTailUpdateTargetAddress": h1_entries["UpdateCombatantStats"],
            "dropItemEffectiveReturnAddress": update_return_rts,
            "pickMithrilCallAddress": pick_call_record["address"],
            "pickMithrilReturnAddress": pick_call_record["address"]
            + len(pick_call_record["bytes"]),
            "clearFlagCallAddress": clear_call_record["address"],
            "clearFlagInstructionTargetAddress": h1_entries["j_ClearFlag"],
            "clearFlagEffectiveTargetAddress": h1_entries["ClearFlag"],
            "clearFlagEffectiveReturnAddress": clear_return_rts,
            "prePresentationReturnAddress": pre_presentation_record["address"],
            "frameOffsetsBytes": {
                "clientClass": action_frame_offsets["clientClass"],
                "clientMember": action_frame_offsets["clientMember"],
                "itemSlot": action_frame_offsets["itemSlot"],
                "pendingOrdersNumber": action_frame_offsets["pendingOrdersNumber"],
            },
            "h1InstructionBytes": [
                *guarded_instructions(place_instructions, place_labels),
                *guarded_instructions(decrease_instructions, decrease_labels),
                *guarded_instructions(drop_instructions, drop_labels),
                *guarded_instructions(clear_instructions, clear_labels),
                *guarded_instructions(get_flag_instructions, get_flag_labels),
                *guarded_instructions(combatant_instructions, combatant_labels),
                *guarded_instructions(update_instructions, update_labels),
            ],
        },
        "fulfillment": {
            "addItemEntryAddress": fulfill_labels["@AddItem"],
            "addItemCallAddress": add_call_record["address"],
            "addItemReturnAddress": add_call_record["address"]
            + len(add_call_record["bytes"]),
            "addItemInstructionTargetAddress": h1_entries["j_AddItem"],
            "addItemEffectiveTargetAddress": add_labels["AddItem"],
            "addItemEffectiveReturnAddress": add_return_rts,
            "orderReadInstructionAddress": order_read_record["address"],
            "orderReadObserveAddress": order_clear_record["address"],
            "orderClearAddress": order_clear_record["address"],
            "orderClearedObserveAddress": fulfilled_increment_record["address"],
            "fulfilledOrdersIncrementAddress": fulfilled_increment_record["address"],
            "fulfilledOrdersIncrementedObserveAddress": fulfilled_increment_after[
                "address"
            ],
            "equippabilityCallAddress": equippable_call_record["address"],
            "equippabilityInstructionTargetAddress": h1_entries[
                "j_IsWeaponOrRingEquippable"
            ],
            "equippabilityEffectiveTargetAddress": equippable_labels[
                "IsWeaponOrRingEquippable"
            ],
            "equippabilityEffectiveReturnAddress": equippable_return_rts,
            "postEquippabilityReturnAddress": post_equippable_record["address"],
            "updateCombatantStatsAddress": update_labels["UpdateCombatantStats"],
            "updateCombatantStatsReached": False,
            "frameOffsetsBytes": fulfillment_frame_offsets,
            "ordersCounterMinimum": 1,
            "ordersCounterMaximum": required["BLACKSMITH_MAX_ORDERS_NUMBER"],
            "h1InstructionBytes": [
                *guarded_instructions(fulfillment_block_instructions, fulfill_labels),
                *guarded_instructions(add_instructions, add_labels),
                *guarded_instructions(equippable_instructions, equippable_labels),
                *guarded_instructions(combatant_instructions, combatant_labels),
                *guarded_instructions(update_instructions, update_labels),
            ],
            "itemDefinitionFields": [
                {
                    "itemIndex": item_index,
                    "equipFlagsAddress": definition["address"]
                    + required["ITEMDEF_OFFSET_EQUIPFLAGS"],
                    "equipFlagsBytes": definition["equipFlags"].to_bytes(4, "big"),
                    "itemTypeAddress": definition["address"]
                    + required["ITEMDEF_OFFSET_TYPE"],
                    "itemTypeBytes": bytes([definition["itemType"]]),
                }
                for item_index, definition in sorted(fulfillment_item_definitions.items())
            ],
        },
        "model": {"classGroups": groups, "weaponRows": rows},
        "h1": {
            "instructionBytes": [
                {
                    **instruction,
                    "romBytes": _rom_guard_instruction_bytes(
                        instruction,
                        labels,
                        {
                            "list_MithrilWeaponClasses": h1_entries[
                                "list_MithrilWeaponClasses"
                            ],
                            "table_MithrilWeapons": h1_entries["table_MithrilWeapons"],
                        },
                    ),
                }
                for instruction in instructions
            ],
            "classTableAddress": h1_entries["list_MithrilWeaponClasses"],
            "weaponTableAddress": h1_entries["table_MithrilWeapons"],
            "classTableBytes": class_bytes,
            "weaponTableBytes": weapon_bytes,
        },
    }


def validate_static_contract(
    fixture: dict[str, Any], rom_path: Path, upstream_path: Path = UPSTREAM
) -> dict[str, Any]:
    """Reject H1/source/table/ROM drift before writing a Lua configuration file."""
    static = build_static_contract(fixture, upstream_path)
    rom = rom_path.resolve(strict=True).read_bytes()
    for instruction in (
        static["h1"]["instructionBytes"]
        + static["transaction"]["h1InstructionBytes"]
        + static["fulfillment"]["h1InstructionBytes"]
    ):
        address = instruction["address"]
        expected = instruction["romBytes"]
        if rom[address : address + len(expected)] != expected:
            raise ValueError(f"blacksmith H1/ROM instruction guard drift at {address:#x}")
    for name, address, expected in (
        ("class", static["h1"]["classTableAddress"], static["h1"]["classTableBytes"]),
        ("weapon", static["h1"]["weaponTableAddress"], static["h1"]["weaponTableBytes"]),
    ):
        if rom[address : address + len(expected)] != expected:
            raise ValueError(f"blacksmith H1/ROM {name} table guard drift")
    for field in static["fulfillment"]["itemDefinitionFields"]:
        for address_key, bytes_key in (
            ("equipFlagsAddress", "equipFlagsBytes"),
            ("itemTypeAddress", "itemTypeBytes"),
        ):
            address = field[address_key]
            expected = field[bytes_key]
            if rom[address : address + len(expected)] != expected:
                raise ValueError(
                    "blacksmith fulfillment H1/ROM item-definition guard drift "
                    f"at {address:#x}"
                )
    return static


def _rng_roll(seed: int, range_word: int) -> tuple[int, int]:
    """Use the accepted original-generator semantics, not a fixture result value."""
    return rng._rng_step(seed, range_word)


def model_case(case: dict[str, Any], static: dict[str, Any]) -> dict[str, Any]:
    """Independent picker model from parsed class/weapon tables and accepted RNG semantics."""
    groups = static["model"]["classGroups"]
    rows = static["model"]["weaponRows"]
    constants = static["constants"]
    class_value = case["clientClass"]
    seed = case["randomSeedBefore"]
    class_group_index = next(
        (index for index, group in enumerate(groups[:-1]) if class_value in group), None
    )
    rng_calls: list[dict[str, int | str]] = []
    if class_group_index is None:
        if class_value not in groups[-1]:
            raise ValueError("blacksmith matrix case is outside source-owned class groups")
        seed, value = _rng_roll(seed, 2)
        rng_calls.append(
            {
                "role": "fallback-row-roll",
                "callPc": static["function"]["fallbackRngCallAddress"],
                "targetPc": static["function"]["rngEntryAddress"],
                "returnPc": static["function"]["fallbackRngReturnAddress"],
                "rangeWord": 2,
                "result": value,
                "randomSeedAfter": seed,
            }
        )
        row_index = 2 if value == 0 else 0
        class_group_index = len(groups) - 1
    else:
        row_index = class_group_index
    selected_choice_index: int | None = None
    item_index: int | None = None
    for choice_index, choice in enumerate(rows[row_index]):
        seed, value = _rng_roll(seed, choice["denominator"])
        rng_calls.append(
            {
                "role": "weapon-row-roll",
                "callPc": static["function"]["weaponRngCallAddress"],
                "targetPc": static["function"]["rngEntryAddress"],
                "returnPc": static["function"]["weaponRngReturnAddress"],
                "rangeWord": choice["denominator"],
                "result": value,
                "randomSeedAfter": seed,
            }
        )
        if value == 0:
            selected_choice_index = choice_index
            item_index = choice["itemIndex"]
            break
    if selected_choice_index is None or item_index is None:
        raise ValueError("blacksmith source-owned final row choice did not converge")
    orders_before = list(case["ordersBefore"])
    if len(orders_before) != constants["orderSlotCount"]:
        raise ValueError("blacksmith case order-slot width drift")
    order_write_index = next(
        (index for index, value in enumerate(orders_before) if value == 0), None
    )
    orders_after = orders_before.copy()
    if order_write_index is not None:
        orders_after[order_write_index] = item_index
    return {
        "id": case["id"],
        "classGroupIndex": class_group_index,
        "weaponRowIndex": row_index,
        "choiceIndex": selected_choice_index,
        "itemIndex": item_index,
        "orderWriteIndex": order_write_index,
        "ordersAfter": orders_after,
        "randomSeedAfter": seed,
        "rngCalls": rng_calls,
        "functionReturnSeen": True,
        "preservedD0": case["registerSentinels"]["d0"],
        "preservedD7": case["registerSentinels"]["d7"],
    }


def _transaction_chronology(static: dict[str, Any]) -> list[dict[str, int | str]]:
    transaction = static["transaction"]
    return [
        {"role": "place-entry", "pc": transaction["placeEntryAddress"]},
        {"role": "decrease-gold-call", "pc": transaction["decreaseGoldCallAddress"]},
        {
            "role": "decrease-gold-instruction-target",
            "pc": transaction["decreaseGoldInstructionTargetAddress"],
        },
        {
            "role": "decrease-gold-effective-target",
            "pc": transaction["decreaseGoldEffectiveTargetAddress"],
        },
        {
            "role": "decrease-gold-effective-return",
            "pc": transaction["decreaseGoldEffectiveReturnAddress"],
        },
        {
            "role": "pending-orders-incremented",
            "pc": transaction["pendingOrdersIncrementedObserveAddress"],
        },
        {"role": "drop-item-call", "pc": transaction["dropItemCallAddress"]},
        {
            "role": "drop-item-instruction-target",
            "pc": transaction["dropItemInstructionTargetAddress"],
        },
        {
            "role": "drop-item-effective-target",
            "pc": transaction["dropItemEffectiveTargetAddress"],
        },
        {
            "role": "drop-item-tail-update-target",
            "pc": transaction["dropItemTailUpdateTargetAddress"],
        },
        {
            "role": "drop-item-effective-return",
            "pc": transaction["dropItemEffectiveReturnAddress"],
        },
        {"role": "pick-mithril-call", "pc": transaction["pickMithrilCallAddress"]},
        {
            "role": "pick-mithril-effective-target",
            "pc": static["function"]["entryAddress"],
        },
        {
            "role": "pick-mithril-effective-return",
            "pc": static["function"]["returnRtsAddress"],
        },
        {"role": "clear-flag-call", "pc": transaction["clearFlagCallAddress"]},
        {
            "role": "clear-flag-instruction-target",
            "pc": transaction["clearFlagInstructionTargetAddress"],
        },
        {
            "role": "clear-flag-effective-target",
            "pc": transaction["clearFlagEffectiveTargetAddress"],
        },
        {
            "role": "clear-flag-pre-presentation-return",
            "pc": transaction["clearFlagEffectiveReturnAddress"],
        },
    ]


def model_transaction_case(case: dict[str, Any], static: dict[str, Any]) -> dict[str, Any]:
    """Derive the post-confirmation mutation state without importing a golden row."""
    constants = static["constants"]
    if case["goldBefore"] < constants["orderCost"]:
        raise ValueError("blacksmith transaction case violates caller gold gate")
    items_before = list(case["clientItemWordsBefore"])
    item_slot = case["itemSlot"]
    if not 0 <= item_slot < constants["combatantItemSlotCount"]:
        raise ValueError("blacksmith transaction item slot is outside source-owned domain")
    if len(items_before) != constants["combatantItemSlotCount"]:
        raise ValueError("blacksmith transaction item-word domain drift")
    if (
        items_before[item_slot] & constants["itemIndexMask"]
    ) != constants["mithrilItemIndex"]:
        raise ValueError("blacksmith transaction selected item is not source mithril")
    picker = model_case(
        {
            "id": case["id"],
            "clientClass": case["clientClass"],
            "randomSeedBefore": case["randomSeedBefore"],
            "ordersBefore": case["ordersBefore"],
            "registerSentinels": {"d0": 0, "d7": 0},
        },
        static,
    )
    items_after = items_before.copy()
    del items_after[item_slot]
    items_after.append(constants["itemNothingIndex"])
    flag_before = case["flag80OwningByteBefore"]
    flag_after = flag_before & (~constants["flag80BitMask"] & 0xFF)
    return {
        "id": case["id"],
        "clientMember": case["clientMember"],
        "itemSlot": item_slot,
        "goldBefore": case["goldBefore"],
        "goldAfter": case["goldBefore"] - constants["orderCost"],
        "pendingOrdersBefore": case["pendingOrdersBefore"],
        "pendingOrdersAfter": case["pendingOrdersBefore"] + 1,
        "clientItemWordsBefore": items_before,
        "clientItemWordsAfter": items_after,
        "ordersBefore": list(case["ordersBefore"]),
        "ordersAfter": picker["ordersAfter"],
        "flag80OwningByteBefore": flag_before,
        "flag80OwningByteAfter": flag_after,
        "randomSeedBefore": case["randomSeedBefore"],
        "randomSeedAfter": picker["randomSeedAfter"],
        "classGroupIndex": picker["classGroupIndex"],
        "weaponRowIndex": picker["weaponRowIndex"],
        "choiceIndex": picker["choiceIndex"],
        "itemIndex": picker["itemIndex"],
        "orderWriteIndex": picker["orderWriteIndex"],
        "rngCalls": picker["rngCalls"],
        "callbackChronology": _transaction_chronology(static),
        "safeExitOriginalReturnPc": static["transaction"]["prePresentationReturnAddress"],
        "safeExitSeen": True,
    }


def _fulfillment_chronology(static: dict[str, Any]) -> list[dict[str, int | str]]:
    fulfillment = static["fulfillment"]
    return [
        {"role": "fulfillment-add-item-call", "pc": fulfillment["addItemCallAddress"]},
        {
            "role": "fulfillment-add-item-instruction-target",
            "pc": fulfillment["addItemInstructionTargetAddress"],
        },
        {
            "role": "fulfillment-add-item-effective-target",
            "pc": fulfillment["addItemEffectiveTargetAddress"],
        },
        {
            "role": "fulfillment-add-item-effective-return",
            "pc": fulfillment["addItemEffectiveReturnAddress"],
        },
        {
            "role": "fulfillment-order-read",
            "pc": fulfillment["orderReadObserveAddress"],
        },
        {
            "role": "fulfillment-order-cleared",
            "pc": fulfillment["orderClearedObserveAddress"],
        },
        {
            "role": "fulfillment-orders-incremented",
            "pc": fulfillment["fulfilledOrdersIncrementedObserveAddress"],
        },
        {
            "role": "fulfillment-equippability-call",
            "pc": fulfillment["equippabilityCallAddress"],
        },
        {
            "role": "fulfillment-equippability-instruction-target",
            "pc": fulfillment["equippabilityInstructionTargetAddress"],
        },
        {
            "role": "fulfillment-equippability-effective-target",
            "pc": fulfillment["equippabilityEffectiveTargetAddress"],
        },
        {
            "role": "fulfillment-equippability-effective-return",
            "pc": fulfillment["equippabilityEffectiveReturnAddress"],
        },
    ]


def model_fulfillment_case(case: dict[str, Any], static: dict[str, Any]) -> dict[str, Any]:
    """Model the original direct @AddItem block from source/H1-derived facts."""
    constants = static["constants"]
    fulfillment = static["fulfillment"]
    items_before = list(case["clientItemWordsBefore"])
    orders_before = list(case["ordersBefore"])
    orders_counter = int(case["ordersCounter"])
    if not fulfillment["ordersCounterMinimum"] <= orders_counter <= fulfillment[
        "ordersCounterMaximum"
    ]:
        raise ValueError("blacksmith fulfillment ordersCounter is outside source domain")
    if len(items_before) != constants["combatantItemSlotCount"]:
        raise ValueError("blacksmith fulfillment inventory word domain drift")
    item_write_index = next(
        (
            index
            for index, item_word in enumerate(items_before)
            if (item_word & constants["itemIndexMask"]) == constants["itemNothingIndex"]
        ),
        None,
    )
    if item_write_index is None:
        raise ValueError("blacksmith fulfillment requires first ITEM_NOTHING inventory slot")
    selected_order_index = constants["orderSlotCount"] - orders_counter
    if not 0 <= selected_order_index < constants["orderSlotCount"]:
        raise ValueError("blacksmith fulfillment source-selected order slot drift")
    source_order_word_read = orders_before[selected_order_index]
    if source_order_word_read == 0:
        raise ValueError("blacksmith fulfillment target order is already empty")
    if source_order_word_read != case["itemIndex"]:
        raise ValueError("blacksmith fulfillment target order/item mismatch")
    items_after = items_before.copy()
    items_after[item_write_index] = case["itemIndex"] & constants["itemIndexAndBrokenMask"]
    orders_after = orders_before.copy()
    orders_after[selected_order_index] = 0
    class_mask = 1 << case["recipientClass"]
    definition = next(
        field
        for field in fulfillment["itemDefinitionFields"]
        if field["itemIndex"] == case["itemIndex"]
    )
    equip_flags = int.from_bytes(definition["equipFlagsBytes"], "big")
    item_type = definition["itemTypeBytes"][0]
    equippable_carry_set = bool(
        item_type & (constants["weaponTypeMask"] | constants["ringTypeMask"])
        and equip_flags & class_mask
    )
    if equippable_carry_set != bool(case["equippableCarrySet"]):
        raise ValueError("blacksmith fulfillment source-backed carry expectation drift")
    return {
        "id": case["id"],
        "clientMember": case["clientMember"],
        "recipientClass": case["recipientClass"],
        "itemIndex": case["itemIndex"],
        "clientItemWordsBefore": items_before,
        "clientItemWordsAfter": items_after,
        "itemWriteIndex": item_write_index,
        "addItemResultCode": 0,
        "ordersBefore": orders_before,
        "ordersAfter": orders_after,
        "ordersCounter": orders_counter,
        "selectedOrderIndex": selected_order_index,
        "sourceOrderWordRead": source_order_word_read,
        "fulfilledOrdersBefore": case["fulfilledOrdersBefore"],
        "fulfilledOrdersAfter": case["fulfilledOrdersBefore"] + 1,
        "equippableCarrySet": equippable_carry_set,
        "callbackChronology": _fulfillment_chronology(static),
        "safeExitOriginalReturnPc": fulfillment["postEquippabilityReturnAddress"],
        "safeExitSeen": True,
    }


def expected_observation(fixture: dict[str, Any], static: dict[str, Any]) -> dict[str, Any]:
    helper_records = [model_case(case, static) for case in fixture["cases"]]
    transaction_records = [
        model_transaction_case(case, static) for case in fixture["transactionCases"]
    ]
    fulfillment_records = [
        model_fulfillment_case(case, static) for case in fixture["fulfillmentCases"]
    ]
    return {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["id"],
        "caseOrder": [case["id"] for case in fixture["cases"]],
        "records": helper_records,
        "transactionCaseOrder": [case["id"] for case in fixture["transactionCases"]],
        "transactionRecords": transaction_records,
        "fulfillmentCaseOrder": [case["id"] for case in fixture["fulfillmentCases"]],
        "fulfillmentRecords": fulfillment_records,
        "callbacksCleared": 0,
        "restoration": {
            "currentGoldLongRestored": True,
            "randomSeedWordRestored": True,
            "orderWordsRestored": True,
            "flag80OwningByteRestored": True,
            "clientCombatantRecordsRestored": True,
        },
    }


def _validate_case_matrix(fixture: dict[str, Any], static: dict[str, Any]) -> None:
    if tuple(case["id"] for case in fixture["cases"]) != CASE_IDS or tuple(
        fixture["caseOrder"]
    ) != CASE_IDS:
        raise ValueError("blacksmith fixture case order drift")
    records = [model_case(case, static) for case in fixture["cases"]]
    fallback_records = [
        record
        for record in records
        if record["rngCalls"][0]["role"] == "fallback-row-roll"
    ]
    if [record["id"] for record in fallback_records] != list(CASE_IDS[2:4]):
        raise ValueError("blacksmith fallback case-role coverage drift")
    if [record["rngCalls"][0]["result"] for record in fallback_records] != [0, 1]:
        raise ValueError("blacksmith fallback outcome coverage drift")
    if [record["orderWriteIndex"] for record in records] != [0, 1, 2, 3, None]:
        raise ValueError("blacksmith first-empty order-slot coverage drift")
    if [call["rangeWord"] for call in records[1]["rngCalls"]] != [16, 8, 4, 1]:
        raise ValueError("blacksmith weighted final-fallback coverage drift")
    if records[0]["choiceIndex"] != 0 or records[1]["choiceIndex"] != 3:
        raise ValueError("blacksmith early/final choice coverage drift")
    if records[-1]["ordersAfter"] != fixture["cases"][-1]["ordersBefore"]:
        raise ValueError("blacksmith all-occupied no-write coverage drift")


def _validate_transaction_case_matrix(fixture: dict[str, Any], static: dict[str, Any]) -> None:
    if tuple(fixture["transactionCaseOrder"]) != TRANSACTION_CASE_IDS or tuple(
        case["id"] for case in fixture["transactionCases"]
    ) != TRANSACTION_CASE_IDS:
        raise ValueError("blacksmith transaction case order drift")
    helper_inputs = {
        (case["clientClass"], case["randomSeedBefore"], tuple(case["ordersBefore"]))
        for case in fixture["cases"]
    }
    transaction_inputs = {
        (case["clientClass"], case["randomSeedBefore"], tuple(case["ordersBefore"]))
        for case in fixture["transactionCases"]
    }
    if helper_inputs & transaction_inputs:
        raise ValueError("blacksmith transaction duplicates accepted helper-local case")
    records = [model_transaction_case(case, static) for case in fixture["transactionCases"]]
    if [record["orderWriteIndex"] for record in records] != [0, 2, 1]:
        raise ValueError("blacksmith transaction first-empty order-slot coverage drift")
    if [record["itemSlot"] for record in records] != [0, 1, 3]:
        raise ValueError("blacksmith transaction DropItemBySlot domain coverage drift")
    if [record["weaponRowIndex"] for record in records] != [3, 1, 2]:
        raise ValueError("blacksmith transaction class/row variation drift")
    if [record["choiceIndex"] for record in records] != [0, 3, 0]:
        raise ValueError("blacksmith transaction RNG outcome variation drift")
    if [len(record["rngCalls"]) for record in records] != [1, 4, 2]:
        raise ValueError("blacksmith transaction RNG call-count variation drift")
    if any(
        record["safeExitOriginalReturnPc"]
        != static["transaction"]["prePresentationReturnAddress"]
        for record in records
    ):
        raise ValueError("blacksmith transaction pre-presentation exit boundary drift")


def _validate_fulfillment_case_matrix(fixture: dict[str, Any], static: dict[str, Any]) -> None:
    if tuple(fixture["fulfillmentCaseOrder"]) != FULFILLMENT_CASE_IDS or tuple(
        case["id"] for case in fixture["fulfillmentCases"]
    ) != FULFILLMENT_CASE_IDS:
        raise ValueError("blacksmith fulfillment case order drift")
    records = [model_fulfillment_case(case, static) for case in fixture["fulfillmentCases"]]
    if [record["selectedOrderIndex"] for record in records] != [3, 2, 0]:
        raise ValueError("blacksmith fulfillment physical order-slot coverage drift")
    if [record["itemWriteIndex"] for record in records] != [3, 2, 0]:
        raise ValueError("blacksmith fulfillment first-ITEM_NOTHING coverage drift")
    if [record["equippableCarrySet"] for record in records] != [True, True, False]:
        raise ValueError("blacksmith fulfillment carry-polarity coverage drift")
    if [record["fulfilledOrdersAfter"] for record in records] != [1, 2, 3]:
        raise ValueError("blacksmith fulfillment counter-increment coverage drift")
    if any(
        record["safeExitOriginalReturnPc"]
        != static["fulfillment"]["postEquippabilityReturnAddress"]
        for record in records
    ):
        raise ValueError("blacksmith fulfillment stack-return exit boundary drift")


def _assert_golden(fixture: dict[str, Any], static: dict[str, Any]) -> dict[str, Any]:
    _validate_case_matrix(fixture, static)
    _validate_transaction_case_matrix(fixture, static)
    _validate_fulfillment_case_matrix(fixture, static)
    expected = expected_observation(fixture, static)
    accepted = fixture["acceptedObservation"]
    if accepted != expected:
        raise ValueError("blacksmith accepted observation disagrees with independent model")
    return expected


def _assert_observation(
    fixture: dict[str, Any], static: dict[str, Any], observed: dict[str, Any]
) -> None:
    expected = _assert_golden(fixture, static)
    if observed != expected:
        raise ValueError("blacksmith exact observed case matrix mismatch")


def _failure_diagnostic(status_path: Path) -> str | None:
    payload = callback_failure_status(status_path, owner=OWNER, schema_path=FAILURE_SCHEMA)
    if payload is None:
        return None
    lines = status_path.read_text(encoding="utf-8").splitlines()
    failures = [index for index, line in enumerate(lines) if line.startswith(STATUS_PREFIX)]
    if len(failures) != 1 or failures[0] != len(lines) - 1:
        raise ValueError(
            "blacksmith-mithril callback failure must be one terminal exact failure line"
        )
    if not any(line.startswith("milestone:") for line in lines[: failures[0]]):
        raise ValueError("blacksmith-mithril callback failure lacks preceding milestone")
    return json.dumps(payload, sort_keys=True)


def _assert_status(status_path: Path) -> None:
    assert_observer_status(
        status_path,
        owner=OWNER,
        schema_path=FAILURE_SCHEMA,
        required_milestones=(
            "milestone:direct-function-probe",
            "milestone:first-case-entered",
            "milestone:transaction-cases-entered",
            "milestone:fulfillment-cases-entered",
            "milestone:transaction-state-restored",
        ),
    )


def _observer_config(fixture: dict[str, Any], static: dict[str, Any]) -> dict[str, Any]:
    """Keep accepted output facts out of the executable observer configuration."""
    return {
        "id": fixture["id"],
        "core": fixture["emulator"]["core"],
        "cases": fixture["cases"],
        "caseOrder": fixture["caseOrder"],
        "transactionCases": fixture["transactionCases"],
        "transactionCaseOrder": fixture["transactionCaseOrder"],
        "fulfillmentCases": fixture["fulfillmentCases"],
        "fulfillmentCaseOrder": fixture["fulfillmentCaseOrder"],
        "function": static["function"],
        "transaction": {
            key: value
            for key, value in static["transaction"].items()
            if key != "h1InstructionBytes"
        },
        "fulfillment": {
            key: value
            for key, value in static["fulfillment"].items()
            if key not in {"h1InstructionBytes", "itemDefinitionFields"}
        },
        "ram": static["ram"],
        "constants": static["constants"],
        "observerFailureContract": OBSERVER_FAILURE_CONTRACT,
    }


def verify_blacksmith_mithril(
    rom_path: Path, upstream_path: Path = UPSTREAM, *, timeout_seconds: int = 180
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner="blacksmith mithril fixture")
    verify_runtime_contract(fixture, rom_path)
    static = validate_static_contract(fixture, rom_path, upstream_path)
    _assert_golden(fixture, static)
    status_path = DERIVED_ROOT / f"{OWNER}.status.txt"
    try:
        observed = run_observer(
            rom_path=rom_path,
            observer_path=OBSERVER,
            config=_observer_config(fixture, static),
            output_name=OWNER,
            timeout_seconds=timeout_seconds,
        )
    except RuntimeError as error:
        diagnostic = _failure_diagnostic(status_path)
        if diagnostic is not None:
            raise RuntimeError(f"{OWNER} observer callback failure: {diagnostic}") from error
        raise
    _assert_status(status_path)
    validate_json(observed, OBSERVATION_SCHEMA, owner="blacksmith mithril observation")
    _assert_observation(fixture, static, observed)
    return {
        "Fixture": fixture["id"],
        "Cases": len(fixture["cases"])
        + len(fixture["transactionCases"])
        + len(fixture["fulfillmentCases"]),
        "HelperCases": len(fixture["cases"]),
        "TransactionCases": len(fixture["transactionCases"]),
        "FulfillmentCases": len(fixture["fulfillmentCases"]),
        "BizHawkLaunches": 1,
        "CallbacksCleared": observed["callbacksCleared"],
        "Restoration": observed["restoration"],
        "Status": "PASS",
    }
