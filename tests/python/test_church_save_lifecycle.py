"""Static adversarial tests for the Church save lifecycle H3 slice."""

from __future__ import annotations

import json
import shutil
from copy import deepcopy
from hashlib import sha256
from pathlib import Path

import pytest

from sf2tool.h3 import church_save_lifecycle as rail
from sf2tool.h3.bizhawk import bizhawk_contract, validate_lua_syntax
from sf2tool.jsonio import load_json, validate_json


def _rom() -> Path:
    return rail.repo_path("local/roms/sf2-us.bin")


def test_static_contract_derives_the_church_save_control_flow_and_sram_geometry() -> None:
    static = rail.build_static_contract(_rom())
    assert static["sourceContext"] == {"churchMenuEntryAddress": 0x20A02}
    assert static["actionDispatcher"] == {
        "comparisons": [0, 1, 2],
        "saveAction": 3,
        "raiseAction": 0,
        "cureAction": 1,
        "promotionAction": 2,
    }
    assert static["addresses"]["actionReturn"] == 0x20A36
    assert static["prompts"]["first"]["acceptTarget"] == 0x20FE6
    assert static["prompts"]["postSave"]["continueTarget"] == 0x20A40
    assert static["saveGame"]["selector"] == {"zeroSlot": 1, "nonzeroSlot": 2}
    assert static["saveGame"]["rts"] == 0x6FAA
    assert static["saveGame"]["actualStoredBytes"] == 4016
    assert static["saveGame"]["interleavedAddressIntervalBytes"] == 8032
    assert static["mutations"]["flag399"]["mask"] == 0x01
    assert static["suspendBoundary"]["entryOnly"] is True


def test_fixture_is_closed_exact_golden_and_schema_valid() -> None:
    fixture = load_json(rail.FIXTURE)
    static = rail.build_static_contract(_rom())
    validate_json(fixture, rail.FIXTURE_SCHEMA, owner=rail.OWNER)
    rail._assert_fixture(fixture, static)
    assert fixture["acceptedObservation"] == rail.expected_observation(fixture, static)
    validate_json(fixture["acceptedObservation"], rail.OBSERVATION_SCHEMA, owner=rail.OWNER)
    fixture_schema = load_json(rail.FIXTURE_SCHEMA)
    observation_schema = load_json(rail.OBSERVATION_SCHEMA)
    assert "const" not in json.dumps(fixture_schema["properties"]["caseOrder"], sort_keys=True)
    assert "enum" not in json.dumps(
        fixture_schema["definitions"]["case"]["properties"]["caseId"], sort_keys=True
    )
    assert "const" not in json.dumps(fixture_schema["definitions"]["sourceContext"], sort_keys=True)
    assert "const" not in json.dumps(observation_schema["definitions"]["record"], sort_keys=True)


@pytest.mark.parametrize(
    ("name", "mutate"),
    (
        ("case-order", lambda value: value["caseOrder"].reverse()),
        ("case-id", lambda value: value["cases"][0].update(caseId="wrong")),
        (
            "source-context",
            lambda value: value["sourceContext"].update(churchMenuEntryAddress=0x20A04),
        ),
        ("result-order", lambda value: value["acceptedObservation"]["records"].reverse()),
        (
            "result-pc",
            lambda value: value["acceptedObservation"]["records"][0].update(churchEntryPc=0x20A04),
        ),
        ("map-boundary", lambda value: value["cases"][2].update(currentMap=77)),
        (
            "output-drift",
            lambda value: value["acceptedObservation"]["records"][1].update(saveGame=False),
        ),
    ),
)
def test_schema_valid_cross_field_drift_rejects_before_observer(
    name: str, mutate: object, tmp_path: Path, monkeypatch: pytest.MonkeyPatch
) -> None:
    fixture = load_json(rail.FIXTURE)
    mutate(fixture)  # type: ignore[operator]
    validate_json(fixture, rail.FIXTURE_SCHEMA, owner=f"{rail.OWNER} structural drift")
    fixture_path = tmp_path / f"{name}.json"
    fixture_path.write_text(json.dumps(fixture), encoding="utf-8")
    monkeypatch.setattr(rail, "FIXTURE", fixture_path)

    def forbidden_launch(**_: object) -> dict[str, object]:
        raise AssertionError("schema-valid fixture drift reached run_observer")

    monkeypatch.setattr(rail, "run_observer", forbidden_launch)
    rail.OBSERVED_OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    rail.OBSERVED_OUTPUT.write_text("stale generated observation", encoding="utf-8")
    rejected = "(identity|ID/order|matrix|source-context|accepted observation)"
    with pytest.raises(ValueError, match=rejected):
        rail.verify_church_save_lifecycle(_rom(), timeout_seconds=1)
    assert not rail.OBSERVED_OUTPUT.exists()


def test_schema_rejection_removes_generated_observation_before_launch(
    tmp_path: Path, monkeypatch: pytest.MonkeyPatch
) -> None:
    fixture = load_json(rail.FIXTURE)
    fixture["cases"][0]["unexpected"] = True
    fixture_path = tmp_path / "invalid-schema.json"
    fixture_path.write_text(json.dumps(fixture), encoding="utf-8")
    monkeypatch.setattr(rail, "FIXTURE", fixture_path)

    def forbidden_launch(**_: object) -> dict[str, object]:
        raise AssertionError("schema-invalid fixture drift reached run_observer")

    monkeypatch.setattr(rail, "run_observer", forbidden_launch)
    rail.OBSERVED_OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    rail.OBSERVED_OUTPUT.write_text("stale generated observation", encoding="utf-8")
    with pytest.raises(ValueError, match="failed schema validation"):
        rail.verify_church_save_lifecycle(_rom(), timeout_seconds=1)
    assert not rail.OBSERVED_OUTPUT.exists()


def _static_surface(tmp_path: Path) -> Path:
    root = tmp_path / "SF2DISASM"
    sources = (
        rail.CHURCH,
        rail.SRAM,
        rail.WITCH_SUSPEND,
        rail.CONSTANTS,
        rail.ENUMS,
        rail.MACROS,
        rail.GAME_FLAGS,
    )
    for relative in sources:
        source = rail.UPSTREAM / "disasm" / relative
        target = root / "disasm" / relative
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(source, target)
    (root / "build").mkdir(parents=True, exist_ok=True)
    shutil.copy2(rail.UPSTREAM / "build/sf2build-h1.lst", root / "build/sf2build-h1.lst")
    return root


def test_source_h1_rom_and_session_patch_mutations_fail_closed(
    tmp_path: Path, monkeypatch: pytest.MonkeyPatch
) -> None:
    source = _static_surface(tmp_path)
    candidate = tmp_path / "candidate.bin"
    shutil.copy2(_rom(), candidate)
    church = source / "disasm" / rail.CHURCH
    church.write_text(
        church.read_text(encoding="utf-8").replace("setFlg  399", "setFlg  398", 1),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="source guard drift"):
        rail.build_static_contract(candidate, source)

    source = _static_surface(tmp_path / "h1")
    listing = source / "build/sf2build-h1.lst"
    listing.write_text(
        listing.read_text(encoding="utf-8").replace("00020FF4 4EB8 6F6A", "00020FF4 4EB8 6F6B", 1),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="target/return|use-site"):
        rail.build_static_contract(candidate, source)

    source = _static_surface(tmp_path / "rts-h1")
    listing = source / "build/sf2build-h1.lst"
    listing.write_text(
        listing.read_text(encoding="utf-8").replace("00006FAA 4E75", "00006FAA 4E71", 1),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="use-site"):
        rail.build_static_contract(candidate, source)

    source = _static_surface(tmp_path / "rts-source")
    sram = source / "disasm" / rail.SRAM
    sram.write_text(
        sram.read_text(encoding="utf-8").replace(
            "movem.l (sp)+,d0-d1/d7-a2\n                rts",
            "movem.l (sp)+,d0-d1/d7-a2\n                nop",
            1,
        ),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="source guard drift"):
        rail.build_static_contract(candidate, source)

    def candidate_with_byte(address: int) -> Path:
        mutated = tmp_path / f"candidate-{address:X}.bin"
        data = bytearray(_rom().read_bytes())
        data[address] ^= 0x01
        mutated.write_bytes(data)
        monkeypatch.setattr(rail, "ROM_SHA256", sha256(data).hexdigest().upper())
        return mutated

    target_mutation = candidate_with_byte(rail.ADDRESSES["saveGameCall"] + 3)
    with pytest.raises(ValueError, match="use-site|target/return"):
        rail.build_static_contract(target_mutation)

    rts_mutation = candidate_with_byte(rail.ADDRESSES["saveGameRts"])
    with pytest.raises(ValueError, match="use-site"):
        rail.build_static_contract(rts_mutation)

    original_hex_mutation = candidate_with_byte(0x20A18)
    with pytest.raises(ValueError, match="original-byte"):
        rail.build_static_contract(original_hex_mutation)

    monkeypatch.setattr(rail, "ROM_SHA256", sha256(_rom().read_bytes()).hexdigest().upper())
    static = rail.build_static_contract(_rom())
    session = tmp_path / "session.bin"
    rail._instrument_session_rom(_rom(), static, session)
    rail._assert_session_readback(session, static["sessionPatches"])
    broken = deepcopy(static)
    broken["sessionPatches"][1]["address"] = broken["sessionPatches"][0]["address"]
    with pytest.raises(ValueError, match="width/overlap"):
        rail._instrument_session_rom(_rom(), broken, tmp_path / "overlap.bin")


def test_map_content_owner_derives_the_external_79_map_domain(tmp_path: Path) -> None:
    domain = rail._external_map_domain()
    assert domain == {
        "minimum": 0,
        "maximum": 78,
        "count": 79,
        "ownerFixture": "tests/fixtures/h2/map-content-static-v1.json",
        "ownerId": "sf2-map-content-static-v1",
        "ownerUpstreamCommit": "c834c652b6862bc5679fd7f69a38a7093206efc6",
    }
    fixture = load_json(rail.MAP_CONTENT_FIXTURE)
    for name, mutate, message in (
        ("id", lambda value: value.update(id="wrong-owner"), "schema validation|provenance"),
        (
            "upstream",
            lambda value: value.update(upstreamCommit="0" * 40),
            "provenance",
        ),
        ("count", lambda value: value["summary"].update(mapCount=78), "count/parity"),
    ):
        drift = deepcopy(fixture)
        mutate(drift)
        path = tmp_path / f"{name}.json"
        path.write_text(json.dumps(drift), encoding="utf-8")
        with pytest.raises(ValueError, match=message):
            rail._external_map_domain(path)
    with pytest.raises(FileNotFoundError):
        rail._external_map_domain(tmp_path / "wrong-owner-path.json")


def _failure_payload() -> dict[str, object]:
    return {
        "owner": rail.OWNER,
        "caseId": rail.CASE_IDS[0],
        "phase": "case-entry",
        "role": "case-entry",
        "actualPc": 0xFF6800,
        "expectedEventPc": 0xFF6800,
        "expectedCallPc": None,
        "expectedTargetPc": None,
        "expectedReturnPc": None,
        "pendingCallback": {
            "active": True,
            "kind": "event",
            "mode": "case-entry",
            "caseIndex": 1,
            "expectedCaseId": rail.CASE_IDS[0],
            "expectedEventPc": 0xFF6800,
            "expectedCallPc": None,
            "expectedTargetPc": None,
            "expectedReturnPc": None,
            "rolesAtPc": ["case-entry"],
            "observedRoles": [],
        },
        "restoration": {
            "scopeArmed": True,
            "currentMap": True,
            "egressMap": True,
            "currentSaveSlot": True,
            "flag399": True,
            "slotSram": True,
            "checksum": True,
            "saveFlags": True,
            "dialoguePortraitScratch": True,
            "generatedRam": True,
            "bootstrapFrame": True,
            "sessionCartPatches": False,
            "callbacksCleared": True,
            "outputRemoved": True,
        },
        "restorationMismatch": None,
        "error": "forced callback failure",
    }


def test_callback_failure_schema_status_and_deterministic_lua_contract(
    tmp_path: Path, monkeypatch: pytest.MonkeyPatch
) -> None:
    rail.assert_lua_role_contract()
    _contract, executable = bizhawk_contract()
    validate_lua_syntax(rail.OBSERVER, executable)
    status = tmp_path / "failure.status.txt"
    payload = _failure_payload()
    status.write_text(rail.STATUS_PREFIX + json.dumps(payload) + "\n", encoding="utf-8")
    assert rail._failure_diagnostic(status, load_json(rail.FIXTURE)) == payload

    invalid = deepcopy(payload)
    invalid["pendingCallback"]["rolesAtPc"] = []  # type: ignore[index]
    status.write_text(rail.STATUS_PREFIX + json.dumps(invalid) + "\n", encoding="utf-8")
    with pytest.raises(ValueError, match="failed schema validation"):
        rail._failure_diagnostic(status, load_json(rail.FIXTURE))

    for key in ("expectedEventPc", "expectedCallPc", "expectedTargetPc", "expectedReturnPc"):
        drift = deepcopy(payload)
        drift[key] = 0x20A02
        status.write_text(rail.STATUS_PREFIX + json.dumps(drift) + "\n", encoding="utf-8")
        with pytest.raises(ValueError, match="pending-state drift"):
            rail._failure_diagnostic(status, load_json(rail.FIXTURE))

    restored_with_mismatch = deepcopy(payload)
    restored_with_mismatch["restorationMismatch"] = {
        "domain": "a6",
        "address": None,
        "expected": 1,
        "actual": 2,
    }
    status.write_text(
        rail.STATUS_PREFIX + json.dumps(restored_with_mismatch) + "\n", encoding="utf-8"
    )
    with pytest.raises(ValueError, match="successful restoration"):
        rail._failure_diagnostic(status, load_json(rail.FIXTURE))

    unreported_mismatch = deepcopy(payload)
    unreported_mismatch["restoration"]["bootstrapFrame"] = False  # type: ignore[index]
    status.write_text(rail.STATUS_PREFIX + json.dumps(unreported_mismatch) + "\n", encoding="utf-8")
    with pytest.raises(ValueError, match="requires first mismatch"):
        rail._failure_diagnostic(status, load_json(rail.FIXTURE))

    for key, value in (
        ("domain", "wrong-domain"),
        ("address", "wrong-address"),
        ("expected", -1),
    ):
        schema_invalid = deepcopy(restored_with_mismatch)
        schema_invalid["restorationMismatch"][key] = value  # type: ignore[index]
        status.write_text(rail.STATUS_PREFIX + json.dumps(schema_invalid) + "\n", encoding="utf-8")
        with pytest.raises(ValueError, match="failed schema validation"):
            rail._failure_diagnostic(status, load_json(rail.FIXTURE))

    source = rail.OBSERVER.read_text(encoding="utf-8")
    assert source.count("event.on_bus_exec(function()") == 1
    assert 'role~="bootstrap-check-sram" and not booted then return end' in source
    assert 'require_mode("save-game-entry")' in source
    assert 'require_mode("save-game-rts")' in source
    assert 'require_mode("fade-entry")' in source
    assert source.index('role=="fade-return"') < source.index('role=="witch-tail-jump"')
    assert "client.exitCode(config.observerFailureContract.exitCode)" in source
    assert "emu.setregister" not in source

    guardless = tmp_path / "guardless.lua"
    guardless.write_text(
        source.replace('require_mode("save-game-entry")', 'require_mode("wrong-mode")', 1),
        encoding="utf-8",
    )
    monkeypatch.setattr(rail, "OBSERVER", guardless)
    with pytest.raises(ValueError, match="pending-mode guard drift"):
        rail.assert_lua_role_contract()


def test_observer_config_recursively_excludes_golden_outputs() -> None:
    fixture = load_json(rail.FIXTURE)
    static = rail.build_static_contract(_rom())
    config = rail._observer_config(fixture, static)
    rail._assert_clean_observer_config(config)
    serialized = json.dumps(config, sort_keys=True)
    for forbidden in ("acceptedObservation", "expectedObservation", '"records"', "chronology"):
        assert forbidden not in serialized
    polluted = deepcopy(config)
    polluted["static"]["nested"] = {"acceptedObservation": {"records": []}}
    with pytest.raises(ValueError, match="output corpus"):
        rail._assert_clean_observer_config(polluted)


def test_status_requires_unique_ordered_milestones() -> None:
    fixture = load_json(rail.FIXTURE)
    expected = rail._expected_milestones(fixture)
    path = rail.repo_path("local/derived/h3/church-save-lifecycle.status.txt")
    path.parent.mkdir(parents=True, exist_ok=True)
    try:
        duplicate = expected + [
            "milestone:callbacks-cleared:0",
            "milestone:observer-finished",
        ]
        path.write_text("\n".join(duplicate) + "\n", encoding="utf-8")
        rail._assert_status(path, fixture)
        missing = expected[1:] + [
            "milestone:callbacks-cleared:0",
            "milestone:observer-finished",
        ]
        path.write_text("\n".join(missing) + "\n", encoding="utf-8")
        with pytest.raises(RuntimeError, match="required milestone"):
            rail._assert_status(path, fixture)

        swapped = expected.copy()
        swapped[3], swapped[4] = swapped[4], swapped[3]
        swapped += ["milestone:callbacks-cleared:0", "milestone:observer-finished"]
        path.write_text("\n".join(swapped) + "\n", encoding="utf-8")
        with pytest.raises(RuntimeError, match="ordered"):
            rail._assert_status(path, fixture)
    finally:
        path.unlink(missing_ok=True)
