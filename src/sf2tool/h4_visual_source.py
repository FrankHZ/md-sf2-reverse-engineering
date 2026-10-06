"""Selected visual source IO; the shared C# child owns qualification predicates."""

from __future__ import annotations

import base64
import hashlib
import io
import os
import subprocess
import sys
from types import SimpleNamespace

from sf2tool.h4_dotnet import ResourceProcessError, _selected
from sf2tool.h4_inventory import _pack
from sf2tool.h4_materials import _measure
from sf2tool.paths import repo_path

_BOOLEAN = {
    "visual-binding",
    "visual-git",
    "visual-tileset",
    "visual-atlas-bytes",
    "visual-field-bytes",
}
_EMPTY = {
    "visual-start",
    "visual-provenance",
    "visual-pins",
    "visual-canonical",
    "visual-definitions",
    "visual-layout",
    "visual-palette",
    "visual-metadata",
    "visual-atlas-digest",
    "visual-atlas-png",
    "visual-atlas-finish",
    "visual-word",
    "visual-field-start",
    "visual-field-allies",
    "visual-field-enemy",
    "visual-field-span",
    "visual-field-digest",
    "visual-field-finish",
    "visual-cancel",
}


def validate_reply(operation, result):
    if isinstance(result, dict) and set(result) == {"sourceRead"}:
        row = result["sourceRead"]
        if (
            isinstance(row, dict)
            and set(row) == {"sequence", "source", "action", "index", "cursor"}
            and type(row["sequence"]) is int and row["sequence"] > 0
            and type(row["source"]) is int and row["source"] >= 0
            and row["action"] in ("length", "next", "index")
            and (type(row["index"]) is int or row["action"] == "index"
                 and isinstance(row["index"], str))
            and type(row["cursor"]) is int and row["cursor"] >= 0
            and (row["action"] != "next" or row["index"] >= 0 and row["cursor"] > 0)
            and (row["action"] == "next" or row["cursor"] == 0)
            and (row["action"] != "length" or row["index"] == 0)
        ):
            return
        raise ResourceProcessError("Malformed visual source read request")
    valid = (
        operation in _BOOLEAN
        and type(result) is bool
        or operation in _EMPTY
        and result is None
        or operation == "visual-read" and (result is None or type(result) is bool)
        or operation in ("visual-select", "visual-read")
        and isinstance(result, list)
        and len(result) <= 256
        and all(type(value) is bool for value in result)
    )
    if not valid:
        raise ResourceProcessError(f"Malformed H4 visual-source reply for {operation}")


def _digest(data):
    return hashlib.sha256(data).hexdigest().upper()


def _path(path):
    return path.resolve() if path.is_absolute() else repo_path(path)


def _visual(value):
    """Keep every equality field, replacing only encoded atlas text by measured facts."""
    header, text = value, None
    if isinstance(value, dict):
        header = dict(value)
        atlas = header.get("atlas")
        if isinstance(atlas, dict) and isinstance(atlas.get("data"), str):
            header["atlas"] = dict(atlas)
            text = header["atlas"]["data"]
            header["atlas"]["data"] = None
    return dict(header=header, text=text is not None), text


class VisualSources:
    def __init__(self, resources, read, pins, sequence_type):
        self.resources, self.read, self.pins = resources, read, pins
        self.sequence_type = sequence_type
        self.sequences, self.sequence_ids = [], {}

    def operands(self, value):
        references = []

        def visit(value, path):
            if isinstance(value, self.sequence_type):
                index = self.sequence_ids.get(id(value))
                if index is None:
                    index = len(self.sequences)
                    self.sequence_ids[id(value)] = index
                    self.sequences.append(value)
                references.append(dict(path=path, source=index, type=type(value).__name__))
                return None
            if isinstance(value, dict):
                return {key: visit(item, [*path, key]) for key, item in value.items()}
            if isinstance(value, list):
                return [visit(item, [*path, i]) for i, item in enumerate(value)]
            return value

        return dict(value=visit(value, []), references=references)

    def send(self, operation, **values):
        self.resources.flush_checks()
        # Start has only fixed primitive configuration; subsequent source operands may
        # contain the caller's bounded sequences, including nested unused fields.
        message = dict(op="visual-" + operation, **values) if operation == "start" else dict(
            op="visual-" + operation, operands=self.operands(values)
        )
        iterators, positions = {}, {}
        sequence = 0
        try:
            result = self.resources.exchange(message)
            while isinstance(result, dict):
                request = result["sourceRead"]
                sequence += 1
                index = request["source"]
                if request["sequence"] != sequence or index >= len(self.sequences):
                    raise ResourceProcessError("Visual source read sequence/reference changed")
                source, action = self.sequences[index], request["action"]
                try:
                    if action == "length":
                        value = len(source)
                    elif action == "index":
                        value = source[request["index"]]
                    else:
                        cursor = request["cursor"]
                        if positions.get(cursor, (index, 0)) != (index, request["index"]):
                            raise ResourceProcessError("Visual source iterator position changed")
                        if cursor not in iterators:
                            iterators[cursor] = iter(source)
                        try:
                            value = dict(done=False, value=next(iterators[cursor]))
                        except StopIteration:
                            value = dict(done=True, value=None)
                        positions[cursor] = (index, None if value["done"] else request["index"] + 1)
                except (KeyError, IndexError, TypeError, ValueError, AttributeError,
                        OverflowError, OSError):
                    self.resources.exchange(dict(op="visual-cancel"))
                    raise
                result = self.resources.exchange(dict(
                    op="visual-read", sequence=sequence, value=self.operands(value)
                ))
            validate_reply("visual-" + operation, result)
            return result
        finally:
            for iterator in iterators.values():
                close = getattr(iterator, "close", None)
                if close is not None:
                    close()

    def select(self, rows, key):
        # Select each source header before requesting the next source row. Layouts
        # may be lazy, and neither selection nor transport should read them here.
        for row in rows:
            admitted = self.send("select", key=key, rows=[_selected(row, (key,))])
            if len(admitted) != 1:
                raise ResourceProcessError("Visual source admission count changed")
            if admitted[0]:
                yield row

    def prepare(
        self,
        selection,
        source_root,
        canonical_content,
        tileset_metadata,
        palette_metadata,
        scope,
        source_only,
        enabled,
        budget,
        private_bytes,
    ):
        from sf2tool.h2.map_import import MANIFEST, _canonical_bytes
        from sf2tool.private_inputs import ROM_INPUT_IDENTITY, private_input_path
        from sf2tool.remake_asset_build import (
            _MAP3_ATLAS,
            _MAP19_20_ATLAS,
            _MAP21_ATLAS,
            _MAP40_ATLAS,
            _MAP57_ATLAS,
            ACCEPTED_PALETTE_METADATA_SHA256,
            ACCEPTED_TILESET_METADATA_SHA256,
            _build_world_atlas_source,
            _scale_rgba_nearest,
        )
        from sf2tool.remake_exploration_content import OriginalPrograms, prepare_visuals
        from sf2tool.texture_extract import write_png_rgba

        world_path, scene_path, process_path, scene_root, asset_root = [
            _path(p) for p in selection[:5]
        ]
        source_root = _path(source_root)
        document, scene, process = (
            self.read(world_path),
            self.read(scene_path),
            self.read(process_path),
        )
        world, presentation = document["world"], document["world"]["presentation"]
        selected_maps = set(scope["maps"]) if scope is not None and not source_only else None
        self.send(
            "start",
            pins=dict(
                self.pins,
                tileset=ACCEPTED_TILESET_METADATA_SHA256,
                palette=ACCEPTED_PALETTE_METADATA_SHA256,
            ),
            selectedMaps=list(selected_maps) if selected_maps is not None else None,
            integerDigitLimit=sys.get_int_max_str_digits(),
        )
        world_maps = list(self.select(world["maps"], "id"))
        for key, path in (
            ("SF2_PRIVATE_EXPLORATION_CONTENT", world_path),
            ("SF2_PRIVATE_BATTLE_SCENE_CONTENT", scene_path),
        ):
            if not self.send(
                "binding",
                actual=os.path.normcase(str(repo_path(process["selectedInputs"][key]).resolve())),
                expected=os.path.normcase(str(path)),
            ):
                break
        self.send(
            "provenance", provenance=_selected(document["provenance"], ("commit", "romSha256"))
        )
        head = subprocess.check_output(
            ["git", "-C", str(source_root), "rev-parse", "HEAD"], text=True
        ).strip()
        code = subprocess.run(
            [
                "git",
                "-C",
                str(source_root),
                "diff",
                "--quiet",
                self.pins["upstream"],
                "--",
                "disasm",
            ],
            check=False,
        ).returncode
        hash_rom = self.send("git", head=head, returncode=code)
        rom_path = private_input_path(ROM_INPUT_IDENTITY)
        rom = rom_path.read_bytes()
        self.send("pins", digest=_digest(rom) if hash_rom else None)
        canonical_content = _path(canonical_content)
        canonical = self.read(canonical_content)
        self.send(
            "canonical",
            digest=_digest(_canonical_bytes(canonical)),
            manifest=_selected(self.read(MANIFEST), ("outputSha256",)),
        )
        manifest = self.read(asset_root / "manifests/presentation-assets-v1.json")
        assets = {a["assetId"]: a for a in manifest["assets"]}
        families = {
            m: family
            for family in (_MAP3_ATLAS, _MAP19_20_ATLAS, _MAP21_ATLAS, _MAP40_ATLAS, _MAP57_ATLAS)
            for m in family.map_indices
        }
        atlas_bindings = {
            row["id"]: families[int(row["id"].split("-")[-1])].asset_id for row in world_maps
        }
        compiler = OriginalPrograms(canonical, source_root)
        compiler.programs = {p["id"]: p for p in world["programs"]}
        expected = prepare_visuals(
            compiler, canonical, world_maps, rom_path, asset_root, selection[7], atlas_bindings
        )
        maps = {m["map"]: m for m in self.select(presentation["maps"], "map")}
        source_maps = {m["map"]: m for m in expected["maps"]}
        sprites = {m["sprite"]: m for m in presentation["sprites"]}
        source_sprites = {m["sprite"]: m for m in expected["sprites"]}
        portraits = {m["portrait"]: m for m in presentation["portraits"]}
        source_portraits = {m["portrait"]: m for m in expected["portraits"]}
        canonical_maps = {m["id"]: m for m in canonical["maps"]}
        canonical_layouts = {m["id"]: m for m in canonical["resources"]["layouts"]}
        self.indices = {name: i for i, name in enumerate(maps)}

        def definitions():
            for name, visual in maps.items():
                actual_header, actual_text = _visual(visual)
                expected_header, expected_text = _visual(source_maps.get(name))
                yield dict(
                    index=self.indices[name],
                    actual=actual_header,
                    expected=expected_header,
                    data=_measure(actual_text, expected_text)
                    if actual_text is not None and expected_text is not None
                    else None,
                )

        for definition in definitions():
            self.send("definitions", rows=[definition])
        for row in world_maps if "map" in enabled else ():
            original = canonical_maps[int(row["id"].split("-")[-1])]
            layout = canonical_layouts[original["references"]["layout"]]
            self.send(
                "layout",
                row=_pack(_selected(row, ("id", "layout"))),
                layout=_selected(layout, ("words",)),
            )
        tileset_metadata, palette_metadata = _path(tileset_metadata), _path(palette_metadata)
        tilesets, palettes = self.read(tileset_metadata), self.read(palette_metadata)
        if self.send("tileset", digest=_digest(tileset_metadata.read_bytes())):
            self.send("palette", digest=_digest(palette_metadata.read_bytes()))
        self.send("metadata")
        decoded = source_bytes = png = encoded = None
        for name, visual in maps.items() if "map" in enabled else ():
            family = families[int(name.split("-")[-1])]
            decoded = _build_world_atlas_source(rom, tilesets, palettes, family)
            source_bytes = (asset_root / family.source_file).read_bytes()
            png = base64.b64decode(visual["atlas"]["data"], validate=True)
            encoded = io.BytesIO()
            write_png_rgba(
                SimpleNamespace(write_bytes=encoded.write),
                128 * visual["scale"],
                320 * visual["scale"],
                _scale_rgba_nearest(decoded.rgba_pixels, 128, 320, visual["scale"]),
            )
            if self.send("atlas-bytes", measurement=_measure(source_bytes, decoded.source_bundle)):
                self.send(
                    "atlas-digest",
                    digest=_digest(source_bytes),
                    asset=_selected(assets[family.asset_id], ("source",)),
                )
            self.send("atlas-png", measurement=_measure(png, encoded.getvalue()))
            self.send("atlas-finish", name=name, index=self.indices[name])
        self.state = SimpleNamespace(
            world=world,
            process=process,
            maps=maps,
            sprites=sprites,
            source_sprites=source_sprites,
            portraits=portraits,
            source_portraits=source_portraits,
        )
        self.scene_path, self.scene, self.rom, self.compiler = scene_path, scene, rom, compiler
        # Keep prepared sources alive through the caller and its source-loaded baseline.
        self.loaded_sources = (
            document,
            presentation,
            world_maps,
            canonical,
            manifest,
            assets,
            families,
            atlas_bindings,
            expected,
            source_maps,
            canonical_maps,
            canonical_layouts,
            tilesets,
            palettes,
            decoded,
            source_bytes,
            png,
            encoded,
        )
        if budget is not None:
            budget.checkpoint("source loaded", force=True)
        self.source_only_private = private_bytes() if source_only else None
        return self

    def word(self, required, weight, locator):
        name = required["identity"]["map"]
        self.state.maps[name]  # Keep the existing native dictionary lookup/absence boundary.
        self.send(
            "word",
            index=self.indices[name],
            want=_selected(required["expected"], ("block", "tile", "word")),
            weight=weight,
            locator=locator,
        )

    def field_death(self):
        from sf2tool.compression import decode_basic_compressed
        from sf2tool.remake_asset_build import (
            PLAYER_PALETTE_ADDRESS,
            PLAYER_POINTER_TABLE_ADDRESS,
            _combine_player_halves,
            _render_player_frame,
        )
        from sf2tool.texture_extract import md_palette_color

        field = self.read(self.scene_path.parent / "field-death-provenance.json")
        self.send(
            "field-start", field=_selected(field, ("upstreamCommit", "romSha256", "effectSprite"))
        )
        assignments = field["allyAssignments"]
        self.send(
            "field-allies",
            actual=assignments,
            expected=[
                _selected(row, ("character", "sprite"))
                for row in self.compiler.initial_ally_sprites()[:3]
            ],
        )
        self.send(
            "field-enemy",
            value=self.rom[field["enemyTableAddress"] + field["enemyId"]],
            field=_selected(field, ("enemySprite",)),
        )
        palette = [
            md_palette_color(int.from_bytes(self.rom[i : i + 2], "big"))
            for i in range(PLAYER_PALETTE_ADDRESS, PLAYER_PALETTE_ADDRESS + 32, 2)
        ]
        for direction, span in enumerate(field["spans"]):
            entry = PLAYER_POINTER_TABLE_ADDRESS + (63 * 3 + direction) * 4
            address = int.from_bytes(self.rom[entry : entry + 4], "big")
            decoded = decode_basic_compressed(self.rom[address:], expected_output_bytes=576)
            pixels = bytes(
                _combine_player_halves(
                    _render_player_frame(decoded.output[:288], palette),
                    _render_player_frame(decoded.output[288:], palette),
                )
            )
            raster = self.scene["rasters"][self.scene["fieldDeath"]["exitFrames"][direction]]
            self.send(
                "field-span",
                span=span,
                facts=dict(
                    pointerAddress=entry, address=address, byteLength=decoded.input_bytes_consumed
                ),
            )
            decoded_raster = base64.b64decode(raster["data"], validate=True)
            if self.send("field-bytes", measurement=_measure(decoded_raster, pixels)):
                self.send(
                    "field-digest", digest=_digest(pixels), raster=_selected(raster, ("sha256",))
                )
        self.send("field-finish")
