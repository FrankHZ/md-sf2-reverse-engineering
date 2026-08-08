from __future__ import annotations

import hashlib
import json
import re
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_ai import _parse_source_file
from sf2tool.h2.battle_scene_animations import _listing_address
from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.h2.battlefield import _require_ordered_fragments
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path

ID = "sf2-common-scripting-static-v1"
SOURCE_ROOT = Path("code/common/scripting")
UNLABELED_DATA_PATH = SOURCE_ROOT / "text/unused_textfunctionsdata.asm"
MANIFEST = repo_path("manifests/extractions/common-scripting-static.json")
SCHEMA = repo_path("schemas/common-scripting-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/common-scripting-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-common-scripting-static-fixture.schema.json")
RESEARCH_INDEX = repo_path("manifests/research-index.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")
TOOLCHAIN_MANIFEST = repo_path("manifests/toolchain.json")


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _relative_jump_table(path: Path, label: str) -> list[str]:
    lines = path.read_text(encoding="utf-8").splitlines()
    try:
        start = next(index for index, line in enumerate(lines) if line.strip() == f"{label}:")
    except StopIteration:
        raise ValueError(f"missing scripting jump table {label} in {path.name}") from None
    targets: list[str] = []
    for line in lines[start + 1 :]:
        if re.match(r"^[A-Za-z0-9_@]+:", line):
            break
        if not line.strip() or line.lstrip().startswith(";"):
            continue
        target = re.search(r"dc\.w\s+\(?([A-Za-z0-9_]+)-", line)
        if not target:
            raise ValueError(f"unparsed scripting jump-table row in {path.name}: {line}")
        targets.append(target.group(1))
    if not targets:
        raise ValueError(f"empty scripting jump table {label} in {path.name}")
    return targets


def _unlabeled_data_fact(path: Path) -> dict[str, Any]:
    source = path.read_text(encoding="utf-8")
    address_range = re.search(r"; 0x(?P<start>[0-9A-F]+)\.\.0x(?P<end>[0-9A-F]+)", source)
    if not address_range:
        raise ValueError("unused text data ROM range is missing")
    start = int(address_range.group("start"), 16)
    end = int(address_range.group("end"), 16)
    directives = len(re.findall(r"^\s*dc\.b\s+", source, re.MULTILINE))
    if directives != end - start:
        raise ValueError("unused text data byte count does not match annotated ROM range")
    return {
        "path": UNLABELED_DATA_PATH.as_posix(),
        "startAddress": start,
        "endAddressExclusive": end,
        "sizeBytes": end - start,
        "byteDirectiveCount": directives,
        "hasGlobalLabel": False,
        "excludedFromStrictSymbolReach": True,
    }


def _index_records_for_source_root(source_paths: set[str]) -> dict[str, Any]:
    """Join every research record whose source is recursively under this owner.

    A common-scripting source can be evidence for a map, entity, text, or
    runtime owner.  Membership therefore depends exclusively on the canonical
    recursive source path; record ID, subsystem, document, and evidence level
    do not participate in this inventory-owner join.
    """
    records_by_source_path: dict[str, list[str]] = {}
    for record in load_json(RESEARCH_INDEX)["records"]:
        source_path = record["sourcePath"]
        path = Path(source_path)
        if not path.is_relative_to(SOURCE_ROOT):
            continue
        if ".." in path.parts or source_path != path.as_posix():
            raise ValueError(f"invalid common scripting indexed source path: {source_path}")
        if source_path not in source_paths:
            raise ValueError(
                "common scripting indexed source is absent from the discovered root "
                f"inventory: {source_path}"
            )
        records_by_source_path.setdefault(source_path, []).append(record["id"])

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
        raise ValueError("common scripting research-index duplicate record ID")
    return {
        "indexedRecordIds": sorted(indexed_record_ids),
        "indexedSourcePaths": [
            row["sourcePath"] for row in indexed_records_by_source_path
        ],
        "indexedRecordsBySourcePath": indexed_records_by_source_path,
    }


def _verify_indexed_record_join(output: dict[str, Any]) -> None:
    """Reject schema-valid drift between the recursive index join's fields."""
    relation = output["indexedRecordsBySourcePath"]
    relation_source_paths = [row["sourcePath"] for row in relation]
    relation_record_ids = [record_id for row in relation for record_id in row["recordIds"]]
    if len(relation_source_paths) != len(set(relation_source_paths)):
        raise ValueError("common scripting indexed relation duplicate source path")
    if len(relation_record_ids) != len(set(relation_record_ids)):
        raise ValueError("common scripting indexed relation duplicate record ID")
    if relation_source_paths != sorted(relation_source_paths):
        raise ValueError("common scripting indexed relation source order drift")
    if any(row["recordIds"] != sorted(row["recordIds"]) for row in relation):
        raise ValueError("common scripting indexed relation record order drift")

    indexed_record_ids = output["indexedRecordIds"]
    indexed_source_paths = output["indexedSourcePaths"]
    if indexed_record_ids != sorted(relation_record_ids):
        raise ValueError("common scripting indexedRecordIds relation drift")
    if indexed_source_paths != relation_source_paths:
        raise ValueError("common scripting indexedSourcePaths relation order drift")
    file_paths = [row["path"] for row in output["files"]]
    if len(file_paths) != len(set(file_paths)) or file_paths != sorted(file_paths):
        raise ValueError("common scripting source inventory path order drift")
    if not set(indexed_source_paths).issubset(file_paths):
        raise ValueError("common scripting indexedSourcePaths source inventory drift")
    unlabeled_paths = [row["path"] for row in output["files"] if not row["globalLabels"]]
    if set(file_paths) - set(indexed_source_paths) != set(unlabeled_paths):
        raise ValueError("common scripting indexedSourcePaths unlabeled relation drift")

    summary = output["summary"]
    if summary["indexedRecordCount"] != len(indexed_record_ids) or summary[
        "indexedRecordCount"
    ] != len(relation_record_ids):
        raise ValueError("common scripting summary indexedRecordCount relation drift")
    if summary["indexedFileCount"] != len(indexed_source_paths) or summary[
        "indexedFileCount"
    ] != len(relation_source_paths):
        raise ValueError("common scripting summary indexedFileCount relation drift")
    if summary["unlabeledFileCount"] != len(file_paths) - len(indexed_source_paths):
        raise ValueError("common scripting summary unlabeledFileCount relation drift")


def _verify_fixture_provenance(fixture: dict[str, Any], output: dict[str, Any]) -> None:
    """Derive fixture provenance from the independently pinned owners."""
    toolchain = load_json(TOOLCHAIN_MANIFEST)["sf2disasm"]
    fixture_commit = fixture["upstreamCommit"]
    output_upstream = output["upstream"]
    if fixture_commit != toolchain["commit"] or fixture_commit != output_upstream["commit"]:
        raise ValueError("common scripting fixture upstream provenance drift")
    if output_upstream["repository"] != toolchain["repository"]:
        raise ValueError("common scripting output upstream provenance drift")
    if fixture["romSha256"] != load_json(ROM_MANIFEST)["hashes"]["sha256"]:
        raise ValueError("common scripting fixture ROM provenance drift")


def _core_facts(disasm: Path, map_targets: list[str], entity_targets: list[str]) -> dict[str, Any]:
    root = disasm / SOURCE_ROOT
    map_engine = root / "map/mapscriptengine_2.asm"
    entity_engine = root / "entity/entityscriptengine_2.asm"
    _require_ordered_fragments(
        map_engine,
        [
            "move.l  #FF9004_LOADING_SPACE,(dword_FFB1A4).l",
            "movea.l a0,a6",
            "clr.b   ((SKIP_CUTSCENE_TEXT-$1000000)).w",
            "btst    #INPUT_BIT_START,((PLAYER_2_INPUT-$1000000)).w",
            "tst.b   (DEBUG_MODE_TOGGLE).l",
            "move.b  #-1,((SKIP_CUTSCENE_TEXT-$1000000)).w",
            "move.w  (a6)+,d0",
            "cmpi.w  #-1,d0",
            "tst.w   d0",
            "andi.w  #BYTE_MASK,d0",
            "jsr     (Sleep).w",
            "add.w   d0,d0",
            "move.w  rjt_cutsceneScriptCommands(pc,d0.w),d0",
            "jsr     rjt_cutsceneScriptCommands(pc,d0.w)",
            "tst.w   ((DIALOGUE_WINDOW_INDEX-$1000000)).w",
            "jsr     (WaitForViewScrollEnd).w",
            "clr.w   ((VIEW_SCROLLING_SPEED-$1000000)).w",
        ],
    )
    _require_ordered_fragments(
        entity_engine,
        [
            "clr.b   ((SPRITES_TO_LOAD_NUMBER-$1000000)).w",
            "lea     ((ENTITY_DATA-$1000000)).w,a0",
            "moveq   #ENTITIES_COUNTER,d7",
            "cmpi.w  #$7000,d0",
            "bsr.w   UpdateEntityData",
            "move.l  ENTITYDEF_OFFSET_ACTSCRIPTADDR(a0),d0",
            "move.w  (a1),d2",
            "add.w   d2,d2",
            "move.w  rjt_EntityScriptCommands(pc,d2.w),d2",
            "jmp     rjt_EntityScriptCommands(pc,d2.w)",
        ],
    )
    _require_ordered_fragments(
        root / "text/textfunctions_1.asm",
        [
            "bsr.w   CreateDialogueWindow",
            "move.b  #1,((CURRENTLY_TYPEWRITING-$1000000)).w",
            "lsr.w   #6,d0",
            "andi.b  #$FC,d0",
            "movea.l (p_pt_TextBanks).l,a0",
            "andi.w  #BYTE_MASK,d0",
            "move.b  (a0)+,((COMPRESSED_STRING_LENGTH-$1000000)).w",
            "jsr     j_InitializeHuffmanDecoder",
            "bsr.w   GetNextTextSymbol",
            "cmpi.b  #$FE,d0",
            "cmpi.b  #$EE,d0",
            "clr.b   ((CURRENTLY_TYPEWRITING-$1000000)).w",
        ],
    )
    _require_ordered_fragments(
        root / "text/decoding.asm",
        [
            "move.b  #$FE,(DECODED_TEXT_SYMBOL).l",
            "clr.w   (STRING_BIT_COUNTER).l",
            "clr.w   (STRING_BYTE).l",
            "move.b  2(a3),d1",
            "lea     TextBankTreeOffsets(pc), a1",
            "lea     TextBankTreeData(pc), a1",
            "move.b  -1(a2,d5.w),d0",
            "move.w  d6,(a3)",
            "move.w  d7,-2(a3)",
            "move.b  d0,2(a3)",
        ],
    )
    map_counts = Counter(map_targets)
    entity_counts = Counter(entity_targets)
    return {
        "mapScript": {
            "commandCount": len(map_targets),
            "uniqueTargetCount": len(map_counts),
            "terminator": 65535,
            "negativeWordSleepsLowByte": True,
            "debugPlayer2StartSkipsDialogueAndSleep": True,
            "nonnegativeCommandsUseWordIndex": True,
            "waitsForViewScrollWhenDialogueWindowOpen": True,
            "clearsViewScrollingSpeedOnReturn": True,
            "doNothingIndices": [
                index for index, target in enumerate(map_targets) if target == "csc_doNothing"
            ],
        },
        "entityScript": {
            "commandCount": len(entity_targets),
            "uniqueTargetCount": len(entity_counts),
            "emptySlotCoordinateThreshold": 28672,
            "requiresNonzeroActscriptPointer": True,
            "dispatchesOneWordCommandPerEntityVint": True,
            "fillerTarget": "esc_goToNextEntity",
            "fillerIndices": [
                index
                for index, target in enumerate(entity_targets)
                if target == "esc_goToNextEntity"
            ],
        },
        "text": {
            "textBankPointerIndexUsesStringIndexDiv256Times4": True,
            "withinBankIndexMask": 255,
            "compressedStringLengthPrefixBytes": 1,
            "huffmanInitialPreviousSymbol": 254,
            "symbolsAtOrAbove238AreControlCodes": True,
            "terminatorSymbol": 254,
            "decoderTreeSelectedByPreviousSymbol": True,
            "decoderPersistsBitBarrelAndPreviousSymbol": True,
        },
        "inventoryBoundary": {
            "endCreditsInventoried": True,
            "entityMapAndTextHelpersInventoried": True,
            "renderingTimingAndCallerDependentMeaningRemainQueued": True,
        },
    }


def build_scripting_inventory(upstream_path: Path) -> dict[str, Any]:
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"common scripting H1 listing is missing: {listing_path}")
    listing = listing_path.read_text(encoding="utf-8")
    paths = sorted((disasm / SOURCE_ROOT).rglob("*.asm"), key=lambda path: path.as_posix())
    files = [_parse_source_file(path, path.relative_to(disasm).as_posix()) for path in paths]
    if len(files) != 29:
        raise ValueError(f"common scripting file-count drift: {len(files)}")
    representative_symbols: dict[str, str] = {}
    representative_addresses: dict[str, int] = {}
    calls: Counter[str] = Counter()
    labels: set[str] = set()
    for row in files:
        for call in row["directCalls"]:
            calls[call["target"]] += call["siteCount"]
        labels.update(row["globalLabels"])
        if row["globalLabels"]:
            relative = Path(row["path"]).relative_to(SOURCE_ROOT).as_posix()
            symbol = row["globalLabels"][0]
            representative_symbols[relative] = symbol
            representative_addresses[symbol] = _listing_address(listing, symbol)
        elif row["path"] != UNLABELED_DATA_PATH.as_posix():
            raise ValueError(f"unexpected unlabeled common scripting file: {row['path']}")
    indexed_records = _index_records_for_source_root({row["path"] for row in files})
    map_targets = _relative_jump_table(
        disasm / SOURCE_ROOT / "map/mapscriptengine_2.asm", "rjt_cutsceneScriptCommands"
    )
    entity_targets = _relative_jump_table(
        disasm / SOURCE_ROOT / "entity/entityscriptengine_2.asm", "rjt_EntityScriptCommands"
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
        "indexedRecordCount": len(indexed_records["indexedRecordIds"]),
        "indexedFileCount": len(indexed_records["indexedSourcePaths"]),
        "unlabeledFileCount": sum(not row["globalLabels"] for row in files),
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "scope": SOURCE_ROOT.as_posix(),
        "summary": summary,
        **indexed_records,
        "representativeSymbols": representative_symbols,
        "representativeAddresses": representative_addresses,
        "internalDirectCallTargets": sorted(target for target in calls if target in labels),
        "externalDirectCallTargets": sorted(target for target in calls if target not in labels),
        "dispatchTables": {"mapScript": map_targets, "entityScript": entity_targets},
        "coreFacts": _core_facts(disasm, map_targets, entity_targets),
        "unlabeledData": _unlabeled_data_fact(disasm / UNLABELED_DATA_PATH),
        "files": files,
    }


def verify_scripting_inventory(
    upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_scripting_inventory(upstream_path)
    validate_json(output, SCHEMA, owner="common scripting static inventory")
    _verify_indexed_record_join(output)
    _verify_fixture_provenance(fixture, output)
    for field in (
        "indexedRecordIds",
        "indexedSourcePaths",
        "indexedRecordsBySourcePath",
    ):
        if output[field] != fixture["expected"][field]:
            raise ValueError(f"common scripting {field} drift")
    if output["summary"] != manifest["summary"]:
        raise ValueError("common scripting summary drift")
    if output["representativeAddresses"] != fixture["function"]:
        raise ValueError("common scripting H1 address drift")
    if output["coreFacts"] != fixture["expected"]["coreFacts"]:
        raise ValueError("common scripting model drift")
    if output["unlabeledData"] != fixture["expected"]["unlabeledData"]:
        raise ValueError("common scripting unlabeled-data drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"]:
        raise ValueError("common scripting canonical hash drift")
    destination = output_path or repo_path("local/derived/common-scripting-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Inventory": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Files": output["summary"]["fileCount"],
        "MapCommands": output["coreFacts"]["mapScript"]["commandCount"],
        "EntityCommands": output["coreFacts"]["entityScript"]["commandCount"],
        "IndexedRecords": output["summary"]["indexedRecordCount"],
        "UnlabeledFiles": output["summary"]["unlabeledFileCount"],
        "Status": "PASS",
    }
