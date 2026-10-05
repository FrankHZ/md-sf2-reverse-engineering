"""Pinned animation/text source and accepted material selectors."""

import re
import subprocess
from pathlib import Path

from sf2tool.paths import repo_path
from sf2tool.remake_h4.scene_checks import absent, picked
from sf2tool.remake_h4_reference import UPSTREAM, require


def source_sequences(source_root, materials):
    """Selected source sequence bytes, independently of the mounted frame observations."""
    root = Path(source_root)
    root = root if root.is_absolute() else repo_path(root)
    require(
        subprocess.check_output(["git", "-C", str(root), "rev-parse", "HEAD"], text=True).strip()
        == UPSTREAM,
        "scene source pin",
    )
    require(
        subprocess.run(
            ["git", "-C", str(root), "diff", "--quiet", UPSTREAM, "--", "disasm"], check=False
        ).returncode
        == 0,
        "scene source modifications",
    )
    disasm = root / "disasm"
    texts = {
        str(int(k, 16)): v
        for k, v in re.findall(
            r"^([0-9A-F]{4})=(.*)$",
            (disasm / "data/scripting/text/gamescript.txt").read_text(encoding="utf-8"),
            re.M,
        )
    }
    sequences = {}
    for visual in materials.get("actors", []):
        side, sprite = visual["side"], visual["sprite"]
        for purpose in visual["sequences"]:
            # This accepted cohort has ordinary SDMN/PRST/KNTE weapons, not a spear.
            index = sprite + ((40 if side == "ally" else 60) if purpose == "dodge" else 0)
            folder = "allies" if side == "ally" else "enemies"
            path = (
                disasm
                / f"data/graphics/battles/battlesprites/{folder}/animations"
                / f"{side}animation{index:03}.bin"
            )
            data = path.read_bytes()
            size = 8 if side == "ally" else 4
            require(len(data) == size * (data[0] + 1), "scene animation source length")

            def signed(n):
                return n - 256 if n >= 128 else n

            def weapon(raw):
                return dict(frame=raw[0], layer=raw[1], x=signed(raw[2]), y=signed(raw[3]))

            frames = []
            for offset in range(size, len(data), size):
                raw = data[offset : offset + size]
                frames.append(
                    dict(
                        frame=raw[0],
                        ticks=raw[1],
                        x=signed(raw[2]),
                        y=signed(raw[3]),
                        weapon=weapon(raw[4:]) if side == "ally" else None,
                    )
                )
            sequences[(side, sprite, purpose)] = dict(
                index=index,
                trigger=data[1],
                spell=data[2],
                terminate=data[3],
                idleWeapon=weapon(data[4:8]) if side == "ally" else None,
                frames=frames,
            )
    return sequences, texts


class SceneMaterials:
    """Read-only accepted resource identities and source text substitutions."""

    def __init__(self, materials, texts):
        self.materials = materials
        self.texts = texts
        self.visuals = {(v["side"], v["sprite"]): v for v in materials.get("actors", [])}

    def visual(self, actor):
        if not isinstance(actor, str):
            return {}
        return (
            self.visuals.get(("ally", int(actor[5:])), {})
            if actor.startswith("ally-")
            else self.visuals.get(("enemy", 22), {})
        )

    def name(self, actor):
        if isinstance(actor, str) and actor.startswith("ally-"):
            return picked(self.materials.get("memberNames", []), int(actor[5:]))
        return "GIZMO"

    def render(self, text_id, who=None, amount=0, spell=""):
        text = self.texts.get(str(text_id))
        if text is None or amount is None or self.name(who) is absent:
            return absent
        return re.sub(
            r"\{D[0-9]+\}",
            "",
            text.replace("{NAME}", self.name(who))
            .replace("{#}", str(int(amount)))
            .replace("{SPELL}", spell)
            .replace("{N}", "\n"),
        )
