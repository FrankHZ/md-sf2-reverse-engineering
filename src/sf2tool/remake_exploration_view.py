"""Lower the reached native view-register operations and finite counted loops."""

import re

from sf2tool.h2.battle_global_data import _tokens


def view_operation(compiler, op, args):
    if len(args) != 2 or not args[0].startswith("#"):
        return None
    if match := re.fullmatch(
        r"\(\(MAP_AREA_LAYER([12])_AUTOSCROLL_([XY])-\$1000000\)\)\.w", args[1]
    ):
        if op not in ("move.b", "addq.b", "subq.b"):
            return None
        amount = compiler.number(args[0][1:]) * (-1 if op == "subq.b" else 1)
        return {
            "op": "plane-autoscroll",
            "axis": (int(match[1]) - 1) * 2 + int(match[2] == "Y"),
            "value": amount,
            "relative": op != "move.b",
        }
    if op == "move.w" and (
        match := re.fullmatch(r"\(\(VIEW_PLANE_([AB])_PIXEL_([XY])-\$1000000\)\)\.w", args[1])
    ):
        return {
            "op": "plane-position",
            "axis": int(match[1] == "B") * 2 + int(match[2] == "Y"),
            "value": compiler.number(args[0][1:]),
        }
    return None


def counted_view_loop(compiler, relative, symbol, rows):
    # The caller supplies a finite d7 count immediately before the labeled loop.
    # Read it from source, never infer a hardcoded scene-specific repetition count.
    if not any(row["opcode"] == "dbf" for row in rows):
        return None
    text = (compiler.upstream / relative).read_text(encoding="utf8")
    match = re.search(r"moveq\s+#([^,\s]+),d7\s*\n" + re.escape(symbol) + r":", text)
    if match is None:
        return None
    end = next(i for i, r in enumerate(rows) if r["opcode"] == "dbf")
    if _tokens(rows[end]["operandText"]) != ["d7", symbol]:
        return None
    ops = []
    delay = None
    for row in rows[:end]:
        op, args = row["opcode"], _tokens(row["operandText"])
        lowered = view_operation(compiler, op, args)
        if lowered is not None:
            ops.append(lowered)
        elif op == "moveq" and len(args) == 2 and args[1] == "d0":
            delay = compiler.number(args[0][1:])
        elif op == "jsr" and args == ["(Sleep).w"] and delay is not None:
            ops.append({"op": "wait-ticks", "ticks": delay})
        elif op == "jsr" and args == ["(WaitForVInt).w"]:
            ops.append({"op": "wait-ticks", "ticks": 1})
        else:
            return None
    count = compiler.number(match[1]) + 1
    if not 1 <= count <= 256:
        raise ValueError("view loop budget")
    return ops * count, rows[end + 1 :]
