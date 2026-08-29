from __future__ import annotations

import hashlib
import json
import sys
import zipfile
from pathlib import Path

import pytest

from sf2tool.remake_godot import (
    ArtifactSpec,
    ProcessReceipt,
    _parse_smoke_receipt,
    _require_clean_export_output,
    _scan_export,
    extract_zip_members,
    load_toolchain_manifest,
    run_bounded_process,
    verify_artifact,
)

ROOT = Path(__file__).resolve().parents[2]


def test_tracked_toolchain_manifest_locks_official_godot_dotnet_release() -> None:
    toolchain = load_toolchain_manifest(ROOT / "remake" / "toolchain.json")

    assert toolchain.version_output == "4.7.2.stable.mono.official.ed1daf0bf"
    assert toolchain.template_directory == "4.7.2.stable.mono"
    assert toolchain.public_export_preset == "Public Synthetic Windows"
    assert toolchain.editor.file_name == "Godot_v4.7.2-stable_mono_win64.zip"
    assert toolchain.editor.sha256 == (
        "a2a48473a7414c5f19fab690518caebb738c09ef9601f6bd2388676a7f53b3c0"
    )
    assert toolchain.export_templates.file_name == (
        "Godot_v4.7.2-stable_mono_export_templates.tpz"
    )
    assert toolchain.export_templates.sha256 == (
        "92f8681e349ef1f90891b792da95e3b2b0bd1ed610b78018c58feb2d87e15a9d"
    )
    assert toolchain.export_template_members == (
        "templates/version.txt",
        "templates/windows_debug_x86_64.exe",
        "templates/windows_debug_x86_64_console.exe",
        "templates/windows_release_x86_64.exe",
        "templates/windows_release_x86_64_console.exe",
    )


def test_artifact_verification_is_exact_for_size_and_sha256(tmp_path: Path) -> None:
    artifact = tmp_path / "artifact.zip"
    artifact.write_bytes(b"official bytes")
    digest = hashlib.sha256(artifact.read_bytes()).hexdigest()
    spec = ArtifactSpec("artifact.zip", "https://example.invalid/artifact.zip", digest, 14)

    assert verify_artifact(artifact, spec)["status"] == "Pass"

    with pytest.raises(ValueError, match="size mismatch"):
        verify_artifact(
            artifact,
            ArtifactSpec(spec.file_name, spec.url, spec.sha256, spec.size + 1),
        )
    with pytest.raises(ValueError, match="SHA-256 mismatch"):
        verify_artifact(
            artifact,
            ArtifactSpec(spec.file_name, spec.url, "0" * 64, spec.size),
        )


def test_archive_extraction_closes_locked_members_and_strips_prefix(tmp_path: Path) -> None:
    archive = tmp_path / "templates.tpz"
    with zipfile.ZipFile(archive, "w") as output:
        output.writestr("templates/version.txt", "4.7.2.stable.mono\n")
        output.writestr("templates/windows_release_x86_64.exe", b"template")
        output.writestr("templates/unlocked.exe", b"excluded")

    destination = tmp_path / "templates"
    extracted = extract_zip_members(
        archive,
        destination,
        members=(
            "templates/version.txt",
            "templates/windows_release_x86_64.exe",
        ),
        strip_prefix="templates",
    )

    assert {path.relative_to(destination).as_posix() for path in extracted} == {
        "version.txt",
        "windows_release_x86_64.exe",
    }
    assert not (destination / "unlocked.exe").exists()


def test_archive_extraction_rejects_missing_and_escaping_members(tmp_path: Path) -> None:
    missing_archive = tmp_path / "missing.zip"
    with zipfile.ZipFile(missing_archive, "w") as output:
        output.writestr("templates/version.txt", "version")

    with pytest.raises(ValueError, match="missing locked member"):
        extract_zip_members(
            missing_archive,
            tmp_path / "missing-output",
            members=("templates/release.exe",),
        )

    unsafe_archive = tmp_path / "unsafe.zip"
    with zipfile.ZipFile(unsafe_archive, "w") as output:
        output.writestr("../escape.txt", "escape")

    with pytest.raises(ValueError, match="unsafe archive member"):
        extract_zip_members(unsafe_archive, tmp_path / "unsafe-output")
    assert not (tmp_path / "escape.txt").exists()


def test_bounded_process_records_success_without_shell(tmp_path: Path) -> None:
    receipt = run_bounded_process(
        "success",
        [sys.executable, "-c", "print('bounded')"],
        cwd=tmp_path,
        environment={},
        timeout=10,
        termination_timeout=5,
        reap_timeout=5,
    )

    assert receipt.passed
    assert receipt.stdout_tail.strip() == "bounded"
    assert receipt.cleanup_status == "clean"


def test_bounded_process_times_out_and_reaps_process(tmp_path: Path) -> None:
    receipt = run_bounded_process(
        "timeout",
        [sys.executable, "-c", "import time; time.sleep(60)"],
        cwd=tmp_path,
        environment={},
        timeout=1,
        termination_timeout=5,
        reap_timeout=5,
    )

    assert receipt.timed_out
    assert receipt.cleanup_status == "clean"
    assert not receipt.passed


def test_smoke_receipt_requires_exact_public_synthetic_projection() -> None:
    payload = {
        "status": "Pass",
        "profile": "public-synthetic",
        "scenarioId": "map3-public-synthetic-smoke",
        "exactControlledAdmission": False,
        "capability": "map3-synthetic-exploration-smoke",
        "evidenceOwner": "sf2-map3-admitted-start-runtime-v1",
        "mapId": "map3",
        "opaqueStartFacing": 3,
        "before": {"x": 56, "y": 3},
        "after": {"x": 57, "y": 3},
        "outcome": "Moved",
        "simulationStep": 1,
        "banner": "PUBLIC SYNTHETIC — NOT ORIGINAL FIDELITY",
    }
    output = "Godot Engine\nSF2_MAP3_SMOKE " + json.dumps(payload) + "\n"

    assert _parse_smoke_receipt(output) == payload

    payload["status"] = "Fail"
    with pytest.raises(RuntimeError, match="closed public-synthetic contract"):
        _parse_smoke_receipt("SF2_MAP3_SMOKE " + json.dumps(payload))


def test_zero_exit_export_still_rejects_godot_error_or_warning() -> None:
    clean = ProcessReceipt("export", ("godot",), 0, False, "clean", "Exported", "")
    errored = ProcessReceipt("export", ("godot",), 0, False, "clean", "", "ERROR: failed")
    warned = ProcessReceipt(
        "export",
        ("godot",),
        0,
        False,
        "clean",
        'Project export for preset "Public" completed with warnings.',
        "",
    )

    _require_clean_export_output(clean)
    with pytest.raises(RuntimeError, match="zero exit code"):
        _require_clean_export_output(errored)
    with pytest.raises(RuntimeError, match="zero exit code"):
        _require_clean_export_output(warned)


def test_export_scan_closes_required_outputs_without_returning_every_runtime_file(
    tmp_path: Path,
) -> None:
    required = (
        "sf2-map3-public-synthetic.exe",
        "sf2-map3-public-synthetic.pck",
        "Sf2.Remake.Application.dll",
        "Sf2.Remake.Content.dll",
        "Sf2.Remake.Domain.dll",
        "Sf2.Remake.Godot.dll",
    )
    for name in required:
        path = tmp_path / ("data" if name.endswith(".dll") else "") / name
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_bytes(name.encode())
    (tmp_path / "data/System.Private.CoreLib.dll").write_bytes(b"runtime")

    receipt = _scan_export(tmp_path)

    assert receipt["fileCount"] == 7
    assert len(receipt["requiredFiles"]) == 6
    assert len(receipt["manifestSha256"]) == 64

    (tmp_path / "leaked.rom").write_bytes(b"private")
    with pytest.raises(RuntimeError, match="forbidden payload"):
        _scan_export(tmp_path)
