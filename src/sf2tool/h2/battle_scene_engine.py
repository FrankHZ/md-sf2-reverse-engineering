from __future__ import annotations

import hashlib
import json
import re
import subprocess
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_ai import _parse_source_file
from sf2tool.h2.battlefield import _require_ordered_fragments
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path

ID = "sf2-battle-scene-engine-static-v1"
SOURCE_ROOT = Path("code/gameflow/battle/battlescenes")
MANIFEST = repo_path("manifests/extractions/battle-scene-engine-static.json")
SCHEMA = repo_path("schemas/battle-scene-engine-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/battle-scene-engine-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-battle-scene-engine-static-fixture.schema.json")
TOOLCHAIN = repo_path("manifests/toolchain.json")
RESEARCH_INDEX = repo_path("manifests/research-index.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

REPRESENTATIVE_SYMBOLS = {
    "battlesceneengine_0.asm": "ExecuteBattlesceneScript",
    "battlesceneengine_1.asm": "GetEnemyAnimation",
    "battlesceneengine_2.asm": "GetBattlesceneGround",
    "battlesceneengine_3.asm": "ExecuteSpellcastFlashEffect",
    "battlesceneengine_4.asm": "VInt_UpdateBattlesceneGraphics",
    "getallyanimation.asm": "GetAllyAnimation",
    "getbattlescenebackground.asm": "GetBattlesceneBackground",
    "getweaponspriteandpalette.asm": "GetWeaponspriteAndPalette",
    "initializebattlescene.asm": "InitializeBattlescene",
    "nullsub_18010.asm": "nullsub_18010",
    "tintscreen.asm": "StoreBattlespritePalette",
    "updatespellanimation.asm": "UpdateSpellanimation",
}


def _jump_table(path: Path, label: str) -> list[str]:
    lines = path.read_text(encoding="utf-8").splitlines()
    try:
        start = next(index for index, line in enumerate(lines) if line.strip() == f"{label}:")
    except StopIteration:
        raise ValueError(f"missing jump table {label} in {path.name}") from None
    targets: list[str] = []
    for line in lines[start + 1 :]:
        if re.match(r"^[A-Za-z0-9_@]+:", line):
            break
        if not line.strip() or line.lstrip().startswith(";"):
            continue
        target = re.search(r"dc\.w\s+([A-Za-z0-9_]+)-", line)
        if not target:
            raise ValueError(f"unparsed jump-table row in {path.name}: {line}")
        targets.append(target.group(1))
    if not targets:
        raise ValueError(f"empty jump table {label} in {path.name}")
    return targets


def _build_scene_facts(disasm: Path) -> dict[str, Any]:
    root = disasm / SOURCE_ROOT
    engine0 = root / "battlesceneengine_0.asm"
    engine2 = root / "battlesceneengine_2.asm"
    updates = root / "updatespellanimation.asm"
    _require_ordered_fragments(
        engine0,
        [
            "lea     (FF0000_RAM_START).l,a6",
            "clr.w   ((DEAD_COMBATANTS_LIST_LENGTH-$1000000)).w",
            "move.b  #-1,((DEAD_COMBATANTS_LIST-$1000000)).w",
            "move.w  (a6)+,d0",
            "cmpi.w  #-1,d0",
            "move.w  rjt_BattlesceneScriptCommands(pc,d0.w),d0",
            "jsr     rjt_BattlesceneScriptCommands(pc,d0.w)",
            "clr.w   d0",
        ],
    )
    _require_ordered_fragments(
        root / "initializebattlescene.asm",
        [
            "lea     ((BATTLESCENE_BACKGROUND_MODIFICATION_POINTER-$1000000)).w,a0",
            "bsr.w   GetBattlespriteAndPalette",
            "bsr.w   GetWeaponspriteAndPalette",
            "bsr.w   GetBattlesceneBackground",
            "bsr.w   LoadBattlesceneBackground",
            "dc.w VINTS_CLEAR",
            "bsr.w   InitializeBattlescenePalettes",
            "bsr.w   LoadBattlesceneBackgroundLayout",
            "bsr.w   DmaBattlesceneEnemyLayout",
            "bsr.w   GetAllyBattlespriteIdleAnimate",
            "bsr.w   GetBattlesceneGround",
            "bsr.w   LoadWeaponsprite",
            "bsr.w   ApplyStatusEffectsToAnimations",
            "dc.l VInt_UpdateBattlesceneGraphics",
            "dc.l VInt_UpdateWindows",
            "bsr.w   FadeInFromBlackIntoBattlescene",
        ],
    )
    _require_ordered_fragments(
        root / "getweaponspriteandpalette.asm",
        [
            "cmpi.w  #COMBATANT_ENEMIES_START,d0",
            "jsr     j_GetEquippedWeapon",
            "cmpi.w  #ITEMINDEX_WEAPONS_START,d1",
            "cmpi.w  #ITEMINDEX_WEAPONS_END,d1",
            "move.w  #-1,d2",
            "move.w  d2,d3",
        ],
    )
    _require_ordered_fragments(
        root / "getbattlescenebackground.asm",
        [
            "cmpi.w  #ENEMY_ZEON,d1",
            "moveq   #BATTLEBACKGROUND_VERSUS_ZEON,d1",
            "lea     table_CustomBackgrounds(pc), a0",
            "cmpi.b  #BATTLEBACKGROUND_OVERWORLD,d1",
            "jsr     j_GetCurrentTerrainType",
            "move.b  table_TerrainBackgrounds(pc,d0.w),d1",
        ],
    )
    _require_ordered_fragments(
        root / "getallyanimation.asm",
        [
            "cmpi.w  #ALLYBATTLESPRITE_KNTE",
            "cmpi.w  #ALLYBATTLESPRITE_PLDN",
            "cmpi.w  #ALLYBATTLESPRITE_PGNT",
            "cmpi.w  #WEAPONSPRITE_SPEAR",
            "cmpi.w  #WEAPONSPRITE_JAVELIN",
            "cmpi.w  #BATTLEANIMATION_DODGE,d1",
            "clr.w   d1",
            "movea.l (p_pt_AllyAnimations).l,a0",
        ],
    )
    _require_ordered_fragments(
        engine2,
        [
            "tst.b   ((UPDATE_SPELLANIMATION_TOGGLE-$1000000)).w",
            "cmpi.b  #-1,d0",
            "lsr.w   #SPELLANIMATION_BITS_VARIANT,d1",
            "btst    #SPELLANIMATION_BIT_MIRRORED,d0",
            "andi.w  #SPELLANIMATION_MASK_INDEX,d0",
            "jmp     rjt_SpellanimationSetups(pc,d0.w)",
        ],
    )
    _require_ordered_fragments(
        updates,
        [
            "tst.b   ((UPDATE_SPELLANIMATION_TOGGLE-$1000000)).w",
            "move.b  ((CURRENT_SPELLANIMATION-$1000000)).w,d7",
            "jmp     rjt_SpellanimationUpdates(pc,d7.w)",
        ],
    )
    commands = _jump_table(engine0, "rjt_BattlesceneScriptCommands")
    setups = _jump_table(engine2, "rjt_SpellanimationSetups")
    update_targets = _jump_table(updates, "rjt_SpellanimationUpdates")
    if len(commands) != 21 or len(setups) != 32 or len(update_targets) != 32:
        raise ValueError("battle-scene dispatch table size drift")
    return {
        "scriptInterpreter": {
            "commandBuffer": 0xFF0000,
            "terminator": 0xFFFF,
            "commandCount": len(commands),
            "commands": commands,
            "clearsDeadListLength": True,
            "seedsDeadListFirstEntry": 255,
            "returnValue": 0,
        },
        "initialization": {
            "clearsSceneDataBlock": True,
            "loadsEnemyThenAllyGraphics": True,
            "backgroundActorPreference": ["enemy", "ally"],
            "clearsExistingVints": True,
            "addsVints": ["VInt_UpdateBattlesceneGraphics", "VInt_UpdateWindows"],
            "optionalLayers": ["enemy", "ally", "ground", "weapon"],
            "loadsStatusAnimationTiles": True,
            "appliesStatusAnimationsBeforeFadeIn": True,
        },
        "selectors": {
            "weaponGraphicsAlliesOnly": True,
            "invalidWeaponSpritePalette": [-1, -1],
            "backgroundPriority": ["Zeon", "battle-custom", "terrain"],
            "missingBackgroundActorFallback": "saved actor then combatant 0",
            "allyDefaultAnimation": "regular attack",
            "allyDodgeUsesDodgeBlock": True,
            "centaurSpearJavelinSpecials": ["KNTE", "PLDN", "PGNT"],
        },
        "spellAnimation": {
            "setupCount": len(setups),
            "updateCount": len(update_targets),
            "setupTargets": setups,
            "updateTargets": update_targets,
            "disabledOrMinusOneSetupReturns": True,
            "variantStoredAsOneBased": True,
            "mirroredBitPreserved": True,
            "updateRequiresToggleAndPhase": True,
        },
        "presentationBoundary": {
            "paletteTintDispatchInventoried": True,
            "graphicsVintInventoried": True,
            "enemyGroundAndPaletteHelpersInventoried": True,
            "nullSubTracked": True,
            "frameTimingAndVdpEffectsRemainQueued": True,
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
        raise ValueError(f"battle-scene engine requires SF2DISASM {expected}, got {commit}")
    disasm = upstream_path / "disasm"
    if not (disasm / SOURCE_ROOT).is_dir():
        raise ValueError(f"battle-scene source root is missing: {disasm / SOURCE_ROOT}")
    return disasm, commit, toolchain


def build_battle_scene_engine_inventory(upstream_path: Path) -> dict[str, Any]:
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    paths = sorted((disasm / SOURCE_ROOT).glob("*.asm"), key=lambda path: path.as_posix())
    files = [_parse_source_file(path, path.relative_to(disasm).as_posix()) for path in paths]
    if {Path(row["path"]).name for row in files} != set(REPRESENTATIVE_SYMBOLS):
        raise ValueError("battle-scene root file set drift")
    labels = {label for row in files for label in row["globalLabels"]}
    calls: Counter[str] = Counter()
    for row in files:
        for call in row["directCalls"]:
            calls[call["target"]] += call["siteCount"]
    records = [
        r
        for r in load_json(RESEARCH_INDEX)["records"]
        if Path(r["sourcePath"]).parent == SOURCE_ROOT
    ]
    summary = {
        "fileCount": len(files),
        "sourceLineCount": sum(r["sourceLineCount"] for r in files),
        "statementCount": sum(r["statementCount"] for r in files),
        "globalLabelCount": sum(len(r["globalLabels"]) for r in files),
        "localLabelCount": sum(r["localLabelCount"] for r in files),
        "directCallSiteCount": sum(calls.values()),
        "indirectCallSiteCount": sum(r["indirectCallSiteCount"] for r in files),
        "uniqueDirectTargetCount": len(calls),
        "internalDirectTargetCount": sum(t in labels for t in calls),
        "externalDirectTargetCount": sum(t not in labels for t in calls),
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
        "internalDirectCallTargets": sorted(t for t in calls if t in labels),
        "externalDirectCallTargets": sorted(t for t in calls if t not in labels),
        "sceneFacts": _build_scene_facts(disasm),
        "files": files,
    }


def verify_battle_scene_engine_inventory(
    upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_battle_scene_engine_inventory(upstream_path)
    validate_json(output, SCHEMA, owner="battle-scene engine static inventory")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != load_json(ROM_MANIFEST)["hashes"]["sha256"]
    ):
        raise ValueError("battle-scene engine provenance drift")
    if output["summary"] != manifest["summary"]:
        raise ValueError("battle-scene engine summary drift")
    by_name = {Path(row["path"]).name: row for row in output["files"]}
    for filename, symbol in fixture["expected"]["representativeSymbols"].items():
        if symbol not in by_name[filename]["globalLabels"]:
            raise ValueError(f"battle-scene representative symbol drift: {filename}::{symbol}")
    if output["sceneFacts"] != fixture["expected"]["sceneFacts"]:
        raise ValueError("battle-scene engine model drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"]:
        raise ValueError("battle-scene engine canonical hash drift")
    destination = output_path or repo_path("local/derived/battle-scene-engine-static.json")
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
