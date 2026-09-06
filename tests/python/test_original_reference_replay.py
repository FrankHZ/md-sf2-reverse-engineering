from __future__ import annotations

import io
import json
import zipfile
from copy import deepcopy
from ctypes import CDLL, POINTER, byref, c_char_p, c_int, c_size_t, c_ssize_t, c_void_p, string_at
from pathlib import Path
from typing import Any

import pytest

import sf2tool.h3.original_reference_replay as replay
from sf2tool.h3 import bizhawk
from sf2tool.h3.original_reference_replay import (
    CAPABILITY_SCHEMA,
    CapabilityError,
    _candidate_identity,
    _canonical_json,
    _config_template,
    _input_rows,
    _load_observer_status,
    _sha256,
    _validate_archive_members,
    load_capability_fixture,
    materialize_movie,
    run_original_reference_replay,
    validate_materialized_movie,
    validate_passive_observer,
)
from sf2tool.jsonio import validate_json


def _archive_with_member_change(data: bytes, member: str, replacement: bytes) -> bytes:
    target = io.BytesIO()
    with (
        zipfile.ZipFile(io.BytesIO(data), "r") as source,
        zipfile.ZipFile(target, "w", compression=zipfile.ZIP_STORED) as output,
    ):
        for info in source.infolist():
            copied = zipfile.ZipInfo(info.filename, date_time=info.date_time)
            copied.compress_type = info.compress_type
            output.writestr(
                copied,
                replacement if info.filename == member else source.read(info.filename),
            )
    return target.getvalue()


def test_materializer_keeps_warmup_outside_unchanged_semantic_rows() -> None:
    fixture = load_capability_fixture()
    movie = materialize_movie(fixture)
    recipe = fixture["movieRecipe"]

    assert movie.recipe_sha256 == "BAD3DE219DBDA1F2FB9A2DA7191351290575CAFA4FF03A00B75A289ADA2BD867"
    assert movie.bk2_sha256 == "250F4086E1C1AD08BF64A7CB5C84787E4EE8DA41D83D53630F54DE7FB8085E64"
    assert recipe["warmUpRow"] == {"bk2Row": 0, "port1": []}
    assert recipe["semanticRowOffset"] == 1
    rows = _input_rows(recipe)
    assert len(rows) == 34  # input-log key plus physical BK2 rows 0..32
    assert rows[1] == "|.|........|"
    assert rows[2:10] == ("|.|........|",) * 8
    assert rows[10:14] == ("|.|...R....|",) * 4
    assert _sha256(("\n".join(rows) + "\n").encode()) == recipe["inputLogSha256"]

    for member in ("Header", "Input Log", "SyncSettings"):
        with pytest.raises(CapabilityError, match=member):
            validate_materialized_movie(
                _archive_with_member_change(movie.data, member, b"drift\n"), fixture
            )


def test_capability_schema_recursively_closes_warmup_and_config_projection() -> None:
    fixture = load_capability_fixture()
    assert json.loads(_canonical_json(fixture)) == fixture
    assert _config_template(fixture["launchContract"]["configProjection"])["Movies"] == {
        "MovieEndAction": 3,
        "EnableBackupMovies": False,
        "MoviesOnDisk": False,
        "PlaySoundOnMovieEnd": False,
    }

    for mutate in (
        lambda value: value["movieRecipe"]["warmUpRow"].update(port1=["A"]),
        lambda value: value["movieRecipe"]["rows"][8].update(port1=["Left"]),
        lambda value: value["launchContract"]["configProjection"]["settings"].update(
            SoundEnabled=True
        ),
        lambda value: value["launchContract"]["configProjection"]["paths"][0].update(
            path="./ROM"
        ),
        lambda value: value["launchContract"]["configProjection"]["paths"].reverse(),
        lambda value: value["launchContract"]["configProjection"]["paths"][0].update(extra=True),
        lambda value: value["receiptContract"]["globalOrdinalOne"].update(receiptSha256="0" * 64),
    ):
        changed = deepcopy(fixture)
        mutate(changed)
        with pytest.raises(ValueError):
            validate_json(changed, CAPABILITY_SCHEMA, owner="original-reference contract mutation")


def _zip_info(name: str) -> zipfile.ZipInfo:
    return zipfile.ZipInfo(name)


@pytest.mark.parametrize(
    "names, message",
    [
        (["../escape"], "traverses containment"),
        (["C:/escape"], "drive/ADS"),
        (["member:stream"], "drive/ADS"),
        (["config.ini"], "forbidden mutable"),
        (["one", "ONE"], "case-collision"),
        (["same", "same"], "duplicate"),
    ],
)
def test_archive_member_validator_rejects_path_and_collision_near_misses(
    monkeypatch: pytest.MonkeyPatch, names: list[str], message: str
) -> None:
    infos = [_zip_info(name) for name in names]
    monkeypatch.setattr(replay, "_ARCHIVE_MEMBER_COUNT", len(infos))
    monkeypatch.setattr(
        replay,
        "_ARCHIVE_MEMBER_SET_SHA256",
        _sha256(_canonical_json(sorted(name.filename for name in infos))),
    )
    with pytest.raises(CapabilityError, match=message):
        _validate_archive_members(infos)


def test_archive_member_validator_rejects_reparse_member(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    info = _zip_info("regular")
    info.external_attr = 0o120777 << 16
    monkeypatch.setattr(replay, "_ARCHIVE_MEMBER_COUNT", 1)
    monkeypatch.setattr(replay, "_ARCHIVE_MEMBER_SET_SHA256", _sha256(_canonical_json(["regular"])))
    with pytest.raises(CapabilityError, match="reparse/symlink"):
        _validate_archive_members([info])


def test_archive_member_validator_rejects_windows_reparse_attribute(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    info = _zip_info("regular")
    info.external_attr = 0x400
    monkeypatch.setattr(replay, "_ARCHIVE_MEMBER_COUNT", 1)
    monkeypatch.setattr(replay, "_ARCHIVE_MEMBER_SET_SHA256", _sha256(_canonical_json(["regular"])))
    with pytest.raises(CapabilityError, match="reparse/symlink"):
        _validate_archive_members([info])


def test_passive_observer_denies_input_and_dynamic_aliases(tmp_path: Path) -> None:
    fixture = load_capability_fixture()
    for source, message in (
        ("joypad.set(1, {})", "joypad.set"),
        ("local setter = movie.stop\nsetter()", "movie.stop"),
        ('movie["stop"]()', "dynamic member access"),
        ("local api = movie\napi.stop()", "aliases API namespace"),
        ("unsafe.launch()", "unallowed API: unsafe.launch"),
        ("unsafe()", "unallowed Lua call: unsafe"),
    ):
        candidate = tmp_path / "observer.lua"
        candidate.write_text(source + "\n", encoding="utf-8")
        changed = deepcopy(fixture)
        changed["passiveObserverPolicy"]["observerSha256"] = _sha256(
            replay._canonical_observer_transport_bytes(candidate)
        )
        with pytest.raises(CapabilityError, match=message):
            validate_passive_observer(changed, candidate)


def test_observer_transport_canonicalizes_crlf_but_rejects_other_byte_drift(tmp_path: Path) -> None:
    fixture = load_capability_fixture()
    canonical = replay._canonical_observer_transport_bytes(replay.OBSERVER_PATH)
    assert _sha256(canonical) == fixture["passiveObserverPolicy"]["observerSha256"]

    lf_checkout = tmp_path / "lf-checkout.lua"
    crlf_checkout = tmp_path / "crlf-checkout.lua"
    lf_checkout.write_bytes(canonical)
    crlf_checkout.write_bytes(canonical.replace(b"\n", b"\r\n"))
    lf_launch = tmp_path / "lf-launch.lua"
    crlf_launch = tmp_path / "crlf-launch.lua"

    assert validate_passive_observer(fixture, lf_checkout) == _sha256(canonical)
    assert validate_passive_observer(fixture, crlf_checkout) == _sha256(canonical)
    assert (
        replay._materialize_passive_observer(fixture, lf_checkout, lf_launch)
        == _sha256(canonical)
    )
    assert (
        replay._materialize_passive_observer(fixture, crlf_checkout, crlf_launch)
        == _sha256(canonical)
    )
    assert lf_launch.read_bytes() == crlf_launch.read_bytes() == canonical

    non_line_ending_drift = tmp_path / "byte-drift.lua"
    non_line_ending_drift.write_bytes(canonical.replace(b"local", b"LOCAL", 1))
    with pytest.raises(CapabilityError, match="hash drift"):
        validate_passive_observer(fixture, non_line_ending_drift)

    lone_carriage_return = tmp_path / "lone-carriage-return.lua"
    lone_carriage_return.write_bytes(canonical.replace(b"\n", b"\r", 1))
    with pytest.raises(CapabilityError, match="non-CRLF carriage return"):
        validate_passive_observer(fixture, lone_carriage_return)


def _identity(character: str) -> dict[str, object]:
    return {"sha256": character * 64, "sizeBytes": 1}


def _facts(tmp_path: Path, rom: Path) -> dict[str, object]:
    archive = tmp_path / "archive.zip"
    if not archive.exists():
        archive.write_bytes(b"synthetic archive")
    return {
        "romSha256": "9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9",
        "romIdentity": _identity("1"),
        "archive": replay._file_identity(archive),
        "archiveMemberSetSha256": "3" * 64,
        "archivePath": str(archive),
        "hostToolchainRoot": str(tmp_path / "host"),
        "runner": _identity("4"),
        "helper": _identity("5"),
        "fixtureSha256": "6" * 64,
        "observerSha256": _sha256(replay._canonical_observer_transport_bytes()),
    }


def _passing_status(fixture: dict[str, object]) -> dict[str, object]:
    recipe = fixture["movieRecipe"]
    assert isinstance(recipe, dict)
    return {
        "status": "PASS",
        "callbacksRemaining": 0,
        "moviePosition": 32,
        "inputPollTrace": [
            {"semanticIndex": index, "bk2Row": index + 1, "emuFrame": index + 1, "input": value}
            for index, value in enumerate(_input_rows(recipe)[2:])
        ],
        "initialFrame": 1,
        "firstPollFrame": 1,
        "terminalFrame": 33,
        "movieMode": "FINISHED",
        "readOnly": True,
        "powerOn": True,
        "headerPlatform": "GEN",
        "headerCore": "Genesis Plus GX",
        "clientVersion": "2.11.1",
        "statusWriteOk": True,
    }


def _prepare_fake_launch(launch: Path) -> dict[str, Path]:
    toolchain = launch / "toolchain"
    executable = toolchain / "EmuHawk.exe"
    config = toolchain / "config.ini"
    movie = toolchain / "Movies" / "replay.bk2"
    observer = toolchain / "Lua" / "original_reference_replay_observer.lua"
    rom = toolchain / "ROM" / "sf2.bin"
    for path in (executable, config, movie, observer, rom, toolchain / "dll" / "lua54.dll"):
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_bytes(b"contained")
    return {
        "toolchain": toolchain,
        "executable": executable,
        "config": config,
        "movie": movie,
        "observer": observer,
        "rom": rom,
    }


def _install_simulated_runner(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path, statuses: list[dict[str, object] | None]
) -> tuple[dict[str, object], object, Path, list[list[str]]]:
    fixture = deepcopy(load_capability_fixture())
    contained_identity = _sha256(b"contained")
    fixture["toolchainContract"].update(
        {
            "executableSha256": contained_identity,
            "executableSizeBytes": len(b"contained"),
            "lua54Sha256": contained_identity,
            "lua54SizeBytes": len(b"contained"),
        }
    )
    movie = materialize_movie(fixture)
    rom = tmp_path / "source-rom.bin"
    rom.write_bytes(b"rom")
    host = tmp_path / "host"
    host.mkdir()
    facts = _facts(tmp_path, rom)
    facts["hostToolchainRoot"] = str(host)
    facts["romIdentity"] = replay._file_identity(rom)
    commands: list[list[str]] = []
    monkeypatch.setattr(replay, "DERIVED_ROOT", tmp_path / "derived")
    monkeypatch.setattr(replay, "load_capability_fixture", lambda *_args: fixture)
    monkeypatch.setattr(replay, "_preflight", lambda *_: (facts, movie))
    monkeypatch.setattr(
        replay,
        "_prepare_contained_launch",
        lambda *_args: _prepare_fake_launch(_args[3]),
    )

    def fake_native(**kwargs: object) -> bizhawk.NativeProcessResult:
        commands.append(kwargs["command"])
        callback = kwargs["on_started"]
        assert callable(callback)
        callback(77)
        status = statuses.pop(0)
        if status is not None:
            environment = kwargs["environment"]
            assert isinstance(environment, dict)
            Path(environment["SF2_ORIGINAL_REFERENCE_STATUS"]).write_text(
                json.dumps(status), encoding="utf-8"
            )
        return bizhawk.NativeProcessResult(0, "stdout", "stderr", False, pid=77)

    monkeypatch.setattr(replay, "run_native_bizhawk_process", fake_native)
    return fixture, movie, rom, commands


def _install_synthetic_global_ordinal_one(
    monkeypatch: pytest.MonkeyPatch, fixture: dict[str, Any]
) -> dict[str, object]:
    candidate = "A" * 64
    receipt_path = replay.DERIVED_ROOT / candidate / "launch-1" / "receipt.json"
    receipt_path.parent.mkdir(parents=True)
    receipt_path.write_bytes(b"synthetic immutable ordinal one")
    receipt_sha256 = _sha256(receipt_path.read_bytes())
    monkeypatch.setattr(replay, "_GLOBAL_ORDINAL_ONE_CANDIDATE", candidate)
    monkeypatch.setattr(replay, "_GLOBAL_ORDINAL_ONE_RECEIPT_SHA256", receipt_sha256)
    fixture["receiptContract"]["globalOrdinalOne"] = {
        "ordinal": 1,
        "candidateSha256": candidate,
        "receiptSha256": receipt_sha256,
    }
    row: dict[str, object] = {
        "ordinal": 1,
        "candidateSha256": candidate,
        "runClass": "diagnostic",
        "receiptPath": str(receipt_path),
        "receiptSha256": receipt_sha256,
        "status": "FAIL",
        "processStarted": True,
        "pid": 1,
    }
    replay._write_ledger([row])
    return row


def test_preflight_is_materializer_only_and_candidate_closes_archive_and_template(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    fixture, movie, rom, commands = _install_simulated_runner(monkeypatch, tmp_path, [])
    result = run_original_reference_replay(rom_path=rom, preflight_only=True)
    identity = _candidate_identity(fixture, _facts(tmp_path, rom), movie)

    assert result["ProcessStarts"] == 0
    assert commands == []
    assert set(identity) == {
        "candidateSha256",
        "romSha256",
        "archiveSha256",
        "archiveSizeBytes",
        "archiveMemberSetSha256",
        "runnerSha256",
        "helperSha256",
        "fixtureSha256",
        "capabilitySchemaSha256",
        "receiptSchemaSha256",
        "observerSha256",
        "recipeSha256",
        "bk2Sha256",
        "configTemplateSha256",
    }


def test_legacy_preflight_identity_and_ledger_boundary_are_byte_semantic(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    """Freeze the accepted capability transport before shared-kernel extraction.

    This intentionally uses no private archive, ROM, receipt, or ledger bytes.  The
    synthetic fact set lets the test pin the complete legacy candidate and public
    preflight receipt while proving that a preflight cannot read or write the
    consumed private launch ledger.
    """

    fixture = deepcopy(load_capability_fixture())
    movie = materialize_movie(fixture)
    facts: dict[str, Any] = {
        "romSha256": "9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9",
        "archive": _identity("A") | {"sizeBytes": 123},
        "archiveMemberSetSha256": "B" * 64,
        "runner": _identity("C") | {"sizeBytes": 234},
        "helper": _identity("D") | {"sizeBytes": 345},
        "fixtureSha256": "C1818C7A03DC2A846709970E24FE41C88C5C3349A1AEFE1D592CBB63C611C365",
        "observerSha256": "E59E827AD08BE43217A7E778E1D55919D239C3EDA53921C04F338A795B72BDCE",
    }
    expected_candidate = {
        "candidateSha256": "CDE8AED2A9CFD716D0F315FC09F2BECC7B0B0D0FC938D0FC8315913374232700",
        "romSha256": facts["romSha256"],
        "archiveSha256": "A" * 64,
        "archiveSizeBytes": 123,
        "archiveMemberSetSha256": "B" * 64,
        "runnerSha256": "C" * 64,
        "helperSha256": "D" * 64,
        "fixtureSha256": facts["fixtureSha256"],
        "capabilitySchemaSha256": (
            "99E8E508C5A1DF53BAA5C3167832E1D7707FCCF9204A92BA81072663C8E89F20"
        ),
        "receiptSchemaSha256": "C7C90811E4680512A7AEF31FA6E36C94FF5D38DD8A81C3383474D696D6FEBFE4",
        "observerSha256": facts["observerSha256"],
        "recipeSha256": "BAD3DE219DBDA1F2FB9A2DA7191351290575CAFA4FF03A00B75A289ADA2BD867",
        "bk2Sha256": "250F4086E1C1AD08BF64A7CB5C84787E4EE8DA41D83D53630F54DE7FB8085E64",
        "configTemplateSha256": "74C5447770E447491296E888AB78FAC55AA38832A4077F95BEC4D1C92FC5A096",
    }

    assert _sha256(replay.FIXTURE_PATH.read_bytes()) == facts["fixtureSha256"]
    assert _candidate_identity(fixture, facts, movie) == expected_candidate
    monkeypatch.setattr(replay, "load_capability_fixture", lambda: fixture)
    monkeypatch.setattr(replay, "_preflight", lambda *_: (facts, movie))

    def fail_if_ledger_is_touched(*_args: object, **_kwargs: object) -> None:
        raise AssertionError("legacy preflight must not access the consumed private ledger")

    monkeypatch.setattr(replay, "_load_ledger", fail_if_ledger_is_touched)
    monkeypatch.setattr(replay, "_write_ledger", fail_if_ledger_is_touched)

    assert run_original_reference_replay(
        rom_path=tmp_path / "unread-private-rom.bin", preflight_only=True
    ) == {
        "Status": "PASS",
        "Mode": "PREFLIGHT",
        "CapabilityId": "sf2-original-reference-replay-capability-v1",
        "RecipeSha256": movie.recipe_sha256,
        "Bk2Sha256": movie.bk2_sha256,
        "ObserverSha256": facts["observerSha256"],
        "CandidateSha256": expected_candidate["candidateSha256"],
        "ProcessStarts": 0,
    }


def test_contained_launch_never_executes_host_and_cleanup_retains_only_receipt(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    fixture, movie, rom, commands = _install_simulated_runner(
        monkeypatch, tmp_path, [_passing_status(load_capability_fixture())]
    )
    _install_synthetic_global_ordinal_one(monkeypatch, fixture)
    receipt = run_original_reference_replay(rom_path=rom, run_class="diagnostic", launch_ordinal=2)
    candidate = _candidate_identity(fixture, _facts(tmp_path, rom), movie)["candidateSha256"]
    launch = replay.DERIVED_ROOT / candidate / "launch-2"

    assert commands[0][0].endswith("launch-2\\toolchain\\EmuHawk.exe")
    assert all("host" not in part for part in commands[0])
    assert [path.name for path in launch.iterdir()] == ["receipt.json"]
    assert receipt["cleanup"]["residualArtifacts"] == []
    assert receipt["runner"]["processStarts"] == 1
    assert receipt["runner"]["archivePost"] == {
        "status": "captured",
        "identity": {"exists": True, **receipt["runner"]["archive"]},
        "error": None,
    }
    assert receipt["isolation"]["hostDrift"] is False
    assert receipt["isolation"]["hostToolchainDrift"] is False
    assert receipt["isolation"]["hostToolchainBefore"] == {
        "entryCount": 0,
        "sha256": _sha256(_canonical_json([])),
        "entries": [],
    }
    assert receipt["isolation"]["hostToolchainAfter"] == {
        "status": "captured",
        "tree": receipt["isolation"]["hostToolchainBefore"],
        "error": None,
    }
    validate_json(receipt, replay.RECEIPT_SCHEMA, owner="contained replay receipt")
    for mutate in (
        lambda value: value["observer"]["inputPollTrace"][8].update(bk2Row=10),
        lambda value: value["observer"].update(extra=True),
        lambda value: value["execution"].update(exitCode=1),
        lambda value: value["isolation"]["hostAfter"].update(
            status="unavailable", inventory=None, error="synthetic post-snapshot failure"
        ),
        lambda value: value["cleanup"].update(residualArtifacts=["toolchain"]),
    ):
        changed = deepcopy(receipt)
        mutate(changed)
        with pytest.raises(ValueError):
            validate_json(changed, replay.RECEIPT_SCHEMA, owner="PASS receipt mutation")


def test_archive_post_mutation_is_typed_without_archive_restore(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    fixture, movie, rom, _commands = _install_simulated_runner(
        monkeypatch, tmp_path, [_passing_status(load_capability_fixture())]
    )
    archive = Path(_facts(tmp_path, rom)["archivePath"])
    expected_archive = replay._file_identity(archive)
    candidate = _candidate_identity(fixture, _facts(tmp_path, rom), movie)["candidateSha256"]
    _install_synthetic_global_ordinal_one(monkeypatch, fixture)
    original = replay.run_native_bizhawk_process

    def drift(**kwargs: object) -> bizhawk.NativeProcessResult:
        archive.write_bytes(b"mutated synthetic archive")
        return original(**kwargs)

    monkeypatch.setattr(replay, "run_native_bizhawk_process", drift)
    with pytest.raises(CapabilityError, match="archive-drift"):
        run_original_reference_replay(rom_path=rom, run_class="diagnostic", launch_ordinal=2)

    receipt = json.loads(
        (replay.DERIVED_ROOT / candidate / "launch-2" / "receipt.json").read_text()
    )
    assert receipt["failure"]["code"] == "archive-drift"
    assert receipt["failure"]["expected"] == expected_archive
    assert receipt["failure"]["actual"] == {
        "exists": True,
        **replay._file_identity(archive),
    }
    assert archive.read_bytes() == b"mutated synthetic archive"


def test_archive_post_snapshot_unavailable_is_receipted(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    fixture, movie, rom, _commands = _install_simulated_runner(
        monkeypatch, tmp_path, [_passing_status(load_capability_fixture())]
    )
    archive = Path(_facts(tmp_path, rom)["archivePath"])
    _install_synthetic_global_ordinal_one(monkeypatch, fixture)
    original = replay._snapshot_file_identity

    def unavailable_archive(path: Path) -> dict[str, object]:
        if path == archive:
            return {
                "status": "unavailable",
                "identity": None,
                "error": "synthetic archive readback unavailable",
            }
        return original(path)

    monkeypatch.setattr(replay, "_snapshot_file_identity", unavailable_archive)
    with pytest.raises(CapabilityError, match="post-snapshot-unavailable"):
        run_original_reference_replay(rom_path=rom, run_class="diagnostic", launch_ordinal=2)

    candidate = _candidate_identity(fixture, _facts(tmp_path, rom), movie)["candidateSha256"]
    receipt = json.loads(
        (replay.DERIVED_ROOT / candidate / "launch-2" / "receipt.json").read_text()
    )
    assert receipt["failure"]["code"] == "post-snapshot-unavailable"
    assert receipt["runner"]["archivePost"] == {
        "status": "unavailable",
        "identity": None,
        "error": "synthetic archive readback unavailable",
    }
    assert archive.read_bytes() == b"synthetic archive"


def test_missing_status_is_nullable_not_fabricated(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    fixture, movie, rom, _commands = _install_simulated_runner(monkeypatch, tmp_path, [None])
    _install_synthetic_global_ordinal_one(monkeypatch, fixture)
    with pytest.raises(CapabilityError, match="status-missing"):
        run_original_reference_replay(rom_path=rom, run_class="diagnostic", launch_ordinal=2)
    candidate = _candidate_identity(fixture, _facts(tmp_path, rom), movie)["candidateSha256"]
    receipt = json.loads(
        (replay.DERIVED_ROOT / candidate / "launch-2" / "receipt.json").read_text()
    )
    assert receipt["observer"] == {
        "sha256": fixture["passiveObserverPolicy"]["observerSha256"],
        "statusPresent": False,
        "callbacksRemaining": None,
        "status": None,
        "inputPollTrace": None,
        "initialFrame": None,
        "firstPollFrame": None,
        "terminalFrame": None,
        "movieMode": None,
        "readOnly": None,
        "powerOn": None,
        "headerPlatform": None,
        "headerCore": None,
        "clientVersion": None,
        "statusWriteOk": None,
    }
    fabricated = deepcopy(receipt)
    fabricated["observer"]["callbacksRemaining"] = 0
    with pytest.raises(ValueError, match="callbacksRemaining"):
        validate_json(
            fabricated,
            replay.RECEIPT_SCHEMA,
            owner="missing observer status cannot fabricate callback facts",
        )


def test_observer_failure_requires_a_nonzero_emulator_exit(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    status = _passing_status(load_capability_fixture())
    status["status"] = "FAIL:input-poll-callback:expected=callback-success:actual=synthetic"
    fixture, movie, rom, _commands = _install_simulated_runner(
        monkeypatch, tmp_path, [status]
    )
    _install_synthetic_global_ordinal_one(monkeypatch, fixture)
    with pytest.raises(CapabilityError, match="observer-failure-zero-exit"):
        run_original_reference_replay(rom_path=rom, run_class="diagnostic", launch_ordinal=2)
    candidate = _candidate_identity(fixture, _facts(tmp_path, rom), movie)["candidateSha256"]
    receipt = json.loads(
        (replay.DERIVED_ROOT / candidate / "launch-2" / "receipt.json").read_text()
    )
    assert receipt["failure"] == {
        "phase": "process",
        "code": "observer-failure-zero-exit",
        "expected": "non-zero exit for observer failure",
        "actual": {
            "exitCode": 0,
            "observerStatus": status["status"],
            "callbacksRemaining": 0,
        },
    }


def test_host_drift_is_typed_without_host_delete(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    fixture, movie, rom, _commands = _install_simulated_runner(
        monkeypatch, tmp_path, [_passing_status(load_capability_fixture())]
    )
    host = Path(_facts(tmp_path, rom)["hostToolchainRoot"])
    host.mkdir(exist_ok=True)
    _install_synthetic_global_ordinal_one(monkeypatch, fixture)
    original = replay.run_native_bizhawk_process

    def drift(**kwargs: object) -> bizhawk.NativeProcessResult:
        (host / "config.ini").write_text("host changed", encoding="utf-8")
        return original(**kwargs)

    monkeypatch.setattr(replay, "run_native_bizhawk_process", drift)
    with pytest.raises(CapabilityError, match="host-toolchain-drift"):
        run_original_reference_replay(rom_path=rom, run_class="diagnostic", launch_ordinal=2)
    assert (host / "config.ini").read_text(encoding="utf-8") == "host changed"


def test_nested_gamedb_user_surface_is_inventoried_and_host_drift_is_typed(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    fixture, movie, rom, _commands = _install_simulated_runner(
        monkeypatch, tmp_path, [_passing_status(load_capability_fixture())]
    )
    host = Path(_facts(tmp_path, rom)["hostToolchainRoot"])
    obsolete_root = host / "gamedb_user"
    obsolete_root.write_bytes(b"obsolete root alias")
    obsolete_only = replay._inventory_mutable_surfaces(host)
    obsolete_entries = {entry["surface"]: entry for entry in obsolete_only["entries"]}
    assert "gamedb_user" not in obsolete_entries
    assert obsolete_entries["gamedb/gamedb_user.txt"]["kind"] == "absent"

    nested = host / "gamedb" / "gamedb_user.txt"
    nested.parent.mkdir()
    nested.write_bytes(b"before nested drift")
    expected_before = replay._inventory_mutable_surfaces(host)
    before_entries = {entry["surface"]: entry for entry in expected_before["entries"]}
    assert before_entries["gamedb/gamedb_user.txt"] == {
        "surface": "gamedb/gamedb_user.txt",
        "kind": "file",
        **replay._file_identity(nested),
        "fileCount": 1,
    }

    _install_synthetic_global_ordinal_one(monkeypatch, fixture)
    original = replay.run_native_bizhawk_process

    def drift(**kwargs: object) -> bizhawk.NativeProcessResult:
        nested.write_bytes(b"after nested drift")
        return original(**kwargs)

    monkeypatch.setattr(replay, "run_native_bizhawk_process", drift)
    with pytest.raises(CapabilityError, match="host-toolchain-drift"):
        run_original_reference_replay(rom_path=rom, run_class="diagnostic", launch_ordinal=2)

    candidate = _candidate_identity(fixture, _facts(tmp_path, rom), movie)["candidateSha256"]
    receipt = json.loads(
        (replay.DERIVED_ROOT / candidate / "launch-2" / "receipt.json").read_text()
    )
    after = receipt["isolation"]["hostAfter"]["inventory"]
    assert receipt["failure"]["code"] == "host-toolchain-drift"
    assert receipt["isolation"]["hostDrift"] is True
    assert receipt["isolation"]["hostToolchainDrift"] is True
    assert receipt["isolation"]["hostBefore"] == expected_before
    assert after is not None
    after_entries = {entry["surface"]: entry for entry in after["entries"]}
    assert (
        after_entries["gamedb/gamedb_user.txt"]["sha256"]
        != before_entries["gamedb/gamedb_user.txt"]["sha256"]
    )
    assert nested.read_bytes() == b"after nested drift"
    assert obsolete_root.read_bytes() == b"obsolete root alias"


def test_unclassified_host_file_drift_is_typed_without_host_revert(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    fixture, movie, rom, _commands = _install_simulated_runner(
        monkeypatch, tmp_path, [_passing_status(load_capability_fixture())]
    )
    host = Path(_facts(tmp_path, rom)["hostToolchainRoot"])
    unclassified = host / "unclassified-host-file.bin"
    unclassified.write_bytes(b"before")
    tree_before = replay._inventory_host_toolchain_tree(host)
    assert tree_before["entries"] == [
        {
            "kind": "file",
            "path": "unclassified-host-file.bin",
            **replay._file_identity(unclassified),
            "lastWriteUtcTicks": unclassified.stat().st_mtime_ns // 100,
        }
    ]
    _install_synthetic_global_ordinal_one(monkeypatch, fixture)
    original = replay.run_native_bizhawk_process

    def drift(**kwargs: object) -> bizhawk.NativeProcessResult:
        unclassified.write_bytes(b"after")
        return original(**kwargs)

    monkeypatch.setattr(replay, "run_native_bizhawk_process", drift)
    with pytest.raises(CapabilityError, match="host-toolchain-drift"):
        run_original_reference_replay(rom_path=rom, run_class="diagnostic", launch_ordinal=2)

    candidate = _candidate_identity(fixture, _facts(tmp_path, rom), movie)["candidateSha256"]
    receipt = json.loads(
        (replay.DERIVED_ROOT / candidate / "launch-2" / "receipt.json").read_text()
    )
    assert receipt["failure"]["code"] == "host-toolchain-drift"
    assert receipt["failure"]["expected"] == tree_before["entries"][0]
    assert receipt["failure"]["actual"]["path"] == "unclassified-host-file.bin"
    assert receipt["isolation"]["hostDrift"] is False
    assert receipt["isolation"]["hostToolchainDrift"] is True
    assert receipt["cleanup"]["residualArtifacts"] == []
    assert unclassified.read_bytes() == b"after"


def test_added_empty_host_directory_is_typed_without_host_delete(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    fixture, movie, rom, _commands = _install_simulated_runner(
        monkeypatch, tmp_path, [_passing_status(load_capability_fixture())]
    )
    host = Path(_facts(tmp_path, rom)["hostToolchainRoot"])
    _install_synthetic_global_ordinal_one(monkeypatch, fixture)
    original = replay.run_native_bizhawk_process

    def drift(**kwargs: object) -> bizhawk.NativeProcessResult:
        (host / "added-empty").mkdir()
        return original(**kwargs)

    monkeypatch.setattr(replay, "run_native_bizhawk_process", drift)
    with pytest.raises(CapabilityError, match="host-toolchain-drift"):
        run_original_reference_replay(rom_path=rom, run_class="diagnostic", launch_ordinal=2)

    candidate = _candidate_identity(fixture, _facts(tmp_path, rom), movie)["candidateSha256"]
    receipt = json.loads(
        (replay.DERIVED_ROOT / candidate / "launch-2" / "receipt.json").read_text()
    )
    assert receipt["failure"] == {
        "phase": "isolation",
        "code": "host-toolchain-drift",
        "expected": None,
        "actual": {
            "kind": "directory",
            "path": "added-empty",
            "sha256": None,
            "sizeBytes": 0,
            "lastWriteUtcTicks": receipt["failure"]["actual"]["lastWriteUtcTicks"],
        },
    }
    assert receipt["isolation"]["hostDrift"] is False
    assert receipt["isolation"]["hostToolchainDrift"] is True
    assert (host / "added-empty").is_dir()


def test_host_toolchain_reparse_is_rejected_and_snapshot_is_captured(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    host = tmp_path / "host"
    blocked = host / "reparse"
    blocked.mkdir(parents=True)
    original = replay._reject_reparse

    def reject_reparse(path: Path) -> None:
        original(path)
        if path == blocked:
            raise CapabilityError("synthetic host reparse")

    monkeypatch.setattr(replay, "_reject_reparse", reject_reparse)
    with pytest.raises(CapabilityError, match="synthetic host reparse"):
        replay._inventory_host_toolchain_tree(host)
    snapshot = replay._snapshot_host_toolchain_tree(host)
    assert snapshot == {
        "status": "unavailable",
        "tree": None,
        "error": "synthetic host reparse",
    }


def test_contained_mutable_surface_drift_is_typed_before_cleanup(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    fixture, movie, rom, _commands = _install_simulated_runner(
        monkeypatch, tmp_path, [_passing_status(load_capability_fixture())]
    )
    _install_synthetic_global_ordinal_one(monkeypatch, fixture)
    original = replay.run_native_bizhawk_process

    def drift(**kwargs: object) -> bizhawk.NativeProcessResult:
        executable = kwargs["executable"]
        assert isinstance(executable, Path)
        save_ram = executable.parent / "Genesis" / "SaveRAM" / "unexpected.srm"
        save_ram.parent.mkdir(parents=True)
        save_ram.write_bytes(b"contained drift")
        return original(**kwargs)

    monkeypatch.setattr(replay, "run_native_bizhawk_process", drift)
    with pytest.raises(CapabilityError, match="contained-drift"):
        run_original_reference_replay(rom_path=rom, run_class="diagnostic", launch_ordinal=2)
    candidate = _candidate_identity(fixture, _facts(tmp_path, rom), movie)["candidateSha256"]
    receipt = json.loads(
        (replay.DERIVED_ROOT / candidate / "launch-2" / "receipt.json").read_text()
    )
    assert receipt["failure"]["code"] == "contained-drift"
    assert receipt["cleanup"]["residualArtifacts"] == []


def test_ordinal_three_requires_same_candidate_ordinal_two_pass(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    fixture, _movie, rom, _commands = _install_simulated_runner(
        monkeypatch,
        tmp_path,
        [
            _passing_status(load_capability_fixture()),
            _passing_status(load_capability_fixture()),
        ],
    )
    _install_synthetic_global_ordinal_one(monkeypatch, fixture)
    diagnostic = run_original_reference_replay(
        rom_path=rom, run_class="diagnostic", launch_ordinal=2
    )
    accepted = run_original_reference_replay(
        rom_path=rom, run_class="frozen-acceptance", launch_ordinal=3
    )
    assert diagnostic["execution"]["status"] == accepted["execution"]["status"] == "PASS"
    assert diagnostic["determinism"]["replayDigest"] == accepted["determinism"]["replayDigest"]
    with pytest.raises(CapabilityError, match="globally reserved"):
        run_original_reference_replay(rom_path=rom, run_class="diagnostic", launch_ordinal=2)


@pytest.mark.parametrize(
    ("mutate", "message"),
    [
        (lambda rows: rows.clear(), "exactly one global ordinal 1"),
        (lambda rows: rows[0].update(candidateSha256="B" * 64), "ledger"),
        (lambda rows: rows[0].update(ordinal="1"), "numeric"),
        (lambda rows: rows.append(deepcopy(rows[0])), "duplicate ordinal"),
    ],
)
def test_global_ordinal_one_lock_rejects_forged_or_missing_rows_before_process(
    monkeypatch: pytest.MonkeyPatch,
    tmp_path: Path,
    mutate: Any,
    message: str,
) -> None:
    fixture, _movie, rom, commands = _install_simulated_runner(monkeypatch, tmp_path, [])
    _install_synthetic_global_ordinal_one(monkeypatch, fixture)
    rows = replay._load_ledger()
    mutate(rows)
    replay._write_ledger(rows)

    with pytest.raises(CapabilityError, match=message):
        run_original_reference_replay(rom_path=rom, run_class="diagnostic", launch_ordinal=2)
    assert commands == []


def test_real_global_ordinal_one_lock_is_consumed_read_only() -> None:
    if not replay._ledger_path().exists():
        pytest.skip("this worktree has no optional private historical launch ledger")
    ledger = replay._validate_ledger(replay._load_ledger())
    ordinal_one = next(row for row in ledger if row["ordinal"] == 1)

    assert ordinal_one["candidateSha256"] == replay._GLOBAL_ORDINAL_ONE_CANDIDATE
    assert ordinal_one["receiptSha256"] == replay._GLOBAL_ORDINAL_ONE_RECEIPT_SHA256
    assert ordinal_one["status"] == "FAIL"


def test_ordinal_three_rejects_tampered_ordinal_two_receipt_before_process(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    fixture, movie, rom, commands = _install_simulated_runner(
        monkeypatch,
        tmp_path,
        [_passing_status(load_capability_fixture())],
    )
    _install_synthetic_global_ordinal_one(monkeypatch, fixture)
    run_original_reference_replay(rom_path=rom, run_class="diagnostic", launch_ordinal=2)
    candidate = _candidate_identity(fixture, _facts(tmp_path, rom), movie)["candidateSha256"]
    receipt_path = replay.DERIVED_ROOT / candidate / "launch-2" / "receipt.json"
    receipt_path.write_bytes(b"tampered ordinal two receipt")

    with pytest.raises(CapabilityError, match="receipt bytes do not match ledger hash"):
        run_original_reference_replay(rom_path=rom, run_class="frozen-acceptance", launch_ordinal=3)
    assert len(commands) == 1


@pytest.mark.parametrize("target", ["host", "toolchain"])
def test_post_launch_inventory_exception_is_receipted_and_contained(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path, target: str
) -> None:
    fixture, movie, rom, _commands = _install_simulated_runner(
        monkeypatch, tmp_path, [_passing_status(load_capability_fixture())]
    )
    _install_synthetic_global_ordinal_one(monkeypatch, fixture)
    host = Path(_facts(tmp_path, rom)["hostToolchainRoot"])
    protected = host / "config.ini"
    protected.write_bytes(b"host must remain untouched")
    original = replay._inventory_mutable_surfaces
    calls: dict[str, int] = {"host": 0, "toolchain": 0}

    def fail_post_snapshot(root: Path) -> dict[str, Any]:
        label = "host" if root == host else "toolchain"
        calls[label] += 1
        if label == target and calls[label] == 2:
            raise CapabilityError(f"{target} post inventory reparse")
        return original(root)

    monkeypatch.setattr(replay, "_inventory_mutable_surfaces", fail_post_snapshot)
    with pytest.raises(CapabilityError, match="post-snapshot-unavailable"):
        run_original_reference_replay(rom_path=rom, run_class="diagnostic", launch_ordinal=2)
    candidate = _candidate_identity(fixture, _facts(tmp_path, rom), movie)["candidateSha256"]
    receipt = json.loads(
        (replay.DERIVED_ROOT / candidate / "launch-2" / "receipt.json").read_text()
    )
    assert receipt["execution"]["status"] == "FAIL"
    assert receipt["failure"]["code"] == "post-snapshot-unavailable"
    assert receipt["cleanup"]["residualArtifacts"] == []
    assert protected.read_bytes() == b"host must remain untouched"
    assert receipt["isolation"][f"{target}After"]["status"] == "unavailable"


def _execute_observer_with_transformed_lua_api(api: str) -> tuple[dict[str, Any], str]:
    """Run the production observer against a minimal injected Lua API, never EmuHawk."""

    _, executable = bizhawk.bizhawk_contract()
    library = CDLL(str(executable.parent / "dll" / "lua54.dll"))
    library.luaL_newstate.argtypes = []
    library.luaL_newstate.restype = c_void_p
    library.luaL_openlibs.argtypes = [c_void_p]
    library.luaL_openlibs.restype = None
    library.luaL_loadbufferx.argtypes = [c_void_p, c_char_p, c_size_t, c_char_p, c_char_p]
    library.luaL_loadbufferx.restype = c_int
    library.lua_pcallk.argtypes = [c_void_p, c_int, c_int, c_int, c_ssize_t, c_void_p]
    library.lua_pcallk.restype = c_int
    library.lua_getglobal.argtypes = [c_void_p, c_char_p]
    library.lua_getglobal.restype = c_int
    library.lua_tolstring.argtypes = [c_void_p, c_int, POINTER(c_size_t)]
    library.lua_tolstring.restype = c_void_p
    library.lua_close.argtypes = [c_void_p]
    library.lua_close.restype = None

    state = library.luaL_newstate()
    assert state
    source = (
        "__observer_emitted = nil\n__observer_exit = nil\n"
        + api
        + "\n"
        + replay.OBSERVER_PATH.read_text(encoding="utf-8")
    ).encode("utf-8")
    try:
        library.luaL_openlibs(state)
        assert library.luaL_loadbufferx(state, source, len(source), b"@observer-harness", b"t") == 0
        assert library.lua_pcallk(state, 0, 0, 0, 0, None) == 0

        def lua_global(name: str) -> str:
            library.lua_getglobal(state, name.encode("ascii"))
            length = c_size_t()
            pointer = library.lua_tolstring(state, -1, byref(length))
            assert pointer
            return string_at(pointer, length.value).decode("utf-8")

        return json.loads(lua_global("__observer_emitted")), lua_global("__observer_exit")
    finally:
        library.lua_close(state)


def _observer_timing_api(frameadvance: str) -> str:
    return """
local frame, callback = 1, nil
os = {
    getenv = function(name)
        if name == "SF2_ORIGINAL_REFERENCE_STATUS" then return "status.json" end
        return "32"
    end
}
io = {
    open = function()
        return {
            write = function(_, value) __observer_emitted = value return true end,
            close = function() return true end
        }
    end
}
event = {
    oninputpoll = function(value) callback = value return 7 end,
    unregisterbyid = function() return true end
}
emu = {
    framecount = function() return frame end,
    getsystemid = function() return "GEN" end,
    frameadvance = function()
__FRAMEADVANCE__
    end
}
movie = {
    getreadonly = function() return true end,
    startsfromsavestate = function() return false end,
    startsfromsaveram = function() return false end,
    isloaded = function() return true end,
    length = function() return 33 end,
    mode = function() return frame == 33 and "FINISHED" or "PLAY" end,
    getheader = function()
        return { Platform = "GEN", Core = "Genesis Plus GX" }
    end,
    getinputasmnemonic = function() return "|.|........|" end,
    getinput = function() return {} end
}
joypad = { get = function() return {} end }
client = {
    getversion = function() return "2.11.1" end,
    exitCode = function(code) __observer_exit = tostring(code) end
}
    """.replace("__FRAMEADVANCE__", frameadvance)


def test_observer_records_semantic_frames_one_through_32_then_writes_at_33() -> None:
    status, exit_code = _execute_observer_with_transformed_lua_api(
        _observer_timing_api("callback()\n        frame = frame + 1")
    )

    assert exit_code == "0"
    assert status["status"] == "PASS"
    assert status["callbacksRemaining"] == 0
    assert status["initialFrame"] == status["firstPollFrame"] == 1
    assert status["terminalFrame"] == 33
    assert status["inputPollTrace"] == [
        {
            "semanticIndex": index,
            "bk2Row": index + 1,
            "emuFrame": index + 1,
            "input": "|.|........|",
        }
        for index in range(32)
    ]


@pytest.mark.parametrize(
    ("frameadvance", "expected_trace_count"),
    [
        pytest.param("frame = frame + 1\n        callback()", 0, id="delayed-first"),
        pytest.param(
            "if frame == 1 then\n"
            "    callback()\n"
            "    frame = frame + 1\n"
            "else\n"
            "    frame = frame + 2\n"
            "    callback()\n"
            "end",
            1,
            id="skipped-frame",
        ),
        pytest.param("callback()", 1, id="duplicate-frame"),
        pytest.param(
            "callback()\n        callback()\n        frame = frame + 1", 1, id="multiple-poll"
        ),
    ],
)
def test_observer_rejects_nonconsecutive_or_multiple_input_polls(
    frameadvance: str, expected_trace_count: int
) -> None:
    status, exit_code = _execute_observer_with_transformed_lua_api(
        _observer_timing_api(frameadvance)
    )

    assert exit_code == "1"
    assert status["status"].startswith("FAIL:semantic-frame:")
    assert status["status"] != "PASS"
    assert status["callbacksRemaining"] == 0
    assert len(status["inputPollTrace"]) == expected_trace_count


@pytest.mark.parametrize(
    ("api", "expected_callbacks", "expected_diagnostic"),
    [
        (
            """
os = {
    getenv = function(name)
        if name == "SF2_ORIGINAL_REFERENCE_STATUS" then return "status.json" end
        return "32"
    end
}
io = {
    open = function()
        return {
            write = function(_, value) __observer_emitted = value return true end,
            close = function() return true end
        }
    end
}
event = {
    oninputpoll = function() return 7 end,
    unregisterbyid = function() return true end
}
emu = {
    framecount = function() error("frame diagnostic") end,
    getsystemid = function() return "GEN" end,
    frameadvance = function() end
}
movie = {
    getreadonly = function() error("early diagnostic") end,
    startsfromsavestate = function() return false end,
    startsfromsaveram = function() return false end,
    mode = function() error("mode diagnostic") end
}
joypad = { get = function() return {} end }
client = {
    getversion = function() return "2.11.1" end,
    exitCode = function(code) __observer_exit = tostring(code) end
}
            """,
            0,
            None,
        ),
        (
            """
local frame, callback = 1, nil
os = {
    getenv = function(name)
        if name == "SF2_ORIGINAL_REFERENCE_STATUS" then return "status.json" end
        return "32"
    end
}
io = {
    open = function()
        return {
            write = function(_, value) __observer_emitted = value return true end,
            close = function() return true end
        }
    end
}
event = {
    oninputpoll = function(value) callback = value return 7 end,
    unregisterbyid = function() return false end
}
emu = {
    framecount = function() return frame end,
    getsystemid = function() return "GEN" end,
    frameadvance = function()
        callback()
        frame = frame + 1
        error("stop after callback")
    end
}
movie = {
    getreadonly = function() return true end,
    startsfromsavestate = function() return false end,
    startsfromsaveram = function() return false end,
    isloaded = function() return true end,
    length = function() return 33 end,
    mode = function() return "PLAY" end,
    getheader = function()
        return { Platform = "GEN", Core = "Genesis Plus GX" }
    end,
    getinputasmnemonic = function() return "|.|........|" end,
    getinput = function() return {} end
}
local diagnostic = 'quote" slash' .. string.char(92)
diagnostic = diagnostic .. ' newline' .. string.char(10)
diagnostic = diagnostic .. ' tab' .. string.char(9)
diagnostic = diagnostic .. ' unit' .. string.char(31)
joypad = { get = function() error(diagnostic) end }
client = {
    getversion = function() return "2.11.1" end,
    exitCode = function(code) __observer_exit = tostring(code) end
}
            """,
            1,
            'quote" slash\\ newline\n tab\t unit\x1f',
        ),
    ],
)
def test_observer_failure_json_and_cleanup_are_nonzero_and_not_pass(
    api: str, expected_callbacks: int, expected_diagnostic: str | None
) -> None:
    status, exit_code = _execute_observer_with_transformed_lua_api(api)

    assert exit_code == "1"
    assert status["status"].startswith("FAIL:")
    assert status["status"] != "PASS"
    assert status["callbacksRemaining"] == expected_callbacks
    assert status["statusWriteOk"] is True
    if expected_diagnostic is not None:
        assert expected_diagnostic in status["status"]
    if expected_callbacks == 0:
        assert status["initialFrame"] is None
        assert status["terminalFrame"] is None
        assert status["readOnly"] is None
        assert status["movieMode"] is None


def test_observer_utf8_boundary_truncation_is_valid_json() -> None:
    api = """
local frame, callback = 1, nil
os = {
    getenv = function(name)
        if name == "SF2_ORIGINAL_REFERENCE_STATUS" then return "status.json" end
        return "32"
    end
}
io = {
    open = function()
        return {
            write = function(_, value) __observer_emitted = value return true end,
            close = function() return true end
        }
    end
}
event = {
    oninputpoll = function(value) callback = value return 7 end,
    unregisterbyid = function() return false end
}
emu = {
    framecount = function() return frame end,
    getsystemid = function() return "GEN" end,
    frameadvance = function()
        callback()
        frame = frame + 1
        error("stop after callback")
    end
}
movie = {
    getreadonly = function() return true end,
    startsfromsavestate = function() return false end,
    startsfromsaveram = function() return false end,
    isloaded = function() return true end,
    length = function() return 33 end,
    mode = function() return "PLAY" end,
    getheader = function()
        return { Platform = "GEN", Core = "Genesis Plus GX" }
    end,
    getinputasmnemonic = function() return "|.|........|" end,
    getinput = function() return {} end
}
local diagnostic = string.rep("a", 964) .. string.char(0xE2, 0x82, 0xAC)
diagnostic = diagnostic .. ' quote" slash' .. string.char(92)
diagnostic = diagnostic .. ' newline' .. string.char(10) .. ' unit' .. string.char(31)
joypad = { get = function() error(diagnostic, 0) end }
client = {
    getversion = function() return "2.11.1" end,
    exitCode = function(code) __observer_exit = tostring(code) end
}
    """

    status, exit_code = _execute_observer_with_transformed_lua_api(api)

    failure_prefix = "FAIL:input-poll-callback:expected=callback-success:actual="
    expected = failure_prefix + "a" * 964 + "...[truncated]"
    assert exit_code == "1"
    assert status["status"].startswith("FAIL:input-poll-callback:")
    assert status["status"] != "PASS"
    assert status["callbacksRemaining"] == 1
    assert status["statusWriteOk"] is True
    assert expected in status["status"]
    assert "€" not in status["status"]
    assert 'quote" slash' not in status["status"]


def test_observer_status_loader_and_source_preserve_semantic_mapping(tmp_path: Path) -> None:
    assert _load_observer_status(tmp_path / "missing.json") is None
    observer = replay.OBSERVER_PATH.read_text(encoding="utf-8")
    for text in (
        "local bk2_row = semantic_index + 1",
        "semanticIndex = semantic_index, bk2Row = bk2_row, emuFrame = frame, input = actual",
        'fail("semantic-frame", tostring(semantic_index + 1), tostring(frame))',
        'fail("semantic-attachment", tostring(initial_frame), tostring(first_poll_frame))',
        'fail("terminal-frame", "33", tostring(terminal_frame))',
        "exit_with(1)",
        "callbacks = unresolved",
        "local function json_boolean_or_null(value)",
        "local function json_string_or_null(value)",
        "local JSON_STRING_MAX_BYTES = 1024",
        "local function utf8_safe_prefix(text, max_bytes)",
        'text = utf8_safe_prefix(text, JSON_STRING_MAX_BYTES) .. "...[truncated]"',
        r'string.format("\\u%04X", byte)',
    ):
        assert text in observer
    assert 'string.format("%q", tostring(value))' not in observer
