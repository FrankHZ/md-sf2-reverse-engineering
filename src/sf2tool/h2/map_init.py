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

ID = "sf2-map-init-static-v1"
SOURCE_ROOT = Path("data/maps/entries")
DISPATCH_PATH = Path("code/common/scripting/map/mapsetupsfunctions_1.asm")
MANIFEST = repo_path("manifests/extractions/map-init-static.json")
SCHEMA = repo_path("schemas/map-init-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/map-init-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-map-init-static-fixture.schema.json")


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _function_body(source: str, symbol: str, owner_symbol: str) -> str:
    start = re.search(rf"(?m)^{re.escape(symbol)}:\s*$", source)
    if start is None:
        raise ValueError(f"map init entry label missing: {symbol}")
    end = source.find(f"End of function {owner_symbol}", start.end())
    if end < 0:
        raise ValueError(f"map init function boundary missing: {symbol}")
    return source[start.end() : end]


def _operation_rows(body: str) -> list[dict[str, Any]]:
    rows: list[dict[str, Any]] = []
    pending_labels: list[str] = []
    for raw_line in body.splitlines():
        line = raw_line.split(";", 1)[0].strip()
        while line and (label_match := re.match(r"^(@?[A-Za-z_][A-Za-z0-9_]*):\s*", line)):
            pending_labels.append(label_match.group(1))
            line = line[label_match.end() :].strip()
        if not line:
            continue
        parts = line.split(None, 1)
        opcode = parts[0]
        operand_text = parts[1] if len(parts) == 2 else ""
        rows.append(
            {
                "index": len(rows),
                "labels": pending_labels,
                "opcode": opcode,
                "operandText": operand_text.strip(),
                "branchTargetSymbol": None,
                "branchTargetAddress": None,
                "localBranchTargetIndex": None,
            }
        )
        pending_labels = []
    if pending_labels:
        raise ValueError(f"map init labels have no following operation: {pending_labels}")
    label_indices: dict[str, int] = {}
    for row in rows:
        for label in row["labels"]:
            if label in label_indices:
                raise ValueError(f"duplicate map init local label: {label}")
            label_indices[label] = row["index"]
    for row in rows:
        if not re.fullmatch(r"b(?!sr)[a-z]{2}(?:\.[bswl])?", row["opcode"]):
            continue
        target = row["operandText"].rsplit(",", 1)[-1].strip()
        target = re.sub(r"\(pc\)$", "", target)
        row["branchTargetSymbol"] = target
        row["localBranchTargetIndex"] = label_indices.get(target)
    return rows


def _statement_rows(body: str) -> list[str]:
    return [
        row["opcode"] + (f" {row['operandText']}" if row["operandText"] else "")
        for row in _operation_rows(body)
    ]


def _call_targets(statements: list[str]) -> list[str]:
    targets: list[str] = []
    for statement in statements:
        match = re.match(r"(?:jsr|bsr)(?:\.[bwl])?\s+\(?([A-Za-z_][A-Za-z0-9_]*)", statement)
        if match:
            targets.append(match.group(1))
    return targets


def _source_rows(
    disasm: Path, addresses: dict[str, int], rom: bytes, target_symbols: set[str]
) -> list[dict[str, Any]]:
    paths = sorted(
        (
            path
            for path in (disasm / SOURCE_ROOT).rglob("s6_initfunction*.asm")
            if "mapsetups" in path.parts
        ),
        key=lambda path: path.as_posix(),
    )
    if len(paths) != 84:
        raise ValueError(f"map init source boundary drift: {len(paths)} files")
    files: list[dict[str, Any]] = []
    for path in paths:
        source = read_upstream_text(path)
        labels = re.findall(r"^([A-Za-z_][A-Za-z0-9_]*):", source, re.MULTILINE)
        if not labels or labels[0] not in addresses:
            raise ValueError(f"map init source has no H1-bound entry label: {path}")
        owner_symbol = labels[0]
        owned_targets = sorted(target_symbols & set(labels), key=labels.index)
        if not owned_targets or owner_symbol not in owned_targets:
            raise ValueError(f"map init source owns no primary setup target: {path}")
        for symbol in owned_targets:
            address = addresses[symbol]
            body = _function_body(source, symbol, owner_symbol)
            operations = _operation_rows(body)
            for operation in operations:
                target = operation["branchTargetSymbol"]
                if target is not None:
                    operation["branchTargetAddress"] = addresses.get(target)
            statements = [
                row["opcode"] + (f" {row['operandText']}" if row["operandText"] else "")
                for row in operations
            ]
            if not statements:
                raise ValueError(f"map init function has no statements: {symbol}")
            direct_return_stub = statements == ["rts"]
            if direct_return_stub and rom[address : address + 2] != b"\x4e\x75":
                raise ValueError(f"map init direct-return stub ROM drift: {symbol}")
            if not direct_return_stub and rom[address : address + 2] == b"\x4e\x75":
                raise ValueError(f"active map init unexpectedly starts with rts: {symbol}")
            tokens = [re.match(r"([A-Za-z.]+)", row).group(1) for row in statements]
            script_targets = [
                match.group(1)
                for statement in statements
                if (match := re.match(r"script\s+([A-Za-z_][A-Za-z0-9_]*)", statement))
            ]
            files.append(
                {
                    "path": path.relative_to(disasm).as_posix(),
                    "symbol": symbol,
                    "address": address,
                    "primarySourceEntry": symbol == owner_symbol,
                    "directReturnStub": direct_return_stub,
                    "statementCount": len(statements),
                    "bodySha256": hashlib.sha256(("\n".join(statements) + "\n").encode())
                    .hexdigest()
                    .upper(),
                    "tokenCounts": dict(sorted(Counter(tokens).items())),
                    "flagCheckCount": tokens.count("chkFlg"),
                    "flagSetCount": tokens.count("setFlg"),
                    "flagClearCount": tokens.count("clrFlg"),
                    "scriptTargets": script_targets,
                    "callTargets": _call_targets(statements),
                    "operations": operations,
                }
            )
    return files


def _consumer_facts(disasm: Path) -> dict[str, Any]:
    source = read_upstream_text(disasm / DISPATCH_PATH)
    fragments = (
        "RunMapSetupInitFunction:",
        "bsr.w   GetCurrentMapSetup",
        "cmpi.w  #-1,(a0)",
        "movea.l MAPSETUP_OFFSET_INIT_FUNCTION(a0),a0",
        "jsr     (a0)",
    )
    position = -1
    for fragment in fragments:
        position = source.find(fragment, position + 1)
        if position < 0:
            raise ValueError(f"map init dispatcher source-shape drift: {fragment!r}")
    return {
        "pointerOffset": 20,
        "selectedSetupCallsExactlyOneInitTarget": True,
        "missingMapVoidSkipsInitCall": True,
        "directReturnTargetsAreNoOpCallables": True,
    }


def build_map_init_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"map init H1 listing is missing: {listing_path}")
    addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))
    rom = rom_path.read_bytes()
    setup = build_map_setup_contract(rom_path, upstream_path)
    if setup["upstream"]["commit"] != commit:
        raise ValueError("map init/setup provenance drift")
    targets = [table["targets"]["initFunction"] for table in setup["pointerTables"]]
    target_counts = Counter(target["symbol"] for target in targets)
    files = _source_rows(disasm, addresses, rom, set(target_counts))
    if set(target_counts) != {row["symbol"] for row in files}:
        raise ValueError("setup pointers do not own the complete map init source boundary")
    by_symbol = {row["symbol"]: row for row in files}
    token_counts: Counter[str] = Counter()
    call_counts: Counter[str] = Counter()
    script_counts: Counter[str] = Counter()
    for row in files:
        if not row["primarySourceEntry"]:
            continue
        token_counts.update(row["tokenCounts"])
        call_counts.update(row["callTargets"])
        script_counts.update(row["scriptTargets"])
    setup_statement_count = sum(by_symbol[target["symbol"]]["statementCount"] for target in targets)
    primary_operations = [
        operation for row in files if row["primarySourceEntry"] for operation in row["operations"]
    ]
    summary = {
        "sourceFileCount": sum(row["primarySourceEntry"] for row in files),
        "setupPointerReferenceCount": len(targets),
        "uniqueTargetCount": len(target_counts),
        "internalEntryTargetCount": sum(not row["primarySourceEntry"] for row in files),
        "aliasedTargetCount": sum(count > 1 for count in target_counts.values()),
        "activeFunctionCount": sum(not row["directReturnStub"] for row in files),
        "directReturnStubCount": sum(row["directReturnStub"] for row in files),
        "activeSetupReferenceCount": sum(
            target_counts[row["symbol"]] for row in files if not row["directReturnStub"]
        ),
        "directReturnStubReferenceCount": sum(
            target_counts[row["symbol"]] for row in files if row["directReturnStub"]
        ),
        "sourceStatementCount": sum(
            row["statementCount"] for row in files if row["primarySourceEntry"]
        ),
        "setupStatementReferenceCount": setup_statement_count,
        "maximumFunctionStatementCount": max(row["statementCount"] for row in files),
        "flagCheckCount": token_counts["chkFlg"],
        "flagSetCount": token_counts["setFlg"],
        "flagClearCount": token_counts["clrFlg"],
        "scriptCallCount": token_counts["script"],
        "uniqueScriptTargetCount": len(script_counts),
        "directCallCount": sum(call_counts.values()),
        "uniqueDirectCallTargetCount": len(call_counts),
        "moveEntityOutOfMapCallCount": call_counts["MoveEntityOutOfMap"],
        "labeledOperationCount": sum(bool(row["labels"]) for row in primary_operations),
        "branchOperationCount": sum(
            bool(re.fullmatch(r"b(?!sr)[a-z]{2}(?:\.[bswl])?", row["opcode"]))
            for row in primary_operations
        ),
        "resolvedLocalBranchCount": sum(
            row["localBranchTargetIndex"] is not None for row in primary_operations
        ),
        "resolvedBranchTargetCount": sum(
            row["localBranchTargetIndex"] is not None or row["branchTargetAddress"] is not None
            for row in primary_operations
        ),
        "externalBranchTargetCount": sum(
            row["localBranchTargetIndex"] is None and row["branchTargetAddress"] is not None
            for row in primary_operations
        ),
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "romSha256": setup["romSha256"],
        "scope": f"{SOURCE_ROOT.as_posix()}/*/mapsetups/s6_initfunction*.asm",
        "function": {"RunMapSetupInitFunction": addresses["RunMapSetupInitFunction"]},
        "summary": summary,
        "entryTokenCounts": dict(sorted(token_counts.items())),
        "directCallTargetCounts": dict(sorted(call_counts.items())),
        "scriptTargetCounts": dict(sorted(script_counts.items())),
        "duplicatePointerTargets": [
            {"symbol": symbol, "setupReferenceCount": count}
            for symbol, count in sorted(target_counts.items())
            if count > 1
        ],
        "consumerFacts": _consumer_facts(disasm),
        "runtimeQuestions": [
            "init-script-side-effects-and-transition-persistence",
            "init-entity-mutation-order-and-visibility-timing",
            "init-fade-audio-and-presentation-sequencing",
        ],
        "sourceFiles": files,
    }


def verify_map_init_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_map_init_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="map init static contract")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != output["romSha256"]
        or fixture["function"] != output["function"]
    ):
        raise ValueError("map init provenance/address drift")
    for field in (
        "summary",
        "entryTokenCounts",
        "directCallTargetCounts",
        "scriptTargetCounts",
        "consumerFacts",
        "runtimeQuestions",
    ):
        if fixture["expected"][field] != output[field]:
            raise ValueError(f"map init {field} drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"] or output["summary"] != manifest["summary"]:
        raise ValueError("map init canonical output drift")
    destination = output_path or repo_path("local/derived/map-init-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "SourceFiles": output["summary"]["sourceFileCount"],
        "ActiveFunctions": output["summary"]["activeFunctionCount"],
        "DirectReturnStubs": output["summary"]["directReturnStubCount"],
        "Statements": output["summary"]["sourceStatementCount"],
        "ScriptCalls": output["summary"]["scriptCallCount"],
        "Status": "PASS",
    }
