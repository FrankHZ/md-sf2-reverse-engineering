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
SFX_REFERENCE_SOURCE = Path("code/common/tech/sound/sfx.txt")
DRIVER_OUTPUT = Path("data/sound/sounddriver.bin")
ENUM_SOURCE = Path("sf2enums.asm")
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
DRIVER_ROM_OFFSET = 0x1EC000
SFX_POINTER_TABLE_ADDRESS = 0x15BD
SFX_DATA_START_ADDRESS = 0x162D
SFX_DATA_END_ADDRESS = 0x1F29
SFX_COMMAND_START = 0x41
SFX_ENTRY_COUNT = 56
SFX_TYPE_1_SLOTS = (
    "ym1-1",
    "ym1-2",
    "ym1-3",
    "ym2-4",
    "ym2-5",
    "ym2-6-dac",
    "psg-tone-1",
    "psg-tone-2",
    "psg-tone-3",
    "psg-noise",
)
SFX_TYPE_2_SLOTS = ("ym2-4", "ym2-5", "ym2-6-dac")
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
CHANNEL_SLOT_ROLES = (
    "ym1",
    "ym1",
    "ym1",
    "ym2",
    "ym2",
    "dac",
    "psg-tone",
    "psg-tone",
    "psg-tone",
    "psg-noise",
)
ALL_CHANNEL_ROLES = frozenset(CHANNEL_SLOT_ROLES)
YM_CHANNEL_ROLES = frozenset({"ym1", "ym2", "dac"})
PSG_CHANNEL_ROLES = frozenset({"psg-tone", "psg-noise"})
MACRO_ALLOWED_ROLES = {
    "channel_end": ALL_CHANNEL_ROLES,
    "countedLoopEnd": ALL_CHANNEL_ROLES,
    "countedLoopStart": ALL_CHANNEL_ROLES,
    "inst": YM_CHANNEL_ROLES,
    "mainLoopEnd": ALL_CHANNEL_ROLES,
    "mainLoopStart": ALL_CHANNEL_ROLES,
    "noSlide": YM_CHANNEL_ROLES,
    "note": YM_CHANNEL_ROLES,
    "noteL": YM_CHANNEL_ROLES,
    "psgInst": PSG_CHANNEL_ROLES,
    "psgNote": PSG_CHANNEL_ROLES,
    "psgNoteL": PSG_CHANNEL_ROLES,
    "repeatEnd": ALL_CHANNEL_ROLES,
    "repeatSection1Start": ALL_CHANNEL_ROLES,
    "repeatSection2Start": ALL_CHANNEL_ROLES,
    "repeatSection3Start": ALL_CHANNEL_ROLES,
    "repeatStart": ALL_CHANNEL_ROLES,
    "sample": frozenset({"dac"}),
    "sampleL": frozenset({"dac"}),
    "setRelease": ALL_CHANNEL_ROLES,
    "setSlide": YM_CHANNEL_ROLES,
    "shifting": frozenset(ALL_CHANNEL_ROLES - {"psg-noise"}),
    "stereo": YM_CHANNEL_ROLES,
    "sustain": ALL_CHANNEL_ROLES,
    "vibrato": frozenset(ALL_CHANNEL_ROLES - {"psg-noise"}),
    "vol": YM_CHANNEL_ROLES,
    "wait": ALL_CHANNEL_ROLES,
    "waitL": ALL_CHANNEL_ROLES,
    "ymTimer": frozenset({"psg-tone"}),
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


def _parse_asm_int(value: str) -> int:
    value = value.strip()
    if value.startswith("$"):
        return int(value[1:], 16)
    return int(value[:-1], 16) if value.lower().endswith("h") else int(value)


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
        if any(left[1] != right[0] for left, right in zip(ranges, ranges[1:], strict=False)):
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
            value.strip() for value in (match.group("args") or "").split(",") if value.strip()
        ]
        byte_expressions = []
        for body_line in match.group("body").splitlines():
            line = body_line.split(";", 1)[0].strip()
            directive = re.match(r"^db\s+(.+)$", line, re.IGNORECASE)
            if directive:
                byte_expressions.extend(value.strip() for value in directive.group(1).split(","))
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
    channel_pointers = re.findall(r"^\s*dw\s+(Music_\d+_Channel_\d+)\s*$", source, re.MULTILINE)
    label_matches = list(re.finditer(r"^(Music_\d+(?:_Channel_\d+)?):.*$", source, re.MULTILINE))
    channel_roles: dict[str, set[str]] = {}
    channel_bodies: dict[str, str] = {}
    entry_pointer_rows = []
    for index, label_match in enumerate(label_matches):
        label = label_match.group(1)
        end = label_matches[index + 1].start() if index + 1 < len(label_matches) else len(source)
        inline_body = label_match.group(0).split(":", 1)[1]
        body = inline_body + source[label_match.end() : end]
        if "_Channel_" in label:
            channel_bodies[label] = body
            continue
        pointers = re.findall(r"^\s*dw\s+(Music_\d+_Channel_\d+)\s*$", body, re.MULTILINE)
        if len(pointers) != len(CHANNEL_SLOT_ROLES):
            raise ValueError(
                f"music entry does not have ten channel pointers: {source_path}::{label}"
            )
        header_values = []
        for raw_line in body.splitlines():
            line = raw_line.split(";", 1)[0].strip()
            match = re.fullmatch(r"db\s+([0-9A-F]+h|\d+)", line, re.IGNORECASE)
            if match:
                header_values.append(_parse_asm_int(match.group(1)))
        if len(header_values) != 4 or any(value > 0xFF for value in header_values):
            raise ValueError(f"music entry header shape drift: {source_path}::{label}")
        entry_pointer_rows.append(
            {
                "entryLabel": label,
                "header": {
                    "typeMarker": header_values[0],
                    "dacDisabled": header_values[1] != 0,
                    "reservedTimerA": header_values[2],
                    "timerB": header_values[3],
                },
                "targets": pointers,
            }
        )
        for target, role in zip(pointers, CHANNEL_SLOT_ROLES, strict=True):
            channel_roles.setdefault(target, set()).add(role)
    if set(channel_bodies) != set(channel_roles):
        raise ValueError(f"music channel pointer/label mismatch: {source_path}")
    channels = []
    for label, body in channel_bodies.items():
        channel_invocations = Counter()
        for raw_line in body.splitlines():
            line = raw_line.split(";", 1)[0].strip()
            if not line:
                continue
            token = line.split(None, 1)[0]
            if token in macro_names:
                channel_invocations[token] += 1
        channels.append(
            {
                "label": label,
                "roles": sorted(channel_roles[label]),
                "macroInvocationCount": sum(channel_invocations.values()),
                "macroInvocations": dict(sorted(channel_invocations.items())),
            }
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
        "entryPointers": entry_pointer_rows,
        "channels": channels,
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
        {name: sum(row["macroInvocations"].get(name, 0) for row in songs) for name in macro_names}
    )
    unused = sorted(name for name, count in invocation_counts.items() if count == 0)
    if macro_names != set(MACRO_ALLOWED_ROLES):
        raise ValueError("music macro channel-role contract drift")
    role_pointer_counts = Counter(
        role for row in songs for _entry in row["entryPointers"] for role in CHANNEL_SLOT_ROLES
    )
    role_label_counts = Counter(
        role for row in songs for channel in row["channels"] for role in channel["roles"]
    )
    macro_role_uses = {
        name: sorted(
            {
                role
                for row in songs
                for channel in row["channels"]
                if channel["macroInvocations"].get(name, 0)
                for role in channel["roles"]
            }
        )
        for name in sorted(macro_names)
    }
    role_set_counts = Counter(
        tuple(channel["roles"]) for row in songs for channel in row["channels"]
    )
    compatibility_violations = []
    for row in songs:
        for channel in row["channels"]:
            for macro, count in channel["macroInvocations"].items():
                incompatible_roles = sorted(set(channel["roles"]) - MACRO_ALLOWED_ROLES[macro])
                if incompatible_roles:
                    compatibility_violations.append(
                        {
                            "sourcePath": row["sourcePath"],
                            "channelLabel": channel["label"],
                            "macro": macro,
                            "roles": incompatible_roles,
                            "invocationCount": count,
                        }
                    )
    header_rows = [
        {
            "sourcePath": row["sourcePath"],
            "entryLabel": entry["entryLabel"],
            **entry["header"],
        }
        for row in songs
        for entry in row["entryPointers"]
    ]
    timer_b_counts = Counter(row["timerB"] for row in header_rows)
    return {
        "summary": {
            "macroDefinitionCount": len(macro_definitions),
            "usedMacroCount": len(macro_definitions) - len(unused),
            "unusedMacroCount": len(unused),
            "flowMacroCount": sum(row["flowControl"] for row in macro_definitions),
            "songEntryLabelCount": sum(len(row["entryLabels"]) for row in songs),
            "channelLabelCount": sum(len(row["channelLabels"]) for row in songs),
            "channelPointerCount": sum(row["channelPointerCount"] for row in songs),
            "uniqueChannelPointerCount": sum(row["uniqueChannelPointerCount"] for row in songs),
            "macroInvocationCount": sum(invocation_counts.values()),
            "flowInvocationCount": sum(invocation_counts[name] for name in FLOW_MACROS),
            "multiRoleChannelLabelCount": sum(
                len(channel["roles"]) > 1 for row in songs for channel in row["channels"]
            ),
        },
        "macroDefinitions": macro_definitions,
        "invocationCounts": dict(sorted(invocation_counts.items())),
        "unusedMacros": unused,
        "channelRoles": {
            "slotRoles": list(CHANNEL_SLOT_ROLES),
            "pointerCounts": dict(sorted(role_pointer_counts.items())),
            "uniqueLabelCounts": dict(sorted(role_label_counts.items())),
            "roleSetCounts": [
                {"roles": list(roles), "labelCount": count}
                for roles, count in sorted(role_set_counts.items())
            ],
            "macroRoleUses": macro_role_uses,
            "compatibility": {
                "allowedRolesByMacro": {
                    name: sorted(roles) for name, roles in sorted(MACRO_ALLOWED_ROLES.items())
                },
                "violationCount": len(compatibility_violations),
                "violations": compatibility_violations,
            },
        },
        "musicHeaders": {
            "summary": {
                "entryCount": len(header_rows),
                "zeroTypeMarkerCount": sum(row["typeMarker"] == 0 for row in header_rows),
                "dacEnabledEntryCount": sum(not row["dacDisabled"] for row in header_rows),
                "dacDisabledEntryCount": sum(row["dacDisabled"] for row in header_rows),
                "nonzeroReservedTimerACount": sum(
                    row["reservedTimerA"] != 0 for row in header_rows
                ),
                "uniqueTimerBValueCount": len(timer_b_counts),
                "minimumTimerB": min(timer_b_counts),
                "maximumTimerB": max(timer_b_counts),
            },
            "timerBValueCounts": [
                {"value": value, "entryCount": count}
                for value, count in sorted(timer_b_counts.items())
            ],
            "entries": header_rows,
        },
        "songs": songs,
    }


def _music_bank_selection_contract(
    sources: dict[str, str], bank_payloads: dict[str, bytes], enum_source: str
) -> dict[str, Any]:
    enum_section = enum_source.split("; enum Music", 1)[1].split("; enum Sfx", 1)[0]
    enum_rows = re.findall(
        r"^(MUSIC_[A-Z0-9_]+):\s+equ\s+(\$?[0-9A-F]+)\s*$",
        enum_section,
        re.MULTILINE,
    )
    enum_names = {
        int(value.removeprefix("$"), 16 if value.startswith("$") else 10): name
        for name, value in enum_rows
    }
    if enum_names.get(0) != "MUSIC_NOTHING":
        raise ValueError("music enum zero-command boundary drift")

    symbol_sources = {}
    for source_path, source in sources.items():
        if not re.search(r"/music\d+\.asm$", source_path):
            continue
        for symbol in re.findall(r"^(Music_\d+):", source, re.MULTILINE):
            if symbol in symbol_sources:
                raise ValueError(f"duplicate music entry symbol: {symbol}")
            symbol_sources[symbol] = source_path

    slots = []
    for bank, first_command in (("bank0", 1), ("bank1", 33)):
        bank_source = sources[BANK_SOURCES[bank].as_posix()]
        pointer_symbols = re.findall(r"^\s*dw\s+(Music_\d+)\s*$", bank_source, re.MULTILINE)
        if len(pointer_symbols) != 32:
            raise ValueError(f"music command pointer-table boundary drift: {bank}")
        payload = bank_payloads[bank]
        for slot_index, target_symbol in enumerate(pointer_symbols):
            command_id = first_command + slot_index
            target_address = int.from_bytes(payload[slot_index * 2 : slot_index * 2 + 2], "little")
            target_offset = target_address - BANK_ORIGIN
            if target_symbol not in symbol_sources or not 64 <= target_offset < len(payload):
                raise ValueError(f"music command target boundary drift: {command_id}")
            slots.append(
                {
                    "commandId": command_id,
                    "commandHex": f"{command_id:02X}",
                    "enumName": enum_names.get(command_id),
                    "bank": bank,
                    "bankRegisterValue": 1 if bank == "bank0" else 0,
                    "pointerSlotIndex": slot_index,
                    "targetSymbol": target_symbol,
                    "targetZ80Address": target_address,
                    "targetRomOffset": BANK_ROM_OFFSETS[bank] + target_offset,
                    "targetSourcePath": symbol_sources[target_symbol],
                    "targetHeaderMarker": payload[target_offset],
                }
            )

    target_commands: dict[str, list[int]] = {}
    for row in slots:
        target_commands.setdefault(row["targetSymbol"], []).append(row["commandId"])
    aliases = [
        {"targetSymbol": symbol, "commandIds": commands}
        for symbol, commands in sorted(target_commands.items())
        if len(commands) > 1
    ]
    named_slots = [row for row in slots if row["enumName"] is not None]
    return {
        "enumSourcePath": ENUM_SOURCE.as_posix(),
        "enumSourceSha256": _sha256(enum_source.encode()),
        "summary": {
            "commandSlotCount": len(slots),
            "namedMusicCommandCount": len(named_slots),
            "unnamedCommandSlotCount": len(slots) - len(named_slots),
            "uniquePointerTargetCount": len(target_commands),
            "zeroHeaderMarkerSlotCount": sum(row["targetHeaderMarker"] == 0 for row in slots),
            "sfxRedirectSlotCount": sum(row["targetHeaderMarker"] != 0 for row in slots),
            "crossBankFallbackEdgeCount": 0,
        },
        "bankRules": [
            {
                "commandRange": "01-20",
                "bank": "bank0",
                "bankRegisterValue": 1,
                "romOffset": BANK_ROM_OFFSETS["bank0"],
                "pointerIndexExpression": "command-1",
            },
            {
                "commandRange": "21-40",
                "bank": "bank1",
                "bankRegisterValue": 0,
                "romOffset": BANK_ROM_OFFSETS["bank1"],
                "pointerIndexExpression": "command-33",
            },
        ],
        "zeroCommandBehavior": "ignored-by-main-loop",
        "nonzeroHeaderBehavior": "redirect-to-load-sfx",
        "aliasTargets": aliases,
        "slots": slots,
    }


def _frequency_table_values(
    driver_source: str, start_label: str, end_label: str, *, overlapping_first_word: bool
) -> list[int]:
    prefix, section_and_tail = driver_source.split(start_label, 1)
    section = section_and_tail.split(end_label, 1)[0]
    words = [
        _parse_asm_int(value)
        for value in re.findall(r"\bdw\s+([0-9A-F]+h|\d+)", section, re.IGNORECASE)
    ]
    if not overlapping_first_word:
        return words
    previous_bytes = re.findall(r"\bdb\s+([0-9A-F]+h|\d+)", prefix, re.IGNORECASE)
    leading_bytes = re.findall(r"^\s*db\s+([0-9A-F]+h|\d+)", section, re.MULTILINE | re.IGNORECASE)
    if not previous_bytes or not leading_bytes:
        raise ValueError("YM frequency table overlap boundary drift")
    first_word = (_parse_asm_int(leading_bytes[0]) << 8) | _parse_asm_int(previous_bytes[-1])
    return [first_word, *words]


def _music_frequency_contract(sources: dict[str, str], driver_source: str) -> dict[str, Any]:
    enum_path = (SOURCE_ROOT / "musicenums.asm").as_posix()
    enum_source = sources[enum_path]
    note_rows = re.findall(
        r"^([A-G](?:s)?\d):\s+equ\s+([0-9A-F]+h|\d+)\s*$",
        enum_source,
        re.MULTILINE | re.IGNORECASE,
    )
    note_names = {_parse_asm_int(value): name for name, value in note_rows}
    if sorted(note_names) != list(range(108)):
        raise ValueError("music note enum boundary drift")

    ym_values = _frequency_table_values(
        driver_source,
        "t_YM_FREQUENCIES:",
        "t_PSG_FREQUENCIES:",
        overlapping_first_word=True,
    )
    psg_values = _frequency_table_values(
        driver_source,
        "t_PSG_FREQUENCIES:",
        "t_YM_LEVELS:",
        overlapping_first_word=False,
    )
    if len(ym_values) != 84 or len(psg_values) != 64:
        raise ValueError("sound frequency table boundary drift")

    uses: dict[str, Counter[int]] = {
        "ym": Counter(),
        "psg": Counter(),
    }
    macro_uses: Counter[str] = Counter()
    shift_arguments: Counter[int] = Counter()
    for source_path, source in sources.items():
        if not re.search(r"/music\d+\.asm$", source_path):
            continue
        for raw_line in source.splitlines():
            line = raw_line.split(";", 1)[0]
            shift_match = re.match(r"^\s*shifting\s+([^,\s]+)", line)
            if shift_match:
                shift_arguments[_parse_asm_int(shift_match.group(1))] += 1
            match = re.match(r"^\s*(noteL?|psgNoteL?)\s+([^,\s]+)", line)
            if match is None:
                continue
            macro, note_name = match.groups()
            note_value = next(
                (value for value, name in note_names.items() if name == note_name), None
            )
            if note_value is None:
                raise ValueError(f"unknown music note enum use: {note_name}")
            family = "psg" if macro.startswith("psg") else "ym"
            uses[family][note_value] += 1
            macro_uses[macro] += 1

    ym_raw_indexes = Counter({value - 24: count for value, count in uses["ym"].items()})
    psg_raw_indexes = Counter({value - 21: count for value, count in uses["psg"].items()})

    shift_audit = _psg_note_shift_audit(sources, note_names, len(psg_values))

    def family_row(
        values: list[int], raw_indexes: Counter[int], enum_offset: int
    ) -> dict[str, Any]:
        outside = [
            {
                "note": note_names[index + enum_offset],
                "rawIndex": index,
                "invocationCount": count,
            }
            for index, count in sorted(raw_indexes.items())
            if not 0 <= index < len(values)
        ]
        return {
            "summary": {
                "tableEntryCount": len(values),
                "sourceInvocationCount": sum(raw_indexes.values()),
                "uniqueSourceNoteCount": len(raw_indexes),
                "minimumRawIndex": min(raw_indexes),
                "maximumRawIndex": max(raw_indexes),
                "rawIndexOutsideTableInvocationCount": sum(
                    row["invocationCount"] for row in outside
                ),
            },
            "tableSha256": _sha256(b"".join(value.to_bytes(2, "big") for value in values)),
            "entries": [
                {
                    "index": index,
                    "note": note_names[index + enum_offset],
                    "registerValue": value,
                }
                for index, value in enumerate(values)
            ],
            "rawOutsideTableUses": outside,
        }

    return {
        "enumSourcePath": enum_path,
        "enumSourceSha256": _sha256(enum_source.encode()),
        "noteEnumCount": len(note_names),
        "macroInvocationCounts": dict(sorted(macro_uses.items())),
        "ym": family_row(ym_values, ym_raw_indexes, 24),
        "psg": {
            **family_row(psg_values, psg_raw_indexes, 21),
            "shiftAudit": shift_audit,
        },
        "driverRules": {
            "ymMacroIndexExpression": "note-enum-24",
            "psgMacroIndexExpression": "note-enum-21",
            "runtimeNoteShiftStateOffset": 28,
            "ymFrequencyShiftStateOffset": 29,
            "shiftCommandArguments": [
                {
                    "argument": value,
                    "invocationCount": count,
                    "decodedNoteShift": _decode_note_shift(value),
                    "ymFrequencyShift": (value >> 3) & 0x0E,
                    "psgFrequencyShift": ((value >> 3) & 0x0E) >> 1,
                }
                for value, count in sorted(shift_arguments.items())
            ],
        },
    }


def _decode_note_shift(value: int) -> int:
    masked = value & 0x8F
    if masked & 0x80:
        masked |= 0xF0
    return masked - 0x100 if masked & 0x80 else masked


def _psg_note_shift_audit(
    sources: dict[str, str], note_names: dict[int, str], table_size: int
) -> dict[str, Any]:
    name_values = {name: value for value, name in note_names.items()}
    occurrences = []
    for source_path, source in sorted(sources.items()):
        if not re.search(r"/music\d+\.asm$", source_path):
            continue
        labels = list(re.finditer(r"^(Music_\d+_Channel_\d+):.*$", source, re.MULTILINE))
        for label_index, label_match in enumerate(labels):
            end = labels[label_index + 1].start() if label_index + 1 < len(labels) else len(source)
            body = label_match.group(0).split(":", 1)[1] + source[label_match.end() : end]
            instructions = []
            for line_number, raw_line in enumerate(body.splitlines(), start=1):
                line = raw_line.split(";", 1)[0].strip()
                match = re.match(r"^(\w+)(?:\s+([^,\s]+))?", line)
                if match:
                    instructions.append((match.group(1), match.group(2), line_number))
            if not instructions:
                continue

            edges = {index: set() for index in range(len(instructions))}
            main_start = None
            counted_starts = []
            repeat_start = None
            repeat_sections = []
            for index, (macro, _argument, _line_number) in enumerate(instructions):
                if index + 1 < len(instructions) and macro not in {"channel_end", "mainLoopEnd"}:
                    edges[index].add(index + 1)
                if macro == "mainLoopStart":
                    main_start = index
                elif macro == "mainLoopEnd" and main_start is not None:
                    edges[index].add(main_start + 1)
                elif macro == "countedLoopStart":
                    counted_starts.append(index)
                elif macro == "countedLoopEnd" and counted_starts:
                    edges[index].add(counted_starts.pop() + 1)
                elif macro == "repeatStart":
                    repeat_start = index
                    repeat_sections = []
                elif macro.startswith("repeatSection"):
                    repeat_sections.append(index)
                elif macro == "repeatEnd" and repeat_start is not None:
                    for target in (repeat_start, *repeat_sections):
                        if target + 1 < len(instructions):
                            edges[index].add(target + 1)

            states = [set() for _ in instructions]
            states[0].add(0)
            queue = [0]
            while queue:
                index = queue.pop()
                macro, argument, _line_number = instructions[index]
                output_states = states[index]
                if macro == "shifting" and argument is not None:
                    output_states = {_decode_note_shift(_parse_asm_int(argument))}
                for target in edges[index]:
                    new_states = output_states - states[target]
                    if new_states:
                        states[target].update(new_states)
                        queue.append(target)

            for index, (macro, argument, line_number) in enumerate(instructions):
                if macro not in {"psgNote", "psgNoteL"} or argument is None:
                    continue
                base_index = name_values[argument] - 21
                shifts = sorted(states[index])
                effective = sorted({base_index + shift for shift in shifts})
                occurrences.append(
                    {
                        "sourcePath": source_path,
                        "channelLabel": label_match.group(1),
                        "channelLine": line_number,
                        "macro": macro,
                        "note": argument,
                        "baseIndex": base_index,
                        "possibleNoteShifts": shifts,
                        "effectiveIndexes": effective,
                        "inRange": all(0 <= value < table_size for value in effective),
                    }
                )

    violations = [row for row in occurrences if not row["inRange"]]
    ambiguous = [row for row in occurrences if len(row["possibleNoteShifts"]) > 1]
    return {
        "summary": {
            "sourceInvocationCount": len(occurrences),
            "ambiguousShiftInvocationCount": len(ambiguous),
            "minimumEffectiveIndex": min(
                value for row in occurrences for value in row["effectiveIndexes"]
            ),
            "maximumEffectiveIndex": max(
                value for row in occurrences for value in row["effectiveIndexes"]
            ),
            "outOfRangeInvocationCount": len(violations),
        },
        "possibleShiftValues": sorted(
            {value for row in occurrences for value in row["possibleNoteShifts"]}
        ),
        "violations": violations,
    }


def _db_table_values(driver_source: str, start_label: str, end_label: str) -> list[int]:
    section = driver_source.split(start_label, 1)[1].split(end_label, 1)[0]
    return [
        _parse_asm_int(value)
        for value in re.findall(r"\bdb\s+([0-9A-F]+h|\d+)", section, re.IGNORECASE)
    ]


def _music_instrument_contract(
    sources: dict[str, str], driver_source: str, rom: bytes
) -> dict[str, Any]:
    ym_uses: Counter[int] = Counter()
    psg_values: Counter[int] = Counter()
    volume_uses: Counter[int] = Counter()
    for source_path, source in sources.items():
        if not re.search(r"/music\d+\.asm$", source_path):
            continue
        for raw_line in source.splitlines():
            line = raw_line.split(";", 1)[0]
            match = re.match(r"^\s*(inst|psgInst|vol)\s+([^,\s]+)", line)
            if match is None:
                continue
            macro, argument = match.groups()
            value = _parse_asm_int(argument)
            if macro == "inst":
                ym_uses[value] += 1
            elif macro == "psgInst":
                psg_values[value] += 1
            else:
                volume_uses[value] += 1

    ym_levels = _db_table_values(driver_source, "t_YM_LEVELS:", "t_SLOTS_PER_ALGO:")
    slot_masks = _db_table_values(driver_source, "t_SLOTS_PER_ALGO:", "pt_PITCH_EFFECTS:")
    if len(ym_levels) != 16 or len(slot_masks) != 8:
        raise ValueError("YM instrument support-table boundary drift")
    if max(volume_uses) >= len(ym_levels):
        raise ValueError("YM volume use outside level table")

    ym_entry_size = 0x29
    ym_rom_base = 0x1EB000
    ym_entries = []
    for index, count in sorted(ym_uses.items()):
        rom_offset = ym_rom_base + index * ym_entry_size
        payload = rom[rom_offset : rom_offset + ym_entry_size]
        if len(payload) != ym_entry_size or rom_offset + ym_entry_size > 0x1F0000:
            raise ValueError("YM instrument ROM range drift")
        ym_entries.append(
            {
                "instrumentIndex": index,
                "invocationCount": count,
                "romOffset": rom_offset,
                "sizeBytes": ym_entry_size,
                "payloadSha256": _sha256(payload),
            }
        )

    psg_pointer_section = driver_source.split("pt_PSG_INSTRUMENTS:", 1)[1].split("byte_12D2:", 1)[0]
    psg_targets = re.findall(r"\bdw\s+(byte_[0-9A-F]+)", psg_pointer_section)
    if len(psg_targets) != 16:
        raise ValueError("PSG instrument pointer-table boundary drift")
    psg_instrument_uses = Counter()
    psg_level_uses = Counter()
    for value, count in psg_values.items():
        psg_instrument_uses[value >> 4] += count
        psg_level_uses[value & 0x0F] += count
    if max(psg_instrument_uses) >= len(psg_targets):
        raise ValueError("PSG instrument use outside pointer table")

    return {
        "summary": {
            "ymInvocationCount": sum(ym_uses.values()),
            "ymUsedInstrumentCount": len(ym_uses),
            "ymMinimumInstrumentIndex": min(ym_uses),
            "ymMaximumInstrumentIndex": max(ym_uses),
            "psgInvocationCount": sum(psg_values.values()),
            "psgUsedInstrumentCount": len(psg_instrument_uses),
            "psgUsedLevelCount": len(psg_level_uses),
            "volumeInvocationCount": sum(volume_uses.values()),
            "volumeUsedLevelCount": len(volume_uses),
        },
        "ym": {
            "instrumentRomBase": ym_rom_base,
            "instrumentSizeBytes": ym_entry_size,
            "levelTable": ym_levels,
            "slotMasksByAlgorithm": slot_masks,
            "usedEntries": ym_entries,
            "volumeInvocationCounts": [
                {"level": value, "invocationCount": count}
                for value, count in sorted(volume_uses.items())
            ],
        },
        "psg": {
            "pointerTargets": psg_targets,
            "instrumentInvocationCounts": [
                {"instrumentIndex": value, "invocationCount": count}
                for value, count in sorted(psg_instrument_uses.items())
            ],
            "levelInvocationCounts": [
                {"level": value, "invocationCount": count}
                for value, count in sorted(psg_level_uses.items())
            ],
        },
    }


def _music_sample_contract(
    sources: dict[str, str], driver_source: str, rom: bytes
) -> dict[str, Any]:
    section = driver_source.split("t_SAMPLE_LOAD_DATA:", 1)[1].split("pt_SFX:", 1)[0]
    entries = []
    for raw_line in section.splitlines():
        line = raw_line.split(";", 1)[0].strip()
        match = re.match(r"^(?:db\s+)?(.+)$", line)
        if match is None:
            continue
        entry_text = match.group(1)
        values = [value.strip() for value in entry_text.split(",")]
        if len(values) != 8:
            continue
        parsed = [_parse_asm_int(value) for value in values]
        frame_period, ignored_1, bank, ignored_3, length_lo, length_hi, ptr_lo, ptr_hi = parsed
        length = length_lo | (length_hi << 8)
        pointer = ptr_lo | (ptr_hi << 8)
        rom_offset = 0x1E0000 + bank * 0x8000 + pointer - BANK_ORIGIN
        if ignored_1 or ignored_3 or bank not in (0, 1):
            raise ValueError("DAC sample load-table field boundary drift")
        if not 0x1E0000 <= rom_offset < rom_offset + length <= 0x1F0000:
            raise ValueError("DAC sample ROM range boundary drift")
        entries.append(
            {
                "sampleIndex": len(entries),
                "framePeriod": frame_period,
                "bank": bank,
                "lengthBytes": length,
                "pointerZ80Address": pointer,
                "romOffset": rom_offset,
                "romEndOffset": rom_offset + length,
                "payloadSha256": _sha256(rom[rom_offset : rom_offset + length]),
            }
        )
    if len(entries) != 17:
        raise ValueError("DAC sample load-table entry-count drift")

    uses: Counter[int] = Counter()
    for source_path, source in sources.items():
        if not re.search(r"/music\d+\.asm$", source_path):
            continue
        for match in re.finditer(r"^\s*sampleL?\s+([^,\s]+)", source, re.MULTILINE):
            index = _parse_asm_int(match.group(1))
            if not 0 <= index < len(entries):
                raise ValueError(f"music DAC sample index outside load table: {index}")
            uses[index] += 1
    return {
        "summary": {
            "tableEntryCount": len(entries),
            "musicInvocationCount": sum(uses.values()),
            "musicUsedSampleCount": len(uses),
            "minimumMusicSampleIndex": min(uses),
            "maximumMusicSampleIndex": max(uses),
            "musicUnusedTableEntryCount": len(entries) - len(uses),
        },
        "musicInvocationCounts": [
            {"sampleIndex": index, "invocationCount": count}
            for index, count in sorted(uses.items())
        ],
        "entries": entries,
    }


def _sfx_source_headers(driver_source: str) -> dict[str, dict[str, Any]]:
    lines = driver_source.splitlines()
    headers: dict[str, dict[str, Any]] = {}
    pattern = re.compile(r"^(sfx_([0-9A-F]{2})):\s*db\s+([12])\b")
    for line_index, line in enumerate(lines):
        match = pattern.match(line)
        if not match:
            continue
        label, _, type_text = match.groups()
        sfx_type = int(type_text)
        pointer_count = 10 if sfx_type == 1 else 3
        targets: list[str] = []
        for following in lines[line_index + 1 :]:
            statement = following.split(";", 1)[0].strip()
            if not statement:
                continue
            pointer_match = re.match(r"^dw\s+(.+)$", statement)
            if not pointer_match:
                break
            targets.extend(value.strip() for value in pointer_match.group(1).split(","))
            if len(targets) >= pointer_count:
                break
        if len(targets) != pointer_count:
            raise ValueError(f"SFX source header pointer-count drift: {label}")
        if any(not re.fullmatch(r"byte_[0-9A-F]{4}", target) for target in targets):
            raise ValueError(f"SFX source header target-label drift: {label}")
        headers[label] = {"type": sfx_type, "targets": targets}
    return headers


def _decode_sfx_streams(
    driver_payload: bytes, entries: list[dict[str, Any]]
) -> dict[str, Any]:
    references: dict[int, list[dict[str, Any]]] = {}
    for entry in entries:
        slots = SFX_TYPE_1_SLOTS if entry["type"] == 1 else SFX_TYPE_2_SLOTS
        for slot, pointer in zip(slots, entry["channelPointers"], strict=True):
            if driver_payload[pointer] == 0xFF:
                continue
            references.setdefault(pointer, []).append(
                {
                    "commandId": entry["commandId"],
                    "sfxIndex": entry["sfxIndex"],
                    "slot": slot,
                }
            )

    token_kinds: dict[int, str] = {}
    loop_subcommands: dict[int, int] = {}
    unique_token_addresses: set[int] = set()
    unique_decoded_addresses: set[int] = set()
    redirects: set[tuple[int, int]] = set()
    counted_loop_edges: set[tuple[int, int, int]] = set()
    streams = []
    for start, stream_references in sorted(references.items()):
        address = start
        visited_segment_starts: set[int] = set()
        segments = []
        stream_token_count = 0
        redirect_depth = 0
        counted_loop_anchor: tuple[int, int] | None = None
        stream_counted_loop_edges = []
        while True:
            if address in visited_segment_starts:
                raise ValueError(f"SFX redirect cycle at {address:04X}")
            if not SFX_DATA_START_ADDRESS <= address < SFX_DATA_END_ADDRESS:
                raise ValueError(f"SFX stream address outside data range: {address:04X}")
            visited_segment_starts.add(address)
            segment_start = address
            segment_token_count = 0
            while True:
                if not SFX_DATA_START_ADDRESS <= address < SFX_DATA_END_ADDRESS:
                    raise ValueError(f"SFX token outside data range: {address:04X}")
                opcode = driver_payload[address]
                token_start = address
                if opcode == 0xFF:
                    width = 3
                    if address + width > SFX_DATA_END_ADDRESS:
                        raise ValueError(f"truncated SFX FF token at {address:04X}")
                    target = int.from_bytes(driver_payload[address + 1 : address + 3], "little")
                    kind = "terminate" if target == 0 else "redirect"
                elif opcode >= 0xF8:
                    width = 2
                    if address + width > SFX_DATA_END_ADDRESS:
                        raise ValueError(f"truncated SFX command at {address:04X}")
                    target = None
                    kind = "command"
                    if opcode == 0xF8:
                        parameter = driver_payload[address + 1]
                        subcommand = parameter >> 5
                        loop_subcommands[token_start] = subcommand
                        if subcommand == 6:
                            if counted_loop_anchor is not None:
                                raise ValueError(
                                    f"nested SFX counted loop at {token_start:04X}"
                                )
                            counted_loop_anchor = (address + width, (parameter & 0x1F) + 1)
                        elif subcommand == 7:
                            if counted_loop_anchor is None:
                                raise ValueError(
                                    f"unmatched SFX counted-loop end at {token_start:04X}"
                                )
                            loop_target, iteration_count = counted_loop_anchor
                            edge = (token_start, loop_target, iteration_count)
                            counted_loop_edges.add(edge)
                            stream_counted_loop_edges.append(
                                {
                                    "endTokenZ80Address": token_start,
                                    "targetZ80Address": loop_target,
                                    "iterationCount": iteration_count,
                                }
                            )
                            counted_loop_anchor = None
                else:
                    width = 2 if opcode & 0x80 else 1
                    if address + width > SFX_DATA_END_ADDRESS:
                        raise ValueError(f"truncated SFX note/sample at {address:04X}")
                    target = None
                    kind = "note-or-sample"

                token_kind = f"{opcode:02X}" if opcode >= 0xF8 else kind
                if token_start in token_kinds and token_kinds[token_start] != token_kind:
                    raise ValueError(f"inconsistent SFX token decode at {token_start:04X}")
                token_kinds[token_start] = token_kind
                unique_token_addresses.add(token_start)
                unique_decoded_addresses.update(range(token_start, token_start + width))
                segment_token_count += 1
                stream_token_count += 1
                address += width
                if opcode != 0xFF:
                    continue

                segment = {
                    "startZ80Address": segment_start,
                    "endZ80AddressExclusive": address,
                    "sizeBytes": address - segment_start,
                    "tokenCount": segment_token_count,
                    "terminalKind": kind,
                    "sha256": _sha256(driver_payload[segment_start:address]),
                }
                if target:
                    if not SFX_DATA_START_ADDRESS <= target < SFX_DATA_END_ADDRESS:
                        raise ValueError(
                            f"SFX redirect target outside data range: {target:04X}"
                        )
                    segment["redirectTargetZ80Address"] = target
                    redirects.add((token_start, target))
                    redirect_depth += 1
                segments.append(segment)
                if not target:
                    if counted_loop_anchor is not None:
                        raise ValueError(f"unterminated SFX counted loop from {start:04X}")
                    address = 0
                else:
                    address = target
                break
            if address == 0:
                break

        streams.append(
            {
                "startZ80Address": start,
                "referenceCount": len(stream_references),
                "roles": sorted({row["slot"] for row in stream_references}),
                "commandIds": sorted({row["commandId"] for row in stream_references}),
                "tokenCount": stream_token_count,
                "redirectDepth": redirect_depth,
                "countedLoopEdges": stream_counted_loop_edges,
                "segments": segments,
            }
        )

    return {
        "tokenRules": [
            {
                "kind": "note-or-sample",
                "opcodeRange": "00-F7",
                "width": "1 byte when bit 7 is clear; otherwise 2 bytes",
            },
            {"kind": "command", "opcodeRange": "F8-FE", "width": "2 bytes"},
            {
                "kind": "end-or-redirect",
                "opcodeRange": "FF",
                "width": "3 bytes; little-endian zero terminates, nonzero redirects",
            },
        ],
        "summary": {
            "activeReferenceCount": sum(len(rows) for rows in references.values()),
            "uniqueActiveStartCount": len(references),
            "decodedStreamCount": len(streams),
            "traversedTokenCount": sum(row["tokenCount"] for row in streams),
            "uniqueTokenStartCount": len(unique_token_addresses),
            "uniqueDecodedByteCount": len(unique_decoded_addresses),
            "redirectEdgeCount": len(redirects),
            "countedLoopEdgeCount": len(counted_loop_edges),
            "maximumCountedLoopIterationCount": max(
                (edge[2] for edge in counted_loop_edges), default=0
            ),
            "maximumRedirectDepth": max(
                (row["redirectDepth"] for row in streams), default=0
            ),
            "maximumStreamTokenCount": max(
                (row["tokenCount"] for row in streams), default=0
            ),
        },
        "opcodeCounts": [
            {"opcodeOrKind": opcode, "tokenCount": count}
            for opcode, count in sorted(Counter(token_kinds.values()).items())
        ],
        "loopSubcommandCounts": [
            {"subcommand": subcommand, "tokenCount": count}
            for subcommand, count in sorted(Counter(loop_subcommands.values()).items())
        ],
        "streams": streams,
    }


def _sfx_contract(
    driver_source: str,
    reference_source: str,
    enum_source: str,
    driver_payload: bytes,
    rom: bytes,
) -> dict[str, Any]:
    if len(driver_payload) != 0x2000:
        raise ValueError(f"sound driver binary size drift: {len(driver_payload)}")
    if rom[DRIVER_ROM_OFFSET : DRIVER_ROM_OFFSET + len(driver_payload)] != driver_payload:
        raise ValueError("sound driver binary does not match the canonical ROM slice")

    pointer_table_end = SFX_POINTER_TABLE_ADDRESS + SFX_ENTRY_COUNT * 2
    if pointer_table_end != SFX_DATA_START_ADDRESS:
        raise ValueError("SFX pointer-table/data boundary drift")
    table_labels: list[str] = []
    table_source = driver_source[
        driver_source.index("pt_SFX:") : driver_source.index("sfx_01:")
    ].replace("pt_SFX:", "", 1)
    for statement in re.findall(r"^\s*dw\s+([^;\r\n]+)", table_source, re.MULTILINE):
        table_labels.extend(value.strip() for value in statement.split(","))
    expected_labels = [f"sfx_{index:02X}" for index in range(1, SFX_ENTRY_COUNT + 1)]
    if table_labels != expected_labels:
        raise ValueError("SFX source pointer-table order drift")

    source_headers = _sfx_source_headers(driver_source)
    if sorted(source_headers) != sorted(expected_labels):
        raise ValueError("SFX source header inventory drift")
    enums = {
        _parse_asm_int(value): name
        for name, value in re.findall(
            r"^(SFX_[A-Z0-9_]+):\s+equ\s+([^\s;]+)", enum_source, re.MULTILINE
        )
        if SFX_COMMAND_START
        <= _parse_asm_int(value)
        < SFX_COMMAND_START + SFX_ENTRY_COUNT
    }
    if sorted(enums) != list(range(SFX_COMMAND_START, SFX_COMMAND_START + SFX_ENTRY_COUNT)):
        raise ValueError("SFX enum command domain drift")

    entries = []
    all_targets: list[int] = []
    active_slot_counts: Counter[str] = Counter()
    for slot_index, label in enumerate(expected_labels):
        table_offset = SFX_POINTER_TABLE_ADDRESS + slot_index * 2
        header_address = int.from_bytes(driver_payload[table_offset : table_offset + 2], "little")
        if not SFX_DATA_START_ADDRESS <= header_address < SFX_DATA_END_ADDRESS:
            raise ValueError(f"SFX header outside data range: {label}")
        sfx_type = driver_payload[header_address]
        source_header = source_headers[label]
        if sfx_type != source_header["type"]:
            raise ValueError(f"SFX source/binary type drift: {label}")
        slots = SFX_TYPE_1_SLOTS if sfx_type == 1 else SFX_TYPE_2_SLOTS
        pointers = [
            int.from_bytes(
                driver_payload[header_address + 1 + index * 2 : header_address + 3 + index * 2],
                "little",
            )
            for index in range(len(slots))
        ]
        source_targets = source_header["targets"]
        source_addresses = [int(target.removeprefix("byte_"), 16) for target in source_targets]
        if pointers != source_addresses:
            raise ValueError(f"SFX source/binary channel-pointer drift: {label}")
        if any(
            not SFX_DATA_START_ADDRESS <= pointer < SFX_DATA_END_ADDRESS
            for pointer in pointers
        ):
            raise ValueError(f"SFX channel pointer outside data range: {label}")
        active_slots = [
            slot
            for slot, pointer in zip(slots, pointers, strict=True)
            if driver_payload[pointer] != 0xFF
        ]
        if sfx_type == 1 and any(slot not in {"psg-tone-3", "psg-noise"} for slot in active_slots):
            raise ValueError(f"type-1 SFX consumes a music-owned channel: {label}")
        active_slot_counts.update(active_slots)
        all_targets.extend(pointers)
        command_id = SFX_COMMAND_START + slot_index
        entries.append(
            {
                "sfxIndex": slot_index + 1,
                "commandId": command_id,
                "commandHex": f"{command_id:02X}",
                "enumName": enums[command_id],
                "sourceLabel": label,
                "headerZ80Address": header_address,
                "type": sfx_type,
                "headerSizeBytes": 1 + len(slots) * 2,
                "channelPointers": pointers,
                "sourceTargets": source_targets,
                "activeSlots": active_slots,
            }
        )

    type_counts = Counter(row["type"] for row in entries)
    active_pointer_count = sum(len(row["activeSlots"]) for row in entries)
    return {
        "driverSourcePath": DRIVER_SOURCE.as_posix(),
        "driverSourceSha256": _sha256(driver_source.encode()),
        "referenceSourcePath": SFX_REFERENCE_SOURCE.as_posix(),
        "referenceSourceSha256": _sha256(reference_source.encode()),
        "driverBinary": {
            "sourcePath": DRIVER_OUTPUT.as_posix(),
            "romOffset": DRIVER_ROM_OFFSET,
            "sizeBytes": len(driver_payload),
            "sha256": _sha256(driver_payload),
            "romParity": True,
        },
        "layout": {
            "pointerTableStart": SFX_POINTER_TABLE_ADDRESS,
            "pointerTableEnd": pointer_table_end,
            "dataStart": SFX_DATA_START_ADDRESS,
            "dataEnd": SFX_DATA_END_ADDRESS,
            "dataSizeBytes": SFX_DATA_END_ADDRESS - SFX_DATA_START_ADDRESS,
            "driverCopyEndExclusive": 0x1F80,
            "tableAndDataSha256": _sha256(
                driver_payload[SFX_POINTER_TABLE_ADDRESS:SFX_DATA_END_ADDRESS]
            ),
        },
        "summary": {
            "commandCount": len(entries),
            "minimumCommand": SFX_COMMAND_START,
            "maximumCommand": SFX_COMMAND_START + len(entries) - 1,
            "namedCommandCount": len(enums),
            "type1EntryCount": type_counts[1],
            "type2EntryCount": type_counts[2],
            "channelPointerCount": len(all_targets),
            "uniqueChannelPointerCount": len(set(all_targets)),
            "activeChannelPointerCount": active_pointer_count,
            "inactiveChannelPointerCount": len(all_targets) - active_pointer_count,
        },
        "activeSlotCounts": [
            {"slot": slot, "entryCount": active_slot_counts[slot]}
            for slot in SFX_TYPE_1_SLOTS
            if active_slot_counts[slot]
        ],
        "streamModel": _decode_sfx_streams(driver_payload, entries),
        "entries": entries,
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
            "Main:",
            "cp\t21h",
            "ld\ta, 1",
            "ld\t(MUSIC_BANK_TO_LOAD), a",
            "call\tLoadMusicBank",
            "ld\tde, 8000h",
            "loc_201:",
            "xor\ta",
            "ld\t(MUSIC_BANK_TO_LOAD), a",
            "call\tLoadMusicBank",
            "sub\t20h",
            "Load_Music:",
            "dec\ta",
            "add\ta, a",
            "jp\tnz, Load_SFX",
            "call\tStopMusic",
            "ld\t(MUSIC_DOESNT_USE_SAMPLES), a",
            "ld\tb, 26h",
            "ld\tc, (hl)",
            "call\tYM1_Input",
            "ld\tb, 0Ah",
            "Load_Music_Channels:",
            "UpdateSound:",
            "ld\tiy, CURRENT_CHANNEL",
            "call\tYM1_ParseData",
            "call\tYM1_ParseData",
            "call\tYM1_ParseData",
            "call\tYM2_ParseData",
            "call\tYM2_ParseData",
            "call\tYM2_ParseChannel6Data",
            "call\tPSG_ParseToneData",
            "call\tPSG_ParseToneData",
            "call\tPSG_ParseToneData",
            "call\tPSG_ParseNoiseData",
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
        "musicUpdateSlotRoles": list(CHANNEL_SLOT_ROLES),
        "musicHeaderFields": [
            {"offset": 0, "meaning": "zero-music-type-marker"},
            {"offset": 1, "meaning": "nonzero-disables-dac"},
            {"offset": 2, "meaning": "reserved-timer-a-byte"},
            {"offset": 3, "meaning": "ym-timer-b-value"},
            {"offset": 4, "meaning": "ten-little-endian-channel-pointers"},
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
    sources = {path.relative_to(disasm).as_posix(): read_upstream_text(path) for path in paths}

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
    enum_source = read_upstream_text(disasm / ENUM_SOURCE)
    command_model["bankSelection"] = _music_bank_selection_contract(
        sources, bank_payloads, enum_source
    )
    command_model["frequencyModel"] = _music_frequency_contract(sources, driver_source)
    command_model["instrumentModel"] = _music_instrument_contract(sources, driver_source, rom)
    command_model["sampleModel"] = _music_sample_contract(sources, driver_source, rom)
    driver_payload = (disasm / DRIVER_OUTPUT).read_bytes()
    reference_source = read_upstream_text(disasm / SFX_REFERENCE_SOURCE)
    command_model["sfxModel"] = _sfx_contract(
        driver_source,
        reference_source,
        enum_source,
        driver_payload,
        rom,
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
            "dac-sample-rate-and-live-channel-state-runtime-semantics",
            "tempo-loop-and-instrument-timing",
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
    for field in (
        "summary",
        "unusedMacros",
        "invocationCounts",
        "driver",
    ):
        if output["commandModel"][field] != fixture["expected"]["commandModel"][field]:
            raise ValueError(f"sound data command-model {field} drift")
    expected_bank_selection = fixture["expected"]["commandModel"]["bankSelection"]
    for field in (
        "enumSourcePath",
        "enumSourceSha256",
        "summary",
        "bankRules",
        "zeroCommandBehavior",
        "nonzeroHeaderBehavior",
        "aliasTargets",
    ):
        if output["commandModel"]["bankSelection"][field] != expected_bank_selection[field]:
            raise ValueError(f"sound data bank-selection {field} drift")
    expected_channel_roles = fixture["expected"]["commandModel"]["channelRoles"]
    for field in (
        "slotRoles",
        "pointerCounts",
        "uniqueLabelCounts",
        "roleSetCounts",
        "macroRoleUses",
    ):
        if output["commandModel"]["channelRoles"][field] != expected_channel_roles[field]:
            raise ValueError(f"sound data channel-role {field} drift")
    compatibility = output["commandModel"]["channelRoles"]["compatibility"]
    if compatibility["violationCount"] != expected_channel_roles["compatibilityViolationCount"]:
        raise ValueError("sound data channel-role compatibility drift")
    expected_headers = fixture["expected"]["commandModel"]["musicHeaders"]
    for field in ("summary", "timerBValueCounts"):
        if output["commandModel"]["musicHeaders"][field] != expected_headers[field]:
            raise ValueError(f"sound data music-header {field} drift")
    frequency = output["commandModel"]["frequencyModel"]
    expected_frequency = fixture["expected"]["commandModel"]["frequencyModel"]
    for field in (
        "enumSourcePath",
        "enumSourceSha256",
        "noteEnumCount",
        "macroInvocationCounts",
        "driverRules",
    ):
        if frequency[field] != expected_frequency[field]:
            raise ValueError(f"sound data frequency-model {field} drift")
    for family in ("ym", "psg"):
        for field in ("summary", "tableSha256", "rawOutsideTableUses"):
            if frequency[family][field] != expected_frequency[family][field]:
                raise ValueError(f"sound data {family}-frequency {field} drift")
    if frequency["psg"]["shiftAudit"] != expected_frequency["psg"]["shiftAudit"]:
        raise ValueError("sound data PSG note-shift audit drift")
    sample_model = output["commandModel"]["sampleModel"]
    expected_samples = fixture["expected"]["commandModel"]["sampleModel"]
    for field in ("summary", "musicInvocationCounts"):
        if sample_model[field] != expected_samples[field]:
            raise ValueError(f"sound data sample-model {field} drift")
    instrument_model = output["commandModel"]["instrumentModel"]
    expected_instruments = fixture["expected"]["commandModel"]["instrumentModel"]
    if instrument_model["summary"] != expected_instruments["summary"]:
        raise ValueError("sound data instrument-model summary drift")
    for field in ("levelTable", "slotMasksByAlgorithm", "volumeInvocationCounts"):
        if instrument_model["ym"][field] != expected_instruments["ym"][field]:
            raise ValueError(f"sound data YM instrument {field} drift")
    if instrument_model["psg"] != expected_instruments["psg"]:
        raise ValueError("sound data PSG instrument model drift")
    sfx_model = output["commandModel"]["sfxModel"]
    expected_sfx = fixture["expected"]["commandModel"]["sfxModel"]
    for field in ("driverBinary", "layout", "summary", "activeSlotCounts"):
        if sfx_model[field] != expected_sfx[field]:
            raise ValueError(f"sound data SFX model {field} drift")
    for field in ("tokenRules", "summary", "opcodeCounts", "loopSubcommandCounts"):
        if sfx_model["streamModel"][field] != expected_sfx["streamModel"][field]:
            raise ValueError(f"sound data SFX stream model {field} drift")
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
