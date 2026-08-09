from __future__ import annotations

import hashlib
import json
import re
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_ai import _parse_source_file
from sf2tool.h2.battle_scene_animations import _listing_address
from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.source_text import read_upstream_text

ID = "sf2-special-screens-static-v1"
SOURCE_ROOT = Path("code/specialscreens")
EXPECTED_GROUP_COUNTS = {
    "endkiss": 2,
    "jewelend": 1,
    "segalogo": 2,
    "suspend": 3,
    "title": 3,
    "witch": 5,
    "witchend": 3,
}
WITCH_START_PATH = Path("code/specialscreens/witch/witchstart.asm")
WITCH_MAIN_MENU_PATH = Path("code/specialscreens/witch/witchmainmenu.asm")
WITCH_FUNCTIONS_PATH = Path("code/specialscreens/witch/witchfunctions.asm")
WITCH_ACTION_LABELS = (
    "witchMenuAction_New",
    "witchMenuAction_Load",
    "witchMenuAction_Del",
    "witchMenuAction_Copy",
)
WITCH_SRAM_SERVICE_TARGETS = (
    "CheckSram",
    "SaveGame",
    "LoadGame",
    "CopySave",
    "ClearSaveSlotFlag",
)
MANIFEST = repo_path("manifests/extractions/special-screens-static.json")
SCHEMA = repo_path("schemas/special-screens-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/special-screens-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-special-screens-static-fixture.schema.json")
RESEARCH_INDEX = repo_path("manifests/research-index.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")
TOOLCHAIN = repo_path("manifests/toolchain.json")


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _require_fragments(source: str, fragments: tuple[str, ...], owner: str) -> None:
    missing = [fragment for fragment in fragments if fragment not in source]
    if missing:
        raise ValueError(f"{owner} source-shape drift: missing {missing}")


def _strip_comment(line: str) -> str:
    return line.split(";", 1)[0].rstrip()


def _source_lines(source: str) -> list[tuple[int, str]]:
    return [
        (line_number, _strip_comment(line))
        for line_number, line in enumerate(source.splitlines(), start=1)
    ]


def _section(source: str, start_label: str, end_marker: str) -> str:
    start = re.search(rf"^{re.escape(start_label)}:\s*$", source, re.MULTILINE)
    if start is None:
        raise ValueError(f"witch section is missing: {start_label}")
    end = source.find(end_marker, start.end())
    if end < 0:
        raise ValueError(f"witch section end is missing: {end_marker}")
    return source[start.start() : end]


def _direct_instruction_target(operand: str) -> str | None:
    target = operand.strip().split(",", 1)[0]
    target = re.sub(r"\.[bwl]$", "", target, flags=re.IGNORECASE)
    if target.startswith("(") and target.endswith(")"):
        target = target[1:-1]
    target = re.sub(r"\(pc\)$", "", target, flags=re.IGNORECASE)
    if re.fullmatch(r"[A-Za-z_][A-Za-z0-9_]*", target):
        return target
    return None


def _direct_call_sites(source: str) -> list[dict[str, Any]]:
    call_pattern = re.compile(r"^\s*(?:bsr|jsr)(?:\.[bswl])?\s+([^\s;]+)", re.IGNORECASE)
    label_pattern = re.compile(r"^\s*(?:[A-Za-z_][A-Za-z0-9_]*|@[A-Za-z0-9_]+):\s*")
    sites: list[dict[str, Any]] = []
    for line_number, line in _source_lines(source):
        match = call_pattern.match(label_pattern.sub("", line))
        if match is None:
            continue
        target = _direct_instruction_target(match.group(1))
        if target is not None:
            sites.append({"line": line_number, "instructionTarget": target})
    return sites


def _listing_alias_target(listing: str, instruction_target: str) -> str:
    if not instruction_target.startswith("j_"):
        return instruction_target
    match = re.search(
        rf"^[0-9A-F]{{8}}.*\b{re.escape(instruction_target)}:\s*$"
        rf"(?:\n.*){{0,3}}?\n^[0-9A-F]{{8}}.*\bjmp\s+"
        rf"([A-Za-z_][A-Za-z0-9_]*)\(pc\)",
        listing,
        re.MULTILINE,
    )
    if match is None:
        raise ValueError(
            f"witch jump-interface alias is absent from H1 listing: {instruction_target}"
        )
    return match.group(1)


def _call_identity(listing: str, instruction_target: str) -> dict[str, str]:
    return {
        "instructionTarget": instruction_target,
        "effectiveTarget": _listing_alias_target(listing, instruction_target),
    }


def _integer_token(token: str, constants: dict[str, int]) -> int:
    token = token.strip()
    if token.startswith("#"):
        token = token[1:]
    if token in constants:
        return constants[token]
    if token.startswith("$"):
        return int(token[1:], 16)
    if token.startswith("%"):
        return int(token[1:], 2)
    try:
        return int(token, 10)
    except ValueError as error:
        raise ValueError(f"witch integer token is not a parsed constant: {token}") from error


def _read_equates(path: Path, names: tuple[str, ...]) -> dict[str, int]:
    source = read_upstream_text(path)
    values: dict[str, int] = {}
    for name in names:
        match = re.search(
            rf"^{re.escape(name)}:\s+equ\s+(\$[0-9A-Fa-f]+|-?\d+)",
            source,
            re.MULTILINE,
        )
        if match is None:
            raise ValueError(f"witch constant is absent: {name}")
        values[name] = _integer_token(match.group(1), {})
    return values


def _require_ordered_fragments(section: str, fragments: tuple[str, ...], owner: str) -> None:
    position = 0
    for fragment in fragments:
        next_position = section.find(fragment, position)
        if next_position < 0:
            raise ValueError(f"{owner} source order drift: {fragment}")
        position = next_position + len(fragment)


def _immediate_register_values(
    section: str, mnemonic: str, register: str, constants: dict[str, int]
) -> list[int]:
    pattern = re.compile(
        rf"^\s*{re.escape(mnemonic)}\s+#([^,\s]+),{re.escape(register)}\s*$",
        re.MULTILINE,
    )
    return [_integer_token(token, constants) for token in pattern.findall(section)]


def _named_section(source: str, name: str) -> str:
    return _section(source, name, f"; End of function {name}")


def _call_order(
    section: str, listing: str, expected_instruction_targets: tuple[str, ...], owner: str
) -> list[dict[str, str]]:
    relevant_targets = set(expected_instruction_targets)
    actual_targets = [
        site["instructionTarget"]
        for site in _direct_call_sites(section)
        if site["instructionTarget"] in relevant_targets
    ]
    if actual_targets != list(expected_instruction_targets):
        raise ValueError(f"{owner} call order drift")
    return [_call_identity(listing, target) for target in actual_targets]


def _selector_transform(section: str, *, inverted: bool, owner: str) -> dict[str, int]:
    and_values = _immediate_register_values(section, "andi.w", "d2", {})
    shift_values = _immediate_register_values(section, "lsl.w", "d2", {})
    xor_values = _immediate_register_values(section, "eori.w", "d2", {})
    if len(and_values) != 1 or len(shift_values) != 1:
        raise ValueError(f"{owner} selector transform drift")
    if inverted:
        if xor_values != and_values:
            raise ValueError(f"{owner} selector inversion drift")
    elif xor_values:
        raise ValueError(f"{owner} selector unexpectedly inverts")
    return {
        "saveFlagsMask": and_values[0],
        "leftShiftCount": shift_values[0],
        "invertsSaveFlags": inverted,
    }


def _action_selection(section: str, *, inverted: bool, owner: str) -> dict[str, Any]:
    transform = _selector_transform(section, inverted=inverted, owner=owner)
    branch_match = re.search(
        r"btst\s+#([^,\s]+),d2\s*\n\s*beq\.s\s+(@[A-Za-z0-9_]+)"
        r".*?moveq\s+#([^,\s]+),d0\s*\n\s*bra\.s\s+(@[A-Za-z0-9_]+)"
        r".*?\2:\s*\n\s*moveq\s+#([^,\s]+),d0",
        section,
        re.DOTALL,
    )
    if branch_match is None:
        raise ValueError(f"{owner} selector branch polarity drift")
    bit, clear_target, set_value, _, clear_value = branch_match.groups()
    page_values = _immediate_register_values(section, "moveq", "d1", {})
    if not page_values:
        raise ValueError(f"{owner} menu page drift")
    return_subtract_values = _immediate_register_values(section, "subq.w", "d0", {})
    if len(return_subtract_values) != 1:
        raise ValueError(f"{owner} selector return adjustment drift")
    _require_ordered_fragments(
        section,
        (
            "subq.w",
            "move.w  d0,((CURRENT_SAVE_SLOT-$1000000)).w",
        ),
        owner,
    )
    return {
        "pageIndex": page_values[0],
        "availabilityTransform": transform,
        "branchBit": _integer_token(bit, {}),
        "clearBranchMnemonic": "beq.s",
        "clearBranchTarget": clear_target,
        "initialSelectionWhenBitSet": _integer_token(set_value, {}),
        "initialSelectionWhenBitClear": _integer_token(clear_value, {}),
        "selectionReturnSubtract": return_subtract_values[0],
        "writesCurrentSaveSlot": True,
    }


def _witch_entry_flow(start_source: str, listing: str) -> dict[str, Any]:
    before_dispatch = start_source[: start_source.index("rjt_WitchMenuActions:")]
    check_result_matches = re.findall(
        r"tst\.w\s+(d[01])\s*\n\s*(bpl\.[sbwl])\s+(@[A-Za-z0-9_]+)",
        before_dispatch,
    )
    if check_result_matches != [
        ("d0", "bpl.s", "@IsSaveSlot2Corrupted"),
        ("d1", "bpl.s", "@StartWitchDialogue"),
    ]:
        raise ValueError("witch CheckSram result branch polarity/order drift")
    page_section_match = re.search(
        r"@DisplayText:\s*(.*?)\n\s*add\.w\s+d0,d0\s*\n"
        r"\s*move\.w\s+rjt_WitchMenuActions\(pc,d0\.w\),d0\s*\n"
        r"\s*jmp\s+rjt_WitchMenuActions\(pc,d0\.w\)",
        before_dispatch,
        re.DOTALL,
    )
    if page_section_match is None:
        raise ValueError("witch main action-page dispatch flow drift")
    page_section = page_section_match.group(1)
    mask_values = _immediate_register_values(page_section, "andi.w", "d3", {})
    if len(mask_values) != 1:
        raise ValueError("witch action-page save-flags mask drift")
    availability_match = re.search(
        r"bne\.s\s+@loc_5[^\n]*\n\s*clr\.w\s+d0[^\n]*\n\s*moveq\s+#([^,\s]+),d2"
        r".*?moveq\s+#([^,\s]+),d0[^\n]*\n\s*cmpi\.w\s+#([^,\s]+),d3"
        r".*?moveq\s+#([^,\s]+),d2[^\n]*\n.*?move\.w\s+#([^,\s]+),d2",
        page_section,
        re.DOTALL,
    )
    if availability_match is None:
        raise ValueError("witch action-page availability order drift")
    zero_mask, _, all_set_value, all_set_mask, other_mask = availability_match.groups()
    if _integer_token(all_set_value, {}) != mask_values[0]:
        raise ValueError("witch action-page all-set comparison no longer matches mask")
    scale_match = re.search(r"\n\s*add\.(b|w|l)\s+(d0),\2\s*\n", before_dispatch)
    if scale_match is None:
        raise ValueError("witch action-page dispatch scale drift")
    width_match = re.search(r"^\s*dc\.(b|w|l)\s+witchMenuAction_", start_source, re.MULTILINE)
    if width_match is None:
        raise ValueError("witch action-page dispatch record width drift")
    byte_widths = {"b": 1, "w": 2, "l": 4}
    page_index_match = re.search(r"\n\s*clr\.w\s+d1\s*\n", page_section)
    if page_index_match is None:
        raise ValueError("witch action-page index initialization drift")
    dispatch_table_match = re.search(
        r"move\.w\s+([A-Za-z_][A-Za-z0-9_]*)\(pc,d0\.w\),d0\s*\n"
        r"\s*jmp\s+\1\(pc,d0\.w\)",
        before_dispatch,
    )
    if dispatch_table_match is None:
        raise ValueError("witch action-page dispatch-table identity drift")
    return {
        "checkSram": {
            **_call_identity(listing, "CheckSram"),
            "effectiveTargetAddress": _listing_address(listing, "CheckSram"),
            "resultChecks": [
                {
                    "resultRegister": register,
                    "continueBranchMnemonic": mnemonic,
                    "continueBranchTarget": target,
                }
                for register, mnemonic, target in check_result_matches
            ],
        },
        "actionPage": {
            "pageIndex": 0 if page_index_match is not None else None,
            "saveFlagsMask": mask_values[0],
            "availabilityMasks": [
                _integer_token(zero_mask, {}),
                _integer_token(all_set_mask, {}),
                _integer_token(other_mask, {}),
            ],
            "availabilityCaseOrder": ["zero", "allSet", "otherNonzero"],
            "cancelBranchMnemonic": "bmi.s",
            "dispatchIndexScale": byte_widths[scale_match.group(1)],
            "dispatchRecordWidthBytes": byte_widths[width_match.group(1)],
            "dispatchTable": dispatch_table_match.group(1),
        },
    }


def _witch_main_menu_facts(
    main_source: str, listing: str, constants: dict[str, int]
) -> dict[str, Any]:
    execute = _named_section(main_source, "ExecuteWitchMainMenu")
    navigation = _named_section(main_source, "sub_1678A")
    drawing = _named_section(main_source, "sub_1679E")
    input_mask = _immediate_register_values(execute, "andi.w", "d0", constants)
    if input_mask != [constants["BYTE_LOWER_NIBBLE_MASK"]]:
        raise ValueError("witch main-menu initial selection mask drift")
    cancel_match = re.search(
        r"btst\s+#(INPUT_BIT_B),\(\(CURRENT_PLAYER_INPUT-\$1000000\)\)\.w\s*\n"
        r"\s*(bne\.[sbwl])\s+loc_16756\s*\n.*?loc_16756:\s*\n\s*move\.w\s+#(-?\d+),d0",
        execute,
        re.DOTALL,
    )
    if cancel_match is None:
        raise ValueError("witch main-menu cancel result drift")
    wrap_values = _immediate_register_values(navigation, "andi.w", "d0", {})
    bit_positions = [
        _integer_token(value, {}) for value in re.findall(r"btst\s+#([^,\s]+),d6", drawing)
    ]
    if bit_positions != list(range(len(bit_positions))):
        raise ValueError("witch main-menu option bit positions drift")
    if wrap_values != [bit_positions[-1]]:
        raise ValueError("witch main-menu navigation mask no longer matches option range")
    page_labels = (
        "@Page0_FileOptions",
        "@Page1_NewFileNames",
        "@Page2_LoadedFileNames",
        "@Page3_Difficulties",
    )
    if any(f"{label}:" not in main_source for label in page_labels):
        raise ValueError("witch main-menu page label drift")
    page_kinds = ("actions", "newSlotNames", "loadedSlotNames", "difficulties")
    return {
        "functionAddress": _listing_address(listing, "ExecuteWitchMainMenu"),
        "initialSelectionMask": input_mask[0],
        "cancel": {
            "inputSymbol": cancel_match.group(1),
            "branchMnemonic": cancel_match.group(2),
            "returnValue": _integer_token(cancel_match.group(3), {}),
        },
        "navigation": {
            "availableBitPositions": bit_positions,
            "wrapMask": wrap_values[0],
        },
        "pages": [
            {
                "pageIndex": _integer_token(
                    re.fullmatch(r"@Page(\d+)_[A-Za-z]+", label).group(1), {}
                ),
                "sourceLabel": label,
                "pageKind": page_kind,
            }
            for label, page_kind in zip(page_labels, page_kinds, strict=True)
        ],
    }


def _constant_reference(name: str, constants: dict[str, int]) -> dict[str, Any]:
    return {"name": name, "value": constants[name]}


def _provenance_use_site(
    source: str,
    source_path: Path,
    use_site_id: str,
    source_line: int,
    expected_instruction: str,
) -> dict[str, Any]:
    lines = source.splitlines()
    if not 1 <= source_line <= len(lines):
        raise ValueError(f"witch provenance use-site drift: {use_site_id}")
    instruction = re.sub(r"\s+", " ", _strip_comment(lines[source_line - 1]).strip())
    if instruction != expected_instruction:
        raise ValueError(f"witch provenance use-site drift: {use_site_id}")
    parts = instruction.split(maxsplit=1)
    return {
        "id": use_site_id,
        "sourcePath": source_path.as_posix(),
        "sourceLine": source_line,
        "instruction": instruction,
        "opcode": parts[0],
        "operand": parts[1] if len(parts) == 2 else "",
    }


def _witch_save_menu_provenance(sources: dict[str, str]) -> dict[str, Any]:
    start = sources[WITCH_START_PATH.as_posix()]
    main = sources[WITCH_MAIN_MENU_PATH.as_posix()]
    specifications = (
        ("entry.checkSram.call", WITCH_START_PATH, 45, "bsr.w CheckSram"),
        ("entry.checkSram.d0Test", WITCH_START_PATH, 55, "tst.w d0"),
        ("entry.checkSram.d0Branch", WITCH_START_PATH, 56, "bpl.s @IsSaveSlot2Corrupted"),
        ("entry.checkSram.d1Test", WITCH_START_PATH, 64, "tst.w d1"),
        ("entry.checkSram.d1Branch", WITCH_START_PATH, 65, "bpl.s @StartWitchDialogue"),
        ("entry.actionPage.saveFlags", WITCH_START_PATH, 104, "move.b (SAVE_FLAGS).l,d3"),
        ("entry.actionPage.mask", WITCH_START_PATH, 105, "andi.w #3,d3"),
        ("entry.actionPage.nonzeroBranch", WITCH_START_PATH, 106, "bne.s @loc_5"),
        ("entry.actionPage.zeroPage", WITCH_START_PATH, 108, "clr.w d0"),
        ("entry.actionPage.zeroMask", WITCH_START_PATH, 109, "moveq #1,d2"),
        ("entry.actionPage.zeroToMenu", WITCH_START_PATH, 110, "bra.s @WitchMenu"),
        ("entry.actionPage.occupiedPage", WITCH_START_PATH, 113, "moveq #1,d0"),
        ("entry.actionPage.allSetCompare", WITCH_START_PATH, 114, "cmpi.w #3,d3"),
        ("entry.actionPage.otherBranch", WITCH_START_PATH, 115, "bne.s @loc_6"),
        ("entry.actionPage.allSetMask", WITCH_START_PATH, 117, "moveq #%110,d2"),
        ("entry.actionPage.allSetToMenu", WITCH_START_PATH, 118, "bra.s @WitchMenu"),
        ("entry.actionPage.otherMask", WITCH_START_PATH, 121, "move.w #%1111,d2"),
        ("entry.actionPage.pageIndex", WITCH_START_PATH, 124, "clr.w d1"),
        ("entry.actionPage.menuCall", WITCH_START_PATH, 125, "jsr j_ExecuteWitchMainMenu"),
        ("entry.actionPage.cancelTest", WITCH_START_PATH, 126, "tst.w d0"),
        ("entry.actionPage.cancelBranch", WITCH_START_PATH, 127, "bmi.s byte_73C2"),
        ("entry.actionPage.scale", WITCH_START_PATH, 129, "add.w d0,d0"),
        (
            "entry.actionPage.tableLoad",
            WITCH_START_PATH,
            130,
            "move.w rjt_WitchMenuActions(pc,d0.w),d0",
        ),
        ("entry.actionPage.tableJump", WITCH_START_PATH, 131, "jmp rjt_WitchMenuActions(pc,d0.w)"),
        ("dispatcher.new", WITCH_START_PATH, 136, "dc.w witchMenuAction_New-rjt_WitchMenuActions"),
        (
            "dispatcher.load",
            WITCH_START_PATH,
            137,
            "dc.w witchMenuAction_Load-rjt_WitchMenuActions",
        ),
        (
            "dispatcher.delete",
            WITCH_START_PATH,
            138,
            "dc.w witchMenuAction_Del-rjt_WitchMenuActions",
        ),
        (
            "dispatcher.copy",
            WITCH_START_PATH,
            139,
            "dc.w witchMenuAction_Copy-rjt_WitchMenuActions",
        ),
        ("main.function", WITCH_MAIN_MENU_PATH, 19, "ExecuteWitchMainMenu:"),
        ("main.initialMask", WITCH_MAIN_MENU_PATH, 23, "andi.w #BYTE_LOWER_NIBBLE_MASK,d0"),
        (
            "main.cancelTest",
            WITCH_MAIN_MENU_PATH,
            77,
            "btst #INPUT_BIT_B,((CURRENT_PLAYER_INPUT-$1000000)).w",
        ),
        ("main.cancelBranch", WITCH_MAIN_MENU_PATH, 78, "bne.w loc_16756"),
        ("main.cancelValue", WITCH_MAIN_MENU_PATH, 96, "move.w #-1,d0"),
        ("main.navigationAdd", WITCH_MAIN_MENU_PATH, 127, "add.w d3,d0"),
        ("main.navigationMask", WITCH_MAIN_MENU_PATH, 128, "andi.w #3,d0"),
        ("main.navigationAvailability", WITCH_MAIN_MENU_PATH, 130, "btst d0,d1"),
        ("main.optionBit0", WITCH_MAIN_MENU_PATH, 154, "btst #0,d6"),
        ("main.optionBit1", WITCH_MAIN_MENU_PATH, 162, "btst #1,d6"),
        ("main.optionBit2", WITCH_MAIN_MENU_PATH, 170, "btst #2,d6"),
        ("main.optionBit3", WITCH_MAIN_MENU_PATH, 178, "btst #3,d6"),
        ("main.page0", WITCH_MAIN_MENU_PATH, 242, "@Page0_FileOptions:"),
        ("main.page1", WITCH_MAIN_MENU_PATH, 270, "@Page1_NewFileNames:"),
        ("main.page2", WITCH_MAIN_MENU_PATH, 292, "@Page2_LoadedFileNames:"),
        ("main.page3", WITCH_MAIN_MENU_PATH, 330, "@Page3_Difficulties:"),
        ("new.selection.saveFlags", WITCH_START_PATH, 148, "move.b (SAVE_FLAGS).l,d2"),
        ("new.selection.mask", WITCH_START_PATH, 149, "andi.w #3,d2"),
        ("new.selection.inversion", WITCH_START_PATH, 150, "eori.w #3,d2"),
        ("new.selection.shift", WITCH_START_PATH, 151, "lsl.w #1,d2"),
        ("new.selection.bit", WITCH_START_PATH, 152, "btst #1,d2"),
        ("new.selection.clearBranch", WITCH_START_PATH, 153, "beq.s @loc_8"),
        ("new.selection.setChoice", WITCH_START_PATH, 155, "moveq #1,d0"),
        ("new.selection.clearChoice", WITCH_START_PATH, 159, "moveq #2,d0"),
        ("new.selection.page", WITCH_START_PATH, 162, "moveq #1,d1"),
        ("new.selection.returnSubtract", WITCH_START_PATH, 167, "subq.w #1,d0"),
        (
            "new.selection.currentSlot",
            WITCH_START_PATH,
            168,
            "move.w d0,((CURRENT_SAVE_SLOT-$1000000)).w",
        ),
        ("new.call.menu", WITCH_START_PATH, 163, "jsr j_ExecuteWitchMainMenu"),
        ("new.call.newGame", WITCH_START_PATH, 169, "jsr j_NewGame"),
        ("new.call.nameFirst", WITCH_START_PATH, 172, "jsr j_NameAlly"),
        ("new.call.nameLoop", WITCH_START_PATH, 182, "jsr j_NameAlly"),
        ("new.call.configuration", WITCH_START_PATH, 193, "bsr.w CheatModeConfiguration"),
        ("new.call.configurationMenu", WITCH_START_PATH, 198, "jsr j_ExecuteWitchMainMenu"),
        ("new.call.configurationText", WITCH_START_PATH, 215, "bsr.w DisplayText"),
        ("new.call.saveGame", WITCH_START_PATH, 220, "bsr.w SaveGame"),
        ("new.configuration.page", WITCH_START_PATH, 196, "moveq #3,d1"),
        ("new.configuration.mask", WITCH_START_PATH, 197, "moveq #%1111,d2"),
        ("new.configuration.bit0", WITCH_START_PATH, 204, "btst #0,d0"),
        ("new.configuration.flag78", WITCH_START_PATH, 206, "setFlg 78"),
        ("new.configuration.bit1", WITCH_START_PATH, 209, "btst #1,d0"),
        ("new.configuration.flag79", WITCH_START_PATH, 211, "setFlg 79"),
        (
            "new.handoff.currentMap",
            WITCH_START_PATH,
            218,
            "move.b #GAMESTART_MAP,((CURRENT_MAP-$1000000)).w",
        ),
        (
            "new.handoff.egressMap",
            WITCH_START_PATH,
            219,
            "move.b #GAMESTART_MAP,((EGRESS_MAP-$1000000)).w",
        ),
        ("new.handoff.map", WITCH_START_PATH, 224, "move.b #GAMESTART_MAP,d0"),
        ("new.handoff.savepointX", WITCH_START_PATH, 225, "move.w #GAMESTART_SAVEPOINT_X,d1"),
        ("new.handoff.savepointY", WITCH_START_PATH, 226, "move.w #GAMESTART_SAVEPOINT_Y,d2"),
        ("new.handoff.facing", WITCH_START_PATH, 227, "move.w #GAMESTART_FACING,d3"),
        ("new.handoff.d4", WITCH_START_PATH, 228, "moveq #1,d4"),
        ("new.handoff.mainLoop", WITCH_START_PATH, 229, "bra.w MainLoop"),
        ("load.selection.saveFlags", WITCH_START_PATH, 241, "move.b (SAVE_FLAGS).l,d2"),
        ("load.selection.mask", WITCH_START_PATH, 242, "andi.w #3,d2"),
        ("load.selection.shift", WITCH_START_PATH, 243, "lsl.w #1,d2"),
        ("load.selection.bit", WITCH_START_PATH, 244, "btst #1,d2"),
        ("load.selection.clearBranch", WITCH_START_PATH, 245, "beq.s @loc_16"),
        ("load.selection.setChoice", WITCH_START_PATH, 246, "moveq #1,d0"),
        ("load.selection.clearChoice", WITCH_START_PATH, 250, "moveq #2,d0"),
        ("load.selection.page", WITCH_START_PATH, 253, "moveq #2,d1"),
        ("load.selection.returnSubtract", WITCH_START_PATH, 257, "subq.w #1,d0"),
        (
            "load.selection.currentSlot",
            WITCH_START_PATH,
            258,
            "move.w d0,((CURRENT_SAVE_SLOT-$1000000)).w",
        ),
        ("load.call.menu", WITCH_START_PATH, 254, "jsr j_ExecuteWitchMainMenu"),
        ("load.call.loadGame", WITCH_START_PATH, 259, "bsr.w LoadGame"),
        ("load.call.configuration", WITCH_START_PATH, 261, "bsr.w CheatModeConfiguration"),
        ("load.call.battleLoop", WITCH_START_PATH, 267, "jsr j_BattleLoop"),
        ("load.call.savepoint", WITCH_START_PATH, 273, "jsr GetSavepointForMap(pc)"),
        ("load.handoff.flag", WITCH_START_PATH, 265, "chkFlg 88"),
        ("load.handoff.zeroBranch", WITCH_START_PATH, 266, "beq.s @loc_18"),
        ("load.handoff.battleBranch", WITCH_START_PATH, 268, "bra.w alt_MainLoopEntry"),
        ("load.handoff.savepointBranch", WITCH_START_PATH, 276, "bra.w alt_MainLoopEntry"),
        ("copy.confirmation.prompt", WITCH_START_PATH, 288, "jsr j_alt_YesNoPrompt"),
        ("copy.confirmation.result", WITCH_START_PATH, 289, "tst.w d0"),
        ("copy.confirmation.nonzeroBranch", WITCH_START_PATH, 290, "bne.w byte_73C2"),
        ("copy.selector.saveFlags", WITCH_START_PATH, 291, "move.b (SAVE_FLAGS).l,d0"),
        ("copy.selector.mask", WITCH_START_PATH, 292, "andi.w #3,d0"),
        ("copy.selector.subtract", WITCH_START_PATH, 293, "subq.w #1,d0"),
        ("copy.call.service", WITCH_START_PATH, 294, "bsr.w CopySave"),
        ("delete.selection.saveFlags", WITCH_START_PATH, 308, "move.b (SAVE_FLAGS).l,d2"),
        ("delete.selection.mask", WITCH_START_PATH, 309, "andi.w #3,d2"),
        ("delete.selection.shift", WITCH_START_PATH, 310, "lsl.w #1,d2"),
        ("delete.selection.bit", WITCH_START_PATH, 311, "btst #1,d2"),
        ("delete.selection.clearBranch", WITCH_START_PATH, 312, "beq.s @loc_19"),
        ("delete.selection.setChoice", WITCH_START_PATH, 313, "moveq #1,d0"),
        ("delete.selection.clearChoice", WITCH_START_PATH, 317, "moveq #2,d0"),
        ("delete.selection.page", WITCH_START_PATH, 320, "moveq #2,d1"),
        ("delete.selection.returnSubtract", WITCH_START_PATH, 324, "subq.w #1,d0"),
        (
            "delete.selection.currentSlot",
            WITCH_START_PATH,
            325,
            "move.w d0,((CURRENT_SAVE_SLOT-$1000000)).w",
        ),
        ("delete.call.menu", WITCH_START_PATH, 321, "jsr j_ExecuteWitchMainMenu"),
        ("delete.confirmation.prompt", WITCH_START_PATH, 327, "jsr j_alt_YesNoPrompt"),
        ("delete.confirmation.result", WITCH_START_PATH, 328, "tst.w d0"),
        ("delete.confirmation.nonzeroBranch", WITCH_START_PATH, 329, "bne.w byte_73C2"),
        ("delete.call.service", WITCH_START_PATH, 331, "bsr.w ClearSaveSlotFlag"),
    )
    source_by_path = {WITCH_START_PATH: start, WITCH_MAIN_MENU_PATH: main}
    use_sites = [
        _provenance_use_site(
            source_by_path[path], path, use_site_id, source_line, expected_instruction
        )
        for use_site_id, path, source_line, expected_instruction in specifications
    ]
    if len(use_sites) != len({site["id"] for site in use_sites}):
        raise ValueError("witch provenance use-site id duplication")
    use_site_ids = {site["id"] for site in use_sites}
    summary_provenance = {
        "entry.checkSram": [
            "entry.checkSram.call",
            "entry.checkSram.d0Test",
            "entry.checkSram.d0Branch",
            "entry.checkSram.d1Test",
            "entry.checkSram.d1Branch",
        ],
        "entry.actionPage": [
            "entry.actionPage.saveFlags",
            "entry.actionPage.mask",
            "entry.actionPage.nonzeroBranch",
            "entry.actionPage.zeroPage",
            "entry.actionPage.zeroMask",
            "entry.actionPage.zeroToMenu",
            "entry.actionPage.occupiedPage",
            "entry.actionPage.allSetCompare",
            "entry.actionPage.otherBranch",
            "entry.actionPage.allSetMask",
            "entry.actionPage.allSetToMenu",
            "entry.actionPage.otherMask",
            "entry.actionPage.pageIndex",
            "entry.actionPage.menuCall",
            "entry.actionPage.cancelTest",
            "entry.actionPage.cancelBranch",
            "entry.actionPage.scale",
            "entry.actionPage.tableLoad",
            "entry.actionPage.tableJump",
            "dispatcher.new",
        ],
        "dispatcher": [
            "dispatcher.new",
            "dispatcher.load",
            "dispatcher.delete",
            "dispatcher.copy",
        ],
        "mainMenu": [
            "main.function",
            "main.initialMask",
            "main.cancelTest",
            "main.cancelBranch",
            "main.cancelValue",
            "main.navigationAdd",
            "main.navigationMask",
            "main.navigationAvailability",
            "main.optionBit0",
            "main.optionBit1",
            "main.optionBit2",
            "main.optionBit3",
            "main.page0",
            "main.page1",
            "main.page2",
            "main.page3",
        ],
        "actions.New.selection": [
            "new.selection.saveFlags",
            "new.selection.mask",
            "new.selection.inversion",
            "new.selection.shift",
            "new.selection.bit",
            "new.selection.clearBranch",
            "new.selection.setChoice",
            "new.selection.clearChoice",
            "new.selection.page",
            "new.selection.returnSubtract",
            "new.selection.currentSlot",
        ],
        "actions.New.callOrder": [
            "new.call.menu",
            "new.call.newGame",
            "new.call.nameFirst",
            "new.call.nameLoop",
            "new.call.configuration",
            "new.call.configurationMenu",
            "new.call.configurationText",
            "new.call.saveGame",
        ],
        "actions.New.configuration": [
            "new.configuration.page",
            "new.configuration.mask",
            "new.configuration.bit0",
            "new.configuration.flag78",
            "new.configuration.bit1",
            "new.configuration.flag79",
        ],
        "actions.New.initialMainLoopHandoff": [
            "new.handoff.currentMap",
            "new.handoff.egressMap",
            "new.call.saveGame",
            "new.handoff.map",
            "new.handoff.savepointX",
            "new.handoff.savepointY",
            "new.handoff.facing",
            "new.handoff.d4",
            "new.handoff.mainLoop",
        ],
        "actions.Load.selection": [
            "load.selection.saveFlags",
            "load.selection.mask",
            "load.selection.shift",
            "load.selection.bit",
            "load.selection.clearBranch",
            "load.selection.setChoice",
            "load.selection.clearChoice",
            "load.selection.page",
            "load.selection.returnSubtract",
            "load.selection.currentSlot",
        ],
        "actions.Load.callOrder": [
            "load.call.menu",
            "load.call.loadGame",
            "load.call.configuration",
            "load.call.battleLoop",
            "load.call.savepoint",
        ],
        "actions.Load.postLoadHandoff": [
            "load.handoff.flag",
            "load.handoff.zeroBranch",
            "load.call.battleLoop",
            "load.handoff.battleBranch",
            "load.call.savepoint",
            "load.handoff.savepointBranch",
        ],
        "actions.Del.selection": [
            "delete.selection.saveFlags",
            "delete.selection.mask",
            "delete.selection.shift",
            "delete.selection.bit",
            "delete.selection.clearBranch",
            "delete.selection.setChoice",
            "delete.selection.clearChoice",
            "delete.selection.page",
            "delete.selection.returnSubtract",
            "delete.selection.currentSlot",
        ],
        "actions.Del.callOrder": [
            "delete.call.menu",
            "delete.confirmation.prompt",
            "delete.call.service",
        ],
        "actions.Del.confirmation": [
            "delete.confirmation.prompt",
            "delete.confirmation.result",
            "delete.confirmation.nonzeroBranch",
            "delete.call.service",
        ],
        "actions.Copy.selectorArithmetic": [
            "copy.selector.saveFlags",
            "copy.selector.mask",
            "copy.selector.subtract",
        ],
        "actions.Copy.callOrder": ["copy.confirmation.prompt", "copy.call.service"],
        "actions.Copy.confirmation": [
            "copy.confirmation.prompt",
            "copy.confirmation.result",
            "copy.confirmation.nonzeroBranch",
            "copy.call.service",
        ],
        "sramServiceCalls": [
            "entry.checkSram.call",
            "new.call.saveGame",
            "load.call.loadGame",
            "copy.call.service",
            "delete.call.service",
        ],
    }
    if any(
        use_site_id not in use_site_ids
        for use_site_ids_for_summary in summary_provenance.values()
        for use_site_id in use_site_ids_for_summary
    ):
        raise ValueError("witch summary provenance reference drift")
    referenced_use_site_ids = {
        use_site_id
        for use_site_ids_for_summary in summary_provenance.values()
        for use_site_id in use_site_ids_for_summary
    }
    if referenced_use_site_ids != use_site_ids:
        raise ValueError("witch provenance has unreferenced use sites")
    return {"useSites": use_sites, "summaryUseSiteIds": summary_provenance}


def _witch_actions(
    start_source: str,
    listing: str,
    constants: dict[str, int],
    dispatch_indices: dict[str, int],
) -> list[dict[str, Any]]:
    new = _named_section(start_source, "witchMenuAction_New")
    load = _named_section(start_source, "witchMenuAction_Load")
    copy = _named_section(start_source, "witchMenuAction_Copy")
    delete = _named_section(start_source, "witchMenuAction_Del")
    new_selection = _action_selection(new, inverted=True, owner="witch new action")
    load_selection = _action_selection(load, inverted=False, owner="witch load action")
    delete_selection = _action_selection(delete, inverted=False, owner="witch delete action")
    new_configuration_pages = _immediate_register_values(new, "moveq", "d1", {})
    new_configuration_masks = _immediate_register_values(new, "moveq", "d2", {})
    if len(new_configuration_pages) < 2 or len(new_configuration_masks) != 1:
        raise ValueError("witch new action page/configuration drift")
    configuration_mask = new_configuration_masks[0]
    configuration_bits = [
        _integer_token(value, {}) for value in re.findall(r"btst\s+#([^,\s]+),d0", new)
    ]
    configuration_flags = [
        _integer_token(value, {}) for value in re.findall(r"setFlg\s+(\d+)", new)
    ]
    if len(configuration_flags) != len(configuration_bits):
        raise ValueError("witch new action configuration bit/flag drift")
    _require_ordered_fragments(
        new,
        (
            "move.b  #GAMESTART_MAP,((CURRENT_MAP-$1000000)).w",
            "move.b  #GAMESTART_MAP,((EGRESS_MAP-$1000000)).w",
            "bsr.w   SaveGame",
            "move.b  #GAMESTART_MAP,d0",
            "move.w  #GAMESTART_SAVEPOINT_X,d1",
            "move.w  #GAMESTART_SAVEPOINT_Y,d2",
            "move.w  #GAMESTART_FACING,d3",
            "moveq   #",
            "bra.w   MainLoop",
        ),
        "witch new action initial handoff",
    )
    copy_mask_values = _immediate_register_values(copy, "andi.w", "d0", {})
    copy_subtract_values = _immediate_register_values(copy, "subq.w", "d0", {})
    if copy_mask_values != [
        new_selection["availabilityTransform"]["saveFlagsMask"]
    ] or copy_subtract_values != [new_selection["selectionReturnSubtract"]]:
        raise ValueError("witch copy action selector arithmetic drift")
    d4_values = _immediate_register_values(new, "moveq", "d4", {})
    if len(d4_values) != 1:
        raise ValueError("witch new action main-loop handoff d4 drift")
    copy_confirm = re.search(r"tst\.w\s+d0\s*\n\s*(bne\.[sbwl])\s+(byte_73C2)", copy)
    delete_confirm = re.search(r"tst\.w\s+d0\s*\n\s*(bne\.[sbwl])\s+(byte_73C2)", delete)
    if copy_confirm is None or delete_confirm is None:
        raise ValueError("witch copy/delete confirmation branch polarity drift")
    load_branch = re.search(
        r"chkFlg\s+(\d+)[^\n]*\n\s*(beq\.[sbwl])\s+(@loc_18)"
        r".*?jsr\s+j_BattleLoop\s*\n\s*bra\.w\s+alt_MainLoopEntry"
        r".*?@loc_18:\s*\n.*?jsr\s+GetSavepointForMap\(pc\)\s*\n.*?bra\.w\s+alt_MainLoopEntry",
        load,
        re.DOTALL,
    )
    if load_branch is None:
        raise ValueError("witch load action battle/savepoint handoff drift")
    return [
        {
            "dispatchIndex": dispatch_indices["witchMenuAction_New"],
            "sourceLabel": "witchMenuAction_New",
            "selection": new_selection,
            "callOrder": _call_order(
                new,
                listing,
                (
                    "j_ExecuteWitchMainMenu",
                    "j_NewGame",
                    "j_NameAlly",
                    "j_NameAlly",
                    "CheatModeConfiguration",
                    "j_ExecuteWitchMainMenu",
                    "DisplayText",
                    "SaveGame",
                ),
                "witch new action",
            ),
            "configuration": {
                "pageIndex": new_configuration_pages[-1],
                "availabilityMask": configuration_mask,
                "choiceBitPositions": configuration_bits,
                "setFlagOperands": configuration_flags,
            },
            "initialMainLoopHandoff": {
                "map": _constant_reference("GAMESTART_MAP", constants),
                "savepointX": _constant_reference("GAMESTART_SAVEPOINT_X", constants),
                "savepointY": _constant_reference("GAMESTART_SAVEPOINT_Y", constants),
                "facing": _constant_reference("GAMESTART_FACING", constants),
                "d4Immediate": d4_values[0],
                "branchTarget": "MainLoop",
            },
        },
        {
            "dispatchIndex": dispatch_indices["witchMenuAction_Load"],
            "sourceLabel": "witchMenuAction_Load",
            "selection": load_selection,
            "callOrder": _call_order(
                load,
                listing,
                (
                    "j_ExecuteWitchMainMenu",
                    "LoadGame",
                    "CheatModeConfiguration",
                    "j_BattleLoop",
                    "GetSavepointForMap",
                ),
                "witch load action",
            ),
            "postLoadHandoff": {
                "flagOperand": _integer_token(load_branch.group(1), {}),
                "zeroBranchMnemonic": load_branch.group(2),
                "zeroBranchTarget": load_branch.group(3),
                "nonzeroCall": _call_identity(listing, "j_BattleLoop"),
                "zeroCall": _call_identity(listing, "GetSavepointForMap"),
                "sharedBranchTarget": "alt_MainLoopEntry",
            },
        },
        {
            "dispatchIndex": dispatch_indices["witchMenuAction_Del"],
            "sourceLabel": "witchMenuAction_Del",
            "selection": delete_selection,
            "callOrder": _call_order(
                delete,
                listing,
                ("j_ExecuteWitchMainMenu", "j_alt_YesNoPrompt", "ClearSaveSlotFlag"),
                "witch delete action",
            ),
            "confirmation": {
                "nonzeroBranchMnemonic": delete_confirm.group(1),
                "nonzeroBranchTarget": delete_confirm.group(2),
                "service": _call_identity(listing, "ClearSaveSlotFlag"),
            },
        },
        {
            "dispatchIndex": dispatch_indices["witchMenuAction_Copy"],
            "sourceLabel": "witchMenuAction_Copy",
            "selectorArithmetic": {
                "saveFlagsMask": copy_mask_values[0],
                "subtractImmediate": copy_subtract_values[0],
            },
            "callOrder": _call_order(
                copy,
                listing,
                ("j_alt_YesNoPrompt", "CopySave"),
                "witch copy action",
            ),
            "confirmation": {
                "nonzeroBranchMnemonic": copy_confirm.group(1),
                "nonzeroBranchTarget": copy_confirm.group(2),
                "service": _call_identity(listing, "CopySave"),
            },
        },
    ]


def _witch_dispatch_table(start_source: str, listing: str) -> list[dict[str, Any]]:
    section = _section(start_source, "rjt_WitchMenuActions", "witchMenuAction_New:")
    records: list[dict[str, Any]] = []
    pattern = re.compile(
        r"^\s*dc\.w\s+(witchMenuAction_[A-Za-z]+)-rjt_WitchMenuActions\s*$",
        re.MULTILINE,
    )
    section_start_line = start_source[: start_source.index(section)].count("\n") + 1
    matches = list(pattern.finditer(section))
    targets = [match.group(1) for match in matches]
    if targets != list(WITCH_ACTION_LABELS):
        raise ValueError("witch menu dispatch target/order drift")
    for index, match in enumerate(matches):
        target = targets[index]
        records.append(
            {
                "index": index,
                "sourceLine": section_start_line + section[: match.start()].count("\n"),
                "target": target,
                "targetAddress": _listing_address(listing, target),
            }
        )
    if len(records) != len(WITCH_ACTION_LABELS):
        raise ValueError("witch menu dispatch record count drift")
    return records


def _witch_sram_service_calls(sources: dict[str, str], listing: str) -> dict[str, Any]:
    instruction_totals = {target: 0 for target in WITCH_SRAM_SERVICE_TARGETS}
    effective_totals = {target: 0 for target in WITCH_SRAM_SERVICE_TARGETS}
    internal_effective_totals = {target: 0 for target in WITCH_SRAM_SERVICE_TARGETS}
    external_effective_totals = {target: 0 for target in WITCH_SRAM_SERVICE_TARGETS}
    source_labels = {
        label
        for source in sources.values()
        for label in re.findall(r"^([A-Za-z_][A-Za-z0-9_]*):", source, re.MULTILINE)
    }
    callers: list[dict[str, Any]] = []
    for source_path in (WITCH_START_PATH, WITCH_MAIN_MENU_PATH, WITCH_FUNCTIONS_PATH):
        for site in _direct_call_sites(sources[source_path.as_posix()]):
            target = site["instructionTarget"]
            if target not in instruction_totals:
                continue
            instruction_totals[target] += 1
            effective_totals[target] += 1
            if target in source_labels:
                internal_effective_totals[target] += 1
            else:
                external_effective_totals[target] += 1
            callers.append(
                {
                    "sourcePath": source_path.as_posix(),
                    "sourceLine": site["line"],
                    "instructionTarget": target,
                    "effectiveTarget": target,
                    "effectiveTargetAddress": _listing_address(listing, target),
                }
            )
    if any(total != 1 for total in effective_totals.values()):
        raise ValueError("witch SRAM service caller total drift")
    return {
        "declaredEffectiveTargets": list(WITCH_SRAM_SERVICE_TARGETS),
        "instructionTargetSiteCounts": instruction_totals,
        "effectiveTargetSiteCounts": effective_totals,
        "internalEffectiveTargetSiteCounts": internal_effective_totals,
        "externalEffectiveTargetSiteCounts": external_effective_totals,
        "callers": callers,
    }


def _witch_save_menu_facts_from_sources(
    disasm: Path, sources: dict[str, str], listing: str
) -> dict[str, Any]:
    start_source = sources[WITCH_START_PATH.as_posix()]
    constants = _read_equates(
        disasm / "sf2enums.asm",
        (
            "BYTE_LOWER_NIBBLE_MASK",
            "GAMESTART_MAP",
            "GAMESTART_SAVEPOINT_X",
            "GAMESTART_SAVEPOINT_Y",
            "GAMESTART_FACING",
        ),
    )
    dispatcher = _witch_dispatch_table(start_source, listing)
    actions = _witch_actions(
        start_source,
        listing,
        constants,
        {record["target"]: record["index"] for record in dispatcher},
    )
    entry = _witch_entry_flow(start_source, listing)
    action_masks = {
        action["selection"]["availabilityTransform"]["saveFlagsMask"]
        for action in actions
        if "selection" in action
    }
    if action_masks != {entry["actionPage"]["saveFlagsMask"]}:
        raise ValueError("witch action-page and slot-selector save-flags masks drift")
    if [action["sourceLabel"] for action in actions] != [record["target"] for record in dispatcher]:
        raise ValueError("witch action records no longer match dispatcher order")
    return {
        "sourcePaths": {
            "start": WITCH_START_PATH.as_posix(),
            "mainMenu": WITCH_MAIN_MENU_PATH.as_posix(),
            "functions": WITCH_FUNCTIONS_PATH.as_posix(),
        },
        "entry": entry,
        "dispatcher": dispatcher,
        "mainMenu": _witch_main_menu_facts(
            sources[WITCH_MAIN_MENU_PATH.as_posix()], listing, constants
        ),
        "actions": actions,
        "sramServiceCalls": _witch_sram_service_calls(sources, listing),
        "provenance": _witch_save_menu_provenance(sources),
        "runtimeQuestions": ["witch-save-menu-and-suspend-presentation"],
    }


def _witch_save_menu_facts(disasm: Path, sources: dict[str, str], listing: str) -> dict[str, Any]:
    return _witch_save_menu_facts_from_sources(disasm, sources, listing)


def _resource_targets(disasm: Path, paths: list[Path]) -> dict[str, str]:
    pattern = re.compile(
        r'^([A-Za-z_][A-Za-z0-9_]*):[^;\n]*(?:\n[ \t]*)?incbin[ \t]+"([^"]+)"',
        re.MULTILINE | re.IGNORECASE,
    )
    targets: dict[str, str] = {}
    for path in paths:
        for label, target in pattern.findall(read_upstream_text(path)):
            if label in targets:
                raise ValueError(f"duplicate special-screen resource label: {label}")
            if not (disasm / target).is_file():
                raise ValueError(f"missing special-screen resource target: {target}")
            targets[label] = target.replace("\\", "/")
    return dict(sorted(targets.items()))


def _screen_facts(
    disasm: Path, sources: dict[str, str], resource_targets: dict[str, str], listing: str
) -> dict[str, Any]:
    logo0 = sources["code/specialscreens/segalogo/segalogo_0.asm"]
    logo1 = sources["code/specialscreens/segalogo/segalogo_1.asm"]
    title = sources["code/specialscreens/title/title.asm"]
    witch = sources["code/specialscreens/witch/witchstart.asm"]
    sound_test = sources["code/specialscreens/witch/soundtest.asm"]
    suspend = sources["code/specialscreens/suspend/suspend.asm"]
    witch_suspend = sources["code/specialscreens/suspend/witchsuspend.asm"]
    witch_end = sources["code/specialscreens/witchend/witchend.asm"]
    end_kiss = sources["code/specialscreens/endkiss/endkissfunctions_0.asm"]
    _require_fragments(
        logo0,
        ("CalculateRomChecksum", "VInt_CheckConfigurationModeCheat", "DisplaySegaLogo_Quit"),
        "Sega logo",
    )
    _require_fragments(
        logo1,
        ("VInt_CheckDebugModeCheat", "VInt_ActivateDebugModeCheat", "CheckDebugModeInputSequence"),
        "Sega logo debug cheat",
    )
    _require_fragments(
        title,
        ("WaitForPlayer1InputStart:", "TitleScreenLoop1:", "TitleScreenLoop2:", "EndTitleScreen:"),
        "title screen",
    )
    menu_actions = re.findall(r"^(witchMenuAction_[A-Za-z]+):", witch, re.MULTILINE)
    if menu_actions != [
        "witchMenuAction_New",
        "witchMenuAction_Load",
        "witchMenuAction_Copy",
        "witchMenuAction_Del",
    ]:
        raise ValueError("witch menu action routing drift")
    _require_fragments(witch, ("bsr.w   CheckSram", "rjt_WitchMenuActions:"), "witch start")
    if not re.search(r"^j_SoundTest:\s+\n\s*rts\s*$", sound_test, re.MULTILINE):
        raise ValueError("US sound-test stub drift")
    _require_fragments(
        suspend,
        ("moveq   #60,d0", "LoadStackCompressedData", "ApplyVIntVramDma"),
        "suspend screen",
    )
    _require_fragments(
        witch_suspend,
        ("move.w  #600,d0", "INPUT_BIT_START", "movea.l (p_Start).w,a0"),
        "witch suspend",
    )
    _require_fragments(
        witch_end,
        ("VInt_FallingJewels:", "VInt_PerformEndingWitchBlink:"),
        "witch ending",
    )
    _require_fragments(
        end_kiss,
        ("DrawEndingKissPictureWithPixelFilling:", "table_EndingKissPixelFillingData:"),
        "ending kiss",
    )
    graphics_resources = {
        label: target
        for label, target in resource_targets.items()
        if "segalogo" not in target.casefold()
    }
    return {
        "groupFileCounts": EXPECTED_GROUP_COUNTS,
        "resourceEntryCount": len(resource_targets),
        "standaloneGraphicsResourceCount": len(graphics_resources),
        "embeddedSegaLogoResourceCount": len(resource_targets) - len(graphics_resources),
        "segaLogoComputesRomChecksum": True,
        "segaLogoSupportsConfigurationAndDebugCheats": True,
        "segaLogoCanReturnEarlyOnStart": True,
        "titleScrollLoopCount": 2,
        "titleHasBoundedStartPolling": True,
        "witchMenuActions": menu_actions,
        "witchChecksSramBeforeMenu": True,
        "usSoundTestIsReturnOnly": True,
        "suspendInitialSleepFrames": 60,
        "suspendRestartWaitFrames": 600,
        "suspendRestartCanExitEarlyOnStart": True,
        "suspendResetsThroughStartVector": True,
        "endingUsesPixelFillAndFallingJewels": True,
        "compressedTileCorpusConfirmed": True,
        "witchSaveMenu": _witch_save_menu_facts(disasm, sources, listing),
    }


def _index_records_for_source_root(source_paths: set[str]) -> dict[str, Any]:
    """Join every research record owned by a discovered special-screen source.

    ``sourcePath`` is deliberately the only membership predicate. A record may
    have originated in an H3 owner yet belongs to this inventory when its
    source path names one of the discovered special-screen files.
    """
    records_by_source_path: dict[str, list[str]] = {}
    for record in load_json(RESEARCH_INDEX)["records"]:
        source_path = record["sourcePath"]
        path = Path(source_path)
        if not path.is_relative_to(SOURCE_ROOT):
            continue
        if ".." in path.parts or source_path != path.as_posix():
            raise ValueError(f"invalid special-screens indexed source path: {source_path}")
        if source_path not in source_paths:
            raise ValueError(
                "special-screens indexed source is absent from the discovered root "
                f"inventory: {source_path}"
            )
        records_by_source_path.setdefault(source_path, []).append(record["id"])

    missing_paths = sorted(source_paths - set(records_by_source_path))
    if missing_paths:
        raise ValueError(
            "special-screens discovered source lacks a research-index record: "
            + ", ".join(missing_paths)
        )
    indexed_records_by_source_path = [
        {"sourcePath": source_path, "recordIds": sorted(record_ids)}
        for source_path, record_ids in sorted(records_by_source_path.items())
    ]
    indexed_record_ids = [
        record_id for row in indexed_records_by_source_path for record_id in row["recordIds"]
    ]
    if len(indexed_record_ids) != len(set(indexed_record_ids)):
        raise ValueError("special-screens research-index duplicate record ID")
    return {
        "indexedRecordIds": sorted(indexed_record_ids),
        "indexedSourcePaths": [row["sourcePath"] for row in indexed_records_by_source_path],
        "indexedRecordsBySourcePath": indexed_records_by_source_path,
    }


def _verify_indexed_record_join(
    output: dict[str, Any],
    expected_index_membership: dict[str, Any],
    discovered_source_paths: list[str],
) -> None:
    """Reconcile output membership against the authoritative research-index join."""
    relation = output["indexedRecordsBySourcePath"]
    relation_source_paths = [row["sourcePath"] for row in relation]
    relation_record_ids = [record_id for row in relation for record_id in row["recordIds"]]
    if len(relation_source_paths) != len(set(relation_source_paths)):
        raise ValueError("special-screens indexed relation duplicate source path")
    if len(relation_record_ids) != len(set(relation_record_ids)):
        raise ValueError("special-screens indexed relation duplicate record ID")
    if set(relation_source_paths) != set(discovered_source_paths):
        raise ValueError("special-screens indexed relation source inventory drift")
    if relation_source_paths != sorted(relation_source_paths):
        raise ValueError("special-screens indexed relation source order drift")
    if any(row["recordIds"] != sorted(row["recordIds"]) for row in relation):
        raise ValueError("special-screens indexed relation record order drift")

    indexed_record_ids = output["indexedRecordIds"]
    indexed_source_paths = output["indexedSourcePaths"]
    if indexed_record_ids != sorted(relation_record_ids):
        raise ValueError("special-screens indexedRecordIds relation drift")
    if indexed_source_paths != relation_source_paths:
        raise ValueError("special-screens indexedSourcePaths relation order drift")
    if output["summary"]["indexedRecordCount"] != len(indexed_record_ids) or output["summary"][
        "indexedRecordCount"
    ] != len(relation_record_ids):
        raise ValueError("special-screens summary indexedRecordCount relation drift")
    if output["summary"]["indexedFileCount"] != len(indexed_source_paths) or output["summary"][
        "indexedFileCount"
    ] != len(relation_source_paths):
        raise ValueError("special-screens summary indexedFileCount relation drift")

    file_paths = [row["path"] for row in output["files"]]
    if len(file_paths) != len(set(file_paths)):
        raise ValueError("special-screens source inventory duplicate path")
    if file_paths != discovered_source_paths:
        raise ValueError("special-screens source inventory drift")
    if indexed_source_paths != discovered_source_paths:
        raise ValueError("special-screens indexedSourcePaths source inventory drift")
    for field in (
        "indexedRecordIds",
        "indexedSourcePaths",
        "indexedRecordsBySourcePath",
    ):
        if output[field] != expected_index_membership[field]:
            raise ValueError(f"special-screens {field} source-membership drift")


def _verify_fixture_indexed_membership(output: dict[str, Any], expected: dict[str, Any]) -> None:
    """Keep the fixture's exact corpus comparison distinct from index membership."""
    for field in (
        "indexedRecordIds",
        "indexedSourcePaths",
        "indexedRecordsBySourcePath",
    ):
        if output[field] != expected[field]:
            raise ValueError(f"special-screens fixture {field} drift")


def _verify_fixture_provenance(fixture: dict[str, Any], output: dict[str, Any]) -> None:
    """Derive fixture provenance from the independently pinned manifests."""
    toolchain = load_json(TOOLCHAIN)["sf2disasm"]
    output_upstream = output["upstream"]
    if (
        fixture["upstreamCommit"] != toolchain["commit"]
        or fixture["upstreamCommit"] != output_upstream["commit"]
    ):
        raise ValueError("special-screens fixture upstream provenance drift")
    if output_upstream["repository"] != toolchain["repository"]:
        raise ValueError("special-screens output upstream provenance drift")
    if fixture["romSha256"] != load_json(ROM_MANIFEST)["hashes"]["sha256"]:
        raise ValueError("special-screens fixture ROM provenance drift")


def build_special_screen_inventory(upstream_path: Path) -> dict[str, Any]:
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"special-screens H1 listing is missing: {listing_path}")
    listing = listing_path.read_text(encoding="utf-8")
    paths = sorted((disasm / SOURCE_ROOT).rglob("*.asm"), key=lambda path: path.as_posix())
    group_counts = Counter(path.parent.name for path in paths)
    if dict(sorted(group_counts.items())) != EXPECTED_GROUP_COUNTS:
        raise ValueError("special-screen source group drift")
    files = [_parse_source_file(path, path.relative_to(disasm).as_posix()) for path in paths]
    layout = "\n".join(
        path.read_text(encoding="utf-8") for path in sorted((disasm / "layout").glob("*.asm"))
    )
    for row in files:
        if row["path"].replace("/", "\\") not in layout:
            raise ValueError(f"special-screen source is absent from layout: {row['path']}")
        if not row["globalLabels"]:
            raise ValueError(f"unexpected unlabeled special-screen file: {row['path']}")
    source_paths = [row["path"] for row in files]
    if len(source_paths) != len(set(source_paths)) or source_paths != sorted(source_paths):
        raise ValueError("special-screens discovered source inventory drift")
    representative_symbols = {row["path"]: row["globalLabels"][0] for row in files}
    representative_addresses = {
        symbol: _listing_address(listing, symbol) for symbol in representative_symbols.values()
    }
    index_membership = _index_records_for_source_root(set(source_paths))
    labels = {label for row in files for label in row["globalLabels"]}
    calls: Counter[str] = Counter()
    for row in files:
        for call in row["directCalls"]:
            calls[call["target"]] += call["siteCount"]
    sources = {path.relative_to(disasm).as_posix(): read_upstream_text(path) for path in paths}
    resource_targets = _resource_targets(disasm, paths)
    summary = {
        "fileCount": len(files),
        "screenGroupCount": len(group_counts),
        "sourceLineCount": sum(row["sourceLineCount"] for row in files),
        "statementCount": sum(row["statementCount"] for row in files),
        "globalLabelCount": sum(len(row["globalLabels"]) for row in files),
        "localLabelCount": sum(row["localLabelCount"] for row in files),
        "directCallSiteCount": sum(calls.values()),
        "uniqueDirectTargetCount": len(calls),
        "internalDirectTargetCount": sum(target in labels for target in calls),
        "externalDirectTargetCount": sum(target not in labels for target in calls),
        "layoutIncludedFileCount": len(files),
        "indexedRecordCount": len(index_membership["indexedRecordIds"]),
        "indexedFileCount": len(index_membership["indexedSourcePaths"]),
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "scope": SOURCE_ROOT.as_posix(),
        "summary": summary,
        **index_membership,
        "representativeSymbols": representative_symbols,
        "representativeAddresses": representative_addresses,
        "internalDirectCallTargets": sorted(target for target in calls if target in labels),
        "externalDirectCallTargets": sorted(target for target in calls if target not in labels),
        "screenFacts": _screen_facts(disasm, sources, resource_targets, listing),
        "resourceTargets": resource_targets,
        "runtimeQuestions": [
            "logo-title-cheat-and-input-presentation",
            "witch-save-menu-and-suspend-presentation",
            "ending-kiss-jewels-and-witch-presentation",
        ],
        "files": files,
    }


def verify_special_screen_inventory(
    upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_special_screen_inventory(upstream_path)
    validate_json(output, SCHEMA, owner="special-screens static inventory")
    disasm, _, _ = _resolve_upstream(upstream_path)
    discovered_source_paths = [
        path.relative_to(disasm).as_posix()
        for path in sorted((disasm / SOURCE_ROOT).rglob("*.asm"), key=lambda path: path.as_posix())
    ]
    expected_index_membership = _index_records_for_source_root(set(discovered_source_paths))
    _verify_indexed_record_join(output, expected_index_membership, discovered_source_paths)
    _verify_fixture_indexed_membership(output, fixture["expected"])
    _verify_fixture_provenance(fixture, output)
    if output["summary"] != manifest["summary"]:
        raise ValueError("special-screens summary drift")
    if output["representativeAddresses"] != fixture["function"]:
        raise ValueError("special-screens H1 address drift")
    if output["representativeSymbols"] != fixture["expected"]["representativeSymbols"]:
        raise ValueError("special-screens representative symbol drift")
    files_by_path = {row["path"]: row for row in output["files"]}
    for source_path, symbol in fixture["expected"]["representativeSymbols"].items():
        if (
            source_path not in files_by_path
            or symbol not in files_by_path[source_path]["globalLabels"]
        ):
            raise ValueError(f"special-screens representative source model drift: {source_path}")
    for field in ("screenFacts", "resourceTargets", "runtimeQuestions"):
        if output[field] != fixture["expected"][field]:
            raise ValueError(f"special-screens {field} drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"]:
        raise ValueError("special-screens canonical hash drift")
    destination = output_path or repo_path("local/derived/special-screens-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Inventory": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Files": output["summary"]["fileCount"],
        "ScreenGroups": output["summary"]["screenGroupCount"],
        "Resources": output["screenFacts"]["resourceEntryCount"],
        "RuntimeQuestions": len(output["runtimeQuestions"]),
        "Status": "PASS",
    }
