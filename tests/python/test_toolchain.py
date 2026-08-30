from __future__ import annotations

import hashlib
import json
from copy import deepcopy
from pathlib import Path

import pytest

import sf2tool.toolchain as toolchain
from sf2tool.private_inputs import JDK_INPUT_IDENTITY, SHARED_INPUT_ROOT_ENV

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
