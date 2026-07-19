from __future__ import annotations

import hashlib
import json
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_ai import _parse_source_file
from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.h2.battlefield import _require_ordered_fragments
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path

ID = "sf2-common-maps-static-v1"
SOURCE_ROOT = Path("code/common/maps")
MANIFEST = repo_path("manifests/extractions/common-maps-static.json")
SCHEMA = repo_path("schemas/common-maps-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/common-maps-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-common-maps-static-fixture.schema.json")
RESEARCH_INDEX = repo_path("manifests/research-index.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

REPRESENTATIVE_SYMBOLS = {
    "animations.asm": "IsMapScrollingToViewTarget",
    "camerafunctions.asm": "VInt_UpdateViewData",
    "egressinit.asm": "GetSavepointForMap",
    "getbattle.asm": "CheckBattle",
    "mapinit_0.asm": "SwitchMap",
    "mapload.asm": "LoadMapLayoutData",
    "unused_mapload.asm": "sub_2EC0",
}


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _map_facts(disasm: Path) -> dict[str, Any]:
    root = disasm / SOURCE_ROOT
    _require_ordered_fragments(
        root / "mapinit_0.asm",
        [
            "lea     table_FlagSwitchedMaps(pc), a0",
            "move.w  (a0),d2",
            "bmi.w   @Done",
            "cmp.w   d0,d2",
            "move.w  2(a0),d1",
            "jsr     j_CheckFlag",
            "move.w  4(a0),d0",
            "addq.l  #6,a0",
        ],
    )
    _require_ordered_fragments(
        root / "getbattle.asm",
        [
            "cmpi.b  #MAP_CURRENT,d0",
            "move.b  ((CURRENT_MAP-$1000000)).w,d0",
            "lea     table_BattleMapCoordinates(pc), a0",
            "moveq   #BATTLES_MAX_INDEX,d6",
            "cmp.b   (a0),d0",
            "move.w  #BATTLE_UNLOCKED_FLAGS_START,d1",
            "jsr     j_CheckFlag",
            "cmpi.b  #-1,BATTLEMAPCOORDINATES_OFFSET_TRIGGER_X(a0)",
            "cmpi.b  #-1,BATTLEMAPCOORDINATES_OFFSET_TRIGGER_Y(a0)",
            "move.b  BATTLEMAPCOORDINATES_OFFSET_X(a0),((BATTLE_AREA_X-$1000000)).w",
            "addi.w  #BATTLE_UNLOCKED_TO_COMPLETED_FLAGS_OFFSET,d1",
            "jsr     j_CheckFlag",
            "jsr     j_ClearFlag",
            "addq.l  #BATTLEMAPCOORDINATES_ENTRY_SIZE_FULL,a0",
            "moveq   #-1,d7",
        ],
    )
    _require_ordered_fragments(
        root / "egressinit.asm",
        [
            "chkFlg  399",
            "moveq   #GAMESTART_MAP,d0",
            "moveq   #GAMESTART_SAVEPOINT_X,d1",
            "moveq   #GAMESTART_SAVEPOINT_Y,d2",
            "moveq   #GAMESTART_FACING,d3",
            "moveq   #1,d1",
            "moveq   #1,d2",
            "moveq   #UP,d3",
            "lea     table_SavepointMapCoordinates(pc), a0",
            "cmpi.b  #-1,(a0)",
            "addq.l  #4,a0",
            "chkFlg  64",
            "move.b  1(a0),((RAFT_MAP-$1000000)).w",
            "move.b  2(a0),((RAFT_X-$1000000)).w",
            "move.b  3(a0),((RAFT_Y-$1000000)).w",
        ],
    )
    _require_ordered_fragments(
        root / "animations.asm",
        [
            "bclr    #0,((VIEW_PLANE_UPDATE_TOGGLE_BITFIELD-$1000000)).w",
            "bsr.w   UpdateVdpPlaneA",
            "bsr.w   CopyPlaneALayoutForWindows",
            "bsr.w   FixWindowsPositions",
            "bclr    #1,((VIEW_PLANE_UPDATE_TOGGLE_BITFIELD-$1000000)).w",
            "bsr.w   UpdateVdpPlaneB",
            "move.l  ((TILE_ANIMATION_DATA_ADDRESS-$1000000)).w,d0",
            "subq.w  #1,((TILE_ANIMATION_COUNTER-$1000000)).w",
            "move.w  (a0)+,((TILE_ANIMATION_COUNTER-$1000000)).w",
            "bsr.w   ApplyVIntVramDma",
        ],
    )
    _require_ordered_fragments(
        root / "mapload.asm",
        [
            "lea     (MAP_LAYOUT_HISTORY_MAP_SIZES).l,a4",
            "lea     $2000(a1),a6",
            "cmpa.l  a6,a1",
            "bsr.w   ReadMapLayoutBarrelForBlockFlags",
            "move.w  -$80(a1),(a1)+",
            "move.w  -2(a1),(a1)+",
            "lea     (FF6800_MAP_LOADING_LEFT_HISTORY_MAP).l,a5",
            "lea     (FF8800_MAP_LOADING_UPPER_HISTORY_MAP).l,a5",
            "LoadMap:",
            "bsr.w   InitializeDisplay",
            "move.b  d1,((CURRENT_MAP-$1000000)).w",
            "bsr.w   LoadMapBlocksAndLayout",
            "LoadMapBlocksAndLayout:",
            "bsr.w   LoadMapBlocks",
            "bsr.w   LoadMapLayoutData",
            "cmpi.b  #NOT_CURRENTLY_IN_BATTLE,((CURRENT_BATTLE-$1000000)).w",
            "bsr.w   CopyMapBlocks",
        ],
    )
    _require_ordered_fragments(
        root / "unused_mapload.asm",
        [
            "move.w  #$20,d6",
            "bsr.w   GenerateRandomNumber",
            "move.w  #4,d6",
            "bsr.w   GenerateRandomNumber",
            "addi.w  #$1C,d1",
            "move.w  #$10,d6",
            "bsr.w   GenerateRandomNumber",
            "move.w  #4,d6",
            "bsr.w   GenerateRandomNumber",
            "cmpi.w  #128,d0",
        ],
    )
    return {
        "mapSwitch": {
            "entryBytes": 6,
            "terminatesOnNegativeSourceMap": True,
            "firstMatchingSetFlagReplacesMap": True,
        },
        "battleTrigger": {
            "usesCurrentMapForMinusOneInput": True,
            "requiresUnlockedFlag": True,
            "triggerCoordinatesAllowMinusOneWildcard": True,
            "writesBattleAreaBeforeCompletionCheck": True,
            "clearsUnlockedFlagWhenCompleted": True,
            "noMatchBattleIndex": -1,
        },
        "egress": {
            "preFlag399UsesGameStartConstants": True,
            "savepointEntryBytes": 4,
            "savepointTerminator": -1,
            "missingMapDefault": {"x": 1, "y": 1, "facing": "UP"},
            "raftResetRequiresFlag64": True,
            "raftResetEntryBytes": 4,
        },
        "mapLayout": {
            "outputSizeBytes": 8192,
            "clearsHistoryMapsFirst": True,
            "modes": ["new block", "copy run", "left history", "upper history"],
            "mapLoadClearsScrollState": True,
            "newMapUpdatesCurrentMap": True,
            "blocksLoadBeforeLayout": True,
            "battleAreaOverlayAppliedInBattle": True,
        },
        "vint": {
            "planeAToggleBit": 0,
            "planeBToggleBit": 1,
            "planeAAlsoRefreshesWindowsWhenPresent": True,
            "tileAnimationRequiresPositiveDataPointer": True,
            "tileAnimationUsesCountdown": True,
            "tileAnimationUsesVintDma": True,
        },
        "inventoryBoundary": {
            "cameraStateMachineInventoried": True,
            "unusedRandomMaploadInventoried": True,
            "cameraAndVdpTimingRemainQueued": True,
        },
    }


def build_map_inventory(upstream_path: Path) -> dict[str, Any]:
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    paths = sorted((disasm / SOURCE_ROOT).glob("*.asm"), key=lambda path: path.as_posix())
    files = [_parse_source_file(path, path.relative_to(disasm).as_posix()) for path in paths]
    if {Path(row["path"]).name for row in files} != set(REPRESENTATIVE_SYMBOLS):
        raise ValueError("common maps file set drift")
    labels = {label for row in files for label in row["globalLabels"]}
    calls: Counter[str] = Counter()
    for row in files:
        for call in row["directCalls"]:
            calls[call["target"]] += call["siteCount"]
    records = [
        record
        for record in load_json(RESEARCH_INDEX)["records"]
        if Path(record["sourcePath"]).parent == SOURCE_ROOT
    ]
    summary = {
        "fileCount": len(files),
        "sourceLineCount": sum(row["sourceLineCount"] for row in files),
        "statementCount": sum(row["statementCount"] for row in files),
        "globalLabelCount": sum(len(row["globalLabels"]) for row in files),
        "localLabelCount": sum(row["localLabelCount"] for row in files),
        "directCallSiteCount": sum(calls.values()),
        "indirectCallSiteCount": sum(row["indirectCallSiteCount"] for row in files),
        "uniqueDirectTargetCount": len(calls),
        "internalDirectTargetCount": sum(target in labels for target in calls),
        "externalDirectTargetCount": sum(target not in labels for target in calls),
        "indexedRecordCount": len(records),
        "indexedFileCount": len({record["sourcePath"] for record in records}),
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "scope": SOURCE_ROOT.as_posix(),
        "summary": summary,
        "indexedRecordIds": sorted(record["id"] for record in records),
        "indexedSourcePaths": sorted({record["sourcePath"] for record in records}),
        "internalDirectCallTargets": sorted(target for target in calls if target in labels),
        "externalDirectCallTargets": sorted(target for target in calls if target not in labels),
        "mapFacts": _map_facts(disasm),
        "files": files,
    }


def verify_map_inventory(upstream_path: Path, *, output_path: Path | None = None) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_map_inventory(upstream_path)
    validate_json(output, SCHEMA, owner="common maps static inventory")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != load_json(ROM_MANIFEST)["hashes"]["sha256"]
    ):
        raise ValueError("common maps provenance drift")
    if output["summary"] != manifest["summary"]:
        raise ValueError("common maps summary drift")
    by_name = {Path(row["path"]).name: row for row in output["files"]}
    for filename, symbol in fixture["expected"]["representativeSymbols"].items():
        if symbol not in by_name[filename]["globalLabels"]:
            raise ValueError(f"common maps symbol drift: {filename}::{symbol}")
    if output["mapFacts"] != fixture["expected"]["mapFacts"]:
        raise ValueError("common maps model drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"]:
        raise ValueError("common maps canonical hash drift")
    destination = output_path or repo_path("local/derived/common-maps-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Inventory": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Files": output["summary"]["fileCount"],
        "GlobalLabels": output["summary"]["globalLabelCount"],
        "DirectCallSites": output["summary"]["directCallSiteCount"],
        "IndexedRecords": output["summary"]["indexedRecordCount"],
        "Status": "PASS",
    }
