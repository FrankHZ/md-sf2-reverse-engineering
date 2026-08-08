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

WAIT_CASES = [
    "wait-player-input-immediate",
    "wait-player-input-delayed",
    "wait-player1-new-input-neutral-press",
    "wait-player1-new-input-release-repress",
    "wait-one-second-early-input",
    "wait-one-second-timeout",
    "wait-three-seconds-early-input",
    "wait-three-seconds-timeout",
]


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


def test_fixture_is_closed_input_only_and_has_the_bounded_sixteen_case_matrix() -> None:
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
        *WAIT_CASES,
    ]
    assert [case["kind"] for case in fixture["cases"]] == [
        *("sample",) * 5,
        *("repeat",) * 3,
        *("wait",) * 8,
    ]
    assert all("expected" not in case for case in fixture["cases"])
    assert all("result" not in case for case in fixture["cases"])
    assert "sub_15A4" not in json.dumps(fixture)


def test_wait_model_derives_the_exact_source_bounded_chronology() -> None:
    fixture = _fixture()
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
    held = observed["records"][7]["result"]["frames"]
    release_repress = observed["records"][6]["result"]["frames"]
    assert [frame["currentPlayerInput"] for frame in release_repress] == [32, 0, 32]
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

    waits = {record["id"]: record["result"] for record in observed["records"][8:]}
    assert [waits[case]["waitForVIntEntryCount"] for case in WAIT_CASES] == [
        0,
        2,
        1,
        2,
        0,
        60,
        0,
        180,
    ]
    assert [waits[case]["waitForVIntReturnCount"] for case in WAIT_CASES] == [
        0,
        2,
        1,
        2,
        0,
        60,
        0,
        180,
    ]
    assert [waits[case]["vIntInputStageCount"] for case in WAIT_CASES] == [
        0,
        2,
        1,
        2,
        0,
        60,
        0,
        180,
    ]
    assert [waits[case]["helperEntryCount"] for case in WAIT_CASES] == [
        1,
        3,
        1,
        2,
        1,
        1,
        1,
        1,
    ]
    assert waits["wait-player-input-delayed"]["frames"][-1]["rawStateBytes"] == [32, 32, 0, 0]
    assert waits["wait-player1-new-input-release-repress"]["frames"][-1]["currentPlayerInput"] == 32
    assert waits["wait-player1-new-input-release-repress"]["frames"][0]["inputRepeatDelayer"] == 0
    assert waits["wait-player-input-delayed"]["frames"][0]["inputRepeatDelayer"] == 1
    assert waits["wait-one-second-timeout"]["frames"][-1]["inputRepeatDelayer"] == 18
    assert waits["wait-three-seconds-timeout"]["frames"][-1]["inputRepeatDelayer"] == 18
    timed_cases = {
        "wait-one-second-early-input",
        "wait-one-second-timeout",
        "wait-three-seconds-early-input",
        "wait-three-seconds-timeout",
    }
    assert all("d5After" not in waits[case] for case in set(WAIT_CASES) - timed_cases)
    assert all(waits[case]["d5After"] == controller_input.WAIT_D5_SENTINEL for case in timed_cases)

    held_release = copy.deepcopy(fixture["cases"][11])
    assert held_release["initial"]["player1Buttons"] == ["C"]
    neutralized = copy.deepcopy(held_release)
    neutralized["initial"]["player1Buttons"] = []
    assert controller_input.model_case(held_release, static)["frames"][0]["inputRepeatDelayer"] == 0
    assert controller_input.model_case(neutralized, static)["frames"][0]["inputRepeatDelayer"] == 1


def test_source_context_is_closed_and_binds_every_new_h1_callback_site() -> None:
    fixture = _fixture()
    context = fixture["sourceContext"]
    expected = {
        "updatePlayerInputsEntryAddress": 5390,
        "applyZ80BusUpdatesEntryAddress": 2270,
        "waitForPlayerInputEntryAddress": 5494,
        "waitForPlayer1NewInputEntryAddress": 5510,
        "waitForInputFor1SecondEntryAddress": 5592,
        "waitForInputFor3SecondsEntryAddress": 5620,
        "waitForVIntEntryAddress": 3822,
        "waitForVIntRtsPc": 3842,
        "waitingNextVIntAddress": 16768759,
        "vIntEntryAddress": 1428,
        "timedWaitLoopEntryAddress": 5598,
        "waitForPlayerInputRtsPc": 5508,
        "waitForPlayer1NewInputRtsPc": 5538,
        "waitForInputFor1SecondRtsPc": 5618,
        "waitForInputFor3SecondsRtsPc": 5618,
        "waitForPlayerInputVIntCallAddress": 5502,
        "waitForPlayerInputVIntReturnAddress": 5506,
        "waitForPlayer1NewInputReleaseVIntCallAddress": 5518,
        "waitForPlayer1NewInputReleaseVIntReturnAddress": 5522,
        "waitForPlayer1NewInputPressVIntCallAddress": 5532,
        "waitForPlayer1NewInputPressVIntReturnAddress": 5536,
        "timedWaitVIntCallAddress": 5606,
        "timedWaitVIntReturnAddress": 5610,
        "waitForInputFor3SecondsLoopBranchAddress": 5630,
        "waitForInputFor3SecondsLoopReturnAddress": 5632,
        "vIntApplyInputCallAddress": 1466,
        "vIntApplyInputReturnAddress": 1470,
    }
    assert context == expected
    for field in expected:
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


def test_research_index_assigns_wait_and_vint_facts_to_their_source_owners() -> None:
    index = load_json(controller_input.repo_path("manifests/research-index.json"))
    records = {record["id"]: record for record in index["records"]}
    fixture_path = "tests/fixtures/h3/controller-input-v1.json"

    def bindings(record_id: str) -> dict[str, str]:
        record = records[record_id]
        evidence = next(item for item in record["evidence"] if item["fixture"] == fixture_path)
        return {item["addressId"]: item["fixtureField"] for item in evidence["bindings"]}

    input_bindings = bindings("tech.services.input")
    assert input_bindings["wait-player-input-entry"] == (
        "sourceContext.waitForPlayerInputEntryAddress"
    )
    assert "wait-for-vint-entry" not in input_bindings
    assert "vint-entry" not in input_bindings
    assert "vint-apply-input-call" not in input_bindings

    assert bindings("tech.interrupts.vint-engine-core") == {
        "wait-for-vint-entry": "sourceContext.waitForVIntEntryAddress",
        "wait-for-vint-rts": "sourceContext.waitForVIntRtsPc",
        "waiting-next-vint": "sourceContext.waitingNextVIntAddress",
    }
    assert bindings("tech.interrupts.vint") == {
        "entry": "sourceContext.vIntEntryAddress",
        "apply-input-call": "sourceContext.vIntApplyInputCallAddress",
        "apply-input-return": "sourceContext.vIntApplyInputReturnAddress",
    }
    assert bindings("tech.interrupts.z80-fade-input") == {
        "entry": "sourceContext.applyZ80BusUpdatesEntryAddress"
    }


def test_controller_input_cli_has_the_one_launch_timeout_contract() -> None:
    args = build_parser().parse_args(["h3", "controller-input"])
    assert (args.command, args.h3_command, args.timeout_seconds) == ("h3", "controller-input", 180)


def test_static_contract_derives_h2_source_h1_rom_callback_flow_and_config_mapping() -> None:
    fixture = _fixture()
    static = _static(fixture)
    assert static["functionEntries"] == {
        "CheckSram": 28326,
        "UpdatePlayerInputs": 5390,
        "ApplyZ80BusUpdates": 2270,
        "WaitForVInt": 3822,
        "VInt": 1428,
        "WaitForPlayerInput": 5494,
        "WaitForPlayer1NewInput": 5510,
        "WaitForInputFor1Second": 5592,
        "WaitForInputFor3Seconds": 5620,
    }
    assert static["recognizedButtonMask"] == 255
    assert static["sampling"]["controllerPortStrideBytes"] == 2
    assert static["sampling"]["rawStateBytesPerController"] == 2
    assert static["repeat"] == {
        "initialDelayFrames": 24,
        "repeatCadenceFrames": 6,
        "unchangedInputSuppressedBeforeDelay": True,
    }
    assert static["waits"] == {
        "recognizedButtonMask": 255,
        "waitForPlayerInputUsesCurrentInput": True,
        "waitForPlayerInputReturnsWhenRecognizedInputIsNonzero": True,
        "waitForPlayer1NewInputRequiresReleaseThenRecognizedPress": True,
        "oneSecondMaximumVintWaits": 60,
        "threeSecondMaximumVintWaits": 180,
        "boundedWaitsReturnEarlyOnRecognizedPlayer1Input": True,
    }
    assert static["flow"] == {
        "applyInputCall": (2550, 2554),
        "updateRtsPc": 5492,
        "applyRtsPc": 2744,
        "waitForVIntRtsPc": 3842,
        "waitingNextVIntAddress": 16768759,
        "waitForVIntWaitingFlagSet": (3830, 3836),
        "vIntWaitingFlagClear": (1502, 1506),
        "waitHelper": {
            "WaitForPlayerInput": {"entry": 5494, "rtsPc": 5508, "vintCalls": [(5502, 5506)]},
            "WaitForPlayer1NewInput": {
                "entry": 5510,
                "rtsPc": 5538,
                "vintCalls": [(5518, 5522), (5532, 5536)],
            },
            "WaitForInputFor1Second": {"entry": 5592, "rtsPc": 5618, "vintCalls": [(5606, 5610)]},
            "WaitForInputFor3Seconds": {
                "entry": 5620,
                "rtsPc": 5618,
                "vintCalls": [(5606, 5610)],
                "loopBranch": (5630, 5632),
            },
        },
        "vIntApplyInput": (1466, 1470),
    }
    expectations = controller_input.callback_expectations(static)
    assert expectations["sample"] == [
        {
            "role": "direct-call",
            "callbackAddress": 0xFF6820,
            "callSiteAddress": 0xFF6820,
            "targetAddress": 5390,
            "returnAddress": 0xFF6826,
        },
        {
            "role": "update-target",
            "callbackAddress": 5390,
            "callSiteAddress": 0xFF6820,
            "targetAddress": 5390,
            "returnAddress": 0xFF6826,
        },
        {
            "role": "direct-return",
            "callbackAddress": 0xFF6826,
            "callSiteAddress": 0xFF6820,
            "targetAddress": 5390,
            "returnAddress": 0xFF6826,
        },
    ]
    assert [
        (
            entry["role"],
            entry["callbackAddress"],
            entry["callSiteAddress"],
            entry["targetAddress"],
            entry["returnAddress"],
            entry.get("flowIndex"),
        )
        for entry in expectations["WaitForPlayer1NewInput"]
    ] == [
        ("direct-call", 0xFF6820, 0xFF6820, 5510, 0xFF6826, None),
        ("wait-helper-target", 5510, 0xFF6820, 5510, 0xFF6826, None),
        ("wait-helper-return", 5538, 0xFF6820, 5510, 0xFF6826, None),
        ("direct-return", 0xFF6826, 0xFF6820, 5510, 0xFF6826, None),
        ("vint-target", 1428, None, None, None, None),
        ("vint-input-call", 1466, 1466, 2270, 1470, None),
        ("vint-input-stage", 2270, 1466, 2270, 1470, None),
        ("vint-input-return", 1470, 1466, 2270, 1470, None),
        ("wait-for-vint-call", 5518, 5518, 3822, 5522, 0),
        ("wait-for-vint-target", 3822, 5518, 3822, 5522, 0),
        ("wait-for-vint-rts", 3842, 5518, 3822, 5522, 0),
        ("wait-for-vint-return", 5522, 5518, 3822, 5522, 0),
        ("wait-for-vint-call", 5532, 5532, 3822, 5536, 1),
        ("wait-for-vint-target", 3822, 5532, 3822, 5536, 1),
        ("wait-for-vint-rts", 3842, 5532, 3822, 5536, 1),
        ("wait-for-vint-return", 5536, 5532, 3822, 5536, 1),
    ]
    config = controller_input.observer_config(fixture, static)
    assert config["static"]["flow"]["vIntApplyInput"] == [1466, 1470]
    assert config["callbackExpectations"] == expectations
    assert config["waitExpectations"]["wait-three-seconds-timeout"]["waitForVIntEntryCount"] == 180
    assert "expected" not in json.dumps(config["cases"])


@pytest.mark.parametrize(
    "mutation",
    (
        "missing",
        "extra",
        "wrong-role",
        "wrong-address",
        "wrong-target-triple",
        "wrong-return-triple",
        "wrong-vint-triple",
    ),
)
def test_callback_expectations_are_exact_and_configured_not_role_only(mutation: str) -> None:
    static = _static(_fixture())
    altered = copy.deepcopy(controller_input.callback_expectations(static))
    entries = altered["WaitForPlayer1NewInput"]
    if mutation == "missing":
        entries.pop()
    elif mutation == "extra":
        entries.append(copy.deepcopy(entries[-1]))
    elif mutation == "wrong-role":
        entries[1]["role"] = "wait-helper-return"
    elif mutation == "wrong-address":
        entries[1]["callbackAddress"] = 5512
    elif mutation == "wrong-target-triple":
        entries[1]["targetAddress"] = 5512
    elif mutation == "wrong-return-triple":
        entries[1]["returnAddress"] = 0xFF6824
    else:
        next(entry for entry in entries if entry["role"] == "vint-target")["targetAddress"] = 1428
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
        ("wait-source-context", "source context"),
        ("waiting-vint-source-context", "WAITING_NEXT_VINT"),
        ("mask", "H2 masks"),
        ("repeat-threshold", "repeat derivation"),
        ("repeat-cadence", "repeat derivation"),
    ),
)
def test_wrong_but_schema_valid_owner_facts_fail_before_runtime(
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
    elif mutation == "wait-source-context":
        fixture["sourceContext"]["waitForPlayer1NewInputPressVIntReturnAddress"] += 2
    elif mutation == "waiting-vint-source-context":
        fixture["sourceContext"]["waitingNextVIntAddress"] += 1
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


@pytest.mark.parametrize(
    ("source_name", "needle", "replacement"),
    (
        ("input", "bne.s   @Return", "beq.s   @Return"),
        ("input", "moveq   #59,d5", "moveq   #58,d5"),
        ("input", "move.l  #179,d5", "move.l  #178,d5"),
        ("input", "dbf     d5,WaitForInput_Loop", "dbt     d5,WaitForInput_Loop"),
        (
            "vint-engine",
            "move.b  #1,((WAITING_NEXT_VINT-$1000000)).w",
            "clr.b   ((WAITING_NEXT_VINT-$1000000)).w",
        ),
        ("vint", "clr.b   ((WAITING_NEXT_VINT-$1000000)).w", "nop"),
    ),
)
def test_wait_source_masks_counters_backedge_and_vint_progression_mutations_fail(
    source_name: str, needle: str, replacement: str
) -> None:
    fixture = _fixture()
    upstream = controller_input.repo_path("local/upstream/SF2DISASM")
    paths = {
        "input": controller_input.INPUT_SOURCE,
        "vint-engine": controller_input.VINT_ENGINE_SOURCE,
        "vint": controller_input.VINT_SOURCE,
    }
    source = (upstream / "disasm" / paths[source_name]).read_text(encoding="utf-8")
    altered = source.replace(needle, replacement, 1)
    kwargs = {
        "input_source_text": altered if source_name == "input" else None,
        "vint_engine_source_text": altered if source_name == "vint-engine" else None,
        "vint_source_text": altered if source_name == "vint" else None,
    }
    with pytest.raises(ValueError, match="source operation drift"):
        controller_input.build_static_contract(fixture, upstream, **kwargs)


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


def test_h1_wait_flow_and_rom_first_instruction_mutations_fail_before_runtime(
    tmp_path: Path,
) -> None:
    fixture = _fixture()
    upstream = controller_input.repo_path("local/upstream/SF2DISASM")
    listing = (upstream / controller_input.LISTING).read_text(encoding="utf-8")
    altered_listing = listing.replace(
        "0000157E 6100 F96E                                  bsr.w   WaitForVInt",
        "0000157E 6100 F96E                                  bsr.w   WaitForVIntz",
        1,
    )
    with pytest.raises(ValueError, match="H1 guard expected one"):
        controller_input.build_static_contract(fixture, upstream, listing_text=altered_listing)

    altered_waiting_operand = listing.replace(
        "00000EF6 11FC 0001 DEF7                             move.b  #1,"
        "((WAITING_NEXT_VINT-$1000000)).w",
        "00000EF6 11FC 0001 DEF8                             move.b  #1,"
        "((WAITING_NEXT_VINT-$1000001)).w",
        1,
    )
    with pytest.raises(ValueError, match="H1 guard expected one"):
        controller_input.build_static_contract(
            fixture, upstream, listing_text=altered_waiting_operand
        )

    const_source = (upstream / "disasm" / controller_input.CONST_SOURCE).read_text(encoding="utf-8")
    altered_const = const_source.replace(
        "WAITING_NEXT_VINT: equ $FFDEF7", "WAITING_NEXT_VINT: equ $FFDEF8", 1
    )
    with pytest.raises(ValueError, match="WAITING_NEXT_VINT"):
        controller_input.build_static_contract(fixture, upstream, const_source_text=altered_const)

    rom = bytearray(controller_input.repo_path("local/roms/sf2-us.bin").read_bytes())
    rom[3830] ^= 0x01
    altered_rom = tmp_path / "wait-for-vint-flag-operand-drift.bin"
    altered_rom.write_bytes(rom)
    with pytest.raises(ValueError, match="H1/ROM operand guard drift: WaitForVInt"):
        controller_input.validate_static_contract(fixture, altered_rom, upstream)


def test_fixture_and_observation_mutations_reject_outputs_order_and_wrong_wait_shape() -> None:
    fixture = _fixture()
    extra = copy.deepcopy(fixture)
    extra["cases"][8]["expected"] = {"waitForVIntEntryCount": 0}
    with pytest.raises(ValueError):
        validate_json(extra, controller_input.FIXTURE_SCHEMA, owner="controller-input fixture")
    bad_wait = copy.deepcopy(fixture)
    bad_wait["cases"][8]["helper"] = "sub_15A4"
    with pytest.raises(ValueError):
        validate_json(bad_wait, controller_input.FIXTURE_SCHEMA, owner="controller-input fixture")
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
    observed = controller_input.expected_observation(fixture, _static(fixture))
    observed["records"][13]["result"]["d5After"] = -1
    with pytest.raises(ValueError):
        validate_json(
            observed, controller_input.OBSERVATION_SCHEMA, owner="controller-input observation"
        )


def _callback_failure_payload() -> dict[str, object]:
    return {
        "owner": "controller-input",
        "caseId": "wait-player-input-delayed",
        "phase": "wait-for-vint-target",
        "role": "wait-for-vint-target",
        "actualPc": 3822,
        "expectedCallPc": 5502,
        "expectedTargetPc": 3822,
        "expectedReturnPc": 5506,
        "pendingCallback": {
            "active": True,
            "caseIndex": 9,
            "frameIndex": 0,
            "expectedFunctionPc": 5494,
            "pendingReturnPc": 0xFF6826,
            "rolesAtPc": ["wait-for-vint-target"],
        },
        "error": "WaitForVInt target PC drift",
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


def _vint_entry_failure_payload() -> dict[str, object]:
    return {
        "owner": "controller-input",
        "caseId": "wait-player-input-delayed",
        "phase": "vint-target",
        "role": "vint-target",
        "actualPc": 1428,
        "expectedCallPc": None,
        "expectedTargetPc": None,
        "expectedReturnPc": None,
        "pendingCallback": {
            "active": True,
            "caseIndex": 9,
            "frameIndex": 0,
            "expectedFunctionPc": 5494,
            "pendingReturnPc": 0xFF6826,
            "rolesAtPc": ["vint-target"],
        },
        "error": "VInt entry PC drift",
    }


@pytest.mark.parametrize(
    "mutation",
    ("missing", "extra", "wrong-role", "missing-pending", "extra-pending", "wrong-pending-role"),
)
def test_callback_failure_schema_closes_wait_role_and_pending_shape(mutation: str) -> None:
    payload = _callback_failure_payload()
    if mutation == "missing":
        del payload["role"]
    elif mutation == "extra":
        payload["unexpected"] = True
    elif mutation == "wrong-role":
        payload["role"] = "wrong-role"
    elif mutation == "missing-pending":
        del payload["pendingCallback"]["pendingReturnPc"]
    elif mutation == "extra-pending":
        payload["pendingCallback"]["unexpected"] = True
    else:
        payload["pendingCallback"]["rolesAtPc"] = ["wrong-role"]
    with pytest.raises(ValueError):
        validate_json(payload, controller_input.FAILURE_SCHEMA, owner="controller-input failure")


def test_registration_failure_is_valid_and_every_runtime_role_is_schema_admitted() -> None:
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
    allowed_failure_roles = set(
        shared["definitions"]["controllerInputFailure"]["properties"]["role"]["enum"]
    )
    allowed_pending_roles = set(
        shared["definitions"]["controllerInputPendingCallback"]["properties"]["rolesAtPc"]["items"][
            "enum"
        ]
    )
    source = controller_input.OBSERVER.read_text(encoding="utf-8")
    runtime_roles = {"registration"} | set(
        re.findall(r'current_phase,current_role(?:,current_expectation)?="[^"]+","([^"]+)"', source)
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
        "wait-helper-target",
        "wait-helper-return",
        "wait-for-vint-call",
        "wait-for-vint-target",
        "wait-for-vint-rts",
        "wait-for-vint-return",
        "vint-target",
        "vint-input-call",
        "vint-input-stage",
        "vint-input-return",
        "direct-return",
    }
    assert runtime_roles <= allowed_failure_roles
    assert allowed_pending_roles == {
        "bootstrap-entry",
        "bootstrap-return",
        "direct-call",
        "apply-target",
        "source-call",
        "update-target",
        "source-return",
        "direct-return",
        "wait-helper-target",
        "wait-helper-return",
        "wait-for-vint-call",
        "wait-for-vint-target",
        "wait-for-vint-rts",
        "wait-for-vint-return",
        "vint-target",
        "vint-input-call",
        "vint-input-stage",
        "vint-input-return",
    }
    config_roles = {
        entry["role"]
        for entries in controller_input.callback_expectations(_static(_fixture())).values()
        for entry in entries
    }
    assert config_roles == allowed_pending_roles - {"bootstrap-entry", "bootstrap-return"}
    assert 'register_exec(f.WaitForVInt,"wait-for-vint-target",0)' in source
    assert "entry.callbackAddress==current_pc" in source
    assert "waitForVIntActive=false" in source
    assert "wait_state.waitForVIntActive=true" in source
    assert "not wait_state.waitForVIntActive then return" in source
    assert source.index("wait_state.waitForVIntActive=true") < source.index(
        'current_phase,current_role,current_expectation="wait-for-vint-rts"'
    )


def test_vint_entry_failure_has_no_source_call_triple() -> None:
    payload = _vint_entry_failure_payload()
    validate_json(payload, controller_input.FAILURE_SCHEMA, owner="controller-input VInt failure")
    assert (
        payload["actualPc"],
        payload["expectedCallPc"],
        payload["expectedTargetPc"],
        payload["expectedReturnPc"],
    ) == (1428, None, None, None)
    source = controller_input.OBSERVER.read_text(encoding="utf-8")
    vint = source.index("local function on_vint_target()")
    assert 'require_pc(f.VInt,"VInt entry")' in source[vint:]
    vint_end = source.index("local function on_vint_input_call()")
    assert "current_expectation.targetPc" not in source[vint:vint_end]


def test_observer_counts_only_flagged_vint_inside_the_source_owned_wait_window() -> None:
    source = controller_input.OBSERVER.read_text(encoding="utf-8")
    assert "waitForVIntActive=false,ownedVInt=false" in source
    assert "wait_state.waitForVIntActive=true" in source
    assert (
        'if memory.read_u8(config.static.flow.waitingNextVIntAddress,"M68K BUS")==0 '
        "then wait_state.ownedVInt=false;return end"
    ) in source
    assert "wait_state.ownedVInt=true" in source
    assert "or not wait_state.ownedVInt then return" in source
    assert source.index("memory.read_u8(config.static.flow.waitingNextVIntAddress") < source.index(
        "wait_state.vIntEntryCount=wait_state.vIntEntryCount+1"
    )


def test_wait_cycle_chronology_fails_at_the_missing_role_and_counts_targets_as_entries() -> None:
    source = controller_input.OBSERVER.read_text(encoding="utf-8")
    for error in (
        "wait-for-vint call before wait helper target",
        "wait-for-vint call before prior cycle return",
        "wait-for-vint target before call",
        "duplicate wait-for-vint target",
        "wait-for-vint rts before target",
        "duplicate wait-for-vint rts",
        "wait-for-vint return before rts",
        "duplicate VInt ApplyZ80BusUpdates call",
        "VInt ApplyZ80BusUpdates target before call",
        "duplicate VInt ApplyZ80BusUpdates target",
        "VInt ApplyZ80BusUpdates return before stage",
    ):
        assert f'error("{error}")' in source
    call = source.index("wait_state.waitForVIntCallCount=wait_state.waitForVIntCallCount+1")
    target = source.index("wait_state.waitForVIntEntryCount=wait_state.waitForVIntEntryCount+1")
    assert call < target
    assert source.index("waitForVIntCallCount=0,waitForVIntEntryCount=0") < call
    assert "wait_state.waitForVIntCallSeen=false;wait_state.waitForVIntTargetSeen=false" in source
    target = source.index("local function on_wait_for_vint_target()")
    missing = source.index('error("wait-for-vint target before call")', target)
    target_expectation = source.index(
        'current_expectation=expectation(case,"wait-for-vint-target",wait_state.flowIndex)',
        target,
    )
    assert (
        source.index(
            'current_phase,current_role,current_expectation="wait-for-vint-target",'
            '"wait-for-vint-target",nil',
            target,
        )
        < missing
    )
    assert target_expectation < missing
    for role, error in (
        ("wait-for-vint-rts", "wait-for-vint rts before target"),
        ("wait-for-vint-return", "wait-for-vint return before rts"),
    ):
        handler = source.index(f"local function on_{role.replace('-', '_')}()")
        resolved = source.index(
            f'current_expectation=expectation(case,"{role}",wait_state.flowIndex)', handler
        )
        assert resolved < source.index(f'error("{error}")', handler)
    payload = _callback_failure_payload()
    payload["expectedCallPc"] = None
    payload["expectedTargetPc"] = None
    payload["expectedReturnPc"] = None
    payload["error"] = "wait-for-vint target before call"
    validate_json(
        payload, controller_input.FAILURE_SCHEMA, owner="controller-input chronology failure"
    )
    assert payload["role"] == "wait-for-vint-target"
    assert (
        payload["expectedCallPc"],
        payload["expectedTargetPc"],
        payload["expectedReturnPc"],
    ) == (None, None, None)


def test_wait_harness_uses_two_stage_setup_without_later_wait_resets() -> None:
    source = controller_input.OBSERVER.read_text(encoding="utf-8")
    helper_target = source.index("local function on_wait_helper_target()")
    entry_seed = source.index(
        "if wait_state.helperEntryCount==0 then seed_helper_entry_inputs(case) end"
    )
    assert helper_target < entry_seed
    assert (
        source.index('require_pc(current_expectation.targetPc,"wait helper target")', helper_target)
        < entry_seed
    )
    for write, read in (
        ("a.PLAYER_1_INPUT,p1", 'a.PLAYER_1_INPUT,"M68K BUS")~=p1'),
        ("a.CURRENT_PLAYER_INPUT,p1", 'a.CURRENT_PLAYER_INPUT,"M68K BUS")~=p1'),
    ):
        assert f"memory.write_u8({write}" in source
        assert f"memory.read_u8({read}" in source
    first_call = source.index("local function on_wait_for_vint_call()")
    first_call_seed = source.index(
        "if not wait_state.firstWaitForVIntSetupConsumed then ", first_call
    )
    assert first_call < first_call_seed
    seed_call = source.index("seed_first_wait_for_vint_state(case)", first_call_seed)
    assert "wait_state.firstWaitForVIntSetupConsumed=true" in source[first_call_seed:]
    assert source.find("seed_first_wait_for_vint_state(case)", seed_call + 1) == -1
    for write, read in (
        ("a.CURRENT_PLAYER_INPUT,p1", 'a.CURRENT_PLAYER_INPUT,"M68K BUS")~=p1'),
        ("a.LAST_PLAYER_INPUT,p1", 'a.LAST_PLAYER_INPUT,"M68K BUS")~=p1'),
        ("a.INPUT_REPEAT_DELAYER,0", 'a.INPUT_REPEAT_DELAYER,"M68K BUS")~=0'),
    ):
        assert f"memory.write_u8({write}" in source
        assert f"memory.read_u8({read}" in source
    d5_stub = source.index('memory.write_u16_be(call_pc-6,0x2A3C,"M68K BUS")')
    direct_jsr = source.index('memory.write_u16_be(call_pc,0x4EB9,"M68K BUS")')
    assert d5_stub < direct_jsr
    assert "direct input probe D5 preamble write drift" in source


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
        "'caseId': 'wait-player-input-delayed'",
        "'phase': 'wait-for-vint-target'",
        "'role': 'wait-for-vint-target'",
        "'actualPc': 3822",
        "'expectedCallPc': 5502",
        "'expectedTargetPc': 3822",
        "'expectedReturnPc': 5506",
        "'pendingCallback': {'active': True, 'caseIndex': 9, 'frameIndex': 0",
    ):
        assert expected in message


def test_append_log_failure_is_structured_terminal_and_observer_has_one_dispatcher(
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
            status, owner=controller_input.OWNER, schema_path=controller_input.FAILURE_SCHEMA
        )
        == payload
    )
    assert controller_input._failure_diagnostic(status) == str(payload)
    status.write_text(status.read_text(encoding="utf-8") * 2, encoding="utf-8")
    with pytest.raises(ValueError, match="multiplicity"):
        callback_failure_status(
            status, owner=controller_input.OWNER, schema_path=controller_input.FAILURE_SCHEMA
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
    assert "local loop_ok,loop_message=pcall(function()" in source
    assert "if not loop_ok then fail_callback(loop_message) end" in source
    assert "bootstrap_armed=true" in source
    assert "bootstrapped=true" in source
    assert source.index("bootstrap_armed=true") < source.index("bootstrapped=true")
    assert "local function arm_step()" in source
    assert "direct input probe gate arm drift" in source
    assert "direct input probe gate pause drift" in source
    assert "wait callback count drift" in source
    assert "emu.setregister" not in source
    assert 'memory.write_u16_be(call_pc-6,0x2A3C,"M68K BUS")' in source
    assert 'memory.write_u32_be(call_pc-4,config.probeD5,"M68K BUS")' in source
    assert "direct input probe D5 preamble write drift" in source
    assert "wait helper entry input readback drift" in source
    assert "wait helper first-call setup readback drift" in source
    assert "if wait_state.helperEntryCount==0 then seed_helper_entry_inputs(case) end" in source
    for milestone in (
        "milestone:observer-loaded",
        "milestone:direct-input-probe",
        "milestone:callbacks-cleared:0",
        "milestone:observer-finished",
    ):
        assert milestone in source
