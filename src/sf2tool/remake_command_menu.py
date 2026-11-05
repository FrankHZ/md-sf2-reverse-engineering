"""Add original battle, church or field icons to a copy of an existing private input.

Only eight 24x24 rasters are decoded. Source verification, palette decoding and
tile rendering reuse texture_extract; the ordinary Content raster reader owns
runtime admission. No canonical scene, ROM or asset-library file is changed.
"""

import argparse
import base64
import hashlib
import json
import shutil
from pathlib import Path

from sf2tool.compression import decode_stack_compressed
from sf2tool.paths import repo_path
from sf2tool.texture_extract import (
    BASE_PALETTE_PATH,
    MAIN_MENU_ICON_BYTES,
    MAIN_MENU_PATH,
    PALETTE_BYTES,
    _load_base_palette,
    _render_tile_pool_sheet,
    _verified_source,
    md_palette_color,
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


def prepare_church(
    rom: Path,
    upstream: Path,
    world: Path,
    out_dir: Path,
    listing: Path | None = None,
    rom_assets: bool = False,
    menu: str = "church",
) -> None:
    """Mount four source menu slots through the existing world raster admission."""
    if menu not in ("church", "field"):
        raise ValueError("unknown world menu")
    if out_dir.exists():
        raise ValueError("output directory already exists")
    source, commit, identity = _verified_source(
        rom, upstream, owner=f"{menu}-menu", listing_path=listing
    )
    field = menu == "field"
    symbol = "tiles_MainMenu" if field else "tiles_ChurchMenu"
    source_path = MAIN_MENU_PATH if field else Path("data/graphics/tech/menus/churchmenutiles.bin")
    address = source.addresses[symbol]
    consumed = None
    if rom_assets:
        # A deliberate canonical-ROM route for read-only source checkouts without split bins.
        # Existing split payloads still undergo the unchanged strict parity reader.
        fixture = json.loads(
            repo_path("tests/fixtures/h2/ui-graphics-decode-v1.json").read_text(encoding="utf-8")
        )
        symbols = (
            ("tiles_MainMenu", "tiles_ItemMenu", "p_tiles_MainMenu", "pt_tiles_Menu")
            if field
            else ("tiles_ChurchMenu", "tiles_ShopMenu", "p_tiles_ChurchMenu", "pt_tiles_Menu")
        )
        for checked in symbols:
            if source.addresses[checked] != fixture["table"][checked]:
                raise ValueError(f"{menu}-menu H1/accepted-address drift")
        pointer = source.addresses["p_tiles_MainMenu" if field else "p_tiles_ChurchMenu"]
        if int.from_bytes(source.rom[pointer : pointer + 4], "big") != address:
            raise ValueError(f"{menu}-menu ROM pointer drift")
        byte_count = (
            7 * MAIN_MENU_ICON_BYTES if field else source.addresses["tiles_ShopMenu"] - address
        )
        compressed = source.rom[address : address + byte_count]
        if (source.disasm / source_path).exists() and len(
            source.read(source_path, symbol=symbol)
        ) != byte_count:
            raise ValueError(f"{menu}-menu split payload size drift")
        palette_address = source.addresses["palette_Base"]
        palette_fixture = json.loads(
            repo_path(
                "tests/fixtures/h2/map3-original-player-reference-frame-static-v1.json"
            ).read_text(encoding="utf-8")
        )
        if (
            palette_address != palette_fixture["static"]["indexBindings"]["paletteBase"]
            or PALETTE_BYTES != 32
        ):
            raise ValueError(f"{menu}-menu H1/accepted-palette drift")
        palette_data = source.rom[palette_address : palette_address + PALETTE_BYTES]
        if (source.disasm / BASE_PALETTE_PATH).exists() and len(
            source.read(BASE_PALETTE_PATH, symbol="palette_Base")
        ) != PALETTE_BYTES:
            raise ValueError(f"{menu}-menu split palette size drift")
        palette = [
            md_palette_color(int.from_bytes(palette_data[i : i + 2], "big"))
            for i in range(0, PALETTE_BYTES, 2)
        ]
    else:
        compressed = source.read(source_path, symbol=symbol)
        palette = _load_base_palette(source)
        palette_address = source.addresses["palette_Base"]
    if field:
        if len(compressed) != 7 * MAIN_MENU_ICON_BYTES:
            raise ValueError("field-menu source size drift")
        table = source.addresses["pt_tiles_Menu"]
        if int.from_bytes(source.rom[table : table + 4], "big") != 0x85010204:
            raise ValueError("field-menu slot mapping drift")
        payload = compressed
    else:
        decoded = decode_stack_compressed(compressed, expected_output_bytes=2304)
        payload, consumed = decoded.output, decoded.input_bits_consumed
    document = json.loads(world.read_text(encoding="utf-8-sig"))
    world_document = document.get("world", document)
    property_name = "fieldMenu" if field else "churchMenu"
    if property_name in world_document["presentation"]:
        raise ValueError(f"input world already owns {property_name}")
    rasters, receipts = {}, []
    names = ("member", "magic", "item", "search") if field else ("raise", "cure", "promote", "save")
    indices = (5, 1, 2, 4) if field else (0, 1, 2, 3)
    for index, name in zip(indices, names, strict=True):
        for frame, state in enumerate(("normal", "selected")):
            start = index * MAIN_MENU_ICON_BYTES + frame * 288
            pixels, width, height = _render_tile_pool_sheet(
                payload[start : start + 288], palette, sheet_columns=3
            )
            key = f"{name}.{state}"
            digest = hashlib.sha256(bytes(pixels)).hexdigest()
            rasters[key] = {
                "width": width,
                "height": height,
                "format": "rgba8",
                "data": base64.b64encode(bytes(pixels)).decode("ascii"),
                "sha256": digest,
            }
            receipts.append(
                {"resource": f"{menu}-menu.{key}", "decodedOffset": start, "sha256": digest}
            )
    world_document["presentation"][property_name] = rasters
    # The private-world reader intentionally resolves these selections beneath its world directory.
    # Preserve that package boundary when publishing the one new world, rather than rewriting paths.
    dependencies = {
        value
        for row in document.get("encounters", [])
        for value in row.values()
        if value is not None
    }
    dependencies.update(
        sound["runtimeWav"]
        for sound in world_document["presentation"].get("audio", [])
        if "runtimeWav" in sound
    )
    source_root = world.resolve().parent
    for relative in dependencies:
        path = Path(relative)
        if (
            path.is_absolute()
            or ".." in path.parts
            or not (source_root / path).resolve().is_relative_to(source_root)
        ):
            raise ValueError("world dependency escapes input package")
        if not (source_root / path).is_file():
            raise ValueError(f"world dependency missing: {relative}")
    out_dir.mkdir(parents=True, exist_ok=False)
    for relative in sorted(dependencies):
        destination = out_dir / relative
        destination.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(source_root / relative, destination)
    (out_dir / "world.json").write_text(json.dumps(document, ensure_ascii=False), encoding="utf-8")
    (out_dir / "provenance.json").write_text(
        json.dumps(
            {
                "upstreamCommit": commit,
                "romSha256": identity["hashes"]["sha256"],
                "inputMode": "canonical-ROM" if rom_assets else "split-payload-parity",
                "sourceAddress": address,
                "sourceBytes": len(compressed),
                "compressedBytes": None if field else len(compressed),
                "inputBitsConsumed": consumed,
                "decodedBytes": len(payload),
                "paletteAddress": palette_address,
                "paletteBytes": PALETTE_BYTES,
                "copiedDependencies": sorted(dependencies),
                "source": source_path.as_posix(),
                "symbol": symbol,
                "palette": "base palette via texture_extract",
                "transparentIndex": 0,
                "paletteSource": (
                    "displayinit.asm InitializeDisplay copies "
                    "palette_Base/CRAM_PALETTE_SIZE to PALETTE_3_BASE; "
                    "diamondmenulayout.asm MENUTILE slots select PALETTE3"
                ),
                "paletteAddressFixture": (
                    " "
                    "tests/fixtures/h2/map3-original-player-reference-frame-static-v1.json "
                    "static.indexBindings.paletteBase"
                ),
                "layout": (
                    f"slots {indices} in source up/left/right/down order; 288-byte "
                    "normal/selected halves, 3x3 tiles"
                ),
                "mappingSource": (
                    f"menustringspointertable.asm pt_{menu.title()}MenuNames; diamondmenu.asm "
                    "dmaDiamondMenuIcon_Up/Left/Right/Down"
                ),
                "resources": receipts,
            },
            indent=2,
        ),
        encoding="utf-8",
    )


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ("rom", "upstream", "out-dir"):
        parser.add_argument(f"--{name}", type=Path, required=True)
    inputs = parser.add_mutually_exclusive_group(required=True)
    inputs.add_argument("--scene", type=Path)
    inputs.add_argument("--world", type=Path)
    parser.add_argument(
        "--listing",
        type=Path,
        help="Existing H1 listing for a read-only upstream checkout (world menus only)",
    )
    parser.add_argument(
        "--rom-assets",
        action="store_true",
        help="Decode world-menu bytes directly from the verified canonical ROM",
    )
    parser.add_argument("--menu", choices=("church", "field"), default="church")
    args = parser.parse_args()
    if (
        args.listing is not None or args.rom_assets or args.menu != "church"
    ) and args.world is None:
        parser.error("--listing/--rom-assets/--menu field requires --world")
    if args.world is not None:
        prepare_church(
            args.rom,
            args.upstream,
            args.world,
            args.out_dir,
            args.listing,
            args.rom_assets,
            args.menu,
        )
    else:
        prepare(args.rom, args.upstream, args.scene, args.out_dir)


if __name__ == "__main__":
    main()
