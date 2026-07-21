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

ID = "sf2-tech-services-static-v1"
SOURCE_PATHS = (
    Path("code/common/tech/bytecopy.asm"),
    Path("code/common/tech/incbins/s03_incbins_iconhighlight.asm"),
    Path("code/common/tech/incbins/s06_incbins_graphics.asm"),
    Path("code/common/tech/incbins/s06_incbins_textbanktrees.asm"),
    Path("code/common/tech/incbins/s06_incbins_titlescreen.asm"),
    Path("code/common/tech/incbins/s17_incbins_basetiles.asm"),
    Path("code/common/tech/input.asm"),
    Path("code/common/tech/randomnumbergenerator.asm"),
    Path("code/common/tech/sound/music.asm"),
    Path("code/common/tech/sound/sounddriver.asm"),
    Path("code/common/tech/sram/sramfunctions.asm"),
    Path("code/common/tech/thinkingairng.asm"),
)
SOUND_DRIVER_PATH = Path("code/common/tech/sound/sounddriver.asm")
SRAM_SOURCE_PATH = Path("code/common/tech/sram/sramfunctions.asm")
SRAM_CONST_PATH = Path("sf2const.asm")
SRAM_ENUM_PATH = Path("sf2enums.asm")
SRAM_FUNCTIONS = (
    "CheckSram",
    "SaveGame",
    "LoadGame",
    "CopySave",
    "ClearSaveSlotFlag",
    "CopyBytesToSram",
    "CopyBytesFromSram",
)
REPRESENTATIVE_OVERRIDES = {
    "code/common/tech/sram/sramfunctions.asm": "CheckSram",
}
INCBIN_ROOT = Path("code/common/tech/incbins")
MANIFEST = repo_path("manifests/extractions/tech-services-static.json")
SCHEMA = repo_path("schemas/tech-services-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/tech-services-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-tech-services-static-fixture.schema.json")
RESEARCH_INDEX = repo_path("manifests/research-index.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

DEEP_RESOURCE_OWNER_GROUPS = (
    {
        "fixture": "tests/fixtures/h2/text-huffman-static-v1.json",
        "verifier": "src/sf2tool/h2/text_huffman.py",
        "command": "uv run sf2 h2 text-huffman",
        "symbols": ("TextBankTreeData", "TextBankTreeOffsets"),
    },
    {
        "fixture": "tests/fixtures/h2/special-screen-graphics-decode-v1.json",
        "verifier": "src/sf2tool/h2/special_screen_graphics.py",
        "command": "uv run sf2 h2 special-screen-graphics",
        "symbols": ("font_TitleScreen", "tiles_SpeechBalloon"),
    },
    {
        "fixture": "tests/fixtures/h2/variable-width-font-static-v1.json",
        "verifier": "src/sf2tool/h2/variable_width_font.py",
        "command": "uv run sf2 h2 variable-width-font",
        "symbols": ("font_VariableWidth",),
    },
    {
        "fixture": "tests/fixtures/h2/special-screen-presentation-static-v1.json",
        "verifier": "src/sf2tool/h2/special_screen_presentation.py",
        "command": "uv run sf2 h2 special-screen-presentation",
        "symbols": ("palette_TitleScreenFont",),
    },
    {
        "fixture": "tests/fixtures/h2/unused-technical-assets-static-v1.json",
        "verifier": "src/sf2tool/h2/unused_technical_assets.py",
        "command": "uv run sf2 h2 unused-tech-assets",
        "symbols": ("palette_UnusedBase", "tiles_UnusedCloud"),
    },
    {
        "fixture": "tests/fixtures/h2/witch-menu-graphics-static-v1.json",
        "verifier": "src/sf2tool/h2/witch_menu_graphics.py",
        "command": "uv run sf2 h2 witch-menu-graphics",
        "symbols": ("palette_WitchChoice", "table_WitchBubbleAnimation"),
    },
    {
        "fixture": "tests/fixtures/h2/ui-graphics-decode-v1.json",
        "verifier": "src/sf2tool/h2/ui_graphics.py",
        "command": "uv run sf2 h2 ui-graphics",
        "symbols": (
            "tiles_Base",
            "tiles_BattleFieldMenu",
            "tiles_CaravanMenu",
            "tiles_ChurchMenu",
            "tiles_DepotMenu",
            "tiles_ItemMenu",
            "tiles_MainMenu",
            "tiles_ShopMenu",
            "tiles_YesNoPrompt",
        ),
    },
    {
        "fixture": "tests/fixtures/h2/icon-graphics-static-v1.json",
        "verifier": "src/sf2tool/h2/icon_graphics.py",
        "command": "uv run sf2 h2 icon-graphics",
        "symbols": ("tiles_IconHighlight",),
    },
)


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _require_fragments(source: str, fragments: tuple[str, ...], owner: str) -> None:
    missing = [fragment for fragment in fragments if fragment not in source]
    if missing:
        raise ValueError(f"{owner} source-shape drift: missing {missing}")


def _require_sram_section(
    source: str, start_marker: str, end_marker: str, fragments: tuple[str, ...]
) -> None:
    start = source.find(start_marker)
    if start < 0:
        raise ValueError(f"SRAM section start drift: {start_marker}")
    end = source.find(end_marker, start + len(start_marker))
    if end < 0:
        raise ValueError(f"SRAM section end drift: {end_marker}")
    section = source[start:end]
    missing = [fragment for fragment in fragments if fragment not in section]
    if missing:
        raise ValueError(f"SRAM section semantic drift at {start_marker}: {missing}")


def _require_sram_ordered_section(
    source: str, start_marker: str, end_marker: str, fragments: tuple[str, ...]
) -> None:
    start = source.find(start_marker)
    end = source.find(end_marker, start + len(start_marker))
    if start < 0 or end < 0:
        raise ValueError(f"SRAM ordered section boundary drift: {start_marker}")
    position = start
    for fragment in fragments:
        position = source.find(fragment, position, end)
        if position < 0:
            raise ValueError(
                f"SRAM ordered section semantic drift at {start_marker}: {fragment}"
            )
        position += len(fragment)


def _read_equ_values(path: Path, names: tuple[str, ...]) -> dict[str, int]:
    source = read_upstream_text(path)
    values: dict[str, int] = {}
    for name in names:
        match = re.search(rf"^{re.escape(name)}:\s+equ\s+(\$[0-9A-F]+|\d+)", source, re.MULTILINE)
        if not match:
            raise ValueError(f"missing SRAM constant: {name}")
        raw = match.group(1)
        values[name] = int(raw[1:], 16) if raw.startswith("$") else int(raw)
    return values


def _sram_facts(disasm: Path, listing: str) -> dict[str, Any]:
    source = read_upstream_text(disasm / SRAM_SOURCE_PATH)
    constants = _read_equ_values(
        disasm / SRAM_CONST_PATH,
        (
            "SRAM_START",
            "SAVE1_DATA",
            "SRAM_STRING",
            "SAVE_FLAGS",
            "SAVE1_CHECKSUM",
            "SAVE2_CHECKSUM",
            "SAVE2_DATA",
        ),
    )
    sizes = _read_equ_values(
        disasm / SRAM_ENUM_PATH,
        (
            "SAVE_FLAGS_SIZE",
            "SAVE_CHECKSUM_SIZE",
            "SRAM_STRING_CHECK_COUNTER",
            "SRAM_STRING_WRITE_COUNTER",
            "SRAM_STRING_LENGTH",
            "SAVE_SLOT_REAL_SIZE",
            "SAVE_SLOT_SIZE",
            "SRAM_BYTES_COUNTER",
        ),
    )
    _require_sram_section(
        source,
        "CheckSram:",
        "; End of function CheckSram",
        (
            "cmpm.b  (a0)+,(a1)+",
            "lea     1(a1),a1 ; skip filler bytes",
            "btst    #1,(SAVE_FLAGS).l",
            "lea     (SAVE2_DATA).l,a0",
            "cmp.b   (SAVE2_CHECKSUM).l,d0",
            "moveq   #1,d1",
            "moveq   #-1,d1",
            "bclr    #1,(SAVE_FLAGS).l",
            "btst    #0,(SAVE_FLAGS).l",
            "lea     (SAVE1_DATA).l,a0",
            "cmp.b   (SAVE1_CHECKSUM).l,d0",
            "moveq   #1,d0",
            "moveq   #-1,d0",
            "bclr    #0,(SAVE_FLAGS).l",
            "lea     (SRAM_START).l,a0",
            "addq.l  #2,a0 ; skip filler bytes",
            "bsr.w   CopyBytesToSram",
            "clr.b   (SAVE_FLAGS).l",
        ),
    )
    _require_sram_ordered_section(
        source,
        "CheckSram:",
        "; End of function CheckSram",
        (
            "cmpm.b  (a0)+,(a1)+",
            "btst    #1,(SAVE_FLAGS).l",
            "btst    #0,(SAVE_FLAGS).l",
            "lea     (SRAM_START).l,a0",
            "bsr.w   CopyBytesToSram",
            "clr.b   (SAVE_FLAGS).l",
        ),
    )
    _require_sram_section(
        source,
        "SaveGame:",
        "; End of function SaveGame",
        (
            "tst.b   d0",
            "lea     (SAVE1_DATA).l,a1",
            "lea     (SAVE1_CHECKSUM).l,a2",
            "lea     (SAVE2_DATA).l,a1",
            "lea     (SAVE2_CHECKSUM).l,a2",
            "move.w  #SAVE_SLOT_REAL_SIZE,d7",
            "bsr.w   CopyBytesToSram",
            "move.b  d0,(a2)",
            "bset    d1,(SAVE_FLAGS).l",
        ),
    )
    _require_sram_ordered_section(
        source,
        "SaveGame:",
        "; End of function SaveGame",
        (
            "tst.b   d0",
            "bne.s   @Slot2",
            "lea     (SAVE1_DATA).l,a1",
            "lea     (SAVE1_CHECKSUM).l,a2",
            "clr.w   d1",
            "bra.s   @Continue",
            "@Slot2:",
            "lea     (SAVE2_DATA).l,a1",
            "lea     (SAVE2_CHECKSUM).l,a2",
            "moveq   #1,d1",
            "bsr.w   CopyBytesToSram",
            "move.b  d0,(a2)",
            "bset    d1,(SAVE_FLAGS).l",
        ),
    )
    _require_sram_section(
        source,
        "LoadGame:",
        "; End of function LoadGame",
        (
            "lea     (COMBATANT_DATA).l,a1",
            "tst.b   d0",
            "bne.s   @Slot2",
            "lea     (SAVE1_DATA).l,a0",
            "clr.w   d1",
            "bra.s   @Continue",
            "@Slot2:",
            "lea     (SAVE2_DATA).l,a0",
            "moveq   #1,d1",
            "move.w  #SAVE_SLOT_REAL_SIZE,d7",
            "bsr.w   CopyBytesFromSram",
        ),
    )
    _require_sram_ordered_section(
        source,
        "LoadGame:",
        "; End of function LoadGame",
        (
            "lea     (COMBATANT_DATA).l,a1",
            "tst.b   d0",
            "bne.s   @Slot2",
            "lea     (SAVE1_DATA).l,a0",
            "clr.w   d1",
            "bra.s   @Continue",
            "@Slot2:",
            "lea     (SAVE2_DATA).l,a0",
            "moveq   #1,d1",
            "move.w  #SAVE_SLOT_REAL_SIZE,d7",
            "bsr.w   CopyBytesFromSram",
        ),
    )
    _require_sram_section(
        source,
        "CopySave:",
        "; End of function CopySave",
        ("bsr.s   LoadGame", "eori.w  #1,d0", "andi.w  #1,d0", "bsr.s   SaveGame"),
    )
    _require_sram_ordered_section(
        source,
        "CopySave:",
        "; End of function CopySave",
        ("bsr.s   LoadGame", "eori.w  #1,d0", "andi.w  #1,d0", "bsr.s   SaveGame"),
    )
    _require_sram_section(
        source,
        "ClearSaveSlotFlag:",
        "; End of function ClearSaveSlotFlag",
        ("tst.b   d0", "bclr    #0,(SAVE_FLAGS).l", "bclr    #1,(SAVE_FLAGS).l"),
    )
    _require_sram_ordered_section(
        source,
        "ClearSaveSlotFlag:",
        "; End of function ClearSaveSlotFlag",
        (
            "tst.b   d0",
            "bne.s   @Slot2",
            "bclr    #0,(SAVE_FLAGS).l",
            "bra.s   @Return",
            "@Slot2:",
            "bclr    #1,(SAVE_FLAGS).l",
        ),
    )
    _require_sram_section(
        source,
        "CopyBytesToSram:",
        "; End of function CopyBytesToSram",
        ("clr.w   d0", "subq.w  #1,d7", "move.b  (a0),(a1)", "add.b   (a0)+,d0", "addq.l  #2,a1"),
    )
    _require_sram_ordered_section(
        source,
        "CopyBytesToSram:",
        "; End of function CopyBytesToSram",
        ("move.b  (a0),(a1)", "add.b   (a0)+,d0", "addq.l  #2,a1"),
    )
    _require_sram_section(
        source,
        "CopyBytesFromSram:",
        "; End of function CopyBytesFromSram",
        ("clr.w   d0", "subq.w  #1,d7", "move.b  (a0),(a1)+", "add.b   (a0),d0", "addq.l  #2,a0"),
    )
    _require_sram_ordered_section(
        source,
        "CopyBytesFromSram:",
        "; End of function CopyBytesFromSram",
        ("move.b  (a0),(a1)+", "add.b   (a0),d0", "addq.l  #2,a0"),
    )

    caller_targets = set(SRAM_FUNCTIONS[:5])
    callers: dict[str, dict[str, int]] = {}
    for path in sorted((disasm / "code").rglob("*.asm"), key=lambda item: item.as_posix()):
        if path.relative_to(disasm) == SRAM_SOURCE_PATH:
            continue
        parsed = _parse_source_file(path, path.relative_to(disasm).as_posix())
        sites = {
            call["target"]: call["siteCount"]
            for call in parsed["directCalls"]
            if call["target"] in caller_targets
        }
        if sites:
            callers[path.relative_to(disasm).as_posix()] = sites

    return {
        "sourcePath": SRAM_SOURCE_PATH.as_posix(),
        "sourceLineCount": len(source.splitlines()),
        "functionEntries": {
            name: _listing_address(listing, name) for name in SRAM_FUNCTIONS
        },
        "constants": {"addresses": constants, "sizes": sizes},
        "layout": {
            "logicalSlotCount": 2,
            "slotSelector": {"zero": "slot1", "nonZero": "slot2"},
            "logicalBytesPerSlot": sizes["SAVE_SLOT_REAL_SIZE"],
            "storedPhysicalByteCountPerSlot": sizes["SAVE_SLOT_REAL_SIZE"],
            "physicalAddressIntervalPerSlot": sizes["SAVE_SLOT_SIZE"],
            "physicalAddressStepPerLogicalByte": 2,
            "fullClearLogicalByteCount": sizes["SRAM_BYTES_COUNTER"] + 1,
            "occupiedFlagBits": {"slot1": 0, "slot2": 1},
        },
        "operations": {
            "checkOrder": ["signature", "slot2", "slot1"],
            "validOccupiedSlotResult": 1,
            "emptySlotResult": 0,
            "invalidOccupiedSlotResult": -1,
            "invalidChecksumClearsOccupiedFlag": True,
            "signatureMismatchInitializesAllLogicalSramBytes": True,
            "initializationWritesSignatureThenClearsSaveFlags": True,
            "saveCopiesCombatantDataThenStoresChecksumThenSetsOccupiedFlag": True,
            "loadCopiesSelectedSlotToCombatantDataWithoutLocalChecksumComparison": True,
            "copyLoadsSelectedSlotThenSavesToOtherSlot": True,
            "clearOnlyClearsSelectedOccupiedFlag": True,
        },
        "checksum": {
            "accumulatorBits": 8,
            "copyToSramAddsSourceByteAfterStore": True,
            "copyFromSramAddsInterleavedSourceByte": True,
            "storedAsByteAtSelectedChecksumAddress": True,
            "checkComparesComputedByteToSelectedChecksumByte": True,
        },
        "externalCallerOccurrences": callers,
        "runtimeQuestions": [
            "sram-signature-and-full-clear-on-real-persistent-media",
            "sram-valid-invalid-checksum-slot-flag-matrix",
            "sram-save-copy-delete-and-reload-persistence-ordering",
            "sram-power-loss-and-partial-write-boundaries",
        ],
    }


def _incbin_targets(disasm: Path, paths: list[Path]) -> dict[str, str]:
    pattern = re.compile(
        r'^([A-Za-z_][A-Za-z0-9_]*):[^;\n]*(?:\n[ \t]*)?incbin[ \t]+"([^"]+)"',
        re.MULTILINE | re.IGNORECASE,
    )
    targets: dict[str, str] = {}
    for path in paths:
        source = read_upstream_text(path)
        labels = re.findall(r"^([A-Za-z_][A-Za-z0-9_]*):", source, re.MULTILINE)
        matches = pattern.findall(source)
        if len(matches) != len(labels):
            raise ValueError(f"technical resource incbin shape drift: {path.name}")
        for label, target in matches:
            if label in targets:
                raise ValueError(f"duplicate technical resource label: {label}")
            if not (disasm / target).is_file():
                raise ValueError(f"missing technical resource target: {target}")
            targets[label] = target.replace("\\", "/")
    return dict(sorted(targets.items()))


def _contains_json_value(value: Any, expected: str) -> bool:
    if value == expected:
        return True
    if isinstance(value, dict):
        return expected in value or any(
            _contains_json_value(item, expected) for item in value.values()
        )
    if isinstance(value, list):
        return any(_contains_json_value(item, expected) for item in value)
    return False


def _deep_resource_ownership(targets: dict[str, str]) -> dict[str, Any]:
    owners: dict[str, dict[str, str]] = {}
    for group in DEEP_RESOURCE_OWNER_GROUPS:
        fixture_path = repo_path(group["fixture"])
        verifier_path = repo_path(group["verifier"])
        if not fixture_path.is_file() or not verifier_path.is_file():
            raise ValueError(f"missing deep technical-resource owner: {group['fixture']}")
        fixture = load_json(fixture_path)
        for symbol in group["symbols"]:
            if symbol in owners:
                raise ValueError(f"duplicate deep technical-resource owner: {symbol}")
            if not _contains_json_value(fixture, symbol):
                raise ValueError(
                    f"deep technical-resource fixture does not identify {symbol}: "
                    f"{group['fixture']}"
                )
            owners[symbol] = {
                "fixture": group["fixture"],
                "verifier": group["verifier"],
                "command": group["command"],
            }
    missing = sorted(set(targets) - set(owners))
    extra = sorted(set(owners) - set(targets))
    if missing or extra:
        raise ValueError(
            f"technical-resource deep ownership drift: missing={missing}, extra={extra}"
        )
    return {
        "ownerFixtureCount": len({row["fixture"] for row in owners.values()}),
        "ownedDirectiveCount": len(owners),
        "unownedDirectiveCount": 0,
        "owners": {symbol: owners[symbol] for symbol in sorted(owners)},
    }


def _service_facts(disasm: Path) -> dict[str, Any]:
    copy_source = read_upstream_text(disasm / "code/common/tech/bytecopy.asm")
    input_source = read_upstream_text(disasm / "code/common/tech/input.asm")
    music_source = read_upstream_text(disasm / "code/common/tech/sound/music.asm")
    sram_source = read_upstream_text(disasm / "code/common/tech/sram/sramfunctions.asm")
    thinking_source = read_upstream_text(disasm / "code/common/tech/thinkingairng.asm")
    _require_fragments(
        copy_source,
        ("cmpa.l  a0,a1", "move.b  (a0)+,(a1)+", "move.b  -(a0),-(a1)"),
        "byte copy",
    )
    _require_fragments(
        input_source,
        ("lea     (DATA1).l,a6", "addq.w  #2,a6", "moveq   #59,d5", "move.l  #179,d5"),
        "input",
    )
    _require_fragments(
        music_source,
        ("SOUND_COMMAND_WAIT_MUSIC_END", "SOUND_COMMAND_GET_D0_PARAMETER", "moveq   #3,d0"),
        "music wait",
    )
    _require_fragments(
        sram_source,
        (
            "SramCheckString:dc.b 'Taguchi New Supra',$FF",
            "move.w  #SAVE_SLOT_REAL_SIZE,d7",
            "addq.l  #2,a0",
            "addq.l  #2,a1",
            "add.b   (a0)+,d0",
            "bclr    #0,(SAVE_FLAGS).l",
            "bclr    #1,(SAVE_FLAGS).l",
        ),
        "SRAM",
    )
    _require_fragments(
        thinking_source,
        ("mulu.w  #541,d7", "addi.w  #12345,d7", "move.b  d7,(a0)"),
        "thinking AI RNG",
    )
    return {
        "byteCopyChoosesBackwardWhenDestinationIsHigher": True,
        "byteCopyChoosesForwardOtherwise": True,
        "inputControllerPortCount": 2,
        "inputStateBytesPerController": 2,
        "inputWaitFrameCounts": [60, 180],
        "musicWaitSleepFrameCount": 3,
        "sramSaveSlotCount": 2,
        "sramUsesInterleavedPhysicalBytes": True,
        "sramChecksumBits": 8,
        "sramClearsInvalidOccupiedSlotFlags": True,
        "thinkingRngMultiplier": 541,
        "thinkingRngIncrement": 12345,
        "thinkingRngStoredStateBits": 8,
    }


def build_service_inventory(upstream_path: Path) -> dict[str, Any]:
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"tech services H1 listing is missing: {listing_path}")
    listing = listing_path.read_text(encoding="utf-8")
    paths = [disasm / path for path in SOURCE_PATHS]
    if not all(path.is_file() for path in paths):
        raise ValueError("tech service source boundary is incomplete")
    files = [_parse_source_file(path, path.relative_to(disasm).as_posix()) for path in paths]
    layout = "\n".join(
        path.read_text(encoding="utf-8") for path in sorted((disasm / "layout").glob("*.asm"))
    )
    main_files = [row for row in files if Path(row["path"]) != SOUND_DRIVER_PATH]
    for row in main_files:
        if row["path"].replace("/", "\\") not in layout:
            raise ValueError(f"tech service source is absent from main layout: {row['path']}")
        if not row["globalLabels"]:
            raise ValueError(f"unexpected unlabeled tech service file: {row['path']}")

    representative_symbols = {
        row["path"]: REPRESENTATIVE_OVERRIDES.get(row["path"], row["globalLabels"][0])
        for row in main_files
    }
    representative_addresses = {
        symbol: _listing_address(listing, symbol) for symbol in representative_symbols.values()
    }
    incbin_paths = [path for path in paths if path.parent.name == INCBIN_ROOT.name]
    incbin_targets = _incbin_targets(disasm, incbin_paths)
    deep_ownership = _deep_resource_ownership(incbin_targets)

    build_script = read_upstream_text(upstream_path / "build/build.bat")
    _require_fragments(
        build_script,
        (
            "asw.exe .\\sounddriver.asm",
            "sounddriver.p ..\\..\\..\\..\\data\\sound\\sounddriver.bin -k -r $0000-$1fff",
        ),
        "sound driver build",
    )
    sound_source = (disasm / SOUND_DRIVER_PATH).read_bytes()
    try:
        sound_source.decode("utf-8")
        sound_encoding = "utf-8"
    except UnicodeDecodeError:
        sound_encoding = "latin-1"
    sound_blob = disasm / "data/sound/sounddriver.bin"
    sound_blob_bytes = sound_blob.read_bytes()
    sound_driver_facts = {
        "sourceCpu": "z80",
        "sourceEncoding": sound_encoding,
        "assembler": "asw",
        "binaryPath": "data/sound/sounddriver.bin",
        "binarySize": len(sound_blob_bytes),
        "binarySha256": hashlib.sha256(sound_blob_bytes).hexdigest().upper(),
        "buildRangeStart": 0,
        "buildRangeEndInclusive": 8191,
        "romBlobSymbol": "SoundDriver",
        "romBlobAddress": _listing_address(listing, "SoundDriver"),
        "mainLayoutUsesGeneratedBinary": (
            'SoundDriver:    incbin "data/sound/sounddriver.bin"' in layout
        ),
    }

    source_paths = {path.as_posix() for path in SOURCE_PATHS}
    records = [
        record
        for record in load_json(RESEARCH_INDEX)["records"]
        if record["sourcePath"] in source_paths
    ]
    calls: Counter[str] = Counter()
    labels = {label for row in main_files for label in row["globalLabels"]}
    for row in main_files:
        for call in row["directCalls"]:
            calls[call["target"]] += call["siteCount"]
    summary = {
        "fileCount": len(files),
        "incbinFileCount": len(incbin_paths),
        "soundFileCount": 2,
        "sramFileCount": 1,
        "coreServiceFileCount": 4,
        "sourceLineCount": sum(row["sourceLineCount"] for row in files),
        "statementCount": sum(row["statementCount"] for row in files),
        "globalLabelCount": sum(len(row["globalLabels"]) for row in files),
        "localLabelCount": sum(row["localLabelCount"] for row in files),
        "mainLayoutIncludedFileCount": len(main_files),
        "auxiliaryBuildFileCount": 1,
        "m68kDirectCallSiteCount": sum(calls.values()),
        "m68kInternalDirectTargetCount": sum(target in labels for target in calls),
        "m68kExternalDirectTargetCount": sum(target not in labels for target in calls),
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
        "resourceFacts": {
            "incbinDirectiveCount": len(incbin_targets),
            "layoutSectionNumbers": [3, 6, 17],
            "targets": incbin_targets,
            "deepOwnership": deep_ownership,
        },
        "soundDriverFacts": sound_driver_facts,
        "serviceFacts": _service_facts(disasm),
        "sramFacts": _sram_facts(disasm, listing),
        "runtimeQuestions": [
            "input-hardware-and-repeat-timing",
            "sram-persistence-and-corruption-matrix",
            "z80-mailbox-channel-and-audio-timing",
            "thinking-rng-caller-distribution-and-delay",
            "unused-technical-resource-raw-reach-and-presentation",
        ],
        "files": files,
    }


def verify_service_inventory(
    upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_service_inventory(upstream_path)
    validate_json(output, SCHEMA, owner="tech services static inventory")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != load_json(ROM_MANIFEST)["hashes"]["sha256"]
    ):
        raise ValueError("tech services provenance drift")
    if output["summary"] != manifest["summary"]:
        raise ValueError("tech services summary drift")
    if output["representativeAddresses"] != fixture["function"]:
        raise ValueError("tech services H1 address drift")
    for field in (
        "resourceFacts",
        "soundDriverFacts",
        "serviceFacts",
        "sramFacts",
        "runtimeQuestions",
    ):
        if output[field] != fixture["expected"][field]:
            raise ValueError(f"tech services {field} drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"]:
        raise ValueError("tech services canonical hash drift")
    destination = output_path or repo_path("local/derived/tech-services-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Inventory": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Files": output["summary"]["fileCount"],
        "ResourceEntries": output["resourceFacts"]["incbinDirectiveCount"],
        "IndexedFiles": output["summary"]["indexedFileCount"],
        "RuntimeQuestions": len(output["runtimeQuestions"]),
        "Status": "PASS",
    }
