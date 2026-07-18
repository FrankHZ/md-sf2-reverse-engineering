from __future__ import annotations

import hashlib
import subprocess
from pathlib import Path
from typing import Any

from sf2tool.jsonio import load_json
from sf2tool.paths import repo_path

DEFAULT_MANIFEST = repo_path("manifests/toolchain.json")


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


def verify_toolchain(
    upstream_path: Path,
    manifest_path: Path = DEFAULT_MANIFEST,
    java_path: Path | None = None,
) -> dict[str, Any]:
    manifest = load_json(manifest_path.resolve(strict=True))
    if manifest["schemaVersion"] != 1:
        raise ValueError(f"unsupported toolchain manifest version: {manifest['schemaVersion']}")
    upstream_path = upstream_path.resolve(strict=True)
    java_path = (java_path or repo_path(manifest["java"]["localJavaPath"])).resolve(strict=True)

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

    for tool in manifest["sf2disasm"]["buildTools"]:
        _verify_file(
            upstream_path / tool["path"],
            size=tool["sizeBytes"],
            sha256=tool["sha256"],
            owner=tool["path"],
        )

    java_output = _run([java_path, "-version"])
    expected_java = manifest["java"]["version"].split("+")[0]
    if expected_java not in java_output:
        raise ValueError(
            f"Java version mismatch: expected {manifest['java']['version']}, got {java_output}"
        )

    bizhawk = manifest["bizhawk"]
    archive = repo_path(bizhawk["localArchivePath"])
    executable = repo_path(bizhawk["localExecutablePath"])
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
