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

ID = "sf2-common-menus-static-v1"
SOURCE_ROOT = Path("code/common/menus")
ALTERNATE_SOURCE = SOURCE_ROOT / "writememberlisttext.asm"
CANONICAL_CONTAINER = SOURCE_ROOT / "memberslistscreen.asm"
MANIFEST = repo_path("manifests/extractions/common-menus-static.json")
SCHEMA = repo_path("schemas/common-menus-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/common-menus-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-common-menus-static-fixture.schema.json")
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
        path.read_text(encoding="utf-8")
        for path in sorted((disasm / "layout").glob("*.asm"))
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


def _service_state_machines(disasm: Path) -> dict[str, Any]:
    """Extract the built service-menu control-flow boundary without interpreting UI timing."""
    root = disasm / SOURCE_ROOT
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
        (disasm / path).read_text(encoding="utf-8")
        for path in SERVICE_SOURCE_PATHS[:2]
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
        "sharedSelectionScreen": {
            "entrySymbol": "ExecuteShopScreen",
            "cancelResult": -1,
            "confirmButtons": ["A", "C"],
            "cancelButton": "B",
            "selectionAddressing": "page-times-items-per-page-plus-selection",
            "stateReadsAndWrites": [
                "CURRENT_SHOP_PAGE",
                "CURRENT_SHOP_SELECTION",
                "CURRENT_SHOP_PAGE_ITEMS_NUMBER",
                "GENERIC_LIST",
                "GENERIC_LIST_LENGTH",
            ],
        },
        "shop": {
            "entrySymbol": "ShopMenu",
            "selectionMenu": "MENU_SHOP",
            "choiceOrder": ["buy", "sell", "repair", "deals"],
            "dispatch": "ordered-conditional-chain",
            "actionLoop": "each non-exit action returns to the shop choice loop",
            "exit": "diamond-menu-cancel-exits-service",
            "listSources": [
                "count-prefixed-current-shop-inventory",
                "eligible-deals-not-in-current-shop",
            ],
            "confirmedEffects": {
                "buy": ["decrease-gold", "add-item"],
                "sell": ["increase-gold", "drop-item-by-slot", "rare-item-adds-to-deals"],
                "repair": ["decrease-gold", "repair-item-by-slot"],
                "deals": ["decrease-gold", "add-item", "remove-item-from-deals"],
            },
        },
        "church": {
            "entrySymbol": "ChurchMenu",
            "selectionMenu": "MENU_CHURCH",
            "choiceOrder": ["raise", "cure", "promote", "save"],
            "dispatch": "ordered-conditional-chain",
            "actionLoop": "actions converge on the save-or-return boundary before returning",
            "exit": "diamond-menu-cancel-exits-service",
            "confirmedEffects": {
                "raise": ["decrease-gold", "increase-current-hp", "update-ally-mapsprite"],
                "cure": ["decrease-gold", "set-status-effects"],
                "promote": ["promotion-data-gated-member-selection", "set-class", "promote"],
                "save": ["save-game"],
            },
        },
        "caravan": {
            "entrySymbol": "CaravanMenu",
            "selectionMenu": "MENU_CARAVAN",
            "choiceOrder": ["join", "depot", "item", "purge"],
            "dispatch": "four-entry-relative-jump-table",
            "actionLoop": "each action returns to the caravan choice loop",
            "exit": "diamond-menu-cancel-exits-service",
            "depot": {
                "selectionMenu": "MENU_DEPOT",
                "choiceOrder": ["look", "deposit", "derive", "drop"],
                "dispatch": "four-entry-relative-jump-table",
                "effects": {
                    "deposit": ["add-item-to-caravan", "drop-item-by-slot"],
                    "derive": ["add-item", "remove-item-from-caravan"],
                    "drop": ["remove-item-from-caravan", "rare-item-adds-to-deals"],
                },
            },
            "item": {
                "selectionMenu": "MENU_ITEM",
                "choiceOrder": ["use", "give", "equip", "drop"],
                "dispatch": "four-entry-relative-jump-table",
            },
            "partyEffects": ["join-battle-party", "leave-battle-party"],
        },
        "blacksmith": {
            "entrySymbol": "BlacksmithMenu",
            "dispatch": "ordered-fulfill-ready-orders-then-place-pending-order",
            "noDiamondMenu": True,
            "perVisitCounters": [
                "readyToFulfillOrdersNumber",
                "pendingOrdersNumber",
                "fulfilledOrdersNumber",
                "fulfillOrdersFlag",
            ],
            "fulfillment": ["select-recipient", "add-item", "optional-equip"],
            "placementGuards": [
                "select-mithril-item",
                "select-eligible-promoted-customer",
                "confirm-order-cost",
                "sufficient-gold",
                "free-order-slot",
            ],
            "placementEffects": [
                "decrease-gold",
                "drop-mithril-by-slot",
                "pick-mithril-weapon",
                "clear-flag-80",
            ],
            "randomBoundary": (
                "mithril-weapon-selection-includes-random-class-and-weighted-row-selection"
            ),
        },
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


def verify_menu_inventory(
    upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_menu_inventory(upstream_path)
    validate_json(output, SCHEMA, owner="common menus static inventory")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != load_json(ROM_MANIFEST)["hashes"]["sha256"]
    ):
        raise ValueError("common menus provenance drift")
    if output["summary"] != manifest["summary"]:
        raise ValueError("common menus summary drift")
    if output["representativeAddresses"] != fixture["function"]:
        raise ValueError("common menus H1 address drift")
    if output["menuFacts"] != fixture["expected"]["menuFacts"]:
        raise ValueError("common menus model drift")
    if output["alternateSource"] != fixture["expected"]["alternateSource"]:
        raise ValueError("common menus alternate-source drift")
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
