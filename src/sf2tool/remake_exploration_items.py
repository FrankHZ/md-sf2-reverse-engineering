"""Bind selected map item-use tables and their joined-inventory services."""

import re

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
        # Guarded handlers return d6=0 on repeat. The lookup/removal caller has
        # no flag prerequisite; its selected held item is checked by the runtime.
        handler = compiler.source_operations(relative, symbol)
        lookup = [r["opcode"] for r in handler[:2]] == ["moveq", "jsr"] and (
            _tokens(handler[0]["operandText"]) == ["#" + args[3], "d1"]
            and handler[1]["operandText"] == "j_GetItemInventoryLocation"
        )
        guard = [r["opcode"] for r in handler[:2]]
        no_effect = guard == ["chkFlg", "bne.s"] and [
            r["opcode"] for r in compiler.source_operations(relative, handler[1]["operandText"])
        ] == ["rts"]
        if (
            not lookup
            and not no_effect
            and not (
                guard == ["chkFlg", "beq.s"]
                and handler[2]["opcode"] == "moveq"
                and _tokens(handler[2]["operandText"]) == ["#0", "d6"]
                and handler[3]["opcode"] == "bra.s"
                and compiler.source_operations(relative, handler[3]["operandText"])[0]["opcode"]
                == "rts"
            )
        ):
            raise ValueError("unsupported map item event guard: " + symbol)
        events.append(
            {
                "kind": "use-item",
                "item": item,
                "x": x,
                "y": y,
                "facing": None if facing == 255 else facing,
                "requiredFlag": None if lookup else compiler.number(handler[0]["operandText"]),
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
    if (op == "bsr.w" and args == ["ReceiveMandatoryItem"]
            and relative == map_path + "map06/mapsetups/s2_entityevents_701.asm"):
        test, branch = rows[index:index + 2]
        if not (registers.get("d1") == 0 and test["opcode"] == "btst"
                and _tokens(test["operandText"]) == ["#0", "d0"]
                and branch["opcode"] == "bne.s"):
            raise ValueError("unsupported mandatory-item result/option")
        compiler.sources.update(("disasm/code/common/stats/iteminventory.asm",
                                 "disasm/code/common/stats/itemstats.asm"))
        from sf2tool.remake_exploration_joins import music_input_return
        from sf2tool.remake_exploration_scene import sound_operation

        target = compiler.compile(branch["operandText"]).lower().replace("_", "-")
        return [
            {"op": "try-add-party-item", "item": registers["d0"],
             "fullTarget": {"program": target, "instruction": 0}},
            sound_operation("MUSIC_ITEM"),
            {"op": "show-text", "text": 213, "mode": "single", "speaker": None,
             "useItemReceipt": True, "waitForAcknowledgement": False, "explicitWindows": True},
            *music_input_return(), {"op": "wait-text-input"}, {"op": "close-text"},
        ], index + 2
    if (op in ("bsr.s", "bsr.w") and args == ["sub_5A278"]
            and relative in (map_path + "map30/mapsetups/s2_entityevents.asm",
                             map_path + "map30/mapsetups/s3_zoneevents.asm")):
        return {"op": "call", "target": {
            "program": compiler.compile(args[0]).lower().replace("_", "-"),
            "instruction": 0}}, index
    if op == "sndCom" and relative == map_path + "map30/mapsetups/s2_entityevents.asm":
        from sf2tool.remake_exploration_scene import sound_operation

        if args == ["SOUND_COMMAND_PLAY_PREVIOUS_MUSIC"]:
            return {"op": "present", "kind": "PreviousMusic", "resource": None,
                    "entity": None, "position": None}, index
        return sound_operation(args[0]), index
    cliff = relative == map_path + "map72/mapsetups/s3_zoneevents.asm"
    if (cliff and op == "move.w" and len(args) == 2 and args[0].startswith("#ITEM_")
            and args[1] == "((DIALOGUE_NAME_INDEX_1-$1000000)).w"):
        return {"op": "name-dialogue-item", "item": compiler.number(args[0][1:])}, index
    if op != "jsr" or relative not in (
        map_path + "map06/mapsetups/s2_entityevents_701.asm",
        map_path + "map08/mapsetups/s5_itemevents.asm",
        map_path + "map09/mapsetups/s2_entityevents.asm",
        map_path + "map22/mapsetups/s5_itemevents.asm",
        map_path + "map30/mapsetups/s2_entityevents.asm",
        map_path + "map72/mapsetups/s3_zoneevents.asm",
    ):
        return None
    if args == ["(Sleep).w"] and "d0" in registers:
        return {"op": "wait-ticks", "ticks": registers["d0"]}, index
    if args == ["j_GetItemBySlotAndHeldItemsNumber"]:
        compiler.sources.add("disasm/code/common/stats/itemstats.asm")
        compare, branch = rows[index : index + 2]
        if not (registers.get("d1") == 0 and compare["opcode"] == "cmpi.w"
                and _tokens(compare["operandText"]) == ["#COMBATANT_ITEMSLOTS", "d2"]
                and compiler.number("COMBATANT_ITEMSLOTS") == 4 and branch["opcode"] == "bne.s"):
            raise ValueError("unsupported actor pack capacity branch")
        return {"op": "branch-pack-space", "actor": "ally-" + str(registers["d0"]),
                "target": {"program": compiler.compile(branch["operandText"])
                           .lower().replace("_", "-"),
                           "instruction": 0}}, index + 2
    if args == ["j_AddItem"]:
        compiler.sources.add("disasm/code/common/stats/itemstats.asm")
        return {"op": "try-add-actor-item", "actor": "ally-" + str(registers["d0"]),
                "item": registers["d1"]}, index
    if args == ["j_GetItemInventoryLocation"]:
        compiler.sources.add("disasm/code/common/stats/itemstats.asm")
        compare, branch = rows[index : index + 2]
        if not (compare["opcode"] == "cmpi.w" and _tokens(compare["operandText"]) == ["#-1", "d0"]
                and branch["opcode"] in ("beq.s", "beq.w", "bne.s", "bne.w")):
            raise ValueError("unsupported joined-item lookup result")
        missing = branch["opcode"].startswith("beq")
        tail = rows[index + 2:]
        if (
            missing and tail and tail[0]["opcode"] == "jsr"
            and tail[0]["operandText"] == "j_RemoveItemBySlot"
        ):
            # The caller removes the returned slot and rejoins after that single instruction.
            target = branch["operandText"]
            text = (compiler.upstream / relative).read_text(encoding="utf-8")
            if len(tail) != 1 or not re.search(
                r"jsr\s+j_RemoveItemBySlot\s*\n" + re.escape(target) + r":", text
            ):
                raise ValueError("conditional removal must rejoin its fallthrough")
            return {"op": "remove-party-item", "item": registers["d1"]}, index + 3
        target = compiler.compile(branch["operandText"]).lower().replace("_", "-")
        return {
            "op": "branch-missing-item" if missing else "branch-has-item",
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
