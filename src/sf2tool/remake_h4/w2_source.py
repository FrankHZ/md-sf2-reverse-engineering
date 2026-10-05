"""Independent pinned W2 text and bounded compiled source continuation."""

import re
import subprocess
from dataclasses import dataclass
from pathlib import Path

from sf2tool.paths import repo_path
from sf2tool.remake_h4_reference import UPSTREAM

_W2_COHORT = (
    ("cs-5145c", 5, 510),
    ("cs-5145c", 8, 511),
    ("map3-entityevent0", 4, 512),
    ("map3-entityevent15", 2, 500),
    ("byte-50e96", 4, 514),
    ("byte-50e96", 6, 515),
    ("cs-5149a", 9, 517),
    ("cs-5149a", 12, 518),
    ("cs-5149a", 94, 526),
    ("cs-53996", 158, 2193),
    ("cs-52f0c", 3, 575),
    ("cs-52f0c", 3, 575),
    ("cs-52f0c", 8, 576),
    ("bbcs-01", 17, 2292),
    ("bbcs-01", 93, 2299),
    ("bbcs-01", 146, 2303),
)


@dataclass
class W2Source:
    texts: dict
    programs: dict

    def continuation(self, owner):
        text = self.texts.get(owner.get("text"))
        if text is None or "{W2}" not in text:
            return None, None
        program, instruction = owner.get("program"), owner.get("instruction")
        if not isinstance(instruction, (int, float)) or int(instruction) != instruction:
            return None, None
        instruction = int(instruction)
        # Every selected W2 in these nonterminal texts has remaining text. The
        # accepted 575 occurrences therefore resume text at the same producer.
        if not text.endswith("{W2}"):
            return [], dict(Program=program, Instruction=instruction)
        instruction += 1
        expected = []
        details = {
            "open-portrait": "OpenPortrait",
            "wait-view": "WaitForView",
            "text-cursor": "SetTextCursor",
            "show-text": "ShowText",
            "jump": "JumpProgram",
            "set-flag": "WriteFlag",
            "face": "SetEntityFacing",
        }
        # This is a bounded source continuation for the admitted cohort, not a
        # second script interpreter. Unhandled call/return/branch paths are Unknown.
        for _ in range(16):
            instructions = self.programs.get(program, {}).get("instructions", [])
            if not isinstance(instruction, int) or not 0 <= instruction < len(instructions):
                return None, None
            op = instructions[instruction]
            if op.get("op") not in details:
                return None, None
            cursor = dict(Program=program, Instruction=instruction)
            expected.append(dict(Program=cursor, Detail=details[op["op"]]))
            if op["op"] in ("wait-view", "show-text", "face"):
                return expected, cursor
            if op["op"] == "jump":
                program, instruction = op["target"]["program"], op["target"]["instruction"]
            else:
                instruction += 1
        return None, None


def load_source(source_root, checks):
    texts = {}
    continuation_programs = {}
    try:
        root = Path(source_root) if source_root is not None else None
        if root is None:
            raise ValueError("source unavailable")
        root = root.resolve() if root.is_absolute() else repo_path(root)
        pin = subprocess.check_output(
            ["git", "-C", str(root), "rev-parse", "HEAD"], text=True
        ).strip()
        checks.check("original source pin", pin == UPSTREAM)
        script = subprocess.check_output(
            [
                "git",
                "-C",
                str(root),
                "show",
                f"{UPSTREAM}:disasm/data/scripting/text/gamescript.txt",
            ],
            text=True,
            encoding="utf-8",
        )
        texts = {
            int(line[:4], 16): line[5:]
            for line in script.splitlines()
            if re.match(r"^[0-9A-Fa-f]{4}=", line)
        }
        clean = subprocess.run(
            ["git", "-C", str(root), "diff", "--quiet", UPSTREAM, "--", "disasm"], check=False
        ).returncode
        checks.check("pinned clean continuation source", pin == UPSTREAM and clean == 0)
        if pin == UPSTREAM and clean == 0:
            from sf2tool.remake_exploration_content import OriginalPrograms

            compiler = OriginalPrograms(
                {"resources": {"standaloneScriptPrograms": [], "initSourcePrograms": []}}, root
            )
            for path in (
                "data/maps/entries/map03/mapsetups/scripts_1.asm",
                "data/maps/entries/map03/mapsetups/s2_entityevents.asm",
                "data/maps/entries/map03/mapsetups/s3_zoneevents.asm",
                "data/battles/entries/battle01/cs_beforebattle.asm",
            ):
                compiler.register_file("disasm/" + path)
            for symbol in (
                "cs_5145C",
                "Map3_EntityEvent0",
                "Map3_EntityEvent15",
                "byte_50E96",
                "cs_5149A",
                "bbcs_01",
            ):
                compiler.compile(symbol)
            continuation_programs = {p["id"]: p for p in compiler.programs.values()}
    except (OSError, ValueError, subprocess.SubprocessError):
        checks.check("pinned original text unavailable", None)

    return W2Source(texts, continuation_programs)
