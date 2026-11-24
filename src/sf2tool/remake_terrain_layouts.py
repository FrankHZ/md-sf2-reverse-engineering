"""Source-space footprints and working-layout alternatives for the selected pre-voyage terrain."""

from __future__ import annotations

import itertools
import json
from dataclasses import dataclass

# Campaign selections. Operands come from the selected world/encounter, including
# the map switch halfway through the docks script and the arrival continuation.
MAPS = (3, 4, 66, 19, 20, 21, 40, 57, 60, 58, 5, 43, 41, 42, 16, 17, 18, 44, 73, 14)
FIELD_SCRIPTS = (
    (4, ("cs-51750",)),
    (17, ("cs-5247c", "cs-52530", "cs-528d4")),
    (44, ("cs-54578",)),
    (73, ("cs-503a6", "cs-50426", "cs-50624")),
)


@dataclass(frozen=True)
class Rect:
    x: int
    y: int
    w: int
    h: int

    def clipped(self, other):
        x, y = max(self.x, other.x), max(self.y, other.y)
        right, bottom = (
            min(self.x + self.w, other.x + other.w),
            min(self.y + self.h, other.y + other.h),
        )
        if right > x and bottom > y:
            return Rect(x, y, right - x, bottom - y)
        return None

    def halo(self, radius=1):
        return Rect(self.x - radius, self.y - radius, self.w + 2 * radius, self.h + 2 * radius)

    def offset(self, x, y):
        return Rect(self.x + x, self.y + y, self.w, self.h)


BOARD = Rect(0, 0, 64, 64)


@dataclass(frozen=True)
class Edit:
    dest: Rect
    source: tuple[int, int] | None = None
    fill: int = 0

    @classmethod
    def copy(cls, row):
        c = row["copy"]
        dest = Rect(c["destination"]["x"], c["destination"]["y"], c["width"], c["height"])
        src = None if row.get("clear") else (c["source"]["x"], c["source"]["y"])
        return cls(dest, src)

    def apply(self, layout):
        result = [row.copy() for row in layout]
        d = self.dest
        for y in range(d.h):
            for x in range(d.w):
                result[d.y + y][d.x + x] = (
                    self.fill
                    if self.source is None
                    else result[self.source[1] + y][self.source[0] + x]
                )
        return result


def area_rect(a):
    return Rect(a["minX"], a["minY"], a["maxX"] - a["minX"] + 1, a["maxY"] - a["minY"] + 1)


def area_planes(a):
    rect = area_rect(a)
    yield rect
    yield rect.offset(a["overlay"]["x"], a["overlay"]["y"])
    view = a["view"]
    for plane in ("background", "foreground"):
        yield rect.offset(view[plane + "X"], view[plane + "Y"])


def encounter_regions(base, doc):
    paths = [base / "opening-inputs/SF2_PRIVATE_BATTLE01_SCENE.json"]
    paths.extend(base / row["scene"] for row in doc["encounters"])
    for path in paths:
        scene = json.loads(path.read_bytes())
        if scene["battle"]["id"] <= 7:
            yield scene["battle"]["id"], scene["map"]


def script_edits(programs, start_map, chain):
    current = start_map
    edits = []
    for name in chain:
        for row in programs[name]["instructions"]:
            if row["op"] == "scene-map":
                if edits:
                    yield current, edits
                current = int(row["map"].removeprefix("map-"))
                edits = []
            elif row["op"] == "copy-layout":
                edits.append(Edit.copy(row))
    if edits:
        yield current, edits


def independent_groups(edits):
    # Only changes within two cells can jointly affect a 3x3 key. Group
    # overlapping source reads too, so ordered copies can consume a prior edit.
    groups = []
    for edit in dict.fromkeys(edits):
        group = [edit]
        for existing in groups.copy():
            if any(interact(edit, other) for other in existing):
                group.extend(existing)
                groups.remove(existing)
        groups.append(group)
    return groups


def interact(a, b):
    if a.dest.halo(2).clipped(b.dest):
        return True
    return any(
        read.source and write.dest.clipped(Rect(*read.source, read.dest.w, read.dest.h))
        for read, write in ((a, b), (b, a))
    )


def option_states(layout, map_def):
    events = map_def.get("layoutEvents", {})
    search = map_def.get("search", {})
    singles = [Edit.copy(row) for row in events.get("doors", [])]
    singles += [
        Edit(
            Rect(row["position"]["x"], row["position"]["y"], 1, 1), fill=search["openChest"] & 1023
        )
        for row in search.get("chests", [])
    ]
    roofs = list(dict.fromkeys(Edit.copy(row) for row in events.get("roofs", [])))
    for group in independent_groups(singles):
        for count in range(1, len(group) + 1):
            for chosen in itertools.combinations(group, count):
                state = layout
                for edit in chosen:
                    state = edit.apply(state)
                yield state, [edit.dest for edit in chosen]
                for roof in roofs:
                    if any(interact(edit, roof) for edit in chosen):
                        yield roof.apply(state), [roof.dest, *(e.dest for e in chosen)]
    for roof in roofs:
        yield roof.apply(layout), [roof.dest]


def flag_layouts(initial, events):
    flags = dict.fromkeys(row["flag"] for row in events.get("flags", []))
    for count in range(len(flags) + 1):
        for active in itertools.combinations(flags, count):
            layout = initial
            for row in events.get("flags", []):
                if row["flag"] in active:
                    layout = Edit.copy(row).apply(layout)
            yield layout


def projections(number, map_def, programs, encounters):
    """Yield (layout, plane footprints, affected rectangles, comparison label)."""
    initial = [[word & 1023 for word in row] for row in map_def["layout"]]
    planes = {p for area in map_def["areas"] for p in area_planes(area)}
    chains = [
        edits
        for start, chain in FIELD_SCRIPTS
        for target, edits in script_edits(programs, start, chain)
        if target == number
    ]
    # Detached/autoscrolling secondary cameras can sample beyond the ordinary
    # packed room. Prepare the full source layout for these independent reads.
    if any(
        i["op"] in ("plane-position", "plane-autoscroll")
        for p in programs.values()
        if f"/map{number:02}/" in p["source"]
        for i in p["instructions"]
    ):
        planes.add(BOARD)
    regions = []
    for battle, region in encounters:
        if region["id"] != number:
            continue
        rect = Rect(region["x"], region["y"], region["width"], region["height"])
        area = next(a for a in map_def["areas"] if area_rect(a).clipped(Rect(rect.x, rect.y, 1, 1)))
        planes.add(rect)
        view = area["view"]
        if any(area["overlay"].values()):
            prefix = "background" if view["layer"] else "foreground"
            planes.add(Rect(view[prefix + "X"], view[prefix + "Y"], rect.w, rect.h))
        regions.append((battle, rect))
    planes = sorted(
        {c for p in planes if (c := p.clipped(BOARD))}, key=lambda r: (r.y, r.x, r.w, r.h)
    )
    for flags, layout in enumerate(flag_layouts(initial, map_def.get("layoutEvents", {}))):
        yield layout, planes, [BOARD], f"load-{flags}"
        for state, dirty in option_states(layout, map_def):
            yield state, planes, dirty, "field-event"
        for chain in chains:
            state = layout
            for step, edit in enumerate(chain, 1):
                state = edit.apply(state)
                yield state, planes, [edit.dest], f"script-{step}"
                for variant, dirty in option_states(state, map_def):
                    yield variant, planes, dirty, "field-event"
        for battle, rect in regions:
            # Field before/after scenes can copy the encounter crop to (0,0).
            local = Rect(0, 0, rect.w, rect.h)
            copy = Edit(local, (rect.x, rect.y))
            state = copy.apply(layout)
            scene_planes = [*planes, local]
            yield state, scene_planes, [BOARD], "scene-load"
            after = f"abcs-battle{battle:02}"
            if after in programs:
                for target, edits in script_edits(programs, number, [after]):
                    if target == number:
                        for edit in edits:
                            state = edit.apply(state)
                            yield state, scene_planes, [edit.dest], "scene-copy"
