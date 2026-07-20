from __future__ import annotations

import hashlib
import json
import posixpath
import re
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_ai import _parse_source_file
from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.source_text import read_upstream_text

ID = "sf2-sound-data-static-v1"
SOURCE_ROOT = Path("data/sound")
DRIVER_SOURCE = Path("code/common/tech/sound/sounddriver.asm")
BANK_SOURCES = {
    "bank0": SOURCE_ROOT / "musicbank0/musicbank0.asm",
    "bank1": SOURCE_ROOT / "musicbank1/musicbank1.asm",
}
BANK_OUTPUTS = {
    "bank0": SOURCE_ROOT / "musicbank0.bin",
    "bank1": SOURCE_ROOT / "musicbank1.bin",
}
BANK_ROM_OFFSETS = {"bank1": 0x1F0000, "bank0": 0x1F8000}
BANK_SIZE = 0x8000
BANK_ORIGIN = 0x8000
FLOW_MACROS = {
    "channel_end",
    "countedLoopEnd",
    "countedLoopStart",
    "mainLoopEnd",
    "mainLoopStart",
    "repeatEnd",
    "repeatSection1Start",
    "repeatSection2Start",
    "repeatSection3Start",
    "repeatStart",
}
MANIFEST = repo_path("manifests/extractions/sound-data-static.json")
SCHEMA = repo_path("schemas/sound-data-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/sound-data-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-sound-data-static-fixture.schema.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _sha256(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest().upper()


def _resolve_include(source_path: str, target: str) -> str:
    normalized = target.replace("\\", "/")
    return posixpath.normpath(posixpath.join(posixpath.dirname(source_path), normalized))


def _song_file_catalog(
    sources: dict[str, str], bank_payloads: dict[str, bytes]
) -> list[dict[str, Any]]:
    rows = []
    header_pattern = re.compile(
        r"^; ASM FILE .+?\n; 0x([0-9A-F]+)\.\.0x([0-9A-F]+) : Music ",
        re.MULTILINE,
    )
    for source_path, source in sorted(sources.items()):
        filename = Path(source_path).name
        match = re.fullmatch(r"music(\d+)\.asm", filename)
        if match is None:
            continue
        music_index = int(match.group(1))
        bank = "bank0" if "/musicbank0/" in source_path else "bank1"
        first_index = 1 if bank == "bank0" else 33
        if not first_index <= music_index < first_index + 32:
            raise ValueError(f"song index is outside {bank}: {music_index}")
        header = header_pattern.search(source)
        if header is None:
            raise ValueError(f"song source range header is missing: {source_path}")
        start = int(header.group(1), 16)
        end = int(header.group(2), 16)
        if not BANK_ORIGIN + 64 <= start < end <= BANK_ORIGIN + BANK_SIZE:
            raise ValueError(f"song source range is outside {bank}: {source_path}")
        symbol = f"Music_{music_index}"
        if not re.search(rf"^{re.escape(symbol)}:", source, re.MULTILINE):
            raise ValueError(f"song entry symbol is missing: {source_path}::{symbol}")
        payload = bank_payloads[bank]
        pointer_offset = (music_index - first_index) * 2
        entry = int.from_bytes(payload[pointer_offset : pointer_offset + 2], "little")
        if not start <= entry < end:
            raise ValueError(f"song entry pointer is outside source range: {source_path}")
        file_payload = payload[start - BANK_ORIGIN : end - BANK_ORIGIN]
        rows.append(
            {
                "id": Path(source_path).stem,
                "sourcePath": source_path,
                "bank": bank,
                "musicIndex": music_index,
                "entrySymbol": symbol,
                "entryZ80Address": entry,
                "entryRomOffset": BANK_ROM_OFFSETS[bank] + entry - BANK_ORIGIN,
                "fileStartZ80Address": start,
                "fileEndZ80Address": end,
                "fileRomOffset": BANK_ROM_OFFSETS[bank] + start - BANK_ORIGIN,
                "sizeBytes": len(file_payload),
                "sourceSha256": _sha256(source.encode()),
                "payloadSha256": _sha256(file_payload),
            }
        )
    if len(rows) != 37:
        raise ValueError(f"sound song-file boundary drift: {len(rows)}")
    for bank in BANK_SOURCES:
        ranges = sorted(
            (row["fileStartZ80Address"], row["fileEndZ80Address"])
            for row in rows
            if row["bank"] == bank
        )
        if not ranges or ranges[0][0] != BANK_ORIGIN + 64:
            raise ValueError(f"{bank} song payload does not start after its pointer table")
        if any(
            left[1] != right[0]
            for left, right in zip(ranges, ranges[1:], strict=False)
        ):
            raise ValueError(f"{bank} song source ranges are not contiguous")
    return rows


def _parse_music_macros(source: str) -> list[dict[str, Any]]:
    rows = []
    pattern = re.compile(
        r"^[ \t]*(?P<name>[A-Za-z_][A-Za-z0-9_]*)[ \t]+macro"
        r"(?:[ \t]+(?P<args>[^\r\n]+))?[ \t]*\r?$"
        r"(?P<body>.*?)^[ \t]*endm[ \t]*\r?$",
        re.MULTILINE | re.DOTALL | re.IGNORECASE,
    )
    for match in pattern.finditer(source):
        arguments = [
            value.strip()
            for value in (match.group("args") or "").split(",")
            if value.strip()
        ]
        byte_expressions = []
        for body_line in match.group("body").splitlines():
            line = body_line.split(";", 1)[0].strip()
            directive = re.match(r"^db\s+(.+)$", line, re.IGNORECASE)
            if directive:
                byte_expressions.extend(
                    value.strip() for value in directive.group(1).split(",")
                )
        if not byte_expressions:
            raise ValueError(f"music macro emits no bytes: {match.group('name')}")
        rows.append(
            {
                "name": match.group("name"),
                "parameters": arguments,
                "emittedByteCount": len(byte_expressions),
                "byteExpressions": byte_expressions,
                "flowControl": match.group("name") in FLOW_MACROS,
            }
        )
    names = [row["name"] for row in rows]
    if len(names) != len(set(names)):
        raise ValueError("duplicate music macro definition")
    return rows


def _song_command_row(source_path: str, source: str, macro_names: set[str]) -> dict[str, Any]:
    invocations: Counter[str] = Counter()
    directive_counts: Counter[str] = Counter()
    unknown: Counter[str] = Counter()
    for raw_line in source.splitlines():
        line = raw_line.split(";", 1)[0].strip()
        if not line:
            continue
        line = re.sub(r"^[A-Za-z_][A-Za-z0-9_]*:\s*", "", line)
        if not line:
            continue
        token = line.split(None, 1)[0]
        if token in macro_names:
            invocations[token] += 1
        elif token.lower() in {"db", "dw"}:
            directive_counts[token.lower()] += 1
        else:
            unknown[token] += 1
    if unknown:
        raise ValueError(f"unknown music source statements in {source_path}: {dict(unknown)}")
    entry_labels = re.findall(r"^(Music_\d+):", source, re.MULTILINE)
    channel_labels = re.findall(r"^(Music_\d+_Channel_\d+):", source, re.MULTILINE)
    channel_pointers = re.findall(
        r"^\s*dw\s+(Music_\d+_Channel_\d+)\s*$", source, re.MULTILINE
    )
    return {
        "sourcePath": source_path,
        "entryLabels": entry_labels,
        "channelLabels": channel_labels,
        "channelPointerCount": len(channel_pointers),
        "uniqueChannelPointerCount": len(set(channel_pointers)),
        "macroInvocationCount": sum(invocations.values()),
        "macroInvocations": dict(sorted(invocations.items())),
        "directiveCounts": dict(sorted(directive_counts.items())),
    }


def _music_command_model(sources: dict[str, str]) -> dict[str, Any]:
    macro_path = (SOURCE_ROOT / "musicmacros.asm").as_posix()
    macro_definitions = _parse_music_macros(sources[macro_path])
    macro_names = {row["name"] for row in macro_definitions}
    songs = [
        _song_command_row(path, sources[path], macro_names)
        for path in sorted(sources)
        if re.search(r"/music\d+\.asm$", path)
    ]
    invocation_counts = Counter(
        {
            name: sum(row["macroInvocations"].get(name, 0) for row in songs)
            for name in macro_names
        }
    )
    unused = sorted(name for name, count in invocation_counts.items() if count == 0)
    return {
        "summary": {
            "macroDefinitionCount": len(macro_definitions),
            "usedMacroCount": len(macro_definitions) - len(unused),
            "unusedMacroCount": len(unused),
            "flowMacroCount": sum(row["flowControl"] for row in macro_definitions),
            "songEntryLabelCount": sum(len(row["entryLabels"]) for row in songs),
            "channelLabelCount": sum(len(row["channelLabels"]) for row in songs),
            "channelPointerCount": sum(row["channelPointerCount"] for row in songs),
            "uniqueChannelPointerCount": sum(
                row["uniqueChannelPointerCount"] for row in songs
            ),
            "macroInvocationCount": sum(invocation_counts.values()),
            "flowInvocationCount": sum(
                invocation_counts[name] for name in FLOW_MACROS
            ),
        },
        "macroDefinitions": macro_definitions,
        "invocationCounts": dict(sorted(invocation_counts.items())),
        "unusedMacros": unused,
        "songs": songs,
    }


def _fixed_opcode_families(
    macro_definitions: list[dict[str, Any]],
) -> tuple[dict[str, list[str]], list[str]]:
    families: dict[str, list[str]] = {}
    dynamic = []
    for row in macro_definitions:
        first = row["byteExpressions"][0]
        match = re.fullmatch(r"0([0-9A-F]+)h", first, re.IGNORECASE)
        if match is None:
            dynamic.append(row["name"])
            continue
        opcode = f"{int(match.group(1), 16):02X}"
        families.setdefault(opcode, []).append(row["name"])
    return (
        {opcode: sorted(names) for opcode, names in sorted(families.items())},
        sorted(dynamic),
    )


def _require_driver_fragments(source: str, fragments: list[str]) -> None:
    cursor = 0
    for fragment in fragments:
        cursor = source.find(fragment, cursor)
        if cursor < 0:
            raise ValueError(f"sound driver instruction sequence drift: {fragment}")
        cursor += len(fragment)


def _music_driver_contract(
    driver_source: str, macro_definitions: list[dict[str, Any]]
) -> dict[str, Any]:
    _require_driver_fragments(
        driver_source,
        [
            "YM1_ParseData:",
            "cp\t0FFh",
            "cp\t0FEh",
            "cp\t0FDh",
            "cp\t0FCh",
            "cp\t0FBh",
            "cp\t0FAh",
            "cp\t0F9h",
            "cp\t0F8h",
            "call\tParseLoopCommand",
            "YM2_ParseData:",
            "call\tParseLoopCommand",
            "YM2_ParseChannel6Data:",
            "call\tParseLoopCommand",
            "PSG_ParseToneData:",
            "call\tPSG_LoadInstrument",
            "call\tSetRelease",
            "call\tLoadVibrato",
            "call\tYM1_Input",
            "call\tLoadNoteShift",
            "call\tParseLoopCommand",
            "PSG_ParseNoiseData:",
            "call\tParseLoopCommand",
            "ParseLoopCommand:",
            "and\t7",
            "cp\t1",
            "cp\t2",
            "cp\t3",
            "cp\t4",
            "cp\t5",
            "cp\t6",
            "and\t1Fh",
            "inc\ta",
            "dec\t(ix+19h)",
        ],
    )
    families, dynamic = _fixed_opcode_families(macro_definitions)
    if len(re.findall(r"call\s+ParseLoopCommand", driver_source)) != 5:
        raise ValueError("sound driver loop-parser caller boundary drift")
    return {
        "sourcePath": DRIVER_SOURCE.as_posix(),
        "sourceSha256": _sha256(driver_source.encode()),
        "channelParsers": [
            "YM1_ParseData",
            "YM2_ParseData",
            "YM2_ParseChannel6Data",
            "PSG_ParseToneData",
            "PSG_ParseNoiseData",
        ],
        "sharedLoopParser": "ParseLoopCommand",
        "sharedLoopParserCallerCount": 5,
        "fixedOpcodeFamilies": families,
        "dynamicFirstByteMacros": dynamic,
        "channelSpecificCommands": {
            "FA": {"ym": "stereo", "psg": "timer-b"},
            "FC": {"ym": "slide-or-key-release", "psg": "key-release"},
            "FD": {"ym": "volume", "psg": "instrument"},
            "FE": {"ym": "instrument", "psg": "unsupported"},
        },
        "ffForms": {
            "ym1": {
                "0000": "end-channel",
                "nonzero-low-00-high": "queue-operation-and-end-channel",
                "nonzero-high": "jump-absolute-z80-address",
            },
            "ym2AndPsg": {
                "0000": "end-channel",
                "nonzero-word": "jump-absolute-z80-address",
            },
        },
        "loopSubcommands": [
            {"parameter": "00", "meaning": "main-loop-start", "stateOffsets": [19, 20]},
            {"parameter": "20", "meaning": "repeat-start", "stateOffsets": [21, 22]},
            {"parameter": "40", "meaning": "repeat-section-1", "stateOffsets": [26]},
            {"parameter": "60", "meaning": "repeat-section-2", "stateOffsets": [27]},
            {"parameter": "80", "meaning": "repeat-section-3-terminator", "stateOffsets": []},
            {"parameter": "A0", "meaning": "repeat-end", "stateOffsets": [21, 22]},
            {"parameter": "A1", "meaning": "main-loop-end", "stateOffsets": [19, 20]},
            {
                "parameter": "C0-DF",
                "meaning": "counted-loop-start-count-low5-plus-1",
                "stateOffsets": [23, 24, 25],
            },
            {"parameter": "E0", "meaning": "counted-loop-end", "stateOffsets": [23, 24, 25]},
        ],
    }


def build_sound_data_inventory(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    expected_rom = load_json(ROM_MANIFEST)
    rom = rom_path.read_bytes()
    if len(rom) != expected_rom["sizeBytes"] or _sha256(rom) != expected_rom["hashes"]["sha256"]:
        raise ValueError("sound data ROM identity drift")

    root = disasm / SOURCE_ROOT
    paths = sorted(root.rglob("*.asm"))
    if len(paths) != 41:
        raise ValueError(f"sound data boundary drift: expected 41 files, got {len(paths)}")
    files = [_parse_source_file(path, path.relative_to(disasm).as_posix()) for path in paths]
    sources = {
        path.relative_to(disasm).as_posix(): read_upstream_text(path) for path in paths
    }

    include_edges = []
    for source_path, source in sources.items():
        for target in re.findall(r'^\s*include\s+"([^"]+)"', source, re.MULTILINE):
            include_edges.append(
                {
                    "sourcePath": source_path,
                    "targetPath": _resolve_include(source_path, target),
                }
            )
    targets = [edge["targetPath"] for edge in include_edges]
    if len(include_edges) != 41 or len(set(targets)) != 39:
        raise ValueError("sound data include graph boundary drift")
    if set(targets) - set(sources):
        raise ValueError("sound bank includes a source outside data/sound")
    entry_paths = sorted(path.as_posix() for path in BANK_SOURCES.values())
    reachable = set(entry_paths)
    outgoing: dict[str, list[str]] = {}
    for edge in include_edges:
        outgoing.setdefault(edge["sourcePath"], []).append(edge["targetPath"])
    queue = list(entry_paths)
    while queue:
        for target in outgoing.get(queue.pop(), []):
            if target not in reachable:
                reachable.add(target)
                queue.append(target)
    if reachable != set(sources):
        raise ValueError("sound bank entry points do not reach the complete source directory")

    bank_facts: dict[str, dict[str, Any]] = {}
    bank_payloads: dict[str, bytes] = {}
    for bank, source_path in BANK_SOURCES.items():
        output_path = disasm / BANK_OUTPUTS[bank]
        if not output_path.is_file():
            raise ValueError(f"generated {bank} binary is missing: {output_path}")
        payload = output_path.read_bytes()
        if len(payload) != BANK_SIZE:
            raise ValueError(f"generated {bank} size drift: {len(payload)}")
        offset = BANK_ROM_OFFSETS[bank]
        rom_payload = rom[offset : offset + BANK_SIZE]
        if payload != rom_payload:
            raise ValueError(f"generated {bank} does not match the canonical ROM slice")
        bank_payloads[bank] = payload
        source = sources[source_path.as_posix()]
        pointers = re.findall(r"^\s*dw\s+(Music_\d+)\s*$", source, re.MULTILINE)
        song_includes = [
            target
            for target in re.findall(r'^\s*include\s+"([^"]+)"', source, re.MULTILINE)
            if target.lower().startswith("music") and target.lower().endswith(".asm")
        ]
        bank_facts[bank] = {
            "sourcePath": source_path.as_posix(),
            "romOffset": offset,
            "sizeBytes": len(payload),
            "sha256": _sha256(payload),
            "pointerSlotCount": len(pointers),
            "uniquePointerTargetCount": len(set(pointers)),
            "songIncludeCount": len(song_includes),
            "romParity": True,
        }

    song_files = _song_file_catalog(sources, bank_payloads)
    song_paths = [row["sourcePath"] for row in song_files]
    command_model = _music_command_model(sources)
    driver_source = read_upstream_text(disasm / DRIVER_SOURCE)
    command_model["driver"] = _music_driver_contract(
        driver_source, command_model["macroDefinitions"]
    )
    summary = {
        "fileCount": len(files),
        "sourceLineCount": sum(row["sourceLineCount"] for row in files),
        "statementCount": sum(row["statementCount"] for row in files),
        "globalLabelCount": sum(len(row["globalLabels"]) for row in files),
        "bankEntryFileCount": len(entry_paths),
        "sharedDefinitionFileCount": 2,
        "songFileCount": len(song_paths),
        "transitiveIncludeFileCount": len(set(targets)),
        "strictH1IndexedFileCount": 0,
        "z80BankBoundSongFileCount": len(song_files),
        "songPayloadBytes": sum(row["sizeBytes"] for row in song_files),
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "rom": {"id": expected_rom["id"], "sha256": expected_rom["hashes"]["sha256"]},
        "scope": SOURCE_ROOT.as_posix(),
        "summary": summary,
        "entryPaths": entry_paths,
        "includeEdges": include_edges,
        "songPaths": song_paths,
        "facts": {
            "assemblyCpu": "z80",
            "bankAddressSpaceOrigin": 0x8000,
            "banks": bank_facts,
            "romLayoutOrder": ["bank1", "bank0"],
            "sourceContentParsed": True,
            "macroAbiParsed": True,
            "musicSemanticsParsed": False,
        },
        "songFiles": song_files,
        "commandModel": command_model,
        "strictIndexExclusion": (
            "song files use explicit z80-music-bank ROM bindings; the two unlabeled bank entry "
            "files and two macro/enum sources remain outside symbol reach"
        ),
        "runtimeQuestions": [
            "note-sample-frequency-and-channel-state-runtime-semantics",
            "tempo-loop-and-instrument-timing",
            "bank-selection-and-cross-bank-fallback-behavior",
        ],
        "files": files,
    }


def verify_sound_data_inventory(
    rom_path: Path,
    upstream_path: Path,
    *,
    output_path: Path | None = None,
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_sound_data_inventory(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="sound data static inventory")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != output["rom"]["sha256"]
    ):
        raise ValueError("sound data provenance drift")
    if output["summary"] != manifest["summary"]:
        raise ValueError("sound data summary drift")
    for field in ("facts", "strictIndexExclusion", "runtimeQuestions"):
        if output[field] != fixture["expected"][field]:
            raise ValueError(f"sound data {field} drift")
    song_rom_offsets = {row["id"]: row["entryRomOffset"] for row in output["songFiles"]}
    if fixture["table"]["songRomOffsets"] != song_rom_offsets:
        raise ValueError("sound data song ROM-offset drift")
    for field in ("summary", "unusedMacros", "invocationCounts", "driver"):
        if output["commandModel"][field] != fixture["expected"]["commandModel"][field]:
            raise ValueError(f"sound data command-model {field} drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"]:
        raise ValueError("sound data canonical hash drift")
    destination = output_path or repo_path("local/derived/sound-data-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Inventory": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Files": output["summary"]["fileCount"],
        "Songs": output["summary"]["songFileCount"],
        "Banks": len(output["facts"]["banks"]),
        "RomParity": all(bank["romParity"] for bank in output["facts"]["banks"].values()),
        "Status": "PASS",
    }
