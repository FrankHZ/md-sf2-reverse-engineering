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

ID = "sf2-special-screens-static-v1"
SOURCE_ROOT = Path("code/specialscreens")
EXPECTED_GROUP_COUNTS = {
    "endkiss": 2,
    "jewelend": 1,
    "segalogo": 2,
    "suspend": 3,
    "title": 3,
    "witch": 5,
    "witchend": 3,
}
MANIFEST = repo_path("manifests/extractions/special-screens-static.json")
SCHEMA = repo_path("schemas/special-screens-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/special-screens-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-special-screens-static-fixture.schema.json")
RESEARCH_INDEX = repo_path("manifests/research-index.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _require_fragments(source: str, fragments: tuple[str, ...], owner: str) -> None:
    missing = [fragment for fragment in fragments if fragment not in source]
    if missing:
        raise ValueError(f"{owner} source-shape drift: missing {missing}")


def _resource_targets(disasm: Path, paths: list[Path]) -> dict[str, str]:
    pattern = re.compile(
        r'^([A-Za-z_][A-Za-z0-9_]*):[^;\n]*(?:\n[ \t]*)?incbin[ \t]+"([^"]+)"',
        re.MULTILINE | re.IGNORECASE,
    )
    targets: dict[str, str] = {}
    for path in paths:
        for label, target in pattern.findall(read_upstream_text(path)):
            if label in targets:
                raise ValueError(f"duplicate special-screen resource label: {label}")
            if not (disasm / target).is_file():
                raise ValueError(f"missing special-screen resource target: {target}")
            targets[label] = target.replace("\\", "/")
    return dict(sorted(targets.items()))


def _screen_facts(sources: dict[str, str], resource_targets: dict[str, str]) -> dict[str, Any]:
    logo0 = sources["code/specialscreens/segalogo/segalogo_0.asm"]
    logo1 = sources["code/specialscreens/segalogo/segalogo_1.asm"]
    title = sources["code/specialscreens/title/title.asm"]
    witch = sources["code/specialscreens/witch/witchstart.asm"]
    sound_test = sources["code/specialscreens/witch/soundtest.asm"]
    suspend = sources["code/specialscreens/suspend/suspend.asm"]
    witch_suspend = sources["code/specialscreens/suspend/witchsuspend.asm"]
    witch_end = sources["code/specialscreens/witchend/witchend.asm"]
    end_kiss = sources["code/specialscreens/endkiss/endkissfunctions_0.asm"]
    _require_fragments(
        logo0,
        ("CalculateRomChecksum", "VInt_CheckConfigurationModeCheat", "DisplaySegaLogo_Quit"),
        "Sega logo",
    )
    _require_fragments(
        logo1,
        ("VInt_CheckDebugModeCheat", "VInt_ActivateDebugModeCheat", "CheckDebugModeInputSequence"),
        "Sega logo debug cheat",
    )
    _require_fragments(
        title,
        ("WaitForPlayer1InputStart:", "TitleScreenLoop1:", "TitleScreenLoop2:", "EndTitleScreen:"),
        "title screen",
    )
    menu_actions = re.findall(r"^(witchMenuAction_[A-Za-z]+):", witch, re.MULTILINE)
    if menu_actions != [
        "witchMenuAction_New",
        "witchMenuAction_Load",
        "witchMenuAction_Copy",
        "witchMenuAction_Del",
    ]:
        raise ValueError("witch menu action routing drift")
    _require_fragments(witch, ("bsr.w   CheckSram", "rjt_WitchMenuActions:"), "witch start")
    if not re.search(r"^j_SoundTest:\s+\n\s*rts\s*$", sound_test, re.MULTILINE):
        raise ValueError("US sound-test stub drift")
    _require_fragments(
        suspend,
        ("moveq   #60,d0", "LoadStackCompressedData", "ApplyVIntVramDma"),
        "suspend screen",
    )
    _require_fragments(
        witch_suspend,
        ("move.w  #600,d0", "INPUT_BIT_START", "movea.l (p_Start).w,a0"),
        "witch suspend",
    )
    _require_fragments(
        witch_end,
        ("VInt_FallingJewels:", "VInt_PerformEndingWitchBlink:"),
        "witch ending",
    )
    _require_fragments(
        end_kiss,
        ("DrawEndingKissPictureWithPixelFilling:", "table_EndingKissPixelFillingData:"),
        "ending kiss",
    )
    graphics_resources = {
        label: target
        for label, target in resource_targets.items()
        if "segalogo" not in target.casefold()
    }
    return {
        "groupFileCounts": EXPECTED_GROUP_COUNTS,
        "resourceEntryCount": len(resource_targets),
        "standaloneGraphicsResourceCount": len(graphics_resources),
        "embeddedSegaLogoResourceCount": len(resource_targets) - len(graphics_resources),
        "segaLogoComputesRomChecksum": True,
        "segaLogoSupportsConfigurationAndDebugCheats": True,
        "segaLogoCanReturnEarlyOnStart": True,
        "titleScrollLoopCount": 2,
        "titleHasBoundedStartPolling": True,
        "witchMenuActions": menu_actions,
        "witchChecksSramBeforeMenu": True,
        "usSoundTestIsReturnOnly": True,
        "suspendInitialSleepFrames": 60,
        "suspendRestartWaitFrames": 600,
        "suspendRestartCanExitEarlyOnStart": True,
        "suspendResetsThroughStartVector": True,
        "endingUsesPixelFillAndFallingJewels": True,
        "compressedTileCorpusConfirmed": True,
    }


def build_special_screen_inventory(upstream_path: Path) -> dict[str, Any]:
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"special-screens H1 listing is missing: {listing_path}")
    listing = listing_path.read_text(encoding="utf-8")
    paths = sorted((disasm / SOURCE_ROOT).rglob("*.asm"), key=lambda path: path.as_posix())
    group_counts = Counter(path.parent.name for path in paths)
    if dict(sorted(group_counts.items())) != EXPECTED_GROUP_COUNTS:
        raise ValueError("special-screen source group drift")
    files = [_parse_source_file(path, path.relative_to(disasm).as_posix()) for path in paths]
    layout = "\n".join(
        path.read_text(encoding="utf-8") for path in sorted((disasm / "layout").glob("*.asm"))
    )
    for row in files:
        if row["path"].replace("/", "\\") not in layout:
            raise ValueError(f"special-screen source is absent from layout: {row['path']}")
        if not row["globalLabels"]:
            raise ValueError(f"unexpected unlabeled special-screen file: {row['path']}")
    representative_symbols = {row["path"]: row["globalLabels"][0] for row in files}
    representative_addresses = {
        symbol: _listing_address(listing, symbol) for symbol in representative_symbols.values()
    }
    records = [
        record
        for record in load_json(RESEARCH_INDEX)["records"]
        if Path(record["sourcePath"]).is_relative_to(SOURCE_ROOT)
    ]
    labels = {label for row in files for label in row["globalLabels"]}
    calls: Counter[str] = Counter()
    for row in files:
        for call in row["directCalls"]:
            calls[call["target"]] += call["siteCount"]
    sources = {path.relative_to(disasm).as_posix(): read_upstream_text(path) for path in paths}
    resource_targets = _resource_targets(disasm, paths)
    summary = {
        "fileCount": len(files),
        "screenGroupCount": len(group_counts),
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
        "scope": SOURCE_ROOT.as_posix(),
        "summary": summary,
        "indexedRecordIds": sorted(record["id"] for record in records),
        "indexedSourcePaths": sorted({record["sourcePath"] for record in records}),
        "representativeSymbols": representative_symbols,
        "representativeAddresses": representative_addresses,
        "internalDirectCallTargets": sorted(target for target in calls if target in labels),
        "externalDirectCallTargets": sorted(target for target in calls if target not in labels),
        "screenFacts": _screen_facts(sources, resource_targets),
        "resourceTargets": resource_targets,
        "runtimeQuestions": [
            "logo-title-cheat-and-input-presentation",
            "witch-save-menu-and-suspend-presentation",
            "ending-kiss-jewels-and-witch-presentation",
        ],
        "files": files,
    }


def verify_special_screen_inventory(
    upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_special_screen_inventory(upstream_path)
    validate_json(output, SCHEMA, owner="special-screens static inventory")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != load_json(ROM_MANIFEST)["hashes"]["sha256"]
    ):
        raise ValueError("special-screens provenance drift")
    if output["summary"] != manifest["summary"]:
        raise ValueError("special-screens summary drift")
    if output["representativeAddresses"] != fixture["function"]:
        raise ValueError("special-screens H1 address drift")
    for field in ("screenFacts", "resourceTargets", "runtimeQuestions"):
        if output[field] != fixture["expected"][field]:
            raise ValueError(f"special-screens {field} drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"]:
        raise ValueError("special-screens canonical hash drift")
    destination = output_path or repo_path("local/derived/special-screens-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Inventory": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Files": output["summary"]["fileCount"],
        "ScreenGroups": output["summary"]["screenGroupCount"],
        "Resources": output["screenFacts"]["resourceEntryCount"],
        "RuntimeQuestions": len(output["runtimeQuestions"]),
        "Status": "PASS",
    }
