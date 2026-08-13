"""Contract and adversarial tests for the isolated Church Cure H3 rail."""

from __future__ import annotations

import json
import shutil
from copy import deepcopy
from pathlib import Path
from typing import Any

import pytest

from sf2tool.h3 import church_cure_lifecycle as rail
from sf2tool.jsonio import load_json, validate_json


def _rom() -> Path:
    return rail.repo_path("local/roms/sf2-us.bin")


def _static() -> dict[str, Any]:
    return rail.build_static_contract(_rom())


def _write(path: Path, value: dict[str, Any]) -> None:
    path.write_text(json.dumps(value), encoding="utf-8")


def _failure_payload(static: dict[str, Any], fixture: dict[str, Any]) -> dict[str, Any]:
    case = fixture["cases"][0]
    event_pc = static["harness"]["harnessBase"]
    return {
        "owner": rail.OWNER,
        "caseId": case["caseId"],
        "phase": "case-entry",
        "role": "case-entry",
        "actualPc": event_pc,
        "expectedEventPc": event_pc,
        "expectedCallPc": None,
        "expectedTargetPc": None,
        "expectedReturnPc": None,
        "pendingCallback": {
            "active": False,
            "kind": "event",
            "caseIndex": 1,
            "expectedCaseId": case["caseId"],
            "memberId": case["member"]["memberId"],
            "expectedEventPc": event_pc,
            "expectedCallPc": None,
            "expectedTargetPc": None,
            "expectedReturnPc": None,
            "rolesAtPc": ["case-entry"],
            "family": None,
            "observedRoles": [],
        },
        "restoration": {
            "scopeArmed": True,
            "gold": True,
            "combatantRecords": True,
            "targetsListLength": True,
            "targetsListBytes": True,
            "dialogueScratch": True,
            "currentPortrait": True,
            "generatedRam": True,
            "a6a7Balance": True,
            "sessionCartPatches": False,
            "callbacksCleared": True,
            "outputRemoved": True,
        },
        "restorationMismatch": None,
        "error": "forced callback failure",
    }


def _failure_status(path: Path, payload: dict[str, Any]) -> None:
    path.write_text(rail.STATUS_PREFIX + json.dumps(payload) + "\n", encoding="utf-8")


def _static_surface(tmp_path: Path) -> Path:
    """Copy only the source/H1 files consumed by the static parser."""
    root = tmp_path / "SF2DISASM"
    for relative in (
        rail.CHURCH,
        rail.CHURCH_HELPER,
        rail.STATS,
        rail.ITEMS,
        rail.UPDATE,
        rail.ITEM_DEFINITIONS,
        rail.ENUMS,
        rail.CONSTANTS,
        Path("build/sf2build-h1.lst"),
    ):
        source = rail.UPSTREAM / "disasm" / relative
        if relative.parts[0] == "build":
            source = rail.UPSTREAM / relative
        destination = root / ("disasm" / relative if relative.parts[0] != "build" else relative)
        destination.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(source, destination)
    return root


def _build_without_identity(
    monkeypatch: pytest.MonkeyPatch, rom: Path, source: Path
) -> dict[str, Any]:
    monkeypatch.setattr(rail, "_assert_input_identity", lambda *_: None)
    return rail.build_static_contract(rom, source)


def test_fixture_has_the_exact_eleven_case_matrix_and_model() -> None:
    fixture = load_json(rail.FIXTURE)
    static = _static()
    validate_json(fixture, rail.FIXTURE_SCHEMA, owner=rail.OWNER)
    rail._assert_fixture(fixture, static)
    observed = rail.expected_observation(fixture, static)
    validate_json(observed, rail.OBSERVATION_SCHEMA, owner=rail.OWNER)
    assert observed["caseOrder"] == list(rail.CASE_IDS)
    assert observed["records"][-1]["goldAfter"] == 0
    assert [item["family"] for item in observed["records"][-1]["mutations"]] == [
        "poison",
        "stun",
        "curse",
    ]


def test_schema_is_structural_and_recursively_closed() -> None:
    fixture_schema = rail.FIXTURE_SCHEMA.read_text(encoding="utf-8")
    observation_schema = rail.OBSERVATION_SCHEMA.read_text(encoding="utf-8")
    for case_id in rail.CASE_IDS:
        assert case_id not in fixture_schema
        assert case_id not in observation_schema
    fixture = load_json(rail.FIXTURE)
    extra = deepcopy(fixture)
    extra["cases"][0]["member"]["extra"] = 1
    with pytest.raises(ValueError, match="member"):
        validate_json(extra, rail.FIXTURE_SCHEMA, owner=rail.OWNER)
    observed = rail.expected_observation(fixture, _static())
    extra_observation = deepcopy(observed)
    extra_observation["records"][0]["mutations"] = [
        {
            "family": "poison",
            "cost": 10,
            "statusAfter": 0,
            "itemSlotsAfter": [127, 127, 127, 127],
            "extra": True,
        }
    ]
    with pytest.raises(ValueError, match="mutation"):
        validate_json(extra_observation, rail.OBSERVATION_SCHEMA, owner=rail.OWNER)


@pytest.mark.parametrize(
    ("name", "mutate"),
    (
        ("case-id", lambda value: value["cases"][0].update(caseId="poison-decline")),
        ("order", lambda value: value["caseOrder"].reverse()),
        ("member", lambda value: value["cases"][3]["member"].update(memberId=9)),
        ("status", lambda value: value["cases"][3]["member"].update(statusEffects=1)),
        ("items", lambda value: value["cases"][9]["member"]["items"].__setitem__(0, 0x007F)),
        ("prompt", lambda value: value["cases"][1].update(promptResults=[0])),
        ("gold", lambda value: value["cases"][9].update(gold=4249)),
    ),
)
def test_schema_valid_matrix_drift_rejects_before_observer_launch(
    tmp_path: Path, monkeypatch: pytest.MonkeyPatch, name: str, mutate: Any
) -> None:
    fixture = load_json(rail.FIXTURE)
    mutate(fixture)
    fixture_path = tmp_path / f"{name}.json"
    _write(fixture_path, fixture)
    launched = False

    def unexpected_launch(**_: Any) -> dict[str, Any]:
        nonlocal launched
        launched = True
        raise AssertionError("run_observer must not be reached")

    monkeypatch.setattr(rail, "FIXTURE", fixture_path)
    monkeypatch.setattr(rail, "run_observer", unexpected_launch)
    with pytest.raises(ValueError, match="(ID/order|input matrix)"):
        rail.verify_church_cure_lifecycle(_rom())
    assert not launched


def test_static_prelaunch_rejects_source_h1_rom_and_dark_sword_drift(
    tmp_path: Path, monkeypatch: pytest.MonkeyPatch
) -> None:
    source = _static_surface(tmp_path)
    candidate = tmp_path / "candidate.bin"
    shutil.copy2(_rom(), candidate)
    church = source / "disasm" / rail.CHURCH
    church.write_text(
        church.read_text(encoding="utf-8").replace(
            "CHURCHMENU_CURE_POISON_COST", "CHURCHMENU_CURE_POISON_COST_DRIFT", 1
        ),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="source guard drift"):
        _build_without_identity(monkeypatch, candidate, source)

    source = _static_surface(tmp_path / "h1-target")
    listing = source / "build/sf2build-h1.lst"
    listing.write_text(
        listing.read_text(encoding="utf-8").replace(
            "00020BFA 4EB9 0000 8160                             jsr     j_DecreaseGold",
            "00020BFA 4EB9 0000 8160                             jsr     j_DecreaseCoins",
            1,
        ),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="use-site drift"):
        _build_without_identity(monkeypatch, candidate, source)

    source = _static_surface(tmp_path / "h1-width")
    listing = source / "build/sf2build-h1.lst"
    listing.write_text(
        listing.read_text(encoding="utf-8").replace(
            "00020BFA 4EB9 0000 8160", "00020BFA 4EB9 0000", 1
        ),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="instruction width drift"):
        _build_without_identity(monkeypatch, candidate, source)

    source = _static_surface(tmp_path / "source-dark-sword")
    item_defs = source / "disasm" / rail.ITEM_DEFINITIONS
    item_defs.write_text(
        item_defs.read_text(encoding="utf-8").replace(
            "price        17000", "price        17001", 1
        ),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="Dark Sword source/H1/ROM price drift"):
        _build_without_identity(monkeypatch, candidate, source)

    source = _static_surface(tmp_path / "rom-opcode")
    opcode_rom = tmp_path / "opcode.bin"
    data = bytearray(_rom().read_bytes())
    data[0x20BFA] ^= 1
    opcode_rom.write_bytes(data)
    with pytest.raises(ValueError, match="H1/ROM use-site drift"):
        _build_without_identity(monkeypatch, opcode_rom, source)

    source = _static_surface(tmp_path / "rom-dark-sword")
    static = _build_without_identity(monkeypatch, candidate, source)
    dark_rom = tmp_path / "dark-sword.bin"
    data = bytearray(_rom().read_bytes())
    equates = rail._equates((source / "disasm" / rail.ENUMS).read_text(encoding="utf-8"))
    equates |= rail._equates((source / "disasm" / rail.CONSTANTS).read_text(encoding="utf-8"))
    item_address = (
        rail._listing_label(
            (source / "build/sf2build-h1.lst").read_text(encoding="utf-8"),
            "table_ItemDefinitions",
        )
        + static["constants"]["darkSwordItem"] * equates["ITEMDEF_SIZE"]
        + equates["ITEMDEF_OFFSET_PRICE"]
    )
    data[item_address : item_address + 2] = (17001).to_bytes(2, "big")
    dark_rom.write_bytes(data)
    with pytest.raises(ValueError, match="Dark Sword source/H1/ROM price drift"):
        _build_without_identity(monkeypatch, dark_rom, source)


def test_session_plan_guards_width_overlap_original_readback_and_canonical_immutability(
    tmp_path: Path,
) -> None:
    static = _static()
    canonical_before = _rom().read_bytes()
    session = tmp_path / "session.bin"
    rail._instrument_session_rom(_rom(), static, session)
    assert _rom().read_bytes() == canonical_before
    broken = deepcopy(static)
    broken["sessionPatches"][0]["width"] += 1
    with pytest.raises(ValueError, match="width/overlap"):
        rail._instrument_session_rom(_rom(), broken, tmp_path / "width.bin")
    broken = deepcopy(static)
    broken["sessionPatches"][1]["address"] = broken["sessionPatches"][0]["address"]
    with pytest.raises(ValueError, match="width/overlap"):
        rail._instrument_session_rom(_rom(), broken, tmp_path / "overlap.bin")
    broken = deepcopy(static)
    broken["sessionPatches"][0]["originalHex"] = "00" * broken["sessionPatches"][0]["width"]
    with pytest.raises(ValueError, match="canonical patch guard"):
        rail._instrument_session_rom(_rom(), broken, tmp_path / "original.bin")
    patch = static["sessionPatches"][0]
    content = bytearray(session.read_bytes())
    content[patch["address"]] ^= 1
    session.write_bytes(content)
    with pytest.raises(ValueError, match="readback drift"):
        rail._assert_session_readback(session, static["sessionPatches"])


def test_observer_config_excludes_every_expected_output_corpus() -> None:
    fixture = load_json(rail.FIXTURE)
    config = rail._observer_config(fixture, _static())
    rail._assert_clean_observer_config(config)
    serialized = json.dumps(config, sort_keys=True)
    for forbidden in (
        "acceptedObservation",
        "expectedObservation",
        "records",
        "mutations",
        "successChronology",
    ):
        assert forbidden not in serialized
    polluted = deepcopy(config)
    polluted["static"]["nested"] = {"acceptedObservation": []}
    with pytest.raises(ValueError, match="output corpus"):
        rail._assert_clean_observer_config(polluted)


def test_failure_diagnostic_enforces_schema_roles_event_and_first_false_restoration(
    tmp_path: Path,
) -> None:
    static, fixture = _static(), load_json(rail.FIXTURE)
    payload = _failure_payload(static, fixture)
    status = tmp_path / "failure.status.txt"
    _failure_status(status, payload)
    assert rail._failure_diagnostic(status, static, fixture) == payload

    gold = deepcopy(payload)
    gold["restoration"]["gold"] = False
    gold["restorationMismatch"] = {
        "domain": "gold",
        "address": static["ram"]["currentGold"],
        "expected": 10,
        "actual": 9,
    }
    _failure_status(status, gold)
    assert rail._failure_diagnostic(status, static, fixture) == gold

    generated = deepcopy(payload)
    generated["restoration"]["generatedRam"] = False
    generated["restorationMismatch"] = {
        "domain": "generatedRamByte",
        "address": static["harness"]["harnessBase"],
        "expected": 1,
        "actual": 0,
    }
    _failure_status(status, generated)
    assert rail._failure_diagnostic(status, static, fixture) == generated

    a6 = deepcopy(payload)
    a6["restoration"]["a6a7Balance"] = False
    a6["restorationMismatch"] = {"domain": "a6", "address": None, "expected": 1, "actual": 0}
    _failure_status(status, a6)
    assert rail._failure_diagnostic(status, static, fixture) == a6

    scope = deepcopy(payload)
    scope["restoration"].update(
        scopeArmed=False,
        gold=False,
        combatantRecords=False,
        targetsListLength=False,
        targetsListBytes=False,
        dialogueScratch=False,
        currentPortrait=False,
        generatedRam=False,
        a6a7Balance=False,
    )
    scope["restorationMismatch"] = {"domain": "scope", "address": None, "expected": 1, "actual": 0}
    _failure_status(status, scope)
    assert rail._failure_diagnostic(status, static, fixture) == scope

    unarmed_success = deepcopy(scope)
    unarmed_success["restoration"]["gold"] = True
    _failure_status(status, unarmed_success)
    with pytest.raises(ValueError, match="unarmed scope"):
        rail._failure_diagnostic(status, static, fixture)

    inverted = deepcopy(payload)
    inverted["restorationMismatch"] = {
        "domain": "gold",
        "address": static["ram"]["currentGold"],
        "expected": 1,
        "actual": 0,
    }
    _failure_status(status, inverted)
    with pytest.raises(ValueError, match="must be null"):
        rail._failure_diagnostic(status, static, fixture)

    missing = deepcopy(gold)
    missing["restorationMismatch"] = None
    _failure_status(status, missing)
    with pytest.raises(ValueError, match="missing for first failed"):
        rail._failure_diagnostic(status, static, fixture)

    nested = deepcopy(gold)
    nested["restorationMismatch"]["unexpected"] = True
    _failure_status(status, nested)
    with pytest.raises(ValueError, match="failed schema validation"):
        rail._failure_diagnostic(status, static, fixture)

    for mutator in (
        lambda value: value.pop("expectedEventPc"),
        lambda value: value["pendingCallback"].pop("expectedEventPc"),
        lambda value: value["pendingCallback"].update(rolesAtPc=[]),
        lambda value: value.update(role="not-a-cure-role"),
        lambda value: value.update(phase="not-a-cure-phase"),
    ):
        invalid = deepcopy(payload)
        mutator(invalid)
        _failure_status(status, invalid)
        with pytest.raises(ValueError, match="failed schema validation"):
            rail._failure_diagnostic(status, static, fixture)


def test_status_requires_one_complete_ordered_lifecycle(tmp_path: Path) -> None:
    fixture, static = load_json(rail.FIXTURE), _static()
    expected = rail._expected_milestones(fixture, static)
    status = tmp_path / "success.status.txt"
    status.write_text("\n".join(expected) + "\n", encoding="utf-8")
    rail._assert_status(status, fixture, static)
    for changed in (
        expected[:3] + expected[4:],
        expected[:4] + [expected[3]] + expected[4:],
        expected[:3] + [expected[4], expected[3]] + expected[5:],
    ):
        status.write_text("\n".join(changed) + "\n", encoding="utf-8")
        with pytest.raises(RuntimeError, match="ordered milestone lifecycle drift"):
            rail._assert_status(status, fixture, static)


def test_schema_valid_drifted_runtime_observation_is_removed_after_rejection(
    tmp_path: Path, monkeypatch: pytest.MonkeyPatch
) -> None:
    captured: dict[str, Any] = {}
    session_paths: list[Path] = []
    output = tmp_path / "observed.json"
    output.write_text("stale", encoding="utf-8")
    status_path = rail.repo_path(f"local/derived/h3/{rail.OWNER}.status.txt")
    status_path.parent.mkdir(parents=True, exist_ok=True)
    status_path.unlink(missing_ok=True)
    real_instrument = rail._instrument_session_rom

    def instrument(rom: Path, static: dict[str, Any], session: Path) -> None:
        session_paths.append(session)
        real_instrument(rom, static, session)

    def observe(**kwargs: Any) -> dict[str, Any]:
        captured["config"] = kwargs["config"]
        expected = rail.expected_observation(load_json(rail.FIXTURE), kwargs["config"]["static"])
        status_path.write_text(
            "\n".join(
                rail._expected_milestones(load_json(rail.FIXTURE), kwargs["config"]["static"])
            )
            + "\n",
            encoding="utf-8",
        )
        drift = deepcopy(expected)
        drift["records"][0]["goldAfter"] += 1
        return drift

    monkeypatch.setattr(rail, "OBSERVED_OUTPUT", output)
    monkeypatch.setattr(rail, "_instrument_session_rom", instrument)
    monkeypatch.setattr(rail, "_with_instrumented_rom_database", lambda _a, _b, action: action())
    monkeypatch.setattr(rail, "run_observer", observe)
    with pytest.raises(ValueError, match="runtime observation mismatch"):
        rail.verify_church_cure_lifecycle(_rom())
    assert not output.exists()
    assert session_paths and all(not path.exists() for path in session_paths)
    rail._assert_clean_observer_config(captured["config"])
    assert captured["config"] == rail._observer_config(load_json(rail.FIXTURE), _static())
    status_path.unlink(missing_ok=True)


def test_lua_has_one_dispatcher_and_generated_terminal_restore() -> None:
    rail.assert_lua_role_contract()
    source = rail.OBSERVER.read_text(encoding="utf-8")
    assert source.count("event.on_bus_exec(function()") == 1
    for fragment in (
        "unexpected mutation helper while not pending",
        "w16(h.terminalStub,0x2C7C)",
        "terminal_finalize_executed=true",
        "expectedEventPc",
        "transition watchdog exhausted",
        "callbacks-cleared:0",
        "client.exitCode(config.observerFailureContract.exitCode)",
    ):
        assert fragment in source
    assert "emu.setregister" not in source


def test_preflight_creates_only_a_disposable_session_rom() -> None:
    result = rail.preflight_church_cure_lifecycle(_rom())
    assert result["Cases"] == 11
    assert result["SessionPatches"] > 30
    assert result["Status"] == "PRELAUNCH-PASS"
