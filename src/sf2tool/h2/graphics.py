from __future__ import annotations

import hashlib
import json
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_ai import _parse_source_file
from sf2tool.h2.battle_scene_animations import _listing_address
from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.h2.battlefield import _require_ordered_fragments
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path

ID = "sf2-tech-graphics-static-v1"
SOURCE_ROOT = Path("code/common/tech/graphics")
MANIFEST = repo_path("manifests/extractions/tech-graphics-static.json")
SCHEMA = repo_path("schemas/tech-graphics-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/tech-graphics-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-tech-graphics-static-fixture.schema.json")
RESEARCH_INDEX = repo_path("manifests/research-index.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")
TOOLCHAIN_MANIFEST = repo_path("manifests/toolchain.json")


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _graphics_facts(disasm: Path) -> dict[str, Any]:
    root = disasm / SOURCE_ROOT
    _require_ordered_fragments(
        root / "decompression.asm",
        [
            "LoadBasicCompressedData:",
            "movea.l a1,a3",
            "move.l  a1,d0",
            "movem.l (sp)+,d1-d2/a0-a3",
            "sub.l   a1,d0",
            "LoadStackCompressedData:",
            "link    a6,#-32",
            "lea     history(a6),a5",
            "move.l  #$40005,(a5)+",
            "move.l  #$E000F,(a5)+",
            "move.l  a1,d0",
            "unlk    a6",
            "movem.l (sp)+,d1-a5",
            "sub.l   a1,d0",
        ],
    )
    _require_ordered_fragments(
        root / "displayinit.asm",
        [
            "dc.w VINTS_DEACTIVATE",
            "bsr.w   WaitForVInt",
            "bsr.w   DisableDisplayAndInterrupts",
            "bsr.w   ClearSpriteTable",
            "move.w  #$8C00,d0",
            "move.w  #$9000,d0",
            "move.w  #$8230,d0",
            "move.w  #$8407,d0",
            "move.w  #$8B00,d0",
            "move.w  #$8D3B,d0",
            "lea     layout_BlackScreen(pc), a0",
            "bsr.w   ApplyImmediateVramDma",
            "lea     sprite_Masks(pc), a0",
            "lea     palette_Base(pc), a0",
        ],
    )
    _require_ordered_fragments(
        root / "graphics_1.asm",
        [
            "lea     (SPRITE_TABLE).l,a0",
            "move.w  #1,d1",
            "move.w  #1,(a0)+",
            "move.w  d1,(a0)+",
            "addq.w  #1,d1",
            "dbf     d0,@Loop",
            "subq.l  #6,a0",
            "clr.w   (a0)",
        ],
    )
    _require_ordered_fragments(
        root / "updatefadingpalette.asm",
        [
            "lea     (PALETTE_1_BASE).l,a1",
            "lea     (PALETTE_1_CURRENT).l,a0",
            "lea     ((PALETTE_1_COPY-$1000000)).w,a1",
            "move.b  #32,((FADING_TIMER_WORD-$1000000)).w",
            "subq.w  #1,d6",
            "move.b  d6,((FADING_TIMER_WORD-$1000000)).w",
            "lsr.w   #2,d6",
            "moveq   #8,d7",
            "jsr     ApplyVIntCramDma(pc)",
            "tst.b   ((FADING_TIMER_BYTE-$1000000)).w",
            "bne.w   UpdateBasePalettesAndBackupCurrent",
        ],
    )
    _require_ordered_fragments(
        root / "specialsprites.asm",
        [
            "move.b  #MAPSPRITES_SPECIALS_END,d0",
            "andi.w  #MAPSPRITE_MASK,d0",
            "movea.l pt_SpecialSprites(pc,d0.w),a0",
            "lea     (PALETTE_4_BASE).l,a1",
            "move.w  rjt_SpecialSpriteFunctions(pc,d1.w),d1",
            "dc.w specialSprite_Battle-rjt_SpecialSpriteFunctions",
            "dc.w specialSprite_Exploration-rjt_SpecialSpriteFunctions",
            "jsr     (LoadStackCompressedData).w",
            "jsr     (ApplyImmediateVramDma).w",
            "AnimateSpecialSprite:",
            "jsr     (ApplyVIntVramDma).w",
            "jsr     (EnableDmaQueueProcessing).w",
        ],
    )
    _require_ordered_fragments(
        root / "display.asm",
        [
            "SetViewDestination:",
            "mulu.w  ((MAP_AREA_LAYER1_PARALLAX_X-$1000000)).w,d0",
            "mulu.w  ((MAP_AREA_LAYER1_PARALLAX_Y-$1000000)).w,d1",
            "mulu.w  ((MAP_AREA_LAYER2_PARALLAX_X-$1000000)).w,d2",
            "mulu.w  ((MAP_AREA_LAYER2_PARALLAX_Y-$1000000)).w,d3",
            "tst.b   ((MAP_AREA_LAYER1_AUTOSCROLL_X-$1000000)).w",
            "tst.b   ((MAP_AREA_LAYER1_AUTOSCROLL_Y-$1000000)).w",
            "tst.b   ((MAP_AREA_LAYER2_AUTOSCROLL_X-$1000000)).w",
            "tst.b   ((MAP_AREA_LAYER2_AUTOSCROLL_Y-$1000000)).w",
        ],
    )
    _require_ordered_fragments(
        root / "flashwhite.asm",
        [
            "script  cs_FlashScreen",
            "cs_FlashScreen: dc.w $41",
            "dc.w $1E",
            "dc.w $FFFF",
        ],
    )
    return {
        "decompression": {
            "entryPoints": ["LoadBasicCompressedData", "LoadStackCompressedData"],
            "sourceRegister": "a0",
            "destinationRegister": "a1",
            "returnsOutputByteCountInD0": True,
            "stackHistoryBytes": 32,
            "stackInitialHistoryWords": list(range(4, 16)),
        },
        "displayInitialization": {
            "deactivatesContextualVintFunctionsFirst": True,
            "waitsForVintBeforeDisablingDisplay": True,
            "clearsSpriteTable": True,
            "configuresH32V32NonInterlacedPlanes": True,
            "loadsBlackScreenImmediately": True,
            "loadsSpriteMasksAndBaseUiPalette": True,
        },
        "sprites": {
            "initializerUsesDbfCounter": True,
            "initializerWritesSequentialLinks": True,
            "initializerTerminatesLastLink": True,
            "battleSpriteLinkHelpersInventoried": True,
        },
        "paletteTransition": {
            "timerFrames": 32,
            "blendStepDivisor": 4,
            "blendWeightTotal": 8,
            "queuesCramDmaEachUpdate": True,
            "canPromoteCopyToNewBaseAtCompletion": True,
        },
        "specialSprites": {
            "dispatchSlotCount": 9,
            "explorationSlotIndex": 2,
            "loadsPalette4": True,
            "initialBattleOrExplorationLoadUsesImmediateDma": True,
            "animationRefreshUsesQueuedDma": True,
        },
        "viewDestination": {
            "appliesSeparatePlaneParallaxFactors": True,
            "autoscrollPreservesCurrentAxisPosition": True,
            "nonAutoscrollWritesDestinationAxis": True,
        },
        "flashScreen": {"scriptWords": [65, 30, 65535]},
        "inventoryBoundary": {
            "unusedDisplayAndGraphicsHelpersInventoried": True,
            "battleTerrainStackCorpusConfirmed": True,
            "battleBackgroundStackCorpusConfirmed": True,
            "battleSpriteStackCorpusConfirmed": True,
            "battleWeaponAndGroundStackCorporaConfirmed": True,
            "mapSpriteBasicCorpusConfirmed": True,
            "portraitStackCorpusConfirmed": True,
            "specialSpriteStackCorpusConfirmed": True,
            "remainingDecompressionCorporaRemainQueued": True,
            "visualAndVdpTimingRemainQueued": True,
            "specialSpriteFramePresentationRemainsQueued": True,
        },
    }


def _index_records_for_source_root(source_paths: set[str]) -> dict[str, Any]:
    """Join every research record whose source belongs to this inventory.

    ``sourcePath`` is deliberately the sole membership rule.  A graphics file
    can carry evidence for another subsystem, so record ID, subsystem, status,
    document, and fixture metadata must not filter this owner join.
    """
    records_by_source_path: dict[str, list[str]] = {}
    for record in load_json(RESEARCH_INDEX)["records"]:
        source_path = record["sourcePath"]
        path = Path(source_path)
        if not path.is_relative_to(SOURCE_ROOT):
            continue
        if ".." in path.parts or source_path != path.as_posix():
            raise ValueError(f"invalid tech graphics indexed source path: {source_path}")
        if source_path not in source_paths:
            raise ValueError(
                "tech graphics indexed source is absent from the discovered root "
                f"inventory: {source_path}"
            )
        records_by_source_path.setdefault(source_path, []).append(record["id"])

    missing_paths = sorted(source_paths - set(records_by_source_path))
    if missing_paths:
        raise ValueError(
            "tech graphics discovered source lacks a research-index record: "
            + ", ".join(missing_paths)
        )
    indexed_records_by_source_path = [
        {"sourcePath": source_path, "recordIds": sorted(record_ids)}
        for source_path, record_ids in sorted(records_by_source_path.items())
    ]
    indexed_record_ids = [
        record_id
        for row in indexed_records_by_source_path
        for record_id in row["recordIds"]
    ]
    if len(indexed_record_ids) != len(set(indexed_record_ids)):
        raise ValueError("tech graphics research-index duplicate record ID")
    return {
        "indexedRecordIds": sorted(indexed_record_ids),
        "indexedSourcePaths": [
            row["sourcePath"] for row in indexed_records_by_source_path
        ],
        "indexedRecordsBySourcePath": indexed_records_by_source_path,
    }


def _verify_indexed_record_join(
    output: dict[str, Any],
    expected_index_membership: dict[str, Any],
    discovered_source_paths: list[str],
) -> None:
    """Reconcile output membership against the authoritative research-index join."""
    relation = output["indexedRecordsBySourcePath"]
    relation_source_paths = [row["sourcePath"] for row in relation]
    relation_record_ids = [
        record_id for row in relation for record_id in row["recordIds"]
    ]
    if len(relation_source_paths) != len(set(relation_source_paths)):
        raise ValueError("tech graphics indexed relation duplicate source path")
    if len(relation_record_ids) != len(set(relation_record_ids)):
        raise ValueError("tech graphics indexed relation duplicate record ID")
    if set(relation_source_paths) != set(discovered_source_paths):
        raise ValueError("tech graphics indexed relation source inventory drift")
    if relation_source_paths != sorted(relation_source_paths):
        raise ValueError("tech graphics indexed relation source order drift")
    if any(row["recordIds"] != sorted(row["recordIds"]) for row in relation):
        raise ValueError("tech graphics indexed relation record order drift")

    indexed_record_ids = output["indexedRecordIds"]
    indexed_source_paths = output["indexedSourcePaths"]
    if indexed_record_ids != sorted(relation_record_ids):
        raise ValueError("tech graphics indexedRecordIds relation drift")
    if indexed_source_paths != relation_source_paths:
        raise ValueError("tech graphics indexedSourcePaths relation order drift")
    summary = output["summary"]
    if summary["indexedRecordCount"] != len(indexed_record_ids) or summary[
        "indexedRecordCount"
    ] != len(relation_record_ids):
        raise ValueError("tech graphics summary indexedRecordCount relation drift")
    if summary["indexedFileCount"] != len(indexed_source_paths) or summary[
        "indexedFileCount"
    ] != len(relation_source_paths):
        raise ValueError("tech graphics summary indexedFileCount relation drift")

    file_paths = [row["path"] for row in output["files"]]
    if len(file_paths) != len(set(file_paths)):
        raise ValueError("tech graphics source inventory duplicate path")
    if file_paths != discovered_source_paths:
        raise ValueError("tech graphics source inventory drift")
    if indexed_source_paths != discovered_source_paths:
        raise ValueError("tech graphics indexedSourcePaths source inventory drift")
    for field in (
        "indexedRecordIds",
        "indexedSourcePaths",
        "indexedRecordsBySourcePath",
    ):
        if output[field] != expected_index_membership[field]:
            raise ValueError(f"tech graphics {field} source-membership drift")


def _verify_fixture_indexed_membership(output: dict[str, Any], expected: dict[str, Any]) -> None:
    """Keep exact golden-corpus checks distinct from source membership."""
    for field in (
        "indexedRecordIds",
        "indexedSourcePaths",
        "indexedRecordsBySourcePath",
    ):
        if output[field] != expected[field]:
            raise ValueError(f"tech graphics fixture {field} drift")


def _verify_fixture_provenance(fixture: dict[str, Any], output: dict[str, Any]) -> None:
    """Derive fixture provenance from independently pinned manifests."""
    toolchain = load_json(TOOLCHAIN_MANIFEST)["sf2disasm"]
    output_upstream = output["upstream"]
    if (
        fixture["upstreamCommit"] != toolchain["commit"]
        or fixture["upstreamCommit"] != output_upstream["commit"]
    ):
        raise ValueError("tech graphics fixture upstream provenance drift")
    if output_upstream["repository"] != toolchain["repository"]:
        raise ValueError("tech graphics output upstream provenance drift")
    if fixture["romSha256"] != load_json(ROM_MANIFEST)["hashes"]["sha256"]:
        raise ValueError("tech graphics fixture ROM provenance drift")


def build_graphics_inventory(upstream_path: Path) -> dict[str, Any]:
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"tech graphics H1 listing is missing: {listing_path}")
    listing = listing_path.read_text(encoding="utf-8")
    paths = sorted((disasm / SOURCE_ROOT).glob("*.asm"), key=lambda path: path.as_posix())
    files = [_parse_source_file(path, path.relative_to(disasm).as_posix()) for path in paths]
    if len(files) != 11:
        raise ValueError(f"tech graphics file-count drift: {len(files)}")
    source_paths = [row["path"] for row in files]
    if len(source_paths) != len(set(source_paths)) or source_paths != sorted(source_paths):
        raise ValueError("tech graphics discovered source inventory drift")
    layout = "\n".join(
        path.read_text(encoding="utf-8")
        for path in sorted((disasm / "layout").glob("*.asm"))
    )
    representative_symbols: dict[str, str] = {}
    representative_addresses: dict[str, int] = {}
    calls: Counter[str] = Counter()
    labels: set[str] = set()
    for row in files:
        path = Path(row["path"])
        relative = path.relative_to(SOURCE_ROOT).as_posix()
        if row["path"].replace("/", "\\") not in layout:
            raise ValueError(f"tech graphics source is absent from layout: {row['path']}")
        if not row["globalLabels"]:
            raise ValueError(f"unexpected unlabeled tech graphics file: {row['path']}")
        symbol = row["globalLabels"][0]
        representative_symbols[relative] = symbol
        representative_addresses[symbol] = _listing_address(listing, symbol)
        labels.update(row["globalLabels"])
        for call in row["directCalls"]:
            calls[call["target"]] += call["siteCount"]
    index_membership = _index_records_for_source_root(set(source_paths))
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
        "layoutIncludedFileCount": len(files),
        "indexedRecordCount": len(index_membership["indexedRecordIds"]),
        "indexedFileCount": len(index_membership["indexedSourcePaths"]),
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "scope": SOURCE_ROOT.as_posix(),
        "summary": summary,
        **index_membership,
        "representativeSymbols": representative_symbols,
        "representativeAddresses": representative_addresses,
        "internalDirectCallTargets": sorted(target for target in calls if target in labels),
        "externalDirectCallTargets": sorted(target for target in calls if target not in labels),
        "graphicsFacts": _graphics_facts(disasm),
        "files": files,
    }


def verify_graphics_inventory(
    upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_graphics_inventory(upstream_path)
    validate_json(output, SCHEMA, owner="tech graphics static inventory")
    disasm, _, _ = _resolve_upstream(upstream_path)
    discovered_source_paths = [
        path.relative_to(disasm).as_posix()
        for path in sorted((disasm / SOURCE_ROOT).glob("*.asm"), key=lambda path: path.as_posix())
    ]
    expected_index_membership = _index_records_for_source_root(
        set(discovered_source_paths)
    )
    _verify_indexed_record_join(
        output, expected_index_membership, discovered_source_paths
    )
    _verify_fixture_indexed_membership(output, fixture["expected"])
    _verify_fixture_provenance(fixture, output)
    if output["summary"] != manifest["summary"]:
        raise ValueError("tech graphics summary drift")
    if output["representativeSymbols"] != fixture["expected"]["representativeSymbols"]:
        raise ValueError("tech graphics representative symbol fixture drift")
    if output["representativeAddresses"] != fixture["function"]:
        raise ValueError("tech graphics H1 address drift")
    if output["graphicsFacts"] != fixture["expected"]["graphicsFacts"]:
        raise ValueError("tech graphics model drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"]:
        raise ValueError("tech graphics canonical hash drift")
    destination = output_path or repo_path("local/derived/tech-graphics-static.json")
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
