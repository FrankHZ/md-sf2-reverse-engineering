"""Source operands for the bounded Church consumer (no caller or story policy)."""

from __future__ import annotations

from typing import Any
import re

from sf2tool.h2.battle_global_data import _arguments, _tokens


def shop_service(compiler: Any, index: int) -> dict[str, Any]:
    """Only the finite early Galam stock is admitted, from the pinned source table."""
    if index not in (compiler.number("SHOP_WEAPON_GALAM_0"), compiler.number("SHOP_ITEM_GALAM_0")):
        return compiler.native("j_ShopMenu", "stock-" + str(index))
    path = "disasm/data/stats/items/shopinventories.asm"
    compiler.sources.add(path)
    source = (compiler.upstream / path).read_text(encoding="utf-8")
    source = re.sub(r"&\s*\n", "", source)
    inventories = _arguments(source, "shopInventory")
    return {"op": "shop", "stock": [compiler.number("ITEM_" + token) for token in _tokens(inventories[index])]}


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
