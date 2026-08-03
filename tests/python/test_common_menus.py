import json
import shutil
from copy import deepcopy

import pytest

import sf2tool.h2.menus as menus_module
from sf2tool.h2.menus import (
    _blacksmith_static_contract,
    _caravan_static_contract,
    _church_static_contract,
    _shared_selection_screen_contract,
    _shop_direct_call_occurrences,
    _shop_static_contract,
    _verify_menu_fixture_owner,
    build_menu_inventory,
    verify_menu_inventory,
)
from sf2tool.jsonio import load_json, schema_composition_audit, validate_json
from sf2tool.paths import repo_path

UPSTREAM = repo_path("local/upstream/SF2DISASM")
OUTPUT_SCHEMA = repo_path("schemas/h2/common-menus-output.schema.json")
FIXTURE_SCHEMA = repo_path("schemas/h2/common-menus-fixture.schema.json")
INSTRUCTION_SCHEMA = repo_path("schemas/h2/common-menus-instruction.schema.json")
SHOP_SCHEMA = repo_path("schemas/h2/common-menus-shop.schema.json")
CHURCH_SCHEMA = repo_path("schemas/h2/common-menus-church.schema.json")
CARAVAN_SCHEMA = repo_path("schemas/h2/common-menus-caravan.schema.json")
BLACKSMITH_SCHEMA = repo_path("schemas/h2/common-menus-blacksmith.schema.json")
SHARED_SELECTION_SCHEMA = repo_path("schemas/h2/common-menus-shared-selection.schema.json")
SERVICE_STATE_MACHINES_SCHEMA = repo_path(
    "schemas/h2/common-menus-service-state-machines.schema.json"
)
FIXTURE = repo_path("tests/fixtures/h2/common-menus-static-v1.json")


def test_shop_direct_call_parser_is_instruction_scoped(tmp_path) -> None:
    path = tmp_path / "calls.asm"
    path.write_text(
        "bsr.s ShopMenu\njsr.l (ExecuteShopScreen).l\nlabel: bsr.w ShopMenu\n"
        "jsr.w j_ShopMenu\n"
        "; jsr.w ShopMenu\ndc.l ShopMenu\njsr.w ShopMenu trailing\n",
        encoding="utf-8",
    )
    assert _shop_direct_call_occurrences(
        path,
        {"j_ShopMenu": "ShopMenu"},
        {"ShopMenu", "ExecuteShopScreen"},
    ) == [
        {
            "instructionTarget": "ExecuteShopScreen",
            "effectiveTarget": "ExecuteShopScreen",
            "siteCount": 1,
        },
        {"instructionTarget": "ShopMenu", "effectiveTarget": "ShopMenu", "siteCount": 2},
        {"instructionTarget": "j_ShopMenu", "effectiveTarget": "ShopMenu", "siteCount": 1},
    ]


def test_church_alias_call_parser_retains_instruction_and_effective_targets(tmp_path) -> None:
    path = tmp_path / "church-calls.asm"
    path.write_text(
        "label: jsr.l (j_ChurchMenu).l\nbsr.s ChurchMenu\n"
        "; jsr.w j_ChurchMenu\ndc.l j_ChurchMenu\njsr.w j_ChurchMenu trailing\n",
        encoding="utf-8",
    )
    assert _shop_direct_call_occurrences(
        path,
        {"j_ChurchMenu": "ChurchMenu"},
        {"ChurchMenu"},
    ) == [
        {"instructionTarget": "ChurchMenu", "effectiveTarget": "ChurchMenu", "siteCount": 1},
        {"instructionTarget": "j_ChurchMenu", "effectiveTarget": "ChurchMenu", "siteCount": 1},
    ]


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_shop_choice_dispatch_ignores_route_confirmation_compares() -> None:
    shop = build_menu_inventory(UPSTREAM)["menuFacts"]["serviceStateMachines"]["shop"]
    assert shop["choiceDispatch"] == {
        "menuLabel": "MENU_SHOP",
        "comparedChoiceValues": [0, 1, 2],
        "comparedRouteOrder": ["buy", "sell", "repair"],
        "fallthroughRoute": "deals",
    }
    assert shop["externalDirectCallerOccurrences"] == {
        "code/common/menus/caravan/caravanactions_1.asm": [
            {
                "instructionTarget": "j_ExecuteShopScreen",
                "effectiveTarget": "ExecuteShopScreen",
                "siteCount": 3,
            }
        ],
        "code/common/scripting/map/mapscriptengine_2.asm": [
            {"instructionTarget": "j_ShopMenu", "effectiveTarget": "ShopMenu", "siteCount": 1}
        ],
        "code/gameflow/special/battletest.asm": [
            {"instructionTarget": "j_ShopMenu", "effectiveTarget": "ShopMenu", "siteCount": 1}
        ],
    }
    assert shop["jumpInterfaceAliases"] == {
        "j_ExecuteShopScreen": {
            "effectiveTarget": "ExecuteShopScreen",
            "sourcePath": "code/common/tech/jumpinterfaces/s03_jumpinterface_1.asm",
        },
        "j_ShopMenu": {
            "effectiveTarget": "ShopMenu",
            "sourcePath": "code/common/tech/jumpinterfaces/s05_jumpinterface.asm",
        },
    }
    assert shop["internalEffectiveDirectCallSiteCounts"]["ExecuteShopScreen"] == 2
    assert shop["externalEffectiveDirectCallSiteCounts"] == {
        "DetermineDealsItemsNotInCurrentShop": 0,
        "DoesCurrentShopContainItem": 0,
        "ExecuteShopScreen": 3,
        "GetShopInventoryAddress": 0,
        "PopulateShopInventoryList": 0,
        "ShopMenu": 2,
        "WaitForMusicResumeAndPlayerInput_Shop": 0,
    }


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_church_contract_starts_with_structured_route_inventory() -> None:
    church = build_menu_inventory(UPSTREAM)["menuFacts"]["serviceStateMachines"]["church"]
    assert church["choiceDispatch"] == {
        "menuLabel": "MENU_CHURCH",
        "comparedChoiceValues": [0, 1, 2],
        "comparedRouteOrder": ["raise", "cure", "promote"],
        "fallthroughRoute": "save",
        "cancelValue": -1,
        "cancelBranchTarget": "@ExitMenu",
    }
    assert set(church["routeOperations"]) == {"raise", "cure", "promote", "save"}
    assert church["routeOperations"]["raise"][0] == {
        "labels": ["@CheckRaiseAction"],
        "opcode": "cmpi.w",
        "operands": ["#0", "d0"],
        "directTarget": None,
        "branchTarget": None,
    }
    assert church["routeDerived"] == {
        "raise": {
            "levelCostMultiplier": 10,
            "promotedExtraCost": 200,
            "aliveBranchOpcode": "bhi.w",
            "goldBranchTarget": "@DoRaise",
            "mutationCalls": ["j_DecreaseGold", "j_IncreaseCurrentHp", "UpdateAllyMapsprite"],
            "hpCap": 200,
        },
        "cure": {
            "poisonCost": 10,
            "stunCost": 20,
            "statusMasks": {"poison": 2, "stun": 1, "curse": 4, "allStatusBits": 65535},
            "curseItemPrice": {
                "itemDefinitionOffsetBytes": 6,
                "loadWidthBits": 16,
                "rightShiftBits": 2,
            },
        },
        "promote": {"minimumLevel": 20, "classAndPromotionCalls": ["j_SetClass", "j_Promote"]},
        "save": {"saveCallOperand": "(SaveGame).w", "suspendJumpOperand": "(WitchSuspend).w"},
    }
    assert church["jumpInterfaceAliases"] == {
        "j_ChurchMenu": {
            "effectiveTarget": "ChurchMenu",
            "sourcePath": "code/common/tech/jumpinterfaces/s05_jumpinterface.asm",
        }
    }
    assert church["externalDirectCallerOccurrences"] == {
        "code/common/menus/main/mainactions.asm": [
            {
                "instructionTarget": "WaitForMusicResumeAndPlayerInput",
                "effectiveTarget": "WaitForMusicResumeAndPlayerInput",
                "siteCount": 5,
            }
        ],
        "code/common/scripting/map/mapscriptengine_2.asm": [
            {"instructionTarget": "j_ChurchMenu", "effectiveTarget": "ChurchMenu", "siteCount": 1}
        ],
        "code/gameflow/exploration/explorationvints.asm": [
            {"instructionTarget": "j_ChurchMenu", "effectiveTarget": "ChurchMenu", "siteCount": 1}
        ],
        "code/gameflow/special/battletest.asm": [
            {"instructionTarget": "j_ChurchMenu", "effectiveTarget": "ChurchMenu", "siteCount": 2}
        ],
    }
    assert church["internalEffectiveDirectCallSiteCounts"] == {
        "ChurchMenu": 0, "Church_CureStun": 1, "Church_GetCurrentForceMemberInfo": 4,
        "CountPromotableMembers": 1, "FindPromotionSection": 4, "GetPromotionData": 5,
        "ReplaceSpellsWithSorcDefaults": 1, "UpdateAllyMapsprite": 2,
        "WaitForMusicResumeAndPlayerInput": 6,
    }
    assert church["externalEffectiveDirectCallSiteCounts"]["ChurchMenu"] == 4


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_caravan_contract_starts_with_relative_dispatch_inventory() -> None:
    caravan = _caravan_static_contract(UPSTREAM / "disasm/code/common/menus")
    assert caravan["dispatchTables"] == {
        "top": {
            "baseLabel": "rjt_CaravanMenuActions",
            "entryWidthBytes": 2,
            "targets": [
                "caravanMenu_Join",
                "caravanMenu_Depot",
                "caravanMenu_Item",
                "caravanMenu_Purge",
            ],
        },
        "depot": {
            "baseLabel": "rjt_CaravanDepotSubmenuActions",
            "entryWidthBytes": 2,
            "targets": [
                "caravanDepotSubmenu_Look",
                "caravanDepotSubmenu_Deposit",
                "caravanDepotSubmenu_Derive",
                "caravanDepotSubmenu_Drop",
            ],
        },
        "item": {
            "baseLabel": "rjt_CaravanItemSubmenuActions",
            "entryWidthBytes": 2,
            "targets": [
                "caravanItemSubmenu_Use",
                "caravanItemSubmenu_Give",
                "caravanItemSubmenu_Equip",
                "caravanItemSubmenu_Drop",
            ],
        },
    }
    assert caravan["topDispatch"] == {
        "menuLabel": "MENU_CARAVAN",
        "selectorScaleBytes": 2,
        "cancelValue": -1,
        "cancelBranchTarget": "@ExitCaravan",
        "loopBranchTarget": "@RestartCaravan",
    }
    assert set(caravan["routeOperations"]) == {
        "entry", "join", "purge", "depot", "depotLook", "depotDeposit", "depotDerive",
        "depotDrop", "item", "itemUse", "itemGive", "itemEquip", "itemDrop",
    }
    assert caravan["routeOperations"]["entry"][0] == {
        "labels": ["CaravanMenu"],
        "opcode": "module",
        "operands": [],
        "directTarget": None,
        "branchTarget": None,
    }


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_blacksmith_contract_starts_with_complete_named_function_records() -> None:
    blacksmith = _blacksmith_static_contract(UPSTREAM / "disasm/code/common/menus")
    assert blacksmith["sourcePaths"] == [
        "code/common/menus/blacksmith/blacksmithactions.asm",
        "code/common/menus/blacksmith/pickmithrilweapon.asm",
    ]
    assert set(blacksmith["functionOperations"]) == {
        "BlacksmithMenu",
        "ProcessBlacksmithOrders",
        "BlacksmithAction_FulfillOrder",
        "BlacksmithAction_PlaceOrder",
        "WaitForMusicResumeAndPlayerInput_Blacksmith",
        "CountPendingAndReadyToFulfillOrders",
        "IsClassBlacksmithEligible",
        "PickMithrilWeapon",
    }
    assert blacksmith["functionOperations"]["BlacksmithMenu"][0] == {
        "labels": ["BlacksmithMenu"],
        "opcode": "movem.l",
        "operands": ["d0-a5", "-(sp)"],
        "directTarget": None,
        "branchTarget": None,
    }
    assert [
        (
            gate["name"],
            gate["operation"]["operands"],
            gate["branch"]["opcode"],
            gate["branch"]["branchTarget"],
        )
        for gate in blacksmith["derived"]["place"]["gateSequence"]
    ] == [
        ("materialSelectionCancel", ["#-1", "d0"], "beq.w", "@Done"),
        ("mithrilMatch", ["#BLACKSMITH_MITHRIL_ITEM", "d2"], "beq.w", "byte_21D1A"),
        ("customerSelectionCancel", ["#-1", "d0"], "beq.s", "byte_21CDE"),
        (
            "promotionFloor",
            ["#CHAR_CLASS_FIRSTPROMOTED", "d1"],
            "bcc.w",
            "@IsCustomerClassEligible",
        ),
        ("eligibilityResult", ["#0", "d0"], "beq.w", "@ConfirmOrder"),
        ("confirmationResult", ["#0", "d0"], "beq.s", "@CheckGold"),
        ("goldComparison", ["#BLACKSMITH_ORDER_COST", "d1"], "bcc.w", "@PlaceOrder"),
    ]
    assert [branch["name"] for branch in blacksmith["derived"]["fulfill"]["branchSequence"]] == [
        "recipientCancel",
        "inventoryCapacity",
        "equipmentType",
        "equippability",
        "optionalEquipEligibility",
        "optionalEquipConfirmation",
        "weaponCurseRejection",
        "ringCurseRejection",
        "newlyEquippedCurseOutcome",
    ]
    force_copy = blacksmith["derived"]["process"]["forceCopy"]
    assert force_copy["counterSource"] == {
        "labels": [],
        "opcode": "move.w",
        "operands": ["((TARGETS_LIST_LENGTH-$1000000)).w", "d7"],
        "directTarget": None,
        "branchTarget": None,
    }
    assert force_copy["counterSourceOperand"] == force_copy["sourceLengthOperand"]
    assert force_copy["counterDestination"] == "d7"
    pick = blacksmith["derived"]["pick"]
    assert [
        pick["classGroupScan"][name]["opcode"]
        for name in (
            "prefixRead",
            "prefixDecrement",
            "classRead",
            "characterClassRead",
            "classCompare",
            "classMatchBranch",
            "innerLoop",
            "groupIndexIncrement",
            "outerLoop",
        )
    ] == ["move.w", "subq.w", "move.w", "move.w", "cmp.w", "beq.w", "dbf", "addi.w", "dbf"]
    assert [
        pick["weightedRngLoop"][name]["operands"]
        for name in ("parameterRead", "itemRead", "parameterToRngRange", "resultCompare", "loop")
    ] == [
        ["(a0)+", "d0"],
        ["(a0)+", "d1"],
        ["d0", "d6"],
        ["#0", "d7"],
        ["d5", "@PickWeapon_Loop"],
    ]
    assert pick["weightedRngLoop"]["parameterColumnDenominators"] == {
        "owner": "item-auxiliary",
        "sourcePath": "data/stats/items/mithrilweapons.asm",
        "values": [16, 8, 4, 1],
    }
    assert [
        pick["orderSlot"][name]["branchTarget"]
        for name in ("occupiedBranch", "loop")
    ] == ["@Next", "@LoadIndex_Loop"]


def _copy_church_source_root(tmp_path):
    disasm = tmp_path / "disasm"
    source_root = UPSTREAM / "disasm"
    for relative in (
        "sf2enums.asm",
        "code/common/menus/church/churchactions_1.asm",
        "code/common/menus/church/churchactions_2.asm",
    ):
        destination = disasm / relative
        destination.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(source_root / relative, destination)
    return disasm / "code/common/menus"


def _copy_caravan_source_root(tmp_path):
    disasm = tmp_path / "disasm"
    source_root = UPSTREAM / "disasm"
    for relative in (
        "sf2enums.asm",
        "code/common/menus/caravan/caravanactions_1.asm",
        "code/common/menus/caravan/caravanactions_2.asm",
    ):
        destination = disasm / relative
        destination.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(source_root / relative, destination)
    return disasm / "code/common/menus"


def _copy_blacksmith_source_root(tmp_path):
    disasm = tmp_path / "disasm"
    source_root = UPSTREAM / "disasm"
    for relative in (
        "sf2enums.asm",
        "code/common/menus/blacksmith/blacksmithactions.asm",
        "code/common/menus/blacksmith/pickmithrilweapon.asm",
        "data/stats/allies/classes/blacksmitheligibleclasses.asm",
        "data/stats/items/mithrilweapons.asm",
    ):
        destination = disasm / relative
        destination.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(source_root / relative, destination)
    return disasm / "code/common/menus"


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("relative", "needle", "replacement"),
    (
        ("blacksmithactions.asm", "link    a6,#-24", "link    a6,#-22"),
        (
            "blacksmithactions.asm",
            "cmpi.w  #BLACKSMITH_MITHRIL_ITEM,d2",
            "cmpi.w  #COMBATANT_ITEMSLOTS,d2",
        ),
        (
            "blacksmithactions.asm",
            "cmpi.l  #BLACKSMITH_ORDER_COST,d1",
            "cmpi.l  #BLACKSMITH_MITHRIL_ITEM,d1",
        ),
        ("blacksmithactions.asm", "beq.w   @ConfirmOrder", "bne.w   @ConfirmOrder"),
        ("blacksmithactions.asm", "move.w  #80,d1", "move.w  #81,d1"),
        ("blacksmithactions.asm", "move.w  #0,(a1)", "move.b  #0,(a1)"),
        ("blacksmithactions.asm", "move.b  (a0)+,(a1)+", "move.w  (a0)+,(a1)+"),
        ("blacksmithactions.asm", "subq.b  #1,d7", "subq.w  #1,d7"),
        (
            "blacksmithactions.asm",
            "move.w  ((TARGETS_LIST_LENGTH-$1000000)).w,d7",
            "move.w  ((GENERIC_LIST_LENGTH-$1000000)).w,d7",
        ),
        (
            "blacksmithactions.asm",
            "dbf     d7,@CopyForceMembersList_Loop",
            "dbf     d7,@Loop",
        ),
        ("pickmithrilweapon.asm", "lsl.w   #3,d0", "lsl.w   #2,d0"),
        ("pickmithrilweapon.asm", "move.w  #2,d6", "move.w  #1,d6"),
        (
            "pickmithrilweapon.asm",
            "bne.w   @GetWeaponsEntryAddress",
            "beq.w   @GetWeaponsEntryAddress",
        ),
        ("pickmithrilweapon.asm", "move.b  (a0)+,d1", "move.w  (a0)+,d1"),
        ("pickmithrilweapon.asm", "cmpi.w  #0,(a0)", "cmpi.w  #1,(a0)"),
        ("pickmithrilweapon.asm", "subq.w  #1,d6", "subq.w  #2,d6"),
        (
            "pickmithrilweapon.asm",
            "dbf     d6,@FindCharacterClass_Loop",
            "dbf     d6,@FindWeaponClass_Loop",
        ),
        ("pickmithrilweapon.asm", "move.w  d0,d6", "move.w  d1,d6"),
        ("pickmithrilweapon.asm", "beq.w   @LoadIndex", "bne.w   @LoadIndex"),
        ("pickmithrilweapon.asm", "bne.w   @Next", "beq.w   @Next"),
        (
            "pickmithrilweapon.asm",
            "dbf     d7,@LoadIndex_Loop",
            "dbf     d7,@PickWeapon_Loop",
        ),
    ),
)
def test_blacksmith_source_mutations_fail_before_fixture_comparison(
    tmp_path, relative, needle, replacement
) -> None:
    root = _copy_blacksmith_source_root(tmp_path)
    source = root / "blacksmith" / relative
    source.write_text(
        source.read_text(encoding="latin-1").replace(needle, replacement, 1),
        encoding="latin-1",
    )
    with pytest.raises(ValueError, match="blacksmith"):
        _blacksmith_static_contract(root)


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("section_symbol", "needle", "replacement", "occurrence"),
    (
        ("BlacksmithAction_PlaceOrder", "cmpi.w  #-1,d0", "cmpi.w  #-2,d0", 2),
        ("BlacksmithAction_PlaceOrder", "cmpi.w  #0,d0", "cmpi.w  #1,d0", 2),
        ("BlacksmithAction_PlaceOrder", "move.w  #80,d1", "move.w  #81,d1", 1),
        ("BlacksmithAction_FulfillOrder", "cmpi.w  #2,d2", "cmpi.w  #3,d2", 3),
        (
            "BlacksmithAction_PlaceOrder",
            "jsr     j_DropItemBySlot\n                bsr.w   PickMithrilWeapon",
            "bsr.w   PickMithrilWeapon\n                jsr     j_DropItemBySlot",
            1,
        ),
    ),
)
def test_blacksmith_duplicate_branch_and_order_mutations_are_scoped(
    tmp_path, section_symbol, needle, replacement, occurrence
) -> None:
    root = _copy_blacksmith_source_root(tmp_path)
    source_path = root / "blacksmith/blacksmithactions.asm"
    source = source_path.read_text(encoding="latin-1")
    section_start = source.index(f"{section_symbol}:")
    section = source[section_start:]
    start = -1
    for _ in range(occurrence):
        start = section.index(needle, start + 1)
    section = section[:start] + section[start:].replace(needle, replacement, 1)
    source_path.write_text(source[:section_start] + section, encoding="latin-1")
    with pytest.raises(ValueError, match="blacksmith"):
        _blacksmith_static_contract(root)


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("needle", "replacement"),
    (
        ("cmpi.w  #0,d0", "cmpi.w  #3,d0"),
        ("mulu.w  #CHURCHMENU_PER_LEVEL_RAISE_COST,d1", "mulu.w  #2,d1"),
        ("bcc.s   @DoRaise", "bcs.s   @DoRaise"),
        ("andi.w  #STATUSEFFECT_POISON,d3", "andi.w  #STATUSEFFECT_CURSE,d3"),
        ("lsr.w   #2,d4", "lsr.w   #1,d4"),
        (
            "jsr     j_SetClass\n                jsr     j_Promote",
            "jsr     j_Promote\n                jsr     j_SetClass",
        ),
        ("jmp     (WitchSuspend).w", "jmp     (FadeOutToBlack).w"),
    ),
)
def test_church_source_mutations_are_route_scoped(tmp_path, needle, replacement) -> None:
    root = _copy_church_source_root(tmp_path)
    actions = root / "church/churchactions_1.asm"
    actions.write_text(
        actions.read_text(encoding="latin-1").replace(needle, replacement, 1),
        encoding="latin-1",
    )
    with pytest.raises(ValueError, match="church"):
        _church_static_contract(root)


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("needle", "replacement"),
    (
        (
            "dc.w caravanMenu_Join-rjt_CaravanMenuActions",
            "dc.w caravanMenu_Purge-rjt_CaravanMenuActions",
        ),
        ("beq.w   @ExitCaravan", "bne.w   @ExitCaravan"),
        (
            "cmpi.w  #CARAVAN_MAX_ITEMS_NUMBER,((GENERIC_LIST_LENGTH-$1000000)).w",
            "cmpi.w  #COMBATANT_ITEMSLOTS,((GENERIC_LIST_LENGTH-$1000000)).w",
        ),
        (
            "jsr     j_AddItemToCaravan\n"
            "                move.w  itemSlot(a6),d1\n"
            "                jsr     j_DropItemBySlot",
            "jsr     j_DropItemBySlot\n"
            "                move.w  itemSlot(a6),d1\n"
            "                jsr     j_AddItemToCaravan",
        ),
        (
            "btst    #ITEMTYPE_BIT_RARE,ITEMDEF_OFFSET_TYPE(a0)",
            "btst    #ITEMTYPE_BIT_UNSELLABLE,ITEMDEF_OFFSET_TYPE(a0)",
        ),
        ("move.w  ITEMDEF_OFFSET_PRICE(a0),d1", "move.w  ITEMDEF_OFFSET_TYPE(a0),d1"),
        (
            "mulu.w  #ITEMSELLPRICE_MULTIPLIER,d1",
            "mulu.w  #ITEMSELLPRICE_BITSHIFTRIGHT,d1",
        ),
        (
            "lsr.l   #ITEMSELLPRICE_BITSHIFTRIGHT,d1",
            "lsr.l   #ITEMSELLPRICE_MULTIPLIER,d1",
        ),
        (
            "; Give item\n"
            "                move.w  targetMember(a6),d0\n"
            "                move.w  itemIndex(a6),d1\n"
            "                jsr     j_AddItem\n"
            "                move.w  member(a6),d0\n"
            "                move.w  itemSlot(a6),d1\n"
            "                jsr     j_RemoveItemBySlot",
            "; Give item\n"
            "                move.w  targetMember(a6),d0\n"
            "                move.w  itemIndex(a6),d1\n"
            "                jsr     j_RemoveItemBySlot\n"
            "                move.w  member(a6),d0\n"
            "                move.w  itemSlot(a6),d1\n"
            "                jsr     j_AddItem",
        ),
        ("ITEM_SUBMENU_ACTION_EQUIP", "ITEM_SUBMENU_ACTION_DROP"),
    ),
)
def test_caravan_source_mutations_are_function_scoped(tmp_path, needle, replacement) -> None:
    root = _copy_caravan_source_root(tmp_path)
    actions = root / "caravan/caravanactions_1.asm"
    actions.write_text(
        actions.read_text(encoding="latin-1").replace(needle, replacement, 1),
        encoding="latin-1",
    )
    with pytest.raises(ValueError, match="caravan"):
        _caravan_static_contract(root)


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_caravan_nested_selector_width_and_item_rare_guards_are_scoped(tmp_path) -> None:
    root = _copy_caravan_source_root(tmp_path)
    actions = root / "caravan/caravanactions_1.asm"
    source = actions.read_text(encoding="latin-1")
    depot_start = source.index("caravanMenu_Depot:")
    changed = source[:depot_start] + source[depot_start:].replace(
        "add.w   d0,d0", "add.l   d0,d0", 1
    )
    actions.write_text(changed, encoding="latin-1")
    with pytest.raises(ValueError, match="caravan"):
        _caravan_static_contract(root)

    root = _copy_caravan_source_root(tmp_path)
    actions = root / "caravan/caravanactions_1.asm"
    source = actions.read_text(encoding="latin-1")
    item_drop_start = source.index("caravanItemSubmenu_Drop:")
    changed = source[:item_drop_start] + source[item_drop_start:].replace(
        "btst    #ITEMTYPE_BIT_RARE,ITEMDEF_OFFSET_TYPE(a0)",
        "btst    #ITEMTYPE_BIT_UNSELLABLE,ITEMDEF_OFFSET_TYPE(a0)",
        1,
    )
    actions.write_text(changed, encoding="latin-1")
    with pytest.raises(ValueError, match="caravan"):
        _caravan_static_contract(root)


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_caravan_unsellable_helper_bit_guard_is_scoped(tmp_path) -> None:
    root = _copy_caravan_source_root(tmp_path)
    helpers = root / "caravan/caravanactions_2.asm"
    helpers.write_text(
        helpers.read_text(encoding="latin-1").replace(
            "btst    #ITEMTYPE_BIT_UNSELLABLE,ITEMDEF_OFFSET_TYPE(a0)",
            "btst    #ITEMTYPE_BIT_RARE,ITEMDEF_OFFSET_TYPE(a0)",
            1,
        ),
        encoding="latin-1",
    )
    with pytest.raises(ValueError, match="caravan"):
        _caravan_static_contract(root)


def _copy_shop_source_root(tmp_path):
    disasm = tmp_path / "disasm"
    source_root = UPSTREAM / "disasm"
    for relative in (
        "sf2enums.asm",
        "code/common/tech/bytecopy.asm",
        "code/common/menus/shop/shopactions.asm",
        "code/common/menus/shopscreen.asm",
    ):
        destination = disasm / relative
        destination.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(source_root / relative, destination)
    return disasm / "code/common/menus"


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("needle", "replacement"),
    (
        ("mulu.w  #ITEMSELLPRICE_MULTIPLIER,d0", "mulu.w  #2,d0"),
        ("bcc.s   byte_2013C", "bcs.s   byte_2013C"),
        ("bcs.s   loc_206A0", "bcc.s   loc_206A0"),
        ("jsr     j_RemoveItemFromDeals", "jsr     j_RemoveItemFromDealsChanged"),
        ("bra.w   @CheckChoice_Deals", "bra.w   loc_20088"),
    ),
)
def test_shop_source_guards_reject_semantic_mutations(tmp_path, needle, replacement) -> None:
    root = _copy_shop_source_root(tmp_path)
    actions = root / "shop/shopactions.asm"
    actions.write_text(
        actions.read_text(encoding="latin-1").replace(needle, replacement, 1),
        encoding="latin-1",
    )
    with pytest.raises(ValueError, match="shop"):
        _shop_static_contract(root)


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("needle", "replacement"),
    (
        ("bne.w   @Cancel", "beq.w   @Cancel"),
        ("bne.w   @Confirm", "beq.w   @Confirm"),
        ("moveq   #-1,d0", "moveq   #-2,d0"),
        ("moveq   #-20,d1", "moveq   #-19,d1"),
        (
            "btst    #INPUT_BIT_C,((CURRENT_PLAYER_INPUT-$1000000)).w\n"
            "                bne.w   @Confirm\n"
            "                btst    #INPUT_BIT_A,((CURRENT_PLAYER_INPUT-$1000000)).w",
            "btst    #INPUT_BIT_A,((CURRENT_PLAYER_INPUT-$1000000)).w\n"
            "                bne.w   @Confirm\n"
            "                btst    #INPUT_BIT_C,((CURRENT_PLAYER_INPUT-$1000000)).w",
        ),
        ("mulu.w  #6,d0", "mulu.w  #5,d0"),
        ("btst    #INPUT_BIT_RIGHT", "btst    #INPUT_BIT_LEFT"),
        ("btst    #INPUT_BIT_LEFT", "btst    #INPUT_BIT_RIGHT"),
        ("btst    #INPUT_BIT_UP", "btst    #INPUT_BIT_DOWN"),
        ("btst    #INPUT_BIT_DOWN", "btst    #INPUT_BIT_UP"),
        (
            "move.w  #5,((CURRENT_SHOP_SELECTION-$1000000)).w",
            "move.w  #4,((CURRENT_SHOP_SELECTION-$1000000)).w",
        ),
        ("moveq   #ITEMS_PER_SHOP_PAGE,d1", "moveq   #5,d1"),
        ("lsl.w   #5,d0", "lsl.w   #4,d0"),
        (
            "lea     layout_ShopInventoryWindow(pc), a0",
            "lea     layout_ShopInventoryWindowChanged(pc), a0",
        ),
        (
            "movea.l inventoryWindowLayoutEndAddress(a6),a1",
            "movea.l inventoryWindowLayoutEndAddressChanged(a6),a1",
        ),
        ("move.w  #324,d7", "move.w  #323,d7"),
        ("move.w  #1599,d7", "move.w  #1598,d7"),
        ("move.l  #-1,(a0)+", "move.w  #-1,(a0)+"),
        ("dbf     d7,@Clear_Loop", "dbf     d7,@Clear_LoopChanged"),
        ("dbf     d7,@Loop", "dbf     d7,@LoopChanged"),
        ("bra.s   MoveSelectedItemInfoWindow", "bra.s   sub_14EC0"),
    ),
)
def test_shared_selection_source_guards_reject_semantic_mutations(
    tmp_path, needle, replacement
) -> None:
    root = _copy_shop_source_root(tmp_path)
    source = root / "shopscreen.asm"
    source.write_text(
        source.read_text(encoding="latin-1").replace(needle, replacement, 1),
        encoding="latin-1",
    )
    with pytest.raises(ValueError, match="shop"):
        _shared_selection_screen_contract(root.parents[2], root)


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_shop_buy_price_load_is_route_local(tmp_path) -> None:
    root = _copy_shop_source_root(tmp_path)
    baseline = _shop_static_contract(root)
    actions = root / "shop/shopactions.asm"
    actions.write_text(
        actions.read_text(encoding="latin-1").replace(
            "move.w  ITEMDEF_OFFSET_PRICE(a0),itemPrice(a6)",
            "move.b  ITEMDEF_OFFSET_PRICE(a0),itemPrice(a6)",
            1,
        ),
        encoding="latin-1",
    )
    changed = _shop_static_contract(root)
    assert changed["prices"]["routePriceDataflow"]["buy"] == {
        "itemDefinitionPriceLoadWidthBits": 8,
        "transformOpcodes": [],
    }
    assert changed["prices"]["routePriceDataflow"] != baseline["prices"]["routePriceDataflow"]


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_service_menu_state_machine_contract_covers_built_services() -> None:
    machines = build_menu_inventory(UPSTREAM)["menuFacts"]["serviceStateMachines"]

    assert len(machines["builtSourcePaths"]) == 8
    assert machines["sourceInventory"] == {
        "fileCount": 8,
        "sourceLineCount": 4286,
        "directCallSiteCount": 420,
        "indirectCallSiteCount": 16,
        "uniqueDirectTargetCount": 115,
        "entrySymbols": {
            "blacksmith": "BlacksmithMenu",
            "caravan": "CaravanMenu",
            "church": "ChurchMenu",
            "shop": "ShopMenu",
            "sharedSelection": "ExecuteShopScreen",
        },
    }
    assert machines["shop"]["choiceDispatch"]["fallthroughRoute"] == "deals"
    assert machines["shop"]["prices"] == {
        "itemDefinitionPriceOffsetBytes": 6,
        "routePriceDataflow": {
            "buy": {"itemDefinitionPriceLoadWidthBits": 16, "transformOpcodes": []},
            "sell": {
                "itemDefinitionPriceLoadWidthBits": 16,
                "transformOpcodes": ["mulu.w", "lsr.l"],
            },
            "repair": {"itemDefinitionPriceLoadWidthBits": 16, "transformOpcodes": ["lsr.w"]},
            "deals": {"itemDefinitionPriceLoadWidthBits": 16, "transformOpcodes": []},
        },
        "sellMultiplier": 3,
        "sellRightShiftBits": 2,
        "repairRightShiftBits": 2,
    }
    assert machines["shop"]["mutations"]["sell"] == [
        "j_IncreaseGold",
        "j_DropItemBySlot",
        "j_AddItemToDeals",
    ]
    buy_operations = machines["shop"]["routeOperations"]["buy"]
    assert buy_operations[:5] == [
        {
            "labels": ["@CheckChoice_Buy"],
            "opcode": "cmpi.w",
            "operands": ["#0", "d0"],
            "directTarget": None,
            "branchTarget": None,
        },
        {
            "labels": [],
            "opcode": "bne.w",
            "operands": ["@CheckChoice_Sell"],
            "directTarget": None,
            "branchTarget": "@CheckChoice_Sell",
        },
        {
            "labels": ["byte_200CE"],
            "opcode": "txt",
            "operands": ["162"],
            "directTarget": None,
            "branchTarget": None,
        },
        {
            "labels": [],
            "opcode": "jsr",
            "operands": ["PopulateShopInventoryList(pc)"],
            "directTarget": "PopulateShopInventoryList",
            "branchTarget": None,
        },
        {"labels": [], "opcode": "nop", "operands": [], "directTarget": None, "branchTarget": None},
    ]
    assert buy_operations[47:49] == [
        {
            "labels": [],
            "opcode": "jsr",
            "operands": ["j_GetItemBySlotAndHeldItemsNumber"],
            "directTarget": "j_GetItemBySlotAndHeldItemsNumber",
            "branchTarget": None,
        },
        {
            "labels": [],
            "opcode": "cmpi.w",
            "operands": ["#COMBATANT_ITEMSLOTS", "d2"],
            "directTarget": None,
            "branchTarget": None,
        },
    ]
    assert machines["church"]["choiceDispatch"]["comparedRouteOrder"] == [
        "raise",
        "cure",
        "promote",
    ]
    assert machines["caravan"]["dispatchTables"]["top"]["targets"] == [
        "caravanMenu_Join",
        "caravanMenu_Depot",
        "caravanMenu_Item",
        "caravanMenu_Purge",
    ]
    assert machines["caravan"]["routeDerived"]["depot"]["deriveNormalMutationCalls"] == [
        "j_AddItem",
        "j_RemoveItemFromCaravan",
    ]
    assert machines["blacksmith"]["derived"]["place"]["mutationCalls"] == [
        "j_DecreaseGold",
        "j_DropItemBySlot",
        "PickMithrilWeapon",
        "j_ClearFlag",
    ]
    shared_selection = machines["sharedSelectionScreen"]
    assert shared_selection["entrySymbol"] == "ExecuteShopScreen"
    assert shared_selection["constants"]["ITEMS_PER_SHOP_PAGE"] == 6
    assert set(shared_selection["routineOperations"]) == {
        "ExecuteShopScreen",
        "LoadShopInventoryHighlightSprites",
        "WriteGoldAmount",
        "WriteItemNameAndGoldAmount",
        "LoadItemIconsAndPriceTagTiles",
        "LoadPriceTagTiles",
        "LoadIconPixelsInShopScreen",
        "GetCurrentShopSelection",
        "sub_14D0C",
        "sub_14D6A",
        "sub_14DBE",
        "sub_14DC0",
        "sub_14E06",
        "sub_14E5E",
        "ShiftShopInventoryWindowLayout",
        "sub_14EC0",
        "MoveSelectedItemInfoWindow",
    }
    assert shared_selection["inputBranches"] == {
        "sourceOrder": ["cancel", "confirmC", "confirmA"],
        "cancel": {"button": "B", "testIndex": 123, "branchIndex": 124},
        "confirmC": {"button": "C", "testIndex": 125, "branchIndex": 126},
        "confirmA": {"button": "A", "testIndex": 127, "branchIndex": 128},
        "cancelResult": {"name": "minusOneResult", "instructionIndex": 132},
        "confirmSelectionFormula": {
            "routine": "ExecuteShopScreen",
            "pageLoadIndex": 134,
            "pageMultiplierIndex": 135,
            "selectionAddIndex": 136,
            "listBaseIndex": 137,
            "resultByteReadIndex": 138,
            "resultMaskIndex": 139,
        },
    }
    navigation = shared_selection["navigation"]
    assert navigation == {
        "right": {
            "routine": "ExecuteShopScreen",
            "references": [
                {"name": name, "instructionIndex": index}
                for name, index in (
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
                )
            ],
        },
        "left": {
            "routine": "ExecuteShopScreen",
            "references": [
                {"name": name, "instructionIndex": index}
                for name, index in (
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
                )
            ],
        },
        "up": {
            "routine": "ExecuteShopScreen",
            "references": [
                {"name": name, "instructionIndex": index}
                for name, index in (
                    ("inputTest", 87),
                    ("inputAbsentBranch", 88),
                    ("pageZeroTest", 89),
                    ("pageZeroBoundBranch", 90),
                    ("pageDecrement", 91),
                    ("shiftDirectionSet", 93),
                    ("scrollHelperConvergence", 94),
                )
            ],
        },
        "down": {
            "routine": "ExecuteShopScreen",
            "references": [
                {"name": name, "instructionIndex": index}
                for name, index in (
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
                )
            ],
        },
    }
    assert shared_selection["routineOperations"]["LoadShopInventoryHighlightSprites"][
        shared_selection["highlightSemantics"]["selectionShiftIndex"]
    ]["operands"] == ["#5", "d0"]
    assert shared_selection["resourceTransfers"]["LoadItemIconsAndPriceTagTiles"] == {
        "routine": "LoadItemIconsAndPriceTagTiles",
        "copyBytesTransfers": [
            {
                "name": "inventoryLayoutCopy",
                "sourceOperandInstructionIndex": 1,
                "destinationOperandInstructionIndex": 0,
                "storedCountOperandInstructionIndex": 2,
                "storedCountValue": 324,
                "copyCallInstructionIndex": 3,
                "copyCountUnit": "bytes",
                "transferredByteCount": 324,
            }
        ],
        "loopWrites": [
            {
                "name": "clearLoop",
                "sourceOperandInstructionIndex": 7,
                "destinationOperandInstructionIndex": 7,
                "storedCountOperandInstructionIndex": 6,
                "storedCountValue": 1599,
                "writeInstructionIndex": 7,
                "writeOpcodeWidthBits": 32,
                "loopInstructionIndex": 8,
                "inclusiveCounter": True,
                "iterationCount": 1600,
                "longwordWriteCount": 1600,
                "loopTarget": "@Clear_Loop",
                "exitConvergenceInstructionIndex": 9,
            },
            {
                "name": "itemLoop",
                "sourceOperandInstructionIndex": 28,
                "destinationOperandInstructionIndex": 38,
                "storedCountOperandInstructionIndex": 22,
                "storedCountOperand": "d1",
                "writeInstructionIndex": 38,
                "writeOpcodeWidthBits": 16,
                "loopInstructionIndex": 41,
                "inclusiveCounter": True,
                "loopTarget": "@Main_Loop",
                "exitConvergenceInstructionIndex": 42,
            },
        ],
        "vintDmaArgumentInstructionIndexes": [42, 43, 44, 45, 46, 47],
        "terminalConvergence": {"name": "terminalOperation", "instructionIndex": 48},
    }
    assert shared_selection["resourceTransfers"]["WriteItemNameAndGoldAmount"][
        "namedOperations"
    ][0] == {
        "name": "itemNameText",
        "sourceOperandInstructionIndex": 6,
        "destinationOperandInstructionIndex": 7,
        "preCallD1ArgumentInstructionIndex": 9,
        "preCallD1ArgumentValue": -20,
        "writeCallInstructionIndex": 10,
    }
    assert machines["staticBoundary"]["callerDependentServiceAdmissionAndReturnState"] == "inferred"
    assert machines["staticBoundary"]["persistenceAcrossMapLoadSaveAndStoryProgress"] == "unknown"


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_shared_selection_contract_matches_fixture_and_rejects_nested_drift() -> None:
    fixture = load_json(FIXTURE)
    shared = build_menu_inventory(UPSTREAM)["menuFacts"]["serviceStateMachines"][
        "sharedSelectionScreen"
    ]
    assert (
        shared == fixture["expected"]["menuFacts"]["serviceStateMachines"]["sharedSelectionScreen"]
    )
    instruction_schema = load_json(INSTRUCTION_SCHEMA)
    shared_schema = load_json(SHARED_SELECTION_SCHEMA)
    assert instruction_schema["additionalProperties"] is False
    instruction_ref = {"$ref": instruction_schema["$id"]}
    routine_schemas = shared_schema["properties"]["routineOperations"]["properties"].values()
    assert all(
        records == {"type": "array", "items": instruction_ref}
        for records in routine_schemas
    )
    assert len(list(routine_schemas)) == 17
    assert len(json.dumps(shared_schema, sort_keys=True)) < 50_000
    assert len(json.dumps(instruction_schema, sort_keys=True)) < 1_000

    def expect_invalid(mutate) -> None:
        broken = deepcopy(fixture)
        mutate(broken["expected"]["menuFacts"]["serviceStateMachines"]["sharedSelectionScreen"])
        with pytest.raises(ValueError, match="sharedSelectionScreen"):
            validate_json(broken, FIXTURE_SCHEMA, owner="sharedSelectionScreen contract")

    expect_invalid(lambda shared: shared.pop("navigation"))
    expect_invalid(lambda shared: shared["inputBranches"]["cancel"].pop("branchIndex"))
    expect_invalid(
        lambda shared: shared["highlightSemantics"].__setitem__("unexpectedCoordinate", 0)
    )
    expect_invalid(
        lambda shared: shared["resourceTransfers"]["LoadPriceTagTiles"].pop("loopWrites")
    )
    expect_invalid(
        lambda shared: shared["routineOperations"]["ExecuteShopScreen"][0].pop("opcode")
    )
    expect_invalid(
        lambda shared: shared["routineOperations"]["ExecuteShopScreen"][0].__setitem__(
            "unexpectedOperand", 0
        )
    )
    broken = deepcopy(fixture)
    broken["expected"]["menuFacts"]["serviceStateMachines"]["sharedSelectionScreen"][
        "resourceTransfers"
    ]["WriteItemNameAndGoldAmount"]["namedOperations"][0].pop("preCallD1ArgumentValue")
    validate_json(broken, FIXTURE_SCHEMA, owner="schema-valid shared-selection drift")
    with pytest.raises(ValueError, match="common menus model drift"):
        _verify_menu_fixture_owner(
            broken,
            build_menu_inventory(UPSTREAM),
            rom_manifest=load_json(menus_module.ROM_MANIFEST),
        )


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_shop_contract_matches_fixture_and_schemas_are_closed() -> None:
    fixture = load_json(FIXTURE)
    output = build_menu_inventory(UPSTREAM)
    assert (
        output["menuFacts"]["serviceStateMachines"]["shop"]
        == fixture["expected"]["menuFacts"]["serviceStateMachines"]["shop"]
    )
    shop_schema = load_json(SHOP_SCHEMA)

    def closed(value):
        if isinstance(value, dict):
            if value.get("type") == "object":
                assert value.get("additionalProperties") is False
                assert set(value["required"]) == set(value["properties"])
            for child in value.values():
                closed(child)
        elif isinstance(value, list):
            for child in value:
                closed(child)

    closed(shop_schema)

    def expect_structurally_invalid(mutate) -> None:
        broken = deepcopy(fixture)
        mutate(broken["expected"]["menuFacts"]["serviceStateMachines"]["shop"])
        with pytest.raises(ValueError, match="shop"):
            validate_json(broken, FIXTURE_SCHEMA, owner="shop contract")

    def expect_owner_rejected(mutate) -> None:
        broken = deepcopy(fixture)
        mutate(broken["expected"]["menuFacts"]["serviceStateMachines"]["shop"])
        validate_json(broken, FIXTURE_SCHEMA, owner="schema-valid shop drift")
        with pytest.raises(ValueError, match="common menus model drift"):
            _verify_menu_fixture_owner(
                broken,
                output,
                rom_manifest=load_json(menus_module.ROM_MANIFEST),
            )

    expect_structurally_invalid(lambda shop: shop.__setitem__("extra", True))
    expect_structurally_invalid(lambda shop: shop.pop("prices"))
    expect_structurally_invalid(lambda shop: shop.__setitem__("priceData", shop.pop("prices")))
    expect_owner_rejected(lambda shop: shop["prices"].__setitem__("sellMultiplier", 2))
    expect_owner_rejected(
        lambda shop: shop["routeCalls"].__setitem__("buy", shop["routeCalls"]["buy"][::-1])
    )
    expect_owner_rejected(lambda shop: shop["eligibility"].__setitem__("inventoryCapacity", 5))


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_church_contract_matches_fixture_and_uses_compact_instruction_schema() -> None:
    fixture = load_json(FIXTURE)
    output = build_menu_inventory(UPSTREAM)
    church = output["menuFacts"]["serviceStateMachines"]["church"]
    assert church == fixture["expected"]["menuFacts"]["serviceStateMachines"]["church"]
    church_schema = load_json(CHURCH_SCHEMA)
    instruction_schema = load_json(INSTRUCTION_SCHEMA)
    route_schemas = church_schema["properties"]["routeOperations"]["properties"]
    assert all(
        route_schema
        == {"type": "array", "items": {"$ref": instruction_schema["$id"]}}
        for route_schema in route_schemas.values()
    )

    def expect_structurally_invalid(mutate) -> None:
        broken = deepcopy(fixture)
        mutate(broken["expected"]["menuFacts"]["serviceStateMachines"]["church"])
        with pytest.raises(ValueError, match="church"):
            validate_json(broken, FIXTURE_SCHEMA, owner="church contract")

    def expect_owner_rejected(mutate) -> None:
        broken = deepcopy(fixture)
        mutate(broken["expected"]["menuFacts"]["serviceStateMachines"]["church"])
        validate_json(broken, FIXTURE_SCHEMA, owner="schema-valid church drift")
        with pytest.raises(ValueError, match="common menus model drift"):
            _verify_menu_fixture_owner(
                broken,
                output,
                rom_manifest=load_json(menus_module.ROM_MANIFEST),
            )

    expect_structurally_invalid(lambda church: church.__setitem__("extra", True))
    expect_structurally_invalid(lambda church: church.pop("constants"))
    expect_structurally_invalid(
        lambda church: church.__setitem__("constantData", church.pop("constants"))
    )
    expect_owner_rejected(lambda church: church["constants"].__setitem__("STATUSEFFECT_POISON", 3))
    expect_owner_rejected(
        lambda church: church["routeOperations"].__setitem__(
            "raise", church["routeOperations"]["raise"][::-1]
        )
    )
    expect_owner_rejected(
        lambda church: church["routeDerived"]["promote"].__setitem__("minimumLevel", 19)
    )


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_blacksmith_contract_matches_fixture_and_uses_compact_instruction_schema() -> None:
    fixture = load_json(FIXTURE)
    output = build_menu_inventory(UPSTREAM)
    blacksmith = output["menuFacts"]["serviceStateMachines"]["blacksmith"]
    assert blacksmith == fixture["expected"]["menuFacts"]["serviceStateMachines"]["blacksmith"]
    assert blacksmith["derived"] == fixture["expected"]["menuFacts"]["serviceStateMachines"][
        "blacksmith"
    ]["derived"]
    assert blacksmith["sourceRanges"] == [
        {
            "path": "code/common/menus/blacksmith/blacksmithactions.asm",
            "startAddress": 137786,
            "endAddressExclusive": 138934,
            "physicalSpanBytes": 1148,
        },
        {
            "path": "code/common/menus/blacksmith/pickmithrilweapon.asm",
            "startAddress": 138966,
            "endAddressExclusive": 139106,
            "physicalSpanBytes": 140,
        },
    ]
    blacksmith_schema = load_json(BLACKSMITH_SCHEMA)
    instruction_schema = load_json(INSTRUCTION_SCHEMA)
    function_schemas = blacksmith_schema["properties"]["functionOperations"]["properties"]
    assert all(
        schema == {"type": "array", "items": {"$ref": instruction_schema["$id"]}}
        for schema in function_schemas.values()
    )

    def expect_structurally_invalid(mutate) -> None:
        broken = deepcopy(fixture)
        mutate(broken["expected"]["menuFacts"]["serviceStateMachines"]["blacksmith"])
        with pytest.raises(ValueError, match="blacksmith"):
            validate_json(broken, FIXTURE_SCHEMA, owner="blacksmith contract")

    def expect_owner_rejected(mutate) -> None:
        broken = deepcopy(fixture)
        mutate(broken["expected"]["menuFacts"]["serviceStateMachines"]["blacksmith"])
        validate_json(broken, FIXTURE_SCHEMA, owner="schema-valid blacksmith drift")
        with pytest.raises(ValueError, match="common menus model drift"):
            _verify_menu_fixture_owner(
                broken,
                output,
                rom_manifest=load_json(menus_module.ROM_MANIFEST),
            )

    expect_structurally_invalid(lambda blacksmith: blacksmith.pop("constants"))
    expect_structurally_invalid(lambda blacksmith: blacksmith.__setitem__("extra", True))
    expect_structurally_invalid(
        lambda blacksmith: blacksmith["derived"]["orders"].pop("slotWidthBytes")
    )
    expect_structurally_invalid(
        lambda blacksmith: blacksmith["derived"]["process"]["forceCopy"].pop(
            "entryCopyOperands"
        )
    )
    expect_structurally_invalid(
        lambda blacksmith: blacksmith["derived"]["process"]["readiness"].pop(
            "checkFlagLoad"
        )
    )
    expect_structurally_invalid(
        lambda blacksmith: blacksmith["derived"]["fulfill"].__setitem__(
            "unexpectedMutation", True
        )
    )
    expect_owner_rejected(
        lambda blacksmith: blacksmith["derived"]["pick"].__setitem__("rowStrideBytes", 4)
    )
    expect_owner_rejected(
        lambda blacksmith: blacksmith["functionOperations"].__setitem__(
            "PickMithrilWeapon", blacksmith["functionOperations"]["PickMithrilWeapon"][::-1]
        )
    )


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_caravan_contract_matches_fixture_and_uses_compact_instruction_schema() -> None:
    fixture = load_json(FIXTURE)
    output = build_menu_inventory(UPSTREAM)
    caravan = output["menuFacts"]["serviceStateMachines"]["caravan"]
    assert caravan == fixture["expected"]["menuFacts"]["serviceStateMachines"]["caravan"]
    assert caravan["routeDerived"] == {
        "join": {
            "battlePartyCapacity": 12,
            "capacityBranchOpcode": "bcc.s",
            "capacityBranchTarget": "@ChooseRelief",
            "partyMutationCalls": [
                "j_JoinBattleParty",
                "j_LeaveBattleParty",
                "j_JoinBattleParty",
            ],
        },
        "purge": {"partyMutationCalls": ["j_LeaveBattleParty"]},
        "depot": {
            "storedItemCapacity": 64,
            "depositCapacityBranchOpcode": "bcc.s",
            "depositCapacityBranchTarget": "@Exit",
            "depositMutationCalls": ["j_AddItemToCaravan", "j_DropItemBySlot"],
            "recipientItemCapacity": 4,
            "deriveCapacityBranchTarget": "@Exchange",
            "deriveNormalMutationCalls": ["j_AddItem", "j_RemoveItemFromCaravan"],
            "deriveExchangeMutationCalls": [
                "j_RemoveItemBySlot",
                "j_RemoveItemFromCaravan",
                "j_AddItem",
                "j_AddItemToCaravan",
            ],
            "dropMutationCalls": [
                "j_RemoveItemFromCaravan",
                "j_GetItemDefinitionAddress",
                "j_AddItemToDeals",
            ],
            "rareBit": 3,
            "unsellableBit": 4,
            "lookPrice": {
                "itemDefinitionOffsetBytes": 6,
                "loadWidthBits": 16,
                "multiplyConstant": 3,
                "rightShiftBits": 2,
            },
        },
        "item": {
            "useMutationCalls": ["UseItemOnField", "j_RemoveItemBySlot"],
            "recipientItemCapacity": 4,
            "giveCapacityBranchTarget": "@ExchangeItems",
            "giveSelfMutationCalls": ["j_RemoveItemBySlot", "j_AddItem"],
            "giveNormalMutationCalls": ["j_AddItem", "j_RemoveItemBySlot"],
            "giveExchangeMutationCalls": [
                "j_RemoveItemBySlot",
                "j_RemoveItemBySlot",
                "j_AddItem",
                "j_AddItem",
            ],
            "equipSelectionAction": "ITEM_SUBMENU_ACTION_EQUIP",
            "dropMutationCalls": [
                "j_DropItemBySlot",
                "j_GetItemDefinitionAddress",
                "j_AddItemToDeals",
            ],
            "rareBit": 3,
        },
    }
    assert caravan["sourceRanges"] == [
        {
            "path": "code/common/menus/caravan/caravanactions_1.asm",
            "startAddress": 139218,
            "endAddressExclusive": 141474,
            "physicalSpanBytes": 2256,
        },
        {
            "path": "code/common/menus/caravan/caravanactions_2.asm",
            "startAddress": 141480,
            "endAddressExclusive": 141770,
            "physicalSpanBytes": 290,
        },
    ]
    assert caravan["externalDirectCallerOccurrences"] == {
        "code/gameflow/exploration/explorationvints.asm": [
            {
                "instructionTarget": "j_CaravanMenu",
                "effectiveTarget": "CaravanMenu",
                "siteCount": 1,
            }
        ],
        "code/gameflow/special/battletest.asm": [
            {
                "instructionTarget": "j_CaravanMenu",
                "effectiveTarget": "CaravanMenu",
                "siteCount": 1,
            }
        ],
    }
    assert caravan["internalEffectiveDirectCallSiteCounts"]["CopyCaravanItems"] == 4
    assert caravan["externalEffectiveDirectCallSiteCounts"]["CaravanMenu"] == 2
    caravan_schema = load_json(CARAVAN_SCHEMA)
    instruction_schema = load_json(INSTRUCTION_SCHEMA)
    for collection in ("routeOperations", "helperOperations"):
        schemas = caravan_schema["properties"][collection]["properties"]
        assert all(
            schema == {"type": "array", "items": {"$ref": instruction_schema["$id"]}}
            for schema in schemas.values()
        )

    def expect_structurally_invalid(mutate) -> None:
        broken = deepcopy(fixture)
        mutate(broken["expected"]["menuFacts"]["serviceStateMachines"]["caravan"])
        with pytest.raises(ValueError, match="caravan"):
            validate_json(broken, FIXTURE_SCHEMA, owner="caravan contract")

    def expect_owner_rejected(mutate) -> None:
        broken = deepcopy(fixture)
        mutate(broken["expected"]["menuFacts"]["serviceStateMachines"]["caravan"])
        validate_json(broken, FIXTURE_SCHEMA, owner="schema-valid Caravan drift")
        with pytest.raises(ValueError, match="common menus model drift"):
            _verify_menu_fixture_owner(
                broken,
                output,
                rom_manifest=load_json(menus_module.ROM_MANIFEST),
            )

    expect_structurally_invalid(lambda caravan: caravan.__setitem__("extra", True))
    expect_structurally_invalid(lambda caravan: caravan.pop("constants"))
    expect_owner_rejected(lambda caravan: caravan["constants"].__setitem__("FORCE_MAX_SIZE", 13))
    expect_owner_rejected(
        lambda caravan: caravan["sourceRanges"][0].pop("physicalSpanBytes")
    )
    expect_structurally_invalid(lambda caravan: caravan["routeDerived"]["depot"].pop("lookPrice"))
    expect_owner_rejected(
        lambda caravan: caravan["dispatchTables"]["top"].__setitem__(
            "targets", caravan["dispatchTables"]["top"]["targets"][::-1]
        )
    )
    expect_owner_rejected(
        lambda caravan: caravan["routeOperations"].__setitem__(
            "itemGive", caravan["routeOperations"]["itemGive"][::-1]
        )
    )


def test_common_menus_roots_reuse_local_service_components() -> None:
    output_schema = load_json(OUTPUT_SCHEMA)
    fixture_schema = load_json(FIXTURE_SCHEMA)
    service_schema = load_json(SERVICE_STATE_MACHINES_SCHEMA)
    assert "definitions" not in output_schema
    assert "definitions" not in fixture_schema
    service_ref = {"$ref": service_schema["$id"]}
    assert (
        output_schema["properties"]["menuFacts"]["properties"]["serviceStateMachines"]
        == service_ref
    )
    assert (
        fixture_schema["properties"]["expected"]["properties"]["menuFacts"]["properties"]
        ["serviceStateMachines"]
        == service_ref
    )
    expected_refs = {
        "sharedSelectionScreen": load_json(SHARED_SELECTION_SCHEMA)["$id"],
        "shop": load_json(SHOP_SCHEMA)["$id"],
        "church": load_json(CHURCH_SCHEMA)["$id"],
        "caravan": load_json(CARAVAN_SCHEMA)["$id"],
        "blacksmith": load_json(BLACKSMITH_SCHEMA)["$id"],
    }
    service_properties = service_schema["properties"]
    for field, reference in expected_refs.items():
        assert service_properties[field] == {"$ref": reference}


def test_common_menus_schema_composition_audit_stays_local_and_golden_free() -> None:
    schema_paths = [
        INSTRUCTION_SCHEMA,
        SHOP_SCHEMA,
        CHURCH_SCHEMA,
        CARAVAN_SCHEMA,
        BLACKSMITH_SCHEMA,
        SHARED_SELECTION_SCHEMA,
        SERVICE_STATE_MACHINES_SCHEMA,
        OUTPUT_SCHEMA,
        FIXTURE_SCHEMA,
    ]
    report = schema_composition_audit(schema_paths)
    assert report["schemaCount"] == 9
    assert report["totalSizeBytes"] < 200_000
    assert report["constCount"] == 4
    assert report["constPayloadBytes"] == 58
    assert report["largeConstCount"] == 0
    assert report["referencedResourceCount"] == 7
    assert report["unresolvedReferences"] == []
    assert report["duplicateBodyGroups"] == []
    components = report["files"][:7]
    assert all(component["constCount"] == 0 for component in components)


def test_common_menus_fixture_owner_rejects_schema_valid_exact_drift() -> None:
    fixture = load_json(FIXTURE)
    canonical_output = {
        "upstream": {"commit": fixture["upstreamCommit"]},
        "representativeAddresses": fixture["function"],
        "menuFacts": fixture["expected"]["menuFacts"],
        "alternateSource": fixture["expected"]["alternateSource"],
    }
    cases = [
        (
            lambda broken: broken.__setitem__("romSha256", "0" * 64),
            "common menus provenance drift",
        ),
        (
            lambda broken: broken.__setitem__("upstreamCommit", "0" * 40),
            "common menus provenance drift",
        ),
        (
            lambda broken: broken["function"].__setitem__("ShopMenu", 131174),
            "common menus H1 address drift",
        ),
        (
            lambda broken: broken["expected"]["alternateSource"].__setitem__(
                "alternateExcludedFromStrictReach", False
            ),
            "common menus alternate-source drift",
        ),
    ]
    for mutate, message in cases:
        broken = deepcopy(fixture)
        mutate(broken)
        validate_json(broken, FIXTURE_SCHEMA, owner="schema-valid common-menu owner drift")
        with pytest.raises(ValueError, match=message):
            _verify_menu_fixture_owner(
                broken,
                canonical_output,
                rom_manifest=load_json(menus_module.ROM_MANIFEST),
            )


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_common_menus_verifier_rejects_schema_valid_golden_drift_before_write(
    tmp_path, monkeypatch
) -> None:
    broken = deepcopy(load_json(FIXTURE))
    broken["expected"]["menuFacts"]["serviceStateMachines"]["shop"]["prices"][
        "sellMultiplier"
    ] = 2
    fixture_path = tmp_path / "common-menus.json"
    fixture_path.write_text(json.dumps(broken, indent=2) + "\n", encoding="utf-8")
    validate_json(broken, FIXTURE_SCHEMA, owner="schema-valid common-menu golden drift")

    output_path = tmp_path / "output.json"
    monkeypatch.setattr(menus_module, "FIXTURE", fixture_path)
    with pytest.raises(ValueError, match="common menus model drift"):
        verify_menu_inventory(UPSTREAM, output_path=output_path)
    assert not output_path.exists()
