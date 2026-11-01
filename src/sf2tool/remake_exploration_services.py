"""Source operands for the bounded Church consumer (no caller or story policy)."""

from __future__ import annotations

from typing import Any


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
