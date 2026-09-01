from __future__ import annotations

import hashlib
import json
import os
import shutil
import struct
import subprocess
import zipfile
import zlib
from dataclasses import dataclass
from pathlib import Path, PurePosixPath
from typing import Any

import pytest
from jsonschema import Draft202012Validator

from sf2tool import remake_asset_build, remake_assets
from sf2tool.remake_godot import ProcessReceipt

HOOK_BYTES = (
    b"#!/bin/sh\n"
    b'echo "Push rejected: md-sf2-remake-assets is a local-only repository." >&2\n'
    b"exit 1\n"
)
TWO_X_BYTES = b"project-authored-test-png-2x"
FOUR_X_BYTES = b"project-authored-test-png-4x"


def _digest(value: bytes) -> str:
    return hashlib.sha256(value).hexdigest().upper()


def _run(root: Path, *arguments: str) -> bytes:
    completed = subprocess.run(
        ["git", *arguments],
        cwd=root,
        check=False,
        capture_output=True,
        shell=False,
        timeout=30,
    )
    if completed.returncode != 0:
        raise AssertionError(f"test Git command failed: {arguments!r}")
    return completed.stdout


def _bucket(scale: int, path: str, width: int, height: int, payload: bytes) -> dict[str, Any]:
    return {
        "scale": scale,
        "runtimePath": path,
        "width": width,
        "height": height,
        "byteLength": len(payload),
        "sha256": _digest(payload),
        "mediaType": "image/png",
        "filter": "nearest",
        "mipmaps": False,
        "repeat": False,
        "colorSpace": "srgb",
        "alphaMode": "straight",
    }


def _manifest() -> dict[str, Any]:
    return {
        "schemaVersion": 1,
        "packageId": remake_assets.PACKAGE_ID,
        "repositoryId": remake_assets.REPOSITORY_ID,
        "profile": remake_assets.PROFILE,
        "capabilities": [remake_assets.PACK_CAPABILITY],
        "logicalPresentation": {"width": 960, "height": 540},
        "assets": [
            {
                "assetId": "ui.project-authored-panel",
                "kind": "raster-image",
                "logicalSize": {"width": 8, "height": 6},
                "source": {
                    "assetId": "source.ui.project-authored-panel",
                    "sha256": _digest(b"project-authored-source"),
                },
                "derivation": {
                    "policyId": "project-authored-test-policy-v1",
                    "generatorId": "project-authored-test-generator",
                    "generatorVersion": "1",
                    "generatorArtifactSha256": _digest(b"project-authored-generator"),
                },
                "buckets": [
                    _bucket(
                        2,
                        "runtime/ui/project-authored-panel@2x.png",
                        16,
                        12,
                        TWO_X_BYTES,
                    ),
                    _bucket(
                        4,
                        "runtime/ui/project-authored-panel@4x.png",
                        32,
                        24,
                        FOUR_X_BYTES,
                    ),
                ],
            }
        ],
    }


@dataclass
class AssetRepository:
    root: Path
    manifest: dict[str, Any]

    @classmethod
    def create(cls, root: Path) -> AssetRepository:
        root.mkdir(parents=True)
        _run(root, "init", "--initial-branch=main")
        _run(root, "config", "user.name", "Project Authored Test")
        _run(root, "config", "user.email", "project-authored@example.invalid")
        _run(root, "config", "core.hooksPath", ".githooks")
        (root / ".githooks").mkdir()
        (root / ".githooks" / "pre-push").write_bytes(HOOK_BYTES)
        (root / ".gitattributes").write_text(
            "* text=auto\n.githooks/* text eol=lf\n*.json text eol=lf\n*.png binary\n",
            encoding="utf-8",
        )
        (root / ".gitignore").write_text("cache/\nscratch/\n", encoding="utf-8")
        (root / "source").mkdir()
        (root / "source" / "project-authored-source.txt").write_text(
            "project-authored source\n",
            encoding="utf-8",
        )
        (root / "masters").mkdir()
        (root / "masters" / "project-authored-panel.svg").write_text(
            '<svg viewBox="0 0 8 6"><rect id="panel" width="8" height="6"/></svg>\n',
            encoding="utf-8",
        )
        repository = cls(root, _manifest())
        repository.write_payloads()
        repository.write_manifest()
        repository.commit("bootstrap project-authored test pack")
        return repository

    @property
    def manifest_path(self) -> Path:
        return self.root / "manifests" / "presentation-assets-v1.json"

    def write_payloads(self) -> None:
        runtime = self.root / "runtime" / "ui"
        runtime.mkdir(parents=True, exist_ok=True)
        (runtime / "project-authored-panel@2x.png").write_bytes(TWO_X_BYTES)
        (runtime / "project-authored-panel@4x.png").write_bytes(FOUR_X_BYTES)

    def write_manifest(self) -> None:
        self.manifest_path.parent.mkdir(parents=True, exist_ok=True)
        self.manifest_path.write_text(
            json.dumps(self.manifest, indent=2, ensure_ascii=True) + "\n",
            encoding="utf-8",
        )

    def commit(self, message: str) -> None:
        _run(self.root, "add", "--all")
        _run(self.root, "commit", "--message", message)

    def pins(self) -> dict[str, str]:
        return {
            "expected_commit": _run(self.root, "rev-parse", "--verify", "HEAD^{commit}")
            .decode("ascii")
            .strip(),
            "expected_tree": _run(self.root, "rev-parse", "HEAD^{tree}").decode("ascii").strip(),
            "expected_manifest_sha256": _digest(self.manifest_path.read_bytes()),
        }


def _repository(tmp_path: Path) -> AssetRepository:
    return AssetRepository.create(tmp_path / "asset-repository")


def _assert_code(error: pytest.ExceptionInfo[remake_assets.AssetPreflightError], code: str) -> None:
    assert error.value.code == code
    assert "asset-repository" not in error.value.message
    assert "project-authored-panel" not in error.value.message


def test_checkout_descriptor_is_closed_path_free_and_self_validating(tmp_path: Path) -> None:
    repository = _repository(tmp_path)
    before = (_run(repository.root, "status", "--porcelain=v2", "-z"), repository.pins())

    descriptor = remake_assets.preflight_asset_checkout(
        str(repository.root),
        **repository.pins(),
    )

    schema = json.loads(remake_assets.MOUNT_SCHEMA.read_text(encoding="utf-8"))
    Draft202012Validator.check_schema(schema)
    Draft202012Validator(schema).validate(descriptor)
    assert descriptor["mountKind"] == "checkout"
    assert descriptor["assetCount"] == 1
    assert descriptor["bucketCount"] == 2
    assert descriptor["bucketScales"] == [2, 4]
    encoded = json.dumps(descriptor, sort_keys=True)
    for forbidden in (
        str(repository.root),
        "main",
        "runtime/",
        "project-authored-panel@2x.png",
        "source/",
        "masters/",
    ):
        assert forbidden not in encoded
    assert _run(repository.root, "status", "--porcelain=v2", "-z") == before[0] == b""
    assert repository.pins() == before[1]


def test_export_is_fresh_minimal_atomic_and_keeps_source_unchanged(
    tmp_path: Path,
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    repository = _repository(tmp_path)
    destination = tmp_path / "exported-pack"
    before = repository.pins()
    observed_descriptor_last = False
    original_rename = os.rename

    def checked_rename(source: str | os.PathLike[str], target: str | os.PathLike[str]) -> None:
        nonlocal observed_descriptor_last
        source_path = Path(source)
        assert (source_path / remake_assets.DESCRIPTOR_FILE_NAME).is_file()
        observed_descriptor_last = True
        original_rename(source, target)

    monkeypatch.setattr(remake_assets.os, "rename", checked_rename)
    descriptor = remake_assets.export_asset_pack(
        str(repository.root),
        str(destination),
        **before,
    )

    assert observed_descriptor_last
    assert descriptor["mountKind"] == "exported-pack"
    exported = {
        path.relative_to(destination).as_posix()
        for path in destination.rglob("*")
        if path.is_file()
    }
    assert exported == {
        "manifests/presentation-assets-v1.json",
        "runtime/ui/project-authored-panel@2x.png",
        "runtime/ui/project-authored-panel@4x.png",
        remake_assets.DESCRIPTOR_FILE_NAME,
    }
    assert not (destination / ".git").exists()
    assert not (destination / "source").exists()
    assert not (destination / "masters").exists()
    assert repository.pins() == before
    assert _run(repository.root, "status", "--porcelain=v2", "-z") == b""


def test_relative_checkout_and_export_paths_are_not_resolved(tmp_path: Path) -> None:
    repository = _repository(tmp_path)
    with pytest.raises(remake_assets.AssetPreflightError) as relative_root:
        remake_assets.preflight_asset_checkout("asset-repository", **repository.pins())
    _assert_code(relative_root, "InvalidRequest")

    with pytest.raises(remake_assets.AssetPreflightError) as relative_destination:
        remake_assets.export_asset_pack(
            str(repository.root),
            "exported-pack",
            **repository.pins(),
        )
    _assert_code(relative_destination, "InvalidRequest")
    assert not (Path.cwd() / "exported-pack").exists()


def test_existing_export_destination_is_never_overwritten(tmp_path: Path) -> None:
    repository = _repository(tmp_path)
    destination = tmp_path / "existing"
    destination.mkdir()
    sentinel = destination / "sentinel.txt"
    sentinel.write_text("keep\n", encoding="utf-8")

    with pytest.raises(remake_assets.AssetPreflightError) as rejected:
        remake_assets.export_asset_pack(
            str(repository.root),
            str(destination),
            **repository.pins(),
        )

    _assert_code(rejected, "ExportDestinationRejected")
    assert sentinel.read_text(encoding="utf-8") == "keep\n"


def test_export_destination_inside_ignored_checkout_is_rejected_without_residue(
    tmp_path: Path,
) -> None:
    repository = _repository(tmp_path)
    ignored_parent = repository.root / "scratch"
    ignored_parent.mkdir()
    destination = ignored_parent / "exported-pack"
    before = repository.pins()

    with pytest.raises(remake_assets.AssetPreflightError) as rejected:
        remake_assets.export_asset_pack(
            str(repository.root),
            str(destination),
            **before,
        )

    _assert_code(rejected, "ExportDestinationRejected")
    assert not destination.exists()
    assert list(ignored_parent.iterdir()) == []
    assert repository.pins() == before
    assert _run(repository.root, "status", "--porcelain=v2", "-z") == b""


@pytest.mark.parametrize("kind", ("staged", "unstaged", "untracked"))
def test_dirty_checkout_rejects_without_leaking_dirty_path(
    tmp_path: Path,
    kind: str,
) -> None:
    repository = _repository(tmp_path)
    secret_name = "private-sensitive-name.txt"
    if kind == "staged":
        (repository.root / secret_name).write_text("private\n", encoding="utf-8")
        _run(repository.root, "add", secret_name)
    elif kind == "unstaged":
        (repository.root / "source" / "project-authored-source.txt").write_text(
            "changed\n", encoding="utf-8"
        )
    else:
        (repository.root / secret_name).write_text("private\n", encoding="utf-8")

    with pytest.raises(remake_assets.AssetPreflightError) as rejected:
        remake_assets.preflight_asset_checkout(str(repository.root), **repository.pins())

    _assert_code(rejected, "RepositoryStateMismatch")
    assert secret_name not in rejected.value.message


@pytest.mark.parametrize("pin", ("expected_commit", "expected_tree", "expected_manifest_sha256"))
def test_exact_caller_pins_fail_closed(tmp_path: Path, pin: str) -> None:
    repository = _repository(tmp_path)
    pins = repository.pins()
    pins[pin] = ("a" * 40) if pin != "expected_manifest_sha256" else ("A" * 64)

    with pytest.raises(remake_assets.AssetPreflightError) as rejected:
        remake_assets.preflight_asset_checkout(str(repository.root), **pins)

    expected = (
        "ManifestDigestMismatch"
        if pin == "expected_manifest_sha256"
        else "RepositoryIdentityMismatch"
    )
    _assert_code(rejected, expected)


def test_remote_submodule_and_hook_policy_drift_fail_closed(tmp_path: Path) -> None:
    remote_repository = _repository(tmp_path / "remote-case")
    _run(remote_repository.root, "remote", "add", "origin", "https://example.invalid/assets")
    with pytest.raises(remake_assets.AssetPreflightError) as remote:
        remake_assets.preflight_asset_checkout(
            str(remote_repository.root), **remote_repository.pins()
        )
    _assert_code(remote, "RemotePolicyMismatch")

    submodule_repository = _repository(tmp_path / "submodule-case")
    head = submodule_repository.pins()["expected_commit"]
    _run(
        submodule_repository.root,
        "update-index",
        "--add",
        "--cacheinfo",
        f"160000,{head},vendor/project-authored-submodule",
    )
    _run(submodule_repository.root, "commit", "--message", "add test gitlink")
    with pytest.raises(remake_assets.AssetPreflightError) as submodule:
        remake_assets.preflight_asset_checkout(
            str(submodule_repository.root), **submodule_repository.pins()
        )
    _assert_code(submodule, "SubmoduleRejected")

    hook_repository = _repository(tmp_path / "hook-case")
    _run(hook_repository.root, "config", "core.hooksPath", ".other-hooks")
    with pytest.raises(remake_assets.AssetPreflightError) as hook:
        remake_assets.preflight_asset_checkout(str(hook_repository.root), **hook_repository.pins())
    _assert_code(hook, "HookPolicyMismatch")


def test_tracked_hook_content_drift_rejects(tmp_path: Path) -> None:
    repository = _repository(tmp_path)
    (repository.root / ".githooks" / "pre-push").write_bytes(b"#!/bin/sh\nexit 0\n")
    repository.commit("mutate test hook")

    with pytest.raises(remake_assets.AssetPreflightError) as rejected:
        remake_assets.preflight_asset_checkout(str(repository.root), **repository.pins())

    _assert_code(rejected, "HookPolicyMismatch")


def test_manifest_digest_precedes_json_and_semantic_validation(tmp_path: Path) -> None:
    repository = _repository(tmp_path)
    repository.manifest_path.write_bytes(b"not-json")
    repository.commit("malformed project-authored test manifest")
    pins = repository.pins()
    pins["expected_manifest_sha256"] = "A" * 64
    with pytest.raises(remake_assets.AssetPreflightError) as digest:
        remake_assets.preflight_asset_checkout(str(repository.root), **pins)
    _assert_code(digest, "ManifestDigestMismatch")

    with pytest.raises(remake_assets.AssetPreflightError) as semantic:
        remake_assets.preflight_asset_checkout(str(repository.root), **repository.pins())
    _assert_code(semantic, "InvalidManifest")


@pytest.mark.parametrize("mutation", ("unknown", "duplicate-asset", "duplicate-runtime"))
def test_closed_manifest_and_duplicate_identities_reject(
    tmp_path: Path,
    mutation: str,
) -> None:
    repository = _repository(tmp_path)
    if mutation == "unknown":
        repository.manifest["unknown"] = True
    else:
        duplicate = json.loads(json.dumps(repository.manifest["assets"][0]))
        if mutation == "duplicate-runtime":
            duplicate["assetId"] = "ui.project-authored-panel-copy"
        repository.manifest["assets"].append(duplicate)
    repository.write_manifest()
    repository.commit("mutate project-authored test manifest")

    with pytest.raises(remake_assets.AssetPreflightError) as rejected:
        remake_assets.preflight_asset_checkout(str(repository.root), **repository.pins())

    assert rejected.value.code in {"InvalidManifest", "DuplicateIdentity"}
    assert str(repository.root) not in rejected.value.message


@pytest.mark.skipif(os.name != "nt", reason="Windows path aliases are case-insensitive")
def test_windows_case_aliases_cannot_name_one_runtime_payload_twice(tmp_path: Path) -> None:
    repository = _repository(tmp_path)
    duplicate = json.loads(json.dumps(repository.manifest["assets"][0]))
    duplicate["assetId"] = "ui.project-authored-panel-copy"
    for bucket in duplicate["buckets"]:
        bucket["runtimePath"] = bucket["runtimePath"].replace(
            "runtime/ui/project-authored-panel",
            "runtime/UI/PROJECT-AUTHORED-PANEL",
        )
    repository.manifest["assets"].append(duplicate)
    repository.write_manifest()
    repository.commit("add case-alias project-authored test paths")

    with pytest.raises(remake_assets.AssetPreflightError) as rejected:
        remake_assets.preflight_asset_checkout(str(repository.root), **repository.pins())

    _assert_code(rejected, "DuplicateIdentity")


def test_payload_drift_and_untracked_manifest_fail_closed(tmp_path: Path) -> None:
    repository = _repository(tmp_path / "payload-case")
    payload = repository.root / "runtime" / "ui" / "project-authored-panel@2x.png"
    payload.write_bytes(b"changed-project-authored-test-payload")
    repository.commit("mutate project-authored runtime payload")
    with pytest.raises(remake_assets.AssetPreflightError) as payload_rejected:
        remake_assets.preflight_asset_checkout(str(repository.root), **repository.pins())
    _assert_code(payload_rejected, "PayloadMismatch")

    manifest_repository = _repository(tmp_path / "manifest-case")
    _run(
        manifest_repository.root,
        "rm",
        "--cached",
        "manifests/presentation-assets-v1.json",
    )
    (manifest_repository.root / ".gitignore").write_text(
        "cache/\nscratch/\nmanifests/presentation-assets-v1.json\n",
        encoding="utf-8",
    )
    manifest_repository.commit("make test manifest ignored and untracked")
    with pytest.raises(remake_assets.AssetPreflightError) as manifest_rejected:
        remake_assets.preflight_asset_checkout(
            str(manifest_repository.root), **manifest_repository.pins()
        )
    _assert_code(manifest_rejected, "RepositoryStateMismatch")


def test_reparse_detection_is_fail_closed_and_path_free(
    tmp_path: Path,
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    repository = _repository(tmp_path)
    real = remake_assets._is_reparse_point
    payload = repository.root / "runtime" / "ui" / "project-authored-panel@2x.png"
    monkeypatch.setattr(
        remake_assets,
        "_is_reparse_point",
        lambda path: path == payload or real(path),
    )

    with pytest.raises(remake_assets.AssetPreflightError) as rejected:
        remake_assets.preflight_asset_checkout(str(repository.root), **repository.pins())

    _assert_code(rejected, "AssetPathRejected")


def test_failed_export_cleans_only_owned_temp(
    tmp_path: Path,
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    repository = _repository(tmp_path)
    destination = tmp_path / "exported-pack"
    unrelated = tmp_path / ".sf2-presentation-export-unrelated.tmp"
    unrelated.mkdir()
    (unrelated / "keep.txt").write_text("keep\n", encoding="utf-8")

    def fail_copy(_source: str | os.PathLike[str], _target: str | os.PathLike[str]) -> None:
        raise OSError("project-authored test failure")

    monkeypatch.setattr(remake_assets.shutil, "copyfile", fail_copy)
    with pytest.raises(remake_assets.AssetPreflightError) as rejected:
        remake_assets.export_asset_pack(
            str(repository.root),
            str(destination),
            **repository.pins(),
        )

    _assert_code(rejected, "ExportFailed")
    assert not destination.exists()
    assert (unrelated / "keep.txt").read_text(encoding="utf-8") == "keep\n"
    owned_residue = [
        path
        for path in tmp_path.iterdir()
        if path.name.startswith(remake_assets._EXPORT_TEMP_PREFIX) and path != unrelated
    ]
    assert owned_residue == []


def test_staging_creation_failure_is_typed_and_path_free(
    tmp_path: Path,
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    repository = _repository(tmp_path)
    destination = tmp_path / "exported-pack"
    original_mkdir = Path.mkdir

    def fail_owned_temp(path: Path, *args: Any, **kwargs: Any) -> None:
        if path.parent == destination.parent and path.name.startswith(
            remake_assets._EXPORT_TEMP_PREFIX
        ):
            raise OSError(f"injected failure at {path}")
        original_mkdir(path, *args, **kwargs)

    monkeypatch.setattr(Path, "mkdir", fail_owned_temp)
    with pytest.raises(remake_assets.AssetPreflightError) as rejected:
        remake_assets.export_asset_pack(
            str(repository.root),
            str(destination),
            **repository.pins(),
        )

    _assert_code(rejected, "ExportStagingFailed")
    assert str(repository.root) not in rejected.value.message
    assert str(destination) not in rejected.value.message
    assert not destination.exists()


def test_cleanup_failure_is_typed_and_cli_output_remains_path_free(
    tmp_path: Path,
    monkeypatch: pytest.MonkeyPatch,
    capsys: pytest.CaptureFixture[str],
) -> None:
    repository = _repository(tmp_path)
    destination = tmp_path / "exported-pack"
    unrelated = tmp_path / ".sf2-presentation-export-unrelated.tmp"
    unrelated.mkdir()
    (unrelated / "keep.txt").write_text("keep\n", encoding="utf-8")
    original_rmtree = shutil.rmtree

    def fail_copy(_source: str | os.PathLike[str], _target: str | os.PathLike[str]) -> None:
        raise OSError(f"injected copy failure at {destination}")

    def fail_cleanup(path: str | os.PathLike[str], *_args: Any, **_kwargs: Any) -> None:
        raise OSError(f"injected cleanup failure at {path}")

    monkeypatch.setattr(remake_assets.shutil, "copyfile", fail_copy)
    monkeypatch.setattr(remake_assets.shutil, "rmtree", fail_cleanup)
    pins = repository.pins()
    exit_code = remake_assets.main(
        [
            "export",
            "--asset-root",
            str(repository.root),
            "--expected-commit",
            pins["expected_commit"],
            "--expected-tree",
            pins["expected_tree"],
            "--expected-manifest-sha256",
            pins["expected_manifest_sha256"],
            "--destination",
            str(destination),
        ]
    )

    captured = capsys.readouterr()
    assert exit_code == 2
    assert captured.out == ""
    diagnostic = json.loads(captured.err)["diagnostic"]
    assert diagnostic["code"] == "ExportCleanupFailed"
    for forbidden in (str(repository.root), str(destination), "project-authored-panel"):
        assert forbidden not in captured.err
    assert (unrelated / "keep.txt").read_text(encoding="utf-8") == "keep\n"
    residue = [
        path
        for path in tmp_path.iterdir()
        if path.name.startswith(remake_assets._EXPORT_TEMP_PREFIX) and path != unrelated
    ]
    assert len(residue) == 1
    original_rmtree(residue[0])


def test_native_runner_uses_argument_array_no_shell_and_has_timeout(
    tmp_path: Path,
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    captured: dict[str, Any] = {}

    def fake_run(arguments: list[str], **kwargs: Any) -> subprocess.CompletedProcess[bytes]:
        captured["arguments"] = arguments
        captured.update(kwargs)
        return subprocess.CompletedProcess(arguments, 0, b"ok", b"")

    monkeypatch.setattr(remake_assets.subprocess, "run", fake_run)
    result = remake_assets._run_native(
        ("git", "status", "--porcelain=v2", "-z"),
        tmp_path,
        7,
    )

    assert result == remake_assets.NativeResult(0, b"ok")
    assert captured["arguments"] == ["git", "status", "--porcelain=v2", "-z"]
    assert captured["shell"] is False
    assert captured["timeout"] == 7


def test_native_timeout_is_typed_and_path_free(
    tmp_path: Path,
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    def timeout(*_arguments: Any, **_kwargs: Any) -> None:
        raise subprocess.TimeoutExpired(["git", "status"], 1)

    monkeypatch.setattr(remake_assets.subprocess, "run", timeout)
    with pytest.raises(remake_assets.AssetPreflightError) as rejected:
        remake_assets._run_native(("git", "status"), tmp_path, 1)

    _assert_code(rejected, "NativeToolTimeout")
    assert str(tmp_path) not in rejected.value.message


def test_cli_rejection_never_prints_local_or_dirty_path(
    tmp_path: Path,
    capsys: pytest.CaptureFixture[str],
) -> None:
    repository = _repository(tmp_path)
    dirty_name = "private-do-not-print.txt"
    (repository.root / dirty_name).write_text("private\n", encoding="utf-8")
    pins = repository.pins()

    exit_code = remake_assets.main(
        [
            "checkout",
            "--asset-root",
            str(repository.root),
            "--expected-commit",
            pins["expected_commit"],
            "--expected-tree",
            pins["expected_tree"],
            "--expected-manifest-sha256",
            pins["expected_manifest_sha256"],
        ]
    )

    captured = capsys.readouterr()
    assert exit_code == 2
    assert captured.out == ""
    assert str(repository.root) not in captured.err
    assert dirty_name not in captured.err
    assert json.loads(captured.err)["diagnostic"]["code"] == "RepositoryStateMismatch"


HUD_MASTER = (
    b'<svg xmlns="http://www.w3.org/2000/svg" id="hud-root" width="8" height="6" '
    b'viewBox="0 0 8 6">'
    b'<rect id="panel" x="0" y="0" width="8" height="6" fill="#123456"/>'
    b"</svg>\n"
)


@dataclass
class CandidateRepository:
    root: Path

    @classmethod
    def create(cls, root: Path, master: bytes = HUD_MASTER) -> CandidateRepository:
        root.mkdir(parents=True)
        _run(root, "init", "--initial-branch=main")
        _run(root, "config", "user.name", "Project Authored Test")
        _run(root, "config", "user.email", "project-authored@example.invalid")
        _run(root, "config", "core.hooksPath", ".githooks")
        (root / ".githooks").mkdir()
        (root / ".githooks" / "pre-push").write_bytes(HOOK_BYTES)
        (root / ".gitattributes").write_text(
            "* text=auto\n.githooks/* text eol=lf\n*.svg text eol=lf\n*.png binary\n",
            encoding="utf-8",
        )
        (root / ".gitignore").write_text("cache/\nscratch/\n", encoding="utf-8")
        (root / "README.md").write_text("# Project Authored Candidate Repo\n", encoding="utf-8")
        for directory in ("manifests", "masters/ui", "runtime", "source"):
            target = root.joinpath(*directory.split("/"))
            target.mkdir(parents=True)
            (target / ".gitkeep").write_text("", encoding="utf-8")
        _run(root, "add", "--all")
        _run(root, "commit", "--message", "bootstrap project-authored candidate repo")
        (root / "masters" / "ui" / "panel.svg").write_bytes(master)
        return cls(root)

    def pins(self) -> tuple[str, str]:
        return (
            _run(self.root, "rev-parse", "--verify", "HEAD^{commit}").decode().strip(),
            _run(self.root, "rev-parse", "HEAD^{tree}").decode().strip(),
        )


def _png(width: int, height: int, marker: bytes = b"") -> bytes:
    def chunk(kind: bytes, payload: bytes) -> bytes:
        checksum = zlib.crc32(kind + payload) & 0xFFFFFFFF
        return struct.pack(">I", len(payload)) + kind + payload + struct.pack(">I", checksum)

    rows = b"".join(b"\0" + bytes([20, 40, 60, 255]) * width for _ in range(height))
    chunks = [
        chunk(b"IHDR", struct.pack(">IIBBBBB", width, height, 8, 6, 0, 0, 0)),
        chunk(b"IDAT", zlib.compress(rows)),
    ]
    if marker:
        chunks.append(chunk(b"tEXt", b"marker\0" + marker))
    chunks.append(chunk(b"IEND", b""))
    return remake_asset_build.PNG_SIGNATURE + b"".join(chunks)


def _test_toolchain(
    root: Path,
    *,
    case_alias_member: bool = False,
) -> tuple[Path, Path]:
    root.mkdir(parents=True, exist_ok=True)
    archive = root / "resvg-win64.zip"
    with zipfile.ZipFile(archive, "w", compression=zipfile.ZIP_DEFLATED) as output:
        output.writestr("resvg.exe", b"project-authored-test-resvg")
        if case_alias_member:
            output.writestr("RESVG.EXE", b"case-alias")
        output.writestr("LICENSE", b"project-authored test license")
    archive_bytes = archive.read_bytes()
    document = json.loads(
        (remake_asset_build.DEFAULT_TOOLCHAIN_MANIFEST).read_text(encoding="utf-8")
    )
    document["archive"]["size"] = len(archive_bytes)
    document["archive"]["sha256"] = _digest(archive_bytes)
    manifest = root / "presentation-toolchain.json"
    manifest.write_text(json.dumps(document, indent=2) + "\n", encoding="utf-8")
    return manifest, archive


def _fake_resvg_runner(mode: str = "pass") -> Any:
    calls: dict[str, int] = {}

    def runner(
        step: str,
        command: tuple[str, ...],
        **_kwargs: Any,
    ) -> ProcessReceipt:
        calls[step] = calls.get(step, 0) + 1
        cleanup = "survivor" if mode == "cleanup" else "clean"
        if step == "version":
            version = "9.9.9\n" if mode == "version" else "0.47.0\n"
            return ProcessReceipt(step, command, 0, False, cleanup, version, "")
        scale = int(step.removeprefix("rasterize-").removesuffix("x"))
        width = 8 * scale
        height = 6 * scale
        if mode == "dimensions":
            width += 1
        marker = b""
        if mode == "nondeterministic" and calls[step] == 2:
            marker = b"different"
        payload = _png(width, height, marker)
        if mode == "crc":
            payload = payload[:-1] + bytes([payload[-1] ^ 1])
        Path(command[-1]).write_bytes(payload)
        return ProcessReceipt(step, command, 0, False, cleanup, "", "")

    return runner


def _build_candidate(
    repository: CandidateRepository,
    archive: Path,
    runner: Any,
) -> dict[str, object]:
    commit, tree = repository.pins()
    return remake_asset_build.build_hud_svg_candidate(
        asset_root=str(repository.root),
        expected_commit=commit,
        expected_tree=tree,
        asset_id="hud.panel",
        expected_master_sha256=_digest(
            (repository.root / "masters" / "ui" / "panel.svg").read_bytes()
        ),
        resvg_archive=str(archive),
        candidate_name="panel-candidate",
        process_runner=runner,
    )


def test_checkout_identity_allows_only_one_exact_untracked_master(tmp_path: Path) -> None:
    repository = CandidateRepository.create(tmp_path / "candidate-repository")
    commit, tree = repository.pins()
    master = PurePosixPath("masters/ui/panel.svg")

    checkout = remake_assets.validate_asset_checkout_identity(
        str(repository.root),
        expected_commit=commit,
        expected_tree=tree,
        allowed_untracked_path=master,
        required_ignored_path=PurePosixPath("cache/panel-candidate"),
    )

    assert checkout.identity == remake_assets.CheckoutIdentity(commit, tree)
    assert checkout.root == repository.root
    with pytest.raises(remake_assets.AssetPreflightError) as default_rejected:
        remake_assets.validate_asset_checkout_identity(
            str(repository.root),
            expected_commit=commit,
            expected_tree=tree,
        )
    _assert_code(default_rejected, "RepositoryStateMismatch")

    (repository.root / "extra-private.txt").write_text("extra\n", encoding="utf-8")
    with pytest.raises(remake_assets.AssetPreflightError) as extra_rejected:
        remake_assets.validate_asset_checkout_identity(
            str(repository.root),
            expected_commit=commit,
            expected_tree=tree,
            allowed_untracked_path=master,
        )
    _assert_code(extra_rejected, "RepositoryStateMismatch")


@pytest.mark.parametrize("mutation", ("staged", "tracked-modified", "ignored", "renamed"))
def test_allowed_untracked_master_rejects_every_other_porcelain_shape(
    tmp_path: Path,
    mutation: str,
) -> None:
    repository = CandidateRepository.create(tmp_path / mutation)
    commit, tree = repository.pins()
    master = repository.root / "masters" / "ui" / "panel.svg"
    if mutation == "staged":
        _run(repository.root, "add", "--", "masters/ui/panel.svg")
    elif mutation == "tracked-modified":
        (repository.root / "README.md").write_text("changed\n", encoding="utf-8")
    elif mutation == "ignored":
        (repository.root / ".gitignore").write_text(
            "cache/\nscratch/\nmasters/ui/panel.svg\n", encoding="utf-8"
        )
        master.unlink()
        _run(repository.root, "add", "--", ".gitignore")
        _run(repository.root, "commit", "--message", "ignore candidate master")
        commit, tree = repository.pins()
        master.write_bytes(HUD_MASTER)
    else:
        _run(repository.root, "add", "--", "masters/ui/panel.svg")
        _run(repository.root, "mv", "masters/ui/panel.svg", "masters/ui/panel-copy.svg")

    with pytest.raises(remake_assets.AssetPreflightError) as rejected:
        remake_assets.validate_asset_checkout_identity(
            str(repository.root),
            expected_commit=commit,
            expected_tree=tree,
            allowed_untracked_path=PurePosixPath("masters/ui/panel.svg"),
        )
    _assert_code(rejected, "RepositoryStateMismatch")


def test_hud_svg_candidate_build_is_deterministic_ignored_and_path_free(
    tmp_path: Path,
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    repository = CandidateRepository.create(tmp_path / "candidate-repository")
    toolchain, archive = _test_toolchain(tmp_path)
    monkeypatch.setattr(remake_asset_build, "DEFAULT_TOOLCHAIN_MANIFEST", toolchain)
    before = (*repository.pins(), _run(repository.root, "status", "--porcelain=v2", "-z"))

    receipt = _build_candidate(repository, archive, _fake_resvg_runner())

    candidate = repository.root / "cache" / "panel-candidate"
    assert sorted(
        path.relative_to(candidate).as_posix() for path in candidate.rglob("*") if path.is_file()
    ) == [
        "manifests/presentation-assets-v1.json",
        "runtime/ui/panel@2x.png",
        "runtime/ui/panel@4x.png",
    ]
    manifest = json.loads(
        (candidate / "manifests" / "presentation-assets-v1.json").read_text(encoding="utf-8")
    )
    assert manifest["assets"][0]["assetId"] == "hud.panel"
    assert [bucket["scale"] for bucket in manifest["assets"][0]["buckets"]] == [2, 4]
    assert receipt["status"] == "Pass"
    assert receipt["cleanupStatus"] == "clean"
    encoded = json.dumps(receipt, sort_keys=True)
    for forbidden in (
        str(repository.root),
        str(archive),
        "masters/",
        "runtime/",
        "panel.svg",
        "candidate.png",
    ):
        assert forbidden not in encoded
    assert repository.pins() == before[:2]
    assert _run(repository.root, "status", "--porcelain=v2", "-z") == before[2]
    assert not list((repository.root / "cache").glob(".sf2-hud-svg-build-*"))


@pytest.mark.parametrize(
    ("replacement", "code"),
    (
        (
            b'<svg xmlns="http://www.w3.org/2000/svg" id="root" width="8" height="6" '
            b'viewBox="0 0 8 6"><text id="text">NO</text></svg>',
            "InvalidSvg",
        ),
        (
            b'<svg xmlns="http://www.w3.org/2000/svg" id="root" width="8" height="6" '
            b'viewBox="0 0 8 6"><image id="image" '
            b'href="data:image/png;base64,AA"/></svg>',
            "InvalidSvg",
        ),
        (
            b'<!DOCTYPE svg><svg xmlns="http://www.w3.org/2000/svg" id="root" '
            b'width="8" height="6" viewBox="0 0 8 6"/>',
            "InvalidSvg",
        ),
        (
            b'<svg xmlns="http://www.w3.org/2000/svg" id="root" width="8" '
            b'height="6" viewBox="0 0 9 6"/>',
            "InvalidSvg",
        ),
        (
            b'<svg xmlns="http://www.w3.org/2000/svg" id="same" width="8" height="6" '
            b'viewBox="0 0 8 6"><rect id="same" width="8" height="6"/></svg>',
            "InvalidSvg",
        ),
        (
            b'<svg xmlns="http://www.w3.org/2000/svg" id="root" width="8" height="6" '
            b'viewBox="0 0 8 6"><rect id="outer" width="8" height="6" fill="#123456">'
            b'<rect id="inner" width="4" height="3" fill="#654321"/></rect></svg>',
            "InvalidSvg",
        ),
    ),
)
def test_hud_svg_closed_subset_rejects_unsafe_or_ambiguous_masters(
    tmp_path: Path,
    monkeypatch: pytest.MonkeyPatch,
    replacement: bytes,
    code: str,
) -> None:
    repository = CandidateRepository.create(tmp_path / "candidate-repository", replacement)
    toolchain, archive = _test_toolchain(tmp_path)
    monkeypatch.setattr(remake_asset_build, "DEFAULT_TOOLCHAIN_MANIFEST", toolchain)

    with pytest.raises(remake_asset_build.AssetBuildError) as rejected:
        _build_candidate(repository, archive, _fake_resvg_runner())

    assert rejected.value.code == code
    assert not (repository.root / "cache" / "panel-candidate").exists()


@pytest.mark.parametrize(
    "leaf",
    (
        b'<rect id="panel" width="8" height="6"/>',
        b'<rect id="panel" width="8" height="6" fill="none" stroke="none"/>',
        b'<line id="panel" x1="0" y1="0" x2="8" y2="6" fill="#123456"/>',
    ),
)
def test_hud_svg_rejects_leaf_without_effective_visible_paint(
    tmp_path: Path,
    monkeypatch: pytest.MonkeyPatch,
    leaf: bytes,
) -> None:
    master = (
        b'<svg xmlns="http://www.w3.org/2000/svg" id="root" width="8" height="6" '
        b'viewBox="0 0 8 6">' + leaf + b"</svg>"
    )
    repository = CandidateRepository.create(tmp_path / "candidate-repository", master)
    toolchain, archive = _test_toolchain(tmp_path)
    monkeypatch.setattr(remake_asset_build, "DEFAULT_TOOLCHAIN_MANIFEST", toolchain)

    with pytest.raises(remake_asset_build.AssetBuildError) as rejected:
        _build_candidate(repository, archive, _fake_resvg_runner())

    assert rejected.value.code == "InvalidSvg"
    assert not (repository.root / "cache" / "panel-candidate").exists()


def test_hud_svg_accepts_explicit_paint_inherited_from_group(
    tmp_path: Path,
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    master = (
        b'<svg xmlns="http://www.w3.org/2000/svg" id="root" width="8" height="6" '
        b'viewBox="0 0 8 6"><g id="chrome" fill="#123456">'
        b'<rect id="panel" width="8" height="6"/></g></svg>'
    )
    repository = CandidateRepository.create(tmp_path / "candidate-repository", master)
    toolchain, archive = _test_toolchain(tmp_path)
    monkeypatch.setattr(remake_asset_build, "DEFAULT_TOOLCHAIN_MANIFEST", toolchain)

    receipt = _build_candidate(repository, archive, _fake_resvg_runner())

    assert receipt["status"] == "Pass"
    assert (repository.root / "cache" / "panel-candidate").is_dir()


@pytest.mark.parametrize(
    ("mode", "code"),
    (
        ("version", "ToolchainVersionMismatch"),
        ("cleanup", "GeneratorCleanupFailed"),
        ("dimensions", "GeneratorOutputInvalid"),
        ("crc", "GeneratorOutputInvalid"),
        ("nondeterministic", "NonDeterministicOutput"),
    ),
)
def test_generator_process_and_png_failures_are_typed_and_atomic(
    tmp_path: Path,
    monkeypatch: pytest.MonkeyPatch,
    mode: str,
    code: str,
) -> None:
    repository = CandidateRepository.create(tmp_path / "candidate-repository")
    toolchain, archive = _test_toolchain(tmp_path)
    monkeypatch.setattr(remake_asset_build, "DEFAULT_TOOLCHAIN_MANIFEST", toolchain)
    unrelated = repository.root / "cache" / "unrelated"
    unrelated.mkdir(parents=True)
    (unrelated / "keep.txt").write_text("keep\n", encoding="utf-8")

    with pytest.raises(remake_asset_build.AssetBuildError) as rejected:
        _build_candidate(repository, archive, _fake_resvg_runner(mode))

    assert rejected.value.code == code
    assert str(repository.root) not in rejected.value.message
    assert str(archive) not in rejected.value.message
    assert not (repository.root / "cache" / "panel-candidate").exists()
    assert (unrelated / "keep.txt").read_text(encoding="utf-8") == "keep\n"
    assert not list((repository.root / "cache").glob(".sf2-hud-svg-build-*"))


def test_archive_digest_precedes_zip_parse_and_case_alias_members_reject(
    tmp_path: Path,
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    repository = CandidateRepository.create(tmp_path / "digest-repository")
    toolchain, archive = _test_toolchain(tmp_path / "digest-tool")
    monkeypatch.setattr(remake_asset_build, "DEFAULT_TOOLCHAIN_MANIFEST", toolchain)
    archive.write_bytes(b"not a zip and no longer the pinned digest")

    with pytest.raises(remake_asset_build.AssetBuildError) as digest_rejected:
        _build_candidate(repository, archive, _fake_resvg_runner())
    assert digest_rejected.value.code == "ToolchainDigestMismatch"

    alias_root = tmp_path / "alias-tool"
    alias_root.mkdir()
    alias_toolchain, alias_archive = _test_toolchain(alias_root, case_alias_member=True)
    alias_repository = CandidateRepository.create(tmp_path / "alias-repository")
    monkeypatch.setattr(remake_asset_build, "DEFAULT_TOOLCHAIN_MANIFEST", alias_toolchain)
    with pytest.raises(remake_asset_build.AssetBuildError) as alias_rejected:
        _build_candidate(alias_repository, alias_archive, _fake_resvg_runner())
    assert alias_rejected.value.code == "InvalidToolchain"


def test_post_promotion_failure_rolls_back_candidate(
    tmp_path: Path,
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    repository = CandidateRepository.create(tmp_path / "candidate-repository")
    toolchain, archive = _test_toolchain(tmp_path)
    monkeypatch.setattr(remake_asset_build, "DEFAULT_TOOLCHAIN_MANIFEST", toolchain)
    original_rmdir = Path.rmdir

    def fail_staging_rmdir(path: Path) -> None:
        if path.name.startswith(remake_asset_build._STAGING_PREFIX):
            raise OSError(f"injected staging failure at {path}")
        original_rmdir(path)

    monkeypatch.setattr(Path, "rmdir", fail_staging_rmdir)
    with pytest.raises(remake_asset_build.AssetBuildError) as rejected:
        _build_candidate(repository, archive, _fake_resvg_runner())

    assert rejected.value.code == "CandidateWriteFailed"
    assert str(repository.root) not in rejected.value.message
    assert not (repository.root / "cache" / "panel-candidate").exists()
    assert not list((repository.root / "cache").glob(".sf2-hud-svg-build-*"))


def test_candidate_cleanup_failure_is_typed_path_free_and_exact_owned(
    tmp_path: Path,
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    repository = CandidateRepository.create(tmp_path / "candidate-repository")
    toolchain, archive = _test_toolchain(tmp_path)
    monkeypatch.setattr(remake_asset_build, "DEFAULT_TOOLCHAIN_MANIFEST", toolchain)
    unrelated = repository.root / "cache" / "unrelated"
    unrelated.mkdir(parents=True)
    (unrelated / "keep.txt").write_text("keep\n", encoding="utf-8")
    original_rmtree = shutil.rmtree

    def fail_owned_cleanup(path: str | os.PathLike[str], *_args: Any, **_kwargs: Any) -> None:
        candidate = Path(path)
        if candidate.name.startswith(remake_asset_build._STAGING_PREFIX):
            raise OSError(f"injected cleanup failure at {candidate}")
        original_rmtree(path)

    monkeypatch.setattr(remake_asset_build.shutil, "rmtree", fail_owned_cleanup)
    with pytest.raises(remake_asset_build.AssetBuildError) as rejected:
        _build_candidate(repository, archive, _fake_resvg_runner("version"))

    assert rejected.value.code == "CleanupFailed"
    assert str(repository.root) not in rejected.value.message
    assert (unrelated / "keep.txt").read_text(encoding="utf-8") == "keep\n"
    residue = list((repository.root / "cache").glob(".sf2-hud-svg-build-*"))
    assert len(residue) == 1
    original_rmtree(residue[0])


def test_generator_launch_failure_and_cli_diagnostic_are_path_free(
    tmp_path: Path,
    monkeypatch: pytest.MonkeyPatch,
    capsys: pytest.CaptureFixture[str],
) -> None:
    repository = CandidateRepository.create(tmp_path / "candidate-repository")
    toolchain, archive = _test_toolchain(tmp_path)
    monkeypatch.setattr(remake_asset_build, "DEFAULT_TOOLCHAIN_MANIFEST", toolchain)

    def fail_launch(*_args: Any, **_kwargs: Any) -> ProcessReceipt:
        raise OSError(f"injected launch failure at {repository.root}")

    commit, tree = repository.pins()
    exit_code = remake_asset_build.main(
        [
            "hud-svg-candidate",
            "--asset-root",
            str(repository.root),
            "--expected-commit",
            commit,
            "--expected-tree",
            tree,
            "--asset-id",
            "hud.panel",
            "--expected-master-sha256",
            _digest(HUD_MASTER),
            "--resvg-archive",
            str(archive),
            "--candidate-name",
            "panel-candidate",
        ]
    )
    captured = capsys.readouterr()
    # The CLI uses the production runner; exercise the injectable launch mapping separately.
    with pytest.raises(remake_asset_build.AssetBuildError) as rejected:
        _build_candidate(repository, archive, fail_launch)
    assert rejected.value.code == "GeneratorLaunchFailed"
    assert str(repository.root) not in rejected.value.message
    assert exit_code == 2
    assert captured.out == ""
    assert str(repository.root) not in captured.err
    assert str(archive) not in captured.err
    assert json.loads(captured.err)["diagnostic"]["code"] in {
        "GeneratorLaunchFailed",
        "GeneratorFailed",
    }


def test_official_resvg_candidate_build_opt_in(tmp_path: Path) -> None:
    archive_value = os.environ.get("SF2_RESVG_ARCHIVE")
    if not archive_value:
        pytest.skip("set SF2_RESVG_ARCHIVE to the pinned ignored resvg-win64.zip")
    repository = CandidateRepository.create(tmp_path / "official-candidate-repository")

    receipt = _build_candidate(
        repository,
        Path(archive_value),
        remake_asset_build.run_bounded_process,
    )

    assert receipt["status"] == "Pass"
    assert receipt["generatorVersion"] == "0.47.0"
    assert receipt["cleanupStatus"] == "clean"
    assert not list((repository.root / "cache").glob(".sf2-hud-svg-build-*"))
