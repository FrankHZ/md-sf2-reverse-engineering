"""Prepare selected private original exploration content; never read comparison fixtures.

The registered canonical export supplies actual layouts, entity records and ordered program
operations. This offline boundary verifies that export and the pinned source checkout. Runtime
receives normalized immutable definitions; controlled starts are a separate reference input.
Unimplemented native operations remain executable stop instructions at their source location.
"""

from __future__ import annotations

import argparse
import hashlib
import json
import re
import subprocess
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_ai import _equates
from sf2tool.h2.battle_global_data import _arguments, _integer, _statements, _tokens
from sf2tool.h2.map_import import MANIFEST, _canonical_bytes
from sf2tool.jsonio import load_json


def _location(program: str, instruction: int = 0) -> dict[str, Any]:
    return {"program": program.lower().replace("_", "-"), "instruction": instruction}


class OriginalPrograms:
    """Lower source operations, preserving unsupported operations and all branch targets."""

    def __init__(self, canonical: dict[str, Any], upstream: Path):
        self.upstream = upstream
        self.equates = _equates(upstream / "disasm")
        self.sources: set[str] = {"disasm/sf2enums.asm"}
        resources = canonical["resources"]
        self.raw = {
            row["id"]: row
            for key in ("standaloneScriptPrograms", "initSourcePrograms")
            for row in resources[key]
        }
        self.programs: dict[str, dict[str, Any]] = {}
        self.actions: dict[str, list[dict[str, Any]]] = {}

    def number(self, value: str) -> int:
        return self.equates[value] if value in self.equates else _integer(value)

    def entity(self, value: str) -> str:
        return "entity-" + str(self.number(value))

    def source_operations(self, relative: str, symbol: str) -> list[dict[str, Any]]:
        self.sources.add(relative)
        text = (self.upstream / relative).read_text(encoding="utf-8")
        match = re.search(
            rf"^{re.escape(symbol)}:[ \t]*(.*?)(?=^[A-Za-z_]\w*:|\Z)",
            text,
            re.MULTILINE | re.DOTALL,
        )
        if match is None:
            raise ValueError("selected source label is missing: " + symbol)
        result = []
        for index, statement in enumerate(_statements(match.group(1))):
            opcode, _, operands = statement.partition(" ")
            result.append({"index": index, "opcode": opcode, "operandText": operands.strip()})
        return result

    def native(self, opcode: str, source: str) -> dict[str, Any]:
        return {"op": "native-call", "symbol": opcode, "source": source}

    def action_stream(self, symbol: str) -> list[dict[str, Any]]:
        if symbol in self.actions:
            return self.actions[symbol]
        path = "disasm/data/scripting/entity/eas_actions.asm"
        rows = self.source_operations(path, symbol)
        result: list[dict[str, Any]] = []
        self.actions[symbol] = result
        for row in rows:
            op, args = row["opcode"], _tokens(row["operandText"])
            source = f"{path}:{symbol}[{row['index']}]"
            if op == "ac_jump" and args == ["eas_Idle"]:
                # Stop action ownership at the source idle service boundary. Its autonomous
                # sprite/animation work is outside this controlled action projection.
                break
            result.extend(self.action(op, args, source))
            if op in ("ac_end", "ac_jump"):
                break
        return result

    def action(self, op: str, args: list[str], source: str) -> list[dict[str, Any]]:
        directions = {
            "Right": (1, 0),
            "Up": (0, -1),
            "Left": (-1, 0),
            "Down": (0, 1),
            "UpRight": (1, -1),
            "UpLeft": (-1, -1),
            "DownLeft": (-1, 1),
            "DownRight": (1, 1),
        }
        if op.startswith("move") and op[4:] in directions:
            x, y = directions[op[4:]]
            return [{"op": "move", "x": x * self.number(args[0]), "y": y * self.number(args[0])}]
        if op.startswith("face") and op[4:] in directions:
            facing = [
                "Right",
                "Up",
                "Left",
                "Down",
                "UpRight",
                "UpLeft",
                "DownLeft",
                "DownRight",
            ].index(op[4:])
            return [{"op": "face", "facing": facing}, {"op": "wait", "ticks": self.number(args[0])}]
        if op == "eaWait" or op == "ac_wait":
            return [{"op": "wait", "ticks": self.number(args[0])}]
        if op in ("ac_setSpeed", "ac_accelFactors"):
            return [
                {
                    "op": "speed" if op == "ac_setSpeed" else "acceleration",
                    "x": self.number(args[0]),
                    "y": self.number(args[1]),
                }
            ]
        flag_ops = {
            "ac_entityObstructable": ("a", 128),
            "ac_mapUncollidable": ("a", 64),
            "ac_entityUncollidable": ("a", 32),
            "ac_autoFacing": ("b", 64),
            "ac_immersed": ("b", 32),
        }
        if op in flag_ops:
            field, mask = flag_ops[op]
            return [
                {
                    "op": "flags",
                    "field": field,
                    "mask": mask,
                    "value": mask if args[0] == "ON" else 0,
                }
            ]
        if op in ("ac_acceleration", "ac_deceleration"):
            shift = 0 if op == "ac_acceleration" else 2
            return [
                {
                    "op": "flags",
                    "field": "a",
                    "mask": 3 << shift,
                    "value": ((int(args[0] == "ON") | (int(args[1] == "ON") << 1)) << shift),
                }
            ]
        if op in ("ac_orientUp", "ac_orientLeft", "ac_orientDown", "ac_orientRight"):
            return [
                {
                    "op": "flags",
                    "field": "b",
                    "mask": 3,
                    "value": [
                        "ac_orientUp",
                        "ac_orientLeft",
                        "ac_orientDown",
                        "ac_orientRight",
                    ].index(op),
                }
            ]
        if op == "ac_setSize":
            return [{"op": "sprite-size", "size": self.number(args[0])}]
        if op == "ac_updateSprite":
            return [{"op": "refresh-sprite", "source": source}]
        if op == "ac_setFacing":
            return [{"op": "face", "facing": self.number(args[0])}]
        return [self.native(op, source)]

    def compile(self, symbol: str) -> str:
        if symbol in self.programs:
            return symbol
        if symbol not in self.raw:
            raise ValueError("source program target not imported: " + symbol)
        raw = self.raw[symbol]
        relative = "disasm/" + raw["path"]
        self.sources.add(relative)
        result: list[dict[str, Any]] = []
        self.programs[symbol] = {
            "id": symbol.lower().replace("_", "-"),
            "source": f"{relative}:{symbol}",
            "instructions": result,
        }
        rows, index = raw["operations"], 0
        while index < len(rows):
            row = rows[index]
            op, args = row["opcode"], _tokens(row["operandText"])
            source = f"{relative}:{symbol}[{row['index']}]"
            index += 1
            if op in ("csc_end", "rts") or (op == "dc.w" and args == ["$FFFF"]):
                result.append({"op": "end"})
            elif op in ("jump", "jumpIfFlagSet", "jumpIfFlagClear"):
                target = self.compile(args[-1])
                result.append(
                    {"op": "jump", "target": _location(target)}
                    if op == "jump"
                    else {
                        "op": "branch-flag",
                        "flag": self.number(args[0]),
                        "whenSet": op == "jumpIfFlagSet",
                        "target": _location(target),
                    }
                )
            elif op in ("setF", "clearF", "setFlg", "clrFlg", "setStoryFlag", "clearStoryFlag"):
                result.append(
                    {
                        "op": "set-flag",
                        "flag": self.number(args[0]) + (400 if "Story" in op else 0),
                        "value": op in ("setF", "setFlg", "setStoryFlag"),
                    }
                )
            elif op == "textCursor":
                result.append({"op": "text-cursor", "text": self.number(args[0])})
            elif op in ("nextText", "nextSingleText"):
                result.append(
                    {
                        "op": "show-text",
                        "mode": "single" if op == "nextSingleText" else "continued",
                        "speaker": self.entity(args[1]),
                        "speakerFlags": self.number(args[0]),
                    }
                )
            elif op == "closeTxt":
                result.append({"op": "close-text"})
            elif op == "yesNo":
                result.append({"op": "yes-no", "flag": 89})
            elif op == "csWait":
                result.append({"op": "wait-ticks", "ticks": self.number(args[0])})
            elif op == "setFacing":
                result.append(
                    {"op": "face", "entity": self.entity(args[0]), "facing": self.number(args[1])}
                )
            elif op == "setPos":
                result.append(
                    {
                        "op": "position",
                        "entity": self.entity(args[0]),
                        "position": {"x": self.number(args[1]), "y": self.number(args[2])},
                        "facing": self.number(args[3]),
                    }
                )
            elif op in (
                "entityActions",
                "entityActionsWait",
                "customActscript",
                "customActscriptWait",
            ):
                actions = []
                while index < len(rows) and rows[index]["opcode"] not in ("endActions", "ac_end"):
                    action = rows[index]
                    if action["opcode"] == "ac_jump" and _tokens(action["operandText"]) == [
                        "eas_Idle"
                    ]:
                        index += 1
                        continue
                    actions.extend(
                        self.action(
                            action["opcode"],
                            _tokens(action["operandText"]),
                            f"{relative}:{symbol}[{action['index']}]",
                        )
                    )
                    index += 1
                if index == len(rows):
                    raise ValueError("unterminated entity action: " + symbol)
                index += 1
                result.append(
                    {
                        "op": "motion",
                        "entity": self.entity(args[0]),
                        "wait": op.endswith("Wait"),
                        "actions": actions,
                    }
                )
            elif op in ("setActscript", "setActscriptWait"):
                actions = self.action_stream(args[1])
                result.append(
                    {
                        "op": "motion",
                        "entity": self.entity(args[0]),
                        "wait": op.endswith("Wait"),
                        "actions": actions,
                    }
                )
            else:
                # Presentation/native services are not acknowledged by this compiler. The
                # native owner must implement the service before this instruction can finish.
                result.append(self.native(op, source))
        if not result or result[-1]["op"] not in ("end", "return", "jump", "native-call"):
            source_text = (self.upstream / relative).read_text(encoding="utf-8")
            labels = list(re.finditer(r"^([A-Za-z_]\w*):", source_text, re.MULTILINE))
            label_index = next(i for i, match in enumerate(labels) if match.group(1) == symbol)
            successor = labels[label_index + 1].group(1) if label_index + 1 < len(labels) else None
            if successor is None or successor not in self.raw:
                raise ValueError("source program fallthrough requires an explicit owner: " + symbol)
            result.append({"op": "jump", "target": _location(self.compile(successor))})
        return symbol

    def frontier(self, symbol: str, source: str) -> str:
        self.programs.setdefault(
            symbol,
            {
                "id": symbol.lower().replace("_", "-"),
                "source": source,
                "instructions": [self.native(symbol, source)],
            },
        )
        return symbol


def prepare(
    canonical_path: Path, upstream: Path, selection_path: Path, output: Path
) -> dict[str, int]:
    output = output.resolve()
    repository = Path(__file__).resolve().parents[2]
    if (
        not output.is_relative_to(repository / "local")
        or output in (canonical_path.resolve(), selection_path.resolve())
        or output.is_relative_to(upstream.resolve())
    ):
        raise ValueError("output must be ignored scratch distinct from read-only inputs")
    canonical = load_json(canonical_path)
    manifest = load_json(MANIFEST)
    if hashlib.sha256(_canonical_bytes(canonical)).hexdigest().upper() != manifest["outputSha256"]:
        raise ValueError("registered canonical input identity mismatch")
    expected_commit = canonical["upstream"]["commit"]
    completed = subprocess.run(
        ["git", "rev-parse", "HEAD"], cwd=upstream, capture_output=True, text=True, check=True
    )
    if completed.stdout.strip() != expected_commit:
        raise ValueError("pinned source commit mismatch")
    selection = load_json(selection_path)
    compiler = OriginalPrograms(canonical, upstream)
    for source in selection["additionalProgramSources"]:
        compiler.raw[source["symbol"]] = {
            "id": source["symbol"],
            "path": source["path"],
            "operations": compiler.source_operations("disasm/" + source["path"], source["symbol"]),
        }
    for symbol in selection["programs"]:
        compiler.compile(symbol)
    battle_routes = {}
    coordinates_path = "disasm/data/battles/global/battlemapcoords.asm"
    compiler.sources.add(coordinates_path)
    coordinates = _arguments(
        (upstream / coordinates_path).read_text(encoding="utf-8"), "battleMapCoordinates"
    )
    for battle_id in selection["battles"]:
        map_id, _, _, _, _, trigger_x, trigger_y = map(
            compiler.number, _tokens(coordinates[battle_id])
        )
        if (trigger_x, trigger_y) != (255, 255):
            raise ValueError("coordinate-triggered battle selection is not admitted")
        hooks = []
        for hook in ("beforebattle", "battlestart"):
            path = "disasm/data/battles/cutscenes/" + hook + "cutscenes.asm"
            compiler.sources.add(path)
            expressions = _arguments((upstream / path).read_text(encoding="utf-8"), "dc.w")
            symbol = re.search(r"[A-Za-z_]\w*", expressions[battle_id]).group(0)
            hooks.append(_location(compiler.compile(symbol)))
        battle_routes[map_id] = {
            "encounter": "battle-" + str(battle_id),
            "unlockedFlag": 400 + battle_id,
            "completedFlag": 500 + battle_id,
            "introFlag": 450 + battle_id,
            "before": hooks[0],
            "start": hooks[1],
        }
    resources = {
        key: {row["id"]: row for row in rows} for key, rows in canonical["resources"].items()
    }
    map_rows = {row["id"]: row for row in canonical["maps"]}
    maps = []
    for selected in selection["maps"]:
        row = map_rows[selected]
        references = row["references"]
        words = resources["layouts"][references["layout"]]["words"]
        areas = resources["areaTables"][references["areaTable"]]["records"]
        route_id = references["setupRoute"]
        route = resources["setupRoutes"].get(route_id)
        entities = []
        if route is not None:
            setup = resources["setupDefinitions"][route["defaultSetup"]]
            records = resources["entityLists"][setup["references"]["entities"]]["records"]
            npc = 128
            for entity in records:
                sprite = entity["mapSprite"]
                if sprite >= compiler.equates["MAPSPRITES_SPECIALS_START"]:
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
            destination = warp["targetMap"]
            common = {
                "x": None if warp["trigger"]["x"] == 255 else warp["trigger"]["x"],
                "y": None if warp["trigger"]["y"] == 255 else warp["trigger"]["y"],
                "marker": 4096,
                "requiredFlag": None,
                "requiredValue": True,
            }
            if (
                destination in selection["maps"]
                and not warp["retainsCoordinates"]
                and warp["scrollMode"] == 0
            ):
                events.append(
                    {
                        "kind": "warp",
                        **common,
                        "map": "map-" + str(destination),
                        "position": warp["destination"],
                        "facing": warp["facing"],
                    }
                )
            else:
                symbol = compiler.frontier(
                    f"map-{selected}-warp-{index}", f"{references['warpEventTable']}[{index}]"
                )
                events.append({"kind": "warp-frontier", **common, "program": _location(symbol)})
        on_load = compiler.frontier(
            f"map-{selected}-setup", f"{route_id}:ordered setup/init/population"
        )
        flag_table = references["flagEventTable"]
        flag_records = resources["flagEventTables"][flag_table]["records"]
        if flag_records:
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
        battle = battle_routes.get(selected)
        input_program = None
        if entities or resources["stepEventTables"][references["stepEventTable"]]["records"]:
            input_program = _location(
                compiler.frontier(
                    f"map-{selected}-input", f"{route_id}:input/event/autonomous-entity services"
                )
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
                    }
                    for a in areas
                ],
                "entities": entities,
                "events": events,
                "onLoad": _location(on_load),
                "battle": battle,
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
    for selected in selection["maps"]:
        row = map_rows[selected]
        route = resources["setupRoutes"].get(row["references"]["setupRoute"])
        if route is None:
            compiler.programs[f"map-{selected}-setup"]["instructions"] = [{"op": "end"}]
            continue
        setup = resources["setupDefinitions"][route["defaultSetup"]]
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
    texts_path = "disasm/data/scripting/text/gamescript.txt"
    compiler.sources.add(texts_path)
    # Private text retains source tags; the adapter does not claim original tag/audio timing.
    texts = [
        {"id": int(line[:4], 16), "text": line[5:]}
        for line in (upstream / texts_path).read_text(encoding="utf-8").splitlines()
        if re.match(r"^[0-9A-Fa-f]{4}=.+", line)
    ]
    sources = []
    for relative in sorted(compiler.sources):
        raw = (upstream / relative).read_bytes()
        committed = subprocess.run(
            ["git", "show", f"{expected_commit}:{relative}"],
            cwd=upstream,
            capture_output=True,
            check=True,
        ).stdout
        if raw.replace(b"\r\n", b"\n") != committed.replace(b"\r\n", b"\n"):
            raise ValueError("selected pinned source has local changes: " + relative)
        sources.append({"path": relative, "sha256": hashlib.sha256(raw).hexdigest().upper()})
    result = {
        "formatVersion": 1,
        "profile": "private-local-controlled-start",
        "package": selection["package"],
        "provenance": {
            **canonical["upstream"],
            "romSha256": canonical["romSha256"],
            "sources": sources,
            "controlledBoundary": selection["controlledBoundary"],
        },
        "world": {"maps": maps, "programs": list(compiler.programs.values()), "texts": texts},
    }
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps(result, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    return {"maps": len(maps), "programs": len(compiler.programs), "sources": len(sources)}


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--canonical", required=True, type=Path)
    parser.add_argument("--upstream", required=True, type=Path)
    parser.add_argument("--selection", required=True, type=Path)
    parser.add_argument("--output", required=True, type=Path)
    args = parser.parse_args()
    destination = args.output.resolve()
    repo = Path(__file__).resolve().parents[2]
    if not destination.is_relative_to(repo / "local"):
        raise SystemExit(
            "private content output must stay in this worktree's ignored local directory"
        )
    try:
        print(json.dumps(prepare(args.canonical, args.upstream, args.selection, destination)))
    except (ValueError, KeyError, OSError, subprocess.CalledProcessError) as error:
        # Do not expose local private input paths through an exception traceback.
        raise SystemExit(
            "private exploration preparation failed: " + type(error).__name__
        ) from None


if __name__ == "__main__":
    main()
