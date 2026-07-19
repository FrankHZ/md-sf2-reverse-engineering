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
ALTERNATE_SOURCES = {
    SOURCE_ROOT / "items/itemfunctions_s7_0.asm": SOURCE_ROOT / "iteminventory.asm",
    SOURCE_ROOT / "items/fielditemeffects.asm": Path(
        "code/common/menus/item/fielditemeffects.asm"
    ),
    SOURCE_ROOT / "items/itemactions_1.asm": Path(
        "code/common/menus/item/isitemusableonfield.asm"
    ),
}
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


def _alternate_source_fact(
    disasm: Path, alternate_path: Path, canonical_path: Path, layout: str
) -> dict[str, Any]:
    canonical = disasm / canonical_path
    alternate = disasm / alternate_path
    canonical_bytes = canonical.read_bytes()
    alternate_bytes = alternate.read_bytes()
    range_pattern = re.compile(rb"; 0x([0-9A-F]+)\.\.0x([0-9A-F]+)")
    canonical_range = range_pattern.search(canonical_bytes)
    alternate_range = range_pattern.search(alternate_bytes)
    if not canonical_range or canonical_range.groups() != alternate_range.groups():
        raise ValueError(f"alternate source ROM range drift: {alternate_path}")
    canonical_labels = set(
        _parse_source_file(canonical, canonical_path.as_posix())["globalLabels"]
    )
    alternate_labels = set(
        _parse_source_file(alternate, alternate_path.as_posix())["globalLabels"]
    )
    canonical_include = f'include "{str(canonical_path).replace("/", chr(92))}"'
    alternate_include = f'include "{str(alternate_path).replace("/", chr(92))}"'
    if canonical_include not in layout or alternate_include in layout:
        raise ValueError(f"alternate source layout inclusion drift: {alternate_path}")
    return {
        "canonicalPath": canonical_path.as_posix(),
        "alternatePath": alternate_path.as_posix(),
        "sameAnnotatedRomRange": True,
        "sourceByteIdentical": canonical_bytes == alternate_bytes,
        "sharedGlobalSymbols": sorted(canonical_labels & alternate_labels),
        "canonicalIncludedByLayout": True,
        "alternateIncludedByLayout": False,
        "alternateExcludedFromStrictReach": True,
        "canonicalSha256": hashlib.sha256(canonical_bytes).hexdigest().upper(),
        "alternateSha256": hashlib.sha256(alternate_bytes).hexdigest().upper(),
    }


def _stats_facts(disasm: Path) -> dict[str, Any]:
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
    layout = "\n".join(
        path.read_text(encoding="utf-8")
        for path in sorted((disasm / "layout").glob("*.asm"))
    )
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
        "excludedAlternateFileCount": len(ALTERNATE_SOURCES),
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
        "statsFacts": _stats_facts(disasm),
        "alternateSources": [
            _alternate_source_fact(disasm, alternate, canonical, layout)
            for alternate, canonical in ALTERNATE_SOURCES.items()
        ],
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
    if output["alternateSources"] != fixture["expected"]["alternateSources"]:
        raise ValueError("common stats alternate-source drift")
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
        "ExcludedAlternates": output["summary"]["excludedAlternateFileCount"],
        "Status": "PASS",
    }
