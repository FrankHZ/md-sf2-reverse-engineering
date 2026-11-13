"""Add original battle, church or field icons to a copy of an existing private input.

Each family contains eight 24x24 rasters. --menu all publishes the ordinary
opening world and its selected battle scenes together, with one world copy.
Source verification, palette decoding and
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


def menu_assets(
    rom: Path,
    upstream: Path,
    listing: Path | None = None,
    rom_assets: bool = False,
    menu: str = "church",
) -> tuple[dict, dict]:
    """Decode an existing menu family without publishing or copying a world."""
    source, commit, identity = _verified_source(
        rom, upstream, owner=f"{menu}-menu", listing_path=listing
    )
    main_menu = menu in ("field", "command")
    symbol = (
        "tiles_MainMenu"
        if main_menu
        else {"battlefield": "tiles_BattleFieldMenu"}.get(menu, f"tiles_{menu.title()}Menu")
    )
    pointer_symbol = "p_tiles_BattlefieldMenu" if menu == "battlefield" else f"p_{symbol}"
    next_symbol = {
        "item": "tiles_BattleFieldMenu",
        "battlefield": "tiles_ChurchMenu",
        "shop": "tiles_CaravanMenu",
    }.get(menu, "tiles_ShopMenu")
    source_path = (
        MAIN_MENU_PATH if main_menu else Path(f"data/graphics/tech/menus/{menu}menutiles.bin")
    )
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
            if main_menu
            else (
                symbol,
                next_symbol,
                pointer_symbol,
                "pt_tiles_Menu",
            )
        )
        for checked in symbols:
            if source.addresses[checked] != fixture["table"][checked]:
                raise ValueError(f"{menu}-menu H1/accepted-address drift")
        pointer = source.addresses["p_tiles_MainMenu" if main_menu else pointer_symbol]
        if int.from_bytes(source.rom[pointer : pointer + 4], "big") != address:
            raise ValueError(f"{menu}-menu ROM pointer drift")
        byte_count = (
            7 * MAIN_MENU_ICON_BYTES if main_menu else source.addresses[next_symbol] - address
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
    if main_menu:
        if len(compressed) != 7 * MAIN_MENU_ICON_BYTES:
            raise ValueError("field-menu source size drift")
        table = source.addresses["pt_tiles_Menu"] + (4 if menu == "command" else 0)
        expected_mapping = 0x80010203 if menu == "command" else 0x85010204
        if int.from_bytes(source.rom[table : table + 4], "big") != expected_mapping:
            raise ValueError(f"{menu}-menu slot mapping drift")
        payload = compressed
    else:
        if menu in ("item", "battlefield", "shop"):
            entry = {"item": 12, "battlefield": 16, "shop": 24}[menu]
            pointer_entry = source.addresses["pt_tiles_Menu"] + entry
            if (
                int.from_bytes(source.rom[pointer_entry : pointer_entry + 4], "big")
                != source.addresses[pointer_symbol]
            ):
                raise ValueError(f"{menu}-menu slot mapping drift")
        decoded = decode_stack_compressed(compressed, expected_output_bytes=2304)
        payload, consumed = decoded.output, decoded.input_bits_consumed
    rasters, receipts = {}, []
    names, indices = {
        "field": (("member", "magic", "item", "search"), (5, 1, 2, 4)),
        "church": (("raise", "cure", "promote", "save"), (0, 1, 2, 3)),
        "command": (("attack", "magic", "item", "stay"), (0, 1, 2, 3)),
        "item": (("use", "give", "equip", "drop"), (0, 1, 2, 3)),
        "shop": (("buy", "sell", "repair", "deals"), (0, 1, 2, 3)),
        "battlefield": (("member", "map", "speed", "quit"), (0, 1, 2, 3)),
    }[menu]
    if menu in ("shop", "battlefield"):
        # Read the pinned ROM string table; modern consumer order is not source evidence.
        table = source.addresses[f"pt_{menu.title()}MenuNames"]
        for index, name in enumerate(names):
            pointer = int.from_bytes(source.rom[table + index * 4 : table + index * 4 + 4], "big")
            end = source.rom.index(0, pointer)
            if source.rom[pointer:end].decode("ascii").lower() != name:
                raise ValueError(f"{menu}-menu name/slot drift")
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
    return rasters, {
        "upstreamCommit": commit,
        "romSha256": identity["hashes"]["sha256"],
        "inputMode": "canonical-ROM" if rom_assets else "split-payload-parity",
        "sourceAddress": address,
        "sourceBytes": len(compressed),
        "compressedBytes": None if main_menu else len(compressed),
        "inputBitsConsumed": consumed,
        "decodedBytes": len(payload),
        "paletteAddress": palette_address,
        "paletteBytes": PALETTE_BYTES,
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
            "menustringspointertable.asm "
            f"pt_{'BattleMenuWithStay' if menu == 'command' else menu.title() + 'Menu'}Names; "
            "diamondmenu.asm "
            "dmaDiamondMenuIcon_Up/Left/Right/Down"
        ),
        "resources": receipts,
    }


def mount(rasters: dict, additions: dict, prefix: str = "") -> None:
    """Keep already prepared identical art; reject partial or conflicting namespaces."""
    selected = {prefix + key: value for key, value in additions.items()}
    existing = {key: value for key, value in rasters.items() if key.startswith(prefix)}
    if existing and existing != selected:
        raise ValueError("input already owns partial or different menu rasters")
    rasters.update(selected)


def prepare(
    rom: Path,
    upstream: Path,
    scene: Path,
    out_dir: Path,
    listing: Path | None = None,
    rom_assets: bool = False,
) -> None:
    if out_dir.exists():
        raise ValueError("output directory already exists")
    art, provenance = menu_assets(rom, upstream, listing, rom_assets, "command")
    document = json.loads(scene.read_text(encoding="utf-8-sig"))
    mount(document["rasters"], art, "command-menu.")
    out_dir.mkdir(parents=True, exist_ok=False)
    write_json(out_dir / "scene.json", document)
    write_json(out_dir / "provenance.json", provenance)


def write_json(path: Path, document: dict) -> None:
    # Stream serialization; avoid a second world-sized encoded string.
    with path.open("w", encoding="utf-8") as stream:
        json.dump(document, stream, ensure_ascii=False)


def world_dependencies(document: dict, world: Path) -> set[str]:
    """Validate the relative encounter/audio inputs of an existing publication."""
    world_document = document.get("world", document)
    dependencies = {
        value
        for row in document.get("encounters", [])
        for value in row.values()
        if value is not None
    }
    dependencies.update(
        f"audio-{sound['command']:02x}-{sound['timerB']:02x}/"
        f"command-{sound['command']:02x}-timer-{sound['timerB']:02x}.wav"
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
    return dependencies


def prepare_church(
    rom: Path,
    upstream: Path,
    world: Path,
    out_dir: Path,
    listing: Path | None = None,
    rom_assets: bool = False,
    menu: str = "church",
    scene: Path | None = None,
    identity_icons: bool = False,
    item_menu: bool = False,
    shop_battlefield: bool = False,
    target_cursor: bool = False,
) -> None:
    """Publish once, including every declared encounter when selecting all menus."""
    if out_dir.exists():
        raise ValueError("output directory already exists")
    if menu == "all" and scene is None:
        raise ValueError("all menus require the opening battle scene")
    if item_menu and menu != "all":
        raise ValueError("item-menu requires all menus")
    if target_cursor and menu != "all":
        raise ValueError("target-cursor requires all menus")
    menus = ("field", "church", "command") if menu == "all" else (menu,)
    if item_menu:
        menus += ("item",)
    if shop_battlefield:
        if menu != "all":
            raise ValueError("shop-battlefield requires all menus")
        menus += ("shop", "battlefield")
    assets = {name: menu_assets(rom, upstream, listing, rom_assets, name) for name in menus}
    icons = None
    cursor = None
    if target_cursor:
        from sf2tool.remake_battle_scene_content import single_cell_cursor

        cursor = single_cell_cursor(rom.read_bytes())
    document = json.loads(world.read_text(encoding="utf-8-sig"))
    world_document = document.get("world", document)
    if identity_icons:
        from sf2tool.remake_identity_icons import (
            field_item_ids,
            identity_assets,
            mount_identity_icons,
        )

        icons = identity_assets(
            rom, upstream, listing, rom_assets, field_items=field_item_ids(world_document)
        )
    if icons:
        presentation = world_document["presentation"]
        mount_identity_icons(presentation.setdefault("identityIcons", {}), icons)
    for name in menus:
        if name not in ("command", "battlefield"):
            presentation = world_document["presentation"]
            key = name + "Menu"
            if key in presentation and presentation[key] != assets[name][0]:
                raise ValueError(f"input world already owns partial or different {key}")
            presentation[key] = assets[name][0]
    dependencies = world_dependencies(document, world)
    source_root = world.resolve().parent
    scenes = {}
    if menu == "all":
        # The opening battle is an external selection; later battles are relative dependencies.
        # Validate all of them before creating the publication. No rules or selection paths change.
        scenes["scene.json"] = json.loads(scene.read_text(encoding="utf-8-sig"))
        for row in document.get("encounters", []):
            if row.get("battleScenes"):
                relative = row["battleScenes"]
                if relative in ("world.json", "provenance.json", "scene.json"):
                    raise ValueError("battle scene collides with publication output")
                scenes[relative] = json.loads(
                    (source_root / relative).read_text(encoding="utf-8-sig")
                )
        for battle_scene in scenes.values():
            if cursor:
                mount(battle_scene["rasters"], {"single-cell": cursor[0]}, "battle-cursor.")
            mount(battle_scene["rasters"], assets["command"][0], "command-menu.")
            if shop_battlefield:
                mount(battle_scene["rasters"], assets["battlefield"][0], "battlefield-menu.")
            if icons:
                mount_identity_icons(battle_scene["rasters"], icons)
    if dependencies.intersection({"world.json", "provenance.json", "scene.json"}):
        raise ValueError("world dependency collides with publication output")
    out_dir.mkdir(parents=True, exist_ok=False)
    for relative in sorted(dependencies):
        destination = out_dir / relative
        destination.parent.mkdir(parents=True, exist_ok=True)
        if relative not in scenes:
            shutil.copyfile(source_root / relative, destination)
    for relative, battle_scene in scenes.items():
        write_json(out_dir / relative, battle_scene)
    write_json(out_dir / "world.json", document)
    provenance = (
        assets[menu][1]
        if menu != "all"
        else {"menus": {name: asset[1] for name, asset in assets.items()}}
    )
    if icons:
        provenance["identityIcons"] = icons[1]
    if cursor:
        provenance["targetCursor"] = cursor[1]
    provenance["copiedDependencies"] = sorted(dependencies - scenes.keys())
    provenance["preparedBattleScenes"] = sorted(scenes)
    write_json(out_dir / "provenance.json", provenance)


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ("rom", "upstream", "out-dir"):
        parser.add_argument(f"--{name}", type=Path, required=True)
    parser.add_argument("--scene", type=Path, help="Opening battle scene; required with --menu all")
    parser.add_argument("--world", type=Path)
    parser.add_argument(
        "--listing", type=Path, help="Existing H1 listing for a read-only pinned checkout"
    )
    parser.add_argument(
        "--rom-assets", action="store_true", help="Decode from verified canonical ROM"
    )
    parser.add_argument(
        "--identity-icons", action="store_true", help="Add bounded item/spell art with --menu all"
    )
    parser.add_argument("--menu", choices=("church", "field", "all"), default="church")
    parser.add_argument(
        "--item-menu", action="store_true", help="Add source Use/Give/Equip/Drop art to a world"
    )
    parser.add_argument(
        "--shop-battlefield",
        action="store_true",
        help="Add source shop and empty-ground options art with --menu all",
    )
    parser.add_argument(
        "--target-cursor",
        action="store_true",
        help="Add original single-cell F4/F8 art to all scenes",
    )
    args = parser.parse_args()
    if args.target_cursor and args.menu != "all":
        parser.error("--target-cursor requires --menu all")
    if args.shop_battlefield and args.menu != "all":
        parser.error("--shop-battlefield requires --menu all")
    if args.item_menu and args.menu != "all":
        parser.error("--item-menu requires --menu all")
    if args.identity_icons and args.menu != "all":
        parser.error("--identity-icons requires --menu all")
    if args.menu == "all":
        if args.world is None or args.scene is None:
            parser.error("--menu all requires both --world and --scene")
    elif (args.world is None) == (args.scene is None):
        parser.error("select either --world or --scene; both require --menu all")
    elif args.world is None and args.menu != "church":
        parser.error("--menu field requires --world")
    if args.world is not None:
        prepare_church(
            args.rom,
            args.upstream,
            args.world,
            args.out_dir,
            args.listing,
            args.rom_assets,
            args.menu,
            args.scene,
            args.identity_icons,
            args.item_menu,
            args.shop_battlefield,
            args.target_cursor,
        )
    else:
        prepare(args.rom, args.upstream, args.scene, args.out_dir, args.listing, args.rom_assets)


if __name__ == "__main__":
    main()
