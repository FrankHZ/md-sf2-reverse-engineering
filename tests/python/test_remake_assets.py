from __future__ import annotations

import hashlib
import json
import os
import shutil
import subprocess
from dataclasses import dataclass
from pathlib import Path
from typing import Any

import pytest
from jsonschema import Draft202012Validator

from sf2tool import remake_assets

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
