"""One-launch controller raw-sampling, repeat, and bounded direct-wait matrix.

The rail observes five direct ``UpdatePlayerInputs`` raw two-port cases, three
direct ``ApplyZ80BusUpdates`` repeat cases, and eight direct original wait-helper
cases through their owned ``WaitForVInt``/enabled-VInt input-stage progression.
It does not establish ``sub_15A4``, controller negotiation or latency, normal
game/UI caller progression, or user-visible UI behavior.
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
VINT_ENGINE_SOURCE = Path("code/common/tech/interrupts/vintengine_1.asm")
VINT_SOURCE = Path("code/common/tech/interrupts/vint.asm")
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
WAIT_HELPERS = (
    "WaitForPlayerInput",
    "WaitForPlayer1NewInput",
    "WaitForInputFor1Second",
    "WaitForInputFor3Seconds",
)
WAIT_D5_SENTINEL = 0x13579BDF


def callback_expectations(static: dict[str, Any]) -> dict[str, list[dict[str, Any]]]:
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
    expectations: dict[str, list[dict[str, Any]]] = {
        "sample": [
            {"role": "direct-call", "callbackAddress": DIRECT_CALL_PC, **direct_sample},
            {"role": "update-target", "callbackAddress": update, **direct_sample},
            {"role": "direct-return", "callbackAddress": DIRECT_RETURN_PC, **direct_sample},
        ],
        "repeat": [
            {"role": "direct-call", "callbackAddress": DIRECT_CALL_PC, **direct_repeat},
            {"role": "apply-target", "callbackAddress": apply, **direct_repeat},
            {"role": "source-call", "callbackAddress": source_call, **nested_repeat},
            {"role": "update-target", "callbackAddress": update, **nested_repeat},
            {"role": "source-return", "callbackAddress": source_return, **nested_repeat},
            {"role": "direct-return", "callbackAddress": DIRECT_RETURN_PC, **direct_repeat},
        ],
    }
    vint_apply_call, vint_apply_return = static["flow"]["vIntApplyInput"]
    for helper in WAIT_HELPERS:
        target = static["functionEntries"][helper]
        direct_wait = {
            "callSiteAddress": DIRECT_CALL_PC,
            "targetAddress": target,
            "returnAddress": DIRECT_RETURN_PC,
        }
        entries = [
            {"role": "direct-call", "callbackAddress": DIRECT_CALL_PC, **direct_wait},
            {"role": "wait-helper-target", "callbackAddress": target, **direct_wait},
            {
                "role": "wait-helper-return",
                "callbackAddress": static["flow"]["waitHelper"][helper]["rtsPc"],
                **direct_wait,
            },
            {"role": "direct-return", "callbackAddress": DIRECT_RETURN_PC, **direct_wait},
            {
                "role": "vint-target",
                "callbackAddress": static["functionEntries"]["VInt"],
                "callSiteAddress": None,
                "targetAddress": None,
                "returnAddress": None,
            },
            {
                "role": "vint-input-call",
                "callbackAddress": vint_apply_call,
                "callSiteAddress": vint_apply_call,
                "targetAddress": apply,
                "returnAddress": vint_apply_return,
            },
            {
                "role": "vint-input-stage",
                "callbackAddress": apply,
                "callSiteAddress": vint_apply_call,
                "targetAddress": apply,
                "returnAddress": vint_apply_return,
            },
            {
                "role": "vint-input-return",
                "callbackAddress": vint_apply_return,
                "callSiteAddress": vint_apply_call,
                "targetAddress": apply,
                "returnAddress": vint_apply_return,
            },
        ]
        for flow_index, (call, returned) in enumerate(
            static["flow"]["waitHelper"][helper]["vintCalls"]
        ):
            wait_for_vint = {
                "flowIndex": flow_index,
                "callSiteAddress": call,
                "targetAddress": static["functionEntries"]["WaitForVInt"],
                "returnAddress": returned,
            }
            entries.extend(
                (
                    {
                        "role": "wait-for-vint-call",
                        "callbackAddress": call,
                        **wait_for_vint,
                    },
                    {
                        "role": "wait-for-vint-target",
                        "callbackAddress": static["functionEntries"]["WaitForVInt"],
                        **wait_for_vint,
                    },
                    {
                        "role": "wait-for-vint-rts",
                        "callbackAddress": static["flow"]["waitForVIntRtsPc"],
                        **wait_for_vint,
                    },
                    {
                        "role": "wait-for-vint-return",
                        "callbackAddress": returned,
                        **wait_for_vint,
                    },
                )
            )
        expectations[helper] = entries
    return expectations


def validate_callback_expectations(
    static: dict[str, Any], actual: dict[str, list[dict[str, Any]]]
) -> None:
    if actual != callback_expectations(static):
        raise ValueError("controller-input callback role expectation drift")


def observer_config(fixture: dict[str, Any], static: dict[str, Any]) -> dict[str, Any]:
    """Build the input-only runtime request plus exact callback control-flow guards."""

    def lua_value(value: Any) -> Any:
        if isinstance(value, tuple):
            return [lua_value(item) for item in value]
        if isinstance(value, list):
            return [lua_value(item) for item in value]
        if isinstance(value, dict):
            return {key: lua_value(item) for key, item in value.items()}
        return value

    runtime_static = lua_value(static)
    return {
        "id": fixture["id"],
        "core": fixture["emulator"]["core"],
        "caseOrder": fixture["caseOrder"],
        "cases": fixture["cases"],
        "static": runtime_static,
        "callbackExpectations": callback_expectations(static),
        "waitExpectations": wait_expectations(fixture, static),
        "probeD5": WAIT_D5_SENTINEL,
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


def _h1_label_address(listing: str, label: str) -> int:
    records = re.findall(rf"^([0-9A-F]{{8}})\s+{re.escape(label)}:\s*$", listing, re.MULTILINE)
    if len(records) != 1:
        raise ValueError(f"controller-input H1 guard expected one label: {label}")
    return int(records[0], 16)


def _h1_first_instruction(listing: str, symbol: str) -> bytes:
    start = re.search(rf"^[0-9A-F]{{8}}\s+{re.escape(symbol)}:\s*$", listing, re.MULTILINE)
    if not start:
        raise ValueError(f"controller-input H1 guard missing symbol: {symbol}")
    match = re.search(r"^[0-9A-F]{8}\s+((?:[0-9A-F]{4}\s+)+)", listing[start.end() :], re.MULTILINE)
    if not match:
        raise ValueError(f"controller-input H1 guard missing first instruction: {symbol}")
    return bytes.fromhex(re.sub(r"\s+", "", match.group(1)))


def _h1_instruction_bytes(listing: str, symbol: str, instruction: str) -> bytes:
    start = re.search(rf"^[0-9A-F]{{8}}\s+{re.escape(symbol)}:\s*$", listing, re.MULTILINE)
    if not start:
        raise ValueError(f"controller-input H1 guard missing symbol: {symbol}")
    end = listing.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"controller-input H1 guard missing end marker: {symbol}")
    encoded_matches: list[bytes] = []
    normalized = re.sub(r"\s+", " ", instruction.strip())
    for line in listing[start.end() : end].splitlines():
        match = re.fullmatch(r"[0-9A-F]{8}\s+((?:[0-9A-F]{4}\s+)+)\s+(.+?)\s*", line)
        if match and re.sub(r"\s+", " ", match.group(2).strip()) == normalized:
            encoded_matches.append(bytes.fromhex(re.sub(r"\s+", "", match.group(1))))
    if len(encoded_matches) != 1:
        raise ValueError(
            f"controller-input H1 guard expected one {symbol} instruction: {instruction}"
        )
    return encoded_matches[0]


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
    vint = interrupts["function"].get("VInt")
    if (
        facts["sourcePath"] != INPUT_SOURCE.as_posix()
        or provenance["inputSourcePath"] != facts["sourcePath"]
        or provenance["interruptSourcePath"] != INTERRUPT_SOURCE.as_posix()
        or facts["functionEntries"].get("UpdatePlayerInputs") is None
        or not isinstance(apply, int)
        or not isinstance(vint, int)
        or any(not isinstance(facts["functionEntries"].get(name), int) for name in WAIT_HELPERS)
    ):
        raise ValueError("controller-input H2 owner shape drift")
    if (
        fixture["sourceContext"]["updatePlayerInputsEntryAddress"]
        != facts["functionEntries"]["UpdatePlayerInputs"]
    ):
        raise ValueError("controller-input source context disagrees with H2 input entry")
    if fixture["sourceContext"]["applyZ80BusUpdatesEntryAddress"] != apply:
        raise ValueError("controller-input source context disagrees with H2 interrupt entry")
    for name in WAIT_HELPERS:
        field = name[0].lower() + name[1:] + "EntryAddress"
        if fixture["sourceContext"][field] != facts["functionEntries"][name]:
            raise ValueError(
                f"controller-input source context disagrees with H2 input entry: {name}"
            )
    if fixture["sourceContext"]["vIntEntryAddress"] != vint:
        raise ValueError("controller-input source context disagrees with H2 VInt entry")
    return facts, repeat, bootstrap


def _source_contract(
    facts: dict[str, Any],
    repeat: dict[str, Any],
    input_source: str,
    interrupt_source: str,
    vint_engine_source: str,
    vint_source: str,
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
    waits = facts["waits"]
    expected_waits = {
        "recognizedButtonMask": recognized_mask,
        "waitForPlayerInputUsesCurrentInput": True,
        "waitForPlayerInputReturnsWhenRecognizedInputIsNonzero": True,
        "waitForPlayer1NewInputRequiresReleaseThenRecognizedPress": True,
        "oneSecondMaximumVintWaits": 60,
        "threeSecondMaximumVintWaits": 180,
        "boundedWaitsReturnEarlyOnRecognizedPlayer1Input": True,
    }
    if waits != expected_waits:
        raise ValueError("controller-input H2 wait derivation drift")
    button_mask_operands = (
        "#input_up|input_down|input_left|input_right|input_b|input_c|input_a|input_start"
    )
    current_mask = button_mask_operands + ",((current_player_input-$1000000)).w"
    player1_mask = button_mask_operands + ",((player_1_input-$1000000)).w"
    _require_order(
        input_source,
        "WaitForPlayerInput",
        (
            ("andi.b", current_mask),
            ("bne.s", "@return"),
            ("bsr.w", "waitforvint"),
            ("bra.s", "waitforplayerinput"),
            ("rts", ""),
        ),
    )
    _require_order(
        input_source,
        "WaitForPlayer1NewInput",
        (
            ("andi.b", player1_mask),
            ("beq.s", "@wait"),
            ("bsr.w", "waitforvint"),
            ("bra.s", "waitforplayer1newinput"),
            ("andi.b", player1_mask),
            ("bne.s", "@return"),
            ("bsr.w", "waitforvint"),
            ("bra.s", "@wait"),
            ("rts", ""),
        ),
    )
    _require_order(
        input_source,
        "WaitForInputFor1Second",
        (
            ("movem.l", "d5,-(sp)"),
            ("moveq", "#59,d5"),
            ("andi.b", player1_mask),
            ("bne.s", "@done"),
            ("bsr.w", "waitforvint"),
            ("dbf", "d5,waitforinput_loop"),
            ("movem.l", "(sp)+,d5"),
            ("rts", ""),
        ),
    )
    _require_order(
        input_source,
        "WaitForInputFor3Seconds",
        (
            ("movem.l", "d5,-(sp)"),
            ("move.l", "#179,d5"),
            ("bra.s", "waitforinput_loop"),
        ),
    )
    _require_count(input_source, "WaitForPlayerInput", ("bsr.w", "waitforvint"), 1)
    _require_count(input_source, "WaitForPlayer1NewInput", ("bsr.w", "waitforvint"), 2)
    _require_count(input_source, "WaitForInputFor1Second", ("bsr.w", "waitforvint"), 1)
    _require_order(
        vint_engine_source,
        "WaitForVInt",
        (
            ("bset", "#enable_vint,(vint_parameters).l"),
            ("move.b", "#1,((waiting_next_vint-$1000000)).w"),
            ("tst.b", "((waiting_next_vint-$1000000)).w"),
            ("bne.s", "@wait"),
            ("rts", ""),
        ),
    )
    _require_order(
        vint_source,
        "VInt",
        (
            ("bclr", "#enable_vint,(vint_parameters).l"),
            ("beq.s", "@skipupdates"),
            ("bsr.w", "applyz80busupdates"),
            ("bsr.w", "callcontextualfunctions"),
            ("clr.b", "((waiting_next_vint-$1000000)).w"),
        ),
    )
    return addresses, masks, recognized_mask


def build_static_contract(
    fixture: dict[str, Any],
    upstream_path: Path,
    *,
    h2_services_fixture_path: Path = H2_SERVICES_FIXTURE,
    h2_interrupts_fixture_path: Path = H2_INTERRUPTS_FIXTURE,
    input_source_text: str | None = None,
    interrupt_source_text: str | None = None,
    vint_engine_source_text: str | None = None,
    vint_source_text: str | None = None,
    const_source_text: str | None = None,
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
    vint_engine_source = vint_engine_source_text or (disasm / VINT_ENGINE_SOURCE).read_text(
        encoding="utf-8"
    )
    vint_source = vint_source_text or (disasm / VINT_SOURCE).read_text(encoding="utf-8")
    const_source = const_source_text or (disasm / CONST_SOURCE).read_text(encoding="utf-8")
    enum_source = (disasm / ENUM_SOURCE).read_text(encoding="utf-8")
    listing = listing_text or (upstream / LISTING).read_text(encoding="utf-8")
    addresses, masks, recognized_mask = _source_contract(
        facts,
        repeat,
        input_source,
        interrupt_source,
        vint_engine_source,
        vint_source,
        const_source,
        enum_source,
    )
    waiting_next_vint = _equate(const_source, "WAITING_NEXT_VINT")
    if fixture["sourceContext"]["waitingNextVIntAddress"] != waiting_next_vint:
        raise ValueError("controller-input source context disagrees with WAITING_NEXT_VINT")
    symbols = listing_symbol_addresses(listing)
    update = facts["functionEntries"]["UpdatePlayerInputs"]
    apply = interrupts["function"]["ApplyZ80BusUpdates"]
    vint = interrupts["function"]["VInt"]
    wait_entries = {name: facts["functionEntries"][name] for name in WAIT_HELPERS}
    wait_for_vint = symbols.get("WaitForVInt")
    if (
        symbols.get("UpdatePlayerInputs") != update
        or symbols.get("ApplyZ80BusUpdates") != apply
        or symbols.get("VInt") != vint
        or not isinstance(wait_for_vint, int)
        or any(symbols.get(name) != address for name, address in wait_entries.items())
        or symbols.get("CheckSram") != check_sram
    ):
        raise ValueError("controller-input H2/H1 entry derivation drift")
    wait_for_vint_rts = _h1_return_address(listing, "WaitForVInt")
    wait_for_vint_waiting_set = _one_h1_instruction(
        listing, "WaitForVInt", "move.b  #1,((WAITING_NEXT_VINT-$1000000)).w"
    )
    vint_waiting_clear = _one_h1_instruction(
        listing, "VInt", "clr.b   ((WAITING_NEXT_VINT-$1000000)).w"
    )
    wait_helper_flow = {
        "WaitForPlayerInput": {
            "entry": wait_entries["WaitForPlayerInput"],
            "rtsPc": _h1_return_address(listing, "WaitForPlayerInput"),
            "vintCalls": [
                _one_h1_instruction(listing, "WaitForPlayerInput", "bsr.w   WaitForVInt")
            ],
        },
        "WaitForPlayer1NewInput": {
            "entry": wait_entries["WaitForPlayer1NewInput"],
            "rtsPc": _h1_return_address(listing, "WaitForPlayer1NewInput"),
            "vintCalls": [
                (address, address + width)
                for address, width in _h1_instruction_addresses(
                    listing, "WaitForPlayer1NewInput", "bsr.w   WaitForVInt"
                )
            ],
        },
        "WaitForInputFor1Second": {
            "entry": wait_entries["WaitForInputFor1Second"],
            "rtsPc": _h1_return_address(listing, "WaitForInputFor1Second"),
            "vintCalls": [
                _one_h1_instruction(listing, "WaitForInputFor1Second", "bsr.w   WaitForVInt")
            ],
        },
        "WaitForInputFor3Seconds": {
            "entry": wait_entries["WaitForInputFor3Seconds"],
            "rtsPc": _h1_return_address(listing, "WaitForInputFor1Second"),
            "vintCalls": [
                _one_h1_instruction(listing, "WaitForInputFor1Second", "bsr.w   WaitForVInt")
            ],
            "loopBranch": _one_h1_instruction(
                listing, "WaitForInputFor3Seconds", "bra.s   WaitForInput_Loop"
            ),
        },
    }
    if len(wait_helper_flow["WaitForPlayer1NewInput"]["vintCalls"]) != 2:
        raise ValueError("controller-input H1 guard expected two WaitForPlayer1NewInput VInt calls")
    if (
        fixture["sourceContext"]["updatePlayerInputsEntryAddress"] != symbols["UpdatePlayerInputs"]
        or fixture["sourceContext"]["applyZ80BusUpdatesEntryAddress"]
        != symbols["ApplyZ80BusUpdates"]
        or fixture["sourceContext"]["waitForVIntEntryAddress"] != wait_for_vint
        or fixture["sourceContext"]["waitForVIntRtsPc"] != wait_for_vint_rts
        or fixture["sourceContext"]["vIntEntryAddress"] != vint
        or fixture["sourceContext"]["timedWaitLoopEntryAddress"]
        != _h1_label_address(listing, "WaitForInput_Loop")
    ):
        raise ValueError("controller-input source context disagrees with H1 entry")
    for name, flow in wait_helper_flow.items():
        prefix = name[0].lower() + name[1:]
        if fixture["sourceContext"][prefix + "RtsPc"] != flow["rtsPc"]:
            raise ValueError(f"controller-input source context disagrees with H1 return: {name}")
    expected_context = {
        "waitForPlayerInputVIntCallAddress": wait_helper_flow["WaitForPlayerInput"]["vintCalls"][0][
            0
        ],
        "waitForPlayerInputVIntReturnAddress": wait_helper_flow["WaitForPlayerInput"]["vintCalls"][
            0
        ][1],
        "waitForPlayer1NewInputReleaseVIntCallAddress": wait_helper_flow["WaitForPlayer1NewInput"][
            "vintCalls"
        ][0][0],
        "waitForPlayer1NewInputReleaseVIntReturnAddress": wait_helper_flow[
            "WaitForPlayer1NewInput"
        ]["vintCalls"][0][1],
        "waitForPlayer1NewInputPressVIntCallAddress": wait_helper_flow["WaitForPlayer1NewInput"][
            "vintCalls"
        ][1][0],
        "waitForPlayer1NewInputPressVIntReturnAddress": wait_helper_flow["WaitForPlayer1NewInput"][
            "vintCalls"
        ][1][1],
        "timedWaitVIntCallAddress": wait_helper_flow["WaitForInputFor1Second"]["vintCalls"][0][0],
        "timedWaitVIntReturnAddress": wait_helper_flow["WaitForInputFor1Second"]["vintCalls"][0][1],
        "waitForInputFor3SecondsLoopBranchAddress": wait_helper_flow["WaitForInputFor3Seconds"][
            "loopBranch"
        ][0],
        "waitForInputFor3SecondsLoopReturnAddress": wait_helper_flow["WaitForInputFor3Seconds"][
            "loopBranch"
        ][1],
        "vIntApplyInputCallAddress": _one_h1_instruction(
            listing, "VInt", "bsr.w   ApplyZ80BusUpdates"
        )[0],
        "vIntApplyInputReturnAddress": _one_h1_instruction(
            listing, "VInt", "bsr.w   ApplyZ80BusUpdates"
        )[1],
    }
    for key, value in expected_context.items():
        if fixture["sourceContext"][key] != value:
            raise ValueError(f"controller-input source context disagrees with H1: {key}")
    apply_input_call = _one_h1_instruction(
        listing, "ApplyZ80BusUpdates", "bsr.w   UpdatePlayerInputs"
    )
    return {
        "functionEntries": {
            "CheckSram": check_sram,
            "UpdatePlayerInputs": update,
            "ApplyZ80BusUpdates": apply,
            "WaitForVInt": wait_for_vint,
            "VInt": vint,
            **wait_entries,
        },
        "addresses": addresses,
        "buttonMasks": masks,
        "recognizedButtonMask": recognized_mask,
        "sampling": facts["sampling"],
        "repeat": repeat,
        "waits": facts["waits"],
        "flow": {
            "applyInputCall": apply_input_call,
            "updateRtsPc": _h1_return_address(listing, "UpdatePlayerInputs"),
            "applyRtsPc": _h1_return_address(listing, "ApplyZ80BusUpdates"),
            "waitForVIntRtsPc": wait_for_vint_rts,
            "waitingNextVIntAddress": waiting_next_vint,
            "waitForVIntWaitingFlagSet": wait_for_vint_waiting_set,
            "vIntWaitingFlagClear": vint_waiting_clear,
            "waitHelper": wait_helper_flow,
            "vIntApplyInput": _one_h1_instruction(listing, "VInt", "bsr.w   ApplyZ80BusUpdates"),
        },
    }


def validate_static_contract(
    fixture: dict[str, Any], rom_path: Path, upstream_path: Path
) -> dict[str, Any]:
    static = build_static_contract(fixture, upstream_path)
    listing = (upstream_path.resolve(strict=True) / LISTING).read_text(encoding="utf-8")
    rom = rom_path.resolve(strict=True).read_bytes()
    for symbol in (
        "UpdatePlayerInputs",
        "ApplyZ80BusUpdates",
        "CheckSram",
        "WaitForVInt",
        "VInt",
        *WAIT_HELPERS,
    ):
        address = static["functionEntries"][symbol]
        opcode = _h1_first_instruction(listing, symbol)
        if rom[address : address + len(opcode)] != opcode:
            raise ValueError(f"controller-input H1/ROM first-instruction guard drift: {symbol}")
    for symbol, instruction in (
        ("WaitForVInt", "move.b  #1,((WAITING_NEXT_VINT-$1000000)).w"),
        ("VInt", "clr.b   ((WAITING_NEXT_VINT-$1000000)).w"),
    ):
        address, _ = _one_h1_instruction(listing, symbol, instruction)
        opcode = _h1_instruction_bytes(listing, symbol, instruction)
        if rom[address : address + len(opcode)] != opcode:
            raise ValueError(f"controller-input H1/ROM operand guard drift: {symbol}")
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


def _wait_vint_input(case: dict[str, Any], index: int) -> dict[str, list[str]]:
    frames = case["vintInputs"]
    if not frames:
        return {"player1Buttons": [], "player2Buttons": []}
    return frames[min(index, len(frames) - 1)]


def _wait_vint_step(
    case: dict[str, Any],
    index: int,
    state: dict[str, int],
    masks: dict[str, int],
    repeat: dict[str, Any],
) -> dict[str, int | list[int]]:
    raw = _raw_states(_wait_vint_input(case, index), masks)
    state.update(_repeat_step(raw[0], state, repeat))
    return {"rawStateBytes": raw, **state}


def _wait_result(case: dict[str, Any], static: dict[str, Any]) -> dict[str, Any]:
    masks = static["buttonMasks"]
    recognized = static["recognizedButtonMask"]
    initial = case["initial"]
    raw_player1 = _button_value(initial["player1Buttons"], masks)
    state = {
        "currentPlayerInput": raw_player1,
        "lastPlayerInput": raw_player1,
        "inputRepeatDelayer": 0,
    }
    frames: list[dict[str, int | list[int]]] = []

    def wait_once() -> None:
        frames.append(_wait_vint_step(case, len(frames), state, masks, static["repeat"]))

    helper = case["helper"]
    helper_entry_count = 1
    if helper == "WaitForPlayerInput":
        while not (state["currentPlayerInput"] & recognized):
            if len(frames) >= 255:
                raise ValueError(
                    "controller-input delayed player-input fixture never presses a button"
                )
            wait_once()
        helper_entry_count = len(frames) + 1
    elif helper == "WaitForPlayer1NewInput":
        started_held = bool(raw_player1 & recognized)
        while raw_player1 & recognized:
            if len(frames) >= 255:
                raise ValueError(
                    "controller-input new-input fixture never releases its held button"
                )
            wait_once()
            raw_player1 = frames[-1]["rawStateBytes"][0]  # type: ignore[index]
        while not (raw_player1 & recognized):
            if len(frames) >= 255:
                raise ValueError("controller-input new-input fixture never represses a button")
            wait_once()
            raw_player1 = frames[-1]["rawStateBytes"][0]  # type: ignore[index]
        helper_entry_count = 2 if started_held else 1
    elif helper in {"WaitForInputFor1Second", "WaitForInputFor3Seconds"}:
        maximum = static["waits"][
            "oneSecondMaximumVintWaits"
            if helper == "WaitForInputFor1Second"
            else "threeSecondMaximumVintWaits"
        ]
        for _ in range(maximum):
            if raw_player1 & recognized:
                break
            wait_once()
            raw_player1 = frames[-1]["rawStateBytes"][0]  # type: ignore[index]
        else:
            pass
    else:
        raise ValueError(f"controller-input unknown wait helper: {helper}")
    wait_count = len(frames)
    result: dict[str, Any] = {
        "helperEntryCount": helper_entry_count,
        "helperReturnCount": 1,
        "waitForVIntEntryCount": wait_count,
        "waitForVIntReturnCount": wait_count,
        "vIntEntryCount": wait_count,
        "vIntInputStageCount": wait_count,
        "frames": frames,
    }
    # The two timed helpers explicitly save and restore D5.  The player-input
    # helpers do not own that register, so a harness observation must not turn
    # their incidental register value into a contract.
    if helper in {"WaitForInputFor1Second", "WaitForInputFor3Seconds"}:
        result["d5After"] = WAIT_D5_SENTINEL
    return result


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
    if case["kind"] == "wait":
        return _wait_result(case, static)
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


def wait_expectations(fixture: dict[str, Any], static: dict[str, Any]) -> dict[str, dict[str, int]]:
    """Derived wait callback counts sent to the observer; the fixture contains input only."""
    return {
        case["id"]: {
            key: value for key, value in model_case(case, static).items() if key.endswith("Count")
        }
        for case in fixture["cases"]
        if case["kind"] == "wait"
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
