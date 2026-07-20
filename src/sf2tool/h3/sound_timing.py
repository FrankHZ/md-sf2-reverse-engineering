from __future__ import annotations

from pathlib import Path
from typing import Any

from sf2tool.h2.sound_data import build_sound_data_inventory
from sf2tool.h3.bizhawk import run_observer, verify_runtime_contract
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

FIXTURE = repo_path("tests/fixtures/h3/sound-timing-v1.json")
SCHEMA = repo_path("schemas/h3-sound-timing-fixture.schema.json")
OBSERVER = repo_path("tools/bizhawk/sound_timing_observer.lua")

RAM = {
    "channelBaseAddress": 0x1380,
    "channelCount": 10,
    "channelRecordSize": 0x20,
    "newSampleAddress": 0x1FF8,
    "musicBankAddress": 0x152D,
    "fadeInTimerAddress": 0x1533,
    "musicDacModeAddress": 0x1534,
    "newOperationAddress": 0x1FFF,
}

BANK_OUTPUTS = {
    "bank0": "disasm/data/sound/musicbank0.bin",
    "bank1": "disasm/data/sound/musicbank1.bin",
}


def _header_pointers(payload: bytes, z80_address: int) -> list[int]:
    offset = z80_address - 0x8000
    header = payload[offset : offset + 24]
    if len(header) != 24:
        raise ValueError(f"music header falls outside its bank: 0x{z80_address:04X}")
    return [int.from_bytes(header[index : index + 2], "little") for index in range(4, 24, 2)]


def _static_cases(
    fixture: dict[str, Any], inventory: dict[str, Any], upstream_path: Path
) -> list[dict[str, Any]]:
    command_model = inventory["commandModel"]
    slots = {row["commandId"]: row for row in command_model["bankSelection"]["slots"]}
    headers = {
        row["entryLabel"]: row for row in command_model["musicHeaders"]["entries"]
    }
    bank_payloads = {
        bank: (upstream_path / relative).read_bytes()
        for bank, relative in BANK_OUTPUTS.items()
    }

    cases = []
    for case in fixture["cases"]:
        slot = slots[case["command"]]
        header = headers[slot["targetSymbol"]]
        expected_static = {
            "entryLabel": slot["targetSymbol"],
            "bank": slot["bank"],
            "bankRegisterValue": slot["bankRegisterValue"],
            "dacDisabled": header["dacDisabled"],
            "timerB": header["timerB"],
        }
        actual_static = {key: case[key] for key in expected_static}
        if actual_static != expected_static:
            raise ValueError(f"sound runtime static case drift: {case['id']}")

        pointers = _header_pointers(
            bank_payloads[slot["bank"]], slot["targetZ80Address"]
        )
        first = case["expectedCheckpoints"][0]
        if first["frame"] != fixture["checkpointFrames"][0] or first["pointers"] != pointers:
            raise ValueError(f"sound runtime initial channel pointers drift: {case['id']}")
        if [row["frame"] for row in case["expectedCheckpoints"]] != fixture[
            "checkpointFrames"
        ]:
            raise ValueError(f"sound runtime checkpoint order drift: {case['id']}")
        for checkpoint in case["expectedCheckpoints"]:
            if checkpoint["operation"] != 0:
                raise ValueError(f"sound command was not accepted in golden: {case['id']}")
            if checkpoint["musicBank"] != case["bankRegisterValue"]:
                raise ValueError(f"sound bank state disagrees with H2: {case['id']}")
            if checkpoint["dacDisabled"] != int(case["dacDisabled"]):
                raise ValueError(f"sound DAC state disagrees with H2: {case['id']}")
            for field in ("pointers", "timeCounters", "inactive"):
                if len(checkpoint[field]) != RAM["channelCount"]:
                    raise ValueError(f"sound checkpoint channel width drift: {case['id']}::{field}")
        cases.append({"id": case["id"], "command": case["command"]})
    return cases


def verify_sound_timing(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 120
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner="sound timing runtime fixture")
    verify_runtime_contract(fixture, rom_path)
    upstream_path = upstream_path.resolve(strict=True)
    inventory = build_sound_data_inventory(rom_path, upstream_path)
    if fixture["ram"] != RAM:
        raise ValueError("sound timing Z80 RAM contract drift")
    cases = _static_cases(fixture, inventory, upstream_path)

    observed = run_observer(
        rom_path=rom_path,
        observer_path=OBSERVER,
        config={
            "fixtureId": fixture["id"],
            "bootFrames": fixture["bootFrames"],
            "checkpointFrames": fixture["checkpointFrames"],
            "ram": fixture["ram"],
            "cases": cases,
        },
        output_name="sound-timing",
        timeout_seconds=timeout_seconds,
    )
    expected = {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["id"],
        "bootFrames": fixture["bootFrames"],
        "records": [
            {
                "id": case["id"],
                "command": case["command"],
                "checkpoints": case["expectedCheckpoints"],
            }
            for case in fixture["cases"]
        ],
    }
    if observed != expected:
        raise ValueError(
            "sound timing runtime matrix mismatch\n"
            f"expected={expected!r}\nobserved={observed!r}"
        )
    return {
        "Fixture": fixture["id"],
        "Cases": len(cases),
        "Checkpoints": len(cases) * len(fixture["checkpointFrames"]),
        "ChannelSnapshots": len(cases)
        * len(fixture["checkpointFrames"])
        * RAM["channelCount"],
        "BizHawkLaunches": 1,
        "Status": "PASS",
    }
