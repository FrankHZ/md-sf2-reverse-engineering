"""Decode named source regions and resolve retained mutators and school actors."""

import re


def source_regions(root, map_id):
    """Decode only the named map and its three original copy tables."""
    from sf2tool.h2.map_layouts import decode_map_blocks, decode_map_layout

    folder = root / f"disasm/data/maps/entries/map{int(map_id[4:]):02d}"
    words = decode_map_blocks((folder / "0-blocks.bin").read_bytes())[0]
    layout = decode_map_layout((folder / "1-layout.bin").read_bytes(), len(words) // 9)[0]
    tables = {}
    for name, file, macro in (
        ("flags", "3-flag-events.asm", "fbc"),
        ("doors", "4-step-events.asm", "sbc"),
        ("roofs", "5-roof-events.asm", "slbc"),
    ):
        source = (folder / file).read_text(encoding="utf-8")

        def pair(suffix, macro=macro, source=source):
            return [
                tuple(map(int, m))
                for m in re.findall(rf"\b{macro}{suffix}\s+(\d+),\s*(\d+)", source)
            ]

        tables[name] = [
            dict(source=s, rect=(*d, *size))
            for s, d, size in zip(pair("Source"), pair("Dest"), pair("Size"), strict=True)
        ]
        triggers = (
            [int(v) for v in re.findall(r"\bfbcFlag\s+(\d+)", source)]
            if name == "flags"
            else pair("")
        )
        for row, trigger in zip(tables[name], triggers, strict=True):
            row["trigger"] = trigger
    return dict(layout=layout, blocks=[words[i : i + 9] for i in range(0, len(words), 9)], **tables)


def source_population(root, events, check):
    # Every retained mutator must resolve to original source. New mutator kinds
    # require an explicit class; they cannot be silently classified as unchanged.
    from sf2tool.remake_exploration_content import OriginalPrograms

    compiler = OriginalPrograms(
        {"resources": {"standaloneScriptPrograms": [], "initSourcePrograms": []}}, root
    )
    try:
        for m in (3, 19):
            for file in (root / f"disasm/data/maps/entries/map{m:02d}/mapsetups").glob("*.asm"):
                compiler.register_file(file.relative_to(root).as_posix())
        by_id = {symbol.lower().replace("_", "-"): symbol for symbol in compiler.raw}
        for event in events:
            if event.get("Kind") != "program-instruction":
                continue
            if event.get("Detail") in ("LoadSceneMap", "TransferToMap", "ReturnBattleMap"):
                check("new layout mutation class needs coverage", None)
            if event.get("Detail") == "WriteFlag":
                location = event.get("Program") or {}
                symbol = by_id.get(location.get("Program"))
                if symbol is None:
                    check("source flag writer available", None)
                    continue
                program = compiler.compile(symbol)
                instruction = compiler.programs[program]["instructions"][
                    int(location["Instruction"])
                ]
                check(
                    "source flag effect leaves selected layout/setup gates unchanged",
                    instruction["op"] == "set-flag"
                    and instruction["flag"] not in {501, 506, 507, 543, 609, 982},
                )
    except KeyError:
        check("source mutator operand absent", None)
    except (ValueError, IndexError, OSError):
        check("source mutator resolution", False)

    school_entities = {"entity-0"}
    npc = 128
    for operation in compiler.raw.get("ms_map3_Entities", {}).get("operations", []):
        if operation["opcode"] not in ("msFixedEntity", "msWalkingEntity"):
            continue
        sprite = operation["operandText"].split(",")[3].strip()
        if sprite.startswith("ALLY_"):
            school_entities.add("entity-" + str(compiler.number(sprite)))
        else:
            school_entities.add("entity-" + str(npc))
            npc += 1
    return school_entities
