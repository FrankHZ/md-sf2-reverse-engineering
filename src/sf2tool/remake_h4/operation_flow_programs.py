"""Instruction lookup and complete reached source-body lowering."""


def instruction_reader(programs):
    def instruction(e):
        loc = e.get("Program")
        if not loc or loc.get("Program") not in programs:
            return None
        index = loc.get("Instruction")
        body = programs[loc["Program"]]["instructions"]
        return (
            body[int(index)]
            if index is not None and int(index) == index and 0 <= index < len(body)
            else None
        )

    return instruction


def operation_flow_programs(
    executed,
    programs,
    source_root,
    compiler,
    tracked,
    routes,
    result,
    common,
    _encode_source,
    _decode_source_table,
):
    for pid in dict.fromkeys(e["Program"]["Program"] for e, _ in executed):
        p = programs.get(pid)
        value = None
        if p:
            source = p.get("source", "")
            path, symbol = source.rsplit(":", 1) if ":" in source else ("", source)
            if pid.endswith("-flag-layout"):
                map_id = int(pid.split("-")[1])
                try:
                    data, count, tail = _encode_source(
                        source_root / f"disasm/data/maps/entries/map{map_id:02d}/3-flag-events.asm",
                        "flagEvents",
                        compiler.equates,
                    )
                    flag_rows = _decode_source_table("flagEvents", data, count, tail)
                    expected = []
                    for index, row in enumerate(flag_rows):
                        expected.extend(
                            [
                                dict(
                                    op="branch-flag",
                                    flag=row["flag"],
                                    whenSet=False,
                                    target=dict(program=pid, instruction=index * 2 + 2),
                                ),
                                dict(
                                    op="native-call",
                                    symbol="flag-layout-copy",
                                    source=f"{source}[{index}]",
                                ),
                            ]
                        )
                    expected.append(
                        dict(op="jump", target=dict(program=f"map-{map_id}-setup", instruction=0))
                    )
                    value = p["instructions"] == expected
                except (KeyError, OSError, ValueError):
                    value = None
            elif pid.startswith("map-") and pid.endswith("-setup"):
                map_name = pid.removesuffix("-setup")
                value = (
                    map_name not in routes
                    and p["instructions"] == [dict(op="end")]
                    and source == "None:ordered setup/init/population"
                )
            elif path not in tracked:
                value = False
            elif pid in ("source-battle-load", "source-outcome-return"):
                # Accepted native compositions have their own implementation-neutral contracts.
                middle = (
                    dict(op="present", kind="BattleLoad", resource=None, entity=None, position=None)
                    if pid == "source-battle-load"
                    else dict(op="battle-return-map")
                )
                expected = [
                    dict(
                        op="present", kind="FadeOut", resource="black", entity=None, position=None
                    ),
                    middle,
                    dict(op="present", kind="FadeIn", resource="black", entity=None, position=None),
                    dict(op="end"),
                ]
                value = p["instructions"] == expected and symbol == (
                    "LoadBattle" if pid == "source-battle-load" else "ExplorationLoop"
                )
            else:
                try:
                    compiler.compile(symbol)
                    value = p["instructions"] == compiler.programs[symbol][
                        "instructions"
                    ] and p.get("entitiesRunning") == compiler.programs[symbol].get(
                        "entitiesRunning"
                    )
                except (KeyError, OSError, ValueError):
                    value = None
        result["programs"].append(
            dict(program=pid, source=p.get("source") if p else None, value=value)
        )
        common("complete reached body " + pid, value)
    common("lowering dependencies pinned", compiler.sources <= tracked)
