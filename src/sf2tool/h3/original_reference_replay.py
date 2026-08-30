"""Private, transport-only original-reference replay capability.

This module deliberately has no scenario semantics.  It can preflight and later
launch one frozen BK2 through the pinned original-runtime toolchain, while the
tracked fixture remains a public recipe rather than game-play evidence.
"""

from __future__ import annotations

import hashlib
import io
import json
import os
import re
import shutil
import stat
import tempfile
import zipfile
from dataclasses import dataclass
from pathlib import Path, PurePosixPath
from typing import Any

from sf2tool.h3.bizhawk import (
    TOOLCHAIN_MANIFEST,
    run_native_bizhawk_process,
    validate_lua_syntax,
)
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path
from sf2tool.rom import inspect_rom

CAPABILITY_ID = "sf2-original-reference-replay-capability-v1"
FIXTURE_PATH = repo_path("tests/fixtures/core/original-reference-replay-capability-v1.json")
CAPABILITY_SCHEMA = repo_path("schemas/core/original-reference-replay-capability.schema.json")
RECEIPT_SCHEMA = repo_path("schemas/core/original-reference-replay-receipt.schema.json")
OBSERVER_PATH = repo_path("tools/bizhawk/original_reference_replay_observer.lua")
DERIVED_ROOT = repo_path("local/derived/h3/original-reference-replay-capability")
ZIP_MEMBERS = ("Header", "Input Log", "SyncSettings")
MAX_DIAGNOSTIC_LAUNCHES = 2
ACCEPTANCE_LAUNCH_ORDINAL = 3
MAX_LAUNCH_ORDINAL = ACCEPTANCE_LAUNCH_ORDINAL
_DOS_EPOCH = (1980, 1, 1, 0, 0, 0)
_DIAGNOSTIC_LIMIT = 4_000
_ARCHIVE_MEMBER_SET_SHA256 = "6A903AA1503B5BAECEF7199BA36591DEFB83FB8B38770A2D09CF2A86A4721BE1"
_ARCHIVE_MEMBER_COUNT = 477
_MUTABLE_SURFACES = (
    "config.ini",
    "gamedb/gamedb_user.txt",
    "Genesis/SaveRAM",
    "Genesis/State",
    "Logs",
    "Movies",
    "AV",
    "Tools",
    "Lua",
    "Watch",
    "ExternalTools",
    "Temp",
    "MultiDisk",
    "ROM",
    "Genesis/Screenshots",
    "Genesis/Cheats",
)
_FORBIDDEN_ARCHIVE_MUTABLE_MEMBERS = (
    "config.ini",
    "gamedb_user",
    "Genesis/SaveRAM",
    "Genesis/State",
)
_GLOBAL_ORDINAL_ONE_CANDIDATE = "9F8417BC1A515FEB5D9466DCC1BC489B981D97741E44518D572E6B0E63380BDF"
_GLOBAL_ORDINAL_ONE_RECEIPT_SHA256 = (
    "BDE38876750E51E59CF1D2897495EFFD8EE42955F7FE87C3F12A9DB853C14CA6"
)
_SHA256_RE = re.compile(r"^[A-F0-9]{64}$")


@dataclass(frozen=True)
class MaterializedMovie:
    """The byte-exact public transport artifact before it enters private storage."""

    data: bytes
    recipe_sha256: str
    bk2_sha256: str
    members: tuple[str, ...]


class CapabilityError(ValueError):
    """A deterministic preflight or replay-contract failure."""


class PrivateInputUnavailable(CapabilityError):
    """Only an absent ignored ROM or pinned local toolchain may be unavailable."""


def _canonical_json(value: Any) -> bytes:
    return (
        json.dumps(value, ensure_ascii=False, sort_keys=True, separators=(",", ":")) + "\n"
    ).encode("utf-8")


def _sha256(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest().upper()


def _canonical_observer_transport_bytes(path: Path = OBSERVER_PATH) -> bytes:
    """Return the closed UTF-8/LF observer transport representation.

    Git may check this text file out with CRLF on Windows. The capability
    accepts precisely that checkout variation, converts it to the declared LF
    transport bytes, and rejects a lone carriage return or every other byte
    change through the declared digest.
    """

    raw = path.resolve(strict=True).read_bytes()
    try:
        text = raw.decode("utf-8")
    except UnicodeDecodeError as error:
        raise CapabilityError(f"passive observer is not UTF-8: {error}") from error
    canonical = text.replace("\r\n", "\n")
    if "\r" in canonical:
        raise CapabilityError("passive observer has a non-CRLF carriage return")
    return canonical.encode("utf-8")


def _bounded(value: str) -> str:
    return value[-_DIAGNOSTIC_LIMIT:]


def _file_identity(path: Path) -> dict[str, Any]:
    resolved = path.resolve(strict=True)
    data = resolved.read_bytes()
    return {"sha256": _sha256(data), "sizeBytes": len(data)}


def _snapshot_file_identity(path: Path) -> dict[str, Any]:
    """Capture a post-launch file identity without allowing a readback error to escape."""

    try:
        _reject_reparse(path)
        if not path.is_file():
            return {
                "status": "captured",
                "identity": {"exists": False, "sha256": None, "sizeBytes": 0},
                "error": None,
            }
        return {
            "status": "captured",
            "identity": {"exists": True, **_file_identity(path)},
            "error": None,
        }
    except (CapabilityError, OSError) as error:
        return {"status": "unavailable", "identity": None, "error": str(error)}


def _config_template(projection: dict[str, Any]) -> dict[str, Any]:
    """Render the deliberately small, closed BizHawk configuration projection."""

    required = {"paths", "settings"}
    if set(projection) != required:
        raise CapabilityError("contained configuration projection keys drift")
    paths = projection["paths"]
    settings = projection["settings"]
    if not isinstance(paths, list) or not isinstance(settings, dict):
        raise CapabilityError("contained configuration projection types drift")
    expected_paths = {
        ("Global_NULL", "Base", "."),
        ("Global_NULL", "ROM", "./ROM"),
        ("Global_NULL", "Firmware", "./Firmware"),
        ("Global_NULL", "Movies", "./Movies"),
        ("Global_NULL", "Movie backups", "./Movies/backup"),
        ("Global_NULL", "A/V Dumps", "./AV"),
        ("Global_NULL", "Tools", "./Tools"),
        ("Global_NULL", "Lua", "./Lua"),
        ("Global_NULL", "Watch (.wch)", "./Watch"),
        ("Global_NULL", "Debug Logs", "./Logs"),
        ("Global_NULL", "Macros", "./Movies/Macros"),
        ("Global_NULL", "Multi-Disk Bundles", "./MultiDisk"),
        ("Global_NULL", "External Tools", "./ExternalTools"),
        ("Global_NULL", "Temp Files", "./Temp"),
        ("GEN", "Base", "./Genesis"),
        ("GEN", "ROM", "./ROM"),
        ("GEN", "Savestates", "./State"),
        ("GEN", "Save RAM", "./SaveRAM"),
        ("GEN", "Screenshots", "./Screenshots"),
        ("GEN", "Cheats", "./Cheats"),
    }
    actual_paths = {
        (entry.get("system"), entry.get("type"), entry.get("path"))
        for entry in paths
        if isinstance(entry, dict)
    }
    if actual_paths != expected_paths or len(paths) != len(expected_paths):
        raise CapabilityError("contained configuration path allowlist drift")
    expected_settings = {
        "UseRecentForRoms": False,
        "StartPaused": False,
        "FirstBoot": False,
        "SingleInstanceMode": False,
        "AutoLoadLastSaveSlot": False,
        "AutoSaveLastSaveSlot": False,
        "AutosaveSaveRAM": False,
        "BackupSaveram": False,
        "UpdateAutoCheckEnabled": False,
        "RACheevosActive": False,
        "Movies": {
            "MovieEndAction": 3,
            "EnableBackupMovies": False,
            "MoviesOnDisk": False,
            "PlaySoundOnMovieEnd": False,
        },
        "SoundEnabled": False,
    }
    if settings != expected_settings:
        raise CapabilityError("contained configuration setting allowlist drift")
    return {
        "PreferredCores": {"GEN": "Genplus-gx"},
        "PathEntries": {
            "Paths": [
                {"System": entry["system"], "Type": entry["type"], "Path": entry["path"]}
                for entry in paths
            ]
        },
        **settings,
    }


def _safe_archive_member(name: str) -> PurePosixPath:
    if not name or "\\" in name or name.startswith(("/", "\\")):
        raise CapabilityError(f"archive member path is not contained: {name!r}")
    if re.match(r"^[A-Za-z]:", name) or any(":" in part for part in name.split("/")):
        raise CapabilityError(f"archive member path uses drive/ADS syntax: {name!r}")
    member = PurePosixPath(name)
    if member.is_absolute() or any(part in {"", ".", ".."} for part in member.parts):
        raise CapabilityError(f"archive member path traverses containment: {name!r}")
    return member


def _validate_archive_members(infos: list[zipfile.ZipInfo]) -> None:
    names = [info.filename for info in infos]
    folded = [name.casefold() for name in names]
    if len(names) != _ARCHIVE_MEMBER_COUNT or len(set(names)) != len(names):
        raise CapabilityError("pristine BizHawk archive member count/duplicate mismatch")
    if len(set(folded)) != len(folded):
        raise CapabilityError("pristine BizHawk archive case-collision")
    if _sha256(_canonical_json(sorted(names))) != _ARCHIVE_MEMBER_SET_SHA256:
        raise CapabilityError("pristine BizHawk archive member-set mismatch")
    for info in infos:
        member = _safe_archive_member(info.filename.rstrip("/"))
        mode = info.external_attr >> 16
        windows_attributes = info.external_attr & 0xFFFF
        if stat.S_ISLNK(mode) or (mode & 0x400) or (windows_attributes & 0x400):
            raise CapabilityError(
                f"pristine BizHawk archive reparse/symlink member: {info.filename}"
            )
        if info.flag_bits & 0x1:
            raise CapabilityError(f"pristine BizHawk archive encrypted member: {info.filename}")
        folded_name = member.as_posix().casefold()
        if any(
            folded_name == forbidden.casefold()
            or folded_name.startswith(forbidden.casefold() + "/")
            for forbidden in _FORBIDDEN_ARCHIVE_MUTABLE_MEMBERS
        ):
            raise CapabilityError(
                f"pristine BizHawk archive contains forbidden mutable member: {info.filename}"
            )


def _archive_facts(fixture: dict[str, Any]) -> tuple[dict[str, Any], Path]:
    """Validate the pristine ZIP before any extraction or process creation."""

    manifest = load_json(TOOLCHAIN_MANIFEST)
    contract = manifest["bizhawk"]
    toolchain = fixture["toolchainContract"]
    archive = repo_path(contract["localArchivePath"]).resolve(strict=True)
    identity = _file_identity(archive)
    if (
        identity["sizeBytes"] != toolchain["archiveSizeBytes"]
        or identity["sha256"] != toolchain["archiveSha256"]
        or toolchain["archiveSizeBytes"] != contract["archiveSizeBytes"]
        or toolchain["archiveSha256"] != contract["archiveSha256"]
    ):
        raise CapabilityError("pristine BizHawk archive identity mismatch")
    try:
        with zipfile.ZipFile(archive, "r") as bundle:
            infos = bundle.infolist()
            _validate_archive_members(infos)
    except zipfile.BadZipFile as error:
        raise CapabilityError(f"pristine BizHawk archive is invalid: {error}") from error
    return {
        **identity,
        "memberCount": _ARCHIVE_MEMBER_COUNT,
        "memberSetSha256": _ARCHIVE_MEMBER_SET_SHA256,
        "hostToolchainRoot": str(
            repo_path(contract["localExecutablePath"]).resolve(strict=True).parent
        ),
        "executableRelativePath": "EmuHawk.exe",
        "lua54RelativePath": "dll/lua54.dll",
    }, archive


def _candidate_identity(
    fixture: dict[str, Any], facts: dict[str, Any], movie: MaterializedMovie
) -> dict[str, Any]:
    components = {
        "romSha256": facts["romSha256"],
        "archiveSha256": facts["archive"]["sha256"],
        "archiveSizeBytes": facts["archive"]["sizeBytes"],
        "archiveMemberSetSha256": facts["archiveMemberSetSha256"],
        "runnerSha256": facts["runner"]["sha256"],
        "helperSha256": facts["helper"]["sha256"],
        "fixtureSha256": facts["fixtureSha256"],
        "capabilitySchemaSha256": _file_identity(CAPABILITY_SCHEMA)["sha256"],
        "receiptSchemaSha256": _file_identity(RECEIPT_SCHEMA)["sha256"],
        "observerSha256": facts["observerSha256"],
        "recipeSha256": movie.recipe_sha256,
        "bk2Sha256": movie.bk2_sha256,
        "configTemplateSha256": _sha256(
            _canonical_json(_config_template(fixture["launchContract"]["configProjection"]))
        ),
    }
    return {"candidateSha256": _sha256(_canonical_json(components)), **components}


def load_capability_fixture(path: Path | None = None) -> dict[str, Any]:
    fixture = load_json((FIXTURE_PATH if path is None else path).resolve(strict=True))
    validate_json(fixture, CAPABILITY_SCHEMA, owner="original-reference replay capability fixture")
    if fixture["capabilityId"] != CAPABILITY_ID:
        raise CapabilityError("original-reference capability ID drift")
    if fixture["receiptContract"]["globalOrdinalOne"] != {
        "ordinal": 1,
        "candidateSha256": _GLOBAL_ORDINAL_ONE_CANDIDATE,
        "receiptSha256": _GLOBAL_ORDINAL_ONE_RECEIPT_SHA256,
    }:
        raise CapabilityError("public global ordinal 1 lock drift")
    return fixture


def _recipe_identity(recipe: dict[str, Any]) -> dict[str, Any]:
    return {
        key: value
        for key, value in recipe.items()
        if key not in {"recipeSha256", "bk2Sha256", "inputLogSha256"}
    }


def _input_rows(recipe: dict[str, Any]) -> tuple[str, ...]:
    key = recipe["inputLogKey"]
    button_order = tuple(recipe["buttonOrder"])
    warm_up = recipe["warmUpRow"]
    if warm_up != {"bk2Row": 0, "port1": []} or recipe["semanticRowOffset"] != 1:
        raise CapabilityError("movie warm-up attachment contract drift")
    rows = ["|.|........|"]
    for expected_frame, row in enumerate(recipe["rows"]):
        if row["frame"] != expected_frame:
            raise CapabilityError(
                f"movie recipe frame order drift: expected {expected_frame}, got {row['frame']}"
            )
        buttons = set(row["port1"])
        unknown = buttons.difference(button_order)
        if unknown:
            raise CapabilityError(f"movie recipe has unknown port-1 button(s): {sorted(unknown)}")
        encoded = "".join(button[0] if button in buttons else "." for button in button_order)
        rows.append(f"|.|{encoded}|")
    if len(rows) != 33:
        raise CapabilityError(f"movie recipe must contain 33 physical rows, got {len(rows)}")
    return (key, *rows)


def _zip_member(name: str, payload: bytes) -> zipfile.ZipInfo:
    info = zipfile.ZipInfo(name, date_time=_DOS_EPOCH)
    info.compress_type = zipfile.ZIP_STORED
    info.create_system = 0
    info.external_attr = 0
    info.flag_bits = 0
    return info


def materialize_movie(fixture: dict[str, Any]) -> MaterializedMovie:
    """Build the fixed, power-on-only BK2 bytes and verify their public identity."""

    recipe = fixture["movieRecipe"]
    recipe_sha256 = _sha256(_canonical_json(_recipe_identity(recipe)))
    if recipe_sha256 != recipe["recipeSha256"]:
        raise CapabilityError(
            f"movie recipe hash drift: expected {recipe['recipeSha256']}, got {recipe_sha256}"
        )
    header = "\n".join(recipe["headerLines"]) + "\n"
    input_log = "\n".join(_input_rows(recipe)) + "\n"
    if _sha256(input_log.encode("utf-8")) != recipe["inputLogSha256"]:
        raise CapabilityError("Input Log hash drift")
    sync_settings = _canonical_json(recipe["syncSettings"])
    members = {
        "Header": header.encode("utf-8"),
        "Input Log": input_log.encode("utf-8"),
        "SyncSettings": sync_settings,
    }
    with tempfile.SpooledTemporaryFile(max_size=1_000_000, mode="w+b") as buffer:
        with zipfile.ZipFile(
            buffer, "w", compression=zipfile.ZIP_STORED, strict_timestamps=True
        ) as archive:
            for name in ZIP_MEMBERS:
                archive.writestr(_zip_member(name, members[name]), members[name])
        buffer.seek(0)
        data = buffer.read()
    result = MaterializedMovie(data, recipe_sha256, _sha256(data), ZIP_MEMBERS)
    if result.bk2_sha256 != recipe["bk2Sha256"]:
        raise CapabilityError(
            f"BK2 hash drift: expected {recipe['bk2Sha256']}, got {result.bk2_sha256}"
        )
    validate_materialized_movie(result.data, fixture)
    return result


def validate_materialized_movie(data: bytes, fixture: dict[str, Any]) -> None:
    """Reject a near-miss archive before it can be passed to EmuHawk."""

    recipe = fixture["movieRecipe"]
    try:
        with zipfile.ZipFile(io.BytesIO(data), "r") as archive:
            infos = archive.infolist()
            names = tuple(info.filename for info in infos)
            if names != ZIP_MEMBERS:
                raise CapabilityError(
                    f"BK2 member order drift: expected {ZIP_MEMBERS}, got {names}"
                )
            if any(info.date_time != _DOS_EPOCH for info in infos):
                raise CapabilityError("BK2 member timestamp drift")
            if any(info.compress_type != zipfile.ZIP_STORED for info in infos):
                raise CapabilityError("BK2 member compression drift")
            forbidden = {"CoreState", "SaveRAM", "SRAM"}.intersection(names)
            if forbidden:
                raise CapabilityError(
                    f"BK2 contains forbidden state member(s): {sorted(forbidden)}"
                )
            expected_header = ("\n".join(recipe["headerLines"]) + "\n").encode("utf-8")
            expected_log = ("\n".join(_input_rows(recipe)) + "\n").encode("utf-8")
            expected_sync = _canonical_json(recipe["syncSettings"])
            if archive.read("Header") != expected_header:
                raise CapabilityError("BK2 Header readback drift")
            if archive.read("Input Log") != expected_log:
                raise CapabilityError("BK2 Input Log readback drift")
            if archive.read("SyncSettings") != expected_sync:
                raise CapabilityError("BK2 SyncSettings readback drift")
    except zipfile.BadZipFile as error:
        raise CapabilityError(f"BK2 is not a valid ZIP archive: {error}") from error


_ALLOWED_LUA_APIS = {
    "movie": {
        "isloaded",
        "getreadonly",
        "startsfromsavestate",
        "startsfromsaveram",
        "length",
        "mode",
        "getheader",
        "getinput",
        "getinputasmnemonic",
    },
    "event": {"oninputpoll", "onexit", "unregisterbyid"},
    "emu": {"framecount", "getsystemid", "frameadvance"},
    "joypad": {"get"},
    "client": {"getversion", "exitCode"},
    "os": {"getenv"},
    "io": {"open"},
}
_ALLOWED_LUA_STANDARD_APIS = {
    "string": {"byte", "char", "format", "sub"},
    "table": {"concat"},
}
_ALLOWED_LUA_BARE_CALLS = {"ipairs", "pcall", "tonumber", "tostring"}
_LUA_KEYWORDS = {"function"}
_ALLOWED_LUA_API_NAMES = (
    "movie.isloaded",
    "movie.getreadonly",
    "movie.startsfromsavestate",
    "movie.startsfromsaveram",
    "movie.length",
    "movie.mode",
    "movie.getheader",
    "movie.getinput",
    "movie.getinputasmnemonic",
    "event.oninputpoll",
    "event.onexit",
    "event.unregisterbyid",
    "emu.framecount",
    "emu.getsystemid",
    "emu.frameadvance",
    "joypad.get",
    "client.getversion",
    "client.exitCode",
    "os.getenv",
    "io.open",
)
_FORBIDDEN_CAPABILITIES = (
    "adaptive-input",
    "bootstrap",
    "dynamic-call",
    "gameplay-mechanics",
    "memory-write",
    "movie-mutation",
    "rom-control",
    "savestate",
    "shell-process",
)
_FORBIDDEN_LUA_PATTERNS = (
    "joypad.set",
    "memory.",
    "savestate.",
    "emu.set",
    "movie.play",
    "movie.save",
    "movie.set",
    "movie.stop",
    "client.openrom",
    "client.closerom",
    "client.reboot",
    "os.execute",
    "io.popen",
    "require",
    "dofile",
    "loadfile",
    "_G",
    "_ENV",
    "rawget",
    "setmetatable",
    "getmetatable",
    "package.",
    "debug.",
    "bootstrap.lua",
)


def validate_passive_observer(fixture: dict[str, Any], path: Path = OBSERVER_PATH) -> str:
    """Check canonical final bytes and deny aliases/dynamic calls before compilation."""

    source = _canonical_observer_transport_bytes(path)
    digest = _sha256(source)
    policy = fixture["passiveObserverPolicy"]
    if tuple(policy["allowedApis"]) != _ALLOWED_LUA_API_NAMES:
        raise CapabilityError("passive observer allowed API contract drift")
    if tuple(policy["forbiddenCapabilities"]) != _FORBIDDEN_CAPABILITIES:
        raise CapabilityError("passive observer forbidden capability contract drift")
    if digest != policy["observerSha256"]:
        raise CapabilityError(
            f"passive observer hash drift: expected {policy['observerSha256']}, got {digest}"
        )
    text = source.decode("utf-8")
    lowered = text.lower()
    for pattern in _FORBIDDEN_LUA_PATTERNS:
        if pattern.lower() in lowered:
            raise CapabilityError(f"passive observer uses forbidden Lua surface: {pattern}")
    if re.search(r"\b[A-Za-z_][A-Za-z0-9_]*\s*\[[^\]]*\]\s*\(", text):
        raise CapabilityError("passive observer uses dynamic member access")
    for namespace in _ALLOWED_LUA_APIS:
        if re.search(
            rf"\blocal\s+[A-Za-z_][A-Za-z0-9_]*\s*=\s*{namespace}\b(?!\s*\.)",
            text,
        ):
            raise CapabilityError(f"passive observer aliases API namespace: {namespace}")
    for match in re.finditer(r"\b([a-z_][a-z0-9_]*)\.([A-Za-z_][A-Za-z0-9_]*)", text):
        namespace, method = match.groups()
        if not text[match.end() :].lstrip().startswith("("):
            continue
        allowed_methods = _ALLOWED_LUA_APIS.get(namespace) or _ALLOWED_LUA_STANDARD_APIS.get(
            namespace
        )
        if allowed_methods is None or method not in allowed_methods:
            raise CapabilityError(f"passive observer uses unallowed API: {namespace}.{method}")
    local_functions = set(re.findall(r"\blocal\s+function\s+([A-Za-z_][A-Za-z0-9_]*)\s*\(", text))
    for match in re.finditer(r"\b([A-Za-z_][A-Za-z0-9_]*)\s*\(", text):
        name = match.group(1)
        if match.start() > 0 and text[match.start() - 1] in {".", ":"}:
            continue
        if name in _LUA_KEYWORDS or name in local_functions or name in _ALLOWED_LUA_BARE_CALLS:
            continue
        raise CapabilityError(f"passive observer uses unallowed Lua call: {name}")
    if re.search(r"\bload\s*\(", text):
        raise CapabilityError("passive observer uses forbidden Lua surface: load")
    return digest


def _materialize_passive_observer(
    fixture: dict[str, Any], source_path: Path, contained_path: Path
) -> str:
    """Write and read back the validated canonical observer bytes for launch."""

    source = _canonical_observer_transport_bytes(source_path)
    contained_path.write_bytes(source)
    digest = validate_passive_observer(fixture, contained_path)
    if contained_path.read_bytes() != source:
        raise CapabilityError("contained passive observer readback drift")
    return digest


def _preflight(fixture: dict[str, Any], rom_path: Path) -> tuple[dict[str, Any], MaterializedMovie]:
    try:
        rom_path = rom_path.resolve(strict=True)
    except FileNotFoundError as error:
        raise PrivateInputUnavailable(f"private ROM unavailable: {rom_path}") from error
    actual_rom = inspect_rom(rom_path)
    try:
        archive_identity, archive = _archive_facts(fixture)
    except FileNotFoundError as error:
        raise PrivateInputUnavailable("pristine BizHawk archive unavailable") from error
    toolchain = fixture["toolchainContract"]
    if actual_rom["sha256"] != toolchain["romSha256"]:
        raise CapabilityError(
            f"original-reference ROM mismatch: expected {toolchain['romSha256']}, "
            f"got {actual_rom['sha256']}"
        )
    if fixture["launchContract"]["platform"] != "GEN":
        raise CapabilityError("original-reference replay requires platform GEN")
    observer_digest = validate_passive_observer(fixture)
    movie = materialize_movie(fixture)
    return {
        "romSha256": actual_rom["sha256"],
        "romIdentity": _file_identity(rom_path),
        "archive": {
            "sha256": archive_identity["sha256"],
            "sizeBytes": archive_identity["sizeBytes"],
        },
        "archiveMemberSetSha256": archive_identity["memberSetSha256"],
        "archivePath": str(archive),
        "hostToolchainRoot": archive_identity["hostToolchainRoot"],
        "runner": _file_identity(Path(__file__)),
        "helper": _file_identity(Path(run_native_bizhawk_process.__code__.co_filename)),
        "fixtureSha256": _sha256(FIXTURE_PATH.read_bytes()),
        "observerSha256": observer_digest,
    }, movie


def preflight_original_reference_replay(rom_path: Path) -> dict[str, Any]:
    """Perform all deterministic checks without creating a movie or process."""

    try:
        fixture = load_capability_fixture()
        facts, movie = _preflight(fixture, rom_path)
    except PrivateInputUnavailable as error:
        return {
            "Status": "UNAVAILABLE",
            "Mode": "PREFLIGHT",
            "CapabilityId": CAPABILITY_ID,
            "Failure": str(error),
            "ProcessStarts": 0,
        }
    except FileNotFoundError as error:
        return {
            "Status": "FAIL",
            "Mode": "PREFLIGHT",
            "CapabilityId": CAPABILITY_ID,
            "Failure": str(error),
            "ProcessStarts": 0,
        }
    except (CapabilityError, OSError, ValueError, json.JSONDecodeError) as error:
        return {
            "Status": "FAIL",
            "Mode": "PREFLIGHT",
            "CapabilityId": CAPABILITY_ID,
            "Failure": str(error),
            "ProcessStarts": 0,
        }
    return {
        "Status": "PASS",
        "Mode": "PREFLIGHT",
        "CapabilityId": CAPABILITY_ID,
        "RecipeSha256": movie.recipe_sha256,
        "Bk2Sha256": movie.bk2_sha256,
        "ObserverSha256": facts["observerSha256"],
        "CandidateSha256": _candidate_identity(fixture, facts, movie)["candidateSha256"],
        "ProcessStarts": 0,
    }


def _reject_reparse(path: Path) -> None:
    """Reject symlink/reparse traversal on all containment boundaries."""

    if path.is_symlink():
        raise CapabilityError(f"replay containment rejects symlink: {path}")
    try:
        attributes = os.lstat(path).st_file_attributes
    except (AttributeError, FileNotFoundError):
        return
    if attributes & 0x400:
        raise CapabilityError(f"replay containment rejects reparse point: {path}")


def _contained_launch_directory(candidate_hash: str, ordinal: int) -> Path:
    if ordinal < 1 or ordinal > MAX_LAUNCH_ORDINAL:
        raise CapabilityError(f"launch ordinal must be 1..{MAX_LAUNCH_ORDINAL}, got {ordinal}")
    if len(candidate_hash) != 64 or any(
        character not in "0123456789ABCDEF" for character in candidate_hash
    ):
        raise CapabilityError("candidate hash must be an uppercase SHA-256")
    configured_root = DERIVED_ROOT.absolute()
    if configured_root.exists():
        _reject_reparse(configured_root)
    root = configured_root.resolve()
    for parent in (root, root.parent, root.parent.parent, root.parent.parent.parent):
        if parent.exists():
            _reject_reparse(parent)
    candidate = root / candidate_hash
    launch = candidate / f"launch-{ordinal}"
    if launch.exists() or launch.is_symlink():
        raise CapabilityError(f"replay launch directory already exists: {launch}")
    candidate.mkdir(parents=True, exist_ok=True)
    _reject_reparse(candidate)
    launch.mkdir()
    if launch.resolve().parent != candidate.resolve():
        raise CapabilityError("replay launch directory escaped candidate containment")
    return launch


def _ensure_contained(path: Path, root: Path) -> Path:
    resolved_root = root.resolve(strict=True)
    resolved = path.resolve(strict=False)
    if resolved != resolved_root and resolved_root not in resolved.parents:
        raise CapabilityError(f"replay path escaped contained launch root: {path}")
    return resolved


def _inventory_mutable_surfaces(root: Path) -> dict[str, Any]:
    """Hash the declared write surface without following links or reading host payloads into Git."""

    _reject_reparse(root)
    root = root.resolve(strict=True)
    entries: list[dict[str, Any]] = []
    for relative in _MUTABLE_SURFACES:
        surface = root / relative
        if not surface.exists():
            entries.append(
                {
                    "surface": relative,
                    "kind": "absent",
                    "sha256": None,
                    "sizeBytes": 0,
                    "fileCount": 0,
                }
            )
            continue
        _reject_reparse(surface)
        if surface.is_file():
            entries.append(
                {
                    "surface": relative,
                    "kind": "file",
                    **_file_identity(surface),
                    "fileCount": 1,
                }
            )
            continue
        files: list[dict[str, Any]] = []
        for current, directories, names in os.walk(surface, followlinks=False):
            current_path = Path(current)
            _ensure_contained(current_path, root)
            _reject_reparse(current_path)
            for directory in directories:
                _reject_reparse(current_path / directory)
            for name in names:
                item = current_path / name
                _reject_reparse(item)
                if not item.is_file():
                    raise CapabilityError(f"mutable surface contains non-file entry: {item}")
                files.append(
                    {
                        "path": item.relative_to(root).as_posix(),
                        **_file_identity(item),
                    }
                )
        entries.append(
            {
                "surface": relative,
                "kind": "directory",
                "fileCount": len(files),
                "sha256": _sha256(_canonical_json(sorted(files, key=lambda item: item["path"]))),
                "sizeBytes": sum(item["sizeBytes"] for item in files),
            }
        )
    return {
        "surfaceCount": len(entries),
        "sha256": _sha256(_canonical_json(entries)),
        "entries": entries,
    }


def _snapshot_inventory(root: Path) -> dict[str, Any]:
    """Retain a typed post-launch inventory failure instead of bypassing receipt cleanup."""

    try:
        return {
            "status": "captured",
            "inventory": _inventory_mutable_surfaces(root),
            "error": None,
        }
    except (CapabilityError, OSError) as error:
        return {"status": "unavailable", "inventory": None, "error": str(error)}


def _host_tree_entry(path: Path, root: Path, metadata: os.stat_result) -> dict[str, Any]:
    relative = path.relative_to(root).as_posix()
    if not relative or relative == "." or relative.startswith("../"):
        raise CapabilityError(f"host toolchain entry escaped containment: {path}")
    last_write_ticks = metadata.st_mtime_ns // 100
    if stat.S_ISREG(metadata.st_mode):
        return {
            "kind": "file",
            "path": relative,
            **_file_identity(path),
            "lastWriteUtcTicks": last_write_ticks,
        }
    if stat.S_ISDIR(metadata.st_mode):
        return {
            "kind": "directory",
            "path": relative,
            "sha256": None,
            "sizeBytes": 0,
            "lastWriteUtcTicks": last_write_ticks,
        }
    raise CapabilityError(f"host toolchain contains non-file entry: {path}")


def _inventory_host_toolchain_tree(root: Path) -> dict[str, Any]:
    """Capture every regular file and empty directory without following host reparse points."""

    _reject_reparse(root)
    root = root.resolve(strict=True)
    entries: list[dict[str, Any]] = []
    for current, directories, names in os.walk(root, topdown=True, followlinks=False):
        current_path = Path(current)
        _ensure_contained(current_path, root)
        _reject_reparse(current_path)
        directories.sort()
        names.sort()
        for directory in directories:
            child = current_path / directory
            _ensure_contained(child, root)
            _reject_reparse(child)
            if not stat.S_ISDIR(os.lstat(child).st_mode):
                raise CapabilityError(f"host toolchain contains non-directory walk entry: {child}")
        for name in names:
            item = current_path / name
            _ensure_contained(item, root)
            _reject_reparse(item)
            entries.append(_host_tree_entry(item, root, os.lstat(item)))
        if current_path != root and not directories and not names:
            entries.append(_host_tree_entry(current_path, root, os.lstat(current_path)))
    entries.sort(key=lambda entry: entry["path"])
    return {
        "entryCount": len(entries),
        "sha256": _sha256(_canonical_json(entries)),
        "entries": entries,
    }


def _snapshot_host_toolchain_tree(root: Path) -> dict[str, Any]:
    """Convert a post-run host-tree readback failure into typed receipt state."""

    try:
        return {
            "status": "captured",
            "tree": _inventory_host_toolchain_tree(root),
            "error": None,
        }
    except (CapabilityError, OSError) as error:
        return {"status": "unavailable", "tree": None, "error": str(error)}


def _first_host_toolchain_difference(
    expected: dict[str, Any], actual: dict[str, Any]
) -> tuple[dict[str, Any] | None, dict[str, Any] | None]:
    expected_entries = {entry["path"]: entry for entry in expected["entries"]}
    actual_entries = {entry["path"]: entry for entry in actual["entries"]}
    for relative in sorted(set(expected_entries) | set(actual_entries)):
        if expected_entries.get(relative) != actual_entries.get(relative):
            return expected_entries.get(relative), actual_entries.get(relative)
    raise CapabilityError("host toolchain inventory digest changed without an entry difference")


def _extract_contained_toolchain(
    archive: Path, launch: Path, expected_archive: dict[str, Any]
) -> Path:
    """Extract only validated, regular archive members below one fresh launch directory."""

    toolchain = launch / "toolchain"
    if toolchain.exists():
        raise CapabilityError("contained toolchain target already exists")
    toolchain.mkdir()
    _ensure_contained(toolchain, launch)
    try:
        if _file_identity(archive) != expected_archive:
            raise CapabilityError("pristine BizHawk archive changed after preflight")
        with zipfile.ZipFile(archive, "r") as bundle:
            infos = bundle.infolist()
            _validate_archive_members(infos)
            for info in infos:
                name = _safe_archive_member(info.filename.rstrip("/"))
                target = toolchain.joinpath(*name.parts)
                _ensure_contained(target, toolchain)
                if info.is_dir():
                    target.mkdir(parents=True, exist_ok=True)
                    continue
                target.parent.mkdir(parents=True, exist_ok=True)
                with bundle.open(info, "r") as source, target.open("xb") as destination:
                    shutil.copyfileobj(source, destination)
    except (OSError, zipfile.BadZipFile) as error:
        raise CapabilityError(f"contained toolchain extraction failed: {error}") from error
    return toolchain


def _prepare_contained_launch(
    fixture: dict[str, Any],
    facts: dict[str, Any],
    movie: MaterializedMovie,
    launch: Path,
    rom_path: Path,
) -> dict[str, Any]:
    toolchain = _extract_contained_toolchain(Path(facts["archivePath"]), launch, facts["archive"])
    executable = toolchain / "EmuHawk.exe"
    lua54 = toolchain / "dll" / "lua54.dll"
    if not executable.is_file() or not lua54.is_file():
        raise CapabilityError("pristine archive lacks contained EmuHawk/lua54")
    toolchain_contract = fixture["toolchainContract"]
    if _file_identity(executable) != {
        "sha256": toolchain_contract["executableSha256"],
        "sizeBytes": toolchain_contract["executableSizeBytes"],
    }:
        raise CapabilityError("contained EmuHawk identity mismatch")
    if _file_identity(lua54) != {
        "sha256": toolchain_contract["lua54Sha256"],
        "sizeBytes": toolchain_contract["lua54SizeBytes"],
    }:
        raise CapabilityError("contained lua54 identity mismatch")
    config_path = toolchain / "config.ini"
    config_path.write_bytes(
        _canonical_json(_config_template(fixture["launchContract"]["configProjection"]))
    )
    movie_path = toolchain / "Movies" / "replay.bk2"
    movie_path.parent.mkdir(parents=True, exist_ok=True)
    movie_path.write_bytes(movie.data)
    observer_path = toolchain / "Lua" / OBSERVER_PATH.name
    observer_path.parent.mkdir(parents=True, exist_ok=True)
    observer_digest = _materialize_passive_observer(fixture, OBSERVER_PATH, observer_path)
    if observer_digest != facts["observerSha256"]:
        raise CapabilityError("contained passive observer identity drift")
    private_rom = toolchain / "ROM" / "sf2.bin"
    private_rom.parent.mkdir(parents=True, exist_ok=True)
    shutil.copyfile(rom_path, private_rom)
    validate_lua_syntax(observer_path, executable)
    return {
        "toolchain": toolchain,
        "executable": executable,
        "config": config_path,
        "movie": movie_path,
        "observer": observer_path,
        "rom": private_rom,
    }


def _launch_command(
    executable: Path, config_path: Path, movie_path: Path, observer_path: Path, rom_path: Path
) -> list[str]:
    return [
        str(executable),
        "--chromeless",
        f"--config={config_path}",
        f"--movie={movie_path}",
        f"--lua={observer_path}",
        str(rom_path),
    ]


def _safe_delete_launch_files(launch: Path, receipt_path: Path) -> dict[str, Any]:
    _reject_reparse(launch)
    launch = launch.resolve(strict=True)
    receipt_path = receipt_path.resolve(strict=False)
    if receipt_path.parent != launch:
        raise CapabilityError("receipt path is outside the contained launch directory")
    removed = []
    mismatch: str | None = None
    for child in sorted(launch.iterdir(), key=lambda item: item.name):
        if child == receipt_path:
            continue
        try:
            _reject_reparse(child)
            if child.is_dir():
                _ensure_contained(child, launch)
                for current, directories, names in os.walk(child, topdown=False, followlinks=False):
                    current_path = Path(current)
                    _ensure_contained(current_path, launch)
                    _reject_reparse(current_path)
                    for name in names:
                        item = current_path / name
                        _reject_reparse(item)
                        item.unlink()
                    for directory in directories:
                        item = current_path / directory
                        _reject_reparse(item)
                        item.rmdir()
                child.rmdir()
                removed.append(child.name)
                continue
            child.unlink()
            removed.append(child.name)
        except (CapabilityError, OSError) as error:
            mismatch = f"{child.name}: {error}"
            break
    residual = sorted(child.name for child in launch.iterdir() if child != receipt_path)
    return {
        "removedArtifacts": removed,
        "residualArtifacts": residual,
        "firstMismatch": mismatch,
    }


def canonical_replay_digest(receipt: dict[str, Any]) -> str:
    """Hash only declared replay semantics, never PID/path/ordinal/wall-clock diagnostics."""

    value = {
        "capabilityId": receipt["capabilityId"],
        "candidateIdentity": receipt["candidateIdentity"],
        "runner": {
            "bizhawkRelease": receipt["runner"]["bizhawkRelease"],
            "core": receipt["runner"]["core"],
            "configAfter": receipt["runner"]["configAfter"],
        },
        "movie": {
            "recipeSha256": receipt["movie"]["recipeSha256"],
            "bk2Sha256": receipt["movie"]["bk2Sha256"],
        },
        "observer": {
            key: receipt["observer"][key]
            for key in (
                "sha256",
                "statusPresent",
                "inputPollTrace",
                "initialFrame",
                "firstPollFrame",
                "terminalFrame",
                "movieMode",
                "readOnly",
                "powerOn",
                "headerPlatform",
                "headerCore",
                "clientVersion",
                "statusWriteOk",
            )
        },
        "isolation": {
            "hostDrift": receipt["isolation"]["hostDrift"],
            "hostToolchainDrift": receipt["isolation"]["hostToolchainDrift"],
        },
    }
    return _sha256(_canonical_json(value))


def _receipt(
    *,
    fixture: dict[str, Any],
    run_class: str,
    ordinal: int,
    facts: dict[str, Any],
    movie: MaterializedMovie,
    status: str,
    observer_status: dict[str, Any] | None,
    process: dict[str, Any],
    failure: dict[str, Any] | None,
    cleanup: dict[str, Any],
    config_before: dict[str, Any],
    config_after: dict[str, Any],
    archive_post: dict[str, Any],
    emulator: dict[str, Any],
    lua54: dict[str, Any],
    host_before: dict[str, Any],
    host_after: dict[str, Any],
    host_toolchain_before: dict[str, Any],
    host_toolchain_after: dict[str, Any],
    toolchain_before: dict[str, Any],
    toolchain_after: dict[str, Any],
    rom_post: dict[str, Any],
    host_drift: bool | None,
    host_toolchain_drift: bool | None,
    prelaunch_identity: dict[str, str],
) -> dict[str, Any]:
    status_present = observer_status is not None
    observer_status = {} if observer_status is None else observer_status
    receipt = {
        "schemaVersion": 1,
        "capabilityId": CAPABILITY_ID,
        "runClass": run_class,
        "launchOrdinal": ordinal,
        "candidateIdentity": prelaunch_identity,
        "inputs": {
            "startState": "power-on",
            "saveState": False,
            "saveRam": False,
            "romPre": facts["romIdentity"],
            "romPost": rom_post,
        },
        "runner": {
            "emulatorPath": "contained-toolchain/EmuHawk.exe",
            "processStarts": int(process["started"]),
            "bizhawkRelease": fixture["toolchainContract"]["bizhawkRelease"],
            "core": fixture["toolchainContract"]["core"],
            "archive": facts["archive"],
            "archivePost": archive_post,
            "emulator": emulator,
            "lua54": lua54,
            "runner": facts["runner"],
            "helper": facts["helper"],
            "configBefore": config_before,
            "configAfter": config_after,
        },
        "movie": {
            "recipeSha256": movie.recipe_sha256,
            "bk2Sha256": movie.bk2_sha256,
            "inputLogSha256": fixture["movieRecipe"]["inputLogSha256"],
            "physicalRows": 33,
            "semanticRows": 32,
        },
        "observer": {
            "sha256": facts["observerSha256"],
            "statusPresent": status_present,
            "callbacksRemaining": observer_status.get("callbacksRemaining"),
            "status": observer_status.get("status"),
            "inputPollTrace": observer_status.get("inputPollTrace"),
            "initialFrame": observer_status.get("initialFrame"),
            "firstPollFrame": observer_status.get("firstPollFrame"),
            "terminalFrame": observer_status.get("terminalFrame"),
            "movieMode": observer_status.get("movieMode"),
            "readOnly": observer_status.get("readOnly"),
            "powerOn": observer_status.get("powerOn"),
            "headerPlatform": observer_status.get("headerPlatform"),
            "headerCore": observer_status.get("headerCore"),
            "clientVersion": observer_status.get("clientVersion"),
            "statusWriteOk": observer_status.get("statusWriteOk"),
        },
        "execution": {"status": status, **process},
        "determinism": {"replayDigest": ""},
        "isolation": {
            "hostBefore": host_before,
            "hostAfter": host_after,
            "hostToolchainBefore": host_toolchain_before,
            "hostToolchainAfter": host_toolchain_after,
            "toolchainBefore": toolchain_before,
            "toolchainAfter": toolchain_after,
            "hostDrift": host_drift,
            "hostToolchainDrift": host_toolchain_drift,
        },
        "cleanup": cleanup,
        "failure": failure,
    }
    receipt["determinism"]["replayDigest"] = canonical_replay_digest(receipt)
    validate_json(receipt, RECEIPT_SCHEMA, owner="original-reference replay receipt")
    return receipt


def _write_receipt(path: Path, receipt: dict[str, Any]) -> None:
    path.write_bytes(_canonical_json(receipt))


def _load_observer_status(path: Path) -> dict[str, Any] | None:
    _reject_reparse(path)
    if not path.is_file():
        return None
    try:
        status = load_json(path)
    except (OSError, json.JSONDecodeError) as error:
        raise CapabilityError(f"passive observer status is invalid: {error}") from error
    if not isinstance(status, dict):
        raise CapabilityError("passive observer status must be an object")
    required = {
        "status",
        "callbacksRemaining",
        "moviePosition",
        "inputPollTrace",
        "initialFrame",
        "firstPollFrame",
        "terminalFrame",
        "movieMode",
        "readOnly",
        "powerOn",
        "headerPlatform",
        "headerCore",
        "clientVersion",
        "statusWriteOk",
    }
    if set(status) != required:
        raise CapabilityError("passive observer status keys drift")
    if (
        not isinstance(status["status"], str)
        or not isinstance(status["callbacksRemaining"], int)
        or status["callbacksRemaining"] < 0
        or not isinstance(status["moviePosition"], int)
        or status["moviePosition"] < 0
        or not isinstance(status["inputPollTrace"], list)
    ):
        raise CapabilityError("passive observer status types drift")
    for semantic_index, row in enumerate(status["inputPollTrace"]):
        if set(row) != {"semanticIndex", "bk2Row", "emuFrame", "input"}:
            raise CapabilityError("passive observer status trace keys drift")
        if (
            not isinstance(row["semanticIndex"], int)
            or not isinstance(row["bk2Row"], int)
            or not isinstance(row["emuFrame"], int)
            or not isinstance(row["input"], str)
            or not re.fullmatch(r"\|\.\|[.UDLRABCS]{8}\|", row["input"])
            or row["semanticIndex"] != semantic_index
            or row["bk2Row"] != semantic_index + 1
            or row["emuFrame"] != semantic_index + 1
        ):
            raise CapabilityError("passive observer status semantic row mapping drift")
    return status


def _ledger_path() -> Path:
    return DERIVED_ROOT / "launch-ledger.json"


def _load_ledger() -> list[dict[str, Any]]:
    path = _ledger_path()
    if not path.exists():
        return []
    _reject_reparse(path)
    value = load_json(path)
    if not isinstance(value, list):
        raise CapabilityError("launch ledger must be a list")
    return value


def _ledger_receipt_path(candidate_sha256: str, ordinal: int) -> Path:
    return DERIVED_ROOT / candidate_sha256 / f"launch-{ordinal}" / "receipt.json"


def _ledger_path_matches(row: dict[str, Any]) -> bool:
    expected = _ledger_receipt_path(row["candidateSha256"], row["ordinal"])
    return Path(row["receiptPath"]).resolve(strict=False) == expected.resolve(strict=False)


def _validate_ledger_row(row: Any) -> dict[str, Any]:
    if not isinstance(row, dict):
        raise CapabilityError("launch ledger row must be an object")
    basic_keys = {
        "ordinal",
        "candidateSha256",
        "runClass",
        "receiptPath",
        "receiptSha256",
        "status",
    }
    if not basic_keys.issubset(row):
        raise CapabilityError("launch ledger row misses required fields")
    ordinal = row["ordinal"]
    if isinstance(ordinal, bool) or not isinstance(ordinal, int) or ordinal not in {1, 2, 3}:
        raise CapabilityError("launch ledger ordinal must be numeric 1, 2, or 3")
    if not isinstance(row["candidateSha256"], str) or not _SHA256_RE.fullmatch(
        row["candidateSha256"]
    ):
        raise CapabilityError("launch ledger candidate hash is invalid")
    if row["runClass"] not in {"diagnostic", "frozen-acceptance"}:
        raise CapabilityError("launch ledger run class is invalid")
    if not isinstance(row["receiptPath"], str) or not row["receiptPath"]:
        raise CapabilityError("launch ledger receipt path is invalid")
    if not _ledger_path_matches(row):
        raise CapabilityError("launch ledger receipt path does not match ordinal")
    status = row["status"]
    if status not in {"RESERVED", "STARTED", "PASS", "FAIL"}:
        raise CapabilityError("launch ledger status is invalid")
    expected_keys = set(basic_keys)
    if status in {"STARTED", "PASS", "FAIL"}:
        expected_keys.add("processStarted")
        if row.get("processStarted") is not True and status in {"STARTED", "PASS"}:
            raise CapabilityError("launch ledger started/PASS row must have processStarted true")
        if not isinstance(row.get("processStarted"), bool):
            raise CapabilityError("launch ledger processStarted must be boolean")
        if row["processStarted"]:
            expected_keys.add("pid")
            if isinstance(row.get("pid"), bool) or not isinstance(row.get("pid"), int):
                raise CapabilityError("launch ledger started process needs numeric pid")
    if set(row) != expected_keys:
        raise CapabilityError("launch ledger row keys do not match transition state")
    receipt_sha256 = row["receiptSha256"]
    if status in {"RESERVED", "STARTED"}:
        if receipt_sha256 is not None:
            raise CapabilityError("launch ledger nonterminal row has receipt hash")
    elif not isinstance(receipt_sha256, str) or not _SHA256_RE.fullmatch(receipt_sha256):
        raise CapabilityError("launch ledger terminal row has invalid receipt hash")
    return row


def _validate_global_ordinal_one(row: dict[str, Any]) -> None:
    if (
        row["ordinal"] != 1
        or row["candidateSha256"] != _GLOBAL_ORDINAL_ONE_CANDIDATE
        or row["runClass"] != "diagnostic"
        or row["status"] != "FAIL"
        or row["receiptSha256"] != _GLOBAL_ORDINAL_ONE_RECEIPT_SHA256
        or row.get("processStarted") is not True
    ):
        raise CapabilityError("global ordinal 1 lock mismatch")
    path = Path(row["receiptPath"])
    try:
        _ensure_contained(path, DERIVED_ROOT)
        _reject_reparse(path)
        if _file_identity(path)["sha256"] != _GLOBAL_ORDINAL_ONE_RECEIPT_SHA256:
            raise CapabilityError("global ordinal 1 receipt bytes mismatch")
    except (FileNotFoundError, OSError) as error:
        raise CapabilityError(f"global ordinal 1 receipt unavailable: {error}") from error


def _validate_ledger(rows: list[dict[str, Any]]) -> list[dict[str, Any]]:
    if not isinstance(rows, list):
        raise CapabilityError("launch ledger must be a list")
    validated = [_validate_ledger_row(row) for row in rows]
    ordinals = [row["ordinal"] for row in validated]
    if len(set(ordinals)) != len(ordinals):
        raise CapabilityError("launch ledger has duplicate ordinal")
    ordinal_one = [row for row in validated if row["ordinal"] == 1]
    if len(ordinal_one) != 1:
        raise CapabilityError("launch ledger must retain exactly one global ordinal 1")
    _validate_global_ordinal_one(ordinal_one[0])
    return validated


def _launch_ledger_row(
    rows: list[dict[str, Any]], candidate_sha256: str, ordinal: int
) -> dict[str, Any]:
    matches = [
        row
        for row in rows
        if row["candidateSha256"] == candidate_sha256 and row["ordinal"] == ordinal
    ]
    if len(matches) != 1:
        raise CapabilityError("launch ledger transition row is missing or duplicated")
    return matches[0]


def _write_ledger(rows: list[dict[str, Any]]) -> None:
    path = _ledger_path()
    path.parent.mkdir(parents=True, exist_ok=True)
    _reject_reparse(path.parent)
    if path.exists():
        _reject_reparse(path)
    temporary = path.with_suffix(".tmp")
    if temporary.exists():
        raise CapabilityError("launch ledger temporary collision")
    temporary.write_bytes(_canonical_json(rows))
    os.replace(temporary, path)


def _prior_ordinal_two_receipt(
    candidate: Path, identity: dict[str, Any], row: dict[str, Any]
) -> dict[str, Any]:
    path = candidate / "launch-2" / "receipt.json"
    if Path(row["receiptPath"]).resolve(strict=False) != path.resolve(strict=False):
        raise CapabilityError("ordinal 2 ledger receipt path mismatch")
    try:
        _ensure_contained(path, DERIVED_ROOT)
        _reject_reparse(path)
        actual_sha256 = _file_identity(path)["sha256"]
    except (FileNotFoundError, OSError) as error:
        raise CapabilityError(f"ordinal 2 receipt unavailable: {error}") from error
    if actual_sha256 != row["receiptSha256"]:
        raise CapabilityError("ordinal 2 receipt bytes do not match ledger hash")
    receipt = load_json(path)
    validate_json(receipt, RECEIPT_SCHEMA, owner="prior original-reference replay receipt")
    if (
        receipt["candidateIdentity"] != identity
        or receipt["launchOrdinal"] != 2
        or receipt["runClass"] != "diagnostic"
        or receipt["execution"]["status"] != "PASS"
    ):
        raise CapabilityError("ordinal 2 receipt terminal contract mismatch")
    return receipt


def _validate_launch_sequence(
    candidate: Path, identity: dict[str, Any], run_class: str, ordinal: int
) -> list[dict[str, Any]]:
    ledger = _validate_ledger(_load_ledger())
    if any(row["ordinal"] == ordinal for row in ledger):
        raise CapabilityError(f"launch ordinal {ordinal} is already globally reserved")
    if run_class not in {"diagnostic", "frozen-acceptance"}:
        raise CapabilityError("run class must be diagnostic or frozen-acceptance")
    if run_class == "diagnostic" and ordinal != 2:
        raise CapabilityError("corrected diagnostic uses only ordinal 2")
    if run_class == "frozen-acceptance" and ordinal != ACCEPTANCE_LAUNCH_ORDINAL:
        raise CapabilityError("frozen acceptance uses ordinal 3")
    if ordinal == 2 and [row["ordinal"] for row in ledger] != [1]:
        raise CapabilityError("corrected ordinal 2 requires only locked global ordinal 1")
    if ordinal == 3:
        if [row["ordinal"] for row in ledger] != [1, 2]:
            raise CapabilityError("frozen acceptance requires exactly ordinal 1 then ordinal 2")
        ordinal_two = ledger[1]
        if (
            ordinal_two["candidateSha256"] != identity["candidateSha256"]
            or ordinal_two["runClass"] != "diagnostic"
            or ordinal_two["status"] != "PASS"
        ):
            raise CapabilityError("frozen acceptance requires same-candidate ordinal 2 PASS")
        return [_prior_ordinal_two_receipt(candidate, identity, ordinal_two)]
    return []


def run_original_reference_replay(
    *,
    rom_path: Path,
    preflight_only: bool = False,
    run_class: str | None = None,
    launch_ordinal: int | None = None,
    timeout_seconds: int = 120,
) -> dict[str, Any]:
    """Run one contained launch; preflight is deliberately the default integration path."""

    if preflight_only and (run_class is not None or launch_ordinal is not None):
        raise CapabilityError("preflight-only cannot reserve or execute a replay launch")
    try:
        fixture = load_capability_fixture()
        facts, movie = _preflight(fixture, rom_path)
    except PrivateInputUnavailable as error:
        return {
            "Status": "UNAVAILABLE",
            "Mode": "PREFLIGHT" if preflight_only else "REPLAY",
            "CapabilityId": CAPABILITY_ID,
            "Failure": str(error),
            "ProcessStarts": 0,
        }
    except FileNotFoundError as error:
        return {
            "Status": "FAIL",
            "Mode": "PREFLIGHT" if preflight_only else "REPLAY",
            "CapabilityId": CAPABILITY_ID,
            "Failure": str(error),
            "ProcessStarts": 0,
        }
    except (CapabilityError, OSError, ValueError, json.JSONDecodeError) as error:
        return {
            "Status": "FAIL",
            "Mode": "PREFLIGHT" if preflight_only else "REPLAY",
            "CapabilityId": CAPABILITY_ID,
            "Failure": str(error),
            "ProcessStarts": 0,
        }
    if preflight_only:
        return {
            "Status": "PASS",
            "Mode": "PREFLIGHT",
            "CapabilityId": CAPABILITY_ID,
            "RecipeSha256": movie.recipe_sha256,
            "Bk2Sha256": movie.bk2_sha256,
            "ObserverSha256": facts["observerSha256"],
            "CandidateSha256": _candidate_identity(fixture, facts, movie)["candidateSha256"],
            "ProcessStarts": 0,
        }
    if run_class is None or launch_ordinal is None:
        raise CapabilityError("non-preflight replay requires --run-class and --launch-ordinal")
    if timeout_seconds < 1 or timeout_seconds > 120:
        raise CapabilityError("timeout seconds must be in 1..120")
    identity = _candidate_identity(fixture, facts, movie)
    candidate = DERIVED_ROOT / identity["candidateSha256"]
    successful_prior = _validate_launch_sequence(candidate, identity, run_class, launch_ordinal)
    launch = _contained_launch_directory(identity["candidateSha256"], launch_ordinal)
    status_path = launch / "observer-status.json"
    receipt_path = launch / "receipt.json"
    host_root = Path(facts["hostToolchainRoot"])
    try:
        host_before = _inventory_mutable_surfaces(host_root)
        host_toolchain_before = _inventory_host_toolchain_tree(host_root)
        prepared = _prepare_contained_launch(
            fixture, facts, movie, launch, rom_path.resolve(strict=True)
        )
    except (CapabilityError, OSError) as error:
        cleanup = _safe_delete_launch_files(launch, receipt_path)
        if cleanup["residualArtifacts"] or cleanup["firstMismatch"] is not None:
            raise CapabilityError("pre-start contained cleanup failed") from error
        launch.rmdir()
        raise
    executable = prepared["executable"]
    config_path = prepared["config"]
    config_before = {"exists": True, **_file_identity(config_path)}
    toolchain_before = _inventory_mutable_surfaces(prepared["toolchain"])
    environment = os.environ.copy()
    environment.update(
        {
            "SF2_ORIGINAL_REFERENCE_STATUS": str(status_path),
            "SF2_ORIGINAL_REFERENCE_EXPECTED_ROWS": "32",
        }
    )
    command = _launch_command(
        executable,
        config_path,
        prepared["movie"],
        prepared["observer"],
        prepared["rom"],
    )
    process_result: dict[str, Any] = {
        "started": False,
        "exitCode": None,
        "timedOut": False,
        "processTerminated": False,
        "timeoutTreeKilled": False,
        "pid": None,
        "stdout": "",
        "stderr": "",
        "moviePosition": None,
    }
    observer_status: dict[str, Any] | None = None
    failure: dict[str, Any] | None = None
    ledger = _validate_ledger(_load_ledger())
    ledger.append(
        {
            "ordinal": launch_ordinal,
            "candidateSha256": identity["candidateSha256"],
            "runClass": run_class,
            "receiptPath": str(receipt_path),
            "receiptSha256": None,
            "status": "RESERVED",
        }
    )
    _write_ledger(ledger)

    def mark_started(pid: int) -> None:
        rows = _validate_ledger(_load_ledger())
        row = _launch_ledger_row(rows, identity["candidateSha256"], launch_ordinal)
        if row["status"] != "RESERVED":
            raise CapabilityError("launch ledger cannot transition to STARTED")
        row.update({"status": "STARTED", "processStarted": True, "pid": pid})
        _write_ledger(rows)

    try:
        native = run_native_bizhawk_process(
            command=command,
            executable=executable,
            environment=environment,
            timeout_seconds=timeout_seconds,
            on_started=mark_started,
        )
        process_result["started"] = native.started
        if native.timed_out:
            process_result["timedOut"] = True
            failure = {
                "phase": "process",
                "code": "timeout",
                "expected": f"exit within {timeout_seconds}s",
                "actual": "timeout",
            }
        elif native.error is not None:
            failure = {
                "phase": "process",
                "code": "post-start-error",
                "expected": "process communication",
                "actual": native.error,
            }
        process_result.update(
            {
                "exitCode": native.returncode,
                "stdout": _bounded(native.stdout),
                "stderr": _bounded(native.stderr),
                "processTerminated": native.process_terminated,
                "timeoutTreeKilled": native.timeout_tree_killed,
                "pid": native.pid,
            }
        )
        if not native.timed_out:
            observer_status = _load_observer_status(status_path)
            if observer_status is None:
                if failure is None:
                    failure = {
                        "phase": "observer",
                        "code": "status-missing",
                        "expected": "typed observer status record",
                        "actual": {"statusPresent": False},
                    }
            else:
                process_result["moviePosition"] = observer_status.get("moviePosition")
            if failure is not None:
                pass
            elif native.returncode != 0:
                failure = {
                    "phase": "process",
                    "code": "nonzero-exit",
                    "expected": 0,
                    "actual": native.returncode,
                }
            elif observer_status.get("status") != "PASS":
                if native.returncode == 0:
                    failure = {
                        "phase": "process",
                        "code": "observer-failure-zero-exit",
                        "expected": "non-zero exit for observer failure",
                        "actual": {
                            "exitCode": native.returncode,
                            "observerStatus": observer_status.get("status"),
                            "callbacksRemaining": observer_status.get("callbacksRemaining"),
                        },
                    }
                else:
                    failure = {
                        "phase": "observer",
                        "code": "status",
                        "expected": "PASS",
                        "actual": observer_status.get("status"),
                    }
            elif not native.process_terminated:
                failure = {
                    "phase": "process",
                    "code": "residual-process",
                    "expected": True,
                    "actual": False,
                }
            elif observer_status.get("callbacksRemaining") != 0:
                failure = {
                    "phase": "cleanup",
                    "code": "callbacks",
                    "expected": 0,
                    "actual": observer_status.get("callbacksRemaining"),
                }
            elif observer_status.get("moviePosition") != 32:
                failure = {
                    "phase": "movie",
                    "code": "position",
                    "expected": 32,
                    "actual": observer_status.get("moviePosition"),
                }
            elif not all(
                (
                    observer_status.get("movieMode") == "FINISHED",
                    observer_status.get("readOnly") is True,
                    observer_status.get("powerOn") is True,
                    observer_status.get("headerPlatform") == "GEN",
                    observer_status.get("headerCore") == fixture["toolchainContract"]["core"],
                    observer_status.get("statusWriteOk") is True,
                    observer_status.get("initialFrame") == 1,
                    observer_status.get("firstPollFrame") == 1,
                    observer_status.get("terminalFrame") == 33,
                    observer_status.get("clientVersion")
                    == fixture["toolchainContract"]["bizhawkRelease"],
                )
            ):
                failure = {
                    "phase": "observer",
                    "code": "terminal-contract",
                    "expected": "FINISHED/read-only/power-on/semantic-row-1-through-32",
                    "actual": observer_status,
                }
            else:
                trace = observer_status.get("inputPollTrace")
                expected_trace = [
                    {
                        "semanticIndex": index,
                        "bk2Row": index + 1,
                        "emuFrame": index + 1,
                        "input": value,
                    }
                    for index, value in enumerate(_input_rows(fixture["movieRecipe"])[2:])
                ]
                actual_trace = trace if isinstance(trace, list) else ()
                if actual_trace != expected_trace:
                    failure = {
                        "phase": "observer",
                        "code": "input-poll-trace",
                        "expected": list(expected_trace),
                        "actual": trace,
                    }
    except OSError as error:
        failure = {
            "phase": "process",
            "code": "start-failed",
            "expected": "BizHawk process start",
            "actual": str(error),
        }
    except CapabilityError as error:
        failure = {
            "phase": "observer",
            "code": "status-contract",
            "expected": "typed observer status",
            "actual": str(error),
        }

    def record_first_failure(value: dict[str, Any]) -> None:
        nonlocal failure
        if failure is None:
            failure = value

    config_after = _snapshot_file_identity(config_path)
    toolchain_after = _snapshot_inventory(prepared["toolchain"])
    host_after = _snapshot_inventory(host_root)
    host_toolchain_after = _snapshot_host_toolchain_tree(host_root)
    rom_post = _snapshot_file_identity(rom_path)
    archive_post = _snapshot_file_identity(Path(facts["archivePath"]))
    contained_emulator = _snapshot_file_identity(executable)
    contained_lua54 = _snapshot_file_identity(prepared["toolchain"] / "dll" / "lua54.dll")
    for name, snapshot in (
        ("config", config_after),
        ("contained mutable surfaces", toolchain_after),
        ("host mutable surfaces", host_after),
        ("host toolchain tree", host_toolchain_after),
        ("private ROM", rom_post),
        ("pristine archive", archive_post),
        ("contained emulator", contained_emulator),
        ("contained Lua runtime", contained_lua54),
    ):
        if snapshot["status"] == "unavailable":
            record_first_failure(
                {
                    "phase": "isolation",
                    "code": "post-snapshot-unavailable",
                    "expected": f"captured {name} snapshot",
                    "actual": f"{name}: {snapshot['error']}",
                }
            )

    host_toolchain_drift: bool | None = None
    if host_toolchain_after["status"] == "captured":
        host_toolchain_drift = host_toolchain_before != host_toolchain_after["tree"]
        if host_toolchain_drift:
            expected_entry, actual_entry = _first_host_toolchain_difference(
                host_toolchain_before, host_toolchain_after["tree"]
            )
            record_first_failure(
                {
                    "phase": "isolation",
                    "code": "host-toolchain-drift",
                    "expected": expected_entry,
                    "actual": actual_entry,
                }
            )
    host_drift: bool | None = None
    if host_after["status"] == "captured":
        host_drift = host_before != host_after["inventory"]
        if host_drift:
            record_first_failure(
                {
                    "phase": "isolation",
                    "code": "host-drift",
                    "expected": host_before,
                    "actual": host_after["inventory"],
                }
            )
    if rom_post["status"] == "captured" and rom_post["identity"] != {
        "exists": True,
        **facts["romIdentity"],
    }:
        record_first_failure(
            {
                "phase": "isolation",
                "code": "rom-drift",
                "expected": facts["romIdentity"],
                "actual": rom_post["identity"],
            }
        )
    if archive_post["status"] == "captured" and archive_post["identity"] != {
        "exists": True,
        **facts["archive"],
    }:
        record_first_failure(
            {
                "phase": "isolation",
                "code": "archive-drift",
                "expected": facts["archive"],
                "actual": archive_post["identity"],
            }
        )
    if toolchain_after["status"] == "captured" and toolchain_before != toolchain_after["inventory"]:
        record_first_failure(
            {
                "phase": "isolation",
                "code": "contained-drift",
                "expected": toolchain_before,
                "actual": toolchain_after["inventory"],
            }
        )
    for name, snapshot, expected in (
        (
            "contained-emulator-drift",
            contained_emulator,
            {
                "exists": True,
                "sha256": fixture["toolchainContract"]["executableSha256"],
                "sizeBytes": fixture["toolchainContract"]["executableSizeBytes"],
            },
        ),
        (
            "contained-lua54-drift",
            contained_lua54,
            {
                "exists": True,
                "sha256": fixture["toolchainContract"]["lua54Sha256"],
                "sizeBytes": fixture["toolchainContract"]["lua54SizeBytes"],
            },
        ),
    ):
        if snapshot["status"] == "captured" and snapshot["identity"] != expected:
            record_first_failure(
                {
                    "phase": "isolation",
                    "code": name,
                    "expected": expected,
                    "actual": snapshot["identity"],
                }
            )
    try:
        cleanup = _safe_delete_launch_files(launch, receipt_path)
    except (CapabilityError, OSError) as error:
        cleanup = {
            "removedArtifacts": [],
            "residualArtifacts": [],
            "firstMismatch": f"cleanup unavailable: {error}",
        }
    if cleanup["residualArtifacts"] or cleanup["firstMismatch"] is not None:
        record_first_failure(
            {
                "phase": "cleanup",
                "code": "residual-artifact",
                "expected": [],
                "actual": cleanup["residualArtifacts"] or cleanup["firstMismatch"],
            }
        )
    try:
        post_facts, post_movie = _preflight(fixture, rom_path)
        if _candidate_identity(fixture, post_facts, post_movie) != identity:
            record_first_failure(
                {
                    "phase": "determinism",
                    "code": "candidate-drift",
                    "expected": identity,
                    "actual": _candidate_identity(fixture, post_facts, post_movie),
                }
            )
    except (CapabilityError, OSError) as error:
        record_first_failure(
            {
                "phase": "determinism",
                "code": "candidate-drift",
                "expected": identity,
                "actual": str(error),
            }
        )
    status = "PASS" if failure is None else "FAIL"
    receipt = _receipt(
        fixture=fixture,
        run_class=run_class,
        ordinal=launch_ordinal,
        facts=facts,
        movie=movie,
        status=status,
        observer_status=observer_status,
        process=process_result,
        failure=failure,
        cleanup=cleanup,
        config_before=config_before,
        config_after=config_after,
        archive_post=archive_post,
        emulator=contained_emulator,
        lua54=contained_lua54,
        host_before=host_before,
        host_after=host_after,
        host_toolchain_before=host_toolchain_before,
        host_toolchain_after=host_toolchain_after,
        toolchain_before=toolchain_before,
        toolchain_after=toolchain_after,
        rom_post=rom_post,
        host_drift=host_drift,
        host_toolchain_drift=host_toolchain_drift,
        prelaunch_identity=identity,
    )
    if (
        status == "PASS"
        and run_class == "frozen-acceptance"
        and any(
            receipt["determinism"]["replayDigest"] != prior["determinism"]["replayDigest"]
            for prior in successful_prior
        )
    ):
        receipt["execution"]["status"] = "FAIL"
        failure = {
            "phase": "determinism",
            "code": "acceptance-digest",
            "expected": successful_prior[0]["determinism"]["replayDigest"],
            "actual": receipt["determinism"]["replayDigest"],
        }
        receipt["failure"] = failure
        receipt["determinism"]["replayDigest"] = canonical_replay_digest(receipt)
        status = "FAIL"
    _write_receipt(receipt_path, receipt)
    ledger = _validate_ledger(_load_ledger())
    ledger_row = _launch_ledger_row(ledger, identity["candidateSha256"], launch_ordinal)
    if ledger_row["status"] not in {"RESERVED", "STARTED"}:
        raise CapabilityError("launch ledger cannot transition to terminal status")
    ledger_row.update(
        {
            "processStarted": bool(process_result["started"]),
            "receiptSha256": _sha256(receipt_path.read_bytes()),
            "status": receipt["execution"]["status"],
        }
    )
    if ledger_row["processStarted"]:
        if not isinstance(process_result["pid"], int):
            raise CapabilityError("started process lacks terminal numeric pid")
        ledger_row["pid"] = process_result["pid"]
    _validate_ledger(ledger)
    _write_ledger(ledger)
    if status != "PASS":
        raise CapabilityError(
            "original-reference replay failed: " + json.dumps(failure, sort_keys=True)
        )
    return receipt
