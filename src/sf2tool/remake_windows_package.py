"""Build a private local Windows candidate from already prepared runtime inputs."""

from __future__ import annotations

import argparse
import json
import os
import shutil
import subprocess
import time
import zipfile
from pathlib import Path

from sf2tool.dotnet_environment import shared_dotnet_environment
from sf2tool.paths import REPO_ROOT
from sf2tool.remake_command_menu import world_dependencies
from sf2tool.remake_media_selection import (
    DEFAULT_SELECTION,
    package_arguments,
    read_selection,
    shared_root,
)
from sf2tool.remake_private_package import OPENING_INPUTS, package_files, private_output, verify


def relative_file(root: Path, name: str) -> Path:
    path = (root / name).resolve(strict=True)
    if (
        Path(name).is_absolute()
        or ".." in Path(name).parts
        or not path.is_relative_to(root.resolve())
        or not path.is_file()
    ):
        raise ValueError("runtime dependency must be a contained file")
    return path


def inputs(campaign: Path, entry: Path, opening: Path, catalog: Path) -> dict[Path, Path]:
    files = package_files(
        campaign / "world.json", campaign / "main-scenes.json", entry, campaign / "opening-inputs"
    )
    files[Path("entry/entry.zh-CN.json")] = entry.with_name("entry.zh-CN.json")
    files[Path("world.zh-CN.json")] = catalog
    selection = json.loads(opening.read_text(encoding="utf-8"))
    base = opening.parent
    selected = {"opening.json": opening}
    for key in ("world", "start", "party", "catalog"):
        name = selection[key]
        selected[name] = relative_file(base, name)
    world = selected[selection["world"]]
    for name in world_dependencies(json.loads(world.read_text(encoding="utf-8")), world):
        selected[(world.parent / name).relative_to(base).as_posix()] = relative_file(
            world.parent, name
        )
    for name in OPENING_INPUTS:
        relative = "opening-inputs/" + name + ".json"
        selected[relative] = relative_file(base, relative)
    files.update({Path("opening") / name: source for name, source in selected.items()})
    for source in files.values():
        if not source.is_file() or source.stat().st_size == 0:
            raise ValueError("missing or empty required runtime input")
    return files


def build(args: argparse.Namespace) -> None:
    started = time.monotonic()
    if any(not value.is_absolute() for value in vars(args).values()):
        raise ValueError("all input and output paths must be absolute")
    destination = private_output(args.output)
    zip_path = Path(str(destination) + ".zip")
    if destination.exists() or zip_path.exists():
        raise ValueError("candidate directory and ZIP must be fresh")
    log_root = REPO_ROOT / "local/windows-package" / (destination.name + "-logs")
    if log_root.exists():
        raise ValueError("candidate name already has retained build logs; choose a new name")
    files = inputs(
        args.campaign.resolve(),
        args.entry.resolve(),
        args.opening.resolve(),
        args.catalog.resolve(),
    )
    environment = shared_dotnet_environment(os.environ, REPO_ROOT)
    godot = Path(environment.get("GODOT_BIN", ""))
    if not godot.is_absolute() or not godot.is_file():
        raise ValueError("GODOT_BIN must select the owned Godot .NET editor")
    if godot.name.endswith("_console.exe"):
        godot = godot.with_name(godot.name.replace("_console.exe", ".exe"))
        if not godot.is_file():
            raise ValueError("owned Godot installation lacks its actual editor executable")
    template_names = ["windows_release_x86_64.exe", "windows_release_x86_64_console.exe"]
    with zipfile.ZipFile(args.templates) as archive:
        version = archive.read("templates/version.txt").decode().strip()
        if version != "4.7.2.stable.mono":
            raise ValueError("requires matching Godot 4.7.2 stable mono templates")
        template_bytes = sum(
            archive.getinfo("templates/" + name).file_size for name in template_names
        )
        input_bytes = sum(path.stat().st_size for path in files.values())
        # Candidate, incompressible ZIP, validation copy, templates plus build/cache headroom.
        budget = 3 * (input_bytes + 400 * 1024**2) + template_bytes + 2 * 1024**3
        if (
            shutil.disk_usage(destination.parent if destination.parent.exists() else REPO_ROOT).free
            < budget
        ):
            raise ValueError("insufficient free space for candidate, ZIP and validation budget")
        print(
            json.dumps(
                {
                    "inputFiles": len(files),
                    "inputBytes": input_bytes,
                    "templateBytes": template_bytes,
                    "peakDiskBudgetBytes": budget,
                }
            ),
            flush=True,
        )
        profile = log_root / "godot-profile"
        templates = profile / "Godot/export_templates" / version
        templates.mkdir(parents=True, exist_ok=True)
        for name in template_names:
            with (
                archive.open("templates/" + name) as source,
                (templates / name).open("wb") as target,
            ):
                shutil.copyfileobj(source, target)
        (templates / "version.txt").write_text(version, encoding="utf-8")
    environment["APPDATA"] = str(profile)
    destination.mkdir(parents=True)
    content = destination / "content"
    content.mkdir()
    for relative, source in files.items():
        target = content / relative
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(source, target)
    verify(files, content)
    # Godot publish rewrites RID-specific NuGet locks and creates import UIDs.
    # A disposable source copy keeps those export side effects out of the checkout.
    source_root = log_root / "source"
    names = (
        subprocess.check_output(["git", "ls-files", "-z", "--", "remake"], cwd=REPO_ROOT)
        .decode()
        .split("\0")
    )
    for name in names:
        if not name or not (REPO_ROOT / name).is_file():
            continue
        target = source_root / name
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(REPO_ROOT / name, target)
    project = source_root / "remake/game"

    def run(name: str, command: list[str]) -> None:
        with (log_root / name).open("wb") as log:
            result = subprocess.run(
                command,
                cwd=project,
                env=environment,
                stdout=log,
                stderr=subprocess.STDOUT,
                timeout=300,
            )
        if result.returncode:
            raise ValueError(
                f"{name} failed with exit {result.returncode}; "
                f"retain local/windows-package/{destination.name}-logs"
            )

    run("editor-version.log", [str(godot), "--version"])
    editor_version = (log_root / "editor-version.log").read_text(encoding="utf-8").strip()
    if editor_version != "4.7.2.stable.mono.official.ed1daf0bf":
        raise ValueError("GODOT_BIN differs from the owned Godot 4.7.2 mono revision")
    run(
        "restore.log",
        [environment["DOTNET_BIN"], "restore", "Sf2.Remake.Godot.csproj", "--locked-mode"],
    )
    run("import.log", [str(godot), "--headless", "--path", str(project), "--editor", "--import"])
    run(
        "export.log",
        [
            str(godot),
            "--headless",
            "--path",
            str(project),
            "--export-release",
            "Common Game Windows",
            str(destination / "Sf2Remake.exe"),
        ],
    )
    if (
        not (destination / "Sf2Remake.exe").is_file()
        or not (destination / "Sf2Remake.pck").is_file()
    ):
        raise ValueError("export did not produce the executable and separate PCK")
    # Portable debug symbols contain build paths and are not runtime dependencies.
    for symbols in destination.rglob("*.pdb"):
        symbols.unlink()
    inventory = [
        {"path": path.relative_to(destination).as_posix(), "bytes": path.stat().st_size}
        for path in sorted(destination.rglob("*"))
        if path.is_file()
    ]
    report = {
        "sourceCommit": subprocess.check_output(
            ["git", "rev-parse", "HEAD"], cwd=REPO_ROOT, text=True
        ).strip(),
        "sourceDirty": bool(
            subprocess.check_output(
                [
                    "git",
                    "status",
                    "--porcelain",
                    "--",
                    "remake",
                    "src/sf2tool/remake_windows_package.py",
                ],
                cwd=REPO_ROOT,
            )
        ),
        "templateVersion": version,
        "editorVersion": editor_version,
        "inputBytes": input_bytes,
        "files": inventory,
        "candidateBytes": sum(row["bytes"] for row in inventory),
        "buildSeconds": round(time.monotonic() - started, 2),
    }
    (destination / "candidate.json").write_text(
        json.dumps(report, indent=2) + "\n", encoding="utf-8"
    )
    shutil.make_archive(str(destination), "zip", destination)
    print(json.dumps({key: value for key, value in report.items() if key != "files"}), flush=True)
    print(
        json.dumps(
            {
                "zipBytes": zip_path.stat().st_size,
                "totalSeconds": round(time.monotonic() - started, 2),
            }
        ),
        flush=True,
    )


def runtime_arguments(selection, root, campaign=None, entry=None, opening=None, catalog=None):
    paths = dict(campaign=campaign, entry=entry, opening=opening, catalog=catalog)
    if any(value is None for value in paths.values()):
        defaults = package_arguments(read_selection(selection), shared_root(root))
        paths = {key: defaults[key] if value is None else value for key, value in paths.items()}
    return paths


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--selection", type=Path, default=DEFAULT_SELECTION)
    parser.add_argument("--shared-root", type=Path)
    for name in ("campaign", "entry", "opening", "catalog"):
        parser.add_argument("--" + name, type=Path, help="explicit override for this role")
    for name in ("templates", "output"):
        parser.add_argument("--" + name, type=Path, required=True)
    try:
        args = parser.parse_args()
        paths = runtime_arguments(
            args.selection, args.shared_root, args.campaign, args.entry, args.opening, args.catalog
        )
        build(argparse.Namespace(**paths, templates=args.templates, output=args.output))
    except (ValueError, OSError, KeyError, zipfile.BadZipFile, subprocess.TimeoutExpired) as error:
        parser.exit(1, f"Windows candidate failed: {error}\n")


if __name__ == "__main__":
    main()
