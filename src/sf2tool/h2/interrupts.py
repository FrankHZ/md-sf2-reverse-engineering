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

ID = "sf2-tech-interrupts-static-v1"
SOURCE_ROOT = Path("code/common/tech/interrupts")
MANIFEST = repo_path("manifests/extractions/tech-interrupts-static.json")
SCHEMA = repo_path("schemas/tech-interrupts-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/tech-interrupts-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-tech-interrupts-static-fixture.schema.json")
RESEARCH_INDEX = repo_path("manifests/research-index.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _interrupt_facts(disasm: Path) -> dict[str, Any]:
    root = disasm / SOURCE_ROOT
    _require_ordered_fragments(
        root / "vint.asm",
        [
            "bclr    #ENABLE_VINT,(VINT_PARAMETERS).l",
            "beq.s   @SkipUpdates",
            "bsr.w   WaitDmaEnd",
            "bsr.w   DisableDisplay",
            "bsr.w   ProcessVdpQueues",
            "bsr.w   EnableDisplayOnVdp",
            "bsr.w   ProcessVramRead",
            "bsr.w   ApplyFadingEffect",
            "bsr.w   ApplyZ80BusUpdates",
            "bsr.w   CallContextualFunctions",
            "clr.b   ((WAITING_NEXT_VINT-$1000000)).w",
            "addq.b  #1,((FRAME_COUNTER-$1000000)).w",
            "move.l  ((AFTER_INTRO_JUMP_POINTER-$1000000)).w,d0",
            "btst    #INPUT_BIT_START,((PLAYER_1_INPUT-$1000000)).w",
        ],
    )
    _require_ordered_fragments(
        root / "vint.asm",
        [
            "cmpi.b  #60,d0",
            "addq.l  #1,((SECONDS_COUNTER-$1000000)).w",
            "lea     ((VINT_FUNC_ADDRS-$1000000)).w,a0",
            "moveq   #7,d7",
            "btst    d6,((VINT_FUNCS_ENABLED_BITFIELD-$1000000)).w",
            "jsr     (a0)",
        ],
    )
    _require_ordered_fragments(
        root / "trap9_contextualfunctions.asm",
        [
            "move.w  rjt_Trap9Actions(pc,d0.w),d0",
            "dc.w trap9_ClearPointers-rjt_Trap9Actions",
            "dc.w trap9_SetFunctionAndTrigger-rjt_Trap9Actions",
            "dc.w trap9_ClearFunctionAndTrigger-rjt_Trap9Actions",
            "dc.w trap9_ClearTrigger-rjt_Trap9Actions",
            "dc.w trap9_SetTrigger-rjt_Trap9Actions",
            "moveq   #7,d7",
            "bset    d1,((VINT_FUNCS_ENABLED_BITFIELD-$1000000)).w",
        ],
    )
    _require_ordered_fragments(
        root / "vintengine_1.asm",
        [
            "WaitForVInt:",
            "bset    #ENABLE_VINT,(VINT_PARAMETERS).l",
            "move.b  #1,((WAITING_NEXT_VINT-$1000000)).w",
            "tst.b   ((WAITING_NEXT_VINT-$1000000)).w",
            "Sleep:",
            "subq.w  #1,d0",
            "bsr.s   WaitForVInt",
        ],
    )
    _require_ordered_fragments(
        root / "vintengine_2.asm",
        [
            "ApplyImmediateVramDma:",
            "move    #$2700,sr",
            "move.w  #$100,(Z80BusReq).l",
            "move.w  #0,(Z80BusReq).l",
            "ApplyVIntVramDma:",
            "sf      ((VINT_ENABLED-$1000000)).w",
            "movea.l (DMA_QUEUE_POINTER).l,a6",
            "move.l  a6,(DMA_QUEUE_POINTER).l",
            "addq.b  #1,(DMA_QUEUE_SIZE).l",
            "move.b  (sp)+,((VINT_ENABLED-$1000000)).w",
        ],
    )
    _require_ordered_fragments(
        root / "vint.asm",
        [
            "ProcessDmaQueue:",
            "bclr    #DMA_REQUEST,(VINT_PARAMETERS).l",
            "btst    #DEACTIVATE_DMA,(VINT_PARAMETERS).l",
            "bsr.w   UpdateVdpSpriteTable",
            "tst.b   (DMA_QUEUE_SIZE).l",
            "subq.b  #1,(DMA_QUEUE_SIZE).l",
            "move.l  #DMA_QUEUE,(DMA_QUEUE_POINTER).l",
        ],
    )
    _require_ordered_fragments(
        root / "fadingcommands.asm",
        [
            "move.b  #IN_FROM_BLACK,((FADING_SETTING-$1000000)).w",
            "move.b  #OUT_TO_BLACK,((FADING_SETTING-$1000000)).w",
            "move.b  #IN_FROM_WHITE,((FADING_SETTING-$1000000)).w",
            "move.b  #OUT_TO_WHITE,((FADING_SETTING-$1000000)).w",
            "clr.w   ((FADING_TIMER_WORD-$1000000)).w",
            "move.b  #%1111,((FADING_PALETTE_BITFIELD-$1000000)).w",
            "bsr.w   WaitForVInt",
            "tst.b   ((FADING_SETTING-$1000000)).w",
            "bsr.w   WaitForVInt",
            "cmpi.w  #$F,d3",
            "cmpi.w  #$F0,d4",
            "cmpi.w  #$F00,d4",
            "bsr.w   ApplyVIntCramDma",
            "bsr.w   EnableDmaQueueProcessing",
        ],
    )
    _require_ordered_fragments(
        root / "applyfadingeffectandz80busupdate.asm",
        [
            "bsr.w   UpdatePlayerInputs",
            "cmp.b   ((LAST_PLAYER_INPUT-$1000000)).w,d0",
            "addq.b  #1,((INPUT_REPEAT_DELAYER-$1000000)).w",
            "cmpi.b  #24,((INPUT_REPEAT_DELAYER-$1000000)).w",
            "clr.b   ((CURRENT_PLAYER_INPUT-$1000000)).w",
            "subq.b  #6,((INPUT_REPEAT_DELAYER-$1000000)).w",
        ],
    )
    _require_ordered_fragments(
        root / "trap0_soundcommand.asm",
        [
            "cmpi.w  #-1,d1",
            "move.w  d0,d1",
            "tst.b   ((SOUND_COMMANDS_DEACTIVATED-$1000000)).w",
            "lea     (SOUND_COMMAND_QUEUE).l,a0",
            "moveq   #3,d0",
            "move.w  d1,-2(a0)",
        ],
    )
    _require_ordered_fragments(
        root / "trap5_textbox.asm",
        [
            "cmpi.w  #-1,d0",
            "bsr.w   CloseDialogueWindow",
            "bsr.w   DisplayText",
        ],
    )
    _require_ordered_fragments(
        root / "trap6_mapscript.asm",
        [
            "trap    #VINT_FUNCTIONS",
            "dc.w VINTS_ACTIVATE",
            "dc.l VInt_UpdateEntities",
            "jsr     j_ExecuteMapScript",
        ],
    )
    return {
        "vint": {
            "updatesRequireEnableBit": True,
            "updateOrder": [
                "wait DMA",
                "disable display",
                "VDP queues",
                "enable display",
                "VRAM read",
                "fade",
                "Z80/input",
                "contextual functions",
            ],
            "clearsWaitingFlagAfterContextualFunctions": True,
            "frameCounterIncrementsEvenWhenUpdatesSkipped": True,
            "startCanJumpOutOfIntroWhenPointerIsSet": True,
        },
        "contextualFunctions": {
            "slotCount": 8,
            "enabledByBitfield": True,
            "secondsCounterFrames": 60,
            "trapActionCount": 5,
            "actions": [
                "clear pointers",
                "set function and trigger",
                "clear function and trigger",
                "clear trigger",
                "set trigger",
            ],
        },
        "waitAndSleep": {
            "waitSetsEnableBit": True,
            "waitSpinsUntilVintClearsFlag": True,
            "sleepZeroReturnsWithoutWaiting": True,
            "positiveSleepWaitsRequestedFrameCount": True,
        },
        "dma": {
            "immediatePathMasksInterruptsAndRequestsZ80Bus": True,
            "queuedPathTemporarilyDisablesVint": True,
            "queuedEntryIncrementsQueueSize": True,
            "processingRequiresRequestUnlessDmaActive": True,
            "spriteTableUpdatesBeforeQueuedTransfers": True,
            "processingResetsQueuePointer": True,
        },
        "fading": {
            "modes": ["in from black", "out to black", "in from white", "out to white"],
            "initialPaletteBitfield": 15,
            "executeWaitsUntilSettingClears": True,
            "executeWaitsOneAdditionalVint": True,
            "colorComponentsClampToNibbleRange": True,
            "queuesCramDmaAfterColorUpdate": True,
        },
        "inputRepeat": {
            "initialDelayFrames": 24,
            "repeatCadenceFrames": 6,
            "unchangedInputSuppressedBeforeDelay": True,
        },
        "traps": {
            "soundQueueSlots": 4,
            "soundMinusOneParameterUsesD0": True,
            "soundDeactivationDropsCommands": True,
            "flagTrapCount": 4,
            "textMinusOneClosesDialogue": True,
            "mapScriptActivatesEntityVintFirst": True,
        },
        "inventoryBoundary": {
            "errorAndHintHandlersInventoried": True,
            "unusedVintAndPalettePathsInventoried": True,
            "hardwareBusAndVdpTimingRemainQueued": True,
            "visualFadeAndQueueCapacityBehaviorRemainQueued": True,
        },
    }


def build_interrupt_inventory(upstream_path: Path) -> dict[str, Any]:
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"tech interrupts H1 listing is missing: {listing_path}")
    listing = listing_path.read_text(encoding="utf-8")
    paths = sorted((disasm / SOURCE_ROOT).glob("*.asm"), key=lambda path: path.as_posix())
    files = [_parse_source_file(path, path.relative_to(disasm).as_posix()) for path in paths]
    if len(files) != 21:
        raise ValueError(f"tech interrupts file-count drift: {len(files)}")
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
        include_path = row["path"].replace("/", "\\")
        if include_path not in layout:
            raise ValueError(f"tech interrupts source is absent from layout: {row['path']}")
        if not row["globalLabels"]:
            raise ValueError(f"unexpected unlabeled tech interrupts file: {row['path']}")
        symbol = row["globalLabels"][0]
        representative_symbols[relative] = symbol
        representative_addresses[symbol] = _listing_address(listing, symbol)
        labels.update(row["globalLabels"])
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
        "layoutIncludedFileCount": len(files),
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
        "representativeSymbols": representative_symbols,
        "representativeAddresses": representative_addresses,
        "internalDirectCallTargets": sorted(target for target in calls if target in labels),
        "externalDirectCallTargets": sorted(target for target in calls if target not in labels),
        "interruptFacts": _interrupt_facts(disasm),
        "files": files,
    }


def verify_interrupt_inventory(
    upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_interrupt_inventory(upstream_path)
    validate_json(output, SCHEMA, owner="tech interrupts static inventory")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != load_json(ROM_MANIFEST)["hashes"]["sha256"]
    ):
        raise ValueError("tech interrupts provenance drift")
    if output["summary"] != manifest["summary"]:
        raise ValueError("tech interrupts summary drift")
    if output["representativeAddresses"] != fixture["function"]:
        raise ValueError("tech interrupts H1 address drift")
    if output["interruptFacts"] != fixture["expected"]["interruptFacts"]:
        raise ValueError("tech interrupts model drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"]:
        raise ValueError("tech interrupts canonical hash drift")
    destination = output_path or repo_path("local/derived/tech-interrupts-static.json")
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
