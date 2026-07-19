from __future__ import annotations

import hashlib
import json
import re
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_ai import _parse_source_file
from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.h2.battlefield import _require_ordered_fragments
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path

ID = "sf2-common-stats-static-v1"
SOURCE_ROOT = Path("code/common/stats")
DUPLICATE_SOURCE = SOURCE_ROOT / "items/itemfunctions_s7_0.asm"
CANONICAL_ITEM_INVENTORY = SOURCE_ROOT / "iteminventory.asm"
MANIFEST = repo_path("manifests/extractions/common-stats-static.json")
SCHEMA = repo_path("schemas/common-stats-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/common-stats-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-common-stats-static-fixture.schema.json")
RESEARCH_INDEX = repo_path("manifests/research-index.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

REPRESENTATIVE_SYMBOLS = {
    "battleparty.asm": "UpdateForce",
    "caravaninventory.asm": "AddItemToCaravan",
    "combatantstats_1.asm": "GetCombatantName",
    "combatantstats_2.asm": "LoadAllyName",
    "combatantstats_3.asm": "GetCombatantEntryAddress",
    "dealsinventory.asm": "GetDealsItemAmount",
    "findname.asm": "GetClassName",
    "gameflags.asm": "CheckFlag",
    "getcombatanttype.asm": "GetCombatantType",
    "gold.asm": "SetGold",
    "iteminventory.asm": "ReceiveMandatoryItem",
    "items/fielditemeffects.asm": "UseItemOnField",
    "items/itemactions_1.asm": "IsItemUsableOnField",
    "items/itemfunctions_s7_0.asm": "ReceiveMandatoryItem",
    "itemstats.asm": "GetItemName",
    "levelup.asm": "LevelUp",
    "newgame.asm": "NewGame",
    "spellstats.asm": "GetSpellName",
    "unusedsub_9482.asm": "nullsub_9482",
    "updatecombatantstats.asm": "UpdateCombatantStats",
}


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _field_item_pairs(path: Path) -> list[dict[str, str]]:
    lines = path.read_text(encoding="utf-8").splitlines()
    start = next(
        index for index, line in enumerate(lines) if line.strip() == "rjt_FieldItemEffects:"
    )
    values: list[str] = []
    for line in lines[start + 1 :]:
        if re.match(r"^[A-Za-z0-9_@]+:", line):
            break
        match = re.search(r"dc\.w\s+([^\s;]+)", line)
        if match:
            values.append(match.group(1))
    if values[-1] != "-1" or (len(values) - 1) % 2:
        raise ValueError("field item dispatch table shape drift")
    pairs = []
    for index in range(0, len(values) - 1, 2):
        effect = values[index + 1].split("-")[0]
        pairs.append({"item": values[index], "effect": effect})
    return pairs


def _duplicate_fact(disasm: Path) -> dict[str, Any]:
    canonical = disasm / CANONICAL_ITEM_INVENTORY
    duplicate = disasm / DUPLICATE_SOURCE
    canonical_bytes = canonical.read_bytes()
    duplicate_bytes = duplicate.read_bytes()
    range_pattern = re.compile(rb"; 0x([0-9A-F]+)\.\.0x([0-9A-F]+)")
    canonical_range = range_pattern.search(canonical_bytes)
    duplicate_range = range_pattern.search(duplicate_bytes)
    if not canonical_range or canonical_range.groups() != duplicate_range.groups():
        raise ValueError("alternate item inventory ROM range drift")
    canonical_labels = set(
        _parse_source_file(canonical, CANONICAL_ITEM_INVENTORY.as_posix())["globalLabels"]
    )
    duplicate_labels = set(
        _parse_source_file(duplicate, DUPLICATE_SOURCE.as_posix())["globalLabels"]
    )
    layout = (disasm / "layout/sf2-07-0x044000-0x064000.asm").read_text(encoding="utf-8")
    canonical_include = 'include "code\\common\\stats\\iteminventory.asm"'
    duplicate_include = 'include "code\\common\\stats\\items\\itemfunctions_s7_0.asm"'
    if canonical_include not in layout or duplicate_include in layout:
        raise ValueError("item inventory layout inclusion drift")
    return {
        "canonicalPath": CANONICAL_ITEM_INVENTORY.as_posix(),
        "alternatePath": DUPLICATE_SOURCE.as_posix(),
        "sameAnnotatedRomRange": True,
        "sourceByteIdentical": canonical_bytes == duplicate_bytes,
        "sharedGlobalSymbols": sorted(canonical_labels & duplicate_labels),
        "canonicalIncludedByLayout": True,
        "alternateIncludedByLayout": False,
        "alternateExcludedFromStrictReach": True,
        "canonicalSha256": hashlib.sha256(canonical_bytes).hexdigest().upper(),
        "alternateSha256": hashlib.sha256(duplicate_bytes).hexdigest().upper(),
    }


def _stats_facts(disasm: Path, field_item_pairs: list[dict[str, str]]) -> dict[str, Any]:
    root = disasm / SOURCE_ROOT
    _require_ordered_fragments(
        root / "gameflags.asm",
        [
            "andi.l  #FLAG_MASK,d1",
            "divu.w  #8,d1",
            "lea     ((GAME_FLAGS-$1000000)).w,a0",
            "adda.w  d1,a0",
            "swap    d1",
            "moveq   #$FFFFFF80,d0",
            "lsr.b   d1,d0",
        ],
    )
    _require_ordered_fragments(
        root / "battleparty.asm",
        [
            "lea     ((TARGETS_LIST-$1000000)).w,a2",
            "lea     ((BATTLE_PARTY_MEMBERS-$1000000)).w,a3",
            "lea     ((RESERVE_MEMBERS-$1000000)).w,a4",
            "addi.w  #FORCEMEMBER_JOINED_FLAGS_START,d1",
            "bsr.s   CheckFlag",
            "addi.w  #FORCEMEMBER_ACTIVE_FLAGS_START,d1",
            "bsr.s   CheckFlag",
            "move.w  d2,((TARGETS_LIST_LENGTH-$1000000)).w",
            "move.w  d3,((BATTLE_PARTY_MEMBERS_NUMBER-$1000000)).w",
            "move.w  d4,((OTHER_PARTY_MEMBERS_NUMBER-$1000000)).w",
            "cmpi.w  #FORCE_MAX_SIZE,((BATTLE_PARTY_MEMBERS_NUMBER-$1000000)).w",
            "bsr.w   JoinBattleParty",
        ],
    )
    _require_ordered_fragments(
        root / "caravaninventory.asm",
        [
            "moveq   #CARAVAN_MAX_ITEMS_NUMBER_MINUS_ONE,d0",
            "cmp.w   ((CARAVAN_ITEMS_NUMBER-$1000000)).w,d0",
            "andi.w  #ITEMENTRY_MASK_INDEX,d1",
            "move.b  d1,(a0,d0.w)",
            "addq.w  #1,((CARAVAN_ITEMS_NUMBER-$1000000)).w",
            "subq.w  #1,((CARAVAN_ITEMS_NUMBER-$1000000)).w",
            "move.b  #ITEM_NOTHING,(a0)",
        ],
    )
    _require_ordered_fragments(
        root / "dealsinventory.asm",
        [
            "cmpi.b  #DEALS_MAX_NUMBER_PER_ITEM,d2",
            "add.b   d0,(a0)",
            "tst.b   d2",
            "sub.b   d0,(a0)",
            "andi.l  #ITEMENTRY_MASK_INDEX,d1",
            "divu.w  #2,d1",
            "btst    #DEALS_BIT_REMAINDER,d1",
            "moveq   #DEALS_ADD_AMOUNT_EVEN,d0",
            "moveq   #DEALS_ADD_AMOUNT_ODD,d0",
        ],
    )
    _require_ordered_fragments(
        root / "getcombatanttype.asm",
        [
            "btst    #COMBATANT_BIT_ENEMY,d0",
            "bsr.w   GetClass",
            "move.b  table_ClassTypes(pc,d1.w),d1",
            "mulu.w  #COMBATANT_ALLIES_NUMBER,d1",
            "add.w   d0,d1",
            "bset    #15,d1",
            "bsr.s   GetEnemy",
        ],
    )
    _require_ordered_fragments(
        root / "spellstats.asm",
        [
            "andi.w  #SPELLENTRY_MASK_INDEX,d1",
            "movea.l (p_table_SpellNames).l,a0",
            "bsr.w   FindName",
            "movea.l (p_table_SpellDefinitions).l,a0",
            "movea.l (p_table_SpellDefinitions).l,a0",
            "move.w  #1,d2",
            "andi.w  #SPELLENTRY_MASK_INDEX,d4",
            "lsr.w   #SPELLENTRY_OFFSET_LV,d5",
            "move.b  d1,(a0)",
            "move.w  #2,d2",
            "move.b  d1,-(a0)",
            "clr.w   d2",
        ],
    )
    _require_ordered_fragments(
        root / "newgame.asm",
        [
            "bsr.w   InitializeGameSettings",
            "bsr.w   InitializeAllyCombatantEntry",
            "moveq   #GAMESTART_GOLD,d1",
            "bsr.w   SetGold",
            "moveq   #ALLY_BOWIE,d0",
            "bsr.w   JoinForce",
            "move.l  #LONGWORD_SPELLS_INITVALUE,COMBATANT_OFFSET_SPELLS(a1)",
            "bsr.w   LoadAllyClassData",
            "bsr.w   InitializeAllyStats",
            "bsr.w   UpdateCombatantStats",
            "lea     ((GAME_FLAGS-$1000000)).w,a0",
            "lea     ((DEALS_ITEMS-$1000000)).w,a0",
            "lea     ((CARAVAN_ITEMS-$1000000)).w,a0",
            "move.b  #2,((MESSAGE_SPEED-$1000000)).w",
        ],
    )
    _require_ordered_fragments(
        root / "items/itemactions_1.asm",
        [
            "lea     table_UsableOnFieldItems(pc), a0",
            "cmp.b   (a0)+,d1",
            "cmpi.b  #-1,(a0)",
            "moveq   #-1,d2",
        ],
    )
    return {
        "flags": {
            "flagIndexMasked": True,
            "bitsPerByte": 8,
            "maskStartsAtBit7": True,
            "checkSetAndClearShareGetFlag": True,
        },
        "party": {
            "joinedAndActiveUseSeparateFlagRanges": True,
            "updateBuildsForceActiveAndReserveLists": True,
            "joinForceAutoActivatesBelowForceMax": True,
            "leaveForceMovesCombatantOffMap": True,
        },
        "inventories": {
            "caravanMasksItemStatusBits": True,
            "caravanFullAddIsIgnored": True,
            "caravanRemovalCompactsAndWritesNothing": True,
            "dealsStoresTwoItemCountsPerByte": True,
            "dealsCountSaturates": True,
            "dealsRemoveAtZeroIsIgnored": True,
        },
        "combatantType": {
            "allySetsHighBit": True,
            "allyEncodesClassTypeTimesAllyCountPlusIndex": True,
            "enemyReturnsEnemyIndex": True,
            "upstreamMarksFeatureUnused": True,
        },
        "fieldItems": {
            "dispatchPairCount": len(field_item_pairs),
            "pairs": field_item_pairs,
            "terminator": -1,
            "usabilityListTerminator": -1,
            "unlistedItemResult": -1,
        },
        "spells": {
            "definitionMissDefaultsToFirstEntry": True,
            "learnSuccess": 0,
            "sameOrHigherKnownFailure": 1,
            "noRoomFailure": 2,
            "higherLevelReplacesKnownEntry": True,
        },
        "newGame": {
            "settingsBeforeAllies": True,
            "allAlliesInitialized": True,
            "startingGoldThenBowieJoin": True,
            "allySpellSlotsInitializedToNothing": True,
            "classDataThenStatsThenDerivedStats": True,
            "clearsFlagsDealsAndCaravan": True,
            "defaultMessageSpeed": 2,
        },
        "inventoryBoundary": {
            "combatantGettersAndSettersInventoried": True,
            "itemDefinitionHelpersInventoried": True,
            "existingLevelGoldAndDerivedStatRailsRetained": True,
            "callerDependentUiAndItemEffectsRemainQueued": True,
        },
    }


def build_stats_inventory(upstream_path: Path) -> dict[str, Any]:
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    paths = sorted((disasm / SOURCE_ROOT).rglob("*.asm"), key=lambda path: path.as_posix())
    files = [_parse_source_file(path, path.relative_to(disasm).as_posix()) for path in paths]
    if {
        Path(row["path"]).relative_to(SOURCE_ROOT).as_posix(): row["globalLabels"][0]
        for row in files
    } != REPRESENTATIVE_SYMBOLS:
        raise ValueError("common stats file/symbol set drift")
    labels = {label for row in files for label in row["globalLabels"]}
    calls: Counter[str] = Counter()
    for row in files:
        for call in row["directCalls"]:
            calls[call["target"]] += call["siteCount"]
    records = [
        record
        for record in load_json(RESEARCH_INDEX)["records"]
        if Path(record["sourcePath"]).is_relative_to(SOURCE_ROOT)
    ]
    field_item_pairs = _field_item_pairs(disasm / SOURCE_ROOT / "items/fielditemeffects.asm")
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
        "excludedDuplicateFileCount": 1,
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
        "statsFacts": _stats_facts(disasm, field_item_pairs),
        "duplicateSource": _duplicate_fact(disasm),
        "files": files,
    }


def verify_stats_inventory(
    upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_stats_inventory(upstream_path)
    validate_json(output, SCHEMA, owner="common stats static inventory")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != load_json(ROM_MANIFEST)["hashes"]["sha256"]
    ):
        raise ValueError("common stats provenance drift")
    if output["summary"] != manifest["summary"]:
        raise ValueError("common stats summary drift")
    by_relative = {
        Path(row["path"]).relative_to(SOURCE_ROOT).as_posix(): row for row in output["files"]
    }
    for relative, symbol in fixture["expected"]["representativeSymbols"].items():
        if symbol not in by_relative[relative]["globalLabels"]:
            raise ValueError(f"common stats symbol drift: {relative}::{symbol}")
    if output["statsFacts"] != fixture["expected"]["statsFacts"]:
        raise ValueError("common stats model drift")
    if output["duplicateSource"] != fixture["expected"]["duplicateSource"]:
        raise ValueError("common stats duplicate-source drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"]:
        raise ValueError("common stats canonical hash drift")
    destination = output_path or repo_path("local/derived/common-stats-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Inventory": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Files": output["summary"]["fileCount"],
        "IndexedFiles": output["summary"]["indexedFileCount"],
        "FieldItemPairs": output["statsFacts"]["fieldItems"]["dispatchPairCount"],
        "ExcludedDuplicates": output["summary"]["excludedDuplicateFileCount"],
        "Status": "PASS",
    }
