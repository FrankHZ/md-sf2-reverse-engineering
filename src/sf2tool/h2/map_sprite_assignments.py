from __future__ import annotations

import hashlib
import json
import re
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_global_data import _arguments, _tokens
from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.h2.enemy_map_sprites import build_enemy_map_sprites_contract
from sf2tool.h2.map_entities import build_map_entities_contract
from sf2tool.h3.growth import _parse_equates
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.rom import inspect_rom
from sf2tool.source_text import read_upstream_text

ID = "sf2-map-sprite-assignments-static-v1"
MANIFEST = repo_path("manifests/extractions/map-sprite-assignments-static.json")
SCHEMA = repo_path("schemas/map-sprite-assignments-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/map-sprite-assignments-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-map-sprite-assignments-static-fixture.schema.json")

ALLY_TABLE_PATH = Path("data/stats/allies/allymapsprites.asm")
WRITER_SYMBOLS = (
    "DeclareNewEntity",
    "esc17_setSpriteNumber",
    "UpdateEntityProperties",
    "csc1A_setEntitySprite",
    "GetAllyMapsprite",
    "table_AllyMapsprites",
)
SCRIPT_MACROS = ("setSprite", "newEntity", "ac_setSprite")


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _value(expression: str, equates: dict[str, int]) -> int:
    if expression in equates:
        return equates[expression]
    if expression.startswith("$"):
        return int(expression[1:], 16)
    return int(expression)


def _domain(value: int) -> str:
    if 237 <= value <= 239:
        return "regular-sentinel"
    if 240 <= value <= 250:
        return "unbacked-special"
    if value >= 251:
        return "routed-special"
    return "regular-backed"


def _script_assignments(disasm: Path, equates: dict[str, int]) -> list[dict[str, Any]]:
    pattern = re.compile(
        r"^\s*(?:[A-Za-z_][A-Za-z0-9_]*:\s*)?"
        r"(setSprite|newEntity|ac_setSprite)\s+(.+?)\s*$"
    )
    rows = []
    paths = sorted([*(disasm / "code").rglob("*.asm"), *(disasm / "data").rglob("*.asm")])
    for path in paths:
        for line_number, raw_line in enumerate(read_upstream_text(path).splitlines(), 1):
            match = pattern.match(raw_line.split(";", 1)[0])
            if not match:
                continue
            arguments = [argument.strip() for argument in match.group(2).split(",")]
            expression = arguments[-1]
            value = _value(expression, equates)
            rows.append(
                {
                    "path": path.relative_to(disasm).as_posix(),
                    "line": line_number,
                    "macro": match.group(1),
                    "expression": expression,
                    "value": value,
                    "domain": _domain(value),
                }
            )
    return rows


def _update_callers(disasm: Path) -> list[dict[str, Any]]:
    rows = []
    pattern = re.compile(r"(?:jsr|bsr).*UpdateEntityProperties")
    assignment = re.compile(
        r"\b(?:moveq?|clr)(?:\.[bwl])?\s+(.+?),d3\b", re.IGNORECASE
    )
    for path in sorted((disasm / "code").rglob("*.asm")):
        lines = read_upstream_text(path).splitlines()
        for index, line in enumerate(lines):
            if not pattern.search(line):
                continue
            window = [row.split(";", 1)[0] for row in lines[max(0, index - 30) : index]]
            candidates = [
                match.group(1).strip()
                for row in window
                if (match := assignment.search(row))
            ]
            if not candidates:
                raise ValueError(f"UpdateEntityProperties caller has no bounded d3 input: {path}")
            source = candidates[-1]
            joined = "\n".join(window)
            if source == "#-1":
                kind = "preserve-existing"
            elif source.startswith("#MAPSPRITE_"):
                kind = "literal-map-sprite"
            elif (
                source == "d4"
                and path.relative_to(disasm).as_posix()
                == "code/common/menus/memberscreen.asm"
                and index + 1 == 238
                and "#MAPSPRITE_CARAVAN" in joined
            ):
                kind = "ally-or-literal-vehicle"
            elif source == "d4" and "GetAllyMapsprite" in joined:
                kind = "ally-table-derived"
            elif source == "d4" and "#MAPSPRITE_CARAVAN" in joined:
                kind = "literal-vehicle-branch"
            else:
                raise ValueError(
                    f"unclassified UpdateEntityProperties d3 input at {path}:{index + 1}: {source}"
                )
            rows.append(
                {
                    "path": path.relative_to(disasm).as_posix(),
                    "line": index + 1,
                    "inputKind": kind,
                    "lastD3Source": source,
                }
            )
    return rows


def _writer_sites(disasm: Path) -> list[dict[str, Any]]:
    expected = (
        (
            "code/common/scripting/entity/entityfunctions_1.asm",
            "DeclareNewEntity",
            "move.b  d4,ENTITYDEF_OFFSET_MAPSPRITE(a0)",
        ),
        (
            "code/common/scripting/entity/entityscriptengine_2.asm",
            "esc17_setSpriteNumber",
            "move.b  3(a1),ENTITYDEF_OFFSET_MAPSPRITE(a0)",
        ),
        (
            "code/common/scripting/entity/entityscriptengine_2.asm",
            "UpdateEntityProperties",
            "move.b  d3,ENTITYDEF_OFFSET_MAPSPRITE(a0)",
        ),
        (
            "code/common/scripting/map/mapscriptengine_1.asm",
            "csc1A_setEntitySprite",
            "move.b  d0,ENTITYDEF_OFFSET_MAPSPRITE(a5)",
        ),
        (
            "code/common/scripting/map/followersfunctions_2.asm",
            "direct-player-raft-write",
            "move.b  #MAPSPRITE_RAFT,((ENTITY_MAPSPRITE-$1000000)).w",
        ),
    )
    rows = []
    for relative_path, owner, fragment in expected:
        source = read_upstream_text(disasm / relative_path)
        if source.count(fragment) != 1:
            raise ValueError(f"map-sprite writer-site drift: {relative_path}:{owner}")
        rows.append({"path": relative_path, "owner": owner, "instruction": fragment})

    code = "\n".join(read_upstream_text(path) for path in sorted((disasm / "code").rglob("*.asm")))
    offset_writes = re.findall(
        r"^\s*move\.b\s+[^;\r\n]+,ENTITYDEF_OFFSET_MAPSPRITE\([^)]+\)",
        code,
        re.MULTILINE,
    )
    if len(offset_writes) != 4:
        raise ValueError(f"entity map-sprite offset-writer count drift: {len(offset_writes)}")
    if code.count("ENTITY_MAPSPRITE-$1000000)).w") != 1:
        raise ValueError("direct player map-sprite writer count drift")
    return rows


def build_map_sprite_assignment_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"map-sprite assignment H1 listing is missing: {listing_path}")
    addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))
    equates = _parse_equates(disasm)
    rom_identity = inspect_rom(rom_path)

    entities = build_map_entities_contract(rom_path, upstream_path)
    enemies = build_enemy_map_sprites_contract(rom_path, upstream_path)
    if entities["upstream"]["commit"] != commit or enemies["upstream"]["commit"] != commit:
        raise ValueError("map-sprite assignment dependency provenance drift")

    ally_source = read_upstream_text(disasm / ALLY_TABLE_PATH)
    ally_codes = [
        token
        for expression in _arguments(ally_source, "mapsprite")
        for token in _tokens(expression)
    ]
    ally_values = [equates[f"MAPSPRITE_{code}"] for code in ally_codes]
    if len(ally_values) != 30:
        raise ValueError(f"ally map-sprite row count drift: {len(ally_values)}")
    ally_table_address = addresses["table_AllyMapsprites"]
    ally_bytes = bytes(ally_values)
    if rom_path.read_bytes()[ally_table_address : ally_table_address + 30] != ally_bytes:
        raise ValueError("ally map-sprite table ROM parity drift")

    assignments = _script_assignments(disasm, equates)
    callers = _update_callers(disasm)
    writers = _writer_sites(disasm)
    macro_counts = Counter(row["macro"] for row in assignments)
    domain_counts = Counter(row["domain"] for row in assignments)
    caller_counts = Counter(row["inputKind"] for row in callers)
    reserved = sorted(
        {
            row["value"]
            for row in assignments
            if row["domain"] in {"regular-sentinel", "unbacked-special"}
        }
    )
    summary = {
        "writerSiteCount": len(writers),
        "scriptAssignmentCount": len(assignments),
        "scriptAssignmentUniqueValueCount": len({row["value"] for row in assignments}),
        "setSpriteCount": macro_counts["setSprite"],
        "newEntityCount": macro_counts["newEntity"],
        "actionSetSpriteCount": macro_counts["ac_setSprite"],
        "scriptRegularBackedCount": domain_counts["regular-backed"],
        "scriptRoutedSpecialCount": domain_counts["routed-special"],
        "scriptReservedCount": len(reserved),
        "updateEntityPropertiesCallerCount": len(callers),
        "updatePreserveCallerCount": caller_counts["preserve-existing"],
        "updateAllyDerivedCallerCount": caller_counts["ally-table-derived"],
        "updateAllyOrVehicleCallerCount": caller_counts["ally-or-literal-vehicle"],
        "updateLiteralCallerCount": (
            caller_counts["literal-map-sprite"] + caller_counts["literal-vehicle-branch"]
        ),
        "allyTableRowCount": len(ally_values),
        "enemyTableRowCount": enemies["summary"]["tableRowCount"],
        "initialEntityRecordCount": entities["summary"]["sourcePhysicalRecordCount"],
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "romSha256": rom_identity["sha256"],
        "table": {symbol: addresses[symbol] for symbol in WRITER_SYMBOLS},
        "summary": summary,
        "writerSites": writers,
        "scriptAssignmentFacts": {
            "macroCounts": dict(sorted(macro_counts.items())),
            "domainCounts": dict(sorted(domain_counts.items())),
            "reservedIdsPresent": reserved,
            "highestRegularValue": max(row["value"] for row in assignments if row["value"] < 240),
            "routedSpecialValues": sorted(
                {row["value"] for row in assignments if row["value"] >= 251}
            ),
        },
        "derivedDomainFacts": {
            "initialEntityReservedIdsPresent": sorted(
                entities["mapSpriteFacts"]["sentinelRegularIdsPresent"]
                + entities["mapSpriteFacts"]["unbackedSpecialIdsPresent"]
            ),
            "allyTableValueRange": {"minimum": min(ally_values), "maximum": max(ally_values)},
            "enemyTableValueRange": {
                "minimum": min(row["mapSprite"]["value"] for row in enemies["definitionRows"]),
                "maximum": max(row["mapSprite"]["value"] for row in enemies["tailRows"]),
            },
            "allyDerivationOnlySubtractsOrUsesNamedBlueFlameNpcFallbacks": True,
            "originalBuiltDomainsContainReservedIds": False,
        },
        "updateCallerFacts": {
            "inputKindCounts": dict(sorted(caller_counts.items())),
            "allCallersClassified": True,
        },
        "scriptAssignments": assignments,
        "updateCallers": callers,
        "runtimeQuestions": [
            "Can malformed scripts, raw RAM edits, or corrupt combatant state inject IDs 237-250?",
            "What visible failure modes result when each sentinel or unbacked ID reaches its "
            "loader?",
        ],
    }


def verify_map_sprite_assignment_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_map_sprite_assignment_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="map-sprite assignment static contract")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != output["romSha256"]
    ):
        raise ValueError("map-sprite assignment provenance drift")
    for field in (
        "table",
        "summary",
        "writerSites",
        "scriptAssignmentFacts",
        "derivedDomainFacts",
        "updateCallerFacts",
        "runtimeQuestions",
    ):
        if fixture[field] != output[field]:
            raise ValueError(f"map-sprite assignment fixture drift: {field}")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if output["summary"] != manifest["summary"] or digest != manifest["outputSha256"]:
        raise ValueError("map-sprite assignment canonical manifest drift")
    destination = output_path or repo_path("local/derived/map-sprite-assignments-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Writers": output["summary"]["writerSiteCount"],
        "ScriptAssignments": output["summary"]["scriptAssignmentCount"],
        "ReservedIds": output["summary"]["scriptReservedCount"],
        "Status": "PASS",
    }
