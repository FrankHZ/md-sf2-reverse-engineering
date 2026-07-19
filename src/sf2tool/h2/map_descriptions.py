from __future__ import annotations

import hashlib
import json
import re
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.h2.map_setup import build_map_setup_contract
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.source_text import read_upstream_text

ID = "sf2-map-descriptions-static-v1"
SOURCE_ROOT = Path("data/maps/entries")
DISPATCH_PATH = Path("code/common/scripting/map/mapsetupsfunctions_1.asm")
CALLER_PATH = Path("code/gameflow/exploration/explorationvints.asm")
CHECK_AREA_PATH = Path("code/gameflow/exploration/explorationfunctions_0.asm")
MANIFEST = repo_path("manifests/extractions/map-descriptions-static.json")
SCHEMA = repo_path("schemas/map-descriptions-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/map-descriptions-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-map-descriptions-static-fixture.schema.json")

RECORD_MACROS = {
    "msDesc": "text",
    "msDescFunction": "function",
    "msDescFunctionD6": "conditionedFunction",
    "msDescEnd": "end",
}
FUNCTION_SYMBOLS = ("RunMapSetupAreaDescription", "DisplayAreaDescription")


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _require_ordered(source: str, fragments: tuple[str, ...], owner: str) -> None:
    position = -1
    for fragment in fragments:
        position = source.find(fragment, position + 1)
        if position < 0:
            raise ValueError(f"{owner} source-shape drift: missing or reordered {fragment!r}")


def _parse_number(value: str) -> int:
    return int(value[1:], 16) if value.startswith("$") else int(value)


def _instruction_tokens(source: str) -> list[str]:
    tokens: list[str] = []
    for raw_line in source.splitlines():
        line = raw_line.split(";", 1)[0].strip()
        line = re.sub(r"^[A-Za-z_][A-Za-z0-9_]*:\s*", "", line).strip()
        if line:
            tokens.append(line)
    return tokens


def _source_rows(
    disasm: Path, addresses: dict[str, int], display_address: int, rom: bytes
) -> list[dict[str, Any]]:
    paths = sorted(
        (
            path
            for path in (disasm / SOURCE_ROOT).rglob("s4_descriptions*.asm")
            if "mapsetups" in path.parts
        ),
        key=lambda path: path.as_posix(),
    )
    if len(paths) != 75:
        raise ValueError(f"map description source boundary drift: {len(paths)} files")
    files: list[dict[str, Any]] = []
    for path in paths:
        source = read_upstream_text(path)
        labels = re.findall(r"^([A-Za-z_][A-Za-z0-9_]*):", source, re.MULTILINE)
        if not labels or labels[0] not in addresses:
            raise ValueError(f"map description source has no H1-bound entry label: {path}")
        symbol = labels[0]
        address = addresses[symbol]
        table_match = re.search(
            r"\blea\s+([A-Za-z_][A-Za-z0-9_]*)\(pc\),\s*a0", source
        )
        is_stub = table_match is None
        if is_stub:
            if _instruction_tokens(source) != ["rts"] or rom[address : address + 2] != b"\x4E\x75":
                raise ValueError(f"area-description direct-return stub drift: {symbol}")
            files.append(
                {
                    "path": path.relative_to(disasm).as_posix(),
                    "symbol": symbol,
                    "address": address,
                    "directReturnStub": True,
                    "tableSymbol": None,
                    "tableAddress": None,
                    "descriptionTextBase": None,
                    "entryCount": 0,
                    "macroCounts": {},
                    "entries": [],
                    "terminatorAddress": None,
                }
            )
            continue

        table_symbol = table_match.group(1)
        if table_symbol not in addresses:
            raise ValueError(f"description table absent from H1 listing: {table_symbol}")
        table_address = addresses[table_symbol]
        base_match = re.search(r"\bmove\.w\s+#(\$[0-9A-Fa-f]+|\d+),d3\b", source)
        if base_match is None:
            raise ValueError(f"description wrapper has no text base: {symbol}")
        description_text_base = _parse_number(base_match.group(1))
        expected_wrapper = (
            b"\x36\x3C"
            + description_text_base.to_bytes(2, "big")
            + b"\x41\xFA\x00\x0A\x4E\x71\x4E\xF9"
            + display_address.to_bytes(4, "big")
        )
        if table_address != address + 16 or rom[address:table_address] != expected_wrapper:
            raise ValueError(f"description wrapper source/ROM parity drift: {symbol}")

        kinds: list[str] = []
        macro_counts: Counter[str] = Counter()
        for raw_line in source.splitlines():
            line = raw_line.split(";", 1)[0].strip()
            line = re.sub(r"^[A-Za-z_][A-Za-z0-9_]*:\s*", "", line)
            match = re.match(r"([A-Za-z_][A-Za-z0-9_]*)\b", line)
            if match and match.group(1) in RECORD_MACROS:
                macro = match.group(1)
                kinds.append(RECORD_MACROS[macro])
                macro_counts[macro] += 1
        if not kinds or kinds[-1] != "end" or kinds.count("end") != 1:
            raise ValueError(f"description table terminator shape drift: {symbol}")

        entries: list[dict[str, Any]] = []
        cursor = table_address
        for expected_kind in kinds[:-1]:
            raw = rom[cursor : cursor + 6]
            entry = _decode_entry(table_address, cursor, raw, description_text_base)
            if entry["kind"] != expected_kind:
                raise ValueError(f"description source/ROM entry drift at 0x{cursor:X}")
            entries.append(entry)
            cursor += 6
        if rom[cursor : cursor + 2] != b"\xFD\x00":
            raise ValueError(f"description terminator ROM drift: {symbol}")
        files.append(
            {
                "path": path.relative_to(disasm).as_posix(),
                "symbol": symbol,
                "address": address,
                "directReturnStub": False,
                "tableSymbol": table_symbol,
                "tableAddress": table_address,
                "descriptionTextBase": description_text_base,
                "entryCount": len(entries),
                "macroCounts": dict(sorted(macro_counts.items())),
                "entries": entries,
                "terminatorAddress": cursor,
            }
        )
    return files


def _decode_entry(
    table_address: int, record_address: int, data: bytes, description_text_base: int
) -> dict[str, Any]:
    if len(data) != 6:
        raise ValueError("area-description entry must contain six bytes")
    condition_byte = data[2]
    payload_kind = data[3]
    if payload_kind == 0 and condition_byte == 0:
        return {
            "address": record_address,
            "kind": "text",
            "x": data[0],
            "y": data[1],
            "conditionByte": condition_byte,
            "payloadKind": payload_kind,
            "investigationTextOffset": data[4],
            "investigationTextIndex": 423 + data[4],
            "descriptionTextOffset": data[5],
            "descriptionTextIndex": description_text_base + data[5],
        }
    if payload_kind == 1:
        relative_offset = int.from_bytes(data[4:], "big", signed=True)
        return {
            "address": record_address,
            "kind": "conditionedFunction" if condition_byte else "function",
            "x": data[0],
            "y": data[1],
            "conditionByte": condition_byte,
            "payloadKind": payload_kind,
            "relativeOffset": relative_offset,
            "resolvedTargetAddress": table_address + relative_offset,
        }
    raise ValueError(f"unknown area-description payload at 0x{record_address:X}")


def _consumer_facts(disasm: Path) -> dict[str, Any]:
    dispatch = read_upstream_text(disasm / DISPATCH_PATH)
    caller = read_upstream_text(disasm / CALLER_PATH)
    check_area = read_upstream_text(disasm / CHECK_AREA_PATH)
    _require_ordered(
        dispatch,
        (
            "RunMapSetupAreaDescription:",
            "movea.l MAPSETUP_OFFSET_AREA_DESCRIPTIONS(a0),a0",
            "jsr     (a0)",
            "DisplayAreaDescription:",
            "cmpi.b  #$FD,(a0,d7.w)",
            "cmp.w   (a0,d7.w),d0",
            "tst.b   2(a0,d7.w)",
            "tst.w   d6",
            "tst.b   3(a0,d7.w)",
            "addi.w  #423,d0",
            "add.w   d3,d0",
            "adda.w  4(a0,d7.w),a0",
            "addq.w  #6,d7",
        ),
        "area-description dispatcher",
    )
    _require_ordered(
        caller,
        ("moveq   #1,d6", "jsr     CheckArea"),
        "normal area-check caller",
    )
    _require_ordered(
        check_area,
        (
            "CheckArea:",
            "jsr     j_RunMapSetupAreaDescription",
            "tst.w   d6",
        ),
        "area-check dispatch",
    )
    code_sources = "\n".join(
        read_upstream_text(path)
        for path in sorted((disasm / "code").rglob("*.asm"), key=lambda path: path.as_posix())
    )
    normal_call_count = len(
        re.findall(r"\bjsr\s+j_RunMapSetupAreaDescription\b", code_sources)
    )
    if normal_call_count != 1:
        raise ValueError("area-description normal caller count drift")
    return {
        "setupTargetKind": "callable-wrapper-or-direct-return-stub",
        "wrapperBytes": 16,
        "entryBytes": 6,
        "terminatorBytes": 2,
        "terminatorFirstByte": 0xFD,
        "coordinateMatch": "packed-x-y-word",
        "matchOrder": "first-matching-entry",
        "textInvestigationIndexBase": 423,
        "textDescriptionIndexBaseRegister": "d3",
        "functionRelativeOffsetsResolveFromTableBase": True,
        "nonzeroConditionByteRequiresZeroD6": True,
        "normalExplorationCallCount": normal_call_count,
        "normalExplorationD6Value": 1,
        "normalExplorationD6Meaning": "no-entity-event",
        "conditionedFunctionsSkippedInNormalExploration": True,
    }


def build_map_descriptions_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"map descriptions H1 listing is missing: {listing_path}")
    addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))
    rom = rom_path.read_bytes()
    setup = build_map_setup_contract(rom_path, upstream_path)
    if setup["upstream"]["commit"] != commit:
        raise ValueError("map descriptions/setup provenance drift")

    files = _source_rows(disasm, addresses, addresses["DisplayAreaDescription"], rom)
    targets = [table["targets"]["areaDescriptions"] for table in setup["pointerTables"]]
    target_counts = Counter(target["symbol"] for target in targets)
    if set(target_counts) != {row["symbol"] for row in files}:
        raise ValueError("setup pointers do not own the complete description source boundary")
    by_symbol = {row["symbol"]: row for row in files}
    physical_kinds = Counter(
        entry["kind"] for row in files for entry in row["entries"]
    )
    setup_kinds: Counter[str] = Counter()
    for target in targets:
        setup_kinds.update(entry["kind"] for entry in by_symbol[target["symbol"]]["entries"])
    macro_counts: Counter[str] = Counter()
    for row in files:
        macro_counts.update(row["macroCounts"])
    conditioned_functions = [
        {"wrapperSymbol": row["symbol"], "tableSymbol": row["tableSymbol"], **entry}
        for row in files
        for entry in row["entries"]
        if entry["kind"] == "conditionedFunction"
    ]
    summary = {
        "sourceFileCount": len(files),
        "setupPointerReferenceCount": len(targets),
        "uniqueTargetCount": len(target_counts),
        "aliasedTargetCount": sum(count > 1 for count in target_counts.values()),
        "wrapperCount": sum(not row["directReturnStub"] for row in files),
        "directReturnStubCount": sum(row["directReturnStub"] for row in files),
        "wrapperSetupReferenceCount": sum(
            target_counts[row["symbol"]] for row in files if not row["directReturnStub"]
        ),
        "directReturnStubReferenceCount": sum(
            target_counts[row["symbol"]] for row in files if row["directReturnStub"]
        ),
        "physicalEntryCount": sum(physical_kinds.values()),
        "textPhysicalEntryCount": physical_kinds["text"],
        "functionPhysicalEntryCount": physical_kinds["function"],
        "conditionedFunctionPhysicalEntryCount": physical_kinds["conditionedFunction"],
        "setupEntryReferenceCount": sum(setup_kinds.values()),
        "textSetupEntryReferenceCount": setup_kinds["text"],
        "functionSetupEntryReferenceCount": setup_kinds["function"],
        "conditionedFunctionSetupEntryReferenceCount": setup_kinds["conditionedFunction"],
        "terminatorCount": sum(not row["directReturnStub"] for row in files),
        "physicalTableByteCount": sum(
            row["entryCount"] * 6 + 2 for row in files if not row["directReturnStub"]
        ),
        "maximumTableEntryCount": max(row["entryCount"] for row in files),
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "romSha256": setup["romSha256"],
        "scope": f"{SOURCE_ROOT.as_posix()}/*/mapsetups/s4_descriptions*.asm",
        "function": {symbol: addresses[symbol] for symbol in FUNCTION_SYMBOLS},
        "summary": summary,
        "sourceMacroCounts": dict(sorted(macro_counts.items())),
        "physicalEntryKinds": dict(sorted(physical_kinds.items())),
        "setupReferenceEntryKinds": dict(sorted(setup_kinds.items())),
        "duplicatePointerTargets": [
            {"symbol": symbol, "setupReferenceCount": count}
            for symbol, count in sorted(target_counts.items())
            if count > 1
        ],
        "conditionedFunctions": conditioned_functions,
        "consumerFacts": _consumer_facts(disasm),
        "runtimeQuestions": [
            "description-conditioned-functions-under-direct-or-mutated-d6-callers",
            "description-function-side-effects-and-transition-persistence",
            "description-text-portrait-and-window-timing",
        ],
        "sourceFiles": files,
    }


def verify_map_descriptions_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_map_descriptions_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="map descriptions static contract")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != output["romSha256"]
        or fixture["function"] != output["function"]
    ):
        raise ValueError("map descriptions provenance/address drift")
    for field in (
        "summary",
        "sourceMacroCounts",
        "physicalEntryKinds",
        "setupReferenceEntryKinds",
        "conditionedFunctions",
        "consumerFacts",
        "runtimeQuestions",
    ):
        if fixture["expected"][field] != output[field]:
            raise ValueError(f"map descriptions {field} drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"] or output["summary"] != manifest["summary"]:
        raise ValueError("map descriptions canonical output drift")
    destination = output_path or repo_path("local/derived/map-descriptions-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "SourceFiles": output["summary"]["sourceFileCount"],
        "Wrappers": output["summary"]["wrapperCount"],
        "DirectReturnStubs": output["summary"]["directReturnStubCount"],
        "PhysicalEntries": output["summary"]["physicalEntryCount"],
        "SetupReferences": output["summary"]["setupEntryReferenceCount"],
        "Status": "PASS",
    }
