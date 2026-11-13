"""Bind selected map item-use tables and their joined-inventory services."""

from sf2tool.h2.battle_global_data import _tokens


def item_events(compiler, folder, setup, resources):
    relative = folder + "s5_itemevents.asm"
    compiler.register_file(relative)
    table = setup["references"]["itemEvents"]
    events = []
    records = resources["itemEventHandlers"][table]["records"]
    for row, record in zip(compiler.source_operations(relative, table), records, strict=True):
        args = _tokens(row["operandText"])
        symbol, base = args[-1].split("-")
        if base != table:
            raise ValueError("unexpected map item event table base: " + base)
        if row["opcode"] == "msDefaultItemEvent":
            if record["kind"] != "default" or [
                r["opcode"] for r in compiler.source_operations(relative, symbol)
            ] != ["rts"]:
                raise ValueError("unsupported default map item effect: " + symbol)
            return events
        if row["opcode"] != "msItemEvent":
            raise ValueError("unsupported map item event: " + row["opcode"])
        x, y, facing, item = map(compiler.number, args[:4])
        if record["kind"] != "specific" or (x, y, facing, item) != tuple(
            record[key] for key in ("x", "y", "facing", "item")
        ):
            raise ValueError("map item event binding mismatch: " + symbol)
        # Both admitted handlers first test an already-completed flag, returning
        # d6=0 on repeat. Keep the menu's no-effect path separate from script entry.
        handler = compiler.source_operations(relative, symbol)
        guard = [r["opcode"] for r in handler[:2]]
        no_effect = guard == ["chkFlg", "bne.s"] and [
            r["opcode"] for r in compiler.source_operations(relative, handler[1]["operandText"])
        ] == ["rts"]
        if not no_effect and not (
            guard == ["chkFlg", "beq.s"]
            and handler[2]["opcode"] == "moveq"
            and _tokens(handler[2]["operandText"]) == ["#0", "d6"]
            and handler[3]["opcode"] == "bra.s"
            and compiler.source_operations(relative, handler[3]["operandText"])[0]["opcode"]
            == "rts"
        ):
            raise ValueError("unsupported map item event guard: " + symbol)
        events.append(
            {
                "kind": "use-item",
                "item": item,
                "x": x,
                "y": y,
                "facing": None if facing == 255 else facing,
                "requiredFlag": compiler.number(handler[0]["operandText"]),
                "requiredValue": False,
                "program": {
                    "program": compiler.compile(symbol).lower().replace("_", "-"),
                    "instruction": 0,
                },
            }
        )
    raise ValueError("unterminated map item event table: " + table)


def inventory_operation(compiler, relative, op, args, registers, rows, index):
    # Other chapters call these services with different operands and continuations.
    # They retain their existing boundaries until their callers are admitted.
    map_path = "disasm/data/maps/entries/"
    if op != "jsr" or relative not in (
        map_path + "map08/mapsetups/s5_itemevents.asm",
        map_path + "map09/mapsetups/s2_entityevents.asm",
    ):
        return None
    if args == ["(Sleep).w"] and "d0" in registers:
        return {"op": "wait-ticks", "ticks": registers["d0"]}, index
    if args == ["j_GetItemInventoryLocation"]:
        compiler.sources.add("disasm/code/common/stats/itemstats.asm")
        compare, branch = rows[index : index + 2]
        if not (
            compare["opcode"] == "cmpi.w"
            and _tokens(compare["operandText"]) == ["#-1", "d0"]
            and branch["opcode"] in ("bne.s", "bne.w")
        ):
            raise ValueError("unsupported joined-item lookup result")
        target = compiler.compile(branch["operandText"]).lower().replace("_", "-")
        return {
            "op": "branch-has-item",
            "item": registers["d1"],
            "target": {"program": target, "instruction": 0},
        }, index + 2
    if args == ["RemoveItemFromInventory"]:
        compiler.sources.update(
            ("disasm/code/common/stats/iteminventory.asm", "disasm/code/common/stats/itemstats.asm")
        )
        return {"op": "remove-party-item", "item": registers["d0"]}, index
    if args == ["GetRhodeFacing"]:
        return {"op": "face-interaction", "entity": compiler.entity(str(registers["d0"]))}, index
    return None
