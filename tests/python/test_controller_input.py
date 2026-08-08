from __future__ import annotations

import copy
import json
import re
from pathlib import Path

import pytest

from sf2tool.cli import build_parser
from sf2tool.h3 import controller_input
from sf2tool.h3.observer_status import CALLBACK_FAILURE_PREFIX, callback_failure_status
from sf2tool.jsonio import load_json, validate_json


def _fixture() -> dict[str, object]:
    return load_json(controller_input.FIXTURE)


def _static(fixture: dict[str, object]) -> dict[str, object]:
    return controller_input.build_static_contract(
        fixture, controller_input.repo_path("local/upstream/SF2DISASM")
    )


def _write(tmp_path: Path, name: str, value: dict[str, object]) -> Path:
    path = tmp_path / name
    path.write_text(json.dumps(value), encoding="utf-8")
    return path


def test_fixture_is_closed_and_contains_only_raw_sampling_repeat_inputs() -> None:
    fixture = _fixture()
    validate_json(fixture, controller_input.FIXTURE_SCHEMA, owner="controller-input fixture")
    assert fixture["caseOrder"] == [
        "sample-neutral",
        "sample-p1-up-b",
        "sample-p2-c-start",
        "sample-two-port-combined",
        "sample-release",
        "repeat-new-press",
        "repeat-release-repress",
        "repeat-held-threshold-cadence",
    ]
    assert {case["kind"] for case in fixture["cases"]} == {"sample", "repeat"}
    assert all("expected" not in case for case in fixture["cases"])
    static = _static(fixture)
    observed = controller_input.expected_observation(fixture, static)
    validate_json(
        observed, controller_input.OBSERVATION_SCHEMA, owner="controller-input observation"
    )
    assert observed["records"][0] == {
        "id": "sample-neutral",
        "result": {"rawStateBytes": [0, 0, 0, 0]},
    }
    assert observed["records"][3] == {
        "id": "sample-two-port-combined",
        "result": {"rawStateBytes": [68, 68, 34, 34]},
    }
    release_repress = observed["records"][6]["result"]["frames"]
    assert [frame["currentPlayerInput"] for frame in release_repress] == [32, 0, 32]
    held = observed["records"][7]["result"]["frames"]
    assert held[0] == {
        "rawStateBytes": [32, 32, 0, 0],
        "currentPlayerInput": 32,
        "lastPlayerInput": 32,
        "inputRepeatDelayer": 0,
    }
    assert held[23]["currentPlayerInput"] == 0
    assert held[24]["currentPlayerInput"] == 32
    assert held[24]["inputRepeatDelayer"] == 18
    assert held[30]["currentPlayerInput"] == 32


def test_source_context_requires_both_h2_owned_entries_and_forbids_extras() -> None:
    fixture = _fixture()
    for field in ("updatePlayerInputsEntryAddress", "applyZ80BusUpdatesEntryAddress"):
        missing = copy.deepcopy(fixture)
        del missing["sourceContext"][field]
        with pytest.raises(ValueError):
            validate_json(
                missing, controller_input.FIXTURE_SCHEMA, owner="controller-input fixture"
            )
    extra = copy.deepcopy(fixture)
    extra["sourceContext"]["unexpectedAddress"] = 0
    with pytest.raises(ValueError):
        validate_json(extra, controller_input.FIXTURE_SCHEMA, owner="controller-input fixture")


def test_controller_input_cli_has_the_one_launch_timeout_contract() -> None:
    args = build_parser().parse_args(["h3", "controller-input"])
    assert args.command == "h3"
    assert args.h3_command == "controller-input"
    assert args.timeout_seconds == 180


def test_runtime_config_has_exact_callback_triples_without_expected_results() -> None:
    fixture = _fixture()
    static = _static(fixture)
    assert static["flow"]["applyInputCall"] == (0x09F6, 0x09FA)
    config = controller_input.observer_config(fixture, static)
    assert config["static"]["flow"]["applyInputCall"] == [0x09F6, 0x09FA]
    assert config["callbackExpectations"] == controller_input.callback_expectations(static)
    assert "expected" not in json.dumps(config["cases"])
    altered = copy.deepcopy(config["callbackExpectations"])
    altered["repeat"][1]["returnAddress"] = 0xFF6824
    with pytest.raises(ValueError, match="callback role expectation drift"):
        controller_input.validate_callback_expectations(static, altered)


def test_static_contract_derives_h2_source_h1_and_callback_flow() -> None:
    fixture = _fixture()
    assert fixture["sourceContext"] == {
        "updatePlayerInputsEntryAddress": 5390,
        "applyZ80BusUpdatesEntryAddress": 2270,
    }
    static = _static(fixture)
    assert static["functionEntries"] == {
        "CheckSram": 28326,
        "UpdatePlayerInputs": 5390,
        "ApplyZ80BusUpdates": 2270,
    }
    assert static["sampling"]["controllerPortStrideBytes"] == 2
    assert static["sampling"]["rawStateBytesPerController"] == 2
    assert static["recognizedButtonMask"] == 255
    assert static["repeat"] == {
        "initialDelayFrames": 24,
        "repeatCadenceFrames": 6,
        "unchangedInputSuppressedBeforeDelay": True,
    }
    assert static["flow"]["applyInputCall"] == (2550, 2554)
    assert static["flow"]["updateRtsPc"] == 5492
    assert static["flow"]["applyRtsPc"] == 2744
    expectations = controller_input.callback_expectations(static)
    assert expectations == {
        "sample": [
            {
                "role": "direct-call",
                "callSiteAddress": 0xFF6820,
                "targetAddress": 5390,
                "returnAddress": 0xFF6826,
            },
            {
                "role": "update-target",
                "callSiteAddress": 0xFF6820,
                "targetAddress": 5390,
                "returnAddress": 0xFF6826,
            },
            {
                "role": "direct-return",
                "callSiteAddress": 0xFF6820,
                "targetAddress": 5390,
                "returnAddress": 0xFF6826,
            },
        ],
        "repeat": [
            {
                "role": "direct-call",
                "callSiteAddress": 0xFF6820,
                "targetAddress": 2270,
                "returnAddress": 0xFF6826,
            },
            {
                "role": "apply-target",
                "callSiteAddress": 0xFF6820,
                "targetAddress": 2270,
                "returnAddress": 0xFF6826,
            },
            {
                "role": "source-call",
                "callSiteAddress": 2550,
                "targetAddress": 5390,
                "returnAddress": 2554,
            },
            {
                "role": "update-target",
                "callSiteAddress": 2550,
                "targetAddress": 5390,
                "returnAddress": 2554,
            },
            {
                "role": "source-return",
                "callSiteAddress": 2550,
                "targetAddress": 5390,
                "returnAddress": 2554,
            },
            {
                "role": "direct-return",
                "callSiteAddress": 0xFF6820,
                "targetAddress": 2270,
                "returnAddress": 0xFF6826,
            },
        ],
    }
    for mutation in ("missing", "extra", "wrong", "wrong-address"):
        altered = copy.deepcopy(expectations)
        if mutation == "missing":
            altered["repeat"].pop()
        elif mutation == "extra":
            altered["sample"].append(copy.deepcopy(altered["sample"][0]))
        elif mutation == "wrong":
            altered["repeat"][1]["role"] = "wrong-role"
        else:
            altered["repeat"][1]["returnAddress"] = 0xFF6824
        with pytest.raises(ValueError, match="callback role expectation drift"):
            controller_input.validate_callback_expectations(static, altered)


@pytest.mark.parametrize(
    ("mutation", "error"),
    (
        ("provenance", "provenance"),
        ("rom-identity", "provenance"),
        ("address", "H2 constants"),
        ("source-context", "source context"),
        ("source-context-interrupt", "source context"),
        ("mask", "H2 masks"),
        ("repeat-threshold", "repeat derivation"),
        ("repeat-cadence", "repeat derivation"),
    ),
)
def test_wrong_but_structurally_valid_h2_facts_fail_before_runtime(
    tmp_path: Path, mutation: str, error: str
) -> None:
    fixture = _fixture()
    services = load_json(controller_input.H2_SERVICES_FIXTURE)
    interrupts = load_json(controller_input.H2_INTERRUPTS_FIXTURE)
    if mutation == "provenance":
        services["upstreamCommit"] = "0" * 40
    elif mutation == "rom-identity":
        fixture["romSha256"] = "0" * 64
    elif mutation == "address":
        services["expected"]["inputFacts"]["constants"]["addresses"]["DATA2"] += 2
    elif mutation == "source-context":
        fixture["sourceContext"]["updatePlayerInputsEntryAddress"] += 2
    elif mutation == "source-context-interrupt":
        fixture["sourceContext"]["applyZ80BusUpdatesEntryAddress"] += 2
    elif mutation == "mask":
        services["expected"]["inputFacts"]["constants"]["buttonMasks"]["INPUT_B"] = 17
    elif mutation == "repeat-threshold":
        interrupts["expected"]["interruptFacts"]["inputRepeat"]["initialDelayFrames"] = 23
    elif mutation == "repeat-cadence":
        interrupts["expected"]["interruptFacts"]["inputRepeat"]["repeatCadenceFrames"] = 5
    else:
        raise AssertionError(mutation)
    validate_json(fixture, controller_input.FIXTURE_SCHEMA, owner="controller-input fixture")
    with pytest.raises(ValueError, match=error):
        controller_input.build_static_contract(
            fixture,
            controller_input.repo_path("local/upstream/SF2DISASM"),
            h2_services_fixture_path=_write(tmp_path, "services.json", services),
            h2_interrupts_fixture_path=_write(tmp_path, "interrupts.json", interrupts),
        )


def test_comment_near_miss_and_branch_polarity_mutations_fail_source_guard() -> None:
    fixture = _fixture()
    upstream = controller_input.repo_path("local/upstream/SF2DISASM")
    input_source = (upstream / "disasm" / controller_input.INPUT_SOURCE).read_text(encoding="utf-8")
    interrupts = (upstream / "disasm" / controller_input.INTERRUPT_SOURCE).read_text(
        encoding="utf-8"
    )
    commented = input_source.replace("bsr.s   @loc_1", "; bsr.s   @loc_1", 1)
    with pytest.raises(ValueError, match="source operation drift"):
        controller_input.build_static_contract(fixture, upstream, input_source_text=commented)
    near_miss = input_source.replace("bsr.s   @loc_1", "bsr.s   @loc_10", 1)
    with pytest.raises(ValueError, match="source operation drift"):
        controller_input.build_static_contract(fixture, upstream, input_source_text=near_miss)
    polarity = interrupts.replace("bcc.s   @IgnoreInput", "bcs.s   @IgnoreInput", 1)
    with pytest.raises(ValueError, match="source operation drift"):
        controller_input.build_static_contract(fixture, upstream, interrupt_source_text=polarity)


def test_h1_nested_callback_flow_mutation_fails_before_runtime() -> None:
    fixture = _fixture()
    upstream = controller_input.repo_path("local/upstream/SF2DISASM")
    listing = (upstream / controller_input.LISTING).read_text(encoding="utf-8")
    altered = listing.replace("bsr.w   UpdatePlayerInputs", "bsr.w   UpdatePlayerInputz", 1)
    with pytest.raises(ValueError, match="H1 guard expected one"):
        controller_input.build_static_contract(fixture, upstream, listing_text=altered)


def test_apply_entry_h1_and_rom_first_instruction_guards_fail_before_runtime(
    tmp_path: Path,
) -> None:
    fixture = _fixture()
    upstream = controller_input.repo_path("local/upstream/SF2DISASM")
    listing = (upstream / controller_input.LISTING).read_text(encoding="utf-8")
    altered_listing = listing.replace(
        "000008DE                            ApplyZ80BusUpdates:",
        "000008E0                            ApplyZ80BusUpdates:",
        1,
    )
    with pytest.raises(ValueError, match="H2/H1 entry derivation drift"):
        controller_input.build_static_contract(fixture, upstream, listing_text=altered_listing)

    rom = bytearray(controller_input.repo_path("local/roms/sf2-us.bin").read_bytes())
    rom[fixture["sourceContext"]["applyZ80BusUpdatesEntryAddress"]] ^= 0x01
    altered_rom = tmp_path / "apply-entry-drift.bin"
    altered_rom.write_bytes(rom)
    with pytest.raises(
        ValueError, match="H1/ROM first-instruction guard drift: ApplyZ80BusUpdates"
    ):
        controller_input.validate_static_contract(fixture, altered_rom, upstream)


def test_fixture_mutations_reject_extra_output_and_wrong_case_order() -> None:
    fixture = _fixture()
    extra = copy.deepcopy(fixture)
    extra["cases"][0]["expected"] = {"rawStateBytes": [0, 0, 0, 0]}
    with pytest.raises(ValueError):
        validate_json(extra, controller_input.FIXTURE_SCHEMA, owner="controller-input fixture")
    wrong_order = copy.deepcopy(fixture)
    wrong_order["caseOrder"][0], wrong_order["caseOrder"][1] = (
        wrong_order["caseOrder"][1],
        wrong_order["caseOrder"][0],
    )
    with pytest.raises(ValueError):
        validate_json(
            wrong_order, controller_input.FIXTURE_SCHEMA, owner="controller-input fixture"
        )
    observed = controller_input.expected_observation(fixture, _static(fixture))
    observed["records"][0], observed["records"][1] = (
        observed["records"][1],
        observed["records"][0],
    )
    with pytest.raises(ValueError):
        validate_json(
            observed, controller_input.OBSERVATION_SCHEMA, owner="controller-input observation"
        )


def _callback_failure_payload() -> dict[str, object]:
    return {
        "owner": "controller-input",
        "caseId": "sample-neutral",
        "phase": "direct-call",
        "role": "direct-call",
        "actualPc": 0xFF6820,
        "expectedCallPc": 0xFF6820,
        "expectedTargetPc": 0x150E,
        "expectedReturnPc": 0xFF6826,
        "pendingCallback": {
            "active": True,
            "caseIndex": 0,
            "frameIndex": 0,
            "expectedFunctionPc": 0x150E,
            "pendingReturnPc": 0xFF6826,
            "rolesAtPc": ["direct-call"],
        },
        "error": "direct input call target drift",
    }


def _registration_failure_payload() -> dict[str, object]:
    return {
        "owner": "controller-input",
        "caseId": None,
        "phase": "registration",
        "role": "registration",
        "actualPc": None,
        "expectedCallPc": None,
        "expectedTargetPc": None,
        "expectedReturnPc": None,
        "pendingCallback": {
            "active": False,
            "caseIndex": 0,
            "frameIndex": 0,
            "expectedFunctionPc": None,
            "pendingReturnPc": None,
            "rolesAtPc": [],
        },
        "error": "probe registration write drift",
    }


@pytest.mark.parametrize(
    "mutation",
    (
        "missing",
        "extra",
        "wrong-role",
        "missing-pending-field",
        "extra-pending-field",
        "wrong-pending-role",
    ),
)
def test_callback_failure_schema_closes_role_expectations(mutation: str) -> None:
    payload = _callback_failure_payload()
    if mutation == "missing":
        del payload["role"]
    elif mutation == "extra":
        payload["unexpected"] = True
    elif mutation == "wrong-role":
        payload["role"] = "wrong-role"
    elif mutation == "missing-pending-field":
        del payload["pendingCallback"]["pendingReturnPc"]  # type: ignore[index]
    elif mutation == "extra-pending-field":
        payload["pendingCallback"]["unexpected"] = True  # type: ignore[index]
    elif mutation == "wrong-pending-role":
        payload["pendingCallback"]["rolesAtPc"] = ["wrong-role"]  # type: ignore[index]
    else:
        raise AssertionError(mutation)
    with pytest.raises(ValueError):
        validate_json(payload, controller_input.FAILURE_SCHEMA, owner="controller-input failure")


def test_registration_failure_has_closed_null_pending_shape_and_runtime_roles_are_admitted(
) -> None:
    registration = _registration_failure_payload()
    validate_json(
        registration, controller_input.FAILURE_SCHEMA, owner="controller-input registration failure"
    )
    assert registration["pendingCallback"] == {
        "active": False,
        "caseIndex": 0,
        "frameIndex": 0,
        "expectedFunctionPc": None,
        "pendingReturnPc": None,
        "rolesAtPc": [],
    }
    shared = load_json(
        controller_input.repo_path("schemas/h3/observer-callback-contract.schema.json")
    )
    allowed = set(shared["definitions"]["controllerInputFailure"]["properties"]["role"]["enum"])
    source = controller_input.OBSERVER.read_text(encoding="utf-8")
    runtime_roles = set(
        re.findall(
            r'(?:local )?current_phase,current_role,(?:current_pc,)?'
            r'current_expectation="[^"]+","([^"]+)"',
            source,
        )
    )
    assert runtime_roles == {
        "registration",
        "bootstrap-return-redirect",
        "direct-input-probe",
        "direct-call",
        "apply-target",
        "source-call",
        "update-target",
        "source-return",
        "direct-return",
    }
    assert runtime_roles <= allowed


def test_verifier_promotes_append_status_callback_failure(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    fixture = _fixture()
    static = _static(fixture)
    status_root = tmp_path / "derived"
    status_root.mkdir()
    payload = _callback_failure_payload()
    (status_root / "controller-input.status.txt").write_text(
        "milestone:observer-loaded\n" + CALLBACK_FAILURE_PREFIX + json.dumps(payload) + "\n",
        encoding="utf-8",
    )

    def fail_observer(**_: object) -> dict[str, object]:
        raise RuntimeError("BizHawk observation failed with exit code 1")

    monkeypatch.setattr(controller_input, "DERIVED_ROOT", status_root)
    monkeypatch.setattr(controller_input, "verify_runtime_contract", lambda *_: None)
    monkeypatch.setattr(controller_input, "validate_static_contract", lambda *_: static)
    monkeypatch.setattr(controller_input, "run_observer", fail_observer)
    with pytest.raises(RuntimeError, match="controller-input observer callback failure") as error:
        controller_input.verify_controller_input(
            tmp_path / "input.bin", tmp_path, timeout_seconds=1
        )
    message = str(error.value)
    for expected in (
        "'caseId': 'sample-neutral'",
        "'phase': 'direct-call'",
        "'role': 'direct-call'",
        "'actualPc': 16738336",
        "'expectedCallPc': 16738336",
        "'expectedTargetPc': 5390",
        "'expectedReturnPc': 16738342",
        "'pendingCallback': {'active': True, 'caseIndex': 0, 'frameIndex': 0",
    ):
        assert expected in message


def test_append_log_callback_failure_is_structured_terminal_and_observer_is_coalesced(
    tmp_path: Path,
) -> None:
    payload = _callback_failure_payload()
    status = tmp_path / "controller-input.status.txt"
    status.write_text(
        "milestone:observer-loaded\n" + CALLBACK_FAILURE_PREFIX + json.dumps(payload) + "\n",
        encoding="utf-8",
    )
    assert (
        callback_failure_status(
            status,
            owner=controller_input.OWNER,
            schema_path=controller_input.FAILURE_SCHEMA,
        )
        == payload
    )
    assert controller_input._failure_diagnostic(status) == str(payload)
    assert status.read_text(encoding="utf-8").splitlines() == [
        "milestone:observer-loaded",
        CALLBACK_FAILURE_PREFIX + json.dumps(payload),
    ]
    status.write_text(status.read_text(encoding="utf-8") * 2, encoding="utf-8")
    with pytest.raises(ValueError, match="multiplicity"):
        callback_failure_status(
            status,
            owner=controller_input.OWNER,
            schema_path=controller_input.FAILURE_SCHEMA,
        )
    source = controller_input.OBSERVER.read_text(encoding="utf-8")
    assert source.count("event.on_bus_exec(function()") == 1
    assert "duplicate physical-PC callback role" in source
    assert (
        "for _,entry in ipairs(callbacks[address]) do dispatch(address,entry.role,entry.index) end"
        in source
    )
    assert "direct input probe JSR/loop write drift" in source
    assert "local callback_expectations=config.callbackExpectations" in source
    assert "entry.callSiteAddress" in source
    assert "entry.targetAddress" in source
    assert "entry.returnAddress" in source
    assert "not direct_call_seen then return end" in source
    assert "bootstrap_armed=true" in source
    assert "bootstrapped=true" in source
    assert source.index("bootstrap_armed=true") < source.index("bootstrapped=true")
    assert "local function arm_step()" in source
    assert "direct input probe gate arm drift" in source
    assert "direct input probe gate pause drift" in source
    assert "emu.setregister" not in source
    for role in (
        '"apply-target","apply-target"',
        '"source-call","source-call"',
        '"update-target","update-target"',
        '"source-return","source-return"',
    ):
        assert role in source
    assert "local loop_ok,loop_message=pcall(function()" in source
    assert "if not loop_ok then fail_callback(loop_message) end" in source
    for milestone in (
        "milestone:observer-loaded",
        "milestone:direct-input-probe",
        "milestone:callbacks-cleared:0",
        "milestone:observer-finished",
    ):
        assert milestone in source
