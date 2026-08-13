from __future__ import annotations

import json
from copy import deepcopy
from pathlib import Path

import pytest

import sf2tool.h3.story_state as story_state
from sf2tool.h3.bizhawk import bizhawk_contract, validate_lua_syntax
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
        "saveLoadPersistence": {
            "evidenceLabel": "Confirmed",
            "h2Owner": {
                "fixturePath": "tests/fixtures/h2/tech-services-static-v1.json",
                "fixtureId": "sf2-tech-services-static-v1",
                "field": "expected.sramFacts",
                "reproductionCommand": "uv run sf2 h2 tech-services",
            },
            "ramLogicalSpan": {
                "baseAddress": 16771072,
                "logicalByteCount": 4016,
                "gameFlagsAddress": 16774790,
                "gameFlagsOffset": 3718,
            },
            "saveLoadFunctions": {
                "SaveGame": {
                    "h1Address": 28522,
                    "copyCallInstruction": "bsr.w CopyBytesToSram",
                    "copyCallSourceLine": 114,
                    "copyCounterInstruction": "move.w #SAVE_SLOT_REAL_SIZE,d7",
                    "copyCounterSourceLine": 113,
                    "checksumWriteInstruction": "move.b d0,(a2)",
                    "checksumWriteSourceLine": 115,
                    "occupiedFlagInstruction": "bset d1,(SAVE_FLAGS).l",
                    "occupiedFlagSourceLine": 116,
                },
                "LoadGame": {
                    "h1Address": 28588,
                    "copyCallInstruction": "bsr.w CopyBytesFromSram",
                    "copyCallSourceLine": 142,
                    "copyCounterInstruction": "move.w #SAVE_SLOT_REAL_SIZE,d7",
                    "copyCounterSourceLine": 141,
                },
            },
            "slotSelections": [
                {
                    "selector": 0,
                    "slot": "slot1",
                    "selectedDataAddress": 2097329,
                    "selectedChecksumAddress": 2105399,
                    "occupiedFlagBit": 0,
                    "selectedPhysicalByteStride": 2,
                    "selectedPhysicalAddressInterval": 8032,
                    "selectedFlagByteAddress": 2104765,
                },
                {
                    "selector": 1,
                    "slot": "slot2",
                    "selectedDataAddress": 2105403,
                    "selectedChecksumAddress": 2105401,
                    "occupiedFlagBit": 1,
                    "selectedPhysicalByteStride": 2,
                    "selectedPhysicalAddressInterval": 8032,
                    "selectedFlagByteAddress": 2112839,
                },
            ],
            "saveFlagsAddress": 2105397,
            "physicalWindowBaseAddress": 2097152,
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
        "scratchGap": {
            "evidenceLabel": "Confirmed",
            "lowerNeighbor": {
                "symbol": "byte_FF4000",
                "address": 16728064,
                "clearInstruction": "lea (byte_FF4000).l,a0",
                "clearCounterInstruction": "move.w #511,d0",
                "clearLongwordCount": 512,
                "clearByteCount": 2048,
                "clearEndExclusive": 16730112,
                "clearSourceLine": 395,
            },
            "gap": {
                "startAddress": 16730112,
                "endExclusiveAddress": 16730624,
                "byteCount": 512,
            },
            "upperNeighbor": {"symbol": "byte_FF4A00", "address": 16730624},
            "sourceReferenceAudit": {
                "pattern": "FF48xx-or-FF49xx-code-reference",
                "referenceCount": 0,
            },
            "rejectedOwnerRange": {
                "symbol": "MAP_LAYOUT_HISTORY_MAP_SIZES",
                "startAddress": 16736256,
                "endExclusiveAddress": 16738304,
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


def test_story_state_persistence_matrix_is_exact_and_source_derived() -> None:
    static = _static_contract()
    persistence = story_state.derive_story_state_persistence_case_matrix(static)
    fixture = load_json(repo_path("tests/fixtures/h3/story-state-v2.json"))
    assert story_state.persistence_fixture_projection(persistence) == fixture["cases"][10:]
    assert [
        (
            case["id"],
            case["expected"]["selectedSlot"]["selectedDataAddress"],
            case["expected"]["selectedSlot"]["selectedChecksumAddress"],
            case["expected"]["selectedSlot"]["occupiedFlagBit"],
            case["expected"]["stateBytes"],
            case["expected"]["finalCheck"]["handler"],
        )
        for case in persistence
    ] == [
        (
            "csc10-set-slot1-save-load-branch", 2097329, 2105399, 0,
            {"before": 0, "mutated": 1, "poisoned": 0, "restored": 1},
            "csc0C_jumpIfFlagSet",
        ),
        (
            "csc10-clear-slot2-save-load-branch", 2105403, 2105401, 1,
            {"before": 128, "mutated": 0, "poisoned": 128, "restored": 0},
            "csc0D_jumpIfFlagClear",
        ),
        (
            "csc11-flag89-set-slot1-save-load-branch", 2097329, 2105399, 0,
            {"before": 0, "mutated": 64, "poisoned": 0, "restored": 64},
            "csc0C_jumpIfFlagSet",
        ),
        (
            "csc11-flag89-clear-slot2-save-load-branch", 2105403, 2105401, 1,
            {"before": 64, "mutated": 0, "poisoned": 64, "restored": 0},
            "csc0D_jumpIfFlagClear",
        ),
        (
            "csc13-flag400-slot1-save-load-branch", 2097329, 2105399, 0,
            {"before": 0, "mutated": 128, "poisoned": 0, "restored": 128},
            "csc0C_jumpIfFlagSet",
        ),
        (
            "csc13-word-wrap-flag0-slot2-save-load-branch", 2105403, 2105401, 1,
            {"before": 0, "mutated": 128, "poisoned": 0, "restored": 128},
            "csc0C_jumpIfFlagSet",
        ),
    ]
    runtime = story_state._runtime_contract(
        static, repo_path("local/upstream/SF2DISASM")
    )
    expected = story_state.expected_story_state_observation(
        fixture,
        runtime,
        story_state.derive_story_state_case_matrix(static) + persistence,
    )
    assert expected == fixture["observation"]
    assert expected["records"][:10] == fixture["observation"]["records"][:10]
    assert expected["records"][10]["chronology"][0]["role"] == "mutation-handler-entry"
    assert expected["records"][10]["chronology"][-1]["role"] == "final-branch-result"


def test_story_state_retained_v1_projection_rejects_before_runtime_config(
    tmp_path: Path, monkeypatch: pytest.MonkeyPatch
) -> None:
    fixture = load_json(repo_path("tests/fixtures/h3/story-state-v2.json"))
    fixture["cases"][0]["expected"]["targetInputValue"] = 0
    path = tmp_path / "story-state-v2-mutated.json"
    path.write_text(json.dumps(fixture), encoding="utf-8")
    monkeypatch.setattr(story_state, "FIXTURE", path)
    monkeypatch.setattr(
        story_state,
        "verify_runtime_contract",
        lambda *_args, **_kwargs: pytest.fail("retained projection drift reached runtime contract"),
    )
    with pytest.raises(ValueError, match="retained-v1 projection"):
        story_state.verify_story_state(
            Path("missing-rom.bin"), repo_path("local/upstream/SF2DISASM")
        )


@pytest.mark.parametrize(
    ("relative", "before", "after"),
    [
        (story_state.CONSTANT_SOURCE_PATH, "GAME_FLAGS: equ $FFF686", "GAME_FLAGS: equ $FFF685"),
        (
            story_state.ENUM_SOURCE_PATH,
            "SAVE_SLOT_REAL_SIZE: equ 4016",
            "SAVE_SLOT_REAL_SIZE: equ 4015",
        ),
        (story_state.SRAM_SOURCE_PATH, "bsr.w   CopyBytesToSram", "bsr.w   CopyBytesFromSram"),
        (story_state.SRAM_SOURCE_PATH, "bset    d1,(SAVE_FLAGS).l", "bclr    d1,(SAVE_FLAGS).l"),
    ],
)
def test_story_state_persistence_source_and_constant_mutations_fail_before_fixture(
    monkeypatch: pytest.MonkeyPatch, relative: Path, before: str, after: str
) -> None:
    upstream = repo_path("local/upstream/SF2DISASM").resolve()
    target = (upstream / relative).resolve()
    original = Path.read_text

    def altered(path: Path, *args: object, **kwargs: object) -> str:
        source = original(path, *args, **kwargs)
        if path.resolve() != target:
            return source
        if relative == story_state.SRAM_SOURCE_PATH and "CopyBytesToSram" in before:
            start = source.index("SaveGame:")
            end = source.index("; End of function SaveGame", start)
            section = source[start:end]
            changed_section = section.replace(before, after, 1)
            changed = source[:start] + changed_section + source[end:]
        else:
            changed = source.replace(before, after, 1)
        if changed == source:
            raise AssertionError(f"persistence mutation target drift: {before}")
        return changed

    monkeypatch.setattr(Path, "read_text", altered)
    with pytest.raises(ValueError, match="story-state"):
        story_state.build_story_state_static_contract(upstream)


def test_story_state_runtime_config_excludes_golden_and_binds_failure_contract() -> None:
    fixture = load_json(repo_path("tests/fixtures/h3/story-state-v2.json"))
    static = _static_contract()
    runtime = story_state._runtime_contract(static, repo_path("local/upstream/SF2DISASM"))
    assert runtime["waitForVInt"] == {
        "entryAddress": 3822,
        "waitLoopBranchAddress": 3840,
        "waitLoopBranchInstruction": "bne.s @Wait",
    }
    config = story_state._runtime_config(
        fixture,
        static,
        runtime,
        story_state.derive_story_state_case_matrix(static)
        + story_state.derive_story_state_persistence_case_matrix(static),
    )
    serialized = json.dumps(config, sort_keys=True)
    assert '"observation"' not in serialized
    assert config["observerFailureContract"] == story_state.OBSERVER_FAILURE_CONTRACT
    assert [case["id"] for case in config["caseInputs"][10:]] == list(
        story_state.PERSISTENCE_CASE_ORDER
    )
    assert [case["streamAddress"] for case in config["caseInputs"][:10]] == [0xFF4004] * 10
    assert [case["streamAddress"] for case in config["caseInputs"][10:]] == [0xFF4840] * 6
    assert config["wrapperRoute"] == {
        "wrapperEntryAddress": 292092,
        "bypassAddress": 292116,
        "outerCallSiteAddress": 292114,
        "outerTargetAddress": 65416,
        "outerReturnAddress": 292120,
        "trampolineEntryAddress": 65416,
        "innerCallSiteAddress": 65430,
        "innerTargetAddress": 16730112,
        "innerReturnAddress": 65432,
        "probeEntryAddress": 16730112,
    }
    assert config["scratchLayout"] == {
        "ranges": [
            {"name": "generatedProgram", "address": 16730112, "byteCount": 42},
            {"name": "mutationStream", "address": 16730176, "byteCount": 6},
            {"name": "finalStream", "address": 16730192, "byteCount": 6},
        ],
        "pointerScratch": {"address": 16728064, "byteCount": 4},
        "retainedV1Stream": {"address": 16728068, "byteCount": 6},
    }


def test_story_state_scratch_gap_rejects_old_map_layout_and_source_drift(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    upstream = repo_path("local/upstream/SF2DISASM").resolve()
    static = _static_contract()
    fixture = load_json(repo_path("tests/fixtures/h3/story-state-v2.json"))
    instrumentation = deepcopy(fixture["instrumentation"])
    instrumentation["persistenceProbe"]["programAddress"] = 0xFF6000
    with pytest.raises(ValueError, match="owner-range overlap"):
        story_state._generated_scratch_layout(instrumentation, static["scratchGap"])
    instrumentation = deepcopy(fixture["instrumentation"])
    instrumentation["persistenceProbe"]["mutationStreamAddress"] = 0xFF4804
    with pytest.raises(ValueError, match="scratch range overlap"):
        story_state._generated_scratch_layout(instrumentation, static["scratchGap"])
    instrumentation = deepcopy(fixture["instrumentation"])
    instrumentation["retainedV1Stream"]["address"] = 0xFF4006
    with pytest.raises(ValueError, match="retained-v1 stream/pointer adjacency"):
        story_state._generated_scratch_layout(instrumentation, static["scratchGap"])
    instrumentation = deepcopy(fixture["instrumentation"])
    instrumentation["retainedV1Stream"]["byteCount"] = 5
    with pytest.raises(ValueError, match="retained-v1 stream/pointer adjacency"):
        story_state._generated_scratch_layout(instrumentation, static["scratchGap"])
    instrumentation = deepcopy(fixture["instrumentation"])
    instrumentation["trampoline"]["ramInputAddress"] = 0xFF4004
    with pytest.raises(ValueError, match="pointer scratch"):
        story_state._generated_scratch_layout(instrumentation, static["scratchGap"])

    constants = (upstream / story_state.CONSTANT_SOURCE_PATH).resolve()
    original_text = Path.read_text

    def altered_text(path: Path, *args: object, **kwargs: object) -> str:
        source = original_text(path, *args, **kwargs)
        if path.resolve() == constants:
            return source.replace("byte_FF4A00: equ $FF4A00", "byte_FF4A00: equ $FF4A01", 1)
        return source

    monkeypatch.setattr(Path, "read_text", altered_text)
    with pytest.raises(ValueError, match="scratch-gap"):
        story_state.build_story_state_static_contract(upstream)


def test_story_state_source_reference_audit_and_route_mutations_fail(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    upstream = repo_path("local/upstream/SF2DISASM").resolve()
    source_path = (upstream / story_state.MAP_SCRIPT_SOURCE_PATH).resolve()
    original_bytes = Path.read_bytes

    def altered_bytes(path: Path, *args: object, **kwargs: object) -> bytes:
        value = original_bytes(path, *args, **kwargs)
        if path.resolve() == source_path:
            return value + b"\nmove.l $FF4800,d0\n"
        return value

    monkeypatch.setattr(Path, "read_bytes", altered_bytes)
    with pytest.raises(ValueError, match="scratch-gap source reference"):
        story_state.build_story_state_static_contract(upstream)
    monkeypatch.undo()

    fixture = load_json(repo_path("tests/fixtures/h3/story-state-v2.json"))
    static = _static_contract()
    runtime = story_state._runtime_contract(static, upstream)
    broken = deepcopy(fixture["instrumentation"])
    broken["trampoline"]["callSiteAddress"] = 292116
    with pytest.raises(ValueError, match="wrapper call-site"):
        story_state._trampoline_route(
            broken,
            runtime,
            story_state._generated_scratch_layout(
                fixture["instrumentation"], runtime["scratchGap"]
            ),
        )
    wrong_outer_return = deepcopy(runtime)
    wrong_outer_return["wrapper"]["returnInstructionAddress"] = 65432
    with pytest.raises(ValueError, match="outer-return"):
        story_state._trampoline_route(
            fixture["instrumentation"],
            wrong_outer_return,
            story_state._generated_scratch_layout(
                fixture["instrumentation"], runtime["scratchGap"]
            ),
        )
    invalid_stub = deepcopy(fixture["instrumentation"])
    invalid_stub["trampoline"]["stubHex"] = invalid_stub["trampoline"]["stubHex"].replace(
        "00FF4000", "00FF4010", 1
    )
    with pytest.raises(ValueError, match="instruction/operand"):
        story_state._trampoline_route(
            invalid_stub,
            runtime,
            story_state._generated_scratch_layout(
                fixture["instrumentation"], runtime["scratchGap"]
            ),
        )
    invalid_inner_call = deepcopy(fixture["instrumentation"])
    invalid_inner_call["trampoline"]["stubHex"] = invalid_inner_call["trampoline"][
        "stubHex"
    ].replace("4E904E75", "4E914E75")
    with pytest.raises(ValueError, match="instruction/operand"):
        story_state._trampoline_route(
            invalid_inner_call,
            runtime,
            story_state._generated_scratch_layout(
                fixture["instrumentation"], runtime["scratchGap"]
            ),
        )
    invalid_inner_return = deepcopy(fixture["instrumentation"])
    invalid_inner_return["trampoline"]["stubHex"] = invalid_inner_return["trampoline"][
        "stubHex"
    ].replace("4E904E75", "4E904E71")
    with pytest.raises(ValueError, match="instruction/operand"):
        story_state._trampoline_route(
            invalid_inner_return,
            runtime,
            story_state._generated_scratch_layout(
                fixture["instrumentation"], runtime["scratchGap"]
            ),
        )


def test_story_state_probe_is_armed_before_it_mutates_and_has_a_local_watchdog() -> None:
    source = story_state.OBSERVER.read_text(encoding="utf-8")
    begin = source[
        source.index("local function begin()"):source.index("local function transition_event(")
    ]
    probe_entry_start = source.index("local function actual_probe_entry()")
    probe_entry_end = source.index("local function next_case()")
    probe_entry = source[probe_entry_start:probe_entry_end]
    assert story_state._pattern_byte(33, 0xF11) == 0xDC
    assert "arm_probe(case,case_input)" in begin
    for forbidden in ("seed_probe_case", "snapshot_probe_domains", "clear_selected_slot"):
        assert forbidden not in begin
    assert "seed_probe_case(current(),input(),armed)" in probe_entry
    assert 'status("milestone:story-state-probe")' in probe_entry
    assert source.count("event.on_bus_exec") == 1
    assert "frames>transition.deadline" in source
    assert "frames>pending_trampoline_return.deadline" in source
    assert "wrapper-transition-watchdog" in source
    assert "wrapper-to-probe transition bypassed" in source
    assert "trampoline call/return stack imbalance" in source
    assert "local function stream_address(case_input)" in source
    assert "layout().retainedV1Stream.address" in source
    assert "story-state persistence stream used retained-v1 address" in source
    assert "retained-v1 stream" in source
    assert "returnPc=r.outerReturnAddress" in begin
    assert "set_transition(\"inner\",next_role,route().innerTargetAddress)" in source
    assert "transition_matches(\"outer\")" in source
    assert "transition_matches(\"inner\")" in source
    assert "outerTargetAddress,returnPc=r.innerReturnAddress" not in source
    transition_source = source[
        source.index("local function transition_event("):source.index(
            "local function actual_probe_entry()"
        )
    ]
    assert transition_source.index(
        'set_transition("inner",next_role,route().innerTargetAddress)'
    ) < transition_source.index('"story-state trampoline target drift"')


def test_story_state_later_case_failure_reports_current_mutations_and_restores_session_scopes(
) -> None:
    source = story_state.OBSERVER.read_text(encoding="utf-8")
    begin = source[
        source.index("local function begin()"):source.index("local function transition_event(")
    ]
    restore_start = source.index("local function restore_scopes()")
    restore_end = source.index("local function restoration_json()")
    restore = source[restore_start:restore_end]
    failure_start = source.index("local function fail(")
    failure_end = source.index("local function add_callback(")
    failure = source[failure_start:failure_end]
    assert "local case_mutation_state={logicalRam=false,sram=false,scratch=false}" in source
    assert "local session_touched={logicalRam=false,sram=false,scratch=false}" in source
    assert "case_mutation_state={logicalRam=false,sram=false,scratch=false}" in begin
    assert "case_mutation_state.scratch=true;session_touched.scratch=true" in source
    assert "case_mutation_state.logicalRam=true;session_touched.logicalRam=true" in source
    assert "case_mutation_state.sram=true;session_touched.sram=true" in source
    assert "if session_touched.logicalRam then" in restore
    assert "if session_touched.sram then" in restore
    assert "if session_touched.scratch then" in restore
    assert 'snapshots.scratch.retainedV1Stream,"retainedV1Stream"' in restore
    assert "retainedV1Stream={address=layout().retainedV1Stream.address" in source
    assert "bool(case_mutation_state.logicalRam)" in failure
    assert "bool(case_mutation_state.sram)" in failure
    assert "bool(case_mutation_state.scratch)" in failure


def test_story_state_retained_projection_guard_runs_again_at_prelaunch_golden_boundary(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    seen: list[str] = []
    actual_guard = story_state._assert_retained_v1_projection

    def counted_guard(value: dict[str, object]) -> None:
        seen.append("guard")
        actual_guard(value)

    def semantic_boundary(
        value: dict[str, object], _static: dict[str, object], _runtime: dict[str, object]
    ) -> tuple[list[dict[str, object]], dict[str, object]]:
        story_state._assert_retained_v1_projection(value)  # type: ignore[arg-type]
        return [], {}

    monkeypatch.setattr(story_state, "_assert_retained_v1_projection", counted_guard)
    monkeypatch.setattr(story_state, "verify_runtime_contract", lambda *_args, **_kwargs: None)
    monkeypatch.setattr(story_state, "build_story_state_static_contract", lambda *_args: {})
    monkeypatch.setattr(story_state, "_validate_story_state_h1_rom", lambda *_args: None)
    monkeypatch.setattr(story_state, "_runtime_contract", lambda *_args: {})
    monkeypatch.setattr(story_state, "validate_story_state_fixture_semantics", semantic_boundary)
    monkeypatch.setattr(story_state, "_runtime_config", lambda *_args: {})
    monkeypatch.setattr(
        story_state,
        "_instrument_story_state_rom",
        lambda *_args: pytest.fail("instrumentation reached after prelaunch golden boundary"),
    )
    monkeypatch.setattr(story_state, "FIXTURE", repo_path("tests/fixtures/h3/story-state-v2.json"))
    with pytest.raises(pytest.fail.Exception, match="instrumentation reached"):
        story_state.verify_story_state(
            Path("missing-rom.bin"), repo_path("local/upstream/SF2DISASM")
        )
    assert seen == ["guard", "guard"]


def test_story_state_sleep_return_uses_the_parsed_h1_instruction_width() -> None:
    static = _static_contract()
    runtime = story_state._runtime_contract(static, repo_path("local/upstream/SF2DISASM"))
    prompt = runtime["handlerRecords"][3]
    sleep = prompt["directCalls"][3]
    assert sleep == {
        "h1Address": 292016,
        "returnAddress": 292020,
        "instructionTarget": "Sleep",
        "effectiveTarget": "Sleep",
    }
    fixture = load_json(repo_path("tests/fixtures/h3/story-state-v2.json"))
    fixture["runtimeContract"]["handlerRecords"][3]["directCalls"][3]["returnAddress"] = 292022
    validate_json(
        fixture,
        repo_path("schemas/h3-story-state-fixture.schema.json"),
        owner="story-state Sleep return shape",
    )
    with pytest.raises(ValueError, match="fixture/static runtime"):
        story_state.validate_story_state_fixture_semantics(fixture, static, runtime)


def test_story_state_observer_uses_dispatcher_status_and_scoped_restoration() -> None:
    source = story_state.OBSERVER.read_text(encoding="utf-8")
    for required in (
        "local function dispatch",
        "pcall(entry.handler)",
        "config.observerFailureContract.statusPrefix",
        "os.remove(config.outputPath)",
        "client.exitCode(config.observerFailureContract.exitCode)",
        '\\"callbacksRemaining\\":0',
        "callbacks-cleared",
        "sessionStateRestored",
        "SaveGame",
        "LoadGame",
        "save-return-poison",
        "final-branch-result",
        "mutationState",
        "restorationMismatch",
        "restore_bytes",
        "verify_bytes",
        "story-state pointer scratch readback drift",
    ):
        assert required in source
    _, executable = bizhawk_contract()
    validate_lua_syntax(story_state.OBSERVER, executable)


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
    fixture = load_json(repo_path("tests/fixtures/h3/story-state-v2.json"))
    fixture_schema = repo_path("schemas/h3-story-state-fixture.schema.json")
    observation_schema = repo_path("schemas/h3-story-state-observation.schema.json")
    validate_json(fixture, fixture_schema, owner="story-state fixture baseline")
    validate_json(
        fixture["observation"], observation_schema, owner="story-state observation baseline"
    )

    missing_retained_stream = deepcopy(fixture)
    del missing_retained_stream["instrumentation"]["retainedV1Stream"]
    with pytest.raises(ValueError, match="retainedV1Stream"):
        validate_json(
            missing_retained_stream,
            fixture_schema,
            owner="story-state fixture retained-v1 stream missing",
        )

    missing_retained_restoration = deepcopy(fixture["observation"])
    del missing_retained_restoration["restoration"]["retainedV1Stream"]
    with pytest.raises(ValueError, match="retainedV1Stream"):
        validate_json(
            missing_retained_restoration,
            observation_schema,
            owner="story-state observation retained-v1 restoration missing",
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
    validate_json(reordered, observation_schema, owner="story-state observation reordered shape")

    boundary = deepcopy(fixture)
    boundary["cases"][0]["expected"]["targetInputValue"] = 1 << 32
    with pytest.raises(ValueError, match="not valid"):
        validate_json(boundary, fixture_schema, owner="story-state fixture target boundary")

    handler_reordered = deepcopy(fixture)
    records = handler_reordered["runtimeContract"]["handlerRecords"]
    records[0], records[1] = records[1], records[0]
    validate_json(handler_reordered, fixture_schema, owner="story-state handler reordered shape")

    duplicate_cursor = deepcopy(fixture)
    cursor_sites = duplicate_cursor["runtimeContract"]["handlerRecords"][0]["cursorUseSites"]
    cursor_sites[1] = deepcopy(cursor_sites[0])
    validate_json(duplicate_cursor, fixture_schema, owner="story-state cursor duplicate shape")

    reordered_calls = deepcopy(fixture)
    calls = reordered_calls["runtimeContract"]["handlerRecords"][3]["directCalls"]
    calls[0], calls[1] = calls[1], calls[0]
    validate_json(reordered_calls, fixture_schema, owner="story-state direct-call reordered shape")

    nonempty_prompt_cursor = deepcopy(fixture)
    nonempty_prompt_cursor["runtimeContract"]["handlerRecords"][3]["cursorUseSites"].append(
        {"id": "csc11_promptYesNoForStoryFlow:a6:0", "h1Address": 291984}
    )
    validate_json(nonempty_prompt_cursor, fixture_schema, owner="story-state nonempty cursor shape")

    fixture_observation_reordered = deepcopy(fixture)
    fixture_records = fixture_observation_reordered["observation"]["records"]
    fixture_records[0], fixture_records[1] = fixture_records[1], fixture_records[0]
    validate_json(
        fixture_observation_reordered,
        fixture_schema,
        owner="story-state fixture observation reordered shape",
    )

    static = _static_contract()
    runtime = story_state._runtime_contract(static, repo_path("local/upstream/SF2DISASM"))
    for mutated in (
        handler_reordered,
        duplicate_cursor,
        reordered_calls,
        nonempty_prompt_cursor,
        fixture_observation_reordered,
    ):
        with pytest.raises(ValueError, match="story-state"):
            story_state.validate_story_state_fixture_semantics(mutated, static, runtime)

    chronology_reordered = deepcopy(fixture["observation"])
    chronology = chronology_reordered["records"][0]["chronology"]
    chronology[0], chronology[1] = chronology[1], chronology[0]
    chronology_fixture = deepcopy(fixture)
    chronology_fixture["observation"] = chronology_reordered
    with pytest.raises(ValueError, match="retained-v1 projection"):
        story_state._assert_retained_v1_projection(chronology_fixture)

    chronology_identity = deepcopy(fixture["observation"])
    chronology_identity["records"][0]["chronology"][1]["effectiveTarget"] = "SetFlag"
    chronology_fixture = deepcopy(fixture)
    chronology_fixture["observation"] = chronology_identity
    with pytest.raises(ValueError, match="retained-v1 projection"):
        story_state._assert_retained_v1_projection(chronology_fixture)

    null_observation = deepcopy(fixture)
    null_observation["observation"] = None
    with pytest.raises(ValueError, match="not of type 'object'"):
        validate_json(
            null_observation, fixture_schema, owner="story-state null fixture observation"
        )
    with pytest.raises(ValueError, match="not of type 'object'"):
        validate_json(None, observation_schema, owner="story-state null observation")
