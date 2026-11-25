"""Copy or verify a closed, already-prepared private campaign package without the ROM."""

from __future__ import annotations

import argparse
import filecmp
import hashlib
import json
import os
import re
import shutil
import subprocess
import wave
from pathlib import Path
from tempfile import TemporaryDirectory

from sf2tool.paths import repo_path
from sf2tool.private_inputs import SHARED_INPUT_ROOT_ENV
from sf2tool.remake_asset_build import ACCEPTED_UPSTREAM_COMMIT
from sf2tool.remake_attack_spell_mount import mount_attack_spells
from sf2tool.remake_authored_audio import is_authored_defeat
from sf2tool.remake_battle_scene_content import compose, sprite_layout
from sf2tool.remake_command_menu import world_dependencies, write_json
from sf2tool.remake_exploration_content import _selected_growth, checked_canonical, map_compiler
from sf2tool.remake_exploration_maps import compile_maps
from sf2tool.remake_exploration_programs import OriginalPrograms
from sf2tool.remake_exploration_scene import (
    ally_sprites,
    promotion_sprite_fields,
    promotion_sprites,
)
from sf2tool.remake_promotion_appearances import (
    installed_png,
    mount_promotion_appearances,
    mount_static_equipment,
    mount_walking,
    prepared_ally_sequence,
    read_json,
    shared_color_weapon_frames,
)
from sf2tool.texture_extract import md_palette_color, write_png_rgba

OPENING_INPUTS = (
    "SF2_PRIVATE_BATTLE01_DATA",
    "SF2_PRIVATE_BATTLE01_SCENE",
    "SF2_PRIVATE_BATTLE01_TERRAIN",
    "SF2_PRIVATE_STATIC_DATA",
    "SF2_PRIVATE_ENEMY_DATA",
    "SF2_PRIVATE_ENEMY_GOLD",
)


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
    entry_document = read_json(entry)
    for group, suffix in (("resources", ".png"), ("hdResources", ".hd.png")):
        for resource in entry_document.get(group, []):
            name = resource["file"]
            if Path(name).name != name or name != resource["name"] + suffix:
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


def bind_promotion_progression(package: dict, upstream: Path, promotions: dict[int, str]) -> None:
    """Bind each freshly mounted appearance without replacing other class destinations."""
    commit = subprocess.check_output(
        ["git", "-C", str(upstream), "rev-parse", "HEAD"], text=True
    ).strip()
    if commit != ACCEPTED_UPSTREAM_COMMIT or package["provenance"]["commit"] != commit:
        raise ValueError("Promotion metadata needs the pinned upstream revision")

    selection = read_json(repo_path("remake/content/private/pre-voyage-opening.json"))
    selected = [
        row
        for row in selection["promotionClasses"]
        if promotions.get(row["member"]) == row["class"]
    ]
    if {row["member"] for row in selected} != promotions.keys():
        raise ValueError("mounted promotions differ from campaign selection")
    compiler = OriginalPrograms(
        {"resources": {"standaloneScriptPrograms": [], "initSourcePrograms": []}},
        upstream,
        promotion_classes=tuple(selected),
    )
    # Only these parsers' actual dependencies belong to the existing world provenance.
    compiler.sources = {"disasm/sf2enums.asm"}
    variants = {
        row["character"]: row for row in ally_sprites(compiler) if row["character"] in promotions
    }
    additions = _selected_growth(compiler, selected)

    world = package["world"]
    sprites = {row["sprite"] for row in world["presentation"]["sprites"]}
    selected_sprites = {
        sprite for row in variants.values() for sprite in promotion_sprites(row).values()
    }
    if not selected_sprites <= sprites:
        raise ValueError("Promotion requires its mounted walking sprites")
    for row in additions:
        key = row["actor"], row["classId"]
        previous = next((r for r in world["growth"] if (r["actor"], r["classId"]) == key), None)
        if previous is None:
            world["growth"].append(row)
        elif previous != row:
            raise ValueError(f"conflicting promotion growth: {key}")

    def bind_population(population):
        if population is None:
            return
        for appearance in population["allySprites"]:
            if (variant := variants.get(appearance["character"])) is None:
                continue
            if appearance["sprite"] != variant["sprite"]:
                raise ValueError("Promotion differs from the mounted base character")
            bindings = promotion_sprites(appearance)
            for role, sprite in promotion_sprites(variant).items():
                if bindings.setdefault(role, sprite) != sprite:
                    raise ValueError("conflicting promotion sprite")
            for key in ("promotionClass", "promotionSprite", "promotionSprites"):
                appearance.pop(key, None)
            appearance.update(promotion_sprite_fields(bindings))

    for row in world["maps"]:
        bind_population(row.get("population"))
        for setup in row.get("setups") or ():
            bind_population(setup.get("population"))
    for program in world["programs"]:
        for instruction in program["instructions"]:
            bind_population(instruction.get("population"))

    include_sources(package, upstream, compiler.sources)


def include_sources(package, upstream, paths):
    commit = package["provenance"]["commit"]
    sources = package["provenance"]["sources"]
    existing = {row["path"] for row in sources}
    for name in sorted(paths):
        raw = (upstream / name).read_bytes()
        pinned = subprocess.check_output(["git", "-C", str(upstream), "show", f"{commit}:{name}"])
        if raw.replace(b"\r\n", b"\n") != pinned.replace(b"\r\n", b"\n"):
            raise ValueError("selected pinned source has local changes: " + name)
        if name not in existing:
            sources.append({"path": name, "sha256": hashlib.sha256(raw).hexdigest().upper()})


def prepared_spell_tiles(directory: Path, tileset: int) -> tuple[bytes, list[tuple[int, int, int]]]:
    record = next(
        row
        for row in read_json(directory / "index.json")["records"]
        if row["family"] == "spell" and row["id"] == tileset
    )
    tiles = (directory / record["tiles"]).read_bytes()
    words = read_json(directory / "base-palette.json")
    spell_colors = bytes.fromhex(record["palette"])
    # LoadSpellTileset replaces these three colors in battlescene palette 3.
    for index, slot in enumerate((9, 13, 14)):
        words[slot] = int.from_bytes(spell_colors[2 * index : 2 * index + 2], "big")
    return tiles, [md_palette_color(word) for word in words]


def mount_monster_breath(directory: Path, scratch: Path, scene: dict) -> None:
    tiles, palette = prepared_spell_tiles(directory, 1)
    # graphic_FlameBreathParticle opens on tile 1 and alternates its bit-3 tile.
    for tile in (1, 9):
        leaf = f"kiwi-flame-{tile}.png"
        write_png_rgba(scratch / leaf, 8, 8, compose(tiles, palette, 8, 8, [tile]))
        key = f"kiwi-breath/tile-{tile}"
        raster = installed_png(scratch, leaf)
        if scene["rasters"].setdefault(key, raster) != raster:
            raise ValueError(f"conflicting prepared flame particle {key}")


def mount_phoenix_waves(directory: Path, scratch: Path, scene: dict) -> None:
    tiles, palette = prepared_spell_tiles(directory, 20)
    layout = sprite_layout(4, 4)
    for frame in range(4):
        leaf = f"phoenix-wave-{frame}.png"
        write_png_rgba(
            scratch / leaf,
            32,
            32,
            compose(tiles, palette, 32, 32, [frame * 16 + tile for tile in layout]),
        )
        key = f"phoenix-wave/frame-{frame}"
        raster = installed_png(scratch, leaf)
        if scene["rasters"].setdefault(key, raster) != raster:
            raise ValueError(f"conflicting prepared sound wave {key}")


def mount_prepared_audio(directory, command, dest, presentation):
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
        k: runtime[k] for k in ("sampleRate", "channels", "sampleFrames", "loopBegin", "loopEnd")
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
    if target.exists():
        if target.read_bytes() != wav.read_bytes():
            raise ValueError(f"conflicting WAV {command}")
    else:
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
        if is_authored_defeat(existing) and row["cue"] == existing["cue"] and timer == 196:
            presentation["audio"][presentation["audio"].index(existing)] = row
        else:
            raise ValueError(f"conflicting audio binding {command}")


def mount_indra_staff(library: Path, scratch: Path, scene: dict) -> None:
    directory = library / "weapons"
    index = read_json(directory / "index.json")["records"]
    if index["items"]["92"] != [16, 31]:
        raise ValueError("Indra Staff requires the pinned Guardian Staff graphics selector")
    frames = shared_color_weapon_frames(directory, index, scratch, scene, 92, scale=1)
    animations = read_json(library / "bodies/ally-animations.json")
    for member, body, palette in ((1, 25, 1), (4, 21, 2), (15, 25, 2), (17, 21, 1)):
        (donor,) = (
            row
            for row in scene["actors"]
            if row["side"] == "ally"
            and row["sprite"] == body
            and row["palette"] == palette
            and member in row.get("members", [])
            and row["item"] == 89
        )
        # The selected copy can break while worn, leaving this same member unarmed.
        for item, layers in ((92, frames), (None, [])):
            attack = prepared_ally_sequence(body, animations[body], bool(layers))
            dodge = prepared_ally_sequence(body + 40, animations[body + 40], bool(layers))
            row = dict(
                donor,
                item=item,
                weaponFrames=layers,
                sequences=dict(idle=attack, attack=attack, dodge=dodge, item=attack, cast=attack),
            )
            if row not in scene["actors"]:
                scene["actors"].append(row)


def mount_south_shrine(package, library, upstream, canonical_path):
    """Extend a mounted Grans world without regenerating its encounters, setups or artwork."""
    selection = read_json(repo_path("remake/content/private/pre-voyage-opening.json"))
    canonical = checked_canonical(canonical_path, upstream)
    directory = library / "maps"
    prepared = read_json(directory / "index.json")
    provenance = package["provenance"]
    if (
        canonical["upstream"]["commit"] != provenance["commit"]
        or prepared["upstream"] != provenance["commit"]
        or canonical["romSha256"] != provenance["romSha256"]
        or prepared["rom"] != provenance["romSha256"]
    ):
        raise ValueError("shrine inputs differ from the mounted campaign provenance")
    sound = read_json(library / "music/9/candidate.json")["asset"]
    expected = selection["runtimeWavCues"]["MUSIC_INTRO"]
    if (
        sound["cue"] != "MUSIC_INTRO"
        or sound["command"] != 9
        or sound["timerB"] != expected["timerB"]
        or sound["runtime"]["sha256"] != expected["wavSha256"]
        or any(sound["runtime"][key] != expected[key] for key in ("loopBegin", "loopEnd"))
    ):
        raise ValueError("prepared shrine cue differs from the selected WAV")
    world = package["world"]
    old_maps = {row["id"]: row for row in world["maps"]}
    if any(f"map-{i}" in old_maps for i in (41, 42)):
        raise ValueError("shrine maps already mounted")

    compiler = map_compiler(canonical, upstream, selection, [41, 42])
    additions = compile_maps(compiler, canonical, selection, {}, only_maps=[41, 42])
    visual = world["presentation"]
    by_id = {row["id"]: row for row in prepared["records"]["maps"]}
    blocks = {row["id"]: row["blocks"] for row in canonical["resources"]["blocksets"]}
    for map_row in additions:
        index = int(map_row["id"][4:])
        row = by_id[index]
        atlas = installed_png(directory, row["atlas"])
        palette = prepared["records"]["palettes"][str(row["palette"])]
        map_row["basePalette"] = {"color2": palette[2], "color3": palette[3]}
        compiler.sources.add(f"disasm/data/maps/entries/map{index:02d}/00-tilesets.asm")
        refs = canonical["maps"][index]["references"]
        if (row["blocks"], row["layout"]) != (refs["blockset"] + ".json", refs["layout"] + ".json"):
            raise ValueError("prepared shrine block or layout reference differs")
        words = read_json(directory / row["layout"])
        tiles = blocks[refs["blockset"]]
        if words != [word for line in map_row["layout"] for word in line] or tiles != read_json(
            directory / row["blocks"]
        ):
            raise ValueError(f"prepared map {index} differs from the canonical import")
        music = [area["defaultMusic"] for area in row["areas"]]
        # These source commands retain their identity in PlayMapMusic.
        if not set(music) <= {
            compiler.number(name) for name in ("MUSIC_INTRO", "MUSIC_WITCH", "MUSIC_SHRINE")
        }:
            raise ValueError("unexpected shrine area music")
        visual["maps"].append(
            {
                "map": map_row["id"],
                "music": [{"field": m, "battle": m} for m in music],
                "atlas": atlas,
                "scale": 1,
                "blocks": tiles,
            }
        )
    world["maps"].extend(additions)
    new_programs = list(compiler.programs.values())
    if {p["id"] for p in new_programs} & {p["id"] for p in world["programs"]}:
        raise ValueError("shrine programs would replace mounted behavior")
    world["programs"].extend(new_programs)

    # Replace only the former frontier at each matching source warp. Keep live Map66
    # battle routes, setup overrides and all other event priorities from the base.
    warp_selection = {**selection, "eventMaps": [], "searchMaps": []}
    incoming = compile_maps(compiler, canonical, warp_selection, {}, only_maps=[66])[0]["events"]
    events = old_maps["map-66"]["events"]
    for warp in (e for e in incoming if e.get("map") == "map-41"):
        (index,) = (
            i
            for i, e in enumerate(events)
            if e["kind"] == "warp-frontier" and (e["x"], e["y"]) == (warp["x"], warp["y"])
        )
        obsolete = events[index]["program"]["program"]
        events[index] = warp
        world["programs"].remove(next(p for p in world["programs"] if p["id"] == obsolete))
    compiler.sources.add("disasm/data/graphics/maps/mappalettes/entries.asm")
    include_sources(package, upstream, compiler.sources)
    provenance["controlledBoundary"] += (
        " Includes the early southern shrine exterior, entrance hall and lower chamber."
    )

    return {
        placement["itemWord"] & 127
        for row in additions
        for table in ("chests", "otherItems")
        for placement in row["search"][table]
        if placement["itemWord"] is not None
    }


def bind_field_item_icons(presentation, library, dest, scenes, items):
    # The field and encounter readers validate item identity even for late placements.
    images = read_json(library / "icons/identity-icons.json")
    keys = sorted(
        f"identity-icon.item.{item}.{state}" for item in items for state in ("normal", "selected")
    )
    for key in keys:
        presentation["identityIcons"].setdefault(key, images[key])
    for relative in scenes:
        path = dest / relative
        chapter = read_json(path)
        missing = set(keys) - chapter["rasters"].keys()
        if missing and any(k.startswith("identity-icon.") for k in chapter["rasters"]):
            chapter["rasters"].update((key, images[key]) for key in sorted(missing))
            path.write_text(json.dumps(chapter, separators=(",", ":")), encoding="utf-8")


def mount(root, base, destination, families, commands, *, upstream=None, canonical_path=None):
    """Mount first-consumer operands into a new closed package; needs no production inputs."""
    permitted = {"walking", "icons", "sfx", "music", "pldn"}
    retained_families = {
        "pgnt": {14: "PGNT"},
        "brn": {3: "BRN", 16: "BRN"},
        "brgn": {13: "BRGN", 18: "BRGN"},
        "mmnk": {1: "MMNK", 15: "MMNK"},
        "sorc": {4: "SORC", 17: "SORC"},
        "wiz": {4: "WIZ", 17: "WIZ"},
        "melee": {3: "GLDT", 16: "GLDT", 10: "BDBT"},
        "ranged": {13: "SNIP", 18: "SNIP", 8: "BWNT"},
        "vicar": {1: "VICR", 15: "VICR"},
        "innate": {9: "WFBR", 7: "PHNX"},
        "ninja": {5: "NINJ"},
        "monster": {6: "MNST"},
    }
    optional = retained_families.keys() | {
        "static-equipment",
        "phoenix-wave",
        "south-shrine",
        "indra",
        "blaze",
        "blaze-2",
        "blast-1",
    }
    selected = permitted | (set(families) & optional) if "all" in families else set(families)
    special_promotions = {"pgnt", "brn", "brgn", "mmnk", "sorc"}
    if selected & special_promotions and not selected <= special_promotions:
        raise ValueError("mount special promotions on a prepared ordinary promotion package")
    shrine = "south-shrine" in selected
    if shrine and (upstream is None or canonical_path is None):
        raise ValueError("south-shrine needs --canonical and --upstream")
    retained_promotions = {
        member: class_code
        for family, variants in retained_families.items()
        if family in selected
        for member, class_code in variants.items()
    }
    if retained_promotions and upstream is None:
        raise ValueError("Mounting promotion metadata requires the pinned --upstream checkout")
    parts = (
        ("pldn-party", "pldn-eric")
        if "pldn" in selected
        else tuple(part for part in ("pldn-party", "pldn-eric") if part in selected)
    )
    if not selected <= (permitted | {"pldn-party", "pldn-eric"} | optional):
        raise ValueError(
            "select walking, icons, sfx, music, pldn, pgnt, brn, wiz, melee, "
            "ranged, vicar, innate, ninja, monster, phoenix-wave, indra, south-shrine, "
            "blaze, blaze-2 or blast-1"
        )
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
        world = read_json(base / "world.json")
        scenes = {Path("main-scenes.json")} | {
            Path(row["battleScenes"]) for row in world.get("encounters", []) if row["battleScenes"]
        }
        spell_scenes = {}
        spell_families = selected & {"blaze", "blaze-2", "blast-1"}
        spell_only = selected == spell_families and bool(spell_families)
        if spell_families:
            for row in world["encounters"]:
                battle = read_json(files[Path(row["placement"])])["battle"]["id"]
                if 4 <= battle <= 7:
                    if battle in spell_scenes:
                        raise ValueError(f"duplicate attack-spell scene for Battle{battle:02d}")
                    spell_scenes[battle] = Path(row["battleScenes"])
            if set(spell_scenes) != {4, 5, 6, 7} or len(set(spell_scenes.values())) != 4:
                raise ValueError("attack-spell mount requires the four pre-voyage scenes")
        spell_paths = set(spell_scenes.values())
        if "blast-1" in spell_families:
            spell_paths.add(Path("world.json"))
        indra_only = selected == {"indra"}
        battle26_scene = Path("inputs/battle26/battle-scenes.json")
        for rel, source in files.items():
            if rel == Path("world.json") and not (indra_only or spell_only):
                continue
            target = dest / rel
            target.parent.mkdir(parents=True, exist_ok=True)
            if (
                (selected == {"south-shrine"} and rel not in scenes)
                or (selected == {"static-equipment"} and rel not in scenes)
                or ((indra_only or selected <= special_promotions) and rel != battle26_scene)
                or (spell_only and rel not in spell_paths)
            ):
                # Immutable external battle/audio/entry inputs. Rewritten scenes get private files.
                try:
                    os.link(source, target)
                    continue
                except OSError:
                    pass  # Different storage volumes require a real copy.
            shutil.copyfile(source, target)
        if spell_only:
            mount_attack_spells(root / "effects", Path(temp), dest, spell_scenes, spell_families)
            if "blast-1" in spell_families:
                mount_prepared_audio(root / "sfx/114", 114, dest, world["world"]["presentation"])
                (dest / "world.json").write_text(
                    json.dumps(world, separators=(",", ":")), encoding="utf-8"
                )
            dest.rename(destination)
            return
        presentation = world["world"]["presentation"]
        scene_path = dest / battle26_scene
        if scene_path.relative_to(dest) not in files:
            raise ValueError("selected package has no expected Battle26 consumer")
        scene = read_json(scene_path)
        if scene["encounter"] != "battle-26":
            raise ValueError("Battle26 selection differs")
        spell_icons = [7, 8, 9] if "icons" in selected else []
        if "ninja" in selected:
            spell_icons.extend((27, 28))
        if "sorc" in selected:
            spell_icons.extend((29, 30, 31, 32))
        if spell_icons:
            images = read_json(root / "icons/identity-icons.json")
            for i in spell_icons:
                for state in ("normal", "selected"):
                    name = f"identity-icon.spell.{i}.{state}"
                    for target in (presentation["identityIcons"], scene["rasters"]):
                        if target.setdefault(name, images[name]) != images[name]:
                            raise ValueError(f"conflicting mounted {name}")
        if "walking" in selected:
            sprites = read_json(root / "walking/index.json")["records"]["sprites"]
            mount_walking(root / "walking", world["world"], sprites, (6, 28, 34))
        promotions: dict[int, str] = {}
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
                promotions.update(dict.fromkeys(actor["members"], "PLDN"))
                if actor not in scene["actors"]:
                    scene["actors"].append(actor)
        if retained_promotions:
            mount_promotion_appearances(
                root, Path(temp), world["world"], scene, retained_promotions
            )
            promotions.update(retained_promotions)
        if "sorc" in selected:
            # Refresh the already admitted rod users from the shared weapon mount;
            # their existing progression remains owned by the base package.
            mount_promotion_appearances(
                root,
                Path(temp),
                world["world"],
                scene,
                {4: "WIZ", 17: "WIZ", 1: "VICR", 15: "VICR"},
                mount_field=False,
            )
        if "indra" in selected:
            mount_indra_staff(root, Path(temp), scene)
        if "static-equipment" in selected:
            mount_static_equipment(root, Path(temp), scene)
        if "monster" in selected:
            mount_monster_breath(root / "effects", Path(temp), scene)
        if "phoenix-wave" in selected:
            mount_phoenix_waves(root / "effects", Path(temp), scene)
        if upstream is not None and promotions:
            bind_promotion_progression(world, upstream, promotions)
        if "blast-1" in spell_families:
            mount_prepared_audio(root / "sfx/114", 114, dest, presentation)
        mounted_commands = list(commands or (25, 87, 99))
        if shrine and 9 not in mounted_commands:
            mounted_commands.append(9)
        for command in mounted_commands:
            family = "music" if command <= 64 else "sfx"
            if family not in selected and not (shrine and command == 9):
                continue
            directory = root / family / str(command)
            if not directory.is_dir():
                if any(
                    row["command"] == command and not is_authored_defeat(row)
                    for row in presentation["audio"]
                ):
                    continue
                entry_audio = read_json(base / "entry/entry.json")["presentation"]["audio"]
                available = [r for r in entry_audio if r["command"] == command]
                if available:
                    presentation["audio"].extend(available)
                    continue
            mount_prepared_audio(directory, command, dest, presentation)
        scene_path.write_text(json.dumps(scene, separators=(",", ":")), encoding="utf-8")
        if shrine:
            items = mount_south_shrine(world, root, upstream, canonical_path)
            bind_field_item_icons(presentation, root, dest, scenes, items)
        if "static-equipment" in selected:
            source = read_json(base / "opening-inputs/SF2_PRIVATE_STATIC_DATA.json")
            items = {
                row["id"]
                for row in source["items"]
                if set(row["itemTypeTokens"]) & {"WEAPON", "RING"}
            }
            bind_field_item_icons(presentation, root, dest, scenes, items)
        if spell_scenes:
            mount_attack_spells(root / "effects", Path(temp), dest, spell_scenes, spell_families)
        if not indra_only:
            (dest / "world.json").write_text(
                json.dumps(world, separators=(",", ":")), encoding="utf-8"
            )
        if (shrine or "static-equipment" in selected) and (
            dest / "world.json"
        ).stat().st_size > 128 * 1024 * 1024:
            raise ValueError("world exceeds the private reader limit")
        package_files(
            dest / "world.json",
            dest / "main-scenes.json",
            dest / "entry/entry.json",
            dest / "opening-inputs",
        )
        dest.rename(destination)


def write_hd_terrain(package: Path, visual: dict, relative: str) -> None:
    """Write the selected world-relative bank in a fresh staged package."""
    bank = {"map": visual["map"], "hdBlocks": visual.pop("hdBlocks")}
    if "hdBlockContexts" in visual:
        bank["hdBlockContexts"] = visual.pop("hdBlockContexts")
    path = package / relative
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("x", encoding="utf-8") as stream:
        json.dump(bank, stream, separators=(",", ":"), ensure_ascii=False)
    if path.stat().st_size > 8 * 1024 * 1024:
        raise ValueError("HD terrain exceeds the 8 MiB reader limit")
    visual["hdTerrainFile"] = relative


def externalize_hd_terrain(package: Path, map_id: str) -> None:
    """Move an existing inline bank in the staged copy without regenerating its rasters."""
    if not re.fullmatch(r"[a-z][a-z0-9-]{0,63}", map_id):
        raise ValueError("invalid map ID")
    world = package / "world.json"
    document = read_json(world)
    matches = [v for v in document["world"]["presentation"]["maps"] if v["map"] == map_id]
    if len(matches) != 1 or "hdBlocks" not in matches[0] or "hdTerrainFile" in matches[0]:
        raise ValueError("select one map with an inline HD bank")
    visual = matches[0]
    write_hd_terrain(package, visual, f"hd/terrain/{map_id}.json")
    write_json(world, document)
    if world.stat().st_size > 128 * 1024 * 1024:
        raise ValueError("world exceeds the private reader limit")


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ("world", "scene", "entry", "opening-inputs", "destination"):
        parser.add_argument(f"--{name}", type=Path, required=True)
    parser.add_argument(
        "--check", action="store_true", help="Compare existing output byte for byte"
    )
    parser.add_argument(
        "--externalize-hd-terrain",
        metavar="MAP",
        help="Move one inline map bank into the new package",
    )
    args = parser.parse_args()
    if args.check and args.externalize_hd_terrain is not None:
        parser.error("--externalize-hd-terrain requires a fresh copy")
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
            if args.externalize_hd_terrain is not None:
                externalize_hd_terrain(candidate, args.externalize_hd_terrain)
                files = package_files(
                    candidate / "world.json",
                    candidate / "main-scenes.json",
                    candidate / "entry/entry.json",
                    candidate / "opening-inputs",
                )
                size = sum(path.stat().st_size for path in files.values())
            candidate.rename(destination)
    print(json.dumps({"files": len(files), "bytes": size, "checked": args.check}))


if __name__ == "__main__":
    main()
