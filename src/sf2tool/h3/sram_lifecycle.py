"""One-launch direct-function observation of the seven H2-owned SRAM services.

The fixture deliberately records compact whole-span facts instead of SRAM or save
files.  Static addresses, dimensions, selectors, and flag bits are always read
from the accepted tech-services H2 fixture; this rail has no second constants map.
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

FIXTURE = repo_path("tests/fixtures/h3/sram-lifecycle-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h3/sram-lifecycle-fixture.schema.json")
OBSERVATION_SCHEMA = repo_path("schemas/h3/sram-lifecycle-observation.schema.json")
FAILURE_SCHEMA = repo_path("schemas/h3/sram-lifecycle-callback-failure.schema.json")
OBSERVER = repo_path("tools/bizhawk/sram_lifecycle_observer.lua")
H2_FIXTURE = repo_path("tests/fixtures/h2/tech-services-static-v1.json")
H2_SCHEMA = repo_path("schemas/h2-tech-services-static-fixture.schema.json")
TOOLCHAIN_MANIFEST = repo_path("manifests/toolchain.json")

OWNER = "sram-lifecycle"
STATUS_PREFIX = CALLBACK_FAILURE_PREFIX
OBSERVER_FAILURE_CONTRACT = observer_failure_contract(OWNER)
SRAM_SOURCE_RELATIVE = Path("code/common/tech/sram/sramfunctions.asm")
CONST_SOURCE_RELATIVE = Path("sf2const.asm")
LISTING_RELATIVE = Path("build/sf2build-h1.lst")
SRAM_FUNCTIONS = (
    "CheckSram",
    "SaveGame",
    "LoadGame",
    "CopySave",
    "ClearSaveSlotFlag",
    "CopyBytesToSram",
    "CopyBytesFromSram",
)
FUNCTION_FOR_OPERATION = {
    "check": "CheckSram",
    "save": "SaveGame",
    "load": "LoadGame",
    "copy": "CopySave",
    "clear": "ClearSaveSlotFlag",
}
CASE_MATRIX = (
    ("signature-mismatch-init", "check", 0),
    ("valid-signature-empty", "check", 0),
    ("valid-slot1", "check", 0),
    ("valid-slot2", "check", 1),
    ("invalid-slot1-clears-bit0", "check", 0),
    ("invalid-slot2-clears-bit1", "check", 1),
    ("save-game-slot1", "save", 0),
    ("save-game-slot2", "save", 1),
    ("load-game-slot1", "load", 0),
    ("load-game-slot2", "load", 1),
    ("copy-save-1-to-2", "copy", 0),
    ("copy-save-2-to-1", "copy", 1),
    ("clear-save-slot-flag-selector0", "clear", 0),
    ("clear-save-slot-flag-selector1", "clear", 1),
)


def _noncomment_instructions(source: str, symbol: str) -> list[tuple[str, str]]:
    """Parse only real instructions in one named source section."""
    start = re.search(rf"^{re.escape(symbol)}:\s*$", source, re.MULTILINE)
    if not start:
        raise ValueError(f"SRAM source guard missing function section: {symbol}")
    end = source.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"SRAM source guard missing function end marker: {symbol}")
    records: list[tuple[str, str]] = []
    for raw in source[start.start() : end].splitlines():
        line = raw.split(";", 1)[0].strip()
        if not line or line.endswith(":"):
            continue
        match = re.fullmatch(r"([A-Za-z][A-Za-z0-9]*(?:\.[bwls])?)\s*(.*)", line)
        if not match:
            raise ValueError(f"SRAM source guard cannot parse instruction: {raw!r}")
        records.append((match.group(1).lower(), re.sub(r"\s+", "", match.group(2)).lower()))
    return records


def _require_order(
    source: str, symbol: str, required: tuple[tuple[str, str], ...]
) -> None:
    instructions = _noncomment_instructions(source, symbol)
    cursor = 0
    for expected in required:
        while cursor < len(instructions) and instructions[cursor] != expected:
            cursor += 1
        if cursor == len(instructions):
            opcode, operand = expected
            raise ValueError(
                f"SRAM source guard semantic drift in {symbol}: expected {opcode} {operand}"
            )
        cursor += 1


def _signature_bytes(source: str, write_count: int) -> bytes:
    match = re.search(
        r"^SramCheckString:\s*dc\.b\s*'([^']*)'\s*,\s*\$([0-9A-F]{2})\s*$",
        source,
        re.MULTILINE,
    )
    if not match:
        raise ValueError("SRAM source guard missing exact SramCheckString declaration")
    declared = match.group(1).encode("ascii") + bytes((int(match.group(2), 16),))
    if write_count <= 0 or write_count > len(declared):
        raise ValueError("SRAM signature write-count derivation drift")
    return declared[:write_count]


def _equate(source: str, name: str) -> int:
    match = re.search(rf"^{re.escape(name)}:\s+equ\s+(\$[0-9A-F]+|\d+)", source, re.MULTILINE)
    if not match:
        raise ValueError(f"SRAM source guard missing constant: {name}")
    raw = match.group(1)
    return int(raw[1:], 16) if raw.startswith("$") else int(raw)


def _require_source_shape(facts: dict[str, Any], source: str) -> bytes:
    """Bind each promoted H2 relationship to a mutation-sensitive source use site."""
    layout = facts["layout"]
    slot1_bit = layout["occupiedFlagBits"]["slot1"]
    slot2_bit = layout["occupiedFlagBits"]["slot2"]
    for name in SRAM_FUNCTIONS:
        _noncomment_instructions(source, name)
    _require_order(
        source,
        "CheckSram",
        (
            ("cmpm.b", "(a0)+,(a1)+"),
            ("dbne", "d7,@checksramstring_loop"),
            ("bne.w", "@initsram"),
            ("btst", f"#{slot2_bit},(save_flags).l"),
            ("lea", "(save2_data).l,a0"),
            ("bsr.w", "copybytesfromsram"),
            ("cmp.b", "(save2_checksum).l,d0"),
            ("bclr", f"#{slot2_bit},(save_flags).l"),
            ("btst", f"#{slot1_bit},(save_flags).l"),
            ("lea", "(save1_data).l,a0"),
            ("bsr.w", "copybytesfromsram"),
            ("cmp.b", "(save1_checksum).l,d0"),
            ("bclr", f"#{slot1_bit},(save_flags).l"),
            ("lea", "(sram_start).l,a0"),
            ("move.w", "#sram_bytes_counter,d7"),
            ("clr.b", "(a0)"),
            ("addq.l", "#2,a0"),
            ("dbf", "d7,@clearsram_loop"),
            ("lea", "sramcheckstring(pc),a0"),
            ("bsr.w", "copybytestosram"),
            ("clr.b", "(save_flags).l"),
        ),
    )
    _require_order(
        source,
        "SaveGame",
        (
            ("lea", "(combatant_data).l,a0"),
            ("tst.b", "d0"),
            ("lea", "(save1_data).l,a1"),
            ("lea", "(save1_checksum).l,a2"),
            ("lea", "(save2_data).l,a1"),
            ("lea", "(save2_checksum).l,a2"),
            ("move.w", "#save_slot_real_size,d7"),
            ("bsr.w", "copybytestosram"),
            ("move.b", "d0,(a2)"),
            ("bset", "d1,(save_flags).l"),
        ),
    )
    _require_order(
        source,
        "LoadGame",
        (
            ("lea", "(combatant_data).l,a1"),
            ("tst.b", "d0"),
            ("lea", "(save1_data).l,a0"),
            ("lea", "(save2_data).l,a0"),
            ("move.w", "#save_slot_real_size,d7"),
            ("bsr.w", "copybytesfromsram"),
        ),
    )
    _require_order(
        source,
        "CopySave",
        (
            ("bsr.s", "loadgame"),
            ("eori.w", "#1,d0"),
            ("andi.w", "#1,d0"),
            ("bsr.s", "savegame"),
        ),
    )
    _require_order(
        source,
        "ClearSaveSlotFlag",
        (
            ("tst.b", "d0"),
            ("bclr", f"#{slot1_bit},(save_flags).l"),
            ("bclr", f"#{slot2_bit},(save_flags).l"),
        ),
    )
    _require_order(
        source,
        "CopyBytesToSram",
        (
            ("clr.w", "d0"),
            ("subq.w", "#1,d7"),
            ("move.b", "(a0),(a1)"),
            ("add.b", "(a0)+,d0"),
            ("addq.l", "#2,a1"),
        ),
    )
    _require_order(
        source,
        "CopyBytesFromSram",
        (
            ("clr.w", "d0"),
            ("subq.w", "#1,d7"),
            ("move.b", "(a0),(a1)+"),
            ("add.b", "(a0),d0"),
            ("addq.l", "#2,a0"),
        ),
    )
    return _signature_bytes(source, facts["constants"]["sizes"]["SRAM_STRING_WRITE_COUNTER"])


def _owner_facts(fixture: dict[str, Any], h2_fixture: dict[str, Any]) -> dict[str, Any]:
    provenance = fixture["provenance"]
    pinned = load_json(TOOLCHAIN_MANIFEST)["sf2disasm"]
    if (
        fixture["romSha256"] != h2_fixture["romSha256"]
        or provenance["h2Fixture"] != H2_FIXTURE.relative_to(repo_path(".")).as_posix()
        or provenance["h2FixtureId"] != h2_fixture["id"]
        or provenance["upstreamCommit"] != h2_fixture["upstreamCommit"]
        or provenance["upstreamRepository"] != pinned["repository"].removesuffix(".git")
        or provenance["upstreamCommit"] != pinned["commit"]
    ):
        raise ValueError("SRAM lifecycle fixture provenance disagrees with H2 tech-services owner")
    facts = h2_fixture["expected"]["sramFacts"]
    if (
        facts["sourcePath"] != fixture["provenance"]["sourcePath"]
        or tuple(facts["functionEntries"]) != SRAM_FUNCTIONS
        or facts["layout"]["logicalSlotCount"] != len(facts["layout"]["occupiedFlagBits"])
    ):
        raise ValueError("SRAM lifecycle H2 owner shape drift")
    return facts


def build_static_contract(
    fixture: dict[str, Any],
    upstream_path: Path,
    *,
    h2_fixture_path: Path = H2_FIXTURE,
    source_text: str | None = None,
    listing_text: str | None = None,
) -> dict[str, Any]:
    """Derive all runtime inputs from the accepted H2 owner, H1, and source use sites."""
    h2_fixture = load_json(h2_fixture_path)
    facts = _owner_facts(fixture, h2_fixture)
    disasm = upstream_path.resolve(strict=True) / "disasm"
    source = source_text or (disasm / SRAM_SOURCE_RELATIVE).read_text(encoding="utf-8")
    const_source = (disasm / CONST_SOURCE_RELATIVE).read_text(encoding="utf-8")
    listing = listing_text or (
        upstream_path.resolve(strict=True) / LISTING_RELATIVE
    ).read_text(encoding="utf-8")
    signature = _require_source_shape(facts, source)
    if facts["operations"].get("copyLoadsSelectedSlotThenSavesToOtherSlot") is not True:
        raise ValueError("SRAM H2 CopySave operation fact disagrees with source shape")
    h1_entries = listing_symbol_addresses(listing)
    function_entries = facts["functionEntries"]
    for name in SRAM_FUNCTIONS:
        if h1_entries.get(name) != function_entries[name]:
            raise ValueError(f"SRAM H2/H1 entry derivation drift: {name}")
    if fixture["function"]["checkSramAddress"] != function_entries["CheckSram"]:
        raise ValueError("SRAM fixture CheckSram address does not derive from H2 owner")
    addresses = facts["constants"]["addresses"]
    layout = facts["layout"]
    size = layout["logicalBytesPerSlot"]
    interval = layout["physicalAddressIntervalPerSlot"]
    step = layout["physicalAddressStepPerLogicalByte"]
    if size != facts["constants"]["sizes"]["SAVE_SLOT_REAL_SIZE"]:
        raise ValueError("SRAM logical payload size no longer derives from H2 size constant")
    if interval != facts["constants"]["sizes"]["SAVE_SLOT_SIZE"]:
        raise ValueError("SRAM physical interval no longer derives from H2 size constant")
    if interval != size * step:
        raise ValueError("SRAM storage stride derivation drift")
    if layout["fullClearLogicalByteCount"] != facts["constants"]["sizes"]["SRAM_BYTES_COUNTER"] + 1:
        raise ValueError("SRAM full-clear counter derivation drift")
    return {
        "functionEntries": function_entries,
        "addresses": addresses,
        "layout": layout,
        "signatureBytes": list(signature),
        "ram": {"combatantDataAddress": _equate(const_source, "COMBATANT_DATA")},
        "storage": {
            "physicalWindowBaseAddress": addresses["SRAM_START"] & ~1,
            "firstStoredPhysicalByteAddress": addresses["SRAM_START"],
            "slotDataAddresses": {
                "slot1": addresses["SAVE1_DATA"],
                "slot2": addresses["SAVE2_DATA"],
            },
            "slotChecksumAddresses": {
                "slot1": addresses["SAVE1_CHECKSUM"],
                "slot2": addresses["SAVE2_CHECKSUM"],
            },
            "saveFlagsAddress": addresses["SAVE_FLAGS"],
            "signatureAddress": addresses["SRAM_STRING"],
        },
        "copyFlow": {
            "loadCallPc": _h1_instruction_address(listing, "CopySave", "bsr.s LoadGame")[0],
            "loadReturnPc": _h1_instruction_address(listing, "CopySave", "bsr.s LoadGame")[1],
            "saveCallPc": _h1_instruction_address(listing, "CopySave", "bsr.s SaveGame")[0],
            "saveReturnPc": _h1_instruction_address(listing, "CopySave", "bsr.s SaveGame")[1],
        },
    }


def _h1_first_instruction(listing: str, symbol: str) -> bytes:
    start = re.search(rf"^[0-9A-F]{{8}}\s+{re.escape(symbol)}:\s*$", listing, re.MULTILINE)
    if not start:
        raise ValueError(f"SRAM H1 guard missing symbol: {symbol}")
    match = re.search(r"^[0-9A-F]{8}\s+((?:[0-9A-F]{4}\s+)+)", listing[start.end() :], re.MULTILINE)
    if not match:
        raise ValueError(f"SRAM H1 guard missing first instruction: {symbol}")
    return bytes.fromhex(re.sub(r"\s+", "", match.group(1)))


def _h1_instruction_address(
    listing: str, symbol: str, instruction: str
) -> tuple[int, int]:
    """Return one H1 instruction and its encoded fall-through address."""
    start = re.search(rf"^[0-9A-F]{{8}}\s+{re.escape(symbol)}:\s*$", listing, re.MULTILINE)
    if not start:
        raise ValueError(f"SRAM H1 guard missing symbol: {symbol}")
    end = listing.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"SRAM H1 guard missing end marker: {symbol}")
    matches = []
    for line in listing[start.end() : end].splitlines():
        match = re.fullmatch(
            r"([0-9A-F]{8})\s+((?:[0-9A-F]{4}\s+)+)\s+(.+?)\s*", line
        )
        if match and re.sub(r"\s+", " ", match.group(3).strip()) == instruction:
            encoded = re.sub(r"\s+", "", match.group(2))
            matches.append((int(match.group(1), 16), len(encoded) // 2))
    if len(matches) != 1:
        raise ValueError(f"SRAM H1 guard expected one {symbol} instruction: {instruction}")
    address, width = matches[0]
    return address, address + width


def validate_static_contract(
    fixture: dict[str, Any], rom_path: Path, upstream_path: Path
) -> dict[str, Any]:
    static = build_static_contract(fixture, upstream_path)
    listing = (upstream_path.resolve(strict=True) / LISTING_RELATIVE).read_text(encoding="utf-8")
    rom = rom_path.resolve(strict=True).read_bytes()
    for symbol, address in static["functionEntries"].items():
        opcode = _h1_first_instruction(listing, symbol)
        if rom[address : address + len(opcode)] != opcode:
            raise ValueError(f"SRAM H1/ROM first-instruction guard drift: {symbol}")
    return static


def _pattern_byte(seed: int, logical_offset: int) -> int:
    return (seed + 17 * logical_offset + 29 * (logical_offset // 8)) & 0xFF


def _span(bytes_: list[int], sentinel_offsets: list[int]) -> dict[str, Any]:
    return {
        "logicalByteCount": len(bytes_),
        "checksumByte": sum(bytes_) & 0xFF,
        "mismatchCount": 0,
        "boundary": {"first": bytes_[0], "last": bytes_[-1]},
        "sentinels": [
            {"logicalOffset": offset, "byte": bytes_[offset]} for offset in sentinel_offsets
        ],
    }


def _seed_bytes(seed: int, size: int) -> list[int]:
    return [_pattern_byte(seed, offset) for offset in range(size)]


def _setup_checksum(mode: str, bytes_: list[int]) -> int:
    checksum = sum(bytes_) & 0xFF
    return checksum if mode == "computed" else (checksum + 1) & 0xFF


def expected_observation(
    fixture: dict[str, Any], static: dict[str, Any]
) -> dict[str, Any]:
    """Independent static model for every compact whole-span runtime record."""
    size = static["layout"]["logicalBytesPerSlot"]
    full_size = static["layout"]["fullClearLogicalByteCount"]
    sentinel_offsets = fixture["pattern"]["sentinelOffsets"]
    slot_bits = static["layout"]["occupiedFlagBits"]
    slots = ("slot1", "slot2")
    records: list[dict[str, Any]] = []
    if [case["id"] for case in fixture["cases"]] != fixture["caseOrder"]:
        raise ValueError("SRAM lifecycle fixture case order drift")
    if tuple(
        (case["id"], case["operation"], case["selector"]) for case in fixture["cases"]
    ) != CASE_MATRIX:
        raise ValueError("SRAM lifecycle fixture case operation/selector drift")
    for case in fixture["cases"]:
        setup = case["setup"]
        expected = case["expected"]
        payload = {slot: _seed_bytes(setup[f"{slot}Seed"], size) for slot in slots}
        stored = {
            slot: _setup_checksum(setup[f"{slot}Checksum"], payload[slot]) for slot in slots
        }
        flags = setup["flags"]
        operation = case["operation"]
        selector = case["selector"]
        selected = "slot1" if selector == 0 else "slot2"
        other = "slot2" if selected == "slot1" else "slot1"
        combatant: list[int] | None = None
        full_sram_fact: dict[str, Any] | None = None
        result_d0 = selector
        result_d1 = 0
        expected_slots = [selected] if operation in ("check", "save", "load") else list(slots)
        if operation == "check" and setup["signature"] == "mismatch":
            payload = {slot: [0] * size for slot in slots}
            stored = {slot: 0 for slot in slots}
            flags = 0
            result_d0 = 0
            result_d1 = 0
            full = [0] * full_size
            base = static["addresses"]["SRAM_START"]
            signature_index = (static["addresses"]["SRAM_STRING"] - base) // static["layout"][
                "physicalAddressStepPerLogicalByte"
            ]
            for offset, byte in enumerate(static["signatureBytes"]):
                full[signature_index + offset] = byte
            full_sram_fact = {
                "logicalByteCount": full_size,
                "checksumByte": sum(full) & 0xFF,
                "mismatchCount": 0,
                "boundary": {"first": full[0], "last": full[-1]},
            }
        elif operation == "check":
            for slot, register in (("slot2", "d1"), ("slot1", "d0")):
                bit = slot_bits[slot]
                result = 0
                if flags & (1 << bit):
                    result = 1 if stored[slot] == (sum(payload[slot]) & 0xFF) else -1
                    if result == -1:
                        flags &= ~(1 << bit)
                if register == "d0":
                    result_d0 = result
                else:
                    result_d1 = result
        elif operation == "save":
            combatant = _seed_bytes(setup["ramSeed"], size)
            payload[selected] = combatant
            stored[selected] = sum(combatant) & 0xFF
            flags |= 1 << slot_bits[selected]
        elif operation == "load":
            combatant = payload[selected]
        elif operation == "copy":
            combatant = payload[selected]
            payload[other] = combatant.copy()
            stored[other] = sum(combatant) & 0xFF
            flags |= 1 << slot_bits[other]
        elif operation == "clear":
            flags &= ~(1 << slot_bits[selected])
        if (
            flags != expected["saveFlags"]
            or result_d0 != expected["resultD0"]
            or result_d1 != expected["resultD1"]
            or expected_slots != expected["observedSlots"]
            or (full_sram_fact is not None) != expected["fullSramInitialized"]
        ):
            raise ValueError(f"SRAM fixture static model semantic drift: {case['id']}")
        record = {
            "id": case["id"],
            "resultD0": result_d0,
            "resultD1": result_d1,
            "saveFlags": flags,
            "slotFacts": [
                {
                    "slot": slot,
                    "storedChecksumByte": stored[slot],
                    "span": _span(payload[slot], sentinel_offsets),
                }
                for slot in expected["observedSlots"]
            ],
            "combatantFacts": _span(combatant, sentinel_offsets)
            if combatant is not None
            else None,
            "fullSramFact": full_sram_fact,
        }
        records.append(record)
    return {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["id"],
        "caseOrder": fixture["caseOrder"],
        "records": records,
        "sramResidueZero": True,
        "callbacksCleared": 0,
    }


def _assert_observation(
    fixture: dict[str, Any], static: dict[str, Any], observed: dict[str, Any]
) -> None:
    validate_json(observed, OBSERVATION_SCHEMA, owner="SRAM lifecycle observation")
    expected = expected_observation(fixture, static)
    if observed != expected:
        raise ValueError(
            "SRAM lifecycle runtime matrix mismatch\n"
            f"expected={expected!r}\nobserved={observed!r}"
        )


def _assert_status(status_path: Path) -> None:
    diagnostic = _failure_diagnostic(status_path)
    if diagnostic is not None:
        raise RuntimeError(f"{OWNER} observer callback failure: {diagnostic}")
    assert_observer_status(
        status_path,
        owner=OWNER,
        schema_path=FAILURE_SCHEMA,
        required_milestones=("milestone:direct-function-probe",),
    )


def _failure_diagnostic(status_path: Path) -> str | None:
    payload = callback_failure_status(status_path, owner=OWNER, schema_path=FAILURE_SCHEMA)
    if payload is None:
        return None
    lines = status_path.read_text(encoding="utf-8").splitlines()
    failure_indices = [
        index for index, line in enumerate(lines) if line.startswith(STATUS_PREFIX)
    ]
    if len(failure_indices) != 1 or failure_indices[0] != len(lines) - 1:
        raise ValueError("SRAM callback failure status must be one terminal exact failure line")
    if not any(line.startswith("milestone:") for line in lines[: failure_indices[0]]):
        raise ValueError("SRAM callback failure status lacks preceding milestone")
    return str(payload)


def verify_sram_lifecycle(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 180
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner="SRAM lifecycle fixture")
    h2_fixture = load_json(H2_FIXTURE)
    validate_json(h2_fixture, H2_SCHEMA, owner="SRAM H2 owner fixture")
    verify_runtime_contract(fixture, rom_path)
    static = validate_static_contract(fixture, rom_path, upstream_path)
    try:
        observed = run_observer(
            rom_path=rom_path,
            observer_path=OBSERVER,
            config={
                "id": fixture["id"],
                "core": fixture["emulator"]["core"],
                "cases": fixture["cases"],
                "caseOrder": fixture["caseOrder"],
                "pattern": fixture["pattern"],
                "static": static,
                "observerFailureContract": OBSERVER_FAILURE_CONTRACT,
            },
            output_name=OWNER,
            timeout_seconds=timeout_seconds,
        )
    except RuntimeError as error:
        diagnostic = _failure_diagnostic(DERIVED_ROOT / f"{OWNER}.status.txt")
        if diagnostic is not None:
            raise RuntimeError(f"{OWNER} observer callback failure: {diagnostic}") from error
        raise
    _assert_status(DERIVED_ROOT / f"{OWNER}.status.txt")
    _assert_observation(fixture, static, observed)
    return {
        "Fixture": fixture["id"],
        "Cases": len(fixture["cases"]),
        "LogicalBytesPerSlot": static["layout"]["logicalBytesPerSlot"],
        "BizHawkLaunches": 1,
        "CallbacksCleared": observed["callbacksCleared"],
        "SramResidueZero": observed["sramResidueZero"],
        "Status": "PASS",
    }
