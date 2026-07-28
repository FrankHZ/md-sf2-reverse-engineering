from collections import Counter
from copy import deepcopy
from pathlib import Path

import pytest

from sf2tool.h2 import map_script_engine
from sf2tool.h2.map_script_engine import (
    DIALOGUE_HANDLER_BY_MACRO,
    DIALOGUE_MACROS,
    DIALOGUE_MODIFIER_MACROS,
    ENTITY_DIALOGUE_CONSUMER,
    ENTITY_DIALOGUE_CONSUMER_PATH,
    PORTRAIT_HANDLER,
    _cursor_flow,
    _dialogue_handler_facts,
    _direct_call_sites,
    _emission_rows,
    _entity_dialogue_consumer_facts,
    _force_state_aliases,
    _force_state_direct_calls,
    _force_state_program_facts,
    _force_state_section_guard,
    _logical_source_lines,
    _modifier_source_labels,
    _program_corpus,
    _statements,
    _story_state_facts,
    _substitute_alias_layout,
    build_map_script_engine_contract,
)
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path


def test_emission_rows_preserve_shorthand_width_and_stream_offset() -> None:
    rows = _emission_rows(
        """
        dc.w $22
        dc.w \\1
        defineShorthand.w ENTITY_TRANSITION_,\\2
        """
    )

    assert [row["streamOffset"] for row in rows] == [0, 2, 4]
    assert [row["widthBytes"] for row in rows] == [2, 2, 2]
    assert rows[2]["encoding"] == "shorthand:ENTITY_TRANSITION_"
    assert rows[2]["parameterOrdinals"] == [2]


def test_alias_layout_substitutes_constants_without_losing_physical_fields() -> None:
    layout = _emission_rows("dc.w \\1\ndc.w \\2")
    actual = _substitute_alias_layout(layout, ["\\1", "$FFFF"])

    assert [row["expression"] for row in actual] == ["\\1", "$FFFF"]
    assert [row["parameterOrdinals"] for row in actual] == [[1], []]


def test_cursor_flow_distinguishes_jump_and_inline_program_shapes() -> None:
    assert _cursor_flow("csc0B_jump", ["movea.l (a6),a6"]) == "absolute-jump"
    assert (
        _cursor_flow(
            "csc0C_jumpIfFlagSet",
            ["movea.l (a6),a6", "addq.w #4,a6"],
        )
        == "conditional-absolute-jump"
    )
    assert (
        _cursor_flow(
            "csc14_setEntityActscriptManual",
            [
                "move.l a6,ENTITYDEF_OFFSET_ACTSCRIPTADDR(a5)",
                "cmpi.w #$8080,(a6)+",
            ],
        )
        == "inline-action-program"
    )


def test_logical_source_lines_join_ampersand_continuations() -> None:
    assert _logical_source_lines("  setBlocks 1,2,&\n    3,4,5,6\n") == [
        (1, "  setBlocks 1,2, 3,4,5,6")
    ]


def test_force_state_call_parser_ignores_near_misses_and_accepts_size_suffixes() -> None:
    calls = _force_state_direct_calls(
        [
            "jsr.w j_JoinForce",
            "bsr.l GetCurrentHp",
            "move.w j_JoinForce,d0",
            "j_JoinForce:",
            "jsr (a0)",
            "jsr j_JoinForce ; comment is already stripped by handler parsing",
        ]
    )

    assert calls == [
        {"opcode": "jsr", "instructionTarget": "j_JoinForce"},
        {"opcode": "bsr", "instructionTarget": "GetCurrentHp"},
        {"opcode": "jsr", "instructionTarget": "j_JoinForce"},
    ]


def test_force_state_program_totals_keep_zero_rows_and_exact_site_order() -> None:
    corpus = {
        "summary": {"programCount": 2},
        "programs": [
            {
                "id": "first",
                "commands": [
                    {"index": 0, "sourceLine": 10, "macro": "join", "arguments": ["1"]},
                    {"index": 1, "sourceLine": 11, "macro": "wait", "arguments": []},
                ],
            },
            {"id": "second", "commands": []},
        ],
    }

    sites, totals = _force_state_program_facts(corpus)

    assert sites == [
        {
            "programId": "first",
            "commands": [
                {"commandIndex": 0, "sourceLine": 10, "macro": "join", "arguments": ["1"]}
            ],
        }
    ]
    assert totals == [
        {
            "programId": "first",
            "commandCount": 1,
            "macroCounts": {
                "join": 1,
                "jumpIfDefeatedByLastAttack": 0,
                "jumpIfDead": 0,
                "allyDefeated": 0,
                "updateDefeatedAllies": 0,
                "reviveAlly": 0,
            },
        },
        {
            "programId": "second",
            "commandCount": 0,
            "macroCounts": {
                "join": 0,
                "jumpIfDefeatedByLastAttack": 0,
                "jumpIfDead": 0,
                "allyDefeated": 0,
                "updateDefeatedAllies": 0,
                "reviveAlly": 0,
            },
        },
    ]


def test_force_state_alias_parser_requires_named_jump_and_accepts_size_suffix(
    tmp_path: Path,
) -> None:
    interface = tmp_path / "code/common/tech/jumpinterfaces"
    interface.mkdir(parents=True)
    alias_path = interface / "s02_jumpinterface.asm"
    alias_path.write_text(
        "; j_Target in a comment is not an alias definition\n"
        "j_Target:\n"
        "    jmp.w Target(pc) ; legal instruction-size suffix\n",
        encoding="utf-8",
    )
    addresses = {"j_Target": 0, "Target": 4}
    rom = b"\x4e\xfa\x00\x02" + b"\x00" * 4

    assert _force_state_aliases(tmp_path, {"j_Target"}, addresses, rom) == {
        "j_Target": {
            "effectiveTarget": "Target",
            "sourcePath": "code/common/tech/jumpinterfaces/s02_jumpinterface.asm",
        }
    }

    alias_path.write_text("j_Target:\n    jsr Target(pc)\n", encoding="utf-8")
    with pytest.raises(ValueError, match="alias instruction drift"):
        _force_state_aliases(tmp_path, {"j_Target"}, addresses, rom)


def test_program_corpus_owns_anonymous_and_jump_terminated_programs(tmp_path) -> None:
    (tmp_path / "code").mkdir()
    (tmp_path / "data").mkdir()
    (tmp_path / "sf2enums.asm").write_text(
        "FLAG_INDEX_YES_NO_PROMPT: equ 89\n"
        "BATTLE_UNLOCKED_FLAGS_START: equ 400\n",
        encoding="utf-8",
    )
    (tmp_path / "code" / "one.asm").write_text(
        "entry:\n  csc_end\n\n  csc_end\n", encoding="utf-8"
    )
    (tmp_path / "data" / "two.asm").write_text(
        "tail:\n  jump entry\n", encoding="utf-8"
    )
    contracts = {
        "csc_end": {"kind": "terminator", "opcode": None, "encodedBytes": 2},
        "jump": {"kind": "command", "opcode": 11, "encodedBytes": 6},
    }

    actual = _program_corpus(
        tmp_path,
        ["code/one.asm", "data/two.asm"],
        contracts,
        {"entry": 0x100, "tail": 0x200},
    )

    assert actual["summary"]["programCount"] == 3
    assert actual["summary"]["anonymousProgramCount"] == 1
    assert actual["summary"]["absoluteJumpTerminatedProgramCount"] == 1
    assert actual["transferCounts"] == {"absolute-jump:cross-program": 1}
    assert actual["referenceSummary"]["referencedProgramCount"] == 1
    assert actual["referenceSummary"]["unreferencedProgramCount"] == 2
    assert actual["referenceSummary"]["referencedLabelCount"] == 1
    assert actual["referenceSummary"]["unreferencedLabelCount"] == 1


def test_story_state_facts_resolve_prompt_and_battle_flag_domains(tmp_path) -> None:
    (tmp_path / "sf2enums.asm").write_text(
        "FLAG_INDEX_YES_NO_PROMPT: equ 89\n"
        "BATTLE_UNLOCKED_FLAGS_START: equ 400\n",
        encoding="utf-8",
    )
    programs = [
        {
            "id": "scene",
            "commands": [
                {
                    "index": 0,
                    "macro": "jumpIfFlagSet",
                    "arguments": ["89", "target"],
                    "targetSymbol": "target",
                },
                {"index": 1, "macro": "yesNo", "arguments": []},
                {"index": 2, "macro": "setF", "arguments": ["70"]},
                {"index": 3, "macro": "clearF", "arguments": ["71"]},
                {"index": 4, "macro": "setStoryFlag", "arguments": ["4"]},
            ],
        }
    ]

    actual = _story_state_facts(tmp_path, programs)

    assert actual["summary"]["conditionalReadCount"] == 1
    assert actual["summary"]["uniqueWriteFlagCount"] == 4
    assert actual["constants"] == {
        "yesNoPromptFlag": 89,
        "battleUnlockedFlagsStart": 400,
    }
    assert actual["readWriteOverlapFlags"] == [89]
    assert actual["battleUnlockFlags"] == [404]


def _named_handler(name: str, statements: list[str]) -> str:
    return "\n".join(
        [
            f"{name}:",
            *[f"    {statement}" for statement in statements],
            f"; End of function {name}",
            "",
        ]
    )


def _synthetic_dialogue_handler_inputs(tmp_path: Path):
    map_path = tmp_path / "code/common/scripting/map"
    map_path.mkdir(parents=True)
    second_path = map_path / "mapscriptengine_2.asm"
    first_path = map_path / "mapscriptengine_1.asm"
    bodies: dict[str, list[str]] = {}

    for macro in DIALOGUE_MACROS[:4]:
        is_single = macro.startswith("nextSingle")
        has_skip_guard = macro in {"nextSingleText", "nextText"}
        has_vars = macro.endswith("Var")
        statements = []
        if has_skip_guard:
            statements.extend(["tst.b ((SKIP_CUTSCENE_TEXT-$1000000)).w", "bne.s @skip"])
        statements.extend(
            [
                "cmpi.w #-1,(a6)",
                "beq.s @noPortrait",
                "bsr.w csc1D_showPortrait",
                "bsr.w GetEntityPortaitAndSpeechSfx",
            ]
        )
        if has_vars:
            statements.extend(
                [
                    "move.w (a6)+,((DIALOGUE_NAME_INDEX_1-$1000000)).w",
                    "move.w (a6)+,((DIALOGUE_NAME_INDEX_2-$1000000)).w",
                ]
            )
        statements.extend(
            [
                "jsr (DisplayText).l",
                "addq.w #1,((CUTSCENE_DIALOG_INDEX-$1000000)).w",
            ]
        )
        if is_single:
            statements.extend(
                ["jsr j_ClosePortraitWindow", "clsTxt", "moveq #10,d0", "jsr (Sleep).w"]
            )
        statements.append("rts")
        bodies[DIALOGUE_HANDLER_BY_MACRO[macro]] = statements

    bodies[DIALOGUE_HANDLER_BY_MACRO["textCursor"]] = [
        "move.w (a6)+,((CUTSCENE_DIALOG_INDEX-$1000000)).w",
        "rts",
    ]
    bodies[DIALOGUE_HANDLER_BY_MACRO["hideText"]] = [
        "jsr j_ClosePortraitWindow",
        "clsTxt",
        "rts",
    ]
    second_path.write_text(
        "".join(_named_handler(name, statements) for name, statements in bodies.items()),
        encoding="utf-8",
    )
    portrait_statements = [
        "move.w (a6)+,d0",
        "moveq #0,d3",
        "btst #$F,d0",
        "beq.s @rightDone",
        "moveq #-1,d3",
        "moveq #0,d4",
        "btst #$E,d0",
        "beq.s @mirrorDone",
        "moveq #-1,d4",
        "bsr.w GetEntityPortaitAndSpeechSfx",
        "rts",
    ]
    first_path.write_text(
        _named_handler("csc1D_showPortrait", portrait_statements), encoding="utf-8"
    )

    widths = [4, 6, 4, 8, 4, 2]
    targets = ["csc_doNothing"] * 10
    handlers = []
    opcodes_by_macro = {
        "nextSingleText": 0,
        "nextSingleTextVar": 1,
        "nextText": 2,
        "nextTextVar": 3,
        "textCursor": 4,
        "hideText": 9,
    }
    for address, (macro, width) in enumerate(
        zip(DIALOGUE_MACROS, widths, strict=True), start=100
    ):
        name = DIALOGUE_HANDLER_BY_MACRO[macro]
        targets[opcodes_by_macro[macro]] = name
        handlers.append(
            {
                "name": name,
                "opcodes": [next(index for index, target in enumerate(targets) if target == name)],
                "encodedCommandBytes": width,
                "sourcePath": "code/common/scripting/map/mapscriptengine_2.asm",
                "statementCount": len(_statements("\n".join(bodies[name]))),
                "address": address,
            }
        )
    handlers.append(
        {
            "name": "csc1D_showPortrait",
            "opcodes": [29],
            "encodedCommandBytes": 4,
            "sourcePath": "code/common/scripting/map/mapscriptengine_1.asm",
            "statementCount": len(_statements("\n".join(portrait_statements))),
            "address": 99,
        }
    )
    macros = {
        name: {
            "kind": "command",
            "aliasOf": None,
            "opcode": index,
            "encodedBytes": widths[position],
        }
        for position, (name, index) in enumerate(
            zip(DIALOGUE_MACROS, opcodes_by_macro.values(), strict=True)
        )
    }
    return macros, targets, handlers


def _modifier_entity_pairs() -> Counter[tuple[int, int]]:
    return Counter({(0, 1): 1, (128, 128): 1, (192, 128): 1, (255, 255): 1})


def _synthetic_entity_dialogue_consumer() -> dict[str, str]:
    return {
        "function": ENTITY_DIALOGUE_CONSUMER,
        "sourcePath": ENTITY_DIALOGUE_CONSUMER_PATH.as_posix(),
    }


def test_dialogue_handler_guards_reject_use_site_order_and_operand_mutations(tmp_path) -> None:
    macros, targets, handlers = _synthetic_dialogue_handler_inputs(tmp_path)
    facts, portrait, callers = _dialogue_handler_facts(
        tmp_path,
        macros,
        targets,
        handlers,
        _modifier_entity_pairs(),
        _synthetic_entity_dialogue_consumer(),
    )

    assert [row["macro"] for row in facts] == list(DIALOGUE_MACROS)
    assert portrait["handlerTestedModifierByteMask"] == 192
    assert portrait["modifierBitTests"] == [
        {"bit": 15, "destination": "d3"},
        {"bit": 14, "destination": "d4"},
    ]
    targets_by_handler = {
        "csc00_displaySingleTextbox": (1, 1),
        "csc01_displaySingleTextboxWithVars": (1, 1),
        "csc02_displayTextbox": (1, 1),
        "csc03_displayTextboxWithVars": (1, 1),
        "csc04_setTextIndex": (0, 0),
        "csc09_hideDialogueAndPortraitWindows": (0, 0),
        PORTRAIT_HANDLER: (0, 1),
    }
    assert callers == {
        "callerHandlers": [
            {
                "handler": handler,
                "sourcePath": (
                    "code/common/scripting/map/mapscriptengine_1.asm"
                    if handler == PORTRAIT_HANDLER
                    else "code/common/scripting/map/mapscriptengine_2.asm"
                ),
                "instructionTargetSiteCounts": {
                    PORTRAIT_HANDLER: portrait_count,
                    ENTITY_DIALOGUE_CONSUMER: consumer_count,
                },
                "effectiveTargetSiteCounts": {
                    PORTRAIT_HANDLER: portrait_count,
                    ENTITY_DIALOGUE_CONSUMER: consumer_count,
                },
            }
            for handler, (portrait_count, consumer_count) in targets_by_handler.items()
        ],
        "targetResolutions": [
            {
                "instructionTarget": PORTRAIT_HANDLER,
                "effectiveTarget": PORTRAIT_HANDLER,
                "effectiveTargetSourcePath": "code/common/scripting/map/mapscriptengine_1.asm",
                "effectiveTargetScope": "internal",
            },
            {
                "instructionTarget": ENTITY_DIALOGUE_CONSUMER,
                "effectiveTarget": ENTITY_DIALOGUE_CONSUMER,
                "effectiveTargetSourcePath": ENTITY_DIALOGUE_CONSUMER_PATH.as_posix(),
                "effectiveTargetScope": "external",
            },
        ],
        "instructionTargetTotals": {PORTRAIT_HANDLER: 4, ENTITY_DIALOGUE_CONSUMER: 5},
        "effectiveTargetTotals": {PORTRAIT_HANDLER: 4, ENTITY_DIALOGUE_CONSUMER: 5},
        "internalInstructionTargetTotals": {PORTRAIT_HANDLER: 4, ENTITY_DIALOGUE_CONSUMER: 0},
        "externalInstructionTargetTotals": {PORTRAIT_HANDLER: 0, ENTITY_DIALOGUE_CONSUMER: 5},
        "internalEffectiveTargetTotals": {PORTRAIT_HANDLER: 4, ENTITY_DIALOGUE_CONSUMER: 0},
        "externalEffectiveTargetTotals": {PORTRAIT_HANDLER: 0, ENTITY_DIALOGUE_CONSUMER: 5},
    }

    bad_macros = deepcopy(macros)
    bad_macros["nextText"]["opcode"] = 1
    with pytest.raises(ValueError, match="dispatcher target"):
        _dialogue_handler_facts(
            tmp_path,
            bad_macros,
            targets,
            handlers,
            _modifier_entity_pairs(),
            _synthetic_entity_dialogue_consumer(),
        )

    engine = tmp_path / "code/common/scripting/map/mapscriptengine_2.asm"
    engine.write_text(
        engine.read_text(encoding="utf-8").replace(
            "addq.w #1,((CUTSCENE_DIALOG_INDEX-$1000000)).w",
            "addq.w #2,((CUTSCENE_DIALOG_INDEX-$1000000)).w",
            1,
        ),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="addq"):
        _dialogue_handler_facts(
            tmp_path,
            macros,
            targets,
            handlers,
            _modifier_entity_pairs(),
            _synthetic_entity_dialogue_consumer(),
        )


def test_dialogue_handler_guards_reject_call_order_mutation(tmp_path) -> None:
    macros, targets, handlers = _synthetic_dialogue_handler_inputs(tmp_path)
    engine = tmp_path / "code/common/scripting/map/mapscriptengine_2.asm"
    engine.write_text(
        engine.read_text(encoding="utf-8").replace(
            "bsr.w GetEntityPortaitAndSpeechSfx\n    jsr (DisplayText).l",
            "jsr (DisplayText).l\n    bsr.w GetEntityPortaitAndSpeechSfx",
            1,
        ),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="DisplayText"):
        _dialogue_handler_facts(
            tmp_path,
            macros,
            targets,
            handlers,
            _modifier_entity_pairs(),
            _synthetic_entity_dialogue_consumer(),
        )

    target_root = tmp_path / "target"
    macros, targets, handlers = _synthetic_dialogue_handler_inputs(target_root)
    engine = target_root / "code/common/scripting/map/mapscriptengine_2.asm"
    engine.write_text(
        engine.read_text(encoding="utf-8").replace(
            "bsr.w csc1D_showPortrait", "bsr.w csc1C_otherPortrait", 1
        ),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="helper call count"):
        _dialogue_handler_facts(
            target_root,
            macros,
            targets,
            handlers,
            _modifier_entity_pairs(),
            _synthetic_entity_dialogue_consumer(),
        )


def test_dialogue_handler_guards_reject_sentinel_skip_bit_and_name_mutations(tmp_path) -> None:
    sentinel_root = tmp_path / "sentinel"
    macros, targets, handlers = _synthetic_dialogue_handler_inputs(sentinel_root)
    second = sentinel_root / "code/common/scripting/map/mapscriptengine_2.asm"
    second.write_text(
        second.read_text(encoding="utf-8").replace("cmpi.w #-1,(a6)", "cmpi.w #0,(a6)", 1),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="disagree"):
        _dialogue_handler_facts(
            sentinel_root,
            macros,
            targets,
            handlers,
            _modifier_entity_pairs(),
            _synthetic_entity_dialogue_consumer(),
        )

    skip_root = tmp_path / "skip"
    macros, targets, handlers = _synthetic_dialogue_handler_inputs(skip_root)
    second = skip_root / "code/common/scripting/map/mapscriptengine_2.asm"
    second.write_text(
        second.read_text(encoding="utf-8").replace("bne.s @skip", "beq.s @skip", 1),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="bne"):
        _dialogue_handler_facts(
            skip_root,
            macros,
            targets,
            handlers,
            _modifier_entity_pairs(),
            _synthetic_entity_dialogue_consumer(),
        )

    bit_root = tmp_path / "bit"
    macros, targets, handlers = _synthetic_dialogue_handler_inputs(bit_root)
    first = bit_root / "code/common/scripting/map/mapscriptengine_1.asm"
    first.write_text(
        first.read_text(encoding="utf-8").replace("btst #$F,d0", "btst #$D,d0", 1),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="handler-tested modifier"):
        _dialogue_handler_facts(
            bit_root,
            macros,
            targets,
            handlers,
            _modifier_entity_pairs(),
            _synthetic_entity_dialogue_consumer(),
        )

    unsupported_root = tmp_path / "unsupported"
    macros, targets, handlers = _synthetic_dialogue_handler_inputs(unsupported_root)
    unsupported_pairs = _modifier_entity_pairs()
    unsupported_pairs[(1, 1)] = 1
    with pytest.raises(ValueError, match="handler-tested modifier"):
        _dialogue_handler_facts(
            unsupported_root,
            macros,
            targets,
            handlers,
            unsupported_pairs,
            _synthetic_entity_dialogue_consumer(),
        )

    name_root = tmp_path / "name"
    macros, targets, handlers = _synthetic_dialogue_handler_inputs(name_root)
    second = name_root / "code/common/scripting/map/mapscriptengine_2.asm"
    second.write_text(
        second.read_text(encoding="utf-8").replace(
            "DIALOGUE_NAME_INDEX_2", "DIALOGUE_NAME_INDEX_3", 1
        ),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="name-word"):
        _dialogue_handler_facts(
            name_root,
            macros,
            targets,
            handlers,
            _modifier_entity_pairs(),
            _synthetic_entity_dialogue_consumer(),
        )


def test_entity_dialogue_consumer_guard_rejects_mask_mutation(tmp_path) -> None:
    path = tmp_path / "code/common/scripting/entity"
    path.mkdir(parents=True)
    source_path = path / "getentityportaitandspeechsfx.asm"
    source_path.write_text(
        _named_handler(
            "GetEntityPortaitAndSpeechSfx",
            [
                "andi.w #COMBATANT_MASK_ALL,d0",
                "bsr.w GetEntityAddressFromCharacter",
                "move.b ENTITYDEF_OFFSET_MAPSPRITE(a5),d0",
                "rts",
            ],
        ),
        encoding="utf-8",
    )
    constants = {"COMBATANT_MASK_ALL": 255, "COMBATANT_MASK_INDEX": 63}
    actual = _entity_dialogue_consumer_facts(
        tmp_path, constants, {"GetEntityPortaitAndSpeechSfx": 0x45638}
    )
    assert actual["lowDomainMask"] == {"constant": "COMBATANT_MASK_ALL", "value": 255}

    source_path.write_text(
        source_path.read_text(encoding="utf-8").replace(
            "COMBATANT_MASK_ALL", "COMBATANT_MASK_INDEX"
        ),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="low-domain mask"):
        _entity_dialogue_consumer_facts(
            tmp_path, constants, {"GetEntityPortaitAndSpeechSfx": 0x45638}
        )


def test_dialogue_text_cursor_rejects_source_line_domain_boundary(monkeypatch) -> None:
    original = map_script_engine.build_text_line_domain_contract

    def narrowed_domain(*args, **kwargs):
        value = original(*args, **kwargs)
        value["gamescriptFacts"]["lastLineId"] = 4232
        return value

    monkeypatch.setattr(map_script_engine, "build_text_line_domain_contract", narrowed_domain)
    with pytest.raises(ValueError, match="outside the source text-line domain"):
        build_map_script_engine_contract(
            repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
        )


def test_dialogue_modifier_labels_and_call_parser_reject_near_misses(tmp_path) -> None:
    (tmp_path / "sf2cutscenemacros.asm").write_text(
        "".join(
            f"{macro}: macro\n"
            "    dc.b \\1 ; portrait modifier "
            "($0-none, $40-mirrored, $80-display on right, $FF-undisplayed)\n"
            "    endm\n"
            for macro in DIALOGUE_MODIFIER_MACROS
        ),
        encoding="utf-8",
    )
    labels = _modifier_source_labels(
        tmp_path,
        [{"bit": 15, "destination": "d3"}, {"bit": 14, "destination": "d4"}],
        0xFFFF,
    )
    assert labels[1] == {
        "modifierByteValue": 64,
        "sourceLabel": "mirrored",
        "handlerWordBit": 14,
    }
    source = _statements(
        "; bsr.w GetEntityPortaitAndSpeechSfx\n"
        "GetEntityPortaitAndSpeechSfx:\n"
        "bsr.s GetEntityPortaitAndSpeechSfx ; legal short suffix\n"
        "jsr (GetEntityPortaitAndSpeechSfx).w\n"
        "move.w #GetEntityPortaitAndSpeechSfx,d0\n"
    )
    assert _direct_call_sites(source, "GetEntityPortaitAndSpeechSfx") == [0, 1]

    macro_path = tmp_path / "sf2cutscenemacros.asm"
    macro_path.write_text(
        macro_path.read_text(encoding="utf-8").replace("$40-mirrored", "$20-mirrored"),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="handler bit test"):
        _modifier_source_labels(
            tmp_path,
            [{"bit": 15, "destination": "d3"}, {"bit": 14, "destination": "d4"}],
            0xFFFF,
        )


@pytest.fixture(scope="module")
def map_script_engine_output() -> dict:
    return build_map_script_engine_contract(
        repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
    )


def test_dialogue_contract_matches_complete_golden_fixture(map_script_engine_output: dict) -> None:
    fixture = load_json(repo_path("tests/fixtures/h2/map-script-engine-static-v1.json"))
    actual = map_script_engine_output["dialogueCommandFacts"]

    assert actual == fixture["expected"]["dialogueCommandFacts"]
    assert len(actual["programTotals"]) == 304
    assert sum(
        len(row["commandIndexes"]) for row in actual["sourceSiteReferences"]
    ) == 2883


def test_dialogue_schemas_reject_missing_extra_reordered_and_boundary_content(
    map_script_engine_output: dict,
) -> None:
    output_schema = repo_path("schemas/map-script-engine-static.schema.json")
    fixture_schema = repo_path("schemas/h2-map-script-engine-static-fixture.schema.json")
    fixture = load_json(repo_path("tests/fixtures/h2/map-script-engine-static-v1.json"))
    validate_json(map_script_engine_output, output_schema, owner="dialogue output")
    validate_json(fixture, fixture_schema, owner="dialogue fixture")

    missing = deepcopy(map_script_engine_output)
    del missing["dialogueCommandFacts"]["macros"][0]["operandLayout"][0]["widthBytes"]
    with pytest.raises(ValueError, match="widthBytes"):
        validate_json(missing, output_schema, owner="dialogue output missing field")

    renamed = deepcopy(map_script_engine_output)
    operand = renamed["dialogueCommandFacts"]["macros"][0]["operandLayout"][0]
    operand["widthByte"] = operand.pop("widthBytes")
    with pytest.raises(ValueError, match="widthBytes"):
        validate_json(renamed, output_schema, owner="dialogue output renamed field")

    extra = deepcopy(map_script_engine_output)
    extra["dialogueCommandFacts"]["entityDialogueConsumer"]["lowDomainMask"]["extra"] = 1
    with pytest.raises(ValueError, match="extra"):
        validate_json(extra, output_schema, owner="dialogue output extra field")

    reordered = deepcopy(map_script_engine_output)
    references = reordered["dialogueCommandFacts"]["sourceSiteReferences"]
    references[0], references[1] = references[1], references[0]
    with pytest.raises(ValueError, match="const"):
        validate_json(reordered, output_schema, owner="dialogue output reordered sites")

    missing_zero_caller = deepcopy(map_script_engine_output)
    del missing_zero_caller["dialogueCommandFacts"]["callerBreakdown"]["callerHandlers"][4]
    with pytest.raises(ValueError, match="const"):
        validate_json(
            missing_zero_caller, output_schema, owner="dialogue output missing zero caller"
        )

    extra_caller_target = deepcopy(map_script_engine_output)
    extra_caller_target["dialogueCommandFacts"]["callerBreakdown"]["callerHandlers"][4][
        "instructionTargetSiteCounts"
    ]["csc1C_otherPortrait"] = 0
    with pytest.raises(ValueError, match="csc1C_otherPortrait"):
        validate_json(
            extra_caller_target, output_schema, owner="dialogue output extra caller target"
        )

    reordered_callers = deepcopy(map_script_engine_output)
    caller_rows = reordered_callers["dialogueCommandFacts"]["callerBreakdown"][
        "callerHandlers"
    ]
    caller_rows[4], caller_rows[5] = caller_rows[5], caller_rows[4]
    with pytest.raises(ValueError, match="const"):
        validate_json(reordered_callers, output_schema, owner="dialogue output reordered callers")

    boundary = deepcopy(fixture)
    bounds = boundary["expected"]["dialogueCommandFacts"]["operandFacts"][
        "textCursorValueBounds"
    ]
    bounds["maximum"] += 1
    with pytest.raises(ValueError, match="const"):
        validate_json(boundary, fixture_schema, owner="dialogue fixture boundary")


def test_transition_contract_and_closed_schema_mutations(map_script_engine_output: dict) -> None:
    output_schema = repo_path("schemas/map-script-engine-static.schema.json")
    fixture_schema = repo_path("schemas/h2-map-script-engine-static-fixture.schema.json")
    fixture = load_json(repo_path("tests/fixtures/h2/map-script-engine-static-v1.json"))
    actual = map_script_engine_output["transitionCommandFacts"]
    assert actual == fixture["expected"]["transitionCommandFacts"]
    assert actual["handlers"] == [
        {
            "macro": "warp",
            "handler": "csc07_warp",
            "address": 291714,
            "opcode": 7,
            "cursorReadWidths": [1, 1, 1, 1],
            "mapEventTypeValue": 1,
            "mapEventClearByteValue": 0,
            "d1Immediate": None,
            "packedCoordinateMultiplier": None,
            "directServiceCalls": [],
            "fallsThroughTo": None,
        },
        {
            "macro": "resetMap",
            "handler": "csc36_resetMap",
            "address": 288142,
            "opcode": 54,
            "cursorReadWidths": [],
            "mapEventTypeValue": None,
            "mapEventClearByteValue": None,
            "d1Immediate": None,
            "packedCoordinateMultiplier": None,
            "directServiceCalls": ["ResetCurrentMap"],
            "fallsThroughTo": None,
        },
        {
            "macro": "loadMapFadeIn",
            "handler": "csc37_loadMapAndFadeIn",
            "address": 288154,
            "opcode": 55,
            "cursorReadWidths": [],
            "mapEventTypeValue": None,
            "mapEventClearByteValue": None,
            "d1Immediate": None,
            "packedCoordinateMultiplier": None,
            "directServiceCalls": [],
            "fallsThroughTo": "csc48_loadMap",
        },
        {
            "macro": "reloadMap",
            "handler": "csc46_reloadMap",
            "address": 288520,
            "opcode": 70,
            "cursorReadWidths": [2, 2],
            "mapEventTypeValue": None,
            "mapEventClearByteValue": None,
            "d1Immediate": -1,
            "packedCoordinateMultiplier": 3,
            "directServiceCalls": ["LoadMap", "EnableDisplayAndInterrupts"],
            "fallsThroughTo": None,
        },
        {
            "macro": "mapLoad",
            "handler": "csc48_loadMap",
            "address": 288182,
            "opcode": 72,
            "cursorReadWidths": [2, 2, 2],
            "mapEventTypeValue": None,
            "mapEventClearByteValue": None,
            "d1Immediate": None,
            "packedCoordinateMultiplier": 3,
            "directServiceCalls": [
                "LoadMapTilesets",
                "LoadMap",
                "EnableDisplayAndInterrupts",
            ],
            "fallsThroughTo": None,
        },
    ]
    assert actual["callerBreakdown"] == {
        "callerHandlers": [
            {
                "handler": "csc07_warp",
                "instructionTargetSiteCounts": {
                    "ResetCurrentMap": 0,
                    "LoadMapTilesets": 0,
                    "LoadMap": 0,
                    "EnableDisplayAndInterrupts": 0,
                },
                "effectiveTargetSiteCounts": {
                    "ResetCurrentMap": 0,
                    "LoadMapTilesets": 0,
                    "LoadMap": 0,
                    "EnableDisplayAndInterrupts": 0,
                },
            },
            {
                "handler": "csc36_resetMap",
                "instructionTargetSiteCounts": {
                    "ResetCurrentMap": 1,
                    "LoadMapTilesets": 0,
                    "LoadMap": 0,
                    "EnableDisplayAndInterrupts": 0,
                },
                "effectiveTargetSiteCounts": {
                    "ResetCurrentMap": 1,
                    "LoadMapTilesets": 0,
                    "LoadMap": 0,
                    "EnableDisplayAndInterrupts": 0,
                },
            },
            {
                "handler": "csc37_loadMapAndFadeIn",
                "instructionTargetSiteCounts": {
                    "ResetCurrentMap": 0,
                    "LoadMapTilesets": 0,
                    "LoadMap": 0,
                    "EnableDisplayAndInterrupts": 0,
                },
                "effectiveTargetSiteCounts": {
                    "ResetCurrentMap": 0,
                    "LoadMapTilesets": 0,
                    "LoadMap": 0,
                    "EnableDisplayAndInterrupts": 0,
                },
            },
            {
                "handler": "csc46_reloadMap",
                "instructionTargetSiteCounts": {
                    "ResetCurrentMap": 0,
                    "LoadMapTilesets": 0,
                    "LoadMap": 1,
                    "EnableDisplayAndInterrupts": 1,
                },
                "effectiveTargetSiteCounts": {
                    "ResetCurrentMap": 0,
                    "LoadMapTilesets": 0,
                    "LoadMap": 1,
                    "EnableDisplayAndInterrupts": 1,
                },
            },
            {
                "handler": "csc48_loadMap",
                "instructionTargetSiteCounts": {
                    "ResetCurrentMap": 0,
                    "LoadMapTilesets": 1,
                    "LoadMap": 1,
                    "EnableDisplayAndInterrupts": 1,
                },
                "effectiveTargetSiteCounts": {
                    "ResetCurrentMap": 0,
                    "LoadMapTilesets": 1,
                    "LoadMap": 1,
                    "EnableDisplayAndInterrupts": 1,
                },
            },
        ],
        "targetResolutions": [
            {
                "instructionTarget": "ResetCurrentMap",
                "effectiveTarget": "ResetCurrentMap",
                "effectiveTargetScope": "external",
            },
            {
                "instructionTarget": "LoadMapTilesets",
                "effectiveTarget": "LoadMapTilesets",
                "effectiveTargetScope": "external",
            },
            {
                "instructionTarget": "LoadMap",
                "effectiveTarget": "LoadMap",
                "effectiveTargetScope": "external",
            },
            {
                "instructionTarget": "EnableDisplayAndInterrupts",
                "effectiveTarget": "EnableDisplayAndInterrupts",
                "effectiveTargetScope": "external",
            },
        ],
        "instructionTargetTotals": {
            "ResetCurrentMap": 1,
            "LoadMapTilesets": 1,
            "LoadMap": 2,
            "EnableDisplayAndInterrupts": 2,
        },
        "effectiveTargetTotals": {
            "ResetCurrentMap": 1,
            "LoadMapTilesets": 1,
            "LoadMap": 2,
            "EnableDisplayAndInterrupts": 2,
        },
        "internalEffectiveTargetTotals": {
            "ResetCurrentMap": 0,
            "LoadMapTilesets": 0,
            "LoadMap": 0,
            "EnableDisplayAndInterrupts": 0,
        },
        "externalEffectiveTargetTotals": {
            "ResetCurrentMap": 1,
            "LoadMapTilesets": 1,
            "LoadMap": 2,
            "EnableDisplayAndInterrupts": 2,
        },
    }
    assert actual["runtimeQuestions"] == [
        "map-script-transition-presentation-matrix"
    ]
    assert actual["canonicalMapDomain"] == {
        "contractId": "sf2-map-content-static-v1",
        "mapCount": 79,
        "mapIds": list(range(79)),
        "sourceMapCurrentValue": 255,
    }
    assert [row["sourceCommandCount"] for row in actual["macros"]] == [38, 7, 60, 24, 17]
    assert len(actual["programTotals"]) == 304
    assert actual["canonicalMapDomain"]["mapCount"] == 79
    validate_json(fixture, fixture_schema, owner="transition fixture")

    missing = deepcopy(map_script_engine_output)
    del missing["transitionCommandFacts"]["sourceSites"][0]["commands"][0][
        "coordinateXValue"
    ]
    with pytest.raises(ValueError, match="coordinateXValue"):
        validate_json(missing, output_schema, owner="transition output missing field")

    renamed = deepcopy(map_script_engine_output)
    command = renamed["transitionCommandFacts"]["sourceSites"][0]["commands"][0]
    command["coordinateX"] = command.pop("coordinateXValue")
    with pytest.raises(ValueError, match="coordinateXValue"):
        validate_json(renamed, output_schema, owner="transition output renamed field")

    extra = deepcopy(map_script_engine_output)
    extra["transitionCommandFacts"]["callerBreakdown"]["callerHandlers"][0][
        "instructionTargetSiteCounts"
    ]["OtherTarget"] = 0
    with pytest.raises(ValueError, match="OtherTarget"):
        validate_json(extra, output_schema, owner="transition output extra target")

    reordered = deepcopy(map_script_engine_output)
    totals = reordered["transitionCommandFacts"]["programTotals"]
    totals[0], totals[1] = totals[1], totals[0]
    with pytest.raises(ValueError):
        validate_json(reordered, output_schema, owner="transition output reordered totals")

    out_of_bounds = deepcopy(map_script_engine_output)
    out_of_bounds["transitionCommandFacts"]["sourceSites"][0]["commands"][0][
        "destinationMapValue"
    ] = 79
    with pytest.raises(ValueError):
        validate_json(out_of_bounds, output_schema, owner="transition output map boundary")

    fixture_missing = deepcopy(fixture)
    del fixture_missing["expected"]["transitionCommandFacts"]["handlers"][0][
        "mapEventTypeValue"
    ]
    with pytest.raises(ValueError):
        validate_json(fixture_missing, fixture_schema, owner="transition fixture missing field")


def test_transition_guards_reject_mutated_source_operand_and_use_site(monkeypatch) -> None:
    original_program_corpus = map_script_engine._program_corpus

    def invalid_map_operand(*args, **kwargs):
        corpus = original_program_corpus(*args, **kwargs)
        for program in corpus["programs"]:
            for command in program["commands"]:
                if command["macro"] == "warp":
                    command["arguments"][0] = "79"
                    return corpus
        raise AssertionError("expected a warp source use site")

    monkeypatch.setattr(map_script_engine, "_program_corpus", invalid_map_operand)
    with pytest.raises(ValueError, match="outside the canonical map domain"):
        build_map_script_engine_contract(
            repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
        )


def test_transition_guards_reject_mutated_service_use_site(monkeypatch) -> None:
    original_statements = map_script_engine._stable_handler_statements

    def changed_scale(disasm, handler):
        statements = original_statements(disasm, handler)
        if handler["name"] == "csc48_loadMap":
            return [statement.replace("mulu.w #3,d0", "mulu.w #2,d0") for statement in statements]
        return statements

    monkeypatch.setattr(map_script_engine, "_stable_handler_statements", changed_scale)
    with pytest.raises(ValueError, match="coordinate selector scale disagreement"):
        build_map_script_engine_contract(
            repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
        )


def test_force_state_contract_matches_complete_golden_and_zero_inclusive_maps(
    map_script_engine_output: dict,
) -> None:
    fixture = load_json(repo_path("tests/fixtures/h2/map-script-engine-static-v1.json"))
    actual = map_script_engine_output["forceStateCommandFacts"]

    assert actual == fixture["expected"]["forceStateCommandFacts"]
    assert [row["sourceCommandCount"] for row in actual["macros"]] == [34, 0, 0, 5, 1, 3]
    assert len(actual["programTotals"]) == 304
    assert actual["callerBreakdown"]["effectiveTargetTotals"] == {
        "FadeOut_WaitForP1Input": 1,
        "GetClass": 1,
        "GetCombatantX": 1,
        "GetCurrentHp": 1,
        "JoinForce": 3,
        "Sleep": 1,
        "WaitForViewScrollEnd": 1,
    }
    assert actual["callerBreakdown"]["internalEffectiveTargetTotals"] == {
        target: 0
        for target in actual["callerBreakdown"]["effectiveTargetTotals"]
    }
    scopes = {
        row["effectiveTarget"]: row["effectiveTargetScope"]
        for row in actual["callerBreakdown"]["targetResolutions"]
    }
    assert scopes == {
        "FadeOut_WaitForP1Input": "external",
        "GetClass": "external",
        "GetCombatantX": "external",
        "GetCurrentHp": "external",
        "JoinForce": "external",
        "Sleep": "external",
        "WaitForViewScrollEnd": "external",
    }
    assert actual["callerBreakdown"]["internalEffectiveTargetTotals"] == {
        target: actual["callerBreakdown"]["effectiveTargetTotals"][target]
        if scopes[target] == "internal"
        else 0
        for target in actual["callerBreakdown"]["effectiveTargetTotals"]
    }
    assert actual["callerBreakdown"]["externalEffectiveTargetTotals"] == {
        target: actual["callerBreakdown"]["effectiveTargetTotals"][target]
        if scopes[target] == "external"
        else 0
        for target in actual["callerBreakdown"]["effectiveTargetTotals"]
    }
    assert actual["commonStatsIdentity"] == {
        "contractId": "sf2-common-stats-static-v1",
        "upstreamCommit": "c834c652b6862bc5679fd7f69a38a7093206efc6",
        "sourcePath": "code/common/stats/battleparty.asm",
        "sourceSha256": "670A25075D807BA60B0AA3C6D158DDF80E5248264753361DBC495F7655ED8B37",
        "services": ["JoinForce", "UpdateForce"],
    }
    assert actual["runtimeQuestions"] == [
        "force-state/roster-death-persistence-visible-outcomes"
    ]


def test_force_state_schemas_reject_nested_mutations_and_boundary_content(
    map_script_engine_output: dict,
) -> None:
    output_schema = repo_path("schemas/map-script-engine-static.schema.json")
    fixture_schema = repo_path("schemas/h2-map-script-engine-static-fixture.schema.json")
    fixture = load_json(repo_path("tests/fixtures/h2/map-script-engine-static-v1.json"))
    validate_json(map_script_engine_output, output_schema, owner="force-state output")
    validate_json(fixture, fixture_schema, owner="force-state fixture")

    missing = deepcopy(map_script_engine_output)
    del missing["forceStateCommandFacts"]["handlers"][0]["sectionGuard"]["branchRecords"][0][
        "branchInstruction"
    ]
    with pytest.raises(ValueError, match="branchInstruction"):
        validate_json(missing, output_schema, owner="force-state output missing field")

    renamed = deepcopy(map_script_engine_output)
    branch = renamed["forceStateCommandFacts"]["handlers"][0]["sectionGuard"]["branchRecords"][0]
    branch["branch"] = branch.pop("branchInstruction")
    with pytest.raises(ValueError, match="branchInstruction"):
        validate_json(renamed, output_schema, owner="force-state output renamed field")

    extra = deepcopy(map_script_engine_output)
    extra["forceStateCommandFacts"]["handlers"][0]["sectionGuard"]["branchRecords"][0][
        "extra"
    ] = True
    with pytest.raises(ValueError, match="extra"):
        validate_json(extra, output_schema, owner="force-state output extra field")

    reordered = deepcopy(map_script_engine_output)
    totals = reordered["forceStateCommandFacts"]["programTotals"]
    totals[0], totals[1] = totals[1], totals[0]
    with pytest.raises(ValueError, match="const"):
        validate_json(reordered, output_schema, owner="force-state output reordered totals")

    out_of_bounds = deepcopy(map_script_engine_output)
    out_of_bounds["forceStateCommandFacts"]["macros"][0]["encodedBytes"] = 3
    with pytest.raises(ValueError, match="const"):
        validate_json(out_of_bounds, output_schema, owner="force-state output boundary")

    wrong_scope = deepcopy(map_script_engine_output)
    wrong_scope["forceStateCommandFacts"]["callerBreakdown"]["targetResolutions"][0][
        "effectiveTargetScope"
    ] = "internal"
    with pytest.raises(ValueError, match="const"):
        validate_json(wrong_scope, output_schema, owner="force-state output wrong scope")

    extra_effective_target = deepcopy(map_script_engine_output)
    extra_effective_target["forceStateCommandFacts"]["callerBreakdown"][
        "effectiveTargetTotals"
    ]["OtherTarget"] = 0
    with pytest.raises(ValueError, match="OtherTarget"):
        validate_json(
            extra_effective_target, output_schema, owner="force-state output extra target"
        )

    fixture_missing = deepcopy(fixture)
    del fixture_missing["expected"]["forceStateCommandFacts"]["handlers"][4]["sectionGuard"]
    with pytest.raises(ValueError, match="sectionGuard"):
        validate_json(fixture_missing, fixture_schema, owner="force-state fixture missing field")


def test_force_state_section_guards_reject_mutated_branch_operands_before_fixture(
    monkeypatch,
) -> None:
    original_statements = map_script_engine._stable_handler_statements

    def changed_use_site(disasm, handler):
        statements = original_statements(disasm, handler)
        if handler["name"] == "csc20_updateDefeatedAllies":
            return [statement.replace("cmpi.w #-1,d1", "cmpi.w #0,d1") for statement in statements]
        return statements

    monkeypatch.setattr(map_script_engine, "_stable_handler_statements", changed_use_site)
    with pytest.raises(ValueError, match="comparison operand drift"):
        build_map_script_engine_contract(
            repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
        )


def test_force_state_section_guard_rejects_branch_polarity_mutation() -> None:
    statements = [
        "move.w (a6)+,d0",
        "jsr j_GetCurrentHp",
        "tst.w d1",
        "bne.w alive",
        "movea.l (a6),a6",
        "bra.s return",
        "addq.w #4,a6",
        "rts",
    ]
    assert _force_state_section_guard("jumpIfDead", statements, {})["branchRecords"][0] == {
        "testInstruction": "tst.w d1",
        "branchInstruction": "bne.w alive",
        "fallthroughInstruction": "movea.l (a6),a6",
        "branchTargetInstruction": "addq.w #4,a6",
    }
    statements[3] = "beq.w alive"
    with pytest.raises(ValueError, match="csc0F_jumpIfCharacterDead statement is missing"):
        _force_state_section_guard("jumpIfDead", statements, {})
