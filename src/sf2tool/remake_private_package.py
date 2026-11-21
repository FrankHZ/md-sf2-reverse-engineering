"""Copy or verify a closed, already-prepared private campaign package without the ROM."""

from __future__ import annotations

import argparse
import base64
import filecmp
import hashlib
import json
import os
import shutil
import subprocess
import wave
from pathlib import Path
from tempfile import TemporaryDirectory

from sf2tool.paths import repo_path
from sf2tool.private_inputs import SHARED_INPUT_ROOT_ENV
from sf2tool.remake_asset_build import ACCEPTED_UPSTREAM_COMMIT
from sf2tool.remake_command_menu import world_dependencies
from sf2tool.remake_exploration_content import _selected_growth
from sf2tool.remake_exploration_programs import OriginalPrograms
from sf2tool.remake_exploration_scene import ally_sprites

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


def installed_png(root, name):
    path = (root / name).resolve(strict=True)
    if not path.is_relative_to(root.resolve()):
        raise ValueError("field raster escapes the prepared family")
    data = path.read_bytes()
    if data[:8] != b"\x89PNG\r\n\x1a\n":
        raise ValueError("invalid prepared field PNG")
    return dict(
        width=int.from_bytes(data[16:20], "big"),
        height=int.from_bytes(data[20:24], "big"),
        format="png",
        data=base64.b64encode(data).decode(),
        sha256=hashlib.sha256(data).hexdigest().upper(),
    )


def bind_pldn_progression(package: dict, upstream: Path, members: set[int]) -> None:
    """Add gameplay metadata for the PLDN appearances already selected for this package."""
    commit = subprocess.check_output(
        ["git", "-C", str(upstream), "rev-parse", "HEAD"], text=True
    ).strip()
    if commit != ACCEPTED_UPSTREAM_COMMIT or package["provenance"]["commit"] != commit:
        raise ValueError("PLDN metadata needs the pinned upstream revision")

    selection = read_json(repo_path("remake/content/private/pre-voyage-opening.json"))
    selected = [
        row
        for row in selection["promotionClasses"]
        if row["member"] in members and row["class"] == "PLDN"
    ]
    if {row["member"] for row in selected} != members:
        raise ValueError("mounted PLDN member differs from campaign selection")
    compiler = OriginalPrograms(
        {"resources": {"standaloneScriptPrograms": [], "initSourcePrograms": []}},
        upstream,
        promotion_classes=tuple(selected),
    )
    # Only these parsers' actual dependencies belong to the existing world provenance.
    compiler.sources = {"disasm/sf2enums.asm"}
    variants = {
        row["character"]: row for row in ally_sprites(compiler) if row["character"] in members
    }
    additions = _selected_growth(compiler, selected)

    world = package["world"]
    sprites = {row["sprite"] for row in world["presentation"]["sprites"]}
    if any(row["promotionSprite"] not in sprites for row in variants.values()):
        raise ValueError("PLDN progression requires its mounted walking sprites")
    for row in additions:
        key = row["actor"], row["classId"]
        previous = next((r for r in world["growth"] if (r["actor"], r["classId"]) == key), None)
        if previous is None:
            world["growth"].append(row)
        elif previous != row:
            raise ValueError(f"conflicting PLDN growth: {key}")

    def bind_population(population):
        if population is None:
            return
        for appearance in population["allySprites"]:
            if (variant := variants.get(appearance["character"])) is None:
                continue
            if appearance["sprite"] != variant["sprite"]:
                raise ValueError("PLDN variant differs from the mounted base character")
            for field in ("promotionClass", "promotionSprite"):
                if appearance.setdefault(field, variant[field]) != variant[field]:
                    raise ValueError("conflicting promotion sprite")

    for row in world["maps"]:
        bind_population(row.get("population"))
        for setup in row.get("setups") or ():
            bind_population(setup.get("population"))
    for program in world["programs"]:
        for instruction in program["instructions"]:
            bind_population(instruction.get("population"))

    sources = package["provenance"]["sources"]
    existing = {row["path"] for row in sources}
    for name in sorted(compiler.sources):
        raw = (upstream / name).read_bytes()
        pinned = subprocess.check_output(["git", "-C", str(upstream), "show", f"{commit}:{name}"])
        if raw.replace(b"\r\n", b"\n") != pinned.replace(b"\r\n", b"\n"):
            raise ValueError("selected pinned source has local changes: " + name)
        if name not in existing:
            sources.append({"path": name, "sha256": hashlib.sha256(raw).hexdigest().upper()})


def mount(root, base, destination, families, commands, *, upstream=None):
    """Mount first-consumer operands into a new closed package; needs no production inputs."""
    permitted = {"walking", "icons", "sfx", "music", "pldn"}
    selected = permitted if "all" in families else set(families)
    parts = (
        ("pldn-party", "pldn-eric")
        if "pldn" in selected
        else tuple(part for part in ("pldn-party", "pldn-eric") if part in selected)
    )
    if not selected <= (permitted | {"pldn-party", "pldn-eric"}):
        raise ValueError("select walking, icons, sfx, music or pldn for this consumer")
    destination = private_output(destination)
    if destination.exists() or any(destination.is_relative_to(p.resolve()) for p in (root, base)):
        raise ValueError("select a fresh output outside input roots")
    destination.parent.mkdir(parents=True, exist_ok=True)
    files = package_files(
        base / "world.json",
        base / "main-scenes.json",
        base / "entry/entry.json",
        base / "opening-inputs",
    )
    with TemporaryDirectory(dir=destination.parent) as temp:
        dest = Path(temp) / "package"
        for rel, source in files.items():
            target = dest / rel
            target.parent.mkdir(parents=True, exist_ok=True)
            shutil.copyfile(source, target)
        world = read_json(dest / "world.json")
        presentation = world["world"]["presentation"]
        scene_path = dest / "inputs/battle26/battle-scenes.json"
        if scene_path.relative_to(dest) not in files:
            raise ValueError("selected package has no expected Battle26 consumer")
        scene = read_json(scene_path)
        if scene["encounter"] != "battle-26":
            raise ValueError("Battle26 selection differs")
        if "icons" in selected:
            images = read_json(root / "icons/identity-icons.json")
            for i in (7, 8, 9):
                for state in ("normal", "selected"):
                    name = f"identity-icon.spell.{i}.{state}"
                    for target in (presentation["identityIcons"], scene["rasters"]):
                        if target.setdefault(name, images[name]) != images[name]:
                            raise ValueError(f"conflicting mounted {name}")
        if "walking" in selected:
            sprites = read_json(root / "walking/index.json")["records"]["sprites"]
            for i in (6, 28, 34):
                row = dict(sprites[i])
                row["directions"] = [installed_png(root / "walking", p) for p in row["directions"]]
                existing = next((r for r in presentation["sprites"] if r["sprite"] == i), None)
                if existing is None:
                    presentation["sprites"].append(row)
                elif existing != row:
                    raise ValueError(f"conflicting field sprite {i}")
        promoted_members = set()
        for part in parts:
            additions = read_json(root / part / "scene-additions.json")
            # A legacy weapon ID may select a different palette in another document.
            for key, raster in additions["rasters"].items():
                target = key
                if key in scene["rasters"] and scene["rasters"][key] != raster:
                    target = part + "/" + key
                    for actor in additions["actors"]:
                        for field in ("frames", "weaponFrames"):
                            actor[field] = [target if n == key else n for n in actor[field]]
                if scene["rasters"].setdefault(target, raster) != raster:
                    raise ValueError(f"conflicting PLDN raster {target}")
            for actor in additions["actors"]:
                promoted_members.update(actor["members"])
                if actor not in scene["actors"]:
                    scene["actors"].append(actor)
        if upstream is not None and promoted_members:
            bind_pldn_progression(world, upstream, promoted_members)
        for command in commands or (25, 87, 99):
            family = "music" if command <= 64 else "sfx"
            if family not in selected:
                continue
            directory = root / family / str(command)
            if not directory.is_dir():
                if any(row["command"] == command for row in presentation["audio"]):
                    continue
                entry_audio = read_json(base / "entry/entry.json")["presentation"]["audio"]
                available = [r for r in entry_audio if r["command"] == command]
                if available:
                    presentation["audio"].extend(available)
                    continue
            entry = read_json(directory / "candidate.json")["asset"]
            runtime, timer = entry["runtime"], entry["timerB"]
            leaf = f"command-{command:02x}-timer-{timer:02x}.wav"
            wav = directory / leaf
            if (
                entry["command"] != command
                or hashlib.sha256(wav.read_bytes()).hexdigest().upper() != runtime["sha256"]
            ):
                raise ValueError(f"invalid prepared audio {command}")
            with wave.open(str(wav), "rb") as sound:
                pcm = sound.readframes(sound.getnframes())
            row = {
                k: runtime[k]
                for k in ("sampleRate", "channels", "sampleFrames", "loopBegin", "loopEnd")
            }
            row.update(
                command=command,
                timerB=timer,
                cue=entry["cue"],
                runtimeWav=True,
                sha256=hashlib.sha256(pcm).hexdigest().upper(),
            )
            target = dest / f"audio-{command:02x}-{timer:02x}" / leaf
            target.parent.mkdir(exist_ok=True)
            if target.exists() and target.read_bytes() != wav.read_bytes():
                raise ValueError(f"conflicting WAV {command}")
            shutil.copyfile(wav, target)
            existing = next(
                (
                    r
                    for r in presentation["audio"]
                    if r["command"] == command and (command <= 64 or r["timerB"] == timer)
                ),
                None,
            )
            if existing is None:
                presentation["audio"].append(row)
            elif existing != row:
                raise ValueError(f"conflicting audio binding {command}")
        scene_path.write_text(json.dumps(scene, separators=(",", ":")), encoding="utf-8")
        (dest / "world.json").write_text(json.dumps(world, separators=(",", ":")), encoding="utf-8")
        package_files(
            dest / "world.json",
            dest / "main-scenes.json",
            dest / "entry/entry.json",
            dest / "opening-inputs",
        )
        dest.rename(destination)


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
