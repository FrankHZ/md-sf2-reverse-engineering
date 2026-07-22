import shutil
from copy import deepcopy

import pytest

from sf2tool.h2.menus import (
    _blacksmith_static_contract,
    _caravan_static_contract,
    _church_static_contract,
    _shop_direct_call_occurrences,
    _shop_static_contract,
    build_menu_inventory,
)
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

UPSTREAM = repo_path("local/upstream/SF2DISASM")
OUTPUT_SCHEMA = repo_path("schemas/common-menus-static.schema.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-common-menus-static-fixture.schema.json")
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
    assert machines["sharedSelectionScreen"]["cancelResult"] == -1
    assert machines["sharedSelectionScreen"]["confirmButtons"] == ["A", "C"]
    assert (
        machines["sharedSelectionScreen"]["selectionAddressing"]
        == "page-times-items-per-page-plus-selection"
    )
    assert machines["staticBoundary"]["callerDependentServiceAdmissionAndReturnState"] == "inferred"
    assert machines["staticBoundary"]["persistenceAcrossMapLoadSaveAndStoryProgress"] == "unknown"


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_shop_contract_matches_fixture_and_schemas_are_closed() -> None:
    fixture = load_json(FIXTURE)
    output = build_menu_inventory(UPSTREAM)
    assert (
        output["menuFacts"]["serviceStateMachines"]["shop"]
        == fixture["expected"]["menuFacts"]["serviceStateMachines"]["shop"]
    )
    output_schema = load_json(OUTPUT_SCHEMA)
    fixture_schema = load_json(FIXTURE_SCHEMA)
    assert output_schema["definitions"]["shopFacts"] == fixture_schema["definitions"]["shopFacts"]

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

    closed(output_schema["definitions"]["shopFacts"])
    def expect_invalid(mutate) -> None:
        broken = deepcopy(fixture)
        mutate(broken["expected"]["menuFacts"]["serviceStateMachines"]["shop"])
        with pytest.raises(ValueError, match="shop"):
            validate_json(broken, FIXTURE_SCHEMA, owner="shop contract")

    expect_invalid(lambda shop: shop.__setitem__("extra", True))
    expect_invalid(lambda shop: shop.pop("prices"))
    expect_invalid(lambda shop: shop.__setitem__("priceData", shop.pop("prices")))
    expect_invalid(lambda shop: shop["prices"].__setitem__("sellMultiplier", 2))
    expect_invalid(
        lambda shop: shop["routeCalls"].__setitem__("buy", shop["routeCalls"]["buy"][::-1])
    )
    expect_invalid(lambda shop: shop["eligibility"].__setitem__("inventoryCapacity", 5))


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_church_contract_matches_fixture_and_uses_compact_instruction_schema() -> None:
    fixture = load_json(FIXTURE)
    church = build_menu_inventory(UPSTREAM)["menuFacts"]["serviceStateMachines"]["church"]
    assert church == fixture["expected"]["menuFacts"]["serviceStateMachines"]["church"]
    output_schema = load_json(OUTPUT_SCHEMA)
    fixture_schema = load_json(FIXTURE_SCHEMA)
    assert (
        output_schema["definitions"]["churchFacts"]
        == fixture_schema["definitions"]["churchFacts"]
    )
    assert (
        output_schema["definitions"]["churchInstructionRecord"]
        == fixture_schema["definitions"]["churchInstructionRecord"]
    )
    route_schemas = output_schema["definitions"]["churchFacts"]["properties"]["routeOperations"][
        "properties"
    ]
    assert all(
        route_schema["allOf"][0]["items"] == {"$ref": "#/definitions/churchInstructionRecord"}
        and isinstance(route_schema["allOf"][1]["const"], list)
        for route_schema in route_schemas.values()
    )
    assert "properties" not in route_schemas["raise"]["allOf"][1]

    def expect_invalid(mutate) -> None:
        broken = deepcopy(fixture)
        mutate(broken["expected"]["menuFacts"]["serviceStateMachines"]["church"])
        with pytest.raises(ValueError, match="church"):
            validate_json(broken, FIXTURE_SCHEMA, owner="church contract")

    expect_invalid(lambda church: church.__setitem__("extra", True))
    expect_invalid(lambda church: church.pop("constants"))
    expect_invalid(lambda church: church.__setitem__("constantData", church.pop("constants")))
    expect_invalid(lambda church: church["constants"].__setitem__("STATUSEFFECT_POISON", 3))
    expect_invalid(
        lambda church: church["routeOperations"].__setitem__(
            "raise", church["routeOperations"]["raise"][::-1]
        )
    )
    expect_invalid(
        lambda church: church["routeDerived"]["promote"].__setitem__("minimumLevel", 19)
    )


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_blacksmith_contract_matches_fixture_and_uses_compact_instruction_schema() -> None:
    fixture = load_json(FIXTURE)
    blacksmith = build_menu_inventory(UPSTREAM)["menuFacts"]["serviceStateMachines"]["blacksmith"]
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
    output_schema = load_json(OUTPUT_SCHEMA)
    fixture_schema = load_json(FIXTURE_SCHEMA)
    assert (
        output_schema["definitions"]["blacksmithFacts"]
        == fixture_schema["definitions"]["blacksmithFacts"]
    )
    function_schemas = output_schema["definitions"]["blacksmithFacts"]["properties"][
        "functionOperations"
    ]["properties"]
    assert all(
        schema["allOf"][0]["items"] == {"$ref": "#/definitions/blacksmithInstructionRecord"}
        and isinstance(schema["allOf"][1]["const"], list)
        for schema in function_schemas.values()
    )

    def expect_invalid(mutate) -> None:
        broken = deepcopy(fixture)
        mutate(broken["expected"]["menuFacts"]["serviceStateMachines"]["blacksmith"])
        with pytest.raises(ValueError, match="blacksmith"):
            validate_json(broken, FIXTURE_SCHEMA, owner="blacksmith contract")

    expect_invalid(lambda blacksmith: blacksmith.pop("constants"))
    expect_invalid(lambda blacksmith: blacksmith.__setitem__("extra", True))
    expect_invalid(lambda blacksmith: blacksmith["derived"]["orders"].pop("slotWidthBytes"))
    expect_invalid(
        lambda blacksmith: blacksmith["derived"]["process"]["forceCopy"].pop(
            "entryCopyOperands"
        )
    )
    expect_invalid(
        lambda blacksmith: blacksmith["derived"]["process"]["readiness"].pop(
            "checkFlagLoad"
        )
    )
    expect_invalid(
        lambda blacksmith: blacksmith["derived"]["fulfill"].__setitem__(
            "unexpectedMutation", True
        )
    )
    expect_invalid(
        lambda blacksmith: blacksmith["derived"]["pick"].__setitem__("rowStrideBytes", 4)
    )
    expect_invalid(
        lambda blacksmith: blacksmith["functionOperations"].__setitem__(
            "PickMithrilWeapon", blacksmith["functionOperations"]["PickMithrilWeapon"][::-1]
        )
    )


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_caravan_contract_matches_fixture_and_uses_compact_instruction_schema() -> None:
    fixture = load_json(FIXTURE)
    caravan = build_menu_inventory(UPSTREAM)["menuFacts"]["serviceStateMachines"]["caravan"]
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
    output_schema = load_json(OUTPUT_SCHEMA)
    fixture_schema = load_json(FIXTURE_SCHEMA)
    assert (
        output_schema["definitions"]["caravanFacts"]
        == fixture_schema["definitions"]["caravanFacts"]
    )
    assert (
        output_schema["definitions"]["caravanInstructionRecord"]
        == fixture_schema["definitions"]["caravanInstructionRecord"]
    )
    for collection in ("routeOperations", "helperOperations"):
        schemas = output_schema["definitions"]["caravanFacts"]["properties"][collection][
            "properties"
        ]
        assert all(
            schema["allOf"][0]["items"] == {"$ref": "#/definitions/caravanInstructionRecord"}
            and isinstance(schema["allOf"][1]["const"], list)
            for schema in schemas.values()
        )

    def expect_invalid(mutate) -> None:
        broken = deepcopy(fixture)
        mutate(broken["expected"]["menuFacts"]["serviceStateMachines"]["caravan"])
        with pytest.raises(ValueError, match="caravan"):
            validate_json(broken, FIXTURE_SCHEMA, owner="caravan contract")

    expect_invalid(lambda caravan: caravan.__setitem__("extra", True))
    expect_invalid(lambda caravan: caravan.pop("constants"))
    expect_invalid(lambda caravan: caravan["constants"].__setitem__("FORCE_MAX_SIZE", 13))
    expect_invalid(lambda caravan: caravan["sourceRanges"][0].pop("physicalSpanBytes"))
    expect_invalid(lambda caravan: caravan["routeDerived"]["depot"].pop("lookPrice"))
    expect_invalid(
        lambda caravan: caravan["dispatchTables"]["top"].__setitem__(
            "targets", caravan["dispatchTables"]["top"]["targets"][::-1]
        )
    )
    expect_invalid(
        lambda caravan: caravan["routeOperations"].__setitem__(
            "itemGive", caravan["routeOperations"]["itemGive"][::-1]
        )
    )
