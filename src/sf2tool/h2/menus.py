from __future__ import annotations

import hashlib
import json
import re
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_ai import _parse_source_file
from sf2tool.h2.battle_scene_animations import _listing_address
from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.h2.battlefield import _require_ordered_fragments
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.source_text import read_upstream_text

ID = "sf2-common-menus-static-v1"
SOURCE_ROOT = Path("code/common/menus")
ALTERNATE_SOURCE = SOURCE_ROOT / "writememberlisttext.asm"
CANONICAL_CONTAINER = SOURCE_ROOT / "memberslistscreen.asm"
MANIFEST = repo_path("manifests/extractions/common-menus-static.json")
SCHEMA = repo_path("schemas/h2/common-menus-output.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/common-menus-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2/common-menus-fixture.schema.json")
RESEARCH_INDEX = repo_path("manifests/research-index.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

SERVICE_SOURCE_PATHS = (
    Path("code/common/menus/blacksmith/blacksmithactions.asm"),
    Path("code/common/menus/blacksmith/pickmithrilweapon.asm"),
    Path("code/common/menus/caravan/caravanactions_1.asm"),
    Path("code/common/menus/caravan/caravanactions_2.asm"),
    Path("code/common/menus/church/churchactions_1.asm"),
    Path("code/common/menus/church/churchactions_2.asm"),
    Path("code/common/menus/shop/shopactions.asm"),
    Path("code/common/menus/shopscreen.asm"),
)


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _layout_menu_paths(disasm: Path) -> set[str]:
    layout = "\n".join(
        path.read_text(encoding="utf-8") for path in sorted((disasm / "layout").glob("*.asm"))
    )
    return {
        match.replace("\\", "/")
        for match in re.findall(r'include "(code\\common\\menus\\[^\"]+\.asm)"', layout)
    }


def _field_item_pairs(path: Path) -> list[dict[str, Any]]:
    lines = path.read_text(encoding="utf-8").splitlines()
    start = next(
        index for index, line in enumerate(lines) if line.strip() == "rjt_FieldItemEffects:"
    )
    values: list[str] = []
    for line in lines[start + 1 :]:
        if line.lstrip().startswith("@Done:"):
            break
        match = re.search(r"dc\.w\s+([^\s;]+)", line)
        if match:
            values.append(match.group(1))
    if values[-1] != "$FFFF" or (len(values) - 1) % 2:
        raise ValueError("field-item dispatch table shape drift")
    pairs: list[dict[str, Any]] = []
    for index in range(0, len(values) - 1, 2):
        raw_item = values[index]
        item = int(raw_item[1:], 16) if raw_item.startswith("$") else int(raw_item)
        pairs.append({"itemIndex": item, "effect": values[index + 1].split("-")[0]})
    return pairs


def _alternate_source_fact(disasm: Path, listing: str) -> dict[str, Any]:
    alternate = disasm / ALTERNATE_SOURCE
    canonical = disasm / CANONICAL_CONTAINER
    alternate_bytes = alternate.read_bytes()
    canonical_bytes = canonical.read_bytes()
    address_range = re.search(rb"; 0x([0-9A-F]+)\.\.0x([0-9A-F]+)", alternate_bytes)
    if not address_range:
        raise ValueError("member-list alternate range is missing")
    start, end = (int(value, 16) for value in address_range.groups())
    if _listing_address(listing, "BuildMembersListWindow") != start:
        raise ValueError("member-list canonical function start drift")
    canonical_source = canonical.read_text(encoding="utf-8")
    if "BuildMembersListWindow:" not in canonical_source or end != 0x137AC:
        raise ValueError("member-list canonical function boundary drift")
    return {
        "canonicalPath": CANONICAL_CONTAINER.as_posix(),
        "canonicalSymbol": "BuildMembersListWindow",
        "alternatePath": ALTERNATE_SOURCE.as_posix(),
        "alternateSymbol": "WriteMembersListText",
        "sameFunctionStartAddress": True,
        "startAddress": start,
        "endAddressExclusive": end,
        "sourceByteIdentical": canonical_bytes == alternate_bytes,
        "canonicalIncludedByLayout": True,
        "alternateIncludedByLayout": False,
        "alternateExcludedFromStrictReach": True,
        "canonicalSha256": hashlib.sha256(canonical_bytes).hexdigest().upper(),
        "alternateSha256": hashlib.sha256(alternate_bytes).hexdigest().upper(),
    }


def _require_service_section(
    path: Path, start_marker: str, end_marker: str, fragments: list[str]
) -> None:
    source = path.read_text(encoding="utf-8")
    start = source.find(start_marker)
    if start < 0:
        raise ValueError(f"service section start drift in {path.name}: {start_marker}")
    end = source.find(end_marker, start + len(start_marker))
    if end < 0:
        raise ValueError(f"service section end drift in {path.name}: {end_marker}")
    section = source[start:end]
    missing = [fragment for fragment in fragments if fragment not in section]
    if missing:
        raise ValueError(
            f"service section semantic drift in {path.name} ({start_marker}): {missing}"
        )


def _shop_section(source: str, start: str, end: str) -> str:
    begin = source.find(start)
    finish = source.find(end, begin + len(start))
    if begin < 0 or finish < 0:
        raise ValueError(f"shop section boundary drift: {start}..{end}")
    return source[begin:finish]


_SHOP_CALL_PATTERN = re.compile(
    r"^\s*(?:[A-Za-z_][A-Za-z0-9_]*:\s*)?(?:bsr|jsr)(?:\.[bswl])?\s+([^\s,;]+)\s*$",
    re.IGNORECASE,
)
_SHOP_DIRECT_TARGET_PATTERN = re.compile(r"^[A-Za-z_][A-Za-z0-9_]*$")
_SHOP_REGISTER_TARGETS = {
    *(f"a{index}" for index in range(8)),
    *(f"d{index}" for index in range(8)),
    "sp",
    "pc",
}


def _shop_direct_target(operand: str) -> str | None:
    """Return a direct symbol operand, excluding indexed/register call forms."""
    operand = re.sub(r"\.[bwl]$", "", operand, flags=re.IGNORECASE)
    if operand.endswith("(pc)"):
        operand = operand[:-4]
    if operand.startswith("(") and operand.endswith(")"):
        operand = operand[1:-1]
    if (
        not _SHOP_DIRECT_TARGET_PATTERN.fullmatch(operand)
        or operand.lower() in _SHOP_REGISTER_TARGETS
    ):
        return None
    return operand


def _shop_calls(section: str) -> list[str]:
    """Read direct bsr/jsr target order from instruction fields only."""
    targets: list[str] = []
    for raw_line in section.splitlines():
        match = _SHOP_CALL_PATTERN.match(raw_line.split(";", 1)[0])
        if match and (target := _shop_direct_target(match.group(1))) is not None:
            targets.append(target)
    return targets


def _shop_operands(operand_text: str) -> list[str]:
    """Split an ASM operand list without splitting indexed-address commas."""
    operands: list[str] = []
    current: list[str] = []
    depth = 0
    for character in operand_text.strip():
        if character == "(":
            depth += 1
        elif character == ")":
            depth -= 1
        if character == "," and depth == 0:
            operands.append("".join(current).strip())
            current = []
            continue
        current.append(character)
    if current:
        operands.append("".join(current).strip())
    return operands


def _shop_instruction_records(section: str) -> list[dict[str, Any]]:
    """Preserve local labels, opcode, operands, and direct/branch target identity."""
    records: list[dict[str, Any]] = []
    pending_labels: list[str] = []
    branch_opcodes = {
        "bcc",
        "bcs",
        "beq",
        "bge",
        "bgt",
        "bhi",
        "ble",
        "blo",
        "blt",
        "bmi",
        "bne",
        "bpl",
        "bra",
        "bvc",
        "bvs",
        "dbf",
    }
    for raw in section.splitlines():
        statement = raw.split(";", 1)[0].strip()
        if not statement:
            continue
        label = re.fullmatch(r"([@A-Za-z_][A-Za-z0-9_@]*):", statement)
        if label:
            pending_labels.append(label.group(1))
            continue
        match = re.fullmatch(r"([A-Za-z][A-Za-z0-9_]*)(\.[A-Za-z])?\s*(.*)", statement)
        if not match:
            continue
        opcode = match.group(1).lower() + (match.group(2) or "").lower()
        operands = _shop_operands(match.group(3))
        direct_target = None
        call_match = _SHOP_CALL_PATTERN.match(statement)
        if call_match:
            direct_target = _shop_direct_target(call_match.group(1))
        opcode_base = opcode.split(".", 1)[0]
        branch_target = operands[-1] if opcode_base in branch_opcodes and operands else None
        records.append(
            {
                "labels": pending_labels,
                "opcode": opcode,
                "operands": operands,
                "directTarget": direct_target,
                "branchTarget": branch_target,
            }
        )
        pending_labels = []
    return records


def _shop_direct_call_occurrences(
    path: Path, alias_targets: dict[str, str], effective_targets: set[str]
) -> list[dict[str, Any]]:
    """Count direct call instructions with both alias spelling and resolved target retained."""
    counts: Counter[tuple[str, str]] = Counter()
    for raw in read_upstream_text(path).splitlines():
        match = _SHOP_CALL_PATTERN.match(raw.split(";", 1)[0])
        instruction_target = _shop_direct_target(match.group(1)) if match else None
        effective_target = alias_targets.get(instruction_target, instruction_target)
        if instruction_target and effective_target in effective_targets:
            counts[(instruction_target, effective_target)] += 1
    return [
        {
            "instructionTarget": instruction_target,
            "effectiveTarget": effective_target,
            "siteCount": site_count,
        }
        for (instruction_target, effective_target), site_count in sorted(counts.items())
    ]


def _shop_jump_aliases(disasm: Path, effective_targets: set[str]) -> dict[str, dict[str, str]]:
    """Resolve the direct jump-interface aliases used by Shop callers."""
    aliases: dict[str, dict[str, str]] = {}
    interface_root = disasm / "code/common/tech/jumpinterfaces"
    for path in sorted(interface_root.rglob("*.asm"), key=lambda value: value.as_posix()):
        pending_label: str | None = None
        for raw in read_upstream_text(path).splitlines():
            statement = raw.split(";", 1)[0].strip()
            label = re.fullmatch(r"([A-Za-z_][A-Za-z0-9_]*):", statement)
            if label:
                pending_label = label.group(1)
                continue
            if pending_label is None:
                continue
            jump = re.fullmatch(r"jmp(?:\.[bswl])?\s+([^\s,;]+)", statement, re.IGNORECASE)
            if jump and (target := _shop_direct_target(jump.group(1))) in effective_targets:
                aliases[pending_label] = {
                    "effectiveTarget": target,
                    "sourcePath": path.relative_to(disasm).as_posix(),
                }
            if statement:
                pending_label = None
    return dict(sorted(aliases.items()))


def _require_ordered_shop_section(
    path: Path, start_marker: str, end_marker: str, fragments: tuple[str, ...]
) -> None:
    """Guard the local Shop block, including branch/order-sensitive fragments."""
    section = _shop_section(read_upstream_text(path), start_marker, end_marker)
    lines = [
        re.sub(r"\s+", " ", line.split(";", 1)[0].strip())
        for line in section.splitlines()
        if line.split(";", 1)[0].strip()
    ]
    position = 0
    for fragment in fragments:
        expected = re.sub(r"\s+", " ", fragment.strip())
        try:
            position = lines.index(expected, position)
        except ValueError:
            raise ValueError(
                f"shop control-flow drift in {path.name} ({start_marker}): {fragment}"
            ) from None
        position += 1


def _shop_static_contract(root: Path) -> dict[str, Any]:
    """Parse the Shop-only static state/control-flow surface."""
    actions = read_upstream_text(root / "shop/shopactions.asm")
    screen = read_upstream_text(root / "shopscreen.asm")
    enums = read_upstream_text(root.parents[2] / "sf2enums.asm")
    enum_names = (
        "ITEMSELLPRICE_MULTIPLIER",
        "ITEMSELLPRICE_BITSHIFTRIGHT",
        "ITEMDEF_OFFSET_PRICE",
        "COMBATANT_ITEMSLOTS",
        "ITEMS_PER_SHOP_PAGE",
        "DEALS_ITEMS_COUNTER",
    )
    enum_values: dict[str, int] = {}
    for name in enum_names:
        match = re.search(rf"^{name}:\s+equ\s+(\$[0-9A-Fa-f]+|\d+)", enums, re.MULTILINE)
        if not match:
            raise ValueError(f"shop enum drift: {name}")
        raw = match.group(1)
        enum_values[name] = int(raw[1:], 16) if raw.startswith("$") else int(raw)
    routes = {
        "buy": _shop_section(actions, "@CheckChoice_Buy:", "@CheckChoice_Sell:"),
        "sell": _shop_section(actions, "@CheckChoice_Sell:", "@CheckChoice_Repair:"),
        "repair": _shop_section(actions, "@CheckChoice_Repair:", "@CheckChoice_Deals:"),
        "deals": _shop_section(actions, "@CheckChoice_Deals:", "PopulateShopInventoryList:"),
    }
    compared_choice_values = [
        int(value)
        for value in re.findall(r"@CheckChoice_[A-Za-z]+:\s+cmpi\.w\s+#(\d+),d0", actions)
    ]
    if len(compared_choice_values) != len(routes) - 1:
        raise ValueError("shop choice comparison chain drift")
    route_calls = {name: _shop_calls(section) for name, section in routes.items()}
    route_operations = {
        name: _shop_instruction_records(section) for name, section in routes.items()
    }
    required = {
        "buy": ["j_GetItemDefinitionAddress", "j_GetGold", "j_DecreaseGold", "j_AddItem"],
        "sell": [
            "j_GetItemDefinitionAddress",
            "j_IncreaseGold",
            "j_DropItemBySlot",
            "j_AddItemToDeals",
        ],
        "repair": [
            "j_GetItemDefinitionAddress",
            "j_GetGold",
            "j_DecreaseGold",
            "j_RepairItemBySlot",
        ],
        "deals": [
            "DetermineDealsItemsNotInCurrentShop",
            "j_GetGold",
            "j_DecreaseGold",
            "j_AddItem",
            "j_RemoveItemFromDeals",
        ],
    }
    for route, targets in required.items():
        try:
            positions = [route_calls[route].index(target) for target in targets]
        except ValueError as error:
            raise ValueError(f"shop {route} required call drift") from error
        if positions != sorted(positions):
            raise ValueError(f"shop {route} ordered call drift")
    route_price_loads: dict[str, dict[str, Any]] = {}
    for route, records in route_operations.items():
        loads = [
            record
            for record in records
            if re.fullmatch(r"move\.[bwl]", record["opcode"])
            and record["operands"]
            and record["operands"][0] == "ITEMDEF_OFFSET_PRICE(a0)"
        ]
        if len(loads) != 1:
            raise ValueError(f"shop {route} item-price load drift")
        load = loads[0]
        width_by_suffix = {".b": 8, ".w": 16, ".l": 32}
        route_price_loads[route] = {
            "itemDefinitionPriceLoadWidthBits": width_by_suffix[load["opcode"][-2:]],
            "transformOpcodes": [
                record["opcode"]
                for record in records[records.index(load) + 1 :]
                if record["opcode"] in {"mulu.w", "lsr.w", "lsr.l"}
            ],
        }
    sell_multiplier = re.search(r"mulu\.w\s+#([A-Z_]+),d0", routes["sell"])
    sell_shift = re.search(r"lsr\.l\s+#([A-Z_]+),d0", routes["sell"])
    repair_shift = re.search(r"lsr\.w\s+#(\d+),d0", routes["repair"])
    page_scale = re.search(r"mulu\.w\s+#(\d+),d0", screen)
    if not all((sell_multiplier, sell_shift, repair_shift, page_scale)):
        raise ValueError("shop price or page arithmetic drift")
    if int(page_scale.group(1)) != enum_values["ITEMS_PER_SHOP_PAGE"]:
        raise ValueError("shop page-size declarations disagree")
    eligibility_labels = {
        "unsellableTypeLabel": re.search(r"andi\.b\s+#([A-Z_]+),d1", routes["sell"]),
        "rareTypeLabel": re.search(
            r"andi\.b\s+#([A-Z_]+),d1", routes["sell"][routes["sell"].find("@NotKeyItem:") :]
        ),
        "brokenItemBitLabel": re.search(r"btst\s+#([A-Z_]+),d2", routes["repair"]),
    }
    if not all(eligibility_labels.values()):
        raise ValueError("shop eligibility label drift")
    for route, fragments in {
        "buy": (
            "cmpi.w  #0,d0",
            "bne.w   @CheckChoice_Sell",
            "cmp.l   d0,d1",
            "bcc.s   byte_2013C",
            "cmpi.w  #COMBATANT_ITEMSLOTS,d2",
            "bcs.s   loc_201AC",
            "jsr     j_DecreaseGold",
            "jsr     j_AddItem",
            "bra.w   byte_200CE",
        ),
        "sell": (
            "cmpi.w  #1,d0",
            "bne.w   @CheckChoice_Repair",
            "mulu.w  #ITEMSELLPRICE_MULTIPLIER,d0",
            "lsr.l   #ITEMSELLPRICE_BITSHIFTRIGHT,d0",
            "andi.b  #ITEMTYPE_UNSELLABLE,d1",
            "andi.b  #ITEMTYPE_RARE,d1",
            "jsr     j_IncreaseGold",
            "jsr     j_DropItemBySlot",
            "jsr     j_AddItemToDeals",
            "bra.w   byte_202D2",
        ),
        "repair": (
            "cmpi.w  #2,d0",
            "bne.w   @CheckChoice_Deals",
            "lsr.w   #2,d0",
            "btst    #ITEMENTRY_BIT_BROKEN,d2",
            "bne.w   loc_204DC",
            "cmp.l   d0,d1",
            "bcc.s   loc_2051A",
            "jsr     j_DecreaseGold",
            "jsr     j_RepairItemBySlot",
            "bra.w   byte_2044A",
        ),
        "deals": (
            "jsr     DetermineDealsItemsNotInCurrentShop(pc)",
            "tst.w   ((GENERIC_LIST_LENGTH-$1000000)).w",
            "bne.s   byte_205C8",
            "cmp.l   d0,d1",
            "bcc.s   byte_20630",
            "cmpi.w  #COMBATANT_ITEMSLOTS,d2",
            "bcs.s   loc_206A0",
            "jsr     j_DecreaseGold",
            "jsr     j_AddItem",
            "jsr     j_RemoveItemFromDeals",
            "bra.w   @CheckChoice_Deals",
        ),
    }.items():
        _require_ordered_shop_section(
            root / "shop/shopactions.asm",
            f"@CheckChoice_{route.capitalize()}:",
            {
                "buy": "@CheckChoice_Sell:",
                "sell": "@CheckChoice_Repair:",
                "repair": "@CheckChoice_Deals:",
                "deals": "PopulateShopInventoryList:",
            }[route],
            fragments,
        )
    _require_ordered_shop_section(
        root / "shop/shopactions.asm",
        "ShopMenu:",
        "@CheckChoice_Buy:",
        (
            "jsr     j_ExecuteDiamondMenu",
            "cmpi.w  #-1,d0",
            "beq.s   @ExitShop",
            "bra.w   @CheckChoice_Buy",
        ),
    )
    _require_ordered_shop_section(
        root / "shop/shopactions.asm",
        "PopulateShopInventoryList:",
        "DetermineDealsItemsNotInCurrentShop:",
        (
            "bsr.s   GetShopInventoryAddress",
            "move.b  (a0)+,d7",
            "move.w  d7,((GENERIC_LIST_LENGTH-$1000000)).w",
            "dbf     d7,@Loop",
        ),
    )
    _require_ordered_shop_section(
        root / "shop/shopactions.asm",
        "DetermineDealsItemsNotInCurrentShop:",
        "DoesCurrentShopContainItem:",
        (
            "moveq   #DEALS_ITEMS_COUNTER,d7",
            "jsr     j_GetDealsItemAmount",
            "tst.b   d2",
            "bsr.w   DoesCurrentShopContainItem",
            "addq.w  #1,((GENERIC_LIST_LENGTH-$1000000)).w",
            "dbf     d7,@Loop",
        ),
    )
    _require_ordered_shop_section(
        root / "shop/shopactions.asm",
        "DoesCurrentShopContainItem:",
        "GetShopInventoryAddress:",
        (
            "bsr.w   GetShopInventoryAddress",
            "cmp.b   (a0)+,d1",
            "beq.w   @Done",
            "dbf     d7,@Loop",
        ),
    )
    _require_ordered_shop_section(
        root / "shop/shopactions.asm",
        "GetShopInventoryAddress:",
        "; End of function GetShopInventoryAddress",
        (
            "move.b  (CURRENT_SHOP_INDEX).l,d7",
            "subq.b  #1,d7",
            "bcs.w   @Done",
            "adda.w  d0,a0",
            "dbf     d7,@Loop",
        ),
    )
    targets = {
        "ShopMenu",
        "ExecuteShopScreen",
        "PopulateShopInventoryList",
        "DetermineDealsItemsNotInCurrentShop",
        "DoesCurrentShopContainItem",
        "GetShopInventoryAddress",
        "WaitForMusicResumeAndPlayerInput_Shop",
    }
    disasm = root.parents[2]
    aliases = _shop_jump_aliases(disasm, targets)
    alias_targets = {alias: fact["effectiveTarget"] for alias, fact in aliases.items()}
    external_callers = {
        path.relative_to(disasm).as_posix(): occurrences
        for path in sorted((disasm / "code").rglob("*.asm"), key=lambda value: value.as_posix())
        if path != root / "shop/shopactions.asm"
        if path != root / "shopscreen.asm"
        if (occurrences := _shop_direct_call_occurrences(path, alias_targets, targets))
    }
    internal_callers = {
        path.relative_to(disasm).as_posix(): _shop_direct_call_occurrences(
            path, alias_targets, targets
        )
        for path in (root / "shop/shopactions.asm", root / "shopscreen.asm")
    }

    def effective_site_counts(callers: dict[str, list[dict[str, Any]]]) -> dict[str, int]:
        return {
            target: sum(
                occurrence["siteCount"]
                for occurrences in callers.values()
                for occurrence in occurrences
                if occurrence["effectiveTarget"] == target
            )
            for target in sorted(targets)
        }

    return {
        "entrySymbol": "ShopMenu",
        "sourcePaths": [
            path.relative_to(disasm).as_posix()
            for path in (root / "shop/shopactions.asm", root / "shopscreen.asm")
        ],
        "choiceDispatch": {
            "menuLabel": re.search(r"moveq\s+#(MENU_SHOP),d2", actions).group(1),
            "comparedChoiceValues": compared_choice_values,
            "comparedRouteOrder": list(routes)[:-1],
            "fallthroughRoute": next(reversed(routes)),
        },
        "routeCalls": route_calls,
        "routeOperations": route_operations,
        "prices": {
            "itemDefinitionPriceOffsetBytes": enum_values["ITEMDEF_OFFSET_PRICE"],
            "routePriceDataflow": route_price_loads,
            "sellMultiplier": enum_values[sell_multiplier.group(1)],
            "sellRightShiftBits": enum_values[sell_shift.group(1)],
            "repairRightShiftBits": int(repair_shift.group(1)),
        },
        "eligibility": {
            "inventoryCapacity": enum_values["COMBATANT_ITEMSLOTS"],
            **{name: match.group(1) for name, match in eligibility_labels.items()},
        },
        "listConstruction": {
            "shopInventoryBuilder": "PopulateShopInventoryList",
            "dealsBuilder": "DetermineDealsItemsNotInCurrentShop",
            "pageItems": enum_values["ITEMS_PER_SHOP_PAGE"],
            "selectionAddressingInstructions": [
                instruction.strip()
                for instruction in re.findall(
                    r"^\s*((?:mulu\.w\s+#ITEMS_PER_SHOP_PAGE|add\.w\s+\(\(CURRENT_SHOP_SELECTION).*?)$",
                    _shop_section(screen, "@Confirm:", "@loc_15:"),
                    re.MULTILINE,
                )
            ],
            "dealsCounterInclusive": enum_values["DEALS_ITEMS_COUNTER"],
        },
        "mutations": {
            name: [
                target
                for target in calls
                if re.fullmatch(
                    r"j_(?:Increase|Decrease|Add|Drop|Remove|Repair)[A-Za-z0-9_]*", target
                )
            ]
            for name, calls in route_calls.items()
        },
        "unconditionalBranchTargets": {
            name: re.findall(r"\bbra\.[bswl]\s+([^\s;]+)", section)
            for name, section in routes.items()
        },
        "entryControlFlow": [
            line.split(";", 1)[0].rstrip()
            for line in _shop_section(actions, "ShopMenu:", "@CheckChoice_Buy:").splitlines()
            if re.match(r"\s*(?:jsr|cmp|beq|bra|rts)", line)
        ],
        "helperControlFlow": {
            "shopInventory": [
                line.strip()
                for line in _shop_section(
                    actions, "PopulateShopInventoryList:", "DetermineDealsItemsNotInCurrentShop:"
                ).splitlines()
                if re.match(r"\s*(?:bsr|move|subq|dbf|rts)", line)
            ],
            "deals": [
                line.strip()
                for line in _shop_section(
                    actions, "DetermineDealsItemsNotInCurrentShop:", "DoesCurrentShopContainItem:"
                ).splitlines()
                if re.match(r"\s*(?:jsr|tst|beq|move|addq|dbf|rts)", line)
            ],
            "currentShopMembership": [
                line.strip()
                for line in _shop_section(
                    actions, "DoesCurrentShopContainItem:", "GetShopInventoryAddress:"
                ).splitlines()
                if re.match(r"\s*(?:bsr|move|subq|cmp|beq|dbf|rts)", line)
            ],
            "shopInventoryAddress": [
                line.strip()
                for line in actions[actions.find("GetShopInventoryAddress:") :].splitlines()
                if re.match(r"\s*(?:move|subq|bcs|adda|dbf|rts)", line)
            ],
        },
        "selectionScreen": {
            "cancelValue": int(
                re.search(
                    r"moveq\s+#(-?\d+),d0", _shop_section(screen, "@Cancel:", "@Confirm:")
                ).group(1)
            ),
            "confirmInputBits": re.findall(
                r"btst\s+#(INPUT_BIT_[AC]),", _shop_section(screen, "@CheckRight:", "@loc_12:")
            ),
            "cancelInputBit": re.search(
                r"btst\s+#(INPUT_BIT_B),", _shop_section(screen, "@CheckRight:", "@loc_12:")
            ).group(1),
        },
        "jumpInterfaceAliases": aliases,
        "internalDirectCallerOccurrences": internal_callers,
        "internalEffectiveDirectCallSiteCounts": effective_site_counts(internal_callers),
        "externalDirectCallerOccurrences": external_callers,
        "externalEffectiveDirectCallSiteCounts": effective_site_counts(external_callers),
        "indirectBehavior": {"directCallInventoryDoesNotEstablishReachability": True},
    }


def _church_static_contract(root: Path) -> dict[str, Any]:
    """Parse the Church entry, route sections, and local promotion helper source surface."""
    actions = read_upstream_text(root / "church/churchactions_1.asm")
    helpers = read_upstream_text(root / "church/churchactions_2.asm")
    enums = read_upstream_text(root.parents[2] / "sf2enums.asm")
    constant_names = (
        "CHURCHMENU_PER_LEVEL_RAISE_COST",
        "CHURCHMENU_CURE_POISON_COST",
        "CHURCHMENU_CURE_STUN_COST",
        "CHURCHMENU_MIN_PROMOTABLE_LEVEL",
        "CHURCHMENU_RAISE_COST_EXTRA_WHEN_PROMOTED",
        "STATUSEFFECT_POISON",
        "STATUSEFFECT_STUN",
        "STATUSEFFECT_CURSE",
        "STATUSEFFECT_MASK",
        "CHAR_STATCAP_HP",
        "ITEMDEF_OFFSET_PRICE",
    )
    constants: dict[str, int] = {}
    for name in constant_names:
        match = re.search(rf"^{name}:\s+equ\s+(\$[0-9A-Fa-f]+|\d+)", enums, re.MULTILINE)
        if not match:
            raise ValueError(f"church constant drift: {name}")
        raw = match.group(1)
        constants[name] = int(raw[1:], 16) if raw.startswith("$") else int(raw)
    routes = {
        "raise": _shop_section(actions, "@CheckRaiseAction:", "@CheckCureAction:"),
        "cure": _shop_section(actions, "@CheckCureAction:", "@CheckPromoAction:"),
        "promote": _shop_section(actions, "@CheckPromoAction:", "@StartSave:"),
        "save": _shop_section(actions, "@StartSave:", "; End of function ChurchMenu"),
    }
    entry_operations = _shop_instruction_records(
        _shop_section(actions, "ChurchMenu:", "@CheckRaiseAction:")
    )
    operations = {name: _shop_instruction_records(section) for name, section in routes.items()}
    helper_operations = _shop_instruction_records(helpers)
    if not any(
        record["opcode"] == "cmpi.w"
        and record["operands"] == ["#CHURCHMENU_MIN_PROMOTABLE_LEVEL", "d1"]
        for record in helper_operations
    ):
        raise ValueError("church promotion-level helper cross-check drift")
    for route, fragments in {
        "raise": (
            "cmpi.w  #0,d0",
            "jsr     j_GetCurrentHp",
            "bhi.w   @RaiseNextMember",
            "mulu.w  #CHURCHMENU_PER_LEVEL_RAISE_COST,d1",
            "bcc.s   @DoRaise",
            "jsr     j_DecreaseGold",
            "jsr     j_IncreaseCurrentHp",
            "bsr.w   UpdateAllyMapsprite",
        ),
        "cure": (
            "cmpi.w  #1,d0",
            "andi.w  #STATUSEFFECT_POISON,d3",
            "andi.w  #(STATUSEFFECT_MASK-STATUSEFFECT_POISON),d1",
            "jsr     j_SetStatusEffects",
            "andi.w  #STATUSEFFECT_CURSE,d2",
        ),
        "promote": (
            "cmpi.w  #2,d0",
            "cmpi.w  #CHURCHMENU_MIN_PROMOTABLE_LEVEL,d1",
            "jsr     j_SetClass",
            "jsr     j_Promote",
            "bra.w   @StartPromo",
        ),
        "save": (
            "jsr     (SaveGame).w",
            "cmpi.w  #0,d0",
            "beq.w   @ExitMenu",
            "jmp     (WitchSuspend).w",
        ),
    }.items():
        _require_ordered_shop_section(
            root / "church/churchactions_1.asm",
            {
                "raise": "@CheckRaiseAction:",
                "cure": "@CheckCureAction:",
                "promote": "@CheckPromoAction:",
                "save": "@StartSave:",
            }[route],
            {
                "raise": "@CheckCureAction:",
                "cure": "@CheckPromoAction:",
                "promote": "@StartSave:",
                "save": "; End of function ChurchMenu",
            }[route],
            fragments,
        )
    targets = {
        "ChurchMenu",
        "CountPromotableMembers",
        "GetPromotionData",
        "FindPromotionSection",
        "ReplaceSpellsWithSorcDefaults",
        "Church_GetCurrentForceMemberInfo",
        "Church_CureStun",
        "WaitForMusicResumeAndPlayerInput",
        "UpdateAllyMapsprite",
    }
    disasm = root.parents[2]
    aliases = _shop_jump_aliases(disasm, targets)
    alias_targets = {alias: fact["effectiveTarget"] for alias, fact in aliases.items()}
    external = {
        path.relative_to(disasm).as_posix(): occurrences
        for path in sorted((disasm / "code").rglob("*.asm"), key=lambda value: value.as_posix())
        if path not in (root / "church/churchactions_1.asm", root / "church/churchactions_2.asm")
        if (occurrences := _shop_direct_call_occurrences(path, alias_targets, targets))
    }
    internal = {
        path.relative_to(disasm).as_posix(): _shop_direct_call_occurrences(
            path, alias_targets, targets
        )
        for path in (root / "church/churchactions_1.asm", root / "church/churchactions_2.asm")
    }

    def effective_counts(callers: dict[str, list[dict[str, Any]]]) -> dict[str, int]:
        return {
            target: sum(
                occurrence["siteCount"]
                for occurrences in callers.values()
                for occurrence in occurrences
                if occurrence["effectiveTarget"] == target
            )
            for target in sorted(targets)
        }

    choice_records = [operations[name][0] for name in list(routes)[:-1]]
    cancel_compare = next(
        record
        for record in entry_operations
        if record["opcode"] == "cmpi.w" and record["operands"] == ["#-1", "d0"]
    )
    cancel_branch = entry_operations[entry_operations.index(cancel_compare) + 1]
    cure_price_load = next(
        record
        for record in operations["cure"]
        if record["opcode"] == "move.w" and record["operands"] == ["ITEMDEF_OFFSET_PRICE(a0)", "d4"]
    )
    try:
        cure_price_shift = next(
            record
            for record in operations["cure"]
            if record["opcode"] == "lsr.w" and record["operands"] == ["#2", "d4"]
        )
    except StopIteration as error:
        raise ValueError("church cure price-shift drift") from error
    return {
        "entrySymbol": "ChurchMenu",
        "sourcePaths": [
            path.relative_to(disasm).as_posix()
            for path in (root / "church/churchactions_1.asm", root / "church/churchactions_2.asm")
        ],
        "choiceDispatch": {
            "menuLabel": re.search(r"moveq\s+#(MENU_CHURCH),d2", actions).group(1),
            "comparedChoiceValues": [int(record["operands"][0][1:]) for record in choice_records],
            "comparedRouteOrder": ["raise", "cure", "promote"],
            "fallthroughRoute": "save",
            "cancelValue": int(cancel_compare["operands"][0][1:]),
            "cancelBranchTarget": cancel_branch["branchTarget"],
        },
        "constants": constants,
        "routeDerived": {
            "raise": {
                "levelCostMultiplier": constants["CHURCHMENU_PER_LEVEL_RAISE_COST"],
                "promotedExtraCost": constants["CHURCHMENU_RAISE_COST_EXTRA_WHEN_PROMOTED"],
                "aliveBranchOpcode": next(
                    record["opcode"]
                    for record in operations["raise"]
                    if record["branchTarget"] == "@RaiseNextMember"
                    and not record["opcode"].startswith("bra")
                ),
                "goldBranchTarget": next(
                    record["branchTarget"]
                    for record in operations["raise"]
                    if record["opcode"] == "bcc.s"
                ),
                "mutationCalls": [
                    record["directTarget"]
                    for record in operations["raise"]
                    if record["directTarget"]
                    in {"j_DecreaseGold", "j_IncreaseCurrentHp", "UpdateAllyMapsprite"}
                ],
                "hpCap": constants["CHAR_STATCAP_HP"],
            },
            "cure": {
                "poisonCost": constants["CHURCHMENU_CURE_POISON_COST"],
                "stunCost": constants["CHURCHMENU_CURE_STUN_COST"],
                "statusMasks": {
                    "poison": constants["STATUSEFFECT_POISON"],
                    "stun": constants["STATUSEFFECT_STUN"],
                    "curse": constants["STATUSEFFECT_CURSE"],
                    "allStatusBits": constants["STATUSEFFECT_MASK"],
                },
                "curseItemPrice": {
                    "itemDefinitionOffsetBytes": constants["ITEMDEF_OFFSET_PRICE"],
                    "loadWidthBits": 16 if cure_price_load["opcode"] == "move.w" else 0,
                    "rightShiftBits": int(cure_price_shift["operands"][0][1:]),
                },
            },
            "promote": {
                "minimumLevel": constants["CHURCHMENU_MIN_PROMOTABLE_LEVEL"],
                "classAndPromotionCalls": [
                    record["directTarget"]
                    for record in operations["promote"]
                    if record["directTarget"] in {"j_SetClass", "j_Promote"}
                ],
            },
            "save": {
                "saveCallOperand": next(
                    record["operands"][0]
                    for record in operations["save"]
                    if record["opcode"] == "jsr" and record["operands"] == ["(SaveGame).w"]
                ),
                "suspendJumpOperand": next(
                    record["operands"][0]
                    for record in operations["save"]
                    if record["opcode"] == "jmp" and record["operands"] == ["(WitchSuspend).w"]
                ),
            },
        },
        "routeOperations": operations,
        "helperOperations": helper_operations,
        "jumpInterfaceAliases": aliases,
        "internalDirectCallerOccurrences": internal,
        "internalEffectiveDirectCallSiteCounts": effective_counts(internal),
        "externalDirectCallerOccurrences": external,
        "externalEffectiveDirectCallSiteCounts": effective_counts(external),
        "indirectBehavior": {"directCallInventoryDoesNotEstablishReachability": True},
    }


def _caravan_function_section(source: str, symbol: str) -> str:
    """Return one named Caravan routine through its assembler function boundary."""
    start = source.find(f"{symbol}:")
    if start < 0:
        raise ValueError(f"caravan function drift: {symbol}")
    finish = source.find("; End of function", start)
    if finish < 0:
        raise ValueError(f"caravan function end drift: {symbol}")
    return source[start:finish]


def _caravan_relative_dispatch(source: str, label: str) -> dict[str, Any]:
    """Parse a contiguous word-relative jump table without duplicating its order."""
    table = _caravan_function_section(source, label)
    rows = re.findall(
        rf"^\s*dc\.([bwl])\s+([A-Za-z_][A-Za-z0-9_]*)-{re.escape(label)}\s*$",
        table,
        re.MULTILINE,
    )
    if not rows:
        raise ValueError(f"caravan relative dispatch drift: {label}")
    widths = {"b": 1, "w": 2, "l": 4}
    suffixes = {suffix for suffix, _target in rows}
    if len(suffixes) != 1:
        raise ValueError(f"caravan relative dispatch width drift: {label}")
    return {
        "baseLabel": label,
        "entryWidthBytes": widths[suffixes.pop()],
        "targets": [target for _suffix, target in rows],
    }


def _caravan_local_section(source: str, symbol: str, start: str, end: str) -> str:
    routine = _caravan_function_section(source, symbol)
    return _shop_section(routine, start, end)


def _caravan_range(source: str, path: str) -> dict[str, Any]:
    match = re.search(r"; 0x([0-9A-F]+)\.\.0x([0-9A-F]+)\s*:", source)
    if not match:
        raise ValueError(f"caravan source range drift: {path}")
    start, end = (int(value, 16) for value in match.groups())
    return {
        "path": path,
        "startAddress": start,
        "endAddressExclusive": end,
        "physicalSpanBytes": end - start,
    }


def _caravan_opcode_width_bytes(opcode: str) -> int:
    widths = {".b": 1, ".w": 2, ".l": 4}
    try:
        return widths[opcode[-2:]]
    except KeyError as error:
        raise ValueError(f"caravan opcode width drift: {opcode}") from error


def _caravan_constant_operand(operand: str, constants: dict[str, int]) -> tuple[str, int]:
    match = re.fullmatch(r"#([A-Z][A-Z0-9_]*)", operand)
    if not match or match.group(1) not in constants:
        raise ValueError(f"caravan constant operand drift: {operand}")
    name = match.group(1)
    return name, constants[name]


def _caravan_offset_operand(operand: str, constants: dict[str, int]) -> tuple[str, int]:
    match = re.fullmatch(r"([A-Z][A-Z0-9_]*)\(a0\)", operand)
    if not match or match.group(1) not in constants:
        raise ValueError(f"caravan offset operand drift: {operand}")
    name = match.group(1)
    return name, constants[name]


def _require_ordered_caravan_function(source: str, symbol: str, fragments: tuple[str, ...]) -> None:
    """Guard a named Caravan routine without treating whole-file text as control flow."""
    section = _caravan_function_section(source, symbol)
    position = 0
    for fragment in fragments:
        position = section.find(fragment, position)
        if position < 0:
            raise ValueError(f"caravan control-flow drift in {symbol}: {fragment}")
        position += len(fragment)


def _caravan_static_contract(root: Path) -> dict[str, Any]:
    """Parse the Caravan-only static dispatch and routine inventory."""
    actions = read_upstream_text(root / "caravan/caravanactions_1.asm")
    helpers = read_upstream_text(root / "caravan/caravanactions_2.asm")
    for symbol, fragments in {
        "CaravanMenu": (
            "cmpi.w  #-1,d0",
            "beq.w   @ExitCaravan",
            "add.w   d0,d0",
            "move.w  rjt_CaravanMenuActions(pc,d0.w),d0",
            "jsr     rjt_CaravanMenuActions(pc,d0.w)",
            "bra.s   @RestartCaravan",
            "dc.w caravanMenu_Join-rjt_CaravanMenuActions",
            "dc.w caravanMenu_Depot-rjt_CaravanMenuActions",
            "dc.w caravanMenu_Item-rjt_CaravanMenuActions",
            "dc.w caravanMenu_Purge-rjt_CaravanMenuActions",
        ),
        "caravanMenu_Depot": (
            "cmpi.w  #-1,d0",
            "beq.w   @Return",
            "add.w   d0,d0",
            "move.w  rjt_CaravanDepotSubmenuActions(pc,d0.w),d0",
            "jsr     rjt_CaravanDepotSubmenuActions(pc,d0.w)",
            "bra.s   @Restart",
        ),
        "caravanMenu_Item": (
            "cmpi.w  #-1,d0",
            "beq.w   @Return",
            "add.w   d0,d0",
            "move.w  rjt_CaravanItemSubmenuActions(pc,d0.w),d0",
            "jsr     rjt_CaravanItemSubmenuActions(pc,d0.w)",
            "bra.s   @Restart",
        ),
        "caravanMenu_Join": (
            "cmpi.w  #FORCE_MAX_SIZE,((GENERIC_LIST_LENGTH-$1000000)).w",
            "bcc.s   @ChooseRelief",
            "jsr     j_JoinBattleParty",
            "jsr     j_LeaveBattleParty",
            "jsr     j_JoinBattleParty",
        ),
        "caravanMenu_Purge": ("jsr     j_LeaveBattleParty",),
        "caravanDepotSubmenu_Deposit": (
            "cmpi.w  #CARAVAN_MAX_ITEMS_NUMBER,((GENERIC_LIST_LENGTH-$1000000)).w",
            "bcc.s   @Exit",
            "jsr     j_AddItemToCaravan",
            "jsr     j_DropItemBySlot",
        ),
        "caravanDepotSubmenu_Look": (
            "move.w  ITEMDEF_OFFSET_PRICE(a0),d1",
            "mulu.w  #ITEMSELLPRICE_MULTIPLIER,d1",
            "lsr.l   #ITEMSELLPRICE_BITSHIFTRIGHT,d1",
        ),
        "caravanDepotSubmenu_Derive": (
            "cmpi.w  #COMBATANT_ITEMSLOTS,d2",
            "beq.s   @Exchange",
            "jsr     j_AddItem",
            "jsr     j_RemoveItemFromCaravan",
            "@Exchange:",
            "jsr     j_RemoveItemBySlot",
            "jsr     j_RemoveItemFromCaravan",
            "jsr     j_AddItem",
            "jsr     j_AddItemToCaravan",
        ),
        "caravanDepotSubmenu_Drop": (
            "jsr     j_RemoveItemFromCaravan",
            "jsr     j_GetItemDefinitionAddress",
            "btst    #ITEMTYPE_BIT_RARE,ITEMDEF_OFFSET_TYPE(a0)",
            "beq.s   @Continue",
            "jsr     j_AddItemToDeals",
        ),
        "caravanItemSubmenu_Give": (
            "cmpi.w  #COMBATANT_ITEMSLOTS,d2",
            "beq.s   @ExchangeItems",
            "jsr     j_AddItem",
            "jsr     j_RemoveItemBySlot",
        ),
        "caravanItemSubmenu_Use": (
            "bsr.w   UseItemOnField",
            "jsr     j_RemoveItemBySlot",
        ),
        "caravanItemSubmenu_Equip": (
            "move.b  #ITEM_SUBMENU_ACTION_EQUIP,((CURRENT_ITEM_SUBMENU_ACTION-$1000000)).w",
            "jsr     j_ExecuteMembersListScreenOnItemSummaryPage",
        ),
        "caravanItemSubmenu_Drop": (
            "jsr     j_DropItemBySlot",
            "jsr     j_GetItemDefinitionAddress",
            "btst    #ITEMTYPE_BIT_RARE,ITEMDEF_OFFSET_TYPE(a0)",
            "beq.s   @Continue",
            "jsr     j_AddItemToDeals",
        ),
    }.items():
        _require_ordered_caravan_function(actions, symbol, fragments)
    _require_ordered_caravan_function(
        helpers,
        "IsItemUnsellable",
        (
            "jsr     j_GetItemDefinitionAddress",
            "btst    #ITEMTYPE_BIT_UNSELLABLE,ITEMDEF_OFFSET_TYPE(a0)",
        ),
    )
    dispatch_tables = {
        "top": _caravan_relative_dispatch(actions, "rjt_CaravanMenuActions"),
        "depot": _caravan_relative_dispatch(actions, "rjt_CaravanDepotSubmenuActions"),
        "item": _caravan_relative_dispatch(actions, "rjt_CaravanItemSubmenuActions"),
    }
    operations = {
        "entry": _shop_instruction_records(_caravan_function_section(actions, "CaravanMenu")),
        "join": _shop_instruction_records(_caravan_function_section(actions, "caravanMenu_Join")),
        "purge": _shop_instruction_records(_caravan_function_section(actions, "caravanMenu_Purge")),
        "depot": _shop_instruction_records(_caravan_function_section(actions, "caravanMenu_Depot")),
        "depotLook": _shop_instruction_records(
            _caravan_function_section(actions, "caravanDepotSubmenu_Look")
        ),
        "depotDeposit": _shop_instruction_records(
            _caravan_function_section(actions, "caravanDepotSubmenu_Deposit")
        ),
        "depotDerive": _shop_instruction_records(
            _caravan_function_section(actions, "caravanDepotSubmenu_Derive")
        ),
        "depotDrop": _shop_instruction_records(
            _caravan_function_section(actions, "caravanDepotSubmenu_Drop")
        ),
        "item": _shop_instruction_records(_caravan_function_section(actions, "caravanMenu_Item")),
        "itemUse": _shop_instruction_records(
            _caravan_function_section(actions, "caravanItemSubmenu_Use")
        ),
        "itemGive": _shop_instruction_records(
            _caravan_function_section(actions, "caravanItemSubmenu_Give")
        ),
        "itemEquip": _shop_instruction_records(
            _caravan_function_section(actions, "caravanItemSubmenu_Equip")
        ),
        "itemDrop": _shop_instruction_records(
            _caravan_function_section(actions, "caravanItemSubmenu_Drop")
        ),
    }
    helper_sources = {
        "DisplaySpecialCaravanDescription": actions,
        "DisplayCaravanMessageWithPortrait": helpers,
        "PopulateGenericListWithMembersList": helpers,
        "CopyCaravanItems": helpers,
        "IsItemInSlotEquippedAndCursed": helpers,
        "PlayPreviousMusicAfterCurrentOne": helpers,
        "IsItemUnsellable": helpers,
    }
    helper_operations = {
        name: _shop_instruction_records(_caravan_function_section(source, name))
        for name, source in helper_sources.items()
    }
    enum_names = (
        "CARAVAN_MAX_ITEMS_NUMBER",
        "COMBATANT_ITEMSLOTS",
        "FORCE_MAX_SIZE",
        "ITEMDEF_OFFSET_PRICE",
        "ITEMDEF_OFFSET_TYPE",
        "ITEMENTRY_BIT_EQUIPPED",
        "ITEMENTRY_MASK_INDEX",
        "ITEMSELLPRICE_BITSHIFTRIGHT",
        "ITEMSELLPRICE_MULTIPLIER",
        "ITEMTYPE_BIT_RARE",
        "ITEMTYPE_BIT_UNSELLABLE",
    )
    enums = read_upstream_text(root.parents[2] / "sf2enums.asm")
    constants: dict[str, int] = {}
    for name in enum_names:
        match = re.search(rf"^{name}:\s+equ\s+(\$[0-9A-Fa-f]+|\d+)", enums, re.MULTILINE)
        if not match:
            raise ValueError(f"caravan constant drift: {name}")
        raw = match.group(1)
        constants[name] = int(raw[1:], 16) if raw.startswith("$") else int(raw)

    def direct_calls(records: list[dict[str, Any]], targets: set[str]) -> list[str]:
        return [record["directTarget"] for record in records if record["directTarget"] in targets]

    def submenu_dispatch(symbol: str, table_name: str) -> dict[str, Any]:
        records = operations[symbol]
        cancel_compare = next(
            record
            for record in records
            if record["opcode"] == "cmpi.w" and record["operands"] == ["#-1", "d0"]
        )
        cancel_branch = records[records.index(cancel_compare) + 1]
        selector_add = next(
            record
            for record in records
            if record["opcode"] == "add.w" and record["operands"] == ["d0", "d0"]
        )
        selector_scale_bytes = _caravan_opcode_width_bytes(selector_add["opcode"])
        if cancel_branch["opcode"] != "beq.w":
            raise ValueError(f"caravan {symbol} selector or cancel polarity drift")
        if selector_scale_bytes != dispatch_tables[table_name]["entryWidthBytes"]:
            raise ValueError(f"caravan {symbol} selector/table width drift")
        return {
            "entrySymbol": {
                "depot": "caravanMenu_Depot",
                "item": "caravanMenu_Item",
            }[symbol],
            "menuLabel": next(
                record["operands"][0][1:]
                for record in records
                if record["opcode"] == "moveq" and record["operands"][1] == "d2"
            ),
            "dispatchTable": table_name,
            "selectorScaleBytes": selector_scale_bytes,
            "cancelValue": int(cancel_compare["operands"][0][1:]),
            "cancelBranchTarget": cancel_branch["branchTarget"],
            "loopBranchTarget": next(
                record["branchTarget"]
                for record in records
                if record["opcode"] == "bra.s" and record["branchTarget"] == "@Restart"
            ),
        }

    entry_records = operations["entry"]
    selector_add = next(
        record
        for record in entry_records
        if record["opcode"] == "add.w" and record["operands"] == ["d0", "d0"]
    )
    cancel_compare = next(
        record
        for record in entry_records
        if record["opcode"] == "cmpi.w" and record["operands"] == ["#-1", "d0"]
    )
    cancel_branch = entry_records[entry_records.index(cancel_compare) + 1]
    if cancel_branch["opcode"] != "beq.w":
        raise ValueError("caravan top cancel branch polarity drift")
    selector_scale_bytes = _caravan_opcode_width_bytes(selector_add["opcode"])
    if selector_scale_bytes != dispatch_tables["top"]["entryWidthBytes"]:
        raise ValueError("caravan top selector/table width drift")
    join_capacity_compare = next(
        record
        for record in operations["join"]
        if record["opcode"] == "cmpi.w"
        and record["operands"] == ["#FORCE_MAX_SIZE", "((GENERIC_LIST_LENGTH-$1000000)).w"]
    )
    join_capacity_branch = operations["join"][operations["join"].index(join_capacity_compare) + 1]
    join_capacity_name, battle_party_capacity = _caravan_constant_operand(
        join_capacity_compare["operands"][0], constants
    )
    deposit_capacity_compare = next(
        record
        for record in operations["depotDeposit"]
        if record["opcode"] == "cmpi.w"
        and record["operands"]
        == ["#CARAVAN_MAX_ITEMS_NUMBER", "((GENERIC_LIST_LENGTH-$1000000)).w"]
    )
    deposit_capacity_branch = operations["depotDeposit"][
        operations["depotDeposit"].index(deposit_capacity_compare) + 1
    ]
    deposit_capacity_name, stored_item_capacity = _caravan_constant_operand(
        deposit_capacity_compare["operands"][0], constants
    )
    derive_capacity_compare = next(
        record
        for record in operations["depotDerive"]
        if record["opcode"] == "cmpi.w" and record["operands"] == ["#COMBATANT_ITEMSLOTS", "d2"]
    )
    give_capacity_compare = next(
        record
        for record in operations["itemGive"]
        if record["opcode"] == "cmpi.w" and record["operands"] == ["#COMBATANT_ITEMSLOTS", "d2"]
    )
    derive_capacity_branch = operations["depotDerive"][
        operations["depotDerive"].index(derive_capacity_compare) + 1
    ]
    give_capacity_branch = operations["itemGive"][
        operations["itemGive"].index(give_capacity_compare) + 1
    ]
    derive_capacity_name, recipient_item_capacity = _caravan_constant_operand(
        derive_capacity_compare["operands"][0], constants
    )
    give_capacity_name, give_recipient_item_capacity = _caravan_constant_operand(
        give_capacity_compare["operands"][0], constants
    )
    if (
        join_capacity_name != "FORCE_MAX_SIZE"
        or deposit_capacity_name != "CARAVAN_MAX_ITEMS_NUMBER"
        or derive_capacity_name != "COMBATANT_ITEMSLOTS"
        or give_capacity_name != "COMBATANT_ITEMSLOTS"
        or recipient_item_capacity != give_recipient_item_capacity
    ):
        raise ValueError("caravan capacity operand identity drift")
    look_price_load = next(
        record
        for record in operations["depotLook"]
        if record["opcode"] == "move.w" and record["operands"] == ["ITEMDEF_OFFSET_PRICE(a0)", "d1"]
    )
    look_price_index = operations["depotLook"].index(look_price_load)
    look_price_multiply = operations["depotLook"][look_price_index + 1]
    look_price_shift = operations["depotLook"][look_price_index + 2]
    if look_price_multiply["opcode"] != "mulu.w" or look_price_shift["opcode"] != "lsr.l":
        raise ValueError("caravan depot look price arithmetic drift")
    price_offset_name, price_offset_bytes = _caravan_offset_operand(
        look_price_load["operands"][0], constants
    )
    multiply_name, multiply_constant = _caravan_constant_operand(
        look_price_multiply["operands"][0], constants
    )
    shift_name, right_shift_bits = _caravan_constant_operand(
        look_price_shift["operands"][0], constants
    )
    if (
        price_offset_name != "ITEMDEF_OFFSET_PRICE"
        or multiply_name != "ITEMSELLPRICE_MULTIPLIER"
        or shift_name != "ITEMSELLPRICE_BITSHIFTRIGHT"
    ):
        raise ValueError("caravan depot look price operand identity drift")
    rare_bit_records = {
        "depot": next(
            record
            for record in operations["depotDrop"]
            if record["opcode"] == "btst" and record["operands"][1] == "ITEMDEF_OFFSET_TYPE(a0)"
        ),
        "item": next(
            record
            for record in operations["itemDrop"]
            if record["opcode"] == "btst" and record["operands"][1] == "ITEMDEF_OFFSET_TYPE(a0)"
        ),
        "unsellable": next(
            record
            for record in helper_operations["IsItemUnsellable"]
            if record["opcode"] == "btst" and record["operands"][1] == "ITEMDEF_OFFSET_TYPE(a0)"
        ),
    }
    rare_bit_names_and_values = {
        route: _caravan_constant_operand(record["operands"][0], constants)
        for route, record in rare_bit_records.items()
    }
    if (
        rare_bit_names_and_values["depot"][0] != "ITEMTYPE_BIT_RARE"
        or rare_bit_names_and_values["item"][0] != "ITEMTYPE_BIT_RARE"
        or rare_bit_names_and_values["unsellable"][0] != "ITEMTYPE_BIT_UNSELLABLE"
    ):
        raise ValueError("caravan item-type bit operand identity drift")
    semantic_sections = {
        "depotDeriveNormal": _shop_instruction_records(
            _caravan_local_section(
                actions, "caravanDepotSubmenu_Derive", "; Derive item", "@Exchange:"
            )
        ),
        "depotDeriveExchange": _shop_instruction_records(
            _caravan_local_section(
                actions, "caravanDepotSubmenu_Derive", "@Exchange:", "@Goto_Restart:"
            )
        ),
        "itemGiveSelf": _shop_instruction_records(
            _caravan_local_section(
                actions, "caravanItemSubmenu_Give", "; Is giving to self?", "@IsInventoryFull:"
            )
        ),
        "itemGiveNormal": _shop_instruction_records(
            _caravan_local_section(
                actions, "caravanItemSubmenu_Give", "; Give item", "@ExchangeItems:"
            )
        ),
        "itemGiveExchange": _shop_instruction_records(
            _caravan_local_section(
                actions, "caravanItemSubmenu_Give", "@ExchangeItems:", "@Goto_Restart:"
            )
        ),
    }
    if direct_calls(
        semantic_sections["depotDeriveNormal"], {"j_AddItem", "j_RemoveItemFromCaravan"}
    ) != ["j_AddItem", "j_RemoveItemFromCaravan"]:
        raise ValueError("caravan depot derive normal mutation order drift")
    if direct_calls(semantic_sections["itemGiveNormal"], {"j_AddItem", "j_RemoveItemBySlot"}) != [
        "j_AddItem",
        "j_RemoveItemBySlot",
    ]:
        raise ValueError("caravan item give normal mutation order drift")
    targets = {
        "CaravanMenu",
        "caravanMenu_Join",
        "caravanMenu_Depot",
        "caravanMenu_Item",
        "caravanMenu_Purge",
        "caravanDepotSubmenu_Look",
        "caravanDepotSubmenu_Deposit",
        "caravanDepotSubmenu_Derive",
        "caravanDepotSubmenu_Drop",
        "caravanItemSubmenu_Use",
        "caravanItemSubmenu_Give",
        "caravanItemSubmenu_Equip",
        "caravanItemSubmenu_Drop",
        *helper_operations,
    }
    disasm = root.parents[2]
    aliases = _shop_jump_aliases(disasm, targets)
    alias_targets = {alias: fact["effectiveTarget"] for alias, fact in aliases.items()}
    source_paths = (root / "caravan/caravanactions_1.asm", root / "caravan/caravanactions_2.asm")
    internal = {
        path.relative_to(disasm).as_posix(): _shop_direct_call_occurrences(
            path, alias_targets, targets
        )
        for path in source_paths
    }
    external = {
        path.relative_to(disasm).as_posix(): occurrences
        for path in sorted((disasm / "code").rglob("*.asm"), key=lambda value: value.as_posix())
        if path not in source_paths
        if (occurrences := _shop_direct_call_occurrences(path, alias_targets, targets))
    }

    def effective_counts(callers: dict[str, list[dict[str, Any]]]) -> dict[str, int]:
        return {
            target: sum(
                occurrence["siteCount"]
                for occurrences in callers.values()
                for occurrence in occurrences
                if occurrence["effectiveTarget"] == target
            )
            for target in sorted(targets)
        }

    return {
        "entrySymbol": "CaravanMenu",
        "sourcePaths": [
            path.as_posix()
            for path in (
                Path("code/common/menus/caravan/caravanactions_1.asm"),
                Path("code/common/menus/caravan/caravanactions_2.asm"),
            )
        ],
        "dispatchTables": dispatch_tables,
        "topDispatch": {
            "menuLabel": next(
                record["operands"][0][1:]
                for record in entry_records
                if record["opcode"] == "moveq" and record["operands"][1] == "d2"
            ),
            "selectorScaleBytes": selector_scale_bytes,
            "cancelValue": int(cancel_compare["operands"][0][1:]),
            "cancelBranchTarget": cancel_branch["branchTarget"],
            "loopBranchTarget": next(
                record["branchTarget"]
                for record in entry_records
                if record["opcode"] == "bra.s" and record["branchTarget"] == "@RestartCaravan"
            ),
        },
        "submenus": {
            "depot": submenu_dispatch("depot", "depot"),
            "item": submenu_dispatch("item", "item"),
        },
        "constants": constants,
        "sourceRanges": [
            _caravan_range(actions, "code/common/menus/caravan/caravanactions_1.asm"),
            _caravan_range(helpers, "code/common/menus/caravan/caravanactions_2.asm"),
        ],
        "routeDerived": {
            "join": {
                "battlePartyCapacity": battle_party_capacity,
                "capacityBranchOpcode": join_capacity_branch["opcode"],
                "capacityBranchTarget": join_capacity_branch["branchTarget"],
                "partyMutationCalls": direct_calls(
                    operations["join"], {"j_LeaveBattleParty", "j_JoinBattleParty"}
                ),
            },
            "purge": {
                "partyMutationCalls": direct_calls(operations["purge"], {"j_LeaveBattleParty"}),
            },
            "depot": {
                "storedItemCapacity": stored_item_capacity,
                "depositCapacityBranchOpcode": deposit_capacity_branch["opcode"],
                "depositCapacityBranchTarget": deposit_capacity_branch["branchTarget"],
                "depositMutationCalls": direct_calls(
                    operations["depotDeposit"], {"j_AddItemToCaravan", "j_DropItemBySlot"}
                ),
                "recipientItemCapacity": recipient_item_capacity,
                "deriveCapacityBranchTarget": derive_capacity_branch["branchTarget"],
                "deriveNormalMutationCalls": direct_calls(
                    semantic_sections["depotDeriveNormal"], {"j_AddItem", "j_RemoveItemFromCaravan"}
                ),
                "deriveExchangeMutationCalls": direct_calls(
                    semantic_sections["depotDeriveExchange"],
                    {
                        "j_RemoveItemBySlot",
                        "j_RemoveItemFromCaravan",
                        "j_AddItem",
                        "j_AddItemToCaravan",
                    },
                ),
                "dropMutationCalls": direct_calls(
                    operations["depotDrop"],
                    {"j_RemoveItemFromCaravan", "j_GetItemDefinitionAddress", "j_AddItemToDeals"},
                ),
                "rareBit": rare_bit_names_and_values["depot"][1],
                "unsellableBit": rare_bit_names_and_values["unsellable"][1],
                "lookPrice": {
                    "itemDefinitionOffsetBytes": price_offset_bytes,
                    "loadWidthBits": _caravan_opcode_width_bytes(look_price_load["opcode"]) * 8,
                    "multiplyConstant": multiply_constant,
                    "rightShiftBits": right_shift_bits,
                },
            },
            "item": {
                "useMutationCalls": direct_calls(
                    operations["itemUse"], {"UseItemOnField", "j_RemoveItemBySlot"}
                ),
                "recipientItemCapacity": give_recipient_item_capacity,
                "giveCapacityBranchTarget": give_capacity_branch["branchTarget"],
                "giveSelfMutationCalls": direct_calls(
                    semantic_sections["itemGiveSelf"], {"j_RemoveItemBySlot", "j_AddItem"}
                ),
                "giveNormalMutationCalls": direct_calls(
                    semantic_sections["itemGiveNormal"], {"j_AddItem", "j_RemoveItemBySlot"}
                ),
                "giveExchangeMutationCalls": direct_calls(
                    semantic_sections["itemGiveExchange"], {"j_RemoveItemBySlot", "j_AddItem"}
                ),
                "equipSelectionAction": next(
                    record["operands"][0][1:]
                    for record in operations["itemEquip"]
                    if record["opcode"] == "move.b"
                    and record["operands"][1] == "((CURRENT_ITEM_SUBMENU_ACTION-$1000000)).w"
                ),
                "dropMutationCalls": direct_calls(
                    operations["itemDrop"],
                    {"j_DropItemBySlot", "j_GetItemDefinitionAddress", "j_AddItemToDeals"},
                ),
                "rareBit": rare_bit_names_and_values["item"][1],
            },
        },
        "routeOperations": operations,
        "helperOperations": helper_operations,
        "jumpInterfaceAliases": aliases,
        "internalDirectCallerOccurrences": internal,
        "internalEffectiveDirectCallSiteCounts": effective_counts(internal),
        "externalDirectCallerOccurrences": external,
        "externalEffectiveDirectCallSiteCounts": effective_counts(external),
        "indirectBehavior": {"directCallInventoryDoesNotEstablishReachability": True},
    }


def _blacksmith_function_section(source: str, symbol: str) -> str:
    start = source.find(f"{symbol}:")
    if start < 0:
        raise ValueError(f"blacksmith function drift: {symbol}")
    finish = source.find("; End of function", start)
    if finish < 0:
        raise ValueError(f"blacksmith function end drift: {symbol}")
    return source[start:finish]


def _require_ordered_blacksmith_function(
    source: str, symbol: str, fragments: tuple[str, ...]
) -> None:
    """Guard Blacksmith control flow within its named source function."""
    section = _blacksmith_function_section(source, symbol)
    position = 0
    for fragment in fragments:
        position = section.find(fragment, position)
        if position < 0:
            raise ValueError(f"blacksmith control-flow drift in {symbol}: {fragment}")
        position += len(fragment)


def _blacksmith_static_contract(root: Path) -> dict[str, Any]:
    """Parse the Blacksmith source surface before interpreting its static relationships."""
    actions = read_upstream_text(root / "blacksmith/blacksmithactions.asm")
    picker = read_upstream_text(root / "blacksmith/pickmithrilweapon.asm")
    enums = read_upstream_text(root.parents[2] / "sf2enums.asm")
    constant_names = (
        "BLACKSMITH_MAX_ORDERS_NUMBER",
        "BLACKSMITH_ORDERS_COUNTER",
        "BLACKSMITH_MITHRIL_ITEM",
        "BLACKSMITH_ORDER_COST",
        "CHAR_CLASS_FIRSTPROMOTED",
        "COMBATANT_ITEMSLOTS",
        "MITHRIL_WEAPON_CLASSES_COUNTER",
        "MITHRIL_WEAPON_ORDER_SLOT_SIZE",
        "MITHRIL_WEAPONS_PER_CLASS_COUNTER",
        "SOUND_COMMAND_PLAY_PREVIOUS_MUSIC",
    )
    constants: dict[str, int] = {}
    for name in constant_names:
        match = re.search(rf"^{name}:\s+equ\s+(\$[0-9A-Fa-f]+|\d+)", enums, re.MULTILINE)
        if not match:
            raise ValueError(f"blacksmith constant drift: {name}")
        raw = match.group(1)
        constants[name] = int(raw[1:], 16) if raw.startswith("$") else int(raw)
    operation_sources = {
        "BlacksmithMenu": actions,
        "ProcessBlacksmithOrders": actions,
        "BlacksmithAction_FulfillOrder": actions,
        "BlacksmithAction_PlaceOrder": actions,
        "WaitForMusicResumeAndPlayerInput_Blacksmith": actions,
        "CountPendingAndReadyToFulfillOrders": actions,
        "IsClassBlacksmithEligible": actions,
        "PickMithrilWeapon": picker,
    }
    operations = {
        symbol: _shop_instruction_records(_blacksmith_function_section(source, symbol))
        for symbol, source in operation_sources.items()
    }

    def immediate(record: dict[str, Any], operand_index: int = 0) -> tuple[str, int]:
        return _caravan_constant_operand(record["operands"][operand_index], constants)

    def calls(symbol: str, targets: set[str]) -> list[str]:
        return [
            record["directTarget"]
            for record in operations[symbol]
            if record["directTarget"] in targets
        ]

    for symbol, fragments in {
        "BlacksmithMenu": (
            "link    a6,#-24",
            "clr.w   readyToFulfillOrdersNumber(a6)",
            "clr.w   pendingOrdersNumber(a6)",
            "clr.w   fulfilledOrdersNumber(a6)",
            "clr.w   fulfillOrdersFlag(a6)",
            "bsr.w   ProcessBlacksmithOrders",
        ),
        "ProcessBlacksmithOrders": (
            "jsr     j_UpdateForce",
            "move.w  ((TARGETS_LIST_LENGTH-$1000000)).w,((GENERIC_LIST_LENGTH-$1000000)).w",
            "move.w  ((TARGETS_LIST_LENGTH-$1000000)).w,d7",
            "subq.b  #1,d7",
            "move.b  (a0)+,(a1)+",
            "dbf     d7,@CopyForceMembersList_Loop",
            "bsr.w   CountPendingAndReadyToFulfillOrders",
            "move.w  #BLACKSMITH_MAX_ORDERS_NUMBER,d7",
            "bsr.w   BlacksmithAction_FulfillOrder",
            "bsr.w   BlacksmithAction_PlaceOrder",
        ),
        "BlacksmithAction_FulfillOrder": (
            "cmpi.w  #-1,d0",
            "bne.s   @IsMemberInventoryFull",
            "cmpi.w  #COMBATANT_ITEMSLOTS,d2",
            "bcs.s   @CheckEquipmentType",
            "cmpi.w  #EQUIPMENTTYPE_TOOL,d2",
            "beq.s   @AddItem",
            "jsr     j_IsWeaponOrRingEquippable",
            "bcs.s   @AddItem",
            "jsr     j_AddItem",
            "move.w  #0,(a1)",
            "addi.w  #1,fulfilledOrdersNumber(a6)",
            "bcc.w   byte_21CD0",
            "cmpi.w  #0,d0",
            "bne.w   byte_21CD0",
            "jsr     j_UnequipItemBySlotIfNotCursed",
            "cmpi.w  #2,d2",
            "bne.w   @EquipNewItem",
            "jsr     j_UnequipItemBySlotIfNotCursed",
            "cmpi.w  #2,d2",
            "bne.w   @EquipNewItem",
            "jsr     j_EquipItemBySlot",
            "cmpi.w  #2,d2",
            "bne.s   byte_21CC8",
        ),
        "BlacksmithAction_PlaceOrder": (
            "cmpi.w  #-1,d0",
            "beq.w   @Done",
            "cmpi.w  #BLACKSMITH_MITHRIL_ITEM,d2",
            "beq.w   byte_21D1A",
            "cmpi.w  #-1,d0",
            "beq.s   byte_21CDE",
            "cmpi.w  #CHAR_CLASS_FIRSTPROMOTED,d1",
            "bcc.w   @IsCustomerClassEligible",
            "bsr.w   IsClassBlacksmithEligible",
            "cmpi.w  #0,d0",
            "beq.w   @ConfirmOrder",
            "jsr     j_alt_YesNoPrompt",
            "cmpi.w  #0,d0",
            "beq.s   @CheckGold",
            "cmpi.l  #BLACKSMITH_ORDER_COST,d1",
            "bcc.w   @PlaceOrder",
            "jsr     j_DecreaseGold",
            "jsr     j_DropItemBySlot",
            "bsr.w   PickMithrilWeapon",
            "move.w  #80,d1",
            "jsr     j_ClearFlag",
            "cmpi.w  #BLACKSMITH_MAX_ORDERS_NUMBER,d0",
            "bne.s   byte_21E16",
        ),
        "CountPendingAndReadyToFulfillOrders": (
            "move.w  #80,d1",
            "jsr     j_CheckFlag",
            "beq.w   @Continue",
            "move.w  #BLACKSMITH_MAX_ORDERS_NUMBER,d7",
            "addi.w  #1,readyToFulfillOrdersNumber(a6)",
            "addi.w  #1,pendingOrdersNumber(a6)",
            "dbf     d7,@Loop",
        ),
        "IsClassBlacksmithEligible": (
            "move.w  (a0)+,d7",
            "subq.w  #1,d7",
            "dbf     d7,@Loop",
        ),
        "PickMithrilWeapon": (
            "move.w  #MITHRIL_WEAPON_CLASSES_COUNTER,d7",
            "move.w  (a0)+,d6",
            "subq.w  #1,d6",
            "move.w  (a0)+,d1",
            "move.w  clientClass(a6),d2",
            "cmp.w   d1,d2",
            "beq.w   @GetWeaponsEntryAddress",
            "dbf     d6,@FindCharacterClass_Loop",
            "addi.w  #1,d0",
            "dbf     d7,@FindWeaponClass_Loop",
            "move.w  #2,d6",
            "jsr     (GenerateRandomNumber).w",
            "cmpi.w  #0,d7",
            "bne.w   @GetWeaponsEntryAddress",
            "move.w  #2,d0",
            "lsl.w   #3,d0",
            "move.w  #MITHRIL_WEAPONS_PER_CLASS_COUNTER,d5",
            "move.b  (a0)+,d0",
            "move.b  (a0)+,d1",
            "move.w  d0,d6",
            "jsr     (GenerateRandomNumber).w",
            "cmpi.w  #0,d7",
            "beq.w   @LoadIndex",
            "dbf     d5,@PickWeapon_Loop",
            "move.w  #BLACKSMITH_ORDERS_COUNTER,d7",
            "cmpi.w  #0,(a0)",
            "bne.w   @Next",
            "move.w  d1,(a0)",
            "@Next:",
            "adda.w  d0,a0",
            "dbf     d7,@LoadIndex_Loop",
        ),
    }.items():
        _require_ordered_blacksmith_function(operation_sources[symbol], symbol, fragments)

    frame_link = next(
        record
        for record in operations["BlacksmithMenu"]
        if record["opcode"] == "link" and record["operands"][0] == "a6"
    )
    frame_size = abs(int(frame_link["operands"][1][1:]))
    frame_declarations = dict(
        (name, int(offset))
        for name, offset in re.findall(
            r"^\s*([A-Za-z][A-Za-z0-9]*)\s*=\s*(-\d+)\s*$",
            actions[: actions.find("BlacksmithMenu:")],
            re.MULTILINE,
        )
    )
    counter_names = (
        "readyToFulfillOrdersNumber",
        "fulfilledOrdersNumber",
        "pendingOrdersNumber",
        "fulfillOrdersFlag",
    )
    counter_offsets = {name: frame_declarations[name] for name in counter_names}
    if frame_size != max(abs(offset) for offset in frame_declarations.values()):
        raise ValueError("blacksmith frame declaration/link drift")
    initialization_order = [
        record["operands"][0].removesuffix("(a6)")
        for record in operations["BlacksmithMenu"]
        if record["opcode"] == "clr.w"
        and record["operands"][0].removesuffix("(a6)") in counter_names
    ]
    max_orders_record = next(
        record
        for record in operations["ProcessBlacksmithOrders"]
        if record["opcode"] == "move.w"
        and record["operands"] == ["#BLACKSMITH_MAX_ORDERS_NUMBER", "d7"]
    )
    max_orders_name, max_orders = immediate(max_orders_record)
    order_counter_record = next(
        record
        for record in operations["PickMithrilWeapon"]
        if record["opcode"] == "move.w"
        and record["operands"] == ["#BLACKSMITH_ORDERS_COUNTER", "d7"]
    )
    order_counter_name, order_counter = immediate(order_counter_record)
    slot_shift = next(
        record
        for record in operations["PickMithrilWeapon"]
        if record["opcode"] == "lsl.w" and record["operands"] == ["#3", "d0"]
    )
    row_stride_bytes = 1 << int(slot_shift["operands"][0][1:])
    slot_stride_record = next(
        record
        for record in operations["PickMithrilWeapon"]
        if record["opcode"] == "adda.w" and record["operands"] == ["d0", "a0"]
    )
    slot_width_record = next(
        record
        for record in operations["BlacksmithAction_FulfillOrder"]
        if record["opcode"] == "lsl.w" and record["operands"] == ["#1", "d6"]
    )
    order_slot_width_bytes = 1 << int(slot_width_record["operands"][0][1:])
    storage_clear_record = next(
        record
        for record in operations["BlacksmithAction_FulfillOrder"]
        if record["opcode"] == "move.w" and record["operands"] == ["#0", "(a1)"]
    )
    slot_size_name = "MITHRIL_WEAPON_ORDER_SLOT_SIZE"
    if (
        max_orders_name != "BLACKSMITH_MAX_ORDERS_NUMBER"
        or order_counter_name != "BLACKSMITH_ORDERS_COUNTER"
        or order_counter + 1 != max_orders
        or constants[slot_size_name] != order_slot_width_bytes
        or slot_stride_record["operands"] != ["d0", "a0"]
    ):
        raise ValueError("blacksmith order capacity/width relation drift")
    mithril_compare = next(
        record
        for record in operations["BlacksmithAction_PlaceOrder"]
        if record["opcode"] == "cmpi.w" and record["operands"][1] == "d2"
    )
    mithril_name, mithril_item = immediate(mithril_compare)
    promotion_compare = next(
        record
        for record in operations["BlacksmithAction_PlaceOrder"]
        if record["opcode"] == "cmpi.w" and record["operands"][1] == "d1"
    )
    promotion_name, promotion_class = immediate(promotion_compare)
    gold_compare = next(
        record
        for record in operations["BlacksmithAction_PlaceOrder"]
        if record["opcode"] == "cmpi.l" and record["operands"][1] == "d1"
    )
    gold_name, order_cost = immediate(gold_compare)
    inventory_compare = next(
        record
        for record in operations["BlacksmithAction_FulfillOrder"]
        if record["opcode"] == "cmpi.w" and record["operands"][1] == "d2"
    )
    inventory_name, inventory_capacity = immediate(inventory_compare)
    class_counter_record = next(
        record
        for record in operations["PickMithrilWeapon"]
        if record["opcode"] == "move.w"
        and record["operands"][1] == "d7"
        and record["operands"][0] == "#MITHRIL_WEAPON_CLASSES_COUNTER"
    )
    class_counter_name, class_counter = immediate(class_counter_record)
    weapon_counter_record = next(
        record
        for record in operations["PickMithrilWeapon"]
        if record["opcode"] == "move.w" and record["operands"][1] == "d5"
    )
    weapon_counter_name, weapon_counter = immediate(weapon_counter_record)
    fallback_bound_record = next(
        record
        for record in operations["PickMithrilWeapon"]
        if record["opcode"] == "move.w" and record["operands"] == ["#2", "d6"]
    )
    fallback_random_bound = int(fallback_bound_record["operands"][0][1:])
    fallback_compare = next(
        record
        for record in operations["PickMithrilWeapon"]
        if record["opcode"] == "cmpi.w" and record["operands"] == ["#0", "d7"]
    )
    fallback_branch = operations["PickMithrilWeapon"][
        operations["PickMithrilWeapon"].index(fallback_compare) + 1
    ]
    fallback_row_record = next(
        record
        for record in operations["PickMithrilWeapon"]
        if record["opcode"] == "move.w" and record["operands"] == ["#2", "d0"]
    )
    initial_group_record = next(
        record
        for record in operations["PickMithrilWeapon"]
        if record["opcode"] == "clr.w" and record["operands"] == ["d0"]
    )
    fallback_convergence_record = next(
        record
        for record in operations["PickMithrilWeapon"]
        if "@GetWeaponsEntryAddress" in record["labels"]
    )
    data_root = root.parents[2] / "data/stats"
    eligible = read_upstream_text(data_root / "allies/classes/blacksmitheligibleclasses.asm")
    weapons = read_upstream_text(data_root / "items/mithrilweapons.asm")
    eligible_classes = re.search(r"^\s*classes\s+([^\r\n]+)", eligible, re.MULTILINE)
    class_groups = re.findall(r"^\s*classes\s+([^\r\n]+)", weapons, re.MULTILINE)
    weapon_rows = re.findall(r"^\s*mithrilWeapons\s+", weapons, re.MULTILINE)
    if (
        not eligible_classes
        or len(class_groups) != class_counter + 2
        or len(weapon_rows) != class_counter + 1
    ):
        raise ValueError("blacksmith cross-owned class/weapon table shape drift")
    if (
        mithril_name != "BLACKSMITH_MITHRIL_ITEM"
        or promotion_name != "CHAR_CLASS_FIRSTPROMOTED"
        or gold_name != "BLACKSMITH_ORDER_COST"
        or inventory_name != "COMBATANT_ITEMSLOTS"
        or class_counter_name != "MITHRIL_WEAPON_CLASSES_COUNTER"
        or weapon_counter_name != "MITHRIL_WEAPONS_PER_CLASS_COUNTER"
        or row_stride_bytes != 2 * (weapon_counter + 1)
    ):
        raise ValueError("blacksmith use-site operand/row relation drift")

    def find(symbol: str, predicate) -> dict[str, Any]:
        try:
            return next(record for record in operations[symbol] if predicate(record))
        except StopIteration as error:
            raise ValueError(f"blacksmith parsed use-site drift: {symbol}") from error

    def successor(symbol: str, record: dict[str, Any]) -> dict[str, Any]:
        index = next(
            index for index, candidate in enumerate(operations[symbol]) if candidate is record
        )
        return operations[symbol][index + 1]

    def after(symbol: str, start: dict[str, Any], predicate) -> dict[str, Any]:
        start_index = next(
            index for index, candidate in enumerate(operations[symbol]) if candidate is start
        )
        try:
            return next(
                record for record in operations[symbol][start_index + 1 :] if predicate(record)
            )
        except StopIteration as error:
            raise ValueError(f"blacksmith ordered use-site drift: {symbol}") from error

    def conditional_branch(name: str, symbol: str, operation: dict[str, Any]) -> dict[str, Any]:
        return {
            "name": name,
            "operation": operation,
            "branch": successor(symbol, operation),
        }

    process_copy_length = find(
        "ProcessBlacksmithOrders",
        lambda record: (
            record["opcode"] == "move.w"
            and record["operands"]
            == ["((TARGETS_LIST_LENGTH-$1000000)).w", "((GENERIC_LIST_LENGTH-$1000000)).w"]
        ),
    )
    process_copy_counter_source = after(
        "ProcessBlacksmithOrders",
        process_copy_length,
        lambda record: record["opcode"] == "move.w" and record["operands"][1] == "d7",
    )
    process_copy_width = find(
        "ProcessBlacksmithOrders",
        lambda record: record["opcode"] == "move.b" and record["operands"] == ["(a0)+", "(a1)+"],
    )
    process_copy_adjust = find(
        "ProcessBlacksmithOrders",
        lambda record: record["opcode"] == "subq.b" and record["operands"] == ["#1", "d7"],
    )
    process_copy_loop = find(
        "ProcessBlacksmithOrders",
        lambda record: (
            record["opcode"] == "dbf" and record["branchTarget"] == "@CopyForceMembersList_Loop"
        ),
    )
    readiness_flag_load = find(
        "CountPendingAndReadyToFulfillOrders",
        lambda record: record["opcode"] == "move.w" and record["operands"] == ["#80", "d1"],
    )
    readiness_check = successor("CountPendingAndReadyToFulfillOrders", readiness_flag_load)
    clear_flag_load = find(
        "BlacksmithAction_PlaceOrder",
        lambda record: record["opcode"] == "move.w" and record["operands"] == ["#80", "d1"],
    )
    clear_flag_call = successor("BlacksmithAction_PlaceOrder", clear_flag_load)
    readiness_flag = int(readiness_flag_load["operands"][0][1:])
    clear_flag = int(clear_flag_load["operands"][0][1:])
    if (
        readiness_flag != clear_flag
        or readiness_check["directTarget"] != "j_CheckFlag"
        or clear_flag_call["directTarget"] != "j_ClearFlag"
    ):
        raise ValueError("blacksmith flag-80 use-site relation drift")
    fulfill_add = find(
        "BlacksmithAction_FulfillOrder",
        lambda record: record["directTarget"] == "j_AddItem",
    )
    fulfill_increment = find(
        "BlacksmithAction_FulfillOrder",
        lambda record: (
            record["opcode"] == "addi.w"
            and record["operands"] == ["#1", "fulfilledOrdersNumber(a6)"]
        ),
    )
    fulfill_equip = [
        record
        for record in operations["BlacksmithAction_FulfillOrder"]
        if record["directTarget"] == "j_EquipItemBySlot"
    ][-1]
    fulfill_cancel = find(
        "BlacksmithAction_FulfillOrder",
        lambda record: record["opcode"] == "cmpi.w" and record["operands"] == ["#-1", "d0"],
    )
    fulfill_curse = [
        record
        for record in operations["BlacksmithAction_FulfillOrder"]
        if record["opcode"] == "cmpi.w" and record["operands"] == ["#2", "d2"]
    ][-1]
    fulfill_inventory = inventory_compare
    fulfill_equipment_type = find(
        "BlacksmithAction_FulfillOrder",
        lambda record: (
            record["opcode"] == "cmpi.w" and record["operands"] == ["#EQUIPMENTTYPE_TOOL", "d2"]
        ),
    )
    fulfill_equippability = after(
        "BlacksmithAction_FulfillOrder",
        fulfill_equipment_type,
        lambda record: record["directTarget"] == "j_IsWeaponOrRingEquippable",
    )
    fulfill_post_add_equippability = after(
        "BlacksmithAction_FulfillOrder",
        fulfill_increment,
        lambda record: record["directTarget"] == "j_IsWeaponOrRingEquippable",
    )
    fulfill_optional_prompt = after(
        "BlacksmithAction_FulfillOrder",
        fulfill_post_add_equippability,
        lambda record: record["directTarget"] == "j_alt_YesNoPrompt",
    )
    fulfill_optional_confirmation = after(
        "BlacksmithAction_FulfillOrder",
        fulfill_optional_prompt,
        lambda record: record["opcode"] == "cmpi.w" and record["operands"] == ["#0", "d0"],
    )
    fulfill_weapon_unequip = after(
        "BlacksmithAction_FulfillOrder",
        fulfill_optional_confirmation,
        lambda record: record["directTarget"] == "j_UnequipItemBySlotIfNotCursed",
    )
    fulfill_weapon_curse = after(
        "BlacksmithAction_FulfillOrder",
        fulfill_weapon_unequip,
        lambda record: record["opcode"] == "cmpi.w" and record["operands"] == ["#2", "d2"],
    )
    fulfill_ring_unequip = after(
        "BlacksmithAction_FulfillOrder",
        fulfill_weapon_curse,
        lambda record: record["directTarget"] == "j_UnequipItemBySlotIfNotCursed",
    )
    fulfill_ring_curse = after(
        "BlacksmithAction_FulfillOrder",
        fulfill_ring_unequip,
        lambda record: record["opcode"] == "cmpi.w" and record["operands"] == ["#2", "d2"],
    )
    place_material_cancel = find(
        "BlacksmithAction_PlaceOrder",
        lambda record: record["opcode"] == "cmpi.w" and record["operands"] == ["#-1", "d0"],
    )
    place_customer_cancel = after(
        "BlacksmithAction_PlaceOrder",
        mithril_compare,
        lambda record: record["opcode"] == "cmpi.w" and record["operands"] == ["#-1", "d0"],
    )
    place_eligibility_call = after(
        "BlacksmithAction_PlaceOrder",
        promotion_compare,
        lambda record: record["directTarget"] == "IsClassBlacksmithEligible",
    )
    place_eligibility_result = after(
        "BlacksmithAction_PlaceOrder",
        place_eligibility_call,
        lambda record: record["opcode"] == "cmpi.w" and record["operands"] == ["#0", "d0"],
    )
    place_confirmation_prompt = after(
        "BlacksmithAction_PlaceOrder",
        place_eligibility_result,
        lambda record: record["directTarget"] == "j_alt_YesNoPrompt",
    )
    place_confirmation_result = after(
        "BlacksmithAction_PlaceOrder",
        place_confirmation_prompt,
        lambda record: record["opcode"] == "cmpi.w" and record["operands"] == ["#0", "d0"],
    )
    place_capacity = find(
        "BlacksmithAction_PlaceOrder",
        lambda record: (
            record["opcode"] == "cmpi.w"
            and record["operands"] == ["#BLACKSMITH_MAX_ORDERS_NUMBER", "d0"]
        ),
    )
    prefix_load = find(
        "IsClassBlacksmithEligible",
        lambda record: record["opcode"] == "move.w" and record["operands"] == ["(a0)+", "d7"],
    )
    prefix_decrement = successor("IsClassBlacksmithEligible", prefix_load)
    prefix_loop = find(
        "IsClassBlacksmithEligible",
        lambda record: record["opcode"] == "dbf" and record["branchTarget"] == "@Loop",
    )
    picker_reads = [
        record
        for record in operations["PickMithrilWeapon"]
        if record["opcode"] == "move.b" and record["operands"][0] == "(a0)+"
    ]
    order_slot_empty = find(
        "PickMithrilWeapon",
        lambda record: record["opcode"] == "cmpi.w" and record["operands"] == ["#0", "(a0)"],
    )
    order_slot_write = find(
        "PickMithrilWeapon",
        lambda record: record["opcode"] == "move.w" and record["operands"] == ["d1", "(a0)"],
    )
    group_prefix_read = find(
        "PickMithrilWeapon",
        lambda record: record["opcode"] == "move.w" and record["operands"] == ["(a0)+", "d6"],
    )
    group_prefix_decrement = successor("PickMithrilWeapon", group_prefix_read)
    class_read = after(
        "PickMithrilWeapon",
        group_prefix_decrement,
        lambda record: record["opcode"] == "move.w" and record["operands"] == ["(a0)+", "d1"],
    )
    character_class_read = successor("PickMithrilWeapon", class_read)
    class_compare = successor("PickMithrilWeapon", character_class_read)
    class_match_branch = successor("PickMithrilWeapon", class_compare)
    class_inner_loop = after(
        "PickMithrilWeapon",
        class_match_branch,
        lambda record: (
            record["opcode"] == "dbf" and record["branchTarget"] == "@FindCharacterClass_Loop"
        ),
    )
    group_index_increment = successor("PickMithrilWeapon", class_inner_loop)
    group_outer_loop = successor("PickMithrilWeapon", group_index_increment)
    weighted_parameter_read = after(
        "PickMithrilWeapon",
        weapon_counter_record,
        lambda record: record["opcode"] == "move.b" and record["operands"] == ["(a0)+", "d0"],
    )
    weighted_item_read = successor("PickMithrilWeapon", weighted_parameter_read)
    weighted_parameter_range = successor("PickMithrilWeapon", weighted_item_read)
    weighted_rng_call = successor("PickMithrilWeapon", weighted_parameter_range)
    weighted_result_compare = successor("PickMithrilWeapon", weighted_rng_call)
    weighted_result_branch = successor("PickMithrilWeapon", weighted_result_compare)
    weighted_loop = successor("PickMithrilWeapon", weighted_result_branch)
    order_slot_occupied_branch = successor("PickMithrilWeapon", order_slot_empty)
    order_slot_stride_load = after(
        "PickMithrilWeapon",
        order_slot_write,
        lambda record: record["opcode"] == "move.w" and record["operands"] == ["#2", "d0"],
    )
    order_slot_stride_add = successor("PickMithrilWeapon", order_slot_stride_load)
    order_slot_loop = successor("PickMithrilWeapon", order_slot_stride_add)
    parameter_item_pairs = re.findall(
        r"^\s*(?:mithrilWeapons\s+)?(\d+),\s*([A-Z][A-Z0-9_]*)",
        weapons,
        re.MULTILINE,
    )
    parameter_rows = [
        [int(parameter) for parameter, _item in parameter_item_pairs[index : index + 4]]
        for index in range(0, len(parameter_item_pairs), 4)
    ]
    denominators = parameter_rows[0] if parameter_rows else []
    if (
        process_copy_counter_source["operands"][0] != process_copy_length["operands"][0]
        or process_copy_counter_source["operands"][1] != "d7"
        or process_copy_loop["opcode"] != "dbf"
        or prefix_decrement["opcode"] != "subq.w"
        or prefix_loop["opcode"] != "dbf"
        or len(picker_reads) != 2
        or initial_group_record["opcode"] != "clr.w"
        or fallback_row_record["operands"] != ["#2", "d0"]
        or group_prefix_decrement["operands"] != ["#1", "d6"]
        or class_compare["operands"] != ["d1", "d2"]
        or class_inner_loop["operands"] != ["d6", "@FindCharacterClass_Loop"]
        or group_index_increment["operands"] != ["#1", "d0"]
        or group_outer_loop["operands"] != ["d7", "@FindWeaponClass_Loop"]
        or weighted_parameter_range["operands"] != ["d0", "d6"]
        or weighted_rng_call["directTarget"] != "GenerateRandomNumber"
        or weighted_result_compare["operands"] != ["#0", "d7"]
        or weighted_result_branch["branchTarget"] != "@LoadIndex"
        or weighted_loop["operands"] != ["d5", "@PickWeapon_Loop"]
        or order_slot_occupied_branch["branchTarget"] != "@Next"
        or order_slot_stride_add["operands"] != ["d0", "a0"]
        or order_slot_loop["operands"] != ["d7", "@LoadIndex_Loop"]
        or len(parameter_item_pairs) != len(weapon_rows) * (weapon_counter + 1)
        or parameter_rows != [denominators] * len(weapon_rows)
        or denominators != [16, 8, 4, 1]
    ):
        raise ValueError("blacksmith copy/list/picker use-site relation drift")
    targets = set(operation_sources)
    disasm = root.parents[2]
    aliases = _shop_jump_aliases(disasm, targets)
    alias_targets = {alias: fact["effectiveTarget"] for alias, fact in aliases.items()}
    source_paths = tuple(
        root / path
        for path in ("blacksmith/blacksmithactions.asm", "blacksmith/pickmithrilweapon.asm")
    )
    internal = {
        path.relative_to(disasm).as_posix(): _shop_direct_call_occurrences(
            path, alias_targets, targets
        )
        for path in source_paths
    }
    external = {
        path.relative_to(disasm).as_posix(): occurrences
        for path in sorted((disasm / "code").rglob("*.asm"), key=lambda value: value.as_posix())
        if path not in source_paths
        if (occurrences := _shop_direct_call_occurrences(path, alias_targets, targets))
    }

    def effective_counts(callers: dict[str, list[dict[str, Any]]]) -> dict[str, int]:
        return {
            target: sum(
                occurrence["siteCount"]
                for occurrences in callers.values()
                for occurrence in occurrences
                if occurrence["effectiveTarget"] == target
            )
            for target in sorted(targets)
        }

    return {
        "entrySymbol": "BlacksmithMenu",
        "sourcePaths": [
            "code/common/menus/blacksmith/blacksmithactions.asm",
            "code/common/menus/blacksmith/pickmithrilweapon.asm",
        ],
        "constants": constants,
        "sourceRanges": [
            _caravan_range(actions, "code/common/menus/blacksmith/blacksmithactions.asm"),
            _caravan_range(picker, "code/common/menus/blacksmith/pickmithrilweapon.asm"),
        ],
        "frame": {
            "frameSizeBytes": frame_size,
            "counterOffsetsBytes": counter_offsets,
            "initializationOrder": initialization_order,
        },
        "derived": {
            "process": {
                "forceCopy": {
                    "sourceLengthOperand": process_copy_length["operands"][0],
                    "destinationLengthOperand": process_copy_length["operands"][1],
                    "counterSource": process_copy_counter_source,
                    "counterSourceOperand": process_copy_counter_source["operands"][0],
                    "counterDestination": process_copy_counter_source["operands"][1],
                    "entryCopyOpcode": process_copy_width["opcode"],
                    "entryCopyOperands": process_copy_width["operands"],
                    "counterAdjustment": process_copy_adjust,
                    "counterLoopOpcode": process_copy_loop["opcode"],
                    "loopTarget": process_copy_loop["branchTarget"],
                },
                "readiness": {
                    "flagId": readiness_flag,
                    "checkFlagLoad": readiness_flag_load,
                    "checkCall": readiness_check["directTarget"],
                    "clearFlagLoad": clear_flag_load,
                    "clearCall": clear_flag_call["directTarget"],
                },
            },
            "orders": {
                "maximumSlots": max_orders,
                "inclusiveCounter": order_counter,
                "slotWidthBytes": order_slot_width_bytes,
                "storageWriteOpcode": storage_clear_record["opcode"],
            },
            "fulfill": {
                "inventoryCapacity": inventory_capacity,
                "inventoryFullBranchOpcode": successor(
                    "BlacksmithAction_FulfillOrder", inventory_compare
                )["opcode"],
                "mutationCalls": calls(
                    "BlacksmithAction_FulfillOrder", {"j_AddItem", "j_EquipItemBySlot"}
                ),
                "orderedMutationSequence": [
                    fulfill_add,
                    storage_clear_record,
                    fulfill_increment,
                    fulfill_equip,
                ],
                "branchSequence": [
                    conditional_branch(
                        "recipientCancel", "BlacksmithAction_FulfillOrder", fulfill_cancel
                    ),
                    conditional_branch(
                        "inventoryCapacity", "BlacksmithAction_FulfillOrder", fulfill_inventory
                    ),
                    conditional_branch(
                        "equipmentType", "BlacksmithAction_FulfillOrder", fulfill_equipment_type
                    ),
                    conditional_branch(
                        "equippability", "BlacksmithAction_FulfillOrder", fulfill_equippability
                    ),
                    conditional_branch(
                        "optionalEquipEligibility",
                        "BlacksmithAction_FulfillOrder",
                        fulfill_post_add_equippability,
                    ),
                    conditional_branch(
                        "optionalEquipConfirmation",
                        "BlacksmithAction_FulfillOrder",
                        fulfill_optional_confirmation,
                    ),
                    conditional_branch(
                        "weaponCurseRejection",
                        "BlacksmithAction_FulfillOrder",
                        fulfill_weapon_curse,
                    ),
                    conditional_branch(
                        "ringCurseRejection", "BlacksmithAction_FulfillOrder", fulfill_ring_curse
                    ),
                    conditional_branch(
                        "newlyEquippedCurseOutcome",
                        "BlacksmithAction_FulfillOrder",
                        fulfill_curse,
                    ),
                ],
            },
            "place": {
                "mithrilItem": mithril_item,
                "promotionClassFloor": promotion_class,
                "orderCost": order_cost,
                "goldBranchTarget": successor("BlacksmithAction_PlaceOrder", gold_compare)[
                    "branchTarget"
                ],
                "mutationCalls": calls(
                    "BlacksmithAction_PlaceOrder",
                    {"j_DecreaseGold", "j_DropItemBySlot", "PickMithrilWeapon", "j_ClearFlag"},
                ),
                "gateSequence": [
                    conditional_branch(
                        "materialSelectionCancel",
                        "BlacksmithAction_PlaceOrder",
                        place_material_cancel,
                    ),
                    conditional_branch(
                        "mithrilMatch", "BlacksmithAction_PlaceOrder", mithril_compare
                    ),
                    conditional_branch(
                        "customerSelectionCancel",
                        "BlacksmithAction_PlaceOrder",
                        place_customer_cancel,
                    ),
                    conditional_branch(
                        "promotionFloor", "BlacksmithAction_PlaceOrder", promotion_compare
                    ),
                    conditional_branch(
                        "eligibilityResult",
                        "BlacksmithAction_PlaceOrder",
                        place_eligibility_result,
                    ),
                    conditional_branch(
                        "confirmationResult",
                        "BlacksmithAction_PlaceOrder",
                        place_confirmation_result,
                    ),
                    conditional_branch(
                        "goldComparison", "BlacksmithAction_PlaceOrder", gold_compare
                    ),
                ],
                "postPlacementCapacityBranch": conditional_branch(
                    "postPlacementCapacity",
                    "BlacksmithAction_PlaceOrder",
                    place_capacity,
                ),
                "orderedMutationSequence": [
                    find(
                        "BlacksmithAction_PlaceOrder",
                        lambda record: record["directTarget"] == "j_DecreaseGold",
                    ),
                    find(
                        "BlacksmithAction_PlaceOrder",
                        lambda record: record["directTarget"] == "j_DropItemBySlot",
                    ),
                    find(
                        "BlacksmithAction_PlaceOrder",
                        lambda record: record["directTarget"] == "PickMithrilWeapon",
                    ),
                    clear_flag_load,
                    clear_flag_call,
                ],
            },
            "classLists": {
                "eligibleClassCount": len(eligible_classes.group(1).split(",")),
                "eligiblePrefix": {
                    "loadOpcode": prefix_load["opcode"],
                    "decrementOpcode": prefix_decrement["opcode"],
                    "loopTarget": prefix_loop["branchTarget"],
                },
                "mithrilWeaponClassGroups": len(class_groups),
                "weaponRows": len(weapon_rows),
            },
            "pick": {
                "classGroupInclusiveCounter": class_counter,
                "weightedRowInclusiveCounter": weapon_counter,
                "rowStrideBytes": row_stride_bytes,
                "initialGroupRowIndexOpcode": initial_group_record["opcode"],
                "initialGroupRowIndex": 0,
                "fallbackRandomBound": fallback_random_bound,
                "fallback": {
                    "compareValue": int(fallback_compare["operands"][0][1:]),
                    "branchOpcode": fallback_branch["opcode"],
                    "nonzeroTarget": fallback_branch["branchTarget"],
                    "zeroResultRowIndex": int(fallback_row_record["operands"][0][1:]),
                    "convergenceLabel": fallback_convergence_record["labels"][0],
                },
                "classGroupScan": {
                    "prefixRead": group_prefix_read,
                    "prefixDecrement": group_prefix_decrement,
                    "classRead": class_read,
                    "characterClassRead": character_class_read,
                    "classCompare": class_compare,
                    "classMatchBranch": class_match_branch,
                    "innerLoop": class_inner_loop,
                    "groupIndexIncrement": group_index_increment,
                    "outerLoop": group_outer_loop,
                },
                "weightedRngLoop": {
                    "parameterRead": weighted_parameter_read,
                    "itemRead": weighted_item_read,
                    "parameterToRngRange": weighted_parameter_range,
                    "rngCall": weighted_rng_call,
                    "resultCompare": weighted_result_compare,
                    "resultBranch": weighted_result_branch,
                    "loop": weighted_loop,
                    "parameterColumnDenominators": {
                        "owner": "item-auxiliary",
                        "sourcePath": "data/stats/items/mithrilweapons.asm",
                        "values": denominators[:4],
                    },
                },
                "orderSlot": {
                    "emptyCompare": order_slot_empty,
                    "occupiedBranch": order_slot_occupied_branch,
                    "scanCounter": order_counter,
                    "strideLoad": order_slot_stride_load,
                    "strideAdd": order_slot_stride_add,
                    "loop": order_slot_loop,
                    "write": order_slot_write,
                },
            },
        },
        "functionOperations": operations,
        "jumpInterfaceAliases": aliases,
        "internalDirectCallerOccurrences": internal,
        "internalEffectiveDirectCallSiteCounts": effective_counts(internal),
        "externalDirectCallerOccurrences": external,
        "externalEffectiveDirectCallSiteCounts": effective_counts(external),
        "indirectBehavior": {"directCallInventoryDoesNotEstablishReachability": True},
    }


def _shared_selection_screen_contract(disasm: Path, root: Path) -> dict[str, Any]:
    """Parse the shared shop/caravan selection screen as source-shaped static facts."""
    source_path = root / "shopscreen.asm"
    source = read_upstream_text(source_path)
    enums = read_upstream_text(disasm / "sf2enums.asm")
    copy_bytes_source = read_upstream_text(disasm / "code/common/tech/bytecopy.asm")
    if "; In: a0 = Source, a1 = Destination, d7.w = Length" not in copy_bytes_source:
        raise ValueError("shared selection CopyBytes ABI drift")
    _require_ordered_shop_section(
        source_path,
        "ExecuteShopScreen:",
        "LoadShopInventoryHighlightSprites:",
        (
            "mulu.w  #6,d0",
            "btst    #INPUT_BIT_RIGHT,((CURRENT_PLAYER_INPUT-$1000000)).w",
            "btst    #INPUT_BIT_LEFT,((CURRENT_PLAYER_INPUT-$1000000)).w",
            "btst    #INPUT_BIT_UP,((CURRENT_PLAYER_INPUT-$1000000)).w",
            "btst    #INPUT_BIT_DOWN,((CURRENT_PLAYER_INPUT-$1000000)).w",
            "btst    #INPUT_BIT_B,((CURRENT_PLAYER_INPUT-$1000000)).w",
            "bne.w   @Cancel",
            "btst    #INPUT_BIT_C,((CURRENT_PLAYER_INPUT-$1000000)).w",
            "bne.w   @Confirm",
            "btst    #INPUT_BIT_A,((CURRENT_PLAYER_INPUT-$1000000)).w",
            "bne.w   @Confirm",
            "moveq   #-1,d0",
            "mulu.w  #ITEMS_PER_SHOP_PAGE,d1",
            "move.b  (a0,d1.w),d0",
        ),
    )
    _require_ordered_shop_section(
        source_path,
        "LoadShopInventoryHighlightSprites:",
        "WriteGoldAmount:",
        (
            "lsl.w   #5,d0",
            "addi.w  #156,d0",
            "addq.l  #VDP_SPRITE_ENTRY_SIZE,a0",
            "mulu.w  #ITEMS_PER_SHOP_PAGE,d0",
        ),
    )
    _require_ordered_shop_section(
        source_path,
        "@CheckLeft:",
        "@CheckUp:",
        (
            "btst    #INPUT_BIT_LEFT,((CURRENT_PLAYER_INPUT-$1000000)).w",
            "ble.s   @CheckUp",
            "move.w  #5,((CURRENT_SHOP_SELECTION-$1000000)).w",
            "move.b  #1,((WINDOW_LAYOUT_SHIFT_DIRECTION-$1000000)).w",
            "bra.w   @loc_12",
        ),
    )
    _require_ordered_shop_section(
        source_path,
        "@CheckDown:",
        "@loc_11:",
        (
            "btst    #INPUT_BIT_DOWN,((CURRENT_PLAYER_INPUT-$1000000)).w",
            "mulu.w  #ITEMS_PER_SHOP_PAGE,d2",
            "ble.s   @loc_8",
            "moveq   #ITEMS_PER_SHOP_PAGE,d1",
            "move.w  d1,((CURRENT_SHOP_PAGE_ITEMS_NUMBER-$1000000)).w",
            "bne.s   @loc_9",
            "clr.b   ((WINDOW_LAYOUT_SHIFT_DIRECTION-$1000000)).w",
        ),
    )
    _require_ordered_shop_section(
        source_path,
        "LoadItemIconsAndPriceTagTiles:",
        "LoadPriceTagTiles:",
        (
            "movea.l inventoryWindowLayoutEndAddress(a6),a1",
            "lea     layout_ShopInventoryWindow(pc), a0",
            "move.w  #324,d7",
            "jsr     (CopyBytes).w",
            "move.w  #1599,d7",
            "move.l  #-1,(a0)+",
            "dbf     d7,@Clear_Loop",
            "dbf     d7,@Main_Loop",
            "move.w  #$3C0,d0",
            "jsr     (ApplyVIntVramDma).w",
        ),
    )
    _require_ordered_shop_section(
        source_path,
        "LoadPriceTagTiles:",
        "LoadIconPixelsInShopScreen:",
        (
            "moveq   #31,d7",
            "move.l  (a1)+,(a0)+",
            "dbf     d7,@LoadBlankTiles_Loop",
            "moveq   #4,d7",
            "dbf     d7,@Main_Loop",
        ),
    )
    _require_ordered_shop_section(
        source_path,
        "WriteItemNameAndGoldAmount:",
        "LoadItemIconsAndPriceTagTiles:",
        (
            "moveq   #-20,d1",
            "bsr.w   WriteTilesFromAsciiWithRegularFont",
            "moveq   #5,d7",
            "jsr     WriteTilesFromNumber",
        ),
    )
    _require_ordered_shop_section(
        source_path,
        "LoadIconPixelsInShopScreen:",
        "GetCurrentShopSelection:",
        (
            "moveq   #ICON_PIXELS_LONGWORD_COUNTER,d7",
            "move.l  (a1)+,(a0)+",
            "dbf     d7,@Loop",
        ),
    )
    _require_ordered_shop_section(
        source_path,
        "ShiftShopInventoryWindowLayout:",
        "sub_14EC0:",
        (
            "tst.b   ((WINDOW_LAYOUT_SHIFT_DIRECTION-$1000000)).w",
            "bne.s   loc_14E82",
            "bra.s   loc_14E86",
            "bne.s   loc_14EAA",
            "bra.s   loc_14EAE",
            "bra.s   MoveSelectedItemInfoWindow",
        ),
    )
    constant_names = (
        "ITEMS_PER_SHOP_PAGE",
        "WINDOW_SHOP_INVENTORY_SIZE",
        "WINDOW_SHOP_INVENTORY_DEST",
        "WINDOW_SHOP_ITEM_NAME_AND_PRICE_SIZE",
        "WINDOW_SHOP_ITEM_NAME_AND_PRICE_DEST",
        "WINDOW_SHOP_GOLD_SIZE",
        "WINDOW_SHOP_GOLD_DEST",
        "ICON_PIXELS_LONGWORD_COUNTER",
    )
    constants: dict[str, int] = {}
    for name in constant_names:
        match = re.search(rf"^{name}:\s+equ\s+(\$[0-9A-Fa-f]+|\d+)", enums, re.MULTILINE)
        if not match:
            raise ValueError(f"shared selection constant drift: {name}")
        raw = match.group(1)
        constants[name] = int(raw[1:], 16) if raw.startswith("$") else int(raw)
    routine_names = re.findall(
        r"^\s*; End of function ([A-Za-z_][A-Za-z0-9_]*).*?$", source, re.MULTILINE
    )

    def routine_section(name: str) -> str:
        start = source.find(f"{name}:")
        end = source.find("\n    ; End of function", start)
        if start < 0 or end < 0:
            raise ValueError(f"shared selection routine boundary drift: {name}")
        return source[start:end]

    routines = {name: _shop_instruction_records(routine_section(name)) for name in routine_names}
    targets = set(routines)
    aliases = _shop_jump_aliases(disasm, targets)
    alias_targets = {alias: fact["effectiveTarget"] for alias, fact in aliases.items()}
    internal = {
        "code/common/menus/shopscreen.asm": _shop_direct_call_occurrences(
            source_path, alias_targets, targets
        )
    }
    external = {
        path.relative_to(disasm).as_posix(): occurrences
        for path in sorted((disasm / "code").rglob("*.asm"), key=lambda value: value.as_posix())
        if path != source_path
        if (occurrences := _shop_direct_call_occurrences(path, alias_targets, targets))
    }

    def totals(callers: dict[str, list[dict[str, Any]]]) -> dict[str, int]:
        return {
            target: sum(
                occurrence["siteCount"]
                for occurrences in callers.values()
                for occurrence in occurrences
                if occurrence["effectiveTarget"] == target
            )
            for target in sorted(targets)
        }

    entry = routines["ExecuteShopScreen"]
    selection = routines["GetCurrentShopSelection"]
    highlight = routines["LoadShopInventoryHighlightSprites"]

    def locate(records: list[dict[str, Any]], predicate) -> dict[str, Any]:
        try:
            return next(record for record in records if predicate(record))
        except StopIteration as error:
            raise ValueError("shared selection parsed use-site drift") from error

    def following(records: list[dict[str, Any]], record: dict[str, Any]) -> dict[str, Any]:
        index = next(index for index, candidate in enumerate(records) if candidate is record)
        return records[index + 1]

    def state_accesses(routine: str) -> list[dict[str, Any]]:
        """Classify source-faithful local/global operands without naming their lifecycle."""
        accesses: list[dict[str, Any]] = []
        for instruction_index, record in enumerate(routines[routine]):
            opcode = record["opcode"]
            for operand_index, operand in enumerate(record["operands"]):
                local = re.search(r"([A-Za-z_][A-Za-z0-9_]*)\(a6\)", operand)
                global_state = re.search(r"\(\(([A-Z][A-Z0-9_]+)-\$1000000\)\)\.w", operand)
                if not local and not global_state:
                    continue
                if opcode.startswith(("cmp", "btst", "tst")):
                    mode = "read"
                elif opcode.startswith("clr"):
                    mode = "write"
                elif opcode.startswith(("add", "sub", "ori", "andi", "lsl", "lsr", "ror")):
                    mode = "readWrite" if operand_index == len(record["operands"]) - 1 else "read"
                elif len(record["operands"]) == 1:
                    mode = "read"
                else:
                    mode = "read" if operand_index == 0 else "write"
                accesses.append(
                    {
                        "fieldName": (local or global_state).group(1),
                        "scope": "stackLocal" if local else "global",
                        "mode": mode,
                        "instructionIndex": instruction_index,
                    }
                )
        return accesses

    def labelled_section(records: list[dict[str, Any]], label: str) -> list[dict[str, Any]]:
        start = next(index for index, record in enumerate(records) if label in record["labels"])
        end = next(
            (
                index
                for index, record in enumerate(records[start + 1 :], start + 1)
                if record["labels"]
            ),
            len(records),
        )
        return records[start:end]

    input_sections = {
        name: labelled_section(entry, name)
        for name in (
            "@CheckRight",
            "@CheckLeft",
            "@CheckUp",
            "@CheckDown",
            "@loc_11",
            "@Cancel",
            "@Confirm",
            "@Exit",
        )
    }
    button_tests = input_sections["@loc_11"]
    cancel_test = locate(
        button_tests,
        lambda record: record["opcode"] == "btst" and "INPUT_BIT_B" in record["operands"][0],
    )
    confirm_c_test = locate(
        button_tests,
        lambda record: record["opcode"] == "btst" and "INPUT_BIT_C" in record["operands"][0],
    )
    confirm_a_test = locate(
        button_tests,
        lambda record: record["opcode"] == "btst" and "INPUT_BIT_A" in record["operands"][0],
    )
    cancel_result = locate(
        input_sections["@Cancel"],
        lambda record: record["opcode"] == "moveq" and record["operands"] == ["#-1", "d0"],
    )
    selection_page_load = selection[0]
    selection_scale = selection[1:5]
    selection_final_read = locate(
        selection,
        lambda record: record["opcode"] == "move.b" and record["operands"] == ["(a0,d0.w)", "d0"],
    )
    highlight_selection_shift = locate(
        highlight,
        lambda record: record["opcode"] == "lsl.w" and record["operands"] == ["#5", "d0"],
    )
    highlight_coordinate_add = following(highlight, highlight_selection_shift)
    highlight_sprite_stride = locate(
        highlight,
        lambda record: (
            record["opcode"] == "addq.l" and record["operands"] == ["#VDP_SPRITE_ENTRY_SIZE", "a0"]
        ),
    )
    highlight_page_bound = locate(
        highlight,
        lambda record: (
            record["opcode"] == "mulu.w" and record["operands"] == ["#ITEMS_PER_SHOP_PAGE", "d0"]
        ),
    )
    helper_flows = {
        name: {
            "directCalls": [record for record in routines[name] if record["directTarget"]],
            "branches": [record for record in routines[name] if record["branchTarget"]],
            "terminalOperation": routines[name][-1],
        }
        for name in (
            "sub_14D0C",
            "sub_14D6A",
            "sub_14DBE",
            "sub_14DC0",
            "sub_14E06",
            "sub_14E5E",
            "ShiftShopInventoryWindowLayout",
            "sub_14EC0",
            "MoveSelectedItemInfoWindow",
        )
    }

    def indexes(routine: str, records: list[dict[str, Any]]) -> list[int]:
        corpus = routines[routine]
        return [
            next(index for index, candidate in enumerate(corpus) if candidate is record)
            for record in records
        ]

    def index(routine: str, record: dict[str, Any]) -> int:
        return indexes(routine, [record])[0]

    def named_reference(name: str, routine: str, record: dict[str, Any]) -> dict[str, Any]:
        return {"name": name, "instructionIndex": index(routine, record)}

    def named_entry_references(specification: list[tuple[str, int]]) -> list[dict[str, Any]]:
        return [
            named_reference(name, "ExecuteShopScreen", entry[instruction_index])
            for name, instruction_index in specification
        ]

    navigation_indexes = {
        "right": {
            "routine": "ExecuteShopScreen",
            "references": named_entry_references(
                [
                    ("selectedIndexCandidateLoad", 48),
                    ("inputTest", 49),
                    ("inputAbsentBranch", 50),
                    ("pageLoad", 51),
                    ("pageScale", 52),
                    ("globalListCandidateAdd", 53),
                    ("globalListCandidateIncrement", 54),
                    ("globalListLengthCompare", 55),
                    ("globalListBoundBranch", 56),
                    ("selectedIndexIncrement", 57),
                    ("pageLocalCountCompare", 59),
                    ("pageLocalBoundBranch", 60),
                    ("pageIncrement", 61),
                    ("selectionReset", 62),
                    ("shiftDirectionReset", 63),
                    ("scrollHelperCall", 64),
                    ("scrollHelperConvergence", 65),
                    ("selectionStore", 66),
                    ("partialPageHelperCall", 67),
                    ("partialPageConvergence", 68),
                ]
            ),
        },
        "left": {
            "routine": "ExecuteShopScreen",
            "references": named_entry_references(
                [
                    ("selectedIndexCandidateLoad", 69),
                    ("inputTest", 70),
                    ("inputAbsentBranch", 71),
                    ("pageLoad", 72),
                    ("pageScale", 73),
                    ("globalListCandidateAdd", 74),
                    ("globalListBoundBranch", 75),
                    ("selectedIndexDecrement", 76),
                    ("pageLocalBoundBranch", 78),
                    ("pageDecrement", 79),
                    ("selectionReset", 80),
                    ("shiftDirectionSet", 81),
                    ("scrollHelperCall", 82),
                    ("scrollHelperConvergence", 83),
                    ("selectionStore", 84),
                    ("partialPageHelperCall", 85),
                    ("partialPageConvergence", 86),
                ]
            ),
        },
        "up": {
            "routine": "ExecuteShopScreen",
            "references": named_entry_references(
                [
                    ("inputTest", 87),
                    ("inputAbsentBranch", 88),
                    ("pageZeroTest", 89),
                    ("pageZeroBoundBranch", 90),
                    ("pageDecrement", 91),
                    ("shiftDirectionSet", 93),
                    ("scrollHelperConvergence", 94),
                ]
            ),
        },
        "down": {
            "routine": "ExecuteShopScreen",
            "references": named_entry_references(
                [
                    ("inputTest", 95),
                    ("inputAbsentBranch", 96),
                    ("nextPageCandidateLoad", 97),
                    ("nextPageCandidateIncrement", 98),
                    ("nextPageScale", 99),
                    ("globalListLengthCompare", 100),
                    ("globalListBoundBranch", 101),
                    ("pageIncrement", 102),
                    ("selectedIndexLoad", 104),
                    ("pageLoadForPartialCount", 105),
                    ("partialPageScaleCopy", 106),
                    ("partialPageScaleDouble", 107),
                    ("partialPageScaleAdd", 108),
                    ("partialPageScaleDoubleFinal", 109),
                    ("globalListLengthLoad", 110),
                    ("partialPageLengthSubtract", 111),
                    ("partialPageCountCompare", 112),
                    ("partialPageBoundBranch", 113),
                    ("partialPageCountCap", 114),
                    ("pageItemCountStore", 115),
                    ("selectionClampCompare", 116),
                    ("selectionClampBranch", 117),
                    ("selectionClampDecrement", 118),
                    ("selectionClampLoop", 119),
                    ("selectionStore", 120),
                    ("shiftDirectionReset", 121),
                    ("scrollHelperConvergence", 122),
                ]
            ),
        },
    }
    helper_indexes = {
        name: {
            "routine": name,
            "stateAccesses": state_accesses(name),
            "directCalls": [
                named_reference(record["directTarget"], name, record)
                for record in facts["directCalls"]
            ],
            "branches": [
                named_reference(record["branchTarget"], name, record)
                for record in facts["branches"]
            ],
            "terminalConvergence": named_reference(
                "terminalOperation", name, facts["terminalOperation"]
            ),
        }
        for name, facts in helper_flows.items()
    }

    def immediate_value(record: dict[str, Any]) -> int:
        raw = record["operands"][0].removeprefix("#")
        if raw in constants:
            return constants[raw]
        return int(raw[1:], 16) if raw.startswith("$") else int(raw)

    def transfer_reference(
        name: str,
        routine: str,
        *,
        source_index: int,
        destination_index: int,
        count_index: int | None = None,
        call_index: int | None = None,
        write_index: int | None = None,
        loop_index: int | None = None,
        exit_index: int | None = None,
    ) -> dict[str, Any]:
        fact: dict[str, Any] = {
            "name": name,
            "sourceOperandInstructionIndex": source_index,
            "destinationOperandInstructionIndex": destination_index,
        }
        if count_index is not None:
            fact["storedCountOperandInstructionIndex"] = count_index
            count_operand = routines[routine][count_index]["operands"][0]
            if count_operand.startswith("#"):
                fact["storedCountValue"] = immediate_value(routines[routine][count_index])
            else:
                fact["storedCountOperand"] = count_operand
        if call_index is not None:
            fact["copyCallInstructionIndex"] = call_index
            fact["copyCountUnit"] = "bytes"
            if "storedCountValue" in fact:
                fact["transferredByteCount"] = fact["storedCountValue"]
        if write_index is not None:
            fact["writeInstructionIndex"] = write_index
            fact["writeOpcodeWidthBits"] = {"move.b": 8, "move.w": 16, "move.l": 32}[
                routines[routine][write_index]["opcode"]
            ]
        if loop_index is not None:
            fact["loopInstructionIndex"] = loop_index
            fact["inclusiveCounter"] = True
            if "storedCountValue" in fact:
                fact["iterationCount"] = fact["storedCountValue"] + 1
                if fact.get("writeOpcodeWidthBits") == 32:
                    fact["longwordWriteCount"] = fact["iterationCount"]
            fact["loopTarget"] = routines[routine][loop_index]["branchTarget"]
        if exit_index is not None:
            fact["exitConvergenceInstructionIndex"] = exit_index
        return fact

    resource_indexes = {
        "WriteGoldAmount": {
            "routine": "WriteGoldAmount",
            "namedOperations": [
                {
                    "name": "goldLabelText",
                    "sourceOperandInstructionIndex": 3,
                    "destinationOperandInstructionIndex": 4,
                    "storedCountOperandInstructionIndex": 7,
                    "storedCountValue": immediate_value(routines["WriteGoldAmount"][7]),
                    "writeCallInstructionIndex": 8,
                },
                {
                    "name": "goldNumber",
                    "sourceOperandInstructionIndex": 9,
                    "destinationOperandInstructionIndex": 11,
                    "storedCountOperandInstructionIndex": 13,
                    "storedCountValue": immediate_value(routines["WriteGoldAmount"][13]),
                    "writeCallInstructionIndex": 14,
                },
            ],
            "terminalConvergence": named_reference(
                "terminalOperation", "WriteGoldAmount", routines["WriteGoldAmount"][-1]
            ),
        },
        "WriteItemNameAndGoldAmount": {
            "routine": "WriteItemNameAndGoldAmount",
            "namedOperations": [
                {
                    "name": "itemNameText",
                    "sourceOperandInstructionIndex": 6,
                    "destinationOperandInstructionIndex": 7,
                    "preCallD1ArgumentInstructionIndex": 9,
                    "preCallD1ArgumentValue": immediate_value(
                        routines["WriteItemNameAndGoldAmount"][9]
                    ),
                    "writeCallInstructionIndex": 10,
                },
                {
                    "name": "itemPriceNumber",
                    "sourceOperandInstructionIndex": 14,
                    "destinationOperandInstructionIndex": 15,
                    "storedCountOperandInstructionIndex": 17,
                    "storedCountValue": immediate_value(routines["WriteItemNameAndGoldAmount"][17]),
                    "writeCallInstructionIndex": 18,
                },
            ],
            "terminalConvergence": named_reference(
                "terminalOperation",
                "WriteItemNameAndGoldAmount",
                routines["WriteItemNameAndGoldAmount"][-1],
            ),
        },
        "LoadItemIconsAndPriceTagTiles": {
            "routine": "LoadItemIconsAndPriceTagTiles",
            "copyBytesTransfers": [
                transfer_reference(
                    "inventoryLayoutCopy",
                    "LoadItemIconsAndPriceTagTiles",
                    source_index=1,
                    destination_index=0,
                    count_index=2,
                    call_index=3,
                ),
            ],
            "loopWrites": [
                transfer_reference(
                    "clearLoop",
                    "LoadItemIconsAndPriceTagTiles",
                    source_index=7,
                    destination_index=7,
                    count_index=6,
                    write_index=7,
                    loop_index=8,
                    exit_index=9,
                ),
                transfer_reference(
                    "itemLoop",
                    "LoadItemIconsAndPriceTagTiles",
                    source_index=28,
                    destination_index=38,
                    count_index=22,
                    write_index=38,
                    loop_index=41,
                    exit_index=42,
                ),
            ],
            "vintDmaArgumentInstructionIndexes": [42, 43, 44, 45, 46, 47],
            "terminalConvergence": named_reference(
                "terminalOperation",
                "LoadItemIconsAndPriceTagTiles",
                routines["LoadItemIconsAndPriceTagTiles"][-1],
            ),
        },
        "LoadPriceTagTiles": {
            "routine": "LoadPriceTagTiles",
            "loopWrites": [
                transfer_reference(
                    "blankTileLongwordLoop",
                    "LoadPriceTagTiles",
                    source_index=5,
                    destination_index=5,
                    count_index=4,
                    write_index=5,
                    loop_index=6,
                    exit_index=7,
                ),
                transfer_reference(
                    "digitCharacterLoop",
                    "LoadPriceTagTiles",
                    source_index=13,
                    destination_index=20,
                    count_index=11,
                    write_index=20,
                    loop_index=27,
                    exit_index=28,
                ),
            ],
            "digitTileLongwordWriteInstructionIndexes": [20, 21, 22, 23, 24],
            "digitTileLongwordCountPerNonSpaceCharacter": 5,
            "terminalConvergence": named_reference(
                "terminalOperation", "LoadPriceTagTiles", routines["LoadPriceTagTiles"][-1]
            ),
        },
        "LoadIconPixelsInShopScreen": {
            "routine": "LoadIconPixelsInShopScreen",
            "loopWrites": [
                transfer_reference(
                    "iconPixelLongwordLoop",
                    "LoadIconPixelsInShopScreen",
                    source_index=9,
                    destination_index=9,
                    count_index=8,
                    write_index=9,
                    loop_index=10,
                    exit_index=11,
                ),
            ],
            "terminalConvergence": named_reference(
                "terminalOperation",
                "LoadIconPixelsInShopScreen",
                routines["LoadIconPixelsInShopScreen"][-1],
            ),
        },
    }
    if (
        constants["ITEMS_PER_SHOP_PAGE"] != 6
        or not any(record["operands"] == ["#-1", "d0"] for record in entry)
        or not any(record["operands"] == ["#BYTE_MASK", "d0"] for record in selection)
    ):
        raise ValueError("shared selection entry/selection relation drift")
    return {
        "entrySymbol": "ExecuteShopScreen",
        "sourcePath": "code/common/menus/shopscreen.asm",
        "sourceRange": _caravan_range(source, "code/common/menus/shopscreen.asm"),
        "constants": constants,
        "routineOperations": routines,
        "entryStateAndWindowFlow": {
            "routine": "ExecuteShopScreen",
            "preflightIndexes": list(range(7)),
            "windowCreationAndOpenIndexes": list(
                range(7, entry.index(input_sections["@CheckRight"][0]))
            ),
            "cleanupAndReturnIndexes": indexes("ExecuteShopScreen", input_sections["@Exit"]),
        },
        "inputBranches": {
            "sourceOrder": ["cancel", "confirmC", "confirmA"],
            "cancel": {
                "button": "B",
                "testIndex": index("ExecuteShopScreen", cancel_test),
                "branchIndex": index("ExecuteShopScreen", following(button_tests, cancel_test)),
            },
            "confirmC": {
                "button": "C",
                "testIndex": index("ExecuteShopScreen", confirm_c_test),
                "branchIndex": index("ExecuteShopScreen", following(button_tests, confirm_c_test)),
            },
            "confirmA": {
                "button": "A",
                "testIndex": index("ExecuteShopScreen", confirm_a_test),
                "branchIndex": index("ExecuteShopScreen", following(button_tests, confirm_a_test)),
            },
            "cancelResult": named_reference("minusOneResult", "ExecuteShopScreen", cancel_result),
            "confirmSelectionFormula": {
                "routine": "ExecuteShopScreen",
                "pageLoadIndex": 134,
                "pageMultiplierIndex": 135,
                "selectionAddIndex": 136,
                "listBaseIndex": 137,
                "resultByteReadIndex": 138,
                "resultMaskIndex": 139,
            },
        },
        "navigation": navigation_indexes,
        "selectionFormula": {
            "routine": "GetCurrentShopSelection",
            "pageLoadIndex": index("GetCurrentShopSelection", selection_page_load),
            "pageTimesItemsPerPageAndSelectionIndexes": indexes(
                "GetCurrentShopSelection", selection_scale
            ),
            "resultByteReadIndex": index("GetCurrentShopSelection", selection_final_read),
            "maskIndex": len(selection) - 2,
        },
        "highlightSemantics": {
            "routine": "LoadShopInventoryHighlightSprites",
            "selectionShiftIndex": index(
                "LoadShopInventoryHighlightSprites", highlight_selection_shift
            ),
            "selectionCoordinateAddIndex": index(
                "LoadShopInventoryHighlightSprites", highlight_coordinate_add
            ),
            "spriteEntryStrideIndex": index(
                "LoadShopInventoryHighlightSprites", highlight_sprite_stride
            ),
            "pageBoundMultiplierIndex": index(
                "LoadShopInventoryHighlightSprites", highlight_page_bound
            ),
            "branchIndexes": indexes(
                "LoadShopInventoryHighlightSprites",
                [record for record in highlight if record["branchTarget"]],
            ),
            "terminalOperationIndex": len(highlight) - 1,
        },
        "resourceTransfers": resource_indexes,
        "windowScrollHelperFlows": helper_indexes,
        "jumpInterfaceAliases": aliases,
        "internalDirectCallerOccurrences": internal,
        "internalEffectiveDirectCallSiteCounts": totals(internal),
        "externalDirectCallerOccurrences": external,
        "externalEffectiveDirectCallSiteCounts": totals(external),
        "staticBoundary": {
            "hardwareTiming": "unknown",
            "renderedAppearance": "unknown",
            "callerLifecycleAndAdmission": "inferred",
        },
    }


def _service_state_machines(disasm: Path) -> dict[str, Any]:
    """Extract the built service-menu control-flow boundary without interpreting UI timing."""
    root = disasm / SOURCE_ROOT
    shop_contract = _shop_static_contract(root)
    church_contract = _church_static_contract(root)
    caravan_contract = _caravan_static_contract(root)
    blacksmith_contract = _blacksmith_static_contract(root)
    shared_selection_contract = _shared_selection_screen_contract(disasm, root)
    if not all((disasm / path).is_file() for path in SERVICE_SOURCE_PATHS):
        raise ValueError("service-menu source boundary is incomplete")
    service_files = [
        _parse_source_file(disasm / path, path.as_posix()) for path in SERVICE_SOURCE_PATHS
    ]
    service_calls: Counter[str] = Counter()
    for row in service_files:
        for call in row["directCalls"]:
            service_calls[call["target"]] += call["siteCount"]

    _require_ordered_fragments(
        root / "shop/shopactions.asm",
        [
            "moveq   #MENU_SHOP,d2",
            "jsr     j_ExecuteDiamondMenu",
            "cmpi.w  #-1,d0",
            "@CheckChoice_Buy:",
            "jsr     j_DecreaseGold",
            "jsr     j_AddItem",
            "@CheckChoice_Sell:",
            "jsr     j_IncreaseGold",
            "jsr     j_DropItemBySlot",
            "@CheckChoice_Repair:",
            "jsr     j_RepairItemBySlot",
            "@CheckChoice_Deals:",
            "jsr     j_RemoveItemFromDeals",
        ],
    )
    _require_ordered_fragments(
        root / "shopscreen.asm",
        [
            "ExecuteShopScreen:",
            "clr.w   ((CURRENT_SHOP_PAGE-$1000000)).w",
            "clr.w   ((CURRENT_SHOP_SELECTION-$1000000)).w",
            "btst    #INPUT_BIT_B,((CURRENT_PLAYER_INPUT-$1000000)).w",
            "btst    #INPUT_BIT_C,((CURRENT_PLAYER_INPUT-$1000000)).w",
            "btst    #INPUT_BIT_A,((CURRENT_PLAYER_INPUT-$1000000)).w",
            "moveq   #-1,d0",
            "GetCurrentShopSelection:",
            "add.w   d0,d0",
            "add.w   d1,d0",
            "move.w  ((CURRENT_SHOP_SELECTION-$1000000)).w,d1",
            "add.w   d1,d0",
        ],
    )
    _require_ordered_fragments(
        root / "church/churchactions_1.asm",
        [
            "moveq   #MENU_CHURCH,d2",
            "jsr     j_ExecuteDiamondMenu",
            "@CheckRaiseAction:",
            "jsr     j_DecreaseGold",
            "jsr     j_IncreaseCurrentHp",
            "@CheckCureAction:",
            "jsr     j_SetStatusEffects",
            "@CheckPromoAction:",
            "jsr     j_Promote",
            "@StartSave:",
            "jsr     (SaveGame).w",
        ],
    )
    _require_ordered_fragments(
        root / "caravan/caravanactions_1.asm",
        [
            "CaravanMenu:",
            "rjt_CaravanMenuActions:",
            "dc.w caravanMenu_Join-rjt_CaravanMenuActions",
            "dc.w caravanMenu_Depot-rjt_CaravanMenuActions",
            "dc.w caravanMenu_Item-rjt_CaravanMenuActions",
            "dc.w caravanMenu_Purge-rjt_CaravanMenuActions",
            "rjt_CaravanDepotSubmenuActions:",
            "dc.w caravanDepotSubmenu_Look-rjt_CaravanDepotSubmenuActions",
            "dc.w caravanDepotSubmenu_Deposit-rjt_CaravanDepotSubmenuActions",
            "dc.w caravanDepotSubmenu_Derive-rjt_CaravanDepotSubmenuActions",
            "dc.w caravanDepotSubmenu_Drop-rjt_CaravanDepotSubmenuActions",
            "jsr     j_AddItemToCaravan",
            "jsr     j_RemoveItemFromCaravan",
            "jsr     j_AddItemToDeals",
            "rjt_CaravanItemSubmenuActions:",
            "dc.w caravanItemSubmenu_Use-rjt_CaravanItemSubmenuActions",
            "dc.w caravanItemSubmenu_Give-rjt_CaravanItemSubmenuActions",
            "dc.w caravanItemSubmenu_Equip-rjt_CaravanItemSubmenuActions",
            "dc.w caravanItemSubmenu_Drop-rjt_CaravanItemSubmenuActions",
        ],
    )
    _require_ordered_fragments(
        root / "blacksmith/blacksmithactions.asm",
        [
            "BlacksmithMenu:",
            "clr.w   readyToFulfillOrdersNumber(a6)",
            "clr.w   pendingOrdersNumber(a6)",
            "clr.w   fulfilledOrdersNumber(a6)",
            "clr.w   fulfillOrdersFlag(a6)",
            "bsr.w   ProcessBlacksmithOrders",
            "BlacksmithAction_PlaceOrder:",
            "cmpi.w  #BLACKSMITH_MITHRIL_ITEM,d2",
            "jsr     j_DecreaseGold",
            "jsr     j_DropItemBySlot",
            "bsr.w   PickMithrilWeapon",
            "jsr     j_ClearFlag",
            "CountPendingAndReadyToFulfillOrders:",
            "move.w  #80,d1",
            "jsr     j_CheckFlag",
        ],
    )
    _require_ordered_fragments(
        root / "blacksmith/pickmithrilweapon.asm",
        [
            "PickMithrilWeapon:",
            "list_MithrilWeaponClasses",
            "table_MithrilWeapons",
            "jsr     (GenerateRandomNumber).w",
            "lea     ((MITHRIL_WEAPONS_ON_ORDER-$1000000)).w,a0",
        ],
    )
    _require_service_section(
        root / "shopscreen.asm",
        "ExecuteShopScreen:",
        "LoadShopInventoryHighlightSprites:",
        [
            "btst    #INPUT_BIT_B,((CURRENT_PLAYER_INPUT-$1000000)).w",
            "btst    #INPUT_BIT_C,((CURRENT_PLAYER_INPUT-$1000000)).w",
            "btst    #INPUT_BIT_A,((CURRENT_PLAYER_INPUT-$1000000)).w",
            "moveq   #-1,d0",
        ],
    )
    _require_service_section(
        root / "shopscreen.asm",
        "GetCurrentShopSelection:",
        "inventoryWindowLayoutLoadingSpace = -240",
        [
            "((CURRENT_SHOP_PAGE-$1000000)).w,d0",
            "((CURRENT_SHOP_SELECTION-$1000000)).w,d1",
            "lea     ((GENERIC_LIST-$1000000)).w,a0",
            "move.b  (a0,d0.w),d0",
        ],
    )
    _require_service_section(
        root / "shop/shopactions.asm",
        "@CheckChoice_Buy:",
        "@CheckChoice_Sell:",
        ["jsr     j_DecreaseGold", "jsr     j_AddItem"],
    )
    _require_service_section(
        root / "shop/shopactions.asm",
        "@CheckChoice_Sell:",
        "@CheckChoice_Repair:",
        ["jsr     j_IncreaseGold", "jsr     j_DropItemBySlot", "jsr     j_AddItemToDeals"],
    )
    _require_service_section(
        root / "shop/shopactions.asm",
        "@CheckChoice_Repair:",
        "@CheckChoice_Deals:",
        ["jsr     j_DecreaseGold", "jsr     j_RepairItemBySlot"],
    )
    _require_service_section(
        root / "shop/shopactions.asm",
        "@CheckChoice_Deals:",
        "PopulateShopInventoryList:",
        ["jsr     j_DecreaseGold", "jsr     j_AddItem", "jsr     j_RemoveItemFromDeals"],
    )
    _require_service_section(
        root / "shop/shopactions.asm",
        "PopulateShopInventoryList:",
        "DetermineDealsItemsNotInCurrentShop:",
        ["bsr.s   GetShopInventoryAddress", "((GENERIC_LIST_LENGTH-$1000000)).w"],
    )
    _require_service_section(
        root / "shop/shopactions.asm",
        "DetermineDealsItemsNotInCurrentShop:",
        "DoesCurrentShopContainItem:",
        ["j_GetDealsItemAmount", "DoesCurrentShopContainItem", "((GENERIC_LIST-$1000000)).w"],
    )
    _require_service_section(
        root / "church/churchactions_1.asm",
        "@CheckRaiseAction:",
        "@CheckCureAction:",
        ["jsr     j_DecreaseGold", "jsr     j_IncreaseCurrentHp", "bsr.w   UpdateAllyMapsprite"],
    )
    _require_service_section(
        root / "church/churchactions_1.asm",
        "@CheckCureAction:",
        "@CheckPromoAction:",
        ["jsr     j_DecreaseGold", "jsr     j_SetStatusEffects"],
    )
    _require_service_section(
        root / "church/churchactions_1.asm",
        "@CheckPromoAction:",
        "@StartSave:",
        ["bsr.w   CountPromotableMembers", "jsr     j_SetClass", "jsr     j_Promote"],
    )
    _require_service_section(
        root / "church/churchactions_2.asm",
        "CountPromotableMembers:",
        "GetPromotionData:",
        ["jsr     j_GetClass", "bsr.w   GetPromotionData", "jsr     j_GetLevel"],
    )
    _require_service_section(
        root / "caravan/caravanactions_1.asm",
        "CaravanMenu:",
        "caravanMenu_Join:",
        ["moveq   #MENU_CARAVAN,d2", "jsr     j_ExecuteDiamondMenu", "rjt_CaravanMenuActions:"],
    )
    _require_service_section(
        root / "caravan/caravanactions_1.asm",
        "caravanMenu_Join:",
        "caravanMenu_Purge:",
        ["jsr     j_JoinBattleParty", "jsr     j_LeaveBattleParty"],
    )
    _require_service_section(
        root / "caravan/caravanactions_1.asm",
        "caravanMenu_Depot:",
        "caravanDepotSubmenu_Look:",
        ["moveq   #MENU_DEPOT,d2", "jsr     j_ExecuteDiamondMenu"],
    )
    _require_service_section(
        root / "caravan/caravanactions_1.asm",
        "caravanDepotSubmenu_Deposit:",
        "caravanDepotSubmenu_Derive:",
        ["jsr     j_AddItemToCaravan", "jsr     j_DropItemBySlot"],
    )
    _require_service_section(
        root / "caravan/caravanactions_1.asm",
        "caravanDepotSubmenu_Derive:",
        "caravanDepotSubmenu_Drop:",
        ["jsr     j_AddItem", "jsr     j_RemoveItemFromCaravan"],
    )
    _require_service_section(
        root / "caravan/caravanactions_1.asm",
        "caravanDepotSubmenu_Drop:",
        "caravanMenu_Item:",
        ["jsr     j_RemoveItemFromCaravan", "jsr     j_AddItemToDeals"],
    )
    _require_service_section(
        root / "caravan/caravanactions_1.asm",
        "caravanMenu_Item:",
        "modend",
        ["moveq   #MENU_ITEM,d2", "jsr     j_ExecuteDiamondMenu", "rjt_CaravanItemSubmenuActions:"],
    )
    _require_service_section(
        root / "blacksmith/blacksmithactions.asm",
        "ProcessBlacksmithOrders:",
        "BlacksmithAction_FulfillOrder:",
        [
            "CountPendingAndReadyToFulfillOrders",
            "#BLACKSMITH_MAX_ORDERS_NUMBER",
            "BlacksmithAction_PlaceOrder",
        ],
    )
    _require_service_section(
        root / "blacksmith/blacksmithactions.asm",
        "BlacksmithAction_FulfillOrder:",
        "BlacksmithAction_PlaceOrder:",
        ["jsr     j_AddItem", "jsr     j_EquipItemBySlot"],
    )
    _require_service_section(
        root / "blacksmith/blacksmithactions.asm",
        "BlacksmithAction_PlaceOrder:",
        "WaitForMusicResumeAndPlayerInput_Blacksmith:",
        [
            "cmpi.w  #BLACKSMITH_MITHRIL_ITEM,d2",
            "jsr     j_GetClass",
            "bsr.w   IsClassBlacksmithEligible",
            "#BLACKSMITH_ORDER_COST",
            "jsr     j_GetGold",
            "jsr     j_DecreaseGold",
            "bsr.w   PickMithrilWeapon",
            "jsr     j_ClearFlag",
        ],
    )
    blacksmith_sources = "\n".join(
        (disasm / path).read_text(encoding="utf-8") for path in SERVICE_SOURCE_PATHS[:2]
    )
    if "ExecuteDiamondMenu" in blacksmith_sources:
        raise ValueError("blacksmith service unexpectedly enters ExecuteDiamondMenu")

    return {
        "builtSourcePaths": [path.as_posix() for path in SERVICE_SOURCE_PATHS],
        "sourceInventory": {
            "fileCount": len(service_files),
            "sourceLineCount": sum(row["sourceLineCount"] for row in service_files),
            "directCallSiteCount": sum(service_calls.values()),
            "indirectCallSiteCount": sum(row["indirectCallSiteCount"] for row in service_files),
            "uniqueDirectTargetCount": len(service_calls),
            "entrySymbols": {
                "blacksmith": "BlacksmithMenu",
                "caravan": "CaravanMenu",
                "church": "ChurchMenu",
                "shop": "ShopMenu",
                "sharedSelection": "ExecuteShopScreen",
            },
        },
        "sharedSelectionScreen": shared_selection_contract,
        "shop": shop_contract,
        "church": church_contract,
        "caravan": caravan_contract,
        "blacksmith": blacksmith_contract,
        "staticBoundary": {
            "callerDependentServiceAdmissionAndReturnState": "inferred",
            "persistenceAcrossMapLoadSaveAndStoryProgress": "unknown",
            "windowPortraitSoundAndInputTiming": "unknown",
            "unbuiltAlternatePaths": [ALTERNATE_SOURCE.as_posix()],
        },
    }


def _menu_facts(disasm: Path, field_item_pairs: list[dict[str, Any]]) -> dict[str, Any]:
    root = disasm / SOURCE_ROOT
    _require_ordered_fragments(
        root / "diamondmenu.asm",
        [
            "move.b  d0,((CURRENT_DIAMOND_MENU_CHOICE-$1000000)).w",
            "btst    #INPUT_BIT_LEFT,((CURRENT_PLAYER_INPUT-$1000000)).w",
            "moveq   #1,d1",
            "btst    #INPUT_BIT_RIGHT,((CURRENT_PLAYER_INPUT-$1000000)).w",
            "moveq   #2,d1",
            "btst    #INPUT_BIT_UP,((CURRENT_PLAYER_INPUT-$1000000)).w",
            "clr.w   d1",
            "btst    #INPUT_BIT_DOWN,((CURRENT_PLAYER_INPUT-$1000000)).w",
            "moveq   #3,d1",
            "btst    #INPUT_BIT_B,((CURRENT_PLAYER_INPUT-$1000000)).w",
            "moveq   #-1,d0",
            "btst    #INPUT_BIT_C,((CURRENT_PLAYER_INPUT-$1000000)).w",
            "btst    #INPUT_BIT_A,((CURRENT_PLAYER_INPUT-$1000000)).w",
            "move.w  #$100,d6",
            "jsr     (GenerateRandomNumber).w",
            "jsr     (WaitForVInt).w",
        ],
    )
    _require_ordered_fragments(
        root / "yesnoprompt.asm",
        [
            "clr.b   ((CURRENT_DIAMOND_MENU_CHOICE-$1000000)).w",
            "btst    #INPUT_BIT_LEFT,((CURRENT_PLAYER_INPUT-$1000000)).w",
            "clr.w   d1",
            "btst    #INPUT_BIT_RIGHT,((CURRENT_PLAYER_INPUT-$1000000)).w",
            "moveq   #-1,d1",
            "btst    #INPUT_BIT_B,((CURRENT_PLAYER_INPUT-$1000000)).w",
            "moveq   #-1,d0",
            "btst    #INPUT_BIT_C,((CURRENT_PLAYER_INPUT-$1000000)).w",
            "btst    #INPUT_BIT_A,((CURRENT_PLAYER_INPUT-$1000000)).w",
            "move.b  ((CURRENT_DIAMOND_MENU_CHOICE-$1000000)).w,d0",
            "ext.w   d0",
        ],
    )
    _require_ordered_fragments(
        root / "numberprompt.asm",
        [
            "moveq   #1,d3",
            "moveq   #-1,d3",
            "moveq   #10,d3",
            "moveq   #-10,d3",
            "btst    #INPUT_BIT_B,((CURRENT_PLAYER_INPUT-$1000000)).w",
            "move.w  #256,d6",
            "jsr     (GenerateRandomNumber).w",
            "move.w  #-1,numberEntry(a6)",
            "add.w   d3,d0",
            "cmp.w   numberMax(a6),d0",
            "move.w  numberMax(a6),d0",
            "cmp.w   numberMin(a6),d0",
            "move.w  numberMin(a6),d0",
        ],
    )
    _require_ordered_fragments(
        root / "menuenginecommon.asm",
        [
            "move.w  #-1,useOrangeFont(a6)",
            "jsr     (WriteAsciiNumber).w",
            "lea     ((LOADED_NUMBER-$1000000)).w,a0",
            "clr.w   useOrangeFont(a6)",
            "cmpi.b  #TEXT_CODE_MOVEDOWN,d0",
            "cmpi.b  #TEXT_CODE_TOGGLEFONTCOLOR,d0",
            "cmpi.b  #TEXT_CODE_NEWLINE,d0",
            "eori.w  #$FFFF,useOrangeFont(a6)",
        ],
    )
    _require_ordered_fragments(
        root / "item/isitemusableonfield.asm",
        [
            "moveq   #0,d2",
            "lea     table_UsableOnFieldItems(pc), a0",
            "cmp.b   (a0)+,d1",
            "cmpi.b  #-1,(a0)",
            "moveq   #-1,d2",
        ],
    )
    return {
        "diamondMenu": {
            "choiceByDirection": {"up": 0, "left": 1, "right": 2, "down": 3},
            "confirmButtons": ["A", "C"],
            "cancelButton": "B",
            "cancelResult": -1,
            "optionalCallbackOnOpenAndSelectionChange": True,
            "idleRngRange": 256,
            "waitsOneVintPerIdleIteration": True,
        },
        "yesNoPrompt": {
            "initialResult": 0,
            "yesResult": 0,
            "noResult": -1,
            "cancelResult": -1,
            "leftSelectsYes": True,
            "rightSelectsNo": True,
            "confirmButtons": ["A", "C"],
            "movesDialogueAndGoldWindowsWhenPresent": True,
        },
        "numberPrompt": {
            "directionDeltas": {"right": 1, "left": -1, "down": 10, "up": -10},
            "clampsToCallerMinimum": True,
            "clampsToCallerMaximum": True,
            "confirmButtons": ["A", "C"],
            "cancelButton": "B",
            "cancelResult": -1,
            "idleRngRange": 256,
            "waitsOneVintPerIdleIteration": True,
        },
        "textRendering": {
            "separateRegularAndOrangeEntryPoints": True,
            "numbersUseLoadedNumberBuffer": True,
            "supportsMoveDownControl": True,
            "supportsFontToggleControl": True,
            "supportsNewlineControl": True,
        },
        "fieldItems": {
            "dispatchPairCount": len(field_item_pairs),
            "pairs": field_item_pairs,
            "terminator": 65535,
            "masksItemEntryToIndex": True,
            "usabilityListTerminator": -1,
            "unlistedItemResult": -1,
        },
        "serviceEntries": [
            "BlacksmithMenu",
            "CaravanMenu",
            "ChurchMenu",
            "FieldMenu",
            "ShopMenu",
        ],
        "serviceStateMachines": _service_state_machines(disasm),
        "inventoryBoundary": {
            "battlefieldAndFieldMenusInventoried": True,
            "shopsChurchCaravanAndBlacksmithInventoried": True,
            "portraitMemberMinimapAndEndingPresentationInventoried": True,
            "windowMovementPortraitAndAnimationTimingRemainQueued": True,
            "serviceCallerStateAndUiSequencesRemainQueued": True,
        },
    }


def build_menu_inventory(upstream_path: Path) -> dict[str, Any]:
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"common menus H1 listing is missing: {listing_path}")
    listing = listing_path.read_text(encoding="utf-8")
    paths = sorted((disasm / SOURCE_ROOT).rglob("*.asm"), key=lambda path: path.as_posix())
    files = [_parse_source_file(path, path.relative_to(disasm).as_posix()) for path in paths]
    if len(files) != 42:
        raise ValueError(f"common menus file-count drift: {len(files)}")
    layout_paths = _layout_menu_paths(disasm)
    expected_layout_paths = {row["path"] for row in files} - {ALTERNATE_SOURCE.as_posix()}
    if layout_paths != expected_layout_paths:
        raise ValueError("common menus layout include set drift")
    representative_symbols: dict[str, str] = {}
    representative_addresses: dict[str, int] = {}
    calls: Counter[str] = Counter()
    labels: set[str] = set()
    for row in files:
        for call in row["directCalls"]:
            calls[call["target"]] += call["siteCount"]
        labels.update(row["globalLabels"])
        relative = Path(row["path"]).relative_to(SOURCE_ROOT).as_posix()
        if not row["globalLabels"]:
            raise ValueError(f"unexpected unlabeled common menus file: {row['path']}")
        representative_symbols[relative] = row["globalLabels"][0]
        if row["path"] in layout_paths:
            symbol = row["globalLabels"][0]
            representative_addresses[symbol] = _listing_address(listing, symbol)
    records = [
        record
        for record in load_json(RESEARCH_INDEX)["records"]
        if Path(record["sourcePath"]).is_relative_to(SOURCE_ROOT)
    ]
    field_item_pairs = _field_item_pairs(disasm / SOURCE_ROOT / "item/fielditemeffects.asm")
    category_counts = Counter(
        relative.split("/", 1)[0] if "/" in relative else "root"
        for relative in representative_symbols
    )
    summary = {
        "fileCount": len(files),
        "sourceLineCount": sum(row["sourceLineCount"] for row in files),
        "statementCount": sum(row["statementCount"] for row in files),
        "globalLabelCount": sum(len(row["globalLabels"]) for row in files),
        "localLabelCount": sum(row["localLabelCount"] for row in files),
        "directCallSiteCount": sum(calls.values()),
        "indirectCallSiteCount": sum(row["indirectCallSiteCount"] for row in files),
        "uniqueDirectTargetCount": len(calls),
        "internalDirectTargetCount": sum(target in labels for target in calls),
        "externalDirectTargetCount": sum(target not in labels for target in calls),
        "layoutIncludedFileCount": len(layout_paths),
        "indexedRecordCount": len(records),
        "indexedFileCount": len({record["sourcePath"] for record in records}),
        "excludedAlternateFileCount": 1,
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "scope": SOURCE_ROOT.as_posix(),
        "summary": summary,
        "categoryFileCounts": dict(sorted(category_counts.items())),
        "indexedRecordIds": sorted(record["id"] for record in records),
        "indexedSourcePaths": sorted({record["sourcePath"] for record in records}),
        "representativeSymbols": representative_symbols,
        "representativeAddresses": representative_addresses,
        "internalDirectCallTargets": sorted(target for target in calls if target in labels),
        "externalDirectCallTargets": sorted(target for target in calls if target not in labels),
        "menuFacts": _menu_facts(disasm, field_item_pairs),
        "alternateSource": _alternate_source_fact(disasm, listing),
        "files": files,
    }


def _verify_menu_fixture_owner(
    fixture: dict[str, Any],
    output: dict[str, Any],
    *,
    rom_manifest: dict[str, Any],
) -> None:
    """Keep exact common-menu evidence outside the reusable shape schemas."""
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != rom_manifest["hashes"]["sha256"]
    ):
        raise ValueError("common menus provenance drift")
    if output["representativeAddresses"] != fixture["function"]:
        raise ValueError("common menus H1 address drift")
    if output["menuFacts"] != fixture["expected"]["menuFacts"]:
        raise ValueError("common menus model drift")
    if output["alternateSource"] != fixture["expected"]["alternateSource"]:
        raise ValueError("common menus alternate-source drift")


def verify_menu_inventory(
    upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_menu_inventory(upstream_path)
    validate_json(output, SCHEMA, owner="common menus static inventory")
    _verify_menu_fixture_owner(
        fixture,
        output,
        rom_manifest=load_json(ROM_MANIFEST),
    )
    if output["summary"] != manifest["summary"]:
        raise ValueError("common menus summary drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"]:
        raise ValueError("common menus canonical hash drift")
    destination = output_path or repo_path("local/derived/common-menus-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Inventory": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Files": output["summary"]["fileCount"],
        "LayoutIncludedFiles": output["summary"]["layoutIncludedFileCount"],
        "IndexedRecords": output["summary"]["indexedRecordCount"],
        "FieldItemPairs": output["menuFacts"]["fieldItems"]["dispatchPairCount"],
        "ExcludedAlternates": output["summary"]["excludedAlternateFileCount"],
        "Status": "PASS",
    }
