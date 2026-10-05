"""Selected world admission and pinned compiler/source route lifetime."""

import subprocess

from sf2tool.paths import repo_path
from sf2tool.remake_h4_reference import ROM, UPSTREAM


def operation_flow_source(selection, source_root, read, common):
    try:
        if not selection or source_root is None:
            raise ValueError("missing source selection")
        world_path, _, receipt_path = selection[:3]
        world_path = world_path if world_path.is_absolute() else repo_path(world_path)
        receipt_path = receipt_path if receipt_path.is_absolute() else repo_path(receipt_path)
        source_root = source_root if source_root.is_absolute() else repo_path(source_root)
        document, receipt = read(world_path), read(receipt_path)
        world = document["world"]
        programs = {p["id"]: p for p in world["programs"]}
        maps = {m["id"]: m for m in world["maps"]}
        common(
            "selected original provenance",
            document["provenance"]["commit"] == UPSTREAM
            and document["provenance"]["romSha256"] == ROM,
        )
        common(
            "same-run world selection",
            repo_path(receipt["selectedInputs"]["SF2_PRIVATE_EXPLORATION_CONTENT"]).resolve()
            == world_path.resolve(),
        )
        common(
            "pinned clean source",
            subprocess.check_output(
                ["git", "-C", str(source_root), "rev-parse", "HEAD"], text=True
            ).strip()
            == UPSTREAM
            and subprocess.run(
                ["git", "-C", str(source_root), "diff", "--quiet", UPSTREAM, "--", "disasm"],
                check=False,
            ).returncode
            == 0,
        )
        tracked = set(
            subprocess.check_output(
                ["git", "-C", str(source_root), "ls-tree", "-r", "--name-only", UPSTREAM], text=True
            ).splitlines()
        )
        from sf2tool.h2.map_content import _encode_source
        from sf2tool.h2.map_import import _decode_source_table
        from sf2tool.h2.map_setup import _parse_routes
        from sf2tool.remake_exploration_content import OriginalPrograms, _tokens

        compiler = OriginalPrograms(
            {"resources": {"standaloneScriptPrograms": [], "initSourcePrograms": []}},
            source_root,
            scene_maps=[57],
        )
        for p in programs.values():
            path = p.get("source", "").rsplit(":", 1)[0]
            if path in tracked:
                compiler.register_file(path)
        routes = {
            "map-" + str(row["map"]): row
            for row in _parse_routes(
                (source_root / "disasm/data/maps/mapsetups.asm").read_text(encoding="utf-8")
            )
        }
    except (KeyError, OSError, ValueError, subprocess.CalledProcessError):
        common("source/selection operands absent", None)
        return None
    return (
        world,
        programs,
        maps,
        source_root,
        tracked,
        compiler,
        routes,
        _encode_source,
        _decode_source_table,
        _tokens,
    )
