"""Lower source entity action streams used by maps and story programs."""

import re
from typing import Any

from sf2tool.h2.battle_global_data import _statements, _tokens


def finish_in_idle(actions: list[dict[str, Any]]) -> None:
    # Source esc34 clears the timer before entering eas_Idle in the same service.
    actions.extend([{"op": "jump", "instruction": len(actions) + 1}, {"op": "idle"}])


def action_stream(self, symbol: str) -> list[dict[str, Any]]:
    if symbol in self.actions:
        return self.actions[symbol]
    if symbol == "eas_Walking":
        path = "disasm/data/scripting/entity/eas_main.asm"
        text = (self.upstream / path).read_text(encoding="utf-8")
        match = re.search(r"^byte_45074:[ \t]*ac_randomWalk ([^\r\n;]+)", text, re.MULTILINE)
        if match is None:
            raise ValueError("source walking action operands")
        result = self.walking(*map(self.number, _tokens(match[1])))
        self.actions[symbol] = result
        return result
    path = "disasm/data/scripting/entity/eas_actions.asm"
    if symbol in ("eas_Motionless", "eas_LyingLeft", "eas_LyingRight"):
        path = "disasm/data/scripting/entity/eas_battleneutralentities.asm"
        self.sources.add(path)
        text = (self.upstream / path).read_text(encoding="utf-8")
        if not re.search(
            r"eas_Motionless:\s*ac_motion OFF\s+word_44984:\s*ac_branch"
            r"\s+dc.w eas_Idle-word_44984",
            text,
        ):
            raise ValueError("motionless source action drift")
        result = []
        if symbol != "eas_Motionless":
            direction = "Left" if symbol == "eas_LyingLeft" else "Right"
            if not re.search(rf"{symbol}:\s+ac_orient{direction}\s+ac_updateSprite", text):
                raise ValueError("lying source action drift")
            if direction == "Right" and not re.search(
                r"word_4498E:\s+ac_branch\s+dc.w \(eas_Motionless-word_4498E\) & \$FFFF", text
            ):
                raise ValueError("lying source continuation drift")
            result.extend(compile_action(self, "ac_orient" + direction, [], path + ":" + symbol))
            result.extend(compile_action(self, "ac_updateSprite", [], path + ":" + symbol))
        result.extend(compile_action(self, "ac_motion", ["OFF"], path + ":eas_Motionless"))
        finish_in_idle(result)
        self.actions[symbol] = result
        return result
    if symbol in self.raw:
        path = "disasm/" + self.raw[symbol]["path"]
    if symbol.startswith("eas_Follower"):
        path = "disasm/data/scripting/entity/eas_main.asm"
    self.sources.add(path)
    text = (self.upstream / path).read_text(encoding="utf-8")
    body = re.search(
        rf"^{re.escape(symbol)}:[ \t]*(.*?)(?=^(?!byte_|word_)[A-Za-z_]\w*:|\Z)",
        text,
        re.MULTILINE | re.DOTALL,
    )
    if body is None:
        raise ValueError("selected action source label is missing: " + symbol)
    if symbol.startswith("eas_Follower"):
        result, labels, pending = [], {}, False
        for line in body[1].splitlines():
            if match := re.match(r"^(\w+):[ \t]*(.*)", line):
                labels[match[1]] = len(result)
                line = match[2]
            for statement in _statements(line):
                op, _, operands = statement.partition(" ")
                if op == "ac_branch":
                    pending = True
                elif op == "dc.w" and pending:
                    target = re.match(r"\((\w+)-\w+\)", operands.strip())
                    if target is None or target[1] not in labels:
                        raise ValueError("source action branch")
                    result.append({"op": "jump", "instruction": labels[target[1]]})
                    pending = False
                else:
                    result.extend(compile_action(self, op, _tokens(operands), path + ":" + symbol))
        self.actions[symbol] = result
        return result
    rows = self.source_operations(path, symbol)
    result: list[dict[str, Any]] = []
    self.actions[symbol] = result
    for row in rows:
        op, args = row["opcode"], _tokens(row["operandText"])
        source = f"{path}:{symbol}[{row['index']}]"
        if op == "ac_jump" and args == ["eas_Idle"]:
            finish_in_idle(result)
            break
        result.extend(compile_action(self, op, args, source))
        if op in ("ac_end", "ac_jump"):
            break
    return result


def compile_action(self, op: str, args: list[str], source: str) -> list[dict[str, Any]]:
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
    if op == "jumpUp":
        # csc2D_D copies the 34-byte eas_Jump body, excluding its jump to idle.
        self.sources.add("disasm/code/common/scripting/map/mapscriptengine_1.asm")
        jump = action_stream(self, "eas_Jump")
        return jump[:-2]
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
        ticks = self.number(args[0])
        return [{"op": "wait", "ticks": ticks}] if 0 <= ticks <= 255 else [self.native(op, source)]
    if op == "ac_motion":
        return [{"op": "animation-counter", "counter": 1 if self.number(args[0]) else 255}]
    if op == "ac_moveRel":
        return [{"op": "move", "x": self.number(args[0]), "y": self.number(args[1]), "wait": False}]
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
        "ac_animSpeedX2": ("b", 16),
        "ac_resizable": ("b", 8),
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
                "value": (
                    (int(self.number(args[0]) != 0) | (int(self.number(args[1]) != 0) << 1))
                    << shift
                ),
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
    if op == "ac_setPos":
        return [
            {"op": "position", "position": {"x": self.number(args[0]), "y": self.number(args[1])}}
        ]
    if op == "ac_updateSprite":
        return [{"op": "refresh-sprite", "source": source}]
    if op == "ac_follow":
        return [
            {
                "op": "follow-physical",
                "slot": self.number(args[0]),
                "x": self.number(args[1]),
                "y": self.number(args[2]),
            }
        ]
    if op == "ac_waitDest":
        return [{"op": "wait-destination"}]
    if op == "ac_soundCommand":
        return [{"op": "sound", "command": self.number(args[0])}]
    if op == "ac_setFacing":
        return [{"op": "face", "facing": self.number(args[0])}]
    return [self.native(op, source)]
