"""Source operands for shop and church services."""

from __future__ import annotations

import re
from typing import Any

from sf2tool.h2.battle_global_data import _arguments, _tokens


def shop_service(compiler: Any, index: int) -> dict[str, Any]:
    """Resolve the selected ordinary inventory in source order."""
    path = "disasm/data/stats/items/shopinventories.asm"
    compiler.sources.add(path)
    source = (compiler.upstream / path).read_text(encoding="utf-8")
    source = re.sub(r"&\s*\n", "", source)
    inventories = _arguments(source, "shopInventory")
    if not 0 <= index < len(inventories):
        return compiler.native("j_ShopMenu", "stock-" + str(index))
    stock = [compiler.number("ITEM_" + token) for token in _tokens(inventories[index])]
    return {"op": "shop", "stock": stock}


def church_service(compiler: Any) -> dict[str, Any]:
    compiler.sources.update(
        {
            "disasm/code/common/menus/church/churchactions_1.asm",
            "disasm/code/common/menus/church/churchactions_2.asm",
        }
    )
    operands = {
        "raisePerLevel": "CHURCHMENU_PER_LEVEL_RAISE_COST",
        "promotedExtra": "CHURCHMENU_RAISE_COST_EXTRA_WHEN_PROMOTED",
        "raisedHpCap": "CHAR_STATCAP_HP",
        "poisonCost": "CHURCHMENU_CURE_POISON_COST",
        "stunCost": "CHURCHMENU_CURE_STUN_COST",
    }
    return {"op": "church", **{key: compiler.number(value) for key, value in operands.items()}}
