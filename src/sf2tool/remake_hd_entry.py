"""Derive optional entry layers from a complete prepared private package."""
from __future__ import annotations

import argparse
import json
import shutil
import tempfile
from pathlib import Path

from PIL import Image

from sf2tool.remake_command_menu import write_json
from sf2tool.remake_hd_actors import frame
from sf2tool.remake_map3_ramp import load_filter
from sf2tool.remake_private_package import package_files, private_output, read_json


def prepare(base: Path, destination: Path, gfx: Path, pilot: bool) -> dict:
    base = base.resolve(strict=True)
    destination = private_output(destination)
    if destination.exists() or destination.is_relative_to(base):
        raise ValueError("select a fresh output outside the source package")
    files = package_files(base / "world.json", base / "main-scenes.json",
                          base / "entry/entry.json", base / "opening-inputs")
    entry = read_json(base / "entry/entry.json")
    if "hdResources" in entry:
        raise ValueError("source already declares alternate entry layers")
    names = {"title", "numeral", "witch-arrival", "witch", "witch-blink", "witch-mouth"}
    if {row["name"] for row in entry["resources"]} != names or len(entry["resources"]) != 6:
        raise ValueError("expected six original entry layers")
    size = sum(path.stat().st_size for path in files.values())
    destination.parent.mkdir(parents=True, exist_ok=True)
    if shutil.disk_usage(destination.parent).free < size + 1024**3:
        raise OSError("retain one GiB free after complete package staging")
    scale = load_filter(gfx)[1]
    with tempfile.TemporaryDirectory(prefix=".entry-", dir=destination.parent) as temporary:
        stage = Path(temporary) / "package"
        for relative, source in files.items():
            target = stage / relative
            target.parent.mkdir(parents=True, exist_ok=True)
            shutil.copyfile(source, target)
        entry["hdResources"] = []
        for row in entry["resources"]:
            if pilot and row["name"] != "title":
                continue
            with Image.open(base / "entry" / row["file"]) as source:
                if source.size != (256, 224):
                    raise ValueError("expected original 256x224 entry layer")
                with source.convert("RGBA") as rgba, frame(rgba, scale) as variant:
                    name = row["name"] + ".hd.png"
                    variant.save(stage / "entry" / name)
            entry["hdResources"].append(dict(name=row["name"], file=name, width=1024, height=896))
        write_json(stage / "entry/entry.json", entry)
        closed = package_files(stage / "world.json", stage / "main-scenes.json",
                               stage / "entry/entry.json", stage / "opening-inputs")
        result = dict(files=len(closed), bytes=sum(p.stat().st_size for p in closed.values()),
                      layers=len(entry["hdResources"]))
        stage.rename(destination)
    return result


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ("base", "out", "gfx-root"):
        parser.add_argument("--" + name, type=Path, required=True)
    parser.add_argument("--pilot", action="store_true")
    args = parser.parse_args()
    print(json.dumps(prepare(args.base, args.out, args.gfx_root, args.pilot)))


if __name__ == "__main__":
    main()
