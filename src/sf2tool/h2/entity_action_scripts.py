from __future__ import annotations

import hashlib
import json
import re
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.rom import inspect_rom
from sf2tool.source_text import read_upstream_text

ID = "sf2-entity-action-scripts-static-v1"
MANIFEST = repo_path("manifests/extractions/entity-action-scripts-static.json")
SCHEMA = repo_path("schemas/entity-action-scripts-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/entity-action-scripts-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-entity-action-scripts-static-fixture.schema.json")

SOURCE_PATH = Path("data/scripting/entity/eas_actions.asm")
MACRO_PATH = Path("sf2cutscenemacros.asm")
START_SYMBOL = "eas_Jump"
END_ADDRESS = 0x46506


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _macro_contracts(disasm: Path) -> dict[str, dict[str, int]]:
    source = read_upstream_text(disasm / MACRO_PATH)
    contracts = {}
    pattern = re.compile(
        r"^(ac_[A-Za-z0-9_]+):\s*macro\s*\n(.*?)(?=^\s*endm\s*$)",
        re.MULTILINE | re.DOTALL,
    )
    for match in pattern.finditer(source):
        body = match.group(2)
        first_word = re.search(r"^\s*dc\.w\s+(\$?[0-9A-Fa-f]+)\b", body, re.MULTILINE)
        if first_word is None:
            raise ValueError(f"entity-action macro has no literal opcode: {match.group(1)}")
        token = first_word.group(1)
        opcode = int(token[1:], 16) if token.startswith("$") else int(token)
        size = (
            len(re.findall(r"^\s*dc\.b\b", body, re.MULTILINE))
            + 2 * len(re.findall(r"^\s*dc\.w\b", body, re.MULTILINE))
            + 4 * len(re.findall(r"^\s*dc\.l\b", body, re.MULTILINE))
        )
        contracts[match.group(1)] = {"opcode": opcode, "encodedBytes": size}
    if len(contracts) != 44:
        raise ValueError(f"entity-action macro boundary drift: {len(contracts)}")
    return contracts


def build_entity_action_script_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"entity-action H1 listing is missing: {listing_path}")
    addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))
    rom_identity = inspect_rom(rom_path)
    source = read_upstream_text(disasm / SOURCE_PATH)
    macro_contracts = _macro_contracts(disasm)

    labels = []
    commands = []
    active_label: str | None = None
    command_pattern = re.compile(
        r"^\s*(?:([A-Za-z_][A-Za-z0-9_]*):\s*)?(ac_[A-Za-z0-9_]+)\b(.*)$"
    )
    label_pattern = re.compile(r"^([A-Za-z_][A-Za-z0-9_]*):")
    for line_number, raw_line in enumerate(source.splitlines(), 1):
        code = raw_line.split(";", 1)[0].rstrip()
        label_match = label_pattern.match(code)
        if label_match:
            active_label = label_match.group(1)
            if active_label not in addresses:
                raise ValueError(f"entity-action label lacks H1 address: {active_label}")
            labels.append(
                {
                    "name": active_label,
                    "address": addresses[active_label],
                    "line": line_number,
                }
            )
        command_match = command_pattern.match(code)
        if not command_match:
            continue
        inline_label, macro, arguments = command_match.groups()
        if inline_label is not None:
            active_label = inline_label
        if macro not in macro_contracts:
            raise ValueError(f"undefined entity-action macro at line {line_number}: {macro}")
        commands.append(
            {
                "line": line_number,
                "ownerLabel": active_label,
                "macro": macro,
                "arguments": arguments.strip(),
                **macro_contracts[macro],
            }
        )

    direct_words = re.findall(r"^\s*dc\.w\s+(.+?)(?:\s*;.*)?$", source, re.MULTILINE)
    command_bytes = sum(row["encodedBytes"] for row in commands)
    direct_word_bytes = len(direct_words) * 2
    start_address = addresses[START_SYMBOL]
    byte_count = END_ADDRESS - start_address
    if command_bytes + direct_word_bytes != byte_count:
        raise ValueError(
            "entity-action source sizing drift: "
            f"commands={command_bytes}, directWords={direct_word_bytes}, range={byte_count}"
        )

    branch_targets = []
    lines = source.splitlines()
    for index, line in enumerate(lines):
        if re.search(r"\bac_branch\b", line.split(";", 1)[0]) is None:
            continue
        if index + 1 >= len(lines):
            raise ValueError("entity-action branch lacks displacement word")
        target_match = re.search(
            r"\((?:\s*)?([A-Za-z_][A-Za-z0-9_]*)-", lines[index + 1]
        ) or re.search(r"dc\.w\s+([A-Za-z_][A-Za-z0-9_]*)-", lines[index + 1])
        if target_match is None:
            raise ValueError(f"entity-action branch target drift at line {index + 1}")
        branch_targets.append(target_match.group(1))
    jump_targets = [
        row["arguments"] for row in commands if row["macro"] == "ac_jump"
    ]
    internal_labels = {row["name"] for row in labels}
    if any(target not in internal_labels for target in branch_targets):
        raise ValueError("entity-action relative branch escapes the source corpus")
    if set(jump_targets) - (internal_labels | {"eas_Idle"}):
        raise ValueError("entity-action absolute jump has an unknown target")

    rom_range = rom_path.read_bytes()[start_address:END_ADDRESS]
    command_counts = Counter(row["macro"] for row in commands)
    label_kinds = Counter(row["name"].split("_", 1)[0] for row in labels)
    summary = {
        "byteCount": byte_count,
        "labelCount": len(labels),
        "entryLabelCount": label_kinds["eas"],
        "internalByteLabelCount": label_kinds["byte"],
        "internalWordLabelCount": label_kinds["word"],
        "commandCount": len(commands),
        "uniqueCommandMacroCount": len(command_counts),
        "definedCommandMacroCount": len(macro_contracts),
        "unusedCommandMacroCount": len(set(macro_contracts) - set(command_counts)),
        "directDisplacementWordCount": len(direct_words),
        "absoluteJumpCount": command_counts["ac_jump"],
        "relativeBranchCount": command_counts["ac_branch"],
        "externalIdleJumpCount": jump_targets.count("eas_Idle"),
        "internalAbsoluteJumpCount": sum(target in internal_labels for target in jump_targets),
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "romSha256": rom_identity["sha256"],
        "table": {START_SYMBOL: start_address},
        "summary": summary,
        "romRange": {
            "sourcePath": SOURCE_PATH.as_posix(),
            "start": start_address,
            "endExclusive": END_ADDRESS,
            "sha256": hashlib.sha256(rom_range).hexdigest().upper(),
        },
        "commandCounts": dict(sorted(command_counts.items())),
        "controlFlowFacts": {
            "relativeBranchTargets": branch_targets,
            "absoluteJumpTargetCounts": dict(sorted(Counter(jump_targets).items())),
            "allRelativeBranchesStayInsideCorpus": True,
            "onlyExternalAbsoluteTarget": "eas_Idle",
        },
        "labels": labels,
        "commands": commands,
        "runtimeQuestions": [
            "What are the exact frame durations and collision effects of movement/action commands?",
            "Which story routes invoke each of the 39 entry labels?",
        ],
    }


def verify_entity_action_script_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_entity_action_script_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="entity-action script static contract")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != output["romSha256"]
    ):
        raise ValueError("entity-action script provenance drift")
    for field in (
        "table",
        "summary",
        "romRange",
        "commandCounts",
        "controlFlowFacts",
        "runtimeQuestions",
    ):
        if fixture[field] != output[field]:
            raise ValueError(f"entity-action script fixture drift: {field}")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if output["summary"] != manifest["summary"] or digest != manifest["outputSha256"]:
        raise ValueError("entity-action script canonical manifest drift")
    destination = output_path or repo_path("local/derived/entity-action-scripts-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Labels": output["summary"]["labelCount"],
        "Commands": output["summary"]["commandCount"],
        "Status": "PASS",
    }
