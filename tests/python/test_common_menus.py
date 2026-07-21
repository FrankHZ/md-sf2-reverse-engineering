from __future__ import annotations

import pytest

from sf2tool.h2.menus import build_menu_inventory
from sf2tool.paths import repo_path

UPSTREAM = repo_path("local/upstream/SF2DISASM")


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
    assert machines["shop"]["choiceOrder"] == ["buy", "sell", "repair", "deals"]
    assert machines["shop"]["confirmedEffects"]["sell"] == [
        "increase-gold",
        "drop-item-by-slot",
        "rare-item-adds-to-deals",
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
