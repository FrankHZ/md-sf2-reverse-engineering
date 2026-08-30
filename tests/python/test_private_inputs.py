from __future__ import annotations

from pathlib import Path

import pytest

import sf2tool.private_inputs as private_inputs
from sf2tool.cli import build_parser, validate_verify_plan_args
from sf2tool.paths import repo_path
from sf2tool.private_inputs import (
    BIZHAWK_ARCHIVE_INPUT_IDENTITY,
    JDK_INPUT_IDENTITY,
    ROM_INPUT_IDENTITY,
    SHARED_INPUT_ROOT_ENV,
    private_input_path,
)


def _shared_rom(root: Path) -> Path:
    rom = root / ROM_INPUT_IDENTITY
    rom.parent.mkdir(parents=True)
    rom.write_bytes(b"synthetic-test-input")
    return rom


def _shared_jdk(root: Path) -> Path:
    jdk = root / JDK_INPUT_IDENTITY
    jdk.mkdir(parents=True)
    return jdk


def _shared_bizhawk_archive(root: Path) -> Path:
    archive = root / BIZHAWK_ARCHIVE_INPUT_IDENTITY
    archive.parent.mkdir(parents=True)
    archive.write_bytes(b"synthetic-bizhawk-archive")
    return archive


def test_unset_root_keeps_the_repo_local_fallback() -> None:
    assert private_input_path(ROM_INPUT_IDENTITY, environment={}) == repo_path(
        "local/roms/sf2-us.bin"
    )
    assert private_input_path(JDK_INPUT_IDENTITY, environment={}) == repo_path(
        "local/toolchains/jdk-17.0.19+10"
    )
    assert private_input_path(BIZHAWK_ARCHIVE_INPUT_IDENTITY, environment={}) == repo_path(
        "local/toolchains/BizHawk-2.11.1-win-x64.zip"
    )


def test_configured_root_resolves_the_registered_rom_without_reading_it(tmp_path: Path) -> None:
    root = tmp_path / "shared"
    expected = _shared_rom(root).resolve()

    assert (
        private_input_path(
            ROM_INPUT_IDENTITY,
            environment={SHARED_INPUT_ROOT_ENV: str(root.resolve())},
        )
        == expected
    )


def test_configured_root_resolves_the_registered_jdk_directory(tmp_path: Path) -> None:
    root = tmp_path / "shared"
    expected = _shared_jdk(root).resolve()

    assert (
        private_input_path(
            JDK_INPUT_IDENTITY,
            environment={SHARED_INPUT_ROOT_ENV: str(root.resolve())},
        )
        == expected
    )


def test_configured_root_resolves_the_registered_bizhawk_archive(tmp_path: Path) -> None:
    root = tmp_path / "shared"
    expected = _shared_bizhawk_archive(root).resolve()

    assert (
        private_input_path(
            BIZHAWK_ARCHIVE_INPUT_IDENTITY,
            environment={SHARED_INPUT_ROOT_ENV: str(root.resolve())},
        )
        == expected
    )


@pytest.mark.parametrize(
    "identity",
    (
        "",
        ".",
        "../roms/sf2-us.bin",
        "roms/../sf2-us.bin",
        "/roms/sf2-us.bin",
        r"\roms\sf2-us.bin",
        r"C:roms\sf2-us.bin",
        r"C:\roms\sf2-us.bin",
    ),
)
def test_invalid_input_identities_are_rejected(identity: str) -> None:
    with pytest.raises(ValueError):
        private_input_path(identity, environment={})


def test_unregistered_relative_identity_is_rejected() -> None:
    with pytest.raises(ValueError, match="not registered"):
        private_input_path("archives/tool.zip", environment={})


@pytest.mark.parametrize("configured", ("", "relative/root", r"C:relative-root"))
def test_shared_root_must_be_nonempty_and_absolute(configured: str) -> None:
    with pytest.raises(ValueError):
        private_input_path(
            ROM_INPUT_IDENTITY,
            environment={SHARED_INPUT_ROOT_ENV: configured},
        )


def test_missing_shared_input_is_not_created(tmp_path: Path) -> None:
    root = tmp_path / "shared"
    root.mkdir()
    before = tuple(root.rglob("*"))

    with pytest.raises(FileNotFoundError):
        private_input_path(
            ROM_INPUT_IDENTITY,
            environment={SHARED_INPUT_ROOT_ENV: str(root.resolve())},
        )

    assert tuple(root.rglob("*")) == before


def test_missing_shared_jdk_directory_is_not_created(tmp_path: Path) -> None:
    root = tmp_path / "shared"
    root.mkdir()

    with pytest.raises(FileNotFoundError):
        private_input_path(
            JDK_INPUT_IDENTITY,
            environment={SHARED_INPUT_ROOT_ENV: str(root.resolve())},
        )

    assert tuple(root.rglob("*")) == ()


def test_missing_shared_bizhawk_archive_is_not_created(tmp_path: Path) -> None:
    root = tmp_path / "shared"
    root.mkdir()

    with pytest.raises(FileNotFoundError):
        private_input_path(
            BIZHAWK_ARCHIVE_INPUT_IDENTITY,
            environment={SHARED_INPUT_ROOT_ENV: str(root.resolve())},
        )

    assert tuple(root.rglob("*")) == ()


def test_registered_input_types_are_enforced(tmp_path: Path) -> None:
    file_root = tmp_path / "file-root"
    (file_root / ROM_INPUT_IDENTITY).mkdir(parents=True)
    with pytest.raises(ValueError, match="must be a file"):
        private_input_path(
            ROM_INPUT_IDENTITY,
            environment={SHARED_INPUT_ROOT_ENV: str(file_root.resolve())},
        )

    directory_root = tmp_path / "directory-root"
    jdk = directory_root / JDK_INPUT_IDENTITY
    jdk.parent.mkdir(parents=True)
    jdk.write_bytes(b"synthetic-not-a-directory")
    with pytest.raises(ValueError, match="must be a directory"):
        private_input_path(
            JDK_INPUT_IDENTITY,
            environment={SHARED_INPUT_ROOT_ENV: str(directory_root.resolve())},
        )

    archive_root = tmp_path / "archive-root"
    (archive_root / BIZHAWK_ARCHIVE_INPUT_IDENTITY).mkdir(parents=True)
    with pytest.raises(ValueError, match="must be a file"):
        private_input_path(
            BIZHAWK_ARCHIVE_INPUT_IDENTITY,
            environment={SHARED_INPUT_ROOT_ENV: str(archive_root.resolve())},
        )


def test_post_resolution_reparse_escape_is_rejected(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    root = tmp_path / "shared"
    expected = _shared_rom(root)
    outside = tmp_path / "outside" / "sf2-us.bin"
    outside.parent.mkdir()
    outside.write_bytes(b"synthetic-outside-input")
    original_resolve = private_inputs._resolve_existing

    def resolve_with_escape(path: Path) -> Path:
        if path == expected:
            return outside.resolve()
        return original_resolve(path)

    monkeypatch.setattr(private_inputs, "_resolve_existing", resolve_with_escape)
    with pytest.raises(ValueError, match="resolves outside"):
        private_input_path(
            ROM_INPUT_IDENTITY,
            environment={SHARED_INPUT_ROOT_ENV: str(root.resolve())},
        )


def test_bizhawk_archive_post_resolution_reparse_escape_is_rejected(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    root = tmp_path / "shared"
    expected = _shared_bizhawk_archive(root)
    outside = tmp_path / "outside" / expected.name
    outside.parent.mkdir()
    outside.write_bytes(b"synthetic-outside-archive")
    original_resolve = private_inputs._resolve_existing

    def resolve_with_escape(path: Path) -> Path:
        if path == expected:
            return outside.resolve()
        return original_resolve(path)

    monkeypatch.setattr(private_inputs, "_resolve_existing", resolve_with_escape)
    with pytest.raises(ValueError, match="resolves outside"):
        private_input_path(
            BIZHAWK_ARCHIVE_INPUT_IDENTITY,
            environment={SHARED_INPUT_ROOT_ENV: str(root.resolve())},
        )


def test_parser_reads_environment_per_build_and_freezes_each_default(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    monkeypatch.delenv(SHARED_INPUT_ROOT_ENV, raising=False)
    fallback_parser = build_parser()
    fallback = fallback_parser.parse_args(["verify"])

    root = tmp_path / "shared"
    shared_rom = _shared_rom(root).resolve()
    monkeypatch.setenv(SHARED_INPUT_ROOT_ENV, str(root.resolve()))
    shared_parser = build_parser()
    shared = shared_parser.parse_args(["verify"])

    monkeypatch.delenv(SHARED_INPUT_ROOT_ENV)
    assert fallback.rom_path == repo_path("local/roms/sf2-us.bin")
    assert fallback_parser.parse_args(["verify"]).rom_path == fallback.rom_path
    assert shared.rom_path == shared_rom
    assert shared_parser.parse_args(["verify"]).rom_path == shared_rom
    assert build_parser().parse_args(["verify"]).rom_path == fallback.rom_path


def test_explicit_rom_path_overrides_the_shared_parser_default(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    root = tmp_path / "shared"
    _shared_rom(root)
    explicit = tmp_path / "explicit.bin"
    monkeypatch.setenv(SHARED_INPUT_ROOT_ENV, str(root.resolve()))

    args = build_parser().parse_args(["verify", "--rom-path", str(explicit)])

    assert args.rom_path == explicit


def test_verify_plan_accepts_its_frozen_shared_default(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    root = tmp_path / "shared"
    shared_rom = _shared_rom(root).resolve()
    monkeypatch.setenv(SHARED_INPUT_ROOT_ENV, str(root.resolve()))

    args = build_parser().parse_args(["verify", "plan", "--base", "origin/main"])

    assert args.rom_path == shared_rom
    validate_verify_plan_args(args)
