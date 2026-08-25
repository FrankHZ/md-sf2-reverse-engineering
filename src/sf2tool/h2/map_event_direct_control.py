"""Public H2 caller-side direct-control topology for map-event programs.

The retained map-events corpus owns target-program parsing.  This owner rebuilds
that corpus and narrows it to source-level direct JSR/BSR/JMP relationships:
instruction and effective targets, jump-interface aliases, callee entry owners,
returning-call lexical continuations, and tail-transfer suffixes.  It never
enters a callee body or assigns a runtime effect.
"""

from __future__ import annotations

import hashlib
import json
import re
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.h2.map_events import _canonical_bytes as _map_events_canonical_bytes
from sf2tool.h2.map_events import build_map_events_contract
from sf2tool.h2.map_events_fixture import load_map_events_fixture
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path

ID = "sf2-map-event-direct-control-static-v1"
FIXTURE = repo_path("tests/fixtures/h2/map-event-direct-control-static-v1.json")
SCHEMA = repo_path("schemas/h2/map-event-direct-control-static-fixture.schema.json")
MAP_EVENTS_MANIFEST = repo_path("manifests/extractions/map-events-static.json")

_CATEGORIES = ("entityEvents", "zoneEvents", "itemEvents")
_PROGRAM_FIELDS = {
    "entityEvents": "entityTargetPrograms",
    "zoneEvents": "zoneTargetPrograms",
    "itemEvents": "itemTargetPrograms",
}
_TRANSFER_MNEMONICS = {"jsr": "direct-call", "bsr": "direct-call", "jmp": "direct-jump"}
_H1_INSTRUCTION = re.compile(
    r"^([0-9A-Fa-f]{8})\s+([0-9A-Fa-f]{2,4}(?: [0-9A-Fa-f]{2,4})*)\s{2,}(.+)$"
)
_INLINE_LABEL = re.compile(r"^[A-Za-z_][A-Za-z0-9_]*:\s*(.*)$")
_UNKNOWN_KEYS = (
    "naturalProgramReachability",
    "callerEntryState",
    "runtimeTransferOrder",
    "preCallRegisterValues",
    "calleeEntryState",
    "calleeSideEffects",
    "calleeReturnRegistersAndCcr",
    "postCallConsumerSelection",
    "tailTransferReturnBehavior",
    "crossMapStateLifetime",
    "saveLoadPersistence",
    "inputUiDialogueAudioTimingAndStoryMeaning",
)


def canonical_json_bytes(value: dict[str, Any]) -> bytes:
    """Emit the one canonical UTF-8 representation for this public fixture."""
    return (
        json.dumps(value, ensure_ascii=False, sort_keys=True, separators=(",", ":")).encode("utf-8")
        + b"\n"
    )


def _normalise_statement(statement: str) -> str:
    statement = statement.split(";", maxsplit=1)[0].strip()
    inline_label = _INLINE_LABEL.match(statement)
    if inline_label is not None:
        statement = inline_label.group(1)
    return re.sub(r"\s*,\s*", ",", re.sub(r"\s+", " ", statement.strip()))


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
        row = (bytes.fromhex("".join(match.group(2).split())), statement)
        existing = rows.get(address)
        if existing is not None and existing != row:
            raise ValueError(f"map-event direct-control ambiguous H1 instruction: {address:#x}")
        rows[address] = row
    return rows


def _fixture_sha256(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest().upper()


def _fresh_retained_map_events(
    rom_path: Path, upstream_path: Path
) -> tuple[dict[str, Any], dict[str, Any]]:
    """Build the retained map-event owner before consuming its projection."""
    map_events = build_map_events_contract(rom_path, upstream_path)
    fixture = load_map_events_fixture()
    if fixture["expected"] != map_events:
        raise ValueError("map-event direct-control retained map-events projection drift")
    digest = hashlib.sha256(_map_events_canonical_bytes(map_events)).hexdigest().upper()
    manifest = load_json(MAP_EVENTS_MANIFEST)
    if digest != manifest["outputSha256"] or map_events["summary"] != manifest["summary"]:
        raise ValueError("map-event direct-control retained map-events digest drift")
    return map_events, {
        "fixtureId": fixture["id"],
        "fixtureSha256": _fixture_sha256(repo_path("tests/fixtures/h2/map-events-static-v1.json")),
        "outputSha256": digest,
        "summary": map_events["summary"],
    }


def _mother_corpus_projection(map_events: dict[str, Any]) -> dict[str, Any]:
    """Derive the complete zero-inclusive program denominator once."""
    expected = {
        "entityEvents": (684, 2624),
        "zoneEvents": (150, 809),
        "itemEvents": (80, 146),
    }
    categories: list[dict[str, Any]] = []
    for category in _CATEGORIES:
        programs = map_events[_PROGRAM_FIELDS[category]]
        row = {
            "category": category,
            "programContextCount": len(programs),
            "operationCount": sum(len(program["operations"]) for program in programs),
        }
        if (
            tuple(row[key] for key in ("programContextCount", "operationCount"))
            != expected[category]
        ):
            raise ValueError(f"map-event direct-control retained {category} denominator drift")
        categories.append(row)
    if sum(row["programContextCount"] for row in categories) != 914:
        raise ValueError("map-event direct-control retained program denominator drift")
    if sum(row["operationCount"] for row in categories) != 3579:
        raise ValueError("map-event direct-control retained operation denominator drift")
    return {"categories": categories}


def _source_table_rows(map_events: dict[str, Any]) -> dict[tuple[str, str], dict[str, Any]]:
    rows: dict[tuple[str, str], dict[str, Any]] = {}
    for category in _CATEGORIES:
        for source_file in map_events["categories"][category]["sourceFiles"]:
            key = (category, source_file["path"])
            if key in rows:
                raise ValueError("map-event direct-control duplicate retained source table path")
            rows[key] = source_file
    return rows


def _operation_shape(operation: dict[str, Any]) -> dict[str, Any]:
    """Retain public structural operation facts, never comments or payload text."""
    return {
        "sourceOrder": operation["sourceOrder"],
        "sourceLine": operation["sourceLine"],
        "romPc": operation["address"],
        "family": operation["family"],
        "mnemonic": operation["mnemonic"],
        "sizeSuffix": operation["sizeSuffix"],
        "operandCount": len(operation["operandTexts"]),
        "controlFlowKind": operation["controlFlowKind"],
    }


def _operation_statement(operation: dict[str, Any]) -> str:
    """Reconstruct the retained lexical source statement without its payload text."""
    operands = operation["operandTexts"]
    source = operation["sourceMnemonic"]
    if operands:
        source += " " + ", ".join(operands)
    return _normalise_statement(source)


def _continuation_kind(operation: dict[str, Any]) -> str:
    if operation["family"] != "raw-68000-control-flow":
        return "ordinary"
    mnemonic = operation["mnemonic"]
    if mnemonic == "rts":
        return "return"
    if mnemonic in {"jsr", "bsr"}:
        return "direct-call"
    if mnemonic == "jmp":
        return "direct-jump"
    if mnemonic == "bra":
        return "unconditional-branch"
    if mnemonic.startswith("b"):
        return "conditional-branch"
    raise ValueError("map-event direct-control continuation control-flow kind drift")


def _assert_source_statement(
    lines: list[str], *, source_line: int, expected: str, context: str
) -> None:
    if not 1 <= source_line <= len(lines):
        raise ValueError(f"map-event direct-control source line range drift: {context}")
    if _normalise_statement(lines[source_line - 1]) != expected:
        raise ValueError(f"map-event direct-control source mnemonic/operand-order drift: {context}")


def _assert_entry_label(lines: list[str], *, source_line: int, symbol: str, context: str) -> None:
    """Guard the H1-resolved source entry label without expanding callee bodies."""
    if not 1 <= source_line <= len(lines):
        raise ValueError(f"map-event direct-control entry label range drift: {context}")
    source_label = lines[source_line - 1].split(";", maxsplit=1)[0].strip()
    if source_label != f"{symbol}:":
        raise ValueError(f"map-event direct-control callee entry label drift: {context}")


def _h1_rom_anchor(
    *,
    anchor_id: str,
    kind: str,
    address: int,
    expected_statement: str | None,
    h1_rows: dict[int, tuple[bytes, str]],
    rom: bytes,
    context: str,
    encoded_target_address: int | None = None,
) -> dict[str, Any]:
    row = h1_rows.get(address)
    if row is None:
        raise ValueError(f"map-event direct-control missing H1 instruction: {context}")
    encoded, statement = row
    if expected_statement is not None and statement != expected_statement:
        raise ValueError(f"map-event direct-control H1 mnemonic/operand-order drift: {context}")
    rom_encoded = rom[address : address + len(encoded)]
    if rom_encoded != encoded:
        # The H1 listing deliberately preserves unresolved branch, PC-relative,
        # and absolute target operands as zero extension words.  The ROM must
        # carry their source-resolved encoding.  Validate target form and
        # opcode/width rather than falsely demanding byte identity.
        opcode = int.from_bytes(encoded[:2], byteorder="big") if len(encoded) >= 2 else -1
        if encoded_target_address is None or encoded[:2] != rom_encoded[:2]:
            raise ValueError(f"map-event direct-control H1/ROM instruction-byte drift: {context}")
        if opcode in {0x6100, 0x4EBA, 0x4EFA} and len(encoded) == 4:
            resolved_target = (
                address + 2 + int.from_bytes(rom_encoded[2:], byteorder="big", signed=True)
            )
        elif (
            opcode in {0x4EB8, 0x4EF8}
            and len(encoded) == 4
            or opcode in {0x4EB9, 0x4EF9}
            and len(encoded) == 6
        ):
            resolved_target = int.from_bytes(rom_encoded[2:], byteorder="big")
        else:
            raise ValueError(f"map-event direct-control H1 relocation form drift: {context}")
        if resolved_target != encoded_target_address:
            raise ValueError(f"map-event direct-control H1/ROM target drift: {context}")
    return {
        "id": anchor_id,
        "kind": kind,
        "romPc": address,
        "instructionByteLength": len(encoded),
        "h1InstructionSha256": hashlib.sha256(encoded).hexdigest().upper(),
        "romInstructionSha256": hashlib.sha256(rom_encoded).hexdigest().upper(),
    }


def _direct_control_projection(
    map_events: dict[str, Any], *, upstream_path: Path, rom_path: Path
) -> tuple[dict[str, Any], dict[str, Any], dict[str, Any]]:
    """Derive direct transfers and reject source/H1/ROM drift before fixture comparison."""
    root = upstream_path.resolve(strict=True)
    disasm = root / "disasm"
    if not disasm.is_dir():
        disasm = root
    listing_path = root / "build/sf2build-h1.lst"
    listing_text = listing_path.read_text(encoding="utf-8")
    h1_rows = _h1_instruction_rows(listing_text)
    rom = rom_path.resolve(strict=True).read_bytes()
    table_rows = _source_table_rows(map_events)
    source_lines: dict[str, list[str]] = {}

    def lines_for(path: str) -> list[str]:
        if path not in source_lines:
            source_lines[path] = (disasm / path).read_text(encoding="utf-8").splitlines()
        return source_lines[path]

    transfer_sites: list[dict[str, Any]] = []
    program_contexts: list[dict[str, Any]] = []
    caller_sources: dict[str, dict[str, Any]] = {}
    physical_contexts: dict[int, list[dict[str, Any]]] = {}
    effective_targets: dict[tuple[str, int], dict[str, Any]] = {}
    alias_joins: dict[tuple[str, int, str, int], dict[str, Any]] = {}
    anchors: dict[str, dict[str, Any]] = {}
    call_continuations: list[dict[str, Any]] = []
    tail_transfers: list[dict[str, Any]] = []

    for category in _CATEGORIES:
        for program in map_events[_PROGRAM_FIELDS[category]]:
            operations = program["operations"]
            transfers = [
                (index, operation)
                for index, operation in enumerate(operations)
                if operation["family"] == "raw-68000-control-flow"
                and operation["mnemonic"] in _TRANSFER_MNEMONICS
            ]
            program_contexts.append(
                {
                    "category": category,
                    "programSymbol": program["canonicalSymbol"],
                    "programEntryAddress": program["entryAddress"],
                    "sourcePath": program["sourcePath"],
                    "transferSiteCount": len(transfers),
                }
            )
            if not transfers:
                continue
            table = table_rows.get((category, program["sourcePath"]))
            if table is None:
                raise ValueError("map-event direct-control retained caller table join drift")
            caller_source = caller_sources.setdefault(
                table["symbol"],
                {
                    "category": category,
                    "tableSymbol": table["symbol"],
                    "tableEntryAddress": table["address"],
                    "sourcePath": table["path"],
                    "programContextCount": 0,
                    "transferSiteCount": 0,
                },
            )
            caller_source["programContextCount"] += 1
            caller_source["transferSiteCount"] += len(transfers)
            for operation_index, operation in transfers:
                target = operation["target"]
                if target is None or len(operation["operandTexts"]) != 1:
                    raise ValueError("map-event direct-control direct target operand drift")
                expected = _normalise_statement(
                    operation["sourceMnemonic"] + " " + operation["operandTexts"][0]
                )
                context = f"{program['canonicalSymbol']}:{operation['sourceLine']}"
                _assert_source_statement(
                    lines_for(program["sourcePath"]),
                    source_line=operation["sourceLine"],
                    expected=expected,
                    context=context,
                )
                instruction_symbol = target["instructionTargetSymbol"]
                instruction_address = target["instructionTargetAddress"]
                effective_symbol = target["effectiveTargetSymbol"]
                effective_address = target["effectiveTargetAddress"]
                anchor_id = f"transfer:{operation['address']:06X}"
                anchors.setdefault(
                    anchor_id,
                    _h1_rom_anchor(
                        anchor_id=anchor_id,
                        kind="transfer-instruction",
                        address=operation["address"],
                        expected_statement=expected,
                        h1_rows=h1_rows,
                        rom=rom,
                        context=context,
                        encoded_target_address=instruction_address,
                    ),
                )
                site = {
                    "siteOrder": len(transfer_sites),
                    "category": category,
                    "programSymbol": program["canonicalSymbol"],
                    "programEntryAddress": program["entryAddress"],
                    "tableSymbol": table["symbol"],
                    "tableEntryAddress": table["address"],
                    "sourcePath": program["sourcePath"],
                    "sourceLine": operation["sourceLine"],
                    "romPc": operation["address"],
                    "transferKind": _TRANSFER_MNEMONICS[operation["mnemonic"]],
                    "sourceMnemonic": operation["sourceMnemonic"],
                    "operandShape": "direct-symbol",
                    "instructionTargetSymbol": instruction_symbol,
                    "instructionTargetAddress": instruction_address,
                    "effectiveTargetSymbol": effective_symbol,
                    "effectiveTargetAddress": effective_address,
                }
                transfer_sites.append(site)
                physical_contexts.setdefault(operation["address"], []).append(site)
                target_key = (effective_symbol, effective_address)
                effective_record = effective_targets.setdefault(
                    target_key,
                    {
                        "symbol": effective_symbol,
                        "entryAddress": effective_address,
                        "sourcePath": target["effectiveTargetAddressLabels"][0]["sourcePath"],
                        "sourceLine": target["effectiveTargetAddressLabels"][0]["sourceLine"],
                        "contextTransferSiteCount": 0,
                        "physicalTransferSiteCount": 0,
                    },
                )
                effective_record["contextTransferSiteCount"] += 1
                if len(physical_contexts[operation["address"]]) == 1:
                    effective_record["physicalTransferSiteCount"] += 1
                if (
                    instruction_symbol != effective_symbol
                    or instruction_address != effective_address
                ):
                    instruction_label = target["instructionTargetAddressLabels"][0]
                    alias_key = (
                        instruction_symbol,
                        instruction_address,
                        effective_symbol,
                        effective_address,
                    )
                    alias_joins.setdefault(
                        alias_key,
                        {
                            "instructionTargetSymbol": instruction_symbol,
                            "instructionTargetAddress": instruction_address,
                            "effectiveTargetSymbol": effective_symbol,
                            "effectiveTargetAddress": effective_address,
                            "sourcePath": instruction_label["sourcePath"],
                            "sourceLine": instruction_label["sourceLine"],
                        },
                    )
                if operation["mnemonic"] in {"jsr", "bsr"}:
                    if operation_index + 1 >= len(operations):
                        raise ValueError(
                            "map-event direct-control returning call has no lexical continuation"
                        )
                    next_operation = operations[operation_index + 1]
                    _assert_source_statement(
                        lines_for(program["sourcePath"]),
                        source_line=next_operation["sourceLine"],
                        expected=_operation_statement(next_operation),
                        context=f"{program['canonicalSymbol']}:{next_operation['sourceLine']}",
                    )
                    call_continuations.append(
                        {
                            "siteOrder": site["siteOrder"],
                            "romPc": operation["address"],
                            "kind": _continuation_kind(next_operation),
                            "operation": _operation_shape(next_operation),
                        }
                    )
                else:
                    suffix = operations[operation_index + 1 :]
                    for suffix_operation in suffix:
                        _assert_source_statement(
                            lines_for(program["sourcePath"]),
                            source_line=suffix_operation["sourceLine"],
                            expected=_operation_statement(suffix_operation),
                            context=(
                                f"{program['canonicalSymbol']}:{suffix_operation['sourceLine']}"
                            ),
                        )
                    tail_transfers.append(
                        {
                            "siteOrder": site["siteOrder"],
                            "romPc": operation["address"],
                            "lexicalSuffix": [_operation_shape(item) for item in suffix],
                        }
                    )

    for alias in alias_joins.values():
        _assert_entry_label(
            lines_for(alias["sourcePath"]),
            source_line=alias["sourceLine"],
            symbol=alias["instructionTargetSymbol"],
            context=alias["instructionTargetSymbol"],
        )
        anchor_id = f"alias:{alias['instructionTargetAddress']:06X}"
        anchors[anchor_id] = _h1_rom_anchor(
            anchor_id=anchor_id,
            kind="jump-interface-alias",
            address=alias["instructionTargetAddress"],
            expected_statement=None,
            h1_rows=h1_rows,
            rom=rom,
            context=alias["instructionTargetSymbol"],
            encoded_target_address=alias["effectiveTargetAddress"],
        )
    for target in effective_targets.values():
        _assert_entry_label(
            lines_for(target["sourcePath"]),
            source_line=target["sourceLine"],
            symbol=target["symbol"],
            context=target["symbol"],
        )
        anchor_id = f"effective:{target['entryAddress']:06X}"
        anchor = _h1_rom_anchor(
            anchor_id=anchor_id,
            kind="effective-callee-entry",
            address=target["entryAddress"],
            expected_statement=None,
            h1_rows=h1_rows,
            rom=rom,
            context=target["symbol"],
        )
        anchors[anchor_id] = anchor
        target["firstInstruction"] = {
            "romPc": anchor["romPc"],
            "instructionByteLength": anchor["instructionByteLength"],
            "h1InstructionSha256": anchor["h1InstructionSha256"],
            "romInstructionSha256": anchor["romInstructionSha256"],
            "statementShape": h1_rows[target["entryAddress"]][1].split(" ", maxsplit=1)[0],
        }

    physical_sites: list[dict[str, Any]] = []
    for rom_pc, sites in physical_contexts.items():
        representative = sites[0]
        if any(
            (site["transferKind"], site["instructionTargetSymbol"], site["effectiveTargetSymbol"])
            != (
                representative["transferKind"],
                representative["instructionTargetSymbol"],
                representative["effectiveTargetSymbol"],
            )
            for site in sites
        ):
            raise ValueError("map-event direct-control shared-PC transfer identity drift")
        physical_sites.append(
            {
                "romPc": rom_pc,
                "transferKind": representative["transferKind"],
                "sourceMnemonic": representative["sourceMnemonic"],
                "operandShape": representative["operandShape"],
                "instructionTargetSymbol": representative["instructionTargetSymbol"],
                "instructionTargetAddress": representative["instructionTargetAddress"],
                "effectiveTargetSymbol": representative["effectiveTargetSymbol"],
                "effectiveTargetAddress": representative["effectiveTargetAddress"],
                "contextCount": len(sites),
                "contextSiteOrders": [site["siteOrder"] for site in sites],
            }
        )

    for target in effective_targets.values():
        target["physicalTransferSiteCount"] = sum(
            item["effectiveTargetSymbol"] == target["symbol"]
            and item["effectiveTargetAddress"] == target["entryAddress"]
            for item in physical_sites
        )

    source_identity_roles: dict[str, set[str]] = {}
    for row in caller_sources.values():
        source_identity_roles.setdefault(row["sourcePath"], set()).add("caller-table")
    for row in effective_targets.values():
        source_identity_roles.setdefault(row["sourcePath"], set()).add("effective-target")
    for row in alias_joins.values():
        source_identity_roles.setdefault(row["sourcePath"], set()).add("alias-definition")
    owner_joins = [
        {
            "sourcePath": path,
            "sha256": hashlib.sha256((disasm / path).read_bytes()).hexdigest().upper(),
            "roles": sorted(roles),
        }
        for path, roles in sorted(source_identity_roles.items())
    ]
    continuation_counts = Counter(row["kind"] for row in call_continuations)
    expected_summary = {
        "programContextCount": 914,
        "positiveProgramContextCount": 154,
        "zeroProgramContextCount": 760,
        "contextTransferSiteCount": 205,
        "physicalTransferSiteCount": 201,
        "directCallContextSiteCount": 143,
        "directJumpContextSiteCount": 62,
        "callerTableSourceCount": 53,
        "effectiveTargetIdentityCount": 35,
        "aliasJoinCount": 15,
        "ownerSourceIdentityCount": 81,
        "h1RomAnchorCount": 251,
        "callContinuationCount": 143,
        "tailTransferCount": 62,
    }
    summary = {
        "programContextCount": len(program_contexts),
        "positiveProgramContextCount": sum(
            row["transferSiteCount"] > 0 for row in program_contexts
        ),
        "zeroProgramContextCount": sum(row["transferSiteCount"] == 0 for row in program_contexts),
        "contextTransferSiteCount": len(transfer_sites),
        "physicalTransferSiteCount": len(physical_sites),
        "directCallContextSiteCount": sum(
            row["transferKind"] == "direct-call" for row in transfer_sites
        ),
        "directJumpContextSiteCount": sum(
            row["transferKind"] == "direct-jump" for row in transfer_sites
        ),
        "callerTableSourceCount": len(caller_sources),
        "effectiveTargetIdentityCount": len(effective_targets),
        "aliasJoinCount": len(alias_joins),
        "ownerSourceIdentityCount": len(owner_joins),
        "h1RomAnchorCount": len(anchors),
        "callContinuationCount": len(call_continuations),
        "tailTransferCount": len(tail_transfers),
    }
    if summary != expected_summary:
        raise ValueError("map-event direct-control source/H1/ROM denominator drift")
    expected_continuations = {
        "ordinary": 72,
        "return": 57,
        "direct-call": 6,
        "unconditional-branch": 6,
        "conditional-branch": 1,
        "direct-jump": 1,
    }
    if dict(continuation_counts) != expected_continuations:
        raise ValueError("map-event direct-control call continuation denominator drift")
    expected_targets = {
        "BlacksmithMenu",
        "CaravanMenu",
        "ChangeEntityFacing",
        "CheckRandomBattle",
        "ChurchMenu",
        "ClosePortraitEyes",
        "ClosePortraitWindow",
        "DisplayCurrentPortrait",
        "DisplayTacticalBaseQuote",
        "DisplayText",
        "ExecuteMapScript",
        "GenerateRandomNumber",
        "GetCurrentHp",
        "GetEntityPortaitAndSpeechSfx",
        "GetItemInventoryLocation",
        "GetMaxHp",
        "GetMaxMp",
        "GetRhodeFacing",
        "MakeEntityWalk",
        "MoveEntityOutOfMap",
        "NameAlly",
        "PlayEndingCredits",
        "PlayIntroOrEndCutscene",
        "ReceiveMandatoryItem",
        "RemoveItemBySlot",
        "RemoveItemFromInventory",
        "SetCurrentHp",
        "SetCurrentMp",
        "ShopMenu",
        "Sleep",
        "WaitForEntityToStopMoving",
        "WaitForViewScrollEnd",
        "WitchEnd",
        "YesNoPrompt",
        "sub_5A278",
    }
    if set(symbol for symbol, _address in effective_targets) != expected_targets:
        raise ValueError("map-event direct-control effective target identity drift")
    return (
        {
            "sourceFiles": {key: caller_sources[key] for key in sorted(caller_sources)},
            "programContexts": program_contexts,
            "transferSites": transfer_sites,
            "physicalSites": physical_sites,
            "aliasJoins": list(alias_joins.values()),
            "effectiveTargets": list(effective_targets.values()),
            "callContinuations": call_continuations,
            "tailTransfers": tail_transfers,
            "ownerJoins": owner_joins,
        },
        summary,
        {
            "h1Listing": {
                "path": "build/sf2build-h1.lst",
                "sha256": hashlib.sha256(listing_text.encode("utf-8")).hexdigest().upper(),
            },
            "h1RomAnchors": list(anchors.values()),
        },
    )


def build_map_event_direct_control_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    """Build the direct-control contract from fresh retained source/H1/ROM inputs."""
    map_events, retained_map_events = _fresh_retained_map_events(rom_path, upstream_path)
    event_direct_control, summary, source_context = _direct_control_projection(
        map_events, upstream_path=upstream_path, rom_path=rom_path
    )
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": map_events["upstream"],
        "romSha256": map_events["romSha256"],
        "scope": _mother_corpus_projection(map_events),
        "sourceContext": source_context,
        "retainedMapEvents": retained_map_events,
        "eventDirectControl": event_direct_control,
        "unknowns": {key: "Unknown" for key in _UNKNOWN_KEYS},
        "summary": summary,
    }


def verify_map_event_direct_control_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner=str(FIXTURE))
    output = build_map_event_direct_control_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="map-event direct-control static contract")
    if fixture != output:
        raise ValueError("map-event direct-control complete semantic fixture drift")
    digest = hashlib.sha256(canonical_json_bytes(output)).hexdigest().upper()
    destination = output_path or repo_path("local/derived/map-event-direct-control-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(
        json.dumps(output, ensure_ascii=False, indent=2).encode("utf-8") + b"\n"
    )
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Transfers": output["summary"]["contextTransferSiteCount"],
        "Status": "PASS",
    }
