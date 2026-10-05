"""Pinned HEAL source tables and one independent fairy/controller opportunity."""

import re
import subprocess
from pathlib import Path

from sf2tool.h3.rng import _rng_step
from sf2tool.paths import repo_path
from sf2tool.remake_h4_reference import UPSTREAM


def load_tables(source_root, checks):
    """Read pinned motion, cast and idle operands; caller owns no source resources."""
    check = checks.check
    quarter, cast, idle = None, None, None
    try:
        root = Path(source_root) if source_root is not None else None
        if root is not None:
            root = root.resolve() if root.is_absolute() else repo_path(root)
        if root is None:
            raise ValueError("source unavailable")
        pin = subprocess.check_output(
            ["git", "-C", str(root), "rev-parse", "HEAD"], text=True
        ).strip()
        clean = subprocess.run(
            ["git", "-C", str(root), "diff", "--quiet", UPSTREAM, "--", "disasm"], check=False
        ).returncode
        check("pinned clean source", pin == UPSTREAM and clean == 0)
        data = (
            (root / "disasm/data/tech/spellanimations.asm")
            .read_text(encoding="utf-8")
            .split("table_1840:", 1)[1]
        )
        quarter = [
            int(x, 16) if x.startswith("0x") else int(x) for x in re.findall(r"dc\.w\s+(\d+)", data)
        ][:65]
        sprites = root / "disasm/data/graphics/battles/battlesprites/allies"
        cast = (sprites / "animations/allyanimation001.bin").read_bytes()
        idle = [
            int.from_bytes((sprites / f"allybattlesprite{i:02}.bin").read_bytes()[:2], "big")
            for i in range(3)
        ]
        check("source tables available", len(quarter) == 65 and len(cast) == 8 + cast[0] * 8)
    except (OSError, ValueError, subprocess.SubprocessError):
        check("source rules unavailable", None)

    return quarter, cast, idle


def fairy_source_step(previous, seed, quarter, setup=False):
    """Pinned healingfairy setup/update + controller word semantics, at one opportunity.

    Source: setup 1A848, update 1C53E, graphics sub_179C/table_1840. This
    independent matched-state rule uses actual inputs, never a captured output as expected.
    """
    import copy

    draws = []

    def draw(range_, purpose):
        nonlocal seed
        word, value = _rng_step(int(seed) >> 16, range_ * 2)
        after = (word << 16) | (int(seed) & 65535)
        draws.append(
            dict(
                Kind="rng-fairy-" + purpose,
                Before=seed,
                After=after,
                RandomRange=range_,
                RandomValue=value >> 1,
            )
        )
        seed = after
        return value >> 1

    def signed(value):
        value = int(value) & 65535
        return value if value < 32768 else value - 65536

    if setup:
        y, delay, dust = (
            draw(32, "setup-y") + 128,
            draw(30, "setup-delay") + 1,
            draw(12, "setup-dust") + 1,
        )
        state = dict(
            Lifetime=65535,
            Control=1,
            ActiveCount=1,
            CleanupPending=False,
            PendingDustX=0,
            PendingDustY=0,
            Fairies=[
                dict(
                    Age=1,
                    Active=True,
                    Phase=7,
                    Angle=0,
                    Speed=0,
                    XFraction=delay,
                    YFraction=0,
                    WingClock=0,
                    WingFrame=0,
                    DustClock=dust,
                    X=384,
                    Y=y,
                    BodyFrame=1,
                    Mirrored=False,
                )
            ],
            Dust=[dict(Age=0, Frame=0, Clock=0, X=0, Y=0) for _ in range(23)],
        )
        return state, seed, draws
    state = copy.deepcopy(previous)

    def cleanup():
        state.update(
            Lifetime=0,
            Control=0,
            PendingDustX=0,
            PendingDustY=0,
            CleanupPending=True,
            ActiveCount=0,
        )
        for f in state["Fairies"]:
            for key in (
                "Age",
                "Phase",
                "Angle",
                "Speed",
                "XFraction",
                "YFraction",
                "WingClock",
                "WingFrame",
                "DustClock",
            ):
                f[key] = 0
            f["Active"] = False
        state["Dust"] = [dict(Age=0, Frame=0, Clock=0, X=0, Y=0) for _ in state["Dust"]]

    if state["Control"] > 2:
        cleanup()
        return state, seed, draws
    if not state["Control"] or not any(f["Age"] for f in state["Fairies"]):
        return state, seed, draws
    lifetime = 0 if state["Control"] == 2 else max(0, state["Lifetime"] - 1)
    state["Lifetime"] = lifetime
    for f in state["Fairies"]:
        if not f["Age"]:
            continue
        f["Age"] = (int(f["Age"]) + 1) & 65535
        phase = int(f["Phase"])
        if phase & 3 == 3:
            if not lifetime:
                f["Age"], f["Active"] = 0, False
            else:
                f["XFraction"] = signed(f["XFraction"] - 1)
                if not f["XFraction"]:
                    f.update(
                        Age=2,
                        Phase=(phase + 1) & 7,
                        Angle=(230 + draw(16, "reentry")) << 4,
                        Speed=240,
                        XFraction=0,
                        YFraction=0,
                    )
            continue
        if f["Age"] in (44, 72):
            phase = (phase + 1) & 65535
            f.update(Phase=phase, BodyFrame=0 if f["Age"] == 44 else 1, Mirrored=bool(phase & 4))
        speed = signed(f["Speed"] + (-20 if phase & 1 else 20))
        if phase & 1:
            speed = max(0, speed)
        else:
            f["Angle"] = (int(f["Angle"]) + 6) & 4095
        f["Speed"] = speed & 65535
        angle = int(f["Angle"]) >> 4
        low, high = quarter[angle & 63], quarter[64 - (angle & 63)]
        horizontal, vertical = ((high, -low), (-low, -high), (-high, low), (low, high))[
            (angle & 255) >> 6
        ]
        for direction, coordinate, residual in (
            (horizontal, "X", "XFraction"),
            (vertical, "Y", "YFraction"),
        ):
            value = signed(((direction * speed) >> 8) + f[residual])
            whole = abs(value) // 256 * (-1 if value < 0 else 1)
            f[residual] = value - whole * 256
            if coordinate == "X" and not phase & 4:
                whole = -whole
            f[coordinate] = (int(f[coordinate]) + whole) & 65535
        f["WingClock"] = (int(f["WingClock"]) + 1) & 255
        if f["WingClock"] >= 4:
            f["WingClock"], f["WingFrame"] = 0, int(f["WingFrame"]) ^ 1
        if not 96 <= f["X"] <= 384:
            phase = (phase + 1) & 65535
            f.update(
                Phase=phase,
                XFraction=1 + draw(28, "boundary-delay"),
                X=384 if phase & 4 else 96,
                Y=128 + draw(32, "boundary-y"),
                Mirrored=not f["Mirrored"],
            )
        f["DustClock"] = (int(f["DustClock"]) - 1) & 65535
        if not f["DustClock"]:
            f["DustClock"] = 3 + draw(12, "dust")
            state["PendingDustX"], state["PendingDustY"] = f["X"], f["Y"]
    for d in state["Dust"]:
        if not d["Age"]:
            if state["PendingDustY"]:
                d.update(
                    Age=1,
                    Frame=0,
                    Clock=6,
                    X=(int(state["PendingDustX"]) + 12) & 65535,
                    Y=(int(state["PendingDustY"]) + 12) & 65535,
                )
                state["PendingDustX"] = state["PendingDustY"] = 0
        else:
            d.update(
                Age=(int(d["Age"]) + 1) & 65535,
                Y=(int(d["Y"]) + 1) & 65535,
                Clock=(int(d["Clock"]) - 1) & 65535,
            )
            if not d["Clock"]:
                if d["Frame"] == 4:
                    d.update(Age=0, Frame=0, Clock=6, X=1, Y=1)
                else:
                    d.update(Clock=6, Frame=d["Frame"] + 1)
    state["ActiveCount"] = sum(bool(f["Age"]) for f in state["Fairies"])
    if state["ActiveCount"] == 0:
        cleanup()
    return state, seed, draws
