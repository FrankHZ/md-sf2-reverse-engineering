from __future__ import annotations

import hashlib
import os
import shutil
import stat
import subprocess
import zipfile
from pathlib import Path, PureWindowsPath
from typing import Any

from sf2tool.jsonio import load_json
from sf2tool.paths import repo_path
from sf2tool.private_inputs import (
    BIZHAWK_ARCHIVE_INPUT_IDENTITY,
    BIZHAWK_INPUT_IDENTITY,
    H1_INPUT_IDENTITY,
    JDK_INPUT_IDENTITY,
    private_input_path,
)

DEFAULT_MANIFEST = repo_path("manifests/toolchain.json")
JDK_TREE_DIGEST_ALGORITHM = "posix-relative-ordinal-path-tab-size-tab-uppercase-sha256-lf-v1"
_WINDOWS_REPARSE_POINT = getattr(stat, "FILE_ATTRIBUTE_REPARSE_POINT", 0x400)


def _sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for block in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest().upper()


def _run(arguments: list[str | Path], *, cwd: Path | None = None) -> str:
    completed = subprocess.run(
        [str(argument) for argument in arguments],
        cwd=cwd,
        env=local_tool_environment(),
        check=True,
        capture_output=True,
        text=True,
        encoding="utf-8",
        errors="replace",
    )
    return (completed.stdout + completed.stderr).strip()


def _verify_file(path: Path, *, size: int, sha256: str, owner: str) -> None:
    path = path.resolve(strict=True)
    actual_size = path.stat().st_size
    actual_hash = _sha256(path)
    if actual_size != size or actual_hash != sha256:
        raise ValueError(
            f"{owner} provenance mismatch: expected {size}/{sha256}, "
            f"got {actual_size}/{actual_hash}"
        )


def _is_reparse(path: Path) -> bool:
    details = path.lstat()
    return path.is_symlink() or bool(
        getattr(details, "st_file_attributes", 0) & _WINDOWS_REPARSE_POINT
    )


def _tree_identity(root: Path) -> dict[str, int | str]:
    root = root.resolve(strict=True)
    if not root.is_dir():
        raise ValueError("JDK input root is not a directory")

    pending = [root]
    files: list[Path] = []
    while pending:
        directory = pending.pop()
        for child in directory.iterdir():
            if _is_reparse(child):
                raise ValueError("JDK input tree contains a reparse entry")
            if child.is_dir():
                pending.append(child)
            elif child.is_file():
                files.append(child)
            else:
                raise ValueError("JDK input tree contains a non-file entry")

    records = sorted(
        (
            (
                path.relative_to(root).as_posix(),
                path.stat().st_size,
                _sha256(path),
            )
            for path in files
        ),
        key=lambda record: record[0],
    )
    canonical = "\n".join(f"{relative}\t{size}\t{sha256}" for relative, size, sha256 in records)
    return {
        "fileCount": len(records),
        "sizeBytes": sum(size for _, size, _ in records),
        "sha256": hashlib.sha256(canonical.encode("utf-8")).hexdigest().upper(),
    }


def _relative_manifest_path(value: str, *, owner: str) -> Path:
    if not value or not value.strip():
        raise ValueError(f"{owner} must not be empty")
    windows = PureWindowsPath(value)
    if windows.is_absolute() or windows.drive or windows.root:
        raise ValueError(f"{owner} must be relative")
    segments = value.replace("\\", "/").split("/")
    if any(segment in {"", ".", ".."} for segment in segments):
        raise ValueError(f"{owner} contains an unsafe segment")
    return Path(*segments)


def _verify_jdk_directory(root: Path, contract: dict[str, Any]) -> None:
    if contract["extractedTreeDigestAlgorithm"] != JDK_TREE_DIGEST_ALGORITHM:
        raise ValueError("unsupported JDK tree digest algorithm")
    actual = _tree_identity(root)
    expected = {
        "fileCount": contract["extractedFileCount"],
        "sizeBytes": contract["extractedSizeBytes"],
        "sha256": contract["extractedTreeSha256"],
    }
    if actual != expected:
        raise ValueError(f"JDK tree provenance mismatch: expected {expected}, got {actual}")


def _resolve_java_path(manifest: dict[str, Any], java_path: Path | None) -> Path:
    contract = manifest["java"]
    if contract["sharedInputIdentity"] != JDK_INPUT_IDENTITY.as_posix():
        raise ValueError("JDK shared input identity does not match the registered identity")
    relative_java = _relative_manifest_path(
        contract["sharedJavaRelativePath"], owner="sharedJavaRelativePath"
    )
    jdk_root = private_input_path(JDK_INPUT_IDENTITY)
    _verify_jdk_directory(jdk_root, contract)
    resolved_root = jdk_root.resolve(strict=True)
    resolved_java = (resolved_root / relative_java).resolve(strict=True)
    if java_path is not None and java_path.resolve(strict=True) != resolved_java:
        raise ValueError("JavaPath must select the registered shared Java executable")
    if not resolved_java.is_relative_to(resolved_root):
        raise ValueError("shared Java executable resolves outside the JDK input root")
    _verify_file(
        resolved_java,
        size=contract["javaExecutableSizeBytes"],
        sha256=contract["javaExecutableSha256"],
        owner="Java executable",
    )
    return resolved_java


def _resolve_bizhawk_archive(contract: dict[str, Any]) -> Path:
    if contract["sharedArchiveInputIdentity"] != BIZHAWK_ARCHIVE_INPUT_IDENTITY.as_posix():
        raise ValueError("BizHawk shared archive identity does not match the registered identity")
    return private_input_path(BIZHAWK_ARCHIVE_INPUT_IDENTITY)


def shared_bizhawk(contract: dict[str, Any]) -> Path:
    """Verify the executable and Lua before any maintained consumer uses them."""
    root = private_input_path(BIZHAWK_INPUT_IDENTITY)
    for relative, prefix in (("EmuHawk.exe", "executable"), ("dll/lua54.dll", "lua54")):
        path = (root / relative).resolve(strict=True)
        if not path.is_relative_to(root):
            raise ValueError("BizHawk release member escapes the installation")
        _verify_file(
            path,
            size=contract[f"{prefix}SizeBytes"],
            sha256=contract[f"{prefix}Sha256"],
            owner=relative,
        )
    return root / "EmuHawk.exe"


def verify_bizhawk_installation(contract: dict[str, Any]) -> tuple[Path, list[str]]:
    """Compare release members directly with the pinned archive; ignore old user state."""
    archive = _resolve_bizhawk_archive(contract)
    _verify_file(
        archive,
        size=contract["archiveSizeBytes"],
        sha256=contract["archiveSha256"],
        owner="BizHawk archive",
    )
    root = shared_bizhawk(contract).parent
    members = []
    with zipfile.ZipFile(archive) as bundle:
        for entry in bundle.infolist():
            relative = _relative_manifest_path(entry.filename.rstrip("/"), owner="archive member")
            if entry.is_dir():
                continue
            target = (root / relative).resolve(strict=True)
            if not target.is_relative_to(root) or _is_reparse(root / relative):
                raise ValueError("BizHawk release member escapes the installation")
            if target.read_bytes() != bundle.read(entry):
                raise ValueError(f"BizHawk release member mismatch: {entry.filename}")
            members.append(relative.as_posix())
    return root, members


def h1_tool_paths(manifest: dict[str, Any]) -> dict[str, str]:
    root = private_input_path(H1_INPUT_IDENTITY)
    result = {}
    for tool in manifest["sf2disasm"]["buildTools"]:
        relative = _relative_manifest_path(tool["path"], owner="H1 tool")
        path = (root / relative.relative_to("tools")).resolve(strict=True)
        if not path.is_relative_to(root):
            raise ValueError("H1 tool escapes the shared installation")
        _verify_file(path, size=tool["sizeBytes"], sha256=tool["sha256"], owner=tool["path"])
        result[tool["path"]] = str(path)
    # asw/p2bin load adjacent message catalogs even for ordinary successful builds.
    # An EXE-only installation passes binary identity checks but cannot start asw.
    for support in manifest["sf2disasm"]["buildSupportFiles"]:
        relative = _relative_manifest_path(support, owner="H1 support file")
        path = (root / relative.relative_to("tools")).resolve()
        if not path.is_relative_to(root):
            raise ValueError("H1 support file escapes the shared installation")
        if not path.is_file() or path.stat().st_size == 0:
            raise ValueError(f"H1 installation requires a nonempty support file: {support}")
    return result


def local_tool_environment() -> dict[str, str]:
    temp = repo_path("local/tmp/toolchain")
    temp.mkdir(parents=True, exist_ok=True)
    return {**os.environ, "TEMP": str(temp), "TMP": str(temp)}


def require_local_upstream(path: Path) -> Path:
    resolved = path.resolve()
    local = repo_path("local")
    if not resolved.is_relative_to(local) or resolved == local:
        raise ValueError("writable SF2DISASM checkout must be beneath this worktree's local/")
    return resolved


def initialize_research(
    rom_path: Path, manifest_path: Path = DEFAULT_MANIFEST, *, skip_defender_scan: bool = False
) -> dict[str, Any]:
    """Reuse shared installations; initialize only the owning writable source checkout."""
    from sf2tool.rom import verify_rom

    manifest = load_json(manifest_path.resolve(strict=True))
    _resolve_java_path(manifest, None)
    h1_tool_paths(manifest)
    verify_bizhawk_installation(manifest["bizhawk"])
    verify_rom(rom_path)
    upstream = require_local_upstream(repo_path(manifest["sf2disasm"]["localPath"]))
    if not (upstream / ".git").exists():
        if upstream.exists() and any(upstream.iterdir()):
            raise ValueError("refusing to initialize a non-empty SF2DISASM checkout")
        upstream.mkdir(parents=True, exist_ok=True)
        _run(["git", "init"], cwd=upstream)
        _run(["git", "remote", "add", "origin", manifest["sf2disasm"]["repository"]], cwd=upstream)
        _run(
            ["git", "fetch", "--depth", "1", "origin", manifest["sf2disasm"]["commit"]],
            cwd=upstream,
        )
        _run(["git", "checkout", "--detach", "FETCH_HEAD"], cwd=upstream)
    if not skip_defender_scan and os.name == "nt":
        scanner = shutil.which("pwsh")
        if not scanner:
            raise ValueError(
                "Defender scan requires PowerShell; use --skip-defender-scan explicitly"
            )
        environment = local_tool_environment()
        environment["SF2_SCAN_ROOT"] = str(private_input_path(H1_INPUT_IDENTITY))
        subprocess.run(
            [
                scanner,
                "-NoProfile",
                "-NonInteractive",
                "-Command",
                "$ErrorActionPreference='Stop'; "
                "if (Get-Command Start-MpScan -ErrorAction SilentlyContinue) { "
                "Start-MpScan -ScanType CustomScan -ScanPath $env:SF2_SCAN_ROOT }",
            ],
            check=True,
            env=environment,
            cwd=repo_path("."),
        )
    return verify_toolchain(upstream, manifest_path)


def verify_toolchain(
    upstream_path: Path,
    manifest_path: Path = DEFAULT_MANIFEST,
    java_path: Path | None = None,
) -> dict[str, Any]:
    manifest = load_json(manifest_path.resolve(strict=True))
    if manifest["schemaVersion"] != 1:
        raise ValueError(f"unsupported toolchain manifest version: {manifest['schemaVersion']}")
    upstream_path = require_local_upstream(upstream_path).resolve(strict=True)
    java_path = _resolve_java_path(manifest, java_path)

    remote = _run(["git", "remote", "get-url", "origin"], cwd=upstream_path)
    commit = _run(["git", "rev-parse", "HEAD"], cwd=upstream_path)
    tracked_changes = _run(
        ["git", "status", "--porcelain", "--untracked-files=no"], cwd=upstream_path
    )
    if remote != manifest["sf2disasm"]["repository"]:
        raise ValueError(
            f"SF2DISASM remote mismatch: expected {manifest['sf2disasm']['repository']!r}, "
            f"got {remote!r}"
        )
    if commit != manifest["sf2disasm"]["commit"]:
        raise ValueError(
            f"SF2DISASM commit mismatch: expected {manifest['sf2disasm']['commit']}, got {commit}"
        )
    if tracked_changes:
        raise ValueError(f"SF2DISASM has tracked local changes:\n{tracked_changes}")

    h1_tool_paths(manifest)

    java_output = _run([java_path, "-version"])
    expected_java = manifest["java"]["version"].split("+")[0]
    if expected_java not in java_output:
        raise ValueError(
            f"Java version mismatch: expected {manifest['java']['version']}, got {java_output}"
        )

    bizhawk = manifest["bizhawk"]
    archive = _resolve_bizhawk_archive(bizhawk)
    executable = shared_bizhawk(bizhawk)
    _verify_file(
        archive,
        size=bizhawk["archiveSizeBytes"],
        sha256=bizhawk["archiveSha256"],
        owner="BizHawk archive",
    )
    _verify_file(
        executable,
        size=bizhawk["executableSizeBytes"],
        sha256=bizhawk["executableSha256"],
        owner="BizHawk executable",
    )
    return {
        "UpstreamPath": str(upstream_path),
        "UpstreamCommit": commit,
        "BuildToolsVerified": len(manifest["sf2disasm"]["buildTools"]),
        "JavaPath": str(java_path),
        "JavaVersion": manifest["java"]["version"],
        "BizHawkPath": str(executable),
        "BizHawkVersion": bizhawk["release"],
        "Status": "PASS",
    }
