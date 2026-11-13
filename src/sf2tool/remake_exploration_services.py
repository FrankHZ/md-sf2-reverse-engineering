"""Lower source field services for shops, churches and area descriptions."""

from __future__ import annotations

import re
from typing import Any

from sf2tool.h2.battle_global_data import _arguments, _tokens
from sf2tool.remake_exploration_scene import sound_operation


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


def raw_display_text():
    return {
        "op": "show-text",
        "mode": "single",
        "speaker": None,
        "waitForAcknowledgement": False,
        "explicitWindows": True,
    }


def description_service(op, args, registers):
    if op == "clr.w" and args == ["((DIALOGUE_NAME_INDEX_1-$1000000)).w"]:
        registers["nameMember"] = 0
        return []
    if op in ("jsr", "jmp") and args == ["(DisplayText).w"]:
        text = {**raw_display_text(), "text": registers["d0"]}
        if "nameMember" in registers:
            text["nameMember"] = registers["nameMember"]
        return [text, {"op": "end"}] if op == "jmp" else [text]
    if op == "ori.b" and args == ["#0", "d0"]:
        return []  # No value change; the following sound service does not read the condition codes.
    if op == "sndCom":
        return [sound_operation(args[0])]
    if op == "jsr" and args == ["(Sleep).w"]:
        return [{"op": "wait-ticks", "ticks": registers["d0"]}]
    return None
