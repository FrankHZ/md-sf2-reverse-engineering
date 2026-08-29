"""Official-CLI Godot gate for the public-synthetic remake profile."""

from __future__ import annotations

import argparse
import csv
import hashlib
import json
import os
import shutil
import subprocess
import uuid
import zipfile
from collections.abc import Callable, Iterable, Mapping
from contextlib import suppress
from dataclasses import dataclass
from pathlib import Path, PurePosixPath
from typing import Any

from sf2tool.paths import REPO_ROOT, display_path, repo_path

DEFAULT_MANIFEST = repo_path("remake/toolchain.json")
DEFAULT_TOOLCHAIN_ROOT = repo_path("local/toolchains/godot-4.7.2")
DEFAULT_PROJECT = repo_path("remake/game")
DEFAULT_SCRATCH_PARENT = repo_path("local/gates/godot")
SMOKE_MARKER = "SF2_MAP3_SMOKE "
MAX_DIAGNOSTIC_CHARACTERS = 8192


@dataclass(frozen=True)
class ArtifactSpec:
    """One immutable official release artifact."""

    file_name: str
    url: str
    sha256: str
    size: int


@dataclass(frozen=True)
class GateTimeouts:
    """Finite wall-clock budgets for every external-process stage."""

    version: int
    restore: int
    build: int
    import_project: int
    run: int
    export: int
    termination: int
    reap: int


@dataclass(frozen=True)
class GodotToolchain:
    """Validated tracked identity for the editor and export templates."""

    version_output: str
    template_directory: str
    public_export_preset: str
    editor: ArtifactSpec
    export_templates: ArtifactSpec
    export_template_members: tuple[str, ...]
    timeouts: GateTimeouts


@dataclass(frozen=True)
class ProcessReceipt:
    """Bounded result of one native process."""

    step: str
    command: tuple[str, ...]
    exit_code: int | None
    timed_out: bool
    cleanup_status: str
    stdout_tail: str
    stderr_tail: str

    @property
    def passed(self) -> bool:
        return self.exit_code == 0 and not self.timed_out and self.cleanup_status == "clean"

    def as_dict(self) -> dict[str, Any]:
        return {
            "step": self.step,
            "command": list(self.command),
            "exitCode": self.exit_code,
            "timedOut": self.timed_out,
            "cleanupStatus": self.cleanup_status,
            "stdoutTail": self.stdout_tail,
            "stderrTail": self.stderr_tail,
            "status": "Pass" if self.passed else "Fail",
        }


def _required_mapping(value: object, field: str) -> Mapping[str, object]:
    if not isinstance(value, dict):
        raise ValueError(f"{field} must be an object")
    return value


def _required_string(mapping: Mapping[str, object], field: str) -> str:
    value = mapping.get(field)
    if not isinstance(value, str) or not value.strip():
        raise ValueError(f"{field} must be a non-empty string")
    return value


def _required_positive_int(mapping: Mapping[str, object], field: str) -> int:
    value = mapping.get(field)
    if not isinstance(value, int) or isinstance(value, bool) or value < 1:
        raise ValueError(f"{field} must be a positive integer")
    return value


def _artifact(mapping: Mapping[str, object], field: str) -> ArtifactSpec:
    artifact = _required_mapping(mapping.get(field), field)
    sha256 = _required_string(artifact, "sha256").lower()
    if len(sha256) != 64 or any(character not in "0123456789abcdef" for character in sha256):
        raise ValueError(f"{field}.sha256 must be a lowercase SHA-256 digest")
    return ArtifactSpec(
        _required_string(artifact, "fileName"),
        _required_string(artifact, "url"),
        sha256,
        _required_positive_int(artifact, "size"),
    )


def load_toolchain_manifest(path: Path = DEFAULT_MANIFEST) -> GodotToolchain:
    """Load and validate the tracked Godot artifact lock."""

    try:
        document = json.loads(path.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError) as error:
        raise ValueError(f"cannot read Godot toolchain manifest: {error}") from error
    root = _required_mapping(document, "manifest")
    if root.get("schemaVersion") != 1:
        raise ValueError("unsupported Godot toolchain manifest schemaVersion")
    release_repository = _required_string(root, "releaseRepository")
    if release_repository != "https://github.com/godotengine/godot-builds":
        raise ValueError("Godot artifacts must come from the official godot-builds repository")
    if _required_string(root, "releaseTag") != "4.7.2-stable":
        raise ValueError("Godot releaseTag must remain 4.7.2-stable")

    templates = _required_mapping(root.get("exportTemplates"), "exportTemplates")
    members = templates.get("members")
    if not isinstance(members, list) or not members or not all(
        isinstance(member, str) and member.startswith("templates/") for member in members
    ):
        raise ValueError("exportTemplates.members must be a non-empty templates/ list")
    if len(set(members)) != len(members):
        raise ValueError("exportTemplates.members must not contain duplicates")

    timeouts = _required_mapping(root.get("timeoutsSeconds"), "timeoutsSeconds")
    return GodotToolchain(
        _required_string(root, "godotVersionOutput"),
        _required_string(root, "templateDirectory"),
        _required_string(root, "publicExportPreset"),
        _artifact(root, "editor"),
        _artifact(root, "exportTemplates"),
        tuple(members),
        GateTimeouts(
            _required_positive_int(timeouts, "version"),
            _required_positive_int(timeouts, "restore"),
            _required_positive_int(timeouts, "build"),
            _required_positive_int(timeouts, "import"),
            _required_positive_int(timeouts, "run"),
            _required_positive_int(timeouts, "export"),
            _required_positive_int(timeouts, "termination"),
            _required_positive_int(timeouts, "reap"),
        ),
    )


def _sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        while chunk := stream.read(1024 * 1024):
            digest.update(chunk)
    return digest.hexdigest()


def verify_artifact(path: Path, spec: ArtifactSpec) -> dict[str, object]:
    """Fail closed unless a local artifact exactly matches its tracked lock."""

    if not path.is_file():
        raise ValueError(f"required Godot artifact is unavailable: {spec.file_name}")
    size = path.stat().st_size
    if size != spec.size:
        raise ValueError(
            f"Godot artifact size mismatch for {spec.file_name}: expected {spec.size}, got {size}"
        )
    digest = _sha256(path)
    if digest != spec.sha256:
        raise ValueError(
            f"Godot artifact SHA-256 mismatch for {spec.file_name}: "
            f"expected {spec.sha256}, got {digest}"
        )
    return {
        "fileName": spec.file_name,
        "url": spec.url,
        "size": size,
        "sha256": digest,
        "status": "Pass",
    }


def _safe_member_path(member: str) -> PurePosixPath:
    normalized = PurePosixPath(member.replace("\\", "/"))
    if normalized.is_absolute() or ".." in normalized.parts or not normalized.parts:
        raise ValueError(f"unsafe archive member: {member}")
    return normalized


def extract_zip_members(
    archive: Path,
    destination: Path,
    *,
    members: Iterable[str] | None = None,
    strip_prefix: str | None = None,
) -> tuple[Path, ...]:
    """Extract exact safe ZIP members into a fresh caller-owned directory."""

    if destination.exists():
        raise ValueError(f"archive destination already exists: {destination}")
    requested = None if members is None else tuple(members)
    requested_set = None if requested is None else set(requested)
    extracted: list[Path] = []
    destination.mkdir(parents=True)
    with zipfile.ZipFile(archive) as source:
        names = set(source.namelist())
        if requested_set is not None:
            missing = sorted(requested_set - names)
            if missing:
                raise ValueError(f"archive is missing locked member(s): {', '.join(missing)}")
        for info in source.infolist():
            if requested_set is not None and info.filename not in requested_set:
                continue
            member = _safe_member_path(info.filename)
            if strip_prefix is not None:
                prefix = PurePosixPath(strip_prefix)
                if member.parts[: len(prefix.parts)] != prefix.parts:
                    raise ValueError(f"archive member does not use {strip_prefix}: {info.filename}")
                member = PurePosixPath(*member.parts[len(prefix.parts) :])
                if not member.parts:
                    continue
            target = destination.joinpath(*member.parts)
            if not target.resolve().is_relative_to(destination.resolve()):
                raise ValueError(f"archive member escapes destination: {info.filename}")
            if info.is_dir():
                target.mkdir(parents=True, exist_ok=True)
                continue
            target.parent.mkdir(parents=True, exist_ok=True)
            with source.open(info) as input_stream, target.open("wb") as output_stream:
                shutil.copyfileobj(input_stream, output_stream)
            extracted.append(target)
    return tuple(extracted)


def _bounded_tail(value: str) -> str:
    return value[-MAX_DIAGNOSTIC_CHARACTERS:]


def _terminate_process_tree(
    process: subprocess.Popen[str],
    *,
    termination_timeout: int,
    reap_timeout: int,
) -> str:
    if process.poll() is not None:
        return "clean"
    if os.name == "nt":
        with suppress(subprocess.TimeoutExpired):
            subprocess.run(
                ["taskkill", "/PID", str(process.pid), "/T", "/F"],
                check=False,
                capture_output=True,
                text=True,
                encoding="utf-8",
                errors="replace",
                timeout=termination_timeout,
            )
    else:
        process.terminate()
    try:
        process.wait(timeout=reap_timeout)
    except subprocess.TimeoutExpired:
        process.kill()
        try:
            process.wait(timeout=reap_timeout)
        except subprocess.TimeoutExpired:
            return "survivor"
    return "clean" if process.poll() is not None else "survivor"


def run_bounded_process(
    step: str,
    command: Iterable[str | Path],
    *,
    cwd: Path,
    environment: Mapping[str, str],
    timeout: int,
    termination_timeout: int,
    reap_timeout: int,
) -> ProcessReceipt:
    """Run one native command with finite timeout and process-tree cleanup."""

    arguments = tuple(str(item) for item in command)
    creationflags = getattr(subprocess, "CREATE_NEW_PROCESS_GROUP", 0) if os.name == "nt" else 0
    process = subprocess.Popen(
        arguments,
        cwd=cwd,
        env=dict(environment),
        stdin=subprocess.DEVNULL,
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
        text=True,
        encoding="utf-8",
        errors="replace",
        creationflags=creationflags,
        start_new_session=os.name != "nt",
    )
    timed_out = False
    cleanup_status = "clean"
    try:
        stdout, stderr = process.communicate(timeout=timeout)
    except subprocess.TimeoutExpired as error:
        timed_out = True
        cleanup_status = _terminate_process_tree(
            process,
            termination_timeout=termination_timeout,
            reap_timeout=reap_timeout,
        )
        stdout = error.stdout or ""
        stderr = error.stderr or ""
        try:
            remaining_stdout, remaining_stderr = process.communicate(timeout=reap_timeout)
            stdout += remaining_stdout or ""
            stderr += remaining_stderr or ""
        except subprocess.TimeoutExpired:
            cleanup_status = "survivor"
            if process.stdout is not None:
                process.stdout.close()
            if process.stderr is not None:
                process.stderr.close()
    return ProcessReceipt(
        step,
        arguments,
        process.poll(),
        timed_out,
        cleanup_status,
        _bounded_tail(stdout),
        _bounded_tail(stderr),
    )


def _tracked_remake_files() -> tuple[Path, ...]:
    completed = subprocess.run(
        ["git", "ls-files", "-z", "--", "remake"],
        cwd=REPO_ROOT,
        check=True,
        capture_output=True,
    )
    return tuple(
        REPO_ROOT / Path(value.decode("utf-8"))
        for value in completed.stdout.split(b"\0")
        if value
    )


def _copy_tracked_remake(destination_root: Path) -> Path:
    workspace = destination_root / "workspace"
    if workspace.exists():
        raise ValueError(f"workspace destination already exists: {workspace}")
    workspace.mkdir(parents=True)
    tracked_files = _tracked_remake_files()
    if not tracked_files:
        raise ValueError("tracked remake project is empty")
    for source in tracked_files:
        relative = source.relative_to(REPO_ROOT)
        target = workspace / relative
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(source, target)
    return workspace


def _validate_scratch_parent(path: Path) -> Path:
    resolved = path.resolve()
    if resolved == REPO_ROOT or resolved == Path(resolved.anchor):
        raise ValueError("scratch parent cannot be a repository or filesystem root")
    if resolved.is_relative_to(REPO_ROOT) and not resolved.is_relative_to(repo_path("local")):
        raise ValueError("repository-local Godot scratch must stay under ignored local/")
    resolved.mkdir(parents=True, exist_ok=True)
    return resolved


def _new_scratch(scratch_parent: Path) -> Path:
    parent = _validate_scratch_parent(scratch_parent)
    scratch = parent / f"public-synthetic-{uuid.uuid4().hex}"
    scratch.mkdir()
    return scratch


def _find_editor(editor_root: Path) -> Path:
    matches = sorted(editor_root.rglob("Godot_v4.7.2-stable_mono_win64.exe"))
    if len(matches) != 1:
        raise ValueError("editor archive must contain exactly one locked Windows .NET executable")
    return matches[0]


def _process_snapshot() -> set[tuple[int, str]]:
    if os.name != "nt":
        return set()
    completed = subprocess.run(
        ["tasklist", "/FO", "CSV", "/NH"],
        check=True,
        capture_output=True,
        text=True,
        encoding="utf-8",
        errors="replace",
        timeout=15,
    )
    relevant = {
        "dotnet.exe",
        "godot_v4.7.2-stable_mono_win64.exe",
        "sf2-map3-public-synthetic.exe",
    }
    result: set[tuple[int, str]] = set()
    for row in csv.reader(completed.stdout.splitlines()):
        if len(row) < 2 or row[0].lower() not in relevant:
            continue
        try:
            result.add((int(row[1]), row[0].lower()))
        except ValueError:
            continue
    return result


def _cleanup_new_processes(
    before: set[tuple[int, str]],
    *,
    termination_timeout: int,
) -> tuple[str, list[tuple[int, str]]]:
    """Terminate only tracked Godot/dotnet processes created by this gate."""

    survivors = sorted(_process_snapshot() - before)
    if not survivors or os.name != "nt":
        return "clean", survivors
    for process_id, _ in survivors:
        try:
            subprocess.run(
                ["taskkill", "/PID", str(process_id), "/T", "/F"],
                check=False,
                capture_output=True,
                text=True,
                encoding="utf-8",
                errors="replace",
                timeout=termination_timeout,
            )
        except subprocess.TimeoutExpired:
            continue
    remaining = sorted(_process_snapshot() - before)
    return ("clean" if not remaining else "survivor"), remaining


def _write_receipt(path: Path, receipt: Mapping[str, object]) -> None:
    path.write_text(
        json.dumps(receipt, ensure_ascii=False, indent=2) + "\n",
        encoding="utf-8",
    )


def _require_passed(result: ProcessReceipt) -> None:
    if not result.passed:
        raise RuntimeError(
            f"Godot gate step {result.step} failed: exit={result.exit_code}, "
            f"timedOut={result.timed_out}, cleanup={result.cleanup_status}"
        )


def _scan_export(export_root: Path) -> dict[str, object]:
    files = sorted(path for path in export_root.rglob("*") if path.is_file())
    if not files:
        raise RuntimeError("Godot export produced no files")
    forbidden_suffixes = {".bin", ".rom", ".srm", ".sav", ".state", ".trace"}
    required_names = {
        "sf2-map3-public-synthetic.exe",
        "sf2-map3-public-synthetic.pck",
        "Sf2.Remake.Application.dll",
        "Sf2.Remake.Content.dll",
        "Sf2.Remake.Domain.dll",
        "Sf2.Remake.Godot.dll",
    }
    required_rows: dict[str, dict[str, object]] = {}
    manifest_digest = hashlib.sha256()
    total_size = 0
    for path in files:
        relative = path.relative_to(export_root).as_posix()
        lowered = relative.lower()
        if path.suffix.lower() in forbidden_suffixes or "local/" in lowered:
            raise RuntimeError(
                f"public-synthetic export contains a forbidden payload path: {relative}"
            )
        size = path.stat().st_size
        digest = _sha256(path)
        total_size += size
        manifest_digest.update(f"{relative}\0{size}\0{digest}\n".encode())
        if path.name in required_names:
            if path.name in required_rows:
                raise RuntimeError(f"Godot export contains duplicate required output: {path.name}")
            required_rows[path.name] = {
                "path": relative,
                "size": size,
                "sha256": digest,
            }
    missing = sorted(required_names - required_rows.keys())
    if missing:
        raise RuntimeError(f"Godot export is missing required output(s): {', '.join(missing)}")
    return {
        "fileCount": len(files),
        "totalSize": total_size,
        "manifestSha256": manifest_digest.hexdigest(),
        "requiredFiles": [required_rows[name] for name in sorted(required_rows)],
    }


def _parse_smoke_receipt(output: str) -> dict[str, object]:
    lines = [line for line in output.splitlines() if SMOKE_MARKER in line]
    if len(lines) != 1:
        raise RuntimeError("Godot smoke must emit exactly one typed SF2_MAP3_SMOKE receipt")
    payload = lines[0].split(SMOKE_MARKER, 1)[1]
    try:
        receipt = json.loads(payload)
    except json.JSONDecodeError as error:
        raise RuntimeError("Godot smoke receipt is not valid JSON") from error
    if not isinstance(receipt, dict):
        raise RuntimeError("Godot smoke receipt must be a JSON object")
    expected = {
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
    if receipt != expected:
        raise RuntimeError(
            "Godot smoke receipt does not match the closed public-synthetic contract"
        )
    return receipt


def _require_clean_export_output(result: ProcessReceipt) -> None:
    combined = result.stdout_tail + "\n" + result.stderr_tail
    if "ERROR:" in combined or "completed with warnings" in combined.lower():
        raise RuntimeError("Godot export reported an error or warning despite a zero exit code")


def verify_remake_godot(
    *,
    toolchain_root: Path = DEFAULT_TOOLCHAIN_ROOT,
    project_path: Path = DEFAULT_PROJECT,
    scratch_parent: Path = DEFAULT_SCRATCH_PARENT,
    manifest_path: Path = DEFAULT_MANIFEST,
    process_runner: Callable[..., ProcessReceipt] = run_bounded_process,
) -> dict[str, object]:
    """Run import, smoke, and export against only tracked public-synthetic inputs."""

    toolchain = load_toolchain_manifest(manifest_path)
    editor_archive = toolchain_root / toolchain.editor.file_name
    template_archive = toolchain_root / toolchain.export_templates.file_name
    artifacts = [
        verify_artifact(editor_archive, toolchain.editor),
        verify_artifact(template_archive, toolchain.export_templates),
    ]
    resolved_project = project_path.resolve()
    if not resolved_project.is_relative_to(REPO_ROOT) or not resolved_project.is_dir():
        raise ValueError("Godot project must be an existing path inside this repository")
    project_relative = resolved_project.relative_to(REPO_ROOT)
    if project_relative.as_posix() != "remake/game":
        raise ValueError("the maintained Godot gate owns exactly remake/game")

    scratch = _new_scratch(scratch_parent)
    receipt_path = scratch / "receipt.json"
    steps: list[ProcessReceipt] = []
    receipt: dict[str, object] = {
        "schemaVersion": 1,
        "profile": "public-synthetic",
        "status": "Fail",
        "godotVersion": toolchain.version_output,
        "artifacts": artifacts,
        "steps": [],
        "exportFiles": {},
        "cleanupStatus": "NotRun",
        "scratch": display_path(scratch),
    }
    before_processes: set[tuple[int, str]] | None = None
    try:
        editor_root = scratch / "editor"
        extract_zip_members(editor_archive, editor_root)
        editor = _find_editor(editor_root)

        template_root = (
            scratch
            / "appdata"
            / "Godot"
            / "export_templates"
            / toolchain.template_directory
        )
        extracted_templates = extract_zip_members(
            template_archive,
            template_root,
            members=toolchain.export_template_members,
            strip_prefix="templates",
        )
        version_file = template_root / "version.txt"
        if version_file.read_text(encoding="utf-8").strip() != toolchain.template_directory:
            raise RuntimeError("export template version.txt does not match the locked directory")
        if len(extracted_templates) != len(toolchain.export_template_members):
            raise RuntimeError("export template extraction did not close the locked member set")

        workspace = _copy_tracked_remake(scratch)
        scratch_project = workspace / project_relative
        export_root = scratch / "export"
        export_root.mkdir()
        export_path = export_root / "sf2-map3-public-synthetic.exe"
        environment = dict(os.environ)
        environment.update(
            {
                "APPDATA": str(scratch / "appdata"),
                "LOCALAPPDATA": str(scratch / "localappdata"),
                "DOTNET_CLI_HOME": str(scratch / "dotnet-home"),
                "DOTNET_CLI_TELEMETRY_OPTOUT": "1",
                "DOTNET_CLI_UI_LANGUAGE": "en-US",
                "DOTNET_NOLOGO": "1",
                "GODOT_SILENCE_ROOT_WARNING": "1",
            }
        )
        before_processes = _process_snapshot()

        def run(step: str, arguments: Iterable[str | Path], timeout: int) -> ProcessReceipt:
            result = process_runner(
                step,
                arguments,
                cwd=scratch_project,
                environment=environment,
                timeout=timeout,
                termination_timeout=toolchain.timeouts.termination,
                reap_timeout=toolchain.timeouts.reap,
            )
            steps.append(result)
            _require_passed(result)
            return result

        version = run("version", [editor, "--version"], toolchain.timeouts.version)
        if version.stdout_tail.strip() != toolchain.version_output:
            raise RuntimeError(
                "Godot version mismatch after extraction: "
                f"expected {toolchain.version_output}, got {version.stdout_tail.strip()}"
            )
        solution = workspace / "remake" / "Sf2.Remake.sln"
        run(
            "restore",
            ["dotnet", "restore", solution, "--locked-mode"],
            toolchain.timeouts.restore,
        )
        run(
            "build",
            [
                "dotnet",
                "build",
                solution,
                "--configuration",
                "Debug",
                "--no-restore",
            ],
            toolchain.timeouts.build,
        )
        run(
            "import",
            [editor, "--headless", "--path", scratch_project, "--editor", "--quit-after", "1"],
            toolchain.timeouts.import_project,
        )
        smoke = run(
            "run",
            [editor, "--headless", "--path", scratch_project, "--", "--map3-smoke"],
            toolchain.timeouts.run,
        )
        smoke_receipt = _parse_smoke_receipt(smoke.stdout_tail)
        export_result = run(
            "export",
            [
                editor,
                "--headless",
                "--path",
                scratch_project,
                "--export-release",
                toolchain.public_export_preset,
                export_path,
            ],
            toolchain.timeouts.export,
        )
        _require_clean_export_output(export_result)
        export_files = _scan_export(export_root)
        exported_smoke = run(
            "export-run",
            [export_path, "--headless", "--", "--map3-smoke"],
            toolchain.timeouts.run,
        )
        export_smoke_receipt = _parse_smoke_receipt(exported_smoke.stdout_tail)
        cleanup_status, survivors = _cleanup_new_processes(
            before_processes,
            termination_timeout=toolchain.timeouts.termination,
        )
        if survivors:
            raise RuntimeError(f"Godot gate left process survivors: {survivors}")
        receipt.update(
            {
                "status": "Pass",
                "steps": [step.as_dict() for step in steps],
                "smokeReceipt": smoke_receipt,
                "exportSmokeReceipt": export_smoke_receipt,
                "exportFiles": export_files,
                "cleanupStatus": cleanup_status,
            }
        )
        _write_receipt(receipt_path, receipt)
        return receipt
    except Exception:
        cleanup_status = (
            "clean"
            if before_processes is None
            else _cleanup_new_processes(
                before_processes,
                termination_timeout=toolchain.timeouts.termination,
            )[0]
        )
        receipt.update(
            {
                "steps": [step.as_dict() for step in steps],
                "cleanupStatus": cleanup_status,
            }
        )
        _write_receipt(receipt_path, receipt)
        raise


def main(argv: list[str] | None = None) -> int:
    """Parse the dedicated non-evidence gate command line."""

    parser = argparse.ArgumentParser(
        description="Verify the public-synthetic remake through official Godot CLI gates."
    )
    parser.add_argument(
        "--toolchain-root",
        type=Path,
        default=DEFAULT_TOOLCHAIN_ROOT,
        help="directory containing the two hash-locked official Godot archives",
    )
    parser.add_argument(
        "--project-path",
        type=Path,
        default=DEFAULT_PROJECT,
        help="tracked Godot project (maintained value: remake/game)",
    )
    parser.add_argument(
        "--scratch-parent",
        type=Path,
        default=DEFAULT_SCRATCH_PARENT,
        help="ignored or ephemeral parent for a fresh gate output directory",
    )
    parser.add_argument(
        "--manifest-path",
        type=Path,
        default=DEFAULT_MANIFEST,
        help="tracked Godot toolchain identity manifest",
    )
    arguments = parser.parse_args(argv)
    receipt = verify_remake_godot(
        toolchain_root=arguments.toolchain_root,
        project_path=arguments.project_path,
        scratch_parent=arguments.scratch_parent,
        manifest_path=arguments.manifest_path,
    )
    print(json.dumps(receipt, ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
