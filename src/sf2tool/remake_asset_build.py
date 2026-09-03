"""Deterministic ignored candidate builders for the private presentation pack."""

from __future__ import annotations

import argparse
import hashlib
import json
import os
import re
import shutil
import stat
import struct
import sys
import uuid
import xml.etree.ElementTree as element_tree
import zipfile
import zlib
from collections.abc import Callable, Mapping
from contextlib import suppress
from dataclasses import dataclass
from pathlib import Path, PurePosixPath

from jsonschema import Draft202012Validator
from jsonschema.exceptions import SchemaError, ValidationError

from sf2tool.compression import (
    BasicDecodeResult,
    StackDecodeResult,
    decode_basic_compressed,
    decode_stack_compressed,
)
from sf2tool.paths import repo_path
from sf2tool.remake_assets import (
    MANIFEST_RELATIVE_PATH,
    PACK_CAPABILITY,
    PACK_SCHEMA,
    PACKAGE_ID,
    PROFILE,
    REPOSITORY_ID,
    AssetPreflightError,
    validate_asset_checkout_identity,
)
from sf2tool.remake_godot import ProcessReceipt, run_bounded_process
from sf2tool.texture_extract import (
    TILE_BYTES_4BPP,
    decode_md_4bpp_tile,
    md_palette_color,
    palette_index_rgba,
    render_tileset_sheet,
    write_png_rgba,
)

DEFAULT_TOOLCHAIN_MANIFEST = repo_path("remake/presentation-toolchain.json")
BUILD_CAPABILITY = "private-local-presentation-hud-svg-candidate-build-v1"
WORLD_BUILD_CAPABILITY = "private-local-map3-base-tileset-atlas-candidate-build-v1"
WORLD_ASSET_ID = "world.map3.base-tileset-atlas"
WORLD_SOURCE_ASSET_ID = "source.world.map3.base-visual-selection"
WORLD_POLICY_ID = "private-local-map3-base-nearest-rgba8-v1"
WORLD_GENERATOR_ID = "sf2tool-remake-asset-build"
WORLD_GENERATOR_VERSION = "1"
WORLD_GENERATOR_FINGERPRINT_VERSION = "sf2-remake-generator-fingerprint-v1"
WORLD_GENERATOR_COMPONENTS = (
    "src/sf2tool/remake_asset_build.py",
    "src/sf2tool/compression.py",
    "src/sf2tool/texture_extract.py",
)
WORLD_SOURCE_FILE = "source/world/map3/base-visual-selection-v1.bin"
WORLD_MASTER_FILE = "masters/world/map3/base-tileset-atlas.png"
WORLD_RUNTIME_TEMPLATE = "runtime/world/map3/base-tileset-atlas@{scale}x.png"
WORLD_SOURCE_MAGIC = b"SF2-MAP3-BASE-VISUAL-SELECTION-V1\x00"
ACCEPTED_ROM_SHA256 = "9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9"
ACCEPTED_ROM_SIZE = 2_097_152
ACCEPTED_TILESET_METADATA_SHA256 = (
    "2EA6AB3485CAE4F92F31647C05233F0E1C07E81CCB02806706A51F9F0C1E087F"
)
ACCEPTED_TILESET_METADATA_SIZE = 102_965
ACCEPTED_PALETTE_METADATA_SHA256 = (
    "4F977B4B3EB8E731D2ABB6664F36030487DC186D267E66E9C2DAF3CB211007AB"
)
ACCEPTED_PALETTE_METADATA_SIZE = 20_554
ACCEPTED_UPSTREAM_REPOSITORY = "https://github.com/ShiningForceCentral/SF2DISASM.git"
ACCEPTED_UPSTREAM_COMMIT = "c834c652b6862bc5679fd7f69a38a7093206efc6"
ACCEPTED_TILESET_METADATA_ID = "sf2-map-tileset-decode-v1"
ACCEPTED_PALETTE_METADATA_ID = "sf2-map-palette-static-v1"
ACCEPTED_MAP_INDEX = 3
ACCEPTED_PALETTE_INDEX = 0
ACCEPTED_TILESET_SLOTS = (0, 37, 43, 53, 66)
WORLD_SCALES = (2, 4)
WORLD_TILESET_COUNT = 5
WORLD_DECODED_BYTES_PER_TILESET = 4_096
WORLD_TILES_PER_TILESET = 128
WORLD_TILE_GRID_COLUMNS = 16
WORLD_TILE_GRID_ROWS = 8
WORLD_TILE_PIXEL_SIZE = 8
WORLD_SHEET_WIDTH = WORLD_TILE_GRID_COLUMNS * WORLD_TILE_PIXEL_SIZE
WORLD_SHEET_HEIGHT = WORLD_TILE_GRID_ROWS * WORLD_TILE_PIXEL_SIZE
WORLD_ATLAS_WIDTH = WORLD_SHEET_WIDTH
WORLD_ATLAS_HEIGHT = WORLD_SHEET_HEIGHT * WORLD_TILESET_COUNT
WORLD_PALETTE_WORD_COUNT = 16
WORLD_PALETTE_BYTES = WORLD_PALETTE_WORD_COUNT * 2
WORLD_PALETTE_MASK = 0x0EEE
WORLD_MAXIMUM_PNG_BYTES = 32 * 1024 * 1024
PLAYER_BUILD_CAPABILITY = (
    "private-local-map3-original-player-initial-reference-frame-candidate-build-v1"
)
PLAYER_ASSET_ID = "world.map3.player.initial-reference-frame"
PLAYER_SOURCE_ASSET_ID = "source.world.map3.player.initial-reference-frame"
PLAYER_POLICY_ID = "private-local-map3-player-reference-nearest-rgba8-v1"
PLAYER_SOURCE_FILE = "source/world/map3/player-initial-reference-frame-v1.bin"
PLAYER_MASTER_FILE = "masters/world/map3/player-initial-reference-frame.png"
PLAYER_RUNTIME_TEMPLATE = "runtime/world/map3/player-initial-reference-frame@{scale}x.png"
PLAYER_SOURCE_MAGIC = b"SF2-MAP3-PLAYER-INITIAL-REFERENCE-FRAME-V1\x00"
PLAYER_MAPSPRITE_ID = 0
PLAYER_SOURCE_SLOT = 2
PLAYER_SELECTED_HALF = 0
PLAYER_POINTER_TABLE_ADDRESS = 819_200
PLAYER_SELECTED_PAYLOAD_ADDRESS = 822_782
PLAYER_PALETTE_ADDRESS = 12_446
PLAYER_DECODED_BYTES = 576
PLAYER_FRAME_BYTES = 288
PLAYER_FRAME_TILE_COUNT = 9
PLAYER_FRAME_TILES_PER_SIDE = 3
PLAYER_FRAME_WIDTH = 24
PLAYER_FRAME_HEIGHT = 24
_TILESET_ROOT_FIELDS = {
    "schemaVersion",
    "id",
    "upstream",
    "romSha256",
    "function",
    "table",
    "summary",
    "unusedTilesetIndices",
    "animationTileCountDistribution",
    "tilesets",
    "maps",
    "animations",
    "runtimeQuestions",
}
_TILESET_FIELDS = {
    "index",
    "symbol",
    "sourcePath",
    "sourceAddress",
    "compressedBytes",
    "decodedBytes",
    "sourceSha256",
    "decodedSha256",
    "inputBitsConsumed",
    "trailingBits",
    "commandGroupCount",
    "literalWordCount",
    "copyCommandCount",
    "copiedWordCount",
    "maximumCopyOffsetWords",
    "maximumCopyLengthWords",
}
_TILESET_MAP_FIELDS = {"mapIndex", "sourcePath", "mapAddress", "paletteIndex", "tilesetSlots"}
_PALETTE_ROOT_FIELDS = {
    "schemaVersion",
    "id",
    "upstream",
    "romSha256",
    "function",
    "table",
    "summary",
    "usageCounts",
    "palettes",
    "maps",
    "runtimeQuestions",
}
_PALETTE_FIELDS = {
    "index",
    "symbol",
    "sourcePath",
    "sourceAddress",
    "byteCount",
    "colorCount",
    "sourceFirstColor",
    "effectiveFirstColor",
    "sourceSha256",
    "effectiveSha256",
}
_PALETTE_MAP_FIELDS = {"mapIndex", "sourcePath", "mapAddress", "paletteIndex"}
SVG_NAMESPACE = "http://www.w3.org/2000/svg"
PNG_SIGNATURE = b"\x89PNG\r\n\x1a\n"
_STAGING_PREFIX = ".sf2-hud-svg-build-"
_WORLD_STAGING_PREFIX = ".sf2-map3-world-atlas-build-"
_PLAYER_STAGING_PREFIX = ".sf2-map3-player-reference-build-"
_ASSET_ID_PATTERN = re.compile(r"^hud\.([a-z0-9]+(?:-[a-z0-9]+)*)$")
_CANDIDATE_NAME_PATTERN = re.compile(r"^[a-z0-9](?:[a-z0-9-]{0,62}[a-z0-9])?$")
_ELEMENT_ID_PATTERN = re.compile(r"^[A-Za-z][A-Za-z0-9._-]{0,127}$")
_INTEGER_PATTERN = re.compile(r"^[1-9][0-9]*$")
_NUMBER_PATTERN = re.compile(r"^-?(?:0|[1-9][0-9]*)(?:\.[0-9]+)?$")
_COLOR_PATTERN = re.compile(r"^(?:none|#[0-9A-Fa-f]{6}|#[0-9A-Fa-f]{8})$")
_PATH_DATA_PATTERN = re.compile(r"^[MmLlHhVvCcSsQqTtAaZz0-9+,. eE-]+$")
_POINTS_PATTERN = re.compile(r"^[0-9+,. eE-]+$")
_WINDOWS_RESERVED = {
    "con",
    "prn",
    "aux",
    "nul",
    "clock$",
    *(f"com{value}" for value in range(1, 10)),
    *(f"lpt{value}" for value in range(1, 10)),
}
_COMMON_ATTRIBUTES = {
    "id",
    "fill",
    "stroke",
    "stroke-width",
    "opacity",
    "fill-opacity",
    "stroke-opacity",
    "fill-rule",
    "stroke-linecap",
    "stroke-linejoin",
    "stroke-miterlimit",
}
_ELEMENT_ATTRIBUTES = {
    "svg": {"id", "width", "height", "viewBox"},
    "g": _COMMON_ATTRIBUTES,
    "path": _COMMON_ATTRIBUTES | {"d"},
    "rect": _COMMON_ATTRIBUTES | {"x", "y", "width", "height", "rx", "ry"},
    "circle": _COMMON_ATTRIBUTES | {"cx", "cy", "r"},
    "ellipse": _COMMON_ATTRIBUTES | {"cx", "cy", "rx", "ry"},
    "line": _COMMON_ATTRIBUTES | {"x1", "y1", "x2", "y2"},
    "polyline": _COMMON_ATTRIBUTES | {"points"},
    "polygon": _COMMON_ATTRIBUTES | {"points"},
}
_PAINTABLE_ELEMENTS = frozenset(
    {"path", "rect", "circle", "ellipse", "line", "polyline", "polygon"}
)
_NUMERIC_ATTRIBUTES = {
    "x",
    "y",
    "width",
    "height",
    "rx",
    "ry",
    "cx",
    "cy",
    "r",
    "x1",
    "y1",
    "x2",
    "y2",
    "stroke-width",
    "opacity",
    "fill-opacity",
    "stroke-opacity",
    "stroke-miterlimit",
}


class AssetBuildError(ValueError):
    """A typed build rejection that never embeds a machine path."""

    def __init__(self, code: str, field: str, message: str) -> None:
        super().__init__(message)
        self.code = code
        self.field = field
        self.message = message

    def as_dict(self) -> dict[str, str]:
        return {"code": self.code, "field": self.field, "message": self.message}


def _reject(code: str, field: str, message: str) -> AssetBuildError:
    return AssetBuildError(code, field, message)


@dataclass(frozen=True)
class ArchiveIdentity:
    file_name: str
    url: str
    sha256: str
    size: int
    member: str
    maximum_member_bytes: int


@dataclass(frozen=True)
class BuildPolicy:
    policy_id: str
    scales: tuple[int, int]
    shape_rendering: str
    text_rendering: str
    image_rendering: str
    filter: str
    mipmaps: bool
    repeat: bool
    color_space: str
    alpha_mode: str


@dataclass(frozen=True)
class BuildLimits:
    maximum_master_bytes: int
    maximum_logical_dimension: int
    maximum_png_bytes: int
    maximum_svg_elements: int
    maximum_svg_depth: int
    version_seconds: int
    rasterize_seconds: int
    termination_seconds: int
    reap_seconds: int


@dataclass(frozen=True)
class PresentationToolchain:
    release_repository: str
    release_tag: str
    generator_id: str
    generator_version: str
    version_output: str
    archive: ArchiveIdentity
    policy: BuildPolicy
    limits: BuildLimits


@dataclass(frozen=True)
class SvgMaster:
    width: int
    height: int
    sha256: str
    data: bytes


@dataclass(frozen=True)
class PngIdentity:
    width: int
    height: int
    byte_length: int
    sha256: str


@dataclass(frozen=True)
class WorldTilesetRecord:
    index: int
    source_address: int
    compressed_bytes: int
    source_sha256: str
    decoded_sha256: str
    input_bits_consumed: int
    trailing_bits: int
    command_group_count: int
    literal_word_count: int
    copy_command_count: int
    copied_word_count: int
    maximum_copy_offset_words: int
    maximum_copy_length_words: int


@dataclass(frozen=True)
class WorldPaletteRecord:
    index: int
    source_address: int
    source_first_color: int
    source_sha256: str
    effective_sha256: str


@dataclass(frozen=True)
class WorldAtlasSource:
    source_bundle: bytes
    rgba_pixels: tuple[int, ...]
    selected_slots: tuple[int, ...]


@dataclass(frozen=True)
class PlayerReferenceSource:
    source_bundle: bytes
    rgba_pixels: tuple[int, ...]


ProcessRunner = Callable[..., ProcessReceipt]


def _closed_mapping(
    value: object,
    field: str,
    expected_fields: set[str],
) -> Mapping[str, object]:
    if not isinstance(value, Mapping) or set(value) != expected_fields:
        raise _reject("InvalidToolchain", field, "The tracked toolchain manifest is not closed.")
    return value


def _string(mapping: Mapping[str, object], field: str) -> str:
    value = mapping.get(field)
    if not isinstance(value, str) or not value:
        raise _reject("InvalidToolchain", field, "A tracked toolchain identity is invalid.")
    return value


def _positive_int(mapping: Mapping[str, object], field: str) -> int:
    value = mapping.get(field)
    if not isinstance(value, int) or isinstance(value, bool) or value < 1:
        raise _reject("InvalidToolchain", field, "A tracked toolchain bound is invalid.")
    return value


def _boolean(mapping: Mapping[str, object], field: str) -> bool:
    value = mapping.get(field)
    if not isinstance(value, bool):
        raise _reject("InvalidToolchain", field, "A tracked toolchain policy is invalid.")
    return value


def _sha256(value: bytes) -> str:
    return hashlib.sha256(value).hexdigest().upper()


def _composite_generator_fingerprint(records: Mapping[str, bytes]) -> str:
    if not records:
        raise _reject(
            "GeneratorIdentityInvalid",
            "generator",
            "The generator implementation identity is empty.",
        )
    digest = hashlib.sha256()
    magic = WORLD_GENERATOR_FINGERPRINT_VERSION.encode("ascii")
    digest.update(len(magic).to_bytes(4, "big"))
    digest.update(magic)
    digest.update(len(records).to_bytes(4, "big"))
    for name in sorted(records):
        relative = PurePosixPath(name)
        value = records[name]
        if (
            not name
            or relative.is_absolute()
            or ".." in relative.parts
            or relative.as_posix() != name
            or not isinstance(value, bytes)
        ):
            raise _reject(
                "GeneratorIdentityInvalid",
                "generator",
                "A generator implementation identity record is invalid.",
            )
        encoded_name = name.encode("utf-8")
        digest.update(len(encoded_name).to_bytes(4, "big"))
        digest.update(encoded_name)
        digest.update(len(value).to_bytes(8, "big"))
        digest.update(value)
    return digest.hexdigest().upper()


def _world_generator_artifact_sha256() -> str:
    source_root = Path(__file__).resolve().parents[2]
    implementation_version = sys.implementation.version
    records: dict[str, bytes] = {
        "runtime/python-implementation": sys.implementation.name.encode("ascii"),
        "runtime/python-version": (
            f"{implementation_version.major}.{implementation_version.minor}."
            f"{implementation_version.micro}-{implementation_version.releaselevel}-"
            f"{implementation_version.serial}"
        ).encode("ascii"),
        "runtime/zlib-compile-version": zlib.ZLIB_VERSION.encode("ascii"),
        "runtime/zlib-runtime-version": zlib.ZLIB_RUNTIME_VERSION.encode("ascii"),
    }
    try:
        for relative in WORLD_GENERATOR_COMPONENTS:
            records[relative] = source_root.joinpath(*PurePosixPath(relative).parts).read_bytes()
    except OSError as error:
        raise _reject(
            "GeneratorUnavailable",
            "generator",
            "A generator implementation component is unavailable.",
        ) from error
    return _composite_generator_fingerprint(records)


def _require_sha256(value: str, field: str) -> str:
    if len(value) != 64 or any(character not in "0123456789ABCDEF" for character in value):
        raise _reject("InvalidRequest", field, "The expected SHA-256 is not canonical.")
    return value


def _read_fixed_private_input(
    path_value: str,
    caller_pin: str,
    fixed_digest: str,
    fixed_size: int,
    field: str,
) -> bytes:
    caller_pin = _require_sha256(caller_pin, f"expected{field[0].upper()}{field[1:]}Sha256")
    if not path_value or not os.path.isabs(path_value):
        raise _reject("InvalidRequest", field, "A private candidate input path must be absolute.")
    path = Path(os.path.abspath(path_value))
    _require_no_reparse_chain(path, field)
    try:
        if not path.is_file() or path.stat().st_size != fixed_size:
            raise _reject(
                "ContentDigestMismatch",
                field,
                "A private candidate input does not match its accepted fixed identity.",
            )
        data = path.read_bytes()
    except AssetBuildError:
        raise
    except OSError as error:
        raise _reject(
            "PackageUnavailable",
            field,
            "A required private candidate input is unavailable.",
        ) from error
    actual = _sha256(data)
    if caller_pin != fixed_digest or actual != fixed_digest:
        raise _reject(
            "ContentDigestMismatch",
            field,
            "The private input bytes and caller pin must both match the accepted fixed identity.",
        )
    return data


def _metadata_object(value: object, fields: set[str], field: str) -> Mapping[str, object]:
    if not isinstance(value, Mapping) or set(value) != fields:
        raise _reject(
            "InvalidMetadata",
            field,
            "Private visual metadata has an unknown or missing field.",
        )
    return value


def _metadata_array(value: object, count: int, field: str) -> list[object]:
    if not isinstance(value, list) or len(value) != count:
        raise _reject(
            "InvalidMetadata",
            field,
            "Private visual metadata has an invalid ordered record count.",
        )
    return value


def _metadata_int(value: object, field: str, *, minimum: int = 0) -> int:
    if not isinstance(value, int) or isinstance(value, bool) or value < minimum:
        raise _reject("InvalidMetadata", field, "A private visual metadata integer is invalid.")
    return value


def _metadata_string(value: object, field: str) -> str:
    if not isinstance(value, str) or not value:
        raise _reject("InvalidMetadata", field, "A private visual metadata string is invalid.")
    return value


def _metadata_sha256(value: object, field: str) -> str:
    string = _metadata_string(value, field)
    if len(string) != 64 or any(character not in "0123456789ABCDEF" for character in string):
        raise _reject("InvalidMetadata", field, "A private visual metadata digest is invalid.")
    return string


def _require_metadata_provenance(
    root: Mapping[str, object],
    expected_id: str,
    field: str,
) -> None:
    upstream = _metadata_object(
        root.get("upstream"),
        {"repository", "commit"},
        f"{field}.upstream",
    )
    if (
        root.get("schemaVersion") != 1
        or root.get("id") != expected_id
        or root.get("romSha256") != ACCEPTED_ROM_SHA256
        or upstream.get("repository") != ACCEPTED_UPSTREAM_REPOSITORY
        or upstream.get("commit") != ACCEPTED_UPSTREAM_COMMIT
    ):
        raise _reject(
            "ProvenanceMismatch",
            field,
            "Private visual metadata provenance does not match the accepted contract.",
        )


def _parse_json_bytes(data: bytes, field: str) -> Mapping[str, object]:
    def closed_pairs(pairs: list[tuple[str, object]]) -> dict[str, object]:
        result: dict[str, object] = {}
        for key, value in pairs:
            if key in result:
                raise _reject(
                    "InvalidMetadata",
                    field,
                    "Private visual metadata contains a duplicate field.",
                )
            result[key] = value
        return result

    try:
        document = json.loads(data, object_pairs_hook=closed_pairs)
    except AssetBuildError:
        raise
    except (json.JSONDecodeError, UnicodeDecodeError) as error:
        raise _reject(
            "InvalidMetadata",
            field,
            "A fixed private metadata document is not valid JSON.",
        ) from error
    if not isinstance(document, Mapping):
        raise _reject("InvalidMetadata", field, "Private visual metadata must be an object.")
    return document


def _parse_world_tileset_metadata(
    document: Mapping[str, object],
    rom_length: int,
) -> tuple[WorldTilesetRecord, ...]:
    root = _metadata_object(document, _TILESET_ROOT_FIELDS, "tilesetMetadata")
    _require_metadata_provenance(root, ACCEPTED_TILESET_METADATA_ID, "tilesetMetadata")
    rows = _metadata_array(root.get("tilesets"), 115, "tilesetMetadata.tilesets")
    selected: dict[int, WorldTilesetRecord] = {}
    symbols: set[str] = set()
    for ordinal, raw in enumerate(rows):
        field = f"tilesetMetadata.tilesets[{ordinal}]"
        row = _metadata_object(raw, _TILESET_FIELDS, field)
        index = _metadata_int(row.get("index"), f"{field}.index")
        symbol = _metadata_string(row.get("symbol"), f"{field}.symbol")
        _metadata_string(row.get("sourcePath"), f"{field}.sourcePath")
        if index != ordinal or symbol in symbols:
            raise _reject(
                "InvalidMetadata",
                field,
                "Tileset records must retain unique contiguous source identities.",
            )
        symbols.add(symbol)
        source_address = _metadata_int(row.get("sourceAddress"), f"{field}.sourceAddress")
        compressed_bytes = _metadata_int(
            row.get("compressedBytes"),
            f"{field}.compressedBytes",
            minimum=1,
        )
        if source_address + compressed_bytes > rom_length:
            raise _reject("InvalidMetadata", field, "A selected source range exceeds the ROM.")
        if row.get("decodedBytes") != WORLD_DECODED_BYTES_PER_TILESET:
            raise _reject("InvalidMetadata", field, "A tileset decoded-size identity drifted.")
        input_bits = _metadata_int(
            row.get("inputBitsConsumed"),
            f"{field}.inputBitsConsumed",
            minimum=1,
        )
        trailing_bits = _metadata_int(row.get("trailingBits"), f"{field}.trailingBits")
        if input_bits > compressed_bytes * 8 or trailing_bits != compressed_bytes * 8 - input_bits:
            raise _reject("InvalidMetadata", field, "A tileset Stack boundary drifted.")
        record = WorldTilesetRecord(
            index,
            source_address,
            compressed_bytes,
            _metadata_sha256(row.get("sourceSha256"), f"{field}.sourceSha256"),
            _metadata_sha256(row.get("decodedSha256"), f"{field}.decodedSha256"),
            input_bits,
            trailing_bits,
            _metadata_int(row.get("commandGroupCount"), f"{field}.commandGroupCount"),
            _metadata_int(row.get("literalWordCount"), f"{field}.literalWordCount"),
            _metadata_int(row.get("copyCommandCount"), f"{field}.copyCommandCount"),
            _metadata_int(row.get("copiedWordCount"), f"{field}.copiedWordCount"),
            _metadata_int(row.get("maximumCopyOffsetWords"), f"{field}.maximumCopyOffsetWords"),
            _metadata_int(row.get("maximumCopyLengthWords"), f"{field}.maximumCopyLengthWords"),
        )
        if index in ACCEPTED_TILESET_SLOTS:
            selected[index] = record

    maps = _metadata_array(root.get("maps"), 79, "tilesetMetadata.maps")
    for ordinal, raw in enumerate(maps):
        field = f"tilesetMetadata.maps[{ordinal}]"
        row = _metadata_object(raw, _TILESET_MAP_FIELDS, field)
        if _metadata_int(row.get("mapIndex"), f"{field}.mapIndex") != ordinal:
            raise _reject("InvalidSelection", field, "Map references must remain ordered.")
        _metadata_string(row.get("sourcePath"), f"{field}.sourcePath")
        _metadata_int(row.get("mapAddress"), f"{field}.mapAddress")
        palette = _metadata_int(row.get("paletteIndex"), f"{field}.paletteIndex")
        slots = _metadata_array(row.get("tilesetSlots"), 5, f"{field}.tilesetSlots")
        parsed_slots = tuple(_metadata_int(value, f"{field}.tilesetSlots") for value in slots)
        if any(value != 255 and value >= 115 for value in parsed_slots):
            raise _reject("InvalidSelection", field, "A map tileset slot is out of range.")
        if ordinal == ACCEPTED_MAP_INDEX and (
            palette != ACCEPTED_PALETTE_INDEX or parsed_slots != ACCEPTED_TILESET_SLOTS
        ):
            raise _reject("InvalidSelection", field, "The accepted Map 3 selection drifted.")

    if set(selected) != set(ACCEPTED_TILESET_SLOTS):
        raise _reject(
            "InvalidSelection",
            "tilesetMetadata.tilesets",
            "The accepted Map 3 tileset selection is incomplete.",
        )
    return tuple(selected[index] for index in ACCEPTED_TILESET_SLOTS)


def _parse_world_palette_metadata(
    document: Mapping[str, object],
    rom_length: int,
) -> WorldPaletteRecord:
    root = _metadata_object(document, _PALETTE_ROOT_FIELDS, "paletteMetadata")
    _require_metadata_provenance(root, ACCEPTED_PALETTE_METADATA_ID, "paletteMetadata")
    rows = _metadata_array(root.get("palettes"), 16, "paletteMetadata.palettes")
    selected: WorldPaletteRecord | None = None
    symbols: set[str] = set()
    for ordinal, raw in enumerate(rows):
        field = f"paletteMetadata.palettes[{ordinal}]"
        row = _metadata_object(raw, _PALETTE_FIELDS, field)
        index = _metadata_int(row.get("index"), f"{field}.index")
        symbol = _metadata_string(row.get("symbol"), f"{field}.symbol")
        _metadata_string(row.get("sourcePath"), f"{field}.sourcePath")
        if index != ordinal or symbol in symbols:
            raise _reject(
                "InvalidMetadata",
                field,
                "Palette records must retain unique contiguous source identities.",
            )
        symbols.add(symbol)
        source_address = _metadata_int(row.get("sourceAddress"), f"{field}.sourceAddress")
        if (
            row.get("byteCount") != WORLD_PALETTE_BYTES
            or row.get("colorCount") != WORLD_PALETTE_WORD_COUNT
            or source_address + WORLD_PALETTE_BYTES > rom_length
        ):
            raise _reject("InvalidMetadata", field, "A palette shape or source range drifted.")
        source_first = _metadata_int(row.get("sourceFirstColor"), f"{field}.sourceFirstColor")
        if source_first > WORLD_PALETTE_MASK or row.get("effectiveFirstColor") != 0:
            raise _reject("InvalidMetadata", field, "A palette first-color identity drifted.")
        record = WorldPaletteRecord(
            index,
            source_address,
            source_first,
            _metadata_sha256(row.get("sourceSha256"), f"{field}.sourceSha256"),
            _metadata_sha256(row.get("effectiveSha256"), f"{field}.effectiveSha256"),
        )
        if index == ACCEPTED_PALETTE_INDEX:
            selected = record

    maps = _metadata_array(root.get("maps"), 79, "paletteMetadata.maps")
    for ordinal, raw in enumerate(maps):
        field = f"paletteMetadata.maps[{ordinal}]"
        row = _metadata_object(raw, _PALETTE_MAP_FIELDS, field)
        if _metadata_int(row.get("mapIndex"), f"{field}.mapIndex") != ordinal:
            raise _reject("InvalidSelection", field, "Palette map references must remain ordered.")
        _metadata_string(row.get("sourcePath"), f"{field}.sourcePath")
        _metadata_int(row.get("mapAddress"), f"{field}.mapAddress")
        palette = _metadata_int(row.get("paletteIndex"), f"{field}.paletteIndex")
        if palette > 15 or (ordinal == ACCEPTED_MAP_INDEX and palette != ACCEPTED_PALETTE_INDEX):
            raise _reject(
                "InvalidSelection",
                field,
                "The accepted Map 3 palette selection drifted.",
            )
    if selected is None:
        raise _reject(
            "InvalidSelection",
            "paletteMetadata.palettes",
            "The accepted Map 3 palette is missing.",
        )
    return selected


def _matches_stack_record(decoded: StackDecodeResult, record: WorldTilesetRecord) -> bool:
    return (
        decoded.input_bits_consumed == record.input_bits_consumed
        and record.trailing_bits == record.compressed_bytes * 8 - decoded.input_bits_consumed
        and decoded.command_group_count == record.command_group_count
        and decoded.literal_word_count == record.literal_word_count
        and decoded.copy_command_count == record.copy_command_count
        and decoded.copied_word_count == record.copied_word_count
        and decoded.maximum_copy_offset_words == record.maximum_copy_offset_words
        and decoded.maximum_copy_length_words == record.maximum_copy_length_words
    )


def _build_world_atlas_source(
    rom: bytes,
    tileset_document: Mapping[str, object],
    palette_document: Mapping[str, object],
) -> WorldAtlasSource:
    tileset_records = _parse_world_tileset_metadata(tileset_document, len(rom))
    palette_record = _parse_world_palette_metadata(palette_document, len(rom))
    palette_source = rom[
        palette_record.source_address : palette_record.source_address + WORLD_PALETTE_BYTES
    ]
    if _sha256(palette_source) != palette_record.source_sha256:
        raise _reject(
            "SourcePayloadMismatch",
            "palettePayload",
            "The selected palette source identity drifted.",
        )
    source_words = [
        int.from_bytes(palette_source[offset : offset + 2], "big")
        for offset in range(0, WORLD_PALETTE_BYTES, 2)
    ]
    if source_words[0] != palette_record.source_first_color or any(
        word & ~WORLD_PALETTE_MASK for word in source_words
    ):
        raise _reject(
            "PalettePayloadMismatch",
            "palettePayload",
            "The selected palette word projection drifted.",
        )
    effective_palette_bytes = b"\x00\x00" + palette_source[2:]
    if _sha256(effective_palette_bytes) != palette_record.effective_sha256:
        raise _reject(
            "PalettePayloadMismatch",
            "palettePayload",
            "The accepted palette-zero transform drifted.",
        )
    effective_words = [
        int.from_bytes(effective_palette_bytes[offset : offset + 2], "big")
        for offset in range(0, WORLD_PALETTE_BYTES, 2)
    ]
    palette = [md_palette_color(word) for word in effective_words]

    decoded_buffers: list[bytes] = []
    atlas_pixels: list[int] = []
    for record in tileset_records:
        compressed = rom[record.source_address : record.source_address + record.compressed_bytes]
        if _sha256(compressed) != record.source_sha256:
            raise _reject(
                "SourcePayloadMismatch",
                "tilesetPayload",
                "A selected compressed tileset identity drifted.",
            )
        try:
            decoded = decode_stack_compressed(
                compressed,
                expected_output_bytes=WORLD_DECODED_BYTES_PER_TILESET,
            )
        except ValueError as error:
            raise _reject(
                "DecodeFailure",
                "tilesetPayload",
                "A selected tileset failed bounded Stack decoding.",
            ) from error
        if not _matches_stack_record(decoded, record):
            raise _reject(
                "DecodeFailure",
                "tilesetPayload",
                "A selected tileset Stack-consumption identity drifted.",
            )
        if _sha256(decoded.output) != record.decoded_sha256:
            raise _reject(
                "DecodedPayloadMismatch",
                "tilesetPayload",
                "A selected decoded tileset identity drifted.",
            )
        tiles = [
            decode_md_4bpp_tile(decoded.output[offset : offset + TILE_BYTES_4BPP])
            for offset in range(0, WORLD_DECODED_BYTES_PER_TILESET, TILE_BYTES_4BPP)
        ]
        atlas_pixels.extend(render_tileset_sheet(tiles, palette))
        decoded_buffers.append(decoded.output)

    if len(atlas_pixels) != WORLD_ATLAS_WIDTH * WORLD_ATLAS_HEIGHT * 4:
        raise _reject("GeneratorOutputInvalid", "atlas", "The world atlas geometry drifted.")
    source_bundle = b"".join(
        (
            WORLD_SOURCE_MAGIC,
            bytes((ACCEPTED_MAP_INDEX, ACCEPTED_PALETTE_INDEX, *ACCEPTED_TILESET_SLOTS)),
            effective_palette_bytes,
            *decoded_buffers,
        )
    )
    return WorldAtlasSource(source_bundle, tuple(atlas_pixels), ACCEPTED_TILESET_SLOTS)


def _build_player_reference_source(rom: bytes) -> PlayerReferenceSource:
    pointer_offset = PLAYER_POINTER_TABLE_ADDRESS + (PLAYER_SOURCE_SLOT * 4)
    if (
        pointer_offset < 0
        or pointer_offset + 4 > len(rom)
        or PLAYER_SELECTED_PAYLOAD_ADDRESS < 0
        or len(rom) <= PLAYER_SELECTED_PAYLOAD_ADDRESS
    ):
        raise _reject(
            "SourcePayloadMismatch",
            "playerPayload",
            "The selected player reference-frame pointer boundary is unavailable.",
        )
    selected_address = int.from_bytes(rom[pointer_offset : pointer_offset + 4], "big")
    if selected_address != PLAYER_SELECTED_PAYLOAD_ADDRESS:
        raise _reject(
            "SourcePayloadMismatch",
            "playerPayload",
            "The selected player reference-frame pointer identity drifted.",
        )

    encoded = rom[selected_address:]
    try:
        decoded: BasicDecodeResult = decode_basic_compressed(
            encoded,
            expected_output_bytes=PLAYER_DECODED_BYTES,
        )
    except ValueError as error:
        raise _reject(
            "DecodeFailure",
            "playerPayload",
            "The selected player reference frame failed bounded Basic decoding.",
        ) from error
    if (
        decoded.input_bytes_consumed < 1
        or decoded.input_bytes_consumed > len(encoded)
        or len(decoded.output) != PLAYER_DECODED_BYTES
    ):
        raise _reject(
            "DecodeFailure",
            "playerPayload",
            "The selected player reference-frame decode shape drifted.",
        )
    compressed = encoded[: decoded.input_bytes_consumed]
    frame_start = PLAYER_SELECTED_HALF * PLAYER_FRAME_BYTES
    frame = decoded.output[frame_start : frame_start + PLAYER_FRAME_BYTES]
    if len(frame) != PLAYER_FRAME_BYTES:
        raise _reject(
            "DecodedPayloadMismatch",
            "playerPayload",
            "The selected player reference-frame half is unavailable.",
        )

    palette_end = PLAYER_PALETTE_ADDRESS + WORLD_PALETTE_BYTES
    if PLAYER_PALETTE_ADDRESS < 0 or palette_end > len(rom):
        raise _reject(
            "PalettePayloadMismatch",
            "playerPalette",
            "The selected player palette boundary is unavailable.",
        )
    palette_source = rom[PLAYER_PALETTE_ADDRESS:palette_end]
    palette_words = [
        int.from_bytes(palette_source[offset : offset + 2], "big")
        for offset in range(0, WORLD_PALETTE_BYTES, 2)
    ]
    if len(palette_words) != WORLD_PALETTE_WORD_COUNT or any(
        word & ~WORLD_PALETTE_MASK for word in palette_words
    ):
        raise _reject(
            "PalettePayloadMismatch",
            "playerPalette",
            "The selected player palette word projection drifted.",
        )
    palette = [md_palette_color(word) for word in palette_words]
    tiles = [
        decode_md_4bpp_tile(frame[offset : offset + TILE_BYTES_4BPP])
        for offset in range(0, PLAYER_FRAME_BYTES, TILE_BYTES_4BPP)
    ]
    if len(tiles) != PLAYER_FRAME_TILE_COUNT:
        raise _reject(
            "DecodedPayloadMismatch",
            "playerPayload",
            "The selected player reference-frame tile count drifted.",
        )
    pixels = [0] * (PLAYER_FRAME_WIDTH * PLAYER_FRAME_HEIGHT * 4)
    for tile_index, tile in enumerate(tiles):
        tile_column = tile_index // PLAYER_FRAME_TILES_PER_SIDE
        tile_row = tile_index % PLAYER_FRAME_TILES_PER_SIDE
        origin_x = tile_column * WORLD_TILE_PIXEL_SIZE
        origin_y = tile_row * WORLD_TILE_PIXEL_SIZE
        for row in range(WORLD_TILE_PIXEL_SIZE):
            for column in range(WORLD_TILE_PIXEL_SIZE):
                palette_index = tile[(row * WORLD_TILE_PIXEL_SIZE) + column]
                destination = (((origin_y + row) * PLAYER_FRAME_WIDTH) + origin_x + column) * 4
                pixels[destination : destination + 4] = palette_index_rgba(
                    palette,
                    palette_index,
                )
    source_bundle = b"".join(
        (
            PLAYER_SOURCE_MAGIC,
            bytes((PLAYER_MAPSPRITE_ID, PLAYER_SOURCE_SLOT, PLAYER_SELECTED_HALF)),
            len(compressed).to_bytes(4, "big"),
            compressed,
            palette_source,
        )
    )
    return PlayerReferenceSource(source_bundle, tuple(pixels))


def _scale_rgba_nearest(
    pixels: tuple[int, ...],
    width: int,
    height: int,
    scale: int,
) -> list[int]:
    if scale not in WORLD_SCALES or len(pixels) != width * height * 4:
        raise _reject("GeneratorOutputInvalid", "atlas", "The nearest-neighbor input is invalid.")
    output: list[int] = []
    for row in range(height):
        expanded_row: list[int] = []
        for column in range(width):
            offset = (row * width + column) * 4
            pixel = pixels[offset : offset + 4]
            for _ in range(scale):
                expanded_row.extend(pixel)
        for _ in range(scale):
            output.extend(expanded_row)
    return output


def load_toolchain_manifest(path: Path | None = None) -> PresentationToolchain:
    path = DEFAULT_TOOLCHAIN_MANIFEST if path is None else path
    try:
        document = json.loads(path.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError, UnicodeDecodeError) as error:
        raise _reject(
            "InvalidToolchain",
            "toolchain",
            "The tracked presentation toolchain manifest is unavailable or invalid.",
        ) from error
    root = _closed_mapping(
        document,
        "toolchain",
        {
            "schemaVersion",
            "releaseRepository",
            "releaseTag",
            "generatorId",
            "generatorVersion",
            "versionOutput",
            "archive",
            "policy",
            "limits",
        },
    )
    if root.get("schemaVersion") != 1:
        raise _reject("InvalidToolchain", "schemaVersion", "The toolchain schema is unsupported.")
    archive = _closed_mapping(
        root.get("archive"),
        "archive",
        {"fileName", "url", "sha256", "size", "member", "maximumMemberBytes"},
    )
    policy = _closed_mapping(
        root.get("policy"),
        "policy",
        {
            "policyId",
            "scales",
            "shapeRendering",
            "textRendering",
            "imageRendering",
            "filter",
            "mipmaps",
            "repeat",
            "colorSpace",
            "alphaMode",
        },
    )
    limits = _closed_mapping(
        root.get("limits"),
        "limits",
        {
            "maximumMasterBytes",
            "maximumLogicalDimension",
            "maximumPngBytes",
            "maximumSvgElements",
            "maximumSvgDepth",
            "versionSeconds",
            "rasterizeSeconds",
            "terminationSeconds",
            "reapSeconds",
        },
    )
    scales = policy.get("scales")
    if scales != [2, 4]:
        raise _reject("InvalidToolchain", "scales", "The runtime bucket policy is unsupported.")
    archive_sha = _string(archive, "sha256").upper()
    _require_sha256(archive_sha, "archiveSha256")
    toolchain = PresentationToolchain(
        _string(root, "releaseRepository"),
        _string(root, "releaseTag"),
        _string(root, "generatorId"),
        _string(root, "generatorVersion"),
        _string(root, "versionOutput"),
        ArchiveIdentity(
            _string(archive, "fileName"),
            _string(archive, "url"),
            archive_sha,
            _positive_int(archive, "size"),
            _string(archive, "member"),
            _positive_int(archive, "maximumMemberBytes"),
        ),
        BuildPolicy(
            _string(policy, "policyId"),
            (2, 4),
            _string(policy, "shapeRendering"),
            _string(policy, "textRendering"),
            _string(policy, "imageRendering"),
            _string(policy, "filter"),
            _boolean(policy, "mipmaps"),
            _boolean(policy, "repeat"),
            _string(policy, "colorSpace"),
            _string(policy, "alphaMode"),
        ),
        BuildLimits(
            _positive_int(limits, "maximumMasterBytes"),
            _positive_int(limits, "maximumLogicalDimension"),
            _positive_int(limits, "maximumPngBytes"),
            _positive_int(limits, "maximumSvgElements"),
            _positive_int(limits, "maximumSvgDepth"),
            _positive_int(limits, "versionSeconds"),
            _positive_int(limits, "rasterizeSeconds"),
            _positive_int(limits, "terminationSeconds"),
            _positive_int(limits, "reapSeconds"),
        ),
    )
    if (
        toolchain.generator_id != "resvg-cli"
        or toolchain.generator_version != "0.47.0"
        or toolchain.archive.file_name != "resvg-win64.zip"
        or toolchain.archive.member != "resvg.exe"
        or toolchain.policy.filter != "linear"
        or toolchain.policy.mipmaps
        or toolchain.policy.repeat
        or toolchain.policy.color_space != "srgb"
        or toolchain.policy.alpha_mode != "straight"
    ):
        raise _reject("InvalidToolchain", "toolchain", "The toolchain policy is unsupported.")
    return toolchain


def _is_reparse_point(path: Path) -> bool:
    try:
        metadata = os.lstat(path)
    except OSError:
        return False
    attributes = getattr(metadata, "st_file_attributes", 0)
    reparse_attribute = getattr(stat, "FILE_ATTRIBUTE_REPARSE_POINT", 0)
    return stat.S_ISLNK(metadata.st_mode) or bool(attributes & reparse_attribute)


def _require_no_reparse_chain(path: Path, field: str) -> None:
    current = Path(os.path.abspath(path))
    chain: list[Path] = []
    while True:
        chain.append(current)
        if current.parent == current:
            break
        current = current.parent
    for component in reversed(chain):
        if os.path.lexists(component) and _is_reparse_point(component):
            raise _reject("PathRejected", field, "A local build path crosses a reparse point.")


def _require_archive(path_value: str, toolchain: PresentationToolchain) -> Path:
    if not path_value or not os.path.isabs(path_value):
        raise _reject("InvalidRequest", "resvgArchive", "The resvg archive path must be absolute.")
    archive = Path(os.path.abspath(path_value))
    _require_no_reparse_chain(archive, "resvgArchive")
    if not archive.is_file() or archive.name != toolchain.archive.file_name:
        raise _reject(
            "ToolchainUnavailable", "resvgArchive", "The pinned resvg archive is unavailable."
        )
    try:
        size = archive.stat().st_size
        if size != toolchain.archive.size:
            raise _reject(
                "ToolchainDigestMismatch",
                "resvgArchive",
                "The resvg archive does not match the tracked artifact identity.",
            )
        data = archive.read_bytes()
    except AssetBuildError:
        raise
    except OSError as error:
        raise _reject(
            "ToolchainUnavailable",
            "resvgArchive",
            "The pinned resvg archive could not be read.",
        ) from error
    if len(data) != size or _sha256(data) != toolchain.archive.sha256:
        raise _reject(
            "ToolchainDigestMismatch",
            "resvgArchive",
            "The resvg archive does not match the tracked artifact identity.",
        )
    return archive


def _extract_resvg(archive: Path, destination: Path, toolchain: PresentationToolchain) -> Path:
    target = destination / toolchain.archive.member
    try:
        destination.mkdir()
        with zipfile.ZipFile(archive) as source:
            infos = source.infolist()
            normalized = [os.path.normcase(info.filename.replace("\\", "/")) for info in infos]
            if len(normalized) != len(set(normalized)):
                raise _reject(
                    "InvalidToolchain",
                    "resvgArchive",
                    "The resvg archive has duplicate member identities.",
                )
            matches = [info for info in infos if info.filename == toolchain.archive.member]
            if len(matches) != 1:
                raise _reject(
                    "InvalidToolchain",
                    "resvgArchive",
                    "The resvg archive does not contain one exact executable member.",
                )
            info = matches[0]
            if (
                info.is_dir()
                or info.flag_bits & 0x1
                or info.file_size < 1
                or info.file_size > toolchain.archive.maximum_member_bytes
                or PurePosixPath(info.filename).parts != (toolchain.archive.member,)
            ):
                raise _reject(
                    "InvalidToolchain",
                    "resvgArchive",
                    "The resvg executable member is invalid.",
                )
            with source.open(info) as input_stream, target.open("xb") as output_stream:
                shutil.copyfileobj(input_stream, output_stream, 1024 * 1024)
            if target.stat().st_size != info.file_size:
                raise _reject(
                    "InvalidToolchain",
                    "resvgArchive",
                    "The resvg executable member was not copied completely.",
                )
    except AssetBuildError:
        raise
    except (OSError, zipfile.BadZipFile, RuntimeError) as error:
        raise _reject(
            "InvalidToolchain",
            "resvgArchive",
            "The resvg executable member could not be admitted.",
        ) from error
    return target


def _require_name(asset_id: str, candidate_name: str) -> tuple[str, PurePosixPath]:
    match = _ASSET_ID_PATTERN.fullmatch(asset_id)
    if match is None or not _CANDIDATE_NAME_PATTERN.fullmatch(candidate_name):
        raise _reject(
            "InvalidRequest", "identity", "The HUD asset or candidate identity is invalid."
        )
    name = match.group(1)
    for value in (name, candidate_name):
        normalized = os.path.normcase(value).rstrip(" .")
        if normalized != os.path.normcase(value) or normalized in _WINDOWS_RESERVED:
            raise _reject(
                "InvalidRequest", "identity", "A Windows-ambiguous identity was rejected."
            )
    return name, PurePosixPath("masters", "ui", f"{name}.svg")


def _read_svg_master(path: Path, expected_sha256: str, limits: BuildLimits) -> SvgMaster:
    _require_no_reparse_chain(path, "master")
    try:
        data = path.read_bytes()
    except OSError as error:
        raise _reject(
            "MasterUnavailable", "master", "The HUD SVG master is unavailable."
        ) from error
    if not data or len(data) > limits.maximum_master_bytes or _sha256(data) != expected_sha256:
        raise _reject("MasterDigestMismatch", "master", "The HUD SVG master identity drifted.")
    if data.startswith(b"\xef\xbb\xbf") or b"<!" in data or b"<?" in data:
        raise _reject("InvalidSvg", "master", "The HUD SVG contains a forbidden declaration.")
    try:
        text = data.decode("utf-8")
        root = element_tree.fromstring(text)
    except (UnicodeDecodeError, element_tree.ParseError) as error:
        raise _reject("InvalidSvg", "master", "The HUD SVG is not canonical UTF-8 XML.") from error
    if root.tag != f"{{{SVG_NAMESPACE}}}svg":
        raise _reject("InvalidSvg", "master", "The HUD SVG root namespace is invalid.")
    width_value = root.attrib.get("width", "")
    height_value = root.attrib.get("height", "")
    if not _INTEGER_PATTERN.fullmatch(width_value) or not _INTEGER_PATTERN.fullmatch(height_value):
        raise _reject("InvalidSvg", "master", "The HUD SVG dimensions must be positive integers.")
    width = int(width_value)
    height = int(height_value)
    if width > limits.maximum_logical_dimension or height > limits.maximum_logical_dimension:
        raise _reject("InvalidSvg", "master", "The HUD SVG dimensions exceed the admitted bounds.")
    if root.attrib.get("viewBox") != f"0 0 {width} {height}":
        raise _reject(
            "InvalidSvg", "master", "The HUD SVG viewBox does not match its logical size."
        )

    element_count = 0
    identities: set[str] = set()
    normalized_identities: set[str] = set()

    def visit(
        element: element_tree.Element,
        depth: int,
        inherited_fill: str | None,
        inherited_stroke: str | None,
    ) -> None:
        nonlocal element_count
        element_count += 1
        if element_count > limits.maximum_svg_elements or depth > limits.maximum_svg_depth:
            raise _reject("InvalidSvg", "master", "The HUD SVG structure exceeds its bounds.")
        prefix = f"{{{SVG_NAMESPACE}}}"
        if not isinstance(element.tag, str) or not element.tag.startswith(prefix):
            raise _reject("InvalidSvg", "master", "The HUD SVG contains a foreign element.")
        local_name = element.tag[len(prefix) :]
        allowed = _ELEMENT_ATTRIBUTES.get(local_name)
        if allowed is None or set(element.attrib) - allowed:
            raise _reject(
                "InvalidSvg", "master", "The HUD SVG contains an unsupported element or attribute."
            )
        if local_name in _PAINTABLE_ELEMENTS and len(element):
            raise _reject(
                "InvalidSvg",
                "master",
                "A HUD SVG paintable element must be a leaf.",
            )
        identity = element.attrib.get("id", "")
        normalized_identity = os.path.normcase(identity).rstrip(" .")
        if (
            not _ELEMENT_ID_PATTERN.fullmatch(identity)
            or identity in identities
            or normalized_identity in normalized_identities
            or normalized_identity != os.path.normcase(identity)
            or normalized_identity in _WINDOWS_RESERVED
        ):
            raise _reject("InvalidSvg", "master", "The HUD SVG element identities are invalid.")
        identities.add(identity)
        normalized_identities.add(normalized_identity)
        if element.text and element.text.strip():
            raise _reject("InvalidSvg", "master", "The HUD SVG contains semantic text.")
        if element.tail and element.tail.strip():
            raise _reject("InvalidSvg", "master", "The HUD SVG contains semantic text.")
        for attribute, value in element.attrib.items():
            if "url(" in value.lower() or "data:" in value.lower() or "\\" in value:
                raise _reject("InvalidSvg", "master", "The HUD SVG references external content.")
            if attribute in _NUMERIC_ATTRIBUTES and not _NUMBER_PATTERN.fullmatch(value):
                raise _reject(
                    "InvalidSvg", "master", "The HUD SVG contains an invalid numeric value."
                )
            if attribute in {"opacity", "fill-opacity", "stroke-opacity"} and not (
                0.0 <= float(value) <= 1.0
            ):
                raise _reject("InvalidSvg", "master", "The HUD SVG opacity is outside its bounds.")
            if (
                attribute
                in {
                    "width",
                    "height",
                    "rx",
                    "ry",
                    "r",
                    "stroke-width",
                    "stroke-miterlimit",
                }
                and float(value) < 0.0
            ):
                raise _reject("InvalidSvg", "master", "The HUD SVG geometry is outside its bounds.")
            if attribute in {"fill", "stroke"} and not _COLOR_PATTERN.fullmatch(value):
                raise _reject(
                    "InvalidSvg", "master", "The HUD SVG contains an invalid color value."
                )
            if attribute == "d" and (not value or not _PATH_DATA_PATTERN.fullmatch(value)):
                raise _reject("InvalidSvg", "master", "The HUD SVG path data is invalid.")
            if attribute == "points" and (not value or not _POINTS_PATTERN.fullmatch(value)):
                raise _reject("InvalidSvg", "master", "The HUD SVG point data is invalid.")
            if attribute == "fill-rule" and value not in {"nonzero", "evenodd"}:
                raise _reject("InvalidSvg", "master", "The HUD SVG fill rule is invalid.")
            if attribute == "stroke-linecap" and value not in {"butt", "round", "square"}:
                raise _reject("InvalidSvg", "master", "The HUD SVG line cap is invalid.")
            if attribute == "stroke-linejoin" and value not in {"miter", "round", "bevel"}:
                raise _reject("InvalidSvg", "master", "The HUD SVG line join is invalid.")

        effective_fill = element.attrib.get("fill", inherited_fill)
        effective_stroke = element.attrib.get("stroke", inherited_stroke)
        if local_name == "line":
            if effective_stroke in {None, "none"}:
                raise _reject(
                    "InvalidSvg",
                    "master",
                    "A HUD SVG line requires an explicit non-none stroke.",
                )
        elif local_name in _PAINTABLE_ELEMENTS and (
            effective_fill in {None, "none"} and effective_stroke in {None, "none"}
        ):
            raise _reject(
                "InvalidSvg",
                "master",
                "A HUD SVG shape requires an explicit non-none fill or stroke.",
            )

        child_fill = effective_fill if local_name == "g" else inherited_fill
        child_stroke = effective_stroke if local_name == "g" else inherited_stroke
        for child in element:
            visit(child, depth + 1, child_fill, child_stroke)

    visit(root, 1, None, None)
    return SvgMaster(width, height, expected_sha256, data)


def _validate_png(path: Path, width: int, height: int, maximum_bytes: int) -> PngIdentity:
    try:
        data = path.read_bytes()
    except OSError as error:
        raise _reject("GeneratorOutputInvalid", "png", "A generated PNG is unavailable.") from error
    if (
        len(data) < len(PNG_SIGNATURE) + 12
        or len(data) > maximum_bytes
        or not data.startswith(PNG_SIGNATURE)
    ):
        raise _reject("GeneratorOutputInvalid", "png", "A generated PNG has an invalid envelope.")
    offset = len(PNG_SIGNATURE)
    chunks: list[tuple[bytes, bytes]] = []
    while offset < len(data):
        if offset + 12 > len(data):
            raise _reject("GeneratorOutputInvalid", "png", "A generated PNG is truncated.")
        length = struct.unpack(">I", data[offset : offset + 4])[0]
        chunk_type = data[offset + 4 : offset + 8]
        end = offset + 12 + length
        if end > len(data):
            raise _reject("GeneratorOutputInvalid", "png", "A generated PNG chunk is truncated.")
        payload = data[offset + 8 : offset + 8 + length]
        expected_crc = struct.unpack(">I", data[offset + 8 + length : end])[0]
        if zlib.crc32(chunk_type + payload) & 0xFFFFFFFF != expected_crc:
            raise _reject(
                "GeneratorOutputInvalid", "png", "A generated PNG chunk failed CRC validation."
            )
        chunks.append((chunk_type, payload))
        offset = end
        if chunk_type == b"IEND":
            break
    if offset != len(data):
        raise _reject("GeneratorOutputInvalid", "png", "A generated PNG has trailing bytes.")
    types = [chunk_type for chunk_type, _payload in chunks]
    if not types or types[0] != b"IHDR" or types[-1] != b"IEND" or b"IDAT" not in types:
        raise _reject(
            "GeneratorOutputInvalid", "png", "A generated PNG is missing required chunks."
        )
    if types.count(b"IHDR") != 1 or types.count(b"IEND") != 1:
        raise _reject(
            "GeneratorOutputInvalid", "png", "A generated PNG has duplicate structural chunks."
        )
    header = chunks[0][1]
    if len(header) != 13:
        raise _reject("GeneratorOutputInvalid", "png", "A generated PNG IHDR is invalid.")
    actual_width, actual_height, bit_depth, color_type, compression, filtering, interlace = (
        struct.unpack(">IIBBBBB", header)
    )
    if (
        actual_width != width
        or actual_height != height
        or bit_depth != 8
        or color_type != 6
        or compression != 0
        or filtering != 0
        or interlace != 0
    ):
        raise _reject(
            "GeneratorOutputInvalid",
            "png",
            "A generated PNG does not match the admitted RGBA8 noninterlaced shape.",
        )
    return PngIdentity(width, height, len(data), _sha256(data))


def _run_process(
    runner: ProcessRunner,
    step: str,
    command: tuple[str | Path, ...],
    *,
    cwd: Path,
    timeout: int,
    toolchain: PresentationToolchain,
) -> ProcessReceipt:
    environment = os.environ.copy()
    environment.update({"LC_ALL": "C", "LANG": "C"})
    try:
        receipt = runner(
            step,
            command,
            cwd=cwd,
            environment=environment,
            timeout=timeout,
            termination_timeout=toolchain.limits.termination_seconds,
            reap_timeout=toolchain.limits.reap_seconds,
        )
    except (OSError, RuntimeError, ValueError) as error:
        raise _reject(
            "GeneratorLaunchFailed",
            "generator",
            "The pinned presentation generator could not be launched or reaped.",
        ) from error
    if receipt.cleanup_status != "clean":
        raise _reject(
            "GeneratorCleanupFailed",
            "generator",
            "The pinned presentation generator process tree was not cleaned.",
        )
    if receipt.timed_out:
        raise _reject(
            "GeneratorTimeout", "generator", "The pinned presentation generator timed out."
        )
    if receipt.exit_code != 0:
        raise _reject("GeneratorFailed", "generator", "The pinned presentation generator failed.")
    return receipt


def _rasterize(
    runner: ProcessRunner,
    executable: Path,
    master_path: Path,
    output: Path,
    scale: int,
    toolchain: PresentationToolchain,
) -> None:
    command: tuple[str | Path, ...] = (
        executable,
        "--skip-system-fonts",
        "--quiet",
        "--shape-rendering",
        toolchain.policy.shape_rendering,
        "--text-rendering",
        toolchain.policy.text_rendering,
        "--image-rendering",
        toolchain.policy.image_rendering,
        "--zoom",
        str(scale),
        master_path,
        output,
    )
    _run_process(
        runner,
        f"rasterize-{scale}x",
        command,
        cwd=output.parent,
        timeout=toolchain.limits.rasterize_seconds,
        toolchain=toolchain,
    )


def _manifest(
    asset_id: str,
    name: str,
    master: SvgMaster,
    toolchain: PresentationToolchain,
    buckets: tuple[PngIdentity, PngIdentity],
) -> dict[str, object]:
    bucket_records = []
    for scale, identity in zip(toolchain.policy.scales, buckets, strict=True):
        bucket_records.append(
            {
                "scale": scale,
                "runtimePath": f"runtime/ui/{name}@{scale}x.png",
                "width": identity.width,
                "height": identity.height,
                "byteLength": identity.byte_length,
                "sha256": identity.sha256,
                "mediaType": "image/png",
                "filter": toolchain.policy.filter,
                "mipmaps": toolchain.policy.mipmaps,
                "repeat": toolchain.policy.repeat,
                "colorSpace": toolchain.policy.color_space,
                "alphaMode": toolchain.policy.alpha_mode,
            }
        )
    return {
        "schemaVersion": 1,
        "packageId": PACKAGE_ID,
        "repositoryId": REPOSITORY_ID,
        "profile": PROFILE,
        "capabilities": [PACK_CAPABILITY],
        "logicalPresentation": {"width": 960, "height": 540},
        "assets": [
            {
                "assetId": asset_id,
                "kind": "raster-image",
                "logicalSize": {"width": master.width, "height": master.height},
                "source": {"assetId": f"source.{asset_id}", "sha256": master.sha256},
                "derivation": {
                    "policyId": toolchain.policy.policy_id,
                    "generatorId": toolchain.generator_id,
                    "generatorVersion": toolchain.generator_version,
                    "generatorArtifactSha256": toolchain.archive.sha256,
                },
                "buckets": bucket_records,
            }
        ],
    }


def _write_candidate(
    destination: Path,
    name: str,
    manifest: Mapping[str, object],
    first_outputs: Mapping[int, Path],
) -> tuple[str, bytes]:
    candidate = destination / "candidate"
    try:
        candidate.mkdir()
        runtime = candidate / "runtime" / "ui"
        runtime.mkdir(parents=True)
        for scale, source in first_outputs.items():
            shutil.copyfile(source, runtime / f"{name}@{scale}x.png")
        manifest_path = candidate.joinpath(*MANIFEST_RELATIVE_PATH.parts)
        manifest_path.parent.mkdir(parents=True)
        manifest_bytes = (
            json.dumps(manifest, indent=2, ensure_ascii=True, sort_keys=True) + "\n"
        ).encode("utf-8")
        manifest_path.write_bytes(manifest_bytes)
        try:
            schema = json.loads(PACK_SCHEMA.read_text(encoding="utf-8"))
            Draft202012Validator.check_schema(schema)
            Draft202012Validator(schema).validate(json.loads(manifest_bytes))
        except (OSError, json.JSONDecodeError, SchemaError, ValidationError) as error:
            raise _reject(
                "CandidateManifestInvalid",
                "manifest",
                "The generated candidate manifest failed its tracked closed schema.",
            ) from error
    except AssetBuildError:
        raise
    except OSError as error:
        raise _reject(
            "CandidateWriteFailed",
            "candidate",
            "The candidate pack could not be written.",
        ) from error
    return _sha256(manifest_bytes), manifest_bytes


def _cleanup_owned(path: Path, parent: Path, prefix: str = _STAGING_PREFIX) -> None:
    if not os.path.lexists(path):
        return
    if path.parent != parent or not path.name.startswith(prefix) or _is_reparse_point(path):
        raise _reject(
            "CleanupRejected", "candidate", "The owned candidate staging identity drifted."
        )
    try:
        shutil.rmtree(path)
    except OSError as error:
        raise _reject(
            "CleanupFailed",
            "candidate",
            "The owned candidate staging directory could not be cleaned.",
        ) from error


def build_hud_svg_candidate(
    *,
    asset_root: str,
    expected_commit: str,
    expected_tree: str,
    asset_id: str,
    expected_master_sha256: str,
    resvg_archive: str,
    candidate_name: str,
    process_runner: ProcessRunner = run_bounded_process,
) -> dict[str, object]:
    """Build one ignored two-bucket HUD candidate without changing tracked asset state."""

    name, master_relative = _require_name(asset_id, candidate_name)
    expected_master_sha256 = _require_sha256(expected_master_sha256, "expectedMasterSha256")
    candidate_relative = PurePosixPath("cache", candidate_name)
    try:
        checkout = validate_asset_checkout_identity(
            asset_root,
            expected_commit=expected_commit,
            expected_tree=expected_tree,
            allowed_untracked_path=master_relative,
            required_ignored_path=candidate_relative,
        )
    except AssetPreflightError as error:
        raise _reject(error.code, error.field, error.message) from error
    toolchain = load_toolchain_manifest()
    archive = _require_archive(resvg_archive, toolchain)
    master_path = checkout.root.joinpath(*master_relative.parts)
    master = _read_svg_master(master_path, expected_master_sha256, toolchain.limits)

    cache = checkout.root / "cache"
    _require_no_reparse_chain(cache, "candidate")
    created_cache = False
    if not cache.exists():
        try:
            cache.mkdir()
            created_cache = True
        except OSError as error:
            raise _reject(
                "CandidateWriteFailed", "candidate", "The ignored cache is unavailable."
            ) from error
    if not cache.is_dir():
        raise _reject("PathRejected", "candidate", "The ignored cache boundary is invalid.")
    destination = cache / candidate_name
    _require_no_reparse_chain(destination, "candidate")
    if os.path.lexists(destination):
        raise _reject("CandidateExists", "candidate", "The candidate destination must be fresh.")
    staging = cache / f"{_STAGING_PREFIX}{uuid.uuid4().hex}.tmp"
    if os.path.lexists(staging):
        raise _reject("CandidateExists", "candidate", "The owned staging identity already exists.")

    published = False
    receipt: dict[str, object] | None = None
    try:
        try:
            staging.mkdir()
            executable = _extract_resvg(archive, staging / "tool", toolchain)
        except AssetBuildError:
            raise
        except OSError as error:
            raise _reject(
                "CandidateWriteFailed",
                "candidate",
                "The owned candidate staging directory is unavailable.",
            ) from error
        version = _run_process(
            process_runner,
            "version",
            (executable, "--version"),
            cwd=staging,
            timeout=toolchain.limits.version_seconds,
            toolchain=toolchain,
        )
        if version.stdout_tail.strip() != toolchain.version_output:
            raise _reject(
                "ToolchainVersionMismatch",
                "generator",
                "The resvg executable version does not match the tracked toolchain.",
            )

        outputs: dict[int, tuple[Path, Path]] = {}
        identities: list[PngIdentity] = []
        for scale in toolchain.policy.scales:
            first_dir = staging / f"run-a-{scale}x"
            second_dir = staging / f"run-b-{scale}x"
            first_dir.mkdir()
            second_dir.mkdir()
            first = first_dir / "candidate.png"
            second = second_dir / "candidate.png"
            _rasterize(process_runner, executable, master_path, first, scale, toolchain)
            _rasterize(process_runner, executable, master_path, second, scale, toolchain)
            first_identity = _validate_png(
                first,
                master.width * scale,
                master.height * scale,
                toolchain.limits.maximum_png_bytes,
            )
            second_identity = _validate_png(
                second,
                master.width * scale,
                master.height * scale,
                toolchain.limits.maximum_png_bytes,
            )
            if first_identity != second_identity or first.read_bytes() != second.read_bytes():
                raise _reject(
                    "NonDeterministicOutput",
                    "png",
                    "The pinned generator did not produce byte-identical outputs.",
                )
            outputs[scale] = (first, second)
            identities.append(first_identity)

        manifest = _manifest(
            asset_id,
            name,
            master,
            toolchain,
            (identities[0], identities[1]),
        )
        manifest_sha256, _manifest_bytes = _write_candidate(
            staging,
            name,
            manifest,
            {scale: paths[0] for scale, paths in outputs.items()},
        )

        try:
            repeated = validate_asset_checkout_identity(
                asset_root,
                expected_commit=expected_commit,
                expected_tree=expected_tree,
                allowed_untracked_path=master_relative,
                required_ignored_path=candidate_relative,
            )
        except AssetPreflightError as error:
            raise _reject(error.code, error.field, error.message) from error
        repeated_master = _read_svg_master(
            repeated.root.joinpath(*master_relative.parts),
            expected_master_sha256,
            toolchain.limits,
        )
        if repeated_master != master:
            raise _reject(
                "MasterDigestMismatch", "master", "The HUD SVG master changed during build."
            )

        for child in tuple(staging.iterdir()):
            if child.name != "candidate":
                shutil.rmtree(child)
        os.rename(staging / "candidate", destination)
        published = True
        staging.rmdir()
        try:
            final_checkout = validate_asset_checkout_identity(
                asset_root,
                expected_commit=expected_commit,
                expected_tree=expected_tree,
                allowed_untracked_path=master_relative,
                required_ignored_path=candidate_relative,
            )
        except AssetPreflightError as error:
            raise _reject(error.code, error.field, error.message) from error
        final_master = _read_svg_master(
            final_checkout.root.joinpath(*master_relative.parts),
            expected_master_sha256,
            toolchain.limits,
        )
        if final_master != master:
            raise _reject(
                "MasterDigestMismatch",
                "master",
                "The HUD SVG master changed at publish.",
            )
        receipt = {
            "schemaVersion": 1,
            "capability": BUILD_CAPABILITY,
            "status": "Pass",
            "assetRepositoryCommit": checkout.identity.commit,
            "assetRepositoryTree": checkout.identity.tree,
            "assetId": asset_id,
            "masterSha256": master.sha256,
            "generatorId": toolchain.generator_id,
            "generatorVersion": toolchain.generator_version,
            "generatorArtifactSha256": toolchain.archive.sha256,
            "policyId": toolchain.policy.policy_id,
            "manifestSha256": manifest_sha256,
            "logicalSize": {"width": master.width, "height": master.height},
            "buckets": [
                {
                    "scale": scale,
                    "width": identity.width,
                    "height": identity.height,
                    "byteLength": identity.byte_length,
                    "sha256": identity.sha256,
                }
                for scale, identity in zip(toolchain.policy.scales, identities, strict=True)
            ],
            "cleanupStatus": "clean",
        }
    except AssetBuildError:
        raise
    except OSError as error:
        raise _reject("CandidateWriteFailed", "candidate", "The candidate build failed.") from error
    finally:
        cleanup_error: AssetBuildError | None = None
        if published and receipt is None and os.path.lexists(destination):
            try:
                shutil.rmtree(destination)
                published = False
            except OSError as error:
                cleanup_error = _reject(
                    "CleanupFailed",
                    "candidate",
                    "A failed candidate publication could not be rolled back.",
                )
                cleanup_error.__cause__ = error
        if os.path.lexists(staging):
            try:
                _cleanup_owned(staging, cache)
            except AssetBuildError as error:
                if cleanup_error is None:
                    cleanup_error = error
        if cleanup_error is not None and published and os.path.lexists(destination):
            try:
                shutil.rmtree(destination)
                published = False
            except OSError:
                pass
        if created_cache and not published:
            with suppress(OSError):
                cache.rmdir()
        if cleanup_error is not None:
            raise cleanup_error
    if receipt is None or not published:
        raise _reject("CandidateWriteFailed", "candidate", "The candidate build did not publish.")
    return receipt


def _require_world_candidate_name(candidate_name: str) -> PurePosixPath:
    if not _CANDIDATE_NAME_PATTERN.fullmatch(candidate_name):
        raise _reject("InvalidRequest", "candidate", "The world candidate identity is invalid.")
    normalized = os.path.normcase(candidate_name).rstrip(" .")
    if normalized != os.path.normcase(candidate_name) or normalized in _WINDOWS_RESERVED:
        raise _reject(
            "InvalidRequest",
            "candidate",
            "A Windows-ambiguous world candidate identity was rejected.",
        )
    return PurePosixPath("cache", candidate_name)


def _world_manifest(
    source: WorldAtlasSource,
    generator_artifact_sha256: str,
    buckets: tuple[PngIdentity, PngIdentity],
) -> dict[str, object]:
    bucket_records = []
    for scale, identity in zip(WORLD_SCALES, buckets, strict=True):
        bucket_records.append(
            {
                "scale": scale,
                "runtimePath": WORLD_RUNTIME_TEMPLATE.format(scale=scale),
                "width": identity.width,
                "height": identity.height,
                "byteLength": identity.byte_length,
                "sha256": identity.sha256,
                "mediaType": "image/png",
                "filter": "nearest",
                "mipmaps": False,
                "repeat": False,
                "colorSpace": "srgb",
                "alphaMode": "straight",
            }
        )
    return {
        "schemaVersion": 1,
        "packageId": PACKAGE_ID,
        "repositoryId": REPOSITORY_ID,
        "profile": PROFILE,
        "capabilities": [PACK_CAPABILITY],
        "logicalPresentation": {"width": 960, "height": 540},
        "assets": [
            {
                "assetId": WORLD_ASSET_ID,
                "kind": "raster-image",
                "logicalSize": {"width": WORLD_ATLAS_WIDTH, "height": WORLD_ATLAS_HEIGHT},
                "source": {
                    "assetId": WORLD_SOURCE_ASSET_ID,
                    "sha256": _sha256(source.source_bundle),
                },
                "derivation": {
                    "policyId": WORLD_POLICY_ID,
                    "generatorId": WORLD_GENERATOR_ID,
                    "generatorVersion": WORLD_GENERATOR_VERSION,
                    "generatorArtifactSha256": generator_artifact_sha256,
                },
                "buckets": bucket_records,
            }
        ],
    }


def _write_world_candidate(
    destination: Path,
    source: WorldAtlasSource,
    master: Path,
    outputs: Mapping[int, Path],
    manifest: Mapping[str, object],
) -> tuple[str, bytes]:
    candidate = destination / "candidate"
    try:
        candidate.mkdir()
        source_target = candidate.joinpath(*PurePosixPath(WORLD_SOURCE_FILE).parts)
        source_target.parent.mkdir(parents=True)
        source_target.write_bytes(source.source_bundle)
        master_target = candidate.joinpath(*PurePosixPath(WORLD_MASTER_FILE).parts)
        master_target.parent.mkdir(parents=True)
        shutil.copyfile(master, master_target)
        for scale, output in outputs.items():
            runtime_target = candidate.joinpath(
                *PurePosixPath(WORLD_RUNTIME_TEMPLATE.format(scale=scale)).parts
            )
            runtime_target.parent.mkdir(parents=True, exist_ok=True)
            shutil.copyfile(output, runtime_target)
        manifest_path = candidate.joinpath(*MANIFEST_RELATIVE_PATH.parts)
        manifest_path.parent.mkdir(parents=True)
        manifest_bytes = (
            json.dumps(manifest, indent=2, ensure_ascii=True, sort_keys=True) + "\n"
        ).encode("utf-8")
        manifest_path.write_bytes(manifest_bytes)
        try:
            schema = json.loads(PACK_SCHEMA.read_text(encoding="utf-8"))
            Draft202012Validator.check_schema(schema)
            Draft202012Validator(schema).validate(json.loads(manifest_bytes))
        except (OSError, json.JSONDecodeError, SchemaError, ValidationError) as error:
            raise _reject(
                "CandidateManifestInvalid",
                "manifest",
                "The generated world candidate manifest failed its tracked closed schema.",
            ) from error
    except AssetBuildError:
        raise
    except OSError as error:
        raise _reject(
            "CandidateWriteFailed",
            "candidate",
            "The world candidate pack could not be written.",
        ) from error
    return _sha256(manifest_bytes), manifest_bytes


def build_map3_base_atlas_candidate(
    *,
    asset_root: str,
    expected_commit: str,
    expected_tree: str,
    rom_path: str,
    expected_rom_sha256: str,
    tileset_metadata_path: str,
    expected_tileset_metadata_sha256: str,
    palette_metadata_path: str,
    expected_palette_metadata_sha256: str,
    candidate_name: str,
) -> dict[str, object]:
    """Build one ignored private Map 3 atlas candidate without changing tracked assets."""

    candidate_relative = _require_world_candidate_name(candidate_name)
    try:
        checkout = validate_asset_checkout_identity(
            asset_root,
            expected_commit=expected_commit,
            expected_tree=expected_tree,
            required_ignored_path=candidate_relative,
        )
    except AssetPreflightError as error:
        raise _reject(error.code, error.field, error.message) from error
    rom = _read_fixed_private_input(
        rom_path,
        expected_rom_sha256,
        ACCEPTED_ROM_SHA256,
        ACCEPTED_ROM_SIZE,
        "rom",
    )
    tileset_metadata = _read_fixed_private_input(
        tileset_metadata_path,
        expected_tileset_metadata_sha256,
        ACCEPTED_TILESET_METADATA_SHA256,
        ACCEPTED_TILESET_METADATA_SIZE,
        "tilesetMetadata",
    )
    palette_metadata = _read_fixed_private_input(
        palette_metadata_path,
        expected_palette_metadata_sha256,
        ACCEPTED_PALETTE_METADATA_SHA256,
        ACCEPTED_PALETTE_METADATA_SIZE,
        "paletteMetadata",
    )
    source = _build_world_atlas_source(
        rom,
        _parse_json_bytes(tileset_metadata, "tilesetMetadata"),
        _parse_json_bytes(palette_metadata, "paletteMetadata"),
    )
    generator_artifact_sha256 = _world_generator_artifact_sha256()

    cache = checkout.root / "cache"
    _require_no_reparse_chain(cache, "candidate")
    created_cache = False
    if not cache.exists():
        try:
            cache.mkdir()
            created_cache = True
        except OSError as error:
            raise _reject(
                "CandidateWriteFailed", "candidate", "The ignored cache is unavailable."
            ) from error
    if not cache.is_dir():
        raise _reject("PathRejected", "candidate", "The ignored cache boundary is invalid.")
    destination = cache / candidate_name
    _require_no_reparse_chain(destination, "candidate")
    if os.path.lexists(destination):
        raise _reject("CandidateExists", "candidate", "The candidate destination must be fresh.")
    staging = cache / f"{_WORLD_STAGING_PREFIX}{uuid.uuid4().hex}.tmp"
    if os.path.lexists(staging):
        raise _reject("CandidateExists", "candidate", "The owned staging identity already exists.")

    published = False
    receipt: dict[str, object] | None = None
    try:
        staging.mkdir()
        run_outputs: list[tuple[Path, dict[int, Path], PngIdentity, tuple[PngIdentity, ...]]] = []
        for run_name in ("run-a", "run-b"):
            run = staging / run_name
            run.mkdir()
            master = run / "master.png"
            write_png_rgba(
                master,
                WORLD_ATLAS_WIDTH,
                WORLD_ATLAS_HEIGHT,
                list(source.rgba_pixels),
            )
            master_identity = _validate_png(
                master,
                WORLD_ATLAS_WIDTH,
                WORLD_ATLAS_HEIGHT,
                WORLD_MAXIMUM_PNG_BYTES,
            )
            outputs: dict[int, Path] = {}
            identities: list[PngIdentity] = []
            for scale in WORLD_SCALES:
                output = run / f"atlas-{scale}x.png"
                write_png_rgba(
                    output,
                    WORLD_ATLAS_WIDTH * scale,
                    WORLD_ATLAS_HEIGHT * scale,
                    _scale_rgba_nearest(
                        source.rgba_pixels,
                        WORLD_ATLAS_WIDTH,
                        WORLD_ATLAS_HEIGHT,
                        scale,
                    ),
                )
                outputs[scale] = output
                identities.append(
                    _validate_png(
                        output,
                        WORLD_ATLAS_WIDTH * scale,
                        WORLD_ATLAS_HEIGHT * scale,
                        WORLD_MAXIMUM_PNG_BYTES,
                    )
                )
            run_outputs.append((master, outputs, master_identity, tuple(identities)))

        first_master, first_outputs, master_identity, bucket_identities = run_outputs[0]
        second_master, second_outputs, second_master_identity, second_bucket_identities = (
            run_outputs[1]
        )
        if (
            master_identity != second_master_identity
            or first_master.read_bytes() != second_master.read_bytes()
            or bucket_identities != second_bucket_identities
            or any(
                first_outputs[scale].read_bytes() != second_outputs[scale].read_bytes()
                for scale in WORLD_SCALES
            )
        ):
            raise _reject(
                "NonDeterministicOutput",
                "atlas",
                "The Map 3 atlas derivation did not produce byte-identical outputs.",
            )
        manifest = _world_manifest(
            source,
            generator_artifact_sha256,
            (bucket_identities[0], bucket_identities[1]),
        )
        manifest_sha256, _manifest_bytes = _write_world_candidate(
            staging,
            source,
            first_master,
            first_outputs,
            manifest,
        )
        try:
            repeated = validate_asset_checkout_identity(
                asset_root,
                expected_commit=expected_commit,
                expected_tree=expected_tree,
                required_ignored_path=candidate_relative,
            )
        except AssetPreflightError as error:
            raise _reject(error.code, error.field, error.message) from error
        if repeated.identity != checkout.identity:
            raise _reject(
                "RepositoryStateMismatch",
                "assetRepository",
                "The local asset repository identity changed during candidate construction.",
            )
        for child in tuple(staging.iterdir()):
            if child.name != "candidate":
                shutil.rmtree(child)
        os.rename(staging / "candidate", destination)
        published = True
        staging.rmdir()
        try:
            final_checkout = validate_asset_checkout_identity(
                asset_root,
                expected_commit=expected_commit,
                expected_tree=expected_tree,
                required_ignored_path=candidate_relative,
            )
        except AssetPreflightError as error:
            raise _reject(error.code, error.field, error.message) from error
        if final_checkout.identity != checkout.identity:
            raise _reject(
                "RepositoryStateMismatch",
                "assetRepository",
                "The local asset repository identity changed at candidate publication.",
            )
        receipt = {
            "schemaVersion": 1,
            "capability": WORLD_BUILD_CAPABILITY,
            "status": "Pass",
            "assetRepositoryCommit": checkout.identity.commit,
            "assetRepositoryTree": checkout.identity.tree,
            "assetId": WORLD_ASSET_ID,
            "sourceAssetId": WORLD_SOURCE_ASSET_ID,
            "sourceBundleByteLength": len(source.source_bundle),
            "sourceBundleSha256": _sha256(source.source_bundle),
            "masterSha256": master_identity.sha256,
            "generatorId": WORLD_GENERATOR_ID,
            "generatorVersion": WORLD_GENERATOR_VERSION,
            "generatorArtifactSha256": generator_artifact_sha256,
            "policyId": WORLD_POLICY_ID,
            "acceptedTilesetSlots": list(source.selected_slots),
            "segmentCount": WORLD_TILESET_COUNT,
            "segmentSize": {"width": WORLD_SHEET_WIDTH, "height": WORLD_SHEET_HEIGHT},
            "tileGrid": {
                "columns": WORLD_TILE_GRID_COLUMNS,
                "rows": WORLD_TILE_GRID_ROWS,
                "tileWidth": WORLD_TILE_PIXEL_SIZE,
                "tileHeight": WORLD_TILE_PIXEL_SIZE,
                "tilesPerSegment": WORLD_TILES_PER_TILESET,
            },
            "atlasSize": {"width": WORLD_ATLAS_WIDTH, "height": WORLD_ATLAS_HEIGHT},
            "palettePolicy": {
                "channelExpansion": "v<<5|v<<2|v>>1",
                "transparentIndex": 0,
                "colorSpace": "srgb",
                "alphaMode": "straight",
                "parityClaim": "project-authored-review-candidate-only",
            },
            "buckets": [
                {
                    "scale": scale,
                    "width": identity.width,
                    "height": identity.height,
                    "byteLength": identity.byte_length,
                    "sha256": identity.sha256,
                    "filter": "nearest",
                    "mipmaps": False,
                    "repeat": False,
                }
                for scale, identity in zip(WORLD_SCALES, bucket_identities, strict=True)
            ],
            "manifestSha256": manifest_sha256,
            "cleanupStatus": "clean",
        }
    except AssetBuildError:
        raise
    except OSError as error:
        raise _reject(
            "CandidateWriteFailed", "candidate", "The world candidate build failed."
        ) from error
    finally:
        cleanup_error: AssetBuildError | None = None
        if published and receipt is None and os.path.lexists(destination):
            try:
                shutil.rmtree(destination)
                published = False
            except OSError as error:
                cleanup_error = _reject(
                    "CleanupFailed",
                    "candidate",
                    "A failed world candidate publication could not be rolled back.",
                )
                cleanup_error.__cause__ = error
        if os.path.lexists(staging):
            try:
                _cleanup_owned(staging, cache, _WORLD_STAGING_PREFIX)
            except AssetBuildError as error:
                if cleanup_error is None:
                    cleanup_error = error
        if cleanup_error is not None and published and os.path.lexists(destination):
            try:
                shutil.rmtree(destination)
                published = False
            except OSError:
                pass
        if created_cache and not published:
            with suppress(OSError):
                cache.rmdir()
        if cleanup_error is not None:
            raise cleanup_error
    if receipt is None or not published:
        raise _reject("CandidateWriteFailed", "candidate", "The world candidate did not publish.")
    return receipt


def _player_manifest(
    source: PlayerReferenceSource,
    generator_artifact_sha256: str,
    buckets: tuple[PngIdentity, PngIdentity],
) -> dict[str, object]:
    bucket_records = [
        {
            "scale": scale,
            "runtimePath": PLAYER_RUNTIME_TEMPLATE.format(scale=scale),
            "width": identity.width,
            "height": identity.height,
            "byteLength": identity.byte_length,
            "sha256": identity.sha256,
            "mediaType": "image/png",
            "filter": "nearest",
            "mipmaps": False,
            "repeat": False,
            "colorSpace": "srgb",
            "alphaMode": "straight",
        }
        for scale, identity in zip(WORLD_SCALES, buckets, strict=True)
    ]
    return {
        "schemaVersion": 1,
        "packageId": PACKAGE_ID,
        "repositoryId": REPOSITORY_ID,
        "profile": PROFILE,
        "capabilities": [PACK_CAPABILITY],
        "logicalPresentation": {"width": 960, "height": 540},
        "assets": [
            {
                "assetId": PLAYER_ASSET_ID,
                "kind": "raster-image",
                "logicalSize": {"width": PLAYER_FRAME_WIDTH, "height": PLAYER_FRAME_HEIGHT},
                "source": {
                    "assetId": PLAYER_SOURCE_ASSET_ID,
                    "sha256": _sha256(source.source_bundle),
                },
                "derivation": {
                    "policyId": PLAYER_POLICY_ID,
                    "generatorId": WORLD_GENERATOR_ID,
                    "generatorVersion": WORLD_GENERATOR_VERSION,
                    "generatorArtifactSha256": generator_artifact_sha256,
                },
                "buckets": bucket_records,
            }
        ],
    }


def _write_player_candidate(
    destination: Path,
    source: PlayerReferenceSource,
    master: Path,
    outputs: Mapping[int, Path],
    manifest: Mapping[str, object],
) -> str:
    candidate = destination / "candidate"
    try:
        candidate.mkdir()
        source_target = candidate.joinpath(*PurePosixPath(PLAYER_SOURCE_FILE).parts)
        source_target.parent.mkdir(parents=True)
        source_target.write_bytes(source.source_bundle)
        master_target = candidate.joinpath(*PurePosixPath(PLAYER_MASTER_FILE).parts)
        master_target.parent.mkdir(parents=True)
        shutil.copyfile(master, master_target)
        for scale, output in outputs.items():
            runtime_target = candidate.joinpath(
                *PurePosixPath(PLAYER_RUNTIME_TEMPLATE.format(scale=scale)).parts
            )
            runtime_target.parent.mkdir(parents=True, exist_ok=True)
            shutil.copyfile(output, runtime_target)
        manifest_path = candidate.joinpath(*MANIFEST_RELATIVE_PATH.parts)
        manifest_path.parent.mkdir(parents=True)
        manifest_bytes = (
            json.dumps(manifest, indent=2, ensure_ascii=True, sort_keys=True) + "\n"
        ).encode("utf-8")
        manifest_path.write_bytes(manifest_bytes)
        try:
            schema = json.loads(PACK_SCHEMA.read_text(encoding="utf-8"))
            Draft202012Validator.check_schema(schema)
            Draft202012Validator(schema).validate(json.loads(manifest_bytes))
        except (OSError, json.JSONDecodeError, SchemaError, ValidationError) as error:
            raise _reject(
                "CandidateManifestInvalid",
                "manifest",
                "The generated player reference candidate manifest failed its tracked "
                "closed schema.",
            ) from error
    except AssetBuildError:
        raise
    except OSError as error:
        raise _reject(
            "CandidateWriteFailed",
            "candidate",
            "The player reference candidate pack could not be written.",
        ) from error
    return _sha256(manifest_bytes)


def build_map3_player_reference_frame_candidate(
    *,
    asset_root: str,
    expected_commit: str,
    expected_tree: str,
    rom_path: str,
    expected_rom_sha256: str,
    candidate_name: str,
) -> dict[str, object]:
    """Build one ignored private Map 3 player initial-reference-frame candidate."""

    candidate_relative = _require_world_candidate_name(candidate_name)
    try:
        checkout = validate_asset_checkout_identity(
            asset_root,
            expected_commit=expected_commit,
            expected_tree=expected_tree,
            required_ignored_path=candidate_relative,
        )
    except AssetPreflightError as error:
        raise _reject(error.code, error.field, error.message) from error
    rom = _read_fixed_private_input(
        rom_path,
        expected_rom_sha256,
        ACCEPTED_ROM_SHA256,
        ACCEPTED_ROM_SIZE,
        "rom",
    )
    source = _build_player_reference_source(rom)
    generator_artifact_sha256 = _world_generator_artifact_sha256()

    cache = checkout.root / "cache"
    _require_no_reparse_chain(cache, "candidate")
    created_cache = False
    if not cache.exists():
        try:
            cache.mkdir()
            created_cache = True
        except OSError as error:
            raise _reject(
                "CandidateWriteFailed",
                "candidate",
                "The ignored cache is unavailable.",
            ) from error
    if not cache.is_dir():
        raise _reject("PathRejected", "candidate", "The ignored cache boundary is invalid.")
    destination = cache / candidate_name
    _require_no_reparse_chain(destination, "candidate")
    if os.path.lexists(destination):
        raise _reject("CandidateExists", "candidate", "The candidate destination must be fresh.")
    staging = cache / f"{_PLAYER_STAGING_PREFIX}{uuid.uuid4().hex}.tmp"
    if os.path.lexists(staging):
        raise _reject("CandidateExists", "candidate", "The owned staging identity already exists.")

    published = False
    receipt: dict[str, object] | None = None
    try:
        staging.mkdir()
        run_outputs: list[tuple[Path, dict[int, Path], PngIdentity, tuple[PngIdentity, ...]]] = []
        for run_name in ("run-a", "run-b"):
            run = staging / run_name
            run.mkdir()
            master = run / "master.png"
            write_png_rgba(
                master,
                PLAYER_FRAME_WIDTH,
                PLAYER_FRAME_HEIGHT,
                list(source.rgba_pixels),
            )
            master_identity = _validate_png(
                master,
                PLAYER_FRAME_WIDTH,
                PLAYER_FRAME_HEIGHT,
                WORLD_MAXIMUM_PNG_BYTES,
            )
            outputs: dict[int, Path] = {}
            identities: list[PngIdentity] = []
            for scale in WORLD_SCALES:
                output = run / f"player-reference-{scale}x.png"
                write_png_rgba(
                    output,
                    PLAYER_FRAME_WIDTH * scale,
                    PLAYER_FRAME_HEIGHT * scale,
                    _scale_rgba_nearest(
                        source.rgba_pixels,
                        PLAYER_FRAME_WIDTH,
                        PLAYER_FRAME_HEIGHT,
                        scale,
                    ),
                )
                outputs[scale] = output
                identities.append(
                    _validate_png(
                        output,
                        PLAYER_FRAME_WIDTH * scale,
                        PLAYER_FRAME_HEIGHT * scale,
                        WORLD_MAXIMUM_PNG_BYTES,
                    )
                )
            run_outputs.append((master, outputs, master_identity, tuple(identities)))

        first_master, first_outputs, master_identity, bucket_identities = run_outputs[0]
        second_master, second_outputs, second_master_identity, second_bucket_identities = (
            run_outputs[1]
        )
        if (
            master_identity != second_master_identity
            or first_master.read_bytes() != second_master.read_bytes()
            or bucket_identities != second_bucket_identities
            or any(
                first_outputs[scale].read_bytes() != second_outputs[scale].read_bytes()
                for scale in WORLD_SCALES
            )
        ):
            raise _reject(
                "NonDeterministicOutput",
                "playerReference",
                "The player reference-frame derivation did not produce byte-identical outputs.",
            )
        manifest = _player_manifest(
            source,
            generator_artifact_sha256,
            (bucket_identities[0], bucket_identities[1]),
        )
        manifest_sha256 = _write_player_candidate(
            staging,
            source,
            first_master,
            first_outputs,
            manifest,
        )
        try:
            repeated = validate_asset_checkout_identity(
                asset_root,
                expected_commit=expected_commit,
                expected_tree=expected_tree,
                required_ignored_path=candidate_relative,
            )
        except AssetPreflightError as error:
            raise _reject(error.code, error.field, error.message) from error
        if repeated.identity != checkout.identity:
            raise _reject(
                "RepositoryStateMismatch",
                "assetRepository",
                "The local asset repository identity changed during candidate construction.",
            )
        for child in tuple(staging.iterdir()):
            if child.name != "candidate":
                shutil.rmtree(child)
        os.rename(staging / "candidate", destination)
        published = True
        staging.rmdir()
        try:
            final_checkout = validate_asset_checkout_identity(
                asset_root,
                expected_commit=expected_commit,
                expected_tree=expected_tree,
                required_ignored_path=candidate_relative,
            )
        except AssetPreflightError as error:
            raise _reject(error.code, error.field, error.message) from error
        if final_checkout.identity != checkout.identity:
            raise _reject(
                "RepositoryStateMismatch",
                "assetRepository",
                "The local asset repository identity changed at candidate publication.",
            )
        receipt = {
            "schemaVersion": 1,
            "capability": PLAYER_BUILD_CAPABILITY,
            "status": "Pass",
            "assetRepositoryCommit": checkout.identity.commit,
            "assetRepositoryTree": checkout.identity.tree,
            "assetId": PLAYER_ASSET_ID,
            "sourceAssetId": PLAYER_SOURCE_ASSET_ID,
            "masterSha256": master_identity.sha256,
            "generatorId": WORLD_GENERATOR_ID,
            "generatorVersion": WORLD_GENERATOR_VERSION,
            "generatorArtifactSha256": generator_artifact_sha256,
            "policyId": PLAYER_POLICY_ID,
            "selection": {
                "controlledEntityIndex": 0,
                "allyIndex": 0,
                "regularMapSpriteId": PLAYER_MAPSPRITE_ID,
                "direction": "DOWN",
                "facing": 3,
                "sourceSlot": PLAYER_SOURCE_SLOT,
                "horizontalMirror": False,
                "selectedHalf": PLAYER_SELECTED_HALF,
                "framePolicy": "initial-reference-frame",
            },
            "logicalSize": {"width": PLAYER_FRAME_WIDTH, "height": PLAYER_FRAME_HEIGHT},
            "palettePolicy": {
                "sourceSymbol": "palette_Base",
                "wordMask": "0x0EEE",
                "channelExpansion": "v<<5|v<<2|v>>1",
                "transparentIndex": 0,
                "colorSpace": "srgb",
                "alphaMode": "straight",
                "parityClaim": "project-inferred-rendering-policy",
            },
            "buckets": [
                {
                    "scale": scale,
                    "width": identity.width,
                    "height": identity.height,
                    "byteLength": identity.byte_length,
                    "sha256": identity.sha256,
                    "filter": "nearest",
                    "mipmaps": False,
                    "repeat": False,
                }
                for scale, identity in zip(WORLD_SCALES, bucket_identities, strict=True)
            ],
            "manifestSha256": manifest_sha256,
            "cleanupStatus": "clean",
        }
    except AssetBuildError:
        raise
    except OSError as error:
        raise _reject(
            "CandidateWriteFailed",
            "candidate",
            "The player reference candidate build failed.",
        ) from error
    finally:
        cleanup_error: AssetBuildError | None = None
        if published and receipt is None and os.path.lexists(destination):
            try:
                shutil.rmtree(destination)
                published = False
            except OSError as error:
                cleanup_error = _reject(
                    "CleanupFailed",
                    "candidate",
                    "A failed player reference publication could not be rolled back.",
                )
                cleanup_error.__cause__ = error
        if os.path.lexists(staging):
            try:
                _cleanup_owned(staging, cache, _PLAYER_STAGING_PREFIX)
            except AssetBuildError as error:
                if cleanup_error is None:
                    cleanup_error = error
        if cleanup_error is not None and published and os.path.lexists(destination):
            try:
                shutil.rmtree(destination)
                published = False
            except OSError:
                pass
        if created_cache and not published:
            with suppress(OSError):
                cache.rmdir()
        if cleanup_error is not None:
            raise cleanup_error
    if receipt is None or not published:
        raise _reject(
            "CandidateWriteFailed",
            "candidate",
            "The player reference candidate did not publish.",
        )
    return receipt


def _parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        description="Build one deterministic ignored local HUD SVG presentation candidate."
    )
    subparsers = parser.add_subparsers(dest="command", required=True)
    candidate = subparsers.add_parser("hud-svg-candidate")
    candidate.add_argument("--asset-root", required=True)
    candidate.add_argument("--expected-commit", required=True)
    candidate.add_argument("--expected-tree", required=True)
    candidate.add_argument("--asset-id", required=True)
    candidate.add_argument("--expected-master-sha256", required=True)
    candidate.add_argument("--resvg-archive", required=True)
    candidate.add_argument("--candidate-name", required=True)
    world = subparsers.add_parser("map3-base-atlas-candidate")
    world.add_argument("--asset-root", required=True)
    world.add_argument("--expected-commit", required=True)
    world.add_argument("--expected-tree", required=True)
    world.add_argument("--rom", required=True)
    world.add_argument("--expected-rom-sha256", required=True)
    world.add_argument("--tileset-metadata", required=True)
    world.add_argument("--expected-tileset-metadata-sha256", required=True)
    world.add_argument("--palette-metadata", required=True)
    world.add_argument("--expected-palette-metadata-sha256", required=True)
    world.add_argument("--candidate-name", required=True)
    player = subparsers.add_parser("map3-player-reference-frame-candidate")
    player.add_argument("--asset-root", required=True)
    player.add_argument("--expected-commit", required=True)
    player.add_argument("--expected-tree", required=True)
    player.add_argument("--rom", required=True)
    player.add_argument("--expected-rom-sha256", required=True)
    player.add_argument("--candidate-name", required=True)
    return parser


def main(argv: list[str] | None = None) -> int:
    arguments = _parser().parse_args(argv)
    try:
        if arguments.command == "hud-svg-candidate":
            receipt = build_hud_svg_candidate(
                asset_root=arguments.asset_root,
                expected_commit=arguments.expected_commit,
                expected_tree=arguments.expected_tree,
                asset_id=arguments.asset_id,
                expected_master_sha256=arguments.expected_master_sha256,
                resvg_archive=arguments.resvg_archive,
                candidate_name=arguments.candidate_name,
            )
        elif arguments.command == "map3-base-atlas-candidate":
            receipt = build_map3_base_atlas_candidate(
                asset_root=arguments.asset_root,
                expected_commit=arguments.expected_commit,
                expected_tree=arguments.expected_tree,
                rom_path=arguments.rom,
                expected_rom_sha256=arguments.expected_rom_sha256,
                tileset_metadata_path=arguments.tileset_metadata,
                expected_tileset_metadata_sha256=arguments.expected_tileset_metadata_sha256,
                palette_metadata_path=arguments.palette_metadata,
                expected_palette_metadata_sha256=arguments.expected_palette_metadata_sha256,
                candidate_name=arguments.candidate_name,
            )
        else:
            receipt = build_map3_player_reference_frame_candidate(
                asset_root=arguments.asset_root,
                expected_commit=arguments.expected_commit,
                expected_tree=arguments.expected_tree,
                rom_path=arguments.rom,
                expected_rom_sha256=arguments.expected_rom_sha256,
                candidate_name=arguments.candidate_name,
            )
    except AssetBuildError as error:
        print(json.dumps({"status": "Rejected", "diagnostic": error.as_dict()}), file=sys.stderr)
        return 2
    print(json.dumps(receipt, sort_keys=True))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
