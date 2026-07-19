from __future__ import annotations

import hashlib
import json
import subprocess
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_ai import _parse_source_file
from sf2tool.h2.battlefield import _require_ordered_fragments
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path

ID = "sf2-battle-actions-static-v1"
SOURCE_ROOT = Path("code/gameflow/battle/battleactions")
MANIFEST = repo_path("manifests/extractions/battle-actions-static.json")
SCHEMA = repo_path("schemas/battle-actions-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/battle-actions-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-battle-actions-static-fixture.schema.json")
TOOLCHAIN = repo_path("manifests/toolchain.json")
RESEARCH_INDEX = repo_path("manifests/research-index.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

REPRESENTATIVE_SYMBOLS = {
    "animateaction.asm": "battlesceneScript_AnimateAction",
    "attack.asm": "battlesceneScript_Attack",
    "battleactionsengine_1.asm": "WriteBattlesceneScript",
    "battleactionsengine_2.asm": "battlesceneScript_End",
    "breakuseditem.asm": "battlesceneScript_BreakUsedItem",
    "calculatedamage.asm": "battlesceneScript_CalculateDamage",
    "calculatespelldamage.asm": "battlesceneScript_CalculateSpellDamage",
    "castspell.asm": "battlesceneScript_CastSpell",
    "createbattlesceneanimation.asm": "battlesceneScript_PerformAnimation",
    "createbattlescenemessage.asm": "battlesceneScript_DisplayActionMessage",
    "determinecriticalhit.asm": "battlesceneScript_DetermineCriticalHit",
    "determinedodge.asm": "battlesceneScript_DetermineDodge",
    "determinedoubleandcounter.asm": "battlesceneScript_DetermineDoubleAndCounter",
    "determineineffectiveattack.asm": "battlesceneScript_DetermineIneffectiveAttack",
    "displaydeathmessage.asm": "battlesceneScript_DisplayDeathMessage",
    "dropenemyitem.asm": "battlesceneScript_DropEnemyItem",
    "earnexp.asm": "battlesceneScript_CalculateHealingExp",
    "getresistancetospell.asm": "GetResistanceToSpell",
    "getspellanimation.asm": "battlesceneScript_GetSpellanimation",
    "giveexpandgold.asm": "battlesceneScript_GiveExpAndGold",
    "inflictailment.asm": "battlesceneScript_InflictAilment",
    "inflictcursedamage.asm": "battlesceneScript_InflictCurseDamage",
    "inflictdamage.asm": "battlesceneScript_InflictDamage",
    "initbattlesceneproperties.asm": "battlesceneScript_InitializeBattlesceneProperties",
    "isabletocounterattack.asm": "battlesceneScript_ValidateCounterAttack",
    "nullsub_BBE4.asm": "nullsub_BBE4",
    "sorttargets.asm": "battlesceneScript_SortTargets",
    "unused_battleactions.asm": "OneSecondSleep",
    "useitem.asm": "battlesceneScript_UseItem",
}


def _build_action_facts(disasm: Path) -> dict[str, Any]:
    root = disasm / SOURCE_ROOT
    _require_ordered_fragments(
        root / "battleactionsengine_1.asm",
        [
            "move.w  d1,((BATTLESCENE_EXP-$1000000)).w",
            "move.w  d1,((BATTLESCENE_GOLD-$1000000)).w",
            "move.w  d1,((BATTLESCENE_ATTACK_TYPE-$1000000)).w",
            "bsr.w   battlesceneScript_DetermineTargetsByAction",
            "bsr.w   battlesceneScript_InitializeBattlesceneProperties",
            "bsr.w   battlesceneScript_DetermineIneffectiveAttack",
            "bsr.w   battlesceneScript_InitializeActors",
            "bsr.w   battlesceneScript_DisplayActionMessage",
            "bsr.w   battlesceneScript_PerformAnimation",
            "bsr.w   battlesceneScript_ApplyActionEffect",
            "bsr.w   battlesceneScript_DropEnemyItem",
            "bsr.w   battlesceneScript_BreakUsedItem",
            "bsr.w   battlesceneScript_ValidateDoubleAttack",
            "bsr.w   battlesceneScript_ValidateCounterAttack",
            "bsr.w   battlesceneScript_End",
        ],
    )
    _require_ordered_fragments(
        root / "battleactionsengine_1.asm",
        [
            "cmpi.w  #BATTLEACTION_ATTACK,(a3)",
            "cmpi.w  #BATTLEACTION_CAST_SPELL,(a3)",
            "cmpi.w  #BATTLEACTION_USE_ITEM,(a3)",
            "cmpi.w  #BATTLEACTION_BURST_ROCK,(a3)",
            "cmpi.w  #BATTLEACTION_MUDDLED,(a3)",
            "cmpi.w  #BATTLEACTION_PRISM_LASER,(a3)",
            "bsr.w   battlesceneScript_SortTargets",
        ],
    )
    _require_ordered_fragments(
        root / "attack.asm",
        [
            "bsr.w   battlesceneScript_DetermineDodge",
            "bsr.w   battlesceneScript_CalculateDamage",
            "bsr.w   battlesceneScript_DetermineCriticalHit",
            "bsr.w   battlesceneScript_InflictDamage",
            "bsr.w   battlesceneScript_InflictAilment",
            "bsr.w   battlesceneScript_InflictCurseDamage",
            "bsr.w   battlesceneScript_DetermineDoubleAndCounter",
        ],
    )
    _require_ordered_fragments(
        root / "breakuseditem.asm",
        [
            "cmpi.w  #BATTLEACTION_USE_ITEM,(a3)",
            "jsr     GetEquipmentType",
            "beq.w   @RemoveItem",
            "btst    #ITEMTYPE_BIT_BREAKABLE,ITEMDEF_OFFSET_TYPE(a0)",
            "btst    #COMBATANT_BIT_ENEMY,(a4)",
            "btst    #ITEMENTRY_BIT_BROKEN,d0",
            "moveq   #CHANCE_TO_BREAK_USED_ITEM,d0",
            "jsr     (GenerateRandomOrDebugNumber).w",
            "jsr     BreakItemBySlot",
            "jsr     RemoveItemBySlot",
        ],
    )
    _require_ordered_fragments(
        root / "useitem.asm",
        [
            "move.b  ITEMDEF_OFFSET_USE_SPELL(a0),d0",
            "andi.w  #SPELLENTRY_MASK_INDEX,d0",
            "lsr.b   #SPELLENTRY_OFFSET_LV,d0",
            "bra.w   battlesceneScript_CastSpell",
        ],
    )
    _require_ordered_fragments(
        root / "determineineffectiveattack.asm",
        [
            "cmpi.b  #BATTLE_VERSUS_TAROS,((CURRENT_BATTLE-$1000000)).w",
            "cmpi.w  #BATTLEACTION_ATTACK,(a3)",
            "cmpi.w  #ENEMY_TAROS,d1",
            "cmpi.w  #ITEM_ACHILLES_SWORD,d1",
            "move.b  #-1,ineffectiveAttackToggle(a2)",
        ],
    )
    _require_ordered_fragments(
        root / "sorttargets.asm",
        [
            "cmpi.w  #ENEMY_BURST_ROCK,d1",
            "ori.b   #COMBATANT_MASK_SORT_BIT,d0",
            "cmp.b   (a0,d1.w),d2",
            "jsr     GetCurrentHp",
            "andi.b  #COMBATANT_MASK_INDEX_AND_ENEMY_BIT,(a0,d7.w)",
        ],
    )
    null_source = (root / "nullsub_BBE4.asm").read_text(encoding="utf-8")
    unused_source = (root / "unused_battleactions.asm").read_text(encoding="utf-8")
    if "nullsub_BBE4:" not in null_source or "OneSecondSleep:" not in unused_source:
        raise ValueError("battle action unused/null helper drift")
    return {
        "engine": {
            "initialZeroedAccumulators": ["exp", "gold", "attack-type"],
            "targetActions": [
                "attack",
                "cast-spell",
                "use-item",
                "burst-rock",
                "muddled",
                "prism-laser",
            ],
            "sortsTargetsAfterConstruction": True,
            "perTargetOrder": ["switch-targets", "apply-effect", "drop-enemy-item"],
            "postTargetsOrder": [
                "actor-idle",
                "break-used-item",
                "validate-double",
                "validate-counter",
                "explode",
                "end",
            ],
            "burstRockExplosionReentersTargetAndActionSetup": True,
        },
        "physicalAttack": {
            "order": [
                "dodge",
                "damage",
                "critical",
                "inflict-damage",
                "ailment",
                "curse-damage",
                "double-counter",
            ],
            "dodgeSkipsDamageCriticalAilmentAndCurse": True,
            "directLethalSkipsAilmentCurseAndFollowups": True,
            "curseLethalSkipsFollowups": True,
        },
        "items": {
            "useItemDelegatesToPackedSpell": True,
            "nonEquipmentConsumedUnconditionally": True,
            "equipmentMustBeBreakableAndAllyUsed": True,
            "alreadyBrokenEquipmentIsDestroyed": True,
            "freshBreakableEquipmentUsesRng": True,
            "breakRngSuccessValue": 0,
        },
        "taros": {
            "battleSpecific": True,
            "allyPhysicalAttackOnly": True,
            "targetEnemy": "Taros",
            "effectiveWeapon": "Achilles Sword",
            "ineffectiveToggleOtherwise": True,
            "transientFlag": 112,
        },
        "targetSort": {
            "primaryOrder": "unsigned combatant byte ascending",
            "burstRockSortBitPlacesAfterOrdinaryTargets": True,
            "burstRockSecondaryOrder": "higher HP before lower HP",
            "sortBitClearedBeforeReturn": True,
        },
        "unused": {
            "nullsubTracked": True,
            "sleepAndNopHelpersTracked": True,
            "notClaimedReachable": True,
        },
    }


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _resolve_upstream(upstream_path: Path) -> tuple[Path, str, dict[str, Any]]:
    upstream_path = upstream_path.resolve(strict=True)
    toolchain = load_json(TOOLCHAIN)
    commit = subprocess.run(
        ["git", "rev-parse", "HEAD"],
        cwd=upstream_path,
        check=True,
        capture_output=True,
        text=True,
        encoding="utf-8",
    ).stdout.strip()
    expected = toolchain["sf2disasm"]["commit"]
    if commit != expected:
        raise ValueError(f"battle-actions inventory requires SF2DISASM {expected}, got {commit}")
    disasm = upstream_path / "disasm"
    if not (disasm / SOURCE_ROOT).is_dir():
        raise ValueError(f"battle-actions source root is missing: {disasm / SOURCE_ROOT}")
    return disasm, commit, toolchain


def build_battle_actions_inventory(upstream_path: Path) -> dict[str, Any]:
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    source_paths = sorted((disasm / SOURCE_ROOT).glob("*.asm"), key=lambda path: path.as_posix())
    files = [_parse_source_file(path, path.relative_to(disasm).as_posix()) for path in source_paths]
    if {Path(row["path"]).name for row in files} != set(REPRESENTATIVE_SYMBOLS):
        raise ValueError("battle-actions source file set drift")
    all_labels = {label for row in files for label in row["globalLabels"]}
    direct_calls: Counter[str] = Counter()
    for row in files:
        for call in row["directCalls"]:
            direct_calls[call["target"]] += call["siteCount"]
    index = load_json(RESEARCH_INDEX)
    records = [r for r in index["records"] if Path(r["sourcePath"]).parent == SOURCE_ROOT]
    summary = {
        "fileCount": len(files),
        "sourceLineCount": sum(row["sourceLineCount"] for row in files),
        "statementCount": sum(row["statementCount"] for row in files),
        "globalLabelCount": sum(len(row["globalLabels"]) for row in files),
        "localLabelCount": sum(row["localLabelCount"] for row in files),
        "directCallSiteCount": sum(direct_calls.values()),
        "indirectCallSiteCount": sum(row["indirectCallSiteCount"] for row in files),
        "uniqueDirectTargetCount": len(direct_calls),
        "internalDirectTargetCount": sum(target in all_labels for target in direct_calls),
        "externalDirectTargetCount": sum(target not in all_labels for target in direct_calls),
        "indexedRecordCount": len(records),
        "indexedFileCount": len({r["sourcePath"] for r in records}),
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "scope": SOURCE_ROOT.as_posix(),
        "summary": summary,
        "indexedRecordIds": sorted(r["id"] for r in records),
        "indexedSourcePaths": sorted({r["sourcePath"] for r in records}),
        "internalDirectCallTargets": sorted(t for t in direct_calls if t in all_labels),
        "externalDirectCallTargets": sorted(t for t in direct_calls if t not in all_labels),
        "actionFacts": _build_action_facts(disasm),
        "files": files,
    }


def verify_battle_actions_inventory(
    upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_battle_actions_inventory(upstream_path)
    validate_json(output, SCHEMA, owner="battle-actions static inventory")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != load_json(ROM_MANIFEST)["hashes"]["sha256"]
    ):
        raise ValueError("battle-actions fixture provenance drift")
    if output["summary"] != manifest["summary"]:
        raise ValueError("battle-actions static summary drift")
    by_name = {Path(row["path"]).name: row for row in output["files"]}
    for filename, symbol in fixture["expected"]["representativeSymbols"].items():
        if symbol not in by_name[filename]["globalLabels"]:
            raise ValueError(f"battle-actions representative symbol drift: {filename}::{symbol}")
    if output["actionFacts"] != fixture["expected"]["actionFacts"]:
        raise ValueError("battle-actions model drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"]:
        raise ValueError(
            "battle-actions static hash mismatch: expected "
            f"{manifest['outputSha256']}, got {digest}"
        )
    destination = output_path or repo_path("local/derived/battle-actions-static.json")
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
