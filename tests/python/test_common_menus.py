import shutil
from copy import deepcopy

import pytest

from sf2tool.h2.menus import (
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
    assert machines["caravan"]["choiceOrder"] == ["join", "depot", "item", "purge"]
    assert machines["caravan"]["depot"]["choiceOrder"] == ["look", "deposit", "derive", "drop"]
    assert machines["caravan"]["depot"]["effects"]["derive"] == [
        "add-item",
        "remove-item-from-caravan",
    ]
    assert machines["blacksmith"]["noDiamondMenu"] is True
    assert machines["blacksmith"]["placementEffects"][-1] == "clear-flag-80"
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
