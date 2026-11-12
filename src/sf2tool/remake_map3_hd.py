"""Publish a private switchable Map3 review pack, inventory and viewable map plates."""

from __future__ import annotations

import argparse
import base64
import csv
import hashlib
import io
import json
import shutil
import tempfile
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont, ImageOps

from sf2tool.map3_painted_materials import NEAREST, brushes, inked_figure, painted_block
from sf2tool.remake_command_menu import world_dependencies, write_json


def decode(row):
    data = base64.b64decode(row["data"], validate=True)
    if hashlib.sha256(data).hexdigest() != row["sha256"].lower():
        raise ValueError("input raster identity mismatch")
    size = (row["width"], row["height"])
    if row["format"] == "rgba8":
        return Image.frombytes("RGBA", size, data)
    with Image.open(io.BytesIO(data)) as image:
        if row["format"] != "png" or image.size != size:
            raise ValueError("unexpected raster shape")
        return image.convert("RGBA")


def raster(image):
    # A fixed palette for transparent padding prevents halos on nearest-scaled game edges.
    image = image.quantize(224, method=Image.Quantize.FASTOCTREE, dither=Image.Dither.NONE)
    stream = io.BytesIO()
    image.save(stream, format="PNG", optimize=True)
    data = stream.getvalue()
    return dict(
        width=image.width,
        height=image.height,
        format="png",
        sha256=hashlib.sha256(data).hexdigest(),
        data=base64.b64encode(data).decode("ascii"),
    )


def block_images(visual):
    atlas = decode(visual["atlas"])
    unit = 8 * visual["scale"]
    for words in visual["blocks"]:
        block = Image.new("RGBA", (24, 24))
        for position, word in enumerate(words):
            tile = (word & 1023) - 256
            if tile < 0:
                continue
            x, y = tile % 16 * unit, tile // 16 * unit
            piece = atlas.crop((x, y, x + unit, y + unit)).resize((8, 8), NEAREST)
            if word & 2048:
                piece = ImageOps.mirror(piece)
            if word & 4096:
                piece = ImageOps.flip(piece)
            block.paste(piece, (position % 3 * 8, position // 3 * 8))
        yield block


def asset_rows(world, encounters, path, scene, entry, treatments):
    p = world["presentation"]
    for item in entry["resources"]:
        yield (
            "entry",
            "title / witch",
            item["name"],
            f"{item['width']}x{item['height']}",
            "",
            "entry layer",
            "",
            "",
            f"{item['usedTiles']} source tiles",
        )
    for owner, sounds in [
        ("entry", entry["presentation"].get("audio", [])),
        ("world", p.get("audio", [])),
    ]:
        for sound in sounds:
            yield (
                owner,
                "audio cue",
                sound["cue"],
                f"{sound['sampleRate']} Hz",
                "",
                "audio",
                "",
                "",
                f"command={sound['command']} timer={sound['timerB']}",
            )
    for v in p["maps"]:
        map_id = v["map"]
        layout = next(m for m in world["maps"] if m["id"] == map_id)
        uses = {}
        roles = {}
        for y, row in enumerate(layout["layout"]):
            for x, word in enumerate(row):
                block = word & 1023
                uses.setdefault(block, []).append((x, y))
                tags = roles.setdefault(block, set())
                for n, area in enumerate(layout["areas"]):
                    if area["minX"] <= x <= area["maxX"] and area["minY"] <= y <= area["maxY"]:
                        tags.add(f"area{n}.base")
                    off = area.get("overlay") or {}
                    dx, dy = off.get("x", 0), off.get("y", 0)
                    if (
                        (dx or dy)
                        and area["minX"] <= x - dx <= area["maxX"]
                        and area["minY"] <= y - dy <= area["maxY"]
                    ):
                        tags.add(f"area{n}.overlay")
        for i, words in enumerate(v["blocks"]):
            coords = uses.get(i, [])
            yield (
                map_id,
                "block",
                i,
                "24x24",
                "96x96" if map_id == "map-3" else "",
                treatments[i] if map_id == "map-3" else "original",
                len(coords),
                " ".join(f"{x},{y}" for x, y in coords[:4]),
                " ".join(sorted(roles.get(i, [])))
                + "; tiles="
                + " ".join(str((word & 1023) - 256) for word in words),
            )
    for actor in p["sprites"]:
        for d in range(len(actor["directions"])):
            for half in range(2):
                yield (
                    "shared actors",
                    "walking frame",
                    f"{actor['sprite']}/{d}/{half}",
                    "24x24",
                    "96x96" if "hdDirections" in actor else "",
                    "direction + half; lateral mirror",
                    "",
                    "",
                    f"portrait={actor['portrait']}",
                )
    for portrait in p["portraits"]:
        yield (
            "shared dialogue",
            "portrait atlas",
            portrait["portrait"],
            "64x64",
            "256x256" if "hdRaster" in portrait else "",
            "6 visible columns; 2 alternate columns",
            "",
            "",
            f"eyes={len(portrait.get('eyes', []))} mouth={len(portrait.get('mouth', []))}",
        )
    for key, entries in p.items():
        if isinstance(entries, dict) and key != "hdIdentityIcons":
            for identity, row in entries.items():
                if "width" in row:
                    hd = p.get("hdIdentityIcons", {}).get(identity)
                    yield (
                        "field UI",
                        key,
                        identity,
                        f"{row['width']}x{row['height']}",
                        f"{hd['width']}x{hd['height']}" if hd else "",
                        "UI",
                        "",
                        "",
                        "",
                    )
    for location in dict.fromkeys([scene, *(path.parent / e["battleScenes"] for e in encounters)]):
        document = json.loads(location.read_text(encoding="utf-8-sig"))
        for key, row in document.get("rasters", {}).items():
            yield (
                document.get("encounter", "opening"),
                "battle raster",
                key,
                f"{row['width']}x{row['height']}",
                "",
                "battle UI/scene",
                "",
                "",
                "",
            )


def compose(map_def, images, area):
    cell = images[0].width
    x0, y0 = area["minX"], area["minY"]
    cols, rows = area["maxX"] - x0 + 1, area["maxY"] - y0 + 1
    layout = [row.copy() for row in map_def["layout"]]
    for event in map_def["layoutEvents"]["roofs"]:
        if event["clear"]:
            continue
        copy = event["copy"]
        sx, sy = copy["source"]["x"], copy["source"]["y"]
        dx, dy = copy["destination"]["x"], copy["destination"]["y"]
        patch = [r[sx : sx + copy["width"]] for r in layout[sy : sy + copy["height"]]]
        for i, values in enumerate(patch):
            layout[dy + i][dx : dx + len(values)] = values
    output = Image.new("RGBA", (cols * cell, rows * cell), (0, 0, 0, 255))
    ox, oy = area["overlay"]["x"], area["overlay"]["y"]
    for y in range(rows):
        for x in range(cols):
            pos = (x * cell, y * cell)
            output.alpha_composite(images[layout[y0 + y][x0 + x] & 1023], pos)
            if ox or oy:
                overlay = layout[y0 + y + oy][x0 + x + ox] & 1023
                if overlay:
                    output.alpha_composite(images[overlay], pos)
    return output.convert("RGB")


def comparison(left, right, file, title):
    sheet = Image.new("RGB", (left.width * 2 + 16, left.height + 72), "#18231f")
    draw = ImageDraw.Draw(sheet)
    font = ImageFont.load_default(size=22)
    draw.text((12, 18), "ORIGINAL   /   " + title, fill="white", font=font)
    draw.text((left.width + 28, 18), "HD   /   " + title, fill="white", font=font)
    sheet.paste(left, (0, 60))
    sheet.paste(right, (left.width + 16, 60))
    sheet.save(file)


def build(world_path, scene, entry_path, master, out):
    data = json.loads(world_path.read_text(encoding="utf-8-sig"))
    world = data["world"]
    visual = next(v for v in world["presentation"]["maps"] if v["map"] == "map-3")
    if "hdBlocks" in visual:
        raise ValueError("already contains painted Map3 blocks")
    with Image.open(master) as sheet:
        material = brushes(sheet)
    dependencies = world_dependencies(data, world_path)
    out.mkdir(parents=True)
    review = out / "review"
    review.mkdir()
    base = list(block_images(visual))
    hd, descriptions = zip(*(painted_block(image, material) for image in base), strict=True)
    visual["hdBlocks"] = [raster(image) for image in hd]
    map_def = next(m for m in world["maps"] if m["id"] == "map-3")
    # Shared identities may enter by event/entity substitution. Supply all admitted frames;
    # the field renderer chooses them only on maps that declare the painted variant.
    for actor in world["presentation"]["sprites"]:
        actor["hdDirections"] = []
        for row in actor["directions"]:
            original = decode(row)
            output = Image.new("RGBA", (192, 96))
            for x in (0, 24):
                pose = original.crop((x, 0, x + 24, 24))
                output.paste(inked_figure(pose, material["cloth"]), (x * 4, 0))
            actor["hdDirections"].append(raster(output))
    for portrait in world["presentation"]["portraits"]:
        original = decode(portrait["raster"])
        target = Image.new("RGBA", (256, 256))
        # Keep the six-column face and its separate eye/mouth replacement tiles aligned.
        target.paste(inked_figure(original.crop((0, 0, 48, 64)), material["cloth"]), (0, 0))
        for y in range(0, 64, 8):
            for x in (48, 56):
                patch = original.crop((x, y, x + 8, y + 8))
                target.paste(inked_figure(patch, material["cloth"]), (x * 4, y * 4))
        portrait["hdRaster"] = raster(target)
    entry = json.loads(entry_path.read_text(encoding="utf-8-sig"))
    with (review / "assets.csv").open("w", newline="", encoding="utf-8-sig") as stream:
        writer = csv.writer(stream)
        writer.writerow(
            [
                "owner",
                "family",
                "id",
                "original",
                "hd",
                "treatment",
                "layout uses",
                "example positions",
                "tile IDs or binding",
            ]
        )
        writer.writerows(
            asset_rows(world, data["encounters"], world_path, scene, entry, descriptions)
        )
    with (review / "map3-cast.csv").open("w", newline="", encoding="utf-8-sig") as stream:
        writer = csv.writer(stream)
        writer.writerow(
            ["scope", "setup or predicate", "slot / member", "sprite", "portrait", "x", "y"]
        )
        cast = {s["sprite"]: s["portrait"] for s in world["presentation"]["sprites"]}
        for setup in [map_def, *map_def["setups"]]:
            for entity in setup["entities"]:
                sprite = entity.get("sprite")
                writer.writerow(
                    [
                        "entity",
                        setup["id"],
                        entity["id"],
                        sprite,
                        cast.get(sprite),
                        entity["position"]["x"],
                        entity["position"]["y"],
                    ]
                )
        population = map_def["population"]
        writer.writerow(
            [
                "player",
                "default",
                0,
                population["playerSprite"],
                cast.get(population["playerSprite"]),
                "",
                "",
            ]
        )
        for row in population["followers"]:
            writer.writerow(
                [
                    "follower",
                    row["flag"],
                    row["character"],
                    row["sprite"],
                    cast.get(row["sprite"]),
                    "",
                    "",
                ]
            )
        for row in population["allySprites"]:
            for key in ("sprite", "unjoinedSprite"):
                if row.get(key) is not None:
                    writer.writerow(
                        [
                            key,
                            row.get("joinedFlag"),
                            row["character"],
                            row[key],
                            cast.get(row[key]),
                            "",
                            "",
                        ]
                    )
    hd = [decode(r) for r in visual["hdBlocks"]]
    first = world["presentation"]["sprites"][0]
    portrait = next(
        p for p in world["presentation"]["portraits"] if p["portrait"] == first["portrait"]
    )

    def character_plate(hd_mode):
        plate = Image.new("RGB", (432, 288), "#35433e")
        faces = decode(portrait["hdRaster" if hd_mode else "raster"])
        density = faces.width // 64
        faces = faces.crop((0, 0, 48 * density, 64 * density)).resize((192, 256), NEAREST)
        plate.paste(faces, (8, 16), faces)
        for index, row in enumerate(first["hdDirections" if hd_mode else "directions"]):
            poses = decode(row).resize((192, 96), NEAREST)
            plate.paste(poses, (220, index * 96), poses)
        return plate

    comparison(character_plate(False), character_plate(True), review / "bowie.png", "BOWIE")
    for number, area in enumerate(map_def["areas"]):
        original = compose(map_def, base, area)
        painted = compose(map_def, hd, area)
        original.save(review / f"area-{number}-original.png")
        painted.save(review / f"area-{number}-hd.png")
        if number == 0:
            box = (4 * 96, 5 * 96, 12 * 96, 11 * 96)
            crop = original.crop(tuple(v // 4 for v in box)).resize((768, 576), NEAREST)
            comparison(crop, painted.crop(box), review / "town-detail.png", "GRANSEAL")
        else:
            scaled = original.resize(painted.size, NEAREST)
            comparison(scaled, painted, review / f"room-{number}.png", f"INTERIOR {number}")
    if dependencies.intersection({"world.json", "scene.json", "review"}):
        raise ValueError("output/dependency collision")
    for relative in sorted(dependencies):
        destination = out / relative
        destination.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(world_path.parent / relative, destination)
    shutil.copyfile(scene, out / "scene.json")
    write_json(out / "world.json", data)
    if (out / "world.json").stat().st_size > 128 * 1024 * 1024:
        raise ValueError("painted world exceeds the accepted private document limit")


def publish(world_path, scene, entry_path, master, out):
    if out.exists():
        raise ValueError("choose a fresh output directory")
    out.parent.mkdir(parents=True, exist_ok=True)
    with tempfile.TemporaryDirectory(dir=out.parent, prefix=".map-art-") as folder:
        stage = Path(folder) / "pack"
        build(world_path, scene, entry_path, master, stage)
        stage.rename(out)


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ("world", "scene", "entry", "master", "out"):
        parser.add_argument("--" + name, required=True, type=Path)
    args = parser.parse_args()
    publish(args.world, args.scene, args.entry, args.master, args.out)
