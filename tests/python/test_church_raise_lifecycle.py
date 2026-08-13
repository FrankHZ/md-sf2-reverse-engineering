from __future__ import annotations

import copy
import json
from pathlib import Path

import pytest

from sf2tool.h3 import church_raise_lifecycle as rail
from sf2tool.jsonio import load_json, validate_json


def _static() -> dict[str, object]:
    return rail.build_static_contract(rail.repo_path("local/roms/sf2-us.bin"))


def test_static_fixture_and_seven_case_golden_are_closed() -> None:
    fixture = load_json(rail.FIXTURE)
    static = _static()
    validate_json(fixture, rail.FIXTURE_SCHEMA, owner=rail.OWNER)
    rail._assert_fixture(fixture, static)
    observed = rail.expected_observation(fixture, static)
    validate_json(observed, rail.OBSERVATION_SCHEMA, owner=rail.OWNER)
    assert fixture["acceptedObservation"] == observed
    assert [record["goldAfter"] for record in observed["records"]] == [999, 50, 69, 0, 0, 279, 0]
    assert observed["records"][6]["deadMemberCount"] == 2
    assert observed["records"][6]["mutations"] == [{"memberId": 8, "cost": 60, "hpAfter": 45}]


def test_schemas_are_closed_structures_not_the_seven_case_golden() -> None:
    fixture_schema = rail.FIXTURE_SCHEMA.read_text(encoding="utf-8")
    observation_schema = rail.OBSERVATION_SCHEMA.read_text(encoding="utf-8")
    failure_schema = rail.FAILURE_SCHEMA.read_text(encoding="utf-8")
    for case_id in rail.CASE_IDS:
        assert case_id not in fixture_schema
        assert case_id not in observation_schema
        assert case_id not in failure_schema
    assert '"callbacksCleared":{"type":"boolean"}' in observation_schema
    assert '"gold":{"type":"boolean"}' in observation_schema


def test_static_guards_reject_branch_and_golden_drift() -> None:
    fixture = load_json(rail.FIXTURE)
    static = _static()
    broken = copy.deepcopy(static)
    broken["cost"]["promotedExtra"] = 199
    with pytest.raises(ValueError, match="static golden"):
        rail._assert_fixture(fixture, broken)
    broken = copy.deepcopy(fixture)
    broken["cases"][3]["gold"] = 69
    with pytest.raises(ValueError, match="exact seven-case"):
        rail._assert_fixture(broken, static)


def test_schema_valid_coordinated_case_and_observation_drift_cannot_reach_launch() -> None:
    fixture = load_json(rail.FIXTURE)
    static = _static()
    fixture["cases"][3]["gold"] = 69
    fixture["acceptedObservation"] = rail.expected_observation(fixture, static)
    validate_json(fixture, rail.FIXTURE_SCHEMA, owner=rail.OWNER)
    with pytest.raises(ValueError, match="exact seven-case"):
        rail._assert_fixture(fixture, static)

    source_relation_drift = copy.deepcopy(static)
    source_relation_drift["regularBaseClasses"].append(12)
    with pytest.raises(ValueError, match="promoted/class"):
        rail._assert_fixture(load_json(rail.FIXTURE), source_relation_drift)


def test_observer_config_omits_accepted_output_corpus() -> None:
    fixture = load_json(rail.FIXTURE)
    config = rail._observer_config(fixture, _static())
    assert set(config) == {
        "caseOrder",
        "cases",
        "sourceContext",
        "static",
        "owner",
        "observerFailureContract",
    }

    def keys(value: object) -> set[str]:
        if isinstance(value, dict):
            return set(value) | set().union(*(keys(item) for item in value.values()))
        if isinstance(value, list):
            return set().union(*(keys(item) for item in value))
        return set()

    assert not keys(config) & {
        "acceptedObservation",
        "expectedObservation",
        "records",
        "mutations",
        "successChronology",
    }


def test_static_h1_rom_helper_and_session_patch_mutations_fail_closed(tmp_path: Path) -> None:
    canonical = rail.repo_path("local/roms/sf2-us.bin")
    static = _static()
    for address in (
        0x20A3E,  # original BRA @CheckRaiseAction displacement
        0x20B14,  # j_DecreaseGold absolute call target
        static["aliases"]["jDecreaseGold"]["address"] + 2,
        static["aliases"]["jDecreaseGold"]["return"],
        0x20B36,  # UpdateAllyMapsprite BSR target
    ):
        mutated = tmp_path / f"helper-{address:X}.bin"
        payload = bytearray(canonical.read_bytes())
        payload[address] ^= 1
        mutated.write_bytes(payload)
        with pytest.raises(ValueError):
            rail.build_static_contract(mutated)

    categories: dict[tuple[int, str], dict[str, object]] = {}
    for patch in static["sessionPatches"]:
        categories.setdefault((patch["width"], patch["originalHex"][:4]), patch)
    assert len(categories) == 5
    for patch in categories.values():
        mutated = tmp_path / f"patch-{patch['address']:X}.bin"
        payload = bytearray(canonical.read_bytes())
        payload[patch["address"]] ^= 1
        mutated.write_bytes(payload)
        with pytest.raises(ValueError, match="source/H1/ROM original-byte drift"):
            rail.preflight_church_raise_lifecycle(mutated)
    assert canonical.read_bytes() == rail.repo_path("local/roms/sf2-us.bin").read_bytes()


def test_lua_defers_generated_ram_emission_until_bootstrap_snapshot() -> None:
    rail.assert_lua_role_contract()
    source = rail.OBSERVER.read_text(encoding="utf-8")
    static = _static()
    registration = source.index("register_callbacks();register(h.checkSram")
    bootstrap = source.index('if role=="bootstrap-check-sram"')
    assert "write_harness()" not in source[registration:bootstrap]
    snapshot = source.index("generated_snapshots={};for _,span in ipairs")
    assert snapshot < source.index("write_harness()", snapshot)
    assert source.count("generatedStubBytes") == 2
    assert "targetsLength=u16(s.ram.targetsListLength)" in source
    assert "portrait=u16(s.ram.currentPortrait)" in source
    assert "local saved=bootstrap_frame" in source
    assert "case_frame=" not in source
    assert "roles_json(pc(),role)" in source
    h = static["harness"]
    spans = (
        range(h["harnessBase"], h["harnessBase"] + h["generatedHarnessBytes"]),
        range(h["actionStub"], h["actionStub"] + h["generatedStubBytes"]),
        range(h["promptStub"], h["promptStub"] + h["generatedStubBytes"]),
        range(h["terminalStub"], h["terminalStub"] + h["generatedTerminalBytes"]),
    )
    assert not set(spans[0]) & set(spans[1])
    assert not set(spans[0]) & set(spans[2])
    assert not set(spans[1]) & set(spans[2])
    assert not set(spans[0]) & set(spans[3])
    assert not set(spans[1]) & set(spans[3])
    assert not set(spans[2]) & set(spans[3])
    assert h["targetsSnapshotBytes"] == 3
    assert "terminal-finalize" in source
    assert "final_ready=true end" in source
    assert 'emu.setregister("M68K A7"' not in source
    assert "w16(h.terminalStub,0x2C7C)" in source


def test_lua_failure_places_restoration_mismatch_at_payload_top_level() -> None:
    source = rail.OBSERVER.read_text(encoding="utf-8")
    restoration = source[
        source.index("local function restoration_json") : source.index(
            "local function restore_case"
        )
    ]
    failure = source[
        source.index("local function failure(") : source.index("local function expect(")
    ]
    assert '"restorationMismatch"' not in restoration
    assert (
        "..',\"restoration\":'..restoration_json(state,generated_ok,frame_ok,#event_ids==0,removed)"
        "..',\"restorationMismatch\":'..restoration_mismatch_json()..',\"error\":'"
    ) in failure

    static = _static()
    seam = static["callbackSeams"]["decreaseGold"]
    payload = {
        "owner": rail.OWNER,
        "caseId": "unpromoted-exact-cost-success",
        "phase": "decrease-gold-entry",
        "role": "decrease-gold-entry",
        "actualPc": seam["target"],
        "expectedCallPc": seam["call"],
        "expectedTargetPc": seam["target"],
        "expectedReturnPc": seam["return"],
        "pendingCallback": {
            "active": True,
            "kind": "helper",
            "caseIndex": 4,
            "expectedCaseId": "unpromoted-exact-cost-success",
            "rolesAtPc": ["decrease-gold-entry"],
            "observedChronology": [static["helperChronology"][0]],
            "expectedChronology": static["helperChronology"],
            "observedChronologyCount": 1,
            "expectedChronologyCount": len(static["helperChronology"]),
        },
        "restoration": {
            "scopeArmed": True,
            "gold": False,
            "combatantRecords": False,
            "mapspriteBytes": False,
            "dialogueScratch": False,
            "targetsListLength": False,
            "targetsListBytes": False,
            "currentPortrait": False,
            "generatedRam": False,
            "a6a7Balance": False,
            "sessionCartPatches": False,
            "callbacksCleared": True,
            "outputRemoved": True,
        },
        "restorationMismatch": {
            "domain": "gold",
            "address": static["ram"]["currentGold"],
            "expected": 99,
            "actual": 0,
        },
        "error": "forced callback failure",
    }
    validate_json(payload, rail.FAILURE_SCHEMA, owner="church raise Lua failure payload")
    assert "restorationMismatch" not in payload["restoration"]
    nested = copy.deepcopy(payload)
    nested["restoration"]["restorationMismatch"] = nested.pop("restorationMismatch")
    with pytest.raises(ValueError, match="restorationMismatch|Additional properties"):
        validate_json(nested, rail.FAILURE_SCHEMA, owner="nested church raise restoration mismatch")


def test_failure_diagnostic_rejects_stale_pc_and_unverified_session_claim(tmp_path: Path) -> None:
    static = _static()
    seam = static["callbackSeams"]["decreaseGold"]
    payload = {
        "owner": rail.OWNER,
        "caseId": "unpromoted-exact-cost-success",
        "phase": "decrease-gold-entry",
        "role": "decrease-gold-entry",
        "actualPc": seam["target"],
        "expectedCallPc": seam["call"],
        "expectedTargetPc": seam["target"],
        "expectedReturnPc": seam["return"],
        "pendingCallback": {
            "active": True,
            "kind": "helper",
            "caseIndex": 4,
            "expectedCaseId": "unpromoted-exact-cost-success",
            "rolesAtPc": ["decrease-gold-entry"],
            "observedChronology": [static["helperChronology"][0]],
            "expectedChronology": static["helperChronology"],
            "observedChronologyCount": 1,
            "expectedChronologyCount": len(static["helperChronology"]),
        },
        "restorationMismatch": {
            "domain": "gold",
            "address": static["ram"]["currentGold"],
            "expected": 99,
            "actual": 0,
        },
        "restoration": {
            "scopeArmed": True,
            "gold": False,
            "combatantRecords": False,
            "mapspriteBytes": False,
            "dialogueScratch": False,
            "targetsListLength": False,
            "targetsListBytes": False,
            "currentPortrait": False,
            "generatedRam": False,
            "a6a7Balance": False,
            "sessionCartPatches": False,
            "callbacksCleared": True,
            "outputRemoved": True,
        },
        "error": "forced callback failure",
    }
    status = tmp_path / "failure.status.txt"
    status.write_text(rail.STATUS_PREFIX + json.dumps(payload) + "\n", encoding="utf-8")
    assert rail._failure_diagnostic(status, static) == payload

    a6 = copy.deepcopy(payload)
    for key in rail.RESTORATION_CHECK_KEYS:
        a6["restoration"][key] = True
    a6["restoration"]["a6a7Balance"] = False
    a6["restorationMismatch"] = {
        "domain": "a6",
        "address": None,
        "expected": 0xC00004,
        "actual": 0xFF6980,
    }
    status.write_text(rail.STATUS_PREFIX + json.dumps(a6) + "\n", encoding="utf-8")
    assert rail._failure_diagnostic(status, static) == a6

    successful_restore = copy.deepcopy(payload)
    for key in rail.RESTORATION_CHECK_KEYS:
        successful_restore["restoration"][key] = True
    successful_restore["restorationMismatch"] = None
    status.write_text(rail.STATUS_PREFIX + json.dumps(successful_restore) + "\n", encoding="utf-8")
    assert rail._failure_diagnostic(status, static) == successful_restore

    missing_mismatch = copy.deepcopy(successful_restore)
    missing_mismatch["restorationMismatch"] = payload["restorationMismatch"]
    status.write_text(rail.STATUS_PREFIX + json.dumps(missing_mismatch) + "\n", encoding="utf-8")
    with pytest.raises(ValueError, match="successful restoration"):
        rail._failure_diagnostic(status, static)

    false_without_mismatch = copy.deepcopy(payload)
    false_without_mismatch["restorationMismatch"] = None
    status.write_text(
        rail.STATUS_PREFIX + json.dumps(false_without_mismatch) + "\n", encoding="utf-8"
    )
    with pytest.raises(ValueError, match="requires first mismatch"):
        rail._failure_diagnostic(status, static)

    unarmed = copy.deepcopy(successful_restore)
    unarmed["restoration"]["scopeArmed"] = False
    status.write_text(rail.STATUS_PREFIX + json.dumps(unarmed) + "\n", encoding="utf-8")
    assert rail._failure_diagnostic(status, static) == unarmed
    unarmed["restoration"]["gold"] = False
    status.write_text(rail.STATUS_PREFIX + json.dumps(unarmed) + "\n", encoding="utf-8")
    with pytest.raises(ValueError, match="unarmed"):
        rail._failure_diagnostic(status, static)
    stale = copy.deepcopy(payload)
    stale["actualPc"] += 2
    status.write_text(rail.STATUS_PREFIX + json.dumps(stale) + "\n", encoding="utf-8")
    with pytest.raises(ValueError, match="stale actual"):
        rail._failure_diagnostic(status, static)
    unverified = copy.deepcopy(payload)
    unverified["restoration"]["sessionCartPatches"] = True
    status.write_text(rail.STATUS_PREFIX + json.dumps(unverified) + "\n", encoding="utf-8")
    with pytest.raises(ValueError, match="cannot claim"):
        rail._failure_diagnostic(status, static)

    watchdog = copy.deepcopy(payload)
    watchdog.update(
        {
            "phase": "case-watchdog",
            "role": "case-watchdog",
            "actualPc": 0x2090,
            "expectedCallPc": None,
            "expectedTargetPc": None,
            "expectedReturnPc": None,
        }
    )
    watchdog["pendingCallback"].update({"kind": "event", "rolesAtPc": ["case-watchdog"]})
    status.write_text(rail.STATUS_PREFIX + json.dumps(watchdog) + "\n", encoding="utf-8")
    assert rail._failure_diagnostic(status, static) == watchdog

    route = copy.deepcopy(payload)
    route_seam = static["callbackSeams"]["raiseRoute"]
    route.update(
        {
            "phase": "raise-route",
            "role": "raise-route",
            "actualPc": route_seam["target"],
            "expectedCallPc": route_seam["call"],
            "expectedTargetPc": route_seam["target"],
            "expectedReturnPc": route_seam["return"],
        }
    )
    route["pendingCallback"].update(
        {"kind": "route", "rolesAtPc": ["raise-route"], "observedChronology": []}
    )
    status.write_text(rail.STATUS_PREFIX + json.dumps(route) + "\n", encoding="utf-8")
    assert rail._failure_diagnostic(status, static) == route


def test_preflight_instruments_only_a_disposable_session_rom() -> None:
    result = rail.preflight_church_raise_lifecycle(rail.repo_path("local/roms/sf2-us.bin"))
    assert result == {
        "Fixture": "sf2-church-raise-lifecycle-runtime-v1",
        "Cases": 7,
        "SessionPatches": 15,
        "Status": "PRELAUNCH-PASS",
    }


def test_runtime_rejection_removes_observed_output_and_session(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    fixture = load_json(rail.FIXTURE)
    sessions: list[Path] = []
    real_instrument = rail._instrument_session_rom

    def instrument(rom: Path, static: dict[str, object], destination: Path) -> None:
        sessions.append(destination)
        real_instrument(rom, static, destination)

    def fake_observer(*, config: dict[str, object], **_: object) -> dict[str, object]:
        observed = rail.expected_observation(fixture, config["static"])
        observed["records"][0]["goldAfter"] = 998
        rail.OBSERVED_OUTPUT.parent.mkdir(parents=True, exist_ok=True)
        rail.OBSERVED_OUTPUT.write_text(json.dumps(observed), encoding="utf-8")
        lines = [
            "milestone:observer-loaded",
            "milestone:direct-function-probe-armed",
            "milestone:direct-function-probe",
        ]
        for case_id in rail.CASE_IDS:
            lines.extend(
                (
                    f"milestone:case-entry:{case_id}",
                    "milestone:church-entry",
                    "milestone:raise-route",
                )
            )
        lines.extend(
            (
                "milestone:do-raise:mixed-decline-then-success",
                "milestone:callbacks-cleared:0",
                "milestone:observer-finished",
            )
        )
        rail.repo_path(f"local/derived/h3/{rail.OWNER}.status.txt").write_text(
            "\n".join(lines) + "\n", encoding="utf-8"
        )
        return observed

    monkeypatch.setattr(rail, "_instrument_session_rom", instrument)
    monkeypatch.setattr(rail, "run_observer", fake_observer)
    with pytest.raises(ValueError, match="runtime observation mismatch"):
        rail.verify_church_raise_lifecycle(
            rail.repo_path("local/roms/sf2-us.bin"), timeout_seconds=1
        )
    assert not rail.OBSERVED_OUTPUT.exists()
    assert sessions and not sessions[0].exists()


def test_status_rejects_duplicate_case_route_and_missing_final_callback(tmp_path: Path) -> None:
    lines = [
        "milestone:observer-loaded",
        "milestone:direct-function-probe-armed",
        "milestone:direct-function-probe",
    ]
    for case_id in rail.CASE_IDS:
        lines.extend(
            (f"milestone:case-entry:{case_id}", "milestone:church-entry", "milestone:raise-route")
        )
    lines.extend(
        (
            "milestone:do-raise:mixed-decline-then-success",
            "milestone:callbacks-cleared:0",
            "milestone:observer-finished",
        )
    )
    status = tmp_path / "church.status.txt"
    status.write_text("\n".join(lines) + "\n", encoding="utf-8")
    rail._assert_church_status(status)

    duplicate = [*lines]
    duplicate.insert(4, "milestone:church-entry")
    status.write_text("\n".join(duplicate) + "\n", encoding="utf-8")
    with pytest.raises(RuntimeError, match="multiplicity"):
        rail._assert_church_status(status)

    missing_final = [*lines]
    missing_final[-3] = "milestone:raise-route"
    status.write_text("\n".join(missing_final) + "\n", encoding="utf-8")
    with pytest.raises(RuntimeError, match="final callback"):
        rail._assert_church_status(status)
