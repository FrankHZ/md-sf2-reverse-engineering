import shutil
from copy import deepcopy

import pytest

from sf2tool.h2.menus import (
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
    assert machines["church"]["choiceOrder"] == ["raise", "cure", "promote", "save"]
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
