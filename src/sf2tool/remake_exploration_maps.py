"""Prepare selected map setups while sharing layout, palette and warp definitions."""

import re

from sf2tool.h2.battle_global_data import _tokens
from sf2tool.remake_exploration_actions import action_stream, walking_actions
from sf2tool.remake_exploration_items import item_events
from sf2tool.remake_exploration_search import compile_search


def _location(program, instruction=0):
    return {"program": program.lower().replace("_", "-"), "instruction": instruction}


def compile_maps(
    compiler, canonical, selection, battle_routes, selected_setups=None, only_maps=None
):
    selected_setups = selected_setups or {}

    def rows(folder, prefix, symbol):
        matches = [
            path
            for path in (compiler.upstream / folder).glob(prefix + "*.asm")
            if re.search(
                r"^" + re.escape(symbol) + r":", path.read_text(encoding="utf-8"), re.MULTILINE
            )
        ]
        if len(matches) != 1:
            raise ValueError("selected setup source must resolve once: " + symbol)
        return compiler.source_operations(
            matches[0].relative_to(compiler.upstream).as_posix(), symbol
        )

    def sequenced(folder, symbol):
        # sub_44D0E copies eas_Walking's 32-byte prefix, then emits move/waitDest/
        # wait10 for each nonnegative orientation and jumps to the first move.
        compiler.sources.add("disasm/code/common/scripting/entity/entityfunctions_2.asm")
        walking = walking_actions(compiler, 0, 0, 0)
        actions = walking[: next(i for i, row in enumerate(walking) if row["op"] == "random-walk")]
        loop = len(actions)
        for row in rows(folder, "s1_entities", symbol):
            if row["opcode"] == "emsEnd":
                actions.append({"op": "jump", "instruction": loop})
                return actions
            if row["opcode"] != "dc.b":
                raise ValueError("unsupported movement sequence operation")
            direction = compiler.number(row["operandText"])
            if direction not in range(4):
                raise ValueError("unsupported movement sequence direction")
            x, y = [(1, 0), (0, -1), (-1, 0), (0, 1)][direction]
            actions.extend([{"op": "move", "x": x, "y": y}, {"op": "wait-timer", "operand": 10}])
        raise ValueError("unterminated movement sequence")

    resources = {
        key: {row["id"]: row for row in rows} for key, rows in canonical["resources"].items()
    }
    map_rows = {row["id"]: row for row in canonical["maps"]}
    switches = {}
    for switch in compiler.source_operations(
        "disasm/data/maps/global/flagswitchedmaps.asm", "table_FlagSwitchedMaps"
    ):
        if switch["opcode"] == "flagSwitchedMap":
            original, flag, replacement = map(compiler.number, _tokens(switch["operandText"]))
            switches[original] = (flag, replacement)
    overworld = {
        compiler.number(row["operandText"])
        for row in compiler.source_operations(
            "disasm/data/maps/global/overworldmaps.asm", "table_OverworldMaps"
        )
        if row["opcode"] == "dc.b"
    }
    caravan_rows = []
    for row in compiler.source_operations(
        "disasm/data/scripting/entity/followers.asm", "table_OverworldFollowers"
    ):
        if row["opcode"] == "follower":
            flag, character, sprite, _ = map(compiler.number, _tokens(row["operandText"]))
            caravan_rows.append({"flag": flag, "character": character, "sprite": sprite})
    maps = []
    for selected in only_maps or selection["maps"]:
        row = map_rows[selected]
        references = row["references"]
        words = resources["layouts"][references["layout"]]["words"]
        areas = resources["areaTables"][references["areaTable"]]["records"]
        route_id = references["setupRoute"]
        route = resources["setupRoutes"].get(route_id)
        setup = None
        entities = []
        if route is not None:
            setup = resources["setupDefinitions"][
                selected_setups.get(selected, route["defaultSetup"])
            ]
            records = resources["entityLists"][setup["references"]["entities"]]["records"]
            npc = 128
            for entity in records:
                sprite = entity["mapSprite"]
                if (
                    sprite >= compiler.equates["MAPSPRITES_SPECIALS_START"]
                    and sprite != compiler.equates["MAPSPRITE_TAROS"]
                ):
                    raise ValueError("special entity index mapping is not admitted")
                identity = sprite if sprite < 30 else npc
                if sprite >= 30:
                    npc += 1
                entities.append(
                    {
                        "id": "entity-" + str(identity),
                        "position": {"x": entity["x"], "y": entity["y"]},
                        "facing": entity["facing"],
                        "speed": selection["controlledEntitySpeed"],
                        "visible": True,
                        "obstruction": False,
                    }
                )
        # Every source warp row is retained. Unadmitted destination/setup/scroll branches stop
        # before transfer; selected simple rows use the ordinary common transfer rule.
        events = []
        for index, warp in enumerate(
            resources["warpEventTables"][references["warpEventTable"]]["records"]
        ):
            destination = selected if warp["targetMap"] == 255 else warp["targetMap"]
            common = {
                "x": None if warp["trigger"]["x"] == 255 else warp["trigger"]["x"],
                "y": None if warp["trigger"]["y"] == 255 else warp["trigger"]["y"],
                "marker": 4096,
                "requiredFlag": None,
                "requiredValue": True,
            }
            scroll = warp["scrollMode"]
            retained_axis = "y" if scroll in (16, 18) else "x"
            edge_axis = "x" if retained_axis == "y" else "y"
            if (
                destination in selection["maps"]
                and scroll in (16, 17, 18, 19)
                and warp["destination"][retained_axis] == 255
                and 0 <= warp["destination"][edge_axis] < 64
            ):
                events.append(
                    {
                        "kind": "warp",
                        **common,
                        "map": f"map-{destination}",
                        "position": {**warp["destination"], retained_axis: None},
                        "facing": warp["facing"],
                        "loadMode": "scroll",
                    }
                )
            elif (
                destination in selection["maps"]
                and not warp["retainsCoordinates"]
                and warp["scrollMode"] == 0
                and all(0 <= warp["destination"][axis] < 64 for axis in ("x", "y"))
                and all(
                    warp["trigger"][axis] < 64 or warp["trigger"][axis] == 255
                    for axis in ("x", "y")
                )
            ):
                events.append(
                    {
                        "kind": "warp",
                        **common,
                        "map": "map-" + str(destination),
                        "position": warp["destination"],
                        "facing": warp["facing"],
                        "loadMode": "preserve" if warp["targetMap"] == 255 else "rebuild",
                    }
                )
            else:
                symbol = compiler.frontier(
                    f"map-{selected}-warp-{index}", f"{references['warpEventTable']}[{index}]"
                )
                events.append({"kind": "warp-frontier", **common, "program": _location(symbol)})
        # SwitchMap checks the source flag table before an explicit map transfer.
        # Express it with the existing conditional event dispatch; current-map
        # warps preserve their map and do not invoke SwitchMap.
        switched_events = []
        for event in events:
            target = int(event["map"][4:]) if event["kind"] == "warp" else None
            if event.get("loadMode") == "rebuild" and target in switches:
                flag, replacement = switches[target]
                if replacement in selection["maps"]:
                    switched_events.append(
                        {**event, "map": f"map-{replacement}", "requiredFlag": flag}
                    )
                else:
                    symbol = compiler.frontier(
                        f"map-{selected}-switched-{target}",
                        f"SwitchMap:{target}:{flag}:{replacement}",
                    )
                    switched_events.append(
                        {
                            **{key: event[key] for key in ("x", "y", "marker", "requiredValue")},
                            "kind": "warp-frontier",
                            "requiredFlag": flag,
                            "program": _location(symbol),
                        }
                    )
                event = {**event, "requiredFlag": flag, "requiredValue": False}
            switched_events.append(event)
        events = switched_events
        population = (
            compiler.population() if route is None and "outcomeEgressMap" in selection else None
        )
        layout_events = None
        full_setup = selected in selection.get("eventMaps", [])
        if setup is not None and (full_setup or selected in selection.get("populationMaps", [])):
            folder = f"disasm/data/maps/entries/map{selected:02d}/mapsetups/"
            source_rows = rows(folder, "s1_entities", setup["references"]["entities"])
            # An unterminated label can fall through into a shared entity list.
            # Canonical record addresses identify the following source owner.
            while (
                source_rows
                and source_rows[-1]["opcode"] != "msEntitiesEnd"
                and len(source_rows) < len(records)
            ):
                address = records[len(source_rows)]["address"]
                continuations = [
                    entry
                    for entry in resources["entityLists"].values()
                    if entry["address"] == address
                ]
                if len(continuations) == 1:
                    tail_symbol = continuations[0]["id"]
                else:
                    tail_symbol = f"byte_{address:X}"
                tail = rows(folder, "s1_entities", tail_symbol)
                if not tail:
                    raise ValueError("empty entity-list continuation")
                source_rows.extend(tail)
            source_rows = [source for source in source_rows if source["opcode"] != "msEntitiesEnd"]
            if len(source_rows) != len(records):
                raise ValueError("entity source/canonical cardinality mismatch")
            for entity, source_row, source_record in zip(
                entities, source_rows, records, strict=True
            ):
                args = _tokens(source_row["operandText"])
                entity["sprite"] = source_record["mapSprite"]
                entity["actions"] = (
                    walking_actions(
                        compiler,
                        **{
                            "x": source_record["walking"]["originX"],
                            "y": source_record["walking"]["originY"],
                            "radius": source_record["walking"]["range"],
                        },
                    )
                    if source_record["kind"] == "walking"
                    else sequenced(folder, args[4])
                    if source_record["kind"] == "sequenced"
                    else action_stream(compiler, args[4])
                )
            population = compiler.population()
        if population is not None and selected in overworld:
            population["followers"] = caravan_rows + population["followers"]
        if full_setup and setup is not None:
            if selected in selection.get("itemEventMaps", []):
                events.extend(item_events(compiler, folder, setup, resources))
            for key, filename, macro, default in (
                ("entityEvents", "s2_entityevents.asm", "msEntityEvent", "msDefaultEntityEvent"),
                ("zoneEvents", "s3_zoneevents.asm", "msZoneEvent", "msDefaultZoneEvent"),
            ):
                table = rows(folder, filename.removesuffix(".asm"), setup["references"][key])
                # Retain the reviewed raw default displacement as an explicit frontier.
                # Its target lies in entity data; the neighboring rts is not that target.
                if key == "zoneEvents" and [(r["opcode"], r["operandText"]) for r in table] == [
                    ("dc.w", "$FD00"),
                    ("dc.w", "byte_54868+4-ms_map44_ZoneEvents"),
                ]:
                    target = compiler.frontier(
                        "raw-map44-zone-default-expression-boundary", folder + filename
                    )
                    events.append(
                        {
                            "kind": "source-zone",
                            "program": _location(target),
                            "requiredFlag": None,
                            "requiredValue": True,
                            "marker": 0x1400,
                            "x": None,
                            "y": None,
                            "actions": action_stream(compiler, "eas_Init"),
                        }
                    )
                    continue
                if [row["opcode"] for row in table] == ["rts"]:
                    # Some maps point directly to the empty handler.
                    continue
                for source_row in table:
                    args = _tokens(source_row["operandText"])
                    explicit_default = (
                        key == "entityEvents" and source_row["opcode"] == "msDftEntityEvent"
                    )
                    if source_row["opcode"] not in (macro, default) and not explicit_default:
                        raise ValueError(
                            "unsupported event table row: "
                            f"{folder}{filename}[{source_row['index']}]"
                        )
                    symbol = re.search(r"[A-Za-z_]\w*", args[-1]).group(0)
                    common = {
                        "program": _location(compiler.compile(symbol)),
                        "requiredFlag": None,
                        "requiredValue": True,
                    }
                    if key == "entityEvents":
                        events.append(
                            {
                                "kind": "interact",
                                **common,
                                "entity": compiler.entity(args[0])
                                if source_row["opcode"] == macro
                                else None,
                                "entityFlags": compiler.number(args[1])
                                if source_row["opcode"] == macro
                                else compiler.number(args[0])
                                if explicit_default
                                else 0,
                            }
                        )
                    else:
                        events.append(
                            {
                                "kind": "source-zone",
                                "actions": action_stream(compiler, "eas_Init"),
                                **common,
                                "marker": 0x1400,
                                "x": None
                                if source_row["opcode"] == default
                                or compiler.number(args[0]) == 255
                                else compiler.number(args[0]),
                                "y": None
                                if source_row["opcode"] == default
                                or compiler.number(args[1]) == 255
                                else compiler.number(args[1]),
                            }
                        )
                    if source_row["opcode"] == default:
                        # The default terminates the table; following instructions are not rows.
                        break

        if selected in selection.get("eventMaps", []):

            def block_copy(record):
                return {
                    "source": record["source"] if record["source"]["y"] < 128 else {"x": 0, "y": 0},
                    "destination": record["destination"],
                    **record["size"],
                }

            layout_events = {
                "doors": [
                    {"trigger": record["trigger"], "copy": block_copy(record)}
                    for record in resources["stepEventTables"][references["stepEventTable"]][
                        "records"
                    ]
                ],
                "flags": [
                    {"flag": record["flag"], "copy": block_copy(record)}
                    for record in resources["flagEventTables"][references["flagEventTable"]][
                        "records"
                    ]
                ],
                "roofs": [
                    {
                        "trigger": record["trigger"],
                        "copy": block_copy(record),
                        "clear": record["source"]["y"] >= 128,
                    }
                    for record in resources["roofEventTables"][references["roofEventTable"]][
                        "records"
                    ]
                ],
            }
        on_load = compiler.frontier(
            f"map-{selected}-setup", f"{route_id}:ordered setup/init/population"
        )
        flag_table = references["flagEventTable"]
        flag_records = resources["flagEventTables"][flag_table]["records"]
        if flag_records and selected not in selection.get("eventMaps", []):
            guarded_load = compiler.frontier(f"map-{selected}-flag-layout", flag_table)
            instructions = []
            for index, record in enumerate(flag_records):
                instructions.extend(
                    [
                        {
                            "op": "branch-flag",
                            "flag": record["flag"],
                            "whenSet": False,
                            "target": _location(guarded_load, len(instructions) + 2),
                        },
                        compiler.native("flag-layout-copy", f"{flag_table}[{index}]"),
                    ]
                )
            instructions.append({"op": "jump", "target": _location(on_load)})
            compiler.programs[guarded_load]["instructions"] = instructions
            on_load = guarded_load
        battles = battle_routes.get(selected, [])
        input_program = None
        if entities or resources["stepEventTables"][references["stepEventTable"]]["records"]:
            input_program = _location(
                compiler.frontier(
                    f"map-{selected}-input", f"{route_id}:input/event/autonomous-entity services"
                )
            )
        if selected in selection.get("eventMaps", []) and setup is not None:
            input_program = None
            on_load = compiler.compile(setup["references"]["initFunction"])
        search = (
            compile_search(compiler, resources, references, setup, selected)
            if selected in selection.get("searchMaps", [])
            else None
        )
        maps.append(
            {
                "id": "map-" + str(selected),
                "layout": [words[n : n + 64] for n in range(0, 4096, 64)],
                "areas": [
                    {
                        "minX": a["mainLayerStart"]["x"],
                        "minY": a["mainLayerStart"]["y"],
                        "maxX": a["mainLayerEnd"]["x"],
                        "maxY": a["mainLayerEnd"]["y"],
                        "view": {
                            "foregroundX": a["secondLayerForegroundStart"]["x"],
                            "foregroundY": a["secondLayerForegroundStart"]["y"],
                            "backgroundX": a["secondLayerBackgroundStart"]["x"],
                            "backgroundY": a["secondLayerBackgroundStart"]["y"],
                            **{
                                f"parallax{plane}{axis.upper()}": a[key][axis]
                                for plane, key in (
                                    ("A", "mainLayerParallax"),
                                    ("B", "secondLayerParallax"),
                                )
                                for axis in ("x", "y")
                            },
                            **{
                                f"autoscroll{plane}{axis.upper()}": a[key][axis]
                                for plane, key in (
                                    ("A", "mainLayerAutoscroll"),
                                    ("B", "secondLayerAutoscroll"),
                                )
                                for axis in ("x", "y")
                            },
                            "layer": a["mainLayerType"],
                        },
                        "overlay": {
                            axis: a["secondLayerForegroundStart"][axis]
                            - a["secondLayerBackgroundStart"][axis]
                            for axis in ("x", "y")
                        },
                    }
                    for a in areas
                ],
                "entities": entities,
                **(
                    {
                        "population": population,
                        **({"layoutEvents": layout_events} if layout_events is not None else {}),
                        "entryFlags": [
                            {"flag": flag, "value": False}
                            for flag in range(
                                compiler.equates["MAPSETUP_TEMP_FLAGS_START"],
                                compiler.equates["MAPSETUP_TEMP_FLAGS_START"]
                                + compiler.equates["MAPSETUP_TEMP_FLAGS_COUNTER"]
                                + 1,
                            )
                        ]
                        + [{"flag": 80, "value": True}],
                    }
                    if population
                    else {}
                ),
                "events": events,
                **({"search": search} if search is not None else {}),
                "onLoad": _location(on_load),
                "battle": battles[0] if len(battles) == 1 else None,
                **({"battles": battles} if len(battles) > 1 else {}),
                "input": input_program,
                "setup": None
                if route is None
                else {
                    "default": route["defaultSetup"].lower().replace("_", "-"),
                    "variants": [
                        {
                            "flag": variant["flag"],
                            "setup": variant["setup"].lower().replace("_", "-"),
                        }
                        for variant in route["flagVariants"]
                    ],
                },
            }
        )
    # Map setup can be selected only at its implemented boundary. Source Map40's guarded
    # init function is compiled below; other setups remain explicit reached frontiers.
    for selected in only_maps or selection["maps"]:
        row = map_rows[selected]
        route = resources["setupRoutes"].get(row["references"]["setupRoute"])
        if route is None:
            compiler.programs[f"map-{selected}-setup"]["instructions"] = [{"op": "end"}]
            continue
        setup = resources["setupDefinitions"][selected_setups.get(selected, route["defaultSetup"])]
        init = resources["initFunctions"][setup["references"]["initFunction"]]
        operations = init["operations"]
        # Only the existing source guard/script/return shape is supported, not an ID special case.
        if (
            len(operations) == 4
            and [op["opcode"] for op in operations] == ["chkFlg", "beq.s", "script", "rts"]
            and operations[1]["localBranchTargetIndex"] == 3
        ):
            program_id = f"map-{selected}-setup"
            body = [
                {
                    "op": "branch-flag",
                    "flag": compiler.number(operations[0]["operandText"]),
                    "whenSet": False,
                    "target": _location(program_id, 2),
                },
                {"op": "call", "target": _location(compiler.compile(operations[2]["operandText"]))},
                {"op": "end"},
            ]
            compiler.programs[program_id]["instructions"] = body
    return maps
