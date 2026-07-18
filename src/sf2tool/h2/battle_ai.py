from __future__ import annotations

import hashlib
import json
import re
import subprocess
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path

ID = "sf2-battle-ai-static-v1"
SOURCE_ROOT = Path("code/gameflow/battle/ai")
MANIFEST = repo_path("manifests/extractions/battle-ai-static.json")
SCHEMA = repo_path("schemas/battle-ai-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/battle-ai-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-battle-ai-static-fixture.schema.json")
TOOLCHAIN = repo_path("manifests/toolchain.json")
RESEARCH_INDEX = repo_path("manifests/research-index.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

GLOBAL_LABEL_PATTERN = re.compile(r"^([A-Za-z_][A-Za-z0-9_]*):")
LOCAL_LABEL_PATTERN = re.compile(r"^\s*(@[A-Za-z0-9_]+):")
CALL_PATTERN = re.compile(r"\b(?:bsr|jsr)(?:\.[bwl])?\s+([^\s,;]+)", re.IGNORECASE)
DIRECT_TARGET_PATTERN = re.compile(r"^[A-Za-z_][A-Za-z0-9_]*$")
EQUATE_PATTERN = re.compile(
    r"^([A-Z][A-Z0-9_]+):\s+equ\s+(\$[0-9A-Fa-f]+|-?\d+)", re.MULTILINE
)
SPELL_COMPARE_PATTERN = re.compile(r"cmpi\.b\s+#(SPELL_[A-Z0-9_]+),d5")
REGISTER_TARGETS = {f"a{index}" for index in range(8)} | {
    f"d{index}" for index in range(8)
} | {"sp", "pc"}


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _strip_comment(line: str) -> str:
    return line.split(";", 1)[0].rstrip()


def _direct_target(operand: str) -> str | None:
    operand = re.sub(r"\.[bwl]$", "", operand, flags=re.IGNORECASE)
    if operand.startswith("(") and operand.endswith(")"):
        operand = operand[1:-1]
    if not DIRECT_TARGET_PATTERN.fullmatch(operand) or operand.lower() in REGISTER_TARGETS:
        return None
    return operand


def _parse_source_file(path: Path, relative_path: str) -> dict[str, Any]:
    raw = path.read_bytes()
    text = raw.decode("utf-8")
    lines = text.splitlines()
    global_labels: list[str] = []
    local_label_count = 0
    statement_count = 0
    direct_calls: Counter[str] = Counter()
    indirect_call_count = 0

    for raw_line in lines:
        line = _strip_comment(raw_line)
        if not line.strip():
            continue
        global_match = GLOBAL_LABEL_PATTERN.match(line)
        local_match = LOCAL_LABEL_PATTERN.match(line)
        if global_match:
            global_labels.append(global_match.group(1))
            line = line[global_match.end() :]
        elif local_match:
            local_label_count += 1
            line = line[local_match.end() :]
        if not line.strip():
            continue
        statement_count += 1
        call_match = CALL_PATTERN.search(line)
        if not call_match:
            continue
        target = _direct_target(call_match.group(1))
        if target is None:
            indirect_call_count += 1
        else:
            direct_calls[target] += 1

    return {
        "path": relative_path,
        "sha256": hashlib.sha256(raw).hexdigest().upper(),
        "sourceLineCount": len(lines),
        "statementCount": statement_count,
        "globalLabels": global_labels,
        "localLabelCount": local_label_count,
        "directCalls": [
            {"target": target, "siteCount": count}
            for target, count in sorted(direct_calls.items())
        ],
        "indirectCallSiteCount": indirect_call_count,
    }


def _integer(expression: str) -> int:
    if expression.startswith("$"):
        return int(expression[1:], 16)
    return int(expression, 10)


def _equates(disasm: Path) -> dict[str, int]:
    text = (disasm / "sf2enums.asm").read_text(encoding="utf-8")
    return {name: _integer(expression) for name, expression in EQUATE_PATTERN.findall(text)}


def _function_block(source: str, name: str) -> str:
    match = re.search(
        rf"^{re.escape(name)}:\s*$" rf"(.*?)" rf"^\s*; End of function {re.escape(name)}\s*$",
        source,
        re.MULTILINE | re.DOTALL,
    )
    if not match:
        raise ValueError(f"battle AI action-filter function is missing: {name}")
    return match.group(1)


def _named_values(names: list[str], equates: dict[str, int]) -> list[dict[str, int | str]]:
    missing = [name for name in names if name not in equates]
    if missing:
        raise ValueError(f"battle AI action-filter equates are missing: {missing}")
    return [{"name": name, "value": equates[name]} for name in names]


def _parse_action_filters(disasm: Path) -> dict[str, Any]:
    source = (disasm / SOURCE_ROOT / "getnextuseableaiaction.asm").read_text(encoding="utf-8")
    equates = _equates(disasm)
    attack_spell = _function_block(source, "GetNextUsableAttackSpell")
    healing_spell = _function_block(source, "GetNextHealingSpell")
    support_spell = _function_block(source, "GetNextSupportSpell")
    attack_item = _function_block(source, "GetNextUsableAttackItem")
    healing_item = _function_block(source, "GetNextUsableHealingItem")

    attack_spell_allowlist = SPELL_COMPARE_PATTERN.findall(attack_spell)
    attack_item_allowlist = SPELL_COMPARE_PATTERN.findall(attack_item)
    required_fragments = {
        "attack spell": (
            (attack_spell, "bsr.w   IsCombatantConfused"),
            (attack_spell, "move.w  #1,d7"),
            (attack_spell, "bsr.w   GetHighestUsableSpellLevel"),
            (attack_spell, "bra.w   @Next"),
        ),
        "healing spell": (
            (healing_spell, "cmpi.b  #SPELLPROPS_TYPE_HEAL,d2"),
            (healing_spell, "move.w  #SPELL_NOTHING,d1"),
        ),
        "support spell": (
            (support_spell, "cmpi.b  #SPELLPROPS_TYPE_SUPPORT,d2"),
            (support_spell, "move.w  #SPELL_NOTHING,d1"),
        ),
        "attack item": (
            (attack_item, "move.w  #1,d6"),
            (attack_item, "btst    #ITEMENTRY_BIT_EQUIPPED,d1"),
            (attack_item, "btst    #ITEMENTRY_BIT_USABLE_BY_AI,d1"),
            (attack_item, "bra.w   @Nothing"),
        ),
        "healing item": (
            (healing_item, "cmpi.b  #ITEM_HEALING_RAIN,d7"),
            (healing_item, "btst    #ITEMENTRY_BIT_USABLE_BY_AI,d1"),
            (healing_item, "beq.w   @Next"),
        ),
    }
    for owner, checks in required_fragments.items():
        if any(fragment not in block for block, fragment in checks):
            raise ValueError(f"battle AI {owner} source contract drift")

    if len(attack_spell_allowlist) != 6 or len(attack_item_allowlist) != 4:
        raise ValueError("battle AI confused-action allowlist shape drift")

    return {
        "slotCounts": {
            "spells": equates["COMBATANT_SPELLSLOTS"],
            "items": equates["COMBATANT_ITEMSLOTS"],
        },
        "sentinels": {
            "spellNothing": equates["SPELL_NOTHING"],
            "itemNothing": equates["ITEM_NOTHING"],
        },
        "spellTypes": {
            "attack": equates["SPELLPROPS_TYPE_ATTACK"],
            "heal": equates["SPELLPROPS_TYPE_HEAL"],
            "support": equates["SPELLPROPS_TYPE_SUPPORT"],
        },
        "attackSpell": {
            "function": "GetNextUsableAttackSpell",
            "alliesAlwaysUseConfusedFilter": True,
            "confusedEnemyUsesFilter": True,
            "confusedAllowlist": _named_values(attack_spell_allowlist, equates),
            "requiredSpellType": "attack",
            "rejectedCandidatePolicy": "continue-to-next-slot",
            "returnsHighestUsableLevel": True,
        },
        "healingSpell": {
            "function": "GetNextHealingSpell",
            "requiredSpellType": "heal",
            "rejectedCandidatePolicy": "continue-to-next-slot",
            "confusionFilter": False,
        },
        "supportSpell": {
            "function": "GetNextSupportSpell",
            "requiredSpellType": "support",
            "rejectedCandidatePolicy": "continue-to-next-slot",
            "confusionFilter": False,
        },
        "attackItem": {
            "function": "GetNextUsableAttackItem",
            "alliesAlwaysUseConfusedFilter": True,
            "confusedEnemyUsesFilter": True,
            "confusedAllowlist": _named_values(attack_item_allowlist, equates),
            "requiredSpellType": "attack",
            "equippedItemBypassesAiUseFlag": True,
            "unequippedItemRequiresAiUseFlag": True,
            "unusableCandidatePolicy": "continue-to-next-slot",
            "rejectedCandidatePolicy": "stop-with-item-nothing",
        },
        "healingItem": {
            "function": "GetNextUsableHealingItem",
            "requiredSpellType": "heal",
            "healingRainItem": {
                "name": "ITEM_HEALING_RAIN",
                "value": equates["ITEM_HEALING_RAIN"],
            },
            "healingRainBypassesAiUseFlag": True,
            "otherItemsRequireAiUseFlag": True,
            "rejectedCandidatePolicy": "continue-to-next-slot",
            "confusionFilter": False,
        },
    }


def _resolve_upstream(upstream_path: Path) -> tuple[Path, str, dict[str, Any]]:
    upstream_path = upstream_path.resolve(strict=True)
    toolchain = load_json(TOOLCHAIN)
    commit = subprocess.run(
        ["git", "rev-parse", "HEAD"],
        cwd=upstream_path,
        check=True,
        capture_output=True,
        text=True,
        encoding="utf-8",
    ).stdout.strip()
    if commit != toolchain["sf2disasm"]["commit"]:
        raise ValueError(
            f"battle AI inventory requires SF2DISASM {toolchain['sf2disasm']['commit']}, "
            f"got {commit}"
        )
    disasm = upstream_path / "disasm"
    source_root = disasm / SOURCE_ROOT
    if not source_root.is_dir():
        raise ValueError(f"battle AI source root is missing: {source_root}")
    return disasm, commit, toolchain


def build_battle_ai_inventory(upstream_path: Path) -> dict[str, Any]:
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    source_root = disasm / SOURCE_ROOT
    source_paths = sorted(source_root.rglob("*.asm"), key=lambda path: path.as_posix())
    if not source_paths:
        raise ValueError("battle AI source inventory is empty")

    files = [
        _parse_source_file(path, path.relative_to(disasm).as_posix()) for path in source_paths
    ]
    all_labels = {label for file in files for label in file["globalLabels"]}
    direct_calls: Counter[str] = Counter()
    for file in files:
        for call in file["directCalls"]:
            direct_calls[call["target"]] += call["siteCount"]
    internal_targets = sorted(target for target in direct_calls if target in all_labels)
    external_targets = sorted(target for target in direct_calls if target not in all_labels)

    research_index = load_json(RESEARCH_INDEX)
    indexed_records = sorted(
        record["id"]
        for record in research_index["records"]
        if Path(record["sourcePath"]).is_relative_to(SOURCE_ROOT)
    )
    indexed_files = sorted(
        {
            record["sourcePath"]
            for record in research_index["records"]
            if Path(record["sourcePath"]).is_relative_to(SOURCE_ROOT)
        }
    )
    summary = {
        "fileCount": len(files),
        "sourceLineCount": sum(file["sourceLineCount"] for file in files),
        "statementCount": sum(file["statementCount"] for file in files),
        "globalLabelCount": sum(len(file["globalLabels"]) for file in files),
        "localLabelCount": sum(file["localLabelCount"] for file in files),
        "directCallSiteCount": sum(direct_calls.values()),
        "indirectCallSiteCount": sum(file["indirectCallSiteCount"] for file in files),
        "uniqueDirectTargetCount": len(direct_calls),
        "internalDirectTargetCount": len(internal_targets),
        "externalDirectTargetCount": len(external_targets),
        "indexedRecordCount": len(indexed_records),
        "indexedFileCount": len(indexed_files),
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {
            "repository": toolchain["sf2disasm"]["repository"],
            "commit": commit,
        },
        "scope": SOURCE_ROOT.as_posix(),
        "summary": summary,
        "actionFilters": _parse_action_filters(disasm),
        "indexedRecordIds": indexed_records,
        "indexedSourcePaths": indexed_files,
        "internalDirectCallTargets": internal_targets,
        "externalDirectCallTargets": external_targets,
        "files": files,
    }


def _action_filter_facts(action_filters: dict[str, Any]) -> dict[str, Any]:
    return {
        "attackSpellAllowlist": [
            entry["value"] for entry in action_filters["attackSpell"]["confusedAllowlist"]
        ],
        "attackItemAllowlist": [
            entry["value"] for entry in action_filters["attackItem"]["confusedAllowlist"]
        ],
        "attackSpellRejectedCandidatePolicy": action_filters["attackSpell"][
            "rejectedCandidatePolicy"
        ],
        "attackItemRejectedCandidatePolicy": action_filters["attackItem"][
            "rejectedCandidatePolicy"
        ],
        "attackItemUnusableCandidatePolicy": action_filters["attackItem"][
            "unusableCandidatePolicy"
        ],
        "allyAttackSpellUsesConfusedFilter": action_filters["attackSpell"][
            "alliesAlwaysUseConfusedFilter"
        ],
        "allyAttackItemUsesConfusedFilter": action_filters["attackItem"][
            "alliesAlwaysUseConfusedFilter"
        ],
        "equippedAttackItemBypassesAiUseFlag": action_filters["attackItem"][
            "equippedItemBypassesAiUseFlag"
        ],
        "healingRainItem": action_filters["healingItem"]["healingRainItem"]["value"],
        "healingRainBypassesAiUseFlag": action_filters["healingItem"][
            "healingRainBypassesAiUseFlag"
        ],
        "healingSpellType": action_filters["healingSpell"]["requiredSpellType"],
        "supportSpellType": action_filters["supportSpell"]["requiredSpellType"],
    }


def verify_battle_ai_inventory(
    upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    manifest = load_json(MANIFEST)
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    rom_manifest = load_json(ROM_MANIFEST)
    output = build_battle_ai_inventory(upstream_path)
    validate_json(output, SCHEMA, owner="battle AI static inventory")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != rom_manifest["hashes"]["sha256"]
    ):
        raise ValueError("battle AI static fixture provenance drift")
    if _action_filter_facts(output["actionFilters"]) != fixture["expected"]:
        raise ValueError("battle AI action-filter facts disagree with fixture")
    if output["summary"] != manifest["summary"]:
        raise ValueError(
            "battle AI static summary drift: "
            f"expected {manifest['summary']}, got {output['summary']}"
        )
    encoded = _canonical_bytes(output)
    digest = hashlib.sha256(encoded).hexdigest().upper()
    if manifest["outputSha256"] != "PENDING" and digest != manifest["outputSha256"]:
        raise ValueError(
            "battle AI static inventory hash mismatch: "
            f"expected {manifest['outputSha256']}, got {digest}"
        )
    destination = output_path or repo_path(manifest["outputPath"])
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(encoded)
    return {
        "Inventory": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Files": output["summary"]["fileCount"],
        "GlobalLabels": output["summary"]["globalLabelCount"],
        "DirectCallSites": output["summary"]["directCallSiteCount"],
        "IndexedRecords": output["summary"]["indexedRecordCount"],
        "Status": "PASS",
    }
