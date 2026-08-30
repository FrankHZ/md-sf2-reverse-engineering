from __future__ import annotations

import hashlib
import json
from copy import deepcopy
from pathlib import Path

import pytest

import sf2tool.private_inputs as private_inputs
import sf2tool.toolchain as toolchain
from sf2tool.private_inputs import (
    BIZHAWK_ARCHIVE_INPUT_IDENTITY,
    JDK_INPUT_IDENTITY,
    SHARED_INPUT_ROOT_ENV,
)

ROOT = Path(__file__).resolve().parents[2]


def _write_jdk(shared_root: Path) -> Path:
    root = shared_root / JDK_INPUT_IDENTITY
    java = root / "jdk-17.0.19+10" / "bin" / "java.exe"
    java.parent.mkdir(parents=True)
    java.write_bytes(b"synthetic-java")
    (root / "jdk-17.0.19+10" / "NOTICE").write_bytes(b"upper")
    (root / "jdk-17.0.19+10" / "bin" / "alpha.dll").write_bytes(b"lower")
    return root


def _contract(root: Path) -> dict[str, object]:
    identity = toolchain._tree_identity(root)
    java = root / "jdk-17.0.19+10" / "bin" / "java.exe"
    return {
        "sharedInputIdentity": JDK_INPUT_IDENTITY.as_posix(),
        "sharedJavaRelativePath": "jdk-17.0.19+10/bin/java.exe",
        "localJavaPath": (
            "local/toolchains/jdk-17.0.19+10/"
            "jdk-17.0.19+10/bin/java.exe"
        ),
        "extractedFileCount": identity["fileCount"],
        "extractedSizeBytes": identity["sizeBytes"],
        "extractedTreeDigestAlgorithm": toolchain.JDK_TREE_DIGEST_ALGORITHM,
        "extractedTreeSha256": identity["sha256"],
        "javaExecutableSizeBytes": java.stat().st_size,
        "javaExecutableSha256": toolchain._sha256(java),
    }


def _write_bizhawk_archive(shared_root: Path) -> Path:
    archive = shared_root / BIZHAWK_ARCHIVE_INPUT_IDENTITY
    archive.parent.mkdir(parents=True)
    archive.write_bytes(b"synthetic-bizhawk-archive")
    return archive


def _use_synthetic_repo(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> Path:
    root = tmp_path / "repo"
    root.mkdir()

    def synthetic_repo_path(relative: str | Path) -> Path:
        return root / relative

    monkeypatch.setattr(private_inputs, "repo_path", synthetic_repo_path)
    monkeypatch.setattr(toolchain, "repo_path", synthetic_repo_path)
    return root


def test_tracked_manifest_pins_the_audited_shared_jdk_tree() -> None:
    manifest = json.loads((ROOT / "manifests" / "toolchain.json").read_text(encoding="utf-8"))
    java = manifest["java"]

    assert java["sharedInputIdentity"] == "toolchains/jdk-17.0.19+10"
    assert java["sharedJavaRelativePath"] == "jdk-17.0.19+10/bin/java.exe"
    assert java["extractedFileCount"] == 492
    assert java["extractedSizeBytes"] == 317_525_612
    assert java["extractedTreeDigestAlgorithm"] == toolchain.JDK_TREE_DIGEST_ALGORITHM
    assert java["extractedTreeSha256"] == (
        "6437CD09380EF13FA9BFE553D16F0EED34D796037649C95DF6C72758CD90DDAE"
    )
    assert java["javaExecutableSizeBytes"] == 50_344
    assert java["javaExecutableSha256"] == (
        "B3AFE83E1AB067DA4C56F1A7B2BA4C14EC832D694333F35B2B45178E9AC596EF"
    )


def test_tracked_manifest_registers_the_audited_shared_bizhawk_archive() -> None:
    manifest = json.loads((ROOT / "manifests" / "toolchain.json").read_text(encoding="utf-8"))
    bizhawk = manifest["bizhawk"]

    assert bizhawk["sharedArchiveInputIdentity"] == (
        "archives/BizHawk-2.11.1-win-x64.zip"
    )
    assert bizhawk["localArchivePath"] == (
        "local/toolchains/BizHawk-2.11.1-win-x64.zip"
    )
    assert bizhawk["archiveSizeBytes"] == 97_984_101
    assert bizhawk["archiveSha256"] == (
        "DD7CBD5E205B09C5BCE6AFABC4F8AA525ACC0BBEB9753B198A8DB3FE6A5E5717"
    )


def test_tree_identity_uses_case_sensitive_ordinal_paths(tmp_path: Path) -> None:
    root = _write_jdk(tmp_path / "shared")
    rows = []
    for path in root.rglob("*"):
        if path.is_file():
            rows.append(
                (
                    path.relative_to(root).as_posix(),
                    path.stat().st_size,
                    toolchain._sha256(path),
                )
            )
    ordinal = sorted(rows, key=lambda row: row[0])
    casefolded = sorted(rows, key=lambda row: row[0].casefold())

    def digest(records: list[tuple[str, int, str]]) -> str:
        text = "\n".join(f"{path}\t{size}\t{sha}" for path, size, sha in records)
        return hashlib.sha256(text.encode("utf-8")).hexdigest().upper()

    assert ordinal != casefolded
    assert toolchain._tree_identity(root)["sha256"] == digest(ordinal)
    assert digest(ordinal) != digest(casefolded)


@pytest.mark.parametrize("drift", ("content", "path", "count", "size"))
def test_jdk_directory_verification_rejects_every_tree_drift(
    drift: str, tmp_path: Path
) -> None:
    root = _write_jdk(tmp_path / "shared")
    contract = _contract(root)
    alpha = root / "jdk-17.0.19+10" / "bin" / "alpha.dll"
    if drift == "content":
        alpha.write_bytes(b"LOWER")
    elif drift == "path":
        alpha.rename(alpha.with_name("beta.dll"))
    elif drift == "count":
        alpha.with_name("extra.dll").write_bytes(b"")
    else:
        alpha.write_bytes(b"different-size")

    with pytest.raises(ValueError, match="JDK tree provenance mismatch"):
        toolchain._verify_jdk_directory(root, contract)


def test_jdk_tree_rejects_an_internal_reparse_entry(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    root = _write_jdk(tmp_path / "shared")
    marker = root / "jdk-17.0.19+10" / "NOTICE"
    original = toolchain._is_reparse
    monkeypatch.setattr(toolchain, "_is_reparse", lambda path: path == marker or original(path))

    with pytest.raises(ValueError, match="reparse entry"):
        toolchain._tree_identity(root)


def test_default_java_uses_and_verifies_the_shared_jdk(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    shared = tmp_path / "shared"
    root = _write_jdk(shared)
    manifest = {"java": _contract(root)}
    monkeypatch.setenv(SHARED_INPUT_ROOT_ENV, str(shared.resolve()))

    resolved = toolchain._resolve_java_path(manifest, None)

    assert resolved == (root / "jdk-17.0.19+10" / "bin" / "java.exe").resolve()


def test_default_java_rejects_manifest_layout_drift(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    shared = tmp_path / "shared"
    root = _write_jdk(shared)
    contract = _contract(root)
    contract["sharedJavaRelativePath"] = "jdk-17.0.19+10/bin/other.exe"
    monkeypatch.setenv(SHARED_INPUT_ROOT_ENV, str(shared.resolve()))

    with pytest.raises(ValueError, match="layouts disagree"):
        toolchain._resolve_java_path({"java": contract}, None)


def test_explicit_java_override_ignores_an_invalid_shared_root(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    java = tmp_path / "explicit-java.exe"
    java.write_bytes(b"synthetic-explicit-java")
    monkeypatch.setenv(SHARED_INPUT_ROOT_ENV, "relative-invalid-root")

    assert toolchain._resolve_java_path({}, java) == java.resolve()


def test_jdk_verifier_rejects_unknown_digest_algorithm(tmp_path: Path) -> None:
    root = _write_jdk(tmp_path / "shared")
    contract = deepcopy(_contract(root))
    contract["extractedTreeDigestAlgorithm"] = "unknown"

    with pytest.raises(ValueError, match="unsupported JDK tree digest algorithm"):
        toolchain._verify_jdk_directory(root, contract)


def test_bizhawk_archive_uses_the_shared_input_without_a_local_archive(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    repo = _use_synthetic_repo(monkeypatch, tmp_path)
    shared = tmp_path / "shared"
    expected = _write_bizhawk_archive(shared).resolve()
    contract = json.loads(
        (ROOT / "manifests" / "toolchain.json").read_text(encoding="utf-8")
    )["bizhawk"]
    monkeypatch.setenv(SHARED_INPUT_ROOT_ENV, str(shared.resolve()))

    assert not (repo / contract["localArchivePath"]).exists()
    assert toolchain._resolve_bizhawk_archive(contract) == expected


def test_bizhawk_archive_keeps_the_repo_local_fallback_when_unset(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    repo = _use_synthetic_repo(monkeypatch, tmp_path)
    contract = json.loads(
        (ROOT / "manifests" / "toolchain.json").read_text(encoding="utf-8")
    )["bizhawk"]
    monkeypatch.delenv(SHARED_INPUT_ROOT_ENV, raising=False)

    assert toolchain._resolve_bizhawk_archive(contract) == repo / contract["localArchivePath"]


@pytest.mark.parametrize(
    ("field", "value", "message"),
    (
        ("sharedArchiveInputIdentity", "archives/other.zip", "registered identity"),
        ("localArchivePath", "local/toolchains/other.zip", "layouts disagree"),
    ),
)
def test_bizhawk_archive_rejects_manifest_identity_and_layout_drift(
    monkeypatch: pytest.MonkeyPatch,
    tmp_path: Path,
    field: str,
    value: str,
    message: str,
) -> None:
    shared = tmp_path / "shared"
    _write_bizhawk_archive(shared)
    contract = deepcopy(
        json.loads((ROOT / "manifests" / "toolchain.json").read_text(encoding="utf-8"))[
            "bizhawk"
        ]
    )
    contract[field] = value
    monkeypatch.setenv(SHARED_INPUT_ROOT_ENV, str(shared.resolve()))

    with pytest.raises(ValueError, match=message):
        toolchain._resolve_bizhawk_archive(contract)


def test_verify_toolchain_uses_shared_archive_and_repo_local_executable_independently(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    repo = _use_synthetic_repo(monkeypatch, tmp_path)
    manifest = json.loads((ROOT / "manifests" / "toolchain.json").read_text(encoding="utf-8"))
    manifest_path = tmp_path / "toolchain.json"
    manifest_path.write_text(json.dumps(manifest), encoding="utf-8")
    upstream = tmp_path / "upstream"
    upstream.mkdir()
    java = tmp_path / "java.exe"
    java.write_bytes(b"synthetic-java")
    shared = tmp_path / "shared"
    archive = _write_bizhawk_archive(shared).resolve()
    monkeypatch.setenv(SHARED_INPUT_ROOT_ENV, str(shared.resolve()))
    verified: list[tuple[Path, str]] = []

    def fake_run(arguments: list[str | Path], *, cwd: Path | None = None) -> str:
        command = [str(argument) for argument in arguments]
        if command[:4] == ["git", "remote", "get-url", "origin"]:
            return manifest["sf2disasm"]["repository"]
        if command[:3] == ["git", "rev-parse", "HEAD"]:
            return manifest["sf2disasm"]["commit"]
        if command[:3] == ["git", "status", "--porcelain"]:
            return ""
        if command == [str(java.resolve()), "-version"]:
            return 'openjdk version "17.0.19"'
        raise AssertionError(f"unexpected command: {command}, cwd={cwd}")

    def capture_file(
        path: Path, *, size: int, sha256: str, owner: str
    ) -> None:
        del size, sha256
        verified.append((path.resolve(), owner))

    monkeypatch.setattr(toolchain, "_run", fake_run)
    monkeypatch.setattr(toolchain, "_verify_file", capture_file)

    result = toolchain.verify_toolchain(upstream, manifest_path, java)

    assert (archive, "BizHawk archive") in verified
    assert (
        (repo / manifest["bizhawk"]["localExecutablePath"]).resolve(),
        "BizHawk executable",
    ) in verified
    assert result["BizHawkPath"] == str(
        repo / manifest["bizhawk"]["localExecutablePath"]
    )
