"""Static contract for the Church save branch and its bounded H3 observer.

The rail enters the original ``ChurchMenu`` and only replaces presentation and
input seams.  The Church action dispatcher, map-to-Egress copy, save routine,
and terminal targets remain source-backed original code.
"""

from __future__ import annotations

import hashlib
import json
import re
import tempfile
from pathlib import Path
from typing import Any

from sf2tool.h3.bizhawk import run_observer
from sf2tool.h3.map_lifecycle import _with_instrumented_rom_database
from sf2tool.h3.observer_status import (
    CALLBACK_FAILURE_PREFIX,
    assert_observer_status,
    callback_failure_status,
    observer_failure_contract,
)
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

FIXTURE = repo_path("tests/fixtures/h3/church-save-lifecycle-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h3-church-save-lifecycle-fixture.schema.json")
OBSERVATION_SCHEMA = repo_path("schemas/h3-church-save-lifecycle-observation.schema.json")
FAILURE_SCHEMA = repo_path("schemas/h3/church-save-lifecycle-callback-failure.schema.json")
OBSERVER = repo_path("tools/bizhawk/church_save_lifecycle_observer.lua")
MAP_CONTENT_FIXTURE = repo_path("tests/fixtures/h2/map-content-static-v1.json")
MAP_CONTENT_FIXTURE_SCHEMA = repo_path("schemas/h2-map-content-static-fixture.schema.json")
UPSTREAM = repo_path("local/upstream/SF2DISASM")
DISASM = UPSTREAM / "disasm"
LISTING = UPSTREAM / "build/sf2build-h1.lst"
OWNER = "church-save-lifecycle"
CANONICAL_ROM_SHA256 = "9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9"
ROM_SHA256 = CANONICAL_ROM_SHA256
MAP_CONTENT_ID = "sf2-map-content-static-v1"
MAP_CONTENT_UPSTREAM_COMMIT = "c834c652b6862bc5679fd7f69a38a7093206efc6"
OBSERVED_OUTPUT = repo_path(f"local/derived/h3/{OWNER}.observed.json")
STATUS_PREFIX = CALLBACK_FAILURE_PREFIX
OBSERVER_FAILURE_CONTRACT = observer_failure_contract(OWNER)

CHURCH = Path("code/common/menus/church/churchactions_1.asm")
SRAM = Path("code/common/tech/sram/sramfunctions.asm")
WITCH_SUSPEND = Path("code/specialscreens/suspend/witchsuspend.asm")
CONSTANTS = Path("sf2const.asm")
ENUMS = Path("sf2enums.asm")
MACROS = Path("sf2macros.asm")
GAME_FLAGS = Path("code/common/stats/gameflags.asm")

CASE_IDS = (
    "initial-save-prompt-decline-no-service",
    "slot1-map0-save-continue",
    "slot2-map78-save-continue",
    "slot1-existing-flag-save-continue",
    "slot2-save-rest-suspend-boundary",
)

LUA_REQUIRED_CALLBACK_ROLES = frozenset(
    {
        "bootstrap-check-sram",
        "case-entry",
        "case-result",
        "terminal-finalize",
        "church-entry",
        "start-save",
        "action-return",
        "first-prompt-call",
        "first-prompt-return",
        "do-save-game",
        "save-game-call",
        "save-game-entry",
        "save-game-rts",
        "save-game-return",
        "post-save-prompt-call",
        "post-save-prompt-return",
        "exit-save",
        "exit-menu",
        "fade-call",
        "fade-entry",
        "fade-return",
        "witch-tail-jump",
        "witch-suspend-entry",
    }
)
LUA_FAILURE_ROLES = LUA_REQUIRED_CALLBACK_ROLES | {
    "registration",
    "bootstrap-watchdog",
    "case-watchdog",
}

ADDRESSES = {
    "churchMenu": 0x20A02,
    "actionReturn": 0x20A36,
    "startSave": 0x20FCC,
    "doSaveGame": 0x20FE6,
    "exitSave": 0x21028,
    "exitMenu": 0x20A40,
    "firstPromptCall": 0x20FD0,
    "firstPromptReturn": 0x20FD6,
    "firstPromptCompare": 0x20FD6,
    "firstPromptAcceptBranch": 0x20FDA,
    "currentMapToEgress": 0x20FE6,
    "currentSaveSlotLoad": 0x20FEC,
    "flag399Trap": 0x20FF0,
    "flag399Operand": 0x20FF2,
    "saveGameCall": 0x20FF4,
    "saveGameReturn": 0x20FF8,
    "postPromptCall": 0x2100A,
    "postPromptReturn": 0x21010,
    "postPromptContinueBranch": 0x21014,
    "fadeCall": 0x2101C,
    "fadeReturn": 0x21020,
    "witchSuspendTailJump": 0x21020,
    "saveGame": 0x6F6A,
    "saveGameRts": 0x6FAA,
    "witchSuspend": 0x7034,
    "fadeOutToBlack": 0xCE0,
}

SESSION_PATCH_SPECS = (
    (0x20A18, "4E45006E4E45FFFF", "4E714E714E714E71", "entry-presentation-bypass"),
    (0x20A20, "4EB90001003C", "4E714E714E71", "entry-portrait-close-bypass"),
    (0x20A30, "4EB900010000", "4EB900FF6D00", "controlled-save-selection"),
    (0x20FCC, "4E450072", "4E714E71", "save-prompt-text-bypass"),
    (0x20FD0, "4EB900010074", "4EB900FF6D10", "controlled-first-prompt"),
    (0x20FDE, "4E45007C", "4E714E71", "decline-text-bypass"),
    (0x20FF8, "4E4000154EBA023A4E71", "4E714E714E714E714E71", "save-presentation-bypass"),
    (0x21002, "4E450073", "4E714E71", "post-save-text-one-bypass"),
    (0x21006, "4E450074", "4E714E71", "post-save-text-two-bypass"),
    (0x2100A, "4EB900010074", "4EB900FF6D10", "controlled-post-save-prompt"),
    (0x21018, "4E450075", "4E714E71", "suspend-text-bypass"),
    (0x21028, "4E45FFFF4E450070", "4EF900FF6D204E71", "exit-save-result-redirect"),
    (0x20A40, "72003038B0A0", "4EF900FF6D20", "exit-menu-result-redirect"),
    (0x0CE0, "11FC0002DEF0", "4E754E714E71", "controlled-fade-return"),
    (0x7034, "4E40000B61000BA4", "4EF900FF6D404E71", "witch-suspend-entry-boundary"),
)


def _normal(line: str) -> str:
    return re.sub(r"\s+", " ", line.split(";", 1)[0].strip()).lower()


def _equates(source: str) -> dict[str, int]:
    values: dict[str, int] = {}
    for name, literal in re.findall(
        r"^([A-Z][A-Z0-9_]*):\s+equ\s+(\$[0-9A-F]+|\d+)", source, re.MULTILINE
    ):
        values[name] = int(literal[1:], 16) if literal.startswith("$") else int(literal)
    return values


def _section(source: str, symbol: str) -> str:
    start = re.search(rf"^{re.escape(symbol)}:\s*$", source, re.MULTILINE)
    if start is None:
        raise ValueError(f"church save source function missing: {symbol}")
    end = source.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"church save source function end missing: {symbol}")
    return source[start.start() : end]


def _require_order(source: str, symbol: str, fragments: tuple[str, ...]) -> None:
    rows = [_normal(row) for row in _section(source, symbol).splitlines()]
    cursor = 0
    for fragment in fragments:
        expected = _normal(fragment)
        try:
            cursor = rows.index(expected, cursor) + 1
        except ValueError as error:
            raise ValueError(f"church save source guard drift in {symbol}: {expected!r}") from error


def _listing_section(listing: str, symbol: str) -> tuple[dict[str, int], list[dict[str, Any]]]:
    start = re.search(rf"^[0-9A-F]{{8}}\s+{re.escape(symbol)}:\s*$", listing, re.MULTILINE)
    if start is None:
        raise ValueError(f"church save H1 function missing: {symbol}")
    end = listing.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"church save H1 function end missing: {symbol}")
    labels: dict[str, int] = {}
    rows: list[dict[str, Any]] = []
    for line in listing[start.start() : end].splitlines():
        label = re.fullmatch(r"([0-9A-F]{8})\s+([@A-Za-z_][@A-Za-z0-9_]*):\s*", line)
        if label:
            labels[label.group(2)] = int(label.group(1), 16)
            continue
        instruction = re.fullmatch(r"([0-9A-F]{8})\s+((?:[0-9A-F]{4}\s+)+)(.+?)\s*", line)
        if instruction:
            rows.append(
                {
                    "address": int(instruction.group(1), 16),
                    "hex": re.sub(r"\s+", "", instruction.group(2)),
                    "text": re.sub(r"\s+", " ", instruction.group(3).strip()),
                }
            )
    if symbol not in labels:
        raise ValueError(f"church save H1 entry label omitted: {symbol}")
    return labels, rows


def _row(rows: list[dict[str, Any]], address: int, fragment: str) -> dict[str, Any]:
    matches = [
        row for row in rows if row["address"] == address and fragment.lower() in row["text"].lower()
    ]
    if len(matches) != 1:
        raise ValueError(f"church save H1 use-site drift at 0x{address:X}: expected {fragment!r}")
    return matches[0]


def _rom_hex(rom: bytes, address: int, width: int) -> str:
    value = rom[address : address + width].hex().upper()
    if len(value) != width * 2:
        raise ValueError(f"church save ROM is short at 0x{address:X}")
    return value


def _listing_span_hex(listing: str, address: int, width: int) -> str:
    cells: dict[int, int] = {}
    for line in listing.splitlines():
        match = re.match(r"^([0-9A-F]{8})\s+((?:[0-9A-F]{4}\s+)+)", line)
        if match is None:
            continue
        row_address = int(match.group(1), 16)
        row = bytes.fromhex(re.sub(r"\s+", "", match.group(2)))
        for offset, value in enumerate(row):
            cell = row_address + offset
            if address <= cell < address + width:
                if cell in cells:
                    raise ValueError(f"church save H1 patch-cell overlap at 0x{cell:X}")
                cells[cell] = value
    if len(cells) != width:
        raise ValueError(f"church save H1 patch span incomplete at 0x{address:X}")
    return bytes(cells[cell] for cell in range(address, address + width)).hex().upper()


def _session_patch_plan(listing: str, rom: bytes) -> list[dict[str, Any]]:
    patches: list[dict[str, Any]] = []
    for address, original_hex, patched_hex, purpose in SESSION_PATCH_SPECS:
        width = len(bytes.fromhex(original_hex))
        if len(bytes.fromhex(patched_hex)) != width:
            raise ValueError(f"church save session patch width drift: {purpose}")
        h1_hex = _listing_span_hex(listing, address, width)
        # H1 leaves selected PC-relative displacement words unresolved.  ROM
        # identity plus the dedicated target checks below provides that binding.
        relocated_h1 = {
            "4E4000154EBA00004E71",
            "4E40000B61000000",
        }
        if h1_hex != original_hex and h1_hex not in relocated_h1:
            raise ValueError(f"church save session patch H1 drift at 0x{address:X}")
        if _rom_hex(rom, address, width) != original_hex:
            raise ValueError(f"church save session original-byte drift at 0x{address:X}")
        patches.append(
            {
                "address": address,
                "width": width,
                "originalHex": original_hex,
                "hex": patched_hex,
                "purpose": purpose,
            }
        )
    return patches


def _assert_rom_instruction(rom: bytes, row: dict[str, Any]) -> None:
    width = len(row["hex"]) // 2
    expected = row["hex"]
    actual = _rom_hex(rom, row["address"], width)
    # The H1 listing intentionally retains unresolved PC-relative word
    # displacements as zero.  H1 still fixes the opcode and width; the ROM
    # target is independently resolved below from the candidate instruction.
    branches = ("beq", "bne", "bra", "bsr", "jsr")
    if expected.endswith("0000") and row["text"].lower().startswith(branches):
        expected, actual = expected[:-4], actual[:-4]
    if actual != expected:
        raise ValueError(f"church save H1/ROM use-site drift at 0x{row['address']:X}")


def _absolute_long_target(rom: bytes, address: int, symbol: str) -> tuple[int, int]:
    if _rom_hex(rom, address, 2) != "4EB9":
        raise ValueError(f"church save ROM call opcode drift for {symbol} at 0x{address:X}")
    return int.from_bytes(rom[address + 2 : address + 6], "big"), address + 6


def _absolute_word_target(rom: bytes, address: int, opcode: str, symbol: str) -> int:
    if _rom_hex(rom, address, 2) != opcode:
        raise ValueError(f"church save ROM target opcode drift for {symbol} at 0x{address:X}")
    return int.from_bytes(rom[address + 2 : address + 4], "big")


def _branch_target(rom: bytes, address: int, opcode: str, symbol: str) -> tuple[int, int]:
    if _rom_hex(rom, address, 2) != opcode:
        raise ValueError(f"church save ROM branch opcode drift for {symbol} at 0x{address:X}")
    displacement = int.from_bytes(rom[address + 2 : address + 4], "big", signed=True)
    return address + 2 + displacement, address + 4


def _source_guards(
    church: str, sram: str, witch_suspend: str, macros: str, game_flags: str
) -> None:
    _require_order(
        church,
        "ChurchMenu",
        (
            "cmpi.w #0,d0",
            "bne.w @CheckCureAction",
            "cmpi.w #1,d0",
            "bne.w @CheckPromoAction",
            "cmpi.w #2,d0",
            "bne.w @StartSave",
            "@StartSave:",
            "txt 114",
            "jsr j_alt_YesNoPrompt",
            "cmpi.w #0,d0",
            "beq.w @DoSaveGame",
            "bra.w @ExitSave",
            "@DoSaveGame:",
            "move.b ((CURRENT_MAP-$1000000)).w,((EGRESS_MAP-$1000000)).w",
            "move.w ((CURRENT_SAVE_SLOT-$1000000)).w,d0",
            "setFlg 399",
            "jsr (SaveGame).w",
            "jsr j_alt_YesNoPrompt",
            "cmpi.w #0,d0",
            "beq.w @ExitMenu",
            "jsr (FadeOutToBlack).w",
            "jmp (WitchSuspend).w",
            "@ExitSave:",
        ),
    )
    church_menu_lines = _section(church, "ChurchMenu").splitlines()
    if _normal("@ExitMenu:") not in {_normal(row) for row in church_menu_lines}:
        raise ValueError("church save source exit-menu label drift")
    _require_order(
        sram,
        "SaveGame",
        (
            "tst.b d0",
            "bne.s @Slot2",
            "lea (SAVE1_DATA).l,a1",
            "lea (SAVE1_CHECKSUM).l,a2",
            "@Slot2:",
            "lea (SAVE2_DATA).l,a1",
            "lea (SAVE2_CHECKSUM).l,a2",
            "@Continue:",
            "move.w #SAVE_SLOT_REAL_SIZE,d7",
            "bsr.w CopyBytesToSram",
            "move.b d0,(a2)",
            "bset d1,(SAVE_FLAGS).l",
            "rts",
        ),
    )
    _require_order(
        sram,
        "CopyBytesToSram",
        (
            "subq.w #1,d7",
            "@Loop:",
            "move.b (a0),(a1)",
            "add.b (a0)+,d0",
            "addq.l #2,a1",
            "dbf d7,@Loop",
        ),
    )
    _require_order(witch_suspend, "WitchSuspend", ("sndCom MUSIC_SUSPEND",))
    _require_order(
        game_flags,
        "GetFlag",
        (
            "andi.l #FLAG_MASK,d1",
            "divu.w #8,d1",
            "lea ((GAME_FLAGS-$1000000)).w,a0",
            "adda.w d1,a0",
            "swap d1",
            "moveq #$FFFFFF80,d0",
            "lsr.b d1,d0",
        ),
    )
    if _normal("setFlg: macro") not in {_normal(row) for row in macros.splitlines()}:
        raise ValueError("church save setFlg macro identity drift")


def _external_map_domain(map_fixture_path: Path = MAP_CONTENT_FIXTURE) -> dict[str, Any]:
    map_fixture = load_json(map_fixture_path)
    validate_json(
        map_fixture,
        MAP_CONTENT_FIXTURE_SCHEMA,
        owner="church save accepted map-content owner fixture",
    )
    if (
        map_fixture["id"] != MAP_CONTENT_ID
        or map_fixture["upstreamCommit"] != MAP_CONTENT_UPSTREAM_COMMIT
        or map_fixture["romSha256"] != CANONICAL_ROM_SHA256
    ):
        raise ValueError("church save map-content owner provenance drift")
    summary = map_fixture["summary"]
    count = summary["mapCount"]
    if not isinstance(count, int) or count < 1 or summary["mapEntryRomParityCount"] != count:
        raise ValueError("church save map-content owner count/parity drift")
    return {
        "minimum": 0,
        "maximum": count - 1,
        "count": count,
        "ownerFixture": "tests/fixtures/h2/map-content-static-v1.json",
        "ownerId": map_fixture["id"],
        "ownerUpstreamCommit": map_fixture["upstreamCommit"],
    }


def build_static_contract(
    rom_path: Path,
    upstream_path: Path = UPSTREAM,
    map_fixture_path: Path = MAP_CONTENT_FIXTURE,
) -> dict[str, Any]:
    """Derive Church save control flow and SRAM geometry from source/H1/ROM."""
    disasm = upstream_path / "disasm"
    church = (disasm / CHURCH).read_text(encoding="utf-8")
    sram = (disasm / SRAM).read_text(encoding="utf-8")
    witch_suspend = (disasm / WITCH_SUSPEND).read_text(encoding="utf-8")
    macros = (disasm / MACROS).read_text(encoding="utf-8")
    game_flags = (disasm / GAME_FLAGS).read_text(encoding="utf-8")
    _source_guards(church, sram, witch_suspend, macros, game_flags)

    constants = _equates((disasm / CONSTANTS).read_text(encoding="utf-8"))
    constants.update(_equates((disasm / ENUMS).read_text(encoding="utf-8")))
    required = (
        "CURRENT_MAP",
        "EGRESS_MAP",
        "CURRENT_SAVE_SLOT",
        "GAME_FLAGS",
        "FLAG_INDEX_BATTLE_CUTSCENE_GIZMOS",
        "SAVE_SLOT_REAL_SIZE",
        "SAVE1_DATA",
        "SAVE2_DATA",
        "SAVE1_CHECKSUM",
        "SAVE2_CHECKSUM",
        "SAVE_FLAGS",
        "CURRENT_PORTRAIT",
        "DIALOGUE_NAME_INDEX_1",
        "DIALOGUE_NUMBER",
    )
    missing = [name for name in required if name not in constants]
    if missing:
        raise ValueError(f"church save constants missing: {missing}")

    listing = (upstream_path / "build/sf2build-h1.lst").read_text(encoding="utf-8")
    church_labels, church_rows = _listing_section(listing, "ChurchMenu")
    save_labels, save_rows = _listing_section(listing, "SaveGame")
    copy_labels, copy_rows = _listing_section(listing, "CopyBytesToSram")
    witch_labels, witch_rows = _listing_section(listing, "WitchSuspend")
    flag_labels, flag_rows = _listing_section(listing, "GetFlag")
    for symbol, expected in (
        ("ChurchMenu", ADDRESSES["churchMenu"]),
        ("@StartSave", ADDRESSES["startSave"]),
        ("@DoSaveGame", ADDRESSES["doSaveGame"]),
        ("@ExitSave", ADDRESSES["exitSave"]),
        ("@ExitMenu", ADDRESSES["exitMenu"]),
        ("SaveGame", ADDRESSES["saveGame"]),
        ("WitchSuspend", ADDRESSES["witchSuspend"]),
        ("GetFlag", 0x98E8),
    ):
        labels = church_labels if symbol.startswith("@") or symbol == "ChurchMenu" else save_labels
        if symbol == "WitchSuspend":
            labels = witch_labels
        if symbol == "GetFlag":
            labels = flag_labels
        if labels.get(symbol) != expected:
            raise ValueError(f"church save H1 label drift for {symbol}")
    if save_labels.get("@Slot2") != 0x6F88 or save_labels.get("@Continue") != 0x6F96:
        raise ValueError("church save H1 slot-branch convergence drift")

    rom = rom_path.read_bytes()
    if hashlib.sha256(rom).hexdigest().upper() != ROM_SHA256:
        raise ValueError("church save input ROM identity drift")
    h1_rows = (
        (_row(church_rows, ADDRESSES["actionReturn"], "cmpi.w #-1,d0"), "action"),
        (_row(church_rows, ADDRESSES["firstPromptCall"], "jsr j_alt_YesNoPrompt"), "first"),
        (_row(church_rows, ADDRESSES["firstPromptCompare"], "cmpi.w #0,d0"), "first"),
        (_row(church_rows, ADDRESSES["firstPromptAcceptBranch"], "beq.w @DoSaveGame"), "first"),
        (_row(church_rows, ADDRESSES["currentMapToEgress"], "move.b"), "map"),
        (_row(church_rows, ADDRESSES["currentSaveSlotLoad"], "move.w"), "slot"),
        (_row(church_rows, ADDRESSES["flag399Trap"], "trap #set_flag"), "flag"),
        (_row(church_rows, ADDRESSES["saveGameCall"], "jsr (SaveGame).w"), "save"),
        (_row(church_rows, ADDRESSES["postPromptCall"], "jsr j_alt_YesNoPrompt"), "post"),
        (_row(church_rows, ADDRESSES["postPromptReturn"], "cmpi.w #0,d0"), "post"),
        (_row(church_rows, ADDRESSES["postPromptContinueBranch"], "beq.w @ExitMenu"), "post"),
        (_row(church_rows, ADDRESSES["fadeCall"], "jsr (FadeOutToBlack).w"), "fade"),
        (_row(church_rows, ADDRESSES["witchSuspendTailJump"], "jmp (WitchSuspend).w"), "witch"),
        (_row(save_rows, ADDRESSES["saveGame"], "movem.l"), "save-entry"),
        (_row(save_rows, ADDRESSES["saveGameRts"], "rts"), "save-rts"),
        (_row(copy_rows, 0x700C, "move.b (a0),(a1)"), "copy"),
        (_row(flag_rows, 0x98E8, "andi.l #FLAG_MASK,d1"), "flag-mask"),
        (_row(flag_rows, 0x98EE, "divu.w #8,d1"), "flag-byte"),
        (_row(flag_rows, 0x98FA, "moveq #$FFFFFF80,d0"), "flag-msb"),
        (_row(flag_rows, 0x98FC, "lsr.b d1,d0"), "flag-shift"),
    )
    for row, _name in h1_rows:
        _assert_rom_instruction(rom, row)
    flag_operand = _rom_hex(rom, ADDRESSES["flag399Operand"], 2)
    if flag_operand != f"{constants['FLAG_INDEX_BATTLE_CUTSCENE_GIZMOS']:04X}":
        raise ValueError("church save flag 399 operand drift")

    first_prompt_target, first_prompt_return = _absolute_long_target(
        rom, ADDRESSES["firstPromptCall"], "first prompt"
    )
    first_target, first_branch_return = _branch_target(
        rom, ADDRESSES["firstPromptAcceptBranch"], "6700", "first prompt accept"
    )
    save_target = _absolute_word_target(rom, ADDRESSES["saveGameCall"], "4EB8", "SaveGame")
    post_prompt_target, post_prompt_return = _absolute_long_target(
        rom, ADDRESSES["postPromptCall"], "post-save prompt"
    )
    post_target, post_branch_return = _branch_target(
        rom, ADDRESSES["postPromptContinueBranch"], "6700", "post-save continue"
    )
    fade_target = _absolute_word_target(rom, ADDRESSES["fadeCall"], "4EB8", "FadeOutToBlack")
    witch_target = _absolute_word_target(
        rom, ADDRESSES["witchSuspendTailJump"], "4EF8", "WitchSuspend"
    )
    if (
        first_prompt_return != ADDRESSES["firstPromptReturn"]
        or first_branch_return != ADDRESSES["firstPromptAcceptBranch"] + 4
        or first_target != ADDRESSES["doSaveGame"]
        or save_target != ADDRESSES["saveGame"]
        or post_prompt_return != ADDRESSES["postPromptReturn"]
        or post_branch_return != ADDRESSES["postPromptContinueBranch"] + 4
        or post_target != ADDRESSES["exitMenu"]
        or fade_target != ADDRESSES["fadeOutToBlack"]
        or witch_target != ADDRESSES["witchSuspend"]
    ):
        raise ValueError("church save ROM target/return relationship drift")

    flag_index = constants["FLAG_INDEX_BATTLE_CUTSCENE_GIZMOS"]
    flag_byte = constants["GAME_FLAGS"] + flag_index // 8
    map_domain = _external_map_domain(map_fixture_path)
    slot_bytes = constants["SAVE_SLOT_REAL_SIZE"]
    slot_span = slot_bytes * 2
    session_patches = _session_patch_plan(listing, rom)
    return {
        "system": "sf2-church-save-lifecycle-runtime-v1",
        "sourceContext": {"churchMenuEntryAddress": ADDRESSES["churchMenu"]},
        "addresses": ADDRESSES.copy(),
        "actionDispatcher": {
            "comparisons": [0, 1, 2],
            "saveAction": 3,
            "raiseAction": 0,
            "cureAction": 1,
            "promotionAction": 2,
        },
        "prompts": {
            "first": {
                "call": ADDRESSES["firstPromptCall"],
                "target": first_prompt_target,
                "return": first_prompt_return,
                "compare": ADDRESSES["firstPromptCompare"],
                "acceptResult": 0,
                "acceptBranch": ADDRESSES["firstPromptAcceptBranch"],
                "acceptTarget": first_target,
                "declineTarget": ADDRESSES["exitSave"],
            },
            "postSave": {
                "call": ADDRESSES["postPromptCall"],
                "target": post_prompt_target,
                "return": post_prompt_return,
                "compare": ADDRESSES["postPromptReturn"],
                "continueResult": 0,
                "continueBranch": ADDRESSES["postPromptContinueBranch"],
                "continueTarget": post_target,
                "suspendTarget": ADDRESSES["witchSuspend"],
            },
        },
        "mutations": {
            "currentMapToEgress": {
                "pc": ADDRESSES["currentMapToEgress"],
                "source": constants["CURRENT_MAP"],
                "target": constants["EGRESS_MAP"],
            },
            "flag399": {
                "trap": ADDRESSES["flag399Trap"],
                "operand": ADDRESSES["flag399Operand"],
                "index": flag_index,
                "owningByte": flag_byte,
                "mask": 0x80 >> (flag_index % 8),
            },
        },
        "saveGame": {
            "call": ADDRESSES["saveGameCall"],
            "target": save_target,
            "rts": ADDRESSES["saveGameRts"],
            "return": ADDRESSES["saveGameReturn"],
            "selector": {"zeroSlot": 1, "nonzeroSlot": 2},
            "actualStoredBytes": slot_bytes,
            "interleavedAddressIntervalBytes": slot_span,
            "slot1Data": constants["SAVE1_DATA"],
            "slot2Data": constants["SAVE2_DATA"],
            "slot1Checksum": constants["SAVE1_CHECKSUM"],
            "slot2Checksum": constants["SAVE2_CHECKSUM"],
            "saveFlags": constants["SAVE_FLAGS"],
        },
        "ram": {
            "currentMap": constants["CURRENT_MAP"],
            "egressMap": constants["EGRESS_MAP"],
            "currentSaveSlot": constants["CURRENT_SAVE_SLOT"],
            "gameFlags": constants["GAME_FLAGS"],
            "flag399Byte": flag_byte,
            "currentPortrait": constants["CURRENT_PORTRAIT"],
            "dialogueName": constants["DIALOGUE_NAME_INDEX_1"],
            "dialogueNumber": constants["DIALOGUE_NUMBER"],
        },
        "harness": {
            "harnessBase": 0xFF6800,
            "harnessStride": 32,
            "resultOffset": 20,
            "stackTop": 0xFFFF00,
            "actionStub": 0xFF6D00,
            "promptStub": 0xFF6D10,
            "resultStub": 0xFF6D20,
            "terminalStub": 0xFF6D40,
            "checkSram": 0x6EA6,
            "bootstrapFrameBudget": 720,
            "caseFrameBudget": 180,
            "generatedHarnessBytes": 160,
            "generatedActionBytes": 4,
            "generatedPromptBytes": 4,
            "generatedResultBytes": 6,
            "generatedTerminalBytes": 18,
        },
        "sessionPatches": session_patches,
        "suspendBoundary": {
            "fadeCall": ADDRESSES["fadeCall"],
            "fadeTarget": fade_target,
            "fadeReturn": ADDRESSES["fadeReturn"],
            "witchTailJump": ADDRESSES["witchSuspendTailJump"],
            "witchTarget": witch_target,
            "entryOnly": True,
        },
        "externalMapDomain": map_domain,
    }


def _canonical_static(static: dict[str, Any]) -> dict[str, Any]:
    return {
        key: static[key]
        for key in (
            "sourceContext",
            "addresses",
            "actionDispatcher",
            "prompts",
            "mutations",
            "saveGame",
            "ram",
            "harness",
            "suspendBoundary",
            "externalMapDomain",
            "sessionPatches",
        )
    }


def _assert_fixture(fixture: dict[str, Any], static: dict[str, Any]) -> None:
    if (
        fixture["id"] != f"sf2-{OWNER}-runtime-v1"
        or fixture["system"] != fixture["id"]
        or fixture["romSha256"] != ROM_SHA256
    ):
        raise ValueError("church save fixture identity drift")
    fixture_ids = [case["caseId"] for case in fixture["cases"]]
    if fixture["caseOrder"] != list(CASE_IDS) or fixture_ids != list(CASE_IDS):
        raise ValueError("church save case ID/order drift")
    matrix = (
        ("initial-save-prompt-decline-no-service", 0, 0, False, [-1], "exit-save"),
        ("slot1-map0-save-continue", 0, 0, False, [0, 0], "exit-menu"),
        ("slot2-map78-save-continue", 1, 78, False, [0, 0], "exit-menu"),
        ("slot1-existing-flag-save-continue", 0, 5, True, [0, 0], "exit-menu"),
        ("slot2-save-rest-suspend-boundary", 1, 1, False, [0, -1], "witch-suspend-entry"),
    )
    actual = tuple(
        (
            case["caseId"],
            case["selector"],
            case["currentMap"],
            case["flag399InitiallySet"],
            case["promptResults"],
            case["terminal"],
        )
        for case in fixture["cases"]
    )
    if actual != matrix:
        raise ValueError("church save input matrix drift")
    if fixture["sourceContext"] != static["sourceContext"]:
        raise ValueError("church save source-context golden drift")
    if fixture["acceptedObservation"] != expected_observation(fixture, static):
        raise ValueError("church save accepted observation drift")


def _expected_roles(case: dict[str, Any]) -> list[str]:
    roles = ["church-entry", "start-save", "first-prompt-call", "first-prompt-return"]
    if case["promptResults"][0] != 0:
        return roles + ["exit-save"]
    roles += [
        "do-save-game",
        "save-game-call",
        "save-game-entry",
        "save-game-rts",
        "save-game-return",
        "post-save-prompt-call",
        "post-save-prompt-return",
    ]
    if case["promptResults"][1] == 0:
        return roles + ["exit-menu"]
    return roles + [
        "fade-call",
        "fade-entry",
        "fade-return",
        "witch-tail-jump",
        "witch-suspend-entry",
    ]


def expected_observation(fixture: dict[str, Any], static: dict[str, Any]) -> dict[str, Any]:
    records = []
    for case in fixture["cases"]:
        initial_egress = 255 - case["currentMap"]
        is_saved = case["promptResults"][0] == 0
        records.append(
            {
                "caseId": case["caseId"],
                "churchEntryPc": static["addresses"]["churchMenu"],
                "startSavePc": static["addresses"]["startSave"],
                "saveGame": is_saved,
                "terminal": case["terminal"],
                "egressMapBefore": initial_egress,
                "egressMapAfter": case["currentMap"] if is_saved else initial_egress,
                "flag399Before": case["flag399InitiallySet"],
                "flag399After": True if is_saved else case["flag399InitiallySet"],
                "chronology": _expected_roles(case),
            }
        )
    return {
        "system": fixture["system"],
        "caseOrder": fixture["caseOrder"],
        "records": records,
        "callbacksCleared": True,
        "restoration": {
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
            "sessionCartPatches": True,
        },
    }


def _instrument_session_rom(rom_path: Path, static: dict[str, Any], destination: Path) -> None:
    canonical = rom_path.read_bytes()
    payload = bytearray(canonical)
    occupied: set[int] = set()
    for patch in static["sessionPatches"]:
        data = bytes.fromhex(patch["hex"])
        original = bytes.fromhex(patch["originalHex"])
        start = patch["address"]
        cells = set(range(start, start + patch["width"]))
        if patch["width"] != len(data) or len(data) != len(original) or cells & occupied:
            raise ValueError("church save session patch width/overlap drift")
        if payload[start : start + len(data)] != original:
            raise ValueError(f"church save canonical patch guard drift at 0x{start:X}")
        payload[start : start + len(data)] = data
        if payload[start : start + len(data)] != data:
            raise ValueError(f"church save session patch readback drift at 0x{start:X}")
        occupied |= cells
    destination.write_bytes(payload)
    if rom_path.read_bytes() != canonical:
        raise ValueError("church save canonical ROM mutation detected")


def _assert_session_readback(session: Path, patches: list[dict[str, Any]]) -> None:
    payload = session.read_bytes()
    for patch in patches:
        actual = payload[patch["address"] : patch["address"] + patch["width"]].hex().upper()
        if actual != patch["hex"]:
            raise ValueError(f"church save session readback drift at 0x{patch['address']:X}")


def _observer_config(fixture: dict[str, Any], static: dict[str, Any]) -> dict[str, Any]:
    return {
        "owner": OWNER,
        "caseOrder": fixture["caseOrder"],
        "cases": fixture["cases"],
        "sourceContext": fixture["sourceContext"],
        "static": static,
        "observerFailureContract": OBSERVER_FAILURE_CONTRACT,
    }


def _assert_clean_observer_config(config: dict[str, Any]) -> None:
    serialized = json.dumps(config, sort_keys=True)
    for forbidden in ("acceptedObservation", "expectedObservation", '"records"', "chronology"):
        if forbidden in serialized:
            raise ValueError("church save observer config contains output corpus")


def _expected_milestones(fixture: dict[str, Any]) -> list[str]:
    lines = [
        "milestone:observer-loaded",
        "milestone:direct-function-probe-armed",
        "milestone:direct-function-probe",
    ]
    for case in fixture["cases"]:
        lines.append(f"milestone:case-entry:{case['caseId']}")
        lines.extend(f"milestone:{role}:{case['caseId']}" for role in _expected_roles(case))
    return lines


def _assert_status(status_path: Path, fixture: dict[str, Any]) -> None:
    expected = _expected_milestones(fixture)
    assert_observer_status(
        status_path,
        owner=OWNER,
        schema_path=FAILURE_SCHEMA,
        required_milestones=tuple(expected),
    )
    lines = status_path.read_text(encoding="utf-8").splitlines()
    cursor = -1
    for milestone in expected:
        if lines.count(milestone) != 1:
            raise RuntimeError(f"church save milestone multiplicity drift: {milestone}")
        next_index = lines.index(milestone)
        if next_index <= cursor:
            raise RuntimeError("church save ordered milestone lifecycle drift")
        cursor = next_index


def _failure_diagnostic(status_path: Path, fixture: dict[str, Any]) -> dict[str, Any] | None:
    payload = callback_failure_status(status_path, owner=OWNER, schema_path=FAILURE_SCHEMA)
    if payload is None:
        return None
    if payload["caseId"] not in CASE_IDS:
        raise ValueError("church save callback failure case identity drift")
    pending = payload["pendingCallback"]
    if (
        pending["expectedCaseId"] != payload["caseId"]
        or pending["caseIndex"] != CASE_IDS.index(payload["caseId"]) + 1
        or not pending["rolesAtPc"]
        or payload["expectedEventPc"] != pending["expectedEventPc"]
        or payload["expectedCallPc"] != pending["expectedCallPc"]
        or payload["expectedTargetPc"] != pending["expectedTargetPc"]
        or payload["expectedReturnPc"] != pending["expectedReturnPc"]
        or payload["role"] not in pending["rolesAtPc"]
    ):
        raise ValueError("church save callback failure pending-state drift")
    restoration = payload["restoration"]
    cleaned = restoration["callbacksCleared"] and restoration["outputRemoved"]
    if restoration["sessionCartPatches"] or not cleaned:
        raise ValueError("church save callback cleanup/restoration claim drift")
    mismatch = payload["restorationMismatch"]
    checks = (
        "currentMap",
        "egressMap",
        "currentSaveSlot",
        "flag399",
        "slotSram",
        "checksum",
        "saveFlags",
        "dialoguePortraitScratch",
        "generatedRam",
        "bootstrapFrame",
    )
    if all(restoration[key] for key in checks):
        if mismatch is not None:
            raise ValueError("church save successful restoration must have null mismatch")
    elif mismatch is None:
        raise ValueError("church save failed restoration requires first mismatch")
    return payload


def assert_lua_role_contract() -> None:
    source = OBSERVER.read_text(encoding="utf-8")
    registered = set(re.findall(r'(?<![A-Za-z_])register\([^,]+,"([^"]+)"', source))
    dispatched = set(re.findall(r'role=="([^"]+)"', source))
    if registered != LUA_REQUIRED_CALLBACK_ROLES:
        raise ValueError("church save Lua callback registration role drift")
    if dispatched < LUA_REQUIRED_CALLBACK_ROLES:
        raise ValueError("church save Lua deterministic-dispatch role drift")
    if source.count("event.on_bus_exec(function()") != 1:
        raise ValueError("church save Lua must have one PC dispatcher")
    if 'role~="bootstrap-check-sram" and not booted then return end' not in source:
        raise ValueError("church save Lua bootstrap phase guard drift")
    pending_roles = (
        "save-game-call",
        "save-game-entry",
        "save-game-rts",
        "save-game-return",
        "fade-call",
        "fade-entry",
        "fade-return",
        "witch-tail-jump",
        "witch-suspend-entry",
    )
    for pending_role in pending_roles:
        guarded = f'role=="{pending_role}" then require_mode("{pending_role}")'
        if guarded not in source:
            raise ValueError("church save Lua pending-mode guard drift")
    shared_pc_dispatch = (
        'register(s.suspendBoundary.fadeReturn,"fade-return",0);'
        'register(s.suspendBoundary.fadeReturn,"witch-tail-jump",0)'
    )
    if shared_pc_dispatch not in source:
        raise ValueError("church save Lua shared-PC dispatch order drift")
    if "client.exitCode(config.observerFailureContract.exitCode)" not in source:
        raise ValueError("church save Lua callback exit contract drift")
    if "emu.setregister" in source:
        raise ValueError("church save Lua terminal restoration must use generated 68K frame")


def preflight_church_save_lifecycle(rom_path: Path) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=OWNER)
    static = build_static_contract(rom_path)
    _assert_fixture(fixture, static)
    assert_lua_role_contract()
    with tempfile.TemporaryDirectory(prefix="sf2-church-save-") as temporary:
        session = Path(temporary) / "church-save-session.bin"
        _instrument_session_rom(rom_path, static, session)
        _assert_session_readback(session, static["sessionPatches"])
    return {"Fixture": fixture["system"], "Cases": len(CASE_IDS), "Status": "PRELAUNCH-PASS"}


def _verify_church_save_lifecycle(
    rom_path: Path, upstream_path: Path = UPSTREAM, *, timeout_seconds: int = 180
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=OWNER)
    static = build_static_contract(rom_path, upstream_path)
    _assert_fixture(fixture, static)
    assert_lua_role_contract()
    expected = expected_observation(fixture, static)
    validate_json(expected, OBSERVATION_SCHEMA, owner=f"{OWNER} expected observation")
    config = _observer_config(fixture, static)
    _assert_clean_observer_config(config)
    status_path = repo_path(f"local/derived/h3/{OWNER}.status.txt")
    session_deleted = False
    try:
        with tempfile.TemporaryDirectory(prefix="sf2-church-save-") as temporary:
            session = Path(temporary) / "church-save-session.bin"
            _instrument_session_rom(rom_path, static, session)
            _assert_session_readback(session, static["sessionPatches"])
            try:
                observed = _with_instrumented_rom_database(
                    session,
                    OWNER,
                    lambda: run_observer(
                        observer_path=OBSERVER,
                        rom_path=session,
                        config=config,
                        output_name=OWNER,
                        timeout_seconds=timeout_seconds,
                    ),
                )
            except Exception:
                _failure_diagnostic(status_path, fixture)
                raise
            _assert_status(status_path, fixture)
        session_deleted = not session.exists()
        observed["restoration"]["sessionCartPatches"] = session_deleted
        validate_json(observed, OBSERVATION_SCHEMA, owner=f"{OWNER} observation")
        if observed != expected:
            raise ValueError(
                f"church save runtime observation mismatch: expected={expected}; actual={observed}"
            )
        OBSERVED_OUTPUT.write_text(json.dumps(observed, indent=2) + "\n", encoding="utf-8")
    except Exception:
        OBSERVED_OUTPUT.unlink(missing_ok=True)
        raise
    return {
        "Fixture": fixture["system"],
        "Cases": len(CASE_IDS),
        "Status": "PASS",
        "SessionRomDeleted": session_deleted,
    }


def verify_church_save_lifecycle(
    rom_path: Path, upstream_path: Path = UPSTREAM, *, timeout_seconds: int = 180
) -> dict[str, Any]:
    """Run the rail and delete a stale accepted observation on every rejection."""
    try:
        return _verify_church_save_lifecycle(
            rom_path, upstream_path, timeout_seconds=timeout_seconds
        )
    except Exception:
        OBSERVED_OUTPUT.unlink(missing_ok=True)
        raise
