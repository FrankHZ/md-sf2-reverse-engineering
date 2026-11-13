"""Lower the existing canonical field descriptions and item tables, without assets."""

from sf2tool.h2.battle_global_data import _arguments, _tokens


def _callback_targets(compiler, path, symbol, records):
    """Bind source symbols in physical table order, including repeated coordinates."""
    callbacks = [row for row in records if row["kind"] != "text"]
    if not callbacks:
        return []
    # A setup can reference a callback defined in the other table file for this map.
    for source in sorted((compiler.upstream / path).parent.glob("s4_descriptions*.asm")):
        compiler.register_file(source.relative_to(compiler.upstream).as_posix())
    wrapper = compiler.source_operations(path, symbol)
    table = next(op["operandText"].split("(pc)")[0] for op in wrapper if op["opcode"] == "lea")
    entries = (
        op
        for op in compiler.source_operations(path, table)
        if op["opcode"] in ("msDescFunction", "msDescFunctionD6")
    )
    targets = []
    for entry, row in zip(entries, callbacks, strict=True):
        args = _tokens(entry["operandText"])
        x, y, target = args[0], args[1], args[-1]
        condition = compiler.number(args[2]) if entry["opcode"] == "msDescFunctionD6" else 0
        callback, base = target.split("-")
        if (
            base != table
            or (compiler.number(x), compiler.number(y)) != (row["x"], row["y"])
            or condition != row["conditionByte"]
        ):
            raise ValueError("field description callback binding mismatch: " + symbol)
        targets.append(callback)
    return targets


def _description_program(compiler, callback):
    # DisplayAreaDescription calls the function and closes its windows on every return.
    symbol = compiler.compile(callback)
    body = compiler.programs[symbol]
    wrapper = "search-" + body["id"]
    compiler.programs[wrapper] = {
        "id": wrapper,
        "source": body["source"],
        "entitiesRunning": False,
        "instructions": [
            {"op": "call", "target": {"program": body["id"], "instruction": 0}},
            {"op": "close-portrait"},
            {"op": "close-text"},
            {"op": "end"},
        ],
    }
    return {"program": wrapper, "instruction": 0}


def compile_search(compiler, resources, references, setup, selected):
    descriptions = []
    if setup is not None:
        symbol = setup["references"]["areaDescriptions"]
        handler = resources["areaDescriptionHandlers"][symbol]
        folder = compiler.upstream / f"disasm/data/maps/entries/map{selected:02d}/mapsetups"
        matches = [
            p
            for p in folder.glob("s4_descriptions*.asm")
            if symbol + ":" in p.read_text(encoding="utf-8")
        ]
        if len(matches) != 1:
            raise ValueError("field description source must resolve once: " + symbol)
        relative = matches[0].relative_to(compiler.upstream).as_posix()
        compiler.sources.add(relative)
        targets = iter(_callback_targets(compiler, relative, symbol, handler["records"]))
        for row in handler["records"]:
            entry = {
                "position": {"x": row["x"], "y": row["y"]},
                "menuOnly": row["conditionByte"] != 0,
            }
            if row["kind"] == "text":
                entry["texts"] = [row["investigationTextIndex"], row["descriptionTextIndex"]]
            else:
                entry["program"] = _description_program(compiler, next(targets))
            descriptions.append(entry)
    gold_path = "disasm/data/stats/items/chestgoldamounts.asm"
    compiler.sources.add(gold_path)
    gold = [
        compiler.number(value)
        for value in _arguments((compiler.upstream / gold_path).read_text(encoding="utf-8"), "dc.w")
    ]
    compiler.sources.update(
        {
            "disasm/code/gameflow/exploration/explorationfunctions_0.asm",
            "disasm/code/gameflow/exploration/explorationfunctions_1.asm",
            "disasm/code/gameflow/exploration/exploration.asm",
            "disasm/code/common/scripting/map/mapsetupsfunctions_1.asm",
            "disasm/sf2mapmacros.asm",
            "disasm/sf2mapsetupmacros.asm",
        }
    )

    def items(key):
        table = resources["itemTables"][references[key]]
        # Some other-item pointers alias the map's chest table (e.g. Map58).
        filename = {"chestItems": "7-chest-items", "otherItems": "8-other-items"}[
            table["sourceKind"]
        ]
        compiler.sources.add(f"disasm/data/maps/entries/map{selected:02d}/{filename}.asm")
        result = []
        for row in table["records"]:
            item = row["item"]
            amount = None if item < 128 else gold[(item - 128) & 127]
            result.append(
                {
                    "position": {"x": row["x"], "y": row["y"]},
                    "flag": row["flag"],
                    "itemWord": item if amount is None else None,
                    "gold": amount,
                }
            )
        return result

    return {
        "chests": items("chestItemTable"),
        "otherItems": items("otherItemTable"),
        "descriptions": descriptions,
        "closedChest": compiler.equates["MAP_BLOCKINDEX_CLOSED_CHEST"],
        "openChest": compiler.equates["MAP_BLOCKINDEX_OPEN_CHEST"],
    }
