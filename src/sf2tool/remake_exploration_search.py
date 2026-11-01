"""Lower the existing canonical field descriptions and item tables, without assets."""

from sf2tool.h2.battle_global_data import _arguments


def compile_search(compiler, resources, references, setup, selected):
    descriptions = []
    if setup is not None:
        symbol = setup["references"]["areaDescriptions"]
        handler = resources["areaDescriptionHandlers"][symbol]
        # Withhold Search admission for callback maps; other compiled capabilities remain usable.
        # The opening tables contain only text records. A callback is never replaced by text.
        if any(row["kind"] != "text" for row in handler["records"]):
            return None
        for row in handler["records"]:
            descriptions.append(
                {
                    "position": {"x": row["x"], "y": row["y"]},
                    "menuOnly": row["conditionByte"] != 0,
                    "texts": [row["investigationTextIndex"], row["descriptionTextIndex"]],
                }
            )
        folder = compiler.upstream / f"disasm/data/maps/entries/map{selected:02d}/mapsetups"
        matches = [
            p
            for p in folder.glob("s4_descriptions*.asm")
            if symbol + ":" in p.read_text(encoding="utf-8")
        ]
        if len(matches) != 1:
            raise ValueError("field description source must resolve once: " + symbol)
        compiler.sources.add(matches[0].relative_to(compiler.upstream).as_posix())
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

    def items(key, filename):
        compiler.sources.add(f"disasm/data/maps/entries/map{selected:02d}/{filename}.asm")
        result = []
        for row in resources["itemTables"][references[key]]["records"]:
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
        "chests": items("chestItemTable", "7-chest-items"),
        "otherItems": items("otherItemTable", "8-other-items"),
        "descriptions": descriptions,
        "closedChest": compiler.equates["MAP_BLOCKINDEX_CLOSED_CHEST"],
        "openChest": compiler.equates["MAP_BLOCKINDEX_OPEN_CHEST"],
    }
