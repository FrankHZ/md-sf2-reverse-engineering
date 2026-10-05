"""Check selected mouth/view reader operands and pinned no-delay source path."""

import re
import subprocess
from pathlib import Path

from sf2tool.remake_h4_reference import UPSTREAM


def compare_source(selected, source_root, checks):
    check, match = checks.check, checks.match
    for row in selected[1:3]:
        check(
            "view default branch operands",
            match(
                dict(
                    facts=dict(
                        registers=dict(D7=24),
                        values=dict(
                            VIEW_TARGET_ENTITY=0,
                            VIEW_SCROLLING_PLANES_BITFIELD=0,
                            MAP_AREA_LAYER_TYPE=0,
                            PLAYER_1_INPUT=4,
                            CURRENT_PLAYER_INPUT=4,
                        ),
                    )
                ),
                row,
            ),
        )
    check(
        "glyph input reader",
        match(
            dict(
                facts=dict(values=dict(PLAYER_1_INPUT=0, CURRENT_PLAYER_INPUT=0)),
                state=dict(typewriting=1),
            ),
            selected[3],
        ),
    )
    try:
        root = Path(source_root)
        pin = subprocess.check_output(
            ["git", "-C", str(root), "rev-parse", "HEAD"], text=True
        ).strip()
        dirty = subprocess.check_output(
            ["git", "-C", str(root), "diff", "HEAD", "--", "disasm"], text=True
        )
        check("pinned clean original source", pin == UPSTREAM and not dirty)
        text = (
            (root / "disasm/data/scripting/text/gamescript.txt")
            .read_text(encoding="utf-8")
            .splitlines()
        )
        check(
            "selected opening needs no delay token",
            all(not re.search(r"\{(?:D[123]?|DELAY[^}]*)\}", text[i]) for i in (510, 511, 483)),
        )
    except (OSError, TypeError, IndexError, subprocess.CalledProcessError):
        check("pinned original source and token path", None)
