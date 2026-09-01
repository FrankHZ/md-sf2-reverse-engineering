"""Local-only checkout and export preflight for presentation asset packs."""

from __future__ import annotations

import argparse
import hashlib
import json
import os
import shutil
import stat
import subprocess
import sys
import uuid
from collections.abc import Callable, Mapping, Sequence
from dataclasses import dataclass
from pathlib import Path, PurePosixPath

from jsonschema import Draft202012Validator
from jsonschema.exceptions import SchemaError, ValidationError

from sf2tool.paths import repo_path

PACK_SCHEMA = repo_path("remake/schemas/local-presentation-asset-pack-v1.schema.json")
MOUNT_SCHEMA = repo_path("remake/schemas/local-presentation-asset-mount-v1.schema.json")
MANIFEST_RELATIVE_PATH = PurePosixPath("manifests/presentation-assets-v1.json")
DESCRIPTOR_FILE_NAME = "presentation-asset-mount-v1.json"
DESCRIPTOR_ID = "sf2-local-presentation-asset-mount-v1"
PACKAGE_ID = "sf2-local-presentation-asset-pack-v1"
REPOSITORY_ID = "md-sf2-remake-assets"
PROFILE = "private-local"
PACK_CAPABILITY = "private-local-presentation-asset-pack-admission-v1"
PREFLIGHT_CAPABILITY = "private-local-presentation-asset-checkout-export-preflight-v1"
EXPECTED_HOOKS_PATH = ".githooks"
EXPECTED_PRE_PUSH_HOOK_SHA256 = "F0227073B9E24139FF4C12212F9C2C2B3722909E432ADA494F73166C6C222D7D"
MAXIMUM_MANIFEST_BYTES = 4 * 1024 * 1024
MAXIMUM_PAYLOAD_BYTES = 256 * 1024 * 1024
GIT_TIMEOUT_SECONDS = 30
_EXPORT_TEMP_PREFIX = ".sf2-presentation-export-"


class AssetPreflightError(ValueError):
    """A typed, path-free preflight rejection."""

    def __init__(self, code: str, field: str, message: str) -> None:
        super().__init__(message)
        self.code = code
        self.field = field
        self.message = message

    def as_dict(self) -> dict[str, str]:
        return {"code": self.code, "field": self.field, "message": self.message}


@dataclass(frozen=True)
class NativeResult:
    exit_code: int
    stdout: bytes


NativeRunner = Callable[[Sequence[str], Path, int], NativeResult]


@dataclass(frozen=True)
class CheckoutIdentity:
    commit: str
    tree: str


@dataclass(frozen=True)
class ValidatedAssetCheckout:
    root: Path
    identity: CheckoutIdentity


@dataclass(frozen=True)
class RuntimePayload:
    relative_path: PurePosixPath
    source_path: Path
    byte_length: int
    sha256: str


@dataclass(frozen=True)
class PackInspection:
    asset_root: Path
    identity: CheckoutIdentity
    manifest_bytes: bytes
    manifest_sha256: str
    asset_count: int
    payloads: tuple[RuntimePayload, ...]
    content_set_sha256: str


def _reject(code: str, field: str, message: str) -> AssetPreflightError:
    return AssetPreflightError(code, field, message)


def _run_native(arguments: Sequence[str], cwd: Path, timeout: int) -> NativeResult:
    environment = os.environ.copy()
    environment.update(
        {
            "GIT_TERMINAL_PROMPT": "0",
            "GCM_INTERACTIVE": "Never",
            "GIT_OPTIONAL_LOCKS": "0",
            "LC_ALL": "C",
        }
    )
    try:
        completed = subprocess.run(
            list(arguments),
            cwd=cwd,
            check=False,
            capture_output=True,
            timeout=timeout,
            shell=False,
            env=environment,
        )
    except FileNotFoundError as error:
        raise _reject(
            "NativeToolUnavailable",
            "repository",
            "The required local Git executable is unavailable.",
        ) from error
    except subprocess.TimeoutExpired as error:
        raise _reject(
            "NativeToolTimeout",
            "repository",
            "A bounded local Git inspection timed out.",
        ) from error
    return NativeResult(completed.returncode, completed.stdout)


def _git(
    asset_root: Path,
    arguments: Sequence[str],
    *,
    runner: NativeRunner,
    allowed_exit_codes: tuple[int, ...] = (0,),
) -> bytes:
    command = (
        "git",
        "-c",
        "core.fsmonitor=false",
        "-c",
        "core.untrackedCache=false",
        *arguments,
    )
    result = runner(command, asset_root, GIT_TIMEOUT_SECONDS)
    if result.exit_code not in allowed_exit_codes:
        raise _reject(
            "RepositoryUnavailable",
            "repository",
            "The local asset repository could not be inspected.",
        )
    return result.stdout


def _is_reparse_point(path: Path) -> bool:
    try:
        metadata = os.lstat(path)
    except OSError:
        return False
    attributes = getattr(metadata, "st_file_attributes", 0)
    reparse_attribute = getattr(stat, "FILE_ATTRIBUTE_REPARSE_POINT", 0)
    return stat.S_ISLNK(metadata.st_mode) or bool(attributes & reparse_attribute)


def _require_no_reparse_chain(path: Path, field: str) -> None:
    absolute = Path(os.path.abspath(path))
    chain: list[Path] = []
    current = absolute
    while True:
        chain.append(current)
        if current.parent == current:
            break
        current = current.parent
    for component in reversed(chain):
        if os.path.lexists(component) and _is_reparse_point(component):
            raise _reject(
                "AssetPathRejected",
                field,
                "A local asset path traverses a symbolic link or reparse point.",
            )


def _require_fully_qualified_directory(value: str, field: str) -> Path:
    if not value or not os.path.isabs(value):
        raise _reject(
            "InvalidRequest",
            field,
            "The local directory must be supplied as a fully qualified path.",
        )
    path = Path(os.path.abspath(value))
    _require_no_reparse_chain(path, field)
    if not path.is_dir():
        raise _reject(
            "RepositoryUnavailable",
            field,
            "The required local directory is unavailable.",
        )
    return path


def _require_canonical_git_id(value: str, field: str) -> str:
    if len(value) != 40 or any(character not in "0123456789abcdef" for character in value):
        raise _reject(
            "InvalidRequest",
            field,
            "The expected Git identity must be 40 lowercase hexadecimal characters.",
        )
    return value


def _require_sha256(value: str, field: str) -> str:
    if len(value) != 64 or any(character not in "0123456789ABCDEF" for character in value):
        raise _reject(
            "InvalidRequest",
            field,
            "The expected SHA-256 must be 64 uppercase hexadecimal characters.",
        )
    return value


def _decode_single_ascii(value: bytes, code: str, field: str) -> str:
    try:
        decoded = value.decode("ascii").strip()
    except UnicodeDecodeError as error:
        raise _reject(code, field, "A local repository identity is not canonical ASCII.") from error
    if not decoded or "\n" in decoded or "\r" in decoded:
        raise _reject(code, field, "A local repository identity is not singular.")
    return decoded


def _sha256_bytes(value: bytes) -> str:
    return hashlib.sha256(value).hexdigest().upper()


def _sha256_file(path: Path, expected_length: int | None = None) -> tuple[int, str]:
    try:
        length = path.stat().st_size
    except OSError as error:
        raise _reject(
            "PayloadUnavailable",
            "payload",
            "A referenced runtime payload is unavailable.",
        ) from error
    if expected_length is not None and length != expected_length:
        raise _reject(
            "PayloadMismatch",
            "payload",
            "A referenced runtime payload length drifted.",
        )
    if length < 1 or length > MAXIMUM_PAYLOAD_BYTES:
        raise _reject(
            "PayloadMismatch",
            "payload",
            "A referenced runtime payload exceeds the admitted byte bounds.",
        )
    digest = hashlib.sha256()
    try:
        with path.open("rb") as stream:
            while chunk := stream.read(1024 * 1024):
                digest.update(chunk)
    except OSError as error:
        raise _reject(
            "PayloadUnavailable",
            "payload",
            "A referenced runtime payload could not be read.",
        ) from error
    return length, digest.hexdigest().upper()


def _load_validator(path: Path, field: str) -> Draft202012Validator:
    try:
        schema = json.loads(path.read_text(encoding="utf-8"))
        Draft202012Validator.check_schema(schema)
    except (OSError, json.JSONDecodeError, SchemaError) as error:
        raise _reject(
            "SchemaUnavailable",
            field,
            "The tracked closed schema is unavailable or invalid.",
        ) from error
    return Draft202012Validator(schema)


def _validate_checkout_identity(
    asset_root: Path,
    *,
    expected_commit: str,
    expected_tree: str,
    allowed_untracked_path: PurePosixPath | None,
    required_ignored_path: PurePosixPath | None,
    runner: NativeRunner,
) -> CheckoutIdentity:
    git_directory = asset_root / ".git"
    _require_no_reparse_chain(git_directory, "repository")
    if not git_directory.is_dir():
        raise _reject(
            "RepositoryUnavailable",
            "repository",
            "The asset checkout must be an ordinary top-level Git repository.",
        )
    if _git(asset_root, ("rev-parse", "--is-inside-work-tree"), runner=runner).strip() != b"true":
        raise _reject(
            "RepositoryUnavailable",
            "repository",
            "The asset checkout is not a Git worktree.",
        )
    if _git(asset_root, ("rev-parse", "--is-bare-repository"), runner=runner).strip() != b"false":
        raise _reject(
            "RepositoryUnavailable",
            "repository",
            "A bare repository cannot be mounted as an asset checkout.",
        )
    if _git(asset_root, ("rev-parse", "--show-prefix"), runner=runner).strip():
        raise _reject(
            "RepositoryUnavailable",
            "repository",
            "The supplied asset directory is not the repository top level.",
        )

    commit = _decode_single_ascii(
        _git(asset_root, ("rev-parse", "--verify", "HEAD^{commit}"), runner=runner),
        "RepositoryIdentityMismatch",
        "assetRepositoryCommit",
    )
    tree = _decode_single_ascii(
        _git(asset_root, ("rev-parse", "HEAD^{tree}"), runner=runner),
        "RepositoryIdentityMismatch",
        "assetRepositoryTree",
    )
    if commit != expected_commit or tree != expected_tree:
        raise _reject(
            "RepositoryIdentityMismatch",
            "repository",
            "The asset repository commit or tree does not match the explicit pin.",
        )

    if _git(asset_root, ("remote",), runner=runner).strip():
        raise _reject(
            "RemotePolicyMismatch",
            "repository",
            "The local-only asset repository must have no configured remotes.",
        )

    staged = _git(asset_root, ("ls-files", "--stage", "-z"), runner=runner)
    if any(record.startswith(b"160000 ") for record in staged.split(b"\0") if record):
        raise _reject(
            "SubmoduleRejected",
            "repository",
            "The local asset repository must not contain submodules.",
        )
    if _git(asset_root, ("ls-files", "-z", "--", ".gitmodules"), runner=runner):
        raise _reject(
            "SubmoduleRejected",
            "repository",
            "The local asset repository must not declare submodules.",
        )

    status = _git(
        asset_root,
        ("status", "--porcelain=v2", "-z", "--untracked-files=all", "--ignored=no"),
        runner=runner,
    )
    expected_status = b""
    if allowed_untracked_path is not None:
        allowed = allowed_untracked_path.as_posix()
        if (
            allowed_untracked_path.is_absolute()
            or not allowed_untracked_path.parts
            or any(part in {"", ".", ".."} for part in allowed_untracked_path.parts)
            or "\\" in allowed
            or "\0" in allowed
            or "\n" in allowed
            or "\r" in allowed
        ):
            raise _reject(
                "InvalidRequest",
                "allowedUntrackedPath",
                "The allowed untracked asset path is not canonical.",
            )
        try:
            expected_status = b"? " + allowed.encode("utf-8") + b"\0"
        except UnicodeEncodeError as error:
            raise _reject(
                "InvalidRequest",
                "allowedUntrackedPath",
                "The allowed untracked asset path is not canonical UTF-8.",
            ) from error
        allowed_path = asset_root.joinpath(*allowed_untracked_path.parts)
        _require_no_reparse_chain(allowed_path, "repository")
        if not allowed_path.is_file():
            raise _reject(
                "RepositoryStateMismatch",
                "repository",
                "The explicitly allowed untracked asset is unavailable.",
            )
    if status != expected_status:
        raise _reject(
            "RepositoryStateMismatch",
            "repository",
            "The asset repository has tracked or nonignored untracked changes.",
        )

    if required_ignored_path is not None:
        ignored = required_ignored_path.as_posix()
        if (
            required_ignored_path.is_absolute()
            or not required_ignored_path.parts
            or any(part in {"", ".", ".."} for part in required_ignored_path.parts)
            or "\\" in ignored
            or "\0" in ignored
            or "\n" in ignored
            or "\r" in ignored
        ):
            raise _reject(
                "InvalidRequest",
                "requiredIgnoredPath",
                "The required ignored asset path is not canonical.",
            )
        try:
            ignored_bytes = ignored.encode("utf-8") + b"\n"
        except UnicodeEncodeError as error:
            raise _reject(
                "InvalidRequest",
                "requiredIgnoredPath",
                "The required ignored asset path is not canonical UTF-8.",
            ) from error
        ignored_output = _git(
            asset_root,
            ("check-ignore", "--no-index", "--", ignored),
            runner=runner,
            allowed_exit_codes=(0, 1),
        )
        if ignored_output != ignored_bytes:
            raise _reject(
                "RepositoryStateMismatch",
                "repository",
                "The candidate output boundary is not ignored by the asset repository.",
            )

    hooks_path = _git(
        asset_root,
        ("config", "--local", "--get", "core.hooksPath"),
        runner=runner,
        allowed_exit_codes=(0, 1),
    )
    if hooks_path.decode("utf-8", errors="replace").strip() != EXPECTED_HOOKS_PATH:
        raise _reject(
            "HookPolicyMismatch",
            "repository",
            "The local-only rejecting hook directory is not configured.",
        )
    tracked_hook = _git(
        asset_root,
        ("ls-files", "-z", "--", ".githooks/pre-push"),
        runner=runner,
    )
    if tracked_hook != b".githooks/pre-push\0":
        raise _reject(
            "HookPolicyMismatch",
            "repository",
            "The local-only rejecting pre-push hook is not tracked.",
        )
    hook_path = asset_root / ".githooks" / "pre-push"
    _require_no_reparse_chain(hook_path, "repository")
    try:
        hook_digest = _sha256_bytes(hook_path.read_bytes())
    except OSError as error:
        raise _reject(
            "HookPolicyMismatch",
            "repository",
            "The local-only rejecting pre-push hook is unavailable.",
        ) from error
    if hook_digest != EXPECTED_PRE_PUSH_HOOK_SHA256:
        raise _reject(
            "HookPolicyMismatch",
            "repository",
            "The local-only rejecting pre-push hook drifted.",
        )
    return CheckoutIdentity(commit, tree)


def validate_asset_checkout_identity(
    asset_root: str,
    *,
    expected_commit: str,
    expected_tree: str,
    allowed_untracked_path: PurePosixPath | None = None,
    required_ignored_path: PurePosixPath | None = None,
    runner: NativeRunner = _run_native,
) -> ValidatedAssetCheckout:
    """Validate one exact local-only asset checkout without reading its product manifest."""

    expected_commit = _require_canonical_git_id(expected_commit, "expectedCommit")
    expected_tree = _require_canonical_git_id(expected_tree, "expectedTree")
    root = _require_fully_qualified_directory(asset_root, "assetRoot")
    identity = _validate_checkout_identity(
        root,
        expected_commit=expected_commit,
        expected_tree=expected_tree,
        allowed_untracked_path=allowed_untracked_path,
        required_ignored_path=required_ignored_path,
        runner=runner,
    )
    return ValidatedAssetCheckout(root, identity)


def _tracked_asset_paths(asset_root: Path, runner: NativeRunner) -> set[str]:
    output = _git(
        asset_root,
        ("ls-files", "-z", "--", "manifests", "runtime"),
        runner=runner,
    )
    try:
        return {entry.decode("utf-8") for entry in output.split(b"\0") if entry}
    except UnicodeDecodeError as error:
        raise _reject(
            "AssetPathRejected",
            "manifest",
            "A tracked asset path is not canonical UTF-8.",
        ) from error


def _runtime_payload_path(asset_root: Path, value: str) -> tuple[PurePosixPath, Path]:
    if "\\" in value:
        raise _reject(
            "AssetPathRejected",
            "payload",
            "A runtime payload path is not a canonical relative path.",
        )
    relative = PurePosixPath(value)
    if (
        relative.is_absolute()
        or not relative.parts
        or relative.parts[0] != "runtime"
        or any(part in {"", ".", ".."} for part in relative.parts)
    ):
        raise _reject(
            "AssetPathRejected",
            "payload",
            "A runtime payload path is not a canonical runtime-relative path.",
        )
    target = asset_root.joinpath(*relative.parts)
    _require_no_reparse_chain(target, "payload")
    try:
        resolved_root = asset_root.resolve(strict=True)
        resolved_target = target.resolve(strict=True)
        resolved_target.relative_to(resolved_root)
    except (OSError, ValueError) as error:
        raise _reject(
            "AssetPathRejected",
            "payload",
            "A runtime payload does not resolve inside the admitted checkout.",
        ) from error
    if not target.is_file():
        raise _reject(
            "PayloadUnavailable",
            "payload",
            "A referenced runtime payload is unavailable.",
        )
    return relative, target


def _inspect_manifest(
    asset_root: Path,
    *,
    expected_manifest_sha256: str,
    runner: NativeRunner,
) -> tuple[bytes, int, tuple[RuntimePayload, ...], str]:
    manifest_path = asset_root.joinpath(*MANIFEST_RELATIVE_PATH.parts)
    _require_no_reparse_chain(manifest_path, "manifest")
    try:
        manifest_size = manifest_path.stat().st_size
    except OSError as error:
        raise _reject(
            "ManifestUnavailable",
            "manifest",
            "The presentation asset manifest is unavailable.",
        ) from error
    if manifest_size < 1 or manifest_size > MAXIMUM_MANIFEST_BYTES:
        raise _reject(
            "InvalidManifest",
            "manifest",
            "The presentation asset manifest exceeds the admitted byte bounds.",
        )
    try:
        manifest_bytes = manifest_path.read_bytes()
    except OSError as error:
        raise _reject(
            "ManifestUnavailable",
            "manifest",
            "The presentation asset manifest could not be read.",
        ) from error
    manifest_digest = _sha256_bytes(manifest_bytes)
    if manifest_digest != expected_manifest_sha256:
        raise _reject(
            "ManifestDigestMismatch",
            "manifestSha256",
            "The presentation asset manifest does not match the explicit digest pin.",
        )

    try:
        document = json.loads(manifest_bytes)
        _load_validator(PACK_SCHEMA, "manifestSchema").validate(document)
    except (json.JSONDecodeError, UnicodeDecodeError, ValidationError) as error:
        raise _reject(
            "InvalidManifest",
            "manifest",
            "The presentation asset manifest failed its closed schema.",
        ) from error
    if not isinstance(document, dict):
        raise _reject(
            "InvalidManifest",
            "manifest",
            "The presentation asset manifest must be an object.",
        )

    tracked = _tracked_asset_paths(asset_root, runner)
    manifest_key = MANIFEST_RELATIVE_PATH.as_posix()
    if manifest_key not in tracked:
        raise _reject(
            "RepositoryStateMismatch",
            "manifest",
            "The presentation asset manifest is not owned by the pinned Git tree.",
        )

    assets = document.get("assets")
    if not isinstance(assets, list):
        raise _reject("InvalidManifest", "assets", "The asset collection is invalid.")
    asset_ids: set[str] = set()
    runtime_paths: set[str] = set()
    resolved_payload_identities: set[str] = set()
    payloads: list[RuntimePayload] = []
    for asset in assets:
        if not isinstance(asset, Mapping):
            raise _reject("InvalidManifest", "assets", "An asset record is invalid.")
        asset_id = asset.get("assetId")
        if not isinstance(asset_id, str) or asset_id in asset_ids:
            raise _reject(
                "DuplicateIdentity",
                "assets",
                "The presentation asset manifest has duplicate asset identities.",
            )
        asset_ids.add(asset_id)
        buckets = asset.get("buckets")
        if not isinstance(buckets, list):
            raise _reject("InvalidManifest", "buckets", "An asset bucket collection is invalid.")
        for bucket in buckets:
            if not isinstance(bucket, Mapping):
                raise _reject("InvalidManifest", "bucket", "An asset bucket record is invalid.")
            runtime_path = bucket.get("runtimePath")
            byte_length = bucket.get("byteLength")
            expected_digest = bucket.get("sha256")
            if (
                not isinstance(runtime_path, str)
                or runtime_path in runtime_paths
                or not isinstance(byte_length, int)
                or isinstance(byte_length, bool)
                or not isinstance(expected_digest, str)
            ):
                raise _reject(
                    "DuplicateIdentity",
                    "buckets",
                    "The presentation asset manifest has an invalid or duplicate runtime identity.",
                )
            runtime_paths.add(runtime_path)
            relative, source_path = _runtime_payload_path(asset_root, runtime_path)
            resolved_identity = os.path.normcase(os.path.realpath(source_path))
            if resolved_identity in resolved_payload_identities:
                raise _reject(
                    "DuplicateIdentity",
                    "buckets",
                    "Multiple runtime identities resolve to the same local payload.",
                )
            resolved_payload_identities.add(resolved_identity)
            if relative.as_posix() not in tracked:
                raise _reject(
                    "RepositoryStateMismatch",
                    "payload",
                    "A runtime payload is not owned by the pinned Git tree.",
                )
            actual_length, actual_digest = _sha256_file(source_path, byte_length)
            if actual_digest != expected_digest.upper():
                raise _reject(
                    "PayloadMismatch",
                    "payload",
                    "A referenced runtime payload digest drifted.",
                )
            payloads.append(RuntimePayload(relative, source_path, actual_length, actual_digest))

    ordered_payloads = tuple(sorted(payloads, key=lambda payload: payload.relative_path.as_posix()))
    content_set = hashlib.sha256()
    content_set.update(DESCRIPTOR_ID.encode("ascii"))
    content_set.update(b"\0")
    content_set.update(manifest_digest.encode("ascii"))
    content_set.update(b"\0")
    for payload in ordered_payloads:
        content_set.update(payload.relative_path.as_posix().encode("utf-8"))
        content_set.update(b"\0")
        content_set.update(str(payload.byte_length).encode("ascii"))
        content_set.update(b"\0")
        content_set.update(payload.sha256.encode("ascii"))
        content_set.update(b"\0")
    return manifest_bytes, len(assets), ordered_payloads, content_set.hexdigest().upper()


def inspect_asset_checkout(
    asset_root: str,
    *,
    expected_commit: str,
    expected_tree: str,
    expected_manifest_sha256: str,
    runner: NativeRunner = _run_native,
) -> PackInspection:
    """Validate one exact clean local asset checkout without changing it."""

    expected_manifest_sha256 = _require_sha256(
        expected_manifest_sha256,
        "expectedManifestSha256",
    )
    checkout = validate_asset_checkout_identity(
        asset_root,
        expected_commit=expected_commit,
        expected_tree=expected_tree,
        runner=runner,
    )
    manifest_bytes, asset_count, payloads, content_set_sha256 = _inspect_manifest(
        checkout.root,
        expected_manifest_sha256=expected_manifest_sha256,
        runner=runner,
    )
    return PackInspection(
        checkout.root,
        checkout.identity,
        manifest_bytes,
        expected_manifest_sha256,
        asset_count,
        payloads,
        content_set_sha256,
    )


def _descriptor(inspection: PackInspection, mount_kind: str) -> dict[str, object]:
    descriptor: dict[str, object] = {
        "schemaVersion": 1,
        "descriptorId": DESCRIPTOR_ID,
        "packageId": PACKAGE_ID,
        "repositoryId": REPOSITORY_ID,
        "profile": PROFILE,
        "packCapability": PACK_CAPABILITY,
        "preflightCapability": PREFLIGHT_CAPABILITY,
        "mountKind": mount_kind,
        "assetRepositoryCommit": inspection.identity.commit,
        "assetRepositoryTree": inspection.identity.tree,
        "manifestSha256": inspection.manifest_sha256,
        "assetCount": inspection.asset_count,
        "bucketCount": len(inspection.payloads),
        "bucketScales": [2, 4],
        "totalPayloadBytes": sum(payload.byte_length for payload in inspection.payloads),
        "contentSetSha256": inspection.content_set_sha256,
        "status": "Pass",
    }
    try:
        _load_validator(MOUNT_SCHEMA, "mountSchema").validate(descriptor)
    except ValidationError as error:
        raise _reject(
            "InvalidDescriptor",
            "descriptor",
            "The generated mount descriptor failed its closed schema.",
        ) from error
    return descriptor


def preflight_asset_checkout(
    asset_root: str,
    *,
    expected_commit: str,
    expected_tree: str,
    expected_manifest_sha256: str,
    runner: NativeRunner = _run_native,
) -> dict[str, object]:
    inspection = inspect_asset_checkout(
        asset_root,
        expected_commit=expected_commit,
        expected_tree=expected_tree,
        expected_manifest_sha256=expected_manifest_sha256,
        runner=runner,
    )
    return _descriptor(inspection, "checkout")


def _is_within(candidate: Path, parent: Path) -> bool:
    try:
        common = os.path.commonpath(
            (
                os.path.normcase(os.path.abspath(candidate)),
                os.path.normcase(os.path.abspath(parent)),
            )
        )
    except ValueError:
        return False
    return common == os.path.normcase(os.path.abspath(parent))


def _require_fresh_export_destination(
    value: str,
    asset_root: Path,
) -> tuple[Path, Path]:
    if not value or not os.path.isabs(value):
        raise _reject(
            "InvalidRequest",
            "destination",
            "The export destination must be supplied as a fully qualified path.",
        )
    destination = Path(os.path.abspath(value))
    parent = destination.parent
    _require_no_reparse_chain(parent, "destination")
    if not parent.is_dir():
        raise _reject(
            "ExportDestinationRejected",
            "destination",
            "The export destination parent is unavailable.",
        )
    if _is_within(destination, asset_root):
        raise _reject(
            "ExportDestinationRejected",
            "destination",
            "The export destination must remain outside the source asset checkout.",
        )
    if os.path.lexists(destination):
        raise _reject(
            "ExportDestinationRejected",
            "destination",
            "The export destination must not already exist.",
        )
    return destination, parent


def _cleanup_owned_temp(temp: Path, parent: Path) -> None:
    if (
        temp.parent != parent
        or not temp.name.startswith(_EXPORT_TEMP_PREFIX)
        or not os.path.lexists(temp)
    ):
        return
    if _is_reparse_point(temp):
        raise _reject(
            "ExportCleanupRejected",
            "destination",
            "The owned export staging directory became a reparse point.",
        )
    try:
        shutil.rmtree(temp)
    except OSError as error:
        raise _reject(
            "ExportCleanupFailed",
            "destination",
            "The owned export staging directory could not be cleaned.",
        ) from error


def export_asset_pack(
    asset_root: str,
    destination: str,
    *,
    expected_commit: str,
    expected_tree: str,
    expected_manifest_sha256: str,
    runner: NativeRunner = _run_native,
) -> dict[str, object]:
    """Copy only an exact manifest and its referenced runtime payloads atomically."""

    inspection = inspect_asset_checkout(
        asset_root,
        expected_commit=expected_commit,
        expected_tree=expected_tree,
        expected_manifest_sha256=expected_manifest_sha256,
        runner=runner,
    )
    export_root, export_parent = _require_fresh_export_destination(
        destination,
        inspection.asset_root,
    )
    temp = export_parent / f"{_EXPORT_TEMP_PREFIX}{uuid.uuid4().hex}.tmp"
    if os.path.lexists(temp):
        raise _reject(
            "ExportDestinationRejected",
            "destination",
            "The fresh export staging identity unexpectedly exists.",
        )
    promoted = False
    try:
        try:
            temp.mkdir()
        except OSError as error:
            raise _reject(
                "ExportStagingFailed",
                "destination",
                "The fresh export staging directory could not be created.",
            ) from error
        manifest_target = temp.joinpath(*MANIFEST_RELATIVE_PATH.parts)
        manifest_target.parent.mkdir(parents=True)
        manifest_target.write_bytes(inspection.manifest_bytes)
        if _sha256_bytes(manifest_target.read_bytes()) != inspection.manifest_sha256:
            raise _reject(
                "ExportVerificationFailed",
                "manifest",
                "The copied presentation manifest failed post-copy verification.",
            )

        for payload in inspection.payloads:
            target = temp.joinpath(*payload.relative_path.parts)
            target.parent.mkdir(parents=True, exist_ok=True)
            shutil.copyfile(payload.source_path, target)
            copied_length, copied_digest = _sha256_file(target, payload.byte_length)
            if copied_length != payload.byte_length or copied_digest != payload.sha256:
                raise _reject(
                    "ExportVerificationFailed",
                    "payload",
                    "A copied runtime payload failed post-copy verification.",
                )

        repeated = inspect_asset_checkout(
            asset_root,
            expected_commit=inspection.identity.commit,
            expected_tree=inspection.identity.tree,
            expected_manifest_sha256=inspection.manifest_sha256,
            runner=runner,
        )
        if (
            repeated.content_set_sha256 != inspection.content_set_sha256
            or repeated.asset_count != inspection.asset_count
            or len(repeated.payloads) != len(inspection.payloads)
        ):
            raise _reject(
                "RepositoryStateMismatch",
                "repository",
                "The source checkout changed during export.",
            )

        descriptor = _descriptor(inspection, "exported-pack")
        descriptor_bytes = (
            json.dumps(descriptor, indent=2, ensure_ascii=True, sort_keys=True) + "\n"
        ).encode("utf-8")
        (temp / DESCRIPTOR_FILE_NAME).write_bytes(descriptor_bytes)
        if os.path.lexists(export_root):
            raise _reject(
                "ExportDestinationRejected",
                "destination",
                "The export destination appeared before atomic promotion.",
            )
        os.rename(temp, export_root)
        promoted = True
        return descriptor
    except AssetPreflightError:
        raise
    except OSError as error:
        raise _reject(
            "ExportFailed",
            "destination",
            "The local presentation asset export failed.",
        ) from error
    finally:
        if not promoted:
            _cleanup_owned_temp(temp, export_parent)


def _add_identity_arguments(parser: argparse.ArgumentParser) -> None:
    parser.add_argument("--asset-root", required=True)
    parser.add_argument("--expected-commit", required=True)
    parser.add_argument("--expected-tree", required=True)
    parser.add_argument("--expected-manifest-sha256", required=True)


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(
        description="Validate or export one exact local presentation asset checkout."
    )
    subparsers = parser.add_subparsers(dest="command", required=True)
    checkout_parser = subparsers.add_parser("checkout")
    _add_identity_arguments(checkout_parser)
    export_parser = subparsers.add_parser("export")
    _add_identity_arguments(export_parser)
    export_parser.add_argument("--destination", required=True)
    arguments = parser.parse_args(argv)

    try:
        common = {
            "expected_commit": arguments.expected_commit,
            "expected_tree": arguments.expected_tree,
            "expected_manifest_sha256": arguments.expected_manifest_sha256,
        }
        if arguments.command == "checkout":
            descriptor = preflight_asset_checkout(arguments.asset_root, **common)
        else:
            descriptor = export_asset_pack(
                arguments.asset_root,
                arguments.destination,
                **common,
            )
    except AssetPreflightError as error:
        print(
            json.dumps(
                {"status": "Rejected", "diagnostic": error.as_dict()},
                ensure_ascii=True,
                sort_keys=True,
            ),
            file=sys.stderr,
        )
        return 2
    print(json.dumps(descriptor, ensure_ascii=True, sort_keys=True))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
