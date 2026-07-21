from __future__ import annotations

import pytest

from sf2tool.h2.services import build_service_inventory
from sf2tool.paths import repo_path

UPSTREAM = repo_path("local/upstream/SF2DISASM")


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_sram_static_contract_covers_layout_operations_and_unknowns() -> None:
    sram = build_service_inventory(UPSTREAM)["sramFacts"]

    assert sram["functionEntries"] == {
        "CheckSram": 28326,
        "SaveGame": 28522,
        "LoadGame": 28588,
        "CopySave": 28634,
        "ClearSaveSlotFlag": 28652,
        "CopyBytesToSram": 28676,
        "CopyBytesFromSram": 28700,
    }
    assert sram["layout"] == {
        "logicalSlotCount": 2,
        "slotSelector": {"zero": "slot1", "nonZero": "slot2"},
        "logicalBytesPerSlot": 4016,
        "storedPhysicalByteCountPerSlot": 4016,
        "physicalAddressIntervalPerSlot": 8032,
        "physicalAddressStepPerLogicalByte": 2,
        "fullClearLogicalByteCount": 8192,
        "occupiedFlagBits": {"slot1": 0, "slot2": 1},
    }
    assert sram["operations"] == {
        "checkOrder": ["signature", "slot2", "slot1"],
        "validOccupiedSlotResult": 1,
        "emptySlotResult": 0,
        "invalidOccupiedSlotResult": -1,
        "invalidChecksumClearsOccupiedFlag": True,
        "signatureMismatchInitializesAllLogicalSramBytes": True,
        "initializationWritesSignatureThenClearsSaveFlags": True,
        "saveCopiesCombatantDataThenStoresChecksumThenSetsOccupiedFlag": True,
        "loadCopiesSelectedSlotToCombatantDataWithoutLocalChecksumComparison": True,
        "copyLoadsSelectedSlotThenSavesToOtherSlot": True,
        "clearOnlyClearsSelectedOccupiedFlag": True,
    }
    assert sram["checksum"] == {
        "accumulatorBits": 8,
        "copyToSramAddsSourceByteAfterStore": True,
        "copyFromSramAddsInterleavedSourceByte": True,
        "storedAsByteAtSelectedChecksumAddress": True,
        "checkComparesComputedByteToSelectedChecksumByte": True,
    }
    assert sram["externalCallerOccurrences"] == {
        "code/common/menus/church/churchactions_1.asm": {"SaveGame": 1},
        "code/gameflow/battle/battlefunctions/battlefunctions_2.asm": {"SaveGame": 1},
        "code/specialscreens/witch/witchstart.asm": {
            "CheckSram": 1,
            "SaveGame": 1,
            "LoadGame": 1,
            "CopySave": 1,
            "ClearSaveSlotFlag": 1,
        },
    }
    assert sram["runtimeQuestions"] == [
        "sram-signature-and-full-clear-on-real-persistent-media",
        "sram-valid-invalid-checksum-slot-flag-matrix",
        "sram-save-copy-delete-and-reload-persistence-ordering",
        "sram-power-loss-and-partial-write-boundaries",
    ]


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_input_static_contract_covers_sampling_waits_callers_and_unknowns() -> None:
    input_facts = build_service_inventory(UPSTREAM)["inputFacts"]

    assert input_facts == {
        "sourcePath": "code/common/tech/input.asm",
        "sourceLineCount": 158,
        "functionEntries": {
            "UpdatePlayerInputs": 5390,
            "WaitForPlayerInput": 5494,
            "WaitForPlayer1NewInput": 5510,
            "sub_15A4": 5540,
            "WaitForInputFor1Second": 5592,
            "WaitForInputFor3Seconds": 5620,
        },
        "sourceDirectCallSiteCounts": {"WaitForVInt": 4},
        "constants": {
            "addresses": {
                "DATA1": 10551299,
                "DATA2": 10551301,
                "PLAYER_1_INPUT": 16768663,
                "PLAYER_2_INPUT": 16768665,
                "CURRENT_PLAYER_INPUT": 16768667,
                "byte_FFDE9E": 16768670,
                "byte_FFDE9F": 16768671,
                "LAST_PLAYER_INPUT": 16768748,
                "INPUT_REPEAT_DELAYER": 16768749,
            },
            "buttonMasks": {
                "INPUT_UP": 1,
                "INPUT_DOWN": 2,
                "INPUT_LEFT": 4,
                "INPUT_RIGHT": 8,
                "INPUT_B": 16,
                "INPUT_C": 32,
                "INPUT_A": 64,
                "INPUT_START": 128,
            },
        },
        "sampling": {
            "controllerPortAddresses": [10551299, 10551301],
            "controllerPortStrideBytes": 2,
            "controllerCount": 2,
            "rawStateBytesPerController": 2,
            "rawStateStorageAddresses": [16768663, 16768665],
            "thLowWriteValue": 0,
            "thHighWriteValue": 64,
            "highBitsLeftShift": 2,
            "highBitsMask": 192,
            "lowBitsMask": 63,
            "invertsComposedState": True,
            "storesTwoComposedStatesPerPort": True,
        },
        "waits": {
            "recognizedButtonMask": 255,
            "waitForPlayerInputUsesCurrentInput": True,
            "waitForPlayerInputReturnsWhenRecognizedInputIsNonzero": True,
            "waitForPlayer1NewInputRequiresReleaseThenRecognizedPress": True,
            "oneSecondMaximumVintWaits": 60,
            "threeSecondMaximumVintWaits": 180,
            "boundedWaitsReturnEarlyOnRecognizedPlayer1Input": True,
        },
        "heldInputSuppression": {
            "player1AndMaskUsesScratchMaskAddress": 16768670,
            "counterAddress": 16768671,
            "counterThreshold": 10,
            "overlapBelowThresholdClearsPlayer1Input": True,
            "zeroOverlapOrCounterAtLeastThresholdClearsScratchState": True,
        },
        "externalDirectCallerOccurrences": {
            "code/common/menus/blacksmith/blacksmithactions.asm": {
                "WaitForPlayerInput": 1,
            },
            "code/common/menus/church/churchactions_2.asm": {
                "WaitForPlayerInput": 1,
            },
            "code/common/menus/memberslistscreen.asm": {"WaitForPlayerInput": 1},
            "code/common/menus/shop/shopactions.asm": {"WaitForPlayerInput": 1},
            "code/common/stats/iteminventory.asm": {"WaitForPlayerInput": 2},
            "code/common/stats/items/itemfunctions_s7_0.asm": {
                "WaitForPlayerInput": 2,
            },
            "code/common/tech/interrupts/applyfadingeffectandz80busupdate.asm": {
                "UpdatePlayerInputs": 1,
            },
            "code/gameflow/battle/battlefunctions/battlefunctions_0.asm": {
                "WaitForPlayerInput": 1,
            },
            "code/gameflow/battle/battlefunctions/battlefunctions_2.asm": {
                "WaitForPlayerInput": 1,
            },
        },
        "externalDirectCallSiteCounts": {
            "UpdatePlayerInputs": 1,
            "WaitForPlayerInput": 10,
            "WaitForPlayer1NewInput": 0,
            "sub_15A4": 0,
            "WaitForInputFor1Second": 0,
            "WaitForInputFor3Seconds": 0,
        },
        "runtimeQuestions": [
            "controller-input-matrix-raw-state-a-b-to-last-current",
            "controller-input-matrix-new-press-and-release-repress",
            "controller-input-matrix-held-24-frame-initial-and-6-frame-repeat",
            "controller-input-matrix-one-and-three-second-early-exit-and-timeout",
            "controller-input-matrix-three-versus-six-button-and-hardware-latency-edges",
        ],
    }
    assert input_facts["sampling"]["controllerPortStrideBytes"] == (
        input_facts["constants"]["addresses"]["DATA2"]
        - input_facts["constants"]["addresses"]["DATA1"]
    )
    assert input_facts["sampling"]["controllerPortStrideBytes"] == (
        input_facts["constants"]["addresses"]["PLAYER_2_INPUT"]
        - input_facts["constants"]["addresses"]["PLAYER_1_INPUT"]
    )
    expected_mask = 0
    for value in input_facts["constants"]["buttonMasks"].values():
        expected_mask |= value
    assert input_facts["waits"]["recognizedButtonMask"] == expected_mask
