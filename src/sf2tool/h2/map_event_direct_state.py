"""Public H2 source/H1/ROM inventory for direct fixed-RAM map-event accesses.

The map-events and map-data rails retain their complete event/program/table corpora.
This owner builds those rails afresh, then narrows their already-guarded source
contexts to direct raw-68000 accesses to the fixed-RAM symbols declared below.
"""

from __future__ import annotations

import hashlib
import json
import re
from pathlib import Path
from typing import Any

from sf2tool.h2.map_data import build_map_data_inventory
from sf2tool.h2.map_events import _canonical_bytes as _map_events_canonical_bytes
from sf2tool.h2.map_events import build_map_events_contract
from sf2tool.h2.map_events_fixture import load_map_events_fixture
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path

ID = "sf2-map-event-direct-state-static-v1"
FIXTURE = repo_path("tests/fixtures/h2/map-event-direct-state-static-v1.json")
SCHEMA = repo_path("schemas/h2/map-event-direct-state-static-fixture.schema.json")
MAP_EVENTS_MANIFEST = repo_path("manifests/extractions/map-events-static.json")
MAP_DATA_MANIFEST = repo_path("manifests/extractions/map-data-static.json")
MAP_DATA_FIXTURE = repo_path("tests/fixtures/h2/map-data-static-v1.json")

_DIRECT_SYMBOLS = (
    "CURRENT_PORTRAIT",
    "CURRENT_SHOP_INDEX",
    "CURRENT_SPEECH_SFX",
    "DIALOGUE_NAME_INDEX_1",
    "EGRESS_MAP",
    "ENTITY_FACING",
    "EVENT_RELATIVE_POSITION",
    "MAP_EVENT_TYPE",
    "MESSAGE_SPEED",
    "RAFT_MAP",
    "RAFT_X",
    "RAFT_Y",
    "SPEECH_SFX_COPY",
)
_CATEGORIES = ("entityEvents", "zoneEvents", "itemEvents")
_PROGRAM_FIELDS = {
    "entityEvents": "entityTargetPrograms",
    "zoneEvents": "zoneTargetPrograms",
    "itemEvents": "itemTargetPrograms",
}
_DIRECT_RAM_OPERAND = re.compile(r"^\(\(([A-Z][A-Z0-9_]*)-\$1000000\)\)\.w$")
_EQUATE = re.compile(r"^\s*([A-Za-z_][A-Za-z0-9_]*)\s*:\s+equ\s+([^\s;]+)")
_INLINE_LABEL = re.compile(r"^[A-Za-z_][A-Za-z0-9_]*:\s*(.*)$")
_REGISTER = re.compile(r"^[dDaA][0-7]$")
_H1_INSTRUCTION = re.compile(
    r"^([0-9A-Fa-f]{8})\s+([0-9A-Fa-f]{2,4}(?: [0-9A-Fa-f]{2,4})*)\s{2,}(.+)$"
)
_UNKNOWN_KEYS = (
    "normal-story-program-reachability",
    "caller-entry-state",
    "runtime-branch-and-access-order",
    "runtime-before-after-values",
    "callee-and-service-side-effects",
    "cross-map-state-lifetime",
    "save-load-persistence",
    "input-text-portrait-audio-vint-timing",
    "player-facing-story-meaning",
)


def canonical_json_bytes(value: dict[str, Any]) -> bytes:
    """Emit the one canonical UTF-8 representation for this public fixture."""
    return (
        json.dumps(value, ensure_ascii=False, sort_keys=True, separators=(",", ":")).encode("utf-8")
        + b"\n"
    )


def _fixture_sha256(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest().upper()


def _fresh_retained_mother_corpus(
    rom_path: Path, upstream_path: Path
) -> tuple[dict[str, Any], dict[str, Any], dict[str, Any]]:
    """Build and validate retained owners before consuming their projections.

    This deliberately calls the maintained builders instead of loading their
    golden fixtures as a substitute for source/H1/ROM construction.  The
    returned objects remain read-only joins; this owner does not reimplement
    their table, selector, or program-boundary algorithms.
    """
    map_events = build_map_events_contract(rom_path, upstream_path)
    map_events_fixture = load_map_events_fixture()
    map_events_manifest = load_json(MAP_EVENTS_MANIFEST)
    if map_events_fixture["expected"] != map_events:
        raise ValueError("map-event direct-state retained map-events projection drift")
    map_events_digest = hashlib.sha256(_map_events_canonical_bytes(map_events)).hexdigest().upper()
    if (
        map_events_digest != map_events_manifest["outputSha256"]
        or map_events["summary"] != map_events_manifest["summary"]
    ):
        raise ValueError("map-event direct-state retained map-events digest drift")

    map_data = build_map_data_inventory(upstream_path)
    map_data_fixture = load_json(MAP_DATA_FIXTURE)
    map_data_manifest = load_json(MAP_DATA_MANIFEST)
    if map_data["representativeAddresses"] != map_data_fixture["table"]:
        raise ValueError("map-event direct-state retained map-data address projection drift")
    map_data_digest = hashlib.sha256(_map_events_canonical_bytes(map_data)).hexdigest().upper()
    if (
        map_data_digest != map_data_manifest["outputSha256"]
        or map_data["summary"] != map_data_manifest["summary"]
    ):
        raise ValueError("map-event direct-state retained map-data digest drift")

    return (
        map_events,
        map_data,
        {
            "mapEvents": {
                "fixtureId": map_events_fixture["id"],
                "fixtureSha256": _fixture_sha256(
                    repo_path("tests/fixtures/h2/map-events-static-v1.json")
                ),
                "outputSha256": map_events_digest,
            },
            "mapData": {
                "fixtureId": map_data_fixture["id"],
                "fixtureSha256": _fixture_sha256(MAP_DATA_FIXTURE),
                "outputSha256": map_data_digest,
            },
        },
    )


def _mother_corpus_projection(map_events: dict[str, Any]) -> dict[str, Any]:
    """Keep source-program denominators separate from the direct-state subset."""
    expected_program_counts = {"entityEvents": 684, "zoneEvents": 150, "itemEvents": 80}
    expected_operation_counts = {"entityEvents": 2624, "zoneEvents": 809, "itemEvents": 146}
    expected_raw_instruction_counts = {
        "entityEvents": 183,
        "zoneEvents": 70,
        "itemEvents": 19,
    }
    categories: list[dict[str, Any]] = []
    for category in _CATEGORIES:
        programs = map_events[_PROGRAM_FIELDS[category]]
        program_count = len(programs)
        operation_count = sum(len(program["operations"]) for program in programs)
        raw_instruction_count = sum(
            operation["family"] == "raw-68000-instruction"
            for program in programs
            for operation in program["operations"]
        )
        if (
            program_count != expected_program_counts[category]
            or operation_count != expected_operation_counts[category]
            or raw_instruction_count != expected_raw_instruction_counts[category]
        ):
            raise ValueError(f"map-event direct-state retained {category} denominator drift")
        categories.append(
            {
                "category": category,
                "programContextCount": program_count,
                "operationCount": operation_count,
                "rawInstructionContextCount": raw_instruction_count,
            }
        )
    if sum(row["programContextCount"] for row in categories) != 914:
        raise ValueError("map-event direct-state retained program denominator drift")
    if sum(row["operationCount"] for row in categories) != 3579:
        raise ValueError("map-event direct-state retained operation denominator drift")
    if sum(row["rawInstructionContextCount"] for row in categories) != 272:
        raise ValueError("map-event direct-state retained raw instruction denominator drift")
    return {"categories": categories}


def _without_comments(statement: str) -> str:
    return statement.split(";", maxsplit=1)[0].strip()


def _normalise_statement(statement: str) -> str:
    statement = _without_comments(statement)
    inline_label = _INLINE_LABEL.match(statement)
    if inline_label is not None:
        statement = inline_label.group(1)
    return re.sub(r"\s*,\s*", ",", re.sub(r"\s+", " ", statement.strip()))


def _parse_number(token: str) -> int | None:
    if token.startswith("$"):
        return int(token[1:], 16)
    if token.lstrip("-").isdigit():
        return int(token, 10)
    return None


def _parse_equates(source: str, *, source_path: str) -> dict[str, dict[str, int]]:
    definitions: dict[str, dict[str, int]] = {}
    for source_line, raw_line in enumerate(source.splitlines(), start=1):
        match = _EQUATE.match(raw_line)
        if match is None:
            continue
        value = _parse_number(match.group(2))
        if value is None:
            continue
        symbol = match.group(1)
        # The enum source deliberately reuses some names in later source
        # sections.  Match the source's final assembly definition while the
        # direct-state use sites below still reject any missing immediate name.
        definitions[symbol] = {"sourceLine": source_line, "value": value}
    return definitions


def _expected_source_statement(operation: dict[str, Any]) -> str:
    operands = operation["operandTexts"]
    return operation["sourceMnemonic"] + (" " + ",".join(operands) if operands else "")


def _h1_instruction_rows(listing_text: str) -> dict[int, tuple[bytes, str]]:
    rows: dict[int, tuple[bytes, str]] = {}
    for raw_line in listing_text.splitlines():
        match = _H1_INSTRUCTION.match(raw_line)
        if match is None:
            continue
        statement = _normalise_statement(match.group(3))
        if not statement:
            continue
        address = int(match.group(1), 16)
        encoded = bytes.fromhex("".join(match.group(2).split()))
        existing = rows.get(address)
        row = (encoded, statement)
        if existing is not None and existing != row:
            raise ValueError(f"map-event direct-state ambiguous H1 instruction: {address:#x}")
        rows[address] = row
    return rows


def _direct_symbol(operand: str) -> str | None:
    match = _DIRECT_RAM_OPERAND.fullmatch(operand)
    return None if match is None else match.group(1)


def _value_descriptor(
    operand: str | None, *, enum_definitions: dict[str, dict[str, int]]
) -> dict[str, Any]:
    if operand is None:
        return {"valueKind": "other", "valueToken": None, "resolvedValue": None}
    ram_symbol = _direct_symbol(operand)
    if ram_symbol is not None:
        return {
            "valueKind": "ram-copy",
            "valueToken": operand,
            "resolvedValue": None,
        }
    if _REGISTER.fullmatch(operand) is not None:
        return {"valueKind": "register", "valueToken": operand, "resolvedValue": None}
    if operand.startswith("#"):
        immediate = operand[1:]
        number = _parse_number(immediate)
        if number is not None:
            return {
                "valueKind": "immediate-number",
                "valueToken": operand,
                "resolvedValue": number,
            }
        enum = enum_definitions.get(immediate)
        if enum is None:
            raise ValueError(f"map-event direct-state unresolved immediate enum: {immediate}")
        return {
            "valueKind": "immediate-enum",
            "valueToken": operand,
            "resolvedValue": enum["value"],
        }
    return {"valueKind": "other", "valueToken": operand, "resolvedValue": None}


def _direct_access_positions(operation: dict[str, Any]) -> list[tuple[int, str, str | None]]:
    """Classify direct fixed-RAM operands by source mnemonic and operand position."""
    mnemonic = operation["mnemonic"]
    operands = operation["operandTexts"]
    direct_positions = [index for index, operand in enumerate(operands) if _direct_symbol(operand)]
    if not direct_positions:
        return []
    if operation["sizeSuffix"] not in (".b", ".w", ".l"):
        raise ValueError("map-event direct-state direct instruction width drift")
    if mnemonic == "move":
        if len(operands) != 2:
            raise ValueError("map-event direct-state move operand count drift")
        return [
            (index, "read" if index == 0 else "write", operands[1 - index])
            for index in direct_positions
        ]
    if mnemonic == "cmpi":
        if direct_positions != [1] or len(operands) != 2:
            raise ValueError("map-event direct-state cmpi operand-position drift")
        return [(1, "read", operands[0])]
    if mnemonic == "tst":
        if direct_positions != [0] or len(operands) != 1:
            raise ValueError("map-event direct-state tst operand-position drift")
        return [(0, "read", None)]
    if mnemonic == "clr":
        if direct_positions != [0] or len(operands) != 1:
            raise ValueError("map-event direct-state clr operand-position drift")
        return [(0, "write", None)]
    raise ValueError(f"map-event direct-state unclassified direct mnemonic: {mnemonic}")


def _source_table_rows(map_events: dict[str, Any]) -> dict[tuple[str, str], dict[str, Any]]:
    rows: dict[tuple[str, str], dict[str, Any]] = {}
    for category in _CATEGORIES:
        for source_file in map_events["categories"][category]["sourceFiles"]:
            key = (category, source_file["path"])
            if key in rows:
                raise ValueError("map-event direct-state duplicate retained source table path")
            rows[key] = source_file
    return rows


def _direct_state_projection(
    map_events: dict[str, Any],
    *,
    upstream_path: Path,
    rom_path: Path,
    map_events_output_sha256: str | None = None,
) -> tuple[dict[str, Any], dict[str, Any], dict[str, Any], dict[str, Any]]:
    """Scan all retained program contexts and retain only direct state access rows."""
    disasm = upstream_path.resolve(strict=True) / "disasm"
    if not disasm.is_dir():
        disasm = upstream_path.resolve(strict=True)
    const_path = disasm / "sf2const.asm"
    enums_path = disasm / "sf2enums.asm"
    listing_path = upstream_path.resolve(strict=True) / "build/sf2build-h1.lst"
    const_source = const_path.read_text(encoding="utf-8")
    enums_source = enums_path.read_text(encoding="utf-8")
    listing_text = listing_path.read_text(encoding="utf-8")
    rom = rom_path.resolve(strict=True).read_bytes()
    constant_definitions = _parse_equates(const_source, source_path="sf2const.asm")
    enum_definitions = _parse_equates(enums_source, source_path="sf2enums.asm")
    if set(_DIRECT_SYMBOLS) - set(constant_definitions):
        raise ValueError("map-event direct-state fixed-RAM symbol definition drift")
    symbol_definitions = [
        {
            "symbol": symbol,
            "address": constant_definitions[symbol]["value"],
            "sourcePath": "sf2const.asm",
            "sourceLine": constant_definitions[symbol]["sourceLine"],
        }
        for symbol in _DIRECT_SYMBOLS
    ]
    h1_rows = _h1_instruction_rows(listing_text)
    table_rows = _source_table_rows(map_events)
    source_text: dict[str, list[str]] = {}
    access_sites: list[dict[str, Any]] = []
    positive_programs: dict[tuple[str, str, int], dict[str, Any]] = {}
    positive_source_files: dict[tuple[str, str], dict[str, Any]] = {}
    physical_instruction_keys: set[int] = set()
    contextual_instruction_keys: set[tuple[str, str, int, int]] = set()
    physical_access_keys: set[tuple[int, int, str]] = set()

    for category in _CATEGORIES:
        for program in map_events[_PROGRAM_FIELDS[category]]:
            if program["sourcePath"] not in source_text:
                source_text[program["sourcePath"]] = (
                    (disasm / program["sourcePath"]).read_text(encoding="utf-8").splitlines()
                )
            lines = source_text[program["sourcePath"]]
            program_key = (category, program["canonicalSymbol"], program["entryAddress"])
            for operation in program["operations"]:
                if operation["family"] != "raw-68000-instruction":
                    continue
                source_line = operation["sourceLine"]
                if not 1 <= source_line <= len(lines):
                    raise ValueError("map-event direct-state source-line range drift")
                expected_statement = _normalise_statement(_expected_source_statement(operation))
                if _normalise_statement(lines[source_line - 1]) != expected_statement:
                    raise ValueError(
                        "map-event direct-state source mnemonic/operand-order drift: "
                        f"{program['canonicalSymbol']}:{source_line}"
                    )
                direct_positions = _direct_access_positions(operation)
                if not direct_positions:
                    continue
                table = table_rows.get((category, program["sourcePath"]))
                if table is None:
                    raise ValueError(
                        "map-event direct-state retained direct-use/table source join drift"
                    )
                h1_row = h1_rows.get(operation["address"])
                if h1_row is None or h1_row[1] != expected_statement:
                    raise ValueError(
                        "map-event direct-state H1 mnemonic/operand-order drift: "
                        f"{program['canonicalSymbol']}:{source_line}"
                    )
                instruction_bytes = h1_row[0]
                rom_slice = rom[
                    operation["address"] : operation["address"] + len(instruction_bytes)
                ]
                if rom_slice != instruction_bytes:
                    raise ValueError(
                        "map-event direct-state H1/ROM instruction-byte drift: "
                        f"{program['canonicalSymbol']}:{source_line}"
                    )
                contextual_instruction_keys.add(
                    (
                        category,
                        program["canonicalSymbol"],
                        program["entryAddress"],
                        operation["address"],
                    )
                )
                physical_instruction_keys.add(operation["address"])
                positive_programs.setdefault(
                    program_key,
                    {
                        "category": category,
                        "programSymbol": program["canonicalSymbol"],
                        "programEntryAddress": program["entryAddress"],
                        "sourcePath": program["sourcePath"],
                        "tableSymbol": table["symbol"],
                        "tableEntryAddress": table["address"],
                        "contextInstructionSiteCount": 0,
                        "contextAccessSiteCount": 0,
                    },
                )
                positive_source_files.setdefault(
                    (category, table["symbol"]),
                    {
                        "category": category,
                        "tableSymbol": table["symbol"],
                        "tableEntryAddress": table["address"],
                        "sourcePath": table["path"],
                        "programContextCount": 0,
                        "contextInstructionSiteCount": 0,
                        "contextAccessSiteCount": 0,
                    },
                )
                positive_programs[program_key]["contextInstructionSiteCount"] += 1
                positive_source_files[(category, table["symbol"])][
                    "contextInstructionSiteCount"
                ] += 1
                for operand_index, access_kind, value_operand in direct_positions:
                    symbol = _direct_symbol(operation["operandTexts"][operand_index])
                    if symbol not in _DIRECT_SYMBOLS:
                        raise ValueError("map-event direct-state unexpected fixed-RAM symbol")
                    definition = constant_definitions[symbol]
                    value = _value_descriptor(value_operand, enum_definitions=enum_definitions)
                    access_sites.append(
                        {
                            "siteOrder": len(access_sites),
                            "category": category,
                            "programSymbol": program["canonicalSymbol"],
                            "programEntryAddress": program["entryAddress"],
                            "tableSymbol": table["symbol"],
                            "sourcePath": program["sourcePath"],
                            "sourceLine": source_line,
                            "romPc": operation["address"],
                            "mnemonic": operation["mnemonic"],
                            "width": operation["sizeSuffix"][1:],
                            "operandTexts": operation["operandTexts"],
                            "accessOperandIndex": operand_index,
                            "symbol": symbol,
                            "address": definition["value"],
                            "accessKind": access_kind,
                            "instructionByteLength": len(instruction_bytes),
                            "instructionSha256": hashlib.sha256(instruction_bytes)
                            .hexdigest()
                            .upper(),
                            **value,
                        }
                    )
                    physical_access_keys.add((operation["address"], operand_index, symbol))
                    positive_programs[program_key]["contextAccessSiteCount"] += 1
                    positive_source_files[(category, table["symbol"])][
                        "contextAccessSiteCount"
                    ] += 1

    for profile in positive_programs.values():
        positive_source_files[(profile["category"], profile["tableSymbol"])][
            "programContextCount"
        ] += 1
    program_profiles = list(positive_programs.values())
    source_file_rows = list(positive_source_files.values())
    source_files = {row["tableSymbol"]: row for row in source_file_rows}
    category_summaries: list[dict[str, Any]] = []
    for category in _CATEGORIES:
        programs = map_events[_PROGRAM_FIELDS[category]]
        category_sites = [row for row in access_sites if row["category"] == category]
        contextual_instruction_count = len(
            {
                (row["programSymbol"], row["programEntryAddress"], row["romPc"])
                for row in category_sites
            }
        )
        physical_instruction_count = len({row["romPc"] for row in category_sites})
        physical_access_count = len(
            {(row["romPc"], row["accessOperandIndex"], row["symbol"]) for row in category_sites}
        )
        positive_program_count = sum(row["category"] == category for row in program_profiles)
        category_summaries.append(
            {
                "category": category,
                "programContextCount": len(programs),
                "positiveDirectProgramContextCount": positive_program_count,
                "zeroDirectProgramContextCount": len(programs) - positive_program_count,
                "contextInstructionSiteCount": contextual_instruction_count,
                "physicalInstructionSiteCount": physical_instruction_count,
                "contextAccessSiteCount": len(category_sites),
                "physicalAccessSiteCount": physical_access_count,
                "sourceFileCount": sum(row["category"] == category for row in source_file_rows),
            }
        )
    summary = {
        "sourceIdentityCount": len(source_file_rows) + 2,
        "programContextCount": sum(row["programContextCount"] for row in category_summaries),
        "positiveDirectProgramContextCount": len(program_profiles),
        "zeroDirectProgramContextCount": sum(
            row["zeroDirectProgramContextCount"] for row in category_summaries
        ),
        "contextInstructionSiteCount": len(contextual_instruction_keys),
        "physicalInstructionSiteCount": len(physical_instruction_keys),
        "contextAccessSiteCount": len(access_sites),
        "physicalAccessSiteCount": len(physical_access_keys),
        "symbolDefinitionCount": len(symbol_definitions),
        "sourceFileCount": len(source_file_rows),
    }
    expected_summary = {
        "sourceIdentityCount": 40,
        "programContextCount": 914,
        "positiveDirectProgramContextCount": 65,
        "zeroDirectProgramContextCount": 849,
        "contextInstructionSiteCount": 127,
        "physicalInstructionSiteCount": 124,
        "contextAccessSiteCount": 152,
        "physicalAccessSiteCount": 148,
        "symbolDefinitionCount": 13,
        "sourceFileCount": 38,
    }
    if summary != expected_summary:
        raise ValueError("map-event direct-state source/H1/ROM denominator drift")
    return (
        {
            "symbolDefinitions": symbol_definitions,
            "symbolDefinitionOrder": [
                f"{row['symbol']}:{row['address']}" for row in symbol_definitions
            ],
            "categorySummaries": category_summaries,
            "accessSites": access_sites,
            "accessSiteOrder": [
                "|".join(
                    (
                        row["category"],
                        row["programSymbol"],
                        str(row["programEntryAddress"]),
                        str(row["romPc"]),
                        str(row["accessOperandIndex"]),
                        row["symbol"],
                    )
                )
                for row in access_sites
            ],
            "programProfiles": program_profiles,
            "programProfileOrder": [
                f"{row['category']}|{row['programSymbol']}|{row['programEntryAddress']}"
                for row in program_profiles
            ],
            "sourceFiles": source_files,
            "sourceFileOrder": [
                f"{row['category']}|{row['tableSymbol']}|{row['tableEntryAddress']}"
                for row in source_file_rows
            ],
            "digests": {
                "accessSitesSha256": hashlib.sha256(
                    canonical_json_bytes({"accessSites": access_sites})
                )
                .hexdigest()
                .upper(),
                "programProfilesSha256": hashlib.sha256(
                    canonical_json_bytes({"programProfiles": program_profiles})
                )
                .hexdigest()
                .upper(),
                "sourceFilesSha256": hashlib.sha256(
                    canonical_json_bytes({"sourceFiles": source_files})
                )
                .hexdigest()
                .upper(),
            },
        },
        summary,
        {
            "h1Listing": {
                "path": "build/sf2build-h1.lst",
                "sha256": hashlib.sha256(listing_text.encode("utf-8")).hexdigest().upper(),
            },
            "sourceIdentities": [
                {
                    "path": row["sourcePath"],
                    "sha256": hashlib.sha256((disasm / row["sourcePath"]).read_bytes())
                    .hexdigest()
                    .upper(),
                }
                for row in source_file_rows
            ]
            + [
                {
                    "path": "sf2const.asm",
                    "sha256": hashlib.sha256(const_path.read_bytes()).hexdigest().upper(),
                },
                {
                    "path": "sf2enums.asm",
                    "sha256": hashlib.sha256(enums_path.read_bytes()).hexdigest().upper(),
                },
            ],
        },
        {
            "categories": _mother_corpus_projection(map_events)["categories"],
            # The fresh owner supplies its canonical digest.  The fallback is
            # used only by the focused projection tests, which intentionally
            # consume the retained parent fixture as a read-only input.
            "mapEventsOutputSha256": map_events_output_sha256
            or load_json(MAP_EVENTS_MANIFEST)["outputSha256"],
        },
    )


def build_map_event_direct_state_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    """Build the direct-state contract from fresh retained source/H1/ROM owners."""
    map_events, _map_data, retained_owners = _fresh_retained_mother_corpus(rom_path, upstream_path)
    event_direct_state, summary, source_context, mother_corpus = _direct_state_projection(
        map_events,
        upstream_path=upstream_path,
        rom_path=rom_path,
        map_events_output_sha256=retained_owners["mapEvents"]["outputSha256"],
    )
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": map_events["upstream"],
        "romSha256": map_events["romSha256"],
        "sourceContext": {**source_context, "motherCorpus": mother_corpus},
        "retainedOwners": retained_owners,
        "summary": summary,
        "eventDirectState": event_direct_state,
        "unknowns": {key: "Unknown" for key in _UNKNOWN_KEYS},
    }


def verify_map_event_direct_state_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner=str(FIXTURE))
    output = build_map_event_direct_state_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="map-event direct-state static contract")
    if fixture != output:
        raise ValueError("map-event direct-state complete semantic fixture drift")
    digest = hashlib.sha256(canonical_json_bytes(output)).hexdigest().upper()
    destination = output_path or repo_path("local/derived/map-event-direct-state-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(
        json.dumps(output, ensure_ascii=False, indent=2).encode("utf-8") + b"\n"
    )
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Programs": output["summary"]["positiveDirectProgramContextCount"],
        "Accesses": output["summary"]["contextAccessSiteCount"],
        "Status": "PASS",
    }
