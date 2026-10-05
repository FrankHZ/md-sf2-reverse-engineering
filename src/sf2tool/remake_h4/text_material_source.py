"""Read-only pinned text, font and imported selection admission."""

import hashlib
import re
import subprocess

from sf2tool.paths import repo_path
from sf2tool.remake_h4_reference import ROM, UPSTREAM


def text_material_source(selection, source_root, read, check):
    w, texts, names, enemy_names = {}, {}, [], []
    ascii_map = advances = None
    try:
        world_path, scene_path, process_path = selection[:3]
        world_path, scene_path, process_path = (
            p.resolve() if p.is_absolute() else repo_path(p)
            for p in (world_path, scene_path, process_path)
        )
        world, scene, process = read(world_path), read(scene_path), read(process_path)
        w = world["world"]
        check(
            "world original identity",
            world["provenance"]["commit"] == UPSTREAM and world["provenance"]["romSha256"] == ROM,
        )
        selected = process.get("selectedInputs", {})
        check(
            "same-run material selection",
            all(
                repo_path(selected[k]).resolve() == p.resolve()
                for k, p in (
                    ("SF2_PRIVATE_EXPLORATION_CONTENT", world_path),
                    ("SF2_PRIVATE_BATTLE_SCENE_CONTENT", scene_path),
                )
            ),
        )
        source_root = source_root.resolve() if source_root.is_absolute() else repo_path(source_root)
        pin = subprocess.check_output(
            ["git", "-C", str(source_root), "rev-parse", "HEAD"], text=True
        ).strip()
        check("original source pin", pin == UPSTREAM)

        def source(path):
            return subprocess.check_output(
                ["git", "-C", str(source_root), "show", f"{UPSTREAM}:disasm/{path}"]
            )

        from sf2tool.h2.variable_width_font import _glyph_metadata, _parse_ascii_map

        texts = {
            int(line[:4], 16): line[5:]
            for line in source("data/scripting/text/gamescript.txt").decode("utf-8").splitlines()
            if re.match(r"^[0-9A-Fa-f]{4}=.+", line)
        }
        check("full source text import", {t["id"]: t["text"] for t in w["texts"]} == texts)
        ascii_map = _parse_ascii_map(
            source("data/scripting/text/asciitotextsymbolmap.asm").decode("utf-8")
        )
        # Extracted private font bytes are ignored upstream. Validate against the
        # accepted source/ROM parity fixture before deriving their advances.
        try:
            font_fixture = read(repo_path("tests/fixtures/h2/variable-width-font-static-v1.json"))
            font_bytes = (
                source_root / "disasm/data/graphics/tech/fonts/variablewidthfont.bin"
            ).read_bytes()
            check(
                "original font bytes",
                font_fixture["upstreamCommit"] == UPSTREAM
                and font_fixture["romSha256"] == ROM
                and hashlib.sha256(font_bytes).hexdigest().upper()
                == font_fixture["fontHashes"]["fontSha256"],
            )
            advances = [g["advancePixels"] for g in _glyph_metadata(font_bytes, 0)]
        except (KeyError, ValueError, OSError):
            check("missing original font operand", None)
        names = re.findall(r'"([^"]*)"', source("data/stats/allies/allynames.asm").decode("utf-8"))
        enemies = re.findall(
            r'"([^"]*)"', source("data/stats/enemies/enemynames.asm").decode("utf-8")
        )
        check(
            "source symbol map and advances",
            None
            if ascii_map is None or advances is None
            else w["textFont"]["asciiToSymbol"] == ascii_map
            and w["textFont"]["advances"] == advances,
        )
        check("member names", w["memberNames"] == scene["memberNames"] == names)
        check(
            "scene text import",
            all(text == texts[int(tid)] for tid, text in scene["texts"].items()),
        )
        check("admitted enemy name", "GIZMO" in enemies)
        battle = read(repo_path(selected["SF2_PRIVATE_BATTLE01_DATA"]))
        battle_source = source("data/battles/spritesets/spriteset01.asm")
        enemy_names = re.findall(
            r"^\s*enemyCombatant\s+(\w+),", battle_source.decode("utf-8"), re.MULTILINE
        )
        selected_enemies = [e for e in battle["entities"] if e["kind"] == "enemy"]
        check(
            "original Battle01 enemy selectors",
            battle["provenance"]["commit"] == UPSTREAM
            and battle["provenance"]["sourcePath"] == "data/battles/spritesets/spriteset01.asm"
            # This admitted extractor recorded its Windows CRLF checkout bytes;
            # reproduce that representation from the pinned Git LF object.
            and battle["provenance"]["sourceSha256"]
            == hashlib.sha256(battle_source.replace(b"\n", b"\r\n")).hexdigest().upper()
            and [e["identityExpression"] for e in selected_enemies] == enemy_names
            and all(n == "GIZMO" for n in enemy_names),
        )
    except (KeyError, IndexError, ValueError, OSError, subprocess.CalledProcessError):
        check("missing source/material operand", None)
    return w, texts, names, enemy_names, ascii_map, advances
