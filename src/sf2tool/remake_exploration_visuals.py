"""Private map presentation preparation from admitted source assets and ROM spans."""

from __future__ import annotations

import base64
import hashlib
import io
import json
import wave
from pathlib import Path
from typing import Any

from sf2tool.compression import decode_basic_compressed, decode_stack_compressed
from sf2tool.h2.battle_global_data import _arguments, _tokens
from sf2tool.h2.portraits import _read_animation_entries
from sf2tool.h2.variable_width_font import _assert_source_shape, _glyph_metadata, _parse_ascii_map
from sf2tool.remake_asset_build import (
    ACCEPTED_ROM_SHA256,
    PLAYER_PALETTE_ADDRESS,
    PLAYER_POINTER_TABLE_ADDRESS,
    _combine_player_halves,
    _render_player_frame,
)
from sf2tool.texture_extract import (
    decode_md_4bpp_tile,
    md_palette_color,
    palette_index_rgba,
    render_tileset_sheet,
)


def prepare_font(rom_path, upstream):
    # Pinned US extraction operands; variable-width-font-static owns the accepted
    # source/ROM comparison. Preparation does not load verification fixtures or H1 outputs.
    table = {
        "font_VariableWidth": 167938,
        "p_font_VariableWidth": 163852,
        "table_AsciiToTextSymbolMap": 26222,
    }
    hashes = {
        "fontSha256": "2B6866AF86381545496C58AC7C5B7B515B461AE4C642ECC790641C331B4EF55A",
        "pointerSha256": "F34C3F98D4F2678294FABFC07462D282708191B6C57E30363670E3A20F28FF68",
        "asciiMapSha256": "0644F0D663D7B3BBCE15C4C2A1A846D015CD1A0FB753CB7BE32E62AB3E8357B8",
    }
    rom = rom_path.read_bytes()

    def sha(value):
        return hashlib.sha256(value).hexdigest().upper()

    if sha(rom) != ACCEPTED_ROM_SHA256:
        raise ValueError("font ROM identity drift")
    _assert_source_shape(upstream / "disasm")
    address = table["font_VariableWidth"]
    font = rom[address : address + 2560]
    pointer = rom[table["p_font_VariableWidth"] : table["p_font_VariableWidth"] + 4]
    ascii_values = _parse_ascii_map(
        (upstream / "disasm/data/scripting/text/asciitotextsymbolmap.asm").read_text()
    )
    ascii_bytes = bytes(ascii_values)
    if (
        sha(font) != hashes["fontSha256"]
        or sha(pointer) != hashes["pointerSha256"]
        or int.from_bytes(pointer, "big") != address
        or sha(ascii_bytes) != hashes["asciiMapSha256"]
        or rom[table["table_AsciiToTextSymbolMap"] : table["table_AsciiToTextSymbolMap"] + 256]
        != ascii_bytes
    ):
        raise ValueError("font source/ROM identity drift")
    return {
        "asciiToSymbol": ascii_values,
        "advances": [row["advancePixels"] for row in _glyph_metadata(font, address)],
    }, sha(font)


def source_map_atlas(rom, definition):
    """Same five source tileset sheets as the existing atlas producer, at native scale."""
    # Pinned map-content, map-palette and map-tileset source/ROM pointer tables.
    if hashlib.sha256(rom).hexdigest().upper() != ACCEPTED_ROM_SHA256:
        raise ValueError("map atlas ROM identity drift")

    def pointer(address):
        return int.from_bytes(rom[address : address + 4], "big")

    header = pointer(609162 + definition["id"] * 4)
    if rom[header : header + 6] != bytes([definition["palette"], *definition["tilesets"]]):
        raise ValueError("map atlas header identity drift")
    if pointer(409604) != 608586 or pointer(409600) != 409612:
        raise ValueError("map atlas pointer table drift")
    palette_address = pointer(608586 + definition["palette"] * 4)
    palette = [
        md_palette_color(int.from_bytes(rom[i : i + 2], "big") & 0xEEE)
        for i in range(palette_address, palette_address + 32, 2)
    ]
    palette[0] = (0, 0, 0)
    pixels = []
    for index in definition["tilesets"]:
        if index == 255:
            pixels.extend([0] * (128 * 64 * 4))
            continue
        address = pointer(409612 + index * 4)
        data = decode_stack_compressed(rom[address:], expected_output_bytes=4096).output
        pixels.extend(
            render_tileset_sheet(
                [decode_md_4bpp_tile(data[i : i + 32]) for i in range(0, len(data), 32)], palette
            )
        )
    return bytes(pixels)


def prepare_visuals(
    compiler,
    canonical: dict[str, Any],
    maps: list[dict[str, Any]],
    rom_path: Path,
    asset_root: Path,
    manifest_sha256: str,
    atlas_bindings: dict[str, str],
    audio_candidates: tuple[Path, ...] = (),
    battles: tuple[int, ...] = (),
) -> dict[str, Any]:
    rom = rom_path.read_bytes()
    if hashlib.sha256(rom).hexdigest().upper() != canonical["romSha256"]:
        raise ValueError("visual source ROM identity mismatch")
    manifest_bytes = (asset_root / "manifests/presentation-assets-v1.json").read_bytes()
    if hashlib.sha256(manifest_bytes).hexdigest().upper() != manifest_sha256:
        raise ValueError("admitted presentation manifest identity mismatch")
    manifest = json.loads(manifest_bytes)
    assets = {asset["assetId"]: asset for asset in manifest["assets"]}

    def raster(width, height, data, format="rgba8"):
        data = bytes(data)
        return {
            "width": width,
            "height": height,
            "format": format,
            "data": base64.b64encode(data).decode("ascii"),
            "sha256": hashlib.sha256(data).hexdigest().upper(),
        }

    resources = {
        key: {row["id"]: row for row in rows} for key, rows in canonical["resources"].items()
    }
    map_definitions = {"map-" + str(row["id"]): row for row in canonical["maps"]}
    map_visuals = []
    sprites = {0}
    # PlayMapMusic's explicit original field-to-battle substitutions.
    music_replacements = {
        compiler.equates[name]: compiler.equates[target]
        for name, target in (
            ("MUSIC_NOTHING", "MUSIC_BATTLE_THEME_3"),
            ("MUSIC_TOWN", "MUSIC_BATTLE_THEME_3"),
            ("MUSIC_MITULA", "MUSIC_BATTLE_THEME_3"),
            ("MUSIC_MITULA_SHRINE", "MUSIC_BATTLE_THEME_1"),
            ("MUSIC_CASTLE", "MUSIC_BATTLE_THEME_1"),
        )
    }
    compiler.sources.add("disasm/code/gameflow/battle/battlemusic.asm")
    for map in maps:
        refs = map_definitions[map["id"]]["references"]
        if map["id"] in atlas_bindings:
            asset = assets[atlas_bindings[map["id"]]]
            bucket = next(bucket for bucket in asset["buckets"] if bucket["scale"] == 2)
            path = (asset_root / bucket["runtimePath"]).resolve(strict=True)
            if not path.is_relative_to(asset_root.resolve()):
                raise ValueError("presentation asset path escapes selected root")
            payload = path.read_bytes()
            if (
                len(payload) != bucket["byteLength"]
                or hashlib.sha256(payload).hexdigest().upper() != bucket["sha256"]
            ):
                raise ValueError("admitted raster identity mismatch")
            atlas = raster(bucket["width"], bucket["height"], payload, "png")
            scale = bucket["scale"]
        else:
            atlas = raster(128, 320, source_map_atlas(rom, map_definitions[map["id"]]))
            scale = 1
        map_visuals.append(
            {
                "map": map["id"],
                "music": [
                    {
                        "field": area["defaultMusic"],
                        "battle": music_replacements.get(
                            area["defaultMusic"], area["defaultMusic"]
                        ),
                    }
                    for area in resources["areaTables"][refs["areaTable"]]["records"]
                ],
                "atlas": atlas,
                "scale": scale,
                "blocks": resources["blocksets"][refs["blockset"]]["blocks"],
            }
        )
        for variant in [map, *map.get("setups", [])]:
            if "population" not in variant:
                continue
            sprites.update(
                entity["sprite"]
                for entity in variant["entities"]
                if entity["sprite"] >= variant["population"]["allyCount"]
            )
            sprites.update(row["sprite"] for row in variant["population"]["allySprites"])
            sprites.update(
                row["unjoinedSprite"]
                for row in variant["population"]["allySprites"]
                if row["unjoinedSprite"] is not None
            )
            sprites.update(
                row["sprite"]
                for row in variant["population"]["followers"]
                if row["character"] >= variant["population"]["allyCount"]
            )
    for program in compiler.programs.values():
        for instruction in program["instructions"]:
            if instruction["op"] in ("sprite", "new-entity"):
                sprites.add(instruction["sprite"])
            if instruction["op"] == "scene-entities":
                population = instruction["population"]
                sprites.update(
                    row["sprite"]
                    for row in instruction["entities"]
                    if row["sprite"] >= population["allyCount"]
                )
                sprites.update(row["sprite"] for row in population["allySprites"])
                sprites.update(
                    row["unjoinedSprite"]
                    for row in population["allySprites"]
                    if row["unjoinedSprite"] is not None
                )
                sprites.update(
                    row["sprite"]
                    for row in population["followers"]
                    if row["character"] >= population["allyCount"]
                )
    if battles:
        enemy_sprites_path = "disasm/data/stats/enemies/enemymapsprites.asm"
        compiler.sources.add(enemy_sprites_path)
        enemy_sprites = _arguments(
            (compiler.upstream / enemy_sprites_path).read_text(encoding="utf-8"), "mapsprite"
        )
        for battle in battles:
            path = f"disasm/data/battles/spritesets/spriteset{battle:02d}.asm"
            compiler.sources.add(path)
            for row in _arguments(
                (compiler.upstream / path).read_text(encoding="utf-8"), "enemyCombatant"
            ):
                enemy = compiler.number("ENEMY_" + _tokens(row)[0])
                sprites.add(compiler.number("MAPSPRITE_" + enemy_sprites[enemy]))
    properties_path = "disasm/data/spritedialogproperties.asm"
    compiler.sources.update(
        (
            properties_path,
            "disasm/data/graphics/mapsprites/entries.asm",
            "disasm/data/graphics/portraits/entries.asm",
            "disasm/code/common/scripting/entity/getentityportaitandspeechsfx.asm",
            "disasm/code/common/scripting/entity/entityfunctions_4.asm",
            "disasm/code/common/menus/portraitfunctions.asm",
        )
    )
    properties = (compiler.upstream / properties_path).read_text(encoding="utf-8")
    columns = [
        [compiler.number(prefix + value) for value in _arguments(properties, macro)]
        for prefix, macro in (
            ("MAPSPRITE_", "mapsprite"),
            ("PORTRAIT_", "portrait"),
            ("SFX_", "speechSfx"),
        )
    ]
    dialogue = {
        sprite: (portrait if portrait < 128 else None, speech)
        for sprite, portrait, speech in zip(*columns, strict=True)
    }
    palette = [
        md_palette_color(int.from_bytes(rom[offset : offset + 2], "big"))
        for offset in range(PLAYER_PALETTE_ADDRESS, PLAYER_PALETTE_ADDRESS + 32, 2)
    ]
    sprite_visuals = []
    for sprite in sorted(sprites):
        directions = []
        for direction in range(3):
            entry = PLAYER_POINTER_TABLE_ADDRESS + (sprite * 3 + direction) * 4
            address = int.from_bytes(rom[entry : entry + 4], "big")
            decoded = decode_basic_compressed(rom[address:], expected_output_bytes=576).output
            directions.append(
                raster(
                    48,
                    24,
                    _combine_player_halves(
                        _render_player_frame(decoded[:288], palette),
                        _render_player_frame(decoded[288:], palette),
                    ),
                )
            )
        portrait, speech = dialogue.get(sprite, (None, compiler.equates["SFX_DIALOG_BLEEP_6"]))
        sprite_visuals.append(
            {"sprite": sprite, "directions": directions, "portrait": portrait, "speech": speech}
        )
    portrait_visuals = []
    for portrait in sorted(
        {row["portrait"] for row in sprite_visuals if row["portrait"] is not None}
    ):
        # pt_Portraits: data/graphics/portraits/entries.asm, accepted pointer table at 0x1C8004.
        entry = 0x1C8004 + portrait * 4
        address = int.from_bytes(rom[entry : entry + 4], "big")
        data = rom[address:]
        eyes, offset = _read_animation_entries(data, 0, "eye")
        mouth, offset = _read_animation_entries(data, offset, "mouth")
        palette = [
            md_palette_color(int.from_bytes(data[index : index + 2], "big"))
            for index in range(offset, offset + 32, 2)
        ]
        decoded = decode_stack_compressed(data[offset + 32 :], expected_output_bytes=2048).output
        pixels = bytearray(64 * 64 * 4)
        for tile_id in range(64):
            tile = decode_md_4bpp_tile(decoded[tile_id * 32 : (tile_id + 1) * 32])
            for y in range(8):
                for x in range(8):
                    target = ((tile_id // 8 * 8 + y) * 64 + tile_id % 8 * 8 + x) * 4
                    pixels[target : target + 4] = bytes(
                        palette_index_rgba(palette, tile[y * 8 + x])
                    )
        portrait_visuals.append(
            {"portrait": portrait, "raster": raster(64, 64, pixels), "eyes": eyes, "mouth": mouth}
        )
    audio = []
    selected_audio = []
    for asset in manifest["assets"]:
        if asset["kind"] != "audio":
            continue
        path = (asset_root / asset["runtime"]["runtimePath"]).resolve(strict=True)
        if not path.is_relative_to(asset_root.resolve()):
            raise ValueError("audio asset path escapes selected root")
        selected_audio.append((asset, path))
    for candidate in audio_candidates:
        asset = json.loads(candidate.read_text(encoding="utf-8"))["asset"]
        runtime_path = Path(asset["runtime"]["runtimePath"])
        if (
            asset["kind"] != "audio"
            or len(runtime_path.parts) != 3
            or runtime_path.parts[:2] != ("runtime", "audio")
        ):
            raise ValueError("expected a reviewed remake_audio candidate")
        path = (candidate.parent / runtime_path.name).resolve(strict=True)
        if not path.is_relative_to(candidate.parent.resolve()):
            raise ValueError("audio candidate path escapes selected root")
        selected_audio.append((asset, path))
    cues, identities = set(), set()
    for asset, path in selected_audio:
        identity = (asset["command"], asset["timerB"])
        if asset["cue"] in cues or identity in identities:
            raise ValueError("duplicate selected audio cue or command/timer")
        cues.add(asset["cue"])
        identities.add(identity)
        runtime = asset["runtime"]
        payload = path.read_bytes()
        if (
            len(payload) != runtime["byteLength"]
            or hashlib.sha256(payload).hexdigest().upper() != runtime["sha256"].upper()
        ):
            raise ValueError("admitted audio identity mismatch")
        with wave.open(io.BytesIO(payload), "rb") as sound:
            if (
                sound.getsampwidth() != 2
                or sound.getcomptype() != "NONE"
                or sound.getnchannels() != runtime["channels"]
                or sound.getframerate() != runtime["sampleRate"]
                or sound.getnframes() != runtime["sampleFrames"]
            ):
                raise ValueError("admitted audio format mismatch")
            pcm = sound.readframes(sound.getnframes())
            if len(pcm) != sound.getnframes() * sound.getnchannels() * 2:
                raise ValueError("admitted audio is truncated")
        audio.append(
            {
                "cue": asset["cue"],
                "command": asset["command"],
                "timerB": asset["timerB"],
                "sampleRate": runtime["sampleRate"],
                "channels": runtime["channels"],
                "sampleFrames": runtime["sampleFrames"],
                "loopBegin": runtime["loopBegin"],
                "loopEnd": runtime["loopEnd"],
                "pcm16": base64.b64encode(pcm).decode("ascii"),
                "sha256": hashlib.sha256(pcm).hexdigest().upper(),
                # Accepted modern step clock; this is not a PCM/VInt conversion.
                **({"modernEndStep": 505} if asset["command"] == 19 else {}),
            }
        )
    if not audio:
        raise ValueError("private original audio is missing from the selected asset pack")
    return {
        "maps": map_visuals,
        "sprites": sprite_visuals,
        "portraits": portrait_visuals,
        "audio": audio,
    }
