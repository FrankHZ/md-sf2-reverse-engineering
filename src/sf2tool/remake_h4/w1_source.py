"""Pinned W1 source cohort, token/control-flow rules and RNG arithmetic."""

import re
import subprocess
from dataclasses import dataclass
from pathlib import Path

from sf2tool.h3.rng import _rng_step
from sf2tool.paths import repo_path
from sf2tool.remake_h4_reference import UPSTREAM

_W1_COHORT = (
    ("cs-5145c", 12, 483),
    ("byte-50f6a", 3, 481),
    ("map3-zoneevent7", 5, 513),
    ("byte-51052", 1, 501),
    ("byte-50e96", 8, 516),
    ("cs-5149a", 15, 519),
    ("cs-5149a", 23, 520),
    ("cs-5149a", 42, 521),
    ("cs-5149a", 48, 522),
    ("cs-5149a", 54, 523),
    ("cs-5149a", 60, 524),
    ("cs-5149a", 79, 525),
    ("cs-5149a", 97, 527),
    ("cs-5149a", 107, 528),
    ("cs-5149a", 113, 529),
    ("cs-5149a", 119, 530),
    ("cs-5149a", 127, 531),
    ("cs-51614", 3, 535),
    ("cs-51614", 10, 536),
    ("cs-51652", 7, 537),
    ("cs-51652", 15, 538),
    ("cs-51652", 21, 539),
    ("cs-51652", 27, 540),
    ("cs-51652", 33, 541),
    ("cs-51652", 39, 542),
    ("cs-53996", 16, 2176),
    ("cs-53996", 22, 2177),
    ("cs-53996", 30, 2178),
    ("cs-53996", 36, 2179),
    ("cs-53996", 46, 2180),
    ("cs-53996", 53, 2181),
    ("cs-53996", 70, 2182),
    ("cs-53996", 84, 2183),
    ("cs-53996", 91, 2184),
    ("cs-53996", 100, 2185),
    ("cs-53996", 108, 2186),
    ("cs-53996", 116, 2187),
    ("cs-53996", 124, 2188),
    ("cs-53996", 132, 2189),
    ("cs-53996", 135, 2190),
    ("cs-53996", 141, 2191),
    ("cs-53996", 151, 2192),
    ("cs-53996", 158, 2193),
    ("cs-53996", 167, 2194),
    ("cs-53996", 175, 2195),
    ("cs-53996", 182, 2196),
    ("cs-52f0c", 3, 575),
    ("cs-52f0c", 8, 576),
    ("cs-52f24", 3, 577),
    ("cs-52f40", 3, 578),
    ("byte-53ec8", 1, 579),
    ("byte-53ec8", 1, 579),
    ("bbcs-01", 20, 2293),
    ("bbcs-01", 31, 2294),
    ("bbcs-01", 39, 2295),
    ("bbcs-01", 46, 2296),
    ("bbcs-01", 72, 2297),
    ("bbcs-01", 79, 2298),
    ("bbcs-01", 96, 2300),
    ("bbcs-01", 105, 2301),
    ("bbcs-01", 113, 2302),
    ("bbcs-01", 150, 2304),
    ("abcs-battle01", 14, 2305),
    ("abcs-battle01", 46, 2306),
    ("abcs-battle01", 56, 2307),
    ("abcs-battle01", 75, 2308),
    ("abcs-battle01", 83, 2309),
    ("abcs-battle01", 96, 2310),
)
_W1_SOURCE_FILES = (
    "data/battles/entries/battle01/cs_afterbattle.asm",
    "data/battles/entries/battle01/cs_beforebattle.asm",
    "data/maps/entries/map03/mapsetups/s2_entityevents.asm",
    "data/maps/entries/map03/mapsetups/s3_zoneevents.asm",
    "data/maps/entries/map03/mapsetups/s6_initfunction.asm",
    "data/maps/entries/map03/mapsetups/scripts_1.asm",
    "data/maps/entries/map19/mapsetups/s2_entityevents.asm",
    "data/maps/entries/map20/mapsetups/s6_initfunction.asm",
    "data/maps/entries/map21/mapsetups/s2_entityevents_506.asm",
)
_W1_SOURCE_SYMBOLS = (
    "Map3_DefaultEntityEvent",
    "Map3_DefaultZoneEvent",
    "Map3_EntityEvent15",
    "Map3_ZoneEvent4",
    "Map3_ZoneEvent6",
    "Map3_ZoneEvent7",
    "Map3_ZoneEvent8",
    "abcs_battle01",
    "bbcs_01",
    "byte_50E2C",
    "byte_50E32",
    "byte_50E96",
    "byte_50F6A",
    "byte_51052",
    "byte_51390",
    "byte_513A8",
    "byte_53EC8",
    "cs_513A0",
    "cs_513BA",
    "cs_513D6",
    "cs_51454",
    "cs_5145C",
    "cs_5148C",
    "cs_5149A",
    "cs_51614",
    "cs_51650",
    "cs_51652",
    "cs_52F0C",
    "cs_52F24",
    "cs_52F40",
    "cs_53996",
    "cs_53B60",
    "cs_53EF4",
    "loc_50F82",
    "loc_50F88",
    "ms_map3_InitFunction",
    "return_50E42",
    "return_50E64",
    "return_50ED0",
    "return_50F96",
    "return_513B8",
    "return_53EDC",
)


def load_source(source_root, checks):
    check = checks.check
    programs, texts, names, compiler, layout, mutable = {}, {}, [], None, None, []
    portrait_ids = {}
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
        check("pinned original source", pin == UPSTREAM and clean == 0)
        if pin == UPSTREAM and clean == 0:
            from sf2tool.h2.map_layouts import decode_map_blocks, decode_map_layout
            from sf2tool.remake_exploration_content import OriginalPrograms

            compiler = OriginalPrograms(
                {"resources": {"standaloneScriptPrograms": [], "initSourcePrograms": []}}, root
            )
            for relative in _W1_SOURCE_FILES:
                compiler.register_file("disasm/" + relative)
            for symbol in _W1_SOURCE_SYMBOLS:
                compiler.compile(symbol)
            programs = {p["id"]: p for p in compiler.programs.values()}
            sprite = None
            for row in compiler.source_operations(
                "disasm/data/spritedialogproperties.asm", "table_MapspriteDialogueProperties"
            ):
                if row["opcode"] == "mapsprite":
                    sprite = compiler.number("MAPSPRITE_" + row["operandText"])
                elif row["opcode"] == "portrait" and sprite is not None:
                    portrait_ids[sprite] = compiler.number("PORTRAIT_" + row["operandText"])
                    if portrait_ids[sprite] >= 128:
                        portrait_ids[sprite] -= 256
            script = (root / "disasm/data/scripting/text/gamescript.txt").read_text(
                encoding="utf-8"
            )
            texts = {
                int(line[:4], 16): line[5:]
                for line in script.splitlines()
                if re.match(r"^[0-9A-Fa-f]{4}=", line)
            }
            names = re.findall(
                r'"([^"]*)"',
                (root / "disasm/data/stats/allies/allynames.asm").read_text(encoding="utf-8"),
            )
            folder = root / "disasm/data/maps/entries/map03"
            blocks = decode_map_blocks((folder / "0-blocks.bin").read_bytes())[0]
            layout = decode_map_layout((folder / "1-layout.bin").read_bytes(), len(blocks) // 9)[0]
            for file, macro in (
                ("3-flag-events.asm", "fbc"),
                ("4-step-events.asm", "sbc"),
                ("5-roof-events.asm", "slbc"),
            ):
                source = (folder / file).read_text(encoding="utf-8")
                sizes = re.findall(rf"{macro}Size\s+(\d+),\s*(\d+)", source)
                destinations = re.findall(rf"{macro}Dest\s+(\d+),\s*(\d+)", source)
                mutable.extend(
                    tuple(map(int, (*d, *s))) for d, s in zip(destinations, sizes, strict=True)
                )
    except (OSError, ValueError, subprocess.SubprocessError):
        check("original operands available", None)

    return W1Source(programs, texts, names, compiler, layout, mutable, portrait_ids)


def random(seed, range_):
    if seed is None:
        return None, None
    word, value = _rng_step(int(seed) >> 16, range_ * 2)
    return (word << 16) | (int(seed) & 65535), value >> 1


@dataclass
class W1Source:
    """Independently loaded source operands; no candidate observations."""

    programs: dict
    texts: dict
    names: list
    compiler: object
    layout: list | None
    mutable: list
    portrait_ids: dict

    def source_units(self, text):
        names = self.names
        out = []
        for part in re.split(r"(\{[^}]+\})", text):
            if part in ("{N}", "{W1}", "{W2}", "{D1}"):
                out.append(
                    (
                        {"{N}": 1, "{W1}": 3, "{W2}": 4, "{D1}": 6}[part],
                        "\n" if part == "{N}" else "",
                    )
                )
                continue
            if part.startswith("{NAME;"):
                part = names[int(part[6:-1])]
            elif part == "{LEADER}":
                part = names[0]  # The selected original-language cohort's named leader.
            elif part.startswith("{"):
                return None
            out.extend((0, ch) for ch in part)
        return out

    def resume_paths(self, cursor, open_portrait):
        programs = self.programs
        # Bounded source control flow only until the first blocking continuation.
        # Both flag outcomes are legal here; flag truth has a separate owner.
        details = {
            "close-portrait": "ClosePortrait",
            "close-text": "CloseText",
            "open-portrait": "OpenPortrait",
            "wait-view": "WaitForView",
            "yes-no": "ChooseYesNo",
            "branch-flag": "BranchFlag",
            "jump": "JumpProgram",
            "call": "CallProgram",
            "motion": "StartEntityMotion",
            "set-flag": "WriteFlag",
            "position": "SetEntityPosition",
            "end": "EndProgram",
            "end-map-script": "EndProgram",
        }
        queue = [(cursor["Program"], int(cursor["Instruction"]) + 1, [])]
        paths = []
        while queue:
            program, i, path = queue.pop()
            instructions = programs.get(program, {}).get("instructions", [])
            if len(path) >= 16 or not 0 <= i < len(instructions):
                continue
            op = instructions[i]
            kind = op.get("op")
            if kind not in details:
                continue
            cur = dict(Program=program, Instruction=i)
            path = path + [dict(Program=cur, Detail=details[kind])]
            wait = None
            if kind == "close-portrait" and open_portrait:
                wait = "PortraitMovementWait"
            elif kind == "close-text":
                wait = "TextCloseWait"
            elif kind in ("wait-view", "end-map-script"):
                wait = "ViewWait"
            elif kind == "yes-no":
                wait = "ChoiceWait"
            elif kind == "motion" and op.get("wait"):
                wait = "EntityWait"
            elif kind == "end":
                wait = "PortraitMovementWait" if open_portrait else "TextCloseWait"
                cur = None
            if wait:
                paths.append((path, cur, wait))
                continue
            if kind in ("jump", "call", "branch-flag"):
                target = op.get("target", {})
                queue.append((target.get("program"), target.get("instruction", 0), path))
                if kind == "branch-flag":
                    queue.append((program, i + 1, path))
            else:
                queue.append((program, i + 1, path))
        return paths
