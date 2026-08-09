"""One-launch direct observation of blacksmith picker, commitment, and fulfillment seams.

The work-RAM probe enters only original ROM routines after the ordinary startup
``CheckSram`` return.  It retains the accepted direct ``PickMithrilWeapon``
matrix and adds separate, post-confirmation ``@PlaceOrder`` and fulfillment
matrices.  The commitment cohort redirects only the original ``ClearFlag``
return away from the first text trap.  The fulfillment cohort enters the
original ``@AddItem`` block and redirects only the original
``IsWeaponOrRingEquippable`` return away from its following branch.  The v4
   pre-commit cohort starts at the original fulfillment selection-loop label,
controls named service returns through generated work-RAM stubs, and stops at
``@AddItem`` or a source branch immediately before an excluded presentation
   path.  The v5 equip-decision cohort continues the original ``@AddItem``
   return only through neutral pre-presentation boundaries.  Its one controlled
   prompt result is a session-ROM harness input; it never executes text, sound,
   music, or prompt bodies.
"""

from __future__ import annotations

import hashlib
import json
import re
import tempfile
from pathlib import Path
from typing import Any

from sf2tool.h3 import rng
from sf2tool.h3.bizhawk import DERIVED_ROOT, run_observer, verify_runtime_contract
from sf2tool.h3.map_lifecycle import _with_instrumented_rom_database
from sf2tool.h3.observer_status import (
    CALLBACK_FAILURE_PREFIX,
    assert_observer_status,
    callback_failure_status,
    observer_failure_contract,
)
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.rom import inspect_rom

FIXTURE = repo_path("tests/fixtures/h3/blacksmith-mithril-v5.json")
FIXTURE_SCHEMA = repo_path("schemas/h3/blacksmith-mithril-fixture.schema.json")
OBSERVATION_SCHEMA = repo_path("schemas/h3/blacksmith-mithril-observation.schema.json")
FAILURE_SCHEMA = repo_path("schemas/h3/blacksmith-mithril-callback-failure.schema.json")
OBSERVER = repo_path("tools/bizhawk/blacksmith_mithril_observer.lua")
TOOLCHAIN_MANIFEST = repo_path("manifests/toolchain.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")
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
PRECOMMIT_CASE_IDS = (
    "recipient-cancel-pre-presentation",
    "full-inventory-pre-presentation",
    "tool-direct-add-item-admission",
    "equippable-direct-add-item-admission",
    "non-equippable-pre-presentation",
)
PRECOMMIT_CASE_FRAME_BUDGET = 180
PRECOMMIT_TRANSITION_FRAME_BUDGET = 180
PRECOMMIT_SERVICE_STUB_ADDRESS = 0xFF6D00
PRECOMMIT_SERVICE_STUB_SIZE = 6
PRECOMMIT_TERMINAL_STUB_ADDRESS = 0xFF6D20
PRECOMMIT_TERMINAL_STUB_SIZE = 6
PRECOMMIT_CLEANUP_STACK_DEPTH_BYTES = 8
EQUIP_DECISION_CASE_IDS = (
    "non-equippable-no-prompt-do-not-equip",
    "hero-levanter-prompt-decline-do-not-equip",
    "hero-levanter-no-equipped-weapon",
    "hero-levanter-replaces-uncursed-battle-sword",
    "hero-levanter-blocked-by-cursed-dark-sword",
)
EQUIP_DECISION_PROMPT_STUB_ADDRESS = 0xFF6D40
EQUIP_DECISION_PROMPT_STUB_SIZE = 6
EQUIP_DECISION_TERMINAL_STUB_ADDRESS = 0xFF6D60
EQUIP_DECISION_TERMINAL_STUB_SIZE = 6


def _retained_v4_projection(fixture: dict[str, Any]) -> dict[str, Any]:
    """Return the accepted v4 corpus exactly, excluding only the v5 root ID.

    v5 is intentionally additive: its new cohort may change the encompassing
    observation identity but may not silently rewrite any accepted v4 input or
    observation.  Keep this projection small and named instead of embedding a
    second large corpus in a schema.
    """

    keys = (
        "caseOrder",
        "cases",
        "transactionCaseOrder",
        "transactionCases",
        "fulfillmentCaseOrder",
        "fulfillmentCases",
        "precommitCaseOrder",
        "precommitCases",
    )
    observation_keys = (
        "system",
        "core",
        "caseOrder",
        "records",
        "transactionCaseOrder",
        "transactionRecords",
        "fulfillmentCaseOrder",
        "fulfillmentRecords",
        "precommitCaseOrder",
        "precommitRecords",
        "callbacksCleared",
        "precommitInstrumentation",
        "precommitRestoration",
        "restoration",
    )
    accepted = fixture["acceptedObservation"]
    return {
        **{key: fixture[key] for key in keys},
        "acceptedObservation": {key: accepted[key] for key in observation_keys},
    }


def _retained_v4_sha256(fixture: dict[str, Any]) -> str:
    encoded = json.dumps(
        _retained_v4_projection(fixture), sort_keys=True, separators=(",", ":")
    ).encode("utf-8")
    return hashlib.sha256(encoded).hexdigest().upper()


def _assert_retained_v4_digest(fixture: dict[str, Any]) -> None:
    retained = fixture.get("retainedV4")
    if not isinstance(retained, dict) or retained.get("caseCount") != 16:
        raise ValueError("blacksmith retained-v4 case-count guard drift")
    if retained.get("sha256") != _retained_v4_sha256(fixture):
        raise ValueError("blacksmith retained-v4 digest guard drift")


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
    matches = re.findall(rf"^\s*{re.escape(name)}\s*=\s*(-\d+)\s*$", declarations, re.MULTILINE)
    if len(matches) != 1:
        raise ValueError(f"blacksmith source local declaration drift: {name}")
    return int(matches[0])


def _source_frame_offsets(source: str, symbol: str, names: tuple[str, ...]) -> dict[str, int]:
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
        "COMBATANT_OFFSET_STATUSEFFECTS",
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
        "ITEMTYPE_CURSED",
        "ITEMENTRY_BIT_EQUIPPED",
        "STATUSEFFECT_CURSE",
        "EQUIPMENTTYPE_TOOL",
        "EQUIPMENTTYPE_WEAPON",
        "EQUIPMENTTYPE_RING",
        "CLASS_HERO",
        "CLASS_VICR",
        "CLASS_SNIP",
        "ITEM_LEVANTER",
        "ITEM_GODDESS_STAFF",
        "ITEM_MYSTERY_STAFF",
        "ITEM_BATTLE_SWORD",
        "ITEM_DARK_SWORD",
        "CLASS_BRN",
        "CLASS_RDBN",
        "ITEM_MITHRIL",
        "MITHRIL_WEAPONS_ON_ORDER",
        "RANDOM_SEED",
        "DIALOGUE_NAME_INDEX_1",
        "SELECTED_ITEM_INDEX",
        "CURRENT_ITEM_SUBMENU_ACTION",
        "COMBATANT_OFFSET_STATUSEFFECTS",
        "STATUSEFFECT_CURSE",
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
        or re.fullmatch(r"move\.([bwl]) d1,\(a0\)", _h1_text(write_instruction)) is None
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
    branch = re.fullmatch(r"(?:b[a-z]+)\.(s|w)\s+([@A-Za-z_][@A-Za-z0-9_]*)", text)
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


def _require_fulfillment_precommit_source_shape(actions_source: str) -> None:
    """Guard the handler-local admission path before the original ``@AddItem`` entry.

    The service calls are intentionally retained as named seams.  The H3
    observer supplies their returns as harness controls, so this parser does
    not attribute member-list or yes/no UI behaviour to the original game.
    """
    section = _source_section(actions_source, "BlacksmithAction_FulfillOrder")
    add_item = section.find("@AddItem:")
    if add_item < 0:
        raise ValueError("blacksmith fulfillment precommit source missing @AddItem")
    precommit = section[:add_item]
    _require_source_sequence(
        precommit,
        (
            "blacksmithaction_fulfillorder:",
            "movem.l d0-a1,-(sp)",
            "move.w itemindex(a6),((dialogue_name_index_1-$1000000)).w",
            "txt 207",
            "txt 166",
            "clstxt",
            "byte_21b58:",
            "clstxt",
            "move.w itemindex(a6),((selected_item_index-$1000000)).w",
            "move.b #item_submenu_action_use,((current_item_submenu_action-$1000000)).w",
            "jsr j_executememberslistscreenonitemsummarypage",
            "cmpi.w #-1,d0",
            "bne.s @ismemberinventoryfull",
            "bra.w @done",
            "@ismemberinventoryfull:",
            "move.w d0,clientmember(a6)",
            "moveq #0,d1",
            "jsr j_getitembyslotandhelditemsnumber",
            "cmpi.w #combatant_itemslots,d2",
            "bcs.s @checkequipmenttype",
            "move.w clientmember(a6),((dialogue_name_index_1-$1000000)).w",
            "txt 208",
            "jsr j_alt_yesnoprompt",
            "cmpi.w #0,d0",
            "beq.s byte_21b58",
            "bra.w @done",
            "@checkequipmenttype:",
            "move.w itemindex(a6),d1",
            "jsr j_getequipmenttype",
            "cmpi.w #equipmenttype_tool,d2",
            "beq.s @additem",
            "move.w itemindex(a6),d1",
            "move.w clientmember(a6),d0",
            "jsr j_isweaponorringequippable",
            "bcs.s @additem",
            "move.w clientmember(a6),((dialogue_name_index_1-$1000000)).w",
            "txt 167",
            "jsr j_alt_yesnoprompt",
            "cmpi.w #0,d0",
            "bne.w byte_21b58",
        ),
        name="BlacksmithAction_FulfillOrder precommit admission block",
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


def _require_fulfillment_equip_decision_source_shape(actions_source: str, item_source: str) -> None:
    """Guard the complete post-``@AddItem`` decision section before runtime.

    The checks deliberately retain the ring and newly-equipped-cursed paths even
    though the current Mithril table domain statically excludes them.  Source
    presence is not runtime reachability.
    """

    _require_source_sequence(
        _source_section(actions_source, "BlacksmithAction_FulfillOrder"),
        (
            "@additem:",
            "jsr j_isweaponorringequippable",
            "bcc.w byte_21cd0",
            "txt 173",
            "jsr j_alt_yesnoprompt",
            "cmpi.w #0,d0",
            "bne.w byte_21cd0",
            "move.w itemindex(a6),d1",
            "jsr j_getequipmenttype",
            "cmpi.w #equipmenttype_weapon,d2",
            "bne.s @hasringequipped",
            "move.w clientmember(a6),d0",
            "jsr j_getequippedweapon",
            "cmpi.w #-1,d1",
            "beq.s @equipnewitem",
            "move.w d2,d1",
            "jsr j_unequipitembyslotifnotcursed",
            "cmpi.w #2,d2",
            "bne.w @equipnewitem",
            "txt 176",
            "bra.s byte_21cd0",
            "@hasringequipped:",
            "move.w clientmember(a6),d0",
            "jsr j_getequippedring",
            "cmpi.w #-1,d1",
            "beq.s @equipnewitem",
            "move.w d2,d1",
            "jsr j_unequipitembyslotifnotcursed",
            "cmpi.w #2,d2",
            "bne.w @equipnewitem",
            "txt 176",
            "bra.s byte_21cd0",
            "@equipnewitem:",
            "moveq #0,d1",
            "jsr j_getitembyslotandhelditemsnumber",
            "move.w d2,d1",
            "subq.w #1,d1",
            "jsr j_equipitembyslot",
            "cmpi.w #2,d2",
            "bne.s byte_21cc8",
            "sndcom music_cursed_item",
            "bsr.w waitformusicresumeandplayerinput_blacksmith",
            "txt 175",
            "bra.w @done",
            "@notcursed:",
            "txt 174",
            "bra.w @done",
            "@donotequipnewitem:",
            "txt 209",
            "@done:",
        ),
        name="BlacksmithAction_FulfillOrder post-AddItem equip decision",
    )
    _require_source_sequence(
        _source_section(item_source, "GetEquipmentType"),
        (
            "getequipmenttype:",
            "bsr.s getitemdefinitionaddress",
            "addq.w #itemdef_offset_type,a0",
            "btst #itemtype_bit_weapon,(a0)",
            "bne.s @weapon",
            "btst #itemtype_bit_ring,(a0)",
            "bne.s @ring",
            "clr.w d2",
            "@ring:",
            "move.w #equipmenttype_ring,d2",
            "@weapon:",
            "move.w #equipmenttype_weapon,d2",
        ),
        name="GetEquipmentType weapon/ring split",
    )
    _require_source_sequence(
        _source_section(item_source, "GetEquippedWeapon"),
        (
            "getequippedweapon:",
            "movem.l d3-d4/a0-a1,-(sp)",
            "move.w #itemtype_weapon,d4",
            "bra.s getequippeditembytype",
        ),
        name="GetEquippedWeapon type wrapper",
    )
    _require_source_sequence(
        _source_section(item_source, "GetEquippedRing"),
        ("getequippedring:", "movem.l d3-d4/a0-a1,-(sp)", "move.w #itemtype_ring,d4"),
        name="GetEquippedRing type wrapper",
    )
    _require_source_sequence(
        _source_section(item_source, "UnequipItemBySlotIfNotCursed"),
        (
            "unequipitembyslotifnotcursed:",
            "bsr.s isiteminslotequippedorcursed",
            "tst.w d2",
            "bne.s @skip",
            "bclr #itementry_bit_equipped,itementry_offset_index_and_equipped_bit(a0)",
            "bra.w updatecombatantstats",
        ),
        name="UnequipItemBySlotIfNotCursed result/mutation tail",
    )
    _require_source_sequence(
        _source_section(item_source, "IsItemInSlotEquippedOrCursed"),
        (
            "isiteminslotequippedorcursed:",
            "andi.w #itementry_mask_index,d1",
            "cmpi.w #item_nothing,d1",
            "beq.s @emptyslot",
            "btst #itementry_bit_equipped,itementry_offset_index_and_equipped_bit(a0)",
            "beq.s @notequipped",
            "btst #itemtype_bit_cursed,itemdef_offset_type(a0)",
            "bne.s @cursed",
            "clr.w d2",
            "@cursed:",
            "move.w #2,d2",
        ),
        name="IsItemInSlotEquippedOrCursed curse result",
    )
    _require_source_sequence(
        _source_section(item_source, "EquipItemBySlot"),
        (
            "equipitembyslot:",
            "andi.w #itementry_mask_index,d1",
            "cmpi.w #item_nothing,d1",
            "beq.s @nothing",
            "bsr.s isitemequippableandcursed",
            "cmpi.w #1,d2",
            "beq.s @goto_done",
            "bset #itementry_bit_equipped,itementry_offset_index_and_equipped_bit(a0)",
            "bra.w updatecombatantstats",
        ),
        name="EquipItemBySlot equipped-bit/result tail",
    )
    _require_source_sequence(
        _source_section(item_source, "IsItemEquippableAndCursed"),
        (
            "isitemequippableandcursed:",
            "move.b combatant_offset_class(a0),d0",
            "addq.b #1,d0",
            "move.l (a0),d1",
            "lsr.l d0,d1",
            "bcc.s @notequippable",
            "btst #itemtype_bit_cursed,itemdef_offset_type(a0)",
            "bne.s @equippableandcursed",
            "clr.w d2",
            "@equippableandcursed:",
            "move.w #2,d2",
            "@notequippable:",
            "move.w #1,d2",
        ),
        name="IsItemEquippableAndCursed result contract",
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
        if (
            declared["fixture"] != expected_path.relative_to(repo_path(".")).as_posix()
            or declared["fixtureId"] != owner["id"]
        ):
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
    call_indices = [index for index, token in enumerate(picker_tokens) if token == generator_call]
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
    actions_source = actions_source_text or (disasm / BLACKSMITH_ACTIONS_RELATIVE).read_text(
        encoding="utf-8"
    )
    gold_source = gold_source_text or (disasm / GOLD_SOURCE_RELATIVE).read_text(encoding="utf-8")
    item_source = item_source_text or (disasm / ITEM_SOURCE_RELATIVE).read_text(encoding="utf-8")
    flag_source = flag_source_text or (disasm / FLAG_SOURCE_RELATIVE).read_text(encoding="utf-8")
    combatant_source = combatant_source_text or (disasm / COMBATANT_SOURCE_RELATIVE).read_text(
        encoding="utf-8"
    )
    itemdefs_source = itemdefs_source_text or (disasm / ITEMDEFS_SOURCE_RELATIVE).read_text(
        encoding="utf-8"
    )
    _require_pick_source_shape(pick_source)
    _require_rng_source_shape(rng_source)
    readiness_flag_id = _require_place_order_source_shape(actions_source)
    _require_supporting_mutation_source_shape(
        gold_source, item_source, flag_source, combatant_source
    )
    _require_fulfillment_precommit_source_shape(actions_source)
    _require_fulfillment_source_shape(actions_source, item_source)
    _require_fulfillment_equip_decision_source_shape(actions_source, item_source)
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
        h2_readiness_flag_id = common_menus["expected"]["menuFacts"]["serviceStateMachines"][
            "blacksmith"
        ]["derived"]["process"]["readiness"]["flagId"]
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
        group for group in groups if group == [required["CLASS_BRN"], required["CLASS_RDBN"]]
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
        item_owner["table"]["list_MithrilWeaponClasses"] != h1_entries["list_MithrilWeaponClasses"]
        or item_owner["table"]["table_MithrilWeapons"] != h1_entries["table_MithrilWeapons"]
    ):
        raise ValueError("blacksmith item-owner H1 table address drift")
    try:
        common_stats_itemstats_symbol = common_stats["expected"]["representativeSymbols"][
            "itemstats.asm"
        ]
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
    mithril_item_indexes = {int(choice["itemIndex"]) for row in rows for choice in row}
    fulfillment_item_indexes = {int(case["itemIndex"]) for case in fixture["fulfillmentCases"]}
    equip_decision_item_indexes = {int(case["itemIndex"]) for case in fixture["equipDecisionCases"]}
    prior_equipment_item_indexes = {
        int(case["existingEquippedItemIndex"])
        for case in fixture["equipDecisionCases"]
        if case["existingEquippedItemIndex"] is not None
    }
    required_item_indexes = (
        mithril_item_indexes
        | fulfillment_item_indexes
        | equip_decision_item_indexes
        | prior_equipment_item_indexes
    )
    if any(
        item_index < 0 or item_index >= item_definition_count
        for item_index in required_item_indexes
    ):
        raise ValueError("blacksmith fulfillment/equip item-definition source domain drift")
    item_definitions = _item_definition_rows(
        itemdefs_source,
        equates,
        required_item_indexes,
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
        required["BLACKSMITH_MAX_ORDERS_NUMBER"] != required["BLACKSMITH_ORDERS_COUNTER"] + 1
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
        or source_context["itemDefinitionsSourcePath"] != ITEMDEFS_SOURCE_RELATIVE.as_posix()
        or source_context["h1ListingPath"] != LISTING_RELATIVE.as_posix()
        or source_context["functionEntryAddress"] != entry
    ):
        raise ValueError("blacksmith fixture source-context identity drift")

    place_labels, place_instructions = _listing_section(listing, "BlacksmithAction_PlaceOrder")
    if source_context["placeEntryAddress"] != place_labels["@PlaceOrder"]:
        raise ValueError("blacksmith fixture place-order source-context drift")
    fulfill_labels, fulfill_instructions = _listing_section(
        listing, "BlacksmithAction_FulfillOrder"
    )
    fulfill_post_equippability = next(
        instruction
        for instruction in fulfill_instructions
        if _h1_text(instruction).startswith("bcc.w byte_21CD0")
    )
    if (
        source_context["fulfillAddItemEntryAddress"] != fulfill_labels["@AddItem"]
        or source_context["fulfillEntryAddress"] != fulfill_labels["BlacksmithAction_FulfillOrder"]
        or source_context["fulfillSelectionLoopAddress"] != fulfill_labels["byte_21B58"]
        or source_context["fulfillDoneAddress"] != fulfill_labels["@Done"]
        or source_context["fulfillPostEquippabilityAddress"]
        != fulfill_post_equippability["address"]
    ):
        raise ValueError("blacksmith fixture fulfill-order source-context drift")
    add_labels, add_instructions = _listing_section(listing, "AddItem")
    equippable_labels, equippable_instructions = _listing_section(
        listing, "IsWeaponOrRingEquippable"
    )
    equipment_type_labels, equipment_type_instructions = _listing_section(
        listing, "GetEquipmentType"
    )
    equipped_weapon_labels, equipped_weapon_instructions = _listing_section(
        listing, "GetEquippedWeapon"
    )
    equipped_ring_labels, equipped_ring_instructions = _listing_section(listing, "GetEquippedRing")
    equipped_item_labels, equipped_item_instructions = _listing_symbol_section(
        listing, "GetEquippedItemByType"
    )
    held_items_labels, held_items_instructions = _listing_section(
        listing, "GetItemBySlotAndHeldItemsNumber"
    )
    unequip_labels, unequip_instructions = _listing_section(listing, "UnequipItemBySlotIfNotCursed")
    equip_labels, equip_instructions = _listing_section(listing, "EquipItemBySlot")
    item_curse_labels, item_curse_instructions = _listing_section(
        listing, "IsItemEquippableAndCursed"
    )
    decrease_labels, decrease_instructions = _listing_section(listing, "DecreaseGold")
    drop_labels, drop_instructions = _listing_section(listing, "DropItemBySlot")
    clear_labels, clear_instructions = _listing_section(listing, "ClearFlag")
    get_flag_labels, get_flag_instructions = _listing_section(listing, "GetFlag")
    combatant_labels, combatant_instructions = _listing_section(listing, "GetCombatantEntryAddress")
    update_labels, update_instructions = _listing_section(listing, "UpdateCombatantStats")

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
    pending_increment_after_record = successor_record(place_instructions, pending_increment_record)
    client_member_record = instruction_record(place_instructions, "move.w clientMember(a6),d0")
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
        or int.from_bytes(clear_flag_load_record["bytes"][-2:], "big") != readiness_flag_id
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
    flag_base_record = instruction_record(get_flag_instructions, "lea ((GAME_FLAGS-$1000000)).w,a0")
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
        or combatant_labels["GetCombatantEntryAddress"] != h1_entries["GetCombatantEntryAddress"]
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

    precommit_instructions = [
        instruction
        for instruction in fulfill_instructions
        if fulfill_labels["BlacksmithAction_FulfillOrder"]
        <= instruction["address"]
        < fulfill_labels["@AddItem"]
    ]

    def precommit_instruction(text: str, *, occurrence: int = 1) -> dict[str, Any]:
        matches = [item for item in precommit_instructions if _h1_text(item) == text]
        if len(matches) < occurrence:
            raise ValueError(f"blacksmith precommit H1 instruction missing: {text}")
        return matches[occurrence - 1]

    def precommit_call(
        record: dict[str, Any], instruction_symbol: str, effective_symbol: str
    ) -> dict[str, int]:
        if (
            len(record["bytes"]) != 6
            or record["bytes"][:2] != b"\x4e\xb9"
            or int.from_bytes(record["bytes"][-4:], "big") != h1_entries[instruction_symbol]
        ):
            raise ValueError("blacksmith precommit H1 call instruction-target drift")
        return {
            "callAddress": record["address"],
            "instructionTargetAddress": h1_entries[instruction_symbol],
            "effectiveTargetAddress": h1_entries[effective_symbol],
            "returnAddress": record["address"] + len(record["bytes"]),
        }

    precommit_member_list_call = precommit_instruction(
        "jsr j_ExecuteMembersListScreenOnItemSummaryPage"
    )
    precommit_held_items_call = precommit_instruction("jsr j_GetItemBySlotAndHeldItemsNumber")
    precommit_equipment_type_call = precommit_instruction("jsr j_GetEquipmentType")
    precommit_equippability_call = precommit_instruction("jsr j_IsWeaponOrRingEquippable")
    precommit_full_yes_no_call = precommit_instruction("jsr j_alt_YesNoPrompt")
    precommit_nonequippable_yes_no_call = precommit_instruction(
        "jsr j_alt_YesNoPrompt", occurrence=2
    )
    precommit_member_cancel_compare = precommit_instruction("cmpi.w #-1,d0")
    precommit_member_cancel_branch = precommit_instruction("bne.s @IsMemberInventoryFull")
    precommit_member_cancel_done = precommit_instruction("bra.w @Done", occurrence=1)
    precommit_capacity_compare = precommit_instruction("cmpi.w #COMBATANT_ITEMSLOTS,d2")
    precommit_capacity_branch = precommit_instruction("bcs.s @CheckEquipmentType")
    precommit_full_prompt_compare = precommit_instruction("cmpi.w #0,d0")
    precommit_full_prompt_branch = precommit_instruction("beq.s byte_21B58")
    precommit_full_prompt_done = precommit_instruction("bra.w @Done", occurrence=2)
    precommit_equipment_type_compare = precommit_instruction("cmpi.w #EQUIPMENTTYPE_TOOL,d2")
    precommit_tool_branch = precommit_instruction("beq.s @AddItem")
    precommit_equippability_branch = precommit_instruction("bcs.s @AddItem")
    precommit_nonequippable_prompt_compare = precommit_instruction("cmpi.w #0,d0", occurrence=2)
    precommit_nonequippable_retry_branch = precommit_instruction("bne.w byte_21B58")
    precommit_client_member_store = precommit_instruction("move.w d0,clientMember(a6)")
    precommit_type_item_index_load = precommit_instruction("move.w itemIndex(a6),d1")
    precommit_text_traps = [
        instruction
        for instruction in precommit_instructions
        if _h1_text(instruction) == "M trap #textbox"
    ]

    def h1_span(instructions: list[dict[str, Any]], address: int, size: int = 6) -> bytes:
        """Read one exact instrumented-ROM span from consecutive H1 records."""
        start_index = next(
            (
                index
                for index, instruction in enumerate(instructions)
                if instruction["address"] == address
            ),
            None,
        )
        if start_index is None:
            raise ValueError("blacksmith precommit terminal boundary H1 start drift")
        chunks: list[bytes] = []
        next_address = address
        for instruction in instructions[start_index:]:
            if instruction["address"] != next_address:
                raise ValueError("blacksmith precommit terminal boundary H1 continuity drift")
            encoded = instruction["bytes"]
            chunks.append(encoded)
            next_address += len(encoded)
            if sum(len(chunk) for chunk in chunks) >= size:
                break
        joined = b"".join(chunks)
        if len(joined) < size:
            raise ValueError("blacksmith precommit terminal boundary H1 width drift")
        return joined[:size]

    def precommit_text_boundary(
        record: dict[str, Any], *, text_id: int, name: str
    ) -> tuple[int, bytes]:
        index = precommit_instructions.index(record)
        if (
            record["bytes"] != b"\x4e\x45"
            or index + 1 >= len(precommit_instructions)
            or precommit_instructions[index + 1]["address"]
            != record["address"] + len(record["bytes"])
            or precommit_instructions[index + 1]["bytes"] != text_id.to_bytes(2, "big")
        ):
            raise ValueError(f"blacksmith precommit {name} text boundary H1 drift")
        return record["address"], h1_span(precommit_instructions, record["address"])

    def precommit_displacement(record: dict[str, Any], name: str) -> int:
        encoded = record["bytes"]
        if len(encoded) < 4:
            raise ValueError(f"blacksmith precommit H1 frame displacement width drift: {name}")
        return int.from_bytes(encoded[-2:], "big", signed=True)

    if (
        len(precommit_text_traps) != 8
        or precommit_member_list_call["address"] != fulfill_labels["byte_21B58"] + 16
        or precommit_member_cancel_compare["address"]
        != precommit_member_list_call["address"] + len(precommit_member_list_call["bytes"])
        or precommit_member_cancel_branch["address"]
        != precommit_member_cancel_compare["address"]
        + len(precommit_member_cancel_compare["bytes"])
        or precommit_member_cancel_done["address"] <= precommit_member_cancel_branch["address"]
        or precommit_capacity_compare["address"]
        != precommit_held_items_call["address"] + len(precommit_held_items_call["bytes"])
        or precommit_capacity_branch["address"]
        != precommit_capacity_compare["address"] + len(precommit_capacity_compare["bytes"])
        or precommit_full_prompt_compare["address"]
        != precommit_full_yes_no_call["address"] + len(precommit_full_yes_no_call["bytes"])
        or precommit_full_prompt_branch["address"]
        != precommit_full_prompt_compare["address"] + len(precommit_full_prompt_compare["bytes"])
        or precommit_full_prompt_done["address"] <= precommit_full_prompt_branch["address"]
        or precommit_equipment_type_compare["address"]
        != precommit_equipment_type_call["address"] + len(precommit_equipment_type_call["bytes"])
        or precommit_tool_branch["address"]
        != precommit_equipment_type_compare["address"]
        + len(precommit_equipment_type_compare["bytes"])
        or precommit_equippability_branch["address"]
        != precommit_equippability_call["address"] + len(precommit_equippability_call["bytes"])
        or precommit_nonequippable_prompt_compare["address"]
        != precommit_nonequippable_yes_no_call["address"]
        + len(precommit_nonequippable_yes_no_call["bytes"])
        or precommit_nonequippable_retry_branch["address"]
        != precommit_nonequippable_prompt_compare["address"]
        + len(precommit_nonequippable_prompt_compare["bytes"])
        or precommit_displacement(precommit_client_member_store, "clientMember")
        != fulfillment_frame_offsets["clientMember"]
        or precommit_displacement(precommit_type_item_index_load, "itemIndex")
        != fulfillment_frame_offsets["itemIndex"]
        or fulfill_labels["@Done"] <= fulfill_labels["@AddItem"]
        or precommit_member_cancel_branch["bytes"][0] != 0x66
        or precommit_capacity_branch["bytes"][0] != 0x65
        or precommit_full_prompt_branch["bytes"][0] != 0x67
        or precommit_tool_branch["bytes"][0] != 0x67
        or precommit_equippability_branch["bytes"][0] != 0x65
        or precommit_nonequippable_retry_branch["bytes"][0] != 0x66
    ):
        raise ValueError("blacksmith precommit source/H1 branch chronology drift")
    if len(precommit_text_traps) != 8:
        raise ValueError("blacksmith precommit text-trap inventory drift")
    recipient_cancel_text_address, recipient_cancel_text_span = precommit_text_boundary(
        precommit_text_traps[4], text_id=197, name="recipient-cancel"
    )
    full_inventory_text_address, full_inventory_text_span = precommit_text_boundary(
        precommit_text_traps[5], text_id=208, name="full-inventory"
    )
    non_equippable_text_address, non_equippable_text_span = precommit_text_boundary(
        precommit_text_traps[7], text_id=167, name="non-equippable"
    )
    add_item_entry = next(
        instruction
        for instruction in fulfill_instructions
        if instruction["address"] == fulfill_labels["@AddItem"]
    )
    if _h1_text(add_item_entry) != "move.w clientMember(a6),d0":
        raise ValueError("blacksmith precommit AddItem terminal boundary H1 drift")
    if (
        recipient_cancel_text_address
        != precommit_member_cancel_branch["address"] + len(precommit_member_cancel_branch["bytes"])
        or full_inventory_text_address <= precommit_capacity_branch["address"]
        or non_equippable_text_address <= precommit_equippability_branch["address"]
        or add_item_entry["address"] != fulfill_labels["@AddItem"]
    ):
        raise ValueError("blacksmith precommit terminal boundary source chronology drift")
    if (
        required["EQUIPMENTTYPE_TOOL"] != 0
        or required["EQUIPMENTTYPE_WEAPON"] == required["EQUIPMENTTYPE_TOOL"]
        or required["COMBATANT_ITEMSLOTS"] != required["COMBATANT_ITEMSLOTS_COUNTER"] + 1
    ):
        raise ValueError("blacksmith precommit source constant relation drift")

    precommit = {
        "entryAddress": fulfill_labels["BlacksmithAction_FulfillOrder"],
        "selectionLoopAddress": fulfill_labels["byte_21B58"],
        "runtimeStartAddress": fulfill_labels["byte_21B58"],
        "doneAddress": fulfill_labels["@Done"],
        "addItemEntryAddress": fulfill_labels["@AddItem"],
        "memberList": precommit_call(
            precommit_member_list_call,
            "j_ExecuteMembersListScreenOnItemSummaryPage",
            "ExecuteMembersListScreenOnItemSummaryPage",
        ),
        "heldItems": precommit_call(
            precommit_held_items_call,
            "j_GetItemBySlotAndHeldItemsNumber",
            "GetItemBySlotAndHeldItemsNumber",
        ),
        "equipmentType": precommit_call(
            precommit_equipment_type_call,
            "j_GetEquipmentType",
            "GetEquipmentType",
        ),
        "equippability": precommit_call(
            precommit_equippability_call,
            "j_IsWeaponOrRingEquippable",
            "IsWeaponOrRingEquippable",
        ),
        "fullInventoryYesNo": precommit_call(
            precommit_full_yes_no_call,
            "j_alt_YesNoPrompt",
            "alt_YesNoPrompt",
        ),
        "nonEquippableYesNo": precommit_call(
            precommit_nonequippable_yes_no_call,
            "j_alt_YesNoPrompt",
            "alt_YesNoPrompt",
        ),
        "memberCancelCompareAddress": precommit_member_cancel_compare["address"],
        "memberCancelBranchAddress": precommit_member_cancel_branch["address"],
        "capacityCompareAddress": precommit_capacity_compare["address"],
        "capacityBranchAddress": precommit_capacity_branch["address"],
        "fullInventoryPromptCompareAddress": precommit_full_prompt_compare["address"],
        "fullInventoryRetryBranchAddress": precommit_full_prompt_branch["address"],
        "equipmentTypeCompareAddress": precommit_equipment_type_compare["address"],
        "toolAdmissionBranchAddress": precommit_tool_branch["address"],
        "equippabilityBranchAddress": precommit_equippability_branch["address"],
        "nonEquippablePromptCompareAddress": precommit_nonequippable_prompt_compare["address"],
        "nonEquippableRetryBranchAddress": precommit_nonequippable_retry_branch["address"],
        "presentationTrapAddresses": [
            instruction["address"] for instruction in precommit_text_traps
        ],
        "presentationTrapReturnAddresses": [
            instruction["address"] + len(instruction["bytes"])
            for instruction in precommit_text_traps
        ],
        "frameOffsetsBytes": {
            "clientMember": fulfillment_frame_offsets["clientMember"],
            "itemIndex": fulfillment_frame_offsets["itemIndex"],
            "fulfilledOrdersNumber": fulfillment_frame_offsets["fulfilledOrdersNumber"],
        },
        "h1InstructionBytes": [],
    }
    precommit_service_shim_sources = (
        ("member-list", precommit_member_list_call, precommit["memberList"]),
        ("held-items", precommit_held_items_call, precommit["heldItems"]),
        ("equipment-type", precommit_equipment_type_call, precommit["equipmentType"]),
        ("equippability", precommit_equippability_call, precommit["equippability"]),
    )
    precommit["serviceShims"] = []
    for role, record, service in precommit_service_shim_sources:
        original_hex = record["bytes"].hex().upper()
        if original_hex[:4] != "4EB9" or len(original_hex) != 12:
            raise ValueError("blacksmith precommit service shim original opcode drift")
        precommit["serviceShims"].append(
            {
                "role": role,
                "callAddress": service["callAddress"],
                "instructionTargetAddress": service["instructionTargetAddress"],
                "effectiveTargetAddress": service["effectiveTargetAddress"],
                "returnAddress": service["returnAddress"],
                "originalHex": original_hex,
                "patchedHex": f"4EB9{PRECOMMIT_SERVICE_STUB_ADDRESS:08X}",
                "generatedStubTarget": PRECOMMIT_SERVICE_STUB_ADDRESS,
            }
        )
    precommit["terminalShims"] = [
        {
            "role": "recipient-cancel-terminal-boundary-shim",
            "type": "terminal-jmp",
            "boundaryAddress": recipient_cancel_text_address,
            "originalHex": recipient_cancel_text_span.hex().upper(),
            "patchedHex": f"4EF9{PRECOMMIT_TERMINAL_STUB_ADDRESS:08X}",
            "generatedStubTarget": PRECOMMIT_TERMINAL_STUB_ADDRESS,
        },
        {
            "role": "full-inventory-terminal-boundary-shim",
            "type": "terminal-jmp",
            "boundaryAddress": full_inventory_text_address,
            "originalHex": full_inventory_text_span.hex().upper(),
            "patchedHex": f"4EF9{PRECOMMIT_TERMINAL_STUB_ADDRESS:08X}",
            "generatedStubTarget": PRECOMMIT_TERMINAL_STUB_ADDRESS,
        },
        {
            "role": "non-equippable-terminal-boundary-shim",
            "type": "terminal-jmp",
            "boundaryAddress": non_equippable_text_address,
            "originalHex": non_equippable_text_span.hex().upper(),
            "patchedHex": f"4EF9{PRECOMMIT_TERMINAL_STUB_ADDRESS:08X}",
            "generatedStubTarget": PRECOMMIT_TERMINAL_STUB_ADDRESS,
        },
    ]
    _validate_precommit_instrumentation(precommit)

    fulfillment_start = next(
        index
        for index, instruction in enumerate(fulfill_instructions)
        if instruction["address"] == fulfill_labels["@AddItem"]
    )
    fulfillment_instructions = fulfill_instructions[fulfillment_start:]

    def fulfillment_instruction(text: str, *, occurrence: int = 1) -> dict[str, Any]:
        matches = [item for item in fulfillment_instructions if _h1_text(item) == text]
        if len(matches) < occurrence:
            raise ValueError(f"blacksmith fulfillment H1 instruction missing: {text}")
        return matches[occurrence - 1]

    fulfillment_client_member = fulfillment_instruction("move.w clientMember(a6),d0")
    fulfillment_item_index = fulfillment_instruction("move.w itemIndex(a6),d1")
    add_call_record = fulfillment_instruction("jsr j_AddItem")
    orders_counter_record = fulfillment_instruction("sub.w ordersCounter(a6),d6")
    order_base_record = fulfillment_instruction("lea ((MITHRIL_WEAPONS_ON_ORDER-$1000000)).w,a1")
    order_read_record = fulfillment_instruction("move.w (a1),d2")
    order_clear_record = fulfillment_instruction("move.w #0,(a1)")
    fulfilled_increment_record = fulfillment_instruction("addi.w #1,fulfilledOrdersNumber(a6)")
    fulfilled_increment_after = successor_record(fulfill_instructions, fulfilled_increment_record)
    equippable_call_record = fulfillment_instruction("jsr j_IsWeaponOrRingEquippable", occurrence=1)
    post_equippable_record = successor_record(fulfill_instructions, equippable_call_record)
    fulfillment_block_instructions = [
        instruction
        for instruction in fulfill_instructions
        if fulfill_labels["@AddItem"] <= instruction["address"] <= post_equippable_record["address"]
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
        "itemIndex": h1_frame_displacement(fulfillment_item_index, "fulfillment itemIndex"),
        "ordersCounter": h1_frame_displacement(orders_counter_record, "fulfillment ordersCounter"),
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
        != {name: fulfillment_frame_offsets[name] for name in fulfillment_h1_offsets}
        or order_base_record["address"]
        != orders_counter_record["address"] + len(orders_counter_record["bytes"])
        or order_read_record["address"]
        != order_clear_record["address"] - len(order_read_record["bytes"])
        or fulfilled_increment_after["address"]
        != fulfilled_increment_record["address"] + len(fulfilled_increment_record["bytes"])
        or post_equippable_record["text"]
        not in {
            "bcc.w byte_21CD0 ; @DoNotEquipNewItem",
            "bcc.w byte_21CD0",
        }
        or int.from_bytes(add_call_record["bytes"][-4:], "big") != h1_entries["j_AddItem"]
        or int.from_bytes(equippable_call_record["bytes"][-4:], "big")
        != h1_entries["j_IsWeaponOrRingEquippable"]
        or len(equippable_call_record["bytes"]) != 6
        or equippable_call_record["bytes"][:2] != b"\x4e\xb9"
        or post_equippable_record["address"]
        != equippable_call_record["address"] + len(equippable_call_record["bytes"])
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
        or required["COMBATANT_ITEMSLOTS_COUNTER"] + 1 != required["COMBATANT_ITEMSLOTS"]
        or (required["ITEMTYPE_WEAPON"] | required["ITEMTYPE_RING"]) == 0
    ):
        raise ValueError("blacksmith fulfillment source/H1 ABI chronology drift")
    item_definition_address = h1_entries["table_ItemDefinitions"]
    fulfillment_item_definitions: dict[int, dict[str, int]] = {}
    for item_index in fulfillment_item_indexes:
        definition = item_definitions[item_index]
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
            definition["itemType"] & (required["ITEMTYPE_WEAPON"] | required["ITEMTYPE_RING"])
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

    precommit["h1InstructionBytes"] = guarded_instructions(precommit_instructions, fulfill_labels)
    precommit["cleanupEquippability"] = {
        "callAddress": equippable_call_record["address"],
        "instructionTargetAddress": h1_entries["j_IsWeaponOrRingEquippable"],
        "effectiveTargetAddress": equippable_labels["IsWeaponOrRingEquippable"],
        "effectiveReturnAddress": equippable_return_rts,
        "returnAddress": post_equippable_record["address"],
    }

    decision_instructions = [
        instruction
        for instruction in fulfill_instructions
        if post_equippable_record["address"] <= instruction["address"] <= fulfill_labels["@Done"]
    ]

    def decision_instruction(text: str, *, occurrence: int = 1) -> dict[str, Any]:
        matches = [item for item in decision_instructions if _h1_text(item) == text]
        if len(matches) < occurrence:
            raise ValueError(f"blacksmith equip-decision H1 instruction missing: {text}")
        return matches[occurrence - 1]

    def decision_call(
        record: dict[str, Any], instruction_symbol: str, effective_symbol: str, return_address: int
    ) -> dict[str, int]:
        if (
            len(record["bytes"]) != 6
            or record["bytes"][:2] != b"\x4e\xb9"
            or int.from_bytes(record["bytes"][-4:], "big") != h1_entries[instruction_symbol]
        ):
            raise ValueError("blacksmith equip-decision call target/width drift")
        return {
            "callAddress": record["address"],
            "instructionTargetAddress": h1_entries[instruction_symbol],
            "effectiveTargetAddress": h1_entries[effective_symbol],
            "returnAddress": record["address"] + len(record["bytes"]),
            "effectiveReturnAddress": return_address,
        }

    prompt_call = decision_instruction("jsr j_alt_YesNoPrompt")
    equipment_type_call = decision_instruction("jsr j_GetEquipmentType")
    equipped_weapon_call = decision_instruction("jsr j_GetEquippedWeapon")
    equipped_ring_call = decision_instruction("jsr j_GetEquippedRing")
    unequip_call = decision_instruction("jsr j_UnequipItemBySlotIfNotCursed")
    held_items_call = decision_instruction("jsr j_GetItemBySlotAndHeldItemsNumber")
    equip_call = decision_instruction("jsr j_EquipItemBySlot")
    equipment_type_rts, _ = _h1_instruction(equipment_type_instructions, "rts")
    equipped_item_rts, _ = _h1_instruction(equipped_item_instructions, "rts")
    held_items_rts, _ = _h1_instruction(held_items_instructions, "rts")
    equip_decision = {
        "postEquippabilityBranchAddress": post_equippable_record["address"],
        "prompt": decision_call(
            prompt_call,
            "j_alt_YesNoPrompt",
            "alt_YesNoPrompt",
            prompt_call["address"] + len(prompt_call["bytes"]),
        ),
        "promptCompareAddress": decision_instruction("cmpi.w #0,d0")["address"],
        "promptDeclineBranchAddress": decision_instruction("bne.w byte_21CD0")["address"],
        "equipmentType": decision_call(
            equipment_type_call, "j_GetEquipmentType", "GetEquipmentType", equipment_type_rts
        ),
        "equipmentTypeCompareAddress": decision_instruction("cmpi.w #EQUIPMENTTYPE_WEAPON,d2")[
            "address"
        ],
        "weaponTypeBranchAddress": decision_instruction("bne.s @HasRingEquipped")["address"],
        "getEquippedWeapon": decision_call(
            equipped_weapon_call, "j_GetEquippedWeapon", "GetEquippedWeapon", equipped_item_rts
        ),
        "weaponEquippedCompareAddress": decision_instruction("cmpi.w #-1,d1", occurrence=1)[
            "address"
        ],
        "weaponEmptyBranchAddress": decision_instruction("beq.s @EquipNewItem", occurrence=1)[
            "address"
        ],
        "getEquippedRing": decision_call(
            equipped_ring_call, "j_GetEquippedRing", "GetEquippedRing", equipped_item_rts
        ),
        "ringEquippedCompareAddress": decision_instruction("cmpi.w #-1,d1", occurrence=2)[
            "address"
        ],
        "ringEmptyBranchAddress": decision_instruction("beq.s @EquipNewItem", occurrence=2)[
            "address"
        ],
        "unequip": decision_call(
            unequip_call,
            "j_UnequipItemBySlotIfNotCursed",
            "UnequipItemBySlotIfNotCursed",
            update_return_rts,
        ),
        "weaponUnequipCompareAddress": decision_instruction("cmpi.w #2,d2", occurrence=1)[
            "address"
        ],
        "weaponUnequipBranchAddress": decision_instruction("bne.w @EquipNewItem", occurrence=1)[
            "address"
        ],
        "ringUnequipCompareAddress": decision_instruction("cmpi.w #2,d2", occurrence=2)["address"],
        "ringUnequipBranchAddress": decision_instruction("bne.w @EquipNewItem", occurrence=2)[
            "address"
        ],
        "heldItems": decision_call(
            held_items_call,
            "j_GetItemBySlotAndHeldItemsNumber",
            "GetItemBySlotAndHeldItemsNumber",
            held_items_rts,
        ),
        "equip": decision_call(
            equip_call, "j_EquipItemBySlot", "EquipItemBySlot", update_return_rts
        ),
        "newEquipCursedCompareAddress": decision_instruction("cmpi.w #2,d2", occurrence=3)[
            "address"
        ],
        "newEquipNoncursedBranchAddress": decision_instruction("bne.s byte_21CC8")["address"],
        "currentCursedBoundaryAddress": decision_instruction("M trap #textbox", occurrence=2)[
            "address"
        ],
        "newCursedBoundaryAddress": decision_instruction("M trap #textbox", occurrence=4)[
            "address"
        ],
        "noncursedBoundaryAddress": decision_instruction("M trap #textbox", occurrence=5)[
            "address"
        ],
        "doNotEquipBoundaryAddress": decision_instruction("M trap #textbox", occurrence=6)[
            "address"
        ],
        "doneAddress": fulfill_labels["@Done"],
        "mithrilDomain": {
            "choiceCount": sum(len(row) for row in rows),
            "uniqueItemIndexes": sorted(mithril_item_indexes),
            "uniqueItemCount": len(mithril_item_indexes),
            "weaponItemCount": sum(
                bool(item_definitions[index]["itemType"] & required["ITEMTYPE_WEAPON"])
                for index in mithril_item_indexes
            ),
            "ringItemCount": sum(
                bool(item_definitions[index]["itemType"] & required["ITEMTYPE_RING"])
                for index in mithril_item_indexes
            ),
            "cursedItemCount": sum(
                bool(item_definitions[index]["itemType"] & required["ITEMTYPE_CURSED"])
                for index in mithril_item_indexes
            ),
        },
    }

    def shared_update_tail(role: str, instructions: list[dict[str, Any]]) -> dict[str, Any]:
        matches = [
            instruction
            for instruction in instructions
            if _h1_text(instruction) == "bra.w UpdateCombatantStats"
        ]
        if len(matches) != 1:
            raise ValueError(f"blacksmith equip-decision {role} UpdateCombatantStats tail drift")
        tail = matches[0]
        encoded = tail["bytes"]
        if (
            len(encoded) != 4
            or encoded[:2] != b"\x60\x00"
            or tail["address"] + 2 + int.from_bytes(encoded[2:], "big", signed=True)
            != h1_entries["UpdateCombatantStats"]
        ):
            raise ValueError(f"blacksmith equip-decision {role} UpdateCombatantStats branch drift")
        return {
            "role": role,
            "tailAddress": tail["address"],
            "tailHex": encoded.hex().upper(),
        }

    equip_decision["sharedUpdateEffectiveReturn"] = {
        "address": update_return_rts,
        "targetAddress": h1_entries["UpdateCombatantStats"],
        "services": [
            shared_update_tail("unequip", unequip_instructions),
            shared_update_tail("equip", equip_instructions),
        ],
    }
    shared_update = equip_decision["sharedUpdateEffectiveReturn"]
    if (
        shared_update["address"] != 0x8A24
        or shared_update["targetAddress"] != 0x89CE
        or shared_update["services"]
        != [
            {"role": "unequip", "tailAddress": 0x8DB2, "tailHex": "6000FC1A"},
            {"role": "equip", "tailAddress": 0x8D66, "tailHex": "6000FC66"},
        ]
        or any(
            equip_decision[row["role"]]["effectiveReturnAddress"] != shared_update["address"]
            for row in shared_update["services"]
        )
    ):
        raise ValueError("blacksmith equip-decision shared UpdateCombatantStats return drift")
    if (
        equip_decision["mithrilDomain"] != fixture["equipDecisionDomain"]
        or equip_decision["mithrilDomain"]["choiceCount"] != 32
        or equip_decision["mithrilDomain"]["uniqueItemCount"] != 26
        or equip_decision["mithrilDomain"]["weaponItemCount"] != 26
        or equip_decision["mithrilDomain"]["ringItemCount"] != 0
        or equip_decision["mithrilDomain"]["cursedItemCount"] != 0
    ):
        raise ValueError("blacksmith complete Mithril table/item-definition domain drift")
    if any(
        value["bytes"][0] not in {0x64, 0x66, 0x67}
        for value in (
            post_equippable_record,
            decision_instruction("bne.w byte_21CD0"),
            decision_instruction("bne.s @HasRingEquipped"),
            decision_instruction("beq.s @EquipNewItem", occurrence=1),
            decision_instruction("bne.w @EquipNewItem", occurrence=1),
            decision_instruction("beq.s @EquipNewItem", occurrence=2),
            decision_instruction("bne.w @EquipNewItem", occurrence=2),
            decision_instruction("bne.s byte_21CC8"),
        )
    ) or (
        post_equippable_record["bytes"][0],
        decision_instruction("bne.w byte_21CD0")["bytes"][0],
        decision_instruction("bne.s @HasRingEquipped")["bytes"][0],
    ) != (0x64, 0x66, 0x66):
        raise ValueError("blacksmith equip-decision branch polarity drift")

    def decision_text_boundary(
        record: dict[str, Any], *, text_id: int, name: str
    ) -> tuple[int, bytes]:
        index = decision_instructions.index(record)
        if (
            record["bytes"] != b"\x4e\x45"
            or index + 1 >= len(decision_instructions)
            or decision_instructions[index + 1]["address"] != record["address"] + 2
            or decision_instructions[index + 1]["bytes"] != text_id.to_bytes(2, "big")
        ):
            raise ValueError(f"blacksmith equip-decision {name} terminal text span drift")
        chunks: list[bytes] = []
        for instruction in decision_instructions[index:]:
            chunks.append(_rom_guard_instruction_bytes(instruction, fulfill_labels, h1_entries))
            if sum(len(chunk) for chunk in chunks) >= 6:
                break
        span = b"".join(chunks)[:6]
        if len(span) != 6:
            raise ValueError(f"blacksmith equip-decision {name} terminal ROM span drift")
        return record["address"], span

    current_cursed_address, current_cursed_span = decision_text_boundary(
        decision_instruction("M trap #textbox", occurrence=2), text_id=176, name="current-cursed"
    )
    prompt_text_address, prompt_text_span = decision_text_boundary(
        decision_instruction("M trap #textbox", occurrence=1), text_id=173, name="prompt"
    )
    noncursed_address, noncursed_span = decision_text_boundary(
        decision_instruction("M trap #textbox", occurrence=5), text_id=174, name="noncursed"
    )
    do_not_address, do_not_span = decision_text_boundary(
        decision_instruction("M trap #textbox", occurrence=6), text_id=209, name="do-not-equip"
    )
    if (
        current_cursed_address != equip_decision["currentCursedBoundaryAddress"]
        or prompt_text_address != post_equippable_record["address"] + 4
        or noncursed_address != equip_decision["noncursedBoundaryAddress"]
        or do_not_address != equip_decision["doNotEquipBoundaryAddress"]
    ):
        raise ValueError("blacksmith equip-decision terminal source/H1 boundary drift")
    equip_decision["terminalShims"] = [
        {
            "role": "current-cursed-terminal-boundary-shim",
            "terminal": "current-cursed-pre-presentation",
            "boundaryAddress": current_cursed_address,
            "originalHex": current_cursed_span.hex().upper(),
            "patchedHex": f"4EF9{EQUIP_DECISION_TERMINAL_STUB_ADDRESS:08X}",
            "generatedStubTarget": EQUIP_DECISION_TERMINAL_STUB_ADDRESS,
        },
        {
            "role": "noncursed-terminal-boundary-shim",
            "terminal": "noncursed-equip-pre-presentation",
            "boundaryAddress": noncursed_address,
            "originalHex": noncursed_span.hex().upper(),
            "patchedHex": f"4EF9{EQUIP_DECISION_TERMINAL_STUB_ADDRESS:08X}",
            "generatedStubTarget": EQUIP_DECISION_TERMINAL_STUB_ADDRESS,
        },
        {
            "role": "do-not-equip-terminal-boundary-shim",
            "terminal": "do-not-equip-pre-presentation",
            "boundaryAddress": do_not_address,
            "originalHex": do_not_span.hex().upper(),
            "patchedHex": f"4EF9{EQUIP_DECISION_TERMINAL_STUB_ADDRESS:08X}",
            "generatedStubTarget": EQUIP_DECISION_TERMINAL_STUB_ADDRESS,
        },
    ]
    equip_decision["promptPresentationSkip"] = {
        "boundaryAddress": prompt_text_address,
        "originalHex": prompt_text_span[:4].hex().upper(),
        # 68k word-branch displacement is relative to the extension-word PC:
        # ``bra.w +2`` therefore lands on the preserved prompt JSR at 0x21C24.
        "patchedHex": "60000002",
        "targetAddress": prompt_call["address"],
        "instructionWidthBytes": 4,
        "branchBaseAddress": prompt_text_address + 2,
        "branchDisplacementBytes": 2,
    }
    equip_decision["h1InstructionBytes"] = [
        *guarded_instructions(decision_instructions, fulfill_labels),
        *guarded_instructions(equipment_type_instructions, equipment_type_labels),
        *guarded_instructions(equipped_weapon_instructions, equipped_weapon_labels),
        *guarded_instructions(equipped_ring_instructions, equipped_ring_labels),
        *guarded_instructions(equipped_item_instructions, equipped_item_labels),
        *guarded_instructions(held_items_instructions, held_items_labels),
        *guarded_instructions(unequip_instructions, unequip_labels),
        *guarded_instructions(equip_instructions, equip_labels),
        *guarded_instructions(item_curse_instructions, item_curse_labels),
    ]
    equip_decision["itemDefinitionFields"] = [
        {
            "itemIndex": item_index,
            "equipFlagsAddress": item_definition_start
            + item_index * required["ITEMDEF_SIZE"]
            + required["ITEMDEF_OFFSET_EQUIPFLAGS"],
            "equipFlagsBytes": definition["equipFlags"].to_bytes(4, "big"),
            "itemTypeAddress": item_definition_start
            + item_index * required["ITEMDEF_SIZE"]
            + required["ITEMDEF_OFFSET_TYPE"],
            "itemTypeBytes": bytes([definition["itemType"]]),
        }
        for item_index, definition in sorted(item_definitions.items())
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
            "dialogueNameIndex1Address": required["DIALOGUE_NAME_INDEX_1"],
            "selectedItemIndexAddress": required["SELECTED_ITEM_INDEX"],
            "currentItemSubmenuActionAddress": required["CURRENT_ITEM_SUBMENU_ACTION"],
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
            "cursedTypeMask": required["ITEMTYPE_CURSED"],
            "equippedItemBit": required["ITEMENTRY_BIT_EQUIPPED"],
            "equipmentTypeTool": required["EQUIPMENTTYPE_TOOL"],
            "equipmentTypeWeapon": required["EQUIPMENTTYPE_WEAPON"],
            "equipmentTypeRing": required["EQUIPMENTTYPE_RING"],
            "combatantEntrySizeBytes": required["COMBATANT_DATA_ENTRY_REAL_SIZE"],
            "combatantItemSlotCount": required["COMBATANT_ITEMSLOTS"],
            "combatantClassOffsetBytes": required["COMBATANT_OFFSET_CLASS"],
            "combatantItemsOffsetBytes": required["COMBATANT_OFFSET_ITEMS"],
            "combatantStatusEffectsOffsetBytes": required["COMBATANT_OFFSET_STATUSEFFECTS"],
            "curseStatusMask": required["STATUSEFFECT_CURSE"],
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
            "pendingOrdersIncrementedObserveAddress": pending_increment_after_record["address"],
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
            "addItemReturnAddress": add_call_record["address"] + len(add_call_record["bytes"]),
            "addItemInstructionTargetAddress": h1_entries["j_AddItem"],
            "addItemEffectiveTargetAddress": add_labels["AddItem"],
            "addItemEffectiveReturnAddress": add_return_rts,
            "orderReadInstructionAddress": order_read_record["address"],
            "orderReadObserveAddress": order_clear_record["address"],
            "orderClearAddress": order_clear_record["address"],
            "orderClearedObserveAddress": fulfilled_increment_record["address"],
            "fulfilledOrdersIncrementAddress": fulfilled_increment_record["address"],
            "fulfilledOrdersIncrementedObserveAddress": fulfilled_increment_after["address"],
            "equippabilityCallAddress": equippable_call_record["address"],
            "equippabilityInstructionTargetAddress": h1_entries["j_IsWeaponOrRingEquippable"],
            "equippabilityEffectiveTargetAddress": equippable_labels["IsWeaponOrRingEquippable"],
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
                    "itemTypeAddress": definition["address"] + required["ITEMDEF_OFFSET_TYPE"],
                    "itemTypeBytes": bytes([definition["itemType"]]),
                }
                for item_index, definition in sorted(fulfillment_item_definitions.items())
            ],
        },
        "precommit": precommit,
        "equipDecision": equip_decision,
        "model": {"classGroups": groups, "weaponRows": rows},
        "h1": {
            "instructionBytes": [
                {
                    **instruction,
                    "romBytes": _rom_guard_instruction_bytes(
                        instruction,
                        labels,
                        {
                            "list_MithrilWeaponClasses": h1_entries["list_MithrilWeaponClasses"],
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
        + static["precommit"]["h1InstructionBytes"]
        + static["equipDecision"]["h1InstructionBytes"]
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
    for field in (
        static["fulfillment"]["itemDefinitionFields"]
        + static["equipDecision"]["itemDefinitionFields"]
    ):
        for address_key, bytes_key in (
            ("equipFlagsAddress", "equipFlagsBytes"),
            ("itemTypeAddress", "itemTypeBytes"),
        ):
            address = field[address_key]
            expected = field[bytes_key]
            if rom[address : address + len(expected)] != expected:
                raise ValueError(
                    f"blacksmith fulfillment H1/ROM item-definition guard drift at {address:#x}"
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
    if (items_before[item_slot] & constants["itemIndexMask"]) != constants["mithrilItemIndex"]:
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
    if (
        not fulfillment["ordersCounterMinimum"]
        <= orders_counter
        <= fulfillment["ordersCounterMaximum"]
    ):
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


def _equip_decision_event(role: str, pc: int) -> dict[str, int | str]:
    return {"role": role, "pc": pc}


def _equip_decision_call_events(role: str, service: dict[str, int]) -> list[dict[str, int | str]]:
    return [
        _equip_decision_event(f"equip-decision-{role}-call", service["callAddress"]),
        _equip_decision_event(
            f"equip-decision-{role}-instruction-target", service["instructionTargetAddress"]
        ),
        _equip_decision_event(
            f"equip-decision-{role}-effective-target", service["effectiveTargetAddress"]
        ),
        _equip_decision_event(
            f"equip-decision-{role}-effective-return", service["effectiveReturnAddress"]
        ),
        _equip_decision_event(f"equip-decision-{role}-original-return", service["returnAddress"]),
    ]


def model_equip_decision_case(case: dict[str, Any], static: dict[str, Any]) -> dict[str, Any]:
    """Model the reachable post-``AddItem`` weapon decision without UI claims."""
    constants = static["constants"]
    decision = static["equipDecision"]
    items_before = list(case["clientItemWordsBefore"])
    orders_before = list(case["ordersBefore"])
    selected_order_index = constants["orderSlotCount"] - int(case["ordersCounter"])
    if not 0 <= selected_order_index < constants["orderSlotCount"]:
        raise ValueError("blacksmith equip-decision order counter source domain drift")
    if orders_before[selected_order_index] != case["itemIndex"]:
        raise ValueError("blacksmith equip-decision source order item drift")
    item_write_index = next(
        (
            index
            for index, value in enumerate(items_before)
            if (value & constants["itemIndexMask"]) == constants["itemNothingIndex"]
        ),
        None,
    )
    if item_write_index is None:
        raise ValueError("blacksmith equip-decision requires one original AddItem slot")
    fields = {int(field["itemIndex"]): field for field in decision["itemDefinitionFields"]}
    item = fields.get(int(case["itemIndex"]))
    if item is None:
        raise ValueError("blacksmith equip-decision item definition is unbound")
    item_type = item["itemTypeBytes"][0]
    class_mask = int.from_bytes(item["equipFlagsBytes"], "big")
    carry_set = bool(
        item_type & (constants["weaponTypeMask"] | constants["ringTypeMask"])
        and class_mask & (1 << int(case["recipientClass"]))
    )
    items_after = items_before.copy()
    items_after[item_write_index] = int(case["itemIndex"])
    orders_after = orders_before.copy()
    orders_after[selected_order_index] = 0
    # v4 already retains the direct AddItem-through-equippability chronology.
    # This v5 record begins at the source continuation branch it newly owns.
    chronology: list[dict[str, int | str]] = [
        _equip_decision_event(
            "equip-decision-post-equippability-branch",
            decision["postEquippabilityBranchAddress"],
        )
    ]
    prompt_shown = False
    equipment_type: int | None = None
    existing_slot: int | None = None
    existing_index: int | None = None
    unequip_result: int | None = None
    equip_slot: int | None = None
    equip_result: int | None = None
    status_after = int(case["statusEffectsBefore"])
    if not carry_set:
        terminal, terminal_pc = (
            "do-not-equip-pre-presentation",
            decision["doNotEquipBoundaryAddress"],
        )
    else:
        prompt_shown = True
        chronology.extend(
            [
                _equip_decision_event(
                    "equip-decision-controlled-prompt-call-shim",
                    decision["prompt"]["callAddress"],
                ),
                _equip_decision_event(
                    "equip-decision-generated-prompt-stub", EQUIP_DECISION_PROMPT_STUB_ADDRESS
                ),
                _equip_decision_event(
                    "equip-decision-prompt-original-return",
                    decision["prompt"]["returnAddress"],
                ),
                _equip_decision_event(
                    "equip-decision-prompt-compare", decision["promptCompareAddress"]
                ),
                _equip_decision_event(
                    "equip-decision-prompt-decline-branch",
                    decision["promptDeclineBranchAddress"],
                ),
            ]
        )
        if int(case["promptResult"]) != 0:
            terminal, terminal_pc = (
                "do-not-equip-pre-presentation",
                decision["doNotEquipBoundaryAddress"],
            )
        else:
            equipment_type = constants["equipmentTypeWeapon"]
            chronology.extend(
                _equip_decision_call_events("equipment-type", decision["equipmentType"])
            )
            chronology.extend(
                [
                    _equip_decision_event(
                        "equip-decision-equipment-type-compare",
                        decision["equipmentTypeCompareAddress"],
                    ),
                    _equip_decision_event(
                        "equip-decision-weapon-type-branch", decision["weaponTypeBranchAddress"]
                    ),
                ]
            )
            expected = case["existingEquippedItemIndex"]
            matching = [
                index
                for index, value in enumerate(items_before)
                if value & (1 << constants["equippedItemBit"])
                and (value & constants["itemIndexMask"]) == expected
            ]
            if expected is None:
                matching = []
            if len(matching) > 1:
                raise ValueError("blacksmith equip-decision duplicate equipped weapon input")
            chronology.extend(
                _equip_decision_call_events("get-equipped-weapon", decision["getEquippedWeapon"])
            )
            chronology.extend(
                [
                    _equip_decision_event(
                        "equip-decision-weapon-equipped-compare",
                        decision["weaponEquippedCompareAddress"],
                    ),
                    _equip_decision_event(
                        "equip-decision-weapon-empty-branch", decision["weaponEmptyBranchAddress"]
                    ),
                ]
            )
            if matching:
                existing_slot = matching[0]
                existing_index = int(expected)
                existing = fields.get(existing_index)
                if existing is None:
                    raise ValueError(
                        "blacksmith equip-decision existing item definition is unbound"
                    )
                existing_cursed = bool(existing["itemTypeBytes"][0] & constants["cursedTypeMask"])
                unequip_result = 2 if existing_cursed else 0
                chronology.extend(_equip_decision_call_events("unequip", decision["unequip"]))
                chronology.extend(
                    [
                        _equip_decision_event(
                            "equip-decision-weapon-unequip-compare",
                            decision["weaponUnequipCompareAddress"],
                        ),
                        _equip_decision_event(
                            "equip-decision-weapon-unequip-branch",
                            decision["weaponUnequipBranchAddress"],
                        ),
                    ]
                )
                if existing_cursed:
                    status_after = constants["curseStatusMask"]
                    terminal, terminal_pc = (
                        "current-cursed-pre-presentation",
                        decision["currentCursedBoundaryAddress"],
                    )
                    chronology.extend(
                        [
                            _equip_decision_event(f"equip-decision-{terminal}", terminal_pc),
                            _equip_decision_event(
                                "equip-decision-generated-terminal-stub",
                                EQUIP_DECISION_TERMINAL_STUB_ADDRESS,
                            ),
                        ]
                    )
                    return {
                        "id": case["id"],
                        "clientMember": case["clientMember"],
                        "recipientClass": case["recipientClass"],
                        "itemIndex": case["itemIndex"],
                        "clientItemWordsBefore": items_before,
                        "clientItemWordsAfter": items_after,
                        "itemWriteIndex": item_write_index,
                        "ordersBefore": orders_before,
                        "ordersAfter": orders_after,
                        "ordersCounter": case["ordersCounter"],
                        "selectedOrderIndex": selected_order_index,
                        "fulfilledOrdersBefore": case["fulfilledOrdersBefore"],
                        "fulfilledOrdersAfter": case["fulfilledOrdersBefore"] + 1,
                        "equippableCarrySet": carry_set,
                        "promptResult": case["promptResult"],
                        "promptShown": prompt_shown,
                        "equipmentType": equipment_type,
                        "existingEquippedSlot": existing_slot,
                        "existingEquippedItemIndex": existing_index,
                        "unequipResult": unequip_result,
                        "equipSlot": equip_slot,
                        "equipResult": equip_result,
                        "statusEffectsBefore": case["statusEffectsBefore"],
                        "statusEffectsAfter": status_after,
                        "terminal": terminal,
                        "terminalPc": terminal_pc,
                        "callbackChronology": chronology,
                    }
                items_after[existing_slot] &= ~(1 << constants["equippedItemBit"])
            chronology.extend(_equip_decision_call_events("held-items", decision["heldItems"]))
            equip_slot = item_write_index
            chronology.extend(_equip_decision_call_events("equip", decision["equip"]))
            items_after[equip_slot] |= 1 << constants["equippedItemBit"]
            equip_result = 0
            chronology.extend(
                [
                    _equip_decision_event(
                        "equip-decision-new-equip-cursed-compare",
                        decision["newEquipCursedCompareAddress"],
                    ),
                    _equip_decision_event(
                        "equip-decision-new-equip-noncursed-branch",
                        decision["newEquipNoncursedBranchAddress"],
                    ),
                ]
            )
            terminal, terminal_pc = (
                "noncursed-equip-pre-presentation",
                decision["noncursedBoundaryAddress"],
            )
    chronology.extend(
        [
            _equip_decision_event(f"equip-decision-{terminal}", terminal_pc),
            _equip_decision_event(
                "equip-decision-generated-terminal-stub",
                EQUIP_DECISION_TERMINAL_STUB_ADDRESS,
            ),
        ]
    )
    return {
        "id": case["id"],
        "clientMember": case["clientMember"],
        "recipientClass": case["recipientClass"],
        "itemIndex": case["itemIndex"],
        "clientItemWordsBefore": items_before,
        "clientItemWordsAfter": items_after,
        "itemWriteIndex": item_write_index,
        "ordersBefore": orders_before,
        "ordersAfter": orders_after,
        "ordersCounter": case["ordersCounter"],
        "selectedOrderIndex": selected_order_index,
        "fulfilledOrdersBefore": case["fulfilledOrdersBefore"],
        "fulfilledOrdersAfter": case["fulfilledOrdersBefore"] + 1,
        "equippableCarrySet": carry_set,
        "promptResult": case["promptResult"],
        "promptShown": prompt_shown,
        "equipmentType": equipment_type,
        "existingEquippedSlot": existing_slot,
        "existingEquippedItemIndex": existing_index,
        "unequipResult": unequip_result,
        "equipSlot": equip_slot,
        "equipResult": equip_result,
        "statusEffectsBefore": case["statusEffectsBefore"],
        "statusEffectsAfter": status_after,
        "terminal": terminal,
        "terminalPc": terminal_pc,
        "callbackChronology": chronology,
    }


def _precommit_event(role: str, pc: int) -> dict[str, int | str]:
    return {"role": role, "pc": pc}


def model_precommit_case(
    case: dict[str, Any], static: dict[str, Any], fulfillment_cases: list[dict[str, Any]]
) -> dict[str, Any]:
    """Derive direct-handler admission chronology from source-bound branch polarity.

    The member-list, item-count, equipment-type, and equippability values are
    deliberately supplied harness controls. Routes that reach presentation or
    ``@AddItem`` stop at their source boundary before any precommit observation
    can include its body. The runtime then reuses a retained direct-fulfillment
    case solely as harness cleanup. This model therefore proves only the
    handler-local selection/capacity/equipment admission branches.
    """
    constants = static["constants"]
    precommit = static["precommit"]
    attempts = list(case["attempts"])
    if len(attempts) != 1:
        raise ValueError("blacksmith precommit case must stop before any prompt retry")
    if len(case["ordersBefore"]) != constants["orderSlotCount"]:
        raise ValueError("blacksmith precommit order-word domain drift")
    attempt = attempts[0]
    terminal_shims = {row["role"]: row for row in precommit["terminalShims"]}
    fulfillment_by_id = {row["id"]: row for row in fulfillment_cases}

    def service_chronology(role: str, service: dict[str, Any]) -> None:
        chronology.extend(
            (
                _precommit_event(
                    f"precommit-{role}-controlled-service-call-shim",
                    service["callAddress"],
                ),
                _precommit_event(
                    "precommit-generated-service-stub", PRECOMMIT_SERVICE_STUB_ADDRESS
                ),
                _precommit_event(f"precommit-{role}-original-return", service["returnAddress"]),
            )
        )

    def terminal_boundary(role: str, terminal: str) -> tuple[str, int]:
        shim = terminal_shims.get(role)
        if shim is None:
            raise ValueError(f"blacksmith precommit terminal shim missing: {role}")
        chronology.append(_precommit_event(f"precommit-{role}", shim["boundaryAddress"]))
        chronology.append(
            _precommit_event("precommit-generated-result-stub", PRECOMMIT_TERMINAL_STUB_ADDRESS)
        )
        return terminal, shim["boundaryAddress"]

    def add_item_boundary() -> tuple[str, int]:
        cleanup_id = case["cleanupFulfillmentCaseId"]
        cleanup = fulfillment_by_id.get(cleanup_id)
        if cleanup is None:
            raise ValueError("blacksmith precommit add-item cleanup fixture identity drift")
        if cleanup["itemIndex"] != case["itemIndex"]:
            raise ValueError("blacksmith precommit add-item cleanup item identity drift")
        selected_order = constants["orderSlotCount"] - cleanup["ordersCounter"]
        if case["ordersBefore"][selected_order] != case["itemIndex"]:
            raise ValueError("blacksmith precommit add-item cleanup order-word identity drift")
        chronology.append(
            _precommit_event("precommit-add-item-boundary", precommit["addItemEntryAddress"])
        )
        return "add-item", precommit["addItemEntryAddress"]

    chronology = [
        _precommit_event("precommit-selection-loop-entry", precommit["runtimeStartAddress"])
    ]
    service_chronology("member-list", precommit["memberList"])
    chronology.extend(
        (
            _precommit_event(
                "precommit-member-cancel-compare", precommit["memberCancelCompareAddress"]
            ),
            _precommit_event(
                "precommit-member-cancel-branch", precommit["memberCancelBranchAddress"]
            ),
        )
    )
    member = attempt["selectedMemberResult"]
    if member == -1:
        terminal, terminal_pc = terminal_boundary(
            "recipient-cancel-terminal-boundary-shim", "recipient-cancel-pre-presentation"
        )
        selected_member = None
    else:
        if not 0 <= member <= 31:
            raise ValueError("blacksmith precommit selected member is outside byte domain")
        selected_member = member
        held_items = attempt["heldItemsCountResult"]
        if held_items is None:
            raise ValueError("blacksmith precommit selected member lacks held-item result")
        service_chronology("held-items", precommit["heldItems"])
        chronology.extend(
            (
                _precommit_event("precommit-capacity-compare", precommit["capacityCompareAddress"]),
                _precommit_event("precommit-capacity-branch", precommit["capacityBranchAddress"]),
            )
        )
        if held_items >= constants["combatantItemSlotCount"]:
            terminal, terminal_pc = terminal_boundary(
                "full-inventory-terminal-boundary-shim", "full-inventory-pre-presentation"
            )
        else:
            equipment_type = attempt["equipmentTypeResult"]
            if equipment_type is None:
                raise ValueError("blacksmith precommit non-full selection lacks equipment type")
            service_chronology("equipment-type", precommit["equipmentType"])
            chronology.extend(
                (
                    _precommit_event(
                        "precommit-equipment-type-compare",
                        precommit["equipmentTypeCompareAddress"],
                    ),
                    _precommit_event(
                        "precommit-tool-admission-branch", precommit["toolAdmissionBranchAddress"]
                    ),
                )
            )
            if equipment_type == constants["equipmentTypeTool"]:
                terminal, terminal_pc = add_item_boundary()
            else:
                carry = attempt["equippableCarrySetResult"]
                if carry is None:
                    raise ValueError(
                        "blacksmith precommit non-tool selection lacks equippability carry"
                    )
                service_chronology("equippability", precommit["equippability"])
                chronology.append(
                    _precommit_event(
                        "precommit-equippability-branch",
                        precommit["equippabilityBranchAddress"],
                    )
                )
                if carry:
                    terminal, terminal_pc = add_item_boundary()
                else:
                    terminal, terminal_pc = terminal_boundary(
                        "non-equippable-terminal-boundary-shim",
                        "non-equippable-pre-presentation",
                    )
    return {
        "id": case["id"],
        "itemIndex": case["itemIndex"],
        "attemptCount": 1,
        "selectedMember": selected_member,
        "ordersBefore": list(case["ordersBefore"]),
        "ordersAfter": list(case["ordersBefore"]),
        "fulfilledOrdersBefore": case["fulfilledOrdersBefore"],
        "fulfilledOrdersAfter": case["fulfilledOrdersBefore"],
        "terminal": terminal,
        "terminalPc": terminal_pc,
        "addItemMutationObserved": False,
        "orderMutationObserved": False,
        "fulfilledOrdersMutationObserved": False,
        "callbackChronology": chronology,
    }


def expected_observation(fixture: dict[str, Any], static: dict[str, Any]) -> dict[str, Any]:
    helper_records = [model_case(case, static) for case in fixture["cases"]]
    transaction_records = [
        model_transaction_case(case, static) for case in fixture["transactionCases"]
    ]
    fulfillment_records = [
        model_fulfillment_case(case, static) for case in fixture["fulfillmentCases"]
    ]
    precommit_records = [
        model_precommit_case(case, static, fixture["fulfillmentCases"])
        for case in fixture["precommitCases"]
    ]
    equip_decision_records = [
        model_equip_decision_case(case, static) for case in fixture["equipDecisionCases"]
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
        "precommitCaseOrder": [case["id"] for case in fixture["precommitCases"]],
        "precommitRecords": precommit_records,
        "equipDecisionCaseOrder": [case["id"] for case in fixture["equipDecisionCases"]],
        "equipDecisionRecords": equip_decision_records,
        "callbacksCleared": 0,
        "precommitInstrumentation": {
            "serviceCallSitesReadback": [
                "member-list",
                "held-items",
                "equipment-type",
                "equippability",
            ],
            "terminalBoundarySitesReadback": [
                "recipient-cancel-terminal-boundary-shim",
                "full-inventory-terminal-boundary-shim",
                "non-equippable-terminal-boundary-shim",
            ],
            "generatedServiceStubWritesReadback": True,
            "generatedResultStubWritesReadback": True,
        },
        "restoration": {
            "currentGoldLongRestored": True,
            "randomSeedWordRestored": True,
            "orderWordsRestored": True,
            "flag80OwningByteRestored": True,
            "clientCombatantRecordsRestored": True,
        },
        "precommitRestoration": {
            "dialogueNameIndex1WordRestored": True,
            "selectedItemIndexWordRestored": True,
            "currentItemSubmenuActionByteRestored": True,
        },
        "equipDecisionInstrumentation": {
            "promptCallSiteReadback": True,
            "promptPresentationSkipReadback": True,
            "generatedPromptStubWriteReadback": True,
            "terminalBoundarySitesReadback": [
                "current-cursed-terminal-boundary-shim",
                "noncursed-terminal-boundary-shim",
                "do-not-equip-terminal-boundary-shim",
            ],
            "generatedTerminalStubWriteReadback": True,
        },
    }


def _validate_case_matrix(fixture: dict[str, Any], static: dict[str, Any]) -> None:
    if (
        tuple(case["id"] for case in fixture["cases"]) != CASE_IDS
        or tuple(fixture["caseOrder"]) != CASE_IDS
    ):
        raise ValueError("blacksmith fixture case order drift")
    records = [model_case(case, static) for case in fixture["cases"]]
    fallback_records = [
        record for record in records if record["rngCalls"][0]["role"] == "fallback-row-roll"
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
    if (
        tuple(fixture["transactionCaseOrder"]) != TRANSACTION_CASE_IDS
        or tuple(case["id"] for case in fixture["transactionCases"]) != TRANSACTION_CASE_IDS
    ):
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
        record["safeExitOriginalReturnPc"] != static["transaction"]["prePresentationReturnAddress"]
        for record in records
    ):
        raise ValueError("blacksmith transaction pre-presentation exit boundary drift")


def _validate_fulfillment_case_matrix(fixture: dict[str, Any], static: dict[str, Any]) -> None:
    if (
        tuple(fixture["fulfillmentCaseOrder"]) != FULFILLMENT_CASE_IDS
        or tuple(case["id"] for case in fixture["fulfillmentCases"]) != FULFILLMENT_CASE_IDS
    ):
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


def _validate_precommit_case_matrix(fixture: dict[str, Any], static: dict[str, Any]) -> None:
    if (
        tuple(fixture["precommitCaseOrder"]) != PRECOMMIT_CASE_IDS
        or tuple(case["id"] for case in fixture["precommitCases"]) != PRECOMMIT_CASE_IDS
    ):
        raise ValueError("blacksmith precommit case order drift")
    all_existing_ids = {
        *CASE_IDS,
        *TRANSACTION_CASE_IDS,
        *FULFILLMENT_CASE_IDS,
    }
    if all_existing_ids & set(PRECOMMIT_CASE_IDS):
        raise ValueError("blacksmith precommit duplicates accepted v3 case ownership")
    records = [
        model_precommit_case(case, static, fixture["fulfillmentCases"])
        for case in fixture["precommitCases"]
    ]
    if [record["terminal"] for record in records] != [
        "recipient-cancel-pre-presentation",
        "full-inventory-pre-presentation",
        "add-item",
        "add-item",
        "non-equippable-pre-presentation",
    ]:
        raise ValueError("blacksmith precommit terminal matrix coverage drift")
    if [record["attemptCount"] for record in records] != [1, 1, 1, 1, 1]:
        raise ValueError("blacksmith precommit pre-presentation boundary coverage drift")
    if [record["selectedMember"] for record in records] != [
        None,
        0,
        3,
        4,
        5,
    ]:
        raise ValueError("blacksmith precommit selected-member coverage drift")
    if any(
        record["ordersBefore"] != record["ordersAfter"]
        or record["fulfilledOrdersBefore"] != record["fulfilledOrdersAfter"]
        or record["addItemMutationObserved"]
        or record["orderMutationObserved"]
        or record["fulfilledOrdersMutationObserved"]
        for record in records
    ):
        raise ValueError("blacksmith precommit stop-before-mutation boundary drift")
    if any(
        event["role"].endswith("terminal-boundary-shim")
        for record in records
        for event in record["callbackChronology"][:-2]
    ):
        raise ValueError("blacksmith precommit terminal shim must be terminal-only")
    if any(
        not record["callbackChronology"][-2]["role"].endswith("terminal-boundary-shim")
        or record["callbackChronology"][-1]["role"] != "precommit-generated-result-stub"
        for record in records
        if record["terminal"] != "add-item"
    ):
        raise ValueError("blacksmith precommit terminal shim missing")
    if any(
        record["callbackChronology"][-1]["role"] != "precommit-add-item-boundary"
        for record in records
        if record["terminal"] == "add-item"
    ):
        raise ValueError("blacksmith precommit add-item boundary missing")


def _validate_equip_decision_case_matrix(
    fixture: dict[str, Any], static: dict[str, Any] | None = None
) -> None:
    """Lock the five reachable Mithril-output decisions independently of golden JSON.

    The root case order is launch input, not merely output metadata.  The
    optional ``static`` stage lets the verifier reject an ID/order drift before
    any ROM setup, then derives terminals, controlled-prompt results, and the
    shared ``UpdateCombatantStats`` return sequence before launch.
    """
    if (
        tuple(fixture.get("equipDecisionCaseOrder", ())) != EQUIP_DECISION_CASE_IDS
        or tuple(case.get("id") for case in fixture.get("equipDecisionCases", ()))
        != EQUIP_DECISION_CASE_IDS
    ):
        raise ValueError("blacksmith equip-decision case order drift")
    if static is None:
        return

    records = [model_equip_decision_case(case, static) for case in fixture["equipDecisionCases"]]
    if tuple(record["id"] for record in records) != EQUIP_DECISION_CASE_IDS:
        raise ValueError("blacksmith equip-decision model case identity drift")
    if [record["terminal"] for record in records] != [
        "do-not-equip-pre-presentation",
        "do-not-equip-pre-presentation",
        "noncursed-equip-pre-presentation",
        "noncursed-equip-pre-presentation",
        "current-cursed-pre-presentation",
    ]:
        raise ValueError("blacksmith equip-decision terminal matrix coverage drift")
    if [(record["promptShown"], record["promptResult"]) for record in records] != [
        (False, None),
        (True, -1),
        (True, 0),
        (True, 0),
        (True, 0),
    ]:
        raise ValueError("blacksmith equip-decision prompt matrix coverage drift")

    shared_return = static["equipDecision"]["sharedUpdateEffectiveReturn"]["address"]
    shared_roles = [
        [event["role"] for event in record["callbackChronology"] if event["pc"] == shared_return]
        for record in records
    ]
    if shared_roles != [
        [],
        [],
        ["equip-decision-equip-effective-return"],
        [
            "equip-decision-unequip-effective-return",
            "equip-decision-equip-effective-return",
        ],
        ["equip-decision-unequip-effective-return"],
    ]:
        raise ValueError("blacksmith equip-decision shared-return matrix coverage drift")
    cursed = records[-1]
    if (
        cursed["equipSlot"],
        cursed["equipResult"],
        cursed["statusEffectsBefore"],
        cursed["statusEffectsAfter"],
    ) != (None, None, 0, 4):
        raise ValueError("blacksmith equip-decision cursed-stop matrix coverage drift")


def _assert_golden(fixture: dict[str, Any], static: dict[str, Any]) -> dict[str, Any]:
    _validate_case_matrix(fixture, static)
    _validate_transaction_case_matrix(fixture, static)
    _validate_fulfillment_case_matrix(fixture, static)
    _validate_precommit_case_matrix(fixture, static)
    _validate_equip_decision_case_matrix(fixture, static)
    expected = expected_observation(fixture, static)
    accepted = fixture["acceptedObservation"]
    if accepted != expected:
        raise ValueError("blacksmith accepted observation disagrees with independent model")
    _assert_retained_v4_digest(fixture)
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
    required_milestones = (
        "milestone:direct-function-probe",
        "milestone:first-case-entered",
        "milestone:transaction-cases-entered",
        "milestone:fulfillment-cases-entered",
        "milestone:precommit-cases-entered",
        "milestone:equip-decision-transition-armed",
        "milestone:equip-decision-cases-entered",
        "milestone:transaction-state-restored",
    )
    assert_observer_status(
        status_path,
        owner=OWNER,
        schema_path=FAILURE_SCHEMA,
        required_milestones=required_milestones,
    )
    lines = status_path.read_text(encoding="utf-8").splitlines()
    positions = []
    for milestone in required_milestones:
        if lines.count(milestone) != 1:
            raise RuntimeError(
                f"{OWNER} observer required milestone multiplicity drift: {milestone}"
            )
        positions.append(lines.index(milestone))
    if positions != sorted(positions):
        raise RuntimeError(f"{OWNER} observer required milestone order drift")


def _validate_precommit_instrumentation(precommit: dict[str, Any]) -> None:
    expected_roles = ("member-list", "held-items", "equipment-type", "equippability")
    shims = precommit.get("serviceShims")
    if not isinstance(shims, list) or tuple(shim.get("role") for shim in shims) != expected_roles:
        raise ValueError("blacksmith precommit service shim role/order drift")
    call_ranges: list[range] = []
    for role, shim in zip(expected_roles, shims, strict=True):
        service_name = {
            "member-list": "memberList",
            "held-items": "heldItems",
            "equipment-type": "equipmentType",
            "equippability": "equippability",
        }[role]
        service = precommit[service_name]
        if (
            shim["callAddress"] != service["callAddress"]
            or shim["instructionTargetAddress"] != service["instructionTargetAddress"]
            or shim["effectiveTargetAddress"] != service["effectiveTargetAddress"]
            or shim["returnAddress"] != service["returnAddress"]
            or shim["generatedStubTarget"] != PRECOMMIT_SERVICE_STUB_ADDRESS
            or shim["patchedHex"] != f"4EB9{PRECOMMIT_SERVICE_STUB_ADDRESS:08X}"
            or shim["originalHex"] != f"4EB9{service['instructionTargetAddress']:08X}"
        ):
            raise ValueError(f"blacksmith precommit service shim ABI drift: {role}")
        call_range = range(shim["callAddress"], shim["callAddress"] + 6)
        if any(
            call_range.start < existing.stop and existing.start < call_range.stop
            for existing in call_ranges
        ):
            raise ValueError("blacksmith precommit service shim overlapping call-site drift")
        call_ranges.append(call_range)
    terminal_roles = (
        "recipient-cancel-terminal-boundary-shim",
        "full-inventory-terminal-boundary-shim",
        "non-equippable-terminal-boundary-shim",
    )
    terminals = precommit.get("terminalShims")
    if (
        not isinstance(terminals, list)
        or tuple(row.get("role") for row in terminals) != terminal_roles
    ):
        raise ValueError("blacksmith precommit terminal shim role/order drift")
    all_ranges = call_ranges.copy()
    for role, terminal in zip(terminal_roles, terminals, strict=True):
        original = terminal.get("originalHex")
        patched = terminal.get("patchedHex")
        if (
            terminal.get("type") != "terminal-jmp"
            or terminal.get("generatedStubTarget") != PRECOMMIT_TERMINAL_STUB_ADDRESS
            or patched != f"4EF9{PRECOMMIT_TERMINAL_STUB_ADDRESS:08X}"
            or not isinstance(original, str)
            or len(original) != 12
        ):
            raise ValueError(f"blacksmith precommit terminal shim ABI drift: {role}")
        boundary_address = terminal.get("boundaryAddress")
        if not isinstance(boundary_address, int):
            raise ValueError(f"blacksmith precommit terminal shim address drift: {role}")
        terminal_range = range(boundary_address, boundary_address + 6)
        if any(
            terminal_range.start < existing.stop and existing.start < terminal_range.stop
            for existing in all_ranges
        ):
            raise ValueError("blacksmith precommit instrumentation overlapping span drift")
        all_ranges.append(terminal_range)
    if len(all_ranges) != 7:
        raise ValueError("blacksmith precommit instrumentation span count drift")


def _retained_blacksmith_observation_pcs(static: dict[str, Any]) -> set[int]:
    """Return all retained v3 helper/transaction/fulfillment observation PCs."""

    def collect(value: Any) -> set[int]:
        if isinstance(value, int):
            return {value}
        if isinstance(value, dict):
            return set().union(*(collect(item) for item in value.values()))
        if isinstance(value, list):
            return set().union(*(collect(item) for item in value))
        return set()

    return set().union(
        collect(static["function"]),
        collect(static["transaction"]),
        collect(static["fulfillment"]),
    )


def _validate_precommit_retained_compatibility(
    static: dict[str, Any], spans: list[dict[str, Any]]
) -> None:
    """Reject session-ROM instrumentation overlapping accepted v3 observation PCs."""

    retained_pcs = _retained_blacksmith_observation_pcs(static)
    overlapping = [
        row["role"]
        for row in spans
        if any(
            pc in range(row["address"], row["address"] + len(row["originalBytes"]))
            for pc in retained_pcs
        )
    ]
    if overlapping:
        raise ValueError(
            "blacksmith precommit instrumentation overlaps retained v3 observation PCs: "
            + ", ".join(overlapping)
        )
    if static["precommit"]["addItemEntryAddress"] not in retained_pcs:
        raise ValueError("blacksmith precommit AddItem retained-v3 compatibility inventory drift")


def _validate_precommit_cleanup_equippability(static: dict[str, Any]) -> None:
    """Bind cleanup to the direct ``@AddItem`` call, never the admission seam."""
    precommit = static["precommit"]
    cleanup = precommit.get("cleanupEquippability")
    fulfillment = static["fulfillment"]
    if not isinstance(cleanup, dict):
        raise ValueError("blacksmith precommit cleanup equippability use-site is missing")
    if cleanup.get("callAddress") == precommit["equippability"]["callAddress"]:
        raise ValueError("blacksmith precommit cleanup reuses admission equippability call")
    expected = {
        "callAddress": fulfillment["equippabilityCallAddress"],
        "instructionTargetAddress": fulfillment["equippabilityInstructionTargetAddress"],
        "effectiveTargetAddress": fulfillment["equippabilityEffectiveTargetAddress"],
        "effectiveReturnAddress": fulfillment["equippabilityEffectiveReturnAddress"],
        "returnAddress": fulfillment["postEquippabilityReturnAddress"],
    }
    if cleanup != expected:
        raise ValueError("blacksmith precommit cleanup equippability source/H1/ROM relation drift")
    if cleanup["returnAddress"] != cleanup["callAddress"] + 6:
        raise ValueError("blacksmith precommit cleanup equippability return relation drift")


def _precommit_instrumentation_spans(static: dict[str, Any]) -> list[dict[str, Any]]:
    """Normalize the seven source-bound session-ROM patch spans."""
    precommit = static["precommit"]
    _validate_precommit_instrumentation(precommit)
    result: list[dict[str, Any]] = []
    for shim in precommit["serviceShims"]:
        original = bytes.fromhex(shim["originalHex"])
        patched = bytes.fromhex(shim["patchedHex"])
        if (
            len(original) != PRECOMMIT_SERVICE_STUB_SIZE
            or len(patched) != PRECOMMIT_SERVICE_STUB_SIZE
            or patched != b"\x4e\xb9" + shim["generatedStubTarget"].to_bytes(4, "big")
            or shim["returnAddress"] != shim["callAddress"] + 6
        ):
            raise ValueError(f"blacksmith precommit service JSR patch shape drift: {shim['role']}")
        result.append(
            {
                "role": shim["role"],
                "type": "service-jsr",
                "address": shim["callAddress"],
                "originalBytes": original,
                "patchedBytes": patched,
                "generatedStubTarget": shim["generatedStubTarget"],
                "returnAddress": shim["returnAddress"],
            }
        )
    for shim in precommit["terminalShims"]:
        original = bytes.fromhex(shim["originalHex"])
        patched = bytes.fromhex(shim["patchedHex"])
        if (
            len(original) != PRECOMMIT_TERMINAL_STUB_SIZE
            or len(patched) != PRECOMMIT_TERMINAL_STUB_SIZE
            or patched != b"\x4e\xf9" + shim["generatedStubTarget"].to_bytes(4, "big")
        ):
            raise ValueError(f"blacksmith precommit terminal JMP patch shape drift: {shim['role']}")
        result.append(
            {
                "role": shim["role"],
                "type": "terminal-jmp",
                "address": shim["boundaryAddress"],
                "originalBytes": original,
                "patchedBytes": patched,
                "generatedStubTarget": shim["generatedStubTarget"],
            }
        )
    ranges = [range(row["address"], row["address"] + 6) for row in result]
    if len(result) != 7 or any(
        left.start < right.stop and right.start < left.stop
        for index, left in enumerate(ranges)
        for right in ranges[index + 1 :]
    ):
        raise ValueError("blacksmith precommit session-ROM patch span overlap drift")
    _validate_precommit_retained_compatibility(static, result)
    return result


def _equip_decision_instrumentation_spans(static: dict[str, Any]) -> list[dict[str, Any]]:
    """Bind controlled prompt and neutral terminal stops to a session copy."""
    prompt = static["equipDecision"]["prompt"]
    original = b"\x4e\xb9" + prompt["instructionTargetAddress"].to_bytes(4, "big")
    patched = b"\x4e\xb9" + EQUIP_DECISION_PROMPT_STUB_ADDRESS.to_bytes(4, "big")
    decision = static["equipDecision"]
    if (
        prompt["returnAddress"] != prompt["callAddress"] + EQUIP_DECISION_PROMPT_STUB_SIZE
        or prompt["instructionTargetAddress"] == EQUIP_DECISION_PROMPT_STUB_ADDRESS
        or len(original) != EQUIP_DECISION_PROMPT_STUB_SIZE
        or prompt["callAddress"] != 0x21C24
        or prompt["instructionTargetAddress"] != 0x10074
        or prompt["effectiveTargetAddress"] != 0x1528C
        or prompt["returnAddress"] != 0x21C2A
    ):
        raise ValueError("blacksmith equip-decision prompt instrumentation ABI drift")
    result = [
        {
            "role": "equip-decision-prompt",
            "type": "prompt-jsr",
            "address": prompt["callAddress"],
            "originalBytes": original,
            "patchedBytes": patched,
            "generatedStubTarget": EQUIP_DECISION_PROMPT_STUB_ADDRESS,
            "returnAddress": prompt["returnAddress"],
        }
    ]
    skip = decision["promptPresentationSkip"]
    original_skip = bytes.fromhex(skip["originalHex"])
    patched_skip = bytes.fromhex(skip["patchedHex"])
    if (
        len(original_skip) != 4
        or original_skip != b"\x4e\x45\x00\xad"
        or patched_skip != b"\x60\x00\x00\x02"
        or skip["instructionWidthBytes"] != len(patched_skip)
        or skip["branchBaseAddress"] != skip["boundaryAddress"] + 2
        or skip["branchDisplacementBytes"] != int.from_bytes(patched_skip[2:], "big")
        or skip["targetAddress"] != static["equipDecision"]["prompt"]["callAddress"]
        or skip["targetAddress"] != skip["branchBaseAddress"] + skip["branchDisplacementBytes"]
    ):
        raise ValueError("blacksmith equip-decision prompt presentation skip ABI drift")
    result.append(
        {
            "role": "equip-decision-prompt-presentation-skip",
            "type": "prompt-text-bra",
            "address": skip["boundaryAddress"],
            "originalBytes": original_skip,
            "patchedBytes": patched_skip,
            "targetAddress": skip["targetAddress"],
        }
    )
    terminal_roles = (
        "current-cursed-terminal-boundary-shim",
        "noncursed-terminal-boundary-shim",
        "do-not-equip-terminal-boundary-shim",
    )
    expected_terminals = (
        ("current-cursed-pre-presentation", 0x21C68, "4E4500B06062"),
        ("noncursed-equip-pre-presentation", 0x21CC8, "4E4500AE6000"),
        ("do-not-equip-pre-presentation", 0x21CD0, "4E4500D14CDF"),
    )
    shims = decision.get("terminalShims")
    if not isinstance(shims, list) or tuple(shim.get("role") for shim in shims) != terminal_roles:
        raise ValueError("blacksmith equip-decision terminal shim role/order drift")
    for shim, (terminal, address, original_hex) in zip(shims, expected_terminals, strict=True):
        original = bytes.fromhex(shim["originalHex"])
        patched = bytes.fromhex(shim["patchedHex"])
        if (
            shim["terminal"] != terminal
            or shim["boundaryAddress"] != address
            or shim["originalHex"] != original_hex
            or shim["generatedStubTarget"] != EQUIP_DECISION_TERMINAL_STUB_ADDRESS
            or len(original) != EQUIP_DECISION_TERMINAL_STUB_SIZE
            or len(patched) != EQUIP_DECISION_TERMINAL_STUB_SIZE
            or patched != b"\x4e\xf9" + EQUIP_DECISION_TERMINAL_STUB_ADDRESS.to_bytes(4, "big")
        ):
            raise ValueError("blacksmith equip-decision terminal instrumentation ABI drift")
        result.append(
            {
                "role": shim["role"],
                "type": "terminal-jmp",
                "address": shim["boundaryAddress"],
                "originalBytes": original,
                "patchedBytes": patched,
                "generatedStubTarget": shim["generatedStubTarget"],
                "terminal": shim["terminal"],
            }
        )
    if len(result) != 5:
        raise ValueError("blacksmith equip-decision instrumentation span count drift")
    return result


def _validate_session_instrumentation_spans(
    static: dict[str, Any], spans: list[dict[str, Any]]
) -> None:
    """Reject any mixed-width session-patch overlap before a disposable ROM is written."""
    ranges = [range(row["address"], row["address"] + len(row["originalBytes"])) for row in spans]
    if len(spans) != 12 or any(
        left.start < right.stop and right.start < left.stop
        for index, left in enumerate(ranges)
        for right in ranges[index + 1 :]
    ):
        raise ValueError("blacksmith session instrumentation span overlap/count drift")
    _validate_precommit_retained_compatibility(static, spans)


def _session_instrumentation_spans(static: dict[str, Any]) -> list[dict[str, Any]]:
    """Prove all v5 session patches are distinct and retain v4 observation PCs."""
    spans = [
        *_precommit_instrumentation_spans(static),
        *_equip_decision_instrumentation_spans(static),
    ]
    _validate_session_instrumentation_spans(static, spans)
    return spans


def _session_instrumentation_config(static: dict[str, Any]) -> dict[str, Any]:
    """Serialize every mixed-width disposable-ROM span for Lua readback."""
    spans = _session_instrumentation_spans(static)
    plan = {
        "spanCount": len(spans),
        "spans": [
            {
                "role": row["role"],
                "type": row["type"],
                "address": row["address"],
                "widthBytes": len(row["patchedBytes"]),
                "originalHex": row["originalBytes"].hex().upper(),
                "patchedHex": row["patchedBytes"].hex().upper(),
            }
            for row in spans
        ],
    }
    _validate_session_instrumentation_config(static, plan)
    return plan


def _validate_session_instrumentation_config(static: dict[str, Any], plan: dict[str, Any]) -> None:
    """Reject a Lua plan that omits, widens, or changes any session-ROM span."""
    spans = _session_instrumentation_spans(static)
    expected_rows = [
        {
            "role": row["role"],
            "type": row["type"],
            "address": row["address"],
            "widthBytes": len(row["patchedBytes"]),
            "originalHex": row["originalBytes"].hex().upper(),
            "patchedHex": row["patchedBytes"].hex().upper(),
        }
        for row in spans
    ]
    if plan.get("spanCount") != len(expected_rows) or plan.get("spans") != expected_rows:
        raise ValueError("blacksmith session-ROM Lua readback plan drift")


def _validate_precommit_instrumented_copy(
    original: bytes, instrumented: bytes, spans: list[dict[str, Any]]
) -> None:
    """Prove the disposable copy differs only at the declared mixed-width spans."""
    if len(original) != len(instrumented):
        raise ValueError("blacksmith precommit instrumented ROM size drift")
    expected_addresses = {row["address"] for row in spans}
    observed_addresses = {
        row["address"]
        for row in spans
        if original[row["address"] : row["address"] + len(row["originalBytes"])]
        != instrumented[row["address"] : row["address"] + len(row["patchedBytes"])]
    }
    if observed_addresses != expected_addresses:
        raise ValueError("blacksmith precommit instrumented ROM span-set drift")
    expected_changed = {
        row["address"] + offset
        for row in spans
        for offset, (before, after) in enumerate(
            zip(row["originalBytes"], row["patchedBytes"], strict=True)
        )
        if before != after
    }
    changed = {
        address
        for address, (before, after) in enumerate(zip(original, instrumented, strict=True))
        if before != after
    }
    if changed != expected_changed:
        raise ValueError("blacksmith precommit instrumented ROM exact byte-diff drift")
    for row in spans:
        address = row["address"]
        if instrumented[address : address + len(row["patchedBytes"])] != row["patchedBytes"]:
            raise ValueError(f"blacksmith precommit instrumented ROM patch drift: {row['role']}")


def _instrument_precommit_rom(
    rom_path: Path,
    fixture: dict[str, Any],
    static: dict[str, Any],
    *,
    output_path: Path | None = None,
) -> Path:
    """Build one private, disposable twelve-span ROM image for the H3 session."""
    canonical = rom_path.resolve(strict=True)
    manifest = load_json(ROM_MANIFEST)
    canonical_identity = inspect_rom(canonical)
    if (
        fixture["romSha256"] != manifest["hashes"]["sha256"]
        or canonical_identity["sha256"] != manifest["hashes"]["sha256"]
        or canonical_identity["sizeBytes"] != manifest["sizeBytes"]
    ):
        raise ValueError("blacksmith precommit canonical ROM manifest identity drift")
    original = canonical.read_bytes()
    spans = _session_instrumentation_spans(static)
    instrumented = bytearray(original)
    for row in spans:
        address = row["address"]
        if original[address : address + len(row["originalBytes"])] != row["originalBytes"]:
            raise ValueError(f"blacksmith precommit source call-site bytes drift: {row['role']}")
        instrumented[address : address + len(row["patchedBytes"])] = row["patchedBytes"]
    _validate_precommit_instrumented_copy(original, bytes(instrumented), spans)
    if inspect_rom(canonical)["sha256"] != canonical_identity["sha256"]:
        raise ValueError("blacksmith precommit instrumentation altered canonical ROM")
    if output_path is None:
        DERIVED_ROOT.mkdir(parents=True, exist_ok=True)
        with tempfile.NamedTemporaryFile(
            mode="wb",
            prefix=f"{OWNER}-session-",
            suffix=".instrumented.bin",
            dir=DERIVED_ROOT,
            delete=False,
        ) as handle:
            output = Path(handle.name)
    else:
        output = output_path
        output.parent.mkdir(parents=True, exist_ok=True)
    output.write_bytes(instrumented)
    if output.read_bytes() != bytes(instrumented):
        raise ValueError("blacksmith precommit instrumented ROM readback drift")
    return output


def _observer_config(fixture: dict[str, Any], static: dict[str, Any]) -> dict[str, Any]:
    """Keep accepted output facts out of the executable observer configuration."""
    _validate_precommit_case_matrix(fixture, static)
    _validate_equip_decision_case_matrix(fixture, static)
    _session_instrumentation_spans(static)
    _validate_precommit_cleanup_equippability(static)
    return {
        "id": fixture["id"],
        "core": fixture["emulator"]["core"],
        "cases": fixture["cases"],
        "caseOrder": fixture["caseOrder"],
        "transactionCases": fixture["transactionCases"],
        "transactionCaseOrder": fixture["transactionCaseOrder"],
        "fulfillmentCases": fixture["fulfillmentCases"],
        "fulfillmentCaseOrder": fixture["fulfillmentCaseOrder"],
        "precommitCases": fixture["precommitCases"],
        "precommitCaseOrder": fixture["precommitCaseOrder"],
        "equipDecisionCases": fixture["equipDecisionCases"],
        "equipDecisionCaseOrder": fixture["equipDecisionCaseOrder"],
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
        "precommit": {
            key: value
            for key, value in static["precommit"].items()
            if key
            not in {
                "h1InstructionBytes",
                "fullInventoryYesNo",
                "nonEquippableYesNo",
                "fullInventoryPromptCompareAddress",
                "fullInventoryRetryBranchAddress",
                "nonEquippablePromptCompareAddress",
                "nonEquippableRetryBranchAddress",
            }
        },
        "equipDecision": {
            key: value
            for key, value in static["equipDecision"].items()
            if key not in {"h1InstructionBytes", "itemDefinitionFields", "mithrilDomain"}
        },
        "precommitCaseFrameBudget": PRECOMMIT_CASE_FRAME_BUDGET,
        "precommitTransitionFrameBudget": PRECOMMIT_TRANSITION_FRAME_BUDGET,
        "precommitCleanupStackDepthBytes": PRECOMMIT_CLEANUP_STACK_DEPTH_BYTES,
        "instrumentedRom": _session_instrumentation_config(static),
        "ram": static["ram"],
        "constants": static["constants"],
        "observerFailureContract": OBSERVER_FAILURE_CONTRACT,
    }


def verify_blacksmith_mithril(
    rom_path: Path, upstream_path: Path = UPSTREAM, *, timeout_seconds: int = 180
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner="blacksmith mithril fixture")
    _assert_retained_v4_digest(fixture)
    _validate_equip_decision_case_matrix(fixture)
    verify_runtime_contract(fixture, rom_path)
    static = validate_static_contract(fixture, rom_path, upstream_path)
    _assert_golden(fixture, static)
    status_path = DERIVED_ROOT / f"{OWNER}.status.txt"
    observed_path = DERIVED_ROOT / f"{OWNER}.observed.json"
    instrumented_rom = _instrument_precommit_rom(rom_path, fixture, static)
    try:
        try:
            observed = _with_instrumented_rom_database(
                instrumented_rom,
                "SF2 H3 blacksmith mithril precommit and equip-decision instrumentation",
                lambda: run_observer(
                    rom_path=instrumented_rom,
                    observer_path=OBSERVER,
                    config=_observer_config(fixture, static),
                    output_name=OWNER,
                    timeout_seconds=timeout_seconds,
                ),
            )
        except RuntimeError as error:
            diagnostic = _failure_diagnostic(status_path)
            if diagnostic is not None:
                raise RuntimeError(f"{OWNER} observer callback failure: {diagnostic}") from error
            raise
        _assert_status(status_path)
        validate_json(observed, OBSERVATION_SCHEMA, owner="blacksmith mithril observation")
        _assert_observation(fixture, static, observed)
    except Exception:
        # A successful Lua exit is not accepted evidence until the Python
        # schema/golden comparison passes.  Do not leave that candidate output
        # for a later run to consume as though it were accepted.
        observed_path.unlink(missing_ok=True)
        raise
    finally:
        instrumented_rom.unlink(missing_ok=True)
    return {
        "Fixture": fixture["id"],
        "Cases": len(fixture["cases"])
        + len(fixture["transactionCases"])
        + len(fixture["fulfillmentCases"])
        + len(fixture["precommitCases"])
        + len(fixture["equipDecisionCases"]),
        "HelperCases": len(fixture["cases"]),
        "TransactionCases": len(fixture["transactionCases"]),
        "FulfillmentCases": len(fixture["fulfillmentCases"]),
        "PrecommitCases": len(fixture["precommitCases"]),
        "EquipDecisionCases": len(fixture["equipDecisionCases"]),
        "BizHawkLaunches": 1,
        "CallbacksCleared": observed["callbacksCleared"],
        "Restoration": observed["restoration"],
        "Status": "PASS",
    }
