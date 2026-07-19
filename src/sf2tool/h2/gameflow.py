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
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.source_text import read_upstream_text

ID = "sf2-gameflow-core-static-v1"
SOURCE_PATHS = (
    Path("code/gameflow/exploration/exploration.asm"),
    Path("code/gameflow/exploration/explorationfunctions_0.asm"),
    Path("code/gameflow/exploration/explorationfunctions_1.asm"),
    Path("code/gameflow/exploration/explorationfunctions_2.asm"),
    Path("code/gameflow/exploration/explorationvints.asm"),
    Path("code/gameflow/mainloop.asm"),
    Path("code/gameflow/start/basetiles.asm"),
    Path("code/gameflow/start/gameinit.asm"),
    Path("code/gameflow/start/gameintro.asm"),
    Path("code/gameflow/start/gamestart.asm"),
    Path("code/gameflow/start/regioncheck.asm"),
    Path("code/gameflow/start/systeminit.asm"),
    Path("code/gameflow/start/z80init.asm"),
)
MANIFEST = repo_path("manifests/extractions/gameflow-core-static.json")
SCHEMA = repo_path("schemas/gameflow-core-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/gameflow-core-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-gameflow-core-static-fixture.schema.json")
RESEARCH_INDEX = repo_path("manifests/research-index.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _require_fragments(source: str, fragments: tuple[str, ...], owner: str) -> None:
    missing = [fragment for fragment in fragments if fragment not in source]
    if missing:
        raise ValueError(f"{owner} source-shape drift: missing {missing}")


def _startup_facts(sources: dict[str, str]) -> dict[str, Any]:
    start = sources["code/gameflow/start/gamestart.asm"]
    system = sources["code/gameflow/start/systeminit.asm"]
    z80 = sources["code/gameflow/start/z80init.asm"]
    game = sources["code/gameflow/start/gameinit.asm"]
    intro = sources["code/gameflow/start/gameintro.asm"]
    region = sources["code/gameflow/start/regioncheck.asm"]
    base_tiles = sources["code/gameflow/start/basetiles.asm"]
    _require_fragments(
        start,
        (
            "moveq   #23,d1",
            "moveq   #37,d2",
            "dc.w $3FFF",
            "moveq   #31,d3",
            "moveq   #19,d4",
            "moveq   #3,d5",
            "bra.w   InitializeSystem",
        ),
        "cold start",
    )
    _require_fragments(
        system,
        (
            "bsr.s   InitializeVdp",
            "bsr.w   InitializeZ80",
            "bsr.s   InitializeVdpData",
            "jmp     (InitializeGame).l",
            "moveq   #18,d1",
        ),
        "system initialization",
    )
    _require_fragments(
        z80,
        ("#SOUND_DRIVER_BYTE_SIZE", "lea     (SoundDriver).l,a1", "bsr.w   CopyByteToZ80"),
        "Z80 initialization",
    )
    _require_fragments(
        game,
        (
            "bsr.w   LoadBaseTiles",
            "bsr.w   CheckRegion",
            "jsr     j_NewGame",
            "jsr     j_DisplaySegaLogo",
        ),
        "game initialization",
    )
    _require_fragments(
        intro,
        ("jsr     j_PlayIntroOrEndCutscene", "jsr     StartTitleScreen", "movea.l (p_Start).w,a0"),
        "game intro",
    )
    _require_fragments(
        region,
        ("andi.b  #$C0,d0", "cmpi.b  #$80,d0", "@InfiniteLoop:"),
        "region gate",
    )
    _require_fragments(
        base_tiles,
        ("move.w  #4096,d0", "moveq   #2,d1", "ApplyImmediateVramDmaOnCompressedTiles"),
        "base tiles",
    )
    return {
        "coldStartVdpRegisterCount": 24,
        "coldStartZ80BootstrapByteCount": 38,
        "coldStartRamClearBytes": 65536,
        "coldStartCramClearBytes": 128,
        "coldStartVsramClearBytes": 80,
        "coldStartPsgChannelCount": 4,
        "systemVdpRegisterCount": 19,
        "initializeSystemOrder": [
            "InitializeVdp",
            "InitializeZ80",
            "InitializeVdpData",
            "InitializeGame",
        ],
        "z80LoadsGeneratedSoundDriver": True,
        "gameInitializationOrder": ["LoadBaseTiles", "CheckRegion", "NewGame", "DisplaySegaLogo"],
        "segaLogoStartCanSkipIntro": True,
        "titleExitCanResetThroughStartVector": True,
        "allowedRegionHardwareBits": 128,
        "rejectedRegionPathLoopsForever": True,
        "baseTileCount": 4096,
        "baseTileCompressionMode": 2,
    }


def _exploration_facts(sources: dict[str, str]) -> dict[str, Any]:
    main = sources["code/gameflow/mainloop.asm"]
    functions0 = sources["code/gameflow/exploration/explorationfunctions_0.asm"]
    functions1 = sources["code/gameflow/exploration/explorationfunctions_1.asm"]
    functions2 = sources["code/gameflow/exploration/explorationfunctions_2.asm"]
    actions = sources["code/gameflow/exploration/explorationvints.asm"]
    exploration = sources["code/gameflow/exploration/exploration.asm"]
    _require_fragments(
        main,
        (
            "bsr.w   CheckBattle",
            "cmpi.w  #-1,d7",
            "jsr     j_BattleLoop",
            "jsr     j_ExplorationLoop",
        ),
        "main loop",
    )
    event_matches = re.findall(r"beq\.w\s+ProcessMapEventType(\d)_([A-Za-z0-9_]+)", functions2)
    if [int(number) for number, _ in event_matches] != list(range(1, 7)):
        raise ValueError("exploration map-event dispatch drift")
    _require_fragments(
        functions2,
        (
            "andi.w  #INPUT_C|INPUT_A,d1",
            "HealLivingAndImmortalAllies",
            "j_RunMapSetupInitFunction",
            "WaitForEvent",
            "ProcessPlayerAction",
            "chkFlg  530",
        ),
        "exploration loop",
    )
    _require_fragments(
        functions0,
        (
            "moveq   #$2F,d7",
            "bsr.w   IsFollowerEntity",
            "cmpi.w  #MAP_TILE_SIZE,d5",
            "cmpi.w  #$1800,d3",
            "cmpi.w  #$2C00,d3",
            "cmpi.w  #$3000,d3",
            "cmpi.w  #$3400,d3",
            "cmpi.w  #$1C00,d3",
        ),
        "exploration interaction",
    )
    _require_fragments(
        functions1,
        (
            "j_GetItemBySlotAndHeldItemsNumber",
            "OTHER_FORCE_MEMBERS_LIST",
            "CloseChest",
            "RefillNonChestItem",
        ),
        "exploration item handoff",
    )
    _require_fragments(
        actions,
        (
            "btst    #INPUT_BIT_A,d7",
            "btst    #INPUT_BIT_C,d7",
            "j_CaravanMenu",
            "GetActivatedEntity",
            "CheckArea",
            "j_FieldMenu",
        ),
        "player action",
    )
    _require_fragments(
        exploration,
        ("OpenDoor:", "ToggleRoofOnMapLoad:", "OpenChest:", "UpdateVdpPlaneA:", "UpdateVdpPlaneB:"),
        "exploration engine",
    )
    return {
        "mainLoopChecksMapSwitchBeforeBattle": True,
        "mainLoopNoBattleSentinel": -1,
        "mainLoopReturnsFromBattleThroughMapSwitch": True,
        "mapEventTypeTargets": [target for _, target in event_matches],
        "mapEventTypeCount": len(event_matches),
        "actionButtonPriority": ["A", "C"],
        "actionFallbackOpensFieldMenu": True,
        "activatedEntityCandidateCount": 48,
        "activatedEntitySkipsPlayer": True,
        "activatedEntitySkipsFollowers": True,
        "activatedEntityDistanceLimitUnits": 384,
        "areaBlockKinds": {
            "chest": 6144,
            "generic": 7168,
            "vase": 11264,
            "barrel": 12288,
            "bookshelf": 13312,
        },
        "fullInventoryRefillsMapItem": True,
        "hardcodedPacalonCompletionFlag": 530,
        "explorationOwnsDoorRoofChestAndPlaneUpdates": True,
    }


def build_gameflow_inventory(upstream_path: Path) -> dict[str, Any]:
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"gameflow core H1 listing is missing: {listing_path}")
    listing = listing_path.read_text(encoding="utf-8")
    paths = [disasm / path for path in SOURCE_PATHS]
    if not all(path.is_file() for path in paths):
        raise ValueError("gameflow core source boundary is incomplete")
    files = [_parse_source_file(path, path.relative_to(disasm).as_posix()) for path in paths]
    layout = "\n".join(
        path.read_text(encoding="utf-8") for path in sorted((disasm / "layout").glob("*.asm"))
    )
    for row in files:
        if row["path"].replace("/", "\\") not in layout:
            raise ValueError(f"gameflow core source is absent from layout: {row['path']}")
        if not row["globalLabels"]:
            raise ValueError(f"unexpected unlabeled gameflow core file: {row['path']}")
    representative_symbols = {row["path"]: row["globalLabels"][0] for row in files}
    representative_addresses = {
        symbol: _listing_address(listing, symbol) for symbol in representative_symbols.values()
    }
    source_paths = {path.as_posix() for path in SOURCE_PATHS}
    records = [
        record
        for record in load_json(RESEARCH_INDEX)["records"]
        if record["sourcePath"] in source_paths
    ]
    sources = {path.as_posix(): read_upstream_text(disasm / path) for path in SOURCE_PATHS}
    labels = {label for row in files for label in row["globalLabels"]}
    calls: Counter[str] = Counter()
    for row in files:
        for call in row["directCalls"]:
            calls[call["target"]] += call["siteCount"]
    summary = {
        "fileCount": len(files),
        "startupFileCount": 7,
        "mainLoopFileCount": 1,
        "explorationFileCount": 5,
        "sourceLineCount": sum(row["sourceLineCount"] for row in files),
        "statementCount": sum(row["statementCount"] for row in files),
        "globalLabelCount": sum(len(row["globalLabels"]) for row in files),
        "localLabelCount": sum(row["localLabelCount"] for row in files),
        "directCallSiteCount": sum(calls.values()),
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
        "scopes": [path.as_posix() for path in SOURCE_PATHS],
        "summary": summary,
        "indexedRecordIds": sorted(record["id"] for record in records),
        "indexedSourcePaths": sorted({record["sourcePath"] for record in records}),
        "representativeSymbols": representative_symbols,
        "representativeAddresses": representative_addresses,
        "internalDirectCallTargets": sorted(target for target in calls if target in labels),
        "externalDirectCallTargets": sorted(target for target in calls if target not in labels),
        "startupFacts": _startup_facts(sources),
        "explorationFacts": _exploration_facts(sources),
        "runtimeQuestions": [
            "reset-tmss-region-hardware-matrix",
            "intro-title-skip-and-debug-input-timing",
            "exploration-simultaneous-event-and-action-order",
            "exploration-scroll-door-roof-and-transition-frames",
        ],
        "files": files,
    }


def verify_gameflow_inventory(
    upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_gameflow_inventory(upstream_path)
    validate_json(output, SCHEMA, owner="gameflow core static inventory")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != load_json(ROM_MANIFEST)["hashes"]["sha256"]
    ):
        raise ValueError("gameflow core provenance drift")
    if output["summary"] != manifest["summary"]:
        raise ValueError("gameflow core summary drift")
    if output["representativeAddresses"] != fixture["function"]:
        raise ValueError("gameflow core H1 address drift")
    for field in ("startupFacts", "explorationFacts", "runtimeQuestions"):
        if output[field] != fixture["expected"][field]:
            raise ValueError(f"gameflow core {field} drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"]:
        raise ValueError("gameflow core canonical hash drift")
    destination = output_path or repo_path("local/derived/gameflow-core-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Inventory": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Files": output["summary"]["fileCount"],
        "StartupFiles": output["summary"]["startupFileCount"],
        "ExplorationFiles": output["summary"]["explorationFileCount"],
        "MapEventTypes": output["explorationFacts"]["mapEventTypeCount"],
        "RuntimeQuestions": len(output["runtimeQuestions"]),
        "Status": "PASS",
    }
