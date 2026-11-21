"""Store existing prepared resources once and assemble ordinary private packages offline."""

from __future__ import annotations

import argparse
import base64
import csv
import hashlib
import json
import re
import shutil
from pathlib import Path
from tempfile import TemporaryDirectory

from sf2tool.remake_command_menu import write_json
from sf2tool.remake_private_package import package_files, private_output, read_json, verify

FAMILIES = (
    "maps",
    "walking",
    "portraits",
    "battle-bodies",
    "battle-weapons",
    "battle-terrain",
    "battle-effects",
    "ui",
    "entry",
    "music",
    "sfx",
)
REFERENCE = "$preparedAsset"
COLUMNS = ("family", "id", "document", "payload")
MIB = 1024 * 1024


def audio_family(command: int) -> str:
    return "music" if command < 65 else "sfx"


def scene_family(key: str) -> str:
    if re.match(r"(?:ally|enemy)\d", key):
        return "battle-bodies"
    if re.match(r"weapon\d", key):
        return "battle-weapons"
    if re.match(r"(?:ground|background)\d", key):
        return "battle-terrain"
    if key.startswith(("command-menu.", "battlefield-menu.", "identity-icon.", "battle-cursor.")):
        return "ui"
    return "battle-effects"


def payload_path(value: str) -> Path:
    # Internal binary blobs use the already declared raster/PCM identity.
    if not re.fullmatch(
        r"(?:[a-z-]+/[0-9a-f]{64}\.bin|entry/[a-z-]+\.png|"
        r"(?:music|sfx)/[0-9a-f]{2}-[0-9a-f]{2}\.wav|maps/map-\d+-[a-z]+\.json)",
        value,
    ):
        raise ValueError("invalid prepared asset reference")
    if value.split("/", 1)[0] not in FAMILIES:
        raise ValueError("invalid prepared asset family")
    return Path(value)


def copy_file(source: Path, target: Path) -> None:
    target.parent.mkdir(parents=True, exist_ok=True)
    shutil.copyfile(source, target)


class LibraryStaging:
    """Write the package's bindings and the requested families into a fresh staging directory."""

    def __init__(self, stage: Path, families: set[str]) -> None:
        self.stage = stage
        self.families = families
        self.rows: list[tuple[str, str, str, str]] = []

    def reference(self, family: str, identity: str, document: Path, path: str) -> Path | None:
        payload = payload_path(path)
        self.rows.append((family, identity, document.as_posix(), payload.as_posix()))
        return self.stage / "assets" / payload if family in self.families else None

    def embedded(self, row: dict, field: str, family: str, identity: str, document: Path) -> None:
        digest = row["sha256"].lower()
        path = f"{family}/{digest}.bin"
        target = self.reference(family, identity, document, path)
        if target is not None:
            data = base64.b64decode(row[field], validate=True)
            if hashlib.sha256(data).hexdigest() != digest:
                raise ValueError(f"prepared payload identity differs: {family}/{identity}")
            if not target.exists():
                target.parent.mkdir(parents=True, exist_ok=True)
                target.write_bytes(data)
            elif target.read_bytes() != data:
                raise ValueError(f"conflicting prepared payload: {family}/{identity}")
        row[field] = {REFERENCE: path}

    def external(
        self,
        files: dict[Path, Path],
        family: str,
        identity: str,
        document: Path,
        relative: Path,
        path: str,
    ) -> None:
        source = files.pop(relative)
        self.external_paths[relative.as_posix()] = path
        target = self.reference(family, identity, document, path)
        if target is not None:
            copy_file(source, target)

    def map_structure(self, row: dict, map_id: str, field: str, document: Path) -> None:
        path = f"maps/{map_id}-{field.lower()}.json"
        target = self.reference("maps", f"{map_id}:{field}", document, path)
        if target is not None:
            target.parent.mkdir(parents=True, exist_ok=True)
            with target.open("x", encoding="utf-8") as stream:
                json.dump(row[field], stream, ensure_ascii=False)
        row[field] = {REFERENCE: path}

    def presentation(self, presentation: dict, relative: Path, files: dict[Path, Path]) -> None:
        for row in presentation["maps"]:
            self.embedded(row["atlas"], "data", "maps", row["map"], relative)
            self.map_structure(row, row["map"], "blocks", relative)
        for row in presentation["sprites"]:
            for direction, strip in enumerate(row["directions"]):
                self.embedded(strip, "data", "walking", f"{row['sprite']}:{direction}", relative)
        for row in presentation["portraits"]:
            self.embedded(row["raster"], "data", "portraits", str(row["portrait"]), relative)
        for field in ("fieldMenu", "churchMenu", "itemMenu", "shopMenu", "identityIcons"):
            for identity, row in presentation.get(field, {}).items():
                key = identity if field == "identityIcons" else f"{field}.{identity}"
                self.embedded(row, "data", "ui", key, relative)
        for row in presentation.get("audio", []):
            command, timer = row["command"], row["timerB"]
            family = audio_family(command)
            key = f"{command}:{timer}"
            if "runtimeWav" in row:
                stem = f"{command:02x}-{timer:02x}"
                wav = Path(f"audio-{stem}/command-{command:02x}-timer-{timer:02x}.wav")
                self.external(files, family, key, relative, wav, f"{family}/{stem}.wav")
            else:
                self.embedded(row, "pcm16", family, key, relative)

    def stage_package(self, files: dict[Path, Path]) -> None:
        self.external_paths: dict[str, str] = {}
        base = self.stage / "base"
        documents = base / "documents"
        world = read_json(files.pop(Path("world.json")))
        scene_paths = {
            Path(row["battleScenes"]) for row in world["encounters"] if row["battleScenes"]
        }
        self.presentation(world["world"]["presentation"], Path("world.json"), files)
        for row in world["world"]["maps"]:
            for field in ("layout", "areas", "layoutEvents", "basePalette"):
                if field in row:
                    self.map_structure(row, row["id"], field, Path("world.json"))
        documents.mkdir(parents=True)
        write_json(documents / "world.json", world)
        del world

        for relative in sorted(scene_paths | {Path("main-scenes.json")}):
            scene = read_json(files.pop(relative))
            for identity, row in scene["rasters"].items():
                self.embedded(row, "data", scene_family(identity), identity, relative)
            target = documents / relative
            target.parent.mkdir(parents=True, exist_ok=True)
            write_json(target, scene)

        entry_relative = Path("entry/entry.json")
        entry = read_json(files.pop(entry_relative))
        for row in entry["resources"]:
            self.external(
                files,
                "entry",
                row["name"],
                entry_relative,
                Path("entry") / row["file"],
                f"entry/{row['file']}",
            )
        self.presentation(entry["presentation"], entry_relative, files)
        (documents / "entry").mkdir()
        write_json(documents / entry_relative, entry)
        for relative, source in files.items():
            copy_file(source, base / "files" / relative)
        write_json(base / "external.json", self.external_paths)
        with (base / "bindings.csv").open("w", encoding="utf-8", newline="") as stream:
            writer = csv.writer(stream)
            writer.writerow(COLUMNS)
            writer.writerows(sorted(self.rows))


def tree_files(root: Path) -> dict[Path, Path]:
    result = {path.relative_to(root): path for path in root.rglob("*") if path.is_file()}
    if any(not path.resolve().is_relative_to(root.resolve()) for path in result.values()):
        raise ValueError("stored file escapes its owning directory")
    return result


def asset_file(assets: Path, relative: Path) -> Path:
    path = (assets / relative).resolve(strict=True)
    if not path.is_relative_to(assets.resolve()):
        raise ValueError("stored payload escapes the asset directory")
    return path


def prepare(package: Path, library: Path, families: set[str], budget: int) -> dict:
    package = package.resolve(strict=True)
    if library.is_relative_to(package) or package.is_relative_to(library):
        raise ValueError("package and library require separate directories")
    files = package_files(
        package / "world.json",
        package / "main-scenes.json",
        package / "entry/entry.json",
        package / "opening-inputs",
    )
    source_bytes = sum(path.stat().st_size for path in files.values())
    library.parent.mkdir(parents=True, exist_ok=True)
    if 2 * source_bytes > min(budget, shutil.disk_usage(library.parent).free):
        raise ValueError("package exceeds available staging budget")
    with TemporaryDirectory(prefix=".sf2-library-", dir=library.parent) as temp:
        stage = Path(temp) / "library"
        LibraryStaging(stage, families).stage_package(files)
        if not library.exists():
            stage.rename(library)
        else:
            # A resumed family must describe the exact same input selection.
            verify(tree_files(stage / "base"), library / "base")
            for family in sorted(families):
                candidate = stage / "assets" / family
                if not candidate.is_dir():
                    continue
                target = library / "assets" / family
                if target.exists():
                    verify(tree_files(candidate), target)
                else:
                    target.parent.mkdir(parents=True, exist_ok=True)
                    candidate.rename(target)
    return status(library)


def bindings(library: Path) -> list[dict[str, str]]:
    with (library / "base/bindings.csv").open(encoding="utf-8", newline="") as stream:
        return list(csv.DictReader(stream))


def status(library: Path) -> dict:
    rows = bindings(library)
    result = {}
    for family in FAMILIES:
        selected = [row for row in rows if row["family"] == family]
        paths = {payload_path(row["payload"]) for row in selected}
        files = [(library / "assets" / path) for path in paths]
        available = [path for path in files if path.is_file()]
        result[family] = {
            "ids": len({row["id"] for row in selected}),
            "bindings": len(selected),
            "payloads": len(paths),
            "stored": len(available),
            "bytes": sum(path.stat().st_size for path in available),
        }
    return result


def resolve(library: Path, family: str, identity: str) -> list[dict]:
    grouped: dict[Path, list[str]] = {}
    for row in bindings(library):
        if row["family"] == family and row["id"] == identity:
            path = Path("assets") / payload_path(row["payload"])
            grouped.setdefault(path, []).append(row["document"])
    return [
        {
            "payload": path.as_posix(),
            "stored": (library / path).is_file(),
            "documents": sorted(set(documents)),
        }
        for path, documents in grouped.items()
    ]


def restore(document: dict | list, assets: Path) -> None:
    if isinstance(document, list):
        for value in document:
            if isinstance(value, (dict, list)):
                restore(value, assets)
        return
    for key, value in document.items():
        if isinstance(value, dict) and set(value) == {REFERENCE}:
            relative = payload_path(value[REFERENCE])
            source = asset_file(assets, relative)
            if relative.suffix == ".json":
                document[key] = read_json(source)
                continue
            data = source.read_bytes()
            if (
                key not in ("data", "pcm16")
                or document["sha256"].lower() != relative.stem
                or relative.stem != hashlib.sha256(data).hexdigest()
            ):
                raise ValueError(f"prepared payload identity differs: {relative.as_posix()}")
            document[key] = base64.b64encode(data).decode("ascii")
        elif isinstance(value, (dict, list)):
            restore(value, assets)


def assemble(library: Path, destination: Path, budget: int) -> dict:
    library = library.resolve(strict=True)
    if destination.exists() or destination.is_relative_to(library):
        raise ValueError("assembly requires a fresh directory outside the library")
    family_status = status(library)
    missing = [name for name, row in family_status.items() if row["stored"] != row["payloads"]]
    if missing:
        raise ValueError("prepare required families first: " + ", ".join(missing))
    base = library / "base"
    assets = library / "assets"
    files = tree_files(base / "files")
    documents = tree_files(base / "documents")
    size = sum(path.stat().st_size for path in (*files.values(), *documents.values()))
    with (base / "bindings.csv").open(encoding="utf-8", newline="") as stream:
        # Count bytes per use rather than per blob. Base64 can expand each use by 4/3.
        size += sum(
            4 * ((assets / payload_path(row["payload"])).stat().st_size + 2) // 3
            for row in csv.DictReader(stream)
        )
    destination.parent.mkdir(parents=True, exist_ok=True)
    if size > min(budget, shutil.disk_usage(destination.parent).free):
        raise ValueError("package exceeds available assembly budget")
    with TemporaryDirectory(prefix=".sf2-assembly-", dir=destination.parent) as temp:
        stage = Path(temp) / "package"
        for relative, source in files.items():
            copy_file(source, stage / relative)
        for relative, source in documents.items():
            document = read_json(source)
            restore(document, assets)
            target = stage / relative
            target.parent.mkdir(parents=True, exist_ok=True)
            write_json(target, document)
            del document
        for name, reference in read_json(base / "external.json").items():
            relative = Path(name)
            if relative.anchor or ".." in relative.parts:
                raise ValueError("external binding escapes assembled package")
            copy_file(asset_file(assets, payload_path(reference)), stage / relative)
        closure = package_files(
            stage / "world.json",
            stage / "main-scenes.json",
            stage / "entry/entry.json",
            stage / "opening-inputs",
        )
        if closure.keys() != tree_files(stage).keys():
            raise ValueError("assembled file set differs from package closure")
        stage.rename(destination)
    return {
        "files": len(closure),
        "bytes": sum((destination / name).stat().st_size for name in closure),
    }


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    commands = parser.add_subparsers(dest="command", required=True)
    staging = commands.add_parser("prepare", help="import already prepared package resources")
    staging.add_argument("--package", type=Path, required=True)
    staging.add_argument("--family", action="append", choices=(*FAMILIES, "all"))
    assembly = commands.add_parser("assemble", help="restore a standard package from the library")
    assembly.add_argument("--destination", type=Path, required=True)
    inspect = commands.add_parser(
        "status", help="count available prepared payloads for each family"
    )
    lookup = commands.add_parser("resolve", help="find stored payload variants for a family and ID")
    lookup.add_argument("--family", required=True, choices=FAMILIES)
    lookup.add_argument("--id", required=True)
    for command in (staging, assembly, inspect, lookup):
        command.add_argument("--library", type=Path, required=True)
    for command in (staging, assembly):
        command.add_argument("--budget-mib", type=int, default=1024)
    args = parser.parse_args()
    library = private_output(args.library)
    if args.command == "prepare":
        families = set(args.family or ["all"])
        if "all" in families:
            families = set(FAMILIES)
        result = prepare(args.package, library, families, args.budget_mib * MIB)
    elif args.command == "assemble":
        result = assemble(library, private_output(args.destination), args.budget_mib * MIB)
    elif args.command == "resolve":
        result = resolve(library, args.family, args.id)
    else:
        result = status(library)
    print(json.dumps(result, sort_keys=True))


if __name__ == "__main__":
    main()
