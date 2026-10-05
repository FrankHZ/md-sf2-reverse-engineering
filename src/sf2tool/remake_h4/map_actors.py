"""Derive actor visibility and legal passes before generating original sprite masks."""

import re

from sf2tool.compression import decode_basic_compressed
from sf2tool.remake_asset_build import _render_player_frame
from sf2tool.remake_h4.map_geometry import add, f, intersect, mul


def admit_actors(projection, screen, scale, actor_population_complete):
    # ExplorationPresentation allocates one pass per actor, then restores each
    # high plane only for an onscreen low-priority actor. Visibility and bounds
    # come from geometry, never the reported visible flag or mask pass itself.
    actor_ok, actor_rows, unknown_subjects = True, [], set()
    missing_actor = False
    passes = {4}
    foreground = any(layer["name"].startswith("foreground") for layer in projection["layers"])
    previous_order = None
    for actor in projection["actors"]:
        size = mul(24, scale)
        bounds = (f(actor["x"]), f(actor["y"]), size, size) if {"x", "y"} <= actor.keys() else None
        visible = intersect(screen, bounds) is not None if bounds is not None else None
        if bounds is None:
            missing_actor = True
            if actor.get("entity") is not None:
                unknown_subjects.add(actor["entity"])
        for key, value in (("width", size), ("height", size), ("visible", visible)):
            if key not in actor:
                missing_actor = True
            elif value is not None:
                actor_ok &= (
                    actor[key] is value if key == "visible" else abs(actor[key] - value) < 0.002
                )
        high = None
        if "layer" in actor:
            layer = int(actor["layer"])
            high = (layer if layer < 128 else layer - 256) > 0
            if "highPriority" in actor:
                actor_ok &= actor["highPriority"] is high
        if {"spritePriority", "y"} <= actor.keys():
            order = (actor["spritePriority"], actor["y"])
            actor_ok &= previous_order is None or previous_order <= order
            previous_order = order
        pass_ = next(iter(passes)) if len(passes) == 1 else None
        if actor_population_complete:
            if "pass" not in actor:
                missing_actor = True
            else:
                actor_ok &= actor["pass"] in passes
                # Older captures omit priority for actors outside the witnessed
                # region. Retain only renderer-legal allocations at that seam.
                if actor["pass"] in passes:
                    pass_ = int(actor["pass"])
        else:
            missing_actor = True
            pass_ = None
        actor_rows.append((actor, bounds, visible, high, pass_))
        increments = {1} if visible is False or high is True else {2 + int(foreground)}
        if visible is None or visible and high is None:
            increments.add(1)
        passes = {p + n for p in ({pass_} if pass_ is not None else passes) for n in increments}

    return actor_ok, actor_rows, missing_actor, unknown_subjects


class SpriteInk:
    """Source alpha runs cached only for one draw inventory."""

    def __init__(self, root, scale):
        self.root = root
        self.scale = scale
        self.entries = None
        self.runs = {}

    def regions(self, actor):
        sprite, facing = int(actor["sprite"]), int(actor["facing"])
        direction = 0 if facing == 1 else 2 if facing == 3 else 1
        half = int(15 < actor["animationCounter"] < 128)
        key = sprite, direction, half
        if actor["lowered"] or actor["mosaicBlock"] is not None or actor["shiverOffsetX"]:
            raise ValueError("mutable map witness has an unallocated actor transformation")
        if key not in self.runs:
            if self.entries is None:
                text = (self.root / "disasm/data/graphics/mapsprites/entries.asm").read_text(
                    encoding="utf-8"
                )
                self.entries = (
                    re.findall(r"\bdc\.l\s+(Mapsprite\d{3}_[012])", text),
                    dict(re.findall(r'(Mapsprite\d{3}_[012]):\s*incbin\s+"([^"]+)"', text)),
                )
            references, paths = self.entries
            payload = (
                self.root / "disasm" / paths[references[sprite * 3 + direction]]
            ).read_bytes()
            decoded = decode_basic_compressed(payload, expected_output_bytes=576).output
            pixels = _render_player_frame(decoded[half * 288 : (half + 1) * 288], [(0, 0, 0)] * 16)
            runs = []
            for y in range(24):
                x = 0
                while x < 24:
                    if not pixels[(y * 24 + x) * 4 + 3]:
                        x += 1
                        continue
                    start = x
                    x += 1
                    while x < 24 and pixels[(y * 24 + x) * 4 + 3]:
                        x += 1
                    runs.append((start, y, x - start, 1))
            self.runs[key] = runs
        mirror = facing in (0, 4, 7)
        return [
            (
                add(actor["x"], mul(24 - x - w if mirror else x, self.scale)),
                add(actor["y"], mul(y, self.scale)),
                mul(w, self.scale),
                mul(h, self.scale),
            )
            for x, y, w, h in self.runs[key]
        ]
