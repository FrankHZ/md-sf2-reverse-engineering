from __future__ import annotations

from copy import deepcopy
from pathlib import Path

import pytest

import sf2tool.h3.story_state as story_state
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path


def _static_contract() -> dict[str, object]:
    return story_state.build_story_state_static_contract(repo_path("local/upstream/SF2DISASM"))


def _expected_flag_storage(
    flag_index_input_word: int, initial_flag_set: bool, final_flag_set: bool
) -> dict[str, object]:
    effective = flag_index_input_word & 1023
    byte_offset = effective // 8
    return {
        "flagIndexInputWord": flag_index_input_word,
        "effectiveFlagIndex": effective,
        "flagByteOffset": byte_offset,
        "flagStorageAddress": 16774790 + byte_offset,
        "flagBitMask": 128 >> (effective % 8),
        "initialFlagSet": initial_flag_set,
        "finalFlagSet": final_flag_set,
        "derivedFromFlagStorageInstructions": [
            "andi.l #FLAG_MASK,d1",
            "divu.w #8,d1",
            "lea ((GAME_FLAGS-$1000000)).w,a0",
            "adda.w d1,a0",
            "swap d1",
            "moveq #$FFFFFF80,d0",
            "lsr.b d1,d0",
        ],
    }


def test_story_state_static_bridge_is_complete_and_source_bound() -> None:
    actual = _static_contract()
    assert actual == {
        "evidenceLabel": "Confirmed",
        "provenance": {
            "sourceFixturePath": "tests/fixtures/h2/map-script-engine-static-v1.json",
            "sourceFixtureId": "sf2-map-script-engine-static-v1",
            "sourceFixtureField": "expected.storyStateCommandFacts",
            "reproductionCommand": "uv run sf2 h2 map-script-engine",
            "sourcePath": "code/common/scripting/map/mapscriptengine_2.asm",
            "h1ListingPath": "build/sf2build-h1.lst",
            "upstream": {
                "repository": "https://github.com/ShiningForceCentral/SF2DISASM.git",
                "commit": "c834c652b6862bc5679fd7f69a38a7093206efc6",
                "branch": "master",
            },
        },
        "sourceForms": [
            {
                "name": "jumpIfFlagSet",
                "opcode": 12,
                "encodedBytes": 8,
                "aliasOf": None,
                "handler": "csc0C_jumpIfFlagSet",
                "sourceCommandCount": 24,
                "selectorValue": None,
            },
            {
                "name": "jumpIfFlagClear",
                "opcode": 13,
                "encodedBytes": 8,
                "aliasOf": None,
                "handler": "csc0D_jumpIfFlagClear",
                "sourceCommandCount": 27,
                "selectorValue": None,
            },
            {
                "name": "csc10",
                "opcode": 16,
                "encodedBytes": 6,
                "aliasOf": None,
                "handler": "csc10_toggleFlag",
                "sourceCommandCount": 0,
                "selectorValue": None,
            },
            {
                "name": "setF",
                "opcode": 16,
                "encodedBytes": 6,
                "aliasOf": "csc10",
                "handler": "csc10_toggleFlag",
                "sourceCommandCount": 37,
                "selectorValue": 65535,
            },
            {
                "name": "clearF",
                "opcode": 16,
                "encodedBytes": 6,
                "aliasOf": "csc10",
                "handler": "csc10_toggleFlag",
                "sourceCommandCount": 16,
                "selectorValue": 0,
            },
            {
                "name": "yesNo",
                "opcode": 17,
                "encodedBytes": 2,
                "aliasOf": None,
                "handler": "csc11_promptYesNoForStoryFlow",
                "sourceCommandCount": 22,
                "selectorValue": None,
            },
            {
                "name": "setStoryFlag",
                "opcode": 19,
                "encodedBytes": 4,
                "aliasOf": None,
                "handler": "csc13_setStoryFlag",
                "sourceCommandCount": 20,
                "selectorValue": None,
            },
        ],
        "handlers": [
            {
                "handler": "csc0C_jumpIfFlagSet",
                "h1Address": 291864,
                "instructions": [
                    {"instruction": "move.w (a6)+,d1", "sourceLine": 450},
                    {"instruction": "jsr j_CheckFlag", "sourceLine": 451},
                    {"instruction": "beq.w loc_47428", "sourceLine": 452},
                    {"instruction": "movea.l (a6),a6", "sourceLine": 453},
                    {"instruction": "bra.s return_4742A", "sourceLine": 454},
                    {"instruction": "addq.w #4,a6", "sourceLine": 457},
                    {"instruction": "rts", "sourceLine": 460},
                ],
                "cursorUseSites": [
                    {
                        "id": "csc0C_jumpIfFlagSet:a6:0",
                        "kind": "read",
                        "sourceRegister": "a6",
                        "destinationOperand": "d1",
                        "transferredByteCount": 2,
                        "cursorAdvanceByteCount": 2,
                        "instruction": "move.w (a6)+,d1",
                        "sourceLine": 450,
                    },
                    {
                        "id": "csc0C_jumpIfFlagSet:a6:1",
                        "kind": "targetRead",
                        "sourceRegister": "a6",
                        "destinationOperand": "a6",
                        "transferredByteCount": 4,
                        "cursorAdvanceByteCount": 0,
                        "instruction": "movea.l (a6),a6",
                        "sourceLine": 453,
                    },
                    {
                        "id": "csc0C_jumpIfFlagSet:a6:2",
                        "kind": "skip",
                        "sourceRegister": "a6",
                        "destinationOperand": "a6",
                        "transferredByteCount": 0,
                        "cursorAdvanceByteCount": 4,
                        "instruction": "addq.w #4,a6",
                        "sourceLine": 457,
                    },
                ],
                "branchPlan": {
                    "kind": "conditionalTarget",
                    "checkFlagCallInstruction": "jsr j_CheckFlag",
                    "branchInstruction": "beq.w loc_47428",
                    "branchOpcode": "beq",
                    "branchTargetLabel": "loc_47428",
                    "flagReadUseSiteId": "csc0C_jumpIfFlagSet:a6:0",
                    "targetReadUseSiteId": "csc0C_jumpIfFlagSet:a6:1",
                    "skipUseSiteId": "csc0C_jumpIfFlagSet:a6:2",
                },
                "directCallPlan": [
                    {
                        "opcode": "jsr",
                        "instructionTarget": "j_CheckFlag",
                        "sourceLine": 451,
                        "instruction": "jsr j_CheckFlag",
                        "h1Address": 291866,
                    }
                ],
            },
            {
                "handler": "csc0D_jumpIfFlagClear",
                "h1Address": 291884,
                "instructions": [
                    {"instruction": "move.w (a6)+,d1", "sourceLine": 472},
                    {"instruction": "jsr j_CheckFlag", "sourceLine": 473},
                    {"instruction": "bne.w loc_4743C", "sourceLine": 474},
                    {"instruction": "movea.l (a6),a6", "sourceLine": 475},
                    {"instruction": "bra.s return_4743E", "sourceLine": 476},
                    {"instruction": "addq.w #4,a6", "sourceLine": 479},
                    {"instruction": "rts", "sourceLine": 482},
                ],
                "cursorUseSites": [
                    {
                        "id": "csc0D_jumpIfFlagClear:a6:0",
                        "kind": "read",
                        "sourceRegister": "a6",
                        "destinationOperand": "d1",
                        "transferredByteCount": 2,
                        "cursorAdvanceByteCount": 2,
                        "instruction": "move.w (a6)+,d1",
                        "sourceLine": 472,
                    },
                    {
                        "id": "csc0D_jumpIfFlagClear:a6:1",
                        "kind": "targetRead",
                        "sourceRegister": "a6",
                        "destinationOperand": "a6",
                        "transferredByteCount": 4,
                        "cursorAdvanceByteCount": 0,
                        "instruction": "movea.l (a6),a6",
                        "sourceLine": 475,
                    },
                    {
                        "id": "csc0D_jumpIfFlagClear:a6:2",
                        "kind": "skip",
                        "sourceRegister": "a6",
                        "destinationOperand": "a6",
                        "transferredByteCount": 0,
                        "cursorAdvanceByteCount": 4,
                        "instruction": "addq.w #4,a6",
                        "sourceLine": 479,
                    },
                ],
                "branchPlan": {
                    "kind": "conditionalTarget",
                    "checkFlagCallInstruction": "jsr j_CheckFlag",
                    "branchInstruction": "bne.w loc_4743C",
                    "branchOpcode": "bne",
                    "branchTargetLabel": "loc_4743C",
                    "flagReadUseSiteId": "csc0D_jumpIfFlagClear:a6:0",
                    "targetReadUseSiteId": "csc0D_jumpIfFlagClear:a6:1",
                    "skipUseSiteId": "csc0D_jumpIfFlagClear:a6:2",
                },
                "directCallPlan": [
                    {
                        "opcode": "jsr",
                        "instructionTarget": "j_CheckFlag",
                        "sourceLine": 473,
                        "instruction": "jsr j_CheckFlag",
                        "h1Address": 291886,
                    }
                ],
            },
            {
                "handler": "csc10_toggleFlag",
                "h1Address": 291962,
                "instructions": [
                    {"instruction": "move.w (a6)+,d1", "sourceLine": 544},
                    {"instruction": "move.w (a6)+,d0", "sourceLine": 545},
                    {"instruction": "bne.s loc_47488", "sourceLine": 546},
                    {"instruction": "jsr j_ClearFlag", "sourceLine": 547},
                    {"instruction": "bra.s return_4748E", "sourceLine": 548},
                    {"instruction": "jsr j_SetFlag", "sourceLine": 551},
                    {"instruction": "rts", "sourceLine": 554},
                ],
                "cursorUseSites": [
                    {
                        "id": "csc10_toggleFlag:a6:0",
                        "kind": "read",
                        "sourceRegister": "a6",
                        "destinationOperand": "d1",
                        "transferredByteCount": 2,
                        "cursorAdvanceByteCount": 2,
                        "instruction": "move.w (a6)+,d1",
                        "sourceLine": 544,
                    },
                    {
                        "id": "csc10_toggleFlag:a6:1",
                        "kind": "read",
                        "sourceRegister": "a6",
                        "destinationOperand": "d0",
                        "transferredByteCount": 2,
                        "cursorAdvanceByteCount": 2,
                        "instruction": "move.w (a6)+,d0",
                        "sourceLine": 545,
                    },
                ],
                "branchPlan": {
                    "kind": "selectorMutation",
                    "branchInstruction": "bne.s loc_47488",
                    "flagReadUseSiteId": "csc10_toggleFlag:a6:0",
                    "selectorReadUseSiteId": "csc10_toggleFlag:a6:1",
                    "zeroResultInstructionTarget": "j_ClearFlag",
                    "nonzeroResultInstructionTarget": "j_SetFlag",
                },
                "directCallPlan": [
                    {
                        "opcode": "jsr",
                        "instructionTarget": "j_ClearFlag",
                        "sourceLine": 547,
                        "instruction": "jsr j_ClearFlag",
                        "h1Address": 291968,
                    },
                    {
                        "opcode": "jsr",
                        "instructionTarget": "j_SetFlag",
                        "sourceLine": 551,
                        "instruction": "jsr j_SetFlag",
                        "h1Address": 291976,
                    },
                ],
            },
            {
                "handler": "csc11_promptYesNoForStoryFlow",
                "h1Address": 291984,
                "instructions": [
                    {"instruction": "move.l a6,-(sp)", "sourceLine": 564},
                    {"instruction": "jsr j_YesNoPrompt", "sourceLine": 565},
                    {"instruction": "movea.l (sp)+,a6", "sourceLine": 566},
                    {"instruction": "moveq #FLAG_INDEX_YES_NO_PROMPT,d1", "sourceLine": 567},
                    {"instruction": "tst.w d0", "sourceLine": 568},
                    {"instruction": "bne.s loc_474A8", "sourceLine": 569},
                    {"instruction": "jsr j_SetFlag", "sourceLine": 570},
                    {"instruction": "bra.s loc_474AE", "sourceLine": 571},
                    {"instruction": "jsr j_ClearFlag", "sourceLine": 574},
                    {"instruction": "moveq #10,d0", "sourceLine": 577},
                    {"instruction": "jsr (Sleep).w", "sourceLine": 578},
                    {"instruction": "rts", "sourceLine": 579},
                ],
                "cursorUseSites": [],
                "branchPlan": {
                    "kind": "promptResultMutation",
                    "promptCallInstruction": "jsr j_YesNoPrompt",
                    "cursorSaveInstruction": "move.l a6,-(sp)",
                    "cursorRestoreInstruction": "movea.l (sp)+,a6",
                    "branchInstruction": "bne.s loc_474A8",
                    "zeroResultInstructionTarget": "j_SetFlag",
                    "nonzeroResultInstructionTarget": "j_ClearFlag",
                    "sleepValueInstruction": "moveq #10,d0",
                    "sleepCallInstruction": "jsr (Sleep).w",
                },
                "directCallPlan": [
                    {
                        "opcode": "jsr",
                        "instructionTarget": "j_YesNoPrompt",
                        "sourceLine": 565,
                        "instruction": "jsr j_YesNoPrompt",
                        "h1Address": 291986,
                    },
                    {
                        "opcode": "jsr",
                        "instructionTarget": "j_SetFlag",
                        "sourceLine": 570,
                        "instruction": "jsr j_SetFlag",
                        "h1Address": 292000,
                    },
                    {
                        "opcode": "jsr",
                        "instructionTarget": "j_ClearFlag",
                        "sourceLine": 574,
                        "instruction": "jsr j_ClearFlag",
                        "h1Address": 292008,
                    },
                    {
                        "opcode": "jsr",
                        "instructionTarget": "Sleep",
                        "sourceLine": 578,
                        "instruction": "jsr (Sleep).w",
                        "h1Address": 292016,
                    },
                ],
            },
            {
                "handler": "csc13_setStoryFlag",
                "h1Address": 292064,
                "instructions": [
                    {"instruction": "move.w (a6)+,d1", "sourceLine": 619},
                    {"instruction": "addi.w #BATTLE_UNLOCKED_FLAGS_START,d1", "sourceLine": 620},
                    {"instruction": "jsr j_SetFlag", "sourceLine": 621},
                    {"instruction": "rts", "sourceLine": 622},
                ],
                "cursorUseSites": [
                    {
                        "id": "csc13_setStoryFlag:a6:0",
                        "kind": "read",
                        "sourceRegister": "a6",
                        "destinationOperand": "d1",
                        "transferredByteCount": 2,
                        "cursorAdvanceByteCount": 2,
                        "instruction": "move.w (a6)+,d1",
                        "sourceLine": 619,
                    }
                ],
                "branchPlan": {
                    "kind": "wordAddMutation",
                    "battleReadUseSiteId": "csc13_setStoryFlag:a6:0",
                    "addInstruction": "addi.w #BATTLE_UNLOCKED_FLAGS_START,d1",
                    "setFlagInstruction": "jsr j_SetFlag",
                    "wordWidthBytes": 2,
                },
                "directCallPlan": [
                    {
                        "opcode": "jsr",
                        "instructionTarget": "j_SetFlag",
                        "sourceLine": 621,
                        "instruction": "jsr j_SetFlag",
                        "h1Address": 292070,
                    }
                ],
            },
        ],
        "constants": {
            "evidenceLabel": "Confirmed",
            "FLAG_INDEX_YES_NO_PROMPT": {
                "value": 89,
                "handler": "csc11_promptYesNoForStoryFlow",
                "instruction": "moveq #FLAG_INDEX_YES_NO_PROMPT,d1",
            },
            "BATTLE_UNLOCKED_FLAGS_START": {
                "value": 400,
                "handler": "csc13_setStoryFlag",
                "instruction": "addi.w #BATTLE_UNLOCKED_FLAGS_START,d1",
            },
        },
        "flagStorage": {
            "evidenceLabel": "Confirmed",
            "GAME_FLAGS": {
                "value": 16774790,
                "sourcePath": "code/common/stats/gameflags.asm",
                "useSite": {"instruction": "lea ((GAME_FLAGS-$1000000)).w,a0", "sourceLine": 57},
            },
            "FLAG_MASK": {
                "value": 1023,
                "sourcePath": "code/common/stats/gameflags.asm",
                "useSite": {"instruction": "andi.l #FLAG_MASK,d1", "sourceLine": 55},
            },
            "addressing": {
                "inputWordMaskInstruction": "andi.l #FLAG_MASK,d1",
                "inputWordMask": 1023,
                "byteDivisorInstruction": "divu.w #8,d1",
                "byteDivisor": 8,
                "addressableByteSpan": 128,
                "addressableByteSpanDerivedFrom": [
                    "andi.l #FLAG_MASK,d1",
                    "divu.w #8,d1",
                ],
                "baseInstruction": "lea ((GAME_FLAGS-$1000000)).w,a0",
                "byteOffsetAddInstruction": "adda.w d1,a0",
                "remainderInstruction": "swap d1",
                "msbMaskSeedInstruction": "moveq #$FFFFFF80,d0",
                "msbMaskSeed": 128,
                "bitShiftInstruction": "lsr.b d1,d0",
            },
        },
        "callerBreakdown": {
            "callerHandlers": [
                {
                    "handler": "csc0C_jumpIfFlagSet",
                    "instructionTargetSiteCounts": {
                        "Sleep": 0,
                        "j_CheckFlag": 1,
                        "j_ClearFlag": 0,
                        "j_SetFlag": 0,
                        "j_YesNoPrompt": 0,
                    },
                    "effectiveTargetSiteCounts": {
                        "CheckFlag": 1,
                        "ClearFlag": 0,
                        "SetFlag": 0,
                        "Sleep": 0,
                        "YesNoPrompt": 0,
                    },
                },
                {
                    "handler": "csc0D_jumpIfFlagClear",
                    "instructionTargetSiteCounts": {
                        "Sleep": 0,
                        "j_CheckFlag": 1,
                        "j_ClearFlag": 0,
                        "j_SetFlag": 0,
                        "j_YesNoPrompt": 0,
                    },
                    "effectiveTargetSiteCounts": {
                        "CheckFlag": 1,
                        "ClearFlag": 0,
                        "SetFlag": 0,
                        "Sleep": 0,
                        "YesNoPrompt": 0,
                    },
                },
                {
                    "handler": "csc10_toggleFlag",
                    "instructionTargetSiteCounts": {
                        "Sleep": 0,
                        "j_CheckFlag": 0,
                        "j_ClearFlag": 1,
                        "j_SetFlag": 1,
                        "j_YesNoPrompt": 0,
                    },
                    "effectiveTargetSiteCounts": {
                        "CheckFlag": 0,
                        "ClearFlag": 1,
                        "SetFlag": 1,
                        "Sleep": 0,
                        "YesNoPrompt": 0,
                    },
                },
                {
                    "handler": "csc11_promptYesNoForStoryFlow",
                    "instructionTargetSiteCounts": {
                        "Sleep": 1,
                        "j_CheckFlag": 0,
                        "j_ClearFlag": 1,
                        "j_SetFlag": 1,
                        "j_YesNoPrompt": 1,
                    },
                    "effectiveTargetSiteCounts": {
                        "CheckFlag": 0,
                        "ClearFlag": 1,
                        "SetFlag": 1,
                        "Sleep": 1,
                        "YesNoPrompt": 1,
                    },
                },
                {
                    "handler": "csc13_setStoryFlag",
                    "instructionTargetSiteCounts": {
                        "Sleep": 0,
                        "j_CheckFlag": 0,
                        "j_ClearFlag": 0,
                        "j_SetFlag": 1,
                        "j_YesNoPrompt": 0,
                    },
                    "effectiveTargetSiteCounts": {
                        "CheckFlag": 0,
                        "ClearFlag": 0,
                        "SetFlag": 1,
                        "Sleep": 0,
                        "YesNoPrompt": 0,
                    },
                },
            ],
            "targetResolutions": [
                {
                    "instructionTarget": "Sleep",
                    "effectiveTarget": "Sleep",
                    "aliasSourcePath": None,
                    "effectiveTargetScope": "external",
                },
                {
                    "instructionTarget": "j_CheckFlag",
                    "effectiveTarget": "CheckFlag",
                    "aliasSourcePath": "code/common/tech/jumpinterfaces/s02_jumpinterface.asm",
                    "effectiveTargetScope": "external",
                },
                {
                    "instructionTarget": "j_ClearFlag",
                    "effectiveTarget": "ClearFlag",
                    "aliasSourcePath": "code/common/tech/jumpinterfaces/s02_jumpinterface.asm",
                    "effectiveTargetScope": "external",
                },
                {
                    "instructionTarget": "j_SetFlag",
                    "effectiveTarget": "SetFlag",
                    "aliasSourcePath": "code/common/tech/jumpinterfaces/s02_jumpinterface.asm",
                    "effectiveTargetScope": "external",
                },
                {
                    "instructionTarget": "j_YesNoPrompt",
                    "effectiveTarget": "YesNoPrompt",
                    "aliasSourcePath": "code/common/tech/jumpinterfaces/s03_jumpinterface_1.asm",
                    "effectiveTargetScope": "external",
                },
            ],
            "instructionTargetTotals": {
                "Sleep": 1,
                "j_CheckFlag": 2,
                "j_ClearFlag": 2,
                "j_SetFlag": 3,
                "j_YesNoPrompt": 1,
            },
            "effectiveTargetTotals": {
                "CheckFlag": 2,
                "ClearFlag": 2,
                "SetFlag": 3,
                "Sleep": 1,
                "YesNoPrompt": 1,
            },
            "internalEffectiveTargetTotals": {
                "CheckFlag": 0,
                "ClearFlag": 0,
                "SetFlag": 0,
                "Sleep": 0,
                "YesNoPrompt": 0,
            },
            "externalEffectiveTargetTotals": {
                "CheckFlag": 2,
                "ClearFlag": 2,
                "SetFlag": 3,
                "Sleep": 1,
                "YesNoPrompt": 1,
            },
        },
        "runtimeQuestionQueue": [
            {"id": "story-state/normal-story-reachability", "evidenceLabel": "Unknown"},
            {"id": "story-state/save-load-lifecycle-persistence", "evidenceLabel": "Unknown"},
            {
                "id": "story-state/player-visible-yes-no-presentation-timing",
                "evidenceLabel": "Unknown",
            },
        ],
    }


def test_story_state_case_matrix_derives_all_ten_records() -> None:
    actual = story_state.derive_story_state_case_matrix(
        _static_contract(), input_cursor_offset_bytes=7
    )
    assert actual == [
        {
            "id": "jump-if-flag-set-zero-skip",
            "expected": {
                "handler": "csc0C_jumpIfFlagSet",
                "h1Address": 291864,
                "targetInputValue": 1193046,
                "flagStorage": _expected_flag_storage(8, False, False),
                "checkFlagResultZero": True,
                "checkFlagInstructionTarget": "j_CheckFlag",
                "checkFlagEffectiveTarget": "CheckFlag",
                "cursor": {
                    "kind": "inputOffset",
                    "value": 13,
                    "skipUseSiteId": "csc0C_jumpIfFlagSet:a6:2",
                },
                "derivedFromUseSiteIds": [
                    "csc0C_jumpIfFlagSet:a6:0",
                    "csc0C_jumpIfFlagSet:a6:1",
                    "csc0C_jumpIfFlagSet:a6:2",
                ],
            },
        },
        {
            "id": "jump-if-flag-set-nonzero-target",
            "expected": {
                "handler": "csc0C_jumpIfFlagSet",
                "h1Address": 291864,
                "targetInputValue": 1193046,
                "flagStorage": _expected_flag_storage(8, True, True),
                "checkFlagResultZero": False,
                "checkFlagInstructionTarget": "j_CheckFlag",
                "checkFlagEffectiveTarget": "CheckFlag",
                "cursor": {
                    "kind": "targetValue",
                    "value": 1193046,
                    "inputOffsetBeforeTargetRead": 9,
                    "targetReadUseSiteId": "csc0C_jumpIfFlagSet:a6:1",
                },
                "derivedFromUseSiteIds": [
                    "csc0C_jumpIfFlagSet:a6:0",
                    "csc0C_jumpIfFlagSet:a6:1",
                    "csc0C_jumpIfFlagSet:a6:2",
                ],
            },
        },
        {
            "id": "jump-if-flag-clear-zero-target",
            "expected": {
                "handler": "csc0D_jumpIfFlagClear",
                "h1Address": 291884,
                "targetInputValue": 6636321,
                "flagStorage": _expected_flag_storage(71, False, False),
                "checkFlagResultZero": True,
                "checkFlagInstructionTarget": "j_CheckFlag",
                "checkFlagEffectiveTarget": "CheckFlag",
                "cursor": {
                    "kind": "targetValue",
                    "value": 6636321,
                    "inputOffsetBeforeTargetRead": 9,
                    "targetReadUseSiteId": "csc0D_jumpIfFlagClear:a6:1",
                },
                "derivedFromUseSiteIds": [
                    "csc0D_jumpIfFlagClear:a6:0",
                    "csc0D_jumpIfFlagClear:a6:1",
                    "csc0D_jumpIfFlagClear:a6:2",
                ],
            },
        },
        {
            "id": "jump-if-flag-clear-nonzero-skip",
            "expected": {
                "handler": "csc0D_jumpIfFlagClear",
                "h1Address": 291884,
                "targetInputValue": 6636321,
                "flagStorage": _expected_flag_storage(71, True, True),
                "checkFlagResultZero": False,
                "checkFlagInstructionTarget": "j_CheckFlag",
                "checkFlagEffectiveTarget": "CheckFlag",
                "cursor": {
                    "kind": "inputOffset",
                    "value": 13,
                    "skipUseSiteId": "csc0D_jumpIfFlagClear:a6:2",
                },
                "derivedFromUseSiteIds": [
                    "csc0D_jumpIfFlagClear:a6:0",
                    "csc0D_jumpIfFlagClear:a6:1",
                    "csc0D_jumpIfFlagClear:a6:2",
                ],
            },
        },
        {
            "id": "set-f-nonzero-selector",
            "expected": {
                "handler": "csc10_toggleFlag",
                "h1Address": 291962,
                "sourceForm": "setF",
                "flagIndexInput": 31,
                "selectorInput": 65535,
                "expectedInstructionTarget": "j_SetFlag",
                "expectedEffectiveTarget": "SetFlag",
                "flagStorage": _expected_flag_storage(31, False, True),
                "cursorOutputOffsetBytes": 11,
                "derivedFromUseSiteIds": ["csc10_toggleFlag:a6:0", "csc10_toggleFlag:a6:1"],
            },
        },
        {
            "id": "clear-f-zero-selector",
            "expected": {
                "handler": "csc10_toggleFlag",
                "h1Address": 291962,
                "sourceForm": "clearF",
                "flagIndexInput": 32,
                "selectorInput": 0,
                "expectedInstructionTarget": "j_ClearFlag",
                "expectedEffectiveTarget": "ClearFlag",
                "flagStorage": _expected_flag_storage(32, True, False),
                "cursorOutputOffsetBytes": 11,
                "derivedFromUseSiteIds": ["csc10_toggleFlag:a6:0", "csc10_toggleFlag:a6:1"],
            },
        },
        {
            "id": "yes-no-zero-set",
            "expected": {
                "handler": "csc11_promptYesNoForStoryFlow",
                "h1Address": 291984,
                "promptResultZero": True,
                "flagStorage": _expected_flag_storage(89, False, True),
                "expectedInstructionTarget": "j_SetFlag",
                "expectedEffectiveTarget": "SetFlag",
                "sleepInputValue": 10,
                "cursorOutputOffsetBytes": 7,
                "derivedFromInstructions": [
                    "move.l a6,-(sp)",
                    "movea.l (sp)+,a6",
                    "bne.s loc_474A8",
                    "moveq #10,d0",
                    "jsr (Sleep).w",
                ],
            },
        },
        {
            "id": "yes-no-nonzero-clear",
            "expected": {
                "handler": "csc11_promptYesNoForStoryFlow",
                "h1Address": 291984,
                "promptResultZero": False,
                "flagStorage": _expected_flag_storage(89, True, False),
                "expectedInstructionTarget": "j_ClearFlag",
                "expectedEffectiveTarget": "ClearFlag",
                "sleepInputValue": 10,
                "cursorOutputOffsetBytes": 7,
                "derivedFromInstructions": [
                    "move.l a6,-(sp)",
                    "movea.l (sp)+,a6",
                    "bne.s loc_474A8",
                    "moveq #10,d0",
                    "jsr (Sleep).w",
                ],
            },
        },
        {
            "id": "set-story-flag-base",
            "expected": {
                "handler": "csc13_setStoryFlag",
                "h1Address": 292064,
                "battleInputWord": 0,
                "resultFlagIndexWord": 400,
                "expectedInstructionTarget": "j_SetFlag",
                "expectedEffectiveTarget": "SetFlag",
                "flagStorage": _expected_flag_storage(400, False, True),
                "cursorOutputOffsetBytes": 9,
                "derivedFromUseSiteIds": ["csc13_setStoryFlag:a6:0"],
                "derivedFromInstruction": "addi.w #BATTLE_UNLOCKED_FLAGS_START,d1",
            },
        },
        {
            "id": "set-story-flag-word-wrap-boundary",
            "expected": {
                "handler": "csc13_setStoryFlag",
                "h1Address": 292064,
                "battleInputWord": 65136,
                "resultFlagIndexWord": 0,
                "expectedInstructionTarget": "j_SetFlag",
                "expectedEffectiveTarget": "SetFlag",
                "flagStorage": _expected_flag_storage(0, False, True),
                "cursorOutputOffsetBytes": 9,
                "derivedFromUseSiteIds": ["csc13_setStoryFlag:a6:0"],
                "derivedFromInstruction": "addi.w #BATTLE_UNLOCKED_FLAGS_START,d1",
            },
        },
    ]


def test_story_state_cursor_derives_from_use_sites_not_command_width_shortcut() -> None:
    static = _static_contract()
    changed = deepcopy(static)
    handler = next(row for row in changed["handlers"] if row["handler"] == "csc0C_jumpIfFlagSet")
    handler["cursorUseSites"][2]["cursorAdvanceByteCount"] = 6
    matrix = story_state.derive_story_state_case_matrix(changed, input_cursor_offset_bytes=3)
    assert matrix[0]["expected"]["cursor"] == {
        "kind": "inputOffset",
        "value": 11,
        "skipUseSiteId": "csc0C_jumpIfFlagSet:a6:2",
    }
    handler["cursorUseSites"][0]["id"] = "missing"
    with pytest.raises(ValueError, match="cursor use site is missing"):
        story_state.derive_story_state_case_matrix(changed)


@pytest.mark.parametrize(
    ("symbol", "before", "after"),
    [
        ("csc0C_jumpIfFlagSet", "beq.w   loc_47428", "bne.w   loc_47428"),
        (
            "csc0C_jumpIfFlagSet",
            (
                "movea.l (a6),a6\n                bra.s   return_4742A\n"
                "loc_47428:\n                \n                addq.w  #4,a6"
            ),
            (
                "addq.w  #4,a6\n                bra.s   return_4742A\n"
                "loc_47428:\n                \n                movea.l (a6),a6"
            ),
        ),
        ("csc10_toggleFlag", "bne.s   loc_47488", "beq.s   loc_47488"),
        ("csc10_toggleFlag", "jsr     j_ClearFlag", "jsr     j_SetFlag"),
        ("csc11_promptYesNoForStoryFlow", "bne.s   loc_474A8", "beq.s   loc_474A8"),
        (
            "csc11_promptYesNoForStoryFlow",
            "moveq   #10,d0\n                jsr     (Sleep).w",
            "jsr     (Sleep).w\n                moveq   #10,d0",
        ),
        (
            "csc13_setStoryFlag",
            "addi.w  #BATTLE_UNLOCKED_FLAGS_START,d1",
            "subi.w  #BATTLE_UNLOCKED_FLAGS_START,d1",
        ),
        ("csc0D_jumpIfFlagClear", "move.w  (a6)+,d1", "move.b  (a6)+,d1"),
        ("csc0D_jumpIfFlagClear", "movea.l (a6),a6", "movea.l (a6)+,a6"),
    ],
)
def test_story_state_source_mutations_fail_before_matrix_derivation(
    monkeypatch: pytest.MonkeyPatch, symbol: str, before: str, after: str
) -> None:
    upstream = repo_path("local/upstream/SF2DISASM").resolve()
    source_path = (upstream / story_state.MAP_SCRIPT_SOURCE_PATH).resolve()
    original = Path.read_text

    def altered(path: Path, *args: object, **kwargs: object) -> str:
        source = original(path, *args, **kwargs)
        if path.resolve() != source_path:
            return source
        start = source.index(f"{symbol}:")
        end = source.index(f"; End of function {symbol}", start)
        section = source[start:end]
        changed_section = section.replace(before, after, 1)
        if changed_section == section:
            raise AssertionError(f"story-state source mutation target drift: {before}")
        return source[:start] + changed_section + source[end:]

    monkeypatch.setattr(Path, "read_text", altered)
    with pytest.raises(ValueError, match="story-state"):
        story_state.build_story_state_static_contract(upstream)


def test_story_state_direct_call_parser_handles_suffixes_comments_and_near_misses() -> None:
    rows = [
        {"instruction": "jsr j_SetFlag", "sourceLine": 1},
        {"instruction": "jsr.w j_ClearFlag", "sourceLine": 2},
        {"instruction": "bsr.s j_CheckFlag", "sourceLine": 3},
        {"instruction": "bsr.l j_YesNoPrompt", "sourceLine": 4},
        {"instruction": "jsr (Sleep).w", "sourceLine": 5},
        {"instruction": "move.w j_SetFlag,d0", "sourceLine": 6},
        {"instruction": "nearjsr j_ClearFlag", "sourceLine": 7},
        {"instruction": "jsr_label:", "sourceLine": 8},
        {"instruction": "jsr j_SetFlag ; comment", "sourceLine": 9},
    ]
    assert story_state._direct_calls(rows) == [
        {
            "opcode": "jsr",
            "instructionTarget": "j_SetFlag",
            "sourceLine": 1,
            "instruction": "jsr j_SetFlag",
        },
        {
            "opcode": "jsr",
            "instructionTarget": "j_ClearFlag",
            "sourceLine": 2,
            "instruction": "jsr.w j_ClearFlag",
        },
        {
            "opcode": "bsr",
            "instructionTarget": "j_CheckFlag",
            "sourceLine": 3,
            "instruction": "bsr.s j_CheckFlag",
        },
        {
            "opcode": "bsr",
            "instructionTarget": "j_YesNoPrompt",
            "sourceLine": 4,
            "instruction": "bsr.l j_YesNoPrompt",
        },
        {
            "opcode": "jsr",
            "instructionTarget": "Sleep",
            "sourceLine": 5,
            "instruction": "jsr (Sleep).w",
        },
    ]
    section = "\n".join(
        ("demo:", "    jsr (Sleep).w ; parsed call", "    rts", "; End of function demo")
    )
    assert story_state._direct_calls(story_state._source_section(section, "demo")) == [
        {
            "opcode": "jsr",
            "instructionTarget": "Sleep",
            "sourceLine": 2,
            "instruction": "jsr (Sleep).w",
        }
    ]


def test_story_state_schemas_reject_nested_mutations_and_exact_case_order() -> None:
    fixture = load_json(repo_path("tests/fixtures/h3/story-state-v1.json"))
    fixture_schema = repo_path("schemas/h3-story-state-fixture.schema.json")
    observation_schema = repo_path("schemas/h3-story-state-observation.schema.json")
    validate_json(fixture, fixture_schema, owner="story-state fixture baseline")
    validate_json(
        fixture["observation"], observation_schema, owner="story-state observation baseline"
    )

    missing = deepcopy(fixture)
    del missing["cases"][0]["expected"]["targetInputValue"]
    with pytest.raises(ValueError, match="not valid"):
        validate_json(missing, fixture_schema, owner="story-state fixture missing nested")

    renamed = deepcopy(fixture)
    record = renamed["observation"]["records"][0]
    record["finalFlag"] = record.pop("finalFlagSet")
    with pytest.raises(ValueError, match="finalFlagSet|finalFlag"):
        validate_json(renamed, fixture_schema, owner="story-state fixture renamed nested")

    extra = deepcopy(fixture["observation"])
    extra["records"][0]["chronology"][0]["extra"] = True
    with pytest.raises(ValueError, match="extra"):
        validate_json(extra, observation_schema, owner="story-state observation extra nested")

    reordered = deepcopy(fixture["observation"])
    reordered["records"][0], reordered["records"][1] = (
        reordered["records"][1],
        reordered["records"][0],
    )
    with pytest.raises(ValueError, match="jump-if-flag-set-zero-skip"):
        validate_json(reordered, observation_schema, owner="story-state observation exact order")

    boundary = deepcopy(fixture)
    boundary["cases"][0]["expected"]["targetInputValue"] = 1 << 32
    with pytest.raises(ValueError, match="not valid"):
        validate_json(boundary, fixture_schema, owner="story-state fixture target boundary")

    handler_reordered = deepcopy(fixture)
    records = handler_reordered["runtimeContract"]["handlerRecords"]
    records[0], records[1] = records[1], records[0]
    with pytest.raises(ValueError, match="csc0C_jumpIfFlagSet"):
        validate_json(handler_reordered, fixture_schema, owner="story-state handler exact order")

    duplicate_cursor = deepcopy(fixture)
    cursor_sites = duplicate_cursor["runtimeContract"]["handlerRecords"][0]["cursorUseSites"]
    cursor_sites[1] = deepcopy(cursor_sites[0])
    with pytest.raises(ValueError, match="csc0C_jumpIfFlagSet:a6:1"):
        validate_json(duplicate_cursor, fixture_schema, owner="story-state cursor exact identity")

    reordered_calls = deepcopy(fixture)
    calls = reordered_calls["runtimeContract"]["handlerRecords"][3]["directCalls"]
    calls[0], calls[1] = calls[1], calls[0]
    with pytest.raises(ValueError, match="j_YesNoPrompt"):
        validate_json(reordered_calls, fixture_schema, owner="story-state direct-call exact order")

    nonempty_prompt_cursor = deepcopy(fixture)
    nonempty_prompt_cursor["runtimeContract"]["handlerRecords"][3]["cursorUseSites"].append(
        {"id": "csc11_promptYesNoForStoryFlow:a6:0", "h1Address": 291984}
    )
    with pytest.raises(ValueError, match="expected to be empty"):
        validate_json(nonempty_prompt_cursor, fixture_schema, owner="story-state empty cursor")

    fixture_observation_reordered = deepcopy(fixture)
    fixture_records = fixture_observation_reordered["observation"]["records"]
    fixture_records[0], fixture_records[1] = fixture_records[1], fixture_records[0]
    with pytest.raises(ValueError, match="jump-if-flag-set-zero-skip"):
        validate_json(
            fixture_observation_reordered,
            fixture_schema,
            owner="story-state fixture observation exact order",
        )

    chronology_reordered = deepcopy(fixture["observation"])
    chronology = chronology_reordered["records"][0]["chronology"]
    chronology[0], chronology[1] = chronology[1], chronology[0]
    with pytest.raises(ValueError, match="291864"):
        validate_json(
            chronology_reordered, observation_schema, owner="story-state chronology exact order"
        )

    chronology_identity = deepcopy(fixture["observation"])
    chronology_identity["records"][0]["chronology"][1]["effectiveTarget"] = "SetFlag"
    with pytest.raises(ValueError, match="CheckFlag"):
        validate_json(
            chronology_identity, observation_schema, owner="story-state chronology exact identity"
        )

    null_observation = deepcopy(fixture)
    null_observation["observation"] = None
    with pytest.raises(ValueError, match="not of type 'object'"):
        validate_json(
            null_observation, fixture_schema, owner="story-state null fixture observation"
        )
    with pytest.raises(ValueError, match="not of type 'object'"):
        validate_json(None, observation_schema, owner="story-state null observation")
