"""Deterministic local HUD SVG candidate builder for the private presentation pack."""

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

DEFAULT_TOOLCHAIN_MANIFEST = repo_path("remake/presentation-toolchain.json")
BUILD_CAPABILITY = "private-local-presentation-hud-svg-candidate-build-v1"
SVG_NAMESPACE = "http://www.w3.org/2000/svg"
PNG_SIGNATURE = b"\x89PNG\r\n\x1a\n"
_STAGING_PREFIX = ".sf2-hud-svg-build-"
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


def _require_sha256(value: str, field: str) -> str:
    if len(value) != 64 or any(character not in "0123456789ABCDEF" for character in value):
        raise _reject("InvalidRequest", field, "The expected SHA-256 is not canonical.")
    return value


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


def _cleanup_owned(path: Path, parent: Path) -> None:
    if not os.path.lexists(path):
        return
    if (
        path.parent != parent
        or not path.name.startswith(_STAGING_PREFIX)
        or _is_reparse_point(path)
    ):
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
    return parser


def main(argv: list[str] | None = None) -> int:
    arguments = _parser().parse_args(argv)
    try:
        receipt = build_hud_svg_candidate(
            asset_root=arguments.asset_root,
            expected_commit=arguments.expected_commit,
            expected_tree=arguments.expected_tree,
            asset_id=arguments.asset_id,
            expected_master_sha256=arguments.expected_master_sha256,
            resvg_archive=arguments.resvg_archive,
            candidate_name=arguments.candidate_name,
        )
    except AssetBuildError as error:
        print(json.dumps({"status": "Rejected", "diagnostic": error.as_dict()}), file=sys.stderr)
        return 2
    print(json.dumps(receipt, sort_keys=True))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
