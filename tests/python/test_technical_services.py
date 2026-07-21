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
