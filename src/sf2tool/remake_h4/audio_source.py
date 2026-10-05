"""Classify active sound slots from the verified pinned driver."""

import re
import subprocess

from sf2tool.paths import repo_path
from sf2tool.remake_h4_reference import UPSTREAM


def classify_slots(source_root, check):
    slots, types = {}, {}
    try:
        from sf2tool.h2.sound_data import (
            SFX_TYPE_1_SLOTS,
            SFX_TYPE_2_SLOTS,
            _sfx_source_headers,
        )
        from sf2tool.source_text import read_upstream_text

        if source_root is None:
            raise ValueError("no pinned original source")
        source_root = source_root.resolve() if source_root.is_absolute() else repo_path(source_root)
        pin = subprocess.check_output(
            ["git", "-C", str(source_root), "rev-parse", "HEAD"], text=True
        ).strip()
        clean = subprocess.run(
            ["git", "-C", str(source_root), "diff", "--quiet", UPSTREAM, "--", "disasm"],
            check=False,
        ).returncode
        check("pinned original audio rules", pin == UPSTREAM and clean == 0)
        driver = read_upstream_text(source_root / "disasm/code/common/tech/sound/sounddriver.asm")
        for label, header in _sfx_source_headers(driver).items():
            command = 64 + int(label[4:], 16)
            active = set()
            for slot, target in zip(
                SFX_TYPE_1_SLOTS if header["type"] == 1 else SFX_TYPE_2_SLOTS,
                header["targets"],
                strict=True,
            ):
                first = re.search(r"^" + target + r":\s*db\s+([^\s,;]+)", driver, re.M)
                if first is None:
                    raise ValueError("SFX target operand absent")
                if first[1] != "0FFh":
                    active.add(slot)
            slots[command], types[command] = active, header["type"]
    except (OSError, ValueError, subprocess.CalledProcessError):
        check("original sound slot classification absent", None)
        return None

    return slots, types
