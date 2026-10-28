"""Add original command icons to a copy of an existing private scene input.

Only eight 24x24 rasters are decoded. Source verification, palette decoding and
tile rendering reuse texture_extract; the ordinary Content raster reader owns
runtime admission. No canonical scene, ROM or asset-library file is changed.
"""

import argparse
import base64
import hashlib
import json
from pathlib import Path

from sf2tool.texture_extract import (
    MAIN_MENU_ICON_BYTES,
    MAIN_MENU_PATH,
    _load_base_palette,
    _render_tile_pool_sheet,
    _verified_source,
)


def prepare(rom: Path, upstream: Path, scene: Path, out_dir: Path) -> None:
    source, commit, identity = _verified_source(rom, upstream, owner="command-menu")
    payload = source.read(MAIN_MENU_PATH, symbol="tiles_MainMenu")
    if len(payload) != 7 * MAIN_MENU_ICON_BYTES:
        raise ValueError("command-menu source size drift")
    palette = _load_base_palette(source)
    document = json.loads(scene.read_text(encoding="utf-8-sig"))
    rasters = document["rasters"]
    if any(key.startswith("command-menu.") for key in rasters):
        raise ValueError("input scene already owns command-menu rasters")
    receipts = []
    for index, name in enumerate(("attack", "magic", "item", "stay")):
        for frame, state in enumerate(("normal", "selected")):
            start = index * MAIN_MENU_ICON_BYTES + frame * 288
            pixels, width, height = _render_tile_pool_sheet(
                payload[start : start + 288], palette, sheet_columns=3
            )
            key = f"command-menu.{name}.{state}"
            digest = hashlib.sha256(bytes(pixels)).hexdigest()
            rasters[key] = {
                "width": width,
                "height": height,
                "format": "rgba8",
                "data": base64.b64encode(bytes(pixels)).decode("ascii"),
                "sha256": digest,
            }
            receipts.append({"resource": key, "sourceOffset": start, "sha256": digest})
    # New output directory only: partial/failed publications cannot overwrite a prior input/run.
    out_dir.mkdir(parents=True, exist_ok=False)
    (out_dir / "scene.json").write_text(json.dumps(document), encoding="utf-8")
    (out_dir / "provenance.json").write_text(
        json.dumps(
            {
                "upstreamCommit": commit,
                "romSha256": identity["hashes"]["sha256"],
                "source": MAIN_MENU_PATH.as_posix(),
                "symbol": "tiles_MainMenu",
                "palette": "base palette via texture_extract",
                "transparentIndex": 0,
                "layout": "nine-tile halves in 3x3 row-major order; first normal, second selected",
                "resources": receipts,
            },
            indent=2,
        ),
        encoding="utf-8",
    )


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ("rom", "upstream", "scene", "out-dir"):
        parser.add_argument(f"--{name}", type=Path, required=True)
    args = parser.parse_args()
    prepare(args.rom, args.upstream, args.scene, args.out_dir)


if __name__ == "__main__":
    main()
