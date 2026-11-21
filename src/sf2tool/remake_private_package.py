"""Copy or verify a closed, already-prepared private campaign package without the ROM."""

from __future__ import annotations

import argparse
import filecmp
import json
import os
import shutil
from pathlib import Path
from tempfile import TemporaryDirectory

from sf2tool.paths import repo_path
from sf2tool.private_inputs import SHARED_INPUT_ROOT_ENV
from sf2tool.remake_command_menu import world_dependencies

OPENING_INPUTS = (
    "SF2_PRIVATE_BATTLE01_DATA",
    "SF2_PRIVATE_BATTLE01_SCENE",
    "SF2_PRIVATE_BATTLE01_TERRAIN",
    "SF2_PRIVATE_STATIC_DATA",
    "SF2_PRIVATE_ENEMY_DATA",
    "SF2_PRIVATE_ENEMY_GOLD",
)


def read_json(path: Path) -> dict:
    with path.open(encoding="utf-8-sig") as stream:
        return json.load(stream)


def private_output(path: Path) -> Path:
    """Select a child of worktree scratch or the configured stable derived root."""
    if not path.is_absolute():
        raise ValueError("output must be absolute")
    output = path.resolve()
    roots = [repo_path("local")]
    shared = Path(os.environ.get(SHARED_INPUT_ROOT_ENV, ""))
    if shared.is_absolute():
        derived = (shared / "derived").resolve()
        if derived.is_relative_to(shared.resolve()) and derived != shared.resolve():
            roots.append(derived)
    if not any(output != root and output.is_relative_to(root) for root in roots):
        raise ValueError("output must be under worktree local/ or registered shared derived/")
    return output


def package_files(world: Path, scene: Path, entry: Path, opening_inputs: Path) -> dict[Path, Path]:
    world = world.resolve(strict=True)
    document = read_json(world)
    files = {Path(name): world.parent / name for name in world_dependencies(document, world)}
    extras = {
        Path("world.json"): world,
        Path("main-scenes.json"): scene,
        Path("entry/entry.json"): entry,
    }
    for resource in read_json(entry)["resources"]:
        name = resource["file"]
        if Path(name).name != name or name != resource["name"] + ".png":
            raise ValueError("entry resource is not a named sibling PNG")
        path = (entry.parent / name).resolve(strict=True)
        if not path.is_relative_to(entry.parent.resolve()):
            raise ValueError("entry resource escapes input directory")
        extras[Path("entry") / name] = path
    extras.update(
        {
            Path("opening-inputs") / (name + ".json"): opening_inputs / (name + ".json")
            for name in OPENING_INPUTS
        }
    )
    if files.keys() & extras.keys():
        raise ValueError("world dependency collides with an external opening input")
    files.update(extras)
    for name, source in files.items():
        if not source.is_file() or not source.stat().st_size:
            raise ValueError(f"missing or empty input: {name.as_posix()}")
    return files


def verify(files: dict[Path, Path], destination: Path) -> None:
    actual = {path.relative_to(destination) for path in destination.rglob("*") if path.is_file()}
    if files.keys() != actual:
        raise ValueError("destination file set differs from package closure")
    for relative, source in files.items():
        target = destination / relative
        if not target.resolve().is_relative_to(destination):
            raise ValueError(f"destination file escapes package: {relative.as_posix()}")
        if not filecmp.cmp(source, target, shallow=False):
            raise ValueError(f"destination differs: {relative.as_posix()}")


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ("world", "scene", "entry", "opening-inputs", "destination"):
        parser.add_argument(f"--{name}", type=Path, required=True)
    parser.add_argument(
        "--check", action="store_true", help="Compare existing output byte for byte"
    )
    args = parser.parse_args()
    destination = private_output(args.destination)
    if args.check != destination.is_dir() or (destination.exists() and not args.check):
        parser.error("select an existing directory for --check or a fresh destination for copying")
    files = package_files(args.world, args.scene, args.entry, args.opening_inputs)
    size = sum(path.stat().st_size for path in files.values())
    if args.check:
        verify(files, destination)
    else:
        destination.parent.mkdir(parents=True, exist_ok=True)
        if shutil.disk_usage(destination.parent).free < size:
            raise OSError("insufficient free space for prepared package")
        with TemporaryDirectory(prefix=".sf2-package-", dir=destination.parent) as staging:
            candidate = Path(staging) / "package"
            for relative, source in files.items():
                target = candidate / relative
                target.parent.mkdir(parents=True, exist_ok=True)
                shutil.copyfile(source, target)
            verify(files, candidate)
            candidate.rename(destination)
    print(json.dumps({"files": len(files), "bytes": size, "checked": args.check}))


if __name__ == "__main__":
    main()
