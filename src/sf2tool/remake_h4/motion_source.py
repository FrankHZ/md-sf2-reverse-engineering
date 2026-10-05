"""Selected world identity and pinned source compiler admission."""

import subprocess

from sf2tool.paths import repo_path
from sf2tool.remake_h4_reference import ROM, UPSTREAM


def motion_source(selection, source_root, read, common):
    world, programs, compiler = {}, {}, None
    tracked_source = set()
    try:
        if not selection:
            raise ValueError("no selected world")
        world_path, _, receipt_path = selection[:3]
        world_path = world_path.resolve() if world_path.is_absolute() else repo_path(world_path)
        receipt_path = (
            receipt_path.resolve() if receipt_path.is_absolute() else repo_path(receipt_path)
        )
        document, receipt = read(world_path), read(receipt_path)
        world = document["world"]
        programs = {p["id"]: p for p in world["programs"]}
        common(
            "selected original identity",
            document["provenance"]["commit"] == UPSTREAM
            and document["provenance"]["romSha256"] == ROM,
        )
        common(
            "same-run selected world",
            repo_path(receipt["selectedInputs"]["SF2_PRIVATE_EXPLORATION_CONTENT"]).resolve()
            == world_path,
        )
    except (KeyError, OSError, ValueError):
        common("selected world operand absent", None)
    try:
        if source_root is None:
            raise ValueError("no original source")
        source_root = source_root.resolve() if source_root.is_absolute() else repo_path(source_root)
        pin = subprocess.check_output(
            ["git", "-C", str(source_root), "rev-parse", "HEAD"], text=True
        ).strip()
        common("original source pin", pin == UPSTREAM)
        clean = subprocess.run(
            ["git", "-C", str(source_root), "diff", "--quiet", UPSTREAM, "--", "disasm"],
            check=False,
        ).returncode
        common("original compiler reads pinned tracked source", clean == 0)
        tracked_source = set(
            subprocess.check_output(
                ["git", "-C", str(source_root), "ls-tree", "-r", "--name-only", UPSTREAM], text=True
            ).splitlines()
        )
        from sf2tool.remake_exploration_content import OriginalPrograms

        compiler = OriginalPrograms(
            {"resources": {"standaloneScriptPrograms": [], "initSourcePrograms": []}}, source_root
        )
        for p in programs.values():
            source = p.get("source", "")
            if source.startswith("disasm/") and ":" in source:
                path = source.rsplit(":", 1)[0]
                if path in tracked_source:
                    compiler.register_file(path)
    except (KeyError, OSError, ValueError, subprocess.CalledProcessError):
        common("source lowering operand absent", None)
        compiler = None
    return programs, compiler, tracked_source
