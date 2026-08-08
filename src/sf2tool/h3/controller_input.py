"""One-launch raw controller sampling and VInt repeat matrix.

This H3 rail deliberately stops at the direct ``UpdatePlayerInputs`` seam and
the input portion of the original ``ApplyZ80BusUpdates`` VInt stage.  It does
not model input wait helpers, controller negotiation, or any UI consumer.
"""

from __future__ import annotations

import re
from pathlib import Path
from typing import Any

from sf2tool.h3.bizhawk import DERIVED_ROOT, run_observer, verify_runtime_contract
from sf2tool.h3.observer_status import (
    CALLBACK_FAILURE_PREFIX,
    assert_observer_status,
    callback_failure_status,
    observer_failure_contract,
)
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path
from sf2tool.research_index import listing_symbol_addresses

FIXTURE = repo_path("tests/fixtures/h3/controller-input-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h3/controller-input-fixture.schema.json")
OBSERVATION_SCHEMA = repo_path("schemas/h3/controller-input-observation.schema.json")
FAILURE_SCHEMA = repo_path("schemas/h3/controller-input-callback-failure.schema.json")
OBSERVER = repo_path("tools/bizhawk/controller_input_observer.lua")
H2_SERVICES_FIXTURE = repo_path("tests/fixtures/h2/tech-services-static-v1.json")
H2_INTERRUPTS_FIXTURE = repo_path("tests/fixtures/h2/tech-interrupts-static-v1.json")
TOOLCHAIN_MANIFEST = repo_path("manifests/toolchain.json")

OWNER = "controller-input"
STATUS_PREFIX = CALLBACK_FAILURE_PREFIX
OBSERVER_FAILURE_CONTRACT = observer_failure_contract(OWNER)
INPUT_SOURCE = Path("code/common/tech/input.asm")
INTERRUPT_SOURCE = Path("code/common/tech/interrupts/applyfadingeffectandz80busupdate.asm")
CONST_SOURCE = Path("sf2const.asm")
ENUM_SOURCE = Path("sf2enums.asm")
LISTING = Path("build/sf2build-h1.lst")
BUTTON_SYMBOLS = (
    "INPUT_UP",
    "INPUT_DOWN",
    "INPUT_LEFT",
    "INPUT_RIGHT",
    "INPUT_B",
    "INPUT_C",
    "INPUT_A",
    "INPUT_START",
)
BUTTON_NAMES = {
    "Up": "INPUT_UP",
    "Down": "INPUT_DOWN",
    "Left": "INPUT_LEFT",
    "Right": "INPUT_RIGHT",
    "B": "INPUT_B",
    "C": "INPUT_C",
    "A": "INPUT_A",
    "Start": "INPUT_START",
}
INPUT_ADDRESS_NAMES = (
    "DATA1",
    "DATA2",
    "PLAYER_1_INPUT",
    "PLAYER_2_INPUT",
    "CURRENT_PLAYER_INPUT",
    "LAST_PLAYER_INPUT",
    "INPUT_REPEAT_DELAYER",
)
DIRECT_CALL_PC = 0xFF6820
DIRECT_RETURN_PC = DIRECT_CALL_PC + 6


def callback_expectations(static: dict[str, Any]) -> dict[str, list[dict[str, int | str]]]:
    """The exact source/H1 callback roles used by the one physical-PC dispatcher."""
    update = static["functionEntries"]["UpdatePlayerInputs"]
    apply = static["functionEntries"]["ApplyZ80BusUpdates"]
    source_call, source_return = static["flow"]["applyInputCall"]
    direct_sample = {
        "callSiteAddress": DIRECT_CALL_PC,
        "targetAddress": update,
        "returnAddress": DIRECT_RETURN_PC,
    }
    direct_repeat = {
        "callSiteAddress": DIRECT_CALL_PC,
        "targetAddress": apply,
        "returnAddress": DIRECT_RETURN_PC,
    }
    nested_repeat = {
        "callSiteAddress": source_call,
        "targetAddress": update,
        "returnAddress": source_return,
    }
    return {
        "sample": [
            {"role": "direct-call", **direct_sample},
            {"role": "update-target", **direct_sample},
            {"role": "direct-return", **direct_sample},
        ],
        "repeat": [
            {"role": "direct-call", **direct_repeat},
            {"role": "apply-target", **direct_repeat},
            {"role": "source-call", **nested_repeat},
            {"role": "update-target", **nested_repeat},
            {"role": "source-return", **nested_repeat},
            {"role": "direct-return", **direct_repeat},
        ],
    }


def validate_callback_expectations(
    static: dict[str, Any], actual: dict[str, list[dict[str, int | str]]]
) -> None:
    if actual != callback_expectations(static):
        raise ValueError("controller-input callback role expectation drift")


def observer_config(fixture: dict[str, Any], static: dict[str, Any]) -> dict[str, Any]:
    """Build the input-only runtime request plus exact callback control-flow guards."""
    runtime_static = {
        **static,
        "flow": {**static["flow"], "applyInputCall": list(static["flow"]["applyInputCall"])},
    }
    return {
        "id": fixture["id"],
        "core": fixture["emulator"]["core"],
        "caseOrder": fixture["caseOrder"],
        "cases": fixture["cases"],
        "static": runtime_static,
        "callbackExpectations": callback_expectations(static),
        "observerFailureContract": OBSERVER_FAILURE_CONTRACT,
    }


def _equate(source: str, name: str) -> int:
    match = re.search(rf"^{re.escape(name)}:\s+equ\s+(\$[0-9A-F]+|\d+)", source, re.MULTILINE)
    if not match:
        raise ValueError(f"controller-input source guard missing constant: {name}")
    token = match.group(1)
    return int(token[1:], 16) if token.startswith("$") else int(token)


def _instructions(source: str, symbol: str) -> list[tuple[str, str]]:
    start = re.search(rf"^{re.escape(symbol)}:\s*$", source, re.MULTILINE)
    if not start:
        raise ValueError(f"controller-input source guard missing section: {symbol}")
    end = source.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"controller-input source guard missing end marker: {symbol}")
    records: list[tuple[str, str]] = []
    for raw in source[start.start() : end].splitlines():
        code = raw.split(";", 1)[0].strip()
        if not code or code.endswith(":"):
            continue
        match = re.fullmatch(r"([A-Za-z][A-Za-z0-9]*(?:\.[bBwWlLsS])?)\s*(.*)", code)
        if not match:
            raise ValueError(f"controller-input source guard cannot parse instruction: {raw!r}")
        records.append((match.group(1).lower(), re.sub(r"\s+", "", match.group(2)).lower()))
    return records


def _require_order(source: str, symbol: str, required: tuple[tuple[str, str], ...]) -> None:
    instructions = _instructions(source, symbol)
    cursor = 0
    for expected in required:
        while cursor < len(instructions) and instructions[cursor] != expected:
            cursor += 1
        if cursor == len(instructions):
            raise ValueError(
                "controller-input source operation drift in "
                f"{symbol}: expected {expected[0]} {expected[1]}"
            )
        cursor += 1


def _require_count(source: str, symbol: str, expected: tuple[str, str], count: int) -> None:
    actual = _instructions(source, symbol).count(expected)
    if actual != count:
        raise ValueError(
            "controller-input source count drift in "
            f"{symbol}: {expected} expected {count}, got {actual}"
        )


def _h1_instruction_addresses(listing: str, symbol: str, instruction: str) -> list[tuple[int, int]]:
    start = re.search(rf"^[0-9A-F]{{8}}\s+{re.escape(symbol)}:\s*$", listing, re.MULTILINE)
    if not start:
        raise ValueError(f"controller-input H1 guard missing symbol: {symbol}")
    end = listing.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"controller-input H1 guard missing end marker: {symbol}")
    records: list[tuple[int, int]] = []
    for line in listing[start.end() : end].splitlines():
        match = re.fullmatch(r"([0-9A-F]{8})\s+((?:[0-9A-F]{4}\s+)+)\s+(.+?)\s*", line)
        if match and re.sub(r"\s+", " ", match.group(3).strip()) == re.sub(
            r"\s+", " ", instruction.strip()
        ):
            encoded = re.sub(r"\s+", "", match.group(2))
            records.append((int(match.group(1), 16), len(encoded) // 2))
    return records


def _one_h1_instruction(listing: str, symbol: str, instruction: str) -> tuple[int, int]:
    records = _h1_instruction_addresses(listing, symbol, instruction)
    if len(records) != 1:
        raise ValueError(
            f"controller-input H1 guard expected one {symbol} instruction: {instruction}"
        )
    address, width = records[0]
    return address, address + width


def _h1_return_address(listing: str, symbol: str) -> int:
    records = _h1_instruction_addresses(listing, symbol, "rts")
    if len(records) != 1:
        raise ValueError(f"controller-input H1 guard expected one {symbol} return")
    return records[0][0]


def _h1_first_instruction(listing: str, symbol: str) -> bytes:
    start = re.search(rf"^[0-9A-F]{{8}}\s+{re.escape(symbol)}:\s*$", listing, re.MULTILINE)
    if not start:
        raise ValueError(f"controller-input H1 guard missing symbol: {symbol}")
    match = re.search(r"^[0-9A-F]{8}\s+((?:[0-9A-F]{4}\s+)+)", listing[start.end() :], re.MULTILINE)
    if not match:
        raise ValueError(f"controller-input H1 guard missing first instruction: {symbol}")
    return bytes.fromhex(re.sub(r"\s+", "", match.group(1)))


def _owner_facts(
    fixture: dict[str, Any], services: dict[str, Any], interrupts: dict[str, Any]
) -> tuple[dict[str, Any], dict[str, Any], int]:
    provenance = fixture["provenance"]
    toolchain = load_json(TOOLCHAIN_MANIFEST)["sf2disasm"]
    if (
        fixture["romSha256"] != services["romSha256"]
        or fixture["romSha256"] != interrupts["romSha256"]
        or provenance["upstreamRepository"] != toolchain["repository"].removesuffix(".git")
        or provenance["upstreamCommit"] != toolchain["commit"]
        or provenance["upstreamCommit"] != services["upstreamCommit"]
        or provenance["upstreamCommit"] != interrupts["upstreamCommit"]
        or provenance["h2TechServicesFixture"]
        != H2_SERVICES_FIXTURE.relative_to(repo_path(".")).as_posix()
        or provenance["h2TechServicesFixtureId"] != services["id"]
        or provenance["h2TechInterruptsFixture"]
        != H2_INTERRUPTS_FIXTURE.relative_to(repo_path(".")).as_posix()
        or provenance["h2TechInterruptsFixtureId"] != interrupts["id"]
    ):
        raise ValueError("controller-input provenance disagrees with pinned H2 owners")
    facts = services["expected"]["inputFacts"]
    repeat = interrupts["expected"]["interruptFacts"]["inputRepeat"]
    bootstrap = services["expected"]["sramFacts"]["functionEntries"]["CheckSram"]
    apply = interrupts["function"].get("ApplyZ80BusUpdates")
    if (
        facts["sourcePath"] != INPUT_SOURCE.as_posix()
        or provenance["inputSourcePath"] != facts["sourcePath"]
        or provenance["interruptSourcePath"] != INTERRUPT_SOURCE.as_posix()
        or facts["functionEntries"].get("UpdatePlayerInputs") is None
        or not isinstance(apply, int)
    ):
        raise ValueError("controller-input H2 owner shape drift")
    if (
        fixture["sourceContext"]["updatePlayerInputsEntryAddress"]
        != facts["functionEntries"]["UpdatePlayerInputs"]
    ):
        raise ValueError("controller-input source context disagrees with H2 input entry")
    if fixture["sourceContext"]["applyZ80BusUpdatesEntryAddress"] != apply:
        raise ValueError("controller-input source context disagrees with H2 interrupt entry")
    return facts, repeat, bootstrap


def _source_contract(
    facts: dict[str, Any],
    repeat: dict[str, Any],
    input_source: str,
    interrupt_source: str,
    const_source: str,
    enum_source: str,
) -> tuple[dict[str, int], dict[str, int], int]:
    all_addresses = facts["constants"]["addresses"]
    addresses = {name: _equate(const_source, name) for name in INPUT_ADDRESS_NAMES}
    if any(all_addresses[name] != value for name, value in addresses.items()):
        raise ValueError("controller-input H2 constants disagree with source declarations")
    masks = {name: _equate(enum_source, name) for name in BUTTON_SYMBOLS}
    if facts["constants"]["buttonMasks"] != masks:
        raise ValueError("controller-input H2 masks disagree with source declarations")
    recognized_mask = 0
    for value in masks.values():
        recognized_mask |= value
    sampling = {
        "controllerPortAddresses": [addresses["DATA1"], addresses["DATA2"]],
        "controllerPortStrideBytes": addresses["DATA2"] - addresses["DATA1"],
        "controllerCount": 2,
        "rawStateBytesPerController": addresses["PLAYER_2_INPUT"] - addresses["PLAYER_1_INPUT"],
        "rawStateStorageAddresses": [addresses["PLAYER_1_INPUT"], addresses["PLAYER_2_INPUT"]],
        "thLowWriteValue": 0,
        "thHighWriteValue": 0x40,
        "highBitsLeftShift": 2,
        "highBitsMask": 0xC0,
        "lowBitsMask": 0x3F,
        "invertsComposedState": True,
        "storesTwoComposedStatesPerPort": True,
    }
    if facts["sampling"] != sampling or recognized_mask != 0xFF:
        raise ValueError("controller-input H2 sampling derivation drift")
    _require_order(
        input_source,
        "UpdatePlayerInputs",
        (
            ("lea", "((player_1_input-$1000000)).w,a5"),
            ("lea", "(data1).l,a6"),
            ("bsr.s", "@loc_1"),
            ("addq.w", "#2,a6"),
            ("move.b", "#0,(a6)"),
            ("move.b", "#$40,(a6)"),
            ("lsl.b", "#2,d6"),
            ("andi.b", "#$c0,d6"),
            ("andi.b", "#$3f,d7"),
            ("or.b", "d7,d6"),
            ("not.b", "d6"),
            ("move.b", "d6,(a5)+"),
        ),
    )
    _require_count(input_source, "UpdatePlayerInputs", ("move.b", "d6,(a5)+"), 2)
    _require_count(input_source, "UpdatePlayerInputs", ("move.b", "#$40,(a6)"), 4)
    _require_order(
        interrupt_source,
        "ApplyZ80BusUpdates",
        (
            ("bsr.w", "updateplayerinputs"),
            ("move.b", "((player_1_input-$1000000)).w,d0"),
            ("move.b", "d0,((current_player_input-$1000000)).w"),
            ("cmp.b", "((last_player_input-$1000000)).w,d0"),
            ("bne.s", "@waspushingdirection"),
            ("addq.b", "#1,((input_repeat_delayer-$1000000)).w"),
            ("cmpi.b", "#24,((input_repeat_delayer-$1000000)).w"),
            ("bcc.s", "@ignoreinput"),
            ("clr.b", "((current_player_input-$1000000)).w"),
            ("subq.b", "#6,((input_repeat_delayer-$1000000)).w"),
            ("move.b", "((current_player_input-$1000000)).w,((last_player_input-$1000000)).w"),
        ),
    )
    if repeat != {
        "initialDelayFrames": 24,
        "repeatCadenceFrames": 6,
        "unchangedInputSuppressedBeforeDelay": True,
    }:
        raise ValueError("controller-input H2 repeat derivation drift")
    return addresses, masks, recognized_mask


def build_static_contract(
    fixture: dict[str, Any],
    upstream_path: Path,
    *,
    h2_services_fixture_path: Path = H2_SERVICES_FIXTURE,
    h2_interrupts_fixture_path: Path = H2_INTERRUPTS_FIXTURE,
    input_source_text: str | None = None,
    interrupt_source_text: str | None = None,
    listing_text: str | None = None,
) -> dict[str, Any]:
    """Derive all runtime addresses/masks from accepted H2, H1, and source use sites."""
    services = load_json(h2_services_fixture_path)
    interrupts = load_json(h2_interrupts_fixture_path)
    facts, repeat, check_sram = _owner_facts(fixture, services, interrupts)
    upstream = upstream_path.resolve(strict=True)
    disasm = upstream / "disasm"
    input_source = input_source_text or (disasm / INPUT_SOURCE).read_text(encoding="utf-8")
    interrupt_source = interrupt_source_text or (disasm / INTERRUPT_SOURCE).read_text(
        encoding="utf-8"
    )
    const_source = (disasm / CONST_SOURCE).read_text(encoding="utf-8")
    enum_source = (disasm / ENUM_SOURCE).read_text(encoding="utf-8")
    listing = listing_text or (upstream / LISTING).read_text(encoding="utf-8")
    addresses, masks, recognized_mask = _source_contract(
        facts, repeat, input_source, interrupt_source, const_source, enum_source
    )
    symbols = listing_symbol_addresses(listing)
    update = facts["functionEntries"]["UpdatePlayerInputs"]
    apply = interrupts["function"]["ApplyZ80BusUpdates"]
    if (
        symbols.get("UpdatePlayerInputs") != update
        or symbols.get("ApplyZ80BusUpdates") != apply
        or symbols.get("CheckSram") != check_sram
    ):
        raise ValueError("controller-input H2/H1 entry derivation drift")
    if (
        fixture["sourceContext"]["updatePlayerInputsEntryAddress"]
        != symbols["UpdatePlayerInputs"]
        or fixture["sourceContext"]["applyZ80BusUpdatesEntryAddress"]
        != symbols["ApplyZ80BusUpdates"]
    ):
        raise ValueError("controller-input source context disagrees with H1 entry")
    apply_input_call = _one_h1_instruction(
        listing, "ApplyZ80BusUpdates", "bsr.w   UpdatePlayerInputs"
    )
    return {
        "functionEntries": {
            "CheckSram": check_sram,
            "UpdatePlayerInputs": update,
            "ApplyZ80BusUpdates": apply,
        },
        "addresses": addresses,
        "buttonMasks": masks,
        "recognizedButtonMask": recognized_mask,
        "sampling": facts["sampling"],
        "repeat": repeat,
        "flow": {
            "applyInputCall": apply_input_call,
            "updateRtsPc": _h1_return_address(listing, "UpdatePlayerInputs"),
            "applyRtsPc": _h1_return_address(listing, "ApplyZ80BusUpdates"),
        },
    }


def validate_static_contract(
    fixture: dict[str, Any], rom_path: Path, upstream_path: Path
) -> dict[str, Any]:
    static = build_static_contract(fixture, upstream_path)
    listing = (upstream_path.resolve(strict=True) / LISTING).read_text(encoding="utf-8")
    rom = rom_path.resolve(strict=True).read_bytes()
    for symbol in ("UpdatePlayerInputs", "ApplyZ80BusUpdates", "CheckSram"):
        address = static["functionEntries"][symbol]
        opcode = _h1_first_instruction(listing, symbol)
        if rom[address : address + len(opcode)] != opcode:
            raise ValueError(f"controller-input H1/ROM first-instruction guard drift: {symbol}")
    return static


def _button_value(buttons: list[str], masks: dict[str, int]) -> int:
    value = 0
    for button in buttons:
        try:
            value |= masks[BUTTON_NAMES[button]]
        except KeyError as error:
            raise ValueError(f"controller-input unknown button input: {button}") from error
    return value


def _raw_states(frame: dict[str, list[str]], masks: dict[str, int]) -> list[int]:
    player1 = _button_value(frame["player1Buttons"], masks)
    player2 = _button_value(frame["player2Buttons"], masks)
    return [player1, player1, player2, player2]


def _repeat_step(raw: int, state: dict[str, int], repeat: dict[str, Any]) -> dict[str, int]:
    current, last, delay = raw, state["lastPlayerInput"], state["inputRepeatDelayer"]
    if raw == last:
        delay = (delay + 1) & 0xFF
        if delay < repeat["initialDelayFrames"]:
            current = 0
        else:
            delay = (delay - repeat["repeatCadenceFrames"]) & 0xFF
    else:
        prior_had_direction = (last & 0x0F) != 0
        last = current
        if (raw & 0x0F) == 0 or not prior_had_direction:
            delay = 0
    return {
        "currentPlayerInput": current,
        "lastPlayerInput": last,
        "inputRepeatDelayer": delay,
    }


def model_case(case: dict[str, Any], static: dict[str, Any]) -> dict[str, Any]:
    """Independent expected model; the fixture contains inputs only."""
    masks = static["buttonMasks"]
    if case["kind"] == "sample":
        return {"rawStateBytes": _raw_states(case["frames"][0], masks)}
    if case["kind"] == "repeat":
        state = {"lastPlayerInput": 0, "inputRepeatDelayer": 0}
        frames = []
        for frame in case["frames"]:
            raw = _raw_states(frame, masks)
            state = _repeat_step(raw[0], state, static["repeat"])
            frames.append({"rawStateBytes": raw, **state})
        return {"frames": frames}
    raise ValueError(f"controller-input unknown case kind: {case['kind']}")


def expected_observation(fixture: dict[str, Any], static: dict[str, Any]) -> dict[str, Any]:
    if [case["id"] for case in fixture["cases"]] != fixture["caseOrder"]:
        raise ValueError("controller-input fixture case order drift")
    return {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["id"],
        "caseOrder": fixture["caseOrder"],
        "records": [
            {"id": case["id"], "result": model_case(case, static)} for case in fixture["cases"]
        ],
        "callbacksCleared": 0,
    }


def _failure_diagnostic(status_path: Path) -> str | None:
    payload = callback_failure_status(status_path, owner=OWNER, schema_path=FAILURE_SCHEMA)
    if payload is None:
        return None
    lines = status_path.read_text(encoding="utf-8").splitlines()
    failures = [index for index, line in enumerate(lines) if line.startswith(STATUS_PREFIX)]
    if len(failures) != 1 or failures[0] != len(lines) - 1:
        raise ValueError(
            "controller-input callback failure must be one terminal exact failure line"
        )
    return str(payload)


def _assert_observation(
    fixture: dict[str, Any], static: dict[str, Any], observed: dict[str, Any]
) -> None:
    validate_json(observed, OBSERVATION_SCHEMA, owner="controller-input observation")
    expected = expected_observation(fixture, static)
    if observed != expected:
        raise ValueError(
            "controller-input runtime matrix mismatch\n"
            f"expected={expected!r}\nobserved={observed!r}"
        )


def verify_controller_input(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 180
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner="controller-input fixture")
    verify_runtime_contract(fixture, rom_path)
    static = validate_static_contract(fixture, rom_path, upstream_path)
    config = observer_config(fixture, static)
    validate_callback_expectations(static, config["callbackExpectations"])
    try:
        observed = run_observer(
            rom_path=rom_path,
            observer_path=OBSERVER,
            config=config,
            output_name=OWNER,
            timeout_seconds=timeout_seconds,
        )
    except RuntimeError as error:
        diagnostic = _failure_diagnostic(DERIVED_ROOT / f"{OWNER}.status.txt")
        if diagnostic is not None:
            raise RuntimeError(f"{OWNER} observer callback failure: {diagnostic}") from error
        raise
    assert_observer_status(
        DERIVED_ROOT / f"{OWNER}.status.txt",
        owner=OWNER,
        schema_path=FAILURE_SCHEMA,
        required_milestones=("milestone:direct-input-probe",),
    )
    _assert_observation(fixture, static, observed)
    return {
        "Fixture": fixture["id"],
        "Cases": len(fixture["cases"]),
        "BizHawkLaunches": 1,
        "CallbacksCleared": observed["callbacksCleared"],
        "Status": "PASS",
    }
